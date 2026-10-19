using System.Text.Json;
using Lokad.Image;
using Lokad.Image.Tooling;

internal sealed class WarmJpeg411Decode
{
    private JpegBenchmarkCase state = null!;
    private NativeWarmReference native = null!;
    private byte[] input = [], expected = [], nativePixels = [];
    private NativeWarmSample[] before = [];
    private JsonElement prepared, sourceIdentity, validation;
    private int cpu, count, maximum, scalarMaximum;
    private double rmse, scalarRmse;
    private bool ready, smoke;

    public void Setup(int cpu, bool smoke, int count)
    {
        this.cpu = cpu; this.smoke = smoke; this.count = count; state = new JpegBenchmarkCase();
        try
        {
            var folder = Path.Combine(state.Root, "artifacts/warm-reference/jpeg-411");
            using (var document = JsonDocument.Parse(File.ReadAllText(Path.Combine(folder, "preparation.json")))) prepared = document.RootElement.Clone();
            using (var document = JsonDocument.Parse(File.ReadAllText(Path.Combine(folder, "native-validation.json")))) validation = document.RootElement.Clone();
            input = File.ReadAllBytes(Path.Combine(folder, "input.jpg")); expected = File.ReadAllBytes(Path.Combine(folder, "expected.rgba"));
            if (prepared.GetProperty("project").GetString() != "Lokad.Image" || prepared.GetProperty("schemaVersion").GetInt32() != 1
                || prepared.GetProperty("rasterSha256").GetString() != JpegBenchmarkCase.Hash(HorizontalFlipInput.Create(8))
                || prepared.GetProperty("encodedSha256").GetString() != "eeb88c7589ae0f2dd91c967103a9e8f9a849b4ab1991d42671263d0a88a56e77"
                || JpegBenchmarkCase.Hash(input) != prepared.GetProperty("encodedSha256").GetString() || input.Length != prepared.GetProperty("encodedBytes").GetInt32()
                || JpegBenchmarkCase.Hash(expected) != prepared.GetProperty("rgbaSha256").GetString()
                || JpegBenchmarkCase.HashFile(Path.Combine(folder, "projection.ycbcr")) != prepared.GetProperty("projectionSha256").GetString()
                || JpegBenchmarkCase.HashFile(Path.Combine(folder, "blocks.ycbcr")) != prepared.GetProperty("blocksSha256").GetString()
                || !validation.GetProperty("passed").GetBoolean() || !validation.GetProperty("nativePlanesExact").GetBoolean() || !validation.GetProperty("exactCenteredGridNormalization").GetBoolean()
                || validation.GetProperty("inputSha256").GetString() != JpegBenchmarkCase.Hash(input) || validation.GetProperty("ffmpegSha256").GetString() != JpegBenchmarkCase.HashFile(state.Ffmpeg))
                throw new InvalidOperationException("Original411 packet, analytical reference or centered native validation differs.");
            using (var document = JsonDocument.Parse(File.ReadAllText(Path.Combine(state.Root, "tmp/benchmark-source.json"))))
                sourceIdentity = JsonSerializer.SerializeToElement(new { sourceSha256 = document.RootElement.GetProperty("sourceSha256").GetString(), sourceRevision = document.RootElement.GetProperty("sourceRevision").GetString() });
            File.WriteAllBytes(state.Input, input); Validate();
            native = new NativeWarmReference(state, cpu, "jpeg-411-decode", expected.Length); nativePixels = File.ReadAllBytes(state.NativeOutput);
            (maximum, rmse) = Agreement(nativePixels);
            foreach (var output in validation.GetProperty("outputs").EnumerateArray())
            {
                var pixels = File.ReadAllBytes(Path.Combine(folder, output.GetProperty("file").GetString()!));
                if (JpegBenchmarkCase.Hash(pixels) != output.GetProperty("sha256").GetString()) throw new InvalidOperationException("Frozen411 CLI samples differ.");
                if (output.GetProperty("scalar").GetBoolean()) (scalarMaximum, scalarRmse) = Agreement(pixels);
                else if (!pixels.AsSpan().SequenceEqual(nativePixels)) throw new InvalidOperationException("Native411 worker and explicit centered CLI differ.");
            }
            before = native.Measure(smoke, sampleCount: count); ready = true;
        }
        catch { Dispose(); throw; }
    }

    private (int Maximum, double Rmse) Agreement(byte[] actual)
    {
        var result = JpegQualityCorpus.Compare(expected, actual);
        if (result.Maximum > 4 || result.Rmse > 1.5) throw new InvalidOperationException("JPEG411 maximum4/RMSE1.5 exceeded.");
        return (result.Maximum, result.Rmse);
    }

    private void Validate()
    {
        using var first = ImageCodec.Decode(input); using var survivor = ImageCodec.Decode(input);
        if ((first.Width, first.Height, first.BitDepth, first.Orientation) != (513, 259, 8, ImageOrientation.Normal)
            || !OriginalPixels.Bytes(first).AsSpan().SequenceEqual(expected) || !OriginalPixels.Bytes(survivor).AsSpan().SequenceEqual(expected))
            throw new InvalidOperationException("Exact analytical411 RGBA pixels differ.");
        first.GetRow8(0)[0] ^= 255; first.Dispose();
        if (!OriginalPixels.Bytes(survivor).AsSpan().SequenceEqual(expected) || JpegBenchmarkCase.Hash(input) != prepared.GetProperty("encodedSha256").GetString()
            || !File.ReadAllBytes(state.Input).AsSpan().SequenceEqual(input)) throw new InvalidOperationException("JPEG411 source changed or outputs alias.");
    }

    public int Apply() { using var output = ImageCodec.Decode(input); return output.Stride * output.Height ^ output.GetRow8(0)[513 * 2]; }

    public void Cleanup()
    {
        try
        {
            if (!ready) return;
            Validate(); var after = native.Measure(smoke, before[0].Operations, count); native.Check(); native.Stop();
            if (!File.ReadAllBytes(state.NativeOutput).AsSpan().SequenceEqual(nativePixels)) throw new InvalidOperationException("Repeated411 native samples differ.");
            var path = Path.Combine(state.Root, "artifacts/benchmark-validation", "WarmJpegDecode-" + Guid.NewGuid().ToString("N") + ".json"); Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            File.WriteAllText(path, JsonSerializer.Serialize(new
            {
                schemaVersion = 1, name = "WarmJpegDecode", caseId = "Decode411", publicBoundary = "JpegDecode/411Resident",
                runId = Environment.GetEnvironmentVariable("LOKAD_IMAGE_BENCHMARK_RUN_ID"), smoke, cpu, width = 513, height = 259, depth = 8, sourceIdentity,
                preparation = prepared, nativeValidation = validation, inputSha256 = JpegBenchmarkCase.Hash(input), inputBytes = input.Length,
                managedOutputBytes = expected.Length, nativeOutputBytes = nativePixels.Length, managedOutputSha256 = JpegBenchmarkCase.Hash(expected), nativeOutputSha256 = JpegBenchmarkCase.Hash(nativePixels),
                decoderMaximum = maximum, decoderRmse = rmse, scalarDecoderMaximum = scalarMaximum, scalarDecoderRmse = scalarRmse,
                decoderAgreementWithinBounds = true, exactAnalyticalManagedPixels = true, nativeCliPixelsExact = true, scalarCliAgreementWithinBounds = true,
                alphaExact = true, sourceUnchanged = true, independentOwnedOutput = true, iccConversion = false, exifReorientation = false,
                librarySha256 = JpegBenchmarkCase.HashFile(typeof(ImageBuffer).Assembly.Location), benchmarkAssemblySha256 = JpegBenchmarkCase.HashFile(typeof(WarmJpeg411Decode).Assembly.Location),
                nativeBuild = native.BuildIdentity, nativeReady = native.Ready, nativeBefore = before, nativeAfter = after, nativeExitCode = 0,
                boundary = "Same resident original authored DC/unit-quantization baseline H4V1 JPEG to fresh independently owned RGBA8, all three component decodes, exact centered quarter-width chroma, full-range BT601 matrix/opaque alpha, output acquisition/disposal and plane release timed. FFmpeg softwareMJPEG plus explicit centered516-column grid conversion and in-place compaction to513, extending only owned luma padding; original packet and valid decoded samples unchanged. No second RGBA raster or decoded cache. Codec/conversion/packet/empty wrapper reused. ICC/EXIF transforms disabled. One CPU/thread; startup, preparation, I/O, IPC and checks excluded. Exact analytical managed pixels; normal/scalar native maximum4/RMSE1.5 and exact alpha."
            }, EncoderQualityCalibration.Json) + "\n");
            Console.WriteLine("Validated JPEG411 decode: " + path);
        }
        finally { Dispose(); }
    }

    private void Dispose() { native?.Dispose(); state?.Dispose(); ready = false; }
}
