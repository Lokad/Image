using System.Buffers.Binary;
using System.Text.Json;
using Lokad.Image;
using Lokad.Image.Tooling;

internal sealed class WarmPngHeaders
{
    private const string InputHash = "4c0c823a1b84d7619f784ace6c2749ed10c0d54b7dc353beed3ad4888e0af57f";
    private static readonly ImageInfo Expected = new(ImageFormat.Png, 513, 259, 8, true, ImageOrientation.Normal)
        { SourceColor = SourceColorModel.Rgb, SourceAlpha = SourceAlphaRepresentation.Channel, SourceComponentCount = 4 };
    private JpegBenchmarkCase state = null!;
    private NativeWarmReference native = null!;
    private byte[] encoded = [];
    private NativeWarmSample[] before = [];
    private JsonElement proof, sourceIdentity;
    private int cpu, count;
    private bool smoke, ready;

    public void Setup(int cpu, bool smoke, int count)
    {
        this.cpu = cpu; this.smoke = smoke; this.count = count; state = new JpegBenchmarkCase();
        try
        {
            encoded = File.ReadAllBytes(Path.Combine(state.Root, "artifacts/warm-reference/png-cancel8/input.png"));
            if (JpegBenchmarkCase.Hash(encoded) != InputHash) throw new InvalidOperationException("Original PNG packet differs.");
            using (var document = JsonDocument.Parse(File.ReadAllBytes(Path.Combine(state.Root, "tmp/h17-png-headers-controls/proof.json")))) proof = document.RootElement.Clone();
            if (proof.GetProperty("timing").GetBoolean() || proof.GetProperty("librarySha256").GetString() != JpegBenchmarkCase.HashFile(typeof(ImageCodec).Assembly.Location)
                || proof.GetProperty("inputSha256").GetString() != InputHash || !proof.GetProperty("equivalentMetadataAndValidation").GetBoolean()
                || !proof.GetProperty("boundedSeekableDeliveryVerified").GetBoolean() || !proof.GetProperty("normalScalarNativeAgree").GetBoolean()
                || !proof.GetProperty("typedSourceMetadataExact").GetBoolean()
                || proof.GetProperty("controls").GetArrayLength() != 21) throw new InvalidOperationException("Bounded seekable equivalent-boundary prerequisite differs.");
            using (var document = JsonDocument.Parse(File.ReadAllBytes(Path.Combine(state.Root, "tmp/benchmark-source.json"))))
                sourceIdentity = JsonSerializer.SerializeToElement(new { sourceSha256 = document.RootElement.GetProperty("sourceSha256").GetString(), sourceRevision = document.RootElement.GetProperty("sourceRevision").GetString() });
            Validate(); File.WriteAllBytes(state.Input, encoded);
            native = new NativeWarmReference(state, cpu, Expected, "InspectHeaders/PngRgba8FreshSeekable");
            if (proof.GetProperty("nativeBuild").GetProperty("workerSha256").GetString() != native.BuildIdentity.GetProperty("workerSha256").GetString())
                throw new InvalidOperationException("Prepared native worker differs.");
            RequireNative(); before = native.Measure(smoke, sampleCount: count); ready = true;
        }
        catch { Dispose(); throw; }
    }

    private sealed class ObservedInput(byte[] bytes) : MemoryStream(bytes, writable: false)
    {
        public int Reads, EofReads, Seeks, LargestRequest;
        public long BytesRead;
        public Action? AfterRead;
        public override int Read(Span<byte> buffer)
        {
            Reads++; LargestRequest = Math.Max(LargestRequest, buffer.Length); var count = base.Read(buffer);
            BytesRead += count; if (count == 0) EofReads++; AfterRead?.Invoke(); return count;
        }
        public override long Seek(long offset, SeekOrigin origin) { Seeks++; return base.Seek(offset, origin); }
    }

    private ImageInfo Inspect(byte[] bytes, ImageLimits? limits = null, CancellationToken cancellation = default)
    {
        using var input = new ObservedInput(bytes);
        try
        {
            var info = ImageCodec.InspectHeaders(input, limits, cancellation);
            if (input.Position != bytes.Length || input.EofReads == 0 || input.LargestRequest > 8192 || input.TryGetBuffer(out _))
                throw new InvalidOperationException("Header input/EOF/fixed workspace/view differs.");
            return info;
        }
        finally { if (!input.CanRead) throw new InvalidOperationException("Header inspector closed caller input."); }
    }

    private void Validate()
    {
        var first = Inspect(encoded); var second = Inspect(encoded);
        if (first != Expected || second != Expected || ReferenceEquals(first, second)) throw new InvalidOperationException("Header descriptors differ or alias.");
        var copy = encoded.ToArray(); var survivor = Inspect(copy); Array.Clear(copy);
        if (survivor != Expected || first != Expected || second != Expected) throw new InvalidOperationException("Surviving metadata aliases source.");
        var exact = ImageLimits.Default with { MaxInputBytes = encoded.Length, MaxWorkingBytes = 8448};
        if (Inspect(encoded, exact) != Expected) throw new InvalidOperationException("Exact header admission differs.");
        foreach (var limits in new[] { exact with { MaxInputBytes = encoded.Length - 1 }, exact with { MaxWorkingBytes = 8447 } })
        {
            try { Inspect(encoded, limits); throw new InvalidOperationException("Header inspector accepted insufficient admission."); }
            catch (ImageLimitException) { }
        }
        using (var observed = new ObservedInput(encoded))
        {
            if (ImageCodec.InspectHeaders(observed) != Expected || observed.BytesRead >= encoded.Length || observed.Seeks <= 0
                || observed.EofReads == 0 || observed.LargestRequest > 8192 || !observed.CanRead)
                throw new InvalidOperationException("Opaque data not skipped or fixed-space EOF/input ownership differs.");
        }
        using (var canceled = new CancellationTokenSource())
        {
            using var observed = new ObservedInput(encoded) { AfterRead = canceled.Cancel };
            try { ImageCodec.InspectHeaders(observed, cancellationToken: canceled.Token); throw new InvalidOperationException("Header inspector ignored mid-read cancellation."); }
            catch (OperationCanceledException error) when (error.CancellationToken == canceled.Token && observed.CanRead) { }
        }
        using var cancellation = new CancellationTokenSource(); cancellation.Cancel();
        try { Inspect(encoded, cancellation: cancellation.Token); throw new InvalidOperationException("Header inspector accepted cancellation."); }
        catch (OperationCanceledException error) when (error.CancellationToken == cancellation.Token) { }
        if (JpegBenchmarkCase.Hash(encoded) != InputHash) throw new InvalidOperationException("Header source changed.");
    }

    public ImageInfo Apply()
    {
        using var input = new MemoryStream(encoded, writable: false);
        return ImageCodec.InspectHeaders(input);
    }

    private void RequireNative()
    {
        var bytes = File.ReadAllBytes(state.NativeOutput);
        int[] fields = [0, 513, 259, 8, 1, 1, 2, 2, 4];
        if (bytes.Length != 36) throw new InvalidOperationException("Native descriptor size differs.");
        for (var i = 0; i < fields.Length; i++) if (BinaryPrimitives.ReadInt32LittleEndian(bytes.AsSpan(i * 4)) != fields[i]) throw new InvalidOperationException("Native descriptor differs.");
    }

    public void Cleanup()
    {
        try
        {
            if (!ready) return;
            Validate(); var after = native.Measure(smoke, before[0].Operations, count); native.Check(); RequireNative(); native.Stop();
            var path = Path.Combine(state.Root, "artifacts/benchmark-validation/WarmStreamIdentify-" + Guid.NewGuid().ToString("N") + ".json"); Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            File.WriteAllText(path, JsonSerializer.Serialize(new
            {
                schemaVersion = 1, name = "WarmStreamIdentify", caseId = "Png8HeadersSeekable", publicBoundary = "InspectHeaders/PngRgba8FreshSeekable",
                runId = Environment.GetEnvironmentVariable("LOKAD_IMAGE_BENCHMARK_RUN_ID"), smoke, cpu, sourceIdentity, inputBytes = encoded.Length, inputSha256 = InputHash,
                descriptor = Expected, managedOutputBytes = 36, nativeOutputBytes = 36, descriptorExact = true, typedSourceMetadataExact = true, headerStructure = true, fullCrc = false, pixelDecode = false,
                freshSeekableStream = true, opaquePayloadsSkipped = true, inputView = false, fixedWorkspace = 8448, eofChecked = true, callerSourceLeftOpen = true,
                fixedInputWorkspaceAcquisitionAndReleaseTimed = true, wholeInputCopy = false, streamAcquisitionAndReleaseTimed = true, residentCallbacksCompleteImmediately = true,
                sourceUnchanged = true, independentOwnedOutput = true, resourceAndCancellationGates = true, preparedEquivalentBoundaryControls = 21, preparation = proof,
                librarySha256 = JpegBenchmarkCase.HashFile(typeof(ImageCodec).Assembly.Location), benchmarkAssemblySha256 = JpegBenchmarkCase.HashFile(typeof(WarmPngHeaders).Assembly.Location),
                nativeBuild = native.BuildIdentity, nativeReady = native.Ready, nativeBefore = before, nativeAfter = after, nativeExitCode = 0,
                boundary = "Same original resident PNG through fresh read-only seekable source exposing no input view, to fresh independent nine-field typed source descriptor. Complete public InspectHeaders, fixed8448B local admission/scratch/header storage, actual bounded reads and opaque IDAT seek skipping through EOF, structural/header/EXIF scan, descriptor ownership, source/workspace release are timed. Native fresh FFmpeg AVIO with8192B scratch and original bounded header/EXIF adapter plus fresh owned AVCodecParameters; reads/seek skipping/EOF and all source/workspace/descriptor acquisition/release timed. Public PNG packet parser, CRC and compressed pixels excluded on both sides; parser streaming would buffer the whole input. Resident callbacks immediate. Same caller input left open and unchanged; descriptors survive source/workspace/peer release. No whole-input copy, metadata-block/raster allocation, source view, real I/O or cached descriptor. Startup/IPC/preparation and control hashing excluded. One CPU/thread, normal SIMD, nonconcurrent workstation GC."
            }, EncoderQualityCalibration.Json) + "\n");
        }
        finally { Dispose(); }
    }

    private void Dispose() { native?.Dispose(); state?.Dispose(); ready = false; }

    internal static void Probe(string path)
    {
        var bytes = File.ReadAllBytes(path);
        if (JpegBenchmarkCase.Hash(bytes) == InputHash)
        {
            var value = new WarmPngHeaders { encoded = bytes }; value.Validate();
        }
        using var input = new ObservedInput(bytes); ImageInfo? info = null; string? error = null;
        try { info = ImageCodec.InspectHeaders(input); }
        catch (Exception exception) when (exception is ImageDataException or ImageLimitException or UnsupportedImageException) { error = exception.GetType().Name; }
        if (!input.CanRead || input.LargestRequest > 8192 || input.TryGetBuffer(out _)
            || info is not null && (input.Position != bytes.Length || input.EofReads == 0))
            throw new InvalidOperationException("Bounded probe input/EOF/ownership differs.");
        Console.WriteLine(JsonSerializer.Serialize(new { accepted = info is not null, descriptor = info, error,
            input.Reads, input.EofReads, input.Seeks, input.BytesRead, input.LargestRequest, callerOpen = input.CanRead,
            librarySha256 = JpegBenchmarkCase.HashFile(typeof(ImageCodec).Assembly.Location) }));
    }
}
