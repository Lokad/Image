namespace Lokad.Image.Internal;

internal static partial class JpegTransform
{
    // T.81 A.3.3, restricted to a zero AC spectrum. All output positions
    // share the same frozen C(0) basis entry. Preserve the original binary64
    // multiplication order; replacing this with nominal DC/8 changes half ties.
    internal static bool TryInverseDc(ReadOnlySpan<int> coefficients, ReadOnlySpan<ushort> quantization,
        Span<byte> pixels, OperationContext context)
    {
        if (coefficients.Length != 64 || quantization.Length != 64 || pixels.Length != 64)
            throw new ArgumentException("JPEG transforms operate on complete 8x8 blocks.");
        if (coefficients[1..].ContainsAnyExcept(0)) return false;
        context.Work(4096);
        var sample = (double)coefficients[0] * quantization[0];
        var horizontal = sample * Cosines[0]; var sum = horizontal * Cosines[0];
        var value = (byte)Math.Clamp(Math.Round(sum * .25 + 128, MidpointRounding.AwayFromZero), 0, 255);
        pixels.Fill(value); return true;
    }
}
