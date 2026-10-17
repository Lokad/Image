using System.Runtime.Intrinsics.X86;
using Lokad.Image.Internal;

namespace Lokad.Image.Tests;

public sealed class JpegRefinementBlockTests
{
    [Theory]
    [InlineData(0, false, false)] [InlineData(1, false, false)] [InlineData(2, false, false)]
    [InlineData(0, true, false)] [InlineData(1, true, false)] [InlineData(2, true, false)]
    [InlineData(0, false, true)] [InlineData(1, false, true)] [InlineData(2, false, true)]
    [InlineData(0, true, true)] [InlineData(1, true, true)] [InlineData(2, true, true)]
    public void FusedBlockMatchesOriginalTraversalAndPartialFailures(int initialEob, bool longCodes, bool cancelable)
    {
        if (!Avx2.IsSupported || !Bmi2.X64.IsSupported) return;
        using var cancellation = new CancellationTokenSource();
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
                var context = new OperationContext(ImageLimits.Default with { MaxWork = 4096 }, cancelable ? cancellation.Token : default);
                var reader = new JpegEntropyReader(data, 0, context);
                var low = seed % 3 == 0 ? 13 : 0; var increment = 1 << low;
                var values = Enumerable.Range(0, 64).Select(i => (i + seed) % 5 == 0 ? 0
                    : (2 + i % 2) * increment * (i % 3 == 0 ? -1 : 1)).ToArray();
                var eob = initialEob; var tail = 0; Exception? failure = null;
                try
                {
                    if ((seed & 7) != 0) reader.ReadBits(seed & 7);
                    if (fused) Assert.True(cancelable
                        ? table.TryReadCancelableRefinementBlock(ref reader, values, 1, end, low, ref eob)
                        : table.TryReadRefinementBlock(ref reader, values, 1, end, low, ref eob));
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

    [Theory]
    [InlineData(2047, false)] [InlineData(2048, false)]
    [InlineData(2047, true)] [InlineData(2048, true)]
    public void CancelableBlockGuardsRetainOriginalScopeAndBudget(int budget, bool local)
    {
        using var cancellation = new CancellationTokenSource();
        var context = new OperationContext(ImageLimits.Default with { MaxWork = budget }, cancellation.Token);
        var counts = new byte[16]; counts[0] = 1;
        var table = new JpegHuffman(counts, [0], new OperationContext(ImageLimits.Default, default));
        var reader = new JpegEntropyReader([0], 0, context); var values = new int[64]; var eob = 1;
        if (local) reader.BeginLocalWork();
        var eligible = budget >= 2048 && !local && Avx2.IsSupported && Bmi2.X64.IsSupported;
        Assert.Equal(eligible, table.TryReadCancelableRefinementBlock(ref reader, values, 1, 63, 0, ref eob));
        if (local) reader.EndLocalWork();
        Assert.Equal(0, reader.Position); Assert.Equal(budget, context.RemainingWork);
        Assert.Equal(eligible ? 0 : 1, eob); Assert.All(values, value => Assert.Equal(0, value));
    }

    [Fact]
    public void CancelableBlockKeepsPendingBitsAndWorkOnCancellation()
    {
        if (!Avx2.IsSupported || !Bmi2.X64.IsSupported) return;
        using var cancellation = new CancellationTokenSource();
        var context = new OperationContext(ImageLimits.Default, cancellation.Token);
        var counts = new byte[16]; counts[0] = 1;
        var table = new JpegHuffman(counts, [0], new OperationContext(ImageLimits.Default, default));
        var reader = new JpegEntropyReader([0xA5], 0, context); var values = new int[64]; values[1] = 2;
        Assert.Equal(5, reader.ReadBits(3));
        var remaining = context.RemainingWork; var eob = 1;
        cancellation.Cancel(); OperationCanceledException? failure = null;
        try { table.TryReadCancelableRefinementBlock(ref reader, values, 1, 63, 0, ref eob); }
        catch (OperationCanceledException error) { failure = error; }
        Assert.NotNull(failure); Assert.Equal(cancellation.Token, failure.CancellationToken);
        Assert.Equal(1, reader.Position); Assert.Equal(remaining, context.RemainingWork);
        Assert.Equal(2, values[1]); Assert.Equal(1, eob);
    }

    [Fact]
    public void EmptyCancelableEobTailRetainsItsOriginalNoReadBehavior()
    {
        if (!Avx2.IsSupported || !Bmi2.X64.IsSupported) return;
        using var cancellation = new CancellationTokenSource();
        var context = new OperationContext(ImageLimits.Default, cancellation.Token);
        var counts = new byte[16]; counts[0] = 1;
        var table = new JpegHuffman(counts, [0], new OperationContext(ImageLimits.Default, default));
        var reader = new JpegEntropyReader([], 0, context); var values = new int[64]; var eob = 1;
        var remaining = context.RemainingWork; cancellation.Cancel();
        Assert.True(table.TryReadCancelableRefinementBlock(ref reader, values, 1, 63, 0, ref eob));
        Assert.Equal(0, eob); Assert.Equal(0, reader.Position); Assert.Equal(remaining, context.RemainingWork);
    }

    [Fact]
    public void CancelableOverflowRetainsTheAlreadyConsumedCorrectionBit()
    {
        if (!Avx2.IsSupported || !Bmi2.X64.IsSupported) return;
        using var cancellation = new CancellationTokenSource();
        var context = new OperationContext(ImageLimits.Default, cancellation.Token);
        var counts = new byte[16]; counts[0] = 1;
        var table = new JpegHuffman(counts, [0], new OperationContext(ImageLimits.Default, default));
        var reader = new JpegEntropyReader([255, 0], 0, context); var values = new int[64]; values[1] = int.MinValue;
        var remaining = context.RemainingWork; var eob = 1; OverflowException? failure = null;
        try { table.TryReadCancelableRefinementBlock(ref reader, values, 1, 63, 0, ref eob); }
        catch (OverflowException error) { failure = error; }
        Assert.NotNull(failure); Assert.Equal(int.MinValue, values[1]); Assert.Equal(1, eob);
        Assert.Equal(2, reader.Position); Assert.Equal(remaining - 1, context.RemainingWork);
        Assert.Equal(127, reader.ReadBits(7));
    }

    [Fact]
    public void ConcurrentCancellationPublishesAnOriginalLogicalPrefix()
    {
        if (!Avx2.IsSupported || !Bmi2.X64.IsSupported) return;
        var counts = new byte[16]; counts[0] = 1;
        var table = new JpegHuffman(counts, [0], new OperationContext(ImageLimits.Default, default));
        for (var attempt = 0; attempt < 8; attempt++)
        {
            using var cancellation = new CancellationTokenSource();
            var context = new OperationContext(ImageLimits.Default, cancellation.Token);
            var data = Enumerable.Range(0, 16).Select(i => (byte)((attempt * 73 + i * 41) % 255)).ToArray();
            var worker = new Thread(() => { Thread.SpinWait(20000); cancellation.Cancel(); });
            var canceled = false;
            worker.Start();
            try
            {
                for (var round = 0; round < 100000; round++)
                {
                    var reader = new JpegEntropyReader(data, 0, context);
                    var values = Enumerable.Repeat(2, 64).ToArray(); var eob = 1;
                    var before = context.RemainingWork;
                    try { Assert.True(table.TryReadCancelableRefinementBlock(ref reader, values, 1, 63, 0, ref eob)); }
                    catch (OperationCanceledException error)
                    {
                        var consumed = (int)(before - context.RemainingWork);
                        Assert.InRange(consumed, 0, 63); Assert.Equal(1, eob);
                        Assert.Equal(cancellation.Token, error.CancellationToken);
                        var reference = new JpegEntropyReader(data, 0, new OperationContext(ImageLimits.Default, default));
                        var expected = Enumerable.Repeat(2, 64).ToArray();
                        for (var i = 1; i <= consumed; i++)
                            if (reference.ReadBits(1) != 0) expected[i]++;
                        Assert.Equal(expected, values); Assert.Equal(reference.Position, reader.Position);
                        canceled = true; break;
                    }
                }
                Assert.True(canceled);
            }
            finally { cancellation.Cancel(); worker.Join(); }
        }
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
