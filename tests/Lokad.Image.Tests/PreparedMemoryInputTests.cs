using Lokad.Image.Internal;
using Lokad.Image.Tooling;

namespace Lokad.Image.Tests;

public sealed class PreparedMemoryInputTests
{
    [Fact]
    public void OrdinaryMemoryInputOwnsOneAdmittedCapacityIndependentOfTheCaller()
    {
        var original = Enumerable.Range(0, 218775).Select(i => (byte)(i * 67 + 31)).ToArray();
        var caller = original.ToArray();
        using var root = new TestMemoryBudget(262144);
        using var context = new OperationContext(new() { MemoryBudget = root, MaxWorkingBytes = 262144 }, default);
        using var stream = new MemoryStream(caller, writable: false);
        Assert.False(stream.TryGetBuffer(out _));
        using var input = IdentificationInput.Read(stream, context);
        Assert.Equal(262144, input.Capacity); Assert.Equal(262144, root.Peak);
        Assert.Equal(262144, root.Bytes); Assert.Equal(0, context.RemainingWorkingBytes);
        Assert.Equal(stream.Length, stream.Position); Assert.Equal(original, caller);
        caller.AsSpan().Fill(0xA5); stream.Dispose();
        Assert.Equal(original, input.Span.ToArray()); Assert.False(root.IsDisposed);
    }

    [Theory]
    [InlineData(false)] [InlineData(true)]
    public void DerivedMemoryStreamsKeepTheirReadAndMetadataFailureSemantics(bool fail)
    {
        var expected = new IOException("Original read failure.");
        var bytes = Enumerable.Range(0, 257).Select(i => (byte)(i * 31)).ToArray();
        using var stream = new HostileMetadataStream(bytes, fail ? expected : null);
        using var root = new TestMemoryBudget(768);
        using var context = new OperationContext(new() { MemoryBudget = root, MaxWorkingBytes = 768 }, default);
        if (fail)
        {
            Assert.Same(expected, Assert.Throws<IOException>(() => IdentificationInput.Read(stream, context)));
            Assert.Equal(0, root.Bytes); Assert.Equal(768, context.RemainingWorkingBytes);
        }
        else
        {
            using var input = IdentificationInput.Read(stream, context);
            Assert.Equal(bytes, input.Span.ToArray()); Assert.Equal(768, root.Peak);
            Assert.Equal(512, input.Capacity);
        }
        Assert.True(stream.CanRead); Assert.False(root.IsDisposed);
    }

    [Fact]
    public void LargeDeclaredMemoryInputsKeepIncrementalAdmission()
    {
        using var stream = new MemoryStream(new byte[4 * 1024 * 1024 + 1], writable: false);
        using var root = new TestMemoryBudget(256);
        using var context = new OperationContext(new() { MemoryBudget = root, MaxWorkingBytes = 256 }, default);
        var error = Assert.Throws<ImageLimitException>(() => IdentificationInput.Read(stream, context));
        Assert.Equal(ImageLimitKind.WorkingBytes, error.Limit);
        Assert.Equal(257, stream.Position); Assert.Equal(256, root.Peak);
        Assert.Equal(0, root.Bytes); Assert.Equal(256, context.RemainingWorkingBytes);
        Assert.True(stream.CanRead); Assert.False(root.IsDisposed);
    }

    [Fact]
    public void KnownLengthDoesNotSkipTheRealInputLimitProbe()
    {
        using var stream = new MemoryStream(new byte[257], writable: false);
        using var root = new TestMemoryBudget(256);
        using var context = new OperationContext(new()
            { MemoryBudget = root, MaxWorkingBytes = 256, MaxInputBytes = 256 }, default);
        var error = Assert.Throws<ImageLimitException>(() => IdentificationInput.Read(stream, context));
        Assert.Equal(ImageLimitKind.InputBytes, error.Limit);
        Assert.Equal(257, stream.Position); Assert.Equal(0, root.Refusals);
        Assert.Equal(0, root.Bytes); Assert.True(stream.CanRead);
    }

    private sealed class HostileMetadataStream(byte[] bytes, IOException? failure) : MemoryStream(bytes, writable: false)
    {
        public override long Length => throw new InvalidOperationException("Length must not be queried.");
        public override long Position
        {
            get => throw new InvalidOperationException("Position must not be queried.");
            set => throw new InvalidOperationException("Position must not be changed.");
        }
        public override int Read(Span<byte> destination)
            => failure is null ? base.Read(destination) : throw failure;
    }
}
