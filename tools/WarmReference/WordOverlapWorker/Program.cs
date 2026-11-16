using System.Diagnostics;
using System.Runtime;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text.Json;
using Lokad.Image;
using Lokad.Image.Tooling;

if (args.Length is < 3 or > 4) throw new ArgumentException("INPUT OUTPUT LIBRARY_SHA256 [check-only]");
const int width = 513, height = 259, copyWidth = 257, copyHeight = 129, pixelBytes = 8;
const string inputHash = "3f861b61360b442c84063e7a6e1cb4f35957dcaa16f898048beaa36bbf4272f9";
static string Hash(byte[] value) => Convert.ToHexStringLower(SHA256.HashData(value));
var input = OriginalPixels.Generate(width, height, 16);
// Match the frozen drawing raster, including hidden RGB at both row ends.
input.AsSpan(6, 2).Clear(); input.AsSpan((width * 4 - 1) * 2, 2).Clear();
if (Hash(input) != inputHash) throw new InvalidOperationException("Original RGBA16 raster differs.");
if (File.Exists(args[0]))
{
    if (!File.ReadAllBytes(args[0]).AsSpan().SequenceEqual(input)) throw new InvalidOperationException("Input delivery differs.");
}
else File.WriteAllBytes(args[0], input);
var libraryHash = Hash(File.ReadAllBytes(typeof(ImageBuffer).Assembly.Location));
if (libraryHash != args[2]) throw new InvalidOperationException("Immutable library differs.");
using var canvas = OriginalPixels.Buffer(width, height, 16);
canvas.GetRow16(0)[3] = 0; canvas.GetRow16(0)[width * 4 - 1] = 0;
var forward = Snapshot(input, 0, 0, 1, 1); var expected = Snapshot(forward, 1, 1, 0, 0);
Check();
using (var canceled = new CancellationTokenSource())
{
    canceled.Cancel();
    try { ImageDrawing.CopyRegion(canvas, 0, 0, copyWidth, copyHeight, canvas, 1, 1, cancellationToken: canceled.Token); throw new InvalidOperationException("Cancellation was ignored."); }
    catch (OperationCanceledException e) when (e.CancellationToken == canceled.Token) { }
}
try { ImageDrawing.CopyRegion(canvas, 0, 0, copyWidth, copyHeight, canvas, 1, 1, new() { MaxWorkingBytes = 1 }); throw new InvalidOperationException("Limit was ignored."); }
catch (ImageLimitException) { }
Require(expected); Check();
File.WriteAllBytes(args[1], OriginalPixels.Bytes(canvas));
if (args.Length == 4 && args[3] == "check-only")
{
    Write(new { state = "checked", exact = true, libraryHash, inputHash, fullWordSnapshotExact = true, cancellationAndRefusalRecovery = true });
    return;
}
if (!OperatingSystem.IsLinux() || Environment.ProcessorCount != 1 || GCSettings.IsServerGC
    || Environment.GetEnvironmentVariable("DOTNET_TieredCompilation") != "0"
    || !GC.GetConfigurationVariables().TryGetValue("ConcurrentGC", out var concurrent) || Convert.ToBoolean(concurrent))
    throw new InvalidOperationException("Use the quiet single-CPU optimized nonconcurrent workstation-GC boundary.");
var cpu = Thread.GetCurrentProcessorId();
Write(new { state = "ready", libraryHash, inputHash, width, height, depth = 16, copyWidth, copyHeight,
    sourceAliasesCanvas = true, borrowedCanvas = true, publicMovesPerPair = 2, cpu, fullWordSnapshotExact = true,
    cancellationAndRefusalRecovery = true, forwardHash = Hash(forward), outputHash = Hash(expected), clockReadsPerBatch = 2 });
while (Console.ReadLine() is { } line)
{
    if (line == "quit") break;
    if (line == "check") { Check(); Write(new { state = "checked", exact = true }); continue; }
    if (!line.StartsWith("sample ") || !long.TryParse(line.AsSpan(7), out var count) || count is < 1 or > 10_000_000)
        throw new ArgumentException("Invalid bounded sample request.");
    var allocated = GC.GetAllocatedBytesForCurrentThread(); var start = Stopwatch.GetTimestamp();
    for (long i = 0; i < count; i++) Apply();
    var elapsed = Stopwatch.GetTimestamp() - start; allocated = GC.GetAllocatedBytesForCurrentThread() - allocated;
    if (Thread.GetCurrentProcessorId() != cpu) throw new InvalidOperationException("CPU changed.");
    Write(new { state = "sample", operations = count, elapsedNanoseconds = elapsed * (1_000_000_000d / Stopwatch.Frequency), allocatedBytes = allocated, cpu });
}
Check();
Write(new { state = "stopped", exact = true, libraryHash, inputUnchanged = Hash(input) == inputHash });

void Apply()
{
    ImageDrawing.CopyRegion(canvas, 0, 0, copyWidth, copyHeight, canvas, 1, 1);
    ImageDrawing.CopyRegion(canvas, 1, 1, copyWidth, copyHeight, canvas, 0, 0);
}
void Check()
{
    for (var repeat = 0; repeat < 2; repeat++)
    {
        ImageDrawing.CopyRegion(canvas, 0, 0, copyWidth, copyHeight, canvas, 1, 1); Require(forward);
        ImageDrawing.CopyRegion(canvas, 1, 1, copyWidth, copyHeight, canvas, 0, 0); Require(expected);
    }
    if (Hash(input) != inputHash) throw new InvalidOperationException("Borrowed seed changed.");
}
void Require(byte[] value)
{
    for (var y = 0; y < height; y++)
        if (!MemoryMarshal.AsBytes(canvas.GetRow16(y)).SequenceEqual(value.AsSpan(y * width * pixelBytes, width * pixelBytes)))
            throw new InvalidOperationException("Full-word snapshot or untouched pixels differ.");
}
static byte[] Snapshot(byte[] source, int sx, int sy, int dx, int dy)
{
    var output = source.ToArray();
    for (var y = 0; y < copyHeight; y++)
        source.AsSpan(((sy + y) * width + sx) * pixelBytes, copyWidth * pixelBytes)
            .CopyTo(output.AsSpan(((dy + y) * width + dx) * pixelBytes));
    return output;
}
static void Write<T>(T value) => Console.WriteLine(JsonSerializer.Serialize(value));
