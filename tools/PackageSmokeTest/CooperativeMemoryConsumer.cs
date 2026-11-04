namespace Lokad.Image.Tooling;

internal static class CooperativeMemoryConsumer
{
    private static void VerifyStreamingBicubic()
    {
        foreach (var (depth, width, height, expectedHash) in new[]
        {
            (8, 8, 17, "481df76194da6537d6ecfbc24f37c842ae750b7752791f35d40d43454532eee1"),
            (16, 8, 17, "9f1aaa6ef72a650982022d0a36563789f9ae50d79a094f97141af8ed50051b6c"),
            (8, 4, 9, "df49ddc41185b3feaa2a0fc283fb3cd2432e463aa4aa7294a041cd76036effd1"),
            (16, 4, 9, "367e7576a57e596e44740e0f6b4e66c72772689e2d784011e29e39608bcdff76")
        })
        {
            var sourceBytes = 513L * 259 * (depth / 2);
            var outputBytes = (long)width * height * (depth / 2);
            using var root = new TestMemoryBudget(sourceBytes + 2 * outputBytes + 4096);
            var limits = new ImageLimits { MemoryBudget = root, MaxWorkingBytes = sourceBytes + outputBytes + 4096 };
            using var seed = OriginalPixels.Buffer(513, 259, depth);
            using var source = ImageBuffer.Create(513, 259, depth, limits);
            if (depth == 8) seed.GetRow8(0)[3] = seed.GetRow8(0)[513 * 4 - 1] = 0;
            else seed.GetRow16(0)[3] = seed.GetRow16(0)[513 * 4 - 1] = 0;
            for (var y = 0; y < 259; y++)
                if (depth == 8) seed.GetRow8(y).CopyTo(source.GetRow8(y));
                else seed.GetRow16(y).CopyTo(source.GetRow16(y));
            using var first = ImageTransforms.Resize(source, width, height, ResizeFilter.Bicubic, limits);
            using var survivor = ImageTransforms.Resize(source, width, height, ResizeFilter.Bicubic, limits);
            string Hash(ImageBuffer image) => Convert.ToHexStringLower(System.Security.Cryptography.SHA256.HashData(OriginalPixels.Bytes(image)));
            if (Hash(first) != expectedHash || Hash(survivor) != expectedHash || root.Bytes != sourceBytes + 2 * outputBytes)
                throw new Exception("Packaged streaming bicubic changed pixels or retained charges.");
            using (var caller = root.CreateScope())
            {
                if (!caller.TryReserve(4096, out var held)) throw new Exception("Packaged streaming refusal setup differs.");
                try
                {
                    using var refused = ImageTransforms.Resize(source, width, height, ResizeFilter.Bicubic, limits);
                    throw new Exception("Packaged streaming bicubic ignored shared refusal.");
                }
                catch (ImageLimitException e) when (e.Scope == ImageLimitScope.SharedBudget && e.Limit == ImageLimitKind.WorkingBytes) { }
                if (root.Bytes != sourceBytes + 2 * outputBytes + held.Bytes || root.Refusals != 1)
                    throw new Exception("Packaged streaming bicubic refusal leaked ownership.");
            }
            try
            {
                using var canceled = ImageTransforms.Resize(source, width, height, ResizeFilter.Bicubic, limits, new CancellationToken(true));
                throw new Exception("Packaged streaming bicubic ignored cancellation.");
            }
            catch (OperationCanceledException) { }
            if (!OriginalPixels.Bytes(source).AsSpan().SequenceEqual(OriginalPixels.Bytes(seed)))
                throw new Exception("Packaged streaming bicubic changed caller samples.");
            source.Dispose(); first.Dispose();
            if (root.Bytes != outputBytes || Hash(survivor) != expectedHash)
                throw new Exception("Packaged streaming bicubic survivor lost ownership.");
            survivor.Dispose();
            using (var next = ImageTransforms.Resize(seed, width, height, ResizeFilter.Bicubic, limits))
                if (Hash(next) != expectedHash || root.Bytes != outputBytes) throw new Exception("Packaged streaming bicubic did not recover.");
            if (root.Bytes != 0 || root.IsDisposed) throw new Exception("Packaged streaming bicubic disposed or leaked the caller root.");
        }
        Console.WriteLine("Packaged streaming bicubic preserves exact blocks, shared admission and independent survivors.");
    }

    private static void VerifyCompositingRows()
    {
        foreach (var sourceDepth in new[] { 8, 16 })
        foreach (var canvasDepth in new[] { 8, 16 })
        {
            using var root = new TestMemoryBudget(4096); var limits = new ImageLimits { MemoryBudget = root };
            using var source = ImageBuffer.Create(3, 1, sourceDepth, limits);
            using var canvas = ImageBuffer.Create(3, 1, canvasDepth, limits);
            var sourceMax = sourceDepth == 8 ? 255 : 65535; var canvasMax = canvasDepth == 8 ? 255 : 65535;
            for (var x = 0; x < 3; x++)
            {
                for (var c = 0; c < 3; c++)
                {
                    Set(source, x, c, (37 + x * 47 + c * 59) * (sourceDepth == 8 ? 1 : 257));
                    Set(canvas, x, c, (229 - x * 43 - c * 67) * (canvasDepth == 8 ? 1 : 257));
                }
                Set(source, x, 3, x == 0 ? 0 : x == 1 ? sourceMax / 3 : sourceMax);
                Set(canvas, x, 3, x == 0 ? canvasMax : x == 1 ? canvasMax / 2 : 1);
            }
            var expected = new int[12]; var original = OriginalPixels.Bytes(source);
            for (var x = 0; x < 3; x++)
            {
                var sa = (System.Numerics.BigInteger)Sample(source, x, 3) * (sourceDepth == 8 ? 257 : 1);
                var da = (System.Numerics.BigInteger)Sample(canvas, x, 3) * (canvasDepth == 8 ? 257 : 1);
                var coverage = sa * 65535 + da * (65535 - sa);
                for (var c = 0; c < 3; c++)
                {
                    var sc = (System.Numerics.BigInteger)Sample(source, x, c) * (sourceDepth == 8 ? 257 : 1);
                    var dc = (System.Numerics.BigInteger)Sample(canvas, x, c) * (canvasDepth == 8 ? 257 : 1);
                    expected[x * 4 + c] = sa == 0 ? Sample(canvas, x, c)
                        : Round((sc * sa * 65535 + dc * da * (65535 - sa)) * canvasMax, coverage * 65535);
                }
                expected[x * 4 + 3] = sa == 0 ? Sample(canvas, x, 3) : Round(coverage * canvasMax, (System.Numerics.BigInteger)65535 * 65535);
            }
            var ownedBytes = source.Stride + canvas.Stride;
            ImageDrawing.Composite(canvas, source, 0, 0, limits);
            for (var x = 0; x < 3; x++) for (var c = 0; c < 4; c++)
                if (Sample(canvas, x, c) != expected[x * 4 + c]) throw new Exception("Packaged compositing changed exact rational samples.");
            if (root.Bytes != ownedBytes || !original.AsSpan().SequenceEqual(OriginalPixels.Bytes(source)))
                throw new Exception("Packaged compositing changed source or borrowed ownership.");
            source.Dispose(); canvas.Dispose();
            if (root.Bytes != 0 || root.IsDisposed) throw new Exception("Packaged compositing lost caller ownership.");
        }
        Console.WriteLine("Packaged compositing preserves exact rational samples and shared ownership for every depth pair.");

        static int Sample(ImageBuffer image, int x, int c) => image.BitDepth == 8 ? image.GetRow8(0)[x * 4 + c] : image.GetRow16(0)[x * 4 + c];
        static void Set(ImageBuffer image, int x, int c, int value)
        { if (image.BitDepth == 8) image.GetRow8(0)[x * 4 + c] = (byte)value; else image.GetRow16(0)[x * 4 + c] = (ushort)value; }
        static int Round(System.Numerics.BigInteger numerator, System.Numerics.BigInteger denominator)
        {
            var quotient = System.Numerics.BigInteger.DivRem(numerator, denominator, out var remainder);
            return (int)(quotient + (2 * remainder >= denominator ? 1 : 0));
        }
    }

    private static async Task VerifyLanczosPlans()
    {
        foreach (var depth in new[] { 8, 16 })
        {
            using var root = new TestMemoryBudget(3 * 1024 * 1024);
            var limits = new ImageLimits { MemoryBudget = root };
            using var original = OriginalPixels.Buffer(31, 19, depth);
            using var source = original.Clone(limits);
            using var plan = Lanczos3ResizePlan.Create(31, 19, 17, 11, depth, limits);
            if (root.Bytes != source.Stride * source.Height + plan.StorageBytes)
                throw new Exception("Packaged Lanczos preparation lost its retained admission.");
            using var first = plan.Resize(source);
            using var changed = OriginalPixels.Buffer(31, 19, depth, phase: 2);
            for (var y = 0; y < source.Height; y++)
                if (depth == 8) changed.GetRow8(y).CopyTo(source.GetRow8(y));
                else changed.GetRow16(y).CopyTo(source.GetRow16(y));
            using var expected = ImageTransforms.Resize(changed, 17, 11, ResizeFilter.Lanczos3);
            using var survivor = plan.Resize(source);
            var pixels = OriginalPixels.Bytes(expected);
            if (!pixels.AsSpan().SequenceEqual(OriginalPixels.Bytes(survivor)))
                throw new Exception("Packaged Lanczos plan cached source pixels or changed exact results.");
            first.Dispose(); source.Dispose(); plan.Dispose();
            if (root.Bytes != pixels.Length || plan.StorageBytes != 0)
                throw new Exception("Packaged Lanczos survivor lost ownership or retained plan storage.");
            using var output = new MemoryStream();
            await ImageCodec.EncodePngAsync(survivor, output, new PngEncodeOptions { Limits = limits });
            if (root.Bytes != pixels.Length || !pixels.AsSpan().SequenceEqual(OriginalPixels.Bytes(survivor)))
                throw new Exception("Packaged Lanczos survivor lost admission during asynchronous encoding.");
            survivor.Dispose();
            if (root.Bytes != 0 || root.IsDisposed) throw new Exception("Packaged Lanczos plan leaked admission or disposed the caller root.");
        }
        Console.WriteLine("Packaged Lanczos plans preserve exact reuse, retained admission and asynchronous surviving outputs.");
    }
    private static async Task VerifyWebpCandidateWorkspace()
    {
        // Authored flat regions and a small many-color patch exercise empty
        // spatial tiles independently of the photograph/diagram fixtures. The
        // frame is large enough to hand matching heads to ordinary token storage.
        // An authored noise patch also reaches prefix-aggregated cache scoring.
        const int width = 513, height = 259, capacity = 6 * 1024 * 1024;
        var pixels = new byte[width * height * 4]; uint noise = 0x31415926;
        for (var y = 0; y < height; y++) for (var x = 0; x < width; x++)
        {
            var i = (y * width + x) * 4;
            pixels[i] = 32; pixels[i + 1] = 64; pixels[i + 2] = 96; pixels[i + 3] = 255;
            if (x is >= 64 and < 192 && y is >= 32 and < 128)
            {
                noise ^= noise << 13; noise ^= noise >> 17; noise ^= noise << 5;
                pixels[i] = (byte)noise; pixels[i + 1] = (byte)(noise >> 8);
                pixels[i + 2] = (byte)(noise >> 16); pixels[i + 3] = (byte)((x + y) % 7 == 0 ? 0 : noise >> 24);
            }
        }
        using var root = new TestMemoryBudget(capacity); var limits = new ImageLimits { MemoryBudget = root };
        using var source = ImageBuffer.Create(width, height, 8, limits);
        for (var y = 0; y < height; y++) pixels.AsSpan(y * source.Stride, source.Stride).CopyTo(source.GetRow8(y));
        var options = new WebpEncodeOptions { Lossless = true, Effort = 4, Quality = 85, Limits = limits };
        using (var caller = root.CreateScope())
        {
            if (!caller.TryReserve(capacity - pixels.Length - 1024, out var held)) throw new Exception("WebP refusal setup differs.");
            using (held)
            using (var refused = new MemoryStream())
            {
                try { ImageCodec.EncodeWebp(source, refused, options); throw new Exception("WebP ignored shared refusal."); }
                catch (ImageLimitException e) when (e.Scope == ImageLimitScope.SharedBudget) { }
                if (refused.Length != 0 || !refused.CanWrite || root.Bytes != pixels.Length + held.Bytes)
                    throw new Exception("WebP refusal leaked capacity or took output ownership.");
            }
        }
        using var synchronous = new MemoryStream(); using var asynchronous = new MemoryStream();
        ImageCodec.EncodeWebp(source, synchronous, options);
        await ImageCodec.EncodeWebpAsync(source, asynchronous, options);
        if (!synchronous.ToArray().AsSpan().SequenceEqual(asynchronous.ToArray()) || root.Bytes != pixels.Length
            || !synchronous.CanWrite || !asynchronous.CanWrite || !OriginalPixels.Bytes(source).AsSpan().SequenceEqual(pixels))
            throw new Exception("WebP workspace changed packets, source or asynchronous ownership.");
        using var first = ImageCodec.Decode(synchronous.ToArray(), new DecodeOptions { Limits = limits });
        using var survivor = ImageCodec.Decode(asynchronous.ToArray(), new DecodeOptions { Limits = limits });
        source.Dispose(); first.Dispose();
        if (!OriginalPixels.Bytes(survivor).AsSpan().SequenceEqual(pixels) || root.Bytes != pixels.Length)
            throw new Exception("WebP surviving result lost pixels or admission.");
        survivor.Dispose();
        if (root.Bytes != 0 || root.IsDisposed) throw new Exception("WebP workspace leaked capacity or disposed the caller root.");
        Console.WriteLine("Packaged WebP candidate workspace preserves sync/async packets, hidden RGB and shared lifetimes.");
        Console.WriteLine("Packaged WebP head/token handoff preserves large-frame sync/async packets and shared ownership.");
        Console.WriteLine("Packaged WebP cache-prefix scoring preserves noisy-patch sync/async packets and shared ownership.");
    }
    private static void VerifyRectangleFill()
    {
        foreach (var depth in new[] { 8, 16 })
        {
            const int width = 513, height = 259;
            var sampleBytes = depth / 8;
            var input = OriginalPixels.Generate(width, height, depth);
            input.AsSpan(3 * sampleBytes, sampleBytes).Clear();
            input.AsSpan((width * 4 - 1) * sampleBytes, sampleBytes).Clear();
            using var root = new TestMemoryBudget(input.Length);
            var limits = new ImageLimits { MemoryBudget = root };
            using var canvas = ImageBuffer.Create(width, height, depth, limits);
            for (var y = 0; y < height; y++)
                if (depth == 8) input.AsSpan(y * width * 4, width * 4).CopyTo(canvas.GetRow8(y));
                else
                    for (var sample = 0; sample < width * 4; sample++)
                        canvas.GetRow16(y)[sample] = System.Buffers.Binary.BinaryPrimitives.ReadUInt16LittleEndian(
                            input.AsSpan((y * width * 4 + sample) * 2));
            input.AsSpan().Clear();
            var color = depth == 8 ? RgbaColor.From8(23, 57, 91, 128) : new RgbaColor(12345, 23456, 34567, 45678);
            for (var phase = 0; phase < 2; phase++)
            {
                var fill = phase == 0 ? color : color with { A = 0 };
                for (var repeat = 0; repeat < 2; repeat++)
                    ImageDrawing.FillRectangle(canvas, -17, 19, 273, 129, fill, limits);
                var expected = depth == 8
                    ? (phase == 0 ? "b2cae827d4f9e5e328882d4a7aa2080829418d365f6ed7d882977304d510b478" : "8dff1a6f3c931f2f1cc34e172dcecf727eef62692319ce5f667b68debfc4c32d")
                    : (phase == 0 ? "e112187d480232292794a75fcde041e5ba3a156d1b81a0f10989153a9203849a" : "4724530fb82c12776f8e3334cdee92012c653f8de385de51a5d63a4a53e543eb");
                var hash = Convert.ToHexStringLower(System.Security.Cryptography.SHA256.HashData(OriginalPixels.Bytes(canvas)));
                if (hash != expected || root.Bytes != input.Length || root.Refusals != 0 || root.IsDisposed)
                    throw new Exception("Packaged fill changed exact pixels or required additional canvas storage.");
            }
            canvas.Dispose();
            if (root.Bytes != 0 || root.IsDisposed)
                throw new Exception("Packaged fill leaked storage or disposed the borrowed root.");
        }
        Console.WriteLine("Packaged rectangle fill preserves exact byte/word canvases, zero-alpha RGB and shared ownership.");
    }

    private static void VerifyRectangleCopy()
    {
        foreach (var depth in new[] { 8, 16 })
        {
            const int width = 513, height = 259;
            var sampleBytes = depth / 8;
            var input = OriginalPixels.Generate(width, height, depth);
            input.AsSpan(3 * sampleBytes, sampleBytes).Clear();
            input.AsSpan((width * 4 - 1) * sampleBytes, sampleBytes).Clear();
            using var root = new TestMemoryBudget(2L * input.Length);
            var limits = new ImageLimits { MemoryBudget = root };
            using var source = ImageBuffer.Create(width, height, depth, limits);
            using var canvas = ImageBuffer.Create(width, height, depth, limits);
            for (var y = 0; y < height; y++)
                if (depth == 8)
                {
                    input.AsSpan(y * width * 4, width * 4).CopyTo(source.GetRow8(y));
                    input.AsSpan(y * width * 4, width * 4).CopyTo(canvas.GetRow8(y));
                }
                else
                    for (var sample = 0; sample < width * 4; sample++)
                    {
                        var value = System.Buffers.Binary.BinaryPrimitives.ReadUInt16LittleEndian(
                            input.AsSpan((y * width * 4 + sample) * 2));
                        source.GetRow16(y)[sample] = value;
                        canvas.GetRow16(y)[sample] = value;
                    }
            for (var repeat = 0; repeat < 2; repeat++)
                ImageDrawing.CopyRegion(source, 127, 65, 256, 129, canvas, 17, 19, limits);
            if (root.Bytes != 2L * input.Length || root.Refusals != 0 ||
                !OriginalPixels.Bytes(source).AsSpan().SequenceEqual(input))
                throw new Exception("Packaged copy changed its borrowed source or required extra storage.");
            source.Dispose(); input.AsSpan().Clear();
            var expected = depth == 8 ? "d5b9f521f37c7933ec119815d69806c52b7b060d733bb1f14564e0b075856ac4"
                : "046a1b2e10c7c8e744e8af15497a0b3d498e750e84b2f15fa69cd082a3e1078d";
            var hash = Convert.ToHexStringLower(System.Security.Cryptography.SHA256.HashData(OriginalPixels.Bytes(canvas)));
            if (hash != expected || root.Bytes != canvas.Stride * (long)height || root.IsDisposed)
                throw new Exception("Packaged copy changed exact pixels or surviving canvas ownership.");
            canvas.Dispose();
            if (root.Bytes != 0 || root.IsDisposed)
                throw new Exception("Packaged copy leaked storage or disposed the borrowed root.");
        }
        Console.WriteLine("Packaged rectangle copy preserves exact byte/word canvases and shared ownership.");
    }

    private static async Task VerifyDirectRgbResult()
    {
        const int width = 513, height = 259, bytes = width * height * 4;
        var data = AuthoredJpeg.SequentialRgbFrame(width, height, 2, false);
        using var root = new TestMemoryBudget(data.Length + 2L * bytes + 128 * 1024);
        if (!root.TryReserve(data.Length, out var input)) throw new Exception("Cannot admit packaged RGB input.");
        using var inputOwner = input;
        var options = new DecodeOptions { Limits = new() { MemoryBudget = root } };
        using var first = ImageCodec.Decode(data, options);
        using var stream = new MemoryStream(data, writable: false);
        using var second = await ImageCodec.DecodeAsync(stream, options);
        if (root.Bytes != data.Length + 2L * bytes || !stream.CanRead || root.IsDisposed)
            throw new Exception("Packaged RGB result or stream ownership differs.");
        try
        {
            using var unexpected = ImageCodec.Decode(data, options);
            throw new Exception("Packaged RGB ignored aggregate refusal.");
        }
        catch (ImageLimitException error) when (error.Scope == ImageLimitScope.SharedBudget) { }
        if (root.Refusals != 1 || root.Bytes != data.Length + 2L * bytes)
            throw new Exception("Packaged RGB admission was retried or leaked storage.");
        first.Dispose();
        using var recovered = ImageCodec.Decode(data, options);
        data.AsSpan().Clear();
        foreach (var image in new[] { second, recovered })
            for (var y = 0; y < height; y++)
            {
                var row = image.GetRow8(y);
                for (var x = 0; x < width; x++)
                    if (row[x * 4] != 45 || row[x * 4 + 1] != 105 || row[x * 4 + 2] != 205 || row[x * 4 + 3] != 255)
                        throw new Exception("Packaged RGB exact pixels or independent lifetime differ.");
            }
        recovered.Dispose(); second.Dispose();
        if (root.Bytes != data.Length) throw new Exception("Packaged RGB result storage leaked.");
        inputOwner?.Dispose();
        if (root.Bytes != 0 || root.IsDisposed) throw new Exception("Packaged RGB borrowed root differs.");
        Console.WriteLine("Packaged direct RGB JPEG preserves exact pixels, bounded ownership, async and admission recovery.");
    }

    private static void VerifyTranspose(int depth)
    {
        const int width = 35, height = 37;
        var bytes = width * height * (depth / 8) * 4;
        using var root = new TestMemoryBudget(3L * bytes);
        var limits = new ImageLimits { MemoryBudget = root, MaxWorkingBytes = 2L * bytes };
        using var source = ImageBuffer.Create(width, height, depth, limits);
        var original = new ushort[width * height * 4];
        for (var y = 0; y < height; y++)
        for (var x = 0; x < width; x++)
        for (var channel = 0; channel < 4; channel++)
        {
            var value = channel == 3 && (x + y) % 7 == 0 ? (ushort)0
                : (ushort)(x * 999 + y * 357 + channel * 8191);
            if (depth == 8)
            {
                value = (byte)value;
                source.GetRow8(y)[x * 4 + channel] = (byte)value;
            }
            else source.GetRow16(y)[x * 4 + channel] = value;
            original[(y * width + x) * 4 + channel] = value;
        }
        using var first = ImageTransforms.Transpose(source, false, limits);
        using var second = ImageTransforms.Transpose(source, false, limits);
        if (root.Bytes != 3L * bytes) throw new Exception("Packaged transpose retained charges differ.");
        try
        {
            using var unexpected = ImageTransforms.Transpose(source, false, limits);
            throw new Exception("Packaged transpose ignored aggregate refusal.");
        }
        catch (ImageLimitException error) when (error.Scope == ImageLimitScope.SharedBudget) { }
        source.Dispose(); first.Dispose();
        if (root.Bytes != bytes) throw new Exception("Packaged transpose ownership depends on disposed inputs.");
        for (var y = 0; y < height; y++)
        for (var x = 0; x < width; x++)
        for (var channel = 0; channel < 4; channel++)
            if ((depth == 8 ? second.GetRow8(x)[y * 4 + channel] : second.GetRow16(x)[y * 4 + channel])
                != original[(y * width + x) * 4 + channel])
                throw new Exception("Packaged transpose changed complete pixels or scalar tails.");
        using (var restored = ImageTransforms.Transpose(second, false, limits))
        {
            second.Dispose();
            if (root.Bytes != bytes) throw new Exception("Packaged transpose recovery charge differs.");
            for (var y = 0; y < height; y++)
            for (var sample = 0; sample < width * 4; sample++)
                if ((depth == 8 ? restored.GetRow8(y)[sample] : restored.GetRow16(y)[sample])
                    != original[y * width * 4 + sample])
                    throw new Exception("Packaged transpose recovery pixels differ.");
        }
        if (root.Bytes != 0 || root.IsDisposed) throw new Exception("Packaged transpose leaked or disposed the borrowed root.");
        Console.WriteLine($"Packaged RGBA{depth} transpose preserves exact pixels, tails and shared ownership.");
    }

    private static void VerifyWideBicubicRepair()
    {
        foreach (var depth in new[] { 8, 16 })
        {
            const int width = 513, height = 259, outputWidth = 128, outputHeight = 64;
            var sampleBytes = depth / 8;
            var input = OriginalPixels.Generate(width, height, depth);
            input.AsSpan(3 * sampleBytes, sampleBytes).Clear();
            input.AsSpan((width * 4 - 1) * sampleBytes, sampleBytes).Clear();
            using var root = new TestMemoryBudget(4 * 1024 * 1024);
            var limits = new ImageLimits { MemoryBudget = root };
            using var source = ImageBuffer.Create(width, height, depth, limits);
            for (var y = 0; y < height; y++)
                if (depth == 8) input.AsSpan(y * width * 4, width * 4).CopyTo(source.GetRow8(y));
                else
                    for (var sample = 0; sample < width * 4; sample++)
                        source.GetRow16(y)[sample] = System.Buffers.Binary.BinaryPrimitives.ReadUInt16LittleEndian(
                            input.AsSpan((y * width * 4 + sample) * 2));
            using var first = ImageTransforms.Resize(source, outputWidth, outputHeight, ResizeFilter.Bicubic, limits);
            using var second = ImageTransforms.Resize(source, outputWidth, outputHeight, ResizeFilter.Bicubic, limits);
            var expectedHash = depth == 8 ? "5067190b92fe644a75b0001848c4a555feb638c72592239dbf0ea0d63c518e71"
                : "3763385e1f45803392214ccf0b87ef6e4f1ec22f2f37701108cc52e9a20209d9";
            var outputBytes = outputWidth * outputHeight * 4 * sampleBytes;
            if (root.Bytes != input.Length + 2L * outputBytes)
                throw new Exception("Packaged wider bicubic retained temporary storage.");
            source.Dispose(); first.Dispose(); input.AsSpan().Clear();
            var hash = Convert.ToHexStringLower(System.Security.Cryptography.SHA256.HashData(OriginalPixels.Bytes(second)));
            if (hash != expectedHash || root.Bytes != outputBytes)
                throw new Exception("Packaged wider bicubic changed exact pixels or surviving ownership.");
            second.Dispose();
            if (root.Bytes != 0 || root.IsDisposed)
                throw new Exception("Packaged wider bicubic leaked or disposed the borrowed root.");
        }
        Console.WriteLine("Packaged wider bicubic preserves exact byte/word frames and shared ownership.");
    }

    internal static async Task Verify()
    {
        VerifyStreamingBicubic();
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
        await VerifyWordPngPacking(budget, limits);
        VerifyTranspose(8);
        VerifyTranspose(16);
        VerifyWideBicubicRepair();
        VerifyRectangleCopy();
        VerifyRectangleFill();
        VerifyCompositingRows();
        await VerifyWebpCandidateWorkspace();
        await VerifyLanczosPlans();
        await VerifyCompactPngDictionary(budget, limits);
        await VerifyDirectRgbResult();
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
        // Reuse the existing original progressive sample control. A live token
        // retains the original logical-read traversal and supplies the pixel
        // oracle for the ordinary grouped-refinement window in this actual package.
        var progressivePacket = AuthoredJpeg.ProgressiveFrame(31, 23, JpegSampling.Grayscale, 1);
        var progressiveInput = progressivePacket.ToArray();
        using var progressiveCancellation = new CancellationTokenSource();
        var progressive = ImageCodec.Decode(progressivePacket, new() { Limits = limits });
        using var progressiveOriginal = ImageCodec.Decode(progressivePacket, new() { Limits = limits }, progressiveCancellation.Token);
        using var progressiveStream = new MemoryStream(progressivePacket, writable: false);
        using var progressiveAsync = await ImageCodec.DecodeAsync(progressiveStream, new() { Limits = limits });
        var progressivePixels = OriginalPixels.Bytes(progressiveOriginal);
        if (!progressivePixels.AsSpan().SequenceEqual(OriginalPixels.Bytes(progressive))
            || !progressivePixels.AsSpan().SequenceEqual(OriginalPixels.Bytes(progressiveAsync))
            || !progressivePacket.AsSpan().SequenceEqual(progressiveInput) || budget.Bytes != 3 * 31 * 23 * 4)
            throw new Exception("Packaged progressive refinement pixels or retained admission differ.");
        progressivePacket.AsSpan().Clear(); progressive.GetRow8(0)[0] ^= 255; progressive.Dispose();
        if (budget.Bytes != 2 * 31 * 23 * 4 || !progressivePixels.AsSpan().SequenceEqual(OriginalPixels.Bytes(progressiveOriginal))
            || !progressivePixels.AsSpan().SequenceEqual(OriginalPixels.Bytes(progressiveAsync)) || !progressiveStream.CanRead)
            throw new Exception("Packaged progressive independent output or borrowed input lifetime differs.");
        progressiveOriginal.Dispose(); progressiveAsync.Dispose();
        if (budget.Bytes != 0 || budget.IsDisposed) throw new Exception("Packaged progressive storage leaked.");
        Console.WriteLine("Packaged progressive refinement exact pixels, async and retained ownership passed.");
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
        await VerifyJpeg444(budget, limits);
        await GovernedStreamingConsumer.Verify();
    }

    private static async Task VerifyJpeg444(TestMemoryBudget budget, ImageLimits limits)
    {
        foreach (var depth in new[] { 8, 16 })
        {
            using var source = ImageBuffer.Create(17, 11, depth, limits);
            var rgb = new byte[17 * 11 * 3];
            var maximum = depth == 8 ? 255 : 65535;
            for (var y = 0; y < 11; y++)
            for (var x = 0; x < 17; x++)
            {
                var alpha = (x * 977 + y * 43) & maximum;
                for (var c = 0; c < 4; c++)
                {
                    var sample = c == 3 ? alpha : (1 + x * 257 + y * 191 + c * 811) & maximum;
                    if (depth == 8) source.GetRow8(y)[x * 4 + c] = (byte)sample;
                    else source.GetRow16(y)[x * 4 + c] = (ushort)sample;
                    if (c == 3) continue;
                    var background = c == 0 ? 0x20 : c == 1 ? 0x40 : 0x60;
                    var denominator = (long)maximum * maximum;
                    var numerator = sample * (long)alpha * 255 + background * (long)(maximum - alpha) * maximum;
                    rgb[(y * 17 + x) * 3 + c] = (byte)((numerator + denominator / 2) / denominator);
                }
            }
            var pixels = OriginalPixels.Bytes(source);
            var options = new JpegEncodeOptions
                { Quality = 85, Sampling = JpegSampling.YCbCr444, BackgroundRgb = 0x204060, Limits = limits };
            using var caller = new MemoryStream();
            using var owned = new MemoryStream();
            using var asynchronous = new MemoryStream();
            ImageCodec.EncodeJpeg(rgb, 17, 11, JpegSampleFormat.Rgb, caller, options: options);
            ImageCodec.EncodeJpeg(source, owned, options);
            await ImageCodec.EncodeJpegAsync(source, asynchronous, options);
            if (!caller.ToArray().AsSpan().SequenceEqual(owned.ToArray())
                || !caller.ToArray().AsSpan().SequenceEqual(asynchronous.ToArray())
                || !pixels.AsSpan().SequenceEqual(OriginalPixels.Bytes(source))
                || budget.Bytes != pixels.Length || budget.IsDisposed
                || !caller.CanWrite || !owned.CanWrite || !asynchronous.CanWrite)
                throw new Exception("Packaged JPEG444 normalization, packet or borrowed ownership differs.");
        }
        if (budget.Bytes != 0 || budget.IsDisposed)
            throw new Exception("Packaged JPEG444 retained storage leaked.");
        Console.WriteLine("Packaged JPEG444 byte/word normalization, exact packets, async and shared ownership passed.");
    }

    private static async Task VerifyCompactPngDictionary(TestMemoryBudget budget, ImageLimits limits)
    {
        const int width = 513, height = 259, sourceBytes = width * height * 4;
        using var seed = OriginalPixels.Buffer(width, height, 8);
        seed.GetRow8(0)[3] = seed.GetRow8(0)[width * 4 - 1] = 0;
        using var source = seed.Clone(limits); var pixels = OriginalPixels.Bytes(source);
        var options = new PngEncodeOptions { ColorType = PngColorType.Rgba,
            Compression = System.IO.Compression.CompressionLevel.Optimal, Limits = limits };
        var working = sourceBytes + width * 4L + 65536 + 384 * 1024;
        using var rejected = new MemoryStream();
        try
        {
            ImageCodec.EncodePng(source, rejected, options with { Limits = limits with { MaxWorkingBytes = working - 1 } });
            throw new Exception("Packaged compact PNG ignored its working-storage boundary.");
        }
        catch (ImageLimitException e) when (e.Limit == ImageLimitKind.WorkingBytes) { }
        if (rejected.Length != 0 || budget.Bytes != sourceBytes) throw new Exception("Packaged compact PNG refusal leaked or wrote output.");
        using var synchronous = new MemoryStream(); using var asynchronous = new MemoryStream();
        ImageCodec.EncodePng(source, synchronous, options with { Limits = limits with { MaxWorkingBytes = working } });
        await ImageCodec.EncodePngAsync(source, asynchronous, options);
        var packet = synchronous.ToArray();
        var hash = Convert.ToHexStringLower(System.Security.Cryptography.SHA256.HashData(packet));
        if (packet.Length != 210973 || hash != "ad9ed127da583d68bdb749bb983b73966e8e814527216449c4557fdd89668920"
            || !packet.AsSpan().SequenceEqual(asynchronous.ToArray())
            || !pixels.AsSpan().SequenceEqual(OriginalPixels.Bytes(source))
            || budget.Bytes != sourceBytes)
            throw new Exception("Packaged compact PNG changed complete packets, source or asynchronous storage lifetimes.");
        source.Dispose();
        if (budget.Bytes != 0 || budget.IsDisposed || !synchronous.CanWrite || !asynchronous.CanWrite)
            throw new Exception("Packaged compact PNG lost independent output or borrowed-root ownership.");
        Console.WriteLine("Packaged compact PNG preserves full packets, 384KiB admission and async ownership after dictionary slides.");
    }

    private static async Task VerifyWordPngPacking(TestMemoryBudget budget, ImageLimits limits)
    {
        foreach (var color in new[] { PngColorType.Grayscale, PngColorType.Rgb, PngColorType.GrayscaleAlpha })
        {
            using var source = ImageBuffer.Create(17, 2, 16, limits);
            for (var y = 0; y < source.Height; y++)
            for (var x = 0; x < source.Width; x++)
            {
                var pixel = source.GetRow16(y).Slice(x * 4, 4);
                pixel[0] = (ushort)(1 + x * 257 + y * 43);
                pixel[1] = color == PngColorType.Rgb ? (ushort)(pixel[0] ^ 0x1234) : pixel[0];
                pixel[2] = color == PngColorType.Rgb ? (ushort)(pixel[0] ^ 0x8000) : pixel[0];
                pixel[3] = color == PngColorType.GrayscaleAlpha ? (ushort)(x == 0 ? 0 : x * 977 + y) : ushort.MaxValue;
            }
            var pixels = OriginalPixels.Bytes(source); var sourceBytes = pixels.Length;
            var options = new PngEncodeOptions { ColorType = color, Limits = limits };
            using (var caller = budget.CreateScope())
            {
                if (!caller.TryReserve(4 * 1024 * 1024 - sourceBytes - 65536, out var held))
                    throw new Exception("Packaged word PNG refusal setup differs.");
                using var rejected = new MemoryStream();
                try
                {
                    ImageCodec.EncodePng(source, rejected, options);
                    throw new Exception("Packaged word PNG ignored shared admission refusal.");
                }
                catch (ImageLimitException e) when (e.Scope == ImageLimitScope.SharedBudget && e.Limit == ImageLimitKind.WorkingBytes) { }
                if (rejected.Length != 0 || budget.Bytes != sourceBytes + held.Bytes || !rejected.CanWrite)
                    throw new Exception("Packaged word PNG refusal leaked storage or wrote output.");
            }
            using var synchronous = new MemoryStream(); using var asynchronous = new MemoryStream();
            ImageCodec.EncodePng(source, synchronous, options);
            await ImageCodec.EncodePngAsync(source, asynchronous, options);
            if (!synchronous.ToArray().AsSpan().SequenceEqual(asynchronous.ToArray())
                || !pixels.AsSpan().SequenceEqual(OriginalPixels.Bytes(source))
                || budget.Bytes != sourceBytes || budget.IsDisposed || !synchronous.CanWrite || !asynchronous.CanWrite)
                throw new Exception("Packaged word PNG pixels, async cleanup or borrowed owners differ.");
            source.Dispose();
            if (budget.Bytes != 0) throw new Exception("Packaged word PNG source storage leaked.");
            using (var decoded = ImageCodec.Decode(asynchronous.ToArray(), new() { Limits = limits }))
            {
                if (decoded.BitDepth != 16 || !pixels.AsSpan().SequenceEqual(OriginalPixels.Bytes(decoded))
                    || budget.Bytes != sourceBytes)
                    throw new Exception("Packaged word PNG precision or independent retained output differs.");
            }
            if (budget.Bytes != 0 || budget.IsDisposed) throw new Exception("Packaged word PNG retained storage leaked.");
        }
        Console.WriteLine("Packaged full-word PNG color packing, async and shared admission passed.");
    }
}
