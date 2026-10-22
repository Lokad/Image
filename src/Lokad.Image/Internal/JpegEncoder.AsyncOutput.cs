namespace Lokad.Image.Internal;

internal static partial class JpegEncoder
{
    internal static EncodedOutput AsyncOutput(ImageBuffer image, JpegEncodeOptions options, OperationContext context)
    {
        if (options.QuantizationPolicy == JpegQuantizationPolicy.Reference)
        { ValidateReferenceOptions(options, JpegSampleFormat.Rgb); return EncodedOutput.Segmented(context); }
        var capacity = AsyncCapacity(image.Width, image.Height, options.Sampling, options, context);
        if (capacity > 0)
        {
            // Test the original adaptive admission with staging already charged,
            // before renting a large array. No existing encoder method changes.
            var adaptive = false;
            context.Reserve(capacity + (long)EncodedOutput.Overhead);
            try
            {
                using var probe = new MemoryStream();
                using var bounded = new LimitedWriteStream(probe, context.Limits.MaxOutputBytes, context.Cancellation);
                adaptive = CanUseAdaptive(image, bounded, options, context);
            }
            finally { context.Release(capacity + (long)EncodedOutput.Overhead); }
            if (adaptive) return EncodedOutput.Contiguous(context, capacity);
        }
        return EncodedOutput.Segmented(context);
    }

    internal static EncodedOutput AsyncSampleOutput(JpegSampleInput source, JpegEncodeOptions options, OperationContext context)
    {
        if (options.QuantizationPolicy == JpegQuantizationPolicy.Reference)
        { ValidateReferenceOptions(options, source.Format); return EncodedOutput.Segmented(context); }
        var sampling = source.Format == JpegSampleFormat.Grayscale ? JpegSampling.Grayscale : options.Sampling;
        var capacity = AsyncCapacity(source.Width, source.Height, sampling, options, context);
        if (capacity == 0 || context.Cancellation.CanBeCanceled) return EncodedOutput.Segmented(context);
        var h = sampling is JpegSampling.YCbCr420 or JpegSampling.YCbCr422 ? 2 : 1;
        var v = sampling == JpegSampling.YCbCr420 ? 2 : 1;
        var components = sampling == JpegSampling.Grayscale ? 1 : 3;
        var mcus = ((source.Width + h * 8L - 1) / (h * 8)) * ((source.Height + v * 8L - 1) / (v * 8));
        var blocks = mcus * (h * v + components - 1);
        var events = blocks * 64 + (options.RestartInterval == 0 ? 0 : mcus);
        var work = 2 * mcus + mcus * (h * v * (4096 + 768) + (components - 1) * (4096 + 768 * h * v))
            + blocks * (192L + AcDecisionWork(options.Quality, components, sampling, options.QuantizationPolicy))
            + JointPriceTrainingWork(mcus, options.Quality, components, sampling, options.QuantizationPolicy) + 262145
            + (sampling == JpegSampling.YCbCr444 ? Progressive444Work(blocks, options) : 0);
        // The source/fixed sample workspace is already admitted by the new API.
        var memory = events * sizeof(uint) + 65536 + capacity + EncodedOutput.Overhead;
        return blocks >= 256 && events <= Array.MaxLength && work <= context.RemainingWork && memory <= context.RemainingWorkingBytes
            ? EncodedOutput.Contiguous(context, capacity) : EncodedOutput.Segmented(context);
    }

    private static int AsyncCapacity(int width, int height, JpegSampling sampling, JpegEncodeOptions options, OperationContext context)
    {
        if (context.Cancellation.CanBeCanceled || !Enum.IsDefined(sampling)) return 0;
        var h = sampling is JpegSampling.YCbCr420 or JpegSampling.YCbCr422 ? 2 : 1;
        var v = sampling == JpegSampling.YCbCr420 ? 2 : 1;
        var components = sampling == JpegSampling.Grayscale ? 1 : 3;
        var blocks = ((width + h * 8L - 1) / (h * 8)) * ((height + v * 8L - 1) / (v * 8)) * (h * v + components - 1);
        if (blocks < 256) return 0;
        // The existing admitted JPEG bound includes worst-case entropy stuffing.
        var capacity = Math.Min(JpegPacketBound(blocks, options), context.Limits.MaxOutputBytes);
        return capacity > 0 && capacity <= Array.MaxLength && capacity + EncodedOutput.Overhead <= context.RemainingWorkingBytes
            ? (int)capacity : 0;
    }
}
