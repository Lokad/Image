using Lokad.Image.Internal;

namespace Lokad.Image.Tests;

public sealed partial class Vp8TrellisQuantizationTests
{
    [Theory]
    [InlineData(0, 0)] [InlineData(0, 1)] [InlineData(0, 2)]
    [InlineData(1, 0)] [InlineData(1, 1)] [InlineData(1, 2)]
    public void FourModesMatchProductionSingleSearchSignedFragments(int profile, int initialContext)
    {
        using var input = new Inputs(profile);
        var matrix = input.Quantization.Matrix(0, Vp8CoefficientPlane.Luma);
        var lambda = input.Quantization.Costs(0).Luma4Trellis;
        Span<ushort> weights = stackalloc ushort[16]; Vp8TrellisQuantization.Weights(weights);
        Span<short> scratch = stackalloc short[68]; scratch.Fill(-23456);
        Span<short> coefficients = stackalloc short[72]; coefficients.Fill(12345);
        Span<short> levels = stackalloc short[72]; levels.Fill(-12345);
        Span<short> expected = stackalloc short[64]; Span<short> expectedLevels = stackalloc short[64];
        Span<short> residual = stackalloc short[16];
        var before = input.Root.Bytes; input.Context.Reserve(Vp8MacroblockDecision.ScratchBytes);
        try
        {
            var retained = input.Root.Bytes;
            var execution = new Vp8TrellisQuantization.Execution(matrix, input.Rate, weights, input.Context, scratch.Slice(2, 64));
            for (var group = 0; group < 36; group++)
            {
                for (var lane = 0; lane < 4; lane++)
                {
                    var fragment = (group + lane * 9) % 36;
                    for (var p = 0; p < 16; p++) residual[p] = (short)(fragment switch
                    {
                        1 => 255, 2 => -255, 3 => (p / 4 + p % 4) % 2 == 0 ? 255 : -255,
                        >= 4 when p == (fragment - 4) / 2 => (fragment - 4) % 2 == 0 ? 255 : -255, _ => 0
                    });
                    Vp8ForwardTransform.Dct(residual, coefficients.Slice(4 + lane * 16, 16));
                }
                coefficients.Slice(4, 64).CopyTo(expected); expectedLevels.Fill(77); levels.Slice(4, 64).Fill(77);
                uint expectedMask = 0;
                for (var lane = 0; lane < 4; lane++)
                    if (execution.Apply(expected.Slice(lane * 16, 16), expectedLevels.Slice(lane * 16, 16), 3, initialContext, lambda))
                        expectedMask |= 1u << lane;
                var actualMask = execution.ApplyFourLuma4(coefficients.Slice(4, 64), levels.Slice(4, 64), initialContext, lambda);
                Assert.Equal(expectedMask, actualMask);
                Assert.True(expected.SequenceEqual(coefficients.Slice(4, 64)), $"Coefficient mismatch at group {group}.");
                Assert.True(expectedLevels.SequenceEqual(levels.Slice(4, 64)), $"Level mismatch at group {group}.");
                Assert.Equal(retained, input.Root.Bytes);
                Assert.False(coefficients[..4].ContainsAnyExcept((short)12345));
                Assert.False(coefficients[68..].ContainsAnyExcept((short)12345));
                Assert.False(levels[..4].ContainsAnyExcept((short)-12345));
                Assert.False(levels[68..].ContainsAnyExcept((short)-12345));
                Assert.Equal((-23456, -23456, -23456, -23456), ((int)scratch[0], (int)scratch[1], (int)scratch[66], (int)scratch[67]));
            }
        }
        finally { input.Context.Release(Vp8MacroblockDecision.ScratchBytes); }
        Assert.Equal(before, input.Root.Bytes);
    }

    [Theory]
    [InlineData(0)] [InlineData(63)] [InlineData(127)]
    public void FourModesMatchSingleAtQuantizerAndCoefficientBoundaries(int quantizer)
    {
        using var input = new Inputs(1);
        Span<int> matrix = stackalloc int[80]; matrix.Clear();
        for (var p = 0; p < 16; p++)
        {
            var step = p == 0 ? Vp8Tables.DcQuantizers[quantizer] : Vp8Tables.AcQuantizers[quantizer];
            matrix[p] = step; matrix[16 + p] = (1 << 17) / step;
            matrix[64 + p] = (30 * Math.Min(3, (p & 3) + (p >> 2)) * step) >> 11;
        }
        ReadOnlySpan<short> samples = [-2040, -1020, -255, -16, -1, 0, 1, 16, 255, 1020, 2040];
        Span<ushort> weights = stackalloc ushort[16]; Vp8TrellisQuantization.Weights(weights);
        Span<short> scratch = stackalloc short[64]; Span<short> coefficients = stackalloc short[64];
        Span<short> levels = stackalloc short[64]; Span<short> expected = stackalloc short[64];
        Span<short> expectedLevels = stackalloc short[64];
        input.Context.Reserve(Vp8MacroblockDecision.ScratchBytes);
        try
        {
            var execution = new Vp8TrellisQuantization.Execution(new(matrix), input.Rate, weights, input.Context, scratch);
            for (var initial = 0; initial < 3; initial++)
            foreach (var lambda in new[] { 1, 1400, 66654 })
            {
                for (var p = 0; p < 64; p++) coefficients[p] = samples[(p * 7 + initial * 3) % samples.Length];
                coefficients.CopyTo(expected); expectedLevels.Fill(77); levels.Fill(77);
                uint expectedMask = 0;
                for (var lane = 0; lane < 4; lane++)
                    if (execution.Apply(expected.Slice(lane * 16, 16), expectedLevels.Slice(lane * 16, 16), 3, initial, lambda))
                        expectedMask |= 1u << lane;
                Assert.Equal(expectedMask, execution.ApplyFourLuma4(coefficients, levels, initial, lambda));
                Assert.True(expected.SequenceEqual(coefficients)); Assert.True(expectedLevels.SequenceEqual(levels));
            }
            coefficients.Clear(); levels.Fill(77);
            Assert.Equal(0u, execution.ApplyFourLuma4(coefficients, levels, 0, 1400));
            Assert.False(coefficients.ContainsAnyExcept((short)0)); Assert.False(levels.ContainsAnyExcept((short)0));
        }
        finally { input.Context.Release(Vp8MacroblockDecision.ScratchBytes); }
    }

    [Theory]
    [InlineData(0, false)] [InlineData(0, true)]
    [InlineData(255, false)] [InlineData(255, true)]
    public void FourModesPreserveCancellationAndDisposedOwnerBeforeMutation(int coefficient, bool canceled)
    {
        using var input = new Inputs(0); using var cancellation = new CancellationTokenSource();
        using var context = new OperationContext(new() { MemoryBudget = input.Root }, cancellation.Token);
        Span<ushort> weights = stackalloc ushort[16]; Vp8TrellisQuantization.Weights(weights);
        Span<short> scratch = stackalloc short[64]; Span<short> coefficients = stackalloc short[64]; coefficients.Fill((short)coefficient);
        Span<short> levels = stackalloc short[64]; levels.Fill(77);
        var matrix = input.Quantization.Matrix(0, Vp8CoefficientPlane.Luma);
        context.Reserve(Vp8MacroblockDecision.ScratchBytes);
        try
        {
            var execution = new Vp8TrellisQuantization.Execution(matrix, input.Rate, weights, context, scratch);
            if (canceled) cancellation.Cancel(); else input.Rate.Dispose();
            var failed = false;
            try { execution.ApplyFourLuma4(coefficients, levels, 0, 1400); }
            catch (OperationCanceledException error) when (canceled)
            { Assert.Equal(cancellation.Token, error.CancellationToken); failed = true; }
            catch (ObjectDisposedException) when (!canceled) { failed = true; }
            Assert.True(failed);
            Assert.False(coefficients.ContainsAnyExcept((short)coefficient)); Assert.False(levels.ContainsAnyExcept((short)77));
        }
        finally { context.Release(Vp8MacroblockDecision.ScratchBytes); }
    }
}
