using System.Reflection;
using Lokad.Image.Internal;
using Lokad.Image.Tooling;

namespace Lokad.Image.Tests;

public sealed class Vp8SourceQuantizationTests
{
    [Theory]
    [InlineData(1, 1, 0, 0)] [InlineData(2, 3, 1, 1)]
    [InlineData(5, 5, 1, 1)] [InlineData(5, 5, 2, 2)]
    [InlineData(5, 5, 0, 3)] [InlineData(5, 5, 4, 4)]
    [InlineData(6, 4, 5, 3)] [InlineData(6, 4, 3, 0)]
    [InlineData(9, 9, 7, 7)] [InlineData(513, 259, 512, 258)]
    public void UnequalPrecisionNoiseMatchesIndependentCoefficientImpulses(int width, int height, int x, int y)
    {
        // Independent inverse-basis impulses, rather than the covariance formula:
        // each coefficient has variance step²/12. Accumulate the RGB energy after
        // centered chroma interpolation, including coincident clamped taps.
        static double Basis(int k, int p) => k == 0 ? 1 : Math.Sqrt(2) * Math.Cos((2 * p + 1) * k * Math.PI / 8);
        var taps = new Dictionary<(int X, int Y), double>();
        var sx = (x - .5) / 2; var sy = (y - .5) / 2;
        var ix = (int)Math.Floor(sx); var iy = (int)Math.Floor(sy);
        for (var dy = 0; dy < 2; dy++)
        for (var dx = 0; dx < 2; dx++)
        {
            var at = (Math.Clamp(ix + dx, 0, (width + 1) / 2 - 1), Math.Clamp(iy + dy, 0, (height + 1) / 2 - 1));
            taps.TryGetValue(at, out var previous);
            taps[at] = previous + (dx == 0 ? 1 - (sx - ix) : sx - ix) * (dy == 0 ? 1 - (sy - iy) : sy - iy);
        }
        var luma = 3 * Math.Pow(255d / 219, 2);
        var chroma = Math.Pow(1.596026785714286, 2) + Math.Pow(.812967647237771, 2)
            + Math.Pow(.391762290094914, 2) + Math.Pow(2.017232142857143, 2);
        Vp8Encoder.QuantizationPlan.NoiseCoefficients(x, y, width, height, out var acWeight, out var dcWeight, out var chromaDcWeight);
        Assert.True(acWeight > 0 && dcWeight > 0 && chromaDcWeight > 0);
        foreach (var (ac, dc, uvdc) in new[] { (4, 4, 4), (32, 16, 16), (284, 157, 132) })
        {
            double energy = 0;
            for (var v = 0; v < 4; v++)
            for (var u = 0; u < 4; u++)
            {
                var response = Basis(u, x % 4) * Basis(v, y % 4) / 8;
                var step = u == 0 && v == 0 ? dc : ac;
                energy += luma * response * response * step * step / 12;
            }
            foreach (var block in taps.Keys.Select(at => (X: at.X / 4, Y: at.Y / 4)).Distinct())
            for (var v = 0; v < 4; v++)
            for (var u = 0; u < 4; u++)
            {
                double response = 0;
                foreach (var (at, weight) in taps)
                    if (at.X / 4 == block.X && at.Y / 4 == block.Y)
                        response += weight * Basis(u, at.X % 4) * Basis(v, at.Y % 4) / 8;
                var step = u == 0 && v == 0 ? uvdc : ac;
                energy += chroma * response * response * step * step / 12;
            }
            var model = (acWeight * ac * ac + dcWeight * dc * dc + chromaDcWeight * uvdc * uvdc) / 16;
            Assert.InRange(Math.Abs(energy - model), 0, Math.Max(1e-12, energy * 2e-14));
        }
    }

    [Theory] [InlineData(0)] [InlineData(27)] [InlineData(126)] [InlineData(127)]
    public void QuantizerSearchMatchesLinearNoiseThresholdsAndPreservesTheBaseFloor(int baseQ)
    {
        foreach (var (acWeight, dcWeight, chromaDcWeight) in new[] { (0d, 0d, 0d), (3.5, .07, .12) })
        {
            double Noise(int q)
            {
                var ac = (int)Vp8Tables.AcQuantizers[q]; var dc = (int)Vp8Tables.DcQuantizers[q]; var uvdc = Math.Min(132, dc);
                return acWeight * ac * ac + dcWeight * dc * dc + chromaDcWeight * uvdc * uvdc;
            }
            foreach (var q in new[] { 0, 15, 28, 127 })
            foreach (var factor in new[] { .999999, 1.000001 })
            {
                var allowance = Noise(q) * factor; var expected = baseQ;
                if (acWeight + dcWeight + chromaDcWeight != 0)
                    for (var trial = baseQ + 1; trial <= 127; trial++) if (Noise(trial) <= allowance) expected = trial;
                Assert.Equal(expected, Vp8Encoder.QuantizationPlan.SelectQuantizer(baseQ, acWeight, dcWeight, chromaDcWeight, allowance));
            }
        }
    }

    [Theory]
    [InlineData(1, 1, 0, 0)] [InlineData(2, 3, 1, 1)]
    [InlineData(5, 5, 1, 1)] [InlineData(5, 5, 2, 2)]
    [InlineData(5, 5, 0, 3)] [InlineData(5, 5, 4, 4)]
    [InlineData(6, 4, 5, 3)] [InlineData(6, 4, 3, 0)]
    public void CenteredNoiseGainMatchesIndependentImpulseEnergy(int width, int height, int x, int y)
    {
        // Place independent unit impulses in source chroma samples. Compute
        // centered bilinear responses in continuous coordinates, with clamped
        // taps combined in the raster before propagating the RGB matrix.
        var weights = new double[(height + 1) / 2, (width + 1) / 2];
        var sx = (x - .5) / 2; var sy = (y - .5) / 2;
        var ix = (int)Math.Floor(sx); var iy = (int)Math.Floor(sy);
        for (var dy = 0; dy < 2; dy++)
        for (var dx = 0; dx < 2; dx++)
            weights[Math.Clamp(iy + dy, 0, weights.GetLength(0) - 1), Math.Clamp(ix + dx, 0, weights.GetLength(1) - 1)]
                += (dx == 0 ? 1 - (sx - ix) : sx - ix) * (dy == 0 ? 1 - (sy - iy) : sy - iy);
        var energy = 3 * Math.Pow(255d / 219, 2);
        foreach (var weight in weights)
        {
            var red = 1.596026785714286 * weight; var greenV = -.812967647237771 * weight;
            var greenU = -.391762290094914 * weight; var blue = 2.017232142857143 * weight;
            energy += red * red + greenV * greenV + greenU * greenU + blue * blue;
        }
        Assert.Equal(energy / 3, Vp8Encoder.QuantizationPlan.NoiseGain(x, y, width, height), precision: 12);
    }

    private const int Width = 65, Height = 33;
    private static ImageLimits Limits(TestMemoryBudget root) => new() { MemoryBudget = root };
    private static ImageBuffer Source(TestMemoryBudget root, int? alpha = null)
    {
        var image = ImageBuffer.Create(Width, Height, 8, Limits(root));
        for (var y = 0; y < Height; y++)
        for (var x = 0; x < Width; x++)
        for (var c = 0; c < 4; c++)
            image.GetRow8(y)[x * 4 + c] = (byte)(c == 3 ? alpha ?? 255
                : (((x * 12347 + y * 7919 + c * 23459 + x * y * 101) & 65535) + 128) / 257);
        return image;
    }
    private static Vp8Planes Convert(ImageBuffer image, OperationContext context)
        => (Vp8Planes)typeof(Vp8Encoder).GetMethod("Convert", BindingFlags.NonPublic | BindingFlags.Static)!
            .Invoke(null, [image, context, null])!;
    private static byte[] Pixels(ImageBuffer image)
    {
        var result = new byte[image.Width * image.Height * 4];
        for (var y = 0; y < image.Height; y++) image.GetRow8(y).CopyTo(result.AsSpan(y * image.Width * 4));
        return result;
    }

    [Fact]
    public void SampleErrorAgreesWithCompleteRenderingIncludingClippingAndEdges()
    {
        var planes = new Vp8Planes(33, 17, 48, new byte[48 * 32], new byte[24 * 16], new byte[24 * 16]);
        var random = new Random(912); random.NextBytes(planes.Y); random.NextBytes(planes.U); random.NextBytes(planes.V);
        using var context = new OperationContext(ImageLimits.Default, default); context.Reserve(planes.MemoryBytes);
        using var rendered = Vp8Color.Render(planes, null, context);
        foreach (var (x, y) in new[] { (0, 0), (1, 1), (8, 4), (16, 8), (32, 16) })
        {
            var row = rendered.GetRow8(y); var r = 13 - row[x * 4]; var g = 97 - row[x * 4 + 1]; var b = 243 - row[x * 4 + 2];
            Assert.Equal(r * r + g * g + b * b, Vp8Color.SourceError(planes, x, y, 13, 97, 243));
        }
    }

    [Fact]
    public void SourceOnlyMapOwnsItsCapacityAndLeavesInputsAndConvertedSamplesUnchanged()
    {
        using var root = new TestMemoryBudget(1000000); using var source = Source(root); var original = Pixels(source); var caller = root.Bytes;
        using (var context = new OperationContext(Limits(root), default))
        {
            var planes = Convert(source, context); var held = root.Bytes;
            var y = planes.Y.ToArray(); var u = planes.U.ToArray(); var v = planes.V.ToArray();
            using (var plan = Vp8Encoder.QuantizationPlan.TryCreate(source, planes, 0, 100, 4, context))
            {
                Assert.NotNull(plan); Assert.Equal(15, plan.Map.Length); Assert.Equal(held + 15 + 256, root.Bytes);
                Assert.Contains(plan.Quantizers, q => q > 0); Assert.All(plan.Map, group => Assert.InRange(group, (byte)0, (byte)3));
                Assert.Equal(y, planes.Y); Assert.Equal(u, planes.U); Assert.Equal(v, planes.V); Assert.Equal(original, Pixels(source));
            }
            Assert.Equal(held, root.Bytes); Assert.False(root.IsDisposed);
        }
        Assert.Equal(caller, root.Bytes); Assert.Equal(original, Pixels(source));
        source.Dispose(); Assert.Equal(0, root.Bytes);
    }

    [Theory] [InlineData(1)] [InlineData(75)] [InlineData(99)] [InlineData(100)]
    public void EffectiveQuantizersRespectTheUnchangedBaseQualityFloor(int quality)
    {
        using var root = new TestMemoryBudget(1000000); using var source = Source(root); var caller = root.Bytes;
        var q = ((100 - quality) * 127 + 49) / 99;
        using (var context = new OperationContext(Limits(root), default))
        {
            var planes = Convert(source, context); var held = root.Bytes;
            using (var plan = Vp8Encoder.QuantizationPlan.TryCreate(source, planes, q, quality, 4, context))
                if (plan is not null) Assert.All(plan.Quantizers, value => Assert.InRange((int)value, q, 127));
            Assert.Equal(held, root.Bytes);
        }
        Assert.Equal(caller, root.Bytes); Assert.False(root.IsDisposed);
    }

    [Fact]
    public void TightLocalMemoryWorkOrEffortDeclinesBeforeAnySharedReservation()
    {
        using var root = new TestMemoryBudget(1000000); using var source = Source(root);
        using var converted = new OperationContext(ImageLimits.Default, default); var planes = Convert(source, converted);
        var attempts = root.Attempts;
        using var memory = new OperationContext(Limits(root) with { MaxWorkingBytes = 270 }, default);
        Assert.Null(Vp8Encoder.QuantizationPlan.TryCreate(source, planes, 0, 100, 4, memory));
        using var work = new OperationContext(Limits(root) with { MaxWork = 1 }, default);
        Assert.Null(Vp8Encoder.QuantizationPlan.TryCreate(source, planes, 0, 100, 4, work));
        using var effort = new OperationContext(Limits(root), default);
        Assert.Null(Vp8Encoder.QuantizationPlan.TryCreate(source, planes, 0, 100, 3, effort));
        Assert.Equal(attempts, root.Attempts);
    }

    [Fact]
    public void SharedRefusalIsTerminalAndReleasingOtherOwnershipAllowsSuccess()
    {
        using var root = new TestMemoryBudget(1000000); using var source = Source(root); var caller = root.Bytes;
        using var converted = new OperationContext(ImageLimits.Default, default); var planes = Convert(source, converted);
        Assert.True(root.TryReserve(1000000 - root.Bytes - 270, out var retained)); var held = root.Bytes;
        using (var context = new OperationContext(Limits(root), default))
        {
            var error = Assert.Throws<ImageLimitException>(() => Vp8Encoder.QuantizationPlan.TryCreate(source, planes, 0, 100, 4, context));
            Assert.Equal(ImageLimitKind.WorkingBytes, error.Limit); Assert.Equal(ImageLimitScope.SharedBudget, error.Scope);
        }
        Assert.Equal(held, root.Bytes); Assert.Equal(1, root.Refusals); retained.Dispose();
        using (var next = new OperationContext(Limits(root), default))
        using (var plan = Vp8Encoder.QuantizationPlan.TryCreate(source, planes, 0, 100, 4, next)) Assert.NotNull(plan);
        Assert.Equal(caller, root.Bytes); Assert.False(root.IsDisposed);
    }

    [Fact]
    public void CancellationAtMapAdmissionRefundsCapacityAndBorrowsCallerOwnership()
    {
        using var root = new TestMemoryBudget(1000000); using var source = Source(root); var original = Pixels(source); var held = root.Bytes;
        using var converted = new OperationContext(ImageLimits.Default, default); var planes = Convert(source, converted);
        using var cancellation = new CancellationTokenSource(); root.Admitted = bytes => { if (bytes == held + 271) cancellation.Cancel(); };
        using (var context = new OperationContext(Limits(root), cancellation.Token))
            Assert.Throws<OperationCanceledException>(() => Vp8Encoder.QuantizationPlan.TryCreate(source, planes, 0, 100, 4, context));
        Assert.Equal(held, root.Bytes); Assert.Equal(original, Pixels(source)); root.Admitted = null;
        using (var next = new OperationContext(Limits(root), default))
        using (var plan = Vp8Encoder.QuantizationPlan.TryCreate(source, planes, 0, 100, 4, next)) Assert.NotNull(plan);
        Assert.Equal(held, root.Bytes); Assert.False(root.IsDisposed);
    }

    [Fact]
    public void ActualAnalysisWorkExhaustionIsTerminalAndReleasesTheAdmittedMap()
    {
        using var root = new TestMemoryBudget(1000000); using var source = Source(root); var held = root.Bytes;
        using var converted = new OperationContext(ImageLimits.Default, default); var planes = Convert(source, converted);
        using (var context = new OperationContext(Limits(root), default))
        {
            root.Admitted = bytes => { if (bytes == held + 271) context.Work(context.RemainingWork - 1); };
            var error = Assert.Throws<ImageLimitException>(() => Vp8Encoder.QuantizationPlan.TryCreate(source, planes, 0, 100, 4, context));
            Assert.Equal(ImageLimitKind.Work, error.Limit); Assert.Equal(ImageLimitScope.Operation, error.Scope);
            Assert.Equal(held, root.Bytes);
        }
        root.Admitted = null;
        using (var next = new OperationContext(Limits(root), default))
        using (var plan = Vp8Encoder.QuantizationPlan.TryCreate(source, planes, 0, 100, 4, next)) Assert.NotNull(plan);
        Assert.Equal(held, root.Bytes); Assert.False(root.IsDisposed);
    }

    [Theory] [InlineData(0)] [InlineData(255)]
    public void UniformSourcesKeepTheBasePolicyAndReleaseAnUnusedMap(int alpha)
    {
        using var root = new TestMemoryBudget(1000000); using var source = Source(root, alpha);
        for (var y = 0; y < Height; y++)
        for (var x = 0; x < Width; x++) source.GetRow8(y).Slice(x * 4, 3).Fill(127);
        using var context = new OperationContext(Limits(root), default); var planes = Convert(source, context); var held = root.Bytes;
        Assert.Null(Vp8Encoder.QuantizationPlan.TryCreate(source, planes, 0, 100, 4, context));
        Assert.Equal(held, root.Bytes); Assert.False(root.IsDisposed);
    }
}
