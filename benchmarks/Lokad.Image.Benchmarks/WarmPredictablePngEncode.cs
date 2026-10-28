using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text.Json;
using Lokad.Image;
using Lokad.Image.Tooling;

// One required algorithmic control, outside the None-filter packet controls.
// Preparation, snapshots, hashing and independent decoding are never timed.
internal sealed class WarmPredictablePngEncode
{
    private const string RasterHash = "14c4457890991939f2798e6897275eb6c0d5dc53d88e9ede82b3657aaf2bfc95";
    private JpegBenchmarkCase state = null!;
    private NativeWarmReference native = null!;
    private ImageBuffer source = null!;
    private byte[] expected = [];
    private NativeWarmSample[] before = [];
    private JsonElement sourceIdentity, proof;
    private int cpu, count;
    private bool smoke, ready;

    public void Setup(int cpu, bool smoke, int count)
    {
        this.cpu = cpu; this.smoke = smoke; this.count = count;
        try
        {
            state = new JpegBenchmarkCase(".png");
            using (var original = OriginalPngGradient.Create()) source = ImageTransforms.FlipHorizontal(original);
            using (var document = JsonDocument.Parse(File.ReadAllBytes(Path.Combine(state.Root, "tmp/benchmark-source.json"))))
                sourceIdentity = JsonSerializer.SerializeToElement(new { sourceSha256 = document.RootElement.GetProperty("sourceSha256").GetString(), sourceRevision = document.RootElement.GetProperty("sourceRevision").GetString() });
            using (var document = JsonDocument.Parse(File.ReadAllBytes(Path.Combine(state.Root, "artifacts/warm-reference/png-predictable16/proof.json"))))
                proof = document.RootElement.Clone();
            if (proof.GetProperty("timing").GetBoolean() || proof.GetProperty("inputSha256").GetString() != RasterHash
                || proof.GetProperty("packets").GetArrayLength() != 2
                || proof.GetProperty("packets").EnumerateArray().Any(p => !p.GetProperty("everySampleExact").GetBoolean()))
                throw new InvalidOperationException("Predictable PNG normal/scalar CLI preparation differs.");
            RequireSource();
            using var output = new MemoryStream(); ImageCodec.EncodePng(source, output); expected = output.ToArray();
            RequirePacket(expected); ValidateManaged();
            using (var input = File.Create(state.Input))
                for (var y = 0; y < source.Height; y++) input.Write(MemoryMarshal.AsBytes(source.GetRow16(y)));
            native = new NativeWarmReference(state, cpu, predictablePngSize: 3000);
            RequireNative(); before = native.Measure(smoke, sampleCount: count); ready = true;
        }
        catch { Dispose(); throw; }
    }

    private void RequireSource()
    {
        if (!BitConverter.IsLittleEndian) throw new PlatformNotSupportedException("The frozen input is RGBA64LE.");
        using var hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
        for (var y = 0; y < source.Height; y++) hash.AppendData(MemoryMarshal.AsBytes(source.GetRow16(y)));
        if (Convert.ToHexStringLower(hash.GetHashAndReset()) != RasterHash) throw new InvalidOperationException("Predictable PNG source changed.");
    }

    private void RequirePacket(byte[] packet)
    {
        if (packet.Length >= 2 * 1024 * 1024) throw new InvalidOperationException("Predictable PNG size gate failed.");
        IndependentRgbaPng.Verify(packet, source);
    }

    private void ValidateManaged()
    {
        using var first = new MemoryStream(); using var second = new MemoryStream();
        ImageCodec.EncodePng(source, first); ImageCodec.EncodePng(source, second);
        if (!first.CanWrite || !second.CanWrite || !first.ToArray().AsSpan().SequenceEqual(expected))
            throw new InvalidOperationException("Predictable PNG output or caller ownership differs.");
        first.Dispose();
        if (!second.ToArray().AsSpan().SequenceEqual(expected)) throw new InvalidOperationException("Surviving predictable PNG output differs.");
        RequireSource();
    }

    private void RequireNative()
    {
        var packet = File.ReadAllBytes(state.NativeOutput); RequirePacket(packet);
        if (JpegBenchmarkCase.Hash(packet) != proof.GetProperty("packets")[0].GetProperty("sha256").GetString())
            throw new InvalidOperationException("Predictable PNG adapter differs from the frozen CLI reference.");
    }

    public int Apply()
    {
        using var output = new MemoryStream(); ImageCodec.EncodePng(source, output);
        return checked((int)output.Length) ^ output.GetBuffer()[checked((int)output.Length) / 2];
    }

    public void Cleanup()
    {
        try
        {
            if (!ready) return;
            ValidateManaged(); var after = native.Measure(smoke, before[0].Operations, count); native.Check(); RequireNative(); native.Stop();
            var packet = File.ReadAllBytes(state.NativeOutput);
            var path = Path.Combine(state.Root, "artifacts/benchmark-validation/WarmPng-" + Guid.NewGuid().ToString("N") + ".json");
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            File.WriteAllText(path, JsonSerializer.Serialize(new
            {
                schemaVersion = 1, name = "WarmPng", caseId = "EncodePredictable16", runId = Environment.GetEnvironmentVariable("LOKAD_IMAGE_BENCHMARK_RUN_ID"),
                smoke, cpu, sourceIdentity, width = 3000, height = 3000, depth = 16, inputBytes = (long)source.Stride * source.Height, inputSha256 = RasterHash,
                managedLevel = 6, nativeLevel = 6, nativePrediction = "mixed", managedOutputBytes = expected.Length, nativeOutputBytes = packet.Length,
                managedEncodedSha256 = JpegBenchmarkCase.Hash(expected), nativeOutputSha256 = JpegBenchmarkCase.Hash(packet),
                completePacketExact = expected.AsSpan().SequenceEqual(packet), completePacketSizeGate = true,
                allRgbaSamplesExact = true, alphaExact = true, hiddenRgbExact = true, nativeCliNormalScalarPixelsExact = true,
                sourceUnchanged = true, independentOwnedOutput = true, preparedBoundary = proof,
                librarySha256 = JpegBenchmarkCase.HashFile(typeof(ImageCodec).Assembly.Location), benchmarkAssemblySha256 = JpegBenchmarkCase.HashFile(typeof(WarmPng).Assembly.Location),
                nativeBuild = native.BuildIdentity, nativeReady = native.Ready, nativeBefore = before, nativeAfter = after, nativeExitCode = 0,
                boundary = "Identical borrowed resident RGBA64LE to a complete fresh independently owned PNG. Managed public EncodePng includes wire-byte packing, bounded probe, row prediction, Deflate and MemoryStream acquisition/growth/disposal; no final snapshot copy. Native pinned FFmpeg n8.1 level6/pred=mixed includes LE-to-BE conversion into a prepared frame and fresh independently owned packet acquisition/encode/release; reusable codec/frame/wrappers. One CPU/thread, optimized JIT, nonconcurrent workstation GC. Preparation, IO, IPC, hashing, correctness decoding and startup excluded. Exact pixels and <2MiB gate; packet identity is observed, not required for this new practical-compression control."
            }, EncoderQualityCalibration.Json) + "\n");
        }
        finally { Dispose(); }
    }

    private void Dispose() { native?.Dispose(); source?.Dispose(); state?.Dispose(); ready = false; }
}
