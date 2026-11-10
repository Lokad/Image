using System.Runtime.Intrinsics.X86;
using System.Security.Cryptography;
using Lokad.Image.Internal;
using Lokad.Image.Tooling;

namespace Lokad.Image.Tests;

public sealed class WordColumnBicubicGeometryTests
{
    [Fact]
    public void FrozenPackedWordPixelsKeepSharedAdmissionAndIndependentSurvivors()
    {
        if (!Avx512F.IsSupported) return;
        using var source = OriginalPixels.Buffer(513, 259, 16);
        source.GetRow16(0)[3] = source.GetRow16(0)[513 * 4 - 1] = 0;
        var input = OriginalPixels.Bytes(source);
        const int bytes = 64 * 32 * 8;
        const string expectedHash = "59db672430bfd4c9fc4468faae4ae846a730320b185be54196676589b81fb5ce";
        using var root = new TestMemoryBudget(2 * bytes + 4096);
        using var first = ImageBuffer.Create(64, 32, 16, new() { MemoryBudget = root });
        first.Pixels.Fill(0xa5);
        using (var context = new OperationContext(new() { MaxWorkingBytes = 4096, MemoryBudget = root }, default))
            Assert.True(ApplyPacked(source, first, context));
        Assert.Equal(expectedHash, Convert.ToHexStringLower(SHA256.HashData(OriginalPixels.Bytes(first))));
        Assert.Equal(bytes, root.Bytes);
        var limits = new ImageLimits { MaxWorkingBytes = source.ByteLength + bytes + 4096, MemoryBudget = root };
        using var second = ImageTransforms.Resize(source, 64, 32, ResizeFilter.Bicubic, limits);
        Assert.Equal(expectedHash, Convert.ToHexStringLower(SHA256.HashData(OriginalPixels.Bytes(second))));
        Assert.Equal(2 * bytes, root.Bytes); Assert.True(root.Peak <= 2 * bytes + 4096);
        var attempts = root.Attempts;
        Assert.Throws<ImageLimitException>(() => ImageTransforms.Resize(source, 64, 32, ResizeFilter.Bicubic, limits));
        Assert.Equal(attempts + 1, root.Attempts); Assert.Equal(2 * bytes, root.Bytes);
        first.Dispose(); Assert.Equal(bytes, root.Bytes);
        using (var recovered = ImageTransforms.Resize(source, 64, 32, ResizeFilter.Bicubic, limits))
        {
            Assert.Equal(expectedHash, Convert.ToHexStringLower(SHA256.HashData(OriginalPixels.Bytes(recovered))));
            Assert.Equal(2 * bytes, root.Bytes);
        }
        Assert.Equal(expectedHash, Convert.ToHexStringLower(SHA256.HashData(OriginalPixels.Bytes(second))));
        second.Dispose(); Assert.Equal(0, root.Bytes);
        Assert.Equal(input, OriginalPixels.Bytes(source)); Assert.False(root.IsDisposed);
    }

    [Theory]
    [InlineData(257, 129, 41, 33, false)] // Odd stride retains the packed suffix.
    [InlineData(256, 193, 40, 33, true)]
    [InlineData(128, 193, 20, 33, true)] // Minimum four-column prefix.
    [InlineData(128, 193, 16, 33, false)] // No cache footer fits.
    [InlineData(256, 128, 40, 21, false)] // Fewer than32 cache rows.
    [InlineData(385, 193, 65, 33, false)] // Different support and odd final strip.
    public void PackedWordPrefixAndSuffixMatchTheIndependentDirectPath(int width, int height, int outputWidth, int outputHeight, bool usesLoan)
    {
        if (!Avx512F.IsSupported) return;
        using var source = OriginalPixels.Buffer(width, height, 16, opaque: true);
        var input = OriginalPixels.Bytes(source);
        using var expected = ImageTransforms.Resize(source, outputWidth, outputHeight, ResizeFilter.Bicubic,
            new() { MaxWorkingBytes = source.ByteLength + outputWidth * outputHeight * 8 });
        using var result = ImageBuffer.Create(outputWidth, outputHeight, 16); result.Pixels.Fill(0xa5);
        using var root = new TestMemoryBudget(4096);
        using var context = new OperationContext(new() { MaxWorkingBytes = 4096, MemoryBudget = root }, default);
        Assert.Equal(usesLoan, PackedColumnBicubicResize.TryLocateResultCache(result,
            (double)width / outputWidth, (double)height / outputHeight, out var start, out var prefix));
        if (usesLoan)
        {
            Assert.Equal(result.Stride - 128, start); Assert.True(prefix * 8 <= start);
            Assert.True(31L * result.Stride + start + 128 <= result.ByteLength);
        }
        Assert.True(ApplyPacked(source, result, context));
        Assert.Equal(OriginalPixels.Bytes(expected), OriginalPixels.Bytes(result));
        Assert.Equal(input, OriginalPixels.Bytes(source));
        Assert.Equal(0, root.Bytes); Assert.Equal(4096, root.Peak); Assert.False(root.IsDisposed);
    }

    [Fact]
    public void PackedWordRefusalAndCancellationLeaveTheLoanUnpublishedAndAllowRecovery()
    {
        if (!Avx512F.IsSupported) return;
        using var source = ImageBuffer.Create(513, 259, 16);
        for (var y = 0; y < source.Height; y++) source.GetRow16(y).Fill(ushort.MaxValue);
        var input = OriginalPixels.Bytes(source);
        using var result = ImageBuffer.Create(64, 32, 16); result.Pixels.Fill(0xa5);
        using var root = new TestMemoryBudget(4096);
        using (var held = root.CreateScope())
        {
            Assert.True(held.TryReserve(1, out var reservation));
            using var refused = new OperationContext(new() { MaxWorkingBytes = 4096, MemoryBudget = root }, default);
            Assert.Throws<ImageLimitException>(() => ApplyPacked(source, result, refused));
            Assert.Equal(1, root.Refusals); Assert.Equal(1, root.Bytes);
            Assert.Equal(-1, result.Pixels.IndexOfAnyExcept((byte)0xa5));
        }
        using var cancellation = new CancellationTokenSource(); root.Admitted = _ => cancellation.Cancel();
        using (var canceled = new OperationContext(new() { MaxWorkingBytes = 4096, MemoryBudget = root }, cancellation.Token))
            Assert.Throws<OperationCanceledException>(() => ApplyPacked(source, result, canceled));
        Assert.Equal(0, root.Bytes); Assert.Equal(-1, result.Pixels.IndexOfAnyExcept((byte)0xa5));
        root.Admitted = null;
        using (var context = new OperationContext(new() { MaxWorkingBytes = 4096, MemoryBudget = root }, default))
            Assert.True(ApplyPacked(source, result, context));
        for (var y = 0; y < result.Height; y++) Assert.All(result.GetRow16(y).ToArray(), value => Assert.Equal(ushort.MaxValue, value));
        Assert.Equal(input, OriginalPixels.Bytes(source));
        Assert.Equal(0, root.Bytes); Assert.Equal(4096, root.Peak); Assert.False(root.IsDisposed);
    }

    [Fact]
    public void PackedWordTinyAlphaUnwindsTheLoanBeforeExactPublicReplay()
    {
        if (!Avx512F.IsSupported) return;
        using var source = ImageBuffer.Create(513, 259, 16);
        for (var y = 0; y < source.Height; y++) for (var x = 0; x < source.Width; x++)
        {
            var pixel = source.GetRow16(y).Slice(x * 4, 4);
            pixel[0] = 19; pixel[1] = 103; pixel[2] = 241; pixel[3] = y < 130 ? (ushort)1 : (ushort)0;
        }
        var input = OriginalPixels.Bytes(source);
        using var result = ImageBuffer.Create(64, 32, 16); result.Pixels.Fill(0xa5);
        using var root = new TestMemoryBudget(result.ByteLength + 4096);
        using (var context = new OperationContext(new() { MaxWorkingBytes = 4096, MemoryBudget = root }, default))
            Assert.False(ApplyPacked(source, result, context));
        Assert.Equal(0, root.Bytes);
        using (var recovered = ImageTransforms.Resize(source, 64, 32, ResizeFilter.Bicubic,
            new() { MaxWorkingBytes = source.ByteLength + result.ByteLength + 4096, MemoryBudget = root }))
        {
            Assert.Equal(result.ByteLength, root.Bytes);
            for (var y = 0; y < recovered.Height; y++) for (var x = 0; x < recovered.Width; x++)
                Assert.Equal(y < 16 ? new ushort[] { 19, 103, 241, 1 } : new ushort[4], recovered.GetRow16(y).Slice(x * 4, 4).ToArray());
        }
        Assert.Equal(input, OriginalPixels.Bytes(source)); Assert.Equal(0, root.Bytes);
        Assert.True(root.Peak <= result.ByteLength + 4096); Assert.False(root.IsDisposed);
    }

    [Theory]
    [InlineData(0)] [InlineData(1)] [InlineData(2)]
    [InlineData(32769)] [InlineData(65535)]
    public void WordProductsAcrossSignedBoundaryKeepExactConstantSamplesAndChargedResult(int alpha)
    {
        using var source = ImageBuffer.Create(320, 256, 16);
        for (var y = 0; y < source.Height; y++) for (var x = 0; x < source.Width; x++)
        {
            var pixel = source.GetRow16(y).Slice(x * 4, 4);
            pixel[0] = 65535; pixel[1] = 32768; pixel[2] = 32767; pixel[3] = (ushort)alpha;
        }
        var input = OriginalPixels.Bytes(source);
        const int outputBytes = 40 * 32 * 8;
        using var root = new TestMemoryBudget(outputBytes + 4096);
        using (var result = ImageTransforms.Resize(source, 40, 32, ResizeFilter.Bicubic,
            new() { MaxWorkingBytes = source.ByteLength + outputBytes + 4096, MemoryBudget = root }))
        {
            Assert.Equal(outputBytes, root.Bytes);
            for (var y = 0; y < result.Height; y++) for (var x = 0; x < result.Width; x++)
                Assert.Equal(alpha == 0 ? new ushort[4] : new ushort[] { 65535, 32768, 32767, (ushort)alpha },
                    result.GetRow16(y).Slice(x * 4, 4).ToArray());
        }
        Assert.Equal(0, root.Bytes); Assert.True(root.Peak <= outputBytes + 4096);
        Assert.Equal(input, OriginalPixels.Bytes(source)); Assert.False(root.IsDisposed);
    }

    private static bool ApplyPacked(ImageBuffer source, ImageBuffer result, OperationContext context)
    {
        var sx = (double)source.Width / result.Width; var sy = (double)source.Height / result.Height;
        return PackedColumnBicubicResize.TryApply(source, result, sx, sy,
            (long)Math.Ceiling(4 * sx) + 1, (long)Math.Ceiling(4 * sy) + 1, context);
    }

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
    [InlineData(324, 128, 144, 64)] // Cached prefix leaves the last prepared strip out of phase.
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
