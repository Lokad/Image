using System.Buffers.Binary;
using System.Security.Cryptography;
using System.Text;
using Lokad.Image.Internal;
using Lokad.Image.Tooling;

namespace Lokad.Image.Tests;

public sealed class Vp8lReferenceMatcherTests
{
    [Fact]
    public void FrozenIndicesKeepChainOrderAcrossStorageBoundaries()
    {
        // Frozen against the qualified separate-chain implementation before
        // reusing result capacity. These are primitive vectors, not new rasters.
        (string Name, int Count, int Width, int Quality, bool Low, int Pattern)[] cases = [
            ("singleton", 1, 1, 75, false, 0), ("pair", 2, 2, 75, false, 1),
            ("triple", 3, 3, 75, false, 2), ("short", 17, 17, 75, false, 1),
            ("two-rows", 1026, 513, 75, false, 2), ("maximum-run", 4103, 4103, 75, false, 0),
            ("sparse-head-limit", 16383, 16383, 75, false, 2), ("dense-head-limit", 16384, 16384, 75, false, 2),
            ("dense-above", 16385, 5, 75, false, 2), ("wide-two-rows", 16384, 8192, 75, false, 1),
            ("low-effort", 771, 257, 75, true, 2), ("quality100", 387, 129, 100, false, 2),
            ("quality0", 51, 17, 0, false, 2)
        ];
        using var digest = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
        foreach (var item in cases)
        {
            uint state = 0x6d2b79f5; var source = new uint[item.Count];
            for (var i = 0; i < source.Length; i++)
            {
                state ^= state << 13; state ^= state >> 17; state ^= state << 5;
                source[i] = item.Pattern switch
                {
                    0 => 0xff204060u,
                    1 => unchecked(0xff000000u + (uint)(i * 257)),
                    _ => 0xff000000u + (state & 3) * 0x010101u
                };
            }
            source[^1] ^= 0x100;
            var original = source.ToArray();
            using var context = new OperationContext(ImageLimits.Default, default);
            using var result = Vp8lReferenceMatcher.Build(source, item.Width, item.Quality, item.Low, context);
            var bytes = new byte[source.Length * 4];
            for (var i = 0; i < source.Length; i++)
                BinaryPrimitives.WriteUInt32LittleEndian(bytes.AsSpan(i * 4), result.Values[i]);
            digest.AppendData(Encoding.UTF8.GetBytes(item.Name)); digest.AppendData(bytes);
            Assert.Equal(0u, result.Values[0]); Assert.Equal(0u, result.Values[^1]);
            Assert.Equal(original, source);
        }
        Assert.Equal("155b0e2308fa02397573b324cfd1e00ebfa35ebf0a7dda4502faef00ddf66cdd",
            Convert.ToHexStringLower(digest.GetHashAndReset()));
    }

    // Opaque packed carriers of the original 4103x2 two-color control.
    // libwebp1.6.0 quality75, reference-stage query adapter, no upstream fixture input.
    private static uint[] Pixels()
    {
        var pixels = new uint[1026]; Array.Fill(pixels, 0xff000000u); pixels[^1] = 0xff004000u; return pixels;
    }
    private static string Hash(ReadOnlySpan<uint> values)
    {
        var bytes = new byte[values.Length * 4];
        for (var i = 0; i < values.Length; i++) BinaryPrimitives.WriteUInt32LittleEndian(bytes.AsSpan(i * 4), values[i]);
        return Convert.ToHexStringLower(SHA256.HashData(bytes));
    }

    [Theory]
    [InlineData(false, "b901367dc26c29cbae7817e8a8e6a1763f9a24d56e7ff15c2cd44ada736a9736")]
    [InlineData(true, "4f2cfec1c5dc3827cdeb42906713b37cae91e009aa0e2d211c376ccb9969b3ea")]
    public void MatchesEveryNativeIndexEntryAndKeepsOnlyResultCapacityCharged(bool low, string expected)
    {
        using var root = new TestMemoryBudget(1_000_000);
        using var context = new OperationContext(new() { MemoryBudget = root }, default);
        var source = Pixels(); var original = source.ToArray();
        using var first = Vp8lReferenceMatcher.Build(source, 513, 75, low, context);
        Assert.Equal(expected, Hash(first.Values)); Assert.Equal(4104, root.Bytes);
        using var second = Vp8lReferenceMatcher.Build(source, 513, 75, low, context);
        Assert.Equal(expected, Hash(second.Values)); Assert.Equal(8208, root.Bytes);
        second.Writable[0] = uint.MaxValue; Assert.Equal(uint.MaxValue, second.Values[0]);
        Assert.Equal(expected, Hash(first.Values)); Assert.Equal(original, source);
        second.Dispose(); Assert.Equal(4104, root.Bytes);
        first.Dispose(); first.Dispose(); Assert.Equal(0, root.Bytes); Assert.False(root.IsDisposed);
        Assert.Throws<ObjectDisposedException>(() => first.Values.Length);
    }

    [Fact]
    public void DenseHeadsAndMaximalMatchesUseTheExistingAxisResidualReference()
    {
        // Border residuals of the original16384x1 axis recipe; stage-only
        // reference, without a native full-encoder claim at this dimension.
        var source = new uint[16384];
        for (var i = 1; i < source.Length; i++)
            source[i] = 0x00010000u | (i % 256 == 0 ? 0x100u : 0u) | (i % 4 == 0 ? 14u : 13u);
        using var root = new TestMemoryBudget(2_000_000);
        using var context = new OperationContext(new() { MemoryBudget = root }, default);
        using var index = Vp8lReferenceMatcher.Build(source, 16384, 75, false, context);
        Assert.Equal("8680a36126915fd200a43406d31f0f7c2094041afab506de2788d51a44e9f03e", Hash(index.Values));
        Assert.Contains(index.Values.ToArray(), value => (value & 4095) == 4095);
        Assert.Equal(65536, root.Bytes); Assert.Equal(1_114_112, root.Peak);
        index.Dispose(); Assert.Equal(0, root.Bytes);
    }

    [Fact]
    public void BoxMatchesNativeIndexAndRetainsItsOwnChargeUntilDisposed()
    {
        using var root = new TestMemoryBudget(1_000_000);
        using var context = new OperationContext(new() { MemoryBudget = root }, default);
        var source = Pixels();
        using var ordinary = Vp8lReferenceMatcher.Build(source, 513, 75, false, context);
        using var box = Vp8lReferenceMatcher.BuildBox(source, 513, ordinary.Values, context);
        Assert.Equal("036d7cf07d987fa2b59bb7e162f3d0a18141f1cd6ba2747843ec9a729295b6b7", Hash(box.Values));
        Assert.Equal(8208, root.Bytes); box.Dispose(); Assert.Equal(4104, root.Bytes);
        Assert.Equal("b901367dc26c29cbae7817e8a8e6a1763f9a24d56e7ff15c2cd44ada736a9736", Hash(ordinary.Values));
        ordinary.Dispose(); Assert.Equal(0, root.Bytes); Assert.False(root.IsDisposed);
    }

    [Fact]
    public void AggregateReplacementPeakRefusesOnceAndRecoversAfterPriorOwnerEnds()
    {
        using var root = new TestMemoryBudget(42_000);
        Assert.True(root.TryReserve(4104, out var input)); using var inputOwner = input;
        var source = Pixels(); var limits = new ImageLimits { MemoryBudget = root };
        using var firstContext = new OperationContext(limits, default);
        using var secondContext = new OperationContext(limits, default);
        using var first = Vp8lReferenceMatcher.Build(source, 513, 75, false, firstContext);
        var retained = root.Bytes;
        var error = Assert.Throws<ImageLimitException>(() => Vp8lReferenceMatcher.Build(source, 513, 75, false, secondContext));
        Assert.Equal((ImageLimitKind.WorkingBytes, ImageLimitScope.SharedBudget), (error.Limit, error.Scope));
        Assert.Equal(1, root.Refusals); Assert.Equal(retained, root.Bytes); Assert.False(root.IsDisposed);
        first.Dispose(); Assert.Equal(4104, root.Bytes);
        using var next = Vp8lReferenceMatcher.Build(source, 513, 75, false, secondContext);
        Assert.Equal(8208, root.Bytes); next.Dispose(); Assert.Equal(4104, root.Bytes);
    }

    [Fact]
    public void CancellationDuringAdmissionRefundsUnusedCapacityAndDoesNotRetry()
    {
        using var root = new TestMemoryBudget(1_000_000); using var cancellation = new CancellationTokenSource();
        root.Admitted = bytes => { if (bytes > 4104) cancellation.Cancel(); };
        using var context = new OperationContext(new() { MemoryBudget = root }, cancellation.Token);
        Assert.Throws<OperationCanceledException>(() => Vp8lReferenceMatcher.Build(Pixels(), 513, 75, false, context));
        Assert.Equal(0, root.Bytes); Assert.Equal(0, root.Refusals); Assert.False(root.IsDisposed);
        root.Admitted = null;
        using var nextContext = new OperationContext(new() { MemoryBudget = root }, default);
        using var next = Vp8lReferenceMatcher.Build(Pixels(), 513, 75, false, nextContext);
        next.Dispose(); Assert.Equal(0, root.Bytes);
    }

    [Fact]
    public void ZeroOwnerIsValidAndNegativeCapacityAllocatesNothing()
    {
        using var root = new TestMemoryBudget(0);
        using var context = new OperationContext(new() { MemoryBudget = root }, default);
        Assert.Throws<ArgumentOutOfRangeException>(() => new Vp8lMatchIndex(context, -1));
        using var zero = new Vp8lMatchIndex(context, 0); Assert.Empty(zero.Values.ToArray());
        zero.Dispose(); Assert.Equal(0, root.Bytes); Assert.False(root.IsDisposed);
    }
}
