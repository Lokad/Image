using System.Buffers.Binary;
using System.Security.Cryptography;
using Lokad.Image.Internal;
using Lokad.Image.Tooling;

namespace Lokad.Image.Tests;

public sealed class Vp8RatePlanTests
{
    private static byte[] ArithmeticProbabilities() => Enumerable.Range(0, 1056)
        .Select(p => (byte)((37 * p + 17 * (p / 11) + p / 33) & 255)).ToArray();

    private static void AppendWords(IncrementalHash hash, ReadOnlySpan<ushort> values)
    {
        Span<byte> word = stackalloc byte[2];
        foreach (var value in values) { BinaryPrimitives.WriteUInt16LittleEndian(word, value); hash.AppendData(word); }
    }

    [Fact]
    public void AllGeneratedTablesMatchNativeDefaultAndArithmeticProbabilityProfiles()
    {
        using var root = new TestMemoryBudget(1_000_000); using var context = new OperationContext(new() { MemoryBudget = root }, default);
        var probabilities = Vp8Tables.CoefficientProbabilities.ToArray();
        using var first = Vp8RatePlan.Create(probabilities, context); using var second = Vp8RatePlan.Create(ArithmeticProbabilities(), context);
        using var hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
        AppendWords(hash, first.Values); AppendWords(hash, second.EndOfBlock); AppendWords(hash, second.VariableLevels);
        Assert.Equal("58dfedadf26f4d9e6d3c6e7c088d6c149df3108e42d9a9c5814a7a4d0ace17bf", Convert.ToHexStringLower(hash.GetHashAndReset()));
        Assert.Equal(20_192, first.CapacityBytes); Assert.Equal(2 * first.CapacityBytes, root.Bytes);
        Assert.Equal(root.Bytes + 1024, root.Peak); var original = first.Values.ToArray(); probabilities.AsSpan().Clear();
        Assert.True(first.Values.SequenceEqual(original)); // No borrowed probability input is retained.
        first.Dispose(); first.Dispose(); Assert.Equal(second.CapacityBytes, root.Bytes);
        Assert.Throws<ObjectDisposedException>(() => first.Values.Length); second.Dispose(); Assert.Equal(0, root.Bytes); Assert.False(root.IsDisposed);
    }

    private static void AppendResidual(Span<short> levels, int type, int first, Vp8RatePlan defaults,
        Vp8RatePlan arithmetic, IncrementalHash hash)
    {
        Span<byte> bytes = stackalloc byte[24]; Span<short> copy = stackalloc short[16]; levels[..16].CopyTo(copy);
        for (var profile = 0; profile < 2; profile++) for (var ctx = 0; ctx < 3; ctx++)
        {
            var cost = (profile == 0 ? defaults : arithmetic).Residual(levels, type, first, ctx, out var nonzero);
            Assert.Equal(levels[first..16].ContainsAnyExcept((short)0), nonzero);
            BinaryPrimitives.WriteInt32LittleEndian(bytes[((profile * 3 + ctx) * 4)..], cost);
        }
        Assert.True(levels[..16].SequenceEqual(copy)); hash.AppendData(bytes);
    }

    private static void AppendBlock(Span<short> coefficients, Span<short> levels, Vp8QuantizationMatrix matrix,
        int type, bool acCompanion, Vp8RatePlan defaults, Vp8RatePlan arithmetic, IncrementalHash hash)
    {
        Vp8BlockQuantization.Apply(coefficients, levels, matrix);
        AppendResidual(levels, type, 0, defaults, arithmetic, hash);
        if (acCompanion)
        {
            var dc = levels[0]; levels[0] = 0; AppendResidual(levels, 0, 1, defaults, arithmetic, hash); levels[0] = dc;
        }
    }

    private static void AppendPlane(ReadOnlySpan<byte> plane, int stride, Vp8QuantizationMatrix matrix, int type,
        bool acCompanion, Vp8RatePlan defaults, Vp8RatePlan arithmetic, IncrementalHash hash, OperationContext context)
    {
        Span<short> residual = stackalloc short[16]; Span<short> coefficients = stackalloc short[16]; Span<short> levels = stackalloc short[16];
        for (var y = 0; y < plane.Length / stride; y += 4) for (var x = 0; x < stride; x += 4)
        {
            for (var dy = 0; dy < 4; dy++) for (var dx = 0; dx < 4; dx++) residual[dy * 4 + dx] = (short)(plane[(y + dy) * stride + x + dx] - 128);
            Vp8ForwardTransform.Dct(residual, coefficients); AppendBlock(coefficients, levels, matrix, type, acCompanion, defaults, arithmetic, hash);
        }
    }

    [Fact]
    public void EveryNativeResidualCostMatchesForReusedOpaqueCoefficientsAndSparseBoundary()
    {
        using var root = new TestMemoryBudget(16_000_000); using var image = OriginalVp8lControls.Buffer("opaque", 8, root);
        using var context = new OperationContext(new() { MemoryBudget = root }, default);
        using var input = Vp8PreparedInput.Create(image, context); using var analysis = Vp8SourceAnalysis.Create(input, context);
        using var segments = Vp8SourceSegments.Create(analysis, 75, false, context); using var quantization = Vp8QuantizationPlan.Create(segments, analysis.MeanChromaScore, context);
        using var defaults = Vp8RatePlan.Create(Vp8Tables.CoefficientProbabilities, context); using var arithmetic = Vp8RatePlan.Create(ArithmeticProbabilities(), context);
        using var hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256); var retained = root.Bytes; var original = Vp8PreparedInputTests.Hash(input);
        Span<int> minimum = stackalloc int[80];
        for (var p = 0; p < 16; p++)
        {
            var bias = (p == 0 ? 96 : 110) << 9;
            minimum[p] = 4; minimum[16 + p] = (1 << 17) / 4; minimum[32 + p] = bias;
            minimum[48 + p] = ((1 << 17) - 1 - bias) / ((1 << 17) / 4); minimum[64 + p] = 0;
        }
        Span<short> coefficients = stackalloc short[16]; Span<short> levels = stackalloc short[16];
        for (var kind = 0; kind < 4; kind++)
        {
            var matrix = kind == 3 ? new Vp8QuantizationMatrix(minimum) : quantization.Matrix(0, (Vp8CoefficientPlane)kind);
            for (var fragment = 0; fragment < 11; fragment++)
            {
                for (var p = 0; p < 16; p++) coefficients[p] = (short)(fragment switch
                {
                    0 => 0, 1 => 16320, 2 => -16320, 3 => (p / 4 + p % 4) % 2 == 0 ? 16320 : -16320,
                    10 => (p + 1) * 73 * (p % 2 == 0 ? 1 : -1),
                    _ => Math.Max(0, matrix.ZeroThresholds[p] - matrix.Sharpening[p] + (fragment - 4) / 2 - 1) * (fragment % 2 == 0 ? 1 : -1)
                });
                AppendBlock(coefficients, levels, matrix, kind == 0 || kind == 3 ? 3 : kind, false, defaults, arithmetic, hash);
            }
        }
        AppendPlane(input.Y, input.Stride, quantization.Matrix(0, Vp8CoefficientPlane.Luma), 3, true, defaults, arithmetic, hash, context);
        Span<short> residual = stackalloc short[16]; Span<short> dc = stackalloc short[16]; Span<short> dct = stackalloc short[16];
        for (var y = 0; y < input.StorageHeight; y += 16) for (var x = 0; x < input.Stride; x += 16)
        {
            for (var by = 0; by < 16; by += 4) for (var bx = 0; bx < 16; bx += 4)
            {
                for (var dy = 0; dy < 4; dy++) for (var dx = 0; dx < 4; dx++) residual[dy * 4 + dx] = (short)(input.Y[(y + by + dy) * input.Stride + x + bx + dx] - 128);
                Vp8ForwardTransform.Dct(residual, dct); dc[by / 4 * 4 + bx / 4] = dct[0];
            }
            Vp8ForwardTransform.WalshDc(dc, coefficients); AppendBlock(coefficients, levels, quantization.Matrix(0, Vp8CoefficientPlane.Walsh), 1, false, defaults, arithmetic, hash);
        }
        var chroma = quantization.Matrix(0, Vp8CoefficientPlane.Chroma);
        AppendPlane(input.U, input.Stride / 2, chroma, 2, false, defaults, arithmetic, hash, context);
        AppendPlane(input.V, input.Stride / 2, chroma, 2, false, defaults, arithmetic, hash, context);
        short[] sparse = [0, 1, -2, 4, -5, 6, -7, 10, -11, 18, -19, 34, -35, 66, -67, 2047];
        AppendResidual(sparse, 3, 0, defaults, arithmetic, hash);
        Assert.Equal("060287472f05753d8361860c7f4ee8e78fdbca724b2f51dac7e53ea9a8cc7569", Convert.ToHexStringLower(hash.GetHashAndReset()));
        Assert.Equal(retained, root.Bytes); Assert.Equal(original, Vp8PreparedInputTests.Hash(input));
    }

    [Fact]
    public void RefusalAtOwnedOrScratchReservationIsTerminalAndRefundsCapacity()
    {
        foreach (var available in new[] { 20_191, 21_215 })
        {
            using var root = new TestMemoryBudget(available); using var context = new OperationContext(new() { MemoryBudget = root }, default);
            var error = Assert.Throws<ImageLimitException>(() => Vp8RatePlan.Create(Vp8Tables.CoefficientProbabilities, context));
            Assert.Equal((ImageLimitKind.WorkingBytes, ImageLimitScope.SharedBudget), (error.Limit, error.Scope));
            Assert.Equal(0, root.Bytes); Assert.Equal(1, root.Refusals); Assert.False(root.IsDisposed);
            using var later = new OperationContext(new() { MemoryBudget = root }, default); later.Reserve(available); later.Release(available);
        }
    }

    [Fact]
    public void RetainedPlanAndReplacementChargeOneRootAndKeepIndependentLifetimes()
    {
        using var root = new TestMemoryBudget(41_407); using var caller = root.CreateScope();
        using var firstContext = new OperationContext(new() { MemoryBudget = root }, default);
        using var first = Vp8RatePlan.Create(Vp8Tables.CoefficientProbabilities, firstContext); var original = first.Values.ToArray();
        using var nextContext = new OperationContext(new() { MemoryBudget = root }, default);
        Assert.Throws<ImageLimitException>(() => Vp8RatePlan.Create(Vp8Tables.CoefficientProbabilities, nextContext));
        Assert.Equal(first.CapacityBytes, root.Bytes); Assert.True(first.Values.SequenceEqual(original)); first.Dispose();
        using var next = Vp8RatePlan.Create(Vp8Tables.CoefficientProbabilities, nextContext); Assert.True(next.Values.SequenceEqual(original));
        Assert.Equal(next.CapacityBytes, root.Bytes); next.Dispose(); Assert.Equal(0, root.Bytes); Assert.False(root.IsDisposed);
    }

    [Fact]
    public void LocalWorkspaceLimitRemainsIndependentAndInvalidInputsLeaveSourcesUntouched()
    {
        using var root = new TestMemoryBudget(1_000_000); using var limited = new OperationContext(new() { MemoryBudget = root, MaxWorkingBytes = 21_215 }, default);
        var error = Assert.Throws<ImageLimitException>(() => Vp8RatePlan.Create(Vp8Tables.CoefficientProbabilities, limited));
        Assert.Equal((ImageLimitKind.WorkingBytes, ImageLimitScope.Operation), (error.Limit, error.Scope)); Assert.Equal(0, root.Bytes); Assert.Equal(0, root.Refusals);
        using var context = new OperationContext(new() { MemoryBudget = root }, default);
        Assert.Throws<ArgumentException>(() => Vp8RatePlan.Create(new byte[1055], context)); Assert.Equal(0, root.Bytes);
        using var plan = Vp8RatePlan.Create(Vp8Tables.CoefficientProbabilities, context); var levels = new short[18]; levels[16] = 30000; levels[17] = -30000;
        Assert.Throws<ArgumentException>(() => plan.Residual(levels, 4, 0, 0, out _));
        Assert.Throws<ArgumentException>(() => plan.Residual(levels, 0, 2, 0, out _));
        Assert.Throws<ArgumentException>(() => plan.Residual(levels, 0, 0, 3, out _));
        Assert.Throws<ArgumentException>(() => plan.Residual(new short[15], 0, 0, 0, out _));
        levels[0] = 1; Assert.Throws<ArgumentException>(() => plan.Residual(levels, 0, 1, 0, out _));
        levels[0] = 0; levels[15] = short.MinValue; Assert.Throws<ArgumentException>(() => plan.Residual(levels, 0, 0, 0, out _));
        Assert.Equal(short.MinValue, levels[15]); Assert.Equal((30000, -30000), ((int)levels[16], (int)levels[17]));
    }
}
