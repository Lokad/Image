using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
using Lokad.Image.Internal;

namespace Lokad.Image.Tests;

public sealed class JpegTwoRowColumnTests
{
    [Fact]
    public void BatchedColumnsPreserveScalarWordBiasReflectionClippingAndGuardsWithoutAllocation()
    {
        if (!Avx512BW.IsSupported) return;
        ReadOnlySpan<short> edges = [-32768, -32767, -32736, -1025, -1024, -33, -32, -1,
            0, 1, 31, 32, 1023, 32735, 32736, 32767];
        Span<short> input = stackalloc short[32]; input.Fill(7395);
        Span<byte> output = stackalloc byte[80];
        var random = new Random(768);
        Check(input, output); // Warm the actual publisher and format constants.
        var allocated = GC.GetAllocatedBytesForCurrentThread();
        var exact = true;
        for (var a = 0; a < edges.Length; a++) for (var b = 0; b < edges.Length; b++)
        for (var rotate = 0; rotate < 8; rotate++)
        {
            for (var x = 0; x < 8; x++)
            {
                input[8 + x] = edges[(a + x + rotate) % edges.Length];
                input[16 + x] = x % 2 == 0 ? edges[b] : (short)(-edges[b] - 1);
            }
            exact &= Check(input, output);
        }
        for (var sample = 0; sample < 4096; sample++)
        {
            for (var x = 8; x < 24; x++) input[x] = (short)random.Next(-32768, 32768);
            exact &= Check(input, output);
        }
        Assert.Equal(0, GC.GetAllocatedBytesForCurrentThread() - allocated);
        Assert.True(exact);
        Assert.Equal(-1, input[..8].IndexOfAnyExcept((short)7395));
        Assert.Equal(-1, input[24..].IndexOfAnyExcept((short)7395));
    }

    private static bool Check(ReadOnlySpan<short> input, Span<byte> output)
    {
        output.Fill(0xa5);
        JpegInverseTransform.StoreTwoRowColumnsAvx512(
            MemoryMarshal.Read<Vector128<short>>(MemoryMarshal.AsBytes(input.Slice(8, 8))),
            MemoryMarshal.Read<Vector128<short>>(MemoryMarshal.AsBytes(input.Slice(16, 8))), output.Slice(8, 64));
        // Independent scalar T.81 column evaluation, with the declared fixed-
        // point boundaries. Use the cosine definition rather than stored weights.
        for (var y = 0; y < 8; y++)
        {
            var reflected = y < 4 ? y : 7 - y;
            var weight = (int)Math.Round(Math.Sqrt(2) * Math.Cos((2 * reflected + 1) * Math.PI / 16) * 16384,
                MidpointRounding.AwayFromZero);
            if (y >= 4) weight = -weight;
            for (var x = 0; x < 8; x++)
            {
                var biased = unchecked((short)(input[8 + x] + 32));
                var sum = unchecked(biased * 16383 + input[16 + x] * weight);
                if (output[8 + y * 8 + x] != Math.Clamp(sum >> 20, 0, 255)) return false;
            }
        }
        return output[..8].IndexOfAnyExcept((byte)0xa5) == -1
            && output[72..].IndexOfAnyExcept((byte)0xa5) == -1;
    }
}
