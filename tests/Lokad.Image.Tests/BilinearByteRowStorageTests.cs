using System.Buffers.Binary;
using System.Reflection;
using System.Runtime.Intrinsics.X86;
using Lokad.Image.Internal;
using Lokad.Image.Tooling;

namespace Lokad.Image.Tests;

public sealed class BilinearByteRowStorageTests
{
    private delegate void Decode(ReadOnlySpan<byte> input, Span<int> output, CancellationToken cancellation);
    private static readonly Decode DecodeRow = typeof(BilinearUpsample)
        .GetMethod("DecodeRow", BindingFlags.NonPublic | BindingFlags.Static)!.CreateDelegate<Decode>();

    [Fact]
    public void PreparedSourceProductsMatchIndependentIntegersAndPreserveGuards()
    {
        if (!Avx2.IsSupported) return;
        uint[] pixels = [0xffffffff, 0x00abcdef, 0x0100ff00, 0x80808080, 0xff010203];
        var input = new byte[19 * 4];
        for (var x = 0; x < 19; x++) BinaryPrimitives.WriteUInt32LittleEndian(input.AsSpan(x * 4), pixels[x % pixels.Length]);
        var original = input.ToArray(); var storage = Enumerable.Repeat(int.MinValue, input.Length + 4).ToArray();
        DecodeRow(input, storage.AsSpan(2, input.Length), default);
        for (var x = 0; x < 19; x++)
        for (var channel = 0; channel < 4; channel++)
        {
            var expected = channel == 3 ? input[x * 4 + 3] : input[x * 4 + channel] * input[x * 4 + 3];
            Assert.Equal(expected, storage[2 + x * 4 + channel]);
        }
        Assert.Equal(new[] { int.MinValue, int.MinValue }, storage[..2]);
        Assert.Equal(new[] { int.MinValue, int.MinValue }, storage[^2..]);
        Assert.Equal(original, input);
    }

    [Fact]
    public void PrecancelledSourcePreparationLeavesStorageUntouchedAndFreshCallSucceeds()
    {
        if (!Avx2.IsSupported) return;
        var input = OriginalPixels.Generate(19, 1, 8); var output = Enumerable.Repeat(-1, input.Length).ToArray();
        using var cancellation = new CancellationTokenSource(); cancellation.Cancel();
        var error = Assert.Throws<OperationCanceledException>(() => DecodeRow(input, output, cancellation.Token));
        Assert.Equal(cancellation.Token, error.CancellationToken); Assert.All(output, x => Assert.Equal(-1, x));
        DecodeRow(input, output, default); Assert.All(output, x => Assert.InRange(x, 0, 65025));
    }

    [Theory]
    [InlineData(5, 3, 11, 7)] // Borrowed row, then fully published tail.
    [InlineData(5, 3, 6, 7)] // A source row is first needed after scratch retirement.
    [InlineData(1, 1, 2, 2)] // No vector consumer or room for a live loan.
    [InlineData(17, 5, 35, 11)] // Odd source/vector tails with partial transparency.
    public void CompleteRowsMatchDirectPixelsAndKeepIndependentOwnedResults(int width, int height, int outputWidth, int outputHeight)
    {
        using var source = OriginalPixels.Buffer(width, height, 8); var original = OriginalPixels.Bytes(source);
        var bytes = outputWidth * outputHeight * 4;
        using var expected = ImageTransforms.Resize(source, outputWidth, outputHeight, ResizeFilter.Bilinear,
            new() { MaxWorkingBytes = source.ByteLength + bytes + ResizeAlpha.ScratchBytes });
        using var root = new TestMemoryBudget(100_000);
        using var result = ImageTransforms.Resize(source, outputWidth, outputHeight, ResizeFilter.Bilinear,
            new() { MemoryBudget = root });
        Assert.Equal(OriginalPixels.Bytes(expected), OriginalPixels.Bytes(result));
        Assert.Equal(original, OriginalPixels.Bytes(source)); Assert.Equal(bytes, root.Bytes);
        source.Dispose(); var first = expected.GetRow8(0)[0]; result.GetRow8(0)[0] ^= 255;
        Assert.Equal(first, expected.GetRow8(0)[0]); Assert.NotEqual(first, result.GetRow8(0)[0]);
        result.Dispose(); Assert.Equal(0, root.Bytes); Assert.False(root.IsDisposed);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    public void AdmissionRefusalIsTerminalAndReleasesUnpublishedOutput(int phase)
    {
        using var source = OriginalPixels.Buffer(5, 3, 8); var original = OriginalPixels.Bytes(source);
        using var root = new TestMemoryBudget(100_000) { RefuseAtGrowth = phase };
        var limits = new ImageLimits { MemoryBudget = root };
        var error = Assert.Throws<ImageLimitException>(() => ImageTransforms.Resize(source, 11, 7, ResizeFilter.Bilinear, limits));
        Assert.Equal((ImageLimitKind.WorkingBytes, ImageLimitScope.SharedBudget), (error.Limit, error.Scope));
        Assert.Equal(phase, root.GrowthAttempts); Assert.Equal(1, root.Refusals); Assert.Equal(0, root.Bytes);
        Assert.Equal(original, OriginalPixels.Bytes(source)); root.RefuseAtGrowth = 0;
        using var result = ImageTransforms.Resize(source, 11, 7, ResizeFilter.Bilinear, limits);
        Assert.Equal(11 * 7 * 4, root.Bytes); result.Dispose(); Assert.Equal(0, root.Bytes);
    }

    [Fact]
    public void CancellationAfterPreparationAdmissionRefundsStorageAndAllowsAnotherOperation()
    {
        using var source = OriginalPixels.Buffer(5, 3, 8);
        using var root = new TestMemoryBudget(100_000); using var cancellation = new CancellationTokenSource();
        root.Admitted = _ => { if (root.GrowthAttempts == 2) cancellation.Cancel(); };
        var limits = new ImageLimits { MemoryBudget = root };
        var error = Assert.Throws<OperationCanceledException>(() =>
            ImageTransforms.Resize(source, 11, 7, ResizeFilter.Bilinear, limits, cancellation.Token));
        Assert.Equal(cancellation.Token, error.CancellationToken); Assert.Equal(0, root.Bytes);
        root.Admitted = null;
        using var recovered = ImageTransforms.Resize(source, 11, 7, ResizeFilter.Bilinear, limits);
        recovered.Dispose(); Assert.Equal(0, root.Bytes); Assert.False(root.IsDisposed);
    }
}
