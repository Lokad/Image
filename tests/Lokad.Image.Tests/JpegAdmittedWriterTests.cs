using System.Reflection;
using Lokad.Image.Internal;

namespace Lokad.Image.Tests;

public sealed class JpegAdmittedWriterTests
{
    private static readonly FieldInfo Bits = typeof(JpegEntropyWriter).GetField("bits", BindingFlags.NonPublic | BindingFlags.Instance)!;
    private static readonly FieldInfo Available = typeof(JpegEntropyWriter).GetField("available", BindingFlags.NonPublic | BindingFlags.Instance)!;

    [Theory]
    [InlineData(0)] [InlineData(1)] [InlineData(2)] [InlineData(3)] [InlineData(4)]
    public void AdmittedBytesPreserveOriginalWritesWorkAndFailures(int kind)
    {
        for (var pending = 0; pending < 8; pending++)
        foreach (var width in new[] { 1, 7, 8, 9, 16 })
        foreach (var capacity in new[] { 0, 1, 2, 4, 8 })
        foreach (var quota in new[] { 0, 4, 64 })
        foreach (var work in new[] { 1, 3, 16 })
        {
            var expected = Run(false); var actual = Run(true);
            Assert.Equal(expected, actual);

            string Run(bool fast)
            {
                using var cancellation = new CancellationTokenSource();
                using MemoryStream output = kind switch
                {
                    0 => new MemoryStream(),
                    1 => new MemoryStream(new byte[capacity], 0, capacity, writable: true, publiclyVisible: true),
                    2 => new MemoryStream(new byte[capacity], writable: false),
                    3 => new CallbackStream(),
                    _ => new MemoryStream()
                };
                if (kind == 1 && (pending & 1) != 0) output.Position = capacity + 2;
                using var limited = new LimitedWriteStream(output, quota, kind == 4 ? cancellation.Token : default);
                var context = new OperationContext(ImageLimits.Default with { MaxWork = work }, kind == 4 ? cancellation.Token : default);
                var writer = new JpegEntropyWriter(limited, context, fast ? 16 : 0);
                if (kind == 4 && (pending & 1) != 0) cancellation.Cancel();
                Exception? error = null;
                try
                {
                    if (fast) writer.WriteAdmitted((1 << pending) - 1, pending); else writer.Write((1 << pending) - 1, pending);
                    if (fast) writer.WriteAdmitted((1 << width) - 1, width); else writer.Write((1 << width) - 1, width);
                    if (fast) writer.RestartAdmitted(9); else writer.Restart(9);
                    if (fast) writer.WriteAdmitted(5, 3); else writer.Write(5, 3);
                    if (fast) writer.AlignAdmitted(); else writer.Align();
                }
                catch (Exception failure) { error = failure; }
                return string.Join('|', error?.GetType().FullName, error?.Message, Convert.ToHexString(output.ToArray()),
                    output.Position, output.Length, limited.Length, context.RemainingWork, Bits.GetValue(writer),
                    Available.GetValue(writer), output is CallbackStream callback ? callback.Calls : 0);
            }
        }
    }

    private sealed class CallbackStream : MemoryStream
    {
        public int Calls { get; private set; }
        public override void Write(ReadOnlySpan<byte> buffer)
        {
            if (++Calls == 2) throw new IOException("Original span callback failure.");
            base.Write(buffer);
        }
        public override void WriteByte(byte value) => throw new InvalidOperationException("Caller streams retain span callbacks.");
    }
}
