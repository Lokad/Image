using Lokad.Image.Internal;

namespace Lokad.Image.Tests;

public sealed class Vp8PrimitiveTests
{
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
        var limited = new OperationContext(new ImageLimits { MaxWork = 1 }, default);
        var bounded = new Vp8BooleanReader(data, 0, data.Length); bounded.Read(data, 128, limited);
        Assert.Throws<ImageLimitException>(() => bounded.Read(data, 128, limited));
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
        Vp8Prediction.Small(plane, 32, 4, 4, 0, 0, mode);
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
    public void AllAlphaFiltersHaveHandComputedBordersClampAndModuloCases()
    {
        byte[] expected = [10, 20, 250, 30, 40, 50, 240, 100, 110];
        byte[][] residuals = [expected, [10,10,230,20,10,10,210,116,10],
            [10,10,230,20,20,56,210,60,60], [10,10,230,20,0,51,210,106,0]];
        for (var filter = 0; filter < 4; filter++)
        {
            // Reserved flags are ignored; the level-reduction preprocessing flag is informative.
            var payload = new byte[10]; payload[0] = (byte)(0xD0 | filter << 2); residuals[filter].CopyTo(payload, 1);
            Assert.Equal(expected, WebpAlpha.Decode(payload, 3, 3, new OperationContext(ImageLimits.Default, default)));
        }
        Assert.Throws<ImageDataException>(() => WebpAlpha.Decode(new byte[1], 3, 3, new OperationContext(ImageLimits.Default, default)));
        Assert.Throws<UnsupportedImageException>(() => WebpAlpha.Decode(new byte[] { 2 }, 3, 3, new OperationContext(ImageLimits.Default, default)));
    }
}
