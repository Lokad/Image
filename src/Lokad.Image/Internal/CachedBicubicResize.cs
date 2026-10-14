using System.Buffers.Binary;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace Lokad.Image.Internal;

/// <summary>Separable RGBA8/16 cubic filtering with bounded, operation-local row reuse.</summary>
internal static class CachedBicubicResize
{
    internal static bool TryApply(ImageBuffer source, ImageBuffer result, double scaleX, double scaleY,
        long tapsX, long tapsY, long rowWork, OperationContext context)
    {
        var upsample=result.Width>source.Width && result.Height>source.Height;
        var mixed=result.Width>source.Width && result.Height<source.Height
            || result.Width<source.Width && result.Height>source.Height;
        if (source.BitDepth is not (8 or 16) || !upsample && !mixed && (result.Width>=source.Width || result.Height>=source.Height)
            || result.Width < 2 || tapsX > 16 || tapsY > 16 || !Avx512F.IsSupported || !Avx512DQ.IsSupported || !Avx2.IsSupported || !Sse41.IsSupported)
            return false;
        var width=result.Width;
        var wide16=source.BitDepth==16;
        var sampleBytes=wide16?2:1;
        var maximum=wide16?65535d:255d;
        var alphaMargin=wide16?1e-4:1e-6;
        var cacheWidth=(long)width+(mixed?(width&1):0);
        var elements=cacheWidth*64+(long)width*17+(long)source.Width*4;
        var indexElements=(long)width*17;
        var bytes=elements*sizeof(double)+indexElements*sizeof(int)+128;
        if (elements>Array.MaxLength || indexElements>Array.MaxLength || bytes>context.RemainingWorkingBytes-ResizeAlpha.ScratchBytes)
            return false;
        context.Reserve(bytes);
        try
        {
            var storage=new double[(int)elements];
            var geometry=new int[(int)indexElements];
            var cacheElements=(int)(cacheWidth*64);
            var weights=storage.AsSpan(cacheElements,width*16);
            var normalizers=storage.AsSpan(cacheElements+width*16,width);
            var decoded=storage.AsSpan(cacheElements+width*17,source.Width*4);
            var offsets=geometry.AsSpan(0,width*16);
            var counts=geometry.AsSpan(width*16,width);
            for(var x=0;x<width;x++)
            {
                context.Cancellation.ThrowIfCancellationRequested();
                var center=(x+0.5)*source.Width/width-0.5;
                var first=(int)Math.Ceiling(center-2*scaleX);
                var count=(int)Math.Floor(center+2*scaleX)-first+1;
                counts[x]=count;
                for(var t=0;t<count;t++)
                {
                    var w=ImageTransforms.Kernel((first+t-center)/scaleX,ResizeFilter.Bicubic);
                    weights[x*16+t]=w;normalizers[x]+=w;
                    offsets[x*16+t]=Math.Clamp(first+t,0,source.Width-1)*4;
                }
            }
            Span<int> rowIds=stackalloc int[16];rowIds.Fill(-1);
            Span<int> rows=stackalloc int[16];
            Span<double> vertical=stackalloc double[16];
            var input=source.Pixels;
            for(var y=0;y<result.Height;y++)
            {
                context.Work(rowWork);
                var center=(y+0.5)*source.Height/result.Height-0.5;
                var first=(int)Math.Ceiling(center-2*scaleY);
                var count=(int)Math.Floor(center+2*scaleY)-first+1;
                double normalizerY=0;
                for(var t=0;t<count;t++)
                {
                    var w=ImageTransforms.Kernel((first+t-center)/scaleY,ResizeFilter.Bicubic);
                    vertical[t]=w;normalizerY+=w;
                    var sy=Math.Clamp(first+t,0,source.Height-1);
                    var slot=sy&15;rows[t]=(int)(slot*cacheWidth*4);
                    if(rowIds[slot]==sy)continue;
                    rowIds[slot]=sy;
                    var row=input.Slice(sy*source.Stride,source.Stride);
                    var sx=0;
                    for(;sx+1<source.Width;sx+=2)
                    {
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
                    for(;sx<source.Width;sx++)
                    {
                        var pixel=wide16?BinaryPrimitives.ReadUInt64LittleEndian(row.Slice(sx*8,8)):BinaryPrimitives.ReadUInt32LittleEndian(row.Slice(sx*4,4));
                        var channels=wide16?Sse41.ConvertToVector128Int32(Vector128.CreateScalar(pixel).AsUInt16())
                            :Sse41.ConvertToVector128Int32(Vector128.CreateScalar((uint)pixel).AsByte());
                        var a=(int)(wide16?pixel>>48:pixel>>24);
                        var premult=Sse41.MultiplyLow(channels,Vector128.Create(a,a,a,1));
                        var values=wide16?Avx512F.ConvertToVector512Double(Vector256.Create(premult.AsUInt32(),Vector128<uint>.Zero)).GetLower()
                            :Avx.ConvertToVector256Double(premult);
                        MemoryMarshal.Write(MemoryMarshal.AsBytes(decoded.Slice(sx*4,4)),in values);
                    }
                    var cached=storage.AsSpan(rows[t],width*4);
                    var x=0;
                    for(;x+1<width;x+=2)
                    {
                        context.Cancellation.ThrowIfCancellationRequested();
                        var sum0=Vector256<double>.Zero;
                        var sum1=Vector256<double>.Zero;
                        var base0=x*16;var base1=(x+1)*16;
                        var first0=offsets[base0];var first1=offsets[base1];
                        if(mixed && counts[x]==4 && counts[x+1]==4
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
                            for(var tx=0;tx<n;tx++)
                            {
                                var p0=MemoryMarshal.Read<Vector256<double>>(MemoryMarshal.AsBytes(decoded.Slice(offsets[base0+tx],4)));
                                var p1=MemoryMarshal.Read<Vector256<double>>(MemoryMarshal.AsBytes(decoded.Slice(offsets[base1+tx],4)));
                                var w0=Vector256.Create(weights[base0+tx]);
                                var w1=Vector256.Create(weights[base1+tx]);
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
                            sum=Avx.Add(sum,Avx.Multiply(MemoryMarshal.Read<Vector256<double>>(MemoryMarshal.AsBytes(decoded.Slice(offsets[x*16+tx],4))),Vector256.Create(weights[x*16+tx])));
                        MemoryMarshal.Write(MemoryMarshal.AsBytes(cached.Slice(x*4,4)),in sum);
                    }
                }
                var destination=result.Pixels.Slice(y*result.Stride,result.Stride);
                if(mixed)
                {
                    var vectors=MemoryMarshal.Cast<double,Vector512<double>>(storage.AsSpan(0,cacheElements));
                    if(wide16)WriteMixedWordRow(source,result,y,vectors,rows[..count],vertical[..count],
                        normalizers,normalizerY,weights,offsets,counts,first,context);
                    else WriteMixedByteRow(source,result,y,vectors,rows[..count],vertical[..count],
                        normalizers,normalizerY,weights,offsets,counts,first,context);
                    if((width&1)!=0)Original(source,result,width-1,y,weights,offsets,counts,vertical[..count],first,context);
                    continue;
                }
                for(var x=0;x+1<width;x+=2)
                {
                    context.Cancellation.ThrowIfCancellationRequested();
                    var sum=Vector512<double>.Zero;
                    for(var t=0;t<count;t++)
                        sum=Avx512F.Add(sum,Avx512F.Multiply(MemoryMarshal.Read<Vector512<double>>(MemoryMarshal.AsBytes(storage.AsSpan(rows[t]+x*4,8))),Vector512.Create(vertical[t])));
                    var n0=normalizers[x]*normalizerY;var n1=normalizers[x+1]*normalizerY;
                    var a0=sum.GetElement(3);var a1=sum.GetElement(7);
                    var values=Avx512F.Divide(sum,Vector512.Create(Vector256.Create(a0,a0,a0,n0),Vector256.Create(a1,a1,a1,n1)));
                    // At most 16 taps per axis, each bounded by one in magnitude.
                    // 2^-40*256 bounds both accumulation orders' absolute error
                    // per unit sample, including products and normalization sums.
                    // With normalization >= .5 and rounded alpha >= 1, byte
                    // margins 1e-6 (alpha) and 1e-4 (color) cover thresholds.
                    // Word alpha uses 1e-4; color error is below 2.0001/alphaSum
                    // at every threshold in [0,65535]. A reciprocal of the lower
                    // power of two conservatively bounds that quotient.
                    // Tiny alpha and ambiguous quantization retain the original
                    // binary64 order and exact tiny-alpha arithmetic.
                    // Positive alpha and finite, bounded integer samples make
                    // all color ratios finite. Compare the eight quantization
                    // distances together, retaining the same scalar margins.
                    var fractional=Avx512F.Subtract(values,Avx512F.RoundScale(values,1));
                    var delta=Avx512F.Subtract(fractional,Vector512.Create(.5));
                    var distance=Avx512F.AndNot(Vector512.Create(long.MinValue),delta.AsInt64()).AsDouble();
                    var colorMargin0=wide16?2.0001*ReciprocalLowerPower(a0):1e-4;
                    var colorMargin1=wide16?2.0001*ReciprocalLowerPower(a1):1e-4;
                    var margins=Vector512.Create(colorMargin0,colorMargin0,colorMargin0,alphaMargin,
                        colorMargin1,colorMargin1,colorMargin1,alphaMargin);
                    var inRange=Avx512F.And(Avx512F.CompareGreaterThanOrEqual(values,Vector512<double>.Zero).AsInt64(),
                        Avx512F.CompareLessThanOrEqual(values,Vector512.Create(maximum)).AsInt64());
                    var ambiguous=Avx512F.And(inRange,Avx512F.CompareLessThanOrEqual(distance,margins).AsInt64());
                    if(n0<.5 || n1<.5 || !(values.GetElement(3)>=1+alphaMargin && values.GetElement(7)>=1+alphaMargin)
                        || Vector512.ExtractMostSignificantBits(ambiguous)!=0)
                    {
                        Original(source,result,x,y,weights,offsets,counts,vertical[..count],first,context);
                        Original(source,result,x+1,y,weights,offsets,counts,vertical[..count],first,context);
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
                if((width&1)!=0)Original(source,result,width-1,y,weights,offsets,counts,vertical[..count],first,context);
            }
            return true;
        }
        finally {context.Release(bytes);}
    }
    private static void WriteMixedByteRow(ImageBuffer source,ImageBuffer result,int y,
        ReadOnlySpan<Vector512<double>> cached,ReadOnlySpan<int> rows,ReadOnlySpan<double> vertical,
        ReadOnlySpan<double> normalizers,double normalizerY,ReadOnlySpan<double> weights,
        ReadOnlySpan<int> offsets,ReadOnlySpan<int> counts,int firstY,OperationContext context)
    {
        var destination=result.GetRow8(y);
        for(var x=0;x+1<result.Width;x+=2)
        {
            context.Cancellation.ThrowIfCancellationRequested();
            var sum=Vector512<double>.Zero;
            for(var t=0;t<vertical.Length;t++)
                sum=Avx512F.Add(sum,Avx512F.Multiply(cached[(rows[t]>>3)+(x>>1)],Vector512.Create(vertical[t])));
            var n0=normalizers[x]*normalizerY;var n1=normalizers[x+1]*normalizerY;
            var a0=sum.GetElement(3);var a1=sum.GetElement(7);
            var values=Avx512F.Divide(sum,Vector512.Create(Vector256.Create(a0,a0,a0,n0),Vector256.Create(a1,a1,a1,n1)));
            var fractional=Avx512F.Subtract(values,Avx512F.RoundScale(values,1));
            var delta=Avx512F.Subtract(fractional,Vector512.Create(.5));
            var distance=Avx512F.AndNot(Vector512.Create(long.MinValue),delta.AsInt64()).AsDouble();
            var margins=Vector512.Create(1e-4,1e-4,1e-4,1e-6,1e-4,1e-4,1e-4,1e-6);
            var inRange=Avx512F.And(Avx512F.CompareGreaterThanOrEqual(values,Vector512<double>.Zero).AsInt64(),
                Avx512F.CompareLessThanOrEqual(values,Vector512.Create(255d)).AsInt64());
            var ambiguous=Avx512F.And(inRange,Avx512F.CompareLessThanOrEqual(distance,margins).AsInt64());
            if(n0<.5 || n1<.5 || !(values.GetElement(3)>=1+1e-6 && values.GetElement(7)>=1+1e-6)
                || Vector512.ExtractMostSignificantBits(ambiguous)!=0)
            {
                Original(source,result,x,y,weights,offsets,counts,vertical,firstY,context);
                Original(source,result,x+1,y,weights,offsets,counts,vertical,firstY,context);
                continue;
            }
            var clipped=Avx512F.Min(Vector512.Create(255d),Avx512F.Max(Vector512<double>.Zero,values));
            var ints=Avx512F.ConvertToVector256Int32WithTruncation(Avx512F.RoundScale(clipped,0));
            var shorts=Sse2.PackSignedSaturate(ints.GetLower(),ints.GetUpper());
            var packed=Sse2.PackUnsignedSaturate(shorts,Vector128<short>.Zero).AsUInt64().GetElement(0);
            BinaryPrimitives.WriteUInt64LittleEndian(destination.Slice(x*4,8),packed);
        }
    }
    private static void WriteMixedWordRow(ImageBuffer source,ImageBuffer result,int y,
        ReadOnlySpan<Vector512<double>> cached,ReadOnlySpan<int> rows,ReadOnlySpan<double> vertical,
        ReadOnlySpan<double> normalizers,double normalizerY,ReadOnlySpan<double> weights,
        ReadOnlySpan<int> offsets,ReadOnlySpan<int> counts,int firstY,OperationContext context)
    {
        var destination=result.Pixels.Slice(y*result.Stride,result.Stride);
        for(var x=0;x+1<result.Width;x+=2)
        {
            context.Cancellation.ThrowIfCancellationRequested();
            var sum=Vector512<double>.Zero;
            for(var t=0;t<vertical.Length;t++)
                sum=Avx512F.Add(sum,Avx512F.Multiply(cached[(rows[t]>>3)+(x>>1)],Vector512.Create(vertical[t])));
            var n0=normalizers[x]*normalizerY;var n1=normalizers[x+1]*normalizerY;
            var a0=sum.GetElement(3);var a1=sum.GetElement(7);
            var values=Avx512F.Divide(sum,Vector512.Create(Vector256.Create(a0,a0,a0,n0),Vector256.Create(a1,a1,a1,n1)));
            var fractional=Avx512F.Subtract(values,Avx512F.RoundScale(values,1));
            var delta=Avx512F.Subtract(fractional,Vector512.Create(.5));
            var distance=Avx512F.AndNot(Vector512.Create(long.MinValue),delta.AsInt64()).AsDouble();
            // Keep the same power-of-two bounds without calls that spill
            // live vectors in this per-pair precision check.
            var exponent0=(int)((BitConverter.DoubleToUInt64Bits(a0)>>52)&2047);
            var exponent1=(int)((BitConverter.DoubleToUInt64Bits(a1)>>52)&2047);
            var margin0=2.0001*BitConverter.UInt64BitsToDouble((ulong)(2046-exponent0)<<52);
            var margin1=2.0001*BitConverter.UInt64BitsToDouble((ulong)(2046-exponent1)<<52);
            var margins=Vector512.Create(margin0,margin0,margin0,1e-4,margin1,margin1,margin1,1e-4);
            var inRange=Avx512F.And(Avx512F.CompareGreaterThanOrEqual(values,Vector512<double>.Zero).AsInt64(),
                Avx512F.CompareLessThanOrEqual(values,Vector512.Create(65535d)).AsInt64());
            var ambiguous=Avx512F.And(inRange,Avx512F.CompareLessThanOrEqual(distance,margins).AsInt64());
            if(n0<.5 || n1<.5 || !(values.GetElement(3)>=1+1e-4 && values.GetElement(7)>=1+1e-4)
                || Vector512.ExtractMostSignificantBits(ambiguous)!=0)
            {
                Original(source,result,x,y,weights,offsets,counts,vertical,firstY,context);
                Original(source,result,x+1,y,weights,offsets,counts,vertical,firstY,context);
                continue;
            }
            var clipped=Avx512F.Min(Vector512.Create(65535d),Avx512F.Max(Vector512<double>.Zero,values));
            var ints=Avx512F.ConvertToVector256Int32WithTruncation(Avx512F.RoundScale(clipped,0));
            var packed=Sse41.PackUnsignedSaturate(ints.GetLower(),ints.GetUpper());
            MemoryMarshal.Write(destination.Slice(x*8,16),in packed);
        }
    }
    private static double ReciprocalLowerPower(double value)
    {
        var exponent=(int)((BitConverter.DoubleToUInt64Bits(value)>>52)&2047);
        return BitConverter.UInt64BitsToDouble((ulong)(2046-exponent)<<52);
    }
    private static void Original(ImageBuffer source,ImageBuffer result,int x,int y,ReadOnlySpan<double> weights,
        ReadOnlySpan<int> offsets,ReadOnlySpan<int> counts,ReadOnlySpan<double> vertical,int firstY,OperationContext context)
    {
        double r=0,g=0,b=0,a=0,n=0;var wide16=source.BitDepth==16;var maximum=wide16?65535:255;
        for(var ty=0;ty<vertical.Length;ty++)
        {
            context.Cancellation.ThrowIfCancellationRequested();var wy=vertical[ty];if(wy==0)continue;
            var sy=Math.Clamp(firstY+ty,0,source.Height-1);
            ReadOnlySpan<byte> row8=wide16?default:source.GetRow8(sy);
            ReadOnlySpan<ushort> row16=wide16?source.GetRow16(sy):default;
            for(var tx=0;tx<counts[x];tx++)
            {
                var w=wy*weights[x*16+tx];if(w==0)continue;var offset=offsets[x*16+tx];
                var alpha=wide16?row16[offset+3]:row8[offset+3];
                var red=wide16?row16[offset]:row8[offset];var green=wide16?row16[offset+1]:row8[offset+1];var blue=wide16?row16[offset+2]:row8[offset+2];
                r+=w*alpha*red;g+=w*alpha*green;b+=w*alpha*blue;a+=w*alpha;n+=w;
            }
        }
        var normalized=a/n;
        var rounded=normalized>0 && normalized<1?ResizeAlpha.Round(source,x,y,result.Width,result.Height,ResizeFilter.Bicubic,context):Round(normalized,maximum);
        var color=rounded>0 && a>0;
        if(wide16)
        {
            var destination=result.GetRow16(y);
            destination[x*4]=(ushort)Round(color?r/a:0,maximum);destination[x*4+1]=(ushort)Round(color?g/a:0,maximum);
            destination[x*4+2]=(ushort)Round(color?b/a:0,maximum);destination[x*4+3]=(ushort)rounded;
        }
        else
        {
            var destination=result.GetRow8(y);
            destination[x*4]=(byte)Round(color?r/a:0,maximum);destination[x*4+1]=(byte)Round(color?g/a:0,maximum);
            destination[x*4+2]=(byte)Round(color?b/a:0,maximum);destination[x*4+3]=(byte)rounded;
        }
    }
    private static int Round(double value,int maximum)=>(int)Math.Clamp(Math.Round(value,MidpointRounding.AwayFromZero),0,maximum);
}
