using System.Buffers.Binary;
using Lokad.Image.Tooling;

namespace Lokad.Image.PublicApi.Tests;

public sealed class SourceMetadataTests
{
    [Theory]
    [InlineData(0, 1, false)] [InlineData(0, 2, false)] [InlineData(0, 4, true)] [InlineData(0, 16, true)]
    [InlineData(2, 8, false)] [InlineData(2, 16, true)]
    [InlineData(3, 1, false)] [InlineData(3, 2, true)] [InlineData(3, 4, false)] [InlineData(3, 8, true)]
    [InlineData(4, 8, false)] [InlineData(4, 16, false)] [InlineData(6, 8, false)] [InlineData(6, 16, false)]
    public async Task PngReportsEncodedModelPrecisionAndTransparency(int color, int depth, bool transparency)
    {
        var frame = new AuthoredPng.Frame("source-representation", color, depth, 9, 7, false, transparency);
        var data = AuthoredPng.Build(frame);
        var expectedColor = color is 0 or 4 ? SourceColorModel.Grayscale : color == 3 ? SourceColorModel.Indexed : SourceColorModel.Rgb;
        var expectedAlpha = color is 4 or 6 ? SourceAlphaRepresentation.Channel : !transparency ? SourceAlphaRepresentation.None
            : color == 3 ? SourceAlphaRepresentation.PaletteTransparency : SourceAlphaRepresentation.ColorKeyTransparency;
        var count = color switch { 0 or 3 => 1, 2 => 3, 4 => 2, _ => 4 };
        await CheckEveryApi(data, expectedColor, expectedAlpha, depth, count, expectedAlpha != SourceAlphaRepresentation.None);
        using var decoded = ImageCodec.Decode(data);
        Assert.Equal(depth == 16 ? 16 : 8, decoded.BitDepth); // Source packed/index precision is not decoded RGBA precision.
    }

    [Theory]
    [InlineData(1, -1, SourceColorModel.Grayscale)]
    [InlineData(3, -1, SourceColorModel.Unknown)] [InlineData(3, 0, SourceColorModel.Rgb)] [InlineData(3, 1, SourceColorModel.YCbCr)]
    [InlineData(4, -1, SourceColorModel.Unknown)] [InlineData(4, 0, SourceColorModel.Cmyk)] [InlineData(4, 2, SourceColorModel.Ycck)]
    public async Task JpegDistinguishesDeclaredModelsAndUnlabelledComponents(int count, int adobe, SourceColorModel expected)
    {
        var data = AuthoredJpegCompatibility.Create(9, 7, Enumerable.Repeat((byte)123, count).ToArray(), adobe: adobe < 0 ? null : adobe);
        await CheckEveryApi(data, expected, SourceAlphaRepresentation.None, 8, count, false);
    }

    [Theory] [InlineData(3, SourceColorModel.Rgb)] [InlineData(4, SourceColorModel.Cmyk)]
    public async Task JpegConventionalComponentNamesRemainSourceMetadata(int count, SourceColorModel expected)
    {
        var data = AuthoredJpegCompatibility.Create(9, 7, Enumerable.Repeat((byte)123, count).ToArray());
        ReadOnlySpan<byte> names = count == 3 ? "RGB"u8 : "CMYK"u8;
        var frame = data.AsSpan().IndexOf(new byte[] { 255, 192 }); Assert.True(frame > 0);
        for (var i = 0; i < count; i++) data[frame + 10 + i * 3] = names[i];
        // SOS selectors must agree with the authored frame component names.
        var scan = data.AsSpan().IndexOf(new byte[] { 255, 218 }); Assert.True(scan > frame);
        for (var i = 0; i < count; i++) data[scan + 5 + i * 2] = names[i];
        await CheckEveryApi(data, expected, SourceAlphaRepresentation.None, 8, count, false);
    }

    [Fact]
    public async Task JfifDeclaresYcbcrWithoutRequiringPixelDecoding()
    {
        var data = AuthoredJpegCompatibility.Create(9, 7, [123,124,125]);
        byte[] marker = [255,224,0,16,74,70,73,70,0,1,2,0,0,1,0,1,0,0];
        await CheckEveryApi([.. data.AsSpan(0, 2), .. marker, .. data.AsSpan(2)], SourceColorModel.YCbCr, SourceAlphaRepresentation.None, 8, 3, false);
    }

    [Theory] [InlineData(3)] [InlineData(4)]
    public async Task UnrecognizedAdobeTransformIsUnknownDuringContainerInspection(int count)
    {
        var data = AuthoredJpegCompatibility.Create(9, 7, Enumerable.Repeat((byte)123, count).ToArray(), adobe: 255);
        await CheckEveryApi(data, SourceColorModel.Unknown, SourceAlphaRepresentation.None, 8, count, false);
        var unsupported = Assert.Throws<UnsupportedImageException>(() => ImageCodec.Decode(data));
        Assert.Equal(UnsupportedImageFeature.ColorTransform, unsupported.Feature);
    }

    [Fact]
    public async Task ConflictingAdobeDeclarationsDoNotPretendToEstablishAColorModel()
    {
        var data = AuthoredJpegCompatibility.Create(9, 7, [123,124,125], adobe: 0);
        byte[] other = [255,238,0,14,65,100,111,98,101,0,100,0,0,0,0,1];
        await CheckEveryApi([.. data.AsSpan(0,2), .. other, .. data.AsSpan(2)], SourceColorModel.Unknown, SourceAlphaRepresentation.None, 8, 3, false);
    }

    [Fact]
    public async Task OptionalRgbPaletteDoesNotChangeTheEncodedColorModel()
    {
        var frame = new AuthoredPng.Frame("RGB-with-PLTE", 2, 8, 9, 7, false, false);
        var data = AuthoredPng.Wrap(frame, AuthoredPng.StoredZlib(AuthoredPng.FilteredRows(frame)), ("PLTE", new byte[] {12,34,56}));
        await CheckEveryApi(data, SourceColorModel.Rgb, SourceAlphaRepresentation.None, 8, 3, false);
    }

    [Fact]
    public async Task SourceMetadataDoesNotRequireValidCompressedPixels()
    {
        var frame = new AuthoredPng.Frame("opaque-compressed-payload", 3, 4, 9, 7, false, true);
        var data = AuthoredPng.Wrap(frame, [0xA7]); // Original valid container/CRCs, intentionally invalid zlib.
        await CheckEveryApi(data, SourceColorModel.Indexed, SourceAlphaRepresentation.PaletteTransparency, 4, 1, true);
        Assert.Throws<ImageDataException>(() => ImageCodec.Decode(data));
    }

    [Theory] [InlineData(false, false)] [InlineData(false, true)] [InlineData(true, false)] [InlineData(true, true)]
    public async Task WebpSeparatesLogicalAlphaCodingFromItsHeaderHint(bool lossless, bool transparent)
    {
        using var image = OriginalPixels.Buffer(9, 7, 8, opaque: !transparent);
        using var target = new MemoryStream(); ImageCodec.EncodeWebp(image, target, new() { Lossless = lossless });
        var data = target.ToArray();
        if (lossless)
        {
            var chunk = data.AsSpan().IndexOf("VP8L"u8); Assert.True(chunk >= 12);
            data[chunk + 12] &= 0xEF; // Clear the nonbinding alpha hint, retaining the original lossless alpha samples.
        }
        await CheckEveryApi(data, lossless ? SourceColorModel.Rgb : SourceColorModel.YCbCr,
            lossless || transparent ? SourceAlphaRepresentation.Channel : SourceAlphaRepresentation.None, 8,
            lossless || transparent ? 4 : 3, !lossless && transparent);
        if (lossless && transparent)
        {
            using var decoded = ImageCodec.Decode(data);
            Assert.Equal(OriginalPixels.Bytes(image), OriginalPixels.Bytes(decoded));
        }
    }

    [Fact]
    public async Task OpaquePaletteTransparencyStillReportsTheEncodedTable()
    {
        var frame = new AuthoredPng.Frame("opaque-tRNS", 3, 1, 9, 7, false, false);
        var data = AuthoredPng.Wrap(frame, AuthoredPng.StoredZlib(AuthoredPng.FilteredRows(frame)), ("tRNS", new byte[] {255,255}));
        await CheckEveryApi(data, SourceColorModel.Indexed, SourceAlphaRepresentation.PaletteTransparency, 1, 1, true);
        using var decoded = ImageCodec.Decode(data);
        for (var y = 0; y < 7; y++) for (var x = 0; x < 9; x++) Assert.Equal(255, decoded.GetRow8(y)[x * 4 + 3]);
    }

    [Fact]
    public async Task RefusalAndCancellationReleaseHeaderWorkspaceAndPermitRecovery()
    {
        var data = AuthoredJpegCompatibility.Create(9, 7, [123,124,125,126], adobe: 2);
        using var root = new TestMemoryBudget(8448); var limits = new ImageLimits { MemoryBudget = root, MaxWorkingBytes = 8448 };
        using var callerScope = root.CreateScope(); Assert.True(callerScope.TryReserve(8448, out var held));
        using var denied = new FragmentedInput(data, true); var attempts = root.Attempts;
        var refusal = await Assert.ThrowsAsync<ImageLimitException>(() => ImageCodec.InspectHeadersAsync(denied, limits).AsTask());
        Assert.Equal((ImageLimitKind.WorkingBytes, ImageLimitScope.SharedBudget), (refusal.Limit, refusal.Scope));
        Assert.Equal(attempts + 1, root.Attempts); Assert.Equal(0, denied.BytesRead); Assert.Equal(8448, root.Bytes);
        held!.Dispose(); using var token = new CancellationTokenSource(); using var canceled = new FragmentedInput(data, true);
        canceled.AfterRead = () => { if (canceled.BytesRead >= 100) token.Cancel(); };
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => ImageCodec.InspectHeadersAsync(canceled, limits, token.Token).AsTask());
        Assert.Equal(0, root.Bytes); Assert.False(root.IsDisposed); Assert.False(canceled.WasDisposed);
        using var recovery = new FragmentedInput(data, true);
        Assert.Equal(SourceColorModel.Ycck, (await ImageCodec.InspectHeadersAsync(recovery, limits)).SourceColor);
        Assert.Equal(0, root.Bytes); Assert.False(root.IsDisposed);
    }

    [Fact]
    public void SixArgumentConstructionAndDeconstructionKeepUnknownDescriptors()
    {
        var info = new ImageInfo(ImageFormat.Png, 9, 7, 1, true, ImageOrientation.Rotate90);
        var (format,width,height,depth,alpha,orientation) = info;
        Assert.Equal((ImageFormat.Png,9,7,1,true,ImageOrientation.Rotate90), (format,width,height,depth,alpha,orientation));
        Assert.Equal(SourceColorModel.Unknown, info.SourceColor); Assert.Equal(SourceAlphaRepresentation.Unknown, info.SourceAlpha); Assert.Equal(0, info.SourceComponentCount);
        Assert.NotEqual(info, info with { SourceColor = SourceColorModel.Indexed });
    }

    private static async Task CheckEveryApi(byte[] data, SourceColorModel color, SourceAlphaRepresentation alpha, int depth, int count, bool hasAlpha)
    {
        using var root = new TestMemoryBudget(4 * 1024 * 1024); var limits = new ImageLimits { MemoryBudget = root, MaxWork = 1_000_000 };
        var copy = data.ToArray(); var expected = ImageCodec.Identify(data, limits);
        Assert.Equal((color,alpha,depth,count,hasAlpha), (expected.SourceColor,expected.SourceAlpha,expected.BitDepth,(int)expected.SourceComponentCount,expected.HasAlpha));
        using var sync = new FragmentedInput(data, false); Assert.Equal(expected, ImageCodec.Identify(sync, limits));
        using var asynchronous = new FragmentedInput(data, true); Assert.Equal(expected, await ImageCodec.IdentifyAsync(asynchronous, limits));
        using var header = new FragmentedInput(data, false); Assert.Equal(expected, ImageCodec.InspectHeaders(header, limits with { MaxWorkingBytes = 8448 }));
        using var asyncHeader = new FragmentedInput(data, true); Assert.Equal(expected, await ImageCodec.InspectHeadersAsync(asyncHeader, limits with { MaxWorkingBytes = 8448 }));
        var budget = new ImageBudget(1 << 20, 10_000_000); Assert.Equal(expected, budget.Identify(data, limits with { MaxWorkingBytes = 1 << 20 }));
        using var budgetHeader = new FragmentedInput(data, true); Assert.Equal(expected, await budget.InspectHeadersAsync(budgetHeader, limits with { MaxWorkingBytes = 8448 }));
        Assert.Equal(copy, data); Assert.Equal(0, root.Bytes); Assert.False(root.IsDisposed);
        foreach (var input in new[] {sync,asynchronous,header,asyncHeader,budgetHeader}) Assert.False(input.WasDisposed);
    }

    private sealed class FragmentedInput(byte[] bytes, bool asynchronous) : Stream
    {
        private int position;
        public int BytesRead => position;
        public bool WasDisposed;
        public Action? AfterRead;
        public override bool CanRead => !WasDisposed;
        public override bool CanSeek => false;
        public override bool CanWrite => false;
        public override long Length => throw new NotSupportedException();
        public override long Position { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }
        private int Copy(Span<byte> target)
        {
            var n = Math.Min(7, Math.Min(target.Length, bytes.Length - position)); bytes.AsSpan(position, n).CopyTo(target); position += n; AfterRead?.Invoke(); return n;
        }
        public override int Read(byte[] buffer, int offset, int count) => Read(buffer.AsSpan(offset, count));
        public override int Read(Span<byte> buffer) => asynchronous ? throw new InvalidOperationException("Synchronous read during async inspection.") : Copy(buffer);
        public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken token = default)
        { token.ThrowIfCancellationRequested(); return ValueTask.FromResult(Copy(buffer.Span)); }
        public override void Flush() => throw new NotSupportedException();
        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
        public override void SetLength(long value) => throw new NotSupportedException();
        public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
        protected override void Dispose(bool disposing) { WasDisposed = true; base.Dispose(disposing); }
    }
}
