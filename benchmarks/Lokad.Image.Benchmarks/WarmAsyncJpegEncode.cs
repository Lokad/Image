using System.Buffers.Binary;
using System.IO.Compression;
using System.Text.Json;
using Lokad.Image;
using Lokad.Image.Tooling;

internal sealed class WarmAsyncJpegEncode
{
    private const string RasterHash = "70f0fb18c341e2740b875a5a34e21ca63f0821b3386352fe2971020c5a6aac80";
    private static readonly JpegEncodeOptions ProfileOptions = new() { Quality = 69, Sampling = JpegSampling.YCbCr444, BackgroundRgb = 0x204060 };
    private static readonly EncoderQualityProfile QualityProfile = new(513, 8, QualityEncoder.Jpeg444, QualityPattern.Asymmetric);
    private WarmJpegPreparation anchor = null!;
    private byte[] reference = [], nativeExpected = [];
    private string PacketHash => anchor.Match.ClosestOwned.EncodedSha256;
    private JpegBenchmarkCase state = null!;
    private NativeWarmReference native = null!;
    private ImageBuffer source = null!;
    private byte[] pixels = [], expected = [];
    private NativeWarmSample[] before = [];
    private JsonElement proof, sourceIdentity;
    private int cpu, count, nativeBytes;
    private bool smoke, ready;

    private void Prepare()
    {
        state = new JpegBenchmarkCase(); pixels = HorizontalFlipInput.Create(8);
        anchor = JsonSerializer.Deserialize<WarmJpegPreparation>(File.ReadAllBytes(Path.Combine(state.Root, "artifacts/warm-reference/jpeg-adaptive-match.json")), EncoderQualityCalibration.Json)
            ?? throw new InvalidOperationException("Frozen adaptive JPEG anchor missing.");
        expected = File.ReadAllBytes(Path.Combine(state.Root, anchor.ManagedFile)); nativeExpected = File.ReadAllBytes(Path.Combine(state.Root, anchor.NativeFile));
        reference = JpegQualityCorpus.Reference(pixels, 8, 0x204060);
        if (anchor.RasterSha256 != RasterHash || JpegBenchmarkCase.Hash(pixels) != RasterHash || JpegBenchmarkCase.Hash(reference) != anchor.ReferenceSha256
            || anchor.NativeQuality != 5 || anchor.Match.ClosestOwned.Setting != 69 || anchor.BackgroundRgb != 0x204060 || !anchor.Match.Matched
            || anchor.Match.PsnrGapDb > EncoderQualityCalibration.MaximumPsnrGapDb || JpegBenchmarkCase.Hash(expected) != PacketHash
            || JpegBenchmarkCase.Hash(nativeExpected) != anchor.Match.Native.EncodedSha256 || JpegBenchmarkCase.HashFile(state.Ffmpeg) != anchor.FfmpegSha256)
            throw new InvalidOperationException("Frozen JPEG source/quality/packet/oracle anchor differs.");
        source = JpegBenchmarkCase.Buffer(pixels, 513, 259, 8);
    }

    public void Setup(int cpu, bool smoke, int count)
    {
        this.cpu = cpu; this.smoke = smoke; this.count = count;
        try
        {
            Prepare();
            using (var document = JsonDocument.Parse(File.ReadAllBytes(Path.Combine(state.Root, "tmp/h14-async-encode-controls/proof.json")))) proof = document.RootElement.Clone();
            if (proof.GetProperty("timing").GetBoolean() || proof.GetProperty("librarySha256").GetString() != JpegBenchmarkCase.HashFile(typeof(ImageCodec).Assembly.Location)
                || proof.GetProperty("inputSha256").GetString() != RasterHash || !proof.GetProperty("equivalentNonSeekableOutput").GetBoolean()
                || !proof.GetProperty("qualityAndDecoderGatesRevalidated").GetBoolean() || !proof.GetProperty("qualityMatched").GetBoolean())
                throw new InvalidOperationException("Nonseekable async JPEG encode preparation differs.");
            using (var document = JsonDocument.Parse(File.ReadAllBytes(Path.Combine(state.Root, "tmp/benchmark-source.json"))))
                sourceIdentity = JsonSerializer.SerializeToElement(new { sourceSha256 = document.RootElement.GetProperty("sourceSha256").GetString(), sourceRevision = document.RootElement.GetProperty("sourceRevision").GetString() });
            Validate(); File.WriteAllBytes(state.Input, pixels);
            var normalized = Path.Combine(state.DirectoryPath, "normalized.rgba"); File.WriteAllBytes(normalized, reference);
            native = new NativeWarmReference(state, cpu, normalized, asyncJpegEncode: true);
            if (proof.GetProperty("nativeBuild").GetProperty("workerSha256").GetString() != native.BuildIdentity.GetProperty("workerSha256").GetString())
                throw new InvalidOperationException("Prepared JPEG encode worker differs.");
            RequireNative(); before = native.Measure(smoke, sampleCount: count); ready = true;
        }
        catch { Dispose(); throw; }
    }

    private void Encode(NonSeekableAsyncOutput output, JpegEncodeOptions? options = null, CancellationToken cancellation = default, ImageBuffer? image = null)
    {
        try { ImageCodec.EncodeJpegAsync(image ?? source, output, options ?? ProfileOptions, cancellation).GetAwaiter().GetResult(); }
        finally
        {
            if (output.Disposed || !output.CanWrite || output.SyncWrites != 0 || output.MetadataQueries != 0 || output.AsyncFlushes != 0)
                throw new InvalidOperationException("Async encoder closed/flushed caller output or used sync/seek/length APIs.");
        }
    }

    private void RequireManaged(NonSeekableAsyncOutput output)
    {
        if (output.AsyncWrites <= 0 || output.Count != expected.Length || !output.Written.SequenceEqual(expected))
            throw new InvalidOperationException("Complete async JPEG packet/delivery differs.");
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
            try { Encode(output, ProfileOptions with { Limits = limits }); throw new InvalidOperationException("Encoder accepted insufficient admission."); }
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
        ImageCodec.EncodeJpegAsync(source, output, ProfileOptions).GetAwaiter().GetResult();
        return output.Count ^ output.Written[output.Count / 2];
    }

    private void RequireNative()
    {
        var packet = File.ReadAllBytes(state.NativeOutput); nativeBytes = packet.Length;
        if (!packet.AsSpan().SequenceEqual(nativeExpected)) throw new InvalidOperationException("Native JPEG packet changed its qualified quality/size anchor.");
    }

    public void Cleanup()
    {
        try
        {
            if (!ready) return;
            Validate(); var after = native.Measure(smoke, before[0].Operations, count); native.Check(); RequireNative(); native.Stop();
            var path = Path.Combine(state.Root, "artifacts/benchmark-validation/WarmJpeg-" + Guid.NewGuid().ToString("N") + ".json"); Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            File.WriteAllText(path, JsonSerializer.Serialize(new
            {
                schemaVersion = 1, name = "WarmJpeg", caseId = "Jpeg8AsyncNonSeekable", publicBoundary = "AsyncStreamEncode/JpegRgba8FreshNonSeekable",
                runId = Environment.GetEnvironmentVariable("LOKAD_IMAGE_BENCHMARK_RUN_ID"), smoke, cpu, sourceIdentity, inputBytes = pixels.Length, inputSha256 = RasterHash,
                width = 513, height = 259, depth = 8, managedQuality = 69, nativeQuality = 5, sampling = "444", backgroundRgb = 0x204060,
                managedOutputBytes = expected.Length, managedOutputSha256 = PacketHash, nativeOutputBytes = nativeBytes, qualityMatch = anchor.Match,
                managedSnapshotExact = true, encodedPacketsExact = true, qualityMatched = true, freshNonSeekableOutput = true, writeAsyncCallsVerified = true,
                syncWrites = 0, seekOrLengthQueries = 0, callerOutputLeftOpen = true, noCallerFlushRequired = true,
                completeEncodeStagingDeliveryAndReleaseTimed = true, outputAcquisitionGrowthAndReleaseTimed = true, noFinalSnapshotCopyTimed = true,
                residentCallbacksCompleteImmediately = true, sourceUnchanged = true, independentOwnedOutput = true, survivingOutputAfterSourceDisposal = true,
                resourceAndCancellationGates = true, sinkFailureCleanupAndRecovery = true, preparedEquivalentBoundaryControls = true, preparation = proof,
                librarySha256 = JpegBenchmarkCase.HashFile(typeof(ImageCodec).Assembly.Location), benchmarkAssemblySha256 = JpegBenchmarkCase.HashFile(typeof(WarmAsyncJpegEncode).Assembly.Location),
                nativeBuild = native.BuildIdentity, nativeReady = native.Ready, nativeBefore = before, nativeAfter = after, nativeExitCode = 0,
                boundary = "Same resident borrowed RGBA8 to a fresh independently owned nonseekable caller output. Managed public EncodeJpegAsync includes stock-memory adaptive admission probe with staging capacity reserved, actual bounded contiguous staging allocation, full alpha flattening/color conversion/444 sampling/transform/adaptive Huffman/entropy encode, tail clearing, actual WriteAsync copies into growing caller storage, awaited completion and all staging/output release. Native FFmpeg includes recomputed exact RGBA alpha flattening onto0x204060, complete full-range BT601 RGBA-to-YCbCr444 conversion, optimal Huffman JPEG encode into fresh packet staging, fresh nonseekable AVIO/scratch/output growth/callback copies/internal flush/staging and caller-output release. Prepared source/context/wrappers and released private storage may be reused; no prepared normalized input or encoded content is used as a cached result. Both callbacks complete immediately, caller output survives staging release, no final snapshot copy, forced scheduling, real I/O, seek/length/output view or caller FlushAsync requirement. Frozen quality69/nativeqscale5 matched by source PSNR within0.25dB, not equal numeric quality. One CPU/thread, normal SIMD, nonconcurrent workstation GC. Startup, IPC, preparation/hashing and quality checks excluded."
            }, EncoderQualityCalibration.Json) + "\n");
        }
        finally { Dispose(); }
    }

    private void Dispose() { native?.Dispose(); source?.Dispose(); state?.Dispose(); ready = false; }

    internal static void Probe(string path)
    {
        var value = new WarmAsyncJpegEncode();
        try
        {
            value.Prepare(); value.Validate(); using var output = new NonSeekableAsyncOutput(); value.Encode(output); value.RequireManaged(output);
            var packet = output.Snapshot(); File.WriteAllBytes(path, packet);
            var owned = EncoderQualityCalibration.Validate(packet, value.reference, QualityProfile, 69, value.state.Ffmpeg);
            var native = EncoderQualityCalibration.Validate(value.nativeExpected, value.reference, QualityProfile, 5, value.state.Ffmpeg);
            var matched = EncoderQualityCalibration.Match(native, [owned]);
            if (owned != value.anchor.Match.ClosestOwned || native != value.anchor.Match.Native || !matched.Matched)
                throw new InvalidOperationException("Current JPEG decoder/source-quality anchor differs.");
            Console.WriteLine(JsonSerializer.Serialize(new { librarySha256 = JpegBenchmarkCase.HashFile(typeof(ImageCodec).Assembly.Location), inputSha256 = RasterHash,
                referenceSha256 = JpegBenchmarkCase.Hash(value.reference), outputSha256 = JpegBenchmarkCase.Hash(packet), outputBytes = packet.Length,
                nativeOutputSha256 = JpegBenchmarkCase.Hash(value.nativeExpected), nativeOutputBytes = value.nativeExpected.Length,
                qualityMatch = matched, qualityMatched = true, qualityAndDecoderGatesRevalidated = true, sourceFlatteningExact = true,
                output.AsyncWrites, output.SyncWrites, output.MetadataQueries, output.AsyncFlushes, callerOpen = output.CanWrite, sourceUnchanged = true,
                independentRetainedOutputs = true, sourceDisposalDoesNotInvalidateOutput = true, resourceLimits = true, cancellationBeforeAndDuringDelivery = true,
                sinkFailureAndRecovery = true, immediateCallbacks = true, finalSnapshotUntimed = true }));
        }
        finally { value.Dispose(); }
    }
}
