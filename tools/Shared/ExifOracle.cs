namespace Lokad.Image.Tooling;

internal static class ExifOracle
{
    public static string OrientationFilter(ImageOrientation orientation) => orientation switch
    {
        ImageOrientation.Normal => "null", ImageOrientation.MirrorHorizontal => "hflip",
        ImageOrientation.Rotate180 => "hflip,vflip", ImageOrientation.MirrorVertical => "vflip",
        ImageOrientation.Transpose => "transpose=cclock_flip", ImageOrientation.Rotate90 => "transpose=clock",
        ImageOrientation.Transverse => "transpose=clock_flip", ImageOrientation.Rotate270 => "transpose=cclock",
        _ => throw new ArgumentOutOfRangeException(nameof(orientation))
    };

    // Color/chroma conversion precedes geometry; rotating subsampled YUV first
    // changes the original odd-dimension chroma grid. Native EXIF dispatch is
    // deliberately disabled and the explicit transform is prepared outside timing.
    public static string Filter(MetadataFixture fixture, ImageOrientation orientation)
    {
        var conversion = fixture.Jpeg ? JpegOracle.Filter(fixture.Width, fixture.Height, JpegSampling.YCbCr420)
            : fixture.Profile == MetadataProfile.WebpLossyAlpha ? WebpOracle.Filter(fixture.Width, fixture.Height, alpha: true) : "null";
        return conversion + ",format=" + (fixture.Depth == 16 ? "rgba64le" : "rgba") + "," + OrientationFilter(orientation);
    }

    public static byte[] Decode(byte[] encoded, MetadataFixture fixture, ImageOrientation orientation, string? executable = null)
        => OracleProcess.Run(executable ?? OracleProcess.Ffmpeg,
            ["-hide_banner", "-loglevel", "error", "-threads:v", "1", "-filter_complex_threads", "1", "-noautorotate",
             "-f", "image2pipe", "-i", "pipe:0", "-filter_complex", Filter(fixture, orientation), "-frames:v", "1", "-threads:v", "1",
             "-pix_fmt", fixture.Depth == 16 ? "rgba64le" : "rgba", "-f", "rawvideo", "pipe:1"], encoded);

    public static (int Maximum, double RgbRmse) Check(byte[] expected, byte[] actual, bool lossy)
    {
        if (actual.Length != expected.Length) throw new InvalidOperationException("Oriented raster size or precision differs.");
        if (!lossy)
        {
            if (!actual.AsSpan().SequenceEqual(expected)) throw new InvalidOperationException("Lossless oriented pixels differ, including alpha/hidden RGB.");
            return (0, 0);
        }
        var maximum = 0; double squared = 0;
        for (var i = 0; i < actual.Length; i++)
        {
            if (i % 4 == 3)
            {
                if (expected[i] != actual[i]) throw new InvalidOperationException("Oriented alpha differs.");
                continue;
            }
            var error = Math.Abs(expected[i] - actual[i]); maximum = Math.Max(maximum, error); squared += error * error;
        }
        var rmse = Math.Sqrt(squared / (actual.Length / 4 * 3));
        if (maximum > 4 || rmse > 1.5) throw new InvalidOperationException($"Oriented RGB differs: maximum {maximum}, RMSE {rmse}.");
        return (maximum, rmse);
    }
}
