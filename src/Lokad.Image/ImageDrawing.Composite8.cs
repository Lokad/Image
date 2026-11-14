using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace Lokad.Image;

public static partial class ImageDrawing
{
    private static void CompositeRows8(ImageBuffer canvas, ImageBuffer source, Region region,
        bool backwardsX, bool backwardsY, CancellationToken cancellationToken)
    {
        var sourcePixels = source.Pixels; var canvasPixels = canvas.Pixels;
        for (var row = 0; row < region.Height; row++)
        {
            var offsetY = backwardsY ? region.Height - row - 1 : row;
            var sourceRow = sourcePixels.Slice((region.SourceY + offsetY) * source.Stride + region.SourceX * 4, region.Width * 4);
            var canvasRow = canvasPixels.Slice((region.DestinationY + offsetY) * canvas.Stride + region.DestinationX * 4, region.Width * 4);
            for (var first = 0; first < region.Width;)
            {
                var end = first + Math.Min(256, region.Width - first);
                cancellationToken.ThrowIfCancellationRequested();
                var i = first;
                for (; i <= end - 4; i += 4)
                {
                    var at = (backwardsX ? region.Width - i - 4 : i) * 4;
                    // Load both groups before storing: directional traversal
                    // retains snapshot semantics even for one-pixel overlap.
                    var s = Vector128.LoadUnsafe(ref MemoryMarshal.GetReference(sourceRow), (nuint)at);
                    var d = Vector128.LoadUnsafe(ref MemoryMarshal.GetReference(canvasRow), (nuint)at);
                    CompositeFour8(s, d).StoreUnsafe(ref MemoryMarshal.GetReference(canvasRow), (nuint)at);
                }
                for (; i < end; i++)
                {
                    var at = (backwardsX ? region.Width - i - 1 : i) * 4;
                    var sa = sourceRow[at + 3]; if (sa == 0) continue;
                    var sw = sa * 255; var dw = canvasRow[at + 3] * (255 - sa);
                    var coverage = sw + dw;
                    for (var c = 0; c < 3; c++)
                        canvasRow[at + c] = (byte)((sourceRow[at + c] * sw + canvasRow[at + c] * dw + coverage / 2) / coverage);
                    canvasRow[at + 3] = (byte)((coverage + 127) / 255);
                }
                first = end;
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static Vector128<byte> CompositeFour8(Vector128<byte> source, Vector128<byte> canvas)
    {
        var s = Avx512F.ConvertToVector512Int32(source);
        var d = Avx512F.ConvertToVector512Int32(canvas);
        var sa = Avx512F.Shuffle(s, 0xff); var da = Avx512F.Shuffle(d, 0xff);
        var maximum = Vector512.Create(255);
        var sw = sa * maximum; var dw = da * (maximum - sa);
        var coverage = sw + dw;
        var numerator = s * sw + d * dw + (coverage >> 1);
        // Only zero-source-alpha lanes can have zero coverage. They are
        // discarded below; a denominator of one avoids 0/0 conversion.
        var color = DivideByteColor(numerator, Vector512.Max(coverage, Vector512.Create(1)));
        var biased = coverage + Vector512.Create(128);
        var alpha = (biased + (biased >> 8)) >> 8;
        var alphaLanes = Vector512.Create(0, 0, 0, -1, 0, 0, 0, -1, 0, 0, 0, -1, 0, 0, 0, -1);
        var result = Vector512.ConditionalSelect(alphaLanes, alpha, color);
        result = Vector512.ConditionalSelect(Vector512.Equals(sa, Vector512<int>.Zero), d, result);
        return Avx512F.ConvertToVector128Byte(result);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static Vector512<int> DivideByteColor(Vector512<int> roundedNumerator, Vector512<int> denominator)
    {
        // Exact integer-to-float inputs: d<=65025 and n<=255*d+floor(d/2)
        // <=16613887<2^24. The quotient is <=255.5. Its float ULP is at most
        // 2^-16, less than 1/65025, the smallest nonzero rational distance
        // to an integer. Correctly rounded division followed by truncation
        // therefore equals integer division, including exact quotients.
        return Avx512F.ConvertToVector512Int32WithTruncation(Avx512F.Divide(
            Avx512F.ConvertToVector512Single(roundedNumerator), Avx512F.ConvertToVector512Single(denominator)));
    }
}
