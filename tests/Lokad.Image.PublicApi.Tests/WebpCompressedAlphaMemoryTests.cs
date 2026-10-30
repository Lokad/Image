using System.Security.Cryptography;
using System.Text.Json;
using Lokad.Image.Tooling;
using Xunit.Abstractions;

namespace Lokad.Image.PublicApi.Tests;

public sealed class WebpCompressedAlphaMemoryTests(ITestOutputHelper output)
{
    [Fact]
    public void CompressedAlphaFitsTwoRgbaFramesAndReleasesCapacityForRecovery()
    {
        var fixture = OriginalExif.Create(MetadataProfile.WebpLossyAlpha, 513, 259);
        var unchanged = fixture.Encoded.ToArray();
        var frameBytes = fixture.Width * fixture.Height * 4;
        using var root = new TestMemoryBudget(2L * frameBytes);
        var limits = new ImageLimits { MemoryBudget = root, MaxWorkingBytes = unchanged.Length + 2L * frameBytes };
        var options = new DecodeOptions { Limits = limits };
        using var first = ImageCodec.Decode(fixture.Encoded, options);
        var firstPeak = root.Peak;
        Assert.Equal(frameBytes, root.Bytes);
        for (var y = 0; y < first.Height; y++)
            for (var x = 0; x < first.Width; x++)
                Assert.Equal(fixture.Source![(y * first.Width + x) * 4 + 3], first.GetRow8(y)[x * 4 + 3]);
        var expected = OriginalPixels.Bytes(first);
        var refusal = Assert.Throws<ImageLimitException>(() => ImageCodec.Decode(fixture.Encoded, options));
        Assert.Equal((ImageLimitKind.WorkingBytes, ImageLimitScope.SharedBudget), (refusal.Limit, refusal.Scope));
        Assert.Equal(1, root.Refusals);
        Assert.Equal(frameBytes, root.Bytes);
        Assert.Equal(expected, OriginalPixels.Bytes(first));
        first.Dispose();
        Assert.Equal(0, root.Bytes);
        using var recovered = ImageCodec.Decode(fixture.Encoded, options);
        Assert.Equal(expected, OriginalPixels.Bytes(recovered));
        Assert.Equal(frameBytes, root.Bytes);
        recovered.Dispose();
        Assert.Equal(0, root.Bytes);
        Assert.False(root.IsDisposed);
        Assert.Equal(unchanged, fixture.Encoded);
        output.WriteLine(JsonSerializer.Serialize(new { fixture.Width, fixture.Height,
            encodedSha256 = Convert.ToHexStringLower(SHA256.HashData(fixture.Encoded)),
            firstPeakAdmittedBytes = firstPeak, allowedBytes = 2L * frameBytes,
            retainedBytes = frameBytes, terminalRefusals = root.Refusals }));
    }
}
