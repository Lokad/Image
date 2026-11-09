using System.Runtime.Intrinsics.X86;
using System.Security.Cryptography;
using Lokad.Image.Internal;
using Lokad.Image.Tooling;

namespace Lokad.Image.Tests;

public sealed class WordColumnBicubicGeometryTests
{
    [Fact]
    public void FrozenWordPixelsKeepTheColumnPathAndFourKiBAdmission()
    {
        if (!Avx512F.IsSupported) return;
        using var source = OriginalPixels.Buffer(513, 259, 16);
        source.GetRow16(0)[3] = source.GetRow16(0)[513 * 4 - 1] = 0;
        var input = OriginalPixels.Bytes(source);
        using var result = ImageBuffer.Create(256, 129, 16);
        using var root = new TestMemoryBudget(4096);
        using var context = new OperationContext(new() { MaxWorkingBytes = 4096, MemoryBudget = root }, default);
        Assert.True(ColumnBicubicResize.TryApply(source, result, 513d / 256, 259d / 129, 10, 10, context));
        Assert.Equal("2676207be4d1a7c9109061fc3c8eb5a0d6b8d10a97bbbc611fbebda3330ffd9f",
            Convert.ToHexStringLower(SHA256.HashData(OriginalPixels.Bytes(result))));
        Assert.Equal(input, OriginalPixels.Bytes(source));
        Assert.Equal(0, root.Bytes); Assert.Equal(4096, root.Peak);
        Assert.Equal(4096, context.RemainingWorkingBytes); Assert.False(root.IsDisposed);
    }

    [Theory]
    [InlineData(151, 83, 75, 41)] // Eight-column path and a partial final strip.
    [InlineData(225, 99, 100, 44)] // Exact2.25 scale retains six-column scheduling.
    public void WordReductionAndSlotBoundaryMatchTheIndependentDirectPath(int width, int height, int outputWidth, int outputHeight)
    {
        if (!Avx512F.IsSupported) return;
        using var source = OriginalPixels.Buffer(width, height, 16);
        for (var y = 0; y < source.Height; y++)
        {
            var row = source.GetRow16(y);
            for (var x = 0; x < source.Width; x++) row[x * 4 + 3] = ushort.MaxValue;
        }
        var original = OriginalPixels.Bytes(source);
        using var expected = ImageTransforms.Resize(source, outputWidth, outputHeight, ResizeFilter.Bicubic,
            new() { MaxWorkingBytes = source.ByteLength + outputWidth * outputHeight * 8 });
        using var result = ImageBuffer.Create(outputWidth, outputHeight, 16);
        using var root = new TestMemoryBudget(4096);
        using var context = new OperationContext(new() { MaxWorkingBytes = 4096, MemoryBudget = root }, default);
        Assert.True(ColumnBicubicResize.TryApply(source, result, (double)width / outputWidth, (double)height / outputHeight, 10, 10, context));
        Assert.Equal(OriginalPixels.Bytes(expected), OriginalPixels.Bytes(result));
        Assert.Equal(original, OriginalPixels.Bytes(source));
        Assert.Equal(0, root.Bytes); Assert.Equal(4096, root.Peak); Assert.False(root.IsDisposed);
    }
}
