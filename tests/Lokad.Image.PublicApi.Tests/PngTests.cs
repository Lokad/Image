using System.Buffers.Binary;
using System.Security.Cryptography;
using System.Text.Json;

namespace Lokad.Image.PublicApi.Tests;

public sealed class PngTests
{
    private static string Fixtures
    {
        get
        {
            var directory = new DirectoryInfo(AppContext.BaseDirectory);
            while (directory is not null)
            {
                var path = Path.Combine(directory.FullName, "tests", "Fixtures");
                if (File.Exists(Path.Combine(path, "manifest.json"))) return path;
                directory = directory.Parent;
            }
            throw new InvalidOperationException("Frozen fixture manifest not found.");
        }
    }

    [Fact]
    public void FrozenFfmpegPngFixturesMatchExactly()
    {
        using var manifest = JsonDocument.Parse(File.ReadAllBytes(Path.Combine(Fixtures, "manifest.json")));
        Assert.Equal(16, manifest.RootElement.GetProperty("fixtures").GetArrayLength());
        Assert.Equal(64, manifest.RootElement.GetProperty("ffmpegSha256").GetString()!.Length);
        foreach (var fixture in manifest.RootElement.GetProperty("fixtures").EnumerateArray())
        {
            var name = fixture.GetProperty("name").GetString()!;
            var encoded = File.ReadAllBytes(Path.Combine(Fixtures, name + ".png"));
            var expected = File.ReadAllBytes(Path.Combine(Fixtures, name + ".rgba"));
            Assert.Equal(name.EndsWith("adam7", StringComparison.Ordinal) ? 1 : 0, encoded[28]);
            Assert.Equal(fixture.GetProperty("encodedHash").GetString(), Convert.ToHexStringLower(SHA256.HashData(encoded)));
            Assert.Equal(fixture.GetProperty("expectedHash").GetString(), Convert.ToHexStringLower(SHA256.HashData(expected)));
            using var image = ImageCodec.Decode(encoded);
            var depth = fixture.GetProperty("depth").GetInt32();
            Assert.Equal(depth, image.BitDepth);
            Assert.Equal(fixture.GetProperty("width").GetInt32(), image.Width);
            Assert.Equal(fixture.GetProperty("height").GetInt32(), image.Height);
            Assert.Equal(expected, Bytes(image));
            using var encodedAgain = new MemoryStream();
            ImageCodec.EncodePng(image, encodedAgain);
            using var decodedAgain = ImageCodec.Decode(encodedAgain.ToArray());
            Assert.Equal(expected, Bytes(decodedAgain));
        }
    }

    [Theory]
    [InlineData(8)]
    [InlineData(16)]
    public void AllEncoderColorTypesRoundTripAndLeaveStreamOpen(int depth)
    {
        foreach (var color in Enum.GetValues<PngColorType>())
        {
            using var image = ImageBuffer.Create(2, 1, depth);
            for (var x = 0; x < 2; x++)
            {
                if (depth == 8) new byte[] { 17, 17, 17, color is PngColorType.Rgb or PngColorType.Grayscale ? (byte)255 : (byte)91 }.CopyTo(image.GetRow8(0)[(x * 4)..]);
                else new ushort[] { 12345, 12345, 12345, color is PngColorType.Rgb or PngColorType.Grayscale ? (ushort)65535 : (ushort)43210 }.CopyTo(image.GetRow16(0)[(x * 4)..]);
            }
            using var encoded = new MemoryStream();
            ImageCodec.EncodePng(image, encoded, new PngEncodeOptions { ColorType = color });
            Assert.True(encoded.CanWrite);
            using var decoded = ImageCodec.Decode(encoded.ToArray());
            Assert.Equal(Bytes(image), Bytes(decoded));
        }
    }

    [Fact]
    public void TruncationCrcMutationAndLimitsFailExplicitly()
    {
        var data = File.ReadAllBytes(Path.Combine(Fixtures, "png-rgba8-plain.png"));
        for (var length = 8; length < data.Length; length++)
            Assert.Throws<ImageDataException>(() => ImageCodec.Decode(data.AsSpan(0, length)));
        var damaged = data.ToArray(); damaged[40] ^= 1;
        Assert.Throws<ImageDataException>(() => ImageCodec.Decode(damaged));
        Assert.Throws<ImageLimitException>(() => ImageCodec.Decode(data, new DecodeOptions { Limits = new ImageLimits { MaxPixels = 1 } }));
        Assert.Throws<ImageLimitException>(() => ImageCodec.Identify(data, new ImageLimits { MaxInputBytes = 1 }));
        using var image = ImageCodec.Decode(data);
        using var output = new MemoryStream();
        Assert.Throws<ImageLimitException>(() => ImageCodec.EncodePng(image, output, new PngEncodeOptions { Limits = new ImageLimits { MaxOutputBytes = 1 } }));
        Assert.True(output.CanWrite);
    }

    [Fact]
    public void NonseekableInputIsBoundedAndRemainsOpen()
    {
        var data = File.ReadAllBytes(Path.Combine(Fixtures, "png-rgba8-plain.png"));
        using var stream = new Nonseekable(data);
        using var image = ImageCodec.Decode(stream);
        Assert.True(stream.CanRead);
        using var limited = new Nonseekable(data);
        Assert.Throws<ImageLimitException>(() => ImageCodec.Decode(limited, new DecodeOptions { Limits = new ImageLimits { MaxInputBytes = 1 } }));
    }

    private sealed class Nonseekable(byte[] data) : MemoryStream(data)
    {
        public override bool CanSeek => false;
        public override long Length => throw new NotSupportedException();
        public override long Position { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }
    }

    private static byte[] Bytes(ImageBuffer image)
    {
        var bytes = new byte[image.Width * image.Height * 4 * (image.BitDepth / 8)];
        for (var y = 0; y < image.Height; y++)
        {
            if (image.BitDepth == 8) image.GetRow8(y).CopyTo(bytes.AsSpan(y * image.Width * 4));
            else for (var i = 0; i < image.Width * 4; i++)
                BinaryPrimitives.WriteUInt16LittleEndian(bytes.AsSpan((y * image.Width * 4 + i) * 2), image.GetRow16(y)[i]);
        }
        return bytes;
    }
}
