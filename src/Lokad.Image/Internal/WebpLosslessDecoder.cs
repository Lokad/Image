using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace Lokad.Image.Internal;

/// <summary>Independently implemented RFC 9649 section 3 VP8L decoding.</summary>
internal ref struct WebpLosslessDecoder
{
    private WebpBitReader reader;
    private readonly OperationContext context;
    private readonly int width, height;

    private sealed class Transform
    {
        public int Type, Width, Bits;
        public uint[] Data = [];
    }

    public WebpLosslessDecoder(ReadOnlySpan<byte> payload, int width, int height, OperationContext context, bool header = true)
    {
        if (header && (payload.Length < 5 || payload[0] != 0x2F)) throw new ImageDataException("Invalid VP8L header.");
        reader = new WebpBitReader(header ? payload[5..] : payload, context);
        this.width = width; this.height = height; this.context = context;
    }

    public ImageBuffer Decode()
    {
        context.Reserve(512); // Four transform descriptors and fixed parser scratch.
        var transforms = new Transform[4]; var count = 0; var seen = 0; var codedWidth = width;
        while (reader.Read(1) != 0)
        {
            var type = reader.Read(2);
            if ((seen & (1 << type)) != 0) throw new ImageDataException("Duplicate VP8L transform.");
            seen |= 1 << type;
            var transform = new Transform { Type = type, Width = codedWidth };
            if (type is 0 or 1)
            {
                transform.Bits = reader.Read(3) + 2;
                transform.Data = ReadImage(Ceil(codedWidth, transform.Bits), Ceil(height, transform.Bits), false);
                if (type == 0) ValidatePredictorModes(transform.Data, context);
            }
            else if (type == 3)
            {
                var colors = reader.Read(8) + 1;
                transform.Data = ReadImage(colors, 1, false);
                for (var i = 1; i < colors; i++) transform.Data[i] = Add(transform.Data[i], transform.Data[i - 1]);
                transform.Bits = colors <= 2 ? 3 : colors <= 4 ? 2 : colors <= 16 ? 1 : 0;
                codedWidth = Ceil(codedWidth, transform.Bits);
            }
            transforms[count++] = transform;
        }
        var pixels = ReadImage(codedWidth, height, true);
        for (var index = count - 1; index >= 0; index--)
        {
            var transform = transforms[index];
            if (transform.Type == 3)
            {
                var expanded = Allocate(transform.Width, height);
                var perByte = 1 << transform.Bits; var indexBits = 8 >> transform.Bits;
                var mask = (1 << indexBits) - 1;
                for (var y = 0; y < height; y++)
                {
                    context.Work(transform.Width * 4L);
                    for (var x = 0; x < transform.Width; x++)
                    {
                        var packed = (pixels[y * codedWidth + (x >> transform.Bits)] >> 8) & 255;
                        var color = (int)(packed >> ((x & (perByte - 1)) * indexBits)) & mask;
                        expanded[y * transform.Width + x] = color < transform.Data.Length ? transform.Data[color] : 0;
                    }
                }
                context.Release(pixels.Length * 4L + 32); pixels = expanded; codedWidth = transform.Width;
            }
            else if (transform.Type == 0 && Sse2.IsSupported) ApplyPredictor(transform, pixels, codedWidth);
            else Apply(transform, pixels, codedWidth);
            context.Release(transform.Data.Length == 0 ? 0 : transform.Data.Length * 4L + 32);
            transform.Data = [];
        }
        var result = ImageBuffer.Allocate(width, height, 8, context);
        try
        {
            for (var y = 0; y < height; y++)
            {
                context.Work(width * 4L);
                var row = result.GetRow8(y);
                for (var x = 0; x < width; x++)
                {
                    var pixel = pixels[y * width + x];
                    row[x * 4] = (byte)(pixel >> 16); row[x * 4 + 1] = (byte)(pixel >> 8);
                    row[x * 4 + 2] = (byte)pixel; row[x * 4 + 3] = (byte)(pixel >> 24);
                }
            }
            context.Release(pixels.Length * 4L + 32 + 512);
            return result;
        }
        catch { result.Dispose(); throw; }
    }

    private uint[] ReadImage(int imageWidth, int imageHeight, bool spatial)
    {
        var cacheEnabled = reader.Read(1) != 0;
        var cacheBits = cacheEnabled ? reader.Read(4) : 0;
        if (cacheEnabled && cacheBits is < 1 or > 11)
            throw new ImageDataException("Invalid VP8L cache size.");
        var cacheSize = cacheBits == 0 ? 0 : 1 << cacheBits;
        var cacheBytes = cacheSize == 0 ? 0 : cacheSize * 4L + 32;
        context.Reserve(cacheBytes); var cache = cacheSize == 0 ? [] : new uint[cacheSize];
        uint[] entropy = []; int entropyWidth = 0, entropyBits = 0, groups = 1;
        if (spatial && reader.Read(1) != 0)
        {
            entropyBits = reader.Read(3) + 2; entropyWidth = Ceil(imageWidth, entropyBits);
            entropy = ReadImage(entropyWidth, Ceil(imageHeight, entropyBits), false);
            groups = CountEntropyGroups(entropy, context);
        }
        var tableBytes = groups * 5L * 8 + 32;
        context.Reserve(tableBytes); var tables = new WebpHuffman[groups * 5];
        for (var group = 0; group < groups; group++)
        {
            tables[group * 5] = ReadHuffman(280 + cacheSize);
            for (var channel = 1; channel < 4; channel++) tables[group * 5 + channel] = ReadHuffman(256);
            tables[group * 5 + 4] = ReadHuffman(40);
        }
        var pixels = Allocate(imageWidth, imageHeight); var position = 0;
        var scratchStart = 0; var lookupEntries = (long)groups * 4096;
        if (lookupEntries <= pixels.Length / 2)
        {
            for (var group = 0; group < groups; group++)
            {
                var green = tables[group * 5];
                if (!green.CanBuildWideGreen) continue;
                context.Cancellation.ThrowIfCancellationRequested();
                scratchStart = pixels.Length - (int)lookupEntries;
                green.BuildWideGreen(pixels.AsSpan(scratchStart + group * 4096, 4096));
            }
        }
        while (position < pixels.Length)
        {
            context.Work(1);
            var group = entropy.Length == 0 ? 0 : (int)((entropy[(position / imageWidth >> entropyBits) * entropyWidth
                + (position % imageWidth >> entropyBits)] >> 8) & 65535);
            var start = group * 5;
            var symbol = position < scratchStart && tables[start].CanBuildWideGreen
                && reader.TryReadWideGreen(pixels.AsSpan(scratchStart + group * 4096, 4096), out var wideSymbol)
                ? wideSymbol : tables[start].Read(ref reader);
            if (symbol < 256)
            {
                var redTree=tables[start+1];var blueTree=tables[start+2];var alphaTree=tables[start+3];
                uint pixel;
                if(redTree.TryReadByteChannels(blueTree,alphaTree,ref reader,out var channels))
                    pixel=channels|((uint)symbol<<8);
                else
                {
                    var red=redTree.Read(ref reader);var blue=blueTree.Read(ref reader);
                    var alpha=alphaTree.Read(ref reader);
                    pixel=(uint)(alpha<<24|red<<16|symbol<<8|blue);
                }
                pixels[position++] = pixel; Insert(cache, cacheBits, pixel);
            }
            else if (symbol < 280)
            {
                var length = Prefix(symbol - 256);
                var distance = WebpDistances.Resolve(Prefix(tables[start + 4].Read(ref reader)), imageWidth);
                if (distance > position || length > pixels.Length - position)
                    throw new ImageDataException("VP8L backward reference exceeds the raster.");
                context.Work(length);
                for (var i = 0; i < length; i++)
                {
                    var pixel = pixels[position - distance]; pixels[position++] = pixel; Insert(cache, cacheBits, pixel);
                }
            }
            else
            {
                var cacheIndex = symbol - 280;
                if ((uint)cacheIndex >= (uint)cache.Length) throw new ImageDataException("Invalid VP8L cache index.");
                var pixel = cache[cacheIndex]; pixels[position++] = pixel; Insert(cache, cacheBits, pixel);
            }
        }
        foreach (var table in tables) context.Release(table.MemoryBytes);
        context.Release(tableBytes + cacheBytes + (entropy.Length == 0 ? 0 : entropy.Length * 4L + 32));
        return pixels;

    }

    private WebpHuffman ReadHuffman(int alphabet)
    {
        context.Reserve(alphabet + 32); var lengths = new byte[alphabet];
        if (reader.Read(1) != 0)
        {
            var symbols = reader.Read(1) + 1;
            var first = reader.Read(reader.Read(1) == 0 ? 1 : 8);
            if (first >= alphabet) throw new ImageDataException("VP8L prefix symbol exceeds its alphabet.");
            lengths[first] = 1;
            if (symbols == 2)
            {
                var second = reader.Read(8);
                if (second >= alphabet) throw new ImageDataException("VP8L prefix symbol exceeds its alphabet.");
                lengths[second] = 1; // Duplicate simple symbols are permitted by RFC 9649.
            }
        }
        else
        {
            // Normative code-length symbol order in RFC 9649 section 3.7.2.1.2.
            ReadOnlySpan<byte> order = [17, 18, 0, 1, 2, 3, 4, 5, 16, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15];
            Span<byte> codeLengths = stackalloc byte[19];
            codeLengths.Clear();
            var count = reader.Read(4) + 4;
            for (var i = 0; i < count; i++) codeLengths[order[i]] = (byte)reader.Read(3);
            var codes = new WebpHuffman(codeLengths, context);
            var maximum = reader.Read(1) == 0 ? alphabet : reader.Read(2 + 2 * reader.Read(3)) + 2;
            if (maximum > alphabet) throw new ImageDataException("VP8L code-length count exceeds its alphabet.");
            int position = 0, previous = 8;
            for (var token = 0; token < maximum && position < alphabet; token++)
            {
                context.Work(1);
                var symbol = codes.Read(ref reader);
                if (symbol < 16)
                {
                    lengths[position++] = (byte)symbol; if (symbol != 0) previous = symbol;
                }
                else
                {
                    var run = symbol switch { 16 => 3 + reader.Read(2), 17 => 3 + reader.Read(3), _ => 11 + reader.Read(7) };
                    if (run > alphabet - position) throw new ImageDataException("VP8L code-length repeat exceeds its alphabet.");
                    lengths.AsSpan(position, run).Fill(symbol == 16 ? (byte)previous : (byte)0); position += run;
                }
            }
            context.Release(codes.MemoryBytes);
        }
        var table = new WebpHuffman(lengths, context); context.Release(alphabet + 32); return table;
    }

    private int Prefix(int code)
    {
        if (code < 4) return code + 1;
        var bits = (code - 2) / 2;
        return ((2 + (code & 1)) << bits) + reader.Read(bits) + 1;
    }

    private uint[] Allocate(int imageWidth, int imageHeight)
    {
        var count = (long)imageWidth * imageHeight;
        if (count <= 0 || count > Array.MaxLength / 4) throw new ImageLimitException(ImageLimitKind.BufferLength, "VP8L auxiliary raster is too large.");
        context.Reserve(count * 4 + 32); return new uint[(int)count];
    }

    private void Apply(Transform transform, uint[] pixels, int imageWidth)
    {
        var tileWidth = Ceil(imageWidth, transform.Bits);
        for (var y = 0; y < height; y++)
        {
            context.Work(imageWidth * 32L);
            for (var x = 0; x < imageWidth; x++)
            {
                var position = y * imageWidth + x; var pixel = pixels[position];
                if (transform.Type == 2)
                {
                    var green = (pixel >> 8) & 255;
                    pixels[position] = pixel & 0xFF00FF00 | ((pixel >> 16) + green & 255) << 16 | (pixel + green & 255);
                }
                else if (transform.Type == 1)
                {
                    var multipliers = transform.Data[(y >> transform.Bits) * tileWidth + (x >> transform.Bits)];
                    var green = (sbyte)(pixel >> 8); var red = (int)(pixel >> 16) & 255; var blue = (int)pixel & 255;
                    red = (red + ((sbyte)multipliers * green >> 5)) & 255;
                    blue = (blue + ((sbyte)(multipliers >> 8) * green >> 5) + ((sbyte)(multipliers >> 16) * (sbyte)red >> 5)) & 255;
                    pixels[position] = pixel & 0xFF00FF00 | (uint)(red << 16 | blue);
                }
                else
                {
                    uint prediction;
                    if (position == 0) prediction = 0xFF000000;
                    else if (y == 0) prediction = pixels[position - 1];
                    else if (x == 0) prediction = pixels[position - imageWidth];
                    else
                    {
                        var mode = (int)(transform.Data[(y >> transform.Bits) * tileWidth + (x >> transform.Bits)] >> 8) & 15;
                        prediction = Predict(mode, pixels[position - 1], pixels[position - imageWidth], pixels[position - imageWidth - 1],
                            pixels[x + 1 == imageWidth ? y * imageWidth : position - imageWidth + 1]);
                    }
                    pixels[position] = Add(pixel, prediction);
                }
            }
        }
    }

    private void ApplyPredictor(Transform transform, uint[] pixels, int imageWidth)
    {
        var tileWidth = Ceil(imageWidth, transform.Bits); var tileSize = 1 << transform.Bits;
        for (var y = 0; y < height; y++)
        {
            context.Work(imageWidth * 32L);
            var row = y * imageWidth;
            if (y == 0)
            {
                for (var x = 0; x < imageWidth; x++)
                    pixels[x] = Add(pixels[x], x == 0 ? 0xFF000000 : pixels[x - 1]);
                continue;
            }
            pixels[row] = Add(pixels[row], pixels[row - imageWidth]);
            for (var x = 1; x < imageWidth;)
            {
                var mode = (int)(transform.Data[(y >> transform.Bits) * tileWidth + (x >> transform.Bits)] >> 8) & 15;
                var end = x + Math.Min(imageWidth - x, tileSize - (x & (tileSize - 1)));
                for (; x < end; x++)
                {
                    var position = row + x;
                    pixels[position] = PredictAndAdd(mode, pixels[position], pixels[position - 1], pixels[position - imageWidth],
                        pixels[position - imageWidth - 1], pixels[x + 1 == imageWidth ? row : position - imageWidth + 1]);
                }
            }
        }
    }

    internal static uint PredictAndAdd(int mode, uint residual, uint left, uint top, uint topLeft, uint topRight)
    {
        var l = Vector128.CreateScalar(left).AsByte(); var t = Vector128.CreateScalar(top).AsByte();
        var tl = Vector128.CreateScalar(topLeft).AsByte(); var tr = Vector128.CreateScalar(topRight).AsByte();
        var prediction = mode switch
        {
            0 => Vector128.CreateScalar(0xFF000000u).AsByte(), 1 => l, 2 => t, 3 => tr, 4 => tl,
            5 => AverageFloor(AverageFloor(l, tr), t), 6 => AverageFloor(l, tl),
            7 => AverageFloor(l, t), 8 => AverageFloor(tl, t), 9 => AverageFloor(t, tr),
            10 => AverageFloor(AverageFloor(l, tl), AverageFloor(t, tr)),
            11 => Sse2.SumAbsoluteDifferences(t, tl).AsUInt64().GetElement(0)
                < Sse2.SumAbsoluteDifferences(l, tl).AsUInt64().GetElement(0) ? l : t,
            12 => ClampChannels(l, t, tl, false),
            13 => ClampChannels(AverageFloor(l, t), Vector128<byte>.Zero, tl, true),
            _ => throw new ImageDataException("Invalid VP8L predictor.")
        };
        return Sse2.Add(Vector128.CreateScalar(residual).AsByte(), prediction).AsUInt32().GetElement(0);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Vector128<byte> AverageFloor(Vector128<byte> first, Vector128<byte> second)
        => Sse2.Subtract(Sse2.Average(first, second), Sse2.And(Sse2.Xor(first, second), Vector128.Create((byte)1)));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Vector128<byte> ClampChannels(Vector128<byte> first, Vector128<byte> second,
        Vector128<byte> previous, bool half)
    {
        var a = Sse2.UnpackLow(first, Vector128<byte>.Zero).AsInt16();
        var c = Sse2.UnpackLow(previous, Vector128<byte>.Zero).AsInt16();
        Vector128<short> value;
        if (half)
        {
            var difference = Sse2.Subtract(a, c);
            var bias = Sse2.And(Sse2.ShiftRightArithmetic(difference, 15), Vector128.Create((short)1));
            value = Sse2.Add(a, Sse2.ShiftRightArithmetic(Sse2.Add(difference, bias), 1));
        }
        else value = Sse2.Subtract(Sse2.Add(a, Sse2.UnpackLow(second, Vector128<byte>.Zero).AsInt16()), c);
        return Sse2.PackUnsignedSaturate(value, Vector128<short>.Zero);
    }

    internal static uint Predict(int mode, uint left, uint top, uint topLeft, uint topRight)
        => mode switch
        {
            0 => 0xFF000000, 1 => left, 2 => top, 3 => topRight, 4 => topLeft,
            5 => Average(Average(left, topRight), top), 6 => Average(left, topLeft),
            7 => Average(left, top), 8 => Average(topLeft, top), 9 => Average(top, topRight),
            10 => Average(Average(left, topLeft), Average(top, topRight)),
            11 => Select(left, top, topLeft), 12 => ClampPrediction(left, top, topLeft, false),
            13 => ClampPrediction(Average(left, top), 0, topLeft, true),
            _ => throw new ImageDataException("Invalid VP8L predictor.")
        };

    internal static uint Add(uint first, uint second)
    {
        uint result = 0;
        for (var shift = 0; shift < 32; shift += 8) result |= (((first >> shift & 255) + (second >> shift & 255)) & 255) << shift;
        return result;
    }

    private static uint Average(uint first, uint second)
    {
        uint result = 0;
        for (var shift = 0; shift < 32; shift += 8) result |= ((first >> shift & 255) + (second >> shift & 255)) / 2 << shift;
        return result;
    }

    private static uint Select(uint left, uint top, uint topLeft)
    {
        int leftError = 0, topError = 0;
        for (var shift = 0; shift < 32; shift += 8)
        {
            var l = (int)(left >> shift & 255); var t = (int)(top >> shift & 255); var tl = (int)(topLeft >> shift & 255);
            leftError += Math.Abs(t - tl); topError += Math.Abs(l - tl);
        }
        return leftError < topError ? left : top;
    }

    private static uint ClampPrediction(uint first, uint second, uint previous, bool half)
    {
        uint result = 0;
        for (var shift = 0; shift < 32; shift += 8)
        {
            var a = (int)(first >> shift & 255); var b = (int)(second >> shift & 255); var c = (int)(previous >> shift & 255);
            result |= (uint)Math.Clamp(half ? a + (a - c) / 2 : a + b - c, 0, 255) << shift;
        }
        return result;
    }

    private static void Insert(uint[] cache, int bits, uint pixel)
    {
        if (bits != 0) cache[unchecked(pixel * 0x1E35A7BDu) >> (32 - bits)] = pixel;
    }

    private static int Ceil(int value, int bits) => (value + (1 << bits) - 1) >> bits;

    internal static void ValidatePredictorModes(ReadOnlySpan<uint> modes, OperationContext context)
    {
        for (var start = 0; start < modes.Length;)
        {
            var count = Math.Min(1024, modes.Length - start);
            context.Work(count);
            foreach (var value in modes.Slice(start, count))
                if (((value >> 8) & 15) > 13) throw new ImageDataException("Invalid VP8L predictor mode.");
            start += count;
        }
    }

    internal static int CountEntropyGroups(ReadOnlySpan<uint> entropy, OperationContext context)
    {
        var groups = 1;
        for (var start = 0; start < entropy.Length;)
        {
            var count = Math.Min(1024, entropy.Length - start);
            context.Work(count);
            foreach (var value in entropy.Slice(start, count)) groups = Math.Max(groups, (int)((value >> 8) & 65535) + 1);
            start += count;
        }
        return groups;
    }
}
