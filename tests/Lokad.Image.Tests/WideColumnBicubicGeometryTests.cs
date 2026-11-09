using System.Runtime.Intrinsics.X86;
using System.Security.Cryptography;
using Lokad.Image.Internal;
using Lokad.Image.Tooling;

namespace Lokad.Image.Tests;

public sealed class WideColumnBicubicGeometryTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void UncertainBytePixelsRepairLocallyIncludingSingletonTail(bool transparent)
    {
        if (!Avx512F.IsSupported) return;
        using var source = ImageBuffer.Create(68, 68, 8);
        for (var y = 0; y < source.Height; y++) for (var x = 0; x < source.Width; x++)
        {
            var pixel = source.GetRow8(y).Slice(x * 4, 4);
            pixel[0] = (byte)(x & 1); pixel[1] = (byte)(y & 1);
            pixel[2] = (byte)((x + y) & 1); pixel[3] = transparent ? (byte)0 : byte.MaxValue;
        }
        var input = OriginalPixels.Bytes(source);
        // Exact fourfold reduction creates interior half ties. Width17 also
        // exercises a singleton after paired strips. With no scratch allowance,
        // the public reference uses direct source-order evaluation.
        using var expected = ImageTransforms.Resize(source, 17, 17, limits:
            new() { MaxWorkingBytes = source.ByteLength + 17 * 17 * 4 });
        using var result = ImageBuffer.Create(17, 17, 8);
        result.Pixels.Fill(0xa5);
        using var root = new TestMemoryBudget(4096);
        using var context = new OperationContext(new() { MaxWorkingBytes = 4096, MemoryBudget = root }, default);
        Assert.True(Apply(source, result, context));
        Assert.Equal(OriginalPixels.Bytes(expected), OriginalPixels.Bytes(result));
        Assert.Equal(input, OriginalPixels.Bytes(source));
        Assert.Equal(0, root.Bytes); Assert.Equal(4096, root.Peak);
        Assert.Equal(4096, context.RemainingWorkingBytes); Assert.False(root.IsDisposed);
        if (transparent) Assert.Equal(-1, result.Pixels.IndexOfAnyExcept((byte)0));
    }

    [Theory]
    [InlineData(8, 513, 259, 128, 64)] // Four-column preparation and tail retirement.
    [InlineData(16, 513, 259, 128, 64)] // Four-column word preparation.
    [InlineData(16, 385, 193, 96, 48)] // A second eligible word shape.
    [InlineData(8, 528, 264, 128, 64)] // Exact4.125 boundary,17slots.
    [InlineData(16, 528, 264, 128, 64)]
    [InlineData(8, 307, 191, 75, 47)] // Four-column partial final strip.
    [InlineData(16, 307, 191, 75, 47)]
    [InlineData(8, 307, 197, 75, 47)] // Larger support, three columns and partial strip.
    [InlineData(16, 307, 197, 75, 47)]
    [InlineData(8, 49, 41, 11, 9)] // Result too narrow to retain geometry.
    [InlineData(16, 49, 41, 11, 9)]
    public void WideStripsMatchIndependentDirectPixels(int depth, int width, int height, int outputWidth, int outputHeight)
    {
        if (!Avx512F.IsSupported) return;
        using var source = OriginalPixels.Buffer(width, height, depth);
        // Opaque controls keep certification successful so the whole optimized
        // driver, including prepared and final strips, is compared directly.
        for (var y = 0; y < height; y++)
            for (var x = 0; x < width; x++)
                if (depth == 8) source.GetRow8(y)[x * 4 + 3] = byte.MaxValue;
                else source.GetRow16(y)[x * 4 + 3] = ushort.MaxValue;
        var input = OriginalPixels.Bytes(source);
        using var expected = ImageTransforms.Resize(source, outputWidth, outputHeight, ResizeFilter.Bicubic,
            new() { MaxWorkingBytes = source.ByteLength + outputWidth * outputHeight * (depth / 8) * 4 });
        using var result = ImageBuffer.Create(outputWidth, outputHeight, depth);
        result.Pixels.Fill(0xa5);
        using var root = new TestMemoryBudget(4096);
        using var context = new OperationContext(new() { MaxWorkingBytes = 4096, MemoryBudget = root }, default);
        Assert.True(Apply(source, result, context));
        Assert.Equal(OriginalPixels.Bytes(expected), OriginalPixels.Bytes(result));
        Assert.Equal(input, OriginalPixels.Bytes(source));
        Assert.Equal(0, root.Bytes); Assert.Equal(4096, root.Peak);
        Assert.Equal(4096, context.RemainingWorkingBytes); Assert.False(root.IsDisposed);
    }

    [Theory]
    [InlineData(8)]
    [InlineData(16)]
    public void FrozenWidePixelsKeepAggregateAdmissionAndIndependentSurvivors(int depth)
    {
        using var source = OriginalPixels.Buffer(513, 259, depth);
        if (depth == 8) source.GetRow8(0)[3] = source.GetRow8(0)[513 * 4 - 1] = 0;
        else source.GetRow16(0)[3] = source.GetRow16(0)[513 * 4 - 1] = 0;
        var input = OriginalPixels.Bytes(source);
        var bytes = 128 * 64 * 4 * (depth / 8);
        var expectedHash = depth == 8
            ? "5067190b92fe644a75b0001848c4a555feb638c72592239dbf0ea0d63c518e71"
            : "3763385e1f45803392214ccf0b87ef6e4f1ec22f2f37701108cc52e9a20209d9";
        using var root = new TestMemoryBudget(2 * bytes + 4096);
        var limits = new ImageLimits { MaxWorkingBytes = source.ByteLength + bytes + 4096, MemoryBudget = root };
        using var first = ImageTransforms.Resize(source, 128, 64, ResizeFilter.Bicubic, limits);
        using var second = ImageTransforms.Resize(source, 128, 64, ResizeFilter.Bicubic, limits);
        var expected = OriginalPixels.Bytes(second);
        Assert.Equal(expectedHash, Convert.ToHexStringLower(SHA256.HashData(expected)));
        Assert.Equal(2 * bytes, root.Bytes); Assert.True(root.Peak <= 2 * bytes + 4096);
        var attempts = root.Attempts;
        Assert.Throws<ImageLimitException>(() => ImageTransforms.Resize(source, 128, 64, ResizeFilter.Bicubic, limits));
        Assert.Equal(attempts + 1, root.Attempts); Assert.Equal(2 * bytes, root.Bytes);
        first.Pixels[0] ^= 255; first.Dispose();
        Assert.Equal(bytes, root.Bytes); Assert.Equal(expected, OriginalPixels.Bytes(second));
        using (var recovered = ImageTransforms.Resize(source, 128, 64, ResizeFilter.Bicubic, limits))
            Assert.Equal(expected, OriginalPixels.Bytes(recovered));
        Assert.Equal(input, OriginalPixels.Bytes(source)); source.Dispose();
        Assert.Equal(bytes, root.Bytes); Assert.Equal(expected, OriginalPixels.Bytes(second));
        Assert.False(root.IsDisposed);
    }

    [Theory]
    [InlineData(8)]
    [InlineData(16)]
    public void TinyAlphaUnwindsBeforePublicReplayAndReleasesWorkspace(int depth)
    {
        if (!Avx512F.IsSupported) return;
        using var source = ImageBuffer.Create(513, 259, depth);
        for (var y = 0; y < source.Height; y++) for (var x = 0; x < source.Width; x++)
        {
            if (depth == 8)
            {
                var pixel = source.GetRow8(y).Slice(x * 4, 4);
                pixel[0] = 19; pixel[1] = 103; pixel[2] = 241; pixel[3] = y < 130 ? (byte)1 : (byte)0;
            }
            else
            {
                var pixel = source.GetRow16(y).Slice(x * 4, 4);
                pixel[0] = 19; pixel[1] = 103; pixel[2] = 241; pixel[3] = y < 130 ? (ushort)1 : (ushort)0;
            }
        }
        var input = OriginalPixels.Bytes(source);
        using var result = ImageBuffer.Create(128, 64, depth); result.Pixels.Fill(0xa5);
        using var root = new TestMemoryBudget(result.ByteLength + 4096);
        using (var context = new OperationContext(new() { MaxWorkingBytes = 4096, MemoryBudget = root }, default))
            Assert.False(Apply(source, result, context));
        Assert.Equal(0, root.Bytes);
        using (var recovered = ImageTransforms.Resize(source, 128, 64, ResizeFilter.Bicubic,
            new() { MaxWorkingBytes = source.ByteLength + result.ByteLength + 4096, MemoryBudget = root }))
        {
            Assert.Equal(result.ByteLength, root.Bytes);
            for (var y = 0; y < recovered.Height; y++) for (var x = 0; x < recovered.Width; x++)
                if (depth == 8)
                    Assert.Equal(y < 32 ? new byte[] { 19, 103, 241, 1 } : new byte[4], recovered.GetRow8(y).Slice(x * 4, 4).ToArray());
                else
                    Assert.Equal(y < 32 ? new ushort[] { 19, 103, 241, 1 } : new ushort[4], recovered.GetRow16(y).Slice(x * 4, 4).ToArray());
        }
        Assert.Equal(0, root.Bytes); Assert.True(root.Peak <= result.ByteLength + 4096);
        Assert.Equal(input, OriginalPixels.Bytes(source)); Assert.False(root.IsDisposed);
    }

    [Theory]
    [InlineData(8)]
    [InlineData(16)]
    public void RefusalAndCancellationReleaseWorkspaceBeforeRecovery(int depth)
    {
        if (!Avx512F.IsSupported) return;
        using var source = OriginalPixels.Buffer(513, 259, depth);
        using var result = ImageBuffer.Create(128, 64, depth);
        var input = OriginalPixels.Bytes(source);
        result.Pixels.Fill(0xa5);
        using var root = new TestMemoryBudget(4096);
        using (var held = root.CreateScope())
        {
            Assert.True(held.TryReserve(1, out var reservation));
            using var refused = new OperationContext(new() { MaxWorkingBytes = 4096, MemoryBudget = root }, default);
            Assert.Throws<ImageLimitException>(() => Apply(source, result, refused));
            Assert.Equal(1, root.Bytes); Assert.Equal(-1, result.Pixels.IndexOfAnyExcept((byte)0xa5));
        }
        using var cancellation = new CancellationTokenSource();
        root.Admitted = _ => cancellation.Cancel();
        using (var canceled = new OperationContext(new() { MaxWorkingBytes = 4096, MemoryBudget = root }, cancellation.Token))
            Assert.Throws<OperationCanceledException>(() => Apply(source, result, canceled));
        root.Admitted = null;
        Assert.Equal(0, root.Bytes); Assert.Equal(-1, result.Pixels.IndexOfAnyExcept((byte)0xa5));
        using var context = new OperationContext(new() { MaxWorkingBytes = 4096, MemoryBudget = root }, default);
        _ = Apply(source, result, context); // Tiny-alpha certification may request exact replay.
        Assert.Equal(0, root.Bytes); Assert.Equal(4096, root.Peak);
        Assert.Equal(input, OriginalPixels.Bytes(source)); Assert.False(root.IsDisposed);
    }

    private static bool Apply(ImageBuffer source, ImageBuffer result, OperationContext context)
    {
        var sx = (double)source.Width / result.Width; var sy = (double)source.Height / result.Height;
        var tapsX = (long)Math.Ceiling(4 * sx) + 1; var tapsY = (long)Math.Ceiling(4 * sy) + 1;
        return WideColumnBicubicResize.TryApply(source, result, sx, sy, tapsX, tapsY, context);
    }
}
