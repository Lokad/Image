using Lokad.Image.Tooling;

namespace Lokad.Image.PublicApi.Tests;

public sealed class TextRenderPlanTests
{
    private static readonly RgbaColor Ink = new(12345, 54321, 32769, 45678);

    [Theory]
    [InlineData(8, -.375, 1.625)] [InlineData(16, -.375, 1.625)]
    [InlineData(8, 4.125, -2.375)] [InlineData(16, 4.125, -2.375)]
    public void OwnsGeometryAfterFontAndLayoutDisposalAndReadsFreshCanvases(int depth, double x, double y)
    {
        using var font = TrueTypeFont.Load(AuthoredTrueType.Build());
        using var layout = TextLayout.Create(font, "AV gA\nA g", new() { FontSize = 13.25, WrapWidth = 30, LineSpacing = 1.25 });
        using var root = new TestMemoryBudget(1_000_000);
        using var plan = TextRenderPlan.Create(layout, 32, 24, depth, x, y, new() { MemoryBudget = root });
        using var first = OriginalPixels.Buffer(32, 24, depth);
        using var second = OriginalPixels.Buffer(32, 24, depth, phase: 3);
        using var expectedFirst = first.Clone(); using var expectedSecond = second.Clone();
        ImageDrawing.DrawText(expectedFirst, layout, x, y, Ink);
        ImageDrawing.DrawText(expectedSecond, layout, x, y, Ink with { A = 1 });
        layout.Dispose(); font.Dispose();
        Assert.True(plan.StorageBytes > 0); Assert.Equal(plan.StorageBytes, root.Bytes);
        plan.Draw(first, Ink); plan.Draw(second, Ink with { A = 1 });
        Assert.Equal(OriginalPixels.Bytes(expectedFirst), OriginalPixels.Bytes(first));
        Assert.Equal(OriginalPixels.Bytes(expectedSecond), OriginalPixels.Bytes(second));
        plan.Draw(second, Ink with { A = 0 });
        Assert.Equal(OriginalPixels.Bytes(expectedSecond), OriginalPixels.Bytes(second));
        plan.Dispose(); Assert.Equal(0, root.Bytes); Assert.Equal(0, plan.StorageBytes);
        Assert.Equal(OriginalPixels.Bytes(expectedSecond), OriginalPixels.Bytes(second));
        Assert.Throws<ObjectDisposedException>(() => plan.Draw(second, Ink));
        Assert.False(root.IsDisposed); plan.Dispose();
    }

    [Theory] [InlineData(8)] [InlineData(16)]
    public void AggregateRetainedCapacityAndExecutionTokensHaveIndependentLifetimes(int depth)
    {
        using var root = new TestMemoryBudget(1_000_000); var limits = new ImageLimits { MemoryBudget = root };
        using var font = TrueTypeFont.Load(AuthoredTrueType.Build(), limits);
        using var layout = TextLayout.Create(font, "AgVg", new() { Limits = limits });
        using var canvas = ImageBuffer.Create(64, 32, depth, limits);
        var borrowed = root.Bytes;
        using var creation = new CancellationTokenSource();
        using var plan = TextRenderPlan.Create(layout, 64, 32, depth, 3.25, 1.5, limits, creation.Token);
        creation.Cancel(); var retained = root.Bytes;
        Assert.Equal(borrowed + plan.StorageBytes, retained);
        using (var caller = root.CreateScope())
        {
            Assert.True(caller.TryReserve(1_000_000 - retained - 1, out var held));
            using (held)
            {
                // Drawing reuses already admitted capacity even with no room for a second plan.
                plan.Draw(canvas, Ink);
                var refusal = Assert.Throws<ImageLimitException>(() => TextRenderPlan.Create(layout, 64, 32, depth, 3.25, 1.5, limits));
                Assert.Equal(ImageLimitScope.SharedBudget, refusal.Scope); Assert.Equal(ImageLimitKind.WorkingBytes, refusal.Limit);
                Assert.Equal(1, root.Refusals); Assert.Equal(999999, root.Bytes);
            }
        }
        Assert.Equal(retained, root.Bytes);
        using var cancelled = new CancellationTokenSource(); cancelled.Cancel();
        var before = OriginalPixels.Bytes(canvas);
        var failure = Assert.Throws<OperationCanceledException>(() => plan.Draw(canvas, Ink, cancelled.Token));
        Assert.Equal(cancelled.Token, failure.CancellationToken); Assert.Equal(before, OriginalPixels.Bytes(canvas));
        plan.Draw(canvas, Ink); Assert.Equal(retained, root.Bytes);
        using (var next = TextRenderPlan.Create(layout, 64, 32, depth, 0, 0, limits)) next.Draw(canvas, Ink);
        Assert.Equal(retained, root.Bytes);
        plan.Dispose(); Assert.Equal(borrowed, root.Bytes);
        layout.Dispose(); font.Dispose();
        Assert.Equal(canvas.Width * canvas.Height * 4 * (depth / 8), root.Bytes);
        canvas.Dispose(); Assert.Equal(0, root.Bytes); Assert.False(root.IsDisposed);
    }

    [Theory] [InlineData(false)] [InlineData(true)]
    public void PreparationRefusalAndCancellationCleanEveryPartialOwner(bool cancel)
    {
        using var font = TrueTypeFont.Load(AuthoredTrueType.Build());
        using var layout = TextLayout.Create(font, "AgVg", new() { FontSize = 18 });
        long growths;
        using (var root = new TestMemoryBudget(1_000_000))
        {
            using (var plan = TextRenderPlan.Create(layout, 64, 32, 8, 0, 0, new() { MemoryBudget = root })) growths = root.GrowthAttempts;
            Assert.Equal(0, root.Bytes);
        }
        for (long stop = 1; stop <= growths; stop++)
        {
            using var root = new TestMemoryBudget(1_000_000); using var cancellation = new CancellationTokenSource();
            if (cancel) root.Admitted = _ => { if (root.GrowthAttempts == stop) cancellation.Cancel(); };
            else root.RefuseAtGrowth = stop;
            var limits = new ImageLimits { MemoryBudget = root };
            if (cancel)
                Assert.Throws<OperationCanceledException>(() => TextRenderPlan.Create(layout, 64, 32, 8, 0, 0, limits, cancellation.Token));
            else
            {
                var failure = Assert.Throws<ImageLimitException>(() => TextRenderPlan.Create(layout, 64, 32, 8, 0, 0, limits));
                Assert.Equal(ImageLimitScope.SharedBudget, failure.Scope);
                Assert.Equal(stop, root.GrowthAttempts); Assert.Equal(1, root.Refusals);
            }
            Assert.Equal(0, root.Bytes); Assert.False(root.IsDisposed);
            root.Admitted = null; root.RefuseAtGrowth = 0;
            using (var recovered = TextRenderPlan.Create(layout, 64, 32, 8, 0, 0, limits)) Assert.True(root.Bytes > 0);
            Assert.Equal(0, root.Bytes);
        }
    }

    [Fact]
    public void InvalidCanvasesAndLimitsDoNotMutateOrPoisonThePlan()
    {
        using var font = TrueTypeFont.Load(AuthoredTrueType.Build()); using var layout = TextLayout.Create(font, "A");
        using var canvas = ImageBuffer.Create(32, 24, 8);
        using var plan = TextRenderPlan.Create(layout, 32, 24, 8, 0, 0);
        using var wrongShape = ImageBuffer.Create(31, 24, 8); using var wrongDepth = ImageBuffer.Create(32, 24, 16);
        Assert.Throws<ArgumentNullException>(() => plan.Draw(null!, Ink));
        Assert.Throws<ArgumentException>(() => plan.Draw(wrongShape, Ink));
        Assert.Throws<ArgumentException>(() => plan.Draw(wrongDepth, Ink));
        using var disposed = canvas.Clone(); disposed.Dispose();
        Assert.Throws<ObjectDisposedException>(() => plan.Draw(disposed, Ink));
        plan.Draw(canvas, Ink);
        foreach (var limits in new[] { new ImageLimits { MaxDimension = 31 }, new ImageLimits { MaxPixels = 767 },
            new ImageLimits { MaxWorkingBytes = layout.StorageBytes + 32 * 24 * 4 }, new ImageLimits { MaxContainerItems = 1 } })
            Assert.Throws<ImageLimitException>(() => TextRenderPlan.Create(layout, 32, 24, 8, 0, 0, limits));
        Assert.Throws<ArgumentOutOfRangeException>(() => TextRenderPlan.Create(layout, 32, 24, 8, double.NaN, 0));
        Assert.Throws<ArgumentOutOfRangeException>(() => TextRenderPlan.Create(layout, 32, 24, 8, 0, double.PositiveInfinity));
        Assert.Throws<ArgumentOutOfRangeException>(() => TextRenderPlan.Create(layout, 0, 24, 8, 0, 0));
        Assert.Throws<ArgumentOutOfRangeException>(() => TextRenderPlan.Create(layout, 32, 24, 12, 0, 0));
        layout.Dispose(); Assert.Throws<ObjectDisposedException>(() => TextRenderPlan.Create(layout, 32, 24, 8, 0, 0));
    }

    [Theory] [InlineData(8)] [InlineData(16)]
    public void OffCanvasAndEmptyPlansOwnNoRasterButStillValidateCallers(int depth)
    {
        using var font = TrueTypeFont.Load(AuthoredTrueType.Build());
        using var layout = TextLayout.Create(font, "A"); using var empty = TextLayout.Create(font, "");
        using var canvas = OriginalPixels.Buffer(32, 24, depth); var expected = OriginalPixels.Bytes(canvas);
        using var root = new TestMemoryBudget(1_000_000);
        foreach (var (text, x) in new[] { (layout, double.MaxValue), (empty, 0d) })
        {
            using var plan = TextRenderPlan.Create(text, 32, 24, depth, x, 0, new() { MemoryBudget = root });
            Assert.Equal(0, plan.StorageBytes); plan.Draw(canvas, Ink);
            Assert.Equal(expected, OriginalPixels.Bytes(canvas)); Assert.Equal(0, root.Bytes);
            using var disposed = canvas.Clone(); disposed.Dispose();
            Assert.Throws<ObjectDisposedException>(() => plan.Draw(disposed, Ink));
        }
    }
}
