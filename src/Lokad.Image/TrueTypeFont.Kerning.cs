using Lokad.Image.Internal;

namespace Lokad.Image;

public sealed partial class TrueTypeFont
{
    internal sealed class TextKerning : IDisposable
    {
        private readonly TrueTypeFont font;
        private readonly OperationContext context;
        private readonly KernTable[] tables;
        private bool disposed;
        private readonly record struct KernTable(int Offset, int Count, bool Override);
        internal long StorageBytes => 128 + tables.Length * 16L + 32;

        internal TextKerning(TrueTypeFont font, OperationContext context)
        {
            this.font = font; this.context = context;
            var range = font.Find("kern"u8, context);
            if (range is null) { tables = []; context.Reserve(StorageBytes); return; }
            var bytes = font.Span(range.Value);
            if (bytes.Length < 4) throw new ImageDataException("Truncated kerning header.");
            if (U16(bytes) != 0) throw new UnsupportedImageException(UnsupportedImageFeature.FontTable, "Only OpenType kern version zero is supported.");
            var count = U16(bytes[2..]); Items(count, context);
            context.Reserve(128 + count * 16L + 32);
            try
            {
                tables = new KernTable[count]; var offset = 4;
                for (var i = 0; i < count; i++)
                {
                    context.Work(1);
                    if (bytes.Length - offset < 6) throw new ImageDataException("Truncated kerning subtable.");
                    var length = U16(bytes[(offset + 2)..]); var flags = U16(bytes[(offset + 4)..]);
                    if (length < 6 || length > bytes.Length - offset) throw new ImageDataException("Invalid kerning subtable length.");
                    if ((flags & 1) != 0)
                    {
                        if (U16(bytes[offset..]) != 0 || flags >> 8 != 0 || (flags & 6) != 0)
                            throw new UnsupportedImageException(UnsupportedImageFeature.FontTable, "Only ordinary horizontal kern format zero is supported.");
                        if (length < 14) throw new ImageDataException("Truncated kerning pair table.");
                        var pairs = U16(bytes[(offset + 6)..]); Items(pairs, context);
                        if (14L + pairs * 6L > length) throw new ImageDataException("Truncated kerning pairs.");
                        uint previous = 0;
                        for (var j = 0; j < pairs; j++)
                        {
                            context.Work(1); var pair = bytes.Slice(offset + 14 + j * 6, 6); var key = U32(pair);
                            if (U16(pair) >= font.GlyphCount || U16(pair[2..]) >= font.GlyphCount || j != 0 && key <= previous)
                                throw new ImageDataException("Invalid kerning pair order or glyph index.");
                            previous = key;
                        }
                        tables[i] = new(range.Value.Offset + offset + 14, pairs, (flags & 8) != 0);
                    }
                    offset += length;
                }
            }
            catch { context.Release(128 + count * 16L + 32); throw; }
        }
        internal double Get(int left, int right)
        {
            if (left < 0 || right < 0) return 0;
            var key = (uint)left << 16 | (uint)right; double result = 0;
            foreach (var table in tables)
            {
                var low = 0; var high = table.Count - 1;
                while (low <= high)
                {
                    context.Work(1); var mid = (low + high) / 2;
                    var pair = font.data.AsSpan(table.Offset + mid * 6, 6); var actual = U32(pair);
                    if (key < actual) high = mid - 1;
                    else if (key > actual) low = mid + 1;
                    else { var value = I16(pair[4..]); result = table.Override ? value : result + value; break; }
                }
            }
            return result;
        }
        public void Dispose() { if (!disposed) { disposed = true; context.Release(StorageBytes); } }
    }
}
