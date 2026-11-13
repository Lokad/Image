using System.Buffers;
using System.Numerics;

namespace Lokad.Image.Internal;

/// <summary>Private complete input, charged through its last parser/decoder user.</summary>
internal sealed partial class IdentificationInput : IDisposable
{
    // One idle exact power-of-two array per BCL bucket, at most 8 MiB total.
    // Odd capacities and inputs beyond the ceiling are never retained.
    private const int MaxPooledLength = 4 * 1024 * 1024;
    private static readonly ArrayPool<byte> storage = ArrayPool<byte>.Create(MaxPooledLength, 1);
    private byte[] buffer;
    private readonly int length;
    private readonly bool pooled;
    private IdentificationInput(byte[] buffer, int length, bool pooled = true) { this.buffer = buffer; this.length = length; this.pooled = pooled; }
    public ReadOnlySpan<byte> Span => buffer.AsSpan(0, length);
    internal int Capacity => buffer.Length;

    public static IdentificationInput Read(Stream input, OperationContext context)
    {
        ArgumentNullException.ThrowIfNull(input);
        byte[] buffer = [];
        var length = 0;
        try
        {
            buffer = Prepare(input, context);
            Span<byte> probe = stackalloc byte[1];
            while (true)
            {
                context.Cancellation.ThrowIfCancellationRequested();
                if (length == buffer.Length)
                {
                    // Probe before growing: an exact-capacity input needs no
                    // replacement allocation just to establish EOF.
                    var extra = input.Read(probe);
                    if ((uint)extra > 1) throw new ImageDataException("Stream returned an invalid read count.");
                    context.Cancellation.ThrowIfCancellationRequested();
                    if (extra == 0) break;
                    if (length == context.Limits.MaxInputBytes)
                        throw new ImageLimitException(ImageLimitKind.InputBytes, "Encoded input limit exceeded.");
                    buffer = Grow(buffer, length, context);
                    buffer[length++] = probe[0];
                    continue;
                }
                var requested = Math.Min(8192, buffer.Length - length);
                var read = input.Read(buffer.AsSpan(length, requested));
                if ((uint)read > (uint)requested) throw new ImageDataException("Stream returned an invalid read count.");
                context.Cancellation.ThrowIfCancellationRequested();
                if (read == 0) break;
                length += read;
            }
            var result = new IdentificationInput(buffer, length, !context.HasMemoryBudget);
            buffer = [];
            return result;
        }
        finally
        {
            Return(buffer, !context.HasMemoryBudget);
            context.Release(buffer.Length);
        }
    }

    private static byte[] Prepare(Stream input, OperationContext context)
    {
        // Only the ordinary in-memory stream has trusted, inexpensive metadata.
        // Derived/custom streams keep the original metadata-free acquisition.
        if (input.GetType() == typeof(MemoryStream))
        {
            var remaining = input.Length - input.Position;
            // Reuse the existing storage ceiling to bound eager preparation.
            // Large inputs retain incremental admission; Length never proves EOF.
            if (remaining is > 256 and <= MaxPooledLength)
            {
                var capacity = (int)Math.Min(Math.Min(context.Limits.MaxInputBytes, Array.MaxLength),
                    BitOperations.RoundUpToPowerOf2((uint)remaining));
                context.Reserve(capacity);
                try
                {
                    context.Cancellation.ThrowIfCancellationRequested();
                    return Rent(capacity, !context.HasMemoryBudget);
                }
                catch { context.Release(capacity); throw; }
            }
        }
        return Grow([], 0, context);
    }

    private static byte[] Grow(byte[] buffer, int length, OperationContext context)
    {
        var capacity = Math.Min(Math.Min(context.Limits.MaxInputBytes, Array.MaxLength),
            Math.Max(256, buffer.Length * 2L));
        if (capacity <= length)
            throw new ImageLimitException(ImageLimitKind.BufferLength, "Stream buffering memory limit exceeded.");
        // Old and new storage coexist through the copy. Refusal is terminal;
        // never retry with another capacity or return an active IO buffer.
        context.Reserve(capacity);
        byte[] next;
        try
        {
            context.Cancellation.ThrowIfCancellationRequested();
            next = Rent((int)capacity, !context.HasMemoryBudget);
        }
        catch { context.Release(capacity); throw; }
        buffer.AsSpan(0, length).CopyTo(next);
        Return(buffer, !context.HasMemoryBudget); context.Release(buffer.Length);
        return next;
    }
    private static bool Pooled(int length) => length >= 256 && length <= MaxPooledLength && BitOperations.IsPow2(length);
    private static byte[] Rent(int length, bool pooled = true)
    {
        if (pooled && Pooled(length))
        {
            var bytes = storage.Rent(length);
            if (bytes.Length == length) return bytes;
            // The BCL may offer the next bucket when this one is busy. Keep
            // physical capacity identical to the original admission instead.
            storage.Return(bytes);
        }
        return GC.AllocateUninitializedArray<byte>(length);
    }
    private static void Return(byte[] bytes, bool pooled = true) { if (pooled && Pooled(bytes.Length)) storage.Return(bytes); }
    public void Dispose() => Return(Interlocked.Exchange(ref buffer, []), pooled);
}
