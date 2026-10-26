using System.Buffers.Binary;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using Lokad.Image.Internal;
using Lokad.Image.Tooling;

namespace Lokad.Image.Tests;

public sealed class Vp8BlockQuantizationTests
{
    [Theory]
    [InlineData(1, 32767, 32767)]
    [InlineData(-2, 16384, -32768)]
    public void SignedWordEndpointsPublishAtEveryRasterAndDiagonalPosition(short input, int step, short expected)
    {
        int[] order = [0, 1, 4, 8, 5, 2, 3, 6, 9, 12, 13, 10, 7, 11, 14, 15];
        var values = new int[80]; var coefficients = new short[18]; var levels = new short[18];
        for (var p = 0; p < 16; p++) values[16 + p] = 131072;
        coefficients[16] = 30000; coefficients[17] = -30000; levels[16] = 97; levels[17] = -98;
        for (var position = 0; position < 16; position++)
        {
            Array.Fill(values, 1, 0, 16); values[position] = step;
            coefficients.AsSpan(0, 16).Clear(); coefficients[position] = input; levels.AsSpan(0, 16).Fill(29999);
            Assert.True(Vp8BlockQuantization.Apply(coefficients, levels, new(values)));
            for (var p = 0; p < 16; p++)
            {
                Assert.Equal(p == position ? expected : (short)0, coefficients[p]);
                Assert.Equal(order[p] == position ? input : (short)0, levels[p]);
            }
            Assert.Equal((30000, -30000, 97, -98), ((int)coefficients[16], (int)coefficients[17], (int)levels[16], (int)levels[17]));
        }
    }

    [Theory]
    [InlineData(8, 16384)]
    [InlineData(-12, 10923)]
    public void ReconstructionOnePastSignedWordEndpointsKeepsTheExactValidPrefix(short input, int step)
    {
        int[] order = [0, 1, 4, 8, 5, 2, 3, 6, 9, 12, 13, 10, 7, 11, 14, 15];
        for (var position = 0; position < 16; position++)
        {
            var values = new int[80]; var coefficients = Enumerable.Repeat((short)4, 16).ToArray();
            var levels = Enumerable.Repeat((short)29999, 16).ToArray();
            for (var p = 0; p < 16; p++) { values[p] = 1; values[16 + p] = 32768; }
            values[position] = step; coefficients[position] = input;
            Assert.Throws<OverflowException>(() => Vp8BlockQuantization.Apply(coefficients, levels, new(values)));
            for (var p = 0; p < 16; p++)
            {
                Assert.Equal(p == position ? input : p < position ? (short)1 : (short)4, coefficients[p]);
                Assert.Equal(order[p] < position ? (short)1 : (short)29999, levels[p]);
            }
        }
    }

    private static void MinimumStep(Span<int> values)
    {
        for (var p = 0; p < 16; p++)
        {
            var bias = (p == 0 ? 96 : 110) << 9;
            values[p] = 4; values[16 + p] = (1 << 17) / 4; values[32 + p] = bias;
            values[48 + p] = ((1 << 17) - 1 - bias) / ((1 << 17) / 4); values[64 + p] = 0;
        }
    }

    private static bool Append(Span<short> coefficients, Span<short> levels, Vp8QuantizationMatrix matrix, IncrementalHash hash)
    {
        var nonzero = Vp8BlockQuantization.Apply(coefficients, levels, matrix);
        Span<byte> bytes = stackalloc byte[65]; bytes[0] = nonzero ? (byte)1 : (byte)0;
        for (var p = 0; p < 16; p++)
        {
            BinaryPrimitives.WriteInt16LittleEndian(bytes[(1 + 2 * p)..], coefficients[p]);
            BinaryPrimitives.WriteInt16LittleEndian(bytes[(33 + 2 * p)..], levels[p]);
        }
        hash.AppendData(bytes); return nonzero;
    }

    [Theory]
    [InlineData(0, "e4d148c64007223065d096c87058763ab418ccde84a568885755f47b7b3c063d", 653)]
    [InlineData(1, "360e5d6e7a737709549545de8473aa5500e4e3e0abed7f97ed73e627b7f33c25", 340)]
    [InlineData(2, "f1420ae8955485b640473476a5c9a367a94d852e0035224ddc8b3091b9fdc42c", 709)]
    [InlineData(3, "e5dde916bdd05e2355ace2837233ddff32546f1e72e29441374db19ed50ba6dd", 2047)]
    public void SignedThresholdAndSaturationFragmentsMatchNative(int kind, string expected, int maximum)
    {
        using var image = OriginalVp8lControls.Buffer("opaque", 8); using var root = new TestMemoryBudget(16_000_000);
        using var context = new OperationContext(new() { MemoryBudget = root }, default);
        using var input = Vp8PreparedInput.Create(image, context); using var analysis = Vp8SourceAnalysis.Create(input, context);
        using var segments = Vp8SourceSegments.Create(analysis, 75, false, context);
        using var plan = Vp8QuantizationPlan.Create(segments, analysis.MeanChromaScore, context);
        Span<int> minimum = stackalloc int[80]; MinimumStep(minimum);
        var matrix = kind == 3 ? new Vp8QuantizationMatrix(minimum) : plan.Matrix(0, (Vp8CoefficientPlane)kind);
        Span<short> coefficients = stackalloc short[18]; Span<short> levels = stackalloc short[18];
        coefficients[16] = 30000; coefficients[17] = -30000; levels[16] = 97; levels[17] = -98;
        using var hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256); var nonzero = 0; var max = 0; var retained = root.Bytes;
        for (var fragment = 0; fragment < 11; fragment++)
        {
            for (var p = 0; p < 16; p++)
                coefficients[p] = (short)(fragment switch
                {
                    0 => 0, 1 => 16320, 2 => -16320,
                    3 => (p / 4 + p % 4) % 2 == 0 ? 16320 : -16320,
                    10 => (p + 1) * 73 * (p % 2 == 0 ? 1 : -1),
                    _ => Math.Max(0, matrix.ZeroThresholds[p] - matrix.Sharpening[p] + (fragment - 4) / 2 - 1) * (fragment % 2 == 0 ? 1 : -1)
                });
            if (Append(coefficients, levels, matrix, hash)) nonzero++;
            foreach (var level in levels[..16]) max = Math.Max(max, Math.Abs((int)level));
        }
        Assert.Equal(expected, Convert.ToHexStringLower(hash.GetHashAndReset())); Assert.Equal(6, nonzero); Assert.Equal(maximum, max);
        Assert.Equal((30000, -30000, 97, -98), ((int)coefficients[16], (int)coefficients[17], (int)levels[16], (int)levels[17]));
        Assert.Equal(retained, root.Bytes);
    }

    private static int AppendPlane(ReadOnlySpan<byte> plane, int stride, Vp8QuantizationMatrix matrix,
        Span<short> coefficients, Span<short> levels, IncrementalHash hash, OperationContext context)
    {
        Span<short> residual = stackalloc short[16]; var blocks = 0;
        for (var y = 0; y < plane.Length / stride; y += 4)
        for (var x = 0; x < stride; x += 4)
        {
            for (var dy = 0; dy < 4; dy++) for (var dx = 0; dx < 4; dx++)
                residual[dy * 4 + dx] = (short)(plane[(y + dy) * stride + x + dx] - 128);
            Vp8ForwardTransform.Dct(residual, coefficients); Append(coefficients, levels, matrix, hash); blocks++;
        }
        return blocks;
    }

    [Theory]
    [InlineData(0, 8976, "850ac3db2f0feefdb47ce273923c44af8c9446f6c0b2312bc305a34f367805ab")]
    [InlineData(1, 561, "34ddbb9460c2310c3dcb3ce5777137c46c8e360df25e30e445c471fb5187201c")]
    [InlineData(2, 4488, "d92d8a4be549be3c2c456b22e2ce603d20052d01200846634f54e2d3c9755d1b")]
    public void AllExistingOpaqueDctAndWalshBlocksMatchNative(int plane, int expectedBlocks, string expected)
    {
        using var image = OriginalVp8lControls.Buffer("opaque", 8); using var root = new TestMemoryBudget(16_000_000);
        using var context = new OperationContext(new() { MemoryBudget = root }, default);
        using var input = Vp8PreparedInput.Create(image, context); var original = Vp8PreparedInputTests.Hash(input);
        using var analysis = Vp8SourceAnalysis.Create(input, context); using var segments = Vp8SourceSegments.Create(analysis, 75, false, context);
        using var plan = Vp8QuantizationPlan.Create(segments, analysis.MeanChromaScore, context); var retained = root.Bytes;
        var matrix = plan.Matrix(0, (Vp8CoefficientPlane)plane);
        Span<short> coefficients = stackalloc short[18]; Span<short> levels = stackalloc short[18];
        coefficients[16] = 30000; coefficients[17] = -30000; levels[16] = 97; levels[17] = -98;
        using var hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256); var blocks = 0;
        if (plane == 0) blocks = AppendPlane(input.Y, input.Stride, matrix, coefficients, levels, hash, context);
        else if (plane == 2)
            blocks = AppendPlane(input.U, input.Stride / 2, matrix, coefficients, levels, hash, context)
                + AppendPlane(input.V, input.Stride / 2, matrix, coefficients, levels, hash, context);
        else
        {
            Span<short> residual = stackalloc short[16]; Span<short> dc = stackalloc short[16]; Span<short> dct = stackalloc short[16];
            for (var my = 0; my < input.StorageHeight; my += 16)
            for (var mx = 0; mx < input.Stride; mx += 16)
            {
                for (var by = 0; by < 16; by += 4) for (var bx = 0; bx < 16; bx += 4)
                {
                    for (var dy = 0; dy < 4; dy++) for (var dx = 0; dx < 4; dx++)
                        residual[dy * 4 + dx] = (short)(input.Y[(my + by + dy) * input.Stride + mx + bx + dx] - 128);
                    Vp8ForwardTransform.Dct(residual, dct); dc[by / 4 * 4 + bx / 4] = dct[0];
                }
                Vp8ForwardTransform.WalshDc(dc, coefficients); Append(coefficients, levels, matrix, hash); blocks++;
            }
        }
        Assert.Equal(expectedBlocks, blocks); Assert.Equal(expected, Convert.ToHexStringLower(hash.GetHashAndReset()));
        Assert.Equal((30000, -30000, 97, -98), ((int)coefficients[16], (int)coefficients[17], (int)levels[16], (int)levels[17]));
        Assert.Equal(original, Vp8PreparedInputTests.Hash(input)); Assert.Equal(retained, root.Bytes);
    }

    [Theory]
    [InlineData(-16321)] [InlineData(16321)]
    public void InvalidTransformRangeFailsBeforeMutatingEitherBuffer(short invalid)
    {
        var coefficients = new short[18]; coefficients[15] = invalid; var original = coefficients.ToArray();
        var levels = Enumerable.Repeat((short)77, 18).ToArray(); var values = new int[80]; MinimumStep(values);
        Assert.Throws<ArgumentOutOfRangeException>(() => Vp8BlockQuantization.Apply(coefficients, levels, new(values)));
        Assert.Equal(original, coefficients); Assert.All(levels, value => Assert.Equal((short)77, value));
    }

    [Fact]
    public void OverlappingCallerBuffersFailBeforeAnyMutation()
    {
        var data = Enumerable.Range(0, 32).Select(i => (short)i).ToArray(); var original = data.ToArray();
        var values = new int[80]; MinimumStep(values);
        Assert.Throws<ArgumentException>(() => Vp8BlockQuantization.Apply(data.AsSpan(0, 18), data.AsSpan(8, 18), new(values)));
        Assert.Equal(original, data);
    }

    [Fact]
    public void EntireCoefficientDomainAndInt32BoundariesRetainFrozenScalarLevels()
    {
        var coefficients = new short[18]; var levels = new short[18]; var values = new int[80];
        coefficients[16] = 30000; coefficients[17] = -30000; levels[16] = 97; levels[17] = -98;
        var packed = new byte[64]; uint state = 0x9e3779b9; var blocks = 0;
        using var hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
        int Next() { state ^= state << 13; state ^= state >> 17; state ^= state << 5; return (int)state; }
        void Check()
        {
            var nonzero = Vp8BlockQuantization.Apply(coefficients, levels, new(values)); var any = false;
            for (var p = 0; p < 16; p++)
            {
                // Unit reconstruction steps expose raster levels directly;
                // independently check diagonal placement and the return value.
                var x = p & 3; var y = p >> 2; var d = x + y;
                var before = d < 4 ? d * (d + 1) / 2 : 16 - (7 - d) * (8 - d) / 2;
                var rank = before + ((d & 1) != 0 ? Math.Min(3, d) - x : x - Math.Max(0, d - 3));
                Assert.Equal(coefficients[p], levels[rank]); any |= coefficients[p] != 0;
                BinaryPrimitives.WriteInt32LittleEndian(packed.AsSpan(4 * p), coefficients[p]);
            }
            Assert.Equal(any, nonzero); hash.AppendData(packed); blocks++;
        }
        for (var policy = 0; policy < 32; policy++)
        {
            for (var p = 0; p < 16; p++)
            {
                values[p] = 1;
                values[16 + p] = policy < 16 ? 131072 / (4 + policy * 7 + p) : Next();
                values[32 + p] = policy < 16 ? (96 + p) << 9 : Next();
                values[48 + p] = policy < 16 ? policy + p : Next();
                values[64 + p] = policy < 16 ? policy * p : Next();
            }
            var saved = values.ToArray();
            for (var value = -16320; value <= 16320; value += 16)
            {
                for (var p = 0; p < 16; p++) coefficients[p] = (short)Math.Min(16320, value + p);
                Check();
            }
            Assert.Equal(saved, values);
        }
        int[] edges = [int.MinValue, int.MinValue + 1, -131073, -131072, -1, 0, 1,
            131071, 131072, 131073, (2047 << 17) - 1, 2047 << 17,
            (2048 << 17) - 1, 2048 << 17, int.MaxValue - 1, int.MaxValue];
        for (var sample = 0; sample < 8192; sample++)
        {
            for (var p = 0; p < 16; p++)
            {
                var value = sample % 4 == 0 ? 0 : (Next() & int.MaxValue) % 32641 - 16320;
                coefficients[p] = (short)value; values[p] = 1;
                values[64 + p] = sample % 3 == 0 ? edges[(sample + p) & 15] : Next();
                var magnitude = unchecked(Math.Abs(value) + values[64 + p]);
                values[48 + p] = unchecked(magnitude + sample % 3 - 1);
                values[16 + p] = edges[(sample / 16 + p) & 15]; values[32 + p] = edges[(sample + p) & 15];
            }
            Check();
        }
        Assert.Equal(73504, blocks);
        Assert.Equal("c3ac724b5e6973263f3eca415480a53a63c4951e177b92e1f84332eb0d076991",
            Convert.ToHexStringLower(hash.GetHashAndReset()));
        Assert.Equal((30000, -30000, 97, -98), ((int)coefficients[16], (int)coefficients[17], (int)levels[16], (int)levels[17]));
    }

    [Fact]
    public void CheckedReconstructionPublishesOnlyTheValidPrefix()
    {
        foreach (var position in Enumerable.Range(0, 16))
        foreach (var step in new[] { int.MinValue, int.MaxValue, 65536, -65536, 32768, -32769 })
        {
            var coefficients = new short[16]; var levels = Enumerable.Repeat((short)29999, 16).ToArray(); var values = new int[80];
            for (var p = 0; p < 16; p++)
            {
                coefficients[p] = (short)(p % 2 == 0 ? 1000 : -1000);
                values[p] = p == position ? step : 1; values[16 + p] = 32768;
            }
            Assert.Throws<OverflowException>(() => Vp8BlockQuantization.Apply(coefficients, levels, new(values)));
            for (var p = 0; p < 16; p++)
            {
                Assert.Equal((short)((p < position ? 250 : 1000) * (p % 2 == 0 ? 1 : -1)), coefficients[p]);
                var x = p & 3; var y = p >> 2; var d = x + y;
                var before = d < 4 ? d * (d + 1) / 2 : 16 - (7 - d) * (8 - d) / 2;
                var rank = before + ((d & 1) != 0 ? Math.Min(3, d) - x : x - Math.Max(0, d - 3));
                Assert.Equal((short)(p < position ? (p % 2 == 0 ? 250 : -250) : 29999), levels[rank]);
            }
        }
    }

    [Fact]
    public void OutputAliasingBorrowedMatrixFailsBeforeAnyMutation()
    {
        var values = new int[96]; MinimumStep(values); var saved = values.ToArray();
        var ordinary = Enumerable.Repeat((short)77, 18).ToArray(); var original = ordinary.ToArray();
        Assert.Throws<ArgumentException>(() => Vp8BlockQuantization.Apply(
            MemoryMarshal.Cast<int, short>(values.AsSpan()).Slice(0, 18), ordinary, new(values.AsSpan(0, 80))));
        Assert.Equal(saved, values); Assert.Equal(original, ordinary);
        Assert.Throws<ArgumentException>(() => Vp8BlockQuantization.Apply(
            ordinary, MemoryMarshal.Cast<int, short>(values.AsSpan()).Slice(32, 18), new(values.AsSpan(0, 80))));
        Assert.Equal(saved, values); Assert.Equal(original, ordinary);
    }

    [Fact]
    public void WarmQuantizationDoesNotAllocate()
    {
        var values = new int[80]; MinimumStep(values);
        Span<short> coefficients = stackalloc short[16]; coefficients.Fill(1000);
        Span<short> levels = stackalloc short[16]; var matrix = new Vp8QuantizationMatrix(values);
        for (var i = 0; i < 256; i++) Vp8BlockQuantization.Apply(coefficients, levels, matrix);
        var before = GC.GetAllocatedBytesForCurrentThread();
        for (var i = 0; i < 10000; i++) Vp8BlockQuantization.Apply(coefficients, levels, matrix);
        Assert.Equal(0, GC.GetAllocatedBytesForCurrentThread() - before);
    }
}
