using System.Buffers.Binary;

namespace Lokad.Image.Tooling;

internal static class PngOracle
{
    public static string[] DecodeArguments(int depth, byte[]? encoded = null)
    {
        var args = new List<string> { "-hide_banner", "-loglevel", "error", "-cpuflags", "0", "-noautorotate", "-f", "image2pipe", "-i", "pipe:0" };
        if (encoded is not null && PackedGrayKey(encoded) is byte gray)
        {
            if (depth != 8) throw new ArgumentException("Packed grayscale reference output must be RGBA8.", nameof(depth));
            args.AddRange(["-filter_threads", "1", "-vf", $"format=rgba,colorkey=color=0x{gray:x2}{gray:x2}{gray:x2}:similarity=0.01:blend=0"]);
        }
        args.AddRange(["-frames:v", "1", "-threads:v", "1", "-pix_fmt", depth == 16 ? "rgba64le" : "rgba", "-f", "rawvideo", "pipe:1"]);
        return args.ToArray();
    }

    public static byte[] Decode(byte[] encoded, int depth) => OracleProcess.Run(OracleProcess.Ffmpeg, DecodeArguments(depth, encoded), encoded);

    // The registered FFmpeg 8.1 PNG decoder ignores packed-gray tRNS. Apply the
    // declared key through its native colorkey filter. PNG sample expansion is
    // injective at depths 1/2/4: adjacent levels differ by at least 17/255, beyond
    // colorkey's 0.01 radius. Blend zero gives exact binary alpha and keeps RGB.
    // Facts: W3C PNG sections 4.4.5/11.3.1.1; FFmpeg's documented colorkey filter.
    // This bounded metadata reader is development-only and uses no codec helpers.
    public static byte? PackedGrayKey(ReadOnlySpan<byte> encoded)
    {
        if (encoded.Length < 33 || !encoded.StartsWith(new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 })
            || encoded[25] != 0 || encoded[24] is not (1 or 2 or 4)) return null;
        var maximum = (1 << encoded[24]) - 1;
        for (var position = 8; position < encoded.Length;)
        {
            if (encoded.Length - position < 12) throw new InvalidOperationException("Truncated PNG reference chunk.");
            var value = BinaryPrimitives.ReadUInt32BigEndian(encoded[position..]);
            if (value > encoded.Length - position - 12) throw new InvalidOperationException("Invalid PNG reference chunk length.");
            var length = (int)value; var type = encoded.Slice(position + 4, 4);
            if (type.SequenceEqual("IDAT"u8)) return null;
            if (type.SequenceEqual("tRNS"u8))
            {
                if (length != 2) throw new InvalidOperationException("Invalid packed-gray reference tRNS.");
                var sample = BinaryPrimitives.ReadUInt16BigEndian(encoded[(position + 8)..]);
                if (sample > maximum) throw new InvalidOperationException("Reference transparent sample exceeds depth.");
                return (byte)(sample * (255 / maximum));
            }
            position += length + 12;
        }
        return null;
    }
}
