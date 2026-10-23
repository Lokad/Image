using System.Security.Cryptography;
using Lokad.Image.Internal;

namespace Lokad.Image.Tests;

public sealed class JpegReferenceColorTests
{
    [Theory] [InlineData(3)] [InlineData(4)]
    public void OriginalRoundingAndClippingTriplesMatchNativeBytes(int channels)
    {
        ReadOnlySpan<byte> triples =
        [
            0,0,0, 255,255,255, 0,255,255, 255,0,0, 128,128,128,
            164,68,96, 16,128,128, 235,128,128, 1,128,255, 254,255,128
        ];
        var y = new byte[10]; var u = new byte[10]; var v = new byte[10];
        for (var x = 0; x < 10; x++) { y[x] = triples[x * 3]; u[x] = triples[x * 3 + 1]; v[x] = triples[x * 3 + 2]; }
        var before = y.Concat(u).Concat(v).ToArray();
        var output = Enumerable.Repeat((byte)165, 10 * channels + 8).ToArray();
        JpegInverseColor.WriteRow(y, u, v, output.AsSpan(3), channels);
        Assert.All(output.Take(3).Concat(output.Skip(3 + 10 * channels)), value => Assert.Equal(165, value));
        var rgba = new byte[40];
        for (var x = 0; x < 10; x++)
        {
            output.AsSpan(3 + x * channels, 3).CopyTo(rgba.AsSpan(x * 4, 3));
            rgba[x * 4 + 3] = channels == 4 ? output[3 + x * channels + 3] : (byte)255;
        }
        Assert.Equal("4147ca81e3d5b4071b5ab23a84e7dfcd84985b484a7e0f74911f9261fcb39a47", Convert.ToHexStringLower(SHA256.HashData(rgba)));
        Assert.Equal(before, y.Concat(u).Concat(v).ToArray());
    }

    [Theory] [InlineData(3)] [InlineData(4)]
    public void QualifiedInverseAndColorReproduceTheCompleteFrozenImage(int channels)
    {
        var planes = JpegReferenceInverseTests.BaselinePlanes();
        var output = new byte[31 * 23 * channels];
        for (var row = 0; row < 23; row++)
            JpegInverseColor.WriteRow(planes.AsSpan(row * 31, 31), planes.AsSpan(713 + row * 31, 31),
                planes.AsSpan(1426 + row * 31, 31), output.AsSpan(row * 31 * channels, 31 * channels), channels);
        var rgba = new byte[31 * 23 * 4];
        for (var x = 0; x < 713; x++)
        {
            output.AsSpan(x * channels, 3).CopyTo(rgba.AsSpan(x * 4, 3));
            rgba[x * 4 + 3] = channels == 4 ? output[x * channels + 3] : (byte)255;
        }
        Assert.Equal("fbf313b3c7bc442832b67d3b08bf228da503aefc142d4ac2ed8d2bfc720b76e4", Convert.ToHexStringLower(SHA256.HashData(rgba)));
    }

    [Theory] [InlineData(9, 10, 40, 4)] [InlineData(10, 9, 40, 4)] [InlineData(10, 10, 29, 3)]
    public void IncompleteRowsFailBeforeMutation(int uLength, int vLength, int outputLength, int channels)
    {
        var output = Enumerable.Repeat((byte)165, outputLength).ToArray();
        Assert.Throws<ArgumentException>(() => JpegInverseColor.WriteRow(new byte[10], new byte[uLength], new byte[vLength], output, channels));
        Assert.All(output, value => Assert.Equal(165, value));
    }

    [Fact]
    public void OverlapAndUnknownPackingFailBeforeMutationAndEmptyRowsLeaveTails()
    {
        var shared = Enumerable.Repeat((byte)165, 24).ToArray();
        Assert.Throws<ArgumentException>(() => JpegInverseColor.WriteRow(shared.AsSpan(0, 4), new byte[4], new byte[4], shared.AsSpan(2, 16), 4));
        Assert.Throws<ArgumentOutOfRangeException>(() => JpegInverseColor.WriteRow([], [], [], shared, 2));
        JpegInverseColor.WriteRow([], [], [], shared, 4);
        Assert.All(shared, value => Assert.Equal(165, value));
    }
}
