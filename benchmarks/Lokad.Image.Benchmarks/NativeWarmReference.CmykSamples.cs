using System.Diagnostics;
using System.Text.Json;
using Lokad.Image;

internal sealed partial class NativeWarmReference
{
    public NativeWarmReference(JpegBenchmarkCase state, int cpu, JpegSampleFormat format, int expectedBytes)
    {
        if (format != JpegSampleFormat.Cmyk || expectedBytes != 513 * 259 * 4) throw new ArgumentOutOfRangeException(nameof(format));
        Executable = Path.Combine(state.Root, "external/warm-reference/ffmpeg-jpeg-cmyk-samples-warm");
        using (var document = JsonDocument.Parse(File.ReadAllText(Path.Combine(state.Root, "external/warm-reference/jpeg-cmyk-samples-identity.json")))) BuildIdentity = document.RootElement.Clone();
        if (BuildIdentity.GetProperty("workerSha256").GetString() != JpegBenchmarkCase.HashFile(Executable)
            || BuildIdentity.GetProperty("sourceSha256").GetString() != JpegBenchmarkCase.HashFile(Path.Combine(state.Root, "tools/WarmReference/ffmpeg-jpeg-cmyk-samples-warm.c"))
            || BuildIdentity.GetProperty("buildScriptSha256").GetString() != JpegBenchmarkCase.HashFile(Path.Combine(state.Root, "tools/WarmReference/build-jpeg-cmyk-samples-linux.sh")))
            throw new InvalidOperationException("Raw CMYK worker/source/build identity differs.");
        var start = new ProcessStartInfo(Executable) { RedirectStandardInput = true, RedirectStandardOutput = true, RedirectStandardError = true, UseShellExecute = false };
        foreach (var argument in new[] { "jpeg-cmyk-samples", state.Input, state.NativeOutput }) start.ArgumentList.Add(argument);
        process = Process.Start(start) ?? throw new InvalidOperationException("Could not start caller CMYK reference."); errors = process.StandardError.ReadToEndAsync();
        try
        {
            Ready = Read();
            if (Ready.GetProperty("schemaVersion").GetInt32() != 1 || Ready.GetProperty("state").GetString() != "ready" || Ready.GetProperty("operation").GetString() != "jpeg-cmyk-samples"
                || Ready.GetProperty("width").GetInt32() != 513 || Ready.GetProperty("height").GetInt32() != 259 || Ready.GetProperty("depth").GetInt32() != 8
                || Ready.GetProperty("decodedPixelFormat").GetString() != "yuva444p" || Ready.GetProperty("decoder").GetString() != "mjpeg" || Ready.GetProperty("adobeTransform").GetInt32() != -1
                || Ready.GetProperty("rgbaIntermediate").GetBoolean() || Ready.GetProperty("colorTransform").GetBoolean()
                || Ready.GetProperty("outputStride").GetInt32() != 2052 || Ready.GetProperty("outputBytes").GetInt32() != expectedBytes
                || Ready.GetProperty("cpu").GetInt32() != cpu || Ready.GetProperty("affinityCount").GetInt32() != 1 || Ready.GetProperty("codecThreads").GetInt32() != 1
                || Ready.GetProperty("threadType").GetInt32() != 0 || Ready.GetProperty("activeThreadType").GetInt32() != 0 || Ready.GetProperty("conversionThreads").GetInt32() != 1
                || new[] { "rawCmykComponents", "blackPreserved", "freshZeroedCallerOutputTimed", "packingTimed", "stateResetTimed", "decoderPlanesReleaseTimed", "outputAcquisitionAndReleaseTimed", "independentLiveOutputs", "pixelsDeterministic" }.Any(key => !Ready.GetProperty(key).GetBoolean()))
                throw new InvalidOperationException("Caller CMYK raw-plane/CPU/ownership boundary differs.");
            completed = Ready.GetProperty("initialCompletedOperations").GetInt64();
        }
        catch { Dispose(); throw; }
    }
}
