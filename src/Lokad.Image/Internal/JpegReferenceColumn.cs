using System.Runtime.InteropServices;
using System.Runtime.Intrinsics.X86;

namespace Lokad.Image.Internal;

// Width and MCU column determine the horizontal area phase and tile bounds.
// Vertical coordinates and MCU/entropy traversal remain independent of this value.
internal readonly struct JpegReferenceColumn
{
    internal readonly int First, Last;
    internal readonly JpegReferenceHorizontalFilter Filter;

    internal static int ByteSize
    {
        get
        {
            Span<JpegReferenceColumn> value = stackalloc JpegReferenceColumn[1];
            return MemoryMarshal.AsBytes(value).Length;
        }
    }

    internal JpegReferenceColumn(int width, int left, Span<JpegReferenceArea> plans)
    {
        var pair = (width & 1) == 0; var target = (width + 1) / 2;
        First = left; Last = Math.Min(left + 15, width - 1);
        for (var x = 0; x < 8; x++)
        {
            var plan = plans[x] = JpegReferenceArea.Create(pair ? width / 2 : width, target,
                Math.Min(left / 2 + x, target - 1), 14);
            First = Math.Min(First, plan.Source(0) * (pair ? 2 : 1));
            Last = Math.Max(Last, plan.Source(plan.Count - 1) * (pair ? 2 : 1) + (pair ? 1 : 0));
        }
        Filter = Last - First + 1 >= 16 && Avx512BW.IsSupported && BitConverter.IsLittleEndian
            ? new JpegReferenceHorizontalFilter(plans, First, pair) : default;
    }
}
