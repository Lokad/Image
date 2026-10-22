using System.Numerics;
using System.Reflection;
using Lokad.Image.Internal;
using Lokad.Image.Tooling;

namespace Lokad.Image.Tests;

public sealed class JpegChromaTableTests
{
    [Fact]
    public void TableCostIncludesCompleteDhtAndEveryObservedCodeLength()
    {
        var histogram = new long[256]; histogram[0] = 7; histogram[17] = 3;
        using var fit = new OperationContext(ImageLimits.Default, default);
        var table = JpegHuffmanBuilder.Build(histogram, fit);
        using var context = new OperationContext(ImageLimits.Default, default);
        Assert.Equal(8L * (21 + table.Symbols.Length) + 7 * table.Lengths[0] + 3 * table.Lengths[17],
            JpegEncoder.ProgressiveTableBitCost(histogram, table, context));
        Assert.Equal(256, context.ConsumedWork);
    }

    [Theory]
    [InlineData(false)] [InlineData(true)]
    public void TableCostRefusalDoesNotChangeInputsAndNextOperationSucceeds(bool cancel)
    {
        var histogram = new long[256]; histogram[0] = 6; var before = histogram.ToArray();
        using var fit = new OperationContext(ImageLimits.Default, default);
        var table = JpegHuffmanBuilder.Build(histogram, fit); var lengths = table.Lengths.ToArray();
        using var token = new CancellationTokenSource();
        using var context = new OperationContext(ImageLimits.Default with { MaxWork = 255 }, token.Token);
        if (cancel) token.Cancel();
        if (cancel) Assert.ThrowsAny<OperationCanceledException>(() => JpegEncoder.ProgressiveTableBitCost(histogram, table, context));
        else Assert.Equal(ImageLimitKind.Work, Assert.Throws<ImageLimitException>(() => JpegEncoder.ProgressiveTableBitCost(histogram, table, context)).Limit);
        Assert.Equal(0, context.ConsumedWork); Assert.Equal(before, histogram); Assert.Equal(lengths, table.Lengths);
        using var next = new OperationContext(ImageLimits.Default, default);
        Assert.True(JpegEncoder.ProgressiveTableBitCost(histogram, table, next) > 0);
    }

    [Fact]
    public void EveryOriginalQualityKeepsFitsAndBookkeepingInsideExistingWorkPlan()
    {
        for (var quality = 1; quality <= 98; quality++)
        {
            var bands = JpegEncoder.PlannedSourcePriceBands444(quality);
            var y = BitOperations.PopCount(bands.Luma | (1UL << 63));
            var c = BitOperations.PopCount(bands.Chroma | (1UL << 63));
            Assert.InRange(y + c, 2, 12);
            var fits = 3 + y + 3 * c;
            var definitions = 3 + y + 2 * c;
            // Entry/masks; DC/Y clear; paired clear/merge/three cost passes; emitted tables.
            var fixedWork = 2048 + 1920 + 256 * (3 + y) + 1536 * c + 512 * definitions;
            Assert.InRange(fits, 1, 39); Assert.True(fixedWork < 65536);
            Assert.True(fits * 65536L + fixedWork < 192L * 65536);
        }
    }

    [Fact]
    public void MatchingChromaBandsReuseDefinitionsAndDecodeToAuthoredConstantPixels()
    {
        // Original authored 17x11 image: every centered DCT coefficient is zero.
        // Padding blocks are included; decoded Y/Cb/Cr are128, so RGB is128.
        const int blocks = 18; var coefficients = new uint[blocks * 64]; var before = coefficients.ToArray();
        var (y, c) = Quantizers(); var originalY = y.ToArray(); var originalC = c.ToArray();
        using var root = new TestMemoryBudget(1_000_000);
        using var output = new MemoryStream();
        using (var context = new OperationContext(ImageLimits.Default with { MemoryBudget = root }, default))
        {
            context.Reserve(blocks * 432L + 65536);
            JpegEncoder.WriteProgressive444(17, 11, output, new() { Quality = 69, Sampling = JpegSampling.YCbCr444 }, y, c, coefficients, blocks, context);
            Assert.Equal(blocks * 432L + 65536, root.Bytes);
            Assert.True(context.ConsumedWork <= 192L * 65536 + blocks * 512L);
        }
        Assert.Equal(0, root.Bytes); Assert.False(root.IsDisposed); Assert.True(output.CanWrite);
        // The codec caller terminates the packet after this internal scan writer.
        output.Write(new byte[] { 255, 217 });
        var (definitions, scans) = Headers(output.ToArray());
        Assert.Equal(12, definitions); // 3DC +5Y +4shared-chroma instead of16definitions
        Assert.Equal(new[] { (1,0), (2,0), (3,0), (1,1), (1,3), (1,7), (1,9), (1,10),
            (2,1), (3,1), (2,4), (3,4), (2,5), (3,5), (2,6), (3,6) }, scans);
        using var image = ImageCodec.Decode(output.ToArray());
        Assert.Equal((17, 11, 8), (image.Width, image.Height, image.BitDepth));
        for (var row = 0; row < image.Height; row++)
        {
            var pixels = image.GetRow8(row);
            for (var i = 0; i < pixels.Length; i++) Assert.Equal(i % 4 == 3 ? (byte)255 : (byte)128, pixels[i]);
        }
        Assert.Equal(before, coefficients); Assert.Equal(originalY, y); Assert.Equal(originalC, c);
    }

    [Theory]
    [InlineData(2047, false)] [InlineData(3967, false)] [InlineData(10_000_000, true)]
    public void WriterSetupRefusalCleansAdmissionBeforeOutputAndLaterOperationSucceeds(long work, bool cancel)
    {
        var (y, c) = Quantizers(); var coefficients = new uint[3 * 64]; var options = new JpegEncodeOptions { Quality = 69, Sampling = JpegSampling.YCbCr444 };
        using var root = new TestMemoryBudget(100_000); using var output = new MemoryStream();
        using var token = new CancellationTokenSource();
        using (var context = new OperationContext(ImageLimits.Default with { MaxWork = work, MemoryBudget = root }, token.Token))
        {
            context.Reserve(3 * 432L + 65536);
            if (cancel) token.Cancel();
            if (cancel) Assert.ThrowsAny<OperationCanceledException>(() => JpegEncoder.WriteProgressive444(1, 1, output, options, y, c, coefficients, 3, context));
            else Assert.Equal(ImageLimitKind.Work, Assert.Throws<ImageLimitException>(() => JpegEncoder.WriteProgressive444(1, 1, output, options, y, c, coefficients, 3, context)).Limit);
            Assert.Equal(0, output.Length);
        }
        Assert.Equal(0, root.Bytes); Assert.False(root.IsDisposed); Assert.True(output.CanWrite);
        using (var next = new OperationContext(ImageLimits.Default with { MemoryBudget = root }, default))
        {
            next.Reserve(3 * 432L + 65536);
            JpegEncoder.WriteProgressive444(1, 1, output, options, y, c, coefficients, 3, next);
        }
        Assert.True(output.Length > 0); Assert.Equal(0, root.Bytes);
    }

    private static (ushort[], ushort[]) Quantizers()
    {
        var method = typeof(JpegEncoder).GetMethod("Quantization", BindingFlags.Static | BindingFlags.NonPublic,
            null, new[] { typeof(int), typeof(bool) }, null)!;
        return ((ushort[])method.Invoke(null, new object[] { 69, false })!, (ushort[])method.Invoke(null, new object[] { 69, true })!);
    }

    private static (int Definitions, (int Component, int First)[] Scans) Headers(byte[] data)
    {
        var definitions = 0; var scans = new List<(int, int)>(); var at = 2;
        while (at < data.Length)
        {
            Assert.Equal(255, data[at++]); var marker = data[at++];
            if (marker == 217) break;
            var length = data[at] * 256 + data[at + 1]; var start = at + 2; at += length;
            if (marker == 196) definitions++;
            if (marker != 218) continue;
            Assert.Equal(1, data[start]); scans.Add((data[start + 1], data[start + 3]));
            while (at + 1 < data.Length)
            {
                if (data[at] != 255) { at++; continue; }
                if (data[at + 1] == 0) { at += 2; continue; }
                break;
            }
        }
        return (definitions, scans.ToArray());
    }
}
