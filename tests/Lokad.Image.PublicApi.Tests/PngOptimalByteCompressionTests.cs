using System.Buffers.Binary;
using System.Security.Cryptography;
using Lokad.Image.Tooling;

namespace Lokad.Image.PublicApi.Tests;

public sealed class PngOptimalByteCompressionTests
{
    [Theory]
    [InlineData(PngColorType.Grayscale, 8, 72586, "c5b4a248c1ca0f76115d06e7795698f50ac7e623c366fcee4a659cb08da2872a")]
    [InlineData(PngColorType.Rgb, 8, 168729, "e1b0f39154bb07b69c6fab59512dbd55f1fb855c0a9805a44892d510efb73228")]
    [InlineData(PngColorType.GrayscaleAlpha, 8, 120366, "eefb49b94adbd654b38b4e33edc8aac89fdfd9273a8124a1fbf7884ee8789fb5")]
    [InlineData(PngColorType.Rgba, 8, 210973, "ad9ed127da583d68bdb749bb983b73966e8e814527216449c4557fdd89668920")]
    [InlineData(PngColorType.Grayscale, 16, 265433, "97b2e6d66070a7f42d3f487e837bd5c3ef2a1beb0362d5ec02110794007c3f70")]
    [InlineData(PngColorType.Rgb, 16, 739472, "4b8a2c3c99342ab51f2919c35f5dfb50506cb6c5812e84ede61c53f0db7388bf")]
    [InlineData(PngColorType.GrayscaleAlpha, 16, 506887, "addd3f236e49cef7fc937ab70022420681cf4d9f61d895b680605934429aca76")]
    [InlineData(PngColorType.Rgba, 16, 963202, "4f8f4d9950068c51b006ea87e2200523a5064e00842373eeb9c3a7f38bc12a0b")]
    public async Task DefaultCompressionMatchesFrozenNativePacketsAndPreservesOwners(PngColorType color, int depth, int referenceBytes, string packetHash)
    {
        // Existing asymmetric-v1 raster and documented PNG projections. The
        // reference sizes come from FFmpeg level6/None on these same scanlines.
        using var source = OriginalPixels.Buffer(513, 259, depth,
            grayscale: color is PngColorType.Grayscale or PngColorType.GrayscaleAlpha,
            opaque: color is PngColorType.Grayscale or PngColorType.Rgb);
        if (color is PngColorType.GrayscaleAlpha or PngColorType.Rgba)
        {
            if (depth == 8) source.GetRow8(0)[3] = source.GetRow8(0)[513 * 4 - 1] = 0;
            else source.GetRow16(0)[3] = source.GetRow16(0)[513 * 4 - 1] = 0;
        }
        var expected = OriginalPixels.Bytes(source);
        using var first = new MemoryStream(); using var second = new MemoryStream();
        var options = new PngEncodeOptions { ColorType = color };
        ImageCodec.EncodePng(source, first, options); ImageCodec.EncodePng(source, second, options);
        var png = first.ToArray(); Assert.Equal(png, second.ToArray());
        var payload = 0;
        for (var p = 8; p < png.Length;)
        {
            var n = checked((int)BinaryPrimitives.ReadUInt32BigEndian(png.AsSpan(p)));
            if (png.AsSpan(p + 4, 4).SequenceEqual("IDAT"u8)) payload += n;
            p = checked(p + n + 12);
        }
        Assert.True(payload > 0); Assert.Equal(referenceBytes, png.Length);
        Assert.Equal(packetHash, Convert.ToHexStringLower(SHA256.HashData(png)));
        using var asynchronous = new MemoryStream(); await ImageCodec.EncodePngAsync(source, asynchronous, options);
        Assert.Equal(png, asynchronous.ToArray());
        using var decoded = ImageCodec.Decode(png);
        Assert.Equal(expected, OriginalPixels.Bytes(decoded));
        Assert.Equal(expected, OriginalPixels.Bytes(source));
        first.GetBuffer()[first.Length / 2] ^= 255;
        Assert.Equal(png, second.ToArray());
    }

    [Fact]
    public async Task FrozenRgbaPacketMatchesForwardAsyncOnlyAndBudgetDelivery()
    {
        using var root = new TestMemoryBudget(4 * 1024 * 1024);
        var limits = ImageLimits.Default with { MemoryBudget = root };
        using var image = ImageBuffer.Create(513, 259, 8, limits);
        var pixels = OriginalPixels.Generate(513, 259, 8);
        pixels[3] = pixels[513 * 4 - 1] = 0;
        for (var y = 0; y < 259; y++) pixels.AsSpan(y * 513 * 4, 513 * 4).CopyTo(image.GetRow8(y));
        var options = new PngEncodeOptions { Limits = limits };
        var budget = new ImageBudget(limits.MaxWorkingBytes, limits.MaxWork * 2);
        for (var boundary = 0; boundary < 4; boundary++)
        {
            using var output = new ForwardOutput(asyncOnly: boundary >= 2);
            if (boundary == 0) ImageCodec.EncodePng(image, output, options);
            else if (boundary == 1) budget.EncodePng(image, output, options);
            else if (boundary == 2) await ImageCodec.EncodePngAsync(image, output, options);
            else await budget.EncodePngAsync(image, output, options);
            Assert.Equal("ad9ed127da583d68bdb749bb983b73966e8e814527216449c4557fdd89668920",
                Convert.ToHexStringLower(SHA256.HashData(output.Bytes)));
            Assert.Equal(pixels.Length, root.Bytes); Assert.False(output.WasDisposed);
            Assert.Equal(budget.MaxWorkingBytes, budget.RemainingWorkingBytes);
        }
        Assert.Equal(pixels, OriginalPixels.Bytes(image)); Assert.False(root.IsDisposed);
    }

    private sealed class ForwardOutput(bool asyncOnly) : Stream
    {
        private readonly MemoryStream storage = new();
        internal byte[] Bytes => storage.ToArray();
        internal bool WasDisposed { get; private set; }
        public override void Write(ReadOnlySpan<byte> bytes)
        { if (asyncOnly) throw new InvalidOperationException("Async-only output."); storage.Write(bytes); }
        public override void Write(byte[] bytes, int offset, int count) => Write(bytes.AsSpan(offset, count));
        public override ValueTask WriteAsync(ReadOnlyMemory<byte> bytes, CancellationToken cancellationToken = default)
        { cancellationToken.ThrowIfCancellationRequested(); storage.Write(bytes.Span); return ValueTask.CompletedTask; }
        protected override void Dispose(bool disposing) { WasDisposed = true; if (disposing) storage.Dispose(); base.Dispose(disposing); }
        public override bool CanRead => false;
        public override bool CanSeek => false;
        public override bool CanWrite => !WasDisposed;
        public override long Length => throw new NotSupportedException();
        public override long Position { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }
        public override void Flush() { }
        public override int Read(byte[] buffer, int offset, int count) => throw new NotSupportedException();
        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
        public override void SetLength(long value) => throw new NotSupportedException();
    }
}
