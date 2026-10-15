using System.Buffers.Binary;
using System.IO.Compression;
using Lokad.Image.Tooling;

namespace Lokad.Image.PublicApi.Tests;

public sealed class PngAdaptiveEntropyTests
{
    [Theory]
    [InlineData(8)] [InlineData(16)]
    public void AdaptiveEncodingKeepsEverySampleAndFastClass(int depth)
    {
        using var source = OriginalPixels.Buffer(513, 259, depth); var original = OriginalPixels.Bytes(source);
        using var live = new CancellationTokenSource();
        var legacy = Encode(source, token: live.Token); var candidate = Encode(source);
        Assert.True(candidate.Length <= legacy.Length);
        if (depth == 16) Assert.True(candidate.Length < legacy.Length);
        Assert.Equal(0, Idat(candidate)[1] >> 6);
        using var first = ImageCodec.Decode(candidate); using var second = ImageCodec.Decode(candidate);
        Assert.Equal(original, OriginalPixels.Bytes(first)); Assert.Equal(original, OriginalPixels.Bytes(second));
        if (depth == 8) first.GetRow8(0)[0] ^= 255;
        else first.GetRow16(0)[0] ^= 65535;
        Assert.Equal(original, OriginalPixels.Bytes(source)); Assert.Equal(original, OriginalPixels.Bytes(second));
    }

    [Fact]
    public void ExactOptionalMemoryAndWorkBoundariesSelectWithoutChangingLegacyAdmission()
    {
        using var source = OriginalPixels.Buffer(513, 259, 16); using var live = new CancellationTokenSource();
        var legacy = Encode(source, token: live.Token); var adapted = Encode(source);
        var rowBytes = 513 * 8; var filtered = (rowBytes + 1L) * 259; var capacity = filtered * 2 + 65536;
        var oldPeak = 513L * 259 * 8 + rowBytes + 65536 + 1024 * 1024;
        var peak = oldPeak + 2 * capacity + 32768; var work = rowBytes * 259L * 8 + capacity * 16 + 131072;
        Assert.Equal(adapted, Encode(source, ImageLimits.Default with { MaxWorkingBytes = peak, MaxWork = work }));
        Assert.Equal(legacy, Encode(source, ImageLimits.Default with { MaxWorkingBytes = peak - 1 }));
        Assert.Equal(legacy, Encode(source, ImageLimits.Default with { MaxWork = work - 1 }));
        Assert.Equal(legacy, Encode(source, ImageLimits.Default with { MaxWorkingBytes = oldPeak }));
        using var rejected = new MemoryStream();
        Assert.Throws<ImageLimitException>(() => ImageCodec.EncodePng(source, rejected, Options(ImageLimits.Default with { MaxWorkingBytes = oldPeak - 1 })));
        Assert.Equal(0, rejected.Length);
    }

    [Theory]
    [InlineData("memory")] [InlineData("work")] [InlineData("output")]
    public void ConstrainedFailuresKeepOriginalExceptionAndPartialPacket(string constraint)
    {
        using var source = OriginalPixels.Buffer(513, 259, 16); using var live = new CancellationTokenSource();
        var legacy = Encode(source, token: live.Token);
        var limits = constraint switch
        {
            "memory" => ImageLimits.Default with { MaxWorkingBytes = 513L * 259 * 8 + 513 * 8 + 65536 + 1024 * 1024 - 1 },
            "work" => ImageLimits.Default with { MaxWork = 513L * 259 * 8 * 8 - 1 },
            _ => ImageLimits.Default with { MaxOutputBytes = legacy.Length - 1 }
        };
        using var actual = new MemoryStream(); using var reference = new MemoryStream();
        var expected = Assert.Throws<ImageLimitException>(() => ImageCodec.EncodePng(source, reference, Options(limits), live.Token));
        var error = Assert.Throws<ImageLimitException>(() => ImageCodec.EncodePng(source, actual, Options(limits)));
        Assert.Equal(expected.Message, error.Message); Assert.Equal(reference.ToArray(), actual.ToArray());
    }

    [Fact]
    public void NonexpandableOutputKeepsOriginalPrefixAndCapacity()
    {
        using var source = OriginalPixels.Buffer(513, 259, 16); using var live = new CancellationTokenSource();
        using var reference = new MemoryStream(new byte[1024]); using var actual = new MemoryStream(new byte[1024]);
        reference.SetLength(0); actual.SetLength(0);
        Assert.Throws<NotSupportedException>(() => ImageCodec.EncodePng(source, reference, Options(), live.Token));
        Assert.Throws<NotSupportedException>(() => ImageCodec.EncodePng(source, actual, Options()));
        Assert.Equal(reference.ToArray(), actual.ToArray()); Assert.Equal(1024, actual.Capacity);
    }

    [Fact]
    public void CallerCallbackExceptionKeepsCallSequenceAndWrittenPrefix()
    {
        using var source = OriginalPixels.Buffer(513, 259, 16); using var live = new CancellationTokenSource();
        using var reference = new FailingStream(); using var actual = new FailingStream();
        Assert.Same(reference.Failure, Assert.Throws<IOException>(() => ImageCodec.EncodePng(source, reference, Options(), live.Token)));
        Assert.Same(actual.Failure, Assert.Throws<IOException>(() => ImageCodec.EncodePng(source, actual, Options())));
        Assert.Equal(reference.Writes, actual.Writes); Assert.Equal(reference.ToArray(), actual.ToArray());
    }

    private static PngEncodeOptions Options(ImageLimits? limits = null) => new() { Compression = CompressionLevel.Fastest, Limits = limits ?? ImageLimits.Default };
    private static byte[] Encode(ImageBuffer source, ImageLimits? limits = null, CancellationToken token = default)
    {
        using var output = new MemoryStream(); ImageCodec.EncodePng(source, output, Options(limits), token); return output.ToArray();
    }
    private static byte[] Idat(byte[] png)
    {
        using var result = new MemoryStream();
        for (var p = 8; p < png.Length;)
        {
            var size = BinaryPrimitives.ReadInt32BigEndian(png.AsSpan(p));
            if (png.AsSpan(p + 4, 4).SequenceEqual("IDAT"u8)) result.Write(png.AsSpan(p + 8, size));
            p += size + 12;
        }
        return result.ToArray();
    }
    private sealed class FailingStream : MemoryStream
    {
        internal readonly IOException Failure = new("Original callback failure.");
        internal List<int> Writes { get; } = [];
        public override void Write(ReadOnlySpan<byte> data)
        {
            Writes.Add(data.Length); base.Write(data);
            if (Writes.Count == 10) throw Failure;
        }
    }
}
