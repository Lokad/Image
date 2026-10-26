using Lokad.Image.Internal;

namespace Lokad.Image.Tests;

public sealed class Vp8AdmissionTests
{
    [Theory]
    [InlineData(1)] [InlineData(65536)] [InlineData(65537)] [InlineData(131079)]
    public void RawAlphaCopyIsExactAndCooperativelyCancellable(int length)
    {
        var source = Enumerable.Repeat((byte)53, length).ToArray();
        var destination = new byte[length];
        using var context = new OperationContext(ImageLimits.Default, default);
        WebpAlpha.CopyRaw(source, destination, context); Assert.Equal(source, destination);
        Array.Fill(destination, (byte)177);
        using var cancellation = new CancellationTokenSource();
        using var canceled = new OperationContext(ImageLimits.Default, cancellation.Token); cancellation.Cancel();
        Assert.Throws<OperationCanceledException>(() => WebpAlpha.CopyRaw(source, destination, canceled));
        Assert.True(destination.AsSpan().IndexOfAnyExcept((byte)177) < 0);
        Assert.True(source.AsSpan().IndexOfAnyExcept((byte)53) < 0);
    }
}
