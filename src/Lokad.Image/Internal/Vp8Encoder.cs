using System.Buffers.Binary;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Lokad.Image.Internal;

/// <summary>Owned VP8 key-frame encoder with independent intra search and admitted entropy adaptation.</summary>
internal static partial class Vp8Encoder
{
    private const int RateLumaModes = 10;
    // RFC 6386 sections 11 and 13: leaves are represented here as -(token+1).
    private static ReadOnlySpan<sbyte> ModeTree => [-1, 2, -2, 4, -3, 6, 8, 12, -4, 10, -6, -7, -5, 14, -8, 16, -9, -10];
    private static ReadOnlySpan<sbyte> TokenTree => [-12, 2, -1, 4, -2, 6, 8, 12, -3, 10, -4, -5, 14, 16, -6, -7, 18, 20, -8, -9, -10, -11];
    private static ReadOnlySpan<byte> Bands => [0, 1, 2, 3, 6, 4, 5, 6, 6, 6, 6, 6, 6, 6, 6, 7];
    private static ReadOnlySpan<byte> Zigzag => [0, 1, 4, 8, 5, 2, 3, 6, 9, 12, 13, 10, 7, 11, 14, 15];
    private static readonly ulong[] ModePaths = Vp8BooleanWriter.CompileTree(ModeTree, 10);
    private static readonly ulong[] TokenPaths = Vp8BooleanWriter.CompileTree(TokenTree, 12);

    public static void Encode(ImageBuffer image, Stream output, WebpEncodeOptions options, OperationContext context)
    {
        if (options.Quality is < 1 or > 100 || options.Effort is < 0 or > 6) throw new ArgumentOutOfRangeException(nameof(options));
        context.PixelBytes(image.Width, image.Height, image.BitDepth);
        if (image.Width > 16383 || image.Height > 16383) throw new ImageLimitException(ImageLimitKind.Dimension, "VP8 dimensions exceed 16383.");
        context.ReserveBorrowed(image.ByteLength); context.Reserve(4096);
        var source = Convert(image, context, out var alpha);
        var columns = source.Stride / 16; var rows = source.Y.Length / source.Stride / 16; var count = columns * rows;
        context.Reserve(source.MemoryBytes);
        var reconstructed = new Vp8Planes(image.Width, image.Height, source.Stride,
            new byte[source.Y.Length], new byte[source.U.Length], new byte[source.V.Length]);
        var metadataBytes = count * 17L + columns * 12L + 128;
        context.Reserve(metadataBytes);
        var modes = new byte[count * 16]; var chromaModes = new byte[count]; var above = new byte[columns * 8];
        var q = ((100 - options.Quality) * 127 + 49) / 99;
        using var quantization = QuantizationPlan.TryCreate(image, source, q, options.Quality, options.Effort, context);
        using var tokens = new Vp8BooleanWriter(context, options.Limits.MaxOutputBytes);
        Span<byte> left = stackalloc byte[8];
        ReadOnlySpan<byte> candidates = [1, 2, 3, 4, 6, 8, 10];
        using var model = CoefficientModel.TryCreate(source, count, candidates[options.Effort], options.Effort == 0 ? 1 : 4, context,
            options.Effort >= 4 && (q != 0 || quantization is not null) ? modes : null, fullFrame: options.Effort >= 4 && q == 0,
            segmented: quantization is not null, refinementWork: q == 0 && quantization is not null
                ? count * (24L * (RefinementWork + 512) + 512) : 0);
        var intra16 = model?.Intra16 == true;
        Span<short> luma = intra16 ? stackalloc short[256] : default;
        Span<short> y2 = intra16 ? stackalloc short[16] : default;
        Span<short> chromaCoefficients = intra16 ? stackalloc short[128] : default;
        Span<int> bitCosts = intra16 ? stackalloc int[257] : default;
        var lambda = 0;
        if (intra16)
        {
            for (var p = 0; p <= 256; p++) bitCosts[p] = (int)Math.Round(-Math.Log2(Math.Max(1, p) / 256d) * 256, MidpointRounding.AwayFromZero);
            var ac = (int)Vp8Tables.AcQuantizers[q];
            lambda = (int)Math.Round(ac * ac * Math.Log(2) * 256 / 24, MidpointRounding.AwayFromZero);
        }
        for (var my = 0; my < rows; my++)
        {
            left.Clear(); byte leftY2 = 0;
            for (var mx = 0; mx < columns; mx++)
            {
                var index = my * columns + mx; var x = mx * 16; var y = my * 16;
                var blockQ = quantization is null ? q : quantization.Quantizers[quantization.Map[index]];
                var blockLambda = lambda;
                if (intra16 && blockQ != q)
                {
                    context.Work(1); var ac = (int)Vp8Tables.AcQuantizers[blockQ];
                    blockLambda = (int)Math.Round(ac * ac * Math.Log(2) * 256 / 24, MidpointRounding.AwayFromZero);
                }
                if (intra16)
                {
                    var top = above.AsSpan(mx * 8, 8);
                    var large = PrepareLumaMacroblock(source, reconstructed, x, y, blockQ, RateLumaModes,
                        index, columns, modes, top, left, model!.Y2Above[mx] + leftY2,
                        model.Probabilities, bitCosts, blockLambda, luma, y2, context, model.Refine);
                    if (large)
                    {
                        var predictor = model.Y2Above[mx] + leftY2;
                        model.Y2Above[mx] = leftY2 = model.Ready ? Coefficients(tokens, y2, 1, predictor, model.Probabilities)
                            : model.RecordY2(source, index, x, y, y2, predictor);
                    }
                    for (var b = 0; b < 16; b++)
                    {
                        var a = b % 4; var l = b / 4; var values = luma.Slice(b * 16, 16); var plane = large ? 0 : 3;
                        top[a] = left[l] = model.Ready ? Coefficients(tokens, values, plane, top[a] + left[l], model.Probabilities)
                            : model.Record(source.Y, source.Stride, x + a * 4, y + l * 4, 0,
                                index * 24 + b, values, plane, top[a] + left[l]);
                    }
                }
                else
                    for (var b = 0; b < 16; b++)
                    {
                        var bx = x + b % 4 * 4; var by = y + b / 4 * 4;
                        var mode = SelectLuma(source, reconstructed, bx, by, x, y, candidates[options.Effort], context);
                        modes[index * 16 + b] = (byte)mode;
                        Block(source.Y, reconstructed.Y, source.Stride, bx, by, 3, blockQ,
                            tokens, above.AsSpan(mx * 8, 8), left, b % 4, b / 4, context, model, index * 24 + b, 0);
                    }
                var chromaMode = intra16
                    ? SelectRateChroma(source, reconstructed, x / 2, y / 2, blockQ, above.AsSpan(mx * 8, 8), left,
                        model!.Probabilities, bitCosts, blockLambda, chromaCoefficients, context, model.Refine)
                    : SelectChroma(source, reconstructed, x / 2, y / 2, options.Effort == 0 ? 1 : 4, context);
                chromaModes[index] = (byte)chromaMode;
                for (var plane = 0; plane < 2; plane++)
                {
                    var src = plane == 0 ? source.U : source.V; var dst = plane == 0 ? reconstructed.U : reconstructed.V;
                    for (var b = 0; b < 4; b++)
                        if (intra16)
                        {
                            var top = above.AsSpan(mx * 8, 8); var a = 4 + plane * 2 + b % 2; var l = 4 + plane * 2 + b / 2;
                            var values = chromaCoefficients.Slice((plane * 4 + b) * 16, 16);
                            top[a] = left[l] = model!.Ready ? Coefficients(tokens, values, 2, top[a] + left[l], model.Probabilities)
                                : model.Record(src, source.Stride / 2, x / 2 + b % 2 * 4, y / 2 + b / 2 * 4,
                                    source.Y.Length + plane * source.U.Length, index * 24 + 16 + plane * 4 + b,
                                    values, 2, top[a] + left[l]);
                        }
                        else Block(src, dst, source.Stride / 2, x / 2 + b % 2 * 4, y / 2 + b / 2 * 4, 2, blockQ,
                            tokens, above.AsSpan(mx * 8, 8), left, 4 + plane * 2 + b % 2, 4 + plane * 2 + b / 2, context,
                            model, index * 24 + 16 + plane * 4 + b, source.Y.Length + plane * source.U.Length);
                }
                if (model is not null && !model.Ready
                    && (index + 1 == model.PrefixMacroblocks || index + 1 == model.CacheMacroblocks))
                    model.CompleteMacroblock(source, tokens, columns, rows, index + 1);
            }
        }
        tokens.Finish();
        context.Release(source.MemoryBytes + reconstructed.MemoryBytes);
        using var header = BuildHeader(q, modes, chromaModes, columns, rows,
            model is null ? Vp8Tables.CoefficientProbabilities : model.Probabilities, context, quantization);
        using var compressedAlpha = alpha is null ? null
            : WebpLosslessEncoder.AlphaEncoding.TryCreate(alpha, image.Width, options.Effort, context,
                pendingOutputWork: (long)header.Bytes.Length + tokens.Bytes.Length);
        WriteContainer(image.Width, image.Height, alpha, compressedAlpha, header.Bytes, tokens.Bytes, output, context);
        context.Release(metadataBytes + 4096);
    }

    private static Vp8Planes Convert(ImageBuffer image, OperationContext context, out byte[]? alpha)
    {
        var stride = (image.Width + 15) / 16 * 16; var height = (image.Height + 15) / 16 * 16;
        var length = checked(stride * height); context.Reserve(length * 3L / 2 + 128);
        var result = new Vp8Planes(image.Width, image.Height, stride, new byte[length], new byte[length / 4], new byte[length / 4]);
        // The first logical row charge still precedes access to source storage.
        context.Work(image.Width * 4L);
        ReadOnlySpan<byte> pixels = image.Pixels;
        var words = MemoryMarshal.Cast<byte, ushort>(pixels);
        var byteDepth = image.BitDepth == 8; var width = image.Width; var sourceHeight = image.Height;
        var sourceStride = width * 4;
        var transparent = false;
        for (var y = 0; y < image.Height; y++)
        {
            if (y != 0) context.Work(image.Width * 4L);
            for (var x = 0; x < width; x++) transparent |= Sample(pixels, words, byteDepth, y * sourceStride + x * 4 + 3) != 255;
        }
        alpha = null;
        if (transparent)
        {
            context.Reserve((long)image.Width * image.Height + 32);
            alpha = new byte[image.Width * image.Height];
            for (var y = 0; y < image.Height; y++)
            {
                context.Work(image.Width * 4L);
                for (var x = 0; x < width; x++) alpha[y * width + x] = (byte)Sample(pixels, words, byteDepth, y * sourceStride + x * 4 + 3);
            }
        }
        for (var y = 0; y < height; y++)
        {
            context.Work(stride * 32L);
            var sourceRow = Math.Min(y, sourceHeight - 1) * sourceStride;
            for (var x = 0; x < stride; x++)
            {
                var index = sourceRow + Math.Min(x, width - 1) * 4;
                var r = Sample(pixels, words, byteDepth, index);
                var g = Sample(pixels, words, byteDepth, index + 1);
                var b = Sample(pixels, words, byteDepth, index + 2);
                result.Y[y * stride + x] = Round(16 + 219.0 / 255 * (.299 * r + .587 * g + .114 * b));
            }
        }
        for (var y = 0; y < height / 2; y++)
        {
            context.Work(stride * 64L);
            for (var x = 0; x < stride / 2; x++)
            {
                double r = 0, g = 0, b = 0; int alphaSum = 0, redSum = 0, greenSum = 0, blueSum = 0;
                for (var dy = 0; dy < 2; dy++)
                    for (var dx = 0; dx < 2; dx++)
                    {
                        var sx = Math.Min(x * 2 + dx, width - 1); var sy = Math.Min(y * 2 + dy, sourceHeight - 1);
                        var index = sy * sourceStride + sx * 4;
                        var red = Sample(pixels, words, byteDepth, index);
                        var green = Sample(pixels, words, byteDepth, index + 1);
                        var blue = Sample(pixels, words, byteDepth, index + 2);
                        var a = Sample(pixels, words, byteDepth, index + 3);
                        r += red / 4.0; g += green / 4.0; b += blue / 4.0;
                        alphaSum += a; redSum += a * red; greenSum += a * green; blueSum += a * blue;
                    }
                if (alphaSum != 0)
                {
                    r = (double)redSum / alphaSum; g = (double)greenSum / alphaSum; b = (double)blueSum / alphaSum;
                }
                result.U[y * stride / 2 + x] = Round(128 + 224.0 / 255 * (-.168735891647855 * r - .331264108352145 * g + .5 * b));
                result.V[y * stride / 2 + x] = Round(128 + 224.0 / 255 * (.5 * r - .418687589158345 * g - .081312410841655 * b));
            }
        }
        return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int Sample(ReadOnlySpan<byte> pixels, ReadOnlySpan<ushort> words, bool byteDepth, int index)
        => byteDepth ? pixels[index] : (words[index] + 128) / 257;

    private static byte Round(double value) => (byte)Math.Clamp((int)Math.Floor(value + .5), 0, 255);

    private static int SelectLuma(Vp8Planes source, Vp8Planes reconstructed, int x, int y, int macroX, int macroY,
        int candidates, OperationContext context)
    {
        var best = 0; var score = long.MaxValue;
        Span<int> prepared = stackalloc int[13]; var dc = 0;
        for (var mode = 0; mode < candidates; mode++)
        {
            context.Work(192);
            if (mode == 0) dc = Vp8Prediction.PrepareSmall(reconstructed.Y, source.Stride, x, y, macroX, macroY, prepared);
            Vp8Prediction.SmallPrepared(reconstructed.Y, source.Stride, x, y, mode, prepared, dc);
            var error = Error(source.Y, reconstructed.Y, source.Stride, x, y, 4);
            // Squared error cannot improve on zero; equal scores keep the first
            // mode. Its prediction is already present in the destination.
            if (error == 0) return mode;
            if (error < score) { best = mode; score = error; }
        }
        if (candidates > 0) Vp8Prediction.SmallPrepared(reconstructed.Y, source.Stride, x, y, best, prepared, dc);
        else Vp8Prediction.Small(reconstructed.Y, source.Stride, x, y, macroX, macroY, best);
        return best;
    }

    private static int SelectChroma(Vp8Planes source, Vp8Planes reconstructed, int x, int y, int candidates, OperationContext context)
    {
        var best = 0; var score = long.MaxValue; var stride = source.Stride / 2;
        for (var mode = 0; mode < candidates; mode++)
        {
            context.Work(512); Vp8Prediction.Large(reconstructed.U, stride, x, y, 8, mode); Vp8Prediction.Large(reconstructed.V, stride, x, y, 8, mode);
            var error = Error(source.U, reconstructed.U, stride, x, y, 8) + Error(source.V, reconstructed.V, stride, x, y, 8);
            if (error == 0) return mode;
            if (error < score) { best = mode; score = error; }
        }
        Vp8Prediction.Large(reconstructed.U, stride, x, y, 8, best); Vp8Prediction.Large(reconstructed.V, stride, x, y, 8, best); return best;
    }

    private static long Error(byte[] source, byte[] prediction, int stride, int x, int y, int size)
    {
        long sum = 0;
        for (var dy = 0; dy < size; dy++)
            for (var dx = 0; dx < size; dx++)
            {
                var index = (y + dy) * stride + x + dx; var difference = source[index] - prediction[index];
                sum += difference * difference;
            }
        return sum;
    }

    private static void Block(byte[] source, byte[] reconstructed, int stride, int x, int y, int plane, int q,
        Vp8BooleanWriter writer, Span<byte> above, Span<byte> left, int a, int l, OperationContext context,
        CoefficientModel? model, int block, int sampleOffset)
    {
        context.Work(512); Span<short> residual = stackalloc short[16]; Span<short> coefficients = stackalloc short[16];
        for (var dy = 0; dy < 4; dy++)
            for (var dx = 0; dx < 4; dx++)
            {
                var index = (y + dy) * stride + x + dx; residual[dy * 4 + dx] = (short)(source[index] - reconstructed[index]);
            }
        var dc = (int)Vp8Tables.DcQuantizers[q]; if (plane == 2) dc = Math.Min(132, dc);
        var ac = Vp8Tables.AcQuantizers[q]; Vp8ForwardDct.Quantize(residual, coefficients, dc, ac);
        var nonzero = model is null ? Coefficients(writer, coefficients, plane, above[a] + left[l])
            : model.Ready ? Coefficients(writer, coefficients, plane, above[a] + left[l], model.Probabilities)
            : model.Record(source, stride, x, y, sampleOffset, block, coefficients, plane, above[a] + left[l]);
        above[a] = left[l] = nonzero;
        for (var i = 0; i < 16; i++) coefficients[i] = checked((short)(coefficients[i] * (i == 0 ? dc : ac)));
        Vp8Transforms.InverseDct(coefficients);
        for (var dy = 0; dy < 4; dy++)
            for (var dx = 0; dx < 4; dx++)
            {
                var index = (y + dy) * stride + x + dx;
                reconstructed[index] = (byte)Math.Clamp(reconstructed[index] + coefficients[dy * 4 + dx], 0, 255);
            }
    }

    internal static byte Coefficients(Vp8BooleanWriter writer, ReadOnlySpan<short> coefficients, int plane, int predictor)
        => Coefficients(writer, coefficients, plane, predictor, Vp8Tables.CoefficientProbabilities);

    private static byte Coefficients(Vp8BooleanWriter writer, ReadOnlySpan<short> coefficients, int plane, int predictor,
        ReadOnlySpan<byte> probabilities)
    {
        var first = plane == 0 ? 1 : 0; var last = 15;
        while (last >= first && coefficients[Zigzag[last]] == 0) last--;
        var previousZero = false;
        for (var c = first; c <= Math.Min(Math.Max(last + 1, first), 15); c++)
        {
            var p = probabilities.Slice(((plane * 8 + Bands[c]) * 3 + predictor) * 11, 11);
            if (c > last) { writer.Codeword(TokenPaths[11], p); break; }
            var value = coefficients[Zigzag[c]]; var magnitude = Math.Abs((int)value);
            var category = magnitude < 5 ? -1 : magnitude < 7 ? 0 : magnitude < 11 ? 1 : magnitude < 19 ? 2
                : magnitude < 35 ? 3 : magnitude < 67 ? 4 : 5;
            var path = TokenPaths[category < 0 ? magnitude : category + 5];
            writer.Codeword(previousZero ? path >> 5 : path, p);
            if (category >= 0) Category(writer, category, magnitude);
            if (magnitude != 0) writer.Write(value < 0 ? 1 : 0);
            predictor = Math.Min(magnitude, 2); previousZero = magnitude == 0;
        }
        return last < first ? (byte)0 : (byte)1;
    }

    private static void Category(Vp8BooleanWriter writer, int category, int magnitude)
    {
        ReadOnlySpan<byte> p = category switch
        {
            0 => [159], 1 => [165, 145], 2 => [173, 148, 140], 3 => [176, 155, 140, 135],
            4 => [180, 157, 141, 134, 130], _ => [254, 254, 243, 230, 196, 177, 153, 140, 133, 130, 129]
        };
        ReadOnlySpan<int> bases = [5, 7, 11, 19, 35, 67]; var value = magnitude - bases[category];
        if ((uint)value >= 1u << p.Length) throw new InvalidOperationException("VP8 coefficient exceeds its category.");
        for (var bit = 0; bit < p.Length; bit++) writer.Write(value >> (p.Length - 1 - bit) & 1, p[bit]);
    }

    private static Vp8BooleanWriter BuildHeader(int q, byte[] modes, byte[] chroma, int columns, int rows,
        ReadOnlySpan<byte> probabilities, OperationContext context, QuantizationPlan? quantization = null)
    {
        var writer = new Vp8BooleanWriter(context, Math.Min(0x7FFFF, context.Limits.MaxOutputBytes));
        try { Header(writer, q, modes, chroma, columns, rows, probabilities, context, quantization); writer.Finish(); return writer; }
        catch { writer.Dispose(); throw; }
    }

    private static void Header(Vp8BooleanWriter writer, int q, byte[] modes, byte[] chroma, int columns, int rows,
        ReadOnlySpan<byte> probabilities, OperationContext context, QuantizationPlan? quantization = null)
    {
        writer.Write(0); writer.Write(0); // Color and clamp hint.
        if (quantization is null) writer.Write(0); else quantization.Header(writer);
        writer.Write(0); writer.Literal(0, 6); writer.Literal(0, 3); writer.Write(0); // No loop filtering or deltas.
        writer.Literal(0, 2); writer.Literal(q, 7); // One token partition.
        for (var i = 0; i < 5; i++) writer.Write(0);
        writer.Write(0); // Still-image entropy refresh has no following frame.
        for (var i = 0; i < probabilities.Length; i++)
        {
            var changed = probabilities[i] != Vp8Tables.CoefficientProbabilities[i];
            writer.Write(changed ? 1 : 0, Vp8Tables.CoefficientUpdateProbabilities[i]);
            if (changed) writer.Literal(probabilities[i], 8);
        }
        writer.Write(0); // No coefficient-skip syntax.
        var above = new byte[columns * 4]; Span<byte> left = stackalloc byte[4];
        for (var my = 0; my < rows; my++)
        {
            left.Clear();
            for (var mx = 0; mx < columns; mx++)
            {
                context.Work(64); var index = my * columns + mx;
                quantization?.Macroblock(writer, index);
                if ((modes[index * 16] & 128) != 0)
                {
                    var mode = modes[index * 16] & 3;
                    writer.Write(1, 145); writer.Write(mode < 2 ? 0 : 1, 156);
                    writer.Write(mode & 1, mode < 2 ? 163 : 128);
                    var equivalent = LargeEquivalent(mode);
                    above.AsSpan(mx * 4, 4).Fill((byte)equivalent); left.Fill((byte)equivalent);
                }
                else
                {
                    writer.Write(0, 145); // B_PRED.
                    for (var by = 0; by < 4; by++)
                        for (var bx = 0; bx < 4; bx++)
                        {
                            var p = (above[mx * 4 + bx] * 10 + left[by]) * 9; var mode = modes[index * 16 + by * 4 + bx];
                            writer.Codeword(ModePaths[mode], Vp8Tables.BlockModeProbabilities.Slice(p, 9));
                            above[mx * 4 + bx] = left[by] = mode;
                        }
                }
                var uv = chroma[index]; writer.Write(uv == 0 ? 0 : 1, 142);
                if (uv != 0) { writer.Write(uv == 1 ? 0 : 1, 114); if (uv >= 2) writer.Write(uv - 2, 183); }
            }
        }
    }

    private static void WriteContainer(int width, int height, byte[]? alpha,
        WebpLosslessEncoder.AlphaEncoding? compressedAlpha, ReadOnlySpan<byte> header,
        ReadOnlySpan<byte> tokens, Stream output, OperationContext context)
    {
        var payload = 10L + header.Length + tokens.Length;
        var alphaLength = alpha is null ? 0 : (compressedAlpha?.Length ?? alpha.Length) + 1;
        var total = 20L + payload + (payload & 1) + (alpha is null ? 0 : 26L + alphaLength + (alphaLength & 1));
        if (total > context.Limits.MaxOutputBytes) throw new ImageLimitException(ImageLimitKind.OutputBytes, "WebP encoded output limit exceeded.");
        Span<byte> bytes = stackalloc byte[30]; bytes.Clear();
        "RIFF"u8.CopyTo(bytes); BinaryPrimitives.WriteUInt32LittleEndian(bytes[4..], checked((uint)(total - 8)));
        "WEBP"u8.CopyTo(bytes[8..]); output.Write(bytes[..12]);
        if (alpha is not null)
        {
            bytes.Clear(); "VP8X"u8.CopyTo(bytes); BinaryPrimitives.WriteInt32LittleEndian(bytes[4..], 10); bytes[8] = 16;
            var w = width - 1; var h = height - 1;
            for (var i = 0; i < 3; i++) { bytes[12 + i] = (byte)(w >> (8 * i)); bytes[15 + i] = (byte)(h >> (8 * i)); }
            output.Write(bytes[..18]); "ALPH"u8.CopyTo(bytes); BinaryPrimitives.WriteInt32LittleEndian(bytes[4..], alphaLength);
            output.Write(bytes[..8]); output.WriteByte(compressedAlpha?.Header ?? 0);
            if (compressedAlpha is not null) compressedAlpha.Write(alpha, width, output);
            else WritePartition(alpha, output, context);
            if ((alphaLength & 1) != 0) output.WriteByte(0);
        }
        bytes.Clear(); "VP8 "u8.CopyTo(bytes); BinaryPrimitives.WriteInt32LittleEndian(bytes[4..], (int)payload);
        var tag = (header.Length << 5) | 16;
        for (var i = 0; i < 3; i++) bytes[8 + i] = (byte)(tag >> (8 * i));
        bytes[11] = 0x9D; bytes[12] = 1; bytes[13] = 0x2A;
        BinaryPrimitives.WriteUInt16LittleEndian(bytes[14..], (ushort)width); BinaryPrimitives.WriteUInt16LittleEndian(bytes[16..], (ushort)height);
        output.Write(bytes[..18]); WritePartition(header, output, context); WritePartition(tokens, output, context);
        if ((payload & 1) != 0) output.WriteByte(0);
    }

    private static void WritePartition(ReadOnlySpan<byte> bytes, Stream output, OperationContext context)
    {
        while (!bytes.IsEmpty)
        {
            var size = Math.Min(65536, bytes.Length); context.Work(size); output.Write(bytes[..size]); bytes = bytes[size..];
        }
    }
}
