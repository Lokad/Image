using System.IO.Compression;
using Lokad.Image.Tooling;

namespace Lokad.Image.PublicApi.Tests;

public sealed class PngManagedAdmissionTests
{
    [Theory]
    [InlineData(CompressionLevel.NoCompression)] [InlineData(CompressionLevel.Fastest)]
    [InlineData(CompressionLevel.Optimal)] [InlineData(CompressionLevel.SmallestSize)]
    public void NestedEncodesShareOneRootAndReleasedWorkspaceAdmitsTheNextOperation(CompressionLevel compression)
    {
        const int width = 32, height = 24, sourceBytes = width * height * 4;
        using var root = new TestMemoryBudget(sourceBytes + width * 4 + 65536 + 1024 * 1024);
        var limits = ImageLimits.Default with { MemoryBudget = root };
        using var source = ImageBuffer.Create(width, height, 8, limits);
        var options = new PngEncodeOptions { Limits = limits, Compression = compression };
        using var output = new CallbackOutput(() =>
        {
            using var nested = new MemoryStream();
            var error = Assert.Throws<ImageLimitException>(() => ImageCodec.EncodePng(source, nested, options));
            Assert.Equal(ImageLimitScope.SharedBudget, error.Scope); Assert.Equal(0, nested.Length);
        });
        ImageCodec.EncodePng(source, output, options);
        Assert.Equal(1, root.Refusals); Assert.Equal(sourceBytes, root.Bytes);
        using var next = new MemoryStream(); ImageCodec.EncodePng(source, next, options);
        Assert.Equal(output.ToArray(), next.ToArray()); Assert.Equal(sourceBytes, root.Bytes);
        Assert.True(output.CanWrite); Assert.False(root.IsDisposed);
        source.Dispose(); Assert.Equal(0, root.Bytes);
    }

    [Theory]
    [InlineData("shared", CompressionLevel.NoCompression)]
    [InlineData("work", CompressionLevel.NoCompression)]
    [InlineData("cancellation", CompressionLevel.NoCompression)]
    [InlineData("shared", CompressionLevel.Fastest)]
    [InlineData("work", CompressionLevel.Fastest)]
    [InlineData("cancellation", CompressionLevel.Fastest)]
    [InlineData("shared", CompressionLevel.Optimal)]
    [InlineData("work", CompressionLevel.Optimal)]
    [InlineData("cancellation", CompressionLevel.Optimal)]
    [InlineData("shared", CompressionLevel.SmallestSize)]
    [InlineData("work", CompressionLevel.SmallestSize)]
    [InlineData("cancellation", CompressionLevel.SmallestSize)]
    public void FailureReleasesWorkspaceWithoutFallbackAndKeepsRetainedSource(string failure, CompressionLevel compression)
    {
        const int width = 40, height = 29, bytes = width * height * 4;
        using var root = new TestMemoryBudget(4 * 1024 * 1024);
        var limits = ImageLimits.Default with { MemoryBudget = root };
        using var source = ImageBuffer.Create(width, height, 8, limits);
        using var cancellation = new CancellationTokenSource();
        if (failure == "shared") root.RefuseAtGrowth = root.GrowthAttempts + 2;
        if (failure == "work") limits = limits with { MaxWork = 1 };
        using var output = new CallbackOutput(() => { if (failure == "cancellation") cancellation.Cancel(); });
        var error = Record.Exception(() => ImageCodec.EncodePng(source, output, new() { Limits = limits, Compression = compression }, cancellation.Token));
        if (failure == "cancellation") Assert.Equal(cancellation.Token, Assert.IsType<OperationCanceledException>(error).CancellationToken);
        else
        {
            var limit = Assert.IsType<ImageLimitException>(error);
            Assert.Equal(failure == "shared" ? ImageLimitScope.SharedBudget : ImageLimitScope.Operation, limit.Scope);
            Assert.Equal(failure == "shared" ? ImageLimitKind.WorkingBytes : ImageLimitKind.Work, limit.Limit);
        }
        Assert.Equal(failure == "shared" ? 1 : 0, root.Refusals);
        if (failure == "shared") Assert.Equal(0, output.Length);
        Assert.Equal(bytes, root.Bytes); Assert.False(root.IsDisposed); Assert.True(output.CanWrite);
        root.RefuseAtGrowth = 0;
        using var next = new MemoryStream();
        ImageCodec.EncodePng(source, next, new() { Limits = ImageLimits.Default with { MemoryBudget = root }, Compression = compression });
        Assert.True(next.Length > 0); Assert.Equal(bytes, root.Bytes);
    }

    private sealed class CallbackOutput(Action callback) : MemoryStream
    {
        private bool invoked;
        public override void Write(ReadOnlySpan<byte> bytes)
        {
            if (!invoked) { invoked = true; callback(); }
            foreach (var value in bytes) base.WriteByte(value);
        }
        public override void Write(byte[] bytes, int offset, int count) => Write(bytes.AsSpan(offset, count));
    }
}
