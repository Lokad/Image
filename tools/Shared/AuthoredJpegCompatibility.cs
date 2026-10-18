using System.Buffers.Binary;
using System.Numerics;

namespace Lokad.Image.Tooling;

// Original T.81 DC-only images. Unit quantization makes every block's samples
// analytical. Tables are authored tiny canonical codes, not upstream data.
internal static class AuthoredJpegCompatibility
{
    internal static byte[] Create(int width, int height, byte[] values, int h = 1, int v = 1,
        bool blackFull = false, int? adobe = null, bool progressive = false, int restart = 0, bool vary = false)
    {
        using var output = new MemoryStream(); output.Write(new byte[] { 255, 216 });
        if (adobe is not null) Segment(0xEE, [65, 100, 111, 98, 101, 0, 100, 0, 0, 0, 0, (byte)adobe.Value]);
        var quantization = new byte[65]; quantization.AsSpan(1).Fill(1); Segment(0xDB, quantization);
        var count = values.Length; var hs = new int[count]; var vs = new int[count];
        Array.Fill(hs, 1); Array.Fill(vs, 1); hs[0] = h; vs[0] = v;
        if (count == 4 && blackFull) { hs[3] = h; vs[3] = v; }
        var frame = new byte[6 + 3 * count]; frame[0] = 8; frame[5] = (byte)count;
        BinaryPrimitives.WriteUInt16BigEndian(frame.AsSpan(1), (ushort)height);
        BinaryPrimitives.WriteUInt16BigEndian(frame.AsSpan(3), (ushort)width);
        for (var c = 0; c < count; c++) { frame[6 + c * 3] = (byte)(c + 1); frame[7 + c * 3] = (byte)(hs[c] * 16 + vs[c]); }
        Segment(progressive ? (byte)0xC2 : (byte)0xC0, frame);
        var dc = new byte[29]; dc[4] = 12;
        for (var i = 0; i < 12; i++) dc[17 + i] = (byte)i;
        Segment(0xC4, dc);
        var ac = new byte[18]; ac[0] = 16; ac[1] = 1; Segment(0xC4, ac);
        if (restart != 0) { var dri = new byte[2]; BinaryPrimitives.WriteUInt16BigEndian(dri, (ushort)restart); Segment(0xDD, dri); }
        var selectors = Enumerable.Range(1, count).Select(i => (byte)i).ToArray();
        Scan(selectors, 0, progressive ? 0 : 63);
        var bits = new Bits(output); var predictors = new int[count];
        var columns = (width + h * 8 - 1) / (h * 8); var rows = (height + v * 8 - 1) / (v * 8);
        for (var mcu = 0; mcu < columns * rows; mcu++)
        {
            if (restart != 0 && mcu != 0 && mcu % restart == 0) { bits.Restart(mcu / restart - 1); Array.Clear(predictors); }
            for (var c = 0; c < count; c++)
            for (var by = 0; by < vs[c]; by++)
            for (var bx = 0; bx < hs[c]; bx++)
            {
                var sample = Sample(values[c], mcu % columns * hs[c] + bx, mcu / columns * vs[c] + by, vary);
                var coefficient = (sample - 128) * 8; bits.Dc(coefficient - predictors[c]); predictors[c] = coefficient;
                if (!progressive) bits.Write(0, 1);
            }
        }
        bits.Align();
        if (progressive)
        {
            for (var c = 0; c < count; c++)
            {
                Scan([(byte)(c + 1)], 1, 63); bits = new Bits(output);
                var blocks = ((width * hs[c] + h * 8 - 1) / (h * 8)) * ((height * vs[c] + v * 8 - 1) / (v * 8));
                for (var b = 0; b < blocks; b++)
                {
                    if (restart != 0 && b != 0 && b % restart == 0) bits.Restart(b / restart - 1);
                    bits.Write(0, 1);
                }
                bits.Align();
            }
        }
        output.Write(new byte[] { 255, 217 }); return output.ToArray();

        void Segment(byte marker, byte[] payload)
        {
            output.Write(new byte[] { 255, marker, (byte)((payload.Length + 2) >> 8), (byte)(payload.Length + 2) }); output.Write(payload);
        }
        void Scan(byte[] components, int start, int end)
        {
            var scan = new byte[4 + components.Length * 2]; scan[0] = (byte)components.Length;
            for (var c = 0; c < components.Length; c++) scan[1 + c * 2] = components[c];
            scan[^3] = (byte)start; scan[^2] = (byte)end; Segment(0xDA, scan);
        }
    }

    internal static int Sample(int basis, int bx, int by, bool vary) => vary ? (basis + bx * 13 + by * 29) & 255 : basis;

    private sealed class Bits(Stream output)
    {
        private int pending, value;
        internal void Write(int field, int count)
        {
            for (var i = count - 1; i >= 0; i--)
            {
                value = value * 2 + (field >> i & 1);
                if (++pending != 8) continue;
                output.WriteByte((byte)value); if (value == 255) output.WriteByte(0);
                pending = 0; value = 0;
            }
        }
        internal void Dc(int difference)
        {
            var count = difference == 0 ? 0 : 32 - BitOperations.LeadingZeroCount((uint)Math.Abs(difference));
            Write(count, 4); if (count != 0) Write(difference < 0 ? difference + (1 << count) - 1 : difference, count);
        }
        internal void Align() { if (pending != 0) Write((1 << (8 - pending)) - 1, 8 - pending); }
        internal void Restart(int index) { Align(); output.WriteByte(255); output.WriteByte((byte)(0xD0 + (index & 7))); }
    }
}
