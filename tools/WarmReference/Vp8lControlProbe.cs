#nullable enable
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Formats.Tar;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Text.Json;
using Lokad.Image;
public static class Vp8lControlProbe
{
    private static byte[] Fixture(string directory,string name)
    {
        using var image=ImageCodec.Decode(File.ReadAllBytes(Path.Combine(directory,name+".png")));var pixels=new byte[image.Width*image.Height*4];
        for(var y=0;y<image.Height;y++)image.GetRow8(y).CopyTo(pixels.AsSpan(y*image.Width*4));return pixels;
    }
    private static byte[] Member(string archive,string member)
    {
        using var input=File.OpenRead(archive);using var gzip=new GZipStream(input,CompressionMode.Decompress);using var tar=new TarReader(gzip);
        while(tar.GetNextEntry() is {} entry)if(entry.Name==member){using var bytes=new MemoryStream();entry.DataStream!.CopyTo(bytes);return bytes.ToArray();}
        throw new Exception("Missing frozen reference member "+member);
    }
    public static string Run(string originalPath,string fixtureDirectory,string firstArchive,string remainingArchive)
    {
        var original=File.ReadAllBytes(originalPath);var opaque=original.ToArray();for(var i=3;i<opaque.Length;i+=4)opaque[i]=255;
        var inputs=new[]{("Encode8",original,firstArchive,"tmp/e25-native-vp8l-rgba8.webp"),("EncodeOpaque8",opaque,remainingArchive,"artifacts/warm-reference/webp-lossless-opaque8/native.webp"),("PhotographLossless8",Fixture(fixtureDirectory,"photograph"),remainingArchive,"tmp/e33-photograph-native.webp"),("DiagramLossless8",Fixture(fixtureDirectory,"diagram"),remainingArchive,"tmp/e33-diagram-native.webp")};
        var results=new List<object>();
        foreach(var input in inputs)
        {
            var sourceHash=Convert.ToHexStringLower(SHA256.HashData(input.Item2));
            var result=Vp8lPacketParity.Build(input.Item2);var second=Vp8lPacketParity.Build(input.Item2);var reference=Member(input.Item3,input.Item4);
            if(!second.Packet.AsSpan().SequenceEqual(reference)||Object.ReferenceEquals(result.Packet,second.Packet)||Object.ReferenceEquals(result.GroupMap,second.GroupMap))throw new Exception("Independent packet results differ");
            second.Packet[^1]^=1;second.GroupMap[0]^=1;
            if(second.Packet.AsSpan().SequenceEqual(reference)||result.GroupMap.AsSpan().SequenceEqual(second.GroupMap))throw new Exception("Mutated output accepted");
            if(Convert.ToHexStringLower(SHA256.HashData(input.Item2))!=sourceHash)throw new Exception("Borrowed source changed");
            var same=result.Packet.AsSpan().SequenceEqual(reference);var first=-1;for(var i=0;i<Math.Min(result.Packet.Length,reference.Length);i++)if(result.Packet[i]!=reference[i]){first=i;break;}
            results.Add(new{name=input.Item1,rawExact=same,sourceSha256=sourceHash,sourceUnchanged=true,independentOutputs=true,negativeMutationsRejected=true,actualBytes=result.Packet.Length,expectedBytes=reference.Length,sha256=Convert.ToHexStringLower(SHA256.HashData(result.Packet)),firstDifference=first,result.Diagnostics,result.Framing,result.Clustering});
        }
        return JsonSerializer.Serialize(results,new JsonSerializerOptions{WriteIndented=true});
    }
}
