namespace Lokad.Image.Internal;

internal static class ResizeSampling
{
    // floor((index+1/2)*source/target), without a floating division that can
    // round a position immediately below a cell boundary into the next cell.
    // Valid positive int axis lengths and index < target keep this product
    // strictly below 2*int.MaxValue^2, within the signed 64-bit range.
    public static int Nearest(int index, int sourceLength, int targetLength)
        => (int)(((2L * index + 1) * sourceLength) / (2L * targetLength));
}
