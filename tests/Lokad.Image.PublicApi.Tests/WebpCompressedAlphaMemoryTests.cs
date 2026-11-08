using System.Buffers.Binary;
using System.Security.Cryptography;
using System.Text.Json;
using Lokad.Image.Tooling;
using Xunit.Abstractions;

namespace Lokad.Image.PublicApi.Tests;

public sealed class WebpCompressedAlphaMemoryTests(ITestOutputHelper output)
{
    [Fact]
    public void PaletteAlphaKeepsPackedGreenSamplesAndIndependentOwnership()
    {
        var fixture = OriginalExif.Create(MetadataProfile.WebpLossyAlpha, 9, 1);
        using var original = ImageCodec.Decode(fixture.Encoded);
        var expected = OriginalPixels.Bytes(original);
        using var root = new TestMemoryBudget(4 * 1024 * 1024);
        var options = new DecodeOptions { Limits = new() { MemoryBudget = root } };
        foreach (var (colors, predictor) in new[] { (2, false), (4, false), (16, false), (17, false), (2, true) })
        foreach (var filter in new[] { 0, 1 })
        {
            var palette = AuthoredWebp.Palette(colors, predictor);
            var length = BinaryPrimitives.ReadInt32LittleEndian(palette.AsSpan(16));
            var alpha = new byte[length - 4]; alpha[0] = (byte)(1 | filter << 2);
            palette.AsSpan(25, length - 5).CopyTo(alpha.AsSpan(1));
            var encoded = ReplaceAlpha(fixture.Encoded, alpha);
            var unchanged = encoded.ToArray();
            var palettePixels = AuthoredWebp.ExpectedPalette(colors);
            var previous = 0;
            for (var i = 0; i < 9; i++)
            {
                var green = palettePixels[i * 4 + 1];
                previous = filter == 0 ? green : (previous + green) & 255;
                expected[i * 4 + 3] = (byte)previous;
            }
            using (var first = ImageCodec.Decode(encoded, options))
            using (var survivor = ImageCodec.Decode(encoded, options))
            {
                Assert.Equal(expected, OriginalPixels.Bytes(first));
                Assert.Equal(expected, OriginalPixels.Bytes(survivor));
                Assert.Equal(72, root.Bytes);
                first.Dispose();
                Assert.Equal(36, root.Bytes);
                Assert.Equal(expected, OriginalPixels.Bytes(survivor));
            }
            Assert.Equal(0, root.Bytes);
            Assert.Equal(unchanged, encoded);
        }
        Assert.False(root.IsDisposed);
    }

    [Fact]
    public void MalformedAlphaAfterOutputAllocationReleasesItsOwner()
    {
        var fixture = OriginalExif.Create(MetadataProfile.WebpLossyAlpha, 513, 259);
        var encoded = AuthoredAlpha.Wrap(fixture.Encoded, 513, 259, 1, compressed: true);
        var alphaOffset = 12;
        while (!encoded.AsSpan(alphaOffset, 4).SequenceEqual("ALPH"u8))
        {
            var size = BinaryPrimitives.ReadInt32LittleEndian(encoded.AsSpan(alphaOffset + 4));
            alphaOffset += 8 + size + (size & 1);
        }
        var length = BinaryPrimitives.ReadInt32LittleEndian(encoded.AsSpan(alphaOffset + 4));
        var invalid = ReplaceAlpha(encoded, encoded.AsSpan(alphaOffset + 8, length - 1).ToArray());
        var unchanged = invalid.ToArray();
        var frameBytes = 513 * 259 * 4;
        using var root = new TestMemoryBudget(4 * 1024 * 1024);
        var options = new DecodeOptions { Limits = new() { MemoryBudget = root } };
        var outputAdmitted = false;
        root.Admitted = bytes => outputAdmitted |= bytes >= frameBytes;
        Assert.Throws<ImageDataException>(() => ImageCodec.Decode(invalid, options));
        Assert.True(outputAdmitted);
        Assert.Equal(0, root.Bytes);
        Assert.Equal(0, root.Refusals);
        root.Admitted = null;
        using (var recovered = ImageCodec.Decode(encoded, options))
        {
            var expected = AuthoredAlpha.Samples(513, 259);
            for (var i = 0; i < expected.Length; i++)
                Assert.Equal(expected[i], recovered.GetRow8(i / 513)[i % 513 * 4 + 3]);
            Assert.Equal(frameBytes, root.Bytes);
        }
        Assert.Equal(0, root.Bytes);
        Assert.Equal(unchanged, invalid);
        Assert.False(root.IsDisposed);
    }

    [Fact]
    public void AlphaOutputAdmissionRefusalAndCancellationReleasePrivateStorage()
    {
        var fixture = OriginalExif.Create(MetadataProfile.WebpLossyAlpha, 513, 259);
        var unchanged = fixture.Encoded.ToArray();
        var frameBytes = fixture.Width * fixture.Height * 4;
        using var root = new TestMemoryBudget(4 * 1024 * 1024);
        var options = new DecodeOptions { Limits = new() { MemoryBudget = root } };
        long outputGrowth = 0;
        root.Admitted = bytes =>
        {
            if (bytes >= frameBytes && outputGrowth == 0) outputGrowth = root.GrowthAttempts;
        };
        byte[] expected;
        using (var first = ImageCodec.Decode(fixture.Encoded, options))
        {
            expected = OriginalPixels.Bytes(first);
            Assert.Equal(frameBytes, root.Bytes);
        }
        Assert.True(outputGrowth > 0);
        Assert.Equal(0, root.Bytes);
        root.Admitted = null;
        root.RefuseAtGrowth = root.GrowthAttempts + outputGrowth;
        var refused = Assert.Throws<ImageLimitException>(() => ImageCodec.Decode(fixture.Encoded, options));
        Assert.Equal((ImageLimitKind.WorkingBytes, ImageLimitScope.SharedBudget), (refused.Limit, refused.Scope));
        Assert.Equal(1, root.Refusals);
        Assert.Equal(0, root.Bytes);
        root.RefuseAtGrowth = 0;
        using var cancellation = new CancellationTokenSource();
        root.Admitted = bytes =>
        {
            if (bytes >= frameBytes) cancellation.Cancel();
        };
        Assert.ThrowsAny<OperationCanceledException>(() => ImageCodec.Decode(fixture.Encoded, options, cancellation.Token));
        Assert.True(cancellation.IsCancellationRequested);
        Assert.Equal(0, root.Bytes);
        root.Admitted = null;
        using (var recovered = ImageCodec.Decode(fixture.Encoded, options))
        {
            Assert.Equal(expected, OriginalPixels.Bytes(recovered));
            Assert.Equal(frameBytes, root.Bytes);
        }
        Assert.Equal(0, root.Bytes);
        Assert.Equal(unchanged, fixture.Encoded);
        Assert.False(root.IsDisposed);
    }

    [Fact]
    public void CompressedAlphaFitsTwoRgbaFramesAndReleasesCapacityForRecovery()
    {
        var fixture = OriginalExif.Create(MetadataProfile.WebpLossyAlpha, 513, 259);
        var unchanged = fixture.Encoded.ToArray();
        var frameBytes = fixture.Width * fixture.Height * 4;
        using var root = new TestMemoryBudget(2L * frameBytes);
        var limits = new ImageLimits { MemoryBudget = root, MaxWorkingBytes = unchanged.Length + 2L * frameBytes };
        var options = new DecodeOptions { Limits = limits };
        using var first = ImageCodec.Decode(fixture.Encoded, options);
        var firstPeak = root.Peak;
        Assert.Equal(frameBytes, root.Bytes);
        for (var y = 0; y < first.Height; y++)
            for (var x = 0; x < first.Width; x++)
                Assert.Equal(fixture.Source![(y * first.Width + x) * 4 + 3], first.GetRow8(y)[x * 4 + 3]);
        var expected = OriginalPixels.Bytes(first);
        var refusal = Assert.Throws<ImageLimitException>(() => ImageCodec.Decode(fixture.Encoded, options));
        Assert.Equal((ImageLimitKind.WorkingBytes, ImageLimitScope.SharedBudget), (refusal.Limit, refusal.Scope));
        Assert.Equal(1, root.Refusals);
        Assert.Equal(frameBytes, root.Bytes);
        Assert.Equal(expected, OriginalPixels.Bytes(first));
        first.Dispose();
        Assert.Equal(0, root.Bytes);
        using var recovered = ImageCodec.Decode(fixture.Encoded, options);
        Assert.Equal(expected, OriginalPixels.Bytes(recovered));
        Assert.Equal(frameBytes, root.Bytes);
        recovered.Dispose();
        Assert.Equal(0, root.Bytes);
        Assert.False(root.IsDisposed);
        Assert.Equal(unchanged, fixture.Encoded);
        output.WriteLine(JsonSerializer.Serialize(new { fixture.Width, fixture.Height,
            encodedSha256 = Convert.ToHexStringLower(SHA256.HashData(fixture.Encoded)),
            firstPeakAdmittedBytes = firstPeak, allowedBytes = 2L * frameBytes,
            retainedBytes = frameBytes, terminalRefusals = root.Refusals }));
    }

    private static byte[] ReplaceAlpha(byte[] encoded, byte[] alpha)
    {
        using var output = new MemoryStream(); output.Write(encoded.AsSpan(0, 12));
        Span<byte> size = stackalloc byte[4];
        for (var offset = 12; offset < encoded.Length;)
        {
            var length = BinaryPrimitives.ReadInt32LittleEndian(encoded.AsSpan(offset + 4));
            var payload = encoded.AsSpan(offset, 4).SequenceEqual("ALPH"u8)
                ? alpha : encoded.AsSpan(offset + 8, length);
            output.Write(encoded.AsSpan(offset, 4));
            BinaryPrimitives.WriteInt32LittleEndian(size, payload.Length); output.Write(size);
            output.Write(payload);
            if ((payload.Length & 1) != 0) output.WriteByte(0);
            offset += 8 + length + (length & 1);
        }
        var result = output.ToArray();
        BinaryPrimitives.WriteInt32LittleEndian(result.AsSpan(4), result.Length - 8);
        return result;
    }
}
