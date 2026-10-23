namespace Lokad.Image.Internal;

// Encoder analysis policy, not a bitstream alphabet: unquantized coefficients
// contribute to32 magnitude buckets with an eight-unit step and saturation.
// Caller-owned scratch and borrowed coefficients introduce no storage lifetime;
// the enclosing macroblock analysis accounts work/cancellation and workspace.
internal static class Vp8SourceHistogram
{
    internal readonly record struct Shape(int PeakPopulation, int LastBin)
    {
        // Keep this score unclamped until the later luma/chroma mix.
        internal int RawScore => PeakPopulation > 1 ? 510 * LastBin / PeakPopulation : 0;
    }

    internal static void AddBlock(ReadOnlySpan<short> coefficients, Span<int> population)
    {
        if (coefficients.Length < 16 || population.Length < 32) throw new ArgumentException("Analysis requires16coefficients and32buckets.");
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
