using Lokad.Image.Internal;
using Lokad.Image.Tooling;

namespace Lokad.Image.Tests;

public sealed partial class Vp8CoefficientBlockTests
{
    [Theory]
    [InlineData(24)]
    [InlineData(25)]
    public void SharedWindowMatchesIndependentBlocksEveryCursorAndFollowingReads(int blocks)
    {
        var random = new Random(6386359 + blocks);
        using var root = new TestMemoryBudget(0);
        using var context = new OperationContext(new() { MemoryBudget = root }, default);
        using var reference = new OperationContext(new(), default);
        for (var trial = 0; trial < 64; trial++)
        {
            var data = new byte[blocks * 272 + 32]; random.NextBytes(data);
            var saved = data.ToArray(); var probabilities = new byte[1056]; random.NextBytes(probabilities);
            var range = 128 + trial % 128;
            var actual = Reader(data, trial % 12, trial % 8, range, random.Next(range)); var expected = actual;
            Assert.True(actual.CanReadCoefficientWindow(data, context, blocks));
            var window = actual.BeginCoefficientWindow(data);
            var validation = actual; var validationWindow = validation.BeginCoefficientWindow(data);
            var values = new short[18]; var original = new short[18];
            for (var block = 0; block < blocks; block++)
            {
                Array.Fill(values, (short)1234); Array.Fill(original, (short)1234);
                var plane = (trial + block) % 4; var predictor = (trial + block) % 3;
                var dc = trial % 2 == 0 ? 132 : 32767; var ac = trial % 2 == 0 ? 284 : 1;
                var nonzero = Original(ref expected, data, probabilities, original.AsSpan(1, 16), plane, predictor, dc, ac, reference);
                var seenAny = window.ReadCoefficients<Vp8BooleanReader.PixelMode>(Model(probabilities), values.AsSpan(1, 16), plane, predictor, dc, ac, out var seen);
                Assert.Equal(nonzero, seen); Assert.Equal(nonzero != 0, seenAny); Assert.Equal(original, values);
                // Inspect without rebuilding or replacing the live shared cursor.
                var validationAny = validationWindow.ReadCoefficients<Vp8BooleanReader.ValidationMode>(Model(probabilities), default, plane, predictor, 0, 0, out var validationNonzero);
                Assert.Equal(nonzero, validationNonzero); Assert.Equal(nonzero != 0, validationAny);
                validationWindow.Publish(ref validation); Assert.Equal(State(expected), State(validation));
                var checkpoint = actual; window.Publish(ref checkpoint);
                Assert.Equal(State(expected), State(checkpoint));
            }
            window.Publish(ref actual); Assert.Equal(State(expected), State(actual));
            for (var i = 0; i < 19; i++)
            {
                var probability = random.Next(256);
                Assert.Equal(expected.Read(data, probability, reference), actual.Read(data, probability, context));
                Assert.Equal(State(expected), State(actual));
            }
            Assert.Equal(saved, data);
        }
        Assert.Equal(0, root.GrowthAttempts); Assert.Equal(0, root.Bytes); Assert.False(root.IsDisposed);
    }

    [Fact]
    public void SharedWindowSelectionRespectsDeclaredPartitionAndLiveCancellationWithoutAdmission()
    {
        using var root = new TestMemoryBudget(0);
        using var context = new OperationContext(new() { MemoryBudget = root }, default);
        using var cancellation = new CancellationTokenSource();
        using var live = new OperationContext(new() { MemoryBudget = root }, cancellation.Token);
        foreach (var blocks in new[] { 1, 24, 25 })
        {
            var data = new byte[blocks * 272 + 1]; var reader = new Vp8BooleanReader(data, 0, data.Length);
            var before = State(reader);
            Assert.True(reader.CanReadCoefficientWindow(data, context, blocks));
            Assert.False(reader.CanReadCoefficientWindow(data.AsSpan(0, data.Length - 1), context, blocks));
            var shorterPartition = new Vp8BooleanReader(data, 0, data.Length - 1);
            Assert.False(shorterPartition.CanReadCoefficientWindow(data, context, blocks));
            Assert.False(reader.CanReadCoefficientWindow(data, live, blocks));
            Assert.Equal(before, State(reader));
            foreach (var invalid in new[] { int.MinValue, -1, 0, 26, int.MaxValue })
                Assert.False(reader.CanReadCoefficientWindow(data, context, invalid));
        }
        cancellation.Cancel(); var large = new byte[25 * 272 + 1]; var canceled = new Vp8BooleanReader(large, 0, large.Length);
        Assert.False(canceled.CanReadCoefficientWindow(large, live, 25));
        Assert.Equal(0, root.GrowthAttempts); Assert.Equal(0, root.Bytes); Assert.False(root.IsDisposed);
    }
}
