using System.Buffers.Binary;
using Lokad.Image.Tooling;

namespace Lokad.Image.DifferentialTests;

public sealed class ResizeOracleTests
{
    [Fact]
    public void TransparentBilinearDownsamplePreservesPremultipliedColorAndExactAlpha()
    {
        var pixels = OriginalPixels.Generate(513, 259, 8);
        pixels[3] = pixels[513 * 4 - 1] = 0;
        var expected = ExactResizeReference.Resize(pixels, 513, 259, 8, 256, 129, ResizeFilter.Bilinear);
        var native = OracleProcess.Run(OracleProcess.Ffmpeg,
            ResizeOracle.PremultipliedBilinearArguments(513, 259, 256, 129), pixels);
        Assert.Equal(expected.Length, native.Length);
        long squared = 0; var maximum = 0;
        for (var i = 0; i < native.Length; i += 4)
        {
            Assert.Equal(expected[i + 3], native[i + 3]);
            for (var c = 0; c < 3; c++)
            {
                if (native[i + 3] == 0) Assert.Equal(0, native[i + c]);
                var difference = Math.Abs(expected[i + c] - native[i + c]);
                maximum = Math.Max(maximum, difference); squared += difference * difference;
            }
        }
        Assert.True(maximum <= 1 && Math.Sqrt((double)squared / (native.Length / 4 * 3)) <= 0.25,
            "Native premultiplied resize exceeds the existing filtered RGB comparison bound.");
        using var source = OriginalPixels.Buffer(513, 259, 8);
        source.GetRow8(0)[3] = source.GetRow8(0)[513 * 4 - 1] = 0;
        using var resized = ImageTransforms.Resize(source, 256, 129, ResizeFilter.Bilinear);
        Assert.Equal(expected, OriginalPixels.Bytes(resized));
        Assert.Equal(pixels, OriginalPixels.Bytes(source));
    }

    public static IEnumerable<object[]> NearestProfiles()
    {
        foreach (var depth in new[] { 8, 16 })
        foreach (var opaque in new[] { false, true })
        foreach (var size in new[] { (9, 7, 5, 3), (9, 7, 19, 15), (129, 67, 64, 33),
            (129, 67, 257, 135), (513, 259, 256, 129), (513, 259, 1025, 519),
            (1025, 515, 512, 257), (1025, 515, 2049, 1031),
            (2049, 1027, 1024, 513), (2049, 1027, 4097, 2055) })
            yield return [depth, opaque, size.Item1, size.Item2, size.Item3, size.Item4];
    }

    [Theory]
    [MemberData(nameof(NearestProfiles))]
    public void NativePointNearestResizingPreservesEverySample(int depth, bool opaque, int sourceWidth, int sourceHeight, int width, int height)
    {
        var pixels = OriginalPixels.Generate(sourceWidth, sourceHeight, depth, opaque: opaque);
        var expected = ExactResizeReference.Resize(pixels, sourceWidth, sourceHeight, depth, width, height, ResizeFilter.Nearest);
        var native = OracleProcess.Run(OracleProcess.Ffmpeg, ResizeOracle.NearestArguments(sourceWidth, sourceHeight, width, height, depth), pixels);
        Assert.Equal(expected, native);
        using var source = OriginalPixels.Buffer(sourceWidth, sourceHeight, depth, opaque: opaque);
        using var resized = ImageTransforms.Resize(source, width, height, ResizeFilter.Nearest);
        Assert.Equal(expected, OriginalPixels.Bytes(resized));
    }

    [Fact]
    public void NativePointNearestRetainsAllSixteenBitValuesIncludingAlphaAndHiddenRgb()
    {
        var pixels = new byte[256 * 256 * 8];
        for (var value = 0; value < 65536; value++)
        for (var channel = 0; channel < 4; channel++)
            BinaryPrimitives.WriteUInt16LittleEndian(pixels.AsSpan(value * 8 + channel * 2), (ushort)((value + channel * 23459) & 65535));
        var expected = ExactResizeReference.Resize(pixels, 256, 256, 16, 257, 257, ResizeFilter.Nearest);
        Assert.Equal(expected, OracleProcess.Run(OracleProcess.Ffmpeg, ResizeOracle.NearestArguments(256, 256, 257, 257, 16), pixels));
    }

    public static IEnumerable<object[]> LargeNearestEquationProfiles()
    {
        foreach (var sourceWidth in new[] { 1025, 2049 })
        foreach (var depth in new[] { 8, 16 })
        foreach (var upscale in new[] { false, true })
            yield return [sourceWidth, depth, upscale];
    }

    [Theory]
    [MemberData(nameof(LargeNearestEquationProfiles))]
    public void LargeNearestMatchesNativeCoordinateEquationIncludingAlpha(int sourceWidth, int depth, bool upscale)
    {
        var sourceHeight = sourceWidth / 2 + 3;
        var width = upscale ? sourceWidth * 2 - 1 : sourceWidth / 2;
        var height = upscale ? sourceHeight * 2 + 1 : sourceHeight / 2;
        var pixels = OriginalPixels.Generate(sourceWidth, sourceHeight, depth);
        var expected = ExactResizeReference.Resize(pixels, sourceWidth, sourceHeight, depth, width, height, ResizeFilter.Nearest);

        // The native scaler's finite-precision coordinates can select a different
        // cell near large-axis boundaries. Evaluate the contract using FFmpeg's
        // documented geq pixel access instead. This is a correctness oracle;
        // it does not replace the optimized scaler in performance benchmarks.
        // Padding gives geq the required output canvas without modifying input
        // samples; every queried coordinate remains inside the original image.
        var select = $"pad={Math.Max(sourceWidth, width)}:{Math.Max(sourceHeight, height)}:0:0,"
            + $"geq=lum='p(floor((2*X+1)*{sourceWidth}/(2*{width})),floor((2*Y+1)*{sourceHeight}/(2*{height})))':i=nearest,"
            + $"crop={width}:{height}:0:0:exact=1,setsar=1";
        var planar = depth == 16 ? "gbrap16le" : "gbrap";
        var packed = depth == 16 ? "rgba64le" : "rgba";
        var graph = $"extractplanes=r+g+b+a[r][g][b][a];[r]{select}[rs];[g]{select}[gs];[b]{select}[bs];[a]{select}[as];"
            + $"[gs][bs][rs][as]mergeplanes=0x00102030:format={planar},format={packed}";
        var native = OracleProcess.Run(OracleProcess.Ffmpeg,
            ["-hide_banner", "-loglevel", "error", "-threads:v", "1", "-f", "rawvideo", "-pixel_format", packed,
                "-video_size", $"{sourceWidth}x{sourceHeight}", "-i", "pipe:0", "-filter_complex_threads", "1",
                "-filter_complex", graph, "-frames:v", "1", "-threads:v", "1", "-pix_fmt", packed, "-f", "rawvideo", "pipe:1"], pixels);
        Assert.Equal(expected, native);
        using var source = OriginalPixels.Buffer(sourceWidth, sourceHeight, depth);
        using var resized = ImageTransforms.Resize(source, width, height, ResizeFilter.Nearest);
        Assert.Equal(expected, OriginalPixels.Bytes(resized));
    }

    public static IEnumerable<object[]> OpaqueDownsizeProfiles()
    {
        foreach (var pattern in new[] { "asymmetric-v1", "soft-gradient", "color-tiles" })
        foreach (var filter in Enum.GetValues<ResizeFilter>()) yield return [pattern, filter];
    }

    [Theory]
    [MemberData(nameof(OpaqueDownsizeProfiles))]
    public void ExplicitOpaqueDownsizeProfileMatchesTheOriginalSamples(string pattern, ResizeFilter filter)
    {
        var source = JpegQualityCorpus.Source(pattern, 8, JpegSampling.YCbCr444);
        using var image = JpegQualityCorpus.Buffer(source, 8); using var resized = ImageTransforms.Resize(image, 64, 33, filter);
        var native = OracleProcess.Run(OracleProcess.Ffmpeg, ResizeOracle.Arguments(image.Width, image.Height, 64, 33, filter), source);
        var difference = JpegQualityCorpus.Compare(OriginalPixels.Bytes(resized), native);
        Assert.True(difference.Maximum <= (filter == ResizeFilter.Nearest ? 0 : 1) && difference.Rmse <= 0.25,
            $"{pattern}/{filter}: maximum {difference.Maximum}, RMSE {difference.Rmse}.");
    }

    [Fact]
    public void NativeNormalizationChecksEveryPossibleSixteenBitChannelValueExactly()
    {
        const int width = 256, height = 256; var source = new byte[width * height * 8]; var expected = new byte[width * height * 4];
        for (var value = 0; value < 65536; value++)
        {
            for (var channel = 0; channel < 3; channel++)
            {
                var sample = (value + channel * 23459) & 65535;
                BinaryPrimitives.WriteUInt16LittleEndian(source.AsSpan(value * 8 + channel * 2), (ushort)sample);
                expected[value * 4 + channel] = (byte)((sample + 128) / 257);
            }
            BinaryPrimitives.WriteUInt16LittleEndian(source.AsSpan(value * 8 + 6), 65535); expected[value * 4 + 3] = 255;
        }
        Assert.Equal(expected, OracleProcess.Run(OracleProcess.Ffmpeg, RgbaOracle.Normalize16Arguments(width, height), source));
    }
}
