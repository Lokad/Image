using System.Buffers.Binary;
using Lokad.Image.Tooling;

namespace Lokad.Image.PublicApi.Tests;

public sealed class Vp8PrefixWorkTests
{
    // Default limited-work statistics retain384 macroblocks; tighter independent
    // allowances keep the original128/64 owners before shared admission.
    private const int Width = 513, Height = 259, PrefixBytes = 384 * 120 + 16384 + 768;
    private static ImageBuffer Source(int depth, TestMemoryBudget root)
    {
        var bytes = OriginalPixels.Generate(Width, Height, depth);
        var source = ImageBuffer.Create(Width, Height, depth, new ImageLimits { MemoryBudget = root });
        for (var y = 0; y < Height; y++)
        {
            if (depth == 8) bytes.AsSpan(y * Width * 4, Width * 4).CopyTo(source.GetRow8(y));
            else for (var i = 0; i < Width * 4; i++) source.GetRow16(y)[i] = BinaryPrimitives.ReadUInt16LittleEndian(bytes.AsSpan((y * Width * 4 + i) * 2));
        }
        if (depth == 8) source.GetRow8(0)[3] = source.GetRow8(0)[Width * 4 - 1] = 0;
        else source.GetRow16(0)[3] = source.GetRow16(0)[Width * 4 - 1] = 0;
        return source;
    }
    private static WebpEncodeOptions Options(TestMemoryBudget root, long work = 32000000, long memory = 512L * 1024 * 1024)
        => new() { Lossless = false, Quality = 100, Effort = 4,
            Limits = new ImageLimits { MaxWork = work, MaxWorkingBytes = memory, MemoryBudget = root } };

    [Theory] [InlineData(8)] [InlineData(16)]
    public void LimitedWorkEncodingPreservesQualityExactAlphaAndIndependentOwners(int depth)
    {
        using var root = new TestMemoryBudget(8000000); using var source = Source(depth, root);
        var unchanged = OriginalPixels.Bytes(source); var held = root.Bytes;
        using var encoded = new MemoryStream(); using var fixedPacket = new MemoryStream();
        ImageCodec.EncodeWebp(source, encoded, Options(root)); Assert.Equal(held, root.Bytes);
        ImageCodec.EncodeWebp(source, fixedPacket, Options(root, memory: 8000000)); Assert.Equal(held, root.Bytes);
        Assert.True(encoded.Length < fixedPacket.Length); Assert.NotSame(encoded.GetBuffer(), fixedPacket.GetBuffer());
        using var expected = ImageCodec.Decode(fixedPacket.ToArray());
        using var actual = ImageCodec.Decode(encoded.ToArray(), new DecodeOptions { Limits = new ImageLimits { MemoryBudget = root } });
        // Optional coefficient decisions can change lossy RGB with local work/
        // memory admission. Keep the same reconstruction-quality tolerance and
        // exact source alpha while checking the actual result's surviving owner.
        var baseline = OriginalPixels.Bytes(expected); var pixels = OriginalPixels.Bytes(actual);
        long baselineError = 0, actualError = 0;
        for (var i = 0; i < pixels.Length; i++)
        {
            var sourceSample = depth == 8 ? unchanged[i]
                : (BinaryPrimitives.ReadUInt16LittleEndian(unchanged.AsSpan(i * 2)) + 128) / 257;
            if (i % 4 == 3) { Assert.Equal(sourceSample, pixels[i]); continue; }
            var previous = baseline[i] - sourceSample; var current = pixels[i] - sourceSample;
            baselineError += (long)previous * previous; actualError += (long)current * current;
        }
        Assert.InRange(10 * Math.Log10((double)Math.Max(baselineError, actualError)
            / Math.Max(1, Math.Min(baselineError, actualError))), 0, .25);
        Assert.Equal(unchanged, OriginalPixels.Bytes(source));
        source.Dispose(); encoded.GetBuffer().AsSpan().Clear(); fixedPacket.GetBuffer().AsSpan().Clear();
        Assert.Equal(Width * Height * 4, root.Bytes); Assert.Equal(pixels, OriginalPixels.Bytes(actual));
        actual.Dispose(); Assert.Equal(0, root.Bytes); Assert.False(root.IsDisposed);
    }

    [Fact]
    public void ActualWorkExhaustionAfterPrefixAdmissionIsTerminalAndCleanupAllowsSuccess()
    {
        using var root = new TestMemoryBudget(8000000); using var source = Source(8, root);
        var held = root.Bytes; var previous = held; var admitted = false; var unchanged = OriginalPixels.Bytes(source);
        // The lower failure allowance can select the smaller 64-block prefix
        // before shared admission. Every selected owner must clean up on actual
        // exhaustion; default refusal/cancellation below require the384 cache.
        root.Admitted = used => { if (used - previous is PrefixBytes or (128 * 120 + 16384) or (64 * 120 + 16384)) admitted = true; previous = used; };
        using var failed = new MemoryStream();
        failed.Write([1, 2, 3, 4]);
        // Actual exhaustion can move between stages as compression changes.
        // Caller streams allow partial output; existing content remains owned.
        var failure = Assert.Throws<ImageLimitException>(() => ImageCodec.EncodeWebp(source, failed, Options(root, work: 30500000)));
        Assert.True(admitted); Assert.Equal(ImageLimitKind.Work, failure.Limit); Assert.Equal(ImageLimitScope.Operation, failure.Scope);
        Assert.Equal(new byte[] { 1, 2, 3, 4 }, failed.ToArray()[..4]); Assert.True(failed.CanWrite);
        Assert.Equal(unchanged, OriginalPixels.Bytes(source)); Assert.Equal(held, root.Bytes); Assert.Equal(0, root.Refusals);
        root.Admitted = null;
        using var success = new MemoryStream(); ImageCodec.EncodeWebp(source, success, Options(root));
        Assert.True(success.Length > 0); Assert.Equal(held, root.Bytes); Assert.False(root.IsDisposed);
    }

    [Fact]
    public void RefusedPrefixDoesNotRetryFixedCodingAndReleasingCapacityPermitsSuccess()
    {
        long prefixGrowth = 0;
        using (var trace = new TestMemoryBudget(8000000))
        using (var source = Source(8, trace))
        {
            var previous = trace.Bytes;
            trace.Admitted = used => { if (used - previous == PrefixBytes) prefixGrowth = trace.GrowthAttempts; previous = used; };
            using var packet = new MemoryStream(); ImageCodec.EncodeWebp(source, packet, Options(trace));
        }
        Assert.True(prefixGrowth > 0);
        using var root = new TestMemoryBudget(8000000); using var input = Source(8, root); var held = root.Bytes;
        root.RefuseAtGrowth = prefixGrowth;
        using var failed = new MemoryStream();
        var error = Assert.Throws<ImageLimitException>(() => ImageCodec.EncodeWebp(input, failed, Options(root)));
        Assert.Equal(ImageLimitKind.WorkingBytes, error.Limit); Assert.Equal(ImageLimitScope.SharedBudget, error.Scope);
        Assert.Equal(1, root.Refusals); Assert.Equal(0, failed.Length); Assert.Equal(held, root.Bytes); Assert.True(failed.CanWrite);
        root.RefuseAtGrowth = 0;
        using var success = new MemoryStream(); ImageCodec.EncodeWebp(input, success, Options(root));
        Assert.True(success.Length > 0); Assert.Equal(held, root.Bytes); Assert.False(root.IsDisposed);
    }

    [Fact]
    public void CancellationAfterPrefixAdmissionReleasesTemporaryStorageAndBorrowsTheInput()
    {
        using var root = new TestMemoryBudget(8000000); using var source = Source(8, root);
        using var cancellation = new CancellationTokenSource();
        var unchanged = OriginalPixels.Bytes(source); var held = root.Bytes; var previous = held; var admitted = false; long cancelledAt = 0;
        root.Admitted = used => { if (used - previous == PrefixBytes) { admitted = true; cancelledAt = root.GrowthAttempts; cancellation.Cancel(); } previous = used; };
        using var output = new MemoryStream();
        Assert.Throws<OperationCanceledException>(() => ImageCodec.EncodeWebp(source, output, Options(root), cancellation.Token));
        Assert.True(admitted); Assert.Equal(held, root.Bytes); Assert.Equal(0, output.Length); Assert.True(output.CanWrite);
        Assert.Equal(cancelledAt, root.GrowthAttempts); // No expanded stack reservation after prefix cancellation.
        Assert.Equal(unchanged, OriginalPixels.Bytes(source)); root.Admitted = null;
        ImageCodec.EncodeWebp(source, output, Options(root)); Assert.True(output.Length > 0);
        Assert.Equal(held, root.Bytes); Assert.False(root.IsDisposed);
    }

    [Fact]
    public void RefusedExpandedStackIsTerminalAndCleanupPermitsSubsequentEncoding()
    {
        long stackGrowth = 0;
        using (var trace = new TestMemoryBudget(8000000))
        using (var source = Source(8, trace))
        {
            var previous = trace.Bytes; var prefix = false;
            trace.Admitted = used =>
            {
                if (used - previous == PrefixBytes) prefix = true;
                else if (prefix && stackGrowth == 0 && used - previous == 4096) stackGrowth = trace.GrowthAttempts;
                previous = used;
            };
            using var packet = new MemoryStream(); ImageCodec.EncodeWebp(source, packet, Options(trace));
        }
        Assert.True(stackGrowth > 0);
        using var root = new TestMemoryBudget(8000000); using var input = Source(8, root);
        var original = OriginalPixels.Bytes(input); var held = root.Bytes; root.RefuseAtGrowth = stackGrowth;
        using var failed = new MemoryStream();
        var error = Assert.Throws<ImageLimitException>(() => ImageCodec.EncodeWebp(input, failed, Options(root)));
        Assert.Equal((ImageLimitKind.WorkingBytes, ImageLimitScope.SharedBudget), (error.Limit, error.Scope));
        Assert.Equal(1, root.Refusals); Assert.Equal(held, root.Bytes); Assert.Equal(0, failed.Length); Assert.True(failed.CanWrite);
        Assert.Equal(original, OriginalPixels.Bytes(input)); root.RefuseAtGrowth = 0;
        using var success = new MemoryStream(); ImageCodec.EncodeWebp(input, success, Options(root));
        Assert.True(success.Length > 0); Assert.Equal(held, root.Bytes); Assert.False(root.IsDisposed);
    }

    [Fact]
    public void CancellationAtExpandedStackAdmissionReleasesItsChargeAndPreservesBorrowedOwners()
    {
        using var root = new TestMemoryBudget(8000000); using var input = Source(8, root);
        using var cancellation = new CancellationTokenSource();
        var original = OriginalPixels.Bytes(input); var held = root.Bytes; var previous = held; var prefix = false; var admitted = false;
        root.Admitted = used =>
        {
            if (used - previous == PrefixBytes) prefix = true;
            else if (prefix && used - previous == 4096) { admitted = true; cancellation.Cancel(); }
            previous = used;
        };
        using var failed = new MemoryStream();
        Assert.Throws<OperationCanceledException>(() => ImageCodec.EncodeWebp(input, failed, Options(root), cancellation.Token));
        Assert.True(admitted); Assert.Equal(held, root.Bytes); Assert.Equal(0, failed.Length); Assert.True(failed.CanWrite);
        Assert.Equal(original, OriginalPixels.Bytes(input)); root.Admitted = null;
        using var success = new MemoryStream(); ImageCodec.EncodeWebp(input, success, Options(root));
        Assert.True(success.Length > 0); Assert.Equal(held, root.Bytes); Assert.False(root.IsDisposed);
    }
}
