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

    public NativeWarmReference(JpegBenchmarkCase state, int depth, int cpu, string operation = "hflip", string? expectedRaw = null, int? nativeQuality = null)
    {
        if (operation is not ("hflip" or "png-encode" or "png-decode" or "jpeg-encode" or "jpeg-decode" or "webp-encode" or "webp-decode" or "vp8-decode" or "nearest-resize" or "bilinear-resize")) throw new ArgumentOutOfRangeException(nameof(operation));
        var nearest = operation == "nearest-resize";
        var bilinear = operation == "bilinear-resize";
        var codec = operation is not ("hflip" or "nearest-resize" or "bilinear-resize"); var jpeg = operation == "jpeg-encode"; var jpegDecode = operation == "jpeg-decode"; var webp = operation == "webp-encode"; var webpDecode = operation == "webp-decode"; var vp8Decode = operation == "vp8-decode";
        var stem = bilinear ? "ffmpeg-bilinear-warm" : nearest ? "ffmpeg-nearest-warm" : vp8Decode ? "ffmpeg-vp8-decode-warm" : webpDecode ? "ffmpeg-webp-decode-warm" : jpegDecode ? "ffmpeg-jpeg-decode-warm" : webp ? "ffmpeg-webp-warm" : jpeg ? "ffmpeg-jpeg-warm" : codec ? "ffmpeg-png-warm" : "ffmpeg-warm";
        var buildScript = bilinear ? "build-bilinear-linux.sh" : nearest ? "build-nearest-linux.sh" : vp8Decode ? "build-vp8-decode-linux.sh" : webpDecode ? "build-webp-decode-linux.sh" : jpegDecode ? "build-jpeg-decode-linux.sh" : webp ? "build-webp-linux.sh" : jpeg ? "build-jpeg-linux.sh" : codec ? "build-png-linux.sh" : "build-linux.sh";
        Executable = Path.Combine(state.Root, "external", "warm-reference", stem);
        var identityPath = Path.Combine(Path.GetDirectoryName(Executable)!, bilinear ? "bilinear-identity.json" : nearest ? "nearest-identity.json" : vp8Decode ? "vp8-decode-identity.json" : webpDecode ? "webp-decode-identity.json" : jpegDecode ? "jpeg-decode-identity.json" : webp ? "webp-identity.json" : jpeg ? "jpeg-identity.json" : codec ? "png-identity.json" : "identity.json");
        if (!File.Exists(Executable) || !File.Exists(identityPath))
            throw new InvalidOperationException("Build the pinned Linux warm reference with its tools/WarmReference build script.");
        using (var document = JsonDocument.Parse(File.ReadAllText(identityPath))) BuildIdentity = document.RootElement.Clone();
        if (BuildIdentity.GetProperty("workerSha256").GetString() != JpegBenchmarkCase.HashFile(Executable)
            || BuildIdentity.GetProperty("sourceSha256").GetString() != JpegBenchmarkCase.HashFile(Path.Combine(state.Root, "tools", "WarmReference", stem + ".c"))
            || BuildIdentity.GetProperty("buildScriptSha256").GetString() != JpegBenchmarkCase.HashFile(Path.Combine(state.Root, "tools", "WarmReference", buildScript)))
            throw new InvalidOperationException("Native reference build identity differs from the current source/binary.");
        var start = new ProcessStartInfo(Executable) { RedirectStandardInput = true, RedirectStandardOutput = true,
            RedirectStandardError = true, UseShellExecute = false };
        foreach (var argument in new[] { operation, state.Input, "513", "259", depth.ToString(CultureInfo.InvariantCulture), state.NativeOutput })
            start.ArgumentList.Add(argument);
        if (operation == "png-decode") start.ArgumentList.Add(expectedRaw ?? throw new ArgumentNullException(nameof(expectedRaw)));
        if (jpeg || webp)
        {
            if (depth != 8 || !nativeQuality.HasValue || nativeQuality < 1 || nativeQuality > (webp ? 100 : 31)) throw new ArgumentOutOfRangeException(nameof(nativeQuality));
            start.ArgumentList.Add(nativeQuality.Value.ToString(CultureInfo.InvariantCulture));
        }
        else if (nativeQuality.HasValue) throw new ArgumentException("Native quality applies only to JPEG or VP8.", nameof(nativeQuality));
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
                || ((nearest || bilinear) && ((nearest ? depth is not (8 or 16) : depth != 8) || Ready.GetProperty("outputWidth").GetInt32() != 256
                    || Ready.GetProperty("outputHeight").GetInt32() != 129 || !Ready.GetProperty("independentLiveOutputs").GetBoolean()))
                || (bilinear && (Ready.GetProperty("kernel").GetString() != "triangle" || Ready.GetProperty("alphaMode").GetString() != "straight"
                    || !Ready.GetProperty("premultipliedAlphaTimed").GetBoolean() || !Ready.GetProperty("encodedSampleSpace").GetBoolean()))
                || (jpeg && (Ready.GetProperty("conversionThreads").GetInt32() != 1 || Ready.GetProperty("qscale").GetInt32() != nativeQuality
                    || Ready.GetProperty("backgroundRgb").GetInt32() != 0x204060 || Ready.GetProperty("sampling").GetString() != "444"
                    || !Ready.GetProperty("colorConversionTimed").GetBoolean() || !Ready.GetProperty("flatteningTimed").GetBoolean()
                    || !Ready.GetProperty("bitstreamDeterministic").GetBoolean()))
                || (webp && (Ready.GetProperty("conversionThreads").GetInt32() != 1 || Ready.GetProperty("quality").GetInt32() != nativeQuality
                    || Ready.GetProperty("effort").GetInt32() != 4 || Ready.GetProperty("lossless").GetBoolean()
                    || Ready.GetProperty("inputFormat").GetString() != "BGRA" || !Ready.GetProperty("repackingTimed").GetBoolean()
                    || !Ready.GetProperty("bitstreamDeterministic").GetBoolean()))
                || (jpegDecode && (depth != 8 || Ready.GetProperty("conversionThreads").GetInt32() != 1
                    || Ready.GetProperty("decoder").GetString() != "mjpeg" || Ready.GetProperty("sampling").GetString() != "444"
                    || !Ready.GetProperty("colorConversionTimed").GetBoolean() || !Ready.GetProperty("stateResetTimed").GetBoolean()
                    || !Ready.GetProperty("pixelsDeterministic").GetBoolean()
                    || Ready.GetProperty("outputBytes").GetInt32() != HorizontalFlipInput.Width * HorizontalFlipInput.Height * 4))
                || (webpDecode && (depth != 8 || Ready.GetProperty("conversionThreads").GetInt32() != 1
                    || Ready.GetProperty("decoder").GetString() != "webp" || Ready.GetProperty("inputFormat").GetString() != "ARGB"
                    || !Ready.GetProperty("lossless").GetBoolean() || !Ready.GetProperty("colorConversionTimed").GetBoolean()
                    || !Ready.GetProperty("stateResetTimed").GetBoolean() || !Ready.GetProperty("pixelsDeterministic").GetBoolean()
                    || Ready.GetProperty("outputBytes").GetInt32() != HorizontalFlipInput.Width * HorizontalFlipInput.Height * 4))
                || (vp8Decode && (depth != 8 || Ready.GetProperty("conversionThreads").GetInt32() != 1
                    || Ready.GetProperty("decoder").GetString() != "webp" || Ready.GetProperty("inputFormat").GetString() != "YUVA420P"
                    || Ready.GetProperty("lossless").GetBoolean() || !Ready.GetProperty("colorConversionTimed").GetBoolean()
                    || !Ready.GetProperty("stateResetTimed").GetBoolean() || !Ready.GetProperty("pixelsDeterministic").GetBoolean()
                    || Ready.GetProperty("outputBytes").GetInt32() != HorizontalFlipInput.Width * HorizontalFlipInput.Height * 4))
                || (operation == "png-encode" && (Ready.GetProperty("compressionLevel").GetInt32() != 6 || Ready.GetProperty("filter").GetInt32() != 0)))
                throw new InvalidOperationException("Native warm worker geometry/thread/CPU contract differs.");
            completed = codec || nearest || bilinear ? Ready.GetProperty("initialCompletedOperations").GetInt64() : 1;
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
            || (Ready.GetProperty("operation").GetString() is "png-encode" or "jpeg-encode" or "webp-encode" ? sample.OutputStride != 0 || sample.OutputBytes != Ready.GetProperty("outputBytes").GetInt32()
                : sample.OutputStride < (Ready.GetProperty("operation").GetString() is "nearest-resize" or "bilinear-resize" ? 256 : HorizontalFlipInput.Width) * (Ready.GetProperty("depth").GetInt32() / 8) * 4)
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
        var verified = Ready.GetProperty("operation").GetString() == "bilinear-resize"
            ? value.GetProperty("matched").GetBoolean() && value.GetProperty("alphaExact").GetBoolean()
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
