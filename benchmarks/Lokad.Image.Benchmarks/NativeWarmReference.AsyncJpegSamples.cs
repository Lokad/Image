using System.Diagnostics;
using System.Text.Json;
using Lokad.Image;

internal sealed partial class NativeWarmReference
{
    public NativeWarmReference(JpegBenchmarkCase state, int cpu, JpegSampleFormat format, bool asyncSampleEncode)
    {
        if (!asyncSampleEncode) throw new ArgumentOutOfRangeException(nameof(asyncSampleEncode));
        if (format != JpegSampleFormat.Rgb) throw new ArgumentOutOfRangeException(nameof(format));
        Executable = Path.Combine(state.Root, "external/warm-reference/ffmpeg-jpeg-samples-nonseekable-encode-warm");
        using (var document = JsonDocument.Parse(File.ReadAllBytes(Path.Combine(state.Root, "external/warm-reference/jpeg-samples-nonseekable-encode-identity.json"))))
            BuildIdentity = document.RootElement.Clone();
        if (BuildIdentity.GetProperty("workerSha256").GetString() != JpegBenchmarkCase.HashFile(Executable)
            || BuildIdentity.GetProperty("sourceSha256").GetString() != JpegBenchmarkCase.HashFile(Path.Combine(state.Root, "tools/WarmReference/ffmpeg-jpeg-samples-nonseekable-encode-warm.c"))
            || BuildIdentity.GetProperty("buildScriptSha256").GetString() != JpegBenchmarkCase.HashFile(Path.Combine(state.Root, "tools/WarmReference/build-jpeg-samples-nonseekable-encode-linux.sh")))
            throw new InvalidOperationException("Nonseekable JPEG encode reference identity differs.");
        using var anchor = JsonDocument.Parse(File.ReadAllBytes(Path.Combine(state.Root, "artifacts/warm-reference/jpeg420-adaptive-match.json")));
        var frozen = Path.Combine(state.Root, anchor.RootElement.GetProperty("nativeFile").GetString()!);
        var start = new ProcessStartInfo(Executable) { RedirectStandardInput = true, RedirectStandardOutput = true, RedirectStandardError = true, UseShellExecute = false };
        foreach (var argument in new[] { "jpeg-samples-nonseekable-encode", state.Input, "513", "259", state.NativeOutput, frozen }) start.ArgumentList.Add(argument);
        process = Process.Start(start) ?? throw new InvalidOperationException("Could not start nonseekable JPEG encode reference.");
        errors = process.StandardError.ReadToEndAsync();
        try
        {
            Ready = Read();
            if (Ready.GetProperty("state").GetString() != "ready" || Ready.GetProperty("operation").GetString() != "jpeg-samples-nonseekable-encode"
                || Ready.GetProperty("schemaVersion").GetInt32() != 1 || Ready.GetProperty("width").GetInt32() != 513
                || Ready.GetProperty("height").GetInt32() != 259 || Ready.GetProperty("depth").GetInt32() != 8
                || Ready.GetProperty("cpu").GetInt32() != cpu || Ready.GetProperty("affinityCount").GetInt32() != 1
                || Ready.GetProperty("codecThreads").GetInt32() != 1 || Ready.GetProperty("conversionThreads").GetInt32() != 1
                || Ready.GetProperty("threadType").GetInt32() != 0 || Ready.GetProperty("activeThreadType").GetInt32() != 0
                || Ready.GetProperty("nativeQuality").GetInt32() != 5 || Ready.GetProperty("sampling").GetString() != "420" || Ready.GetProperty("rawInputChannels").GetInt32() != 3 || Ready.GetProperty("rgbaIntermediate").GetBoolean()
                || Ready.GetProperty("seekCallback").GetBoolean()
                || Ready.GetProperty("lengthQueries").GetInt32() != 0 || Ready.GetProperty("outputView").GetBoolean()
                || Ready.GetProperty("outputBytes").GetInt32() <= 0
                || Ready.GetProperty("writes").GetInt32() <= 0 || Ready.GetProperty("growths").GetInt32() <= 0
                || new[] { "rawInputBorrowed", "colorConversionTimed", "freshNonSeekableOutput", "outputGrowthTimed", "outputCopyTimed", "encodedStagingTimed", "internalAvioFlushTimed",
                    "streamAcquisitionAndReleaseTimed", "residentCallbacksImmediate", "callerOutputLeftOpen", "independentLiveOutputs", "independentOutputFromStaging" }
                    .Any(key => !Ready.GetProperty(key).GetBoolean()))
                throw new InvalidOperationException("Nonseekable JPEG encode output/ownership/settings/CPU boundary differs.");
            completed = Ready.GetProperty("initialCompletedOperations").GetInt64();
        }
        catch { Dispose(); throw; }
    }
}
