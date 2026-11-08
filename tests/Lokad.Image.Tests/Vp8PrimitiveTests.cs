using Lokad.Image.Internal;

namespace Lokad.Image.Tests;

public sealed class Vp8PrimitiveTests
{
    [Fact]
    public void PreparedSmallPredictionMatchesEveryOriginalModeAndNeighborBoundary()
    {
        uint state = 0x58638619;
        foreach (var stride in new[] { 16, 32, 48 })
        for (var macroX = 0; macroX < stride; macroX += 16)
        foreach (var macroY in new[] { 0, 16 })
        for (var dx = 0; dx < 16; dx += 4) for (var dy = 0; dy < 16; dy += 4)
        for (var pattern = 0; pattern < 3; pattern++)
        {
            var expected = new byte[stride * (macroY + 16)];
            for (var i = 0; i < expected.Length; i++)
            {
                state = unchecked(1664525 * state + 1013904223);
                expected[i] = pattern == 0 ? (byte)(state >> 24)
                    : pattern == 1 ? (byte)((i & 1) == 0 ? 0 : 255) : (byte)(127 + i % 3);
            }
            var actual = expected.ToArray(); var prepared = new int[13]; var x = macroX + dx; var y = macroY + dy;
            var dc = Vp8Prediction.PrepareSmall(actual, stride, x, y, macroX, macroY, prepared);
            var unchanged = prepared.ToArray();
            // Each candidate overwrites this block; the original gathers again.
            // Shared samples must remain valid through the final chosen mode.
            foreach (var mode in Enumerable.Range(0, 10).Concat(new[] { 3, 1, 0, 9 }))
            {
                Vp8PredictionReference.Small(expected, stride, x, y, macroX, macroY, mode);
                Vp8Prediction.SmallPrepared(actual, stride, x, y, mode, prepared, dc);
                Assert.Equal(expected, actual); Assert.Equal(unchanged, prepared);
            }
        }
    }

    [Fact]
    public void PreparedSmallInvalidModePreservesOriginalErrorBeforeAnyWrite()
    {
        var plane = Enumerable.Repeat((byte)129, 256).ToArray(); var prepared = new int[13];
        var dc = Vp8Prediction.PrepareSmall(plane, 16, 0, 0, 0, 0, prepared); var unchanged = plane.ToArray();
        foreach (var mode in new[] { -1, 10 })
        {
            Assert.Throws<ImageDataException>(() => Vp8Prediction.SmallPrepared(plane, 16, 0, 0, mode, prepared, dc));
            Assert.Equal(unchanged, plane);
        }
    }

    [Fact]
    public void BooleanIntervalsHaveHandComputedBitsAndBoundedInput()
    {
        byte[] data = [192, 0, 0, 0]; var context = new OperationContext(ImageLimits.Default, default);
        var reader = new Vp8BooleanReader(data, 0, data.Length);
        Assert.Equal(1, reader.Read(data, 128, context)); Assert.Equal(1, reader.Read(data, 128, context));
        for (var bit = 0; bit < 6; bit++) Assert.Equal(0, reader.Read(data, 128, context));
        Assert.Equal(1, reader.Read(data, 128, context));
        for (var bit = 0; bit < 6; bit++) Assert.Equal(0, reader.Read(data, 128, context));
        byte[] rare = [254, 0, 0, 0]; var rareReader = new Vp8BooleanReader(rare, 0, rare.Length);
        Assert.Equal(1, rareReader.Read(rare, 255, context)); Assert.Equal(0, rareReader.Read(rare, 0, context));
        byte[] tiny = [0]; var truncated = new Vp8BooleanReader(tiny, 0, 1);
        Assert.Equal(0, truncated.Read(tiny, 128, context));
        Assert.Throws<ImageDataException>(() => truncated.Read(tiny, 128, context));
        Assert.Throws<ImageDataException>(() => new Vp8BooleanReader(tiny, 1, 1));
        var invalid = new Vp8BooleanReader(new byte[] { 255 }, 0, 1);
        Assert.Throws<ImageDataException>(() => invalid.Read(new byte[] { 255 }, 128, context));

    }

    [Fact]
    public void IntegerTransformsUseSpecifiedPositiveAndNegativeRounding()
    {
        foreach (var dc in new short[] { 0, 8, -8, 4, -4 })
        {
            var block = new short[16]; block[0] = dc; Vp8Transforms.InverseDct(block);
            Assert.All(block, value => Assert.Equal((dc + 4) >> 3, value));
        }
        var dct = new short[16]; dct[1] = 8; Vp8Transforms.InverseDct(dct);
        Assert.Equal(Enumerable.Repeat(new short[] { 1, 1, 0, -1 }, 4).SelectMany(row => row), dct);
        var walsh = new short[16]; walsh[1] = 8; Vp8Transforms.InverseWalsh(walsh);
        Assert.Equal(Enumerable.Repeat(new short[] { 1, 1, -1, -1 }, 4).SelectMany(row => row), walsh);
        walsh.AsSpan().Clear(); walsh[0] = -5; Vp8Transforms.InverseWalsh(walsh);
        Assert.All(walsh, value => Assert.Equal(-1, value));
    }

    public static IEnumerable<object[]> Predictions()
    {
        int[][] expected =
        [
            [65,65,65,65, 65,65,65,65, 65,65,65,65, 65,65,65,65],
            [0,0,0,0, 0,0,0,10, 0,0,10,20, 0,10,20,30],
            [43,20,30,40, 43,20,30,40, 43,20,30,40, 43,20,30,40],
            [103,103,103,103, 100,100,100,100, 110,110,110,110, 118,118,118,118],
            [20,30,40,50, 30,40,50,60, 40,50,60,70, 50,60,70,78],
            [90,43,20,30, 103,90,43,20, 100,103,90,43, 110,100,103,90],
            [70,15,25,35, 90,43,20,30, 103,70,15,25, 100,90,43,20],
            [15,25,35,45, 20,30,40,50, 25,35,45,60, 30,40,50,70],
            [110,90,43,20, 95,103,110,90, 105,100,95,103, 115,110,105,100],
            [95,100,105,110, 105,110,115,118, 115,118,120,120, 120,120,120,120]
        ];
        for (var mode = 0; mode < expected.Length; mode++) yield return [mode, expected[mode]];
    }

    [Theory]
    [MemberData(nameof(Predictions))]
    public void AllTenSubblockModesHaveHandComputedSamples(int mode, int[] expected)
    {
        var plane = new byte[32 * 32]; plane[3 * 32 + 3] = 130;
        for (var x = 0; x < 8; x++) plane[3 * 32 + 4 + x] = (byte)(10 + x * 10);
        for (var y = 0; y < 4; y++) plane[(y + 4) * 32 + 3] = (byte)(90 + y * 10);
        Span<int> prepared = stackalloc int[13];
        var dc = Vp8Prediction.PrepareSmall(plane, 32, 4, 4, 0, 0, prepared);
        Vp8Prediction.SmallPrepared(plane, 32, 4, 4, mode, prepared, dc);
        for (var y = 0; y < 4; y++)
            for (var x = 0; x < 4; x++) Assert.Equal(expected[y * 4 + x], plane[(y + 4) * 32 + 4 + x]);
    }

    [Fact]
    public void LargePredictionUsesTheSpecifiedMissingEdgeValues()
    {
        var plane = new byte[16 * 16];
        foreach (var (mode, value) in new[] { (0, 128), (1, 127), (2, 129), (3, 129) })
        {
            Vp8Prediction.Large(plane, 16, 0, 0, 16, mode);
            Assert.All(plane, pixel => Assert.Equal(value, pixel));
        }
    }

    [Fact]
    public void LoopFilterWidthsAndThresholdsHaveHandComputedSamples()
    {
        int[] original = [100, 100, 100, 100, 110, 110, 110, 110]; var samples = original.ToArray();
        Vp8LoopFilter.Segment(samples, 40, 20, 0, true, false);
        Assert.Equal(new[] { 100, 101, 103, 104, 106, 107, 109, 110 }, samples);
        samples = original.ToArray(); Vp8LoopFilter.Segment(samples, 40, 20, 0, false, false);
        Assert.Equal(new[] { 100, 100, 102, 104, 106, 108, 110, 110 }, samples);
        samples = original.ToArray(); Vp8LoopFilter.Segment(samples, 40, 20, 0, true, true);
        Assert.Equal(new[] { 100, 100, 100, 102, 107, 110, 110, 110 }, samples);
        samples = original.ToArray(); Vp8LoopFilter.Segment(samples, 20, 20, 0, true, false);
        Assert.Equal(original, samples);
    }

    [Fact]
    public void HorizontalGreenPublicationKeepsPrefixesBordersTailsAndOwnership()
    {
        foreach (var width in new[] { 1, 3, 4, 5, 7, 8, 9, 31, 32, 33, 513 })
        foreach (var seed in new[] { 0, 1, 255 })
        {
            const int height = 3;
            var input = Enumerable.Repeat(0xA731D649u, width * height + 2).ToArray();
            var expected = new byte[width * height];
            for (var i = 0; i < expected.Length; i++)
            {
                var green = unchecked((byte)(seed + i * 91 + (i / width) * 37));
                input[i + 1] = 0xA700D649u & ~0xFF00u | (uint)green << 8;
                expected[i] = green;
            }
            // Independent byte recurrence from RFC 9649. First-column samples
            // use the sample above; every other sample uses its left neighbor.
            for (var y = 0; y < height; y++) for (var x = 0; x < width; x++)
            {
                var at = y * width + x;
                var predictor = x != 0 ? expected[at - 1] : y != 0 ? expected[at - width] : 0;
                expected[at] = unchecked((byte)(expected[at] + predictor));
            }
            var original = input.ToArray();
            using var root = new Lokad.Image.Tooling.TestMemoryBudget(0);
            using var context = new OperationContext(new() { MemoryBudget = root }, default);
            var compact = Enumerable.Repeat((byte)0xAD, expected.Length + 6).ToArray();
            WebpAlpha.PublishHorizontalGreen(input.AsSpan(1, expected.Length), compact.AsSpan(3, expected.Length), width, height, context);
            Assert.Equal(expected, compact.AsSpan(3, expected.Length).ToArray());
            Assert.Equal(original, input);
            Assert.All(compact[..3], b => Assert.Equal(0xAD, b)); Assert.All(compact[^3..], b => Assert.Equal(0xAD, b));
            WebpAlpha.PublishHorizontalGreen(input.AsSpan(1, expected.Length), default, width, height, context);
            for (var i = 0; i < expected.Length; i++) Assert.Equal((uint)expected[i] << 24, input[i + 1]);
            Assert.Equal(original[0], input[0]); Assert.Equal(original[^1], input[^1]);
            Assert.Equal(0, root.Bytes); Assert.Equal(0, root.GrowthAttempts); Assert.False(root.IsDisposed);
        }
    }

    [Fact]
    public void HorizontalGreenPublicationChecksCancellationBeforeBorrowedStorage()
    {
        using var token = new CancellationTokenSource();
        using var context = new OperationContext(ImageLimits.Default, token.Token); token.Cancel();
        uint[] input = [0x17AD8321, 0xFE001122, 0xAA55EEFF, 0xFF9321AD, 0xAAFF1177];
        var original = input.ToArray(); var compact = Enumerable.Repeat((byte)0xA7, 5).ToArray();
        var error = Assert.Throws<OperationCanceledException>(() => WebpAlpha.PublishHorizontalGreen(input, compact, 5, 1, context));
        Assert.Equal(token.Token, error.CancellationToken); Assert.Equal(original, input); Assert.All(compact, b => Assert.Equal(0xA7, b));
        Assert.Throws<OperationCanceledException>(() => WebpAlpha.PublishHorizontalGreen(input, default, 5, 1, context));
        Assert.Equal(original, input);
    }

    [Fact]
    public void AllAlphaFiltersHaveHandComputedBordersClampAndModuloCases()
    {
        byte[] expected = [10, 20, 250, 30, 40, 50, 240, 100, 110];
        byte[][] residuals = [expected, [10,10,230,20,10,10,210,116,10],
            [10,10,230,20,20,56,210,60,60], [10,10,230,20,0,51,210,106,0]];
        for (var filter = 0; filter < 4; filter++)
        {
            // Reserved flags are ignored; the level-reduction preprocessing flag is informative.
            var payload = new byte[10]; payload[0] = (byte)(0xD0 | filter << 2); residuals[filter].CopyTo(payload, 1);
            using var context = new OperationContext(ImageLimits.Default, default);
            ImageBuffer? result = null;
            Assert.Equal(expected, WebpAlpha.Decode(payload, 3, 3, context, ref result));
            Assert.Null(result);
            var interleaved = Enumerable.Repeat((byte)0xA7, 36).ToArray();
            for (var i = 0; i < 9; i++) interleaved[i * 4 + 3] = residuals[filter][i];
            WebpAlpha.Unfilter(interleaved.AsSpan(3), 3, 3, filter, context, 4);
            for (var i = 0; i < 9; i++)
            {
                Assert.Equal(expected[i], interleaved[i * 4 + 3]);
                Assert.True(interleaved.AsSpan(i * 4, 3).IndexOfAnyExcept((byte)0xA7) < 0);
            }
        }
        using var invalidContext = new OperationContext(ImageLimits.Default, default);
        ImageBuffer? invalidResult = null;
        Assert.Throws<ImageDataException>(() => WebpAlpha.Decode(new byte[1], 3, 3, invalidContext, ref invalidResult));
        Assert.Throws<UnsupportedImageException>(() => WebpAlpha.Decode(new byte[] { 2 }, 3, 3, invalidContext, ref invalidResult));
        Assert.Null(invalidResult);
    }
}
