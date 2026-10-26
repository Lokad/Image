using Lokad.Image.Tooling;

namespace Lokad.Image.PublicApi.Tests;

public sealed class JpegTests
{
    [Theory]
    [InlineData(false)] [InlineData(true)]
    public void ExplicitRgbJpegDoesNotApplyYCbCrConversion(bool adobe)
    {
        using var decoded = ImageCodec.Decode(AuthoredJpeg.DirectRgb(adobe));
        for (var y = 0; y < 8; y++)
        for (var x = 0; x < 8; x++) Assert.Equal(new byte[] { 45, 105, 205, 255 }, decoded.GetRow8(y).Slice(x * 4, 4).ToArray());
    }

    [Fact]
    public void BaselineRequiresEightBitQuantizationRepresentation()
    {
        var original = AuthoredJpeg.DirectRgb(adobe: false);
        using var rewritten = new MemoryStream();
        rewritten.Write(original.AsSpan(0, 2));
        rewritten.Write(new byte[] { 255, 219, 0, 131, 0x10 });
        for (var i = 0; i < 64; i++) rewritten.Write(new byte[] { 0, 1 });
        rewritten.Write(original.AsSpan(71));
        Assert.Throws<ImageDataException>(() => ImageCodec.Decode(rewritten.ToArray()));
    }

    [Theory]
    [InlineData(false)] [InlineData(true)]
    public void ProgressiveRefinementHasExactNativePixels(bool negativeDc)
    {
        var data = AuthoredJpeg.RefinedBlock(negativeDc);
        using var decoded = ImageCodec.Decode(data);
        var expected = JpegFixtureTests.NativePixels(negativeDc ? "jpeg-progressive-gray-negative" : "jpeg-progressive-gray-positive", data);
        for (var y = 0; y < 8; y++) Assert.Equal(expected.AsSpan(y * 32, 32).ToArray(), decoded.GetRow8(y).ToArray());
    }

    [Theory]
    [InlineData(0)] [InlineData(1)]
    public void ProgressiveEobRunsAndRestartsHaveExactNativePixels(int restartInterval)
    {
        var data = AuthoredJpeg.EobRun(restartInterval);
        using var decoded = ImageCodec.Decode(data);
        var expected = JpegFixtureTests.NativePixels($"jpeg-progressive-eob-restart{restartInterval}", data);
        for (var y = 0; y < 8; y++) Assert.Equal(expected.AsSpan(y * 96, 96).ToArray(), decoded.GetRow8(y).ToArray());
    }

    [Fact]
    public void ProgressiveEobRunCannotCrossRestartBoundary()
        => Assert.Throws<ImageDataException>(() => ImageCodec.Decode(AuthoredJpeg.EobRun(1, crossRestart: true)));

    [Fact]
    public void FirstAndRefinementEobRunsCoverMultipleEmptyBlocks()
    {
        using var decoded = ImageCodec.Decode(AuthoredJpeg.EmptyEobRun());
        for (var y = 0; y < 8; y++)
        for (var x = 0; x < 24; x++) Assert.Equal(new byte[] { 128, 128, 128, 255 }, decoded.GetRow8(y).Slice(x * 4, 4).ToArray());
        Assert.Throws<ImageDataException>(() => ImageCodec.Decode(AuthoredJpeg.EmptyEobRun(exceedsScan: true)));
    }

    [Fact]
    public void SplitSpectralBandsAndRefinementZeroRunsHaveExactNativePixels()
    {
        var data = AuthoredJpeg.SparseBands();
        using var decoded = ImageCodec.Decode(data);
        var expected = JpegFixtureTests.NativePixels("jpeg-progressive-sparse-bands", data);
        for (var y = 0; y < 8; y++) Assert.Equal(expected.AsSpan(y * 32, 32).ToArray(), decoded.GetRow8(y).ToArray());
    }

    [Fact]
    public void ProgressiveMutationsStayWithinDocumentedFailuresAndLimits()
    {
        var original = AuthoredJpeg.RefinedBlock();
        var options = new DecodeOptions { Limits = new ImageLimits { MaxPixels = 1024, MaxDimension = 32, MaxWorkingBytes = 128 * 1024 } };
        for (var position = 0; position < original.Length; position++)
        foreach (var mask in new byte[] { 1, 0x80, 0xFF })
        {
            var mutated = original.ToArray(); mutated[position] ^= mask;
            try
            {
                using var decoded = ImageCodec.Decode(mutated, options);
                Assert.InRange(decoded.Width, 1, 32); Assert.InRange(decoded.Height, 1, 32);
                Assert.Equal(8, decoded.BitDepth);
            }
            catch (ImageDataException) { }
            catch (UnsupportedImageException) { }
            catch (ImageLimitException) { }
        }
    }

    [Theory]
    [InlineData(JpegSampling.Grayscale)] [InlineData(JpegSampling.YCbCr444)]
    [InlineData(JpegSampling.YCbCr422)] [InlineData(JpegSampling.YCbCr420)]
    public void UniformColorRoundTripRetainsGeometryAndOpaqueOutput(JpegSampling sampling)
    {
        using var source = ImageBuffer.Create(19, 11, 8);
        for (var y = 0; y < source.Height; y++)
        for (var x = 0; x < source.Width; x++) new byte[] { 138, 138, 138, 255 }.CopyTo(source.GetRow8(y)[(x * 4)..]);
        using var encoded = new MemoryStream();
        ImageCodec.EncodeJpeg(source, encoded, new JpegEncodeOptions { Sampling = sampling, Quality = 100, RestartInterval = 1 });
        var info = ImageCodec.Identify(encoded.ToArray()); Assert.Equal(ImageFormat.Jpeg, info.Format);
        using var decoded = ImageCodec.Decode(encoded.ToArray());
        Assert.Equal((19, 11, 8), (decoded.Width, decoded.Height, decoded.BitDepth));
        for (var y = 0; y < decoded.Height; y++)
        for (var x = 0; x < decoded.Width; x++) Assert.Equal(new byte[] { 138, 138, 138, 255 }, decoded.GetRow8(y).Slice(x * 4, 4).ToArray());
    }

    [Theory]
    [InlineData(8)] [InlineData(16)]
    public void TransparentPixelsRequireAnExplicitBackground(int depth)
    {
        using var source = ImageBuffer.Create(1, 1, depth);
        if (depth == 8) new byte[] { 255, 0, 0, 0 }.CopyTo(source.GetRow8(0));
        else new ushort[] { 65535, 0, 0, 0 }.CopyTo(source.GetRow16(0));
        using var refused = new MemoryStream();
        Assert.Throws<ArgumentException>(() => ImageCodec.EncodeJpeg(source, refused));
        using var encoded = new MemoryStream();
        ImageCodec.EncodeJpeg(source, encoded, new JpegEncodeOptions { BackgroundRgb = 0xFFFFFF, Quality = 100 });
        using var decoded = ImageCodec.Decode(encoded.ToArray());
        Assert.Equal(new byte[] { 255, 255, 255, 255 }, decoded.GetRow8(0).ToArray());
    }

    [Fact]
    public void LimitsAndCorruptionFailThroughTheDocumentedBoundary()
    {
        using var source = ImageBuffer.Create(9, 7, 8);
        for (var y = 0; y < source.Height; y++)
        for (var x = 0; x < source.Width; x++) source.GetRow8(y)[x * 4 + 3] = 255;
        using var encoded = new MemoryStream(); ImageCodec.EncodeJpeg(source, encoded);
        var bytes = encoded.ToArray();
        for (var length = 2; length < bytes.Length; length++)
            Assert.Throws<ImageDataException>(() => ImageCodec.Decode(bytes.AsSpan(0, length)));
        Assert.Throws<ImageLimitException>(() => ImageCodec.Decode(bytes, new DecodeOptions { Limits = new ImageLimits { MaxWorkingBytes = bytes.Length + 1024 } }));
        using var output = new MemoryStream();
        Assert.Throws<ImageLimitException>(() => ImageCodec.EncodeJpeg(source, output, new JpegEncodeOptions { Limits = new ImageLimits { MaxOutputBytes = 1 } }));
        Assert.Throws<OperationCanceledException>(() => ImageCodec.Decode(bytes, cancellationToken: new CancellationToken(true)));
    }
}
