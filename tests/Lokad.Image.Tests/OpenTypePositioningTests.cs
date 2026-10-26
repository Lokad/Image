using System.Runtime.InteropServices;
using Lokad.Image.Internal;
using Lokad.Image.Tooling;
using static Lokad.Image.Tooling.AuthoredOpenType;

namespace Lokad.Image.Tests;

public sealed class OpenTypePositioningTests
{
    private static readonly uint Latin = Tag("latn"), Mark = Tag("mark");
    private static byte[] Program(params Lookup[] lookups) => Positioning(lookups, [new("mark", Enumerable.Range(0, lookups.Length).ToArray())]);
    private static OperationContext Context(ImageLimits? limits = null, CancellationToken cancellation = default) => new(limits ?? ImageLimits.Default, cancellation);
    private static ShapingBuffer Buffer(OperationContext context, params (int Glyph, double Advance)[] glyphs)
    {
        var buffer = new ShapingBuffer(glyphs.Length, context);
        foreach (var (glyph, advance) in glyphs) buffer.Add(new() { Glyph = glyph, Advance = advance, Scalar = 65, SourceOffset = buffer.Count, SourceLength = 1, Features = uint.MaxValue });
        return buffer;
    }
    private static byte[] Classes() => Definitions(ClassRanges((2, 3, 1), (7, 8, 3), (9, 9, 2)));

    [Fact]
    public void CoarseBufferCapacityCoversTheActualRecordLayout()
        => Assert.InRange(Marshal.SizeOf<ShapingGlyph>(), 1, 96);

    [Fact]
    public void StandardFeaturesAreUnionedAndAppliedInLookupOrderRatherThanTagOrder()
    {
        var bytes = Positioning([new(1, 0, [SinglePosition(7, 1, 50)]),
            new(4, 0, [MarkPosition(2, 7, Anchor(100, 600), Anchor(20, 0))]),
            new(1, 0, [SinglePosition(2, 1, 5)])], [new("kern", [2]), new("mark", [1]), new("mkmk", [0])]);
        using var context = Context(); using var run = Buffer(context, (2, 600), (7, 0));
        using var engine = new OpenTypePositioning(bytes, Classes(), 64, context);
        engine.ApplyStandardFeatures(Latin, 0, run); engine.ResolveAttachments(run);
        Assert.Equal(-515, run[1].OffsetX);
    }

    [Fact]
    public void SingleAdjustmentsAccumulatePlacementAndHorizontalAdvance()
    {
        var bytes = Program(new(1, 0, [SinglePosition(2, 15, 11, -12, 13, 999)]), new(1, 0, [SinglePositionArray([2, 3], 3, [4, 5], [-6, 7])]));
        using var context = Context(); using var run = Buffer(context, (2, 600), (3, 700));
        using var engine = new OpenTypePositioning(bytes, default, 64, context);
        engine.ApplyFeature(Latin, 0, Mark, run);
        Assert.Equal((15d, -7d, 613d), (run[0].OffsetX, run[0].OffsetY, run[0].Advance));
        Assert.Equal((-6d, 7d, 700d), (run[1].OffsetX, run[1].OffsetY, run[1].Advance));
    }

    [Fact]
    public void ExplicitPairsWithZeroSecondFormatAllowOverlappingPairKerning()
    {
        var bytes = Program(new Lookup(2, 0, [PairPosition(2, 4, 0, (2, [-30], []), (3, [-70], []))]));
        using var context = Context(); using var run = Buffer(context, (2, 600), (2, 600), (3, 600));
        using var engine = new OpenTypePositioning(bytes, default, 64, context);
        engine.ApplyFeature(Latin, 0, Mark, run);
        Assert.Equal(new[] { 570d, 530, 600 }, run.Span.ToArray().Select(g => g.Advance));
    }

    [Fact]
    public void NonzeroSecondFormatConsumesThePairEvenWhenItsValuesAreZero()
    {
        var bytes = Program(new Lookup(2, 0, [PairPosition(2, 4, 1, (2, [-30], [0]), (3, [-70], [0]))]));
        using var context = Context(); using var run = Buffer(context, (2, 600), (2, 600), (3, 600));
        using var engine = new OpenTypePositioning(bytes, default, 64, context);
        engine.ApplyFeature(Latin, 0, Mark, run);
        Assert.Equal(new[] { 570d, 600, 600 }, run.Span.ToArray().Select(g => g.Advance));
    }

    [Fact]
    public void ClassPairsIncludeClassZeroAndIgnoreMarksForFindingTheSecondGlyph()
    {
        var pairs = ClassPairPosition(2, ClassValues(2, 1), ClassRanges((3, 3, 1)), 2, 2, 4, 1,
            [0, 0], [0, 0], [-10, 5], [-80, 7]);
        var bytes = Program(new Lookup(2, 8, [pairs]));
        using var context = Context(); using var run = Buffer(context, (2, 600), (7, 0), (3, 700), (2, 600), (4, 500));
        using var engine = new OpenTypePositioning(bytes, Classes(), 64, context);
        engine.ApplyFeature(Latin, 0, Mark, run);
        Assert.Equal(520, run[0].Advance); Assert.Equal(7, run[2].OffsetX);
        Assert.Equal(590, run[3].Advance); Assert.Equal(5, run[4].OffsetX);
    }

    [Theory]
    [InlineData(1)] [InlineData(2)] [InlineData(3)]
    public void MarkToBaseAlignsUnhintedAnchorsAndKeepsAdvanceUnchanged(int anchorFormat)
    {
        var bytes = Program(new Lookup(4, 0, [MarkPosition(2, 7, Anchor(280, 680, anchorFormat), Anchor(20, 40, anchorFormat))]));
        using var context = Context(); using var run = Buffer(context, (2, 600), (7, 0));
        using var engine = new OpenTypePositioning(bytes, Classes(), 64, context);
        engine.ApplyFeature(Latin, 0, Mark, run); Assert.Equal(1, run[1].AttachTo);
        engine.ResolveAttachments(run);
        Assert.Equal((-340d, 640d, 0d), (run[1].OffsetX, run[1].OffsetY, run[1].Advance));
        engine.ResolveAttachments(run); Assert.Equal(-340, run[1].OffsetX);
    }

    [Fact]
    public void AnchorCoordinatesRespectTheDecodedGlyphOrigin()
    {
        var bytes = Program(new Lookup(4, 0, [MarkPosition(2, 7, Anchor(280, 680), Anchor(20, 40))]));
        using var context = Context(); using var run = Buffer(context, (2, 600), (7, 0));
        run[0].OriginX = 10; run[1].OriginX = -20;
        using var engine = new OpenTypePositioning(bytes, Classes(), 64, context);
        engine.ApplyFeature(Latin, 0, Mark, run); engine.ResolveAttachments(run);
        Assert.Equal(-370, run[1].OffsetX);
    }

    [Fact]
    public void NonzeroMarkClassesSelectTheCorrectAnchorColumn()
    {
        var bytes = Program(new Lookup(4, 0, [MarkPosition(2, 7, Anchor(280, 680), Anchor(20, 40), markClass: 2, classCount: 3)]));
        using var context = Context(); using var run = Buffer(context, (2, 600), (7, 0));
        using var engine = new OpenTypePositioning(bytes, Classes(), 64, context);
        engine.ApplyFeature(Latin, 0, Mark, run); engine.ResolveAttachments(run);
        Assert.Equal((-340d, 640d), (run[1].OffsetX, run[1].OffsetY));
    }

    [Fact]
    public void RightToLeftResolutionUsesTheActualVisualOrigins()
    {
        var bytes = Program(new Lookup(4, 1, [MarkPosition(2, 7, Anchor(280, 680), Anchor(20, 40))]));
        using var context = Context(); using var run = Buffer(context, (2, 600), (7, 0));
        using var engine = new OpenTypePositioning(bytes, Classes(), 64, context);
        engine.ApplyFeature(Latin, 0, Mark, run); engine.ResolveAttachments(run, rightToLeft: true);
        Assert.Equal((260d, 640d), (run[1].OffsetX, run[1].OffsetY));
    }

    [Theory]
    [InlineData(true, 0, -740, 630)] [InlineData(true, 1, -440, 620)]
    [InlineData(false, 0, -440, 620)] [InlineData(true, 9, -440, 620)]
    public void LigatureMarksUseTheirComponentOrTheLastComponent(bool associated, int component, int expectedX, int expectedY)
    {
        var bytes = Program(new Lookup(5, 0, [LigatureMarkPosition(9, 7, Anchor(40, 20), Anchor(200, 650), Anchor(500, 640))]));
        using var context = Context(); using var run = Buffer(context, (9, 900), (7, 0));
        run[0].Ligature = 13; run[0].ComponentCount = 2; run[1].Ligature = associated ? 13 : 0; run[1].Component = component;
        using var engine = new OpenTypePositioning(bytes, Classes(), 64, context);
        engine.ApplyFeature(Latin, 0, Mark, run); engine.ResolveAttachments(run);
        Assert.Equal(((double)expectedX, (double)expectedY), (run[1].OffsetX, run[1].OffsetY));
    }

    [Fact]
    public void MarkChainsFollowLateParentMovementAndAdvanceChanges()
    {
        var bytes = Program(new(4, 0, [MarkPosition(2, 7, Anchor(100, 600), Anchor(20, 0))]),
            new(6, 0, [MarkPosition(7, 8, Anchor(40, 160), Anchor(10, 20))]),
            new(1, 0, [SinglePosition(2, 7, 12, -8, 50)]));
        using var context = Context(); using var run = Buffer(context, (2, 600), (7, 0), (8, 0));
        using var engine = new OpenTypePositioning(bytes, Classes(), 64, context);
        engine.ApplyFeature(Latin, 0, Mark, run); engine.ResolveAttachments(run);
        Assert.Equal((650d, 12d, -8d), (run[0].Advance, run[0].OffsetX, run[0].OffsetY));
        Assert.Equal((-558d, 592d), (run[1].OffsetX, run[1].OffsetY));
        Assert.Equal((-528d, 732d), (run[2].OffsetX, run[2].OffsetY));
    }

    [Fact]
    public void AnchorsOverrideEarlierMarkPlacementAndLaterValuesAddToTheAttachment()
    {
        var bytes = Program(new(1, 0, [SinglePosition(7, 3, 50, 70)]), new(4, 0, [MarkPosition(2, 7, Anchor(100, 600), Anchor(20, 0))]),
            new(1, 0, [SinglePosition(7, 3, 5, 6)]));
        using var context = Context(); using var run = Buffer(context, (2, 600), (7, 0));
        using var engine = new OpenTypePositioning(bytes, Classes(), 64, context);
        engine.ApplyFeature(Latin, 0, Mark, run); engine.ResolveAttachments(run);
        Assert.Equal((-515d, 606d), (run[1].OffsetX, run[1].OffsetY));
    }

    [Fact]
    public void NullParentAnchorsAllowASubtableFallbackWithoutMovingTheMark()
    {
        var bytes = Program(new Lookup(4, 0, [MarkPosition(2, 7, null, Anchor(20, 0)), MarkPosition(2, 7, Anchor(100, 600), Anchor(20, 0))]));
        using var context = Context(); using var run = Buffer(context, (2, 600), (7, 0));
        using var engine = new OpenTypePositioning(bytes, Classes(), 64, context);
        engine.ApplyFeature(Latin, 0, Mark, run); engine.ResolveAttachments(run); Assert.Equal(-520, run[1].OffsetX);
    }

    [Fact]
    public void ACloserUncoveredBaseOrLigatureStopsParentSearch()
    {
        var bytes = Program(new Lookup(4, 4, [MarkPosition(2, 7, Anchor(100, 600), Anchor(20, 0))]));
        using var context = Context(); using var run = Buffer(context, (2, 600), (9, 900), (7, 0), (2, 600), (3, 600), (7, 0));
        using var engine = new OpenTypePositioning(bytes, Classes(), 64, context);
        engine.ApplyFeature(Latin, 0, Mark, run); Assert.Equal(0, run[2].AttachTo); Assert.Equal(0, run[5].AttachTo);
    }

    [Fact]
    public void MarkToMarkDoesNotCrossLigatureComponentsOrUnrelatedLigatures()
    {
        var bytes = Program(new Lookup(6, 0, [MarkPosition(7, 8, Anchor(40, 160), Anchor(10, 20))]));
        using var context = Context(); using var engine = new OpenTypePositioning(bytes, Classes(), 64, context);
        using var run = Buffer(context, (7, 0), (8, 0)); run[0].Ligature = 1; run[1].Ligature = 2;
        engine.ApplyFeature(Latin, 0, Mark, run); Assert.Equal(0, run[1].AttachTo);
        run[1].Ligature = 1; run[1].Component = 1;
        engine.ApplyFeature(Latin, 0, Mark, run); Assert.Equal(0, run[1].AttachTo);
        run[0].Component = 1; engine.ApplyFeature(Latin, 0, Mark, run); Assert.Equal(1, run[1].AttachTo);
    }

    [Fact]
    public void TabsParagraphsAndSelectedRunLimitsPreventAttachmentAcrossBoundaries()
    {
        var bytes = Program(new Lookup(9, 0, [Extension(4, MarkPosition(2, 7, Anchor(100, 600), Anchor(20, 0)))]));
        using var context = Context(); using var run = Buffer(context, (2, 600), (0, 0), (7, 0), (2, 600), (0, 0), (7, 0), (2, 600), (7, 0));
        run[1].Kind = 2; run[4].Kind = 3;
        using var engine = new OpenTypePositioning(bytes, Classes(), 64, context);
        engine.ApplyFeature(Latin, 0, Mark, run, start: 0, length: 6);
        Assert.All(run.Span.ToArray(), g => Assert.Equal(0, g.AttachTo));
        engine.ApplyFeature(Latin, 0, Mark, run, start: 7, length: 1); Assert.Equal(0, run[7].AttachTo);
        engine.ApplyFeature(Latin, 0, Mark, run, start: 6, length: 2); Assert.Equal(7, run[7].AttachTo);
        engine.ResolveAttachments(run, start: 6, length: 2); Assert.Equal(-520, run[7].OffsetX);
    }

    [Fact]
    public void CyclicOrForeignRunAttachmentReferencesAreTypedDataErrors()
    {
        using var context = Context(); using var run = Buffer(context, (2, 600), (7, 0));
        using var engine = new OpenTypePositioning(default, default, 64, context);
        run[1].AttachTo = 2;
        Assert.Throws<ImageDataException>(() => engine.ResolveAttachments(run));
        run[1].AttachTo = 1;
        Assert.Throws<ImageDataException>(() => engine.ResolveAttachments(run, start: 1, length: 1));
    }

    [Theory]
    [InlineData(3)] [InlineData(7)] [InlineData(8)]
    public void SelectedUnsupportedFamiliesRemainExplicit(int type)
    {
        using var context = Context(); using var run = Buffer(context, (2, 600));
        using var engine = new OpenTypePositioning(Program(new Lookup(type, 0, [Words(1, 0)])), default, 64, context);
        var error = Assert.Throws<UnsupportedImageException>(() => engine.ApplyFeature(Latin, 0, Mark, run));
        Assert.Equal(UnsupportedImageFeature.FontTable, error.Feature);
    }

    [Fact]
    public void ReservedValuesAndNonzeroDeviceCorrectionsAreNotSilentlyIgnored()
    {
        var deviceAnchor = Attach(Words(3, 100, 200, 0, 0), (6, Words(10, 20, 1, 0)));
        var deviceValue = Attach(Words(1, 0, 17, 20, 0), (2, Coverage(2)), (8, Words(10, 20, 1, 0)));
        foreach (var lookup in new[] { new Lookup(1, 0, [SinglePosition(2, 256)]), new Lookup(1, 0, [deviceValue]),
            new Lookup(4, 0, [MarkPosition(2, 7, deviceAnchor, Anchor(20, 0))]) })
        {
            using var context = Context(); using var run = Buffer(context, (2, 600), (7, 0));
            using var engine = new OpenTypePositioning(Program(lookup), Classes(), 64, context);
            Assert.Throws<UnsupportedImageException>(() => engine.ApplyFeature(Latin, 0, Mark, run));
        }
    }

    [Fact]
    public void AllTruncatedPrefixesAndBoundedMutationsPreserveTypedFailures()
    {
        var programs = new[]
        {
            Program(new Lookup(4, 0, [MarkPosition(2, 7, Anchor(100, 600), Anchor(20, 0))])),
            Program(new Lookup(2, 0, [PairPosition(2, 4, 1, (7, [-40], [10]))])),
            Program(new Lookup(2, 0, [ClassPairPosition(2, ClassValues(2, 1), ClassRanges((7, 7, 1)), 2, 2, 4, 0, [0], [0], [0], [-70])])),
            Program(new Lookup(5, 0, [LigatureMarkPosition(2, 7, Anchor(20, 0), Anchor(100, 600), Anchor(300, 700))]))
        };
        foreach (var bytes in programs)
        {
            for (var size = 0; size < bytes.Length; size++) Check(bytes.AsMemory(0, size));
            for (var offset = 0; offset < bytes.Length; offset += 2) { var changed = bytes.ToArray(); Write(changed, offset, 65535); Check(changed); }
        }
        static void Check(ReadOnlyMemory<byte> bytes)
        {
            using var context = Context(new() { }); using var run = Buffer(context, (2, 600), (7, 0));
            try { using var engine = new OpenTypePositioning(bytes, Classes(), 64, context); engine.ApplyFeature(Latin, 0, Mark, run); engine.ResolveAttachments(run); }
            catch (Exception e) { Assert.True(e is ImageDataException or ImageLimitException or UnsupportedImageException, e.ToString()); }
        }
    }

    [Fact]
    public void AttachmentResolutionAdmissionIsTerminalAndReleasesEveryTemporaryOwner()
    {
        using var budget = new TestMemoryBudget(1_000_000);
        var bytes = Program(new Lookup(4, 0, [MarkPosition(2, 7, Anchor(100, 600), Anchor(20, 0))]));
        using (var context = Context(new() { MemoryBudget = budget }))
        using (var run = Buffer(context, (2, 600), (7, 0)))
        using (var engine = new OpenTypePositioning(bytes, Classes(), 64, context))
        {
            engine.ApplyFeature(Latin, 0, Mark, run); var baseline = budget.Bytes;
            budget.RefuseAtGrowth = budget.GrowthAttempts + 1;
            var error = Assert.Throws<ImageLimitException>(() => engine.ResolveAttachments(run));
            Assert.Equal(ImageLimitScope.SharedBudget, error.Scope); Assert.Equal(1, budget.Refusals); Assert.Equal(baseline, budget.Bytes);
        }
        Assert.Equal(0, budget.Bytes); budget.RefuseAtGrowth = 0;
        using (var context = Context(new() { MemoryBudget = budget }))
        using (var run = Buffer(context, (2, 600), (7, 0)))
        using (var engine = new OpenTypePositioning(bytes, Classes(), 64, context))
        {
            engine.ApplyFeature(Latin, 0, Mark, run); engine.ResolveAttachments(run); Assert.Equal(-520, run[1].OffsetX);
        }
        Assert.Equal(0, budget.Bytes); Assert.False(budget.IsDisposed);
    }

    [Theory]
     [InlineData(true)]
    public void FailuresAfterResolutionWorkspaceAdmissionRefundTheWorkspace(bool cancel)
    {
        using var budget = new TestMemoryBudget(1_000_000); using var cancellation = new CancellationTokenSource();
        using (var context = Context(new() { MemoryBudget = budget}, cancellation.Token))
        using (var run = Buffer(context, (2, 600), (7, 0)))
        using (var engine = new OpenTypePositioning(default, default, 64, context))
        {
            run[1].AttachTo = 1; var baseline = budget.Bytes;
            if (cancel) budget.Admitted = bytes => { if (bytes > baseline) cancellation.Cancel(); };
            if (cancel)
            {
                var error = Assert.ThrowsAny<OperationCanceledException>(() => engine.ResolveAttachments(run));
                Assert.Equal(cancellation.Token, error.CancellationToken);
            }
            Assert.Equal(baseline, budget.Bytes); Assert.True(budget.Peak > baseline);
        }
        Assert.Equal(0, budget.Bytes); budget.Admitted = null;
        using (var fresh = Context(new() { MemoryBudget = budget }))
        using (var buffer = Buffer(fresh, (2, 600))) Assert.Equal(160, budget.Bytes);
        Assert.Equal(0, budget.Bytes);
    }
}
