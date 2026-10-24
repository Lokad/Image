using Lokad.Image.Tooling;

namespace Lokad.Image.DifferentialTests;

public sealed class Vp8EncodeFfmpegTests
{
    public static IEnumerable<object[]> Profiles()
    {
        foreach (var size in new[] { (1, 1), (1, 17), (17, 1), (31, 23), (129, 67) })
            foreach (var effort in new[] { 0, 4, 6 })
                foreach (var quality in new[] { 1, 50, 85, 100 })
                    foreach (var depth in new[] { 8, 16 })
                        foreach (var opaque in new[] { false, true }) yield return [size.Item1, size.Item2, effort, quality, depth, opaque];
    }

    [Theory]
    [MemberData(nameof(Profiles))]
    public void IndependentlyEncodedVp8MatchesFfmpegDecodeWithExactAlpha(int width, int height, int effort, int quality, int depth, bool opaque)
    {
        using var source = ImageBuffer.Create(width, height, depth); var input = OriginalPixels.Generate(width, height, depth, opaque: opaque);
        for (var y = 0; y < height; y++)
        {
            var row = depth == 8 ? source.GetRow8(y) : System.Runtime.InteropServices.MemoryMarshal.AsBytes(source.GetRow16(y));
            input.AsSpan(y * source.Stride, source.Stride).CopyTo(row);
        }
        using var output = new MemoryStream();
        ImageCodec.EncodeWebp(source, output, new WebpEncodeOptions { Lossless = false, Quality = quality, Effort = effort });
        var encoded = output.ToArray(); var info = ImageCodec.Identify(encoded);
        var expected = WebpOracle.Decode(encoded, width, height, info.HasAlpha);
        using var decoded = ImageCodec.Decode(encoded); var actual = OriginalPixels.Bytes(decoded);
        Assert.Equal(width * height * 4, expected.Length);
        Assert.Equal(expected, actual);
        for (var i = 0; i < actual.Length; i++)
        {
            if (i % 4 == 3)
            {
                var sample = depth == 8 ? input[i] : (System.Buffers.Binary.BinaryPrimitives.ReadUInt16LittleEndian(input.AsSpan(i * 2)) + 128) / 257;
                Assert.Equal(sample, expected[i]); Assert.Equal(sample, actual[i]);
            }
        }
    }
}
