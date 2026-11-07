using System.Numerics;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace Lokad.Image.Internal;

// Encoder analysis policy, not a bitstream alphabet: unquantized coefficients
// contribute to32 magnitude buckets with an eight-unit step and saturation.
// Caller-owned scratch and borrowed coefficients introduce no storage lifetime;
// the enclosing macroblock analysis accounts cancellation and workspace.
internal static class Vp8SourceHistogram
{
    internal readonly record struct Shape(int PeakPopulation, int LastBin)
    {
        // Keep this score unclamped until the later luma/chroma mix.
        internal int RawScore => PeakPopulation > 1 ? 510 * LastBin / PeakPopulation : 0;
    }

    // Source analysis has finished every DCT read of this private workspace.
    // Recycle its64 coefficients for bins, retaining the per16 density policy.
    internal static void AddFourBlocks(Span<short> workspace, Span<int> population)
    {
        if (workspace.Length < 64 || population.Length < 32)
            throw new ArgumentException("Analysis requires64coefficients and32buckets.");
        if (!Avx512F.IsSupported || !Avx512BW.IsSupported
            || MemoryMarshal.AsBytes(workspace[..64]).Overlaps(MemoryMarshal.AsBytes(population[..32])))
        {
            // Aliased populations can modify a later coefficient block. Keep
            // the original call/read order before mutating any workspace.
            for (var block = 0; block < 4; block++) AddBlock(workspace.Slice(block * 16, 16), population);
            return;
        }
        var first = Vector512.Create(workspace);
        var second = Vector512.Create(workspace[32..]);
        var limit = Vector512.Create((ushort)31);
        var low = Avx512BW.Min(Avx512BW.ShiftRightLogical(Avx512BW.Abs(first).AsUInt16(), 3), limit);
        var high = Avx512BW.Min(Avx512BW.ShiftRightLogical(Avx512BW.Abs(second).AsUInt16(), 3), limit);
        var lowZeros = (uint)Vector512.ExtractMostSignificantBits(Vector512.Equals(low, Vector512<ushort>.Zero));
        var highZeros = (uint)Vector512.ExtractMostSignificantBits(Vector512.Equals(high, Vector512<ushort>.Zero));
        if ((lowZeros & highZeros) == uint.MaxValue) { population[0] += 64; return; }
        var buckets = MemoryMarshal.Cast<short, ushort>(workspace[..64]);
        low.CopyTo(buckets); high.CopyTo(buckets[32..]);
        var combinedZeros = 0;
        for (var block = 0; block < 4; block++)
        {
            var offset = block * 16;
            var mask = ((block < 2 ? lowZeros : highZeros) >> ((block & 1) * 16)) & 65535u;
            var zeros = BitOperations.PopCount(mask);
            if (zeros < 8)
            {
                for (var p = 0; p < 16; p++) population[buckets[offset + p]]++;
            }
            else
            {
                combinedZeros += zeros;
                for (var pending = ~mask & 65535u; pending != 0; pending &= pending - 1)
                    population[buckets[offset + BitOperations.TrailingZeroCount(pending)]]++;
            }
        }
        if (combinedZeros != 0) population[0] += combinedZeros;
    }

    internal static void AddBlock(ReadOnlySpan<short> coefficients, Span<int> population)
    {
        if (coefficients.Length < 16 || population.Length < 32) throw new ArgumentException("Analysis requires16coefficients and32buckets.");
        if (Avx2.IsSupported && !MemoryMarshal.AsBytes(coefficients[..16]).Overlaps(MemoryMarshal.AsBytes(population[..32])))
        {
            // Unsigned magnitude also represents Abs(short.MinValue) exactly.
            var magnitudes = Avx2.Abs(Vector256.Create(coefficients)).AsUInt16();
            var values = Avx2.Min(Avx2.ShiftRightLogical(magnitudes, 3), Vector256.Create((ushort)31));
            var zeroBytes = (uint)Avx2.MoveMask(Avx2.CompareEqual(values, Vector256<ushort>.Zero).AsByte());
            if (zeroBytes == uint.MaxValue) { population[0] += 16; return; }
            Span<ushort> buckets = stackalloc ushort[16]; values.CopyTo(buckets);
            var zeroCount = BitOperations.PopCount(zeroBytes) >> 1;
            // Keep straight updates when fewer than half the lanes are zero.
            if (zeroCount < 8)
            {
                for (var p = 0; p < 16; p++) population[buckets[p]]++;
                return;
            }
            // Coefficients cannot alias populations here. Combine repeated
            // zero-bucket writes, keeping modulo-int counts and borrowed tails.
            population[0] += zeroCount;
            for (var pending = ~zeroBytes; pending != 0;)
            {
                var at = BitOperations.TrailingZeroCount(pending);
                population[buckets[at >> 1]]++;
                pending &= ~(3u << at);
            }
            return;
        }
        for (var p = 0; p < 16; p++)
        {
            var bucket = Math.Min(31, Math.Abs((int)coefficients[p]) >> 3);
            population[bucket]++;
        }
    }

    internal static Shape Describe(ReadOnlySpan<int> population)
    {
        if (population.Length < 32) throw new ArgumentException("Analysis requires32buckets.");
        var peak = 0; var last = 1;
        for (var bucket = 0; bucket < 32; bucket++)
        {
            var frequency = population[bucket];
            if (frequency <= 0) continue;
            peak = Math.Max(peak, frequency); last = bucket;
        }
        return new(peak, last);
    }
}
