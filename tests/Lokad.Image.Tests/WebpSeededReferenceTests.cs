using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Security.Cryptography;
using Lokad.Image.Internal;
using Lokad.Image.Tooling;

namespace Lokad.Image.Tests;

public sealed class WebpSeededReferenceTests
{
    private static object Call(string name, params object?[] args)
    {
        try { return typeof(WebpLosslessEncoder).GetMethod(name, BindingFlags.Static | BindingFlags.NonPublic)!.Invoke(null, args)!; }
        catch (TargetInvocationException e) when (e.InnerException is not null)
        { ExceptionDispatchInfo.Capture(e.InnerException).Throw(); throw; }
    }
    private static object Get(object value, string name) => value.GetType()
        .GetProperty(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)!.GetValue(value)!;
    private static long Coding(object plan)
    {
        var tables = (WebpPrefixCode[])Get(plan, "Tables"); var groups = Get(plan, "Groups");
        return tables.Length * 8L + 32 + tables.Sum(t => t.MemoryBytes)
            + (groups is null ? 0 : ((byte[])Get(groups, "Map")).Length + 80L + ((WebpPrefixCode)Get(groups, "MapTable")).MemoryBytes);
    }
    private static object Prime(ImageBuffer source, uint[] cache, OperationContext context)
    {
        var plan = Call("SelectCachedPlan", source, 4, cache, context);
        Assert.True((bool)Get(plan, "Predict")); Assert.True((bool)Get(plan, "UseCache"));
        Assert.Null(Get(plan, "Modes")); Assert.Null(Get(plan, "Color")); Assert.NotNull(Get(plan, "Groups")); return plan;
    }

    [Theory] [InlineData(8)] [InlineData(16)]
    public void CompleteOpaquePacketPassesSizeGateWithExactLimitsAndIndependentOutputs(int depth)
    {
        using var root = new TestMemoryBudget(64L * 1024 * 1024); using var source = Source(depth, root);
        var held = root.Bytes; var before = source.Pixels.ToArray(); var expected = Normalize(source);
        Assert.Equal("54e27bd73c9ee221bdd4872e6b7bf79676257346cee2eff8ddacccb89e0cdb0a", Convert.ToHexStringLower(SHA256.HashData(expected)));
        var options = new WebpEncodeOptions { Limits = new() { MemoryBudget = root } };
        using var output = new MemoryStream(); ImageCodec.EncodeWebp(source, output, options);
        Assert.InRange(output.Length, 1, 121896); Assert.Equal(before, source.Pixels.ToArray()); Assert.Equal(held, root.Bytes);
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
    public void SelectedCapacityIsChargedAndBorrowedSeedChannelsAreNotReleasedTwice()
    {
        using var root = new TestMemoryBudget(64L * 1024 * 1024); using var source = Source(8, root); var held = root.Bytes;
        using var context = new OperationContext(new() { MemoryBudget = root }, default);
        var cache = new uint[1024]; var baseline = Prime(source, cache, context); var selected = Call("SelectSeededPlan", source, 4, cache, baseline, context);
        var references = Get(selected, "References"); Assert.NotNull(references);
        var tokens = (Array)Get(references, "Tokens"); Assert.True((int)Get(references, "Count") < tokens.Length);
        var retained = tokens.Length * 16L + 64;
        Assert.Equal(retained, (long)Get(references, "MemoryBytes"));
        Assert.Equal(held + Coding(selected) + retained, root.Bytes); Assert.True(root.Peak > held + 5_000_000);
        Call("ReleasePlan", selected, context); Assert.Equal(held, root.Bytes); Assert.False(root.IsDisposed);
    }

    [Theory] [InlineData(false)] [InlineData(true)]
    public void LocalMemoryOrWorkGuardSkipsBeforeAnySharedAdmission(bool work)
    {
        using var root = new TestMemoryBudget(64L * 1024 * 1024); using var source = Source(8, root);
        using var context = new OperationContext(new() { MemoryBudget = root }, default);
        var cache = new uint[1024]; var baseline = Prime(source, cache, context); var borrowed = 0L;
        if (work) context.Work(context.RemainingWork - 1);
        else { borrowed = context.RemainingWorkingBytes - 1; context.ReserveBorrowed(borrowed); }
        var bytes = root.Bytes; var attempts = root.Attempts;
        Assert.Equal(baseline, Call("SelectSeededPlan", source, 4, cache, baseline, context));
        Assert.Equal(bytes, root.Bytes); Assert.Equal(attempts, root.Attempts);
        if (!work) context.ReleaseBorrowed(borrowed);
    }

    [Theory] [InlineData(0)] [InlineData(1)] [InlineData(2)]
    public void InitialRefusalCancellationAndWorkFailureAreTerminalAndRecoverable(int failure)
    {
        using var root = new TestMemoryBudget(64L * 1024 * 1024); using var source = Source(8, root); var held = root.Bytes;
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
            if (failure == 1) Assert.Throws<OperationCanceledException>(() => Call("SelectSeededPlan", source, 4, cache, baseline, context));
            else
            {
                var error = Assert.Throws<ImageLimitException>(() => Call("SelectSeededPlan", source, 4, cache, baseline, context));
                Assert.Equal(failure == 0 ? ImageLimitKind.WorkingBytes : ImageLimitKind.Work, error.Limit);
                Assert.Equal(failure == 0 ? ImageLimitScope.SharedBudget : ImageLimitScope.Operation, error.Scope);
            }
            Assert.Equal(before, root.Bytes);
        }
        Assert.Equal(held, root.Bytes); root.RefuseAtGrowth = 0; root.Admitted = null;
        using var recovery = new MemoryStream(); ImageCodec.EncodeWebp(source, recovery, new() { Limits = new() { MemoryBudget = root } });
        Assert.InRange(recovery.Length, 1, 121896); Assert.Equal(held, root.Bytes);
    }

    [Theory] [InlineData(false)] [InlineData(true)]
    public void PartialSeedTreeRefusalOrCancellationReleasesEveryReservation(bool cancel)
    {
        using var root = new TestMemoryBudget(64L * 1024 * 1024); using var source = Source(8, root); var held = root.Bytes;
        using (var cancellation = new CancellationTokenSource())
        using (var context = new OperationContext(new() { MemoryBudget = root }, cancellation.Token))
        {
            var cache = new uint[1024]; var baseline = Prime(source, cache, context); var before = root.Bytes; var entered = false;
            root.Admitted = bytes =>
            {
                if (entered || bytes <= before + 1_000_000) return;
                entered = true;
                if (cancel) root.Admitted = _ => { root.Admitted = null; cancellation.Cancel(); };
                else root.RefuseAtGrowth = root.GrowthAttempts + 2; // New G owner, then its constructor scratch.
            };
            if (cancel) Assert.Throws<OperationCanceledException>(() => Call("SelectSeededPlan", source, 4, cache, baseline, context));
            else Assert.Equal(ImageLimitScope.SharedBudget,
                Assert.Throws<ImageLimitException>(() => Call("SelectSeededPlan", source, 4, cache, baseline, context)).Scope);
            Assert.True(entered);
        }
        Assert.Equal(held, root.Bytes); root.RefuseAtGrowth = 0; root.Admitted = null;
        using var recovery = new MemoryStream(); ImageCodec.EncodeWebp(source, recovery, new() { Limits = new() { MemoryBudget = root } });
        Assert.InRange(recovery.Length, 1, 121896); Assert.Equal(held, root.Bytes);
    }

    [Fact]
    public void ResidualWindowWrapsRewindsAndRejectsExpiredHistory()
    {
        using var source = Source(8, null, 13, 17); using var context = new OperationContext(ImageLimits.Default, default);
        var type = typeof(WebpLosslessEncoder).GetNestedType("SeededWindow", BindingFlags.NonPublic)!;
        var window = Activator.CreateInstance(type, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
            null, [source, 16, context], null)!;
        var get = type.GetMethod("Get", BindingFlags.Instance | BindingFlags.NonPublic)!;
        for (var pass = 0; pass < 2; pass++)
        {
            for (var at = 0; at < 13 * 17; at++)
            {
                Assert.Equal(WebpLosslessEncoder.LeftResidual(source, at, context), (uint)get.Invoke(window, [at])!);
                if (at >= 7) Assert.Equal(WebpLosslessEncoder.LeftResidual(source, at - 7, context), (uint)get.Invoke(window, [at - 7])!);
            }
            Assert.IsType<InvalidOperationException>(Assert.Throws<TargetInvocationException>(() => get.Invoke(window, [0])).InnerException);
            type.GetMethod("Rewind", BindingFlags.Instance | BindingFlags.NonPublic)!.Invoke(window, null);
        }
    }

    [Fact]
    public void GeneralCopiesCrossHistoryAndLookaheadWrapWithIndependentPixels()
    {
        using var root = new TestMemoryBudget(128L * 1024 * 1024); using var source = Source(8, root, 513, 1030);
        var expected = Normalize(source); var held = root.Bytes;
        using (var context = new OperationContext(new() { MemoryBudget = root, MaxWork = 8_000_000_000 }, default))
        {
            var cache = new uint[1024]; var baseline = Call("PrepareCachedTables", source, 4, cache, true, context, null, null, null, true);
            baseline = Call("SelectEntropyPlan", source, 4, cache, baseline, context); Assert.NotNull(Get(baseline, "Groups"));
            var selected = Call("SelectSeededPlan", source, 4, cache, baseline, context); Assert.NotNull(Get(selected, "References"));
            using var output = new MemoryStream(); Call("CachedPayload", source, output, 4, cache, selected, context);
            var payload = output.ToArray(); source.Dispose();
            using var decoding = new OperationContext(ImageLimits.Default, default);
            using var decoded = new WebpLosslessDecoder(payload, 513, 1030, decoding).Decode();
            payload.AsSpan().Clear(); Assert.Equal(expected, decoded.Pixels.ToArray());
        }
        Assert.Equal(0, root.Bytes); Assert.True(held > 0);
    }

    [Fact]
    public void SeededLeftPathPreservesHiddenRgbAtVariableAlpha()
    {
        using var root = new TestMemoryBudget(64L * 1024 * 1024); using var source = Source(8, root);
        for (var y = 0; y < source.Height; y++)
        for (var x = 0; x < source.Width; x++) source.SetSample(x, y, 3, (x + y) % 13 == 0 ? 0 : 255);
        var expected = Normalize(source);
        using (var context = new OperationContext(new() { MemoryBudget = root }, default))
        {
            var cache = new uint[1024]; var baseline = Call("PrepareCachedTables", source, 4, cache, true, context, null, null, null, true);
            baseline.GetType().GetProperty("Alpha")!.SetValue(baseline, true);
            baseline = Call("SelectEntropyPlan", source, 4, cache, baseline, context); Assert.NotNull(Get(baseline, "Groups"));
            var work = context.ConsumedWork; var selected = Call("SelectSeededPlan", source, 4, cache, baseline, context);
            Assert.True(context.ConsumedWork > work); Assert.NotNull(Get(selected, "References"));
            using var output = new MemoryStream(); Call("CachedPayload", source, output, 4, cache, selected, context);
            var payload = output.ToArray(); source.Dispose();
            using var decoding = new OperationContext(ImageLimits.Default, default);
            using var decoded = new WebpLosslessDecoder(payload, 513, 259, decoding).Decode();
            payload.AsSpan().Clear(); Assert.Equal(expected, decoded.Pixels.ToArray());
        }
        Assert.Equal(0, root.Bytes);
    }

    private static ImageBuffer Source(int depth, IMemoryBudget? budget, int width = 513, int height = 259)
    {
        var result = ImageBuffer.Create(width, height, depth, new() { MemoryBudget = budget });
        for (var y = 0; y < height; y++)
        for (var x = 0; x < width; x++)
        for (var c = 0; c < 4; c++)
        {
            var sample = c == 3 ? 255 : (((x * 12347 + y * 7919 + c * 23459 + x * y * 101) & 65535) + 128) / 257;
            result.SetSample(x, y, c, depth == 8 ? sample : Math.Clamp(sample * 257 + ((x + y) % 3 - 1) * 64, 0, 65535));
        }
        return result;
    }
    private static byte[] Normalize(ImageBuffer image)
    {
        var bytes = new byte[image.Width * image.Height * 4];
        for (var y = 0; y < image.Height; y++)
        for (var x = 0; x < image.Width * 4; x++)
            bytes[y * image.Width * 4 + x] = image.BitDepth == 8 ? image.GetRow8(y)[x] : (byte)((image.GetRow16(y)[x] + 128) / 257);
        return bytes;
    }
}
