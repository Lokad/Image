using System.Diagnostics;
using System.Numerics;
using System.Runtime;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text.Json;
using Lokad.Image;
using Lokad.Image.Tooling;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SharpImage = SixLabors.ImageSharp.Image;

if (args.Length is < 4 or > 5 || !BitConverter.IsLittleEndian)
    throw new ArgumentException("managed|reference|snapshot|probe INPUT OUTPUT CORE_SHA256 [check-only], little-endian host");
var subject = args[0];
if (subject is not ("managed" or "reference" or "snapshot" or "probe")) throw new ArgumentException("Unknown subject.");
const int width = 513, height = 259;
const string inputHash = "3f861b61360b442c84063e7a6e1cb4f35957dcaa16f898048beaa36bbf4272f9";
const string sharpHash = "609a57965d0a7b5ccfb79d919ea7384e7ec86c03266b0390a19bd5f2104bc353";
static string Hash(ReadOnlySpan<byte> value) => Convert.ToHexStringLower(SHA256.HashData(value));
var coreHash = Hash(File.ReadAllBytes(typeof(ImageBuffer).Assembly.Location));
if (coreHash != args[3] || Hash(File.ReadAllBytes(typeof(SharpImage).Assembly.Location)) != sharpHash)
    throw new InvalidOperationException("Immutable core or approved ImageSharp 3.1.12 differs.");
var input = OriginalPixels.Generate(width, height, 16);
input.AsSpan(6, 2).Clear(); input.AsSpan((width * 4 - 1) * 2, 2).Clear();
if (Hash(input) != inputHash) throw new InvalidOperationException("Original full-word input differs.");
if (File.Exists(args[1]))
{
    if (!File.ReadAllBytes(args[1]).AsSpan().SequenceEqual(input)) throw new InvalidOperationException("Input delivery differs.");
}
else File.WriteAllBytes(args[1], input);
var expected = Oracle(input);
var config = Configuration.Default.Clone(); config.MaxDegreeOfParallelism = 1;
var options = new GraphicsOptions { AlphaCompositionMode = PixelAlphaCompositionMode.SrcOver,
    ColorBlendingMode = PixelColorBlendingMode.Normal, BlendPercentage = 1 };
if (subject == "probe")
{
    using var direct = SharpImage.LoadPixelData<Rgba64>(config, input, width, height);
    using var snapshot = SharpImage.LoadPixelData<Rgba64>(config, input, width, height);
    direct.Mutate(c => c.DrawImage(direct, new Point(1, 1), options));
    using (var source = snapshot.Clone()) snapshot.Mutate(c => c.DrawImage(source, new Point(1, 1), options));
    var directBytes = SharpBytes(direct); var snapshotBytes = SharpBytes(snapshot);
    Write(new { state = "probed", inputHash, coreHash, referenceHash = sharpHash, expectedHash = Hash(expected),
        direct = Quality(directBytes), snapshot = Quality(snapshotBytes), directEqualsSnapshot = directBytes.AsSpan().SequenceEqual(snapshotBytes),
        completeReferenceSnapshotMustBeTimedIfRequired = true, timingClaim = false });
    File.WriteAllBytes(args[2], snapshotBytes); return;
}
using var managed = subject == "managed" ? OriginalPixels.Buffer(width, height, 16) : null;
using var reference = subject != "managed" ? SharpImage.LoadPixelData<Rgba64>(config, input, width, height) : null;
Memory<Rgba64> referenceMemory = default;
if (reference is not null && !reference.DangerousTryGetSinglePixelMemory(out referenceMemory))
    throw new InvalidOperationException("Require contiguous caller canvas.");
Action<IImageProcessingContext>? draw = reference is null ? null : c => c.DrawImage(reference, new Point(1, 1), options);
Reset(); Apply(); var quality = Verify();
if (managed is not null)
{
    Reset();
    using (var canceled = new CancellationTokenSource())
    {
        canceled.Cancel();
        try { ImageDrawing.Composite(managed, managed, 1, 1, cancellationToken: canceled.Token); throw new InvalidOperationException("Cancellation ignored."); }
        catch (OperationCanceledException e) when (e.CancellationToken == canceled.Token) { }
    }
    if (!Bytes().AsSpan().SequenceEqual(input)) throw new InvalidOperationException("Canceled input changed.");
    try { ImageDrawing.Composite(managed, managed, 1, 1, new() { MaxWorkingBytes = 1 }); throw new InvalidOperationException("Limit ignored."); }
    catch (ImageLimitException) { }
    if (!Bytes().AsSpan().SequenceEqual(input)) throw new InvalidOperationException("Refused input changed.");
    Apply(); Verify();
}
File.WriteAllBytes(args[2], Bytes());
if (args.Length == 5 && args[4] == "check-only")
{
    Write(new { state = "checked", subject, exactManaged = managed is not null, coreHash, inputHash,
        outputHash = Hash(Bytes()), referenceHash = sharpHash, quality, fullWordSamples = true, borrowedCanvas = true,
        sourceAliasesCanvas = true, cancellationAndRefusalRecovery = managed is not null, timingClaim = false });
    return;
}
if (!OperatingSystem.IsLinux() || Environment.ProcessorCount != 1 || GCSettings.IsServerGC
    || Environment.GetEnvironmentVariable("DOTNET_TieredCompilation") != "0"
    || !GC.GetConfigurationVariables().TryGetValue("ConcurrentGC", out var concurrent) || Convert.ToBoolean(concurrent))
    throw new InvalidOperationException("Use quiet single-CPU optimized nonconcurrent workstation GC.");
var cpu = Thread.GetCurrentProcessorId();
Write(new { state = "ready", subject, coreHash, referenceHash = sharpHash, inputHash, outputHash = Hash(Bytes()),
    width, height, depth = 16, sourceAliasesCanvas = true, borrowedCanvas = true, x = 1, y = 1, cpu,
    maximumDegreeOfParallelism = 1, resetExcludedBeforeEveryOperation = true, clocksPerOperation = 2,
    snapshotCreationAndDisposalTimed = subject == "snapshot", quality });
long completed = 0;
while (Console.ReadLine() is { } line)
{
    if (line == "quit") break;
    if (line == "check") { Reset(); Apply(); Write(new { state = "checked", quality = Verify() }); continue; }
    if (!line.StartsWith("sample ") || !int.TryParse(line.AsSpan(7), out var count) || count is < 1 or > 100_000)
        throw new ArgumentException("Invalid bounded sample request.");
    long elapsed = 0, allocated = 0;
    for (var i = 0; i < count; i++)
    {
        Reset(); var before = GC.GetAllocatedBytesForCurrentThread(); var start = Stopwatch.GetTimestamp();
        Apply();
        elapsed += Stopwatch.GetTimestamp() - start; allocated += GC.GetAllocatedBytesForCurrentThread() - before;
    }
    completed += count;
    if (Thread.GetCurrentProcessorId() != cpu) throw new InvalidOperationException("CPU changed.");
    Verify();
    Write(new { state = "sample", subject, operations = count, elapsedNanoseconds = elapsed * (1_000_000_000d / Stopwatch.Frequency),
        allocatedBytes = allocated, cpu, resetExcluded = true, clocksPerOperation = 2 });
}
managed?.Dispose(); reference?.Dispose();
Write(new { state = "stopped", completed, allCanvasOwnersDisposed = true, inputUnchanged = Hash(input) == inputHash });

void Reset()
{
    if (managed is not null)
        for (var y = 0; y < height; y++) input.AsSpan(y * managed.Stride, managed.Stride).CopyTo(MemoryMarshal.AsBytes(managed.GetRow16(y)));
    else MemoryMarshal.Cast<byte, Rgba64>(input).CopyTo(referenceMemory.Span);
}
void Apply()
{
    if (managed is not null) ImageDrawing.Composite(managed, managed, 1, 1);
    else if (subject == "snapshot")
    {
        using var source = reference!.Clone();
        reference.Mutate(c => c.DrawImage(source, new Point(1, 1), options));
    }
    else reference!.Mutate(draw!);
}
byte[] Bytes() => managed is not null ? OriginalPixels.Bytes(managed) : MemoryMarshal.AsBytes(referenceMemory.Span).ToArray();
QualityResult Verify()
{
    var pixels = Bytes(); var result = Quality(pixels);
    if (Hash(input) != inputHash || !result.Passed || (managed is not null && !pixels.AsSpan().SequenceEqual(expected)))
        throw new InvalidOperationException("Exact managed or approved native pixel/untouched gates failed: " + JsonSerializer.Serialize(result));
    if (reference is not null && (!reference.DangerousTryGetSinglePixelMemory(out var current) || !current.Equals(referenceMemory)))
        throw new InvalidOperationException("Caller canvas storage replaced.");
    return result;
}
QualityResult Quality(byte[] pixels)
{
    var actual = MemoryMarshal.Cast<byte, ushort>(pixels); var want = MemoryMarshal.Cast<byte, ushort>(expected);
    var seed = MemoryMarshal.Cast<byte, ushort>(input); int rgbMax = 0, alphaMax = 0; long rgbSquared = 0, alphaSquared = 0;
    var untouched = true; var zeroSourceAlpha = true;
    for (var y = 0; y < height; y++)
    for (var x = 0; x < width; x++)
    {
        var at = (y * width + x) * 4;
        var zero = x > 0 && y > 0 && seed[((y - 1) * width + x - 1) * 4 + 3] == 0;
        for (var channel = 0; channel < 4; channel++)
        {
            var error = Math.Abs((int)actual[at + channel] - want[at + channel]);
            if (channel == 3) { alphaMax = Math.Max(alphaMax, error); alphaSquared += (long)error * error; }
            else { rgbMax = Math.Max(rgbMax, error); rgbSquared += (long)error * error; }
            if (x == 0 || y == 0) untouched &= actual[at + channel] == seed[at + channel];
            if (zero) zeroSourceAlpha &= actual[at + channel] == seed[at + channel];
        }
    }
    var rgbRmse = Math.Sqrt(rgbSquared / (double)(width * height * 3)); var alphaRmse = Math.Sqrt(alphaSquared / (double)(width * height));
    return new(rgbMax, alphaMax, rgbRmse, alphaRmse, untouched, zeroSourceAlpha,
        rgbMax <= 1 && alphaMax <= 1 && rgbRmse <= .25 && alphaRmse <= .25 && untouched && zeroSourceAlpha);
}
static byte[] SharpBytes(SixLabors.ImageSharp.Image<Rgba64> image)
{
    if (!image.DangerousTryGetSinglePixelMemory(out var memory)) throw new InvalidOperationException("Noncontiguous reference canvas.");
    return MemoryMarshal.AsBytes(memory.Span).ToArray();
}
static byte[] Oracle(byte[] bytes)
{
    var result = bytes.ToArray(); var input = MemoryMarshal.Cast<byte, ushort>(bytes); var output = MemoryMarshal.Cast<byte, ushort>(result.AsSpan());
    for (var y = 1; y < height; y++)
    for (var x = 1; x < width; x++)
    {
        var s = ((y - 1) * width + x - 1) * 4; var d = (y * width + x) * 4;
        BigInteger sa = input[s + 3], da = input[d + 3], maximum = ushort.MaxValue;
        if (sa == 0) continue;
        var a = sa * maximum; var b = da * (maximum - sa); var coverage = a + b;
        for (var c = 0; c < 3; c++) output[d + c] = (ushort)((input[s + c] * a + input[d + c] * b + coverage / 2) / coverage);
        output[d + 3] = (ushort)((coverage + maximum / 2) / maximum);
    }
    return result;
}
static void Write<T>(T value) { Console.WriteLine(JsonSerializer.Serialize(value)); Console.Out.Flush(); }
internal sealed record QualityResult(int RgbMax, int AlphaMax, double RgbRmse, double AlphaRmse,
    bool UntouchedExact, bool ZeroSourceAlphaExact, bool Passed);
