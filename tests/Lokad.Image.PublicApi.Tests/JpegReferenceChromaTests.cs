using System.Security.Cryptography;

namespace Lokad.Image.PublicApi.Tests;

public sealed class JpegReferenceChromaTests
{
    [Theory]
    [InlineData(1, 255, 255, 0, "3f965a82f040f6821a355596e083e689c916b78e29943fa0258d60be1a876b36")]
    [InlineData(2, 0, 255, 255, "1d0c2d105fe3f5da436c3854333825912ddfb9eafa4cd69df1a0ad102295926a")]
    public void SignedHorizontalChromaGuardsPreserveNativePackets(int size, byte red, byte green, byte blue, string packetHash)
    {
        // Original yellow/cyan controls exercise negative full-range Q7 guards
        // for Cb/Cr, odd/even pairing and tiny-edge row reuse. Both205-byte
        // packets match pinned FFmpeg qscale5/optimal420, apart from the strict
        // attribution COM replacement. No upstream image or packet is embedded.
        using var image = ImageBuffer.Create(size, size, 8);
        for (var y = 0; y < size; y++)
        for (var x = 0; x < size; x++)
        {
            var pixel = image.GetRow8(y).Slice(x * 4, 4);
            pixel[0] = red; pixel[1] = green; pixel[2] = blue; pixel[3] = 255;
        }
        using var output = new MemoryStream(); ImageCodec.EncodeJpeg(image, output);
        var packet = output.ToArray(); Assert.Equal(205, packet.Length);
        Assert.Equal(packetHash, Convert.ToHexStringLower(SHA256.HashData(packet)));
    }
}
