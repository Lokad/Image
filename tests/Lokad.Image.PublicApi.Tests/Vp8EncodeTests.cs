using Lokad.Image.Tooling;
using System.Security.Cryptography;
using System.Text.Json;

namespace Lokad.Image.PublicApi.Tests;

public sealed class Vp8EncodeTests
{
    [Fact]
    public void ManagedCorpusReplaysDeclaredPixelsAndUnchangedVp8Payloads()
    {
        var root = new DirectoryInfo(AppContext.BaseDirectory);
        while (root is not null && !File.Exists(Path.Combine(root.FullName, "tests", "Fixtures", "vp8-manifest.json"))) root = root.Parent;
        Assert.NotNull(root);
        using var manifest = JsonDocument.Parse(File.ReadAllBytes(Path.Combine(root.FullName, "tests", "Fixtures", "vp8-manifest.json")));
        var count = 0;
        foreach (var fixture in manifest.RootElement.GetProperty("fixtures").EnumerateArray())
        {
            if (fixture.GetProperty("authoring").GetString() != "Lokad.Image.EncodeWebp") continue;
            count++;
            var options = fixture.GetProperty("managedOptions");
            using var image = OriginalPixels.Buffer(fixture.GetProperty("width").GetInt32(), fixture.GetProperty("height").GetInt32(),
                options.GetProperty("bitDepth").GetInt32(), opaque: !fixture.GetProperty("alpha").GetBoolean());
            Assert.Equal(fixture.GetProperty("inputHash").GetString(), Hash(OriginalPixels.Bytes(image)));
            using var output = new MemoryStream();
            ImageCodec.EncodeWebp(image, output, new WebpEncodeOptions
            {
                Lossless = options.GetProperty("lossless").GetBoolean(), Quality = options.GetProperty("quality").GetInt32(),
                Effort = options.GetProperty("effort").GetInt32()
            });
            var encoded = output.ToArray();
            if (Hash(encoded) != fixture.GetProperty("encodedHash").GetString())
            {
                // Keep the frozen decoder corpus and its original packet hashes.
                // Exact alpha compression may change only ALPH, never the VP8
                // payload or reconstructed pixels of these declared controls.
                var frozen = File.ReadAllBytes(Path.Combine(root.FullName, "tests", "Fixtures",
                    fixture.GetProperty("name").GetString() + ".webp"));
                Assert.Equal(fixture.GetProperty("encodedHash").GetString(), Hash(frozen));
                Assert.True(fixture.GetProperty("alpha").GetBoolean());
                Assert.True(options.GetProperty("effort").GetInt32() > 0);
                Assert.Equal(0, Chunk(frozen, "ALPH"u8)[0]); Assert.Equal(5, Chunk(encoded, "ALPH"u8)[0]);
                Assert.Equal(Chunk(frozen, "VP8 "u8).ToArray(), Chunk(encoded, "VP8 "u8).ToArray());
                using var expected = ImageCodec.Decode(frozen); using var actual = ImageCodec.Decode(encoded);
                Assert.Equal(OriginalPixels.Bytes(expected), OriginalPixels.Bytes(actual));
                Assert.True(encoded.Length < frozen.Length);
            }
        }
        Assert.Equal(42, count);
    }

    private static string Hash(byte[] data) => Convert.ToHexStringLower(SHA256.HashData(data));

    private static ReadOnlySpan<byte> Chunk(byte[] encoded, ReadOnlySpan<byte> name)
    {
        for (var offset = 12; offset < encoded.Length;)
        {
            var length = System.Buffers.Binary.BinaryPrimitives.ReadInt32LittleEndian(encoded.AsSpan(offset + 4));
            if (encoded.AsSpan(offset, 4).SequenceEqual(name)) return encoded.AsSpan(offset + 8, length);
            offset += 8 + length + (length & 1);
        }
        throw new InvalidOperationException("Declared WebP chunk is missing.");
    }

    [Theory]
    [InlineData(1, 0, false)] [InlineData(50, 1, true)] [InlineData(85, 4, false)] [InlineData(100, 6, true)]
    public void LossyOutputKeepsGeometryAlphaAndStreamOwnership(int quality, int effort, bool opaque)
    {
        var pixels = OriginalPixels.Generate(31, 23, 8, opaque: opaque);
        using var image = ImageBuffer.Create(31, 23, 8);
        for (var y = 0; y < 23; y++) pixels.AsSpan(y * 31 * 4, 31 * 4).CopyTo(image.GetRow8(y));
        using var output = new NonseekableOutput();
        ImageCodec.EncodeWebp(image, output, new WebpEncodeOptions { Lossless = false, Quality = quality, Effort = effort });
        Assert.True(output.CanWrite); var encoded = output.ToArray(); var info = ImageCodec.Identify(encoded);
        Assert.Equal((31, 23, !opaque), (info.Width, info.Height, info.HasAlpha));
        Assert.True(encoded.AsSpan().IndexOf("VP8 "u8) >= 0);
        using var decoded = ImageCodec.Decode(encoded); Assert.Equal((31, 23, 8), (decoded.Width, decoded.Height, decoded.BitDepth));
        for (var y = 0; y < 23; y++)
            for (var x = 0; x < 31; x++) Assert.Equal(pixels[(y * 31 + x) * 4 + 3], decoded.GetRow8(y)[x * 4 + 3]);
    }

    [Fact]
    public void SixteenBitNormalizationProducesTheSameBitstreamAsExplicitEightBitInput()
    {
        using var wide = ImageBuffer.Create(7, 3, 16); using var narrow = ImageBuffer.Create(7, 3, 8);
        var values = OriginalPixels.Generate(7, 3, 16);
        for (var y = 0; y < 3; y++)
        {
            values.AsSpan(y * wide.Stride, wide.Stride).CopyTo(System.Runtime.InteropServices.MemoryMarshal.AsBytes(wide.GetRow16(y)));
            for (var i = 0; i < 7 * 4; i++) narrow.GetRow8(y)[i] = (byte)((wide.GetRow16(y)[i] + 128) / 257);
        }
        using var first = new MemoryStream(); using var second = new MemoryStream();
        ImageCodec.EncodeWebp(wide, first, new WebpEncodeOptions { Lossless = false });
        ImageCodec.EncodeWebp(narrow, second, new WebpEncodeOptions { Lossless = false });
        Assert.Equal(second.ToArray(), first.ToArray());
        using var decoded = ImageCodec.Decode(first.ToArray());
        for (var y = 0; y < 3; y++)
            for (var x = 0; x < 7; x++) Assert.Equal(narrow.GetRow8(y)[x * 4 + 3], decoded.GetRow8(y)[x * 4 + 3]);
    }

    [Fact]
    public void CompleteContainerLengthIsCheckedBeforeWritingAndTheExactBoundWorks()
    {
        using var image = OriginalPixels.Buffer(31, 23, 8); using var reference = new MemoryStream();
        ImageCodec.EncodeWebp(image, reference, new WebpEncodeOptions { Lossless = false });
        using var shortOutput = new MemoryStream();
        Assert.Throws<ImageLimitException>(() => ImageCodec.EncodeWebp(image, shortOutput,
            new WebpEncodeOptions { Lossless = false, Limits = new ImageLimits { MaxOutputBytes = (int)reference.Length - 1 } }));
        Assert.Equal(0, shortOutput.Length);
        using var exact = new MemoryStream();
        ImageCodec.EncodeWebp(image, exact,
            new WebpEncodeOptions { Lossless = false, Limits = new ImageLimits { MaxOutputBytes = (int)reference.Length } });
        Assert.Equal(reference.ToArray(), exact.ToArray());
    }

    [Fact]
    public void InvalidOptionsAndResourceLimitsFailBeforeOutput()
    {
        using var image = ImageBuffer.Create(17, 19, 8); using var output = new MemoryStream();
        foreach (var options in new[]
        {
            new WebpEncodeOptions { Lossless = false, Quality = 0 }, new WebpEncodeOptions { Lossless = false, Quality = 101 },
            new WebpEncodeOptions { Lossless = false, Effort = -1 }, new WebpEncodeOptions { Lossless = false, Effort = 7 }
        }) Assert.Throws<ArgumentOutOfRangeException>(() => ImageCodec.EncodeWebp(image, output, options));
        foreach (var limits in new[]
        {
            new ImageLimits { MaxOutputBytes = 1 }, new ImageLimits { MaxWorkingBytes = 64 },
            new ImageLimits { MaxWork = 1 }, new ImageLimits { MaxPixels = 1 }
        }) Assert.Throws<ImageLimitException>(() => ImageCodec.EncodeWebp(image, output, new WebpEncodeOptions { Lossless = false, Limits = limits }));
        Assert.Throws<OperationCanceledException>(() => ImageCodec.EncodeWebp(image, output,
            new WebpEncodeOptions { Lossless = false }, new CancellationToken(true)));
        using var tooWide = ImageBuffer.Create(16384, 1, 8);
        Assert.Throws<ImageLimitException>(() => ImageCodec.EncodeWebp(tooWide, output, new WebpEncodeOptions { Lossless = false }));
        Assert.Equal(0, output.Length);
    }

    private sealed class NonseekableOutput : Stream
    {
        private readonly MemoryStream inner = new();
        public byte[] ToArray() => inner.ToArray();
        public override bool CanRead => false;
        public override bool CanSeek => false;
        public override bool CanWrite => inner.CanWrite;
        public override long Length => throw new NotSupportedException();
        public override long Position { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }
        public override void Flush() => inner.Flush();
        public override void Write(byte[] buffer, int offset, int count) => inner.Write(buffer, offset, count);
        public override void Write(ReadOnlySpan<byte> buffer) => inner.Write(buffer);
        public override void WriteByte(byte value) => inner.WriteByte(value);
        public override int Read(byte[] buffer, int offset, int count) => throw new NotSupportedException();
        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
        public override void SetLength(long value) => throw new NotSupportedException();
        protected override void Dispose(bool disposing) { if (disposing) inner.Dispose(); base.Dispose(disposing); }
    }
}
