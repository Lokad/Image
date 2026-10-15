using Lokad.Image.Internal;

namespace Lokad.Image.Tests;

public sealed class ZlibFramingTests
{
    [Theory]
    [InlineData(false)] [InlineData(true)]
    public void LongLiteralCodesRetainWholePrefixAdmissionAndIncompletePathErrors(bool shortLiteral)
    {
        // Original RFC 1951 grammars, deliberately incomplete: all 257 codes
        // have the long depth, except an optional one-bit literal zero. Each
        // byte value exercises a different canonical symbol offset. Framing
        // accepts assigned paths in incomplete trees, including empty distances.
        foreach (var depth in new[] { 10, 11, 15 })
        {
            foreach (var cancellable in new[] { false, true })
            {
                var encoded = LongLiteralStream(depth, shortLiteral, false);
                var original = encoded.ToArray();
                using var cancellation = new CancellationTokenSource();
                var token = cancellable ? cancellation.Token : default;
                const long prior = 11;
                var work = 10413L + 257 * depth - (shortLiteral ? depth - 1 : 0);
                var limits = ImageLimits.Default with { MaxWork = prior + work, MaxWorkingBytes = 4096 };
                var context = new OperationContext(limits, token); context.Work(prior);
                Assert.Equal(encoded.Length, ZlibFraming.Validate(encoded, 256, context));
                Assert.Equal(original, encoded);
                Assert.Throws<ImageLimitException>(() => context.Work(1));
                context.Reserve(4096); context.Release(4096);

                var tight = new OperationContext(limits with { MaxWork = limits.MaxWork - 1 }, token); tight.Work(prior);
                var limitError = Assert.Throws<ImageLimitException>(() => ZlibFraming.Validate(encoded, 256, tight));
                Assert.Equal("Computational work limit exceeded.", limitError.Message);
                tight.Work(depth - 1); // EOB's entire prefix fails admission.
                Assert.Throws<ImageLimitException>(() => tight.Work(1));
                tight.Reserve(4096); tight.Release(4096);
                Assert.Equal(original, encoded);

                var invalid = LongLiteralStream(depth, shortLiteral, true);
                var invalidOriginal = invalid.ToArray();
                var failed = new OperationContext(limits, token); failed.Work(prior);
                var dataError = Assert.Throws<ImageDataException>(() => ZlibFraming.Validate(invalid, 256, failed));
                Assert.Equal("Invalid PNG Deflate prefix code.", dataError.Message);
                Assert.Throws<ImageLimitException>(() => failed.Work(1));
                failed.Reserve(4096); failed.Release(4096);
                var failedTight = new OperationContext(limits with { MaxWork = limits.MaxWork - 1 }, token); failedTight.Work(prior);
                Assert.Throws<ImageLimitException>(() => ZlibFraming.Validate(invalid, 256, failedTight));
                failedTight.Work(depth - 1);
                Assert.Throws<ImageLimitException>(() => failedTight.Work(1));
                failedTight.Reserve(4096); failedTight.Release(4096);
                Assert.Equal(invalidOriginal, invalid);
            }
        }
    }

    private static byte[] LongLiteralStream(int depth, bool shortLiteral, bool invalidEnd)
    {
        var bits = new BitWriter(); bits.Number(1, 1); bits.Number(2, 2);
        bits.Number(0, 5); bits.Number(0, 5); bits.Number(15, 4);
        ReadOnlySpan<int> order = [16, 17, 18, 0, 8, 7, 9, 6, 10, 5, 11, 4, 12, 3, 13, 2, 14, 1, 15];
        foreach (var symbol in order) bits.Number(symbol < 16 ? 4 : 0, 3);
        for (var symbol = 0; symbol < 257; symbol++) bits.Code(shortLiteral && symbol == 0 ? 1 : depth, 4);
        bits.Code(0, 4); // Empty distance alphabet.
        var first = shortLiteral ? (1 << (depth - 1)) - 1 : 0;
        uint a = 1, b = 0;
        for (var symbol = 0; symbol < 256; symbol++)
        {
            bits.Code(shortLiteral && symbol == 0 ? 0 : first + symbol, shortLiteral && symbol == 0 ? 1 : depth);
            a = (a + (uint)symbol) % 65521; b = (b + a) % 65521;
        }
        bits.Code(invalidEnd ? (1 << depth) - 1 : first + 256, depth); bits.Align();
        return [0x78, 0x01, .. bits.Bytes(), (byte)(b >> 8), (byte)b, (byte)(a >> 8), (byte)a];
    }

    [Theory]
    [InlineData(false)] [InlineData(true)]
    public void TruncatedDistanceExtraBitsRetainAdmittedWorkAndErrorPrecedence(bool cancellable)
    {
        // Original fixed stream, deliberately missing the distance's thirteen
        // extra bits and checksum. Read charges before attempting its refill.
        var bits = new BitWriter(); bits.Number(1, 1); bits.Number(1, 2);
        for (var i = 0; i < 12; i++) bits.Code(0x30 + 65, 8);
        bits.Code(1, 7); bits.Code(29, 5); bits.Align();
        byte[] encoded = [0x78, 0x01, .. bits.Bytes()];
        using var cancellation = new CancellationTokenSource();
        var token = cancellable ? cancellation.Token : default;
        const long prior = 11, work = 9380 + 12 * 8 + 7 + 5 + 13;
        var limits = ImageLimits.Default with { MaxWork = prior + work, MaxWorkingBytes = 4096 };
        var context = new OperationContext(limits, token); context.Work(prior);
        var error = Assert.Throws<ImageDataException>(() => ZlibFraming.Validate(encoded, 64, context));
        Assert.Equal("Truncated PNG Deflate stream.", error.Message);
        Assert.Throws<ImageLimitException>(() => context.Work(1));
        context.Reserve(4096); context.Release(4096);

        var tight = new OperationContext(limits with { MaxWork = limits.MaxWork - 1 }, token); tight.Work(prior);
        Assert.Throws<ImageLimitException>(() => ZlibFraming.Validate(encoded, 64, tight));
        tight.Work(12); // The failing thirteen-bit charge is atomic.
        Assert.Throws<ImageLimitException>(() => tight.Work(1));
        tight.Reserve(4096); tight.Release(4096);
    }

    [Theory]
    [InlineData(false, false)] [InlineData(false, true)]
    [InlineData(true, false)] [InlineData(true, true)]
    public void ReservedCodeAfterLiteralsRetainsExactFailureWork(bool distanceError, bool cancellable)
    {
        // Original RFC 1951 fixed literals followed by a reserved code, with
        // enough declared output for further literals. A speculative lookahead
        // must not charge or consume future prefixes before this format error.
        var bits = new BitWriter(); bits.Number(1, 1); bits.Number(1, 2);
        for (var i = 0; i < 12; i++) bits.Code(0x30 + 65, 8);
        if (distanceError) { bits.Code(1, 7); bits.Code(30, 5); }
        else bits.Code(198, 8); // Reserved fixed literal/length symbol 286.
        bits.Align(); byte[] encoded = [0x78, 0x01, .. bits.Bytes(), 0, 0, 0, 0];
        using var cancellation = new CancellationTokenSource();
        var token = cancellable ? cancellation.Token : default;
        // Header plus fixed trees/lookups: 9,380; twelve eight-bit literals;
        // then either eight reserved length bits or seven length + five distance.
        var work = 9380 + 12 * 8 + (distanceError ? 12 : 8);
        var context = new OperationContext(ImageLimits.Default with { MaxWork = work, MaxWorkingBytes = 4096 }, token);
        var error = Assert.Throws<ImageDataException>(() => ZlibFraming.Validate(encoded, 64, context));
        Assert.Equal(distanceError ? "Invalid PNG Deflate distance code." : "Invalid PNG Deflate length code.", error.Message);
        Assert.Throws<ImageLimitException>(() => context.Work(1));
        context.Reserve(4096); context.Release(4096);

        var tight = new OperationContext(ImageLimits.Default with { MaxWork = work - 1, MaxWorkingBytes = 4096 }, token);
        Assert.Throws<ImageLimitException>(() => ZlibFraming.Validate(encoded, 64, tight));
        tight.Work(distanceError ? 4 : 7); // Failing prefix admission is atomic.
        Assert.Throws<ImageLimitException>(() => tight.Work(1));
        tight.Reserve(4096); tight.Release(4096);
    }

    [Theory]
    [InlineData(false)] [InlineData(true)]
    public void LiteralRunsRetainEveryTightBudgetAndOutputFailure(bool reservedCode)
    {
        var bits = new BitWriter(); bits.Number(1, 1); bits.Number(1, 2);
        for (var i = 0; i < 40; i++)
            if ((i & 1) == 0) bits.Code(48 + 65, 8); else bits.Code(400, 9);
        if (reservedCode) bits.Code(198, 8); else bits.Code(0, 7);
        bits.Align(); byte[] encoded = [0x78, 0x01, .. bits.Bytes(), 0, 0, 0, 0];
        var original = encoded.ToArray();
        using var cancellation = new CancellationTokenSource();
        // Compare both counting loops with cancelable materialization, whose
        // CopyLiteralRun remains disabled and dispatches every symbol separately.
        // Check every bit budget through the literal run and EOB.
        foreach (var expected in new[] { 0L, 19, 39, 40, 41 })
        for (var extra = 0; extra <= 348; extra++)
        {
            var limits = ImageLimits.Default with { MaxWork = 9380 + extra, MaxWorkingBytes = 4096 };
            var direct = new OperationContext(limits, default);
            var checkedSymbols = new OperationContext(limits, cancellation.Token);
            var first = Record.Exception(() => ZlibFraming.Validate(encoded, expected, direct));
            var second = Record.Exception(() => ZlibFraming.Validate(encoded, expected, checkedSymbols));
            Assert.Equal(second?.GetType(), first?.GetType());
            Assert.Equal(second?.Message, first?.Message);
            Assert.Equal(checkedSymbols.RemainingWork, direct.RemainingWork);
            if (expected > 0)
            {
                var perSymbol = new OperationContext(limits, cancellation.Token);
                var output = new byte[(int)expected];
                var reference = Record.Exception(() => ZlibFraming.ValidateAndInflate(encoded, output, perSymbol, out _));
                Assert.Equal(reference?.GetType(), first?.GetType());
                Assert.Equal(reference?.Message, first?.Message);
                Assert.Equal(perSymbol.RemainingWork, direct.RemainingWork);
                Assert.Equal(perSymbol.RemainingWorkingBytes, direct.RemainingWorkingBytes);
            }
            direct.Reserve(4096); direct.Release(4096);
            checkedSymbols.Reserve(4096); checkedSymbols.Release(4096);
        }
        Assert.Equal(original, encoded);
    }

    [Theory]
    [InlineData(0)] [InlineData(1)] [InlineData(2)] [InlineData(3)]
    public void MatchRunsRetainEveryTightBudgetOutputAndTruncationFailure(int shape)
    {
        // Independently authored fixed-code streams exercise repeated matches,
        // extra bits, overlapping history and a distance beyond available history.
        var bits = new BitWriter(); bits.Number(1, 1); bits.Number(1, 2);
        for (var i = 0; i < 64; i++) bits.Code(48 + 65, 8);
        for (var i = 0; i < 16; i++)
        {
            if (shape == 0) { bits.Code(1, 7); bits.Code(0, 5); } // Length 3, distance 1.
            else if (shape == 1) { bits.Code(9, 7); bits.Number(1, 1); bits.Code(4, 5); bits.Number(1, 1); } // 12, 6.
            else if (shape == 2) { bits.Code(196, 8); bits.Number(31, 5); bits.Code(11, 5); bits.Number(15, 4); } // 258, 64.
            else { bits.Code(197, 8); bits.Code(13, 5); bits.Number(31, 5); } // 258, 128; first match fails.
        }
        bits.Code(0, 7); bits.Align(); byte[] encoded = [0x78, 0x01, .. bits.Bytes(), 0, 0, 0, 0];
        using var cancellation = new CancellationTokenSource();
        foreach (var expected in new[] { 0L, 63, 64, 65, 112, 256, 4192 })
        for (var extra = 0; extra <= 938; extra++) Compare(encoded, expected, 9380 + extra);
        // Every truncation also compares the remaining atomic work charge.
        for (var length = 0; length <= encoded.Length; length++) Compare(encoded[..length], 4192, 20000);

        void Compare(byte[] input, long expected, long work)
        {
            var limits = ImageLimits.Default with { MaxWork = work, MaxWorkingBytes = 4096 };
            var direct = new OperationContext(limits, default);
            var checkedSymbols = new OperationContext(limits, cancellation.Token);
            var first = Record.Exception(() => ZlibFraming.Validate(input, expected, direct));
            var second = Record.Exception(() => ZlibFraming.Validate(input, expected, checkedSymbols));
            Assert.Equal(second?.GetType(), first?.GetType());
            Assert.Equal(second?.Message, first?.Message);
            Assert.Equal(checkedSymbols.RemainingWork, direct.RemainingWork);
            direct.Reserve(4096); direct.Release(4096);
            checkedSymbols.Reserve(4096); checkedSymbols.Release(4096);
        }
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void PrefixLookaheadReturnsStoredHeaderBytesAndExactWork(bool trailingBytes)
    {
        // Original RFC 1951 fixed literals followed by a final stored block.
        // The prefix lookahead crosses into LEN/NLEN and the stored payload.
        var bits = new BitWriter(); bits.Number(0, 1); bits.Number(1, 2);
        for (var i = 0; i < 12; i++) bits.Code(0x30 + 65, 8);
        bits.Code(0, 7); // Fixed end-of-block symbol 256.
        bits.Number(1, 1); bits.Number(0, 2); bits.Align();
        bits.Number(3, 16); bits.Number(65532, 16);
        bits.Number(0, 8); bits.Number(127, 8); bits.Number(255, 8);
        byte[] raw = [.. Enumerable.Repeat((byte)65, 12), 0, 127, 255];
        uint a = 1, b = 0;
        foreach (var value in raw) { a += value; b += a; }
        byte[] encoded = [0x78, 0x01, .. bits.Bytes(), (byte)(b >> 8), (byte)b, (byte)(a >> 8), (byte)a];
        var streamLength = encoded.Length;
        if (trailingBytes) encoded = [.. encoded, 99, 88];
        var context = new OperationContext(ImageLimits.Default with { MaxWork = 9522, MaxWorkingBytes = 4096 }, default);
        Assert.Equal(streamLength, ZlibFraming.Validate(encoded, raw.Length, context));
        Assert.Throws<ImageLimitException>(() => context.Work(1));
        context.Reserve(4096); context.Release(4096);
        using var inflater = new System.IO.Compression.ZLibStream(new MemoryStream(encoded, 0, streamLength), System.IO.Compression.CompressionMode.Decompress);
        var actual = new byte[raw.Length]; inflater.ReadExactly(actual);
        Assert.Equal(raw, actual); Assert.Equal(-1, inflater.ReadByte());

        var tight = new OperationContext(ImageLimits.Default with { MaxWork = 9521, MaxWorkingBytes = 4096 }, default);
        Assert.Throws<ImageLimitException>(() => ZlibFraming.Validate(encoded, raw.Length, tight));
        // Stored-byte work remains one three-unit charge after LEN/NLEN;
        // an insufficient charge does not consume the last two budget units.
        tight.Work(2); tight.Reserve(4096); tight.Release(4096);
        Assert.Throws<ImageLimitException>(() => tight.Work(1));
    }

    [Fact]
    public void LiteralLookaheadRejectsAnUnassignedPrefixAfterAShortLiteral()
    {
        // Original incomplete alphabet: literal 65 = 0, EOB = 10, 11 unassigned.
        var bits = new BitWriter(); bits.Number(1, 1); bits.Number(2, 2);
        bits.Number(0, 5); bits.Number(0, 5); bits.Number(15, 4);
        ReadOnlySpan<int> order = [16, 17, 18, 0, 8, 7, 9, 6, 10, 5, 11, 4, 12, 3, 13, 2, 14, 1, 15];
        foreach (var symbol in order) bits.Number(symbol < 16 ? 4 : 0, 3);
        for (var symbol = 0; symbol < 257; symbol++) bits.Code(symbol == 65 ? 1 : symbol == 256 ? 2 : 0, 4);
        bits.Code(0, 4); bits.Code(0, 1); bits.Code(3, 2); bits.Align();
        byte[] encoded = [0x78, 0x01, .. bits.Bytes(), 0, 0, 0, 1];
        var error = Assert.Throws<ImageDataException>(() => ZlibFraming.Validate(encoded, 2, new OperationContext(ImageLimits.Default, default)));
        Assert.Equal("Invalid PNG Deflate prefix code.", error.Message);
    }

    [Fact]
    public void LiteralLookaheadFallsBackToATenBitCodeAfterAShortLiteral()
    {
        // Original complete comb: EOB at depth 1, literal 17 at depth 2,
        // fillers at depths 3..10, and literal 65 as the final depth-10 leaf.
        var bits = new BitWriter(); bits.Number(1, 1); bits.Number(2, 2);
        bits.Number(0, 5); bits.Number(0, 5); bits.Number(15, 4);
        ReadOnlySpan<int> order = [16, 17, 18, 0, 8, 7, 9, 6, 10, 5, 11, 4, 12, 3, 13, 2, 14, 1, 15];
        foreach (var symbol in order) bits.Number(symbol < 16 ? 4 : 0, 3);
        for (var symbol = 0; symbol < 257; symbol++)
            bits.Code(symbol == 256 ? 1 : symbol == 17 ? 2 : symbol == 65 ? 10 : symbol is >= 1 and <= 8 ? symbol + 2 : 0, 4);
        bits.Code(0, 4); // Empty distance alphabet.
        bits.Code(2, 2); bits.Code(1023, 10); bits.Code(2, 2); bits.Code(0, 1); bits.Align();
        byte[] expanded = [17, 65, 17]; uint a = 1, b = 0;
        foreach (var value in expanded) { a += value; b += a; }
        byte[] encoded = [0x78, 0x01, .. bits.Bytes(), (byte)(b >> 8), (byte)b, (byte)(a >> 8), (byte)a];
        var limits = new ImageLimits { MaxWork = 10428, MaxWorkingBytes = 4096 };
        Assert.Equal(encoded.Length, ZlibFraming.Validate(encoded, 3, new OperationContext(limits, default)));
        Assert.Throws<ImageLimitException>(() => ZlibFraming.Validate(encoded, 3,
            new OperationContext(limits with { MaxWork = 10427 }, default)));
        for (var length = 0; length < encoded.Length; length++)
            Assert.Throws<ImageDataException>(() => ZlibFraming.Validate(encoded.AsSpan(0, length), 3,
                new OperationContext(ImageLimits.Default, default)));
        using var inflater = new System.IO.Compression.ZLibStream(new MemoryStream(encoded), System.IO.Compression.CompressionMode.Decompress);
        var actual = new byte[3]; inflater.ReadExactly(actual);
        Assert.Equal(expanded, actual); Assert.Equal(-1, inflater.ReadByte());
    }

    [Theory]
    [InlineData(0)] [InlineData(1)] [InlineData(2)] [InlineData(3)]
    [InlineData(4)] [InlineData(5)] [InlineData(6)] [InlineData(7)]
    [InlineData(32)] [InlineData(33)]
    public void AdjacentLiteralsPreserveShortTailsAndExactConsumedWork(int literals)
    {
        // Original RFC 1951 fixed stream. Alternating nine/eight-bit literals
        // exercise paired lookahead, an odd final literal and every alignment.
        var bits = new BitWriter(); bits.Number(1, 1); bits.Number(1, 2);
        var expanded = new byte[literals];
        for (var i = 0; i < literals; i++)
        {
            expanded[i] = (byte)((i & 1) == 0 ? 144 : 65);
            bits.Code((i & 1) == 0 ? 400 : 48 + 65, (i & 1) == 0 ? 9 : 8);
        }
        bits.Code(0, 7); bits.Align();
        uint a = 1, b = 0;
        foreach (var value in expanded) { a += value; b += a; }
        byte[] encoded = [0x78, 0x01, .. bits.Bytes(), (byte)(b >> 8), (byte)b, (byte)(a >> 8), (byte)a];
        // Block/header/tree/lookup/EOB plus only the actual literal bits.
        var work = 9387L + 9 * ((literals + 1) / 2) + 8 * (literals / 2);
        var limits = new ImageLimits { MaxWork = work, MaxWorkingBytes = 4096 };
        Assert.Equal(encoded.Length, ZlibFraming.Validate(encoded, literals, new OperationContext(limits, default)));
        Assert.Equal(encoded.Length, ZlibFraming.Validate([.. encoded, 29, 71, 211], literals, new OperationContext(limits, default)));
        Assert.Throws<ImageLimitException>(() => ZlibFraming.Validate(encoded, literals,
            new OperationContext(limits with { MaxWork = work - 1 }, default)));
        for (var length = 0; length < encoded.Length; length++)
            Assert.Throws<ImageDataException>(() => ZlibFraming.Validate(encoded.AsSpan(0, length), literals,
                new OperationContext(ImageLimits.Default, default)));
        if (literals != 0)
            Assert.Throws<ImageDataException>(() => ZlibFraming.Validate(encoded, literals - 1, new OperationContext(ImageLimits.Default, default)));
        using var inflater = new System.IO.Compression.ZLibStream(new MemoryStream(encoded), System.IO.Compression.CompressionMode.Decompress);
        var actual = new byte[literals]; inflater.ReadExactly(actual);
        Assert.Equal(expanded, actual); Assert.Equal(-1, inflater.ReadByte());
    }

    [Theory]
    [InlineData(0)] [InlineData(1)] [InlineData(2)] [InlineData(3)]
    [InlineData(4)] [InlineData(5)] [InlineData(6)] [InlineData(7)]
    [InlineData(24)] [InlineData(25)] [InlineData(26)] [InlineData(27)]
    [InlineData(28)] [InlineData(29)] [InlineData(30)] [InlineData(31)]
    public void PrefixLookaheadPreservesEveryStoredBlockAlignmentAndExactWork(int literals)
    {
        // RFC 1951 sections 3.2.3/3.2.6: literal 144 has a nine-bit fixed
        // prefix, EOB has seven. Varying its count covers all bit alignments.
        // Longer runs cross successive seven-byte refills, including a full
        // 64-bit cache, before returning lookahead bytes to the stored header.
        var bits = new BitWriter();
        bits.Number(0, 1); bits.Number(1, 2); // Nonfinal fixed block.
        for (var i = 0; i < literals; i++) bits.Code(400, 9);
        bits.Code(0, 7); // End of fixed block.
        bits.Number(1, 1); bits.Number(0, 2); bits.Align(); // Final stored block.
        bits.Number(1, 16); bits.Number(65534, 16); bits.Number(7, 8);
        byte[] encoded = [0x78, 0x01, .. bits.Bytes(), 0, 0, 0, 0];
        // One work unit/block, all consumed bits, fixed-tree and lookup construction,
        // and one stored byte. Lookahead must never charge unused bits.
        var work = 720L + 8192 + 512 + 9 * literals;
        var limits = new ImageLimits { MaxWork = work, MaxWorkingBytes = 4096 };
        Assert.Equal(encoded.Length, ZlibFraming.Validate(encoded, literals + 1, new OperationContext(limits, default)));
        Assert.Equal(encoded.Length, ZlibFraming.Validate([.. encoded, 53, 97, 211], literals + 1, new OperationContext(limits, default)));
        Assert.Throws<ImageLimitException>(() => ZlibFraming.Validate(encoded, literals + 1,
            new OperationContext(limits with { MaxWork = work - 1 }, default)));
        Assert.Throws<ImageLimitException>(() => ZlibFraming.Validate(encoded, literals + 1,
            new OperationContext(limits with { MaxWorkingBytes = 4095 }, default)));
        for (var length = 0; length < encoded.Length; length++)
            Assert.Throws<ImageDataException>(() => ZlibFraming.Validate(encoded.AsSpan(0, length), literals + 1,
                new OperationContext(ImageLimits.Default, default)));
    }

    [Theory]
    [InlineData(0)] [InlineData(1)] [InlineData(2)]
    [InlineData(3)] [InlineData(4)] [InlineData(5)]
    public void WordLookaheadPreservesStoredPayloadSkipsAndTheFinalChecksum(int storedLength)
    {
        // Original RFC 1951 stream: fixed literal, nonfinal stored payload,
        // final fixed literal. Payload lengths exercise every word boundary.
        var payload = Enumerable.Range(0, storedLength).Select(i => (byte)(53 * i + 1)).ToArray();
        var bits = new BitWriter();
        bits.Number(0, 1); bits.Number(1, 2); bits.Code(400, 9); bits.Code(0, 7);
        bits.Number(0, 1); bits.Number(0, 2); bits.Align();
        bits.Number(storedLength, 16); bits.Number(storedLength ^ 65535, 16);
        foreach (var value in payload) bits.Number(value, 8);
        bits.Number(1, 1); bits.Number(1, 2); bits.Code(48 + 17, 8); bits.Code(0, 7); bits.Align();
        byte[] expanded = [144, .. payload, 17];
        uint a = 1, b = 0;
        foreach (var value in expanded) { a += value; b += a; }
        var adler = b << 16 | a;
        byte[] encoded = [0x78, 0x01, .. bits.Bytes(), (byte)(adler >> 24), (byte)(adler >> 16), (byte)(adler >> 8), (byte)adler];
        var limits = new ImageLimits { MaxWork = 17803L + 1024 + storedLength, MaxWorkingBytes = 4096 };
        for (var trailing = 0; trailing <= 9; trailing++)
            Assert.Equal(encoded.Length, ZlibFraming.Validate([.. encoded, .. Enumerable.Repeat((byte)211, trailing)], expanded.Length,
                new OperationContext(limits, default)));
        Assert.Throws<ImageLimitException>(() => ZlibFraming.Validate(encoded, expanded.Length,
            new OperationContext(limits with { MaxWork = limits.MaxWork - 1 }, default)));
        for (var length = 0; length < encoded.Length; length++)
            Assert.Throws<ImageDataException>(() => ZlibFraming.Validate(encoded.AsSpan(0, length), expanded.Length,
                new OperationContext(ImageLimits.Default, default)));
        using var inflater = new System.IO.Compression.ZLibStream(new MemoryStream(encoded), System.IO.Compression.CompressionMode.Decompress);
        var actual = new byte[expanded.Length]; inflater.ReadExactly(actual);
        Assert.Equal(expanded, actual); Assert.Equal(-1, inflater.ReadByte());
    }

    [Theory]
    [InlineData(0)] [InlineData(1)] [InlineData(2)] [InlineData(3)]
    [InlineData(4)] [InlineData(5)] [InlineData(6)] [InlineData(7)]
    [InlineData(8)] [InlineData(9)] [InlineData(10)] [InlineData(11)]
    [InlineData(12)] [InlineData(13)] [InlineData(14)] [InlineData(15)]
    [InlineData(16)] [InlineData(17)] [InlineData(18)] [InlineData(19)]
    [InlineData(20)] [InlineData(21)] [InlineData(22)] [InlineData(23)]
    [InlineData(24)] [InlineData(25)] [InlineData(26)] [InlineData(27)]
    [InlineData(28)] [InlineData(29)] [InlineData(30)] [InlineData(31)]
    public void FixedDistanceLookupReachesFullHistoryAndRejectsReservedSymbols(int symbol)
    {
        // Original RFC 1951 fixed stream: 32 KiB zero history then a three-byte
        // match. All-one extra bits reach the upper endpoint of each range.
        var bits = new BitWriter(); bits.StoredZeros(32768);
        bits.Number(1, 1); bits.Number(1, 2); bits.Code(1, 7);
        bits.Code(symbol, 5);
        var extraBits = symbol is >= 4 and < 30 ? symbol / 2 - 1 : 0;
        bits.Number((1 << extraBits) - 1, extraBits); bits.Code(0, 7); bits.Align();
        var encoded = ZeroStream(bits, 32771);
        if (symbol >= 30)
        {
            Assert.Throws<ImageDataException>(() => ZlibFraming.Validate(encoded, 32771,
                new OperationContext(ImageLimits.Default, default)));
            return;
        }
        var limits = new ImageLimits { MaxWork = 42203L + extraBits, MaxWorkingBytes = 4096 };
        Assert.Equal(encoded.Length, ZlibFraming.Validate(encoded, 32771, new OperationContext(limits, default)));
        Assert.Throws<ImageLimitException>(() => ZlibFraming.Validate(encoded, 32771,
            new OperationContext(limits with { MaxWork = limits.MaxWork - 1 }, default)));
        AssertZeroInflation(encoded, 32771);
    }

    [Theory]
    [InlineData(0)] [InlineData(1)] [InlineData(2)] [InlineData(3)]
    [InlineData(4)] [InlineData(5)] [InlineData(6)] [InlineData(7)]
    [InlineData(8)] [InlineData(9)] [InlineData(10)] [InlineData(11)]
    [InlineData(12)] [InlineData(13)] [InlineData(14)] [InlineData(15)]
    public void DynamicDistanceLookupAndFallbackAgreeThroughFifteenBits(int symbol)
    {
        // A complete canonical comb: one leaf at depths 1..14, two at 15.
        // Symbols 0..15 exercise both sides of the five-bit lookup boundary.
        int[] lengths = [.. Enumerable.Range(1, 14), 15, 15];
        var depth = lengths[symbol];
        var code = (1 << depth) - (symbol == 15 ? 1 : 2);
        var extraBits = symbol >= 4 ? symbol / 2 - 1 : 0;
        var encoded = DynamicDistanceStream(lengths, code, depth, extraBits);
        var limits = new ImageLimits { MaxWork = 11059L + depth + extraBits, MaxWorkingBytes = 4096 };
        Assert.Equal(encoded.Length, ZlibFraming.Validate(encoded, 515, new OperationContext(limits, default)));
        Assert.Throws<ImageLimitException>(() => ZlibFraming.Validate(encoded, 515,
            new OperationContext(limits with { MaxWork = limits.MaxWork - 1 }, default)));
        Assert.Throws<ImageLimitException>(() => ZlibFraming.Validate(encoded, 515,
            new OperationContext(limits with { MaxWorkingBytes = 4095 }, default)));
        for (var length = encoded.Length - 8; length < encoded.Length; length++)
            Assert.Throws<ImageDataException>(() => ZlibFraming.Validate(encoded.AsSpan(0, length), 515,
                new OperationContext(ImageLimits.Default, default)));
        AssertZeroInflation(encoded, 515);
    }

    [Theory]
    [InlineData(false)] [InlineData(true)]
    public void UnassignedDistancePrefixesAreRejectedInLookupAndLongFallback(bool longCode)
    {
        // Remove the last comb leaf, or use one one-bit leaf. The all-one
        // path is unassigned in either incomplete alphabet.
        int[] lengths = longCode ? [.. Enumerable.Range(1, 15)] : [1];
        var depth = longCode ? 15 : 1;
        var encoded = DynamicDistanceStream(lengths, (1 << depth) - 1, depth, 0);
        Assert.Throws<ImageDataException>(() => ZlibFraming.Validate(encoded, 515,
            new OperationContext(ImageLimits.Default, default)));
    }

    [Fact]
    public void IncompleteTreeAcceptsItsEndCodeAndRejectsTheUnassignedPrefix()
    {
        // RFC 1951 dynamic block: only EOB has a one-bit code (zero), and
        // distances are empty. Prefix one is explicitly unassigned.
        foreach (var unassigned in new[] { false, true })
        {
            var bits = new BitWriter();
            bits.Number(1, 1); bits.Number(2, 2);
            bits.Number(0, 5); bits.Number(0, 5); bits.Number(14, 4);
            ReadOnlySpan<int> order = [16, 17, 18, 0, 8, 7, 9, 6, 10, 5, 11, 4, 12, 3, 13, 2, 14, 1];
            foreach (var symbol in order) bits.Number(symbol is 0 or 1 ? 1 : 0, 3);
            for (var symbol = 0; symbol < 257; symbol++) bits.Code(symbol == 256 ? 1 : 0, 1);
            bits.Code(0, 1); // Empty distance alphabet.
            bits.Code(unassigned ? 1 : 0, 1); bits.Align();
            byte[] encoded = [0x78, 0x01, .. bits.Bytes(), 0, 0, 0, 1];
            if (unassigned)
                Assert.Throws<ImageDataException>(() => ZlibFraming.Validate(encoded, 0, new OperationContext(ImageLimits.Default, default)));
            else
            {
                Assert.Equal(encoded.Length, ZlibFraming.Validate(encoded, 0, new OperationContext(ImageLimits.Default, default)));
                using var inflater = new System.IO.Compression.ZLibStream(new MemoryStream(encoded), System.IO.Compression.CompressionMode.Decompress);
                Assert.Equal(-1, inflater.ReadByte());
            }
        }
    }

    [Fact]
    public void FramingChecksCancellationOnAnAlreadyCreatedContext()
    {
        using var cancellation = new CancellationTokenSource();
        var context = new OperationContext(ImageLimits.Default, cancellation.Token);
        cancellation.Cancel();
        byte[] emptyFixedBlock = [0x78, 0x01, 3, 0, 0, 0, 0, 1];
        Assert.Throws<OperationCanceledException>(() => ZlibFraming.Validate(emptyFixedBlock, 0, context));
    }

    private static byte[] DynamicDistanceStream(int[] distanceLengths, int code, int depth, int extraBits)
    {
        var bits = new BitWriter(); bits.StoredZeros(512);
        bits.Number(1, 1); bits.Number(2, 2);
        bits.Number(1, 5); bits.Number(distanceLengths.Length - 1, 5); bits.Number(15, 4);
        ReadOnlySpan<int> order = [16, 17, 18, 0, 8, 7, 9, 6, 10, 5, 11, 4, 12, 3, 13, 2, 14, 1, 15];
        // Complete uniform four-bit code-length alphabet for values 0..15.
        foreach (var symbol in order) bits.Number(symbol < 16 ? 4 : 0, 3);
        for (var symbol = 0; symbol < 258; symbol++) bits.Code(symbol >= 256 ? 1 : 0, 4);
        foreach (var length in distanceLengths) bits.Code(length, 4);
        bits.Code(1, 1); bits.Code(code, depth); bits.Number(0, extraBits); bits.Code(0, 1); bits.Align();
        return ZeroStream(bits, 515);
    }

    private static byte[] ZeroStream(BitWriter bits, int expandedLength)
    {
        var checksum = (uint)expandedLength << 16 | 1;
        return [0x78, 0x01, .. bits.Bytes(), (byte)(checksum >> 24), (byte)(checksum >> 16), 0, 1];
    }

    private static void AssertZeroInflation(byte[] encoded, int length)
    {
        using var inflater = new System.IO.Compression.ZLibStream(new MemoryStream(encoded), System.IO.Compression.CompressionMode.Decompress);
        var actual = new byte[length]; inflater.ReadExactly(actual);
        Assert.Equal(new byte[length], actual); Assert.Equal(-1, inflater.ReadByte());
    }

    private sealed class BitWriter
    {
        private readonly List<byte> bytes = [];
        private int used;
        public void Number(int value, int count) { for (var i = 0; i < count; i++) Bit((value >> i) & 1); }
        public void Code(int value, int count) { for (var i = count - 1; i >= 0; i--) Bit((value >> i) & 1); }
        public void Align() { while ((used & 7) != 0) Bit(0); }
        public void StoredZeros(int length)
        {
            Number(0, 1); Number(0, 2); Align();
            Number(length, 16); Number(length ^ 65535, 16);
            for (var i = 0; i < length; i++) Number(0, 8);
        }
        private void Bit(int value)
        {
            if ((used & 7) == 0) bytes.Add(0);
            bytes[^1] |= (byte)(value << (used++ & 7));
        }
        public byte[] Bytes() => bytes.ToArray();
    }
}
