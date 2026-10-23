// Source-only non-palette effort primitives. RFC 9649 defines transform syntax.
// Adaptive method5 sampling is qualified on named controls only; production is open.
#nullable enable
using System;
using System.Linq;
using System.Buffers.Binary;

public sealed record Vp8lCacheComparisonResult(Vp8lPacketResult Selected,int CachedBytes,int UncachedBytes);
public sealed record Vp8lAdaptivePacketResult(byte[] Packet,int CachedBytes,int UncachedBytes,Vp8lAdaptivePredictorResult Predictor,object Details);
public static class Vp8lEffortParity
{
    public static Vp8lDirectPacketResult EncodeFixedSampling(byte[] source,int width,int height,int method)
    {
        if(method!=0&&method!=1)throw new NotSupportedException("This stage covers methods0/1, without adaptive sampling.");
        if(width<1||height<1||width>16384||height>16384||source.Length!=checked(width*height*4))throw new ArgumentException("Expected RGBA8 dimensions.");
        var choice=Vp8lInitialChoice.Select(source,width,height,method);
        if(choice.Mode!=3)throw new NotSupportedException("This stage covers the spatial/subtract-green transform family.");
        var low=method==0;var bits=choice.TransformBits;var columns=(width+(1<<bits)-1)>>bits;
        var modes=low
            ?Enumerable.Repeat((byte)11,columns*((height+(1<<bits)-1)>>bits)).ToArray()
            :Vp8lPredictorParity.EncodeModes(source,width,height,bits);
        var residual=Vp8lPredictorParity.Residual(source,width,height,bits,modes);
        var predictor=Vp8lAuxiliaryParity.Encode(modes.Select(m=>0xff000000u|(uint)m<<8).ToArray(),columns,75,low);
        var writer=new Vp8lParityBits();writer.Put(0x2f,8);writer.Put(width-1,14);writer.Put(height-1,14);
        writer.Put(Enumerable.Range(0,width*height).Any(i=>source[4*i+3]!=255)?1:0,1);writer.Put(0,3);
        writer.Put(1,1);writer.Put(2,2);writer.Put(1,1);writer.Put(0,2);writer.Put(bits-2,3);Append(predictor);
        if(!low&&!choice.RedBlueZero)
        {
            var color=Vp8lCrossColorParity.Select(residual,width,height,bits,75);
            var pixels=new uint[color.Map.Length/4];
            for(var i=0;i<pixels.Length;i++)pixels[i]=BinaryPrimitives.ReadUInt32LittleEndian(color.Map.AsSpan(i*4));
            var coefficients=Vp8lAuxiliaryParity.Encode(pixels,columns,75);
            writer.Put(1,1);writer.Put(1,2);writer.Put(bits-2,3);Append(coefficients);residual=color.Coded;
        }
        var stage=new Vp8lInputParity(writer.Finish(),writer.Count,residual,new{method});
        return Vp8lDirectPacketParity.Compose(stage,width,height,choice.HistogramBits,choice,low);
        void Append(Vp8lAuxiliaryResult image){for(var bit=0;bit<image.BitCount;bit++)writer.Put(image.Bytes[bit>>3]>>(bit&7)&1,1);}
    }

    public static Vp8lAdaptivePacketResult EncodeAdaptiveSampling(byte[] source,int width,int height)
    {
        var choice=Vp8lInitialChoice.Select(source,width,height,5);
        if(choice.Mode!=3)throw new NotSupportedException("This stage covers the spatial/subtract-green transform family at method5.");
        var maximum=choice.TransformBits;
        int Tiles(int bits)=>((width+(1<<bits)-1)>>bits)*((height+(1<<bits)-1)>>bits);
        while(maximum<9&&Tiles(maximum)>16384)maximum++;
        while(maximum>2&&Tiles(maximum-1)==1)maximum--;
        var minimum=Math.Max(2,maximum-2);
        while(minimum<9&&Tiles(minimum)>16384)minimum++;
        while(minimum>2&&Tiles(minimum-1)==1)minimum--;
        var prediction=Vp8lAdaptivePredictorParity.Select(source,width,height,minimum,maximum);
        var predictorColumns=(width+(1<<prediction.StoredBits)-1)>>prediction.StoredBits;
        var predictor=Vp8lAuxiliaryParity.Encode(prediction.Modes.Select(m=>0xff000000u|(uint)m<<8).ToArray(),predictorColumns,75);
        var writer=new Vp8lParityBits();writer.Put(0x2f,8);writer.Put(width-1,14);writer.Put(height-1,14);
        writer.Put(Enumerable.Range(0,width*height).Any(i=>source[4*i+3]!=255)?1:0,1);writer.Put(0,3);
        writer.Put(1,1);writer.Put(2,2);writer.Put(1,1);writer.Put(0,2);writer.Put(prediction.StoredBits-2,3);Append(predictor);
        var residual=prediction.Residual;
        if(!choice.RedBlueZero)
        {
            var color=Vp8lCrossColorParity.Select(residual,width,height,choice.TransformBits,75);
            var pixels=new uint[color.Map.Length/4];
            for(var i=0;i<pixels.Length;i++)pixels[i]=BinaryPrimitives.ReadUInt32LittleEndian(color.Map.AsSpan(i*4));
            var columns=(width+(1<<choice.TransformBits)-1)>>choice.TransformBits;
            var coefficients=Vp8lAuxiliaryParity.Encode(pixels,columns,75);
            writer.Put(1,1);writer.Put(1,2);writer.Put(choice.TransformBits-2,3);Append(coefficients);residual=color.Coded;
        }
        var stage=new Vp8lInputParity(writer.Finish(),writer.Count,residual,new{method=5,prediction.SelectedBits,prediction.StoredBits});
        var cached=Vp8lDirectPacketParity.Compose(stage,width,height,choice.HistogramBits,choice);
        var uncached=Vp8lDirectPacketParity.Compose(stage,width,height,choice.HistogramBits,choice,false,true);
        var first=BinaryPrimitives.ReadUInt32LittleEndian(cached.Packet.AsSpan(16));
        var second=BinaryPrimitives.ReadUInt32LittleEndian(uncached.Packet.AsSpan(16));
        var selected=second<first?uncached:cached;
        return new(selected.Packet,cached.Packet.Length,uncached.Packet.Length,prediction,selected.Details);
        void Append(Vp8lAuxiliaryResult image){for(var bit=0;bit<image.BitCount;bit++)writer.Put(image.Bytes[bit>>3]>>(bit&7)&1,1);}
    }

    // This is the complete writer comparison primitive, not a substitute for
    // adaptive predictor analysis. The declared control's method5 prefix equals4.
    public static Vp8lCacheComparisonResult CompareCacheCandidates(byte[] source)
    {
        var cached=Vp8lPacketParity.Build(source,false);
        var uncached=Vp8lPacketParity.Build(source,true);
        var first=BinaryPrimitives.ReadUInt32LittleEndian(cached.Packet.AsSpan(16));
        var second=BinaryPrimitives.ReadUInt32LittleEndian(uncached.Packet.AsSpan(16));
        return new(second<first?uncached:cached,cached.Packet.Length,uncached.Packet.Length);
    }
}
