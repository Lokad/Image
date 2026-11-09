using System.Runtime.Intrinsics.X86;
using Lokad.Image.Internal;

namespace Lokad.Image.Tests;

public sealed class JpegRefinementBlockTests
{
    [Theory]
    [InlineData(0, true)]
    [InlineData(15, true)]
    [InlineData(16, false)]
    public void TerminalZeroOrdinalsPreserveCorrectionsAndErrorPrefixes(int run, bool inserting)
    {
        if (!Avx2.IsSupported || !Bmi2.X64.IsSupported) return;
        using var tables = new OperationContext(new(), default);
        var symbol = inserting ? (byte)(run << 4 | 1) : (byte)0xf0;
        var table = new JpegHuffman([1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0], [symbol], tables);
        var headers = new uint[512]; table.BuildRefinementHeaders(headers, default);
        using var token = new CancellationTokenSource();
        for (var zeros = 0; zeros <= 16; zeros++)
        foreach (var prepared in new[] { false, true })
        foreach (var length in new[] { 1, 4 })
        {
            byte[] data = [0x55, 0x63, 0xa5, 0xc3, 255, 217];
            data = data[..length];
            Assert.Equal(Run(false), Run(true));

            string Run(bool fused)
            {
                using var context = new OperationContext(new(), token.Token);
                var reader = new JpegEntropyReader(data, 0, context); var eob = 0;
                var values = Enumerable.Range(0, 64).Select(i => i >= 64 - zeros ? 0 : (i % 2 == 0 ? 2 : -3)).ToArray();
                Exception? failure = null; var tail = -1;
                try
                {
                    if (fused) Assert.True(table.TryReadCancelableRefinementBlock(ref reader, values, 48, 63, 0,
                        ref eob, prepared ? headers : default));
                    else Reference(ref reader, values, table, 48, 63, 0, ref eob);
                    tail = reader.ReadBits(3);
                }
                catch (Exception error) when (error is ImageDataException or OverflowException) { failure = error; }
                return $"{failure?.GetType()}:{failure?.Message}:{reader.Position}:{eob}:{tail}:{string.Join(',', values)}";
            }
        }
    }

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
        var headers = new uint[512]; table.BuildRefinementHeaders(headers, default);
        for (var seed = 0; seed < 256; seed++)
        foreach (var length in new[] { 0, 1, 2, 3, 8, 16, 40 })
        foreach (var end in new[] { 5, 63 })
        {
            var data = Enumerable.Range(0, 40).Select(i => (byte)(seed * 73 + i * 41)).Take(length).ToArray();
            Assert.Equal(Run(0), Run(1));
            Assert.Equal(Run(0), Run(2));

            (Type? Error, string? Message, int Position, int Eob, int Tail, string Values) Run(int mode)
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
                    if (mode != 0) Assert.True(cancelable
                        ? table.TryReadCancelableRefinementBlock(ref reader, values, 1, end, low, ref eob, mode == 2 ? headers : default)
                        : table.TryReadRefinementBlock(ref reader, values, 1, end, low, ref eob, mode == 2 ? headers : default));
                    else Reference(ref reader, values, table, 1, end, low, ref eob);
                    tail = reader.ReadBits(3);
                }
                catch (Exception error) when (error is ImageDataException or ImageLimitException) { failure = error; }
                return (failure?.GetType(), failure?.Message, reader.Position, eob, tail, string.Join(',', values));
            }
        }
    }

    [Theory]
    [InlineData(false)] [InlineData(true)]
    public void PreparedHeadersPreserveAllNineBitPrefixesAndErrorTails(bool cancelable)
    {
        if (!Avx2.IsSupported || !Bmi2.X64.IsSupported) return;
        using var tables = new OperationContext(ImageLimits.Default, default);
        byte[] counts = new byte[16]; counts[5] = 33;
        var symbols = Enumerable.Range(0, 16).SelectMany(run => new[] { (byte)(run << 4), (byte)(run << 4 | 1) }).Append((byte)2).ToArray();
        var table = new JpegHuffman(counts, symbols, tables);
        var headers = new uint[512]; Array.Fill(headers, uint.MaxValue); table.BuildRefinementHeaders(headers, default);
        using var token = new CancellationTokenSource();
        for (var prefix = 0; prefix < 512; prefix++)
        foreach (var phase in new[] { 0, 3, 7 })
        foreach (var length in new[] { 1, 4, 16 })
        {
            var bits = new List<int>();
            for (var i = 0; i < phase; i++) bits.Add(1);
            for (var i = 8; i >= 0; i--) bits.Add(prefix >> i & 1);
            for (var i = 0; i < 128; i++) bits.Add((prefix + i * 3) >> (i & 7) & 1);
            var packet = new List<byte>();
            for (var i = 0; i + 8 <= bits.Count; i += 8)
            {
                var value = 0; for (var j = 0; j < 8; j++) value = value * 2 + bits[i + j];
                packet.Add((byte)value); if (value == 255) packet.Add(0);
            }
            packet.AddRange([255, 217]); var data = packet.Take(length).ToArray();
            Assert.Equal(Run(false), Run(true));
            string Run(bool prepared)
            {
                using var context = new OperationContext(ImageLimits.Default, cancelable ? token.Token : default);
                var reader = new JpegEntropyReader(data, 0, context);
                var low = (prefix & 1) == 0 ? 0 : 13; var increment = 1 << low;
                var values = Enumerable.Range(0, 64).Select(i => (i + prefix) % 3 == 0 ? 0 : (i % 2 == 0 ? 2 : -3) * increment).ToArray();
                var eob = 0; var tail = -1; Exception? failure = null;
                try
                {
                    if (phase != 0) reader.ReadBits(phase);
                    if (prepared) Assert.True(cancelable
                        ? table.TryReadCancelableRefinementBlock(ref reader, values, 1, 63, low, ref eob, headers)
                        : table.TryReadRefinementBlock(ref reader, values, 1, 63, low, ref eob, headers));
                    else Reference(ref reader, values, table, 1, 63, low, ref eob);
                    tail = reader.ReadBits(3);
                }
                catch (Exception error) when (error is ImageDataException or OverflowException) { failure = error; }
                return $"{failure?.GetType()}:{failure?.Message}:{reader.Position}:{eob}:{tail}:{string.Join(',', values)}";
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

    [Theory]
    [InlineData(false)] [InlineData(true)]
    public void CancelableOverflowDependsOnTheAlreadyConsumedCorrectionBit(bool set)
    {
        if (!Avx2.IsSupported || !Bmi2.X64.IsSupported) return;
        using var cancellation = new CancellationTokenSource();
        var context = new OperationContext(ImageLimits.Default, cancellation.Token);
        var counts = new byte[16]; counts[0] = 1;
        var table = new JpegHuffman(counts, [0], new OperationContext(ImageLimits.Default, default));
        var reader = new JpegEntropyReader(set ? [255, 0] : [0], 0, context); var values = new int[64]; values[1] = int.MinValue;
var eob = 1; OverflowException? failure = null;
        try { table.TryReadCancelableRefinementBlock(ref reader, values, 1, 63, 0, ref eob); }
        catch (OverflowException error) { failure = error; }
        if (set) Assert.NotNull(failure); else Assert.Null(failure);
        Assert.Equal(int.MinValue, values[1]); Assert.Equal(set ? 1 : 0, eob);
        Assert.Equal(set ? 2 : 1, reader.Position); Assert.Equal(set ? 127 : 0, reader.ReadBits(7));
    }

    [Theory]
    [InlineData(1)] [InlineData(31)] [InlineData(63)]
    public void DelayedCoefficientAccessPreservesGuardedSamplesAndFailurePrefixes(int coefficient)
    {
        if (!Avx2.IsSupported || !Bmi2.X64.IsSupported) return;
        using var tables = new OperationContext(ImageLimits.Default, default);
        var counts = new byte[16]; counts[0] = 1;
        var table = new JpegHuffman(counts, [0], tables);
        using var root = new Lokad.Image.Tooling.TestMemoryBudget(0);
        byte[][] packets = [[], [0], [0x80], [255, 0], [255, 217], [0x5A, 0x63, 0x81, 0x10, 255, 217]];
        foreach (var data in packets)
        foreach (var phase in new[] { 0, 7 })
        foreach (var sample in new[] { 0, 2, -2, 3, int.MinValue, int.MaxValue })
        foreach (var canceled in new[] { false, true })
        {
            var original = data.ToArray();
            Assert.Equal(Run(false), Run(true));
            Assert.Equal(original, data);

            string Run(bool buffered)
            {
                using var cancellation = new CancellationTokenSource();
                using var context = new OperationContext(new() { MemoryBudget = root }, cancellation.Token);
                var values = new int[66]; values[0] = 12345; values[65] = -12345;
                values[coefficient + 1] = sample;
                var reader = new JpegEntropyReader(data, 0, context); var eob = 1;
                Exception? failure = null; Exception? tailFailure = null; var tail = -1;
                try
                {
                    if (phase != 0) reader.ReadBits(phase);
                    if (canceled) cancellation.Cancel();
                    if (buffered) Assert.True(table.TryReadCancelableRefinementBlock(ref reader,
                        values.AsSpan(1, 64), coefficient, coefficient, 0, ref eob));
                    else Reference(ref reader, values.AsSpan(1, 64), table, coefficient, coefficient, 0, ref eob);
                }
                catch (Exception error) when (error is ImageDataException or OverflowException or OperationCanceledException)
                { failure = error; }
                var position = reader.Position;
                try { tail = reader.ReadBits(3); }
                catch (Exception error) when (error is ImageDataException or OperationCanceledException) { tailFailure = error; }
                Assert.Equal(12345, values[0]); Assert.Equal(-12345, values[65]);
                Assert.Equal(0, root.Bytes); Assert.Equal(0, root.GrowthAttempts); Assert.False(root.IsDisposed);
                return $"{failure?.GetType().Name}/{position}/{eob}/{string.Join(',', values)}/{tail}/{tailFailure?.GetType().Name}/{reader.Position}";
            }
        }
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

    [Theory]
    [InlineData(0x01)] [InlineData(0x11)] [InlineData(0x71)]
    [InlineData(0xF1)] [InlineData(0xF0)]
    public void ZeroRunBoundariesKeepCorrectionsErrorsAndTheLogicalTail(int symbol)
    {
        if (!Avx2.IsSupported || !Bmi2.X64.IsSupported) return;
        using var tables = new OperationContext(ImageLimits.Default, default);
        var counts = new byte[16]; counts[1] = 2;
        var table = new JpegHuffman(counts, [(byte)symbol, 0], tables);
        using var cancellation = new CancellationTokenSource();
        using var root = new Lokad.Image.Tooling.TestMemoryBudget(0);
        using var context = new OperationContext(new() { MemoryBudget = root }, cancellation.Token);
        byte[] data = [0x03, 255, 0, 255, 0, 255, 0, 255, 0, 255, 0, 255, 0, 255, 0, 255, 0, 0x5A, 255, 0xD9];
        foreach (var (start, end) in new[] { (1, 5), (1, 63), (31, 63), (48, 63), (63, 63) })
        for (var pattern = 0; pattern < 5; pattern++)
        foreach (var prefix in new[] { 0, 3 })
        foreach (var length in new[] { 1, 3, data.Length })
        foreach (var low in new[] { 0, 13 })
        foreach (var overflow in new[] { false, true })
        {
            Assert.Equal(Run(false), Run(true));

            string Run(bool fused)
            {
                var values = new int[64];
                for (var at = 0; at < 64; at++)
                {
                    var zero = pattern switch { 0 => true, 1 => false, 2 => at % 3 == 0, 3 => at == start, _ => at == end };
                    values[at] = zero ? 0 : (2 + at % 2) * (1 << low) * (at % 3 == 0 ? -1 : 1);
                }
                if (overflow)
                    for (var at = start; at <= end; at++) if (values[at] != 0) { values[at] = int.MinValue; break; }
                var reader = new JpegEntropyReader(data.AsSpan(0, length), 0, context); var eob = 0;
                Exception? failure = null; Exception? tailFailure = null; var tail = -1;
                try
                {
                    if (prefix != 0) reader.ReadBits(prefix);
                    if (fused) Assert.True(table.TryReadCancelableRefinementBlock(ref reader, values, start, end, low, ref eob));
                    else Reference(ref reader, values, table, start, end, low, ref eob);
                }
                catch (Exception error) when (error is ImageDataException or OverflowException) { failure = error; }
                var logicalPosition = reader.Position;
                try { tail = reader.ReadBits(3); }
                catch (ImageDataException error) { tailFailure = error; }
                return $"{failure?.GetType()}:{failure?.Message}:{logicalPosition}:{eob}:{string.Join(',', values)}:{tail}:{tailFailure?.GetType()}:{reader.Position}";
            }
        }
        Assert.Equal(0, root.Bytes); Assert.Equal(0, root.GrowthAttempts); Assert.False(root.IsDisposed);
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
