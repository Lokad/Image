using Lokad.Image.Internal;

namespace Lokad.Image;

public static partial class ImageCodec
{
    /// <summary>Read selected facts and stop as soon as they are established.</summary>
    /// <remarks>Starts at the current position and leaves the stream open, immediately after
    /// the acquired prefix, including at most 8192 bytes of read-ahead when scanning JPEG
    /// entropy for late metadata. Dimension probing does not read ahead into pixel payloads.
    /// No pixel, CRC, full-container or trailing-data
    /// validation is implied. MaxInputBytes bounds declared/traversed encoded extents;
    /// MaxAcquiredBytes separately bounds actual reads. An unvisited tail has unknown length.
    /// Workspace is fixed and charged to Limits.MemoryBudget; no raster is admitted.
    /// Requested late/absent metadata may require traversing pixel payloads.</remarks>
    public static ImageProbeResult Probe(Stream input, ImageProbeOptions? options = null,
        CancellationToken cancellationToken = default)
        => ProbeCore(input, options, cancellationToken, false).GetAwaiter().GetResult();

    /// <summary>Asynchronously read selected facts with the same prefix guarantees as Probe.</summary>
    public static ValueTask<ImageProbeResult> ProbeAsync(Stream input, ImageProbeOptions? options = null,
        CancellationToken cancellationToken = default)
        => ProbeCore(input, options, cancellationToken, true);

    private static async ValueTask<ImageProbeResult> ProbeCore(Stream input, ImageProbeOptions? options,
        CancellationToken cancellationToken, bool asynchronous)
    {
        ArgumentNullException.ThrowIfNull(input);
        var facts = options?.Facts ?? ImageProbeFacts.Dimensions;
        var maximum = options?.MaxAcquiredBytes ?? ImageProbeOptions.DefaultMaxAcquiredBytes;
        var metadataPolicy = options?.MetadataPolicy ?? ImageMetadataPolicy.Strict;
        if (facts == 0 || (facts & ~ImageProbeFacts.All) != 0 || maximum < 1)
            throw new ArgumentOutOfRangeException(nameof(options));
        ExifOrientation.ValidatePolicy(metadataPolicy);
        using var reader = new ProbeReader(input, options is null ? ImageLimits.Default : options.Limits,
            cancellationToken, asynchronous, maximum, facts);
        var info = await StreamInspection.InspectAsync(reader, reader, metadataPolicy).ConfigureAwait(false);
        return reader.Result(info);
    }
}
