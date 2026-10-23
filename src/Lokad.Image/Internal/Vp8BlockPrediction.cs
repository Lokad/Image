namespace Lokad.Image.Internal;

internal enum Vp8IntraMode : byte { Dc, TrueMotion, Vertical, Horizontal }

// RFC6386 section12 large-block prediction. Empty spans mean absent borders;
// supplied borders already contain any source-edge replication. Encoder mode
// decisions and source/reconstructed neighbor selection belong to the caller.
internal static class Vp8BlockPrediction
{
    internal static void Fill(int size, Vp8IntraMode mode, ReadOnlySpan<byte> top,
        ReadOnlySpan<byte> left, byte corner, Span<byte> destination)
    {
        if (size is not (8 or 16)) throw new ArgumentOutOfRangeException(nameof(size));
        if ((uint)mode > 3) throw new ArgumentOutOfRangeException(nameof(mode));
        if ((!top.IsEmpty && top.Length < size) || (!left.IsEmpty && left.Length < size) || destination.Length < size * size)
            throw new ArgumentException("Prediction requires complete supplied borders and a full destination block.");
        var hasTop = !top.IsEmpty; var hasLeft = !left.IsEmpty;
        // Snapshot only the32 boundary bytes so borrowed output may overlap
        // caller borders. No frame copy, heap owner or per-block admission.
        Span<byte> borders = stackalloc byte[32]; var t = borders[..size]; var l = borders.Slice(16, size);
        if (hasTop) top[..size].CopyTo(t);
        if (hasLeft) left[..size].CopyTo(l);
        var sum = 0; var count = 0;
        if (mode == Vp8IntraMode.Dc)
        {
            for (var p = 0; p < size; p++)
            {
                if (hasTop) { sum += t[p]; count++; }
                if (hasLeft) { sum += l[p]; count++; }
            }
        }
        var dc = count == 0 ? 128 : (sum + count / 2) / count;
        for (var y = 0; y < size; y++)
        for (var x = 0; x < size; x++)
        {
            var value = mode switch
            {
                Vp8IntraMode.Dc => dc,
                Vp8IntraMode.Vertical => hasTop ? t[x] : 127,
                Vp8IntraMode.Horizontal => hasLeft ? l[y] : 129,
                _ => !hasLeft ? (hasTop ? t[x] : 129) : !hasTop ? l[y] : Math.Clamp(t[x] + l[y] - corner, 0, 255)
            };
            destination[y * size + x] = (byte)value;
        }
    }
}
