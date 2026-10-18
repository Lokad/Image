using System.IO.Compression;
using Lokad.Image.Tooling;

namespace Lokad.Image.PublicApi.Tests;

public sealed class PngStoredCapacityTests
{
    [Theory]
    [InlineData(8)] [InlineData(16)]
    public void CompletePacketsAndPixelsMatchOriginalWritesWithIndependentOwners(int depth)
    {
        using var source = OriginalPixels.Buffer(513, 259, depth);
        var original = OriginalPixels.Bytes(source);
        var limits = ImageLimits.Default with
        {
            MaxWorkingBytes = original.Length + source.Stride + 1024L * 1024 + 65536,
            MaxWork = (long)source.Stride * source.Height * 8
        };
        var options = Options() with { Limits = limits };
        using (var rejected = new MemoryStream())
        {
            Assert.Throws<ImageLimitException>(() => ImageCodec.EncodePng(source, rejected,
                options with { Limits = limits with { MaxWorkingBytes = limits.MaxWorkingBytes - 1 } }));
            Assert.Equal(0, rejected.Length); Assert.Equal(0, rejected.Capacity);
        }
        foreach (var position in new[] { 0, 7, 80 })
        {
            using var expected = new OriginalOutput(); using var actual = new MemoryStream();
            using var independent = new MemoryStream();
            foreach (var output in new MemoryStream[] { expected, actual, independent })
            {
                output.Write(new byte[] { 251, 127, 3, 99, 13, 17, 23 }); output.Position = position;
                ImageCodec.EncodePng(source, output, options);
            }
            var packet = actual.ToArray();
            Assert.Equal(expected.ToArray(), packet); Assert.Equal(packet, independent.ToArray());
            Assert.Equal(expected.Position, actual.Position); Assert.Equal(expected.Length, actual.Length);
            Assert.True(actual.CanWrite); Assert.True(independent.CanWrite);
            using var decoded = ImageCodec.Decode(packet.AsSpan(position));
            Assert.Equal(original, OriginalPixels.Bytes(decoded));
            actual.Position = position; actual.WriteByte(0);
            Assert.Equal(packet, independent.ToArray()); Assert.Equal(original, OriginalPixels.Bytes(source));
        }
    }

    [Theory]
    [InlineData(8)] [InlineData(16)]
    public void WorkAndOutputLimitsKeepOriginalExceptionPrefixesAndCapacity(int depth)
    {
        using var source = OriginalPixels.Buffer(513, 259, depth);
        using var complete = new OriginalOutput(); ImageCodec.EncodePng(source, complete, Options());
        var exactWork = (long)source.Stride * source.Height * 8;
        foreach (var limits in new[]
        {
            ImageLimits.Default with { MaxWork = exactWork - 1 },
            ImageLimits.Default with { MaxOutputBytes = 1 },
            ImageLimits.Default with { MaxOutputBytes = 40 },
            ImageLimits.Default with { MaxOutputBytes = 100000 },
            ImageLimits.Default with { MaxOutputBytes = (int)complete.Length - 1 },
            ImageLimits.Default with { MaxOutputBytes = (int)complete.Length }
        })
        {
            using var expected = new OriginalOutput(); using var actual = new MemoryStream();
            var options = Options() with { Limits = limits };
            var first = Record.Exception(() => ImageCodec.EncodePng(source, expected, options));
            var second = Record.Exception(() => ImageCodec.EncodePng(source, actual, options));
            Assert.Equal(first?.GetType(), second?.GetType()); Assert.Equal(first?.Message, second?.Message);
            Assert.Equal(expected.ToArray(), actual.ToArray()); Assert.Equal(expected.Position, actual.Position);
            Assert.Equal(expected.Capacity, actual.Capacity); Assert.True(actual.CanWrite);
        }
    }

    [Theory]
    [InlineData(8)] [InlineData(16)]
    public void FixedReadOnlyDisposedAndCancelableStreamsKeepOriginalBehavior(int depth)
    {
        using var source = OriginalPixels.Buffer(513, 259, depth);
        var options = Options();
        using var cancellation = new CancellationTokenSource();
        using (var expected = new OriginalOutput())
        using (var actual = new MemoryStream())
        {
            ImageCodec.EncodePng(source, expected, options, cancellation.Token);
            ImageCodec.EncodePng(source, actual, options, cancellation.Token);
            Assert.Equal(expected.ToArray(), actual.ToArray()); Assert.Equal(expected.Capacity, actual.Capacity);
        }
        foreach (var scenario in new[] { 0, 1, 2, 3 })
        {
            var firstBytes = Enumerable.Repeat((byte)211, 50007).ToArray(); var secondBytes = firstBytes.ToArray();
            using var expected = new OriginalOutput(firstBytes, writable: scenario != 1);
            using var actual = new MemoryStream(secondBytes, 3, 50000, writable: scenario != 1, publiclyVisible: true);
            if (scenario == 2) { expected.Dispose(); actual.Dispose(); }
            if (scenario == 3) { expected.Position = actual.Position = int.MaxValue - 4L; }
            var first = Record.Exception(() => ImageCodec.EncodePng(source, expected, options));
            var second = Record.Exception(() => ImageCodec.EncodePng(source, actual, options));
            Assert.NotNull(first); Assert.Equal(first.GetType(), second?.GetType()); Assert.Equal(first.Message, second?.Message);
            Assert.Equal(firstBytes, secondBytes); Assert.Equal(expected.ToArray(), actual.ToArray());
            if (actual.CanSeek)
            {
                Assert.Equal(expected.Position, actual.Position); Assert.Equal(expected.Capacity, actual.Capacity);
            }
        }
    }

    private static PngEncodeOptions Options() => new() { Compression = CompressionLevel.NoCompression };
    private sealed class OriginalOutput : MemoryStream
    {
        public OriginalOutput() { }
        public OriginalOutput(byte[] buffer, bool writable) : base(buffer, 3, 50000, writable, publiclyVisible: true) { }
    }
}
