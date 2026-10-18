namespace Lokad.Image.Internal;

internal sealed partial class IdentificationInput
{
    // The asynchronous path admits actual simultaneous capacities. Its retained
    // capacity stays charged throughout parsing/decoding, without an exact-sized
    // final encoded-input copy. The original synchronous path is unchanged.
    internal static async ValueTask<IdentificationInput> ReadAsync(Stream input, OperationContext context)
    {
        ArgumentNullException.ThrowIfNull(input);
        const int scratchBytes = 8192;
        context.Reserve(scratchBytes);
        byte[] buffer = [];
        var length = 0;
        try
        {
            var scratch = new byte[scratchBytes];
            while (true)
            {
                context.Cancellation.ThrowIfCancellationRequested();
                var read = await input.ReadAsync(scratch.AsMemory(), context.Cancellation).ConfigureAwait(false);
                if ((uint)read > scratchBytes) throw new ImageDataException("Stream returned an invalid read count.");
                if (read == 0) break;
                if (read > context.Limits.MaxInputBytes - length)
                    throw new ImageLimitException(ImageLimitKind.InputBytes, "Encoded input limit exceeded.");
                var needed = (long)length + read;
                if (needed > buffer.Length)
                {
                    var capacity = Math.Min(context.Limits.MaxInputBytes, Math.Max(needed, Math.Max(256, buffer.Length * 2L)));
                    if (capacity > Array.MaxLength)
                        throw new ImageLimitException(ImageLimitKind.BufferLength, "Stream buffering memory limit exceeded.");
                    context.Reserve(capacity);
                    byte[] next;
                    try { next = Rent((int)capacity); }
                    catch { context.Release(capacity); throw; }
                    buffer.AsSpan(0, length).CopyTo(next);
                    Return(buffer); context.Release(buffer.Length); buffer = next;
                }
                scratch.AsSpan(0, read).CopyTo(buffer.AsSpan(length));
                length += read;
                context.Work(read);
            }
            context.Cancellation.ThrowIfCancellationRequested();
            var result = new IdentificationInput(buffer, length);
            buffer = [];
            return result;
        }
        finally
        {
            context.Release(scratchBytes + (long)buffer.Length);
            Return(buffer);
        }
    }
}
