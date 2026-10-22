#nullable enable
using System;
using System.Linq;
using System.Collections.Generic;
using System.Buffers.Binary;
using System.Security.Cryptography;
internal sealed record Vp8lInputParity(byte[] Bytes,int BitCount,byte[] CodedResidual,object Diagnostics)
{
    internal static Vp8lInputParity Build(byte[] source,Vp8lInitialChoice.Result choice)
    {
        if(choice.Mode==3)
        {
            var result=Vp8lAuxiliaryParity.Build(source);
            return new(result.Bytes,result.BitCount,result.CodedResidual,new{mode=3,predictorSha256=Convert.ToHexStringLower(SHA256.HashData(result.Modes)),colorSha256=Convert.ToHexStringLower(SHA256.HashData(result.ColorMap))});
        }
        if(choice.Mode!=2)throw new NotSupportedException("Other transform families are outside these frozen controls.");
        var pixels=new byte[source.Length];var alpha=false;
        for(var at=0;at<source.Length;at+=4)
        {
            var red=source[at];var green=source[at+1];var blue=source[at+2];var a=source[at+3];alpha|=a!=255;
            var pixel=(uint)a<<24|(uint)((red-green)&255)<<16|(uint)green<<8|(uint)((blue-green)&255);
            BinaryPrimitives.WriteUInt32LittleEndian(pixels.AsSpan(at),pixel);
        }
        ulong header=0x2fUL|512UL<<8|258UL<<22|(alpha?1UL<<36:0)|1UL<<40|2UL<<41;
        var bytes=new byte[6];for(var i=0;i<6;i++)bytes[i]=(byte)(header>>(8*i));
        return new(bytes,43,pixels,new{mode=2});
    }
}
