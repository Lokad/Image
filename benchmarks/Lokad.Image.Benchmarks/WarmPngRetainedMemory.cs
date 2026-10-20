using System.Text.Json;
using Lokad.Image;
using Lokad.Image.Tooling;

internal sealed class WarmPngRetainedMemory
{
    private const string InputHash = "4c0c823a1b84d7619f784ace6c2749ed10c0d54b7dc353beed3ad4888e0af57f";
    private JpegBenchmarkCase state = null!;
    private NativeWarmReference native = null!;
    private TestMemoryBudget root = null!;
    private IMemoryReservation callerInput = null!;
    private DecodeOptions options = null!;
    private byte[] encoded = [], expected = [];
    private NativeWarmSample[] before = [];
    private JsonElement sourceIdentity;
    private int cpu, count;
    private bool smoke, ready;

    public void Setup(int cpu, bool smoke, int count)
    {
        this.cpu=cpu;this.smoke=smoke;this.count=count;state=new JpegBenchmarkCase();
        try
        {
            encoded=File.ReadAllBytes(Path.Combine(state.Root,"artifacts/warm-reference/png-cancel8/input.png"));
            expected=HorizontalFlipInput.Create(8);
            if (JpegBenchmarkCase.Hash(encoded)!=InputHash) throw new InvalidOperationException("Original PNG differs.");
            root=new TestMemoryBudget(32*1024*1024);
            if (!root.TryReserve(encoded.LongLength,out callerInput!)) throw new InvalidOperationException("Cannot reserve caller input.");
            options=new DecodeOptions {Limits=ImageLimits.Default with {MemoryBudget=root}};
            using (var document=JsonDocument.Parse(File.ReadAllBytes(Path.Combine(state.Root,"tmp/benchmark-source.json"))))
                sourceIdentity=JsonSerializer.SerializeToElement(new {sourceSha256=document.RootElement.GetProperty("sourceSha256").GetString(),sourceRevision=document.RootElement.GetProperty("sourceRevision").GetString()});
            Validate();File.WriteAllBytes(state.Input,encoded);
            var raw=Path.Combine(state.DirectoryPath,"original.rgba");File.WriteAllBytes(raw,expected);
            native=new NativeWarmReference(state,cpu,raw,PngRetainedReference.TwoOwnedOutputs);
            RequireNative();before=native.Measure(smoke,sampleCount:count);ready=true;
        }
        catch {Dispose();throw;}
    }
    private void RequirePixels(ImageBuffer image)
    {
        if ((image.Width,image.Height,image.BitDepth,image.Orientation)!=(513,259,8,ImageOrientation.Normal)
            || !OriginalPixels.Bytes(image).AsSpan().SequenceEqual(expected)) throw new InvalidOperationException("Retained PNG pixels differ.");
    }
    private void Validate()
    {
        if (root.IsDisposed || root.Bytes!=encoded.LongLength) throw new InvalidOperationException("Caller root or temporary charges differ.");
        using var first=ImageCodec.Decode(encoded,options);using var second=ImageCodec.Decode(encoded,options);
        RequirePixels(first);RequirePixels(second);
        if (root.Bytes!=encoded.LongLength+2L*expected.Length) throw new InvalidOperationException("Two retained charges differ.");
        first.GetRow8(0)[0]^=255;RequirePixels(second);first.Dispose();RequirePixels(second);
        if (root.Bytes!=encoded.LongLength+expected.Length) throw new InvalidOperationException("Survivor charge differs.");
        second.Dispose();
        root.RefuseAtGrowth=root.GrowthAttempts+1;var refusals=root.Refusals;
        try {using var invalid=ImageCodec.Decode(encoded,options);throw new InvalidOperationException("Expected terminal shared refusal.");}
        catch (ImageLimitException e) when (e.Scope==ImageLimitScope.SharedBudget && e.Limit==ImageLimitKind.WorkingBytes) {}
        finally {root.RefuseAtGrowth=0;}
        if (root.Refusals!=refusals+1 || root.Bytes!=encoded.LongLength) throw new InvalidOperationException("Refusal cleanup differs.");
        using var canceled=new CancellationTokenSource();canceled.Cancel();
        try {using var invalid=ImageCodec.Decode(encoded,options,canceled.Token);throw new InvalidOperationException("Expected cancellation.");}
        catch (OperationCanceledException e) when (e.CancellationToken==canceled.Token) {}
        using (var recovered=ImageCodec.Decode(encoded,options)) RequirePixels(recovered);
        if (root.IsDisposed || root.Bytes!=encoded.LongLength || JpegBenchmarkCase.Hash(encoded)!=InputHash) throw new InvalidOperationException("Root/source/recovery differs.");
    }
    public int Apply()
    {
        using var first=ImageCodec.Decode(encoded,options);
        using var second=ImageCodec.Decode(encoded,options);
        return first.GetRow8(0)[0]+second.GetRow8(0)[0]+first.Stride*first.Height+second.Stride*second.Height;
    }
    private void RequireNative()
    {
        if (!File.ReadAllBytes(state.NativeOutput).AsSpan().SequenceEqual(expected)) throw new InvalidOperationException("Native PNG pixels differ.");
    }
    public void Cleanup()
    {
        try
        {
            if (!ready) return;
            Validate();var after=native.Measure(smoke,before[0].Operations,count);native.Check();RequireNative();native.Stop();
            var path=Path.Combine(state.Root,"artifacts/benchmark-validation/WarmMemoryBudget-"+Guid.NewGuid().ToString("N")+".json");Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            File.WriteAllText(path,JsonSerializer.Serialize(new {
                schemaVersion=1,name="WarmMemoryBudget",caseId="Png8Retained2",publicBoundary="MemoryBudget/Png8Retained2",
                runId=Environment.GetEnvironmentVariable("LOKAD_IMAGE_BENCHMARK_RUN_ID"),smoke,cpu,sourceIdentity,
                inputBytes=encoded.Length,inputSha256=InputHash,width=513,height=259,depth=8,
                managedOutputBytes=2*expected.Length,nativeOutputBytes=2*expected.Length,eachOutputSha256=JpegBenchmarkCase.Hash(expected),
                allRgbaSamplesExact=true,sourceUnchanged=true,twoCompleteDecodesTimed=true,firstRetainedThroughSecond=true,
                independentOwnedOutputs=true,survivorAfterMutationAndRelease=true,sharedRootBorrowed=true,
                retainedChargesExact=true,temporaryChargesReturned=true,terminalRefusalCleanupAndSubsequentSuccess=true,cancellationCleanup=true,
                callerInputChargedBytes=callerInput.Bytes,rootLimitBytes=32*1024*1024,rootPeakBytes=root.Peak,
                librarySha256=JpegBenchmarkCase.HashFile(typeof(ImageCodec).Assembly.Location),benchmarkAssemblySha256=JpegBenchmarkCase.HashFile(typeof(WarmPngRetainedMemory).Assembly.Location),
                nativeBuild=native.BuildIdentity,nativeReady=native.Ready,nativeBefore=before,nativeAfter=after,nativeExitCode=0,
                boundary="Two sequential complete resident PNG8 decodes per operation; first independently owned output remains alive throughout the second, then both are disposed. Same original packet and exact RGBA samples including hidden RGB. One finite borrowed caller IMemoryBudget root and its resident-input reservation prepared outside timing. All managed admission, uncached output/scratch allocation, codec work, retained reservations and refunds are timed. Native public FFmpeg PNG decode, fresh independently owned frames and both releases timed; reusable codec/packet/frame wrappers and padded resident input prepared outside. Native has no shared admission API; managed root accounting is included additional work. No cached decoded pixels, output clones or redundant full-frame copies. One CPU/thread. Startup, I/O, IPC, fixture generation and checks excluded."
            },EncoderQualityCalibration.Json)+"\n");
        }
        finally {Dispose();}
    }
    private void Dispose() {native?.Dispose();callerInput?.Dispose();root?.Dispose();state?.Dispose();ready=false;}
}
