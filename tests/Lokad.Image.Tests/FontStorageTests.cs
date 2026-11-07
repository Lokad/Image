using System.Text;
using Lokad.Image.Internal;
using Lokad.Image.Tooling;

namespace Lokad.Image.Tests;

public sealed class FontStorageTests
{
    [Theory]
    [InlineData(false)] [InlineData(true)]
    public void ReusedStorageIsFullyCopiedAndLiveFacesRemainIndependent(bool longLocations)
    {
        var input = AuthoredTrueType.Build(longLocations: longLocations);
        var pool = new PixelStoragePool(); var dirty = new byte[input.Length];
        dirty.AsSpan().Fill(0xa5); pool.Return(dirty);
        var first = TrueTypeFont.LoadCore(input, null, default, pool);
        var second = TrueTypeFont.LoadCore(AuthoredTrueType.Build(longLocations: longLocations, style: FontStyle.Bold), null, default, pool);
        Assert.Equal(600, first.GetGlyphMetrics(new Rune('A')).AdvanceWidth);
        Assert.Equal(660, second.GetGlyphMetrics(new Rune('A')).AdvanceWidth);
        Assert.Equal(input, dirty); input.AsSpan().Clear();
        Assert.Equal(600, first.GetGlyphMetrics(2).AdvanceWidth);
        first.Dispose(); first.Dispose();
        Assert.Throws<ObjectDisposedException>(() => first.GetGlyphMetrics(2));
        var returned = pool.RentPreservingIdle(dirty.Length);
        Assert.Same(dirty, returned); returned.AsSpan().Fill(0x5a);
        Assert.Equal(660, second.GetGlyphMetrics(2).AdvanceWidth);
        second.Dispose();
    }

    [Theory]
    [InlineData(8)] [InlineData(16)]
    public void PreparedLayoutSurvivesFontDisposalAndCompleteStorageOverwrite(int depth)
    {
        var input = AuthoredTrueType.Build(); var pool = new PixelStoragePool();
        var font = TrueTypeFont.LoadCore(input, null, default, pool);
        using var layout = TextLayout.Create(font, "Ag A\u0301", new TextOptions { FontSize = 20 });
        using var before = ImageBuffer.Create(100, 40, depth);
        ImageDrawing.DrawText(before, layout, 1, 1, RgbaColor.From8(123, 234, 45));
        var bounds = layout.InkBounds;
        font.Dispose();
        var released = pool.RentPreservingIdle(input.Length); released.AsSpan().Fill(0x5a);
        pool.ReturnLatest(released);
        using var another = TrueTypeFont.LoadCore(AuthoredTrueType.Build(style: FontStyle.Bold), null, default, pool);
        using var after = ImageBuffer.Create(100, 40, depth);
        ImageDrawing.DrawText(after, layout, 1, 1, RgbaColor.From8(123, 234, 45));
        Assert.Equal(bounds, layout.InkBounds);
        Assert.True(before.Pixels.SequenceEqual(after.Pixels));
        Assert.Equal(660, another.GetGlyphMetrics(2).AdvanceWidth);
    }

    [Fact]
    public void FontMissPreservesIdleImagesAndLatestReturnHasABoundedWorkingSet()
    {
        var input = AuthoredTrueType.Build(); var pool = new PixelStoragePool(input.Length + 32);
        var image = new byte[32]; image.AsSpan().Fill(0x36); pool.Return(image);
        var font = TrueTypeFont.LoadCore(input, null, default, pool);
        Assert.Same(image, pool.Rent(32)); pool.Return(image);
        font.Dispose();
        var fontBytes = pool.RentPreservingIdle(input.Length);
        Assert.Equal(input, fontBytes); Assert.Same(image, pool.Rent(32));
        Assert.All(image, value => Assert.Equal(0x36, value));
        pool.ReturnLatest(image); pool.ReturnLatest(fontBytes); pool.ReturnLatest(fontBytes);
        var newest = new byte[33]; pool.ReturnLatest(newest);
        Assert.Same(newest, pool.RentPreservingIdle(33));
        Assert.NotSame(fontBytes, pool.RentPreservingIdle(fontBytes.Length)); // Total would exceed the cap.
        var tooLarge = new byte[input.Length + 33]; pool.ReturnLatest(tooLarge);
        Assert.NotSame(tooLarge, pool.RentPreservingIdle(tooLarge.Length));
    }

    [Fact]
    public void GovernedFacesBypassIdleStorageAndRetainAggregateChargesUntilDisposal()
    {
        var input = AuthoredTrueType.Build(); var pool = new PixelStoragePool();
        var idle = new byte[input.Length]; idle.AsSpan().Fill(0xa5); pool.Return(idle);
        var capacity = input.Length + 256L;
        using var root = new TestMemoryBudget(2 * capacity);
        var limits = new ImageLimits { MemoryBudget = root, MaxWorkingBytes = 2L * input.Length + 256 };
        var first = TrueTypeFont.LoadCore(input, limits, default, pool);
        var second = TrueTypeFont.LoadCore(input, limits, default, pool);
        Assert.Equal(2 * capacity, root.Bytes); Assert.Equal(2 * capacity, root.Peak);
        var failure = Assert.Throws<ImageLimitException>(() => TrueTypeFont.LoadCore(input, limits, default, pool));
        Assert.Equal(ImageLimitScope.SharedBudget, failure.Scope);
        Assert.Equal(1, root.Refusals); Assert.Equal(2 * capacity, root.Bytes);
        Assert.Same(idle, pool.RentPreservingIdle(input.Length));
        Assert.All(idle, value => Assert.Equal(0xa5, value)); pool.Return(idle);
        first.Dispose(); Assert.Equal(capacity, root.Bytes);
        using (var recovered = TrueTypeFont.LoadCore(input, limits, default, pool))
            Assert.Equal(2 * capacity, root.Bytes);
        Assert.Equal(capacity, root.Bytes); second.Dispose(); Assert.Equal(0, root.Bytes);
        Assert.False(root.IsDisposed); Assert.Same(idle, pool.RentPreservingIdle(input.Length));
    }

    [Fact]
    public void AdmissionAndCancellationPrecedeStorageReuseAndReleaseUnusedReservations()
    {
        var input = AuthoredTrueType.Build(); var pool = new PixelStoragePool();
        var idle = new byte[input.Length]; idle.AsSpan().Fill(0x36); pool.Return(idle);
        var exact = 2L * input.Length + 256;
        var failure = Assert.Throws<ImageLimitException>(() => TrueTypeFont.LoadCore(input,
            new ImageLimits { MaxWorkingBytes = exact - 1 }, default, pool));
        Assert.Equal(ImageLimitKind.WorkingBytes, failure.Limit);
        using var token = new CancellationTokenSource();
        using var root = new TestMemoryBudget(input.Length + 256L);
        root.Admitted = _ => token.Cancel();
        var limits = new ImageLimits { MemoryBudget = root, MaxWorkingBytes = exact };
        var canceled = Assert.ThrowsAny<OperationCanceledException>(() => TrueTypeFont.LoadCore(input, limits, token.Token, pool));
        Assert.Equal(token.Token, canceled.CancellationToken); Assert.Equal(0, root.Bytes);
        Assert.False(root.IsDisposed); Assert.Same(idle, pool.RentPreservingIdle(input.Length));
        Assert.All(idle, value => Assert.Equal(0x36, value)); pool.Return(idle);
        root.Admitted = null;
        using (var recovered = TrueTypeFont.LoadCore(input, limits, default, pool))
            Assert.Equal(input.Length + 256L, root.Bytes);
        Assert.Equal(0, root.Bytes);
    }

    [Fact]
    public void FailedParsingDoesNotPublishOrRetainAReplacementAndRecoveryFullyOverwrites()
    {
        var input = AuthoredTrueType.Build(); var pool = new PixelStoragePool();
        var old = new byte[input.Length]; pool.Return(old);
        var malformed = input.ToArray(); AuthoredTrueType.W32(malformed, AuthoredTrueType.Offset(malformed, "head") + 12, 0);
        Assert.Throws<ImageDataException>(() => TrueTypeFont.LoadCore(malformed, null, default, pool));
        Assert.Equal(malformed, old);
        var replacement = pool.RentPreservingIdle(input.Length); Assert.NotSame(old, replacement);
        pool.ReturnLatest(replacement);
        using var recovered = TrueTypeFont.LoadCore(input, null, default, pool);
        Assert.Equal(input, replacement); Assert.Equal(600, recovered.GetGlyphMetrics(2).AdvanceWidth);
    }

    [Fact]
    public async Task ConcurrentLatestReturnsNeverShareActiveFontStorage()
    {
        var pool = new PixelStoragePool();
        await Task.WhenAll(Enumerable.Range(0, 8).Select(index => Task.Run(() =>
        {
            var bold = (index & 1) != 0; var input = AuthoredTrueType.Build(style: bold ? FontStyle.Bold : FontStyle.Regular);
            for (var iteration = 0; iteration < 16; iteration++)
            {
                using var font = TrueTypeFont.LoadCore(input, null, default, pool);
                Thread.Yield(); Assert.Equal(bold ? 660 : 600, font.GetGlyphMetrics(2).AdvanceWidth);
            }
        })));
    }
}
