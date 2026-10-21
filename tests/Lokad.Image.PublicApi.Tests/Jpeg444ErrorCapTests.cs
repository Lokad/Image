using System.Security.Cryptography;
using Lokad.Image.Tooling;

namespace Lokad.Image.PublicApi.Tests;

public sealed class Jpeg444ErrorCapTests
{
    [Theory]
    [InlineData(8, 155732, "91bd50003026273a911bf3fa0e60c233994cfe6432e0e3a4e5f6f86b702d57c0")]
    [InlineData(16, 155795, "14f53cb34b5f14ac5dee875dbafd8d66f8cd3ac7847b7dabc055cc96ad3a3847")]
    public async Task IndependentSourceDctPacketsAgreeAcrossInputAndOutputBoundaries(int depth, int bytes, string hash)
    {
        using var source = OriginalPixels.Buffer(513, 259, depth);
        if (depth == 8) source.GetRow8(0)[3] = source.GetRow8(0)[513 * 4 - 1] = 0;
        else source.GetRow16(0)[3] = source.GetRow16(0)[513 * 4 - 1] = 0;
        var original = OriginalPixels.Bytes(source);
        var options = new JpegEncodeOptions { Quality = 69, Sampling = JpegSampling.YCbCr444, BackgroundRgb = 0x204060 };
        using var adaptive = new MemoryStream(); ImageCodec.EncodeJpeg(source, adaptive, options);
        var packet = adaptive.ToArray(); Assert.Equal(bytes, packet.Length); Assert.Equal(hash, Hash(packet));
        using var token = new CancellationTokenSource(); using var cancelable = new MemoryStream();
        ImageCodec.EncodeJpeg(source, cancelable, options, token.Token); Assert.Equal(packet, cancelable.ToArray());
        using var fixedOutput = new MemoryStream();
        ImageCodec.EncodeJpeg(source, fixedOutput, options with { Limits = new() { MaxWorkingBytes = original.Length + 8192L } });
        using var decoded = ImageCodec.Decode(packet); using var fixedPixels = ImageCodec.Decode(fixedOutput.ToArray());
        Assert.Equal(OriginalPixels.Bytes(decoded), OriginalPixels.Bytes(fixedPixels));

        var normalized = JpegQualityCorpus.Reference(original, depth, options.BackgroundRgb);
        var samples = new byte[513 * 259 * 3];
        for (var pixel = 0; pixel < 513 * 259; pixel++) normalized.AsSpan(pixel * 4, 3).CopyTo(samples.AsSpan(pixel * 3, 3));
        var unchanged = samples.ToArray(); using var caller = new MemoryStream();
        ImageCodec.EncodeJpeg(samples, 513, 259, JpegSampleFormat.Rgb, caller, options: options);
        Assert.Equal(packet, caller.ToArray());
        using var asyncImage = new MemoryStream(); await ImageCodec.EncodeJpegAsync(source, asyncImage, options);
        using var asyncSamples = new MemoryStream();
        await ImageCodec.EncodeJpegAsync(samples.AsMemory(), 513, 259, JpegSampleFormat.Rgb, asyncSamples, options: options);
        Assert.Equal(packet, asyncImage.ToArray()); Assert.Equal(packet, asyncSamples.ToArray());
        Assert.Equal(original, OriginalPixels.Bytes(source)); Assert.Equal(unchanged, samples);
        source.Dispose(); adaptive.Dispose(); caller.Dispose(); samples.AsSpan().Clear();
        Assert.Equal(packet, asyncSamples.ToArray()); Assert.True(asyncSamples.CanWrite);
    }

    private static string Hash(byte[] bytes) => Convert.ToHexStringLower(SHA256.HashData(bytes));
}
