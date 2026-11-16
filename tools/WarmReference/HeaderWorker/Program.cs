using System.Buffers.Binary;
using System.Diagnostics;
using System.Runtime;
using System.Security.Cryptography;
using System.Text.Json;
using Lokad.Image;

if (args.Length is < 4 or > 5) throw new ArgumentException("INPUT OUTPUT CORE_SHA EXPECTED_JSON [check-only|control]");
static string Hash(byte[] bytes) => Convert.ToHexStringLower(SHA256.HashData(bytes));
var input = File.ReadAllBytes(args[0]); var inputHash = Hash(input);
var coreHash = Hash(File.ReadAllBytes(typeof(ImageCodec).Assembly.Location));
if (coreHash != args[2]) throw new InvalidOperationException("Immutable library differs.");
var expected = JsonSerializer.Deserialize<ImageInfo>(args[3]) ?? throw new ArgumentException("Missing expected descriptor.");
if (args.Length == 5 && args[4] == "control")
{
    using var source = new Input(input);
    try { var value = ImageCodec.InspectHeaders(source); Write(new { accepted = true, info = value, eof = source.EofReads > 0, callerOpen = source.CanRead, timingClaim = false }); }
    catch (Exception e) when (e is ImageDataException or UnsupportedImageException or ImageLimitException)
    { Write(new { accepted = false, errorType = e.GetType().Name, callerOpen = source.CanRead, timingClaim = false }); }
    return;
}
ImageInfo Run()
{
    using var source = new Input(input);
    var value = ImageCodec.InspectHeaders(source);
    if (!source.CanRead) throw new InvalidOperationException("Library disposed caller input.");
    return value;
}
void Check()
{
    using (var source = new Input(input))
    {
        var value = ImageCodec.InspectHeaders(source);
        if (value != expected || !source.CanRead || source.Position != input.Length || source.EofReads == 0)
            throw new InvalidOperationException("Complete headers/typed fields/EOF/caller owner differ.");
    }
    var first = Run(); var second = Run();
    if (first != expected || second != expected || ReferenceEquals(first, second)) throw new InvalidOperationException("Independent descriptors differ.");
    var copy = input.ToArray(); ImageInfo held;
    using (var source = new Input(copy)) held = ImageCodec.InspectHeaders(source);
    Array.Clear(copy);
    if (held != expected || first != expected || second != expected || Hash(input) != inputHash)
        throw new InvalidOperationException("Surviving descriptor/input lifetime differs.");
}
Check();
using (var source = new Input(input))
{
    using var canceled = new CancellationTokenSource(); canceled.Cancel();
    try { ImageCodec.InspectHeaders(source, cancellationToken: canceled.Token); throw new InvalidOperationException("Cancellation ignored."); }
    catch (OperationCanceledException e) when (e.CancellationToken == canceled.Token) { }
    if (!source.CanRead || source.Position != 0) throw new InvalidOperationException("Canceled caller input changed.");
}
using (var source = new Input(input))
{
    try { ImageCodec.InspectHeaders(source, new() { MaxInputBytes = 1 }); throw new InvalidOperationException("Limit ignored."); }
    catch (ImageLimitException) { }
    if (!source.CanRead) throw new InvalidOperationException("Refused caller input was disposed.");
}
Check();
byte[] Bytes(ImageInfo value)
{
    var output = new byte[36]; uint[] fields = [(uint)value.Format, (uint)value.Width, (uint)value.Height, (uint)value.BitDepth,
        value.HasAlpha ? 1U : 0U, (uint)value.Orientation, (uint)value.SourceColor, (uint)value.SourceAlpha, value.SourceComponentCount];
    for (var i = 0; i < fields.Length; i++) BinaryPrimitives.WriteUInt32LittleEndian(output.AsSpan(i * 4), fields[i]);
    return output;
}
File.WriteAllBytes(args[1], Bytes(Run()));
if (args.Length == 5 && args[4] == "check-only")
{
    Write(new { state = "checked", coreHash, inputHash, inputBytes = input.Length, info = expected,
        exact = true, freshSeekableStream = true, inputView = false, eof = true, independentSurvivingDescriptors = true,
        callerInputLeftOpen = true, cancellationRefusalRecovery = true, timingClaim = false }); return;
}
if (!OperatingSystem.IsLinux() || Environment.ProcessorCount != 1 || GCSettings.IsServerGC
    || Environment.GetEnvironmentVariable("DOTNET_TieredCompilation") != "0"
    || !GC.GetConfigurationVariables().TryGetValue("ConcurrentGC", out var concurrent) || Convert.ToBoolean(concurrent))
    throw new InvalidOperationException("Use quiet one-CPU optimized nonconcurrent workstation GC.");
var cpu = Thread.GetCurrentProcessorId();
Write(new { state = "ready", coreHash, inputHash, inputBytes = input.Length, info = expected, cpu,
    freshSeekableStream = true, inputView = false, wholeInputCopy = false, eof = true, independentSurvivingDescriptors = true,
    sourceAcquisitionAndReleaseTimed = true, descriptorAllocationTimed = true, clocksPerBatch = 2 });
ImageInfo? last = null;
while (Console.ReadLine() is { } line)
{
    if (line == "quit") break;
    if (line == "check") { Check(); Write(new { state = "checked", exact = true }); continue; }
    if (!line.StartsWith("sample ") || !int.TryParse(line.AsSpan(7), out var count) || count is < 1 or > 1_000_000)
        throw new ArgumentException("Invalid bounded sample request.");
    var before = GC.GetAllocatedBytesForCurrentThread(); var start = Stopwatch.GetTimestamp();
    for (var i = 0; i < count; i++) last = Run();
    var elapsed = Stopwatch.GetTimestamp() - start; var allocated = GC.GetAllocatedBytesForCurrentThread() - before;
    if (last != expected || Thread.GetCurrentProcessorId() != cpu) throw new InvalidOperationException("Descriptor/CPU changed.");
    Write(new { state = "sample", operations = count, elapsedNanoseconds = elapsed * (1_000_000_000d / Stopwatch.Frequency), allocatedBytes = allocated, cpu });
}
Check();
Write(new { state = "stopped", exact = true, inputUnchanged = Hash(input) == inputHash });
static void Write<T>(T value) { Console.WriteLine(JsonSerializer.Serialize(value)); Console.Out.Flush(); }
internal sealed class Input(byte[] bytes) : Stream
{
    private int position;
    private bool closed;
    internal int EofReads;
    public override bool CanRead => !closed;
    public override bool CanSeek => !closed;
    public override bool CanWrite => false;
    public override long Length => bytes.Length;
    public override long Position { get => position; set => Seek(value, SeekOrigin.Begin); }
    public override int Read(Span<byte> destination)
    {
        ObjectDisposedException.ThrowIf(closed, this);
        var n = Math.Min(destination.Length, bytes.Length - position); bytes.AsSpan(position, n).CopyTo(destination);
        position += n; if (n == 0) EofReads++; return n;
    }
    public override int Read(byte[] buffer, int offset, int count) => Read(buffer.AsSpan(offset, count));
    public override long Seek(long offset, SeekOrigin origin)
    {
        var next = (origin == SeekOrigin.Begin ? 0L : origin == SeekOrigin.Current ? position : bytes.Length) + offset;
        if (next < 0 || next > bytes.Length) throw new IOException("Seek outside input."); position = (int)next; return next;
    }
    protected override void Dispose(bool disposing) { closed = true; base.Dispose(disposing); }
    public override void Flush() => throw new NotSupportedException();
    public override void SetLength(long value) => throw new NotSupportedException();
    public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
}
