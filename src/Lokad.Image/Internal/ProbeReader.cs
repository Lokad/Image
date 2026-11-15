namespace Lokad.Image.Internal;

// Facts share the acquisition owner, including while a stream read is pending.
internal sealed class ProbeReader(Stream input, ImageLimits limits, CancellationToken cancellation, bool asynchronous,
    long maxAcquiredBytes, ImageProbeFacts requested)
    : InspectionReader(input, limits, cancellation, asynchronous, maxAcquiredBytes,
        dimensionsOnly: requested == ImageProbeFacts.Dimensions)
{
    private ImageProbeFacts established;
    private bool animated;
    internal bool WantsOrientation => (requested & ImageProbeFacts.Orientation) != 0;
    internal void Orientation() => established |= ImageProbeFacts.Orientation;
    internal void Animation(bool value) { animated = value; established |= ImageProbeFacts.Animation; }
    internal bool Reached(ImageProbeFacts facts)
    {
        // Orientation is established explicitly after parsing or proving absence;
        // a probe that skipped EXIF must not manufacture a Normal result at EOF.
        established |= facts & ~ImageProbeFacts.Orientation;
        return (established & requested) == requested;
    }

    internal ImageProbeResult Result(ImageInfo info)
    {
        var dimensions = (established & ImageProbeFacts.Dimensions) != 0;
        var source = (established & ImageProbeFacts.SourceRepresentation) != 0;
        var alpha = (established & ImageProbeFacts.Transparency) != 0;
        return new()
        {
            Format = info.Format, EstablishedFacts = established,
            Width = dimensions ? info.Width : null, Height = dimensions ? info.Height : null,
            BitDepth = source ? info.BitDepth : null, SourceColor = source ? info.SourceColor : null,
            SourceComponentCount = source ? info.SourceComponentCount : null,
            SourceAlpha = alpha ? info.SourceAlpha : null, HasAlpha = alpha ? info.HasAlpha : null,
            Orientation = (established & ImageProbeFacts.Orientation) != 0 ? info.Orientation : null,
            IsAnimated = (established & ImageProbeFacts.Animation) != 0 ? animated : null,
            BytesRead = BytesRead, BytesTraversed = BytesTraversed
        };
    }
}
