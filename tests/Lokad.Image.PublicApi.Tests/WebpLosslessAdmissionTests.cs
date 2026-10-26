using Lokad.Image.Tooling;

namespace Lokad.Image.PublicApi.Tests;

public sealed class WebpLosslessAdmissionTests
{
    public static IEnumerable<object[]> DecodeProfiles()
    {
        foreach (var orient in new[] { false, true })
        {
            yield return ["predictor", 13, 3, 3, 1164L, orient];
            yield return ["cross-color", 0, 5, 3, 1168L, orient];
            yield return ["subtract-green", 0, 3, 2, 1128L, orient];
            foreach (var colors in new[] { 1, 2, 3, 4, 16, 17, 256 })
                yield return ["palette", colors, 9, 1, 2984L + colors * 4, orient];
            yield return ["palette-predictor", 2, 9, 1, 3028L, orient];
            yield return ["backward", 0, 5, 1, 1428L, orient];
            foreach (var groups in new[] { 2, 16, 256 }) yield return ["groups", groups, 8, 1, 808L + groups * 360, orient];
            yield return ["duplicate", 0, 1, 1, 1128L, orient];
            yield return ["repeats", 0, 5, 1, 1428L, orient];
            yield return ["repeats", 1, 1, 1, 2952L, orient];
            foreach (var cacheBits in Enumerable.Range(1, 11))
            {
                var cacheSize = 1 << cacheBits;
                yield return ["cache", cacheBits, 2, 1, 512L + cacheSize * 4 + 32 + Math.Max(748 + cacheSize, 916), orient];
            }
            foreach (var size in new[] { (1, 1), (64, 1), (99, 1), (17, 11), (16384, 1) })
            {
                var count = (long)size.Item1 * size.Item2;
                yield return ["constant", 0, size.Item1, size.Item2, Math.Max(1128L, Math.Max(936 + count * 4, 544 + count * 8)), orient];
            }
        }
    }

    [Theory]
    [MemberData(nameof(DecodeProfiles))]
    public void TransformsCachesAndGroupsHaveExactDecodeAdmission(string profile, int parameter, int width, int height, long workspace, bool orient)
    {
        // Expected peaks are derived from the documented admission charges and
        // these original streams, not measured from production allocator state.
        var encoded = profile switch
        {
            "predictor" => AuthoredWebp.Predictor(parameter), "cross-color" => AuthoredWebp.ColorTransform(),
            "subtract-green" => AuthoredWebp.SubtractGreen(), "palette" => AuthoredWebp.Palette(parameter),
            "palette-predictor" => AuthoredWebp.Palette(parameter, thenPredictor: true),
            "backward" => AuthoredWebp.BackwardReference(), "groups" => AuthoredWebp.EntropyGroups(parameter),
            "duplicate" => AuthoredWebp.DuplicateSimpleSymbols(), "repeats" => AuthoredWebp.CodeLengthRepeats(parameter != 0),
            "cache" => AuthoredWebp.CachedColor(parameter), "constant" => AuthoredWebp.ConstantRaster(width, height),
            _ => throw new ArgumentOutOfRangeException(nameof(profile))
        };
        using var control = ImageCodec.Decode(encoded);
        var expected = OriginalPixels.Bytes(control);
        if (profile is "cache" or "constant")
            for (var offset = 0; offset < expected.Length; offset += 4)
                Assert.Equal(new byte[] { 20, 12, 30, 255 }, expected.AsSpan(offset, 4).ToArray());
        if (profile.StartsWith("palette", StringComparison.Ordinal)) Assert.Equal(AuthoredWebp.ExpectedPalette(parameter), expected);
        if (profile == "groups")
            for (var x = 0; x < 8; x++)
                Assert.Equal(x < 4 ? new byte[] { 10, 20, 30, 255 } : new byte[] { 40, 50, 60, 255 }, expected.AsSpan(x * 4, 4).ToArray());
        if (orient)
        {
            encoded = OriginalExif.Attach(new MetadataFixture(encoded, null, width, height, MetadataProfile.WebpLosslessAlpha),
                ImageOrientation.Rotate90, bigEndian: false);
            expected = OriginalExif.Permute(expected, width, height, 8, ImageOrientation.Rotate90);
        }
        var unchanged = encoded.ToArray();
        var peak = encoded.Length + workspace;
        var options = new DecodeOptions { ApplyOrientation = orient, Limits = new ImageLimits { MaxWorkingBytes = peak } };
        using var exact = ImageCodec.Decode(encoded, options);
        Assert.Equal(expected, OriginalPixels.Bytes(exact));
        Assert.Equal(orient ? (height, width) : (width, height), (exact.Width, exact.Height));
        Assert.Equal(ImageOrientation.Normal, exact.Orientation);
        Assert.Throws<ImageLimitException>(() => ImageCodec.Decode(encoded, options with
        { Limits = options.Limits with { MaxWorkingBytes = peak - 1 } }));
        Assert.Equal(unchanged, encoded);
    }

    public static IEnumerable<object[]> EncodeProfiles()
    {
        foreach (var depth in new[] { 8, 16 })
        foreach (var effort in Enumerable.Range(0, 7)) yield return [depth, effort];
    }

    [Theory]
    [MemberData(nameof(EncodeProfiles))]
    public void PreparedEncoderAdmitsSourceScratchAndChecksCompleteSizeBeforeWriting(int depth, int effort)
    {
        using var source = ImageBuffer.Create(4103, 2, depth);
        for (var y = 0; y < source.Height; y++)
        for (var x = 0; x < source.Width; x++)
        {
            if (depth == 8) new byte[] { 48, 211, 255, 0 }.CopyTo(source.GetRow8(y)[(x * 4)..]);
            else new ushort[] { 12345, 54321, 65534, 0 }.CopyTo(source.GetRow16(y)[(x * 4)..]);
        }
        // Long overlapping runs cross row and maximum length-code boundaries.
        if (depth == 8) source.GetRow8(1)[^4] = 77;
        else source.GetRow16(1)[^4] = 77 * 257;
        var unchanged = OriginalPixels.Bytes(source);
        var options = new WebpEncodeOptions { Lossless = true, Effort = effort };
        var reference = WebpAdmissionControl.Reference(source, effort); var expected = reference.Packet;
        var limits = new ImageLimits { MaxWorkingBytes = reference.WorkingBytes, MaxOutputBytes = expected.Length };
        using var exact = new NonseekableOutput(); ImageCodec.EncodeWebp(source, exact, options with { Limits = limits });
        Assert.Equal(expected, exact.ToArray()); Assert.True(exact.CanWrite);
        using var decoded = ImageCodec.Decode(expected);
        for (var y = 0; y < source.Height; y++)
        for (var sample = 0; sample < source.Width * 4; sample++)
            Assert.Equal(depth == 8 ? source.GetRow8(y)[sample] : (source.GetRow16(y)[sample] + 128) / 257, decoded.GetRow8(y)[sample]);
        using var rejectedMemory = new MemoryStream();
        Assert.Throws<ImageLimitException>(() => ImageCodec.EncodeWebp(source, rejectedMemory, options with
        { Limits = limits with { MaxWorkingBytes = limits.MaxWorkingBytes - 1 } }));
        Assert.Equal(0, rejectedMemory.Length); Assert.True(rejectedMemory.CanWrite);
        using var rejectedOutput = new MemoryStream();
        Assert.Throws<ImageLimitException>(() => ImageCodec.EncodeWebp(source, rejectedOutput, options with
        { Limits = limits with { MaxOutputBytes = expected.Length - 1 } }));
        Assert.Equal(0, rejectedOutput.Length); Assert.True(rejectedOutput.CanWrite);
        using var canceled = new MemoryStream();
        Assert.Throws<OperationCanceledException>(() => ImageCodec.EncodeWebp(source, canceled, options, new CancellationToken(true)));
        Assert.Equal(0, canceled.Length); Assert.True(canceled.CanWrite);
        Assert.Equal(unchanged, OriginalPixels.Bytes(source));
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
