using System.Reflection;
using System.Runtime.ExceptionServices;
using Lokad.Image.Internal;
using Lokad.Image.Tooling;

namespace Lokad.Image.Tests;

public sealed class WebpCodeCostTests
{
    private static object Call(string name, params object?[] arguments)
    {
        try { return typeof(WebpLosslessEncoder).GetMethod(name, BindingFlags.Static | BindingFlags.NonPublic)!.Invoke(null, arguments)!; }
        catch (TargetInvocationException error) when (error.InnerException is not null)
        { ExceptionDispatchInfo.Capture(error.InnerException).Throw(); throw; }
    }
    private static object Get(object value, string name) => value.GetType().GetProperty(name)!.GetValue(value)!;
    private static object Prime(ImageBuffer source, uint[] cache, OperationContext context)
    {
        var raw = Call("SelectCachedPlan", source, 4, cache, context);
        var result = Call("SelectNoCachePlan", source, 4, cache, raw, context);
        Assert.False((bool)Get(result, "UseCache")); return result;
    }
    private static long Tables(object value)
    {
        var tables = (WebpPrefixCode[])value;
        return tables.Length * 8L + 32 + tables.Sum(t => t.MemoryBytes);
    }

    [Theory] [InlineData(8)] [InlineData(16)]
    public void CompletePacketImprovesThePredecessorWithExactLimitsAndIndependentOutputs(int depth)
    {
        using var root = new TestMemoryBudget(32L * 1024 * 1024);
        using var source = WebpAdaptivePredictorTests.Photograph(depth, root); var held = root.Bytes;
        var expected = Normalize(source); var before = source.Pixels.ToArray();
        var options = new WebpEncodeOptions { Effort = 4, Limits = new() { MemoryBudget = root } };
        using var output = new MemoryStream(); ImageCodec.EncodeWebp(source, output, options);
        Assert.InRange(output.Length, 1, 161410); Assert.Equal(before, source.Pixels.ToArray()); Assert.Equal(held, root.Bytes);
        var packet = output.ToArray(); using var exact = new MemoryStream();
        ImageCodec.EncodeWebp(source, exact, options with { Limits = options.Limits with { MaxOutputBytes = packet.Length } });
        Assert.Equal(packet, exact.ToArray()); using var refused = new MemoryStream();
        Assert.Equal(ImageLimitKind.OutputBytes, Assert.Throws<ImageLimitException>(() => ImageCodec.EncodeWebp(source, refused,
            options with { Limits = options.Limits with { MaxOutputBytes = packet.Length - 1 } })).Limit);
        Assert.Equal(0, refused.Length); Assert.True(refused.CanWrite); Assert.Equal(held, root.Bytes);
        source.Dispose(); Assert.Equal(0, root.Bytes);
        using var first = ImageCodec.Decode(packet, new() { Limits = options.Limits });
        using var second = ImageCodec.Decode(exact.ToArray(), new() { Limits = options.Limits });
        packet.AsSpan().Clear(); first.Dispose(); Assert.Equal(expected, second.Pixels.ToArray());
        second.Dispose(); Assert.Equal(0, root.Bytes); Assert.False(root.IsDisposed);
    }

    [Fact]
    public void RefinementReplacesPredictorOwnershipAndTransfersExactlyOneColorOwner()
    {
        using var root = new TestMemoryBudget(32L * 1024 * 1024);
        using var source = WebpAdaptivePredictorTests.Photograph(8, root); var held = root.Bytes;
        using var context = new OperationContext(new() { MemoryBudget = root }, default);
        var cache = new uint[1024]; var baseline = Prime(source, cache, context);
        var selected = Call("SelectCodeCostPlan", source, 4, cache, baseline, context);
        Assert.NotSame(Get(baseline, "Modes"), Get(selected, "Modes"));
        Assert.NotSame(Get(baseline, "ModeTable"), Get(selected, "ModeTable"));
        Assert.Same(Get(baseline, "Color"), Get(selected, "Color"));
        var groups = Get(selected, "Groups"); var color = Get(selected, "Color");
        var bytes = Tables(Get(selected, "Tables")) + ((byte[])Get(selected, "Modes")).Length + 32L
            + ((WebpPrefixCode)Get(selected, "ModeTable")).MemoryBytes
            + ((byte[])Get(groups, "Map")).Length + 80L + ((WebpPrefixCode)Get(groups, "MapTable")).MemoryBytes
            + ((byte[])Get(color, "Coefficients")).Length + 64L + Tables(Get(color, "Tables"));
        Assert.Equal(held + bytes, root.Bytes);
        Call("ReleasePlan", selected, context); Assert.Equal(held, root.Bytes); Assert.False(root.IsDisposed);
    }

    [Theory] [InlineData(false)] [InlineData(true)]
    public void LocalMemoryOrWorkGuardSkipsBeforeSharedAdmission(bool work)
    {
        using var root = new TestMemoryBudget(32L * 1024 * 1024);
        using var source = WebpAdaptivePredictorTests.Photograph(8, root);
        using var context = new OperationContext(new() { MemoryBudget = root }, default);
        var cache = new uint[1024]; var baseline = Prime(source, cache, context);
        var borrowed = 0L;
        if (work) context.Work(context.RemainingWork - 1);
        else { borrowed = context.RemainingWorkingBytes - 1; context.ReserveBorrowed(borrowed); }
        var attempts = root.Attempts; var bytes = root.Bytes;
        Assert.Equal(baseline, Call("SelectCodeCostPlan", source, 4, cache, baseline, context));
        Assert.Equal(attempts, root.Attempts); Assert.Equal(bytes, root.Bytes);
        if (!work) context.ReleaseBorrowed(borrowed);
    }

    [Theory] [InlineData(0)] [InlineData(1)] [InlineData(2)]
    public void InitialRefusalCancellationOrWorkExhaustionIsTerminalAndReleasesTheNewMap(int failure)
    {
        using var root = new TestMemoryBudget(32L * 1024 * 1024);
        using var source = WebpAdaptivePredictorTests.Photograph(8, root); var held = root.Bytes;
        using (var cancellation = new CancellationTokenSource())
        using (var context = new OperationContext(new() { MemoryBudget = root }, cancellation.Token))
        {
            var cache = new uint[1024]; var baseline = Prime(source, cache, context); var before = root.Bytes;
            if (failure == 0) root.RefuseAtGrowth = root.GrowthAttempts + 1;
            else root.Admitted = _ =>
            {
                root.Admitted = null;
                if (failure == 1) cancellation.Cancel(); else context.Work(context.RemainingWork);
            };
            if (failure == 1) Assert.Throws<OperationCanceledException>(() => Call("SelectCodeCostPlan", source, 4, cache, baseline, context));
            else
            {
                var error = Assert.Throws<ImageLimitException>(() => Call("SelectCodeCostPlan", source, 4, cache, baseline, context));
                Assert.Equal(failure == 0 ? ImageLimitKind.WorkingBytes : ImageLimitKind.Work, error.Limit);
                Assert.Equal(failure == 0 ? ImageLimitScope.SharedBudget : ImageLimitScope.Operation, error.Scope);
            }
            Assert.Equal(before, root.Bytes);
        }
        Assert.Equal(held, root.Bytes); root.RefuseAtGrowth = 0; root.Admitted = null;
        using var recovery = new MemoryStream(); ImageCodec.EncodeWebp(source, recovery, new() { Limits = new() { MemoryBudget = root } });
        Assert.InRange(recovery.Length, 1, 161410); Assert.Equal(held, root.Bytes); Assert.False(root.IsDisposed);
    }

    [Theory] [InlineData(false)] [InlineData(true)]
    public void RefitRefusalOrCancellationDoesNotFallBackAndReleasesAllOwners(bool cancel)
    {
        using var root = new TestMemoryBudget(32L * 1024 * 1024);
        using var source = WebpAdaptivePredictorTests.Photograph(8, root); var held = root.Bytes;
        var phases = 0; var high = false; using var cancellation = new CancellationTokenSource();
        root.Admitted = bytes =>
        {
            var now = bytes > held + 5_000_000;
            if (now && !high && ++phases == 5)
            {
                if (cancel) cancellation.Cancel(); else root.RefuseAtGrowth = root.GrowthAttempts + 1;
            }
            high = now;
        };
        using var output = new MemoryStream(); var options = new WebpEncodeOptions { Limits = new() { MemoryBudget = root } };
        if (cancel) Assert.Throws<OperationCanceledException>(() => ImageCodec.EncodeWebp(source, output, options, cancellation.Token));
        else Assert.Equal(ImageLimitScope.SharedBudget, Assert.Throws<ImageLimitException>(() => ImageCodec.EncodeWebp(source, output, options)).Scope);
        Assert.Equal(5, phases); Assert.Equal(0, output.Length); Assert.True(output.CanWrite); Assert.Equal(held, root.Bytes);
        root.RefuseAtGrowth = 0; root.Admitted = null;
        using var recovery = new MemoryStream(); ImageCodec.EncodeWebp(source, recovery, options);
        Assert.InRange(recovery.Length, 1, 161410); Assert.Equal(held, root.Bytes);
    }

    [Fact]
    public void AllChannelCostsPreserveHiddenRgbAndVariableAlphaWithFrozenColorCoefficients()
    {
        using var root = new TestMemoryBudget(32L * 1024 * 1024);
        using var source = WebpAdaptivePredictorTests.Photograph(8, root);
        using var context = new OperationContext(new() { MemoryBudget = root }, default);
        var cache = new uint[1024]; var old = Prime(source, cache, context);
        for (var y = 0; y < source.Height; y++)
        for (var x = 0; x < source.Width; x++) source.SetSample(x, y, 3, (x + 3 * y) % 11 == 0 ? 0 : (x * 13 + y * 37) & 255);
        var expected = Normalize(source);
        // Freeze the old owned maps, then fit actual uncached channel codes for
        // the changed alpha samples. This directly exercises all-channel scoring.
        var baseline = Call("PrepareCachedTables", source, 4, cache, true, context,
            Get(old, "Modes"), Get(old, "ModeTable"), Get(old, "Color"), false);
        baseline.GetType().GetProperty("Alpha")!.SetValue(baseline, true);
        baseline = Call("SelectEntropyPlan", source, 4, cache, baseline, context); Call("ReleaseCodingPlan", old, context);
        var work = context.ConsumedWork; var selected = Call("SelectCodeCostPlan", source, 4, cache, baseline, context);
        Assert.True(context.ConsumedWork > work); Assert.Same(Get(baseline, "Color"), Get(selected, "Color"));
        using var output = new MemoryStream(); Call("CachedPayload", source, output, 4, cache, selected, context);
        var payload = output.ToArray(); source.Dispose();
        using var decoding = new OperationContext(ImageLimits.Default, default);
        using var decoded = new WebpLosslessDecoder(payload, 513, 259, decoding).Decode();
        payload.AsSpan().Clear(); Assert.Equal(expected, decoded.Pixels.ToArray());
        context.Dispose(); Assert.Equal(0, root.Bytes); Assert.False(root.IsDisposed);
    }

    private static byte[] Normalize(ImageBuffer image)
    {
        var result = new byte[image.Width * image.Height * 4];
        for (var y = 0; y < image.Height; y++)
        for (var x = 0; x < image.Width * 4; x++)
            result[y * image.Width * 4 + x] = image.BitDepth == 8 ? image.GetRow8(y)[x] : (byte)((image.GetRow16(y)[x] + 128) / 257);
        return result;
    }
}
