namespace Lokad.Image.Internal;

// Borrowed block reconstruction. Reuse the independently implemented RFC6386
// inverse transforms on minor stack snapshots, preserving separate input buffers.
// These kernels retain no storage; the outer codec admits block cancellation
// and coarse workspace.
internal static partial class Vp8BlockReconstruction
{
    // Pixels initially contain prediction. A pair adds two horizontally adjacent
    // 4x4 blocks from consecutive16-coefficient records. Snapshot BOTH blocks
    // before writes so a caller's coefficient/pixel views may alias safely.
    internal static void Add(ReadOnlySpan<short> coefficients, Span<byte> pixels, int stride, bool pair = false)
    {
        var blocks = pair ? 2 : 1; var width = blocks * 4;
        if (coefficients.Length < blocks * 16 || stride < width || (long)stride * 3 + width > pixels.Length)
            throw new ArgumentException("Reconstruction requires complete coefficient blocks and four pixel rows.");
        // AddVector loads the entire single block before publishing any pixel,
        // so even aliased views need no intervening copy. Pairs must snapshot
        // both blocks before the first block can overwrite the second input.
        if (!pair && System.Runtime.Intrinsics.X86.Sse41.IsSupported)
        {
            AddVector(coefficients, pixels, stride);
            return;
        }
        AddSnapshot(coefficients, pixels, stride, blocks);
    }

    private static void AddSnapshot(ReadOnlySpan<short> coefficients, Span<byte> pixels, int stride, int blocks)
    {
        Span<short> snapshot = stackalloc short[32]; coefficients[..(blocks * 16)].CopyTo(snapshot);
        if (System.Runtime.Intrinsics.X86.Sse41.IsSupported)
        {
            for (var block = 0; block < blocks; block++) AddVector(snapshot.Slice(block * 16, 16), pixels[(block * 4)..], stride);
            return;
        }
        for (var block = 0; block < blocks; block++)
        {
            var residual = snapshot.Slice(block * 16, 16); Vp8Transforms.InverseDct(residual);
            for (var y = 0; y < 4; y++)
            for (var x = 0; x < 4; x++)
            {
                var at = y * stride + block * 4 + x;
                pixels[at] = (byte)Math.Clamp(pixels[at] + residual[y * 4 + x], 0, 255);
            }
        }
    }

    // Restore the sixteen luma-block DCs at16-short intervals. All AC slots and
    // trailing caller capacity remain unchanged. Snapshot supports aliased input.
    internal static void WriteWalshDc(ReadOnlySpan<short> coefficients, Span<short> blocks)
    {
        if (coefficients.Length < 16 || blocks.Length < 16 * 15 + 1)
            throw new ArgumentException("Walsh reconstruction requires16coefficients and sixteen block DC slots.");
        Span<short> dc = stackalloc short[16]; coefficients[..16].CopyTo(dc); Vp8Transforms.InverseWalsh(dc);
        for (var p = 0; p < 16; p++) blocks[p * 16] = dc[p];
    }
}
