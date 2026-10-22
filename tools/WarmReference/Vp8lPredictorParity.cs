// Original source-only VP8L predictor experiment, not a production codec path.
// RFC9649 section3.5.1 defines predictor arithmetic and exact channel residuals.
// The pinned method4 control uses8x8 tiles; cost includes local/accumulated Shannon
// entropy, small-residual preference and neighboring-mode preference. Fixed-point
// logarithm values are derived mathematically, never imported as lookup tables.
using System;
using System.Numerics;

public static class Vp8lPredictorParity
{
    private const long Scale = 1L << 23;
    public static byte[] EncodeModes(byte[] rgba, int width, int height, int bits, bool subtractGreen=true)
        => Select(rgba, width, height, bits, out _, subtractGreen);

    private static long SLog(int count)
    {
        if (count < 2) return 0;
        if (count < 256) return (long)Math.Round(Scale * Math.Log2(count) * count);
        if (count >= 65536) return (long)Math.Floor((Scale / Math.Log(2)) * count * Math.Log(count) + 0.5);
        var shift = BitOperations.Log2((uint)count) - 7;
        var head = count >> shift;
        var log = (long)Math.Round(Scale * Math.Log2(head)) + shift * Scale;
        var remainder = count - (head << shift);
        return count * log + (long)(Scale / Math.Log(2)) * remainder;
    }
    private static long Bias(ReadOnlySpan<int> histogram)
    {
        var weighted = histogram[0] * Scale;
        var weight = 94 * Scale;
        for (var distance = 1; distance < 16; distance++)
        {
            weighted += (weight * (histogram[distance] + histogram[256 - distance]) + 50) / 100;
            weight = (weight * 6 + 5) / 10;
        }
        return -(weighted + 5) / 10;
    }
    private static long Cost(ReadOnlySpan<int> current, ReadOnlySpan<int> previous)
    {
        long value = 0;
        for (var plane = 0; plane < 4; plane++)
        {
            var tile = current.Slice(plane * 256, 256);
            var prior = previous.Slice(plane * 256, 256);
            var sumCurrent = 0; var sumCombined = 0;
            value += Bias(tile);
            for (var symbol = 0; symbol < 256; symbol++)
            {
                var x = tile[symbol]; var combined = x + prior[symbol];
                sumCurrent += x; sumCombined += combined;
                value -= SLog(x) + SLog(combined);
            }
            value += SLog(sumCurrent) + SLog(sumCombined);
        }
        return value;
    }
    internal static byte[] Select(byte[] rgba, int width, int height, int bits, out long[] scores, bool subtractGreen=true)
    {
        if (rgba.Length != checked(width * height * 4)) throw new ArgumentException("RGBA size differs.");
        var pixels = new uint[width * height];
        for (var i = 0; i < pixels.Length; i++)
        {
            var green = rgba[i * 4 + 1]; var adjustment = subtractGreen ? green : 0;
            pixels[i] = (uint)rgba[i * 4 + 3] << 24 | (uint)((rgba[i * 4] - adjustment) & 255) << 16
                | (uint)green << 8 | (uint)((rgba[i * 4 + 2] - adjustment) & 255);
        }
        var side = 1 << bits; var columns = (width + side - 1) >> bits; var rows = (height + side - 1) >> bits;
        var modes = new byte[columns * rows]; scores = new long[modes.Length * 14];
        var accumulated = new int[1024]; var histograms = new int[14 * 1024];
        for (var ty = 0; ty < rows; ty++)
        for (var tx = 0; tx < columns; tx++)
        {
            Array.Clear(histograms);
            for (var mode = 0; mode < 14; mode++)
            {
                var hist = histograms.AsSpan(mode * 1024, 1024);
                for (var y = ty * side; y < Math.Min((ty + 1) * side, height); y++)
                for (var x = tx * side; x < Math.Min((tx + 1) * side, width); x++)
                {
                    var at = y * width + x;
                    var prediction = y == 0 ? x == 0 ? 0xff000000u : pixels[at - 1]
                        : x == 0 ? pixels[at - width] : Predict(mode, pixels[at - 1], pixels[at - width], pixels[at - width - 1], pixels[x == width - 1 ? y * width : at - width + 1]);
                    var actual = pixels[at];
                    for (var plane = 0; plane < 4; plane++)
                    {
                        var shift = (3 - plane) * 8;
                        var residual = (int)(((actual >> shift) - (prediction >> shift)) & 255);
                        hist[plane * 256 + residual]++;
                    }
                }
            }
            var tile = ty * columns + tx; var chosen = 0; var best = long.MaxValue;
            for (var mode = 0; mode < 14; mode++)
            {
                var value = Cost(histograms.AsSpan(mode * 1024, 1024), accumulated);
                if (tx != 0 && mode == modes[tile - 1]) value -= 15 * Scale;
                if (ty != 0 && mode == modes[tile - columns]) value -= 15 * Scale;
                scores[tile * 14 + mode] = value;
                if (value < best) { best = value; chosen = mode; }
            }
            modes[tile] = (byte)chosen;
            for (var i = 0; i < 1024; i++) accumulated[i] += histograms[chosen * 1024 + i];
        }
        return modes;
    }
    public static byte[] Residual(byte[] rgba, int width, int height, int bits, byte[] modes, bool subtractGreen=true)
    {
        var input = new uint[width * height];
        for (var i = 0; i < input.Length; i++)
        {
            var green = rgba[i * 4 + 1]; var adjustment = subtractGreen ? green : 0;
            input[i] = (uint)rgba[i * 4 + 3] << 24 | (uint)((rgba[i * 4] - adjustment) & 255) << 16
                | (uint)green << 8 | (uint)((rgba[i * 4 + 2] - adjustment) & 255);
        }
        var columns = (width + (1 << bits) - 1) >> bits;
        var result = new byte[rgba.Length];
        for (var y = 0; y < height; y++)
        for (var x = 0; x < width; x++)
        {
            var at = y * width + x;
            var prediction = y == 0 ? x == 0 ? 0xff000000u : input[at - 1]
                : x == 0 ? input[at - width]
                : Predict(modes[(y >> bits) * columns + (x >> bits)], input[at - 1], input[at - width], input[at - width - 1], input[x == width - 1 ? y * width : at - width + 1]);
            uint value = 0;
            for (var shift = 0; shift < 32; shift += 8) value |= (((input[at] >> shift) - (prediction >> shift)) & 255) << shift;
            System.Buffers.Binary.BinaryPrimitives.WriteUInt32LittleEndian(result.AsSpan(at * 4), value);
        }
        return result;
    }


    internal static uint Predict(int mode, uint left, uint top, uint topLeft, uint topRight)
        => mode switch
        {
            0 => 0xFF000000, 1 => left, 2 => top, 3 => topRight, 4 => topLeft,
            5 => Average(Average(left, topRight), top), 6 => Average(left, topLeft),
            7 => Average(left, top), 8 => Average(topLeft, top), 9 => Average(top, topRight),
            10 => Average(Average(left, topLeft), Average(top, topRight)),
            11 => Select(left, top, topLeft), 12 => ClampPrediction(left, top, topLeft, false),
            13 => ClampPrediction(Average(left, top), 0, topLeft, true),
            _ => throw new FormatException("Invalid VP8L predictor.")
        };

    private static uint Average(uint first, uint second)
    {
        uint result = 0;
        for (var shift = 0; shift < 32; shift += 8) result |= ((first >> shift & 255) + (second >> shift & 255)) / 2 << shift;
        return result;
    }

    private static uint Select(uint left, uint top, uint topLeft)
    {
        int leftError = 0, topError = 0;
        for (var shift = 0; shift < 32; shift += 8)
        {
            var l = (int)(left >> shift & 255); var t = (int)(top >> shift & 255); var tl = (int)(topLeft >> shift & 255);
            leftError += Math.Abs(t - tl); topError += Math.Abs(l - tl);
        }
        return leftError < topError ? left : top;
    }

    private static uint ClampPrediction(uint first, uint second, uint previous, bool half)
    {
        uint result = 0;
        for (var shift = 0; shift < 32; shift += 8)
        {
            var a = (int)(first >> shift & 255); var b = (int)(second >> shift & 255); var c = (int)(previous >> shift & 255);
            result |= (uint)Math.Clamp(half ? a + (a - c) / 2 : a + b - c, 0, 255) << shift;
        }
        return result;
    }

}
