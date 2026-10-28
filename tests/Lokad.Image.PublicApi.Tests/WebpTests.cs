using Lokad.Image.Tooling;

namespace Lokad.Image.PublicApi.Tests;

public sealed class WebpTests
{
    [Fact]
    public void ExtendedHeaderIgnoresReservedAndFutureFields()
    {
        var original = AuthoredWebp.OrientedPalette(false);
        var future = new byte[original.Length + 2];
        original.AsSpan(0, 30).CopyTo(future); original.AsSpan(30).CopyTo(future.AsSpan(32));
        System.Buffers.Binary.BinaryPrimitives.WriteInt32LittleEndian(future.AsSpan(4), future.Length - 8);
        System.Buffers.Binary.BinaryPrimitives.WriteInt32LittleEndian(future.AsSpan(16), 12);
        future[20] |= 0xC1; future[21] = 17; future[22] = 23; future[23] = 45; future[30] = 67; future[31] = 89;
        using var decoded = ImageCodec.Decode(future);
        Assert.Equal(AuthoredWebp.ExpectedPalette(17), decoded.GetRow8(0).ToArray());
        Assert.Equal(ImageOrientation.Rotate90, decoded.Orientation);
    }

    [Theory]
    [InlineData(false)] [InlineData(true)]
    public void ExtendedExifCanPrecedeOrFollowPixelsAndOrientationAppliesExactly(bool metadataFirst)
    {
        var encoded = AuthoredWebp.OrientedPalette(metadataFirst); var info = ImageCodec.Identify(encoded);
        Assert.Equal((9, 1, 1, 9), (info.Width, info.Height, info.OrientedWidth, info.OrientedHeight));
        using var decoded = ImageCodec.Decode(encoded, new DecodeOptions { ApplyOrientation = true });
        Assert.Equal((1, 9), (decoded.Width, decoded.Height)); Assert.Equal(ImageOrientation.Normal, decoded.Orientation);
        var expected = AuthoredWebp.ExpectedPalette(17);
        for (var y = 0; y < 9; y++) Assert.Equal(expected.AsSpan(y * 4, 4).ToArray(), decoded.GetRow8(y).ToArray());
    }

    [Theory]
    [InlineData(0)] [InlineData(1)] [InlineData(4)] [InlineData(6)]
    public void LosslessEncodePreservesAlphaAndHiddenRgbOnNonseekableOutput(int effort)
    {
        using var source = ImageBuffer.Create(19, 7, 8);
        for (var y = 0; y < source.Height; y++)
        for (var x = 0; x < source.Width; x++)
        {
            var row = source.GetRow8(y);
            row[x * 4] = (byte)(x * 13); row[x * 4 + 1] = (byte)(y * 31); row[x * 4 + 2] = (byte)(x + y);
            row[x * 4 + 3] = x % 3 == 0 ? (byte)0 : (byte)(x * 11);
        }
        using var encoded = new NonseekableOutput();
        ImageCodec.EncodeWebp(source, encoded, new WebpEncodeOptions { Effort = effort });
        Assert.True(encoded.CanWrite);
        using var input = new MemoryStream(encoded.ToArray()); using var decoded = ImageCodec.Decode(input);
        Assert.True(input.CanRead); Assert.Equal((19, 7, 8), (decoded.Width, decoded.Height, decoded.BitDepth));
        for (var y = 0; y < 7; y++) Assert.Equal(source.GetRow8(y).ToArray(), decoded.GetRow8(y).ToArray());
    }

    [Fact]
    public void SixteenBitInputRoundsOnceToEightBitSamples()
    {
        using var source = ImageBuffer.Create(1, 1, 16);
        new ushort[] { 65535, 128, 129, 0 }.CopyTo(source.GetRow16(0));
        using var encoded = new MemoryStream(); ImageCodec.EncodeWebp(source, encoded);
        using var decoded = ImageCodec.Decode(encoded.ToArray());
        Assert.Equal(new byte[] { 255, 0, 1, 0 }, decoded.GetRow8(0).ToArray());
    }

    [Fact]
    public void EncoderChecksOutputMemoryAndFormatLimits()
    {
        using var source = ImageBuffer.Create(1, 1, 8); using var output = new MemoryStream();
        Assert.Throws<ImageLimitException>(() => ImageCodec.EncodeWebp(source, output, new WebpEncodeOptions { Limits = new ImageLimits { MaxOutputBytes = 1 } }));
        Assert.Equal(0, output.Length);
        Assert.Throws<ImageLimitException>(() => ImageCodec.EncodeWebp(source, output, new WebpEncodeOptions { Limits = new ImageLimits { MaxWorkingBytes = 64 } }));
        Assert.Throws<OperationCanceledException>(() => ImageCodec.EncodeWebp(source, output, cancellationToken: new CancellationToken(true)));
        using var tooWide = ImageBuffer.Create(16385, 1, 8);
        Assert.Throws<ImageLimitException>(() => ImageCodec.EncodeWebp(tooWide, output));
    }

    public static IEnumerable<object[]> PredictorCases()
    {
        int[][] middle = [ [1,1,1,1], [3,4,4,5], [3,4,4,5], [4,3,4,4], [2,3,3,3], [3,3,4,4], [2,3,3,3],
            [3,4,4,5], [2,3,3,3], [3,3,4,4], [2,3,3,3], [3,4,4,5], [4,6,6,9], [3,4,4,5] ];
        for (var mode = 0; mode < 14; mode++) yield return [mode, middle[mode]];
    }

    [Theory]
    [MemberData(nameof(PredictorCases))]
    public void AllPredictorsHaveHandComputedPixelsIncludingRightEdge(int mode, int[] middle)
    {
        using var decoded = ImageCodec.Decode(AuthoredWebp.Predictor(mode));
        int[] expected = [1, 2, 3, 2, middle[0], middle[1], 3, middle[2], middle[3]];
        for (var y = 0; y < 3; y++)
        for (var x = 0; x < 3; x++)
        {
            var value = (byte)expected[y * 3 + x];
            Assert.Equal(new byte[] { value, value, value, 255 }, decoded.GetRow8(y).Slice(x * 4, 4).ToArray());
        }
    }

    [Fact]
    public void SignedCrossColorTransformAndModuloGreenAdditionPreserveSamples()
    {
        using var cross = ImageCodec.Decode(AuthoredWebp.ColorTransform());
        for (var y = 0; y < cross.Height; y++)
        for (var x = 0; x < cross.Width; x++) Assert.Equal(new byte[] { 76, 200, 200, 128 }, cross.GetRow8(y).Slice(x * 4, 4).ToArray());
        using var green = ImageCodec.Decode(AuthoredWebp.SubtractGreen());
        for (var y = 0; y < green.Height; y++)
        for (var x = 0; x < green.Width; x++) Assert.Equal(new byte[] { 14, 20, 9, 0 }, green.GetRow8(y).Slice(x * 4, 4).ToArray());
    }

    [Theory]
    [InlineData(1)] [InlineData(2)] [InlineData(3)] [InlineData(4)] [InlineData(16)] [InlineData(17)] [InlineData(256)]
    public void PalettePackingDeltaAccumulationAndMissingEntriesHaveExactPixels(int count)
    {
        foreach (var predictor in new[] { false, true })
        {
            using var decoded = ImageCodec.Decode(AuthoredWebp.Palette(count, predictor));
            Assert.Equal(AuthoredWebp.ExpectedPalette(count), decoded.GetRow8(0).ToArray());
        }
    }

    [Fact]
    public void CacheAndOverlappingBackwardCopyPreserveColors()
    {
        foreach (var encoded in new[] { AuthoredWebp.CachedColor(), AuthoredWebp.BackwardReference(), AuthoredWebp.DuplicateSimpleSymbols() })
        {
            using var decoded = ImageCodec.Decode(encoded);
            for (var x = 0; x < decoded.Width; x++) Assert.Equal(new byte[] { 20, 12, 30, 255 }, decoded.GetRow8(0).Slice(x * 4, 4).ToArray());
        }
        Assert.Throws<ImageDataException>(() => ImageCodec.Decode(AuthoredWebp.BackwardReference(missingLiteral: true)));
        Assert.Throws<ImageDataException>(() => ImageCodec.Decode(AuthoredWebp.BackwardReference(width: 3)));
    }

    [Fact]
    public void EntropyImageSelectsDifferentPrefixGroupsBySpatialTile()
    {
        using var decoded = ImageCodec.Decode(AuthoredWebp.EntropyGroups());
        for (var x = 0; x < 8; x++) Assert.Equal(x < 4 ? new byte[] { 10, 20, 30, 255 } : new byte[] { 40, 50, 60, 255 },
            decoded.GetRow8(0).Slice(x * 4, 4).ToArray());
    }

    [Fact]
    public void EntropyGroupsResumeAfterCopyAcrossTilesAndRows()
    {
        using var decoded = ImageCodec.Decode(AuthoredWebp.EntropyGroupsWithCrossingCopy());
        Assert.Equal((9, 6), (decoded.Width, decoded.Height));
        for (var y = 0; y < 6; y++)
        for (var x = 0; x < 9; x++)
        {
            var secondGroup = y * 9 + x >= 39 && ((y / 4 * 3 + x / 4) & 1) != 0;
            Assert.Equal(secondGroup ? new byte[] { 40, 50, 60, 255 } : new byte[] { 20, 12, 30, 255 },
                decoded.GetRow8(y).Slice(x * 4, 4).ToArray());
        }
    }

    [Theory]
    [InlineData(false)] [InlineData(true)]
    public void CodeLengthRepeatsAndTokenLimitsHaveExplicitPixels(bool defaultEight)
    {
        using var decoded = ImageCodec.Decode(AuthoredWebp.CodeLengthRepeats(defaultEight));
        for (var x = 0; x < decoded.Width; x++) Assert.Equal(new byte[] { 20, defaultEight ? (byte)128 : (byte)12, 30, 255 },
            decoded.GetRow8(0).Slice(x * 4, 4).ToArray());
    }

    [Fact]
    public void InvalidCacheSizesFailExplicitly()
    {
        Assert.Throws<ImageDataException>(() => ImageCodec.Decode(AuthoredWebp.InvalidCache(0)));
        Assert.Throws<ImageDataException>(() => ImageCodec.Decode(AuthoredWebp.InvalidCache(12)));
    }

    [Fact]
    public void CompressedMutationsAndTruncationsStayWithinDocumentedFailures()
    {
        var original = AuthoredWebp.Palette(17, thenPredictor: true);
        var options = new DecodeOptions { Limits = new ImageLimits { MaxDimension = 32, MaxPixels = 1024,
            MaxWorkingBytes = 128 * 1024} };
        for (var position = 25; position < original.Length; position++)
        foreach (var mask in new byte[] { 1, 0x80, 0xFF })
        {
            var mutated = original.ToArray(); mutated[position] ^= mask;
            try
            {
                using var decoded = ImageCodec.Decode(mutated, options);
                Assert.Equal((9, 1, 8), (decoded.Width, decoded.Height, decoded.BitDepth));
            }
            catch (ImageDataException) { }
            catch (ImageLimitException) { }
        }
        var payloadLength = System.Buffers.Binary.BinaryPrimitives.ReadInt32LittleEndian(original.AsSpan(16));
        for (var length = 5; length < payloadLength - 1; length++)
            Assert.Throws<ImageDataException>(() => ImageCodec.Decode(AuthoredWebp.WrapPayload(original.AsSpan(20, length).ToArray())));
        Assert.Throws<ImageLimitException>(() => ImageCodec.Decode(original, new DecodeOptions { Limits = new ImageLimits { MaxWorkingBytes = 64 } }));
        Assert.Throws<OperationCanceledException>(() => ImageCodec.Decode(original, cancellationToken: new CancellationToken(true)));
    }

    private sealed class NonseekableOutput : Stream
    {
        private readonly MemoryStream output = new();
        public byte[] ToArray() => output.ToArray();
        public override bool CanRead => false;
        public override bool CanSeek => false;
        public override bool CanWrite => output.CanWrite;
        public override long Length => throw new NotSupportedException();
        public override long Position { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }
        public override void Flush() => output.Flush();
        public override void Write(byte[] buffer, int offset, int count) => output.Write(buffer, offset, count);
        public override void Write(ReadOnlySpan<byte> buffer) => output.Write(buffer);
        public override void WriteByte(byte value) => output.WriteByte(value);
        public override int Read(byte[] buffer, int offset, int count) => throw new NotSupportedException();
        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
        public override void SetLength(long value) => throw new NotSupportedException();
        protected override void Dispose(bool disposing) { if (disposing) output.Dispose(); base.Dispose(disposing); }
    }
}
