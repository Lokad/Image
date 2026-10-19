using System.Diagnostics;
using System.Text.Json;
using Lokad.Image;

internal sealed partial class NativeWarmReference
{
    public NativeWarmReference(JpegBenchmarkCase state, int cpu, JpegSampleFormat format)
    {
        if (format is not (JpegSampleFormat.Grayscale or JpegSampleFormat.Rgb)) throw new ArgumentOutOfRangeException(nameof(format));
        var gray = format == JpegSampleFormat.Grayscale; var kind = gray ? "gray" : "rgb"; var channels = (int)format;
        Executable = Path.Combine(state.Root, $"external/warm-reference/ffmpeg-jpeg-{kind}-samples-warm");
        using (var document = JsonDocument.Parse(File.ReadAllText(Path.Combine(state.Root, $"external/warm-reference/jpeg-{kind}-samples-identity.json"))))
            BuildIdentity = document.RootElement.Clone();
        if (BuildIdentity.GetProperty("workerSha256").GetString() != JpegBenchmarkCase.HashFile(Executable)
            || BuildIdentity.GetProperty("sourceSha256").GetString() != JpegBenchmarkCase.HashFile(Path.Combine(state.Root, $"tools/WarmReference/ffmpeg-jpeg-{kind}-samples-warm.c"))
            || BuildIdentity.GetProperty("buildScriptSha256").GetString() != JpegBenchmarkCase.HashFile(Path.Combine(state.Root, $"tools/WarmReference/build-jpeg-{kind}-samples-linux.sh")))
            throw new InvalidOperationException("Caller gray decode worker/source/build identity differs.");
        var start = new ProcessStartInfo(Executable) { RedirectStandardInput = true, RedirectStandardOutput = true, RedirectStandardError = true, UseShellExecute = false };
        foreach (var argument in new[] { $"jpeg-{kind}-samples-decode", state.Input, state.NativeOutput }) start.ArgumentList.Add(argument);
        process = Process.Start(start) ?? throw new InvalidOperationException("Could not start caller gray decode reference.");
        errors = process.StandardError.ReadToEndAsync();
        try
        {
            Ready = Read();
            if (Ready.GetProperty("schemaVersion").GetInt32() != 1 || Ready.GetProperty("state").GetString() != "ready"
                || Ready.GetProperty("operation").GetString() != $"jpeg-{kind}-samples-decode"
                || Ready.GetProperty("width").GetInt32() != 513 || Ready.GetProperty("height").GetInt32() != 259
                || Ready.GetProperty("depth").GetInt32() != 8 || Ready.GetProperty("decodedPixelFormat").GetString() != (gray ? "gray8" : "yuvj420p")
                || Ready.GetProperty("decoder").GetString() != "mjpeg" || Ready.GetProperty("rgbaIntermediate").GetBoolean()
                || Ready.GetProperty("outputStride").GetInt32() != 513 * channels || Ready.GetProperty("outputBytes").GetInt32() != 513 * 259 * channels
                || (!gray && (!Ready.GetProperty("chromaExpansionTimed").GetBoolean() || !Ready.GetProperty("colorConversionTimed").GetBoolean()))
                || Ready.GetProperty("cpu").GetInt32() != cpu || Ready.GetProperty("affinityCount").GetInt32() != 1
                || Ready.GetProperty("codecThreads").GetInt32() != 1 || Ready.GetProperty("threadType").GetInt32() != 0
                || Ready.GetProperty("activeThreadType").GetInt32() != 0
                || new[] { "freshZeroedCallerOutputTimed", "packingTimed", "stateResetTimed", "decoderPlanesReleaseTimed", "outputAcquisitionAndReleaseTimed", "independentLiveOutputs", "pixelsDeterministic" }
                    .Any(key => !Ready.GetProperty(key).GetBoolean()))
                throw new InvalidOperationException("Caller gray reference CPU/output/ownership boundary differs.");
            completed = Ready.GetProperty("initialCompletedOperations").GetInt64();
        }
        catch { Dispose(); throw; }
    }
}
