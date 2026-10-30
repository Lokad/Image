namespace Lokad.Image.Internal;

internal ref partial struct JpegDecoder
{
    private const int QuarterSingleStripWorkspaceBytes = QuarterStripPixels * (5 * sizeof(int) + 2 * sizeof(ushort));

    private void RenderQuarterSingleStrips(ImageBuffer output)
    {
        context.Reserve(QuarterSingleStripWorkspaceBytes);
        try
        {
            Span<QuarterStripWindow> windows = stackalloc QuarterStripWindow[QuarterStripPixels];
            Span<ushort> rows = stackalloc ushort[2 * QuarterStripPixels];
            var chroma = components[1];
            var horizontalStep = (int)(((long)chroma.SampleWidth * 65536 + info.Width / 2) / info.Width);
            var horizontalOffset = (horizontalStep >> 1) - 8192;
            for (var start = 0; start < info.Width; start += QuarterStripPixels)
            {
                context.Cancellation.ThrowIfCancellationRequested();
                var count = Math.Min(QuarterStripPixels, info.Width - start);
                for (var i = 0; i < count; i++)
                {
                    var (first, second, weight) = QuarterFilterPosition(start + i, horizontalStep, horizontalOffset, chroma.SampleWidth, 14);
                    // Unlike 4:1:0, these are the original quarter-grid samples.
                    windows[i] = new(first, first, second, second, weight);
                }
                var cb = rows[..count]; var cr = rows.Slice(QuarterStripPixels, count);
                for (var y = 0; y < info.Height; y++)
                {
                    context.Cancellation.ThrowIfCancellationRequested();
                    FillQuarterStripRow(y, windows[..count], cb, cr);
                    // Zero vertical weight preserves (Q7 - 16384) << 2 exactly.
                    JpegInverseColor.WriteFilteredQuarterRow(components[0].Plane.AsSpan(y * components[0].BlockColumns * 8 + start, count),
                        cb, cb, cr, cr, 0, output.GetRow8(y).Slice(start * 4, count * 4));
                }
            }
        }
        finally { context.Release(QuarterSingleStripWorkspaceBytes); }
    }
}
