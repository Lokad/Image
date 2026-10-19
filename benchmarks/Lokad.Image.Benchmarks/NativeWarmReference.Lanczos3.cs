using System.Diagnostics;
using System.Text.Json;
using Lokad.Image;

internal sealed partial class NativeWarmReference
{
    public NativeWarmReference(JpegBenchmarkCase state, int depth, int cpu, ResizeFilter filter, int width, int height, string expected)
    {
        if (filter != ResizeFilter.Lanczos3 || depth is not (8 or 16) || (width, height) is not ((256, 129) or (1027, 519)))
            throw new ArgumentException("Choose one of the four declared Lanczos3 cases.");
        Executable = Path.Combine(state.Root, "external/warm-reference/zimg-lanczos3-warm");
        using (var document = JsonDocument.Parse(File.ReadAllText(Path.Combine(state.Root, "external/warm-reference/lanczos3-identity.json"))))
            BuildIdentity = document.RootElement.Clone();
        if (BuildIdentity.GetProperty("workerSha256").GetString() != JpegBenchmarkCase.HashFile(Executable)
            || BuildIdentity.GetProperty("sourceSha256").GetString() != JpegBenchmarkCase.HashFile(Path.Combine(state.Root, "tools/WarmReference/zimg-lanczos3-warm.c"))
            || BuildIdentity.GetProperty("buildScriptSha256").GetString() != JpegBenchmarkCase.HashFile(Path.Combine(state.Root, "tools/WarmReference/build-lanczos3-linux.sh")))
            throw new InvalidOperationException("Lanczos3 native worker/source/build identity differs.");
        var start = new ProcessStartInfo(Executable) { RedirectStandardInput = true, RedirectStandardOutput = true, RedirectStandardError = true, UseShellExecute = false };
        foreach (var argument in new[] { "lanczos3-resize", state.Input, "513", "259", depth.ToString(), state.NativeOutput, expected, width.ToString(), height.ToString() })
            start.ArgumentList.Add(argument);
        process = Process.Start(start) ?? throw new InvalidOperationException("Could not start Lanczos3 reference.");
        errors = process.StandardError.ReadToEndAsync();
        try
        {
            Ready = Read();
            if (Ready.GetProperty("schemaVersion").GetInt32() != 1 || Ready.GetProperty("state").GetString() != "ready"
                || Ready.GetProperty("operation").GetString() != "lanczos3-resize" || Ready.GetProperty("radius").GetInt32() != 3
                || Ready.GetProperty("width").GetInt32() != 513 || Ready.GetProperty("height").GetInt32() != 259 || Ready.GetProperty("depth").GetInt32() != depth
                || Ready.GetProperty("outputWidth").GetInt32() != width || Ready.GetProperty("outputHeight").GetInt32() != height
                || Ready.GetProperty("outputBytes").GetInt32() != width * height * 4 * (depth / 8)
                || Ready.GetProperty("cpu").GetInt32() != cpu || Ready.GetProperty("affinityCount").GetInt32() != 1
                || Ready.GetProperty("filterThreads").GetInt32() != 1 || Ready.GetProperty("threadType").GetInt32() != 0
                || Ready.GetProperty("scalar").GetBoolean() || Ready.GetProperty("haloPixels").GetInt32() != 8
                || new[] { "haloAndPackingTimed", "outputAllocationAndRepackingTimed", "premultipliedFloatOutput", "unpremultiplicationAndQuantizationTimed", "independentLiveOutputs", "alphaTolerance1" }
                    .Any(key => !Ready.GetProperty(key).GetBoolean()))
                throw new InvalidOperationException("Lanczos3 native kernel/ownership/CPU boundary differs.");
            completed = Ready.GetProperty("initialCompletedOperations").GetInt64();
        }
        catch { Dispose(); throw; }
    }
}
