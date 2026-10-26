using Lokad.Image.Internal;

namespace Lokad.Image.Tests;

public sealed class JpegCancellationTests
{
    [Fact]
    public void CanceledAcLookaheadReturnsEveryPrefetchedByteBeforePrefixAdmission()
    {
        var lengths = new byte[16]; lengths[0] = 1;
        var table = new JpegHuffman(lengths, new byte[] { 0x01 }, new OperationContext(ImageLimits.Default, default));
        for (var phase = 0; phase < 8; phase++)
        {
            using var cancellation = new CancellationTokenSource();
            var context = new OperationContext(ImageLimits.Default, cancellation.Token);
            var reader = new JpegEntropyReader(new byte[] { 0, 0, 0, 0, 0, 0xFF, 0xD0 }, 0, context);
            if (phase != 0) reader.ReadBits(phase);
            cancellation.Cancel();
            var block = Enumerable.Repeat(12345, 64).ToArray(); var coefficient = 1;
            try { table.ReadPlainCancelableAc(ref reader, block, ref coefficient); Assert.Fail("Canceled lookahead must not admit the prefix."); }
            catch (OperationCanceledException error) { Assert.Equal(cancellation.Token, error.CancellationToken); }
            Assert.Equal(phase == 0 ? 0 : 1, reader.Position); Assert.Equal(1, coefficient);
            Assert.All(block, value => Assert.Equal(12345, value));
        }
    }

    [Theory]
    [InlineData(9, true)] [InlineData(10, true)] [InlineData(9, false)] [InlineData(10, false)]
    public void LongAcCodesKeepEveryCachedPhaseAndTruncation(int codeLength, bool cancelable)
    {
        var lengths = new byte[16]; lengths[codeLength - 1] = 1;
        for (var size = 1; size <= 10; size++)
        {
            var table = new JpegHuffman(lengths, new byte[] { (byte)size }, new OperationContext(ImageLimits.Default, default));
            for (var offset = 0; offset < 8; offset++)
            foreach (var positive in new[] { false, true })
            {
                // The only canonical code is zero at the requested length.
                // Original amplitude endpoints, tail and all-one padding.
                var text = new string('1', offset) + new string('0', codeLength)
                    + new string(positive ? '1' : '0', size) + "101";
                text = text.PadRight((text.Length + 7) / 8 * 8, '1');
                var bytes = new List<byte>();
                for (var index = 0; index < text.Length; index += 8)
                {
                    var next = Convert.ToByte(text.Substring(index, 8), 2); bytes.Add(next);
                    if (next == 255) bytes.Add(0);
                }
                bytes.AddRange([255, 0xD0]); var full = bytes.ToArray();
                for (var length = 0; length <= full.Length; length++)
                {
                    var input = full.AsSpan(0, length).ToArray();
                    Assert.Equal(Run(false), Run(true));

                    (Type? Error, string? Message, int Position, int Value, int Tail) Run(bool fused)
                    {
                        using var cancellation = new CancellationTokenSource();
                        var context = new OperationContext(ImageLimits.Default, cancelable ? cancellation.Token : default);
                        var reader = new JpegEntropyReader(input, 0, context); var value = 0; var tail = 0; Exception? error = null;
                        try
                        {
                            if (offset != 0) reader.ReadBits(offset);
                            var coefficient = 63; Span<int> block = stackalloc int[64];
                            if (fused)
                            {
                                if (cancelable) table.ReadPlainCancelableAc(ref reader, block, ref coefficient);
                                else table.ReadPlainBaselineAc(ref reader, block, ref coefficient);
                            }
                            if (fused && coefficient == 64) value = block[JpegTransform.Zigzag[63]];
                            else
                            {
                                var symbol = table.Read(ref reader); value = reader.ReadSigned(symbol & 15);
                            }
                            tail = reader.ReadBits(3); reader.Restart(0);
                        }
                        catch (Exception failure) when (failure is ImageDataException or ImageLimitException) { error = failure; }
                        return (error?.GetType(), error?.Message, reader.Position, value, tail);
                    }
                }
            }
        }
    }

    [Theory]
    [InlineData(true)] [InlineData(false)]
    public void EveryRawBytePairKeepsUnfusedStuffingMarkerAndPendingBitState(bool cancelable)
    {
        var lengths = new byte[16]; lengths[0] = 1;
        var table = new JpegHuffman(lengths, new byte[] { 0x01 }, new OperationContext(ImageLimits.Default, default));
        byte[] data = [0, 0, 0, 0xFF, 0xD0];
        for (var pair = 0; pair <= ushort.MaxValue; pair++)
        {
            data[1] = (byte)(pair >> 8); data[2] = (byte)pair;
            var reference = Run(false); var actual = Run(true);
            Assert.Equal(reference, actual);
        }

        (Type? Error, string? Message, int Position, int Value, int Tail) Run(bool fused)
        {
            using var cancellation = new CancellationTokenSource();
            var context = new OperationContext(ImageLimits.Default, cancelable ? cancellation.Token : default);
            var reader = new JpegEntropyReader(data, 0, context); var value = 0; var tail = 0; Exception? error = null;
            try
            {
                reader.ReadBits(7); // One cached zero bit is the one-bit AC code.
                var coefficient = 63; Span<int> block = stackalloc int[64];
                if (fused)
                {
                    if (cancelable) table.ReadPlainCancelableAc(ref reader, block, ref coefficient);
                    else table.ReadPlainBaselineAc(ref reader, block, ref coefficient);
                }
                if (fused && coefficient == 64) value = block[JpegTransform.Zigzag[63]];
                else
                {
                    var symbol = table.Read(ref reader); value = reader.ReadSigned(symbol & 15);
                }
                tail = reader.ReadBits(6); tail = tail * 4 + reader.ReadBits(2);
            }
            catch (Exception failure) when (failure is ImageDataException or ImageLimitException) { error = failure; }
            return (error?.GetType(), error?.Message, reader.Position, value, tail);
        }
    }
}
