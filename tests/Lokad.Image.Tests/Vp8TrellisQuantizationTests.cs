using System.Buffers.Binary;
using System.Numerics;
using System.Security.Cryptography;
using System.Runtime.Intrinsics.X86;
using Lokad.Image.Internal;
using Lokad.Image.Tooling;

namespace Lokad.Image.Tests;

public sealed class Vp8TrellisQuantizationTests
{
    [Theory]
    [InlineData(0)] [InlineData(63)] [InlineData(127)]
    public void CandidatePreparationKeepsSignedSamplesWideWeightsAndBorrowedGuards(int quantizer)
    {
        using var input = new Inputs(0);
        Span<int> matrix = stackalloc int[80]; Span<ushort> weights = stackalloc ushort[16];
        ReadOnlySpan<short> values = [short.MinValue, -32767, -5000, -2048, -1, 0, 1, 2,
            127, 128, 2047, 2048, 5000, 16320, 32766, short.MaxValue];
        Span<short> coefficients = stackalloc short[20]; coefficients.Fill(12345); values.CopyTo(coefficients[2..]);
        Span<short> scratch = stackalloc short[64];
        Span<int> floors = stackalloc int[20]; floors.Fill(-123456);
        Span<long> delta0 = stackalloc long[20]; delta0.Fill(123456789012345);
        Span<long> delta1 = stackalloc long[20]; delta1.Fill(-123456789012345);
        for (var p = 0; p < 16; p++)
        {
            var step = p == 0 ? Vp8Tables.DcQuantizers[quantizer] : Vp8Tables.AcQuantizers[quantizer];
            matrix[p] = step; matrix[16 + p] = (1 << 17) / step;
            matrix[64 + p] = (30 * Math.Min(3, (p & 3) + (p >> 2)) * step) >> 11;
            weights[p] = (p & 3) switch { 0 => 0, 1 => 1, 2 => 30, _ => ushort.MaxValue };
        }
        var execution = new Vp8TrellisQuantization.Execution(new(matrix), input.Rate, weights, input.Context, scratch);
        var mask = Vp8TrellisQuantization.PrepareCandidates(coefficients.Slice(2, 16), in execution,
            floors.Slice(2, 16), delta0.Slice(2, 16), delta1.Slice(2, 16));
        uint expectedMask = 0;
        for (var p = 0; p < 16; p++)
        {
            var magnitude = BigInteger.Abs(values[p]) + matrix[64 + p];
            var product = magnitude * matrix[16 + p];
            var floor = (int)BigInteger.Min(2047, product >> 17);
            var ceiling = (int)BigInteger.Min(2047, (product + 65536) >> 17);
            Assert.Equal(floor, floors[p + 2]);
            if (ceiling > floor) expectedMask |= 1u << p;
            for (var choice = 0; choice < 2; choice++)
            {
                var error = magnitude - (floor + choice) * matrix[p];
                var expected = (long)((error * error - magnitude * magnitude) * weights[p] * 256);
                Assert.Equal(expected, choice == 0 ? delta0[p + 2] : delta1[p + 2]);
            }
        }
        Assert.Equal(expectedMask, mask);
        Assert.True(values.SequenceEqual(coefficients.Slice(2, 16)));
        foreach (var p in new[] { 0, 1, 18, 19 })
        {
            Assert.Equal(12345, coefficients[p]); Assert.Equal(-123456, floors[p]);
            Assert.Equal(123456789012345, delta0[p]); Assert.Equal(-123456789012345, delta1[p]);
        }
    }

    [Theory]
    [InlineData(0)] [InlineData(1)]
    public void ZeroCandidateCertificateMatchesWrappedIntegerProductsAndKeepsBorrowedTails(int first)
    {
        if (!Avx2.IsSupported) return; // The production caller keeps its scalar search.
        var coefficients = new short[20]; var reciprocals = new int[20]; var sharpening = new int[20];
        int[] boundaries = [int.MinValue, -65537, -65536, -1, 0, 1, 65534, 65535, 65536, 65537, 131071, 131072, int.MaxValue];
        foreach (var offset in new[] { 1, 3 })
        foreach (var value in boundaries)
        foreach (var bias in new[] { -1, 0, 1, int.MinValue, int.MaxValue })
        for (var lane = 0; lane < 16; lane++)
        {
            Array.Fill(coefficients, (short)1); Array.Fill(reciprocals, 1000); Array.Clear(sharpening);
            coefficients[offset + lane] = -1; reciprocals[offset + lane] = value; sharpening[offset + lane] = bias;
            var expected = true;
            for (var p = first; p < 16; p++)
            {
                var product = unchecked((uint)(((long)Math.Abs((int)coefficients[offset + p]) + sharpening[offset + p]) * reciprocals[offset + p]));
                if (product > 65535) expected = false;
            }
            var original = coefficients.ToArray(); var oldReciprocals = reciprocals.ToArray(); var oldSharpening = sharpening.ToArray();
            Assert.Equal(expected, new Vp8TrellisQuantization.ZeroTest(reciprocals.AsSpan(offset, 17), sharpening.AsSpan(offset, 17)).AreZero(coefficients.AsSpan(offset, 17), first));
            Assert.Equal(original, coefficients); Assert.Equal(oldReciprocals, reciprocals); Assert.Equal(oldSharpening, sharpening);
        }
        var zero = new Vp8TrellisQuantization.ZeroTest(reciprocals, sharpening);
        for (var i = 0; i < 32; i++) _ = zero.AreZero(coefficients, first);
        var before = GC.GetAllocatedBytesForCurrentThread();
        for (var i = 0; i < 64; i++) _ = zero.AreZero(coefficients, first);
        Assert.Equal(before, GC.GetAllocatedBytesForCurrentThread());
    }

    [Fact]
    public void ZeroCandidateSearchPreservesCancellationBeforeMutation()
    {
        using var input = new Inputs(0);
        var coefficients = Enumerable.Repeat((short)1, 18).ToArray(); var levels = Enumerable.Repeat((short)77, 18).ToArray();
        var weights = new ushort[16]; Vp8TrellisQuantization.Weights(weights);
        using var cancellation = new CancellationTokenSource();
        using var operation = new OperationContext(new() { MemoryBudget = input.Root }, cancellation.Token);
        var oldCoefficients = coefficients.ToArray(); var oldLevels = levels.ToArray(); var retained = input.Root.Bytes;
        cancellation.Cancel();
        void Apply(short[] destination)
        {
            _ = new Vp8TrellisQuantization.Execution(input.Quantization.Matrix(0, Vp8CoefficientPlane.Luma), input.Rate, weights, operation, stackalloc short[64]).Apply(coefficients, destination, 3, 0, 1);
        }
        var error = Assert.Throws<OperationCanceledException>(() => Apply(levels));
        Assert.Equal(cancellation.Token, error.CancellationToken);
        Assert.Equal(oldCoefficients, coefficients); Assert.Equal(oldLevels, levels); Assert.Equal(retained, input.Root.Bytes);
    }

    [Theory] [InlineData(0, false)] [InlineData(2047, false)] [InlineData(0, true)]
    public void DisposedRateAndCancellationPrecedeZeroOrNonzeroMutation(int coefficient, bool canceled)
    {
        using var input = new Inputs(0); using var cancellation = new CancellationTokenSource();
        using var operation = new OperationContext(new() { MemoryBudget = input.Root }, cancellation.Token);
        var coefficients = Enumerable.Repeat((short)coefficient, 18).ToArray();
        var levels = Enumerable.Repeat((short)77, 18).ToArray();
        var original = coefficients.ToArray(); var oldLevels = levels.ToArray();
        var weights = new ushort[16]; Vp8TrellisQuantization.Weights(weights);
        input.Rate.Dispose(); var retained = input.Root.Bytes; var reservations = input.Root.GrowthAttempts;
        if (canceled) cancellation.Cancel();
        void Apply() => new Vp8TrellisQuantization.Execution(input.Quantization.Matrix(0, Vp8CoefficientPlane.Luma), input.Rate, weights, operation, stackalloc short[64]).Apply(coefficients, levels, 3, 0, 1);
        if (canceled) Assert.Equal(cancellation.Token, Assert.Throws<OperationCanceledException>(Apply).CancellationToken);
        else Assert.Throws<ObjectDisposedException>(Apply);
        Assert.Equal(original, coefficients); Assert.Equal(oldLevels, levels);
        Assert.Equal(retained, input.Root.Bytes); Assert.Equal(reservations, input.Root.GrowthAttempts);
        Assert.False(input.Root.IsDisposed);
    }

    [Theory] [InlineData(0)] [InlineData(2047)]
    public void PreparedExecutionRejectsOwnerDisposalBeforeAnyMutation(int coefficient)
    {
        using var input = new Inputs(0);
        var coefficients = Enumerable.Repeat((short)coefficient, 18).ToArray();
        var levels = Enumerable.Repeat((short)77, 18).ToArray();
        var original = coefficients.ToArray(); var oldLevels = levels.ToArray();
        var weights = new ushort[16]; Vp8TrellisQuantization.Weights(weights);
        var retained = input.Root.Bytes - input.Rate.CapacityBytes; var reservations = input.Root.GrowthAttempts;
        void Apply()
        {
            var execution = new Vp8TrellisQuantization.Execution(input.Quantization.Matrix(0, Vp8CoefficientPlane.Luma), input.Rate, weights, input.Context, stackalloc short[64]);
            input.Rate.Dispose(); execution.Apply(coefficients, levels, 3, 0, 1);
        }
        Assert.Throws<ObjectDisposedException>(Apply);
        Assert.Equal(original, coefficients); Assert.Equal(oldLevels, levels);
        Assert.Equal(retained, input.Root.Bytes); Assert.Equal(reservations, input.Root.GrowthAttempts);
    }

    private sealed class Inputs : IDisposable
    {
        internal readonly TestMemoryBudget Root = new(16_000_000);
        internal readonly OperationContext Context;
        private readonly ImageBuffer image;
        private readonly Vp8PreparedInput input;
        private readonly Vp8SourceAnalysis analysis;
        private readonly Vp8SourceSegments segments;
        internal readonly Vp8QuantizationPlan Quantization;
        internal readonly Vp8RatePlan Rate;
        internal Inputs(int profile)
        {
            Context = new(new() { MemoryBudget = Root }, default); image = OriginalVp8lControls.Buffer("diagram", 8, Root);
            input = Vp8PreparedInput.Create(image, Context); analysis = Vp8SourceAnalysis.Create(input, Context);
            segments = Vp8SourceSegments.Create(analysis, 75, false, Context);
            Quantization = Vp8QuantizationPlan.Create(segments, analysis.MeanChromaScore, Context, 5);
            var probabilities = Vp8Tables.CoefficientProbabilities.ToArray();
            if (profile != 0) for (var p = 0; p < probabilities.Length; p++) probabilities[p] = (byte)((37 * p + 17 * (p / 11) + p / 33) & 255);
            Rate = Vp8RatePlan.Create(probabilities, Context);
        }
        public void Dispose() { Rate.Dispose(); Quantization.Dispose(); segments.Dispose(); analysis.Dispose(); input.Dispose(); image.Dispose(); Context.Dispose(); Root.Dispose(); }
    }

    // Reuse the existing36 signed zero/extreme/unit transform fragments and
    // two probability profiles. Expected output comes from our observer only
    // after its complete method5/6 packets matched the frozen7026/6818B targets.
    [Theory]
    [InlineData(0, 0, "53911b000188cf148a0696c8f72febae2d677be3ea7d2fd62d4f1d45dff44f62")]
    [InlineData(0, 3, "9da1da443994cdafd155dabff5666cf82ba3363c0c1e4de8ee2b36b4e2decbde")]
    [InlineData(1, 0, "41eff35731aed0268f7cba207ec6960b58077e539032494e6fd6b83d6344632d")]
    [InlineData(1, 3, "f6c5bea511a469f8648343406161ed155ff144f29a168b91c28e4a44e09277cb")]
    public void SignedFragmentsMatchAllNativeLevelsDequantizedCoefficientsAndContexts(int profile, int type, string expected)
    {
        using var input = new Inputs(profile); var context = input.Context; var matrix = input.Quantization.Matrix(0, Vp8CoefficientPlane.Luma);
        var lambda = type == 0 ? input.Quantization.Costs(0).Luma16Trellis : input.Quantization.Costs(0).Luma4Trellis;
        Assert.Equal(type == 0 ? 961 : 1400, lambda);
        Span<byte> matrixBytes = stackalloc byte[320];
        for (var p = 0; p < 80; p++) BinaryPrimitives.WriteInt32LittleEndian(matrixBytes[(p * 4)..], matrix.Values[p]);
        Assert.Equal("3fd59f74e4a6b8aeb5935a0e24fd91474aa5438ea548669fed91b1cdfb9446a9", Convert.ToHexStringLower(SHA256.HashData(matrixBytes)));
        Span<ushort> weights = stackalloc ushort[18]; weights[16] = 91; weights[17] = 92; Vp8TrellisQuantization.Weights(weights);
        Span<short> residual = stackalloc short[16]; Span<short> coefficients = stackalloc short[18]; Span<short> levels = stackalloc short[18];
        coefficients[16] = 30000; coefficients[17] = -30000; levels[16] = 97; levels[17] = -98;
        using var hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256); Span<byte> record = stackalloc byte[65];
        context.Reserve(Vp8TrellisQuantization.ScratchBytes);
        try
        {
            var retained = input.Root.Bytes;
            Span<short> scratch = stackalloc short[68]; scratch.Fill(-23456);
            var execution = new Vp8TrellisQuantization.Execution(matrix, input.Rate, weights, context, scratch.Slice(2, 64));
            for (var fragment = 0; fragment < 36; fragment++)
            for (var initial = 0; initial < 3; initial++)
            {
                for (var p = 0; p < 16; p++) residual[p] = (short)(fragment switch
                {
                    1 => 255, 2 => -255, 3 => (p / 4 + p % 4) % 2 == 0 ? 255 : -255,
                    >= 4 when p == (fragment - 4) / 2 => (fragment - 4) % 2 == 0 ? 255 : -255, _ => 0
                });
                Vp8ForwardTransform.Dct(residual, coefficients); levels[..16].Fill(77); var dc = coefficients[0];
                var nonzero = execution.Apply(coefficients, levels, type, initial, lambda);
                if (type == 0) { Assert.Equal(dc, coefficients[0]); Assert.Equal(77, levels[0]); }
                Assert.Equal(nonzero, levels.Slice(type == 0 ? 1 : 0, type == 0 ? 15 : 16).ContainsAnyExcept((short)0));
                Assert.Equal(nonzero, coefficients.Slice(type == 0 ? 1 : 0, type == 0 ? 15 : 16).ContainsAnyExcept((short)0));
                Assert.Equal((-23456, -23456, -23456, -23456), ((int)scratch[0], (int)scratch[1], (int)scratch[66], (int)scratch[67]));
                record[0] = nonzero ? (byte)1 : (byte)0;
                for (var p = 0; p < 16; p++)
                {
                    BinaryPrimitives.WriteInt16LittleEndian(record[(1 + p * 2)..], coefficients[p]);
                    BinaryPrimitives.WriteInt16LittleEndian(record[(33 + p * 2)..], levels[p]);
                }
                hash.AppendData(record);
            }
            Assert.Equal(expected, Convert.ToHexStringLower(hash.GetHashAndReset())); Assert.Equal(retained, input.Root.Bytes);
            Assert.Equal((30000, -30000, 97, -98, 91, 92), ((int)coefficients[16], (int)coefficients[17], (int)levels[16], (int)levels[17], (int)weights[16], (int)weights[17]));
        }
        finally { context.Release(Vp8TrellisQuantization.ScratchBytes); }
    }
}
