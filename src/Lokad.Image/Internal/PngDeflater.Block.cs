using System.Buffers.Binary;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Lokad.Image.Internal;

internal sealed partial class PngDeflater
{
    // RFC1951 section3.2.7 format permutation.
    private static ReadOnlySpan<int> Order => [16, 17, 18, 0, 8, 7, 9, 6, 10, 5, 11, 4, 12, 3, 13, 2, 14, 1, 15];

    private static int LengthSymbol(int length)
    {
        if (length == 258) return 285;
        if (length <= 10) return length + 254;
        var extra = 1;
        while (length >= 3 + (8 << extra)) extra++;
        return 261 + extra * 4 + ((length - 3) >> extra) - 4;
    }
    private static int DistanceSymbol(int distance)
    {
        if (distance <= 4) return distance - 1;
        // RFC1951 section3.2.5: each doubling has two equal-width ranges.
        var extra = BitOperations.Log2((uint)(distance - 1)) - 1;
        return extra * 2 + ((distance - 1) >> extra);
    }
    private static int LengthExtra(int symbol) => symbol is >= 265 and < 285 ? (symbol - 261) / 4 : 0;
    private static int DistanceExtra(int symbol) => symbol < 4 ? 0 : symbol / 2 - 1;

    private void Block(bool final)
    {
        context.Cancellation.ThrowIfCancellationRequested();
        var workspace = treeWorkspace!;
        var frequencies = workspace.BlockFrequencies.AsSpan();
        if (level != 1) frequencies.Clear();
        var literalFrequency = frequencies[..286]; var distanceFrequency = frequencies.Slice(286, 30); literalFrequency[256] = 1;
        var expanded = level == 1 ? fastExpanded : 0;
        if (level != 1)
        for (var index = 0; index < tokenCount; index++)
        {
            var token = tokens[index]; var back = (int)(token >> 9);
            if (back == 0) { literalFrequency[(int)token]++; expanded++; }
            else
            {
                var length = (int)(token & 511); expanded += length;
                literalFrequency[LengthSymbol(length)]++; distanceFrequency[DistanceSymbol(back)]++;
            }
        }
        var literals = DeflateHuffman.Lengths(literalFrequency, 15, context, treeWorkspace!);
        var distances = DeflateHuffman.Lengths(distanceFrequency, 15, context, treeWorkspace!);
        Span<DeflateHuffman.Run> runs = stackalloc DeflateHuffman.Run[316];
        var runCount = DeflateHuffman.Runs(literals, runs);
        runCount += DeflateHuffman.Runs(distances, runs[runCount..]);
        var lengthFrequency = frequencies.Slice(316, 19);
        for (var index = 0; index < runCount; index++) lengthFrequency[runs[index].Symbol]++;
        var lengths = DeflateHuffman.Lengths(lengthFrequency, 7, context, treeWorkspace!); Array.Resize(ref lengths, 19);
        var lengthCount = 19;
        while (lengthCount > 4 && lengths[Order[lengthCount - 1]] == 0) lengthCount--;
        var dynamicBits = 3 + 5 + 5 + 4 + lengthCount * 3; var fixedBits = 3;
        for (var index = 0; index < runCount; index++) dynamicBits += lengths[runs[index].Symbol] + runs[index].Bits;
        for (var symbol = 0; symbol < literalFrequency.Length; symbol++)
        {
            var extra = symbol >= 257 ? LengthExtra(symbol) : 0;
            dynamicBits += literalFrequency[symbol] * ((symbol < literals.Length ? literals[symbol] : 0) + extra);
            fixedBits += literalFrequency[symbol] * (FixedLength(symbol) + extra);
        }
        for (var symbol = 0; symbol < distanceFrequency.Length; symbol++)
        {
            var extra = DistanceExtra(symbol);
            dynamicBits += distanceFrequency[symbol] * ((symbol < distances.Length ? distances[symbol] : 0) + extra);
            fixedBits += distanceFrequency[symbol] * (5 + extra);
        }
        var dynamicBytes = (dynamicBits + 7) / 8; var fixedBytes = (fixedBits + 7) / 8;
        if (blockStart >= windowOrigin && expanded <= 65535 && expanded + 4 <= Math.Min(dynamicBytes, fixedBytes))
        {
            Put(final ? 1 : 0, 3); Align();
            OutputByte((byte)expanded); OutputByte((byte)(expanded >> 8));
            OutputByte((byte)~expanded); OutputByte((byte)(~expanded >> 8));
            var offset = (int)(blockStart - windowOrigin);
            OutputBytes(history.AsSpan(offset, expanded));
        }
        else
        {
            var useFixed = fixedBytes <= dynamicBytes;
            Put((final ? 1 : 0) | (useFixed ? 2 : 4), 3);
            if (useFixed)
            {
                literals = new byte[288]; distances = new byte[32]; Array.Fill(distances, (byte)5);
                for (var symbol = 0; symbol < literals.Length; symbol++) literals[symbol] = (byte)FixedLength(symbol);
            }
            else
            {
                Put(literals.Length - 257, 5); Put(distances.Length - 1, 5); Put(lengthCount - 4, 4);
                for (var index = 0; index < lengthCount; index++) Put(lengths[Order[index]], 3);
                var codes = workspace.BlockCodes.AsSpan(0, 19); DeflateHuffman.Codes(lengths, codes);
                for (var index = 0; index < runCount; index++)
                { var run = runs[index]; Put(codes[run.Symbol], lengths[run.Symbol]); Put(run.Extra, run.Bits); }
            }
            WriteTokens(literals, distances);
        }
        blockStart += expanded; tokenCount = 0;
        if (level == 1) { frequencies.Clear(); fastExpanded = 0; }
    }

    private void WriteTokens(byte[] literalLengths, byte[] distanceLengths)
    {
        var literals = treeWorkspace!.BlockCodes.AsSpan(0, 288); var distances = treeWorkspace.BlockCodes.AsSpan(288, 32);
        DeflateHuffman.Codes(literalLengths, literals); DeflateHuffman.Codes(distanceLengths, distances);
        var input = tokens; var count = tokenCount; var cursor = new TokenCursor(this);
        try
        {
            for (var index = 0; index < count; index++)
            {
                var token = input[index]; var back = (int)(token >> 9);
                if (back == 0)
                {
                    if (index + 2 < count && (input[index + 1] | input[index + 2]) < 256)
                    {
                        var second = (int)input[index + 1]; var third = (int)input[index + 2];
                        var firstBits = literalLengths[(int)token]; var secondBits = literalLengths[second];
                        var width = firstBits + secondBits + literalLengths[third];
                        var value = (ulong)(uint)literals[(int)token] | (ulong)(uint)literals[second] << firstBits
                            | (ulong)(uint)literals[third] << (firstBits + secondBits);
                        cursor.Put(value, width); index += 2; continue;
                    }
                    cursor.Put((ulong)(uint)literals[(int)token], literalLengths[(int)token]); continue;
                }
                var length = (int)(token & 511); var ls = LengthSymbol(length); var ds = DistanceSymbol(back);
                var le = LengthExtra(ls); var de = DistanceExtra(ds);
                var lb = ls < 265 ? ls - 254 : ls == 285 ? 258 : 3 + ((4 + (ls - 265) % 4) << le);
                var db = ds < 4 ? ds + 1 : 1 + ((2 + ds % 2) << de);
                var firstWidth = literalLengths[ls]; var secondWidth = firstWidth + le;
                var thirdWidth = secondWidth + distanceLengths[ds];
                var match = (ulong)(uint)literals[ls] | (ulong)(uint)(length - lb) << firstWidth
                    | (ulong)(uint)distances[ds] << secondWidth | (ulong)(uint)(back - db) << thirdWidth;
                cursor.Put(match, thirdWidth + de);
            }
            cursor.Put((ulong)(uint)literals[256], literalLengths[256]); cursor.Finish();
        }
        finally { cursor.Commit(); }
    }

    // A synchronous borrower of the existing encoded buffer. State is published
    // before calling the output stream and on every exit, including failed writes.
    private ref struct TokenCursor
    {
        private readonly PngDeflater owner;
        private readonly byte[] buffer;
        private ulong pending;
        private int pendingBits, position;

        internal TokenCursor(PngDeflater owner)
        {
            this.owner = owner; buffer = owner.encoded;
            pending = owner.pending; pendingBits = owner.pendingBits; position = owner.encodedCount;
        }

        // A complete match occupies at most48bits. Retaining up to31bits
        // amortizes output over words; drain the rare overflow before appending.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void Put(ulong value, int width)
        {
            if (pendingBits + width > 64) Finish();
            pending |= value << pendingBits; pendingBits += width;
            while (pendingBits >= 32)
            {
                if (position <= buffer.Length - 4)
                {
                    BinaryPrimitives.WriteUInt32LittleEndian(buffer.AsSpan(position), (uint)pending);
                    position += 4; pending >>= 32; pendingBits -= 32;
                    if (position == buffer.Length) Flush();
                }
                else
                    for (var i = 0; i < 4; i++) Byte();
            }
        }

        internal void Finish()
        {
            while (pendingBits >= 8) Byte();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Byte()
        {
            buffer[position++] = (byte)pending;
            pending >>= 8; pendingBits -= 8;
            if (position == buffer.Length) Flush();
        }

        private void Flush()
        {
            Commit();
            try { owner.Flush(); }
            finally { position = owner.encodedCount; }
        }

        internal void Commit()
        {
            owner.pending = pending; owner.pendingBits = pendingBits; owner.encodedCount = position;
        }
    }

    private static int FixedLength(int symbol) => symbol < 144 ? 8 : symbol < 256 ? 9 : symbol < 280 ? 7 : 8;
}
