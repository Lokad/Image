#nullable enable
using System;
using System.Linq;
using System.Collections.Generic;
using System.Buffers.Binary;
using System.Reflection;
using System.Security.Cryptography;
internal readonly record struct Vp8lFrameToken(int Position,uint Pixel,int Length,int Distance,int Cached);
internal static class Vp8lFrameParity
{
    // Reuse our frozen LSB writer and tree-header selector without rebuilding it.
    private sealed class Writer
    {
        internal readonly object State;
        internal readonly Action<int,int> Put;
        internal readonly Func<int> Count;
        internal readonly Func<byte[]> Finish;
        internal Writer()
        {
            var type=typeof(Vp8lAuxiliaryParity).Assembly.GetType("Vp8lParityBits")!;
            State=Activator.CreateInstance(type,true)!;
            Put=type.GetMethod("Put",BindingFlags.Instance|BindingFlags.NonPublic)!.CreateDelegate<Action<int,int>>(State);
            Count=type.GetProperty("Count",BindingFlags.Instance|BindingFlags.NonPublic)!.GetMethod!.CreateDelegate<Func<int>>(State);
            Finish=type.GetMethod("Finish",BindingFlags.Instance|BindingFlags.NonPublic)!.CreateDelegate<Func<byte[]>>(State);
        }
        internal void Append(byte[] bytes,int bits){for(var bit=0;bit<bits;bit++)Put(bytes[bit>>3]>>(bit&7)&1,1);}
    }
    private sealed class Codes
    {
        private readonly byte[] lengths;
        private readonly int[] words;
        internal Codes(byte[] source)
        {
            lengths=source.ToArray();words=new int[source.Length];var populations=new int[16];foreach(var n in source)populations[n]++;
            var next=new int[16];var code=0;
            for(var depth=1;depth<16;depth++){code=(code+(depth==1?0:populations[depth-1]))<<1;next[depth]=code;}
            for(var symbol=0;symbol<source.Length;symbol++)
            {
                var n=source[symbol];if(n==0)continue;var value=next[n]++;var word=0;
                for(var i=0;i<n;i++){word=2*word+(value&1);value>>=1;}words[symbol]=word;
            }
            if(source.Count(x=>x!=0)<=1)Array.Clear(lengths);
        }
        internal void Write(Writer writer,int symbol)=>writer.Put(words[symbol],lengths[symbol]);
    }
    private static MethodInfo Method(string type,string name)=>typeof(Vp8lAuxiliaryParity).Assembly.GetType(type)!.GetMethod(name,BindingFlags.Static|BindingFlags.NonPublic)!;
    private static readonly Func<int[],int,byte[]> Lengths=Method("Vp8lPrefixParity","Lengths").CreateDelegate<Func<int[],int,byte[]>>();
    private static readonly Func<int,int,int> Plane=Method("Vp8lLzParity","Plane").CreateDelegate<Func<int,int,int>>();
    private static void Prefix(Writer writer,Codes codes,int value,int offset)
    {
        if(value<=4){codes.Write(writer,offset+value-1);return;}
        var high=(int)System.Numerics.BitOperations.Log2((uint)(value-1));var extra=high-1;
        var symbol=2*high+((value-1)>>extra&1);codes.Write(writer,offset+symbol);writer.Put((value-1)&((1<<extra)-1),extra);
    }
    internal static (byte[] Packet,object Details) Encode(Vp8lInputParity prefix,int[] map,int[][][] counts,List<Vp8lFrameToken> tokens,int cacheBits)
    {
        var groupPixels=map.Select(x=>(uint)x<<8).ToArray();var auxiliary=counts.Length>1?Vp8lAuxiliaryParity.Encode(groupPixels,65,75):null;
        var trees=counts.Select(group=>group.Select(histogram=>Lengths(histogram,15)).ToArray()).ToArray();
        using var treeHash=IncrementalHash.CreateHash(HashAlgorithmName.SHA256);foreach(var group in trees)foreach(var tree in group)treeHash.AppendData(tree);
        var writer=new Writer();writer.Append(prefix.Bytes,prefix.BitCount);
        writer.Put(0,1);writer.Put(cacheBits==0?0:1,1);if(cacheBits!=0)writer.Put(cacheBits,4);writer.Put(counts.Length>1?1:0,1);if(counts.Length>1)writer.Put(1,3);
        var mapStart=writer.Count();if(auxiliary!=null)writer.Append(auxiliary.Bytes,auxiliary.BitCount);var mapEnd=writer.Count();
        var header=Method("Vp8lBitParity","Tree");foreach(var group in trees)foreach(var tree in group)header.Invoke(null,new object[]{writer.State,tree});
        var dataStart=writer.Count();var codes=trees.Select(group=>group.Select(tree=>new Codes(tree)).ToArray()).ToArray();
        foreach(var token in tokens)
        {
            var group=map[(token.Position/513>>3)*65+(token.Position%513>>3)];var c=codes[group];
            if(token.Distance!=0){Prefix(writer,c[0],token.Length,256);Prefix(writer,c[4],Plane(token.Distance,513),0);}
            else if(token.Cached>=0)c[0].Write(writer,280+token.Cached);
            else{c[0].Write(writer,(int)(token.Pixel>>8&255));c[1].Write(writer,(int)(token.Pixel>>16&255));c[2].Write(writer,(int)(token.Pixel&255));c[3].Write(writer,(int)(token.Pixel>>24));}
        }
        var payload=writer.Finish();var packet=new byte[20+payload.Length+(payload.Length&1)];
        "RIFF"u8.CopyTo(packet);BinaryPrimitives.WriteUInt32LittleEndian(packet.AsSpan(4),(uint)(packet.Length-8));"WEBPVP8L"u8.CopyTo(packet.AsSpan(8));
        BinaryPrimitives.WriteUInt32LittleEndian(packet.AsSpan(16),(uint)payload.Length);payload.CopyTo(packet,20);
        return(packet,new{groups=counts.Length,huffmanTrees=counts.Length*5,treeLengthsSha256=Convert.ToHexStringLower(treeHash.GetHashAndReset()),groupMapBits=auxiliary?.BitCount??0,mapStartBit=mapStart,mapEndBit=mapEnd,firstDataBit=dataStart,endBit=writer.Count(),payloadBytes=payload.Length,packetBytes=packet.Length});
    }
}
