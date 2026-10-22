// Independent scalar factorization of the eight-point DCT. Tangent rotations
// and cosine row weights are generated mathematically; no implementation tables
// are imported. Integer scale, high-product truncation, odd corrections and final
// rounding intentionally match the pinned forward stage. Development tooling.
using System;

public static class JpegForwardParity
{
    private static readonly int Tangent1 = Fixed(Math.Tan(Math.PI / 16), 16);
    private static readonly int Tangent2 = Fixed(Math.Tan(Math.PI / 8), 16);
    private static readonly int Tangent3 = Fixed(Math.Tan(3 * Math.PI / 16), 16) - (1 << 16);
    private static readonly int HalfCosine = Fixed(Math.Cos(Math.PI / 4), 15);
    private static readonly int[] RowWeights = GenerateWeights();

    private static int Fixed(double value, int bits) => (int)Math.Round(value * (1 << bits), MidpointRounding.AwayFromZero);
    private static int HighProduct(int value, int multiplier) => value * multiplier >> 16;

    private static int[] GenerateWeights()
    {
        var result = new int[512];
        for (int v = 0; v < 8; v++)
        {
            int angle = v == 0 || v == 4 ? 4 : Math.Min(v, 8 - v);
            double scale = Math.Cos(angle * Math.PI / 16);
            for (int u = 0; u < 8; u++)
                for (int x = 0; x < 8; x++)
                {
                    double cosine = u == 0 ? Math.Cos(Math.PI / 4) : Math.Cos((2 * x + 1) * u * Math.PI / 16);
                    result[v * 64 + u * 8 + x] = Fixed(cosine * scale, 15);
                }
        }
        return result;
    }

    public static void Forward(ReadOnlySpan<byte> samples, Span<short> output)
    {
        if (samples.Length != 64 || output.Length != 64) throw new ArgumentException("Complete 8x8 block required.");
        Span<int> columns = stackalloc int[64];
        for (int x = 0; x < 8; x++)
        {
            int a = (samples[x] + samples[56 + x]) << 3;
            int b = (samples[8 + x] + samples[48 + x]) << 3;
            int c = (samples[16 + x] + samples[40 + x]) << 3;
            int d = (samples[24 + x] + samples[32 + x]) << 3;
            int outside = a + d, inside = b + c;
            int outsideDifference = a - d, insideDifference = b - c;
            columns[x] = outside + inside;
            columns[32 + x] = outside - inside;
            columns[16 + x] = (outsideDifference + HighProduct(insideDifference, Tangent2)) | 1;
            columns[48 + x] = (HighProduct(outsideDifference, Tangent2) - insideDifference) | 1;

            int first = (samples[x] - samples[56 + x]) << 3;
            int middle1 = (samples[8 + x] - samples[48 + x]) << 4;
            int middle2 = (samples[16 + x] - samples[40 + x]) << 4;
            int last = (samples[24 + x] - samples[32 + x]) << 3;
            int sumRotation = HighProduct(middle1 + middle2, HalfCosine) | 1;
            int differenceRotation = HighProduct(middle1 - middle2, HalfCosine);
            int low = first + sumRotation, high = first - sumRotation;
            int lowRotation = last + differenceRotation, highRotation = last - differenceRotation;
            columns[8 + x] = (low + HighProduct(lowRotation, Tangent1)) | 1;
            columns[56 + x] = HighProduct(low, Tangent1) - lowRotation;
            columns[24 + x] = high - highRotation - HighProduct(highRotation, Tangent3);
            columns[40 + x] = highRotation + high + HighProduct(high, Tangent3);
        }
        for (int v = 0; v < 8; v++)
            for (int u = 0; u < 8; u++)
            {
                long sum = 1 << 16;
                for (int x = 0; x < 8; x++) sum += (long)columns[v * 8 + x] * RowWeights[v * 64 + u * 8 + x];
                output[v * 8 + u] = (short)Math.Clamp(sum >> 17, short.MinValue, short.MaxValue);
            }
    }

    public static int FirstDifference(byte[] planes, byte[] expected)
    {
        const int width = 513, height = 259, plane = width * height;
        if (planes.Length != plane * 3 || expected.Length != 65 * 33 * 3 * 128)
            throw new ArgumentException("Frozen 513x259 planar input and complete little-endian FDCT output required.");
        Span<byte> samples = stackalloc byte[64];
        Span<short> output = stackalloc short[64];
        int index = 0;
        for (int component = 0; component < 3; component++)
            for (int top = 0; top < height; top += 8)
                for (int left = 0; left < width; left += 8)
                {
                    for (int y = 0; y < 8; y++)
                        for (int x = 0; x < 8; x++)
                            samples[y * 8 + x] = planes[component * plane + Math.Min(top + y, height - 1) * width + Math.Min(left + x, width - 1)];
                    Forward(samples, output);
                    for (int k = 0; k < 64; k++, index++)
                        if (output[k] != (short)(expected[index * 2] | expected[index * 2 + 1] << 8)) return index;
                }
        return -1;
    }

    public static void Quantize(ReadOnlySpan<short> transformed, ReadOnlySpan<ushort> quantization, Span<short> output)
    {
        if (transformed.Length != 64 || quantization.Length != 64 || output.Length != 64 || quantization[0] != 8)
            throw new ArgumentException("Complete blocks with the pinned DC precision required.");
        output[0] = (short)(((transformed[0] + 32) >> 6) - 128);
        for (int k = 1; k < 64; k++)
        {
            if (quantization[k] is 0 or > 255) throw new ArgumentException("Pinned 8-bit quantization required.");
            int reciprocal = 8192 / quantization[k];
            int rounding = (24576 + reciprocal / 2) / reciprocal;
            int value = transformed[k];
            int magnitude = ((Math.Abs(value) + rounding) * reciprocal) >> 16;
            output[k] = (short)(value < 0 ? -magnitude : magnitude);
        }
    }

    public static int FirstQuantizationDifference(byte[] planes, byte[] coefficientDump)
    {
        const int width = 513, height = 259, plane = width * height;
        if (planes.Length != plane * 3 || coefficientDump.Length != 824144
            || !coefficientDump.AsSpan(0, 8).SequenceEqual("LJCF0001"u8)
            || Word(coefficientDump, 8) != width || Word(coefficientDump, 12) != height || Word(coefficientDump, 16) != 3)
            throw new ArgumentException("Frozen planar source and public coefficient dump required.");
        Span<byte> samples = stackalloc byte[64];
        Span<short> transformed = stackalloc short[64];
        Span<short> output = stackalloc short[64];
        Span<ushort> quantization = stackalloc ushort[64];
        int cursor = 20, index = 0;
        for (int component = 0; component < 3; component++)
        {
            if (Word(coefficientDump, cursor) != component + 1 || Word(coefficientDump, cursor + 4) != 1
                || Word(coefficientDump, cursor + 8) != 2 || Word(coefficientDump, cursor + 12) != 65
                || Word(coefficientDump, cursor + 16) != 33)
                throw new ArgumentException("Pinned native component geometry differs.");
            cursor += 20;
            for (int k = 0; k < 64; k++, cursor += 2) quantization[k] = (ushort)HalfWord(coefficientDump, cursor);
            for (int top = 0; top < height; top += 8)
                for (int left = 0; left < width; left += 8)
                {
                    for (int y = 0; y < 8; y++)
                        for (int x = 0; x < 8; x++)
                            samples[y * 8 + x] = planes[component * plane + Math.Min(top + y, height - 1) * width + Math.Min(left + x, width - 1)];
                    Forward(samples, transformed);
                    Quantize(transformed, quantization, output);
                    for (int k = 0; k < 64; k++, index++, cursor += 2)
                        if (output[k] != (short)HalfWord(coefficientDump, cursor)) return index;
                }
        }
        return -1;
    }

    private static int HalfWord(byte[] data, int position) => data[position] | data[position + 1] << 8;
    private static int Word(byte[] data, int position) => HalfWord(data, position) | HalfWord(data, position + 2) << 16;
}
