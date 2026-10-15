using Lokad.Image.Internal;

namespace Lokad.Image.Tests;

public sealed class JpegCodewordWriterTests
{
    [Theory]
    [InlineData(0)] [InlineData(1)] [InlineData(2)] [InlineData(3)]
    [InlineData(4)] [InlineData(5)] [InlineData(6)] [InlineData(7)]
    public void PrevalidatedCodesKeepAllPendingBitPositionsStuffingAndRestart(int pending)
    {
        for (var width = 1; width <= 16; width++)
        foreach (var value in new[] { 0, 1, (1 << width) / 2, (1 << width) - 2, (1 << width) - 1 })
            Compare(pending, width, value, 20, 20, 0);
    }

    [Theory]
    [InlineData(0)] [InlineData(1)] [InlineData(2)]
    public void WorkOutputCallbackAndCancellationFailuresKeepTheExactOriginalState(int callback)
    {
        for (var pending = 0; pending < 8; pending++)
        for (var width = 1; width <= 16; width++)
        for (var work = 1; work <= 5; work++)
        for (var quota = 0; quota <= 6; quota++)
            Compare(pending, width, (1 << width) - 1, work, quota, callback);
    }

    private static void Compare(int pending, int width, int value, int work, int quota, int callback)
    {
        var expected = Run(false); var actual = Run(true);
        Assert.Equal(expected.Error?.GetType(), actual.Error?.GetType());
        Assert.Equal(expected.Error?.Message, actual.Error?.Message);
        Assert.Equal(expected.Bytes, actual.Bytes); Assert.Equal(expected.Work, actual.Work);
        Assert.Equal(expected.Calls, actual.Calls); Assert.Equal(expected.Written, actual.Written);

        (Exception? Error, byte[] Bytes, long Work, int Calls, long Written) Run(bool packed)
        {
            using var cancellation = new CancellationTokenSource();
            using var output = new CallbackSink(cancellation, callback);
            using var limited = new LimitedWriteStream(output, quota, cancellation.Token);
            var context = new OperationContext(ImageLimits.Default with { MaxWork = work }, cancellation.Token);
            var writer = new JpegEntropyWriter(limited, context);
            var error = Record.Exception(() =>
            {
                writer.Write((1 << pending) - 1, pending);
                if (packed) writer.WriteCodeword((uint)(width << 16 | value)); else writer.Write(value, width);
                writer.Signed(-7, 3); writer.Restart(9);
            });
            return (error, output.ToArray(), context.RemainingWork, output.Calls, limited.Length);
        }
    }

    private sealed class CallbackSink(CancellationTokenSource cancellation, int mode) : MemoryStream
    {
        public int Calls { get; private set; }
        public override void Write(ReadOnlySpan<byte> buffer)
        {
            Calls++;
            if (mode == 1) throw new IOException("Original caller callback failure.");
            base.Write(buffer);
            if (mode == 2) cancellation.Cancel();
        }
        public override void WriteByte(byte value) => throw new InvalidOperationException("Span callbacks must be retained.");
    }
}
