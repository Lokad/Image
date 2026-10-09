using System.Buffers.Binary;
using System.IO.Compression;
using Lokad.Image.Internal;

namespace Lokad.Image.Tests;

public sealed class PngStructureTests
{
    [Theory]
    [InlineData(1)] [InlineData(2)] [InlineData(4)] [InlineData(8)]
    public void PackedGrayAndPaletteTransparencyDecode(int depth)
    {
        var count = 1 << depth;
        var row = new byte[1 + (count * depth + 7) / 8];
        for (var i = 0; i < count; i++) row[1 + i * depth / 8] |= (byte)(i << (8 - depth - i * depth % 8));
        var transparency = new byte[2]; BinaryPrimitives.WriteUInt16BigEndian(transparency, (ushort)(count - 1));
        using var gray = ImageCodec.Decode(Build(count, 1, depth, 0, row, [("tRNS", transparency)]));
        for (var i = 0; i < count; i++)
        {
            Assert.Equal(i * 255 / (count - 1), gray.GetRow8(0)[i * 4]);
            Assert.Equal(i == count - 1 ? 0 : 255, gray.GetRow8(0)[i * 4 + 3]);
        }
        var palette = new byte[count * 3]; var alpha = new byte[count];
        for (var i = 0; i < count; i++) { palette[i * 3] = (byte)i; palette[i * 3 + 2] = (byte)(255 - i); alpha[i] = (byte)i; }
        using var indexed = ImageCodec.Decode(Build(count, 1, depth, 3, row, [("PLTE", palette), ("tRNS", alpha)]));
        for (var i = 0; i < count; i++)
            Assert.Equal(new byte[] { (byte)i, 0, (byte)(255 - i), (byte)i }, indexed.GetRow8(0).Slice(i * 4, 4).ToArray());
    }

    [Theory]
    [InlineData(1, new byte[] { 1, 2, 3, 4, 5, 6 })]
    [InlineData(2, new byte[] { 3, 2, 1, 6, 5, 4 })]
    [InlineData(3, new byte[] { 6, 5, 4, 3, 2, 1 })]
    [InlineData(4, new byte[] { 4, 5, 6, 1, 2, 3 })]
    [InlineData(5, new byte[] { 1, 4, 2, 5, 3, 6 })]
    [InlineData(6, new byte[] { 4, 1, 5, 2, 6, 3 })]
    [InlineData(7, new byte[] { 6, 3, 5, 2, 4, 1 })]
    [InlineData(8, new byte[] { 3, 6, 2, 5, 1, 4 })]
    public void ExifOrientationUsesHandComputedPixelPermutations(int orientation, byte[] expected)
    {
        // Original little-endian TIFF with one inline SHORT orientation entry.
        byte[] exif = [73, 73, 42, 0, 8, 0, 0, 0, 1, 0, 18, 1, 3, 0, 1, 0, 0, 0, (byte)orientation, 0, 0, 0, 0, 0, 0, 0];
        var encoded = Build(3, 2, 8, 0, [0, 1, 2, 3, 0, 4, 5, 6], [("eXIf", exif)]);
        var info = ImageCodec.Identify(encoded);
        Assert.Equal((ImageOrientation)orientation, info.Orientation);
        Assert.Equal(orientation >= 5 ? 2 : 3, info.OrientedWidth);
        using var decoded = ImageCodec.Decode(encoded, new DecodeOptions { ApplyOrientation = true });
        Assert.Equal(ImageOrientation.Normal, decoded.Orientation);
        var actual = new List<byte>();
        for (var y = 0; y < decoded.Height; y++)
        for (var x = 0; x < decoded.Width; x++) actual.Add(decoded.GetRow8(y)[x * 4]);
        Assert.Equal(expected, actual);
        using var twice = ImageTransforms.AutoOrient(decoded);
        for (var y = 0; y < twice.Height; y++) Assert.Equal(decoded.GetRow8(y).ToArray(), twice.GetRow8(y).ToArray());
    }

    [Fact]
    public void StructuralErrorsAndAnimationHaveDistinctFailures()
    {
        Assert.Throws<UnsupportedImageException>(() => ImageCodec.Decode(Build(1, 1, 8, 0, [0, 1], [("acTL", new byte[8])])));
        Assert.Throws<ImageDataException>(() => ImageCodec.Decode(Build(1, 1, 8, 3, [0, 1], [("PLTE", new byte[3])])));
        Assert.Throws<ImageDataException>(() => ImageCodec.Decode(Build(1, 1, 8, 0, [5, 1], [])));
        Assert.Throws<ImageDataException>(() => ImageCodec.Decode(Build(1, 1, 8, 0, [0, 1, 2], [])));
        Assert.Throws<ImageDataException>(() => ImageCodec.Decode(Build(1, 1, 8, 0, [0], [])));
        Assert.Throws<ImageLimitException>(() => ImageCodec.Decode(Build(1, 1, 8, 0, [0, 1], [("iCCP", new byte[100])]),
            new DecodeOptions { Limits = new ImageLimits { MaxMetadataBytes = 99 } }));
        Assert.Throws<ImageDataException>(() => ImageCodec.Decode(Build(1, 1, 8, 0, [0, 1], [("eXIf", new byte[8])])));
    }

    [Fact]
    public void SkippedMetadataDoesNotGetDecompressed()
    {
        using var image = ImageCodec.Decode(Build(1, 1, 8, 0, [0, 123], [("iCCP", new byte[] { 1, 2, 3, 4 })]));
        Assert.Equal(123, image.GetRow8(0)[0]);
    }

    [Fact]
    public void SeededContainerMutationsStayInsideDocumentedFailureBoundary()
    {
        var valid = Build(3, 2, 8, 0, [0, 1, 2, 3, 0, 4, 5, 6], []);
        var random = new Random(73191);
        for (var i = 0; i < 256; i++)
        {
            var candidate = valid.ToArray();
            candidate[random.Next(candidate.Length)] ^= (byte)(1 << random.Next(8));
            var exception = Record.Exception(() => { using var result = ImageCodec.Decode(candidate); });
            Assert.True(exception is ImageDataException or UnsupportedImageException or ImageLimitException,
                exception?.ToString() ?? "Mutation unexpectedly accepted.");
        }
    }

    [Fact]
    public void ExtremeDimensionsCannotOverflowAllocationArithmetic()
    {
        Assert.Throws<ImageLimitException>(() => ImageBuffer.Create(int.MaxValue, int.MaxValue, 16,
            new ImageLimits { MaxDimension = int.MaxValue, MaxPixels = long.MaxValue, MaxWorkingBytes = long.MaxValue }));
    }

    [Theory]
    [InlineData(0)] [InlineData(3)]
    public void TinyPackedAdam7SkipsEmptyPasses(int color)
    {
        var chunks = color == 3 ? new[] { ("PLTE", new byte[] { 20, 30, 40, 200, 210, 220 }) } : Array.Empty<(string, byte[])>();
        // A 2x2 image uses only passes 1, 6, 7: [0], [1], [1,0].
        using var image = ImageCodec.Decode(Build(2, 2, 1, color, [0, 0, 0, 128, 0, 128], chunks, true));
        Assert.Equal(color == 0 ? 0 : 20, image.GetRow8(0)[0]);
        Assert.Equal(color == 0 ? 255 : 200, image.GetRow8(0)[4]);
        Assert.Equal(color == 0 ? 255 : 200, image.GetRow8(1)[0]);
        Assert.Equal(color == 0 ? 0 : 20, image.GetRow8(1)[4]);
        using var single = ImageCodec.Decode(Build(1, 1, 1, color, [0, 128], chunks, true));
        Assert.Equal(color == 0 ? 255 : 200, single.GetRow8(0)[0]);
    }

    [Fact]
    public void SixteenBitTruecolorTransparencyPreservesHiddenRgb()
    {
        byte[] rgb = [0x30, 0x39, 0, 1, 0xFF, 0xFE];
        using var image = ImageCodec.Decode(Build(1, 1, 16, 2, new byte[] { 0 }.Concat(rgb).ToArray(), [("tRNS", rgb)]));
        Assert.Equal(new ushort[] { 12345, 1, 65534, 0 }, image.GetRow16(0).ToArray());
    }

    private static byte[] Build(int width, int height, int depth, int color, byte[] raw, (string Type, byte[] Payload)[] chunks, bool interlaced = false)
    {
        using var output = new MemoryStream(); output.Write(PngCodec.Signature);
        var header = new byte[13]; BinaryPrimitives.WriteInt32BigEndian(header, width); BinaryPrimitives.WriteInt32BigEndian(header.AsSpan(4), height);
        header[8] = (byte)depth; header[9] = (byte)color; header[12] = interlaced ? (byte)1 : (byte)0;
        PngCodec.WriteChunk(output, "IHDR"u8, header);
        foreach (var chunk in chunks) PngCodec.WriteChunk(output, System.Text.Encoding.ASCII.GetBytes(chunk.Type), chunk.Payload);
        using var encoded = new MemoryStream();
        using (var zlib = new ZLibStream(encoded, CompressionLevel.Optimal, true)) zlib.Write(raw);
        PngCodec.WriteChunk(output, "IDAT"u8, encoded.ToArray()); PngCodec.WriteChunk(output, "IEND"u8, []);
        return output.ToArray();
    }
}
