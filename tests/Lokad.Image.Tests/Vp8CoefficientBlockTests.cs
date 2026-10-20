using System.Reflection;
using Lokad.Image.Internal;
using Lokad.Image.Tooling;

namespace Lokad.Image.Tests;

public sealed class Vp8CoefficientBlockTests
{
    private static readonly FieldInfo[] Fields = new[] { "position", "end", "shift", "range", "value" }
        .Select(n => typeof(Vp8BooleanReader).GetField(n, BindingFlags.Instance | BindingFlags.NonPublic)!).ToArray();
    private static int[] State(Vp8BooleanReader reader) => Fields.Select(f => (int)f.GetValue(reader)!).ToArray();
    private static Vp8BooleanReader Reader(byte[] data, int offset, int phase, int range, int value)
    {
        object reader = new Vp8BooleanReader(data, offset, data.Length - offset);
        Fields[2].SetValue(reader, phase); Fields[3].SetValue(reader, range); Fields[4].SetValue(reader, value);
        return (Vp8BooleanReader)reader;
    }

    // Retain the original block composition over the independently tested
    // bit-at-a-time reader. It neither calls the window nor reuses its state.
    private static byte Original(ref Vp8BooleanReader reader, byte[] data, byte[] probabilities,
        Span<short> values, int plane, int predictor, int dc, int ac, OperationContext context)
    {
        byte[] bands = [0, 1, 2, 3, 6, 4, 5, 6, 6, 6, 6, 6, 6, 6, 6, 7];
        byte[] zigzag = [0, 1, 4, 8, 5, 2, 3, 6, 9, 12, 13, 10, 7, 11, 14, 15];
        var zero = false; byte nonzero = 0;
        for (var c = plane == 0 ? 1 : 0; c < 16; c++)
        {
            var p = probabilities.AsSpan(((plane * 8 + bands[c]) * 3 + predictor) * 11, 11);
            var token = reader.CoefficientToken(data, p, context, zero);
            if (token == 11) break;
            var sample = token;
            if (token >= 5)
            {
                byte[][] categories = [[159], [165, 145], [173, 148, 140], [176, 155, 140, 135],
                    [180, 157, 141, 134, 130], [254, 254, 243, 230, 196, 177, 153, 140, 133, 130, 129]];
                int[] bases = [5, 7, 11, 19, 35, 67]; sample = 0;
                foreach (var probability in categories[token - 5]) sample = sample * 2 + reader.Read(data, probability, context);
                sample += bases[token - 5];
            }
            zero = sample == 0; predictor = Math.Min(sample, 2);
            if (zero) continue;
            nonzero = 1;
            if (reader.Read(data, 128, context) != 0) sample = -sample;
            values[zigzag[c]] = unchecked((short)(sample * (c == 0 ? dc : ac)));
        }
        return nonzero;
    }

    [Fact]
    public void WindowBlocksMatchOriginalCoefficientsWorkFullCursorAndFollowingReads()
    {
        var random = new Random(63861302);
        for (var trial = 0; trial < 1024; trial++)
        {
            var data = new byte[1024]; random.NextBytes(data);
            var probabilities = new byte[4 * 8 * 3 * 11]; random.NextBytes(probabilities);
            var range = 128 + trial % 128; var reader = Reader(data, trial % 12, trial % 8, range, random.Next(range));
            var original = reader; var expected = Enumerable.Repeat((short)1234, 18).ToArray(); var actual = expected.ToArray();
            using var context = new OperationContext(new() { MaxWork = 1000 }, default);
            using var reference = new OperationContext(new() { MaxWork = 1000 }, default);
            Assert.True(reader.CanReadCoefficientBlock(data, context));
            var dc = trial % 2 == 0 ? 132 : 32767; var ac = trial % 2 == 0 ? 284 : 1;
            var nonzero = Original(ref original, data, probabilities, expected.AsSpan(1, 16), trial % 4, trial % 3, dc, ac, reference);
            var result = reader.ReadCoefficientBlock(data, probabilities, actual.AsSpan(1, 16), trial % 4, trial % 3, dc, ac, context, out var seen);
            Assert.Equal(nonzero, seen); Assert.Equal(nonzero != 0, result); Assert.Equal(expected, actual);
            Assert.Equal(State(original), State(reader)); Assert.Equal(reference.RemainingWork, context.RemainingWork);
            for (var i = 0; i < 19; i++)
            {
                var p = random.Next(256); Assert.Equal(original.Read(data, p, reference), reader.Read(data, p, context));
                Assert.Equal(State(original), State(reader)); Assert.Equal(reference.RemainingWork, context.RemainingWork);
            }
        }
    }

    [Fact]
    public void MaximumCategoryBlockUsesExactly304WorkWithin272AvailableBytes()
    {
        for (var phase = 0; phase < 8; phase++)
        {
            var data = Enumerable.Repeat((byte)255, 273).ToArray(); data[0] = 254;
            var reader = Reader(data, 0, phase, 255, 254); var original = reader;
            var probabilities = new byte[1056]; var expected = new short[16]; var actual = new short[16];
            using var context = new OperationContext(new() { MaxWork = 304 }, default);
            using var reference = new OperationContext(new() { MaxWork = 304 }, default);
            Assert.True(reader.CanReadCoefficientBlock(data, context));
            Assert.Equal(1, Original(ref original, data, probabilities, expected, 3, 0, 132, 284, reference));
            Assert.True(reader.ReadCoefficientBlock(data, probabilities, actual, 3, 0, 132, 284, context, out var nonzero));
            Assert.Equal(1, nonzero); Assert.Equal(expected, actual); Assert.Equal(State(original), State(reader));
            Assert.Equal(0, reference.RemainingWork); Assert.Equal(0, context.RemainingWork);
            var before = State(reader); Assert.Throws<ImageLimitException>(() => reader.Read(data, 128, context));
            Assert.Equal(before, State(reader));
        }
    }

    [Fact]
    public void SelectionDoesNotChargeAndKeepsTightWorkShortInputsAndCancelableReadsOriginal()
    {
        using var root = new TestMemoryBudget(0);
        using var enough = new OperationContext(new() { MaxWork = 304, MemoryBudget = root }, default);
        using var tight = new OperationContext(new() { MaxWork = 303, MemoryBudget = root }, default);
        using var cancellation = new CancellationTokenSource();
        using var live = new OperationContext(new() { MemoryBudget = root }, cancellation.Token);
        byte[] data = new byte[273]; var reader = new Vp8BooleanReader(data, 0, data.Length); var before = State(reader);
        Assert.True(reader.CanReadCoefficientBlock(data, enough)); Assert.False(reader.CanReadCoefficientBlock(data, tight));
        Assert.False(reader.CanReadCoefficientBlock(data, live)); cancellation.Cancel();
        Assert.False(reader.CanReadCoefficientBlock(data, live));
        Assert.False(reader.CanReadCoefficientBlock(data.AsSpan(0, 272), enough));
        var shortPartition = new Vp8BooleanReader(data, 0, 272);
        Assert.False(shortPartition.CanReadCoefficientBlock(data, enough));
        var invalid = Reader(data, 0, 7, 255, 255);
        Assert.False(invalid.CanReadCoefficientBlock(data, enough));
        Assert.Equal(before, State(reader)); Assert.Equal(304, enough.RemainingWork); Assert.Equal(303, tight.RemainingWork);
        Assert.Equal(0, root.GrowthAttempts); Assert.Equal(0, root.Bytes); Assert.False(root.IsDisposed);
    }
}
