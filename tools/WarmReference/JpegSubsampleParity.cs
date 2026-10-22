// Original development-only chroma stage for the frozen 513x259 JPEG controls.
// BT.601 matrix constants are generated from decimal definitions. Area weights
// come from pixel-cell overlap at a Q16 phase; no implementation tables, oracle
// samples or packet contents are inputs. Explicit fixed-point rounding preserves
// the pinned conversion boundary rather than averaging already rounded bytes.
using System;
using System.Collections.Generic;

public static class JpegSubsampleParity
{
    private const int Width = 513, Height = 259, ChromaWidth = 257;
    private readonly struct Tap
    {
        public readonly int Source, Weight;
        public Tap(int source, int weight) { Source = source; Weight = weight; }
    }

    private static int Coefficient(int value) => Math.Sign(value)
        * (int)((Math.Abs((long)value) * 224 * 32768 + 127500) / 255000);

    private static int FullQ7(int limitedQ7)
    {
        int scale = (255 * 16384 + 223) / 224;
        int offset = (255 << 21) - 240 * 128 * scale + 8192;
        return Math.Min(32767, (limitedQ7 * scale + offset) >> 14);
    }

    // Three nonzero cells suffice for the frozen near-2:1 reductions. Precision
    // is kept through normalization; quantization carries error left to right.
    private static Tap[][] Area(int sourceLength, int targetLength, int bits)
    {
        var result = new Tap[targetLength][];
        long step = ((long)sourceLength * 65536 + targetLength / 2) / targetLength;
        const long areaUnit = 1L << 46;
        for (int i = 0; i < targetLength; i++)
        {
            long center = step - 65536 + 2 * i * step; // Doubled source coordinate, Q16.
            int first = (int)((center - 65536) / 131072);
            var cells = new SortedDictionary<int, long>();
            bool leading = true;
            for (int j = first; j < first + 3; j++)
            {
                long distance = Math.Abs(j * 131072L - center) * 8192 * targetLength / sourceLength;
                long area = Math.Clamp((1L << 45) - (distance - (1L << 29)) * step, 0, areaUnit);
                // Pinned phase reduction discards only negligible leading cells.
                if (leading && area <= areaUnit / 500) continue;
                leading = false;
                int source = Math.Clamp(j, 0, sourceLength - 1);
                cells.TryGetValue(source, out long value); cells[source] = value + area * 256;
            }
            long sum = 0; foreach (long weight in cells.Values) sum += weight;
            long denominator = (sum + (1L << (bits - 1))) >> bits;
            long error = 0; var taps = new List<Tap>();
            foreach (var cell in cells)
            {
                long value = cell.Value + error;
                int weight = (int)((value + denominator / 2) / denominator);
                error = value - weight * denominator;
                taps.Add(new Tap(cell.Key, weight));
            }
            result[i] = taps.ToArray();
        }
        return result;
    }

    public static byte[] Convert(byte[] source, int depth, int sampling)
    {
        if (depth != 8 && depth != 16 || source.Length != Width * Height * depth / 2
            || sampling != 422 && sampling != 420) throw new ArgumentException("Frozen JPEG source and sampling required.");
        int chromaHeight = sampling == 422 ? Height : 130;
        var result = new byte[Width * Height + 2 * ChromaWidth * chromaHeight];
        JpegColorParity.Convert(source, depth).AsSpan(0, Width * Height).CopyTo(result);
        var limited = new short[2 * Width * Height];
        int ur = Coefficient(-169), ug = Coefficient(-331), ub = Coefficient(500);
        int vr = Coefficient(500), vg = Coefficient(-419), vb = Coefficient(-81);
        for (int i = 0; i < Width * Height; i++)
        {
            int r, g, b;
            if (depth == 8)
            {
                int a = source[i * 4 + 3];
                r = (source[i * 4] * a + 32 * (255 - a) + 127) / 255;
                g = (source[i * 4 + 1] * a + 64 * (255 - a) + 127) / 255;
                b = (source[i * 4 + 2] * a + 96 * (255 - a) + 127) / 255;
            }
            else
            {
                int at = i * 8; long a = source[at + 6] | source[at + 7] << 8;
                const long maximum = 65535, denominator = maximum * maximum;
                r = (int)(((source[at] | source[at + 1] << 8) * a * 255 + 32 * (maximum - a) * maximum + denominator / 2) / denominator);
                g = (int)(((source[at + 2] | source[at + 3] << 8) * a * 255 + 64 * (maximum - a) * maximum + denominator / 2) / denominator);
                b = (int)(((source[at + 4] | source[at + 5] << 8) * a * 255 + 96 * (maximum - a) * maximum + denominator / 2) / denominator);
            }
            limited[i] = (short)((ur * r + ug * g + ub * b + (128 << 15) + 256) >> 9);
            limited[Width * Height + i] = (short)((vr * r + vg * g + vb * b + (128 << 15) + 256) >> 9);
        }
        Tap[][] horizontal = Area(Width, ChromaWidth, 14);
        Tap[][] vertical = sampling == 420 ? Area(Height, chromaHeight, 12) : null;
        var rows = new short[ChromaWidth * Height];
        for (int component = 0; component < 2; component++)
        {
            for (int y = 0; y < Height; y++)
                for (int x = 0; x < ChromaWidth; x++)
                {
                    int sum = 0;
                    foreach (Tap tap in horizontal[x]) sum += limited[component * Width * Height + y * Width + tap.Source] * tap.Weight;
                    rows[y * ChromaWidth + x] = (short)FullQ7(Math.Min(32767, sum >> 13));
                }
            for (int y = 0; y < chromaHeight; y++)
                for (int x = 0; x < ChromaWidth; x++)
                {
                    int value;
                    if (sampling == 422) value = (rows[y * ChromaWidth + x] + 64) >> 7;
                    else
                    {
                        int sum = 1 << 18;
                        foreach (Tap tap in vertical[y]) sum += rows[tap.Source * ChromaWidth + x] * tap.Weight;
                        value = sum >> 19;
                    }
                    result[Width * Height + component * ChromaWidth * chromaHeight + y * ChromaWidth + x] = (byte)Math.Clamp(value, 0, 255);
                }
        }
        return result;
    }
}
