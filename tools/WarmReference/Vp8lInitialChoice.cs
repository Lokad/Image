#nullable enable
using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
internal static class Vp8lInitialChoice
{
    private const long Scale=1L<<23;
    private static readonly Func<int,long> SLog=typeof(Vp8lPredictorParity).GetMethod("SLog",BindingFlags.Static|BindingFlags.NonPublic)!.CreateDelegate<Func<int,long>>();
    internal sealed record Result(int Mode,bool RedBlueZero,long[] Costs,int DistinctCapped,int HistogramBits,int TransformBits);
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
        var colors=new HashSet<uint>();var paletteHash=new int[256];var previous=Pixel(0);
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
            // A scalar multiplicative sketch estimates palette-index entropy;
            // its pinned hash parameter is not a format or implementation table.
            paletteHash[unchecked((pixel+(pixel>>19))*0x39c5fba7u)>>24]++;
        }
        var palettePossible=colors.Count<=256;
        int Tiles(int bits)=>((width+(1<<bits)-1)>>bits)*((height+(1<<bits)-1)>>bits);
        var histogramBits=palettePossible?5:3;
        while(histogramBits<9&&Tiles(histogramBits)>2600)histogramBits++;
        while(histogramBits>2&&Tiles(histogramBits-1)==1)histogramBits--;
        var transformBits=Math.Min(5,histogramBits);
        if(palettePossible&&colors.Count<=16)return new(4,true,Array.Empty<long>(),colors.Count,histogramBits,transformBits);
        foreach(var h in residual)h[0]++;foreach(var h in greenResidual)h[0]++;
        var tiles=Tiles(transformBits);
        var costs=new[]{raw.Sum(Entropy),residual.Sum(Entropy)+tiles*(long)Math.Round(Scale*Math.Log2(14)),Entropy(raw[0])+Entropy(raw[2])+greenRaw.Sum(Entropy),Entropy(residual[0])+Entropy(residual[2])+greenResidual.Sum(Entropy)+tiles*(long)Math.Round(Scale*Math.Log2(24))};
        if(palettePossible)costs=costs.Append(Entropy(paletteHash)+colors.Count*8L*Scale).ToArray();
        var mode=0;for(var k=1;k<costs.Length;k++)if(costs[k]<costs[mode])mode=k;
        var r=mode==0||mode==4?raw[1]:mode==1?residual[1]:mode==2?greenRaw[0]:greenResidual[0];var b=mode==0||mode==4?raw[3]:mode==1?residual[3]:mode==2?greenRaw[1]:greenResidual[1];
        return new(mode,!Enumerable.Range(1,255).Any(i=>r[i]!=0||b[i]!=0),costs,colors.Count,histogramBits,transformBits);
    }
}
