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
        var pixels = canvas.Pixels;
        // Read the native channel order once, then let Span.Fill replicate
        // complete pixels with the runtime's vectorized or portable kernel.
        if (canvas.BitDepth == 8)
        {
            Span<byte> channels = stackalloc byte[]
            {
                (byte)((color.R + 128) / 257), (byte)((color.G + 128) / 257),
                (byte)((color.B + 128) / 257), (byte)((color.A + 128) / 257)
            };
            FillPixels(MemoryMarshal.Cast<byte, uint>(pixels), canvas.Width, region,
                MemoryMarshal.Read<uint>(channels), cancellationToken);
        }
        else
        {
            Span<ushort> channels = stackalloc ushort[] { color.R, color.G, color.B, color.A };
            FillPixels(MemoryMarshal.Cast<byte, ulong>(pixels), canvas.Width, region,
                MemoryMarshal.Read<ulong>(MemoryMarshal.AsBytes(channels)), cancellationToken);
        }
    }

    private static void FillPixels<T>(Span<T> pixels, int stride, Region region, T color,
        CancellationToken cancellationToken) where T : unmanaged
    {
        var offset = region.DestinationY * stride + region.DestinationX;
        for (var row = 0; row < region.Height; row++, offset += stride)
        for (var first = 0; first < region.Width;)
        {
            var count = Math.Min(4096, region.Width - first);
            cancellationToken.ThrowIfCancellationRequested();
            pixels.Slice(offset + first, count).Fill(color);
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
        // Different packed rows are disjoint at either depth; traversal below
        // handles cross-row aliases. Keep same-row memmove and separate-word copies.
        var independentRows = sameBuffer ? region.SourceY != region.DestinationY : canvas.BitDepth == 8;
        var backwardsY = sameBuffer && region.DestinationY > region.SourceY;
        var backwardsX = sameBuffer && region.DestinationX > region.SourceX;
        // Admission and geometry are already validated. Borrow each packed
        // span once; copying bytes preserves both sample depths exactly.
        var sourcePixels = source.Pixels; var canvasPixels = canvas.Pixels;
        var pixelBytes = canvas.BitDepth / 8 * 4;
        var sourceStride = source.Stride; var canvasStride = canvas.Stride;
        var firstRow = backwardsY ? region.Height - 1 : 0;
        var sourceRowOffset = (region.SourceY + firstRow) * sourceStride + region.SourceX * pixelBytes;
        var canvasRowOffset = (region.DestinationY + firstRow) * canvasStride + region.DestinationX * pixelBytes;
        var sourceRowStep = backwardsY ? -sourceStride : sourceStride;
        var canvasRowStep = backwardsY ? -canvasStride : canvasStride;
        var rowBytes = region.Width * pixelBytes;
        if (independentRows && System.Runtime.Intrinsics.Vector512.IsHardwareAccelerated
            && region.Width <= 4096 && rowBytes >= 256)
        {
            if (sameBuffer && canvas.BitDepth == 16)
                IndependentRowCopy.CopySelfRows(canvasPixels, sourceRowOffset, canvasRowOffset,
                    rowBytes, region.Height, canvasRowStep, cancellationToken);
            else if (!sameBuffer && rowBytes >= 512)
                IndependentRowCopy.CopyStagedAlignedRows(sourcePixels, canvasPixels, sourceRowOffset, canvasRowOffset,
                    rowBytes, region.Height, sourceRowStep, canvasRowStep, cancellationToken);
            else
                IndependentRowCopy.CopyRows(sourcePixels, canvasPixels, sourceRowOffset, canvasRowOffset,
                    rowBytes, region.Height, sourceRowStep, canvasRowStep, cancellationToken);
            return;
        }
        if (!sameBuffer && pixelBytes == 8 && region.Width <= 4096 && rowBytes >= 2048 && (rowBytes & 2047) == 0
            && !cancellationToken.CanBeCanceled && System.Runtime.Intrinsics.Vector512.IsHardwareAccelerated)
        {
            IndependentRowCopy.CopySnapshotWordRows(sourcePixels, canvasPixels, sourceRowOffset, canvasRowOffset,
                rowBytes, region.Height, sourceRowStep, canvasRowStep);
            return;
        }
        var rowsRemaining = region.Height;
        while (true)
        {
            for (var first = 0; first < region.Width;)
            {
                var count = Math.Min(4096, region.Width - first); cancellationToken.ThrowIfCancellationRequested();
                var offsetX = backwardsX ? region.Width - first - count : first;
                var offsetBytes = offsetX * pixelBytes; var countBytes = count * pixelBytes;
                var input = sourcePixels.Slice(sourceRowOffset + offsetBytes, countBytes);
                var output = canvasPixels.Slice(canvasRowOffset + offsetBytes, countBytes);
                if (independentRows) IndependentRowCopy.Copy(input, output);
                else input.CopyTo(output);
                first += count;
            }
            // Advance only between valid rows; never calculate a cursor beyond
            // the final row of a potentially array-sized caller buffer.
            if (--rowsRemaining == 0) break;
            sourceRowOffset += sourceRowStep;
            canvasRowOffset += canvasRowStep;
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
