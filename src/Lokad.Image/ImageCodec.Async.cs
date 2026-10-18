using Lokad.Image.Internal;

namespace Lokad.Image;

public static partial class ImageCodec
{
    /// <summary>Read through EOF asynchronously, then perform complete container/EXIF validation. Leaves input open.</summary>
    /// <remarks>CPU parsing is synchronous. Encoded input uses bounded private storage;
    /// this is the full Identify contract, not a header-only inspection.</remarks>
    public static ValueTask<ImageInfo> IdentifyAsync(Stream input, ImageLimits? limits = null,
        CancellationToken cancellationToken = default)
    {
        var context = new OperationContext(limits ?? ImageLimits.Default, cancellationToken);
        ArgumentNullException.ThrowIfNull(input);
        return IdentifyFromStreamAsync(input, context);
    }

    /// <summary>Read through EOF asynchronously and decode into an independently owned image. Leaves input open.</summary>
    /// <remarks>CPU decoding is synchronous. Encoded input is not copied into a final
    /// exact-sized array. Actual private capacities count toward working admission.</remarks>
    public static ValueTask<ImageBuffer> DecodeAsync(Stream input, DecodeOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        options ??= new DecodeOptions();
        var context = new OperationContext(options.Limits, cancellationToken);
        ArgumentNullException.ThrowIfNull(input);
        return DecodeFromStreamAsync(input, options, context);
    }

    private static async ValueTask<ImageInfo> IdentifyFromStreamAsync(Stream input, OperationContext context)
    {
        using var data = await IdentificationInput.ReadAsync(input, context).ConfigureAwait(false);
        try { return IdentifyCore(data.Span, context); }
        finally { context.Release(data.Capacity); }
    }

    private static async ValueTask<ImageBuffer> DecodeFromStreamAsync(Stream input, DecodeOptions options, OperationContext context)
    {
        using var data = await IdentificationInput.ReadAsync(input, context).ConfigureAwait(false);
        try { return DecodeCore(data.Span, options, context); }
        finally { context.Release(data.Capacity); }
    }

    internal static async ValueTask<ImageInfo> IdentifyWithBudgetAsync(Stream input, ImageBudget budget,
        ImageLimits? limits, CancellationToken cancellationToken)
    {
        var context = new OperationContext(limits ?? ImageLimits.Default, cancellationToken);
        ArgumentNullException.ThrowIfNull(input);
        var admission = budget.Begin(context.Limits);
        try { return await IdentifyFromStreamAsync(input, context).ConfigureAwait(false); }
        finally { admission.Complete(context.ConsumedWork); }
    }

    internal static async ValueTask<BudgetedImage> DecodeWithBudgetAsync(Stream input, ImageBudget budget,
        DecodeOptions? options, CancellationToken cancellationToken)
    {
        options ??= new DecodeOptions();
        var context = new OperationContext(options.Limits, cancellationToken);
        ArgumentNullException.ThrowIfNull(input);
        var admission = budget.Begin(context.Limits);
        BudgetedImage? result = null;
        try { return result = BudgetedImage.Create(await DecodeFromStreamAsync(input, options, context).ConfigureAwait(false)); }
        finally { admission.Complete(context.ConsumedWork, result); }
    }
}
