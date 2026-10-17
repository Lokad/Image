using Lokad.Image.Internal;

namespace Lokad.Image.Tests;

public sealed class JpegCorrectionValueTests
{
    [Theory]
    [InlineData(1)] [InlineData(2)] [InlineData(4)] [InlineData(8)]
    [InlineData(16)] [InlineData(32)] [InlineData(64)] [InlineData(128)]
    [InlineData(256)] [InlineData(512)] [InlineData(1024)] [InlineData(2048)]
    [InlineData(4096)] [InlineData(8192)]
    public void CorrectionMatchesSignedMagnitudeAtEveryLowBitAndSign(int increment)
    {
        for (var value = -65536; value <= 65536; value++) Check(value);
        foreach (var value in new[] { int.MinValue + 1, int.MinValue + 8192, int.MinValue + 16384,
            int.MaxValue - 16384, int.MaxValue - 8192, int.MaxValue - 1, int.MaxValue }) Check(value);

        void Check(int value)
        {
            var expected = value;
            if ((Math.Abs(expected) & increment) == 0) expected += expected > 0 ? increment : -increment;
            JpegDecoder.ApplyCorrection(ref value, increment);
            Assert.Equal(expected, value);
        }
    }

    [Fact]
    public void MinimumIntegerRetainsOverflowWithoutMutation()
    {
        var value = int.MinValue;
        var expected = Assert.Throws<OverflowException>(() => Math.Abs(value));
        var actual = Assert.Throws<OverflowException>(() => JpegDecoder.ApplyCorrection(ref value, 1));
        Assert.Equal(expected.Message, actual.Message);
        Assert.Equal(int.MinValue, value);
    }
}
