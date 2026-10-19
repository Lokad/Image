using System.Buffers.Binary;
using System.IO.Compression;
using System.Text.Json;
using Lokad.Image;
using Lokad.Image.Tooling;

internal sealed class WarmAsyncPngEncode
{
    private const string RasterHash = "70f0fb18c341e2740b875a5a34e21ca63f0821b3386352fe2971020c5a6aac80";
    private const string PacketHash = "4c0c823a1b84d7619f784ace6c2749ed10c0d54b7dc353beed3ad4888e0af57f";
    private JpegBenchmarkCase state = null!;
    private NativeWarmReference native = null!;
    private ImageBuffer source = null!;
    private byte[] pixels = [], expected = [];
    private NativeWarmSample[] before = [];
    private JsonElement proof, sourceIdentity;
    private int cpu, count, nativeBytes, nativeIdat;
    private bool smoke, ready;

    private void Prepare()
    {
        state = new JpegBenchmarkCase(".png"); pixels = HorizontalFlipInput.Create(8);
        expected = File.ReadAllBytes(Path.Combine(state.Root, "artifacts/warm-reference/png-cancel8/input.png"));
        if (JpegBenchmarkCase.Hash(pixels) != RasterHash || JpegBenchmarkCase.Hash(expected) != PacketHash)
            throw new InvalidOperationException("Original PNG encode input/profile differs.");
        source = JpegBenchmarkCase.Buffer(pixels, 513, 259, 8);
    }

    public void Setup(int cpu, bool smoke, int count)
    {
        this.cpu = cpu; this.smoke = smoke; this.count = count;
        try
        {
            Prepare();
            using (var document = JsonDocument.Parse(File.ReadAllBytes(Path.Combine(state.Root, "tmp/h13-async-encode-controls/proof.json")))) proof = document.RootElement.Clone();
            if (proof.GetProperty("timing").GetBoolean() || proof.GetProperty("librarySha256").GetString() != JpegBenchmarkCase.HashFile(typeof(ImageCodec).Assembly.Location)
                || proof.GetProperty("inputSha256").GetString() != RasterHash || !proof.GetProperty("equivalentNonSeekableOutput").GetBoolean()
                || !proof.GetProperty("normalScalarNativeAndCliExact").GetBoolean() || !proof.GetProperty("idatSizeGate").GetBoolean())
                throw new InvalidOperationException("Nonseekable async PNG encode preparation differs.");
            using (var document = JsonDocument.Parse(File.ReadAllBytes(Path.Combine(state.Root, "tmp/benchmark-source.json"))))
                sourceIdentity = JsonSerializer.SerializeToElement(new { sourceSha256 = document.RootElement.GetProperty("sourceSha256").GetString(), sourceRevision = document.RootElement.GetProperty("sourceRevision").GetString() });
            Validate(); File.WriteAllBytes(state.Input, pixels);
            native = new NativeWarmReference(state, cpu, asyncPngEncode: true);
            if (proof.GetProperty("nativeBuild").GetProperty("workerSha256").GetString() != native.BuildIdentity.GetProperty("workerSha256").GetString())
                throw new InvalidOperationException("Prepared PNG encode worker differs.");
            RequireNative(); before = native.Measure(smoke, sampleCount: count); ready = true;
        }
        catch { Dispose(); throw; }
    }

    private void Encode(NonSeekableAsyncOutput output, PngEncodeOptions? options = null, CancellationToken cancellation = default, ImageBuffer? image = null)
    {
        try { ImageCodec.EncodePngAsync(image ?? source, output, options, cancellation).GetAwaiter().GetResult(); }
        finally
        {
            if (output.Disposed || !output.CanWrite || output.SyncWrites != 0 || output.MetadataQueries != 0 || output.AsyncFlushes != 0)
                throw new InvalidOperationException("Async encoder closed/flushed caller output or used sync/seek/length APIs.");
        }
    }

    private void RequireManaged(NonSeekableAsyncOutput output)
    {
        if (output.AsyncWrites <= 0 || output.Count != expected.Length || !output.Written.SequenceEqual(expected))
            throw new InvalidOperationException("Complete async PNG packet/delivery differs.");
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
            try { Encode(output, new PngEncodeOptions { Limits = limits }); throw new InvalidOperationException("Encoder accepted insufficient admission."); }
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
        ImageCodec.EncodePngAsync(source, output).GetAwaiter().GetResult();
        return output.Count ^ output.Written[output.Count / 2];
    }

    private void RequireNative()
    {
        var packet = File.ReadAllBytes(state.NativeOutput); nativeBytes = packet.Length; nativeIdat = IdatBytes(packet);
        if (IdatBytes(expected) > nativeIdat * 1.05) throw new InvalidOperationException("Frozen 5% IDAT-size gate failed.");
        using var decoded = ImageCodec.Decode(packet);
        if (decoded.Width != 513 || decoded.Height != 259 || decoded.BitDepth != 8 || !OriginalPixels.Bytes(decoded).AsSpan().SequenceEqual(pixels))
            throw new InvalidOperationException("Native PNG pixels differ.");
    }

    internal static int IdatBytes(byte[] packet)
    {
        if (packet.Length < 33 || packet[24] != 8 || packet[25] != 6 || packet[28] != 0) throw new InvalidOperationException("PNG profile differs.");
        using var idat = new MemoryStream();
        for (var at = 8; at < packet.Length;)
        {
            var size = checked((int)BinaryPrimitives.ReadUInt32BigEndian(packet.AsSpan(at)));
            if (packet.AsSpan(at + 4, 4).SequenceEqual("IDAT"u8)) idat.Write(packet.AsSpan(at + 8, size));
            at = checked(at + size + 12);
        }
        var length = checked((int)idat.Length); idat.Position = 0;
        using var inflate = new ZLibStream(idat, CompressionMode.Decompress); var row = new byte[513 * 4];
        for (var y = 0; y < 259; y++) { if (inflate.ReadByte() != 0) throw new InvalidOperationException("PNG filter differs."); inflate.ReadExactly(row); }
        if (inflate.ReadByte() != -1) throw new InvalidOperationException("PNG inflated length differs.");
        return length;
    }

    public void Cleanup()
    {
        try
        {
            if (!ready) return;
            Validate(); var after = native.Measure(smoke, before[0].Operations, count); native.Check(); RequireNative(); native.Stop();
            var path = Path.Combine(state.Root, "artifacts/benchmark-validation/WarmPng-" + Guid.NewGuid().ToString("N") + ".json"); Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            File.WriteAllText(path, JsonSerializer.Serialize(new
            {
                schemaVersion = 1, name = "WarmPng", caseId = "Png8AsyncNonSeekable", publicBoundary = "AsyncStreamEncode/PngRgba8FreshNonSeekable",
                runId = Environment.GetEnvironmentVariable("LOKAD_IMAGE_BENCHMARK_RUN_ID"), smoke, cpu, sourceIdentity, inputBytes = pixels.Length, inputSha256 = RasterHash,
                width = 513, height = 259, depth = 8, managedLevel = 6, nativeLevel = 6, filterNone = true,
                managedOutputBytes = expected.Length, managedOutputSha256 = PacketHash, managedIdatBytes = IdatBytes(expected), nativeOutputBytes = nativeBytes, nativeIdatBytes = nativeIdat,
                managedSnapshotExact = true, allRgbaSamplesExact = true, idatSizeGate = true, freshNonSeekableOutput = true, writeAsyncCallsVerified = true,
                syncWrites = 0, seekOrLengthQueries = 0, callerOutputLeftOpen = true, noCallerFlushRequired = true,
                completeEncodeStagingDeliveryAndReleaseTimed = true, outputAcquisitionGrowthAndReleaseTimed = true, noFinalSnapshotCopyTimed = true,
                residentCallbacksCompleteImmediately = true, sourceUnchanged = true, independentOwnedOutput = true, survivingOutputAfterSourceDisposal = true,
                resourceAndCancellationGates = true, sinkFailureCleanupAndRecovery = true, preparedEquivalentBoundaryControls = true, preparation = proof,
                librarySha256 = JpegBenchmarkCase.HashFile(typeof(ImageCodec).Assembly.Location), benchmarkAssemblySha256 = JpegBenchmarkCase.HashFile(typeof(WarmAsyncPngEncode).Assembly.Location),
                nativeBuild = native.BuildIdentity, nativeReady = native.Ready, nativeBefore = before, nativeAfter = after, nativeExitCode = 0,
                boundary = "Same resident borrowed RGBA8 to a fresh independently owned nonseekable caller output. Managed EncodePngAsync includes complete PNG encoding into admitted segmented staging, actual WriteAsync copies into caller-owned growing output, awaited completion and all staging/output release. Native single-thread FFmpeg PNG encoding into fresh owned packet staging, fresh nonseekable AVIO/scratch, output growth/callback copies, internal AVIO flush to finish writes, staging/AVIO/output release are timed. Both callbacks complete immediately; caller output remains alive after staging release and is disposed by its owner inside each complete operation. No final snapshot copy, input normalization for already-RGBA8 source, forced scheduling, real I/O, seek/length/output view or cached encoded content. Prepared source/context/wrappers and released private storage may be reused. Level6/filterNone on both sides, exact alpha/hidden RGB and frozen 5% IDAT-size gate. Native internal flush does not require managed caller FlushAsync. One CPU/thread, normal SIMD, nonconcurrent workstation GC. Startup, I/O, IPC, preparation/hashing and validation excluded."
            }, EncoderQualityCalibration.Json) + "\n");
        }
        finally { Dispose(); }
    }

    private void Dispose() { native?.Dispose(); source?.Dispose(); state?.Dispose(); ready = false; }

    internal static void Probe(string path)
    {
        var value = new WarmAsyncPngEncode();
        try
        {
            value.Prepare(); value.Validate(); using var output = new NonSeekableAsyncOutput(); value.Encode(output); value.RequireManaged(output);
            var packet = output.Snapshot(); File.WriteAllBytes(path, packet);
            Console.WriteLine(JsonSerializer.Serialize(new { librarySha256 = JpegBenchmarkCase.HashFile(typeof(ImageCodec).Assembly.Location), inputSha256 = RasterHash,
                outputSha256 = JpegBenchmarkCase.Hash(packet), outputBytes = packet.Length, idatBytes = IdatBytes(packet), managedLevel = 6,
                output.AsyncWrites, output.SyncWrites, output.MetadataQueries, output.AsyncFlushes, callerOpen = output.CanWrite, sourceUnchanged = true,
                independentRetainedOutputs = true, sourceDisposalDoesNotInvalidateOutput = true, resourceLimits = true, cancellationBeforeAndDuringDelivery = true,
                sinkFailureAndRecovery = true, immediateCallbacks = true, finalSnapshotUntimed = true }));
        }
        finally { value.Dispose(); }
    }
}
