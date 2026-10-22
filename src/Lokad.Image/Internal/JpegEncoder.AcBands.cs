namespace Lokad.Image.Internal;

internal static partial class JpegEncoder
{
    // T.81 Annex G first scans reset run/EOB at each spectral band. Bits name
    // zigzag positions. Default retains one AC band; 63 always ends a band.
    internal readonly record struct JointAcBands(ulong Luma, ulong Chroma)
    {
        internal ulong ForComponent(int component) => component == 0 ? Luma : Chroma;
    }

    internal static ulong AcBandEnds444(ReadOnlySpan<ushort> amplitudeLimits)
    {
        if (amplitudeLimits.Length != 64) throw new ArgumentException("Expected one complete amplitude grid.");
        var ends = 1UL << 63;
        // 1023 / factor uniquely identifies each admitted power-of-two factor.
        for (var scan = 1; scan < 63; scan++)
            if (amplitudeLimits[JpegTransform.Zigzag[scan]] != amplitudeLimits[JpegTransform.Zigzag[scan + 1]])
                ends |= 1UL << scan;
        return ends;
    }

    internal static JointAcBands SourcePriceBands444(ReadOnlySpan<ushort> luma,
        ReadOnlySpan<ushort> chroma, int lambda, OperationContext context)
    {
        if (luma.Length != 64 || chroma.Length != 64 || lambda < 0)
            throw new ArgumentException("Source prices require two complete quantizers and a nonnegative weight.");
        if (lambda == 0) return default;
        // Once per operation: <=128*13 factor steps + 128 grid setup
        // + 128 mask setup/visits. Refusal precedes even the histogram clear.
        context.Work(JointPriceBandSetupWork);
        Span<ushort> limits = stackalloc ushort[64];
        var precision = (int)Math.Sqrt(lambda); ulong y = 0, cbcr = 0;
        for (var c = 0; c < 2; c++)
        {
            var q = c == 0 ? luma : chroma;
            for (var i = 0; i < 64; i++) limits[i] = (ushort)(1023 / (1 << PointTransform444(q[i], precision)));
            var ends = AcBandEnds444(limits);
            if (c == 0) y = ends; else cbcr = ends;
        }
        return new(y, cbcr);
    }
}
