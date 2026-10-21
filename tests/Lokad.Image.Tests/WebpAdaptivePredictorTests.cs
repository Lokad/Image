using Lokad.Image.Internal;
using Lokad.Image.Tooling;

namespace Lokad.Image.Tests;

public sealed class WebpAdaptivePredictorTests
{
    [Theory]
    [InlineData(1,1,8)] [InlineData(1,9,8)] [InlineData(9,1,8)] [InlineData(17,9,8)]
    [InlineData(1,1,16)] [InlineData(1,9,16)] [InlineData(9,1,16)] [InlineData(17,9,16)]
    public void AllFourteenResidualModesMatchIndependentChannelArithmetic(int width, int height, int depth)
    {
        using var image = Source(width, height, depth);
        var original = image.Pixels.ToArray(); var columns = (width + 7) / 8; var modes = new byte[columns * ((height + 7) / 8)];
        using var context = new OperationContext(ImageLimits.Default, default);
        for (byte mode = 0; mode < 14; mode++)
        {
            Array.Fill(modes, mode);
            for (var y = 0; y < height; y++)
            for (var x = 0; x < width; x++)
            {
                var pixel = Green(image, x, y); uint prediction;
                if (y == 0) prediction = x == 0 ? 0xff000000u : Green(image, x - 1, y);
                else if (x == 0) prediction = Green(image, x, y - 1);
                else prediction = Reference(mode, Green(image,x-1,y), Green(image,x,y-1), Green(image,x-1,y-1), Green(image,x+1==width?0:x+1,x+1==width?y:y-1));
                uint expected = 0;
                for (var c = 0; c < 4; c++) expected |= (uint)(byte)((pixel >> (c*8)) - (prediction >> (c*8))) << (c*8);
                Assert.Equal(expected, WebpLosslessEncoder.AdaptiveResidual(image, y * width + x, modes, context));
            }
        }
        Assert.Equal(original, image.Pixels.ToArray());
    }

    [Fact]
    public void TileSelectionIsDeterministicBoundedAndReleasesItsScratch()
    {
        using var image = Source(17, 9, 16); var original = image.Pixels.ToArray();
        var first = new byte[6]; var second = new byte[6];
        using var root = new TestMemoryBudget(8192);
        using var context = new OperationContext(new ImageLimits { MemoryBudget = root }, default);
        WebpLosslessEncoder.SelectPredictorModes(image, first, context);
        Assert.Equal(0, root.Bytes); Assert.Equal(8192, root.Peak); Assert.False(root.IsDisposed);
        WebpLosslessEncoder.SelectPredictorModes(image, second, context);
        Assert.Equal(first, second); Assert.All(first, mode => Assert.InRange(mode, (byte)0, (byte)13));
        Assert.Equal(original, image.Pixels.ToArray());
    }

    [Fact]
    public void TileScratchRefusalIsTerminalAndCancellationRefundsIt()
    {
        using var image = Source(17, 9, 8); var map = new byte[6];
        using var root = new TestMemoryBudget(8192); using var host = root.CreateScope(); Assert.True(host.TryReserve(1, out var held));
        using (var context = new OperationContext(new() { MemoryBudget = root }, default))
        {
            var attempts = root.Attempts;
            var refused = Assert.Throws<ImageLimitException>(() => WebpLosslessEncoder.SelectPredictorModes(image, map, context));
            Assert.Equal(ImageLimitScope.SharedBudget, refused.Scope); Assert.Equal(attempts + 1, root.Attempts); Assert.Equal(1, root.Bytes);
        }
        held!.Dispose(); using var token = new CancellationTokenSource();
        root.Admitted = _ => token.Cancel();
        using (var context = new OperationContext(new() { MemoryBudget = root }, token.Token))
            Assert.Throws<OperationCanceledException>(() => WebpLosslessEncoder.SelectPredictorModes(image, map, context));
        Assert.Equal(0, root.Bytes); root.Admitted = null;
        using var recovery = new OperationContext(new() { MemoryBudget = root }, default);
        WebpLosslessEncoder.SelectPredictorModes(image, map, recovery); Assert.Equal(0, root.Bytes); Assert.False(root.IsDisposed);
    }

    [Theory] [InlineData(8)] [InlineData(16)]
    public void AdaptiveMapAndSpatialGroupsPreserveNormalizedPixelsAndOutputOwners(int depth)
    {
        using var root = new TestMemoryBudget(32L * 1024 * 1024);
        using var source = Photograph(depth, root);
        var before = source.Pixels.ToArray(); var expected = new byte[source.Width * source.Height * 4];
        for (var y = 0; y < source.Height; y++)
        for (var x = 0; x < source.Width * 4; x++)
            expected[y * source.Width * 4 + x] = depth == 8 ? source.GetRow8(y)[x] : (byte)((source.GetRow16(y)[x] + 128) / 257);
        var baseline = root.Bytes;
        var options = new WebpEncodeOptions { Effort = 4, Limits = new() { MemoryBudget = root } };
        using var output = new MemoryStream(); ImageCodec.EncodeWebp(source, output, options);
        var packet = output.ToArray();
        Assert.Equal(13, packet[25] & 63); // Subtract-green followed by predictor.
        Assert.Equal(1, packet[25] >> 6 & 3); // 8x8 adaptive map, rather than constant LEFT.
        Assert.Equal(baseline, root.Bytes); Assert.False(root.IsDisposed);
        Assert.True(root.Peak > baseline + 6_000_000); // Spatial analysis overlaps retained plans/map.
        Assert.Equal(before, source.Pixels.ToArray());
        using var exact = new MemoryStream();
        ImageCodec.EncodeWebp(source, exact, options with { Limits = options.Limits with { MaxOutputBytes = packet.Length } });
        Assert.Equal(packet, exact.ToArray()); Assert.Equal(baseline, root.Bytes);
        using var shortOutput = new MemoryStream();
        var error = Assert.Throws<ImageLimitException>(() => ImageCodec.EncodeWebp(source, shortOutput,
            options with { Limits = options.Limits with { MaxOutputBytes = packet.Length - 1 } }));
        Assert.Equal(ImageLimitKind.OutputBytes, error.Limit); Assert.Equal(0, shortOutput.Length);
        Assert.Equal(baseline, root.Bytes); Assert.True(shortOutput.CanWrite);
        source.Dispose(); Assert.Equal(0, root.Bytes);
        var decode = new DecodeOptions { Limits = options.Limits };
        using var first = ImageCodec.Decode(packet, decode);
        using var second = ImageCodec.Decode(exact.ToArray(), decode);
        packet.AsSpan().Clear(); first.Dispose();
        Assert.Equal(expected, second.Pixels.ToArray()); Assert.True(root.Bytes >= expected.Length);
        second.Dispose(); Assert.Equal(0, root.Bytes); Assert.False(root.IsDisposed);
    }

    [Fact]
    public void AdaptiveGroupReplacementRefusesTerminallyAndCancellationReleasesItsOwners()
    {
        using var root = new TestMemoryBudget(32L * 1024 * 1024);
        using var source = Photograph(8, root); var original = source.Pixels.ToArray(); var baseline = root.Bytes;
        var options = new WebpEncodeOptions { Effort = 4, Limits = new() { MemoryBudget = root } };
        var high = false; var phases = 0;
        root.Admitted = bytes =>
        {
            var now = bytes > baseline + 5_000_000;
            if (now && !high && ++phases == 2) root.RefuseAtGrowth = root.GrowthAttempts + 1;
            high = now;
        };
        using var refused = new MemoryStream();
        var error = Assert.Throws<ImageLimitException>(() => ImageCodec.EncodeWebp(source, refused, options));
        Assert.Equal(2, phases); Assert.Equal(ImageLimitScope.SharedBudget, error.Scope);
        Assert.Equal(ImageLimitKind.WorkingBytes, error.Limit); Assert.Equal(1, root.Refusals);
        Assert.Equal(0, refused.Length); Assert.Equal(baseline, root.Bytes);
        root.Admitted = null; root.RefuseAtGrowth = 0; high = false; phases = 0;
        using var cancellation = new CancellationTokenSource();
        root.Admitted = bytes =>
        {
            var now = bytes > baseline + 5_000_000;
            if (now && !high && ++phases == 2) cancellation.Cancel();
            high = now;
        };
        using var cancelled = new MemoryStream();
        Assert.Throws<OperationCanceledException>(() => ImageCodec.EncodeWebp(source, cancelled, options, cancellation.Token));
        Assert.Equal(2, phases); Assert.Equal(0, cancelled.Length); Assert.Equal(baseline, root.Bytes);
        root.Admitted = null;
        using var recovery = new MemoryStream(); ImageCodec.EncodeWebp(source, recovery, options);
        Assert.Equal(1, recovery.GetBuffer()[25] >> 6 & 3);
        Assert.Equal(baseline, root.Bytes); Assert.Equal(original, source.Pixels.ToArray()); Assert.False(root.IsDisposed);
    }

    internal static ImageBuffer Photograph(int depth, TestMemoryBudget root)
    {
        string? path = null;
        for (var directory = new DirectoryInfo(AppContext.BaseDirectory); directory is not null; directory = directory.Parent)
        {
            var candidate = Path.Combine(directory.FullName, "benchmarks/Fixtures/Compression/photograph.png");
            if (File.Exists(candidate)) { path = candidate; break; }
        }
        using var original = ImageCodec.Decode(File.ReadAllBytes(path ?? throw new InvalidOperationException("Original compression fixture missing.")));
        var result = ImageBuffer.Create(original.Width, original.Height, depth, new() { MemoryBudget = root });
        for (var y = 0; y < result.Height; y++)
        for (var x = 0; x < result.Width * 4; x++)
        {
            var value = original.GetRow8(y)[x];
            if (depth == 8) result.GetRow8(y)[x] = value;
            else result.GetRow16(y)[x] = (ushort)Math.Clamp(value * 257 + ((x + y) % 3 - 1) * 64, 0, 65535);
        }
        return result;
    }

    private static ImageBuffer Source(int width, int height, int depth)
    {
        var result = ImageBuffer.Create(width, height, depth); uint state = 0x13929a7;
        for (var y = 0; y < height; y++)
        for (var x = 0; x < width; x++)
        for (var c = 0; c < 4; c++)
        {
            state = unchecked(state * 1664525 + 1013904223);
            result.SetSample(x, y, c, c == 3 && (x + y) % 3 == 0 ? 0 : (int)(state >> 16) & (depth == 8 ? 255 : 65535));
        }
        return result;
    }

    private static uint Green(ImageBuffer image, int x, int y)
    {
        int Sample(int c) => image.BitDepth == 8 ? image.GetRow8(y)[x * 4 + c] : (image.GetRow16(y)[x * 4 + c] + 128) / 257;
        var green = Sample(1);
        return (uint)(Sample(3) << 24 | ((Sample(0)-green)&255) << 16 | green << 8 | ((Sample(2)-green)&255));
    }

    private static uint Reference(int mode, uint left, uint top, uint corner, uint right)
    {
        uint Average(uint a, uint b)
        {
            uint result = 0;
            for (var c=0;c<4;c++) result |= (((a>>(c*8)&255)+(b>>(c*8)&255))/2)<<(c*8);
            return result;
        }
        if (mode <= 4) return new[] {0xff000000u,left,top,right,corner}[mode];
        if (mode == 5) return Average(Average(left,right),top);
        if (mode == 6) return Average(left,corner);
        if (mode == 7) return Average(left,top);
        if (mode == 8) return Average(corner,top);
        if (mode == 9) return Average(top,right);
        if (mode == 10) return Average(Average(left,corner),Average(top,right));
        if (mode == 11)
        {
            var l=0;var t=0;
            for (var c=0;c<4;c++) { l+=Math.Abs((int)(top>>(c*8)&255)-(int)(corner>>(c*8)&255));t+=Math.Abs((int)(left>>(c*8)&255)-(int)(corner>>(c*8)&255)); }
            return l<t?left:top;
        }
        var average = Average(left,top); uint predicted=0;
        for (var c=0;c<4;c++)
        {
            var l=(int)(left>>(c*8)&255);var t=(int)(top>>(c*8)&255);var tl=(int)(corner>>(c*8)&255);var a=(int)(average>>(c*8)&255);
            predicted |= (uint)Math.Clamp(mode==12?l+t-tl:a+(a-tl)/2,0,255)<<(c*8);
        }
        return predicted;
    }
}
