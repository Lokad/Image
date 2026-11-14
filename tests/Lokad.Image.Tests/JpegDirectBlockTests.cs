using Lokad.Image.Internal;
using Lokad.Image.Tooling;

namespace Lokad.Image.Tests;

public sealed class JpegDirectBlockTests
{
    // Generate the T.81 diagonal order independently of the production table.
    private static readonly int[] Order = Enumerable.Range(0, 15).SelectMany(diagonal =>
    {
        var rows = Enumerable.Range(Math.Max(0, diagonal - 7), Math.Min(7, diagonal) - Math.Max(0, diagonal - 7) + 1);
        if ((diagonal & 1) == 0) rows = rows.Reverse();
        return rows.Select(row => row * 8 + diagonal - row);
    }).ToArray();

    private sealed class Oracle
    {
        internal readonly List<byte> Bytes = [];
        private int pending, available;
        internal void Bits(int value, int count)
        {
            for (var bit = count - 1; bit >= 0; bit--)
            {
                pending = pending * 2 + (value >> bit & 1);
                if (++available != 8) continue;
                Bytes.Add((byte)pending); if (pending == 255) Bytes.Add(0);
                pending = available = 0;
            }
        }
        internal void Align() { if (available != 0) Bits((1 << (8 - available)) - 1, 8 - available); }
        internal void Restart(int index) { Align(); Bytes.Add(255); Bytes.Add((byte)(0xd0 + (index & 7))); }
        internal void Block(short[] coefficients, int component, int[] previous, JpegEncodingTable[] tables)
        {
            var family = component == 0 ? 0 : 1;
            void Symbol(int table, int symbol) => Bits(tables[table].Codes[symbol], tables[table].Lengths[symbol]);
            void Coefficient(int table, int run, int value)
            {
                var magnitude = Math.Abs(value); var count = 0;
                for (var remaining = magnitude; remaining != 0; remaining /= 2) count++;
                Symbol(table, run * 16 + count);
                Bits(value < 0 ? ((1 << count) - 1) ^ magnitude : magnitude, count);
            }
            var difference = coefficients[0] - previous[component]; previous[component] = coefficients[0];
            Coefficient(family, 0, difference);
            var zeros = 0;
            for (var i = 1; i < 64; i++)
            {
                var value = coefficients[Order[i]];
                if (value == 0) { zeros++; continue; }
                while (zeros >= 16) { Symbol(family + 2, 240); zeros -= 16; }
                Coefficient(family + 2, zeros, value); zeros = 0;
            }
            if (zeros != 0) Symbol(family + 2, 0);
        }
    }

    private static JpegEncodingTable[] Tables(bool stuffed = false)
    {
        var tables = new JpegEncodingTable[4];
        for (var family = 0; family < 4; family++)
        {
            var codes = new ushort[256]; var lengths = new byte[256];
            for (var symbol = 0; symbol < 256; symbol++)
            {
                var length = stuffed ? 16 : 1 + (symbol * 3 + family * 5) % 16;
                lengths[symbol] = (byte)length;
                codes[symbol] = stuffed ? ushort.MaxValue : (ushort)((symbol * 257 + family * 16381) & ((1 << length) - 1));
            }
            tables[family] = new(new byte[16], [], codes, lengths);
        }
        return tables;
    }

    [Fact]
    public void BlocksMatchIndependentRunPackingWithPredictorsTailsAndRestarts()
    {
        var tables = Tables(); var coefficients = new short[64]; var actualPrevious = new int[3]; var expectedPrevious = new int[3];
        var oracle = new Oracle(); uint random = 0x6d2b79f5;
        uint Next() { random ^= random << 13; random ^= random >> 17; random ^= random << 5; return random; }
        using var context = new OperationContext(ImageLimits.Default, default); using var output = new MemoryStream();
        using var writer = new JpegEntropyWriter(output, context);
        for (var test = 0; test < 4096; test++)
        {
            if (test % 37 == 0)
            {
                writer.Restart(test / 37); oracle.Restart(test / 37); Array.Clear(actualPrevious); Array.Clear(expectedPrevious);
            }
            var tail = test & 7; var prefix = (1 << tail) - 1;
            writer.Write(prefix, tail); oracle.Bits(prefix, tail);
            Array.Clear(coefficients); coefficients[0] = (test & 7) == 0 ? (short)0 : unchecked((short)Next());
            for (var i = 1; i < 64; i++) coefficients[i] = (test % 5) switch
            {
                0 => 0, 1 => (short)((i & 1) == 0 ? -1023 : 1023),
                2 => i == 63 ? (short)-1 : (short)0,
                3 => i % 17 == 0 ? (short)1 : (short)0,
                _ => (short)((int)(Next() % 2047) - 1023)
            };
            writer.WriteBlock(coefficients, test % 3, actualPrevious, tables);
            oracle.Block(coefficients, test % 3, expectedPrevious, tables);
            writer.Write(test & 7, 3); oracle.Bits(test & 7, 3);
            Assert.Equal(expectedPrevious, actualPrevious);
        }
        writer.Align(); oracle.Align(); Assert.Equal(oracle.Bytes, output.ToArray());
    }

    [Fact]
    public void EveryDcCategoryKeepsSignedPayloadWithUnusedGrayTableSlots()
    {
        var tables = Tables(); tables[1] = tables[3] = null!;
        for (var count = 0; count <= 16; count++)
        foreach (var negative in new[] { false, true })
        {
            var magnitude = count == 0 ? 0 : (1 << count) - 1;
            var value = negative ? -magnitude : magnitude;
            var coefficients = new short[64];
            var previous = new[] { count == 16 ? negative ? 32767 : -32768 : 0 };
            coefficients[0] = (short)(previous[0] + value); var expectedPrevious = previous.ToArray();
            var oracle = new Oracle(); oracle.Block(coefficients, 0, expectedPrevious, tables); oracle.Align();
            using var context = new OperationContext(ImageLimits.Default, default); using var output = new MemoryStream();
            using var writer = new JpegEntropyWriter(output, context); writer.WriteBlock(coefficients, 0, previous, tables); writer.Align();
            Assert.Equal(expectedPrevious, previous); Assert.Equal(oracle.Bytes, output.ToArray());
        }
    }

    [Fact]
    public void CompleteDcDifferenceDomainMatchesIndependentSignedPacking()
    {
        var tables = Tables(); var coefficients = new short[64]; var previous = new int[1]; var expectedPrevious = new int[1];
        using var context = new OperationContext(ImageLimits.Default, default); using var output = new MemoryStream();
        for (var difference = -65535; difference <= 65535; difference++)
        {
            previous[0] = expectedPrevious[0] = difference < 0 ? short.MaxValue : short.MinValue;
            coefficients[0] = checked((short)(previous[0] + difference)); output.SetLength(0);
            var oracle = new Oracle(); oracle.Block(coefficients, 0, expectedPrevious, tables); oracle.Align();
            using var writer = new JpegEntropyWriter(output, context); writer.WriteBlock(coefficients, 0, previous, tables); writer.Align();
            Assert.Equal(expectedPrevious, previous); Assert.True(oracle.Bytes.SequenceEqual(output.ToArray()));
        }
    }

    [Theory]
    [InlineData(false)] [InlineData(true)]
    public void FirstAdmissionRefusalOrCancellationRefundsAndPermitsRecovery(bool cancel)
    {
        using var root = new TestMemoryBudget(JpegEntropyWriter.StorageBytes); using var cancellation = new CancellationTokenSource();
        if (cancel) root.Admitted = _ => cancellation.Cancel(); else root.RefuseAtGrowth = 1;
        var tables = Tables(true); var coefficients = new short[64]; var previous = new int[3];
        using var context = new OperationContext(new() { MemoryBudget = root }, cancellation.Token);
        using var output = new MemoryStream(); using var writer = new JpegEntropyWriter(output, context);
        if (cancel) Assert.Throws<OperationCanceledException>(() => writer.WriteBlock(coefficients, 0, previous, tables));
        else Assert.Throws<ImageLimitException>(() => writer.WriteBlock(coefficients, 0, previous, tables));
        writer.Dispose(); Assert.Equal(0, root.Bytes); Assert.Equal(0, output.Length); Assert.Equal(1, root.GrowthAttempts);
        root.Admitted = null; root.RefuseAtGrowth = 0;
        using var nextContext = new OperationContext(new() { MemoryBudget = root }, default);
        using var next = new JpegEntropyWriter(output, nextContext); next.WriteBlock(coefficients, 0, previous, tables); next.Align(); next.Dispose();
        Assert.Equal(0, root.Bytes); Assert.True(output.Length > 0); Assert.True(output.CanWrite); Assert.False(root.IsDisposed);
    }

    private sealed class ObservedOutput(Action onWrite) : MemoryStream
    {
        internal int Writes;
        public override void Write(ReadOnlySpan<byte> bytes) { Writes++; onWrite(); base.Write(bytes); }
    }

    [Theory]
    [InlineData(false)] [InlineData(true)]
    public void OutputFailureKeepsActualUserChargedAndPreservesPublishedPrefix(bool cancel)
    {
        using var root = new TestMemoryBudget(JpegEntropyWriter.StorageBytes); using var cancellation = new CancellationTokenSource();
        using var context = new OperationContext(new() { MemoryBudget = root }, cancellation.Token);
        using var output = new ObservedOutput(() =>
        {
            Assert.Equal(JpegEntropyWriter.StorageBytes, root.Bytes);
            if (cancel) cancellation.Cancel(); else throw new IOException("Authored output failure");
        });
        var tables = Tables(true); var coefficients = Enumerable.Repeat((short)1023, 64).ToArray(); var previous = new int[3];
        var oracle = new Oracle(); var expectedPrevious = new int[3];
        for (var i = 0; i < 16; i++) oracle.Block(coefficients, 0, expectedPrevious, tables);
        using var writer = new JpegEntropyWriter(output, context);
        void Encode() { for (var i = 0; i < 16; i++) writer.WriteBlock(coefficients, 0, previous, tables); }
        if (cancel) Assert.Throws<OperationCanceledException>(Encode); else Assert.Throws<IOException>(Encode);
        Assert.Equal(1, output.Writes); Assert.Equal(oracle.Bytes.Take(cancel ? 256 : 0), output.ToArray());
        writer.Dispose(); writer.Dispose(); Assert.Equal(0, root.Bytes); Assert.Equal(1, output.Writes);
        Assert.True(output.CanWrite); Assert.False(root.IsDisposed);
    }

    [Fact]
    public void MissingSelectedCodeKeepsImageDataFailureAndOwnedCleanup()
    {
        var tables = Tables(true); tables[0].Lengths[0] = 0;
        using var root = new TestMemoryBudget(JpegEntropyWriter.StorageBytes);
        using var context = new OperationContext(new() { MemoryBudget = root }, default); using var output = new MemoryStream();
        using var writer = new JpegEntropyWriter(output, context);
        Assert.Throws<ImageDataException>(() => writer.WriteBlock(new short[64], 0, new int[3], tables));
        Assert.Equal(0, output.Length); Assert.Equal(0, root.Attempts); writer.Dispose(); Assert.Equal(0, root.Bytes);
    }
}
