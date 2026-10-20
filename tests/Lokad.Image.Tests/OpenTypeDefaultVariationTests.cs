using System.Buffers.Binary;
using Lokad.Image.Internal;
using Lokad.Image.Tooling;
using static Lokad.Image.Tooling.AuthoredOpenType;

namespace Lokad.Image.Tests;

public sealed class OpenTypeDefaultVariationTests
{
    private static readonly int[][][] Inactive = [[[0, 16384, 16384], [0, 0, 0]], [[0, 0, 0], [-16384, -16384, 0]]];
    private static readonly int[][][] Active = [[[-16384, 0, 16384]], [[0, 0, 0]]];
    private static byte[] Store() => ItemVariationStore(Inactive, 1, [500, -7], [-200, 9]);
    private static OperationContext Context(ImageLimits? limits = null, CancellationToken token = default) => new(limits ?? ImageLimits.Default, token);
    private static byte[] Index(int outer = 0, int inner = 0) => Words(outer, inner, 0x8000);
    private static byte[] Single(byte[] correction) => Attach(Words(1, 0, 0x44, -30, 0), (2, Coverage(2)), (8, correction));
    private static byte[] Program(int type, byte[] sub) => Positioning([new(type, 0, [sub])], [new("kern", [0])]);
    private static ShapingBuffer Buffer(OperationContext context)
    {
        var run = new ShapingBuffer(2, context);
        run.Add(new() { Glyph = 2, Scalar = 'A', Advance = 600 });
        run.Add(new() { Glyph = 3, Scalar = 'V', Advance = 600 }); return run;
    }

    [Theory]
    [InlineData(0)] [InlineData(1)]
    public void DefaultCoordinatesExcludeAnyRegionWithANonzeroPeak(int inner)
    {
        using var context = Context();
        var classes = new OpenTypeGlyphClasses(DefinitionsWithVariation(Store()), 64, context);
        var before = context.RemainingWorkingBytes;
        classes.CheckDefaultVariation(0, inner);
        Assert.Equal(before, context.RemainingWorkingBytes);
    }

    [Theory]
    [InlineData(20, -20, true)] [InlineData(0, 0, true)]
    [InlineData(20, -19, false)] [InlineData(-20, 19, false)]
    public void ActiveRegionsAreAcceptedOnlyWhenTheirSignedWordAndByteSumIsExactlyZero(int word, int small, bool zero)
    {
        using var context = Context();
        var classes = new OpenTypeGlyphClasses(DefinitionsWithVariation(ItemVariationStore(Active, 1, [word, small])), 64, context);
        if (zero) classes.CheckDefaultVariation(0, 0);
        else Assert.Equal(UnsupportedImageFeature.FontTable, Assert.Throws<UnsupportedImageException>(() => classes.CheckDefaultVariation(0, 0)).Feature);
    }

    [Fact]
    public void NoVariationSentinelAndNullOuterEntriesHaveTheirSpecifiedSemantics()
    {
        using var context = Context();
        new OpenTypeGlyphClasses(default, 64, context).CheckDefaultVariation(0xFFFF, 0xFFFF);
        var store = Store(); BinaryPrimitives.WriteUInt32BigEndian(store.AsSpan(8), 0);
        new OpenTypeGlyphClasses(DefinitionsWithVariation(store), 64, context).CheckDefaultVariation(0, 65535);
        var classes = new OpenTypeGlyphClasses(DefinitionsWithVariation(Store()), 64, context);
        Assert.Throws<ImageDataException>(() => classes.CheckDefaultVariation(0xFFFF, 0));
        Assert.Throws<ImageDataException>(() => classes.CheckDefaultVariation(0, 0xFFFF));
    }

    [Theory]
    [InlineData(1)] [InlineData(2)] [InlineData(3)]
    public void ValueCorrectionsUseTheirImmediateSinglePairSetOrClassPairParent(int kind)
    {
        var correction = Index(); byte[] sub;
        if (kind == 1) sub = Single(correction);
        else if (kind == 2)
        {
            var set = Attach(Words(1, 3, -30, 0), (6, correction));
            sub = Attach(Words(1, 0, 0x44, 0, 1, 0), (2, Coverage(2)), (10, set));
        }
        else sub = Attach(Words(2, 0, 0x44, 0, 0, 0, 1, 1, -30, 0),
            (2, Coverage(2)), (8, ClassRanges()), (10, ClassRanges()), (18, correction));
        using var context = Context(); using var run = Buffer(context);
        using var engine = new OpenTypePositioning(Program(kind == 1 ? 1 : 2, sub), DefinitionsWithVariation(Store()), 64, context);
        engine.ApplyStandardFeatures(Tag("latn"), 0, run);
        Assert.Equal(570, run[0].Advance); Assert.Equal(600, run[1].Advance);
    }

    [Fact]
    public void AnchorCorrectionsAreRelativeToTheAnchorRatherThanTheAttachmentTable()
    {
        var parent = Attach(Words(3, 300, 600, 0, 0), (6, Index()));
        var mark = Attach(Words(3, 20, 40, 0, 0), (8, Index(0, 1)));
        using var context = Context(); using var run = Buffer(context);
        using var engine = new OpenTypePositioning(Program(4, MarkPosition(2, 3, parent, mark)),
            DefinitionsWithVariation(Store(), ClassRanges((2, 2, 1), (3, 3, 3))), 64, context);
        engine.ApplyFeature(Tag("latn"), 0, Tag("kern"), run); engine.ResolveAttachments(run);
        Assert.Equal((-320d, 560d), (run[1].OffsetX, run[1].OffsetY));
    }

    [Fact]
    public void RealDevicesRemainUnsupportedAndMalformedOrMissingReferencesFailAsData()
    {
        using var context = Context(); using var run = Buffer(context);
        using (var engine = new OpenTypePositioning(Program(1, Single(Words(10, 20, 1, 0))), default, 64, context))
            Assert.Throws<UnsupportedImageException>(() => engine.ApplyStandardFeatures(Tag("latn"), 0, run));
        using (var engine = new OpenTypePositioning(Program(1, Single(Index())), default, 64, context))
            Assert.Throws<ImageDataException>(() => engine.ApplyStandardFeatures(Tag("latn"), 0, run));
        var classes = new OpenTypeGlyphClasses(DefinitionsWithVariation(Store()), 64, context);
        Assert.Throws<ImageDataException>(() => classes.CheckDefaultVariation(1, 0));
        Assert.Throws<ImageDataException>(() => classes.CheckDefaultVariation(0, 2));
    }

    [Fact]
    public void InvalidRegionsCannotBeHiddenByAnEarlierInactiveAxis()
    {
        var regions = new[] { new[] { new[] { 0, 16384, 16384 }, new[] { 0, 20000, 20000 } } };
        using var context = Context();
        var classes = new OpenTypeGlyphClasses(DefinitionsWithVariation(ItemVariationStore(regions, 1, [3])), 64, context);
        Assert.Throws<ImageDataException>(() => classes.CheckDefaultVariation(0, 0));
        var crossing = new[] { new[] { new[] { -100, 20, 100 } } };
        classes = new OpenTypeGlyphClasses(DefinitionsWithVariation(ItemVariationStore(crossing, 1, [3])), 64, context);
        Assert.Throws<ImageDataException>(() => classes.CheckDefaultVariation(0, 0));
    }

    [Fact]
    public void TruncationsAndBoundedMutationsPreserveTypedFailuresAndRefundOwnership()
    {
        var valid = DefinitionsWithVariation(Store()); using var budget = new TestMemoryBudget(100_000);
        var inputs = Enumerable.Range(0, valid.Length).Select(n => valid[..n]).ToList();
        for (var i = 0; i < valid.Length; i += 2) { var mutated = valid.ToArray(); Write(mutated, i, 65535); inputs.Add(mutated); }
        foreach (var bytes in inputs)
        {
            try
            {
                using var context = Context(new() { MemoryBudget = budget, MaxWork = 20_000 });
                using var run = Buffer(context);
                using var engine = new OpenTypePositioning(Program(1, Single(Index())), bytes, 64, context);
                engine.ApplyStandardFeatures(Tag("latn"), 0, run);
            }
            catch (Exception e) { Assert.True(e is ImageDataException or UnsupportedImageException or ImageLimitException, e.ToString()); }
            Assert.Equal(0, budget.Bytes);
        }
    }

    [Fact]
    public void RegionWorkAndItemLimitsCancellationRefusalAndSubsequentReuseStayIndependent()
    {
        var definitions = DefinitionsWithVariation(Store()); using var budget = new TestMemoryBudget(100_000);
        using (var context = Context(new() { MemoryBudget = budget, MaxContainerItems = 3 }))
            Assert.Throws<ImageLimitException>(() => new OpenTypeGlyphClasses(definitions, 64, context).CheckDefaultVariation(0, 0));
        using (var context = Context(new() { MemoryBudget = budget, MaxWork = 20 }))
            Assert.Throws<ImageLimitException>(() => new OpenTypeGlyphClasses(definitions, 64, context).CheckDefaultVariation(0, 0));
        using var cancel = new CancellationTokenSource();
        using (var context = Context(new() { MemoryBudget = budget }, cancel.Token))
        {
            var classes = new OpenTypeGlyphClasses(definitions, 64, context); cancel.Cancel();
            Assert.ThrowsAny<OperationCanceledException>(() => classes.CheckDefaultVariation(0, 0));
        }
        budget.RefuseAtGrowth = budget.GrowthAttempts + 1;
        using (var context = Context(new() { MemoryBudget = budget })) Assert.Throws<ImageLimitException>(() => Buffer(context));
        Assert.Equal(0, budget.Bytes); budget.RefuseAtGrowth = 0;
        using (var context = Context(new() { MemoryBudget = budget }))
        using (var run = Buffer(context))
        using (var engine = new OpenTypePositioning(Program(1, Single(Index())), definitions, 64, context))
        {
            var before = budget.Bytes; engine.ApplyStandardFeatures(Tag("latn"), 0, run); Assert.Equal(before, budget.Bytes);
        }
        Assert.Equal(0, budget.Bytes); Assert.False(budget.IsDisposed);
    }
}
