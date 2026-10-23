namespace Lokad.Image.Internal;

internal ref partial struct JpegDecoder
{
    // Owned CMYK and other four-component sample planes share this exact
    // block walk. Allocation, charges and pooled coefficient lifetimes
    // retain the original component order, including subsampled YCCK planes.
    private void MaterializeCmykPlanes()
    {
        Span<byte> block = stackalloc byte[64];
        foreach (var component in components)
        {
            var length = component.Coefficients.Length;
            context.Reserve(length); component.Plane = new byte[length];
            for (var y = 0; y < component.BlockRows; y++)
            for (var x = 0; x < component.BlockColumns; x++)
            {
                ReadOnlySpan<int> coefficients = component.Coefficients.AsSpan((y * component.BlockColumns + x) * 64, 64);
                if (progressive)
                {
                    var natural = component.Approximation.AsSpan();
                    for (var k = 0; k < 64; k++) natural[JpegTransform.Zigzag[k]] = coefficients[k];
                    coefficients = natural;
                }
                JpegInverseTransform.Inverse(coefficients, component.FrozenQuantization!, block, context);
                for (var by = 0; by < 8; by++)
                    block.Slice(by * 8, 8).CopyTo(component.Plane.AsSpan((y * 8 + by) * component.BlockColumns * 8 + x * 8, 8));
            }
            var storage = component.Coefficients; component.Coefficients = [];
            context.Release(length * 4L); context.ReturnCoefficients(storage);
        }
    }
}
