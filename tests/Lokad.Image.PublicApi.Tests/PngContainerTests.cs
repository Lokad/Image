using System.Buffers.Binary;
using Lokad.Image.Tooling;

namespace Lokad.Image.PublicApi.Tests;

public sealed class PngContainerTests
{
    [Fact]
    public void RecomputedCrcDoesNotHideInvalidChunkOrderingOrPaletteSemantics()
    {
        var frame = new AuthoredPng.Frame("container", 3, 1, 2, 1, false);
        var chunks = Chunks(AuthoredPng.Build(frame));
        var candidates = new List<List<(string Type, byte[] Data)>>();
        var duplicateHeader = chunks.ToList(); duplicateHeader.Insert(1, chunks[0]); candidates.Add(duplicateHeader);
        var duplicatePalette = chunks.ToList(); duplicatePalette.Insert(2, chunks[1]); candidates.Add(duplicatePalette);
        var transparencyBeforePalette = chunks.ToList(); (transparencyBeforePalette[1], transparencyBeforePalette[2]) = (transparencyBeforePalette[2], transparencyBeforePalette[1]); candidates.Add(transparencyBeforePalette);
        var duplicateTransparency = chunks.ToList(); duplicateTransparency.Insert(3, chunks[2]); candidates.Add(duplicateTransparency);
        var excessPalette = chunks.ToList(); excessPalette[1] = ("PLTE", new byte[9]); candidates.Add(excessPalette);
        var incompletePalette = chunks.ToList(); incompletePalette[1] = ("PLTE", new byte[5]); candidates.Add(incompletePalette);
        var missingPalette = chunks.ToList(); missingPalette.RemoveAt(1); candidates.Add(missingPalette);
        var invalidIndex = chunks.ToList(); invalidIndex[1] = ("PLTE", new byte[3]); candidates.Add(invalidIndex);
        var excessTransparency = chunks.ToList(); excessTransparency[2] = ("tRNS", new byte[3]); candidates.Add(excessTransparency);
        var nonconsecutive = chunks.ToList(); nonconsecutive.Insert(4, ("vpAg", [])); candidates.Add(nonconsecutive);
        var lateTransparency = chunks.ToList(); lateTransparency.RemoveAt(2); lateTransparency.Insert(lateTransparency.Count - 1, chunks[2]); candidates.Add(lateTransparency);
        var nonemptyEnd = chunks.ToList(); nonemptyEnd[^1] = ("IEND", [0]); candidates.Add(nonemptyEnd);
        var missingEnd = chunks.ToList(); missingEnd.RemoveAt(missingEnd.Count - 1); candidates.Add(missingEnd);
        foreach (var candidate in candidates) Assert.Throws<ImageDataException>(() => ImageCodec.Decode(Encode(candidate)));
        Assert.Throws<ImageDataException>(() => ImageCodec.Decode([.. Encode(chunks), 0]));
    }

    [Fact]
    public void RecognizedExifMustPrecedeIdatAndCannotBeDuplicated()
    {
        var frame = new AuthoredPng.Frame("exif", 6, 8, 1, 1, false);
        byte[] exif = [73, 73, 42, 0, 8, 0, 0, 0, 1, 0, 18, 1, 3, 0, 1, 0, 0, 0, 6, 0, 0, 0, 0, 0, 0, 0];
        var compressed = AuthoredPng.StoredZlib(AuthoredPng.FilteredRows(frame));
        var duplicate = AuthoredPng.Wrap(frame, compressed, ("eXIf", exif), ("eXIf", exif));
        Assert.Throws<ImageDataException>(() => ImageCodec.Identify(duplicate));
        var chunks = Chunks(AuthoredPng.Build(frame)); chunks.Insert(chunks.Count - 1, ("eXIf", exif));
        Assert.Throws<ImageDataException>(() => ImageCodec.Identify(Encode(chunks)));
        Assert.Throws<ImageDataException>(() => ImageCodec.Decode(Encode(chunks)));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(4)]
    public void GrayscaleForbidsPalettesAndTransparencySamplesMustFitDepth(int color)
    {
        var frame = new AuthoredPng.Frame("gray", color, 8, 1, 1, false);
        var compressed = AuthoredPng.StoredZlib(AuthoredPng.FilteredRows(frame));
        Assert.Throws<ImageDataException>(() => ImageCodec.Decode(AuthoredPng.Wrap(frame, compressed, ("PLTE", new byte[3]))));
        var chunks = Chunks(AuthoredPng.Build(frame));
        if (color == 0) chunks[1] = ("tRNS", [1, 0]); // 256 does not fit an 8-bit gray sample.
        else chunks.Insert(1, ("tRNS", [0, 17])); // Gray-alpha already has an alpha channel.
        Assert.Throws<ImageDataException>(() => ImageCodec.Decode(Encode(chunks)));
    }

    [Fact]
    public void MidEncodeCancellationAndOutputLimitsLeaveCallerStreamsOpen()
    {
        using var source = OriginalPixels.Buffer(19, 17, 16);
        using var cancellation = new CancellationTokenSource(); using var output = new CancellingOutput(cancellation);
        Assert.Throws<OperationCanceledException>(() => ImageCodec.EncodePng(source, output, cancellationToken: cancellation.Token));
        Assert.True(output.CanWrite); Assert.True(output.ToArray().Length > 0);
        using var limited = new MemoryStream();
        Assert.Throws<ImageLimitException>(() => ImageCodec.EncodePng(source, limited, new PngEncodeOptions { Limits = new ImageLimits { MaxOutputBytes = 40 } }));
        Assert.True(limited.CanWrite); Assert.InRange(limited.Length, 1, 40);
    }

    [Fact]
    public void EncoderRejectsImplicitColorAndAlphaConversions()
    {
        using var source = ImageBuffer.Create(1, 1, 8); new byte[] { 12, 34, 56, 255 }.CopyTo(source.GetRow8(0));
        using var output = new MemoryStream();
        Assert.Throws<ArgumentException>(() => ImageCodec.EncodePng(source, output, new PngEncodeOptions { ColorType = PngColorType.Grayscale }));
        new byte[] { 12, 12, 12, 0 }.CopyTo(source.GetRow8(0));
        Assert.Throws<ArgumentException>(() => ImageCodec.EncodePng(source, output, new PngEncodeOptions { ColorType = PngColorType.Rgb }));
        Assert.True(output.CanWrite);
    }

    private sealed class CancellingOutput(CancellationTokenSource cancellation) : MemoryStream
    {
        public override void Write(ReadOnlySpan<byte> bytes) { base.Write(bytes); cancellation.Cancel(); }
    }

    private static List<(string Type, byte[] Data)> Chunks(byte[] png)
    {
        var result = new List<(string, byte[])>();
        for (var position = 8; position < png.Length;)
        {
            var length = BinaryPrimitives.ReadInt32BigEndian(png.AsSpan(position));
            result.Add((System.Text.Encoding.ASCII.GetString(png.AsSpan(position + 4, 4)), png.AsSpan(position + 8, length).ToArray()));
            position += length + 12;
        }
        return result;
    }

    private static byte[] Encode(IEnumerable<(string Type, byte[] Data)> chunks)
    {
        using var result = new MemoryStream(); result.Write(new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 });
        foreach (var (type, data) in chunks) AuthoredPng.Chunk(result, type, data);
        return result.ToArray();
    }
}
