using Lokad.Image.Tooling;

namespace Lokad.Image.DifferentialTests;

public sealed class ExifOrientationFfmpegTests
{
    public static IEnumerable<object[]> Profiles()
    {
        foreach (var profile in Enum.GetValues<MetadataProfile>())
        foreach (var orientation in Enum.GetValues<ImageOrientation>())
        foreach (var bigEndian in new[] { false, true }) yield return [profile, orientation, bigEndian];
    }

    [Theory]
    [MemberData(nameof(Profiles))]
    public void ExplicitNativeGeometryMatchesExifAfterColorConversion(MetadataProfile profile, ImageOrientation orientation, bool bigEndian)
    {
        var fixture = OriginalExif.Create(profile, 9, 7);
        var encoded = OriginalExif.Attach(fixture, orientation, bigEndian);
        var nativeRaw = ExifOracle.Decode(encoded, fixture, ImageOrientation.Normal);
        if (!fixture.Lossy) Assert.Equal(fixture.Source, nativeRaw);
        var expectedNative = OriginalExif.Permute(nativeRaw, 9, 7, fixture.Depth, orientation);
        var nativeOriented = ExifOracle.Decode(encoded, fixture, orientation);
        Assert.Equal(expectedNative, nativeOriented);
        using var actual = ImageCodec.Decode(encoded, new DecodeOptions { ApplyOrientation = true });
        Assert.Equal(ImageOrientation.Normal, actual.Orientation);
        Assert.Equal((int)orientation >= 5 ? (7, 9) : (9, 7), (actual.Width, actual.Height));
        ExifOracle.Check(nativeOriented, OriginalPixels.Bytes(actual), fixture.Lossy);
    }
}
