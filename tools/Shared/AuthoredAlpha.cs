using System.Buffers.Binary;
using System.Text;

namespace Lokad.Image.Tooling;

/// <summary>Original raw/compressed ALPH containers around an independently encoded VP8 image.</summary>
internal static class AuthoredAlpha
{
    public static byte[] Samples(int width, int height)
    {
        var samples = new byte[width * height];
        for (var y = 0; y < height; y++)
        for (var x = 0; x < width; x++) samples[y * width + x] = (byte)(23 * x + 37 * y + 7 * x * y);
        return samples;
    }

    public static byte[] Wrap(byte[] opaqueWebp, int width, int height, int filter, int orientation = 1, bool metadataFirst = false, bool compressed = false)
    {
        if (filter is < 0 or > 3) throw new ArgumentOutOfRangeException(nameof(filter));
        var samples = Samples(width,height); var alpha = new byte[samples.Length + 1]; alpha[0] = (byte)(filter << 2);
        for (var y = 0; y < height; y++)
        for (var x = 0; x < width; x++)
        {
            var position = y * width + x; var prediction = 0;
            if (filter != 0 && position != 0)
            {
                if (x == 0) prediction = samples[position - width];
                else if (y == 0 || filter == 1) prediction = samples[position - 1];
                else if (filter == 2) prediction = samples[position - width];
                else prediction = Math.Clamp(samples[position - 1] + samples[position - width] - samples[position - width - 1],0,255);
            }
            alpha[position + 1] = unchecked((byte)(samples[position] - prediction));
        }
        if (compressed)
        {
            // RFC 9649 2.7.1.2: ALPH omits the five-byte VP8L signature/dimension header.
            // Pin full-alphabet trees for admission controls independently of the
            // production encoder's compression choices; green carries alpha.
            var entropy = AuthoredWebp.FixedAlphaResiduals(alpha.AsSpan(1));
            alpha = new byte[entropy.Length + 1]; alpha[0] = (byte)(1 | filter << 2);
            entropy.CopyTo(alpha, 1);
        }
        byte[]? pixels = null;
        for (var offset = 12; offset < opaqueWebp.Length;)
        {
            var length = BinaryPrimitives.ReadInt32LittleEndian(opaqueWebp.AsSpan(offset + 4));
            if (opaqueWebp.AsSpan(offset,4).SequenceEqual("VP8 "u8)) { pixels = opaqueWebp.AsSpan(offset + 8,length).ToArray(); break; }
            offset += 8 + length + (length & 1);
        }
        if (pixels is null) throw new InvalidOperationException("An original opaque VP8 payload is required.");
        var extended = new byte[10]; extended[0] = (byte)(16 | (orientation == 1 ? 0 : 8));
        Write24(extended.AsSpan(4),width - 1); Write24(extended.AsSpan(7),height - 1);
        byte[] exif = [73,73,42,0,8,0,0,0,1,0,18,1,3,0,1,0,0,0,(byte)orientation,0,0,0,0,0,0,0];
        using var body = new MemoryStream(); body.Write("WEBP"u8);
        Chunk(body,"VP8X",extended);
        if (orientation != 1 && metadataFirst) Chunk(body,"EXIF",exif);
        Chunk(body,"ALPH",alpha); Chunk(body,"VP8 ",pixels);
        if (orientation != 1 && !metadataFirst) Chunk(body,"EXIF",exif);
        using var output = new MemoryStream(); output.Write("RIFF"u8);
        Span<byte> size = stackalloc byte[4]; BinaryPrimitives.WriteInt32LittleEndian(size,(int)body.Length);
        output.Write(size); output.Write(body.ToArray()); return output.ToArray();
    }

    private static void Write24(Span<byte> target,int value)
    { target[0] = (byte)value; target[1] = (byte)(value >> 8); target[2] = (byte)(value >> 16); }

    private static void Chunk(Stream output,string name,byte[] payload)
    {
        output.Write(Encoding.ASCII.GetBytes(name)); Span<byte> size = stackalloc byte[4];
        BinaryPrimitives.WriteInt32LittleEndian(size,payload.Length); output.Write(size); output.Write(payload);
        if ((payload.Length & 1) != 0) output.WriteByte(0);
    }
}
