namespace Lokad.Image.Tooling;

// Original frequency recipes, not encoded inputs or upstream trees. They cover
// RFC1951's literal/distance/code-length alphabet sizes, ties and length repair.
internal static class OriginalDeflateTrees
{
    internal static (string Name, int MaximumBits, int[] Counts)[] Create()
    {
        var endOnly = new int[286]; endOnly[256] = 1;
        var sparse = new int[286];
        for (var i = 0; i < sparse.Length; i += 7) sparse[i] = i * 17 % 23 + 1;
        sparse[256] = 1;
        var overflow = new int[286]; var first = 1; var second = 1;
        for (var i = 269; i < 286; i++) { overflow[i] = first; (first, second) = (second, first + second); }
        overflow[256] = 1;
        var lengths = new int[19]; first = second = 1;
        for (var i = 0; i < lengths.Length; i++) { lengths[i] = first; (first, second) = (second, first + second); }
        var distances = new int[30]; distances[0] = 5; distances[5] = 12; distances[29] = 9;
        var lastDistance = new int[30]; lastDistance[29] = 1;
        return
        [
            ("EmptyLiterals", 15, new int[286]),
            ("EndOnly", 15, endOnly),
            ("TiedLiterals", 15, Enumerable.Repeat(1, 286).ToArray()),
            ("SparseLiterals", 15, sparse),
            ("OverflowLiterals", 15, overflow),
            ("OverflowCodeLengths", 7, lengths),
            ("SparseDistances", 15, distances),
            ("LastDistance", 15, lastDistance),
            ("EmptyCodeLengths", 7, new int[19]),
            ("TiedCodeLengths", 7, Enumerable.Repeat(1, 19).ToArray())
        ];
    }
}
