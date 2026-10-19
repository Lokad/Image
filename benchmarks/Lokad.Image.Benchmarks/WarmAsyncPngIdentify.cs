using System.Buffers.Binary;
using System.Text.Json;
using Lokad.Image;
using Lokad.Image.Tooling;

internal sealed class WarmAsyncPngIdentify
{
    private const string InputHash = "4c0c823a1b84d7619f784ace6c2749ed10c0d54b7dc353beed3ad4888e0af57f";
    private static readonly ImageInfo Expected = new(ImageFormat.Png, 513, 259, 8, true, ImageOrientation.Normal);
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
            using (var document = JsonDocument.Parse(File.ReadAllBytes(Path.Combine(state.Root, "tmp/h11-png-async-identify-controls/proof.json")))) proof = document.RootElement.Clone();
            if (proof.GetProperty("timing").GetBoolean() || proof.GetProperty("librarySha256").GetString() != JpegBenchmarkCase.HashFile(typeof(ImageCodec).Assembly.Location)
                || proof.GetProperty("inputSha256").GetString() != InputHash || !proof.GetProperty("equivalentMetadataAndValidation").GetBoolean()
                || !proof.GetProperty("nonSeekableAsyncDeliveryVerified").GetBoolean() || !proof.GetProperty("normalScalarNativeAgree").GetBoolean()
                || proof.GetProperty("controls").GetArrayLength() != 21) throw new InvalidOperationException("Nonseekable async equivalent-boundary prerequisite differs.");
            using (var document = JsonDocument.Parse(File.ReadAllBytes(Path.Combine(state.Root, "tmp/benchmark-source.json"))))
                sourceIdentity = JsonSerializer.SerializeToElement(new { sourceSha256 = document.RootElement.GetProperty("sourceSha256").GetString(), sourceRevision = document.RootElement.GetProperty("sourceRevision").GetString() });
            Validate(); File.WriteAllBytes(state.Input, encoded);
            native = new NativeWarmReference(state, cpu, Expected, stream: true, nonSeekable: true);
            if (proof.GetProperty("nativeBuild").GetProperty("workerSha256").GetString() != native.BuildIdentity.GetProperty("workerSha256").GetString())
                throw new InvalidOperationException("Prepared native worker differs.");
            RequireNative(); before = native.Measure(smoke, sampleCount: count); ready = true;
        }
        catch { Dispose(); throw; }
    }

    private ImageInfo Inspect(byte[] bytes, ImageLimits? limits = null, CancellationToken cancellation = default)
    {
        using var input = new NonSeekableAsyncInput(bytes);
        try { return ImageCodec.IdentifyAsync(input, limits, cancellation).GetAwaiter().GetResult(); }
        finally
        {
            if (!input.CanRead || input.Disposed || input.SyncReads != 0 || input.MetadataQueries != 0)
                throw new InvalidOperationException("Async reader used synchronous/seek/length APIs or closed caller input.");
            if (!cancellation.IsCancellationRequested && limits is null && (input.BytesRead != bytes.Length || input.EofReads == 0 || input.AsyncReads == 0))
                throw new InvalidOperationException("Async reader did not consume through EOF.");
        }
    }

    private void Validate()
    {
        var first = Inspect(encoded); var second = Inspect(encoded);
        if (first != Expected || second != Expected || ReferenceEquals(first, second)) throw new InvalidOperationException("Async descriptors differ or alias.");
        var copy = encoded.ToArray(); var survivor = Inspect(copy); Array.Clear(copy);
        if (survivor != Expected || first != Expected || second != Expected) throw new InvalidOperationException("Surviving metadata aliases source.");
        foreach (var limits in new[] { ImageLimits.Default with { MaxInputBytes = encoded.Length - 1 }, ImageLimits.Default with { MaxWorkingBytes = 1 }, ImageLimits.Default with { MaxWork = 1 } })
        {
            try { Inspect(encoded, limits); throw new InvalidOperationException("Async reader accepted insufficient admission."); }
            catch (ImageLimitException) { }
        }
        using var cancellation = new CancellationTokenSource(); cancellation.Cancel();
        try { Inspect(encoded, cancellation: cancellation.Token); throw new InvalidOperationException("Async reader accepted cancellation."); }
        catch (OperationCanceledException error) when (error.CancellationToken == cancellation.Token) { }
        if (JpegBenchmarkCase.Hash(encoded) != InputHash) throw new InvalidOperationException("Async source changed.");
    }

    public ImageInfo Apply()
    {
        using var input = new NonSeekableAsyncInput(encoded);
        return ImageCodec.IdentifyAsync(input).GetAwaiter().GetResult();
    }

    private void RequireNative()
    {
        var bytes = File.ReadAllBytes(state.NativeOutput);
        int[] fields = [0, 513, 259, 8, 1, 1];
        if (bytes.Length != 24) throw new InvalidOperationException("Native descriptor size differs.");
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
                schemaVersion = 1, name = "WarmStreamIdentify", caseId = "Png8AsyncNonSeekable", publicBoundary = "AsyncStreamIdentify/PngRgba8FreshNonSeekable",
                runId = Environment.GetEnvironmentVariable("LOKAD_IMAGE_BENCHMARK_RUN_ID"), smoke, cpu, sourceIdentity, inputBytes = encoded.Length, inputSha256 = InputHash,
                descriptor = Expected, managedOutputBytes = 24, nativeOutputBytes = 24, descriptorExact = true, fullStructureAndCrc = true, pixelDecode = false,
                freshNonSeekableStream = true, readAsyncCallsVerified = true, syncReads = 0, seekOrLengthQueries = 0, eofChecked = true, callerSourceLeftOpen = true,
                privateInputAcquisitionGrowthAndReleaseTimed = true, streamAcquisitionAndReleaseTimed = true, residentCallbacksCompleteImmediately = true,
                sourceUnchanged = true, independentOwnedOutput = true, resourceAndCancellationGates = true, preparedEquivalentBoundaryControls = 21, preparation = proof,
                librarySha256 = JpegBenchmarkCase.HashFile(typeof(ImageCodec).Assembly.Location), benchmarkAssemblySha256 = JpegBenchmarkCase.HashFile(typeof(WarmAsyncPngIdentify).Assembly.Location),
                nativeBuild = native.BuildIdentity, nativeReady = native.Ready, nativeBefore = before, nativeAfter = after, nativeExitCode = 0,
                boundary = "Same resident original PNG through fresh nonseekable read-only source to a fresh independently owned six-field metadata descriptor. Managed IdentifyAsync performs actual ReadAsync calls through EOF, bounded private input acquisition/growth/copies/release and complete PNG structural/EXIF/CRC validation without pixel decode. Native FFmpeg fresh nonseekable AVIO callbacks, private input acquisition/growth/copies/EOF, public PNG parser/CRC with original structural/metadata compatibility adapter, independent descriptor acquisition/release and all source/AVIO/staging release are timed. Sources expose no length/seek/input view and callbacks complete immediately on both sides; no forced scheduling or real I/O. Parser/context and released private storage may be reused; encoded content/descriptors are not cached. One CPU/thread, normal SIMD, nonconcurrent workstation GC. Startup, I/O, IPC, preparation and checks excluded."
            }, EncoderQualityCalibration.Json) + "\n");
        }
        finally { Dispose(); }
    }

    private void Dispose() { native?.Dispose(); state?.Dispose(); ready = false; }

    internal static void Probe(string path)
    {
        var bytes = File.ReadAllBytes(path); using var input = new NonSeekableAsyncInput(bytes);
        ImageInfo? info = null; string? error = null;
        try { info = ImageCodec.IdentifyAsync(input).GetAwaiter().GetResult(); }
        catch (Exception exception) when (exception is ImageDataException or ImageLimitException or UnsupportedImageException) { error = exception.GetType().Name; }
        if (!input.CanRead || input.Disposed || input.SyncReads != 0 || input.MetadataQueries != 0 || input.BytesRead != bytes.Length || input.EofReads == 0)
            throw new InvalidOperationException("Async nonseekable probe source/EOF boundary differs.");
        Console.WriteLine(JsonSerializer.Serialize(new { accepted = info is not null, descriptor = info, error, input.AsyncReads, input.EofReads, input.SyncReads, input.MetadataQueries, input.BytesRead, callerOpen = input.CanRead }));
    }
}
