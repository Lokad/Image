namespace Lokad.Image.Internal;

internal static partial class WebpLosslessEncoder
{
    // RFC 9649 sections 2.7.1.2 and 3.6/3.7: ALPH is a headerless VP8L
    // green plane. Horizontal residuals and previous-pixel copies need neither
    // a filtered plane nor a redundant RGBA raster. Count before writing so a
    // nonseekable destination needs no encoded-alpha staging buffer either.
    internal sealed partial class AlphaEncoding : IDisposable
    {
        private const int WorkingBytes = 2048;
        private readonly OperationContext context;
        private readonly Table green;
        private WebpPrefixCode? adaptiveGreen;
        private WebpPrefixCode? adaptiveDistance;
        private bool disposed;
        internal int Length { get; private set; }
        internal byte Header => 5; // Exact compressed horizontal ALPH.

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
            // The history plus16KiB bounds both tables and construction scratch.
            // At most12 match comparisons/sample/pass, constant hash maintenance
            // and prefix work fit96*N+5.2M for both candidates and selected output.
            var adaptive = effort >= 4 && context.RemainingWorkingBytes >= HistoryBytes + 16384
                && context.RemainingWork >= alpha.Length * 96L + 5_200_000;
            context.Reserve(WorkingBytes);
            AlphaEncoding? result = null;
            try
            {
                context.Work(0); result = new AlphaEncoding(context);
                using var counter = new CountStream(int.MaxValue);
                result.Write(alpha, width, counter);
                result.Length = counter.Count;
                if (adaptive) result.SelectAdaptive(alpha, width);
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
            if (adaptiveGreen is null) green.Header(writer); else adaptiveGreen.Header(writer, context);
            Singleton(writer, 0); Singleton(writer, 0); Singleton(writer, 0);
            if (adaptiveDistance is null) Singleton(writer, 1); // Code2: previous pixel.
            else adaptiveDistance.Header(writer, context);
            if (history is null) Scan(alpha, width, writer, default);
            else ScanMatches(alpha, width, writer, default, default);
            writer.Finish();
        }

        private void SelectAdaptive(ReadOnlySpan<byte> alpha, int width)
        {
            var originalLength = Length;
            Span<int> frequencies = stackalloc int[280]; frequencies.Clear();
            Span<int> distances = stackalloc int[40]; distances.Clear();
            context.Reserve(HistoryBytes); context.Work(0); history = new int[HistorySize];
            ScanMatches(alpha, width, null, frequencies, distances);
            adaptiveGreen = new WebpPrefixCode(frequencies, context);
            adaptiveDistance = new WebpPrefixCode(distances, context);
            using var counter = new CountStream(int.MaxValue);
            Write(alpha, width, counter);
            var candidate = counter.Count + 1; var original = originalLength + 1;
            if (candidate + (candidate & 1) < original + (original & 1)) Length = counter.Count;
            else
            {
                context.Release(adaptiveGreen.MemoryBytes); adaptiveGreen = null;
                if (adaptiveDistance is not null) { context.Release(adaptiveDistance.MemoryBytes); adaptiveDistance = null; }
                if (history is not null) { context.Release(HistoryBytes); history = null; }
                Length = originalLength;
            }
        }

        private void Scan(ReadOnlySpan<byte> alpha, int width, WebpBitWriter? writer, Span<int> frequencies)
        {
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
                    Symbol(writer, frequencies, 256 + code);
                    if (writer is not null) writer.Write(extra, bits);
                    // All other channels and the distance are single-leaf
                    // codes, consuming no data bits.
                    position += run;
                }
                else { Symbol(writer, frequencies, value); position++; }
                previous = value;
            }
        }

        private void Symbol(WebpBitWriter? writer, Span<int> frequencies, int symbol)
        {
            if (writer is null) frequencies[symbol]++;
            else if (adaptiveGreen is null) green.Symbol(writer, symbol);
            else adaptiveGreen.Write(writer, symbol);
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
            disposed = true;
            if (adaptiveGreen is not null) context.Release(adaptiveGreen.MemoryBytes);
            if (adaptiveDistance is not null) context.Release(adaptiveDistance.MemoryBytes);
            if (history is not null) context.Release(HistoryBytes);
            context.Release(WorkingBytes);
        }
    }
}
