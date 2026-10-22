// Source-only spatial-without-subtract-green model, currently uniform maps only.
// Predictor/color arithmetic, auxiliary syntax and framing follow RFC 9649.
#nullable enable
using System;
using System.Linq;
using System.Reflection;
using System.Buffers.Binary;
using System.Security.Cryptography;
public sealed record Vp8lSpatialOnlyResult(byte[] Packet,byte[] Modes,byte[] ColorMap,byte[] Residual,byte[] Coded,object Details);
public static class Vp8lSpatialOnlyParity
{
    private static MethodInfo Auxiliary(string type,string name)=>typeof(Vp8lAuxiliaryParity).Assembly.GetType(type)!.GetMethod(name,BindingFlags.Static|BindingFlags.NonPublic)!;
    public static Vp8lSpatialOnlyResult Encode(byte[] source,int width,int height)
    {
        var choice=Vp8lInitialChoice.Select(source,width,height);
        if(choice.Mode!=1)throw new NotSupportedException("Expected spatial-only selection.");
        var modes=Vp8lPredictorParity.EncodeModes(source,width,height,choice.TransformBits,false);
        var residual=Vp8lPredictorParity.Residual(source,width,height,choice.TransformBits,modes,false);
        var color=Vp8lCrossColorParity.Select(residual,width,height,choice.TransformBits,75);
        var colors=new uint[color.Map.Length/4];
        for(var i=0;i<colors.Length;i++)colors[i]=BinaryPrimitives.ReadUInt32LittleEndian(color.Map.AsSpan(i*4));
        // Uniform maps can increase sampling through the format maximum without
        // changing any pixel's predictor or color transform. Other maps are open.
        if(width>512||height>512||modes.Distinct().Count()!=1||colors.Distinct().Count()!=1)
            throw new NotSupportedException("Only bounded uniform transform maps are qualified in this stage.");
        var writerType=typeof(Vp8lAuxiliaryParity).Assembly.GetType("Vp8lParityBits")!;var writer=Activator.CreateInstance(writerType,true)!;
        var put=writerType.GetMethod("Put",BindingFlags.Instance|BindingFlags.NonPublic)!.CreateDelegate<Action<int,int>>(writer);
        var finish=writerType.GetMethod("Finish",BindingFlags.Instance|BindingFlags.NonPublic)!.CreateDelegate<Func<byte[]>>(writer);
        var count=writerType.GetProperty("Count",BindingFlags.Instance|BindingFlags.NonPublic)!.GetMethod!.CreateDelegate<Func<int>>(writer);
        var lengths=Auxiliary("Vp8lPrefixParity","Lengths").CreateDelegate<Func<int[],int,byte[]>>();var tree=Auxiliary("Vp8lBitParity","Tree");
        void Singleton(uint pixel)
        {
            put(0,1); // No auxiliary color cache. Singleton trees carry no data.
            var values=new[]{(int)(pixel>>8&255),(int)(pixel>>16&255),(int)(pixel&255),(int)(pixel>>24),-1};
            for(var bank=0;bank<5;bank++)
            {
                var h=new int[bank==0?280:bank==4?40:256];if(values[bank]>=0)h[values[bank]]=1;
                tree.Invoke(null,new object[]{writer,lengths(h,15)});
            }
        }
        put(0x2f,8);put(width-1,14);put(height-1,14);put(Enumerable.Range(0,width*height).Any(i=>source[4*i+3]!=255)?1:0,1);put(0,3);
        put(1,1);put(0,2);put(7,3);Singleton(0xff000000u|(uint)modes[0]<<8);
        put(1,1);put(1,2);put(7,3);Singleton(colors[0]);
        var stage=new Vp8lInputParity(finish(),count(),color.Coded,new{mode=1,predictorBits=9,colorBits=9});
        var frame=Vp8lDirectPacketParity.Compose(stage,width,height,choice.HistogramBits,choice);
        string Hash(byte[] bytes)=>Convert.ToHexStringLower(SHA256.HashData(bytes));
        return new(frame.Packet,modes,color.Map,residual,color.Coded,new{choice,predictorBits=9,colorBits=9,predictorResidualSha256=Hash(residual),codedResidualSha256=Hash(color.Coded),frame=frame.Details});
    }
}
