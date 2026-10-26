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
    [InlineData(8)] [InlineData(16)]
    public void RollingRgbaRowsPreserveAllFiltersByteOrderOwnersAndExactAdmission(int depth)
    {
        // Both independently authored encodings cross several history slides,
        // split rows across slides, and exercise all five PNG row filters.
        var frame = new AuthoredPng.Frame("rolling-rgba", 6, depth, 257, 139, false);
        var raw = AuthoredPng.FilteredRows(frame);
        foreach (var compressed in new[]
        {
            AuthoredPng.StoredZlib(raw),
            AuthoredDeflate.Fixed(raw.Select(value => new AuthoredDeflate.Token(value)), raw)
        })
        {
            var encoded = AuthoredPng.Wrap(frame, compressed); var original = encoded.ToArray();
            var pixels = (long)frame.Width * frame.Height * (depth / 8) * 4;
            var peak = encoded.Length + (long)compressed.Length + pixels + 256 * 1024 + frame.Width * (depth / 8) * 8;
            var options = new DecodeOptions { Limits = new ImageLimits { MaxWorkingBytes = peak } };
            using var cancellation = new CancellationTokenSource();
            using var plain = ImageCodec.Decode(encoded, options);
            using var live = ImageCodec.Decode(encoded, options, cancellation.Token);
            Assert.Equal(AuthoredPng.Expected(frame), OriginalPixels.Bytes(plain));
            Assert.Equal(AuthoredPng.Expected(frame), OriginalPixels.Bytes(live));
            if (depth == 8) plain.GetRow8(0)[0] ^= 255; else plain.GetRow16(0)[0] ^= 65535;
            Assert.Equal(AuthoredPng.Expected(frame), OriginalPixels.Bytes(live));
            Assert.Equal(original, encoded);
            Assert.Throws<ImageLimitException>(() => ImageCodec.Decode(encoded,
                options with { Limits = options.Limits with { MaxWorkingBytes = peak - 1 } }));
            Assert.Throws<ImageLimitException>(() => ImageCodec.Decode(encoded,
                options with { Limits = options.Limits with { MaxWorkingBytes = peak - 1 } }, cancellation.Token));
        }
    }

    [Theory]
    [InlineData(65536)] [InlineData(65537)]
    public void BoundedFilterStorageAndTallerFallbackKeepOriginalPeak(int height)
    {
        var frame = new AuthoredPng.Frame("rolling-height-boundary", 6, 8, 1, height, false);
        var compressed = AuthoredPng.StoredZlib(AuthoredPng.FilteredRows(frame));
        var encoded = AuthoredPng.Wrap(frame, compressed);
        var peak = encoded.Length + (long)compressed.Length + height * 4L + 256 * 1024 + 8;
        var options = new DecodeOptions { Limits = new ImageLimits { MaxDimension = height, MaxWorkingBytes = peak } };
        using var liveToken = new CancellationTokenSource();
        using var result = ImageCodec.Decode(encoded, options, liveToken.Token);
        Assert.Equal(AuthoredPng.Expected(frame), OriginalPixels.Bytes(result));
        Assert.Throws<ImageLimitException>(() => ImageCodec.Decode(encoded,
            options with { Limits = options.Limits with { MaxWorkingBytes = peak - 1 } }, liveToken.Token));
    }

    [Theory]
    [InlineData(0, false)] [InlineData(0, true)]
    [InlineData(16, false)] [InlineData(16, true)]
    public void FilterAndChecksumFailuresKeepTheOriginalAndErrorOrder(int badRow, bool badChecksum)
    {
        var frame = new AuthoredPng.Frame("fused-errors", 6, 16, 19, 17, false);
        var raw = AuthoredPng.FilteredRows(frame); raw[badRow * (19 * 8 + 1)] = 5;
        var compressed = AuthoredDeflate.Fixed(raw.Select(value => new AuthoredDeflate.Token(value)), raw);
        if (badChecksum) compressed[^1] ^= 1;
        var encoded = AuthoredPng.Wrap(frame, compressed);
        var peak = encoded.Length + (long)compressed.Length + 19 * 17 * 8 + 256 * 1024 + 19 * 16;
        using var cancellation = new CancellationTokenSource();
        {
            var options = new DecodeOptions { Limits = new ImageLimits { } };
            var first = Record.Exception(() => { using var value = ImageCodec.Decode(encoded, options); });
            var second = Record.Exception(() => { using var value = ImageCodec.Decode(encoded, options, cancellation.Token); });
            Assert.NotNull(first); Assert.Equal(second?.GetType(), first.GetType()); Assert.Equal(second?.Message, first.Message);
            // Invalid rows/checksums at the exact boundary must replay the BCL
            // path after framing, preserving filter/checksum failure ordering.
            var fallback = Record.Exception(() =>
            {
                using var value = ImageCodec.Decode(encoded,
                    options with { Limits = options.Limits with { MaxWorkingBytes = peak } }, cancellation.Token);
            });
            Assert.Equal(fallback?.GetType(), first.GetType()); Assert.Equal(fallback?.Message, first.Message);
        }
    }
}
