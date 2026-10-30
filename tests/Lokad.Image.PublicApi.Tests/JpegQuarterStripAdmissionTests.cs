using Lokad.Image.Tooling;

namespace Lokad.Image.PublicApi.Tests;

public sealed class JpegQuarterStripAdmissionTests
{
    [Fact]
    public void OwnedStripsMatchRealCallerPixelsAtOddEdgesAcrossBaselineAndProgressive()
    {
        foreach (var (width, height) in new[] { (1, 1), (67, 19), (513, 259) })
        foreach (var progressive in new[] { false, true })
        {
            var data = AuthoredJpegCompatibility.Create(width, height, [60, 105, 145], 4, 2, progressive: progressive, restart: 1, vary: true);
            var original = data.ToArray(); var rgb = new byte[width * height * 3];
            ImageCodec.DecodeJpeg(data, rgb, JpegSampleFormat.Rgb);
            using var root = new TestMemoryBudget(4 * 1024 * 1024);
            using (var owned = ImageCodec.Decode(data, new DecodeOptions { Limits = new() { MemoryBudget = root } }))
            {
                var actual = new byte[rgb.Length];
                for (var y = 0; y < height; y++)
                for (var x = 0; x < width; x++)
                {
                    owned.GetRow8(y).Slice(x * 4, 3).CopyTo(actual.AsSpan((y * width + x) * 3, 3));
                    Assert.Equal(255, owned.GetRow8(y)[x * 4 + 3]);
                }
                Assert.Equal(rgb, actual); Assert.Equal(width * height * 4, root.Bytes);
                if (width == 67) JpegQuarterExactTests.AssertNative(OriginalPixels.Bytes(owned), 2);
                var retained = OriginalPixels.Bytes(owned);
                Assert.Equal(original, data); Array.Clear(data); Assert.Equal(retained, OriginalPixels.Bytes(owned));
            }
            Assert.Equal(0, root.Bytes); Assert.False(root.IsDisposed);
        }
    }

    [Theory] [InlineData(false)] [InlineData(true)]
    public void StripAdmissionRefusalOrCancellationCleansOwnedResultAndAllowsRecovery(bool cancel)
    {
        var data = JpegQuarterExactTests.Input(2); var original = data.ToArray();
        long finalGrowth;
        using (var reference = new TestMemoryBudget(4 * 1024 * 1024))
        {
            var admissions = new List<long>(); reference.Admitted = admissions.Add;
            using (var image = ImageCodec.Decode(data, new DecodeOptions { Limits = new() { MemoryBudget = reference } }))
            {
                finalGrowth = reference.GrowthAttempts; Assert.Equal(67 * 19 * 4, reference.Bytes);
                Assert.Equal(1792, admissions[^1] - admissions[^2]);
            }
            Assert.Equal(0, reference.Bytes);
        }
        using var root = new TestMemoryBudget(4 * 1024 * 1024); using var token = new CancellationTokenSource();
        var options = new DecodeOptions { Limits = new() { MemoryBudget = root } }; var reached = false;
        if (cancel) root.Admitted = _ => { if (root.GrowthAttempts == finalGrowth) { reached = true; token.Cancel(); } };
        else root.RefuseAtGrowth = finalGrowth;
        if (cancel)
        {
            var error = Assert.Throws<OperationCanceledException>(() => ImageCodec.Decode(data, options, token.Token));
            Assert.Equal(token.Token, error.CancellationToken); Assert.True(reached);
        }
        else
        {
            var error = Assert.Throws<ImageLimitException>(() => ImageCodec.Decode(data, options));
            Assert.Equal((ImageLimitKind.WorkingBytes, ImageLimitScope.SharedBudget), (error.Limit, error.Scope));
            Assert.Equal(1, root.Refusals); Assert.Equal(finalGrowth, root.GrowthAttempts);
        }
        Assert.Equal(0, root.Bytes); Assert.False(root.IsDisposed); Assert.Equal(original, data);
        root.Admitted = null; root.RefuseAtGrowth = 0;
        using (var next = ImageCodec.Decode(data, options)) JpegQuarterExactTests.AssertNative(OriginalPixels.Bytes(next), 2);
        Assert.Equal(0, root.Bytes); Assert.False(root.IsDisposed);
    }
}
