using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace Lokad.Image.Internal;

internal static class Adler32
{
    // RFC 1950 section 2.2: a += byte, b += a, both modulo 65521.
    // With normalized initial sums and n <= 4096, the largest b is
    // 65520 + n*65520 + 255*n*(n+1)/2 <= 2408052720 < uint.MaxValue.
    public static void Append(ReadOnlySpan<byte> bytes, ref uint stateA, ref uint stateB,
        CancellationToken cancellation = default)
    {
        cancellation.ThrowIfCancellationRequested();
        var a = stateA; var b = stateB;
        while (!bytes.IsEmpty)
        {
            cancellation.ThrowIfCancellationRequested();
            var block = bytes[..Math.Min(4096, bytes.Length)]; var i = 0;
            if (Avx2.IsSupported)
            {
                // These weights follow directly from the recurrence:
                // byte k adds (32-k)*byte to b and byte to a.
                var weights = Vector256.Create((sbyte)32, 31, 30, 29, 28, 27, 26, 25,
                    24, 23, 22, 21, 20, 19, 18, 17, 16, 15, 14, 13, 12, 11, 10, 9,
                    8, 7, 6, 5, 4, 3, 2, 1);
                for (; i <= block.Length - 32; i += 32)
                {
                    var value = Vector256.Create(block.Slice(i, 32));
                    // Adjacent pairs peak at 255*(32+31)=16065, so the
                    // signed 16-bit intermediate cannot saturate.
                    var pairs = Avx2.MultiplyAddAdjacent(value, weights);
                    var weighted = Avx2.MultiplyAddAdjacent(pairs, Vector256.Create((short)1));
                    b += 32 * a + (uint)Vector256.Sum(weighted);
                    a += (uint)Vector256.Sum(Avx2.SumAbsoluteDifferences(value, Vector256<byte>.Zero).AsUInt64());
                }
            }
            for (; i < block.Length; i++) { a += block[i]; b += a; }
            a %= 65521; b %= 65521; bytes = bytes[block.Length..];
        }
        stateA = a; stateB = b;
    }
}
