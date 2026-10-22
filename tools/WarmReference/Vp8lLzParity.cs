#nullable enable
using System;
using System.Linq;
using System.Collections.Generic;
using System.Numerics;
using System.Reflection;
internal readonly record struct Vp8lParityToken(int Position, uint Pixel, int Length, int Distance)
{
    internal bool Copy => Distance != 0;
}
internal static class Vp8lLzParity
{
    private const long Scale=1L<<23;
    private static readonly Func<int,long> SLog=typeof(Vp8lPredictorParity).GetMethod("SLog",BindingFlags.Static|BindingFlags.NonPublic)!.CreateDelegate<Func<int,long>>();
    private static readonly (int X,int Y)[] Geometry=Enumerable.Range(0,8).SelectMany(y=>Enumerable.Range(-7,16).Where(x=>y!=0||x>0).Select(x=>(X:x,Y:y))).OrderBy(p=>p.X*p.X+p.Y*p.Y).ThenByDescending(p=>p.Y).ThenBy(p=>p.X<0).ToArray();
    internal static int Plane(int distance,int width)
    {
        var y=distance/width;var x=distance%width;
        // The two coordinate forms overlap on narrow images. The native
        // strategy selects the direct coordinates before the wrapped form.
        if(x<=8&&y<8)return Array.IndexOf(Geometry,(x,y))+1;
        if(x>width-8&&y<7)return Array.IndexOf(Geometry,(x-width,y+1))+1;
        return distance+120;
    }
    private static (int Code,int Extra) Prefix(int value)
    {
        if(value<=4)return (Math.Max(0,value-1),0);
        var top=BitOperations.Log2((uint)(value-1));var extra=top-1;
        return (2*top+((value-1)>>extra&1),extra);
    }
    private static int Extra(int code)=>code<4?0:code/2-1;
    private static long Log(int count)
    {
        if(count<2)return 0;
        if(count<256)return (long)Math.Round(Scale*Math.Log2(count));
        if(count>=65536)return (long)Math.Floor(Scale/Math.Log(2)*Math.Log(count)+0.5);
        var shift=BitOperations.Log2((uint)count)-7;var value=(long)Math.Round(Scale*Math.Log2(count>>shift))+shift*Scale;
        if(count>=4096)value+=((long)(Scale/Math.Log(2))*(count&((1<<shift)-1))+count/2)/count;
        return value;
    }
    internal static List<Vp8lParityToken> Greedy(uint[] pixels,uint[] matches)
    {
        var result=new List<Vp8lParityToken>();var at=0;
        while(at<pixels.Length)
        {
            var length=(int)(matches[at]&4095);var distance=(int)(matches[at]>>12);
            if(length>=4)
            {
                var farthest=0;var stop=Math.Min(at+length,pixels.Length-1);
                for(var next=at+1;next<=stop;next++)
                {
                    var span=(int)(matches[next]&4095);var reach=next+(span>=4?span:1);
                    if(reach>farthest){length=next-at;farthest=reach;if(reach>=pixels.Length)break;}
                }
            }
            else length=1;
            result.Add(length==1?new(at,pixels[at],1,0):new(at,0,length,distance));at+=length;
        }
        return result;
    }
    internal static List<Vp8lParityToken> Repeats(uint[] pixels,int width)
    {
        int Match(int at,int distance){var n=0;while(n<Math.Min(4095,pixels.Length-at)&&pixels[at+n]==pixels[at+n-distance])n++;return n;}
        var result=new List<Vp8lParityToken>{new(0,pixels[0],1,0)};var at=1;
        while(at<pixels.Length)
        {
            var repeat=Match(at,1);var above=at>=width?Match(at,width):0;var distance=repeat>=above&&repeat>=4?1:above>=4?width:0;
            var length=distance==1?repeat:distance==width?above:1;
            result.Add(new(at,distance==0?pixels[at]:0,length,distance));at+=length;
        }
        return result;
    }
    internal static int[][] Histograms(IReadOnlyList<Vp8lParityToken> tokens,int width,bool plane)
    {
        var h=new[]{new int[280],new int[256],new int[256],new int[256],new int[40]};
        foreach(var token in tokens)
        {
            if(token.Copy){h[0][256+Prefix(token.Length).Code]++;h[4][Prefix(plane?Plane(token.Distance,width):token.Distance).Code]++;}
            else {h[0][(token.Pixel>>8)&255]++;h[1][(token.Pixel>>16)&255]++;h[2][token.Pixel&255]++;h[3][token.Pixel>>24]++;}
        }
        return h;
    }
    private static long Population(int[] counts)
    {
        var sum=counts.Sum();var nonzero=counts.Count(x=>x!=0);var entropy=SLog(sum)-counts.Sum(x=>SLog(x));var refined=entropy;
        if(nonzero<=1)refined=0;
        else if(nonzero==2)refined=(99*sum*Scale+entropy+50)/100;
        else
        {
            var mix=nonzero==3?950:nonzero==4?700:627;
            var lower=((long)mix*(2*sum-counts.Max())*Scale+(1000-mix)*entropy+500)/1000;
            refined=Math.Max(entropy,lower);
        }
        var runs=new int[2];var shortValues=new int[2];var longValues=new int[2];
        for(var start=0;start<counts.Length;)
        {
            var end=start+1;while(end<counts.Length&&counts[end]==counts[start])end++;
            var n=end-start;var kind=counts[start]==0?0:1;
            if(n>3){runs[kind]++;longValues[kind]+=n;}else shortValues[kind]+=n;
            start=end;
        }
        var overhead=57*Scale-(91*Scale+5)/10;
        overhead+=((long)runs[0]*1600+longValues[0]*240+runs[1]*2640L+longValues[1]*720+shortValues[0]*1840L+shortValues[1]*3360L)*(Scale>>10);
        return refined+overhead;
    }
    internal static long Estimate(IReadOnlyList<Vp8lParityToken> tokens,int width)
    {
        var h=Histograms(tokens,width,false);var cost=h.Sum(Population);
        for(var i=0;i<24;i++)cost+=h[0][256+i]*Extra(i)*Scale;
        for(var i=0;i<40;i++)cost+=h[4][i]*Extra(i)*Scale;
        return cost;
    }
    internal static List<Vp8lParityToken> Trace(uint[] pixels,uint[] matches,int width,IReadOnlyList<Vp8lParityToken> seed)
    {
        var hist=Histograms(seed,width,true);
        var prices=hist.Select(h=>h.Count(x=>x!=0)<=1?new long[h.Length]:h.Select(x=>Log(h.Sum())-Log(x)).ToArray()).ToArray();
        var lengthPrices=Enumerable.Range(0,Math.Min(4095,pixels.Length)).Select(k=>prices[0][256+Prefix(k).Code]+Prefix(k).Extra*Scale).ToArray();
        var costs=Enumerable.Repeat(long.MaxValue,pixels.Length).ToArray();var path=new int[pixels.Length];
        var pending=Enumerable.Repeat(long.MaxValue,pixels.Length).ToArray();var pendingSource=new int[pixels.Length];
        void Resolve(int at){if(pending[at]<costs[at]){costs[at]=pending[at];path[at]=at-pendingSource[at]+1;}}
        void Literal(int at,long prior)
        {
            var p=pixels[at];var price=prices[0][(p>>8)&255]+prices[1][(p>>16)&255]+prices[2][p&255]+prices[3][p>>24];
            var value=prior+(price*82+50)/100;
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
        var result=new List<Vp8lParityToken>();var position=0;
        foreach(var n in lengths){result.Add(n==1?new(position,pixels[position],1,0):new(position,0,n,(int)(matches[position]>>12)));position+=n;}
        return result;
    }
    internal static (List<Vp8lParityToken> Result,object Summary) Encode(uint[] pixels,uint[] matches,int width)
    {
        var standard=Greedy(pixels,matches);var rle=Repeats(pixels,width);var standardCost=Estimate(standard,width);var rleCost=Estimate(rle,width);
        var result=standard;var winner="standard";var finalCost=standardCost;List<Vp8lParityToken>? trace=null;long? traceCost=null;
        if(rleCost<finalCost){result=rle;winner="rle";finalCost=rleCost;}
        if(winner=="standard")
        {
            trace=Trace(pixels,matches,width,standard);traceCost=Estimate(trace,width);
            if(traceCost<finalCost){result=trace;winner="trace";finalCost=traceCost.Value;}
        }
        return (result,new{standardTokens=standard.Count,rleTokens=rle.Count,traceTokens=trace?.Count,standardCost,rleCost,traceCost,winner,finalCost});
    }
}
