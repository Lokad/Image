using System.Diagnostics;
using System.Text.Json;

internal sealed partial class NativeWarmReference
{
    private readonly bool requireFillReset;

    public NativeWarmReference(JpegBenchmarkCase state, int cpu, WarmFillRectangleCase fillCase)
    {
        requireFillReset = true;
        var depth = fillCase switch
        {
            WarmFillRectangleCase.DrawingFill8 or WarmFillRectangleCase.DrawingFill8OriginalClipped => 8,
            WarmFillRectangleCase.DrawingFill16 or WarmFillRectangleCase.DrawingFill16OriginalClipped => 16,
            _ => throw new ArgumentOutOfRangeException(nameof(fillCase))
        };
        var original = fillCase is WarmFillRectangleCase.DrawingFill8OriginalClipped or WarmFillRectangleCase.DrawingFill16OriginalClipped;
        var x = original ? -3 : -17; var y = original ? 5 : 19; var width = original ? 257 : 273;
        Executable = Path.Combine(state.Root, "external/warm-reference/ffmpeg-fill-rectangle-warm");
        using (var document = JsonDocument.Parse(File.ReadAllBytes(Path.Combine(state.Root, "external/warm-reference/fill-rectangle-identity.json"))))
            BuildIdentity = document.RootElement.Clone();
        if (BuildIdentity.GetProperty("workerSha256").GetString() != JpegBenchmarkCase.HashFile(Executable)
            || BuildIdentity.GetProperty("sourceSha256").GetString() != JpegBenchmarkCase.HashFile(Path.Combine(state.Root, "tools/WarmReference/ffmpeg-fill-rectangle-warm.c"))
            || BuildIdentity.GetProperty("buildScriptSha256").GetString() != JpegBenchmarkCase.HashFile(Path.Combine(state.Root, "tools/WarmReference/build-fill-rectangle-linux.sh")))
            throw new InvalidOperationException("Rectangle-fill reference identity differs.");
        var start = new ProcessStartInfo(Executable) { RedirectStandardInput = true, RedirectStandardOutput = true, RedirectStandardError = true, UseShellExecute = false };
        foreach (var argument in new[] { "fill-rectangle", state.Input, "513", "259", depth.ToString(System.Globalization.CultureInfo.InvariantCulture), state.NativeOutput })
            start.ArgumentList.Add(argument);
        if (original) start.ArgumentList.Add("original-clipped");
        process = Process.Start(start) ?? throw new InvalidOperationException("Could not start rectangle-fill reference.");
        errors = process.StandardError.ReadToEndAsync();
        try
        {
            Ready = Read();
            if (Ready.GetProperty("schemaVersion").GetInt32() != 1 || Ready.GetProperty("state").GetString() != "ready"
                || Ready.GetProperty("operation").GetString() != "fill-rectangle" || Ready.GetProperty("width").GetInt32() != 513
                || Ready.GetProperty("height").GetInt32() != 259 || Ready.GetProperty("depth").GetInt32() != depth
                || Ready.GetProperty("cpu").GetInt32() != cpu || Ready.GetProperty("affinityCount").GetInt32() != 1
                || Ready.GetProperty("workerThreads").GetInt32() != 1 || Ready.GetProperty("x").GetInt32() != x
                || Ready.GetProperty("y").GetInt32() != y || Ready.GetProperty("fillWidth").GetInt32() != width
                || Ready.GetProperty("fillHeight").GetInt32() != 129 || Ready.GetProperty("clippedX").GetInt32() != 0
                || Ready.GetProperty("clippedY").GetInt32() != y || Ready.GetProperty("clippedWidth").GetInt32() != width + x
                || Ready.GetProperty("clippedHeight").GetInt32() != 129
                || new[] { "borrowedCanvas", "canvasMutationTimed", "colorNormalizationTimed", "clippingTimed", "resetBeforeEveryOperation" }.Any(key => !Ready.GetProperty(key).GetBoolean())
                || new[] { "sourcePreparationTimed", "canvasPreparationTimed", "outputAllocationTimed" }.Any(key => Ready.GetProperty(key).GetBoolean()))
                throw new InvalidOperationException("Rectangle-fill geometry/CPU/borrowed boundary differs.");
            completed = Ready.GetProperty("initialCompletedOperations").GetInt64();
        }
        catch { Dispose(); throw; }
    }
}
