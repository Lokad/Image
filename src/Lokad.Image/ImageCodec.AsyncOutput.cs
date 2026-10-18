using Lokad.Image.Internal;

namespace Lokad.Image;

public static partial class ImageCodec
{
    /// <summary>Encode synchronously into admitted private staging, then write asynchronously. Leaves output open.</summary>
    public static ValueTask EncodePngAsync(ImageBuffer image, Stream output, PngEncodeOptions? options = null,
        CancellationToken cancellationToken = default) => EncodePngWithBudgetAsync(image, output, null, options, cancellationToken);

    /// <summary>Encode synchronously into admitted private staging, then write asynchronously. Leaves output open.</summary>
    public static ValueTask EncodeJpegAsync(ImageBuffer image, Stream output, JpegEncodeOptions? options = null,
        CancellationToken cancellationToken = default) => EncodeJpegWithBudgetAsync(image, output, null, options, cancellationToken);

    /// <summary>Encode synchronously into admitted private staging, then write asynchronously. Leaves output open.</summary>
    public static ValueTask EncodeWebpAsync(ImageBuffer image, Stream output, WebpEncodeOptions? options = null,
        CancellationToken cancellationToken = default) => EncodeWebpWithBudgetAsync(image, output, null, options, cancellationToken);

    /// <summary>Encode borrowed gray/RGB samples without an RGBA frame, then write asynchronously. Leaves output open.</summary>
    /// <remarks>Keep samples stable and output alive until completion. Failures can leave partial output.</remarks>
    public static ValueTask EncodeJpegAsync(ReadOnlyMemory<byte> samples, int width, int height, JpegSampleFormat format,
        Stream output, int stride = 0, JpegEncodeOptions? options = null, CancellationToken cancellationToken = default)
        => EncodeJpegSamplesWithBudgetAsync(samples, width, height, format, output, stride, null, options, cancellationToken);

    internal static async ValueTask EncodePngWithBudgetAsync(ImageBuffer image, Stream output, ImageBudget? budget,
        PngEncodeOptions? options, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(image); ArgumentNullException.ThrowIfNull(output);
        options ??= new PngEncodeOptions();
        var context = new OperationContext(options.Limits, cancellationToken);
        var admission = budget?.Begin(context.Limits);
        try
        {
            using var staged = EncodedOutput.Segmented(context);
            using var bounded = new LimitedWriteStream(staged.Target, context.Limits.MaxOutputBytes, cancellationToken);
            PngCodec.Encode(image, bounded, options, context);
            await staged.DeliverAsync(output).ConfigureAwait(false);
        }
        finally { admission?.Complete(context.ConsumedWork); }
    }

    internal static async ValueTask EncodeJpegWithBudgetAsync(ImageBuffer image, Stream output, ImageBudget? budget,
        JpegEncodeOptions? options, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(image); ArgumentNullException.ThrowIfNull(output);
        options ??= new JpegEncodeOptions();
        var context = new OperationContext(options.Limits, cancellationToken);
        var admission = budget?.Begin(context.Limits);
        try
        {
            using var staged = JpegEncoder.AsyncOutput(image, options, context);
            using var bounded = new LimitedWriteStream(staged.Target, context.Limits.MaxOutputBytes, cancellationToken);
            JpegEncoder.Encode(image, bounded, options, context);
            await staged.DeliverAsync(output).ConfigureAwait(false);
        }
        finally { admission?.Complete(context.ConsumedWork); }
    }

    internal static async ValueTask EncodeWebpWithBudgetAsync(ImageBuffer image, Stream output, ImageBudget? budget,
        WebpEncodeOptions? options, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(image); ArgumentNullException.ThrowIfNull(output);
        options ??= new WebpEncodeOptions();
        var context = new OperationContext(options.Limits, cancellationToken);
        var admission = budget?.Begin(context.Limits);
        try
        {
            using var staged = EncodedOutput.Segmented(context);
            using var bounded = new LimitedWriteStream(staged.Target, context.Limits.MaxOutputBytes, cancellationToken);
            if (options.Lossless) WebpLosslessEncoder.Encode(image, bounded, options, context);
            else Vp8Encoder.Encode(image, bounded, options, context);
            await staged.DeliverAsync(output).ConfigureAwait(false);
        }
        finally { admission?.Complete(context.ConsumedWork); }
    }

    internal static async ValueTask EncodeJpegSamplesWithBudgetAsync(ReadOnlyMemory<byte> samples, int width, int height,
        JpegSampleFormat format, Stream output, int stride, ImageBudget? budget, JpegEncodeOptions? options, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(output);
        options ??= new JpegEncodeOptions();
        var context = new OperationContext(options.Limits, cancellationToken);
        ImageBudget.OperationReservation? admission = null;
        EncodedOutput? staged = null;
        try
        {
            {
                if (options.Quality is < 1 or > 100 || !Enum.IsDefined(options.Sampling)
                    || options.RestartInterval is < 0 or > 65535 || options.BackgroundRgb > 0xFFFFFF)
                    throw new ArgumentOutOfRangeException(nameof(options));
                var source = new JpegSampleInput(samples.Span, width, height, format, stride, context);
                context.Reserve(samples.Length); context.Reserve(8192);
                admission = budget?.Begin(context.Limits);
                staged = JpegEncoder.AsyncSampleOutput(source, options, context);
                using var bounded = new LimitedWriteStream(staged.Target, context.Limits.MaxOutputBytes, cancellationToken);
                JpegEncoder.EncodeSamples(source, bounded, options, context);
            }
            await staged.DeliverAsync(output).ConfigureAwait(false);
        }
        finally
        {
            staged?.Dispose();
            admission?.Complete(context.ConsumedWork);
        }
    }
}
