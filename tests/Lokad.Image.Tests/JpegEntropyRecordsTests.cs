using System.Runtime.InteropServices;
using System.Security.Cryptography;
using Lokad.Image.Internal;
using Lokad.Image.Tooling;

namespace Lokad.Image.Tests;

public sealed class JpegEntropyRecordsTests
{
    private uint state = 0x6d2b79f5;
    private uint Next() { state ^= state << 13; state ^= state >> 17; state ^= state << 5; return state; }
    private void Input(Span<short> data, int test, int blocks)
    {
        data.Clear();
        for (var b = 0; b < blocks; b++)
        {
            var block = data.Slice(b * 64, 64); var mode = test % 8;
            block[0] = mode == 0 ? (short)0 : mode == 4 ? (b % 2 == 0 ? short.MinValue : short.MaxValue) : unchecked((short)Next());
            for (var k = 1; k < 64; k++)
            {
                var value = mode switch {
                    0 => 0, 1 => (k & 1) == 0 ? -1 : 1,
                    2 => (int)(Next() % 31) - 15,
                    3 => k == 1 + test % 63 ? ((test & 1) == 0 ? -1023 : 1023) : 0,
                    4 => (k & 1) == 0 ? -32767 : 32767,
                    5 => k % 17 == 0 ? -1 : 0,
                    6 => k == 1 ? short.MinValue : 0,
                    _ => (Next() & 7) == 0 ? 0 : (int)(Next() % 2047) - 1023 };
                block[JpegTransform.Zigzag[k]] = (short)value;
            }
        }
    }
    private static int Component(int block, int chromaBlocks) => block < 4 ? 0 : block < 4 + chromaBlocks ? 1 : 2;
    [Fact]
    public void FrozenOriginalEntropyFragmentsAndHistogramsRemainExact()
    {
        var coefficients = new short[8 * 64]; var records = new int[8 * 64];
        var newPrevious = new int[3];
        var newHistogram = new int[4][]; var tables = new JpegEncodingTable[4];
        for (var table = 0; table < 4; table++)
        {
newHistogram[table] = new int[256];
            var codes = new ushort[256]; var lengths = new byte[256];
            for (var symbol = 0; symbol < 256; symbol++) { codes[symbol] = (ushort)(table * 16384 + symbol); lengths[symbol] = 16; }
            tables[table] = new(new byte[16], new byte[256], codes, lengths);
        }
        using var newOutput = new MemoryStream(8192);
        using var newContext = new OperationContext(new() { MaxWork = 10_000_000_000 }, default);
        using var hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
        var totalBlocks = 0; var totalRecords = 0; var maximumPerBlock = 0;
        for (var test = 0; test < 4096; test++)
        {
            var chroma = (test & 1) == 0 ? 1 : 2; var blocks = 4 + chroma * 2;
            Input(coefficients, test, blocks); Array.Clear(newPrevious);
            foreach (var values in newHistogram) Array.Clear(values);
            newOutput.SetLength(0);
            using var newWriter = new JpegEntropyWriter(newOutput, newContext);
            var used = 0; newContext.Work(262144);
            for (var b = 0; b < blocks; b++)
            {
                var block = coefficients.AsSpan(b * 64, 64); var component = Component(b, chroma);
                var count = JpegEntropyRecords.RecordBlock(block, component, newPrevious, newHistogram, records.AsSpan(used, 64));
                if (count is < 1 or > 64) throw new Exception("Record capacity bound exceeded");
                maximumPerBlock = Math.Max(maximumPerBlock, count); used += count; totalBlocks++;
            }
            records[used - 1] |= (int)JpegEntropyRecords.EndMcuBit;
            JpegEntropyRecords.WriteRecords(records.AsSpan(0, used), tables, newWriter, newContext, 262144);
            newWriter.Align();
            var before = newOutput.GetBuffer().AsSpan(0, (int)newOutput.Length);
            hash.AppendData(MemoryMarshal.AsBytes(coefficients.AsSpan(0, blocks * 64))); hash.AppendData(before);
            for (var table = 0; table < 4; table++)
            {
                hash.AppendData(MemoryMarshal.AsBytes(newHistogram[table].AsSpan()));
            }
            totalRecords += used;
        }
        var digest = Convert.ToHexStringLower(hash.GetHashAndReset());
        Assert.Equal(28672, totalBlocks); Assert.Equal(946249, totalRecords); Assert.Equal(64, maximumPerBlock);
        Assert.Equal("2422c6620b1885e9e7535eefd309adfe4d958ae80c315e4f3022847f37014c0d", digest);
    }

    [Fact]
    public void GrowingCapacityKeepsBothArraysChargedUntilReplacementCompletes()
    {
        using var root = new TestMemoryBudget(10000);
        using var context = new OperationContext(new() { MemoryBudget = root }, default);
        using var records = new JpegEntropyRecords(context, 64);
        records.BeginBlock().Fill(17); records.Commit(64); Assert.Equal(384, root.Bytes);
        root.Admitted = _ => { Assert.Equal(896, root.Bytes); Assert.Equal(64, records.Count); Assert.All(records.Values.ToArray(), x => Assert.Equal(17, x)); };
        var next = records.BeginBlock(); root.Admitted = null;
        Assert.Equal(896, root.Peak); Assert.Equal(640, root.Bytes); Assert.Equal(64, records.Count);
        Assert.All(records.Values.ToArray(), x => Assert.Equal(17, x)); Assert.True(next.IndexOfAnyExcept(0) < 0);
        next[0] = 23; records.Commit(1); Assert.Equal(23, records.Values[^1]);
        records.Dispose(); records.Dispose(); Assert.Equal(0, root.Bytes); Assert.False(root.IsDisposed);
        Assert.Throws<ObjectDisposedException>(() => records.Values.Length);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void RefusalOrCancellationDuringGrowthKeepsOldStorageAndReleasesUnusedReservation(bool cancel)
    {
        using var root = new TestMemoryBudget(10000); using var cancellation = new CancellationTokenSource();
        using var context = new OperationContext(new() { MemoryBudget = root }, cancellation.Token);
        using var records = new JpegEntropyRecords(context, 64);
        records.BeginBlock().Fill(41); records.Commit(64); var attempt = root.GrowthAttempts + 1;
        if (cancel) root.Admitted = _ => { if (root.GrowthAttempts == attempt) cancellation.Cancel(); };
        else root.RefuseAtGrowth = attempt;
        if (cancel) Assert.Throws<OperationCanceledException>(() => records.BeginBlock().Length);
        else
        {
            var error = Assert.Throws<ImageLimitException>(() => records.BeginBlock().Length);
            Assert.Equal((ImageLimitKind.WorkingBytes, ImageLimitScope.SharedBudget), (error.Limit, error.Scope));
        }
        Assert.Equal(attempt, root.GrowthAttempts); Assert.Equal(cancel ? 0 : 1, root.Refusals);
        Assert.Equal(384, root.Bytes); Assert.Equal(64, records.Count); Assert.All(records.Values.ToArray(), x => Assert.Equal(41, x));
        records.Dispose(); Assert.Equal(0, root.Bytes); root.Admitted = null; root.RefuseAtGrowth = 0;
        using var nextContext = new OperationContext(new() { MemoryBudget = root }, default);
        using var next = new JpegEntropyRecords(nextContext, 64); next.BeginBlock().Clear(); next.Commit(64); next.BeginBlock().Clear();
        Assert.Equal(640, root.Bytes); next.Dispose(); Assert.Equal(0, root.Bytes); Assert.False(root.IsDisposed);
    }

    [Fact]
    public void DeclaredLocalLimitRefusesBeforeAskingTheSharedRootToGrow()
    {
        using var root = new TestMemoryBudget(10000);
        using var context = new OperationContext(new() { MemoryBudget = root, MaxWorkingBytes = 895 }, default);
        using var records = new JpegEntropyRecords(context, 64); records.BeginBlock().Fill(19); records.Commit(64);
        var attempts = root.GrowthAttempts;
        var error = Assert.Throws<ImageLimitException>(() => records.BeginBlock().Length);
        Assert.Equal((ImageLimitKind.WorkingBytes, ImageLimitScope.Operation), (error.Limit, error.Scope));
        Assert.Equal(attempts, root.GrowthAttempts); Assert.Equal(0, root.Refusals); Assert.Equal(384, root.Bytes);
        Assert.All(records.Values.ToArray(), x => Assert.Equal(19, x)); records.Dispose(); Assert.Equal(0, root.Bytes);
    }

    [Fact]
    public void AggregateAdmissionAllowsGrowthOnlyAfterTheOtherOwnerEnds()
    {
        using var root = new TestMemoryBudget(896);
        using var firstContext = new OperationContext(new() { MemoryBudget = root }, default);
        using var secondContext = new OperationContext(new() { MemoryBudget = root }, default);
        using var first = new JpegEntropyRecords(firstContext, 64); using var second = new JpegEntropyRecords(secondContext, 64);
        first.BeginBlock().Clear(); first.Commit(64); Assert.Equal(768, root.Bytes);
        var error = Assert.Throws<ImageLimitException>(() => first.BeginBlock().Length);
        Assert.Equal(ImageLimitScope.SharedBudget, error.Scope); Assert.Equal(1, root.Refusals); Assert.Equal(768, root.Bytes);
        second.Dispose(); Assert.Equal(384, root.Bytes); first.BeginBlock().Clear();
        Assert.Equal(896, root.Peak); Assert.Equal(640, root.Bytes); first.Dispose(); Assert.Equal(0, root.Bytes); Assert.False(root.IsDisposed);
    }
}
