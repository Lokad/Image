using Lokad.Image.Internal;
using Lokad.Image.Tooling;

namespace Lokad.Image.Tests;

public sealed partial class ZlibMaterializationTests
{
    [Fact]
    public void FullHistorySlabThenEmptyFinalBlockKeepsExactEndAndFailures()
    {
        // Original RGB8-sized None rows. Stored history and maximum-distance
        // copies end exactly at 64 KiB, followed by a separate empty block.
        var raw = new byte[65536];
        for (var i = 0; i < 32768; i++) raw[i] = i % 256 == 0 ? (byte)0 : (byte)(73 * i + 19);
        raw.AsSpan(0, 32768).CopyTo(raw.AsSpan(32768));
        var bits = new Writer(); bits.Number(0, 3); bits.Align();
        bits.Number(32768, 16); bits.Number(32768 ^ 65535, 16);
        foreach (var value in raw.AsSpan(0, 32768)) bits.Number(value, 8);
        bits.Number(0, 1); bits.Number(1, 2);
        for (var i = 0; i < 127; i++)
        {
            bits.Code(197, 8); bits.Code(29, 5); bits.Number(8191, 13);
        }
        bits.Literal(raw[^2]); bits.Literal(raw[^1]); bits.Code(0, 7);
        bits.Number(1, 1); bits.Number(1, 2); bits.Code(0, 7);
        var encoded = Envelope(bits, raw); var original = encoded.ToArray();
        Assert.Equal(raw, Inflate(encoded));
        using var cancellation = new CancellationTokenSource();
        foreach (var cut in Enumerable.Range(0, 11))
        foreach (var size in new[] { raw.Length - 1, raw.Length, raw.Length + 1 })
        foreach (var cancelable in new[] { false, true })
        {
            var input = encoded[..(encoded.Length - cut)];
            using var root = new TestMemoryBudget(4096);
            var limits = ImageLimits.Default with { MaxWorkingBytes = 4096, MemoryBudget = root };
            using var context = new OperationContext(limits, cancelable ? cancellation.Token : default);
            using var reference = new OperationContext(limits, default);
            var rows = new PngRowInflation(255, 256, new byte[PngRowInflation.HistoryBytes]);
            int end = -1, countedEnd = -1;
            var actual = Record.Exception(() => end = ZlibFraming.ValidateAndInflateRows(input, size, context, rows, out _));
            var expected = Record.Exception(() => countedEnd = ZlibFraming.Validate(input, size, reference));
            Assert.Equal(expected?.GetType(), actual?.GetType()); Assert.Equal(expected?.Message, actual?.Message);
            Assert.Equal(countedEnd, end);
            if (actual is null)
            {
                Assert.Equal(encoded.Length, end);
                Assert.True(rows.Finish(System.Buffers.Binary.BinaryPrimitives.ReadUInt32BigEndian(encoded.AsSpan(encoded.Length - 4))));
            }
            Assert.Equal(0, root.Bytes); Assert.False(root.IsDisposed);
            Assert.Equal(4096, context.RemainingWorkingBytes);
        }
        Assert.Equal(original, encoded);
    }

    [Fact]
    public void MaterializedCursorMatchesOriginalAcrossBlockPhasesMatchesFailuresAndEveryTruncation()
    {
        for (var phase = 0; phase < 8; phase++)
        {
            var bits = new Writer(); var raw = new List<byte>();
            bits.Number(0, 1); bits.Number(1, 2);
            foreach (var value in Enumerable.Repeat((byte)144, phase).Concat(Enumerable.Range(0, 32).Select(i => (byte)(i * 73 + 19))))
            { bits.Literal(value); raw.Add(value); }
            bits.Code(0, 7);
            bits.Number(1, 1); bits.Number(1, 2);
            // Original explicit RFC1951 examples, including maximum extra fields
            // and repeating history whose source overlaps the destination.
            foreach (var (code, codeBits, extra, extraBits, length, distanceCode, distanceExtra, distanceBits, distance) in new[] {
                (1,7,0,0,3,0,0,0,1), (9,7,1,1,12,2,0,0,3),
                (13,7,3,2,22,4,1,1,6), (196,8,30,5,257,8,0,3,17),
                (197,8,0,0,258,0,0,0,1) })
            {
                bits.Code(code, codeBits); bits.Number(extra, extraBits);
                bits.Code(distanceCode, 5); bits.Number(distanceExtra, distanceBits);
                for (var i = 0; i < length; i++) raw.Add(raw[^distance]);
            }
            bits.Code(0, 7);
            var encoded = Envelope(bits, raw.ToArray()); Assert.Equal(raw.ToArray(), Inflate(encoded));
            for (var count = 0; count <= encoded.Length; count++)
            foreach (var size in new[] {raw.Count - 1, raw.Count, raw.Count + 1})
                CompareMaterializedCursors(encoded[..count], size);
            CompareMaterializedCursors([.. encoded, 222, 173, 190, 239], raw.Count);
            foreach (var symbol in new[] {286,287})
            {
                var invalid = new Writer(); invalid.Number(1,1); invalid.Number(1,2);
                invalid.Code(symbol - 88,8); // Reserved fixed length codes.
                CompareMaterializedCursors(Envelope(invalid, []), 4);
            }
            foreach (var distanceCode in new[] {1,30,31})
            {
                var invalid = new Writer(); invalid.Number(1,1); invalid.Number(1,2);
                invalid.Literal(65); invalid.Code(1,7); invalid.Code(distanceCode,5);
                CompareMaterializedCursors(Envelope(invalid, []), 4);
            }
        }
    }

    [Fact]
    public void MaterializedCursorPublishesCanonicalDistanceFallbackOnSuccessAndFailure()
    {
        var bits = new Writer(); bits.Number(0,3); bits.Align();
        bits.Number(512,16); bits.Number(512^65535,16);
        for (var i = 0; i < 512; i++) bits.Number(0,8);
        bits.Number(1,1); bits.Number(2,2); bits.Number(1,5); bits.Number(15,5); bits.Number(15,4);
        ReadOnlySpan<int> order = [16,17,18,0,8,7,9,6,10,5,11,4,12,3,13,2,14,1,15];
        foreach (var symbol in order) bits.Number(symbol<16 ? 4 : 0,3);
        for (var symbol = 0; symbol < 258; symbol++) bits.Code(symbol>=256 ? 1 : 0,4);
        // Complete distance comb: lengths1..14,15,15; both last codes exceed
        // the five-bit root, then require six extra bits.
        for (var symbol = 0; symbol < 16; symbol++) bits.Code(Math.Min(symbol+1,15),4);
        bits.Code(1,1); bits.Code(32767,15); bits.Number(63,6); bits.Code(0,1);
        var encoded = Envelope(bits, new byte[515]); Assert.Equal(new byte[515],Inflate(encoded));
        for (var count = 0; count <= encoded.Length; count++) CompareMaterializedCursors(encoded[..count],515);
        CompareMaterializedCursors(encoded,514);
    }

    private static void CompareMaterializedCursors(byte[] encoded, int size)
    {
        var original = encoded.ToArray(); using var cancellation = new CancellationTokenSource();
        using var root = new TestMemoryBudget(4096);
        var limits = ImageLimits.Default with {MaxWorkingBytes=4096,MemoryBudget=root};
        using var fast = new OperationContext(limits, default);
        using var reference = new OperationContext(limits, cancellation.Token);
        var first = Enumerable.Repeat((byte)165,size+16).ToArray(); var second = first.ToArray();
        int firstEnd=-1,secondEnd=-1;bool firstComplete=false,secondComplete=false;
        var firstError = Record.Exception(() => firstEnd=ZlibFraming.ValidateAndInflate(encoded,first.AsSpan(8,size),fast,out firstComplete));
        Assert.Equal(0,root.Bytes);Assert.False(root.IsDisposed);
        var secondError = Record.Exception(() => secondEnd=ZlibFraming.ValidateAndInflate(encoded,second.AsSpan(8,size),reference,out secondComplete));
        Assert.Equal(secondError?.GetType(),firstError?.GetType());Assert.Equal(secondError?.Message,firstError?.Message);
        Assert.Equal(secondEnd,firstEnd);Assert.Equal(secondComplete,firstComplete);
        Assert.Equal(4096,fast.RemainingWorkingBytes);Assert.Equal(4096,reference.RemainingWorkingBytes);
        Assert.Equal(second,first);Assert.Equal(original,encoded);Assert.Equal(0,root.Bytes);
        Assert.All(first[..8],value=>Assert.Equal(165,value));Assert.All(first[(8+size)..],value=>Assert.Equal(165,value));
    }
}
