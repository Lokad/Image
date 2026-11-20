using System.Text;
using Lokad.Image.Internal;
using Lokad.Image.Tooling;

namespace Lokad.Image.Tests;

public sealed class TextDirectionTests
{
    private static int[] Scalars(string text) => text.EnumerateRunes().Select(r => r.Value).ToArray();
    private static string Levels(BidiParagraph paragraph) => string.Join(",", Enumerable.Range(0, paragraph.Count)
        .Select(i => paragraph.IsRemoved(i) ? "x" : paragraph.Level(i).ToString()));

    [Theory]
    [InlineData("ABC אבג 123", 0, "0,0,0,0,1,1,1,1,2,2,2")]
    [InlineData("אבג 123", -1, "1,1,1,1,2,2,2")]
    [InlineData("ب 12,34", -1, "1,1,2,2,2,2,2")]
    [InlineData("A 12+34", -1, "0,0,0,0,0,0,0")]
    [InlineData("א 12+34", 0, "1,1,2,2,2,2,2")]
    [InlineData("א $12", 0, "1,1,2,2,2")]
    [InlineData("א 12 $", 0, "1,1,2,2,0,0")]
    [InlineData("ب 12$", -1, "1,1,2,2,1")]
    [InlineData("א ١,٢", 0, "1,1,2,2,2")]
    [InlineData("\u0301א", 0, "0,1")]
    [InlineData("א\u0301", 0, "1,1")]
    [InlineData("א A", 1, "1,1,2")]
    [InlineData(" 12 ", -1, "0,0,0,0")]
    public void WeakAndImplicitRulesHaveExactLogicalLevels(string text, int direction, string expectedLevels)
    {
        using var context = new OperationContext(ImageLimits.Default, default); var scalars = Scalars(text);
        using var paragraph = new BidiParagraph(scalars, context, direction);
        Assert.Equal(expectedLevels, Levels(paragraph));
    }

    [Theory]
    [InlineData("א (A)", 1, "1,1,1,2,1")]
    [InlineData("א A(A)", 1, "1,1,2,2,2,2")]
    [InlineData("A (א)", 0, "0,0,0,1,0")]
    [InlineData("א (A[ב])", 1, "1,1,1,2,1,1,1,1")]
    [InlineData("א A(\u0301A)\u0301", 1, "1,1,2,2,2,2,2,2")]
    [InlineData("א A\u2329A\u3009", 1, "1,1,2,2,2,2")]
    public void PairedBracketsRespectEmbeddingContextNestingMarksAndCanonicalPairs(string text, int direction, string expectedLevels)
    {
        using var context = new OperationContext(ImageLimits.Default, default); var scalars = Scalars(text);
        using var paragraph = new BidiParagraph(scalars, context, direction);
        Assert.Equal(expectedLevels, Levels(paragraph));
    }

    [Theory]
    [InlineData("A\u202Bב12\u202CZ", "0,x,1,2,2,x,0")]
    [InlineData("A\u202Eab12\u202CZ", "0,x,1,1,1,1,x,0")]
    [InlineData("א\u202Aab\u202Cב", "1,x,2,2,x,1")]
    [InlineData("A\u2067ב12\u2069Z", "0,0,1,2,2,0,0")]
    [InlineData("A\u2068ב12\u2069Z", "0,0,1,2,2,0,0")]
    [InlineData("A\u2068ab\u2069Z", "0,0,2,2,0,0")]
    [InlineData("A\u200Bב", "0,x,1")]
    public void ExplicitScopesAndRemovedControlsHaveExactLogicalLevels(string text, string expectedLevels)
    {
        using var context = new OperationContext(ImageLimits.Default, default); var scalars = Scalars(text);
        using var paragraph = new BidiParagraph(scalars, context);
        Assert.Equal(expectedLevels, Levels(paragraph));
    }

    [Fact]
    public void IsolateContentsDoNotChooseParagraphDirectionOrLeakIntoNumericResolution()
    {
        using var context = new OperationContext(ImageLimits.Default, default);
        using var paragraph = new BidiParagraph(Scalars("\u2067ب\u2069 12 A"), context);
        Assert.Equal(0, paragraph.BaseLevel);
        Assert.Equal("0,1,0,0,0,0,0,0", Levels(paragraph));
        using var nested = new BidiParagraph(Scalars("\u2068\u2067ב\u2069A\u2069א"), context);
        Assert.Equal(1, nested.BaseLevel); Assert.Equal(2, nested.Level(4));
        using var unclosed = new BidiParagraph(Scalars("\u2068ב"), context);
        Assert.Equal(0, unclosed.BaseLevel); Assert.Equal(1, unclosed.Level(1));
    }

    [Fact]
    public void AdjacentIsolateRunSequencesReconnectTheSurroundingContext()
    {
        using var context = new OperationContext(ImageLimits.Default, default);
        using var paragraph = new BidiParagraph(Scalars("א\u2066A\u2069 12\u2066B\u2069ב"), context, 0);
        Assert.Equal("1,1,2,1,1,2,2,1,2,1,1", Levels(paragraph));
    }

    [Fact]
    public void PdfDoesNotCloseAnIsolateAndPdiClosesItsNestedEmbedding()
    {
        using var context = new OperationContext(ImageLimits.Default, default);
        using var paragraph = new BidiParagraph(Scalars("A\u2067\u202Bב\u202C\u202Cג\u2069Z"), context);
        Assert.Equal("0,0,x,3,x,x,1,0,0", Levels(paragraph));
    }

    [Fact]
    public void EmbeddingOverflowAndIsolateOverflowRemainBoundedAndSubsequentTextRecovers()
    {
        using var context = new OperationContext(ImageLimits.Default, default);
        var text = "A" + new string('\u202B', 140) + "ב" + new string('\u202C', 140) + "Z";
        using var embedding = new BidiParagraph(Scalars(text), context);
        Assert.Equal(125, embedding.Level(141)); Assert.Equal(0, embedding.Level(282));
        text = "A" + new string('\u2067', 140) + "ב" + new string('\u2069', 140) + "Z";
        using var isolate = new BidiParagraph(Scalars(text), context);
        Assert.Equal(125, isolate.Level(141)); Assert.Equal(0, isolate.Level(282));
    }

    [Fact]
    public void BracketStackOverflowDiscardsEarlierPairsInThatSequence()
    {
        using var context = new OperationContext(ImageLimits.Default, default);
        var text = "א A(A)" + new string('(', 64) + "א";
        using var paragraph = new BidiParagraph(Scalars(text), context, 1);
        // With N0 disabled by overflow, the opening bracket between Latin letters
        // resolves L under N1, while the close between L and R resolves R.
        Assert.Equal(2, paragraph.Level(3)); Assert.Equal(1, paragraph.Level(5));
    }

    [Fact]
    public void ParagraphArgumentsAreValidatedAndDisposedOwnersRejectUse()
    {
        using var context = new OperationContext(ImageLimits.Default, default);
        Assert.Throws<ArgumentException>(() => new BidiParagraph(Scalars("A\nB"), context));
        Assert.Throws<ArgumentOutOfRangeException>(() => new BidiParagraph([-1], context));
        Assert.Throws<ArgumentOutOfRangeException>(() => new BidiParagraph([], context, 2));
        using var empty = new BidiParagraph([], context, 1); Assert.Equal(0, empty.Count);
        var paragraph = new BidiParagraph(Scalars("A"), context);
        paragraph.Dispose(); paragraph.Dispose(); Assert.Throws<ObjectDisposedException>(() => paragraph.Level(0));
    }

    [Theory]
    [InlineData("ببب", "2,4,8")]
    [InlineData("باب", "2,8,1")]
    [InlineData("ب\u064Eب", "2,0,8")]
    [InlineData("ب\u200Dب", "2,0,8")]
    [InlineData("ب\u200Cب", "1,0,1")]
    [InlineData("ب ب", "1,0,1")]
    [InlineData("\u200Dب", "0,8")]
    [InlineData("ب\u200D", "2,0")]
    [InlineData("\u064Eب", "0,1")]
    public void ArabicJoiningFormsSkipTransparentMarksAndRespectJoinControls(string text, string expected)
    {
        using var context = new OperationContext(ImageLimits.Default, default); using var glyphs = new ShapingBuffer(16, context);
        foreach (var scalar in Scalars(text)) glyphs.Add(new() { Glyph = scalar, Scalar = scalar, Level = 1, Features = 0x1000 });
        ArabicJoining.Apply(glyphs);
        Assert.Equal(expected, string.Join(",", glyphs.Span.ToArray().Select(g => g.Features & ArabicJoining.FormMask)));
        Assert.All(glyphs.Span.ToArray(), g => Assert.Equal(0x1000U, g.Features & ~ArabicJoining.FormMask));
    }

    [Fact]
    public void JoiningStopsAtLevelAndRunBoundariesAndNeverChangesOtherRuns()
    {
        using var context = new OperationContext(ImageLimits.Default, default); using var glyphs = new ShapingBuffer(4, context);
        foreach (var level in new byte[] { 1, 1, 2, 2 }) glyphs.Add(new() { Glyph = 1, Scalar = 0x628, Level = level, Features = 0x1000 });
        ArabicJoining.Apply(glyphs); Assert.Equal(new uint[] { 2, 8, 2, 8 }, glyphs.Span.ToArray().Select(g => g.Features & 15));
        ArabicJoining.Apply(glyphs, 1, 2); Assert.Equal(new uint[] { 2, 1, 1, 8 }, glyphs.Span.ToArray().Select(g => g.Features & 15));
    }

    [Theory]
    [InlineData(0)] [InlineData(1)]
    public void RefusalAtEitherStorageBoundaryIsTerminalAndRefundsForSubsequent(int after)
    {
        using var budget = new TestMemoryBudget(1_000_000) { RefuseAtGrowth = after + 1 };
        using (var context = new OperationContext(new() { MemoryBudget = budget }, default))
        {
            var error = Assert.Throws<ImageLimitException>(() => new BidiParagraph(Scalars("ABC אב"), context));
            Assert.Equal(ImageLimitScope.SharedBudget, error.Scope); Assert.Equal(0, budget.Bytes); Assert.Equal(1, budget.Refusals);
        }
        budget.RefuseAtGrowth = 0;
        using var nextContext = new OperationContext(new() { MemoryBudget = budget }, default);
        using (var next = new BidiParagraph(Scalars("A"), nextContext)) Assert.True(budget.Bytes > 0);
        Assert.Equal(0, budget.Bytes);
    }

    [Fact]
    public void OverlappingRetainedParagraphsShareOneRootAndRespectIndependentLocalLimits()
    {
        using var budget = new TestMemoryBudget(1000);
        using var firstContext = new OperationContext(new() { MemoryBudget = budget }, default);
        using var secondContext = new OperationContext(new() { MemoryBudget = budget }, default);
        using var first = new BidiParagraph(Scalars("ABC אב"), firstContext); var retained = budget.Bytes;
        Assert.Throws<ImageLimitException>(() => new BidiParagraph(Scalars("ABC אב"), secondContext)); Assert.Equal(retained, budget.Bytes);
        first.Dispose(); using var next = new BidiParagraph(Scalars("ABC אב"), secondContext);
        using var limited = new OperationContext(new() { MaxContainerItems = 1 }, default);
        Assert.Throws<ImageLimitException>(() => new BidiParagraph(Scalars("AA"), limited));
        using var bytes = new OperationContext(new() { MaxWorkingBytes = 1 }, default);
        Assert.Throws<ImageLimitException>(() => new BidiParagraph(Scalars("A"), bytes));
    }

    [Fact]
    public void PinnedNormativePropertiesCoverSemanticExceptionsSupplementaryScalarsAndNoncharacters()
    {
        Assert.Equal(BidiClass.L, UnicodeTextProperties.Bidi(0x0CBF));
        Assert.Equal(BidiClass.EN, UnicodeTextProperties.Bidi(0x06F1));
        Assert.Equal(BidiClass.AN, UnicodeTextProperties.Bidi(0x0661));
        Assert.Equal(BidiClass.R, UnicodeTextProperties.Bidi(0x1E900));
        Assert.Equal(BidiClass.AL, UnicodeTextProperties.Bidi(0x10EC0));
        Assert.Equal(BidiClass.BN, UnicodeTextProperties.Bidi(0x10FFFF));
        Assert.Equal(JoiningType.T, UnicodeTextProperties.Joining(0x064E));
        Assert.Equal(JoiningType.C, UnicodeTextProperties.Joining(0x200D));
        Assert.Equal(JoiningType.U, UnicodeTextProperties.Joining(0x200C));
        Assert.Equal(JoiningType.L, UnicodeTextProperties.Joining(0xA872));
        Assert.Equal(0x29, UnicodeTextProperties.Bracket(0x28, out var opening)); Assert.True(opening);
        Assert.Equal(0x28, UnicodeTextProperties.Mirror(0x29));
        Assert.Equal(-1, UnicodeTextProperties.Bracket(0xFD3E, out _)); Assert.Equal(0xFD3E, UnicodeTextProperties.Mirror(0xFD3E));
        Assert.Throws<ArgumentOutOfRangeException>(() => UnicodeTextProperties.Bidi(0xD800));
    }
}
