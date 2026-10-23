namespace Lokad.Image.Tooling;

// Original deterministic controls and frozen libwebp1.6.0 exact=1 packet hashes.
// Existing input formulas/provenance: benchmarks/VP8L-COMPLETE-PACKET.md.
// The same bounded sources/settings qualify internal and actual public APIs.
internal static class OriginalVp8lControls
{
    internal static (byte[] Pixels, int Width, int Height) Input(string name)
    {
        if (name is "asymmetric" or "opaque")
        {
            var bytes = OriginalPixels.Generate(513, 259, 8); bytes[3] = bytes[513 * 4 - 1] = 0;
            if (name == "opaque") for (var i = 3; i < bytes.Length; i += 4) bytes[i] = 255;
            return (bytes, 513, 259);
        }
        if (name is "photograph" or "diagram")
        {
            for (var directory = new DirectoryInfo(AppContext.BaseDirectory); directory is not null; directory = directory.Parent)
            {
                var file = Path.Combine(directory.FullName, "benchmarks", "Fixtures", "Compression", name + ".png");
                if (!File.Exists(file)) continue;
                using var image = ImageCodec.Decode(File.ReadAllBytes(file)); return (OriginalPixels.Bytes(image), image.Width, image.Height);
            }
            throw new InvalidOperationException("Original compression fixture missing.");
        }
        if (name == "hidden")
        {
            var bytes = new byte[19 * 7 * 4];
            for (var y = 0; y < 7; y++) for (var x = 0; x < 19; x++)
            { var i = (y * 19 + x) * 4; bytes[i] = (byte)(13 * x); bytes[i + 1] = (byte)(31 * y); bytes[i + 2] = (byte)(x + y); bytes[i + 3] = (byte)(x % 3 == 0 ? 0 : 11 * x); }
            return (bytes, 19, 7);
        }
        if (name == "singleton") return ([255, 0, 1, 0], 1, 1);
        if (name is "axis-horizontal" or "axis-vertical")
        {
            var bytes = new byte[16383 * 4];
            for (var i = 0; i < 16383; i++)
            { bytes[4 * i] = (byte)i; bytes[4 * i + 1] = (byte)(i >> 8); bytes[4 * i + 2] = (byte)(13 * i + (i >> 2)); bytes[4 * i + 3] = 255; }
            return (bytes, name == "axis-horizontal" ? 16383 : 1, name == "axis-horizontal" ? 1 : 16383);
        }
        if (name == "two-color")
        {
            var bytes = new byte[4103 * 2 * 4];
            for (var i = 0; i < bytes.Length; i += 4) { bytes[i] = 48; bytes[i + 1] = 211; bytes[i + 2] = 255; }
            bytes[^4] = 77; return (bytes, 4103, 2);
        }
        if (name is "palette4" or "palette16" or "palette18")
        {
            var width = name == "palette4" ? 7 : name == "palette16" ? 17 : 23;
            var height = name == "palette18" ? 13 : 3; var colors = int.Parse(name.AsSpan(7));
            return (Palette(width, height, colors), width, height);
        }
        if (name == "direct")
        {
            var bytes = new byte[129 * 129 * 4]; uint state = 0x9e3779b9;
            for (var i = 0; i < bytes.Length; i += 4)
            { state ^= state << 13; state ^= state >> 17; state ^= state << 5; bytes[i] = (byte)state; bytes[i + 1] = (byte)(state >> 8); bytes[i + 2] = (byte)(state >> 16); bytes[i + 3] = 255; }
            return (bytes, 129, 129);
        }
        throw new ArgumentException("Unknown original control.", nameof(name));
    }

    internal static ImageBuffer Buffer(string name, int depth, IMemoryBudget? budget = null)
    {
        var input = Input(name);
        var image = ImageBuffer.Create(input.Width, input.Height, depth, new() { MemoryBudget = budget });
        try
        {
            for (var y = 0; y < input.Height; y++)
            for (var x = 0; x < input.Width * 4; x++)
            {
                var sample = input.Pixels[y * input.Width * 4 + x];
                if (depth == 8) image.GetRow8(y)[x] = sample;
                else image.GetRow16(y)[x] = (ushort)Math.Clamp(sample * 257 + ((x + y) % 3 - 1) * 64, 0, 65535);
            }
            return image;
        }
        catch { image.Dispose(); throw; }
    }

    private static byte[] Palette(int width, int height, int colors)
    {
        var bytes = new byte[width * height * 4]; uint state = 0x6d2b79f5;
        for (var i = 0; i < width * height; i++)
        {
            state ^= state << 13; state ^= state >> 17; state ^= state << 5;
            var color = i < colors ? i : (int)(state % (uint)colors); if (color == 0) continue;
            bytes[4 * i] = (byte)(color * 47 + 11); bytes[4 * i + 1] = (byte)(color * 89 + 7);
            bytes[4 * i + 2] = (byte)(color * 131 + 23); bytes[4 * i + 3] = color % 7 == 0 ? (byte)0 : (byte)255;
        }
        return bytes;
    }

    public static IEnumerable<object[]> Cases()
    {
        yield return ["asymmetric", 4, 183812, "08ddac115f1770d778e6dbc76373440224b32e6f93b616605e234c63206eebde"];
        yield return ["opaque", 4, 121896, "465b70013a8a598ec5e95e2e4d8fff3042317fa21dc31f2350fe9edb46ce465f"];
        yield return ["photograph", 4, 160032, "8e8c2a10a1be8c27233d7c70ab5c704d95924759408ecb8c02a3da50927aabb8"];
        yield return ["diagram", 4, 7234, "399c218d2fd333724a3b9f77d8c53bd958f01483b314c496935b022d7a03e0bf"];
        yield return ["asymmetric", 0, 316566, "a83e601e8d0fd35d256dd92d682db09d6868a7f401466d8f13cb95b1fd6d150b"];
        yield return ["asymmetric", 1, 176216, "f5a66ebfaff467b0f2409663753b3387b02b4f872e711c034869c328317b956d"];
        yield return ["asymmetric", 5, 155162, "1c9c7469eb5b9f3a898704bb5e6848fc3e6d8224e9f4d3fe9964d9c7eff6ded9"];
        yield return ["photograph", 5, 160600, "28932bb732389e34054bd2cd044fcfc7e44de98b89e65bc4dff6ef576fbbd575"];
        yield return ["singleton", 4, 34, "98dc19987970964d8c3a2bd9e57c9e4cd4e29dc1e99fe8658b087f53aabcb618"];
        yield return ["two-color", 0, 48, "ffaf55b38823d030ac8ee2988ddcf34da5a195d6a0f3043506262d68915c8c64"];
        yield return ["two-color", 1, 46, "8f241022ea6585480f36d10a8d4c2cfefcc3e311ae6120b14867e34260aeb9bc"];
        yield return ["two-color", 2, 46, "8f241022ea6585480f36d10a8d4c2cfefcc3e311ae6120b14867e34260aeb9bc"];
        yield return ["two-color", 3, 46, "8f241022ea6585480f36d10a8d4c2cfefcc3e311ae6120b14867e34260aeb9bc"];
        yield return ["two-color", 4, 46, "8f241022ea6585480f36d10a8d4c2cfefcc3e311ae6120b14867e34260aeb9bc"];
        yield return ["two-color", 5, 46, "8f241022ea6585480f36d10a8d4c2cfefcc3e311ae6120b14867e34260aeb9bc"];
        yield return ["two-color", 6, 46, "8f241022ea6585480f36d10a8d4c2cfefcc3e311ae6120b14867e34260aeb9bc"];
        yield return ["palette4", 4, 66, "5b7387a2bc7bbf253f6488dc6d10b26a395eecb5835819e718c8f043ed268ab7"];
        yield return ["palette16", 4, 130, "b24691e94f5e5369d84f8de9074c436ca470f6ca9a3a9a7389c1256972cb1d33"];
        yield return ["palette18", 4, 236, "a4bb56684758d4ff4fa58da15729a126098a74b228c54307204c95d7e4942736"];
        yield return ["palette18", 0, 268, "634ad1b52a2b14326ac2e3336e9e5cfb4848d2b94b24ca305b23df26211ed315"];
        yield return ["palette18", 5, 236, "a4bb56684758d4ff4fa58da15729a126098a74b228c54307204c95d7e4942736"];
        yield return ["hidden", 4, 82, "c432b85f60f9817358985ffad69ae307d33926f02de3fb80abe8645b5f526c58"];
        yield return ["direct", 4, 50010, "18b6120553762b78d4ca5df0fbf34b45d9ee3a3eaaca02462636c17e795e9ec5"];
        yield return ["axis-horizontal", 4, 74, "40964b3fd9868a571bb8c3a37994371b90a00b971a63c79df8521d4bec2a4d03"];
        yield return ["axis-vertical", 4, 74, "f96d04d0bf7da594ca6818123589c4d2611ce0badc4685c10cdfc75a23f916bb"];
    }
}
