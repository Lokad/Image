using Lokad.Image.Internal;
using Lokad.Image.Tooling;

namespace Lokad.Image.Tests;

public sealed class WebpAlphaEncodingTests
{
    [Theory]
    [InlineData(1, 8195, 0)] [InlineData(8195, 1, 1)]
    [InlineData(129, 67, 2)] [InlineData(17, 257, 3)]
    public void HeaderlessGreenCopiesAndHorizontalEdgesRecoverExactAlpha(int width, int height, int pattern)
    {
        var alpha = new byte[width * height];
        for (var y = 0; y < height; y++)
        for (var x = 0; x < width; x++) alpha[y * width + x] = pattern switch
        { 0 => 0, 1 => 77, 2 => unchecked((byte)(x * 23 + y * 37)), _ => (byte)(x < 8 ? 0 : 255) };
        var unchanged = alpha.ToArray();
        using var context = new OperationContext(ImageLimits.Default, default);
        using var plan = WebpLosslessEncoder.AlphaEncoding.TryCreate(alpha, width, 4, context);
        Assert.NotNull(plan);
        using var output = new MemoryStream(); output.WriteByte(plan.Header); plan.Write(alpha, width, output);
        Assert.Equal(plan.Length + 1, output.Length);
        Assert.True(output.Length + (output.Length & 1) < alpha.Length + 1 + ((alpha.Length + 1) & 1));
        using var decode = new OperationContext(ImageLimits.Default, default);
        Assert.Equal(alpha, WebpAlpha.Decode(output.ToArray(), width, height, decode));
        Assert.Equal(unchanged, alpha);
    }

    [Fact]
    public void UneconomicalOrLocallyUnadmittedCandidatesLeaveRawStorageUnchanged()
    {
        var alpha = new byte[4096]; new Random(17).NextBytes(alpha); var unchanged = alpha.ToArray();
        using var root = new TestMemoryBudget(8192);
        using var context = new OperationContext(new ImageLimits { MemoryBudget = root, MaxWorkingBytes = 2048 }, default);
        Assert.Null(WebpLosslessEncoder.AlphaEncoding.TryCreate(alpha, 64, 4, context));
        Assert.Equal(0, root.Bytes); Assert.Equal(unchanged, alpha);
        using var local = new OperationContext(new ImageLimits { MaxWorkingBytes = 2047, MemoryBudget = root }, default);
        var attempts = root.Attempts;
        Assert.Null(WebpLosslessEncoder.AlphaEncoding.TryCreate(alpha, 64, 4, local));
        Assert.Equal(attempts, root.Attempts);
        Assert.Null(WebpLosslessEncoder.AlphaEncoding.TryCreate(alpha, 64, 0, context));
        Assert.Null(WebpLosslessEncoder.AlphaEncoding.TryCreate(alpha.AsSpan(0, 63), 1, 4, context));
    }

    [Fact]
    public void AdmittedWorkCoversBothPassesAndTightWorkSkipsBeforeSharedAdmission()
    {
        var alpha = new byte[8195]; const long bound = 8195 * 4L + 2048;
        using var root = new TestMemoryBudget(8192);
        using var tight = new OperationContext(new ImageLimits { MaxWork = bound - 1, MemoryBudget = root }, default);
        Assert.Null(WebpLosslessEncoder.AlphaEncoding.TryCreate(alpha, 1, 4, tight)); Assert.Equal(0, root.Attempts);
        using var exact = new OperationContext(new ImageLimits { MaxWork = bound, MemoryBudget = root }, default);
        using (var plan = WebpLosslessEncoder.AlphaEncoding.TryCreate(alpha, 1, 4, exact))
        {
            Assert.NotNull(plan); Assert.Equal(2048, root.Bytes);
            using var output = new MemoryStream(); plan.Write(alpha, 1, output);
            Assert.InRange(exact.ConsumedWork, 1, bound);
        }
        Assert.Equal(0, root.Bytes); Assert.False(root.IsDisposed);
    }

    [Fact]
    public void SharedRefusalIsTerminalAndSubsequentOperationCanUseReleasedCapacity()
    {
        using var root = new TestMemoryBudget(2048);
        Assert.True(root.TryReserve(1, out var retained));
        using (var context = new OperationContext(new ImageLimits { MemoryBudget = root, MaxWorkingBytes = 2048 }, default))
        {
            var error = Assert.Throws<ImageLimitException>(() =>
                WebpLosslessEncoder.AlphaEncoding.TryCreate(new byte[1024], 32, 4, context));
            Assert.Equal(ImageLimitKind.WorkingBytes, error.Limit);
            Assert.Equal(ImageLimitScope.SharedBudget, error.Scope);
        }
        Assert.Equal(1, root.Refusals); Assert.Equal(1, root.Bytes); retained.Dispose();
        using (var context = new OperationContext(new ImageLimits { MemoryBudget = root, MaxWorkingBytes = 2048 }, default))
        using (var plan = WebpLosslessEncoder.AlphaEncoding.TryCreate(new byte[1024], 32, 4, context)) Assert.NotNull(plan);
        Assert.Equal(0, root.Bytes); Assert.False(root.IsDisposed);
    }

    [Fact]
    public void CancellationAfterAdmissionReleasesTheCandidateWithoutChangingInput()
    {
        using var cancellation = new CancellationTokenSource();
        using var root = new TestMemoryBudget(2048) { Admitted = _ => cancellation.Cancel() };
        var alpha = Enumerable.Repeat((byte)43, 1024).ToArray();
        using (var context = new OperationContext(new ImageLimits { MemoryBudget = root, MaxWorkingBytes = 2048 }, cancellation.Token))
            Assert.Throws<OperationCanceledException>(() => WebpLosslessEncoder.AlphaEncoding.TryCreate(alpha, 32, 4, context));
        Assert.Equal(0, root.Bytes); Assert.All(alpha, a => Assert.Equal(43, a)); Assert.False(root.IsDisposed);
        root.Admitted = null;
        using var next = new OperationContext(new ImageLimits { MemoryBudget = root, MaxWorkingBytes = 2048 }, default);
        using var plan = WebpLosslessEncoder.AlphaEncoding.TryCreate(alpha, 32, 4, next); Assert.NotNull(plan);
    }

    [Theory] [InlineData(false)] [InlineData(true)]
    public void InterruptedWritingKeepsItsChargeUntilTheWriterOwnerIsDisposed(bool cancel)
    {
        using var cancellation = new CancellationTokenSource();
        using var root = new TestMemoryBudget(2048);
        using var context = new OperationContext(new ImageLimits { MemoryBudget = root, MaxWorkingBytes = 2048 }, cancellation.Token);
        var alpha = new byte[1024];
        using (var plan = WebpLosslessEncoder.AlphaEncoding.TryCreate(alpha, 32, 4, context))
        {
            Assert.NotNull(plan);
            using var output = new InterruptedOutput(() =>
            {
                Assert.Equal(2048, root.Bytes);
                if (cancel) cancellation.Cancel(); else throw new IOException("Original output failure.");
            });
            if (cancel) Assert.Throws<OperationCanceledException>(() => plan.Write(alpha, 32, output));
            else Assert.Throws<IOException>(() => plan.Write(alpha, 32, output));
            Assert.Equal(2048, root.Bytes); Assert.True(output.CanWrite);
        }
        Assert.Equal(0, root.Bytes); Assert.False(root.IsDisposed); Assert.All(alpha, a => Assert.Equal(0, a));
    }

    private sealed class InterruptedOutput(Action interrupt) : MemoryStream
    {
        public override void WriteByte(byte value) { base.WriteByte(value); interrupt(); }
    }
}
