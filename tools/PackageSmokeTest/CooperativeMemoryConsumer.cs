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
    }
}
