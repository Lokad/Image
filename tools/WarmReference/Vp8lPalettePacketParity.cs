// Source-only complete palette packet composition using our frozen helpers.
// Current main-parser/framer reuse restricts packed width to 513; production
// admission, general sizing and other palette orders remain unqualified.
#nullable enable
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
public sealed record Vp8lPalettePacketResult(byte[] Packet,object Details);
public static class Vp8lPalettePacketParity
{
    private static Assembly Model=>typeof(Vp8lPacketParity).Assembly;
    private static MethodInfo Own(string type,string name)=>Model.GetType(type)!.GetMethod(name,BindingFlags.Static|BindingFlags.NonPublic)!;
    private static MethodInfo Auxiliary(string type,string name)=>typeof(Vp8lAuxiliaryParity).Assembly.GetType(type)!.GetMethod(name,BindingFlags.Static|BindingFlags.NonPublic)!;
    public static Vp8lPalettePacketResult Encode(byte[] source,int width,int height)
    {
        var palette=Vp8lPaletteParity.Build(source,width,height);
        if(palette.CodedWidth!=513)throw new NotSupportedException("This stage reuses the frozen width-513 main parser.");
        var pixels=palette.Packed;
        var matches=Auxiliary("Vp8lMatchParity","Build").CreateDelegate<Func<uint[],int,int,uint[]>>()(pixels,palette.CodedWidth,75);
        var parsed=Own("Vp8lMainParsing","Encode").Invoke(null,new object[]{pixels,matches})!;var resultType=parsed.GetType();
        var tokens=(IEnumerable)resultType.GetProperty("Tokens")!.GetValue(parsed)!;var bits=(int)resultType.GetProperty("Bits")!.GetValue(parsed)!;
        var prefix=Auxiliary("Vp8lLzParity","Prefix").CreateDelegate<Func<int,(int Code,int Extra)>>();
        var plane=Auxiliary("Vp8lLzParity","Plane").CreateDelegate<Func<int,int,int>>();
        var population=Auxiliary("Vp8lLzParity","Population").CreateDelegate<Func<int[],long>>();
        var histograms=Enumerable.Range(0,65*((height+7)>>3)).Select(_=>new[]{new int[280+(bits==0?0:1<<bits)],new int[256],new int[256],new int[256],new int[40]}).ToArray();
        var tokenType=Model.GetType("Vp8lFrameToken")!;var list=(IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(tokenType))!;
        foreach(var token in tokens)
        {
            var type=token.GetType();T Get<T>(string name)=>(T)type.GetProperty(name)!.GetValue(token)!;
            var position=Get<int>("Position");var pixel=Get<uint>("Pixel");var length=Get<int>("Length");var distance=Get<int>("Distance");var cached=type.GetProperty("CacheIndex") is {} cp?(int)cp.GetValue(token)!:-1;
            var h=histograms[(position/513>>3)*65+(position%513>>3)];
            if(distance!=0){h[0][256+prefix(length).Code]++;h[4][prefix(plane(distance,513)).Code]++;}
            else if(cached>=0)h[0][280+cached]++;
            else{h[0][pixel>>8&255]++;h[1][pixel>>16&255]++;h[2][pixel&255]++;h[3][pixel>>24]++;}
            list.Add(Activator.CreateInstance(tokenType,new object[]{position,pixel,length,distance,cached}));
        }
        var grouped=Own("Vp8lSpatialParity","Build").Invoke(null,new object[]{histograms,population})!;var groupedType=grouped.GetType();
        var map=(int[])groupedType.GetProperty("Map")!.GetValue(grouped)!;var counts=(int[][][])groupedType.GetProperty("Counts")!.GetValue(grouped)!;
        var writerType=typeof(Vp8lAuxiliaryParity).Assembly.GetType("Vp8lParityBits")!;var writer=Activator.CreateInstance(writerType,true)!;
        var put=writerType.GetMethod("Put",BindingFlags.Instance|BindingFlags.NonPublic)!.CreateDelegate<Action<int,int>>(writer);
        var finish=writerType.GetMethod("Finish",BindingFlags.Instance|BindingFlags.NonPublic)!.CreateDelegate<Func<byte[]>>(writer);
        var count=writerType.GetProperty("Count",BindingFlags.Instance|BindingFlags.NonPublic)!.GetMethod!.CreateDelegate<Func<int>>(writer);
        put(0x2f,8);put(width-1,14);put(height-1,14);put(Enumerable.Range(0,width*height).Any(i=>source[4*i+3]!=255)?1:0,1);put(0,3);put(1,1);put(3,2);put(palette.Colors.Length-1,8);
        var auxiliary=Vp8lAuxiliaryParity.Encode(palette.Deltas,palette.Deltas.Length,75);
        for(var bit=0;bit<auxiliary.BitCount;bit++)put(auxiliary.Bytes[bit>>3]>>(bit&7)&1,1);
        var coded=new byte[pixels.Length*4];Buffer.BlockCopy(pixels,0,coded,0,coded.Length);
        var stage=Activator.CreateInstance(Model.GetType("Vp8lInputParity")!,new object[]{finish(),count(),coded,new{palette=palette.Colors.Length}})!;
        var frame=Own("Vp8lFrameParity","Encode").Invoke(null,new object[]{stage,map,counts,list,bits})!;var frameType=frame.GetType();
        return new((byte[])frameType.GetField("Item1")!.GetValue(frame)!,new{colors=palette.Colors.Length,packingBits=palette.PackingBits,codedWidth=palette.CodedWidth,cacheBits=bits,groups=counts.Length,tokens=list.Count,framing=frameType.GetField("Item2")!.GetValue(frame),parser=resultType.GetProperty("Summary")!.GetValue(parsed)});
    }
}
