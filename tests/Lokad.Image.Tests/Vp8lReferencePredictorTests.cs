using System.Buffers.Binary;
using System.Security.Cryptography;
using Lokad.Image.Internal;
using Lokad.Image.Tooling;

namespace Lokad.Image.Tests;

public sealed class Vp8lReferencePredictorTests
{
    // Original inputs and libwebp1.6.0 stage hashes already qualified by the
    // reference-model checks. No reference encoder is invoked by these tests.
    private static string Hash(ReadOnlySpan<byte> bytes) => Convert.ToHexStringLower(SHA256.HashData(bytes));
    private static string Hash(ReadOnlySpan<uint> values)
    {
        var bytes = new byte[values.Length * 4];
        for (var i = 0; i < values.Length; i++) BinaryPrimitives.WriteUInt32LittleEndian(bytes.AsSpan(i * 4), values[i]);
        return Hash(bytes);
    }

    internal static uint[] Working(ReadOnlySpan<byte> rgba, bool subtractGreen, OperationContext context)
    {
        context.Reserve(rgba.Length);
        var pixels = new uint[rgba.Length / 4];
        for (var at = 0; at < pixels.Length; at++)
        {
            pixels[at] = (uint)rgba[at * 4 + 3] << 24 | (uint)rgba[at * 4] << 16
                | (uint)rgba[at * 4 + 1] << 8 | rgba[at * 4 + 2];
        }
        if (subtractGreen) Vp8lReferenceColorTransform.SubtractGreen(pixels, context);
        return pixels;
    }

    internal static byte[] HiddenRgb()
    {
        var rgba = new byte[19 * 7 * 4];
        for (var y = 0; y < 7; y++)
        for (var x = 0; x < 19; x++)
        {
            var at = (y * 19 + x) * 4;
            rgba[at] = (byte)(x * 13); rgba[at + 1] = (byte)(y * 31);
            rgba[at + 2] = (byte)(x + y); rgba[at + 3] = (byte)(x % 3 == 0 ? 0 : x * 11);
        }
        return rgba;
    }

    internal static byte[] Asymmetric()
    {
        var rgba = OriginalPixels.Generate(513, 259, 8);
        // The existing warm reference fixes both first-row endpoint alphas at zero.
        rgba[3] = rgba[513 * 4 - 1] = 0;
        return rgba;
    }

    [Theory]
    [InlineData(3)]
    [InlineData(2)]
    public void FixedAndAdaptiveCoarseWinnerMatchNativeMapsAndResiduals(int minBits)
    {
        var rgba = Asymmetric();
        Assert.Equal("70f0fb18c341e2740b875a5a34e21ca63f0821b3386352fe2971020c5a6aac80", Hash(rgba));
        using var root = new TestMemoryBudget(8_000_000);
        using var context = new OperationContext(new() { MemoryBudget = root }, default);
        var pixels = Working(rgba, true, context);
        using var plan = Vp8lReferencePredictor.SelectAndApply(pixels, 513, minBits, 3, false, context);
        Assert.Equal((3, 3, 17595015971444L), (plan.SelectedBits, plan.StoredBits, plan.EntropyCost));
        Assert.Equal("bdad28ced56613fe6dc21387f5f10cac85e43ec37c785d77ac80694426c763b7", Hash(plan.Modes));
        Assert.Equal("9a29c48cef0fd5f0de20121fa07b1807c41366ef3e3c9d7cf9937dc4b8558f0a", Hash(pixels));
        Assert.Equal(rgba.Length + plan.Modes.Length, root.Bytes);
        // Retained result allocation overlaps candidate capacity during selection.
        Assert.True(root.Peak > root.Bytes + 60_000);
        plan.Dispose(); plan.Dispose(); Assert.Equal(rgba.Length, root.Bytes);
        Assert.Throws<ObjectDisposedException>(() => plan.Modes.Length); Assert.False(root.IsDisposed);
    }

    [Fact]
    public void AdaptiveFineWinnerMatchesTheExistingPhotograph()
    {
        using var fixtureRoot = new TestMemoryBudget(8_000_000);
        using var image = OriginalVp8lControls.Buffer("photograph", 8, fixtureRoot);
        var rgba = OriginalPixels.Bytes(image);
        Assert.Equal("2b970f3ddee2a651ca381aab10b90db78a2ce08db8c7f70154bec92c1ec33090", Hash(rgba));
        using var root = new TestMemoryBudget(8_000_000);
        using var context = new OperationContext(new() { MemoryBudget = root }, default);
        var pixels = Working(rgba, true, context);
        using var plan = Vp8lReferencePredictor.SelectAndApply(pixels, image.Width, 2, 3, false, context);
        Assert.Equal((2, 2, 12332150353687L), (plan.SelectedBits, plan.StoredBits, plan.EntropyCost));
        Assert.Equal("e2a18ae090c73a2ad05ff670af53270d7a84b73aaef0cc5846049dcc2b93285c", Hash(plan.Modes));
        Assert.Equal("0b7c34310fbd095f64c16999430afff0a28f229e4c23e48a95cab825c6fcee6f", Hash(pixels));
        Assert.Equal(rgba.Length + plan.Modes.Length, root.Bytes);
    }

    [Fact]
    public void LowEffortKeepsConfiguredSamplingAndOnlyAllocatesModesAndRows()
    {
        var rgba = Asymmetric();
        using var root = new TestMemoryBudget(rgba.Length + 45 + 513 * 8);
        using var context = new OperationContext(new() { MemoryBudget = root }, default);
        var pixels = Working(rgba, true, context);
        using var plan = Vp8lReferencePredictor.SelectAndApply(pixels, 513, 6, 6, true, context);
        Assert.Equal((6, 6), (plan.SelectedBits, plan.StoredBits)); Assert.Equal(45, plan.Modes.Length);
        Assert.All(plan.Modes.ToArray(), value => Assert.Equal(11, value));
        Assert.Equal("34b59d80c7d203f3dfbb93c3ddc25c92b9dfa18037c9dbe81ea9a499235f9dcb", Hash(pixels));
        Assert.Equal(rgba.Length + 45 + 513 * 8, root.Peak); Assert.Equal(rgba.Length + 45, root.Bytes);
    }

    [Fact]
    public void UniformMapCompactsAndPreservesHiddenRgbWithoutSubtractGreen()
    {
        var rgba = HiddenRgb();
        Assert.Equal("c4c881cd7125a4f91eb16d548728801b3cfacf1c1f8ede5933f514260eb8eb96", Hash(rgba));
        using var root = new TestMemoryBudget(100_000);
        using var context = new OperationContext(new() { MemoryBudget = root }, default);
        var pixels = Working(rgba, false, context);
        using var plan = Vp8lReferencePredictor.SelectAndApply(pixels, 19, 5, 5, false, context);
        Assert.Equal((5, 9), (plan.SelectedBits, plan.StoredBits)); Assert.Equal(1, plan.Modes.Length);
        Assert.Equal("7416018eeb4d196de281306b2952d9cae631dd8148e419555a311e44d6bcd106", Hash(pixels));
        Assert.Equal(rgba.Length + 1, root.Bytes);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    public void RefusalBeforeAnalysisOrResultAllocationIsTerminalAndLeavesWorkingPixelsUnchanged(int phase)
    {
        var rgba = HiddenRgb();
        using var root = new TestMemoryBudget(100_000);
        using var context = new OperationContext(new() { MemoryBudget = root }, default);
        var pixels = Working(rgba, false, context); var original = pixels.ToArray();
        root.RefuseAtGrowth = root.GrowthAttempts + phase;
        var error = Assert.Throws<ImageLimitException>(() => Vp8lReferencePredictor.SelectAndApply(pixels, 19, 5, 5, false, context));
        Assert.Equal((ImageLimitKind.WorkingBytes, ImageLimitScope.SharedBudget), (error.Limit, error.Scope));
        Assert.Equal(1, root.Refusals); Assert.Equal(rgba.Length, root.Bytes); Assert.Equal(original, pixels);
        Assert.Equal(root.RefuseAtGrowth, root.GrowthAttempts);
        root.RefuseAtGrowth = 0;
        using var next = Vp8lReferencePredictor.SelectAndApply(pixels, 19, 5, 5, false, context);
        Assert.Equal("7416018eeb4d196de281306b2952d9cae631dd8148e419555a311e44d6bcd106", Hash(pixels));
        next.Dispose(); Assert.Equal(rgba.Length, root.Bytes); Assert.False(root.IsDisposed);
    }

    [Fact]
    public void CancellationAtRowAdmissionReleasesAnalysisFirstAndRefundsRowsAndModes()
    {
        var rgba = HiddenRgb();
        using var root = new TestMemoryBudget(100_000); using var cancellation = new CancellationTokenSource();
        using var context = new OperationContext(new() { MemoryBudget = root }, cancellation.Token);
        var pixels = Working(rgba, false, context); var original = pixels.ToArray();
        var rowGrowth = root.GrowthAttempts + 3;
        root.Admitted = bytes =>
        {
            if (root.GrowthAttempts != rowGrowth) return;
            Assert.Equal(rgba.Length + 1 + 19 * 8, bytes); cancellation.Cancel();
        };
        Assert.Throws<OperationCanceledException>(() => Vp8lReferencePredictor.SelectAndApply(pixels, 19, 5, 5, false, context));
        Assert.True(cancellation.IsCancellationRequested); Assert.Equal(rgba.Length, root.Bytes);
        Assert.Equal(original, pixels); Assert.Equal(0, root.Refusals); root.Admitted = null;
        using var nextContext = new OperationContext(new() { MemoryBudget = root }, default);
        using var next = Vp8lReferencePredictor.SelectAndApply(pixels, 19, 5, 5, false, nextContext);
        next.Dispose(); Assert.Equal(rgba.Length, root.Bytes);
    }
}
