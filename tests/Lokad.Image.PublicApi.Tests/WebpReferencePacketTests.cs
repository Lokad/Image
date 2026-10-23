using System.Security.Cryptography;
using Lokad.Image.Tooling;

namespace Lokad.Image.PublicApi.Tests;

public sealed class WebpReferencePacketTests
{
    private static ImageBuffer Source(string name, int depth, TestMemoryBudget root)
        => OriginalVp8lControls.Buffer(name, depth, root);

    [Theory]
    [MemberData(nameof(OriginalVp8lControls.Cases), MemberType = typeof(OriginalVp8lControls))]
    public void PublicLosslessEncodingMatchesTheFrozenNativePackets(string name, int effort, int bytes, string expected)
    {
        using var root = new TestMemoryBudget(128_000_000);
        using var image = Source(name, 8, root); var original = OriginalPixels.Bytes(image); var retained = root.Bytes;
        using var output = new MemoryStream();
        ImageCodec.EncodeWebp(image, output, new() { Effort = effort, Limits = new() { MemoryBudget = root } });
        var packet = output.ToArray(); Assert.Equal(bytes, packet.Length);
        Assert.Equal(expected, Convert.ToHexStringLower(SHA256.HashData(packet)));
        Assert.Equal(original, OriginalPixels.Bytes(image)); Assert.Equal(retained, root.Bytes);
        image.Dispose(); Assert.Equal(0, root.Bytes); Assert.True(output.CanWrite); Assert.False(root.IsDisposed);
        Assert.Equal(expected, Convert.ToHexStringLower(SHA256.HashData(packet)));
    }

    [Theory]
    [InlineData(8)]
    [InlineData(16)]
    public async Task NormalizationAndAllPublicBudgetAsyncWrappersKeepExactPacketsAndSourceOwnership(int depth)
    {
        using var root = new TestMemoryBudget(128_000_000);
        using var image = Source("asymmetric", depth, root); var original = OriginalPixels.Bytes(image); var retained = root.Bytes;
        var options = new WebpEncodeOptions { Limits = new() { MemoryBudget = root, MaxWorkingBytes = 128_000_000 } };
        var budget = new ImageBudget(128_000_000, long.MaxValue);
        for (var mode = 0; mode < 4; mode++)
        {
            using var output = new MemoryStream();
            if (mode == 0) ImageCodec.EncodeWebp(image, output, options);
            else if (mode == 1) budget.EncodeWebp(image, output, options);
            else if (mode == 2) await ImageCodec.EncodeWebpAsync(image, output, options);
            else await budget.EncodeWebpAsync(image, output, options);
            var packet = output.ToArray();
            Assert.Equal("08ddac115f1770d778e6dbc76373440224b32e6f93b616605e234c63206eebde", Convert.ToHexStringLower(SHA256.HashData(packet)));
            Assert.Equal(original, OriginalPixels.Bytes(image)); Assert.Equal(retained, root.Bytes); Assert.True(output.CanWrite);
            Assert.Equal(128_000_000, budget.RemainingWorkingBytes);
        }
        image.Dispose(); Assert.Equal(0, root.Bytes); Assert.False(root.IsDisposed);
    }
}
