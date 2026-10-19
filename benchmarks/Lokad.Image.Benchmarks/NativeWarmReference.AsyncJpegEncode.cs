using System.Diagnostics;
using System.Text.Json;

internal sealed partial class NativeWarmReference
{
    public NativeWarmReference(JpegBenchmarkCase state, int cpu, string normalized, bool asyncJpegEncode)
    {
        if (!asyncJpegEncode) throw new ArgumentOutOfRangeException(nameof(asyncJpegEncode));
        Executable = Path.Combine(state.Root, "external/warm-reference/ffmpeg-jpeg-nonseekable-encode-warm");
        using (var document = JsonDocument.Parse(File.ReadAllBytes(Path.Combine(state.Root, "external/warm-reference/jpeg-nonseekable-encode-identity.json"))))
            BuildIdentity = document.RootElement.Clone();
        if (BuildIdentity.GetProperty("workerSha256").GetString() != JpegBenchmarkCase.HashFile(Executable)
            || BuildIdentity.GetProperty("sourceSha256").GetString() != JpegBenchmarkCase.HashFile(Path.Combine(state.Root, "tools/WarmReference/ffmpeg-jpeg-nonseekable-encode-warm.c"))
            || BuildIdentity.GetProperty("buildScriptSha256").GetString() != JpegBenchmarkCase.HashFile(Path.Combine(state.Root, "tools/WarmReference/build-jpeg-nonseekable-encode-linux.sh")))
            throw new InvalidOperationException("Nonseekable JPEG encode reference identity differs.");
        var start = new ProcessStartInfo(Executable) { RedirectStandardInput = true, RedirectStandardOutput = true, RedirectStandardError = true, UseShellExecute = false };
        foreach (var argument in new[] { "jpeg-nonseekable-encode", state.Input, "513", "259", "8", state.NativeOutput, "5", normalized }) start.ArgumentList.Add(argument);
        process = Process.Start(start) ?? throw new InvalidOperationException("Could not start nonseekable JPEG encode reference.");
        errors = process.StandardError.ReadToEndAsync();
        try
        {
            Ready = Read();
            if (Ready.GetProperty("state").GetString() != "ready" || Ready.GetProperty("operation").GetString() != "jpeg-nonseekable-encode"
                || Ready.GetProperty("schemaVersion").GetInt32() != 1 || Ready.GetProperty("width").GetInt32() != 513
                || Ready.GetProperty("height").GetInt32() != 259 || Ready.GetProperty("depth").GetInt32() != 8
                || Ready.GetProperty("cpu").GetInt32() != cpu || Ready.GetProperty("affinityCount").GetInt32() != 1
                || Ready.GetProperty("codecThreads").GetInt32() != 1 || Ready.GetProperty("conversionThreads").GetInt32() != 1
                || Ready.GetProperty("threadType").GetInt32() != 0 || Ready.GetProperty("activeThreadType").GetInt32() != 0
                || Ready.GetProperty("nativeQuality").GetInt32() != 5 || Ready.GetProperty("sampling").GetString() != "444" || Ready.GetProperty("backgroundRgb").GetInt32() != 0x204060
                || Ready.GetProperty("seekCallback").GetBoolean()
                || Ready.GetProperty("lengthQueries").GetInt32() != 0 || Ready.GetProperty("outputView").GetBoolean()
                || Ready.GetProperty("outputBytes").GetInt32() <= 0
                || Ready.GetProperty("writes").GetInt32() <= 0 || Ready.GetProperty("growths").GetInt32() <= 0
                || new[] { "normalizationTimed", "colorConversionTimed", "freshNonSeekableOutput", "outputGrowthTimed", "outputCopyTimed", "encodedStagingTimed", "internalAvioFlushTimed",
                    "streamAcquisitionAndReleaseTimed", "residentCallbacksImmediate", "callerOutputLeftOpen", "independentLiveOutputs", "independentOutputFromStaging" }
                    .Any(key => !Ready.GetProperty(key).GetBoolean()))
                throw new InvalidOperationException("Nonseekable JPEG encode output/ownership/settings/CPU boundary differs.");
            completed = Ready.GetProperty("initialCompletedOperations").GetInt64();
        }
        catch { Dispose(); throw; }
    }
}
