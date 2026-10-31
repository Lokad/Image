using Lokad.Image.Internal;

namespace Lokad.Image.Tests;

public sealed class Vp8lOpaquePredictionTests
{
    [Fact]
    public void EveryPredictorPreservesOpaqueAlphaAcrossColorExtremes()
    {
        uint state = 0xa56932f1;
        for (var i = 0; i < 4096; i++)
        {
            uint Pixel() { state ^= state << 13; state ^= state >> 17; state ^= state << 5; return state | 0xff000000u; }
            var left = Pixel(); var top = Pixel(); var topLeft = Pixel(); var topRight = Pixel();
            for (var mode = 0; mode < 14; mode++)
                Assert.Equal(255u, WebpLosslessDecoder.Predict(mode, left, top, topLeft, topRight) >> 24);
        }
    }
}
