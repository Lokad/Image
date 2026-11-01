using System.Diagnostics;
using System.Text.Json;

internal sealed partial class NativeWarmReference
{
    public NativeWarmReference(JpegBenchmarkCase state, int cpu, WarmCopyOverlapCase overlapCase)
    {
        if (overlapCase != WarmCopyOverlapCase.DrawingCopyOverlap8) throw new ArgumentOutOfRangeException(nameof(overlapCase));
        const int depth = 8;
        Executable = Path.Combine(state.Root, "external/warm-reference/ffmpeg-copy-region-overlap-warm");
        using (var document = JsonDocument.Parse(File.ReadAllBytes(Path.Combine(state.Root, "external/warm-reference/copy-region-overlap-identity.json"))))
            BuildIdentity = document.RootElement.Clone();
        if (BuildIdentity.GetProperty("workerSha256").GetString() != JpegBenchmarkCase.HashFile(Executable)
            || BuildIdentity.GetProperty("sourceSha256").GetString() != JpegBenchmarkCase.HashFile(Path.Combine(state.Root, "tools/WarmReference/ffmpeg-copy-region-overlap-warm.c"))
            || BuildIdentity.GetProperty("buildScriptSha256").GetString() != JpegBenchmarkCase.HashFile(Path.Combine(state.Root, "tools/WarmReference/build-copy-region-overlap-linux.sh")))
            throw new InvalidOperationException("Rectangle-copy reference identity differs.");
        var start = new ProcessStartInfo(Executable) { RedirectStandardInput = true, RedirectStandardOutput = true, RedirectStandardError = true, UseShellExecute = false };
        foreach (var argument in new[] { "copy-region-overlap", state.Input, "513", "259", depth.ToString(System.Globalization.CultureInfo.InvariantCulture), state.NativeOutput })
            start.ArgumentList.Add(argument);
        process = Process.Start(start) ?? throw new InvalidOperationException("Could not start rectangle-copy reference.");
        errors = process.StandardError.ReadToEndAsync();
        try
        {
            Ready = Read();
            if (Ready.GetProperty("schemaVersion").GetInt32() != 1 || Ready.GetProperty("state").GetString() != "ready"
                || Ready.GetProperty("operation").GetString() != "copy-region-overlap" || Ready.GetProperty("width").GetInt32() != 513
                || Ready.GetProperty("height").GetInt32() != 259 || Ready.GetProperty("depth").GetInt32() != depth
                || Ready.GetProperty("cpu").GetInt32() != cpu || Ready.GetProperty("affinityCount").GetInt32() != 1
                || Ready.GetProperty("workerThreads").GetInt32() != 1 || Ready.GetProperty("publicMovesPerPair").GetInt32() != 2
                || Ready.GetProperty("nativeCallsPerPair").GetInt32() != 6 || Ready.GetProperty("bandHeight").GetInt32() != 46
                || Ready.GetProperty("temporaryPixelBytes").GetInt32() != 0
                || new[] { "sourceAliasesCanvas", "borrowedCanvas", "canvasMutationTimed", "restorationTimed", "disjointNativeRanges" }.Any(key => !Ready.GetProperty(key).GetBoolean())
                || new[] { "sourcePreparationTimed", "canvasPreparationTimed", "outputAllocationTimed" }.Any(key => Ready.GetProperty(key).GetBoolean()))
                throw new InvalidOperationException("Rectangle-copy geometry/CPU/borrowed boundary differs.");
            completed = Ready.GetProperty("initialCompletedOperations").GetInt64();
        }
        catch { Dispose(); throw; }
    }
}
