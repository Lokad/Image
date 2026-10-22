#nullable enable
using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections;
using System.Reflection;
internal static class Vp8lMainParsing
{
    private static object Main(string name,params object[] args)=>typeof(Vp8lMainParity).GetMethod(name,BindingFlags.Static|BindingFlags.NonPublic)!.Invoke(null,args)!;
    private static object Uncached(string name,params object[] args)=>typeof(Vp8lAuxiliaryParity).Assembly.GetType("Vp8lLzParity")!.GetMethod(name,BindingFlags.Static|BindingFlags.NonPublic)!.Invoke(null,args)!;
    private static int BestCache(uint[] pixels,object tokens){var result=Main("Cache",pixels,tokens);return(int)result.GetType().GetProperty("BestBits")!.GetValue(result)!;}
    private static object Repeated(uint[] pixels,int width)
    {
        // The frozen cached parser's original seed entrypoint fixed width513.
        // Reuse its token/cache operations with the dimension-aware repeat seed.
        var type=typeof(Vp8lMainParity).GetNestedType("Token",BindingFlags.NonPublic)!;
        var result=(IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(type))!;
        foreach(var token in (IEnumerable)Uncached("Repeats",pixels,width))
        {
            var t=token.GetType();T Get<T>(string name)=>(T)t.GetProperty(name)!.GetValue(token)!;
            result.Add(Activator.CreateInstance(type,new object[]{Get<int>("Position"),Get<uint>("Pixel"),Get<int>("Length"),Get<int>("Distance"),-1}));
        }
        return result;
    }
    internal sealed record Result(object Tokens,int Bits,object Summary);
    internal static Result Encode(uint[] pixels,uint[] matches,int width=513)
    {
        var standard=Main("Tokens","Greedy",pixels,matches);var repeated=Repeated(pixels,width);
        var standardBits=BestCache(pixels,standard);var repeatedBits=BestCache(pixels,repeated);
        standard=standardBits==0?Uncached("Greedy",pixels,matches):Main("WithCache",pixels,standard,standardBits);
        repeated=repeatedBits==0?Uncached("Repeats",pixels,width):Main("WithCache",pixels,repeated,repeatedBits);
        long Cost(object tokens,int bits)=>(long)(bits==0?Uncached("Estimate",tokens,width):Main("Estimate",tokens,bits));
        var standardCost=Cost(standard,standardBits);var repeatedCost=Cost(repeated,repeatedBits);
        var result=standardCost<=repeatedCost?standard:repeated;var bits=standardCost<=repeatedCost?standardBits:repeatedBits;
        var winner=standardCost<=repeatedCost?"standard":"repeat";long? traceCost=null;
        if(winner=="standard")
        {
            var trace=bits==0?Uncached("Trace",pixels,matches,width,standard):Main("Trace",pixels,matches,width,standard,bits);
            traceCost=Cost(trace,bits);if(traceCost<standardCost){result=trace;winner="trace";}
        }
        return new(result,bits,new{standardBits,repeatedBits,standardCost,repeatedCost,traceCost,winner});
    }
}
