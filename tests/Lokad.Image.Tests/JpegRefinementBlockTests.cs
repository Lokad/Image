using System.Runtime.Intrinsics.X86;
using Lokad.Image.Internal;

namespace Lokad.Image.Tests;

public sealed class JpegRefinementBlockTests
{
    [Theory]
    [InlineData(0, false)] [InlineData(1, false)] [InlineData(2, false)]
    [InlineData(0, true)] [InlineData(1, true)] [InlineData(2, true)]
    public void FusedBlockMatchesOriginalTraversalAndPartialFailures(int initialEob, bool longCodes)
    {
        if (!Avx2.IsSupported || !Bmi2.X64.IsSupported) return;
        byte[] counts = new byte[16]; counts[longCodes ? 8 : 2] = 5;
        var table = new JpegHuffman(counts, [0x00, 0x01, 0xF0, 0x10, 0x02], new OperationContext(ImageLimits.Default, default));
        for (var seed = 0; seed < 256; seed++)
        foreach (var length in new[] { 0, 1, 2, 3, 8, 16, 40 })
        foreach (var end in new[] { 5, 63 })
        {
            var data = Enumerable.Range(0, 40).Select(i => (byte)(seed * 73 + i * 41)).Take(length).ToArray();
            Assert.Equal(Run(false), Run(true));

            (Type? Error, string? Message, int Position, long Remaining, int Eob, int Tail, string Values) Run(bool fused)
            {
                var context = new OperationContext(ImageLimits.Default with { MaxWork = 4096 }, default);
                var reader = new JpegEntropyReader(data, 0, context);
                var low = seed % 3 == 0 ? 13 : 0; var increment = 1 << low;
                var values = Enumerable.Range(0, 64).Select(i => (i + seed) % 5 == 0 ? 0
                    : (2 + i % 2) * increment * (i % 3 == 0 ? -1 : 1)).ToArray();
                var eob = initialEob; var tail = 0; Exception? failure = null;
                try
                {
                    if ((seed & 7) != 0) reader.ReadBits(seed & 7);
                    if (fused) Assert.True(table.TryReadRefinementBlock(ref reader, values, 1, end, low, ref eob));
                    else Reference(ref reader, values, table, 1, end, low, ref eob);
                    tail = reader.ReadBits(3);
                }
                catch (Exception error) when (error is ImageDataException or ImageLimitException) { failure = error; }
                return (failure?.GetType(), failure?.Message, reader.Position, context.RemainingWork, eob, tail, string.Join(',', values));
            }
        }
    }

    [Theory]
    [InlineData(2047, false)] [InlineData(2048, false)] [InlineData(4096, true)]
    public void BudgetAndCancellationGuardsDoNotConsumeInput(int budget, bool cancelable)
    {
        using var cancellation = new CancellationTokenSource();
        var context = new OperationContext(ImageLimits.Default with { MaxWork = budget }, cancelable ? cancellation.Token : default);
        var counts = new byte[16]; counts[0] = 1;
        var table = new JpegHuffman(counts, [0], new OperationContext(ImageLimits.Default, default));
        var reader = new JpegEntropyReader([0], 0, context); var values = new int[64]; var eob = 1;
        var eligible = budget >= 2048 && !cancelable && Avx2.IsSupported && Bmi2.X64.IsSupported;
        Assert.Equal(eligible, table.TryReadRefinementBlock(ref reader, values, 1, 63, 0, ref eob));
        Assert.Equal(0, reader.Position); Assert.Equal(budget, context.RemainingWork);
        Assert.Equal(eligible ? 0 : 1, eob); Assert.All(values, value => Assert.Equal(0, value));
    }

    // Independent T.81 G.1.2 traversal using the established one-bit reader;
    // it neither reads correction fields nor constructs nonzero/run masks.
    private static void Reference(ref JpegEntropyReader reader, Span<int> block, JpegHuffman table,
        int start, int end, int low, ref int eob)
    {
        var index = start; var increment = 1 << low;
        if (eob == 0)
            while (index <= end)
            {
                var symbol = table.Read(ref reader); var run = symbol >> 4; var size = symbol & 15; var inserted = 0;
                if (size != 0)
                {
                    if (size != 1) throw new ImageDataException("Invalid JPEG refinement symbol.");
                    inserted = reader.ReadBits(1) == 0 ? -increment : increment;
                }
                else if (run != 15) { eob = (1 << run) + reader.ReadBits(run); break; }
                else run = 16;
                while (index <= end)
                {
                    ref var value = ref block[index];
                    if (value != 0)
                    {
                        if (reader.ReadBits(1) != 0 && (Math.Abs(value) & increment) == 0)
                            value += value > 0 ? increment : -increment;
                    }
                    else
                    {
                        if (run == 0) break;
                        run--;
                        if (run == 0 && inserted == 0) { index++; break; }
                    }
                    index++;
                }
                if (run != 0 || (inserted != 0 && index > end)) throw new ImageDataException("JPEG refinement run exceeds band.");
                if (inserted != 0) block[index++] = inserted;
            }
        if (eob > 0)
        {
            while (index <= end)
            {
                ref var value = ref block[index++];
                if (value != 0 && reader.ReadBits(1) != 0 && (Math.Abs(value) & increment) == 0)
                    value += value > 0 ? increment : -increment;
            }
            eob--;
        }
    }
}
