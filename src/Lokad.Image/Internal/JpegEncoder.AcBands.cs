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
}
