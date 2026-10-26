using System.Security.Cryptography;
using Lokad.Image.Internal;
using Lokad.Image.Tooling;

namespace Lokad.Image.Tests;

public sealed class Vp8AlphaEncodingTests
{
    [Theory]
    [InlineData(8, 4, 24588, "ca83ebb44c092e661126e81d83a3345877b1de833a2d0bde289217b19d2d25ae", 98590, "8fcc1c935850d7813ebfce329a8b00c41fe9b02683d2be40b91dedfd74192a10")]
    [InlineData(16, 4, 24588, "ca83ebb44c092e661126e81d83a3345877b1de833a2d0bde289217b19d2d25ae", 98590, "8fcc1c935850d7813ebfce329a8b00c41fe9b02683d2be40b91dedfd74192a10")]
    [InlineData(8, 0, 15238, "4753265a0d3567b9b4bce842cf09f6dd174a51b3002d3358ab8fa6243771d15a", 93386, "eb12853d220295a4327d151827462eda69df74bc9a21b938af5ca31f40cf134d")]
    [InlineData(8, 1, 30101, "21ff9bd761842e805268a49eb86b4f31ee605716833fcbc920e04867b12828af", 106588, "9491f97d6c703e0d8869c02eb4ba0edabcde831992f7173cae5ef5b16ab330ce")]
    [InlineData(8, 6, 11667, "9893369a72e99d43c8010e37f10bc33d9aa7a35d85f63faf5324befa6c26cff0", 85796, "e58f431cec4412c38b3ee49da4ff62c25cc896fa71bcffe9f131dd08e1bccc96")]
    [InlineData(8, 2, 29094, "bbd5cc0324b413a166fe362d3f9aa40ecdbc9ce8ceddc1df7dbe0ca77a0220b9", 102582, "e4d5cf3f3c83e2c4c882a6863f1eb3c003b3b5a4647c8169ef8a951148feeefe")]
    [InlineData(8, 3, 28510, "54be30f757cc7e0454f22c448c8a28c992c2b6dd6f2b5e9d1c9291519481d99f", 102502, "5d3ccf6fbbec6308132e7832b757122ba05007706e7786ef8296901862659e93")]
    [InlineData(8, 5, 24387, "f7772abd25c603784e996df2b823b3d2357dd0431a8319b96038c3db8ca48e68", 98602, "2467239de7717bbaef94fdfb2fde2fe872b13c14a1a0e6010b9fc25842df797e")]
    public async Task ExistingTransparentControlMatchesEntireNativePacketWithoutRetainingSource(int depth, int method, int alphaBytes, string alphaHash, int packetBytes, string packetHash)
    {
        using var root = new TestMemoryBudget(128_000_000);
        using var context = new OperationContext(new() { MemoryBudget = root}, default);
        using var source = OriginalVp8lControls.Buffer("asymmetric", depth, root);
        using var input = Vp8PreparedInput.Create(source, context);
        using var alpha = Vp8AlphaEncoding.Create(input.Alpha, input.Width, context, method);
        input.CleanupTransparentArea();
        using var analysis = Vp8SourceAnalysis.Create(input, context, method);
        using var segments = Vp8SourceSegments.Create(analysis, 75, false, context);
        using var quantization = Vp8QuantizationPlan.Create(segments, analysis.MeanChromaScore, context, method);
        using var frame = method <= 2
            ? Vp8FramePlan.CreateDistortion(input, analysis, segments, quantization, method, context)
            : Vp8FramePlan.Create(input, segments, quantization, context, method: method);
        using var packet = Vp8FrameEncoding.Create(frame, context);
        source.Dispose(); input.Dispose(); analysis.Dispose(); segments.Dispose(); quantization.Dispose(); frame.Dispose();
        Assert.Equal(alpha.CapacityBytes + packet.CapacityBytes, root.Bytes);
        using var output = new MemoryStream(); packet.WriteWebp(output, alpha); var bytes = output.ToArray();
        if (Environment.GetEnvironmentVariable("LOKAD_VP8_PACKET_TRACE") is { } path) File.WriteAllBytes(path + "-alpha" + depth + "-m" + method + ".webp", bytes);
        Assert.Equal(alphaBytes, alpha.Bytes.Length); Assert.Equal(5, alpha.Bytes[0]);
        Assert.Equal(alphaHash, Convert.ToHexStringLower(SHA256.HashData(alpha.Bytes)));
        Assert.Equal(packetBytes, packet.WebpLengthWithAlpha(alpha)); Assert.Equal(packetBytes, bytes.Length);
        Assert.Equal(packetHash, Convert.ToHexStringLower(SHA256.HashData(bytes)));
        using var second = new MemoryStream(); await packet.WriteWebpAsync(second, alpha); Assert.Equal(bytes, second.ToArray());
        alpha.Dispose(); packet.Dispose(); Assert.Equal(0, root.Bytes); Assert.True(output.CanWrite); Assert.True(second.CanWrite); Assert.False(root.IsDisposed);
    }

    [Theory]
    [InlineData(4)]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(6)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(5)]
    public void RawSingletonRetainsOnlyItsOwnPayloadAndSurvivesBorrowedInputReplacement(int method)
    {
        using var root = new TestMemoryBudget(1_000_000); using var context = new OperationContext(new() { MemoryBudget = root }, default);
        byte[] input = [37]; using var first = Vp8AlphaEncoding.Create(input, 1, context, method); input[0] = 99;
        Assert.Equal(new byte[] { 0, 37 }, first.Bytes.ToArray()); Assert.Equal(130, root.Bytes);
        using var second = Vp8AlphaEncoding.Create(input, 1, context, method); Assert.Equal(260, root.Bytes); first.Dispose();
        Assert.Equal(130, root.Bytes); Assert.Equal(new byte[] { 0, 99 }, second.Bytes.ToArray());
        first.Dispose(); second.Dispose(); Assert.Equal(0, root.Bytes); Assert.False(root.IsDisposed);
    }

    private static byte[] Ramp() => Enumerable.Range(0, 17 * 9).Select(i => (byte)((i * 17 + i / 17 * 3) & 255)).ToArray();

    [Theory]
    [InlineData(4)]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(6)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(5)]
    public void OwnedScratchAndFinalCandidateRefusalAreTerminalAndRecoveryUsesTheSameRoot(int method)
    {
        var source = Ramp(); var original = source.ToArray(); long peak, last;
        using (var sizingRoot = new TestMemoryBudget(10_000_000))
        using (var sizing = new OperationContext(new() { MemoryBudget = sizingRoot }, default))
        { using var encoded = Vp8AlphaEncoding.Create(source, 17, sizing, method); peak = sizingRoot.Peak; last = sizingRoot.GrowthAttempts; }
        foreach (var phase in new long[] { 1, 2, last })
        {
            using var root = new TestMemoryBudget(10_000_000) { RefuseAtGrowth = phase };
            using var context = new OperationContext(new() { MemoryBudget = root }, default);
            var error = Assert.Throws<ImageLimitException>(() => Vp8AlphaEncoding.Create(source, 17, context, method));
            Assert.Equal((ImageLimitKind.WorkingBytes, ImageLimitScope.SharedBudget), (error.Limit, error.Scope));
            Assert.Equal(1, root.Refusals); Assert.Equal(phase, root.GrowthAttempts); Assert.Equal(0, root.Bytes);
            root.RefuseAtGrowth = 0; using var next = Vp8AlphaEncoding.Create(source, 17, context, method); next.Dispose(); Assert.Equal(0, root.Bytes);
        }
        using var shared = new TestMemoryBudget(peak); using var caller = shared.CreateScope();
        Assert.True(caller.TryReserve(1, out var held)); using var operation = new OperationContext(new() { MemoryBudget = shared }, default);
        Assert.Equal(ImageLimitScope.SharedBudget, Assert.Throws<ImageLimitException>(() => Vp8AlphaEncoding.Create(source, 17, operation, method)).Scope);
        Assert.Equal(1, shared.Bytes); held.Dispose(); using var recovered = Vp8AlphaEncoding.Create(source, 17, operation, method);
        Assert.Equal(original, source); recovered.Dispose(); Assert.Equal(0, shared.Bytes); Assert.False(shared.IsDisposed);
    }

    [Theory]
    [InlineData(4)]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(6)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(5)]
    public void CancellationLocalAndOutputFailuresReleaseAllNewOwnership(int method)
    {
        var source = Ramp();
        using (var sizing = new OperationContext(ImageLimits.Default, default))
        { using var encoded = Vp8AlphaEncoding.Create(source, 17, sizing, method); }
        using var root = new TestMemoryBudget(10_000_000); using var cancel = new CancellationTokenSource();
        using var cancelled = new OperationContext(new() { MemoryBudget = root }, cancel.Token);
        root.Admitted = _ => cancel.Cancel(); Assert.Throws<OperationCanceledException>(() => Vp8AlphaEncoding.Create(source, 17, cancelled, method));
        Assert.Equal(0, root.Bytes); root.Admitted = null;
        foreach (var limits in new ImageLimits[]
        {
            new() { MemoryBudget = root, MaxWorkingBytes = 127 },
            new() { MemoryBudget = root, MaxOutputBytes = 1 }
        })
        {
            using var limited = new OperationContext(limits, default);
            var failure = Assert.Throws<ImageLimitException>(() => Vp8AlphaEncoding.Create(source, 17, limited, method));
            Assert.Equal(ImageLimitScope.Operation, failure.Scope); Assert.Equal(0, root.Bytes);
        }
        using var nextOperation = new OperationContext(new() { MemoryBudget = root }, default);
        using var next = Vp8AlphaEncoding.Create(source, 17, nextOperation, method); next.Dispose(); Assert.Equal(0, root.Bytes);
    }

    [Fact]
    public void InvalidAlphaRowsDoNotAcquireStorage()
    {
        using var root = new TestMemoryBudget(10_000); using var context = new OperationContext(new() { MemoryBudget = root }, default);
        Assert.Throws<ArgumentException>(() => Vp8AlphaEncoding.Create([], 1, context));
        Assert.Throws<ArgumentException>(() => Vp8AlphaEncoding.Create([1, 2], 3, context));
        Assert.Throws<ArgumentException>(() => Vp8AlphaEncoding.Create([1], 0, context));
        Assert.Equal(0, root.Bytes); Assert.Equal(0, root.GrowthAttempts);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task AlphaStorageOutlivesItsActualAsyncUserOnCancellationOrStreamFailure(bool cancellation)
    {
        using var root = new TestMemoryBudget(10_000_000); using var inputOperation = new OperationContext(new() { MemoryBudget = root }, default);
        using var source = OriginalVp8lControls.Buffer("singleton", 8, root); using var input = Vp8PreparedInput.Create(source, inputOperation);
        input.CleanupTransparentArea(); using var analysis = Vp8SourceAnalysis.Create(input, inputOperation);
        using var segments = Vp8SourceSegments.Create(analysis, 75, false, inputOperation);
        using var quantization = Vp8QuantizationPlan.Create(segments, analysis.MeanChromaScore, inputOperation);
        using var frame = Vp8FramePlan.Create(input, segments, quantization, inputOperation); var baseline = root.Bytes;
        using var cancel = new CancellationTokenSource(); using var operation = new OperationContext(new() { MemoryBudget = root }, cancel.Token);
        using var output = new BlockedAlphaStream(cancellation);
        async Task Deliver()
        {
            using var alpha = Vp8AlphaEncoding.Create(input.Alpha, 1, operation);
            using var encoded = Vp8FrameEncoding.Create(frame, operation); await encoded.WriteWebpAsync(output, alpha);
        }
        var delivery = Deliver(); await output.Entered.Task.WaitAsync(TimeSpan.FromSeconds(5)); var admitted = root.Bytes;
        Assert.True(admitted > baseline); using var caller = root.CreateScope(); Assert.True(caller.TryReserve(10_000_000 - admitted - 127, out var held));
        Assert.Equal(ImageLimitScope.SharedBudget, Assert.Throws<ImageLimitException>(() => Vp8AlphaEncoding.Create(input.Alpha, 1, inputOperation)).Scope);
        Assert.Equal(admitted + held.Bytes, root.Bytes); held.Dispose();
        if (cancellation) cancel.Cancel(); await Task.Yield(); Assert.False(delivery.IsCompleted); Assert.Equal(admitted, root.Bytes);
        output.Release.TrySetResult();
        if (cancellation) await Assert.ThrowsAnyAsync<OperationCanceledException>(() => delivery);
        else await Assert.ThrowsAsync<IOException>(() => delivery);
        Assert.Equal(baseline, root.Bytes); Assert.False(root.IsDisposed); Assert.False(output.WasDisposed);
    }

    [Fact]
    public void CompleteContainerLimitAndDimensionMismatchFailBeforeWriting()
    {
        using var root = new TestMemoryBudget(10_000_000); using var operation = new OperationContext(new() { MemoryBudget = root }, default);
        using var source = OriginalVp8lControls.Buffer("singleton", 8, root); using var input = Vp8PreparedInput.Create(source, operation);
        input.CleanupTransparentArea(); using var analysis = Vp8SourceAnalysis.Create(input, operation);
        using var segments = Vp8SourceSegments.Create(analysis, 75, false, operation);
        using var quantization = Vp8QuantizationPlan.Create(segments, analysis.MeanChromaScore, operation);
        using var frame = Vp8FramePlan.Create(input, segments, quantization, operation);
        using var alpha = Vp8AlphaEncoding.Create(input.Alpha, 1, operation); using var encoded = Vp8FrameEncoding.Create(frame, operation);
        using var limited = new OperationContext(new() { MemoryBudget = root, MaxOutputBytes = encoded.WebpLength }, default);
        using var narrow = Vp8FrameEncoding.Create(frame, limited); var retained = root.Bytes; using var output = new MemoryStream();
        Assert.Equal(ImageLimitKind.OutputBytes, Assert.Throws<ImageLimitException>(() => narrow.WriteWebp(output, alpha)).Limit);
        Assert.Equal(0, output.Length); Assert.Equal(retained, root.Bytes);
        using var mismatched = Vp8AlphaEncoding.Create([0, 1], 2, operation);
        Assert.Throws<ArgumentException>(() => encoded.WriteWebp(output, mismatched)); Assert.Equal(0, output.Length);
    }

    private sealed class BlockedAlphaStream(bool cancellation) : Stream
    {
        internal readonly TaskCompletionSource Entered = new(TaskCreationOptions.RunContinuationsAsynchronously);
        internal readonly TaskCompletionSource Release = new(TaskCreationOptions.RunContinuationsAsynchronously);
        internal bool WasDisposed; private int writes;
        public override async ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
        {
            if (++writes == 2)
            {
                Entered.TrySetResult(); await Release.Task.ConfigureAwait(false);
                if (!cancellation) throw new IOException("Original failure probe.");
            }
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
