using Lokad.Image.Internal;
using Lokad.Image.Tooling;

namespace Lokad.Image.Tests;

public sealed class JpegYcckTileTests
{
    [Fact]
    public void HalfGridRowsKeepActualScalarColorAcrossRangesTailsAndRoundingBoundaries()
    {
        using var root = new TestMemoryBudget(0);
        using var context = new OperationContext(ImageLimits.Default with { MemoryBudget = root, MaxWorkingBytes = 1 }, default);
        foreach (var width in new[] { 1, 2, 7, 8, 9, 15, 16, 17, 31, 32, 33, 63, 64, 65, 129, 513 })
        foreach (var interpolate in new[] { false, true })
        foreach (var inverted in new[] { false, true })
        for (var seed = 0; seed < 8; seed++) Check(width, seed, interpolate, inverted, context);
        // Every luma/black byte pair, plus chroma signs and extreme half-grid
        // averages, reaches the preserved actual per-pixel production leaf.
        for (var black = 0; black < 256; black++)
            Check(256, black, interpolate: (black & 1) != 0, inverted: (black & 2) != 0, context, fixedBlack: black);
        Assert.Equal(0, root.Bytes); Assert.Equal(0, root.Attempts); Assert.False(root.IsDisposed);

        static void Check(int width, int seed, bool interpolate, bool inverted, OperationContext context, int fixedBlack = -1)
        {
            var random = new Random(1739 + seed * 613 + width); var half = (width + 1) / 2;
            var inputs = new[] { new byte[width + 2], new byte[half + 2], new byte[half + 2], new byte[half + 2], new byte[half + 2], new byte[width + 2] };
            foreach (var input in inputs) { random.NextBytes(input); input[0] = 19; input[^1] = 231; }
            for (var x = 0; x < width; x++)
            {
                if (fixedBlack >= 0) { inputs[0][x + 1] = (byte)x; inputs[5][x + 1] = (byte)fixedBlack; }
                else if ((x & 3) == 0) { inputs[0][x + 1] = (byte)((seed & 1) * 255); inputs[5][x + 1] = (byte)((seed & 2) == 0 ? 0 : 255); }
            }
            var original = inputs.Select(p => p.ToArray()).ToArray(); var expected = new byte[width * 4];
            for (var x = 0; x < width; x++)
            {
                var left = x / 2 + 1; var right = Math.Min((x + 1) / 2 + 1, half);
                var cb = (inputs[1][left] + inputs[1][right]) / 2; var cr = (inputs[3][left] + inputs[3][right]) / 2;
                if (interpolate) { cb = (cb + (inputs[2][left] + inputs[2][right]) / 2) / 2; cr = (cr + (inputs[4][left] + inputs[4][right]) / 2) / 2; }
                JpegBlackColor.WriteYcck(inputs[0][x + 1], (byte)cb, (byte)cr, inputs[5][x + 1], inverted, expected.AsSpan(x * 4, 4), 4);
            }
            var actual = Enumerable.Repeat((byte)165, expected.Length + 14).ToArray();
            JpegDecoder.RenderHalfYcckRow(inputs[0].AsSpan(1, width), inputs[1].AsSpan(1, half), inputs[2].AsSpan(1, half),
                inputs[3].AsSpan(1, half), inputs[4].AsSpan(1, half), inputs[5].AsSpan(1, width), interpolate, inverted, actual.AsSpan(7, expected.Length), context);
            Assert.Equal(expected, actual.AsSpan(7, expected.Length).ToArray());
            Assert.All(actual.Take(7).Concat(actual.Skip(expected.Length + 7)), b => Assert.Equal(165, b));
            for (var i = 0; i < inputs.Length; i++) Assert.Equal(original[i], inputs[i]);
        }
    }

    [Fact]
    public void RowChecksShapesAndCancellationBeforeWritingAndAllocatesNoHeap()
    {
        using var root = new TestMemoryBudget(0); using var token = new CancellationTokenSource();
        using var context = new OperationContext(ImageLimits.Default with { MemoryBudget = root, MaxWorkingBytes = 1 }, token.Token);
        var full = Enumerable.Range(0, 65).Select(i => (byte)(i * 7)).ToArray(); var half = full[..33];
        var output = Enumerable.Repeat((byte)165, full.Length * 4).ToArray();
        Assert.Throws<ArgumentException>(() => JpegDecoder.RenderHalfYcckRow(full, half.AsSpan(1), half, half, half, full, true, true, output, context));
        Assert.Throws<ArgumentException>(() => JpegDecoder.RenderHalfYcckRow(full, half, half, half, half, full.AsSpan(1), true, true, output, context));
        Assert.Throws<ArgumentException>(() => JpegDecoder.RenderHalfYcckRow(full, half, half, half, half, full, true, true, output.AsSpan(1), context));
        Assert.All(output, b => Assert.Equal(165, b));
        for (var i = 0; i < 16; i++) JpegDecoder.RenderHalfYcckRow(full, half, half, half, half, full, true, true, output, context);
        var before = GC.GetAllocatedBytesForCurrentThread();
        for (var i = 0; i < 256; i++) JpegDecoder.RenderHalfYcckRow(full, half, half, half, half, full, true, true, output, context);
        Assert.Equal(before, GC.GetAllocatedBytesForCurrentThread());
        Array.Fill(output, (byte)165); token.Cancel();
        var error = Assert.Throws<OperationCanceledException>(() => JpegDecoder.RenderHalfYcckRow(full, half, half, half, half, full, true, true, output, context));
        Assert.Equal(token.Token, error.CancellationToken); Assert.All(output, b => Assert.Equal(165, b));
        Assert.Equal(0, root.Bytes); Assert.Equal(0, root.Attempts); Assert.False(root.IsDisposed);
    }
}
