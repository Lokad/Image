using System.Buffers.Binary;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

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
        if (mode == Vp8IntraMode.Dc)
        {
            destination[..(size * size)].Fill((byte)(count == 0 ? 128 : (sum + count / 2) / count));
            return;
        }
        if (mode == Vp8IntraMode.Vertical || mode == Vp8IntraMode.TrueMotion && !hasLeft)
        {
            if (!hasTop) destination[..(size * size)].Fill(mode == Vp8IntraMode.Vertical ? (byte)127 : (byte)129);
            else for (var y = 0; y < size; y++) t.CopyTo(destination.Slice(y * size, size));
            return;
        }
        if (mode == Vp8IntraMode.Horizontal || !hasTop)
        {
            for (var y = 0; y < size; y++) destination.Slice(y * size, size).Fill(hasLeft ? l[y] : (byte)129);
            return;
        }
        // Both borders exist. Signed words hold top + left - corner in
        // [-255,510]; unsigned saturation exactly matches the scalar clamp.
        if (Sse2.IsSupported)
        {
            var values = size == 16
                ? Vector128.LoadUnsafe(ref MemoryMarshal.GetReference(t))
                : Vector128.CreateScalar(BinaryPrimitives.ReadUInt64LittleEndian(t)).AsByte();
            var low = Sse2.UnpackLow(values, Vector128<byte>.Zero).AsInt16();
            var high = Sse2.UnpackHigh(values, Vector128<byte>.Zero).AsInt16();
            for (var y = 0; y < size; y++)
            {
                var delta = Vector128.Create((short)(l[y] - corner));
                var row = Sse2.PackUnsignedSaturate(Sse2.Add(low, delta), Sse2.Add(high, delta));
                var target = destination.Slice(y * size, size);
                if (size == 16) row.StoreUnsafe(ref MemoryMarshal.GetReference(target));
                else row.GetLower().StoreUnsafe(ref MemoryMarshal.GetReference(target));
            }
            return;
        }
        for (var y = 0; y < size; y++)
        for (var x = 0; x < size; x++)
            destination[y * size + x] = (byte)Math.Clamp(t[x] + l[y] - corner, 0, 255);
    }
}
