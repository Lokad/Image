namespace Lokad.Image.Internal;

internal static partial class WebpLosslessEncoder
{
    // RFC 9649 sections 2.7.1.2 and 3.6/3.7: ALPH is a headerless VP8L
    // green plane. Horizontal residuals and previous-pixel copies need neither
    // a filtered plane nor a redundant RGBA raster. Count before writing so a
    // nonseekable destination needs no encoded-alpha staging buffer either.
    internal sealed class AlphaEncoding : IDisposable
    {
        private const int WorkingBytes = 2048;
        private readonly OperationContext context;
        private readonly Table green;
        private bool disposed;
        internal int Length { get; private set; }

        private AlphaEncoding(OperationContext context)
        {
            this.context = context;
            green = new Table(280);
        }

        internal static AlphaEncoding? TryCreate(ReadOnlySpan<byte> alpha, int width, int effort, OperationContext context)
        {
            // Select from independent local allowances BEFORE shared admission.
            // Each pass charges one alpha sample (rather than four RGBA
            // channels) and at most one bit-writer call per sample. A copy
            // consumes at least two samples and at most two writer calls.
            if (effort == 0 || alpha.Length < 64 || context.RemainingWorkingBytes < WorkingBytes
                || context.RemainingWork < alpha.Length * 4L + 2048) return null;
            context.Reserve(WorkingBytes);
            AlphaEncoding? result = null;
            try
            {
                context.Work(0); result = new AlphaEncoding(context);
                using var counter = new CountStream(int.MaxValue);
                result.Write(alpha, width, counter);
                result.Length = counter.Count;
                // Include the ALPH header and RIFF padding in the decision.
                var compressed = result.Length + 1; var raw = alpha.Length + 1;
                if (compressed + (compressed & 1) < raw + (raw & 1)) return result;
            }
            catch
            {
                if (result is null) context.Release(WorkingBytes);
                else result.Dispose();
                throw;
            }
            result.Dispose(); return null;
        }

        internal void Write(ReadOnlySpan<byte> alpha, int width, Stream output)
        {
            ObjectDisposedException.ThrowIf(disposed, this);
            var writer = new WebpBitWriter(output, context);
            writer.Write(0, 1); writer.Write(0, 1); writer.Write(0, 1); // No transforms, cache or groups.
            green.Header(writer);
            Singleton(writer, 0); Singleton(writer, 0); Singleton(writer, 0);
            Singleton(writer, 1); // Distance prefix1 decodes to code2: previous pixel.
            var position = 0; var previous = -1;
            while (position < alpha.Length)
            {
                context.Work(1);
                var value = Residual(alpha, width, position); var run = 1;
                if (value == previous)
                {
                    var maximum = Math.Min(4096, alpha.Length - position);
                    while (run < maximum && Residual(alpha, width, position + run) == value) run++;
                }
                if (value == previous && run >= 2)
                {
                    context.Work(run - 1);
                    Prefix(run, out var code, out var extra, out var bits);
                    green.Symbol(writer, 256 + code); writer.Write(extra, bits);
                    // All other channels and the distance are single-leaf
                    // codes, consuming no data bits.
                    position += run;
                }
                else { green.Symbol(writer, value); position++; }
                previous = value;
            }
            writer.Finish();
        }

        private static int Residual(ReadOnlySpan<byte> alpha, int width, int position)
        {
            var predictor = position == 0 ? 0 : position % width == 0 ? alpha[position - width] : alpha[position - 1];
            return unchecked((byte)(alpha[position] - predictor));
        }

        private static void Singleton(WebpBitWriter writer, int symbol)
        {
            writer.Write(1, 1); writer.Write(0, 1); writer.Write(0, 1); writer.Write(symbol, 1);
        }

        public void Dispose()
        {
            if (disposed) return;
            disposed = true; context.Release(WorkingBytes);
        }
    }
}
