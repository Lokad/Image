using System.Buffers.Binary;

namespace Lokad.Image.Internal;

/// <summary>Owned VP8L literal, repeated-color and previous-row encoder; no upstream code or tables.</summary>
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
        // Decide from this operation's configured local allowance, before any
        // shared reservation. Admission refusal never selects another encoder.
        if (options.Effort >= 3 && (long)image.Width * image.Height >= 16384
            && context.RemainingWorkingBytes >= CachedWorkingBytes)
        { EncodeCached(image, output, options, context); return; }
        context.Reserve(8192);
        var green = new Table(280); var literal = new Table(256); var distance = new Table(40);
        // Count first so nonseekable output needs no encoded-frame staging buffer.
        // The caller must synchronize mutable pixels across the two passes.
        using var counter = new CountStream(options.Limits.MaxOutputBytes);
        var alpha = Payload(image, counter, options.Effort, false, green, literal, distance, context,
            omitAlpha: true, singletonAlpha: false, out var literals);
        // Omitting eight alpha bits per literal preserves byte alignment. The
        // normal single-leaf alpha header below adds exactly 232 bits (29 bytes)
        // to the fixed header. Count is always a lower bound on the final size.
        var singletonAlpha = !alpha && literals > 29;
        var payloadLength = counter.Count + (singletonAlpha ? 29 : literals);
        if (payloadLength + 20L + (payloadLength & 1) > options.Limits.MaxOutputBytes)
            throw new ImageLimitException(ImageLimitKind.OutputBytes, "WebP encoded output limit exceeded.");
        Span<byte> header = stackalloc byte[20];
        "RIFF"u8.CopyTo(header); BinaryPrimitives.WriteInt32LittleEndian(header[4..], payloadLength + 12 + (payloadLength & 1));
        "WEBPVP8L"u8.CopyTo(header[8..]); BinaryPrimitives.WriteInt32LittleEndian(header[16..], payloadLength);
        output.Write(header);
        Payload(image, output, options.Effort, alpha, green, literal, distance, context,
            omitAlpha: false, singletonAlpha, out _);
        if ((payloadLength & 1) != 0) output.WriteByte(0);
    }

    private static bool Payload(ImageBuffer image, Stream output, int effort, bool alphaHint,
        Table green, Table literal, Table distance, OperationContext context,
        bool omitAlpha, bool singletonAlpha, out int literals)
    {
        Span<byte> header = stackalloc byte[5]; header[0] = 0x2F;
        BinaryPrimitives.WriteUInt32LittleEndian(header[1..], (uint)(image.Width - 1) | (uint)(image.Height - 1) << 14 | (alphaHint ? 1u << 28 : 0));
        output.Write(header);
        var writer = new WebpBitWriter(output, context);
        writer.Write(0, 1); writer.Write(0, 1); writer.Write(0, 1); // No transforms, cache or spatial groups.
        green.Header(writer); literal.Header(writer); literal.Header(writer);
        if (singletonAlpha) OpaqueAlphaHeader(writer); else literal.Header(writer);
        distance.Header(writer); literals = 0;
        var count = image.Width * image.Height; var position = 0; uint previous = 0; var alpha = false;
        while (position < count)
        {
            var pixel = Pixel(image, position, context); alpha |= pixel >> 24 != 255;
            var run = 1; var fromAbove = false;
            var maximum = Math.Min(64 << effort, count - position);
            if (effort != 0 && position != 0 && pixel == previous)
            {
                while (run < maximum && Pixel(image, position + run, context) == pixel) run++;
            }
            if (effort != 0 && position >= image.Width && run < maximum)
            {
                // RFC 9649 section 3.6.2.2: a reference may span rows and overlap
                // its destination. Read the resident source, with no row copy.
                var length = 0;
                while (length < maximum && Pixel(image, position + length, context)
                    == Pixel(image, position - image.Width + length, context)) length++;
                if (length > run) { run = length; fromAbove = true; }
            }
            if (run >= 2)
            {
                Prefix(run, out var code, out var extra, out var bits);
                green.Symbol(writer, 256 + code); writer.Write(extra, bits);
                // RFC 9649 section 3.6.2.2.1: code1 is one row above; code2 is
                // one pixel behind. Both use the existing distance alphabet.
                distance.Symbol(writer, fromAbove ? 0 : 1);
                position += run;
                if (fromAbove) pixel = Pixel(image, position - 1, context);
            }
            else
            {
                green.Symbol(writer, (int)(pixel >> 8 & 255)); literal.Symbol(writer, (int)(pixel >> 16 & 255));
                literal.Symbol(writer, (int)(pixel & 255));
                if (omitAlpha || singletonAlpha) writer.Write(0, 0); // Retain the alpha-field work/cancellation check.
                else literal.Symbol(writer, (int)(pixel >> 24));
                position++; literals++;
            }
            previous = pixel;
        }
        writer.Finish(); return alpha;
    }

    private static void OpaqueAlphaHeader(WebpBitWriter writer)
    {
        // RFC 9649 section 3.7.2.1.2: normal code-length tree with symbols0/1.
        // Only alpha255 has length1; using that single leaf consumes zero bits.
        writer.Write(0, 1); writer.Write(0, 4); // Four code-length symbols:17,18,0,1.
        writer.Write(0, 3); writer.Write(0, 3); writer.Write(1, 3); writer.Write(1, 3);
        writer.Write(0, 1); // Full256-entry alpha alphabet.
        for (var i = 0; i < 256; i++) writer.Write(i == 255 ? 1 : 0, 1);
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

    private sealed class CountStream(int maximum) : Stream
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
