namespace Lokad.Image.Internal;

internal sealed partial class IdentificationInput
{
    // Decode keeps actual capacity admitted until its final use, matching async
    // acquisition. Full Identify's established synchronous admission is unchanged.
    internal static IdentificationInput ReadForDecode(Stream input, OperationContext context)
    {
        ArgumentNullException.ThrowIfNull(input);
        const int scratchBytes = 8192;
        context.Reserve(scratchBytes);
        Span<byte> scratch = stackalloc byte[scratchBytes];
        byte[] buffer = [];
        var length = 0;
        try
        {
            while (true)
            {
                context.Cancellation.ThrowIfCancellationRequested();
                var read = input.Read(scratch);
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
                scratch[..read].CopyTo(buffer.AsSpan(length));
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
