using System.IO.Compression;
using System.Security.Cryptography;
using Lokad.Image.Internal;
using Lokad.Image.Tooling;

namespace Lokad.Image.Tests;

public sealed class PngDeflaterMatchPacketTests
{
    // Frozen on the actual qualified producer on both OSes before comparison
    // changes. These exact packets also retain generic-write delivery behavior.
    [Theory]
    [InlineData(false, 6, 194728, "4f018a3e97ec49f78ddb73365caace2d6f8012beb026ffcb6033f38f11148ab7")]
    [InlineData(false, 9, 194728, "d264b218480edf0d84609f1fc2c1297d7eff25730239cf1fdf916f1921086fc8")]
    [InlineData(true, 6, 1702, "0667b95cf3e7480d5979bd9301670229be2d5d70d8911f35bba5379bf932edf4")]
    [InlineData(true, 9, 1475, "6fe7ddbea7185197517ed8015c09e581227b8dc4df6279fedcf39facc48f8bea")]
    public void OriginalRingAndOverlapControlsKeepCompletePackets(bool repeated, int level, int packetBytes, string packetHash)
    {
        var data = OriginalDeflateMatch.Create(repeated);
        var inputHash = repeated ? "8af1885ac7ec8c69b6b4a70bd2438b69e9657ae9db61055afe20d32b08ed9714"
            : "78f54937c38b32d81ab871a11c49e7aa18d344d28c06960516782e4260885401";
        Assert.Equal(inputHash, Hash(data));
        foreach (var chunk in new[] { 1, 262, 65521 })
        {
            using var context = new OperationContext(ImageLimits.Default, default);
            using var output = new MemoryStream();
            using (var compressor = new PngDeflater(output, context, level))
            {
                for (var offset = 0; offset < data.Length; offset += chunk)
                    compressor.Write(data.AsSpan(offset, Math.Min(chunk, data.Length - offset)));
                compressor.Complete();
            }
            var packet = output.ToArray(); Assert.Equal(packetBytes, packet.Length); Assert.Equal(packetHash, Hash(packet));
            Assert.True(output.CanWrite); Assert.Equal(inputHash, Hash(data));
            using var input = new MemoryStream(packet); using var inflater = new ZLibStream(input, CompressionMode.Decompress);
            using var decoded = new MemoryStream(); inflater.CopyTo(decoded); Assert.Equal(data, decoded.ToArray());
        }
    }

    private static string Hash(byte[] bytes) => Convert.ToHexStringLower(SHA256.HashData(bytes));
}
