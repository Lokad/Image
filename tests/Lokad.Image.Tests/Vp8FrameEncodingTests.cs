using System.Security.Cryptography;
using Lokad.Image.Internal;
using Lokad.Image.Tooling;

namespace Lokad.Image.Tests;

public sealed class Vp8FrameEncodingTests
{
    private sealed class Inputs : IDisposable
    {
        internal readonly OperationContext Context;
        private readonly ImageBuffer image;
        internal readonly Vp8PreparedInput Prepared;
        internal readonly Vp8SourceAnalysis Analysis;
        internal readonly Vp8SourceSegments Segments;
        internal readonly Vp8QuantizationPlan Quantization;
        internal Inputs(string name, TestMemoryBudget root)
        {
            Context = new(new() { MemoryBudget = root }, default); image = OriginalVp8lControls.Buffer(name, 8, root);
            // The existing original singleton color, made opaque for lifecycle
            // checks. It adds no benchmark/compression control or size claim.
            if (name == "singleton") image.GetRow8(0)[3] = 255;
            Prepared = Vp8PreparedInput.Create(image, Context); Analysis = Vp8SourceAnalysis.Create(Prepared, Context);
            Segments = Vp8SourceSegments.Create(Analysis, 75, false, Context); Quantization = Vp8QuantizationPlan.Create(Segments, Analysis.MeanChromaScore, Context);
        }
        internal Vp8FramePlan Frame(OperationContext? operation = null) => Vp8FramePlan.Create(Prepared, Segments, Quantization, operation ?? Context);
        internal void ReleaseInputs() { Quantization.Dispose(); Segments.Dispose(); Analysis.Dispose(); Prepared.Dispose(); image.Dispose(); }
        public void Dispose() { ReleaseInputs(); Context.Dispose(); }
    }

    [Theory]
    [InlineData("opaque", 72380, "39eb4095d1aaebc21fee2a92aa45180fb6adcd92bc5538589f914f53aa9ed88f")]
    [InlineData("diagram", 7100, "0b71843059a6c3224cec9df38a460ce6087ad21e1e0f499978f7767747a1a050")]
    [InlineData("photograph", 17688, "e2df1f461cb04516aa8630f326efdc959e1ae8a7dce578ef3d064b20c788719b")]
    public async Task CompletePacketMatchesNativeWithIndependentSourceAndOutputLifetimes(string name, int bytes, string expected)
    {
        using var root = new TestMemoryBudget(16_000_000); using var input = new Inputs(name, root); using var frame = input.Frame();
        Assert.Equal(0, frame.HeaderRetries); Assert.Equal(frame.Count * 820L + 1376, frame.CapacityBytes);
        using var packet = Vp8FrameEncoding.Create(frame, input.Context); input.ReleaseInputs(); frame.Dispose();
        Assert.Equal(packet.CapacityBytes, root.Bytes); Assert.Equal(bytes, packet.WebpLength);
        using var sync = new MemoryStream(); packet.WriteOpaqueWebp(sync); var first = sync.ToArray();
        if (Environment.GetEnvironmentVariable("LOKAD_VP8_PACKET_TRACE") is { } path) File.WriteAllBytes(path + "-" + name + ".webp", first);
        using var asynchronous = new MemoryStream(); await packet.WriteOpaqueWebpAsync(asynchronous); var second = asynchronous.ToArray();
        Assert.Equal(bytes, first.Length); Assert.Equal(expected, Convert.ToHexStringLower(SHA256.HashData(first))); Assert.Equal(first, second);
        Assert.NotSame(first, second); packet.Dispose(); packet.Dispose(); Assert.Equal(0, root.Bytes);
        Assert.Equal(expected, Convert.ToHexStringLower(SHA256.HashData(first))); Assert.True(sync.CanWrite); Assert.True(asynchronous.CanWrite); Assert.False(root.IsDisposed);
    }

    [Fact]
    public void RetainedFrameAndReplacementUseOneRootAndReleaseIndependentlyOfInputs()
    {
        using var root = new TestMemoryBudget(16_000_000); using var input = new Inputs("singleton", root); using var first = input.Frame();
        var original = first.Coefficients.ToArray(); using var caller = root.CreateScope();
        Assert.True(caller.TryReserve(16_000_000 - root.Bytes - first.CapacityBytes + 1, out var held)); var baseline = root.Bytes;
        var error = Assert.Throws<ImageLimitException>(() => input.Frame()); Assert.Equal(ImageLimitScope.SharedBudget, error.Scope);
        Assert.Equal(baseline, root.Bytes); Assert.Equal(original, first.Coefficients.ToArray()); held.Dispose();
        using var second = input.Frame(); input.ReleaseInputs(); Assert.Equal(first.CapacityBytes + second.CapacityBytes, root.Bytes);
        first.Dispose(); Assert.Equal(second.CapacityBytes, root.Bytes); Assert.Equal(original, second.Coefficients.ToArray());
        Assert.Equal(1056, second.Probabilities.Length); Assert.Equal(4, second.Quantizers.Length); Assert.Equal(4, second.FilterLevels.Length);
        second.Dispose(); Assert.Equal(0, root.Bytes); Assert.False(root.IsDisposed); Assert.Throws<ObjectDisposedException>(() => second.Coefficients.Length);
    }

    [Fact]
    public void OwnedAndTransientFrameRefusalsAreTerminalAndAllowSubsequentOperations()
    {
        foreach (var available in new long[] { 2195, 2196 + 25265 - 1, 2196 + 25265 + 20192 - 1 })
        {
            using var root = new TestMemoryBudget(16_000_000); using var input = new Inputs("singleton", root); using var caller = root.CreateScope();
            Assert.True(caller.TryReserve(16_000_000 - root.Bytes - available, out var held)); var baseline = root.Bytes;
            var refusals = root.Refusals; var error = Assert.Throws<ImageLimitException>(() => input.Frame());
            Assert.Equal((ImageLimitKind.WorkingBytes, ImageLimitScope.SharedBudget), (error.Limit, error.Scope));
            Assert.Equal(refusals + 1, root.Refusals); Assert.Equal(baseline, root.Bytes); held.Dispose();
            using var next = input.Frame(); next.Dispose(); Assert.False(root.IsDisposed);
        }
    }

    [Fact]
    public void FrameCancellationAndLateWorkFailureRefundAllNewOwnership()
    {
        using var root = new TestMemoryBudget(16_000_000); using var input = new Inputs("singleton", root); var baseline = root.Bytes;
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
