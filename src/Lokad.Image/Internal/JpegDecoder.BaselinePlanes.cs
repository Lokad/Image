namespace Lokad.Image.Internal;

internal ref partial struct JpegDecoder
{
    // Sequential scans have final coefficients after each block. Reconstruct
    // raw component samples now; color conversion and publication still wait
    // for complete container and component validation at EOI.
    private void ReadBaselineBlocks(scoped ref JpegEntropyReader reader,
        scoped ReadOnlySpan<int> indexes, scoped ReadOnlySpan<int> dc,
        scoped ReadOnlySpan<int> ac, int columns, int rows, bool interleaved, bool writePixels)
    {
        if (writePixels && !interleaved) FillUncodedBaselinePadding(components[indexes[0]], columns, rows);
        Span<int> coefficients = stackalloc int[64];
        Span<byte> pixels = stackalloc byte[64];
        var expectedRestart = 0;
        for (var mcu = 0; mcu < columns * rows; mcu++)
        {
            context.Cancellation.ThrowIfCancellationRequested();
            if (restartInterval != 0 && mcu != 0 && mcu % restartInterval == 0)
            {
                reader.Restart(expectedRestart); expectedRestart = (expectedRestart + 1) & 7;
                for (var i = 0; i < indexes.Length; i++) components[indexes[i]].Predictor = 0;
            }
            for (var i = 0; i < indexes.Length; i++)
            {
                var component = components[indexes[i]];
                var horizontal = interleaved ? component.H : 1;
                var vertical = interleaved ? component.V : 1;
                var stride = component.BlockColumns * 8;
                for (var by = 0; by < vertical; by++)
                for (var bx = 0; bx < horizontal; bx++)
                {
                    var x = (mcu % columns) * horizontal + bx;
                    var y = (mcu / columns) * vertical + by;
                    // Baseline syntax validation only writes coefficients;
                    // it never reads previous values or reconstructs pixels.
                    if (writePixels) coefficients.Clear();
                    ReadBaselineBlock(ref reader, coefficients, ref component.Predictor, dcTables[dc[i]]!, acTables[ac[i]]!, context);
                    if (!writePixels) continue;
                    JpegInverseTransform.Inverse(coefficients, component.FrozenQuantization!, pixels, context);
                    for (var line = 0; line < 8; line++)
                        pixels.Slice(line * 8, 8).CopyTo(component.Plane.AsSpan((y * 8 + line) * stride + x * 8, 8));
                }
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
