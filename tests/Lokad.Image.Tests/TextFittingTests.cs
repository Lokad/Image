using Lokad.Image.Tooling;

namespace Lokad.Image.Tests;

public sealed class TextFittingTests
{
    private static TextOptions Options => new() { FontSize = 10 };
    private static TrueTypeFont Font() => TrueTypeFont.Load(AuthoredTrueType.Build());

    [Fact]
    public void ReturnsLargestGridSizeAndTheSameMeasuredLayoutUsedForRendering()
    {
        using var font = Font();
        Assert.True(TextLayout.TryFit(font, "A A", 8, 16, out var fitted, Options with { FontSize = 12 }, minimumFontSize: 4, fontSizeStep: .5));
        using (fitted)
        {
            Assert.Equal(7, fitted.FontSize); Assert.Equal(2, fitted.LineCount);
            using var prepared = TextLayout.Create(font, "A A", Options with { FontSize = 7, WrapWidth = 8 });
            Assert.Equal(prepared.Bounds, fitted.Bounds);
            Assert.Equal(Enumerable.Range(0, prepared.GlyphCount).Select(prepared.GetGlyph), Enumerable.Range(0, fitted.GlyphCount).Select(fitted.GetGlyph));
            using var actual = ImageBuffer.Create(10, 18, 16); using var expected = ImageBuffer.Create(10, 18, 16);
            ImageDrawing.DrawText(actual, fitted, -fitted.Bounds.X, -fitted.Bounds.Y, RgbaColor.From8(1, 2, 3));
            ImageDrawing.DrawText(expected, prepared, -prepared.Bounds.X, -prepared.Bounds.Y, RgbaColor.From8(1, 2, 3));
            for (var y = 0; y < actual.Height; y++) Assert.True(actual.GetRow16(y).SequenceEqual(expected.GetRow16(y)));
        }
    }

    [Fact]
    public void MaximumAndExactMinimumAreCandidatesEvenWhenTheStepDoesNotDivideTheRange()
    {
        using var font = Font();
        Assert.True(TextLayout.TryFit(font, "A", 100, 100, out var maximum, Options, minimumFontSize: 3, fontSizeStep: 4));
        using (maximum) Assert.Equal(10, maximum.FontSize);
        Assert.True(TextLayout.TryFit(font, "A", 100, 4, out var minimum, Options, minimumFontSize: 3, fontSizeStep: 4));
        using (minimum) Assert.Equal(3, minimum.FontSize);
        Assert.True(TextLayout.TryFit(font, "", 1, 1, out var empty, Options, minimumFontSize: 10));
        using (empty) { Assert.Equal(10, empty.FontSize); Assert.Equal(default, empty.Bounds); }
    }

    [Fact]
    public void FloatingSubtractionCannotPublishASizeBelowTheExactMinimum()
    {
        using var font = Font();
        Assert.True(.08 - 7 * .01 < .01);
        Assert.True(TextLayout.TryFit(font, "A", 1, .01, out var layout, Options with { FontSize = .08 }, minimumFontSize: .01, fontSizeStep: .01));
        using (layout) Assert.Equal(.01, layout.FontSize);
    }

    [Fact]
    public void AStepLargerThanTheRangeCannotLoseTheMaximumThroughDivisionUnderflow()
    {
        using var font = Font(); var minimum = Math.BitDecrement(1d);
        Assert.Equal(0, (1 - minimum) / double.MaxValue);
        Assert.True(TextLayout.TryFit(font, "A", 100, 100, out var layout, Options with { FontSize = 1 },
            minimumFontSize: minimum, fontSizeStep: double.MaxValue));
        using (layout) Assert.Equal(1, layout.FontSize);
    }

    [Fact]
    public void OverhangAndWrappingCanMakeFitNonmonotonicAcrossFontSizes()
    {
        var overhang = AuthoredTrueType.Outline([[(0, 0, true), (0, 600, true), (800, 600, true), (800, 0, true)]]);
        using var font = TrueTypeFont.Load(AuthoredTrueType.Build(glyphOverrides: new Dictionary<int, byte[]> { [4] = overhang }));
        using var smaller = TextLayout.Create(font, "Ag", Options with { FontSize = 11, WrapWidth = 15 });
        using var larger = TextLayout.Create(font, "Ag", Options with { FontSize = 18, WrapWidth = 15 });
        Assert.True(smaller.Bounds.Width > 15); Assert.True(larger.Bounds.Width < 15);
        Assert.True(TextLayout.TryFit(font, "Ag", 15, 100, out var fitted, Options with { FontSize = 20 }, minimumFontSize: 11, fontSizeStep: 1));
        using (fitted) { Assert.Equal(18, fitted.FontSize); Assert.Equal(2, fitted.LineCount); }
    }

    [Fact]
    public void NegativeInkOverhangIsPreservedWithExplicitPlacementGuidance()
    {
        using var font = Font();
        Assert.True(TextLayout.TryFit(font, "Á", 8, 8, out var fitted, Options, minimumFontSize: 4, fontSizeStep: 1));
        using (fitted)
        {
            Assert.Equal(7, fitted.FontSize); Assert.True(fitted.Bounds.Y < 0);
            Assert.False(fitted.Bounds.FitsInside(8, 8));
            Assert.True(fitted.Bounds.Translate(-fitted.Bounds.X, -fitted.Bounds.Y).FitsInside(8, 8));
        }
    }

    [Fact]
    public void NoFitReturnsNullAndReleasesAllTrialStorageWithoutDisposingTheRoot()
    {
        using var font = Font(); using var budget = new TestMemoryBudget(1_000_000);
        var options = Options with { Limits = new() { MemoryBudget = budget } };
        Assert.False(TextLayout.TryFit(font, "A", .1, .1, out var layout, options, minimumFontSize: 8, fontSizeStep: 1));
        Assert.Null(layout); Assert.Equal(0, budget.Bytes); Assert.False(budget.IsDisposed);
        Assert.True(TextLayout.TryFit(font, "A", 100, 100, out var next, options));
        next.Dispose(); Assert.Equal(0, budget.Bytes);
    }

    [Fact]
    public void RejectedCandidatesReleaseCapacityBeforeTheNextOneAndTheResultOwnsItsLifetime()
    {
        using var font = Font(); long largestSingle = 0;
        foreach (var size in new[] { 10d, 9, 8 })
        {
            using var singleBudget = new TestMemoryBudget(1_000_000);
            using var prepared = TextLayout.Create(font, "A", Options with { FontSize = size, WrapWidth = 100, Limits = new() { MemoryBudget = singleBudget } });
            largestSingle = Math.Max(largestSingle, singleBudget.Peak);
        }
        using var budget = new TestMemoryBudget(largestSingle);
        Assert.True(TextLayout.TryFit(font, "A", 100, 8, out var result, Options with { Limits = new() { MemoryBudget = budget } }, minimumFontSize: 8, fontSizeStep: 1));
        Assert.Equal(largestSingle, budget.Peak); Assert.True(budget.Bytes > 0);
        font.Dispose();
        using (var canvas = ImageBuffer.Create(20, 20, 8)) ImageDrawing.DrawText(canvas, result, 0, 0, RgbaColor.From8(1, 2, 3));
        result.Dispose(); result.Dispose(); Assert.Equal(0, budget.Bytes);
    }

    [Fact]
    public void EveryGrowingAdmissionRefusesTerminallyAndAllowsSubsequentWorkAfterCleanup()
    {
        using var font = Font(); using var budget = new TestMemoryBudget(1_000_000);
        var options = Options with { Limits = new() { MemoryBudget = budget }, Shaping = TextShaping.OpenType };
        Assert.True(TextLayout.TryFit(font, "AV", 100, 8, out var sample, options, minimumFontSize: 8, fontSizeStep: 1));
        var boundaries = budget.GrowthAttempts; sample.Dispose(); Assert.Equal(0, budget.Bytes);
        for (var i = 1; i <= boundaries; i++)
        {
            budget.RefuseAtGrowth = budget.GrowthAttempts + i;
            var error = Assert.Throws<ImageLimitException>(() => TextLayout.TryFit(font, "AV", 100, 8, out _, options, minimumFontSize: 8, fontSizeStep: 1));
            Assert.Equal(ImageLimitScope.SharedBudget, error.Scope); Assert.Equal(0, budget.Bytes);
            Assert.Equal(budget.RefuseAtGrowth, budget.GrowthAttempts);
        }
        budget.RefuseAtGrowth = 0;
        Assert.True(TextLayout.TryFit(font, "AV", 100, 100, out var next, options)); next.Dispose(); Assert.Equal(0, budget.Bytes);
    }

    [Fact]
    public void CancellationDuringASecondTrialReleasesStorageBeforeReturning()
    {
        using var font = Font(); using var budget = new TestMemoryBudget(1_000_000); using var cancellation = new CancellationTokenSource();
        var starts = 0;
        budget.Admitted = bytes => { if (bytes == 512 && ++starts == 2) cancellation.Cancel(); };
        Assert.Throws<OperationCanceledException>(() => TextLayout.TryFit(font, "A", 1, 1, out _,
            Options with { Limits = new() { MemoryBudget = budget } }, minimumFontSize: 8, fontSizeStep: 1, cancellationToken: cancellation.Token));
        Assert.Equal(2, starts); Assert.Equal(0, budget.Bytes); budget.Admitted = null;
        Assert.True(TextLayout.TryFit(font, "A", 100, 100, out var next, Options with { Limits = new() { MemoryBudget = budget } }));
        next.Dispose(); Assert.Equal(0, budget.Bytes);
    }

    [Fact]
    public void WorkIsCumulativeAcrossCompletedTrialsRatherThanResetForEachSize()
    {
        using var font = Font(); var low = 1; var high = 1024;
        while (low < high)
        {
            var middle = (low + high) / 2;
            try { using var probe = TextLayout.Create(font, "A", Options with { Limits = new() { MaxWork = middle } }); high = middle; }
            catch (ImageLimitException error) when (error.Limit == ImageLimitKind.Work) { low = middle + 1; }
        }
        using var budget = new TestMemoryBudget(1_000_000); var limits = new ImageLimits { MaxWork = low + 4, MemoryBudget = budget };
        using (var single = TextLayout.Create(font, "A", Options with { Limits = limits })) Assert.True(budget.Bytes > 0);
        Assert.Equal(0, budget.Bytes);
        var failure = Assert.Throws<ImageLimitException>(() => TextLayout.TryFit(font, "A", 1, 1, out _, Options with { Limits = limits }, minimumFontSize: 9, fontSizeStep: 1));
        Assert.Equal(ImageLimitKind.Work, failure.Limit); Assert.Equal(0, budget.Bytes);
    }

    [Fact]
    public void IndependentRetainedResultsUseTheSameCallerRoot()
    {
        using var font = Font(); using var budget = new TestMemoryBudget(1_000_000);
        var options = Options with { Limits = new() { MemoryBudget = budget } };
        Assert.True(TextLayout.TryFit(font, "A", 100, 100, out var first, options)); var retained = budget.Bytes;
        Assert.True(TextLayout.TryFit(font, "V", 100, 100, out var second, options)); Assert.True(budget.Bytes > retained);
        first.Dispose(); Assert.Equal(retained, budget.Bytes); Assert.Equal(3, second.GetGlyph(0).GlyphIndex);
        second.Dispose(); Assert.Equal(0, budget.Bytes);
    }

    [Fact]
    public void AResultRetainedByNestedCallerWorkSurvivesOuterNoFitCleanup()
    {
        using var font = Font(); using var budget = new TestMemoryBudget(1_000_000);
        var options = Options with { Limits = new() { MemoryBudget = budget } };
        TextLayout? nested = null; var entered = false;
        budget.Admitted = _ =>
        {
            if (entered) return; entered = true;
            Assert.True(TextLayout.TryFit(font, "V", 100, 100, out nested, options));
        };
        Assert.False(TextLayout.TryFit(font, "A", .1, .1, out _, options, minimumFontSize: 9, fontSizeStep: 1));
        Assert.NotNull(nested); Assert.True(budget.Bytes > 0); Assert.Equal(3, nested.GetGlyph(0).GlyphIndex);
        nested.Dispose(); Assert.Equal(0, budget.Bytes); Assert.False(budget.IsDisposed);
    }

    [Fact]
    public void CandidateCountAndResourceFailuresAreDistinctFromNoFit()
    {
        using var font = Font(); using var budget = new TestMemoryBudget(1_000_000);
        var limits = new ImageLimits { MaxContainerItems = 2, MemoryBudget = budget };
        Assert.Equal(ImageLimitKind.ContainerItems, Assert.Throws<ImageLimitException>(() => TextLayout.TryFit(font, "A", 100, 100, out _, Options with { Limits = limits }, minimumFontSize: 1, fontSizeStep: 1)).Limit);
        Assert.Equal(0, budget.Bytes);
        Assert.Equal(ImageLimitKind.Dimension, Assert.Throws<ImageLimitException>(() => TextLayout.TryFit(font, "A", 100, 100, out _, Options with { Limits = new() { MaxDimension = 9 } })).Limit);
        Assert.Throws<ArgumentOutOfRangeException>(() => TextLayout.TryFit(font, "A", 0, 1, out _));
        Assert.Throws<ArgumentOutOfRangeException>(() => TextLayout.TryFit(font, "A", 1, 1, out _, minimumFontSize: 20));
        Assert.Throws<ArgumentOutOfRangeException>(() => TextLayout.TryFit(font, "A", 1, 1, out _, fontSizeStep: 0));
        var terminal = Options with { FontSize = 20, Limits = new() { MaxDimension = 20, MemoryBudget = budget } };
        Assert.Equal(ImageLimitKind.Dimension, Assert.Throws<ImageLimitException>(() => TextLayout.TryFit(font, "AAAAAAAAAA", 1000, 20, out _, terminal)).Limit);
        Assert.Equal(0, budget.Bytes);
    }
}
