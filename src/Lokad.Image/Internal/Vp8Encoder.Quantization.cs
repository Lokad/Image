using System.Runtime.InteropServices;

namespace Lokad.Image.Internal;

internal static partial class Vp8Encoder
{
    // Independently derived source-conversion-loss masking. Transform coefficient
    // noise of variance step²/12 gives pixel variance step²/48 for our 4x4 DCT.
    // Propagate independent noise through centered chroma interpolation, combining
    // coincident edge taps. This is a local equal-step encoding heuristic, not an
    // output error bound: reconstruction rounding and noise can be correlated.
    internal sealed class QuantizationPlan : IDisposable
    {
        private const double LumaNoiseGain = (255d / 219) * (255d / 219);
        private const double ChromaNoiseGain = (1.596026785714286 * 1.596026785714286 + .391762290094914 * .391762290094914
                + .812967647237771 * .812967647237771 + 2.017232142857143 * 2.017232142857143) / 3;
        private readonly OperationContext context;
        private readonly long memory;
        private bool disposed;
        internal byte[] Map { get; }
        internal byte[] Quantizers { get; } = new byte[4];
        private readonly byte[] probabilities = new byte[3];

        private QuantizationPlan(int count, OperationContext context)
        { this.context = context; memory = count + 256L; Map = new byte[count]; }

        internal static QuantizationPlan? TryCreate(ImageBuffer image, Vp8Planes source, int q, int quality,
            int effort, OperationContext context)
        {
            if (effort < 4) return null;
            var columns = source.Stride / 16; var count = columns * (source.Y.Length / source.Stride / 16);
            ReadOnlySpan<int> trials = [1, 2, 3, 4, 6, 8, 10];
            var predictionWork = count * (16L * trials[effort] * 192 + 4L * 512 + 24L * 512 + 64);
            var prefixWork = Math.Min(count, 64) * 24L * 256 + 1056 * 16 + count * 24L;
            var analysisWork = count * (16L * 96 + 128 + 4) + 8192;
            var maximumHeader = ((count * (1L + 16 * 9 + 3 + 2) + 1056L * 9 + 192) * 7 + 7) / 8 + 3;
            if (count < 4 || count + 256L > context.RemainingWorkingBytes || maximumHeader > 0x7FFFF
                || predictionWork + prefixWork + analysisWork > context.RemainingWork) return null;
            context.Reserve(count + 256L); QuantizationPlan? plan = null;
            try
            {
                context.Work(0); plan = new QuantizationPlan(count, context);
                if (plan.Prepare(image, source, q, quality, columns)) return plan;
            }
            catch
            {
                if (plan is null) context.Release(count + 256L); else plan.Dispose();
                throw;
            }
            plan.Dispose(); return null;
        }

        private bool Prepare(ImageBuffer image, Vp8Planes source, int baseQ, int quality, int columns)
        {
            ReadOnlySpan<byte> pixels = image.Pixels; var words = MemoryMarshal.Cast<byte, ushort>(pixels);
            var byteDepth = image.BitDepth == 8; var steps = Vp8Tables.AcQuantizers;
            Span<int> histogram = stackalloc int[128]; histogram.Clear(); var active = false;
            for (var index = 0; index < Map.Length; index++)
            {
                long error = 0; double noise = 0;
                var mx = index % columns * 16; var my = index / columns * 16;
                for (var by = 0; by < 4; by++)
                for (var bx = 0; bx < 4; bx++)
                {
                    context.Work(96);
                    var x = Math.Min(image.Width - 1, mx + bx * 4 + 1);
                    var y = Math.Min(image.Height - 1, my + by * 4 + 1);
                    var at = (y * image.Width + x) * 4;
                    var r = Sample(pixels, words, byteDepth, at); var g = Sample(pixels, words, byteDepth, at + 1);
                    var b = Sample(pixels, words, byteDepth, at + 2); var a = Sample(pixels, words, byteDepth, at + 3);
                    var weight = a * a;
                    error += weight * (long)Vp8Color.SourceError(source, x, y, r, g, b);
                    noise += weight * NoiseGain(x, y, image.Width, image.Height);
                }
                context.Work(128);
                var targetSquared = noise == 0 ? 0 : 16d * error / (noise * (quality + 1));
                var q = baseQ;
                while (q < 127 && steps[q + 1] * steps[q + 1] <= targetSquared) q++;
                Map[index] = (byte)q; histogram[q]++; active |= q != baseQ;
            }
            if (!active) return false;
            Span<double> values = stackalloc double[128]; context.Work(128);
            for (var q = 0; q < 128; q++) values[q] = Math.Log2(steps[q]);
            var first = 0; while (histogram[first] == 0) first++;
            var last = 127; while (histogram[last] == 0) last--;
            Span<double> centers = stackalloc double[4];
            for (var i = 0; i < 4; i++) centers[i] = values[first] + (values[last] - values[first]) * i / 3;
            Span<double> sums = stackalloc double[4]; Span<int> counts = stackalloc int[4];
            for (var pass = 0; pass < 4; pass++)
            {
                context.Work(512); sums.Clear(); counts.Clear();
                for (var q = first; q <= last; q++)
                {
                    if (histogram[q] == 0) continue;
                    var group = Nearest(values[q], centers);
                    sums[group] += values[q] * histogram[q]; counts[group] += histogram[q];
                }
                for (var i = 0; i < 4; i++) if (counts[i] != 0) centers[i] = sums[i] / counts[i];
            }
            context.Work(512);
            for (var i = 0; i < 4; i++)
            {
                var q = baseQ; while (q < 127 && values[q + 1] <= centers[i]) q++;
                Quantizers[i] = (byte)q;
            }
            counts.Clear();
            for (var i = 0; i < Map.Length; i++)
            { context.Work(4); Map[i] = (byte)Nearest(values[Map[i]], centers); counts[Map[i]]++; }
            probabilities[0] = Probability(counts[0] + counts[1], Map.Length);
            probabilities[1] = Probability(counts[0], counts[0] + counts[1]);
            probabilities[2] = Probability(counts[2], counts[2] + counts[3]);
            return true;
        }

        internal static double NoiseGain(int x, int y, int width, int height)
            => LumaNoiseGain + ChromaNoiseGain * AxisNoise(x, width) * AxisNoise(y, height);

        private static double AxisNoise(int coordinate, int length)
        {
            var first = (coordinate - 1) / 2; var last = Math.Min(first + 1, (length + 1) / 2 - 1);
            if (first == last || coordinate == 0) return 1;
            var phase = (coordinate & 1) == 0 ? 3 : 1;
            return ((4 - phase) * (4 - phase) + phase * phase) / 16d;
        }

        private static int Nearest(double value, ReadOnlySpan<double> centers)
        {
            var best = 0;
            for (var i = 1; i < 4; i++) if (Math.Abs(value - centers[i]) < Math.Abs(value - centers[best])) best = i;
            return best;
        }
        private static byte Probability(int zero, int total)
            => total == 0 ? (byte)128 : (byte)Math.Clamp((zero * 256L + total / 2) / total, 1, 255);

        // Independently authored RFC6386 section9 absolute segment syntax.
        internal void Header(Vp8BooleanWriter writer)
        {
            writer.Write(1); writer.Write(1); writer.Write(1); writer.Write(1);
            for (var i = 0; i < 4; i++) { writer.Write(1); writer.Literal(Quantizers[i], 7); writer.Write(0); }
            for (var i = 0; i < 4; i++) writer.Write(0);
            for (var i = 0; i < 3; i++) { writer.Write(1); writer.Literal(probabilities[i], 8); }
        }
        internal void Macroblock(Vp8BooleanWriter writer, int index)
        {
            var group = Map[index]; writer.Write(group >> 1, probabilities[0]); writer.Write(group & 1, probabilities[1 + (group >> 1)]);
        }
        public void Dispose()
        { if (disposed) return; disposed = true; context.Release(memory); }
    }
}
