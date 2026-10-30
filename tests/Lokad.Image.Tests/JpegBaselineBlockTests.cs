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
            var context = Context(); var reader = new JpegEntropyReader(data, 0, context);
            var block = Enumerable.Repeat(12345, 66).ToArray(); var predictor = 777;
            JpegDecoder.ReadBaselineBlock(ref reader, block.AsSpan(1, 64), ref predictor, tables.Dc, tables.Ac, context);
            Assert.Equal(777 + value, predictor); Assert.Equal(predictor, block[1]);
            Assert.Equal(12345, block[0]); Assert.All(block.Skip(2), coefficient => Assert.Equal(12345, coefficient));
            Assert.Equal(data.Length - 2, reader.Position); reader.Restart(0); Assert.Equal(data.Length, reader.Position);
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
            var data = Stuff(text); var context = Context(); var reader = new JpegEntropyReader(data, 0, context);
            var block = Enumerable.Repeat(12345, 66).ToArray(); var predictor = 0;
            JpegDecoder.ReadBaselineBlock(ref reader, block.AsSpan(1, 64), ref predictor, tables.Dc, tables.Ac, context);
            for (var index = 0; index < 66; index++)
                Assert.Equal(index == 1 ? 0 : index == 1 + JpegTransform.Zigzag[position] ? value : 12345, block[index]);
            Assert.Equal(data.Length - 2, reader.Position); reader.Restart(0); Assert.Equal(data.Length, reader.Position);
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
        var data = Stuff(text); var context = Context(); var reader = new JpegEntropyReader(data, 0, context);
        var actual = new int[64]; var predictor = 123;
        JpegDecoder.ReadBaselineBlock(ref reader, actual, ref predictor, tables.Dc, tables.Ac, context);
        Assert.Equal(expected, actual); Assert.Equal(92, predictor); Assert.Equal(data.Length - 2, reader.Position);
        reader.Restart(0); Assert.Equal(data.Length, reader.Position);     }

    [Theory]
    [InlineData(0, "Invalid JPEG DC coefficient category.")]
    [InlineData(1, "JPEG DC predictor is out of range.")] [InlineData(2, "JPEG DC predictor is out of range.")]
    [InlineData(3, "Invalid baseline JPEG end-of-block symbol.")]
    [InlineData(4, "Invalid JPEG AC coefficient category.")]
    [InlineData(5, "JPEG zero run exceeds band.")] [InlineData(6, "JPEG coefficient run exceeds band.")]
    public void InvalidSymbolsAndPredictorsKeepFailurePositionPartialWrites(int scenario, string message)
    {
        var tables = Tables();
        var text = scenario switch
        {
            0 => Dc(12), 1 => Dc(1) + "1", 2 => Dc(1) + "0",
            3 => Dc(0) + Ac(0x10), 4 => Dc(0) + Ac(0x0B),
            5 => Dc(0) + string.Concat(Enumerable.Repeat(Ac(0xF0), 4)),
            _ => Dc(0) + string.Concat(Enumerable.Repeat(Ac(0xF0), 3)) + Ac(0xF1)
        };

        var data = Stuff(text); var context = Context(); var reader = new JpegEntropyReader(data, 0, context);
        var predictor = scenario == 1 ? 32767 : scenario == 2 ? -32768 : 0;
        var originalPredictor = predictor; var block = Enumerable.Repeat(12345, 64).ToArray();
        try { JpegDecoder.ReadBaselineBlock(ref reader, block, ref predictor, tables.Dc, tables.Ac, context); Assert.Fail("Invalid coefficient syntax must fail."); }
        catch (ImageDataException error) { Assert.Equal(message, error.Message); }
        Assert.Equal(originalPredictor, predictor); Assert.Equal(scenario < 3 ? 12345 : 0, block[0]);
        Assert.All(block.Skip(1), value => Assert.Equal(12345, value)); Assert.Equal(data.Length - 2, reader.Position);
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
    public void IncompleteBlocksRejectBeforeInputOrMutation(int length)
    {
        var tables = Tables(); var context = Context(); var reader = new JpegEntropyReader([], 0, context);
        var block = Enumerable.Repeat(12345, length).ToArray(); var predictor = 7;
        try { JpegDecoder.ReadBaselineBlock(ref reader, block, ref predictor, tables.Dc, tables.Ac, context); Assert.Fail("Only complete coefficient blocks are valid."); }
        catch (ArgumentException error) { Assert.Equal("block", error.ParamName); }
        Assert.Equal(7, predictor); Assert.Equal(0, reader.Position); Assert.All(block, value => Assert.Equal(12345, value));
    }

    private static (JpegHuffman Dc, JpegHuffman Ac) Tables()
    {
        var dc = new byte[16]; dc[3] = 13; var ac = new byte[16]; ac[7] = (byte)AcSymbols.Length;
        return (new JpegHuffman(dc, Enumerable.Range(0, 13).Select(i => (byte)i).ToArray(), Context()), new JpegHuffman(ac, AcSymbols, Context()));
    }

    [Theory]
    [InlineData(false)] [InlineData(true)]
    public void ConsecutiveShortAcKeepsEveryTruncationAndCachedPhase(bool cancelable)
    {
        var tables = Tables();
        var payload = Dc(0) + string.Concat(Enumerable.Range(1, 63).Select(k => Ac(1) + ((k & 1) == 0 ? "1" : "0")));
        for (var phase = 0; phase < 8; phase++)
        {
            var original = Stuff(new string('0', phase) + payload);
            // Compare each byte truncation and the complete stream once.
            foreach (var length in Enumerable.Range(0, original.Length + 1))
            {
                var input = original.AsSpan(0, length).ToArray();
                var expected = Run(true); var actual = Run(false);
                Assert.Equal(expected.Error, actual.Error); Assert.Equal(expected.Message, actual.Message);
                Assert.Equal(expected.Position, actual.Position);                 Assert.Equal(expected.Predictor, actual.Predictor); Assert.Equal(expected.Block, actual.Block);
                Assert.Equal(original.AsSpan(0, length).ToArray(), input);

                (Type? Error, string? Message, int Position, int Predictor, int[] Block) Run(bool scalar)
                {
                    using var cancellation = new CancellationTokenSource();
                    var context = new OperationContext(ImageLimits.Default,
                        cancelable ? cancellation.Token : default);
                    var reader = new JpegEntropyReader(input, 0, context);
                    var block = Enumerable.Repeat(12345, 64).ToArray(); var predictor = 17; Exception? error = null;
                    try
                    {
                        if (phase != 0) reader.ReadBits(phase);
                        if (scalar) ScalarBaseline(ref reader, block, ref predictor, tables.Dc, tables.Ac, context);
                        else JpegDecoder.ReadBaselineBlock(ref reader, block, ref predictor, tables.Dc, tables.Ac, context);
                        reader.Restart(0);
                    }
                    catch (Exception failure) when (failure is ImageDataException or ImageLimitException) { error = failure; }
                    return (error?.GetType(), error?.Message, reader.Position, predictor, block);
                }
            }
        }
    }

    [Fact]
    public void AcPrefixAndAmplitudeRetainEveryTruncationAndPartialState()
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
            {
                var input = original.AsSpan(0, length).ToArray();
                var checkedSymbols = Decode(input, true, scalar: true);
                foreach (var fast in new[] { Decode(input, false), Decode(input, true) })
                {
                    Assert.Equal(checkedSymbols.Error, fast.Error);
                    Assert.Equal(checkedSymbols.Message, fast.Message);
                    Assert.Equal(checkedSymbols.Position, fast.Position);
                    Assert.Equal(checkedSymbols.Predictor, fast.Predictor);
                    Assert.Equal(checkedSymbols.Block, fast.Block);
                    Assert.Equal(original.AsSpan(0, length).ToArray(), input);
                }
            }
        }

        (Type? Error, string? Message, int Position, int Predictor, int[] Block)
            Decode(byte[] input, bool cancelable, bool scalar = false)
        {
            using var cancellation = new CancellationTokenSource();
            var context = new OperationContext(ImageLimits.Default, cancelable ? cancellation.Token : default);
            var reader = new JpegEntropyReader(input, 0, context);
            var block = Enumerable.Repeat(12345, 64).ToArray(); var predictor = 17;
            Exception? error = null;
            try
            {
                if (scalar) ScalarBaseline(ref reader, block, ref predictor, tables.Dc, tables.Ac, context);
                else JpegDecoder.ReadBaselineBlock(ref reader, block, ref predictor, tables.Dc, tables.Ac, context);
                reader.Restart(0);
            }
            catch (Exception failure) when (failure is ImageDataException or ImageLimitException) { error = failure; }
            return (error?.GetType(), error?.Message, reader.Position, predictor, block);
        }
    }

    [Theory]
    [InlineData(false)] [InlineData(true)]
    public void LookaheadReturnsPendingBitsAcrossBlocksAndEveryByteTruncation(bool cancelable)
    {
        var tables = Tables(); var dense = Dc(3) + Signed(7, 3);
        for (var k = 1; k <= 63; k++)
        {
            var size = 1 + k % 10; var threshold = 1 << (size - 1);
            var amplitude = threshold + (k * 37 & (threshold - 1));
            dense += Ac((byte)size) + Signed((k & 1) == 0 ? amplitude : -amplitude, size);
        }
        var sparse = Dc(1) + Signed(-1, 1) + Ac(0x31) + Signed(1, 1) + Ac(0);
        for (var phase = 0; phase < 8; phase++)
        {
            var original = Stuff(new string('0', phase) + dense + sparse + dense + "101");
            for (var length = 0; length <= original.Length; length++)
            {
                var input = original.AsSpan(0, length).ToArray();
                var expected = Run(true); var actual = Run(false);
                Assert.Equal(expected.Error, actual.Error); Assert.Equal(expected.Message, actual.Message);
                Assert.Equal(expected.Position, actual.Position);                 Assert.Equal(expected.Predictor, actual.Predictor); Assert.Equal(expected.Tail, actual.Tail);
                Assert.Equal(expected.Blocks.Count, actual.Blocks.Count);
                for (var i = 0; i < expected.Blocks.Count; i++) Assert.Equal(expected.Blocks[i], actual.Blocks[i]);
                Assert.Equal(original.AsSpan(0, length).ToArray(), input);

                (Type? Error, string? Message, int Position, int Predictor, int Tail, List<int[]> Blocks) Run(bool scalar)
                {
                    using var cancellation = new CancellationTokenSource();
                    var context = new OperationContext(ImageLimits.Default,
                        cancelable ? cancellation.Token : default);
                    var reader = new JpegEntropyReader(input, 0, context);
                    var predictor = 17; var tail = -1; var blocks = new List<int[]>(); Exception? error = null;
                    try
                    {
                        if (phase != 0) reader.ReadBits(phase);
                        for (var i = 0; i < 3; i++)
                        {
                            var block = Enumerable.Repeat(12345, 64).ToArray(); blocks.Add(block);
                            if (scalar) ScalarBaseline(ref reader, block, ref predictor, tables.Dc, tables.Ac, context);
                            else JpegDecoder.ReadBaselineBlock(ref reader, block, ref predictor, tables.Dc, tables.Ac, context);
                        }
                        tail = reader.ReadBits(3); reader.Restart(0);
                    }
                    catch (Exception failure) when (failure is ImageDataException or ImageLimitException) { error = failure; }
                    return (error?.GetType(), error?.Message, reader.Position, predictor, tail, blocks);
                }
            }
        }
    }

    [Fact]
    public void FusedDcAndEndOfBlockKeepEveryCachedPhaseSignedEndpointAndPendingTail()
    {
        var tables = Tables(); var fused = 0;
        for (var phase = 0; phase < 8; phase++)
        for (var size = 0; size <= 11; size++)
        foreach (var value in size == 0 ? new[] { 0 } : new[] { 1 << (size - 1), (1 << size) - 1, -(1 << (size - 1)), -((1 << size) - 1) })
        {
            var input = Stuff(new string('0', phase) + Dc(size) + Signed(value, size) + Ac(0) + "10101100" + new string('0', 64));
            var original = input.ToArray(); using var context = Context();
            var actual = Enumerable.Repeat(12345, 66).ToArray(); var expected = actual.ToArray();
            var reader = new JpegEntropyReader(input, 0, context); var reference = new JpegEntropyReader(input, 0, context);
            if (phase != 0) { reader.ReadBits(phase); reference.ReadBits(phase); }
            var predictor = 777; var expectedPredictor = predictor; var coefficient = 1;
            if (tables.Dc.TryReadPlainBaselineBlock(ref reader, tables.Ac, actual.AsSpan(1, 64), ref predictor, ref coefficient))
            { fused++; Assert.Equal(64, coefficient); }
            else JpegDecoder.ReadBaselineBlock(ref reader, actual.AsSpan(1, 64), ref predictor, tables.Dc, tables.Ac, context);
            ScalarBaseline(ref reference, expected.AsSpan(1, 64), ref expectedPredictor, tables.Dc, tables.Ac, context);
            Assert.Equal(expected, actual); Assert.Equal(expectedPredictor, predictor); Assert.Equal(reference.Position, reader.Position);
            Assert.Equal(172, reader.ReadBits(8)); Assert.Equal(172, reference.ReadBits(8));
            Assert.Equal(reference.Position, reader.Position); Assert.Equal(original, input);
        }
        Assert.True(fused > 100);
    }

    [Fact]
    public void FusedTraversalCommitsValidPrefixAndHandsLongAcItsOriginalPendingBits()
    {
        using var context = Context(); var dcCounts = new byte[16]; dcCounts[0] = 1;
        var acCounts = new byte[16]; acCounts[0] = acCounts[8] = 1;
        var dc = new JpegHuffman(dcCounts, [0], context); var ac = new JpegHuffman(acCounts, [1, 0x0b], context);
        var input = Stuff("001100000000" + new string('0', 64)); var original = input.ToArray();
        var reader = new JpegEntropyReader(input, 0, context); var storage = Enumerable.Repeat(12345, 66).ToArray();
        var predictor = 17; var coefficient = 1;
        Assert.True(dc.TryReadPlainBaselineBlock(ref reader, ac, storage.AsSpan(1, 64), ref predictor, ref coefficient));
        Assert.Equal(2, coefficient); Assert.Equal(17, predictor); Assert.Equal(1, reader.Position);
        Assert.Equal(17, storage[1]); Assert.Equal(1, storage[1 + JpegTransform.Zigzag[1]]);
        for (var i = 0; i < storage.Length; i++)
            if (i != 1 && i != 1 + JpegTransform.Zigzag[1]) Assert.Equal(12345, storage[i]);
        Assert.Equal(0x0b, ac.Read(ref reader)); Assert.Equal(2, reader.Position);
        Assert.Equal(0, reader.ReadBits(5)); Assert.Equal(original, input);
    }

    [Fact]
    public void FusedInitialMissLeavesPredictorStorageReaderAndFutureMarkersUntouched()
    {
        var tables = Tables(); var longCounts = new byte[16]; longCounts[8] = 1;
        var longDc = new JpegHuffman(longCounts, [0], Context());
        Check(longDc, new byte[8], 17, false);
        Check(tables.Dc, Stuff(Dc(12) + new string('0', 64)), 17, false);
        Check(tables.Dc, Stuff(Dc(1) + "1" + new string('0', 64)), 32767, false);
        Check(tables.Dc, [0, 255, 0xd0, 0, 0], 17, false);
        Check(tables.Dc, new byte[8], 17, true);

        void Check(JpegHuffman dc, byte[] input, int initialPredictor, bool cancelable)
        {
            using var cancellation = new CancellationTokenSource();
            using var context = new OperationContext(ImageLimits.Default, cancelable ? cancellation.Token : default);
            var reader = new JpegEntropyReader(input, 0, context); var reference = new JpegEntropyReader(input, 0, context);
            var storage = Enumerable.Repeat(12345, 66).ToArray(); var predictor = initialPredictor; var coefficient = 1;
            Assert.False(dc.TryReadPlainBaselineBlock(ref reader, tables.Ac, storage.AsSpan(1, 64), ref predictor, ref coefficient));
            Assert.Equal(initialPredictor, predictor); Assert.Equal(1, coefficient); Assert.Equal(0, reader.Position);
            Assert.All(storage, value => Assert.Equal(12345, value));
            Assert.Equal(reference.ReadBits(4), reader.ReadBits(4)); Assert.Equal(reference.Position, reader.Position);
        }
    }

    [Fact]
    public void FusedNonzeroDcAndPartialAcKeepEveryTruncationErrorAndGuard()
    {
        var tables = Tables();
        var lead = Dc(5) + Signed(-20, 5) + Ac(1) + "1" + Ac(0x38) + Signed(255, 8) + Ac(0xf0) + Ac(10) + Signed(-777, 10);
        foreach (var ending in new[] { Ac(0), Ac(0x0b), Ac(0x10) })
        for (var phase = 0; phase < 8; phase++)
        {
            var original = Stuff(new string('0', phase) + lead + ending + "101" + new string('0', 64));
            for (var length = 0; length <= original.Length; length++)
            {
                var input = original.AsSpan(0, length).ToArray(); var expected = Run(false); var actual = Run(true);
                Assert.Equal(expected.Error, actual.Error); Assert.Equal(expected.Message, actual.Message);
                Assert.Equal(expected.Position, actual.Position); Assert.Equal(expected.Predictor, actual.Predictor);
                Assert.Equal(expected.Tail, actual.Tail); Assert.Equal(expected.Block, actual.Block);
                Assert.Equal(original.AsSpan(0, length).ToArray(), input);

                (Type? Error, string? Message, int Position, int Predictor, int Tail, int[] Block) Run(bool fast)
                {
                    using var context = Context(); var reader = new JpegEntropyReader(input, 0, context);
                    var block = Enumerable.Repeat(12345, 66).ToArray(); var predictor = 17; var tail = -1; Exception? error = null;
                    try
                    {
                        if (phase != 0) reader.ReadBits(phase);
                        if (fast) JpegDecoder.ReadBaselineBlock(ref reader, block.AsSpan(1, 64), ref predictor, tables.Dc, tables.Ac, context);
                        else ScalarBaseline(ref reader, block.AsSpan(1, 64), ref predictor, tables.Dc, tables.Ac, context);
                        tail = reader.ReadBits(3);
                    }
                    catch (ImageDataException failure) { error = failure; }
                    return (error?.GetType(), error?.Message, reader.Position, predictor, tail, block);
                }
            }
        }
    }

    [Fact]
    public void FusedBlockUsesNoHeapOrAdditionalStorageOwner()
    {
        var tables = Tables(); using var context = Context(); byte[] input = new byte[8];
        Span<int> block = stackalloc int[64]; var predictor = 0; var coefficient = 1;
        for (var i = 0; i < 16; i++)
        {
            var warm = new JpegEntropyReader(input, 0, context); coefficient = 1;
            Assert.True(tables.Dc.TryReadPlainBaselineBlock(ref warm, tables.Ac, block, ref predictor, ref coefficient));
        }
        var allocated = GC.GetAllocatedBytesForCurrentThread(); var complete = 0;
        for (var i = 0; i < 1000; i++)
        {
            var reader = new JpegEntropyReader(input, 0, context); coefficient = 1;
            if (tables.Dc.TryReadPlainBaselineBlock(ref reader, tables.Ac, block, ref predictor, ref coefficient) && coefficient == 64) complete++;
        }
        allocated = GC.GetAllocatedBytesForCurrentThread() - allocated;
        Assert.Equal(1000, complete); Assert.Equal(0, allocated); Assert.Equal(0, predictor);
    }

    // Original unfused T.81 traversal, independently retaining symbol/amplitude
    // and syntax failure order. No speculative state escapes.
    private static void ScalarBaseline(ref JpegEntropyReader reader, Span<int> block, ref int predictor,
        JpegHuffman dc, JpegHuffman ac, OperationContext context)
    {
        var category = dc.Read(ref reader);
        if (category > 11) throw new ImageDataException("Invalid JPEG DC coefficient category.");
        var next = (long)predictor + reader.ReadSigned(category);
        if (next is < -32768 or > 32767) throw new ImageDataException("JPEG DC predictor is out of range.");
        predictor = (int)next; block[0] = predictor;
        var k = 1;
        while (k <= 63)
        {
            var symbol = ac.Read(ref reader); var run = symbol >> 4; var size = symbol & 15;
            if (size == 0)
            {
                if (run == 15) { k += 16; if (k > 64) throw new ImageDataException("JPEG zero run exceeds band."); continue; }
                if (run != 0) throw new ImageDataException("Invalid baseline JPEG end-of-block symbol.");
                return;
            }
            if (size > 10) throw new ImageDataException("Invalid JPEG AC coefficient category.");
            k += run;
            if (k > 63) throw new ImageDataException("JPEG coefficient run exceeds band.");
            block[JpegTransform.Zigzag[k++]] = reader.ReadSigned(size);
        }
    }
    private static string Dc(int size) => Convert.ToString(size, 2).PadLeft(4, '0');
    private static string Ac(byte symbol) => Convert.ToString(Array.IndexOf(AcSymbols, symbol), 2).PadLeft(8, '0');
    private static string Signed(int value, int size) => size == 0 ? "" : Convert.ToString(value >= 0 ? value : value + (1 << size) - 1, 2).PadLeft(size, '0');
    private static OperationContext Context() => new(ImageLimits.Default, default);
    private static byte[] Stuff(string bits)
    {
        bits = bits.PadRight((bits.Length + 7) / 8 * 8, '1'); var bytes = new List<byte>();
        for (var i = 0; i < bits.Length; i += 8) { var value = Convert.ToByte(bits.Substring(i, 8), 2); bytes.Add(value); if (value == 255) bytes.Add(0); }
        bytes.Add(255); bytes.Add(0xD0); return bytes.ToArray();
    }
}
