using Lokad.Image.Internal;

namespace Lokad.Image.Tests;

public sealed class IdentificationInputTests
{
    [Theory]
    [InlineData(0)] [InlineData(1)] [InlineData(255)] [InlineData(256)]
    [InlineData(8191)] [InlineData(8192)] [InlineData(8193)] [InlineData(218775)]
    public void CompleteAcquisitionMatchesHistoricalIdentifyPolicyAndNeverQueriesStreamMetadata(int length)
    {
        foreach (var chunk in new[] { 3, 257, 8192 }) Compare(length, chunk, ImageLimits.Default);
    }

    [Fact]
    public void InputWorkAndWorkingLimitsPreserveConsumedReadsAndFailureOrder()
    {
        foreach (var length in new[] { 33, 8193, 218775 })
        foreach (var chunk in new[] { 257, 8192 })
        {
            var capacity = 0; var count = 0; long peak = 8192;
            while (count < length)
            {
                var needed = Math.Min(length, count + chunk);
                if (needed > capacity)
                {
                    var next = Math.Max(needed, Math.Max(256, capacity * 2));
                    peak = Math.Max(peak, (long)next + capacity + 8192); capacity = next;
                }
                count = needed;
            }
            peak = Math.Max(peak, (long)capacity + length + 8192);
            foreach (var working in new[] { 8191L, peak - 1, peak })
                Compare(length, chunk, ImageLimits.Default with { MaxWorkingBytes = working });
            foreach (var maximum in new[] { 1L, length - 1, length })
            {
                Compare(length, chunk, ImageLimits.Default with { MaxInputBytes = (int)maximum });
                Compare(length, chunk, ImageLimits.Default with { MaxWork = maximum });
            }
        }
        for (var budget = 1; budget <= 67; budget++) Compare(67, 3, ImageLimits.Default with { MaxWork = budget });
    }

    [Fact]
    public void ReadFailuresAndCallbackCancellationKeepOriginalAdmissionAndToken()
    {
        Compare(218775, 8192, ImageLimits.Default, fail: true);
        Compare(218775, 8192, ImageLimits.Default, cancel: true);
        Compare(33, 3, ImageLimits.Default with { MaxInputBytes = 3 }, cancel: true);
    }

    [Fact]
    public void LiveInputLeasesAndReusedDirtyPrefixesNeverAlias()
    {
        var a = Bytes(218775, 17); var b = Bytes(218775, 83); var c = Bytes(33, 151);
        using var sa = new Reader(a, 8192); using var sb = new Reader(b, 8192);
        var exact = ImageLimits.Default with { MaxWorkingBytes = 262144 + a.Length + 8192 };
        var first = IdentificationInput.Read(sa, new(exact, default));
        using var second = IdentificationInput.Read(sb, new(exact, default));
        Assert.Equal(a, first.Span.ToArray()); Assert.Equal(b, second.Span.ToArray());
        first.Dispose(); first.Dispose();
        using var sc = new Reader(c, 8192);
        using var third = IdentificationInput.Read(sc, new(ImageLimits.Default, default));
        Assert.Equal(c, third.Span.ToArray()); Assert.Equal(b, second.Span.ToArray());
    }

    private static void Compare(int length, int chunk, ImageLimits limits, bool fail = false, bool cancel = false)
    {
        var data = Bytes(length, 31); var original = Run(data, chunk, limits, fast: false, fail, cancel);
        var current = Run(data, chunk, limits, fast: true, fail, cancel);
        Assert.Equal(original.Bytes, current.Bytes);
        Assert.Equal(original.Error, current.Error);
        Assert.Equal(original.RemainingWork, current.RemainingWork);
        Assert.Equal(original.RemainingWorking, current.RemainingWorking);
        Assert.Equal(original.Reads, current.Reads); Assert.Equal(original.Consumed, current.Consumed);
        Assert.Equal(original.CanceledWithExpectedToken, current.CanceledWithExpectedToken);
        Assert.Equal(Bytes(length, 31), data);
    }
    private sealed record Outcome(byte[]? Bytes, Type? Error, long RemainingWork, long RemainingWorking, int Reads, int Consumed, bool CanceledWithExpectedToken);
    private static Outcome Run(byte[] bytes, int chunk, ImageLimits limits, bool fast, bool fail, bool cancel)
    {
        using var cts = new CancellationTokenSource(); using var reader = new Reader(bytes, chunk, fail, cancel ? cts : null);
        var context = new OperationContext(limits, cts.Token); byte[]? result = null; Exception? error = null;
        try
        {
            if (fast) { using var input = IdentificationInput.Read(reader, context); result = input.Span.ToArray(); }
            else result = LegacyInput(reader, context);
        }
        catch (Exception e) { error = e; }
        Assert.True(reader.CanRead);
        return new(result, error?.GetType(), context.RemainingWork, context.RemainingWorkingBytes, reader.Reads, reader.Consumed,
            error is OperationCanceledException canceled && canceled.CancellationToken == cts.Token);
    }
    // Freeze our former reader as the historical synchronous Identify admission
    // oracle. Decode now deliberately uses actual-capacity admission instead.
    private static byte[] LegacyInput(Stream input, OperationContext context)
    {
        ArgumentNullException.ThrowIfNull(input);
        var limits = context.Limits;
        // Bounded growth also covers nonseekable streams. The exact-sized final copy
        // and buffer capacity must both fit the working-memory allowance.
        using var buffer = new MemoryStream();
        context.Reserve(8192);
        var scratch = new byte[8192];
        while (true)
        {
            context.Cancellation.ThrowIfCancellationRequested();
            var read = input.Read(scratch);
            if (read == 0) break;
            if (read > limits.MaxInputBytes - buffer.Length) throw new ImageLimitException(ImageLimitKind.InputBytes, "Encoded input limit exceeded.");
            var needed = buffer.Length + read;
            var capacity = Math.Max(needed, Math.Max(256, buffer.Capacity * 2L));
            if (needed > buffer.Capacity)
            {
                if (capacity + buffer.Capacity + scratch.Length > limits.MaxWorkingBytes || capacity > Array.MaxLength)
                    throw new ImageLimitException(capacity + buffer.Capacity + scratch.Length > limits.MaxWorkingBytes
                        ? ImageLimitKind.WorkingBytes : ImageLimitKind.BufferLength, "Stream buffering memory limit exceeded.");
                buffer.Capacity = (int)capacity;
            }
            buffer.Write(scratch.AsSpan(0, read));
            context.Work(read);
        }
        if (buffer.Capacity + buffer.Length + scratch.Length > limits.MaxWorkingBytes)
            throw new ImageLimitException(ImageLimitKind.WorkingBytes, "Stream buffering memory limit exceeded.");
        var result = buffer.ToArray();
        context.Release(scratch.Length);
        context.Reserve(result.Length);
        return result;
    }
    private static byte[] Bytes(int length, int seed) => Enumerable.Range(0, length).Select(i => (byte)(i * 67 + seed)).ToArray();
    private sealed class Reader(byte[] bytes, int chunk, bool fail = false, CancellationTokenSource? cancel = null) : Stream
    {
        public int Reads { get; private set; }
        public int Consumed { get; private set; }
        private bool disposed;
        public override int Read(Span<byte> output)
        {
            Reads++;
            if (Reads == 2 && fail) throw new IOException("Authored read failure.");
            var count = Math.Min(Math.Min(chunk, output.Length), bytes.Length - Consumed);
            bytes.AsSpan(Consumed, count).CopyTo(output); Consumed += count;
            if (Reads == 2) cancel?.Cancel();
            return count;
        }
        public override int Read(byte[] output, int offset, int count) => Read(output.AsSpan(offset, count));
        public override bool CanRead => !disposed;
        public override bool CanSeek => false;
        public override bool CanWrite => false;
        public override long Length => throw new InvalidOperationException("Length must not be queried.");
        public override long Position { get => throw new InvalidOperationException("Position must not be queried."); set => throw new NotSupportedException(); }
        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
        public override void SetLength(long value) => throw new NotSupportedException();
        public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
        public override void Flush() => throw new NotSupportedException();
        protected override void Dispose(bool disposing) { disposed = true; base.Dispose(disposing); }
    }
}
