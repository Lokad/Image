using Lokad.Image.Tooling;

namespace Lokad.Image.PublicApi.Tests;

public sealed class ExifOrientationTests
{
    public static IEnumerable<object[]> Profiles()
    {
        foreach (var profile in Enum.GetValues<MetadataProfile>())
        foreach (var orientation in Enum.GetValues<ImageOrientation>())
        foreach (var bigEndian in new[] { false, true }) yield return [profile, orientation, bigEndian];
    }

    [Theory]
    [MemberData(nameof(Profiles))]
    public void IdentifyAndDecodeRespectEveryOrientationAndByteOrder(MetadataProfile profile, ImageOrientation orientation, bool bigEndian)
    {
        var fixture = OriginalExif.Create(profile, 9, 7);
        var encoded = OriginalExif.Attach(fixture, orientation, bigEndian);
        var (width, height) = (int)orientation >= 5 ? (7, 9) : (9, 7);
        var expectedInfo = new ImageInfo(fixture.Format, 9, 7, fixture.Depth, fixture.Alpha, orientation);
        var info = ImageCodec.Identify(encoded); Assert.Equal(expectedInfo, info);
        Assert.Equal((width, height), (info.OrientedWidth, info.OrientedHeight));
        using var identifyInput = new MemoryStream(encoded);
        Assert.Equal(expectedInfo, ImageCodec.Identify(identifyInput)); Assert.True(identifyInput.CanRead);
        using var control = ImageCodec.Decode(fixture.Encoded);
        var controlPixels = OriginalPixels.Bytes(control);
        if (!fixture.Lossy) Assert.Equal(fixture.Source, controlPixels);
        using var raw = ImageCodec.Decode(encoded);
        Assert.Equal((9, 7, fixture.Depth, orientation), (raw.Width, raw.Height, raw.BitDepth, raw.Orientation));
        Assert.Equal(controlPixels, OriginalPixels.Bytes(raw));
        var expected = OriginalExif.Permute(controlPixels, 9, 7, fixture.Depth, orientation);
        using var oriented = ImageCodec.Decode(encoded, new DecodeOptions { ApplyOrientation = true });
        Assert.Equal((width, height, fixture.Depth, ImageOrientation.Normal), (oriented.Width, oriented.Height, oriented.BitDepth, oriented.Orientation));
        Assert.Equal(expected, OriginalPixels.Bytes(oriented));
        using var decodeInput = new MemoryStream(encoded);
        using var streamResult = ImageCodec.Decode(decodeInput, new DecodeOptions { ApplyOrientation = true });
        Assert.Equal(expected, OriginalPixels.Bytes(streamResult)); Assert.True(decodeInput.CanRead);
        using var twice = ImageTransforms.AutoOrient(oriented);
        Assert.Equal(expected, OriginalPixels.Bytes(twice));
    }

    public static IEnumerable<object[]> LiteralGrids()
    {
        byte[][] orders = [[1,2,3,4,5,6], [3,2,1,6,5,4], [6,5,4,3,2,1], [4,5,6,1,2,3],
            [1,4,2,5,3,6], [4,1,5,2,6,3], [6,3,5,2,4,1], [3,6,2,5,1,4]];
        for (var orientation = 1; orientation <= 8; orientation++)
        foreach (var depth in new[] { 8, 16 }) yield return [(ImageOrientation)orientation, depth, orders[orientation - 1]];
    }

    [Theory]
    [MemberData(nameof(LiteralGrids))]
    public void IndependentForwardReferenceAgreesWithLiteralGrid(ImageOrientation orientation, int depth, byte[] order)
    {
        var size = depth / 8; var source = new byte[6 * 4 * size]; var expected = new byte[source.Length];
        for (var pixel = 0; pixel < 6; pixel++)
        for (var channelByte = 0; channelByte < 4 * size; channelByte++)
        {
            source[pixel * 4 * size + channelByte] = (byte)((pixel + 1) * 17 + channelByte);
            expected[pixel * 4 * size + channelByte] = (byte)(order[pixel] * 17 + channelByte);
        }
        Assert.Equal(expected, OriginalExif.Permute(source, 3, 2, depth, orientation));
    }
}
