#nullable enable
using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
internal static class Vp8lInitialChoice
{
    private const long Scale=1L<<23;
    private static readonly Func<int,long> SLog=typeof(Vp8lPredictorParity).GetMethod("SLog",BindingFlags.Static|BindingFlags.NonPublic)!.CreateDelegate<Func<int,long>>();
    internal sealed record Result(int Mode,bool RedBlueZero,long[] Costs,int DistinctCapped);
    private static long Entropy(int[] h)
    {
        var total=h.Sum();var nonzero=h.Count(x=>x!=0);if(nonzero<=1)return 0;
        var entropy=SLog(total)-h.Sum(SLog);
        if(nonzero==2)return (99L*total*Scale+entropy+50)/100;
        var mix=nonzero==3?950:nonzero==4?700:627;
        var minimum=((long)mix*(2*total-h.Max())*Scale+(1000-mix)*entropy+500)/1000;
        return Math.Max(entropy,minimum);
    }
    internal static Result Select(byte[] rgba,int width,int height)
    {
        if(rgba.Length!=checked(width*height*4))throw new ArgumentException("RGBA size");
        uint Pixel(int at)=>(uint)rgba[4*at+3]<<24|(uint)rgba[4*at]<<16|(uint)rgba[4*at+1]<<8|rgba[4*at+2];
        var raw=Enumerable.Range(0,4).Select(_=>new int[256]).ToArray();var residual=Enumerable.Range(0,4).Select(_=>new int[256]).ToArray();
        var greenRaw=new[]{new int[256],new int[256]};var greenResidual=new[]{new int[256],new int[256]};
        var colors=new HashSet<uint>();var previous=Pixel(0);
        for(var at=0;at<width*height;at++)
        {
            var pixel=Pixel(at);if(colors.Count<=256)colors.Add(pixel);
            var diff=new int[4];var values=new[]{(int)(pixel>>24),(int)(pixel>>16&255),(int)(pixel>>8&255),(int)(pixel&255)};
            for(var c=0;c<4;c++)diff[c]=(values[c]-(int)(previous>>(24-8*c)&255))&255;
            var same=pixel==previous;previous=pixel;
            if(same||at>=width&&pixel==Pixel(at-width))continue;
            for(var c=0;c<4;c++){raw[c][values[c]]++;residual[c][diff[c]]++;}
            greenRaw[0][(values[1]-values[2])&255]++;greenRaw[1][(values[3]-values[2])&255]++;
            greenResidual[0][(diff[1]-diff[2])&255]++;greenResidual[1][(diff[3]-diff[2])&255]++;
        }
        if(colors.Count<=256)throw new NotSupportedException("Palette analysis is outside these frozen controls.");
        foreach(var h in residual)h[0]++;foreach(var h in greenResidual)h[0]++;
        var tiles=((width+7)>>3)*((height+7)>>3);
        var costs=new[]{raw.Sum(Entropy),residual.Sum(Entropy)+tiles*(long)Math.Round(Scale*Math.Log2(14)),Entropy(raw[0])+Entropy(raw[2])+greenRaw.Sum(Entropy),Entropy(residual[0])+Entropy(residual[2])+greenResidual.Sum(Entropy)+tiles*(long)Math.Round(Scale*Math.Log2(24))};
        var mode=0;for(var k=1;k<4;k++)if(costs[k]<costs[mode])mode=k;
        var r=mode==0?raw[1]:mode==1?residual[1]:mode==2?greenRaw[0]:greenResidual[0];var b=mode==0?raw[3]:mode==1?residual[3]:mode==2?greenRaw[1]:greenResidual[1];
        return new(mode,!Enumerable.Range(1,255).Any(i=>r[i]!=0||b[i]!=0),costs,colors.Count);
    }
}
