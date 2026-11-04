using System.Reflection;
using Lokad.Image.Internal;
using Lokad.Image.Tooling;

namespace Lokad.Image.Tests;

public sealed class PngDeflaterBitOracleTests
{
    private const BindingFlags Private = BindingFlags.Instance | BindingFlags.NonPublic;

    [Fact]
    public void CanonicalTokensKeepAlignmentAndBufferEdges()
    {
        // Exercise the actual token writer as a bitstream primitive. Complete
        // valid zlib/PNG packets have separate frozen/reference coverage.
        // RFC1951: canonical code bits go MSB first, extra bits LSB first.
        byte[][] trees =
        [
            [.. Enumerable.Repeat((byte)8, 226), .. Enumerable.Repeat((byte)9, 60)],
            [1, 2, 3, 4, 5, 6, .. Enumerable.Repeat((byte)14, 232), .. Enumerable.Repeat((byte)15, 48)]
        ];
        byte[] distanceLengths = [4, 4, .. Enumerable.Repeat((byte)5, 28)];
        uint[] alphabet = [0, 1, 2, 255, 3, 4, 5, 128, 7];
        using var root = new TestMemoryBudget(PngDeflater.WorkspaceBytes);
        foreach (var lengths in trees)
        foreach (var run in new[] { 0, 1, 2, 3, 4, 5, 7, 8, 17 })
        foreach (var initialBits in Enumerable.Range(0, 8))
        foreach (var position in new[] { 0, 4093, 4094, 4095 })
        {
            List<uint> input = [];
            for (var i = 0; i < run; i++) input.Add(alphabet[i % alphabet.Length]);
            input.Add(3u | 1u << 9);
            for (var i = 0; i < run; i++) input.Add(alphabet[(alphabet.Length - 1 - i % alphabet.Length)]);
            input.Add(258u | 32768u << 9); input.Add(27); input.Add(10u | 17u << 9); input.Add(11u | 5u << 9);
            var tokens = input.ToArray(); var unchanged = tokens.ToArray();
            var prefix = Enumerable.Range(0, position).Select(i => unchecked((byte)(i * 29 + 7))).ToArray();
            var pending = (1u << initialBits) - 1;
            var expected = Reference(tokens, lengths, distanceLengths, prefix, pending, initialBits);
            using var context = new OperationContext(new() { MemoryBudget = root }, default);
            using var output = new MemoryStream();
            using (var encoder = new PngDeflater(output, context, 9))
            {
                var type = typeof(PngDeflater);
                prefix.CopyTo((byte[])type.GetField("encoded", Private)!.GetValue(encoder)!);
                type.GetField("encodedCount", Private)!.SetValue(encoder, position);
                type.GetField("pending", Private)!.SetValue(encoder, pending);
                type.GetField("pendingBits", Private)!.SetValue(encoder, initialBits);
                tokens.CopyTo((uint[])type.GetField("tokens", Private)!.GetValue(encoder)!, 0);
                type.GetField("tokenCount", Private)!.SetValue(encoder, tokens.Length);
                type.GetMethod("WriteTokens", Private)!.Invoke(encoder, [lengths, distanceLengths]);
                type.GetMethod("Align", Private)!.Invoke(encoder, null); encoder.Flush();
            }
            Assert.Equal(expected, output.ToArray()); Assert.Equal(unchanged, tokens);
            Assert.Equal(0, root.Bytes); Assert.False(root.IsDisposed); Assert.True(output.CanWrite);
        }
    }

    [Fact]
    public void EveryLengthAndDistanceBucketBoundaryKeepsTheIndependentBitstream()
    {
        // Enumerate the RFC distance ranges independently by their doubling
        // widths, rather than the encoder's logarithm-based classification.
        var boundaries = new HashSet<int>(); var start = 1; var width = 1;
        for (var code = 0; code < 30; code++)
        {
            if (code >= 4 && (code & 1) == 0) width *= 2;
            boundaries.Add(start); boundaries.Add(start + width - 1); start += width;
        }
        Assert.Equal(32769, start);
        var input = (from length in Enumerable.Range(3, 256)
                     from distance in boundaries.Order()
                     select (uint)length | (uint)distance << 9).ToArray();
        byte[] lengths = [.. Enumerable.Repeat((byte)8, 226), .. Enumerable.Repeat((byte)9, 60)];
        byte[] distances = [4, 4, .. Enumerable.Repeat((byte)5, 28)];
        var expected = Reference(input, lengths, distances, [], 0, 0);
        using var output = new MemoryStream();
        using var context = new OperationContext(ImageLimits.Default, default);
        using (var encoder = new PngDeflater(output, context, 9))
        {
            var owned = (uint[])typeof(PngDeflater).GetField("tokens", Private)!.GetValue(encoder)!;
            Assert.True(input.Length <= owned.Length);
            input.CopyTo(owned, 0);
            typeof(PngDeflater).GetField("tokenCount", Private)!.SetValue(encoder, input.Length);
            typeof(PngDeflater).GetMethod("WriteTokens", Private)!.Invoke(encoder, [lengths, distances]);
            typeof(PngDeflater).GetMethod("Align", Private)!.Invoke(encoder, null); encoder.Flush();
        }
        Assert.Equal(expected, output.ToArray());
    }

    private static byte[] Reference(uint[] tokens, byte[] lengths, byte[] distances,
        byte[] prefix, uint pending, int initialBits)
    {
        static int[] Canonical(byte[] widths)
        {
            var counts = new int[16]; foreach (var width in widths) counts[width]++;
            counts[0] = 0; var next = new int[16];
            for (var width = 1; width < 16; width++) next[width] = (next[width - 1] + counts[width - 1]) << 1;
            return widths.Select(width => width == 0 ? 0 : next[width]++).ToArray();
        }
        var codes = Canonical(lengths); var distanceCodes = Canonical(distances);
        using var output = new MemoryStream(); output.Write(prefix); var current = (byte)pending; var used = initialBits;
        void Bit(int bit) { current |= (byte)(bit << used); if (++used == 8) { output.WriteByte(current); current = 0; used = 0; } }
        void Symbol(int code, int width) { for (var b = width - 1; b >= 0; b--) Bit(code >> b & 1); }
        void Extra(int value, int width) { for (var b = 0; b < width; b++) Bit(value >> b & 1); }
        foreach (var token in tokens)
        {
            var distance = (int)(token >> 9);
            if (distance == 0) { Symbol(codes[(int)token], lengths[(int)token]); continue; }
            var length = (int)(token & 511);
            var ls = 257;
            for (; ls < 285; ls++)
            {
                var bits = ls < 265 ? 0 : (ls - 261) / 4;
                var start = ls < 265 ? ls - 254 : 3 + ((4 + (ls - 265) % 4) << bits);
                if (length < 258 && length >= start && length < start + (1 << bits)) break;
            }
            var le = ls is >= 265 and < 285 ? (ls - 261) / 4 : 0;
            var lb = ls < 265 ? ls - 254 : ls == 285 ? 258 : 3 + ((4 + (ls - 265) % 4) << le);
            Symbol(codes[ls], lengths[ls]); Extra(length - lb, le);
            for (var ds = 0; ds < 30; ds++)
            {
                var bits = ds < 4 ? 0 : ds / 2 - 1;
                var start = ds < 4 ? ds + 1 : 1 + ((2 + ds % 2) << bits);
                if (distance < start || distance >= start + (1 << bits)) continue;
                Symbol(distanceCodes[ds], distances[ds]); Extra(distance - start, bits); break;
            }
        }
        Symbol(codes[256], lengths[256]); if (used != 0) output.WriteByte(current); return output.ToArray();
    }
}
