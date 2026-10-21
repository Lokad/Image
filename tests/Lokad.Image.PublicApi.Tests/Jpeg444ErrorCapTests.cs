using System.Security.Cryptography;
using Lokad.Image.Tooling;

namespace Lokad.Image.PublicApi.Tests;

public sealed class Jpeg444ErrorCapTests
{
    [Theory]
    [InlineData(8, 141528, "dbdac328278702f19b9eead50120ef391cb1693d722196ea5b615240bef6d099")]
    [InlineData(16, 141529, "eccb78e3c51d2eb2af138c23cc2b12bd169c04c7431a95cbf4834e352ce24e66")]
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
        using var root = new TestMemoryBudget(8192);
        ImageCodec.EncodeJpeg(source, fixedOutput, options with
        { Limits = new() { MaxWorkingBytes = original.Length + 8192L, MemoryBudget = root } });
        Assert.Equal(0, root.Bytes); Assert.Equal(8192, root.Peak); Assert.False(root.IsDisposed);
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

    [Fact]
    public void WorkspaceRefusalIsTerminalAndReleasedCapacityAllowsTheNextOperation()
    {
        using var source = OriginalPixels.Buffer(17, 11, 8);
        var original = OriginalPixels.Bytes(source);
        using var root = new TestMemoryBudget(8192); using var held = root.CreateScope();
        Assert.True(held.TryReserve(1, out var reservation));
        var options = new JpegEncodeOptions { Quality = 69, Sampling = JpegSampling.YCbCr444, BackgroundRgb = 0x204060,
            Limits = new() { MemoryBudget = root, MaxWorkingBytes = original.Length + 8192L } };
        using var output = new MemoryStream(); var attempts = root.Attempts;
        var error = Assert.Throws<ImageLimitException>(() => ImageCodec.EncodeJpeg(source, output, options));
        Assert.Equal((ImageLimitKind.WorkingBytes, ImageLimitScope.SharedBudget), (error.Limit, error.Scope));
        Assert.Equal(attempts + 1, root.Attempts); Assert.Equal(1, root.Bytes); Assert.Equal(0, output.Length);
        reservation!.Dispose(); Assert.Equal(0, root.Bytes);
        ImageCodec.EncodeJpeg(source, output, options);
        Assert.True(output.Length > 0); Assert.True(output.CanWrite); Assert.Equal(0, root.Bytes);
        Assert.Equal(8192, root.Peak); Assert.False(root.IsDisposed); Assert.Equal(original, OriginalPixels.Bytes(source));
    }

    private static string Hash(byte[] bytes) => Convert.ToHexStringLower(SHA256.HashData(bytes));
}
