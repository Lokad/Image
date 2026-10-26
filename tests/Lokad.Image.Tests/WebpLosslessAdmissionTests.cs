using Lokad.Image.Internal;

namespace Lokad.Image.Tests;

public sealed class WebpLosslessAdmissionTests
{
    [Theory]
    [InlineData(false, 1)] [InlineData(true, 1)]
    [InlineData(false, 1024)] [InlineData(true, 1024)]
    [InlineData(false, 1025)] [InlineData(true, 1025)]
    [InlineData(false, 2051)] [InlineData(true, 2051)]
    public void AuxiliaryStateVisitsValidateEveryEntryAndCheckCancellation(bool predictors, int length)
    {
        var values = Enumerable.Range(0, length).Select(i => predictors
            ? 0xFF0000FFu | (uint)(i % 14) << 8 : 0xAA000055u | (uint)(i % 65536) << 8).ToArray();
        var unchanged = values.ToArray();
        var exact = new OperationContext(new ImageLimits { }, default);
Visit(exact);

        using var cancellation = new CancellationTokenSource();
        var canceled = new OperationContext(ImageLimits.Default, cancellation.Token); cancellation.Cancel();
        Assert.Throws<OperationCanceledException>(() => Visit(canceled));
        Assert.Equal(unchanged, values);

        void Visit(OperationContext context)
        {
            if (predictors) WebpLosslessDecoder.ValidatePredictorModes(values, context);
            else Assert.Equal(length, WebpLosslessDecoder.CountEntropyGroups(values, context));
        }
    }

    [Theory]
    [InlineData(14)] [InlineData(15)]
    public void InvalidPredictorAfterCompleteChunksStillRaisesMalformedData(int mode)
    {
        var values = new uint[2051]; values[^1] = (uint)mode << 8;
        Assert.Throws<ImageDataException>(() => WebpLosslessDecoder.ValidatePredictorModes(values,
            new OperationContext(ImageLimits.Default, default)));
    }

    [Fact]
    public void EntropyGroupIdentifiersUseOnlyTheDeclaredSixteenBits()
    {
        Assert.Equal(1, WebpLosslessDecoder.CountEntropyGroups([], new OperationContext(ImageLimits.Default, default)));
        Assert.Equal(65536, WebpLosslessDecoder.CountEntropyGroups(new uint[] { 0xFF0000FF, 0x01FFFFFF },
            new OperationContext(ImageLimits.Default, default)));
    }

    [Theory]
    [InlineData(false)] [InlineData(true)]
    public void HuffmanConstructionAdmitsSingleLeafOrCompleteTreeStorage(bool single)
    {
        byte[] lengths = single ? [0, 0, 1] : [1, 2, 2];
        var bytes = single ? 64 : 374;

        var limits = new ImageLimits { MaxWorkingBytes = bytes};
        Assert.Equal(bytes, new WebpHuffman(lengths, new OperationContext(limits, default)).MemoryBytes);
        Assert.Throws<ImageLimitException>(() => new WebpHuffman(lengths,
            new OperationContext(limits with { MaxWorkingBytes = bytes - 1 }, default)));
        using var cancellation = new CancellationTokenSource();
        var canceled = new OperationContext(limits, cancellation.Token); cancellation.Cancel();
        Assert.Throws<OperationCanceledException>(() => new WebpHuffman(lengths, canceled));
        Assert.Equal(single ? new byte[] { 0, 0, 1 } : new byte[] { 1, 2, 2 }, lengths);
    }
}
