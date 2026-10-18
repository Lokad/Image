using System.Runtime.Intrinsics.X86;
using Lokad.Image.Internal;

namespace Lokad.Image.Tests;

public sealed class WebpPredictorVectorTests
{
    [Fact]
    public void FusedPredictorsMatchOriginalMathForAllModesAndIndependentChannelNeighborhoods()
    {
        if (!Sse2.IsSupported) return;
        uint state = 0x640163A5;
        uint Next() { state ^= state << 13; state ^= state >> 17; state ^= state << 5; return state; }
        uint[] edges = [0, uint.MaxValue, 0x01010101, 0x80808080, 0x7F80FEFF, 0xFFFE8001, 0x000000FF, 0xFF000000];
        for (var mode = 0; mode < 14; mode++)
        for (var n = 0; n < 4096; n++)
        {
            var left = n < 64 ? edges[n & 7] : Next();
            var top = n < 64 ? edges[n >> 3] : Next();
            var corner = n < 64 ? edges[(n * 3) & 7] : Next();
            var right = n < 64 ? edges[(n * 5) & 7] : Next();
            var residual = Next();
            Assert.Equal(WebpLosslessDecoder.Add(residual, WebpLosslessDecoder.Predict(mode, left, top, corner, right)),
                WebpLosslessDecoder.PredictAndAdd(mode, residual, left, top, corner, right));
        }
        Assert.Throws<ImageDataException>(() => WebpLosslessDecoder.PredictAndAdd(14, 0, 0, 0, 0, 0));
    }

    [Fact]
    public void EveryBytePairKeepsModuloAdditionFloorAverageAndSignedHalfGradient()
    {
        if (!Sse2.IsSupported) return;
        for (uint a = 0; a < 256; a++)
        for (uint b = 0; b < 256; b++)
        {
            var first = a | (255 - a) << 8 | (a ^ 128) << 16 | (a ^ 1) << 24;
            var second = b | (b ^ 128) << 8 | (255 - b) << 16 | (b ^ 1) << 24;
            const uint residual = 0xFF0180FE;
            Assert.Equal(WebpLosslessDecoder.Add(first, second), WebpLosslessDecoder.PredictAndAdd(1, first, second, 0, 0, 0));
            Assert.Equal(WebpLosslessDecoder.Add(residual, WebpLosslessDecoder.Predict(7, first, second, 0, 0)),
                WebpLosslessDecoder.PredictAndAdd(7, residual, first, second, 0, 0));
            // left==top spans every average/previous pair, so every negative,
            // positive, odd and even half-gradient is checked independently.
            Assert.Equal(WebpLosslessDecoder.Add(residual, WebpLosslessDecoder.Predict(13, first, first, second, 0)),
                WebpLosslessDecoder.PredictAndAdd(13, residual, first, first, second, 0));
        }
    }
}
