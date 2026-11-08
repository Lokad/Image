using Lokad.Image.Internal;
using Lokad.Image.Tooling;

namespace Lokad.Image.Tests
{
    public sealed class ZlibPreparedPrefixTests
    {
        [Fact]
        public void PreparedTenthBitKeepsFullLookaheadPartialOutputAndCursor()
            => ZlibFraming.CheckPreparedLiteralPrefixes();
    }
}

namespace Lokad.Image.Internal
{
    // The internal test assembly embeds production. Exercise its actual private
    // prepared/cursor kernels against the still-used general canonical decoder.
    internal static partial class ZlibFraming
    {
        internal static void CheckPreparedLiteralPrefixes()
        {
            using var root = new TestMemoryBudget(0);
            using var cancellation = new CancellationTokenSource();
            Span<byte> lengths = stackalloc byte[257]; lengths.Fill(15); lengths[0] = 10;
            Span<int> counts = stackalloc int[16], symbols = stackalloc int[288];
            Span<int> distanceCounts = stackalloc int[16], distanceSymbols = stackalloc int[32];
            Span<ushort> lookup = stackalloc ushort[1024], distanceLookup = stackalloc ushort[32];
            foreach (var live in new[] { false, true })
            {
                using var context = new OperationContext(new() { MemoryBudget = root }, live ? cancellation.Token : default);
                var literals = new Huffman(lengths, counts, symbols, context);
                var distances = new Huffman([0], distanceCounts, distanceSymbols, context);
                BuildLiteralLookup(literals, lookup, context); BuildDistanceLookup(distances, distanceLookup, context);
                // Incomplete canonical tree: symbol0 has ten-bit code0; symbols
                // 1..256 have fifteen-bit codes32..287. All-one prefix is invalid.
                foreach (var (code, bits) in new[] { (0, 10), (32, 15), (287, 15), (32767, 15) })
                for (var phase = 0; phase < 8; phase++)
                {
                    var input = new byte[(phase + bits + 15 + 32 + 7) / 8]; var position = phase;
                    void WriteCode(int value, int width)
                    {
                        for (var bit = width - 1; bit >= 0; bit--, position++)
                            input[position / 8] |= (byte)(((value >> bit) & 1) << (position & 7));
                    }
                    WriteCode(code, bits); WriteCode(287, 15); WriteCode(0x12345678, 32);
                    var saved = input.ToArray();
                    for (var cut = 0; cut <= input.Length; cut++)
                    foreach (var maximum in new[] { 0, 1 })
                    foreach (var cursor in new[] { false, true })
                    {
                        var actual = new Bits(input.AsSpan(0, cut), context);
                        var reference = new Bits(input.AsSpan(0, cut), context);
                        var actualOutput = new byte[maximum]; var referenceOutput = new byte[maximum];
                        var actualCount = 0; var referenceCount = 0; var actualEnded = false; var referenceEnded = false;
                        Exception? actualError = null, referenceError = null;
                        try
                        {
                            actual.Read(phase);
                            if (cursor)
                            {
                                if (live)
                                {
                                    actualCount = actual.InflateLiveCursor(literals, distances, lookup, distanceLookup,
                                        actualOutput, 0, 32768);
                                    actualEnded = true;
                                }
                                else actualCount = actual.InflateCursor(literals, distances, lookup, distanceLookup,
                                    actualOutput, 0, 32768, maximum, int.MaxValue, out actualEnded);
                            }
                            else actualCount = DecodeLiteral(ref actual, literals, lookup);
                            actual.Read(3);
                        }
                        catch (ImageDataException error) { actualError = error; }
                        try
                        {
                            reference.Read(phase);
                            do
                            {
                                // The nine-bit root misses every assigned code.
                                // Its original full-depth lookahead must precede
                                // consumption even when just ten bits would fit.
                                reference.Peek(9);
                                var symbol = Decode(ref reference, literals.Counts, literals.Symbols, literals.Maximum);
                                if (!cursor) { referenceCount = symbol; break; }
                                if (symbol == 256) { referenceEnded = true; break; }
                                if (referenceCount == maximum) throw new ImageDataException("PNG contains excess decompressed pixels.");
                                referenceOutput[referenceCount++] = (byte)symbol;
                            } while (true);
                            reference.Read(3);
                        }
                        catch (ImageDataException error) { referenceError = error; }
                        Assert.Equal(referenceError?.Message, actualError?.Message);
                        if (actualError is null) { Assert.Equal(referenceCount, actualCount); Assert.Equal(referenceEnded, actualEnded); }
                        Assert.Equal(referenceOutput, actualOutput);
                        // Walk/WalkRows discard any failed private cursor. Public
                        // errors and partial stores remain exact. Completed blocks,
                        // including a failed following Read, keep exact cached state.
                        if (actualError is null || !cursor || actualEnded)
                            Assert.Equal(reference.PreparedPrefixTestState, actual.PreparedPrefixTestState);
                    }
                    Assert.Equal(saved, input);
                }
            }
            Assert.Equal(0, root.Bytes); Assert.Equal(0, root.GrowthAttempts); Assert.False(root.IsDisposed);
        }

        private ref partial struct Bits
        {
            internal readonly (int, int, ulong) PreparedPrefixTestState => (position, available, buffer);
        }
    }
}
