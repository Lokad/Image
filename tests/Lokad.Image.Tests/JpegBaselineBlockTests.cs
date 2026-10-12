using Lokad.Image.Internal;

namespace Lokad.Image.Tests;

public sealed class JpegBaselineBlockTests
{
    private static readonly byte[] AcSymbols = [0, .. Enumerable.Range(0, 16).SelectMany(run => Enumerable.Range(1, 10).Select(size => (byte)(run << 4 | size))), 0xF0, 0x10, 0x0B];

    [Fact]
    public void EveryDcCategoryKeepsSignedEndpointsPredictorAndUntouchedAc()
    {
        var tables = Tables();
        for (var size = 0; size <= 11; size++)
        foreach (var value in size == 0 ? new[] { 0 } : new[] { 1 << (size - 1), (1 << size) - 1, -(1 << (size - 1)), -((1 << size) - 1) })
        {
            var text = Dc(size) + Signed(value, size) + Ac(0); var data = Stuff(text);
            var context = Context(76 + (size == 0 ? 0 : 1)); var reader = new JpegEntropyReader(data, 0, context);
            var block = Enumerable.Repeat(12345, 66).ToArray(); var predictor = 777;
            JpegDecoder.ReadBaselineBlock(ref reader, block.AsSpan(1, 64), ref predictor, tables.Dc, tables.Ac, context);
            Assert.Equal(777 + value, predictor); Assert.Equal(predictor, block[1]);
            Assert.Equal(12345, block[0]); Assert.All(block.Skip(2), coefficient => Assert.Equal(12345, coefficient));
            Assert.Equal(data.Length - 2, reader.Position); reader.Restart(0); Assert.Equal(data.Length, reader.Position);
            Assert.Throws<ImageLimitException>(() => context.Work(1));
        }
    }

    [Fact]
    public void EveryAcPositionCategoryAndSignedEndpointKeepsZeroRunsAndNaturalOrder()
    {
        var tables = Tables();
        for (var position = 1; position <= 63; position++)
        for (var size = 1; size <= 10; size++)
        foreach (var value in new[] { 1 << (size - 1), (1 << size) - 1, -(1 << (size - 1)), -((1 << size) - 1) })
        {
            var remaining = position - 1; var text = Dc(0); var symbols = 0;
            while (remaining >= 16) { text += Ac(0xF0); remaining -= 16; symbols++; }
            text += Ac((byte)(remaining << 4 | size)) + Signed(value, size); symbols++;
            if (position != 63) { text += Ac(0); symbols++; }
            var data = Stuff(text); var context = Context(69 + symbols * 8); var reader = new JpegEntropyReader(data, 0, context);
            var block = Enumerable.Repeat(12345, 66).ToArray(); var predictor = 0;
            JpegDecoder.ReadBaselineBlock(ref reader, block.AsSpan(1, 64), ref predictor, tables.Dc, tables.Ac, context);
            for (var index = 0; index < 66; index++)
                Assert.Equal(index == 1 ? 0 : index == 1 + JpegTransform.Zigzag[position] ? value : 12345, block[index]);
            Assert.Equal(data.Length - 2, reader.Position); reader.Restart(0); Assert.Equal(data.Length, reader.Position);
            Assert.Throws<ImageLimitException>(() => context.Work(1));
        }
    }

    [Fact]
    public void DenseMixedBlockEndsAtCoefficient63WithoutRequiringEndOfBlock()
    {
        var tables = Tables(); var text = Dc(5) + Signed(-31, 5); var expected = new int[64]; expected[0] = 92;
        for (var k = 1; k <= 63; k++)
        {
            var size = 1 + k % 10; var value = ((k & 1) == 0 ? 1 : -1) * ((1 << size) - 1);
            text += Ac((byte)size) + Signed(value, size); expected[JpegTransform.Zigzag[k]] = value;
        }
        var data = Stuff(text); var context = Context(69 + 63 * 9); var reader = new JpegEntropyReader(data, 0, context);
        var actual = new int[64]; var predictor = 123;
        JpegDecoder.ReadBaselineBlock(ref reader, actual, ref predictor, tables.Dc, tables.Ac, context);
        Assert.Equal(expected, actual); Assert.Equal(92, predictor); Assert.Equal(data.Length - 2, reader.Position);
        reader.Restart(0); Assert.Equal(data.Length, reader.Position); Assert.Throws<ImageLimitException>(() => context.Work(1));
    }

    [Theory]
    [InlineData(0, "Invalid JPEG DC coefficient category.")]
    [InlineData(1, "JPEG DC predictor is out of range.")] [InlineData(2, "JPEG DC predictor is out of range.")]
    [InlineData(3, "Invalid baseline JPEG end-of-block symbol.")]
    [InlineData(4, "Invalid JPEG AC coefficient category.")]
    [InlineData(5, "JPEG zero run exceeds band.")] [InlineData(6, "JPEG coefficient run exceeds band.")]
    public void InvalidSymbolsAndPredictorsKeepFailurePositionPartialWritesAndWork(int scenario, string message)
    {
        var tables = Tables();
        var text = scenario switch
        {
            0 => Dc(12), 1 => Dc(1) + "1", 2 => Dc(1) + "0",
            3 => Dc(0) + Ac(0x10), 4 => Dc(0) + Ac(0x0B),
            5 => Dc(0) + string.Concat(Enumerable.Repeat(Ac(0xF0), 4)),
            _ => Dc(0) + string.Concat(Enumerable.Repeat(Ac(0xF0), 3)) + Ac(0xF1)
        };
        var work = scenario < 3 ? 68 + (scenario == 0 ? 0 : 1) : scenario < 5 ? 76 : 100;
        var data = Stuff(text); var context = Context(work); var reader = new JpegEntropyReader(data, 0, context);
        var predictor = scenario == 1 ? 32767 : scenario == 2 ? -32768 : 0;
        var originalPredictor = predictor; var block = Enumerable.Repeat(12345, 64).ToArray();
        try { JpegDecoder.ReadBaselineBlock(ref reader, block, ref predictor, tables.Dc, tables.Ac, context); Assert.Fail("Invalid coefficient syntax must fail."); }
        catch (ImageDataException error) { Assert.Equal(message, error.Message); }
        Assert.Equal(originalPredictor, predictor); Assert.Equal(scenario < 3 ? 12345 : 0, block[0]);
        Assert.All(block.Skip(1), value => Assert.Equal(12345, value)); Assert.Equal(data.Length - 2, reader.Position);
        Assert.Throws<ImageLimitException>(() => context.Work(1));
    }

    [Fact]
    public void TightWorkBudgetsKeepBlockAdmissionAndPartialHuffmanBitConsumption()
    {
        var tables = Tables();
        foreach (var limit in new[] { 63, 66 })
        {
            var context = Context(limit); var reader = new JpegEntropyReader(Stuff(Dc(0) + Ac(0)), 0, context);
            var block = Enumerable.Repeat(12345, 64).ToArray(); var predictor = 7;
            try { JpegDecoder.ReadBaselineBlock(ref reader, block, ref predictor, tables.Dc, tables.Ac, context); Assert.Fail("The original work boundary must fail."); }
            catch (ImageLimitException error) { Assert.Equal("Computational work limit exceeded.", error.Message); }
            Assert.Equal(7, predictor); Assert.All(block, value => Assert.Equal(12345, value));
            Assert.Equal(limit == 63 ? 0 : 1, reader.Position);
            if (limit == 63) context.Work(63);
            Assert.Throws<ImageLimitException>(() => context.Work(1));
        }
    }

    [Fact]
    public void CancellationRejectsBeforeReadingOrMutatingTheBlock()
    {
        var tables = Tables(); using var cancellation = new CancellationTokenSource();
        var context = new OperationContext(ImageLimits.Default, cancellation.Token); cancellation.Cancel();
        var reader = new JpegEntropyReader(Stuff(Dc(0) + Ac(0)), 0, context);
        var block = Enumerable.Repeat(12345, 64).ToArray(); var predictor = 7;
        try { JpegDecoder.ReadBaselineBlock(ref reader, block, ref predictor, tables.Dc, tables.Ac, context); Assert.Fail("Cancellation must reject the block."); }
        catch (OperationCanceledException) { }
        Assert.Equal(7, predictor); Assert.Equal(0, reader.Position); Assert.All(block, value => Assert.Equal(12345, value));
    }

    [Theory]
    [InlineData(63)] [InlineData(65)]
    public void IncompleteBlocksRejectBeforeWorkInputOrMutation(int length)
    {
        var tables = Tables(); var context = Context(64); var reader = new JpegEntropyReader([], 0, context);
        var block = Enumerable.Repeat(12345, length).ToArray(); var predictor = 7;
        try { JpegDecoder.ReadBaselineBlock(ref reader, block, ref predictor, tables.Dc, tables.Ac, context); Assert.Fail("Only complete coefficient blocks are valid."); }
        catch (ArgumentException error) { Assert.Equal("block", error.ParamName); }
        Assert.Equal(7, predictor); Assert.Equal(0, reader.Position); Assert.All(block, value => Assert.Equal(12345, value));
        context.Work(64); Assert.Throws<ImageLimitException>(() => context.Work(1));
    }

    [Theory]
    [InlineData(68)] [InlineData(69)] [InlineData(72)]
    public void TruncatedAmplitudeKeepsAdmittedWorkAndReusableReaderWithParentWorkAlreadyConsumed(int allowance)
    {
        var tables = Tables();
        foreach (var cancellable in new[] { false, true })
        {
            using var cancellation = new CancellationTokenSource();
            var context = new OperationContext(ImageLimits.Default with { MaxWork = 11 + allowance },
                cancellable ? cancellation.Token : default);
            context.Work(11);
            // Category five occupies four bits; only four amplitude bits remain.
            // Its original one-unit amplitude charge precedes the missing refill.
            var reader = new JpegEntropyReader(new byte[] { 0x5F }, 0, context);
            var block = Enumerable.Repeat(12345, 64).ToArray(); var predictor = 7;
            try { JpegDecoder.ReadBaselineBlock(ref reader, block, ref predictor, tables.Dc, tables.Ac, context); Assert.Fail("The incomplete amplitude must fail."); }
            catch (ImageLimitException error) { Assert.Equal(68, allowance); Assert.Equal("Computational work limit exceeded.", error.Message); }
            catch (ImageDataException error) { Assert.NotEqual(68, allowance); Assert.Equal("Truncated JPEG entropy data.", error.Message); }
            Assert.Equal(7, predictor); Assert.Equal(1, reader.Position); Assert.All(block, value => Assert.Equal(12345, value));
            Assert.Equal(allowance - Math.Min(allowance, 69), context.RemainingWork);
            if (allowance == 72)
            {
                context.Work(1); Assert.Equal(0, reader.ReadBits(0)); Assert.Equal(1, context.RemainingWork); context.Work(1);
            }
            Assert.Throws<ImageLimitException>(() => context.Work(1));
        }
    }

    private static (JpegHuffman Dc, JpegHuffman Ac) Tables()
    {
        var dc = new byte[16]; dc[3] = 13; var ac = new byte[16]; ac[7] = (byte)AcSymbols.Length;
        return (new JpegHuffman(dc, Enumerable.Range(0, 13).Select(i => (byte)i).ToArray(), Context()), new JpegHuffman(ac, AcSymbols, Context()));
    }

    [Fact]
    public void AcPrefixAndAmplitudeRetainEveryBudgetTruncationAndPartialState()
    {
        var tables = Tables();
        // Original T.81 grammars: mixed amplitudes, a stuffed FF byte, a zero
        // run, EOB, an invalid category and a run exceeding the remaining band.
        var lead = Dc(0) + Ac(4) + Signed(-10, 4) + Ac(0x38) + Signed(255, 8)
            + Ac(0xF0) + Ac(10) + Signed(-777, 10);
        foreach (var text in new[] { lead + Ac(0), lead + Ac(0x0B) + new string('1', 11),
                     Dc(0) + string.Concat(Enumerable.Repeat(Ac(0xF0), 3)) + Ac(0xF1) + "1" })
        {
            var original = Stuff(text);
            for (var length = 0; length <= original.Length; length++)
            for (var work = 63; work <= 130; work++)
            {
                var input = original.AsSpan(0, length).ToArray();
                var fast = Decode(input, work, false); var checkedSymbols = Decode(input, work, true);
                Assert.Equal(checkedSymbols.Error, fast.Error);
                Assert.Equal(checkedSymbols.Message, fast.Message);
                Assert.Equal(checkedSymbols.Position, fast.Position);
                Assert.Equal(checkedSymbols.RemainingWork, fast.RemainingWork);
                Assert.Equal(checkedSymbols.Predictor, fast.Predictor);
                Assert.Equal(checkedSymbols.Block, fast.Block);
                Assert.Equal(original.AsSpan(0, length).ToArray(), input);
            }
        }

        (Type? Error, string? Message, int Position, long RemainingWork, int Predictor, int[] Block)
            Decode(byte[] input, long work, bool cancelable)
        {
            using var cancellation = new CancellationTokenSource();
            var context = new OperationContext(ImageLimits.Default with { MaxWork = work }, cancelable ? cancellation.Token : default);
            var reader = new JpegEntropyReader(input, 0, context);
            var block = Enumerable.Repeat(12345, 64).ToArray(); var predictor = 17;
            Exception? error = null;
            try
            {
                JpegDecoder.ReadBaselineBlock(ref reader, block, ref predictor, tables.Dc, tables.Ac, context);
                reader.Restart(0);
            }
            catch (Exception failure) when (failure is ImageDataException or ImageLimitException) { error = failure; }
            return (error?.GetType(), error?.Message, reader.Position, context.RemainingWork, predictor, block);
        }
    }
    private static string Dc(int size) => Convert.ToString(size, 2).PadLeft(4, '0');
    private static string Ac(byte symbol) => Convert.ToString(Array.IndexOf(AcSymbols, symbol), 2).PadLeft(8, '0');
    private static string Signed(int value, int size) => size == 0 ? "" : Convert.ToString(value >= 0 ? value : value + (1 << size) - 1, 2).PadLeft(size, '0');
    private static OperationContext Context(long work = 2000000000) => new(ImageLimits.Default with { MaxWork = work }, default);
    private static byte[] Stuff(string bits)
    {
        bits = bits.PadRight((bits.Length + 7) / 8 * 8, '1'); var bytes = new List<byte>();
        for (var i = 0; i < bits.Length; i += 8) { var value = Convert.ToByte(bits.Substring(i, 8), 2); bytes.Add(value); if (value == 255) bytes.Add(0); }
        bytes.Add(255); bytes.Add(0xD0); return bytes.ToArray();
    }
}
