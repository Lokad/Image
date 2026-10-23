using System.Security.Cryptography;
using Lokad.Image.Internal;
using Lokad.Image.Tooling;
using static Lokad.Image.Tests.Vp8lReferencePredictorTests;

namespace Lokad.Image.Tests;

public sealed class Vp8lReferenceFrameTests
{
    private sealed class Prepared : IDisposable
    {
        internal Vp8lFramePrefix Prefix;
        internal Vp8lImageCode? Main;
        public void Dispose() { Main?.Dispose(); Prefix.Color?.Dispose(); Prefix.Prediction?.Dispose(); Prefix.Palette?.Dispose(); }
    }

    private static Vp8lImageCode Prediction(Vp8lPredictionPlan plan, int width, bool lowEffort, OperationContext context)
    {
        var reserved = plan.Modes.Length * 4L; context.Reserve(reserved);
        try
        {
            var pixels = new uint[plan.Modes.Length];
            for (var i = 0; i < pixels.Length; i++) pixels[i] = 0xff000000u | (uint)plan.Modes[i] << 8;
            return Vp8lImageCode.Auxiliary(pixels, (width + (1 << plan.StoredBits) - 1) >> plan.StoredBits, 75, lowEffort, context);
        }
        finally { context.Release(reserved); }
    }

    private static Prepared Asymmetric(int method, OperationContext context)
    {
        var result = new Prepared { Prefix = new(513, 259, true) { SubtractGreen = true } };
        var pixels = Working(Vp8lReferencePredictorTests.Asymmetric(), true, context);
        try
        {
            var low = method == 0; var bits = method == 4 ? 3 : 6;
            using var prediction = Vp8lReferencePredictor.SelectAndApply(pixels, 513, bits, bits, low, context);
            result.Prefix = result.Prefix with { Prediction = Prediction(prediction, 513, low, context), PredictionBits = prediction.StoredBits };
            if (!low)
            {
                using var color = Vp8lReferenceColorTransform.SelectAndApply(pixels, 513, bits, 75, context);
                result.Prefix = result.Prefix with { Color = Vp8lImageCode.Auxiliary(color.Coefficients, (513 + (1 << color.StoredBits) - 1) >> color.StoredBits, 75, false, context), ColorBits = color.StoredBits };
            }
            using var matches = Vp8lReferenceMatcher.Build(pixels, 513, 75, low, context);
            var tokens = low ? Vp8lReferenceLz.Greedy(pixels, matches.Values, context)
                : Vp8lReferenceMainParsing.Encode(pixels, matches.Values, 513, false, 10, context);
            result.Main = Vp8lImageCode.TakeMain(tokens, 513, 259, method == 0 ? 7 : method == 1 ? 6 : 3, low, context);
            return result;
        }
        catch { result.Dispose(); throw; }
        finally { context.Release(pixels.Length * 4L); }
    }

    private static Prepared Palette(string name, OperationContext context, bool spatial = false, bool box = false)
    {
        var width = name == "singleton" ? 1 : name == "two-color" ? 4103 : 23;
        var height = name == "singleton" ? 1 : name == "two-color" ? 2 : 13;
        var colors = name == "singleton" ? 1 : name == "two-color" ? 2 : 18;
        var source = name == "singleton" ? new byte[] { 255, 0, 1, 0 } : name == "two-color" ? new byte[width * height * 4] : PalettePackingInput.Create(width, height, colors);
        if (name == "two-color")
        { for (var i = 0; i < source.Length; i += 4) { source[i] = 48; source[i + 1] = 211; source[i + 2] = 255; } source[^4] = 77; }
        var result = new Prepared { Prefix = new(width, height, true) };
        var pixels = Working(source, false, context); var low = name == "numeric";
        try
        {
            var analysis = Vp8lEncodingAnalysis.Select(pixels, width, spatial ? 5 : low ? 0 : 4, context);
            using var palette = Vp8lReferencePalette.SelectAndPack(pixels, width, colors, !low, context);
            result.Prefix = result.Prefix with { Palette = Vp8lImageCode.Auxiliary(palette.EncodedDeltas, palette.EncodedColorCount, 20, low, context), PaletteEntries = palette.EncodedColorCount };
            var input = pixels.AsSpan(0, palette.PackedPixelCount);
            if (spatial)
            {
                var maximum = analysis.TransformBits;
                while (maximum > 2 && ((palette.CodedWidth + (1 << (maximum - 1)) - 1) >> (maximum - 1)) * ((height + (1 << (maximum - 1)) - 1) >> (maximum - 1)) == 1) maximum--;
                using var prediction = Vp8lReferencePredictor.SelectAndApply(input, palette.CodedWidth, Math.Max(2, maximum - 2), maximum, false, context);
                result.Prefix = result.Prefix with { Prediction = Prediction(prediction, palette.CodedWidth, false, context), PredictionBits = prediction.StoredBits };
            }
            using var matches = Vp8lReferenceMatcher.Build(input, palette.CodedWidth, 75, low, context);
            using var boxMatches = box ? Vp8lReferenceMatcher.BuildBox(input, palette.CodedWidth, matches.Values, context) : null;
            var tokens = low ? Vp8lReferenceLz.Greedy(input, matches.Values, context)
                : box ? Vp8lReferenceMainParsing.EncodeBox(input, boxMatches!.Values, palette.CodedWidth, false, System.Numerics.BitOperations.Log2((uint)colors) + 1, context)
                : Vp8lReferenceMainParsing.Encode(input, matches.Values, palette.CodedWidth, false, System.Numerics.BitOperations.Log2((uint)colors) + 1, context);
            result.Main = Vp8lImageCode.TakeMain(tokens, palette.CodedWidth, height, analysis.HistogramBits, low, context);
            return result;
        }
        catch { result.Dispose(); throw; }
        finally { context.Release(pixels.Length * 4L); }
    }

    private static byte[] Emit(Prepared prepared, OperationContext context)
    {
        var count = Vp8lReferenceFrame.CountPayload(prepared.Prefix, prepared.Main!, context);
        using var output = new MemoryStream(); Vp8lReferenceFrame.Write(output, prepared.Prefix, prepared.Main!, count, context);
        return output.ToArray();
    }

    // Full packets from the same seven frozen controls, without native/model
    // inputs. Test orchestration supplies typed production components directly;
    // public effort/transform-family routing is qualified at integration.
    [Theory]
    [InlineData(0, 316566, 9, "a83e601e8d0fd35d256dd92d682db09d6868a7f401466d8f13cb95b1fd6d150b")]
    [InlineData(1, 176216, 6, "f5a66ebfaff467b0f2409663753b3387b02b4f872e711c034869c328317b956d")]
    [InlineData(4, 183812, 3, "08ddac115f1770d778e6dbc76373440224b32e6f93b616605e234c63206eebde")]
    public void CompleteAsymmetricEffortPacketsMatchEveryNativeByte(int method, int length, int storedBits, string expected)
    {
        using var root = new TestMemoryBudget(128_000_000);
        using var context = new OperationContext(new() { MemoryBudget = root }, default);
        using var prepared = Asymmetric(method, context); var retained = root.Bytes;
        Assert.Equal(storedBits, prepared.Main!.StoredTileBits);
        var packet = Emit(prepared, context); var second = Emit(prepared, context);
        Assert.Equal(length, packet.Length); Assert.Equal(expected, Convert.ToHexStringLower(SHA256.HashData(packet)));
        Assert.Equal(packet, second); Assert.Equal(retained, root.Bytes);
        second[0] ^= 1; Assert.NotEqual(packet[0], second[0]); prepared.Dispose(); Assert.Equal(0, root.Bytes);
        Assert.Equal(expected, Convert.ToHexStringLower(SHA256.HashData(packet))); Assert.False(root.IsDisposed);
    }

    [Theory]
    [InlineData("singleton", 34, "98dc19987970964d8c3a2bd9e57c9e4cd4e29dc1e99fe8658b087f53aabcb618")]
    [InlineData("two-color", 46, "8f241022ea6585480f36d10a8d4c2cfefcc3e311ae6120b14867e34260aeb9bc")]
    [InlineData("greedy", 236, "a4bb56684758d4ff4fa58da15729a126098a74b228c54307204c95d7e4942736")]
    [InlineData("numeric", 268, "634ad1b52a2b14326ac2e3336e9e5cfb4848d2b94b24ca305b23df26211ed315")]
    public void CompletePackedPalettePacketsMatchEveryNativeByte(string name, int length, string expected)
    {
        using var root = new TestMemoryBudget(2_000_000);
        using var context = new OperationContext(new() { MemoryBudget = root }, default);
        using var prepared = Palette(name, context); var retained = root.Bytes;
        var packet = Emit(prepared, context); Assert.Equal(retained, root.Bytes);
        Assert.Equal(length, packet.Length); Assert.Equal(expected, Convert.ToHexStringLower(SHA256.HashData(packet)));
        prepared.Dispose(); Assert.Equal(0, root.Bytes); Assert.Equal(expected, Convert.ToHexStringLower(SHA256.HashData(packet)));
    }

    [Theory]
    [InlineData("greedy", false, 256, "2fbb3167c8da95e86dbec67aa0b5c853f4ed651e48bcdc8ebb70ce34cb75ac82")]
    [InlineData("two-color", true, 50, "c14334d5aba4d17c193a0a0026d4a2b95bb2f9f8d7dc557b8fa15c66ad155dca")]
    public void ExistingForcedSpatialAndBoxCandidatePacketsRemainByteExact(string name, bool box, int bytes, string expected)
    {
        using var root = new TestMemoryBudget(2_000_000);
        using var context = new OperationContext(new() { MemoryBudget = root }, default);
        using var prepared = Palette(name, context, true, box); var packet = Emit(prepared, context);
        Assert.Equal(bytes, packet.Length); Assert.Equal(expected, Convert.ToHexStringLower(SHA256.HashData(packet)));
        prepared.Dispose(); Assert.Equal(0, root.Bytes);
    }

    private static Vp8lTokenBuffer Single(OperationContext context)
    { var tokens = new Vp8lTokenBuffer(context, 1); tokens.Add(Vp8lToken.Literal(0x00ff0001)); return tokens; }

    [Theory]
    [InlineData(1)]
    [InlineData(3)]
    [InlineData(5)]
    public void MainPreparationConsumesOwnedTokensAndRefundsScratchMetadataOrCodeRefusal(int phase)
    {
        using var root = new TestMemoryBudget(100_000);
        using var context = new OperationContext(new() { MemoryBudget = root }, default);
        var tokens = Single(context); root.RefuseAtGrowth = root.GrowthAttempts + phase;
        var error = Assert.Throws<ImageLimitException>(() => Vp8lImageCode.TakeMain(tokens, 1, 1, 2, false, context));
        Assert.Equal((ImageLimitKind.WorkingBytes, ImageLimitScope.SharedBudget), (error.Limit, error.Scope));
        Assert.Equal(root.RefuseAtGrowth, root.GrowthAttempts); Assert.Equal(1, root.Refusals); Assert.Equal(0, root.Bytes);
        Assert.Throws<ObjectDisposedException>(() => tokens.Tokens.Length);
        root.RefuseAtGrowth = 0; using var next = Vp8lImageCode.TakeMain(Single(context), 1, 1, 2, false, context);
        next.Dispose(); Assert.Equal(0, root.Bytes); Assert.False(root.IsDisposed);
    }

    [Fact]
    public void CancellationDuringCountKeepsPreparedStateChargedUntilItsOwnerDisposes()
    {
        using var root = new TestMemoryBudget(2_000_000); using var cancellation = new CancellationTokenSource();
        using var context = new OperationContext(new() { MemoryBudget = root }, cancellation.Token);
        using var prepared = Palette("numeric", context); var retained = root.Bytes;
        root.Admitted = _ => cancellation.Cancel();
        Assert.Throws<OperationCanceledException>(() => Vp8lReferenceFrame.CountPayload(prepared.Prefix, prepared.Main!, context));
        Assert.True(cancellation.IsCancellationRequested); Assert.Equal(retained, root.Bytes); root.Admitted = null;
        prepared.Dispose(); Assert.Equal(0, root.Bytes);
        using var nextContext = new OperationContext(new() { MemoryBudget = root }, default);
        using var next = Palette("numeric", nextContext); Assert.Equal(268, Emit(next, nextContext).Length);
        next.Dispose(); Assert.Equal(0, root.Bytes); Assert.Equal(0, root.Refusals);
    }

    [Fact]
    public void CompleteOutputLimitIsCheckedBeforeWritingRiffAndDoesNotStealOutputOwnership()
    {
        using var root = new TestMemoryBudget(100_000);
        using var context = new OperationContext(new() { MemoryBudget = root, MaxOutputBytes = 33 }, default);
        using var prepared = Palette("singleton", context); var retained = root.Bytes;
        var payload = Vp8lReferenceFrame.CountPayload(prepared.Prefix, prepared.Main!, context);
        using var output = new MemoryStream();
        var error = Assert.Throws<ImageLimitException>(() => Vp8lReferenceFrame.Write(output, prepared.Prefix, prepared.Main!, payload, context));
        Assert.Equal(ImageLimitKind.OutputBytes, error.Limit); Assert.Equal(0, output.Length); Assert.True(output.CanWrite);
        Assert.Equal(retained, root.Bytes); prepared.Dispose(); Assert.Equal(0, root.Bytes);
    }

    [Fact]
    public void LateEmissionWorkFailureKeepsCallerStreamOpenAndReleasesOwnedStateOnDisposal()
    {
        long work;
        using (var calibration = new OperationContext(ImageLimits.Default, default))
        { using var ready = Palette("singleton", calibration); Emit(ready, calibration); work = calibration.ConsumedWork; }
        using var root = new TestMemoryBudget(100_000);
        using var context = new OperationContext(new() { MemoryBudget = root, MaxWork = work - 1 }, default);
        using var prepared = Palette("singleton", context); var retained = root.Bytes;
        var count = Vp8lReferenceFrame.CountPayload(prepared.Prefix, prepared.Main!, context);
        using var output = new MemoryStream();
        var error = Assert.Throws<ImageLimitException>(() => Vp8lReferenceFrame.Write(output, prepared.Prefix, prepared.Main!, count, context));
        Assert.Equal(ImageLimitKind.Work, error.Limit); Assert.True(output.CanWrite); Assert.True(output.Length >= 20);
        Assert.Equal(retained, root.Bytes); prepared.Dispose(); Assert.Equal(0, root.Bytes);
        using var nextContext = new OperationContext(new() { MemoryBudget = root }, default);
        using var next = Palette("singleton", nextContext); Assert.Equal(34, Emit(next, nextContext).Length);
        next.Dispose(); Assert.Equal(0, root.Bytes); Assert.False(root.IsDisposed);
    }
}
