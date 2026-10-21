using System.Reflection;
using System.Runtime.ExceptionServices;
using Lokad.Image.Internal;
using Lokad.Image.Tooling;

namespace Lokad.Image.Tests;

public sealed class WebpColorRefitTests
{
    private static object Call(string name, params object?[] args)
    {
        try { return typeof(WebpLosslessEncoder).GetMethod(name, BindingFlags.Static | BindingFlags.NonPublic)!.Invoke(null, args)!; }
        catch (TargetInvocationException error) when (error.InnerException is not null)
        { ExceptionDispatchInfo.Capture(error.InnerException).Throw(); throw; }
    }
    private static object Get(object value, string name) => value.GetType().GetProperty(name)!.GetValue(value)!;
    private static object Prime(ImageBuffer source, uint[] cache, OperationContext context, bool codeCost = false)
    {
        var plan = Call("SelectCachedPlan", source, 4, cache, context);
        plan = Call("SelectNoCachePlan", source, 4, cache, plan, context);
        plan = Call("SelectCodeCostPlan", source, 4, cache, plan, context);
        plan = Call("SelectCodeCostPlan", source, 4, cache, plan, context);
        if (codeCost) plan = Call("SelectColorPlan", source, 4, cache, plan, context);
        Assert.False((bool)Get(plan, "UseCache")); return plan;
    }
    private static string Method(bool codeCost) => codeCost ? "SelectCodeCostColorPlan" : "SelectColorPlan";
    private static long Tables(object value)
    {
        var tables = (WebpPrefixCode[])value;
        return tables.Length * 8L + 32 + tables.Sum(t => t.MemoryBytes);
    }
    private static long Storage(object plan)
    {
        var groups = Get(plan, "Groups"); var color = Get(plan, "Color");
        return Tables(Get(plan, "Tables")) + ((byte[])Get(plan, "Modes")).Length + 32L
            + ((WebpPrefixCode)Get(plan, "ModeTable")).MemoryBytes
            + (groups is null ? 0 : ((byte[])Get(groups, "Map")).Length + 80L + ((WebpPrefixCode)Get(groups, "MapTable")).MemoryBytes)
            + ((byte[])Get(color, "Coefficients")).Length + 64L + Tables(Get(color, "Tables"));
    }

    [Theory] [InlineData(false)] [InlineData(true)]
    public void ColorReplacementPreservesCacheModeAndExactlyOneBorrowedPredictorOwner(bool codeCost)
    {
        using var root = new TestMemoryBudget(32L * 1024 * 1024);
        using var source = WebpAdaptivePredictorTests.Photograph(8, root); var held = root.Bytes;
        using var context = new OperationContext(new() { MemoryBudget = root }, default);
        var cache = new uint[1024]; var baseline = Prime(source, cache, context, codeCost); var before = root.Bytes;
        var firstGrowth = 0L; root.Admitted = bytes => { if (firstGrowth == 0) firstGrowth = bytes; };
        var selected = Call(Method(codeCost), source, 4, cache, baseline, context); root.Admitted = null;
        Assert.NotSame(Get(baseline, "Color"), Get(selected, "Color"));
        Assert.Same(Get(baseline, "Modes"), Get(selected, "Modes"));
        Assert.Same(Get(baseline, "ModeTable"), Get(selected, "ModeTable")); Assert.False((bool)Get(selected, "UseCache"));
        Assert.Equal(before + ((byte[])Get(Get(selected, "Color"), "Coefficients")).Length + 64L, firstGrowth);
        Assert.Equal(held + Storage(selected), root.Bytes);
        if (!codeCost)
        {
            // This fitter repeats the same entropy calculation. Its rejected
            // alternative exercises the shared candidate-release backend.
            var bytes = root.Bytes; var rejected = Call("SelectColorPlan", source, 4, cache, selected, context);
            Assert.Same(Get(selected, "Color"), Get(rejected, "Color"));
            Assert.Same(Get(selected, "Modes"), Get(rejected, "Modes")); Assert.Equal(bytes, root.Bytes);
        }
        Call("ReleasePlan", selected, context); Assert.Equal(held, root.Bytes); Assert.False(root.IsDisposed);
    }

    [Theory] [InlineData(false, false)] [InlineData(true, false)]
    [InlineData(false, true)] [InlineData(true, true)]
    public void LocalMemoryOrWorkGuardSkipsBeforeSharedAdmission(bool work, bool codeCost)
    {
        using var root = new TestMemoryBudget(32L * 1024 * 1024);
        using var source = WebpAdaptivePredictorTests.Photograph(8, root);
        using var context = new OperationContext(new() { MemoryBudget = root }, default);
        var cache = new uint[1024]; var baseline = Prime(source, cache, context, codeCost); var borrowed = 0L;
        if (work) context.Work(context.RemainingWork - 1);
        else { borrowed = context.RemainingWorkingBytes - 1; context.ReserveBorrowed(borrowed); }
        var bytes = root.Bytes; var attempts = root.Attempts;
        Assert.Equal(baseline, Call(Method(codeCost), source, 4, cache, baseline, context));
        Assert.Equal(bytes, root.Bytes); Assert.Equal(attempts, root.Attempts);
        if (!work) context.ReleaseBorrowed(borrowed);
    }

    [Theory] [InlineData(0, false, false)] [InlineData(1, false, false)] [InlineData(2, false, false)]
    [InlineData(0, true, false)] [InlineData(1, true, false)] [InlineData(2, true, false)]
    [InlineData(0, false, true)] [InlineData(1, false, true)] [InlineData(2, false, true)]
    [InlineData(0, true, true)] [InlineData(1, true, true)] [InlineData(2, true, true)]
    public void InitialAndRefitFailuresAreTerminalAndReleaseReplacedAndBorrowedOwners(int failure, bool late, bool codeCost)
    {
        using var root = new TestMemoryBudget(32L * 1024 * 1024);
        using var source = WebpAdaptivePredictorTests.Photograph(8, root); var held = root.Bytes; var reached = false;
        using (var cancellation = new CancellationTokenSource())
        using (var context = new OperationContext(new() { MemoryBudget = root }, cancellation.Token))
        {
            var cache = new uint[1024]; var baseline = Prime(source, cache, context, codeCost); var before = root.Bytes;
            if (failure == 0 && !late) { root.RefuseAtGrowth = root.GrowthAttempts + 1; reached = true; }
            else root.Admitted = bytes =>
            {
                if (late && bytes <= before + 1_000_000) return;
                reached = true; root.Admitted = null;
                if (failure == 0) root.RefuseAtGrowth = root.GrowthAttempts + 1;
                else if (failure == 1) cancellation.Cancel();
                else context.Work(context.RemainingWork);
            };
            if (failure == 1) Assert.Throws<OperationCanceledException>(() => Call(Method(codeCost), source, 4, cache, baseline, context));
            else
            {
                var error = Assert.Throws<ImageLimitException>(() => Call(Method(codeCost), source, 4, cache, baseline, context));
                Assert.Equal(failure == 0 ? ImageLimitKind.WorkingBytes : ImageLimitKind.Work, error.Limit);
                Assert.Equal(failure == 0 ? ImageLimitScope.SharedBudget : ImageLimitScope.Operation, error.Scope);
            }
            Assert.True(reached);
        }
        Assert.Equal(held, root.Bytes); root.RefuseAtGrowth = 0; root.Admitted = null;
        using var recovered = new MemoryStream();
        ImageCodec.EncodeWebp(source, recovered, new() { Limits = new() { MemoryBudget = root } });
        Assert.InRange(recovered.Length, 1, 160032); Assert.Equal(held, root.Bytes); Assert.False(root.IsDisposed);
    }

    [Theory] [InlineData(false)] [InlineData(true)]
    public void RefitPreservesVariableAlphaAndHiddenRgbWithIndependentOutput(bool codeCost)
    {
        using var root = new TestMemoryBudget(32L * 1024 * 1024);
        using var source = WebpAdaptivePredictorTests.Photograph(8, root);
        using (var context = new OperationContext(new() { MemoryBudget = root }, default))
        {
            var cache = new uint[1024]; var old = Prime(source, cache, context, codeCost);
            for (var y = 0; y < source.Height; y++)
            for (var x = 0; x < source.Width; x++) source.SetSample(x, y, 3, (x + 3 * y) % 11 == 0 ? 0 : (x * 13 + y * 37) & 255);
            var expected = source.Pixels.ToArray();
            var baseline = Call("PrepareCachedTables", source, 4, cache, true, context,
                Get(old, "Modes"), Get(old, "ModeTable"), Get(old, "Color"), false);
            baseline.GetType().GetProperty("Alpha")!.SetValue(baseline, true);
            baseline = Call("SelectEntropyPlan", source, 4, cache, baseline, context); Call("ReleaseCodingPlan", old, context);
            var work = context.ConsumedWork; var selected = Call(Method(codeCost), source, 4, cache, baseline, context);
            Assert.True(context.ConsumedWork > work); Assert.False((bool)Get(selected, "UseCache"));
            Assert.Same(Get(baseline, "Modes"), Get(selected, "Modes"));
            using var output = new MemoryStream(); Call("CachedPayload", source, output, 4, cache, selected, context);
            var packet = output.ToArray(); source.Dispose();
            using var decoding = new OperationContext(ImageLimits.Default, default);
            using var decoded = new WebpLosslessDecoder(packet, 513, 259, decoding).Decode();
            packet.AsSpan().Clear(); Assert.Equal(expected, decoded.Pixels.ToArray());
        }
        Assert.Equal(0, root.Bytes); Assert.False(root.IsDisposed);
    }

    [Theory] [InlineData(false)] [InlineData(true)]
    public void LowEffortAndUnmappedCachedPlansSkipBeforeAnyWorkOrSharedAttempt(bool unsuitable)
    {
        using var root = new TestMemoryBudget(32L * 1024 * 1024);
        using var source = unsuitable ? ImageBuffer.Create(17, 9, 8, new() { MemoryBudget = root })
            : WebpAdaptivePredictorTests.Photograph(8, root);
        using var context = new OperationContext(new() { MemoryBudget = root }, default);
        var cache = new uint[1024];
        var baseline = unsuitable ? Call("SelectCachedPlan", source, 4, cache, context) : Prime(source, cache, context, true);
        var bytes = root.Bytes; var attempts = root.Attempts; var work = context.ConsumedWork;
        Assert.Equal(baseline, Call("SelectCodeCostColorPlan", source, unsuitable ? 4 : 3, cache, baseline, context));
        Assert.Equal(bytes, root.Bytes); Assert.Equal(attempts, root.Attempts); Assert.Equal(work, context.ConsumedWork);
    }
}
