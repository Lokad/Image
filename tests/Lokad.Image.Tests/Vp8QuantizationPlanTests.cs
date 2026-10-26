using System.Buffers.Binary;
using System.Security.Cryptography;
using Lokad.Image.Internal;
using Lokad.Image.Tooling;

namespace Lokad.Image.Tests;

public sealed class Vp8QuantizationPlanTests
{
    public static IEnumerable<object[]> Controls()
    {
        yield return ["opaque", 3, 6, "a209f1ad41e08cb946121fcf3cabde5a85804d6caa6c46d8df0b2693401c3d17"];
        yield return ["photograph", 4, -1, "04d1eea159c38a27b0debe3b18888361b780563f3cabb1b905efe6a75e35c4ed"];
        yield return ["diagram", 4, -1, "4f4dd21a46483779a82d26cab83540cbfcfd0008ba68130c94de79943c15e347"];
        yield return ["asymmetric", 3, 6, "b8156d544ed2d27663023bda96221d66bbd175823aa6d69d838da3fbbe385f1d"];
    }

    private static string Hash(Vp8QuantizationPlan plan)
    {
        using var hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
        Span<byte> packed = stackalloc byte[80 * 4];
        for (var segment = 0; segment < plan.SegmentCount; segment++)
        {
            for (var plane = 0; plane < 3; plane++)
            {
                var values = plan.Matrix(segment, (Vp8CoefficientPlane)plane).Values;
                for (var i = 0; i < values.Length; i++) BinaryPrimitives.WriteInt32LittleEndian(packed[(4 * i)..], values[i]);
                hash.AppendData(packed);
            }
            var costs = plan.Costs(segment).Values;
            for (var i = 0; i < costs.Length; i++) BinaryPrimitives.WriteInt32LittleEndian(packed[(4 * i)..], costs[i]);
            hash.AppendData(packed[..(costs.Length * 4)]);
        }
        return Convert.ToHexStringLower(hash.GetHashAndReset());
    }

    [Theory]
    [MemberData(nameof(Controls))]
    public void EveryUsedNativeMatrixAndCostMatchesAndSurvivesItsSources(string name, int count, int uvAc, string expected)
    {
        using var root = new TestMemoryBudget(16_000_000); using var image = OriginalVp8lControls.Buffer(name, 8, root);
        using var context = new OperationContext(new() { MemoryBudget = root }, default);
        var borrowed = image.Pixels.Length; context.ReserveBorrowed(borrowed);
        using var input = Vp8PreparedInput.Create(image, context); input.CleanupTransparentArea();
        using var analysis = Vp8SourceAnalysis.Create(input, context);
        using var segments = Vp8SourceSegments.Create(analysis, 75, false, context);
        var map = segments.Map.ToArray(); var parameters = segments.Parameters.ToArray(); var baseline = root.Bytes;
        using var plan = Vp8QuantizationPlan.Create(segments, analysis.MeanChromaScore, context);
        Assert.Equal(count, plan.SegmentCount); Assert.Equal(new(0, 0, 0, -2, uvAc), plan.QuantizerDeltas);
        Assert.Equal(expected, Hash(plan)); Assert.Equal(1000L * count + 128, plan.CapacityBytes);
        Assert.Equal(baseline + plan.CapacityBytes, root.Bytes); Assert.Equal(root.Bytes, root.Peak);
        Assert.Equal(map, segments.Map.ToArray()); Assert.Equal(parameters, segments.Parameters.ToArray());
        image.Dispose(); context.ReleaseBorrowed(borrowed); input.Dispose(); analysis.Dispose(); segments.Dispose();
        Assert.Equal(plan.CapacityBytes, root.Bytes); Assert.Equal(expected, Hash(plan));
        plan.Dispose(); plan.Dispose(); Assert.Equal(0, root.Bytes); Assert.False(root.IsDisposed);
        Assert.Throws<ObjectDisposedException>(() => plan.Matrix(0, Vp8CoefficientPlane.Luma).Steps.Length);
        Assert.Throws<ObjectDisposedException>(() => plan.Costs(0).Luma4);
    }

    [Fact]
    public void AggregateRefusalIsTerminalAndReleasedCapacityAllowsAnotherOperation()
    {
        using var image = OriginalVp8lControls.Buffer("singleton", 8); using var root = new TestMemoryBudget(16_000_000);
        using var caller = root.CreateScope(); using var context = new OperationContext(new() { MemoryBudget = root }, default);
        using var input = Vp8PreparedInput.Create(image, context); using var analysis = Vp8SourceAnalysis.Create(input, context);
        using var segments = Vp8SourceSegments.Create(analysis, 75, false, context);
        long capacity; using (var sizing = Vp8QuantizationPlan.Create(segments, analysis.MeanChromaScore, context)) capacity = sizing.CapacityBytes;
        Assert.True(caller.TryReserve(16_000_000 - root.Bytes - capacity + 1, out var held)); var baseline = root.Bytes;
        var error = Assert.Throws<ImageLimitException>(() => Vp8QuantizationPlan.Create(segments, analysis.MeanChromaScore, context));
        Assert.Equal((ImageLimitKind.WorkingBytes, ImageLimitScope.SharedBudget), (error.Limit, error.Scope));
        Assert.Equal(baseline, root.Bytes); Assert.Equal(1, root.Refusals); held.Dispose(); baseline = root.Bytes;
        using var next = Vp8QuantizationPlan.Create(segments, analysis.MeanChromaScore, context);
        Assert.Equal(baseline + capacity, root.Bytes); next.Dispose(); Assert.Equal(baseline, root.Bytes); Assert.False(root.IsDisposed);
    }

    [Fact]
    public void CancellationAfterAdmissionRefundsAndDoesNotDisposeBorrowedOwners()
    {
        using var image = OriginalVp8lControls.Buffer("singleton", 8); using var root = new TestMemoryBudget(16_000_000);
        using var cancel = new CancellationTokenSource(); using var context = new OperationContext(new() { MemoryBudget = root }, cancel.Token);
        using var input = Vp8PreparedInput.Create(image, context); using var analysis = Vp8SourceAnalysis.Create(input, context);
        using var segments = Vp8SourceSegments.Create(analysis, 75, false, context); var baseline = root.Bytes;
        root.Admitted = _ => cancel.Cancel();
        Assert.Throws<OperationCanceledException>(() => Vp8QuantizationPlan.Create(segments, analysis.MeanChromaScore, context));
        Assert.Equal(baseline, root.Bytes); root.Admitted = null;
        using var later = new OperationContext(new() { MemoryBudget = root }, default);
        using var next = Vp8QuantizationPlan.Create(segments, analysis.MeanChromaScore, later); next.Dispose();
        Assert.Equal(baseline, root.Bytes); Assert.False(root.IsDisposed);
    }

    [Fact]
    public void LocalWorkingLimitRemainsIndependentOfSharedCapacity()
    {
        using var image = OriginalVp8lControls.Buffer("singleton", 8); using var root = new TestMemoryBudget(16_000_000);
        using var context = new OperationContext(new() { MemoryBudget = root }, default);
        using var input = Vp8PreparedInput.Create(image, context); using var analysis = Vp8SourceAnalysis.Create(input, context);
        using var segments = Vp8SourceSegments.Create(analysis, 75, false, context);
        long capacity; using (var sizing = Vp8QuantizationPlan.Create(segments, analysis.MeanChromaScore, context)) capacity = sizing.CapacityBytes;
        var baseline = root.Bytes;
        using var limited = new OperationContext(new() { MemoryBudget = root, MaxWorkingBytes = capacity - 1 }, default);
        var error = Assert.Throws<ImageLimitException>(() => Vp8QuantizationPlan.Create(segments, analysis.MeanChromaScore, limited));
        Assert.Equal((ImageLimitKind.WorkingBytes, ImageLimitScope.Operation), (error.Limit, error.Scope));
        Assert.Equal(0, root.Refusals); Assert.Equal(baseline, root.Bytes);
        using var exact = new OperationContext(new() { MemoryBudget = root, MaxWorkingBytes = capacity }, default);
        using var next = Vp8QuantizationPlan.Create(segments, analysis.MeanChromaScore, exact); next.Dispose(); Assert.Equal(baseline, root.Bytes);
    }

    [Fact]
    public void RetainedPlanAndReplacementUseTheSameAggregateAllowance()
    {
        using var image = OriginalVp8lControls.Buffer("singleton", 8); using var root = new TestMemoryBudget(16_000_000);
        using var caller = root.CreateScope(); using var context = new OperationContext(new() { MemoryBudget = root }, default);
        using var input = Vp8PreparedInput.Create(image, context); using var analysis = Vp8SourceAnalysis.Create(input, context);
        using var segments = Vp8SourceSegments.Create(analysis, 75, false, context);
        using var first = Vp8QuantizationPlan.Create(segments, analysis.MeanChromaScore, context); var hash = Hash(first);
        Assert.True(caller.TryReserve(16_000_000 - root.Bytes - first.CapacityBytes + 1, out var held)); var baseline = root.Bytes;
        Assert.Throws<ImageLimitException>(() => Vp8QuantizationPlan.Create(segments, analysis.MeanChromaScore, context));
        Assert.Equal(baseline, root.Bytes); Assert.Equal(hash, Hash(first)); held.Dispose(); baseline = root.Bytes;
        using var second = Vp8QuantizationPlan.Create(segments, analysis.MeanChromaScore, context);
        Assert.Equal(baseline + second.CapacityBytes, root.Bytes); first.Dispose(); Assert.Equal(hash, Hash(second));
    }
}
