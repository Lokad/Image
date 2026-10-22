namespace Lokad.Image.Tooling;

internal static class CooperativeMemoryConsumer
{
    internal static async Task Verify()
    {
        using var budget = new TestMemoryBudget(4 * 1024 * 1024);
        var limits = new ImageLimits { MemoryBudget = budget };
        using (var input = ImageBuffer.Create(19, 13, 8, limits))
        {
            input.GetRow8(0)[0] = 123;
            using var encoded = new MemoryStream();
            await ImageCodec.EncodePngAsync(input, encoded, new() { Limits = limits });
            var info = ImageCodec.Identify(encoded.GetBuffer().AsSpan(0, checked((int)encoded.Length)), limits);
            if ((info.SourceColor, info.SourceAlpha, info.SourceComponentCount, info.BitDepth)
                != (SourceColorModel.Rgb, SourceAlphaRepresentation.Channel, (byte)4, 8))
                throw new Exception("Packaged encoded PNG source representation differs.");
            encoded.Position = 0;
            if (await ImageCodec.InspectHeadersAsync(encoded, limits) != info)
                throw new Exception("Packaged bounded source metadata differs.");
            encoded.Position = 0;
            using var decoded = await ImageCodec.DecodeAsync(encoded, new() { Limits = limits });
            if (budget.Bytes != 2 * 19 * 13 * 4 || decoded.GetRow8(0)[0] != 123 || !encoded.CanRead)
                throw new Exception("Cooperative packaged image/async ownership differs.");
            var font = TrueTypeFont.Load(AuthoredTrueType.Build(), limits);
            using var layout = TextLayout.Create(font, "AVg", new() { Limits = limits, FontSize = 10 });
            var before = budget.Bytes; var fontBytes = font.StorageBytes;
            font.Dispose(); font.Dispose();
            if (budget.Bytes != before - fontBytes) throw new Exception("Packaged font admission was not released.");
            ImageDrawing.DrawText(decoded, layout, 0, 0, RgbaColor.From8(12, 34, 56), limits);
        }
        if (budget.Bytes != 0 || budget.IsDisposed) throw new Exception("Packaged owners leaked or disposed the borrowed budget.");
        using var empty = new TestMemoryBudget(0);
        try
        {
            using var unexpected = ImageBuffer.Create(1, 1, 8, new() { MemoryBudget = empty });
            throw new Exception("Packaged cooperative refusal was ignored.");
        }
        catch (ImageLimitException e) when (e.Scope == ImageLimitScope.SharedBudget && e.Limit == ImageLimitKind.WorkingBytes) { }
        if (empty.Bytes != 0 || empty.IsDisposed) throw new Exception("Packaged refusal cleanup differs.");
        foreach (var depth in new[] { 8, 16 })
        {
            var source = ImageBuffer.Create(3, 2, depth, limits);
            for (var y = 0; y < source.Height; y++)
            for (var x = 0; x < source.Width; x++)
            for (var c = 0; c < 4; c++)
                if (depth == 8) source.GetRow8(y)[x * 4 + c] = (byte)(37 + c * 43);
                else source.GetRow16(y)[x * 4 + c] = (ushort)(12345 + c * 1234);
            using var resized = ImageTransforms.ResizeToFit(source, 7, 7,
                allowUpscaling: true, filter: ResizeFilter.Lanczos3, limits: limits);
            var sourceBytes = 3 * 2 * 4 * (depth / 8); var resultBytes = 7 * 5 * 4 * (depth / 8);
            if ((resized.Width, resized.Height, resized.BitDepth) != (7, 5, depth)
                || budget.Bytes != sourceBytes + resultBytes)
                throw new Exception("Packaged Lanczos3 fit/retained admission differs.");
            source.Dispose();
            if (budget.Bytes != resultBytes) throw new Exception("Packaged Lanczos3 lifetime depends on its source.");
            for (var y = 0; y < resized.Height; y++)
            for (var x = 0; x < resized.Width; x++)
            for (var c = 0; c < 4; c++)
                if ((depth == 8 ? resized.GetRow8(y)[x * 4 + c] : resized.GetRow16(y)[x * 4 + c])
                    != (depth == 8 ? 37 + c * 43 : 12345 + c * 1234))
                    throw new Exception("Packaged Lanczos3 samples differ.");
        }
        if (budget.Bytes != 0) throw new Exception("Packaged Lanczos3 retained storage was not released.");
        using (var input = ImageBuffer.Create(129, 129, 8, limits))
        {
            for (var y = 0; y < input.Height; y++)
            for (var x = 0; x < input.Width; x++)
            {
                var row = input.GetRow8(y); var at = x * 4;
                row[at] = (byte)(x * 37 + y * 11); row[at + 1] = (byte)(x * 19 + y * 29);
                row[at + 2] = (byte)(x * 7 + y * 53); row[at + 3] = x % 7 == 0 ? (byte)0 : (byte)255;
            }
            var sourceBytes = (long)input.Stride * input.Height;
            using (var caller = budget.CreateScope())
            {
                if (!caller.TryReserve(4 * 1024 * 1024 - sourceBytes - 8192, out var held))
                    throw new Exception("Packaged WebP refusal setup differs.");
                using var rejected = new MemoryStream();
                try
                {
                    ImageCodec.EncodeWebp(input, rejected, new() { Limits = limits });
                    throw new Exception("Packaged WebP silently selected a lower-memory encoder after shared refusal.");
                }
                catch (ImageLimitException e) when (e.Scope == ImageLimitScope.SharedBudget && e.Limit == ImageLimitKind.WorkingBytes) { }
                if (rejected.Length != 0 || budget.Bytes != sourceBytes + held.Bytes)
                    throw new Exception("Packaged WebP refusal leaked codec storage or wrote output.");
            }
            using var output = new MemoryStream(); await ImageCodec.EncodeWebpAsync(input, output, new() { Limits = limits });
            if (budget.Bytes != sourceBytes || !output.CanWrite || (output.GetBuffer()[25] & 7) != 5)
                throw new Exception("Packaged cached WebP admission, caller stream or transform framing differs.");
            using var decoded = ImageCodec.Decode(output.ToArray(), new() { Limits = limits });
            for (var y = 0; y < input.Height; y++)
                if (!input.GetRow8(y).SequenceEqual(decoded.GetRow8(y))) throw new Exception("Packaged cached WebP hidden RGBA differs.");
            input.Dispose();
            if (budget.Bytes != sourceBytes || decoded.GetRow8(0)[3] != 0)
                throw new Exception("Packaged cached WebP result depends on source lifetime.");
        }
        if (budget.Bytes != 0 || budget.IsDisposed) throw new Exception("Packaged cached WebP owners leaked or closed the borrowed root.");
        var rgb = new byte[17 * 11 * 3];
        for (var i = 0; i < rgb.Length; i++) rgb[i] = (byte)(i * 37 + i / 11);
        var jpegOptions = new JpegEncodeOptions { Quality = 69, Limits = limits };
        using var syncJpeg = new MemoryStream(); using var asyncJpeg = new MemoryStream();
        ImageCodec.EncodeJpeg(rgb, 17, 11, JpegSampleFormat.Rgb, syncJpeg,
            options: jpegOptions);
        await ImageCodec.EncodeJpegAsync(rgb, 17, 11, JpegSampleFormat.Rgb, asyncJpeg, options: jpegOptions);
        if (!syncJpeg.ToArray().AsSpan().SequenceEqual(asyncJpeg.ToArray())
            || budget.Bytes != 0 || !asyncJpeg.CanWrite || budget.IsDisposed)
            throw new Exception("Packaged caller JPEG bytes, async cleanup or borrowed ownership differs.");
        using (var decoded = ImageCodec.Decode(asyncJpeg.ToArray(), new() { Limits = limits }))
        {
            rgb.AsSpan().Clear();
            if ((decoded.Width, decoded.Height, decoded.BitDepth) != (17, 11, 8) || budget.Bytes != 17 * 11 * 4)
                throw new Exception("Packaged caller JPEG result lifetime or geometry differs.");
        }
        if (budget.Bytes != 0 || budget.IsDisposed) throw new Exception("Packaged caller JPEG storage leaked.");
    }
}
