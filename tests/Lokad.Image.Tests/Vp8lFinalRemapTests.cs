using System.Buffers.Binary;
using System.Security.Cryptography;
using System.Text;
using Lokad.Image.Internal;
using Lokad.Image.Tooling;

namespace Lokad.Image.Tests;

public sealed class Vp8lFinalRemapTests
{
    // Finite primitive vectors: one survivor, insufficient reusable capacity,
    // separate constant groups, clipped tiles, cache symbols, low effort,
    // empty-tile inheritance and a varying full-color population.
    private static readonly (string Name, int W, int H, int Bits, int Cache, bool Low, int Pattern)[] Cases =
    [
        ("one", 64, 64, 2, 10, false, 0),
        ("limited", 129, 129, 2, 0, false, 1),
        ("four", 32, 32, 4, 0, false, 2),
        ("clipped", 67, 35, 3, 2, false, 3),
        ("cached", 65, 33, 3, 10, false, 4),
        ("low", 65, 33, 3, 5, true, 3),
        ("empty", 64, 64, 2, 0, false, 5),
        ("varying", 67, 35, 3, 0, false, 6)
    ];

    private static Vp8lToken[] Tokens(int width, int height, int bits, int pattern)
    {
        if (pattern == 5) return [Vp8lToken.Literal(0xff010203), Vp8lToken.Copy(width * height - 1, 1)];
        var result = new Vp8lToken[width * height]; uint state = 0x6d2b79f5;
        uint[] colors = [0xff000000, 0xaa555555, 0x55aaaaaa, 0x00ffffff];
        for (var y = 0; y < height; y++) for (var x = 0; x < width; x++)
        {
            state ^= state << 13; state ^= state >> 17; state ^= state << 5;
            var group = ((x >> bits) + 3 * (y >> bits)) & 3;
            var pixel = pattern switch
            {
                0 => 0xff010203u,
                1 => x < width / 2 ? colors[0] : colors[3],
                2 => colors[y / 16 * 2 + x / 16],
                6 => (state & 0x07070707u) + colors[group] / 2,
                _ => colors[group] ^ ((uint)(x + y) & 3) * 0x00010101u
            };
            var at = y * width + x;
            result[at] = pattern == 4 && at % 11 == 1 ? Vp8lToken.Cached(at & 1023) : Vp8lToken.Literal(pixel);
        }
        return result;
    }

    [Fact]
    public void FinalMapsAndEveryPopulationWordMatchFrozenTileMajorExecution()
    {
        using var digest = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
        foreach (var item in Cases)
        {
            var tokens = Tokens(item.W, item.H, item.Bits, item.Pattern); var original = tokens.ToArray();
            using var root = new TestMemoryBudget(32 * 1024 * 1024);
            using var context = new OperationContext(new() { MemoryBudget = root }, default);
            using var plan = Vp8lReferenceClustering.Build(tokens, item.W, item.H, item.Bits, item.Cache, item.Low, context, 100);
            digest.AppendData(Encoding.UTF8.GetBytes(item.Name)); Append(plan.GroupCount); Append(plan.Map.Length);
            foreach (var value in plan.Map) Append(value);
            foreach (var value in plan.Populations) Append(value);
            Assert.Equal(original, tokens);
            Assert.Equal((plan.Map.Length + plan.Populations.Length) * 4L, root.Bytes);
            plan.Dispose(); Assert.Equal(0, root.Bytes); Assert.False(root.IsDisposed);
        }
        var actual = Convert.ToHexStringLower(digest.GetHashAndReset());
        Assert.Equal("4eaa1c559c26f2253b31643687990512803aa814cff7f8d6871e7dc30310e99e", actual);
        void Append(int value)
        { Span<byte> word = stackalloc byte[4]; BinaryPrimitives.WriteInt32LittleEndian(word, value); digest.AppendData(word); }
    }

    [Fact]
    public void RetainedMapsAndPopulationsHaveIndependentStorageAndCharges()
    {
        var tokens = Tokens(32, 32, 4, 2);
        using var root = new TestMemoryBudget(1 << 24);
        using var context = new OperationContext(new() { MemoryBudget = root }, default);
        using var first = Vp8lReferenceClustering.Build(tokens, 32, 32, 4, 0, false, context, 100);
        var counts = first.Populations.ToArray(); var map = first.Map.ToArray(); var retained = root.Bytes;
        Assert.Equal(4, first.GroupCount);
        using var second = Vp8lReferenceClustering.Build(tokens, 32, 32, 4, 0, false, context, 100);
        Assert.Equal(retained * 2, root.Bytes);
        second.WritablePopulations.Fill(int.MinValue); second.WritableMap.Fill(int.MaxValue);
        second.Dispose(); Assert.Equal(retained, root.Bytes);
        Assert.Equal(counts, first.Populations.ToArray()); Assert.Equal(map, first.Map.ToArray());
        first.Dispose(); Assert.Equal(0, root.Bytes); Assert.False(root.IsDisposed);
    }

}
