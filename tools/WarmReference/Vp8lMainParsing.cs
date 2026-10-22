#nullable enable
using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
internal static class Vp8lMainParsing
{
    private static object Main(string name,params object[] args)=>typeof(Vp8lMainParity).GetMethod(name,BindingFlags.Static|BindingFlags.NonPublic)!.Invoke(null,args)!;
    private static object Uncached(string name,params object[] args)=>typeof(Vp8lAuxiliaryParity).Assembly.GetType("Vp8lLzParity")!.GetMethod(name,BindingFlags.Static|BindingFlags.NonPublic)!.Invoke(null,args)!;
    private static int BestCache(uint[] pixels,object tokens){var result=Main("Cache",pixels,tokens);return(int)result.GetType().GetProperty("BestBits")!.GetValue(result)!;}
    internal sealed record Result(object Tokens,int Bits,object Summary);
    internal static Result Encode(uint[] pixels,uint[] matches)
    {
        var standard=Main("Tokens","Greedy",pixels,matches);var repeated=Main("Tokens","Repeats",pixels,matches);
        var standardBits=BestCache(pixels,standard);var repeatedBits=BestCache(pixels,repeated);
        standard=standardBits==0?Uncached("Greedy",pixels,matches):Main("WithCache",pixels,standard,standardBits);
        repeated=repeatedBits==0?Uncached("Repeats",pixels,513):Main("WithCache",pixels,repeated,repeatedBits);
        long Cost(object tokens,int bits)=>(long)(bits==0?Uncached("Estimate",tokens,513):Main("Estimate",tokens,bits));
        var standardCost=Cost(standard,standardBits);var repeatedCost=Cost(repeated,repeatedBits);
        var result=standardCost<=repeatedCost?standard:repeated;var bits=standardCost<=repeatedCost?standardBits:repeatedBits;
        var winner=standardCost<=repeatedCost?"standard":"repeat";long? traceCost=null;
        if(winner=="standard")
        {
            var trace=bits==0?Uncached("Trace",pixels,matches,513,standard):Main("Trace",pixels,matches,513,standard,bits);
            traceCost=Cost(trace,bits);if(traceCost<standardCost){result=trace;winner="trace";}
        }
        return new(result,bits,new{standardBits,repeatedBits,standardCost,repeatedCost,traceCost,winner});
    }
}
