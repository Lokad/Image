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

            (Type? Error, string? Message, int Position, int Eob, int Tail, string Values) Run(bool fused)
            {
                var context = new OperationContext(ImageLimits.Default, cancelable ? cancellation.Token : default);
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
                return (failure?.GetType(), failure?.Message, reader.Position, eob, tail, string.Join(',', values));
            }
        }
    }

    [Fact]
    public void CancelableBlockKeepsPendingBitsAndOnCancellation()
    {
        if (!Avx2.IsSupported || !Bmi2.X64.IsSupported) return;
        using var cancellation = new CancellationTokenSource();
        var context = new OperationContext(ImageLimits.Default, cancellation.Token);
        var counts = new byte[16]; counts[0] = 1;
        var table = new JpegHuffman(counts, [0], new OperationContext(ImageLimits.Default, default));
        var reader = new JpegEntropyReader([0xA5], 0, context); var values = new int[64]; values[1] = 2;
        Assert.Equal(5, reader.ReadBits(3));
var eob = 1;
        cancellation.Cancel(); OperationCanceledException? failure = null;
        try { table.TryReadCancelableRefinementBlock(ref reader, values, 1, 63, 0, ref eob); }
        catch (OperationCanceledException error) { failure = error; }
        Assert.NotNull(failure); Assert.Equal(cancellation.Token, failure.CancellationToken);
        Assert.Equal(1, reader.Position);         Assert.Equal(2, values[1]); Assert.Equal(1, eob);
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
cancellation.Cancel();
        Assert.True(table.TryReadCancelableRefinementBlock(ref reader, values, 1, 63, 0, ref eob));
        Assert.Equal(0, eob); Assert.Equal(0, reader.Position);     }

    [Fact]
    public void CancelableOverflowRetainsTheAlreadyConsumedCorrectionBit()
    {
        if (!Avx2.IsSupported || !Bmi2.X64.IsSupported) return;
        using var cancellation = new CancellationTokenSource();
        var context = new OperationContext(ImageLimits.Default, cancellation.Token);
        var counts = new byte[16]; counts[0] = 1;
        var table = new JpegHuffman(counts, [0], new OperationContext(ImageLimits.Default, default));
        var reader = new JpegEntropyReader([255, 0], 0, context); var values = new int[64]; values[1] = int.MinValue;
var eob = 1; OverflowException? failure = null;
        try { table.TryReadCancelableRefinementBlock(ref reader, values, 1, 63, 0, ref eob); }
        catch (OverflowException error) { failure = error; }
        Assert.NotNull(failure); Assert.Equal(int.MinValue, values[1]); Assert.Equal(1, eob);
        Assert.Equal(2, reader.Position);         Assert.Equal(127, reader.ReadBits(7));
    }

    [Fact]
    public void BufferedCancelableLookaheadKeepsConsecutiveBlocksAndLogicalPendingBits()
    {
        if (!Avx2.IsSupported || !Bmi2.X64.IsSupported) return;
        using var cancellation = new CancellationTokenSource();
        var counts = new byte[16]; counts[2] = 5;
        var table = new JpegHuffman(counts, [0x00, 0x01, 0xF0, 0x10, 0x02], new OperationContext(ImageLimits.Default, default));
        for (var seed = 0; seed < 128; seed++)
        for (var prefix = 0; prefix < 8; prefix++)
        foreach (var stuffed in new[] { false, true })
        {
            var bytes = Enumerable.Range(0, 128).Select(i => (byte)((seed * 73 + i * 41) % 255)).ToList();
            if (stuffed) for (var i = 7; i < bytes.Count - 1; i += 13) { bytes[i] = 255; bytes[i + 1] = 0; }
            var data = bytes.ToArray();
            Assert.Equal(Run(false), Run(true));

            string Run(bool fused)
            {
                var context = new OperationContext(ImageLimits.Default, cancellation.Token);
                var reader = new JpegEntropyReader(data, 0, context); var eob = seed % 3;
                var values = new int[64]; var states = new List<string>(); Exception? failure = null;
                try
                {
                    if (prefix != 0) reader.ReadBits(prefix);
                    for (var round = 0; round < 4; round++)
                    {
                        var low = seed % 3 == 0 ? 13 : 0; var increment = 1 << low;
                        for (var i = 0; i < 64; i++) values[i] = (i + seed + round) % 5 == 0 ? 0 : (2 + i % 2) * increment * (i % 3 == 0 ? -1 : 1);
                        if (fused) Assert.True(table.TryReadCancelableRefinementBlock(ref reader, values, 1, 63, low, ref eob));
                        else Reference(ref reader, values, table, 1, 63, low, ref eob);
                        states.Add(reader.ReadBits(round + 1).ToString());
                    }
                    states.Add(reader.ReadBits(16).ToString());
                }
                catch (Exception error) when (error is ImageDataException or ImageLimitException) { failure = error; }
                return $"{failure?.GetType()}:{failure?.Message}:{reader.Position}:{eob}:{string.Join(',', values)}:{string.Join(';', states)}";
            }
        }
    }

    [Fact]
    public void BufferedCancelableLookaheadReturnsWholeBytesBeforeStuffingMarkersAndPadding()
    {
        if (!Avx2.IsSupported || !Bmi2.X64.IsSupported) return;
        using var cancellation = new CancellationTokenSource();
        var counts = new byte[16]; counts[0] = 1;
        var table = new JpegHuffman(counts, [0], new OperationContext(ImageLimits.Default, default));
        byte[] all = [0x53, 0xad, 0x17, 0x80, 0x5a, 0, 0x73, 0x19, 255, 0, 0x55, 0xa5, 255, 0, 0x31, 0x62, 255, 0xda];
        for (var length = 0; length <= all.Length; length++)
        for (var prefix = 0; prefix < 8; prefix++)
        foreach (var overflow in new[] { false, true })
        {
            var data = all[..length]; Assert.Equal(Run(false), Run(true));
            (Type?, string?, int, int, string, int) Run(bool fused)
            {
                var context = new OperationContext(ImageLimits.Default, cancellation.Token);
                var reader = new JpegEntropyReader(data, 0, context); var eob = 1;
                var values = Enumerable.Range(0, 64).Select(i => i % 3 == 0 ? 0 : 2).ToArray();
                if (overflow) values[7] = int.MinValue;
                Exception? failure = null; var tail = -1;
                try
                {
                    if (prefix != 0) reader.ReadBits(prefix);
                    if (fused) Assert.True(table.TryReadCancelableRefinementBlock(ref reader, values, 1, 63, 0, ref eob));
                    else Reference(ref reader, values, table, 1, 63, 0, ref eob);
                    tail = reader.ReadBits(3); reader.Align();
                }
                catch (Exception error) when (error is ImageDataException or ImageLimitException or OverflowException) { failure = error; }
                return (failure?.GetType(), failure?.Message, reader.Position, eob, string.Join(',', values), tail);
            }
        }
    }

    // Independent T.81 G.1.2 traversal using the established one-bit reader;
    // it neither reads correction fields nor constructs nonzero/run masks.
    internal static void Reference(ref JpegEntropyReader reader, Span<int> block, JpegHuffman table,
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
