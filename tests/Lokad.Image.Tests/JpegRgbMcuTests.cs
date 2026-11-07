using Lokad.Image.Internal;
using Lokad.Image.Tooling;

namespace Lokad.Image.Tests;

public sealed class JpegRgbMcuTests
{
    [Fact]
    public void PackedMcusMatchIndependentCoordinatesAcrossEqualGridsEdgesAndPadding()
    {
        var random = new Random(6386481);
        foreach (var channels in new[] { 3, 4 })
        for (var horizontal = 1; horizontal <= 3; horizontal++)
        for (var vertical = 1; vertical <= 3; vertical++)
        {
            if (horizontal * vertical * channels > 10) continue;
            var group = horizontal * vertical * 64;
            var source = new byte[channels * group + 12]; random.NextBytes(source); var saved = source.ToArray();
            for (var width = 1; width <= horizontal * 8; width++)
            for (var height = 1; height <= vertical * 8; height++)
            foreach (var padding in new[] { 0, 13 })
            {
                var stride = width * 4 + padding; var length = (height - 1) * stride + width * 4;
                var actual = new byte[length + 11]; random.NextBytes(actual); var expected = actual.ToArray();
                for (var y = 0; y < height; y++)
                for (var x = 0; x < width; x++)
                {
                    var block = (y / 8 * horizontal + x / 8) * 64;
                    var offset = 5 + block + y % 8 * 8 + x % 8;
                    for (var c = 0; c < channels; c++) expected[3 + y * stride + x * 4 + c] = source[offset + c * group];
                    if (channels == 3) expected[3 + y * stride + x * 4 + 3] = 255;
                }
                JpegDecoder.StoreComponentMcu(source.AsSpan(5, channels * group), actual.AsSpan(3, length), stride, width, height, horizontal, vertical, channels);
                Assert.Equal(expected, actual);
            }
            Assert.Equal(saved, source);
        }
    }

    [Fact]
    public void PackingChecksStorageBeforeWritesAndAllocatesNoWorkspace()
    {
        var source = new byte[384]; var output = Enumerable.Repeat((byte)0xa7, 16 * 43).ToArray();
        var saved = output.ToArray();
        Assert.Throws<ArgumentException>(() => JpegDecoder.StoreComponentMcu(source.AsSpan(0, 383), output, 43, 8, 16, 1, 2, 3));
        Assert.Throws<ArgumentException>(() => JpegDecoder.StoreComponentMcu(source, output.AsSpan(0, 676), 43, 8, 16, 1, 2, 3));
        Assert.Throws<ArgumentOutOfRangeException>(() => JpegDecoder.StoreComponentMcu(source, output, 31, 8, 16, 1, 2, 3));
        Assert.Equal(saved, output);
        for (var i = 0; i < 100; i++) JpegDecoder.StoreComponentMcu(source, output, 43, 8, 16, 1, 2, 3);
        var before = GC.GetAllocatedBytesForCurrentThread();
        for (var i = 0; i < 1000; i++) JpegDecoder.StoreComponentMcu(source, output, 43, 8, 16, 1, 2, 3);
        Assert.Equal(0, GC.GetAllocatedBytesForCurrentThread() - before);
    }

    [Fact]
    public void CompleteFramesKeepRestartSeparateScanAndNonunitSamplingPixelsExact()
    {
        foreach (var vertical in new[] { 1, 2 })
        foreach (var separate in new[] { false, true })
        foreach (var restart in new[] { 0, 3 })
        {
            var data = AuthoredJpeg.SequentialRgbFrame(9, 17, vertical, separate, restart);
            var saved = data.ToArray(); using var image = ImageCodec.Decode(data);
            Exact(image); Assert.Equal(saved, data);
        }
    }

    [Fact]
    public void WorkspaceAdmissionIsTerminalAndCancellationPreservesSurvivorsAndRecovery()
    {
        var data = AuthoredJpeg.SequentialRgbFrame(9, 17, 2, false); var saved = data.ToArray();
        using var root = new TestMemoryBudget(16 * 1024 * 1024);
        var options = new DecodeOptions { Limits = new() { MemoryBudget = root } };
        using var first = ImageCodec.Decode(data, options); Exact(first);
        var retained = root.Bytes; var peak = root.Peak; var growths = root.GrowthAttempts;

        // Root capacity is unchanged. A tighter local workspace limit selects
        // the original component-store path before shared admission.
        using (var fallbackRoot = new TestMemoryBudget(16 * 1024 * 1024))
        {
            var limits = new ImageLimits { MemoryBudget = fallbackRoot, MaxWorkingBytes = peak + data.Length - 384 };
            using (var fallback = ImageCodec.Decode(data, new() { Limits = limits })) Exact(fallback);
            Assert.Equal(growths - 1, fallbackRoot.GrowthAttempts);
            Assert.Equal(peak - 384, fallbackRoot.Peak); Assert.Equal(0, fallbackRoot.Bytes);
        }

        // Refuse the last growth, which admits the MCU workspace. No retry or
        // lower-memory execution is allowed after the shared root refuses.
        root.RefuseAtGrowth = root.GrowthAttempts + growths;
        var stopped = root.RefuseAtGrowth;
        var refused = Assert.Throws<ImageLimitException>(() => ImageCodec.Decode(data, options));
        Assert.Equal(ImageLimitScope.SharedBudget, refused.Scope);
        Assert.Equal(stopped, root.GrowthAttempts); Assert.Equal(1, root.Refusals);
        Assert.Equal(retained, root.Bytes); Exact(first);
        root.RefuseAtGrowth = 0;

        using var cancellation = new CancellationTokenSource();
        var cancelAt = root.GrowthAttempts + growths;
        root.Admitted = _ => { if (root.GrowthAttempts == cancelAt) cancellation.Cancel(); };
        var canceled = Assert.ThrowsAny<OperationCanceledException>(() => ImageCodec.Decode(data, options, cancellation.Token));
        Assert.Equal(cancellation.Token, canceled.CancellationToken);
        Assert.Equal(retained, root.Bytes); Exact(first); root.Admitted = null;

        using var recovered = ImageCodec.Decode(data, options); Exact(recovered);
        first.GetRow8(0)[0] ^= 255; first.Dispose(); Exact(recovered);
        Assert.Equal(retained, root.Bytes); recovered.Dispose(); Assert.Equal(0, root.Bytes);
        Assert.False(root.IsDisposed); Assert.Equal(saved, data);
    }

    private static void Exact(ImageBuffer image)
    {
        Assert.Equal(9, image.Width); Assert.Equal(17, image.Height);
        for (var y = 0; y < image.Height; y++)
        {
            var row = image.GetRow8(y);
            for (var x = 0; x < image.Width; x++)
            {
                Assert.Equal((byte)45, row[x * 4]); Assert.Equal((byte)105, row[x * 4 + 1]);
                Assert.Equal((byte)205, row[x * 4 + 2]); Assert.Equal((byte)255, row[x * 4 + 3]);
            }
        }
    }
}
