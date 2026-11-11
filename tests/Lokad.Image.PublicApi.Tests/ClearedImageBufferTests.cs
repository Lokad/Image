using System.Runtime.InteropServices;
using Lokad.Image.Tooling;

namespace Lokad.Image.PublicApi.Tests;

public sealed class ClearedImageBufferTests
{
    [Theory]
    [InlineData(19, 13, 8)] [InlineData(23, 11, 16)]
    [InlineData(7, 5, 8)] [InlineData(7, 5, 16)]
    public void NewCanvasesAreZeroedAfterDirtyDisposalAndSurvivingOwnersStayIndependent(int width, int height, int depth)
    {
        using var first = ImageBuffer.Create(width, height, depth);
        using var survivor = ImageBuffer.Create(width, height, depth);
        Fill(first, 0xa5); Fill(survivor, 0x36); first.Dispose();
        using var next = ImageBuffer.Create(width, height, depth);
        Assert.Equal((width, height, depth), (next.Width, next.Height, next.BitDepth));
        Assert.Equal(width * 4 * (depth / 8), next.Stride); Assert.Equal(ImageOrientation.Normal, next.Orientation);
        Check(next, 0); Check(survivor, 0x36); Fill(next, 0x5a); next.Dispose();
        using var recovered = ImageBuffer.Create(width, height, depth);
        Check(recovered, 0); Check(survivor, 0x36);
        Assert.Throws<ObjectDisposedException>(() => { if (depth == 8) first.GetRow8(0); else first.GetRow16(0); });
    }

    [Fact]
    public void GovernedOwnersKeepAggregateAdmissionAndRecoverAfterRefusalAndCancellation()
    {
        const int bytes = 23 * 11 * 8;
        using var root = new TestMemoryBudget(2L * bytes); var limits = new ImageLimits { MemoryBudget = root };
        using var first = ImageBuffer.Create(23, 11, 16, limits);
        using var survivor = ImageBuffer.Create(23, 11, 16, limits); Fill(first, 0xa5); Fill(survivor, 0x36);
        Assert.Equal(2L * bytes, root.Bytes);
        var error = Assert.Throws<ImageLimitException>(() => ImageBuffer.Create(23, 11, 16, limits));
        Assert.Equal(ImageLimitScope.SharedBudget, error.Scope); Assert.Equal(1, root.Refusals);
        Assert.Equal(2L * bytes, root.Bytes); first.Dispose(); Assert.Equal(bytes, root.Bytes);
        using (var canceled = new CancellationTokenSource())
        {
            canceled.Cancel(); Assert.ThrowsAny<OperationCanceledException>(() => ImageBuffer.Create(23, 11, 16, limits, canceled.Token));
            Assert.Equal(bytes, root.Bytes);
        }
        using (var next = ImageBuffer.Create(23, 11, 16, limits)) { Check(next, 0); Check(survivor, 0x36); Assert.Equal(2L * bytes, root.Bytes); }
        survivor.Dispose(); Assert.Equal(0, root.Bytes); Assert.False(root.IsDisposed);
        using (var recovered = ImageBuffer.Create(23, 11, 16, limits)) { Check(recovered, 0); Assert.Equal(bytes, root.Bytes); }
        Assert.Equal(0, root.Bytes);
    }

    private static void Fill(ImageBuffer image, byte value)
    {
        for (var y = 0; y < image.Height; y++)
            (image.BitDepth == 8 ? image.GetRow8(y) : MemoryMarshal.AsBytes(image.GetRow16(y))).Fill(value);
    }
    private static void Check(ImageBuffer image, byte value)
    {
        for (var y = 0; y < image.Height; y++)
            Assert.All((image.BitDepth == 8 ? image.GetRow8(y) : MemoryMarshal.AsBytes(image.GetRow16(y))).ToArray(), sample => Assert.Equal(value, sample));
    }
}
