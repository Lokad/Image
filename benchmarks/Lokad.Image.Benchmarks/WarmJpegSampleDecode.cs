using System.Text.Json;
using Lokad.Image;
using Lokad.Image.Tooling;

// One named public output boundary; existing fixture/job/reference library.
internal sealed class WarmJpegSampleDecode
{
    private JpegBenchmarkCase state = null!;
    private NativeWarmReference native = null!;
    private byte[] input = [], expected = [], nativePixels = [];
    private WarmJpegFixedPreparation prepared = null!;
    private NativeWarmSample[] before = [];
    private JsonElement sourceIdentity;
    private bool ready, smoke;
    private int cpu, count, maximum, scalarMaximum;
    private double rmse, scalarRmse;

    public void Setup(int cpu, bool smoke, int count)
    {
        this.cpu = cpu; this.smoke = smoke; this.count = count; state = new JpegBenchmarkCase();
        try
        {
            prepared = JsonSerializer.Deserialize<WarmJpegFixedPreparation>(File.ReadAllText(Path.Combine(state.Root, "artifacts/warm-reference/jpeg-decodegray.json")), EncoderQualityCalibration.Json)
                ?? throw new InvalidOperationException("Prepared gray JPEG missing.");
            input = File.ReadAllBytes(Path.Combine(state.Root, prepared.NativeFile));
            if (prepared.Project != "Lokad.Image" || prepared.SchemaVersion != 1 || input.Length != 147825
                || JpegBenchmarkCase.Hash(input) != prepared.EncodedSha256 || prepared.EncodedSha256 != "a53c7643717578453750459a7ecc7edb84c4d95366bde811a4f82db242960859"
                || prepared.RasterSha256 != JpegBenchmarkCase.Hash(HorizontalFlipInput.Create(8))
                || prepared.FfmpegSha256 != JpegBenchmarkCase.HashFile(state.Ffmpeg))
                throw new InvalidOperationException("Existing gray JPEG source/preparation identity differs.");
            using (var manifest = JsonDocument.Parse(File.ReadAllText(Path.Combine(state.Root, "tmp/benchmark-source.json"))))
                sourceIdentity = JsonSerializer.SerializeToElement(new { sourceSha256 = manifest.RootElement.GetProperty("sourceSha256").GetString(), sourceRevision = manifest.RootElement.GetProperty("sourceRevision").GetString() });
            // Preserve the previously qualified exact managed pixels; the old
            // RGBA expansion is an untimed oracle, absent from Apply().
            using (var rgba = ImageCodec.Decode(input))
            {
                var pixels = OriginalPixels.Bytes(rgba);
                if (JpegBenchmarkCase.Hash(pixels) != prepared.ManagedDecodedSha256) throw new InvalidOperationException("Qualified gray pixels changed.");
                expected = new byte[513 * 259];
                for (var i = 0; i < expected.Length; i++)
                {
                    if (pixels[i * 4] != pixels[i * 4 + 1] || pixels[i * 4] != pixels[i * 4 + 2] || pixels[i * 4 + 3] != 255)
                        throw new InvalidOperationException("Gray projection/opaque alpha differs.");
                    expected[i] = pixels[i * 4];
                }
            }
            File.WriteAllBytes(state.Input, input); native = new NativeWarmReference(state, cpu, JpegSampleFormat.Grayscale);
            nativePixels = File.ReadAllBytes(state.NativeOutput); (maximum, rmse) = Agreement(nativePixels, expected);
            foreach (var scalar in new[] { false, true })
            {
                List<string> arguments = ["-hide_banner", "-loglevel", "error"];
                if (scalar) arguments.AddRange(["-cpuflags", "0"]);
                arguments.AddRange(["-threads:v", "1", "-thread_type:v", "0", "-noautorotate", "-f", "image2pipe", "-c:v", "mjpeg", "-i", "pipe:0", "-filter_threads", "1", "-vf", "format=gray", "-frames:v", "1", "-threads:v", "1", "-pix_fmt", "gray", "-f", "rawvideo", "pipe:1"]);
                var oracle = OracleProcess.Run(state.Ffmpeg, arguments.ToArray(), input);
                if (scalar) (scalarMaximum, scalarRmse) = Agreement(oracle, expected);
                else if (!oracle.AsSpan().SequenceEqual(nativePixels)) throw new InvalidOperationException("Native gray packing differs from public FFmpeg CLI.");
            }
            Validate(); before = native.Measure(smoke, sampleCount: count); ready = true;
        }
        catch { Dispose(); throw; }
    }

    private static (int Maximum, double Rmse) Agreement(byte[] a, byte[] b)
    {
        if (a.Length != 513 * 259 || a.Length != b.Length) throw new InvalidOperationException("Gray dimensions differ.");
        var maximum = 0; long squared = 0;
        for (var i = 0; i < a.Length; i++) { var error = Math.Abs(a[i] - b[i]); maximum = Math.Max(maximum, error); squared += error * error; }
        var rmse = Math.Sqrt((double)squared / a.Length);
        if (maximum > 4 || rmse > 1.5) throw new InvalidOperationException("Existing JPEG maximum4/RMSE1.5 gate exceeded.");
        return (maximum, rmse);
    }

    public int Apply()
    {
        var output = new byte[513 * 259];
        ImageCodec.DecodeJpeg(input, output, JpegSampleFormat.Grayscale);
        return output.Length ^ output[output.Length / 2];
    }

    private void Validate()
    {
        var first = new byte[expected.Length]; var second = new byte[expected.Length];
        var info = ImageCodec.DecodeJpeg(input, first, JpegSampleFormat.Grayscale);
        ImageCodec.DecodeJpeg(input, second, JpegSampleFormat.Grayscale);
        if ((info.Width, info.Height, info.BitDepth, info.Orientation) != (513, 259, 8, ImageOrientation.Normal)
            || !first.AsSpan().SequenceEqual(expected) || !second.AsSpan().SequenceEqual(expected)) throw new InvalidOperationException("Caller gray samples/geometry differ.");
        first[0] ^= 255; Array.Clear(first);
        if (!second.AsSpan().SequenceEqual(expected)) throw new InvalidOperationException("Caller outputs alias.");
        var padded = Enumerable.Repeat((byte)165, 520 * 259 + 17).ToArray();
        ImageCodec.DecodeJpeg(input, padded, JpegSampleFormat.Grayscale, stride: 520);
        for (var y = 0; y < 259; y++)
            if (!padded.AsSpan(y * 520, 513).SequenceEqual(expected.AsSpan(y * 513, 513))
                || padded.AsSpan(y * 520 + 513, 7).ContainsAnyExcept((byte)165)) throw new InvalidOperationException("Caller gray padding changed.");
        if (padded.AsSpan(520 * 259).ContainsAnyExcept((byte)165) || JpegBenchmarkCase.Hash(input) != prepared.EncodedSha256
            || !File.ReadAllBytes(state.Input).AsSpan().SequenceEqual(input)) throw new InvalidOperationException("Caller gray suffix/source changed.");
    }

    public void Cleanup()
    {
        try
        {
            if (!ready) return;
            Validate(); var after = native.Measure(smoke, before[0].Operations, count); native.Check(); native.Stop();
            if (!File.ReadAllBytes(state.NativeOutput).AsSpan().SequenceEqual(nativePixels)) throw new InvalidOperationException("Repeated native samples differ.");
            var path = Path.Combine(state.Root, "artifacts/benchmark-validation", "WarmJpegDecode-" + Guid.NewGuid().ToString("N") + ".json");
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            File.WriteAllText(path, JsonSerializer.Serialize(new
            {
                schemaVersion = 1, name = "WarmJpegDecode", caseId = "OwnedDecodeGray", publicBoundary = "JpegSamplesDecode/GrayscaleResident",
                runId = Environment.GetEnvironmentVariable("LOKAD_IMAGE_BENCHMARK_RUN_ID"), smoke, cpu, width = 513, height = 259, depth = 8,
                sourceIdentity, inputSha256 = prepared.EncodedSha256, rasterSha256 = prepared.RasterSha256, inputBytes = input.Length,
                managedOutputBytes = expected.Length, nativeOutputBytes = nativePixels.Length, managedOutputSha256 = JpegBenchmarkCase.Hash(expected), nativeOutputSha256 = JpegBenchmarkCase.Hash(nativePixels),
                decoderMaximum = maximum, decoderRmse = rmse, scalarDecoderMaximum = scalarMaximum, scalarDecoderRmse = scalarRmse,
                decoderAgreementWithinBounds = true, exactQualifiedManagedPixels = true, nativeCliPixelsExact = true, scalarCliAgreementWithinBounds = true,
                sourceUnchanged = true, independentOwnedOutput = true, callerPaddingUnchanged = true, noRgbaIntermediate = true, iccConversion = false, exifReorientation = false,
                librarySha256 = JpegBenchmarkCase.HashFile(typeof(ImageBuffer).Assembly.Location), benchmarkAssemblySha256 = JpegBenchmarkCase.HashFile(typeof(WarmJpegSampleDecode).Assembly.Location),
                nativeBuild = native.BuildIdentity, nativeReady = native.Ready, nativeBefore = before, nativeAfter = after, nativeExitCode = 0,
                boundary = "Identical resident prepared grayscale JPEG to fresh packed caller-owned GRAY8. Managed public DecodeJpeg includes new zeroed destination, admission, complete decode/normalization and GC ownership; native software mjpeg includes state reset, zeroed destination allocation, complete decode, row packing/copy, decoded-plane release and caller-output release. No RGBA intermediate, ICC conversion or EXIF reorientation. Native context, padded resident packet and empty frame wrapper reused; every operation decodes all samples. Startup, I/O, IPC, hashing and checks excluded. One CPU. Managed samples exact to qualified gray projection; native normal CLI exact, normal/scalar maximum4/RMSE1.5. Source/padding and independent surviving outputs checked outside timing."
            }, EncoderQualityCalibration.Json) + "\n");
            Console.WriteLine("Validated caller gray JPEG decode: " + path);
        }
        finally { Dispose(); }
    }

    private void Dispose() { native?.Dispose(); state?.Dispose(); ready = false; }
}
