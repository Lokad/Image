using System.Buffers.Binary;

namespace Lokad.Image.PublicApi.Tests;

public sealed class WebpPreviousRowTests
{
    [Theory] [InlineData(8)] [InlineData(16)]
    public void RepeatedNonconstantRowsCompressWithExactHiddenColorsAndIndependentOwners(int depth)
    {
        using var image = Rows(depth, 47, 101);
        var expected = Expected(image); var packet = Encode(image, 4);
        Assert.True(packet.Length < expected.Length / 8);
        // The transparent colors occur inside copied sequences, including the
        // overlapping tail of a reference longer than a row.
        Assert.NotEqual(0u, BinaryPrimitives.ReadUInt32LittleEndian(packet.AsSpan(21)) & (1u << 28));
        using var output = ImageCodec.Decode(packet);
        image.Dispose(); packet.AsSpan().Clear();
        AssertPixels(expected, output);
    }

    [Theory] [InlineData(1)] [InlineData(2)] [InlineData(17)]
    public void ChangedRowSuffixAndRunFollowingReferenceRemainExact(int width)
    {
        using var image = Rows(8, width, 5);
        var tail = image.GetRow8(3); tail[^4] = 133; tail[^3] = 247; tail[^2] = 29; tail[^1] = 255;
        var last = image.GetRow8(4);
        for (var x = 0; x < width; x++) tail[^4..].CopyTo(last.Slice(x * 4, 4));
        var expected = Expected(image);
        foreach (var effort in new[] { 0, 4, 6 })
        {
            using var output = ImageCodec.Decode(Encode(image, effort)); AssertPixels(expected, output);
        }
        AssertPixels(expected, image);
    }

    [Fact]
    public void PreviousRowEncodingPreservesExactAdmissionAndPreflightBeforeCallerWrites()
    {
        using var image = Rows(8, 47, 101); var reference = WebpAdmissionControl.Reference(image); var packet = reference.Packet;
        var limits = new ImageLimits { MaxWorkingBytes = reference.WorkingBytes, MaxOutputBytes = packet.Length };
        using var exact = new MemoryStream(); ImageCodec.EncodeWebp(image, exact, new() { Limits = limits });
        Assert.Equal(packet, exact.ToArray());
        foreach (var (bound, kind) in new[]
        {
            (limits with { MaxWorkingBytes = limits.MaxWorkingBytes - 1 }, ImageLimitKind.WorkingBytes),
            (limits with { MaxOutputBytes = packet.Length - 1 }, ImageLimitKind.OutputBytes),
            (limits with { MaxWork = 1 }, ImageLimitKind.Work)
        })
        {
            using var rejected = new MemoryStream();
            Assert.Equal(kind, Assert.Throws<ImageLimitException>(() => ImageCodec.EncodeWebp(image, rejected, new() { Limits = bound })).Limit);
            Assert.Equal(0, rejected.Length);
        }
    }

    internal static ImageBuffer Rows(int depth, int width, int height)
    {
        var image = ImageBuffer.Create(width, height, depth);
        for (var y = 0; y < height; y++)
        for (var x = 0; x < width; x++)
        for (var c = 0; c < 4; c++)
        {
            var sample = c == 3 ? (x % 7 == 0 ? 0 : 255) : (x * (c * 23 + 17) + c * 53) & 255;
            if (depth == 8) image.GetRow8(y)[x * 4 + c] = (byte)sample;
            else image.GetRow16(y)[x * 4 + c] = (ushort)Math.Clamp(sample * 257 + (x % 5 - 2) * 63, 0, 65535);
        }
        return image;
    }

    private static byte[] Encode(ImageBuffer image, int effort)
    { using var output = new MemoryStream(); ImageCodec.EncodeWebp(image, output, new() { Effort = effort }); return output.ToArray(); }
    private static byte[] Expected(ImageBuffer image)
    {
        var result = new byte[image.Width * image.Height * 4];
        for (var y = 0; y < image.Height; y++)
        for (var c = 0; c < image.Width * 4; c++) result[y * image.Width * 4 + c] = image.BitDepth == 8 ? image.GetRow8(y)[c] : (byte)((image.GetRow16(y)[c] + 128) / 257);
        return result;
    }
    private static void AssertPixels(byte[] expected, ImageBuffer image)
    {
        for (var y = 0; y < image.Height; y++) Assert.True(expected.AsSpan(y * image.Width * 4, image.Width * 4).SequenceEqual(image.GetRow8(y)));
    }
}
