using System.Buffers.Binary;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace Lokad.Image.Internal;

internal static class Crc32
{
    // PNG section 5.5 specifies the reflected polynomial 0xEDB88320.
    // Generate every entry from that polynomial. Slice k represents a byte
    // followed by k zero bytes; linearity combines eight contributions.
    private static readonly uint[] Table = BuildTable();
    // The normal polynomial is the bit reversal of the PNG polynomial above.
    // Folding constants are polynomial remainders, generated independently.
    private const ulong Polynomial = 0x104C11DB7UL;
    private static readonly Vector128<ulong> FoldFactors = Vector128.Create((ulong)Power(128), (ulong)Power(192));
    private static readonly Vector128<ulong> ReduceFactor = Vector128.Create((ulong)Power(64), 0UL);
    private static readonly Vector128<ulong> Quotient = Vector128.Create(DividePower64(), 0UL);
    private static readonly Vector128<ulong> Divisor = Vector128.Create(Polynomial, 0UL);

    private static uint Power(int exponent)
    {
        uint value = 1;
        for (var bit = 0; bit < exponent; bit++)
            value = value << 1 ^ ((value & 0x80000000) == 0 ? 0 : (uint)(Polynomial & uint.MaxValue));
        return value;
    }

    private static ulong DividePower64()
    {
        var remainder = (UInt128)1 << 64; ulong quotient = 0;
        for (var bit = 32; bit >= 0; bit--)
        {
            if ((remainder & ((UInt128)1 << (bit + 32))) == 0) continue;
            remainder ^= (UInt128)Polynomial << bit; quotient ^= 1UL << bit;
        }
        return quotient;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Vector128<ulong> ReadPolynomial(ReadOnlySpan<byte> bytes)
    {
        var value = Vector128.Create(BinaryPrimitives.ReadUInt64BigEndian(bytes.Slice(8, 8)),
            BinaryPrimitives.ReadUInt64BigEndian(bytes.Slice(0, 8))).AsUInt16();
        // Reverse bits within each byte to put PNG's least-significant-bit-first
        // input in normal polynomial order. Masks prevent bits crossing bytes.
        var ones = Vector128.Create((ushort)0x5555);
        value = Sse2.Or(Sse2.ShiftLeftLogical(Sse2.And(value, ones), 1), Sse2.And(Sse2.ShiftRightLogical(value, 1), ones));
        var pairs = Vector128.Create((ushort)0x3333);
        value = Sse2.Or(Sse2.ShiftLeftLogical(Sse2.And(value, pairs), 2), Sse2.And(Sse2.ShiftRightLogical(value, 2), pairs));
        var nibbles = Vector128.Create((ushort)0x0F0F);
        value = Sse2.Or(Sse2.ShiftLeftLogical(Sse2.And(value, nibbles), 4), Sse2.And(Sse2.ShiftRightLogical(value, 4), nibbles));
        return value.AsUInt64();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static uint Reduce(Vector128<ulong> folded)
    {
        // First reduce the 128-bit polynomial to at most 64 bits using x^64 mod P.
        var intermediate = Sse2.Xor(Vector128.Create(folded.GetElement(0), 0UL),
            Pclmulqdq.CarrylessMultiply(Vector128.Create(folded.GetElement(1), 0UL), ReduceFactor, 0));
        var word = intermediate.GetElement(0) ^ Pclmulqdq.CarrylessMultiply(
            Vector128.Create(intermediate.GetElement(1), 0UL), ReduceFactor, 0).GetElement(0);
        // Polynomial Barrett division: q1 = word / x^31, q3 = q1 * floor(x^64/P) / x^33.
        var product = Pclmulqdq.CarrylessMultiply(Vector128.Create(word >> 31, 0UL), Quotient, 0);
        var quotient = product.GetElement(0) >> 33 | product.GetElement(1) << 31;
        const ulong mask = (1UL << 33) - 1;
        var remainder = (word ^ Pclmulqdq.CarrylessMultiply(Vector128.Create(quotient, 0UL), Divisor, 0).GetElement(0)) & mask;
        if ((remainder & (1UL << 32)) != 0) remainder ^= Polynomial;
        return ReverseBits((uint)remainder);
    }

    private static uint ReverseBits(uint value)
    {
        value = (value & 0x55555555) << 1 | (value >> 1 & 0x55555555);
        value = (value & 0x33333333) << 2 | (value >> 2 & 0x33333333);
        value = (value & 0x0F0F0F0F) << 4 | (value >> 4 & 0x0F0F0F0F);
        return BinaryPrimitives.ReverseEndianness(value);
    }

    private static uint[] BuildTable()
    {
        var table = new uint[8 * 256];
        for (var i = 0; i < 256; i++)
        {
            var value = (uint)i;
            for (var bit = 0; bit < 8; bit++) value = value >> 1 ^ ((value & 1) == 0 ? 0 : 0xEDB88320u);
            table[i] = value;
        }
        for (var i = 0; i < 256; i++)
        {
            var value = table[i];
            for (var slice = 1; slice < 8; slice++)
            {
                value = table[value & 255] ^ (value >> 8); table[slice * 256 + i] = value;
            }
        }
        return table;
    }
    public static uint Compute(ReadOnlySpan<byte> bytes, CancellationToken cancellation = default)
    {
        var crc = uint.MaxValue; var i = 0;
        if (Pclmulqdq.IsSupported && Sse2.IsSupported && bytes.Length >= 16)
        {
            cancellation.ThrowIfCancellationRequested();
            var input = ReadPolynomial(bytes.Slice(0, 16));
            var folded = Sse2.Xor(Pclmulqdq.CarrylessMultiply(
                Vector128.Create(input.GetElement(1) >> 32 ^ uint.MaxValue, 0UL), FoldFactors, 0),
                Sse2.ShiftLeftLogical128BitLane(input.AsByte(), 4).AsUInt64());
            for (i = 16; i <= bytes.Length - 16; i += 16)
            {
                if ((i & 4095) == 0) cancellation.ThrowIfCancellationRequested();
                input = ReadPolynomial(bytes.Slice(i, 16));
                var adjusted = Sse2.Xor(folded, Vector128.Create(input.GetElement(1) >> 32, 0UL));
                folded = Sse2.Xor(Sse2.Xor(Pclmulqdq.CarrylessMultiply(adjusted, FoldFactors, 0),
                    Pclmulqdq.CarrylessMultiply(adjusted, FoldFactors, 0x11)),
                    Sse2.ShiftLeftLogical128BitLane(input.AsByte(), 4).AsUInt64());
            }
            crc = Reduce(folded);
        }
        for (; i <= bytes.Length - 8; i += 8)
        {
            if ((i & 4095) == 0) cancellation.ThrowIfCancellationRequested();
            var word = BinaryPrimitives.ReadUInt64LittleEndian(bytes.Slice(i, 8)) ^ crc;
            crc = Table[7 * 256 + (int)(word & 255)] ^ Table[6 * 256 + (int)(word >> 8 & 255)]
                ^ Table[5 * 256 + (int)(word >> 16 & 255)] ^ Table[4 * 256 + (int)(word >> 24 & 255)]
                ^ Table[3 * 256 + (int)(word >> 32 & 255)] ^ Table[2 * 256 + (int)(word >> 40 & 255)]
                ^ Table[256 + (int)(word >> 48 & 255)] ^ Table[(int)(word >> 56)];
        }
        for (; i < bytes.Length; i++)
        {
            if ((i & 4095) == 0) cancellation.ThrowIfCancellationRequested();
            crc = Table[(crc ^ bytes[i]) & 255] ^ (crc >> 8);
        }
        return ~crc;
    }
    public static uint ComputeChunk(ReadOnlySpan<byte> type, ReadOnlySpan<byte> bytes)
    {
        if (Pclmulqdq.IsSupported && Ssse3.IsSupported && Sse2.IsSupported && bytes.Length >= 64)
            return PngIdentificationCrc32.ComputeChunk(type, bytes);
        var crc = uint.MaxValue;
        // Continue one reflected PNG polynomial state across type and payload.
        // This avoids copying them into a temporary contiguous chunk buffer.
        foreach (var value in type) crc = Table[(crc ^ value) & 255] ^ (crc >> 8);
        var i = 0;
        if (Pclmulqdq.IsSupported && Sse2.IsSupported && bytes.Length >= 16)
        {
            var input = ReadPolynomial(bytes.Slice(0, 16));
            var folded = Sse2.Xor(Pclmulqdq.CarrylessMultiply(
                Vector128.Create(input.GetElement(1) >> 32 ^ ReverseBits(crc), 0UL), FoldFactors, 0),
                Sse2.ShiftLeftLogical128BitLane(input.AsByte(), 4).AsUInt64());
            for (i = 16; i <= bytes.Length - 16; i += 16)
            {
                input = ReadPolynomial(bytes.Slice(i, 16));
                var adjusted = Sse2.Xor(folded, Vector128.Create(input.GetElement(1) >> 32, 0UL));
                folded = Sse2.Xor(Sse2.Xor(Pclmulqdq.CarrylessMultiply(adjusted, FoldFactors, 0),
                    Pclmulqdq.CarrylessMultiply(adjusted, FoldFactors, 0x11)),
                    Sse2.ShiftLeftLogical128BitLane(input.AsByte(), 4).AsUInt64());
            }
            crc = Reduce(folded);
        }
        for (; i <= bytes.Length - 8; i += 8)
        {
            var word = BinaryPrimitives.ReadUInt64LittleEndian(bytes.Slice(i, 8)) ^ crc;
            crc = Table[7 * 256 + (int)(word & 255)] ^ Table[6 * 256 + (int)(word >> 8 & 255)]
                ^ Table[5 * 256 + (int)(word >> 16 & 255)] ^ Table[4 * 256 + (int)(word >> 24 & 255)]
                ^ Table[3 * 256 + (int)(word >> 32 & 255)] ^ Table[2 * 256 + (int)(word >> 40 & 255)]
                ^ Table[256 + (int)(word >> 48 & 255)] ^ Table[(int)(word >> 56)];
        }
        for (; i < bytes.Length; i++)
        {
            crc = Table[(crc ^ bytes[i]) & 255] ^ (crc >> 8);
        }
        return ~crc;
    }
}
