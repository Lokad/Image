using System.Buffers.Binary;
using System.Runtime.Intrinsics;
using System.Security.Cryptography;

namespace Lokad.Image.Tooling;

// Authored arithmetic controls frozen through the qualified publication API.
// Every alpha exponent, mantissa endpoints, signs and half-threshold neighbours
// exercise guards as well as output. These are not image/performance fixtures.
internal static class OriginalBicubicPreparation
{
    internal delegate bool Publish(Vector512<double> sum, double n0, double n1, Span<byte> output);

    internal static (int Cases, int Accepted, string Digest) Verify(Publish publish)
    {
        ulong[] mantissas = [0, 1, 0x000f_ffff_ffff_ffff];
        using var hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
        Span<byte> output = stackalloc byte[32];
        Span<byte> decision = stackalloc byte[1];
        Span<double> sums = stackalloc double[8];
        var cases = 0; var accepted = 0;
        for (var exponent = 0; exponent < 2048; exponent++)
        for (var variant = 0; variant < mantissas.Length; variant++)
        for (var sign = 0; sign < 2; sign++)
        for (var phase = 0; phase < 2; phase++)
        {
            var a0 = BitConverter.UInt64BitsToDouble((ulong)sign << 63 | (ulong)exponent << 52 | mantissas[variant]);
            var a1 = BitConverter.UInt64BitsToDouble((ulong)((exponent + 37) % 2048) << 52 | mantissas[(variant + 1) % 3]);
            var n0 = Math.Max(.5, a0 / 64.25); var n1 = Math.Max(.5, a1 / 128.25);
            if (exponent % 31 == 0) n0 = .499;
            if (exponent % 37 == 0) n1 = double.PositiveInfinity;
            if (exponent % 47 == 0) n1 = double.NaN;
            // Approach the established rounding boundary from both sides.
            // This is mathematical input generation, not an expected decision.
            var radius = a0 > 0 && double.IsFinite(a0) ? Math.ScaleB(2.0001, -Math.ILogB(a0)) : 0;
            var tone = phase == 0 ? 32767.25 : 32767.5 + (variant - 1) * radius;
            sums[0] = tone * a0; sums[1] = 17.25 * a0;
            sums[2] = (variant == 0 ? -1.25 : 65535.75) * a0; sums[3] = a0;
            sums[4] = 45.25 * a1;
            sums[5] = (phase == 0 ? 255.25 : variant == 2 ? double.BitIncrement(255.5) : double.BitDecrement(255.5)) * a1;
            sums[6] = 60000.75 * a1; sums[7] = a1;
            var vector = Vector512.Create(sums[0], sums[1], sums[2], sums[3], sums[4], sums[5], sums[6], sums[7]);
            output.Fill(0xa5);
            var ok = publish(vector, n0, n1, output.Slice(8, 16));
            if (output[..8].ContainsAnyExcept((byte)0xa5) || output[24..].ContainsAnyExcept((byte)0xa5)
                || !ok && output.ContainsAnyExcept((byte)0xa5))
                throw new InvalidOperationException("Publication changed a guard or refused output.");
            if (ok)
            {
                accepted++;
                for (var lane = 0; lane < 8; lane++)
                {
                    var divisor = lane % 4 == 3 ? lane == 3 ? n0 : n1 : lane < 4 ? a0 : a1;
                    var value = sums[lane] / divisor;
                    if (double.IsNaN(value)) continue; // Nonfinite synthetic results remain in the frozen digest.
                    var expected = (ushort)Math.Clamp(Math.Round(value, MidpointRounding.AwayFromZero), 0, 65535);
                    if (BinaryPrimitives.ReadUInt16LittleEndian(output.Slice(8 + lane * 2, 2)) != expected)
                        throw new InvalidOperationException("Accepted word differs from independent scalar rounding.");
                }
            }
            decision[0] = ok ? (byte)1 : (byte)0;
            hash.AppendData(decision); hash.AppendData(output); cases++;
        }
        return (cases, accepted, Convert.ToHexStringLower(hash.GetHashAndReset()));
    }
}
