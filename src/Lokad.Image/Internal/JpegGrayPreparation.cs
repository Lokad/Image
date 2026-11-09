using System.Buffers.Binary;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace Lokad.Image.Internal;

// Borrowed image/RGB samples to one gray8 block. The outer grayscale pass
// validates geometry and admits its8KiB fixed workspace. Row scratch
// stays bounded at32packedRGB bytes or24callerRGB bytes, with no owned storage.
internal static class JpegGrayPreparation
{
    internal const int TileWidth = 64, TileBytes = TileWidth * 8;

    private static byte Gray(int r, int g, int b)
    {
        // Keep separate binary64 products/additions and nearest rounding at
        // the pinned RGB-to-gray boundary; do not round an intermediate luma.
        var luma = (0.299 * r + 0.587 * g) + 0.114 * b;
        return (byte)Math.Floor(luma + 0.5);
    }

    // Each lane follows Gray's separate binary64 operations, without FMA or
    // intermediate rounding. Packed samples have R in the low byte.
    internal static void GrayRow(ReadOnlySpan<int> pixels, Span<byte> output)
    {
        output = output[..pixels.Length]; var at = 0;
        if (Avx2.IsSupported)
        {
            var mask = Vector256.Create(255);
            var redWeight = Vector256.Create(0.299); var greenWeight = Vector256.Create(0.587);
            var blueWeight = Vector256.Create(0.114); var half = Vector256.Create(0.5);
            for (; at <= pixels.Length - 8; at += 8)
            {
                var packed = Vector256.Create(pixels.Slice(at, 8));
                var r = Avx2.And(packed, mask);
                var g = Avx2.And(Avx2.ShiftRightLogical(packed.AsUInt32(), 8).AsInt32(), mask);
                var b = Avx2.And(Avx2.ShiftRightLogical(packed.AsUInt32(), 16).AsInt32(), mask);
                var low = Avx.Add(Avx.Add(Avx.Multiply(Avx.ConvertToVector256Double(r.GetLower()), redWeight),
                    Avx.Multiply(Avx.ConvertToVector256Double(g.GetLower()), greenWeight)),
                    Avx.Multiply(Avx.ConvertToVector256Double(b.GetLower()), blueWeight));
                var high = Avx.Add(Avx.Add(Avx.Multiply(Avx.ConvertToVector256Double(r.GetUpper()), redWeight),
                    Avx.Multiply(Avx.ConvertToVector256Double(g.GetUpper()), greenWeight)),
                    Avx.Multiply(Avx.ConvertToVector256Double(b.GetUpper()), blueWeight));
                var first = Avx.ConvertToVector128Int32WithTruncation(Avx.Floor(Avx.Add(low, half)));
                var second = Avx.ConvertToVector128Int32WithTruncation(Avx.Floor(Avx.Add(high, half)));
                var words = Sse2.PackSignedSaturate(first, second);
                var bytes = Sse2.PackUnsignedSaturate(words, words);
                BinaryPrimitives.WriteUInt64LittleEndian(output[at..], bytes.AsUInt64().GetElement(0));
            }
        }
        for (; at < pixels.Length; at++)
        {
            var rgb = pixels[at]; output[at] = Gray(rgb & 255, rgb >> 8 & 255, rgb >> 16 & 255);
        }
    }

    // The recorded image pass owns this bounded tile and RGB scratch inside its
    // existing fixed workspace. Every reader finishes before the next tile.
    internal static int PrepareTile(ImageBuffer image, int left, int top, uint? background,
        Span<byte> output, Span<int> packed, OperationContext context)
    {
        if (output.Length != TileBytes || packed.Length < TileWidth)
            throw new ArgumentException("Complete grayscale tile and RGB scratch required.");
        var count = Math.Min(TileWidth, image.Width - left);
        packed = packed[..count];
        for (var y = 0; y < 8; y++)
        {
            context.Cancellation.ThrowIfCancellationRequested();
            var sourceY = Math.Min(top + y, image.Height - 1);
            if (image.BitDepth == 8)
                JpegReferenceColor.NormalizeRgba8(image.GetRow8(sourceY).Slice(left * 4, count * 4), packed, background);
            else
                JpegReferenceColor.NormalizeRgba16(image.GetRow16(sourceY).Slice(left * 4, count * 4), packed, background);
            var row = output.Slice(y * TileWidth, TileWidth);
            GrayRow(packed, row);
            row[count..].Fill(row[count - 1]);
        }
        return count;
    }

    internal static void CopyBlock(ImageBuffer? image, JpegSampleInput source, int width, int height,
        int left, int top, uint? background, Span<byte> output)
    {
        if (output.Length != 64) throw new ArgumentException("Complete8x8 grayscale block required.", nameof(output));
        var count = Math.Min(8, width - left);
        if (image is null)
        {
            Span<byte> row = stackalloc byte[24];
            for (var y = 0; y < 8; y++)
            {
                source.CopyRgbRow(left, Math.Min(top + y, height - 1), row[..(count * 3)]);
                for (var x = 0; x < 8; x++)
                {
                    var at = Math.Min(x, count - 1) * 3;
                    output[y * 8 + x] = Gray(row[at], row[at + 1], row[at + 2]);
                }
            }
            return;
        }
        Span<int> packed = stackalloc int[8];
        for (var y = 0; y < 8; y++)
        {
            var sourceY = Math.Min(top + y, height - 1);
            if (image.BitDepth == 8)
                JpegReferenceColor.NormalizeRgba8(image.GetRow8(sourceY).Slice(left * 4, count * 4), packed[..count], background);
            else
                JpegReferenceColor.NormalizeRgba16(image.GetRow16(sourceY).Slice(left * 4, count * 4), packed[..count], background);
            var destination = output.Slice(y * 8, 8);
            GrayRow(packed[..count], destination);
            destination[count..].Fill(destination[count - 1]);
        }
    }
}
