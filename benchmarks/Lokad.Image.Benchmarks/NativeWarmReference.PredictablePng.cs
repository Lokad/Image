using System.Diagnostics;
using System.Text.Json;

internal sealed partial class NativeWarmReference
{
    public NativeWarmReference(JpegBenchmarkCase state, int cpu, int predictablePngSize)
    {
        if (predictablePngSize != 3000) throw new ArgumentOutOfRangeException(nameof(predictablePngSize));
        Executable = Path.Combine(state.Root, "external/warm-reference/ffmpeg-png-predictable-warm");
        using (var document = JsonDocument.Parse(File.ReadAllBytes(Path.Combine(state.Root, "external/warm-reference/png-predictable-identity.json"))))
            BuildIdentity = document.RootElement.Clone();
        if (BuildIdentity.GetProperty("workerSha256").GetString() != JpegBenchmarkCase.HashFile(Executable)
            || BuildIdentity.GetProperty("generatedSourceSha256").GetString() != JpegBenchmarkCase.HashFile(Path.Combine(state.Root, "external/warm-reference/ffmpeg-png-predictable-generated.c"))
            || BuildIdentity.GetProperty("sourceSha256").GetString() != JpegBenchmarkCase.HashFile(Path.Combine(state.Root, "tools/WarmReference/ffmpeg-png-warm.c"))
            || BuildIdentity.GetProperty("buildScriptSha256").GetString() != JpegBenchmarkCase.HashFile(Path.Combine(state.Root, "tools/WarmReference/build-png-predictable-linux.sh")))
            throw new InvalidOperationException("Predictable PNG reference identity differs.");
        var start = new ProcessStartInfo(Executable) { RedirectStandardInput = true, RedirectStandardOutput = true, RedirectStandardError = true, UseShellExecute = false };
        foreach (var argument in new[] { "png-encode", state.Input, "3000", "3000", "16", state.NativeOutput }) start.ArgumentList.Add(argument);
        process = Process.Start(start) ?? throw new InvalidOperationException("Could not start predictable PNG reference.");
        errors = process.StandardError.ReadToEndAsync();
        try
        {
            Ready = Read();
            if (Ready.GetProperty("state").GetString() != "ready" || Ready.GetProperty("operation").GetString() != "png-encode"
                || Ready.GetProperty("schemaVersion").GetInt32() != 1 || Ready.GetProperty("width").GetInt32() != predictablePngSize
                || Ready.GetProperty("height").GetInt32() != predictablePngSize || Ready.GetProperty("depth").GetInt32() != 16
                || Ready.GetProperty("cpu").GetInt32() != cpu || Ready.GetProperty("affinityCount").GetInt32() != 1
                || Ready.GetProperty("codecThreads").GetInt32() != 1 || Ready.GetProperty("conversionThreads").GetInt32() != 0
                || Ready.GetProperty("threadType").GetInt32() != 0 || Ready.GetProperty("activeThreadType").GetInt32() != 0
                || Ready.GetProperty("compressionLevel").GetInt32() != 6 || Ready.GetProperty("filter").GetInt32() != 5
                || Ready.GetProperty("scalarMode").GetBoolean() || !Ready.GetProperty("independentLiveOutputs").GetBoolean())
                throw new InvalidOperationException("Predictable PNG settings/CPU/ownership boundary differs.");
            completed = Ready.GetProperty("initialCompletedOperations").GetInt64();
        }
        catch { Dispose(); throw; }
    }
}
