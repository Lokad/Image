using Lokad.Image.Internal;
using Lokad.Image.Tooling;

namespace Lokad.Image.Tests;

[Collection(FilteredResizeStorageCollection.Name)]
public sealed class Vp8RenderStorageTests
{
    [Theory]
    [InlineData(1, false)] [InlineData(1, true)]
    [InlineData(8, false)] [InlineData(8, true)]
    [InlineData(9, false)] [InlineData(9, true)]
    [InlineData(10, false)] [InlineData(10, true)]
    [InlineData(17, false)] [InlineData(17, true)]
    public void DirtyOutputIsFullyWrittenAndSurvivingImagesStayIndependent(int width, bool explicitAlpha)
    {
        var planes = Planes(width); var source = planes.Y.Concat(planes.U).Concat(planes.V).ToArray();
        var alpha = explicitAlpha ? Enumerable.Range(0, width * 3).Select(i => (byte)(i * 47)).ToArray() : null;
        var alphaCopy = alpha?.ToArray();
        using var root = new TestMemoryBudget(1_000_000);
        using var referenceContext = Context(planes, alpha, root);
        using var reference = Vp8Color.Render(planes, alpha, null, referenceContext);
        var expected = reference.Pixels.ToArray(); var dirty = Poison(expected.Length);

        using var firstContext = Context(planes, alpha);
        using var first = Vp8Color.Render(planes, alpha, null, firstContext);
        Assert.True(first.Pixels.Overlaps(dirty));
        Assert.Equal(expected, first.Pixels.ToArray());
        using var secondContext = Context(planes, alpha);
        using var second = Vp8Color.Render(planes, alpha, null, secondContext);
        Assert.False(first.Pixels.Overlaps(second.Pixels));
        first.Pixels.Fill(0x3c);
        first.Dispose();
        using var nextContext = Context(planes, alpha);
        using var next = Vp8Color.Render(planes, alpha, null, nextContext);
        Assert.True(next.Pixels.Overlaps(dirty));
        Assert.Equal(expected, next.Pixels.ToArray());
        Assert.Equal(expected, second.Pixels.ToArray());
        Assert.Equal(source, planes.Y.Concat(planes.U).Concat(planes.V).ToArray());
        Assert.Equal(alphaCopy, alpha);
    }

    [Fact]
    public void GovernedOwnersBypassPoolAndCleanUpRefusalCancellationAndRenderFailure()
    {
        var planes = Planes(17); var outputBytes = 17 * 3 * 4; var dirty = Poison(outputBytes);
        using var root = new TestMemoryBudget(1_000_000);
        using (var context = Context(planes, null, root))
        {
            var inputBytes = root.Bytes; var attempts = root.Attempts;
            root.RefuseAtGrowth = root.GrowthAttempts + 1;
            var error = Assert.Throws<ImageLimitException>(() => Vp8Color.Render(planes, null, null, context));
            Assert.Equal(ImageLimitScope.SharedBudget, error.Scope);
            Assert.Equal(attempts + 1, root.Attempts);
            Assert.Equal(inputBytes, root.Bytes);
            root.RefuseAtGrowth = 0;
        }
        Assert.Equal(0, root.Bytes);

        using (var cancellation = new CancellationTokenSource())
        using (var context = Context(planes, null, root, cancellation.Token))
        {
            var inputBytes = root.Bytes;
            root.Admitted = _ => cancellation.Cancel();
            Assert.Throws<OperationCanceledException>(() => Vp8Color.Render(planes, null, null, context));
            Assert.Equal(inputBytes, root.Bytes);
            root.Admitted = null;
        }
        Assert.Equal(0, root.Bytes);

        var invalid = planes with { Y = [] };
        using (var context = Context(invalid, null, root))
        {
            var inputBytes = root.Bytes;
            var error = Record.Exception(() => Vp8Color.Render(invalid, null, null, context));
            Assert.True(error is ArgumentException or IndexOutOfRangeException);
            Assert.Equal(inputBytes, root.Bytes);
        }
        Assert.Equal(0, root.Bytes);

        using var recoveredContext = Context(planes, null, root);
        using var result = Vp8Color.Render(planes, null, null, recoveredContext);
        Assert.False(result.Pixels.Overlaps(dirty));
        recoveredContext.Dispose();
        Assert.Equal(outputBytes, root.Bytes);
        Assert.Same(dirty, PixelStoragePool.Shared.Rent(outputBytes));
        PixelStoragePool.Shared.Return(dirty);
        result.Dispose();
        Assert.Equal(0, root.Bytes);
        Assert.False(root.IsDisposed);
    }

    private static byte[] Poison(int length)
    {
        _ = PixelStoragePool.Shared.Rent(length + 1);
        var data = PixelStoragePool.Shared.Rent(length);
        data.AsSpan().Fill(0xa5);
        PixelStoragePool.Shared.Return(data);
        return data;
    }

    private static OperationContext Context(Vp8Planes planes, byte[]? alpha,
        TestMemoryBudget? root = null, CancellationToken cancellation = default)
    {
        var context = new OperationContext(ImageLimits.Default with { MemoryBudget = root }, cancellation);
        context.Reserve(planes.MemoryBytes + (alpha is null ? 0 : alpha.Length + 32L));
        return context;
    }

    private static Vp8Planes Planes(int width)
    {
        var stride = (width + 15) & ~1;
        static byte[] Values(int count, int offset)
            => Enumerable.Range(0, count).Select(i => (byte)(i * 37 + offset)).ToArray();
        return new(width, 3, stride, Values(stride * 3, 19), Values(stride, 53), Values(stride, 137));
    }
}
