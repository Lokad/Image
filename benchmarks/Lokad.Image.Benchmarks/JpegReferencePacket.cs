using System.Buffers.Binary;

// Benchmark-only comparison of frozen native packets. The sole permitted image
// adjustment is the identified encoder-attribution COM, never coded image data.
internal static class JpegReferencePacket
{
    internal static byte[] FromNative(ReadOnlySpan<byte> packet)
    {
        if (packet.Length < 4 || packet[0] != 255 || packet[1] != 216)
            throw new InvalidOperationException("Frozen JPEG signature differs.");
        var begin = -1; var end = -1;
        for (var at = 2; at < packet.Length;)
        {
            if (packet[at] != 255 || at + 4 > packet.Length) throw new InvalidOperationException("Frozen JPEG marker differs.");
            var marker = packet[at + 1]; if (marker is 0xDA or 0xD9) break;
            var length = BinaryPrimitives.ReadUInt16BigEndian(packet[(at + 2)..]);
            if (length < 2 || at + 2 + length > packet.Length) throw new InvalidOperationException("Frozen JPEG segment differs.");
            if (marker == 0xFE)
            {
                if (begin != -1 || !packet.Slice(at + 4, length - 2).SequenceEqual("Lavc62.28.100\0"u8))
                    throw new InvalidOperationException("Unapproved JPEG attribution metadata.");
                begin = at; end = at + 2 + length;
            }
            at += 2 + length;
        }
        if (begin == -1) return packet.ToArray(); // Approved grayscale backend has no COM.
        ReadOnlySpan<byte> attribution = "Lokad.Image\0"u8;
        var output = new byte[packet.Length - (end - begin) + attribution.Length + 4];
        packet[..begin].CopyTo(output); output[begin] = 255; output[begin + 1] = 254;
        BinaryPrimitives.WriteUInt16BigEndian(output.AsSpan(begin + 2), (ushort)(attribution.Length + 2));
        attribution.CopyTo(output.AsSpan(begin + 4)); packet[end..].CopyTo(output.AsSpan(begin + 4 + attribution.Length));
        return output;
    }
}
