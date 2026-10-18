using Lokad.Image.Tooling;

namespace Lokad.Image.DifferentialTests;

public sealed class DrawingFfmpegTests
{
    [Theory]
    [InlineData(8)] [InlineData(16)]
    public void ExactRegionCopyMatchesNativeCropIncludingAlphaAndHiddenRgb(int depth)
    {
        using var source = OriginalPixels.Buffer(9, 7, depth); using var canvas = ImageBuffer.Create(5, 3, depth);
        ImageDrawing.CopyRegion(source, 2, 1, 5, 3, canvas, 0, 0);
        var native = Filter(source, "crop=5:3:2:1:exact=1");
        Assert.Equal(OriginalPixels.Bytes(canvas), native);
    }

    [Theory]
    [InlineData(8)] [InlineData(16)]
    public void RectangleReplacementMatchesNativePlaneExpressions(int depth)
    {
        using var source = OriginalPixels.Buffer(9, 7, depth); using var canvas = source.Clone();
        var color = depth == 8 ? RgbaColor.From8(23, 57, 91, 128) : new RgbaColor(12345, 23456, 34567, 45678);
        ImageDrawing.FillRectangle(canvas, 2, 1, 4, 3, color);
        var scale = depth == 8 ? 257 : 1;
        const string inside = "between(X,2,5)*between(Y,1,3)";
        var planar = depth == 8 ? "gbrap" : "gbrap16le";
        var expression = $"format={planar},geq=r='if({inside},{color.R / scale},r(X,Y))':g='if({inside},{color.G / scale},g(X,Y))':b='if({inside},{color.B / scale},b(X,Y))':a='if({inside},{color.A / scale},alpha(X,Y))'";
        Assert.Equal(OriginalPixels.Bytes(canvas), Filter(source, expression));
    }

    [Theory]
    [InlineData(128)] [InlineData(255)]
    public void StraightAlphaOverOpaqueBlackMatchesNativePlaneCompositing(int alpha)
    {
        using var canvas = ImageBuffer.Create(9, 7, 8); using var source = ImageBuffer.Create(9, 7, 8);
        ImageDrawing.FillRectangle(canvas, 0, 0, 9, 7, RgbaColor.From8(0, 0, 0));
        ImageDrawing.FillRectangle(source, 0, 0, 9, 7, RgbaColor.From8(255, 0, 255, (byte)alpha));
        // Required filters are present in the pinned minimal build. Against
        // opaque black, source-over is premultiplication with opaque output.
        // The declared 0/255 colors make every native product integral; this
        // correctness reference is not a timing baseline or a rounding allowance.
        var native = Filter(source, "format=gbrap,geq=r='r(X,Y)*alpha(X,Y)/255':g='g(X,Y)*alpha(X,Y)/255':b='b(X,Y)*alpha(X,Y)/255':a=255,pad=11:8:2:1:color=black@1,crop=9:7:0:0:exact=1");
        ImageDrawing.Composite(canvas, source, 2, 1);
        Assert.Equal(OriginalPixels.Bytes(canvas), native);
    }

    private static byte[] Filter(ImageBuffer input, string filter)
    {
        var format = input.BitDepth == 8 ? "rgba" : "rgba64le";
        return OracleProcess.Run(OracleProcess.Ffmpeg,
            ["-hide_banner", "-loglevel", "error", "-filter_threads", "1", "-f", "rawvideo", "-pixel_format", format, "-video_size", "9x7", "-i", "pipe:0",
             "-vf", filter, "-frames:v", "1", "-threads:v", "1", "-pix_fmt", format, "-f", "rawvideo", "pipe:1"], OriginalPixels.Bytes(input));
    }
}
