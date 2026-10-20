using System.Diagnostics;
using System.Text.Json;

internal enum PngRetainedReference { TwoOwnedOutputs }

internal sealed partial class NativeWarmReference
{
    public NativeWarmReference(JpegBenchmarkCase state, int cpu, string expectedRaw, PngRetainedReference retainedPng)
    {
        if (retainedPng != PngRetainedReference.TwoOwnedOutputs) throw new ArgumentOutOfRangeException(nameof(retainedPng));
        Executable = Path.Combine(state.Root, "external/warm-reference/ffmpeg-png-retained2-warm");
        using (var document = JsonDocument.Parse(File.ReadAllBytes(Path.Combine(state.Root, "external/warm-reference/png-retained2-identity.json"))))
            BuildIdentity = document.RootElement.Clone();
        foreach (var (key, path) in new[] {
            ("workerSha256", Executable),
            ("sourceSha256", Path.Combine(state.Root,"tools/WarmReference/ffmpeg-png-retained2-warm.c")),
            ("includedSourceSha256", Path.Combine(state.Root,"tools/WarmReference/ffmpeg-png-warm.c")),
            ("buildScriptSha256", Path.Combine(state.Root,"tools/WarmReference/build-png-retained2-linux.sh")) })
            if (BuildIdentity.GetProperty(key).GetString() != JpegBenchmarkCase.HashFile(path))
                throw new InvalidOperationException("Retained PNG reference identity differs.");
        var start = new ProcessStartInfo(Executable) { RedirectStandardInput = true, RedirectStandardOutput = true, RedirectStandardError = true, UseShellExecute = false };
        foreach (var value in new[] {state.Input, expectedRaw, state.NativeOutput}) start.ArgumentList.Add(value);
        process = Process.Start(start) ?? throw new InvalidOperationException("Cannot start retained PNG reference.");
        errors = process.StandardError.ReadToEndAsync();
        try
        {
            Ready = Read();
            if (Ready.GetProperty("state").GetString() != "ready" || Ready.GetProperty("operation").GetString() != "png-retained2-decode"
                || Ready.GetProperty("cpu").GetInt32() != cpu || Ready.GetProperty("affinityCount").GetInt32() != 1
                || Ready.GetProperty("codecThreads").GetInt32() != 1 || Ready.GetProperty("activeThreadType").GetInt32() != 0
                || Ready.GetProperty("width").GetInt32() != 513 || Ready.GetProperty("height").GetInt32() != 259
                || Ready.GetProperty("depth").GetInt32() != 8 || Ready.GetProperty("outputBytes").GetInt32() != 1062936
                || Ready.GetProperty("scalarMode").GetBoolean()
                || new[] {"twoCompleteDecodesTimed","firstRetainedThroughSecond","independentLiveOutputs","survivorAfterMutationAndRelease","outputAcquisitionAndReleaseTimed","residentInputPreparedOutsideTiming"}.Any(k => !Ready.GetProperty(k).GetBoolean()))
                throw new InvalidOperationException("Retained PNG reference boundary differs.");
            completed = Ready.GetProperty("initialCompletedOperations").GetInt64();
        }
        catch { Dispose(); throw; }
    }
}
