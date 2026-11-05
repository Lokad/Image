namespace Lokad.Image.Internal;

internal ref partial struct JpegDecoder
{
    private void ReadPreparedRefinementScan(ref JpegEntropyReader reader, Component component,
        JpegHuffman ac, int columns, int rows, int start, int end, int high, int low)
    {
        const int bytes = 512 * sizeof(uint);
        context.Reserve(bytes);
        try
        {
            Span<uint> headers = stackalloc uint[512];
            ac.BuildRefinementHeaders(headers, context.Cancellation);
            var cancelable = context.Cancellation.CanBeCanceled;
            var eobRun = 0; var expectedRestart = 0; var mcu = 0;
            for (var row = 0; row < rows; row++)
            {
                var offset = row * component.BlockColumns * 64;
                for (var column = 0; column < columns; column++, mcu++, offset += 64)
                {
                    context.Cancellation.ThrowIfCancellationRequested();
                    if (restartInterval != 0 && mcu != 0 && mcu % restartInterval == 0)
                    {
                        if (eobRun != 0) throw new ImageDataException("JPEG EOB run crosses a restart interval.");
                        reader.Restart(expectedRestart); expectedRestart = (expectedRestart + 1) & 7;
                        component.Predictor = 0;
                    }
                    var block = component.Coefficients.AsSpan(offset, 64);
                    context.Cancellation.ThrowIfCancellationRequested();
                    var complete = cancelable
                        ? ac.TryReadCancelableRefinementBlock(ref reader, block, start, end, low, ref eobRun, headers)
                        : ac.TryReadRefinementBlock(ref reader, block, start, end, low, ref eobRun, headers);
                    if (!complete)
                        ReadBlock(ref reader, block, component, null, ac, start, end, high, low, ref eobRun);
                }
            }
            if (eobRun != 0) throw new ImageDataException("JPEG EOB run exceeds the scan.");
        }
        finally { context.Release(bytes); }
    }
}
