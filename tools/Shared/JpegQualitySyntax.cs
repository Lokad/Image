using System.Buffers.Binary;
using System.Security.Cryptography;

namespace Lokad.Image.Tooling;

// Original, bounded development header reader; T.81 B.2 marker/segment layouts.
internal static class JpegQualitySyntax
{
    internal sealed record Header(int Width, int Height, int Depth, int Components, string Sampling,
        byte[][] QuantizationTables, string QuantizationHash);

    public static Header Read(byte[] encoded)
    {
        if (encoded.Length < 2 || encoded[0] != 255 || encoded[1] != 216) throw new InvalidOperationException("Expected JPEG SOI.");
        var tables = new List<byte[]>(); int width = 0, height = 0, depth = 0, components = 0; var sampling = "";
        for (var position = 2; position < encoded.Length;)
        {
            if (encoded[position++] != 255) throw new InvalidOperationException("Expected JPEG marker.");
            while (position < encoded.Length && encoded[position] == 255) position++;
            if (position >= encoded.Length) throw new InvalidOperationException("Truncated JPEG marker.");
            var marker = encoded[position++];
            if (marker == 0xDA)
            {
                if (width == 0 || tables.Count == 0) throw new InvalidOperationException("JPEG frame/tables missing.");
                return new Header(width, height, depth, components, sampling, tables.ToArray(),
                    Convert.ToHexStringLower(SHA256.HashData(tables.SelectMany(table => table).ToArray())));
            }
            if (encoded.Length - position < 2) throw new InvalidOperationException("Truncated JPEG segment.");
            var length = BinaryPrimitives.ReadUInt16BigEndian(encoded.AsSpan(position));
            if (length < 2 || length > encoded.Length - position) throw new InvalidOperationException("Invalid JPEG segment length.");
            var payload = encoded.AsSpan(position + 2, length - 2); position += length;
            if (marker is 0xC0 or 0xC2)
            {
                if (payload.Length < 6) throw new InvalidOperationException("Truncated JPEG frame.");
                depth = payload[0]; height = BinaryPrimitives.ReadUInt16BigEndian(payload[1..]); width = BinaryPrimitives.ReadUInt16BigEndian(payload[3..]); components = payload[5];
                if (payload.Length != 6 + components * 3) throw new InvalidOperationException("Invalid JPEG components.");
                var factors = new List<string>();
                for (var i = 0; i < components; i++) { var value = payload[7 + i * 3]; factors.Add($"{value >> 4}x{value & 15}"); }
                sampling = string.Join(",", factors);
            }
            else if (marker == 0xDB)
            {
                while (!payload.IsEmpty)
                {
                    if (payload.Length < 65 || payload[0] >> 4 != 0) throw new InvalidOperationException("Expected baseline eight-bit quantization.");
                    tables.Add(payload[..65].ToArray()); payload = payload[65..];
                }
            }
        }
        throw new InvalidOperationException("JPEG scan missing.");
    }
}
