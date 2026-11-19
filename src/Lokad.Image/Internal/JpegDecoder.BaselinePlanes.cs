using System.Runtime.CompilerServices;

namespace Lokad.Image.Internal;

internal ref partial struct JpegDecoder
{
    // Keep the bounded stack allocation after caller admission and outside the
    // shared/progressive scan frames. Every borrower completes before release.
    [MethodImpl(MethodImplOptions.NoInlining)]
    private void ReadComponentMcuBlocks(scoped ref JpegEntropyReader reader,
        scoped ReadOnlySpan<int> indexes, scoped ReadOnlySpan<int> dc,
        scoped ReadOnlySpan<int> ac, int columns, int rows, int stagingBytes, scoped Span<byte> target, int targetStride)
    {
        Span<byte> staged = stackalloc byte[stagingBytes];
        ReadBaselineBlocks(ref reader, indexes, dc, ac, columns, rows, true, true, staged, target, targetStride);
    }

    // Sequential scans have final coefficients after each block. Reconstruct
    // raw component samples now; color conversion and publication still wait
    // for complete container and component validation at EOI.
    private void ReadBaselineBlocks(scoped ref JpegEntropyReader reader,
        scoped ReadOnlySpan<int> indexes, scoped ReadOnlySpan<int> dc,
        scoped ReadOnlySpan<int> ac, int columns, int rows, bool interleaved, bool writePixels, scoped Span<byte> staged,
        scoped Span<byte> target, int targetStride)
    {
        var tables = 0;
        if (writePixels && (long)columns * rows >= 64)
        {
            var used = 0;
            for (var i = 0; i < indexes.Length; i++) used |= 1 << ac[i];
            for (var i = 0; i < acTables.Length; i++)
                if ((used & (1 << i)) != 0 && acTables[i]!.CanPrepareBaselinePairs) tables |= 1 << i;
        }
        var bytes = System.Numerics.BitOperations.PopCount((uint)tables) * JpegHuffman.BaselinePairBytes;
        // Choose from the local ceiling before asking the borrowed shared root.
        // A shared refusal terminates; no lower-memory retry follows it.
        var prepare = tables != 0 && context.RemainingWorkingBytes >= bytes;
        try
        {
            if (prepare)
                for (var i = 0; i < acTables.Length; i++)
                    if ((tables & (1 << i)) != 0) acTables[i]!.PrepareBaselinePairs(context);
            ReadBaselineBlockCore(ref reader, indexes, dc, ac, columns, rows, interleaved, writePixels, staged, target, targetStride);
        }
        finally
        {
            if (prepare)
                for (var i = 0; i < acTables.Length; i++)
                    if ((tables & (1 << i)) != 0) acTables[i]!.ReleaseBaselinePairs(context);
        }
    }

    private void ReadBaselineBlockCore(scoped ref JpegEntropyReader reader,
        scoped ReadOnlySpan<int> indexes, scoped ReadOnlySpan<int> dc,
        scoped ReadOnlySpan<int> ac, int columns, int rows, bool interleaved, bool writePixels, scoped Span<byte> staged,
        scoped Span<byte> target, int targetStride)
    {
        if (writePixels && !interleaved && target.IsEmpty) FillUncodedBaselinePadding(components[indexes[0]], columns, rows);
        Span<int> coefficients = stackalloc int[64];
        Span<byte> pixels = stackalloc byte[64];
        var blocksPerComponent = staged.Length / (components.Length * 64);
        var expectedRestart = 0;
        for (var mcu = 0; mcu < columns * rows; mcu++)
        {
            context.Cancellation.ThrowIfCancellationRequested();
            if (restartInterval != 0 && mcu != 0 && mcu % restartInterval == 0)
            {
                reader.Restart(expectedRestart); expectedRestart = (expectedRestart + 1) & 7;
                for (var i = 0; i < indexes.Length; i++) components[indexes[i]].Predictor = 0;
            }
            var mcuX = mcu % columns; var mcuY = mcu / columns;
            for (var i = 0; i < indexes.Length; i++)
            {
                var component = components[indexes[i]];
                var horizontal = interleaved ? component.H : 1;
                var vertical = interleaved ? component.V : 1;
                var stride = component.BlockColumns * 8;
                for (var by = 0; by < vertical; by++)
                for (var bx = 0; bx < horizontal; bx++)
                {
                    var x = mcuX * horizontal + bx;
                    var y = mcuY * vertical + by;
                    // Baseline syntax validation only writes coefficients;
                    // it never reads previous values or reconstructs pixels.
                    if (writePixels) coefficients.Clear();
                    var dcOnly = ReadBaselineBlock(ref reader, coefficients, ref component.Predictor, dcTables[dc[i]]!, acTables[ac[i]]!, context);
                    if (!writePixels) continue;
                    var block = staged.IsEmpty ? pixels
                        : staged.Slice((indexes[i] * blocksPerComponent + by * horizontal + bx) * 64, 64);
                    if (dcOnly)
                    {
                        context.Cancellation.ThrowIfCancellationRequested();
                        block.Fill(JpegInverseTransform.ConstantSample(coefficients[0], component.FrozenQuantization![0]));
                    }
                    else JpegInverseTransform.Inverse(coefficients, component.FrozenQuantization!, block, context);
                    if (!staged.IsEmpty) continue;
                    if (!target.IsEmpty)
                    {
                        var left = x * 8; var top = y * 8;
                        var width = Math.Min(8, info.Width - left); var height = Math.Min(8, info.Height - top);
                        if (width > 0 && height > 0)
                            StoreComponentBlock(pixels, target.Slice(top * targetStride + left * 4,
                                (height - 1) * targetStride + width * 4), targetStride, width, height, indexes[i], components.Length);
                    }
                    else
                        for (var line = 0; line < 8; line++)
                            pixels.Slice(line * 8, 8).CopyTo(component.Plane.AsSpan((y * 8 + line) * stride + x * 8, 8));
                }
            }
            if (!staged.IsEmpty)
            {
                var left = mcuX * maxH * 8; var top = mcuY * maxV * 8;
                var width = Math.Min(maxH * 8, info.Width - left); var height = Math.Min(maxV * 8, info.Height - top);
                if (width > 0 && height > 0)
                    StoreComponentMcu(staged, target.Slice(top * targetStride + left * 4,
                        (height - 1) * targetStride + width * 4), targetStride, width, height, maxH, maxV, components.Length);
            }
        }
    }

    private void FillUncodedBaselinePadding(Component component, int codedColumns, int codedRows)
    {
        // Noninterleaved scans omit MCU padding. The preceding full-coefficient
        // representation reconstructed those zero-coefficient blocks as128.
        var stride = component.BlockColumns * 8;
        var codedWidth = codedColumns * 8;
        var codedHeight = codedRows * 8;
        if (codedWidth < stride)
            for (var y = 0; y < codedHeight; y++)
            {
                context.Cancellation.ThrowIfCancellationRequested();
                component.Plane.AsSpan(y * stride + codedWidth, stride - codedWidth).Fill(128);
            }
        var start = codedHeight * stride;
        while (start < component.Plane.Length)
        {
            context.Cancellation.ThrowIfCancellationRequested();
            var count = Math.Min(65536, component.Plane.Length - start);
            component.Plane.AsSpan(start, count).Fill(128);
            start += count;
        }
    }
}
