using System.Buffers.Binary;

namespace Lokad.Image.Tooling;

internal static class GovernedStreamingConsumer
{
    private const int Capacity = 65536;

    internal static async Task Verify()
    {
        foreach (var format in new[] { ImageFormat.Png, ImageFormat.Jpeg, ImageFormat.WebP })
        {
            using var root = new TestMemoryBudget(8 * 1024 * 1024);
            await RoundTrip(root, format);
            Check(root.Bytes == 0 && !root.IsDisposed, "Governed pipeline leaked or disposed its borrowed root.");
        }
        foreach (var writing in new[] { false, true })
        {
            using var root = new TestMemoryBudget(8 * 1024 * 1024);
            await PendingIo(root, writing);
            Check(root.Bytes == 0 && !root.IsDisposed, "Pending I/O retained a charge after its actual user ended.");
        }
        Console.WriteLine("Packaged governed streaming, retained output and pending-I/O admission passed.");
    }

    private static async Task RoundTrip(TestMemoryBudget root, ImageFormat format)
    {
        var limits = new ImageLimits { MemoryBudget = root };
        using var cancellation = new CancellationTokenSource(); var token = cancellation.Token;
        using var seed = Seed(limits); var original = Hash(seed);
        var packet = CallerStorage(root); using var packetOwner = packet.Owner; using var packetStream = packet.Stream;
        var output = CallerStorage(root); using var outputOwner = output.Owner; using var outputStream = output.Stream;
        await Encode(format, seed, new AsyncOutput(packetStream), limits, token);
        var bytes = packet.Storage.AsMemory(0, checked((int)packetStream.Length));
        var baseline = root.Bytes;
        using (var prefix = new AsyncInput(bytes))
        {
            var probe = await ImageCodec.ProbeAsync(prefix,
                new() { Limits = limits, Facts = ImageProbeFacts.Dimensions }, token);
            Check((probe.Width, probe.Height) == (17, 11) && probe.EstablishedFacts.HasFlag(ImageProbeFacts.Dimensions)
                && (probe.EstablishedFacts.HasFlag(ImageProbeFacts.Orientation) || probe.Orientation is null)
                && (probe.EstablishedFacts.HasFlag(ImageProbeFacts.Transparency) || probe.HasAlpha is null)
                && (probe.EstablishedFacts.HasFlag(ImageProbeFacts.SourceRepresentation) || probe.SourceColor is null)
                && (probe.EstablishedFacts.HasFlag(ImageProbeFacts.Animation) || probe.IsAnimated is null)
                && probe.BytesRead == prefix.Consumed && prefix.Consumed < bytes.Length
                && !prefix.Disposed && root.Bytes == baseline, "Governed probe exceeded its prefix or invented unexamined facts.");
        }
        using (var validation = new AsyncInput(bytes))
        {
            var info = await ImageCodec.ValidatePixelsAsync(validation, limits, token);
            Check((info.Width, info.Height, info.Format) == (17, 11, format)
                && validation.Consumed == bytes.Length && !validation.Disposed && root.Bytes == baseline,
                "Governed pixel validation did not reach EOF or released caller storage.");
        }
        using var input = new AsyncInput(bytes);
        using var decoded = await ImageCodec.DecodeAsync(input, new() { Limits = limits, ApplyOrientation = true }, token);
        Check(input.Consumed == bytes.Length && !input.Disposed && root.Bytes == baseline + PixelBytes(decoded),
            "Governed decode did not retain only its independent raster.");
        if (format != ImageFormat.Jpeg) Check(Hash(decoded) == original, "Lossless governed input pixels differ.");
        else await CallerJpegSamples(bytes, decoded, root, limits, token);
        using var resized = ImageTransforms.Resize(decoded, 9, 7, ResizeFilter.Bilinear,
            limits: limits, cancellationToken: token);
        using var sink = new AsyncOutput(outputStream);
        await Encode(format, resized, sink, limits, token);
        Check(!sink.Disposed && Hash(seed) == original && !root.IsDisposed
            && root.Bytes == 2L * Capacity + PixelBytes(seed) + PixelBytes(decoded) + PixelBytes(resized),
            "Governed stages did not share their live root or borrow their inputs.");
        seed.Dispose(); decoded.Dispose(); resized.Dispose();
        Check(root.Bytes == 2L * Capacity, "Caller encoded capacity must survive image and codec disposal.");
        using var survivingInput = new AsyncInput(output.Storage.AsMemory(0, checked((int)outputStream.Length)));
        using var surviving = await ImageCodec.DecodeAsync(survivingInput, new() { Limits = limits }, token);
        var retained = Hash(surviving); output.Storage.AsSpan().Clear();
        Check((surviving.Width, surviving.Height) == (9, 7) && Hash(surviving) == retained
            && root.Bytes == 2L * Capacity + PixelBytes(surviving), "Surviving raster aliases caller output or lost admission.");
        surviving.Dispose();
        if (format == ImageFormat.Png) await CorruptCoding(bytes, root, limits, token);
    }

    private static async Task CallerJpegSamples(ReadOnlyMemory<byte> packet, ImageBuffer reference,
        TestMemoryBudget root, ImageLimits limits, CancellationToken token)
    {
        const int stride = 17 * 3 + 5, capacity = stride * 11 + 7;
        Check(root.TryReserve(capacity, out var sampleOwner), "Caller JPEG sample admission failed.");
        using var ownedSamples = sampleOwner!;
        var samples = new byte[capacity]; samples.AsSpan().Fill(0xa5); var baseline = root.Bytes;
        var info = ImageCodec.DecodeJpeg(packet.Span, samples, JpegSampleFormat.Rgb, stride,
            new() { Limits = limits, SourceColorSpace = JpegColorSpace.YCbCr,
                CmykInversion = JpegCmykInversion.None, ApplyOrientation = false }, token);
        Check((info.Width, info.Height) == (17, 11) && root.Bytes == baseline,
            "Caller JPEG decode retained scratch or lost coded geometry.");
        for (var y = 0; y < 11; y++)
        {
            for (var x = 0; x < 17; x++) for (var c = 0; c < 3; c++)
                Check(samples[y * stride + x * 3 + c] == reference.GetRow8(y)[x * 4 + c], "Caller RGB conversion differs.");
            for (var at = y * stride + 51; at < (y + 1) * stride; at++) Check(samples[at] == 0xa5, "Caller JPEG padding changed.");
        }
        var before = ByteHash(samples);
        var output = CallerStorage(root); using var owner = output.Owner; using var stream = output.Stream;
        using var sink = new AsyncOutput(stream);
        await ImageCodec.EncodeJpegAsync(samples, 17, 11, JpegSampleFormat.Rgb, sink, stride,
            new() { Limits = limits, Quality = 77, Sampling = JpegSampling.YCbCr420 }, token);
        Check(ByteHash(samples) == before && root.Bytes == baseline + Capacity && !sink.Disposed,
            "Borrowed JPEG samples were copied into retained storage, modified or disposed.");
        Check(ImageCodec.Identify(output.Storage.AsSpan(0, checked((int)stream.Length)), limits).Width == 17,
            "Caller RGB encoding did not produce a complete JPEG.");
    }

    private static async Task CorruptCoding(Memory<byte> packet, TestMemoryBudget root, ImageLimits limits, CancellationToken token)
    {
        // RFC1951 BTYPE=3 is reserved. Repair the PNG chunk CRC so this is a
        // coding error inside a valid container, after a valid dimension prefix.
        for (var at = 8; at < packet.Length;)
        {
            var length = BinaryPrimitives.ReadInt32BigEndian(packet.Span[at..]);
            if (packet.Span.Slice(at + 4, 4).SequenceEqual("IDAT"u8))
            {
                Check(length >= 3, "Authored PNG must begin with a complete zlib/block prefix.");
                packet.Span[at + 10] |= 6;
                uint crc = uint.MaxValue;
                for (var index = at + 4; index < at + 8 + length; index++)
                {
                    crc ^= packet.Span[index];
                    for (var bit = 0; bit < 8; bit++) crc = (crc >> 1) ^ (0xedb88320u & unchecked(0u - (crc & 1)));
                }
                BinaryPrimitives.WriteUInt32BigEndian(packet.Span[(at + 8 + length)..], ~crc);
                break;
            }
            at += length + 12;
        }
        Check(ImageCodec.Identify(packet.Span, limits).Width == 17, "Corrupt-coding control must keep a valid container.");
        using var prefix = new AsyncInput(packet);
        Check((await ImageCodec.ProbeAsync(prefix, new() { Limits = limits }, token)).Width == 17,
            "Probe unexpectedly performed full pixel validation.");
        var retained = root.Bytes;
        foreach (var policy in new[] { ImageMetadataPolicy.Strict, ImageMetadataPolicy.IgnoreInvalid })
        {
            using var input = new AsyncInput(packet);
            try { await ImageCodec.ValidatePixelsAsync(input, limits, token, policy); throw new Exception("Corrupt coding was accepted."); }
            catch (ImageDataException) { }
            Check(root.Bytes == retained && !input.Disposed, "Corrupt coding leaked storage or disposed the input.");
        }
    }

    private static async Task PendingIo(TestMemoryBudget root, bool writing)
    {
        var limits = new ImageLimits { MemoryBudget = root };
        using var seed = Seed(limits);
        var packet = CallerStorage(root); using var packetOwner = packet.Owner; using var packetStream = packet.Stream;
        var output = CallerStorage(root); using var outputOwner = output.Owner; using var outputStream = output.Stream;
        ImageCodec.EncodePng(seed, packetStream, new() { Limits = limits });
        using var cancellation = new CancellationTokenSource(); var gate = new Gate();
        using var input = new AsyncInput(packet.Storage.AsMemory(0, checked((int)packetStream.Length)), writing ? null : gate);
        using var sink = new AsyncOutput(outputStream, writing ? gate : null);
        var baseline = root.Bytes;
        async Task ReadAndDispose() { using var result = await ImageCodec.DecodeAsync(input, new() { Limits = limits }, cancellation.Token); }
        var work = writing ? ImageCodec.EncodePngAsync(seed, sink, new() { Limits = limits }, cancellation.Token).AsTask() : ReadAndDispose();
        try
        {
            await gate.Entered.Task.WaitAsync(TimeSpan.FromSeconds(15));
            var charged = root.Bytes;
            Check(charged > baseline && (writing ? sink.Borrowing : input.Borrowing), "Outstanding I/O was not funded.");
            var refusals = root.Refusals; root.RefuseAtGrowth = root.GrowthAttempts + 1;
            try { using var unexpected = ImageBuffer.Create(1, 1, 8, limits); throw new Exception("Shared refusal retried or was ignored."); }
            catch (ImageLimitException e) when (e.Scope == ImageLimitScope.SharedBudget && e.Limit == ImageLimitKind.WorkingBytes) { }
            Check(root.Refusals == refusals + 1 && root.Bytes == charged && !root.IsDisposed,
                "Refusal affected an existing user's storage or disposed the borrowed root.");
            root.RefuseAtGrowth = 0; cancellation.Cancel(); await Task.Yield();
            Check(!work.IsCompleted && root.Bytes == charged && (writing ? sink.Borrowing : input.Borrowing),
                "Cancellation returned or refunded storage before the actual I/O user finished.");
        }
        finally
        {
            root.RefuseAtGrowth = 0; cancellation.Cancel(); gate.Release.TrySetResult();
            try { await work; } catch (OperationCanceledException e) when (e.CancellationToken == cancellation.Token) { }
        }
        Check(root.Bytes == baseline && !input.Disposed && !sink.Disposed, "Canceled I/O did not clean up and preserve streams.");
        using (var recovery = ImageBuffer.Create(1, 1, 8, limits))
            Check(root.Bytes == baseline + PixelBytes(recovery), "Released capacity was not reusable.");
        Check(root.Bytes == baseline, "Recovery retained a charge.");
    }

    private static ImageBuffer Seed(ImageLimits limits)
    {
        var image = ImageBuffer.Create(17, 11, 8, limits);
        for (var y = 0; y < image.Height; y++) for (var x = 0; x < image.Width; x++)
        {
            var row = image.GetRow8(y); var at = x * 4;
            row[at] = (byte)(x * 17); row[at + 1] = (byte)(y * 23); row[at + 2] = (byte)(x * 7 + y * 13); row[at + 3] = 255;
        }
        return image;
    }

    private static (byte[] Storage, IMemoryReservation Owner, MemoryStream Stream) CallerStorage(IMemoryBudget root)
    {
        Check(root.TryReserve(Capacity, out var owner), "Caller storage admission failed.");
        try
        {
            var storage = new byte[Capacity]; var stream = new MemoryStream(storage, 0, storage.Length, true, true); stream.SetLength(0);
            return (storage, owner!, stream);
        }
        catch { owner!.Dispose(); throw; }
    }

    private static ValueTask Encode(ImageFormat format, ImageBuffer image, Stream output, ImageLimits limits, CancellationToken token)
        => format switch
        {
            ImageFormat.Png => ImageCodec.EncodePngAsync(image, output, new() { Limits = limits }, token),
            ImageFormat.Jpeg => ImageCodec.EncodeJpegAsync(image, output, new() { Limits = limits, Quality = 77, Sampling = JpegSampling.YCbCr420, BackgroundRgb = 0xffffff }, token),
            ImageFormat.WebP => ImageCodec.EncodeWebpAsync(image, output, new() { Limits = limits, Lossless = true }, token),
            _ => throw new ArgumentOutOfRangeException(nameof(format))
        };

    private static ulong Hash(ImageBuffer image)
    {
        ulong value = 14695981039346656037;
        for (var y = 0; y < image.Height; y++) foreach (var sample in image.GetRow8(y)) value = unchecked((value ^ sample) * 1099511628211);
        return value;
    }
    private static long PixelBytes(ImageBuffer image) => (long)image.Stride * image.Height;
    private static ulong ByteHash(ReadOnlySpan<byte> bytes)
    {
        ulong value = 14695981039346656037;
        foreach (var sample in bytes) value = unchecked((value ^ sample) * 1099511628211);
        return value;
    }
    private static void Check(bool condition, string message) { if (!condition) throw new Exception(message); }

    private sealed class Gate
    {
        internal readonly TaskCompletionSource Entered = new(TaskCreationOptions.RunContinuationsAsynchronously);
        internal readonly TaskCompletionSource Release = new(TaskCreationOptions.RunContinuationsAsynchronously);
    }

    private sealed class AsyncInput(ReadOnlyMemory<byte> bytes, Gate? gate = null) : Stream
    {
        public int Consumed { get; private set; }
        public bool Disposed { get; private set; }
        public bool Borrowing { get; private set; }
        public override bool CanRead => !Disposed; public override bool CanSeek => false; public override bool CanWrite => false;
        public override long Length => throw new NotSupportedException(); public override long Position { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }
        public override async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken token = default)
        {
            Borrowing = true;
            try
            {
                if (gate is not null) { gate.Entered.TrySetResult(); await gate.Release.Task; }
                else await Task.Yield();
                token.ThrowIfCancellationRequested(); var count = Math.Min(3, Math.Min(buffer.Length, bytes.Length - Consumed));
                bytes.Slice(Consumed, count).CopyTo(buffer); Consumed += count; return count;
            }
            finally { Borrowing = false; }
        }
        protected override void Dispose(bool disposing) { Disposed = true; base.Dispose(disposing); }
        public override int Read(byte[] buffer, int offset, int count) => throw new Exception("Async-only input used synchronous reads.");
        public override void Flush() => throw new NotSupportedException(); public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
        public override void SetLength(long value) => throw new NotSupportedException(); public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
    }

    private sealed class AsyncOutput(Stream destination, Gate? gate = null) : Stream
    {
        public bool Disposed { get; private set; }
        public bool Borrowing { get; private set; }
        public override bool CanRead => false; public override bool CanSeek => false; public override bool CanWrite => !Disposed;
        public override long Length => throw new NotSupportedException(); public override long Position { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }
        public override async ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken token = default)
        {
            Borrowing = true;
            try
            {
                if (gate is not null) { gate.Entered.TrySetResult(); await gate.Release.Task; }
                else await Task.Yield();
                token.ThrowIfCancellationRequested(); await destination.WriteAsync(buffer, token);
            }
            finally { Borrowing = false; }
        }
        protected override void Dispose(bool disposing) { Disposed = true; base.Dispose(disposing); }
        public override void Write(byte[] buffer, int offset, int count) => throw new Exception("Async-only output used synchronous writes.");
        public override Task FlushAsync(CancellationToken token) => destination.FlushAsync(token);
        public override int Read(byte[] buffer, int offset, int count) => throw new NotSupportedException(); public override void Flush() => throw new NotSupportedException();
        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException(); public override void SetLength(long value) => throw new NotSupportedException();
    }
}
