using System.Text.Json;
using Lokad.Image;
using Lokad.Image.Tooling;

internal sealed class WarmAsyncPngDecode
{
    private const string InputHash = "4c0c823a1b84d7619f784ace6c2749ed10c0d54b7dc353beed3ad4888e0af57f";
    private JpegBenchmarkCase state = null!;
    private NativeWarmReference native = null!;
    private byte[] encoded = [], expected = [];
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
            expected = HorizontalFlipInput.Create(8);
            if (JpegBenchmarkCase.Hash(encoded) != InputHash) throw new InvalidOperationException("Original PNG packet differs.");
            using (var document = JsonDocument.Parse(File.ReadAllBytes(Path.Combine(state.Root, "tmp/h12-async-decode-controls/proof.json")))) proof = document.RootElement.Clone();
            if (proof.GetProperty("timing").GetBoolean() || proof.GetProperty("librarySha256").GetString() != JpegBenchmarkCase.HashFile(typeof(ImageCodec).Assembly.Location)
                || proof.GetProperty("inputSha256").GetString() != InputHash || proof.GetProperty("controls").GetArrayLength() != 21
                || !proof.GetProperty("equivalentNonSeekableDecodeBoundary").GetBoolean() || !proof.GetProperty("normalScalarNativeAgree").GetBoolean()
                || !proof.GetProperty("exactOriginalPixels").GetBoolean()) throw new InvalidOperationException("Nonseekable PNG decode preparation differs.");
            using (var document = JsonDocument.Parse(File.ReadAllBytes(Path.Combine(state.Root, "tmp/benchmark-source.json"))))
                sourceIdentity = JsonSerializer.SerializeToElement(new { sourceSha256 = document.RootElement.GetProperty("sourceSha256").GetString(), sourceRevision = document.RootElement.GetProperty("sourceRevision").GetString() });
            Validate(); File.WriteAllBytes(state.Input, encoded);
            var raw = Path.Combine(state.DirectoryPath, "expected.rgba"); File.WriteAllBytes(raw, expected);
            native = new NativeWarmReference(state, cpu, raw, "png-nonseekable-decode");
            if (proof.GetProperty("nativeBuild").GetProperty("workerSha256").GetString() != native.BuildIdentity.GetProperty("workerSha256").GetString())
                throw new InvalidOperationException("Prepared native decode worker differs.");
            RequireNative(); before = native.Measure(smoke, sampleCount: count); ready = true;
        }
        catch { Dispose(); throw; }
    }

    private static ImageBuffer Decode(byte[] bytes, DecodeOptions? options = null, CancellationToken cancellation = default)
    {
        using var input = new NonSeekableAsyncInput(bytes);
        try { return ImageCodec.DecodeAsync(input, options, cancellation).GetAwaiter().GetResult(); }
        finally
        {
            if (!input.CanRead || input.Disposed || input.SyncReads != 0 || input.MetadataQueries != 0)
                throw new InvalidOperationException("Async decoder used synchronous/seek/length APIs or closed caller input.");
            if (!cancellation.IsCancellationRequested && options is null && (input.BytesRead != bytes.Length || input.EofReads == 0 || input.AsyncReads == 0))
                throw new InvalidOperationException("Async decoder did not consume through EOF.");
        }
    }

    private void RequirePixels(ImageBuffer image)
    {
        if (image.Width != 513 || image.Height != 259 || image.BitDepth != 8 || image.Orientation != ImageOrientation.Normal
            || !OriginalPixels.Bytes(image).AsSpan().SequenceEqual(expected)) throw new InvalidOperationException("Async decode pixels or metadata differ.");
    }

    private void Validate()
    {
        using var first = Decode(encoded); using var second = Decode(encoded); RequirePixels(first); RequirePixels(second);
        first.GetRow8(0)[0] ^= 255; RequirePixels(second); first.Dispose(); RequirePixels(second);
        var copy = encoded.ToArray(); using var survivor = Decode(copy); Array.Clear(copy); RequirePixels(survivor);
        foreach (var limits in new[] { ImageLimits.Default with { MaxInputBytes = encoded.Length - 1 }, ImageLimits.Default with { MaxWorkingBytes = 1 } })
        {
            try { using var invalid = Decode(encoded, new DecodeOptions { Limits = limits }); throw new InvalidOperationException("Async decoder accepted insufficient admission."); }
            catch (ImageLimitException) { }
        }
        using var cancellation = new CancellationTokenSource(); cancellation.Cancel();
        try { using var invalid = Decode(encoded, cancellation: cancellation.Token); throw new InvalidOperationException("Async decoder accepted cancellation."); }
        catch (OperationCanceledException error) when (error.CancellationToken == cancellation.Token) { }
        var lateError = encoded.ToArray(); lateError[^1] ^= 1;
        try { using var invalid = Decode(lateError); throw new InvalidOperationException("Async decoder accepted invalid late CRC."); }
        catch (ImageDataException) { }
        using var recovered = Decode(encoded); RequirePixels(recovered);
        if (JpegBenchmarkCase.Hash(encoded) != InputHash) throw new InvalidOperationException("Async decode source changed.");
    }

    public int Apply()
    {
        using var input = new NonSeekableAsyncInput(encoded);
        using var result = ImageCodec.DecodeAsync(input).GetAwaiter().GetResult();
        return result.GetRow8(0)[0] + result.Stride * result.Height;
    }

    private void RequireNative()
    {
        if (!File.ReadAllBytes(state.NativeOutput).AsSpan().SequenceEqual(expected)) throw new InvalidOperationException("Native pixels differ.");
    }

    public void Cleanup()
    {
        try
        {
            if (!ready) return;
            Validate(); var after = native.Measure(smoke, before[0].Operations, count); native.Check(); RequireNative(); native.Stop();
            var path = Path.Combine(state.Root, "artifacts/benchmark-validation/WarmStreamDecode-" + Guid.NewGuid().ToString("N") + ".json"); Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            File.WriteAllText(path, JsonSerializer.Serialize(new
            {
                schemaVersion = 1, name = "WarmStreamDecode", caseId = "Png8AsyncNonSeekable", publicBoundary = "AsyncStreamDecode/PngRgba8FreshNonSeekable",
                runId = Environment.GetEnvironmentVariable("LOKAD_IMAGE_BENCHMARK_RUN_ID"), smoke, cpu, sourceIdentity, inputBytes = encoded.Length, inputSha256 = InputHash,
                width = 513, height = 259, depth = 8, orientation = "Normal", outputSha256 = JpegBenchmarkCase.Hash(expected), managedOutputBytes = expected.Length, nativeOutputBytes = expected.Length,
                allRgbaSamplesExact = true, fullStructureAndCrc = true, pixelDecode = true, freshNonSeekableStream = true, readAsyncCallsVerified = true,
                syncReads = 0, seekOrLengthQueries = 0, eofChecked = true, callerSourceLeftOpen = true, privateInputAcquisitionGrowthAndReleaseTimed = true,
                streamAcquisitionAndReleaseTimed = true, residentCallbacksCompleteImmediately = true, sourceUnchanged = true, independentOwnedOutput = true,
                resourceAndCancellationGates = true, lateFailureCleanupAndRecovery = true, preparedEquivalentBoundaryControls = 21, preparation = proof,
                librarySha256 = JpegBenchmarkCase.HashFile(typeof(ImageCodec).Assembly.Location), benchmarkAssemblySha256 = JpegBenchmarkCase.HashFile(typeof(WarmAsyncPngDecode).Assembly.Location),
                nativeBuild = native.BuildIdentity, nativeReady = native.Ready, nativeBefore = before, nativeAfter = after, nativeExitCode = 0,
                boundary = "Same resident original PNG through fresh nonseekable read-only source to complete independently owned RGBA8. Managed DecodeAsync performs actual ReadAsync through EOF, bounded private input acquisition/growth/copies/release, complete PNG structural/EXIF/CRC validation and full pixel decode; input/output disposal is timed. Native fresh nonseekable AVIO callbacks, private input acquisition/growth/copies/EOF, owned padded packet handoff without a final full-input copy, original structural/EXIF/CRC adapter, public FFmpeg PNG decode into fresh independent RGBA and all packet/AVIO/staging/pixel release are timed. Sources expose no length/seek/input view; callbacks complete immediately on both sides, with no forced scheduling or real I/O. Native codec/frame/packet wrappers and released private storage may be reused; encoded content and decoded pixels are not cached. One CPU/thread, normal SIMD, nonconcurrent workstation GC. Startup, I/O, IPC, preparation/hashing and checks excluded."
            }, EncoderQualityCalibration.Json) + "\n");
        }
        finally { Dispose(); }
    }

    private void Dispose() { native?.Dispose(); state?.Dispose(); ready = false; }

    internal static void Probe(string path)
    {
        var bytes = File.ReadAllBytes(path); using var input = new NonSeekableAsyncInput(bytes);
        ImageBuffer? image = null; string? error = null; string? outputSha256 = null;
        try { image = ImageCodec.DecodeAsync(input).GetAwaiter().GetResult(); outputSha256 = JpegBenchmarkCase.Hash(OriginalPixels.Bytes(image)); }
        catch (Exception exception) when (exception is ImageDataException or ImageLimitException or UnsupportedImageException) { error = exception.GetType().Name; }
        try
        {
            if (!input.CanRead || input.Disposed || input.SyncReads != 0 || input.MetadataQueries != 0 || input.BytesRead != bytes.Length || input.EofReads == 0)
                throw new InvalidOperationException("Async decode probe source/EOF boundary differs.");
            Console.WriteLine(JsonSerializer.Serialize(new { accepted = image is not null, width = image?.Width, height = image?.Height, depth = image?.BitDepth,
                orientation = image is null ? (int?)null : (int)image.Orientation, outputSha256, error,
                input.AsyncReads, input.EofReads, input.SyncReads, input.MetadataQueries, input.BytesRead, callerOpen = input.CanRead }));
        }
        finally { image?.Dispose(); }
    }
}
