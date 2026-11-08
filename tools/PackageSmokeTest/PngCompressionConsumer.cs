namespace Lokad.Image.Tooling;

internal static class PngCompressionConsumer
{
    internal static void Verify()
    {
        using var source = OriginalPngGradient.Create();
        using var flipped = ImageTransforms.FlipHorizontal(source); using var output = new MemoryStream();
        ImageCodec.EncodePng(flipped, output, new() { Limits = ImageLimits.Default with { MaxOutputBytes = 2 * 1024 * 1024 } });
        if (output.Length >= 2 * 1024 * 1024 || !output.CanWrite)
            throw new InvalidOperationException("Packaged predictable PNG compression or stream ownership differs.");
        IndependentRgbaPng.Verify(output.ToArray(), flipped);
        Console.WriteLine("Packaged predictable RGBA16 PNG passed independent lossless decoding below 2 MiB.");
        VerifyFastest(8, 236828, "a024058695b0f61d4a57440b08975132958f8fdd151f1c0c58d56dce1c720b14").GetAwaiter().GetResult();
        VerifyFastest(16, 968216, "adbcc5115ee9acce294ab70f65b6f24862b7451d90f870a082269a8008c08e25").GetAwaiter().GetResult();
        Console.WriteLine("Packaged Fastest PNG preserves exact byte/word packets and admitted sync/async ownership.");
    }

    private static async Task VerifyFastest(int depth, int packetBytes, string packetHash)
    {
        using var source = OriginalPixels.Buffer(513, 259, depth);
        if (depth == 8) source.GetRow8(0)[3] = source.GetRow8(0)[513 * 4 - 1] = 0;
        else source.GetRow16(0)[3] = source.GetRow16(0)[513 * 4 - 1] = 0;
        using var root = new TestMemoryBudget(4 * 1024 * 1024);
        var options = new PngEncodeOptions
        {
            ColorType = PngColorType.Rgba,
            Compression = System.IO.Compression.CompressionLevel.Fastest,
            Limits = new() { MemoryBudget = root, MaxWorkingBytes = 4 * 1024 * 1024, MaxOutputBytes = 2 * 1024 * 1024 }
        };
        using var live = new CancellationTokenSource();
        using var first = new MemoryStream(); using var second = new MemoryStream();
        ImageCodec.EncodePng(source, first, options, live.Token);
        if (root.Bytes != 0 || root.IsDisposed || !first.CanWrite)
            throw new InvalidOperationException("Packaged Fastest PNG scratch or borrowed ownership differs.");
        var packet = first.ToArray();
        await ImageCodec.EncodePngAsync(source, second, options, live.Token);
        if (root.Bytes != 0 || root.IsDisposed || !second.CanWrite || !packet.AsSpan().SequenceEqual(second.ToArray())
            || packet.Length != packetBytes
            || Convert.ToHexStringLower(System.Security.Cryptography.SHA256.HashData(packet)) != packetHash)
            throw new InvalidOperationException("Packaged Fastest PNG changed its frozen packet or asynchronous lifetime.");
        IndependentRgbaPng.Verify(packet, source);
    }
}
