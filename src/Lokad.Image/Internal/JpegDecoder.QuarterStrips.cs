using System.Runtime.InteropServices;

namespace Lokad.Image.Internal;

internal ref partial struct JpegDecoder
{
    private const int QuarterStripPixels = 64;
    private const int QuarterStripWorkspaceBytes = QuarterStripPixels * (5 * sizeof(int) + 4 * sizeof(ushort));

    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    private readonly struct QuarterStripWindow(int firstLeft, int firstRight, int secondLeft, int secondRight, int weight)
    {
        public readonly int FirstLeft = firstLeft, FirstRight = firstRight, SecondLeft = secondLeft, SecondRight = secondRight, Weight = weight;
    }

    private void RenderQuarterHalfStrips(ImageBuffer output)
    {
        // Fund the actual fixed stack capacity alongside live planes and output.
        // Refusal leaves the old reservation intact and propagates to the owner.
        context.Reserve(QuarterStripWorkspaceBytes);
        try
        {
            Span<QuarterStripWindow> windows = stackalloc QuarterStripWindow[QuarterStripPixels];
            Span<ushort> rows = stackalloc ushort[4 * QuarterStripPixels];
            var chroma = components[1]; var nativeWidth = (info.Width + 1) / 2;
            var horizontalStep = (int)(((long)nativeWidth * 65536 + info.Width / 2) / info.Width);
            var horizontalOffset = (horizontalStep >> 1) - 16384;
            var verticalStep = (int)(((long)chroma.SampleHeight * 65536 + info.Height / 2) / info.Height);
            var verticalOffset = (verticalStep >> 1) - 16384;
            for (var start = 0; start < info.Width; start += QuarterStripPixels)
            {
                context.Cancellation.ThrowIfCancellationRequested();
                var count = Math.Min(QuarterStripPixels, info.Width - start);
                for (var i = 0; i < count; i++)
                {
                    var (first, second, weight) = QuarterFilterPosition(start + i, horizontalStep, horizontalOffset, nativeWidth, 14);
                    windows[i] = new(first / 2, Math.Min((first + 1) / 2, chroma.SampleWidth - 1),
                        second / 2, Math.Min((second + 1) / 2, chroma.SampleWidth - 1), weight);
                }
                var evenRow = -1; var oddRow = -1;
                for (var y = 0; y < info.Height; y++)
                {
                    context.Cancellation.ThrowIfCancellationRequested();
                    var (top, bottom, weight) = QuarterFilterPosition(y, verticalStep, verticalOffset, chroma.SampleHeight, 12);
                    // The two clamped endpoints are equal or adjacent. Distinct
                    // active rows therefore occupy different modulo-two slots.
                    for (var endpoint = 0; endpoint < 2; endpoint++)
                    {
                        var sourceRow = endpoint == 0 ? top : bottom; var slot = sourceRow & 1;
                        if ((slot == 0 ? evenRow : oddRow) == sourceRow) continue;
                        FillQuarterStripRow(sourceRow, windows[..count], rows.Slice(slot * 128, count), rows.Slice(slot * 128 + 64, count));
                        if (slot == 0) evenRow = sourceRow; else oddRow = sourceRow;
                    }
                    JpegInverseColor.WriteFilteredQuarterRow(components[0].Plane.AsSpan(y * components[0].BlockColumns * 8 + start, count),
                        rows.Slice((top & 1) * 128, count), rows.Slice((bottom & 1) * 128, count),
                        rows.Slice((top & 1) * 128 + 64, count), rows.Slice((bottom & 1) * 128 + 64, count), weight,
                        output.GetRow8(y).Slice(start * 4, count * 4));
                }
            }
        }
        finally { context.Release(QuarterStripWorkspaceBytes); }
    }

    private void FillQuarterStripRow(int y, scoped ReadOnlySpan<QuarterStripWindow> windows, scoped Span<ushort> cb, scoped Span<ushort> cr)
    {
        var first = components[1]; var second = components[2];
        var sourceCb = first.Plane.AsSpan(y * first.BlockColumns * 8, first.SampleWidth);
        var sourceCr = second.Plane.AsSpan(y * second.BlockColumns * 8, second.SampleWidth);
        for (var i = 0; i < windows.Length; i++)
        {
            var window = windows[i];
            // Preserve the native 4:1:0 byte expansion BEFORE horizontal Q14.
            var cbFirst = (sourceCb[window.FirstLeft] + sourceCb[window.FirstRight]) >> 1;
            var cbSecond = (sourceCb[window.SecondLeft] + sourceCb[window.SecondRight]) >> 1;
            var crFirst = (sourceCr[window.FirstLeft] + sourceCr[window.FirstRight]) >> 1;
            var crSecond = (sourceCr[window.SecondLeft] + sourceCr[window.SecondRight]) >> 1;
            cb[i] = (ushort)((cbFirst * (16384 - window.Weight) + cbSecond * window.Weight) >> 7);
            cr[i] = (ushort)((crFirst * (16384 - window.Weight) + crSecond * window.Weight) >> 7);
        }
    }
}
