using System.Security.Cryptography;
using Lokad.Image.Tooling;

namespace Lokad.Image.PublicApi.Tests;

public sealed class JpegDefaultProfileTests
{
    [Fact]
    public void ReferenceIsTheDefaultWithoutRenumberingExplicitLegacyProfiles()
    {
        var options = new JpegEncodeOptions();
        Assert.Equal(JpegQuantizationPolicy.Reference, options.QuantizationPolicy);
        Assert.Equal(85, options.Quality); Assert.Equal(JpegSampling.YCbCr420, options.Sampling);
        Assert.Null(options.BackgroundRgb); Assert.Equal(0, options.RestartInterval);
        Assert.Equal(0, (int)JpegQuantizationPolicy.Original);
        Assert.Equal(1, (int)JpegQuantizationPolicy.CompactColor);
        Assert.Equal(2, (int)JpegQuantizationPolicy.Reference);
    }

    [Theory]
    [InlineData(0)] [InlineData(1)] [InlineData(2)] [InlineData(3)]
    [InlineData(4)] [InlineData(5)] [InlineData(6)] [InlineData(7)]
    public async Task AllDefaultWrappersMatchTheFrozenPacketAndKeepExplicitLegacyPackets(int boundary)
    {
        using var root = new TestMemoryBudget(4 * 1024 * 1024);
        var limits = ImageLimits.Default with { MemoryBudget = root };
        var source = OriginalPixels.Generate(513, 259, 8);
        source[3] = 0; source[513 * 4 - 1] = 0;
        var normalized = JpegQualityCorpus.Reference(source, 8, 0x204060);
        using var image = ImageBuffer.Create(513, 259, 8, limits);
        const int stride = 513 * 3 + 5;
        var rgb = Enumerable.Repeat((byte)165, stride * 259 + 7).ToArray();
        for (var y = 0; y < 259; y++)
        {
            normalized.AsSpan(y * 513 * 4, 513 * 4).CopyTo(image.GetRow8(y));
            for (var x = 0; x < 513; x++)
                normalized.AsSpan((y * 513 + x) * 4, 3).CopyTo(rgb.AsSpan(y * stride + x * 3, 3));
        }
        var original = rgb.ToArray();
        var budget = new ImageBudget(ImageLimits.Default.MaxWorkingBytes, long.MaxValue);
        async Task<byte[]> Encode(JpegEncodeOptions? options)
        {
            using var output = new MemoryStream();
            switch (boundary)
            {
                case 0: ImageCodec.EncodeJpeg(image, output, options); break;
                case 1: ImageCodec.EncodeJpeg(rgb, 513, 259, JpegSampleFormat.Rgb, output, stride, options); break;
                case 2: await ImageCodec.EncodeJpegAsync(image, output, options); break;
                case 3: await ImageCodec.EncodeJpegAsync(rgb, 513, 259, JpegSampleFormat.Rgb, output, stride, options); break;
                case 4: budget.EncodeJpeg(image, output, options); break;
                case 5: budget.EncodeJpeg(rgb, 513, 259, JpegSampleFormat.Rgb, output, stride, options); break;
                case 6: await budget.EncodeJpegAsync(image, output, options); break;
                case 7: await budget.EncodeJpegAsync(rgb, 513, 259, JpegSampleFormat.Rgb, output, stride, options); break;
                default: throw new ArgumentOutOfRangeException(nameof(boundary));
            }
            Assert.Equal(513L * 259 * 4, root.Bytes); Assert.False(root.IsDisposed);
            Assert.Equal(budget.MaxWorkingBytes, budget.RemainingWorkingBytes); Assert.True(output.CanWrite);
            return output.ToArray();
        }

        // The normalized control already has a native packet proof. This adds
        // default selection and wrapper coverage, not another content/quality grid.
        var first = await Encode(null);
        var second = await Encode(new() { Limits = limits });
        Assert.Equal(52091, first.Length);
        Assert.Equal("83e9e80a87c1928c8e23e2929b15348d0e8e8ce5568247420f6e366abeb830e3", Hash(first));
        Assert.Equal(first, second); Assert.Equal((byte)0xC0, JpegQualitySyntax.Read(first).FrameMarker);
        var legacy = await Encode(new() { Limits = limits, QuantizationPolicy = JpegQuantizationPolicy.Original });
        Assert.Equal(boundary % 2 == 0 ? "8c69be13f575d2a22f9a75c50d5e7cff1b78bcf4b971e7a16de440329f57b494"
            : "a95c8bb969ffe2c62822afc66b60088fdabb8c4d1d887f724c3264d144dcaa12", Hash(legacy));
        Assert.Equal(original, rgb); Assert.Equal(normalized, OriginalPixels.Bytes(image));
        image.Dispose(); Assert.Equal(0, root.Bytes); Assert.False(root.IsDisposed);
        Assert.Equal(first, second);
    }

    [Fact]
    public async Task DefaultCallerGrayscaleOverridesSamplingWithoutAnRgbaIntermediate()
    {
        var gray = Enumerable.Repeat((byte)123, 64).ToArray();
        using var first = new MemoryStream(); using var second = new MemoryStream();
        ImageCodec.EncodeJpeg(gray, 8, 8, JpegSampleFormat.Grayscale, first);
        await ImageCodec.EncodeJpegAsync(gray, 8, 8, JpegSampleFormat.Grayscale, second,
            options: new() { Sampling = JpegSampling.YCbCr444 });
        Assert.Equal(first.ToArray(), second.ToArray());
        var header = JpegQualitySyntax.Read(first.ToArray());
        Assert.Equal(1, header.Components); Assert.Equal("1x1", header.Sampling);
        Assert.Equal((byte)0xC0, header.FrameMarker);
        gray.AsSpan().Clear(); var decoded = new byte[64];
        ImageCodec.DecodeJpeg(first.ToArray(), decoded, JpegSampleFormat.Grayscale);
        Assert.All(decoded, value => Assert.Equal(123, value));
    }

    private static string Hash(byte[] packet) => Convert.ToHexStringLower(SHA256.HashData(packet));
}
