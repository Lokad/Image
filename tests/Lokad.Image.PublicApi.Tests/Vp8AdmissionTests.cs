using Lokad.Image.Tooling;

namespace Lokad.Image.PublicApi.Tests;

public sealed class Vp8AdmissionTests
{
    public static IEnumerable<object[]> DecodeProfiles()
    {
        foreach (var size in new[] { (1, 1), (17, 19), (65, 33) })
        foreach (var alpha in Enumerable.Range(-1, 9))
        foreach (var orient in new[] { false, true }) yield return [size.Item1, size.Item2, alpha, orient];
    }

    [Theory]
    [MemberData(nameof(DecodeProfiles))]
    public void ReconstructionAlphaAndOrientationShareTheDecodeAdmission(int width, int height, int alphaProfile, bool orient)
    {
        using var source = OriginalPixels.Buffer(width, height, 8, opaque: true);
        using var output = new MemoryStream();
        ImageCodec.EncodeWebp(source, output, new WebpEncodeOptions { Lossless = false, Quality = 85, Effort = 4 });
        var encoded = output.ToArray();
        using var opaque = ImageCodec.Decode(encoded);
        var expected = OriginalPixels.Bytes(opaque);
        if (alphaProfile >= 0)
        {
            encoded = AuthoredAlpha.Wrap(encoded, width, height, alphaProfile % 4, compressed: alphaProfile >= 4);
            var alpha = AuthoredAlpha.Samples(width, height);
            for (var pixel = 0; pixel < alpha.Length; pixel++) expected[pixel * 4 + 3] = alpha[pixel];
        }
        if (orient)
        {
            encoded = OriginalExif.Attach(new MetadataFixture(encoded, null, width, height, MetadataProfile.WebpLossyAlpha, AlphaPresent: alphaProfile >= 0),
                ImageOrientation.Rotate90, bigEndian: true);
            expected = OriginalExif.Permute(expected, width, height, 8, ImageOrientation.Rotate90);
        }
        var unchanged = encoded.ToArray();
        var columns = (width + 15) / 16; var rows = (height + 15) / 16;
        var pixels = (long)width * height;
        var planes = (long)columns * rows * 384 + 128;
        var metadata = columns * rows * 20L + columns * 13L + 256;
        var peak = 8192 + metadata + planes;
        var alphaBytes = alphaProfile < 0 ? 0 : pixels + 32;
        peak = Math.Max(peak, planes + alphaBytes + pixels * 4);
        if (alphaProfile >= 4)
        {
            // The owned alpha encoder's fixed full-alphabet trees, main-raster
            // allocation and RGBA conversion have independently declared peaks.
            var lossless = Math.Max(8944, Math.Max(8904 + pixels * 4, 544 + pixels * 8));
            peak = Math.Max(peak, planes + alphaBytes + lossless);
        }
        if (orient) peak = Math.Max(peak, pixels * 8);
        var options = new DecodeOptions { ApplyOrientation = orient, Limits = new ImageLimits { MaxWorkingBytes = encoded.Length + peak } };
        using var actual = ImageCodec.Decode(encoded, options);
        Assert.Equal(expected, OriginalPixels.Bytes(actual));
        Assert.Equal(orient ? (height, width) : (width, height), (actual.Width, actual.Height));
        Assert.Equal(ImageOrientation.Normal, actual.Orientation);
        Assert.Throws<ImageLimitException>(() => ImageCodec.Decode(encoded, options with
        { Limits = options.Limits with { MaxWorkingBytes = options.Limits.MaxWorkingBytes - 1 } }));
        Assert.Equal(unchanged, encoded);
    }

    public static IEnumerable<object[]> EncodeProfiles()
    {
        foreach (var size in new[] { (1, 1), (17, 19), (65, 33) })
        foreach (var depth in new[] { 8, 16 })
        foreach (var opaque in new[] { false, true })
        foreach (var quality in new[] { 1, 100 })
        foreach (var effort in new[] { 0, 6 }) yield return [size.Item1, size.Item2, depth, opaque, quality, effort];
    }

    [Theory]
    [MemberData(nameof(EncodeProfiles))]
    public void EncoderAdmitsPlanesModesQuantizationAlphaAndArithmeticGrowthBeforeOutput(int width, int height, int depth, bool opaque, int quality, int effort)
    {
        using var source = OriginalPixels.Buffer(width, height, depth, opaque: opaque);
        var unchanged = OriginalPixels.Bytes(source);
        var options = new WebpEncodeOptions { Lossless = false, Quality = quality, Effort = effort };
        // A cancellable token selects individual cache traversal consistently.
        // A generous operation's optional history peak is not a minimum limit.
        using var traversal = new CancellationTokenSource();
        var reference = WebpAdmissionControl.Reference(source, effort, lossless: false, quality: quality, cancellationToken: traversal.Token);
        var expected = reference.Packet; var peak = reference.WorkingBytes;
        var limits = new ImageLimits { MaxWorkingBytes = peak };
        using var exact = new NonseekableOutput();
        ImageCodec.EncodeWebp(source, exact, options with { Limits = limits }, traversal.Token);
        Assert.Equal(expected, exact.ToArray());
        Assert.True(exact.CanWrite);
        using var rejectedMemory = new MemoryStream();
        Assert.Throws<ImageLimitException>(() => ImageCodec.EncodeWebp(source, rejectedMemory, options with
        { Limits = limits with { MaxWorkingBytes = peak - 1 } }, traversal.Token));
        Assert.Equal(0, rejectedMemory.Length); Assert.True(rejectedMemory.CanWrite);
        using var exactSize = new MemoryStream();
        ImageCodec.EncodeWebp(source, exactSize, options with { Limits = limits with { MaxOutputBytes = expected.Length } }, traversal.Token);
        Assert.Equal(expected, exactSize.ToArray());
        using var rejectedSize = new MemoryStream();
        Assert.Throws<ImageLimitException>(() => ImageCodec.EncodeWebp(source, rejectedSize, options with
        { Limits = limits with { MaxOutputBytes = expected.Length - 1 } }, traversal.Token));
        Assert.Equal(0, rejectedSize.Length); Assert.True(rejectedSize.CanWrite);
        using var canceled = new MemoryStream();
        Assert.Throws<OperationCanceledException>(() => ImageCodec.EncodeWebp(source, canceled, options, new CancellationToken(true)));
        Assert.Equal(0, canceled.Length); Assert.True(canceled.CanWrite);
        using var decoded = ImageCodec.Decode(expected);
        Assert.Equal((width, height, 8), (decoded.Width, decoded.Height, decoded.BitDepth));
        for (var y = 0; y < height; y++)
        for (var x = 0; x < width; x++) Assert.Equal(depth == 8 ? source.GetRow8(y)[x * 4 + 3]
            : (source.GetRow16(y)[x * 4 + 3] + 128) / 257, decoded.GetRow8(y)[x * 4 + 3]);
        Assert.Equal(unchanged, OriginalPixels.Bytes(source));
    }

    private static byte[] Vp8Payload(byte[] encoded)
    {
        for (var offset = 12; offset < encoded.Length;)
        {
            var length = System.Buffers.Binary.BinaryPrimitives.ReadInt32LittleEndian(encoded.AsSpan(offset + 4));
            if (encoded.AsSpan(offset, 4).SequenceEqual("VP8 "u8)) return encoded.AsSpan(offset + 8, length).ToArray();
            offset += 8 + length + (length & 1);
        }
        throw new InvalidOperationException("Original managed VP8 chunk not found.");
    }

    private static int Capacity(int bytes)
    {
        var capacity = 64;
        while (capacity < bytes) capacity *= 2;
        return capacity;
    }

    private static long GrowthPeak(int capacity) => capacity == 64 ? 96 : capacity + capacity / 2L + 64;

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
