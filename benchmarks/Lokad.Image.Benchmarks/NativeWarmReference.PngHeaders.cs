using System.Diagnostics;
using System.Text.Json;
using Lokad.Image;

internal sealed partial class NativeWarmReference
{
    public NativeWarmReference(JpegBenchmarkCase state, int cpu, ImageInfo expected, string publicBoundary)
    {
        if (publicBoundary != "InspectHeaders/PngRgba8FreshSeekable"
            || expected != new ImageInfo(ImageFormat.Png, 513, 259, 8, true, ImageOrientation.Normal)
                { SourceColor = SourceColorModel.Rgb, SourceAlpha = SourceAlphaRepresentation.Channel, SourceComponentCount = 4 })
            throw new ArgumentOutOfRangeException(nameof(publicBoundary));
        Executable = Path.Combine(state.Root, "external/warm-reference/ffmpeg-png-headers-warm");
        using (var document = JsonDocument.Parse(File.ReadAllBytes(Path.Combine(state.Root, "external/warm-reference/png-headers-identity.json"))))
            BuildIdentity = document.RootElement.Clone();
        if (BuildIdentity.GetProperty("workerSha256").GetString() != JpegBenchmarkCase.HashFile(Executable)
            || BuildIdentity.GetProperty("sourceSha256").GetString() != JpegBenchmarkCase.HashFile(Path.Combine(state.Root, "tools/WarmReference/ffmpeg-png-headers-warm.c"))
            || BuildIdentity.GetProperty("buildScriptSha256").GetString() != JpegBenchmarkCase.HashFile(Path.Combine(state.Root, "tools/WarmReference/build-png-headers-linux.sh")))
            throw new InvalidOperationException("Bounded PNG header reference identity differs.");
        var start = new ProcessStartInfo(Executable) { RedirectStandardInput = true, RedirectStandardOutput = true, RedirectStandardError = true, UseShellExecute = false };
        foreach (var argument in new[] { "png-header-inspect", state.Input, state.NativeOutput }) start.ArgumentList.Add(argument);
        process = Process.Start(start) ?? throw new InvalidOperationException("Could not start bounded PNG header reference.");
        errors = process.StandardError.ReadToEndAsync();
        try
        {
            Ready = Read();
            if (Ready.GetProperty("state").GetString() != "ready" || Ready.GetProperty("operation").GetString() != "png-header-inspect"
                || Ready.GetProperty("schemaVersion").GetInt32() != 1 || Ready.GetProperty("width").GetInt32() != expected.Width
                || Ready.GetProperty("height").GetInt32() != expected.Height || Ready.GetProperty("depth").GetInt32() != expected.BitDepth
                || Ready.GetProperty("hasAlpha").GetBoolean() != expected.HasAlpha || Ready.GetProperty("orientation").GetInt32() != 1
                || Ready.GetProperty("cpu").GetInt32() != cpu || Ready.GetProperty("affinityCount").GetInt32() != 1
                || Ready.GetProperty("codecThreads").GetInt32() != 1 || Ready.GetProperty("conversionThreads").GetInt32() != 0
                || Ready.GetProperty("threadType").GetInt32() != 0 || Ready.GetProperty("activeThreadType").GetInt32() != 0
                || Ready.GetProperty("scratchBytes").GetInt32() != 8192 || Ready.GetProperty("headerBytes").GetInt32() != 32
                || Ready.GetProperty("outputBytes").GetInt32() != 24 || Ready.GetProperty("seekCalls").GetInt32() <= 0
                || Ready.GetProperty("eofReads").GetInt32() <= 0 || Ready.GetProperty("lengthQueries").GetInt32() <= 0
                || Ready.GetProperty("readBytes").GetInt32() >= Ready.GetProperty("inputBytes").GetInt32()
                || new[] { "scalarMode", "pixelDecode", "wholeInputCopy", "inputView", "publicParserTimed", "fullCrcTimed" }
                    .Any(key => Ready.GetProperty(key).GetBoolean())
                || new[] { "independentLiveOutputs", "freshSeekableStream", "eofChecked", "callerSourceLeftOpen", "opaqueSeekEnabled",
                    "streamAcquisitionAndReleaseTimed", "structureMetadataTimed", "descriptorAcquisitionAndReleaseTimed" }
                    .Any(key => !Ready.GetProperty(key).GetBoolean()))
                throw new InvalidOperationException("Bounded PNG header reference metadata/input/ownership/settings differ.");
            completed = Ready.GetProperty("initialCompletedOperations").GetInt64();
        }
        catch { Dispose(); throw; }
    }
}
