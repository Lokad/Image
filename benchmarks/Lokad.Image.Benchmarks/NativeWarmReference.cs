using System.Diagnostics;
using System.Globalization;
using System.Text.Json;

// Original development worker; no bindings or native assets enter Lokad.Image.
internal sealed class NativeWarmReference : IDisposable
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

    public NativeWarmReference(JpegBenchmarkCase state, int depth, int cpu, string operation = "hflip", string? expectedRaw = null, int? nativeQuality = null, string? jpegSampling = null, bool vp8Opaque = false, int? resizeWidth = null, int? resizeHeight = null, bool bicubicScalar = false)
    {
        if (resizeWidth.HasValue != resizeHeight.HasValue || resizeWidth.HasValue &&
            (operation is not ("bilinear-resize" or "nearest-resize" or "bicubic-resize")
                || (resizeWidth < 256 || resizeHeight < 129) && !(operation == "bicubic-resize" && depth is (8 or 16) && resizeWidth == 128 && resizeHeight == 64) && !(operation == "bicubic-resize" && depth is (8 or 16) && resizeWidth == 64 && resizeHeight == 32) && !(operation == "bicubic-resize" && depth is (8 or 16) && resizeWidth == 32 && resizeHeight == 16) && !(operation == "bicubic-resize" && depth is (8 or 16) && resizeWidth == 16 && resizeHeight == 8) && !(operation == "bicubic-resize" && depth == 16 && resizeWidth == 8 && resizeHeight == 4)
                || resizeWidth > 32768 || resizeHeight > 32768))
            throw new ArgumentException("Explicit resize dimensions require a supported bounded resize geometry.");
        if (vp8Opaque && operation != "vp8-decode") throw new ArgumentException("Opaque VP8 applies only to VP8 decoding.", nameof(vp8Opaque));
        if (jpegSampling is not (null or "444" or "420" or "422") || jpegSampling is not null && operation != "jpeg-encode")
            throw new ArgumentException("JPEG sampling applies only to the 444/420/422 encode worker.", nameof(jpegSampling));
        if (operation is not ("hflip" or "normal-orient" or "transverse" or "transpose" or "quarter-turn-ccw" or "quarter-turn-cw" or "half-turn" or "vflip" or "crop" or "png-encode" or "png-store-encode" or "png-rgb-encode" or "png-gray-encode" or "png-gray-alpha-encode" or "png-decode" or "png-adam7-decode" or "jpeg-encode" or "jpeg-decode" or "jpeg420-decode" or "jpeg422-decode" or "jpeg-gray-decode" or "webp-encode" or "webp-lossless-encode" or "webp-decode" or "vp8-decode" or "nearest-resize" or "bilinear-resize" or "bicubic-resize")) throw new ArgumentOutOfRangeException(nameof(operation));
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
        var bicubicUpsample = bicubic && depth is (8 or 16) && resizeWidth == 1027 && resizeHeight == 519;
        var bicubicMixed = bicubic && depth is (8 or 16) && resizeWidth == 1027 && resizeHeight == 129;
        var bicubicCross = bicubic && depth is (8 or 16) && resizeWidth == 256 && resizeHeight == 519;
        var bicubicWide = bicubic && depth is (8 or 16) && resizeWidth == 128 && resizeHeight == 64;
        var bicubicBeyond256 = bicubic && depth == 16 && resizeWidth == 8 && resizeHeight == 4;
        var bicubicBeyond128 = bicubic && depth is (8 or 16) && resizeWidth == 16 && resizeHeight == 8;
        var bicubicBeyond64 = bicubic && depth is (8 or 16) && resizeWidth == 32 && resizeHeight == 16;
        var bicubicBeyond32 = bicubic && depth is (8 or 16) && resizeWidth == 64 && resizeHeight == 32;
        var bicubicHorizontalUp = bicubic && depth is (8 or 16) && resizeWidth == 1027 && resizeHeight == 259;
        var bicubicVerticalUp = bicubic && depth is (8 or 16) && resizeWidth == 513 && resizeHeight == 519;
        var bicubicVertical = bicubic && depth is (8 or 16) && resizeWidth == 513 && resizeHeight == 129;
        var bicubicHorizontal = bicubic && depth is (8 or 16) && resizeWidth == 256 && resizeHeight == 259;
        if (bicubic && resizeWidth.HasValue && !bicubicUpsample && !bicubicMixed && !bicubicCross && !bicubicWide && !bicubicHorizontal && !bicubicBeyond32 && !bicubicVertical && !bicubicHorizontalUp && !bicubicVerticalUp && !bicubicBeyond64 && !bicubicBeyond128 && !bicubicBeyond256) throw new ArgumentException("Explicit bicubic geometry is limited to a selected fixed case.");
        if (bicubicScalar && !bicubic) throw new ArgumentException("Scalar backend applies only to the bicubic worker.");
        var codec = operation is not ("hflip" or "normal-orient" or "transverse" or "transpose" or "quarter-turn-ccw" or "quarter-turn-cw" or "half-turn" or "vflip" or "crop" or "nearest-resize" or "bilinear-resize" or "bicubic-resize"); var jpeg = operation == "jpeg-encode"; var jpegDecode = operation is "jpeg-decode" or "jpeg420-decode" or "jpeg422-decode" or "jpeg-gray-decode"; var webpLossless = operation == "webp-lossless-encode"; var webp = operation == "webp-encode" || webpLossless; var webpDecode = operation == "webp-decode"; var vp8Decode = operation == "vp8-decode";
        var stem = bicubicBeyond256 ? "zimg-bicubic-down8x4-16-warm" : bicubicBeyond128 ? depth == 16 ? "zimg-bicubic-down16x8-16-warm" : "zimg-bicubic-down16x8-8-warm" : bicubicBeyond64 ? depth == 16 ? "zimg-bicubic-down32x16-16-warm" : "zimg-bicubic-down32x16-8-warm" : bicubicVerticalUp ? depth == 16 ? "zimg-bicubic-vertical-up16-warm" : "zimg-bicubic-vertical-up8-warm" : bicubicHorizontalUp ? depth == 16 ? "zimg-bicubic-horizontal-up16-warm" : "zimg-bicubic-horizontal-up8-warm" : bicubicVertical ? depth == 16 ? "zimg-bicubic-vertical16-warm" : "zimg-bicubic-vertical8-warm" : bicubicBeyond32 ? depth == 16 ? "zimg-bicubic-down64x32-16-warm" : "zimg-bicubic-down64x32-8-warm" : bicubicHorizontal ? depth == 16 ? "zimg-bicubic-horizontal16-warm" : "zimg-bicubic-horizontal8-warm" : bicubicWide ? depth == 16 ? "zimg-bicubic-wide16-warm" : "zimg-bicubic-wide8-warm" : bicubicCross ? depth == 16 ? "zimg-bicubic-cross16-warm" : "zimg-bicubic-cross8-warm" : bicubicMixed ? depth == 16 ? "zimg-bicubic-mixed16-warm" : "zimg-bicubic-mixed8-warm" : bicubicUpsample ? depth == 16 ? "zimg-bicubic-up16-warm" : "zimg-bicubic-up8-warm" : bicubic ? depth == 16 ? "zimg-bicubic16-warm" : "zimg-bicubic-warm" : transverse ? "ffmpeg-transverse-warm" : transpose ? "ffmpeg-transpose-warm" : counterclockwise ? "ffmpeg-counterclockwise-warm" : normal ? "ffmpeg-normal-orient-warm" : clockwise ? "ffmpeg-clockwise-warm" : halfTurn ? "ffmpeg-halfturn-warm" : vertical ? "ffmpeg-vflip-warm" : crop ? "ffmpeg-crop-warm" : bilinear ? "ffmpeg-bilinear-warm" : nearest ? "ffmpeg-nearest-warm" : vp8Decode ? "ffmpeg-vp8-decode-warm" : webpDecode ? "ffmpeg-webp-decode-warm" : jpegDecode ? "ffmpeg-jpeg-decode-warm" : webpLossless ? "libwebp-lossless-warm" : webp ? "ffmpeg-webp-warm" : jpeg ? "ffmpeg-jpeg-warm" : codec ? "ffmpeg-png-warm" : "ffmpeg-warm";
        var buildScript = bicubicBeyond256 ? "build-bicubic-down8x4-16-linux.sh" : bicubicBeyond128 ? depth == 16 ? "build-bicubic-down16x8-16-linux.sh" : "build-bicubic-down16x8-8-linux.sh" : bicubicBeyond64 ? depth == 16 ? "build-bicubic-down32x16-16-linux.sh" : "build-bicubic-down32x16-8-linux.sh" : bicubicVerticalUp ? depth == 16 ? "build-bicubic-vertical-up16-linux.sh" : "build-bicubic-vertical-up8-linux.sh" : bicubicHorizontalUp ? depth == 16 ? "build-bicubic-horizontal-up16-linux.sh" : "build-bicubic-horizontal-up8-linux.sh" : bicubicVertical ? depth == 16 ? "build-bicubic-vertical16-linux.sh" : "build-bicubic-vertical8-linux.sh" : bicubicBeyond32 ? depth == 16 ? "build-bicubic-down64x32-16-linux.sh" : "build-bicubic-down64x32-8-linux.sh" : bicubicHorizontal ? depth == 16 ? "build-bicubic-horizontal16-linux.sh" : "build-bicubic-horizontal8-linux.sh" : bicubicWide ? depth == 16 ? "build-bicubic-wide16-linux.sh" : "build-bicubic-wide8-linux.sh" : bicubicCross ? depth == 16 ? "build-bicubic-cross16-linux.sh" : "build-bicubic-cross8-linux.sh" : bicubicMixed ? depth == 16 ? "build-bicubic-mixed16-linux.sh" : "build-bicubic-mixed8-linux.sh" : bicubicUpsample ? depth == 16 ? "build-bicubic-up16-linux.sh" : "build-bicubic-up8-linux.sh" : bicubic ? depth == 16 ? "build-bicubic16-linux.sh" : "build-bicubic-linux.sh" : transverse ? "build-transverse-linux.sh" : transpose ? "build-transpose-linux.sh" : counterclockwise ? "build-counterclockwise-linux.sh" : normal ? "build-normal-orient-linux.sh" : clockwise ? "build-clockwise-linux.sh" : halfTurn ? "build-halfturn-linux.sh" : vertical ? "build-vflip-linux.sh" : crop ? "build-crop-linux.sh" : bilinear ? "build-bilinear-linux.sh" : nearest ? "build-nearest-linux.sh" : vp8Decode ? "build-vp8-decode-linux.sh" : webpDecode ? "build-webp-decode-linux.sh" : jpegDecode ? "build-jpeg-decode-linux.sh" : webpLossless ? "build-libwebp-lossless-linux.sh" : webp ? "build-webp-linux.sh" : jpeg ? "build-jpeg-linux.sh" : codec ? "build-png-linux.sh" : "build-linux.sh";
        Executable = Path.Combine(state.Root, "external", "warm-reference", stem);
        var identityPath = Path.Combine(Path.GetDirectoryName(Executable)!, bicubicBeyond256 ? "bicubic-down8x4-16-identity.json" : bicubicBeyond128 ? depth == 16 ? "bicubic-down16x8-16-identity.json" : "bicubic-down16x8-8-identity.json" : bicubicBeyond64 ? depth == 16 ? "bicubic-down32x16-16-identity.json" : "bicubic-down32x16-8-identity.json" : bicubicVerticalUp ? depth == 16 ? "bicubic-vertical-up16-identity.json" : "bicubic-vertical-up8-identity.json" : bicubicHorizontalUp ? depth == 16 ? "bicubic-horizontal-up16-identity.json" : "bicubic-horizontal-up8-identity.json" : bicubicVertical ? depth == 16 ? "bicubic-vertical16-identity.json" : "bicubic-vertical8-identity.json" : bicubicBeyond32 ? depth == 16 ? "bicubic-down64x32-16-identity.json" : "bicubic-down64x32-8-identity.json" : bicubicHorizontal ? depth == 16 ? "bicubic-horizontal16-identity.json" : "bicubic-horizontal8-identity.json" : bicubicWide ? depth == 16 ? "bicubic-wide16-identity.json" : "bicubic-wide8-identity.json" : bicubicCross ? depth == 16 ? "bicubic-cross16-identity.json" : "bicubic-cross8-identity.json" : bicubicMixed ? depth == 16 ? "bicubic-mixed16-identity.json" : "bicubic-mixed8-identity.json" : bicubicUpsample ? depth == 16 ? "bicubic-up16-identity.json" : "bicubic-up8-identity.json" : bicubic ? depth == 16 ? "bicubic16-identity.json" : "bicubic-identity.json" : transverse ? "transverse-identity.json" : transpose ? "transpose-identity.json" : counterclockwise ? "counterclockwise-identity.json" : normal ? "normal-orient-identity.json" : clockwise ? "clockwise-identity.json" : halfTurn ? "halfturn-identity.json" : vertical ? "vflip-identity.json" : crop ? "crop-identity.json" : bilinear ? "bilinear-identity.json" : nearest ? "nearest-identity.json" : vp8Decode ? "vp8-decode-identity.json" : webpDecode ? "webp-decode-identity.json" : jpegDecode ? "jpeg-decode-identity.json" : webpLossless ? "libwebp-lossless-identity.json" : webp ? "webp-identity.json" : jpeg ? "jpeg-identity.json" : codec ? "png-identity.json" : "identity.json");
        if (!File.Exists(Executable) || !File.Exists(identityPath))
            throw new InvalidOperationException("Build the pinned Linux warm reference with its tools/WarmReference build script.");
        using (var document = JsonDocument.Parse(File.ReadAllText(identityPath))) BuildIdentity = document.RootElement.Clone();
        if (BuildIdentity.GetProperty("workerSha256").GetString() != JpegBenchmarkCase.HashFile(Executable)
            || BuildIdentity.GetProperty("sourceSha256").GetString() != JpegBenchmarkCase.HashFile(Path.Combine(state.Root, "tools", "WarmReference", (bicubicMixed || bicubicCross || bicubicWide || bicubicHorizontal || bicubicBeyond32 || bicubicVertical || bicubicHorizontalUp || bicubicVerticalUp || bicubicBeyond64 || bicubicBeyond128 || bicubicBeyond256 ? "zimg-bicubic-mixed8-warm" : stem) + ".c"))
            || BuildIdentity.GetProperty("buildScriptSha256").GetString() != JpegBenchmarkCase.HashFile(Path.Combine(state.Root, "tools", "WarmReference", buildScript)))
            throw new InvalidOperationException("Native reference build identity differs from the current source/binary.");
        var start = new ProcessStartInfo(Executable) { RedirectStandardInput = true, RedirectStandardOutput = true,
            RedirectStandardError = true, UseShellExecute = false };
        foreach (var argument in new[] { operation, state.Input, "513", "259", depth.ToString(CultureInfo.InvariantCulture), state.NativeOutput })
            start.ArgumentList.Add(argument);
        if (operation is "png-decode" or "png-adam7-decode" or "bicubic-resize") start.ArgumentList.Add(expectedRaw ?? throw new ArgumentNullException(nameof(expectedRaw)));
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
            if (Ready.GetProperty("state").GetString() != "ready" || Ready.GetProperty("schemaVersion").GetInt32() != 1
                || Ready.GetProperty("width").GetInt32() != HorizontalFlipInput.Width
                || Ready.GetProperty("height").GetInt32() != HorizontalFlipInput.Height
                || Ready.GetProperty("depth").GetInt32() != depth || Ready.GetProperty("cpu").GetInt32() != cpu
                || Ready.GetProperty("affinityCount").GetInt32() != 1 || Ready.GetProperty(codec ? "codecThreads" : "filterThreads").GetInt32() != 1
                || Ready.GetProperty("threadType").GetInt32() != 0 || Ready.GetProperty("operation").GetString() != operation
                || (codec && (Ready.GetProperty("activeThreadType").GetInt32() != 0 || !Ready.GetProperty("independentLiveOutputs").GetBoolean()))
                || ((nearest || bilinear || bicubic || crop) && ((depth is not (8 or 16)) || Ready.GetProperty("outputWidth").GetInt32() != (resizeWidth ?? 256)
                    || Ready.GetProperty("outputHeight").GetInt32() != (resizeHeight ?? 129) || !Ready.GetProperty("independentLiveOutputs").GetBoolean()))
                || ((bicubicUpsample || bicubicMixed || bicubicCross || bicubicWide || bicubicHorizontal || bicubicBeyond32 || bicubicVertical || bicubicHorizontalUp || bicubicVerticalUp || bicubicBeyond64 || bicubicBeyond128 || bicubicBeyond256) && (!Ready.GetProperty("premultipliedFloatOutput").GetBoolean() || !Ready.GetProperty("unpremultiplicationAndQuantizationTimed").GetBoolean()))
                || (bicubicVerticalUp && depth==16 && (!Ready.GetProperty("lowAlphaRgbAllowance").GetBoolean() || Ready.GetProperty("lowAlphaRgbReferenceMaximum").GetInt32()!=16 || Ready.GetProperty("lowAlphaRgbErrorMaximum").GetInt32()!=2))
                || (bicubic && (depth is not (8 or 16) || Ready.GetProperty("scalar").GetBoolean() != bicubicScalar
                    || Ready.GetProperty("haloPixels").GetInt32() != (bicubicBeyond256 ? 128 : bicubicBeyond128 ? 64 : bicubicBeyond64 ? 32 : bicubicBeyond32 ? 16 : bicubicWide ? 8 : 4) || !Ready.GetProperty("haloAndPackingTimed").GetBoolean()
                    || !Ready.GetProperty("outputAllocationAndRepackingTimed").GetBoolean() || !Ready.GetProperty("alphaTolerance1").GetBoolean()
                    || Ready.GetProperty("backend").GetString() != "zimg (FFmpeg zscale backend)"))
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
                || (jpeg && (Ready.GetProperty("conversionThreads").GetInt32() != 1 || Ready.GetProperty("qscale").GetInt32() != nativeQuality
                    || Ready.GetProperty("backgroundRgb").GetInt32() != 0x204060 || Ready.GetProperty("sampling").GetString() != (jpegSampling ?? "444")
                    || !Ready.GetProperty("colorConversionTimed").GetBoolean() || !Ready.GetProperty("flatteningTimed").GetBoolean()
                    || !Ready.GetProperty("bitstreamDeterministic").GetBoolean()
                    || Ready.GetProperty("fullSourceNormalizationTimed").GetBoolean() != (depth == 16)
                    || (depth == 16 && !Ready.GetProperty("normalizedReferenceExact").GetBoolean())))
                || (webp && (Ready.GetProperty("conversionThreads").GetInt32() != 1 || Ready.GetProperty("quality").GetInt32() != nativeQuality
                    || Ready.GetProperty("effort").GetInt32() != 4 || Ready.GetProperty("lossless").GetBoolean() != webpLossless
                    || Ready.GetProperty("inputFormat").GetString() != "BGRA" || !Ready.GetProperty("repackingTimed").GetBoolean()
                    || !Ready.GetProperty("bitstreamDeterministic").GetBoolean()
                    || (webp && (Ready.GetProperty("fullSourceNormalizationTimed").GetBoolean() != (depth == 16)
                        || depth == 16 && !Ready.GetProperty("normalizedReferenceExact").GetBoolean()))))
                || (webpLossless && (!Ready.GetProperty("exactRgb").GetBoolean() || !Ready.GetProperty("inputImportAllocationTimed").GetBoolean()))
                || (jpegDecode && (depth != 8 || Ready.GetProperty("conversionThreads").GetInt32() != 1
                    || Ready.GetProperty("decoder").GetString() != "mjpeg" || Ready.GetProperty("sampling").GetString() != (operation == "jpeg420-decode" ? "420" : operation == "jpeg422-decode" ? "422" : operation == "jpeg-gray-decode" ? "gray" : "444")
                    || Ready.GetProperty("chromaExpansionTimed").GetBoolean() != (operation is "jpeg420-decode" or "jpeg422-decode")
                    || Ready.GetProperty("grayscale").GetBoolean() != (operation == "jpeg-gray-decode")
                    || Ready.GetProperty("chromaVerticalFactor").GetInt32() != (operation == "jpeg420-decode" ? 2 : 1)
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
                || (operation is "png-encode" or "png-store-encode" or "png-rgb-encode" or "png-gray-encode" or "png-gray-alpha-encode" && (Ready.GetProperty("compressionLevel").GetInt32() != (operation == "png-store-encode" ? 0 : 6) || Ready.GetProperty("filter").GetInt32() != 0)))
                throw new InvalidOperationException("Native warm worker geometry/thread/CPU contract differs.");
            completed = codec || nearest || bilinear || bicubic || crop || vertical || halfTurn || clockwise || counterclockwise || transpose || transverse || normal ? Ready.GetProperty("initialCompletedOperations").GetInt64() : 1;
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
            || sample.Cpu != Ready.GetProperty("cpu").GetInt32()
            || (Ready.GetProperty("operation").GetString() is "png-encode" or "png-store-encode" or "png-rgb-encode" or "png-gray-encode" or "png-gray-alpha-encode" or "jpeg-encode" or "webp-encode" or "webp-lossless-encode" ? sample.OutputStride != 0 || sample.OutputBytes != Ready.GetProperty("outputBytes").GetInt32()
                : sample.OutputStride < (Ready.GetProperty("operation").GetString() is "nearest-resize" or "bilinear-resize" or "bicubic-resize" or "crop" ? Ready.GetProperty("outputWidth").GetInt32()
                    : Ready.GetProperty("operation").GetString() is "quarter-turn-cw" or "quarter-turn-ccw" or "transpose" or "transverse" ? 259 : HorizontalFlipInput.Width) * (Ready.GetProperty("depth").GetInt32() / 8) * 4)
            || (Ready.GetProperty("operation").GetString() is "jpeg-decode" or "webp-decode" or "vp8-decode" && sample.OutputBytes != HorizontalFlipInput.Width * HorizontalFlipInput.Height * 4))
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
        var verified = Ready.GetProperty("operation").GetString() is "bilinear-resize" or "bicubic-resize"
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
    long CompletedOperations, ulong Observed, int OutputStride, int Cpu, int OutputBytes = 0)
{
    public double NanosecondsPerOperation => ElapsedNanoseconds / Operations;
}
