using Lokad.Image.Tooling;

namespace Lokad.Image.PublicApi.Tests;

public sealed class JpegProgressiveEncodeTests
{
    [Theory]
    [InlineData(1)] [InlineData(69)] [InlineData(100)]
    public void BaselineOnlyChangesSyntaxWhileRetainingQuantizersAndDecodedPixels(int quality)
    {
        using var image = OriginalPixels.Buffer(129, 129, 8);
        var options = new JpegEncodeOptions { QuantizationPolicy = JpegQuantizationPolicy.Original, Quality = quality, Sampling = JpegSampling.YCbCr444, BackgroundRgb = 0x204060 };
        using var automatic = new MemoryStream(); ImageCodec.EncodeJpeg(image, automatic, options);
        using var baseline = new MemoryStream(); ImageCodec.EncodeJpeg(image, baseline, options with { BaselineOnly = true });
        var a = automatic.ToArray(); var b = baseline.ToArray();
        Assert.Equal(quality == 100 ? 0xC0 : 0xC2, JpegQualitySyntax.Read(a).FrameMarker);
        Assert.Equal(0xC0, JpegQualitySyntax.Read(b).FrameMarker);
        Assert.Equal(JpegQualitySyntax.Read(a).QuantizationHash, JpegQualitySyntax.Read(b).QuantizationHash);
        using var decoded = ImageCodec.Decode(a); using var expected = ImageCodec.Decode(b);
        Assert.Equal(OriginalPixels.Bytes(expected), OriginalPixels.Bytes(decoded));
        using var exact = new MemoryStream(); ImageCodec.EncodeJpeg(image, exact, options with { Limits = new() { MaxOutputBytes = a.Length } });
        Assert.Equal(a, exact.ToArray());
        using var shortOutput = new MemoryStream();
        Assert.Equal(ImageLimitKind.OutputBytes, Assert.Throws<ImageLimitException>(() => ImageCodec.EncodeJpeg(image, shortOutput, options with { Limits = new() { MaxOutputBytes = a.Length - 1 } })).Limit);
    }

    [Fact]
    public void WholePlanAdmissionRefusalIsTerminalAndDoesNotWriteABaselineFallback()
    {
        using var image = OriginalPixels.Buffer(129, 129, 8); using var root = new TestMemoryBudget(8192);
        var options = new JpegEncodeOptions { QuantizationPolicy = JpegQuantizationPolicy.Original, Quality = 69, Sampling = JpegSampling.YCbCr444, BackgroundRgb = 0x204060, Limits = new() { MemoryBudget = root } };
        using var output = new MemoryStream();
        var error = Assert.Throws<ImageLimitException>(() => ImageCodec.EncodeJpeg(image, output, options));
        Assert.Equal((ImageLimitKind.WorkingBytes, ImageLimitScope.SharedBudget), (error.Limit, error.Scope));
        Assert.Equal(0, output.Length); Assert.Equal(0, root.Bytes); Assert.Equal(1, root.Refusals); Assert.False(root.IsDisposed);
        // A deliberate caller-selected local allowance uses the fixed encoder;
        // this is a separate operation after the refused plan has cleaned up.
        ImageCodec.EncodeJpeg(image, output, options with { Limits = options.Limits with { MaxWorkingBytes = (long)image.Width * image.Height * 4 + 8192 } });
        Assert.Equal(0xC0, JpegQualitySyntax.Read(output.ToArray()).FrameMarker); Assert.Equal(0, root.Bytes);
    }

    [Fact]
    public void CancellationKeepsThePlanChargedUntilTheWriterStopsAndAllowsSubsequentSuccess()
    {
        using var image = OriginalPixels.Buffer(129, 129, 8); using var root = new TestMemoryBudget(1_000_000);
        var options = new JpegEncodeOptions { QuantizationPolicy = JpegQuantizationPolicy.Original, Quality = 69, Sampling = JpegSampling.YCbCr444, BackgroundRgb = 0x204060, Limits = new() { MemoryBudget = root } };
        using var token = new CancellationTokenSource(); using var output = new CancelingOutput(token, root);
        Assert.ThrowsAny<OperationCanceledException>(() => ImageCodec.EncodeJpeg(image, output, options, token.Token));
        Assert.True(output.SawChargedPlan); Assert.Equal(0, root.Bytes); Assert.False(root.IsDisposed); Assert.True(output.CanWrite);
        using var next = new MemoryStream(); ImageCodec.EncodeJpeg(image, next, options);
        Assert.Equal(0xC2, JpegQualitySyntax.Read(next.ToArray()).FrameMarker); Assert.Equal(0, root.Bytes);
    }

    private sealed class CancelingOutput(CancellationTokenSource token, TestMemoryBudget root) : MemoryStream
    {
        public bool SawChargedPlan { get; private set; }
        public override void Write(ReadOnlySpan<byte> bytes)
        {
            base.Write(bytes); SawChargedPlan = root.Bytes > 8192; token.Cancel();
        }
    }
}
