using System.Buffers.Binary;
using Lokad.Image.Internal;
using Lokad.Image.Tooling;

namespace Lokad.Image.Tests
{
    public sealed class ZlibCachedLiteralRunTests
    {
        [Fact]
        public void LiteralRunsMatchFixedCodeOracleTailsAndCompletedState()
            => ZlibFraming.CheckCachedFixedLiteralRuns();

        [Fact]
        public void PreCanceledRunsLeaveBorrowedStorageAndCursorUntouched()
            => ZlibFraming.CheckCachedLiteralCancellation();

        [Fact]
        public void LiteralRunsAllocateAndChargeNoAdditionalStorage()
            => ZlibFraming.CheckCachedLiteralAllocation();
    }
}

namespace Lokad.Image.Internal
{
    internal static partial class ZlibFraming
    {
        private static void FixedLiteralTestTable(Span<byte> lengths)
        {
            // RFC1951 section 3.2.6 fixed literal/length alphabet.
            for (var i = 0; i < 288; i++) lengths[i] = (byte)(i < 144 ? 8 : i < 256 ? 9 : i < 280 ? 7 : 8);
        }

        internal static void CheckCachedFixedLiteralRuns()
        {
            using var root = new TestMemoryBudget(0); using var cancellation = new CancellationTokenSource();
            using var context = new OperationContext(new() { MemoryBudget = root }, cancellation.Token);
            Span<byte> lengths = stackalloc byte[288]; FixedLiteralTestTable(lengths);
            Span<int> counts = stackalloc int[16], symbols = stackalloc int[288];
            Span<ushort> lookup = stackalloc ushort[1024];
            var tree = new Huffman(lengths, counts, symbols, context); BuildLiteralLookup(tree, lookup, context);
            Span<byte> input = stackalloc byte[8], actualOutput = stackalloc byte[12], expectedOutput = stackalloc byte[12];
            foreach (var phase in new[] { 0, 1, 7 })
            for (var prefix = 0; prefix < 1024; prefix++)
            {
                BinaryPrimitives.WriteUInt64LittleEndian(input, 0x0c31d76180c00c00UL | (uint)prefix);
                for (var cut = 0; cut <= input.Length; cut++)
                for (var maximum = 0; maximum <= 5; maximum++)
                {
                    if (cut * 8 < phase) continue;
                    var actual = new Bits(input[..cut], context); actual.Read(phase);
                    var (position, available, cached) = actual.CachedLiteralTestState;
                    actualOutput.Fill(0xA5); expectedOutput.Fill(0xA5);
                    var copied = actual.CopyStreamingLiteralRun(lookup, actualOutput.Slice(3, maximum));
                    var expected = 0;
                    while (expected < maximum)
                    {
                        if (available < 9)
                        {
                            // The optional run yields short tails to the full
                            // dispatcher; it never reads or throws on that tail.
                            if (cut - position < 4) break;
                            cached |= (ulong)BinaryPrimitives.ReadUInt32LittleEndian(input.Slice(position, 4)) << available;
                            position += 4; available += 32;
                        }
                        // Independent fixed-code ranges, without the production
                        // lookup or canonical decoder. Bits arrive least first.
                        static int Reverse(ulong value, int n)
                        {
                            var result = 0;
                            for (var i = 0; i < n; i++) result = result * 2 + (int)((value >> i) & 1);
                            return result;
                        }
                        var code = Reverse(cached, 8); int value, width;
                        if (code is >= 0x30 and <= 0xBF) { value = code - 0x30; width = 8; }
                        else
                        {
                            code = Reverse(cached, 9);
                            if (code < 0x190) break; // EOB or length prefix.
                            value = code - 0x190 + 144; width = 9;
                        }
                        expectedOutput[3 + expected++] = (byte)value;
                        cached >>= width; available -= width;
                    }
                    Assert.Equal(expected, copied);
                    Assert.True(expectedOutput.SequenceEqual(actualOutput));
                    Assert.Equal((position, available, cached), actual.CachedLiteralTestState);
                }
            }
            Assert.Equal(0, root.Bytes); Assert.Equal(0, root.Attempts); Assert.False(root.IsDisposed);
        }

        internal static void CheckCachedLiteralCancellation()
        {
            using var root = new TestMemoryBudget(0); using var cancellation = new CancellationTokenSource();
            using var context = new OperationContext(new() { MemoryBudget = root }, cancellation.Token);
            Span<byte> input = stackalloc byte[8]; input.Fill(0x0c);
            Span<byte> output = stackalloc byte[8]; output.Fill(0xA5);
            Span<ushort> lookup = stackalloc ushort[1024]; lookup.Fill(8);
            var actual = new Bits(input, context); actual.Read(1);
            var before = actual.CachedLiteralTestState; cancellation.Cancel();
            Assert.Equal(0, actual.CopyStreamingLiteralRun(lookup, output));
            Assert.Equal(before, actual.CachedLiteralTestState);
            foreach (var value in output) Assert.Equal(0xA5, value);
            Assert.Equal(0, root.Bytes); Assert.Equal(0, root.Attempts); Assert.False(root.IsDisposed);
        }

        internal static void CheckCachedLiteralAllocation()
        {
            using var root = new TestMemoryBudget(0); using var cancellation = new CancellationTokenSource();
            using var context = new OperationContext(new() { MemoryBudget = root }, cancellation.Token);
            Span<byte> input = stackalloc byte[16]; input.Fill(0x0c);
            Span<byte> output = stackalloc byte[8]; Span<ushort> lookup = stackalloc ushort[1024]; lookup.Fill(8);
            var warm = new Bits(input, context); Assert.Equal(8, warm.CopyStreamingLiteralRun(lookup, output));
            var allocated = GC.GetAllocatedBytesForCurrentThread();
            for (var i = 0; i < 16; i++)
            {
                var actual = new Bits(input, context);
                if (actual.CopyStreamingLiteralRun(lookup, output) != 8) throw new Exception("Literal allocation control did not fill output.");
            }
            Assert.Equal(allocated, GC.GetAllocatedBytesForCurrentThread());
            Assert.Equal(0, root.Bytes); Assert.Equal(0, root.Attempts); Assert.False(root.IsDisposed);
        }

        private ref partial struct Bits
        {
            internal readonly (int, int, ulong) CachedLiteralTestState => (position, available, buffer);
        }
    }
}
