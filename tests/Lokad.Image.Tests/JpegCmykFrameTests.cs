using System.Buffers.Binary;
using Lokad.Image.Internal;
using Lokad.Image.Tooling;

namespace Lokad.Image.Tests;

[Collection(FilteredResizeStorageCollection.Name)]
public sealed class JpegCmykFrameTests
{
    [Fact]
    public void InPlaceConversionMatchesIndependentQ16ForEverySampleAndBlackPair()
    {
        foreach (var inverted in new[] { false, true })
        {
            var pixels = new byte[65536 * 4 + 17]; pixels.AsSpan().Fill(0xa7);
            var expected = pixels.ToArray();
            for (var k = 0; k < 256; k++)
            for (var sample = 0; sample < 256; sample++)
            {
                var offset = 3 + (k * 256 + sample) * 4;
                pixels[offset] = (byte)sample; pixels[offset + 1] = (byte)(sample ^ 0x5a);
                pixels[offset + 2] = (byte)(255 - sample); pixels[offset + 3] = (byte)k;
                for (var c = 0; c < 3; c++)
                {
                    var value = inverted ? pixels[offset + c] : 255 - pixels[offset + c];
                    var black = inverted ? k : 255 - k;
                    expected[offset + c] = (byte)(value * black * 257 >> 16);
                }
                expected[offset + 3] = 255;
            }
            JpegDecoder.RenderCmykInPlace(pixels.AsSpan(3, 65536 * 4), inverted);
            Assert.Equal(expected, pixels);
        }
        foreach (var inverted in new[] { false, true })
        foreach (var count in new[] { 0, 1, 7, 8, 9, 15, 16, 17, 31, 32, 33 })
        {
            var pixels = Enumerable.Range(0, count * 4).Select(i => (byte)(i * 59)).ToArray();
            var expected = pixels.ToArray();
            for (var i = 0; i < count; i++)
                JpegBlackColor.WriteCmyk(pixels[i * 4], pixels[i * 4 + 1], pixels[i * 4 + 2], pixels[i * 4 + 3], inverted, expected.AsSpan(i * 4, 4), 4);
            JpegDecoder.RenderCmykInPlace(pixels, inverted); Assert.Equal(expected, pixels);
        }
        var invalid = new byte[31]; var saved = invalid.ToArray();
        Assert.Throws<ArgumentException>(() => JpegDecoder.RenderCmykInPlace(invalid, true)); Assert.Equal(saved, invalid);
        var storage = new byte[68];
        for (var i = 0; i < 100; i++) JpegDecoder.RenderCmykInPlace(storage, true);
        var before = GC.GetAllocatedBytesForCurrentThread();
        for (var i = 0; i < 1000; i++) JpegDecoder.RenderCmykInPlace(storage, true);
        Assert.Equal(0, GC.GetAllocatedBytesForCurrentThread() - before);
    }

    [Fact]
    public void OwnedFramesMatchIndependentCallerSampleStorageAcrossRequiredFallbacks()
    {
        byte[][] packets = [
            AuthoredJpegCompatibility.Create(33, 19, [20, 92, 211, 163]),
            AuthoredJpegCompatibility.Create(33, 19, [20, 92, 211, 163], adobe: 0, vary: true, restart: 3),
            AuthoredJpegCompatibility.Create(33, 19, [120, 92, 211, 163], adobe: 2, vary: true),
            AuthoredJpegCompatibility.Create(33, 19, [20, 92, 211, 163], adobe: 0, progressive: true, vary: true),
            AuthoredJpegCompatibility.Create(33, 19, [120, 92, 211, 163], 2, 2, blackFull: true, adobe: 2, vary: true),
            SeparateBlackFirst()];
        foreach (var packet in packets)
        {
            var saved = packet.ToArray(); var info = ImageCodec.Identify(packet);
            var expected = CallerPixels(packet, info.Width, info.Height);
            using var image = ImageCodec.Decode(packet);
            Assert.Equal(expected, image.Pixels.ToArray()); Assert.Equal(saved, packet);
        }
    }

    [Fact]
    public void DirtyResultIsCompletelyOverwrittenAndLiveOutputsRemainIndependent()
    {
        const int width = 33, height = 19, length = width * height * 4;
        var packet = AuthoredJpegCompatibility.Create(width, height, [20, 92, 211, 163], adobe: 0, vary: true);
        var expected = CallerPixels(packet, width, height);
        _ = PixelStoragePool.Shared.Rent(length + 1);
        var dirty = PixelStoragePool.Shared.Rent(length); dirty.AsSpan().Fill(0xa7); PixelStoragePool.Shared.Return(dirty);
        using var first = ImageCodec.Decode(packet);
        Assert.True(first.Pixels.Overlaps(dirty)); Assert.Equal(expected, first.Pixels.ToArray());
        using var second = ImageCodec.Decode(packet); Assert.False(first.Pixels.Overlaps(second.Pixels));
        first.Pixels.Fill(0); first.Dispose(); Assert.Equal(expected, second.Pixels.ToArray());
    }

    [Fact]
    public async Task SharedRefusalCancellationAndAsyncCompletionKeepResultOwnershipAndRecovery()
    {
        const int width = 33, height = 19, length = width * height * 4;
        var packet = AuthoredJpegCompatibility.Create(width, height, [20, 92, 211, 163], adobe: 0, vary: true);
        var expected = CallerPixels(packet, width, height); var saved = packet.ToArray();
        using var root = new TestMemoryBudget(4 * 1024 * 1024);
        var options = new DecodeOptions { Limits = new() { MemoryBudget = root } };
        using var first = ImageCodec.Decode(packet, options); var growths = root.GrowthAttempts;
        Assert.Equal(length, root.Bytes); Assert.Equal(expected, first.Pixels.ToArray());
        root.RefuseAtGrowth = root.GrowthAttempts + growths;
        var stopped = root.RefuseAtGrowth;
        var error = Assert.Throws<ImageLimitException>(() => ImageCodec.Decode(packet, options));
        Assert.Equal(ImageLimitScope.SharedBudget, error.Scope); Assert.Equal(stopped, root.GrowthAttempts);
        Assert.Equal(1, root.Refusals); Assert.Equal(length, root.Bytes); root.RefuseAtGrowth = 0;
        using var cancellation = new CancellationTokenSource();
        var cancelAt = root.GrowthAttempts + growths;
        root.Admitted = _ => { if (root.GrowthAttempts == cancelAt) cancellation.Cancel(); };
        var canceled = Assert.Throws<OperationCanceledException>(() => ImageCodec.Decode(packet, options, cancellation.Token));
        Assert.Equal(cancellation.Token, canceled.CancellationToken); Assert.Equal(length, root.Bytes); root.Admitted = null;
        using var stream = new MemoryStream(packet, writable: false);
        using var second = await ImageCodec.DecodeAsync(stream, options);
        Assert.True(stream.CanRead); Assert.Equal(2L * length, root.Bytes); Assert.False(first.Pixels.Overlaps(second.Pixels));
        first.Dispose(); Assert.Equal(length, root.Bytes); Assert.Equal(expected, second.Pixels.ToArray());
        second.Dispose(); Assert.Equal(0, root.Bytes); Assert.False(root.IsDisposed); Assert.Equal(saved, packet);
    }

    private static byte[] CallerPixels(byte[] packet, int width, int height)
    {
        var samples = new byte[width * height * 3]; ImageCodec.DecodeJpeg(packet, samples, JpegSampleFormat.Rgb);
        var rgba = new byte[width * height * 4];
        for (var i = 0; i < width * height; i++)
        {
            samples.AsSpan(i * 3, 3).CopyTo(rgba.AsSpan(i * 4, 3)); rgba[i * 4 + 3] = 255;
        }
        return rgba;
    }

    private static byte[] SeparateBlackFirst()
    {
        // Authored DC=0/EOB entropy, one scan per component. K arrives before
        // color channels, so any premature opaque-alpha write destroys it.
        var packet = AuthoredJpegCompatibility.Create(8, 8, [128, 128, 128, 128], adobe: 0);
        var mono = AuthoredJpegCompatibility.Create(8, 8, [128]);
        var (start, _) = Scan(packet); var (_, entropy) = Scan(mono);
        using var output = new MemoryStream(); output.Write(packet.AsSpan(0, start));
        foreach (var id in new byte[] { 4, 1, 2, 3 })
        {
            output.Write(new byte[] { 255, 218, 0, 8, 1, id, 0, 0, 63, 0 });
            output.Write(mono.AsSpan(entropy, mono.Length - 2 - entropy));
        }
        output.Write(new byte[] { 255, 217 }); return output.ToArray();

        static (int Marker, int Entropy) Scan(byte[] data)
        {
            for (var p = 2; p < data.Length - 3;)
            {
                var length = BinaryPrimitives.ReadUInt16BigEndian(data.AsSpan(p + 2, 2));
                if (data[p + 1] == 0xda) return (p, p + 2 + length);
                p += 2 + length;
            }
            throw new InvalidOperationException("Authored scan not found.");
        }
    }
}
