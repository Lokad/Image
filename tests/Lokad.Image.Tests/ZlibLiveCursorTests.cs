using Lokad.Image.Internal;
using Lokad.Image.Tooling;

namespace Lokad.Image.Tests
{
    public sealed class ZlibLiveCursorTests
    {
        [Fact]
        public void LiveFieldsPreserveEveryTailPhaseAndCanceledPrefix()
            => ZlibFraming.CheckLiveCursorFields();

        [Fact]
        public void LiveBlocksKeepFollowingBitsAndCancelBeforeTouchingTheNextBlock()
            => ZlibFraming.CheckLiveCursorFollowingBits();
    }

    public sealed partial class ZlibMaterializationTests
    {
        [Theory]
        [InlineData(false)] [InlineData(true)]
        public void FailedFramingReleasesAdmissionBeforeTheNextIndependentReader(bool cancelable)
        {
            var raw = Enumerable.Range(0, 128).Select(i => (byte)i).ToArray();
            var bits = new Writer(); bits.Number(1, 1); bits.Number(1, 2);
            foreach (var value in raw) bits.Literal(value);
            bits.Code(0, 7); var encoded = Envelope(bits, raw);
            var saved = encoded.ToArray();
            using var cancellation = new CancellationTokenSource();
            using var root = new TestMemoryBudget(4096);
            using var context = new OperationContext(new() { MaxWorkingBytes = 4096, MemoryBudget = root }, cancelable ? cancellation.Token : default);
            var partial = Enumerable.Repeat((byte)165, raw.Length).ToArray();
            Assert.Throws<ImageDataException>(() => { ZlibFraming.ValidateAndInflate(encoded.AsSpan(0, 8), partial, context, out _); });
            Assert.Equal(0, partial[0]); Assert.Contains((byte)165, partial);
            Assert.Equal(0, root.Bytes); Assert.Equal(4096, context.RemainingWorkingBytes);
            var complete = new byte[raw.Length];
            Assert.Equal(encoded.Length, ZlibFraming.ValidateAndInflate(encoded, complete, context, out var trees));
            Assert.True(trees); Assert.Equal(raw, complete); Assert.Equal(saved, encoded);
            Assert.Equal(0, root.Bytes); Assert.Equal(4096, root.Peak); Assert.False(root.IsDisposed);
        }
    }
}

namespace Lokad.Image.Internal
{
    internal static partial class ZlibFraming
    {
        internal static void CheckLiveCursorFields() => Bits.CheckLiveFields();

        private ref partial struct Bits
        {
            internal static void CheckLiveFields()
            {
                var input = Enumerable.Range(0, 13).Select(i => (byte)(73 * i + 19)).ToArray();
                using var root = new TestMemoryBudget(0);
                for (var cut = 0; cut <= input.Length; cut++)
                for (var phase = 0; phase < 8; phase++)
                for (var width = 0; width <= 16; width++)
                foreach (var cancel in new[] { false, true })
                {
                    using var cancellation = new CancellationTokenSource();
                    using var context = new OperationContext(new() { MemoryBudget = root }, cancellation.Token);
                    var actual = new Bits(input.AsSpan(0, cut), context);
                    var scalar = new Bits(input.AsSpan(0, cut), context);
                    Exception? firstError = null, secondError = null;
                    try { actual.Read(phase); } catch (ImageDataException error) { firstError = error; }
                    try { scalar.Read(phase); } catch (ImageDataException error) { secondError = error; }
                    Assert.Equal(secondError?.Message, firstError?.Message);
                    if (firstError is not null) continue;
                    if (cancel) cancellation.Cancel();
                    int first = -1, second = -1;
                    try { first = ReadLiveCursor(actual.data, cancellation.Token, ref actual.buffer, ref actual.available, ref actual.position, width); }
                    catch (Exception error) when (error is ImageDataException or OperationCanceledException) { firstError = error; }
                    try { second = scalar.Read(width); }
                    catch (Exception error) when (error is ImageDataException or OperationCanceledException) { secondError = error; }
                    Assert.Equal(secondError?.GetType(), firstError?.GetType());
                    Assert.Equal(secondError?.Message, firstError?.Message); Assert.Equal(second, first);
                    Assert.Equal(scalar.PreparedPrefixTestState, actual.PreparedPrefixTestState);
                    if (firstError is OperationCanceledException canceled) Assert.Equal(cancellation.Token, canceled.CancellationToken);
                }
                Assert.Equal(0, root.Bytes); Assert.Equal(0, root.GrowthAttempts); Assert.False(root.IsDisposed);
            }
        }

        internal static void CheckLiveCursorFollowingBits()
        {
            using var cancellation = new CancellationTokenSource();
            using var root = new TestMemoryBudget(0);
            using var context = new OperationContext(new() { MemoryBudget = root }, cancellation.Token);
            Span<byte> lengths = stackalloc byte[257]; lengths.Fill(9);
            Span<int> counts = stackalloc int[16], symbols = stackalloc int[288];
            Span<int> distanceCounts = stackalloc int[16], distanceSymbols = stackalloc int[32];
            Span<ushort> lookup = stackalloc ushort[1024], distanceLookup = stackalloc ushort[32];
            var literals = new Huffman(lengths, counts, symbols, context);
            var distances = new Huffman([1], distanceCounts, distanceSymbols, context);
            BuildLiteralLookup(literals, lookup, context); BuildDistanceLookup(distances, distanceLookup, context);
            var input = new byte[32]; var position = 3;
            void WriteCode(int code, int width)
            {
                for (var bit = width - 1; bit >= 0; bit--, position++)
                    input[position / 8] |= (byte)(((code >> bit) & 1) << (position & 7));
            }
            WriteCode(65, 9); WriteCode(256, 9); WriteCode(0x12345678, 32);
            var saved = input.ToArray(); var actual = new Bits(input, context); var scalar = new Bits(input, context);
            actual.Read(3); scalar.Read(3);
            var output = Enumerable.Repeat((byte)165, 8).ToArray();
            Assert.Equal(1, actual.InflateLiveCursor(literals, distances, lookup, distanceLookup, output, 0, 32768));
            Assert.Equal(65, DecodeLiteral(ref scalar, literals, lookup));
            Assert.Equal(256, DecodeLiteral(ref scalar, literals, lookup));
            Assert.Equal(65, output[0]); Assert.All(output[1..], value => Assert.Equal(165, value));
            Assert.Equal(scalar.PreparedPrefixTestState, actual.PreparedPrefixTestState);
            Assert.Equal(scalar.Read(13), actual.Read(13));
            var before = actual.PreparedPrefixTestState; cancellation.Cancel();
            try
            {
                actual.InflateLiveCursor(literals, distances, lookup, distanceLookup, output, 1, 32768);
                throw new Exception("Canceled block was accepted.");
            }
            catch (OperationCanceledException error) { Assert.Equal(cancellation.Token, error.CancellationToken); }
            Assert.Equal(before, actual.PreparedPrefixTestState); Assert.Equal(saved, input);
            Assert.Equal(65, output[0]); Assert.All(output[1..], value => Assert.Equal(165, value));
            Assert.Equal(0, root.Bytes); Assert.Equal(0, root.GrowthAttempts); Assert.False(root.IsDisposed);
        }
    }
}
