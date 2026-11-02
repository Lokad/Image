using Lokad.Image.Internal;
using Lokad.Image.Tooling;

namespace Lokad.Image.Tests;

public sealed class TextRasterizerTests
{
    private static TextLayout Rectangle()
    {
        var outline = AuthoredTrueType.Outline([[(0, 0, true), (0, 600, true), (1000, 600, true), (1000, 0, true)]]);
        return TextLayout.Create(TrueTypeFont.Load(AuthoredTrueType.Build(glyphOverrides: new Dictionary<int, byte[]> { [2] = outline })), "A", new TextOptions { FontSize = 10 });
    }
    [Fact]
    public void ExactPeakAdmissionCountsSubdivisionStackAndReleasesEveryLease()
    {
        var layout = Rectangle(); var context = new OperationContext(new() { MaxWorkingBytes = 6560 }, default);
        var raster = new TextRasterizer(layout, context, 10); Assert.Equal(1328, context.RemainingWorkingBytes);
        raster.Dispose(); raster.Dispose(); Assert.Equal(6560, context.RemainingWorkingBytes);
        var denied = new OperationContext(new() { MaxWorkingBytes = 6559 }, default);
        var failure = Assert.Throws<ImageLimitException>(() => new TextRasterizer(layout, denied, 10));
        Assert.Equal(ImageLimitKind.WorkingBytes, failure.Limit); Assert.Equal(6559, denied.RemainingWorkingBytes);
    }

    [Theory]
    [InlineData(false)] [InlineData(true)]
    public void EveryPreparationGrowthCleansUpOnRefusalOrCancellationAndRootRemainsReusable(bool cancel)
    {
        using var font = TrueTypeFont.Load(AuthoredTrueType.Build());
        using var layout = TextLayout.Create(font, "AgVg", new() { FontSize = 18 });
        long growths;
        using (var root = new TestMemoryBudget(1_000_000))
        {
            using (var context = new OperationContext(new() { MemoryBudget = root }, default))
            using (var raster = new TextRasterizer(layout, context, 40)) growths = root.GrowthAttempts;
            Assert.Equal(0, root.Bytes);
        }
        // Includes edge/event replacements and the buffer shared by preparation
        // and scan conversion. Failure must never retry admission or retain it.
        for (var stop = 1L; stop <= growths; stop++)
        {
            using var root = new TestMemoryBudget(1_000_000);
            using var cancellation = new CancellationTokenSource();
            if (cancel) root.Admitted = _ => { if (root.GrowthAttempts == stop) cancellation.Cancel(); };
            else root.RefuseAtGrowth = stop;
            using (var context = new OperationContext(new() { MemoryBudget = root }, cancellation.Token))
            {
                if (cancel) Assert.ThrowsAny<OperationCanceledException>(() => new TextRasterizer(layout, context, 40));
                else
                {
                    var failure = Assert.Throws<ImageLimitException>(() => new TextRasterizer(layout, context, 40));
                    Assert.Equal(ImageLimitKind.WorkingBytes, failure.Limit);
                    Assert.Equal(ImageLimitScope.SharedBudget, failure.Scope);
                    Assert.Equal(1, root.Refusals);
                    Assert.Equal(stop, root.GrowthAttempts);
                }
                Assert.Equal(0, root.Bytes);
            }
            root.Admitted = null; root.RefuseAtGrowth = 0;
            using (var context = new OperationContext(new() { MemoryBudget = root }, default))
            using (var raster = new TextRasterizer(layout, context, 40)) Assert.True(root.Bytes > 0);
            Assert.Equal(0, root.Bytes); Assert.False(root.IsDisposed);
        }
    }
}
