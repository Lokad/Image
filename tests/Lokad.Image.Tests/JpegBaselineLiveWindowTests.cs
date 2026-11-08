using Lokad.Image.Internal;

namespace Lokad.Image.Tests;

public sealed partial class JpegBaselineBlockTests
{
    [Fact]
    public void LiveWindowCommitsDcAndEndOfBlockWithExactFollowingReads()
    {
        var tables = Tables();
        for (var phase = 0; phase < 8; phase++)
        for (var size = 0; size <= 11; size++)
        foreach (var value in size == 0 ? new[] { 0 } : new[] { 1 << (size - 1), -((1 << size) - 1) })
        {
            var input = Stuff(new string('0', phase) + Dc(size) + Signed(value, size)
                + Ac(1) + "1" + Ac(0) + "1011010010101100" + new string('0', 64));
            using var cancellation = new CancellationTokenSource();
            using var context = new OperationContext(ImageLimits.Default, cancellation.Token);
            var reader = new JpegEntropyReader(input, 0, context); var original = reader;
            if (phase != 0) { reader.ReadBits(phase); original.ReadBits(phase); }
            var actual = Enumerable.Repeat(12345, 66).ToArray(); var expected = actual.ToArray();
            var predictor = 17; var expectedPredictor = 17; var coefficient = 1;
            ScalarBaseline(ref original, expected.AsSpan(1, 64), ref expectedPredictor, tables.Dc, tables.Ac, context);
            Assert.True(tables.Dc.TryReadCancelableBaselineBlock(ref reader, tables.Ac,
                actual.AsSpan(1, 64), ref predictor, ref coefficient));
            Assert.Equal(64, coefficient); Assert.Equal(expectedPredictor, predictor);
            Assert.Equal(expected, actual); Assert.Equal(original.Position, reader.Position);
            for (var count = 1; count <= 16; count++)
            {
                var referenceTail = original; var actualTail = reader;
                Assert.Equal(referenceTail.ReadBits(count), actualTail.ReadBits(count));
                Assert.Equal(referenceTail.Position, actualTail.Position);
            }
        }
    }

    [Fact]
    public void CanceledLiveWindowDoesNotCommitSpeculativeDc()
    {
        var tables = Tables();
        for (var phase = 0; phase < 8; phase++)
        {
            var input = Stuff(new string('0', phase) + Dc(1) + "1" + Ac(0) + new string('0', 64));
            using var cancellation = new CancellationTokenSource();
            using var context = new OperationContext(ImageLimits.Default, cancellation.Token);
            var reader = new JpegEntropyReader(input, 0, context);
            if (phase != 0) reader.ReadBits(phase);
            cancellation.Cancel(); var position = reader.Position;
            var block = Enumerable.Repeat(12345, 64).ToArray(); var predictor = 17; var coefficient = 1;
            try
            {
                tables.Dc.TryReadCancelableBaselineBlock(ref reader, tables.Ac, block, ref predictor, ref coefficient);
                Assert.Fail("The DC prefix must observe cancellation before publication.");
            }
            catch (OperationCanceledException error) { Assert.Equal(cancellation.Token, error.CancellationToken); }
            Assert.Equal(position, reader.Position); Assert.Equal(17, predictor); Assert.Equal(1, coefficient);
            Assert.All(block, value => Assert.Equal(12345, value));
        }
    }
}
