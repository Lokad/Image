using Lokad.Image.Internal;
using Lokad.Image.Tooling;

namespace Lokad.Image.Tests;

public sealed class WebpPaletteOwnershipTests
{
    private const int Width = 513, Height = 67, FrameBytes = Width * Height * 4;

    // Original deterministic samples; construct an explicit palette-first packet
    // through the production coding components, independently checking its decoded
    // samples against the source. No external implementation/fixture is involved.
    private static (byte[] Packet, byte[] Source) Input(int colors)
    {
        var source = PalettePackingInput.Create(Width, Height, colors);
        using var context = new OperationContext(ImageLimits.Default, default);
        var pixels = Vp8lReferencePredictorTests.Working(source, false, context);
        using var palette = Vp8lReferencePalette.SelectAndPack(pixels, Width, colors, Vp8lPaletteOrder.Sorted, context);
        using var codes = Vp8lImageCode.Auxiliary(palette.EncodedDeltas, palette.EncodedColorCount, 20, true, context);
        var packed = pixels.AsSpan(0, palette.PackedPixelCount);
        using var matches = Vp8lReferenceMatcher.Build(packed, palette.CodedWidth, 75, true, context);
        using var main = Vp8lImageCode.TakeMain(Vp8lReferenceLz.Greedy(packed, matches.Values, context),
            palette.CodedWidth, Height, 7, true, context);
        var prefix = new Vp8lFramePrefix(Width, Height, true) { Palette = codes, PaletteEntries = palette.EncodedColorCount };
        using var output = new MemoryStream();
        Vp8lReferenceFrame.Write(output, prefix, main, Vp8lReferenceFrame.CountPayload(prefix, main, context), context);
        return (output.ToArray(), source);
    }

    [Theory]
    [InlineData(2)] [InlineData(4)] [InlineData(16)] [InlineData(17)] [InlineData(256)]
    public void FinalPaletteKeepsExactSamplesAndIndependentAdmittedOwners(int colors)
    {
        var (packet, source) = Input(colors); var unchanged = packet.ToArray();
        using var root = new TestMemoryBudget(4 * FrameBytes);
        var options = new DecodeOptions { Limits = new() { MemoryBudget = root } };
        using var first = ImageCodec.Decode(packet, options);
        using var survivor = ImageCodec.Decode(packet, options);
        Assert.Equal(source, OriginalPixels.Bytes(first)); Assert.Equal(source, OriginalPixels.Bytes(survivor));
        Assert.Equal(2 * FrameBytes, root.Bytes);
        first.GetRow8(0).Fill(0xA7); first.Dispose();
        Assert.Equal(FrameBytes, root.Bytes); Assert.Equal(source, OriginalPixels.Bytes(survivor));
        using var third = ImageCodec.Decode(packet, options);
        Assert.Equal(source, OriginalPixels.Bytes(third)); Assert.Equal(unchanged, packet);
        survivor.Dispose(); third.Dispose(); Assert.Equal(0, root.Bytes); Assert.False(root.IsDisposed);
    }

    [Fact]
    public void PackedSourceAndResultFitWithoutASecondFullFrameAndRefusalCleansUp()
    {
        var (packet, source) = Input(16);
        // Two indices per byte, represented by one word per packed byte. Include
        // coarse parser/table headroom; this stays below two full frames.
        const int capacity = FrameBytes + ((Width + 1) / 2) * Height * 4 + 32768;
        Assert.True(capacity < 2 * FrameBytes);
        using var root = new TestMemoryBudget(capacity);
        var options = new DecodeOptions { Limits = new() { MemoryBudget = root } };
        using (var first = ImageCodec.Decode(packet, options))
        {
            Assert.Equal(source, OriginalPixels.Bytes(first)); Assert.Equal(FrameBytes, root.Bytes);
            var refusals = root.Refusals;
            var error = Assert.Throws<ImageLimitException>(() => ImageCodec.Decode(packet, options));
            Assert.Equal(ImageLimitScope.SharedBudget, error.Scope); Assert.Equal(refusals + 1, root.Refusals);
            Assert.Equal(FrameBytes, root.Bytes); Assert.Equal(source, OriginalPixels.Bytes(first));
        }
        Assert.Equal(0, root.Bytes);
        using var recovered = ImageCodec.Decode(packet, options);
        Assert.Equal(source, OriginalPixels.Bytes(recovered)); recovered.Dispose(); Assert.Equal(0, root.Bytes);
    }

    [Fact]
    public void CancellationAtOutputAdmissionReleasesBothOwnersAndCanRecover()
    {
        var (packet, source) = Input(16); var unchanged = packet.ToArray();
        using var root = new TestMemoryBudget(4 * FrameBytes);
        using var cancellation = new CancellationTokenSource();
        root.Admitted = bytes => { if (bytes >= FrameBytes) cancellation.Cancel(); };
        var options = new DecodeOptions { Limits = new() { MemoryBudget = root } };
        Assert.ThrowsAny<OperationCanceledException>(() => ImageCodec.Decode(packet, options, cancellation.Token));
        Assert.Equal(0, root.Bytes); Assert.False(root.IsDisposed); root.Admitted = null;
        using var recovered = ImageCodec.Decode(packet, options);
        Assert.Equal(source, OriginalPixels.Bytes(recovered)); recovered.Dispose();
        Assert.Equal(0, root.Bytes); Assert.Equal(unchanged, packet);
    }

    [Fact]
    public async Task AsyncPaletteResultSurvivesInputAndOperationDisposal()
    {
        var (packet, source) = Input(16);
        using var root = new TestMemoryBudget(4 * FrameBytes);
        ImageBuffer survivor;
        using (var input = new MemoryStream(packet, writable: false))
        {
            survivor = await ImageCodec.DecodeAsync(input, new() { Limits = new() { MemoryBudget = root } });
            Assert.True(input.CanRead);
        }
        using (survivor)
        {
            packet.AsSpan().Clear(); Assert.Equal(source, OriginalPixels.Bytes(survivor));
            Assert.Equal(FrameBytes, root.Bytes);
        }
        Assert.Equal(0, root.Bytes); Assert.False(root.IsDisposed);
    }
}
