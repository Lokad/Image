using Lokad.Image.Tooling;

namespace Lokad.Image.PublicApi.Tests;

public sealed class PreparedMemoryStreamTests
{
    [Theory]
    [InlineData(256)] [InlineData(8192)]
    public void CompleteMetadataUsesRelativeCursorAndOneOwnedCapacity(int length)
    {
        var data = Packet(length); var caller = new byte[37 + length]; data.CopyTo(caller, 37);
        var saved = caller.ToArray();
        using var root = new TestMemoryBudget(length);
        var limits = new ImageLimits { MemoryBudget = root, MaxWorkingBytes = length, MaxInputBytes = length };
        using var input = new MemoryStream(caller, writable: false) { Position = 37 };
        Assert.Equal(ImageCodec.Identify(data), ImageCodec.Identify(input, limits));
        Assert.Equal(input.Length, input.Position); Assert.True(input.CanRead);
        Assert.Equal(saved, caller); Assert.Equal(length, root.Peak);
        Assert.Equal(0, root.Bytes); Assert.False(root.IsDisposed);
    }

    [Fact]
    public void InitialRefusalDoesNotRetryOrReadAndReleasedCapacityAllowsRecovery()
    {
        var data = Packet(257);
        using var root = new TestMemoryBudget(512);
        var limits = new ImageLimits { MemoryBudget = root, MaxWorkingBytes = 512 };
        using var held = ImageBuffer.Create(3, 1, 8, limits);
        using var input = new MemoryStream(data, writable: false);
        var attempts = root.Attempts;
        var error = Assert.Throws<ImageLimitException>(() => ImageCodec.Identify(input, limits));
        Assert.Equal(ImageLimitScope.SharedBudget, error.Scope);
        Assert.Equal(attempts + 1, root.Attempts); Assert.Equal(1, root.Refusals);
        Assert.Equal(12, root.Bytes); Assert.Equal(0, input.Position); Assert.True(input.CanRead);
        held.Dispose();
        Assert.Equal(ImageCodec.Identify(data), ImageCodec.Identify(input, limits));
        Assert.Equal(0, root.Bytes); Assert.False(root.IsDisposed);
    }

    [Fact]
    public void CancellationAfterPreparationAdmissionCleansUpBeforeAnyRead()
    {
        var data = Packet(257);
        using var root = new TestMemoryBudget(512);
        var limits = new ImageLimits { MemoryBudget = root, MaxWorkingBytes = 512 };
        using var cancellation = new CancellationTokenSource();
        root.Admitted = _ => cancellation.Cancel();
        using var input = new MemoryStream(data, writable: false);
        var error = Assert.ThrowsAny<OperationCanceledException>(() => ImageCodec.Identify(input, limits, cancellation.Token));
        Assert.Equal(cancellation.Token, error.CancellationToken);
        Assert.Equal(0, input.Position); Assert.Equal(512, root.Peak); Assert.Equal(0, root.Bytes);
        Assert.True(input.CanRead); Assert.False(root.IsDisposed);
        root.Admitted = null;
        Assert.Equal(ImageCodec.Identify(data), ImageCodec.Identify(input, limits));
        Assert.Equal(0, root.Bytes);
    }

    private static byte[] Packet(int length)
    {
        var frame = new AuthoredPng.Frame("prepared-memory-stream", 6, 8, 1, 1, false);
        var zlib = AuthoredPng.StoredZlib(AuthoredPng.FilteredRows(frame));
        var minimal = AuthoredPng.Wrap(frame, zlib);
        return AuthoredPng.Wrap(frame, zlib, ("tEXt", new byte[length - minimal.Length - 12]));
    }
}
