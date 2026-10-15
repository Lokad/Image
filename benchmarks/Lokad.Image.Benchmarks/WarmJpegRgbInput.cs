using System.Buffers.Binary;

// Original marker-only fixture authoring, T.81 B.2 frame/scan identifiers and
// Adobe APP14 transform0. Component entropy and quantization remain unchanged.
internal static class WarmJpegRgbInput
{
    internal const string SourceSha256 = "c115cd7d17eb2cd3f34e31c439bdd48421e58ea2c0fce264c1cc8ad3eb1f8274";
    internal const string InputSha256 = "63ea38365b2fc9db4a759b86e199a8e5681d38369dff289559a0b4ae4ac91729";
    internal const string ManagedSha256 = "c66733fae2979ac6388499d415a7bc448fcc2765a5bdb578421de4c04e39c4c9";
    internal const string NativeSha256 = "b13fa169ad04f9181c49cd5613f4d7483b1cd3650d8b63528a2474a7ce5bd369";

    internal const string Source422Sha256 = "f0b28dd0edce8c7286099c9b4572d7fd8baf6290bcd7b4ff9134f1e699354828";
    internal const string Input422Sha256 = "0894aa4975ec61d5a3723bb04ee4703a23313532097e5bc080823b623134b9c9";
    internal const string Managed422Sha256 = "81d19ce7a8331b310ad4e6c1214d1a2b93c7ad8a09afa730ceda2e998355c0de";
    internal const string Native422Sha256 = "5cd92d09be1dc007dc2a696153cf50a3340e93bf015affe7146c3e28af195517";

    internal const string Source420Sha256 = "ccca91bcdaddc90c7aecf62d07cdc629c09c27e607c955cfcc97a7bf91caaf08";
    internal const string Input420Sha256 = "26e897ecf53f5eb6ea45e6bb881c2168e989dddd5a62c7449268e0c1feeb34e5";
    internal const string Managed420Sha256 = "08cabc07ee3df6ec835cf11a99d50c535449663f6a7f0e53bf4215ebac74b361";
    internal const string Native420Sha256 = "e3a693f838f7e27c2764ac61a18db1dc82c4e881e4c4d24479e2f7dcbae325ca";
    internal const string Oracle420Filter = "extractplanes=y+u+v[r][g][b];"
        + "[g]scale=iw*2:ih*2:flags=bilinear+accurate_rnd,crop=513:259,setsar=1[ge];"
        + "[b]scale=iw*2:ih*2:flags=bilinear+accurate_rnd,crop=513:259,setsar=1[be];"
        + "[r]setsar=1[re];[ge][be][re]mergeplanes=0x001020:format=gbrp,format=rgba";

    // Independent untimed CLI reference: immutable even-position samples and
    // rational SPEC centered interpolation, separate from the in-place worker.
    internal static byte[] Center422(ReadOnlySpan<byte> rgba)
    {
        const int width = 513, height = 259;
        if (rgba.Length != width * height * 4) throw new ArgumentException("Expected one RGBA8 raster.");
        var output = rgba.ToArray();
        for (var y = 0; y < height; y++)
        for (var x = 0; x < width; x++)
        for (var c = 1; c <= 2; c++)
        {
            var k = x / 2; var odd = (x & 1) != 0;
            var first = Math.Clamp(odd ? k : k - 1, 0, width / 2);
            var second = Math.Clamp(odd ? k + 1 : k, 0, width / 2);
            output[(y * width + x) * 4 + c] = (byte)(((odd ? 3 : 1) * rgba[(y * width + first * 2) * 4 + c]
                + (odd ? 1 : 3) * rgba[(y * width + second * 2) * 4 + c] + 2) / 4);
        }
        return output;
    }

    internal static byte[] Build(byte[] source, bool rgb422 = false, bool rgb420 = false)
    {
        if (rgb422 && rgb420) throw new ArgumentException("Choose one RGB sampling interpretation.");
        if (JpegBenchmarkCase.Hash(source) != (rgb420 ? Source420Sha256 : rgb422 ? Source422Sha256 : SourceSha256))
            throw new InvalidOperationException("Original native JPEG RGB source packet differs.");
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
        if (!scan || JpegBenchmarkCase.Hash(result) != (rgb420 ? Input420Sha256 : rgb422 ? Input422Sha256 : InputSha256))
            throw new InvalidOperationException("Marker-only RGB interpretation differs.");
        return result;
    }
}
