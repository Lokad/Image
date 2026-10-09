using System.Buffers.Binary;
using System.Security.Cryptography;
using System.Text.Json;
using Lokad.Image.Tooling;

namespace Lokad.Image.PublicApi.Tests;

public sealed class Vp8Tests
{
    [Theory]
    [InlineData(0)] [InlineData(1)] [InlineData(2)] [InlineData(3)]
    public void RawAlphaFiltersPreserveIndependentSourceSamplesAndLossyRgb(int filter)
    {
        var opaque = File.ReadAllBytes(Path.Combine(FindFixtures(),"vp8-ffmpeg-31x23-effort6-quality75-opaque.webp"));
        using var expected = ImageCodec.Decode(opaque);
        using var actual = ImageCodec.Decode(AuthoredAlpha.Wrap(opaque,31,23,filter));
        var alpha = AuthoredAlpha.Samples(31,23);
        for (var y=0;y<23;y++)
        for (var x=0;x<31;x++)
        {
            Assert.Equal(expected.GetRow8(y).Slice(x*4,3).ToArray(),actual.GetRow8(y).Slice(x*4,3).ToArray());
            Assert.Equal(alpha[y*31+x],actual.GetRow8(y)[x*4+3]);
        }
    }

    [Theory]
    [InlineData(false)] [InlineData(true)]
    public void LossyAlphaAndExifOrderingPreserveExactOrientationPermutation(bool metadataFirst)
    {
        var opaque = File.ReadAllBytes(Path.Combine(FindFixtures(),"vp8-ffmpeg-31x23-effort6-quality75-opaque.webp"));
        var encoded = AuthoredAlpha.Wrap(opaque,31,23,3,6,metadataFirst);
        using var raw = ImageCodec.Decode(encoded); using var oriented = ImageCodec.Decode(encoded,new DecodeOptions { ApplyOrientation=true });
        Assert.Equal((23,31),(oriented.Width,oriented.Height)); Assert.Equal(ImageOrientation.Normal,oriented.Orientation);
        for (var y=0;y<23;y++)
        for (var x=0;x<31;x++) Assert.Equal(raw.GetRow8(y).Slice(x*4,4).ToArray(),oriented.GetRow8(x).Slice((22-y)*4,4).ToArray());
    }

    [Theory]
    [InlineData(0,0)] [InlineData(0,3)] [InlineData(1,1)] [InlineData(2,2)] [InlineData(3,3)]
    public void StillVersionsAndVideoDisplayHintsKeepCodedCanvasPixels(int version,int scale)
    {
        var original = File.ReadAllBytes(Path.Combine(FindFixtures(),"vp8-ffmpeg-31x23-effort6-quality75-opaque.webp"));
        var variant = original.ToArray(); variant[20] = (byte)((variant[20]&~14)|(version<<1));
        variant[27] = (byte)((variant[27]&63)|(scale<<6)); variant[29] = (byte)((variant[29]&63)|(scale<<6));
        using var expected = ImageCodec.Decode(original); using var actual = ImageCodec.Decode(variant);
        Assert.Equal((31,23),(actual.Width,actual.Height));
        for (var y=0;y<23;y++) Assert.Equal(expected.GetRow8(y).ToArray(),actual.GetRow8(y).ToArray());
    }

    [Fact]
    public void NonDisplayedFramesAreRejected()
    {
        var original = File.ReadAllBytes(Path.Combine(FindFixtures(),"vp8-ffmpeg-31x23-effort6-quality75-opaque.webp"));
        original[20] &= 239;
        Assert.Throws<ImageDataException>(()=>ImageCodec.Decode(original));
    }

    [Theory]
    [InlineData("vp8-manifest.json", 110)]
    [InlineData("vp8-authored-manifest.json", 54)]
    public void FrozenLossyPixelsMatchExplicitFfmpegConversionAndExactAlpha(string manifestName, int count)
    {
        var directory = FindFixtures();
        using var manifest = JsonDocument.Parse(File.ReadAllBytes(Path.Combine(directory, manifestName)));
        var fixtures = manifest.RootElement.GetProperty("fixtures"); Assert.Equal(count, fixtures.GetArrayLength());
        foreach (var fixture in fixtures.EnumerateArray())
        {
            var name = fixture.GetProperty("name").GetString()!; var encoded = File.ReadAllBytes(Path.Combine(directory, name + ".webp"));
            var expected = File.ReadAllBytes(Path.Combine(directory, name + ".rgba"));
            Assert.Equal(fixture.GetProperty("encodedHash").GetString(), Hash(encoded));
            Assert.Equal(fixture.GetProperty("expectedHash").GetString(), Hash(expected));
            using var actual = ImageCodec.Decode(encoded);
            Assert.Equal((fixture.GetProperty("width").GetInt32(), fixture.GetProperty("height").GetInt32(), 8), (actual.Width, actual.Height, actual.BitDepth));
            var maximum = 0; double squares = 0;
            for (var y = 0; y < actual.Height; y++)
                for (var x = 0; x < actual.Width * 4; x++)
                {
                    var predicted = expected[y * actual.Width * 4 + x]; var value = actual.GetRow8(y)[x];
                    if (x % 4 == 3) { Assert.Equal(predicted, value); continue; }
                    var difference = Math.Abs(predicted - value); maximum = Math.Max(maximum, difference); squares += difference * difference;
                }
            Assert.True(maximum <= 4 && Math.Sqrt(squares / (actual.Width * actual.Height * 3)) <= 1.5, $"{name}: RGB maximum {maximum}.");
        }
    }

    [Fact]
    public void ArithmeticTruncationsAndDeterministicMutationsFailThroughPublicBoundaries()
    {
        var original = File.ReadAllBytes(Path.Combine(FindFixtures(), "vp8-ffmpeg-31x23-effort6-quality75-opaque.webp"));
        var payloadSize = BinaryPrimitives.ReadInt32LittleEndian(original.AsSpan(16));
        var firstPartition = (original[20] | original[21] << 8 | original[22] << 16) >> 5;
        var failures = 0;
        for (var length = 10; length < payloadSize; length++)
        {
            var truncated = new byte[20 + length + (length & 1)]; original.AsSpan(0, 20 + length).CopyTo(truncated);
            BinaryPrimitives.WriteInt32LittleEndian(truncated.AsSpan(4), truncated.Length - 8);
            BinaryPrimitives.WriteInt32LittleEndian(truncated.AsSpan(16), length);
            // Removing padding-only entropy suffixes can still produce a complete valid raster.
            if (length < 10 + firstPartition) Assert.Throws<ImageDataException>(() => ImageCodec.Decode(truncated));
            else
            {
                try { using var decoded = ImageCodec.Decode(truncated); Assert.Equal((31, 23), (decoded.Width, decoded.Height)); }
                catch (ImageDataException) { failures++; }
            }
        }
        Assert.True(failures > 0, "Truncated coefficient partitions must be rejected.");
        var options = new DecodeOptions { Limits = new ImageLimits { MaxDimension = 128, MaxPixels = 4096, MaxWorkingBytes = 1024 * 1024, MaxWork = 2_000_000 } };
        var random = new Random(684921);
        for (var mutation = 0; mutation < 256; mutation++)
        {
            var corrupt = original.ToArray(); corrupt[random.Next(corrupt.Length)] ^= (byte)(1 << random.Next(8));
            try { using var decoded = ImageCodec.Decode(corrupt, options); Assert.InRange((long)decoded.Width * decoded.Height, 1, 4096); Assert.Equal(8, decoded.BitDepth); }
            catch (ImageDataException) { }
            catch (ImageLimitException) { }
            catch (UnsupportedImageException) { }
        }
    }

    [Fact]
    public void LossyDecodeChargesResourceWorkAndHonorsCancellation()
    {
        var encoded = File.ReadAllBytes(Path.Combine(FindFixtures(), "vp8-ffmpeg-31x23-effort6-quality75-alpha.webp"));
        Assert.Throws<ImageLimitException>(() => ImageCodec.Decode(encoded, new DecodeOptions { Limits = new ImageLimits { MaxWorkingBytes = 1024 } }));
        Assert.Throws<ImageLimitException>(() => ImageCodec.Decode(encoded, new DecodeOptions { Limits = new ImageLimits { MaxWork = 64 } }));
        Assert.Throws<ImageLimitException>(() => ImageCodec.Decode(encoded, new DecodeOptions { Limits = new ImageLimits { MaxPixels = 512 } }));
        Assert.Throws<OperationCanceledException>(() => ImageCodec.Decode(encoded, cancellationToken: new CancellationToken(true)));
    }

    private static string FindFixtures()
    {
        for (var directory = new DirectoryInfo(AppContext.BaseDirectory); directory is not null; directory = directory.Parent)
        {
            var path = Path.Combine(directory.FullName, "tests", "Fixtures");
            if (File.Exists(Path.Combine(path, "vp8-manifest.json"))) return path;
        }
        throw new InvalidOperationException("Original VP8 fixture corpus not found.");
    }
    private static string Hash(byte[] data) => Convert.ToHexStringLower(SHA256.HashData(data));
}
