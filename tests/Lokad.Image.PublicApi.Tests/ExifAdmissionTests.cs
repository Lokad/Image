using Lokad.Image.Tooling;

namespace Lokad.Image.PublicApi.Tests;

public sealed class ExifAdmissionTests
{
    public static IEnumerable<object[]> Profiles()
    {
        foreach (var profile in Enum.GetValues<MetadataProfile>())
        foreach (var bigEndian in new[] { false, true }) yield return [profile, bigEndian];
    }

    [Theory]
    [MemberData(nameof(Profiles))]
    public void ValidDirectoryAtEntryLimitIsAcceptedAndExhaustionIsALimitError(MetadataProfile profile, bool bigEndian)
    {
        // Enough entries to exceed the complete container's own marker/chunk count.
        // The orientation follows 63 independently authored, unrelated inline tags.
        var fixture = OriginalExif.Create(profile, 9, 7);
        var encoded = OriginalExif.Attach(fixture, ImageOrientation.Rotate90, bigEndian, directoryEntries: 64);
        var unchanged = encoded.ToArray();
        var limits = new ImageLimits { MaxContainerItems = 64 };
        Assert.Equal(ImageOrientation.Rotate90, ImageCodec.Identify(encoded, limits).Orientation);
        using var input = new MemoryStream(encoded);
        Assert.Equal(ImageOrientation.Rotate90, ImageCodec.Identify(input, limits).Orientation);
        Assert.True(input.CanRead);
        using var control = ImageCodec.Decode(fixture.Encoded);
        var expected = OriginalExif.Permute(OriginalPixels.Bytes(control), 9, 7, fixture.Depth, ImageOrientation.Rotate90);
        var options = new DecodeOptions { ApplyOrientation = true, Limits = limits };
        using var decoded = ImageCodec.Decode(encoded, options);
        Assert.Equal(expected, OriginalPixels.Bytes(decoded));
        using var decodeInput = new MemoryStream(encoded);
        using var streamed = ImageCodec.Decode(decodeInput, options);
        Assert.Equal(expected, OriginalPixels.Bytes(streamed)); Assert.True(decodeInput.CanRead);

        var exhausted = limits with { MaxContainerItems = 63 };
        Assert.Throws<ImageLimitException>(() => ImageCodec.Identify(encoded, exhausted));
        using var rejectedIdentify = new MemoryStream(encoded);
        Assert.Throws<ImageLimitException>(() => ImageCodec.Identify(rejectedIdentify, exhausted));
        Assert.True(rejectedIdentify.CanRead);
        Assert.Throws<ImageLimitException>(() => ImageCodec.Decode(encoded, options with { Limits = exhausted }));
        using var rejectedDecode = new MemoryStream(encoded);
        Assert.Throws<ImageLimitException>(() => ImageCodec.Decode(rejectedDecode, options with { Limits = exhausted }));
        Assert.True(rejectedDecode.CanRead);
        Assert.Equal(unchanged, encoded);
    }
}
