using System.Runtime.InteropServices;

namespace Lokad.Image.Internal;

// Independently derived ordinary, undithered VP8 input conversion. BT.601
// supplies the color weights and limited ranges; RFC6386 defines 4:2:0 planes.
// The pinned encoder's discrete-range/fixed-point and gamma conventions are
// intentional here. Tables are generated from mathematics, never imported.
// Owns normalized preparation planes for the public lossy encoder.
internal sealed class Vp8PreparedInput : IDisposable
{
    private const int MetadataBytes = 128, ScratchBytes = 1024;
    private const int LumaScale = 220 * 256, ChromaScale = 225 * 256;
    private const int Yr = 299 * LumaScale / 1000, Yg = 587 * LumaScale / 1000, Yb = 114 * LumaScale / 1000;
    private const int Ur = -(299 * ChromaScale + 886) / (2 * 886), Ug = -(587 * ChromaScale + 886) / (2 * 886);
    private const int Vr = ChromaScale / 2, Vg = -(587 * ChromaScale + 701) / (2 * 701), Vb = -(114 * ChromaScale + 701) / (2 * 701);
    private readonly OperationContext context;
    private readonly int lumaLength, chromaLength;
    private byte[]? storage;
    private long retainedBytes;
    internal int Width { get; }
    internal int Height { get; }
    internal int Stride { get; }
    internal int StorageHeight { get; }
    internal bool HasAlpha { get; }
    internal long CapacityBytes => retainedBytes;
    private ReadOnlySpan<byte> Storage => storage ?? throw new ObjectDisposedException(nameof(Vp8PreparedInput));
    internal ReadOnlySpan<byte> Y => Storage[..lumaLength];
    internal ReadOnlySpan<byte> U => Storage.Slice(lumaLength, chromaLength);
    internal ReadOnlySpan<byte> V => Storage.Slice(lumaLength + chromaLength, chromaLength);
    internal ReadOnlySpan<byte> Alpha => Storage[(lumaLength + 2 * chromaLength)..];

    private Vp8PreparedInput(OperationContext context, int width, int height, bool alpha)
    {
        this.context = context; Width = width; Height = height; HasAlpha = alpha;
        Stride = (width + 15) / 16 * 16; StorageHeight = (height + 15) / 16 * 16;
        lumaLength = checked(Stride * StorageHeight); chromaLength = lumaLength / 4;
    }

    private static int Sample(ReadOnlySpan<byte> bytes, ReadOnlySpan<ushort> words, bool eightBit, int index)
        => eightBit ? bytes[index] : (words[index] + 128) / 257;

    // Four linear samples use twelve-bit values. Thirty-three inverse knots
    // have seven fractional input bits; interpolation preserves a factor four
    // for the subsequent chroma matrix. Endpoints are rounded half-up.
    private static int Compress(int sum, ReadOnlySpan<int> knots)
    {
        var index = sum >> 9; var fraction = sum & 511;
        return (knots[index] * (512 - fraction) + knots[index + 1] * fraction + 64) >> 7;
    }

    private static int Reduce(ReadOnlySpan<byte> bytes, ReadOnlySpan<ushort> words, bool eightBit,
        int p0, int p1, int p2, int p3, int channel, int alpha,
        ReadOnlySpan<ushort> linear, ReadOnlySpan<int> knots)
    {
        var v0 = linear[Sample(bytes, words, eightBit, p0 + channel)];
        var v1 = linear[Sample(bytes, words, eightBit, p1 + channel)];
        var v2 = linear[Sample(bytes, words, eightBit, p2 + channel)];
        var v3 = linear[Sample(bytes, words, eightBit, p3 + channel)];
        if (alpha is 0 or 1020) return Compress(v0 + v1 + v2 + v3, knots);
        var weighted = (long)v0 * Sample(bytes, words, eightBit, p0 + 3)
            + (long)v1 * Sample(bytes, words, eightBit, p1 + 3)
            + (long)v2 * Sample(bytes, words, eightBit, p2 + 3)
            + (long)v3 * Sample(bytes, words, eightBit, p3 + 3);
        // Generate the bounded reciprocal directly. Truncation is significant;
        // exact division would change some of the pinned encoder's samples.
        return Compress((int)((weighted * ((1 << 19) / alpha)) >> 17), knots);
    }

    // The outer encoder accounts for its borrowed source. This method owns only
    // the output capacity and its transient mathematical workspace, on that SAME
    // operation/root. Neither source pixels nor gamma storage escape preparation.
    internal static Vp8PreparedInput Create(ImageBuffer source, OperationContext context)
    {
        context.Cancellation.ThrowIfCancellationRequested(); context.PixelBytes(source.Width, source.Height, source.BitDepth);
        if (source.Width > 16383 || source.Height > 16383)
            throw new ImageLimitException(ImageLimitKind.Dimension, "VP8 dimensions exceed 16383.");
        ReadOnlySpan<byte> bytes = source.Pixels; var words = MemoryMarshal.Cast<byte, ushort>(bytes);
        var eightBit = source.BitDepth == 8; var width = source.Width; var height = source.Height;
        var hasAlpha = false;
        for (var y = 0; y < height; y++)
        {
            context.Cancellation.ThrowIfCancellationRequested();
            for (var x = 0; x < width; x++)
                hasAlpha |= Sample(bytes, words, eightBit, (y * width + x) * 4 + 3) != 255;
        }
        var result = new Vp8PreparedInput(context, width, height, hasAlpha);
        try
        {
            var capacity = checked(result.lumaLength + 2 * result.chromaLength + (hasAlpha ? width * height : 0));
            context.Reserve(capacity + (long)MetadataBytes); result.retainedBytes = capacity + (long)MetadataBytes;
            context.Cancellation.ThrowIfCancellationRequested(); result.storage = new byte[capacity];
            context.Reserve(ScratchBytes);
            try
            {
                context.Cancellation.ThrowIfCancellationRequested();
                Span<ushort> linear = stackalloc ushort[256]; Span<int> knots = stackalloc int[33];
                for (var i = 0; i < linear.Length; i++) linear[i] = (ushort)Math.Floor(Math.Pow(i / 255d, 0.8) * 4095 + 0.5);
                for (var i = 0; i < knots.Length; i++) knots[i] = (int)Math.Floor(255 * Math.Pow(i * 128d / 4095, 1 / 0.8) + 0.5);
                var yPlane = result.storage.AsSpan(0, result.lumaLength);
                var uPlane = result.storage.AsSpan(result.lumaLength, result.chromaLength);
                var vPlane = result.storage.AsSpan(result.lumaLength + result.chromaLength, result.chromaLength);
                var alphaPlane = result.storage.AsSpan(result.lumaLength + 2 * result.chromaLength);
                for (var y = 0; y < height; y++)
                {
                    context.Cancellation.ThrowIfCancellationRequested();
                    for (var x = 0; x < width; x++)
                    {
                        var p = (y * width + x) * 4;
                        var r = Sample(bytes, words, eightBit, p); var g = Sample(bytes, words, eightBit, p + 1); var b = Sample(bytes, words, eightBit, p + 2);
                        yPlane[y * result.Stride + x] = (byte)((Yr * r + Yg * g + Yb * b + (16 << 16) + (1 << 15)) >> 16);
                        if (hasAlpha) alphaPlane[y * width + x] = (byte)Sample(bytes, words, eightBit, p + 3);
                    }
                }
                var chromaWidth = (width + 1) / 2; var chromaHeight = (height + 1) / 2; var stride = result.Stride / 2;
                for (var y = 0; y < chromaHeight; y++)
                {
                    context.Cancellation.ThrowIfCancellationRequested();
                    var row0 = 2 * y * width * 4; var row1 = Math.Min(2 * y + 1, height - 1) * width * 4;
                    for (var x = 0; x < chromaWidth; x++)
                    {
                        var x0 = 2 * x * 4; var x1 = Math.Min(2 * x + 1, width - 1) * 4;
                        var p0 = row0 + x0; var p1 = row0 + x1; var p2 = row1 + x0; var p3 = row1 + x1;
                        var a = Sample(bytes, words, eightBit, p0 + 3) + Sample(bytes, words, eightBit, p1 + 3)
                            + Sample(bytes, words, eightBit, p2 + 3) + Sample(bytes, words, eightBit, p3 + 3);
                        var r = Reduce(bytes, words, eightBit, p0, p1, p2, p3, 0, a, linear, knots);
                        var g = Reduce(bytes, words, eightBit, p0, p1, p2, p3, 1, a, linear, knots);
                        var b = Reduce(bytes, words, eightBit, p0, p1, p2, p3, 2, a, linear, knots);
                        uPlane[y * stride + x] = (byte)Math.Clamp((Ur * r + Ug * g + (ChromaScale / 2) * b + (128 << 18) + (1 << 17)) >> 18, 0, 255);
                        vPlane[y * stride + x] = (byte)Math.Clamp((Vr * r + Vg * g + Vb * b + (128 << 18) + (1 << 17)) >> 18, 0, 255);
                    }
                }
                result.Pad();
            }
            finally { context.Release(ScratchBytes); }
            return result;
        }
        catch { result.Dispose(); throw; }
    }

    private void Pad()
    {
        var data = storage ?? throw new ObjectDisposedException(nameof(Vp8PreparedInput));
        PadPlane(data.AsSpan(0, lumaLength), Stride, Width, Height);
        PadPlane(data.AsSpan(lumaLength, chromaLength), Stride / 2, (Width + 1) / 2, (Height + 1) / 2);
        PadPlane(data.AsSpan(lumaLength + chromaLength, chromaLength), Stride / 2, (Width + 1) / 2, (Height + 1) / 2);
    }

    private void PadPlane(Span<byte> plane, int stride, int width, int height)
    {
        for (var y = 0; y < height; y++)
        {
            context.Cancellation.ThrowIfCancellationRequested();
            plane.Slice(y * stride + width, stride - width).Fill(plane[y * stride + width - 1]);
        }
        for (var y = height; y < plane.Length / stride; y++)
        {
            context.Cancellation.ThrowIfCancellationRequested();
            plane.Slice((height - 1) * stride, stride).CopyTo(plane.Slice(y * stride));
        }
    }

    // Pinned lossy exact=0 preprocessing. Mixed blocks replace only invisible
    // luma with the visible mean. Fully invisible complete8x8 blocks share the
    // first block's Y/U/V across a horizontal run; partial edge blocks do not.
    // The existing owner is modified in place; no extra storage or allowance.
    internal void CleanupTransparentArea()
    {
        var data = storage ?? throw new ObjectDisposedException(nameof(Vp8PreparedInput));
        context.Cancellation.ThrowIfCancellationRequested(); if (!HasAlpha) return;
        var yPlane = data.AsSpan(0, lumaLength);
        var uPlane = data.AsSpan(lumaLength, chromaLength);
        var vPlane = data.AsSpan(lumaLength + chromaLength, chromaLength);
        var alpha = data.AsSpan(lumaLength + 2 * chromaLength);
        for (var top = 0; top < Height; top += 8)
        {
            var run = false; byte runY = 0, runU = 0, runV = 0;
            var height = Math.Min(8, Height - top);
            for (var left = 0; left < Width; left += 8)
            {
                var width = Math.Min(8, Width - left); var count = 0; var sum = 0;
                context.Cancellation.ThrowIfCancellationRequested();
                for (var y = top; y < top + height; y++)
                for (var x = left; x < left + width; x++)
                    if (alpha[y * Width + x] != 0) { count++; sum += yPlane[y * Stride + x]; }
                if (count is > 0 && count < width * height)
                {
                    var mean = (byte)(sum / count);
                    for (var y = top; y < top + height; y++)
                    for (var x = left; x < left + width; x++)
                        if (alpha[y * Width + x] == 0) yPlane[y * Stride + x] = mean;
                }
                if (count == 0 && width == 8 && height == 8)
                {
                    if (!run)
                    {
                        runY = yPlane[top * Stride + left];
                        runU = uPlane[(top / 2) * (Stride / 2) + left / 2];
                        runV = vPlane[(top / 2) * (Stride / 2) + left / 2]; run = true;
                    }
                    for (var y = top; y < top + 8; y++) yPlane.Slice(y * Stride + left, 8).Fill(runY);
                    for (var y = top / 2; y < top / 2 + 4; y++)
                    {
                        uPlane.Slice(y * (Stride / 2) + left / 2, 4).Fill(runU);
                        vPlane.Slice(y * (Stride / 2) + left / 2, 4).Fill(runV);
                    }
                }
                else run = false;
            }
        }
        // Native macroblock import extends the cleaned logical planes.
        Pad();
    }

    public void Dispose()
    {
        Interlocked.Exchange(ref storage, null);
        var owned = Interlocked.Exchange(ref retainedBytes, 0);
        if (owned != 0) context.Release(owned);
    }
}
