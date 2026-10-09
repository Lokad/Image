using System.Runtime.InteropServices;
using Lokad.Image.Internal;

namespace Lokad.Image;

/// <summary>An owned, tightly packed, top-to-bottom RGBA raster with straight alpha.</summary>
/// <remarks>16-bit channels use native-endian unsigned samples. Row spans remain valid only
/// while this buffer is alive. Disposal drops ownership; previously returned spans cannot be revoked.</remarks>
public sealed class ImageBuffer : IDisposable
{
    private byte[]? pixels;
    public int Width { get; }
    public int Height { get; }
    public int BitDepth { get; }
    public int Stride => checked(Width * 4 * (BitDepth / 8));
    public ImageOrientation Orientation { get; internal set; } = ImageOrientation.Normal;
    internal int ByteLength => Pixels.Length;
    internal Span<byte> Pixels => pixels ?? throw new ObjectDisposedException(nameof(ImageBuffer));

    private ImageBuffer(int width, int height, int bitDepth, byte[] data)
    {
        Width = width;
        Height = height;
        BitDepth = bitDepth;
        pixels = data;
    }

    public static ImageBuffer Create(int width, int height, int bitDepth,
        ImageLimits? limits = null, CancellationToken cancellationToken = default)
        => Allocate(width, height, bitDepth, new OperationContext(limits ?? ImageLimits.Default, cancellationToken));

    internal static ImageBuffer Allocate(int width, int height, int bitDepth, OperationContext context)
    {
        var length = context.PixelBytes(width, height, bitDepth);
        context.Reserve(length);
        return new ImageBuffer(width, height, bitDepth, new byte[length]);
    }

    public Span<byte> GetRow8(int y)
    {
        if (BitDepth != 8) throw new InvalidOperationException("Use GetRow16 for 16-bit pixels.");
        return Row(y);
    }

    public Span<ushort> GetRow16(int y)
    {
        if (BitDepth != 16) throw new InvalidOperationException("Use GetRow8 for 8-bit pixels.");
        return MemoryMarshal.Cast<byte, ushort>(Row(y));
    }

    private Span<byte> Row(int y)
    {
        ObjectDisposedException.ThrowIf(pixels is null, this);
        if ((uint)y >= (uint)Height) throw new ArgumentOutOfRangeException(nameof(y));
        return Pixels.Slice(y * Stride, Stride);
    }

    public ImageBuffer Clone(ImageLimits? limits = null, CancellationToken cancellationToken = default)
    {
        var context = new OperationContext(limits ?? ImageLimits.Default, cancellationToken);
        context.Reserve(ByteLength);
        var result = Allocate(Width, Height, BitDepth, context);
        for (var y = 0; y < Height; y++)
        {
            var row = Row(y); var destination = result.Row(y);
            for (var offset = 0; offset < row.Length;)
            {
                var count = Math.Min(16384, row.Length - offset);
                context.Work(count / (BitDepth / 8));
                row.Slice(offset, count).CopyTo(destination.Slice(offset, count)); offset += count;
            }
        }
        result.Orientation = Orientation;
        return result;
    }

    internal int Sample(int x, int y, int channel)
        => BitDepth == 8 ? GetRow8(y)[x * 4 + channel] : GetRow16(y)[x * 4 + channel];

    internal void SetSample(int x, int y, int channel, int value)
    {
        if (BitDepth == 8) GetRow8(y)[x * 4 + channel] = (byte)value;
        else GetRow16(y)[x * 4 + channel] = (ushort)value;
    }

    public void Dispose() => pixels = null;
}
