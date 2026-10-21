using System.Reflection;
using Lokad.Image.Internal;
using Lokad.Image.Tooling;

namespace Lokad.Image.Tests;

public sealed class WebpColorTransformTests
{
    [Fact]
    public void SignedCoefficientsRoundTripEveryChannelValueWithOriginalRedDependency()
    {
        static int Signed(int value) => value < 128 ? value : value - 256;
        static int Floor32(int value) => value >= 0 ? value / 32 : (value - 31) / 32;
        for (var coefficient = -128; coefficient < 128; coefficient++)
        for (var sample = 0; sample < 256; sample++)
        {
            var a = coefficient; var b = coefficient >= 0 ? 127 - coefficient : -128 - coefficient; var c = -coefficient - 1;
            var g = (sample * 73 + 19) & 255; var blue = (sample * 37 + 101) & 255; var alpha = sample * 29 & 255;
            var pixel = (uint)(alpha << 24 | sample << 16 | g << 8 | blue);
            var encoded = WebpLosslessEncoder.ColorResidual(pixel, a, b, c);
            var redResidual = (sample - Floor32(a * Signed(g))) & 255;
            var blueResidual = (blue - Floor32(b * Signed(g)) - Floor32(c * Signed(sample))) & 255;
            Assert.Equal((uint)(alpha << 24 | redResidual << 16 | g << 8 | blueResidual), encoded);
            var restoredRed = (redResidual + Floor32(a * Signed(g))) & 255;
            var restoredBlue = (blueResidual + Floor32(b * Signed(g)) + Floor32(c * Signed(restoredRed))) & 255;
            Assert.Equal(pixel, (uint)(alpha << 24 | restoredRed << 16 | g << 8 | restoredBlue));
        }
    }

    [Theory]
    [InlineData(1, 1, 8, false)] [InlineData(1, 9, 8, true)] [InlineData(9, 1, 16, true)]
    [InlineData(17, 9, 8, false)] [InlineData(17, 9, 16, true)]
    public void FixedCoefficientFitIsDeterministicAtBordersAndReleasesScratch(int width, int height, int depth, bool predict)
    {
        using var source = ImageBuffer.Create(width, height, depth);
        for (var y = 0; y < height; y++)
        for (var x = 0; x < width; x++)
        for (var c = 0; c < 4; c++)
        {
            var value = (x * 311 + y * 131 + c * 1039 + x * y * 17) & 65535;
            source.SetSample(x, y, c, depth == 8 ? (value + 128) / 257 : value);
        }
        var before = source.Pixels.ToArray();
        var first = new byte[((width + 7) / 8) * ((height + 7) / 8) * 3]; var second = new byte[first.Length];
        using var root = new TestMemoryBudget(4096);
        using var context = new OperationContext(new() { MemoryBudget = root }, default);
        WebpLosslessEncoder.SelectColorCoefficients(source, predict, null, first, context);
        Assert.Equal(0, root.Bytes); Assert.Equal(4096, root.Peak);
        WebpLosslessEncoder.SelectColorCoefficients(source, predict, null, second, context);
        Assert.Equal(first, second); Assert.Equal(before, source.Pixels.ToArray()); Assert.Equal(0, root.Bytes);
        Assert.False(root.IsDisposed);
    }

    [Fact]
    public void ScratchRefusalCancellationAndWorkExhaustionAreTerminalAndRefunded()
    {
        using var source = ImageBuffer.Create(17, 9, 16); var map = new byte[18];
        using var root = new TestMemoryBudget(4096); using var caller = root.CreateScope();
        Assert.True(caller.TryReserve(1, out var held));
        using (var context = new OperationContext(new() { MemoryBudget = root }, default))
        {
            var attempts = root.Attempts;
            var error = Assert.Throws<ImageLimitException>(() => WebpLosslessEncoder.SelectColorCoefficients(source, true, null, map, context));
            Assert.Equal(ImageLimitScope.SharedBudget, error.Scope); Assert.Equal(attempts + 1, root.Attempts); Assert.Equal(1, root.Bytes);
        }
        held!.Dispose(); using var cancellation = new CancellationTokenSource(); root.Admitted = _ => cancellation.Cancel();
        using (var context = new OperationContext(new() { MemoryBudget = root }, cancellation.Token))
            Assert.Throws<OperationCanceledException>(() => WebpLosslessEncoder.SelectColorCoefficients(source, true, null, map, context));
        Assert.Equal(0, root.Bytes); root.Admitted = null;
        using (var context = new OperationContext(new() { MemoryBudget = root, MaxWork = 1 }, default))
            Assert.Equal(ImageLimitKind.Work, Assert.Throws<ImageLimitException>(() => WebpLosslessEncoder.SelectColorCoefficients(source, true, null, map, context)).Limit);
        Assert.Equal(0, root.Bytes);
        using var recovery = new OperationContext(new() { MemoryBudget = root }, default);
        WebpLosslessEncoder.SelectColorCoefficients(source, true, null, map, recovery);
        Assert.Equal(0, root.Bytes); Assert.False(root.IsDisposed);
    }

    [Fact]
    public void SelectedPlanKeepsExactlyOneChargeForInheritedPredictorAndColorOwners()
    {
        using var root = new TestMemoryBudget(32L * 1024 * 1024);
        using var source = WebpAdaptivePredictorTests.Photograph(8, root); var baseline = root.Bytes;
        using (var context = new OperationContext(new() { MemoryBudget = root }, default))
        {
            // Inspect only the selected private owner, without adding production
            // test hooks. The cache and source are caller-owned in this unit boundary.
            var method = typeof(WebpLosslessEncoder).GetMethod("SelectCachedPlan", BindingFlags.Static | BindingFlags.NonPublic)!;
            var plan = method.Invoke(null, [source, 4, new uint[1024], context])!;
            static object Get(object value, string name) => value.GetType().GetProperty(name)!.GetValue(value)!;
            static long Tables(object value)
            {
                var tables = (WebpPrefixCode[])value;
                return tables.Length * 8L + 32 + tables.Sum(t => t.Alphabet * 3L + 128);
            }
            var modes = (byte[])Get(plan, "Modes"); var modeTable = (WebpPrefixCode)Get(plan, "ModeTable");
            var groups = Get(plan, "Groups"); var color = Get(plan, "Color");
            Assert.NotNull(modes); Assert.NotNull(groups); Assert.NotNull(color);
            var bytes = Tables(Get(plan, "Tables")) + modes.Length + 32L + modeTable.Alphabet * 3L + 128
                + ((byte[])Get(groups, "Map")).Length + 80L + ((WebpPrefixCode)Get(groups, "MapTable")).Alphabet * 3L + 128
                + ((byte[])Get(color, "Coefficients")).Length + 64L + Tables(Get(color, "Tables"));
            Assert.Equal(baseline + bytes, root.Bytes); Assert.False(root.IsDisposed);
        }
        Assert.Equal(baseline, root.Bytes);
    }

    [Theory] [InlineData(8)] [InlineData(16)]
    public void CompleteColorPacketImprovesTheFrozenPhotographWithExactPixelsAndIndependentOutputs(int depth)
    {
        using var root = new TestMemoryBudget(32L * 1024 * 1024);
        using var source = WebpAdaptivePredictorTests.Photograph(depth, root); var before = source.Pixels.ToArray();
        var expected = new byte[source.Width * source.Height * 4];
        for (var y = 0; y < source.Height; y++)
        for (var x = 0; x < source.Width * 4; x++)
            expected[y * source.Width * 4 + x] = depth == 8 ? source.GetRow8(y)[x] : (byte)((source.GetRow16(y)[x] + 128) / 257);
        var baseline = root.Bytes; var options = new WebpEncodeOptions { Effort = 4, Limits = new() { MemoryBudget = root } };
        using var output = new MemoryStream(); ImageCodec.EncodeWebp(source, output, options);
        Assert.InRange(output.Length, 1, 172416); // C91 predecessor:172418 complete bytes.
        Assert.Equal(baseline, root.Bytes); Assert.Equal(before, source.Pixels.ToArray());
        var packet = output.ToArray(); using var exact = new MemoryStream();
        ImageCodec.EncodeWebp(source, exact, options with { Limits = options.Limits with { MaxOutputBytes = packet.Length } });
        Assert.Equal(packet, exact.ToArray());
        using var rejected = new MemoryStream();
        Assert.Equal(ImageLimitKind.OutputBytes, Assert.Throws<ImageLimitException>(() => ImageCodec.EncodeWebp(source, rejected,
            options with { Limits = options.Limits with { MaxOutputBytes = packet.Length - 1 } })).Limit);
        Assert.Equal(0, rejected.Length); Assert.True(rejected.CanWrite); Assert.Equal(baseline, root.Bytes);
        source.Dispose(); Assert.Equal(0, root.Bytes);
        using var first = ImageCodec.Decode(packet, new() { Limits = options.Limits });
        using var second = ImageCodec.Decode(exact.ToArray(), new() { Limits = options.Limits });
        packet.AsSpan().Clear(); first.Dispose(); Assert.Equal(expected, second.Pixels.ToArray());
        second.Dispose(); Assert.Equal(0, root.Bytes); Assert.False(root.IsDisposed);
    }

    [Fact]
    public void ColorGroupRefusalAndCancellationReleaseBothMapsWithoutRetry()
    {
        using var root = new TestMemoryBudget(32L * 1024 * 1024);
        using var source = WebpAdaptivePredictorTests.Photograph(8, root); var baseline = root.Bytes;
        var options = new WebpEncodeOptions { Effort = 4, Limits = new() { MemoryBudget = root } };
        var phases = 0; var high = false;
        root.Admitted = bytes =>
        {
            var now = bytes > baseline + 5_000_000;
            if (now && !high && ++phases == 3) root.RefuseAtGrowth = root.GrowthAttempts + 1;
            high = now;
        };
        using var refused = new MemoryStream();
        Assert.Equal(ImageLimitScope.SharedBudget, Assert.Throws<ImageLimitException>(() => ImageCodec.EncodeWebp(source, refused, options)).Scope);
        Assert.Equal(3, phases); Assert.Equal(1, root.Refusals); Assert.Equal(0, refused.Length); Assert.Equal(baseline, root.Bytes);
        root.RefuseAtGrowth = 0; high = false; phases = 0; using var cancellation = new CancellationTokenSource();
        root.Admitted = bytes =>
        {
            var now = bytes > baseline + 5_000_000;
            if (now && !high && ++phases == 3) cancellation.Cancel();
            high = now;
        };
        using var cancelled = new MemoryStream();
        Assert.Throws<OperationCanceledException>(() => ImageCodec.EncodeWebp(source, cancelled, options, cancellation.Token));
        Assert.Equal(3, phases); Assert.Equal(0, cancelled.Length); Assert.Equal(baseline, root.Bytes); root.Admitted = null;
        using var recovery = new MemoryStream(); ImageCodec.EncodeWebp(source, recovery, options);
        Assert.InRange(recovery.Length, 1, 172416); Assert.Equal(baseline, root.Bytes); Assert.False(root.IsDisposed);
    }
}
