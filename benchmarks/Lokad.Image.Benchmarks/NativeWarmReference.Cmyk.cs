using System.Diagnostics;
using System.Text.Json;

internal sealed partial class NativeWarmReference
{
    public NativeWarmReference(JpegBenchmarkCase state, int cpu, string operation, int expectedBytes)
    {
        if (operation is not ("jpeg-cmyk-decode" or "jpeg-ycck-decode") || expectedBytes != 513 * 259 * 4) throw new ArgumentOutOfRangeException(nameof(operation));
        var ycck = operation == "jpeg-ycck-decode"; var kind = ycck ? "ycck" : "cmyk";
        Executable = Path.Combine(state.Root, $"external/warm-reference/ffmpeg-jpeg-{kind}-warm");
        using (var document = JsonDocument.Parse(File.ReadAllText(Path.Combine(state.Root, $"external/warm-reference/jpeg-{kind}-identity.json")))) BuildIdentity = document.RootElement.Clone();
        if (BuildIdentity.GetProperty("workerSha256").GetString() != JpegBenchmarkCase.HashFile(Executable)
            || BuildIdentity.GetProperty("sourceSha256").GetString() != JpegBenchmarkCase.HashFile(Path.Combine(state.Root, $"tools/WarmReference/ffmpeg-jpeg-{kind}-warm.c"))
            || BuildIdentity.GetProperty("buildScriptSha256").GetString() != JpegBenchmarkCase.HashFile(Path.Combine(state.Root, $"tools/WarmReference/build-jpeg-{kind}-linux.sh")))
            throw new InvalidOperationException("CMYK worker/source/build identity differs.");
        var start = new ProcessStartInfo(Executable) { RedirectStandardInput = true, RedirectStandardOutput = true, RedirectStandardError = true, UseShellExecute = false };
        foreach (var argument in new[] { operation, state.Input, state.NativeOutput }) start.ArgumentList.Add(argument);
        process = Process.Start(start) ?? throw new InvalidOperationException("Could not start CMYK reference."); errors = process.StandardError.ReadToEndAsync();
        try
        {
            Ready = Read();
            if (Ready.GetProperty("schemaVersion").GetInt32() != 1 || Ready.GetProperty("state").GetString() != "ready" || Ready.GetProperty("operation").GetString() != operation
                || Ready.GetProperty("width").GetInt32() != 513 || Ready.GetProperty("height").GetInt32() != 259 || Ready.GetProperty("depth").GetInt32() != 8
                || Ready.GetProperty("decodedPixelFormat").GetString() != (ycck ? "yuva444p" : "gbrap") || Ready.GetProperty("decoder").GetString() != "mjpeg" || Ready.GetProperty("adobeTransform").GetInt32() != (ycck ? 2 : 0)
                || (ycck && !Ready.GetProperty("explicitBt601FullRange").GetBoolean())
                || Ready.GetProperty("outputStride").GetInt32() != 513 * 4 || Ready.GetProperty("outputBytes").GetInt32() != expectedBytes
                || Ready.GetProperty("cpu").GetInt32() != cpu || Ready.GetProperty("affinityCount").GetInt32() != 1 || Ready.GetProperty("codecThreads").GetInt32() != 1
                || Ready.GetProperty("threadType").GetInt32() != 0 || Ready.GetProperty("activeThreadType").GetInt32() != 0 || Ready.GetProperty("conversionThreads").GetInt32() != 1
                || new[] { "blackAppliedByDecoderTimed", "opaqueAlpha", "packingTimed", "stateResetTimed", "decoderPlanesReleaseTimed", "outputAcquisitionAndReleaseTimed", "independentLiveOutputs", "pixelsDeterministic" }.Any(key => !Ready.GetProperty(key).GetBoolean()))
                throw new InvalidOperationException("CMYK reference color/CPU/ownership boundary differs.");
            completed = Ready.GetProperty("initialCompletedOperations").GetInt64();
        }
        catch { Dispose(); throw; }
    }
}
