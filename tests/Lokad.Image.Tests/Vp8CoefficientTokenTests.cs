using System.Runtime.InteropServices;
using Lokad.Image.Internal;
using Lokad.Image.Tooling;

namespace Lokad.Image.Tests;

public sealed partial class Vp8CoefficientBlockTests
{
    [Fact]
    public void TokenPreparationKeepsEverySymbolAndPublishedCursorExact()
    {
        var random = new Random(6386369); var seen = new bool[12];
        using var root = new TestMemoryBudget(0);
        using var reference = new OperationContext(new() { MemoryBudget = root }, default);
        using var cancellation = new CancellationTokenSource();
        using var live = new OperationContext(new() { MemoryBudget = root }, cancellation.Token);
        for (var trial = 0; trial < 128; trial++)
        {
            var data = new byte[1024]; random.NextBytes(data); var saved = data.ToArray();
            var range = 128 + trial; var initial = Reader(data, trial % 12, trial % 8, range, random.Next(range));
            var expected = initial; var window = initial.BeginCoefficientWindow(data);
            var checkedReader = new Vp8BooleanReader.CancellableBlockReader(data, initial, cancellation.Token);
            Assert.True(initial.CanReadCoefficientWindow(data, reference, 1));
            Assert.True(initial.CanReadCancellableCoefficientBlock(data, live));
            var probabilities = new byte[11];
            ref readonly var row = ref MemoryMarshal.Cast<byte, Vp8BooleanReader.CoefficientModelRow>(probabilities)[0];
            for (var tokenIndex = 0; tokenIndex < 32; tokenIndex++)
            {
                random.NextBytes(probabilities);
                if (trial % 16 == 0) Array.Fill(probabilities, (byte)0);
                if (trial % 16 == 1) Array.Fill(probabilities, (byte)255);
                var previousZero = (tokenIndex & 1) == 0;
                var token = expected.CoefficientToken(data, probabilities, reference, previousZero);
                seen[token] = true;
                Assert.Equal(token, window.Token(probabilities, previousZero));
                Assert.Equal(token, checkedReader.Token(in row, previousZero));
                var actual = initial; window.Publish(ref actual);
                var checkedActual = initial; checkedReader.Publish(ref checkedActual);
                Assert.Equal(State(expected), State(actual));
                Assert.Equal(State(expected), State(checkedActual));
                var probability = (tokenIndex % 4) switch { 0 => 0, 1 => 127, 2 => 128, _ => 255 };
                var bit = expected.Read(data, probability, reference);
                Assert.Equal(bit, window.Read(probability));
                Assert.Equal(bit, checkedReader.Read(probability));
                window.Publish(ref actual); checkedReader.Publish(ref checkedActual);
                Assert.Equal(State(expected), State(actual));
                Assert.Equal(State(expected), State(checkedActual));
            }
            if (trial == 127)
            {
                var before = initial; checkedReader.Publish(ref before); cancellation.Cancel();
                try { checkedReader.Token(in row, false); Assert.Fail("A canceled bit was read."); }
                catch (OperationCanceledException error) { Assert.Equal(cancellation.Token, error.CancellationToken); }
                var after = initial; checkedReader.Publish(ref after);
                Assert.Equal(State(before), State(after));
            }
            Assert.Equal(saved, data);
        }
        Assert.All(seen, value => Assert.True(value));
        Assert.Equal(0, root.GrowthAttempts); Assert.Equal(0, root.Bytes); Assert.False(root.IsDisposed);
    }
}
