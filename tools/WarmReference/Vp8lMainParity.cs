// Source-only main-image parser for ONE pinned VP8L control, outside production.
// Uses the independently implemented auxiliary-stage model; no oracle inputs.
#nullable enable
using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Collections;
using System.Numerics;
using System.Reflection;
using System.Security.Cryptography;
using System.Text.Json;
using System.Buffers.Binary;
public static partial class Vp8lMainParity
{
    private readonly record struct Token(int Position, uint Pixel, int Length, int Distance, int CacheIndex=-1);
    private const long Scale=1L<<23;
    private static Assembly Model => typeof(Vp8lAuxiliaryParity).Assembly;
    private static MethodInfo Method(string type,string name) => Model.GetType(type)!.GetMethod(name,BindingFlags.Static|BindingFlags.NonPublic)!;
    private static readonly Func<int[],long> Population=Method("Vp8lLzParity","Population").CreateDelegate<Func<int[],long>>();
    private static readonly Func<uint[],int,int,uint[]> Index=Method("Vp8lMatchParity","Build").CreateDelegate<Func<uint[],int,int,uint[]>>();
    private static List<Token> Tokens(string method,uint[] pixels,uint[] matches)
    {
        var args=method=="Greedy"?new object[]{pixels,matches}:new object[]{pixels,513};
        var result=new List<Token>();
        foreach(var item in (IEnumerable)Method("Vp8lLzParity",method).Invoke(null,args)!)
        {
            var type=item.GetType();T Get<T>(string name)=>(T)type.GetProperty(name)!.GetValue(item)!;
            result.Add(new(Get<int>("Position"),Get<uint>("Pixel"),Get<int>("Length"),Get<int>("Distance")));
        }
        return result;
    }
    private static int LengthCode(int length)
    {
        if(length<=4)return length-1;
        var high=(int)BitOperations.Log2((uint)(length-1));return high*2+((length-1)>>(high-1)&1);
    }
    private sealed record CacheChoice(int SeedTokens,int BestBits,long[] Costs);
    private static CacheChoice Cache(uint[] pixels,List<Token> tokens)
        => CacheLimited(pixels,tokens,10);
    private static CacheChoice CacheLimited(uint[] pixels,List<Token> tokens,int maximumBits)
    {
        var costs=new long[maximumBits+1];var best=0;
        for(var bits=0;bits<=maximumBits;bits++)
        {
            var cache=new uint[bits==0?0:1<<bits];
            var hist=new[]{new int[280+cache.Length],new int[256],new int[256],new int[256],new int[40]};
            foreach(var token in tokens)
            {
                if(token.Distance!=0)
                {
                    hist[0][256+LengthCode(token.Length)]++;
                    if(bits>0)for(var at=token.Position;at<token.Position+token.Length;at++)
                    {var pixel=pixels[at];cache[unchecked(pixel*0x1e35a7bdu)>>(32-bits)]=pixel;}
                }
                else
                {
                    var pixel=pixels[token.Position];var key=bits==0?0:(int)(unchecked(pixel*0x1e35a7bdu)>>(32-bits));
                    if(bits>0&&cache[key]==pixel)hist[0][280+key]++;
                    else
                    {
                        hist[0][pixel>>8&255]++;hist[1][pixel>>16&255]++;hist[2][pixel&255]++;hist[3][pixel>>24]++;
                        if(bits>0)cache[key]=pixel;
                    }
                }
            }
            costs[bits]=hist.Sum(Population);if(costs[bits]<costs[best])best=bits;
        }
        return new(tokens.Count,best,costs);
    }
    public static string Run(byte[] source,string? codedOutput=null)
    {
        var hash=Convert.ToHexStringLower(SHA256.HashData(source));
        if(hash!="70f0fb18c341e2740b875a5a34e21ca63f0821b3386352fe2971020c5a6aac80")throw new Exception("Source changed");
        var stage=Vp8lAuxiliaryParity.Build(source);var coded=stage.CodedResidual;
        var pixels=new uint[coded.Length/4];if(!BitConverter.IsLittleEndian)throw new Exception("Little endian stage required");Buffer.BlockCopy(coded,0,pixels,0,coded.Length);
        var matches=Index(pixels,513,75);var indexBytes=new byte[coded.Length];Buffer.BlockCopy(matches,0,indexBytes,0,indexBytes.Length);
        var standard=Tokens("Greedy",pixels,matches);var repeats=Tokens("Repeats",pixels,matches);
        var standardCache=Cache(pixels,standard);var repeatCache=Cache(pixels,repeats);
        var caches=new{standard=standardCache,repeats=repeatCache};
        var parsed=Parse(pixels,matches,standard,repeats,standardCache.BestBits,repeatCache.BestBits);
        if(codedOutput!=null)File.WriteAllBytes(codedOutput,coded);
        if(Convert.ToHexStringLower(SHA256.HashData(source))!=hash)throw new Exception("Source mutated");
        return JsonSerializer.Serialize(new{sourceOnly=true,sourceSha256=hash,codedResidualSha256=Convert.ToHexStringLower(SHA256.HashData(coded)),samples=pixels.Length,indexSha256=Convert.ToHexStringLower(SHA256.HashData(indexBytes)),caches,parser=parsed,oracleEncoderInputs=false,productionChanged=false},new JsonSerializerOptions{WriteIndented=true});
    }
}
public static partial class Vp8lMainParity
{
    private static readonly Func<int,long> Log=Method("Vp8lLzParity","Log").CreateDelegate<Func<int,long>>();
    private static readonly Func<int,int,int> Plane=Method("Vp8lLzParity","Plane").CreateDelegate<Func<int,int,int>>();
    private static readonly Func<int,(int Code,int Extra)> Prefix=Method("Vp8lLzParity","Prefix").CreateDelegate<Func<int,(int,int)>>();
    private static List<Token> WithCache(uint[] pixels,IReadOnlyList<Token> seed,int bits)
    {
        var cache=new uint[1<<bits];var result=new List<Token>(seed.Count);
        foreach(var token in seed)
        {
            if(token.Distance!=0)
            {
                result.Add(token);
                for(var at=token.Position;at<token.Position+token.Length;at++)
                {var p=pixels[at];cache[unchecked(p*0x1e35a7bdu)>>(32-bits)]=p;}
            }
            else
            {
                var p=pixels[token.Position];var key=(int)(unchecked(p*0x1e35a7bdu)>>(32-bits));
                result.Add(cache[key]==p?new(token.Position,0,1,0,key):new(token.Position,p,1,0));cache[key]=p;
            }
        }
        return result;
    }
    private static int[][] Histograms(IReadOnlyList<Token> tokens,int width,bool plane,int bits)
    {
        var h=new[]{new int[280+(1<<bits)],new int[256],new int[256],new int[256],new int[40]};
        foreach(var t in tokens)
        {
            if(t.Distance!=0){h[0][256+Prefix(t.Length).Code]++;h[4][Prefix(plane?Plane(t.Distance,width):t.Distance).Code]++;}
            else if(t.CacheIndex>=0)h[0][280+t.CacheIndex]++;
            else{h[0][t.Pixel>>8&255]++;h[1][t.Pixel>>16&255]++;h[2][t.Pixel&255]++;h[3][t.Pixel>>24]++;}
        }
        return h;
    }
    private static long Estimate(IReadOnlyList<Token> tokens,int bits)
    {
        var h=Histograms(tokens,513,false,bits);var cost=h.Sum(Population);
        for(var code=4;code<24;code++)cost+=(long)h[0][256+code]*(code/2-1)*Scale;
        for(var code=4;code<40;code++)cost+=(long)h[4][code]*(code/2-1)*Scale;
        return cost;
    }
    private static object Parse(uint[] pixels,uint[] matches,List<Token> standard,List<Token> repeats,int standardBits,int repeatBits)
    {
        standard=WithCache(pixels,standard,standardBits);repeats=WithCache(pixels,repeats,repeatBits);
        var standardCost=Estimate(standard,standardBits);var repeatCost=Estimate(repeats,repeatBits);
        var result=standardCost<=repeatCost?standard:repeats;var bits=standardCost<=repeatCost?standardBits:repeatBits;
        var winner=standardCost<=repeatCost?"standard":"repeat";
        var seedCost=Math.Min(standardCost,repeatCost);long? traceCost=null;int? traceTokens=null;
        if(winner=="standard")
        {
            var trace=Trace(pixels,matches,513,standard,bits);traceCost=Estimate(trace,bits);traceTokens=trace.Count;
            if(traceCost<seedCost){result=trace;winner="trace";}
        }
        using var hash=IncrementalHash.CreateHash(HashAlgorithmName.SHA256);Span<byte> record=stackalloc byte[16];
        var literals=0;var copies=0;var caches=0;
        foreach(var t in result)
        {
            var type=t.Distance!=0?1:t.CacheIndex>=0?2:0;
            var value=type==1?(uint)Plane(t.Distance,513):type==2?(uint)t.CacheIndex:t.Pixel;
            BinaryPrimitives.WriteUInt32LittleEndian(record,(uint)t.Position);BinaryPrimitives.WriteUInt32LittleEndian(record[4..],(uint)type);
            BinaryPrimitives.WriteUInt32LittleEndian(record[8..],value);BinaryPrimitives.WriteUInt32LittleEndian(record[12..],(uint)t.Length);hash.AppendData(record);
            if(type==0)literals++;else if(type==1)copies++;else caches++;
        }
        return new{cacheBits=bits,standardTokens=standard.Count,repeatTokens=repeats.Count,standardCost,repeatCost,traceTokens,traceCost,winner,tokens=result.Count,literals,copies,caches,semanticTokenSha256=Convert.ToHexStringLower(hash.GetHashAndReset())};
    }
}
public static partial class Vp8lMainParity
{
    private static List<Token> Trace(uint[] pixels,uint[] matches,int width,IReadOnlyList<Token> seed,int bits)
    {
        var hist=Histograms(seed,width,true,bits);
        var prices=hist.Select(h=>h.Count(x=>x!=0)<=1?new long[h.Length]:h.Select(x=>Log(h.Sum())-Log(x)).ToArray()).ToArray();
        var lengthPrices=Enumerable.Range(0,Math.Min(4095,pixels.Length)).Select(k=>prices[0][256+Prefix(k).Code]+Prefix(k).Extra*Scale).ToArray();
        var cache=new uint[1<<bits];
        var costs=Enumerable.Repeat(long.MaxValue,pixels.Length).ToArray();var path=new int[pixels.Length];
        var pending=Enumerable.Repeat(long.MaxValue,pixels.Length).ToArray();var pendingSource=new int[pixels.Length];
        void Resolve(int at){if(pending[at]<costs[at]){costs[at]=pending[at];path[at]=at-pendingSource[at]+1;}}
        void Literal(int at,long prior)
        {
            var p=pixels[at];var slot=(int)(unchecked(p*0x1e35a7bdu)>>(32-bits));
            var hit=cache[slot]==p;
            var price=hit?prices[0][280+slot]:prices[0][(p>>8)&255]+prices[1][(p>>16)&255]+prices[2][p&255]+prices[3][p>>24];
            if(!hit)cache[slot]=p;
            var value=prior+(price*(hit?68:82)+50)/100;
            if(value<costs[at]){costs[at]=value;path[at]=1;}
        }
        void Push(int at,int length,long prior)
        {
            for(var k=0;k<length;k++)
            {
                var value=prior+lengthPrices[k];var end=at+k;
                if(length<10){if(value<costs[end]){costs[end]=value;path[end]=k+1;}}
                else if(value<pending[end]){pending[end]=value;pendingSource[end]=at;}
            }
        }
        Literal(0,0);var previousDistance=-1;var previousLength=-1;var firstConstant=false;var reach=0;long distancePrice=-1;
        for(var at=1;at<pixels.Length;at++)
        {
            Literal(at,costs[at-1]);var distance=(int)(matches[at]>>12);var length=(int)(matches[at]&4095);
            if(length>=2)
            {
                if(distance!=previousDistance)
                {
                    var prefix=Prefix(Plane(distance,width));distancePrice=prices[4][prefix.Code]+prefix.Extra*Scale;
                    firstConstant=true;Push(at,length,costs[at-1]+distancePrice);
                }
                else
                {
                    if(firstConstant){reach=at-1+previousLength-1;firstConstant=false;}
                    if(at+length-1>reach)
                    {
                        var next=at;var nextLength=0;
                        for(;next<=reach;next++)
                        {
                            nextLength=(int)(matches[next+1]&4095);
                            if((int)(matches[next+1]>>12)!=distance){nextLength=(int)(matches[next]&4095);break;}
                        }
                        Resolve(next-1);Resolve(next);Push(next,nextLength,costs[next-1]+distancePrice);reach=next+nextLength-1;
                    }
                }
            }
            Resolve(at);previousDistance=distance;previousLength=length;
        }
        var lengths=new List<int>();for(var at=pixels.Length-1;at>=0;at-=path[at]){if(path[at]<1)throw new Exception("Incomplete path");lengths.Add(path[at]);}lengths.Reverse();
        var result=new List<Token>();var position=0;
        foreach(var n in lengths){result.Add(n==1?new(position,pixels[position],1,0):new(position,0,n,(int)(matches[position]>>12)));position+=n;}
        return WithCache(pixels,result,bits);
    }
}
