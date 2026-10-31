using Lokad.Image.Internal;
using Lokad.Image.Tooling;

namespace Lokad.Image.Tests;

public sealed class Vp8lPreparedCountingTests
{
    private static Vp8lImageCode Prepare(string kind, OperationContext context)
    {
        if (kind == "auxiliary")
        {
            var pixels = Enumerable.Range(0, 91).Select(i => 0xff000000u | (uint)(i % 13 * 7919)).ToArray();
            var result = Vp8lImageCode.Auxiliary(pixels, 13, 75, false, context);
            var before = Emit(result, context); Array.Clear(pixels);
            Assert.Equal(before, Emit(result, context)); // Prepared state borrows no pixels.
            return result;
        }

        var width = kind == "singleton" ? 1 : kind == "groups" ? 32 : kind == "cache" ? 8 : 17;
        var height = kind == "singleton" ? 1 : kind == "groups" ? 32 : kind == "cache" ? 11 : 19;
        var tokens = new Vp8lTokenBuffer(context, width * height);
        try
        {
            uint[] colors = [0xff000000u, 0xaa555555u, 0x55aaaaaau, 0x00ffffffu];
            var pixels = new uint[width * height];
            for (var i = 0; i < pixels.Length; i++)
            {
                pixels[i] = kind switch
                {
                    "singleton" => 0x00112233u,
                    "groups" => colors[i / width / 16 * 2 + i % width / 16],
                    "cache" => colors[i % colors.Length],
                    _ => 0xff000000u | (uint)(i * 7919 & 0xffffff)
                };
                if (kind != "copy" || i < 200) tokens.Add(Vp8lToken.Literal(pixels[i]));
            }
            if (kind == "copy") tokens.Add(Vp8lToken.Copy(123, 200)); // Cross-row copy with both prefix extras.
            if (kind == "cache")
            {
                Vp8lReferenceCache.Apply(pixels, tokens, 4, context);
                Assert.Contains(tokens.Tokens.ToArray(), token => token.Code < 0);
            }
        }
        catch { tokens.Dispose(); throw; }
        return Vp8lImageCode.TakeMain(tokens, width, height, kind == "groups" ? 4 : 2, false, context, 100);
    }

    private static byte[] Emit(Vp8lImageCode image, OperationContext context)
    {
        using var output = new MemoryStream(); var writer = new WebpBitWriter(output, context);
        image.Write(writer); writer.Finish(); return output.ToArray();
    }

    [Fact]
    public void PreparedCountingMatchesRealEmissionAcrossBitOffsetsAndTokenKinds()
    {
        using var root = new TestMemoryBudget(1_000_000);
        using var context = new OperationContext(new() { MemoryBudget = root }, default);
        foreach (var kind in new[] { "singleton", "copy", "cache", "groups", "auxiliary" })
        {
            using var image = Prepare(kind, context); var retained = root.Bytes;
            if (kind == "groups") Assert.NotEqual(0, Emit(image, context)[0] & 4); // Entropy-map boundary, including auxiliary emission.
            for (var offset = 0; offset < 8; offset++)
            {
                using var output = new MemoryStream(); using var count = new WebpLosslessEncoder.CountStream(int.MaxValue);
                var real = new WebpBitWriter(output, context); var counted = new WebpBitWriter(count, context);
                real.Write((1 << offset) - 1, offset); counted.Write((1 << offset) - 1, offset);
                image.Write(real); image.Write(counted);
                Assert.Equal(real.PendingBits, counted.PendingBits); Assert.Equal(output.Length, count.Count);
                real.Write(0x51d, 11); counted.Write(0x51d, 11); // Continue after the body and its partial byte.
                Assert.Equal(real.PendingBits, counted.PendingBits);
                real.Finish(); counted.Finish(); Assert.Equal(output.Length, count.Count);
                Assert.Equal(retained, root.Bytes);
            }
            image.Dispose(); Assert.Equal(0, root.Bytes);
        }
        Assert.False(root.IsDisposed);
    }

    [Fact]
    public void BoundedCountRefusalPreservesPreparedOwnershipAndAllowsSubsequentUse()
    {
        using var root = new TestMemoryBudget(1_000_000);
        using var context = new OperationContext(new() { MemoryBudget = root }, default);
        using var image = Prepare("copy", context); var retained = root.Bytes;
        var packet = Emit(image, context);
        using var limited = new WebpLosslessEncoder.CountStream(packet.Length - 1);
        var writer = new WebpBitWriter(limited, context);
        var error = Assert.Throws<ImageLimitException>(() => { image.Write(writer); writer.Finish(); });
        Assert.Equal(ImageLimitKind.OutputBytes, error.Limit); Assert.True(limited.CanWrite);
        Assert.Equal(retained, root.Bytes); Assert.Equal(packet, Emit(image, context));
        using var exact = new WebpLosslessEncoder.CountStream(packet.Length);
        writer = new WebpBitWriter(exact, context); image.Write(writer); writer.Finish();
        Assert.Equal(packet.Length, exact.Count); image.Dispose(); Assert.Equal(0, root.Bytes);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void CountingHeaderFailureReleasesScratchAndKeepsBorrowedRootUsable(bool cancel)
    {
        using var root = new TestMemoryBudget(1_000_000); using var cancellation = new CancellationTokenSource();
        using (var context = new OperationContext(new() { MemoryBudget = root }, cancellation.Token))
        {
            using var image = Prepare("copy", context); var retained = root.Bytes;
            if (cancel) root.Admitted = _ => cancellation.Cancel();
            else root.RefuseAtGrowth = root.GrowthAttempts + 1;
            using var count = new WebpLosslessEncoder.CountStream(int.MaxValue);
            var writer = new WebpBitWriter(count, context);
            if (cancel) Assert.Throws<OperationCanceledException>(() => image.Write(writer));
            else
            {
                var error = Assert.Throws<ImageLimitException>(() => image.Write(writer));
                Assert.Equal((ImageLimitKind.WorkingBytes, ImageLimitScope.SharedBudget), (error.Limit, error.Scope));
                Assert.Equal(root.RefuseAtGrowth, root.GrowthAttempts); Assert.Equal(1, root.Refusals);
            }
            Assert.Equal(retained, root.Bytes); root.Admitted = null; root.RefuseAtGrowth = 0;
        }
        Assert.Equal(0, root.Bytes); Assert.False(root.IsDisposed);
        using var nextContext = new OperationContext(new() { MemoryBudget = root }, default);
        using var next = Prepare("copy", nextContext); Assert.NotEmpty(Emit(next, nextContext));
        next.Dispose(); Assert.Equal(0, root.Bytes);
    }
}
