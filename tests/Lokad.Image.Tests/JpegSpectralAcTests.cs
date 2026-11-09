using Lokad.Image.Internal;
using Lokad.Image.Tooling;

namespace Lokad.Image.Tests;

public sealed class JpegSpectralAcTests
{
    private static readonly byte[] Symbols = [0, .. Enumerable.Range(0, 16).SelectMany(run => Enumerable.Range(1, 10).Select(size => (byte)(run << 4 | size))), 0xf0, 0x30, 0x0b];

    [Theory]
    [InlineData(8, false)]
    [InlineData(8, true)]
    [InlineData(9, false)]
    [InlineData(9, true)]
    public void SpectralOrderShiftAndPartialFailuresMatchOriginalTraversal(int codeLength, bool live)
    {
        using var tables = new OperationContext(new(), default);
        var lengths = new byte[16]; lengths[codeLength - 1] = (byte)Symbols.Length;
        var table = new JpegHuffman(lengths, Symbols, tables);
        foreach (var (start, end, low) in new[] { (1, 63, 0), (6, 20, 2), (61, 63, 13) })
        {
            var dense = string.Concat(Enumerable.Range(start, end - start + 1).Select(k => Code((byte)(1 + k % 10)) + new string((k & 1) == 0 ? '0' : '1', 1 + k % 10)));
            foreach (var payload in new[] { dense, Code(0xf0) + Code(1) + "1" + Code(0), Code(0x30) + "101", Code(0x0b), Code(0xf1) + "1", Code(1) + "1" + Code(0) })
            for (var phase = 0; phase < 8; phase++)
            {
                var bits = new string('1', phase) + payload + "101";
                bits = bits.PadRight((bits.Length + 7) / 8 * 8, '1');
                var bytes = new List<byte>();
                for (var at = 0; at < bits.Length; at += 8)
                {
                    var value = Convert.ToByte(bits.Substring(at, 8), 2); bytes.Add(value);
                    if (value == 255) bytes.Add(0);
                }
                bytes.AddRange([255, 0xd0]); var full = bytes.ToArray();
                for (var length = 0; length <= full.Length; length++)
                {
                    var input = full.AsSpan(0, length).ToArray();
                    var scalar = Run(false); var buffered = Run(true);
                    Assert.Equal(scalar.Error, buffered.Error); Assert.Equal(scalar.Message, buffered.Message);
                    Assert.Equal(scalar.Position, buffered.Position); Assert.Equal(scalar.K, buffered.K);
                    Assert.Equal(scalar.Eob, buffered.Eob); Assert.Equal(scalar.Tail, buffered.Tail);
                    Assert.Equal(scalar.Storage, buffered.Storage);
                    Assert.Equal(full.AsSpan(0, length).ToArray(), input);

                    (Type? Error, string? Message, int Position, int K, int Eob, int Tail, int[] Storage) Run(bool fast)
                    {
                        using var cancellation = new CancellationTokenSource();
                        using var context = new OperationContext(new(), live ? cancellation.Token : default);
                        var reader = new JpegEntropyReader(input, 0, context);
                        var storage = Enumerable.Repeat(12345, 66).ToArray(); var block = storage.AsSpan(1, 64);
                        var k = start; var eob = 0; var tail = 0; Exception? error = null;
                        try
                        {
                            if (phase != 0) reader.ReadBits(phase);
                            while (k <= end)
                            {
                                if (fast) table.ReadSpectralAcWindow(ref reader, block, ref k, end, low);
                                if (k > end) break;
                                var symbol = table.Read(ref reader); var run = symbol >> 4; var size = symbol & 15;
                                if (size == 0)
                                {
                                    if (run == 15) { k += 16; if (k > end + 1) throw new ImageDataException("JPEG zero run exceeds band."); continue; }
                                    eob = (1 << run) + reader.ReadBits(run) - 1; break;
                                }
                                if (size > 10) throw new ImageDataException("Invalid JPEG AC coefficient category.");
                                k += run;
                                if (k > end) throw new ImageDataException("JPEG coefficient run exceeds band.");
                                block[k++] = reader.ReadSigned(size) << low;
                            }
                            tail = reader.ReadBits(3); reader.Restart(0);
                        }
                        catch (Exception failure) when (failure is ImageDataException) { error = failure; }
                        Assert.Equal(12345, storage[0]); Assert.Equal(12345, storage[^1]);
                        return (error?.GetType(), error?.Message, reader.Position, k, eob, tail, storage);
                    }
                }
            }
        }
        string Code(byte symbol) => Convert.ToString(Array.IndexOf(Symbols, symbol), 2).PadLeft(codeLength, '0');
    }

    [Fact]
    public void LiveCancellationLeavesTheLogicalReaderAndStorageUntouched()
    {
        using var tables = new OperationContext(new(), default);
        var table = new JpegHuffman([1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0], [1], tables);
        using var cancellation = new CancellationTokenSource();
        using var context = new OperationContext(new(), cancellation.Token);
        var reader = new JpegEntropyReader(new byte[16], 0, context);
        reader.ReadBits(3); cancellation.Cancel();
        var block = Enumerable.Repeat(12345, 64).ToArray(); var k = 6;
        try { table.ReadSpectralAcWindow(ref reader, block, ref k, 20, 2); Assert.Fail("The first logical code must observe cancellation."); }
        catch (OperationCanceledException failure) { Assert.Equal(cancellation.Token, failure.CancellationToken); }
        Assert.Equal(1, reader.Position); Assert.Equal(6, k); Assert.All(block, v => Assert.Equal(12345, v));
        try { table.Read(ref reader); Assert.Fail("The original logical read must observe cancellation."); }
        catch (OperationCanceledException failure) { Assert.Equal(cancellation.Token, failure.CancellationToken); }
        Assert.Equal(1, reader.Position);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void WarmTraversalAllocatesNoStorage(bool live)
    {
        using var tables = new OperationContext(new(), default);
        var table = new JpegHuffman([1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0], [1], tables);
        using var cancellation = new CancellationTokenSource(); using var root = new TestMemoryBudget(0);
        using var context = new OperationContext(new() { MemoryBudget = root }, live ? cancellation.Token : default);
        var data = new byte[32]; var block = new int[64];
        var warm = new JpegEntropyReader(data, 0, context); var k = 6;
        table.ReadSpectralAcWindow(ref warm, block, ref k, 20, 2); Assert.Equal(21, k);
        var reader = new JpegEntropyReader(data, 0, context); k = 6;
        var before = GC.GetAllocatedBytesForCurrentThread();
        table.ReadSpectralAcWindow(ref reader, block, ref k, 20, 2);
        Assert.Equal(before, GC.GetAllocatedBytesForCurrentThread()); Assert.Equal(21, k);
        for (var index = 6; index <= 20; index++) Assert.Equal(-4, block[index]);
        Assert.Equal(0, root.Bytes); Assert.Equal(0, root.GrowthAttempts); Assert.False(root.IsDisposed);
    }
    [Fact]
    public void ConcurrentLiveCancellationMatchesAnOriginalLogicalBitPrefix()
    {
        using var tables = new OperationContext(new(), default);
        var table = new JpegHuffman([1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0], [1], tables);
        const int blocks = 1024;
        var data = new byte[blocks * 63 * 2 / 8];
        foreach (var delay in new[] { 0, 64, 1024, 16384 })
        {
            using var cancellation = new CancellationTokenSource(); using var ready = new ManualResetEventSlim();
            using var root = new TestMemoryBudget(0);
            using var context = new OperationContext(new() { MemoryBudget = root }, cancellation.Token);
            var storage = new int[blocks * 64];
            for (var block = 0; block < blocks; block++) storage[block * 64] = 12345;
            var reader = new JpegEntropyReader(data, 0, context); OperationCanceledException? error = null;
            var thread = new Thread(() => { ready.Wait(); Thread.SpinWait(delay); cancellation.Cancel(); });
            thread.Start(); ready.Set();
            try
            {
                for (var block = 0; block < blocks; block++)
                {
                    var k = 1; var values = storage.AsSpan(block * 64, 64);
                    while (k <= 63)
                    {
                        table.ReadSpectralAcWindow(ref reader, values, ref k, 63, 0);
                        if (k > 63) break;
                        Assert.Equal(1, table.Read(ref reader)); values[k++] = reader.ReadSigned(1);
                    }
                }
            }
            catch (OperationCanceledException failure) { error = failure; }
            finally { thread.Join(); }
            if (error is not null) Assert.Equal(cancellation.Token, error.CancellationToken);
            else Assert.Equal(data.Length, reader.Position);
            var matched = false;
            for (var consumed = Math.Max(0, reader.Position * 8 - 7); consumed <= reader.Position * 8; consumed++)
            {
                var changed = consumed / 2; var equal = true;
                for (var block = 0; block < blocks && equal; block++)
                {
                    if (storage[block * 64] != 12345) { equal = false; break; }
                    for (var coefficient = 1; coefficient < 64; coefficient++)
                        if (storage[block * 64 + coefficient] != (block * 63 + coefficient <= changed ? -1 : 0))
                        { equal = false; break; }
                }
                if (equal) { matched = true; break; }
            }
            Assert.True(matched); Assert.Equal(0, root.Bytes); Assert.Equal(0, root.GrowthAttempts); Assert.False(root.IsDisposed);
            Assert.All(data, value => Assert.Equal(0, value));
        }
    }

}
