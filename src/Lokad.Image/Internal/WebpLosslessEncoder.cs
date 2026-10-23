using System.Buffers.Binary;

namespace Lokad.Image.Internal;

/// <summary>Safe managed VP8L encoding with admitted transforms, references and entropy coding.</summary>
internal static partial class WebpLosslessEncoder
{
    private sealed class Table
    {
        private readonly byte[] lengths;
        private readonly ushort[] codes;

        public Table(int alphabet)
        {
            lengths = new byte[alphabet]; codes = new ushort[alphabet];
            var depth = 0;
            while (1 << (depth + 1) <= alphabet) depth++;
            var shortCodes = (1 << (depth + 1)) - alphabet;
            for (var symbol = 0; symbol < alphabet; symbol++)
            {
                var length = symbol < shortCodes ? depth : depth + 1;
                var code = symbol < shortCodes ? symbol : shortCodes * 2 + symbol - shortCodes;
                lengths[symbol] = (byte)length;
                var reversed = 0;
                for (var bit = 0; bit < length; bit++) { reversed = reversed * 2 + (code & 1); code >>= 1; }
                codes[symbol] = (ushort)reversed;
            }
        }

        public void Header(WebpBitWriter writer)
        {
            ReadOnlySpan<byte> order = [17, 18, 0, 1, 2, 3, 4, 5, 16, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15];
            var first = lengths[0]; var second = lengths[^1]; var count = 4;
            for (var i = 0; i < order.Length; i++) if (order[i] == first || order[i] == second) count = Math.Max(count, i + 1);
            writer.Write(0, 1); writer.Write(count - 4, 4);
            for (var i = 0; i < count; i++) writer.Write(order[i] == first || order[i] == second ? 1 : 0, 3);
            writer.Write(0, 1); // Full alphabet; code-length tree has one or two leaves.
            if (first != second)
                foreach (var length in lengths) writer.Write(length == first ? 0 : 1, 1);
        }

        public void Symbol(WebpBitWriter writer, int symbol) => writer.Write(codes[symbol], lengths[symbol]);
        public int Bits(int symbol) => lengths[symbol];
        public void Symbol(WebpBitWriter writer, int symbol, int extra, int extraBits)
            => writer.Write(codes[symbol] | extra << lengths[symbol], lengths[symbol] + extraBits);
    }

    public static void Encode(ImageBuffer image, Stream output, WebpEncodeOptions options, OperationContext context)
    {
        if (options.Quality is < 1 or > 100 || options.Effort is < 0 or > 6) throw new ArgumentOutOfRangeException(nameof(options));
        context.PixelBytes(image.Width, image.Height, image.BitDepth);
        if (image.Width > 16384 || image.Height > 16384) throw new ImageLimitException(ImageLimitKind.Dimension, "VP8L dimensions exceed 16384.");
        context.ReserveBorrowed(image.ByteLength);
        var count = image.Width * image.Height; var reserved = count * 4L;
        context.Reserve(reserved);
        try
        {
            context.Work(0); var pixels = new uint[count];
            for (var i = 0; i < pixels.Length; i++) pixels[i] = Pixel(image, i, context);
            using var prepared = Vp8lPreparedEncoding.Prepare(pixels, image.Width, options.Effort, context);
            // Prepared tokens contain all literal values and copy distances.
            // Normalized pixels have no remaining users during output delivery.
            context.Release(reserved); reserved = 0;
            prepared.Write(output);
        }
        finally { if (reserved != 0) context.Release(reserved); }
    }

    private static uint Pixel(ImageBuffer image, int position, OperationContext context)
    {
        context.Work(4);
        var x = position % image.Width * 4; var y = position / image.Width;
        if (image.BitDepth == 8)
        {
            var row = image.GetRow8(y); return (uint)(row[x + 3] << 24 | row[x] << 16 | row[x + 1] << 8 | row[x + 2]);
        }
        var samples = image.GetRow16(y);
        return (uint)(((samples[x + 3] + 128) / 257) << 24 | ((samples[x] + 128) / 257) << 16
            | ((samples[x + 1] + 128) / 257) << 8 | ((samples[x + 2] + 128) / 257));
    }

    private static void Prefix(int value, out int code, out int extra, out int bits)
    {
        if (value <= 4) { code = value - 1; extra = 0; bits = 0; return; }
        for (code = 4; code < 24; code++)
        {
            bits = (code - 2) / 2; var start = ((2 + (code & 1)) << bits) + 1;
            if (value < start + (1 << bits)) { extra = value - start; return; }
        }
        throw new ArgumentOutOfRangeException(nameof(value));
    }

    internal sealed class CountStream(int maximum) : Stream
    {
        public int Count { get; private set; }
        public override bool CanRead => false;
        public override bool CanSeek => false;
        public override bool CanWrite => true;
        public override long Length => Count;
        public override long Position { get => Count; set => throw new NotSupportedException(); }
        public override void Flush() { }
        public override void Write(byte[] buffer, int offset, int count) => Write(buffer.AsSpan(offset, count));
        public override void Write(ReadOnlySpan<byte> buffer)
        {
            if (buffer.Length > maximum - Count) throw new ImageLimitException(ImageLimitKind.OutputBytes, "WebP encoded output limit exceeded.");
            Count += buffer.Length;
        }
        public override void WriteByte(byte value)
        {
            if (Count == maximum) throw new ImageLimitException(ImageLimitKind.OutputBytes, "WebP encoded output limit exceeded.");
            Count++;
        }
        public override int Read(byte[] buffer, int offset, int count) => throw new NotSupportedException();
        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
        public override void SetLength(long value) => throw new NotSupportedException();
    }
}
