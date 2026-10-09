using Lokad.Image.Internal;

namespace Lokad.Image.Tests;

public sealed class WebpPrimitiveTests
{
    [Fact]
    public void BitFieldsAndCanonicalPrefixBitsHaveIndependentExpectations()
    {
        var reader = new WebpBitReader(new byte[] { 0x1A }, Context());
        var tree = new WebpHuffman(new byte[] { 1, 2, 2 }, Context());
        Assert.Equal(0, tree.Read(ref reader)); Assert.Equal(1, tree.Read(ref reader)); Assert.Equal(2, tree.Read(ref reader));
        var single = new WebpHuffman(new byte[] { 0, 0, 1 }, Context());
        Assert.Equal(2, single.Read(ref reader)); Assert.Equal(0, reader.Read(3));
        Assert.Throws<ImageDataException>(() => new WebpHuffman(new byte[] { 1, 1, 1 }, Context()));
        Assert.Throws<ImageDataException>(() => new WebpHuffman(new byte[] { 2, 2 }, Context()));
        Assert.Throws<ImageDataException>(() => new WebpHuffman(new byte[] { 0, 0 }, Context()));
        Assert.Throws<ImageDataException>(() => new WebpHuffman(new byte[] { 2 }, Context()));
    }

    [Fact]
    public void DistanceGeometryAgreesWithNormativeNeighborhoodPositions()
    {
        Assert.Equal(new[] { 100, 1, 101, 99, 200, 2 }, Enumerable.Range(1, 6).Select(code => WebpDistances.Resolve(code, 100)));
        Assert.Equal(500, WebpDistances.Resolve(35, 100)); Assert.Equal(403, WebpDistances.Resolve(36, 100));
        Assert.Equal(296, WebpDistances.Resolve(39, 100)); Assert.Equal(5, WebpDistances.Resolve(40, 100));
        Assert.Equal(708, WebpDistances.Resolve(120, 100)); Assert.Equal(1, WebpDistances.Resolve(121, 100));
        Assert.Equal(1, WebpDistances.Resolve(4, 1));
    }

    [Fact]
    public void PredictorsAndChannelAdditionHaveHandComputedResults()
    {
        const uint left = 0x64502814, top = 0x5064503C, corner = 0x1E145064, right = 0x78782828;
        Assert.Equal(left, WebpLosslessDecoder.Predict(1, left, top, corner, right));
        Assert.Equal(0x5A5A3C28u, WebpLosslessDecoder.Predict(7, left, top, corner, right));
        Assert.Equal(0x96A02800u, WebpLosslessDecoder.Predict(12, left, top, corner, right));
        Assert.Equal(0x780D051Cu, WebpLosslessDecoder.Add(0xFAFFFEFD, 0x7E0E071F));
        Assert.Equal(0xFF000000u, WebpLosslessDecoder.Predict(0, left, top, corner, right));
    }

    private static OperationContext Context() => new(ImageLimits.Default, CancellationToken.None);
}
