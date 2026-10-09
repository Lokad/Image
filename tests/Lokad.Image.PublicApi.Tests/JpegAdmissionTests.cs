using Lokad.Image.Tooling;

namespace Lokad.Image.PublicApi.Tests;

public sealed class JpegAdmissionTests
{
    public static IEnumerable<object[]> DecodeProfiles()
    {
        foreach (var size in new[] { (1, 1), (32, 24) })
        foreach (var sampling in Enum.GetValues<JpegSampling>())
        foreach (var progressive in new[] { false, true })
        foreach (var orient in new[] { false, true }) yield return [size.Item1, size.Item2, sampling, progressive, orient];
    }

    [Theory]
    [MemberData(nameof(DecodeProfiles))]
    public void CoefficientsPlanesTablesAndOrientationShareTheDecodeAdmission(int width, int height, JpegSampling sampling, bool progressive, bool orient)
    {
        var encoded = CreateEncoded(width, height, sampling, progressive);
        using var control = ImageCodec.Decode(encoded);
        var expected = OriginalPixels.Bytes(control);
        if (orient)
        {
            encoded = OriginalExif.Attach(new MetadataFixture(encoded, null, width, height,
                progressive ? MetadataProfile.JpegProgressive420 : MetadataProfile.JpegBaseline420), ImageOrientation.Rotate90, bigEndian: true);
            expected = OriginalExif.Permute(expected, width, height, 8, ImageOrientation.Rotate90);
        }
        var peak = encoded.Length + WorkspacePeak(width, height, sampling, progressive, orient);
        var options = new DecodeOptions { ApplyOrientation = orient, Limits = new ImageLimits { MaxWorkingBytes = peak } };
        using var exact = ImageCodec.Decode(encoded, options);
        Assert.Equal(expected, OriginalPixels.Bytes(exact));
        Assert.Equal(orient ? (height, width) : (width, height), (exact.Width, exact.Height));
        Assert.Equal(ImageOrientation.Normal, exact.Orientation);
        Assert.Throws<ImageLimitException>(() => ImageCodec.Decode(encoded, options with
        { Limits = options.Limits with { MaxWorkingBytes = peak - 1 } }));
        Assert.Throws<ImageLimitException>(() => ImageCodec.Decode(encoded, options with
        { Limits = options.Limits with { MaxWork = 1 } }));
        Assert.Throws<OperationCanceledException>(() => ImageCodec.Decode(encoded, options, new CancellationToken(true)));
    }

    [Theory]
    [InlineData(false, 1)] [InlineData(false, 64)]
    [InlineData(true, 1)] [InlineData(true, 64)]
    public void RedefinedHuffmanAllocationsRemainChargedAndReplacedQuantizationIsReleased(bool huffman, int definitions)
    {
        var original = CreateEncoded(32, 24, JpegSampling.Grayscale, progressive: false);
        using var rewritten = new MemoryStream(); rewritten.Write(original.AsSpan(0, 2));
        // Independently construct unused table 3. Its repeated definitions do not
        // affect the coded pixels; DHT charges accumulate, DQT keeps one active copy.
        var payload = new byte[huffman ? 18 : 65]; payload[0] = 3;
        if (huffman) payload[1] = 1;
        else Array.Fill(payload, (byte)1, 1, 64);
        for (var i = 0; i < definitions; i++)
        {
            rewritten.Write(new byte[] { 255, huffman ? (byte)0xC4 : (byte)0xDB, 0, (byte)(payload.Length + 2) });
            rewritten.Write(payload);
        }
        rewritten.Write(original.AsSpan(2)); var encoded = rewritten.ToArray();
        var charge = huffman ? definitions * 205L : 128L;
        var peak = encoded.Length + WorkspacePeak(32, 24, JpegSampling.Grayscale, progressive: false, orient: false) + charge;
        using var expected = ImageCodec.Decode(original);
        var options = new DecodeOptions { Limits = new ImageLimits { MaxWorkingBytes = peak } };
        using var actual = ImageCodec.Decode(encoded, options);
        Assert.Equal(OriginalPixels.Bytes(expected), OriginalPixels.Bytes(actual));
        Assert.Throws<ImageLimitException>(() => ImageCodec.Decode(encoded, options with
        { Limits = options.Limits with { MaxWorkingBytes = peak - 1 } }));
    }

    public static IEnumerable<object[]> EncodeProfiles()
    {
        foreach (var depth in new[] { 8, 16 })
        foreach (var sampling in Enum.GetValues<JpegSampling>()) yield return [depth, sampling];
    }

    [Theory]
    [MemberData(nameof(EncodeProfiles))]
    public void EncoderAdmitsSourceAndScratchBeforeOutputAndBoundsEveryEncodedWrite(int depth, JpegSampling sampling)
    {
        using var source = OriginalPixels.Buffer(17, 11, depth);
        var unchanged = OriginalPixels.Bytes(source);
        var options = new JpegEncodeOptions { Sampling = sampling, Quality = 85, RestartInterval = 1, BackgroundRgb = 0x17395B };
        using var reference = new MemoryStream(); ImageCodec.EncodeJpeg(source, reference, options);
        var expected = reference.ToArray();
        var limits = new ImageLimits { MaxWorkingBytes = unchanged.Length + 8192L, MaxOutputBytes = expected.Length };
        using var exact = new MemoryStream(); ImageCodec.EncodeJpeg(source, exact, options with { Limits = limits });
        Assert.Equal(expected, exact.ToArray()); Assert.True(exact.CanWrite);
        using var rejected = new MemoryStream();
        Assert.Throws<ImageLimitException>(() => ImageCodec.EncodeJpeg(source, rejected, options with
        { Limits = limits with { MaxWorkingBytes = limits.MaxWorkingBytes - 1 } }));
        Assert.Equal(0, rejected.Length); Assert.True(rejected.CanWrite);
        using var shortOutput = new MemoryStream();
        Assert.Throws<ImageLimitException>(() => ImageCodec.EncodeJpeg(source, shortOutput, options with
        { Limits = limits with { MaxOutputBytes = expected.Length - 1 } }));
        Assert.InRange(shortOutput.Length, 1L, expected.Length - 1L); Assert.True(shortOutput.CanWrite);
        using var canceled = new MemoryStream();
        Assert.Throws<OperationCanceledException>(() => ImageCodec.EncodeJpeg(source, canceled, options, new CancellationToken(true)));
        Assert.Equal(0, canceled.Length); Assert.True(canceled.CanWrite);
        Assert.Equal(unchanged, OriginalPixels.Bytes(source));
    }

    private static byte[] CreateEncoded(int width, int height, JpegSampling sampling, bool progressive)
    {
        if (progressive) return AuthoredJpeg.ProgressiveFrame(width, height, sampling, restartInterval: 1);
        using var source = OriginalPixels.Buffer(width, height, 8, opaque: true);
        using var output = new MemoryStream();
        ImageCodec.EncodeJpeg(source, output, new JpegEncodeOptions { Sampling = sampling, Quality = 85, RestartInterval = 1 });
        return output.ToArray();
    }

    private static long WorkspacePeak(int width, int height, JpegSampling sampling, bool progressive, bool orient)
    {
        // T.81 MCU geometry, and the admission policy documented in resource-admission.md.
        // No production parser/allocator or table constants are used here.
        var h = sampling is JpegSampling.YCbCr422 or JpegSampling.YCbCr420 ? 2 : 1;
        var v = sampling == JpegSampling.YCbCr420 ? 2 : 1;
        var mcus = (long)((width + 8 * h - 1) / (8 * h)) * ((height + 8 * v - 1) / (8 * v));
        var firstPlane = mcus * h * v * 64;
        var components = sampling == JpegSampling.Grayscale ? 1 : 3;
        var allPlanes = firstPlane + (components - 1) * mcus * 64;
        var quantization = progressive || components == 1 ? 128 : 256;
        // Both original builders emit one DC/AC pair: 12 DC symbols and 162 or
        // 176 AC symbols. Each table has three admitted 17-element Int32 arrays.
        var tables = 2 * 204 + 12 + (progressive ? 176 : 162);
        var state = 1024 + quantization + tables + components * (256 + 128);
        var coefficientPeak = allPlanes * 4 + firstPlane;
        var resultPeak = allPlanes + (long)width * height * 4 * (orient ? 2 : 1);
        return state + Math.Max(coefficientPeak, resultPeak);
    }
}
