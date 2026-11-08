using System.Numerics;
using System.Runtime.CompilerServices;

namespace Lokad.Image.Internal;

// Operation-private raster arrays share one sort. Static value orders avoid
// delegate creation on every scan band and copying edges through callbacks.
// Partition the smaller side recursively; cap depth and fall back to a heap.
internal static class TextRasterSort
{
    internal interface IOrder<T>
    {
        static abstract int Compare(in T a, in T b);
    }

    internal static void Sort<T, TOrder>(Span<T> values, CancellationToken cancellation)
        where TOrder : struct, IOrder<T>
    {
        cancellation.ThrowIfCancellationRequested();
        if (values.Length > 1)
            Partition<T, TOrder>(values, 2 * (BitOperations.Log2((uint)values.Length) + 1), cancellation);
    }

    private static void Partition<T, TOrder>(Span<T> values, int depth, CancellationToken cancellation)
        where TOrder : struct, IOrder<T>
    {
        while (values.Length > 16)
        {
            if (depth-- == 0) { Heap<T, TOrder>(values, cancellation); return; }
            var middle = values.Length / 2;
            if (Compare<T, TOrder>(in values[0], in values[middle], cancellation) > 0) Swap(values, 0, middle);
            if (Compare<T, TOrder>(in values[middle], in values[^1], cancellation) > 0) Swap(values, middle, values.Length - 1);
            if (Compare<T, TOrder>(in values[0], in values[middle], cancellation) > 0) Swap(values, 0, middle);
            var pivot = values[middle]; var left = 0; var right = values.Length - 1;
            while (left <= right)
            {
                while (left <= right && Compare<T, TOrder>(in values[left], in pivot, cancellation) < 0) left++;
                while (left <= right && Compare<T, TOrder>(in values[right], in pivot, cancellation) > 0) right--;
                if (left > right) break;
                Swap(values, left++, right--);
            }
            var low = values[..(right + 1)]; var high = values[left..];
            if (low.Length < high.Length)
            { if (low.Length > 1) Partition<T, TOrder>(low, depth, cancellation); values = high; }
            else
            { if (high.Length > 1) Partition<T, TOrder>(high, depth, cancellation); values = low; }
        }
        for (var i = 1; i < values.Length; i++)
        {
            var item = values[i]; var destination = i;
            while (destination != 0 && Compare<T, TOrder>(in item, in values[destination - 1], cancellation) < 0)
            { values[destination] = values[destination - 1]; destination--; }
            values[destination] = item;
        }
    }

    private static void Heap<T, TOrder>(Span<T> values, CancellationToken cancellation)
        where TOrder : struct, IOrder<T>
    {
        for (var node = values.Length / 2 - 1; node >= 0; node--) Sink<T, TOrder>(values, node, cancellation);
        for (var end = values.Length - 1; end > 0; end--)
        { Swap(values, 0, end); Sink<T, TOrder>(values[..end], 0, cancellation); }
    }

    private static void Sink<T, TOrder>(Span<T> values, int node, CancellationToken cancellation)
        where TOrder : struct, IOrder<T>
    {
        var item = values[node];
        while (node < values.Length / 2)
        {
            var child = node * 2 + 1;
            if (child + 1 < values.Length && Compare<T, TOrder>(in values[child], in values[child + 1], cancellation) < 0) child++;
            if (Compare<T, TOrder>(in item, in values[child], cancellation) >= 0) break;
            values[node] = values[child]; node = child;
        }
        values[node] = item;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int Compare<T, TOrder>(in T a, in T b, CancellationToken cancellation)
        where TOrder : struct, IOrder<T>
    { cancellation.ThrowIfCancellationRequested(); return TOrder.Compare(in a, in b); }

    private static void Swap<T>(Span<T> values, int a, int b)
    { var item = values[a]; values[a] = values[b]; values[b] = item; }
}
