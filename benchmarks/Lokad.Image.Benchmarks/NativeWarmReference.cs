using System.Diagnostics;
using System.Globalization;
using System.Text.Json;
using Lokad.Image;

// Original development worker; no bindings or native assets enter Lokad.Image.
internal sealed partial class NativeWarmReference : IDisposable
{
    private readonly Process process;
    private readonly Task<string> errors;
    private bool stopped;
    private bool disposed;
    private long completed;
    public JsonElement Ready { get; }
    public string Executable { get; }
    public JsonElement BuildIdentity { get; }
    private static readonly JsonSerializerOptions Options = new() { PropertyNameCaseInsensitive = true };

    public NativeWarmReference(JpegBenchmarkCase state, int cpu, Lokad.Image.ImageInfo expected, bool stream = false, bool nonSeekable = false)
    {
        var jpeg = expected.Format == Lokad.Image.ImageFormat.Jpeg;
        var webp = expected.Format == Lokad.Image.ImageFormat.WebP;
        if (!jpeg && !webp && expected.Format != Lokad.Image.ImageFormat.Png) throw new ArgumentException("PNG/JPEG/WebP Identify only.", nameof(expected));
        if (stream && expected.Format != Lokad.Image.ImageFormat.Png) throw new ArgumentException("Fresh Stream Identify uses the existing PNG only.", nameof(stream));
        if (nonSeekable && !stream) throw new ArgumentException("Nonseekable Identify requires Stream delivery.", nameof(nonSeekable));
        var kind = nonSeekable ? "png-nonseekable" : stream ? "png-stream" : webp ? "webp" : jpeg ? "jpeg" : "png";
        Executable = Path.Combine(state.Root, "external", "warm-reference", "ffmpeg-" + kind + "-identify-warm");
        using (var document = JsonDocument.Parse(File.ReadAllBytes(Path.Combine(state.Root, "external", "warm-reference", kind + "-identify-identity.json"))))
            BuildIdentity = document.RootElement.Clone();
        if (BuildIdentity.GetProperty("workerSha256").GetString() != JpegBenchmarkCase.HashFile(Executable)
            || BuildIdentity.GetProperty("sourceSha256").GetString() != JpegBenchmarkCase.HashFile(Path.Combine(state.Root, "tools", "WarmReference", "ffmpeg-" + kind + "-identify-warm.c"))
            || BuildIdentity.GetProperty("buildScriptSha256").GetString() != JpegBenchmarkCase.HashFile(Path.Combine(state.Root, "tools", "WarmReference", "build-" + kind + "-identify-linux.sh")))
            throw new InvalidOperationException("PNG/JPEG/WebP Identify worker/source/build identity differs.");
        var start = new ProcessStartInfo(Executable) { RedirectStandardInput = true, RedirectStandardOutput = true, RedirectStandardError = true, UseShellExecute = false };
        foreach (var argument in new[] { kind + "-identify", state.Input, state.NativeOutput }) start.ArgumentList.Add(argument);
        process = Process.Start(start) ?? throw new InvalidOperationException("Could not start PNG/JPEG/WebP Identify reference.");
        errors = process.StandardError.ReadToEndAsync();
        try
        {
            Ready = Read();
            if (Ready.GetProperty("state").GetString() != "ready" || Ready.GetProperty("schemaVersion").GetInt32() != 1
                || Ready.GetProperty("operation").GetString() != kind + "-identify"
                || Ready.GetProperty("width").GetInt32() != expected.Width || Ready.GetProperty("height").GetInt32() != expected.Height
                || Ready.GetProperty("depth").GetInt32() != expected.BitDepth || Ready.GetProperty("hasAlpha").GetBoolean() != expected.HasAlpha
                || Ready.GetProperty("orientation").GetInt32() != (int)expected.Orientation
                || Ready.GetProperty("cpu").GetInt32() != cpu || Ready.GetProperty("affinityCount").GetInt32() != 1
                || Ready.GetProperty("codecThreads").GetInt32() != 1 || Ready.GetProperty("threadType").GetInt32() != 0
                || Ready.GetProperty("activeThreadType").GetInt32() != 0 || Ready.GetProperty("outputBytes").GetInt32() != 24
                || Ready.GetProperty("pixelDecode").GetBoolean() || Ready.GetProperty("scalarMode").GetBoolean()
                || (nonSeekable && (Ready.GetProperty("freshSeekableStream").GetBoolean() || Ready.GetProperty("inputLengthQueryTimed").GetBoolean()
                    || !Ready.GetProperty("freshNonSeekableStream").GetBoolean() || !Ready.GetProperty("privateInputGrowthTimed").GetBoolean() || !Ready.GetProperty("residentCallbacksImmediate").GetBoolean()))
                || (stream && (Ready.GetProperty("inputView").GetBoolean() || (nonSeekable ? new[] { "freshNonSeekableStream", "inputCopyTimed", "eofChecked", "callerSourceLeftOpen", "streamAcquisitionAndReleaseTimed" } : new[] { "freshSeekableStream", "inputCopyTimed", "inputLengthQueryTimed", "eofChecked", "callerSourceLeftOpen", "streamAcquisitionAndReleaseTimed" }).Any(key => !Ready.GetProperty(key).GetBoolean())))
                || (jpeg && (Ready.GetProperty("pixelBufferRequests").GetInt32() != 0 || Ready.GetProperty("canonicalHeaderBytes").GetInt32() != 17))
                || (webp ? new[] { "publicParserTimed", "fullStructureTimed", "parserEofFlushTimed", "parserRequiredInputBufferingTimed", "structureMetadataTimed", "descriptorAcquisitionAndReleaseTimed", "independentLiveOutputs" }
                    : jpeg ? new[] { "fullStructureTimed", "canonicalHeaderProjectionTimed", "discardAllHeaderExtractionTimed", "decoderStateResetTimed", "structureMetadataTimed", "descriptorAcquisitionAndReleaseTimed", "independentLiveOutputs" }
                    : new[] { "publicParserTimed", "fullCrcTimed", "structureMetadataTimed", "descriptorAcquisitionAndReleaseTimed", "independentLiveOutputs" })
                    .Any(key => !Ready.GetProperty(key).GetBoolean()))
                throw new InvalidOperationException("PNG/JPEG/WebP Identify metadata/validation/ownership/CPU boundary differs.");
            completed = Ready.GetProperty("initialCompletedOperations").GetInt64();
        }
        catch { Dispose(); throw; }
    }

    // The single-component encoder is an explicit tooling-only baseline
    // exception. Reuse the prepared SIMD worker, including its historical
    // preapproval annotation; benchmark policy records the accepted reference.
    public NativeWarmReference(JpegBenchmarkCase state, int cpu, string expectedGray)
    {
        Executable = Path.Combine(state.Root, "external", "warm-reference", "libjpeg-gray-warm-candidate");
        using (var document = JsonDocument.Parse(File.ReadAllText(Path.Combine(state.Root, "external", "warm-reference", "libjpeg-gray-identity.json"))))
            BuildIdentity = document.RootElement.Clone();
        if (!BuildIdentity.GetProperty("simdEnabled").GetBoolean()
            || BuildIdentity.GetProperty("revision").GetString() != "9217719d3a58633923b096af4c1d50d304768a64"
            || BuildIdentity.GetProperty("workerSha256").GetString() != JpegBenchmarkCase.HashFile(Executable)
            || BuildIdentity.GetProperty("sourceSha256").GetString() != JpegBenchmarkCase.HashFile(Path.Combine(state.Root, "tools", "WarmReference", "libjpeg-gray-warm-candidate.c"))
            || BuildIdentity.GetProperty("buildScriptSha256").GetString() != JpegBenchmarkCase.HashFile(Path.Combine(state.Root, "tools", "WarmReference", "build-libjpeg-gray-linux.sh")))
            throw new InvalidOperationException("Grayscale reference SIMD/source/build identity differs.");
        var start = new ProcessStartInfo(Executable) { RedirectStandardInput = true, RedirectStandardOutput = true, RedirectStandardError = true, UseShellExecute = false };
        foreach (var key in start.Environment.Keys.Where(key => key.StartsWith("JSIMD_FORCE", StringComparison.Ordinal)).ToArray())
            start.Environment.Remove(key);
        foreach (var argument in new[] { "jpeg-gray-encode", state.Input, "513", "259", "8", state.NativeOutput, "69", expectedGray })
            start.ArgumentList.Add(argument);
        process = Process.Start(start) ?? throw new InvalidOperationException("Could not start grayscale reference.");
        errors = process.StandardError.ReadToEndAsync();
        try
        {
            Ready = Read();
            if (Ready.GetProperty("state").GetString() != "ready" || Ready.GetProperty("schemaVersion").GetInt32() != 1
                || Ready.GetProperty("operation").GetString() != "jpeg-gray-encode" || Ready.GetProperty("version").GetString() != "3.1.4.1"
                || Ready.GetProperty("width").GetInt32() != 513 || Ready.GetProperty("height").GetInt32() != 259
                || Ready.GetProperty("depth").GetInt32() != 8 || Ready.GetProperty("components").GetInt32() != 1
                || Ready.GetProperty("sampling").GetString() != "gray" || Ready.GetProperty("quality").GetInt32() != 69
                || Ready.GetProperty("cpu").GetInt32() != cpu || Ready.GetProperty("affinityCount").GetInt32() != 1
                || Ready.GetProperty("codecThreads").GetInt32() != 1 || Ready.GetProperty("conversionThreads").GetInt32() != 1
                || new[] { "ownedMathematicalQuantizers", "optimizeHuffman", "flatteningTimed", "colorConversionTimed", "normalizedReferenceExact", "outputAcquisitionAndReleaseTimed", "independentLiveOutputs", "bitstreamDeterministic" }.Any(key => !Ready.GetProperty(key).GetBoolean()))
                throw new InvalidOperationException("Grayscale reference workload/CPU contract differs.");
            completed = Ready.GetProperty("initialCompletedOperations").GetInt64();
        }
        catch { Dispose(); throw; }
    }

    public NativeWarmReference(JpegBenchmarkCase state, int depth, int cpu, string operation = "hflip", string? expectedRaw = null, int? nativeQuality = null, string? jpegSampling = null, bool vp8Opaque = false, int? resizeWidth = null, int? resizeHeight = null, bool bicubicScalar = false, int vp8Effort = 4)
    {
        if (resizeWidth.HasValue != resizeHeight.HasValue || resizeWidth.HasValue &&
            (operation is not ("bilinear-resize" or "nearest-resize" or "bicubic-resize")
                || (resizeWidth < 256 || resizeHeight < 129) && !(operation == "bicubic-resize" && depth == 8 && resizeWidth == 1 && resizeHeight == 1) && !(operation == "bicubic-resize" && depth is (8 or 16) && resizeWidth == 128 && resizeHeight == 64) && !(operation == "bicubic-resize" && depth is (8 or 16) && resizeWidth == 64 && resizeHeight == 32) && !(operation == "bicubic-resize" && depth is (8 or 16) && resizeWidth == 32 && resizeHeight == 16) && !(operation == "bicubic-resize" && depth is (8 or 16) && resizeWidth == 16 && resizeHeight == 8) && !(operation == "bicubic-resize" && depth is (8 or 16) && resizeWidth == 8 && resizeHeight is (4 or 17)) && !(operation == "bicubic-resize" && depth is (8 or 16) && resizeWidth == 4 && resizeHeight is (2 or 9)) && !(operation == "bicubic-resize" && depth is (8 or 16) && resizeWidth == 2 && resizeHeight is (2 or 9))
                || resizeWidth > 32768 || resizeHeight > 32768))
            throw new ArgumentException("Explicit resize dimensions require a supported bounded resize geometry.");
        if (vp8Effort is not (2 or 4 or 6)) throw new ArgumentOutOfRangeException(nameof(vp8Effort));
        var derivedVp8 = vp8Effort != 4; var vp8Variant = vp8Effort == 2 ? "distortion" : "trellis";
        if (derivedVp8 && (operation != "webp-opaque-encode" || depth != 8))
            throw new ArgumentException("Derived WebP effort requires a fixed opaque RGBA8 control.", nameof(vp8Effort));
        if (vp8Opaque && operation != "vp8-decode") throw new ArgumentException("Opaque VP8 applies only to VP8 decoding.", nameof(vp8Opaque));
        if (jpegSampling is not (null or "444" or "420" or "422") || jpegSampling is not null && operation is not ("jpeg-encode" or "jpeg-restart-encode"))
            throw new ArgumentException("JPEG sampling applies only to the 444/420/422 encode worker.", nameof(jpegSampling));
        if (operation is not ("hflip" or "hflip-retained2" or "normal-orient" or "transverse" or "transpose" or "quarter-turn-ccw" or "quarter-turn-cw" or "half-turn" or "vflip" or "crop" or "png-encode" or "png-smallest8-encode" or "png-smallest16-encode" or "png-fast16-encode" or "png-store16-encode" or "png-store-encode" or "png-rgb-encode" or "png-gray-encode" or "png-gray-alpha-encode" or "png-decode" or "png-stream-decode" or "png-stream16-decode" or "png-stream-crop-fit" or "png-stream16-crop-fit" or "png-adam7-decode" or "jpeg-encode" or "jpeg-restart-encode" or "jpeg-decode" or "jpeg420-stream-decode" or "jpeg420-decode" or "jpeg422-decode" or "jpeg-gray-decode" or "jpeg-rgb-decode" or "jpeg-rgb422-decode" or "jpeg-rgb420-decode" or "webp-encode" or "webp-opaque-encode" or "webp-lossless-encode" or "webp-decode" or "webp-stream-decode" or "vp8-decode" or "vp8-stream-decode" or "nearest-resize" or "bilinear-resize" or "bicubic-resize")) throw new ArgumentOutOfRangeException(nameof(operation));
        var normal = operation == "normal-orient";
        var transverse = operation == "transverse";
        var transpose = operation == "transpose";
        var counterclockwise = operation == "quarter-turn-ccw";
        var clockwise = operation == "quarter-turn-cw";
        var halfTurn = operation == "half-turn";
        var vertical = operation == "vflip";
        var crop = operation == "crop";
        var nearest = operation == "nearest-resize";
        var bilinear = operation == "bilinear-resize";
        var bicubic = operation == "bicubic-resize";
        var bicubicOnePixel = bicubic && depth == 8 && resizeWidth == 1 && resizeHeight == 1;
        var bicubicUpsample = bicubic && depth is (8 or 16) && resizeWidth == 1027 && resizeHeight == 519;
        var bicubicMixed = bicubic && depth is (8 or 16) && resizeWidth == 1027 && resizeHeight == 129;
        var bicubicCross = bicubic && depth is (8 or 16) && resizeWidth == 256 && resizeHeight == 519;
        var bicubicWide = bicubic && depth is (8 or 16) && resizeWidth == 128 && resizeHeight == 64;
        var bicubicNineRows = bicubic && depth is (8 or 16) && resizeWidth == 2 && resizeHeight == 9;
        var bicubicBeyond1024 = bicubic && depth is (8 or 16) && resizeWidth == 2 && resizeHeight is (2 or 9);
        var bicubicPackedNineRows = bicubic && depth is (8 or 16) && resizeWidth == 4 && resizeHeight == 9;
        var bicubicBeyond512 = bicubic && depth is (8 or 16) && resizeWidth == 4 && resizeHeight is (2 or 9);
        var bicubicSeventeenRows = bicubic && depth is (8 or 16) && resizeWidth == 8 && resizeHeight == 17;
        var bicubicBeyond256 = bicubic && depth is (8 or 16) && resizeWidth == 8 && resizeHeight is (4 or 17);
        var bicubicBeyond128 = bicubic && depth is (8 or 16) && resizeWidth == 16 && resizeHeight == 8;
        var bicubicBeyond64 = bicubic && depth is (8 or 16) && resizeWidth == 32 && resizeHeight == 16;
        var bicubicBeyond32 = bicubic && depth is (8 or 16) && resizeWidth == 64 && resizeHeight == 32;
        var bicubicHorizontalUp = bicubic && depth is (8 or 16) && resizeWidth == 1027 && resizeHeight == 259;
        var bicubicVerticalUp = bicubic && depth is (8 or 16) && resizeWidth == 513 && resizeHeight == 519;
        var bicubicVertical = bicubic && depth is (8 or 16) && resizeWidth == 513 && resizeHeight == 129;
        var bicubicHorizontal = bicubic && depth is (8 or 16) && resizeWidth == 256 && resizeHeight == 259;
        if (bicubic && resizeWidth.HasValue && !bicubicOnePixel && !bicubicUpsample && !bicubicMixed && !bicubicCross && !bicubicWide && !bicubicHorizontal && !bicubicBeyond32 && !bicubicVertical && !bicubicHorizontalUp && !bicubicVerticalUp && !bicubicBeyond64 && !bicubicBeyond128 && !bicubicBeyond256 && !bicubicBeyond512 && !bicubicBeyond1024) throw new ArgumentException("Explicit bicubic geometry is limited to a selected fixed case.");
        if (bicubicScalar && !bicubic) throw new ArgumentException("Scalar backend applies only to the bicubic worker.");
        var codec = operation is not ("hflip" or "hflip-retained2" or "normal-orient" or "transverse" or "transpose" or "quarter-turn-ccw" or "quarter-turn-cw" or "half-turn" or "vflip" or "crop" or "nearest-resize" or "bilinear-resize" or "bicubic-resize"); var jpeg = operation is "jpeg-encode" or "jpeg-restart-encode"; var jpegDecode = operation is "jpeg-decode" or "jpeg420-stream-decode" or "jpeg420-decode" or "jpeg422-decode" or "jpeg-gray-decode" or "jpeg-rgb-decode" or "jpeg-rgb422-decode" or "jpeg-rgb420-decode"; var webpLossless = operation == "webp-lossless-encode"; var webp = operation is "webp-encode" or "webp-opaque-encode" || webpLossless; var webpDecode = operation is "webp-decode" or "webp-stream-decode"; var vp8Decode = operation is "vp8-decode" or "vp8-stream-decode";
        var jpegRestart = operation == "jpeg-restart-encode";
        var pngStream16 = operation == "png-stream16-decode";
        var pngComposition16 = operation == "png-stream16-crop-fit";
        var pngComposition = operation is "png-stream-crop-fit" or "png-stream16-crop-fit";
        var pngStream = operation is "png-stream-decode" or "png-stream16-decode" or "png-stream-crop-fit" or "png-stream16-crop-fit";
        var retainedFlip = operation == "hflip-retained2";
        var stem = operation == "png-store16-encode" ? "ffmpeg-png-store16-warm" : operation == "png-fast16-encode" ? "ffmpeg-png-fast16-warm" : operation == "vp8-stream-decode" ? "ffmpeg-vp8-stream-warm" : operation == "jpeg420-stream-decode" ? "ffmpeg-jpeg-stream-warm" : operation == "webp-opaque-encode" ? depth == 16 ? "ffmpeg-webp-opaque16-warm" : "ffmpeg-webp-opaque-warm" : jpegRestart ? "ffmpeg-jpeg-restart-warm" : pngStream16 ? "ffmpeg-png-stream16-warm" : pngComposition16 ? "ffmpeg-png-composition16-warm" : pngComposition ? "ffmpeg-png-composition-warm" : pngStream ? "ffmpeg-png-stream-warm" : retainedFlip ? "ffmpeg-hflip-retained2-warm" : bicubicOnePixel ? "zimg-bicubic-down1x1-8-warm" : bicubicNineRows ? depth == 16 ? "zimg-bicubic-down2x9-16-warm" : "zimg-bicubic-down2x9-8-warm" : bicubicBeyond1024 ? depth == 16 ? "zimg-bicubic-down2x2-16-warm" : "zimg-bicubic-down2x2-8-warm" : bicubicPackedNineRows ? depth == 16 ? "zimg-bicubic-down4x9-16-warm" : "zimg-bicubic-down4x9-8-warm" : bicubicBeyond512 ? depth == 16 ? "zimg-bicubic-down4x2-16-warm" : "zimg-bicubic-down4x2-8-warm" : bicubicSeventeenRows ? depth == 16 ? "zimg-bicubic-down8x17-16-warm" : "zimg-bicubic-down8x17-8-warm" : bicubicBeyond256 ? depth == 16 ? "zimg-bicubic-down8x4-16-warm" : "zimg-bicubic-down8x4-8-warm" : bicubicBeyond128 ? depth == 16 ? "zimg-bicubic-down16x8-16-warm" : "zimg-bicubic-down16x8-8-warm" : bicubicBeyond64 ? depth == 16 ? "zimg-bicubic-down32x16-16-warm" : "zimg-bicubic-down32x16-8-warm" : bicubicVerticalUp ? depth == 16 ? "zimg-bicubic-vertical-up16-warm" : "zimg-bicubic-vertical-up8-warm" : bicubicHorizontalUp ? depth == 16 ? "zimg-bicubic-horizontal-up16-warm" : "zimg-bicubic-horizontal-up8-warm" : bicubicVertical ? depth == 16 ? "zimg-bicubic-vertical16-warm" : "zimg-bicubic-vertical8-warm" : bicubicBeyond32 ? depth == 16 ? "zimg-bicubic-down64x32-16-warm" : "zimg-bicubic-down64x32-8-warm" : bicubicHorizontal ? depth == 16 ? "zimg-bicubic-horizontal16-warm" : "zimg-bicubic-horizontal8-warm" : bicubicWide ? depth == 16 ? "zimg-bicubic-wide16-warm" : "zimg-bicubic-wide8-warm" : bicubicCross ? depth == 16 ? "zimg-bicubic-cross16-warm" : "zimg-bicubic-cross8-warm" : bicubicMixed ? depth == 16 ? "zimg-bicubic-mixed16-warm" : "zimg-bicubic-mixed8-warm" : bicubicUpsample ? depth == 16 ? "zimg-bicubic-up16-warm" : "zimg-bicubic-up8-warm" : bicubic ? depth == 16 ? "zimg-bicubic16-warm" : "zimg-bicubic-warm" : transverse ? "ffmpeg-transverse-warm" : transpose ? "ffmpeg-transpose-warm" : counterclockwise ? "ffmpeg-counterclockwise-warm" : normal ? "ffmpeg-normal-orient-warm" : clockwise ? "ffmpeg-clockwise-warm" : halfTurn ? "ffmpeg-halfturn-warm" : vertical ? "ffmpeg-vflip-warm" : crop ? "ffmpeg-crop-warm" : bilinear ? "ffmpeg-bilinear-warm" : nearest ? "ffmpeg-nearest-warm" : vp8Decode ? "ffmpeg-vp8-decode-warm" : webpDecode ? "ffmpeg-webp-decode-warm" : jpegDecode ? "ffmpeg-jpeg-decode-warm" : webpLossless ? "libwebp-lossless-warm" : webp ? "ffmpeg-webp-warm" : jpeg ? "ffmpeg-jpeg-warm" : codec ? "ffmpeg-png-warm" : "ffmpeg-warm";
        var buildScript = operation == "png-store16-encode" ? "build-png-store16-linux.sh" : operation == "png-fast16-encode" ? "build-png-fast16-linux.sh" : operation == "vp8-stream-decode" ? "build-vp8-stream-linux.sh" : operation == "jpeg420-stream-decode" ? "build-jpeg-stream-linux.sh" : operation == "webp-opaque-encode" ? depth == 16 ? "build-webp-opaque16-linux.sh" : "build-webp-opaque-linux.sh" : jpegRestart ? "build-jpeg-restart-linux.sh" : pngStream16 ? "build-png-stream16-linux.sh" : pngComposition16 ? "build-png-composition16-linux.sh" : pngComposition ? "build-png-composition-linux.sh" : pngStream ? "build-png-stream-linux.sh" : retainedFlip ? "build-hflip-retained2-linux.sh" : bicubicOnePixel ? "build-bicubic-down1x1-8-linux.sh" : bicubicNineRows ? depth == 16 ? "build-bicubic-down2x9-16-linux.sh" : "build-bicubic-down2x9-8-linux.sh" : bicubicBeyond1024 ? depth == 16 ? "build-bicubic-down2x2-16-linux.sh" : "build-bicubic-down2x2-8-linux.sh" : bicubicPackedNineRows ? depth == 16 ? "build-bicubic-down4x9-16-linux.sh" : "build-bicubic-down4x9-8-linux.sh" : bicubicBeyond512 ? depth == 16 ? "build-bicubic-down4x2-16-linux.sh" : "build-bicubic-down4x2-8-linux.sh" : bicubicSeventeenRows ? depth == 16 ? "build-bicubic-down8x17-16-linux.sh" : "build-bicubic-down8x17-8-linux.sh" : bicubicBeyond256 ? depth == 16 ? "build-bicubic-down8x4-16-linux.sh" : "build-bicubic-down8x4-8-linux.sh" : bicubicBeyond128 ? depth == 16 ? "build-bicubic-down16x8-16-linux.sh" : "build-bicubic-down16x8-8-linux.sh" : bicubicBeyond64 ? depth == 16 ? "build-bicubic-down32x16-16-linux.sh" : "build-bicubic-down32x16-8-linux.sh" : bicubicVerticalUp ? depth == 16 ? "build-bicubic-vertical-up16-linux.sh" : "build-bicubic-vertical-up8-linux.sh" : bicubicHorizontalUp ? depth == 16 ? "build-bicubic-horizontal-up16-linux.sh" : "build-bicubic-horizontal-up8-linux.sh" : bicubicVertical ? depth == 16 ? "build-bicubic-vertical16-linux.sh" : "build-bicubic-vertical8-linux.sh" : bicubicBeyond32 ? depth == 16 ? "build-bicubic-down64x32-16-linux.sh" : "build-bicubic-down64x32-8-linux.sh" : bicubicHorizontal ? depth == 16 ? "build-bicubic-horizontal16-linux.sh" : "build-bicubic-horizontal8-linux.sh" : bicubicWide ? depth == 16 ? "build-bicubic-wide16-linux.sh" : "build-bicubic-wide8-linux.sh" : bicubicCross ? depth == 16 ? "build-bicubic-cross16-linux.sh" : "build-bicubic-cross8-linux.sh" : bicubicMixed ? depth == 16 ? "build-bicubic-mixed16-linux.sh" : "build-bicubic-mixed8-linux.sh" : bicubicUpsample ? depth == 16 ? "build-bicubic-up16-linux.sh" : "build-bicubic-up8-linux.sh" : bicubic ? depth == 16 ? "build-bicubic16-linux.sh" : "build-bicubic-linux.sh" : transverse ? "build-transverse-linux.sh" : transpose ? "build-transpose-linux.sh" : counterclockwise ? "build-counterclockwise-linux.sh" : normal ? "build-normal-orient-linux.sh" : clockwise ? "build-clockwise-linux.sh" : halfTurn ? "build-halfturn-linux.sh" : vertical ? "build-vflip-linux.sh" : crop ? "build-crop-linux.sh" : bilinear ? "build-bilinear-linux.sh" : nearest ? "build-nearest-linux.sh" : vp8Decode ? "build-vp8-decode-linux.sh" : webpDecode ? "build-webp-decode-linux.sh" : jpegDecode ? "build-jpeg-decode-linux.sh" : webpLossless ? "build-libwebp-lossless-linux.sh" : webp ? "build-webp-linux.sh" : jpeg ? "build-jpeg-linux.sh" : codec ? "build-png-linux.sh" : "build-linux.sh";
        if (derivedVp8) { stem = "ffmpeg-webp-" + vp8Variant + "-warm"; buildScript = "build-webp-" + vp8Variant + "-linux.sh"; }
        Executable = Path.Combine(state.Root, "external", "warm-reference", stem);
        var identityPath = Path.Combine(Path.GetDirectoryName(Executable)!, operation == "png-store16-encode" ? "png-store16-identity.json" : operation == "png-fast16-encode" ? "png-fast16-identity.json" : operation == "vp8-stream-decode" ? "vp8-stream-identity.json" : operation == "jpeg420-stream-decode" ? "jpeg-stream-identity.json" : operation == "webp-opaque-encode" ? depth == 16 ? "webp-opaque16-identity.json" : "webp-opaque-identity.json" : jpegRestart ? "jpeg-restart-identity.json" : pngStream16 ? "png-stream16-identity.json" : pngComposition16 ? "png-composition16-identity.json" : pngComposition ? "png-composition-identity.json" : pngStream ? "png-stream-identity.json" : retainedFlip ? "hflip-retained2-identity.json" : bicubicOnePixel ? "bicubic-down1x1-8-identity.json" : bicubicNineRows ? depth == 16 ? "bicubic-down2x9-16-identity.json" : "bicubic-down2x9-8-identity.json" : bicubicBeyond1024 ? depth == 16 ? "bicubic-down2x2-16-identity.json" : "bicubic-down2x2-8-identity.json" : bicubicPackedNineRows ? depth == 16 ? "bicubic-down4x9-16-identity.json" : "bicubic-down4x9-8-identity.json" : bicubicBeyond512 ? depth == 16 ? "bicubic-down4x2-16-identity.json" : "bicubic-down4x2-8-identity.json" : bicubicSeventeenRows ? depth == 16 ? "bicubic-down8x17-16-identity.json" : "bicubic-down8x17-8-identity.json" : bicubicBeyond256 ? depth == 16 ? "bicubic-down8x4-16-identity.json" : "bicubic-down8x4-8-identity.json" : bicubicBeyond128 ? depth == 16 ? "bicubic-down16x8-16-identity.json" : "bicubic-down16x8-8-identity.json" : bicubicBeyond64 ? depth == 16 ? "bicubic-down32x16-16-identity.json" : "bicubic-down32x16-8-identity.json" : bicubicVerticalUp ? depth == 16 ? "bicubic-vertical-up16-identity.json" : "bicubic-vertical-up8-identity.json" : bicubicHorizontalUp ? depth == 16 ? "bicubic-horizontal-up16-identity.json" : "bicubic-horizontal-up8-identity.json" : bicubicVertical ? depth == 16 ? "bicubic-vertical16-identity.json" : "bicubic-vertical8-identity.json" : bicubicBeyond32 ? depth == 16 ? "bicubic-down64x32-16-identity.json" : "bicubic-down64x32-8-identity.json" : bicubicHorizontal ? depth == 16 ? "bicubic-horizontal16-identity.json" : "bicubic-horizontal8-identity.json" : bicubicWide ? depth == 16 ? "bicubic-wide16-identity.json" : "bicubic-wide8-identity.json" : bicubicCross ? depth == 16 ? "bicubic-cross16-identity.json" : "bicubic-cross8-identity.json" : bicubicMixed ? depth == 16 ? "bicubic-mixed16-identity.json" : "bicubic-mixed8-identity.json" : bicubicUpsample ? depth == 16 ? "bicubic-up16-identity.json" : "bicubic-up8-identity.json" : bicubic ? depth == 16 ? "bicubic16-identity.json" : "bicubic-identity.json" : transverse ? "transverse-identity.json" : transpose ? "transpose-identity.json" : counterclockwise ? "counterclockwise-identity.json" : normal ? "normal-orient-identity.json" : clockwise ? "clockwise-identity.json" : halfTurn ? "halfturn-identity.json" : vertical ? "vflip-identity.json" : crop ? "crop-identity.json" : bilinear ? "bilinear-identity.json" : nearest ? "nearest-identity.json" : vp8Decode ? "vp8-decode-identity.json" : webpDecode ? "webp-decode-identity.json" : jpegDecode ? "jpeg-decode-identity.json" : webpLossless ? "libwebp-lossless-identity.json" : webp ? "webp-identity.json" : jpeg ? "jpeg-identity.json" : codec ? "png-identity.json" : "identity.json");
        if (derivedVp8) identityPath = Path.Combine(Path.GetDirectoryName(Executable)!, "webp-" + vp8Variant + "-identity.json");
        if (!File.Exists(Executable) || !File.Exists(identityPath))
            throw new InvalidOperationException("Build the pinned Linux warm reference with its tools/WarmReference build script.");
        using (var document = JsonDocument.Parse(File.ReadAllText(identityPath))) BuildIdentity = document.RootElement.Clone();
        if (BuildIdentity.GetProperty("workerSha256").GetString() != JpegBenchmarkCase.HashFile(Executable)
            || BuildIdentity.GetProperty("sourceSha256").GetString() != JpegBenchmarkCase.HashFile(Path.Combine(state.Root, "tools", "WarmReference", (derivedVp8 ? "ffmpeg-webp-opaque-warm" : bicubicOnePixel || bicubicMixed || bicubicCross || bicubicWide || bicubicHorizontal || bicubicBeyond32 || bicubicVertical || bicubicHorizontalUp || bicubicVerticalUp || bicubicBeyond64 || bicubicBeyond128 || bicubicBeyond256 || bicubicBeyond512 || bicubicBeyond1024 ? "zimg-bicubic-mixed8-warm" : stem) + ".c"))
            || BuildIdentity.GetProperty("buildScriptSha256").GetString() != JpegBenchmarkCase.HashFile(Path.Combine(state.Root, "tools", "WarmReference", buildScript)))
            throw new InvalidOperationException("Native reference build identity differs from the current source/binary.");
        if (derivedVp8 && (BuildIdentity.GetProperty("effort").GetInt32() != vp8Effort
            || !BuildIdentity.GetProperty("onlyEffortChanged").GetBoolean()
            || BuildIdentity.GetProperty("generatedSourceSha256").GetString() != JpegBenchmarkCase.HashFile(Path.Combine(state.Root, "external", "warm-reference", stem + ".c"))))
            throw new InvalidOperationException("The derived WebP worker source/effort differs.");
        var start = new ProcessStartInfo(Executable) { RedirectStandardInput = true, RedirectStandardOutput = true,
            RedirectStandardError = true, UseShellExecute = false };
        foreach (var argument in new[] { operation, state.Input, "513", "259", depth.ToString(CultureInfo.InvariantCulture), state.NativeOutput })
            start.ArgumentList.Add(argument);
        if (operation is "png-decode" or "png-stream-decode" or "png-stream16-decode" or "png-stream-crop-fit" or "png-stream16-crop-fit" or "png-adam7-decode" or "bicubic-resize") start.ArgumentList.Add(expectedRaw ?? throw new ArgumentNullException(nameof(expectedRaw)));
        if (jpeg || webp)
        {
            if ((jpeg && depth is not (8 or 16)) || (webp && depth is not (8 or 16)) || !nativeQuality.HasValue || nativeQuality < 1 || nativeQuality > (webp ? 100 : 31)) throw new ArgumentOutOfRangeException(nameof(nativeQuality));
            start.ArgumentList.Add(nativeQuality.Value.ToString(CultureInfo.InvariantCulture));
        }
        else if (nativeQuality.HasValue) throw new ArgumentException("Native quality applies only to JPEG or VP8.", nameof(nativeQuality));
        if ((jpeg || webp) && depth == 16) start.ArgumentList.Add(expectedRaw ?? throw new ArgumentNullException(nameof(expectedRaw)));
        if (jpeg && jpegSampling is "420" or "422") start.ArgumentList.Add(jpegSampling);
        if (resizeWidth.HasValue)
        {
            start.ArgumentList.Add(resizeWidth.Value.ToString(CultureInfo.InvariantCulture));
            start.ArgumentList.Add(resizeHeight!.Value.ToString(CultureInfo.InvariantCulture));
        }
        if (bilinear && depth == 16) start.ArgumentList.Add("--allow-alpha-rounding1");
        var up8Compatibility = bilinear && depth == 8 && resizeWidth == 1027 && resizeHeight == 519;
        if (up8Compatibility) start.ArgumentList.Add("--up8-alpha-compat");
        if (bicubicScalar) start.ArgumentList.Add("--scalar");
        process = Process.Start(start) ?? throw new InvalidOperationException("Could not start native warm worker.");
        errors = process.StandardError.ReadToEndAsync();
        try
        {
            Ready = Read();
            if (operation is ("jpeg-rgb-decode" or "jpeg-rgb422-decode" or "jpeg-rgb420-decode") && (!Ready.GetProperty("directRgb").GetBoolean() || Ready.GetProperty("decodedPixelFormat").GetString() != (operation == "jpeg-rgb420-decode" ? "yuvj420p" : "gbrp")))
                throw new InvalidOperationException("Native direct-RGB JPEG format differs.");
            if (Ready.GetProperty("state").GetString() != "ready" || Ready.GetProperty("schemaVersion").GetInt32() != 1
                || Ready.GetProperty("width").GetInt32() != HorizontalFlipInput.Width
                || Ready.GetProperty("height").GetInt32() != HorizontalFlipInput.Height
                || Ready.GetProperty("depth").GetInt32() != depth || Ready.GetProperty("cpu").GetInt32() != cpu
                || Ready.GetProperty("affinityCount").GetInt32() != 1 || Ready.GetProperty(codec ? "codecThreads" : "filterThreads").GetInt32() != 1
                || Ready.GetProperty("threadType").GetInt32() != 0 || Ready.GetProperty("operation").GetString() != operation
                || (pngStream && (!Ready.GetProperty("freshSeekableStream").GetBoolean() || !Ready.GetProperty("inputCopyTimed").GetBoolean() || !Ready.GetProperty("inputLengthQueryTimed").GetBoolean() || Ready.GetProperty("inputView").GetBoolean() || !Ready.GetProperty("eofChecked").GetBoolean()))
                || (operation is "jpeg420-stream-decode" or "vp8-stream-decode" or "webp-stream-decode" && (!Ready.GetProperty("freshSeekableStream").GetBoolean() || !Ready.GetProperty("inputCopyTimed").GetBoolean() || !Ready.GetProperty("inputLengthQueryTimed").GetBoolean() || Ready.GetProperty("inputView").GetBoolean() || !Ready.GetProperty("eofChecked").GetBoolean()))
                || (retainedFlip && (Ready.GetProperty("outputsPerUnit").GetInt32() != 2 || !Ready.GetProperty("releaseAfterBoth").GetBoolean() || !Ready.GetProperty("independentLiveOutputs").GetBoolean()))
                || (codec && (Ready.GetProperty("activeThreadType").GetInt32() != 0 || !Ready.GetProperty("independentLiveOutputs").GetBoolean()))
                || ((nearest || bilinear || bicubic || crop) && ((depth is not (8 or 16)) || Ready.GetProperty("outputWidth").GetInt32() != (resizeWidth ?? 256)
                    || Ready.GetProperty("outputHeight").GetInt32() != (resizeHeight ?? 129) || !Ready.GetProperty("independentLiveOutputs").GetBoolean()))
                || ((bicubicOnePixel || bicubicUpsample || bicubicMixed || bicubicCross || bicubicWide || bicubicHorizontal || bicubicBeyond32 || bicubicVertical || bicubicHorizontalUp || bicubicVerticalUp || bicubicBeyond64 || bicubicBeyond128 || bicubicBeyond256 || bicubicBeyond512 || bicubicBeyond1024) && (!Ready.GetProperty("premultipliedFloatOutput").GetBoolean() || !Ready.GetProperty("unpremultiplicationAndQuantizationTimed").GetBoolean()))
                || (bicubicOnePixel && (Ready.GetProperty("haloPixelsX").GetInt32()!=770 || Ready.GetProperty("haloPixelsY").GetInt32()!=389))
                || (bicubicVerticalUp && depth==16 && (!Ready.GetProperty("lowAlphaRgbAllowance").GetBoolean() || Ready.GetProperty("lowAlphaRgbReferenceMaximum").GetInt32()!=16 || Ready.GetProperty("lowAlphaRgbErrorMaximum").GetInt32()!=2))
                || (bicubic && (depth is not (8 or 16) || Ready.GetProperty("scalar").GetBoolean() != bicubicScalar
                    || Ready.GetProperty("haloPixels").GetInt32() != (bicubicOnePixel ? 770 : bicubicBeyond1024 ? 512 : bicubicBeyond512 ? 256 : bicubicBeyond256 ? 128 : bicubicBeyond128 ? 64 : bicubicBeyond64 ? 32 : bicubicBeyond32 ? 16 : bicubicWide ? 8 : 4) || !Ready.GetProperty("haloAndPackingTimed").GetBoolean()
                    || !Ready.GetProperty("outputAllocationAndRepackingTimed").GetBoolean() || !Ready.GetProperty("alphaTolerance1").GetBoolean()
                    || Ready.GetProperty("backend").GetString() != "zimg (FFmpeg zscale backend)"))
                || (pngStream16 && (!Ready.GetProperty("nativeEndianConversionTimed").GetBoolean()
                    || Ready.GetProperty("conversionThreads").GetInt32() != 1 || depth != 16))
                || (pngComposition16 && (depth != 16 || !Ready.GetProperty("nativeEndianConversionTimed").GetBoolean() || !Ready.GetProperty("graphCropBeforeEndianConversion").GetBoolean()
                    || Ready.GetProperty("sourcePixelFormat").GetString() != "rgba64be" || Ready.GetProperty("outputPixelFormat").GetString() != "rgba64le"))
                || (pngComposition && (Ready.GetProperty("outputWidth").GetInt32() != 127
                    || Ready.GetProperty("outputHeight").GetInt32() != 64 || Ready.GetProperty("filterThreads").GetInt32() != 1
                    || Ready.GetProperty("conversionThreads").GetInt32() != 1 || Ready.GetProperty("orientation").GetString() != "Normal"
                    || Ready.GetProperty("cropWidth").GetInt32() != 256 || Ready.GetProperty("cropHeight").GetInt32() != 129
                    || Ready.GetProperty("fitMaxWidth").GetInt32() != 128 || Ready.GetProperty("fitMaxHeight").GetInt32() != 64
                    || !Ready.GetProperty("pointSampling").GetBoolean()))
                || (normal && (depth is not (8 or 16) || Ready.GetProperty("outputWidth").GetInt32() != 513
                    || Ready.GetProperty("outputHeight").GetInt32() != 259 || !Ready.GetProperty("independentLiveOutputs").GetBoolean()
                    || !Ready.GetProperty("ownedCopyTimed").GetBoolean() || !Ready.GetProperty("sharedNullView").GetBoolean()))
                || (transverse && (depth is not (8 or 16) || Ready.GetProperty("outputWidth").GetInt32() != 259
                    || Ready.GetProperty("outputHeight").GetInt32() != 513 || !Ready.GetProperty("independentLiveOutputs").GetBoolean()
                    || !Ready.GetProperty("mirroredTranspose").GetBoolean()))
                || (transpose && (depth is not (8 or 16) || Ready.GetProperty("outputWidth").GetInt32() != 259
                    || Ready.GetProperty("outputHeight").GetInt32() != 513 || !Ready.GetProperty("independentLiveOutputs").GetBoolean()
                    || !Ready.GetProperty("plainTranspose").GetBoolean()))
                || (counterclockwise && (depth is not (8 or 16) || Ready.GetProperty("outputWidth").GetInt32() != 259
                    || Ready.GetProperty("outputHeight").GetInt32() != 513 || !Ready.GetProperty("independentLiveOutputs").GetBoolean()
                    || !Ready.GetProperty("counterclockwiseTranspose").GetBoolean()))
                || (clockwise && (depth is not (8 or 16) || Ready.GetProperty("outputWidth").GetInt32() != 259
                    || Ready.GetProperty("outputHeight").GetInt32() != 513 || !Ready.GetProperty("independentLiveOutputs").GetBoolean()
                    || !Ready.GetProperty("clockwiseTranspose").GetBoolean()))
                || (halfTurn && (depth is not (8 or 16) || Ready.GetProperty("outputWidth").GetInt32() != 513
                    || Ready.GetProperty("outputHeight").GetInt32() != 259 || !Ready.GetProperty("independentLiveOutputs").GetBoolean()
                    || !Ready.GetProperty("ownedStrideAccepted").GetBoolean()))
                || (vertical && (depth is not (8 or 16) || Ready.GetProperty("outputWidth").GetInt32() != 513
                    || Ready.GetProperty("outputHeight").GetInt32() != 259 || !Ready.GetProperty("independentLiveOutputs").GetBoolean()
                    || !Ready.GetProperty("ownedCopyTimed").GetBoolean() || !Ready.GetProperty("negativeStrideView").GetBoolean()))
                || (crop && (Ready.GetProperty("cropX").GetInt32() != 127 || Ready.GetProperty("cropY").GetInt32() != 65
                    || !Ready.GetProperty("ownedCopyTimed").GetBoolean()))
                || (bilinear && (Ready.GetProperty("kernel").GetString() != "triangle" || Ready.GetProperty("alphaMode").GetString() != "straight"
                    || !Ready.GetProperty("premultipliedAlphaTimed").GetBoolean() || !Ready.GetProperty("encodedSampleSpace").GetBoolean()
                    || Ready.GetProperty("alphaTolerance1").GetBoolean() != (depth == 16 || up8Compatibility)
                    || Ready.GetProperty("zeroAlphaNormalizationTimed").GetBoolean() != up8Compatibility))
                || (jpegRestart && (depth != 8 || jpegSampling != "420" || !Ready.GetProperty("serialExecutor").GetBoolean()
                    || Ready.GetProperty("serialSlices").GetInt32() != 2 || Ready.GetProperty("restartInterval").GetInt32() != 33
                    || Ready.GetProperty("restartMarkers").GetInt32() != 16))
                || (jpeg && (Ready.GetProperty("conversionThreads").GetInt32() != 1 || Ready.GetProperty("qscale").GetInt32() != nativeQuality
                    || Ready.GetProperty("backgroundRgb").GetInt32() != 0x204060 || Ready.GetProperty("sampling").GetString() != (jpegSampling ?? "444")
                    || !Ready.GetProperty("colorConversionTimed").GetBoolean() || !Ready.GetProperty("flatteningTimed").GetBoolean()
                    || !Ready.GetProperty("bitstreamDeterministic").GetBoolean()
                    || Ready.GetProperty("fullSourceNormalizationTimed").GetBoolean() != (depth == 16)
                    || (depth == 16 && !Ready.GetProperty("normalizedReferenceExact").GetBoolean())))
                || (webp && (Ready.GetProperty("conversionThreads").GetInt32() != 1 || Ready.GetProperty("quality").GetInt32() != nativeQuality
                    || Ready.GetProperty("effort").GetInt32() != vp8Effort || Ready.GetProperty("lossless").GetBoolean() != webpLossless
                    || Ready.GetProperty("inputFormat").GetString() != "BGRA" || !Ready.GetProperty("repackingTimed").GetBoolean()
                    || !Ready.GetProperty("bitstreamDeterministic").GetBoolean()
                    || (webp && (Ready.GetProperty("fullSourceNormalizationTimed").GetBoolean() != (depth == 16)
                        || depth == 16 && !Ready.GetProperty("normalizedReferenceExact").GetBoolean()))))
                || (webpLossless && (!Ready.GetProperty("exactRgb").GetBoolean() || !Ready.GetProperty("inputImportAllocationTimed").GetBoolean()))
                || (jpegDecode && (depth != 8 || Ready.GetProperty("conversionThreads").GetInt32() != 1
                    || Ready.GetProperty("decoder").GetString() != "mjpeg" || Ready.GetProperty("sampling").GetString() != (operation is "jpeg420-decode" or "jpeg420-stream-decode" or "jpeg-rgb420-decode" ? "420" : operation is "jpeg422-decode" or "jpeg-rgb422-decode" ? "422" : operation == "jpeg-gray-decode" ? "gray" : "444")
                    || Ready.GetProperty("chromaExpansionTimed").GetBoolean() != (operation is "jpeg420-decode" or "jpeg420-stream-decode" or "jpeg422-decode" or "jpeg-rgb420-decode")
                    || Ready.GetProperty("grayscale").GetBoolean() != (operation == "jpeg-gray-decode")
                    || Ready.GetProperty("chromaVerticalFactor").GetInt32() != (operation is "jpeg420-decode" or "jpeg420-stream-decode" or "jpeg-rgb420-decode" ? 2 : 1)
                    || !Ready.GetProperty("colorConversionTimed").GetBoolean() || !Ready.GetProperty("stateResetTimed").GetBoolean()
                    || !Ready.GetProperty("pixelsDeterministic").GetBoolean()
                    || Ready.GetProperty("outputBytes").GetInt32() != HorizontalFlipInput.Width * HorizontalFlipInput.Height * 4))
                || (webpDecode && (depth != 8 || Ready.GetProperty("conversionThreads").GetInt32() != 1
                    || Ready.GetProperty("decoder").GetString() != "webp" || Ready.GetProperty("inputFormat").GetString() != "ARGB"
                    || !Ready.GetProperty("lossless").GetBoolean() || !Ready.GetProperty("colorConversionTimed").GetBoolean()
                    || !Ready.GetProperty("stateResetTimed").GetBoolean() || !Ready.GetProperty("pixelsDeterministic").GetBoolean()
                    || Ready.GetProperty("outputBytes").GetInt32() != HorizontalFlipInput.Width * HorizontalFlipInput.Height * 4))
                || (vp8Decode && (depth != 8 || Ready.GetProperty("conversionThreads").GetInt32() != 1
                    || Ready.GetProperty("decoder").GetString() != "webp" || Ready.GetProperty("inputFormat").GetString() != (vp8Opaque ? "YUV420P" : "YUVA420P")
                    || Ready.GetProperty("alphaMergeTimed").GetBoolean() == vp8Opaque
                    || Ready.GetProperty("lossless").GetBoolean() || !Ready.GetProperty("colorConversionTimed").GetBoolean()
                    || !Ready.GetProperty("stateResetTimed").GetBoolean() || !Ready.GetProperty("pixelsDeterministic").GetBoolean()
                    || Ready.GetProperty("outputBytes").GetInt32() != HorizontalFlipInput.Width * HorizontalFlipInput.Height * 4))
                || (operation is "png-encode" or "png-smallest8-encode" or "png-smallest16-encode" or "png-fast16-encode" or "png-store16-encode" or "png-store-encode" or "png-rgb-encode" or "png-gray-encode" or "png-gray-alpha-encode" && (Ready.GetProperty("compressionLevel").GetInt32() != (operation is "png-smallest8-encode" or "png-smallest16-encode" ? 9 : operation == "png-fast16-encode" ? 1 : operation is "png-store-encode" or "png-store16-encode" ? 0 : 6) || Ready.GetProperty("filter").GetInt32() != 0)))
                throw new InvalidOperationException("Native warm worker geometry/thread/CPU contract differs.");
            completed = codec || nearest || bilinear || bicubic || crop || vertical || halfTurn || clockwise || counterclockwise || transpose || transverse || normal ? Ready.GetProperty("initialCompletedOperations").GetInt64() : 1;
        }
        catch { Dispose(); throw; }
    }

    public NativeWarmReference(JpegBenchmarkCase state, int cpu, JpegSampleFormat format, string frozenPacket)
    {
        var kind = format switch { JpegSampleFormat.Grayscale => "gray", JpegSampleFormat.Rgb => "rgb", _ => throw new ArgumentOutOfRangeException(nameof(format)) };
        Executable = Path.Combine(state.Root, "external", "warm-reference", $"jpeg-samples-{kind}-warm");
        using (var document = JsonDocument.Parse(File.ReadAllText(Path.Combine(state.Root, "external", "warm-reference", $"jpeg-samples-{kind}-identity.json"))))
            BuildIdentity = document.RootElement.Clone();
        if (!BuildIdentity.GetProperty("simdEnabled").GetBoolean()
            || BuildIdentity.GetProperty("workerSha256").GetString() != JpegBenchmarkCase.HashFile(Executable)
            || BuildIdentity.GetProperty("sourceSha256").GetString() != JpegBenchmarkCase.HashFile(Path.Combine(state.Root, "tools/WarmReference/jpeg-samples-warm.c"))
            || BuildIdentity.GetProperty("buildScriptSha256").GetString() != JpegBenchmarkCase.HashFile(Path.Combine(state.Root, "tools/WarmReference/build-jpeg-samples-linux.sh")))
            throw new InvalidOperationException("Raw JPEG reference source/build identity differs.");
        var start = new ProcessStartInfo(Executable) { RedirectStandardInput = true, RedirectStandardOutput = true, RedirectStandardError = true, UseShellExecute = false };
        foreach (var key in start.Environment.Keys.Where(key => key.StartsWith("JSIMD_FORCE", StringComparison.Ordinal)).ToArray()) start.Environment.Remove(key);
        foreach (var argument in new[] { "jpeg-samples-encode", state.Input, "513", "259", state.NativeOutput, frozenPacket }) start.ArgumentList.Add(argument);
        process = Process.Start(start) ?? throw new InvalidOperationException("Could not start raw JPEG reference.");
        errors = process.StandardError.ReadToEndAsync();
        try
        {
            Ready = Read();
            if (Ready.GetProperty("state").GetString() != "ready" || Ready.GetProperty("operation").GetString() != "jpeg-samples-encode"
                || Ready.GetProperty("sampleFormat").GetString() != kind || Ready.GetProperty("width").GetInt32() != 513 || Ready.GetProperty("height").GetInt32() != 259
                || Ready.GetProperty("cpu").GetInt32() != cpu || Ready.GetProperty("affinityCount").GetInt32() != 1
                || Ready.GetProperty("codecThreads").GetInt32() != 1 || Ready.GetProperty("conversionThreads").GetInt32() != 1
                || new[] { "rawInputBorrowed", "normalizationTimed", "outputAcquisitionAndReleaseTimed", "independentLiveOutputs", "frozenPacketExact" }.Any(key => !Ready.GetProperty(key).GetBoolean()))
                throw new InvalidOperationException("Raw JPEG reference workload/thread/ownership differs.");
            completed = Ready.GetProperty("initialCompletedOperations").GetInt64();
        }
        catch { Dispose(); throw; }
    }

    private JsonElement Read()
    {
        var line = process.StandardOutput.ReadLineAsync().WaitAsync(TimeSpan.FromSeconds(30)).GetAwaiter().GetResult();
        if (line is null)
            throw new InvalidOperationException("Native warm worker ended: " + errors.WaitAsync(TimeSpan.FromSeconds(5)).GetAwaiter().GetResult());
        using var document = JsonDocument.Parse(line);
        return document.RootElement.Clone();
    }

    public NativeWarmSample Sample(long operations)
    {
        if (operations is < 1 or > 10_000_000) throw new ArgumentOutOfRangeException(nameof(operations));
        process.StandardInput.WriteLine("sample " + operations.ToString(CultureInfo.InvariantCulture));
        process.StandardInput.Flush();
        var response = Read();
        var sample = response.Deserialize<NativeWarmSample>(Options) ?? throw new InvalidOperationException("Missing native sample.");
        if (sample.State != "sample" || sample.Operations != operations || sample.CompletedOperations != completed + operations
            || sample.ElapsedNanoseconds <= 0 || !double.IsFinite(sample.ElapsedNanoseconds)
            || (sample.CpuElapsedNanoseconds is { } cpuElapsed && (cpuElapsed <= 0 || !double.IsFinite(cpuElapsed)))
            || sample.Cpu != Ready.GetProperty("cpu").GetInt32()
            || ((Ready.TryGetProperty("metadataOnly", out var metadataOnly) && metadataOnly.GetBoolean())
                || Ready.GetProperty("operation").GetString() is "png-header-inspect" or "webp-lossless-nonseekable-encode" or "jpeg-samples-nonseekable-encode" or "jpeg-nonseekable-encode" or "png-encode" or "png-nonseekable-encode" or "png-smallest8-encode" or "png-smallest16-encode" or "png-fast16-encode" or "png-store16-encode" or "png-store-encode" or "png-rgb-encode" or "png-gray-encode" or "png-gray-alpha-encode" or "jpeg-encode" or "jpeg-gray-encode" or "jpeg-restart-encode" or "jpeg-samples-encode" or "webp-encode" or "webp-opaque-encode" or "webp-lossless-encode" or "png-identify" or "jpeg-identify" or "webp-identify" or "png-stream-identify" or "png-nonseekable-identify" ? sample.OutputStride != 0 || sample.OutputBytes != Ready.GetProperty("outputBytes").GetInt32()
                : Ready.GetProperty("operation").GetString() == "jpeg-gray-samples-decode" ? sample.OutputStride != 513 || sample.OutputBytes != 513 * 259
                : Ready.GetProperty("operation").GetString() is "jpeg-rgb-samples-decode" or "jpeg-ycck-rgb-decode" ? sample.OutputStride != 513 * 3 || sample.OutputBytes != 513 * 259 * 3
                : sample.OutputStride < (Ready.GetProperty("operation").GetString() is "nearest-resize" or "bilinear-resize" or "bicubic-resize" or "lanczos3-resize" or "crop" or "png-stream-crop-fit" or "png-stream16-crop-fit" ? Ready.GetProperty("outputWidth").GetInt32()
                    : Ready.GetProperty("operation").GetString() is "quarter-turn-cw" or "quarter-turn-ccw" or "transpose" or "transverse" ? 259 : HorizontalFlipInput.Width) * (Ready.GetProperty("depth").GetInt32() / 8) * 4)
            || (Ready.GetProperty("operation").GetString() is "png-stream-decode" or "png-stream16-decode" or "png-stream-crop-fit" or "png-stream16-crop-fit" && sample.OutputBytes != Ready.GetProperty("outputBytes").GetInt32())
            || (Ready.GetProperty("operation").GetString() == "jpeg-cmyk-decode" && sample.OutputBytes != Ready.GetProperty("outputBytes").GetInt32())
            || (Ready.GetProperty("operation").GetString() is "jpeg-decode" or "jpeg-rgb-decode" or "jpeg-rgb422-decode" or "jpeg-rgb420-decode" or "webp-decode" or "webp-stream-decode" or "vp8-decode" or "vp8-stream-decode" && sample.OutputBytes != HorizontalFlipInput.Width * HorizontalFlipInput.Height * 4))
            throw new InvalidOperationException("Native sample counters/elapsed time/CPU differ.");
        completed += operations;
        return sample;
    }

    public NativeWarmSample[] Measure(bool smoke, long? fixedCount = null, int sampleCount = 3)
    {
        if (sampleCount is not (3 or 12)) throw new ArgumentOutOfRangeException(nameof(sampleCount));
        // Shorter complete-operation batches allow prepared quiet confirmations
        // to finish without carrying their own sustained work into the load guard.
        // Historical jobs keep 200 ms; preparation must declare 50 ms explicitly.
        var targetMilliseconds = Environment.GetEnvironmentVariable("LOKAD_IMAGE_WARM_NATIVE_SAMPLE_MS") switch
        {
            null or "200" => 200,
            "50" => 50,
            _ => throw new InvalidOperationException("Native warm sample target must be 50 or 200 ms.")
        };
        if (smoke) return [Sample(1)];
        var count = fixedCount ?? 1;
        if (fixedCount is null)
            while (Sample(count).ElapsedNanoseconds < targetMilliseconds * 1_000_000L)
                count = checked(count * 2);
        for (var i = 0; i < 3; i++) Sample(count);
        return Enumerable.Range(0, sampleCount).Select(_ => Sample(count)).ToArray();
    }

    public void Check()
    {
        process.StandardInput.WriteLine("check"); process.StandardInput.Flush();
        var value = Read();
        var verified = Ready.GetProperty("operation").GetString() is "bilinear-resize" or "bicubic-resize" or "lanczos3-resize"
            ? value.GetProperty("matched").GetBoolean()
                && (Ready.GetProperty("alphaTolerance1").GetBoolean()
                    ? value.GetProperty("alphaMaximum").GetInt32() is >= 0 and <= 1
                        && value.GetProperty("alphaMeanSquared").GetDouble() is >= 0 and <= 0.0625
                    : value.GetProperty("alphaExact").GetBoolean())
            : value.GetProperty("exact").GetBoolean();
        if (value.GetProperty("state").GetString() != "checked" || !verified
            || !value.GetProperty("sourceUnchanged").GetBoolean())
            throw new InvalidOperationException("Native final exact/source check failed.");
        completed++;
    }

    public void Stop()
    {
        if (stopped) return;
        process.StandardInput.WriteLine("quit"); process.StandardInput.Flush(); process.StandardInput.Close();
        if (!process.WaitForExit(10_000)) throw new TimeoutException("Native worker did not stop.");
        var message = errors.GetAwaiter().GetResult();
        if (process.ExitCode != 0 || !string.IsNullOrWhiteSpace(message))
            throw new InvalidOperationException("Native warm worker failed: " + message);
        stopped = true;
    }

    public void Dispose()
    {
        if (disposed) return;
        try { if (!stopped && !process.HasExited) process.Kill(entireProcessTree: true); }
        finally { process.Dispose(); disposed = true; }
    }
}

internal sealed record NativeWarmSample(string State, long Operations, double ElapsedNanoseconds,
    long CompletedOperations, ulong Observed, int OutputStride, int Cpu, int OutputBytes = 0,
    double? CpuElapsedNanoseconds = null)
{
    public double NanosecondsPerOperation => ElapsedNanoseconds / Operations;
}
