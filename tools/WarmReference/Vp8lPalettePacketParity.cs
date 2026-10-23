// Source-only palette packet composition. Palette/index syntax: RFC 9649.
// Production admission and general palette ordering remain unqualified.
#nullable enable
using System;
using System.Linq;
using System.Buffers.Binary;
public sealed record Vp8lPalettePacketResult(byte[] Packet,object Details);
public sealed record Vp8lPaletteComparisonResult(Vp8lPalettePacketResult Selected,Vp8lPalettePacketResult Plain,Vp8lPalettePacketResult Spatial);
public static class Vp8lPalettePacketParity
{
    public static Vp8lPalettePacketResult Encode(byte[] source,int width,int height,int method=4)
    {
        if(method==5)return CompareCandidates(source,width,height).Selected;
        return EncodeCandidate(source,width,height,method,false);
    }
    public static Vp8lPaletteComparisonResult CompareCandidates(byte[] source,int width,int height)
    {
        var plain=EncodeCandidate(source,width,height,5,false);
        var spatial=EncodeCandidate(source,width,height,5,true);
        return new(Payload(spatial.Packet)<Payload(plain.Packet)?spatial:plain,plain,spatial);
    }
    private static uint Payload(byte[] packet)=>BinaryPrimitives.ReadUInt32LittleEndian(packet.AsSpan(16));
    private static Vp8lPalettePacketResult EncodeCandidate(byte[] source,int width,int height,int method,bool spatial)
    {
        if(width<1||width>16384||height<1||height>16384||source.Length!=checked(width*height*4))throw new ArgumentException("Expected RGBA8 dimensions.");
        var choice=Vp8lInitialChoice.Select(source,width,height,method);
        if(choice.Mode!=4)throw new NotSupportedException("This stage composes palette coding only.");
        var palette=Vp8lPaletteParity.Build(source,width,height);
        var entries=palette.Colors.Length;
        if(entries>17&&palette.Colors[^1]==0)entries--; // RFC color-index padding supplies the final zero.
        var bits=new Vp8lParityBits();
        bits.Put(0x2f,8);bits.Put(width-1,14);bits.Put(height-1,14);
        bits.Put(Enumerable.Range(0,width*height).Any(i=>source[4*i+3]!=255)?1:0,1);bits.Put(0,3);
        bits.Put(1,1);bits.Put(3,2);bits.Put(entries-1,8);
        var auxiliary=Vp8lAuxiliaryParity.Encode(palette.Deltas.AsSpan(0,entries).ToArray(),entries,20,method==0);
        for(var bit=0;bit<auxiliary.BitCount;bit++)bits.Put(auxiliary.Bytes[bit>>3]>>(bit&7)&1,1);
        var coded=new byte[palette.Packed.Length*4];
        for(var i=0;i<palette.Packed.Length;i++)BinaryPrimitives.WriteUInt32LittleEndian(coded.AsSpan(i*4),palette.Packed[i]);
        int? predictorBits=null;
        if(spatial)
        {
            // Predict opaque index carriers after packing; palette alpha remains
            // in the preceding transform. There is no subtract-green/cross-color.
            var rgba=new byte[coded.Length];
            for(var i=0;i<palette.Packed.Length;i++){var pixel=palette.Packed[i];rgba[4*i]=(byte)(pixel>>16);rgba[4*i+1]=(byte)(pixel>>8);rgba[4*i+2]=(byte)pixel;rgba[4*i+3]=(byte)(pixel>>24);}
            int Tiles(int size)=>((palette.CodedWidth+(1<<size)-1)>>size)*((height+(1<<size)-1)>>size);
            int Clamp(int size)
            {
                size=Math.Clamp(size,2,9);while(size<9&&Tiles(size)>16384)size++;
                while(size>2&&Tiles(size-1)==1)size--;return size;
            }
            var maximum=Clamp(choice.TransformBits);var minimum=Clamp(maximum-2);
            var prediction=Vp8lAdaptivePredictorParity.Select(rgba,palette.CodedWidth,height,minimum,maximum,false);
            predictorBits=prediction.StoredBits;
            var columns=(palette.CodedWidth+(1<<prediction.StoredBits)-1)>>prediction.StoredBits;
            var map=Vp8lAuxiliaryParity.Encode(prediction.Modes.Select(m=>0xff000000u|(uint)m<<8).ToArray(),columns,75);
            bits.Put(1,1);bits.Put(0,2);bits.Put(prediction.StoredBits-2,3);
            for(var bit=0;bit<map.BitCount;bit++)bits.Put(map.Bytes[bit>>3]>>(bit&7)&1,1);
            coded=prediction.Residual;
        }
        var stage=new Vp8lInputParity(bits.Finish(),bits.Count,coded,new{palette=palette.Colors.Length,spatial,predictorBits});
        var maximumCacheBits=System.Numerics.BitOperations.Log2((uint)palette.Colors.Length)+1;
        var cached=Vp8lDirectPacketParity.Compose(stage,palette.CodedWidth,height,choice.HistogramBits,choice,method==0,false,maximumCacheBits);
        var frame=cached;int? uncachedBytes=null;
        if(method==5)
        {
            var uncached=Vp8lDirectPacketParity.Compose(stage,palette.CodedWidth,height,choice.HistogramBits,choice,false,true,maximumCacheBits);
            uncachedBytes=uncached.Packet.Length;
            if(Payload(uncached.Packet)<Payload(cached.Packet))frame=uncached;
        }
        return new(frame.Packet,new{colors=palette.Colors.Length,packingBits=palette.PackingBits,codedWidth=palette.CodedWidth,spatial,predictorBits,cachedBytes=cached.Packet.Length,uncachedBytes,composition=frame.Details});
    }
}
