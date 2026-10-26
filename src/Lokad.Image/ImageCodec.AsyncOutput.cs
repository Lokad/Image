using Lokad.Image.Internal;

namespace Lokad.Image;

public static partial class ImageCodec
{
    /// <summary>Encode into admitted staging, then write asynchronously. Leaves output open.</summary>
    public static async ValueTask EncodePngAsync(ImageBuffer image, Stream output, PngEncodeOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(image); ArgumentNullException.ThrowIfNull(output);
        options ??= new PngEncodeOptions();
        using var context = new OperationContext(options.Limits, cancellationToken);
        using var staged = EncodedOutput.Segmented(context);
        using var bounded = new LimitedWriteStream(staged.Target, context.Limits.MaxOutputBytes, cancellationToken);
        PngCodec.Encode(image, bounded, options, context);
        await staged.DeliverAsync(output).ConfigureAwait(false);
    }

    /// <summary>Encode into admitted staging, then write asynchronously. Leaves output open.</summary>
    public static async ValueTask EncodeJpegAsync(ImageBuffer image, Stream output, JpegEncodeOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(image); ArgumentNullException.ThrowIfNull(output);
        options ??= new JpegEncodeOptions();
        using var context = new OperationContext(options.Limits, cancellationToken);
        using var staged = JpegEncoder.AsyncOutput(image, options, context);
        using var bounded = new LimitedWriteStream(staged.Target, context.Limits.MaxOutputBytes, cancellationToken);
        JpegEncoder.Encode(image, bounded, options, context);
        await staged.DeliverAsync(output).ConfigureAwait(false);
    }

    /// <summary>Prepare admitted encoded data, then write asynchronously. Leaves output open.</summary>
    public static async ValueTask EncodeWebpAsync(ImageBuffer image, Stream output, WebpEncodeOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(image); ArgumentNullException.ThrowIfNull(output);
        options ??= new WebpEncodeOptions();
        using var context = new OperationContext(options.Limits, cancellationToken);
        if (!options.Lossless)
        {
            await Vp8Encoder.EncodeAsync(image, output, options, context).ConfigureAwait(false);
            return;
        }
        using var staged = EncodedOutput.Segmented(context);
        using var bounded = new LimitedWriteStream(staged.Target, context.Limits.MaxOutputBytes, cancellationToken);
        WebpLosslessEncoder.Encode(image, bounded, options, context);
        await staged.DeliverAsync(output).ConfigureAwait(false);
    }

    /// <summary>Encode borrowed gray/RGB samples without an RGBA frame, then write asynchronously. Leaves output open.</summary>
    /// <remarks>Keep samples stable and output alive until completion. Failures can leave partial output.</remarks>
    public static async ValueTask EncodeJpegAsync(ReadOnlyMemory<byte> samples, int width, int height, JpegSampleFormat format,
        Stream output, int stride = 0, JpegEncodeOptions? options = null, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(output);
        options ??= new JpegEncodeOptions();
        using var context = new OperationContext(options.Limits, cancellationToken);
        EncodedOutput? staged = null;
        try
        {
            // Keep borrowed spans inside the synchronous phase. Staged storage
            // and its admission stay alive until the actual writer completes.
            {
                if (options.Quality is < 1 or > 100 || !Enum.IsDefined(options.Sampling)
                    || options.RestartInterval is < 0 or > 65535 || options.BackgroundRgb > 0xFFFFFF)
                    throw new ArgumentOutOfRangeException(nameof(options));
                var source = new JpegSampleInput(samples.Span, width, height, format, stride, context);
                context.ReserveBorrowed(samples.Length); context.Reserve(8192);
                staged = JpegEncoder.AsyncSampleOutput(source, options, context);
                using var bounded = new LimitedWriteStream(staged.Target, context.Limits.MaxOutputBytes, cancellationToken);
                JpegEncoder.EncodeSamples(source, bounded, options, context);
            }
            await staged.DeliverAsync(output).ConfigureAwait(false);
        }
        finally { staged?.Dispose(); }
    }
}
