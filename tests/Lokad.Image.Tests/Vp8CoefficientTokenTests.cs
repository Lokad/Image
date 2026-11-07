using Lokad.Image.Internal;
using Lokad.Image.Tooling;

namespace Lokad.Image.Tests;

public sealed partial class Vp8CoefficientBlockTests
{
    [Fact]
    public void OrdinaryBlocksKeepEverySymbolCoefficientsAndPublishedCursorExact()
    {
        var random = new Random(6386369); var seen = new bool[12];
        using var root = new TestMemoryBudget(0);
        using var reference = new OperationContext(new() { MemoryBudget = root }, default);
        for (var trial = 0; trial < 4096; trial++)
        {
            var data = new byte[1024]; random.NextBytes(data); var saved = data.ToArray();
            var range = 128 + trial % 128; var actual = Reader(data, trial % 12, trial % 8, range, random.Next(range));
            var expected = actual;
            Assert.True(actual.CanReadCoefficientWindow(data, reference, 1));
            var probabilities = new byte[1056]; random.NextBytes(probabilities);
            if (trial % 16 == 0) Array.Fill(probabilities, (byte)0);
            if (trial % 16 == 1) Array.Fill(probabilities, (byte)255);
            var expectedValues = Enumerable.Repeat((short)1234, 18).ToArray();
            var actualValues = expectedValues.ToArray();
            var dc = trial % 2 == 0 ? 132 : 32767; var ac = trial % 2 == 0 ? 284 : 1;
            var nonzero = Original(ref expected, data, probabilities, expectedValues.AsSpan(1, 16),
                trial % 4, trial % 3, dc, ac, reference, seen);
            var result = actual.ReadCoefficientBlock<Vp8BooleanReader.PixelMode>(data, Model(probabilities),
                actualValues.AsSpan(1, 16), trial % 4, trial % 3, dc, ac, reference, out var actualNonzero);
            Assert.Equal(nonzero, actualNonzero); Assert.Equal(nonzero != 0, result);
            Assert.Equal(expectedValues, actualValues); Assert.Equal(State(expected), State(actual));
            for (var following = 0; following < 19; following++)
            {
                var probability = random.Next(256);
                Assert.Equal(expected.Read(data, probability, reference), actual.Read(data, probability, reference));
                Assert.Equal(State(expected), State(actual));
            }
            Assert.Equal(saved, data);
        }
        Assert.All(seen, value => Assert.True(value));
        Assert.Equal(0, root.GrowthAttempts); Assert.Equal(0, root.Bytes); Assert.False(root.IsDisposed);
    }
}
