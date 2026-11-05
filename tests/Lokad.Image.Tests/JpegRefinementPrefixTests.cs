using System.Runtime.Intrinsics.X86;
using Lokad.Image.Internal;
using Lokad.Image.Tooling;

namespace Lokad.Image.Tests;

public sealed class JpegRefinementPrefixTests
{
    [Fact]
    public void ConcurrentCancellationLeavesAStateFromTheOriginalBitwisePrefix()
    {
        if (!Avx2.IsSupported || !Bmi2.X64.IsSupported) return;
        using var tables = new OperationContext(new(), default);
        var table = new JpegHuffman([1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0], [0], tables);
        var headers = new uint[512]; table.BuildRefinementHeaders(headers, default);
        const int blocks = 1024;
        var data = Enumerable.Repeat((byte)0x55, blocks * 8).ToArray();
        foreach (var prepared in new[] { false, true })
        foreach (var delay in new[] { 0, 64, 1024, 16384 })
        {
            using var cancellation = new CancellationTokenSource(); using var ready = new ManualResetEventSlim();
            using var root = new TestMemoryBudget(0);
            using var context = new OperationContext(new() { MemoryBudget = root }, cancellation.Token);
            var values = Enumerable.Repeat(2, blocks * 64).ToArray();
            var reader = new JpegEntropyReader(data, 0, context); var eob = 0;
            OperationCanceledException? error = null;
            var thread = new Thread(() => { ready.Wait(); Thread.SpinWait(delay); cancellation.Cancel(); });
            thread.Start(); ready.Set();
            try { for (var block = 0; block < blocks; block++) Assert.True(table.TryReadCancelableRefinementBlock(ref reader, values.AsSpan(block * 64, 64), 1, 63, 0, ref eob, prepared ? headers : default)); }
            catch (OperationCanceledException canceled) { error = canceled; }
            finally { thread.Join(); }
            if (error is not null) Assert.Equal(cancellation.Token, error.CancellationToken);
            else Assert.Equal(data.Length, reader.Position);
            // Every input block has one zero EOB code followed by63 correction
            // bits. Byte55 sets precisely the odd correction positions.
            // The logical byte cursor narrows the possible original bit prefixes
            // to at most eight; verify coefficients and EOB state independently.
            var matched = false;
            for (var consumed = Math.Max(0, reader.Position * 8 - 7); consumed <= reader.Position * 8; consumed++)
            {
                var phase = consumed & 63;
                if (eob != (phase == 0 ? 0 : 1) && !(phase == 1 && eob == 0)) continue;
                var exact = true;
                for (var block = 0; block < blocks && exact; block++)
                {
                    var corrected = Math.Clamp(consumed - block * 64 - 1, 0, 63);
                    for (var i = 0; i < 64; i++)
                        if (values[block * 64 + i] != (i <= corrected && (i & 1) != 0 ? 3 : 2))
                        { exact = false; break; }
                }
                if (exact) { matched = true; break; }
            }
            Assert.True(matched); Assert.Equal(0, root.Bytes); Assert.Equal(0, root.GrowthAttempts); Assert.False(root.IsDisposed);
        }
    }

    [Fact]
    public void EverySixteenBitFieldKeepsSignedUpdatesStuffingAndLogicalTail()
    {
        if (!Avx2.IsSupported || !Bmi2.X64.IsSupported) return;
        using var tables = new OperationContext(new(), default);
        var table = new JpegHuffman([1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0], [0], tables);
        using var cancellation = new CancellationTokenSource(); using var root = new TestMemoryBudget(0);
        using var context = new OperationContext(new() { MemoryBudget = root }, cancellation.Token);
        using var reference = new OperationContext(new(), default);
        for (var field = 0; field <= ushort.MaxValue; field++)
        {
            var bytes = new List<byte>();
            foreach (var value in new[] { (byte)(field >> 8), (byte)field })
            { bytes.Add(value); if (value == 255) bytes.Add(0); }
            bytes.AddRange([0x31, 0x63, 255, 0xd9]); var data = bytes.ToArray();
            var low = (field & 1) == 0 ? 0 : 13;
            var expected = Enumerable.Range(0, 64).Select(i => (2 + i % 2) * (1 << low) * (i % 3 == 0 ? -1 : 1)).ToArray();
            var actual = expected.ToArray(); var expectedReader = new JpegEntropyReader(data, 0, reference); var expectedEob = 1;
            JpegRefinementBlockTests.Reference(ref expectedReader, expected, table, 1, 16, low, ref expectedEob);
            var reader = new JpegEntropyReader(data, 0, context); var eob = 1;
            Assert.True(table.TryReadCancelableRefinementBlock(ref reader, actual, 1, 16, low, ref eob));
            Assert.Equal(expected, actual); Assert.Equal(expectedEob, eob); Assert.Equal(expectedReader.Position, reader.Position);
            Assert.Equal(expectedReader.ReadBits(8), reader.ReadBits(8)); Assert.Equal(0x63, reader.ReadBits(8));
        }
        Assert.Equal(0, root.GrowthAttempts); Assert.Equal(0, root.Bytes); Assert.False(root.IsDisposed);
    }

}
