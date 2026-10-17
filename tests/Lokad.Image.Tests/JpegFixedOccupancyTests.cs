using System.Numerics;
using System.Reflection;
using Lokad.Image.Internal;

namespace Lokad.Image.Tests;

public sealed class JpegFixedOccupancyTests
{
    private static readonly uint[] Words = Enumerable.Range(0, 256).Select(i => (uint)(9 << 16 | i)).ToArray();
    private static readonly FieldInfo Bits = typeof(JpegEntropyWriter).GetField("bits", BindingFlags.Instance | BindingFlags.NonPublic)!;
    private static readonly FieldInfo Available = typeof(JpegEntropyWriter).GetField("available", BindingFlags.Instance | BindingFlags.NonPublic)!;

    [Fact]
    public void EveryScanPositionAndSignedExtremeKeepsTheScalarPacket()
    {
        for (var scan = 0; scan < 64; scan++)
        foreach (var value in new[] { -1023, -1, 0, 1, 1023 })
        {
            var block = new int[64]; block[JpegTransform.Zigzag[scan]] = value;
            Compare(block, 3, 256, 1024, 0);
        }
    }

    [Theory]
    [InlineData(false)] [InlineData(true)]
    public void RawMemoryStreamKeepsAllPendingBitsAndWork(bool cancelable)
    {
        using var cancellation = new CancellationTokenSource();
        for (var width = 1; width <= 16; width++)
        for (var pending = 0; pending < 8; pending++)
        foreach (var tail in new[] { false, true })
        {
            var words = Enumerable.Repeat((uint)(width << 16 | (1 << width) - 1), 256).ToArray();
            var block = Enumerable.Range(0, 64).Select(i => (i * 73 + 19) % 1023 * (i % 3 == 0 ? -1 : 1)).ToArray();
            if (tail) block[JpegTransform.Zigzag[63]] = 0;
            Assert.Equal(Run(false), Run(true));

            (string Bytes, long Work, int Predictor, uint Bits, int Available) Run(bool accelerated)
            {
                using var output = new MemoryStream();
                var context = new OperationContext(ImageLimits.Default, cancelable ? cancellation.Token : default);
                var writer = new JpegEntropyWriter(output, context); var predictor = 7;
                writer.Write((1 << pending) - 1, pending);
                if (accelerated) JpegEncoder.WriteFixedBlock(block, ref predictor, writer, words);
                else Scalar(block, ref predictor, writer, words);
                writer.Align(); writer.Restart(9);
                return (Convert.ToHexString(output.ToArray()), context.RemainingWork, predictor,
                    (uint)Bits.GetValue(writer)!, (int)Available.GetValue(writer)!);
            }
        }
    }

    [Theory]
    [InlineData(0)] [InlineData(1)] [InlineData(2)] [InlineData(3)]
    [InlineData(4)] [InlineData(5)] [InlineData(6)] [InlineData(7)]
    public void SparseDenseAndInvalidBlocksKeepWorkQuotaCallbackAndCancellationFailures(int pattern)
    {
        var block = new int[64];
        for (var scan = 0; scan < 64; scan++)
        {
            var value = pattern switch
            {
                0 => 0,
                1 => scan == 63 ? -1023 : 0,
                2 => scan % 17 == 0 ? 7 : 0,
                3 => scan % 2 == 0 ? -1023 : 1023,
                4 => scan % 3 == 0 ? (scan * 73 + 19) % 1023 + 1 : 0,
                5 => scan == 63 ? 2048 : 0,
                6 => scan == 63 ? int.MinValue : 0,
                _ => scan == 0 ? 8192 : 0
            };
            block[JpegTransform.Zigzag[scan]] = value;
        }
        foreach (var pending in new[] { 0, 3, 7 })
        foreach (var work in new[] { 1, 2, 3, 4, 7, 11, 31, 63, 128, 129, 130, 256 })
        foreach (var quota in new[] { 0, 1, 2, 7, 32, 1024 })
        for (var callback = 0; callback < 3; callback++) Compare(block, pending, work, quota, callback);
        if (pattern == 3)
            for (var work = 1; work <= 133; work++) Compare(block, 3, work, 1024, 0);
    }

    private static void Compare(int[] block, int pending, int budget, int quota, int callback)
    {
        var original = block.ToArray(); var expected = Run(false); var actual = Run(true);
        Assert.Equal(expected.Error?.GetType(), actual.Error?.GetType()); Assert.Equal(expected.Error?.Message, actual.Error?.Message);
        Assert.Equal(expected.Bytes, actual.Bytes); Assert.Equal(expected.Predictor, actual.Predictor);
        Assert.Equal(expected.Remaining, actual.Remaining); Assert.Equal(expected.Calls, actual.Calls);
        Assert.Equal(expected.Written, actual.Written); Assert.Equal(expected.Bits, actual.Bits); Assert.Equal(expected.Available, actual.Available);
        Assert.Equal(original, block);

        (Exception? Error, byte[] Bytes, int Predictor, long Remaining, int Calls, long Written, uint Bits, int Available) Run(bool accelerated)
        {
            using var cancellation = new CancellationTokenSource(); using var output = new Sink(cancellation, callback);
            using var limited = new LimitedWriteStream(output, quota, cancellation.Token);
            var context = new OperationContext(ImageLimits.Default with { MaxWork = budget }, cancellation.Token);
            var writer = new JpegEntropyWriter(limited, context); var predictor = 7;
            var error = Record.Exception(() =>
            {
                writer.Write((1 << pending) - 1, pending);
                if (accelerated) JpegEncoder.WriteFixedBlock(block, ref predictor, writer, Words);
                else Scalar(block, ref predictor, writer);
                writer.Align(); writer.Restart(9);
            });
            return (error, output.ToArray(), predictor, context.RemainingWork, output.Calls, limited.Length,
                (uint)Bits.GetValue(writer)!, (int)Available.GetValue(writer)!);
        }
    }

    // Original straightforward scalar traversal; no occupancy masks or SIMD.
    private static void Scalar(int[] block, ref int predictor, JpegEntropyWriter writer, uint[]? words = null)
    {
        words ??= Words;
        static int Category(int value) => value == 0 ? 0 : 32 - BitOperations.LeadingZeroCount((uint)Math.Abs(value));
        var difference = block[0] - predictor; predictor = block[0]; var category = Category(difference);
        if (category > 11) throw new ImageDataException("JPEG DC transform exceeds its coding range.");
        writer.WriteCodeword((uint)(4 << 16 | category)); writer.Signed(difference, category);
        var zeros = 0;
        for (var scan = 1; scan < 64; scan++)
        {
            var value = block[JpegTransform.Zigzag[scan]];
            if (value == 0) { zeros++; continue; }
            while (zeros >= 16) { writer.WriteCodeword(words[0xF0]); zeros -= 16; }
            category = Category(value);
            if (category > 10) throw new ImageDataException("JPEG AC transform exceeds its coding range.");
            writer.WriteCodeword(words[zeros << 4 | category]); writer.Signed(value, category); zeros = 0;
        }
        if (zeros != 0) writer.WriteCodeword(words[0]);
    }

    private sealed class Sink(CancellationTokenSource cancellation, int mode) : MemoryStream
    {
        public int Calls { get; private set; }
        public override void Write(ReadOnlySpan<byte> bytes)
        {
            Calls++;
            if (mode == 1) throw new IOException("Original caller callback failure.");
            base.Write(bytes); if (mode == 2) cancellation.Cancel();
        }
        public override void WriteByte(byte value) => throw new InvalidOperationException("Span callbacks must be retained.");
    }
}
