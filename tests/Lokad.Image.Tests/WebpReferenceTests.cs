using System.Reflection;
using System.Runtime.ExceptionServices;
using Lokad.Image.Internal;
using Lokad.Image.Tooling;

namespace Lokad.Image.Tests;

public sealed class WebpReferenceTests
{
    private static object Call(string name, params object?[] arguments)
    {
        try { return typeof(WebpLosslessEncoder).GetMethod(name, BindingFlags.Static | BindingFlags.NonPublic)!.Invoke(null, arguments)!; }
        catch (TargetInvocationException error) when (error.InnerException is not null)
        { ExceptionDispatchInfo.Capture(error.InnerException).Throw(); throw; }
    }
    private static object Get(object value, string name) => value.GetType()
        .GetProperty(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)!.GetValue(value)!;
    private static long CodingBytes(object plan)
    {
        var tables = (WebpPrefixCode[])Get(plan, "Tables");
        return tables.Length * 8L + 32 + tables.Sum(t => t.MemoryBytes);
    }

    [Theory] [InlineData(8)] [InlineData(16)]
    public void CompleteDiagramPacketClosesTheDeclaredSizeGateWithIndependentOutputs(int depth)
    {
        using var root = new TestMemoryBudget(64L * 1024 * 1024);
        using var source = Diagram(depth, root); var held = root.Bytes; var before = source.Pixels.ToArray();
        var expected = Normalize(source);
        var options = new WebpEncodeOptions { Effort = 4, Limits = new() { MemoryBudget = root } };
        using var output = new MemoryStream(); ImageCodec.EncodeWebp(source, output, options);
        Assert.InRange(output.Length, 1, 7234); Assert.Equal(before, source.Pixels.ToArray()); Assert.Equal(held, root.Bytes);
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
    public void SelectedReferencesRetainCapacityAndReleaseBothLosingPlanAndSearchScratch()
    {
        using var root = new TestMemoryBudget(64L * 1024 * 1024);
        using var source = Diagram(8, root); var held = root.Bytes;
        using var context = new OperationContext(new() { MemoryBudget = root }, default);
        var cache = new uint[1024]; var baseline = Call("SelectCachedPlan", source, 4, cache, context);
        var oldBytes = CodingBytes(baseline); Assert.Equal(held + oldBytes, root.Bytes);
        var plan = Call("SelectReferencePlan", source, 4, cache, baseline, context);
        var references = Get(plan, "References"); Assert.NotNull(references);
        var tokens = (Array)Get(references, "Tokens"); var count = (int)Get(references, "Count");
        Assert.True(count < tokens.Length); // Capacity remains owned after token reduction.
        var retained = tokens.Length * 16L + 64;
        Assert.Equal(retained, (long)Get(references, "MemoryBytes"));
        Assert.Equal(held + CodingBytes(plan) + retained, root.Bytes);
        Assert.True(root.Peak > held + oldBytes + retained * 2 + 1_000_000);
        Call("ReleasePlan", plan, context); Assert.Equal(held, root.Bytes); Assert.False(root.IsDisposed);
    }

    [Theory] [InlineData(false)] [InlineData(true)]
    public void LocalGuardSkipsBeforeSharedAdmission(bool work)
    {
        using var root = new TestMemoryBudget(64L * 1024 * 1024); using var source = Diagram(8, root);
        using var context = new OperationContext(new() { MemoryBudget = root,
            MaxWorkingBytes = work ? ImageLimits.Default.MaxWorkingBytes : 65536 }, default);
        var cache = new uint[1024]; var baseline = Call("SelectCachedPlan", source, 4, cache, context);
        if (work) context.Work(context.RemainingWork - 1);
        var attempts = root.Attempts; var bytes = root.Bytes;
        Assert.Equal(baseline, Call("SelectReferencePlan", source, 4, cache, baseline, context));
        Assert.Equal(attempts, root.Attempts); Assert.Equal(bytes, root.Bytes);
    }

    [Theory] [InlineData(0)] [InlineData(1)] [InlineData(2)]
    public void InitialAdmissionRefusalCancellationAndWorkFailureAreTerminalAndRecoverable(int failure)
    {
        using var root = new TestMemoryBudget(64L * 1024 * 1024); using var source = Diagram(8, root); var held = root.Bytes;
        using (var cancellation = new CancellationTokenSource())
        using (var context = new OperationContext(new() { MemoryBudget = root }, cancellation.Token))
        {
            var cache = new uint[1024]; var baseline = Call("SelectCachedPlan", source, 4, cache, context); var before = root.Bytes;
            if (failure == 0) root.RefuseAtGrowth = root.GrowthAttempts + 1;
            else root.Admitted = _ =>
            {
                root.Admitted = null;
                if (failure == 1) cancellation.Cancel(); else context.Work(context.RemainingWork);
            };
            if (failure == 1) Assert.Throws<OperationCanceledException>(() => Call("SelectReferencePlan", source, 4, cache, baseline, context));
            else
            {
                var error = Assert.Throws<ImageLimitException>(() => Call("SelectReferencePlan", source, 4, cache, baseline, context));
                Assert.Equal(failure == 0 ? ImageLimitKind.WorkingBytes : ImageLimitKind.Work, error.Limit);
                Assert.Equal(failure == 0 ? ImageLimitScope.SharedBudget : ImageLimitScope.Operation, error.Scope);
            }
            Assert.Equal(before, root.Bytes);
        }
        Assert.Equal(held, root.Bytes); root.RefuseAtGrowth = 0; root.Admitted = null;
        using var recovery = new MemoryStream(); ImageCodec.EncodeWebp(source, recovery, new() { Limits = new() { MemoryBudget = root } });
        Assert.InRange(recovery.Length, 1, 7234); Assert.Equal(held, root.Bytes); Assert.False(root.IsDisposed);
    }

    [Theory] [InlineData(false)] [InlineData(true)]
    public void CandidateTableRefusalOrCancellationDoesNotFallBackAndReleasesScratch(bool cancel)
    {
        using var root = new TestMemoryBudget(64L * 1024 * 1024); using var source = Diagram(8, root); var held = root.Bytes;
        using (var cancellation = new CancellationTokenSource())
        using (var context = new OperationContext(new() { MemoryBudget = root }, cancellation.Token))
        {
            var cache = new uint[1024]; var baseline = Call("SelectCachedPlan", source, 4, cache, context); var before = root.Bytes;
            var rises = 0;
            root.Admitted = _ =>
            {
                if (++rises == 1 && !cancel) root.RefuseAtGrowth = root.GrowthAttempts + 1;
                if (rises == 2 && cancel) cancellation.Cancel();
            };
            if (cancel) Assert.Throws<OperationCanceledException>(() => Call("SelectReferencePlan", source, 4, cache, baseline, context));
            else Assert.Equal(ImageLimitScope.SharedBudget,
                Assert.Throws<ImageLimitException>(() => Call("SelectReferencePlan", source, 4, cache, baseline, context)).Scope);
            Assert.Equal(cancel ? 2 : 1, rises); Assert.Equal(before, root.Bytes);
        }
        Assert.Equal(held, root.Bytes); root.RefuseAtGrowth = 0; root.Admitted = null;
        using var recovery = new MemoryStream(); ImageCodec.EncodeWebp(source, recovery, new() { Limits = new() { MemoryBudget = root } });
        Assert.InRange(recovery.Length, 1, 7234); Assert.Equal(held, root.Bytes);
    }

    [Fact]
    public void PlaneAndDirectDistancesRoundTripIncludingNarrowWidthAliases()
    {
        using var context = new OperationContext(ImageLimits.Default, default);
        foreach (var width in new[] { 1, 3, 513 })
        foreach (var distance in new[] { 1, width, 7, 123456, 262144 })
        {
            var value = (int)Call("ReferenceDistance", distance, width, context);
            Assert.Equal(distance, WebpDistances.Resolve(value, width));
            object?[] arguments = [value, 0, 0, 0]; Call("ReferencePrefix", arguments);
            var code = (int)arguments[1]!; var extra = (int)arguments[2]!; var bits = (int)arguments[3]!;
            var restored = code < 4 ? code + 1 : ((2 + (code & 1)) << bits) + extra + 1;
            Assert.InRange(code, 0, 39); Assert.Equal(value, restored);
        }
    }

    [Fact]
    public void ReferenceHistoryWrapsAndOverlappingCopiesPreserveHiddenPixels()
    {
        using var root = new TestMemoryBudget(64L * 1024 * 1024);
        using var source = ImageBuffer.Create(257, 1100, 8, new() { MemoryBudget = root });
        for (var y = 0; y < source.Height; y++)
        for (var x = 0; x < source.Width; x++)
        {
            var value = (x + y * source.Width) % 137;
            for (var c = 0; c < 4; c++) source.SetSample(x, y, c, c == 3 ? value % 3 == 0 ? 0 : 255 : (value * 37 + c * 53) & 255);
        }
        var expected = source.Pixels.ToArray(); var held = root.Bytes;
        using (var context = new OperationContext(new() { MemoryBudget = root }, default))
        {
            var cache = new uint[1024]; var baseline = Call("SelectCachedPlan", source, 4, cache, context);
            var selected = Call("SelectReferencePlan", source, 4, cache, baseline, context);
            Assert.NotNull(Get(selected, "References"));
        }
        Assert.Equal(held, root.Bytes);
        using var packet = new MemoryStream(); ImageCodec.EncodeWebp(source, packet, new() { Limits = new() { MemoryBudget = root } });
        using var decoded = ImageCodec.Decode(packet.ToArray(), new() { Limits = new() { MemoryBudget = root } });
        source.Dispose(); packet.SetLength(0); Assert.Equal(expected, decoded.Pixels.ToArray());
        decoded.Dispose(); Assert.Equal(0, root.Bytes);
    }

    private static ImageBuffer Diagram(int depth, IMemoryBudget budget)
    {
        string? path = null;
        for (var directory = new DirectoryInfo(AppContext.BaseDirectory); directory is not null; directory = directory.Parent)
        {
            var candidate = Path.Combine(directory.FullName, "benchmarks/Fixtures/Compression/diagram.png");
            if (File.Exists(candidate)) { path = candidate; break; }
        }
        var source = ImageCodec.Decode(File.ReadAllBytes(path ?? throw new FileNotFoundException("Original diagram fixture.")),
            new() { Limits = new() { MemoryBudget = budget } });
        if (depth == 8) return source;
        using (source)
        {
            var result = ImageBuffer.Create(source.Width, source.Height, depth, new() { MemoryBudget = budget });
            for (var y = 0; y < source.Height; y++)
            for (var x = 0; x < source.Width; x++)
            for (var c = 0; c < 4; c++)
                result.SetSample(x, y, c, Math.Clamp(source.GetRow8(y)[x * 4 + c] * 257 + ((x + y) % 3 - 1) * 64, 0, 65535));
            return result;
        }
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
