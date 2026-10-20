using Lokad.Image.Tooling;

namespace Lokad.Image.PublicApi.Tests;

public sealed class JpegCmykOutputMemoryTests
{
    [Theory]
    [InlineData(false)] [InlineData(true)]
    public void OwnedCmykFitsTheUnchangedCallerPlaneAdmissionPeak(bool progressive)
    {
        const int width = 65, height = 31;
        var input = AuthoredJpegCompatibility.Create(width, height, [17, 191, 250, 63], adobe: 0,
            progressive: progressive, restart: 1, vary: true);
        using var reference = new TestMemoryBudget(1024 * 1024);
        var cmyk = new byte[width * height * 4];
        ImageCodec.DecodeJpeg(input, cmyk, JpegSampleFormat.Cmyk, options: new() { Limits = new() { MemoryBudget = reference } });
        Assert.Equal(0, reference.Bytes);
        var expected = new byte[width * height * 3]; ImageCodec.DecodeJpeg(input, expected, JpegSampleFormat.Rgb);
        using var root = new TestMemoryBudget(reference.Peak);
        var limits = new ImageLimits { MemoryBudget = root, MaxWorkingBytes = reference.Peak + input.Length + cmyk.Length };
        using (var result = ImageCodec.Decode(input, new() { Limits = limits }))
        {
            Assert.Equal(cmyk.Length, root.Bytes); Assert.True(root.Peak <= reference.Peak);
            for (var y = 0; y < height; y++)
            for (var x = 0; x < width; x++)
            {
                for (var c = 0; c < 3; c++) Assert.Equal(expected[(y * width + x) * 3 + c], result.GetRow8(y)[x * 4 + c]);
                Assert.Equal(255, result.GetRow8(y)[x * 4 + 3]);
            }
        }
        Assert.Equal(0, root.Bytes); Assert.False(root.IsDisposed);
    }

    [Theory]
    [InlineData(false, 1)] [InlineData(false, 2)] [InlineData(true, 0)]
    public void RetainedOwnersAndEveryRefusalOrCanceledOutputAdmissionReleaseCapacity(bool ycck, int acTerms)
    {
        var input = AuthoredJpegCompatibility.Create(17, 9, [80, 170, 90, 200],
            ycck ? 2 : 1, ycck ? 2 : 1, true, ycck ? 2 : 0, vary: true, nonzeroAc: acTerms != 0, acTerms: Math.Max(1, acTerms));
        var original = input.ToArray(); const int outputBytes = 17 * 9 * 4;
        using var baseline = new TestMemoryBudget(1024 * 1024);
        var limits = new ImageLimits { MemoryBudget = baseline };
        using (var first = ImageCodec.Decode(input, new() { Limits = limits }))
        {
            Assert.Equal(outputBytes, baseline.Bytes);
            using var survivor = ImageCodec.Decode(input, new() { Limits = limits });
            var expected = OriginalPixels.Bytes(survivor);
            Assert.Equal(outputBytes * 2, baseline.Bytes);
            first.GetRow8(0).Fill(173); first.Dispose(); Assert.Equal(outputBytes, baseline.Bytes);
            Assert.Equal(expected, OriginalPixels.Bytes(survivor));
        }
        Assert.Equal(0, baseline.Bytes); Assert.False(baseline.IsDisposed);
        using var single = new TestMemoryBudget(1024 * 1024);
        using (ImageCodec.Decode(input, new() { Limits = new() { MemoryBudget = single } })) { }
        var growths = single.GrowthAttempts; Assert.InRange(growths, 1, 100);
        for (var failure = 1; failure <= growths; failure++)
        {
            using var root = new TestMemoryBudget(1024 * 1024) { RefuseAtGrowth = failure };
            var options = new DecodeOptions { Limits = new() { MemoryBudget = root } };
            var error = Assert.Throws<ImageLimitException>(() => ImageCodec.Decode(input, options));
            Assert.Equal(ImageLimitKind.WorkingBytes, error.Limit); Assert.Equal(ImageLimitScope.SharedBudget, error.Scope);
            Assert.Equal(1, root.Refusals); Assert.Equal(failure, root.GrowthAttempts); Assert.Equal(0, root.Bytes);
            root.RefuseAtGrowth = 0;
            using (ImageCodec.Decode(input, options)) { }
            Assert.Equal(0, root.Bytes); Assert.False(root.IsDisposed);
        }
        using var canceled = new TestMemoryBudget(1024 * 1024); using var token = new CancellationTokenSource();
        canceled.Admitted = _ => { if (canceled.GrowthAttempts == growths) token.Cancel(); };
        var cancelOptions = new DecodeOptions { Limits = new() { MemoryBudget = canceled } };
        var cancellation = Assert.Throws<OperationCanceledException>(() => ImageCodec.Decode(input, cancelOptions, token.Token));
        Assert.Equal(token.Token, cancellation.CancellationToken); Assert.Equal(0, canceled.Bytes); Assert.False(canceled.IsDisposed);
        canceled.Admitted = null;
        using (ImageCodec.Decode(input, cancelOptions)) { }
        Assert.Equal(0, canceled.Bytes); Assert.Equal(original, input);
    }
}
