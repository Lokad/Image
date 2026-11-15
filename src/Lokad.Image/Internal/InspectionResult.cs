namespace Lokad.Image.Internal;

// Result construction is specialized at publication; acquisition and grammar stay
// shared. Reference-sized typed values avoid runtime generic result dispatch;
// factories retain no state or additional ownership.
internal readonly struct InfoResult(ImageInfo value) { internal ImageInfo Value { get; } = value; }
internal readonly struct ProbeResult(ImageProbeResult value) { internal ImageProbeResult Value { get; } = value; }

internal interface IInspectionResult<TResult> where TResult : struct
{
    static abstract TResult Create(ProbeReader? probe, ImageFormat format, int width, int height,
        int depth, bool hasAlpha, ImageOrientation orientation, SourceColorModel color,
        SourceAlphaRepresentation alpha, byte components);
}

internal readonly struct InfoResultFactory : IInspectionResult<InfoResult>
{
    public static InfoResult Create(ProbeReader? probe, ImageFormat format, int width, int height,
        int depth, bool hasAlpha, ImageOrientation orientation, SourceColorModel color,
        SourceAlphaRepresentation alpha, byte components)
        => new(new ImageInfo(format, width, height, depth, hasAlpha, orientation)
        {
            SourceColor = color,
            SourceAlpha = alpha,
            SourceComponentCount = components
        });
}

internal readonly struct ProbeResultFactory : IInspectionResult<ProbeResult>
{
    public static ProbeResult Create(ProbeReader? probe, ImageFormat format, int width, int height,
        int depth, bool hasAlpha, ImageOrientation orientation, SourceColorModel color,
        SourceAlphaRepresentation alpha, byte components)
        // Only ProbeCore chooses this factory, with its same non-null reader.
        => new(probe!.Result(format, width, height, depth, hasAlpha, orientation, color, alpha, components));
}
