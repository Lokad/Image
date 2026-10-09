using System.Buffers.Binary;
using System.Numerics;
using Lokad.Image.Internal;

namespace Lokad.Image.Tooling;

/// <summary>Original small VP8 syntax cases, authored without production encoder algorithms.</summary>
internal static class AuthoredVp8
{
    internal sealed record Macroblock
    {
        public int Mode { get; init; } = 4;
        public int Chroma { get; init; }
        public int Segment { get; init; }
        public bool Skip { get; init; }
        public int[] SmallModes { get; init; } = new int[16];
        // Y2, sixteen Y blocks, four U blocks, four V blocks, in natural coefficient order.
        public short[][] Coefficients { get; init; } = Enumerable.Range(0, 25).Select(_ => new short[16]).ToArray();
    }

    internal sealed record Frame(string Name, int Width, int Height, Macroblock[] Blocks)
    {
        public int TokenPartitions { get; init; } = 1;
        public bool OmitUnusedPartitions { get; init; }
        public int Quantizer { get; init; } = 20;
        public int?[] QuantizerDeltas { get; init; } = new int?[5];
        public bool Segmentation { get; init; }
        public bool UpdateSegmentMap { get; init; } = true;
        public bool UpdateSegmentData { get; init; } = true;
        public bool AbsoluteSegments { get; init; }
        public int?[] SegmentQuantizers { get; init; } = new int?[4];
        public int?[] SegmentFilters { get; init; } = new int?[4];
        public int?[] SegmentProbabilities { get; init; } = new int?[3];
        public bool SimpleFilter { get; init; }
        public int FilterLevel { get; init; }
        public int Sharpness { get; init; }
        public bool LoopDeltasEnabled { get; init; }
        public bool UpdateLoopDeltas { get; init; } = true;
        public int?[] ReferenceDeltas { get; init; } = new int?[4];
        public int?[] ModeDeltas { get; init; } = new int?[4];
        public bool SkipSyntax { get; init; } = true;
        public int SkipProbability { get; init; } = 109;
        // Sparse [flattened RFC plane/band/context/branch index, new probability] pairs.
        public int[][] ProbabilityUpdates { get; init; } = [];
    }

    // RFC 6386 sections 11 and 13 format trees. Share only the specified numeric
    // probability tables; arithmetic, token authoring and frame assembly below are owned test code.
    private static ReadOnlySpan<byte> Bands => [0, 1, 2, 3, 6, 4, 5, 6, 6, 6, 6, 6, 6, 6, 6, 7];
    private static ReadOnlySpan<byte> Zigzag => [0, 1, 4, 8, 5, 2, 3, 6, 9, 12, 13, 10, 7, 11, 14, 15];
    private static string[] SmallPaths => ["0", "10", "110", "11100", "11110", "111010", "111011", "111110", "1111110", "1111111"];
    private static string[] TokenPaths => ["10", "110", "11100", "111010", "111011", "111100", "111101", "1111100", "1111101", "1111110", "1111111", "0"];
    private static ReadOnlySpan<sbyte> SmallTree => [-1, 2, -2, 4, -3, 6, 8, 12, -4, 10, -6, -7, -5, 14, -8, 16, -9, -10];
    private static ReadOnlySpan<sbyte> TokenTree => [-12, 2, -1, 4, -2, 6, 8, 12, -3, 10, -4, -5, 14, 16, -6, -7, 18, 20, -8, -9, -10, -11];

    public static IEnumerable<Frame> Cases()
    {
        foreach (var partitions in new[] { 1, 2, 4, 8 })
            yield return Pattern($"partitions-{partitions}", 17, 129, mixed: true) with { TokenPartitions = partitions };
        foreach (var absolute in new[] { false, true })
            foreach (var simple in new[] { false, true })
                yield return Pattern($"segments-{(absolute ? "absolute" : "delta")}-{(simple ? "simple" : "normal")}", 49, 33, mixed: true) with
                {
                    Quantizer = 30, Segmentation = true, AbsoluteSegments = absolute,
                    SegmentQuantizers = [0, 15, -20, 80], SegmentFilters = [0, 12, -7, 31], SegmentProbabilities = [91, 173, 57],
                    FilterLevel = 20, SimpleFilter = simple, LoopDeltasEnabled = true,
                    ReferenceDeltas = [5, -9, 0, 63], ModeDeltas = [-3, 0, 9, -63]
                };
        foreach (var absolute in new[] { false, true })
            yield return Pattern($"segments-clamp-{(absolute ? "absolute" : "delta")}", 49, 33, mixed: true) with
            {
                Segmentation = true, AbsoluteSegments = absolute, Quantizer = 90, FilterLevel = 40,
                SegmentQuantizers = [-127, 127, null, 0], SegmentFilters = [-63, 63, null, 0]
            };
        yield return Pattern("segments-unchanged-defaults", 33, 33, mixed: true) with
        { Segmentation = true, UpdateSegmentData = false, UpdateSegmentMap = false };
        foreach (var simple in new[] { false, true })
        {
            foreach (var level in new[] { 0, 1, 14, 15, 39, 40, 63 })
                yield return Pattern($"filter-{(simple ? "simple" : "normal")}-level{level}") with { SimpleFilter = simple, FilterLevel = level };
            foreach (var sharpness in new[] { 1, 4, 5, 7 })
                yield return Pattern($"filter-{(simple ? "simple" : "normal")}-sharpness{sharpness}") with
                { SimpleFilter = simple, FilterLevel = 40, Sharpness = sharpness };
        }
        var deltaBase = Pattern("filter-deltas", mixed: true) with { FilterLevel = 20, LoopDeltasEnabled = true };
        yield return deltaBase with { Name = "filter-deltas-unchanged", UpdateLoopDeltas = false };
        yield return deltaBase with { Name = "filter-deltas-update-empty" };
        yield return deltaBase with { Name = "filter-deltas-reference", ReferenceDeltas = [11, -63, 7, 0] };
        yield return deltaBase with { Name = "filter-deltas-mode", ModeDeltas = [-9, 63, -7, 0] };
        yield return deltaBase with { Name = "filter-deltas-clamp", ReferenceDeltas = [63, null, null, null], ModeDeltas = [-63, null, null, null] };
        yield return deltaBase with { Name = "filter-deltas-zero-base", FilterLevel = 0, ReferenceDeltas = [11, null, null, null] };
        yield return deltaBase with
        {
            Name = "filter-zero-base-segment-override", FilterLevel = 0, Segmentation = true, AbsoluteSegments = true,
            SegmentFilters = [12, 24, 36, 48], ReferenceDeltas = [11, null, null, null]
        };
        yield return deltaBase with
        {
            Name = "filter-zero-segment-positive-delta", Segmentation = true, AbsoluteSegments = true,
            SegmentFilters = [0, 0, 0, 0], ReferenceDeltas = [11, null, null, null]
        };
        for (var index = 0; index < 5; index++)
        {
            var deltas = new int?[5]; deltas[index] = index % 2 == 0 ? 11 : -9;
            yield return Pattern($"quantizer-delta-{index}", mixed: true) with { Quantizer = 30, QuantizerDeltas = deltas };
        }
        yield return Pattern("quantizer-deltas-low-clamp", mixed: true) with { Quantizer = 0, QuantizerDeltas = [-15, -15, -15, -15, -15] };
        yield return Pattern("quantizer-deltas-high-clamp", mixed: true) with { Quantizer = 127, QuantizerDeltas = [15, 15, 15, 15, 15] };
        yield return Pattern("contexts-y2-and-skip", 49, 49, mixed: true);
        yield return Pattern("contexts-without-skip-syntax", 49, 49, mixed: true) with { SkipSyntax = false };
        var sparse = Pattern("sparse-categories", 33, 33);
        short[] magnitudes = [1,2,3,4,5,6,7,10,11,18,19,34,35,66,67,2047,2114];
        for (var index = 0; index < sparse.Blocks.Length; index++)
            for (var b = 0; b < 25; b++)
            {
                Array.Clear(sparse.Blocks[index].Coefficients[b]);
                var magnitude = magnitudes[(b + index) % magnitudes.Length];
                sparse.Blocks[index].Coefficients[b][Zigzag[(b + index * 5) % 16]] = (short)((b & 1) == 0 ? magnitude : -magnitude);
            }
        yield return sparse;
        yield return sparse with { Name = "sparse-categories-high-quantizer", Quantizer = 127 };
        yield return sparse with
        {
            Name = "sparse-custom-probabilities",
            ProbabilityUpdates = Enumerable.Range(0, 4).SelectMany(plane => Enumerable.Range(0, 3)
                .SelectMany(context => new[] { new[] { ((plane * 8 + 0) * 3 + context) * 11, context == 0 ? 0 : context == 1 ? 255 : 1 },
                    new[] { ((plane * 8 + 6) * 3 + context) * 11 + 3, context == 0 ? 254 : context == 1 ? 1 : 128 } })).ToArray()
        };
        var flat = Pattern("flat-dc", 1, 1) with { SkipSyntax = false };
        foreach (var block in flat.Blocks) foreach (var coefficients in block.Coefficients) Array.Clear(coefficients);
        yield return flat;
    }

    public static IEnumerable<Frame> InvalidEmptyPartitions()
    {
        var flat = Pattern("unused-empty-partitions", 1, 1) with { SkipSyntax = false, TokenPartitions = 8, OmitUnusedPartitions = true };
        foreach (var block in flat.Blocks) foreach (var coefficients in block.Coefficients) Array.Clear(coefficients);
        yield return flat with
        {
            Name = "all-skipped-empty-partitions", SkipSyntax = true,
            Blocks = flat.Blocks.Select(block => block with { Skip = true }).ToArray()
        };
        yield return flat;
    }

    private static Frame Pattern(string name, int width = 33, int height = 33, bool mixed = false)
    {
        var count = ((width + 15) / 16) * ((height + 15) / 16); var blocks = new Macroblock[count];
        for (var index = 0; index < count; index++)
        {
            var mode = mixed ? index % 5 : 4;
            var block = new Macroblock { Mode = mode, Chroma = mixed ? index % 4 : 0, Segment = index % 4, Skip = mixed && index % 7 == 1 };
            block.Coefficients[0][0] = (short)(index % 3 - 1);
            block.Coefficients[0][1] = (short)(index % 2);
            for (var b = 0; b < 24; b++)
            {
                if (mode == 4 || b >= 16) block.Coefficients[b + 1][0] = (short)((b % 4 - 1) * 4 + index % 3);
                if (b % 3 == 0) block.Coefficients[b + 1][Zigzag[b % 2 == 0 ? 1 : 15]] = (short)(index % 2 == 0 ? 1 : -2);
                if (mode == 4 && mixed && b < 16) block.SmallModes[b] = (b + index) % 10;
            }
            blocks[index] = block;
        }
        return new Frame(name, width, height, blocks) { Quantizer = 0 };
    }

    // Keep the entire endpoint in an arbitrary-precision integer. No emitted-byte
    // carry logic or production arithmetic coder is involved in these authored cases.
    private sealed class Interval
    {
        private BigInteger low;
        private int range = 255, shifts;

        public void Bit(int bit, int probability = 128)
        {
            var split = 1 + ((range - 1) * probability >> 8);
            if (bit == 0) range = split; else { low += split; range -= split; }
            while (range < 128) { range *= 2; low *= 2; shifts++; }
        }

        public void Literal(int value, int count)
        { for (var i = count - 1; i >= 0; i--) Bit(value >> i & 1); }

        public void Signed(int value, int count)
        { Literal(Math.Abs(value), count); Bit(value < 0 ? 1 : 0); }

        public void OptionalSigned(int? value, int count)
        { Bit(value.HasValue ? 1 : 0); if (value.HasValue) Signed(value.Value, count); }

        public void Path(string path, ReadOnlySpan<sbyte> tree, ReadOnlySpan<byte> probabilities, bool skipFirst = false)
        {
            var node = skipFirst ? 2 : 0;
            for (var index = skipFirst ? 1 : 0; index < path.Length; index++)
            { var bit = path[index] - '0'; Bit(bit, probabilities[node / 2]); node = tree[node + bit]; }
        }

        public byte[] Finish()
        {
            var length = (shifts + 15) / 8; var value = low << (length * 8 - shifts - 8);
            var significant = value.ToByteArray(isUnsigned: true, isBigEndian: true);
            var result = new byte[length + 2]; significant.CopyTo(result.AsSpan(length - significant.Length));
            return result;
        }
    }

    public static byte[] Build(Frame frame)
    {
        var columns = (frame.Width + 15) / 16; var rows = (frame.Height + 15) / 16;
        if (frame.Blocks.Length != columns * rows || frame.TokenPartitions is not (1 or 2 or 4 or 8)) throw new ArgumentException("Authored geometry/partitions differ.");
        var header = new Interval(); var probabilities = Vp8Tables.CoefficientProbabilities.ToArray();
        header.Bit(0); header.Bit(0); header.Bit(frame.Segmentation ? 1 : 0);
        if (frame.Segmentation)
        {
            header.Bit(frame.UpdateSegmentMap ? 1 : 0); header.Bit(frame.UpdateSegmentData ? 1 : 0);
            if (frame.UpdateSegmentData)
            {
                header.Bit(frame.AbsoluteSegments ? 1 : 0);
                foreach (var value in frame.SegmentQuantizers) header.OptionalSigned(value, 7);
                foreach (var value in frame.SegmentFilters) header.OptionalSigned(value, 6);
            }
            if (frame.UpdateSegmentMap)
                foreach (var probability in frame.SegmentProbabilities)
                { header.Bit(probability.HasValue ? 1 : 0); if (probability.HasValue) header.Literal(probability.Value, 8); }
        }
        header.Bit(frame.SimpleFilter ? 1 : 0); header.Literal(frame.FilterLevel, 6); header.Literal(frame.Sharpness, 3);
        header.Bit(frame.LoopDeltasEnabled ? 1 : 0);
        if (frame.LoopDeltasEnabled)
        {
            header.Bit(frame.UpdateLoopDeltas ? 1 : 0);
            if (frame.UpdateLoopDeltas)
            {
                foreach (var value in frame.ReferenceDeltas) header.OptionalSigned(value, 6);
                foreach (var value in frame.ModeDeltas) header.OptionalSigned(value, 6);
            }
        }
        header.Literal(frame.TokenPartitions == 1 ? 0 : frame.TokenPartitions == 2 ? 1 : frame.TokenPartitions == 4 ? 2 : 3, 2);
        header.Literal(frame.Quantizer, 7); foreach (var value in frame.QuantizerDeltas) header.OptionalSigned(value, 4);
        header.Bit(0);
        var updates = frame.ProbabilityUpdates.ToDictionary(pair => pair[0], pair => pair[1]);
        for (var i = 0; i < probabilities.Length; i++)
        {
            var update = updates.TryGetValue(i, out var probability); header.Bit(update ? 1 : 0, Vp8Tables.CoefficientUpdateProbabilities[i]);
            if (update) { probabilities[i] = (byte)probability; header.Literal(probability, 8); }
        }
        header.Bit(frame.SkipSyntax ? 1 : 0); if (frame.SkipSyntax) header.Literal(frame.SkipProbability, 8);
        var aboveModes = new int[columns * 4]; var leftModes = new int[4];
        for (var row = 0; row < rows; row++)
        {
            Array.Clear(leftModes);
            for (var column = 0; column < columns; column++)
            {
                var block = frame.Blocks[row * columns + column];
                if (frame.Segmentation && frame.UpdateSegmentMap)
                {
                    var half = block.Segment / 2; header.Bit(half, frame.SegmentProbabilities[0] ?? 255);
                    header.Bit(block.Segment % 2, frame.SegmentProbabilities[half + 1] ?? 255);
                }
                if (frame.SkipSyntax) header.Bit(block.Skip ? 1 : 0, frame.SkipProbability);
                header.Bit(block.Mode == 4 ? 0 : 1, 145);
                if (block.Mode != 4)
                {
                    header.Bit(block.Mode < 2 ? 0 : 1, 156); header.Bit(block.Mode % 2, block.Mode < 2 ? 163 : 128);
                    var smallMode = block.Mode switch { 0 => 0, 1 => 2, 2 => 3, _ => 1 };
                    Array.Fill(leftModes, smallMode); Array.Fill(aboveModes, smallMode, column * 4, 4);
                }
                else
                    for (var by = 0; by < 4; by++)
                        for (var bx = 0; bx < 4; bx++)
                        {
                            var index = column * 4 + bx; var mode = block.SmallModes[by * 4 + bx];
                            header.Path(SmallPaths[mode], SmallTree, Vp8Tables.BlockModeProbabilities.Slice((aboveModes[index] * 10 + leftModes[by]) * 9, 9));
                            aboveModes[index] = leftModes[by] = mode;
                        }
                header.Bit(block.Chroma == 0 ? 0 : 1, 142);
                if (block.Chroma != 0) { header.Bit(block.Chroma == 1 ? 0 : 1, 114); if (block.Chroma >= 2) header.Bit(block.Chroma - 2, 183); }
            }
        }
        var tokens = Enumerable.Range(0, frame.TokenPartitions).Select(_ => new Interval()).ToArray();
        var usedPartitions = new bool[tokens.Length];
        var aboveCoefficients = new byte[columns * 9]; var leftCoefficients = new byte[9];
        for (var row = 0; row < rows; row++)
        {
            Array.Clear(leftCoefficients);
            for (var column = 0; column < columns; column++)
            {
                var block = frame.Blocks[row * columns + column]; var hasY2 = block.Mode != 4; var writer = tokens[row % tokens.Length];
                var above = aboveCoefficients.AsSpan(column * 9, 9);
                if (frame.SkipSyntax && block.Skip)
                {
                    above[..8].Clear(); leftCoefficients.AsSpan(0, 8).Clear();
                    if (hasY2) above[8] = leftCoefficients[8] = 0;
                    continue;
                }
                usedPartitions[row % tokens.Length] = true;
                if (hasY2) above[8] = leftCoefficients[8] = Coefficients(writer, block.Coefficients[0], probabilities, 1, above[8] + leftCoefficients[8]);
                for (var b = 0; b < 24; b++)
                {
                    var luma = b < 16; var local = luma ? b : (b - 16) % 4; var side = luma ? 4 : 2;
                    var start = luma ? 0 : b < 20 ? 4 : 6; var a = start + local % side; var l = start + local / side;
                    above[a] = leftCoefficients[l] = Coefficients(writer, block.Coefficients[b + 1], probabilities,
                        luma ? hasY2 ? 0 : 3 : 2, above[a] + leftCoefficients[l]);
                }
            }
        }
        var first = header.Finish(); var parts = tokens.Select((writer,index) => frame.OmitUnusedPartitions && !usedPartitions[index] ? [] : writer.Finish()).ToArray();
        using var payload = new MemoryStream(); Span<byte> bytes = stackalloc byte[10];
        var tag = first.Length * 32 + 16;
        for (var i = 0; i < 3; i++) bytes[i] = (byte)(tag >> (i * 8));
        bytes[3] = 0x9D; bytes[4] = 1; bytes[5] = 0x2A;
        BinaryPrimitives.WriteUInt16LittleEndian(bytes[6..], (ushort)frame.Width); BinaryPrimitives.WriteUInt16LittleEndian(bytes[8..], (ushort)frame.Height);
        payload.Write(bytes); payload.Write(first);
        foreach (var part in parts[..^1])
        { for (var i = 0; i < 3; i++) bytes[i] = (byte)(part.Length >> (i * 8)); payload.Write(bytes[..3]); }
        foreach (var part in parts) payload.Write(part);
        return Wrap(payload.ToArray());
    }

    private static byte Coefficients(Interval writer, short[] coefficients, byte[] probabilities, int plane, int context)
    {
        var start = plane == 0 ? 1 : 0; var last = 15;
        while (last >= start && coefficients[Zigzag[last]] == 0) last--;
        var zero = false;
        for (var c = start; c <= Math.Min(last + 1, 15); c++)
        {
            var p = probabilities.AsSpan(((plane * 8 + Bands[c]) * 3 + context) * 11, 11);
            if (c > last) { writer.Path(TokenPaths[11], TokenTree, p); break; }
            var value = coefficients[Zigzag[c]]; var magnitude = Math.Abs((int)value);
            var category = magnitude < 5 ? -1 : magnitude < 7 ? 0 : magnitude < 11 ? 1 : magnitude < 19 ? 2 : magnitude < 35 ? 3 : magnitude < 67 ? 4 : 5;
            writer.Path(TokenPaths[category < 0 ? magnitude : category + 5], TokenTree, p, zero);
            if (category >= 0)
            {
                ReadOnlySpan<byte> extras = category switch
                {
                    0 => [159], 1 => [165,145], 2 => [173,148,140], 3 => [176,155,140,135],
                    4 => [180,157,141,134,130], _ => [254,254,243,230,196,177,153,140,133,130,129]
                };
                ReadOnlySpan<int> bases = [5,7,11,19,35,67]; var remainder = magnitude - bases[category];
                for (var i = 0; i < extras.Length; i++) writer.Bit(remainder >> (extras.Length - 1 - i) & 1, extras[i]);
            }
            if (magnitude != 0) writer.Bit(value < 0 ? 1 : 0);
            context = Math.Min(2, magnitude); zero = magnitude == 0;
        }
        return last < start ? (byte)0 : (byte)1;
    }

    public static byte[] Wrap(byte[] payload)
    {
        var result = new byte[20 + payload.Length + (payload.Length & 1)];
        "RIFF"u8.CopyTo(result); BinaryPrimitives.WriteInt32LittleEndian(result.AsSpan(4), result.Length - 8);
        "WEBPVP8 "u8.CopyTo(result.AsSpan(8)); BinaryPrimitives.WriteInt32LittleEndian(result.AsSpan(16), payload.Length);
        payload.CopyTo(result, 20); return result;
    }
}
