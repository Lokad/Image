namespace Lokad.Image.Internal;

/// <summary>Safe managed VP8L encoding with admitted transforms, references and entropy coding.</summary>
internal static class WebpLosslessEncoder
{
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
            context.Cancellation.ThrowIfCancellationRequested(); var pixels = new uint[count];
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
        context.Cancellation.ThrowIfCancellationRequested();
        var x = position % image.Width * 4; var y = position / image.Width;
        if (image.BitDepth == 8)
        {
            var row = image.GetRow8(y); return (uint)(row[x + 3] << 24 | row[x] << 16 | row[x + 1] << 8 | row[x + 2]);
        }
        var samples = image.GetRow16(y);
        return (uint)(((samples[x + 3] + 128) / 257) << 24 | ((samples[x] + 128) / 257) << 16
            | ((samples[x + 1] + 128) / 257) << 8 | ((samples[x + 2] + 128) / 257));
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
