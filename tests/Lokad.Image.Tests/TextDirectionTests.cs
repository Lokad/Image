using System.Text;
using Lokad.Image.Internal;
using Lokad.Image.Tooling;

namespace Lokad.Image.Tests;

public sealed class TextDirectionTests
{
    private static int[] Scalars(string text) => text.EnumerateRunes().Select(r => r.Value).ToArray();
    private static string Levels(BidiParagraph paragraph) => string.Join(",", Enumerable.Range(0, paragraph.Count)
        .Select(i => paragraph.IsRemoved(i) ? "x" : paragraph.Level(i).ToString()));
    private static string Visual(BidiParagraph paragraph, int[] scalars, int start = 0, int length = -1)
    {
        length = length < 0 ? scalars.Length - start : length;
        var order = new int[length]; var levels = new byte[length]; var count = paragraph.ReorderLine(start, length, order, levels);
        return string.Concat(order.Take(count).Select(i => char.ConvertFromUtf32(scalars[i])));
    }

    [Theory]
    [InlineData("ABC אבג 123", 0, "0,0,0,0,1,1,1,1,2,2,2", "ABC 123 גבא")]
    [InlineData("אבג 123", -1, "1,1,1,1,2,2,2", "123 גבא")]
    [InlineData("ب 12,34", -1, "1,1,2,2,2,2,2", "12,34 ب")]
    [InlineData("A 12+34", -1, "0,0,0,0,0,0,0", "A 12+34")]
    [InlineData("א 12+34", 0, "1,1,2,2,2,2,2", "12+34 א")]
    [InlineData("א $12", 0, "1,1,2,2,2", "$12 א")]
    [InlineData("א 12 $", 0, "1,1,2,2,0,0", "12 א $")]
    [InlineData("ب 12$", -1, "1,1,2,2,1", "$12 ب")]
    [InlineData("א ١,٢", 0, "1,1,2,2,2", "١,٢ א")]
    [InlineData("\u0301א", 0, "0,1", "\u0301א")]
    [InlineData("א\u0301", 0, "1,1", "\u0301א")]
    [InlineData("א A", 1, "1,1,2", "A א")]
    [InlineData(" 12 ", -1, "0,0,0,0", " 12 ")]
    public void WeakAndImplicitRulesHaveExactLogicalLevelsAndVisualOrder(string text, int direction, string expectedLevels, string expectedVisual)
    {
        using var context = new OperationContext(ImageLimits.Default, default); var scalars = Scalars(text);
        using var paragraph = new BidiParagraph(scalars, context, direction);
        Assert.Equal(expectedLevels, Levels(paragraph)); Assert.Equal(expectedVisual, Visual(paragraph, scalars));
    }

    [Theory]
    [InlineData("א (A)", 1, "1,1,1,2,1", ")A( א")]
    [InlineData("א A(A)", 1, "1,1,2,2,2,2", "A(A) א")]
    [InlineData("A (א)", 0, "0,0,0,1,0", "A (א)")]
    [InlineData("א (A[ב])", 1, "1,1,1,2,1,1,1,1", ")]ב[A( א")]
    [InlineData("א A(\u0301A)\u0301", 1, "1,1,2,2,2,2,2,2", "A(\u0301A)\u0301 א")]
    [InlineData("א A\u2329A\u3009", 1, "1,1,2,2,2,2", "A\u2329A\u3009 א")]
    public void PairedBracketsRespectEmbeddingContextNestingMarksAndCanonicalPairs(string text, int direction, string expectedLevels, string expectedVisual)
    {
        using var context = new OperationContext(ImageLimits.Default, default); var scalars = Scalars(text);
        using var paragraph = new BidiParagraph(scalars, context, direction);
        Assert.Equal(expectedLevels, Levels(paragraph)); Assert.Equal(expectedVisual, Visual(paragraph, scalars));
    }

    [Theory]
    [InlineData("A\u202Bב12\u202CZ", "0,x,1,2,2,x,0", "A12בZ")]
    [InlineData("A\u202Eab12\u202CZ", "0,x,1,1,1,1,x,0", "A21baZ")]
    [InlineData("א\u202Aab\u202Cב", "1,x,2,2,x,1", "בabא")]
    [InlineData("A\u2067ב12\u2069Z", "0,0,1,2,2,0,0", "A\u206712ב\u2069Z")]
    [InlineData("A\u2068ב12\u2069Z", "0,0,1,2,2,0,0", "A\u206812ב\u2069Z")]
    [InlineData("A\u2068ab\u2069Z", "0,0,2,2,0,0", "A\u2068ab\u2069Z")]
    [InlineData("A\u200Bב", "0,x,1", "Aב")]
    public void ExplicitScopesAndRemovedControlsHaveDeterministicOrder(string text, string expectedLevels, string expectedVisual)
    {
        using var context = new OperationContext(ImageLimits.Default, default); var scalars = Scalars(text);
        using var paragraph = new BidiParagraph(scalars, context);
        Assert.Equal(expectedLevels, Levels(paragraph)); Assert.Equal(expectedVisual, Visual(paragraph, scalars));
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
    public void LineResetsUseOriginalTypesAndDoNotMutateTheParagraphOrOtherLines()
    {
        using var context = new OperationContext(ImageLimits.Default, default); var scalars = Scalars("אב  \tגד  ");
        using var paragraph = new BidiParagraph(scalars, context, 0); var before = Levels(paragraph);
        Assert.Equal("בא  \tדג  ", Visual(paragraph, scalars));
        Assert.Equal("בא  ", Visual(paragraph, scalars, 0, 4));
        Assert.Equal(before, Levels(paragraph)); Assert.Equal("  ", Visual(paragraph, scalars, 7, 2));
    }

    [Fact]
    public void TrailingIsolatesResetEvenWhenTheirCurrentTypesWereOverridden()
    {
        using var context = new OperationContext(ImageLimits.Default, default);
        var scalars = Scalars("\t2\u202E\u202E.A \u2067\u202B\u2066\u202D א");
        using var paragraph = new BidiParagraph(scalars, context, 0);
        var order = new int[12]; var levels = new byte[12];
        var count = paragraph.ReorderLine(0, 12, order, levels);
        Assert.Equal(new[] { 0, 1, 5, 4, 6, 7, 9, 11 }, order[..count]);
        Assert.All(levels.AsSpan(4, count - 4).ToArray(), level => Assert.Equal(0, level));
    }

    [Fact]
    public void ParagraphAndBorrowedLineArgumentsAreValidatedAndDisposedOwnersRejectUse()
    {
        using var context = new OperationContext(ImageLimits.Default, default);
        Assert.Throws<ArgumentException>(() => new BidiParagraph(Scalars("A\nB"), context));
        Assert.Throws<ArgumentOutOfRangeException>(() => new BidiParagraph([-1], context));
        Assert.Throws<ArgumentOutOfRangeException>(() => new BidiParagraph([], context, 2));
        using var empty = new BidiParagraph([], context, 1); Assert.Equal(0, empty.ReorderLine(0, 0, [], []));
        var paragraph = new BidiParagraph(Scalars("A"), context);
        Assert.Throws<ArgumentOutOfRangeException>(() => paragraph.ReorderLine(1, 1, new int[1], new byte[1]));
        Assert.Throws<ArgumentException>(() => paragraph.ReorderLine(0, 1, [], new byte[1]));
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
