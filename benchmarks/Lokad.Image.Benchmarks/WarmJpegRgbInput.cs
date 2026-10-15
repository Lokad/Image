using System.Buffers.Binary;

// Original marker-only fixture authoring, T.81 B.2 frame/scan identifiers and
// Adobe APP14 transform0. Component entropy and quantization remain unchanged.
internal static class WarmJpegRgbInput
{
    internal const string SourceSha256 = "c115cd7d17eb2cd3f34e31c439bdd48421e58ea2c0fce264c1cc8ad3eb1f8274";
    internal const string InputSha256 = "63ea38365b2fc9db4a759b86e199a8e5681d38369dff289559a0b4ae4ac91729";
    internal const string ManagedSha256 = "c66733fae2979ac6388499d415a7bc448fcc2765a5bdb578421de4c04e39c4c9";
    internal const string NativeSha256 = "b13fa169ad04f9181c49cd5613f4d7483b1cd3650d8b63528a2474a7ce5bd369";

    internal static byte[] Build(byte[] source)
    {
        if (JpegBenchmarkCase.Hash(source) != SourceSha256)
            throw new InvalidOperationException("Original native JPEG444 packet differs.");
        using var output = new MemoryStream();
        output.Write(new byte[] { 255, 216, 255, 238, 0, 14, 65, 100, 111, 98, 101, 0, 100, 0, 0, 0, 0, 0 });
        var position = 2; var frame = false; var scan = false;
        while (position < source.Length)
        {
            var start = position;
            if (source.Length - position < 4 || source[position] != 255)
                throw new InvalidOperationException("Expected a bounded JPEG segment.");
            var marker = source[position + 1];
            var length = BinaryPrimitives.ReadUInt16BigEndian(source.AsSpan(position + 2));
            if (length < 2 || length > source.Length - position - 2)
                throw new InvalidOperationException("Invalid JPEG segment length.");
            var payload = source.AsSpan(position + 4, length - 2).ToArray(); position += length + 2;
            if (marker == 0xC0)
            {
                if (frame || payload.Length != 15 || payload[0] != 8 || payload[5] != 3)
                    throw new InvalidOperationException("Expected one three-component baseline frame.");
                for (var i = 0; i < 3; i++)
                {
                    if (payload[6 + 3 * i] != i + 1) throw new InvalidOperationException("Frame identifiers differ.");
                    payload[6 + 3 * i] = "RGB"u8[i];
                }
                frame = true;
            }
            else if (marker == 0xDA)
            {
                if (!frame || scan || payload.Length != 10 || payload[0] != 3)
                    throw new InvalidOperationException("Expected one complete three-component scan.");
                for (var i = 0; i < 3; i++)
                {
                    if (payload[1 + 2 * i] != i + 1) throw new InvalidOperationException("Scan identifiers differ.");
                    payload[1 + 2 * i] = "RGB"u8[i];
                }
                scan = true;
            }
            else if (marker == 0xE0 && payload.AsSpan().StartsWith("JFIF\0"u8)
                || marker == 0xEE && payload.AsSpan().StartsWith("Adobe"u8)) continue;
            output.Write(source.AsSpan(start, 4)); output.Write(payload);
            if (scan) { output.Write(source.AsSpan(position)); break; }
        }
        var result = output.ToArray();
        if (!scan || JpegBenchmarkCase.Hash(result) != InputSha256)
            throw new InvalidOperationException("Marker-only RGB interpretation differs.");
        return result;
    }
}
