namespace Lokad.Image.Internal;

internal sealed partial class IdentificationInput
{
    // Storage stays charged through actual asynchronous users and subsequent
    // parsing. Reads target owned tails directly; the optional EOF probe is tiny.
    internal static async ValueTask<IdentificationInput> ReadAsync(Stream input, OperationContext context)
    {
        ArgumentNullException.ThrowIfNull(input);
        byte[] buffer = [];
        var length = 0;
        try
        {
            buffer = Grow(buffer, 0, context);
            byte[]? probe = null;
            while (true)
            {
                context.Cancellation.ThrowIfCancellationRequested();
                if (length == buffer.Length)
                {
                    probe ??= new byte[1];
                    var extra = await input.ReadAsync(probe.AsMemory(), context.Cancellation).ConfigureAwait(false);
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
                var read = await input.ReadAsync(buffer.AsMemory(length, requested), context.Cancellation).ConfigureAwait(false);
                if ((uint)read > (uint)requested) throw new ImageDataException("Stream returned an invalid read count.");
                context.Cancellation.ThrowIfCancellationRequested();
                if (read == 0) break;
                length += read;
            }
            context.Cancellation.ThrowIfCancellationRequested();
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
}
