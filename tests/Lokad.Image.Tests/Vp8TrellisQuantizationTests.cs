using System.Buffers.Binary;
using System.Security.Cryptography;
using System.Runtime.Intrinsics.X86;
using Lokad.Image.Internal;
using Lokad.Image.Tooling;

namespace Lokad.Image.Tests;

public sealed class Vp8TrellisQuantizationTests
{
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
            Assert.Equal(expected, Vp8TrellisQuantization.ZeroCandidates(coefficients.AsSpan(offset, 17),
                reciprocals.AsSpan(offset, 17), sharpening.AsSpan(offset, 17), first));
            Assert.Equal(original, coefficients); Assert.Equal(oldReciprocals, reciprocals); Assert.Equal(oldSharpening, sharpening);
        }
        for (var i = 0; i < 32; i++) _ = Vp8TrellisQuantization.ZeroCandidates(coefficients, reciprocals, sharpening, first);
        var before = GC.GetAllocatedBytesForCurrentThread();
        for (var i = 0; i < 64; i++) _ = Vp8TrellisQuantization.ZeroCandidates(coefficients, reciprocals, sharpening, first);
        Assert.Equal(before, GC.GetAllocatedBytesForCurrentThread());
    }

    [Fact]
    public void ZeroCandidateSearchPreservesValidationAndCancellationBeforeMutation()
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
            _ = Vp8TrellisQuantization.Apply(coefficients, destination, input.Quantization.Matrix(0, Vp8CoefficientPlane.Luma),
                input.Rate, weights, 3, 0, 1, operation);
        }
        var error = Assert.Throws<OperationCanceledException>(() => Apply(levels));
        Assert.Equal(cancellation.Token, error.CancellationToken);
        weights[0]--; Assert.Throws<ArgumentException>(() => Apply(levels)); weights[0]++;
        coefficients[15] = 2041; Assert.Throws<ArgumentOutOfRangeException>(() => Apply(levels)); coefficients[15] = 1;
        Assert.Throws<ArgumentException>(() => Apply(coefficients));
        Assert.Equal(oldCoefficients, coefficients); Assert.Equal(oldLevels, levels); Assert.Equal(retained, input.Root.Bytes);
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
            for (var fragment = 0; fragment < 36; fragment++)
            for (var initial = 0; initial < 3; initial++)
            {
                for (var p = 0; p < 16; p++) residual[p] = (short)(fragment switch
                {
                    1 => 255, 2 => -255, 3 => (p / 4 + p % 4) % 2 == 0 ? 255 : -255,
                    >= 4 when p == (fragment - 4) / 2 => (fragment - 4) % 2 == 0 ? 255 : -255, _ => 0
                });
                Vp8ForwardTransform.Dct(residual, coefficients); levels[..16].Fill(77); var dc = coefficients[0];
                var nonzero = Vp8TrellisQuantization.Apply(coefficients, levels, matrix, input.Rate, weights, type, initial, lambda, context);
                if (type == 0) { Assert.Equal(dc, coefficients[0]); Assert.Equal(77, levels[0]); }
                Assert.Equal(nonzero, levels.Slice(type == 0 ? 1 : 0, type == 0 ? 15 : 16).ContainsAnyExcept((short)0));
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
