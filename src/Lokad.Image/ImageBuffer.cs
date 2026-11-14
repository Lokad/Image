using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Lokad.Image.Internal;

namespace Lokad.Image;

/// <summary>An owned, tightly packed, top-to-bottom RGBA raster with straight alpha.</summary>
/// <remarks>16-bit channels use native-endian unsigned samples. Row spans remain valid only
/// while this buffer is alive. Disposal drops ownership; previously returned spans cannot be revoked.</remarks>
public sealed class ImageBuffer : IDisposable
{
    private byte[]? pixels;
    private readonly bool reusableStorage;
    private IMemoryReservation? memoryOwner;
    public int Width { get; }
    public int Height { get; }
    public int BitDepth { get; }
    public int Stride => checked(Width * 4 * (BitDepth / 8));
    public ImageOrientation Orientation { get; internal set; } = ImageOrientation.Normal;
    internal int ByteLength => Pixels.Length;
    internal Span<byte> Pixels => pixels ?? throw new ObjectDisposedException(nameof(ImageBuffer));

    private ImageBuffer(int width, int height, int bitDepth, byte[] data, bool reusableStorage = false,
        IMemoryReservation? memoryOwner = null)
    {
        Width = width;
        Height = height;
        BitDepth = bitDepth;
        pixels = data;
        this.reusableStorage = reusableStorage;
        this.memoryOwner = memoryOwner;
    }

    public static ImageBuffer Create(int width, int height, int bitDepth,
        ImageLimits? limits = null, CancellationToken cancellationToken = default)
    {
        using var context = new OperationContext(limits ?? ImageLimits.Default, cancellationToken);
        if (context.HasMemoryBudget) return Allocate(width, height, bitDepth, context);
        var length = context.PixelBytes(width, height, bitDepth);
        context.ReserveBorrowed(length);
        byte[]? data = null;
        try
        {
            data = PixelStoragePool.Shared.RentImage(length, context.Cancellation, clear: true);
            return new ImageBuffer(width, height, bitDepth, data, reusableStorage: true);
        }
        catch
        {
            if (data is not null) PixelStoragePool.Shared.Return(data);
            throw;
        }
    }

    internal static ImageBuffer Allocate(int width, int height, int bitDepth, OperationContext context)
    {
        var length = context.PixelBytes(width, height, bitDepth);
        context.ReserveBorrowed(length);
        var owner = MemoryAdmission.Own(context.Limits.MemoryBudget, length);
        try { return new ImageBuffer(width, height, bitDepth, new byte[length], memoryOwner: owner); }
        catch { owner?.Dispose(); throw; }
    }

    // Only complete-overwrite operations may publish these buffers. They must
    // dispose the private result on failure before returning to their caller.
    internal static ImageBuffer AllocateForOverwrite(int width, int height, int bitDepth, OperationContext context)
    {
        var length = context.PixelBytes(width, height, bitDepth);
        context.ReserveBorrowed(length);
        var owner = MemoryAdmission.Own(context.Limits.MemoryBudget, length);
        try
        {
            // Governed calls never populate an unowned process-wide pool. This
            // policy is selected before admission, not as recovery from refusal.
            var reusable = !context.HasMemoryBudget;
            var data = reusable ? PixelStoragePool.Shared.RentImage(length, context.Cancellation, clear: false) : GC.AllocateUninitializedArray<byte>(length);
            return new ImageBuffer(width, height, bitDepth, data, reusable, owner);
        }
        catch { owner?.Dispose(); throw; }
    }

    // Copy-only operations borrow a valid raster and own only their returned
    // pixels. Admit that escaping owner directly; no temporary storage scope is
    // needed. Keep policy, cancellation, source and result checks in that order.
    internal static ImageBuffer AllocateCopyResult(ImageBuffer source, int width, int height,
        ImageLimits? limits, CancellationToken cancellationToken)
    {
        limits ??= ImageLimits.Default; limits.Validate(); cancellationToken.ThrowIfCancellationRequested();
        var sourceLength = source.ByteLength;
        if (sourceLength > limits.MaxWorkingBytes)
            throw new ImageLimitException(ImageLimitKind.WorkingBytes, "Working memory limit exceeded.");
        if (width < 1 || height < 1)
            throw new ArgumentOutOfRangeException(nameof(width), "Dimensions must be positive.");
        // The borrowed ImageBuffer establishes valid 8/16-bit samples. These
        // operations copy a subset or permutation of them, without changing depth.
        var pixelBytes = source.BitDepth / 8 * 4;
        var pixels = (long)width * height;
        if (width > limits.MaxDimension || height > limits.MaxDimension || pixels > limits.MaxPixels
            || pixels > Array.MaxLength / pixelBytes)
            throw new ImageLimitException(width > limits.MaxDimension || height > limits.MaxDimension
                ? ImageLimitKind.Dimension : pixels > limits.MaxPixels
                ? ImageLimitKind.PixelCount : ImageLimitKind.BufferLength,
                "Image dimensions or pixel count exceed the configured limits.");
        var length = (int)(pixels * pixelBytes);
        if (length > limits.MaxWorkingBytes - sourceLength)
            throw new ImageLimitException(ImageLimitKind.WorkingBytes, "Working memory limit exceeded.");
        var owner = MemoryAdmission.Own(limits.MemoryBudget, length);
        try
        {
            var reusable = limits.MemoryBudget is null;
            var data = reusable ? PixelStoragePool.Shared.RentImage(length, cancellationToken, clear: false)
                : GC.AllocateUninitializedArray<byte>(length);
            return new ImageBuffer(width, height, source.BitDepth, data, reusable, owner);
        }
        catch { owner?.Dispose(); throw; }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Span<byte> GetRow8(int y)
    {
        if (BitDepth != 8) throw new InvalidOperationException("Use GetRow16 for 16-bit pixels.");
        return Row(y);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Span<ushort> GetRow16(int y)
    {
        if (BitDepth != 16) throw new InvalidOperationException("Use GetRow8 for 8-bit pixels.");
        return MemoryMarshal.Cast<byte, ushort>(Row(y));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private Span<byte> Row(int y)
    {
        ObjectDisposedException.ThrowIf(pixels is null, this);
        if ((uint)y >= (uint)Height) throw new ArgumentOutOfRangeException(nameof(y));
        return Pixels.Slice(y * Stride, Stride);
    }

    public ImageBuffer Clone(ImageLimits? limits = null, CancellationToken cancellationToken = default)
    {
        var result = AllocateCopyResult(this, Width, Height, limits, cancellationToken);
        try
        {
            ReadOnlySpan<byte> input = Pixels;
            var destination = result.Pixels;
            // A raster is tightly packed. Batch across row boundaries while
            // retaining bounded checkpoints whenever cancellation is possible.
            if (!cancellationToken.CanBeCanceled) input.CopyTo(destination);
            else
            {
                for (var offset = 0; offset < input.Length;)
                {
                    var count = Math.Min(16384, input.Length - offset);
                    cancellationToken.ThrowIfCancellationRequested();
                    input.Slice(offset, count).CopyTo(destination.Slice(offset, count)); offset += count;
                }
            }
            result.Orientation = Orientation;
            return result;
        }
        catch { result.Dispose(); throw; }
    }

    internal int Sample(int x, int y, int channel)
        => BitDepth == 8 ? GetRow8(y)[x * 4 + channel] : GetRow16(y)[x * 4 + channel];

    internal void SetSample(int x, int y, int channel, int value)
    {
        if (BitDepth == 8) GetRow8(y)[x * 4 + channel] = (byte)value;
        else GetRow16(y)[x * 4 + channel] = (ushort)value;
    }

    public void Dispose()
    {
        var data = Interlocked.Exchange(ref pixels, null);
        if (reusableStorage && data is not null) PixelStoragePool.Shared.ReturnLargest(data);
        Interlocked.Exchange(ref memoryOwner, null)?.Dispose();
    }
}
