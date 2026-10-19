using System.Text.Json;
using Lokad.Image;

internal sealed class WarmJpegYcckRgbDecode
{
    private JpegBenchmarkCase state = null!;
    private NativeWarmReference native = null!;
    private YcckRgbPreparation prepared = null!;
    private byte[] input = [], expected = [], nativePixels = [];
    private NativeWarmSample[] before = [];
    private JsonElement sourceIdentity;
    private int cpu, count, maximum, scalarMaximum;
    private double rmse, scalarRmse;
    private bool ready, smoke;

    public void Setup(int cpu, bool smoke, int count)
    {
        this.cpu = cpu; this.smoke = smoke; this.count = count; state = new JpegBenchmarkCase();
        try
        {
            var directory = WarmJpegYcckRgbInput.DirectoryPath(state.Root);
            prepared = JsonSerializer.Deserialize<YcckRgbPreparation>(File.ReadAllText(Path.Combine(directory, "benchmark-preparation.json")), EncoderQualityCalibration.Json) ?? throw new InvalidOperationException("Run --prepare-warm-jpeg-ycck420-caller-rgb first.");
            input = File.ReadAllBytes(Path.Combine(WarmJpegYcckInput.DirectoryPath(state.Root), "input.jpg")); expected = File.ReadAllBytes(Path.Combine(directory, "expected.rgb"));
            if (prepared.Project != "Lokad.Image" || prepared.SchemaVersion != 1 || input.Length != 6704 || expected.Length != 398601
                || prepared.EncodedSha256 != "211fcc243d40db6e6e3e8944a2e479825289201bff140e5cf2901e85ee97053d"
                || input.Length != prepared.EncodedBytes || JpegBenchmarkCase.Hash(input) != prepared.EncodedSha256 || JpegBenchmarkCase.Hash(expected) != prepared.RgbSha256
                || JpegBenchmarkCase.HashFile(state.Ffmpeg) != prepared.FfmpegSha256)
                throw new InvalidOperationException("Original canonical YCCK RGB packet/sample identities differ.");
            using (var manifest = JsonDocument.Parse(File.ReadAllText(Path.Combine(state.Root, "tmp/benchmark-source.json"))))
                sourceIdentity = JsonSerializer.SerializeToElement(new { sourceSha256 = manifest.RootElement.GetProperty("sourceSha256").GetString(), sourceRevision = manifest.RootElement.GetProperty("sourceRevision").GetString() });
            File.WriteAllBytes(state.Input, input); Validate();
            native = new NativeWarmReference(state, cpu, "jpeg-ycck-rgb-decode", expected.Length); nativePixels = File.ReadAllBytes(state.NativeOutput);
            (maximum, rmse) = Agreement(nativePixels, expected);
            using (var validation = JsonDocument.Parse(File.ReadAllText(Path.Combine(directory, "native-validation.json"))))
            {
                var v = validation.RootElement;
                if (!v.GetProperty("passed").GetBoolean() || v.GetProperty("inputSha256").GetString() != prepared.EncodedSha256 || v.GetProperty("ffmpegSha256").GetString() != prepared.FfmpegSha256)
                    throw new InvalidOperationException("Raw YCCK RGB native CLI checks differ.");
                foreach (var output in v.GetProperty("outputs").EnumerateArray())
                {
                    var pixels = File.ReadAllBytes(Path.Combine(directory, output.GetProperty("file").GetString()!));
                    if (JpegBenchmarkCase.Hash(pixels) != output.GetProperty("sha256").GetString()) throw new InvalidOperationException("Frozen native RGB24 bytes differ.");
                    if (output.GetProperty("scalar").GetBoolean()) (scalarMaximum, scalarRmse) = Agreement(pixels, expected);
                    else if (!pixels.AsSpan().SequenceEqual(nativePixels)) throw new InvalidOperationException("Native RGB24 normalization differs from public CLI.");
                }
            }
            before = native.Measure(smoke, sampleCount: count); ready = true;
        }
        catch { Dispose(); throw; }
    }

    internal static (int Maximum, double Rmse) Agreement(byte[] actual, byte[] expected)
    {
        if (actual.Length != 398601 || actual.Length != expected.Length) throw new InvalidOperationException("YCCK caller RGB geometry differs.");
        var maximum = 0; long squared = 0;
        for (var i = 0; i < actual.Length; i++)
        {
            var error = Math.Abs(actual[i] - expected[i]); maximum = Math.Max(maximum, error); squared += error * error;
            var pixel = i / 3; var x = pixel % 513; var y = pixel / 513;
            var transition = x % 16 is 0 or 15 || y % 16 is 0 or 15;
            if (error > (transition ? 16 : 4)) throw new InvalidOperationException("Fixed YCCK native transition/interior bound16/4 exceeded.");
        }
        var rmse = Math.Sqrt((double)squared / actual.Length);
        if (rmse > 1.5) throw new InvalidOperationException("Fixed YCCK native RGB RMSE1.5 exceeded.");
        return (maximum, rmse);
    }

    public int Apply()
    {
        var output = new byte[398601]; ImageCodec.DecodeJpeg(input, output, JpegSampleFormat.Rgb);
        return output.Length ^ output[output.Length / 2];
    }

    private void Validate()
    {
        var first = new byte[expected.Length]; var second = new byte[expected.Length];
        var info = ImageCodec.DecodeJpeg(input, first, JpegSampleFormat.Rgb); ImageCodec.DecodeJpeg(input, second, JpegSampleFormat.Rgb);
        if ((info.Width, info.Height, info.BitDepth, info.Orientation) != (513, 259, 8, ImageOrientation.Normal)
            || !first.AsSpan().SequenceEqual(expected) || !second.AsSpan().SequenceEqual(expected)) throw new InvalidOperationException("Analytical caller YCCK RGB samples/geometry differ.");
        first[0] ^= 255; Array.Clear(first); if (!second.AsSpan().SequenceEqual(expected)) throw new InvalidOperationException("Caller YCCK RGB outputs alias.");
        var padded = Enumerable.Repeat((byte)165, 1546 * 259 + 17).ToArray();
        ImageCodec.DecodeJpeg(input, padded, JpegSampleFormat.Rgb, stride: 1546);
        for (var y = 0; y < 259; y++)
            if (!padded.AsSpan(y * 1546, 1539).SequenceEqual(expected.AsSpan(y * 1539, 1539)) || padded.AsSpan(y * 1546 + 1539, 7).ContainsAnyExcept((byte)165))
                throw new InvalidOperationException("Caller YCCK RGB row/padding differs.");
        if (padded.AsSpan(1546 * 259).ContainsAnyExcept((byte)165) || JpegBenchmarkCase.Hash(input) != prepared.EncodedSha256
            || !File.ReadAllBytes(state.Input).AsSpan().SequenceEqual(input)) throw new InvalidOperationException("YCCK RGB suffix/source changed.");
    }

    public void Cleanup()
    {
        try
        {
            if (!ready) return;
            Validate(); var after = native.Measure(smoke, before[0].Operations, count); native.Check(); native.Stop();
            if (!File.ReadAllBytes(state.NativeOutput).AsSpan().SequenceEqual(nativePixels)) throw new InvalidOperationException("Repeated native YCCK RGB samples differ.");
            var path = Path.Combine(state.Root, "artifacts/benchmark-validation", "WarmJpegDecode-" + Guid.NewGuid().ToString("N") + ".json"); Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            File.WriteAllText(path, JsonSerializer.Serialize(new
            {
                schemaVersion = 1, name = "WarmJpegDecode", caseId = "Ycck420CallerRgb", publicBoundary = "JpegSamplesDecode/Ycck420RgbResident",
                runId = Environment.GetEnvironmentVariable("LOKAD_IMAGE_BENCHMARK_RUN_ID"), smoke, cpu, width = 513, height = 259, depth = 8, sourceIdentity,
                preparation = prepared, inputSha256 = prepared.EncodedSha256, rasterSha256 = prepared.RasterSha256, inputBytes = input.Length,
                managedOutputBytes = expected.Length, nativeOutputBytes = nativePixels.Length, managedOutputSha256 = prepared.RgbSha256, nativeOutputSha256 = JpegBenchmarkCase.Hash(nativePixels),
                decoderMaximum = maximum, decoderRmse = rmse, scalarDecoderMaximum = scalarMaximum, scalarDecoderRmse = scalarRmse,
                decoderAgreementWithinBounds = true, exactAnalyticalManagedPixels = true, nativeCliPixelsExact = true, scalarCliAgreementWithinBounds = true,
                blackIsNotAlpha = true, outputHasAlpha = false, sourceUnchanged = true, independentOwnedOutput = true, callerPaddingUnchanged = true, noRgbaIntermediate = true, iccConversion = false, exifReorientation = false,
                librarySha256 = JpegBenchmarkCase.HashFile(typeof(ImageBuffer).Assembly.Location), benchmarkAssemblySha256 = JpegBenchmarkCase.HashFile(typeof(WarmJpegYcckRgbDecode).Assembly.Location),
                nativeBuild = native.BuildIdentity, nativeReady = native.Ready, nativeBefore = before, nativeAfter = after, nativeExitCode = 0,
                approvedNativeOnlyGate = "Same fixed YCCK420 packet: RGB<=16 only at authored x/y mod16=0or15 transitions, <=4 elsewhere, RGB RMSE<=1.5. Exact managed RGB. Native RGB24 matches approved RGBA colors exactly; no alpha output.",
                boundary = "Same original resident Adobe2 YCCK420/full-K packet to fresh zeroed packed caller RGB24. Includes admission, complete decode of all4components, centered interpolation, binary64 matrix/inversion/black, packing and independent output lifetime. Native software mjpeg resets state, decodes all4components/K, applies chroma/black normalization, converts YUVA444P full-range BT601 directly to freshRGB24 and releases planes/output. Context, padded resident packet and empty wrappers reused, never decoded content. No RGBA intermediate/header patch/ICC/EXIF transformation. Preparation, hashing, validation, I/O, IPC and startup excluded. One CPU/thread. Exact analytical managed samples; native normal/scalar CLI bound transition16/interior4/RMSE1.5 fixed to this packet. Source/padding and two independent live output owners checked outside timing."
            }, EncoderQualityCalibration.Json) + "\n");
            Console.WriteLine("Validated caller YCCK RGB JPEG decode: " + path);
        }
        finally { Dispose(); }
    }

    private void Dispose() { native?.Dispose(); state?.Dispose(); ready = false; }
}
