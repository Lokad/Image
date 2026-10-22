// Source-only direct/no-transform packet model. RFC 9649 defines syntax.
// Current qualification is the existing 129x129 input at method4/quality75.
#nullable enable
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Buffers.Binary;
public sealed record Vp8lDirectPacketResult(byte[] Packet,object Details);
public static class Vp8lDirectPacketParity
{
    private static MethodInfo Auxiliary(string type,string name)=>typeof(Vp8lAuxiliaryParity).Assembly.GetType(type)!.GetMethod(name,BindingFlags.Static|BindingFlags.NonPublic)!;
    public static Vp8lDirectPacketResult Encode(byte[] source,int width,int height)
    {
        if(width<1||width>16384||height<1||height>16384||source.Length!=checked(width*height*4))throw new ArgumentException("Expected RGBA8 dimensions.");
        var choice=Vp8lInitialChoice.Select(source,width,height);
        if(choice.Mode!=0)throw new NotSupportedException("This stage composes direct coding only.");
        var pixels=new uint[width*height];var alpha=false;
        for(var i=0;i<pixels.Length;i++){var at=4*i;pixels[i]=(uint)source[at+3]<<24|(uint)source[at]<<16|(uint)source[at+1]<<8|source[at+2];alpha|=source[at+3]!=255;}
        var header=new byte[5];var value=0x2fUL|((ulong)(width-1)<<8)|((ulong)(height-1)<<22)|(alpha?1UL<<36:0);
        for(var i=0;i<header.Length;i++)header[i]=(byte)(value>>(8*i));
        var residual=new byte[pixels.Length*4];for(var i=0;i<pixels.Length;i++)BinaryPrimitives.WriteUInt32LittleEndian(residual.AsSpan(i*4),pixels[i]);
        return Compose(new(header,40,residual,new{mode=0}),width,height,choice.HistogramBits,choice);
    }
    internal static Vp8lDirectPacketResult Compose(Vp8lInputParity stage,int width,int height,int tileBits,object choice)
    {
        var pixels=new uint[width*height];
        for(var i=0;i<pixels.Length;i++)pixels[i]=BinaryPrimitives.ReadUInt32LittleEndian(stage.CodedResidual.AsSpan(i*4));
        var index=Auxiliary("Vp8lMatchParity","Build").CreateDelegate<Func<uint[],int,int,uint[]>>();
        var prefix=Auxiliary("Vp8lLzParity","Prefix").CreateDelegate<Func<int,(int Code,int Extra)>>();
        var plane=Auxiliary("Vp8lLzParity","Plane").CreateDelegate<Func<int,int,int>>();
        var population=Auxiliary("Vp8lLzParity","Population").CreateDelegate<Func<int[],long>>();
        var parsed=Vp8lMainParsing.Encode(pixels,index(pixels,width,75),width);var bits=parsed.Bits;
        var columns=(width+(1<<tileBits)-1)>>tileBits;
        var histograms=Enumerable.Range(0,columns*((height+(1<<tileBits)-1)>>tileBits)).Select(_=>new[]{new int[280+(bits==0?0:1<<bits)],new int[256],new int[256],new int[256],new int[40]}).ToArray();
        var tokens=new List<Vp8lFrameToken>();
        foreach(var token in (IEnumerable)parsed.Tokens)
        {
            var type=token.GetType();T Get<T>(string name)=>(T)type.GetProperty(name)!.GetValue(token)!;
            var position=Get<int>("Position");var pixel=Get<uint>("Pixel");var length=Get<int>("Length");var distance=Get<int>("Distance");var cached=type.GetProperty("CacheIndex") is {} cp?(int)cp.GetValue(token)!:-1;
            var h=histograms[(position/width>>tileBits)*columns+(position%width>>tileBits)];
            if(distance!=0){h[0][256+prefix(length).Code]++;h[4][prefix(plane(distance,width)).Code]++;}
            else if(cached>=0)h[0][280+cached]++;
            else{h[0][pixel>>8&255]++;h[1][pixel>>16&255]++;h[2][pixel&255]++;h[3][pixel>>24]++;}
            tokens.Add(new(position,pixel,length,distance,cached));
        }
        var grouped=Vp8lSpatialParity.Build(histograms,population);
        var frame=Vp8lFrameParity.Encode(stage,grouped.Map,grouped.Counts,tokens,bits,width,tileBits);
        return new(frame.Packet,new{choice,cacheBits=bits,groups=grouped.Counts.Length,tokens=tokens.Count,parser=parsed.Summary,framing=frame.Details});
    }
}
