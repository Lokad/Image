using System.Numerics;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics.X86;
using Lokad.Image.Internal;

namespace Lokad.Image;

/// <summary>Straight RGBA color with unsigned 16-bit channels; eight-bit canvases round each channel to 0..255.</summary>
public readonly record struct RgbaColor(ushort R, ushort G, ushort B, ushort A = ushort.MaxValue)
{
    /// <summary>Expand eight-bit samples exactly, multiplying by 257.</summary>
    public static RgbaColor From8(byte red, byte green, byte blue, byte alpha = 255)
        => new((ushort)(red * 257), (ushort)(green * 257), (ushort)(blue * 257), (ushort)(alpha * 257));
}

/// <summary>Mutate a caller-owned canvas in coded coordinates; destination rectangles are clipped.</summary>
/// <remarks>No implicit orientation or color management. Callers synchronize pixel access.
/// Limits/cancellation can leave completed chunks; validation and memory admission precede mutation.
/// Existing buffers stay owned by callers and no full-frame temporary is allocated.</remarks>
public static partial class ImageDrawing
{
    /// <summary>Replace a rectangle with a straight RGBA color, including its alpha and hidden RGB.</summary>
    public static void FillRectangle(ImageBuffer canvas, int x, int y, int width, int height, RgbaColor color,
        ImageLimits? limits = null, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(canvas); Extent(width, height);
        limits = ValidateInputs(canvas, canvas, limits, cancellationToken);
        var region = Clip(canvas, 0, 0, x, y, width, height);
        if (region.Width == 0 || region.Height == 0) return;
        ValidateRegion(region, limits);
        var r = canvas.BitDepth == 8 ? (color.R + 128) / 257 : color.R;
        var g = canvas.BitDepth == 8 ? (color.G + 128) / 257 : color.G;
        var b = canvas.BitDepth == 8 ? (color.B + 128) / 257 : color.B;
        var a = canvas.BitDepth == 8 ? (color.A + 128) / 257 : color.A;
        // Read the native channel order once, then let Span.Fill replicate
        // complete pixels with the runtime's vectorized or portable kernel.
        uint pixel8 = 0; ulong pixel16 = 0;
        if (canvas.BitDepth == 8)
            pixel8 = MemoryMarshal.Read<uint>(stackalloc byte[] { (byte)r, (byte)g, (byte)b, (byte)a });
        else
        {
            Span<ushort> channels = stackalloc ushort[] { (ushort)r, (ushort)g, (ushort)b, (ushort)a };
            pixel16 = MemoryMarshal.Read<ulong>(MemoryMarshal.AsBytes(channels));
        }
        for (var row = 0; row < region.Height; row++)
        for (var first = 0; first < region.Width;)
        {
            var count = Math.Min(4096, region.Width - first); cancellationToken.ThrowIfCancellationRequested();
            var yRow = region.DestinationY + row;
            if (canvas.BitDepth == 8)
            {
                var samples = canvas.GetRow8(yRow).Slice((region.DestinationX + first) * 4, count * 4);
                MemoryMarshal.Cast<byte, uint>(samples).Fill(pixel8);
            }
            else
            {
                var samples = canvas.GetRow16(yRow).Slice((region.DestinationX + first) * 4, count * 4);
                MemoryMarshal.Cast<ushort, ulong>(samples).Fill(pixel16);
            }
            first += count;
        }
    }

    /// <summary>Copy a source rectangle's exact samples to the canvas. Depths must match; self-overlap has snapshot semantics.</summary>
    public static void CopyRegion(ImageBuffer source, int sourceX, int sourceY, int width, int height,
        ImageBuffer canvas, int x, int y, ImageLimits? limits = null, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(source); ArgumentNullException.ThrowIfNull(canvas); Extent(width, height);
        if (sourceX < 0 || sourceY < 0 || (long)sourceX + width > source.Width || (long)sourceY + height > source.Height)
            throw new ArgumentOutOfRangeException(nameof(sourceX), "Source rectangle must fit the source.");
        if (source.BitDepth != canvas.BitDepth) throw new ArgumentException("Exact copies require matching pixel depths.", nameof(canvas));
        limits = ValidateInputs(canvas, source, limits, cancellationToken);
        var region = Clip(canvas, sourceX, sourceY, x, y, width, height);
        if (region.Width == 0 || region.Height == 0) return;
        ValidateRegion(region, limits);
        var sameBuffer = ReferenceEquals(source, canvas);
        // The measured byte fast path requires disjoint row slices. Word copies
        // retain the existing runtime kernel; traversal still handles all aliases.
        var independentRows = canvas.BitDepth == 8 && (!sameBuffer || region.SourceY != region.DestinationY);
        var backwardsY = sameBuffer && region.DestinationY > region.SourceY;
        var backwardsX = sameBuffer && region.DestinationX > region.SourceX;
        // Admission and geometry are already validated. Borrow each packed
        // span once; copying bytes preserves both sample depths exactly.
        var sourcePixels = source.Pixels; var canvasPixels = canvas.Pixels;
        var pixelBytes = canvas.BitDepth / 8 * 4;
        var sourceStride = source.Stride; var canvasStride = canvas.Stride;
        for (var row = 0; row < region.Height; row++)
        {
            var offsetY = backwardsY ? region.Height - row - 1 : row;
            for (var first = 0; first < region.Width;)
            {
                var count = Math.Min(4096, region.Width - first); cancellationToken.ThrowIfCancellationRequested();
                var offsetX = backwardsX ? region.Width - first - count : first;
                var sx = (region.SourceX + offsetX) * pixelBytes; var dx = (region.DestinationX + offsetX) * pixelBytes;
                var sy = region.SourceY + offsetY; var dy = region.DestinationY + offsetY;
                var input = sourcePixels.Slice(sy * sourceStride + sx, count * pixelBytes);
                var output = canvasPixels.Slice(dy * canvasStride + dx, count * pixelBytes);
                if (independentRows) IndependentRowCopy.Copy(input, output);
                else input.CopyTo(output);
                first += count;
            }
        }
    }

    /// <summary>Composite an image onto the canvas with straight-alpha source-over, supporting either source/canvas depth.</summary>
    /// <remarks>Unrounded coverage determines color; round positive halves upward only at final samples.
    /// Zero source alpha leaves every destination sample unchanged. Self-overlap has snapshot semantics.</remarks>
    public static void Composite(ImageBuffer canvas, ImageBuffer source, int x, int y,
        ImageLimits? limits = null, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(source); ArgumentNullException.ThrowIfNull(canvas);
        limits = ValidateInputs(canvas, source, limits, cancellationToken);
        var region = Clip(canvas, 0, 0, x, y, source.Width, source.Height);
        if (region.Width == 0 || region.Height == 0) return;
        ValidateRegion(region, limits);
        var backwardsY = ReferenceEquals(source, canvas) && region.DestinationY > region.SourceY;
        var backwardsX = ReferenceEquals(source, canvas) && region.DestinationX > region.SourceX;
        if (source.BitDepth == 8)
        {
            if (canvas.BitDepth == 8)
            {
                if (Avx512F.IsSupported) CompositeRows8(canvas, source, region, backwardsX, backwardsY, cancellationToken);
                else CompositeRows<byte, byte>(canvas, source, region, backwardsX, backwardsY, cancellationToken);
            }
            else CompositeRows<byte, ushort>(canvas, source, region, backwardsX, backwardsY, cancellationToken);
        }
        else if (canvas.BitDepth == 8) CompositeRows<ushort, byte>(canvas, source, region, backwardsX, backwardsY, cancellationToken);
        else CompositeRows<ushort, ushort>(canvas, source, region, backwardsX, backwardsY, cancellationToken);
    }

    private static void CompositeRows<TSource, TCanvas>(ImageBuffer canvas, ImageBuffer source, Region region,
        bool backwardsX, bool backwardsY, CancellationToken cancellationToken)
        where TSource : unmanaged, IBinaryInteger<TSource>, IMinMaxValue<TSource>
        where TCanvas : unmanaged, IBinaryInteger<TCanvas>, IMinMaxValue<TCanvas>
    {
        var sourcePixels = MemoryMarshal.Cast<byte, TSource>(source.Pixels);
        var canvasPixels = MemoryMarshal.Cast<byte, TCanvas>(canvas.Pixels);
        var sourceStride = source.Width * 4; var canvasStride = canvas.Width * 4;
        var sMax = ulong.CreateTruncating(TSource.MaxValue); var dMax = ulong.CreateTruncating(TCanvas.MaxValue);
        for (var row = 0; row < region.Height; row++)
        {
            var offsetY = backwardsY ? region.Height - row - 1 : row;
            var sourceRow = sourcePixels.Slice((region.SourceY + offsetY) * sourceStride + region.SourceX * 4, region.Width * 4);
            var canvasRow = canvasPixels.Slice((region.DestinationY + offsetY) * canvasStride + region.DestinationX * 4, region.Width * 4);
            for (var first = 0; first < region.Width;)
            {
                var count = Math.Min(256, region.Width - first); cancellationToken.ThrowIfCancellationRequested();
                for (var i = first; i < first + count; i++)
                {
                    var at = (backwardsX ? region.Width - i - 1 : i) * 4;
                    var sa = ulong.CreateTruncating(sourceRow[at + 3]); if (sa == 0) continue;
                    var da = ulong.CreateTruncating(canvasRow[at + 3]);
                    var sourceWeight = sa * dMax; var canvasWeight = da * (sMax - sa);
                    var coverage = sourceWeight + canvasWeight;
                    var denominator = sMax * coverage;
                    if (typeof(TSource) == typeof(TCanvas))
                        denominator = coverage;
                    else { sourceWeight *= dMax; canvasWeight *= sMax; }
                    for (var channel = 0; channel < 3; channel++)
                    {
                        // W3C Compositing 1, 9.1.4: exact rational source-over.
                        // Equal depths cancel a common maximum in numerator and
                        // denominator; positive half-up rounding is unchanged.
                        // Mixed depths retain the original full rational weights.
                        // Numerator + rounding < 65535^4 + 65535^3/2 < ulong.MaxValue.
                        var numerator = ulong.CreateTruncating(sourceRow[at + channel]) * sourceWeight
                            + ulong.CreateTruncating(canvasRow[at + channel]) * canvasWeight;
                        canvasRow[at + channel] = TCanvas.CreateTruncating((numerator + denominator / 2) / denominator);
                    }
                    canvasRow[at + 3] = TCanvas.CreateTruncating((coverage + sMax / 2) / sMax);
                }
                first += count;
            }
        }
    }

    private static ImageLimits ValidateInputs(ImageBuffer canvas, ImageBuffer source, ImageLimits? limits, CancellationToken cancellationToken)
    {
        limits ??= ImageLimits.Default;
        limits.Validate();
        cancellationToken.ThrowIfCancellationRequested();
        // These operations borrow all pixel storage and allocate no owned
        // workspace. Count borrowed capacity locally, without creating an owner
        // or charging storage that already belongs to the caller.
        var canvasBytes = canvas.ByteLength;
        if (canvasBytes > limits.MaxWorkingBytes
            || !ReferenceEquals(canvas, source) && source.ByteLength > limits.MaxWorkingBytes - canvasBytes)
            throw new ImageLimitException(ImageLimitKind.WorkingBytes, "Working memory limit exceeded.");
        return limits;
    }

    private static void ValidateRegion(Region region, ImageLimits limits)
    {
        // A nonempty clipped region fits an existing valid ImageBuffer, so its
        // depth, positive dimensions and array-sized capacity are established.
        // Only the caller's stricter region dimension/pixel limits remain.
        var dimensionsExceeded = region.Width > limits.MaxDimension || region.Height > limits.MaxDimension;
        if (dimensionsExceeded || (long)region.Width * region.Height > limits.MaxPixels)
            throw new ImageLimitException(dimensionsExceeded ? ImageLimitKind.Dimension : ImageLimitKind.PixelCount,
                "Image dimensions or pixel count exceed the configured limits.");
    }

    private static void Extent(int width, int height)
    {
        if (width < 0 || height < 0) throw new ArgumentOutOfRangeException(nameof(width), "Rectangle extents cannot be negative.");
    }

    private readonly record struct Region(int SourceX, int SourceY, int DestinationX, int DestinationY, int Width, int Height);
    private static Region Clip(ImageBuffer canvas, int sx, int sy, int dx, int dy, int width, int height)
    {
        var left = Math.Max(0L, -(long)dx); var top = Math.Max(0L, -(long)dy);
        var right = Math.Min((long)width, (long)canvas.Width - dx); var bottom = Math.Min((long)height, (long)canvas.Height - dy);
        if (left >= right || top >= bottom) return default;
        return new((int)(sx + left), (int)(sy + top), (int)(dx + left), (int)(dy + top), (int)(right - left), (int)(bottom - top));
    }
}
