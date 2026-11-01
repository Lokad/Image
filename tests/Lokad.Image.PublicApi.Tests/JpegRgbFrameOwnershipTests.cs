using Lokad.Image.Tooling;

namespace Lokad.Image.PublicApi.Tests;

public sealed class JpegRgbFrameOwnershipTests
{
    private const int Width = 513, Height = 259, FrameBytes = Width * Height * 4;

    [Theory]
    [InlineData(1, 1, 2, false)]
    [InlineData(17, 11, 1, true)]
    [InlineData(13, 19, 2, true)]
    [InlineData(17, 19, 2, false)]
    public async Task OriginalRgbScansPreserveTailsRestartsAndIndependentStreamOutputs(int width, int height, int vertical, bool separate)
    {
        var data = AuthoredJpeg.SequentialRgbFrame(width, height, vertical, separate, 3); var original = data.ToArray();
        Assert.Equal(SourceColorModel.Rgb, ImageCodec.Identify(data).SourceColor);
        using var first = ImageCodec.Decode(data);
        using var sync = new MemoryStream(data, writable: false);
        using var second = ImageCodec.Decode(sync);
        using var asynchronous = new MemoryStream(data, writable: false);
        using var third = await ImageCodec.DecodeAsync(asynchronous);
        AssertPixels(first, width, height); AssertPixels(second, width, height); AssertPixels(third, width, height);
        first.GetRow8(0)[0] ^= 255; first.Dispose();
        AssertPixels(second, width, height); AssertPixels(third, width, height);
        Assert.Equal(original, data); Assert.True(sync.CanRead); Assert.True(asynchronous.CanRead);
    }

    [Fact]
    public void RetainedFramesShareOneRootWithoutTemporaryFullPlanesAndRecoverAfterRefusal()
    {
        var data = AuthoredJpeg.SequentialRgbFrame(Width, Height, 2, false);
        using var root = new TestMemoryBudget(data.Length + 2L * FrameBytes + 65536);
        Assert.True(root.TryReserve(data.Length, out var input)); using var inputOwner = input;
        var options = new DecodeOptions { Limits = new() { MemoryBudget = root } };
        using var first = ImageCodec.Decode(data, options); using var second = ImageCodec.Decode(data, options);
        Assert.Equal(data.Length + 2L * FrameBytes, root.Bytes);
        var failure = Assert.Throws<ImageLimitException>(() => ImageCodec.Decode(data, options));
        Assert.Equal((ImageLimitKind.WorkingBytes, ImageLimitScope.SharedBudget), (failure.Limit, failure.Scope));
        Assert.Equal(1, root.Refusals); Assert.Equal(data.Length + 2L * FrameBytes, root.Bytes);
        first.Dispose(); Assert.Equal(data.Length + FrameBytes, root.Bytes);
        using var recovered = ImageCodec.Decode(data, options); data.AsSpan().Clear();
        AssertPixels(second, Width, Height); AssertPixels(recovered, Width, Height);
        recovered.Dispose(); second.Dispose(); Assert.Equal(data.Length, root.Bytes);
        inputOwner?.Dispose(); Assert.Equal(0, root.Bytes); Assert.False(root.IsDisposed);
    }

    [Theory]
    [InlineData(false)] [InlineData(true)]
    public void OwnedFrameIsReleasedAfterLaterRefusalOrCancellation(bool cancel)
    {
        var data = AuthoredJpeg.SequentialRgbFrame(Width, Height, 2, false); var original = data.ToArray();
        using var root = new TestMemoryBudget(FrameBytes + 65536);
        using var cancellation = new CancellationTokenSource(); var armed = false;
        root.Admitted = bytes =>
        {
            if (armed || bytes < FrameBytes) return;
            armed = true;
            if (cancel) cancellation.Cancel(); else root.RefuseAtGrowth = root.GrowthAttempts + 1;
        };
        var options = new DecodeOptions { Limits = new() { MemoryBudget = root } };
        if (cancel)
            Assert.Equal(cancellation.Token, Assert.Throws<OperationCanceledException>(() => ImageCodec.Decode(data, options, cancellation.Token)).CancellationToken);
        else
        {
            var failure = Assert.Throws<ImageLimitException>(() => ImageCodec.Decode(data, options));
            Assert.Equal((ImageLimitKind.WorkingBytes, ImageLimitScope.SharedBudget), (failure.Limit, failure.Scope));
            Assert.Equal(1, root.Refusals);
        }
        Assert.True(armed); Assert.Equal(0, root.Bytes); Assert.False(root.IsDisposed); Assert.Equal(original, data);
        root.Admitted = null; root.RefuseAtGrowth = 0;
        using (var recovered = ImageCodec.Decode(data, options)) AssertPixels(recovered, Width, Height);
        Assert.Equal(0, root.Bytes);
    }

    [Fact]
    public void LateDuplicateScanReleasesThePrivateImageBeforeRecovery()
    {
        var original = AuthoredJpeg.SequentialRgbFrame(Width, Height, 2, false);
        byte[] repeat = [255, 218, 0, 8, 1, 82, 0, 0, 63, 0];
        var malformed = original[..^2].Concat(repeat).Concat(new byte[] { 255, 217 }).ToArray();
        using var root = new TestMemoryBudget(FrameBytes + 65536);
        var options = new DecodeOptions { Limits = new() { MemoryBudget = root } };
        Assert.Throws<ImageDataException>(() => ImageCodec.Decode(malformed, options));
        Assert.InRange(root.Peak, FrameBytes, FrameBytes + 65536); Assert.Equal(0, root.Bytes);
        using (var recovered = ImageCodec.Decode(original, options)) AssertPixels(recovered, Width, Height);
        Assert.Equal(0, root.Bytes); Assert.False(root.IsDisposed);
    }

    [Theory]
    [InlineData(false)] [InlineData(true)]
    public void LateYcbcrOrConflictingAdobeDeclarationRetainsTheOriginalColorPath(bool conflicting)
    {
        var data = AuthoredJpeg.SequentialRgbFrame(17, 11, 2, false);
        byte[] adobe = [255, 238, 0, 14, 65, 100, 111, 98, 101, 0, 100, 0, 0, 0, 0, 1];
        if (conflicting)
        {
            var first = adobe.ToArray(); first[^1] = 0;
            data = data[..^2].Concat(first).Concat(adobe).Concat(new byte[] { 255, 217 }).ToArray();
        }
        else data = data[..^2].Concat(adobe).Concat(new byte[] { 255, 217 }).ToArray();
        Assert.Equal(conflicting ? SourceColorModel.Unknown : SourceColorModel.YCbCr, ImageCodec.Identify(data).SourceColor);
        var rgb = new byte[17 * 11 * 3]; ImageCodec.DecodeJpeg(data, rgb, JpegSampleFormat.Rgb);
        using var decoded = ImageCodec.Decode(data); var pixels = OriginalPixels.Bytes(decoded);
        for (var i = 0; i < rgb.Length / 3; i++)
        {
            Assert.Equal(rgb.AsSpan(i * 3, 3).ToArray(), pixels.AsSpan(i * 4, 3).ToArray());
            Assert.Equal(255, pixels[i * 4 + 3]);
        }
    }

    [Fact]
    public void CallerSamplesAndExifOrientationKeepTheirOwnOutputBoundaries()
    {
        var original = AuthoredJpeg.SequentialRgbFrame(17, 11, 2, true);
        var data = OriginalExif.Attach(new(original, null, 17, 11, MetadataProfile.JpegBaseline420), ImageOrientation.Rotate90, true);
        using var root = new TestMemoryBudget(128 * 1024);
        var limits = new ImageLimits { MemoryBudget = root };
        var caller = new byte[17 * 11 * 3];
        ImageCodec.DecodeJpeg(data, caller, JpegSampleFormat.Rgb, options: new() { Limits = limits });
        Assert.Equal(0, root.Bytes);
        for (var i = 0; i < caller.Length; i += 3) Assert.Equal(new byte[] { 45, 105, 205 }, caller.AsSpan(i, 3).ToArray());
        using (var image = ImageCodec.Decode(data, new() { Limits = limits }))
        { Assert.Equal(ImageOrientation.Rotate90, image.Orientation); AssertPixels(image, 17, 11); }
        using (var oriented = ImageCodec.Decode(data, new() { Limits = limits, ApplyOrientation = true }))
        { Assert.Equal(ImageOrientation.Normal, oriented.Orientation); AssertPixels(oriented, 11, 17); }
        Assert.Equal(0, root.Bytes); Assert.False(root.IsDisposed);
    }

    private static void AssertPixels(ImageBuffer image, int width, int height)
    {
        Assert.Equal((width, height, 8), (image.Width, image.Height, image.BitDepth));
        var expected = new byte[width * height * 4];
        for (var i = 0; i < expected.Length; i += 4)
        { expected[i] = 45; expected[i + 1] = 105; expected[i + 2] = 205; expected[i + 3] = 255; }
        Assert.Equal(expected, OriginalPixels.Bytes(image));
    }
}
