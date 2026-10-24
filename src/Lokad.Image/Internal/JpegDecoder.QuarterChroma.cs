namespace Lokad.Image.Internal;

internal ref partial struct JpegDecoder
{
    private bool HasQuarterWidthChroma()
        => components.Length == 3 && maxH == 4 && maxV is 1 or 2
            && components[0].H == 4 && components[0].V == maxV
            && components[1].H == 1 && components[1].V == 1
            && components[2].H == 1 && components[2].V == 1;

    private void WriteQuarterYcbcrPixels(int y, int start, int count, Span<byte> destination, int channels)
    {
        // T.81 sample grids plus the pinned full-range conversion precision.
        // Derive positions/weights on demand, without filter tables or a raster.
        var halfVertical = maxV == 2;
        var chroma = components[1];
        var sampleWidth = halfVertical ? (info.Width + 1) / 2 : chroma.SampleWidth;
        var horizontalStep = (int)(((long)sampleWidth * 65536 + info.Width / 2) / info.Width);
        var horizontalOffset = (horizontalStep >> 1) - (halfVertical ? 16384 : 8192);
        var top = y; var bottom = y; var verticalWeight = 0;
        if (halfVertical)
        {
            var verticalStep = (int)(((long)chroma.SampleHeight * 65536 + info.Height / 2) / info.Height);
            (top, bottom, verticalWeight) = QuarterFilterPosition(y, verticalStep, (verticalStep >> 1) - 16384,
                chroma.SampleHeight, 12);
        }
        var luma = components[0].Plane.AsSpan(y * components[0].BlockColumns * 8 + start, count);
        for (var i = 0; i < count; i++)
        {
            var horizontal = QuarterFilterPosition(start + i, horizontalStep, horizontalOffset, sampleWidth, 14);
            var cb = QuarterFilteredChroma(components[1], horizontal, top, bottom, verticalWeight, halfVertical);
            var cr = QuarterFilteredChroma(components[2], horizontal, top, bottom, verticalWeight, halfVertical);
            JpegInverseColor.WriteFilteredPixel(luma[i], cb, cr, destination.Slice(i * channels, channels), channels);
        }
    }

    private static (int First, int Second, int Weight) QuarterFilterPosition(int coordinate, int step,
        int offset, int sampleCount, int bits)
    {
        var position = (long)coordinate * step + offset;
        var first = (int)(position >> 16); var fraction = (int)(position & 65535);
        // The public reference's 1/500 reduction threshold discards a nearly
        // zero leading tap before normalization. Quarter/half-grid increments
        // advance beyond that tap on the next pixel, preserving monotonicity.
        if ((65536 - fraction) * 500 <= 65536)
        {
            var sample = Math.Clamp(first + 1, 0, sampleCount - 1);
            return (sample, sample, 0);
        }
        var precisionShift = 16 - bits; var one = 1 << bits;
        // Quantize the first weight, then preserve the normalized pair's sum.
        var secondWeight = one - ((65536 - fraction + (1 << (precisionShift - 1))) >> precisionShift);
        return (Math.Clamp(first, 0, sampleCount - 1), Math.Clamp(first + 1, 0, sampleCount - 1), secondWeight);
    }

    private static int QuarterFilteredChroma(Component component, (int First, int Second, int Weight) horizontal,
        int top, int bottom, int verticalWeight, bool halfVertical)
    {
        var upper = QuarterHorizontal(component, horizontal, top, halfVertical);
        if (!halfVertical) return (upper - (128 << 7)) << 2;
        var lower = QuarterHorizontal(component, horizontal, bottom, true);
        return (upper * (4096 - verticalWeight) + lower * verticalWeight - (128 << 19)) >> 10;
    }

    private static int QuarterHorizontal(Component component, (int First, int Second, int Weight) position,
        int y, bool expandToHalf)
    {
        var first = QuarterDecoderSample(component, position.First, y, expandToHalf);
        var second = QuarterDecoderSample(component, position.Second, y, expandToHalf);
        return (first * (16384 - position.Weight) + second * position.Weight) >> 7;
    }

    private static int QuarterDecoderSample(Component component, int x, int y, bool expandToHalf)
    {
        var row = y * component.BlockColumns * 8;
        if (!expandToHalf) return component.Plane[row + x];
        // 4:1:0 first expands horizontally to a half grid. Preserve its byte
        // average boundary while avoiding an intermediate image allocation.
        var left = x / 2; var right = Math.Min((x + 1) / 2, component.SampleWidth - 1);
        return (component.Plane[row + left] + component.Plane[row + right]) >> 1;
    }
}
