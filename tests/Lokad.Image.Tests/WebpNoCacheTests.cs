using System.Reflection;
using System.Runtime.ExceptionServices;
using Lokad.Image.Internal;
using Lokad.Image.Tooling;

namespace Lokad.Image.Tests;

public sealed class WebpNoCacheTests
{
    private static object Call(string name, params object?[] arguments)
    {
        try { return typeof(WebpLosslessEncoder).GetMethod(name, BindingFlags.Static | BindingFlags.NonPublic)!.Invoke(null, arguments)!; }
        catch (TargetInvocationException error) when (error.InnerException is not null)
        { ExceptionDispatchInfo.Capture(error.InnerException).Throw(); throw; }
    }
    private static object Get(object value, string name) => value.GetType().GetProperty(name)!.GetValue(value)!;
    private static long Tables(object value)
    {
        var tables = (WebpPrefixCode[])value;
        return tables.Length * 8L + 32 + tables.Sum(t => t.Alphabet * 3L + 128);
    }

    [Theory] [InlineData(8)] [InlineData(16)]
    public void UncachedPayloadHasNoCacheFlagAndPreservesHiddenNormalizedSamples(int depth)
    {
        using var source = ImageBuffer.Create(17, 9, depth); var expected = new byte[17 * 9 * 4];
        for (var y = 0; y < 9; y++)
        for (var x = 0; x < 17; x++)
        for (var c = 0; c < 4; c++)
        {
            var value = c == 3 && (x + y) % 4 == 0 ? 0 : (x * 73 + y * 17 + c * 57) & 255;
            source.SetSample(x, y, c, depth == 8 ? value : Math.Clamp(value * 257 + ((x + y) % 3 - 1) * 64, 0, 65535));
            expected[(y * 17 + x) * 4 + c] = (byte)value;
        }
        var before = source.Pixels.ToArray(); var cache = Enumerable.Repeat(0xa5a5a5a5u, 1024).ToArray();
        using var context = new OperationContext(ImageLimits.Default, default);
        var plan = Call("PrepareCachedTables", source, 4, cache, false, context, null, null, null, false);
        Assert.False((bool)Get(plan, "UseCache")); Assert.Equal(280, ((WebpPrefixCode[])Get(plan, "Tables"))[0].Alphabet);
        using var output = new MemoryStream(); Call("CachedPayload", source, output, 4, cache, plan, context);
        var payload = output.ToArray(); Assert.Equal(5, payload[5] & 31); // SG,transform-end,cache flag0.
        Assert.All(cache, value => Assert.Equal(0xa5a5a5a5u, value)); Assert.Equal(before, source.Pixels.ToArray());
        using var decoding = new OperationContext(ImageLimits.Default, default);
        using var decoded = new WebpLosslessDecoder(payload, 17, 9, decoding).Decode();
        source.Dispose(); payload.AsSpan().Clear(); Assert.Equal(expected, decoded.Pixels.ToArray());
    }

    [Fact]
    public void SelectedUncachedPlanTransfersExactlyOneChargeForBothInheritedMaps()
    {
        using var root = new TestMemoryBudget(32L * 1024 * 1024);
        using var source = WebpAdaptivePredictorTests.Photograph(8, root); var held = root.Bytes;
        using (var context = new OperationContext(new() { MemoryBudget = root }, default))
        {
            var cache = new uint[1024]; // Borrowed source/cache at this private unit boundary.
            var baseline = Call("SelectCachedPlan", source, 4, cache, context); var before = cache.ToArray();
            var plan = Call("SelectNoCachePlan", source, 4, cache, baseline, context);
            Assert.False((bool)Get(plan, "UseCache")); Assert.Same(Get(baseline, "Modes"), Get(plan, "Modes"));
            Assert.Same(Get(baseline, "Color"), Get(plan, "Color")); Assert.Equal(before, cache);
            var tables = (WebpPrefixCode[])Get(plan, "Tables");
            for (var group = 0; group < tables.Length / 5; group++)
                Assert.Equal(new[] { 280, 256, 256, 256, 40 }, tables.Skip(group * 5).Take(5).Select(t => t.Alphabet).ToArray());
            var groups = Get(plan, "Groups"); var color = Get(plan, "Color"); var modes = (byte[])Get(plan, "Modes");
            var bytes = Tables(tables) + modes.Length + 32L + ((WebpPrefixCode)Get(plan, "ModeTable")).Alphabet * 3L + 128
                + ((byte[])Get(groups, "Map")).Length + 80L + ((WebpPrefixCode)Get(groups, "MapTable")).Alphabet * 3L + 128
                + ((byte[])Get(color, "Coefficients")).Length + 64L + Tables(Get(color, "Tables"));
            Assert.Equal(held + bytes, root.Bytes); Assert.False(root.IsDisposed);
        }
        Assert.Equal(held, root.Bytes);
    }

    [Theory] [InlineData(false)] [InlineData(true)]
    public void TightLocalMemoryOrWorkSkipsBeforeAnySharedAdmission(bool work)
    {
        using var root = new TestMemoryBudget(32L * 1024 * 1024);
        using var source = WebpAdaptivePredictorTests.Photograph(8, root);
        using var context = new OperationContext(new() { MemoryBudget = root, MaxWorkingBytes = work ? ImageLimits.Default.MaxWorkingBytes : 65536 }, default);
        var cache = new uint[1024]; var baseline = Call("SelectCachedPlan", source, 4, cache, context);
        if (work) context.Work(context.RemainingWork - 1);
        var attempts = root.Attempts; var bytes = root.Bytes;
        var plan = Call("SelectNoCachePlan", source, 4, cache, baseline, context);
        Assert.Equal(baseline, plan); Assert.True((bool)Get(plan, "UseCache"));
        Assert.Equal(attempts, root.Attempts); Assert.Equal(bytes, root.Bytes); Assert.False(root.IsDisposed);
    }

    [Fact]
    public void WorkExhaustionAfterAdmissionReleasesUnusedCapacityAndDoesNotSelectBaseline()
    {
        using var root = new TestMemoryBudget(32L * 1024 * 1024);
        using var source = WebpAdaptivePredictorTests.Photograph(8, root); var held = root.Bytes;
        using (var context = new OperationContext(new() { MemoryBudget = root }, default))
        {
            var cache = new uint[1024]; var baseline = Call("SelectCachedPlan", source, 4, cache, context); var before = root.Bytes;
            root.Admitted = _ => { root.Admitted = null; context.Work(context.RemainingWork); };
            var error = Assert.Throws<ImageLimitException>(() => Call("SelectNoCachePlan", source, 4, cache, baseline, context));
            Assert.Equal(ImageLimitKind.Work, error.Limit); Assert.Equal(before, root.Bytes); Assert.Equal(0, root.Refusals);
        }
        Assert.Equal(held, root.Bytes);
        using var recovery = new MemoryStream(); ImageCodec.EncodeWebp(source, recovery, new() { Limits = new() { MemoryBudget = root } });
        Assert.InRange(recovery.Length, 1, 169806); Assert.Equal(held, root.Bytes); Assert.False(root.IsDisposed);
    }

    [Theory] [InlineData(8)] [InlineData(16)]
    public void CompleteSelectedPacketImprovesThePredecessorWithIndependentOutputs(int depth)
    {
        using var root = new TestMemoryBudget(32L * 1024 * 1024);
        using var source = WebpAdaptivePredictorTests.Photograph(depth, root); var held = root.Bytes; var before = source.Pixels.ToArray();
        var expected = new byte[source.Width * source.Height * 4];
        for (var y = 0; y < source.Height; y++)
        for (var x = 0; x < source.Width * 4; x++)
            expected[y * source.Width * 4 + x] = depth == 8 ? source.GetRow8(y)[x] : (byte)((source.GetRow16(y)[x] + 128) / 257);
        var options = new WebpEncodeOptions { Effort = 4, Limits = new() { MemoryBudget = root } };
        using var output = new MemoryStream(); ImageCodec.EncodeWebp(source, output, options);
        Assert.InRange(output.Length, 1, 169806); Assert.Equal(before, source.Pixels.ToArray()); Assert.Equal(held, root.Bytes);
        var packet = output.ToArray(); using var exact = new MemoryStream();
        ImageCodec.EncodeWebp(source, exact, options with { Limits = options.Limits with { MaxOutputBytes = packet.Length } });
        Assert.Equal(packet, exact.ToArray());
        using var refused = new MemoryStream();
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
    public void UncachedGroupRefusalAndCancellationAreTerminalWithBothMapsReleased()
    {
        using var root = new TestMemoryBudget(32L * 1024 * 1024);
        using var source = WebpAdaptivePredictorTests.Photograph(8, root); var held = root.Bytes;
        var options = new WebpEncodeOptions { Effort = 4, Limits = new() { MemoryBudget = root } };
        var phases = 0; var high = false;
        root.Admitted = bytes =>
        {
            var now = bytes > held + 5_000_000;
            if (now && !high && ++phases == 4) root.RefuseAtGrowth = root.GrowthAttempts + 1;
            high = now;
        };
        using var refused = new MemoryStream();
        var error = Assert.Throws<ImageLimitException>(() => ImageCodec.EncodeWebp(source, refused, options));
        Assert.Equal(ImageLimitScope.SharedBudget, error.Scope); Assert.Equal(ImageLimitKind.WorkingBytes, error.Limit);
        Assert.Equal(4, phases); Assert.Equal(1, root.Refusals); Assert.Equal(0, refused.Length); Assert.Equal(held, root.Bytes);
        root.RefuseAtGrowth = 0; high = false; phases = 0; using var cancellation = new CancellationTokenSource();
        root.Admitted = bytes =>
        {
            var now = bytes > held + 5_000_000;
            if (now && !high && ++phases == 4) cancellation.Cancel();
            high = now;
        };
        using var cancelled = new MemoryStream();
        Assert.Throws<OperationCanceledException>(() => ImageCodec.EncodeWebp(source, cancelled, options, cancellation.Token));
        Assert.Equal(4, phases); Assert.Equal(0, cancelled.Length); Assert.Equal(held, root.Bytes); root.Admitted = null;
        using var recovery = new MemoryStream(); ImageCodec.EncodeWebp(source, recovery, options);
        Assert.InRange(recovery.Length, 1, 169806); Assert.Equal(held, root.Bytes); Assert.False(root.IsDisposed);
    }
}
