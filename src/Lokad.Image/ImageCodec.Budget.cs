using Lokad.Image.Internal;

namespace Lokad.Image;

public static partial class ImageCodec
{
    internal static ImageInfo IdentifyWithBudget(ReadOnlySpan<byte> input, ImageBudget budget,
        ImageLimits? limits, CancellationToken cancellationToken)
    {
        var context = InputContext(input.Length, limits, cancellationToken);
        var admission = budget.Begin(context.Limits);
        try { return IdentifyCore(input, context); }
        finally { admission.Complete(context.ConsumedWork); }
    }

    internal static ImageInfo IdentifyWithBudget(Stream input, ImageBudget budget,
        ImageLimits? limits, CancellationToken cancellationToken)
    {
        var context = new OperationContext(limits ?? ImageLimits.Default, cancellationToken);
        ArgumentNullException.ThrowIfNull(input);
        var admission = budget.Begin(context.Limits);
        try
        {
            using var data = IdentificationInput.Read(input, context);
            return IdentifyCore(data.Span, context);
        }
        finally { admission.Complete(context.ConsumedWork); }
    }

    internal static BudgetedImage DecodeWithBudget(ReadOnlySpan<byte> input, ImageBudget budget,
        DecodeOptions? options, CancellationToken cancellationToken)
    {
        options ??= new DecodeOptions();
        var context = InputContext(input.Length, options.Limits, cancellationToken);
        var admission = budget.Begin(context.Limits);
        BudgetedImage? result = null;
        try { return result = BudgetedImage.Create(DecodeCore(input, options, context)); }
        finally { admission.Complete(context.ConsumedWork, result); }
    }

    internal static BudgetedImage DecodeWithBudget(Stream input, ImageBudget budget,
        DecodeOptions? options, CancellationToken cancellationToken)
    {
        options ??= new DecodeOptions();
        var context = new OperationContext(options.Limits, cancellationToken);
        ArgumentNullException.ThrowIfNull(input);
        var admission = budget.Begin(context.Limits);
        BudgetedImage? result = null;
        try { return result = BudgetedImage.Create(DecodeFromStream(input, options, context)); }
        finally { admission.Complete(context.ConsumedWork, result); }
    }

    internal static void EncodePngWithBudget(ImageBuffer image, Stream output, ImageBudget budget,
        PngEncodeOptions? options, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(image); ArgumentNullException.ThrowIfNull(output);
        options ??= new PngEncodeOptions();
        var context = new OperationContext(options.Limits, cancellationToken);
        var admission = budget.Begin(context.Limits);
        try
        {
            using var bounded = new LimitedWriteStream(output, options.Limits.MaxOutputBytes, cancellationToken);
            PngCodec.Encode(image, bounded, options, context);
        }
        finally { admission.Complete(context.ConsumedWork); }
    }

    internal static void EncodeJpegWithBudget(ImageBuffer image, Stream output, ImageBudget budget,
        JpegEncodeOptions? options, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(image); ArgumentNullException.ThrowIfNull(output);
        options ??= new JpegEncodeOptions();
        var context = new OperationContext(options.Limits, cancellationToken);
        var admission = budget.Begin(context.Limits);
        try
        {
            using var bounded = new LimitedWriteStream(output, options.Limits.MaxOutputBytes, cancellationToken);
            JpegEncoder.Encode(image, bounded, options, context);
        }
        finally { admission.Complete(context.ConsumedWork); }
    }

    internal static void EncodeWebpWithBudget(ImageBuffer image, Stream output, ImageBudget budget,
        WebpEncodeOptions? options, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(image); ArgumentNullException.ThrowIfNull(output);
        options ??= new WebpEncodeOptions();
        var context = new OperationContext(options.Limits, cancellationToken);
        var admission = budget.Begin(context.Limits);
        try
        {
            using var bounded = new LimitedWriteStream(output, options.Limits.MaxOutputBytes, cancellationToken);
            if (options.Lossless) WebpLosslessEncoder.Encode(image, bounded, options, context);
            else Vp8Encoder.Encode(image, bounded, options, context);
        }
        finally { admission.Complete(context.ConsumedWork); }
    }
}
