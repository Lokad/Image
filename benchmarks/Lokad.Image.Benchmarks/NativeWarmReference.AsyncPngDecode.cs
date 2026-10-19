using System.Diagnostics;
using System.Globalization;
using System.Text.Json;

internal sealed partial class NativeWarmReference
{
    public NativeWarmReference(JpegBenchmarkCase state, int cpu, string expectedRaw, string operation)
    {
        if (operation != "png-nonseekable-decode") throw new ArgumentOutOfRangeException(nameof(operation));
        Executable = Path.Combine(state.Root, "external/warm-reference/ffmpeg-png-nonseekable-decode-warm");
        using (var document = JsonDocument.Parse(File.ReadAllBytes(Path.Combine(state.Root, "external/warm-reference/png-nonseekable-decode-identity.json"))))
            BuildIdentity = document.RootElement.Clone();
        if (BuildIdentity.GetProperty("workerSha256").GetString() != JpegBenchmarkCase.HashFile(Executable)
            || BuildIdentity.GetProperty("sourceSha256").GetString() != JpegBenchmarkCase.HashFile(Path.Combine(state.Root, "tools/WarmReference/ffmpeg-png-nonseekable-decode-warm.c"))
            || BuildIdentity.GetProperty("buildScriptSha256").GetString() != JpegBenchmarkCase.HashFile(Path.Combine(state.Root, "tools/WarmReference/build-png-nonseekable-decode-linux.sh")))
            throw new InvalidOperationException("Nonseekable PNG decode worker/source/build identity differs.");
        var start = new ProcessStartInfo(Executable) { RedirectStandardInput = true, RedirectStandardOutput = true, RedirectStandardError = true, UseShellExecute = false };
        foreach (var argument in new[] { operation, state.Input, "513", "259", "8", state.NativeOutput, expectedRaw }) start.ArgumentList.Add(argument);
        process = Process.Start(start) ?? throw new InvalidOperationException("Could not start nonseekable PNG decode reference.");
        errors = process.StandardError.ReadToEndAsync();
        try
        {
            Ready = Read();
            if (Ready.GetProperty("state").GetString() != "ready" || Ready.GetProperty("operation").GetString() != operation
                || Ready.GetProperty("schemaVersion").GetInt32() != 1 || Ready.GetProperty("width").GetInt32() != 513
                || Ready.GetProperty("height").GetInt32() != 259 || Ready.GetProperty("depth").GetInt32() != 8
                || Ready.GetProperty("orientation").GetInt32() != 1 || Ready.GetProperty("outputBytes").GetInt32() != 513 * 259 * 4
                || Ready.GetProperty("cpu").GetInt32() != cpu || Ready.GetProperty("affinityCount").GetInt32() != 1
                || Ready.GetProperty("codecThreads").GetInt32() != 1 || Ready.GetProperty("conversionThreads").GetInt32() != 0
                || Ready.GetProperty("threadType").GetInt32() != 0 || Ready.GetProperty("activeThreadType").GetInt32() != 0
                || Ready.GetProperty("scalarMode").GetBoolean() || Ready.GetProperty("freshSeekableStream").GetBoolean()
                || Ready.GetProperty("seekCallback").GetBoolean() || Ready.GetProperty("inputLengthQueryTimed").GetBoolean()
                || Ready.GetProperty("lengthQueries").GetInt32() != 0 || Ready.GetProperty("inputView").GetBoolean()
                || Ready.GetProperty("reads").GetInt32() <= 0 || Ready.GetProperty("eofReads").GetInt32() <= 0 || Ready.GetProperty("growths").GetInt32() <= 0
                || new[] { "freshNonSeekableStream", "inputCopyTimed", "eofChecked", "privateInputGrowthTimed", "noFinalInputCopy",
                    "residentCallbacksImmediate", "streamAcquisitionAndReleaseTimed", "fullStructureAndCrcTimed", "pixelDecode", "independentLiveOutputs" }
                    .Any(key => !Ready.GetProperty(key).GetBoolean()))
                throw new InvalidOperationException("Nonseekable PNG decode input/pixel/ownership/CPU boundary differs.");
            completed = Ready.GetProperty("initialCompletedOperations").GetInt64();
        }
        catch { Dispose(); throw; }
    }
}
