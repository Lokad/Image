using System.Security.Cryptography;
using Lokad.Image.Internal;
using Lokad.Image.Tooling;
using static Lokad.Image.Tests.Vp8lReferencePredictorTests;

namespace Lokad.Image.Tests;

public sealed class Vp8lSearchCandidateTests
{
    internal static uint[] AlphaWorking(int filter, OperationContext context)
    {
        var source = OriginalVp8lControls.Input("asymmetric");
        context.Reserve(source.Width * source.Height * 4L);
        try
        {
var result = new uint[source.Width * source.Height];
            for (var i = 0; i < result.Length; i++)
            {
var prediction = filter == 0 || i == 0 ? 0
                    : source.Pixels[(i < source.Width || i % source.Width != 0 ? i - 1 : i - source.Width) * 4 + 3];
                result[i] = (uint)((source.Pixels[i * 4 + 3] - prediction) & 255) << 8;
            }
            return result;
        }
        catch { context.Release(source.Width * source.Height * 4L); throw; }
    }

    // Each body is a cloned/flushed native writer from the existing ALPH6
    // control, accepted only after the complete85796B packet remained exact.
    [Theory]
    [InlineData(0, 0, 73008, "d3aca8b6d8cc0a10efe35aa7c558c7be03cd83415c2af4e7993b9f74b5cfc528")]
    [InlineData(0, 1, 43118, "a3aa05aafbc564066a7c470a492ed4dc2fef40bb68246738885b5dfef676ee91")]
    [InlineData(0, 2, 86904, "2438b4e0de721291acf3e76d6a462e73f1a306fb9d99273719353897f91465b3")]
    [InlineData(0, 3, 55635, "c541334551cffa0a05f2beeb572ace7f4a2cf4ff430d79fbf69433e94df82aab")]
    [InlineData(0, 4, 73020, "e25a10bdd49bfff07a2331abfd917ab7a3cdef4d59b90eb2351b524c44465e8c")]
    [InlineData(0, 5, 42869, "087df73741d8f6f884a17c86fa961f0af1406abb70f04022b9830a1ddc70e0e1")]
    [InlineData(1, 0, 11682, "8160d1a07d6f9455ea58c265843581fc31e9c2b2ce8dc9ce6cc7d93d2f743c9e")]
    [InlineData(1, 1, 23867, "c0e8bd25e164d8bae0293b512e8758b740296ae7d1543ca47e9d96e81d6cdb2f")]
    [InlineData(1, 2, 12009, "6df0b7bcadfe4cbfdf22d97cccbb4d85024c37d6e67aaf77802ec157c6a344f2")]
    [InlineData(1, 3, 32259, "d4aa6ce891a7730c12d7605f9f23eea3b09408dbec21239892582c0421a364a5")]
    [InlineData(1, 4, 11666, "19a44db0498319cc7d67ae5be3b4a9ff03244340d1e8559b3ee5657fc883cda7")]
    [InlineData(1, 5, 24341, "b10197ff0bd30a98a94a1d6ffca042e6a7ab355fddf0255e6faa8e1ca5deeacf")]
    public void ExplicitFamilyAndCacheComparisonMatchesEntireNativeBody(int filter, int kind, int bytes, string expected)
        => AssertCandidate(filter, kind, bytes, expected, Vp8lPaletteOrder.MinDelta);

    [Theory]
    [InlineData(0, 4, 73280, "604a84b2f8b2ded8a2cdb61eee87597215327582c9d4bbaf53b3b6e6ec84154b")]
    [InlineData(0, 5, 125429, "4cd5d4aa615989641516289521c8d6e26082bd8e1a3342759e5f05797c94e083")]
    [InlineData(1, 4, 11672, "d01d75960233e6f9b926435df5a3977b8d457eee534fdb3f31c1b7d505608c3b")]
    [InlineData(1, 5, 24107, "ba4513b22804da18d5257d8f7bde0efb27f61493d979ea1c5610e9eb356567ae")]
    public void ModifiedZengCandidatesMatchEntireNativeBody(int filter, int kind, int bytes, string expected)
        => AssertCandidate(filter, kind, bytes, expected, Vp8lPaletteOrder.ModifiedZeng);

    private static void AssertCandidate(int filter, int kind, int bytes, string expected, Vp8lPaletteOrder order)
    {
        using var root = new TestMemoryBudget(128_000_000);
        // The default2B refusal is a separate gate below. This one expensive
        // native candidate receives an explicit caller allowance for byte proof.
        using var context = new OperationContext(new() { MemoryBudget = root}, default);
        var transform = (Vp8lTransformKind)kind;
        var working = AlphaWorking(filter, context); var analysis = Vp8lEncodingAnalysis.Select(working, 513, 6, context);
        using var prepared = Vp8lPreparedEncoding.PrepareCandidate(working, 513, analysis, transform, context, order);
        Assert.Equal(bytes + 5, prepared.PayloadBytes); Array.Clear(working); context.Release(working.Length * 4L); var retained = root.Bytes;
        using var first = new MemoryStream(); prepared.WriteAlphaBody(first); var body = first.ToArray();
        if (Environment.GetEnvironmentVariable("LOKAD_VP8L_CANDIDATE_TRACE") is { } path) File.WriteAllBytes(path + "-f" + filter + "-" + transform + ".bin", body);
        Assert.Equal(bytes, body.Length); Assert.Equal(expected, Convert.ToHexStringLower(SHA256.HashData(body)));
        using var second = new MemoryStream(); prepared.WriteAlphaBody(second); Assert.Equal(body, second.ToArray());
        Assert.Equal(retained, root.Bytes); prepared.Dispose(); prepared.Dispose(); Assert.Equal(0, root.Bytes); Assert.False(root.IsDisposed);
        Assert.Equal(expected, Convert.ToHexStringLower(SHA256.HashData(body))); Assert.True(first.CanWrite); Assert.True(second.CanWrite);
    }

    [Fact]
    public void RefusalDuringAnotherCodingCandidateDisposesTheEarlierWinnerAndAllowsRecovery()
    {
        var source = OriginalVp8lControls.Input("two-color"); long candidateGrowths;
        using (var sizingRoot = new TestMemoryBudget(10_000_000))
        using (var sizing = new OperationContext(new() { MemoryBudget = sizingRoot }, default))
        {
            var pixels = Working(source.Pixels, false, sizing); var analysis = Vp8lEncodingAnalysis.Select(pixels, source.Width, 6, sizing);
            var start = sizingRoot.GrowthAttempts;
            using var prepared = Vp8lPreparedEncoding.PrepareCandidate(pixels, source.Width, analysis, Vp8lTransformKind.Palette, sizing);
            candidateGrowths = sizingRoot.GrowthAttempts - start; prepared.Dispose(); sizing.Release(pixels.Length * 4L); Assert.Equal(0, sizingRoot.Bytes);
        }
        using var root = new TestMemoryBudget(10_000_000); using var caller = root.CreateScope(); Assert.True(caller.TryReserve(1, out var held));
        using var context = new OperationContext(new() { MemoryBudget = root }, default); var working = Working(source.Pixels, false, context);
        var info = Vp8lEncodingAnalysis.Select(working, source.Width, 6, context);
        // The final allocation belongs to the last coding trial; an earlier
        // counted winner already exists and must be cleaned up, never returned.
        root.RefuseAtGrowth = root.GrowthAttempts + candidateGrowths;
        var error = Assert.Throws<ImageLimitException>(() => Vp8lPreparedEncoding.PrepareCandidate(working, source.Width, info, Vp8lTransformKind.Palette, context));
        Assert.Equal((ImageLimitKind.WorkingBytes, ImageLimitScope.SharedBudget), (error.Limit, error.Scope)); Assert.Equal(1, root.Refusals);
        Assert.Equal(working.Length * 4L + 1, root.Bytes); context.Release(working.Length * 4L); held.Dispose(); root.RefuseAtGrowth = 0; Assert.Equal(0, root.Bytes);
        var nextPixels = Working(source.Pixels, false, context); info = Vp8lEncodingAnalysis.Select(nextPixels, source.Width, 6, context);
        using var next = Vp8lPreparedEncoding.PrepareCandidate(nextPixels, source.Width, info, Vp8lTransformKind.Palette, context);
        context.Release(nextPixels.Length * 4L); next.Dispose(); Assert.Equal(0, root.Bytes); Assert.False(root.IsDisposed);
    }
}
