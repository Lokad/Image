using Lokad.Image.Tooling;

namespace Lokad.Image.DifferentialTests;

public sealed class WebpFfmpegTests
{
    public static IEnumerable<object[]> EncodeProfiles()
    {
        foreach (var effort in new[] { 0, 1, 4, 6 })
        foreach (var depth in new[] { 8, 16 }) yield return [effort, depth];
    }

    [Theory]
    [MemberData(nameof(EncodeProfiles))]
    public void ManagedLosslessEncodeInteroperatesExactly(int effort, int depth)
    {
        using var source = OriginalPixels.Buffer(31, 23, depth);
        using var encoded = new MemoryStream();
        ImageCodec.EncodeWebp(source, encoded, new WebpEncodeOptions { Effort = effort });
        using var actual = ImageCodec.Decode(encoded.ToArray());
        Assert.Equal(ReferenceDecode(encoded.ToArray()), OriginalPixels.Bytes(actual));
        var expected = depth == 8 ? OriginalPixels.Bytes(source) : OriginalPixels.Bytes(source).Chunk(2)
            .Select(sample => (byte)((System.Buffers.Binary.BinaryPrimitives.ReadUInt16LittleEndian(sample) + 128) / 257)).ToArray();
        Assert.Equal(expected, OriginalPixels.Bytes(actual));
    }

    [Theory]
    [InlineData(2)] [InlineData(3)] [InlineData(4)] [InlineData(5)] [InlineData(6)] [InlineData(7)] [InlineData(8)] [InlineData(9)]
    [InlineData(12)] [InlineData(13)] [InlineData(16)] [InlineData(17)] [InlineData(24)] [InlineData(25)]
    [InlineData(3072)] [InlineData(4096)] [InlineData(4097)]
    public void RepeatedColorRunLengthsInteroperateAcrossPrefixBoundaries(int pixels)
    {
        using var source = ImageBuffer.Create(1, pixels, 8);
        for (var y = 0; y < pixels; y++) new byte[] { 10, 20, 30, 40 }.CopyTo(source.GetRow8(y));
        using var encoded = new MemoryStream(); ImageCodec.EncodeWebp(source, encoded, new WebpEncodeOptions { Effort = 6 });
        Assert.Equal(OriginalPixels.Bytes(source), ReferenceDecode(encoded.ToArray()));
    }

    [Fact]
    public void AuthoredLosslessBranchesInteroperateExactly()
    {
        var fixtures = new List<byte[]> { AuthoredWebp.ColorTransform(), AuthoredWebp.SubtractGreen(), AuthoredWebp.CachedColor(),
            AuthoredWebp.BackwardReference(), AuthoredWebp.EntropyGroups(), AuthoredWebp.DuplicateSimpleSymbols(),
            AuthoredWebp.CodeLengthRepeats(false), AuthoredWebp.CodeLengthRepeats(true) };
        fixtures.Add(AuthoredWebp.OrientedPalette(false)); fixtures.Add(AuthoredWebp.OrientedPalette(true));
        foreach (var cacheBits in Enumerable.Range(2, 10)) fixtures.Add(AuthoredWebp.CachedColor(cacheBits));
        foreach (var groups in new[] { 16, 256 }) fixtures.Add(AuthoredWebp.EntropyGroups(groups));
        foreach (var size in new[] { (1, 1), (64, 1), (99, 1), (17, 11), (16384, 1) })
            fixtures.Add(AuthoredWebp.ConstantRaster(size.Item1, size.Item2));
        for (var mode = 0; mode < 14; mode++) fixtures.Add(AuthoredWebp.Predictor(mode));
        foreach (var count in new[] { 1, 2, 3, 4, 16, 17, 256 })
        foreach (var predictor in new[] { false, true }) fixtures.Add(AuthoredWebp.Palette(count, predictor));
        foreach (var encoded in fixtures)
        {
            var expected = ReferenceDecode(encoded);
            using var actual = ImageCodec.Decode(encoded);
            Assert.Equal(expected, OriginalPixels.Bytes(actual));
        }
    }

    public static IEnumerable<object[]> Profiles()
    {
        foreach (var size in new[] { (1, 1), (1, 17), (17, 1), (31, 23), (129, 67) })
        foreach (var effort in new[] { 0, 4, 6 })
        foreach (var opaque in new[] { false, true }) yield return [size.Item1, size.Item2, effort, opaque];
    }

    [Theory]
    [MemberData(nameof(Profiles))]
    public void LosslessDecodeMatchesFfmpegExactly(int width, int height, int effort, bool opaque)
    {
        var source = OriginalPixels.Generate(width, height, 8, opaque: opaque);
        var encoded = OracleProcess.Run(OracleProcess.Ffmpeg,
            ["-hide_banner", "-loglevel", "error", "-cpuflags", "0", "-f", "rawvideo", "-pixel_format", "rgba",
             "-video_size", $"{width}x{height}", "-i", "pipe:0", "-frames:v", "1", "-threads:v", "1", "-c:v", "libwebp",
             "-lossless", "1", "-compression_level", effort.ToString(), "-f", "image2pipe", "pipe:1"], source);
        var expected = ReferenceDecode(encoded);
        using var actual = ImageCodec.Decode(encoded);
        Assert.Equal((width, height, 8), (actual.Width, actual.Height, actual.BitDepth));
        Assert.Equal(expected, OriginalPixels.Bytes(actual));
    }

    internal static byte[] ReferenceDecode(byte[] encoded) => OracleProcess.Run(OracleProcess.Ffmpeg,
            ["-hide_banner", "-loglevel", "error", "-cpuflags", "0", "-noautorotate", "-f", "image2pipe", "-i", "pipe:0",
             "-frames:v", "1", "-threads:v", "1", "-pix_fmt", "rgba", "-f", "rawvideo", "pipe:1"], encoded);
}
