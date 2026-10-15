using System.IO.Compression;
using Lokad.Image.Tooling;

namespace Lokad.Image.DifferentialTests;

public sealed class PngProfileFfmpegTests
{
    [Theory]
    [InlineData(8)] [InlineData(16)]
    public void StreamingFastestOutputHasExactNativeWordsAndHiddenRgb(int depth)
    {
        using var source = OriginalPixels.Buffer(513, 259, depth); using var output = new MemoryStream();
        ImageCodec.EncodePng(source, output, new PngEncodeOptions { Compression = CompressionLevel.Fastest });
        var encoded = output.ToArray(); var expected = OriginalPixels.Bytes(source);
        Assert.Equal(expected, PngOracle.Decode(encoded, depth));
        var normal = PngOracle.DecodeArguments(depth, encoded).ToList();
        normal.RemoveRange(normal.IndexOf("-cpuflags"), 2);
        Assert.Equal(expected, OracleProcess.Run(OracleProcess.Ffmpeg, normal.ToArray(), encoded));
        using var decoded = ImageCodec.Decode(encoded); Assert.Equal(expected, OriginalPixels.Bytes(decoded));
    }

    public static IEnumerable<object[]> Profiles() => AuthoredPng.Cases().Select(frame => new object[] { frame.Name });

    [Theory]
    [MemberData(nameof(Profiles))]
    public void IndependentPngAuthoringMatchesFfmpeg(string name)
    {
        var frame = AuthoredPng.Cases().Single(frame => frame.Name == name); var encoded = AuthoredPng.Build(frame);
        var expected = AuthoredPng.Expected(frame); var depth = frame.Depth == 16 ? 16 : 8;
        Assert.Equal(expected, PngOracle.Decode(encoded, depth));
        using var decoded = ImageCodec.Decode(encoded); Assert.Equal(expected, OriginalPixels.Bytes(decoded));
    }

    public static IEnumerable<object[]> EncoderProfiles()
    {
        foreach (var depth in new[] { 8, 16 })
        foreach (var color in Enum.GetValues<PngColorType>())
        foreach (var compression in Enum.GetValues<CompressionLevel>())
        foreach (var tiny in new[] { false, true }) yield return new object[] { depth, color, compression, tiny };
    }

    [Theory]
    [MemberData(nameof(EncoderProfiles))]
    public void AllManagedEncoderProfilesHaveExactNativeSamples(int depth, PngColorType color, CompressionLevel compression, bool tiny)
    {
        using var source = OriginalPixels.Buffer(tiny ? 1 : 19, tiny ? 1 : 17, depth,
            grayscale: color is PngColorType.Grayscale or PngColorType.GrayscaleAlpha,
            opaque: color is PngColorType.Grayscale or PngColorType.Rgb);
        using var encoded = new NonseekableOutput();
        ImageCodec.EncodePng(source, encoded, new PngEncodeOptions { ColorType = color, Compression = compression });
        Assert.True(encoded.CanWrite); var bytes = encoded.ToArray(); Assert.Equal((int)color, bytes[25]); Assert.Equal(depth, bytes[24]);
        Assert.Equal(OriginalPixels.Bytes(source), PngOracle.Decode(bytes, depth));
        using var decoded = ImageCodec.Decode(bytes); Assert.Equal(OriginalPixels.Bytes(source), OriginalPixels.Bytes(decoded));
    }

    private sealed class NonseekableOutput : MemoryStream
    {
        public override bool CanSeek => false;
        public override long Position { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }
        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void AuthoredPrefixTreesHaveExactIndependentNativePixels(bool dynamic)
    {
        byte[] raw = [0, 17]; var frame = new AuthoredPng.Frame("prefix", 0, 8, 1, 1, false);
        var compressed = dynamic ? AuthoredDeflate.DynamicLiteral(raw) : AuthoredDeflate.Fixed(raw.Select(value => new AuthoredDeflate.Token(value)), raw);
        var encoded = AuthoredPng.Wrap(frame, compressed);
        Assert.Equal(new byte[] { 17, 17, 17, 0 }, PngOracle.Decode(encoded, 8));
        using var image = ImageCodec.Decode(encoded); Assert.Equal(new byte[] { 17, 17, 17, 0 }, image.GetRow8(0).ToArray());
    }

    [Theory]
    [InlineData(16)]
    [InlineData(17)]
    [InlineData(18)]
    [InlineData(19)]
    [InlineData(15)]
    public void DynamicRepeatsAndDeepTreesMatchNativeSamples(int profile)
    {
        byte[] raw = [0, 17]; var frame = new AuthoredPng.Frame("tree", 0, 8, 1, 1, false);
        var compressed = profile == 19 ? AuthoredDeflate.DynamicCrossAlphabet(raw) : profile == 15 ? AuthoredDeflate.DynamicDeep(raw) : AuthoredDeflate.DynamicLiteral(raw, repeat: profile);
        Assert.Equal(new byte[] { 17, 17, 17, 0 }, PngOracle.Decode(AuthoredPng.Wrap(frame, compressed), 8));
    }
}
