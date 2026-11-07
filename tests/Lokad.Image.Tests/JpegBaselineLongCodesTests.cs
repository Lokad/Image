using Lokad.Image.Internal;

namespace Lokad.Image.Tests;

public sealed partial class JpegBaselineBlockTests
{
    [Fact]
    public void ExistingLongLookupCompletesFusedBlocksWithExactFollowingReads()
    {
        using var tables = Context();
        var (dc, ac) = LongBaselineTables(tables);
        for (var phase = 0; phase < 8; phase++)
        foreach (var value in new[] { 700, -700 })
        {
            var input = Stuff(new string('0', phase) + "0" + LongAc(1) + "1"
                + LongAc(0x1a) + Signed(value, 10) + LongAc(0) + "1011010010101100" + new string('0', 64));
            using var context = Context();
            var reader = new JpegEntropyReader(input, 0, context); var original = reader;
            var expected = Enumerable.Repeat(12345, 66).ToArray(); var actual = expected.ToArray();
            if (phase != 0) { reader.ReadBits(phase); original.ReadBits(phase); }
            var predictor = 17; var expectedPredictor = 17; var coefficient = 1;
            ScalarBaseline(ref original, expected.AsSpan(1, 64), ref expectedPredictor, dc, ac, context);
            Assert.True(dc.TryReadPlainBaselineBlock(ref reader, ac, actual.AsSpan(1, 64), ref predictor, ref coefficient));
            Assert.Equal(64, coefficient); Assert.Equal(expected, actual);
            Assert.Equal(expectedPredictor, predictor); Assert.Equal(original.Position, reader.Position);
            for (var count = 1; count <= 16; count++)
            {
                var expectedTail = original; var actualTail = reader;
                Assert.Equal(expectedTail.ReadBits(count), actualTail.ReadBits(count));
                Assert.Equal(expectedTail.Position, actualTail.Position);
            }
        }
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void LongBaselineCodesPreserveTruncationStuffingFailuresAndPartialWrites(bool cancelable)
    {
        using var tables = Context(); var (dc, ac) = LongBaselineTables(tables);
        var prefix = "0" + LongAc(1) + "1" + LongAc(0x1a) + Signed(-1023, 10);
        foreach (var suffix in new[] { LongAc(0), LongAc(0x0b),
                     string.Concat(Enumerable.Repeat(LongAc(0xf0), 4)) })
        for (var phase = 0; phase < 8; phase++)
        {
            var original = Stuff(new string('0', phase) + prefix + suffix + "101" + new string('0', 32));
            for (var length = 0; length <= original.Length; length++)
            {
                var input = original.AsSpan(0, length).ToArray(); var expected = Run(true); var actual = Run(false);
                Assert.Equal(expected.Error, actual.Error); Assert.Equal(expected.Message, actual.Message);
                Assert.Equal(expected.Position, actual.Position); Assert.Equal(expected.Predictor, actual.Predictor);
                Assert.Equal(expected.Block, actual.Block); Assert.Equal(expected.Tail, actual.Tail);
                Assert.Equal(original.AsSpan(0, length).ToArray(), input);

                (Type? Error, string? Message, int Position, int Predictor, int[] Block, int Tail) Run(bool scalar)
                {
                    using var cancellation = new CancellationTokenSource();
                    using var context = new OperationContext(ImageLimits.Default, cancelable ? cancellation.Token : default);
                    var reader = new JpegEntropyReader(input, 0, context);
                    var block = Enumerable.Repeat(12345, 66).ToArray(); var predictor = 17; var tail = -1;
                    Exception? error = null;
                    try
                    {
                        if (phase != 0) reader.ReadBits(phase);
                        if (scalar) ScalarBaseline(ref reader, block.AsSpan(1, 64), ref predictor, dc, ac, context);
                        else JpegDecoder.ReadBaselineBlock(ref reader, block.AsSpan(1, 64), ref predictor, dc, ac, context);
                        tail = reader.ReadBits(3);
                    }
                    catch (ImageDataException failure) { error = failure; }
                    return (error?.GetType(), error?.Message, reader.Position, predictor, block, tail);
                }
            }
        }
    }

    private static (JpegHuffman Dc, JpegHuffman Ac) LongBaselineTables(OperationContext context)
    {
        var dc = new byte[16]; dc[0] = 1;
        var ac = new byte[16]; ac[7] = 1; ac[8] = 1; ac[9] = 3;
        return (new JpegHuffman(dc, [0], context), new JpegHuffman(ac, [0, 1, 0x1a, 0xf0, 0x0b], context));
    }

    // Original canonical grammar for counts at lengths8/9/10: 1/1/3.
    private static string LongAc(byte symbol) => symbol switch
    {
        0 => "00000000", 1 => "000000010", 0x1a => "0000000110",
        0xf0 => "0000000111", 0x0b => "0000001000", _ => throw new ArgumentOutOfRangeException(nameof(symbol))
    };
}
