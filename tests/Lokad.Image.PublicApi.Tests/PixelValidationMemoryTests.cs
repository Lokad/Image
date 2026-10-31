using System.Text.Json;
using System.Security.Cryptography;
using Lokad.Image.Tooling;
using Xunit.Abstractions;

namespace Lokad.Image.PublicApi.Tests;

public sealed class PixelValidationMemoryTests(ITestOutputHelper output)
{
    [Theory]
    [InlineData(MetadataProfile.Png8)] [InlineData(MetadataProfile.Png16)]
    [InlineData(MetadataProfile.JpegBaseline420)] [InlineData(MetadataProfile.WebpLosslessAlpha)] [InlineData(MetadataProfile.WebpLossyAlpha)]
    public void ValidationMemoryMatchesTheDeclaredCodingWorkspace(MetadataProfile profile)
    {
        var data = OriginalExif.Create(profile, 513, 259).Encoded;
        Measure(data, false); Measure(data, true); // Warm both fixed paths, without timing.
        var decode = Measure(data, false); var validate = Measure(data, true);
        // VP8L now shares the same single-frame history in both operations.
        // Small runtime/owner allocations can reverse their near tie; a 4 KiB
        // allowance still rejects another 531,468-byte frame or an input copy.
        var sharedHistoryOverhead = profile == MetadataProfile.WebpLosslessAlpha ? 4096 : 0;
        Assert.True(validate.Allocated < decode.Allocated + sharedHistoryOverhead,
            $"{profile}: validation allocations {validate} versus decode {decode}");
        // VP8L validation retains the same history as decoding. Its temporary
        // UInt32 array includes a 32-byte charge; retained image ownership
        // admits pixel capacity, so direct-output decoding can be 32 bytes lower.
        var arrayHeader = profile == MetadataProfile.WebpLosslessAlpha ? 32 : 0;
        Assert.True(validate.Peak <= decode.Peak + arrayHeader, $"{profile}: validation peak {validate} versus decode {decode}");
        if (profile is MetadataProfile.Png8 or MetadataProfile.Png16)
        {
            using var root = new TestMemoryBudget(64 * 1024 * 1024);
            var tight = new ImageLimits { MemoryBudget = root, MaxWorkingBytes = data.Length + validate.Peak };
            _ = ImageCodec.ValidatePixels(data, tight); Assert.Equal(0, root.Bytes);
            var refusal = Assert.Throws<ImageLimitException>(() => ImageCodec.Decode(data, new() { Limits = tight }));
            Assert.Equal(ImageLimitKind.WorkingBytes, refusal.Limit); Assert.Equal(ImageLimitScope.Operation, refusal.Scope);
            Assert.Equal(0, root.Bytes);
        }
        output.WriteLine(JsonSerializer.Serialize(new { profile = profile.ToString(), width = 513, height = 259,
            encodedBytes = data.Length, encodedSha256 = Convert.ToHexStringLower(SHA256.HashData(data)),
            decode = new { allocatedBytes = decode.Allocated, peakAdmittedBytes = decode.Peak },
            validate = new { allocatedBytes = validate.Allocated, peakAdmittedBytes = validate.Peak },
            host = "TestMemoryBudget adapter included; synchronous borrowed span; no timing" }));
    }

    private static (long Allocated, long Peak) Measure(byte[] data, bool validate)
    {
        using var root = new TestMemoryBudget(64 * 1024 * 1024);
        var limits = new ImageLimits { MemoryBudget = root };
        var options = new DecodeOptions { Limits = limits };
        var before = GC.GetAllocatedBytesForCurrentThread();
        if (validate) _ = ImageCodec.ValidatePixels(data, limits);
        else { using var image = ImageCodec.Decode(data, options); }
        var allocated = GC.GetAllocatedBytesForCurrentThread() - before;
        Assert.Equal(0, root.Bytes); Assert.False(root.IsDisposed);
        return (allocated, root.Peak);
    }
}
