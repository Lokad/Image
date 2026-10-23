using System.Buffers.Binary;
using System.Security.Cryptography;
using Lokad.Image.Internal;
using Lokad.Image.Tooling;

namespace Lokad.Image.Tests;

public sealed class Vp8SourceAnalysisTests
{
    public static IEnumerable<object[]> Controls()
    {
        yield return ["opaque", "315225a22ef50787a8c757b7651f2de8a4d24410b7d104fd5da7a61301723601", "ddbabdd395643a9a39b91d364c57d1fbc6035271993a540d90eac942cf6bbf5b", "15400b8a8a7f4562fdac76ba73f60dd3c48b849c51ea84a276a2fc79913cd1ca", 11, 610];
        yield return ["photograph", "735fd31b29295bc3843af3f1821a9b6b6eeb88d8153282730e2fa03db2cc7301", "57ea731fdb7d35168ee52e4f47dd85e92d7e183ad72e5f886a713ee8fe7eda21", "4540eb8c02a1fb198e3aa2052dfc01c5cb5f8ada9ec3b256cf9bc7b0bf55acad", 172, 38];
        yield return ["diagram", "e79d71b00f0a209e273c7fda7bbaf15f7b0ebd37388d036cb48b85a7508bd77b", "a0bc6e12d2b5f21ea22e6fea51b5518b748e2f2ffeee276db77223bcc6f25f7d", "17535e17d4998767b7c4ec8c67f2b0d6db34d2baeff5d3d0d4f3355f62aa670d", 213, 42];
        yield return ["asymmetric", "07ba7bde4b73e4ed1402651738630526acd3fbb1a9c8370adb8cfa9b26cabb0b", "7d3a61709ed2ca311b451c5043b52c7eead48f8c6fff081b70764801ff8adf38", "2b7e77fe1257f4173f2723c2e0ecb69cebb941ae3a4f5b763b6aa790c741dd47", 11, 682];
    }

    private static string RecordsHash(Vp8SourceAnalysis analysis)
    {
        using var hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
        Span<byte> record = stackalloc byte[3];
        for (var at = 0; at < analysis.Count; at++)
        {
            record[0] = analysis.Alpha[at]; record[1] = analysis.LumaModes[at]; record[2] = analysis.ChromaModes[at];
            hash.AppendData(record);
        }
        return Convert.ToHexStringLower(hash.GetHashAndReset());
    }

    [Theory]
    [MemberData(nameof(Controls))]
    public void NativeCandidateShapesAndDerivedAnalysisMatchAndRetainNoSource(string name, string shapes,
        string records, string histogram, int alpha, int uv)
    {
        using var root = new TestMemoryBudget(16_000_000); using var source = OriginalVp8lControls.Buffer(name, 8, root);
        using var context = new OperationContext(new() { MemoryBudget = root }, default);
        var sourceBytes = source.Pixels.Length; context.ReserveBorrowed(sourceBytes);
        using var input = Vp8PreparedInput.Create(source, context); input.CleanupTransparentArea();
        var inputHash = Vp8PreparedInputTests.Hash(input); var before = root.Bytes;
        using var analysis = Vp8SourceAnalysis.Create(input, context);
        Assert.Equal((33, 17, 561), (analysis.Width, analysis.Height, analysis.Count));
        Assert.Equal(records, RecordsHash(analysis)); Assert.Equal((alpha, uv), (analysis.MeanAlpha, analysis.MeanChromaScore));
        Assert.Equal(before + analysis.CapacityBytes, root.Bytes); Assert.Equal(root.Bytes + 1024, root.Peak);
        Span<byte> packedHistogram = stackalloc byte[1024];
        for (var i = 0; i < 256; i++) BinaryPrimitives.WriteInt32LittleEndian(packedHistogram[(4 * i)..], analysis.Histogram[i]);
        Assert.Equal(histogram, Convert.ToHexStringLower(SHA256.HashData(packedHistogram)));
        Assert.Equal(561, analysis.Histogram.ToArray().Sum());

        context.Reserve(1024);
        try
        {
            using var hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
            Span<byte> prediction = stackalloc byte[256]; Span<int> population = stackalloc int[32]; Span<byte> shape = stackalloc byte[16];
            for (var y = 0; y < analysis.Height; y++)
            for (var x = 0; x < analysis.Width; x++)
            for (var mode = 0; mode < 2; mode++)
            {
                var candidate = Vp8SourceAnalysis.Measure(input, x, y, (Vp8IntraMode)mode, prediction, population, context);
                BinaryPrimitives.WriteInt32LittleEndian(shape, candidate.Luma.PeakPopulation);
                BinaryPrimitives.WriteInt32LittleEndian(shape[4..], candidate.Luma.LastBin);
                BinaryPrimitives.WriteInt32LittleEndian(shape[8..], candidate.Chroma.PeakPopulation);
                BinaryPrimitives.WriteInt32LittleEndian(shape[12..], candidate.Chroma.LastBin);
                hash.AppendData(shape);
            }
            Assert.Equal(shapes, Convert.ToHexStringLower(hash.GetHashAndReset()));
        }
        finally { context.Release(1024); }
        Assert.Equal(inputHash, Vp8PreparedInputTests.Hash(input));
        source.Dispose(); context.ReleaseBorrowed(sourceBytes); input.Dispose();
        Assert.Equal(analysis.CapacityBytes, root.Bytes); Assert.Equal(records, RecordsHash(analysis));
        analysis.Dispose(); analysis.Dispose(); Assert.Equal(0, root.Bytes); Assert.False(root.IsDisposed);
        Assert.Throws<ObjectDisposedException>(() => analysis.Alpha.Length);
        Assert.Throws<ObjectDisposedException>(() => analysis.Histogram.Length);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void AggregateRefusalAtResultOrScratchIsTerminalAndRecovers(bool scratch)
    {
        using var source = OriginalVp8lControls.Buffer("singleton", 8);
        using var root = new TestMemoryBudget(16_000_000); using var caller = root.CreateScope();
        using var context = new OperationContext(new() { MemoryBudget = root }, default);
        using var input = Vp8PreparedInput.Create(source, context);
        long capacity;
        using (var sizing = Vp8SourceAnalysis.Create(input, context)) capacity = sizing.CapacityBytes;
        var free = scratch ? capacity + 1023 : capacity - 1;
        Assert.True(caller.TryReserve(16_000_000 - root.Bytes - free, out var held));
        var before = root.Bytes;
        var error = Assert.Throws<ImageLimitException>(() => Vp8SourceAnalysis.Create(input, context));
        Assert.Equal((ImageLimitKind.WorkingBytes, ImageLimitScope.SharedBudget), (error.Limit, error.Scope));
        Assert.Equal(before, root.Bytes); Assert.Equal(1, root.Refusals); held.Dispose();
        using var next = Vp8SourceAnalysis.Create(input, context);
        Assert.Equal(input.CapacityBytes + capacity, root.Bytes);
        next.Dispose(); input.Dispose(); Assert.Equal(0, root.Bytes); Assert.False(root.IsDisposed);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    public void CancellationAfterResultOrScratchAdmissionRefundsAndAllowsSubsequentWork(int growth)
    {
        using var source = OriginalVp8lControls.Buffer("singleton", 8);
        using var root = new TestMemoryBudget(16_000_000); using var cancel = new CancellationTokenSource();
        using var context = new OperationContext(new() { MemoryBudget = root }, cancel.Token);
        using var input = Vp8PreparedInput.Create(source, context); var baseline = root.Bytes; var initial = root.GrowthAttempts;
        root.Admitted = _ => { if (root.GrowthAttempts == initial + growth) cancel.Cancel(); };
        Assert.Throws<OperationCanceledException>(() => Vp8SourceAnalysis.Create(input, context));
        Assert.Equal(baseline, root.Bytes); root.Admitted = null;
        using var later = new OperationContext(new() { MemoryBudget = root }, default);
        using var next = Vp8SourceAnalysis.Create(input, later); next.Dispose();
        Assert.Equal(baseline, root.Bytes); input.Dispose(); Assert.Equal(0, root.Bytes);
    }

    [Fact]
    public void LateWorkFailureRefundsTheNewOwnerWithoutReleasingBorrowedInput()
    {
        using var source = OriginalVp8lControls.Buffer("singleton", 8);
        using var root = new TestMemoryBudget(16_000_000);
        using var inputContext = new OperationContext(new() { MemoryBudget = root }, default);
        using var input = Vp8PreparedInput.Create(source, inputContext); var baseline = root.Bytes;
        long work;
        using (var sizing = new OperationContext(new() { MemoryBudget = root }, default))
        using (var result = Vp8SourceAnalysis.Create(input, sizing)) work = sizing.ConsumedWork;
        using var limited = new OperationContext(new() { MemoryBudget = root, MaxWork = work - 1 }, default);
        var error = Assert.Throws<ImageLimitException>(() => Vp8SourceAnalysis.Create(input, limited));
        Assert.Equal(ImageLimitKind.Work, error.Limit); Assert.Equal(baseline, root.Bytes);
        using var next = Vp8SourceAnalysis.Create(input, inputContext); next.Dispose(); Assert.Equal(baseline, root.Bytes);
    }

    [Fact]
    public void RetainedAnalysisAndReplacementPeakShareTheCallersRoot()
    {
        using var source = OriginalVp8lControls.Buffer("singleton", 8);
        using var root = new TestMemoryBudget(16_000_000); using var caller = root.CreateScope();
        using var context = new OperationContext(new() { MemoryBudget = root }, default);
        using var input = Vp8PreparedInput.Create(source, context);
        using var first = Vp8SourceAnalysis.Create(input, context); var original = RecordsHash(first);
        using var replacement = new OperationContext(new() { MemoryBudget = root }, default);
        Assert.True(caller.TryReserve(16_000_000 - root.Bytes - first.CapacityBytes - 1023, out var held));
        var retained = root.Bytes;
        Assert.Throws<ImageLimitException>(() => Vp8SourceAnalysis.Create(input, replacement));
        Assert.Equal(retained, root.Bytes); Assert.Equal(original, RecordsHash(first));
        held.Dispose();
        using var second = Vp8SourceAnalysis.Create(input, replacement);
        Assert.Equal(input.CapacityBytes + first.CapacityBytes + second.CapacityBytes, root.Bytes);
        first.Dispose(); Assert.Equal(original, RecordsHash(second));
        second.Dispose(); input.Dispose(); Assert.Equal(0, root.Bytes);
    }
}
