using System.Reflection;
using Lokad.Image.Internal;

namespace Lokad.Image.Tests;

public sealed class Vp8BooleanReaderTests
{
    private static readonly FieldInfo[] Fields = new[] { "position", "end", "shift", "range", "value" }
        .Select(name => typeof(Vp8BooleanReader).GetField(name, BindingFlags.Instance | BindingFlags.NonPublic)!).ToArray();

    // Observe the full arithmetic cursor, including partially advanced failures.
    // No diagnostic API or unsafe access is added to production.
    private static int[] State(Vp8BooleanReader reader) => Fields.Select(field => (int)field.GetValue(reader)!).ToArray();
    private static Vp8BooleanReader Reader(byte[] partition, int range, int value, int shift)
    {
        object reader = new Vp8BooleanReader(partition, 0, partition.Length);
        Fields[2].SetValue(reader, shift); Fields[3].SetValue(reader, range); Fields[4].SetValue(reader, value);
        return (Vp8BooleanReader)reader;
    }
    private static void Equal(Exception? expected, Exception? actual, int? expectedBit, int? actualBit,
        Scalar scalar, Vp8BooleanReader reader, OperationContext referenceWork, OperationContext actualWork)
    {
        Assert.Equal(expected?.GetType(), actual?.GetType());
        if (expected is not null) Assert.Equal(expected.Message, actual!.Message);
        Assert.Equal(expectedBit, actualBit); Assert.Equal(scalar.State, State(reader));
        Assert.Equal(referenceWork.RemainingWork, actualWork.RemainingWork);
    }
    private static void Read(byte[] partition, byte[] data, int range, int value, int shift, int probability)
    {
        var reader = Reader(partition, range, value, shift); var scalar = new Scalar(partition.Length, range, value, shift);
        var actualWork = new OperationContext(new ImageLimits { MaxWork = 1 }, default);
        var referenceWork = new OperationContext(new ImageLimits { MaxWork = 1 }, default);
        int? expectedBit = null, actualBit = null;
        var expected = Record.Exception(() => expectedBit = scalar.Read(data, probability, referenceWork));
        var actual = Record.Exception(() => actualBit = reader.Read(data, probability, actualWork));
        Equal(expected, actual, expectedBit, actualBit, scalar, reader, referenceWork, actualWork);
        Assert.Equal(0, actualWork.RemainingWork);
    }

    [Theory]
    [InlineData(1)] [InlineData(2)] [InlineData(3)] [InlineData(4)]
    [InlineData(5)] [InlineData(6)] [InlineData(7)]
    public void EveryNormalizationWidthPreservesBothIntervalsAndPartialByteCursor(int bits)
    {
        var target = 1 << (7 - bits);
        foreach (var right in new[] { false, true })
        {
            var boundary = right ? 255 - target : target;
            var probability = ((boundary - 1) * 256 + 253) / 254;
            Assert.Equal(boundary, 1 + (254 * probability >> 8));
            foreach (var value in right ? new[] { boundary, 254 } : new[] { 0, boundary - 1 })
                for (var shift = 0; shift < 8; shift++)
                    foreach (var next in new byte[] { 0, 1, 0x55, 0x80, 0xaa, 0xfe, 0xff })
                        for (var length = 1; length <= 3; length++)
                        {
                            byte[] bytes = [0, next, (byte)(next ^ 255)]; var partition = bytes[..length];
                            Read(partition, partition, 255, value, shift, probability);
                        }
        }
    }

    [Fact]
    public void ProbabilityExtremesAndNormalizedIntervalBoundariesMatchScalarRecurrence()
    {
        byte[] data = [0, 0x53, 0xad];
        for (var range = 128; range <= 255; range++)
            foreach (var value in new[] { 0, (range - 1) / 2, range - 1, range })
                foreach (var probability in new[] { -1, 0, 1, 2, 63, 127, 128, 129, 253, 254, 255, 256 })
                    for (var shift = 0; shift < 8; shift++) Read(data, data, range, value, shift, probability);
        foreach (var probability in Enumerable.Range(0, 256))
            foreach (var range in new[] { 128, 191, 255 })
                foreach (var value in new[] { 0, range - 1 }) Read(data, data, range, value, 3, probability);
    }

    [Fact]
    public void ShorterReadSpanPreservesScalarIndexFailureAndPartiallyAdvancedRange()
    {
        byte[] partition = [0, 0xa5, 0x5a];
        for (var length = 0; length < partition.Length; length++)
            for (var shift = 0; shift < 8; shift++)
                foreach (var probability in new[] { 0, 1, 128, 254, 255 })
                    foreach (var value in new[] { 0, 254 })
                        Read(partition, partition[..length], 255, value, shift, probability);
    }

    [Theory]
    [InlineData(false)] [InlineData(true)]
    public void WorkAndCancellationRejectionPrecedeValidationAndLeaveCursorUnchanged(bool cancelled)
    {
        foreach (var probability in new[] { -1, 0, 128, 255, 256 })
            foreach (var value in new[] { 0, 255 })
            {
                using var cancellation = new CancellationTokenSource();
                byte[] data = [0]; var reader = Reader(data, 255, value, 0); var before = State(reader);
                var work = new OperationContext(new ImageLimits { MaxWork = 1 }, cancelled ? cancellation.Token : default);
                if (cancelled) cancellation.Cancel(); else work.Work(1);
                var remaining = work.RemainingWork;
                var error = Record.Exception(() => reader.Read([], probability, work));
                if (cancelled) Assert.IsType<OperationCanceledException>(error); else Assert.IsType<ImageLimitException>(error);
                Assert.Equal(before, State(reader)); Assert.Equal(remaining, work.RemainingWork);
            }
    }

    [Theory]
    [InlineData(-1)] [InlineData(0)] [InlineData(1)] [InlineData(7)] [InlineData(8)]
    [InlineData(9)] [InlineData(16)] [InlineData(24)] [InlineData(25)]
    public void LiteralValuesWorkAndTruncationMatchOriginalRecurrence(int bits)
    {
        byte[] bytes = [192, 0x53, 0xad, 0xf0, 0x17, 0x80, 0x5a, 0];
        foreach (var length in new[] { 1, 2, bytes.Length })
            for (var budget = 1; budget <= 25; budget++)
            {
                var data = bytes[..length]; var reader = new Vp8BooleanReader(data, 0, length);
                var scalar = new Scalar(length, 255, data[0], 7);
                var actualWork = new OperationContext(new ImageLimits { MaxWork = budget }, default);
                var referenceWork = new OperationContext(new ImageLimits { MaxWork = budget }, default);
                int? expectedValue = null, actualValue = null;
                var expected = Record.Exception(() => expectedValue = scalar.Literal(data, bits, referenceWork));
                var actual = Record.Exception(() => actualValue = reader.Literal(data, bits, actualWork));
                Equal(expected, actual, expectedValue, actualValue, scalar, reader, referenceWork, actualWork);
            }
    }

    [Fact]
    public void TreeLeavesPartialWorkAndMalformedTraversalMatchOriginalRecurrence()
    {
        byte[] bytes = [192, 0x53, 0xad];
        foreach (var tree in new sbyte[][] { [2, 4, -1, -2, -3, -4], [2, 4] })
            foreach (var probabilities in new byte[][] { [128, 128, 128], [0, 255, 1], [255, 0, 254], [128] })
                foreach (var length in new[] { 1, bytes.Length })
                    foreach (var budget in new[] { 1, 2, 3 })
                    {
                        var data = bytes[..length]; var reader = new Vp8BooleanReader(data, 0, length);
                        var scalar = new Scalar(length, 255, data[0], 7);
                        var actualWork = new OperationContext(new ImageLimits { MaxWork = budget }, default);
                        var referenceWork = new OperationContext(new ImageLimits { MaxWork = budget }, default);
                        int? expectedValue = null, actualValue = null;
                        var expected = Record.Exception(() => expectedValue = scalar.Tree(data, tree, probabilities, referenceWork));
                        var actual = Record.Exception(() => actualValue = reader.Tree(data, tree, probabilities, actualWork));
                        Equal(expected, actual, expectedValue, actualValue, scalar, reader, referenceWork, actualWork);
                    }
    }

    // RFC 6386 section 13.2: retained tree traversal is the reference for the
    // fixed coefficient-token decision primitive, including its skipped EOB.
    private static readonly sbyte[] CoefficientTree = [-12, 2, -1, 4, -2, 6, 8, 12, -3, 10, -4, -5, 14, 16, -6, -7, 18, 20, -8, -9, -10, -11];
    private static int CoefficientToken(ref Vp8BooleanReader reader, byte[] data, byte[] probabilities,
        OperationContext context, bool previousZero) => reader.CoefficientToken(data, probabilities, context, previousZero);

    private static int? CompareCoefficientToken(byte[] partition, byte[] data, byte[] probabilities,
        int range, int value, int shift, long budget, bool previousZero, bool cancelled = false)
    {
        using var cancellation = new CancellationTokenSource();
        var reader = Reader(partition, range, value, shift);
        var scalar = new Scalar(partition.Length, range, value, shift);
        var actualWork = new OperationContext(new ImageLimits { MaxWork = Math.Max(1, budget) }, cancellation.Token);
        var referenceWork = new OperationContext(new ImageLimits { MaxWork = Math.Max(1, budget) }, cancellation.Token);
        if (budget == 0) { actualWork.Work(1); referenceWork.Work(1); }
        if (cancelled) cancellation.Cancel();
        int? expectedToken = null, actualToken = null;
        var expected = Record.Exception(() => expectedToken = scalar.Tree(data, CoefficientTree, probabilities, referenceWork, previousZero ? 2 : 0));
        var actual = Record.Exception(() => actualToken = CoefficientToken(ref reader, data, probabilities, actualWork, previousZero));
        Equal(expected, actual, expectedToken, actualToken, scalar, reader, referenceWork, actualWork);
        return expected is null ? expectedToken : null;
    }

    [Theory]
    [InlineData(false)] [InlineData(true)]
    public void CoefficientTokensCoverEveryLeafAndPreserveEachPartialWorkBudget(bool previousZero)
    {
        var leaves = new HashSet<int>(); var probabilities = Enumerable.Repeat((byte)128, 11).ToArray();
        for (var value = 0; value < 255; value++)
            for (var budget = 1; budget <= 8; budget++)
            {
                byte[] data = [(byte)value, 0x53, 0xad, 0x17, 0x80, 0x5a, 0, 0];
                var token = CompareCoefficientToken(data, data, probabilities, 255, value, 7, budget, previousZero);
                if (token is not null) leaves.Add(token.Value);
            }
        Assert.Equal(Enumerable.Range(0, previousZero ? 11 : 12), leaves.Order());
    }

    [Theory]
    [InlineData(false)] [InlineData(true)]
    public void CoefficientTokensPreserveIntervalPartitionSpanAndProbabilityFailures(bool previousZero)
    {
        byte[][] probabilities = [Enumerable.Repeat((byte)128, 11).ToArray(), new byte[11],
            Enumerable.Repeat((byte)255, 11).ToArray(), [1, 254, 63, 191, 0, 255, 127, 128, 129, 2, 253]];
        foreach (var p in probabilities)
            foreach (var range in new[] { 128, 191, 255 })
                foreach (var value in new[] { 0, range / 2, range - 1, range })
                    foreach (var shift in new[] { 0, 3, 7 })
                        foreach (var length in new[] { 1, 2, 8 })
                            foreach (var budget in new[] { 1, 2, 4, 8 })
                            {
                                byte[] bytes = [0, 0x53, 0xad, 0x17, 0x80, 0x5a, 0, 0]; var partition = bytes[..length];
                                CompareCoefficientToken(partition, partition, p, range, value, shift, budget, previousZero);
                                CompareCoefficientToken(partition, partition[..(length - 1)], p, range, value, shift, budget, previousZero);
                            }
        for (var length = 0; length < 11; length++)
            for (var value = 0; value < 255; value++)
            {
                byte[] data = [0, 0x53, 0xad, 0x17, 0x80, 0x5a, 0, 0];
                CompareCoefficientToken(data, data, probabilities[0][..length], 255, value, 7, 8, previousZero);
            }
    }

    [Fact]
    public void CoefficientTokenCancellationAndExhaustedWorkPreserveValidationOrder()
    {
        foreach (var previousZero in new[] { false, true })
            foreach (var length in new[] { 0, 1, 11 })
                foreach (var value in new[] { 0, 255 })
                    foreach (var cancelled in new[] { false, true })
                        CompareCoefficientToken([0], [], new byte[length], 255, value, 7, 0, previousZero, cancelled);
    }

    // Retained original bit-by-bit recurrence, independently of any bulk path.
    private sealed class Scalar(int end, int range, int value, int shift)
    {
        private int position = 1;
        public int[] State => [position, end, shift, range, value];
        public int Read(ReadOnlySpan<byte> data, int probability, OperationContext context)
        {
            context.Work(1);
            if ((uint)probability > 255) throw new ArgumentOutOfRangeException(nameof(probability));
            if (value >= range) throw new ImageDataException("Invalid VP8 arithmetic interval.");
            var boundary = 1 + ((range - 1) * probability >> 8); var bit = value >= boundary ? 1 : 0;
            if (bit == 0) range = boundary; else { value -= boundary; range -= boundary; }
            while (range < 128)
            {
                if (position == end) throw new ImageDataException("Truncated VP8 arithmetic bits.");
                range *= 2; value = value * 2 + (data[position] >> shift & 1);
                if (--shift < 0) { shift = 7; position++; }
            }
            return bit;
        }
        public int Literal(ReadOnlySpan<byte> data, int bits, OperationContext context)
        {
            if ((uint)bits > 24) throw new ArgumentOutOfRangeException(nameof(bits));
            var result = 0; for (var i = 0; i < bits; i++) result = result * 2 + Read(data, 128, context);
            return result;
        }
        public int Tree(ReadOnlySpan<byte> data, ReadOnlySpan<sbyte> tree, ReadOnlySpan<byte> probabilities, OperationContext context, int root = 0)
        {
            var node = root; do { node = tree[node + Read(data, probabilities[node / 2], context)]; } while (node >= 0);
            return -node - 1;
        }
    }
}
