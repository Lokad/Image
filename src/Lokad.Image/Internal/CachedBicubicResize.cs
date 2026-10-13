using System.Buffers.Binary;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace Lokad.Image.Internal;

/// <summary>Separable RGBA8 cubic filtering with bounded, operation-local row reuse.</summary>
internal static class CachedBicubicResize
{
    internal static bool TryApply(ImageBuffer source, ImageBuffer result, double scaleX, double scaleY,
        long tapsX, long tapsY, long rowWork, OperationContext context)
    {
        if (source.BitDepth != 8 || result.Width >= source.Width || result.Height >= source.Height
            || result.Width < 2 || tapsX > 16 || tapsY > 16 || !Avx512F.IsSupported || !Avx512DQ.IsSupported || !Avx2.IsSupported || !Sse41.IsSupported)
            return false;
        var width=result.Width;
        var elements=(long)width*81+(long)source.Width*4;
        var indexElements=(long)width*17;
        var bytes=elements*sizeof(double)+indexElements*sizeof(int)+128;
        if (elements>Array.MaxLength || indexElements>Array.MaxLength || bytes>context.RemainingWorkingBytes-ResizeAlpha.ScratchBytes)
            return false;
        context.Reserve(bytes);
        try
        {
            var storage=new double[(int)elements];
            var geometry=new int[(int)indexElements];
            var weights=storage.AsSpan(width*64,width*16);
            var normalizers=storage.AsSpan(width*80,width);
            var decoded=storage.AsSpan(width*81,source.Width*4);
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
                    var slot=sy&15;rows[t]=slot*width*4;
                    if(rowIds[slot]==sy)continue;
                    rowIds[slot]=sy;
                    var row=input.Slice(sy*source.Stride,source.Stride);
                    var sx=0;
                    for(;sx+1<source.Width;sx+=2)
                    {
                        var pixels=Vector128.CreateScalar(BinaryPrimitives.ReadUInt64LittleEndian(row.Slice(sx*4,8))).AsByte();
                        var channels=Avx2.ConvertToVector256Int32(pixels);
                        var factors=Avx2.Blend(Avx2.Shuffle(channels,255),Vector256.Create(1),136);
                        var values=Avx512F.ConvertToVector512Double(Avx2.MultiplyLow(channels,factors));
                        MemoryMarshal.Write(MemoryMarshal.AsBytes(decoded.Slice(sx*4,8)),in values);
                    }
                    for(;sx<source.Width;sx++)
                    {
                        var pixel=BinaryPrimitives.ReadUInt32LittleEndian(row.Slice(sx*4,4));
                        var channels=Sse41.ConvertToVector128Int32(Vector128.CreateScalar(pixel).AsByte());
                        var a=(int)(pixel>>24);
                        var premult=Sse41.MultiplyLow(channels,Vector128.Create(a,a,a,1));
                        var values=Avx.ConvertToVector256Double(premult);
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
                            var p0=MemoryMarshal.Read<Vector256<double>>(MemoryMarshal.AsBytes(decoded.Slice(offsets[x*16+tx],4)));
                            var p1=MemoryMarshal.Read<Vector256<double>>(MemoryMarshal.AsBytes(decoded.Slice(offsets[(x+1)*16+tx],4)));
                            var w0=Vector256.Create(weights[x*16+tx]);
                            var w1=Vector256.Create(weights[(x+1)*16+tx]);
                            // Keep independent accumulators: constructing a wide
                            // vector from variable halves causes stack traffic
                            // on current JITs inside this repeated support loop.
                            sum0=Avx.Add(sum0,Avx.Multiply(p0,w0));
                            sum1=Avx.Add(sum1,Avx.Multiply(p1,w1));
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
                var destination=result.GetRow8(y);
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
                    // With normalization >= .5 and rounded alpha >= 1, margins
                    // 1e-6 (alpha) and 1e-4 (color) cover every byte threshold.
                    // Tiny alpha and ambiguous quantization retain the original
                    // binary64 order and exact tiny-alpha arithmetic.
                    // Positive alpha and finite, bounded integer samples make
                    // all color ratios finite. Compare the eight quantization
                    // distances together, retaining the same scalar margins.
                    var fractional=Avx512F.Subtract(values,Avx512F.RoundScale(values,1));
                    var delta=Avx512F.Subtract(fractional,Vector512.Create(.5));
                    var distance=Avx512F.AndNot(Vector512.Create(long.MinValue),delta.AsInt64()).AsDouble();
                    var margins=Vector512.Create(1e-4,1e-4,1e-4,1e-6,1e-4,1e-4,1e-4,1e-6);
                    var inRange=Avx512F.And(Avx512F.CompareGreaterThanOrEqual(values,Vector512<double>.Zero).AsInt64(),
                        Avx512F.CompareLessThanOrEqual(values,Vector512.Create(255d)).AsInt64());
                    var ambiguous=Avx512F.And(inRange,Avx512F.CompareLessThanOrEqual(distance,margins).AsInt64());
                    if(n0<.5 || n1<.5 || !(values.GetElement(3)>=1.000001 && values.GetElement(7)>=1.000001)
                        || Vector512.ExtractMostSignificantBits(ambiguous)!=0)
                    {
                        Original(source,result,x,y,weights,offsets,counts,vertical[..count],first,context);
                        Original(source,result,x+1,y,weights,offsets,counts,vertical[..count],first,context);
                        continue;
                    }
                    var clipped=Avx512F.Min(Vector512.Create(255d),Avx512F.Max(Vector512<double>.Zero,values));
                    var ints=Avx512F.ConvertToVector256Int32WithTruncation(Avx512F.RoundScale(clipped,0));
                    var shorts=Sse2.PackSignedSaturate(ints.GetLower(),ints.GetUpper());
                    var packed=Sse2.PackUnsignedSaturate(shorts,Vector128<short>.Zero).AsUInt64().GetElement(0);
                    BinaryPrimitives.WriteUInt64LittleEndian(destination.Slice(x*4,8),packed);
                }
                if((width&1)!=0)Original(source,result,width-1,y,weights,offsets,counts,vertical[..count],first,context);
            }
            return true;
        }
        finally {context.Release(bytes);}
    }
    private static void Original(ImageBuffer source,ImageBuffer result,int x,int y,ReadOnlySpan<double> weights,
        ReadOnlySpan<int> offsets,ReadOnlySpan<int> counts,ReadOnlySpan<double> vertical,int firstY,OperationContext context)
    {
        double r=0,g=0,b=0,a=0,n=0;
        for(var ty=0;ty<vertical.Length;ty++)
        {
            context.Cancellation.ThrowIfCancellationRequested();var wy=vertical[ty];if(wy==0)continue;
            var row=source.GetRow8(Math.Clamp(firstY+ty,0,source.Height-1));
            for(var tx=0;tx<counts[x];tx++)
            {
                var w=wy*weights[x*16+tx];if(w==0)continue;var offset=offsets[x*16+tx];var alpha=row[offset+3];
                r+=w*alpha*row[offset];g+=w*alpha*row[offset+1];b+=w*alpha*row[offset+2];a+=w*alpha;n+=w;
            }
        }
        var normalized=a/n;
        var rounded=normalized>0 && normalized<1?ResizeAlpha.Round(source,x,y,result.Width,result.Height,ResizeFilter.Bicubic,context):Round(normalized);
        var color=rounded>0 && a>0;var destination=result.GetRow8(y);
        destination[x*4]=(byte)Round(color?r/a:0);destination[x*4+1]=(byte)Round(color?g/a:0);
        destination[x*4+2]=(byte)Round(color?b/a:0);destination[x*4+3]=(byte)rounded;
    }
    private static int Round(double value)=>(int)Math.Clamp(Math.Round(value,MidpointRounding.AwayFromZero),0,255);
}
