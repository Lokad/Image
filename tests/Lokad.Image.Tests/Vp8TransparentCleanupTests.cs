using Lokad.Image.Internal;
using Lokad.Image.Tooling;
using static Lokad.Image.Tests.Vp8PreparedInputTests;

namespace Lokad.Image.Tests;

public sealed class Vp8TransparentCleanupTests
{
    private const string BandsHash = "da30dc07df6ab6beec72da32ecb8bfb78980a134e68dd9b86d4ae61fb5d02e7f";

    // One original branch control: two complete8-row bands with transparent
    // horizontal runs, visible resets, and partial right/bottom edges. Existing
    // singleton/two-color inputs are too short to exercise complete-block runs.
    private static ImageBuffer Bands()
    {
        var pixels = OriginalPixels.Generate(33, 17, 8);
        for (var y = 0; y < 17; y++)
        for (var x = 0; x < 33; x++)
        {
            var zero = y < 8 && (x < 16 || x is >= 24 and < 32)
                || y is >= 8 and < 16 && x is >= 8 and < 24 || (x + 3 * y) % 11 == 0;
            pixels[(y * 33 + x) * 4 + 3] = zero ? (byte)0 : (byte)((x * 17 + y * 23) % 255 + 1);
        }
        var image = ImageBuffer.Create(33, 17, 8);
        for (var y = 0; y < 17; y++) pixels.AsSpan(y * 33 * 4, 33 * 4).CopyTo(image.GetRow8(y));
        return image;
    }

    [Theory]
    [MemberData(nameof(Controls), MemberType = typeof(Vp8PreparedInputTests))]
    public void CleanupMatchesNativeWithoutChangingAlphaOrOwnedCapacity(string name, int depth, string rawHash)
    {
        using var root = new TestMemoryBudget(16_000_000); using var source = Source(name, depth, root);
        var original = OriginalPixels.Bytes(source); var sourceBytes = root.Bytes;
        using var context = new OperationContext(new() { MemoryBudget = root }, default);
        using var input = Vp8PreparedInput.Create(source, context);
        var alpha = input.Alpha.ToArray(); var peak = root.Peak; var attempts = root.Attempts;
        input.CleanupTransparentArea();
        var expected = name == "asymmetric" ? "ed906b1f159183b9dc46a1c4f1ba0e3273f4afb99a93ad6de832010590482cea" : rawHash;
        Assert.Equal(expected, Hash(input)); Assert.Equal(alpha, input.Alpha.ToArray());
        Assert.Equal(original, OriginalPixels.Bytes(source));
        Assert.Equal(sourceBytes + input.CapacityBytes, root.Bytes); Assert.Equal(peak, root.Peak); Assert.Equal(attempts, root.Attempts);
        Padding(input.Y, input.Stride, input.Width, input.Height);
        Padding(input.U, input.Stride / 2, (input.Width + 1) / 2, (input.Height + 1) / 2);
        Padding(input.V, input.Stride / 2, (input.Width + 1) / 2, (input.Height + 1) / 2);
        input.CleanupTransparentArea(); Assert.Equal(expected, Hash(input));
        input.Dispose(); Assert.Equal(sourceBytes, root.Bytes); Assert.False(root.IsDisposed);
    }

    [Fact]
    public void CompleteTransparentRunsAndPartialEdgesMatchNativeAndPreserveVisibleLuma()
    {
        using var source = Bands(); using var root = new TestMemoryBudget(100_000);
        using var context = new OperationContext(new() { MemoryBudget = root }, default);
        using var input = Vp8PreparedInput.Create(source, context);
        Assert.Equal("e59538e95a097aff66ce58c6bd1831c8c284786b95d4b2c3b932fba64b53bddf", Hash(input));
        var yBefore = input.Y.ToArray(); var alpha = input.Alpha.ToArray(); var charged = root.Bytes;
        input.CleanupTransparentArea(); Assert.Equal(BandsHash, Hash(input)); Assert.Equal(charged, root.Bytes);
        Assert.Equal(alpha, input.Alpha.ToArray());
        for (var y = 0; y < input.Height; y++)
        for (var x = 0; x < input.Width; x++)
            if (alpha[y * input.Width + x] != 0) Assert.Equal(yBefore[y * input.Stride + x], input.Y[y * input.Stride + x]);
        Padding(input.Y, input.Stride, input.Width, input.Height);
        Padding(input.U, input.Stride / 2, (input.Width + 1) / 2, (input.Height + 1) / 2);
        Padding(input.V, input.Stride / 2, (input.Width + 1) / 2, (input.Height + 1) / 2);
    }

    [Fact]
    public void RetainedInputBlocksAnotherOperationUntilOwnerRelease()
    {
        using var source = Bands(); long capacity;
        using (var sizing = new OperationContext(ImageLimits.Default, default))
        using (var sized = Vp8PreparedInput.Create(source, sizing)) capacity = sized.CapacityBytes;
        using var root = new TestMemoryBudget(capacity + 1024);
        using var first = new OperationContext(new() { MemoryBudget = root }, default);
        using var retained = Vp8PreparedInput.Create(source, first); retained.CleanupTransparentArea();
        using var other = new OperationContext(new() { MemoryBudget = root }, default);
        var error = Assert.Throws<ImageLimitException>(() => Vp8PreparedInput.Create(source, other));
        Assert.Equal(ImageLimitScope.SharedBudget, error.Scope); Assert.Equal(capacity, root.Bytes);
        Assert.Equal(BandsHash, Hash(retained)); retained.Dispose(); Assert.Equal(0, root.Bytes);
        using var next = Vp8PreparedInput.Create(source, other); next.CleanupTransparentArea();
        Assert.Equal(BandsHash, Hash(next)); next.Dispose(); Assert.Equal(0, root.Bytes); Assert.False(root.IsDisposed);
    }

    [Fact]
    public void CancelledCleanupKeepsOwnedCapacityUntilDisposalThenAllowsRecovery()
    {
        using var source = Bands(); using var root = new TestMemoryBudget(100_000); using var cancel = new CancellationTokenSource();
        using var context = new OperationContext(new() { MemoryBudget = root }, cancel.Token);
        using var input = Vp8PreparedInput.Create(source, context); var retained = root.Bytes; cancel.Cancel();
        Assert.Throws<OperationCanceledException>(() => input.CleanupTransparentArea());
        Assert.Equal(retained, root.Bytes); input.Dispose(); Assert.Equal(0, root.Bytes);
        using var later = new OperationContext(new() { MemoryBudget = root }, default);
        using var next = Vp8PreparedInput.Create(source, later); next.CleanupTransparentArea(); Assert.Equal(BandsHash, Hash(next));
    }

    [Fact]
    public void LateCleanupWorkFailureDoesNotReleaseStorageBeforeItsOwner()
    {
        using var source = Bands(); long work;
        using (var sizing = new OperationContext(ImageLimits.Default, default))
        using (var input = Vp8PreparedInput.Create(source, sizing))
        { input.CleanupTransparentArea(); work = sizing.ConsumedWork; }
        using var root = new TestMemoryBudget(100_000);
        using var context = new OperationContext(new() { MemoryBudget = root, MaxWork = work - 1 }, default);
        using var failed = Vp8PreparedInput.Create(source, context);
        var error = Assert.Throws<ImageLimitException>(() => failed.CleanupTransparentArea());
        Assert.Equal(ImageLimitKind.Work, error.Limit); Assert.Equal(failed.CapacityBytes, root.Bytes);
        failed.Dispose(); Assert.Equal(0, root.Bytes);
        using var later = new OperationContext(new() { MemoryBudget = root }, default);
        using var next = Vp8PreparedInput.Create(source, later); next.CleanupTransparentArea(); Assert.Equal(BandsHash, Hash(next));
    }
}
