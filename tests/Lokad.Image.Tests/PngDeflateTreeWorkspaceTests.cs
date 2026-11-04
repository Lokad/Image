using System.Buffers.Binary;
using System.Security.Cryptography;
using Lokad.Image.Internal;
using Lokad.Image.Tooling;

namespace Lokad.Image.Tests;

public sealed class PngDeflateTreeWorkspaceTests
{
    // Frozen on both actual qualified producers before introducing reuse.
    private static readonly Dictionary<string, (string Lengths, string Codes)> Expected = new()
    {
            ["EmptyLiterals"] = ("9dcf97a184f32623d11a73124ceb99a5709b083721e878a16d78f596718ba7b2", "01acecb507abfe1a354aa8064f4af5d3f1acd019e37db3c11c97523b71c76e9d"),
            ["EndOnly"] = ("902ba5222794b725dfafa950fba3cca78a0525e53aefa4581e7d8a1798b5bd7a", "1c12923b667e62c6a01a7733812de7d7f8309a090ec47135f4ca78205a7c7cb1"),
            ["TiedLiterals"] = ("590ce352270475299ee6b8e9e48613af28d0e4a2ab862db5d472ad84eea6e11e", "bd9a6cf4dc3bc64846076bcd3a1da5f852231d7efd0750e027d8c28fc6a982df"),
            ["SparseLiterals"] = ("c4b03817ef48a3a46f4be39630ae36f1f361648dbabdfb991425d6d593d30597", "21fa084c74051f8927bdc1ed368c264fc9aae8861b52c42a7dafea65bd48ac47"),
            ["OverflowLiterals"] = ("3f3f9fb13ddebaf61409d9ade7dc7e087363e30417b8394b410742f7f5cc8891", "2b1032b0e1147aade7f8634ea73ca154c8c21d36c134fdca928187bd0ba89ef3"),
            ["OverflowCodeLengths"] = ("09c7945ac093f53f91bd8210b10e7fbc0b0ee85bfca26cb1d92fb147a2ca36f4", "cb3371ae09d79c53184c446903a1a3704621827f37122ecd2f9674ec8dcc31df"),
            ["SparseDistances"] = ("aa9e13fdeabf6e44e76c60dbfe33bc28f771e2e07681bb1b009bc12bb5863169", "687a14267bc1ba8ab4ba4d95d14164517be0cea1cf1fb2be2b3409db697c97a9"),
            ["LastDistance"] = ("a4f02e9b6e7db026656070d34559cca5b1daf15fc7366458b67d5e3df078d1bf", "6742748854b23b8e81e8fe1cf7b0b665efa433155f9c81a054b7dca03c91e51c"),
            ["EmptyCodeLengths"] = ("9dcf97a184f32623d11a73124ceb99a5709b083721e878a16d78f596718ba7b2", "01acecb507abfe1a354aa8064f4af5d3f1acd019e37db3c11c97523b71c76e9d"),
            ["TiedCodeLengths"] = ("b52e367c6b96168ca18d8732b10852c23121baa097e6285c0b982b6ceb2ec9c9", "f79733d77319d1499eb07982888226d2bdef9f49e707fd72520b505c2125b89a"),
    };

    [Fact]
    public void ReusedStoragePreservesFrozenTreesCountsAndIndependentResults()
    {
        var workspace = new DeflateHuffman.Workspace(); var recipes = OriginalDeflateTrees.Create();
        using var context = new OperationContext(ImageLimits.Default, default);
        List<(byte[] Result, byte[] Snapshot)> retained = [];
        foreach (var order in new[] { recipes, recipes.Reverse().ToArray(), recipes })
        foreach (var (name, maximumBits, counts) in order)
        {
            var original = counts.ToArray();
            var result = DeflateHuffman.Lengths(counts, maximumBits, context, workspace);
            Assert.Equal(Expected[name].Lengths, Hash(result));
            Assert.Equal(Expected[name].Codes, CodeHash(DeflateHuffman.Codes(result)));
            Assert.Equal(original, counts); Assert.All(result, width => Assert.InRange((int)width, 0, maximumBits));
            foreach (var previous in retained) Assert.Equal(previous.Snapshot, previous.Result);
            retained.Add((result, result.ToArray()));
        }
    }

    [Fact]
    public void PriorScratchContentsCannotAffectAnyAlphabet()
    {
        var workspace = new DeflateHuffman.Workspace();
        using var context = new OperationContext(ImageLimits.Default, default);
        foreach (var (name, maximumBits, counts) in OriginalDeflateTrees.Create())
        {
            // Poison all preparation slots, including the root, unused symbols
            // and the suffixes left behind when alphabet sizes shrink.
            foreach (var array in new[] { workspace.Weights, workspace.Depth, workspace.Parent,
                workspace.Heap, workspace.Removal, workspace.Bits, workspace.BitCounts }) Array.Fill(array, 12345);
            var original = counts.ToArray();
            var result = DeflateHuffman.Lengths(counts, maximumBits, context, workspace);
            Assert.Equal(Expected[name].Lengths, Hash(result));
            Assert.Equal(Expected[name].Codes, CodeHash(DeflateHuffman.Codes(result))); Assert.Equal(original, counts);
        }
    }

    [Fact]
    public void CancellationKeepsPriorTreesIndependentAndTheWorkspaceReusable()
    {
        var workspace = new DeflateHuffman.Workspace(); var recipes = OriginalDeflateTrees.Create();
        using var context = new OperationContext(ImageLimits.Default, default);
        var before = DeflateHuffman.Lengths(recipes[2].Counts, 15, context, workspace); var snapshot = before.ToArray();
        using var cancellation = new CancellationTokenSource();
        using var canceled = new OperationContext(ImageLimits.Default, cancellation.Token); cancellation.Cancel();
        var error = Assert.Throws<OperationCanceledException>(() => DeflateHuffman.Lengths(recipes[4].Counts, 15, canceled, workspace));
        Assert.Equal(cancellation.Token, error.CancellationToken); Assert.Equal(snapshot, before);
        var next = DeflateHuffman.Lengths(recipes[8].Counts, 7, context, workspace);
        Assert.Equal(Expected[recipes[8].Name].Lengths, Hash(next)); Assert.Equal(snapshot, before);
    }

    private static string Hash(ReadOnlySpan<byte> bytes) => Convert.ToHexStringLower(SHA256.HashData(bytes));
    private static string CodeHash(int[] codes)
    {
        var bytes = new byte[codes.Length * sizeof(int)];
        for (var i = 0; i < codes.Length; i++) BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(i * sizeof(int)), codes[i]);
        return Hash(bytes);
    }
}
