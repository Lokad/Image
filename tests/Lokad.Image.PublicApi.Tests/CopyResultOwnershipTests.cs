using System.Buffers.Binary;
using Lokad.Image.Tooling;

namespace Lokad.Image.PublicApi.Tests;

public sealed class CopyResultOwnershipTests
{
    public static IEnumerable<object[]> Profiles()
    {
        foreach (var depth in new[] { 8, 16 })
        foreach (var operation in new[] { "clone", "crop", "hflip", "vflip", "half", "cw" })
            yield return [depth, operation];
    }

    private static ImageBuffer Apply(ImageBuffer source, string operation, ImageLimits limits, CancellationToken cancellation = default)
        => operation switch
        {
            "clone" => source.Clone(limits, cancellation),
            "crop" => ImageTransforms.Crop(source, 1, 1, source.Width - 2, source.Height - 2, limits, cancellation),
            "hflip" => ImageTransforms.FlipHorizontal(source, limits, cancellation),
            "vflip" => ImageTransforms.FlipVertical(source, limits, cancellation),
            "half" => ImageTransforms.Rotate(source, QuarterTurn.Half, limits, cancellation),
            "cw" => ImageTransforms.Rotate(source, QuarterTurn.Clockwise90, limits, cancellation),
            _ => throw new ArgumentOutOfRangeException(nameof(operation))
        };

    private static ImageBuffer Source(int depth, ImageLimits limits)
    {
        var image = ImageBuffer.Create(17, 9, depth, limits);
        var pixels = OriginalPixels.Generate(17, 9, depth);
        for (var y = 0; y < image.Height; y++)
        {
            if (depth == 8) pixels.AsSpan(y * image.Stride, image.Stride).CopyTo(image.GetRow8(y));
            else
                for (var x = 0; x < image.Width * 4; x++)
                    image.GetRow16(y)[x] = BinaryPrimitives.ReadUInt16LittleEndian(pixels.AsSpan(y * image.Stride + x * 2));
        }
        return image;
    }

    [Theory]
    [MemberData(nameof(Profiles))]
    public void ReturnedPixelsKeepTheirOwnChargeAndRefusalDoesNotConsumeTheBorrowedRoot(int depth, string operation)
    {
        var sourceBytes = 17 * 9 * 4 * (depth / 8);
        var resultBytes = operation == "crop" ? 15 * 7 * 4 * (depth / 8) : sourceBytes;
        using var root = new TestMemoryBudget(sourceBytes + resultBytes);
        using var caller = root.CreateScope();
        var limits = new ImageLimits { MemoryBudget = caller, MaxWorkingBytes = sourceBytes + resultBytes };
        using var source = Source(depth, limits);
        var original = OriginalPixels.Bytes(source);
        using var first = Apply(source, operation, limits);
        var saved = OriginalPixels.Bytes(first);
        Assert.Equal(sourceBytes + resultBytes, root.Bytes);
        var attempts = root.Attempts;
        var failure = Assert.Throws<ImageLimitException>(() => Apply(source, operation, limits));
        Assert.Equal((ImageLimitKind.WorkingBytes, ImageLimitScope.SharedBudget), (failure.Limit, failure.Scope));
        Assert.Equal(attempts + 1, root.Attempts);
        Assert.Equal(1, root.Refusals);
        Assert.Equal(sourceBytes + resultBytes, root.Bytes);
        Assert.Equal(original, OriginalPixels.Bytes(source));
        Assert.Equal(saved, OriginalPixels.Bytes(first));
        source.Dispose();
        Assert.Equal(resultBytes, root.Bytes);
        Assert.Equal(saved, OriginalPixels.Bytes(first));
        using (var next = first.Clone(limits))
        {
            Assert.Equal(2 * resultBytes, root.Bytes);
            Assert.Equal(saved, OriginalPixels.Bytes(next));
        }
        first.Dispose();
        Assert.Equal(0, root.Bytes);
        Assert.True(caller.TryReserve(1, out var borrowedScopeCheck));
        borrowedScopeCheck.Dispose();
        Assert.Equal(0, root.Bytes);
        Assert.False(root.IsDisposed);
    }

    [Theory]
    [InlineData("clone")] [InlineData("crop")] [InlineData("hflip")]
    [InlineData("vflip")] [InlineData("half")] [InlineData("cw")]
    public void CancellationAtOutputAdmissionReleasesOnlyTheFailedResultAndAllowsRecovery(string operation)
    {
        using var root = new TestMemoryBudget(1_000_000);
        using var cancellation = new CancellationTokenSource();
        var limits = new ImageLimits { MemoryBudget = root };
        using var source = Source(8, limits);
        var original = OriginalPixels.Bytes(source); var retained = root.Bytes;
        root.Admitted = _ => cancellation.Cancel();
        var attempts = root.Attempts;
        var failure = Assert.ThrowsAny<OperationCanceledException>(() => Apply(source, operation, limits, cancellation.Token));
        Assert.Equal(cancellation.Token, failure.CancellationToken);
        Assert.Equal(attempts + 1, root.Attempts);
        Assert.Equal(retained, root.Bytes);
        Assert.Equal(original, OriginalPixels.Bytes(source));
        Assert.Equal(0, root.Refusals);
        root.Admitted = null;
        using (var recovered = Apply(source, operation, limits))
            Assert.Equal(retained + recovered.Stride * recovered.Height, root.Bytes);
        Assert.Equal(retained, root.Bytes);
        source.Dispose();
        Assert.Equal(0, root.Bytes);
        Assert.False(root.IsDisposed);
    }

    [Theory]
    [InlineData("clone")] [InlineData("crop")] [InlineData("hflip")]
    [InlineData("vflip")] [InlineData("half")] [InlineData("cw")]
    public void PolicyCancellationDisposedInputAndLocalLimitsKeepTheirFailureOrder(string operation)
    {
        using var source = Source(8, ImageLimits.Default);
        using var root = new TestMemoryBudget(0);
        var limits = new ImageLimits { MemoryBudget = root, MaxWorkingBytes = source.Stride * source.Height - 1, MaxDimension = 1 };
        var borrowed = Assert.Throws<ImageLimitException>(() => Apply(source, operation, limits));
        Assert.Equal((ImageLimitKind.WorkingBytes, ImageLimitScope.Operation), (borrowed.Limit, borrowed.Scope));
        var dimension = Assert.Throws<ImageLimitException>(() => Apply(source, operation, limits with { MaxWorkingBytes = 1_000_000 }));
        Assert.Equal(ImageLimitKind.Dimension, dimension.Limit);
        var pixels = Assert.Throws<ImageLimitException>(() => Apply(source, operation,
            limits with { MaxWorkingBytes = 1_000_000, MaxDimension = 32768, MaxPixels = 1 }));
        Assert.Equal(ImageLimitKind.PixelCount, pixels.Limit);
        Assert.Equal(0, root.Attempts);
        var canceled = new CancellationToken(true);
        Assert.Throws<ArgumentOutOfRangeException>(() => Apply(source, operation, limits with { MaxWorkingBytes = -1 }, canceled));
        source.Dispose();
        Assert.ThrowsAny<OperationCanceledException>(() => Apply(source, operation, limits, canceled));
        Assert.Throws<ArgumentOutOfRangeException>(() => Apply(source, operation, limits with { MaxWorkingBytes = -1 }));
        Assert.Throws<ObjectDisposedException>(() => Apply(source, operation, limits));
        Assert.Equal(0, root.Attempts);
        Assert.Equal(0, root.Bytes);
        Assert.False(root.IsDisposed);
    }
}
