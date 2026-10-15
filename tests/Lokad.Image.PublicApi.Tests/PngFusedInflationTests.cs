using Lokad.Image.Tooling;

namespace Lokad.Image.PublicApi.Tests;

public sealed class PngFusedInflationTests
{
    [Theory]
    [InlineData(8)] [InlineData(16)]
    public void FixedCompleteTreesPreserveExactPixelsAndTightAdmission(int depth)
    {
        var frame = new AuthoredPng.Frame("fused", 6, depth, 19, 17, false);
        var raw = AuthoredPng.FilteredRows(frame);
        var compressed = AuthoredDeflate.Fixed(raw.Select(value => new AuthoredDeflate.Token(value)), raw);
        var encoded = AuthoredPng.Wrap(frame, compressed); var original = encoded.ToArray();
        using var cancellation = new CancellationTokenSource();
        using var fused = ImageCodec.Decode(encoded);
        using var originalPath = ImageCodec.Decode(encoded, cancellationToken: cancellation.Token);
        Assert.Equal(AuthoredPng.Expected(frame), OriginalPixels.Bytes(fused));
        Assert.Equal(OriginalPixels.Bytes(originalPath), OriginalPixels.Bytes(fused));
        var peak = encoded.Length + (long)compressed.Length + 19 * 17 * (depth / 8) * 4
            + 256 * 1024 + 19 * (depth / 8) * 8;
        using var tight = ImageCodec.Decode(encoded, new DecodeOptions { Limits = new ImageLimits { MaxWorkingBytes = peak } });
        using var cancelableTight = ImageCodec.Decode(encoded,
            new DecodeOptions { Limits = new ImageLimits { MaxWorkingBytes = peak } }, cancellation.Token);
        Assert.Equal(OriginalPixels.Bytes(fused), OriginalPixels.Bytes(tight));
        Assert.Equal(AuthoredPng.Expected(frame), OriginalPixels.Bytes(cancelableTight));
        Assert.Throws<ImageLimitException>(() => ImageCodec.Decode(encoded,
            new DecodeOptions { Limits = new ImageLimits { MaxWorkingBytes = peak - 1 } }));
        Assert.Throws<ImageLimitException>(() => ImageCodec.Decode(encoded,
            new DecodeOptions { Limits = new ImageLimits { MaxWorkingBytes = peak - 1 } }, cancellation.Token));
        if (depth == 8) fused.GetRow8(0)[0] ^= 255; else fused.GetRow16(0)[0] ^= 65535;
        Assert.Equal(AuthoredPng.Expected(frame), OriginalPixels.Bytes(originalPath));
        Assert.Equal(original, encoded);
    }

    [Theory]
    [InlineData(0, false)] [InlineData(0, true)]
    [InlineData(16, false)] [InlineData(16, true)]
    public void FilterAndChecksumFailuresKeepTheOriginalWorkAndErrorOrder(int badRow, bool badChecksum)
    {
        var frame = new AuthoredPng.Frame("fused-errors", 6, 16, 19, 17, false);
        var raw = AuthoredPng.FilteredRows(frame); raw[badRow * (19 * 8 + 1)] = 5;
        var compressed = AuthoredDeflate.Fixed(raw.Select(value => new AuthoredDeflate.Token(value)), raw);
        if (badChecksum) compressed[^1] ^= 1;
        var encoded = AuthoredPng.Wrap(frame, compressed);
        var peak = encoded.Length + (long)compressed.Length + 19 * 17 * 8 + 256 * 1024 + 19 * 16;
        using var cancellation = new CancellationTokenSource();
        foreach (var work in new[] { 1L, encoded.Length, 10000, 20000, ImageLimits.Default.MaxWork })
        {
            var options = new DecodeOptions { Limits = new ImageLimits { MaxWork = work } };
            var first = Record.Exception(() => { using var value = ImageCodec.Decode(encoded, options); });
            var second = Record.Exception(() => { using var value = ImageCodec.Decode(encoded, options, cancellation.Token); });
            Assert.NotNull(first); Assert.Equal(second?.GetType(), first.GetType()); Assert.Equal(second?.Message, first.Message);
            // The exact working boundary excludes materialization and remains an
            // independent BCL decode path for filter/checksum failure ordering.
            var fallback = Record.Exception(() =>
            {
                using var value = ImageCodec.Decode(encoded,
                    options with { Limits = options.Limits with { MaxWorkingBytes = peak } }, cancellation.Token);
            });
            Assert.Equal(fallback?.GetType(), first.GetType()); Assert.Equal(fallback?.Message, first.Message);
        }
    }
}
