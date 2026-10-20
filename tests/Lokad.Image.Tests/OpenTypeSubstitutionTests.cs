using Lokad.Image.Internal;
using Lokad.Image.Tooling;
using static Lokad.Image.Tooling.AuthoredOpenType;

namespace Lokad.Image.Tests;

public sealed class OpenTypeSubstitutionTests
{
    private static readonly uint Latin = Tag("latn"), Compose = Tag("ccmp");
    private static byte[] Program(params Lookup[] lookups) => Substitution(lookups, [new("ccmp", Enumerable.Range(0, lookups.Length).ToArray())]);
    private static OperationContext Context(ImageLimits? limits = null, CancellationToken cancellation = default) => new(limits ?? ImageLimits.Default, cancellation);
    private static ShapingBuffer Buffer(OperationContext context, params int[] glyphs)
    {
        var buffer = new ShapingBuffer(glyphs.Length, context);
        for (var i = 0; i < glyphs.Length; i++) buffer.Add(new() { Glyph = glyphs[i], Scalar = 65 + i, SourceOffset = i, SourceLength = 1, Cluster = i, Features = uint.MaxValue });
        return buffer;
    }
    private static int[] Glyphs(ShapingBuffer buffer) => buffer.Span.ToArray().Select(g => g.Glyph).ToArray();

    [Theory]
    [InlineData(false)] [InlineData(true)]
    public void SingleSubstitutionUsesCoverageArraysOrRangesAndRunsEachLookupOnce(bool ranges)
    {
        var coverage = ranges ? CoverageRanges((2, 3, 0)) : Coverage(2, 3);
        var bytes = Program(new Lookup(1, 0, [SingleDelta(coverage, 1)]), new(1, 0, [Single([3, 4], [5, 6])]));
        using var context = Context(); using var run = Buffer(context, 1, 2, 3, 4);
        using var engine = new OpenTypeSubstitution(bytes, default, 64, context);
        Assert.True(engine.ApplyFeature(Latin, 0, Compose, run));
        Assert.Equal(new[] { 1, 5, 6, 6 }, Glyphs(run));
    }

    [Theory]
    [InlineData(2, -1, 64, 1)]
    [InlineData(65534, 2, 65535, 0)]
    public void SingleDeltaUsesUnsignedSixteenBitWrapBeforeGlyphValidation(int input, int delta, int glyphCount, int expected)
    {
        var bytes = Program(new Lookup(1, 0, [SingleDelta(Coverage(input), delta)]));
        using var context = Context(); using var run = Buffer(context, input);
        using var engine = new OpenTypeSubstitution(bytes, default, glyphCount, context);
        engine.ApplyFeature(Latin, 0, Compose, run); Assert.Equal(expected, run[0].Glyph);
    }

    [Fact]
    public void ScriptLanguageSelectionRequiredFeaturesAndLookupListOrderAreExplicit()
    {
        var lookups = new Lookup[] { new(1, 0, [Single([2], [3])]), new(1, 0, [Single([3], [4])]), new(1, 0, [Single([2], [8])]) };
        var features = new Feature[] { new("ccmp", [1, 0, 0]), new("rlig", [2]), new("ccmp", [2]) };
        var scripts = new (string, byte[])[] { ("latn", Script([0], required: 1)), ("arab", Script([2])), ("hebr", NamedLanguage("IWR ", [2])) };
        var bytes = Substitution(lookups, features, scripts);
        using var context = Context(); using var engine = new OpenTypeSubstitution(bytes, default, 64, context);
        using var latin = Buffer(context, 2); engine.ApplyFeature(Latin, 0, Compose, latin); Assert.Equal(4, latin[0].Glyph);
        using var arabic = Buffer(context, 2); engine.ApplyFeature(Tag("arab"), 0, Compose, arabic); Assert.Equal(8, arabic[0].Glyph);
        using var required = Buffer(context, 2); engine.ApplyFeature(Latin, 0, 0, required, required: true); Assert.Equal(8, required[0].Glyph);
        using var hebrew = Buffer(context, 2); Assert.False(engine.ApplyFeature(Tag("hebr"), 0, Compose, hebrew));
        Assert.True(engine.ApplyFeature(Tag("hebr"), Tag("IWR "), Compose, hebrew)); Assert.Equal(8, hebrew[0].Glyph);
    }

    [Fact]
    public void DefaultScriptFallbackAndPerGlyphFeatureMasksDoNotCrossRunBoundaries()
    {
        var bytes = Substitution([new(1, 0, [Single([2], [3])])], [new("init", [0])], [("DFLT", Script([0]))]);
        using var context = Context(); using var run = Buffer(context, 2, 2, 2, 2);
        run[1].Features = 1; run[2].Features = 2;
        using var engine = new OpenTypeSubstitution(bytes, default, 64, context);
        engine.ApplyFeature(Tag("arab"), 0, Tag("init"), run, mask: 1, start: 1, length: 2);
        Assert.Equal(new[] { 2, 3, 2, 2 }, Glyphs(run));
    }

    [Fact]
    public void LigaturePriorityAndIgnoredMarksPreserveSourceCoverageAndComponents()
    {
        var bytes = Program(new Lookup(4, 8, [Ligature(2, (9, [3]), (10, [3, 4]))]));
        var definitions = Definitions(ClassRanges((2, 4, 1), (7, 7, 3), (9, 10, 2)));
        using var context = Context(); using var run = Buffer(context, 2, 7, 3, 4);
        using var engine = new OpenTypeSubstitution(bytes, definitions, 64, context);
        engine.ApplyFeature(Latin, 0, Compose, run);
        Assert.Equal(new[] { 9, 7, 4 }, Glyphs(run));
        Assert.Equal((0, 3, 0, 2), (run[0].SourceOffset, run[0].SourceLength, run[0].Cluster, run[0].ComponentCount));
        Assert.Equal(run[0].Ligature, run[1].Ligature); Assert.Equal(0, run[1].Component); Assert.Equal(0, run[1].Cluster);
        Assert.Equal(3, run[2].SourceOffset);
    }

    [Fact]
    public void NestedLigaturesRetainMarkAttachmentComponentNumbers()
    {
        var bytes = Program(new Lookup(4, 8, [Ligature(2, (9, [3]))]), new(4, 8, [Ligature(9, (10, [4]))]));
        var definitions = Definitions(ClassRanges((2, 4, 1), (7, 7, 3), (9, 10, 2)));
        using var context = Context(); using var run = Buffer(context, 2, 3, 7, 4);
        using var engine = new OpenTypeSubstitution(bytes, definitions, 64, context);
        engine.ApplyFeature(Latin, 0, Compose, run);
        Assert.Equal(new[] { 10, 7 }, Glyphs(run)); Assert.Equal(3, run[0].ComponentCount);
        Assert.Equal(1, run[1].Component); Assert.Equal(run[0].Ligature, run[1].Ligature);
    }

    [Theory]
    [InlineData(1)] [InlineData(2)] [InlineData(3)]
    public void ChainedContextsMatchReversedBacktrackInputAndLookahead(int format)
    {
        var contextRule = format switch
        {
            1 => ChainedGlyphs(2, [1, 6], [3], [4], (1, 1)),
            2 => ChainedClasses(2, 2, ClassValues(1, 1, 0, 0, 0, 0, 6), ClassRanges((2, 2, 2), (3, 3, 3)),
                ClassRanges((4, 4, 4)), [1, 6], [3], [4], (1, 1)),
            _ => ChainedCoverages([1, 6], [2, 3], [4], (1, 1))
        };
        var bytes = Substitution([new(6, 0, [contextRule]), new(1, 0, [Single([3], [8])])], [new("ccmp", [0])]);
        using var context = Context(); using var run = Buffer(context, 6, 1, 2, 3, 4, 2, 3, 4);
        using var engine = new OpenTypeSubstitution(bytes, default, 64, context);
        engine.ApplyFeature(Latin, 0, Compose, run); Assert.Equal(new[] { 6, 1, 2, 8, 4, 2, 3, 4 }, Glyphs(run));
    }

    [Fact]
    public void ContextActionsIndexTheModifiedSequenceAfterLigatureDeletion()
    {
        var rule = ChainedGlyphs(1, [], [2, 3, 4], [], (1, 1), (2, 2));
        var bytes = Substitution([new(6, 0, [rule]), new(4, 0, [Ligature(2, (9, [3]))]), new(1, 0, [Single([4], [8])])], [new("ccmp", [0])]);
        using var context = Context(); using var run = Buffer(context, 1, 2, 3, 4, 4);
        using var engine = new OpenTypeSubstitution(bytes, default, 64, context);
        engine.ApplyFeature(Latin, 0, Compose, run); Assert.Equal(new[] { 1, 9, 8, 4 }, Glyphs(run));
    }

    [Theory]
    [InlineData(false)] [InlineData(true)]
    public void CoverageSequenceContextMatchesSetsSkipsMarksAndHonorsRunBoundary(bool extension)
    {
        var rule = SequenceCoverages([[2, 5], [3]], (1, 1));
        var bytes = Substitution([new(extension ? 7 : 5, 8, [extension ? Extension(5, rule) : rule]),
            new(1, 0, [Single([3], [8])])], [new("ccmp", [0])]);
        var definitions = Definitions(ClassRanges((2, 5, 1), (7, 7, 3)));
        using var context = Context(); using var run = Buffer(context, 2, 7, 3, 5, 3, 2, 3);
        using var engine = new OpenTypeSubstitution(bytes, definitions, 64, context);
        engine.ApplyFeature(Latin, 0, Compose, run, start: 0, length: 6);
        Assert.Equal(new[] { 2, 7, 8, 5, 8, 2, 3 }, Glyphs(run));
        Assert.Equal(2, run[2].SourceOffset); Assert.Equal(2, run[2].Cluster);
    }

    [Fact]
    public void CoverageSequenceActionsAddressTheResultOfEarlierLigatureActions()
    {
        var rule = SequenceCoverages([[1], [2], [3], [4]], (1, 1), (2, 2));
        var bytes = Substitution([new(5, 0, [rule]), new(4, 0, [Ligature(2, (9, [3]))]),
            new(1, 0, [Single([4], [8])])], [new("ccmp", [0])]);
        using var context = Context(); using var run = Buffer(context, 1, 2, 3, 4, 4);
        using var engine = new OpenTypeSubstitution(bytes, default, 64, context);
        engine.ApplyFeature(Latin, 0, Compose, run); Assert.Equal(new[] { 1, 9, 8, 4 }, Glyphs(run));
    }

    [Fact]
    public void CoverageSequenceEmptyActionsConsumeInputAndStopFallback()
    {
        var noAction = SequenceCoverages([[2], [2]]);
        var action = SequenceCoverages([[2]], (0, 1));
        var bytes = Substitution([new(5, 0, [noAction, action]), new(1, 0, [Single([2], [3])])], [new("ccmp", [0])]);
        using var context = Context(); using var run = Buffer(context, 2, 2, 2);
        using var engine = new OpenTypeSubstitution(bytes, default, 64, context);
        engine.ApplyFeature(Latin, 0, Compose, run); Assert.Equal(new[] { 2, 2, 3 }, Glyphs(run));
    }

    [Fact]
    public void CoverageSequenceMalformedProgramsRetainTypedFailureAndCleanup()
    {
        var good = SequenceCoverages([[2], [3]], (1, 1));
        var programs = new List<byte[]> { Words(3, 0, 0), SequenceCoverages([[2]], (1, 1)),
            SequenceCoverages([[2]], (0, 7)) };
        for (var size = 0; size < good.Length; size++) programs.Add(good[..size]);
        for (var offset = 0; offset < good.Length; offset += 2)
        {
            var mutated = good.ToArray(); Write(mutated, offset, 0xFFFF); programs.Add(mutated);
        }
        using var budget = new TestMemoryBudget(100_000);
        foreach (var sub in programs)
        {
            using (var context = Context(new() { MemoryBudget = budget, MaxWork = 20_000 }))
            using (var run = Buffer(context, 2, 3))
            using (var engine = new OpenTypeSubstitution(Substitution([new(5, 0, [sub]), new(1, 0, [Single([3], [8])])],
                [new("ccmp", [0])]), default, 64, context))
            {
                try { engine.ApplyFeature(Latin, 0, Compose, run); }
                catch (Exception error) { Assert.True(error is ImageDataException or UnsupportedImageException or ImageLimitException, error.ToString()); }
            }
            Assert.Equal(0, budget.Bytes);
        }
        Assert.False(budget.IsDisposed);
    }

    [Fact]
    public void ContextConsumesItsWholeInputAndEmptyActionRulesStopSubtableFallback()
    {
        var noAction = ChainedGlyphs(2, [], [2], [], []);
        var action = ChainedGlyphs(2, [], [], [], (0, 1));
        var bytes = Substitution([new(6, 0, [noAction, action]), new(1, 0, [Single([2], [3])])], [new("ccmp", [0])]);
        using var context = Context(); using var run = Buffer(context, 2, 2, 2);
        using var engine = new OpenTypeSubstitution(bytes, default, 64, context);
        engine.ApplyFeature(Latin, 0, Compose, run); Assert.Equal(new[] { 2, 2, 3 }, Glyphs(run));
    }

    [Theory]
    [InlineData(8, false)] [InlineData(0x100, false)] [InlineData(16, false)] [InlineData(0x110, false)]
    [InlineData(0, true)]
    public void MarkSkippingHonorsGdefClassAttachmentClassFilteringSetsAndUnicodeFallback(int flags, bool fallback)
    {
        var definitions = fallback ? Array.Empty<byte>() : Definitions(ClassValues(2, 1, 1, 0, 0, 0, 3), ClassRanges((7, 7, 2)), [[8]]);
        var bytes = Program(new Lookup(4, fallback ? 8 : flags, [Ligature(2, (9, [3]))]));
        using var context = Context(); using var run = Buffer(context, 2, 7, 3);
        run[1].Scalar = 0x301;
        using var engine = new OpenTypeSubstitution(bytes, definitions, 64, context);
        engine.ApplyFeature(Latin, 0, Compose, run); Assert.Equal(new[] { 9, 7 }, Glyphs(run));
    }

    [Theory]
    [InlineData(2)] [InlineData(4)]
    public void BaseAndLigatureIgnoreFlagsAreAppliedBeforeSubstitution(int flags)
    {
        var definitions = Definitions(ClassRanges((2, 2, flags == 2 ? 1 : 2)));
        var bytes = Program(new Lookup(1, flags, [Single([2], [3])]));
        using var context = Context(); using var run = Buffer(context, 2);
        using var engine = new OpenTypeSubstitution(bytes, definitions, 64, context);
        engine.ApplyFeature(Latin, 0, Compose, run); Assert.Equal(2, run[0].Glyph);
    }

    [Fact]
    public void FilteringSetTakesPrecedenceOverTheAttachmentClassAndCanIncludeAMark()
    {
        var definitions = Definitions(ClassRanges((2, 3, 1), (7, 7, 3)), ClassRanges((7, 7, 2)), [[7]]);
        var bytes = Program(new Lookup(4, 0x110, [Ligature(2, (9, [3]))]));
        using var context = Context(); using var run = Buffer(context, 2, 7, 3);
        using var engine = new OpenTypeSubstitution(bytes, definitions, 64, context);
        engine.ApplyFeature(Latin, 0, Compose, run); Assert.Equal(new[] { 2, 7, 3 }, Glyphs(run));
    }

    [Fact]
    public void IgnoredBasesDoNotTransferTheirMarksToAnUnrelatedLigature()
    {
        var definitions = Definitions(ClassRanges((2, 3, 2), (6, 6, 1), (7, 7, 3)));
        var bytes = Program(new Lookup(4, 2 | 8, [Ligature(2, (9, [3]))]));
        using var context = Context(); using var run = Buffer(context, 2, 6, 7, 3);
        using var engine = new OpenTypeSubstitution(bytes, definitions, 64, context);
        engine.ApplyFeature(Latin, 0, Compose, run); Assert.Equal(new[] { 9, 6, 7 }, Glyphs(run));
        Assert.Equal(0, run[2].Ligature);
    }

    [Fact]
    public void RightToLeftLookupFlagDoesNotReverseLogicalGsubProcessing()
    {
        var bytes = Substitution([new(6, 1, [ChainedGlyphs(2, [], [3], [], (0, 1))]), new(1, 1, [Single([2], [4])])], [new("ccmp", [0])]);
        using var context = Context(); using var run = Buffer(context, 2, 3, 2, 3);
        using var engine = new OpenTypeSubstitution(bytes, default, 64, context);
        engine.ApplyFeature(Latin, 0, Compose, run); Assert.Equal(new[] { 4, 3, 4, 3 }, Glyphs(run));
    }

    [Fact]
    public void LongLigatureScratchIsAdmittedBeforeAllocationAndRefundedAfterRefusal()
    {
        const int count = 70;
        var bytes = Program(new Lookup(4, 0, [Ligature(2, (3, Enumerable.Repeat(2, count - 1).ToArray()))]));
        using var budget = new TestMemoryBudget(64 + count * 96 + 33 + count * 4 + 32 - 1);
        using var context = Context(new() { MemoryBudget = budget });
        using (var run = Buffer(context, Enumerable.Repeat(2, count).ToArray()))
        using (var engine = new OpenTypeSubstitution(bytes, default, 64, context))
        {
            var error = Assert.Throws<ImageLimitException>(() => engine.ApplyFeature(Latin, 0, Compose, run));
            Assert.Equal(ImageLimitScope.SharedBudget, error.Scope); Assert.Equal(1, budget.Refusals);
            Assert.Equal(count, run.Count); Assert.All(Glyphs(run), g => Assert.Equal(2, g));
        }
        Assert.Equal(0, budget.Bytes);
        using var roomy = Context(); using var allowed = Buffer(roomy, Enumerable.Repeat(2, count).ToArray());
        using var accepted = new OpenTypeSubstitution(bytes, default, 64, roomy);
        accepted.ApplyFeature(Latin, 0, Compose, allowed); Assert.Equal(new[] { 3 }, Glyphs(allowed));
        Assert.Equal(count, allowed[0].ComponentCount);
    }

    [Fact]
    public void NeitherIgnoredMarksNorExtensionLookupsCrossTabsAndParagraphBoundaries()
    {
        var bytes = Program(new Lookup(7, 8, [Extension(4, Ligature(2, (9, [3])))]));
        using var context = Context(); using var run = Buffer(context, 2, 0, 3, 2, 0, 3);
        run[1].Kind = 2; run[4].Kind = 3;
        using var engine = new OpenTypeSubstitution(bytes, default, 64, context);
        engine.ApplyFeature(Latin, 0, Compose, run); Assert.Equal(new[] { 2, 0, 3, 2, 0, 3 }, Glyphs(run));
    }

    [Fact]
    public void ExtensionSingleLookupUsesTheChildOffsetWithoutChangingOwnership()
    {
        var bytes = Program(new Lookup(7, 0, [Extension(1, Single([2], [3]))]));
        using var context = Context(); using var run = Buffer(context, 2);
        using var engine = new OpenTypeSubstitution(bytes, default, 64, context);
        engine.ApplyFeature(Latin, 0, Compose, run); Assert.Equal(3, run[0].Glyph);
    }

    [Fact]
    public void RecursiveContextualProgramsStopWithAStructuredUnsupportedFeature()
    {
        var bytes = Program(new Lookup(6, 0, [ChainedGlyphs(2, [], [], [], (0, 0))]));
        using var context = Context(); using var run = Buffer(context, 2);
        using var engine = new OpenTypeSubstitution(bytes, default, 64, context);
        var error = Assert.Throws<UnsupportedImageException>(() => engine.ApplyFeature(Latin, 0, Compose, run));
        Assert.Equal(UnsupportedImageFeature.FontTable, error.Feature);
    }

    [Theory]
    [InlineData(2)] [InlineData(3)] [InlineData(5)] [InlineData(8)]
    public void UnimplementedSelectedLookupFamiliesAreExplicitRatherThanSilentlyIgnored(int type)
    {
        var bytes = Program(new Lookup(type, 0, [Words(1, 0)]));
        using var context = Context(); using var run = Buffer(context, 2);
        using var engine = new OpenTypeSubstitution(bytes, default, 64, context);
        var error = Assert.Throws<UnsupportedImageException>(() => engine.ApplyFeature(Latin, 0, Compose, run));
        Assert.Equal(UnsupportedImageFeature.FontTable, error.Feature);
    }

    [Fact]
    public void MissingFilteringSetsAndOutOfRangeClassValuesAreTypedFontDataErrors()
    {
        foreach (var definitions in new[] { Definitions(ClassRanges((7, 7, 3))), Definitions(ClassRanges((7, 7, 5))) })
        {
            var bytes = Program(new Lookup(1, 16, [Single([7], [8])]));
            using var context = Context(); using var run = Buffer(context, 7);
            using var engine = new OpenTypeSubstitution(bytes, definitions, 64, context);
            Assert.Throws<ImageDataException>(() => engine.ApplyFeature(Latin, 0, Compose, run));
        }
    }

    [Fact]
    public void InvalidGlyphOffsetsContextIndexesAndLookupIndexesFailAsFontData()
    {
        var malformed = new[]
        {
            Program(new Lookup(1, 0, [Single([2], [64])])),
            Program(new Lookup(1, 0, [Words(2, 0, 1, 3)])),
            Program(new Lookup(6, 0, [ChainedGlyphs(2, [], [], [], (1, 0))])),
            Program(new Lookup(6, 0, [ChainedGlyphs(2, [], [], [], (0, 7))])),
            Program(new Lookup(7, 0, [Extension(7, Words(0))])),
            Program(new Lookup(1, 0, [Single([2], [])]))
        };
        foreach (var bytes in malformed)
        {
            using var context = Context(); using var run = Buffer(context, 2);
            using var engine = new OpenTypeSubstitution(bytes, default, 64, context);
            Assert.Throws<ImageDataException>(() => engine.ApplyFeature(Latin, 0, Compose, run));
        }
    }

    [Fact]
    public void AllTruncatedPrefixesAndBoundedMutationsStayWithinTypedFailureContracts()
    {
        var bytes = Substitution([new(6, 0, [ChainedClasses(2, 1, ClassRanges(), ClassValues(2, 1, 2), ClassRanges(), [], [2], [], (1, 1))]),
            new(1, 0, [Single([3], [4])])], [new("ccmp", [0])]);
        for (var size = 0; size < bytes.Length; size++)
        {
            var prefix = bytes.AsMemory(0, size);
            AssertTyped(prefix);
        }
        for (var offset = 0; offset < bytes.Length; offset += 2)
        {
            var mutated = bytes.ToArray(); Write(mutated, offset, 0xFFFF); AssertTyped(mutated);
        }
        static void AssertTyped(ReadOnlyMemory<byte> program)
        {
            using var context = Context(new() { MaxWork = 20_000 }); using var run = Buffer(context, 2, 3);
            try { using var engine = new OpenTypeSubstitution(program, default, 64, context); engine.ApplyFeature(Latin, 0, Compose, run); }
            catch (Exception error) { Assert.True(error is ImageDataException or UnsupportedImageException or ImageLimitException, error.ToString()); }
        }
    }

    [Fact]
    public void SharedAdmissionTracksBufferReplacementPeaksRefusalCleanupAndReuse()
    {
        using var budget = new TestMemoryBudget(64 + 96 + 64 + 4 * 96 - 1);
        using var context = Context(new() { MemoryBudget = budget });
        using (var run = new ShapingBuffer(1, context))
        {
            run.Add(new() { Glyph = 2 });
            var error = Assert.Throws<ImageLimitException>(() => run.Add(new() { Glyph = 3 }));
            Assert.Equal(ImageLimitScope.SharedBudget, error.Scope);
            Assert.Equal(64 + 96, budget.Bytes); Assert.Equal(1, run.Count); Assert.Equal(2, run[0].Glyph);
        }
        Assert.Equal(0, budget.Bytes);
        using (var subsequent = new ShapingBuffer(1, context)) subsequent.Add(new() { Glyph = 4 });
        Assert.Equal(0, budget.Bytes); Assert.False(budget.IsDisposed);
    }

    [Fact]
    public void CancellationWorkAndSelectorRefusalReleaseOwnedStorage()
    {
        var bytes = Program(new Lookup(1, 0, [Single([2], [3])]));
        using var budget = new TestMemoryBudget(100_000); using var cancellation = new CancellationTokenSource();
        using (var context = Context(new() { MemoryBudget = budget }, cancellation.Token))
        {
            using var run = Buffer(context, 2); using var engine = new OpenTypeSubstitution(bytes, default, 64, context);
            cancellation.Cancel();
            var error = Assert.ThrowsAny<OperationCanceledException>(() => engine.ApplyFeature(Latin, 0, Compose, run));
            Assert.Equal(cancellation.Token, error.CancellationToken);
        }
        Assert.Equal(0, budget.Bytes);
        budget.RefuseAtGrowth = budget.GrowthAttempts + 1;
        using (var context = Context(new() { MemoryBudget = budget }))
            Assert.Throws<ImageLimitException>(() => new OpenTypeSubstitution(bytes, default, 64, context));
        Assert.Equal(0, budget.Bytes);
        budget.RefuseAtGrowth = 0;
        using var limited = Context(new() { MemoryBudget = budget, MaxWork = 1 });
        Assert.Throws<ImageLimitException>(() => new OpenTypeSubstitution(bytes, default, 64, limited));
    }
}
