using System.Buffers.Binary;

namespace Lokad.Image.Internal;

internal sealed record Vp8Planes(int Width, int Height, int Stride, byte[] Y, byte[] U, byte[] V)
{
    public long MemoryBytes => Y.Length + (long)U.Length + V.Length + 128;
}

/// <summary>Independently implemented RFC 6386 key-frame decoder for still WebP.</summary>
internal ref struct Vp8Decoder
{
    private readonly ReadOnlySpan<byte> data;
    private readonly OperationContext context;
    private readonly int width, height, columns, rows;
    private Vp8BooleanReader header;
    private Vp8BooleanReader[] partitions;
    private byte[] probabilities;
    private int[] segmentQuantizers, segmentFilters, referenceDeltas, modeDeltas, quantizerDeltas;
    private byte[] segmentProbabilities;
    private bool segmentMap, absoluteSegments, segmentation, simpleFilter, deltasEnabled, skipEnabled;
    private int filterLevel, sharpness, quantizer, skipProbability;

    private struct Macroblock { public byte Mode, Chroma, Segment, Flags; }

    public Vp8Decoder(ReadOnlySpan<byte> data, OperationContext context)
    {
        this = default; this.data = data; this.context = context;
        if (data.Length < 10 || (data[0] & 1) != 0 || !data.Slice(3, 3).SequenceEqual(new byte[] { 0x9D, 1, 0x2A }))
            throw new ImageDataException("Invalid VP8 key-frame header.");
        var version = data[0] >> 1 & 7;
        if (version > 3) throw new UnsupportedImageException(UnsupportedImageFeature.Version, "Unsupported VP8 version.", ImageFormat.WebP);
        if ((data[0] & 16) == 0) throw new ImageDataException("WebP requires a displayed VP8 frame.");
        var codedWidth = BinaryPrimitives.ReadUInt16LittleEndian(data[6..]);
        var codedHeight = BinaryPrimitives.ReadUInt16LittleEndian(data[8..]);
        // WebP uses coded canvas dimensions. Video display scaling hints do not resize its pixels.
        width = codedWidth & 0x3FFF; height = codedHeight & 0x3FFF;
        if (width == 0 || height == 0) throw new ImageDataException("Invalid VP8 dimensions.");
        context.PixelBytes(width, height, 8); columns = (width + 15) / 16; rows = (height + 15) / 16;
        context.Reserve(8192); // Header models, eight arithmetic states and fixed transform scratch.
        var firstLength = (data[0] | data[1] << 8 | data[2] << 16) >> 5;
        if (firstLength < 1 || firstLength > data.Length - 10) throw new ImageDataException("Invalid VP8 first partition length.");
        header = new Vp8BooleanReader(data, 10, firstLength);
        probabilities = Vp8Tables.CoefficientProbabilities.ToArray(); partitions = [];
        segmentQuantizers = new int[4]; segmentFilters = new int[4]; segmentProbabilities = [255, 255, 255];
        referenceDeltas = new int[4]; modeDeltas = new int[4]; quantizerDeltas = new int[5];
        ReadHeader(firstLength);
    }

    private int Bit(int probability = 128) => header.Read(data, probability, context);
    private int Literal(int count) => header.Literal(data, count, context);
    private int Signed(int count) { var magnitude = Literal(count); return Bit() == 0 ? magnitude : -magnitude; }

    private void ReadHeader(int firstLength)
    {
        if (Bit() != 0) throw new UnsupportedImageException(UnsupportedImageFeature.ColorSpace, "Unsupported VP8 color space.", ImageFormat.WebP);
        Bit(); // The clamping hint does not change safe reconstruction.
        segmentation = Bit() != 0;
        if (segmentation)
        {
            segmentMap = Bit() != 0;
            if (Bit() != 0)
            {
                absoluteSegments = Bit() != 0; // Annex A section 19.2: 0=delta, 1=absolute.
                for (var i = 0; i < 4; i++) if (Bit() != 0) segmentQuantizers[i] = Signed(7);
                for (var i = 0; i < 4; i++) if (Bit() != 0) segmentFilters[i] = Signed(6);
            }
            if (segmentMap)
                for (var i = 0; i < 3; i++) if (Bit() != 0) segmentProbabilities[i] = (byte)Literal(8);
        }
        simpleFilter = Bit() != 0; filterLevel = Literal(6); sharpness = Literal(3);
        deltasEnabled = Bit() != 0;
        if (deltasEnabled && Bit() != 0)
        {
            for (var i = 0; i < 4; i++) if (Bit() != 0) referenceDeltas[i] = Signed(6);
            for (var i = 0; i < 4; i++) if (Bit() != 0) modeDeltas[i] = Signed(6);
        }
        var partitionCount = 1 << Literal(2);
        quantizer = Literal(7);
        for (var i = 0; i < 5; i++) if (Bit() != 0) quantizerDeltas[i] = Signed(4);
        Bit(); // Entropy refresh is immaterial for an independently decoded still frame.
        for (var i = 0; i < probabilities.Length; i++)
            if (Bit(Vp8Tables.CoefficientUpdateProbabilities[i]) != 0) probabilities[i] = (byte)Literal(8);
        skipEnabled = Bit() != 0; skipProbability = skipEnabled ? Literal(8) : 0;
        var sizesOffset = 10 + firstLength; var start = sizesOffset + 3 * (partitionCount - 1);
        if (start > data.Length) throw new ImageDataException("Truncated VP8 partition sizes.");
        partitions = new Vp8BooleanReader[partitionCount];
        for (var i = 0; i < partitionCount; i++)
        {
            var offset = sizesOffset + i * 3;
            var length = i + 1 == partitionCount ? data.Length - start : data[offset] | data[offset + 1] << 8 | data[offset + 2] << 16;
            partitions[i] = new Vp8BooleanReader(data, start, length); start += length;
        }
    }

    public Vp8Planes DecodePlanes() => ReadPlanes(true)!;

    internal void ValidatePixels()
    {
        ReadPlanes(false);
        context.Cancellation.ThrowIfCancellationRequested();
    }

    private Vp8Planes? ReadPlanes(bool reconstruct)
    {
        var count = columns * rows; var metadataBytes = count * 20L + columns * 13L + 256;
        context.Reserve(metadataBytes);
        var blocks = new Macroblock[count]; var modes = new byte[count * 16]; var aboveModes = new byte[columns * 4];
        Span<byte> leftModes = stackalloc byte[4];
        for (var my = 0; my < rows; my++)
        {
            leftModes.Clear();
            for (var mx = 0; mx < columns; mx++)
            {
                var index = my * columns + mx;
                ref var block = ref blocks[index];
                if (segmentMap)
                {
                    var half = Bit(segmentProbabilities[0]);
                    block.Segment = (byte)(half * 2 + Bit(segmentProbabilities[half + 1]));
                }
                if (skipEnabled && Bit(skipProbability) != 0) block.Flags |= 1;
                block.Mode = (byte)(Bit(145) == 0 ? 4 : Bit(156) == 0 ? Bit(163) : 2 + Bit(128));
                if (block.Mode == 4)
                    for (var by = 0; by < 4; by++)
                        for (var bx = 0; bx < 4; bx++)
                        {
                            var contextIndex = (aboveModes[mx * 4 + bx] * 10 + leftModes[by]) * 9;
                            var mode = ReadSmallMode(Vp8Tables.BlockModeProbabilities.Slice(contextIndex, 9));
                            aboveModes[mx * 4 + bx] = leftModes[by] = modes[index * 16 + by * 4 + bx] = (byte)mode;
                        }
                else
                {
                    var mode = block.Mode switch { 0 => 0, 1 => 2, 2 => 3, _ => 1 };
                    aboveModes.AsSpan(mx * 4, 4).Fill((byte)mode); leftModes.Fill((byte)mode);
                }
                block.Chroma = (byte)(Bit(142) == 0 ? 0 : Bit(114) == 0 ? 1 : 2 + Bit(183));
            }
        }
        var stride = columns * 16; var planeHeight = rows * 16;
        var lumaSize = (long)stride * planeHeight; var chromaSize = lumaSize / 4;
        if (lumaSize > Array.MaxLength) throw new ImageLimitException(ImageLimitKind.BufferLength, "VP8 reconstruction plane is too large.");
        Vp8Planes? result = null;
        if (reconstruct)
        {
            context.Reserve(lumaSize + chromaSize * 2 + 128);
            result = new Vp8Planes(width, height, stride, new byte[(int)lumaSize], new byte[(int)chromaSize], new byte[(int)chromaSize]);
        }
        var aboveCoefficients = new byte[columns * 9]; Span<byte> leftCoefficients = stackalloc byte[9];
        Span<short> coefficients = stackalloc short[25 * 16];
        Span<int> predictionSamples = stackalloc int[13];
        for (var my = 0; my < rows; my++)
        {
            leftCoefficients.Clear();
            for (var mx = 0; mx < columns; mx++)
            {
                context.Cancellation.ThrowIfCancellationRequested();
                var index = my * columns + mx; ref var block = ref blocks[index];
                var q = segmentation ? segmentQuantizers[block.Segment] + (absoluteSegments ? 0 : quantizer) : quantizer;
                q = Math.Clamp(q, 0, 127); var any = false; coefficients.Clear();
                ref var tokens = ref partitions[my % partitions.Length];
                var above = aboveCoefficients.AsSpan(mx * 9, 9);
                var hasY2 = block.Mode != 4;
                if ((block.Flags & 1) == 0)
                {
                    if (hasY2)
                    {
                        any |= Coefficients(ref tokens, coefficients[..16], 1, above[8] + leftCoefficients[8],
                            2 * Dc(q + quantizerDeltas[1]), Math.Max(8, Ac(q + quantizerDeltas[2]) * 155 / 100), out var nonzero);
                        above[8] = leftCoefficients[8] = nonzero;
                        Vp8Transforms.InverseWalsh(coefficients[..16]);
                    }
                    for (var b = 0; b < 24; b++)
                    {
                        var luma = b < 16; var local = luma ? b : (b - 16) % 4; var side = luma ? 4 : 2;
                        var offset = luma ? 0 : b < 20 ? 4 : 6;
                        var a = offset + local % side; var l = offset + local / side;
                        var subblock = coefficients.Slice((b + 1) * 16, 16);
                        any |= Coefficients(ref tokens, subblock, luma ? hasY2 ? 0 : 3 : 2, above[a] + leftCoefficients[l],
                            luma ? Dc(q + quantizerDeltas[0]) : Math.Min(132, Dc(q + quantizerDeltas[3])),
                            luma ? Ac(q) : Ac(q + quantizerDeltas[4]), out var nonzero);
                        above[a] = leftCoefficients[l] = nonzero;
                    }
                }
                else
                {
                    above[..8].Clear(); leftCoefficients[..8].Clear();
                    if (hasY2) above[8] = leftCoefficients[8] = 0;
                }
                if (!any && hasY2) block.Flags |= 2;
                // Entropy contexts depend on parsed nonzeros, never on pixels.
                // Validation consumes the identical tokens without discarded planes.
                if (result is null) continue;
                var x = mx * 16; var y = my * 16;
                if (hasY2) Vp8Prediction.Large(result.Y, stride, x, y, 16, block.Mode);
                Vp8Prediction.Large(result.U, stride / 2, x / 2, y / 2, 8, block.Chroma);
                Vp8Prediction.Large(result.V, stride / 2, x / 2, y / 2, 8, block.Chroma);
                for (var b = 0; b < 24; b++)
                {
                    var luma = b < 16; var local = luma ? b : (b - 16) % 4; var side = luma ? 4 : 2;
                    var bx = (luma ? x : x / 2) + local % side * 4; var by = (luma ? y : y / 2) + local / side * 4;
                    var plane = luma ? result.Y : b < 20 ? result.U : result.V; var pitch = luma ? stride : stride / 2;
                    if (luma && !hasY2)
                    {
                        var predictionDc = Vp8Prediction.PrepareSmall(plane, pitch, bx, by, x, y, predictionSamples);
                        Vp8Prediction.SmallPrepared(plane, pitch, bx, by, modes[index * 16 + b], predictionSamples, predictionDc);
                    }
                    var subblock = coefficients.Slice((b + 1) * 16, 16);
                    if (luma && hasY2) subblock[0] = coefficients[b];
                    // Residual scratch is dead after reconstruction. Keep the Y2
                    // DCs in the separate leading block for subsequent luma blocks.
                    Vp8BlockReconstruction.Add(subblock, plane.AsSpan(by * pitch + bx), pitch);
                }
            }
        }
        // RFC 6386 section 15: a zero frame-level filter disables the entire pass,
        // including positive segment overrides and reference/mode deltas.
        if (result is not null && filterLevel != 0)
            for (var my = 0; my < rows; my++)
            for (var mx = 0; mx < columns; mx++)
            {
                // Check even when this block's adjustments reduce its level to zero.
                context.Cancellation.ThrowIfCancellationRequested();
                var block = blocks[my * columns + mx];
                var level = segmentation ? segmentFilters[block.Segment] + (absoluteSegments ? 0 : filterLevel) : filterLevel;
                if (deltasEnabled) level += referenceDeltas[0] + (block.Mode == 4 ? modeDeltas[0] : 0);
                level = Math.Clamp(level, 0, 63); var skip = (block.Flags & 2) != 0;
                Vp8LoopFilter.Macroblock(result.Y, stride, mx * 16, my * 16, 16, level, sharpness, simpleFilter, skip, context);
                if (!simpleFilter)
                {
                    Vp8LoopFilter.Macroblock(result.U, stride / 2, mx * 8, my * 8, 8, level, sharpness, false, skip, context);
                    Vp8LoopFilter.Macroblock(result.V, stride / 2, mx * 8, my * 8, 8, level, sharpness, false, skip, context);
                }
            }
        context.Release(metadataBytes + 8192); return result;
    }

    private int ReadSmallMode(ReadOnlySpan<byte> p)
    {
        if (Bit(p[0]) == 0) return 0;
        if (Bit(p[1]) == 0) return 1;
        if (Bit(p[2]) == 0) return 2;
        if (Bit(p[3]) == 0) return Bit(p[4]) == 0 ? 3 : Bit(p[5]) == 0 ? 5 : 6;
        if (Bit(p[6]) == 0) return 4;
        if (Bit(p[7]) == 0) return 7;
        return Bit(p[8]) == 0 ? 8 : 9;
    }

    private bool Coefficients(ref Vp8BooleanReader tokens, scoped Span<short> values, int plane, int initialContext,
        int dc, int ac, out byte nonzero)
    {
        if (tokens.CanReadCoefficientBlock(data, context))
            return tokens.ReadCoefficientBlock(data, probabilities, values, plane, initialContext, dc, ac, context, out nonzero);
        if (tokens.CanReadCancellableCoefficientBlock(data, context))
            return tokens.ReadCancellableCoefficientBlock(data, probabilities, values, plane, initialContext, dc, ac, context, out nonzero);
        ReadOnlySpan<byte> bands = [0, 1, 2, 3, 6, 4, 5, 6, 6, 6, 6, 6, 6, 6, 6, 7];
        ReadOnlySpan<byte> zigzag = [0, 1, 4, 8, 5, 2, 3, 6, 9, 12, 13, 10, 7, 11, 14, 15];
        var previousZero = false; var predictor = initialContext; nonzero = 0;
        for (var c = plane == 0 ? 1 : 0; c < 16; c++)
        {
            var p = probabilities.AsSpan(((plane * 8 + bands[c]) * 3 + predictor) * 11, 11);
            var token = tokens.CoefficientToken(data, p, context, previousZero);
            if (token == 11) break;
            var value = token < 5 ? token : Category(ref tokens, token - 5);
            previousZero = value == 0; predictor = Math.Min(value, 2);
            if (value == 0) continue;
            nonzero = 1;
            if (tokens.Read(data, 128, context) != 0) value = -value;
            values[zigzag[c]] = unchecked((short)(value * (c == 0 ? dc : ac)));
        }
        return nonzero != 0;
    }

    private int Category(ref Vp8BooleanReader tokens, int category)
    {
        ReadOnlySpan<byte> p = category switch
        {
            0 => [159],
            1 => [165, 145],
            2 => [173, 148, 140],
            3 => [176, 155, 140, 135],
            4 => [180, 157, 141, 134, 130],
            _ => [254, 254, 243, 230, 196, 177, 153, 140, 133, 130, 129]
        };
        ReadOnlySpan<int> bases = [5, 7, 11, 19, 35, 67]; var value = 0;
        foreach (var probability in p) value = value * 2 + tokens.Read(data, probability, context);
        return bases[category] + value;
    }

    private static int Dc(int index) => Vp8Tables.DcQuantizers[Math.Clamp(index, 0, 127)];
    private static int Ac(int index) => Vp8Tables.AcQuantizers[Math.Clamp(index, 0, 127)];
}
