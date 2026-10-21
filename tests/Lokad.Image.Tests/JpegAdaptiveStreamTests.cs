using Lokad.Image.Tooling;

namespace Lokad.Image.Tests;

public sealed class JpegAdaptiveStreamTests
{
    [Theory]
    [InlineData(0)] [InlineData(1)] [InlineData(2)] [InlineData(3)]
    public void CallbackAndLiveCancellationMatchAdaptivePacketsAtExactQuota(int input)
    {
        using var source = new Input(input); var options = source.Options with { RestartInterval = 3 };
        using var reference = new MemoryStream(); source.Encode(reference, options); var expected = reference.ToArray();
        using var token = new CancellationTokenSource();
        using var actual = new CallbackOutput(); source.Encode(actual, options, token.Token);
        Assert.Equal(expected, actual.Bytes); Assert.True(actual.CanWrite);
        using var exact = new CallbackOutput();
        source.Encode(exact, options with { Limits = new() { MaxOutputBytes = expected.Length } }, token.Token);
        Assert.Equal(expected, exact.Bytes);
        using var shortOutput = new CallbackOutput();
        var error = Assert.Throws<ImageLimitException>(() => source.Encode(shortOutput,
            options with { Limits = new() { MaxOutputBytes = expected.Length - 1 } }, token.Token));
        Assert.Equal(ImageLimitKind.OutputBytes, error.Limit);
        Assert.Equal(expected.AsSpan(0, shortOutput.Bytes.Length).ToArray(), shortOutput.Bytes);
        Assert.True(shortOutput.CanWrite); Assert.InRange(shortOutput.Bytes.Length, 1, expected.Length - 1);
        using var cancelableMemory = new MemoryStream(); source.Encode(cancelableMemory, options, token.Token);
        Assert.Equal(expected, cancelableMemory.ToArray()); source.Unchanged();
    }

    [Theory]
    [InlineData(0, false)] [InlineData(2, false)] [InlineData(3, false)]
    [InlineData(0, true)] [InlineData(2, true)] [InlineData(3, true)]
    public void CancellationAndCallerFailureDuringEntropyRefundAndAllowLaterSuccess(int input, bool fail)
    {
        using var source = new Input(input); using var root = new TestMemoryBudget(4 * 1024 * 1024);
        var options = source.Options with { Limits = new() { MemoryBudget = root } };
        using var token = new CancellationTokenSource(); var failure = new IOException("Caller output failure");
        using var output = new CallbackOutput(); var stoppedAt = 0;
        output.BeforeWrite = () =>
        {
            if (output.Count < 512) return;
            Assert.True(root.Bytes >= source.AdaptiveBytes); stoppedAt = output.Writes;
            if (fail) throw failure; token.Cancel();
        };
        if (fail) Assert.Same(failure, Assert.Throws<IOException>(() => source.Encode(output, options, token.Token)));
        else Assert.Equal(token.Token, Assert.Throws<OperationCanceledException>(() => source.Encode(output, options, token.Token)).CancellationToken);
        Assert.True(stoppedAt > 0); Assert.Equal(stoppedAt, output.Writes);
        Assert.Equal(0, root.Bytes); Assert.False(root.IsDisposed); Assert.True(output.CanWrite);
        using var next = new CallbackOutput(); source.Encode(next, options);
        Assert.True(next.Count > 512); Assert.Equal(0, root.Bytes); source.Unchanged();
    }

    [Theory]
    [InlineData(0)] [InlineData(2)] [InlineData(3)]
    public void EventAdmissionRefusalAndCancellationBeforeOutputAreTerminalAndClean(int input)
    {
        using var source = new Input(input); using var root = new TestMemoryBudget(4 * 1024 * 1024) { RefuseAtGrowth = 2 };
        var options = source.Options with { Limits = new() { MemoryBudget = root } };
        using var output = new CallbackOutput();
        var error = Assert.Throws<ImageLimitException>(() => source.Encode(output, options));
        Assert.Equal((ImageLimitKind.WorkingBytes, ImageLimitScope.SharedBudget), (error.Limit, error.Scope));
        Assert.Equal(1, root.Refusals); Assert.Equal(0, output.Count); Assert.Equal(0, root.Bytes);
        root.RefuseAtGrowth = 0; using var token = new CancellationTokenSource();
        root.Admitted = bytes => { if (bytes >= source.AdaptiveBytes) token.Cancel(); };
        Assert.Equal(token.Token, Assert.Throws<OperationCanceledException>(() => source.Encode(output, options, token.Token)).CancellationToken);
        Assert.Equal(0, output.Count); Assert.Equal(0, root.Bytes); Assert.False(root.IsDisposed);
        root.Admitted = null; source.Encode(output, options); Assert.True(output.Count > 0); Assert.Equal(0, root.Bytes);
    }

    [Theory]
    [InlineData(false)] [InlineData(true)]
    public void ReentrantCallbackSharesAdmissionAndKeepsOuterEventsAlive(bool refuse)
    {
        using var source = new Input(0); using var childSource = new Input(0);
        childSource.Image.GetRow8(0).Fill(255);
        using var root = new TestMemoryBudget(refuse ? source.AdaptiveBytes * 2 - 1 : source.AdaptiveBytes * 2);
        var options = source.Options with { Limits = new() { MemoryBudget = root } };
        using var outer = new CallbackOutput(); using var child = new CallbackOutput(); var called = false;
        outer.BeforeWrite = () =>
        {
            if (called) return; called = true; var held = root.Bytes;
            Assert.Equal(source.AdaptiveBytes, held);
            childSource.Encode(child, options); Assert.Equal(held, root.Bytes);
        };
        if (refuse)
        {
            var error = Assert.Throws<ImageLimitException>(() => source.Encode(outer, options));
            Assert.Equal(ImageLimitScope.SharedBudget, error.Scope); Assert.Equal(1, root.Refusals);
            Assert.Equal(0, child.Count); Assert.Equal(0, outer.Count);
        }
        else
        {
            source.Encode(outer, options); Assert.Equal(source.AdaptiveBytes * 2, root.Peak);
            using var expected = new MemoryStream(); source.Encode(expected, source.Options);
            Assert.Equal(expected.ToArray(), outer.Bytes);
            using var expectedChild = new MemoryStream(); childSource.Encode(expectedChild, childSource.Options);
            Assert.Equal(expectedChild.ToArray(), child.Bytes);
        }
        Assert.True(called); Assert.Equal(0, root.Bytes); Assert.False(root.IsDisposed);
        using var survivor = new CallbackOutput(); source.Encode(survivor, options);
        var heldOutput = survivor.Bytes; source.Dispose(); outer.Dispose(); child.Dispose();
        Assert.Equal(heldOutput, survivor.Bytes); Assert.Equal(0, root.Bytes); Assert.True(survivor.CanWrite);
    }

    private sealed class Input : IDisposable
    {
        internal readonly ImageBuffer Image;
        private readonly int kind;
        private readonly byte[] samples, original;
        internal JpegEncodeOptions Options => new() { Quality = 69, BackgroundRgb = 0x204060, Sampling = JpegSampling.YCbCr444 };
        internal long AdaptiveBytes => 8192L + 17 * 17 * (kind == 3 ? 1 : 3) * 64 * sizeof(uint) + 65536;
        internal Input(int kind)
        {
            this.kind = kind; Image = ImageBuffer.Create(129, 129, kind == 1 ? 16 : 8);
            samples = new byte[129 * 129 * (kind == 3 ? 1 : 3)];
            for (var y = 0; y < 129; y++)
            for (var x = 0; x < 129; x++)
            for (var c = 0; c < 4; c++)
            {
                var value = c == 3 ? (byte)255 : (byte)((x * 71 + y * 43 + c * 19 + x * y * 13) & 255);
                if (kind == 1) Image.GetRow16(y)[x * 4 + c] = (ushort)(value * 257);
                else Image.GetRow8(y)[x * 4 + c] = value;
                if (kind == 3 && c == 0) samples[y * 129 + x] = value;
                if (kind != 3 && c < 3) samples[(y * 129 + x) * 3 + c] = value;
            }
            original = samples.ToArray();
        }
        internal void Encode(Stream output, JpegEncodeOptions options, CancellationToken token = default)
        {
            if (kind < 2) ImageCodec.EncodeJpeg(Image, output, options, token);
            else ImageCodec.EncodeJpeg(samples, 129, 129, kind == 3 ? JpegSampleFormat.Grayscale : JpegSampleFormat.Rgb,
                output, options: options, cancellationToken: token);
        }
        internal void Unchanged() => Assert.Equal(original, samples);
        public void Dispose() => Image.Dispose();
    }

    private sealed class CallbackOutput : Stream
    {
        private readonly MemoryStream storage = new();
        internal Action? BeforeWrite;
        internal long Count => storage.Length;
        internal int Writes;
        internal byte[] Bytes => storage.ToArray();
        public override bool CanRead => false;
        public override bool CanSeek => false;
        public override bool CanWrite => storage.CanWrite;
        public override long Length => throw new InvalidOperationException("No output metadata queries");
        public override long Position { get => throw new InvalidOperationException("No output metadata queries"); set => throw new NotSupportedException(); }
        public override void Write(ReadOnlySpan<byte> bytes) { Writes++; BeforeWrite?.Invoke(); storage.Write(bytes); }
        public override void Write(byte[] bytes, int offset, int count) => Write(bytes.AsSpan(offset, count));
        public override void Flush() => throw new InvalidOperationException("No caller flush");
        public override int Read(byte[] bytes, int offset, int count) => throw new NotSupportedException();
        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
        public override void SetLength(long length) => throw new NotSupportedException();
        protected override void Dispose(bool disposing) { if (disposing) storage.Dispose(); base.Dispose(disposing); }
    }
}
