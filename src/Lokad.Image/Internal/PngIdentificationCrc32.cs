using System.Buffers.Binary;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace Lokad.Image.Internal;

/// <summary>Independent polynomial folds for PNG scans and chunk checksums.</summary>
internal static class PngIdentificationCrc32
{
    // PNG section 5.5: normal/reflected forms of the same CRC polynomial.
    private const ulong Polynomial = 0x104C11DB7;
    private static readonly Vector128<ulong> Fold128 = Vector128.Create((ulong)Power(128), (ulong)Power(192));
    private static readonly Vector128<ulong> Fold512 = Vector128.Create((ulong)Power(512), (ulong)Power(576));
    private static readonly Vector128<ulong> Append32 = Vector128.Create((ulong)Power(32), (ulong)Power(96));
    private static readonly Vector128<ulong> Reduce64 = Vector128.Create((ulong)Power(64), 0UL);
    private static readonly Vector128<ulong> Quotient = Vector128.Create(DividePower64(), 0UL);
    private static readonly Vector128<ulong> Divisor = Vector128.Create(Polynomial, 0UL);
    private static readonly Vector128<byte> Nibbles = NibbleReversals();
    private static readonly Vector128<byte> ByteOrder = ReverseByteOrder();
    private static readonly uint[] TailTable = BuildTailTable();

    private static uint Power(int exponent)
    {
        uint value = 1;
        for (var i = 0; i < exponent; i++)
            value = (value << 1) ^ ((value & 0x80000000) == 0 ? 0 : (uint)(Polynomial & uint.MaxValue));
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
    private static Vector128<byte> NibbleReversals()
    {
        Span<byte> values = stackalloc byte[16];
        for (var i = 0; i < 16; i++)
        {
            var reversed = 0;
            for (var bit = 0; bit < 4; bit++) reversed = (reversed << 1) | ((i >> bit) & 1);
            values[i] = (byte)reversed;
        }
        return MemoryMarshal.Read<Vector128<byte>>(values);
    }
    private static Vector128<byte> ReverseByteOrder()
    {
        Span<byte> values = stackalloc byte[16];
        for (var i = 0; i < 16; i++) values[i] = (byte)(15 - i);
        return MemoryMarshal.Read<Vector128<byte>>(values);
    }
    private static uint[] BuildTailTable()
    {
        var values = new uint[256];
        for (var i = 0; i < values.Length; i++)
        {
            var crc = (uint)i;
            for (var bit = 0; bit < 8; bit++) crc = (crc >> 1) ^ ((crc & 1) == 0 ? 0 : 0xedb88320u);
            values[i] = crc;
        }
        return values;
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Vector128<ulong> Read(ReadOnlySpan<byte> bytes)
    {
        var input = MemoryMarshal.Read<Vector128<byte>>(bytes);
        // GF2P8AFFINEQB uses matrix byte 7-i for output bit i. Giving byte r
        // basis bit r reverses all eight bits, independently in every lane.
        if (Gfni.IsSupported)
            return Ssse3.Shuffle(Gfni.GaloisFieldAffineTransform(input,
                Vector128.Create(0x8040201008040201UL).AsByte(), 0), ByteOrder).AsUInt64();
        var mask = Vector128.Create((byte)15);
        var low = Ssse3.Shuffle(Nibbles, Sse2.And(input, mask));
        var high = Ssse3.Shuffle(Nibbles, Sse2.And(Sse2.ShiftRightLogical(input.AsUInt16(), 4).AsByte(), mask));
        var reversed = Sse2.Or(Sse2.ShiftLeftLogical(low.AsUInt16(), 4).AsByte(), high);
        return Ssse3.Shuffle(reversed, ByteOrder).AsUInt64();
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Vector128<ulong> Fold(Vector128<ulong> value, Vector128<ulong> factors)
        => Sse2.Xor(Pclmulqdq.CarrylessMultiply(value, factors, 0), Pclmulqdq.CarrylessMultiply(value, factors, 0x11));

    private static uint Reduce(Vector128<ulong> value)
    {
        var intermediate = Sse2.Xor(Vector128.Create(value.GetElement(0), 0UL),
            Pclmulqdq.CarrylessMultiply(Vector128.Create(value.GetElement(1), 0UL), Reduce64, 0));
        var word = intermediate.GetElement(0) ^ Pclmulqdq.CarrylessMultiply(
            Vector128.Create(intermediate.GetElement(1), 0UL), Reduce64, 0).GetElement(0);
        var product = Pclmulqdq.CarrylessMultiply(Vector128.Create(word >> 31, 0UL), Quotient, 0);
        var quotient = product.GetElement(0) >> 33 | product.GetElement(1) << 31;
        var remainder = (word ^ Pclmulqdq.CarrylessMultiply(Vector128.Create(quotient, 0UL), Divisor, 0).GetElement(0)) & ((1UL << 33) - 1);
        if ((remainder & (1UL << 32)) != 0) remainder ^= Polynomial;
        var result = (uint)remainder;
        result = ((result & 0x55555555) << 1) | ((result >> 1) & 0x55555555);
        result = ((result & 0x33333333) << 2) | ((result >> 2) & 0x33333333);
        result = ((result & 0x0f0f0f0f) << 4) | ((result >> 4) & 0x0f0f0f0f);
        return BinaryPrimitives.ReverseEndianness(result);
    }

    // The caller admits the required SIMD and at least 64 payload bytes.
    // Continue the reflected type seed, then append the CRC's zero bits once.
    internal static uint ComputeChunk(ReadOnlySpan<byte> type, ReadOnlySpan<byte> bytes)
    {
        var seed = ~Crc32.Compute(type);
        seed = ((seed & 0x55555555) << 1) | ((seed >> 1) & 0x55555555);
        seed = ((seed & 0x33333333) << 2) | ((seed >> 2) & 0x33333333);
        seed = ((seed & 0x0f0f0f0f) << 4) | ((seed >> 4) & 0x0f0f0f0f);
        seed = BinaryPrimitives.ReverseEndianness(seed);
        Vector128<ulong> first, second, third, fourth;
        var i = 64;
        if (Pclmulqdq.V512.IsSupported && Gfni.V512.IsSupported && Avx512BW.IsSupported && Avx512F.IsSupported)
        {
            // Four independent 128-bit residues advance by the same x^512.
            // Wider instructions pack those lanes without changing polynomial order.
            var orderPair = Vector256.Create(ByteOrder, ByteOrder);
            var order = Vector512.Create(orderPair, orderPair);
            var value = Avx512F.Xor(ReadFour(bytes, order), Vector512.Create(0UL, (ulong)seed << 32, 0, 0, 0, 0, 0, 0));
            var pair = Vector256.Create(Fold512, Fold512);
            var factors = Vector512.Create(pair, pair);
            for (; i <= bytes.Length - 64; i += 64)
                value = Avx512F.Xor(Avx512F.Xor(
                    Pclmulqdq.V512.CarrylessMultiply(value, factors, 0),
                    Pclmulqdq.V512.CarrylessMultiply(value, factors, 0x11)), ReadFour(bytes.Slice(i, 64), order));
            var lower = value.GetLower(); var upper = value.GetUpper();
            first = lower.GetLower(); second = lower.GetUpper(); third = upper.GetLower(); fourth = upper.GetUpper();
        }
        else
        {
            first = Sse2.Xor(Read(bytes), Vector128.Create(0UL, (ulong)seed << 32));
            second = Read(bytes.Slice(16, 16));
            third = Read(bytes.Slice(32, 16));
            fourth = Read(bytes.Slice(48, 16));
            for (; i <= bytes.Length - 64; i += 64)
            {
                first = Sse2.Xor(Fold(first, Fold512), Read(bytes.Slice(i, 16)));
                second = Sse2.Xor(Fold(second, Fold512), Read(bytes.Slice(i + 16, 16)));
                third = Sse2.Xor(Fold(third, Fold512), Read(bytes.Slice(i + 32, 16)));
                fourth = Sse2.Xor(Fold(fourth, Fold512), Read(bytes.Slice(i + 48, 16)));
            }
        }
        first = Sse2.Xor(Fold(first, Fold128), second);
        first = Sse2.Xor(Fold(first, Fold128), third);
        first = Sse2.Xor(Fold(first, Fold128), fourth);
        for (; i <= bytes.Length - 16; i += 16)
            first = Sse2.Xor(Fold(first, Fold128), Read(bytes.Slice(i, 16)));
        var crc = Reduce(Fold(first, Append32));
        for (; i < bytes.Length; i++) crc = TailTable[(crc ^ bytes[i]) & 255] ^ (crc >> 8);
        return ~crc;
    }

    public static uint Compute(ReadOnlySpan<byte> bytes, CancellationToken cancellation = default)
    {
        if (!Pclmulqdq.IsSupported || !Ssse3.IsSupported || !Sse2.IsSupported || bytes.Length < 16)
            return Crc32.Compute(bytes, cancellation);
        cancellation.ThrowIfCancellationRequested();
        // Represent the data polynomial before appending the CRC's 32 zero
        // bits. Four residues advance independently by x^512, hiding the
        // carryless-multiply latency; combine once, then append x^32 once.
        var first = Sse2.Xor(Read(bytes), Vector128.Create(0UL, (ulong)uint.MaxValue << 32));
        var i = 16;
        if (bytes.Length >= 64)
        {
            var second = Read(bytes.Slice(16, 16));
            var third = Read(bytes.Slice(32, 16));
            var fourth = Read(bytes.Slice(48, 16));
            for (i = 64; i <= bytes.Length - 64; i += 64)
            {
                if ((i & 4095) == 0) cancellation.ThrowIfCancellationRequested();
                first = Sse2.Xor(Fold(first, Fold512), Read(bytes.Slice(i, 16)));
                second = Sse2.Xor(Fold(second, Fold512), Read(bytes.Slice(i + 16, 16)));
                third = Sse2.Xor(Fold(third, Fold512), Read(bytes.Slice(i + 32, 16)));
                fourth = Sse2.Xor(Fold(fourth, Fold512), Read(bytes.Slice(i + 48, 16)));
            }
            first = Sse2.Xor(Fold(first, Fold128), second);
            first = Sse2.Xor(Fold(first, Fold128), third);
            first = Sse2.Xor(Fold(first, Fold128), fourth);
        }
        for (; i <= bytes.Length - 16; i += 16)
        {
            if ((i & 4095) == 0) cancellation.ThrowIfCancellationRequested();
            first = Sse2.Xor(Fold(first, Fold128), Read(bytes.Slice(i, 16)));
        }
        var crc = Reduce(Fold(first, Append32));
        for (; i < bytes.Length; i++)
        {
            if ((i & 4095) == 0) cancellation.ThrowIfCancellationRequested();
            crc = TailTable[(crc ^ bytes[i]) & 255] ^ (crc >> 8);
        }
        return ~crc;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Vector512<ulong> ReadFour(ReadOnlySpan<byte> bytes, Vector512<byte> order)
    {
        var input = MemoryMarshal.Read<Vector512<byte>>(bytes);
        return Avx512BW.Shuffle(Gfni.V512.GaloisFieldAffineTransform(input,
            Vector512.Create(0x8040201008040201UL).AsByte(), 0), order).AsUInt64();
    }
}
