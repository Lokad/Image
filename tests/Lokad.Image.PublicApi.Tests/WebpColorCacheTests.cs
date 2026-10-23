using System.Buffers.Binary;
using Lokad.Image.Tooling;

namespace Lokad.Image.PublicApi.Tests;

public sealed class WebpColorCacheTests
{
    [Theory] [InlineData(8)] [InlineData(16)]
    public void CacheCollisionsCopiedRowsHiddenColorsAndIndependentOutputsRemainExact(int depth)
    {
        using var source = Source(depth); var expected = Normalized(source);
        var colors = new HashSet<uint>();
        for (var i = 0; i < expected.Length; i += 4) colors.Add(BinaryPrimitives.ReadUInt32LittleEndian(expected.AsSpan(i, 4)));
        Assert.True(colors.Count > 1024); // A bijective transform cannot avoid cache collisions.
        var first = Encode(source); var second = Encode(source); Assert.Equal(first, second);
        using var decoded = ImageCodec.Decode(first);
        first.AsSpan().Clear(); source.Dispose();
        Assert.Equal(expected, OriginalPixels.Bytes(decoded));
        using var independent = ImageCodec.Decode(second); decoded.Dispose();
        Assert.Equal(expected, OriginalPixels.Bytes(independent));
    }

    [Theory] [InlineData(8)] [InlineData(16)]
    public void LocalTightWorkspaceChoosesSimpleEncodingBeforeSharedAdmissionAndStaysLossless(int depth)
    {
        using var source = Source(depth); var expected = Normalized(source);
        var limits = new ImageLimits { MaxWorkingBytes = (long)source.Stride * source.Height + 8192 };
        using var output = new NonseekableOutput();
        ImageCodec.EncodeWebp(source, output, new() { Limits = limits });
        using var decoded = ImageCodec.Decode(output.ToArray());
        Assert.Equal(expected, OriginalPixels.Bytes(decoded)); Assert.Equal(expected, Normalized(source));
        Assert.True(output.CanWrite);
    }

    [Fact]
    public void CachedEncodingRespectsLocalOutputAndWorkPreflightWithoutClosingCallerStream()
    {
        using var source = Source(8);
        // Freeze the cached-codec boundary, excluding optional larger analysis.
        var limits = new ImageLimits { MaxWorkingBytes = (long)source.Stride * source.Height + 65536 };
        var packet = Encode(source, limits); limits = limits with { MaxOutputBytes = packet.Length };
        using var exact = new NonseekableOutput(); ImageCodec.EncodeWebp(source, exact, new() { Limits = limits });
        Assert.Equal(packet, exact.ToArray()); Assert.True(exact.CanWrite);
        foreach (var (denied, kind) in new[]
        {
            (limits with { MaxOutputBytes = packet.Length - 1 }, ImageLimitKind.OutputBytes),
            (limits with { MaxWork = 1 }, ImageLimitKind.Work)
        })
        {
            using var rejected = new NonseekableOutput();
            Assert.Equal(kind, Assert.Throws<ImageLimitException>(() => ImageCodec.EncodeWebp(source, rejected, new() { Limits = denied })).Limit);
            Assert.Empty(rejected.ToArray()); Assert.True(rejected.CanWrite);
        }
    }

    [Fact]
    public void SharedRefusalIsTerminalAndReleasedCapacityAllowsSuccess()
    {
        const int sourceBytes = 129 * 129 * 4;
        using var root = new TestMemoryBudget(sourceBytes + 65536);
        var limits = new ImageLimits { MemoryBudget = root, MaxWorkingBytes = sourceBytes + 65536 };
        using var source = Source(8, limits);
        Assert.Equal(sourceBytes, root.Bytes);
        using var caller = root.CreateScope(); Assert.True(caller.TryReserve(65536 - 8192, out var held));
        using var rejected = new MemoryStream();
        var error = Assert.Throws<ImageLimitException>(() => ImageCodec.EncodeWebp(source, rejected, new() { Limits = limits }));
        Assert.Equal((ImageLimitKind.WorkingBytes, ImageLimitScope.SharedBudget), (error.Limit, error.Scope));
        Assert.Equal(1, root.Refusals); Assert.Equal(sourceBytes + 65536 - 8192, root.Bytes);
        Assert.Equal(0, rejected.Length); Assert.True(rejected.CanWrite);
        held.Dispose();
        using var output = new MemoryStream(); ImageCodec.EncodeWebp(source, output, new() { Limits = limits });
        Assert.Equal(sourceBytes, root.Bytes); Assert.InRange(root.Peak, sourceBytes + 8193, sourceBytes + 65536);
        Assert.False(root.IsDisposed); source.Dispose(); Assert.Equal(0, root.Bytes);
        using var surviving = ImageCodec.Decode(output.ToArray()); Assert.Equal(129, surviving.Width);
    }

    [Fact]
    public void CancellationDuringAdmissionReleasesCodecStorageAndAllowsSubsequentEncode()
    {
        using var root = new TestMemoryBudget(1_000_000);
        using var source = Source(8, new() { MemoryBudget = root }); var charged = root.Bytes;
        var limits = new ImageLimits { MemoryBudget = root, MaxWorkingBytes = charged + 65536 };
        using var cancellation = new CancellationTokenSource();
        root.Admitted = bytes => { if (bytes > charged + 8192) cancellation.Cancel(); };
        using var rejected = new MemoryStream();
        Assert.Throws<OperationCanceledException>(() => ImageCodec.EncodeWebp(source, rejected,
            new() { Limits = limits }, cancellation.Token));
        Assert.Equal(charged, root.Bytes); Assert.Equal(0, rejected.Length); root.Admitted = null;
        using var output = new MemoryStream(); ImageCodec.EncodeWebp(source, output, new() { Limits = limits });
        Assert.Equal(charged, root.Bytes); Assert.False(root.IsDisposed);
    }

    private static ImageBuffer Source(int depth, ImageLimits? limits = null)
    {
        var source = ImageBuffer.Create(129, 129, depth, limits);
        var samples = OriginalPixels.Generate(129, 129, depth);
        for (var y = 0; y < source.Height; y++)
        {
            if (depth == 8) samples.AsSpan(y * source.Width * 4, source.Width * 4).CopyTo(source.GetRow8(y));
            else
                for (var x = 0; x < source.Width * 4; x++)
                    source.GetRow16(y)[x] = BinaryPrimitives.ReadUInt16LittleEndian(samples.AsSpan((y * source.Width * 4 + x) * 2));
            // Independent blue variation gives more distinct normalized colors
            // than cache slots, rather than assuming the correlated source does.
            for (var x = 0; x < source.Width; x++)
            {
                if (depth == 8) source.GetRow8(y)[x * 4 + 2] ^= (byte)(x + y * 11);
                else source.GetRow16(y)[x * 4 + 2] ^= (ushort)(((x + y * 11) & 255) * 257);
            }
        }
        // The first symbol can reference the normative zero-initialized cache.
        if (depth == 8) source.GetRow8(0)[..4].Clear(); else source.GetRow16(0)[..4].Clear();
        // More than one maximum-length reference, overlapping repeated rows,
        // followed by fresh colors that depend on the updated cache state.
        for (var y = 1; y <= 9; y++)
        {
            if (depth == 8) source.GetRow8(0).CopyTo(source.GetRow8(y));
            else source.GetRow16(0).CopyTo(source.GetRow16(y));
        }
        return source;
    }

    private static byte[] Normalized(ImageBuffer source)
    {
        var result = new byte[source.Width * source.Height * 4];
        for (var y = 0; y < source.Height; y++)
        for (var x = 0; x < source.Width * 4; x++)
            result[y * source.Width * 4 + x] = source.BitDepth == 8 ? source.GetRow8(y)[x] : (byte)((source.GetRow16(y)[x] + 128) / 257);
        return result;
    }

    private static byte[] Encode(ImageBuffer source, ImageLimits? limits = null)
    { using var output = new MemoryStream(); ImageCodec.EncodeWebp(source, output, new() { Limits = limits ?? ImageLimits.Default }); return output.ToArray(); }

    private sealed class NonseekableOutput : MemoryStream
    {
        public override bool CanSeek => false;
        public override long Position { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }
        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
    }
}
