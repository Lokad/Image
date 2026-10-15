using System.IO.Compression;

namespace Lokad.Image.Internal;

// Buffered BCL output is speculative: a future compressed-data error must not
// precede the current row's filter, palette or work checks. On that error, reopen
// the same owned input, skip only bytes already delivered and resume unbuffered.
internal sealed class PngInflatedReader : IDisposable
{
    internal const int BufferBytes = 8192;
    private readonly byte[] compressed;
    private readonly int length;
    private ZLibStream? inflater;
    private readonly byte[]? materialized;
    private byte[]? buffer;
    private int position, count;
    private long consumed;

    public PngInflatedReader(byte[] compressed, int length, byte[]? materialized = null)
    {
        this.compressed = compressed; this.length = length; this.materialized = materialized;
        if (materialized is null) inflater = Open();
    }

    private ZLibStream Open() => new(new MemoryStream(compressed, 0, length, false), CompressionMode.Decompress);
    public void EnableBuffer() { if (materialized is null) buffer = new byte[BufferBytes]; }

    public int ReadByte()
    {
        if (materialized is not null) return position == materialized.Length ? -1 : materialized[position++];
        if (buffer is not null)
        {
            if (position == count)
            {
                var filled = Fill();
                if (filled == 0) return -1;
                if (filled < 0) return ReadByte();
            }
            consumed++; return buffer![position++];
        }
        var value = inflater!.ReadByte(); if (value >= 0) consumed++; return value;
    }

    public int Read(Span<byte> destination)
    {
        if (materialized is not null)
        {
            var size = Math.Min(destination.Length, materialized.Length - position);
            materialized.AsSpan(position, size).CopyTo(destination); position += size; return size;
        }
        if (buffer is not null)
        {
            if (position == count)
            {
                var filled = Fill();
                if (filled == 0) return 0;
                if (filled < 0) return Read(destination);
            }
            var size = Math.Min(destination.Length, count - position);
            buffer!.AsSpan(position, size).CopyTo(destination); position += size; consumed += size; return size;
        }
        var read = inflater!.Read(destination); consumed += read; return read;
    }

    private int Fill()
    {
        try { position = 0; return count = inflater!.Read(buffer); }
        catch (InvalidDataException)
        {
            var scratch = buffer!; buffer = null; position = count = 0;
            inflater!.Dispose(); inflater = Open();
            var remaining = consumed;
            while (remaining > 0)
            {
                var read = inflater.Read(scratch.AsSpan(0, (int)Math.Min(remaining, scratch.Length)));
                if (read == 0) throw new ImageDataException("Truncated PNG pixel data.");
                remaining -= read;
            }
            return -1;
        }
    }

    public void Dispose() => inflater?.Dispose();
}
