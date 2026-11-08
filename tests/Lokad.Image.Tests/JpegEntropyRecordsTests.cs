using System.Runtime.InteropServices;
using System.Security.Cryptography;
using Lokad.Image.Internal;
using Lokad.Image.Tooling;

namespace Lokad.Image.Tests;

public sealed class JpegEntropyRecordsTests
{
    [Fact]
    public void PreparedReplayMatchesIndependentBitPackingAcrossFamiliesWidthsAndMcuEdges()
    {
        var tables = new JpegEncodingTable[4]; var records = new List<int>(); var expected = new List<byte>();
        ReadOnlySpan<int> widths = [0, 1, 7, 8, 15, 16, 31];
        for (var frame = 0; frame < 16; frame++)
        {
            for (var family = 0; family < 4; family++)
            {
                var codes = new ushort[256]; var lengths = new byte[256];
                for (var symbol = 0; symbol < 256; symbol++)
                {
                    var length = 1 + (symbol + frame + family) % 16; lengths[symbol] = (byte)length;
                    codes[symbol] = (ushort)((symbol * 251 + family * 16381 + frame * 29) & ((1 << length) - 1));
                }
                tables[family] = new(new byte[16], new byte[256], codes, lengths);
            }
            records.Clear(); expected.Clear(); var pending = 0; var used = 0;
            void Bits(int value, int count)
            {
                for (var bit = count - 1; bit >= 0; bit--)
                {
                    pending = (pending << 1) | (value >> bit & 1);
                    if (++used != 8) continue;
                    var next = (byte)pending; expected.Add(next); if (next == 255) expected.Add(0);
                    pending = used = 0;
                }
            }
            for (var family = 0; family < 4; family++)
            for (var symbol = 0; symbol < 256; symbol++)
            foreach (var bits in widths)
            {
                var payload = bits == 31 ? 65535 : ((1 << bits) - 1) & (symbol * 257 + frame * 31);
                var record = (uint)payload | (uint)symbol << 16 | (uint)family << 24 | (uint)bits << 27;
                if ((symbol & 7) == 0) record |= JpegEntropyRecords.EndMcuBit;
                records.Add(unchecked((int)record));
                Bits(tables[family].Codes[symbol], tables[family].Lengths[symbol]);
                if (bits != 31) Bits(payload, bits);
            }
            records[^1] |= (int)JpegEntropyRecords.EndMcuBit;
            if (used != 0) Bits((1 << (8 - used)) - 1, 8 - used);
            using var context = new OperationContext(new() { }, default); using var output = new MemoryStream();
            using var writer = new JpegEntropyWriter(output, context);
            JpegEntropyRecords.WriteRecords(CollectionsMarshal.AsSpan(records), tables, writer, context); writer.Align();
            Assert.True(CollectionsMarshal.AsSpan(expected).SequenceEqual(output.GetBuffer().AsSpan(0, (int)output.Length)));
        }
    }

    [Fact]
    public void EmptyReplayDoesNotInspectTablesOrPollDisposedWriter()
    {
        using var cancellation = new CancellationTokenSource();
        using var context = new OperationContext(new() { }, cancellation.Token); using var output = new MemoryStream();
        cancellation.Cancel();
        using var writer = new JpegEntropyWriter(output, context); writer.Dispose();
        JpegEntropyRecords.WriteRecords([], [], writer, context); Assert.Equal(0, output.Length);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void MissingGrayEntryPreservesPublishedPrefixAndReleasesOnlyWriterStorage(bool nullSlot)
    {
        var tables = new JpegEncodingTable[4]; var codes = new ushort[256]; var lengths = new byte[256];
        codes[0] = 91; lengths[0] = 8; tables[0] = new(new byte[16], [], codes, lengths);
        var records = Enumerable.Repeat(unchecked((int)(31U << 27 | JpegEntropyRecords.EndMcuBit)), 513).ToArray();
        records[^1] |= nullSlot ? 1 << 24 : 1 << 16;
        using var root = new TestMemoryBudget(16_384 + JpegEntropyWriter.StorageBytes);
        using var context = new OperationContext(new() { MemoryBudget = root }, default); context.Reserve(16_384);
        using var output = new MemoryStream(); using var writer = new JpegEntropyWriter(output, context);
        Assert.Throws<ImageDataException>(() => JpegEntropyRecords.WriteRecords(records, tables, writer, context));
        Assert.Equal(256, output.Length); Assert.All(output.ToArray(), b => Assert.Equal((byte)91, b));
        Assert.Equal(16_384 + JpegEntropyWriter.StorageBytes, root.Bytes);
        writer.Dispose(); Assert.Equal(16_384, root.Bytes); context.Release(16_384);
        Assert.Equal(0, root.Bytes); Assert.False(root.IsDisposed); Assert.True(output.CanWrite);
    }

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
        using var newContext = new OperationContext(new() { }, default);
        using var hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
        var totalBlocks = 0; var totalRecords = 0; var maximumPerBlock = 0;
        for (var test = 0; test < 4096; test++)
        {
            var chroma = (test & 1) == 0 ? 1 : 2; var blocks = 4 + chroma * 2;
            Input(coefficients, test, blocks); Array.Clear(newPrevious);
            foreach (var values in newHistogram) Array.Clear(values);
            newOutput.SetLength(0);
            using var newWriter = new JpegEntropyWriter(newOutput, newContext);
            var used = 0;             for (var b = 0; b < blocks; b++)
            {
                var block = coefficients.AsSpan(b * 64, 64); var component = Component(b, chroma);
                records.AsSpan(used, 64).Fill(0x5a5a5a5a);
                var count = JpegEntropyRecords.RecordBlock(block, component, newPrevious, newHistogram, records.AsSpan(used, 64));
                var untouched = true;
                foreach (var value in records.AsSpan(used + count, 64 - count)) untouched &= value == 0x5a5a5a5a;
                Assert.True(untouched);
                if (count is < 1 or > 64) throw new Exception("Record capacity bound exceeded");
                maximumPerBlock = Math.Max(maximumPerBlock, count); used += count; totalBlocks++;
            }
            records[used - 1] |= (int)JpegEntropyRecords.EndMcuBit;
            JpegEntropyRecords.WriteRecords(records.AsSpan(0, used), tables, newWriter, newContext);
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
