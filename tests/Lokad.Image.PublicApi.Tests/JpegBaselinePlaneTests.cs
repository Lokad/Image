using Lokad.Image.Tooling;

namespace Lokad.Image.PublicApi.Tests;

public sealed class JpegBaselinePlaneTests
{
    private const int Width = 513, Height = 259;
    private static byte[] Packet() => AuthoredJpegCompatibility.Create(Width, Height, [20, 40, 60, 80]);

    [Fact]
    public void CallerBaselineFitsBytePlanesAndBorrowsItsInputDestinationAndRoot()
    {
        var data = Packet(); var source = data.ToArray(); var output = new byte[Width * Height * 4];
        using var root = new TestMemoryBudget(768 * 1024);
        var limits = new ImageLimits { MemoryBudget = root, MaxWorkingBytes = 1400 * 1024 };
        var info = ImageCodec.DecodeJpeg(data, output, JpegSampleFormat.Cmyk, options: new() { Limits = limits });
        Assert.Equal((Width, Height), (info.Width, info.Height));
        for (var i = 0; i < output.Length; i += 4) Assert.Equal(new byte[] { 20, 40, 60, 80 }, output.AsSpan(i, 4).ToArray());
        // Four padded byte planes fit; four full int planes alone need2196480B.
        Assert.InRange(root.Peak, 549120, 768 * 1024);
        Assert.Equal(0, root.Bytes); Assert.False(root.IsDisposed); Assert.Equal(source, data);
    }

    [Fact]
    public void BaselineRetainedResultsShareAggregateAdmissionAndReleaseWithoutGc()
    {
        var data = Packet();
        using var root = new TestMemoryBudget(1250000);
        var options = new DecodeOptions { Limits = new() { MemoryBudget = root } };
        using var first = ImageCodec.Decode(data, options);
        var retained = first.Stride * first.Height;
        Assert.Equal(retained, root.Bytes); Assert.InRange(root.Peak, 549120 + retained, 1250000);
        var error = Assert.Throws<ImageLimitException>(() => ImageCodec.Decode(data, options));
        Assert.Equal((ImageLimitKind.WorkingBytes, ImageLimitScope.SharedBudget), (error.Limit, error.Scope));
        Assert.Equal(1, root.Refusals); Assert.Equal(retained, root.Bytes);
        first.Dispose(); Assert.Equal(0, root.Bytes);
        using (var next = ImageCodec.Decode(data, options)) Assert.Equal(retained, root.Bytes);
        Assert.Equal(0, root.Bytes); Assert.False(root.IsDisposed);
    }

    [Theory]
    [InlineData(false)] [InlineData(true)]
    public void PlaneAdmissionRefusalOrCancellationCleansUpBeforeCallerPublication(bool cancel)
    {
        var data = Packet(); var source = data.ToArray();
        var output = Enumerable.Repeat((byte)165, Width * Height * 4).ToArray();
        using var root = new TestMemoryBudget(768 * 1024);
        using var token = new CancellationTokenSource(); var armed = false;
        root.Admitted = bytes =>
        {
            if (armed || bytes < 100000) return;
            armed = true;
            if (cancel) token.Cancel();
            else root.RefuseAtGrowth = root.GrowthAttempts + 1;
        };
        var options = new JpegDecodeOptions { Limits = new() { MemoryBudget = root } };
        if (cancel)
        {
            var error = Assert.Throws<OperationCanceledException>(() => ImageCodec.DecodeJpeg(data, output, JpegSampleFormat.Cmyk, options: options, cancellationToken: token.Token));
            Assert.Equal(token.Token, error.CancellationToken);
        }
        else
        {
            var error = Assert.Throws<ImageLimitException>(() => ImageCodec.DecodeJpeg(data, output, JpegSampleFormat.Cmyk, options: options));
            Assert.Equal((ImageLimitKind.WorkingBytes, ImageLimitScope.SharedBudget), (error.Limit, error.Scope));
            Assert.Equal(1, root.Refusals);
        }
        Assert.True(armed); Assert.Equal(0, root.Bytes); Assert.False(root.IsDisposed);
        Assert.All(output, value => Assert.Equal(165, value)); Assert.Equal(source, data);
        root.Admitted = null; root.RefuseAtGrowth = 0;
        ImageCodec.DecodeJpeg(data, output, JpegSampleFormat.Cmyk, options: options);
        Assert.Equal(0, root.Bytes);
    }

    [Fact]
    public void LateDuplicateScanDoesNotPublishCallerPixels()
    {
        var original = Packet();
        // A second baseline scan repeats component1. Container structure stays
        // readable; coefficient progression is rejected after the first scan.
        byte[] repeat = [255, 218, 0, 8, 1, 1, 0, 0, 63, 0];
        var data = original[..^2].Concat(repeat).Concat(new byte[] { 255, 217 }).ToArray();
        Assert.Equal(Width, ImageCodec.Identify(data).Width);
        using var root = new TestMemoryBudget(768 * 1024);
        var output = Enumerable.Repeat((byte)165, Width * Height * 4).ToArray();
        Assert.Throws<ImageDataException>(() => ImageCodec.DecodeJpeg(data, output, JpegSampleFormat.Cmyk,
            options: new() { Limits = new() { MemoryBudget = root } }));
        Assert.InRange(root.Peak, 549120, 768 * 1024); Assert.Equal(0, root.Bytes);
        Assert.All(output, value => Assert.Equal(165, value)); Assert.False(root.IsDisposed);
    }

    [Theory]
    [InlineData(false)] [InlineData(true)]
    public void SeparateBaselineScansPreservePaddingAndFirstScanQuantization(bool redefineQuantization)
    {
        const int width = 17, height = 17;
        var template = AuthoredJpegCompatibility.Create(width, height, [128, 128, 128], 4, 2);
        var scan = 2;
        while (template[scan + 1] != 218) scan += 2 + (template[scan + 2] << 8 | template[scan + 3]);
        using var encoded = new MemoryStream(); encoded.Write(template.AsSpan(0, scan));
        // Category9 has the authored four-bit code1001. Y's first DC delta256
        // uses nine amplitude bits; following deltas and chroma DC are zero.
        foreach (var (component, blocks) in new[] { (1, 9), (2, 2), (3, 2) })
        {
            if (component == 2 && redefineQuantization)
            {
                encoded.Write(new byte[] { 255, 219, 0, 67, 0 });
                for (var i = 0; i < 64; i++) encoded.WriteByte(2);
            }
            encoded.Write(new byte[] { 255, 218, 0, 8, 1, (byte)component, 0, 0, 63, 0 });
            uint pending = 0; var available = 0;
            if (component == 1) WriteBits(9728, 14); // DC256 then EOB.
            else WriteBits(0, 5);
            for (var i = 1; i < blocks; i++) WriteBits(0, 5);
            if (available != 0) WriteBits((1 << (8 - available)) - 1, 8 - available);
            void WriteBits(int value, int count)
            {
                pending = pending << count | (uint)value; available += count;
                while (available >= 8)
                {
                    available -= 8; var next = (byte)(pending >> available);
                    encoded.WriteByte(next); if (next == 255) encoded.WriteByte(0);
                }
            }
        }
        encoded.Write(new byte[] { 255, 217 }); var data = encoded.ToArray();
        var rgb = new byte[width * height * 3]; ImageCodec.DecodeJpeg(data, rgb, JpegSampleFormat.Rgb);
        Assert.All(rgb, value => Assert.Equal(160, value));
        using var result = ImageCodec.Decode(data);
        for (var y = 0; y < height; y++)
        for (var x = 0; x < width; x++) Assert.Equal(new byte[] { 160, 160, 160, 255 }, result.GetRow8(y).Slice(x * 4, 4).ToArray());
        Assert.Equal((width, height), (ImageCodec.ValidatePixels(data).Width, ImageCodec.ValidatePixels(data).Height));
    }
}
