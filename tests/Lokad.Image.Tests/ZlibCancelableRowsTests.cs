using System.Buffers.Binary;
using System.IO.Compression;
using Lokad.Image.Internal;
using Lokad.Image.Tooling;

namespace Lokad.Image.Tests;

public sealed partial class ZlibMaterializationTests
{
    [Theory]
    [InlineData(CompressionLevel.NoCompression)]
    [InlineData(CompressionLevel.Optimal)]
    [InlineData(CompressionLevel.SmallestSize)]
    public void CancelableRowsPreserveSamplesChecksumsAndAdmissionAcrossManyHistorySlides(CompressionLevel level)
    {
        const int rowBytes = 255, height = 2048;
        var raw = new byte[(rowBytes + 1) * height];
        for (var i = 0; i < raw.Length; i++) raw[i] = i % 256 == 0 ? (byte)0 : (byte)(i * 73 + (i >> 12));
        using var encodedStream = new MemoryStream();
        using (var zlib = new ZLibStream(encodedStream, level, leaveOpen: true)) zlib.Write(raw);
        var encoded = encodedStream.ToArray(); var original = encoded.ToArray();
        Assert.Equal(raw, Inflate(encoded));
        using var cancellation = new CancellationTokenSource();
        using var root = new TestMemoryBudget(4096);
        var limits = ImageLimits.Default with { MaxWorkingBytes = 4096, MemoryBudget = root };
        using var context = new OperationContext(limits, cancellation.Token);
        var rows = new PngRowInflation(rowBytes, height, new byte[PngRowInflation.HistoryBytes]);
        Assert.Equal(encoded.Length, ZlibFraming.ValidateAndInflateRows(encoded, raw.Length, context, rows, out var complete));
        Assert.True(complete);
        Assert.True(rows.Finish(BinaryPrimitives.ReadUInt32BigEndian(encoded.AsSpan(encoded.Length - 4))));
        Assert.Equal(0, root.Bytes); Assert.False(root.IsDisposed); Assert.Equal(4096, context.RemainingWorkingBytes);
        Assert.Equal(original, encoded);
        cancellation.Cancel();
        Assert.Throws<OperationCanceledException>(() => ZlibFraming.ValidateAndInflateRows(encoded, raw.Length, context,
            new PngRowInflation(rowBytes, height, new byte[PngRowInflation.HistoryBytes]), out _));
        Assert.Equal(0, root.Bytes); Assert.Equal(4096, context.RemainingWorkingBytes);
        using var recovery = new OperationContext(limits, default);
        Assert.Equal(encoded.Length, ZlibFraming.Validate(encoded, raw.Length, recovery));
        Assert.Equal(0, root.Bytes); Assert.False(root.IsDisposed);
    }
}
