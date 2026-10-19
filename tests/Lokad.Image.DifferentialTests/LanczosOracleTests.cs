using System.Buffers.Binary;
using Lokad.Image.Tooling;

namespace Lokad.Image.DifferentialTests;

public sealed class LanczosOracleTests
{
    [Theory]
    [InlineData(8, false)] [InlineData(8, true)]
    [InlineData(16, false)] [InlineData(16, true)]
    public void OpaqueSamplesMatchExplicitRadiusThreeAndClampedEdges(int depth, bool enlarge)
    {
            const int sw = 129, sh = 67; var dw = enlarge ? 257 : 64; var dh = enlarge ? 135 : 33;
            var planar = depth == 16 ? "gbrap16le" : "gbrap"; var packed = depth == 16 ? "rgba64le" : "rgba";
            // zscale mirrors at its image boundary. Extending the source and
            // proportionally enlarging the output preserves centers/scale while
            // putting that boundary beyond the kernel. Correctness only: the
            // surplus border output makes this unsuitable for warm timing.
            // Transparent references also need unclipped premultiplied float
            // output before unpremultiplication; this opaque gate claims neither.
            var filter = $"format={planar},zscale=w={dw * 3}:h={dh * 3}:filter=lanczos:param_a=3:rangein=full:range=full:matrixin=gbr:matrix=gbr:dither=none,"
                + $"crop={dw}:{dh}:{dw}:{dh}:exact=1,format={planar},format={packed}";
            using var source = OriginalPixels.Buffer(sw, sh, depth, opaque: true);
            var input = OriginalPixels.Bytes(source);
            var pixelBytes = depth / 8 * 4; var padded = new byte[sw * sh * 9 * pixelBytes];
            for (var y = 0; y < sh * 3; y++)
            for (var x = 0; x < sw * 3; x++)
                input.AsSpan((Math.Clamp(y - sh, 0, sh - 1) * sw + Math.Clamp(x - sw, 0, sw - 1)) * pixelBytes, pixelBytes)
                    .CopyTo(padded.AsSpan((y * sw * 3 + x) * pixelBytes));
            using var result = ImageTransforms.Resize(source, dw, dh, ResizeFilter.Lanczos3);
            var actual = OriginalPixels.Bytes(result);
            var native = OracleProcess.Run(OracleProcess.Ffmpeg,
                ["-hide_banner", "-loglevel", "error", "-threads:v", "1", "-f", "rawvideo", "-pixel_format", packed,
                    "-video_size", $"{sw * 3}x{sh * 3}", "-i", "pipe:0", "-filter_threads", "1", "-vf", filter,
                    "-frames:v", "1", "-threads:v", "1", "-pix_fmt", packed, "-f", "rawvideo", "pipe:1"], padded);
            Assert.Equal(actual.Length, native.Length);
            int Read(byte[] bytes, int sample) => depth == 8 ? bytes[sample]
                : BinaryPrimitives.ReadUInt16LittleEndian(bytes.AsSpan(sample * 2, 2));
            long rgbSquared = 0; var rgbMax = 0;
            for (var i = 0; i < dw * dh * 4; i++)
            {
                var difference = Math.Abs(Read(actual, i) - Read(native, i));
                if (i % 4 == 3) Assert.Equal(0, difference);
                else { rgbMax = Math.Max(rgbMax, difference); rgbSquared += (long)difference * difference; }
            }
            var rmse = Math.Sqrt(rgbSquared / (dw * dh * 3d));
            Assert.True(rgbMax <= 1 && rmse <= .25, $"Lanczos3/{depth}/{enlarge}: RGB maximum {rgbMax}, RMSE {rmse}.");
            Assert.Equal(input, OriginalPixels.Bytes(source));
    }
}
