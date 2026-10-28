using System.Buffers.Binary;
using System.Runtime.Intrinsics;
using System.Security.Cryptography;

namespace Lokad.Image.Tooling;

// Authored arithmetic/guard controls, not image or performance inputs.
internal static class OriginalBicubicPairs
{
    internal delegate bool Publish(Vector512<double> sum, double n0, double n1, Span<byte> output);

    internal static (int Cases, int Accepted, string Digest) Verify(Publish publish)
    {
        const double error = 2.3283064365386963e-10 * 65535;
        (double, double)[] alphas = [(0, 16), (16, 0), (-1, 32), (error, 32),
            (double.BitIncrement(error), 32), (1, 16), (16, 1), (8, 16),
            (16, 16), (32, 256), (32768, 65535), (65535, 32768),
            (double.PositiveInfinity, 65535), (16, double.NaN)];
        (double, double)[] normalizers = [(.499, 1), (.5, .5), (1, 2), (2, 1),
            (double.NaN, 1), (double.PositiveInfinity, 2)];
        double[] tones = [-1, 0, double.BitDecrement(.5), .5, double.BitIncrement(.5),
            1.4999, 1.5, 255.5, 32767.5, 65534.5, 65535, 65536];
        using var hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
        var cases = 0; var accepted = 0;
        foreach (var (a0, a1) in alphas)
        foreach (var (n0, n1) in normalizers)
        for (var tone = 0; tone < tones.Length; tone++)
        {
            var sums = new[] { tones[tone] * a0, 17.25 * a0, 1024.125 * a0, a0,
                45.25 * a1, (tones[tone] + .125) * a1, 60000.75 * a1, a1 };
            var vector = Vector512.Create(sums[0], sums[1], sums[2], sums[3], sums[4], sums[5], sums[6], sums[7]);
            var output = Enumerable.Repeat((byte)0xa5, 32).ToArray();
            var ok = publish(vector, n0, n1, output.AsSpan(8, 16));
            if (output.AsSpan(0, 8).ContainsAnyExcept((byte)0xa5)
                || output.AsSpan(24).ContainsAnyExcept((byte)0xa5)
                || !ok && output.AsSpan().ContainsAnyExcept((byte)0xa5))
                throw new InvalidOperationException("Guarded publication changed unrelated or refused output.");
            if (ok)
            {
                accepted++;
                for (var lane = 0; lane < 8; lane++)
                {
                    var divisor = lane % 4 == 3 ? lane == 3 ? n0 : n1 : lane < 4 ? a0 : a1;
                    var value = sums[lane] / divisor;
                    // Nonfinite synthetic colors are retained by the frozen digest.
                    if (double.IsNaN(value)) continue;
                    var expected = (ushort)Math.Clamp(Math.Round(value, MidpointRounding.AwayFromZero), 0, 65535);
                    if (BinaryPrimitives.ReadUInt16LittleEndian(output.AsSpan(8 + lane * 2)) != expected)
                        throw new InvalidOperationException("Published word differs from independent scalar rounding.");
                }
            }
            hash.AppendData(new byte[] { ok ? (byte)1 : (byte)0 }); hash.AppendData(output); cases++;
        }
        return (cases, accepted, Convert.ToHexStringLower(hash.GetHashAndReset()));
    }
}
