// Independent integer DCT for the pinned grayscale reference.
// Construct a Q13 kernel from cosine rotations, then evaluate rows/columns with
// their explicit rounding boundaries. No upstream source or numeric table is
// incorporated. The transform definition is ITU-T T.81 A.3.3.
namespace Lokad.Image.Internal;

internal static class JpegGrayReferenceTransform
{
    private static readonly int[] Kernel = BuildKernel();
    private static int Fixed(double value) => (int)Math.Round(value * 8192, MidpointRounding.AwayFromZero);

    private static int[] BuildKernel()
    {
        var kernel = new int[64];
        double root = Math.Sqrt(2), c1 = Math.Cos(Math.PI / 16), c3 = Math.Cos(3 * Math.PI / 16);
        double c5 = Math.Cos(5 * Math.PI / 16), c7 = Math.Cos(7 * Math.PI / 16);
        int rotation = Fixed(root * Math.Cos(3 * Math.PI / 8));
        int near = Fixed(2 * Math.Cos(3 * Math.PI / 8)), far = Fixed(2 * Math.Cos(Math.PI / 8));
        SetEven(kernel, 0, new[] { 8192, 8192, 8192, 8192 });
        SetEven(kernel, 4, new[] { 8192, -8192, -8192, 8192 });
        SetEven(kernel, 2, new[] { rotation + near, rotation, -rotation, -rotation - near });
        SetEven(kernel, 6, new[] { rotation, rotation - far, far - rotation, -rotation });

        // Four odd-frequency rotations expressed as small outer products. Each
        // independent rotation is rounded before constructing the full kernel.
        var odd = new int[16]; Array.Fill(odd, Fixed(root * c3));
        AddSquare(odd, 0, 3, 0, 3, Fixed(root * (c7 - c3)));
        AddSquare(odd, 1, 2, 1, 2, Fixed(root * (-c1 - c3)));
        AddSquare(odd, 1, 3, 0, 2, Fixed(root * (-c3 - c5)));
        AddSquare(odd, 0, 2, 1, 3, Fixed(root * (c5 - c3)));
        odd[3] += Fixed(root * (c1 + c3 - c5 - c7));
        odd[6] += Fixed(root * (c1 + c3 + c5 - c7));
        odd[9] += Fixed(root * (c1 + c3 - c5 + c7));
        odd[12] += Fixed(root * (-c1 + c3 + c5 - c7));
        for (int row = 0; row < 4; row++)
            for (int column = 0; column < 4; column++)
            {
                kernel[(row * 2 + 1) * 8 + 3 - column] = odd[row * 4 + column];
                kernel[(row * 2 + 1) * 8 + 4 + column] = -odd[row * 4 + column];
            }
        return kernel;
    }

    private static void SetEven(int[] kernel, int frequency, int[] weights)
    {
        for (int x = 0; x < 4; x++) kernel[frequency * 8 + x] = kernel[frequency * 8 + 7 - x] = weights[x];
    }

    private static void AddSquare(int[] matrix, int row0, int row1, int col0, int col1, int weight)
    {
        matrix[row0 * 4 + col0] += weight; matrix[row0 * 4 + col1] += weight;
        matrix[row1 * 4 + col0] += weight; matrix[row1 * 4 + col1] += weight;
    }

    public static void Forward(ReadOnlySpan<byte> samples, Span<short> transformed)
    {
        if (samples.Length != 64 || transformed.Length != 64) throw new ArgumentException("Complete8x8 block required.");
        Span<int> rows = stackalloc int[64];
        for (int y = 0; y < 8; y++)
            for (int u = 0; u < 8; u++)
            {
                int sum = 1024;
                for (int x = 0; x < 8; x++) sum += (samples[y * 8 + x] - 128) * Kernel[u * 8 + x];
                rows[y * 8 + u] = sum >> 11;
            }
        for (int v = 0; v < 8; v++)
            for (int u = 0; u < 8; u++)
            {
                int sum = 16384;
                for (int y = 0; y < 8; y++) sum += rows[y * 8 + u] * Kernel[v * 8 + y];
                transformed[v * 8 + u] = (short)(sum >> 15);
            }
    }

    public static ushort[] Quantization(int quality)
    {
        if (quality < 1 || quality > 100) throw new ArgumentOutOfRangeException(nameof(quality));
        int scale = quality < 50 ? 5000 / quality : 200 - 2 * quality;
        var result = new ushort[64];
        for (int v = 0; v < 8; v++)
            for (int u = 0; u < 8; u++)
                result[v * 8 + u] = (ushort)Math.Clamp(((1 + 2 * (u + v) + u * v) * scale + 50) / 100, 1, 255);
        return result;
    }

    public static void Quantize(ReadOnlySpan<short> transformed, ReadOnlySpan<ushort> quantization, Span<short> output)
    {
        for (int k = 0; k < 64; k++)
        {
            int value = transformed[k], divisor = quantization[k] * 8;
            int magnitude = (Math.Abs(value) + divisor / 2) / divisor;
            output[k] = (short)(value < 0 ? -magnitude : magnitude);
        }
    }

}
