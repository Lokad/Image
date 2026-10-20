using System.Numerics;

namespace Lokad.Image.Internal;

internal static partial class WebpLosslessEncoder
{
    internal sealed partial class AlphaEncoding
    {
        private const int HistorySize = 16384 * 4;
        private const int HistoryBytes = HistorySize * 4 + 32;
        private int[]? history;

        // RFC9649 section3.6.2.2 permits exact overlapping backward references.
        // Four recent positions per three-residual hash bound search/storage.
        // Every attempted match is no longer than the selected maximum; copies
        // consume that maximum, bounding comparisons by12 per source sample.
        private void ScanMatches(ReadOnlySpan<byte> alpha, int width, WebpBitWriter? writer,
            Span<int> frequencies, Span<int> distances)
        {
            var entries = history!;
            context.Work(HistorySize); entries.AsSpan().Fill(-1);
            var position = 0;
            while (position < alpha.Length)
            {
                context.Work(1);
                var length = 0; var distance = 0;
                var maximum = Math.Min(4096, alpha.Length - position);
                if (maximum >= 3)
                {
                    context.Work(4); var hash = Hash(alpha, width, position);
                    for (var slot = 0; slot < 4; slot++)
                    {
                        var previous = entries[hash + slot]; if (previous < 0) break;
                        var behind = position - previous;
                        // Generic distance codes above120 carry offset+120.
                        if (behind > 1_048_456) continue;
                        var matched = 0;
                        while (matched < maximum && Residual(alpha, width, previous + matched)
                            == Residual(alpha, width, position + matched)) matched++;
                        context.Work(matched + 1L);
                        if (matched > length) { length = matched; distance = behind; }
                    }
                }
                var consumed = 1;
                if (length >= 3)
                {
                    Prefix(length, out var code, out var extra, out var bits);
                    Symbol(writer, frequencies, 256 + code);
                    if (writer is not null) writer.Write(extra, bits);
                    Distance(distance, width, out code, out extra, out bits);
                    if (writer is null) distances[code]++;
                    else { adaptiveDistance!.Write(writer, code); writer.Write(extra, bits); }
                    consumed = length;
                }
                else Symbol(writer, frequencies, Residual(alpha, width, position));
                context.Work(consumed * 8L);
                for (var i = 0; i < consumed && position + i + 2 < alpha.Length; i++)
                {
                    var hash = Hash(alpha, width, position + i);
                    entries[hash + 3] = entries[hash + 2]; entries[hash + 2] = entries[hash + 1];
                    entries[hash + 1] = entries[hash]; entries[hash] = position + i;
                }
                position += consumed;
            }
        }

        private static int Hash(ReadOnlySpan<byte> alpha, int width, int position)
            => (((Residual(alpha, width, position) * 251 + Residual(alpha, width, position + 1)) * 251
                + Residual(alpha, width, position + 2)) & 16383) * 4;

        private static void Distance(int distance, int width, out int code, out int extra, out int bits)
        {
            if (distance == width) { code = extra = bits = 0; return; }
            if (distance == 1) { code = 1; extra = bits = 0; return; }
            var value = distance + 119; bits = BitOperations.Log2((uint)value) - 1;
            code = 2 * (bits + 1) + (value >> bits & 1);
            extra = value - ((2 + (code & 1)) << bits);
        }
    }
}
