using System.Buffers.Binary;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using Lokad.Image.Internal;
using Lokad.Image.Tooling;

namespace Lokad.Image.Tests;

public sealed class Vp8MacroblockDecisionTests
{
    [Theory]
    [InlineData("diagram", 7100, "0b71843059a6c3224cec9df38a460ce6087ad21e1e0f499978f7767747a1a050")]
    [InlineData("photograph", 17688, "e2df1f461cb04516aa8630f326efdc959e1ae8a7dce578ef3d064b20c788719b")]
    public void LiveCancellationTraversalKeepsFrozenPacketsAndBorrowedInputOwnership(string name, int size, string expected)
    {
        using var root = new TestMemoryBudget(128_000_000);
        using var image = OriginalVp8lControls.Buffer(name, 8, root);
        var pixels = OriginalPixels.Bytes(image); var retained = root.Bytes;
        using var cancellation = new CancellationTokenSource();
        using var first = new MemoryStream(); using var second = new MemoryStream();
        var limits = new ImageLimits { MemoryBudget = root };
        var options = new WebpEncodeOptions { Lossless = false, Quality = 75, Effort = 4, Limits = limits };
        ImageCodec.EncodeWebp(image, first, options);
        Assert.Equal(retained, root.Bytes);
        ImageCodec.EncodeWebp(image, second, options, cancellation.Token);
        Assert.Equal(retained, root.Bytes); Assert.False(root.IsDisposed);
        Assert.Equal(size, first.Length); Assert.Equal(first.ToArray(), second.ToArray());
        Assert.Equal(expected, Convert.ToHexStringLower(SHA256.HashData(first.ToArray())));
        Assert.NotSame(first.GetBuffer(), second.GetBuffer());
        Assert.Equal(pixels, OriginalPixels.Bytes(image)); image.Dispose(); Assert.Equal(0, root.Bytes);
        first.Dispose(); Assert.Equal(size, second.Length);
        Assert.Equal(expected, Convert.ToHexStringLower(SHA256.HashData(second.ToArray())));
    }

    private static void WriteBefore(Span<byte> bytes, int index, int x, int y, byte segment,
        ReadOnlySpan<byte> source, ReadOnlySpan<byte> neighbors, ReadOnlySpan<byte> modes,
        uint top, uint left, byte leftDc, ReadOnlySpan<sbyte> errors, int decisionLevel = 1)
    {
        "BEF8"u8.CopyTo(bytes); var at = 4;
        foreach (var value in new[] { index, x, y, decisionLevel, (int)segment }) { BinaryPrimitives.WriteInt32LittleEndian(bytes[at..], value); at += 4; }
        source[..384].CopyTo(bytes[24..]); neighbors[..71].CopyTo(bytes[408..]); modes[..8].CopyTo(bytes[479..]);
        BinaryPrimitives.WriteUInt32LittleEndian(bytes[487..], top); BinaryPrimitives.WriteUInt32LittleEndian(bytes[491..], left);
        BinaryPrimitives.WriteInt32LittleEndian(bytes[495..], leftDc); BinaryPrimitives.WriteInt32LittleEndian(bytes[499..], 1);
        MemoryMarshal.AsBytes(errors[..8]).CopyTo(bytes[503..]);
    }

    private static void WriteAfter(Span<byte> bytes, int index, Vp8MacroblockScore score,
        ReadOnlySpan<byte> modes, ReadOnlySpan<short> levels, ReadOnlySpan<byte> pixels, ReadOnlySpan<sbyte> errors)
    {
        "AFT8"u8.CopyTo(bytes); var at = 4;
        foreach (var value in new[] { index, score.Nonzero == 0 ? 1 : 0, score.Intra16 ? 1 : 0, score.LumaMode, score.ChromaMode, (int)score.Nonzero })
        { BinaryPrimitives.WriteInt32LittleEndian(bytes[at..], value); at += 4; }
        foreach (var value in new[] { score.Distortion, score.Texture, score.Header, score.Rate, score.Total })
        { BinaryPrimitives.WriteInt64LittleEndian(bytes[at..], value); at += 8; }
        modes[..16].CopyTo(bytes[68..]);
        for (var p = 0; p < 400; p++) BinaryPrimitives.WriteInt16LittleEndian(bytes[(84 + p * 2)..], levels[p]);
        pixels[..384].CopyTo(bytes[884..]); MemoryMarshal.AsBytes(errors[..14]).CopyTo(bytes[1268..]);
    }

    private static void Contexts(uint top, uint left, byte dc, Span<byte> values)
    {
        ReadOnlySpan<int> above = [12, 13, 14, 15, 18, 19, 22, 23, 24];
        ReadOnlySpan<int> side = [3, 7, 11, 15, 17, 19, 21, 23];
        for (var p = 0; p < 9; p++) values[p] = (byte)((top >> above[p]) & 1);
        for (var p = 0; p < 8; p++) values[9 + p] = (byte)((left >> side[p]) & 1);
        values[17] = dc;
    }

    private static uint Record(ReadOnlySpan<short> levels, bool intra16, Span<byte> contexts, Span<uint> statistics)
    {
        var top = contexts[..9]; var left = contexts.Slice(9, 9);
        if (intra16) top[8] = left[8] = Vp8TokenStatistics.Record(levels[..16], 1, 0, top[8] + left[8], statistics) ? (byte)1 : (byte)0;
        for (var y = 0; y < 4; y++) for (var x = 0; x < 4; x++)
            top[x] = left[y] = Vp8TokenStatistics.Record(levels.Slice(16 + (y * 4 + x) * 16, 16), intra16 ? 0 : 3, intra16 ? 1 : 0, top[x] + left[y], statistics) ? (byte)1 : (byte)0;
        for (var channel = 0; channel < 2; channel++) for (var y = 0; y < 2; y++) for (var x = 0; x < 2; x++)
        {
            var a = 4 + channel * 2 + x; var b = 4 + channel * 2 + y;
            top[a] = left[b] = Vp8TokenStatistics.Record(levels.Slice(272 + (channel * 4 + y * 2 + x) * 16, 16), 2, 0, top[a] + left[b], statistics) ? (byte)1 : (byte)0;
        }
        ReadOnlySpan<int> above = [12, 13, 14, 15, 18, 19, 22, 23, 24]; uint packed = 0;
        for (var p = 0; p < 9; p++) packed |= (uint)top[p] << above[p];
        packed |= (uint)left[0] << 3 | (uint)left[1] << 7 | (uint)left[2] << 11 | (uint)left[4] << 17 | (uint)left[6] << 21;
        return packed;
    }

    // These hashes are our observer's complete before/after records from two
    // original controls, after its whole packet matched the frozen reference.
    // Inputs and all rolling state below are generated here, without an oracle
    // file, native dependency, copied corpus or expected-state input at runtime.
    [Theory]
    [InlineData("opaque", "96434ac268b05cdc2e48575fb179dd6fc06bacb517161bfc220df443b7ee0783", "712746471599612d0c67af42d67aeb9313377efdc3abe5dddb8fcbb3d9176871", "c3d707570839159f043e2d20025c9a936c4f60504acca655faf8e4def24d7e26", 0, 0, 4)]
    [InlineData("diagram", "7edee9f0b7b7be0dce5074b0d9b38804f25fff5079fb6fd60a873110941d07e6", "53b0c735333ac5b2def299cac2d6636c42dbe66049a8aea2c6f40dd9ffa42138", "c5c3326ccf2ec81011734300af1120b4a535b60c38368335436ee9a5164477ab", 353, 255, 4)]
    [InlineData("diagram", "d47a2887f3a9107bcd60cb0690e1ba0c571a58f346fa8f979c7d5457b36c3175", "3edef52edaa67c7bb29446ea0032413353656520f6474a05cb829fce1417e9a4", "5df4edf14b246acc1a4f603b84a9e3afd331294dffd54cc40eea896226ea1b2d", 349, 240, 5)]
    [InlineData("diagram", "fcd54df5bb195ec90bb7c2c5afeb99f60da95c2c8a5d460d85eb0b5974f06709", "adc2b5101c386a025d6be9dc7f9117e4112bab3a8afcd95ac16d5138b0a6f6c0", "22120b0e737d76f48b7e16bc26c86bf1e07fc6396f60c75f0a1b47b8bf7c571f", 357, 255, 6)]
    public void CompleteDecisionAndRollingStateMatchNativeOnOriginalControls(string name, string expectedBefore,
        string expectedAfter, string expectedProfiles, int expectedIntra16, int expectedSkips, int method)
    {
        using var root = new TestMemoryBudget(16_000_000); using var image = OriginalVp8lControls.Buffer(name, 8, root);
        using var context = new OperationContext(new() { MemoryBudget = root }, default); using var input = Vp8PreparedInput.Create(image, context);
        using var analysis = Vp8SourceAnalysis.Create(input, context); using var segments = Vp8SourceSegments.Create(analysis, 75, false, context);
        using var quantization = Vp8QuantizationPlan.Create(segments, analysis.MeanChromaScore, context);
        // Test-owned rolling state, counters and stack scratch share the same
        // root as image/analysis/rate owners. No per-block admission or scopes.
        const int scratch = Vp8MacroblockDecision.ScratchBytes + 16_384; context.Reserve(scratch);
        var columns = input.Stride / 16; var rows = input.StorageHeight / 16;
        var yTop = Enumerable.Repeat((byte)127, columns * 16).ToArray(); var uvTop = yTop.ToArray();
        var modeTop = new byte[columns * 4]; var nonzero = new uint[columns + 1]; var topErrors = new sbyte[columns * 4];
        var statistics = new uint[1056]; var probabilities = Vp8Tables.CoefficientProbabilities.ToArray();
        Span<byte> source = stackalloc byte[384]; Span<byte> neighbors = stackalloc byte[71]; Span<byte> modeContexts = stackalloc byte[8];
        Span<byte> contexts = stackalloc byte[18]; Span<byte> modeLeft = stackalloc byte[4]; Span<sbyte> errorLeft = stackalloc sbyte[4];
        Span<sbyte> errorInput = stackalloc sbyte[8]; Span<sbyte> errors = stackalloc sbyte[16]; Span<byte> pixels = stackalloc byte[386];
        Span<short> levels = stackalloc short[402]; Span<byte> modes = stackalloc byte[18]; Span<ushort> weights = stackalloc ushort[16];
        pixels[384] = 19; pixels[385] = 20; levels[400] = -30000; levels[401] = 30000; modes[16] = 21; modes[17] = 22; errors[14] = 23; errors[15] = 24;
        Span<byte> before = stackalloc byte[511]; Span<byte> after = stackalloc byte[1282]; Span<byte> profile = stackalloc byte[1064];
        Vp8Distortion.TextureWeights(weights); using var preHash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
        using var postHash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256); using var profileHash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
        using var trace = Environment.GetEnvironmentVariable("LOKAD_VP8_DECISION_TRACE") is { } path ? File.Create(path + "-" + name + "-m" + method + ".bin") : null;
        Vp8RatePlan? rate = null; var selected16 = 0; var skips = 0; var original = Vp8PreparedInputTests.Hash(input);
        try
        {
            rate = Vp8RatePlan.Create(probabilities, context); var leftDc = (byte)0; var count = columns * rows;
            for (var index = 0; index < count; index++)
            {
var x = index % columns; var y = index / columns;
                if (x == 0)
                {
                    neighbors[..35].Fill(129); neighbors[0] = neighbors[17] = neighbors[26] = y == 0 ? (byte)127 : (byte)129;
                    modeLeft.Clear(); errorLeft.Clear(); leftDc = 0;
                }
                if (index == 0 || (index + 1) % (Math.Max(count >> 3, 96) + 1) == 0)
                {
                    if (index != 0)
                    {
                        Vp8TokenStatistics.Update(statistics, rate.Entropy, probabilities, out _);
                        var next = Vp8RatePlan.Create(probabilities, context); rate.Dispose(); rate = next;
                    }
                    "PRB8"u8.CopyTo(profile); BinaryPrimitives.WriteInt32LittleEndian(profile[4..], index); probabilities.CopyTo(profile[8..]); profileHash.AppendData(profile);
                }
                for (var yy = 0; yy < 16; yy++) input.Y.Slice((y * 16 + yy) * input.Stride + x * 16, 16).CopyTo(source.Slice(yy * 16, 16));
                for (var yy = 0; yy < 8; yy++)
                {
                    input.U.Slice((y * 8 + yy) * (input.Stride / 2) + x * 8, 8).CopyTo(source.Slice(256 + yy * 16, 8));
                    input.V.Slice((y * 8 + yy) * (input.Stride / 2) + x * 8, 8).CopyTo(source.Slice(264 + yy * 16, 8));
                }
                yTop.AsSpan(x * 16, 16).CopyTo(neighbors[35..]); uvTop.AsSpan(x * 16, 16).CopyTo(neighbors[51..]);
                for (var p = 0; p < 4; p++) neighbors[67 + p] = x + 1 < columns ? yTop[(x + 1) * 16 + p] : yTop[x * 16 + 15];
                modeTop.AsSpan(x * 4, 4).CopyTo(modeContexts); modeLeft.CopyTo(modeContexts[4..]);
                Contexts(nonzero[x + 1], nonzero[x], leftDc, contexts); topErrors.AsSpan(x * 4, 4).CopyTo(errorInput); errorLeft.CopyTo(errorInput[4..]);
                var segment = segments.Map[index]; WriteBefore(before, index, x, y, segment, source, neighbors, modeContexts, nonzero[x + 1], nonzero[x], leftDc, errorInput, Math.Max(1, method - 3));
                var retained = root.Bytes;
                var score = Vp8MacroblockDecision.Select(source, neighbors, modeContexts, contexts, errorInput, y != 0, x != 0,
                    quantization.Matrix(segment, Vp8CoefficientPlane.Luma), quantization.Matrix(segment, Vp8CoefficientPlane.Walsh), quantization.Matrix(segment, Vp8CoefficientPlane.Chroma),
                    quantization.Costs(segment), rate, weights, 65536, pixels, levels, modes, errors, context, method: method);
                Assert.Equal(retained, root.Bytes); WriteAfter(after, index, score, modes, levels, pixels, errors);
                preHash.AppendData(before); postHash.AppendData(after); trace?.Write(before); trace?.Write(after);
                if (score.Intra16) selected16++; if (score.Nonzero == 0) skips++;
                nonzero[x + 1] = Record(levels, score.Intra16, contexts, statistics); leftDc = contexts[17];
                for (var p = 0; p < 4; p++)
                {
                    modeTop[x * 4 + p] = score.Intra16 ? (byte)score.LumaMode : modes[12 + p];
                    modeLeft[p] = score.Intra16 ? (byte)score.LumaMode : modes[p * 4 + 3];
                }
                if (x + 1 < columns)
                {
                    neighbors[0] = neighbors[50]; neighbors[17] = neighbors[58]; neighbors[26] = neighbors[66];
                    for (var p = 0; p < 16; p++) neighbors[1 + p] = pixels[p * 16 + 15];
                    for (var p = 0; p < 8; p++) { neighbors[18 + p] = pixels[256 + p * 16 + 7]; neighbors[27 + p] = pixels[256 + p * 16 + 15]; }
                }
                if (y + 1 < rows) { pixels.Slice(240, 16).CopyTo(yTop.AsSpan(x * 16, 16)); pixels.Slice(368, 16).CopyTo(uvTop.AsSpan(x * 16, 16)); }
                errors.Slice(6, 4).CopyTo(topErrors.AsSpan(x * 4, 4)); errors.Slice(10, 4).CopyTo(errorLeft);
            }
            Assert.Equal(expectedBefore, Convert.ToHexStringLower(preHash.GetHashAndReset()));
            Assert.Equal(expectedAfter, Convert.ToHexStringLower(postHash.GetHashAndReset()));
            Assert.Equal(expectedProfiles, Convert.ToHexStringLower(profileHash.GetHashAndReset()));
            Assert.Equal((expectedIntra16, expectedSkips), (selected16, skips)); Assert.Equal(original, Vp8PreparedInputTests.Hash(input));
            Assert.Equal((19, 20, -30000, 30000, 21, 22, 23, 24), ((int)pixels[384], (int)pixels[385], (int)levels[400], (int)levels[401], (int)modes[16], (int)modes[17], (int)errors[14], (int)errors[15]));
        }
        finally { rate?.Dispose(); context.Release(scratch); }
    }

    [Theory]
    [InlineData(true, 4)]
    [InlineData(false, 4)]
    [InlineData(true, 5)]
    [InlineData(false, 5)]
    [InlineData(true, 6)]
    [InlineData(false, 6)]
    public void BorrowedInputsCanAliasResultsAndInvalidLayoutsOrCancellationPreserveSeparateInputs(bool correctDc, int method)
    {
        using var root = new TestMemoryBudget(16_000_000); using var image = OriginalVp8lControls.Buffer("opaque", 8, root);
        using var context = new OperationContext(new() { MemoryBudget = root }, default); using var input = Vp8PreparedInput.Create(image, context);
        using var analysis = Vp8SourceAnalysis.Create(input, context); using var segments = Vp8SourceSegments.Create(analysis, 75, false, context);
        using var quantization = Vp8QuantizationPlan.Create(segments, analysis.MeanChromaScore, context);
        using var rate = Vp8RatePlan.Create(Vp8Tables.CoefficientProbabilities, context);
        var source = new byte[384]; var neighbors = Enumerable.Repeat((byte)129, 71).ToArray();
        neighbors[0] = neighbors[17] = neighbors[26] = 127; neighbors.AsSpan(35).Fill(127);
        for (var y = 0; y < 16; y++) input.Y.Slice(y * input.Stride, 16).CopyTo(source.AsSpan(y * 16, 16));
        for (var y = 0; y < 8; y++) { input.U.Slice(y * input.Stride / 2, 8).CopyTo(source.AsSpan(256 + y * 16, 8)); input.V.Slice(y * input.Stride / 2, 8).CopyTo(source.AsSpan(264 + y * 16, 8)); }
        var originalSource = source.ToArray(); var originalNeighbors = neighbors.ToArray(); var weights = new ushort[16]; Vp8Distortion.TextureWeights(weights);
        var modeInput = new byte[8]; var contexts = new byte[18]; var diffusion = new sbyte[8]; var segment = segments.Map[0];
        Vp8MacroblockScore Run(byte[] src, byte[] modesIn, byte[] ctxIn, sbyte[] errorsIn,
            Span<byte> dst, Span<short> coefficients, Span<byte> modeOut, Span<sbyte> errorOut, OperationContext operation)
            => Vp8MacroblockDecision.Select(src, neighbors, modesIn, ctxIn, errorsIn, false, false,
                quantization.Matrix(segment, Vp8CoefficientPlane.Luma), quantization.Matrix(segment, Vp8CoefficientPlane.Walsh), quantization.Matrix(segment, Vp8CoefficientPlane.Chroma),
                quantization.Costs(segment), rate, weights, 65536, dst, coefficients, modeOut, errorOut, operation, correctDc, method);
        context.Reserve(Vp8MacroblockDecision.ScratchBytes);
        try
        {
            var retained = root.Bytes; var pixels = new byte[386]; var levels = new short[402]; var modes = new byte[18]; var errors = new sbyte[16];
            var expected = Run(source, modeInput, contexts, diffusion, pixels, levels, modes, errors, context);
            if (!correctDc)
            {
                // With DC diffusion disabled, incoming neighbor error is inert
                // and no selected error can escape into the next macroblock.
                Array.Fill(diffusion, (sbyte)31);
                Assert.Equal(expected, Run(source, modeInput, contexts, diffusion, pixels, levels, modes, errors, context));
                Assert.All(errors[..14], value => Assert.Equal((sbyte)0, value)); Array.Clear(diffusion);
            }
            var aliasPixels = new byte[386]; source.CopyTo(aliasPixels, 0); aliasPixels[384] = 97; aliasPixels[385] = 98;
            var aliasLevels = new short[402]; var aliasModes = new byte[18]; var aliasErrors = new sbyte[16];
            aliasLevels[400] = 1234; aliasLevels[401] = -1234; aliasModes[16] = 95; aliasModes[17] = 96; aliasErrors[14] = 93; aliasErrors[15] = 94;
            var actual = Run(aliasPixels, aliasModes, contexts, aliasErrors, aliasPixels, aliasLevels, aliasModes, aliasErrors, context);
            Assert.Equal(expected, actual); Assert.True(aliasPixels.AsSpan(0, 384).SequenceEqual(pixels.AsSpan(0, 384)));
            Assert.True(aliasLevels.AsSpan(0, 400).SequenceEqual(levels.AsSpan(0, 400))); Assert.True(aliasModes.AsSpan(0, 16).SequenceEqual(modes.AsSpan(0, 16))); Assert.True(aliasErrors.AsSpan(0, 14).SequenceEqual(errors.AsSpan(0, 14)));
            Assert.Equal((97, 98, 1234, -1234, 95, 96, 93, 94), ((int)aliasPixels[384], (int)aliasPixels[385], (int)aliasLevels[400], (int)aliasLevels[401], (int)aliasModes[16], (int)aliasModes[17], (int)aliasErrors[14], (int)aliasErrors[15]));
            var shared = Enumerable.Repeat((byte)55, 800).ToArray();
            Assert.Throws<ArgumentException>(() => Run(source, modeInput, contexts, diffusion, shared, MemoryMarshal.Cast<byte, short>(shared.AsSpan()), modes, errors, context));
            Assert.All(shared, value => Assert.Equal((byte)55, value));
            var invalidMode = new byte[8]; invalidMode[7] = 10;
            Assert.Throws<ArgumentException>(() => Run(source, invalidMode, contexts, diffusion, shared, levels, modes, errors, context));
            Assert.All(shared, value => Assert.Equal((byte)55, value));
            using var cancel = new CancellationTokenSource(); using var cancelled = new OperationContext(new() { MemoryBudget = root }, cancel.Token); cancel.Cancel();
            Assert.Throws<OperationCanceledException>(() => Run(source, modeInput, contexts, diffusion, shared, levels, modes, errors, cancelled));
            Assert.All(shared, value => Assert.Equal((byte)55, value));
            using var limited = new OperationContext(new() { MemoryBudget = root}, default);
            Assert.Equal(expected, Run(source, modeInput, contexts, diffusion, pixels, levels, modes, errors, context));
            Assert.Equal(originalSource, source); Assert.Equal(originalNeighbors, neighbors); Assert.Equal(retained, root.Bytes); Assert.False(root.IsDisposed);
        }
        finally { context.Release(Vp8MacroblockDecision.ScratchBytes); }
    }
}
