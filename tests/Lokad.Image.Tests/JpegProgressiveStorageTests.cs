using Lokad.Image.Internal;
using Lokad.Image.Tooling;

namespace Lokad.Image.Tests;

[Collection(FilteredResizeStorageCollection.Name)]
public sealed class JpegProgressiveStorageTests
{
    [Theory]
    [InlineData(1, 1, false)]
    [InlineData(17, 11, true)]
    [InlineData(31, 23, false)]
    [InlineData(513, 259, false)]
    [InlineData(513, 259, true)]
    public void DirtyResultAndStripTailsMatchCallerSamplesWithoutSharingLiveStorage(int width, int height, bool rgb)
        => VerifyDirtyResult(Input(width, height, rgb), width, height);

    [Theory]
    [InlineData("progressive-gray", 1, 1)]
    [InlineData("progressive-gray", 17, 11)]
    [InlineData("baseline-gray", 31, 23)]
    [InlineData("progressive-420", 17, 11)]
    [InlineData("progressive-422", 31, 23)]
    [InlineData("rgb-422", 17, 11)]
    [InlineData("rgb-420", 31, 23)]
    [InlineData("quarter-411", 65, 19)]
    [InlineData("quarter-410", 65, 19)]
    public void CompletedPlaneWritersOverwriteDirtyPixelsAndKeepLiveResultsIndependent(string path, int width, int height)
        => VerifyDirtyResult(CompletedInput(path, width, height), width, height);

    private static void VerifyDirtyResult(byte[] data, int width, int height)
    {
        var source = data.ToArray();
        var expected = CallerPixels(data, width, height); var dirty = Poison(expected.Length);
        using var first = ImageCodec.Decode(data);
        Assert.True(first.Pixels.Overlaps(dirty));
        Assert.Equal(expected, first.Pixels.ToArray());
        using var cancellation = new CancellationTokenSource();
        using var second = ImageCodec.Decode(data, cancellationToken: cancellation.Token);
        Assert.False(first.Pixels.Overlaps(second.Pixels));
        Assert.Equal(expected, second.Pixels.ToArray());
        first.Pixels.Fill(0x3c); first.Dispose();
        using var next = ImageCodec.Decode(data);
        Assert.True(next.Pixels.Overlaps(dirty));
        Assert.Equal(expected, next.Pixels.ToArray());
        Assert.Equal(expected, second.Pixels.ToArray());
        Assert.Equal(source, data);
    }

    [Theory]
    [InlineData(false)] [InlineData(true)]
    public async Task GovernedResultBypassesPoolAndPreservesRefusalCancellationAndAsyncSurvivors(bool rgb)
        => await VerifyGovernedResult(Input(513, 259, rgb));

    [Theory]
    [InlineData("progressive-gray")]
    [InlineData("progressive-420")]
    [InlineData("progressive-422")]
    [InlineData("quarter-410")]
    public async Task CompletedPlaneAdmissionAndAsyncLifetimesBypassIdleStorage(string path)
        => await VerifyGovernedResult(CompletedInput(path, 513, 259));

    private static async Task VerifyGovernedResult(byte[] data)
    {
        const int width = 513, height = 259, outputBytes = width * height * 4;
        var expected = CallerPixels(data, width, height);
        var dirty = Poison(outputBytes);
        long ProbeGrowth(CancellationToken token)
        {
            using var probe = new TestMemoryBudget(4 * 1024 * 1024);
            using (var image = ImageCodec.Decode(data, new() { Limits = new() { MemoryBudget = probe } }, token))
            {
                Assert.False(image.Pixels.Overlaps(dirty)); Assert.Equal(outputBytes, probe.Bytes);
            }
            Assert.Equal(0, probe.Bytes); return probe.GrowthAttempts;
        }
        var outputGrowth = ProbeGrowth(default);
        // Ordinary baseline scans may prepare different lookup storage when
        // cancellation is enabled. Target the actual mode's last admission.
        using var probeCancellation = new CancellationTokenSource();
        var cancelGrowth = ProbeGrowth(probeCancellation.Token);
        using var root = new TestMemoryBudget(4 * 1024 * 1024);
        var options = new DecodeOptions { Limits = new() { MemoryBudget = root } };
        root.RefuseAtGrowth = outputGrowth;
        var error = Assert.Throws<ImageLimitException>(() => ImageCodec.Decode(data, options));
        Assert.Equal(ImageLimitScope.SharedBudget, error.Scope);
        Assert.Equal(outputGrowth, root.GrowthAttempts); Assert.Equal(1, root.Refusals);
        Assert.Equal(0, root.Bytes); root.RefuseAtGrowth = 0;

        using var canceled = new CancellationTokenSource();
        var stop = root.GrowthAttempts + cancelGrowth;
        root.Admitted = _ => { if (root.GrowthAttempts == stop) canceled.Cancel(); };
        Assert.Equal(canceled.Token, Assert.Throws<OperationCanceledException>(() => ImageCodec.Decode(data, options, canceled.Token)).CancellationToken);
        Assert.Equal(0, root.Bytes); root.Admitted = null;

        using var first = ImageCodec.Decode(data, options);
        using var stream = new MemoryStream(data, writable: false);
        using var second = await ImageCodec.DecodeAsync(stream, options);
        Assert.Equal(2L * outputBytes, root.Bytes); Assert.True(stream.CanRead);
        Assert.False(first.Pixels.Overlaps(second.Pixels));
        Assert.False(first.Pixels.Overlaps(dirty)); Assert.False(second.Pixels.Overlaps(dirty));
        first.Pixels.Fill(0); first.Dispose(); data.AsSpan().Clear();
        Assert.Equal(outputBytes, root.Bytes); Assert.Equal(expected, second.Pixels.ToArray());
        Assert.Same(dirty, PixelStoragePool.Shared.Rent(outputBytes)); PixelStoragePool.Shared.Return(dirty);
        second.Dispose(); Assert.Equal(0, root.Bytes); Assert.False(root.IsDisposed);
    }

    private static byte[] CallerPixels(byte[] data, int width, int height)
    {
        var caller = new byte[width * height * 3];
        ImageCodec.DecodeJpeg(data, caller, JpegSampleFormat.Rgb);
        var expected = new byte[width * height * 4];
        for (var i = 0; i < width * height; i++)
        {
            caller.AsSpan(i * 3, 3).CopyTo(expected.AsSpan(i * 4, 3)); expected[i * 4 + 3] = 255;
        }
        return expected;
    }

    private static byte[] Input(int width, int height, bool rgb)
    {
        var data = AuthoredJpeg.ProgressiveFrame(width, height, JpegSampling.YCbCr444, restartInterval: 3);
        return rgb ? AdobeRgb(data) : data;
    }

    private static byte[] CompletedInput(string path, int width, int height) => path switch
    {
        "progressive-gray" => AuthoredJpeg.ProgressiveFrame(width, height, JpegSampling.Grayscale, restartInterval: 3),
        "baseline-gray" => AuthoredJpeg.SequentialFrame(width, height, JpegSampling.Grayscale, restartInterval: 3),
        "progressive-420" => AuthoredJpeg.ProgressiveFrame(width, height, JpegSampling.YCbCr420, restartInterval: 3),
        "progressive-422" => AuthoredJpeg.ProgressiveFrame(width, height, JpegSampling.YCbCr422, restartInterval: 3),
        "rgb-422" => AdobeRgb(AuthoredJpeg.SequentialFrame(width, height, JpegSampling.YCbCr422, restartInterval: 3)),
        "rgb-420" => AdobeRgb(AuthoredJpeg.SequentialFrame(width, height, JpegSampling.YCbCr420, restartInterval: 3)),
        "quarter-411" => AuthoredJpegCompatibility.Create(width, height, [123, 97, 155], h: 4, v: 1, vary: true, nonzeroAc: true),
        "quarter-410" => AuthoredJpegCompatibility.Create(width, height, [123, 97, 155], h: 4, v: 2, vary: true, nonzeroAc: true),
        _ => throw new ArgumentOutOfRangeException(nameof(path))
    };

    private static byte[] AdobeRgb(byte[] data)
    {
        byte[] adobe = [255, 238, 0, 14, 65, 100, 111, 98, 101, 0, 100, 0, 0, 0, 0, 0];
        return data[..^2].Concat(adobe).Concat(new byte[] { 255, 217 }).ToArray();
    }

    private static byte[] Poison(int length)
    {
        _ = PixelStoragePool.Shared.Rent(length + 1);
        var data = PixelStoragePool.Shared.Rent(length);
        data.AsSpan().Fill(0xa5); PixelStoragePool.Shared.Return(data);
        return data;
    }
}
