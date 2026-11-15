using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

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

    internal static void SortGeometry<T, TOrder>(Span<T> values, CancellationToken cancellation)
        where T : unmanaged
        where TOrder : struct, IOrder<T>
    {
        cancellation.ThrowIfCancellationRequested();
        if (values.Length > 16 && TryNatural<T, TOrder>(values, cancellation)) return;
        Sort<T, TOrder>(values, cancellation);
    }

    // Contour/event arrays often contain long monotone runs. Scratch stays in
    // the existing bounded raster frame; return before the recursive fallback.
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static bool TryNatural<T, TOrder>(Span<T> values, CancellationToken cancellation)
        where T : unmanaged
        where TOrder : struct, IOrder<T>
    {
        Span<byte> storage = stackalloc byte[2048];
        var scratch = MemoryMarshal.Cast<byte, T>(storage);
        if (scratch.IsEmpty) return false;
        Span<int> starts = stackalloc int[16]; Span<int> ends = stackalloc int[16];
        var runs = 0; var at = 0;
        while (at < values.Length)
        {
            if (runs == starts.Length) return false;
            var start = at++;
            if (at < values.Length)
            {
                var descending = Compare<T, TOrder>(in values[at - 1], in values[at], cancellation) > 0;
                at++;
                while (at < values.Length)
                {
                    var order = Compare<T, TOrder>(in values[at - 1], in values[at], cancellation);
                    if (descending ? order <= 0 : order > 0) break;
                    at++;
                }
                if (descending)
                    for (int low = start, high = at - 1; low < high; low++, high--)
                    { cancellation.ThrowIfCancellationRequested(); Swap(values, low, high); }
            }
            starts[runs] = start; ends[runs++] = at;
        }
        for (var width = 1; width < runs; width *= 2)
            for (var i = 0; i + width < runs; i += width * 2)
            {
                var low = starts[i]; var middle = starts[i + width]; var high = ends[Math.Min(i + width * 2, runs) - 1];
                if (Compare<T, TOrder>(in values[middle - 1], in values[middle], cancellation) <= 0) continue;
                Merge<T, TOrder>(values, low, middle, high, scratch, cancellation);
            }
        cancellation.ThrowIfCancellationRequested();
        return true;
    }

    private static void Merge<T, TOrder>(Span<T> values, int low, int middle, int high,
        Span<T> scratch, CancellationToken cancellation)
        where T : unmanaged
        where TOrder : struct, IOrder<T>
    {
        // Process the smaller split first: at most log2(Int32.MaxValue)
        // larger splits can be pending. This frame never recurses.
        Span<(int Low, int Middle, int High)> pending = stackalloc (int, int, int)[32];
        var count = 0;
        while (true)
        {
            var left = middle - low; var right = high - middle;
            if (left != 0 && right != 0 && Compare<T, TOrder>(in values[middle - 1], in values[middle], cancellation) > 0)
            {
                if (Math.Min(left, right) <= scratch.Length)
                    MergeBuffered<T, TOrder>(values, low, middle, high, scratch, cancellation);
                else
                {
                    int first, second;
                    if (left >= right)
                    {
                        first = low + left / 2;
                        second = Bound<T, TOrder>(values, middle, high, in values[first], false, cancellation);
                    }
                    else
                    {
                        second = middle + right / 2;
                        first = Bound<T, TOrder>(values, low, middle, in values[second], true, cancellation);
                    }
                    if (first != middle && middle != second)
                    {
                        Reverse(values, first, middle, cancellation); Reverse(values, middle, second, cancellation);
                        Reverse(values, first, second, cancellation);
                    }
                    var center = first + (second - middle);
                    (int Low, int Middle, int High) a = (low, first, center), b = (center, second, high);
                    if (a.High - a.Low > b.High - b.Low) (a, b) = (b, a);
                    pending[count++] = b; (low, middle, high) = a;
                    continue;
                }
            }
            if (count == 0) return;
            (low, middle, high) = pending[--count];
        }
    }

    private static int Bound<T, TOrder>(Span<T> values, int low, int high, in T pivot,
        bool upper, CancellationToken cancellation)
        where TOrder : struct, IOrder<T>
    {
        while (low < high)
        {
            var middle = low + (high - low) / 2;
            var order = Compare<T, TOrder>(in values[middle], in pivot, cancellation);
            if (order < 0 || upper && order == 0) low = middle + 1; else high = middle;
        }
        return low;
    }

    private static void Reverse<T>(Span<T> values, int low, int high, CancellationToken cancellation)
    {
        for (high--; low < high; low++, high--)
        { cancellation.ThrowIfCancellationRequested(); Swap(values, low, high); }
    }

    private static void MergeBuffered<T, TOrder>(Span<T> values, int low, int middle, int high,
        Span<T> scratch, CancellationToken cancellation)
        where T : unmanaged
        where TOrder : struct, IOrder<T>
    {
        var left = middle - low; var right = high - middle;
        if (left <= right)
        {
            values.Slice(low, left).CopyTo(scratch);
            var a = 0; var b = middle; var destination = low;
            while (a < left && b < high)
            {
                if (Compare<T, TOrder>(in scratch[a], in values[b], cancellation) <= 0) values[destination++] = scratch[a++];
                else values[destination++] = values[b++];
            }
            while (a < left) { cancellation.ThrowIfCancellationRequested(); values[destination++] = scratch[a++]; }
        }
        else
        {
            values.Slice(middle, right).CopyTo(scratch);
            var a = middle - 1; var b = right - 1; var destination = high - 1;
            while (a >= low && b >= 0)
            {
                if (Compare<T, TOrder>(in values[a], in scratch[b], cancellation) > 0) values[destination--] = values[a--];
                else values[destination--] = scratch[b--];
            }
            while (b >= 0) { cancellation.ThrowIfCancellationRequested(); values[destination--] = scratch[b--]; }
        }
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
