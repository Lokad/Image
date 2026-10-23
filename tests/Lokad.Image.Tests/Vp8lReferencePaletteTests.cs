using System.Buffers.Binary;
using System.Security.Cryptography;
using Lokad.Image.Internal;
using Lokad.Image.Tooling;
using static Lokad.Image.Tests.Vp8lReferencePredictorTests;

namespace Lokad.Image.Tests;

public sealed class Vp8lReferencePaletteTests
{
    private static string Hash(ReadOnlySpan<uint> values)
    {
        var bytes = new byte[values.Length * 4];
        for (var i = 0; i < values.Length; i++) BinaryPrimitives.WriteUInt32LittleEndian(bytes.AsSpan(i * 4), values[i]);
        return Convert.ToHexStringLower(SHA256.HashData(bytes));
    }

    private static byte[] Source(int width, int height, int colors)
    {
        if (colors == 1) return [255, 0, 1, 0];
        if (colors != 2) return PalettePackingInput.Create(width, height, colors);
        var bytes = new byte[width * height * 4];
        for (var at = 0; at < bytes.Length; at += 4) { bytes[at] = 48; bytes[at + 1] = 211; bytes[at + 2] = 255; }
        bytes[^4] = 77; return bytes;
    }

    // Native palette/index data from the already frozen libwebp1.6.0 packets;
    // the original RFC reader and prior native snapshots supply these hashes.
    [Theory]
    [InlineData(1, 1, 1, true, 3, 1, 1, "108926562b2303b9fffdfaa34f5aeeaf701a785231d1de42a2c9d5948fc6bc3e", "108926562b2303b9fffdfaa34f5aeeaf701a785231d1de42a2c9d5948fc6bc3e", "e3820096cb82366b860b8a4e668453a7aaaf423af03bdf289fa308ea03a79332")]
    [InlineData(4103, 2, 2, true, 3, 2, 1026, "7b701a440e7dfa774aafc90be2a3009ce536082e1b01110ae05df2f2d8d3497d", "2fa98359e0ba2666f98a90f35e3f04081eeed456e0d4fd9bd5c6af67110184f2", "b575d8901a7aa4aabfb48c8505224f1fa438c8a2d990eb74fd3b1c5c1b1f13a2")]
    [InlineData(7, 3, 4, true, 2, 4, 6, "b8c38a9f98ffbf8553567c4a9c04f8847e27045940dfd71cd400cedf412f1f57", "577ff48fb4a72e543f3fedffce84d7be2487cd9cf05315b9c2b5d4700fc4c831", "395f91ddd7ca52eb3edd3ecd46d06949d28532a0a01a3c47aa27eaac52dfc3ee")]
    [InlineData(17, 3, 16, true, 1, 16, 27, "c26429cae0d1eb320040e99f1cc34132758bc557a39d7dd9daefdc56ceadacca", "230a87bd46a5003c82258e4df6635133dd771def621da99975c9db5ae438ec97", "f5225a458c0c84f13964c57cd2cb75eb7d7070ba84855a65c716b0c0cb53a0b8")]
    [InlineData(23, 13, 18, true, 0, 17, 299, "c6f5b9984587c8e5e150540ceaa414cd64996fd2ae866e03e1491937e0625931", "535b0b264eb93b40c27028cd96b942142bb4316d9f0fcb1c861cdfc5f7ffea33", "8c26bbbf5d7fbe12536daa24055c27a35fa4790703c0ba458fe8b888d3c4e398")]
    [InlineData(23, 13, 18, false, 0, 17, 299, "343d570a724262fbaad16d0c55efc0ab33490d3a722e5cee81e5e8c5dc1ae11c", "04ceb85f29dd4748cafd283dd2401f2aec44d15626fa2e21cb264cb3b492090c", "7a5791e5fae50aad4830fd5d94aa24529d4f55d213f074682ccafa076e541929")]
    public void ExistingControlsMatchNativePaletteDeltasAndPackedPixels(int width, int height, int count, bool greedy,
        int bits, int encodedCount, int packedCount, string paletteHash, string deltaHash, string packedHash)
    {
        var rgba = Source(width, height, count);
        using var root = new TestMemoryBudget(1_000_000);
        using var context = new OperationContext(new() { MemoryBudget = root }, default);
        var pixels = Working(rgba, false, context);
        using var plan = Vp8lReferencePalette.SelectAndPack(pixels, width, count, greedy, context);
        Assert.Equal((count, encodedCount, bits, packedCount), (plan.ColorCount, plan.EncodedColorCount, plan.PackingBits, plan.PackedPixelCount));
        Assert.Equal(paletteHash, Hash(plan.Colors)); Assert.Equal(deltaHash, Hash(plan.EncodedDeltas));
        Assert.Equal(packedHash, Hash(pixels.AsSpan(0, packedCount)));
        Assert.Equal(count * 8, plan.CapacityBytes);
        // Logical image/palette shortening does not refund still-live capacity.
        Assert.Equal(rgba.Length + count * 8, root.Bytes);
        Assert.Equal(rgba.Length + count * 8 + 3200, root.Peak);
        plan.Dispose(); plan.Dispose(); Assert.Equal(rgba.Length, root.Bytes);
        Assert.Equal(packedHash, Hash(pixels.AsSpan(0, packedCount)));
        Assert.Throws<ObjectDisposedException>(() => plan.Colors.Length); Assert.False(root.IsDisposed);
    }

    [Fact]
    public void RetainedPaletteAndPackedPixelsHaveIndependentLifetimes()
    {
        var rgba = Source(7, 3, 4);
        using var root = new TestMemoryBudget(10_000);
        using var context = new OperationContext(new() { MemoryBudget = root }, default);
        var firstPixels = Working(rgba, false, context);
        using var first = Vp8lReferencePalette.SelectAndPack(firstPixels, 7, 4, true, context);
        var paletteHash = Hash(first.Colors); var packedHash = Hash(firstPixels.AsSpan(0, first.PackedPixelCount));
        var secondPixels = Working(rgba, false, context);
        using var second = Vp8lReferencePalette.SelectAndPack(secondPixels, 7, 4, true, context);
        Assert.Equal(rgba.Length * 2 + 64, root.Bytes);
        second.WritableColors[0] ^= 1; secondPixels[0] ^= 1; rgba.AsSpan().Clear();
        Assert.Equal(paletteHash, Hash(first.Colors)); Assert.Equal(packedHash, Hash(firstPixels.AsSpan(0, first.PackedPixelCount)));
        second.Dispose(); Assert.Equal(84 * 2 + 32, root.Bytes);
        first.Dispose(); Assert.Equal(84 * 2, root.Bytes); Assert.False(root.IsDisposed);
    }

    [Fact]
    public void OverlappingPaletteCapacityRefusesOnceAndRecoversAfterPriorOwnerEnds()
    {
        var rgba = Source(7, 3, 4);
        using var root = new TestMemoryBudget(3400);
        using var context = new OperationContext(new() { MemoryBudget = root }, default);
        var firstPixels = Working(rgba, false, context);
        using var first = Vp8lReferencePalette.SelectAndPack(firstPixels, 7, 4, true, context);
        var secondPixels = Working(rgba, false, context); var original = secondPixels.ToArray();
        var error = Assert.Throws<ImageLimitException>(() => Vp8lReferencePalette.SelectAndPack(secondPixels, 7, 4, true, context));
        Assert.Equal((ImageLimitKind.WorkingBytes, ImageLimitScope.SharedBudget), (error.Limit, error.Scope));
        Assert.Equal(1, root.Refusals); Assert.Equal(84 * 2 + 32, root.Bytes); Assert.Equal(original, secondPixels);
        first.Dispose(); Assert.Equal(84 * 2, root.Bytes);
        using var next = Vp8lReferencePalette.SelectAndPack(secondPixels, 7, 4, true, context);
        Assert.Equal(3400, root.Peak); next.Dispose(); Assert.Equal(84 * 2, root.Bytes);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    public void PaletteAndLookupRefusalsAreTerminalBeforePackingStarts(int phase)
    {
        var rgba = Source(7, 3, 4);
        using var root = new TestMemoryBudget(10_000);
        using var context = new OperationContext(new() { MemoryBudget = root }, default);
        var pixels = Working(rgba, false, context); var original = pixels.ToArray();
        root.RefuseAtGrowth = root.GrowthAttempts + phase;
        var error = Assert.Throws<ImageLimitException>(() => Vp8lReferencePalette.SelectAndPack(pixels, 7, 4, true, context));
        Assert.Equal((ImageLimitKind.WorkingBytes, ImageLimitScope.SharedBudget), (error.Limit, error.Scope));
        Assert.Equal(root.RefuseAtGrowth, root.GrowthAttempts); Assert.Equal(1, root.Refusals);
        Assert.Equal(rgba.Length, root.Bytes); Assert.Equal(original, pixels);
        root.RefuseAtGrowth = 0;
        using var next = Vp8lReferencePalette.SelectAndPack(pixels, 7, 4, true, context);
        next.Dispose(); Assert.Equal(rgba.Length, root.Bytes);
    }

    [Fact]
    public void CancellationAfterLookupAdmissionRefundsPaletteAndScratch()
    {
        var rgba = Source(7, 3, 4);
        using var root = new TestMemoryBudget(10_000); using var cancellation = new CancellationTokenSource();
        using var context = new OperationContext(new() { MemoryBudget = root }, cancellation.Token);
        var pixels = Working(rgba, false, context); var original = pixels.ToArray();
        var cancelAt = root.GrowthAttempts + 2;
        root.Admitted = _ => { if (root.GrowthAttempts == cancelAt) cancellation.Cancel(); };
        Assert.Throws<OperationCanceledException>(() => Vp8lReferencePalette.SelectAndPack(pixels, 7, 4, true, context));
        Assert.True(cancellation.IsCancellationRequested); Assert.Equal(original, pixels); Assert.Equal(rgba.Length, root.Bytes);
        root.Admitted = null;
        using var nextContext = new OperationContext(new() { MemoryBudget = root }, default);
        using var next = Vp8lReferencePalette.SelectAndPack(pixels, 7, 4, true, nextContext);
        next.Dispose(); Assert.Equal(rgba.Length, root.Bytes); Assert.Equal(0, root.Refusals);
    }

    [Fact]
    public void EarlyAndLateWorkFailuresReleaseScratchAndAnyUnreturnedPalette()
    {
        var rgba = Source(7, 3, 4); long completeWork;
        using (var calibration = new OperationContext(ImageLimits.Default, default))
        {
            var working = Working(rgba, false, calibration);
            using var plan = Vp8lReferencePalette.SelectAndPack(working, 7, 4, true, calibration);
            completeWork = calibration.ConsumedWork;
        }
        foreach (var budget in new[] { 2L, completeWork - 1 })
        {
            using var root = new TestMemoryBudget(10_000);
            using var context = new OperationContext(new() { MemoryBudget = root, MaxWork = budget }, default);
            var pixels = Working(rgba, false, context);
            var error = Assert.Throws<ImageLimitException>(() => Vp8lReferencePalette.SelectAndPack(pixels, 7, 4, true, context));
            Assert.Equal((ImageLimitKind.Work, ImageLimitScope.Operation), (error.Limit, error.Scope));
            Assert.Equal(rgba.Length, root.Bytes); Assert.Equal(0, root.Refusals);
            if (budget != 2) Assert.NotEqual(0u, pixels[0]); // Packing had already begun in owned working storage.
        }
    }
}
