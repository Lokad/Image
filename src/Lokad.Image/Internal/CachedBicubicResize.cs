using System.Buffers.Binary;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace Lokad.Image.Internal;

// One geometry/workspace/row schedule for both cached support ranges. Row
// certificates retain their original precision and replay policies.
internal static partial class CachedBicubicResize
{
    internal static bool TryApply(ImageBuffer source, ImageBuffer result, double scaleX, double scaleY,
        long tapsX, long tapsY, OperationContext context)
    {
        var support=Math.Max(tapsX,tapsY);
        if(source.BitDepth is not (8 or 16) || support>2048)return false;
        var wide=support>16;
        var capacity=!wide?16:support>128?(int)BitOperations.RoundUpToPowerOf2((uint)support):support>64?128:support>32?64:32;
        var heapMetadata=capacity>128;
        var upsample=result.Width>source.Width && result.Height>source.Height;
        var mixed=result.Width>source.Width && result.Height<source.Height
            || result.Width<source.Width && result.Height>source.Height;
        var horizontalDown=result.Width<source.Width && result.Height==source.Height;
        var verticalDown=result.Width==source.Width && result.Height<source.Height;
        var horizontalUp=source.BitDepth==8 && result.Width>source.Width && result.Height==source.Height;
        var verticalUp=result.Width==source.Width && result.Height>source.Height;
        if ((wide?result.Width>=source.Width || result.Height>=source.Height
                :!upsample && !mixed && !horizontalDown && !verticalDown && !horizontalUp && !verticalUp && (result.Width>=source.Width || result.Height>=source.Height))
            || result.Width<2 || tapsX>capacity || tapsY>capacity
            || !Avx512F.IsSupported || !Avx512DQ.IsSupported || !Avx2.IsSupported || !Sse41.IsSupported)
            return false;
        var width=result.Width;var wide16=source.BitDepth==16;
        var identityHorizontal=verticalDown || verticalUp;
        var bounds=wide?WideErrorBounds.Create(wide16,capacity):default;
        var cacheWidth=(long)width+(mixed?(width&1):0);
        var decodedElements=identityHorizontal?0:source.Width*4;
        var dataElements=cacheWidth*(capacity*4)+(long)width*(capacity+1)+decodedElements;
        var geometryElements=(long)width*(capacity+1);
        var elements=dataElements+(heapMetadata?capacity:0);
        var indexElements=geometryElements+(heapMetadata?2*capacity:0);
        var bytes=elements*sizeof(double)+indexElements*sizeof(int)+(heapMetadata?0:capacity*8);
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
            var decodedWorkspace=storage.AsSpan(cacheElements+width*(capacity+1),decodedElements);
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
                    // Wide rows consume pixel vectors directly. Narrow rows
                    // retain channel offsets and their prepared overlap marker.
                    offsets[x*capacity+t]=Math.Clamp(first+t,0,source.Width-1)*(wide?1:4);
                }
            }
            if(!wide && !identityHorizontal)PrepareOverlappingHorizontalPairs(offsets,counts,width,source.Width,context);
            if(wide)PrepareWideHorizontalPairs(offsets,counts,width,source.Width,capacity,context);
            Span<int> rowIds=heapMetadata?geometry.AsSpan((int)geometryElements,capacity):stackalloc int[capacity];rowIds.Fill(-1);
            Span<int> rows=heapMetadata?geometry.AsSpan((int)geometryElements+capacity,capacity):stackalloc int[capacity];
            Span<double> vertical=heapMetadata?storage.AsSpan((int)dataElements,capacity):stackalloc double[capacity];
            var input=source.Pixels;
            var cancellation=context.Cancellation;
            var pollConversion=identityHorizontal && cancellation.CanBeCanceled;
            for(var y=0;y<result.Height;y++)
            {
                context.Cancellation.ThrowIfCancellationRequested();
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
                    // An unchanged horizontal axis preserves these exact products.
                    // Convert directly into its cache slot, without a copied row.
                    var decoded=identityHorizontal?storage.AsSpan(rows[t],width*4):decodedWorkspace;
                    if(pollConversion)DecodePremultipliedRow<PollRow>(row,decoded,source.Width,wide16,cancellation);
                    else DecodePremultipliedRow<NoRowPolling>(row,decoded,source.Width,wide16,default);
                    if(wide)FilterWideHorizontal(decoded,storage.AsSpan(rows[t],width*4),weights,offsets,counts,width,capacity,context);
                    else if(!identityHorizontal)FilterHorizontal(decoded,storage.AsSpan(rows[t],width*4),weights,offsets,counts,width,capacity,
                        mixed || horizontalUp,context);
                }
                if(wide)
                {
                    WriteWideRow(source,result,y,storage.AsSpan(0,cacheElements),rows[..count],vertical[..count],
                        normalizers,normalizerY,weights,offsets,counts,first,capacity,bounds,context);
                    continue;
                }
                var destination=result.Pixels.Slice(y*result.Stride,result.Stride);
                if(mixed)
                {
                    var vectors=MemoryMarshal.Cast<double,Vector512<double>>(storage.AsSpan(0,cacheElements));
                    if(wide16)WriteMixedWordRow(source,result,y,vectors,rows[..count],vertical[..count],
                        normalizers,normalizerY,weights,offsets,counts,first,context);
                    else WriteMixedByteRow(source,result,y,vectors,rows[..count],vertical[..count],
                        normalizers,normalizerY,weights,offsets,counts,first,context);
                    if((width&1)!=0)Original(source,result,width-1,y,weights,offsets,counts,vertical[..count],first,16,context);
                    continue;
                }
                if(!wide16)
                {
                    WriteByteRow(source,result,y,storage,rows[..count],vertical[..count],
                        normalizers,normalizerY,weights,offsets,counts,first,context);
                    if((width&1)!=0)Original(source,result,width-1,y,weights,offsets,counts,vertical[..count],first,16,context);
                    continue;
                }
                for(var x=0;x+1<width;x+=2)
                {
                    context.Cancellation.ThrowIfCancellationRequested();
                    var sum=Vector512<double>.Zero;
                    for(var t=0;t<count;t++)
                        sum=Avx512F.Add(sum,Avx512F.Multiply(MemoryMarshal.Read<Vector512<double>>(MemoryMarshal.AsBytes(storage.AsSpan(rows[t]+x*4,8))),Vector512.Create(vertical[t])));
                    var n0=normalizers[x]*normalizerY;var n1=normalizers[x+1]*normalizerY;
                    if(!BicubicMath.TryWriteWordPair(sum,n0,n1,destination.Slice(x*8,16)))
                    {
                        Original(source,result,x,y,weights,offsets,counts,vertical[..count],first,16,context);
                        Original(source,result,x+1,y,weights,offsets,counts,vertical[..count],first,16,context);
                    }
                }
                if((width&1)!=0)Original(source,result,width-1,y,weights,offsets,counts,vertical[..count],first,16,context);
            }
            return true;
        }
        finally {context.Release(bytes);}
    }
    private static void WriteByteRow(ImageBuffer source,ImageBuffer result,int y,
        ReadOnlySpan<double> cached,ReadOnlySpan<int> rows,ReadOnlySpan<double> vertical,
        ReadOnlySpan<double> normalizers,double normalizerY,ReadOnlySpan<double> weights,
        ReadOnlySpan<int> offsets,ReadOnlySpan<int> counts,int firstY,OperationContext context)
    {
        var destination=result.GetRow8(y);
        // Drop exact zero end weights once per row; retain the full spans for
        // original-order replay and keep all nonzero taps in their stored order.
        var firstTap=0;var lastTap=vertical.Length;
        while(firstTap<lastTap && vertical[firstTap]==0)firstTap++;
        while(lastTap>firstTap && vertical[lastTap-1]==0)lastTap--;
        for(var x=0;x+1<result.Width;x+=2)
        {
            context.Cancellation.ThrowIfCancellationRequested();
            var sum=Vector512<double>.Zero;
            for(var t=firstTap;t<lastTap;t++)
                sum=Avx512F.Add(sum,Avx512F.Multiply(MemoryMarshal.Read<Vector512<double>>(MemoryMarshal.AsBytes(cached.Slice(rows[t]+x*4,8))),Vector512.Create(vertical[t])));
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
                Original(source,result,x,y,weights,offsets,counts,vertical,firstY,16,context);
                Original(source,result,x+1,y,weights,offsets,counts,vertical,firstY,16,context);
                continue;
            }
            var clipped=Avx512F.Min(Vector512.Create(255d),Avx512F.Max(Vector512<double>.Zero,values));
            var ints=Avx512F.ConvertToVector256Int32WithTruncation(Avx512F.RoundScale(clipped,0));
            var shorts=Sse2.PackSignedSaturate(ints.GetLower(),ints.GetUpper());
            var packed=Sse2.PackUnsignedSaturate(shorts,Vector128<short>.Zero).AsUInt64().GetElement(0);
            BinaryPrimitives.WriteUInt64LittleEndian(destination.Slice(x*4,8),packed);
        }
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
                Original(source,result,x,y,weights,offsets,counts,vertical,firstY,16,context);
                Original(source,result,x+1,y,weights,offsets,counts,vertical,firstY,16,context);
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
            if(!BicubicMath.PrepareWordPair(sum,n0,n1,out var values,out var margins))
            {
                Original(source,result,x,y,weights,offsets,counts,vertical,firstY,16,context);
                Original(source,result,x+1,y,weights,offsets,counts,vertical,firstY,16,context);
                continue;
            }
            var fractional=Avx512F.Subtract(values,Avx512F.RoundScale(values,1));
            var delta=Avx512F.Subtract(fractional,Vector512.Create(.5));
            var distance=Avx512F.AndNot(Vector512.Create(long.MinValue),delta.AsInt64()).AsDouble();
            var inRange=Avx512F.And(Avx512F.CompareGreaterThanOrEqual(values,Vector512<double>.Zero).AsInt64(),
                Avx512F.CompareLessThanOrEqual(values,Vector512.Create(65535d)).AsInt64());
            var ambiguous=Avx512F.And(inRange,Avx512F.CompareLessThanOrEqual(distance,margins).AsInt64());
            if(!(values.GetElement(3)>=1+1e-4 && values.GetElement(7)>=1+1e-4)
                || Vector512.ExtractMostSignificantBits(ambiguous)!=0)
            {
                Original(source,result,x,y,weights,offsets,counts,vertical,firstY,16,context);
                Original(source,result,x+1,y,weights,offsets,counts,vertical,firstY,16,context);
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
                // Restore a channel offset only at the original-order boundary.
                if(capacity>16)offset*=4;
                var alpha=wide16?row16[offset+3]:row8[offset+3];
                var red=wide16?row16[offset]:row8[offset];var green=wide16?row16[offset+1]:row8[offset+1];var blue=wide16?row16[offset+2]:row8[offset+2];
                r+=w*alpha*red;g+=w*alpha*green;b+=w*alpha*blue;a+=w*alpha;n+=w;
            }
        }
        var normalized=a/n;
        var rounded=normalized>0 && normalized<1?ResizeAlpha.Round(source,x,y,result.Width,result.Height,ResizeFilter.Bicubic,context):BicubicMath.Round(normalized,maximum);
        var color=rounded>0 && a>0;
        if(wide16)
        {
            var destination=result.GetRow16(y);
            destination[x*4]=(ushort)BicubicMath.Round(color?r/a:0,maximum);destination[x*4+1]=(ushort)BicubicMath.Round(color?g/a:0,maximum);
            destination[x*4+2]=(ushort)BicubicMath.Round(color?b/a:0,maximum);destination[x*4+3]=(ushort)rounded;
        }
        else
        {
            var destination=result.GetRow8(y);
            destination[x*4]=(byte)BicubicMath.Round(color?r/a:0,maximum);destination[x*4+1]=(byte)BicubicMath.Round(color?g/a:0,maximum);
            destination[x*4+2]=(byte)BicubicMath.Round(color?b/a:0,maximum);destination[x*4+3]=(byte)rounded;
        }
    }
}
