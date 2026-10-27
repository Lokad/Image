using System.Runtime.CompilerServices;

namespace Lokad.Image.Internal;

// Private encoder preparation. One operation-owned allocation contains generated
// costs; probabilities and caller buffers remain borrowed. RFC6386 sections11/13
// supply the mode/coefficient trees and category probabilities. No implementation
// cost tables are imported. The public encoder borrows this operation-local plan.
internal sealed partial class Vp8RatePlan : IDisposable
{
    private const int FixedAt = 256, ModesAt = FixedAt + 2048, EobAt = ModesAt + 1008;
    private const int LevelsAt = EobAt + 192, Length = LevelsAt + 4 * 8 * 3 * 68;
    private readonly OperationContext context;
    private ushort[]? storage;
    private long capacity;
    private Vp8RatePlan(OperationContext context) => this.context = context;
    internal long CapacityBytes => capacity;
    internal ReadOnlySpan<ushort> Values => storage ?? throw new ObjectDisposedException(nameof(Vp8RatePlan));
    internal ReadOnlySpan<ushort> Entropy => Values[..FixedAt];
    internal ReadOnlySpan<ushort> FixedLevels => Values.Slice(FixedAt, 2048);
    internal ReadOnlySpan<ushort> Modes => Values.Slice(ModesAt, 1008);
    internal ReadOnlySpan<ushort> EndOfBlock => Values.Slice(EobAt, 192);
    internal ReadOnlySpan<ushort> VariableLevels => Values[LevelsAt..];

    // Cost is ceil(256 * mean renormalization shifts) over the repeated-zero
    // orbit of the inclusive boolean range. Each normalized range is128..255;
    // its count-minus-one is127..254, hence at most128states. This independently
    // generates the pinned integer profile, including probabilities0/255.
    private static int EntropyCost(int probability, Span<int> seen, Span<int> shifts)
    {
        seen.Fill(-1); var range = 127; var step = 0; var total = 0;
        while (seen[range - 127] < 0)
        {
            seen[range - 127] = step; shifts[range - 127] = total;
            var count = (range * probability >> 8) + 1; var shift = 0;
            while (count < 128) { count <<= 1; shift++; }
            range = count - 1; step++; total += shift;
        }
        var period = step - seen[range - 127];
        return ((total - shifts[range - 127]) * 256 + period - 1) / period;
    }

    private static int Bit(ReadOnlySpan<ushort> entropy, bool bit, int probability)
        => entropy[bit ? 255 - probability : probability];

    private static ReadOnlySpan<byte> Category(int index) => index switch
    {
        0 => [159], 1 => [165, 145], 2 => [173, 148, 140],
        3 => [176, 155, 140, 135], 4 => [180, 157, 141, 134, 130],
        _ => [254, 254, 243, 230, 196, 177, 153, 140, 133, 130, 129]
    };

    private static int FixedCost(int level, ReadOnlySpan<ushort> entropy)
    {
        if (level == 0) return 0;
        var cost = 256; // Uniform sign bit.
        if (level < 5) return cost;
        // Pinned RDO pricing omits category-two's low bit for9/10. The actual
        // coefficient syntax still writes it; this is solely a decision cost.
        if (level is 9 or 10) return cost + Bit(entropy, true, 165);
        var category = 0; var start = 5; var bits = 1;
        while (category < 5 && level >= start + (1 << bits))
        { start += 1 << bits; bits++; category++; }
        var probabilities = Category(category); var value = level - start;
        for (var p = 0; p < probabilities.Length; p++)
            cost += Bit(entropy, (value & (1 << (probabilities.Length - 1 - p))) != 0, probabilities[p]);
        return cost;
    }

    private static int Geometry(int mode) => mode switch { 4 => 5, 5 => 6, 6 => 4, _ => mode };

    private static int SmallMode(int mode, ReadOnlySpan<byte> p, ReadOnlySpan<ushort> entropy)
    {
        var cost = Bit(entropy, mode != 0, p[0]); if (mode == 0) return cost;
        cost += Bit(entropy, mode != 1, p[1]); if (mode == 1) return cost;
        cost += Bit(entropy, mode != 2, p[2]); if (mode == 2) return cost;
        cost += Bit(entropy, mode >= 6, p[3]);
        if (mode < 6)
        {
            cost += Bit(entropy, mode != 3, p[4]);
            if (mode != 3) cost += Bit(entropy, mode == 5, p[5]);
        }
        else
        {
            cost += Bit(entropy, mode != 6, p[6]);
            if (mode != 6)
            {
                cost += Bit(entropy, mode != 7, p[7]);
                if (mode != 7) cost += Bit(entropy, mode == 9, p[8]);
            }
        }
        return cost;
    }

    private static int VariableCost(int level, ReadOnlySpan<byte> p, ReadOnlySpan<ushort> entropy)
    {
        var cost = Bit(entropy, level != 0, p[1]); if (level == 0) return cost;
        cost += Bit(entropy, level > 1, p[2]); if (level == 1) return cost;
        cost += Bit(entropy, level > 4, p[3]);
        if (level <= 4)
        {
            cost += Bit(entropy, level != 2, p[4]);
            if (level != 2) cost += Bit(entropy, level == 4, p[5]);
        }
        else
        {
            cost += Bit(entropy, level > 10, p[6]);
            if (level <= 10) cost += Bit(entropy, level > 6, p[7]);
            else
            {
                cost += Bit(entropy, level >= 35, p[8]);
                cost += level >= 35 ? Bit(entropy, level >= 67, p[10]) : Bit(entropy, level >= 19, p[9]);
            }
        }
        return cost;
    }

    internal static Vp8RatePlan Create(ReadOnlySpan<byte> probabilities, OperationContext context)
    {
        if (probabilities.Length != 1056) throw new ArgumentException("VP8 requires 1,056 coefficient probabilities.", nameof(probabilities));
        context.Cancellation.ThrowIfCancellationRequested(); var result = new Vp8RatePlan(context);
        try
        {
            const int bytes = Length * sizeof(ushort) + 128;
            context.Reserve(bytes); result.capacity = bytes;
            context.Cancellation.ThrowIfCancellationRequested(); result.storage = new ushort[Length]; var values = result.storage.AsSpan();
            context.Reserve(1024);
            try
            {
                Span<int> seen = stackalloc int[128]; Span<int> shifts = stackalloc int[128];
                for (var p = 0; p < 256; p++)
                { context.Cancellation.ThrowIfCancellationRequested(); values[p] = (ushort)EntropyCost(p, seen, shifts); }
                ReadOnlySpan<ushort> entropy = values[..FixedAt];
                for (var p = 0; p < 2048; p++)
                { if ((p & 127) == 0) context.Cancellation.ThrowIfCancellationRequested(); values[FixedAt + p] = (ushort)FixedCost(p, entropy); }
                for (var mode = 0; mode < 4; mode++)
                {
                    var branch = mode is 1 or 3;
                    values[ModesAt + mode] = (ushort)(Bit(entropy, true, 145) + Bit(entropy, branch, 156)
                        + Bit(entropy, branch ? mode == 1 : mode == 2, branch ? 128 : 163));
                    // Pinned UV RDO prices invert the first two syntax decisions.
                    // Preserve the pricing tree without changing emitted syntax.
                    var uv = Bit(entropy, mode == 0, 142);
                    if (mode != 0) uv += Bit(entropy, mode == 2, 114);
                    if (mode is 1 or 3) uv += Bit(entropy, mode == 1, 183);
                    values[ModesAt + 4 + mode] = (ushort)uv;
                }
                for (var top = 0; top < 10; top++) for (var left = 0; left < 10; left++)
                {
                    context.Cancellation.ThrowIfCancellationRequested();
                    var p = Vp8Tables.BlockModeProbabilities.Slice((Geometry(top) * 10 + Geometry(left)) * 9, 9);
                    for (var mode = 0; mode < 10; mode++)
                        values[ModesAt + 8 + (top * 10 + left) * 10 + mode] = (ushort)SmallMode(mode, p, entropy);
                }
                FillCoefficientCosts(values, probabilities, context);
                return result;
            }
            finally { context.Release(1024); }
        }
        catch { result.Dispose(); throw; }
    }

    // The frame borrows no table view across probability updates. Keep its
    // operation-owned capacity and invariant prices; no cache or new allowance.
    internal void Refresh(ReadOnlySpan<byte> probabilities)
    {
        if (probabilities.Length != 1056) throw new ArgumentException("VP8 requires 1,056 coefficient probabilities.", nameof(probabilities));
        _ = Values; context.Cancellation.ThrowIfCancellationRequested();
        FillCoefficientCosts(storage!, probabilities, context);
    }

    private static void FillCoefficientCosts(Span<ushort> values, ReadOnlySpan<byte> probabilities, OperationContext context)
    {
        ReadOnlySpan<ushort> entropy = values[..FixedAt];
        for (var index = 0; index < 96; index++)
        {
            context.Cancellation.ThrowIfCancellationRequested(); var p = probabilities.Slice(index * 11, 11); var extra = index % 3 == 0 ? 0 : Bit(entropy, true, p[0]);
            values[EobAt + index * 2] = (ushort)Bit(entropy, false, p[0]);
            values[EobAt + index * 2 + 1] = (ushort)Bit(entropy, true, p[0]);
            for (var level = 0; level < 68; level++)
                values[LevelsAt + index * 68 + level] = (ushort)(extra + VariableCost(level, p, entropy));
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int Band(int position) => position switch
    { 0 => 0, 1 => 1, 2 => 2, 3 => 3, 5 => 4, 6 => 5, 15 => 7, _ => 6 };

    // Borrowed zigzag levels and integer result only. The enclosing encoder
    // admits block cancellation in batches, without per-block scopes.
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal int Residual(ReadOnlySpan<short> levels, int type, int first, int initialContext, out bool nonzero)
    {
        var values = Values;
        if (System.Runtime.Intrinsics.X86.Avx2.IsSupported)
            return ResidualVectors(values, levels, type, first, initialContext, out nonzero);
        if (levels.Length < 16 || (uint)type > 3 || (uint)first > 1 || (uint)initialContext > 2 || first == 1 && levels[0] != 0)
            throw new ArgumentException("Invalid VP8 residual layout or context.");
        var last = -1;
        for (var p = first; p < 16; p++)
        {
            if (levels[p] is < -2047 or > 2047) throw new ArgumentException("VP8 residual level exceeds 2,047.");
            if (levels[p] != 0) last = p;
        }
        nonzero = last >= 0; var index = (type * 8 + Band(first)) * 3 + initialContext;
        if (!nonzero) return values[EobAt + index * 2];
        var cost = initialContext == 0 ? values[EobAt + index * 2 + 1] : 0;
        for (var p = first; p <= last; p++)
        {
            var level = Math.Abs((int)levels[p]);
            cost += values[FixedAt + level] + values[LevelsAt + index * 68 + Math.Min(level, 67)];
            if (p < 15) index = (type * 8 + Band(p + 1)) * 3 + Math.Min(level, 2);
        }
        if (last < 15) cost += values[EobAt + index * 2];
        return cost;
    }

    public void Dispose()
    {
        Interlocked.Exchange(ref storage, null);
        var owned = Interlocked.Exchange(ref capacity, 0);
        if (owned != 0) context.Release(owned);
    }
}
