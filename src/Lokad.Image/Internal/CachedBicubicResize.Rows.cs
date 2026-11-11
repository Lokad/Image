using System.Buffers.Binary;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace Lokad.Image.Internal;

internal static partial class CachedBicubicResize
{
    private readonly struct PollRow { }
    private readonly struct NoRowPolling { }

    // Keep conversion in one small body. The two operation-selected polling
    // modes remove policy/context accesses from ordinary row arithmetic.
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void DecodePremultipliedRow<TPolling>(ReadOnlySpan<byte> row,Span<double> decoded,
        int width,bool wide16,CancellationToken cancellation)
    {
        var sx=0;
        for(;sx+1<width;sx+=2)
        {
            if(typeof(TPolling)==typeof(PollRow))cancellation.ThrowIfCancellationRequested();
            var channels=wide16
                ?Avx2.ConvertToVector256Int32(MemoryMarshal.Read<Vector128<ushort>>(row.Slice(sx*8,16)))
                :Avx2.ConvertToVector256Int32(Vector128.CreateScalar(BinaryPrimitives.ReadUInt64LittleEndian(row.Slice(sx*4,8))).AsByte());
            var factors=Avx2.Blend(Avx2.Shuffle(channels,255),Vector256.Create(1),136);
            // UInt16 products can exceed Int32.MaxValue. Convert the
            // unsigned low products directly; every product fits
            // UInt32 and remains exact in binary64.
            var values=Avx512F.ConvertToVector512Double(Avx2.MultiplyLow(channels,factors).AsUInt32());
            MemoryMarshal.Write(MemoryMarshal.AsBytes(decoded.Slice(sx*4,8)),in values);
        }
        for(;sx<width;sx++)
        {
            if(typeof(TPolling)==typeof(PollRow))cancellation.ThrowIfCancellationRequested();
            var pixel=wide16?BinaryPrimitives.ReadUInt64LittleEndian(row.Slice(sx*8,8)):BinaryPrimitives.ReadUInt32LittleEndian(row.Slice(sx*4,4));
            var channels=wide16?Sse41.ConvertToVector128Int32(Vector128.CreateScalar(pixel).AsUInt16())
                :Sse41.ConvertToVector128Int32(Vector128.CreateScalar((uint)pixel).AsByte());
            var a=(int)(wide16?pixel>>48:pixel>>24);
            var premult=Sse41.MultiplyLow(channels,Vector128.Create(a,a,a,1));
            var values=wide16?Avx512F.ConvertToVector512Double(Vector256.Create(premult.AsUInt32(),Vector128<uint>.Zero)).GetLower()
                :Avx.ConvertToVector256Double(premult);
            MemoryMarshal.Write(MemoryMarshal.AsBytes(decoded.Slice(sx*4,4)),in values);
        }
    }

    internal static void PrepareOverlappingHorizontalPairs(Span<int> offsets,ReadOnlySpan<int> counts,
        int width,int sourceWidth,OperationContext context)
    {
        // Small-support geometry has sixteen slots per output. Eight-tap pairs
        // never read slot fifteen, including during exact-result repair. Use it
        // for the prepared source start plus one (zero means irregular).
        for(var x=0;x+1<width;x+=2)
        {
            context.Cancellation.ThrowIfCancellationRequested();
            if(counts[x]!=8 || counts[x+1]!=8)continue;
            var base0=x*16;var base1=base0+16;
            var first=offsets[base0];
            if(first>sourceWidth*4-40 || offsets[base1]!=first+8)continue;
            var contiguous=true;
            for(var tap=0;tap<8;tap++)
                contiguous &= offsets[base0+tap]==first+tap*4 && offsets[base1+tap]==first+(tap+2)*4;
            if(contiguous)offsets[base0+15]=first+1;
        }
    }

    internal static void FilterHorizontal(ReadOnlySpan<double> decoded,Span<double> cached,
        ReadOnlySpan<double> weights,ReadOnlySpan<int> offsets,ReadOnlySpan<int> counts,
        int width,int capacity,bool adjacent,OperationContext context)
    {
        var x=0;
        for(;x+1<width;x+=2)
        {
            if (Avx512F.IsSupported && capacity == 16 && x + 3 < width && offsets[x * 16 + 14] == PreparedHorizontalQuartet)
            {
                FilterHorizontalQuartet<NarrowHorizontalQuartet>(
                    MemoryMarshal.Cast<double,Vector256<double>>(decoded.Slice(offsets[x * 16], 56)),
                    weights.Slice(x * 16, 64),
                    MemoryMarshal.Cast<double,Vector256<double>>(cached.Slice(x * 4, 16)), context);
                x += 2;
                continue;
            }
            context.Cancellation.ThrowIfCancellationRequested();
            var base0=x*capacity;var base1=(x+1)*capacity;
            var prepared=offsets[base0+15];
            if(prepared!=0)
            {
                var first=prepared-1;
                var sum=BicubicMath.FilterOverlappingHorizontalPair(
                    MemoryMarshal.Cast<double,Vector256<double>>(decoded.Slice(first,40)),
                    weights.Slice(base0,8),weights.Slice(base1,8),8,2);
                MemoryMarshal.Write(MemoryMarshal.AsBytes(cached.Slice(x*4,8)),in sum);
                continue;
            }
            var sum0=Vector256<double>.Zero;
            var sum1=Vector256<double>.Zero;
            var first0=offsets[base0];var first1=offsets[base1];
            if((adjacent) && counts[x]==4 && counts[x+1]==4
                && first1==first0+4 && first1<=decoded.Length-16)
            {
                // Interior adjacent four-tap outputs share three
                // source vectors. Keep each accumulation in tap order.
                var p0=MemoryMarshal.Read<Vector256<double>>(MemoryMarshal.AsBytes(decoded.Slice(first0,4)));
                var p1=MemoryMarshal.Read<Vector256<double>>(MemoryMarshal.AsBytes(decoded.Slice(first1,4)));
                var p2=MemoryMarshal.Read<Vector256<double>>(MemoryMarshal.AsBytes(decoded.Slice(first1+4,4)));
                var p3=MemoryMarshal.Read<Vector256<double>>(MemoryMarshal.AsBytes(decoded.Slice(first1+8,4)));
                var p4=MemoryMarshal.Read<Vector256<double>>(MemoryMarshal.AsBytes(decoded.Slice(first1+12,4)));
                sum0=Avx.Add(sum0,Avx.Multiply(p0,Vector256.Create(weights[base0])));
                sum1=Avx.Add(sum1,Avx.Multiply(p1,Vector256.Create(weights[base1])));
                sum0=Avx.Add(sum0,Avx.Multiply(p1,Vector256.Create(weights[base0+1])));
                sum1=Avx.Add(sum1,Avx.Multiply(p2,Vector256.Create(weights[base1+1])));
                sum0=Avx.Add(sum0,Avx.Multiply(p2,Vector256.Create(weights[base0+2])));
                sum1=Avx.Add(sum1,Avx.Multiply(p3,Vector256.Create(weights[base1+2])));
                sum0=Avx.Add(sum0,Avx.Multiply(p3,Vector256.Create(weights[base0+3])));
                sum1=Avx.Add(sum1,Avx.Multiply(p4,Vector256.Create(weights[base1+3])));
            }
            else
            {
                var n=Math.Max(counts[x],counts[x+1]);
                var tapOffsets0=offsets.Slice(base0,n);
                var tapOffsets1=offsets.Slice(base1,n);
                var tapWeights0=weights.Slice(base0,n);
                var tapWeights1=weights.Slice(base1,n);
                // Geometry offsets address four-double pixels. Bound
                // each tap table once and index the borrowed vector
                // view without constructing a slice for every sample.
                var decodedPixels=MemoryMarshal.Cast<double,Vector256<double>>(decoded);
                for(var tx=0;tx<n;tx++)
                {
                    var p0=decodedPixels[tapOffsets0[tx]>>2];
                    var p1=decodedPixels[tapOffsets1[tx]>>2];
                    var w0=Vector256.Create(tapWeights0[tx]);
                    var w1=Vector256.Create(tapWeights1[tx]);
                    // Independent accumulators avoid variable-half
                    // wide-vector construction and its stack traffic.
                    sum0=Avx.Add(sum0,Avx.Multiply(p0,w0));
                    sum1=Avx.Add(sum1,Avx.Multiply(p1,w1));
                }
            }
            MemoryMarshal.Write(MemoryMarshal.AsBytes(cached.Slice(x*4,4)),in sum0);
            MemoryMarshal.Write(MemoryMarshal.AsBytes(cached.Slice((x+1)*4,4)),in sum1);
        }
        if(x<width)
        {
            var sum=Vector256<double>.Zero;
            for(var tx=0;tx<counts[x];tx++)
                sum=Avx.Add(sum,Avx.Multiply(MemoryMarshal.Read<Vector256<double>>(MemoryMarshal.AsBytes(decoded.Slice(offsets[x*capacity+tx],4))),Vector256.Create(weights[x*capacity+tx])));
            MemoryMarshal.Write(MemoryMarshal.AsBytes(cached.Slice(x*4,4)),in sum);
        }
    }

    private const int OverlappingWidePair = -2;
    private const int PreparedHorizontalQuartet = -3;
    private readonly struct WideHorizontalQuartet { }
    private readonly struct NarrowHorizontalQuartet { }

    // Eight/sixteen live taps leave the same number of unused doubles per
    // record. Four tails hold interleaved coefficients without extra storage.
    private static void PrepareHorizontalQuartets<TGeometry>(Span<double> weights, Span<int> offsets,
        ReadOnlySpan<int> counts, int width, int sourceWidth, OperationContext context)
    {
        var wide = typeof(TGeometry) == typeof(WideHorizontalQuartet);
        var taps = wide ? 16 : 8; var capacity = taps * 2; var pixels = wide ? 28 : 14;
        for (var x = 0; x + 3 < width; x += 2)
        {
            context.Cancellation.ThrowIfCancellationRequested();
            var at = x * capacity;
            if (counts[x] != taps || counts[x + 1] != taps || counts[x + 2] != taps || counts[x + 3] != taps)
                continue;
            var first = offsets[at];
            if (wide
                ? offsets[at + capacity - 1] != OverlappingWidePair || offsets[at + 3 * capacity - 1] != OverlappingWidePair
                : offsets[at + capacity - 1] != first + 1 || offsets[at + 3 * capacity - 1] != offsets[at + 2 * capacity] + 1)
                continue;
            var pixel = wide ? first : first >> 2;
            if (sourceWidth < pixels || (uint)pixel > (uint)(sourceWidth - pixels)
                || offsets[at + 2 * capacity] != first + (wide ? 8 : 16))
                continue;
            for (var tap = 0; tap < taps; tap++)
            {
                var target = at + (wide ? ((tap >> 2) << 5) + 16 + ((tap & 3) << 2)
                    : ((tap >> 1) << 4) + 8 + ((tap & 1) << 2));
                weights[target] = weights[at + tap]; weights[target + 1] = weights[at + capacity + tap];
                weights[target + 2] = weights[at + 2 * capacity + tap]; weights[target + 3] = weights[at + 3 * capacity + tap];
            }
            offsets[at + capacity - 2] = PreparedHorizontalQuartet;
            x += 2;
        }
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void FilterHorizontalQuartet<TGeometry>(ReadOnlySpan<Vector256<double>> decoded,
        ReadOnlySpan<double> weights, Span<Vector256<double>> cached, OperationContext context)
    {
        context.Cancellation.ThrowIfCancellationRequested();
        var wide = typeof(TGeometry) == typeof(WideHorizontalQuartet);
        if (wide) { decoded = decoded[..28]; weights = weights[..128]; }
        else { decoded = decoded[..14]; weights = weights[..64]; }
        cached = cached[..4];
        var b0 = MemoryMarshal.Read<Vector512<double>>(MemoryMarshal.AsBytes(decoded[..2]));
        var b1 = MemoryMarshal.Read<Vector512<double>>(MemoryMarshal.AsBytes(decoded.Slice(2, 2)));
        var b2 = MemoryMarshal.Read<Vector512<double>>(MemoryMarshal.AsBytes(decoded.Slice(4, 2)));
        var b3 = MemoryMarshal.Read<Vector512<double>>(MemoryMarshal.AsBytes(decoded.Slice(6, 2)));
        var b4 = wide ? MemoryMarshal.Read<Vector512<double>>(MemoryMarshal.AsBytes(decoded.Slice(8, 2))) : default;
        var b5 = wide ? MemoryMarshal.Read<Vector512<double>>(MemoryMarshal.AsBytes(decoded.Slice(10, 2))) : default;
        var b6 = wide ? MemoryMarshal.Read<Vector512<double>>(MemoryMarshal.AsBytes(decoded.Slice(12, 2))) : default;
        var first = Vector512<double>.Zero; var second = Vector512<double>.Zero;
        var replicate01 = Vector512.Create(0L, 0, 0, 0, 1, 1, 1, 1);
        var replicate23 = Vector512.Create(2L, 2, 2, 2, 3, 3, 3, 3);
        for (var tap = 0; wide ? tap < 16 : tap < 8; tap += 2)
        {
            var at = wide ? ((tap & 12) << 3) + 16 + ((tap & 2) << 2) : (tap << 3) + 8;
            var even = MemoryMarshal.Read<Vector256<double>>(MemoryMarshal.AsBytes(weights.Slice(at, 4)));
            var odd = MemoryMarshal.Read<Vector256<double>>(MemoryMarshal.AsBytes(weights.Slice(at + 4, 4)));
            var evenWide = Vector512.Create(even, even); var oddWide = Vector512.Create(odd, odd);
            var even01 = Avx512F.PermuteVar8x64(evenWide, replicate01);
            var even23 = Avx512F.PermuteVar8x64(evenWide, replicate23);
            var odd01 = Avx512F.PermuteVar8x64(oddWide, replicate01);
            var odd23 = Avx512F.PermuteVar8x64(oddWide, replicate23);
            first = Avx512F.Add(first, Avx512F.Multiply(Avx512F.Shuffle4x128(b0, wide ? b2 : b1, 0x44), even01));
            second = Avx512F.Add(second, Avx512F.Multiply(Avx512F.Shuffle4x128(wide ? b4 : b2, wide ? b6 : b3, 0x44), even23));
            first = Avx512F.Add(first, Avx512F.Multiply(Avx512F.Shuffle4x128(b0, wide ? b2 : b1, 0xee), odd01));
            second = Avx512F.Add(second, Avx512F.Multiply(Avx512F.Shuffle4x128(wide ? b4 : b2, wide ? b6 : b3, 0xee), odd23));
            b0 = b1; b1 = b2; b2 = b3;
            if (wide)
            {
                b3 = b4; b4 = b5; b5 = b6;
                if (tap + 2 < 16)
                    b6 = MemoryMarshal.Read<Vector512<double>>(MemoryMarshal.AsBytes(decoded.Slice(tap + 14, 2)));
            }
            else if (tap + 2 < 8)
                b3 = MemoryMarshal.Read<Vector512<double>>(MemoryMarshal.AsBytes(decoded.Slice(tap + 8, 2)));
        }
        MemoryMarshal.Write(MemoryMarshal.AsBytes(cached[..2]), in first);
        context.Cancellation.ThrowIfCancellationRequested();
        MemoryMarshal.Write(MemoryMarshal.AsBytes(cached.Slice(2, 2)), in second);
    }

    // Geometry proves shared windows once. Other prepared layouts retain
    // independent windows; irregular layouts keep indexed taps.
    private static void PrepareWideHorizontalPairs(Span<int> offsets,ReadOnlySpan<int> counts,
        int width,int sourceWidth,int capacity,OperationContext context)
    {
        for(var x=0;x+1<width;x+=2)
        {
            context.Cancellation.ThrowIfCancellationRequested();
            var n=counts[x];
            if(n<=0 || n>=capacity || n!=counts[x+1] || n>sourceWidth)continue;
            var base0=x*capacity;var base1=base0+capacity;
            var first0=offsets[base0];var first1=offsets[base1];
            if((uint)first0>(uint)(sourceWidth-n) || (uint)first1>(uint)(sourceWidth-n))continue;
            var contiguous=true;
            for(var tap=0;tap<n;tap++)
                contiguous &= offsets[base0+tap]==first0+tap && offsets[base1+tap]==first1+tap;
            // Both horizontal max-count reads and original-result repair stop
            // before this padding slot. Full/unequal pairs and odd tails keep
            // their original offsets. The proof is shared by every cached row.
            // Valid clamped pixel indexes are nonnegative, so the marker cannot
            // be confused with a live last tap in a full-capacity pair.
            if(contiguous)offsets[base0+capacity-1]=n==16 && first1==first0+4
                && sourceWidth>=20 && first0<=sourceWidth-20?OverlappingWidePair:-1;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void FilterWideHorizontal(ReadOnlySpan<double> decoded,Span<double> cached,
        ReadOnlySpan<double> weights,ReadOnlySpan<int> offsets,ReadOnlySpan<int> counts,
        int width,int capacity,OperationContext context)
    {
        // Wide plans store clamped pixel indexes. Form checked pixel views
        // once here; Original restores channel offsets at its own boundary.
        var decodedPixels=MemoryMarshal.Cast<double,Vector256<double>>(decoded);
        var cachedPixels=MemoryMarshal.Cast<double,Vector256<double>>(cached);
        var x=0;
        for(;x+1<width;x+=2)
        {
            if (Avx512F.IsSupported && capacity == 32 && x + 3 < width && offsets[x * 32 + 30] == PreparedHorizontalQuartet)
            {
                FilterHorizontalQuartet<WideHorizontalQuartet>(decodedPixels.Slice(offsets[x * 32], 28),
                    weights.Slice(x * 32, 128), cachedPixels.Slice(x, 4), context);
                x += 2;
                continue;
            }
            context.Cancellation.ThrowIfCancellationRequested();
            var sum0=Vector256<double>.Zero;
            var sum1=Vector256<double>.Zero;
            var n=Math.Max(counts[x],counts[x+1]);
            // Fix the column views once. Both views include the
            // zero-initialized padding up to the larger tap count,
            // preserving the accumulation order for unequal counts.
            var weights0=weights.Slice(x*capacity,n);
            var weights1=weights.Slice((x+1)*capacity,n);
            var offsets0=offsets.Slice(x*capacity,n);
            var offsets1=offsets.Slice((x+1)*capacity,n);
            if(Avx512F.IsSupported && offsets[x*capacity+capacity-1]==OverlappingWidePair)
            {
                var pair=BicubicMath.FilterOverlappingHorizontalPair(
                    decodedPixels.Slice(offsets0[0],20),weights0[..16],weights1[..16],16,4);
                sum0=pair.GetLower();sum1=pair.GetUpper();
            }
            else if(offsets[x*capacity+capacity-1]<0)
            {
                var pixels0=decodedPixels.Slice(offsets0[0],n);
                var pixels1=decodedPixels.Slice(offsets1[0],n);
                for(var tx=0;tx<n;tx++)
                {
                    sum0=Avx.Add(sum0,Avx.Multiply(pixels0[tx],Vector256.Create(weights0[tx])));
                    sum1=Avx.Add(sum1,Avx.Multiply(pixels1[tx],Vector256.Create(weights1[tx])));
                }
            }
            else
            {
                for(var tx=0;tx<n;tx++)
                {
                    var p0=decodedPixels[offsets0[tx]];
                    var p1=decodedPixels[offsets1[tx]];
                    sum0=Avx.Add(sum0,Avx.Multiply(p0,Vector256.Create(weights0[tx])));
                    sum1=Avx.Add(sum1,Avx.Multiply(p1,Vector256.Create(weights1[tx])));
                }
            }
            cachedPixels[x]=sum0;
            cachedPixels[x+1]=sum1;
        }
        if(x<width)
        {
            var sum=Vector256<double>.Zero;
            for(var tx=0;tx<counts[x];tx++)
                sum=Avx.Add(sum,Avx.Multiply(decodedPixels[offsets[x*capacity+tx]],Vector256.Create(weights[x*capacity+tx])));
            cachedPixels[x]=sum;
        }
    }

    private readonly record struct WideErrorBounds(double Maximum,double AlphaMargin,double WordColorBound,
        double ByteColorMargin,double UnitError)
    {
        internal static WideErrorBounds Create(bool word,int capacity)=>new(word?65535d:255d,
            word?capacity>=128?16e-3:capacity==64?4e-3:1e-3:capacity>=128?4e-4:capacity==64?1e-4:1e-5,
            capacity>=128?128.16:capacity==64?32.04:8.01,
            capacity>=128?16e-3:capacity==64?4e-3:1e-3,
            1d/(1L<<40)*capacity*capacity);
    }

    private readonly struct ByteSamples { }
    private readonly struct ByteSuffixSamples { }
    private readonly struct WordSamples { }

    // Keep quartet accumulators and the small ordinary-byte certificate out
    // of the general publisher's frame. False certificates write no pixels.
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static int WriteWideByteQuartetPrefix(ImageBuffer result,int y,ReadOnlySpan<double> cached,
        Span<int> rows,ReadOnlySpan<double> vertical,ReadOnlySpan<double> normalizers,double normalizerY,
        OperationContext context)
    {
        var width=result.Width;var count=vertical.Length;
        var destination=result.Pixels.Slice(y*result.Stride,result.Stride);
        // The caller's ordinary domain has capacity32 and private stack rows.
        for(var t=0;t<count;t++)rows[t]>>=2;
        var pixels=MemoryMarshal.Cast<double,Vector256<double>>(cached);
        var x=0;
        for(;x+3<width;x+=4)
        {
            context.Cancellation.ThrowIfCancellationRequested();
            BicubicMath.FilterVerticalQuartet(pixels,vertical,rows,x,out var first,out var second);
            if(!BicubicMath.TryWriteOrdinaryBytePair(first,normalizers[x]*normalizerY,
                normalizers[x+1]*normalizerY,destination.Slice(x*4,8)))break;
            context.Cancellation.ThrowIfCancellationRequested();
            if(!BicubicMath.TryWriteOrdinaryBytePair(second,normalizers[x+2]*normalizerY,
                normalizers[x+3]*normalizerY,destination.Slice((x+2)*4,8)))
            {
                x+=2;break;
            }
        }
        // The unchanged general suffix consumes channel offsets again.
        for(var t=0;t<count;t++)rows[t]<<=2;
        return x;
    }

    private static void WriteWideRow<TSamples>(ImageBuffer source,ImageBuffer result,int y,ReadOnlySpan<double> cached,
        ReadOnlySpan<int> rows,ReadOnlySpan<double> vertical,ReadOnlySpan<double> normalizers,double normalizerY,
        ReadOnlySpan<double> weights,ReadOnlySpan<int> offsets,ReadOnlySpan<int> counts,int first,int capacity,
        WideErrorBounds bounds,bool ordinaryCertificate,OperationContext context,int firstX=0) where TSamples : struct
    {
        var width=result.Width;var count=vertical.Length;var wide16=typeof(TSamples)==typeof(WordSamples);
        var maximum=bounds.Maximum;var alphaMargin=bounds.AlphaMargin;var wordColorBound=bounds.WordColorBound;
        var byteColorMargin=bounds.ByteColorMargin;var unitError=bounds.UnitError;
        var destination=result.Pixels.Slice(y*result.Stride,result.Stride);
        for(var x=typeof(TSamples)==typeof(ByteSuffixSamples)?firstX:0;x+1<width;x+=2)
        {
            context.Cancellation.ThrowIfCancellationRequested();
            var sum=Vector512<double>.Zero;
            for(var t=0;t<count;t++)
                sum=Avx512F.Add(sum,Avx512F.Multiply(MemoryMarshal.Read<Vector512<double>>(MemoryMarshal.AsBytes(cached.Slice(rows[t]+x*4,8))),Vector512.Create(vertical[t])));
            var n0=normalizers[x]*normalizerY;var n1=normalizers[x+1]*normalizerY;
            // Public caller tap bounds <=20 cover the shared 400-term proof.
            // Keep the explicit type guard so word import never sees byte locals.
            if((typeof(TSamples)==typeof(ByteSamples) || typeof(TSamples)==typeof(ByteSuffixSamples)) && ordinaryCertificate
                && BicubicMath.TryWriteOrdinaryBytePair(sum,n0,n1,destination.Slice(x*4,8)))continue;
            var a0=sum.GetElement(3);var a1=sum.GetElement(7);
            var values=Avx512F.Divide(sum,Vector512.Create(Vector256.Create(a0,a0,a0,n0),Vector256.Create(a1,a1,a1,n1)));
            // At most capacity taps per axis, each bounded by one. The
            // 2^-40*capacity^2 absolute-error bound per unit sample covers
            // both accumulation orders. With normalization >= .5 and
            // alpha >= 1, 32-tap byte alpha/color margins are 1e-5/1e-3;
            // 64-tap byte margins are 1e-4/4e-3; 128-tap byte margins
            // are 4e-4/16e-3. Word alpha uses
            // 1e-3/4e-3 for 32/64 taps; the color quotient error is below
            // 8.01/alphaSum or 32.04/alphaSum. A reciprocal of the lower power of two
            // conservatively bounds it. Margins >= .25 retain original
            // evaluation, also protecting clamped out-of-range samples.
            var fractional=Avx512F.Subtract(values,Avx512F.RoundScale(values,1));
            var delta=Avx512F.Subtract(fractional,Vector512.Create(.5));
            var distance=Avx512F.AndNot(Vector512.Create(long.MinValue),delta.AsInt64()).AsDouble();
            var exponent0=(int)((BitConverter.DoubleToUInt64Bits(a0)>>52)&2047);
            var exponent1=(int)((BitConverter.DoubleToUInt64Bits(a1)>>52)&2047);
            var colorMargin0=wide16?wordColorBound*BitConverter.UInt64BitsToDouble((ulong)(2046-exponent0)<<52):byteColorMargin;
            var colorMargin1=wide16?wordColorBound*BitConverter.UInt64BitsToDouble((ulong)(2046-exponent1)<<52):byteColorMargin;
            var alphaMargin0=alphaMargin;var alphaMargin1=alphaMargin;
            if(capacity>=128 || !wide16 && capacity==64)
            {
                // Apply the same absolute accumulation-error bound to
                // each actual quotient, including 64-tap bytes. This avoids
                // direct two-dimensional replay when the fixed byte margin
                // is much wider than the certified quotient error.
                (colorMargin0,alphaMargin0)=QuotientMargins(values.GetLower(),a0,n0,maximum,unitError);
                (colorMargin1,alphaMargin1)=QuotientMargins(values.GetUpper(),a1,n1,maximum,unitError);
            }
            var margins=Vector512.Create(colorMargin0,colorMargin0,colorMargin0,alphaMargin0,
                colorMargin1,colorMargin1,colorMargin1,alphaMargin1);
            var inRange=Avx512F.And(Avx512F.CompareGreaterThanOrEqual(values,Vector512<double>.Zero).AsInt64(),
                Avx512F.CompareLessThanOrEqual(values,Vector512.Create(maximum)).AsInt64());
            var ambiguous=Avx512F.And(inRange,Avx512F.CompareLessThanOrEqual(distance,margins).AsInt64());
            var ambiguousMask=Vector512.ExtractMostSignificantBits(ambiguous);
            // A failed certificate needs original-order repair only
            // for that pixel; its certified neighbor keeps the SIMD result.
            var original0=colorMargin0>=.25 || n0<.5 || !(values.GetElement(3)>=1+alphaMargin) || (ambiguousMask&15)!=0;
            var original1=colorMargin1>=.25 || n1<.5 || !(values.GetElement(7)>=1+alphaMargin) || (ambiguousMask&240)!=0;
            if(original0 || original1)
            {
                if(original0)Original(source,result,x,y,weights,offsets,counts,vertical[..count],first,capacity,context);
                else if(wide16)WriteWord(destination,x,values.GetLower());
                else WriteByte(destination,x,values.GetLower());
                if(original1)Original(source,result,x+1,y,weights,offsets,counts,vertical[..count],first,capacity,context);
                else if(wide16)WriteWord(destination,x+1,values.GetUpper());
                else WriteByte(destination,x+1,values.GetUpper());
                continue;
            }
            var clipped=Avx512F.Min(Vector512.Create(maximum),Avx512F.Max(Vector512<double>.Zero,values));
            var ints=Avx512F.ConvertToVector256Int32WithTruncation(Avx512F.RoundScale(clipped,0));
            if(wide16)
            {
                var packed=Sse41.PackUnsignedSaturate(ints.GetLower(),ints.GetUpper());
                MemoryMarshal.Write(destination.Slice(x*8,16),in packed);
            }
            else
            {
                var shorts=Sse2.PackSignedSaturate(ints.GetLower(),ints.GetUpper());
                var packed=Sse2.PackUnsignedSaturate(shorts,Vector128<short>.Zero).AsUInt64().GetElement(0);
                BinaryPrimitives.WriteUInt64LittleEndian(destination.Slice(x*4,8),packed);
            }
        }
        if((width&1)!=0)Original(source,result,width-1,y,weights,offsets,counts,vertical[..count],first,capacity,context);
    }
    private static (double Color,double Alpha) QuotientMargins(Vector256<double> values,double alphaSum,double normalizer,double maximum,double unitError)
    {
        var alphaError=unitError*maximum;
        var colorError=alphaError*maximum;
        if(!(alphaSum>alphaError && normalizer>unitError))return (.25,.25);
        var colorMagnitude=Math.Max(Math.Abs(values.GetElement(0)),
            Math.Max(Math.Abs(values.GetElement(1)),Math.Abs(values.GetElement(2))));
        // For C/A, |delta(C/A)| <= (EC + |C/A| EA)/(A-EA).
        // For A/N, use (EA + |A/N| EN)/(N-EN). Doubling also
        // covers the rounding of this positive bound calculation.
        return (2*(colorError+colorMagnitude*alphaError)/(alphaSum-alphaError),
            2*(alphaError+Math.Abs(values.GetElement(3))*unitError)/(normalizer-unitError));
    }

    private static void WriteByte(Span<byte> destination,int x,Vector256<double> values)
    {
        var clipped=Avx.Min(Vector256.Create(255d),Avx.Max(Vector256<double>.Zero,values));
        var ints=Avx.ConvertToVector128Int32WithTruncation(Avx.RoundToNearestInteger(clipped));
        var shorts=Sse2.PackSignedSaturate(ints,Vector128<int>.Zero);
        var packed=Sse2.PackUnsignedSaturate(shorts,Vector128<short>.Zero).AsUInt32().GetElement(0);
        BinaryPrimitives.WriteUInt32LittleEndian(destination.Slice(x*4,4),packed);
    }
    private static void WriteWord(Span<byte> destination,int x,Vector256<double> values)
    {
        var clipped=Avx.Min(Vector256.Create(65535d),Avx.Max(Vector256<double>.Zero,values));
        var ints=Avx.ConvertToVector128Int32WithTruncation(Avx.RoundToNearestInteger(clipped));
        var packed=Sse41.PackUnsignedSaturate(ints,Vector128<int>.Zero).AsUInt64().GetElement(0);
        BinaryPrimitives.WriteUInt64LittleEndian(destination.Slice(x*8,8),packed);
    }
}
