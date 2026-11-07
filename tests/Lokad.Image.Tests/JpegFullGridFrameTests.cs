using System.Security.Cryptography;
using Lokad.Image.Internal;
using Lokad.Image.Tooling;

namespace Lokad.Image.Tests;

[Collection(FilteredResizeStorageCollection.Name)]
public sealed class JpegFullGridFrameTests
{
    [Fact]
    public void InPlaceColorMatchesIndependentPinnedQ13AcrossBytePairsAndClippingEdges()
    {
        foreach (var cr in new[] { 0, 1, 127, 128, 129, 254, 255 })
        {
            var pixels = Enumerable.Repeat((byte)0xa7, 65536 * 4 + 17).ToArray();
            var expected = pixels.ToArray();
            for (var y = 0; y < 256; y++)
            for (var cb = 0; cb < 256; cb++)
            {
                var at = 3 + (y * 256 + cb) * 4;
                pixels[at] = (byte)y; pixels[at + 1] = (byte)cb; pixels[at + 2] = (byte)cr;
                // Independently expanded BT.601 coefficients after the pinned
                // studio-chroma/full-range rounding boundaries, before Q13.
                var luma = ((long)y << 13) + 4096; var u = cb - 128; var v = cr - 128;
                expected[at] = (byte)Math.Clamp((luma + v * 11485) >> 13, 0, 255);
                expected[at + 1] = (byte)Math.Clamp((luma - u * 2819 - v * 5850) >> 13, 0, 255);
                expected[at + 2] = (byte)Math.Clamp((luma + u * 14516) >> 13, 0, 255); expected[at + 3] = 255;
            }
            JpegInverseColor.WriteInPlace(pixels.AsSpan(3, 65536 * 4)); Assert.Equal(expected, pixels);
        }
        foreach (var count in new[] { 0, 1, 7, 8, 9, 17, 31 })
        {
            var y = Enumerable.Range(0, count).Select(i => (byte)(i * 73)).ToArray();
            var u = Enumerable.Range(0, count).Select(i => (byte)(i * 107 + 1)).ToArray();
            var v = Enumerable.Range(0, count).Select(i => (byte)(i * 37 + 255)).ToArray();
            var actual = new byte[count * 4]; var expected = new byte[count * 4];
            for (var i = 0; i < count; i++) { actual[i * 4] = y[i]; actual[i * 4 + 1] = u[i]; actual[i * 4 + 2] = v[i]; }
            JpegInverseColor.WriteRow(y, u, v, expected, 4); JpegInverseColor.WriteInPlace(actual); Assert.Equal(expected, actual);
        }
        var invalid = new byte[31]; var saved = invalid.ToArray();
        Assert.Throws<ArgumentException>(() => JpegInverseColor.WriteInPlace(invalid)); Assert.Equal(saved, invalid);
        var storage = new byte[68];
        for (var i = 0; i < 100; i++) JpegInverseColor.WriteInPlace(storage);
        var before = GC.GetAllocatedBytesForCurrentThread();
        for (var i = 0; i < 1000; i++) JpegInverseColor.WriteInPlace(storage);
        Assert.Equal(0, GC.GetAllocatedBytesForCurrentThread() - before);
    }

    [Fact]
    public void InPlaceColorReproducesCompleteFrozenNativeQualifiedImage()
    {
        var planes = JpegReferenceInverseTests.BaselinePlanes();
        var actual = new byte[713 * 4];
        for (var x = 0; x < 713; x++)
        {
            actual[x * 4] = planes[x]; actual[x * 4 + 1] = planes[713 + x]; actual[x * 4 + 2] = planes[1426 + x];
        }
        for (var y = 0; y < 23; y++) JpegInverseColor.WriteInPlace(actual.AsSpan(y * 31 * 4, 31 * 4));
        Assert.Equal("fbf313b3c7bc442832b67d3b08bf228da503aefc142d4ac2ed8d2bfc720b76e4", Convert.ToHexStringLower(SHA256.HashData(actual)));
    }

    [Fact]
    public void CompleteFramesPreserveSeparateRestartedNonunitAndLateColorInterpretation()
    {
        var untagged = AuthoredJpegCompatibility.Create(33, 19, [45, 105, 205], vary: true);
        Assert.Equal(SourceColorModel.Unknown, ImageCodec.Identify(untagged).SourceColor);
        var expectedUntagged = CallerPixels(untagged, 33, 19);
        using (var decoded = ImageCodec.Decode(untagged)) Assert.Equal(expectedUntagged, decoded.Pixels.ToArray());
        foreach (var vertical in new[] { 1, 2 })
        foreach (var separate in new[] { false, true })
        foreach (var restart in new[] { 0, 3 })
        foreach (var adobe in new[] { 0, 1 })
        {
            var packet = Packet(9, 17, vertical, separate, restart, adobe); var saved = packet.ToArray();
            var expected = CallerPixels(packet, 9, 17); using var image = ImageCodec.Decode(packet);
            Assert.Equal(expected, image.Pixels.ToArray()); Assert.Equal(saved, packet);
        }
    }

    [Fact]
    public void DirtyResultIsCompletelyOverwrittenAndLiveOutputsRemainIndependent()
    {
        const int width = 33, height = 19, length = width * height * 4;
        var packet = Packet(width, height, 2, false, 3, 1); var expected = CallerPixels(packet, width, height);
        _ = PixelStoragePool.Shared.Rent(length + 1);
        var dirty = PixelStoragePool.Shared.Rent(length); dirty.AsSpan().Fill(0xa7); PixelStoragePool.Shared.Return(dirty);
        using var first = ImageCodec.Decode(packet);
        Assert.True(first.Pixels.Overlaps(dirty)); Assert.Equal(expected, first.Pixels.ToArray());
        using var second = ImageCodec.Decode(packet); Assert.False(first.Pixels.Overlaps(second.Pixels));
        first.Pixels.Fill(0); first.Dispose(); Assert.Equal(expected, second.Pixels.ToArray());
    }

    [Fact]
    public async Task SharedAdmissionCancellationAndAsyncCompletionKeepOwnershipAndRecovery()
    {
        const int width = 33, height = 19, length = width * height * 4, workspace = 384;
        var packet = Packet(width, height, 2, false, 3, 1); var expected = CallerPixels(packet, width, height); var saved = packet.ToArray();
        using var root = new TestMemoryBudget(4 * 1024 * 1024);
        var options = new DecodeOptions { Limits = new() { MemoryBudget = root } };
        using var first = ImageCodec.Decode(packet, options); var growths = root.GrowthAttempts; var peak = root.Peak;
        Assert.Equal(length, root.Bytes); Assert.Equal(expected, first.Pixels.ToArray());
        using (var fallbackRoot = new TestMemoryBudget(4 * 1024 * 1024))
        {
            var limits = new ImageLimits { MemoryBudget = fallbackRoot, MaxWorkingBytes = peak + packet.Length - workspace };
            using (var fallback = ImageCodec.Decode(packet, new() { Limits = limits })) Assert.Equal(expected, fallback.Pixels.ToArray());
            Assert.Equal(growths - 1, fallbackRoot.GrowthAttempts); Assert.Equal(peak - workspace, fallbackRoot.Peak); Assert.Equal(0, fallbackRoot.Bytes);
        }
        root.RefuseAtGrowth = root.GrowthAttempts + growths; var stopped = root.RefuseAtGrowth;
        var error = Assert.Throws<ImageLimitException>(() => ImageCodec.Decode(packet, options));
        Assert.Equal(ImageLimitScope.SharedBudget, error.Scope); Assert.Equal(stopped, root.GrowthAttempts);
        Assert.Equal(1, root.Refusals); Assert.Equal(length, root.Bytes); root.RefuseAtGrowth = 0;
        using var cancellation = new CancellationTokenSource(); var cancelAt = root.GrowthAttempts + growths;
        root.Admitted = _ => { if (root.GrowthAttempts == cancelAt) cancellation.Cancel(); };
        var canceled = Assert.Throws<OperationCanceledException>(() => ImageCodec.Decode(packet, options, cancellation.Token));
        Assert.Equal(cancellation.Token, canceled.CancellationToken); Assert.Equal(length, root.Bytes); root.Admitted = null;
        using var stream = new MemoryStream(packet, writable: false); using var second = await ImageCodec.DecodeAsync(stream, options);
        Assert.True(stream.CanRead); Assert.Equal(2L * length, root.Bytes); Assert.False(first.Pixels.Overlaps(second.Pixels));
        first.Dispose(); Assert.Equal(length, root.Bytes); Assert.Equal(expected, second.Pixels.ToArray());
        second.Dispose(); Assert.Equal(0, root.Bytes); Assert.False(root.IsDisposed); Assert.Equal(saved, packet);
    }

    private static byte[] Packet(int width, int height, int vertical, bool separate, int restart, int adobe)
    {
        var data = AuthoredJpeg.SequentialRgbFrame(width, height, vertical, separate, restart);
        // Use the existing analytical entropy/sampling; final Adobe metadata
        // selects RGB or YCbCr after all scans, exercising private raw storage.
        byte[] marker = [255, 238, 0, 14, 65, 100, 111, 98, 101, 0, 100, 0, 0, 0, 0, (byte)adobe];
        return data[..^2].Concat(marker).Concat(new byte[] { 255, 217 }).ToArray();
    }

    private static byte[] CallerPixels(byte[] packet, int width, int height)
    {
        var samples = new byte[width * height * 3]; ImageCodec.DecodeJpeg(packet, samples, JpegSampleFormat.Rgb);
        var rgba = new byte[width * height * 4];
        for (var i = 0; i < width * height; i++)
        { samples.AsSpan(i * 3, 3).CopyTo(rgba.AsSpan(i * 4, 3)); rgba[i * 4 + 3] = 255; }
        return rgba;
    }
}
