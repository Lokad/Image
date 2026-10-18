using Lokad.Image.Internal;

namespace Lokad.Image.Tests;

public sealed class Vp8LoopFilterReferenceTests
{
    [Theory]
    [InlineData(8)]
    [InlineData(16)]
    public void CompleteMacroblocksMatchOriginalScalarSegments(int size)
    {
        const int stride = 64;
        int[] levels = [0, 1, 2, 14, 15, 39, 40, 63];
        int[] sharpnesses = [0, 1, 4, 5, 7];
        uint state = 0x719d351b;
        for (var iteration = 0; iteration < 640; iteration++)
        {
            var expected = new byte[stride * 64];
            for (var y = 0; y < 64; y++)
            for (var x = 0; x < stride; x++)
            {
                state = unchecked(1664525 * state + 1013904223);
                expected[y * stride + x] = (iteration % 5) switch
                {
                    0 => (byte)(state >> 24),
                    1 => (byte)(124 + (state >> 24) % 9),
                    2 => (byte)(100 + y / 4 * 3 + (x & 7) % 3),
                    3 => (byte)Math.Clamp(((x & 1) == 0 ? 0 : 255) + (int)(state >> 24) % 11 - 5, 0, 255),
                    _ => (byte)(iteration * 73)
                };
            }
            var actual = expected.ToArray();
            var level = levels[(iteration >> 2) & 7];
            var sharpness = sharpnesses[(iteration >> 5) % sharpnesses.Length];
            var simple = (iteration & 1) != 0; var skip = (iteration & 2) != 0;
            var column = (iteration & 256) == 0 ? 0 : 16;
            var row = (iteration & 128) == 0 ? 0 : 16;
            var context = new OperationContext(ImageLimits.Default, default);
            Scalar(expected, stride, column, row, size, level, sharpness, simple, skip);
            Vp8LoopFilter.Macroblock(actual, stride, column, row, size, level, sharpness, simple, skip, context);
            Assert.Equal(expected, actual);
            var edges = (column == 0 ? 0 : 1) + (row == 0 ? 0 : 1) + (skip ? 0 : 2 * (size / 4 - 1));
            Assert.Equal(level == 0 ? 0 : edges * size * 64L, ImageLimits.Default.MaxWork - context.RemainingWork);
        }
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void WorkAndCancellationRejectBeforeFirstWrite(bool cancel)
    {
        var plane = Enumerable.Repeat((byte)108, 64 * 64).ToArray(); var original = plane.ToArray();
        using var cancellation = new CancellationTokenSource();
        var context = new OperationContext(new ImageLimits { MaxWork = cancel ? 4096 : 1 }, cancellation.Token);
        if (cancel) cancellation.Cancel();
        var error = Record.Exception(() => Vp8LoopFilter.Macroblock(plane, 64, 16, 16, 16, 40, 0, false, false, context));
        if (cancel) Assert.IsType<OperationCanceledException>(error); else Assert.IsType<ImageLimitException>(error);
        Assert.Equal(original, plane);
    }

    [Theory]
    [InlineData(8, 8, 0, 0)]
    [InlineData(8, 24, 16, 0)]
    [InlineData(16, 16, 0, 0)]
    [InlineData(16, 32, 16, 16)]
    public void VerticalTransposeKeepsMinimalRowsAndEveryNeighborExact(int size, int stride, int x, int y)
    {
        uint state = 0x275916b3;
        for (var iteration = 0; iteration < 128; iteration++)
        {
            // End exactly at the last macroblock row; no readable padding.
            var expected = new byte[stride * (y + size)];
            for (var i = 0; i < expected.Length; i++)
            {
                state = unchecked(1664525 * state + 1013904223);
                expected[i] = iteration % 4 == 0 ? (byte)(state >> 24)
                    : (byte)Math.Clamp(123 + (i % stride >= x ? iteration % 13 : -iteration % 13)
                        + (int)(state >> 24) % 5, 0, 255);
            }
            var actual = expected.ToArray(); var level = new[] { 1, 14, 15, 39, 40, 63 }[iteration % 6];
            var sharpness = iteration % 8; var simple = (iteration & 1) != 0; var skip = (iteration & 2) != 0;
            var context = new OperationContext(ImageLimits.Default, default);
            Scalar(expected, stride, x, y, size, level, sharpness, simple, skip);
            Vp8LoopFilter.Macroblock(actual, stride, x, y, size, level, sharpness, simple, skip, context);
            Assert.Equal(expected, actual);
            var edges = (x == 0 ? 0 : 1) + (y == 0 ? 0 : 1) + (skip ? 0 : 2 * (size / 4 - 1));
            Assert.Equal(edges * size * 64L, ImageLimits.Default.MaxWork - context.RemainingWork);
        }
    }

    private static void Scalar(byte[] plane, int stride, int x, int y, int size, int level,
        int sharpness, bool simple, bool skip)
    {
        if (level == 0) return;
        var interior = level;
        if (sharpness != 0) interior = Math.Min(interior >> (sharpness > 4 ? 2 : 1), 9 - sharpness);
        interior = Math.Max(1, interior);
        var variance = level >= 40 ? 2 : level >= 15 ? 1 : 0;
        var outer = 2 * (level + 2) + interior; var inner = 2 * level + interior;
        if (x != 0) Line(plane, y * stride + x, 1, stride, size, outer, interior, variance, true, simple);
        if (!skip)
            for (var column = 4; column < size; column += 4)
                Line(plane, y * stride + x + column, 1, stride, size, inner, interior, variance, false, simple);
        if (y != 0) Line(plane, y * stride + x, stride, 1, size, outer, interior, variance, true, simple);
        if (!skip)
            for (var row = 4; row < size; row += 4)
                Line(plane, (y + row) * stride + x, stride, 1, size, inner, interior, variance, false, simple);
    }

    private static void Line(byte[] plane, int start, int step, int along, int count, int edge,
        int interior, int variance, bool macroblock, bool simple)
    {
        Span<int> samples = stackalloc int[8];
        for (var pixel = 0; pixel < count; pixel++)
        {
            var location = start + along * pixel;
            for (var i = 0; i < 8; i++) samples[i] = plane[location + (i - 4) * step];
            Vp8LoopFilter.Segment(samples, edge, interior, variance, macroblock, simple);
            for (var i = 1; i < 7; i++) plane[location + (i - 4) * step] = (byte)samples[i];
        }
    }
}
