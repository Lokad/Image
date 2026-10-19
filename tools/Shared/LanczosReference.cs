using System.Buffers.Binary;
using System.Numerics;

namespace Lokad.Image.Tooling;

// Original test-only scalar reference. Decimal Taylor sine and dense source-axis
// coefficients are independent of production binary64 plans and row caching.
// Pixel accumulation is a two-dimensional BigInteger sum with exact rounding.
internal static class LanczosReference
{
    private const decimal Pi = 3.1415926535897932384626433833m;
    private const long Unit = 1073741824;
    private static decimal Sin(decimal x)
    {
        x %= 2 * Pi; if (x > Pi) x -= 2 * Pi; if (x < -Pi) x += 2 * Pi;
        var term = x; var sum = x;
        for (var n = 1; n < 48; n++)
        {
            term = -term * x * x / (2 * n * (2 * n + 1)); var next = sum + term;
            if (sum == next) break; sum = next;
        }
        return sum;
    }
    private static decimal Kernel(decimal x)
    {
        x = Math.Abs(x); if (x >= 3 || x is 1 or 2) return 0; if (x == 0) return 1;
        return Sin(Pi * x) / (Pi * x) * (Sin(Pi * x / 3) / (Pi * x / 3));
    }
    internal static int[][] Axis(int source, int target)
    {
        var result = new int[target][];
        for (var output = 0; output < target; output++)
        {
            var center = (output + .5m) * source / target - .5m; var scale = Math.Max(1, (decimal)source / target);
            var raw = new decimal[source];
            for (var i = (int)decimal.Ceiling(center - 3 * scale); i <= decimal.Floor(center + 3 * scale); i++)
                raw[Math.Clamp(i, 0, source - 1)] += Kernel((i - center) / scale);
            var normalizer = raw.Sum(); var weights = new int[source]; long previous = 0; decimal prefix = 0;
            for (var i = 0; i < source; i++)
            {
                prefix += raw[i] / normalizer;
                var integral = i + 1 == source ? Unit : (long)decimal.Round(prefix * Unit, 0, MidpointRounding.AwayFromZero);
                weights[i] = checked((int)(integral - previous)); previous = integral;
            }
            result[output] = weights;
        }
        return result;
    }
    internal static byte[] Resize(byte[] source, int sourceWidth, int sourceHeight, int depth, int width, int height)
    {
        if (width == sourceWidth && height == sourceHeight) return source.ToArray();
        var horizontal = Axis(sourceWidth, width); var vertical = Axis(sourceHeight, height); var sample = depth / 8;
        var result = new byte[width * height * sample * 4]; var maximum = depth == 8 ? 255 : 65535;
        int Read(int x, int y, int channel)
        {
            var at = ((y * sourceWidth + x) * 4 + channel) * sample;
            return depth == 8 ? source[at] : BinaryPrimitives.ReadUInt16LittleEndian(source.AsSpan(at, 2));
        }
        for (var y = 0; y < height; y++)
        for (var x = 0; x < width; x++)
        {
            var sums = new BigInteger[4];
            for (var sy = 0; sy < sourceHeight; sy++)
            for (var sx = 0; sx < sourceWidth; sx++)
            {
                if (horizontal[x][sx] == 0 || vertical[y][sy] == 0) continue;
                var weight = (BigInteger)horizontal[x][sx] * vertical[y][sy]; var a = Read(sx, sy, 3);
                for (var c = 0; c < 3; c++) sums[c] += weight * a * Read(sx, sy, c);
                sums[3] += weight * a;
            }
            var alpha = Round(sums[3], (BigInteger)Unit * Unit, maximum);
            for (var c = 0; c < 4; c++)
            {
                var value = c == 3 ? alpha : alpha == 0 ? 0 : Round(sums[c], sums[3], maximum);
                var at = ((y * width + x) * 4 + c) * sample;
                if (depth == 8) result[at] = (byte)value; else BinaryPrimitives.WriteUInt16LittleEndian(result.AsSpan(at, 2), (ushort)value);
            }
        }
        return result;
    }
    private static int Round(BigInteger numerator, BigInteger denominator, int maximum)
        => numerator <= 0 ? 0 : (int)BigInteger.Min(maximum, (2 * numerator + denominator) / (2 * denominator));
}
