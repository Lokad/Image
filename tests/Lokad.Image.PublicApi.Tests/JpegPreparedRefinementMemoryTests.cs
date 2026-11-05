using System.Runtime.Intrinsics.X86;
using Lokad.Image.Tooling;

namespace Lokad.Image.PublicApi.Tests;

public sealed class JpegPreparedRefinementMemoryTests
{
    [Theory]
    [InlineData(false)] [InlineData(true)]
    public void ScanPreparationRefusalAndCancellationReleaseChargesWithoutFallback(bool cancelable)
    {
        if (!Avx2.IsSupported || !Bmi2.X64.IsSupported) return;
        var input = AuthoredJpeg.ProgressiveFrame(64, 32, JpegSampling.Grayscale, restartInterval: 1); var unchanged = input.ToArray();
        using var live = new CancellationTokenSource();
        long planGrowth = 0, previousBytes = 0;
        byte[] expected;
        using (var root = new TestMemoryBudget(1024 * 1024))
        {
            root.Admitted = used =>
            {
                if (planGrowth == 0 && used - previousBytes == 2048) planGrowth = root.GrowthAttempts;
                previousBytes = used;
            };
            using (var result = ImageCodec.Decode(input, new() { Limits = new() { MemoryBudget = root } }, cancelable ? live.Token : default))
            {
                expected = result.GetRow8(0).ToArray();
                Assert.Equal(64 * 32 * 4, root.Bytes);
            }
            Assert.True(planGrowth > 0); Assert.Equal(0, root.Bytes); Assert.False(root.IsDisposed);
        }
        foreach (var cancel in new[] { false, true })
        {
            using var root = new TestMemoryBudget(1024 * 1024);
            using var token = new CancellationTokenSource();
            var options = new DecodeOptions { Limits = new() { MemoryBudget = root } };
            if (cancel) root.Admitted = _ => { if (root.GrowthAttempts == planGrowth) token.Cancel(); };
            else root.RefuseAtGrowth = planGrowth;
            var error = Record.Exception(() => ImageCodec.Decode(input, options, token.Token));
            if (cancel) Assert.Equal(token.Token, Assert.IsType<OperationCanceledException>(error).CancellationToken);
            else
            {
                var refusal = Assert.IsType<ImageLimitException>(error);
                Assert.Equal(ImageLimitKind.WorkingBytes, refusal.Limit);
                Assert.Equal(ImageLimitScope.SharedBudget, refusal.Scope);
                Assert.Equal(1, root.Refusals); Assert.Equal(planGrowth, root.GrowthAttempts);
            }
            Assert.Equal(0, root.Bytes); Assert.False(root.IsDisposed); Assert.Equal(unchanged, input);
            root.Admitted = null; root.RefuseAtGrowth = 0;
            using (var result = ImageCodec.Decode(input, options, cancelable ? live.Token : default))
            {
                Assert.Equal(expected, result.GetRow8(0).ToArray());
                Assert.Equal(64 * 32 * 4, root.Bytes);
            }
            Assert.Equal(0, root.Bytes); Assert.False(root.IsDisposed);
        }
    }
}
