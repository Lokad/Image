using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace Lokad.Image.Internal;

// Separable, alpha-aware fixed-point resampling. No intermediate rounding:
// horizontal Int64 rows feed Int128 vertical sums, followed by one exact divide.
internal static class LanczosResize
{
    internal static ImageBuffer Apply(ImageBuffer source, int width, int height, OperationContext context)
    {
        using var horizontal = LanczosPlan.Create(source.Width, width, context);
        using var vertical = LanczosPlan.Create(source.Height, height, context);
        var fast = LanczosFmaResize.CanUse(source.BitDepth, width, height, horizontal, vertical, context);
        // Keep room for the independently owned result when choosing optional
        // row capacity. This selection does not reserve or charge that output.
        var outputBytes = context.PixelBytes(width, height, source.BitDepth);
        using var workspace = new LanczosWorkspace(horizontal, vertical, source.BitDepth, fast, context, outputBytes);
        return Execute(source, width, height, horizontal, vertical, workspace, context);
    }

    private readonly struct SingleRows { }
    private readonly struct PairedByteRows { }
    private readonly struct PairedWordRows { }

    internal static ImageBuffer Execute(ImageBuffer source, int width, int height, LanczosPlan horizontal,
        LanczosPlan vertical, LanczosWorkspace workspace, OperationContext context)
        => workspace.Fast && source.BitDepth == 8 && height < vertical.SourceLength
            ? Execute<PairedByteRows>(source, width, height, horizontal, vertical, workspace, context)
            : workspace.Fast && source.BitDepth == 16 && width < source.Width && height < source.Height
                && workspace.Products.Length >= source.Width * 8L
                ? Execute<PairedWordRows>(source, width, height, horizontal, vertical, workspace, context)
            : Execute<SingleRows>(source, width, height, horizontal, vertical, workspace, context);

    private static ImageBuffer Execute<TRows>(ImageBuffer source, int width, int height, LanczosPlan horizontal,
        LanczosPlan vertical, LanczosWorkspace workspace, OperationContext context) where TRows : struct
    {
        var slots = workspace.RowIds.Length; var channels = width * 4L;
        var rowIds = workspace.RowIds; Array.Fill(rowIds, -1);
        var result = ImageBuffer.AllocateForOverwrite(width, height, source.BitDepth, context);
        try
        {
            var maximum = source.BitDepth == 8 ? 255 : 65535;
            var errors = workspace.Fast ? LanczosFmaResize.Errors(vertical.MaximumCount,
                source.BitDepth == 16 ? horizontal.MaximumCount : 0) : default;
            var numeratorError = errors.Rgb + 65535d * errors.Alpha;
            for (var y = 0; y < height; y++)
            {
                context.Cancellation.ThrowIfCancellationRequested();
                if ((typeof(TRows) == typeof(PairedByteRows) || typeof(TRows) == typeof(PairedWordRows))
                    && vertical.MaximumCount <= slots
                    && y + 2 <= height - (typeof(TRows) == typeof(PairedWordRows) ? 4 : 8))
                {
                    // Four unpublished word rows or eight byte rows hold one double sum.
                    // Publish only preceding rows while that suffix is borrowed;
                    // the ordinary tail overwrites it after the last pair retires.
                    var second = MemoryMarshal.Cast<byte, double>(result.Pixels.Slice(
                        (height - (typeof(TRows) == typeof(PairedWordRows) ? 4 : 8)) * result.Stride, (int)channels * sizeof(double)));
                    ExecuteOutputPair<TRows>(source, result, y, horizontal, vertical, workspace,
                        second, numeratorError, errors.Alpha, context);
                    y++;
                    continue;
                }
                if (workspace.Fast) workspace.FastSums.AsSpan().Clear();
                else workspace.ExactSums.AsSpan().Clear();
                var window = vertical.Windows[y];
                for (var tap = 0; tap < window.Count; tap++)
                {
                    context.Cancellation.ThrowIfCancellationRequested(); var weight = vertical.Weights[window.Offset + tap]; if (weight == 0) continue;
                    var sy = window.First + tap; var slot = sy % slots;
                    // Geometry, cache publication and result ownership are shared.
                    // The typed kernels retain their arithmetic and private views.
                    if (workspace.Fast)
                    {
                        var row = workspace.FastRows.AsSpan(slot * (int)channels, (int)channels);
                        if (rowIds[slot] != sy)
                        {
                            // Share coefficient expansion only across two rows
                            // that this window will actually consume. Distinct
                            // cache slots stay private until both are complete.
                            var prepared = false;
                            if (typeof(TRows) == typeof(PairedByteRows) || typeof(TRows) == typeof(PairedWordRows))
                            {
                                var next = sy + 1; var nextSlot = next % slots;
                                if (slots > 1 && tap + 1 < window.Count
                                    && vertical.Weights[window.Offset + tap + 1] != 0 && rowIds[nextSlot] != next)
                                {
                                    var nextRow = workspace.FastRows.AsSpan(nextSlot * (int)channels, (int)channels);
                                    if (typeof(TRows) == typeof(PairedByteRows))
                                        LanczosFmaResize.HorizontalBytePair(source, sy, next, horizontal, row, nextRow, context);
                                    else LanczosFmaResize.HorizontalWordPair(source, sy, next, horizontal,
                                        row, nextRow, workspace.Products, context);
                                    rowIds[nextSlot] = next; prepared = true;
                                }
                            }
                            if (!prepared) LanczosFmaResize.Horizontal(source, sy, horizontal, row,
                                typeof(TRows) == typeof(PairedWordRows)
                                    ? workspace.Products.AsSpan(0, source.Width * 4) : workspace.Products, context);
                            rowIds[slot] = sy;
                        }
                        context.Cancellation.ThrowIfCancellationRequested();
                        LanczosFmaResize.Accumulate(row, workspace.FastSums, weight, context);
                    }
                    else
                    {
                        var row = workspace.ExactRows.AsSpan(slot * (int)channels, (int)channels);
                        if (rowIds[slot] != sy) { Horizontal(source, sy, horizontal, row, workspace.Products, context); rowIds[slot] = sy; }
                        context.Cancellation.ThrowIfCancellationRequested();
                        AccumulateVertical(row, workspace.ExactSums, weight, context);
                    }
                }
                context.Cancellation.ThrowIfCancellationRequested();
                if (workspace.Fast) LanczosFmaResize.WriteRow(source, result, y, horizontal, vertical,
                    workspace.FastSums, numeratorError, errors.Alpha, maximum, context);
                else WriteExactRow(result, y, workspace.ExactSums, maximum, context);
            }
            result.Orientation = source.Orientation; return result;
        }
        catch { result.Dispose(); throw; }
    }

    private static void ExecuteOutputPair<TRows>(ImageBuffer source, ImageBuffer result, int y,
        LanczosPlan horizontal, LanczosPlan vertical, LanczosWorkspace workspace,
        Span<double> second, double numeratorError, double alphaError, OperationContext context) where TRows : struct
    {
        var maximum = typeof(TRows) == typeof(PairedWordRows) ? 65535 : 255;
        var first = workspace.FastSums.AsSpan(); first.Clear(); second.Clear();
        var w0 = vertical.Windows[y]; var w1 = vertical.Windows[y + 1];
        var begin = Math.Min(w0.First, w1.First);
        var end = Math.Max(w0.First + w0.Count, w1.First + w1.Count);
        var channels = first.Length; var ids = workspace.RowIds; var slots = ids.Length;
        for (var sy = begin; sy < end; sy++)
        {
            context.Cancellation.ThrowIfCancellationRequested();
            var t0 = sy - w0.First; var t1 = sy - w1.First;
            var weight0 = (uint)t0 < (uint)w0.Count ? vertical.Weights[w0.Offset + t0] : 0;
            var weight1 = (uint)t1 < (uint)w1.Count ? vertical.Weights[w1.Offset + t1] : 0;
            if (weight0 == 0 && weight1 == 0) continue;
            var slot = sy % slots;
            var row = workspace.FastRows.AsSpan(slot * channels, channels);
            if (ids[slot] != sy)
            {
                var next = sy + 1; var nextSlot = next % slots;
                var next0 = next - w0.First; var next1 = next - w1.First;
                var neededNext = (uint)next0 < (uint)w0.Count && vertical.Weights[w0.Offset + next0] != 0
                    || (uint)next1 < (uint)w1.Count && vertical.Weights[w1.Offset + next1] != 0;
                if (slots > 1 && neededNext && ids[nextSlot] != next)
                {
                    var nextRow = workspace.FastRows.AsSpan(nextSlot * channels, channels);
                    if (typeof(TRows) == typeof(PairedWordRows))
                    {
                        if (TryBorrowWordProductRows(source, result, y, workspace, out var firstProducts, out var secondProducts))
                            LanczosFmaResize.HorizontalExpandedWordPair(source, sy, next, horizontal, row, nextRow,
                                firstProducts, secondProducts, context);
                        else LanczosFmaResize.HorizontalWordPair(source, sy, next, horizontal, row, nextRow, workspace.Products, context);
                    }
                    else LanczosFmaResize.HorizontalBytePair(source, sy, next, horizontal, row, nextRow, context);
                    ids[nextSlot] = next;
                }
                else LanczosFmaResize.Horizontal(source, sy, horizontal, row,
                    typeof(TRows) == typeof(PairedWordRows)
                        ? workspace.Products.AsSpan(0, source.Width * 4) : workspace.Products, context);
                ids[slot] = sy;
            }
            // The ascending union keeps each output's original nonzero-tap
            // sequence. Consume every current user before any later row can
            // reuse its ring slot; cached rows may always be reconstructed.
            if (weight0 == 0) LanczosFmaResize.Accumulate(row, second, weight1, context);
            else if (weight1 == 0) LanczosFmaResize.Accumulate(row, first, weight0, context);
            else LanczosFmaResize.AccumulatePair(row, first, second, weight0, weight1, context);
        }
        context.Cancellation.ThrowIfCancellationRequested();
        LanczosFmaResize.WriteRow(source, result, y, horizontal, vertical, first,
            numeratorError, alphaError, maximum, context);
        context.Cancellation.ThrowIfCancellationRequested();
        LanczosFmaResize.WriteRow(source, result, y + 1, horizontal, vertical, second,
            numeratorError, alphaError, maximum, context);
    }

    private static bool TryBorrowWordProductRows(ImageBuffer source, ImageBuffer result, int y,
        LanczosWorkspace workspace, out Span<double> first, out Span<double> second)
    {
        first = second = default;
        var bytes = source.Width * 32L;
        var rows = (bytes + result.Stride - 1) / result.Stride;
        var startRow = result.Height - 4L - rows;
        if (bytes > int.MaxValue || startRow < y || workspace.Products.Length < source.Width * 8L) return false;
        // The two UInt32 rows already owned by the plan hold one exact double
        // row. The second source row precedes the active final-four-row sum.
        // Both source views retire before this output pair is published; a
        // later pair cannot overwrite any completed output row in the loan.
        first = MemoryMarshal.Cast<uint, double>(workspace.Products.AsSpan()).Slice(0, source.Width * 4);
        second = MemoryMarshal.Cast<byte, double>(result.Pixels.Slice((int)(startRow * result.Stride), (int)bytes));
        return true;
    }

    private static void WriteExactRow(ImageBuffer result, int y, ReadOnlySpan<Int128> sums,
        int maximum, OperationContext context)
    {
        Span<byte> destination8 = default; Span<ushort> destination16 = default;
        if (maximum == 255) destination8 = result.GetRow8(y); else destination16 = result.GetRow16(y);
        for (var x = 0; x < result.Width; x++)
        {
            if ((x & 1023) == 0) context.Cancellation.ThrowIfCancellationRequested();
            var at = x * 4; var pixel = Normalize(sums, at, maximum);
            var r = pixel.GetElement(0); var g = pixel.GetElement(1);
            var b = pixel.GetElement(2); var a = pixel.GetElement(3);
            if (maximum == 255)
            {
                destination8[at] = (byte)r; destination8[at + 1] = (byte)g;
                destination8[at + 2] = (byte)b; destination8[at + 3] = (byte)a;
            }
            else
            {
                destination16[at] = (ushort)r; destination16[at + 1] = (ushort)g;
                destination16[at + 2] = (ushort)b; destination16[at + 3] = (ushort)a;
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static Vector128<int> Normalize(ReadOnlySpan<Int128> sums, int index, int maximum)
    {
        if (Avx2.IsSupported && Avx512DQ.VL.IsSupported)
        {
            var limbs = MemoryMarshal.Cast<Int128, long>(sums);
            var low = Avx512DQ.VL.ConvertToVector256Double(Vector256.Create(limbs.Slice(index, 4)));
            var high = Avx512DQ.VL.ConvertToVector256Double(Vector256.Create(limbs.Slice(sums.Length + index, 4)));
            var value = Avx.Add(Avx.Multiply(high, Vector256.Create(4294967296d)), low);
            var denominator = value.GetElement(3);
            // Certified limb bounds give conversion/combination error <2^44
            // for RGB and <2^28 for alpha. Below half Q60, all samples clear.
            const double alphaError = 268435456d, half = 576460752303423488d;
            if (denominator < half - alphaError) return Vector128<int>.Zero;
            if (denominator >= half + alphaError)
            {
                var divisor = Vector256.Create(denominator, denominator, denominator, 1152921504606846976d);
                var estimate = Avx.Min(Avx.Max(Avx.Divide(value, divisor), Vector256<double>.Zero), Vector256.Create((double)maximum));
                var rounded = Avx.RoundToNearestInteger(estimate);
                var distance = Avx.AndNot(Vector256.Create(-0d), Avx.Subtract(estimate, rounded));
                // Clipping is Lipschitz in numerator and positive denominator.
                // Including division/comparison errors, every clipped quotient
                // differs by <2^-10. Uncertain bins retain exact arithmetic.
                if (Avx.MoveMask(Avx.CompareLessThan(distance, Vector256.Create(.5 - 1d / 1024))) == 15)
                    return Avx.ConvertToVector128Int32WithTruncation(rounded);
            }
        }
        var alpha = ReadSum(sums, index + 3);
        var a = Round(alpha, (Int128)LanczosPlan.One * LanczosPlan.One, maximum);
        if (a == 0) return Vector128<int>.Zero;
        return Vector128.Create(Round(ReadSum(sums, index), alpha, maximum),
            Round(ReadSum(sums, index + 1), alpha, maximum), Round(ReadSum(sums, index + 2), alpha, maximum), a);
    }

    internal static void AccumulateVertical(ReadOnlySpan<long> row, Span<Int128> sums, int weight, OperationContext context)
    {
        if (Avx2.IsSupported)
        {
            // Each plan has sum(abs(weights)) <= 2^31. Split row = hi*2^32+lo
            // with signed Int32 limbs. The certified horizontal bound keeps hi
            // in Int32 even at 16 bits; either limb's every partial sum fits
            // Int64 (absolute value <= 2^62). Recombine exactly only at output.
            // Two contiguous planes reuse the original 16 bytes per channel.
            var limbs = MemoryMarshal.Cast<Int128, long>(sums);
            var low = limbs[..sums.Length]; var high = limbs[sums.Length..];
            var i = 0;
            if (Avx512F.IsSupported && sums.Length >= 8)
            {
                // Eight independent channels retain the same per-lane order
                // and prefix bounds; the four-channel tail remains below.
                var factor512 = Vector512.Create(weight);
                for (; i <= sums.Length - 8; i += 8)
                {
                    if ((i & 4095) == 0) context.Cancellation.ThrowIfCancellationRequested();
                    var value = Vector512.Create(row.Slice(i, 8));
                    var upper = Avx512F.Add(Avx512F.ShiftRightLogical(value.AsUInt64(), 32).AsInt32(),
                        Avx512F.ShiftRightLogical(value.AsUInt32(), 31).AsInt32());
                    Avx512F.Add(Vector512.Create(low.Slice(i, 8)), Avx512F.Multiply(value.AsInt32(), factor512)).CopyTo(low.Slice(i, 8));
                    Avx512F.Add(Vector512.Create(high.Slice(i, 8)), Avx512F.Multiply(upper, factor512)).CopyTo(high.Slice(i, 8));
                }
            }
            var factor = Vector256.Create(weight);
            for (; i < sums.Length; i += 4)
            {
                if ((i & 4095) == 0) context.Cancellation.ThrowIfCancellationRequested();
                var value = Vector256.Create(row.Slice(i, 4));
                var upper = Avx2.Add(Avx2.ShiftRightLogical(value.AsUInt64(), 32).AsInt32(),
                    Avx2.ShiftRightLogical(value.AsUInt32(), 31).AsInt32());
                Avx2.Add(Vector256.Create(low.Slice(i, 4)), Avx2.Multiply(value.AsInt32(), factor)).CopyTo(low.Slice(i, 4));
                Avx2.Add(Vector256.Create(high.Slice(i, 4)), Avx2.Multiply(upper, factor)).CopyTo(high.Slice(i, 4));
            }
        }
        else
            for (var i = 0; i < sums.Length; i++)
            { if ((i & 4095) == 0) context.Cancellation.ThrowIfCancellationRequested(); sums[i] += (Int128)row[i] * weight; }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static Int128 ReadSum(ReadOnlySpan<Int128> sums, int index)
    {
        if (!Avx2.IsSupported) return sums[index];
        var limbs = MemoryMarshal.Cast<Int128, long>(sums);
        return ((Int128)limbs[sums.Length + index] << 32) + limbs[index];
    }

    internal static bool PreparedHorizontalSupported(int bitDepth)
        => bitDepth == 16 && Avx2.IsSupported && Avx512DQ.VL.IsSupported;

    internal static void Horizontal(ImageBuffer source, int y, LanczosPlan plan, Span<long> output,
        Span<uint> products, OperationContext context)
    {
        context.Cancellation.ThrowIfCancellationRequested();
        ReadOnlySpan<byte> input8 = default; ReadOnlySpan<ushort> input16 = default;
        if (source.BitDepth == 8) input8 = source.GetRow8(y); else input16 = source.GetRow16(y);
        if (source.BitDepth == 8 && Avx2.IsSupported)
        { Horizontal8(input8, plan, output, context); return; }
        if (PreparedHorizontalSupported(source.BitDepth))
        { PrepareWordProducts(input16, products, context); FilterWordProducts(products, plan, output, context); return; }
        for (var x = 0; x < plan.Windows.Length; x++)
        {
            var window = plan.Windows[x]; context.Cancellation.ThrowIfCancellationRequested();
            long r = 0, g = 0, b = 0, alpha = 0;
            for (var i = 0; i < window.Count; i++)
            {
                if ((i & 1023) == 0) context.Cancellation.ThrowIfCancellationRequested();
                var weight = plan.Weights[window.Offset + i]; var at = (window.First + i) * 4;
                int a, red, green, blue;
                if (source.BitDepth == 8) { a = input8[at + 3]; red = input8[at]; green = input8[at + 1]; blue = input8[at + 2]; }
                else { a = input16[at + 3]; red = input16[at]; green = input16[at + 1]; blue = input16[at + 2]; }
                r += (long)a * red * weight; g += (long)a * green * weight; b += (long)a * blue * weight;
                alpha += (long)a * weight;
            }
            var destination = x * 4; output[destination] = r; output[destination + 1] = g;
            output[destination + 2] = b; output[destination + 3] = alpha;
        }
    }

    private static void Horizontal8(ReadOnlySpan<byte> input, LanczosPlan plan, Span<long> output, OperationContext context)
    {
        var keepRgb = Vector512.Create(Vector256.Create(-1, -1, -1, 0, -1, -1, -1, 0),
            Vector256.Create(-1, -1, -1, 0, -1, -1, -1, 0));
        var alphaOne = Vector512.Create(Vector256.Create(0, 0, 0, 1, 0, 0, 0, 1),
            Vector256.Create(0, 0, 0, 1, 0, 0, 0, 1));
        var replicate01 = Vector512.Create(0L, 0, 0, 0, 1, 1, 1, 1);
        var replicate23 = Vector512.Create(2L, 2, 2, 2, 3, 3, 3, 3);
        for (var x = 0; x < plan.Windows.Length; x++)
        {
            var window = plan.Windows[x]; context.Cancellation.ThrowIfCancellationRequested();
            var sum = Vector256<long>.Zero; var i = 0;
            if (Avx512F.IsSupported)
            {
                // Byte premultiplication is <=65025, so each even Int32
                // lane of the widened values remains a positive operand.
                // Four disjoint tap subsets and their reduction stay <2^47.
                var first = Vector512<long>.Zero; var second = Vector512<long>.Zero;
                for (; i <= window.Count - 4; i += 4)
                {
                    if ((i & 1023) == 0) context.Cancellation.ThrowIfCancellationRequested();
                    var samples = Avx512F.ConvertToVector512Int32(Vector128.Create(input.Slice((window.First + i) * 4, 16)));
                    var rgbOne = Avx512F.Or(Avx512F.And(samples, keepRgb), alphaOne);
                    var products = Avx512F.MultiplyLow(rgbOne, Avx512F.Shuffle(samples, 255)).AsUInt32();
                    var at = window.Offset + i;
                    var coefficients = Avx512F.ConvertToVector512Int64(Vector256.Create(Vector128.Create(plan.Weights.AsSpan(at, 4)), Vector128<int>.Zero));
                    var weights01 = Avx512F.PermuteVar8x64(coefficients, replicate01).AsInt32();
                    var weights23 = Avx512F.PermuteVar8x64(coefficients, replicate23).AsInt32();
                    first = Avx512F.Add(first, Avx512F.Multiply(Avx512F.ConvertToVector512Int64(products.GetLower()).AsInt32(), weights01));
                    second = Avx512F.Add(second, Avx512F.Multiply(Avx512F.ConvertToVector512Int64(products.GetUpper()).AsInt32(), weights23));
                }
                var paired = Avx512F.Add(first, second);
                sum = Avx2.Add(paired.GetLower(), paired.GetUpper());
            }
            for (; i < window.Count; i++)
            {
                if ((i & 1023) == 0) context.Cancellation.ThrowIfCancellationRequested();
                var samples = Sse41.ConvertToVector128Int32(Vector128.CreateScalar(MemoryMarshal.Read<uint>(input.Slice((window.First + i) * 4, 4))).AsByte());
                var premultiplied = Sse41.MultiplyLow(Sse41.Insert(samples, 1, 3), Sse2.Shuffle(samples, 255));
                // Byte products are <=65025, so signed32 operands suffice.
                // Even lanes of the widened values give four exact Int64
                // products; the axis bound keeps all prefixes below2^47.
                var values = Avx2.ConvertToVector256Int64(premultiplied).AsInt32();
                sum = Avx2.Add(sum, Avx2.Multiply(values, Vector256.Create(plan.Weights[window.Offset + i])));
            }
            sum.CopyTo(output.Slice(x * 4, 4));
        }
    }

    // Two required rows share only coefficient expansion. Keep the measured
    // single-row kernel above: unused second spans change its hot-loop ABI.
    internal static void HorizontalBytePair(ImageBuffer source, int y, int nextY, LanczosPlan plan,
        Span<long> output, Span<long> nextOutput, OperationContext context)
        => Horizontal8Pair(source.GetRow8(y), source.GetRow8(nextY), plan, output, nextOutput, context);

    private static void Horizontal8Pair(ReadOnlySpan<byte> input, ReadOnlySpan<byte> nextInput,
        LanczosPlan plan, Span<long> output, Span<long> nextOutput, OperationContext context)
    {
        var keepRgb = Vector512.Create(Vector256.Create(-1, -1, -1, 0, -1, -1, -1, 0),
            Vector256.Create(-1, -1, -1, 0, -1, -1, -1, 0));
        var alphaOne = Vector512.Create(Vector256.Create(0, 0, 0, 1, 0, 0, 0, 1),
            Vector256.Create(0, 0, 0, 1, 0, 0, 0, 1));
        var replicate01 = Vector512.Create(0L, 0, 0, 0, 1, 1, 1, 1);
        var replicate23 = Vector512.Create(2L, 2, 2, 2, 3, 3, 3, 3);
        for (var x = 0; x < plan.Windows.Length; x++)
        {
            var window = plan.Windows[x]; context.Cancellation.ThrowIfCancellationRequested();
            var sum = Vector256<long>.Zero; var nextSum = Vector256<long>.Zero; var i = 0;
            if (Avx512F.IsSupported)
            {
                // Byte premultiplication is <=65025, so each even Int32
                // lane of the widened values remains a positive operand.
                // Four disjoint tap subsets and their reduction stay <2^47.
                var first = Vector512<long>.Zero; var second = Vector512<long>.Zero;
                var nextFirst = Vector512<long>.Zero; var nextSecond = Vector512<long>.Zero;
                for (; i <= window.Count - 4; i += 4)
                {
                    if ((i & 1023) == 0) context.Cancellation.ThrowIfCancellationRequested();
                    var samples = Avx512F.ConvertToVector512Int32(Vector128.Create(input.Slice((window.First + i) * 4, 16)));
                    var rgbOne = Avx512F.Or(Avx512F.And(samples, keepRgb), alphaOne);
                    var products = Avx512F.MultiplyLow(rgbOne, Avx512F.Shuffle(samples, 255)).AsUInt32();
                    var at = window.Offset + i;
                    var coefficients = Avx512F.ConvertToVector512Int64(Vector256.Create(Vector128.Create(plan.Weights.AsSpan(at, 4)), Vector128<int>.Zero));
                    var weights01 = Avx512F.PermuteVar8x64(coefficients, replicate01).AsInt32();
                    var weights23 = Avx512F.PermuteVar8x64(coefficients, replicate23).AsInt32();
                    first = Avx512F.Add(first, Avx512F.Multiply(Avx512F.ConvertToVector512Int64(products.GetLower()).AsInt32(), weights01));
                    second = Avx512F.Add(second, Avx512F.Multiply(Avx512F.ConvertToVector512Int64(products.GetUpper()).AsInt32(), weights23));
                    var nextSamples = Avx512F.ConvertToVector512Int32(Vector128.Create(nextInput.Slice((window.First + i) * 4, 16)));
                    var nextRgbOne = Avx512F.Or(Avx512F.And(nextSamples, keepRgb), alphaOne);
                    var nextProducts = Avx512F.MultiplyLow(nextRgbOne, Avx512F.Shuffle(nextSamples, 255)).AsUInt32();
                    nextFirst = Avx512F.Add(nextFirst, Avx512F.Multiply(Avx512F.ConvertToVector512Int64(nextProducts.GetLower()).AsInt32(), weights01));
                    nextSecond = Avx512F.Add(nextSecond, Avx512F.Multiply(Avx512F.ConvertToVector512Int64(nextProducts.GetUpper()).AsInt32(), weights23));
                }
                var paired = Avx512F.Add(first, second);
                sum = Avx2.Add(paired.GetLower(), paired.GetUpper());
                var nextPaired = Avx512F.Add(nextFirst, nextSecond);
                nextSum = Avx2.Add(nextPaired.GetLower(), nextPaired.GetUpper());
            }
            for (; i < window.Count; i++)
            {
                if ((i & 1023) == 0) context.Cancellation.ThrowIfCancellationRequested();
                var samples = Sse41.ConvertToVector128Int32(Vector128.CreateScalar(MemoryMarshal.Read<uint>(input.Slice((window.First + i) * 4, 4))).AsByte());
                var premultiplied = Sse41.MultiplyLow(Sse41.Insert(samples, 1, 3), Sse2.Shuffle(samples, 255));
                // Byte products are <=65025, so signed32 operands suffice.
                // Even lanes of the widened values give four exact Int64
                // products; the axis bound keeps all prefixes below2^47.
                var values = Avx2.ConvertToVector256Int64(premultiplied).AsInt32();
                var weight = Vector256.Create(plan.Weights[window.Offset + i]);
                sum = Avx2.Add(sum, Avx2.Multiply(values, weight));
                var nextSamples = Sse41.ConvertToVector128Int32(Vector128.CreateScalar(MemoryMarshal.Read<uint>(nextInput.Slice((window.First + i) * 4, 4))).AsByte());
                var nextPremultiplied = Sse41.MultiplyLow(Sse41.Insert(nextSamples, 1, 3), Sse2.Shuffle(nextSamples, 255));
                var nextValues = Avx2.ConvertToVector256Int64(nextPremultiplied).AsInt32();
                nextSum = Avx2.Add(nextSum, Avx2.Multiply(nextValues, weight));
            }
            sum.CopyTo(output.Slice(x * 4, 4));
            nextSum.CopyTo(nextOutput.Slice(x * 4, 4));
        }
    }


    // Source pixels are shared by overlapping output windows. Form their exact
    // unsigned word alpha products once, then borrow them for the entire row.
    internal static void PrepareWordProducts(ReadOnlySpan<ushort> input, Span<uint> products,
        OperationContext context)
    {
        var at = 0;
        if (Avx512F.IsSupported)
        {
            var keepRgb = Vector512.Create(Vector256.Create(-1, -1, -1, 0, -1, -1, -1, 0),
                Vector256.Create(-1, -1, -1, 0, -1, -1, -1, 0));
            var alphaOne = Vector512.Create(Vector256.Create(0, 0, 0, 1, 0, 0, 0, 1),
                Vector256.Create(0, 0, 0, 1, 0, 0, 0, 1));
            for (; at <= input.Length - 16; at += 16)
            {
                if ((at & 1023) == 0) context.Cancellation.ThrowIfCancellationRequested();
                var samples = Avx512F.ConvertToVector512Int32(Vector256.Create(input.Slice(at, 16)));
                var rgbOne = Avx512F.Or(Avx512F.And(samples, keepRgb), alphaOne);
                Avx512F.MultiplyLow(rgbOne, Avx512F.Shuffle(samples, 255)).AsUInt32().CopyTo(products.Slice(at, 16));
            }
        }
        for (; at < input.Length; at += 4)
        {
            if ((at & 1023) == 0) context.Cancellation.ThrowIfCancellationRequested();
            var alpha = (uint)input[at + 3];
            products[at] = alpha * (uint)input[at];
            products[at + 1] = alpha * (uint)input[at + 1];
            products[at + 2] = alpha * (uint)input[at + 2];
            products[at + 3] = alpha;
        }
    }

    private static void FilterWordProducts(ReadOnlySpan<uint> input, LanczosPlan plan,
        Span<long> output, OperationContext context)
    {
        var replicate01 = Vector512.Create(0L, 0, 0, 0, 1, 1, 1, 1);
        var replicate23 = Vector512.Create(2L, 2, 2, 2, 3, 3, 3, 3);
        for (var x = 0; x < plan.Windows.Length; x++)
        {
            var window = plan.Windows[x]; context.Cancellation.ThrowIfCancellationRequested();
            var sum = Vector256<long>.Zero; var i = 0;
            if (Avx512DQ.IsSupported)
            {
                // Q30 absolute-weight bounds certify every disjoint subset
                // and its reduction in signed64, including full RGBA16 products.
                var first = Vector512<long>.Zero; var second = Vector512<long>.Zero;
                for (; i <= window.Count - 4; i += 4)
                {
                    if ((i & 1023) == 0) context.Cancellation.ThrowIfCancellationRequested();
                    var products = Vector512.Create(input.Slice((window.First + i) * 4, 16));
                    var coefficients = Avx512F.ConvertToVector512Int64(Vector256.Create(Vector128.Create(plan.Weights.AsSpan(window.Offset + i, 4)), Vector128<int>.Zero));
                    var weights01 = Avx512F.PermuteVar8x64(coefficients, replicate01);
                    var weights23 = Avx512F.PermuteVar8x64(coefficients, replicate23);
                    var values01 = Avx512F.ConvertToVector512Int64(products.GetLower());
                    var values23 = Avx512F.ConvertToVector512Int64(products.GetUpper());
                    first = Avx512F.Add(first, Avx512DQ.MultiplyLow(values01, weights01));
                    second = Avx512F.Add(second, Avx512DQ.MultiplyLow(values23, weights23));
                }
                var paired = Avx512F.Add(first, second);
                sum = Avx2.Add(paired.GetLower(), paired.GetUpper());
            }
            for (; i < window.Count; i++)
            {
                if ((i & 1023) == 0) context.Cancellation.ThrowIfCancellationRequested();
                var values = Avx2.ConvertToVector256Int64(Vector128.Create(input.Slice((window.First + i) * 4, 4)));
                var weight = plan.Weights[window.Offset + i];
                var weighted = Avx512DQ.VL.MultiplyLow(values, Vector256.Create((long)weight));
                sum = Avx2.Add(sum, weighted);
            }
            sum.CopyTo(output.Slice(x * 4, 4));
        }
    }

    internal static int Round(Int128 numerator, Int128 denominator, int maximum)
    {
        if (numerator <= 0) return 0;
        if (numerator >= denominator * maximum) return maximum;
        // The image arithmetic fits 96 bits. Drop 32 low bits to cheaply
        // estimate using UInt64; no discarded bit decides a pixel. Clamp the
        // candidate so its product fits the already checked sample range, then
        // certify its exact remainder. This also handles wider synthetic inputs.
        var projectedDenominator = (ulong)(denominator >> 32);
        if (projectedDenominator != 0)
        {
            var estimate = (double)(ulong)(numerator >> 32) / projectedDenominator;
            // In this domain, truncation contributes at most
            // (maximum+1)/(2^27-1); binary64 conversion/division add <2^-34.
            // Their sum is <2^-10. Accept only a complete rounding bin;
            // uncertain half boundaries keep the exact remainder below.
            if (maximum <= 65535 && denominator >= (Int128)1 << 59 && denominator < (Int128)1 << 80)
            {
                var rounded = (int)(estimate + .5);
                if (Math.Abs(estimate - rounded) < .5 - 1d / 1024) return rounded;
            }
            var quotient = Math.Clamp((int)estimate, 0, maximum);
            var remainder = numerator - denominator * quotient;
            if (remainder < 0) { quotient--; remainder += denominator; }
            else if (remainder >= denominator) { quotient++; remainder -= denominator; }
            if (remainder >= 0 && remainder < denominator)
                return quotient + (remainder * 2 >= denominator ? 1 : 0);
        }
        var (exact, rest) = Int128.DivRem(numerator, denominator);
        return (int)(exact + (rest * 2 >= denominator ? 1 : 0));
    }
}
