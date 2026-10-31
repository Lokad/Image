using System.Security.Cryptography;
using Lokad.Image.Tooling;

namespace Lokad.Image.PublicApi.Tests;

public sealed class PackedColumnStorageTests
{
    [Theory]
    [InlineData(32)] [InlineData(33)]
    public void CompleteFramesKeepTheEvenAndOddRectangleBoundary(int width)
    {
        const int height = 23;
        using var source = OriginalPixels.Buffer(width * 8 + 1, height * 8 + 7, 8);
        var input = OriginalPixels.Bytes(source);
        using var expected = ImageTransforms.Resize(source, width, height, ResizeFilter.Bicubic);
        var limits = new ImageLimits { MaxWorkingBytes = input.Length + (long)width * height * 4 + 4096 };
        using var first = ImageTransforms.Resize(source, width, height, ResizeFilter.Bicubic, limits);
        using var second = ImageTransforms.Resize(source, width, height, ResizeFilter.Bicubic, limits);
        Assert.Equal(OriginalPixels.Bytes(expected), OriginalPixels.Bytes(first));
        first.GetRow8(0)[0] ^= 255; first.Dispose();
        Assert.Equal(OriginalPixels.Bytes(expected), OriginalPixels.Bytes(second));
        Assert.Equal(input, OriginalPixels.Bytes(source));
    }

    [Fact]
    public void PackedResultsKeepBorrowedInputAggregateAdmissionAndSurvivorLifetimes()
    {
        using var source = OriginalPixels.Buffer(513, 259, 8);
        source.GetRow8(0)[3] = source.GetRow8(0)[513 * 4 - 1] = 0;
        var input = OriginalPixels.Bytes(source);
        const int outputBytes = 64 * 32 * 4;
        using var root = new TestMemoryBudget(2 * outputBytes + 4096);
        var limits = new ImageLimits { MemoryBudget = root, MaxWorkingBytes = input.Length + outputBytes + 4096L };
        var first = ImageTransforms.Resize(source, 64, 32, ResizeFilter.Bicubic, limits);
        var survivor = ImageTransforms.Resize(source, 64, 32, ResizeFilter.Bicubic, limits);
        var expected = OriginalPixels.Bytes(survivor);
        Assert.Equal("e2340f662ce94418bed4e7be5c65ac4c73fb7a705b047af06784e42b37c267c6",
            Convert.ToHexString(SHA256.HashData(expected)).ToLowerInvariant());
        Assert.Equal(expected, OriginalPixels.Bytes(first));
        Assert.Equal(2 * outputBytes, root.Bytes);
        Assert.InRange(root.Peak, 2 * outputBytes, 2 * outputBytes + 4096);
        var refused = Assert.Throws<ImageLimitException>(() => ImageTransforms.Resize(source, 64, 32, ResizeFilter.Bicubic, limits));
        Assert.Equal(ImageLimitScope.SharedBudget, refused.Scope); Assert.Equal(2 * outputBytes, root.Bytes);
        first.Dispose();
        using (var recovered = ImageTransforms.Resize(source, 64, 32, ResizeFilter.Bicubic, limits))
            Assert.Equal(expected, OriginalPixels.Bytes(recovered));
        Assert.Equal(outputBytes, root.Bytes);
        Assert.Equal(expected, OriginalPixels.Bytes(survivor));
        survivor.Dispose(); Assert.Equal(0, root.Bytes);
        using var token = new CancellationTokenSource();
        root.Admitted = bytes => { if (bytes >= outputBytes) token.Cancel(); };
        var canceled = Assert.Throws<OperationCanceledException>(() => ImageTransforms.Resize(source, 64, 32,
            ResizeFilter.Bicubic, limits, token.Token));
        Assert.Equal(token.Token, canceled.CancellationToken); Assert.Equal(0, root.Bytes);
        root.Admitted = null;
        using (var recovered = ImageTransforms.Resize(source, 64, 32, ResizeFilter.Bicubic, limits))
            Assert.Equal(expected, OriginalPixels.Bytes(recovered));
        Assert.Equal(input, OriginalPixels.Bytes(source));
        Assert.Equal(0, root.Bytes); Assert.False(root.IsDisposed);
    }
}
