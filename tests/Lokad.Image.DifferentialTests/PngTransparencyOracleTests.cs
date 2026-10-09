using Lokad.Image.Tooling;

namespace Lokad.Image.DifferentialTests;

public sealed class PngTransparencyOracleTests
{
    public static IEnumerable<object[]> Keys()
    {
        foreach (var depth in new[] { 1, 2, 4 })
        foreach (var interlace in new[] { false, true })
        for (var sample = 0; sample < 1 << depth; sample++) yield return [depth, interlace, sample];
    }

    [Theory]
    [MemberData(nameof(Keys))]
    public void NativeColorKeyHonorsEveryPackedTransparentLevelWithoutAffectingNeighbors(int depth, bool interlace, int sample)
    {
        var frame = new AuthoredPng.Frame("keys", 0, depth, 1 << depth, 1, interlace, false);
        var encoded = AuthoredPng.Wrap(frame, AuthoredPng.StoredZlib(AuthoredPng.FilteredRows(frame)), ("tRNS", new byte[] { 0, (byte)sample }));
        var expected = AuthoredPng.Expected(frame); var gray = sample * 255 / ((1 << depth) - 1); var transparent = 0;
        for (var i = 0; i < expected.Length; i += 4)
            if (expected[i] == gray) { expected[i + 3] = 0; transparent++; }
        Assert.Equal(1, transparent); // The original odd-step row visits every packed level once.
        Assert.Equal(expected, PngOracle.Decode(encoded, 8));
        using var actual = ImageCodec.Decode(encoded); Assert.Equal(expected, OriginalPixels.Bytes(actual));
    }
}
