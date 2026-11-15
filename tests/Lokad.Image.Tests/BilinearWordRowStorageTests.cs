using System.Runtime.Intrinsics.X86;
using Lokad.Image.Internal;
using Lokad.Image.Tooling;

namespace Lokad.Image.Tests;

public sealed class BilinearWordRowStorageTests
{
    [Fact]
    public void AdjacentRowsPreservePixelsSurvivorsAndTerminalAdmission()
    {
        if (!Avx512F.IsSupported || !Avx512DQ.IsSupported || !Avx2.IsSupported || !Sse41.IsSupported) return;
        using var source = OriginalPixels.Buffer(5, 3, 16);
        for (var y = 0; y < source.Height; y++)
        for (var x = 0; x < source.Width; x++) source.GetRow16(y)[x * 4 + 3] = 65535;
        var input = OriginalPixels.Bytes(source);
        const int width = 11, height = 7, outputBytes = width * height * 8;
        const long planBytes = width * 8L * 12 + width * 4L + 192;
        var cacheBytes = width * 16L * 8 + 64;
        using var direct = ImageTransforms.Resize(source, width, height, ResizeFilter.Bilinear,
            new() { MaxWorkingBytes = input.Length + outputBytes + ResizeAlpha.ScratchBytes });
        var expected = OriginalPixels.Bytes(direct);
        using var root = new TestMemoryBudget(20_000);
        var limits = new ImageLimits { MemoryBudget = root };
        using var first = ImageTransforms.Resize(source, width, height, ResizeFilter.Bilinear, limits);
        using var survivor = ImageTransforms.Resize(source, width, height, ResizeFilter.Bilinear, limits);
        Assert.Equal(expected, OriginalPixels.Bytes(first));
        Assert.Equal(expected, OriginalPixels.Bytes(survivor));
        Assert.Equal(2 * outputBytes, root.Bytes);
        Assert.Equal(2 * outputBytes + planBytes + cacheBytes, root.Peak);
        first.GetRow16(0)[0] ^= 65535;
        first.Dispose();
        Assert.Equal(outputBytes, root.Bytes);
        Assert.Equal(expected, OriginalPixels.Bytes(survivor));
        for (var phase = 1; phase <= 3; phase++)
        {
            using var refused = new TestMemoryBudget(20_000) { RefuseAtGrowth = phase };
            var error = Assert.Throws<ImageLimitException>(() => ImageTransforms.Resize(source,
                width, height, ResizeFilter.Bilinear, new() { MemoryBudget = refused }));
            Assert.Equal((ImageLimitKind.WorkingBytes, ImageLimitScope.SharedBudget), (error.Limit, error.Scope));
            Assert.Equal(phase, refused.GrowthAttempts);
            Assert.Equal(1, refused.Refusals);
            Assert.Equal(0, refused.Bytes);
            Assert.False(refused.IsDisposed);
            refused.RefuseAtGrowth = 0;
            using (var recovered = ImageTransforms.Resize(source, width, height, ResizeFilter.Bilinear,
                new() { MemoryBudget = refused })) Assert.Equal(expected, OriginalPixels.Bytes(recovered));
            Assert.Equal(0, refused.Bytes);
        }
        using var canceled = new TestMemoryBudget(20_000);
        using var cancellation = new CancellationTokenSource();
        canceled.Admitted = _ => { if (canceled.GrowthAttempts == 3) cancellation.Cancel(); };
        Assert.Throws<OperationCanceledException>(() => ImageTransforms.Resize(source, width, height,
            ResizeFilter.Bilinear, new() { MemoryBudget = canceled }, cancellation.Token));
        Assert.Equal(0, canceled.Bytes);
        Assert.False(canceled.IsDisposed);
        canceled.Admitted = null;
        using (var recovered = ImageTransforms.Resize(source, width, height, ResizeFilter.Bilinear,
            new() { MemoryBudget = canceled })) Assert.Equal(expected, OriginalPixels.Bytes(recovered));
        Assert.Equal(0, canceled.Bytes);
        Assert.Equal(input, OriginalPixels.Bytes(source));
        source.Dispose();
        Assert.Equal(expected, OriginalPixels.Bytes(survivor));
        survivor.Dispose();
        Assert.Equal(0, root.Bytes);
        Assert.False(root.IsDisposed);
    }
}
