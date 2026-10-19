using System.Diagnostics.CodeAnalysis;
using Lokad.Image.Internal;

namespace Lokad.Image;

public sealed partial class TextLayout
{
    /// <summary>Find the largest fitting size on a descending finite size grid.</summary>
    /// <remarks>
    /// Sizes start at options.FontSize, decrease by fontSizeStep and include the
    /// exact minimum. Wrapping is capped at width. Bounds must fit after translating
    /// their origin to (0,0): draw at (-layout.Bounds.X, -layout.Bounds.Y) within the
    /// target area. No raster is allocated. Completed nonfitting trials release
    /// storage before the next trial; work is cumulative. Limit/admission refusal,
    /// cancellation, unsupported features and allocation failure terminate the call.
    /// </remarks>
    public static bool TryFit(TrueTypeFont font, string text, double width, double height,
        [NotNullWhen(true)] out TextLayout? layout, TextOptions? options = null,
        double minimumFontSize = 1, double fontSizeStep = .25, CancellationToken cancellationToken = default)
    {
        layout = null;
        ArgumentNullException.ThrowIfNull(font); ArgumentNullException.ThrowIfNull(text); options ??= new();
        font.EnsureAlive();
        if (!double.IsFinite(width) || width <= 0 || !double.IsFinite(height) || height <= 0)
            throw new ArgumentOutOfRangeException(nameof(width), "Fitting dimensions must be finite and positive.");
        if (!double.IsFinite(minimumFontSize) || minimumFontSize <= 0 || minimumFontSize > options.FontSize)
            throw new ArgumentOutOfRangeException(nameof(minimumFontSize));
        if (!double.IsFinite(fontSizeStep) || fontSizeStep <= 0)
            throw new ArgumentOutOfRangeException(nameof(fontSizeStep));
        if (!double.IsFinite(options.FontSize) || options.FontSize <= 0 || double.IsNaN(options.WrapWidth) || options.WrapWidth <= 0
            || !double.IsFinite(options.LineSpacing) || options.LineSpacing <= 0 || options.TabSpaces < 1
            || !Enum.IsDefined(options.Shaping) || !Enum.IsDefined(options.Direction) || !Enum.IsDefined(options.Alignment))
            throw new ArgumentOutOfRangeException(nameof(options));
        if (options.Shaping == TextShaping.Legacy && options.Direction == TextDirection.RightToLeft)
            throw new ArgumentException("Right-to-left direction requires OpenType shaping.", nameof(options));
        var context = new OperationContext(options.Limits, cancellationToken);
        Builder? builder = null;
        try
        {
            if (options.FontSize > context.Limits.MaxDimension)
                throw new ImageLimitException(ImageLimitKind.Dimension, "Font size exceeds the dimension limit.");
            var range = options.FontSize - minimumFontSize;
            var intervals = range == 0 ? 0 : fontSizeStep >= range ? 1 : Math.Ceiling(range / fontSizeStep);
            if (!double.IsFinite(intervals) || intervals >= context.Limits.MaxContainerItems)
                throw new ImageLimitException(ImageLimitKind.ContainerItems, "Fitting candidate limit exceeded.");
            var initial = context.RemainingWorkingBytes;
            context.ReserveBorrowed(font.StorageBytes); context.ReserveBorrowed(text.Length * 2L);
            var count = Count(text, context);
            // Descending enumeration does not assume that fit is monotonic:
            // wrapping/overhang changes can make a larger size fit after a
            // smaller size fails. One candidate is live at a time.
            for (var index = 0; index <= (int)intervals; index++)
            {
                context.Work(1); var before = context.RemainingWorkingBytes;
                context.Reserve(512);
                var candidate = options with { FontSize = index == (int)intervals ? minimumFontSize
                    : Math.Max(minimumFontSize, options.FontSize - index * fontSizeStep),
                    WrapWidth = Math.Min(width, options.WrapWidth) };
                builder = new(font, candidate, context, count);
                if (candidate.Shaping == TextShaping.OpenType) { builder.ReadShaped(text); builder.ArrangeShaped(); }
                else { builder.Read(text); builder.Arrange(); }
                builder.AlignLines(); builder.Measure(); builder.ReleasePreparation();
                if (builder.Bounds.Width <= width && builder.Bounds.Height <= height)
                {
                    context.ReleaseBorrowed(text.Length * 2L);
                    layout = new(font, candidate, builder, text.Length, initial - context.RemainingWorkingBytes) { storageOwner = context };
                    return true;
                }
                foreach (var shape in builder.Shapes.Values) shape.Outline.Dispose();
                builder.Shapes.Clear(); builder = null;
                context.Release(before - context.RemainingWorkingBytes);
                if (candidate.FontSize == minimumFontSize) break;
            }
            context.Dispose(); return false;
        }
        catch
        {
            builder?.ReleasePreparation();
            if (builder is not null) foreach (var shape in builder.Shapes.Values) shape.Outline.Dispose();
            context.Dispose(); throw;
        }
    }
}
