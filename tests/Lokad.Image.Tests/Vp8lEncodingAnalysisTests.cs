using Lokad.Image.Internal;
using Lokad.Image.Tooling;
using static Lokad.Image.Tests.Vp8lReferencePredictorTests;

namespace Lokad.Image.Tests;

public sealed class Vp8lEncodingAnalysisTests
{
    private static byte[] Fixture(string name)
    {
        for (var directory = new DirectoryInfo(AppContext.BaseDirectory); directory is not null; directory = directory.Parent)
        {
            var path = Path.Combine(directory.FullName, "benchmarks/Fixtures/Compression/" + name + ".png");
            if (!File.Exists(path)) continue;
            using var image = ImageCodec.Decode(File.ReadAllBytes(path)); return OriginalPixels.Bytes(image);
        }
        throw new InvalidOperationException("Original compression fixture missing.");
    }

    private static (byte[] Rgba, int Width) Input(string name)
    {
        if (name == "asymmetric") return (Asymmetric(), 513);
        if (name == "opaque")
        {
            var bytes = Asymmetric(); for (var at = 3; at < bytes.Length; at += 4) bytes[at] = 255; return (bytes, 513);
        }
        if (name is "photograph" or "diagram") return (Fixture(name), 513);
        if (name == "hidden") return (HiddenRgb(), 19);
        if (name == "singleton") return ([255, 0, 1, 0], 1);
        if (name == "two-color")
        {
            var bytes = new byte[4103 * 2 * 4];
            for (var at = 0; at < bytes.Length; at += 4) { bytes[at] = 48; bytes[at + 1] = 211; bytes[at + 2] = 255; }
            bytes[^4] = 77; return (bytes, 4103);
        }
        if (name == "palette18") return (PalettePackingInput.Create(23, 13, 18), 23);
        if (name != "direct") throw new ArgumentException("Unknown original control.");
        var noise = new byte[129 * 129 * 4]; uint state = 0x9e3779b9u;
        for (var at = 0; at < noise.Length; at += 4)
        {
            state ^= state << 13; state ^= state >> 17; state ^= state << 5;
            noise[at] = (byte)state; noise[at + 1] = (byte)(state >> 8); noise[at + 2] = (byte)(state >> 16); noise[at + 3] = 255;
        }
        return (noise, 129);
    }

    // Decisions from the existing byte-exact complete-packet model controls.
    // These are the existing five transform families and declared effort paths,
    // not an additional image/settings benchmark matrix.
    [Theory]
    [InlineData("asymmetric", 0, (int)Vp8lTransformKind.PredictionSubtractGreen, false, true, 257, 7, 6)]
    [InlineData("asymmetric", 1, (int)Vp8lTransformKind.PredictionSubtractGreen, false, true, 257, 6, 6)]
    [InlineData("asymmetric", 4, (int)Vp8lTransformKind.PredictionSubtractGreen, false, true, 257, 3, 3)]
    [InlineData("asymmetric", 5, (int)Vp8lTransformKind.PredictionSubtractGreen, false, true, 257, 3, 3)]
    [InlineData("opaque", 4, (int)Vp8lTransformKind.PredictionSubtractGreen, false, false, 257, 3, 3)]
    [InlineData("photograph", 5, (int)Vp8lTransformKind.PredictionSubtractGreen, false, false, 257, 3, 3)]
    [InlineData("diagram", 4, (int)Vp8lTransformKind.SubtractGreen, false, false, 257, 3, 3)]
    [InlineData("hidden", 4, (int)Vp8lTransformKind.Prediction, false, true, 133, 5, 5)]
    [InlineData("direct", 4, (int)Vp8lTransformKind.Direct, false, false, 257, 3, 3)]
    [InlineData("singleton", 4, (int)Vp8lTransformKind.Palette, true, true, 1, 2, 2)]
    [InlineData("two-color", 4, (int)Vp8lTransformKind.Palette, true, true, 2, 5, 5)]
    [InlineData("palette18", 4, (int)Vp8lTransformKind.Palette, false, true, 18, 5, 5)]
    [InlineData("palette18", 0, (int)Vp8lTransformKind.Palette, true, true, 18, 5, 5)]
    public void ExistingPacketControlsKeepTheirTransformDecisions(string name, int effort,
        int kind, bool zero, bool alpha, int distinct, int histogramBits, int transformBits)
    {
        var (rgba, width) = Input(name);
        using var root = new TestMemoryBudget(2_000_000);
        using var context = new OperationContext(new() { MemoryBudget = root }, default);
        var pixels = Working(rgba, false, context); var original = pixels.ToArray();
        var result = Vp8lEncodingAnalysis.Select(pixels, width, effort, context);
        Assert.Equal(new((Vp8lTransformKind)kind, zero, alpha, distinct, histogramBits, transformBits), result);
        Assert.Equal(original, pixels); Assert.Equal(rgba.Length, root.Bytes); Assert.False(root.IsDisposed);
    }

    [Theory]
    [InlineData(256, 5)]
    [InlineData(257, 3)]
    public void ExactPaletteCardinalityBoundaryUsesExistingAxisSamples(int count, int histogramBits)
    {
        // Prefixes of the original axis recipe; a cardinality unit check, with
        // no native encoder packet or new benchmark claim at these dimensions.
        using var root = new TestMemoryBudget(100_000);
        using var context = new OperationContext(new() { MemoryBudget = root }, default);
        context.Reserve(count * 4L); var pixels = new uint[count];
        for (var i = 0; i < count; i++)
            pixels[i] = 0xff000000u | (uint)(i & 255) << 16 | (uint)(i >> 8) << 8 | (uint)((13 * i + (i >> 2)) & 255);
        var result = Vp8lEncodingAnalysis.Select(pixels, count, 4, context);
        Assert.Equal(count, result.DistinctColorsCapped); Assert.Equal(histogramBits, result.HistogramBits);
        Assert.False(result.HasNonOpaqueAlpha); Assert.Equal(count * 4, root.Bytes);
    }

    [Fact]
    public void SharedAdmissionRefusalIsTerminalAndReleasedPeerCapacityAllowsRecovery()
    {
        var rgba = HiddenRgb();
        using var root = new TestMemoryBudget(rgba.Length + 16128);
        using var context = new OperationContext(new() { MemoryBudget = root }, default);
        var pixels = Working(rgba, false, context); var original = pixels.ToArray();
        using var peer = root.CreateScope(); Assert.True(peer.TryReserve(1, out var held));
        var error = Assert.Throws<ImageLimitException>(() => Vp8lEncodingAnalysis.Select(pixels, 19, 4, context));
        Assert.Equal((ImageLimitKind.WorkingBytes, ImageLimitScope.SharedBudget), (error.Limit, error.Scope));
        Assert.Equal(1, root.Refusals); Assert.Equal(rgba.Length + 1, root.Bytes); Assert.Equal(original, pixels);
        held.Dispose();
        var result = Vp8lEncodingAnalysis.Select(pixels, 19, 4, context);
        Assert.Equal(Vp8lTransformKind.Prediction, result.Transform); Assert.Equal(rgba.Length, root.Bytes);
        Assert.Equal(rgba.Length + 16128, root.Peak); Assert.False(root.IsDisposed);
    }

    [Fact]
    public void CancellationAfterAdmissionRefundsAllScratchAndPreservesBorrowedPixels()
    {
        var rgba = HiddenRgb();
        using var root = new TestMemoryBudget(100_000); using var cancellation = new CancellationTokenSource();
        using var context = new OperationContext(new() { MemoryBudget = root }, cancellation.Token);
        var pixels = Working(rgba, false, context); var original = pixels.ToArray();
        root.Admitted = bytes => { if (bytes > rgba.Length) cancellation.Cancel(); };
        Assert.Throws<OperationCanceledException>(() => Vp8lEncodingAnalysis.Select(pixels, 19, 4, context));
        Assert.Equal(rgba.Length, root.Bytes); Assert.Equal(original, pixels); root.Admitted = null;
        using var nextContext = new OperationContext(new() { MemoryBudget = root }, default);
        Assert.Equal(Vp8lTransformKind.Prediction, Vp8lEncodingAnalysis.Select(pixels, 19, 4, nextContext).Transform);
        Assert.Equal(rgba.Length, root.Bytes); Assert.Equal(0, root.Refusals);
    }

    [Fact]
    public void LocalWorkLimitRemainsIndependentAndRefundsScratch()
    {
        var rgba = HiddenRgb();
        using var root = new TestMemoryBudget(100_000);
        using var context = new OperationContext(new() { MemoryBudget = root, MaxWork = 2 }, default);
        var pixels = Working(rgba, false, context);
        var error = Assert.Throws<ImageLimitException>(() => Vp8lEncodingAnalysis.Select(pixels, 19, 4, context));
        Assert.Equal((ImageLimitKind.Work, ImageLimitScope.Operation), (error.Limit, error.Scope));
        Assert.Equal(rgba.Length, root.Bytes); Assert.Equal(0, root.Refusals);
    }
}
