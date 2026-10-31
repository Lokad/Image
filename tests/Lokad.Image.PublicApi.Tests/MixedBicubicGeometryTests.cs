using Lokad.Image.Tooling;

namespace Lokad.Image.PublicApi.Tests;

public sealed class MixedBicubicGeometryTests
{
    [Theory]
    [InlineData(8)] [InlineData(16)]
    public void MixedPrefixAndOverwrittenTailKeepPixelsAndIndependentOwners(int depth)
    {
        using var source = OriginalPixels.Buffer(35, 131, depth);
        var input = OriginalPixels.Bytes(source);
        const int width = 71, height = 65;
        var bytes = input.Length + (long)width * height * 4 * (depth / 8);
        // The lower local bound selects the original fallback; the extra byte
        // admits the mixed kernel. Compare the complete frame, including its tail.
        using var expected = ImageTransforms.Resize(source, width, height, ResizeFilter.Bicubic,
            new ImageLimits { MaxWorkingBytes = bytes + 4095 });
        var limits = new ImageLimits { MaxWorkingBytes = bytes + 4096 };
        using var first = ImageTransforms.Resize(source, width, height, ResizeFilter.Bicubic, limits);
        using var second = ImageTransforms.Resize(source, width, height, ResizeFilter.Bicubic, limits);
        Assert.Equal(OriginalPixels.Bytes(expected), OriginalPixels.Bytes(first));
        Assert.Equal(OriginalPixels.Bytes(first), OriginalPixels.Bytes(second));
        if (depth == 8) first.GetRow8(0)[0] ^= 255;
        else first.GetRow16(0)[0] ^= 65535;
        first.Dispose();
        Assert.Equal(OriginalPixels.Bytes(expected), OriginalPixels.Bytes(second));
        Assert.Equal(input, OriginalPixels.Bytes(source));
    }

    [Theory]
    [InlineData(8)] [InlineData(16)]
    public void MixedStorageSharesAdmissionAndCleansRefusalAndCancellation(int depth)
    {
        using var source = OriginalPixels.Buffer(35, 131, depth);
        var input = OriginalPixels.Bytes(source);
        const int width = 71, height = 65;
        var outputBytes = (long)width * height * 4 * (depth / 8);
        using var reference = ImageTransforms.Resize(source, width, height, ResizeFilter.Bicubic,
            new ImageLimits { MaxWorkingBytes = input.Length + outputBytes + 4095 });
        using var root = new TestMemoryBudget(outputBytes + 4096);
        var limits = new ImageLimits { MaxWorkingBytes = input.Length + outputBytes + 4096, MemoryBudget = root };
        var retained = ImageTransforms.Resize(source, width, height, ResizeFilter.Bicubic, limits);
        Assert.Equal(outputBytes, root.Bytes); Assert.InRange(root.Peak, outputBytes, outputBytes + 4096);
        Assert.Equal(OriginalPixels.Bytes(reference), OriginalPixels.Bytes(retained));
        var refused = Assert.Throws<ImageLimitException>(() => ImageTransforms.Resize(source, width, height, ResizeFilter.Bicubic, limits));
        Assert.Equal(ImageLimitScope.SharedBudget, refused.Scope); Assert.Equal(outputBytes, root.Bytes);
        retained.Dispose(); Assert.Equal(0, root.Bytes); Assert.False(root.IsDisposed);
        using var token = new CancellationTokenSource();
        root.Admitted = _ => { if (root.Bytes >= outputBytes) token.Cancel(); };
        var canceled = Assert.Throws<OperationCanceledException>(() => ImageTransforms.Resize(source, width, height,
            ResizeFilter.Bicubic, limits, token.Token));
        Assert.Equal(token.Token, canceled.CancellationToken); Assert.Equal(0, root.Bytes);
        root.Admitted = null;
        using (var next = ImageTransforms.Resize(source, width, height, ResizeFilter.Bicubic, limits))
            Assert.Equal(OriginalPixels.Bytes(reference), OriginalPixels.Bytes(next));
        Assert.Equal(0, root.Bytes); Assert.False(root.IsDisposed); Assert.Equal(input, OriginalPixels.Bytes(source));
    }
}
