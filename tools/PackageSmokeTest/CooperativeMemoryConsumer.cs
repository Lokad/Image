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
            if (budget.Bytes != sourceBytes || !output.CanWrite)
                throw new Exception("Packaged WebP admission or caller stream differs.");
            using var decoded = ImageCodec.Decode(output.ToArray(), new() { Limits = limits });
            for (var y = 0; y < input.Height; y++)
                if (!input.GetRow8(y).SequenceEqual(decoded.GetRow8(y))) throw new Exception("Packaged cached WebP hidden RGBA differs.");
            input.Dispose();
            if (budget.Bytes != sourceBytes || decoded.GetRow8(0)[3] != 0)
                throw new Exception("Packaged cached WebP result depends on source lifetime.");
        }
        if (budget.Bytes != 0 || budget.IsDisposed) throw new Exception("Packaged cached WebP owners leaked or closed the borrowed root.");
        using (var alphaSource = OriginalPixels.Buffer(513, 259, 8))
        using (var alphaPacket = new MemoryStream())
        using (var alphaBudget = new TestMemoryBudget(2L * 513 * 259 * 4))
        {
            alphaSource.GetRow8(0)[3] = 0;
            ImageCodec.EncodeWebp(alphaSource, alphaPacket, new() { Lossless = false, Quality = 85, Effort = 4 });
            var alphaBytes = alphaPacket.ToArray();
            var alphaLimits = new ImageLimits { MemoryBudget = alphaBudget,
                MaxWorkingBytes = alphaBytes.Length + 2L * 513 * 259 * 4 };
            using (var decoded = ImageCodec.Decode(alphaBytes, new() { Limits = alphaLimits }))
            {
                if (alphaBudget.Bytes != 513L * 259 * 4)
                    throw new Exception("Packaged compressed alpha retained storage differs.");
                for (var y = 0; y < 259; y++)
                for (var x = 0; x < 513; x++)
                    if (decoded.GetRow8(y)[x * 4 + 3] != alphaSource.GetRow8(y)[x * 4 + 3])
                        throw new Exception("Packaged compressed alpha samples differ.");
            }
            if (alphaBudget.Bytes != 0 || alphaBudget.IsDisposed)
                throw new Exception("Packaged compressed alpha leaked storage or disposed the borrowed root.");
        }
        Console.WriteLine("Packaged compressed alpha fits two RGBA frames.");
        {
            const int losslessFrameBytes = 257 * 33 * 4;
            var longGreen = AuthoredWebp.LongGreenCacheRaster();
            using var losslessBudget = new TestMemoryBudget(losslessFrameBytes + 16 * 1024);
            using (var image = ImageCodec.Decode(longGreen, new DecodeOptions
                { Limits = new ImageLimits { MemoryBudget = losslessBudget } }))
            {
                if (losslessBudget.Bytes != losslessFrameBytes)
                    throw new Exception("Packaged lossless result storage lifetime differs.");
                for (var y = 0; y < image.Height; y++)
                    if (image.GetRow8(y).IndexOfAnyExcept((byte)0) >= 0)
                        throw new Exception("Packaged lossless long-prefix/cache pixels differ.");
            }
            if (losslessBudget.Bytes != 0 || losslessBudget.IsDisposed)
                throw new Exception("Packaged lossless result leaked storage or disposed the borrowed root.");
        }
        Console.WriteLine("Packaged lossless output fits one frame plus bounded parser storage.");
        using (var pattern = OriginalPixels.Buffer(513, 259, 16))
        {
            const int outputBytes = 256 * 129 * 8;
            using var expected = ImageTransforms.Resize(pattern, 256, 129, ResizeFilter.Bilinear,
                new ImageLimits { MaxWorkingBytes = 513 * 259 * 8 + outputBytes + 4096 });
            var accelerated = System.Runtime.Intrinsics.X86.Avx512F.IsSupported
                && System.Runtime.Intrinsics.X86.Avx2.IsSupported && System.Runtime.Intrinsics.X86.Avx.IsSupported
                && System.Runtime.Intrinsics.X86.Sse41.IsSupported;
            using var bilinearBudget = new TestMemoryBudget(outputBytes + (accelerated ? 24 : 40) * 1024);
            using (var actual = ImageTransforms.Resize(pattern, 256, 129, ResizeFilter.Bilinear,
                new ImageLimits { MemoryBudget = bilinearBudget }))
            {
                if (bilinearBudget.Bytes != outputBytes)
                    throw new Exception("Packaged bilinear preparation retained temporary storage.");
                for (var y = 0; y < actual.Height; y++)
                    if (!actual.GetRow16(y).SequenceEqual(expected.GetRow16(y)))
                        throw new Exception("Packaged bilinear word samples differ.");
            }
            if (bilinearBudget.Bytes != 0 || bilinearBudget.IsDisposed)
                throw new Exception("Packaged bilinear preparation leaked storage or disposed the borrowed root.");
        }
        Console.WriteLine("Packaged bilinear word preparation pixels and admission passed.");
        {
            const int sourceWidth = 5, sourceHeight = 9, width = 11, height = 29;
            const long sourceBytes = sourceWidth * sourceHeight * 4, outputBytes = width * height * 4;
            const long planBytes = width * 100 + 192;
            using var byteBudget = new TestMemoryBudget(sourceBytes + outputBytes + planBytes);
            var byteLimits = new ImageLimits { MemoryBudget = byteBudget,
                MaxWorkingBytes = sourceBytes + outputBytes + planBytes + 4096 };
            using var pattern = ImageBuffer.Create(sourceWidth, sourceHeight, 8, byteLimits);
            for (var y = 0; y < sourceHeight; y++)
            for (var x = 0; x < sourceWidth; x++)
            {
                var row = pattern.GetRow8(y); var at = x * 4;
                row[at] = (byte)(x * 47 + y * 17); row[at + 1] = (byte)(x * 19 + y * 29);
                row[at + 2] = (byte)(x * 31 + y * 11); row[at + 3] = (byte)(64 + (x * 37 + y * 13) % 192);
            }
            using var expected = ImageTransforms.Resize(pattern, width, height, ResizeFilter.Bilinear,
                new ImageLimits { MaxWorkingBytes = sourceBytes + outputBytes + 4096 });
            using var actual = ImageTransforms.Resize(pattern, width, height, ResizeFilter.Bilinear, byteLimits);
            if (byteBudget.Bytes != sourceBytes + outputBytes || byteBudget.Peak != sourceBytes + outputBytes + planBytes)
                throw new Exception("Packaged byte bilinear exceeded existing admitted geometry or retained scratch.");
            pattern.Dispose();
            for (var y = 0; y < height; y++)
                if (!actual.GetRow8(y).SequenceEqual(expected.GetRow8(y)))
                    throw new Exception("Packaged byte bilinear row reuse changed exact pixels or source independence.");
            if (byteBudget.Bytes != outputBytes) throw new Exception("Packaged byte bilinear source ownership differs.");
            actual.Dispose();
            if (byteBudget.Bytes != 0 || byteBudget.IsDisposed)
                throw new Exception("Packaged byte bilinear leaked storage or disposed the caller root.");
        }
        Console.WriteLine("Packaged byte bilinear row reuse pixels and existing admission passed.");
        foreach (var (width, height, expectedHash) in new[]
        {
            (1027, 519, "08cde71ddb8b20be884c7ba812cec349f9302b21a348b79d3614bfda19805b94"),
            (256, 129, "656bb2248362c88dff8fe510dcc21055155976a3dd2b33aa20c48d2b036f59a8")
        })
        {
            const int sourceWidth = 513, sourceHeight = 259;
            const long sourceBytes = sourceWidth * sourceHeight * 4;
            var outputBytes = (long)width * height * 4;
            using var seed = OriginalPixels.Buffer(sourceWidth, sourceHeight, 8);
            seed.GetRow8(0)[3] = seed.GetRow8(0)[sourceWidth * 4 - 1] = 0;
            using var bicubicBudget = new TestMemoryBudget(sourceBytes + outputBytes + 4096);
            var bicubicLimits = new ImageLimits { MemoryBudget = bicubicBudget,
                MaxWorkingBytes = sourceBytes + outputBytes + 4096 };
            using var pattern = ImageBuffer.Create(sourceWidth, sourceHeight, 8, bicubicLimits);
            for (var y = 0; y < sourceHeight; y++) seed.GetRow8(y).CopyTo(pattern.GetRow8(y));
            using var actual = ImageTransforms.Resize(pattern, width, height, ResizeFilter.Bicubic, bicubicLimits);
            if (bicubicBudget.Bytes != sourceBytes + outputBytes || bicubicBudget.Peak > sourceBytes + outputBytes + 4096
                || ((width == 1027 || System.Runtime.Intrinsics.X86.Avx512F.IsSupported)
                    && bicubicBudget.Peak != sourceBytes + outputBytes + 4096))
                throw new Exception("Packaged byte bicubic exceeded admitted scratch or retained temporary storage.");
            pattern.Dispose();
            var digest = Convert.ToHexStringLower(System.Security.Cryptography.SHA256.HashData(OriginalPixels.Bytes(actual)));
            if (digest != expectedHash
                || bicubicBudget.Bytes != outputBytes)
                throw new Exception("Packaged byte bicubic pixels or independent retained ownership differ.");
            actual.Dispose();
            if (bicubicBudget.Bytes != 0 || bicubicBudget.IsDisposed)
                throw new Exception("Packaged byte bicubic leaked storage or disposed the caller root.");
        }
        Console.WriteLine("Packaged tight byte bicubic exact pixels and admission passed.");
        foreach (var depth in new[] { 8, 16 })
        foreach (var filter in new[] { ResizeFilter.Bilinear, ResizeFilter.Bicubic })
        {
            using var alphaRoot = new TestMemoryBudget((2 + 5) * 4 * (depth / 8) + 4096);
            var alphaLimits = new ImageLimits { MemoryBudget = alphaRoot,
                MaxWorkingBytes = (2 + 5) * 4 * (depth / 8) + 4096 };
            using var step = ImageBuffer.Create(1, 2, depth, alphaLimits);
            if (depth == 8) step.GetRow8(0)[3] = 1;
            else step.GetRow16(0)[3] = 1;
            using var result = ImageTransforms.Resize(step, 1, 5, filter, alphaLimits);
            int[] expectedAlpha = [1, 1, 1, 0, 0];
            for (var y = 0; y < 5; y++)
                if ((depth == 8 ? result.GetRow8(y)[3] : result.GetRow16(y)[3]) != expectedAlpha[y])
                    throw new Exception("Packaged exact alpha half or negative-lobe clamp differs.");
            step.Dispose();
            if (alphaRoot.Bytes != 5 * 4 * (depth / 8))
                throw new Exception("Packaged exact alpha retained-owner charge differs.");
            result.Dispose();
            if (alphaRoot.Bytes != 0 || alphaRoot.IsDisposed)
                throw new Exception("Packaged exact alpha leaked storage or disposed the borrowed root.");
        }
        Console.WriteLine("Packaged exact alpha halves, depths, filters and ownership passed.");
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
        using (var probeBudget = new TestMemoryBudget(512))
        {
            var probeOptions = new ImageProbeOptions
                { Limits = new ImageLimits { MemoryBudget = probeBudget, MaxWorkingBytes = 512 } };
            foreach (var asynchronous in new[] { false, true })
            {
                using var jpegInput = new MemoryStream(syncJpeg.ToArray(), writable: false);
                var dimensions = asynchronous ? await ImageCodec.ProbeAsync(jpegInput, probeOptions)
                    : ImageCodec.Probe(jpegInput, probeOptions);
                if ((dimensions.Width, dimensions.Height) != (17, 11)
                    || dimensions.BytesRead <= 0 || dimensions.BytesRead > dimensions.BytesTraversed
                    || dimensions.BytesTraversed != jpegInput.Position
                    || jpegInput.Position >= jpegInput.Length || !jpegInput.CanRead || probeBudget.Bytes != 0)
                    throw new Exception("Packaged JPEG probe prefix, dimensions or borrowed ownership differs.");
            }
            using var invalidPrefix = new MemoryStream([255, 216, 17, 99], writable: false);
            try
            {
                _ = await ImageCodec.ProbeAsync(invalidPrefix, probeOptions with
                    { Limits = probeOptions.Limits with { MaxInputBytes = 3 } });
                throw new Exception("Packaged invalid JPEG marker was accepted.");
            }
            catch (ImageDataException) { }
            if (invalidPrefix.Position != 3 || !invalidPrefix.CanRead
                || probeBudget.Bytes != 0 || probeBudget.Peak != 512 || probeBudget.IsDisposed)
                throw new Exception("Packaged JPEG probe input limit or admission cleanup differs.");
        }
        Console.WriteLine("Packaged JPEG probes preserve bounded prefixes and invalid-marker limits.");
        await GovernedStreamingConsumer.Verify();
    }
}
