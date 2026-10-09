using System.Buffers.Binary;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Lokad.Image.Tooling;

namespace Lokad.Image.PublicApi.Tests;

public sealed class AuthoredVp8Tests
{
    [Fact]
    public void OriginalSyntaxReproducesFrozenIndependentOracleCases()
    {
        var root = new DirectoryInfo(AppContext.BaseDirectory);
        while (root is not null && !File.Exists(Path.Combine(root.FullName, "tests", "Fixtures", "vp8-authored-manifest.json"))) root = root.Parent;
        Assert.NotNull(root);
        using var manifest = JsonDocument.Parse(File.ReadAllBytes(Path.Combine(root.FullName, "tests", "Fixtures", "vp8-authored-manifest.json")));
        var frames = AuthoredVp8.Cases().ToDictionary(frame => "vp8-authored-" + frame.Name); Assert.Equal(54, frames.Count);
        foreach (var fixture in manifest.RootElement.GetProperty("fixtures").EnumerateArray())
        {
            var frame = frames[fixture.GetProperty("name").GetString()!];
            Assert.Equal(fixture.GetProperty("inputHash").GetString(), Hash(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(frame))));
            Assert.Equal(fixture.GetProperty("encodedHash").GetString(), Hash(AuthoredVp8.Build(frame)));
            var recorded = JsonSerializer.Deserialize<AuthoredVp8.Frame>(fixture.GetProperty("syntax").GetRawText());
            Assert.NotNull(recorded);
            Assert.Equal(fixture.GetProperty("inputHash").GetString(), Hash(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(recorded))));
        }
    }

    [Theory]
    [InlineData("filter-deltas-zero-base")]
    [InlineData("filter-zero-base-segment-override")]
    public void ZeroFrameLevelDisablesPositiveFilterAdjustments(string name)
    {
        var frame = AuthoredVp8.Cases().Single(frame => frame.Name == name);
        using var adjusted = ImageCodec.Decode(AuthoredVp8.Build(frame));
        using var unfiltered = ImageCodec.Decode(AuthoredVp8.Build(frame with { LoopDeltasEnabled = false, SegmentFilters = new int?[4] }));
        for (var y = 0; y < frame.Height; y++) Assert.Equal(unfiltered.GetRow8(y).ToArray(), adjusted.GetRow8(y).ToArray());
    }

    [Fact]
    public void FlatAuthoredResidualHasAnalyticalGraySamples()
    {
        var frame = AuthoredVp8.Cases().Single(frame => frame.Name == "flat-dc");
        using var decoded = ImageCodec.Decode(AuthoredVp8.Build(frame));
        // No residual: Y=U=V=128. Limited-range luma (128-16)*255/219 rounds to 130.
        Assert.Equal(new byte[] { 130, 130, 130, 255 }, decoded.GetRow8(0).ToArray());
    }

    [Fact]
    public void PartitionSizesTruncationsAndEmptyPartitionsHaveTypedFailures()
    {
        var frame = AuthoredVp8.Cases().Single(frame => frame.Name == "partitions-8"); var original = AuthoredVp8.Build(frame);
        var headerLength = (original[20] | original[21] << 8 | original[22] << 16) >> 5; var sizes = 30 + headerLength;
        for (var partition = 0; partition < 7; partition++)
            foreach (var length in new[] { 0, 1, 0xFFFFFF })
            {
                var encoded = original.ToArray(); var offset = sizes + partition * 3;
                encoded[offset] = (byte)length; encoded[offset + 1] = (byte)(length >> 8); encoded[offset + 2] = (byte)(length >> 16);
                Assert.Throws<ImageDataException>(() => ImageCodec.Decode(encoded));
            }
        var payload = original.AsSpan(20, BinaryPrimitives.ReadInt32LittleEndian(original.AsSpan(16))).ToArray();
        Assert.Throws<ImageDataException>(() => ImageCodec.Decode(AuthoredVp8.Wrap(payload.AsSpan(0, 10 + headerLength + 20).ToArray())));
        foreach (var invalid in AuthoredVp8.InvalidEmptyPartitions())
            Assert.Throws<ImageDataException>(() => ImageCodec.Decode(AuthoredVp8.Build(invalid)));
    }

    private static string Hash(byte[] bytes) => Convert.ToHexStringLower(SHA256.HashData(bytes));
}
