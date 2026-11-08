using System.Buffers.Binary;
using System.Reflection;
using System.Security.Cryptography;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
using Lokad.Image.Internal;
using Lokad.Image.Tooling;

namespace Lokad.Image.Tests;

public sealed class BicubicByteNormalizationTests
{
    [Fact]
    public void QuartetsKeepBothOriginalPairDecisionsAndRejectedStores()
    {
        if (!Avx512F.IsSupported) return;
        var cases = new List<(Vector512<double> Sum, double N0, double N1)>();
        foreach (var alpha in new[] { 0d, 1e-9, .5, 1, 16, 255, 1020 })
        foreach (var normalizer in new[] { .499999, .5, 4d })
        foreach (var color in new[] { -255d, 0, Math.BitDecrement(.5), .5, Math.BitIncrement(.5), 13, 127.5, 255, 510 })
            cases.Add((Vector512.Create(color * alpha, 13 * alpha, 241 * alpha, alpha,
                53 * alpha, color * alpha, 239 * alpha, alpha), normalizer, normalizer));
        Span<byte> expected = stackalloc byte[20]; Span<byte> actual = stackalloc byte[20];
        var seen = 0;
        for (var i = 0; i < cases.Count; i++)
        {
            var a = cases[i]; var b = cases[(i * 37 + 19) % cases.Count];
            expected.Fill(0xa7); actual.Fill(0xa7);
            var first = BicubicMath.TryWriteBytePair(a.Sum, a.N0, a.N1, expected.Slice(2, 8));
            var second = BicubicMath.TryWriteBytePair(b.Sum, b.N0, b.N1, expected.Slice(10, 8));
            var mask = BicubicMath.WriteByteQuartet(a.Sum, b.Sum, a.N0, a.N1, b.N0, b.N1, actual.Slice(2, 16));
            Assert.Equal((first ? 1 : 0) | (second ? 2 : 0), mask);
            Assert.Equal(expected.ToArray(), actual.ToArray()); seen |= 1 << mask;
        }
        Assert.Equal(15, seen);
    }

    [Theory]
    [InlineData(513, 259, 1027, 129, true)]
    [InlineData(127, 259, 259, 129, true)]
    [InlineData(67, 387, 139, 193, true)]
    [InlineData(17, 137, 35, 65, false)]
    [InlineData(513, 259, 513, 129, false)]
    public void ByteBandsPreserveFramesSuffixesAndBorrowedStorage(int sourceWidth, int sourceHeight,
        int width, int height, bool usesBank)
    {
        if (!Avx512F.IsSupported) return;
        using var source = OriginalPixels.Buffer(sourceWidth, sourceHeight, 8);
        if (sourceWidth != 513 || width != 1027)
            for (var y = 0; y < sourceHeight; y++) for (var x = 0; x < sourceWidth; x++)
                source.GetRow8(y)[x * 4 + 3] = 255;
        else source.GetRow8(0)[3] = source.GetRow8(0)[sourceWidth * 4 - 1] = 0;
        var input = OriginalPixels.Bytes(source);
        using var expected = ImageTransforms.Resize(source, width, height, ResizeFilter.Bicubic,
            new() { MaxWorkingBytes = input.Length + (long)width * height * 4 + 4095 });
        using var result = ImageBuffer.Create(width, height, 8); result.Pixels.Fill(0xa5);
        var located = MixedColumnBicubicResize.TryLocateResultWorkspace(result, out var start, out var safeRows);
        Assert.Equal(usesBank, width > sourceWidth && located);
        if (usesBank)
        {
            Assert.True((long)safeRows * result.Stride <= start);
            Assert.True(start + MixedColumnBicubicResize.ResultWorkspaceBytes <= result.ByteLength - width * 84L);
        }
        using var root = new TestMemoryBudget(4096);
        using var context = new OperationContext(new() { MaxWorkingBytes = 4096, MemoryBudget = root }, default);
        Assert.True(MixedColumnBicubicResize.TryApply(source, result, 1, (double)sourceHeight / height, 5, 10, context));
        Assert.Equal(OriginalPixels.Bytes(expected), OriginalPixels.Bytes(result));
        Assert.Equal(input, OriginalPixels.Bytes(source));
        Assert.Equal(0, root.Bytes); Assert.Equal(4096, root.Peak);
        Assert.Equal(4096, context.RemainingWorkingBytes); Assert.False(root.IsDisposed);
    }

    [Fact]
    public void ByteBandRefusalIsTerminalAndCancellationReleasesBeforeRecovery()
    {
        if (!Avx512F.IsSupported) return;
        using var source = OriginalPixels.Buffer(513, 259, 8);
        using var result = ImageBuffer.Create(1027, 129, 8);
        using var denied = new TestMemoryBudget(4095);
        using (var context = new OperationContext(new() { MaxWorkingBytes = 4096, MemoryBudget = denied }, default))
            Assert.Throws<ImageLimitException>(() => MixedColumnBicubicResize.TryApply(source, result, 1, 259d / 129, 5, 10, context));
        Assert.Equal(1, denied.Attempts); Assert.Equal(1, denied.Refusals); Assert.Equal(0, denied.Bytes);
        using var root = new TestMemoryBudget(4096);
        using var cancellation = new CancellationTokenSource(); root.Admitted = _ => cancellation.Cancel();
        using (var context = new OperationContext(new() { MaxWorkingBytes = 4096, MemoryBudget = root }, cancellation.Token))
            Assert.Throws<OperationCanceledException>(() => MixedColumnBicubicResize.TryApply(source, result, 1, 259d / 129, 5, 10, context));
        Assert.Equal(0, root.Bytes); root.Admitted = null;
        using (var context = new OperationContext(new() { MaxWorkingBytes = 4096, MemoryBudget = root }, default))
            Assert.True(MixedColumnBicubicResize.TryApply(source, result, 1, 259d / 129, 5, 10, context));
        Assert.Equal(0, root.Bytes); Assert.False(root.IsDisposed);
    }

    [Fact]
    public void ByteBandStackRetiresBeforeRequiredExactTinyAlphaReplay()
    {
        if (!Avx512F.IsSupported) return;
        using var source = ImageBuffer.Create(127, 259, 8);
        for (var y = 0; y < source.Height; y++) for (var x = 0; x < source.Width; x++)
        {
            var pixel = source.GetRow8(y).Slice(x * 4, 4);
            pixel[0] = 19; pixel[1] = 103; pixel[2] = 241; pixel[3] = y < 130 ? (byte)1 : (byte)0;
        }
        using var result = ImageBuffer.Create(259, 129, 8);
        using var root = new TestMemoryBudget(4096); var admissions = 0;
        using var cancellation = new CancellationTokenSource();
        root.Admitted = _ => { if (++admissions == 2) cancellation.Cancel(); };
        using (var context = new OperationContext(new() { MaxWorkingBytes = 4096, MemoryBudget = root }, cancellation.Token))
            Assert.Throws<OperationCanceledException>(() => MixedColumnBicubicResize.TryApply(source, result, 1, 259d / 129, 5, 10, context));
        Assert.Equal(2, admissions); Assert.Equal(0, root.Bytes); root.Admitted = null;
        using (var context = new OperationContext(new() { MaxWorkingBytes = 4096, MemoryBudget = root }, default))
            Assert.True(MixedColumnBicubicResize.TryApply(source, result, 1, 259d / 129, 5, 10, context));
        for (var y = 0; y < result.Height; y++) for (var x = 0; x < result.Width; x++)
            Assert.Equal(y <= 64 ? new byte[] { 19, 103, 241, 1 } : new byte[4], result.GetRow8(y).Slice(x * 4, 4).ToArray());
        Assert.Equal(4096, root.Peak); Assert.Equal(0, root.Bytes); Assert.False(root.IsDisposed);
    }

    [Fact]
    public void ByteBandsKeepAggregateAdmissionAndIndependentRetainedResults()
    {
        const int bytes = 1027 * 129 * 4;
        using var source = OriginalPixels.Buffer(513, 259, 8);
        source.GetRow8(0)[3] = source.GetRow8(0)[513 * 4 - 1] = 0;
        using var root = new TestMemoryBudget(2 * bytes + 4096);
        var limits = new ImageLimits { MaxWorkingBytes = source.ByteLength + bytes + 4096, MemoryBudget = root };
        using var first = ImageTransforms.Resize(source, 1027, 129, ResizeFilter.Bicubic, limits);
        using var second = ImageTransforms.Resize(source, 1027, 129, ResizeFilter.Bicubic, limits);
        var expected = OriginalPixels.Bytes(second);
        Assert.Equal("1ca9938539ba1421fe4f1540055be5d0675ac8ddc1ea87df5932a93ca35a0356", Convert.ToHexStringLower(SHA256.HashData(expected)));
        Assert.Equal(2 * bytes, root.Bytes); Assert.True(root.Peak <= 2 * bytes + 4096);
        if (Avx512F.IsSupported) Assert.Equal(2 * bytes + 4096, root.Peak);
        var attempts = root.Attempts;
        Assert.Throws<ImageLimitException>(() => ImageTransforms.Resize(source, 1027, 129, ResizeFilter.Bicubic, limits));
        Assert.Equal(attempts + 1, root.Attempts); Assert.Equal(2 * bytes, root.Bytes);
        first.GetRow8(0)[0] ^= 255; first.Dispose();
        Assert.Equal(bytes, root.Bytes); Assert.Equal(expected, OriginalPixels.Bytes(second));
        using var recovered = ImageTransforms.Resize(source, 1027, 129, ResizeFilter.Bicubic, limits);
        Assert.Equal(expected, OriginalPixels.Bytes(recovered)); Assert.False(root.IsDisposed);
    }

    private delegate bool PairWriter(Vector512<double> sum, double n0, double n1, Span<byte> output);
    private static readonly PairWriter Write = typeof(ColumnBicubicResize)
        .GetMethod("TryWritePair", BindingFlags.NonPublic | BindingFlags.Static)!.CreateDelegate<PairWriter>();

    [Theory]
    [InlineData(16, .5, 0)]
    [InlineData(16, .5, 1)]
    [InlineData(16, 4, 17)]
    [InlineData(17, 4, 127)]
    [InlineData(255, 4, 254)]
    [InlineData(255, .5, 255)]
    [InlineData(1020, 4, -255)]
    [InlineData(1020, 4, 510)]
    public void CertifiedPairsKeepClippingRoundingAndDestinationCanaries(double alpha, double normalizer, double color)
    {
        if (!Avx512F.IsSupported) return;
        Span<byte> destination = stackalloc byte[12];
        destination.Fill(0xa7);
        var sum = Vector512.Create(color * alpha, 13 * alpha, 241 * alpha, alpha,
            53 * alpha, color * alpha, 239 * alpha, alpha);
        Assert.True(Write(sum, normalizer, normalizer, destination.Slice(2, 8)));
        var expectedAlpha = (byte)Math.Clamp(Math.Round(alpha / normalizer, MidpointRounding.AwayFromZero), 0, 255);
        var expectedColor = (byte)Math.Clamp(color, 0, 255);
        Assert.Equal(new byte[] { expectedColor, 13, 241, expectedAlpha, 53, expectedColor, 239, expectedAlpha }, destination.Slice(2, 8).ToArray());
        Assert.Equal(0xa7a7u, BinaryPrimitives.ReadUInt16LittleEndian(destination));
        Assert.Equal(0xa7a7u, BinaryPrimitives.ReadUInt16LittleEndian(destination.Slice(10)));
    }

    [Theory]
    [InlineData(0)] [InlineData(1)] [InlineData(127)] [InlineData(254)]
    public void HalfDecisionsRefuseWithoutWriting(int integer)
    {
        if (!Avx512F.IsSupported) return;
        var half = integer + .5;
        Span<byte> destination = stackalloc byte[8];
        foreach (var value in new[] { Math.BitDecrement(half), half, Math.BitIncrement(half) })
        {
            destination.Fill(0xa7);
            var sum = Vector512.Create(value * 16, 16d, 16d, 16d, 16d, 16d, 16d, 16d);
            Assert.False(Write(sum, 4, 4, destination));
            Assert.Equal(0xa7a7a7a7a7a7a7a7ul, BinaryPrimitives.ReadUInt64LittleEndian(destination));
        }
    }

    [Theory]
    [InlineData(15.999999, 4)]
    [InlineData(16, .499999999)]
    [InlineData(16, 16)]
    public void UncertainAlphaRefusesWithoutWriting(double alpha, double normalizer)
    {
        if (!Avx512F.IsSupported) return;
        Span<byte> destination = stackalloc byte[8];
        destination.Fill(0xa7);
        var sum = Vector512.Create(alpha, alpha, alpha, alpha, alpha, alpha, alpha, alpha);
        Assert.False(Write(sum, normalizer, normalizer, destination));
        Assert.Equal(0xa7a7a7a7a7a7a7a7ul, BinaryPrimitives.ReadUInt64LittleEndian(destination));
    }
}
