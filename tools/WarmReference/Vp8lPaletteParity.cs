// Source-only palette ordering, channel deltas and packed indices.
// Palette syntax and sample arithmetic: RFC 9649, section 4.
// Named palette/index controls qualify numeric and greedy ordering separately.
#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
public sealed record Vp8lPaletteResult(uint[] Colors,uint[] Deltas,uint[] Packed,int PackingBits,int CodedWidth);
public static class Vp8lPaletteParity
{
    private static uint Difference(uint a,uint b)
    {
        uint result=0;for(var shift=0;shift<32;shift+=8)result|=((a>>shift)-(b>>shift)&255)<<shift;return result;
    }
    private static int Distance(uint a,uint b)
    {
        var diff=Difference(a,b);var score=0;
        for(var shift=0;shift<32;shift+=8){var value=(int)(diff>>shift&255);score+=(shift==24?1:9)*Math.Min(value,256-value);}
        return score;
    }
    public static Vp8lPaletteResult Build(byte[] rgba,int width,int height,bool minimizeDeltas=true)
    {
        if(width<1||height<1||rgba.Length!=checked(width*height*4))throw new ArgumentException("Expected RGBA8 dimensions.");
        uint Pixel(int at)=>(uint)rgba[4*at+3]<<24|(uint)rgba[4*at]<<16|(uint)rgba[4*at+1]<<8|rgba[4*at+2];
        var unique=new HashSet<uint>();for(var at=0;at<width*height;at++){unique.Add(Pixel(at));if(unique.Count>256)throw new ArgumentException("Palette exceeds256 colors.");}
        var colors=unique.OrderBy(x=>x).ToArray();var positive=new bool[3];var negative=new bool[3];uint previous=0;
        foreach(var color in colors)
        {
            var delta=Difference(color,previous);previous=color;
            for(var c=0;c<3;c++){var value=delta>>(8*c)&255;if(value==0)continue;if(value<128)positive[c]=true;else negative[c]=true;}
        }
        if(minimizeDeltas&&Enumerable.Range(0,3).Any(c=>positive[c]&&negative[c]))
        {
            var active=colors.Length;if(active>17&&colors[0]==0){active--; (colors[0],colors[active])=(colors[active],colors[0]);}
            previous=0;
            for(var at=0;at<active;at++)
            {
                var winner=at;var best=int.MaxValue;
                for(var trial=at;trial<active;trial++){var cost=Distance(colors[trial],previous);if(cost<best){best=cost;winner=trial;}}
                (colors[at],colors[winner])=(colors[winner],colors[at]);previous=colors[at];
            }
        }
        if(!minimizeDeltas&&colors.Length>17&&colors[0]==0)
        {
            // Keep numeric order for the stored entries; the final zero uses
            // the RFC's implicit palette padding and is not serialized.
            Array.Copy(colors,1,colors,0,colors.Length-1);colors[^1]=0;
        }
        var deltas=new uint[colors.Length];previous=0;for(var at=0;at<colors.Length;at++){deltas[at]=Difference(colors[at],previous);previous=colors[at];}
        var bits=colors.Length<=2?3:colors.Length<=4?2:colors.Length<=16?1:0;var columns=(width+(1<<bits)-1)>>bits;var packed=new uint[columns*height];
        var indices=colors.Select((color,index)=>(color,index)).ToDictionary(x=>x.color,x=>x.index);
        for(var y=0;y<height;y++)for(var block=0;block<columns;block++)
        {
            uint value=0;
            for(var slot=0;slot<(1<<bits)&&block*(1<<bits)+slot<width;slot++)value|=(uint)indices[Pixel(y*width+block*(1<<bits)+slot)]<<(slot*(8>>bits));
            packed[y*columns+block]=0xff000000u|value<<8;
        }
        return new(colors,deltas,packed,bits,columns);
    }
}
