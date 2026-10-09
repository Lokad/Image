using Lokad.Image.Tooling;

namespace Lokad.Image.DifferentialTests;

public sealed class JpegQualityOracleTests
{
    [Theory]
    [InlineData(JpegSampling.YCbCr422)]
    [InlineData(JpegSampling.YCbCr420)]
    public void NativeSubsamplingUsesCenteredCellsWithoutMovingAlignedPrimaryColorEdges(JpegSampling sampling)
    {
        const int size = 48; var source = new byte[size * size * 4];
        for (var y = 0; y < size; y++)
        for (var x = 0; x < size; x++)
        {
            var offset = (y * size + x) * 4; source[offset + (x / 16 + y / 16) % 3] = 255; source[offset + 3] = 255;
        }
        // Inspect conversion before entropy coding or decoder chroma expansion.
        // Every sampling cell is a solid primary: analytical full-range BT.601
        // values establish both planes and edge placement independently.
        var arguments = JpegOracle.EncodeArguments(size, size, sampling, 2).ToList();
        arguments[arguments.IndexOf("-c:v") + 1] = "rawvideo";
        arguments.RemoveRange(arguments.IndexOf("-huffman"), 2);
        arguments.RemoveRange(arguments.IndexOf("-q:v"), 2);
        arguments[arguments.LastIndexOf("-f") + 1] = "rawvideo";
        var actual = OracleProcess.Run(OracleProcess.Ffmpeg, arguments.ToArray(), source);
        var vertical = sampling == JpegSampling.YCbCr420 ? 2 : 1; var chromaWidth = size / 2; var chromaHeight = size / vertical;
        Assert.Equal(size * size + 2 * chromaWidth * chromaHeight, actual.Length);
        int[][] primaryValues = [[76, 85, 255], [150, 44, 21], [29, 255, 107]];
        var position = 0;
        for (var channel = 0; channel < 3; channel++)
        {
            var h = channel == 0 ? 1 : 2; var v = channel == 0 ? 1 : vertical;
            for (var y = 0; y < size / v; y++)
            for (var x = 0; x < size / h; x++)
            {
                var expected = primaryValues[(x * h / 16 + y * v / 16) % 3][channel];
                Assert.InRange(Math.Abs(actual[position++] - expected), 0, 2);
            }
        }
    }

    [Fact]
    public void Native444EncodingPreservesAlignedPrimaryColorCellsWithoutChromaDisplacement()
    {
        const int size = 24; var source = new byte[size * size * 4];
        for (var y = 0; y < size; y++)
        for (var x = 0; x < size; x++)
        {
            var offset = (y * size + x) * 4; source[offset + (x / 8 + y / 8) % 3] = 255; source[offset + 3] = 255;
        }
        // Constant 8x8 blocks need DC only. A wrong half-pixel chroma offset
        // blends neighboring colors and fails this source-pixel bound.
        var encoded = OracleProcess.Run(OracleProcess.Ffmpeg, JpegOracle.EncodeArguments(size, size, JpegSampling.YCbCr444, 2), source);
        var actual = JpegOracle.Decode(encoded, size, size, JpegSampling.YCbCr444);
        var difference = JpegQualityCorpus.Compare(source, actual);
        Assert.True(difference.Maximum <= 4 && difference.Rmse <= 1.5, $"Native color cells: maximum {difference.Maximum}, RMSE {difference.Rmse}.");
        var header = JpegQualitySyntax.Read(encoded); Assert.Equal(3, header.Components);
        Assert.Single(header.Sampling.Split(',').Distinct()); // Equal factors express 4:4:4, including 1x2 for all three.
    }
}
