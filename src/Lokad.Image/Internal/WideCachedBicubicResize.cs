using System.Buffers.Binary;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace Lokad.Image.Internal;

/// <summary>RGBA8/16 downsampling with bounded operation-local row reuse.</summary>
internal static class WideCachedBicubicResize
{
    internal static bool TryApply(ImageBuffer source, ImageBuffer result, double scaleX, double scaleY,
        long tapsX, long tapsY, long rowWork, OperationContext context)
    {
        var capacity=tapsX>32 || tapsY>32?64:32;
        if(source.BitDepth is not (8 or 16) || result.Width>=source.Width || result.Height>=source.Height
            || result.Width<2 || tapsX>capacity || tapsY>capacity
            || !Avx512F.IsSupported || !Avx512DQ.IsSupported || !Avx2.IsSupported || !Sse41.IsSupported)
            return false;
        var width=result.Width;
        var wide16=source.BitDepth==16;
        var maximum=wide16?65535d:255d;
        var alphaMargin=wide16?capacity==64?4e-3:1e-3:capacity==64?1e-4:1e-5;
        var wordColorBound=capacity==64?32.04:8.01;
        var byteColorMargin=capacity==64?4e-3:1e-3;
        var cacheWidth=(long)width;
        var elements=cacheWidth*(capacity*4)+(long)width*(capacity+1)+(long)source.Width*4;
        var indexElements=(long)width*(capacity+1);
        var bytes=elements*sizeof(double)+indexElements*sizeof(int)+capacity*8;
        if (elements>Array.MaxLength || indexElements>Array.MaxLength || bytes>context.RemainingWorkingBytes-ResizeAlpha.ScratchBytes)
            return false;
        context.Reserve(bytes);
        try
        {
            var storage=new double[(int)elements];
            var geometry=new int[(int)indexElements];
            var cacheElements=(int)(cacheWidth*(capacity*4));
            var weights=storage.AsSpan(cacheElements,width*capacity);
            var normalizers=storage.AsSpan(cacheElements+width*capacity,width);
            var decoded=storage.AsSpan(cacheElements+width*(capacity+1),source.Width*4);
            var offsets=geometry.AsSpan(0,width*capacity);
            var counts=geometry.AsSpan(width*capacity,width);
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
                    weights[x*capacity+t]=w;normalizers[x]+=w;
                    offsets[x*capacity+t]=Math.Clamp(first+t,0,source.Width-1)*4;
                }
            }
            Span<int> rowIds=stackalloc int[capacity];rowIds.Fill(-1);
            Span<int> rows=stackalloc int[capacity];
            Span<double> vertical=stackalloc double[capacity];
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
                    var slot=sy&(capacity-1);rows[t]=(int)(slot*cacheWidth*4);
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
                        var n=Math.Max(counts[x],counts[x+1]);
                        for(var tx=0;tx<n;tx++)
                        {
                            var p0=MemoryMarshal.Read<Vector256<double>>(MemoryMarshal.AsBytes(decoded.Slice(offsets[x*capacity+tx],4)));
                            var p1=MemoryMarshal.Read<Vector256<double>>(MemoryMarshal.AsBytes(decoded.Slice(offsets[(x+1)*capacity+tx],4)));
                            sum0=Avx.Add(sum0,Avx.Multiply(p0,Vector256.Create(weights[x*capacity+tx])));
                            sum1=Avx.Add(sum1,Avx.Multiply(p1,Vector256.Create(weights[(x+1)*capacity+tx])));
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
                var destination=result.Pixels.Slice(y*result.Stride,result.Stride);
                for(var x=0;x+1<width;x+=2)
                {
                    context.Cancellation.ThrowIfCancellationRequested();
                    var sum=Vector512<double>.Zero;
                    for(var t=0;t<count;t++)
                        sum=Avx512F.Add(sum,Avx512F.Multiply(MemoryMarshal.Read<Vector512<double>>(MemoryMarshal.AsBytes(storage.AsSpan(rows[t]+x*4,8))),Vector512.Create(vertical[t])));
                    var n0=normalizers[x]*normalizerY;var n1=normalizers[x+1]*normalizerY;
                    var a0=sum.GetElement(3);var a1=sum.GetElement(7);
                    var values=Avx512F.Divide(sum,Vector512.Create(Vector256.Create(a0,a0,a0,n0),Vector256.Create(a1,a1,a1,n1)));
                    // At most capacity taps per axis, each bounded by one. The
                    // 2^-40*capacity^2 absolute-error bound per unit sample covers
                    // both accumulation orders. With normalization >= .5 and
                    // alpha >= 1, 32-tap byte alpha/color margins are 1e-5/1e-3;
                    // 64-tap byte margins are 1e-4/4e-3. Word alpha uses
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
                    var margins=Vector512.Create(colorMargin0,colorMargin0,colorMargin0,alphaMargin,
                        colorMargin1,colorMargin1,colorMargin1,alphaMargin);
                    var inRange=Avx512F.And(Avx512F.CompareGreaterThanOrEqual(values,Vector512<double>.Zero).AsInt64(),
                        Avx512F.CompareLessThanOrEqual(values,Vector512.Create(maximum)).AsInt64());
                    var ambiguous=Avx512F.And(inRange,Avx512F.CompareLessThanOrEqual(distance,margins).AsInt64());
                    var ambiguousMask=Vector512.ExtractMostSignificantBits(ambiguous);
                    if(capacity==64)
                    {
                        // Preserve all original guards independently: uncertainty
                        // in one pixel does not require reevaluating its neighbor.
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
                    }
                    if(colorMargin0>=.25 || colorMargin1>=.25 || n0<.5 || n1<.5 || !(values.GetElement(3)>=1+alphaMargin && values.GetElement(7)>=1+alphaMargin)
                        || ambiguousMask!=0)
                    {
                        Original(source,result,x,y,weights,offsets,counts,vertical[..count],first,capacity,context);
                        Original(source,result,x+1,y,weights,offsets,counts,vertical[..count],first,capacity,context);
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
            return true;
        }
        finally {context.Release(bytes);}
    }
    private static void Original(ImageBuffer source,ImageBuffer result,int x,int y,ReadOnlySpan<double> weights,
        ReadOnlySpan<int> offsets,ReadOnlySpan<int> counts,ReadOnlySpan<double> vertical,int firstY,int capacity,OperationContext context)
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
                var w=wy*weights[x*capacity+tx];if(w==0)continue;var offset=offsets[x*capacity+tx];
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
    private static int Round(double value,int maximum)=>(int)Math.Clamp(Math.Round(value,MidpointRounding.AwayFromZero),0,maximum);
}
