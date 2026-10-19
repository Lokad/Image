using System.Diagnostics;
using System.Text.Json;

internal sealed partial class NativeWarmReference
{
    public NativeWarmReference(JpegBenchmarkCase state, int cpu, bool asyncPngEncode)
    {
        if (!asyncPngEncode) throw new ArgumentOutOfRangeException(nameof(asyncPngEncode));
        Executable = Path.Combine(state.Root, "external/warm-reference/ffmpeg-png-nonseekable-encode-warm");
        using (var document = JsonDocument.Parse(File.ReadAllBytes(Path.Combine(state.Root, "external/warm-reference/png-nonseekable-encode-identity.json"))))
            BuildIdentity = document.RootElement.Clone();
        if (BuildIdentity.GetProperty("workerSha256").GetString() != JpegBenchmarkCase.HashFile(Executable)
            || BuildIdentity.GetProperty("sourceSha256").GetString() != JpegBenchmarkCase.HashFile(Path.Combine(state.Root, "tools/WarmReference/ffmpeg-png-nonseekable-encode-warm.c"))
            || BuildIdentity.GetProperty("buildScriptSha256").GetString() != JpegBenchmarkCase.HashFile(Path.Combine(state.Root, "tools/WarmReference/build-png-nonseekable-encode-linux.sh")))
            throw new InvalidOperationException("Nonseekable PNG encode reference identity differs.");
        var start = new ProcessStartInfo(Executable) { RedirectStandardInput = true, RedirectStandardOutput = true, RedirectStandardError = true, UseShellExecute = false };
        foreach (var argument in new[] { "png-nonseekable-encode", state.Input, "513", "259", "8", state.NativeOutput }) start.ArgumentList.Add(argument);
        process = Process.Start(start) ?? throw new InvalidOperationException("Could not start nonseekable PNG encode reference.");
        errors = process.StandardError.ReadToEndAsync();
        try
        {
            Ready = Read();
            if (Ready.GetProperty("state").GetString() != "ready" || Ready.GetProperty("operation").GetString() != "png-nonseekable-encode"
                || Ready.GetProperty("schemaVersion").GetInt32() != 1 || Ready.GetProperty("width").GetInt32() != 513
                || Ready.GetProperty("height").GetInt32() != 259 || Ready.GetProperty("depth").GetInt32() != 8
                || Ready.GetProperty("cpu").GetInt32() != cpu || Ready.GetProperty("affinityCount").GetInt32() != 1
                || Ready.GetProperty("codecThreads").GetInt32() != 1 || Ready.GetProperty("conversionThreads").GetInt32() != 0
                || Ready.GetProperty("threadType").GetInt32() != 0 || Ready.GetProperty("activeThreadType").GetInt32() != 0
                || Ready.GetProperty("compressionLevel").GetInt32() != 6 || !Ready.GetProperty("filterNone").GetBoolean()
                || Ready.GetProperty("scalarMode").GetBoolean() || Ready.GetProperty("seekCallback").GetBoolean()
                || Ready.GetProperty("lengthQueries").GetInt32() != 0 || Ready.GetProperty("outputView").GetBoolean()
                || Ready.GetProperty("outputBytes").GetInt32() <= 0 || Ready.GetProperty("idatBytes").GetInt32() <= 0
                || Ready.GetProperty("writes").GetInt32() <= 0 || Ready.GetProperty("growths").GetInt32() <= 0
                || new[] { "freshNonSeekableOutput", "outputGrowthTimed", "outputCopyTimed", "encodedStagingTimed", "internalAvioFlushTimed",
                    "streamAcquisitionAndReleaseTimed", "residentCallbacksImmediate", "callerOutputLeftOpen", "independentLiveOutputs", "independentOutputFromStaging" }
                    .Any(key => !Ready.GetProperty(key).GetBoolean()))
                throw new InvalidOperationException("Nonseekable PNG encode output/ownership/settings/CPU boundary differs.");
            completed = Ready.GetProperty("initialCompletedOperations").GetInt64();
        }
        catch { Dispose(); throw; }
    }
}
