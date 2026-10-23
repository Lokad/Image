using System.Runtime.InteropServices;
using System.Security.Cryptography;
using Lokad.Image.Internal;
using Lokad.Image.Tooling;

namespace Lokad.Image.Tests;

public sealed class Vp8lPaletteAdjacencyTests
{
    private static uint[] Fragment(OperationContext context)
    {
        context.Reserve(24);
        return [10, 20, 30, 30, 10, 40];
    }

    // Actual palette permutations from the frozen E93 native ALPH6 control;
    // the encoder's complete85796B packet was checked before observation.
    [Theory]
    [InlineData(0, 256, "05f4782bc162c0d9daa77397bc75630a85ebf02f72c88e323fd12cd89ac44660")]
    [InlineData(1, 109, "0dd2c987421b4f05c1948698ce4a97142b5f9019c4bf3d6383e6655dc763f286")]
    public void ExistingAlphaFiltersMatchNativePermutationAndRecoverEveryPackedSample(int filter, int count, string expected)
    {
        using var root = new TestMemoryBudget(10_000_000);
        using var context = new OperationContext(new() { MemoryBudget = root }, default);
        var pixels = Vp8lSearchCandidateTests.AlphaWorking(filter, context); var original = pixels.ToArray();
        using var plan = Vp8lReferencePalette.SelectAndPack(pixels, 513, count, Vp8lPaletteOrder.ModifiedZeng, context);
        Assert.Equal(expected, Convert.ToHexStringLower(SHA256.HashData(MemoryMarshal.AsBytes(plan.Colors))));
        Assert.Equal(original.Length, plan.PackedPixelCount); Assert.Equal(0, plan.PackingBits);
        for (var i = 0; i < original.Length; i++) Assert.Equal(original[i], plan.Colors[(int)(pixels[i] >> 8 & 255)]);
        Assert.Equal(pixels.Length * 4L + count * 8L, root.Bytes);
        Assert.Equal(root.Bytes + 3200 + count * count * 4L + 513 * 4L + count * 16L, root.Peak);
        Array.Clear(pixels); context.Release(pixels.Length * 4L); Assert.Equal(count * 8L, root.Bytes);
        Assert.Equal(expected, Convert.ToHexStringLower(SHA256.HashData(MemoryMarshal.AsBytes(plan.Colors))));
        plan.Dispose(); plan.Dispose(); Assert.Equal(0, root.Bytes); Assert.False(root.IsDisposed);
    }

    [Fact]
    public void FourNeighborTiesAndEndPlacementHaveAnIndependentSmallExample()
    {
        using var root = new TestMemoryBudget(10_000); using var context = new OperationContext(new() { MemoryBudget = root }, default);
        var pixels = Fragment(context);
        using var plan = Vp8lReferencePalette.SelectAndPack(pixels, 3, 4, Vp8lPaletteOrder.ModifiedZeng, context);
        // Color10 has five differing neighbors. Its strongest ties are20/30
        // at two each, so20 wins. Then30 and40 both prepend by weighted distance.
        Assert.Equal(new uint[] { 40, 30, 10, 20 }, plan.Colors.ToArray());
        // Four colors pack four2-bit indices per byte, one byte per row here.
        Assert.Equal(2, plan.PackedPixelCount);
        Assert.Equal(new uint[] { 0xff001e00, 0xff000900 }, pixels[..2]);
        plan.Dispose(); context.Release(24); Assert.Equal(0, root.Bytes);
    }

    [Fact]
    public void SingletonNeedsNoAdjacencyStorage()
    {
        using var root = new TestMemoryBudget(10_000); using var context = new OperationContext(new() { MemoryBudget = root }, default);
        context.Reserve(4); uint[] pixels = [123]; var growths = root.GrowthAttempts;
        using var plan = Vp8lReferencePalette.SelectAndPack(pixels, 1, 1, Vp8lPaletteOrder.ModifiedZeng, context);
        Assert.Equal(new uint[] { 123 }, plan.Colors.ToArray()); Assert.Equal(growths + 2, root.GrowthAttempts);
        Assert.Equal(4 + 8 + 3200, root.Peak); plan.Dispose(); context.Release(4); Assert.Equal(0, root.Bytes);
    }

    [Fact]
    public void AdjacencyAdmissionRefusalIsTerminalAndReleasesPaletteLookupAndScratch()
    {
        using var root = new TestMemoryBudget(10_000); using var caller = root.CreateScope(); Assert.True(caller.TryReserve(1, out var held));
        using var context = new OperationContext(new() { MemoryBudget = root }, default); var pixels = Fragment(context); var original = pixels.ToArray();
        root.RefuseAtGrowth = root.GrowthAttempts + 3;
        var error = Assert.Throws<ImageLimitException>(() => Vp8lReferencePalette.SelectAndPack(pixels, 3, 4, Vp8lPaletteOrder.ModifiedZeng, context));
        Assert.Equal((ImageLimitKind.WorkingBytes, ImageLimitScope.SharedBudget), (error.Limit, error.Scope));
        Assert.Equal(root.RefuseAtGrowth, root.GrowthAttempts); Assert.Equal(1, root.Refusals); Assert.Equal(25, root.Bytes); Assert.Equal(original, pixels);
        root.RefuseAtGrowth = 0; held.Dispose();
        using var next = Vp8lReferencePalette.SelectAndPack(pixels, 3, 4, Vp8lPaletteOrder.ModifiedZeng, context);
        next.Dispose(); context.Release(24); Assert.Equal(0, root.Bytes); Assert.False(root.IsDisposed);
    }

    [Fact]
    public void CancellationAtAdjacencyAdmissionReleasesStorageBeforeOperationDisposalAndAllowsRecovery()
    {
        using var root = new TestMemoryBudget(10_000); using var cancellation = new CancellationTokenSource();
        using var context = new OperationContext(new() { MemoryBudget = root }, cancellation.Token); var pixels = Fragment(context); var original = pixels.ToArray();
        var cancelAt = root.GrowthAttempts + 3; root.Admitted = _ => { if (root.GrowthAttempts == cancelAt) cancellation.Cancel(); };
        Assert.Throws<OperationCanceledException>(() => Vp8lReferencePalette.SelectAndPack(pixels, 3, 4, Vp8lPaletteOrder.ModifiedZeng, context));
        Assert.Equal(24, root.Bytes); Assert.Equal(original, pixels); Assert.Equal(0, root.Refusals); root.Admitted = null;
        using var nextContext = new OperationContext(new() { MemoryBudget = root }, default);
        using var next = Vp8lReferencePalette.SelectAndPack(pixels, 3, 4, Vp8lPaletteOrder.ModifiedZeng, nextContext);
        next.Dispose(); context.Release(24); Assert.Equal(0, root.Bytes); Assert.False(root.IsDisposed);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void IndependentLocalAndWorkLimitsCleanUpWithoutPacking(bool local)
    {
        using var root = new TestMemoryBudget(10_000);
        using var context = new OperationContext(new() { MemoryBudget = root, MaxWorkingBytes = local ? 24 + 32 + 3200 + 140 - 1 : 10_000, MaxWork = local ? 100_000 : 100 }, default);
        var pixels = Fragment(context); var original = pixels.ToArray(); var growths = root.GrowthAttempts;
        var error = Assert.Throws<ImageLimitException>(() => Vp8lReferencePalette.SelectAndPack(pixels, 3, 4, Vp8lPaletteOrder.ModifiedZeng, context));
        Assert.Equal((local ? ImageLimitKind.WorkingBytes : ImageLimitKind.Work, ImageLimitScope.Operation), (error.Limit, error.Scope));
        Assert.Equal(growths + (local ? 2 : 3), root.GrowthAttempts); Assert.Equal(24, root.Bytes); Assert.Equal(0, root.Refusals); Assert.Equal(original, pixels);
        using var nextContext = new OperationContext(new() { MemoryBudget = root }, default);
        using var next = Vp8lReferencePalette.SelectAndPack(pixels, 3, 4, Vp8lPaletteOrder.ModifiedZeng, nextContext);
        next.Dispose(); context.Release(24); Assert.Equal(0, root.Bytes); Assert.False(root.IsDisposed);
    }
}
