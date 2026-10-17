using Lokad.Image.Internal;

namespace Lokad.Image.Tests;

public sealed class JpegIdentifierTests
{
    [Theory]
    [InlineData(0)] [InlineData(1)] [InlineData(15)] [InlineData(16)] [InlineData(17)]
    [InlineData(31)] [InlineData(32)] [InlineData(33)] [InlineData(63)] [InlineData(64)]
    [InlineData(65)] [InlineData(4095)] [InlineData(4096)] [InlineData(4097)] [InlineData(8193)]
    public void BoundedVectorSearchMatchesScalarMetadataAndWorkAcrossTails(int length)
    {
        var entropy = new List<byte>();
        for (var i = 0; i < length; i++)
        {
            entropy.Add((byte)(17 + i % 200));
            if (i % 509 == 0) entropy.AddRange([255, 0]);
            if (i % 1021 == 0) entropy.AddRange([255, 255, 0xd0]);
        }
        var bytes = Packet(entropy.ToArray()); var original = bytes.ToArray();
        var scalar = Run(bytes, ImageLimits.Default, fast: false);
        var vector = Run(bytes, ImageLimits.Default, fast: true);
        Assert.Equal(scalar, vector);
        Assert.Equal(new ImageInfo(ImageFormat.Jpeg, 513, 259, 8, false, ImageOrientation.Normal), vector.Info);
        Assert.Equal(original, bytes);
    }

    [Fact]
    public void EverySmallWorkBudgetPreservesMetadataOrFailureAndConsumedWork()
    {
        foreach (var entropy in new[] { new byte[65], new byte[] { 17, 255, 0, 18, 255, 255, 0xd2, 19 } })
        {
            var bytes = Packet(entropy);
            for (var budget = 1; budget <= bytes.Length + 3; budget++)
            {
                var limits = ImageLimits.Default with { MaxWork = budget };
                Assert.Equal(Run(bytes, limits, fast: false), Run(bytes, limits, fast: true));
            }
        }
    }

    [Fact]
    public void MalformedMarkersAndTruncationKeepFailureOrderingAtEverySmallBudget()
    {
        var valid = Packet(new byte[33]);
        var badLength = valid.ToArray(); badLength[4] = 0; badLength[5] = 1;
        var badScan = valid.ToArray(); badScan[19] = 0;
        var zeroHeight = valid.ToArray(); zeroHeight[7] = zeroHeight[8] = 0;
        byte[][] variants = [valid[..^1], valid[..^2], [.. valid, 0], badLength, badScan, zeroHeight,
            [255, 216, 255, 0xd0, .. valid.AsSpan(2).ToArray()],
            [255, 216, 255, 0, .. valid.AsSpan(2).ToArray()],
            [255, 216, .. valid.AsSpan(2, 13).ToArray(), .. valid.AsSpan(2).ToArray()]];
        foreach (var bytes in variants)
        for (var budget = 1; budget <= bytes.Length + 3; budget++)
        {
            var limits = ImageLimits.Default with { MaxWork = budget };
            Assert.Equal(Run(bytes, limits, fast: false), Run(bytes, limits, fast: true));
        }
    }

    [Fact]
    public void TheFinalPlainRunConsumesItsAdmittedBudgetBeforeMissingEoi()
    {
        var bytes = Packet(new byte[8193])[..^2];
        foreach (var budget in new[] { 1, 4095, 4096, 4097, 8192, 8193, bytes.Length, bytes.Length + 20 })
        {
            var limits = ImageLimits.Default with { MaxWork = budget };
            Assert.Equal(Run(bytes, limits, fast: false), Run(bytes, limits, fast: true));
        }
    }

    [Fact]
    public void DenseStuffingAtVectorAndSearchEdgesKeepsExactScalarWork()
    {
        using var cancellation = new CancellationTokenSource();
        foreach (var edge in new[] { 0, 1, 62, 63, 64, 65, 4094, 4095, 4096, 4097 })
        {
            var entropy = Enumerable.Repeat((byte)17, edge).ToList();
            for (var i = 0; i < 129; i++) entropy.AddRange([255, 0]);
            entropy.AddRange([17, 255, 255, 0xd3, 17, 255, 0, 17]);
            var bytes = Packet(entropy.ToArray()); var original = bytes.ToArray();
            var expected = Run(bytes, ImageLimits.Default, fast: false); Assert.NotNull(expected.Info);
            var total = ImageLimits.Default.MaxWork - expected.RemainingWork;
            var budgets = Enumerable.Range(1, 80).Select(n => (long)n)
                .Concat([edge + 24L, edge + 25L, edge + 26L, total - 2, total - 1, total, total + 1]);
            foreach (var budget in budgets.Distinct())
            {
                var limits = ImageLimits.Default with { MaxWork = budget };
                var scalar = Run(bytes, limits, fast: false);
                Assert.Equal(scalar, Run(bytes, limits, fast: true));
                Assert.Equal(scalar, Run(bytes, limits, fast: true, cancellation.Token));
            }
            Assert.Equal(original, bytes);
        }
    }

    [Fact]
    public void StuffedTailsAndRealMarkersKeepFailurePrecedence()
    {
        foreach (var edge in new[] { 63, 64, 4095, 4096 })
        {
            var plain = Enumerable.Repeat((byte)17, edge).ToArray();
            var valid = Packet([.. plain, 255, 0, 17, 255, 0]);
            byte[][] variants = [valid[..^1], valid[..^2], [.. valid, 0], Packet([.. plain, 255, 0, 17, 255, 216])];
            foreach (var bytes in variants)
            {
                var expected = Run(bytes, ImageLimits.Default, fast: false); Assert.NotNull(expected.Error);
                var total = ImageLimits.Default.MaxWork - expected.RemainingWork;
                var budgets = Enumerable.Range(1, 80).Select(n => (long)n)
                    .Concat([edge + 24L, edge + 25L, edge + 26L, total - 1, total, total + 1]);
                foreach (var budget in budgets.Distinct())
                {
                    var limits = ImageLimits.Default with { MaxWork = budget };
                    Assert.Equal(Run(bytes, limits, fast: false), Run(bytes, limits, fast: true));
                }
            }
        }
    }

    private static byte[] Packet(byte[] entropy)
        // Original T.81 structural fixture; its opaque payload is never decoded.
        => [255, 216, 255, 0xc0, 0, 11, 8, 1, 3, 2, 1, 1, 1, 0x11, 0,
            255, 0xda, 0, 8, 1, 1, 0, 0, 63, 0, .. entropy, 255, 0xd9];

    private static Outcome Run(byte[] bytes, ImageLimits limits, bool fast, CancellationToken cancellation = default)
    {
        var context = new OperationContext(limits, cancellation); context.Reserve(bytes.Length);
        try
        {
            var info = fast ? JpegIdentifier.Identify(bytes, context) : ContainerHeaders.Jpeg(bytes, context);
            return new(info, null, context.RemainingWork, context.RemainingWorkingBytes);
        }
        catch (Exception error) when (error is ImageDataException or ImageLimitException or UnsupportedImageException)
        { return new(null, error.GetType(), context.RemainingWork, context.RemainingWorkingBytes, error.Message); }
    }
    private sealed record Outcome(ImageInfo? Info, Type? Error, long RemainingWork, long RemainingWorking, string? Message = null);
}
