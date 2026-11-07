using Lokad.Image.Internal;
using Lokad.Image.Tooling;

namespace Lokad.Image.Tests;

public sealed class Vp8AlphaCoefficientStorageTests
{
    private sealed class Inputs : IDisposable
    {
        internal readonly OperationContext Context;
        private readonly ImageBuffer source;
        internal readonly Vp8PreparedInput Prepared;
        internal readonly Vp8SourceAnalysis Analysis;
        internal readonly Vp8SourceSegments Segments;
        internal readonly Vp8QuantizationPlan Quantization;
        private readonly int method;

        internal Inputs(TestMemoryBudget root, int width = 63, int height = 61, int method = 4, CancellationToken cancellation = default)
        {
            this.method = method;
            Context = new(new() { MemoryBudget = root }, cancellation);
            source = OriginalPixels.Buffer(width, height, 8);
            Prepared = Vp8PreparedInput.Create(source, Context);
            Analysis = Vp8SourceAnalysis.Create(Prepared, Context, method);
            Segments = Vp8SourceSegments.Create(Analysis, 75, false, Context);
            Quantization = Vp8QuantizationPlan.Create(Segments, Analysis.MeanChromaScore, Context, method);
        }

        internal Vp8AlphaEncoding Alpha(int? offer = null) => Vp8AlphaEncoding.Create(Prepared.Alpha, Prepared.Width, Context, method,
            coefficientLength: offer ?? Vp8FramePlan.RequiredCoefficientLength(Prepared));
        internal Vp8FramePlan Frame(Vp8AlphaEncoding? alpha = null, int headerLimit = 65536) => method <= 2
            ? Vp8FramePlan.CreateDistortion(Prepared, Analysis, Segments, Quantization, method, Context, coefficientStorage: alpha)
            : Vp8FramePlan.Create(Prepared, Segments, Quantization, Context, method: method, headerLimit: headerLimit, coefficientStorage: alpha);
        internal void ReleaseInputs() { Quantization.Dispose(); Segments.Dispose(); Analysis.Dispose(); Prepared.Dispose(); source.Dispose(); }
        public void Dispose() { ReleaseInputs(); Context.Dispose(); }
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    [InlineData(5)]
    [InlineData(6)]
    public async Task DirtyAlphaStorageBecomesExactLogicalCoefficientsAndIndependentPartitions(int method)
    {
        using var root = new TestMemoryBudget(32_000_000);
        using var input = new Inputs(root, method: method);
        using var ordinary = input.Alpha(0);
        using var alpha = input.Alpha();
        var bytes = alpha.Bytes.ToArray();
        var scratch = input.Prepared.Width * (long)input.Prepared.Height * 4;
        Assert.Equal(ordinary.CapacityBytes + scratch, alpha.CapacityBytes);
        Assert.Equal(ordinary.Bytes.ToArray(), bytes);
        using var fresh = input.Frame();
        var baseline = root.Bytes;
        using var reused = input.Frame(alpha);
        if (method == 2)
        {
            // This frame needs block scratch and instructions. The borrowed
            // alpha owner keeps its unused offer until its actual disposal.
            Assert.NotNull(reused.Program); Assert.Equal(fresh.CapacityBytes, reused.CapacityBytes);
            Assert.Equal(baseline + reused.CapacityBytes, root.Bytes);
            Assert.Equal(ordinary.CapacityBytes + scratch, alpha.CapacityBytes);
        }
        else
        {
            Assert.Equal(baseline - scratch + reused.CapacityBytes, root.Bytes);
            Assert.Equal(fresh.CapacityBytes + scratch - fresh.Count * 800L, reused.CapacityBytes);
            Assert.Equal(reused.Count * 400, reused.Coefficients.Length);
            Assert.Equal(400, reused.Levels(reused.Count - 1).Length);
            Assert.Throws<ArgumentOutOfRangeException>(() => reused.Levels(reused.Count));
            Assert.Equal(fresh.Coefficients.ToArray(), reused.Coefficients.ToArray());
        }
        Assert.Equal(fresh.BlockData.ToArray(), reused.BlockData.ToArray());
        Assert.Equal(fresh.Probabilities.ToArray(), reused.Probabilities.ToArray());
        using var expected = Vp8FrameEncoding.Create(fresh, input.Context);
        using var actual = Vp8FrameEncoding.Create(reused, input.Context);
        using var first = new MemoryStream(); expected.WriteWebp(first, ordinary);
        input.ReleaseInputs(); fresh.Dispose(); reused.Dispose(); expected.Dispose(); ordinary.Dispose();
        Assert.Equal(alpha.CapacityBytes + actual.CapacityBytes, root.Bytes);
        using var second = new MemoryStream(); await actual.WriteWebpAsync(second, alpha);
        Assert.Equal(first.ToArray(), second.ToArray()); Assert.Equal(bytes, alpha.Bytes.ToArray());
        alpha.Dispose(); actual.Dispose(); Assert.Equal(0, root.Bytes); Assert.False(root.IsDisposed);
        Assert.True(first.CanWrite); Assert.True(second.CanWrite);
    }

    [Theory]
    [InlineData(1, 1, false)]
    [InlineData(17, 9, false)]
    [InlineData(17, 9, true)]
    [InlineData(64, 64, false)]
    public void PhysicalFitOrMissKeepsExactPayloadAndReleasesUnusedCapacity(int width, int height, bool forceSmallOffer)
    {
        using var root = new TestMemoryBudget(32_000_000); using var input = new Inputs(root, width, height);
        using var ordinary = input.Alpha(0); using var alpha = input.Alpha(forceSmallOffer ? 1 : null);
        var offered = alpha.CapacityBytes - ordinary.CapacityBytes;
        using var fresh = input.Frame(); using var reused = input.Frame(alpha);
        Assert.Equal(ordinary.CapacityBytes, alpha.CapacityBytes);
        var required = reused.Count * 800L;
        Assert.Equal(fresh.CapacityBytes + (offered >= required ? offered - required : 0), reused.CapacityBytes);
        Assert.Equal(fresh.Coefficients.ToArray(), reused.Coefficients.ToArray());
        Assert.Equal(ordinary.Bytes.ToArray(), alpha.Bytes.ToArray());
        input.ReleaseInputs(); fresh.Dispose(); reused.Dispose(); ordinary.Dispose(); alpha.Dispose();
        Assert.Equal(0, root.Bytes); Assert.False(root.IsDisposed);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void RefusalOrCancellationAfterTransferRefundsTheWholeAdoptedCapacity(bool cancellation)
    {
        using var root = new TestMemoryBudget(32_000_000); using var cancel = new CancellationTokenSource();
        using var input = new Inputs(root, cancellation: cancel.Token); using var alpha = input.Alpha();
        var payload = alpha.Bytes.ToArray(); var retained = input.Prepared.Width * (long)input.Prepared.Height * 4;
        var baseline = root.Bytes; var target = root.GrowthAttempts + 1;
        if (cancellation) root.Admitted = _ => cancel.Cancel(); else root.RefuseAtGrowth = target;
        if (cancellation) Assert.ThrowsAny<OperationCanceledException>(() => input.Frame(alpha));
        else
        {
            var error = Assert.Throws<ImageLimitException>(() => input.Frame(alpha));
            Assert.Equal((ImageLimitKind.WorkingBytes, ImageLimitScope.SharedBudget), (error.Limit, error.Scope));
            Assert.Equal(1, root.Refusals); Assert.Equal(target, root.GrowthAttempts);
        }
        root.Admitted = null; root.RefuseAtGrowth = 0;
        Assert.Equal(baseline - retained, root.Bytes); Assert.Equal(payload, alpha.Bytes.ToArray());
        input.ReleaseInputs(); alpha.Dispose(); Assert.Equal(0, root.Bytes); Assert.False(root.IsDisposed);
        using var next = new Inputs(root); using var nextAlpha = next.Alpha(); using var nextFrame = next.Frame(nextAlpha);
        nextFrame.Dispose(); nextAlpha.Dispose(); next.ReleaseInputs(); Assert.Equal(0, root.Bytes);
    }

    [Fact]
    public void RejectedBorrowerAndUnusedOfferKeepTheirExistingOwner()
    {
        using var root = new TestMemoryBudget(32_000_000); using var input = new Inputs(root); using var alpha = input.Alpha();
        using var foreign = new OperationContext(new() { MemoryBudget = root }, default);
        var baseline = root.Bytes; var owned = alpha.CapacityBytes;
        Assert.Throws<ArgumentException>(() => alpha.DetachCoefficientStorage(400, foreign));
        Assert.Throws<ArgumentOutOfRangeException>(() => alpha.DetachCoefficientStorage(-1, input.Context));
        Assert.Equal(baseline, root.Bytes); Assert.Equal(owned, alpha.CapacityBytes);
        alpha.Dispose(); Assert.Equal(baseline - owned, root.Bytes);
        Assert.Throws<ObjectDisposedException>(() => alpha.DetachCoefficientStorage(400, input.Context));
        input.ReleaseInputs(); Assert.Equal(0, root.Bytes); Assert.False(root.IsDisposed);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    public void HeaderRetryOrEmptyHeaderKeepsFreshZeroCoefficientSemantics(int headerLimit)
    {
        using var root = new TestMemoryBudget(32_000_000); using var input = new Inputs(root); using var alpha = input.Alpha();
        using var fresh = input.Frame(headerLimit: headerLimit); using var reused = input.Frame(alpha, headerLimit);
        Assert.Equal(fresh.HeaderRetries, reused.HeaderRetries);
        Assert.Equal(fresh.Coefficients.ToArray(), reused.Coefficients.ToArray());
        Assert.Equal(fresh.BlockData.ToArray(), reused.BlockData.ToArray());
        Assert.Equal(fresh.Probabilities.ToArray(), reused.Probabilities.ToArray());
    }
}
