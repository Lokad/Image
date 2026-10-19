using System.Text.Json;
using Lokad.Image;
using Lokad.Image.Tooling;

internal sealed class WarmAsyncWebpLosslessEncode
{
    private const string RasterHash = "70f0fb18c341e2740b875a5a34e21ca63f0821b3386352fe2971020c5a6aac80";
    private const string PacketHash = "a4ed2454404ca6c5798450fff0cfc502fd44a592c284b77e7861128b0c53927f";
    private JpegBenchmarkCase state = null!;
    private NativeWarmReference native = null!;
    private ImageBuffer source = null!;
    private byte[] pixels = [], expected = [];
    private NativeWarmSample[] before = [];
    private JsonElement proof, sourceIdentity;
    private int cpu, count, nativeBytes;
    private static readonly WebpEncodeOptions Options = new() { Lossless = true, Effort = 4 };
    private bool smoke, ready;

    private void Prepare()
    {
        state = new JpegBenchmarkCase(".webp"); pixels = HorizontalFlipInput.Create(8);
        source = JpegBenchmarkCase.Buffer(pixels, 513, 259, 8);
        using var packet = new MemoryStream(); ImageCodec.EncodeWebp(source, packet, Options); expected = packet.ToArray();
        if (JpegBenchmarkCase.Hash(pixels) != RasterHash || JpegBenchmarkCase.Hash(expected) != PacketHash)
            throw new InvalidOperationException("Original VP8L encode input/profile differs.");
    }

    public void Setup(int cpu, bool smoke, int count)
    {
        this.cpu = cpu; this.smoke = smoke; this.count = count;
        try
        {
            Prepare();
            using (var document = JsonDocument.Parse(File.ReadAllBytes(Path.Combine(state.Root, "tmp/h16-async-encode-controls/proof.json")))) proof = document.RootElement.Clone();
            if (proof.GetProperty("timing").GetBoolean() || proof.GetProperty("librarySha256").GetString() != JpegBenchmarkCase.HashFile(typeof(ImageCodec).Assembly.Location)
                || proof.GetProperty("inputSha256").GetString() != RasterHash || !proof.GetProperty("equivalentNonSeekableOutput").GetBoolean()
                || !proof.GetProperty("normalScalarNativeAndCliExact").GetBoolean() || !proof.GetProperty("allRgbaSamplesExact").GetBoolean())
                throw new InvalidOperationException("Nonseekable async VP8L encode preparation differs.");
            using (var document = JsonDocument.Parse(File.ReadAllBytes(Path.Combine(state.Root, "tmp/benchmark-source.json"))))
                sourceIdentity = JsonSerializer.SerializeToElement(new { sourceSha256 = document.RootElement.GetProperty("sourceSha256").GetString(), sourceRevision = document.RootElement.GetProperty("sourceRevision").GetString() });
            Validate(); File.WriteAllBytes(state.Input, pixels);
            native = new NativeWarmReference(state, cpu, Options);
            if (proof.GetProperty("nativeBuild").GetProperty("workerSha256").GetString() != native.BuildIdentity.GetProperty("workerSha256").GetString())
                throw new InvalidOperationException("Prepared VP8L encode worker differs.");
            RequireNative(); before = native.Measure(smoke, sampleCount: count); ready = true;
        }
        catch { Dispose(); throw; }
    }

    private void Encode(NonSeekableAsyncOutput output, WebpEncodeOptions? options = null, CancellationToken cancellation = default, ImageBuffer? image = null)
    {
        try { ImageCodec.EncodeWebpAsync(image ?? source, output, options ?? Options, cancellation).GetAwaiter().GetResult(); }
        finally
        {
            if (output.Disposed || !output.CanWrite || output.SyncWrites != 0 || output.MetadataQueries != 0 || output.AsyncFlushes != 0)
                throw new InvalidOperationException("Async encoder closed/flushed caller output or used sync/seek/length APIs.");
        }
    }

    private void RequireManaged(NonSeekableAsyncOutput output)
    {
        if (output.AsyncWrites <= 0 || output.Count != expected.Length || !output.Written.SequenceEqual(expected))
            throw new InvalidOperationException("Complete async VP8L packet/delivery differs.");
    }

    private void Validate()
    {
        using var first = new NonSeekableAsyncOutput(); using var second = new NonSeekableAsyncOutput();
        Encode(first); Encode(second); RequireManaged(first); RequireManaged(second);
        var survivor = second.Snapshot(); first.Dispose(); RequireManaged(second);
        using (var transient = JpegBenchmarkCase.Buffer(pixels, 513, 259, 8))
        {
            using var output = new NonSeekableAsyncOutput(); Encode(output, image: transient); transient.Dispose(); RequireManaged(output);
        }
        foreach (var limits in new[] { ImageLimits.Default with { MaxOutputBytes = 1 }, ImageLimits.Default with { MaxWorkingBytes = 1 }, ImageLimits.Default with { MaxWork = 1 } })
        {
            using var output = new NonSeekableAsyncOutput();
            try { Encode(output, new WebpEncodeOptions { Lossless = true, Effort = 4, Limits = limits }); throw new InvalidOperationException("Encoder accepted insufficient admission."); }
            catch (ImageLimitException) { }
        }
        using (var canceled = new CancellationTokenSource())
        {
            canceled.Cancel(); using var output = new NonSeekableAsyncOutput();
            try { Encode(output, cancellation: canceled.Token); throw new InvalidOperationException("Encoder accepted canceled token."); }
            catch (OperationCanceledException error) when (error.CancellationToken == canceled.Token && output.Count == 0) { }
        }
        using (var canceled = new CancellationTokenSource())
        {
            using var output = new NonSeekableAsyncOutput { AfterWrite = canceled.Cancel };
            try { Encode(output, cancellation: canceled.Token); throw new InvalidOperationException("Encoder continued after delivery cancellation."); }
            catch (OperationCanceledException error) when (error.CancellationToken == canceled.Token && output.AsyncWrites == 1 && output.Count > 0) { }
        }
        using (var failed = new NonSeekableAsyncOutput { FailOnWrite = 2 })
        {
            try { Encode(failed); throw new InvalidOperationException("Encoder ignored sink failure."); }
            catch (IOException) when (failed.AsyncWrites == 2 && failed.Count > 0) { }
        }
        using var recovered = new NonSeekableAsyncOutput(); Encode(recovered); RequireManaged(recovered);
        if (!OriginalPixels.Bytes(source).AsSpan().SequenceEqual(pixels) || !survivor.AsSpan().SequenceEqual(expected))
            throw new InvalidOperationException("Borrowed pixels or surviving encoded bytes changed.");
    }

    public int Apply()
    {
        using var output = new NonSeekableAsyncOutput();
        ImageCodec.EncodeWebpAsync(source, output, Options).GetAwaiter().GetResult();
        return output.Count ^ output.Written[output.Count / 2];
    }

    private void RequireNative()
    {
        var packet = File.ReadAllBytes(state.NativeOutput); nativeBytes = packet.Length;
        if (JpegBenchmarkCase.Hash(packet) != "08ddac115f1770d778e6dbc76373440224b32e6f93b616605e234c63206eebde")
            throw new InvalidOperationException("Frozen native exact VP8L packet differs.");
        using var decoded = ImageCodec.Decode(packet);
        if (decoded.Width != 513 || decoded.Height != 259 || decoded.BitDepth != 8 || !OriginalPixels.Bytes(decoded).AsSpan().SequenceEqual(pixels))
            throw new InvalidOperationException("Native VP8L pixels differ.");
    }

    public void Cleanup()
    {
        try
        {
            if (!ready) return;
            Validate(); var after = native.Measure(smoke, before[0].Operations, count); native.Check(); RequireNative(); native.Stop();
            var path = Path.Combine(state.Root, "artifacts/benchmark-validation/WarmWebpLossless-" + Guid.NewGuid().ToString("N") + ".json"); Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            File.WriteAllText(path, JsonSerializer.Serialize(new
            {
                schemaVersion = 1, name = "WarmWebpLossless", caseId = "WebpLossless8AsyncNonSeekable", publicBoundary = "AsyncStreamEncode/WebpLosslessRgba8FreshNonSeekable",
                runId = Environment.GetEnvironmentVariable("LOKAD_IMAGE_BENCHMARK_RUN_ID"), smoke, cpu, sourceIdentity, inputBytes = pixels.Length, inputSha256 = RasterHash,
                width = 513, height = 259, depth = 8, lossless = true, effort = 4, nativeCompressionQuality = 75, exactRgb = true, baselineBackend = "libwebp exact=1 (FFmpeg encoder backend)",
                managedOutputBytes = expected.Length, managedOutputSha256 = PacketHash, nativeOutputBytes = nativeBytes, nativeOutputSha256 = "08ddac115f1770d778e6dbc76373440224b32e6f93b616605e234c63206eebde",
                managedSnapshotExact = true, allRgbaSamplesExact = true, alphaExact = true, hiddenRgbExact = true, freshNonSeekableOutput = true, writeAsyncCallsVerified = true,
                syncWrites = 0, seekOrLengthQueries = 0, callerOutputLeftOpen = true, noCallerFlushRequired = true,
                completeEncodeStagingDeliveryAndReleaseTimed = true, outputAcquisitionGrowthAndReleaseTimed = true, noFinalSnapshotCopyTimed = true,
                residentCallbacksCompleteImmediately = true, sourceUnchanged = true, independentOwnedOutput = true, survivingOutputAfterSourceDisposal = true,
                resourceAndCancellationGates = true, sinkFailureCleanupAndRecovery = true, preparedEquivalentBoundaryControls = true, preparation = proof,
                librarySha256 = JpegBenchmarkCase.HashFile(typeof(ImageCodec).Assembly.Location), benchmarkAssemblySha256 = JpegBenchmarkCase.HashFile(typeof(WarmAsyncWebpLosslessEncode).Assembly.Location),
                nativeBuild = native.BuildIdentity, nativeReady = native.Ready, nativeBefore = before, nativeAfter = after, nativeExitCode = 0,
                boundary = "Same original borrowed resident RGBA8 to fresh independently owned nonseekable caller output. Managed EncodeWebpAsync, admission, complete exact VP8L encode through segmented staging, actual WriteAsync copies/growth, awaited completion and staging/output release are timed. Native libwebp exact=1 (FFmpeg encoder backend), lossless1/method4/quality75/thread0; full-frame RGBA-to-BGRA repack, fresh import allocation, complete encode into fresh WebPMemoryWriter staging, immediate append callbacks into fresh growing caller output, staging release before output ownership and output release are timed. Native delivers bounded64KiB chunks; its public backend needs no AVIO adapter or flush. Caller output survives staging/source/peer release. Both callbacks complete immediately, no forced scheduling/real I/O/seek/length/output view/final snapshot copy. Exact all RGBA including hidden RGB, original packets and compressed sizes checked. Prepared source/context/wrappers/BGRA workspace and released private storage may be reused; pixels/encoded content never cached. Startup/I/O/IPC/preparation/hashing/validation excluded. One CPU/thread, normal SIMD, nonconcurrent workstation GC. No image/settings grid."
            }, EncoderQualityCalibration.Json) + "\n");
        }
        finally { Dispose(); }
    }

    private void Dispose() { native?.Dispose(); source?.Dispose(); state?.Dispose(); ready = false; }

    internal static void Probe(string path)
    {
        var value = new WarmAsyncWebpLosslessEncode();
        try
        {
            value.Prepare(); value.Validate(); using var output = new NonSeekableAsyncOutput(); value.Encode(output); value.RequireManaged(output);
            var packet = output.Snapshot(); File.WriteAllBytes(path, packet);
            Console.WriteLine(JsonSerializer.Serialize(new { librarySha256 = JpegBenchmarkCase.HashFile(typeof(ImageCodec).Assembly.Location), inputSha256 = RasterHash,
                outputSha256 = JpegBenchmarkCase.Hash(packet), outputBytes = packet.Length, lossless = true, effort = 4,
                output.AsyncWrites, output.SyncWrites, output.MetadataQueries, output.AsyncFlushes, callerOpen = output.CanWrite, sourceUnchanged = true,
                independentRetainedOutputs = true, sourceDisposalDoesNotInvalidateOutput = true, resourceLimits = true, cancellationBeforeAndDuringDelivery = true,
                sinkFailureAndRecovery = true, immediateCallbacks = true, finalSnapshotUntimed = true }));
        }
        finally { value.Dispose(); }
    }
}
