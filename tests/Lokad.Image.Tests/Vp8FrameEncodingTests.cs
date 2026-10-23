using System.Security.Cryptography;
using Lokad.Image.Internal;
using Lokad.Image.Tooling;

namespace Lokad.Image.Tests;

public sealed class Vp8FrameEncodingTests
{
    [Fact]
    public void MergedSegmentPaddingMatchesTheFrozenNativeHeaderAndPreservesActiveDescriptors()
    {
        // The named5632square control has three live segments. The last source
        // descriptor repeats segment2; frame syntax instead refreshes its Q from
        // segment0. The resulting Q/filter fields are read from the frozen packet.
        Vp8SourceSegments.Segment[] source = [new(0, 0, 0, 26, 8), new(0, 0, 128, 17, 4), new(0, 0, 255, 15, 2), new(0, 0, 255, 15, 2)];
        var original = source.ToArray(); Span<int> values = stackalloc int[12]; values.Fill(99);
        Vp8FramePlan.FillHeaderParameters(source, 3, values);
        Assert.Equal(new[] { 26, 17, 15, 26, 8, 4, 2, 4 }, values[..8].ToArray());
        Assert.Equal(new[] { 99, 99, 99, 99 }, values[8..].ToArray()); Assert.Equal(original, source);
    }

    private sealed class Inputs : IDisposable
    {
        internal readonly OperationContext Context;
        private readonly ImageBuffer image;
        internal readonly Vp8PreparedInput Prepared;
        internal readonly Vp8SourceAnalysis Analysis;
        internal readonly Vp8SourceSegments Segments;
        internal readonly Vp8QuantizationPlan Quantization;
        private readonly int method, quality;
        internal Inputs(string name, TestMemoryBudget root, int method = 4, int quality = 75)
        {
            this.method = method; this.quality = quality;
            Context = new(new() { MemoryBudget = root }, default); image = OriginalVp8lControls.Buffer(name, 8, root);
            // The existing original singleton color, made opaque for lifecycle
            // checks. It adds no benchmark/compression control or size claim.
            if (name == "singleton") image.GetRow8(0)[3] = 255;
            Prepared = Vp8PreparedInput.Create(image, Context); Analysis = Vp8SourceAnalysis.Create(Prepared, Context, method, quality);
            Segments = Vp8SourceSegments.Create(Analysis, quality, false, Context); Quantization = Vp8QuantizationPlan.Create(Segments, Analysis.MeanChromaScore, Context, method);
        }
        internal Vp8FramePlan Frame(OperationContext? operation = null) => method <= 2
            ? Vp8FramePlan.CreateDistortion(Prepared, Analysis, Segments, Quantization, method, operation ?? Context, correctDc: quality <= 98)
            : Vp8FramePlan.Create(Prepared, Segments, Quantization, operation ?? Context, correctDc: quality <= 98, method: method);
        internal void ReleaseInputs() { Quantization.Dispose(); Segments.Dispose(); Analysis.Dispose(); Prepared.Dispose(); image.Dispose(); }
        public void Dispose() { ReleaseInputs(); Context.Dispose(); }
    }

    [Theory]
    [InlineData("opaque", 72380, "39eb4095d1aaebc21fee2a92aa45180fb6adcd92bc5538589f914f53aa9ed88f", 4, 75)]
    [InlineData("diagram", 7100, "0b71843059a6c3224cec9df38a460ce6087ad21e1e0f499978f7767747a1a050", 4, 75)]
    [InlineData("diagram", 7082, "3bea8726b4473a4f1436a621229d12a489603537ebbd54945044241d3d478dda", 3, 75)]
    [InlineData("photograph", 17688, "e2df1f461cb04516aa8630f326efdc959e1ae8a7dce578ef3d064b20c788719b", 4, 75)]
    [InlineData("diagram", 9980, "1c454bdfaff968c856a479c9828f8396137556c0708354c85a5a9b37d2bbe5d5", 0, 75)]
    [InlineData("diagram", 9244, "50d50af60b76e99dcb1f49eda60efc548a564dbaaaa931d06fc0164aa28c0bea", 1, 75)]
    [InlineData("diagram", 7330, "325a9d5b55b300db2554681735642edcdc8dcb866a88760501d3fe95fac1423b", 2, 75)]
    [InlineData("diagram", 27668, "eb4cd33ce7a5b39698b49fd94b4dd59a7ec9c0a0d8d5ac0bf12b10d8a0afbdba", 0, 100)]
    [InlineData("diagram", 2304, "28bff919826e67d69d393f457d3920017ffff21cc3732e3db06d55a80f873f54", 4, 1)]
    [InlineData("diagram", 8898, "51a4abeda30c4f8c8b1dea563ab6690a89a5feed93b69c467398f9b736aeb536", 4, 85)]
    [InlineData("diagram", 15172, "6a4332bb32c3b93248a76eb804a0dc74d2c519d4955f310dcb94ef798cd2616e", 4, 100)]
    [InlineData("diagram", 7026, "0e994ebda4b10234f2e9f00caf7ea6c27ae03ce82c1f3cada8eda355430cba68", 5, 75)]
    [InlineData("diagram", 6818, "3c98407656841ff97b185e241e76f750f036c972cfe2a0b704ae2eb35cf0adee", 6, 75)]
    public async Task CompletePacketMatchesNativeWithIndependentSourceAndOutputLifetimes(string name, int bytes, string expected, int method, int quality)
    {
        using var root = new TestMemoryBudget(16_000_000); using var input = new Inputs(name, root, method, quality); using var frame = input.Frame();
        Assert.Equal(0, frame.HeaderRetries); Assert.Equal(frame.Count * 820L + 1376, frame.CapacityBytes);
        using var packet = Vp8FrameEncoding.Create(frame, input.Context); input.ReleaseInputs(); frame.Dispose();
        Assert.Equal(packet.CapacityBytes, root.Bytes); Assert.Equal(bytes, packet.WebpLength);
        using var sync = new MemoryStream(); packet.WriteOpaqueWebp(sync); var first = sync.ToArray();
        if (Environment.GetEnvironmentVariable("LOKAD_VP8_PACKET_TRACE") is { } path) File.WriteAllBytes(path + "-" + name + "-q" + quality + "-m" + method + ".webp", first);
        using var asynchronous = new MemoryStream(); await packet.WriteOpaqueWebpAsync(asynchronous); var second = asynchronous.ToArray();
        Assert.Equal(bytes, first.Length); Assert.Equal(expected, Convert.ToHexStringLower(SHA256.HashData(first))); Assert.Equal(first, second);
        Assert.NotSame(first, second); packet.Dispose(); packet.Dispose(); Assert.Equal(0, root.Bytes);
        Assert.Equal(expected, Convert.ToHexStringLower(SHA256.HashData(first))); Assert.True(sync.CanWrite); Assert.True(asynchronous.CanWrite); Assert.False(root.IsDisposed);
    }

    [Theory]
    [InlineData(4)]
    [InlineData(5)]
    [InlineData(6)]
    public void RetainedFrameAndReplacementUseOneRootAndReleaseIndependentlyOfInputs(int method)
    {
        using var root = new TestMemoryBudget(16_000_000); using var input = new Inputs("singleton", root, method); using var first = input.Frame();
        var original = first.Coefficients.ToArray(); using var caller = root.CreateScope();
        Assert.True(caller.TryReserve(16_000_000 - root.Bytes - first.CapacityBytes + 1, out var held)); var baseline = root.Bytes;
        var error = Assert.Throws<ImageLimitException>(() => input.Frame()); Assert.Equal(ImageLimitScope.SharedBudget, error.Scope);
        Assert.Equal(baseline, root.Bytes); Assert.Equal(original, first.Coefficients.ToArray()); held.Dispose();
        using var second = input.Frame(); input.ReleaseInputs(); Assert.Equal(first.CapacityBytes + second.CapacityBytes, root.Bytes);
        first.Dispose(); Assert.Equal(second.CapacityBytes, root.Bytes); Assert.Equal(original, second.Coefficients.ToArray());
        Assert.Equal(1056, second.Probabilities.Length); Assert.Equal(4, second.Quantizers.Length); Assert.Equal(4, second.FilterLevels.Length);
        second.Dispose(); Assert.Equal(0, root.Bytes); Assert.False(root.IsDisposed); Assert.Throws<ObjectDisposedException>(() => second.Coefficients.Length);
    }

    [Theory]
    [InlineData(4)]
    [InlineData(5)]
    [InlineData(6)]
    public void OwnedAndTransientFrameRefusalsAreTerminalAndAllowSubsequentOperations(int method)
    {
        foreach (var available in new long[] { 2195, 2196 + 25265 - 1, 2196 + 25265 + 20192 - 1 })
        {
            using var root = new TestMemoryBudget(16_000_000); using var input = new Inputs("singleton", root, method); using var caller = root.CreateScope();
            Assert.True(caller.TryReserve(16_000_000 - root.Bytes - available, out var held)); var baseline = root.Bytes;
            var refusals = root.Refusals; var error = Assert.Throws<ImageLimitException>(() => input.Frame());
            Assert.Equal((ImageLimitKind.WorkingBytes, ImageLimitScope.SharedBudget), (error.Limit, error.Scope));
            Assert.Equal(refusals + 1, root.Refusals); Assert.Equal(baseline, root.Bytes); held.Dispose();
            using var next = input.Frame(); next.Dispose(); Assert.False(root.IsDisposed);
        }
    }

    [Theory]
    [InlineData(4)]
    [InlineData(5)]
    [InlineData(6)]
    public void FrameCancellationAndLateWorkFailureRefundAllNewOwnership(int method)
    {
        using var root = new TestMemoryBudget(16_000_000); using var input = new Inputs("singleton", root, method); var baseline = root.Bytes;
        using var cancel = new CancellationTokenSource(); using var cancelled = new OperationContext(new() { MemoryBudget = root }, cancel.Token);
        root.Admitted = _ => cancel.Cancel(); Assert.Throws<OperationCanceledException>(() => input.Frame(cancelled));
        Assert.Equal(baseline, root.Bytes); root.Admitted = null;
        long work; using (var sizing = new OperationContext(new() { MemoryBudget = root }, default)) using (var frame = input.Frame(sizing)) work = sizing.ConsumedWork;
        using var limited = new OperationContext(new() { MemoryBudget = root, MaxWork = work - 1 }, default);
        Assert.Equal(ImageLimitKind.Work, Assert.Throws<ImageLimitException>(() => input.Frame(limited)).Limit); Assert.Equal(baseline, root.Bytes);
        using var local = new OperationContext(new() { MemoryBudget = root, MaxWorkingBytes = 2195 }, default);
        var error = Assert.Throws<ImageLimitException>(() => input.Frame(local)); Assert.Equal(ImageLimitScope.Operation, error.Scope); Assert.Equal(baseline, root.Bytes);
        using var next = input.Frame(); next.Dispose(); Assert.Equal(baseline, root.Bytes);
    }

    [Fact]
    public void PartitionCreationRefusalAndFinalOutputLimitReleaseEveryBuffer()
    {
        using var root = new TestMemoryBudget(16_000_000); using var input = new Inputs("singleton", root); using var frame = input.Frame();
        using var caller = root.CreateScope(); Assert.True(caller.TryReserve(16_000_000 - root.Bytes - 1740, out var held)); var baseline = root.Bytes;
        var error = Assert.Throws<ImageLimitException>(() => Vp8FrameEncoding.Create(frame, input.Context));
        Assert.Equal(ImageLimitScope.SharedBudget, error.Scope); Assert.Equal(baseline, root.Bytes); held.Dispose(); baseline = root.Bytes;
        using var limited = new OperationContext(new() { MemoryBudget = root, MaxOutputBytes = 20 }, default);
        Assert.Equal(ImageLimitKind.OutputBytes, Assert.Throws<ImageLimitException>(() => Vp8FrameEncoding.Create(frame, limited)).Limit);
        Assert.Equal(baseline, root.Bytes); using var next = Vp8FrameEncoding.Create(frame, input.Context); next.Dispose(); Assert.Equal(baseline, root.Bytes);
    }

    [Fact]
    public async Task CancellationWaitsForTheActualAsyncBufferUserBeforeReleasingAdmission()
    {
        using var root = new TestMemoryBudget(16_000_000); using var input = new Inputs("singleton", root); using var frame = input.Frame();
        using var cancel = new CancellationTokenSource(); using var operation = new OperationContext(new() { MemoryBudget = root }, cancel.Token);
        using var output = new BlockedPartitionStream(); var baseline = root.Bytes;
        async Task Deliver()
        {
            using var encoded = Vp8FrameEncoding.Create(frame, operation); await encoded.WriteOpaqueWebpAsync(output);
        }
        var delivery = Deliver(); await output.Entered.Task.WaitAsync(TimeSpan.FromSeconds(5)); var admitted = root.Bytes;
        Assert.True(admitted > baseline); using var caller = root.CreateScope(); Assert.True(caller.TryReserve(16_000_000 - admitted - 511, out var held));
        Assert.Equal(ImageLimitScope.SharedBudget, Assert.Throws<ImageLimitException>(() => Vp8FrameEncoding.Create(frame, input.Context)).Scope);
        Assert.Equal(admitted + held.Bytes, root.Bytes); held.Dispose(); cancel.Cancel(); await Task.Yield();
        Assert.False(delivery.IsCompleted); Assert.Equal(admitted, root.Bytes);
        output.Release.TrySetResult(); await Assert.ThrowsAnyAsync<OperationCanceledException>(() => delivery);
        Assert.Equal(baseline, root.Bytes); Assert.False(output.WasDisposed); Assert.False(root.IsDisposed);
    }

    [Fact]
    public void FastFrameRetainsItsOwnStorageAndAccountsForReplacementPeaks()
    {
        using var root = new TestMemoryBudget(16_000_000); using var input = new Inputs("singleton", root, 0); using var first = input.Frame();
        var original = first.Coefficients.ToArray(); using var caller = root.CreateScope();
        Assert.True(caller.TryReserve(16_000_000 - root.Bytes - first.CapacityBytes + 1, out var held)); var baseline = root.Bytes;
        Assert.Equal(ImageLimitScope.SharedBudget, Assert.Throws<ImageLimitException>(() => input.Frame()).Scope);
        Assert.Equal(baseline, root.Bytes); Assert.Equal(original, first.Coefficients.ToArray()); held.Dispose();
        using var second = input.Frame(); input.ReleaseInputs(); Assert.Equal(first.CapacityBytes + second.CapacityBytes, root.Bytes);
        first.Dispose(); Assert.Equal(second.CapacityBytes, root.Bytes); Assert.Equal(original, second.Coefficients.ToArray());
        second.Dispose(); Assert.Equal(0, root.Bytes); Assert.False(root.IsDisposed);
    }

    [Fact]
    public void FastFrameRefusalsStopWithoutRetryAndRefundOwnedAndTransientStorage()
    {
        // Owned frame, rolling/scratch arrays, then the nested rate owner.
        foreach (var available in new long[] { 2195, 2196 + 25261 - 1, 2196 + 25261 + 20192 - 1 })
        {
            using var root = new TestMemoryBudget(16_000_000); using var input = new Inputs("singleton", root, 0); using var caller = root.CreateScope();
            Assert.True(caller.TryReserve(16_000_000 - root.Bytes - available, out var held)); var baseline = root.Bytes;
            var refusals = root.Refusals; var error = Assert.Throws<ImageLimitException>(() => input.Frame());
            Assert.Equal((ImageLimitKind.WorkingBytes, ImageLimitScope.SharedBudget), (error.Limit, error.Scope));
            Assert.Equal(refusals + 1, root.Refusals); Assert.Equal(baseline, root.Bytes); held.Dispose();
            using var next = input.Frame(); next.Dispose(); Assert.False(root.IsDisposed);
        }
    }

    [Fact]
    public void FastFrameCancellationAndLateWorkFailureReleaseAllNewCharges()
    {
        using var root = new TestMemoryBudget(16_000_000); using var input = new Inputs("singleton", root, 0); var baseline = root.Bytes;
        using var cancel = new CancellationTokenSource(); using var cancelled = new OperationContext(new() { MemoryBudget = root }, cancel.Token);
        root.Admitted = _ => cancel.Cancel(); Assert.Throws<OperationCanceledException>(() => input.Frame(cancelled));
        Assert.Equal(baseline, root.Bytes); root.Admitted = null;
        long work; using (var sizing = new OperationContext(new() { MemoryBudget = root }, default)) using (var frame = input.Frame(sizing)) work = sizing.ConsumedWork;
        using var limited = new OperationContext(new() { MemoryBudget = root, MaxWork = work - 1 }, default);
        Assert.Equal(ImageLimitKind.Work, Assert.Throws<ImageLimitException>(() => input.Frame(limited)).Limit); Assert.Equal(baseline, root.Bytes);
        using var local = new OperationContext(new() { MemoryBudget = root, MaxWorkingBytes = 2195 }, default);
        Assert.Equal(ImageLimitScope.Operation, Assert.Throws<ImageLimitException>(() => input.Frame(local)).Scope); Assert.Equal(baseline, root.Bytes);
        using var next = input.Frame(); next.Dispose(); Assert.Equal(baseline, root.Bytes);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void SkipClearsCoefficientEdgesAndPreservesY2OnlyForIntra4(bool intra16)
    {
        var contexts = Enumerable.Range(1, 23).Select(i => (byte)i).ToArray(); var original = contexts.ToArray();
        Vp8FrameEncoding.ResetAfterSkip(contexts, intra16);
        for (var i = 0; i < 18; i++) Assert.Equal(!intra16 && i is 8 or 17 ? original[i] : (byte)0, contexts[i]);
        Assert.Equal(original[18..], contexts[18..]);
        var shortContexts = original[..17]; Assert.Throws<ArgumentException>(() => Vp8FrameEncoding.ResetAfterSkip(shortContexts, intra16));
        Assert.Equal(original[..17], shortContexts);
    }

    private sealed class BlockedPartitionStream : Stream
    {
        internal readonly TaskCompletionSource Entered = new(TaskCreationOptions.RunContinuationsAsynchronously);
        internal readonly TaskCompletionSource Release = new(TaskCreationOptions.RunContinuationsAsynchronously);
        internal bool WasDisposed; private int writes;
        public override async ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
        {
            if (++writes == 3) { Entered.TrySetResult(); await Release.Task.ConfigureAwait(false); }
            cancellationToken.ThrowIfCancellationRequested();
        }
        protected override void Dispose(bool disposing) { WasDisposed = true; base.Dispose(disposing); }
        public override bool CanRead => false; public override bool CanSeek => false; public override bool CanWrite => true;
        public override long Length => throw new NotSupportedException(); public override long Position { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }
        public override void Flush() { }
        public override int Read(byte[] buffer, int offset, int count) => throw new NotSupportedException();
        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
        public override void SetLength(long value) => throw new NotSupportedException();
        public override void Write(byte[] buffer, int offset, int count) => throw new InvalidOperationException("Only asynchronous writes are expected.");
    }
}
