using Lokad.Image.Internal;

namespace Lokad.Image.Tests;

public sealed class Vp8AdmissionTests
{
    [Fact]
    public void ArithmeticFinalizationChecksCancellationWithAnExistingBuffer()
    {
        using var cancellation = new CancellationTokenSource();
        var context = new OperationContext(ImageLimits.Default, cancellation.Token);
        using var writer = new Vp8BooleanWriter(context, 1024);
        writer.Write(0); writer.Write(0); cancellation.Cancel();
        Assert.Throws<OperationCanceledException>(writer.Finish);
    }

    [Theory]
    [InlineData(1)] [InlineData(65536)] [InlineData(65537)] [InlineData(131079)]
    public void RawAlphaCopyChecksEveryBoundedChunkAndPreservesItsWorkCharge(int length)
    {
        var source = Enumerable.Repeat((byte)53, length).ToArray();
        var destination = new byte[length];
        var exact = new OperationContext(new ImageLimits { MaxWork = length + 1L }, default); exact.Work(1);
        WebpAlpha.CopyRaw(source, destination, exact); Assert.Equal(source, destination);
        Array.Fill(destination, (byte)177);
        var exhausted = new OperationContext(new ImageLimits { MaxWork = length }, default); exhausted.Work(1);
        Assert.Throws<ImageLimitException>(() => WebpAlpha.CopyRaw(source, destination, exhausted));
        var copiedPrefix = (length - 1) / 65536 * 65536;
        Assert.True(destination.AsSpan(0, copiedPrefix).SequenceEqual(source.AsSpan(0, copiedPrefix)));
        Assert.True(destination.AsSpan(copiedPrefix).IndexOfAnyExcept((byte)177) < 0);
        Array.Fill(destination, (byte)177);
        using var cancellation = new CancellationTokenSource();
        var canceled = new OperationContext(ImageLimits.Default, cancellation.Token); cancellation.Cancel();
        Assert.Throws<OperationCanceledException>(() => WebpAlpha.CopyRaw(source, destination, canceled));
        Assert.True(destination.AsSpan().IndexOfAnyExcept((byte)177) < 0);
        Assert.True(source.AsSpan().IndexOfAnyExcept((byte)53) < 0);
    }

    [Theory]
    [InlineData(96, 224)] [InlineData(1024, 256)]
    public void ArithmeticGrowthAdmitsOldAndNewBuffersTogetherAndDisposalReleasesThem(int maximum, int peak)
    {
        var context = new OperationContext(new ImageLimits { MaxWorkingBytes = peak }, default);
        using (var writer = new Vp8BooleanWriter(context, maximum))
        {
            for (var bit = 0; bit < 514; bit++) writer.Write(0);
            writer.Finish();
            Assert.Equal(new byte[68], writer.Bytes.ToArray());
        }
        context.Reserve(peak); Assert.Throws<ImageLimitException>(() => context.Reserve(1));
        var exhausted = new OperationContext(new ImageLimits { MaxWorkingBytes = peak - 1 }, default);
        using (var writer = new Vp8BooleanWriter(exhausted, maximum))
        {
            for (var bit = 0; bit < 513; bit++) writer.Write(0);
            Assert.Throws<ImageLimitException>(() => writer.Write(0));
        }
        exhausted.Reserve(peak - 1);
    }
}
