using System.Buffers.Binary;

namespace Lokad.Image.Tooling;

/// <summary>Original small T.81 streams whose coefficients and scan bits are chosen analytically.</summary>
internal static class AuthoredJpeg
{
    private static readonly byte[] AcSymbols = BuildAcSymbols();

    public static byte[] GrayscaleDcGrid(int width, int height, bool progressive)
    {
        using var output = Header(width, height, 0, progressive: progressive);
        var columns = (width + 7) / 8; var rows = (height + 7) / 8;
        Scan(output, 0, progressive ? 0 : 63, 0, 0, bits =>
        {
            var predictor = 0;
            for (var by = 0; by < rows; by++)
            for (var bx = 0; bx < columns; bx++)
            {
                // Unit quantization and DC-only IDCT give exactly this byte.
                var dc = (((bx + by * columns) & 255) - 128) * 8;
                bits.Dc(dc - predictor); predictor = dc;
                if (!progressive) bits.Ac(0);
            }
        });
        if (progressive)
            Scan(output, 1, 63, 0, 0, bits =>
            {
                for (var block = 0; block < columns * rows; block++) bits.Ac(0);
            });
        output.Write(new byte[] { 255, 217 }); return output.ToArray();
    }

    public static byte[] DirectRgb(bool adobe = true)
    {
        using var output = Header(8, 8, 0, JpegSampling.YCbCr444, progressive: false, rgbIds: !adobe, adobeRgb: adobe);
        Scan(output, 0, 63, 0, 0, bits =>
        {
            foreach (var sample in new[] { 45, 105, 205 }) { bits.Dc((sample - 128) * 8); bits.Ac(0); }
        }, adobe ? new byte[] { 1, 2, 3 } : new byte[] { 82, 71, 66 });
        output.Write(new byte[] { 255, 217 }); return output.ToArray();
    }

    public static byte[] RefinedBlock(bool negativeDc = false)
    {
        using var output = Header(8, 8, 0);
        Scan(output, 0, 0, 0, 2, bits => bits.Dc(negativeDc ? -21 : 20));
        Scan(output, 1, 63, 0, 1, bits =>
        {
            bits.Ac(4); bits.Signed(10, 4);
            bits.Ac(3); bits.Signed(-4, 3);
            bits.Ac(0);
        });
        Scan(output, 0, 0, 2, 1, bits => bits.Write(1, 1));
        Scan(output, 0, 0, 1, 0, bits => bits.Write(1, 1));
        Scan(output, 1, 63, 1, 0, bits =>
        {
            bits.Ac(1); bits.Write(0, 1); // Insert a new -1 after refining two nonzero coefficients.
            bits.Write(1, 1); bits.Write(1, 1);
            bits.Ac(0);
        });
        output.Write(new byte[] { 255, 217 });
        return output.ToArray();
    }

    public static byte[] EobRun(int restartInterval = 0, bool crossRestart = false)
    {
        using var output = Header(24, 8, restartInterval);
        Scan(output, 0, 0, 0, 0, bits =>
        {
            for (var block = 0; block < 3; block++)
            {
                if (restartInterval == 1 && block != 0) bits.Restart(block - 1);
                bits.Dc(0);
            }
        });
        Scan(output, 1, 63, 0, 1, bits =>
        {
            for (var block = 0; block < 3; block++)
            {
                if (restartInterval == 1 && block != 0) bits.Restart(block - 1);
                bits.Ac(1); bits.Write(1, 1); // First AC coefficient = +2.
                bits.Ac(0);
            }
        });
        Scan(output, 1, 63, 1, 0, bits =>
        {
            if (restartInterval == 0 || crossRestart)
            {
                bits.Ac(0x10); bits.Write(1, 1); // EOBRUN = 2 + 1 = 3 blocks.
                bits.Write(7, 3); // Refine the existing +2 in each block to +3.
            }
            else
            {
                for (var block = 0; block < 3; block++)
                {
                    if (block != 0) bits.Restart(block - 1);
                    bits.Ac(0); bits.Write(1, 1);
                }
            }
        });
        output.Write(new byte[] { 255, 217 });
        return output.ToArray();
    }

    public static byte[] ExpectedRefinedBlock(bool negativeDc = false)
        => Expected(8, 8, negativeDc ? -81 : 83, 21, -9, -1);

    public static byte[] ExpectedEobRun() => Expected(24, 8, 0, 3, 0, 0);

    public static byte[] EmptyEobRun(bool exceedsScan = false)
    {
        using var output = Header(24, 8, 0);
        Scan(output, 0, 0, 0, 0, bits => { bits.Dc(0); bits.Dc(0); bits.Dc(0); });
        Scan(output, 1, 63, 0, 1, bits =>
        {
            bits.Ac(exceedsScan ? (byte)0x20 : (byte)0x10);
            bits.Write(1, exceedsScan ? 2 : 1); // EOBRUN 5 (invalid) or 3.
        });
        Scan(output, 1, 63, 1, 0, bits => { bits.Ac(0x10); bits.Write(1, 1); });
        output.Write(new byte[] { 255, 217 }); return output.ToArray();
    }

    public static byte[] SparseBands()
    {
        using var output = Header(8, 8, 0);
        Scan(output, 0, 0, 0, 0, bits => bits.Dc(0));
        Scan(output, 1, 20, 0, 1, bits =>
        {
            bits.Ac(1); bits.Signed(1, 1); // k=1, +2.
            bits.Ac(0xF0); bits.Ac(1); bits.Signed(-1, 1); // k=18, -2, after 16 zeros.
            bits.Ac(0);
        });
        Scan(output, 21, 63, 0, 1, bits =>
        {
            bits.Ac(0x41); bits.Signed(1, 1); // k=25, +2.
            bits.Ac(0xF0); bits.Ac(0xF0); bits.Ac(0x51); bits.Signed(-1, 1); // k=63, -2.
        });
        Scan(output, 1, 20, 1, 0, bits => { bits.Ac(0); bits.Write(3, 2); });
        Scan(output, 21, 63, 1, 0, bits =>
        {
            bits.Ac(0xF0); bits.Write(1, 1); // Refine k=25 while skipping 16 zero coefficients.
            bits.Ac(0xF0);
            bits.Ac(0x11); bits.Write(1, 1); // New +1 at k=55 after another zero.
            bits.Ac(0); bits.Write(1, 1); // Refine k=63 after EOB.
        });
        output.Write(new byte[] { 255, 217 }); return output.ToArray();
    }

    public static byte[] ExpectedSparseBands()
    {
        // Natural positions derived from the diagonal geometry: k=1 -> (1,0),
        // k=18 -> (2,3), k=25 -> (4,2), k=55 -> (6,5), k=63 -> (7,7).
        (int U, int V, int Value)[] frequencies = [(1, 0, 3), (2, 3, -3), (4, 2, 3), (6, 5, 1), (7, 7, -3)];
        var result = new byte[256];
        for (var y = 0; y < 8; y++)
        for (var x = 0; x < 8; x++)
        {
            double sample = 128;
            foreach (var (u, v, value) in frequencies)
                sample += value / 4.0 * (u == 0 ? 1 / Math.Sqrt(2) : 1) * (v == 0 ? 1 / Math.Sqrt(2) : 1)
                    * Math.Cos((2 * x + 1) * u * Math.PI / 16) * Math.Cos((2 * y + 1) * v * Math.PI / 16);
            var rounded = (byte)Math.Clamp(Math.Round(sample, MidpointRounding.AwayFromZero), 0, 255);
            var position = (y * 8 + x) * 4;
            result[position] = rounded; result[position + 1] = rounded; result[position + 2] = rounded; result[position + 3] = 255;
        }
        return result;
    }

    public static byte[] ProgressiveColor(int width, int height, JpegSampling sampling, int restartInterval)
    {
        if (sampling == JpegSampling.Grayscale) throw new ArgumentOutOfRangeException(nameof(sampling));
        return ProgressiveFrame(width, height, sampling, restartInterval);
    }

    public static byte[] ProgressiveFrame(int width, int height, JpegSampling sampling, int restartInterval)
    {
        var componentCount = sampling == JpegSampling.Grayscale ? 1 : 3;
        byte[] componentIds = componentCount == 1 ? [1] : [1, 2, 3];
        var h = sampling is JpegSampling.YCbCr422 or JpegSampling.YCbCr420 ? 2 : 1;
        var v = sampling == JpegSampling.YCbCr420 ? 2 : 1;
        var columns = (width + h * 8 - 1) / (h * 8); var rows = (height + v * 8 - 1) / (v * 8);
        using var output = Header(width, height, restartInterval, sampling);
        foreach (var refine in new[] { false, true })
        {
            Scan(output, 0, 0, refine ? 1 : 0, refine ? 0 : 1, bits =>
            {
                var predictors = new int[3];
                for (var mcu = 0; mcu < columns * rows; mcu++)
                {
                    if (restartInterval != 0 && mcu != 0 && mcu % restartInterval == 0)
                    {
                        bits.Restart(mcu / restartInterval - 1); Array.Clear(predictors);
                    }
                    for (var component = 0; component < componentCount; component++)
                    {
                        var bh = component == 0 ? h : 1; var bv = component == 0 ? v : 1;
                        for (var by = 0; by < bv; by++)
                        for (var bx = 0; bx < bh; bx++)
                        {
                            var dc = Dc(mcu % columns * bh + bx, mcu / columns * bv + by, component);
                            if (refine) bits.Write(dc & 1, 1);
                            else { bits.Dc((dc >> 1) - predictors[component]); predictors[component] = dc >> 1; }
                        }
                    }
                }
            }, componentIds);
            for (var component = 0; component < componentCount; component++)
            {
                var sampleWidth = component == 0 ? width : (width + h - 1) / h;
                var sampleHeight = component == 0 ? height : (height + v - 1) / v;
                var blockColumns = (sampleWidth + 7) / 8; var blockRows = (sampleHeight + 7) / 8;
                Scan(output, 1, 63, refine ? 1 : 0, refine ? 0 : 1, bits =>
                {
                    for (var block = 0; block < blockColumns * blockRows; block++)
                    {
                        if (restartInterval != 0 && block != 0 && block % restartInterval == 0)
                            bits.Restart(block / restartInterval - 1);
                        var sign = ((block % blockColumns + block / blockColumns + component) & 1) == 0 ? 1 : -1;
                        if (refine)
                        {
                            bits.Ac(1); bits.Write(sign > 0 ? 1 : 0, 1);
                            bits.Write(1, 1); bits.Write(1, 1); // Existing +/−36 and −/+20 gain their low bit.
                        }
                        else
                        {
                            bits.Ac(5); bits.Signed(sign * 18, 5);
                            bits.Ac(4); bits.Signed(-sign * 10, 4);
                        }
                        bits.Ac(0);
                    }
                }, (byte)(component + 1));
            }
        }
        output.Write(new byte[] { 255, 217 }); return output.ToArray();

        static int Dc(int bx, int by, int component) => (bx * 41 + by * 23 + component * 53) % 193 - 96;
    }

    private static byte[] Expected(int width, int height, int dc, int horizontal, int vertical, int secondVertical)
    {
        var result = new byte[width * height * 4];
        for (var y = 0; y < height; y++)
        for (var x = 0; x < width; x++)
        {
            var sample = 128 + dc / 8.0
                + horizontal * Math.Cos((2 * (x % 8) + 1) * Math.PI / 16) / (4 * Math.Sqrt(2))
                + vertical * Math.Cos((2 * (y % 8) + 1) * Math.PI / 16) / (4 * Math.Sqrt(2))
                + secondVertical * Math.Cos((2 * (y % 8) + 1) * Math.PI / 8) / (4 * Math.Sqrt(2));
            var value = (byte)Math.Clamp(Math.Round(sample, MidpointRounding.AwayFromZero), 0, 255);
            var position = (y * width + x) * 4;
            result[position] = value; result[position + 1] = value; result[position + 2] = value; result[position + 3] = 255;
        }
        return result;
    }

    private static MemoryStream Header(int width, int height, int restartInterval, JpegSampling sampling = JpegSampling.Grayscale,
        bool progressive = true, bool rgbIds = false, bool adobeRgb = false)
    {
        var output = new MemoryStream(); output.Write(new byte[] { 255, 216 });
        if (adobeRgb) Segment(output, 0xEE, new byte[] { 65, 100, 111, 98, 101, 0, 100, 0, 0, 0, 0, 0 });
        var quantization = new byte[65]; Array.Fill(quantization, (byte)1, 1, 64);
        Segment(output, 0xDB, quantization);
        var count = sampling == JpegSampling.Grayscale ? 1 : 3;
        var h = sampling is JpegSampling.YCbCr422 or JpegSampling.YCbCr420 ? 2 : 1;
        var v = sampling == JpegSampling.YCbCr420 ? 2 : 1;
        var frame = new byte[6 + count * 3]; frame[0] = 8; frame[5] = (byte)count;
        for (var component = 0; component < count; component++)
        {
            frame[6 + 3 * component] = rgbIds ? "RGB"u8[component] : (byte)(component + 1);
            frame[7 + 3 * component] = component == 0 ? (byte)(h << 4 | v) : (byte)0x11;
        }
        BinaryPrimitives.WriteUInt16BigEndian(frame.AsSpan(1), (ushort)height);
        BinaryPrimitives.WriteUInt16BigEndian(frame.AsSpan(3), (ushort)width);
        Segment(output, progressive ? (byte)0xC2 : (byte)0xC0, frame);
        var dc = new byte[29]; dc[4] = 12;
        for (var i = 0; i < 12; i++) dc[17 + i] = (byte)i;
        Segment(output, 0xC4, dc);
        var ac = new byte[17 + AcSymbols.Length]; ac[0] = 0x10; ac[8] = (byte)AcSymbols.Length;
        AcSymbols.CopyTo(ac, 17); Segment(output, 0xC4, ac);
        if (restartInterval != 0)
        {
            var interval = new byte[2]; BinaryPrimitives.WriteUInt16BigEndian(interval, (ushort)restartInterval);
            Segment(output, 0xDD, interval);
        }
        return output;
    }

    private static void Scan(Stream output, int start, int end, int high, int low, Action<Bits> write, params byte[] components)
    {
        if (components.Length == 0) components = [1];
        var scan = new byte[4 + components.Length * 2]; scan[0] = (byte)components.Length;
        for (var i = 0; i < components.Length; i++) scan[1 + 2 * i] = components[i];
        scan[^3] = (byte)start; scan[^2] = (byte)end; scan[^1] = (byte)(high << 4 | low);
        Segment(output, 0xDA, scan);
        var bits = new Bits(output); write(bits); bits.Align();
    }

    private static void Segment(Stream output, byte marker, byte[] payload)
    {
        output.WriteByte(255); output.WriteByte(marker);
        output.WriteByte((byte)((payload.Length + 2) >> 8)); output.WriteByte((byte)(payload.Length + 2));
        output.Write(payload);
    }

    private static byte[] BuildAcSymbols()
    {
        var symbols = new List<byte> { 0 };
        for (var run = 0; run < 16; run++)
        for (var category = 1; category <= 10; category++) symbols.Add((byte)(run << 4 | category));
        symbols.Add(0xF0);
        for (var run = 1; run <= 14; run++) symbols.Add((byte)(run << 4));
        return symbols.ToArray();
    }

    private sealed class Bits(Stream output)
    {
        private int value, count;
        public void Write(int encoded, int length)
        {
            for (var bit = length - 1; bit >= 0; bit--)
            {
                value = value * 2 + ((encoded >> bit) & 1);
                if (++count == 8)
                {
                    output.WriteByte((byte)value); if (value == 255) output.WriteByte(0);
                    value = 0; count = 0;
                }
            }
        }
        public void Signed(int encoded, int category) => Write(encoded < 0 ? encoded + (1 << category) - 1 : encoded, category);
        public void Dc(int difference)
        {
            var category = 0;
            for (var magnitude = Math.Abs(difference); magnitude != 0; magnitude >>= 1) category++;
            Write(category, 4); Signed(difference, category);
        }
        public void Ac(byte symbol) => Write(Array.IndexOf(AcSymbols, symbol), 8);
        public void Align() { if (count != 0) Write((1 << (8 - count)) - 1, 8 - count); }
        public void Restart(int index) { Align(); output.WriteByte(255); output.WriteByte((byte)(0xD0 + index % 8)); }
    }
}
