namespace Lokad.Image.Tooling;

internal static class CooperativeMemoryConsumer
{
    private static void VerifyClearedCanvases()
    {
        foreach (var depth in new[] { 8, 16 })
        {
            using var first = ImageBuffer.Create(23, 11, depth);
            using var survivor = ImageBuffer.Create(23, 11, depth);
            for (var y = 0; y < first.Height; y++)
            {
                if (depth == 8) { first.GetRow8(y).Fill(0xa5); survivor.GetRow8(y).Fill(0x36); }
                else { first.GetRow16(y).Fill(0xa5a5); survivor.GetRow16(y).Fill(0x3636); }
            }
            first.Dispose();
            using var next = ImageBuffer.Create(23, 11, depth);
            for (var y = 0; y < next.Height; y++)
            {
                if (depth == 8)
                {
                    foreach (var sample in next.GetRow8(y))
                        if (sample != 0) throw new Exception("Packaged canvas exposed previous storage.");
                    foreach (var sample in survivor.GetRow8(y))
                        if (sample != 0x36) throw new Exception("Packaged canvas aliased a live sibling.");
                }
                else
                {
                    foreach (var sample in next.GetRow16(y))
                        if (sample != 0) throw new Exception("Packaged word canvas exposed previous storage.");
                    foreach (var sample in survivor.GetRow16(y))
                        if (sample != 0x3636) throw new Exception("Packaged word canvas aliased a live sibling.");
                }
            }
        }
        Console.WriteLine("Packaged cleared canvases preserve byte/word zeroes and independent surviving owners.");
    }

    private static void VerifyBicubicPlans()
    {
        foreach (var depth in new[] { 8, 16 })
        foreach (var (width, height) in new[] { (128, 64), (256, 129) })
        {
            using var source = OriginalPixels.Buffer(513, 259, depth);
            if (depth == 8) source.GetRow8(0)[3] = source.GetRow8(0)[513 * 4 - 1] = 0;
            else source.GetRow16(0)[3] = source.GetRow16(0)[513 * 4 - 1] = 0;
            using var root = new TestMemoryBudget(4 * 1024 * 1024);
            using var plan = BicubicResizePlan.Create(513, 259, width, height, depth,
                new() { MemoryBudget = root });
            var retained = plan.StorageBytes;
            if (root.Bytes != retained) throw new Exception("Packaged bicubic preparation lost its charge.");
            using var result = plan.Resize(source);
            var expected = OriginalPixels.Bytes(result);
            var hash = width == 128
                ? depth == 8 ? "5067190b92fe644a75b0001848c4a555feb638c72592239dbf0ea0d63c518e71"
                    : "3763385e1f45803392214ccf0b87ef6e4f1ec22f2f37701108cc52e9a20209d9"
                : depth == 8 ? "656bb2248362c88dff8fe510dcc21055155976a3dd2b33aa20c48d2b036f59a8"
                    : "2676207be4d1a7c9109061fc3c8eb5a0d6b8d10a97bbbc611fbebda3330ffd9f";
            if (Convert.ToHexStringLower(System.Security.Cryptography.SHA256.HashData(expected)) != hash
                || root.Bytes != retained + expected.Length)
                throw new Exception("Packaged bicubic plan changed frozen samples or retained ownership.");
            Console.WriteLine($"Packaged {(width == 128 ? "" : "narrow ")}bicubic{depth}: retained={retained}, peak={root.Peak}, result={expected.Length}.");
            using var canceled = new CancellationTokenSource(); canceled.Cancel();
            try
            {
                using var unexpected = plan.Resize(source, canceled.Token);
                throw new Exception("Packaged bicubic plan ignored cancellation.");
            }
            catch (OperationCanceledException error) when (error.CancellationToken == canceled.Token) { }
            plan.Dispose(); source.Dispose();
            if (root.Bytes != expected.Length || !expected.AsSpan().SequenceEqual(OriginalPixels.Bytes(result)))
                throw new Exception("Packaged bicubic result did not survive plan/source disposal.");
            result.Dispose();
            if (root.Bytes != 0 || root.IsDisposed) throw new Exception("Packaged bicubic plan leaked or disposed the caller root.");
        }
        Console.WriteLine("Packaged bicubic plans preserve frozen samples and admitted survivor lifetimes.");
    }

    private static async Task VerifyPaletteResult()
    {
        using var root = new TestMemoryBudget(1024 * 1024);
        var options = new DecodeOptions { Limits = new() { MemoryBudget = root } };
        foreach (var colors in new[] { 2, 4, 16, 17, 256 })
        {
            var packet = AuthoredWebp.Palette(colors, thenPredictor: true);
            var expected = AuthoredWebp.ExpectedPalette(colors);
            using var first = ImageCodec.Decode(packet, options);
            ImageBuffer survivor;
            using (var input = new MemoryStream(packet, writable: false))
            {
                survivor = await ImageCodec.DecodeAsync(input, options);
                if (!input.CanRead) throw new Exception("Packaged palette closed borrowed input.");
            }
            using (survivor)
            {
                if (root.Bytes != 72 || !expected.AsSpan().SequenceEqual(OriginalPixels.Bytes(first))
                    || !expected.AsSpan().SequenceEqual(OriginalPixels.Bytes(survivor)))
                    throw new Exception("Packaged palette samples or retained admission differ.");
                first.GetRow8(0).Fill(0xA7); first.Dispose(); packet.AsSpan().Clear();
                if (root.Bytes != 36 || !expected.AsSpan().SequenceEqual(OriginalPixels.Bytes(survivor)))
                    throw new Exception("Packaged palette result aliases another owner or input.");
            }
            if (root.Bytes != 0 || root.IsDisposed) throw new Exception("Packaged palette leaked retained capacity.");
        }
        Console.WriteLine("Packaged final palettes preserve exact packed samples and independent sync/async admitted owners.");
    }

    private static void VerifyBicubicSchedule(int workspaceBytes, bool vertical = false, int depth = 16,
        bool reduction = false, bool packedReduction = false, int crossHeight = 0, bool horizontal = false,
        int heightOverride = 0, int widthOverride = 0)
    {
        var width = widthOverride > 0 ? widthOverride
            : crossHeight > 0 ? 256 : packedReduction ? 64 : reduction ? 256 : vertical ? 513 : 1027;
        var height = heightOverride > 0 ? heightOverride : horizontal ? 259 : crossHeight > 0 ? crossHeight : packedReduction ? 32 : 129;
        var outputBytes = width * height * (depth / 2);
        using var source = OriginalPixels.Buffer(513, 259, depth);
        if (depth == 16) source.GetRow16(0)[3] = source.GetRow16(0)[513 * 4 - 1] = 0;
        else source.GetRow8(0)[3] = source.GetRow8(0)[513 * 4 - 1] = 0;
        var input = OriginalPixels.Bytes(source);
        if (crossHeight > 0 && depth == 8 && Convert.ToHexStringLower(System.Security.Cryptography.SHA256.HashData(input))
            != "70f0fb18c341e2740b875a5a34e21ca63f0821b3386352fe2971020c5a6aac80")
            throw new Exception("Packaged byte cross input differs from the frozen control.");
        if ((vertical || widthOverride > 0) && depth == 16
            && Convert.ToHexStringLower(System.Security.Cryptography.SHA256.HashData(input))
            != "3f861b61360b442c84063e7a6e1cb4f35957dcaa16f898048beaa36bbf4272f9")
            throw new Exception("Packaged vertical word input differs from the frozen control.");
        using var root = new TestMemoryBudget(2L * outputBytes + workspaceBytes);
        var limits = new ImageLimits { MemoryBudget = root, MaxWorkingBytes = input.Length + outputBytes + (long)workspaceBytes };
        var first = ImageTransforms.Resize(source, width, height, ResizeFilter.Bicubic, limits);
        var second = ImageTransforms.Resize(source, width, height, ResizeFilter.Bicubic, limits);
        var expected = OriginalPixels.Bytes(second);
        var vectorBands = System.Runtime.Intrinsics.X86.Avx512F.IsSupported
            && System.Runtime.Intrinsics.X86.Avx2.IsSupported && System.Runtime.Intrinsics.X86.Avx.IsSupported
            && System.Runtime.Intrinsics.X86.Sse41.IsSupported;
        // Hashes are frozen by the existing independently checked complete
        // bicubic controls on this exact original raster, before this change.
        var expectedHash = widthOverride == 32 && heightOverride == 16 && depth == 16
            ? "56948d8eacf081102f915276cdbd5daac3b2414f07ac93e3cf9f91fac5e5f053"
            : vertical && heightOverride > 0 ? (depth == 8
            ? "705c36dd72e49076298952566b7e68b7922cb649c45d4dbf4cf68d9c6cd2887d"
            : "c9f1c8cc6a1a0bbfc4f8b5de0bfabc79484c81d5aa062a3b2630a81fba0f6676")
            : horizontal ? (depth == 8
            ? "ef9e67095fcbc4a0d9a4fb2591577e9141a188e6b6323a44073cfd2ea41b7724"
            : "12073f9f1f3c8aa21fd50826350ec72021c8252923f9f52055302cb811405a35")
            : crossHeight > 0 && depth == 8 ? (crossHeight == 259
            ? "a5d2e75dbc30006f845c8a69061e096ded94ed06248e50350ec2c18456606a20"
            : "46b86650bc27febb8c0ed688a02591b660fe77b099c5b9e2ffe42a94e38cae5e")
            : crossHeight > 0 ? (crossHeight == 259
            ? "ef78fb7dbfb05cb1a756a64702961cbe60abd5046fa5f2b7cc6fb91b3072cceb"
            : "ee02e57ff550047bf5296fb5af51c9396005653a8d37c3e0032abcdbb6db0db1")
            : packedReduction ? (depth == 16
            ? "59db672430bfd4c9fc4468faae4ae846a730320b185be54196676589b81fb5ce"
            : "e2340f662ce94418bed4e7be5c65ac4c73fb7a705b047af06784e42b37c267c6")
            : reduction ? "656bb2248362c88dff8fe510dcc21055155976a3dd2b33aa20c48d2b036f59a8"
            : depth == 8 ? (vertical
            ? "56ff0d8276c6427bb3ad49e8d3af6ad26cfb99b27db697580471ac273fcd7697"
            : "1ca9938539ba1421fe4f1540055be5d0675ac8ddc1ea87df5932a93ca35a0356")
            : vertical ? "275a5c3d38d7ba7e6a1ca713fb69fd2ceaf665c18db8269f0bcc1d3e58dcd6fd"
            : "6e289a9a8316325b78988adbad8ab7102c81e302216b79c45ef086410f6f691a";
        if (Convert.ToHexStringLower(System.Security.Cryptography.SHA256.HashData(expected)) != expectedHash
            || root.Bytes != 2L * outputBytes || root.Peak > 2L * outputBytes + workspaceBytes
            || vectorBands && root.Peak != 2L * outputBytes + workspaceBytes
            || !expected.AsSpan().SequenceEqual(OriginalPixels.Bytes(first)))
            throw new Exception($"Packaged mixed-word rows changed frozen pixels or retained admission: workspace={workspaceBytes}, hash={Convert.ToHexStringLower(System.Security.Cryptography.SHA256.HashData(expected))}, retained={root.Bytes}, peak={root.Peak}.");
        using (var caller = root.CreateScope())
        {
            // Fill spare capacity explicitly: portable paths may need less scratch.
            if (!caller.TryReserve(workspaceBytes, out var held))
                throw new Exception("Packaged bicubic refusal setup differs.");
            using (held)
            {
                try
                {
                    using var unexpected = ImageTransforms.Resize(source, width, height, ResizeFilter.Bicubic, limits);
                    throw new Exception("Packaged mixed-word rows ignored aggregate admission.");
                }
                catch (ImageLimitException error) when (error.Scope == ImageLimitScope.SharedBudget) { }
            }
        }
        if (root.Bytes != 2L * outputBytes) throw new Exception("Packaged mixed-word refusal changed surviving ownership.");
        if (depth == 16) first.GetRow16(0)[0] ^= 65535;
        else first.GetRow8(0)[0] ^= 255;
        first.Dispose();
        if (root.Bytes != outputBytes || !expected.AsSpan().SequenceEqual(OriginalPixels.Bytes(second)))
            throw new Exception("Packaged mixed-word owners alias.");
        second.Dispose();
        using var token = new CancellationTokenSource(); root.Admitted = _ => token.Cancel();
        try
        {
            using var unexpected = ImageTransforms.Resize(source, width, height, ResizeFilter.Bicubic, limits, token.Token);
            throw new Exception("Packaged mixed-word rows ignored cancellation.");
        }
        catch (OperationCanceledException error) when (error.CancellationToken == token.Token) { }
        root.Admitted = null;
        if (root.Bytes != 0) throw new Exception("Packaged mixed-word cancellation leaked ownership.");
        using (var recovered = ImageTransforms.Resize(source, width, height, ResizeFilter.Bicubic, limits))
            if (!expected.AsSpan().SequenceEqual(OriginalPixels.Bytes(recovered)))
                throw new Exception("Packaged mixed-word recovery changed pixels.");
        if (root.Bytes != 0 || root.IsDisposed || !input.AsSpan().SequenceEqual(OriginalPixels.Bytes(source)))
            throw new Exception("Packaged mixed-word rows lost source or borrowed-root ownership.");
        Console.WriteLine(widthOverride == 32
            ? "Packaged deep word bicubic preserves frozen pixels, aggregate admission, cancellation and independent survivors."
            : vertical && heightOverride > 0
            ? "Packaged vertical byte/word enlargement preserves frozen pixels, aggregate admission, cancellation and independent survivors."
            : horizontal
            ? "Packaged horizontal byte/word enlargement preserves frozen pixels, aggregate admission, cancellation and independent survivors."
            : crossHeight > 0 && depth == 8
            ? "Packaged output-backed byte cross prefix preserves frozen pixels, aggregate admission, cancellation and independent survivors."
            : crossHeight > 0
            ? "Packaged identity/general word bicubic preserves frozen pixels, aggregate admission, cancellation and independent survivors."
            : packedReduction
            ? "Packaged packed byte bicubic preserves frozen pixels, aggregate admission, cancellation and independent survivors."
            : reduction
            ? "Packaged byte column strips preserve frozen pixels, aggregate admission, cancellation and independent survivors."
            : depth == 8
            ? "Packaged byte bands preserve frozen pixels, aggregate admission, cancellation and independent survivors."
            : vertical
            ? "Packaged vertical word geometry preserves frozen pixels, aggregate admission, cancellation and independent survivors."
            : "Packaged mixed-word scheduling preserves frozen pixels, aggregate admission, cancellation and independent survivors.");
        if (depth == 16 && workspaceBytes == 8192 && !vertical)
        {
            Console.WriteLine("Packaged8KiB mixed-word boundary preserves exact samples and admitted lifetimes.");
            Console.WriteLine($"Packaged mixed-word vector execution active: {vectorBands}.");
        }
    }

    private static async Task VerifyPngCursors(bool cancellable)
    {
        foreach (var depth in new[] { 8, 16 })
        {
            using var source = OriginalPixels.Buffer(513, 259, depth);
            var expected = OriginalPixels.Bytes(source);
            using var encodedOutput = new MemoryStream();
            ImageCodec.EncodePng(source, encodedOutput);
            var encoded = encodedOutput.ToArray(); var saved = encoded.ToArray();
            using var cancellation = new CancellationTokenSource();
            var token = cancellable ? cancellation.Token : default;
            using var first = ImageCodec.Decode(encoded, cancellationToken: token);
            using var second = ImageCodec.Decode(encoded, cancellationToken: token);
            using var input = new MemoryStream(encoded, writable: false);
            using var asynchronous = await ImageCodec.DecodeAsync(input, cancellationToken: token);
            if (!expected.AsSpan().SequenceEqual(OriginalPixels.Bytes(first))
                || !expected.AsSpan().SequenceEqual(OriginalPixels.Bytes(second))
                || !expected.AsSpan().SequenceEqual(OriginalPixels.Bytes(asynchronous))
                || !encoded.AsSpan().SequenceEqual(saved) || !input.CanRead)
                throw new Exception("Packaged live PNG cursor changed samples, input or borrowed stream ownership.");
            VerifyPngCursorAdmission(encoded, expected, expected.Length, token);
            if (depth == 8) first.GetRow8(0)[0] ^= 255; else first.GetRow16(0)[0] ^= 65535;
            encoded.AsSpan().Clear();
            if (!expected.AsSpan().SequenceEqual(OriginalPixels.Bytes(second))
                || !expected.AsSpan().SequenceEqual(OriginalPixels.Bytes(asynchronous)))
                throw new Exception("Packaged live PNG cursor outputs alias caller input or other owners.");
        }

        // RGB exercises materialized framing; governed RGBA above exercises
        // bounded rolling history. Both use the borrowed shared root.
        const int outputBytes = 513 * 259 * 4;
        using var opaque = OriginalPixels.Buffer(513, 259, 8, opaque: true);
        var opaqueExpected = OriginalPixels.Bytes(opaque);
        using var output = new MemoryStream();
        ImageCodec.EncodePng(opaque, output, new PngEncodeOptions { ColorType = PngColorType.Rgb });
        var packet = output.ToArray(); var packetSaved = packet.ToArray();
        using var live = new CancellationTokenSource();
        VerifyPngCursorAdmission(packet, opaqueExpected, outputBytes, cancellable ? live.Token : default);
        if (!packetSaved.AsSpan().SequenceEqual(packet)) throw new Exception("Packaged PNG cursor changed borrowed input.");
        Console.WriteLine(cancellable
            ? "Packaged live PNG cursors preserve exact byte/word sync/async samples, shared admission and independent survivors."
            : "Packaged ordinary PNG cursors preserve exact byte/word sync/async samples, shared admission and independent survivors.");
    }

    private static void VerifyPngCursorAdmission(byte[] packet, byte[] opaqueExpected, int outputBytes, CancellationToken token)
    {
        var packetSaved = packet.ToArray();
        using var probe = new TestMemoryBudget(8 * 1024 * 1024);
        using (var image = ImageCodec.Decode(packet, new() { Limits = new() { MemoryBudget = probe } }, token))
            if (!opaqueExpected.AsSpan().SequenceEqual(OriginalPixels.Bytes(image)))
                throw new Exception("Packaged governed PNG cursor changed samples.");
        if (probe.Bytes != 0 || probe.IsDisposed || probe.Peak <= outputBytes)
            throw new Exception("Packaged PNG cursor did not release its framing storage.");
        using var root = new TestMemoryBudget(probe.Peak + outputBytes);
        var options = new DecodeOptions { Limits = new() { MemoryBudget = root } };
        var borrowedPacket = packet.ToArray();
        using var held = ImageCodec.Decode(borrowedPacket, options, token);
        using var survivor = ImageCodec.Decode(borrowedPacket, options, token);
        borrowedPacket.AsSpan().Clear();
        if (root.Bytes != 2L * outputBytes || root.Peak != probe.Peak + outputBytes)
            throw new Exception("Packaged PNG cursor changed retained capacity or overlapping admission.");
        var refused = false;
        try { using var unexpected = ImageCodec.Decode(packet, options, token); }
        catch (ImageLimitException error) when (error.Scope == ImageLimitScope.SharedBudget) { refused = true; }
        if (!refused || root.Refusals != 1 || root.Bytes != 2L * outputBytes)
            throw new Exception("Packaged PNG cursor did not clean up terminal admission refusal.");
        if (held.BitDepth == 8) held.GetRow8(0)[0] ^= 255; else held.GetRow16(0)[0] ^= 65535;
        held.Dispose();
        using (var recovered = ImageCodec.Decode(packet, options, token))
            if (!opaqueExpected.AsSpan().SequenceEqual(OriginalPixels.Bytes(recovered)))
                throw new Exception("Packaged PNG cursor failed after capacity was released.");
        if (!packet.AsSpan().SequenceEqual(packetSaved)) throw new Exception("Packaged PNG cursor mutated borrowed input.");
        if (!opaqueExpected.AsSpan().SequenceEqual(OriginalPixels.Bytes(survivor)))
            throw new Exception("Packaged PNG cursor lost a surviving owner.");
        survivor.Dispose();
        if (root.Bytes != 0 || root.IsDisposed) throw new Exception("Packaged PNG cursor lost borrowed-root ownership.");
        using var cancellation = new CancellationTokenSource(); root.Admitted = _ => cancellation.Cancel();
        try
        {
            using var unexpected = ImageCodec.Decode(packet, options, cancellation.Token);
            throw new Exception("Packaged shared PNG cursor ignored cancellation.");
        }
        catch (OperationCanceledException error) when (error.CancellationToken == cancellation.Token) { }
        root.Admitted = null;
        if (root.Bytes != 0 || root.IsDisposed) throw new Exception("Packaged shared PNG cursor cancellation leaked ownership.");
        using (var recovered = ImageCodec.Decode(packet, options, token))
            if (!opaqueExpected.AsSpan().SequenceEqual(OriginalPixels.Bytes(recovered)))
                throw new Exception("Packaged shared PNG cursor did not recover after cancellation.");
        if (root.Bytes != 0 || root.IsDisposed) throw new Exception("Packaged shared PNG cursor retained released storage.");
        Console.WriteLine("Packaged shared PNG cursors preserve exact samples, bounded rows, aggregate admission, refusal, cancellation and independent survivors.");
    }

    private static void VerifyOwnedFonts()
    {
        var input = AuthoredTrueType.Build();
        var font = TrueTypeFont.Load(input);
        using var layout = TextLayout.Create(font, "Ag A\u0301", new() { FontSize = 20 });
        using var before = ImageBuffer.Create(100, 40, 16);
        ImageDrawing.DrawText(before, layout, 1, 1, RgbaColor.From8(123, 234, 45));
        var expected = OriginalPixels.Bytes(before);
        input.AsSpan().Clear(); font.Dispose(); font.Dispose();
        using var bold = TrueTypeFont.Load(AuthoredTrueType.Build(style: FontStyle.Bold));
        if (bold.GetGlyphMetrics(2).AdvanceWidth != 660) throw new Exception("Packaged replacement font differs.");
        using var after = ImageBuffer.Create(100, 40, 16);
        ImageDrawing.DrawText(after, layout, 1, 1, RgbaColor.From8(123, 234, 45));
        if (!expected.AsSpan().SequenceEqual(OriginalPixels.Bytes(after)))
            throw new Exception("Packaged layout borrowed disposed or reused font storage.");
        var valid = AuthoredTrueType.Build();
        using var root = new TestMemoryBudget(valid.Length + 256L);
        var limits = new ImageLimits { MemoryBudget = root };
        var governed = TrueTypeFont.Load(valid, limits);
        try
        {
            using var unexpected = TrueTypeFont.Load(valid, limits);
            throw new Exception("Packaged retained font ignored aggregate admission.");
        }
        catch (ImageLimitException error) when (error.Scope == ImageLimitScope.SharedBudget) { }
        if (root.Bytes != governed.StorageBytes) throw new Exception("Packaged refused font changed retained ownership.");
        governed.Dispose();
        using (var recovered = TrueTypeFont.Load(valid, limits))
            if (root.Bytes != recovered.StorageBytes) throw new Exception("Packaged font recovery was not charged.");
        if (root.Bytes != 0 || root.IsDisposed) throw new Exception("Packaged font disposed the borrowed budget or leaked capacity.");
        Console.WriteLine("Packaged font storage preserves independent faces/layouts, aggregate admission and recovery.");
    }

    private static async Task VerifyDistortionInstructionStorage()
    {
        using var source = OriginalVp8lControls.Buffer("diagram", 8);
        var unchanged = OriginalPixels.Bytes(source);
        using var root = new TestMemoryBudget(512 * 1024);
        var options = new WebpEncodeOptions { Lossless = false, Quality = 75, Effort = 2,
            Limits = new() { MemoryBudget = root } };
        using var output = new MemoryStream();
        await ImageCodec.EncodeWebpAsync(source, output, options);
        var packet = output.ToArray();
        if (packet.Length != 7330 || Convert.ToHexStringLower(System.Security.Cryptography.SHA256.HashData(packet))
            != "325a9d5b55b300db2554681735642edcdc8dcb866a88760501d3fe95fac1423b" || root.Bytes != 0 || root.IsDisposed)
            throw new Exception("Packaged method2 encoding changed its exact packet or bounded ownership.");
        using var canceled = new CancellationTokenSource(); root.Admitted = _ => canceled.Cancel();
        var stopped = false;
        try { await ImageCodec.EncodeWebpAsync(source, Stream.Null, options, canceled.Token); }
        catch (OperationCanceledException error) when (error.CancellationToken == canceled.Token) { stopped = true; }
        root.Admitted = null;
        if (!stopped || root.Bytes != 0) throw new Exception("Packaged method2 cancellation leaked storage.");
        using var recovered = new MemoryStream(); ImageCodec.EncodeWebp(source, recovered, options);
        if (root.Bytes != 0 || root.IsDisposed || !packet.AsSpan().SequenceEqual(recovered.ToArray())
            || !unchanged.AsSpan().SequenceEqual(OriginalPixels.Bytes(source)) || !output.CanWrite)
            throw new Exception("Packaged method2 recovery changed input, output or budget ownership.");
        Console.WriteLine("Packaged method2 residual storage preserves exact packets, bounded admission, cancellation and independent output.");
    }

    private static async Task VerifyLosslessValidationStorage()
    {
        foreach (var lossy in new[] { false, true })
        {
            using var source = OriginalPixels.Buffer(513, 259, 8);
            source.GetRow8(0)[3] = 0;
            using var encoded = new MemoryStream();
            ImageCodec.EncodeWebp(source, encoded, new() { Lossless = !lossy, Quality = 85, Effort = 4 });
            var packet = encoded.ToArray(); var unchanged = packet.ToArray();
            var expectedHash = lossy ? "183b90c4faff3b575ae98cb770d5d3fb3a9066fbf815dba04cdb4b392ea1af71"
                : "db796bb9ad5d480eb273db8ae966d4fa1a8f5995bceaa724475c09d6201dbfb7";
            if (Convert.ToHexStringLower(System.Security.Cryptography.SHA256.HashData(packet)) != expectedHash)
                throw new Exception("Packaged validation source packet differs.");
            var expected = ImageCodec.Identify(packet);
            using var root = new TestMemoryBudget(256 * 1024);
            var limits = new ImageLimits { MemoryBudget = root, MaxWorkingBytes = 1024 * 1024 };
            using var live = new CancellationTokenSource();
            if (ImageCodec.ValidatePixels(packet, limits) != expected
                || ImageCodec.ValidatePixels(packet, limits, live.Token) != expected || root.Bytes != 0 || root.IsDisposed)
                throw new Exception("Packaged validation retained discarded raster storage or changed metadata.");
            var refused = false;
            try { using var image = ImageCodec.Decode(packet, new() { Limits = limits }); }
            catch (ImageLimitException error) when (error.Scope == ImageLimitScope.SharedBudget) { refused = true; }
            if (!refused || root.Bytes != 0) throw new Exception("Packaged full decode ignored admission or leaked ownership.");
            using var canceled = new CancellationTokenSource(); root.Admitted = _ => canceled.Cancel();
            var stopped = false;
            try { ImageCodec.ValidatePixels(packet, limits, canceled.Token); }
            catch (OperationCanceledException error) when (error.CancellationToken == canceled.Token) { stopped = true; }
            root.Admitted = null;
            if (!stopped || root.Bytes != 0 || ImageCodec.ValidatePixels(packet, limits) != expected)
                throw new Exception("Packaged validation cancellation leaked storage or prevented recovery.");
            using var asyncRoot = new TestMemoryBudget(4 * 1024 * 1024);
            using var input = new MemoryStream(packet, writable: false);
            if (await ImageCodec.ValidatePixelsAsync(input, new() { MemoryBudget = asyncRoot }, live.Token) != expected
                || asyncRoot.Bytes != 0 || asyncRoot.IsDisposed || !input.CanRead || !unchanged.AsSpan().SequenceEqual(packet))
                throw new Exception("Packaged asynchronous validation changed input ownership or retained storage.");
        }
        Console.WriteLine("Packaged main validation preserves exact packets/metadata, bounded admission, cancellation and asynchronous ownership.");
    }

    private static async Task VerifyProgressivePlaneOwnership(JpegSampling sampling = JpegSampling.YCbCr444)
    {
        const int width = 31, height = 23, frameBytes = width * height * 4;
        var data = AuthoredJpeg.ProgressiveFrame(width, height, sampling, 3);
        var original = data.ToArray();
        using var root = new TestMemoryBudget(128 * 1024);
        var options = new DecodeOptions { Limits = new() { MemoryBudget = root } };
        var caller = new byte[width * height * 3];
        ImageCodec.DecodeJpeg(data, caller, JpegSampleFormat.Rgb, options: new() { Limits = options.Limits });
        using var live = new CancellationTokenSource();
        var start = root.GrowthAttempts;
        using var first = ImageCodec.Decode(data, options, live.Token);
        var outputGrowth = root.GrowthAttempts - start;
        var expected = OriginalPixels.Bytes(first);
        for (var i = 0; i < width * height; i++)
            if (!caller.AsSpan(i * 3, 3).SequenceEqual(expected.AsSpan(i * 4, 3)) || expected[i * 4 + 3] != 255)
                throw new Exception("Packaged progressive strip output differs from caller samples.");
        using var input = new MemoryStream(data, writable: false);
        using var second = await ImageCodec.DecodeAsync(input, options, live.Token);
        if (root.Bytes != 2 * frameBytes || !input.CanRead || !original.AsSpan().SequenceEqual(data)
            || !expected.AsSpan().SequenceEqual(OriginalPixels.Bytes(second)))
            throw new Exception("Packaged progressive strip async ownership differs.");
        first.Dispose();
        foreach (var cancel in new[] { false, true })
        {
            using var token = new CancellationTokenSource();
            var stop = root.GrowthAttempts + outputGrowth;
            if (cancel) root.Admitted = _ => { if (root.GrowthAttempts == stop) token.Cancel(); };
            else root.RefuseAtGrowth = stop;
            var failed = false;
            try { using var unwanted = ImageCodec.Decode(data, options, token.Token); }
            catch (OperationCanceledException e) when (cancel && e.CancellationToken == token.Token) { failed = true; }
            catch (ImageLimitException e) when (!cancel && e.Scope == ImageLimitScope.SharedBudget) { failed = true; }
            root.Admitted = null; root.RefuseAtGrowth = 0;
            if (!failed || root.Bytes != frameBytes || root.IsDisposed || !original.AsSpan().SequenceEqual(data))
                throw new Exception("Packaged progressive strip refusal/cancellation leaked ownership.");
        }
        using var recovered = ImageCodec.Decode(data, options);
        data.AsSpan().Clear();
        if (root.Bytes != 2 * frameBytes || !expected.AsSpan().SequenceEqual(OriginalPixels.Bytes(second))
            || !expected.AsSpan().SequenceEqual(OriginalPixels.Bytes(recovered)))
            throw new Exception("Packaged progressive strip survivor differs after input disposal.");
        recovered.Dispose(); second.Dispose();
        if (root.Bytes != 0 || root.IsDisposed) throw new Exception("Packaged progressive strip storage leaked.");
        Console.WriteLine("Packaged progressive strips preserve exact samples, async, terminal refusal and surviving ownership.");
    }

    private static async Task VerifyOpaqueWordTiles()
    {
        using var root = new TestMemoryBudget(8 * 1024 * 1024);
        using var source = OriginalPixels.Buffer(513, 259, 16, opaque: true);
        var original = OriginalPixels.Bytes(source);
        var options = new WebpEncodeOptions { Lossless = false, Quality = 75, Effort = 4,
            Limits = new() { MemoryBudget = root } };
        using var first = new MemoryStream(); ImageCodec.EncodeWebp(source, first, options);
        var packet = first.ToArray();
        if (packet.Length != 72380 || Convert.ToHexStringLower(System.Security.Cryptography.SHA256.HashData(packet))
            != "39eb4095d1aaebc21fee2a92aa45180fb6adcd92bc5538589f914f53aa9ed88f" || root.Bytes != 0)
            throw new Exception("Packaged opaque word preparation changed its complete packet or retained admission.");
        // The frozen packet also exercises the live coefficient model with
        // owned sync/async results, compared with the unchanged ordinary reader.
        using (var ordinary = ImageCodec.Decode(packet))
        using (var live = new CancellationTokenSource())
        {
            var expected = OriginalPixels.Bytes(ordinary);
            var decodeInput = packet.ToArray();
            var decodeOptions = new DecodeOptions { Limits = new() { MemoryBudget = root } };
            using var decoded = ImageCodec.Decode(decodeInput, decodeOptions, live.Token);
            using var input = new MemoryStream(decodeInput, writable: false);
            using var awaited = await ImageCodec.DecodeAsync(input, decodeOptions, live.Token);
            decodeInput.AsSpan().Clear();
            if (root.Bytes != 2L * 513 * 259 * 4 || !input.CanRead || root.IsDisposed
                || !expected.AsSpan().SequenceEqual(OriginalPixels.Bytes(decoded))
                || !expected.AsSpan().SequenceEqual(OriginalPixels.Bytes(awaited)))
                throw new Exception("Packaged live VP8 model changed pixels, input ownership or retained results.");
        }
        if (root.Bytes != 0 || root.IsDisposed)
            throw new Exception("Packaged live VP8 model leaked admission or disposed the borrowed root.");
        Console.WriteLine("Packaged live VP8 model preserves exact sync/async frames and independent admitted ownership.");
        foreach (var offset in new long[] { 1, 2 })
        {
            using var refused = new MemoryStream(); var refusals = root.Refusals;
            root.RefuseAtGrowth = root.GrowthAttempts + offset;
            try { ImageCodec.EncodeWebp(source, refused, options); throw new Exception("Packaged word tiles ignored refusal."); }
            catch (ImageLimitException e) when (e.Scope == ImageLimitScope.SharedBudget) { }
            if (root.Bytes != 0 || root.Refusals != refusals + 1 || refused.Length != 0 || !refused.CanWrite)
                throw new Exception("Packaged word tiles leaked admission, retried or took output ownership.");
            root.RefuseAtGrowth = 0;
        }
        using var cancel = new CancellationTokenSource(); var stop = root.GrowthAttempts + 2;
        root.Admitted = _ => { if (root.GrowthAttempts == stop) cancel.Cancel(); };
        using var canceled = new MemoryStream();
        try { ImageCodec.EncodeWebp(source, canceled, options, cancel.Token); throw new Exception("Packaged word tiles ignored cancellation."); }
        catch (OperationCanceledException) { }
        root.Admitted = null;
        using var second = new MemoryStream(); await ImageCodec.EncodeWebpAsync(source, second, options);
        if (root.Bytes != 0 || root.IsDisposed || canceled.Length != 0 || !canceled.CanWrite
            || !packet.AsSpan().SequenceEqual(second.ToArray()) || !original.AsSpan().SequenceEqual(OriginalPixels.Bytes(source)))
            throw new Exception("Packaged word tiles changed async output, source or cleanup.");
        source.Dispose(); first.GetBuffer()[10] ^= 1; first.Dispose();
        if (!packet.AsSpan().SequenceEqual(second.ToArray()) || !second.CanWrite)
            throw new Exception("Packaged word output did not survive source and peer disposal.");
        Console.WriteLine("Packaged opaque word tiles preserve exact sync/async VP8 packets, refusal and ownership.");
    }

    private static async Task VerifyWebpAlphaCoefficientStorage()
    {
        foreach (var depth in new[] { 8, 16 })
        {
            using var root = new TestMemoryBudget(8 * 1024 * 1024);
            using var source = OriginalPixels.Buffer(513, 259, depth);
            if (depth == 8) source.GetRow8(0)[3] = source.GetRow8(0)[513 * 4 - 1] = 0;
            else source.GetRow16(0)[3] = source.GetRow16(0)[513 * 4 - 1] = 0;
            var unchanged = OriginalPixels.Bytes(source);
            var options = new WebpEncodeOptions { Lossless = false, Quality = 75, Effort = 4,
                Limits = new() { MemoryBudget = root } };
            using var first = new MemoryStream();
            var start = root.GrowthAttempts;
            ImageCodec.EncodeWebp(source, first, options);
            var growth = root.GrowthAttempts - start;
            var packet = first.ToArray();
            if (packet.Length != 98590 || Convert.ToHexStringLower(System.Security.Cryptography.SHA256.HashData(packet))
                != "8fcc1c935850d7813ebfce329a8b00c41fe9b02683d2be40b91dedfd74192a10" || root.Bytes != 0)
                throw new Exception("Packaged ALPH/frame storage changed the complete packet or retained admission.");
            foreach (var offset in new long[] { 1, 2, growth })
            {
                using var refused = new MemoryStream();
                var refusals = root.Refusals; root.RefuseAtGrowth = root.GrowthAttempts + offset;
                try { ImageCodec.EncodeWebp(source, refused, options); throw new Exception("Packaged ALPH/frame storage ignored refusal."); }
                catch (ImageLimitException e) when (e.Scope == ImageLimitScope.SharedBudget) { }
                if (root.Bytes != 0 || root.Refusals != refusals + 1 || refused.Length != 0 || !refused.CanWrite)
                    throw new Exception("Packaged ALPH/frame refusal leaked ownership or retried.");
                root.RefuseAtGrowth = 0;
            }
            using var admittedCancel = new CancellationTokenSource(); var stop = root.GrowthAttempts + 2;
            root.Admitted = _ => { if (root.GrowthAttempts == stop) admittedCancel.Cancel(); };
            using var interrupted = new MemoryStream();
            try { ImageCodec.EncodeWebp(source, interrupted, options, admittedCancel.Token); throw new Exception("Packaged alpha tiles ignored cancellation after admission."); }
            catch (OperationCanceledException) { }
            root.Admitted = null;
            if (root.Bytes != 0 || interrupted.Length != 0 || !interrupted.CanWrite)
                throw new Exception("Packaged alpha tile cancellation leaked storage or took output ownership.");
            using var second = new MemoryStream(); await ImageCodec.EncodeWebpAsync(source, second, options);
            using var canceled = new MemoryStream();
            try { await ImageCodec.EncodeWebpAsync(source, canceled, options, new CancellationToken(true)); throw new Exception("Packaged ALPH/frame ignored cancellation."); }
            catch (OperationCanceledException) { }
            if (!packet.AsSpan().SequenceEqual(second.ToArray()) || !unchanged.AsSpan().SequenceEqual(OriginalPixels.Bytes(source))
                || root.Bytes != 0 || root.IsDisposed || canceled.Length != 0 || !canceled.CanWrite)
                throw new Exception("Packaged ALPH/frame storage changed async output, source, cleanup or caller ownership.");
            source.Dispose(); first.Dispose();
            if (!packet.AsSpan().SequenceEqual(second.ToArray()) || !second.CanWrite)
                throw new Exception("Packaged ALPH/frame output did not survive source and peer disposal.");
            Console.WriteLine(depth == 8
                ? "Packaged ALPH/frame storage preserves exact sync/async packets, terminal refusal and independent ownership."
                : "Packaged shared alpha preparation preserves exact word packets, cancellation and surviving output.");
        }
    }

    private static void VerifyStreamingBicubic()
    {
        foreach (var (depth, width, height, expectedHash) in new[]
        {
            (8, 16, 8, "eec2e89602720fa8ca330733763524017f49de642fb341ba839aeca0aca887fd"),
            (8, 8, 4, "5ebc1d60ad1c4bcaeeb105d2f5f83285b0dbac88ebbc83cb2f04732ee134b4bc"),
            (8, 8, 17, "481df76194da6537d6ecfbc24f37c842ae750b7752791f35d40d43454532eee1"),
            (16, 8, 17, "9f1aaa6ef72a650982022d0a36563789f9ae50d79a094f97141af8ed50051b6c"),
            (8, 4, 9, "df49ddc41185b3feaa2a0fc283fb3cd2432e463aa4aa7294a041cd76036effd1"),
            (16, 4, 9, "367e7576a57e596e44740e0f6b4e66c72772689e2d784011e29e39608bcdff76"),
            (8, 256, 129, "656bb2248362c88dff8fe510dcc21055155976a3dd2b33aa20c48d2b036f59a8"),
            (16, 256, 129, "2676207be4d1a7c9109061fc3c8eb5a0d6b8d10a97bbbc611fbebda3330ffd9f"),
            (8, 128, 64, "5067190b92fe644a75b0001848c4a555feb638c72592239dbf0ea0d63c518e71"),
            (16, 128, 64, "3763385e1f45803392214ccf0b87ef6e4f1ec22f2f37701108cc52e9a20209d9")
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
        Console.WriteLine("Packaged word column bicubic preserves frozen pixels, 4KiB admission and independent survivors.");
        Console.WriteLine("Packaged wide column bicubic preserves frozen byte/word pixels, 4KiB admission and independent survivors.");
    }

    private static void VerifyCompositingRows()
    {
        const int width = 7; // A complete four-pixel vector and scalar suffix.
        foreach (var sourceDepth in new[] { 8, 16 })
        foreach (var canvasDepth in new[] { 8, 16 })
        {
            using var root = new TestMemoryBudget(4096); var limits = new ImageLimits { MemoryBudget = root };
            using var source = ImageBuffer.Create(width, 1, sourceDepth, limits);
            using var canvas = ImageBuffer.Create(width, 1, canvasDepth, limits);
            var sourceMax = sourceDepth == 8 ? 255 : 65535; var canvasMax = canvasDepth == 8 ? 255 : 65535;
            for (var x = 0; x < width; x++)
            {
                var pixel = x % 3;
                for (var c = 0; c < 3; c++)
                {
                    Set(source, x, c, (37 + pixel * 47 + c * 59) * (sourceDepth == 8 ? 1 : 257));
                    Set(canvas, x, c, (229 - pixel * 43 - c * 67) * (canvasDepth == 8 ? 1 : 257));
                }
                Set(source, x, 3, pixel == 0 ? 0 : pixel == 1 ? sourceMax / 3 : sourceMax);
                Set(canvas, x, 3, pixel == 0 ? canvasMax : pixel == 1 ? canvasMax / 2 : 1);
            }
            var expected = new int[width * 4]; var original = OriginalPixels.Bytes(source);
            for (var x = 0; x < width; x++)
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
            for (var x = 0; x < width; x++) for (var c = 0; c < 4; c++)
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
        foreach (var (width, height, depth) in new[] { (17, 11, 8), (17, 11, 16), (65, 39, 16) })
        {
            using var root = new TestMemoryBudget(3 * 1024 * 1024);
            var limits = new ImageLimits { MemoryBudget = root };
            using var original = OriginalPixels.Buffer(31, 19, depth);
            using var source = original.Clone(limits);
            using var plan = Lanczos3ResizePlan.Create(31, 19, width, height, depth, limits);
            if (root.Bytes != source.Stride * source.Height + plan.StorageBytes)
                throw new Exception("Packaged Lanczos preparation lost its retained admission.");
            using var first = plan.Resize(source);
            using var changed = OriginalPixels.Buffer(31, 19, depth, phase: 2);
            for (var y = 0; y < source.Height; y++)
                if (depth == 8) changed.GetRow8(y).CopyTo(source.GetRow8(y));
                else changed.GetRow16(y).CopyTo(source.GetRow16(y));
            using var expected = ImageTransforms.Resize(changed, width, height, ResizeFilter.Lanczos3);
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
        VerifyLanczosPendingOutput();
    }

    private static void VerifyLanczosPendingOutput()
    {
        if (!(System.Runtime.Intrinsics.X86.Avx512F.IsSupported
            && System.Runtime.Intrinsics.X86.Avx512DQ.IsSupported
            && System.Runtime.Intrinsics.X86.Avx512DQ.VL.IsSupported
            && System.Runtime.Intrinsics.X86.Avx2.IsSupported
            && System.Runtime.Intrinsics.X86.Fma.IsSupported
            && System.Runtime.Intrinsics.X86.Sse41.IsSupported)) return;
        using var source = OriginalPixels.Buffer(39, 9, 16);
        using var probe = Lanczos3ResizePlan.Create(39, 9, 19, 4, 16);
        const int outputBytes = 19 * 4 * 8;
        // Leave room for the pending result, rather than selecting optional
        // product storage that is sixteen bytes too large at this boundary.
        var limit = (long)source.Stride * source.Height + probe.StorageBytes - 39 * 16L + 2L * outputBytes;
        using var expected = ImageTransforms.Resize(source, 19, 4, ResizeFilter.Lanczos3);
        using var root = new TestMemoryBudget(1 << 20);
        using (var actual = ImageTransforms.Resize(source, 19, 4, ResizeFilter.Lanczos3,
            new ImageLimits { MemoryBudget = root, MaxWorkingBytes = limit }))
        {
            if (!OriginalPixels.Bytes(expected).AsSpan().SequenceEqual(OriginalPixels.Bytes(actual))
                || root.Bytes != outputBytes)
                throw new Exception("Packaged Lanczos pending output lost exact pixels or retained admission.");
        }
        if (root.Bytes != 0 || root.IsDisposed)
            throw new Exception("Packaged Lanczos pending output leaked storage or disposed its borrowed root.");
        Console.WriteLine("Packaged Lanczos local ceiling preserves pending output and independent admitted ownership.");
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
        using var live = new CancellationTokenSource();
        using var first = ImageCodec.Decode(data, options, live.Token);
        using var stream = new MemoryStream(data, writable: false);
        using var second = await ImageCodec.DecodeAsync(stream, options, live.Token);
        if (root.Bytes != data.Length + 2L * bytes || !stream.CanRead || root.IsDisposed)
            throw new Exception("Packaged RGB result or stream ownership differs.");
        try
        {
            using var unexpected = ImageCodec.Decode(data, options, live.Token);
            throw new Exception("Packaged RGB ignored aggregate refusal.");
        }
        catch (ImageLimitException error) when (error.Scope == ImageLimitScope.SharedBudget) { }
        if (root.Refusals != 1 || root.Bytes != data.Length + 2L * bytes)
            throw new Exception("Packaged RGB admission was retried or leaked storage.");
        first.Dispose();
        using var recovered = ImageCodec.Decode(data, options, live.Token);
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
        Console.WriteLine("Packaged live baseline JPEG window preserves exact samples and admitted sync/async ownership.");
    }

    private static async Task VerifyGroupedJpegResult(bool cancelable)
    {
        // Original T.81 stream: RGB, zero DC, two signed AC coefficients and EOB
        // perblock. Frozen on the qualified PFC554 decoder before this change.
        const int width = 513, height = 259, bytes = width * height * 4;
        using var encoded = new MemoryStream(); encoded.Write(new byte[] { 255, 216 });
        void Segment(byte marker, byte[] payload)
        {
            encoded.Write(new byte[] { 255, marker, (byte)((payload.Length + 2) >> 8), (byte)(payload.Length + 2) });
            encoded.Write(payload);
        }
        Segment(238, [65, 100, 111, 98, 101, 0, 100, 0, 0, 0, 0, 0]);
        Segment(219, [0, .. Enumerable.Repeat((byte)64, 64)]);
        Segment(192, [8, 1, 3, 2, 1, 3, 82, 17, 0, 71, 17, 0, 66, 17, 0]);
        Segment(196, [0, 1, .. new byte[15], 0]);
        Segment(196, [16, 1, 1, .. new byte[14], 1, 0]);
        Segment(218, [3, 82, 0, 71, 0, 66, 0, 0, 63, 0]);
        var pending = 0; var count = 0;
        void Bit(int bit)
        {
            pending = pending * 2 + bit;
            if (++count != 8) return;
            encoded.WriteByte((byte)pending); if (pending == 255) encoded.WriteByte(0);
            pending = 0; count = 0;
        }
        for (var block = 0; block < 65 * 33 * 3; block++)
            foreach (var bit in new[] { 0, 0, 1, 0, 0, 1, 0 }) Bit(bit);
        while (count != 0) Bit(1);
        encoded.Write(new byte[] { 255, 217 }); var data = encoded.ToArray();
        if (Convert.ToHexStringLower(System.Security.Cryptography.SHA256.HashData(data))
            != "ba4b888bb53664a59b38eea752598638aa7a85640eba5c18aa2acd9c92e414d2")
            throw new Exception("Authored packaged JPEG pair input changed.");
        using var root = new TestMemoryBudget(data.Length + 2L * bytes + 128 * 1024);
        if (!root.TryReserve(data.Length, out var input)) throw new Exception("Cannot admit paired JPEG input.");
        using var inputOwner = input;
        var options = new DecodeOptions { Limits = new() { MemoryBudget = root } };
        using var live = new CancellationTokenSource();
        var token = cancelable ? live.Token : default;
        using var first = ImageCodec.Decode(data, options, token);
        using var stream = new MemoryStream(data, writable: false);
        using var second = await ImageCodec.DecodeAsync(stream, options, token);
        if (root.Bytes != data.Length + 2L * bytes || !stream.CanRead || root.IsDisposed)
            throw new Exception("Packaged JPEG pair scratch or borrowed ownership differs.");
        try { using var unexpected = ImageCodec.Decode(data, options, token); throw new Exception("JPEG pair aggregate refusal ignored."); }
        catch (ImageLimitException error) when (error.Scope == ImageLimitScope.SharedBudget) { }
        if (root.Refusals != 1 || root.Bytes != data.Length + 2L * bytes)
            throw new Exception("Packaged JPEG pair refusal was retried or leaked.");
        first.Dispose(); using var recovered = ImageCodec.Decode(data, options, token); data.AsSpan().Clear();
        foreach (var image in new[] { second, recovered })
        {
            using var hash = System.Security.Cryptography.IncrementalHash.CreateHash(System.Security.Cryptography.HashAlgorithmName.SHA256);
            for (var y = 0; y < height; y++) hash.AppendData(image.GetRow8(y));
            if (Convert.ToHexStringLower(hash.GetHashAndReset())
                != "a1980c44cfe0ffe7baf040e66f254059fe1ef0702d815399326441f56796d240")
                throw new Exception("Packaged JPEG pair samples or independent lifetime differ.");
        }
        recovered.Dispose(); second.Dispose(); inputOwner?.Dispose();
        if (root.Bytes != 0 || root.IsDisposed) throw new Exception("Packaged JPEG pair storage leaked.");
        Console.WriteLine($"Packaged grouped JPEG entropy preserves frozen pixels, sync/async, scan storage and shared admission: live={cancelable}.");
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

    private static void VerifyOutputWorkspaceBicubic()
    {
        VerifyOutputWorkspaceBicubicDepth(8);
        VerifyOutputWorkspaceBicubicDepth(16);
        Console.WriteLine("Packaged shared byte/word enlargement preserves exact default/bounded frames and owned lifetimes.");
    }

    private static void VerifyOutputWorkspaceBicubicDepth(int depth)
    {
        const long capacity = 16 * 1024 * 1024;
        using var root = new TestMemoryBudget(capacity);
        var input = OriginalPixels.Generate(513, 259, depth);
        if (depth == 8) input[3] = input[513 * 4 - 1] = 0;
        else
        {
            input[6] = input[7] = 0;
            input[513 * 8 - 2] = input[513 * 8 - 1] = 0;
        }
        var limits = new ImageLimits { MemoryBudget = root, MaxWorkingBytes = input.Length + 1027L * 519 * (depth / 2) + 4096 };
        using var source = ImageBuffer.Create(513, 259, depth, limits);
        for (var y = 0; y < 259; y++) input.AsSpan(y * source.Stride, source.Stride).CopyTo(
            depth == 8 ? source.GetRow8(y) : System.Runtime.InteropServices.MemoryMarshal.AsBytes(source.GetRow16(y)));
        using var first = ImageTransforms.Resize(source, 1027, 519, ResizeFilter.Bicubic, limits);
        using var second = ImageTransforms.Resize(source, 1027, 519, ResizeFilter.Bicubic, limits);
        var expected = OriginalPixels.Bytes(first);
        var frozen = depth == 8 ? "08cde71ddb8b20be884c7ba812cec349f9302b21a348b79d3614bfda19805b94"
            : "c17909e129d3a6775ebd3c863c5fce3e8c3fedfd0f9cb5aefd6eda4a68e993c0";
        if (Convert.ToHexStringLower(System.Security.Cryptography.SHA256.HashData(expected)) != frozen
            || !expected.AsSpan().SequenceEqual(OriginalPixels.Bytes(second))
            || !input.AsSpan().SequenceEqual(OriginalPixels.Bytes(source))
            || root.Bytes != input.Length + 2L * expected.Length)
            throw new Exception("Packaged output workspace changed pixels, source or retained capacity.");
        var retained = root.Bytes;
        if (!root.TryReserve(capacity - retained, out var caller)) throw new Exception("Cannot fill caller budget.");
        using (caller)
        {
            try { using var refused = ImageTransforms.Resize(source, 1027, 519, ResizeFilter.Bicubic, limits); throw new Exception("Packaged output workspace ignored refusal."); }
            catch (ImageLimitException e) when (e.Scope == ImageLimitScope.SharedBudget) { }
            if (root.Refusals != 1 || root.Bytes != capacity) throw new Exception("Packaged output workspace retried or leaked refused admission.");
        }
        using var cancel = new CancellationTokenSource(); root.Admitted = _ => cancel.Cancel();
        try { using var stopped = ImageTransforms.Resize(source, 1027, 519, ResizeFilter.Bicubic, limits, cancel.Token); throw new Exception("Packaged output workspace ignored cancellation."); }
        catch (OperationCanceledException) { }
        root.Admitted = null;
        if (root.Bytes != retained) throw new Exception("Packaged output workspace leaked canceled admission.");
        using (var later = ImageTransforms.Resize(source, 1027, 519, ResizeFilter.Bicubic, limits))
            if (!expected.AsSpan().SequenceEqual(OriginalPixels.Bytes(later))) throw new Exception("Packaged output workspace did not recover.");
        using (var defaultOutput = ImageTransforms.Resize(source, 1027, 519, ResizeFilter.Bicubic,
            limits with { MaxWorkingBytes = ImageLimits.Default.MaxWorkingBytes }))
            if (!expected.AsSpan().SequenceEqual(OriginalPixels.Bytes(defaultOutput))
                || root.Bytes != retained + expected.Length)
                throw new Exception("Packaged default byte enlargement changed pixels or shared output ownership.");
        if (root.Bytes != retained) throw new Exception("Packaged default byte enlargement retained operation storage.");
        source.Dispose(); first.Dispose(); input.AsSpan().Clear();
        if (!expected.AsSpan().SequenceEqual(OriginalPixels.Bytes(second)) || root.Bytes != expected.Length)
            throw new Exception("Packaged output workspace aliases a disposed source or sibling.");
        second.Dispose();
        if (root.Bytes != 0 || root.IsDisposed) throw new Exception("Packaged output workspace did not release owned capacity.");
        Console.WriteLine("Packaged output-backed bicubic preserves exact prefix/tail pixels, admission and surviving ownership.");
        Console.WriteLine("Packaged default byte enlargement preserves exact frame and shared output lifetime.");
    }

    private static void VerifyCachedHorizontalWindows()
    {
        foreach(var depth in new[]{8,16})
        foreach(var (width,height,workspace) in new[]{(16,8,12640),(33,8,23756)})
        {
            using var source=OriginalPixels.Buffer(33,17,depth);
            var input=OriginalPixels.Bytes(source);var outputBytes=width*height*(depth/2);
            using var oracle=ImageTransforms.Resize(source,width,height,ResizeFilter.Bicubic,
                new ImageLimits {MaxWorkingBytes=input.Length+outputBytes+(width==33?4096:4095)});
            var expected=OriginalPixels.Bytes(oracle);
            using var root=new TestMemoryBudget(2*outputBytes+workspace);
            var limits=new ImageLimits {MemoryBudget=root};
            using var first=ImageTransforms.Resize(source,width,height,ResizeFilter.Bicubic,limits);
            using var survivor=ImageTransforms.Resize(source,width,height,ResizeFilter.Bicubic,limits);
            if(!expected.AsSpan().SequenceEqual(OriginalPixels.Bytes(first))
                || !expected.AsSpan().SequenceEqual(OriginalPixels.Bytes(survivor))
                || root.Bytes!=2*outputBytes)
                throw new Exception("Packaged cached windows changed pixels or retained workspace.");
            using(var caller=root.CreateScope())
            {
                // Hold the remaining capacity rather than assuming which SIMD
                // workspace the consumer's machine supports.
                if(!caller.TryReserve(workspace,out var held))throw new Exception("Packaged cached refusal setup differs.");
                try
                {
                    using var refused=ImageTransforms.Resize(source,width,height,ResizeFilter.Bicubic,limits);
                    throw new Exception("Packaged cached windows ignored shared refusal.");
                }
                catch(ImageLimitException e)when(e.Scope==ImageLimitScope.SharedBudget && e.Limit==ImageLimitKind.WorkingBytes){}
            }
            if(root.Bytes!=2*outputBytes)throw new Exception("Packaged cached windows leaked refused storage.");
            source.Dispose();first.Dispose();
            if(root.Bytes!=outputBytes || !expected.AsSpan().SequenceEqual(OriginalPixels.Bytes(survivor)))
                throw new Exception("Packaged cached windows alias a disposed source or sibling.");
            survivor.Dispose();
            if(root.Bytes!=0 || root.IsDisposed)throw new Exception("Packaged cached windows leaked owned capacity.");
        }
        Console.WriteLine("Packaged cached horizontal windows preserve exact byte/word samples, terminal refusal and independent owners.");
        Console.WriteLine("Packaged identity cache rows preserve exact byte/word samples, terminal refusal and independent owners.");
    }

    internal static async Task Verify()
    {
        VerifyClearedCanvases();
        VerifyBicubicPlans();
        VerifyBicubicSchedule(4096);
        VerifyBicubicSchedule(8192);
        VerifyBicubicSchedule(4096, vertical: true);
        VerifyBicubicSchedule(8192, vertical: true);
        VerifyBicubicSchedule(4096, depth: 8);
        VerifyBicubicSchedule(4096, vertical: true, depth: 8);
        VerifyBicubicSchedule(4096, depth: 8, reduction: true);
        VerifyBicubicSchedule(4096, depth: 8, packedReduction: true);
        VerifyBicubicSchedule(4096, packedReduction: true);
        VerifyBicubicSchedule(4096, crossHeight: 259);
        VerifyBicubicSchedule(4096, crossHeight: 519);
        VerifyBicubicSchedule(4096, depth: 8, crossHeight: 259);
        VerifyBicubicSchedule(4096, depth: 8, crossHeight: 519);
        VerifyBicubicSchedule(4096, depth: 8, horizontal: true);
        VerifyBicubicSchedule(4096, horizontal: true);
        VerifyBicubicSchedule(4096, vertical: true, depth: 8, heightOverride: 519);
        VerifyBicubicSchedule(4096, vertical: true, heightOverride: 519);
        VerifyBicubicSchedule(4096, widthOverride: 32, heightOverride: 16);
        await VerifyPngCursors(cancellable: true);
        await VerifyPngCursors(cancellable: false);
        VerifyOwnedFonts();
        await VerifyProgressivePlaneOwnership();
        foreach (var sampling in new[] { JpegSampling.Grayscale, JpegSampling.YCbCr420, JpegSampling.YCbCr422 })
            await VerifyProgressivePlaneOwnership(sampling);
        Console.WriteLine("Packaged completed JPEG planes preserve exact gray/subsampled samples, async, terminal refusal and surviving ownership.");
        await VerifyOpaqueWordTiles();
        VerifyStreamingBicubic();
        VerifyOutputWorkspaceBicubic();
        VerifyCachedHorizontalWindows();
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
        await VerifyLosslessValidationStorage();
        await VerifyPaletteResult();
        await VerifyDistortionInstructionStorage();
        await VerifyWebpCandidateWorkspace();
        await VerifyWebpAlphaCoefficientStorage();
        await VerifyLanczosPlans();
        await VerifyCompactPngDictionary(budget, limits);
        await VerifyStoredPngWorkspace();
        await VerifyDirectRgbResult();
        await VerifyGroupedJpegResult(false);
        await VerifyGroupedJpegResult(true);
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
            var horizontalCompressed = false;
            for (var chunk = 12; chunk < alphaBytes.Length;)
            {
                var size = System.Buffers.Binary.BinaryPrimitives.ReadInt32LittleEndian(alphaBytes.AsSpan(chunk + 4));
                if (alphaBytes.AsSpan(chunk, 4).SequenceEqual("ALPH"u8))
                {
                    horizontalCompressed = (alphaBytes[chunk + 8] & 15) == 5;
                    break;
                }
                chunk += 8 + size + (size & 1);
            }
            if (!horizontalCompressed)
                throw new Exception("Packaged alpha control must exercise compressed horizontal publication.");
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
        Console.WriteLine("Packaged horizontal alpha publication preserves exact samples and admitted ownership.");
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
        foreach (var depth in new[] { 8, 16 })
        using (var pattern = OriginalPixels.Buffer(513, 259, depth))
        {
            var outputBytes = 256 * 129 * (depth / 2);
            using var expected = ImageTransforms.Resize(pattern, 256, 129, ResizeFilter.Bilinear,
                new ImageLimits { MaxWorkingBytes = 513 * 259 * (depth / 2) + outputBytes + 4096 });
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
                    if (depth == 8 ? !actual.GetRow8(y).SequenceEqual(expected.GetRow8(y))
                        : !actual.GetRow16(y).SequenceEqual(expected.GetRow16(y)))
                        throw new Exception("Packaged bilinear reduction samples differ.");
            }
            if (bilinearBudget.Bytes != 0 || bilinearBudget.IsDisposed)
                throw new Exception("Packaged bilinear preparation leaked storage or disposed the borrowed root.");
        }
        Console.WriteLine("Packaged bilinear byte/word reduction pixels and admission passed.");
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
            (256, 129, "656bb2248362c88dff8fe510dcc21055155976a3dd2b33aa20c48d2b036f59a8"),
            (32, 16, "3ec9c663c4051ce2f14b6a84acf4f21cf4c495c4017200f50f469de1f4b6132d")
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
            if (width == 32) Console.WriteLine("Packaged deep byte bicubic frozen pixels and shared admission passed.");
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
        // Ordinary/live kernels and awaited async input agree on direct and
        // prepared scans. The32block control also retains every frozen original
        // PGA335 sample; outputs outlive input mutation and one another.
        foreach (var (refinementWidth, refinementHeight) in new[] { (31, 23), (64, 32) })
        {
            var progressivePacket = AuthoredJpeg.ProgressiveFrame(refinementWidth, refinementHeight, JpegSampling.Grayscale, 1);
            var progressiveInput = progressivePacket.ToArray();
            using var progressiveCancellation = new CancellationTokenSource();
            var progressive = ImageCodec.Decode(progressivePacket, new() { Limits = limits });
            using var progressiveOriginal = ImageCodec.Decode(progressivePacket, new() { Limits = limits }, progressiveCancellation.Token);
            using var progressiveStream = new MemoryStream(progressivePacket, writable: false);
            using var progressiveAsync = await ImageCodec.DecodeAsync(progressiveStream, new() { Limits = limits });
            var progressivePixels = OriginalPixels.Bytes(progressiveOriginal);
            if (!progressivePixels.AsSpan().SequenceEqual(OriginalPixels.Bytes(progressive))
                || !progressivePixels.AsSpan().SequenceEqual(OriginalPixels.Bytes(progressiveAsync))
                || !progressivePacket.AsSpan().SequenceEqual(progressiveInput) || budget.Bytes != 3 * refinementWidth * refinementHeight * 4)
                throw new Exception("Packaged progressive refinement pixels or retained admission differ.");
            if (refinementWidth == 64 && (Convert.ToHexStringLower(System.Security.Cryptography.SHA256.HashData(progressivePacket)) != "837a99763d9a8a56e261d609f063fbecbc5217ce420156bbc0a05d8bd21764bb"
                || Convert.ToHexStringLower(System.Security.Cryptography.SHA256.HashData(progressivePixels)) != "1566284931f254f7eb40383a708f3da729a329a2feef9604056042407922aae1"))
                throw new Exception("Packaged prepared refinement changed the frozen original packet or samples.");
            progressivePacket.AsSpan().Clear(); progressive.GetRow8(0)[0] ^= 255; progressive.Dispose();
            if (budget.Bytes != 2 * refinementWidth * refinementHeight * 4 || !progressivePixels.AsSpan().SequenceEqual(OriginalPixels.Bytes(progressiveOriginal))
                || !progressivePixels.AsSpan().SequenceEqual(OriginalPixels.Bytes(progressiveAsync)) || !progressiveStream.CanRead)
                throw new Exception("Packaged progressive independent output or borrowed input lifetime differs.");
            progressiveOriginal.Dispose(); progressiveAsync.Dispose();
            if (budget.Bytes != 0 || budget.IsDisposed) throw new Exception("Packaged progressive storage leaked.");
        }
        Console.WriteLine("Packaged progressive refinement exact pixels, async and retained ownership passed.");
        Console.WriteLine("Packaged prepared refinement preserves frozen samples, async and retained ownership.");
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

    private static async Task VerifyStoredPngWorkspace()
    {
        foreach (var depth in new[] { 8, 16 })
        {
            const int width = 19, height = 131;
            var sourceBytes = width * height * 4 * (depth / 8);
            var working = sourceBytes + width * 4 * (depth / 8) + 4096 + 72 * 1024;
            using var root = new TestMemoryBudget(2 * 1024 * 1024);
            var limits = new ImageLimits { MaxWorkingBytes = working, MemoryBudget = root };
            using var seed = OriginalPixels.Buffer(width, height, depth);
            using var source = seed.Clone(limits); var pixels = OriginalPixels.Bytes(source);
            var options = new PngEncodeOptions { Compression = System.IO.Compression.CompressionLevel.NoCompression, Limits = limits };
            using var rejected = new MemoryStream();
            try
            {
                ImageCodec.EncodePng(source, rejected, options with { Limits = limits with { MaxWorkingBytes = working - 1 } });
                throw new Exception("Packaged stored PNG ignored its smaller working boundary.");
            }
            catch (ImageLimitException e) when (e.Scope == ImageLimitScope.Operation && e.Limit == ImageLimitKind.WorkingBytes) { }
            if (rejected.Length != 0 || root.Bytes != sourceBytes) throw new Exception("Packaged stored PNG refusal changed output or ownership.");
            using var synchronous = new MemoryStream(); using var asynchronous = new MemoryStream();
            ImageCodec.EncodePng(source, synchronous, options);
            if (root.Peak != working || root.Bytes != sourceBytes) throw new Exception("Packaged stored PNG workspace charge differs.");
            await ImageCodec.EncodePngAsync(source, asynchronous, options with { Limits = limits with { MaxWorkingBytes = 1024 * 1024 } });
            if (!synchronous.ToArray().AsSpan().SequenceEqual(asynchronous.ToArray())
                || !pixels.AsSpan().SequenceEqual(OriginalPixels.Bytes(source)) || root.Bytes != sourceBytes
                || !synchronous.CanWrite || !asynchronous.CanWrite) throw new Exception("Packaged stored PNG async packet or borrowed owners differ.");
            source.Dispose();
            if (root.Bytes != 0) throw new Exception("Packaged stored PNG retained work after completion.");
            using (var decoded = ImageCodec.Decode(asynchronous.ToArray(), new() { Limits = limits with { MaxWorkingBytes = 1024 * 1024 } }))
                if (decoded.BitDepth != depth || root.Bytes != sourceBytes || !pixels.AsSpan().SequenceEqual(OriginalPixels.Bytes(decoded)))
                    throw new Exception("Packaged stored PNG lost exact samples or independent retained ownership.");
            if (root.Bytes != 0 || root.IsDisposed) throw new Exception("Packaged stored PNG leaked or disposed the borrowed root.");
        }
        Console.WriteLine("Packaged stored PNG byte/word exact pixels, bounded workspace, async and ownership passed.");
    }

    private static async Task VerifyCompactPngDictionary(TestMemoryBudget budget, ImageLimits limits)
    {
        const int width = 513, height = 259, sourceBytes = width * height * 4;
        using var seed = OriginalPixels.Buffer(width, height, 8);
        seed.GetRow8(0)[3] = seed.GetRow8(0)[width * 4 - 1] = 0;
        using var source = seed.Clone(limits); var pixels = OriginalPixels.Bytes(source);
        var options = new PngEncodeOptions { ColorType = PngColorType.Rgba,
            Compression = System.IO.Compression.CompressionLevel.Optimal, Limits = limits };
        var working = sourceBytes + width * 4L + 4096 + 384 * 1024;
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
