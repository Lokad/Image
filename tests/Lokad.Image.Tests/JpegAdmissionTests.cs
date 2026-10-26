using Lokad.Image.Internal;

namespace Lokad.Image.Tests;

public sealed class JpegAdmissionTests
{
    [Fact]
    public void SingleSymbolHuffmanTableAdmitsItsArraysAndAllLengthVisits()
    {
        var counts = new byte[16]; counts[0] = 1;
        byte[] symbols = [7];
        var limits = new ImageLimits { MaxWorkingBytes = 717};
        _ = new JpegHuffman(counts, symbols, new OperationContext(limits, default));
        Assert.Throws<ImageLimitException>(() => new JpegHuffman(counts, symbols,
            new OperationContext(limits with { MaxWorkingBytes = 716 }, default)));
    }
}
