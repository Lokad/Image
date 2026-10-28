using System.Buffers.Binary;
using System.IO.Compression;
using Lokad.Image;

namespace Lokad.Image.Tooling;

// Small, original RGBA8/16 test oracle: BCL Zlib plus W3C PNG byte predictors.
// No Lokad.Image codec helpers are used. It retains two rows, not a decoded raster.
internal static class IndependentRgbaPng
{
    internal static void Verify(byte[] png, ImageBuffer expected)
    {
        if (!png.AsSpan(0, 8).SequenceEqual(new byte[] {137,80,78,71,13,10,26,10})
            || BinaryPrimitives.ReadInt32BigEndian(png.AsSpan(16)) != expected.Width
            || BinaryPrimitives.ReadInt32BigEndian(png.AsSpan(20)) != expected.Height
            || png[24] != expected.BitDepth || png[25] != 6 || png[28] != 0)
            throw new InvalidOperationException("PNG source descriptor differs.");
        using var compressed = new MemoryStream();
        for (var offset = 8; offset < png.Length;)
        {
            var length = BinaryPrimitives.ReadInt32BigEndian(png.AsSpan(offset));
            if (png.AsSpan(offset + 4, 4).SequenceEqual("IDAT"u8)) compressed.Write(png.AsSpan(offset + 8, length));
            offset = checked(offset + length + 12);
        }
        compressed.Position = 0; using var inflater = new ZLibStream(compressed, CompressionMode.Decompress);
        var stride = expected.BitDepth / 2; var row = new byte[expected.Width * stride]; var previous = new byte[row.Length];
        for (var y = 0; y < expected.Height; y++)
        {
            var filter = inflater.ReadByte(); if (filter is < 0 or > 4) throw new InvalidOperationException("Invalid PNG row filter.");
            inflater.ReadExactly(row);
            for (var i = 0; i < row.Length; i++)
            {
                var left = i < stride ? 0 : row[i - stride]; var above = previous[i]; var corner = i < stride ? 0 : previous[i - stride];
                var p = left + above - corner; var a = Math.Abs(p - left); var b = Math.Abs(p - above); var c = Math.Abs(p - corner);
                var predictor = filter switch {0 => 0,1 => left,2 => above,3 => (left + above) / 2,_ => a <= b && a <= c ? left : b <= c ? above : corner};
                row[i] = unchecked((byte)(row[i] + predictor));
            }
            if (expected.BitDepth == 8)
            {
                if (!row.AsSpan().SequenceEqual(expected.GetRow8(y))) throw new InvalidOperationException("PNG byte samples differ.");
            }
            else
            {
                var samples = expected.GetRow16(y);
                for (var i = 0; i < samples.Length; i++)
                    if (BinaryPrimitives.ReadUInt16BigEndian(row.AsSpan(2 * i)) != samples[i]) throw new InvalidOperationException("PNG word samples differ.");
            }
            (row, previous) = (previous, row);
        }
        if (inflater.ReadByte() != -1) throw new InvalidOperationException("PNG contains excess pixel bytes.");
    }
}
