using System.Buffers.Binary;
using System.Security.Cryptography;
using Lokad.Image.Internal;
using Lokad.Image.Tooling;
using static Lokad.Image.Tests.Vp8lReferencePredictorTests;

namespace Lokad.Image.Tests;

public sealed class Vp8lReferenceColorTransformTests
{
    private static string Hash(ReadOnlySpan<uint> values)
    {
        var bytes = new byte[values.Length * 4];
        for (var i = 0; i < values.Length; i++) BinaryPrimitives.WriteUInt32LittleEndian(bytes.AsSpan(i * 4), values[i]);
        return Convert.ToHexStringLower(SHA256.HashData(bytes));
    }

    // Reuse the packed carriers of the original 4103x2 two-color control.
    // Their zero red/blue channels give an identity map for ownership checks;
    // this is not a new public-encoder reference case.
    private static uint[] Packed(OperationContext context)
    {
        context.Reserve(1026 * 4);
        var pixels = new uint[1026]; Array.Fill(pixels, 0xff000000u); pixels[^1] = 0xff004000u; return pixels;
    }

    [Theory]
    [InlineData(3, "870a05bb67a28441242246a7235844eafae6236571ee264c25f5a84d535420eb", "b72edfa146d02b4adbbe48a667ad28bf10b6073b2800c7679ccfc546c9969610")]
    [InlineData(6, "a6548d3617c0dc7cd79c59d6fdb3dca6b9206a812d77204f26d6a01292db9a5e", "34b642c843a4c532bb600a6813f5182537bdf369055956fd03dae1745ef20e4d")]
    public void DefaultAndCoarseTransformsMatchNativeMapsAndResiduals(int bits, string mapHash, string residualHash)
    {
        var rgba = Asymmetric();
        using var root = new TestMemoryBudget(8_000_000);
        using var context = new OperationContext(new() { MemoryBudget = root }, default);
        var pixels = Working(rgba, true, context);
        using var prediction = Vp8lReferencePredictor.SelectAndApply(pixels, 513, bits, bits, false, context);
        using var color = Vp8lReferenceColorTransform.SelectAndApply(pixels, 513, bits, 75, context);
        Assert.Equal(bits, color.StoredBits); Assert.Equal(mapHash, Hash(color.Coefficients));
        Assert.Equal(residualHash, Hash(pixels));
        Assert.Equal(rgba.Length + prediction.Modes.Length + color.CapacityBytes, root.Bytes);
        prediction.Dispose(); Assert.Equal(rgba.Length + color.CapacityBytes, root.Bytes);
        Assert.Equal(mapHash, Hash(color.Coefficients));
        color.Dispose(); color.Dispose(); Assert.Equal(rgba.Length, root.Bytes);
        Assert.Throws<ObjectDisposedException>(() => color.Coefficients.Length); Assert.False(root.IsDisposed);
    }

    [Fact]
    public void AdaptivePhotographMatchesTheFrozenNativePacketStages()
    {
        using var fixtureRoot = new TestMemoryBudget(8_000_000);
        using var image = OriginalVp8lControls.Buffer("photograph", 8, fixtureRoot);
        var rgba = OriginalPixels.Bytes(image);
        using var root = new TestMemoryBudget(8_000_000);
        using var context = new OperationContext(new() { MemoryBudget = root }, default);
        var pixels = Working(rgba, true, context);
        using var prediction = Vp8lReferencePredictor.SelectAndApply(pixels, image.Width, 2, 3, false, context);
        using var color = Vp8lReferenceColorTransform.SelectAndApply(pixels, image.Width, 3, 75, context);
        Assert.Equal(2, prediction.StoredBits); Assert.Equal(3, color.StoredBits);
        // Original RFC reader observes these directly from the already frozen
        // libwebp1.6.0 method5 packet, SHA28932bb7...; no new native encode.
        Assert.Equal("b0bf344a13b5963d6719cb0a5846d7832937313e17a599ab89c31b0571112c3f", Hash(color.Coefficients));
        Assert.Equal("66ac0ed193f5d4f6bce194ca994d83797c5ee3943b7a19f7bbaa594ef950864f", Hash(pixels));
        Assert.Equal(rgba.Length + prediction.Modes.Length + color.CapacityBytes, root.Bytes);
    }

    [Fact]
    public void SpatialOnlyHiddenRgbKeepsTheNativeIdentityColorTransform()
    {
        var rgba = HiddenRgb();
        using var root = new TestMemoryBudget(100_000);
        using var context = new OperationContext(new() { MemoryBudget = root }, default);
        var pixels = Working(rgba, false, context);
        using var prediction = Vp8lReferencePredictor.SelectAndApply(pixels, 19, 5, 5, false, context);
        using var color = Vp8lReferenceColorTransform.SelectAndApply(pixels, 19, 5, 75, context);
        Assert.Equal(9, color.StoredBits); Assert.Equal(0xff000000u, Assert.Single(color.Coefficients.ToArray()));
        Assert.Equal("7416018eeb4d196de281306b2952d9cae631dd8148e419555a311e44d6bcd106", Hash(pixels));
        Assert.Equal(rgba.Length + prediction.Modes.Length + 4, root.Bytes);
    }

    [Fact]
    public void InPlaceMapCompactionKeepsActualCapacityCharged()
    {
        using var root = new TestMemoryBudget(100_000);
        using var context = new OperationContext(new() { MemoryBudget = root }, default);
        var pixels = Packed(context); var original = pixels.ToArray();
        using var color = Vp8lReferenceColorTransform.SelectAndApply(pixels, 513, 3, 75, context);
        Assert.Equal(9, color.StoredBits); Assert.Equal(2, color.Coefficients.Length);
        Assert.All(color.Coefficients.ToArray(), value => Assert.Equal(0xff000000u, value));
        Assert.Equal(65 * 4, color.CapacityBytes); Assert.Equal(4104 + 260, root.Bytes);
        Assert.Equal(4104 + 260 + 7296, root.Peak); Assert.Equal(original, pixels);
        color.Dispose(); Assert.Equal(4104, root.Bytes); Assert.False(root.IsDisposed);
    }

    [Fact]
    public void OverlappingResultCapacityRefusesOnceThenSucceedsAfterPriorOwnerEnds()
    {
        using var root = new TestMemoryBudget(4104 + 260 + 7296);
        using var context = new OperationContext(new() { MemoryBudget = root }, default);
        var pixels = Packed(context);
        using var first = Vp8lReferenceColorTransform.SelectAndApply(pixels, 513, 3, 75, context);
        var error = Assert.Throws<ImageLimitException>(() => Vp8lReferenceColorTransform.SelectAndApply(pixels, 513, 3, 75, context));
        Assert.Equal((ImageLimitKind.WorkingBytes, ImageLimitScope.SharedBudget), (error.Limit, error.Scope));
        Assert.Equal(1, root.Refusals); Assert.Equal(4104 + 260, root.Bytes);
        Assert.All(first.Coefficients.ToArray(), value => Assert.Equal(0xff000000u, value));
        first.Dispose(); Assert.Equal(4104, root.Bytes);
        using var next = Vp8lReferenceColorTransform.SelectAndApply(pixels, 513, 3, 75, context);
        next.Dispose(); Assert.Equal(4104, root.Bytes); Assert.False(root.IsDisposed);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    public void MapAndScratchRefusalAreTerminalAndCleanUpBeforeWorkingPixelsChange(int phase)
    {
        using var root = new TestMemoryBudget(100_000);
        using var context = new OperationContext(new() { MemoryBudget = root }, default);
        var pixels = Packed(context); var original = pixels.ToArray();
        root.RefuseAtGrowth = root.GrowthAttempts + phase;
        var error = Assert.Throws<ImageLimitException>(() => Vp8lReferenceColorTransform.SelectAndApply(pixels, 513, 3, 75, context));
        Assert.Equal((ImageLimitKind.WorkingBytes, ImageLimitScope.SharedBudget), (error.Limit, error.Scope));
        Assert.Equal(root.RefuseAtGrowth, root.GrowthAttempts); Assert.Equal(1, root.Refusals);
        Assert.Equal(4104, root.Bytes); Assert.Equal(original, pixels);
        root.RefuseAtGrowth = 0;
        using var next = Vp8lReferenceColorTransform.SelectAndApply(pixels, 513, 3, 75, context);
        next.Dispose(); Assert.Equal(4104, root.Bytes); Assert.False(root.IsDisposed);
    }

    [Fact]
    public void CancellationDuringScratchAdmissionRefundsAllUnusedStorageAndAllowsRecovery()
    {
        using var root = new TestMemoryBudget(100_000); using var cancellation = new CancellationTokenSource();
        using var context = new OperationContext(new() { MemoryBudget = root }, cancellation.Token);
        var pixels = Packed(context); var original = pixels.ToArray();
        var cancelAt = root.GrowthAttempts + 2;
        root.Admitted = _ => { if (root.GrowthAttempts == cancelAt) cancellation.Cancel(); };
        Assert.Throws<OperationCanceledException>(() => Vp8lReferenceColorTransform.SelectAndApply(pixels, 513, 3, 75, context));
        Assert.True(cancellation.IsCancellationRequested); Assert.Equal(original, pixels);
        Assert.Equal(4104, root.Bytes); Assert.Equal(0, root.Refusals); root.Admitted = null;
        using var nextContext = new OperationContext(new() { MemoryBudget = root }, default);
        using var next = Vp8lReferenceColorTransform.SelectAndApply(pixels, 513, 3, 75, nextContext);
        next.Dispose(); Assert.Equal(4104, root.Bytes);
    }
}
