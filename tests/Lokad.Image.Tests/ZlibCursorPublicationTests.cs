using Lokad.Image.Internal;
using Lokad.Image.Tooling;

namespace Lokad.Image.Tests
{
    public sealed class ZlibCursorPublicationTests
    {
        [Fact]
        public void OrdinarySlabStopsPublishExactStateBeforeFollowingBlocksAndReads()
            => ZlibFraming.CheckOrdinaryCursorPublication();
    }
}

namespace Lokad.Image.Internal
{
    internal static partial class ZlibFraming
    {
        internal static void CheckOrdinaryCursorPublication()
        {
            using var root = new TestMemoryBudget(0);
            using var context = new OperationContext(new() { MemoryBudget = root }, default);
            Span<byte> lengths = stackalloc byte[288]; lengths.Fill(8);
            lengths.Slice(144, 112).Fill(9); lengths.Slice(256, 24).Fill(7);
            Span<byte> distanceLengths = stackalloc byte[32]; distanceLengths.Fill(5);
            Span<int> counts = stackalloc int[16], symbols = stackalloc int[288];
            Span<int> distanceCounts = stackalloc int[16], distanceSymbols = stackalloc int[32];
            Span<ushort> lookup = stackalloc ushort[1024], distanceLookup = stackalloc ushort[32];
            var literals = new Huffman(lengths, counts, symbols, context);
            var distances = new Huffman(distanceLengths, distanceCounts, distanceSymbols, context);
            BuildLiteralLookup(literals, lookup, context); BuildDistanceLookup(distances, distanceLookup, context);
            var raw = Enumerable.Range(0, 1027).Select(i => (byte)(73 * i + 19)).ToArray();
            for (var phase = 0; phase < 8; phase++)
            {
                var input = new byte[2048]; var position = phase;
                void Code(int value, int width)
                {
                    for (var bit = width - 1; bit >= 0; bit--, position++)
                        input[position / 8] |= (byte)(((value >> bit) & 1) << (position & 7));
                }
                foreach (var value in raw) Code(value < 144 ? value + 48 : value + 256, value < 144 ? 8 : 9);
                Code(0, 7); // Fixed EOB, followed by unrelated caller bits.
                for (var bit = 0; bit < 19; bit++, position++)
                    input[position / 8] |= (byte)(((0x12345 >> bit) & 1) << (position & 7));
                var saved = input.ToArray();
                var actual = new Bits(input, context); var reference = new Bits(input, context);
                actual.Read(phase); reference.Read(phase);
                var actualOutput = Enumerable.Repeat((byte)165, raw.Length + 16).ToArray();
                var expectedOutput = actualOutput.ToArray();
                var written = 0; var expectedCount = 0; var expectedEnd = false;
                foreach (var stop in new[] { 1, 17, 65, raw.Length, int.MaxValue })
                {
                    written = actual.InflateCursor(literals, distances, lookup, distanceLookup,
                        actualOutput.AsSpan(8, raw.Length), written, 32768, raw.Length, stop, out var ended);
                    while (expectedCount < stop)
                    {
                        reference.Peek(9);
                        var symbol = Decode(ref reference, literals.Counts, literals.Symbols, literals.Maximum);
                        if (symbol == 256) { expectedEnd = true; break; }
                        expectedOutput[8 + expectedCount++] = (byte)symbol;
                    }
                    Assert.Equal(expectedCount, written); Assert.Equal(expectedEnd, ended);
                    Assert.Equal(expectedOutput, actualOutput);
                    Assert.Equal(reference.PreparedPrefixTestState, actual.PreparedPrefixTestState);
                }
                Assert.Equal(raw, actualOutput.AsSpan(8, raw.Length).ToArray());
                Assert.Equal(0x12345, actual.Read(19)); Assert.Equal(0x12345, reference.Read(19));
                Assert.Equal(reference.PreparedPrefixTestState, actual.PreparedPrefixTestState);
                Assert.Equal(saved, input);
            }
            Assert.Equal(0, root.Bytes); Assert.Equal(0, root.GrowthAttempts); Assert.False(root.IsDisposed);
        }
    }
}
