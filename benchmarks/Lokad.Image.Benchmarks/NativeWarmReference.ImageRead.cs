using System.Diagnostics;
using System.Text.Json;
using Lokad.Image;

internal enum ReadReferenceMode { ProbeSeekable, ProbeNonseekable, ValidateSpan, ValidateNonseekable }

internal sealed partial class NativeWarmReference
{
    internal NativeWarmReference(JpegBenchmarkCase state, int cpu, ImageInfo expected, ReadReferenceMode mode)
    {
        var operation = mode switch
        {
            ReadReferenceMode.ProbeSeekable => "probe-seekable", ReadReferenceMode.ProbeNonseekable => "probe-nonseekable",
            ReadReferenceMode.ValidateSpan => "validate-span", _ => "validate-nonseekable"
        };
        var probe = mode is ReadReferenceMode.ProbeSeekable or ReadReferenceMode.ProbeNonseekable;
        var nonseek = mode is ReadReferenceMode.ProbeNonseekable or ReadReferenceMode.ValidateNonseekable;
        Executable = Path.Combine(state.Root, "external/warm-reference/ffmpeg-image-read-warm");
        using (var document = JsonDocument.Parse(File.ReadAllBytes(Path.Combine(state.Root, "external/warm-reference/image-read-identity.json"))))
            BuildIdentity = document.RootElement.Clone();
        if (BuildIdentity.GetProperty("workerSha256").GetString() != JpegBenchmarkCase.HashFile(Executable)
            || BuildIdentity.GetProperty("sourceSha256").GetString() != JpegBenchmarkCase.HashFile(Path.Combine(state.Root, "tools/WarmReference/ffmpeg-image-read-warm.c"))
            || BuildIdentity.GetProperty("buildScriptSha256").GetString() != JpegBenchmarkCase.HashFile(Path.Combine(state.Root, "tools/WarmReference/build-image-read-linux.sh")))
            throw new InvalidOperationException("Image-read reference identity differs.");
        var start = new ProcessStartInfo(Executable) { RedirectStandardInput = true, RedirectStandardOutput = true, RedirectStandardError = true, UseShellExecute = false };
        foreach (var argument in new[] { operation, state.Input, state.NativeOutput }) start.ArgumentList.Add(argument);
        process = Process.Start(start) ?? throw new InvalidOperationException("Image-read worker did not start.");
        errors = process.StandardError.ReadToEndAsync();
        try
        {
            Ready = Read();
            if (Ready.GetProperty("state").GetString() != "ready" || Ready.GetProperty("operation").GetString() != operation
                || Ready.GetProperty("schemaVersion").GetInt32() != 1 || Ready.GetProperty("format").GetInt32() != (int)expected.Format
                || Ready.GetProperty("width").GetInt32() != expected.Width || Ready.GetProperty("height").GetInt32() != expected.Height
                || Ready.GetProperty("cpu").GetInt32() != cpu || Ready.GetProperty("affinityCount").GetInt32() != 1
                || Ready.GetProperty("codecThreads").GetInt32() != 1 || Ready.GetProperty("threadType").GetInt32() != 0
                || Ready.GetProperty("activeThreadType").GetInt32() != 0 || Ready.GetProperty("outputBytes").GetInt32() != 36
                || Ready.GetProperty("pixelDecode").GetBoolean() == probe || Ready.GetProperty("freshStream").GetBoolean() != (probe || nonseek)
                || Ready.GetProperty("nonseekable").GetBoolean() != nonseek
                || new[] { "rgbaConversion", "callerPixelCopy", "fuelAccounting" }.Any(k => Ready.GetProperty(k).GetBoolean())
                || new[] { "metadataOnly", "independentLiveOutputs", "sourceUnchanged", "residentCallbacksImmediate" }.Any(k => !Ready.GetProperty(k).GetBoolean()))
                throw new InvalidOperationException("Image-read reference boundary differs.");
            completed = Ready.GetProperty("initialCompletedOperations").GetInt64();
        }
        catch { Dispose(); throw; }
    }
}
