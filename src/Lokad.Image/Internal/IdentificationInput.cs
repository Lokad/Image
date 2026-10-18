using System.Buffers;
using System.Numerics;

namespace Lokad.Image.Internal;

/// <summary>Private, fully written input for metadata inspection; never exposed to a caller.</summary>
internal sealed class IdentificationInput : IDisposable
{
    // One idle exact power-of-two array per BCL bucket, at most 8 MiB total.
    // Odd capacities and inputs beyond the ceiling are never retained.
    private const int MaxPooledLength = 4 * 1024 * 1024;
    private static readonly ArrayPool<byte> storage = ArrayPool<byte>.Create(MaxPooledLength, 1);
    private byte[] buffer;
    private readonly int length;
    private IdentificationInput(byte[] buffer, int length) { this.buffer = buffer; this.length = length; }
    public ReadOnlySpan<byte> Span => buffer.AsSpan(0, length);

    public static IdentificationInput Read(Stream input, OperationContext context)
    {
        ArgumentNullException.ThrowIfNull(input);
        var limits = context.Limits;
        context.Reserve(8192);
        Span<byte> scratch = stackalloc byte[8192];
        byte[] buffer = [];
        var length = 0;
        try
        {
            while (true)
            {
                context.Cancellation.ThrowIfCancellationRequested();
                var read = input.Read(scratch);
                if (read == 0) break;
                if (read > limits.MaxInputBytes - (long)length) throw new ImageLimitException(ImageLimitKind.InputBytes, "Encoded input limit exceeded.");
                var needed = (long)length + read;
                var capacity = Math.Max(needed, Math.Max(256, buffer.Length * 2L));
                if (needed > buffer.Length)
                {
                    if (capacity + buffer.Length + scratch.Length > limits.MaxWorkingBytes || capacity > Array.MaxLength)
                        throw new ImageLimitException(capacity + buffer.Length + scratch.Length > limits.MaxWorkingBytes
                            ? ImageLimitKind.WorkingBytes : ImageLimitKind.BufferLength, "Stream buffering memory limit exceeded.");
                    var next = Rent((int)capacity);
                    buffer.AsSpan(0, length).CopyTo(next);
                    Return(buffer); buffer = next;
                }
                scratch[..read].CopyTo(buffer.AsSpan(length));
                length += read;
                context.Work(read);
            }
            // Keep the established admission and work/error ordering. Parsing the
            // fully acquired private prefix avoids the former exact-sized copy.
            if ((long)buffer.Length + length + scratch.Length > limits.MaxWorkingBytes)
                throw new ImageLimitException(ImageLimitKind.WorkingBytes, "Stream buffering memory limit exceeded.");
            context.Release(scratch.Length);
            context.Reserve(length);
            var result = new IdentificationInput(buffer, length);
            buffer = [];
            return result;
        }
        finally { Return(buffer); }
    }
    private static bool Pooled(int length) => length >= 256 && length <= MaxPooledLength && BitOperations.IsPow2(length);
    private static byte[] Rent(int length)
    {
        if (Pooled(length))
        {
            var bytes = storage.Rent(length);
            if (bytes.Length == length) return bytes;
            // The BCL may offer the next bucket when this one is busy. Keep
            // physical capacity identical to the original admission instead.
            storage.Return(bytes);
        }
        return GC.AllocateUninitializedArray<byte>(length);
    }
    private static void Return(byte[] bytes) { if (Pooled(bytes.Length)) storage.Return(bytes); }
    public void Dispose() => Return(Interlocked.Exchange(ref buffer, []));
}
