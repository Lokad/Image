using Lokad.Image.Internal;
using Lokad.Image.Tooling;

namespace Lokad.Image.Tests;

public sealed partial class Vp8CoefficientBlockTests
{
    [Theory]
    [InlineData(false)] [InlineData(true)]
    public void ValidationConsumesOriginalTokensAndContextsWithoutTouchingDestination(bool live)
    {
        var random = new Random(6386454);
        var seenTokens = new bool[12];
        using var token = new CancellationTokenSource();
        using var root = new TestMemoryBudget(0);
        using var context = new OperationContext(new() { MemoryBudget = root }, live ? token.Token : default);
        using var reference = new OperationContext(new(), live ? token.Token : default);
        for (var trial = 0; trial < 256; trial++)
        {
            var data = new byte[1024]; random.NextBytes(data); var saved = data.ToArray();
            var probabilities = new byte[1056]; random.NextBytes(probabilities);
            if (trial % 16 == 0) Array.Fill(probabilities, (byte)0);
            if (trial % 16 == 1) Array.Fill(probabilities, (byte)255);
            var range = 128 + trial % 128;
            var actual = Reader(data, trial % 12, trial % 8, range, random.Next(range)); var expected = actual;
            var samples = new short[16]; var guarded = Enumerable.Repeat((short)-23456, 18).ToArray();
            var plane = trial % 4; var predictor = trial % 3;
            var expectedNonzero = Original(ref expected, data, probabilities, samples, plane, predictor,
                trial % 2 == 0 ? 32767 : 132, trial % 2 == 0 ? 1 : 284, reference, seenTokens);
            byte seen;
            var any = live
                ? actual.ReadCancellableCoefficientBlock<Vp8BooleanReader.ValidationMode>(data, Model(probabilities),
                    guarded.AsSpan(1, 16), plane, predictor, 0, 0, context, out seen)
                : actual.ReadCoefficientBlock<Vp8BooleanReader.ValidationMode>(data, Model(probabilities),
                    guarded.AsSpan(1, 16), plane, predictor, 0, 0, context, out seen);
            Assert.Equal(expectedNonzero, seen); Assert.Equal(seen != 0, any);
            Assert.Equal(State(expected), State(actual));
            Assert.False(guarded.AsSpan().ContainsAnyExcept((short)-23456));
            for (var i = 0; i < 19; i++)
            {
                var p = random.Next(256);
                Assert.Equal(expected.Read(data, p, reference), actual.Read(data, p, context));
                Assert.Equal(State(expected), State(actual));
            }
            Assert.Equal(saved, data);
        }
        Assert.All(seenTokens, seen => Assert.True(seen));
        Assert.Equal(0, root.GrowthAttempts); Assert.Equal(0, root.Bytes); Assert.False(root.IsDisposed);
    }

    [Theory]
    [InlineData(false)] [InlineData(true)]
    public void ValidationAndMaterializationKeepMalformedModelAndCancellationPrefixes(bool live)
    {
        using var root = new TestMemoryBudget(0);
        var data = Enumerable.Repeat((byte)255, 1024).ToArray(); data[0] = 254;
        foreach (var length in new[] { 0, 11, 33, 1055 })
        foreach (var canceled in new[] { false, true })
        {
            if (canceled && !live) continue;
            using var token = new CancellationTokenSource();
            using var context = new OperationContext(new() { MemoryBudget = root }, live ? token.Token : default);
            var probabilities = Enumerable.Repeat((byte)128, length).ToArray();
            var materialized = Reader(data, 0, 7, 255, 254); var validation = materialized;
            var samples = Enumerable.Repeat((short)1234, 16).ToArray();
            if (canceled) token.Cancel();
            Exception? first = null; Exception? second = null;
            byte firstNonzero = 55, secondNonzero = 55;
            try
            {
                if (live) materialized.ReadCancellableCoefficientBlock<Vp8BooleanReader.PixelMode>(data, Model(probabilities),
                    samples, 3, 2, 132, 284, context, out firstNonzero);
                else materialized.ReadCoefficientBlock<Vp8BooleanReader.PixelMode>(data, Model(probabilities),
                    samples, 3, 2, 132, 284, context, out firstNonzero);
            }
            catch (Exception error) { first = error; }
            try
            {
                if (live) validation.ReadCancellableCoefficientBlock<Vp8BooleanReader.ValidationMode>(data, Model(probabilities),
                    default, 3, 2, 0, 0, context, out secondNonzero);
                else validation.ReadCoefficientBlock<Vp8BooleanReader.ValidationMode>(data, Model(probabilities),
                    default, 3, 2, 0, 0, context, out secondNonzero);
            }
            catch (Exception error) { second = error; }
            Assert.Equal(first?.GetType(), second?.GetType()); Assert.Equal(firstNonzero, secondNonzero);
            Assert.Equal(State(materialized), State(validation));
            if (first is OperationCanceledException a && second is OperationCanceledException b)
            { Assert.Equal(token.Token, a.CancellationToken); Assert.Equal(token.Token, b.CancellationToken); }
        }
        Assert.Equal(0, root.GrowthAttempts); Assert.Equal(0, root.Bytes); Assert.False(root.IsDisposed);
    }
}
