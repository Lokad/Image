using System.Buffers.Binary;
using System.IO.Compression;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using Lokad.Image.Internal;
using Lokad.Image.Tooling;

namespace Lokad.Image.Tests;

public sealed class Vp8DistortionDecisionTests
{
    private static string Hash(ReadOnlySpan<byte> bytes) => Convert.ToHexStringLower(SHA256.HashData(bytes));
    private static void Before(Span<byte> bytes, int call, int x, int y, int segment, bool intra16, int mode, int uv,
        ReadOnlySpan<byte> source, ReadOnlySpan<byte> edges, ReadOnlySpan<byte> contexts, ReadOnlySpan<sbyte> diffusion)
    {
        "BEF8"u8.CopyTo(bytes); var at = 4;
        foreach (var n in new[] { call, x, y, 0, segment, intra16 ? 1 : 0, mode, uv })
        { BinaryPrimitives.WriteInt32LittleEndian(bytes[at..], n); at += 4; }
        source.CopyTo(bytes[36..]); edges.CopyTo(bytes[420..]); contexts.CopyTo(bytes[491..]);
        BinaryPrimitives.WriteInt32LittleEndian(bytes[499..], 1); MemoryMarshal.AsBytes(diffusion).CopyTo(bytes[503..]);
    }
    private static void After(Span<byte> bytes, int call, Vp8MacroblockScore score, ReadOnlySpan<byte> modes,
        ReadOnlySpan<short> levels, ReadOnlySpan<byte> pixels, ReadOnlySpan<sbyte> errors)
    {
        "AFT8"u8.CopyTo(bytes); var at = 4;
        foreach (var n in new[] { call, score.Nonzero == 0 ? 1 : 0, score.Intra16 ? 1 : 0, score.LumaMode, score.ChromaMode, (int)score.Nonzero })
        { BinaryPrimitives.WriteInt32LittleEndian(bytes[at..], n); at += 4; }
        foreach (var n in new[] { score.Distortion, score.Texture, score.Header, score.Rate, score.Total })
        { BinaryPrimitives.WriteInt64LittleEndian(bytes[at..], n); at += 8; }
        modes[..16].CopyTo(bytes[68..]);
        for (var i = 0; i < 400; i++) BinaryPrimitives.WriteInt16LittleEndian(bytes[(84 + i * 2)..], levels[i]);
        pixels[..384].CopyTo(bytes[884..]); MemoryMarshal.AsBytes(errors[..14]).CopyTo(bytes[1268..]);
    }
    private static string Parameters(Vp8SourceSegments segments, Vp8QuantizationPlan quantization, int width, int height)
    {
        Span<byte> bytes = stackalloc byte[4016]; "PAR8"u8.CopyTo(bytes); var at = 4;
        foreach (var n in new[] { width, height, 65536 }) { BinaryPrimitives.WriteInt32LittleEndian(bytes[at..], n); at += 4; }
        for (var s = 0; s < 4; s++)
        {
            var p = segments.Parameters[s]; var c = quantization.Costs(s);
            foreach (var n in new[] { p.Quantizer, p.InitialFilter, c.Luma4, c.Luma16, c.Chroma, c.Mode, c.Texture, c.MinimumDistortion })
            { BinaryPrimitives.WriteInt32LittleEndian(bytes[at..], n); at += 4; }
            BinaryPrimitives.WriteInt64LittleEndian(bytes[at..], c.Intra4Penalty); at += 8;
            for (var plane = 0; plane < 3; plane++) foreach (var n in quantization.Matrix(s, (Vp8CoefficientPlane)plane).Values)
            { BinaryPrimitives.WriteInt32LittleEndian(bytes[at..], n); at += 4; }
        }
        Assert.Equal(bytes.Length, at); return Hash(bytes);
    }

    // Original diagram pixels and self-generated rolling reconstruction only.
    // Native hashes cover all decisions in the statistics AND final pass.
    // Coefficient NZ contexts are not used by this primitive and are excluded.
    [Theory]
    [InlineData(0, "6c7f700dc2407785c6c6997c92a7e92ee32abf02c002b05fe57e19bf5860c878", "a35c269369869f5c2992449357bb759b3c2acaf900f792653f6510bda6fd63eb", "aef574014b86dcb1f7f8d9242d6294b0c9d92b71af8dd671ba7067ca035fe874", "85dcc7a9cb7bf71278180fa89a5db70abfe3738945ea268da705cfcb251263be", "8948c6ff4f09d7644c837fde90ac08350652e5058b8f5ebfa27856708a1c1a19")]
    [InlineData(1, "584520e2ecc90a57e89731dd674597224d5cf01bb69e34433ffb32b3db26eee8", "1321e430af726b2fa9e669b293d81eb55a02e5bca46f0b552ab4dc1e1f8f9f6c", "aef574014b86dcb1f7f8d9242d6294b0c9d92b71af8dd671ba7067ca035fe874", "656c1187843b1daef7c9481fb680fa9e0ff452c0d2a927fbd3f5c1a7c0493feb", "ad9167d55c222f7a71b06b05d5e02a934b0af95e706192a27b81ad494bf8e102")]
    [InlineData(2, "9a44b3a4c502a47b339e5d96ee07963410779b3f4948caa411194052bc7dc6c7", "460fa9a22d62f776d44b2bdc7e1435bbb4d43bec7cd4b6995075878853c8d0eb", "888dc0adcc8659b8000102583680defdf2568427958a929a7696127d74268849", "d5c21a65f122a51b7a59dff9457b4df366db25ad13496777a8ae7db7967a7b54", "646db0f1c004f33730add34a6f4e9bab68700a67fdd12a54675e8ea2725962cf")]
    public void StatisticsAndFinalPassDecisionsMatchNative(int method, string expectedBefore, string expectedAfter, string expectedParameters, string expectedStatisticsBefore, string expectedProfiles)
    {
        using var root = new TestMemoryBudget(16_000_000); using var image = OriginalVp8lControls.Buffer("diagram", 8, root);
        using var context = new OperationContext(new() { MemoryBudget = root }, default); using var input = Vp8PreparedInput.Create(image, context);
        using var analysis = Vp8SourceAnalysis.Create(input, context, method); using var segments = Vp8SourceSegments.Create(analysis, 75, false, context);
        using var quantization = Vp8QuantizationPlan.Create(segments, analysis.MeanChromaScore, context, method);
        using var rate = Vp8RatePlan.Create(Vp8Tables.CoefficientProbabilities, context);
        var columns = input.Stride / 16; var rows = input.StorageHeight / 16; var count = columns * rows;
        Assert.Equal(expectedParameters, Parameters(segments, quantization, columns, rows));
        context.Reserve(Vp8DistortionDecision.ScratchBytes + 32_768);
        try
        {
            var sourceTypes = new bool[count]; var sourceModes = new byte[count]; var uvModes = analysis.ChromaModes.ToArray();
            for (var i = 0; i < count; i++) { sourceTypes[i] = analysis.LumaModes[i] != 2; sourceModes[i] = sourceTypes[i] ? analysis.LumaModes[i] : (byte)0; }
            var statistics = new uint[1056]; var probabilities = Vp8Tables.CoefficientProbabilities.ToArray(); var nonzero = new uint[columns + 1];
            var yTop = new byte[columns * 16]; var uvTop = new byte[columns * 16]; var modeTop = new byte[columns * 4];
            Span<byte> source = stackalloc byte[384]; Span<byte> edges = stackalloc byte[71]; Span<byte> contexts = stackalloc byte[8];
            Span<byte> modeLeft = stackalloc byte[4]; Span<sbyte> diffusion = stackalloc sbyte[8]; diffusion.Clear();
            Span<byte> pixels = stackalloc byte[386]; Span<short> levels = stackalloc short[402]; Span<byte> modes = stackalloc byte[18]; Span<sbyte> errors = stackalloc sbyte[16];
            pixels[384] = 97; pixels[385] = 98; levels[400] = -30000; levels[401] = 30000; modes[16] = 95; modes[17] = 96; errors[14] = 93; errors[15] = 94;
            Span<byte> before = stackalloc byte[511]; Span<byte> after = stackalloc byte[1282]; Span<byte> nzContexts = stackalloc byte[18];
            Span<byte> fullBefore = stackalloc byte[523]; Span<byte> profile = stackalloc byte[1064];
            using var statBefore = IncrementalHash.CreateHash(HashAlgorithmName.SHA256); using var profileHash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
            using var pre = IncrementalHash.CreateHash(HashAlgorithmName.SHA256); using var post = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
            using var trace = Environment.GetEnvironmentVariable("LOKAD_VP8_DISTORTION_TRACE") is { } path
                ? new GZipStream(File.Create(path + "-m" + method + ".bin.gz"), CompressionLevel.Fastest) : null;
            var call = 0; "PRB8"u8.CopyTo(profile); BinaryPrimitives.WriteInt32LittleEndian(profile[4..], 0); probabilities.CopyTo(profile[8..]); profileHash.AppendData(profile);
            for (var pass = 0; pass < 2; pass++)
            {
                yTop.AsSpan().Fill(127); uvTop.AsSpan().Fill(127); modeTop.AsSpan().Clear(); nonzero.AsSpan().Clear(); byte leftDc = 0;
                var length = pass == 0 && method == 0 ? count / 4 : count;
                for (var index = 0; index < length; index++, call++)
                {
                    context.Work(4096); var x = index % columns; var y = index / columns;
                    if (x == 0) { edges[..35].Fill(129); edges[0] = edges[17] = edges[26] = y == 0 ? (byte)127 : (byte)129; modeLeft.Clear(); leftDc = 0; }
                    for (var yy = 0; yy < 16; yy++) input.Y.Slice((y * 16 + yy) * input.Stride + x * 16, 16).CopyTo(source.Slice(yy * 16, 16));
                    for (var yy = 0; yy < 8; yy++)
                    {
                        input.U.Slice((y * 8 + yy) * (input.Stride / 2) + x * 8, 8).CopyTo(source.Slice(256 + yy * 16, 8));
                        input.V.Slice((y * 8 + yy) * (input.Stride / 2) + x * 8, 8).CopyTo(source.Slice(264 + yy * 16, 8));
                    }
                    yTop.AsSpan(x * 16, 16).CopyTo(edges[35..]); uvTop.AsSpan(x * 16, 16).CopyTo(edges[51..]);
                    for (var p = 0; p < 4; p++) edges[67 + p] = x + 1 < columns ? yTop[(x + 1) * 16 + p] : yTop[x * 16 + 15];
                    modeTop.AsSpan(x * 4, 4).CopyTo(contexts); modeLeft.CopyTo(contexts[4..]); var segment = segments.Map[index];
                    Before(before, call, x, y, segment, sourceTypes[index], sourceModes[index], uvModes[index], source, edges, contexts, diffusion);
                    if (pass == 0)
                    {
                        Contexts(nonzero[x + 1], nonzero[x], leftDc, nzContexts);
                        before[..499].CopyTo(fullBefore); BinaryPrimitives.WriteUInt32LittleEndian(fullBefore[499..], nonzero[x + 1]);
                        BinaryPrimitives.WriteUInt32LittleEndian(fullBefore[503..], nonzero[x]); BinaryPrimitives.WriteInt32LittleEndian(fullBefore[507..], leftDc);
                        before[499..].CopyTo(fullBefore[511..]); statBefore.AppendData(fullBefore);
                    }
                    var retained = root.Bytes;
                    var score = Vp8DistortionDecision.Select(source, edges, contexts, diffusion, y != 0, x != 0, method, sourceTypes[index], uvModes[index],
                        256L * 510 * 8 * 1024 / count, true, quantization.Matrix(segment, Vp8CoefficientPlane.Luma), quantization.Matrix(segment, Vp8CoefficientPlane.Walsh),
                        quantization.Matrix(segment, Vp8CoefficientPlane.Chroma), quantization.Costs(segment), rate, pixels, levels, modes, errors, context);
                    Assert.Equal(retained, root.Bytes); After(after, call, score, modes, levels, pixels, errors);
                    pre.AppendData(before); post.AppendData(after); trace?.Write(before); trace?.Write(after);
                    if (pass == 0)
                    {
                        Vp8FramePlan.Record(levels, score.Intra16, nzContexts, statistics, tokenBuffer: false);
                        nonzero[x + 1] = Pack(nzContexts); leftDc = nzContexts[17];
                    }
                    sourceTypes[index] = score.Intra16; sourceModes[index] = score.Intra16 ? (byte)score.LumaMode : modes[0]; uvModes[index] = (byte)score.ChromaMode;
                    for (var p = 0; p < 4; p++)
                    {
                        modeTop[x * 4 + p] = score.Intra16 ? (byte)score.LumaMode : modes[12 + p]; modeLeft[p] = score.Intra16 ? (byte)score.LumaMode : modes[p * 4 + 3];
                    }
                    if (x + 1 < columns)
                    {
                        edges[0] = edges[50]; edges[17] = edges[58]; edges[26] = edges[66];
                        for (var p = 0; p < 16; p++) edges[1 + p] = pixels[p * 16 + 15];
                        for (var p = 0; p < 8; p++) { edges[18 + p] = pixels[256 + p * 16 + 7]; edges[27 + p] = pixels[256 + p * 16 + 15]; }
                    }
                    if (y + 1 < rows) { pixels.Slice(240, 16).CopyTo(yTop.AsSpan(x * 16, 16)); pixels.Slice(368, 16).CopyTo(uvTop.AsSpan(x * 16, 16)); }
                    foreach (var error in errors.Slice(6, 8)) Assert.Equal((sbyte)0, error);
                }
                if (pass == 0)
                {
                    context.Work(1056 * 16L); Vp8TokenStatistics.Update(statistics, rate.Entropy, probabilities, out var changed); Assert.True(changed);
                    "PRB8"u8.CopyTo(profile); BinaryPrimitives.WriteInt32LittleEndian(profile[4..], call); probabilities.CopyTo(profile[8..]); profileHash.AppendData(profile);
                }
            }
            Assert.Equal(expectedStatisticsBefore, Convert.ToHexStringLower(statBefore.GetHashAndReset()));
            Assert.Equal(expectedProfiles, Convert.ToHexStringLower(profileHash.GetHashAndReset()));
            Assert.Equal(method == 0 ? 701 : 1122, call);
            Assert.Equal(expectedBefore, Convert.ToHexStringLower(pre.GetHashAndReset())); Assert.Equal(expectedAfter, Convert.ToHexStringLower(post.GetHashAndReset()));
            Assert.Equal((97, 98, -30000, 30000, 95, 96, 93, 94), ((int)pixels[384], (int)pixels[385], (int)levels[400], (int)levels[401], (int)modes[16], (int)modes[17], (int)errors[14], (int)errors[15]));
        }
        finally { context.Release(Vp8DistortionDecision.ScratchBytes + 32_768); }
    }

    private static void Contexts(uint top, uint left, byte dc, Span<byte> values)
    {
        ReadOnlySpan<int> above = [12, 13, 14, 15, 18, 19, 22, 23, 24];
        ReadOnlySpan<int> side = [3, 7, 11, 15, 17, 19, 21, 23];
        for (var p = 0; p < 9; p++) values[p] = (byte)(top >> above[p] & 1);
        for (var p = 0; p < 8; p++) values[9 + p] = (byte)(left >> side[p] & 1);
        values[17] = dc;
    }
    private static uint Pack(ReadOnlySpan<byte> values)
    {
        ReadOnlySpan<int> above = [12, 13, 14, 15, 18, 19, 22, 23, 24]; uint packed = 0;
        for (var p = 0; p < 9; p++) packed |= (uint)values[p] << above[p];
        return packed | (uint)values[9] << 3 | (uint)values[10] << 7 | (uint)values[11] << 11 | (uint)values[13] << 17 | (uint)values[15] << 21;
    }

    private sealed class Fixture : IDisposable
    {
        internal readonly TestMemoryBudget Root = new(2_000_000);
        internal readonly OperationContext Context;
        private readonly ImageBuffer image;
        private readonly Vp8PreparedInput input;
        private readonly Vp8SourceAnalysis analysis;
        internal readonly Vp8SourceSegments Segments;
        internal readonly Vp8QuantizationPlan Quantization;
        private readonly Vp8RatePlan rate;
        internal readonly byte[] Source = new byte[384], Edges = new byte[71];
        internal Fixture()
        {
            Context = new(new() { MemoryBudget = Root }, default); image = OriginalVp8lControls.Buffer("hidden", 8, Root);
            input = Vp8PreparedInput.Create(image, Context); input.CleanupTransparentArea(); analysis = Vp8SourceAnalysis.Create(input, Context, 0);
            Segments = Vp8SourceSegments.Create(analysis, 75, false, Context); Quantization = Vp8QuantizationPlan.Create(Segments, analysis.MeanChromaScore, Context, 0);
            rate = Vp8RatePlan.Create(Vp8Tables.CoefficientProbabilities, Context);
            for (var y = 0; y < 16; y++) input.Y.Slice(y * input.Stride, 16).CopyTo(Source.AsSpan(y * 16, 16));
            for (var y = 0; y < 8; y++) { input.U.Slice(y * input.Stride / 2, 8).CopyTo(Source.AsSpan(256 + y * 16, 8)); input.V.Slice(y * input.Stride / 2, 8).CopyTo(Source.AsSpan(264 + y * 16, 8)); }
            Edges.AsSpan(0, 35).Fill(129); Edges[0] = Edges[17] = Edges[26] = 127; Edges.AsSpan(35).Fill(127);
        }
        internal Vp8MacroblockScore Run(int method, bool intra16, ReadOnlySpan<byte> source, ReadOnlySpan<byte> modesIn, ReadOnlySpan<sbyte> errorsIn,
            Span<byte> pixels, Span<short> levels, Span<byte> modesOut, Span<sbyte> errorsOut, OperationContext context)
        {
            var segment = Segments.Map[0];
            return Vp8DistortionDecision.Select(source, Edges, modesIn, errorsIn, false, false, method, intra16, 0, 1000000, true,
                Quantization.Matrix(segment, Vp8CoefficientPlane.Luma), Quantization.Matrix(segment, Vp8CoefficientPlane.Walsh), Quantization.Matrix(segment, Vp8CoefficientPlane.Chroma),
                Quantization.Costs(segment), rate, pixels, levels, modesOut, errorsOut, context);
        }
        public void Dispose() { rate.Dispose(); Quantization.Dispose(); Segments.Dispose(); analysis.Dispose(); input.Dispose(); image.Dispose(); Context.Dispose(); Root.Dispose(); }
    }

    [Theory]
    [InlineData(0, false)]
    [InlineData(1, true)]
    [InlineData(2, true)]
    public void BorrowedInputsCanAliasResultsWithoutNewOwnershipOrTailWrites(int method, bool intra16)
    {
        using var f = new Fixture(); var original = f.Source.ToArray(); var originalEdges = f.Edges.ToArray();
        var modeInput = new byte[8]; var diffusion = new sbyte[8]; var pixels = new byte[386]; var levels = new short[402]; var modes = new byte[18]; var errors = new sbyte[16];
        f.Context.Reserve(Vp8DistortionDecision.ScratchBytes);
        try
        {
            var retained = f.Root.Bytes; var attempts = f.Root.Attempts;
            var expected = f.Run(method, intra16, f.Source, modeInput, diffusion, pixels, levels, modes, errors, f.Context);
            var aliased = new byte[386]; f.Source.CopyTo(aliased, 0); aliased[384] = 97; aliased[385] = 98;
            var aliasLevels = new short[402]; var aliasModes = new byte[18]; var aliasErrors = new sbyte[16];
            aliasLevels[400] = -30000; aliasLevels[401] = 30000; aliasModes[16] = 95; aliasModes[17] = 96; aliasErrors[14] = 93; aliasErrors[15] = 94;
            var actual = f.Run(method, intra16, aliased, aliasModes, aliasErrors, aliased, aliasLevels, aliasModes, aliasErrors, f.Context);
            Assert.Equal(expected, actual); Assert.True(aliased.AsSpan(0, 384).SequenceEqual(pixels.AsSpan(0, 384)));
            Assert.True(aliasLevels.AsSpan(0, 400).SequenceEqual(levels.AsSpan(0, 400))); Assert.True(aliasModes.AsSpan(0, 16).SequenceEqual(modes.AsSpan(0, 16)));
            Assert.True(aliasErrors.AsSpan(0, 14).SequenceEqual(errors.AsSpan(0, 14))); Assert.Equal(original, f.Source); Assert.Equal(originalEdges, f.Edges);
            Assert.Equal(retained, f.Root.Bytes); Assert.Equal(attempts, f.Root.Attempts);
            Assert.Equal((97, 98, -30000, 30000, 95, 96, 93, 94), ((int)aliased[384], (int)aliased[385], (int)aliasLevels[400], (int)aliasLevels[401], (int)aliasModes[16], (int)aliasModes[17], (int)aliasErrors[14], (int)aliasErrors[15]));
        }
        finally { f.Context.Release(Vp8DistortionDecision.ScratchBytes); }
    }

    [Fact]
    public void InvalidModesOptionsOrOverlappingResultsRejectBeforeWrites()
    {
        using var f = new Fixture(); var modes = new byte[18]; modes[7] = 10; var errors = new sbyte[16]; var pixels = Enumerable.Repeat((byte)88, 800).ToArray(); var levels = new short[400];
        var before = f.Root.Bytes; var attempts = f.Root.Attempts;
        Assert.Throws<ArgumentException>(() => f.Run(0, true, f.Source, modes, errors, pixels, levels, new byte[16], errors, f.Context));
        modes[7] = 0;
        Assert.Throws<ArgumentException>(() => f.Run(0, true, f.Source, modes, errors, pixels, MemoryMarshal.Cast<byte, short>(pixels.AsSpan()), modes, errors, f.Context));
        Assert.Throws<ArgumentOutOfRangeException>(() => f.Run(3, true, f.Source, modes, errors, pixels, levels, modes, errors, f.Context));
        Assert.Throws<ArgumentOutOfRangeException>(() => Vp8QuantizationPlan.Create(f.Segments, 42, f.Context, -1));
        Assert.Throws<ArgumentOutOfRangeException>(() => Vp8QuantizationPlan.Create(f.Segments, 42, f.Context, 7));
        Assert.All(pixels, value => Assert.Equal((byte)88, value)); Assert.Equal(before, f.Root.Bytes); Assert.Equal(attempts, f.Root.Attempts);
    }

    [Fact]
    public void CallerScratchAdmissionCancellationAndLateWorkFailurePreserveInputsAndRecover()
    {
        using var f = new Fixture(); var original = f.Source.ToArray(); var modes = new byte[18]; var errors = new sbyte[16]; var pixels = new byte[384]; var levels = new short[400];
        var before = f.Root.Bytes; using var caller = f.Root.CreateScope();
        Assert.True(caller.TryReserve(2_000_000 - before - Vp8DistortionDecision.ScratchBytes + 1, out var held));
        Assert.Equal(ImageLimitScope.SharedBudget, Assert.Throws<ImageLimitException>(() => f.Context.Reserve(Vp8DistortionDecision.ScratchBytes)).Scope);
        Assert.Equal(before + held.Bytes, f.Root.Bytes); held.Dispose();
        using var cancellation = new CancellationTokenSource(); using var stopped = new OperationContext(new() { MemoryBudget = f.Root }, cancellation.Token);
        stopped.Reserve(Vp8DistortionDecision.ScratchBytes); cancellation.Cancel();
        Assert.Throws<OperationCanceledException>(() => f.Run(0, true, f.Source, modes, errors, pixels, levels, modes, errors, stopped));
        stopped.Release(Vp8DistortionDecision.ScratchBytes); Assert.Equal(before, f.Root.Bytes);
        long work;
        using (var sizing = new OperationContext(new() { MemoryBudget = f.Root }, default))
        {
            sizing.Reserve(Vp8DistortionDecision.ScratchBytes); f.Run(0, true, f.Source, modes, errors, pixels, levels, modes, errors, sizing); work = sizing.ConsumedWork;
            sizing.Release(Vp8DistortionDecision.ScratchBytes);
        }
        using var limited = new OperationContext(new() { MemoryBudget = f.Root, MaxWork = work - 1 }, default); limited.Reserve(Vp8DistortionDecision.ScratchBytes);
        Assert.Equal(ImageLimitKind.Work, Assert.Throws<ImageLimitException>(() => f.Run(0, true, f.Source, modes, errors, pixels, levels, modes, errors, limited)).Limit);
        limited.Release(Vp8DistortionDecision.ScratchBytes); Assert.Equal(before, f.Root.Bytes); Assert.Equal(original, f.Source);
        f.Context.Reserve(Vp8DistortionDecision.ScratchBytes); f.Run(0, true, f.Source, modes, errors, pixels, levels, modes, errors, f.Context);
        f.Context.Release(Vp8DistortionDecision.ScratchBytes); Assert.Equal(before, f.Root.Bytes); Assert.False(f.Root.IsDisposed);
    }

}
