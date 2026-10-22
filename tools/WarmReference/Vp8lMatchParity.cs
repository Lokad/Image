#nullable enable
using System;
using System.Linq;
using System.Collections.Generic;
// Independent source-only bounded candidate index. Dense fingerprints are
// generated with scalar unsigned arithmetic; no imported hash/match tables.
internal static class Vp8lMatchParity
{
    private const int MaximumLength = (1 << 12) - 1;
    private static int Fingerprint(uint first, uint next)
        => (int)(unchecked(next * 0xc6a4a793u + first * 0x5bd1e996u) >> 14);
    internal static uint[] Build(uint[] pixels, int width, int quality)=>BuildCore(pixels,width,quality,false);
    internal static uint[] BuildLowEffort(uint[] pixels, int width, int quality)=>BuildCore(pixels,width,quality,true);
    private static uint[] BuildCore(uint[] pixels, int width, int quality, bool lowEffort)
    {
        var result = new uint[pixels.Length];
        if (pixels.Length <= 2) return result;
        var heads = new int[1 << 18]; Array.Fill(heads,-1);
        var previous = new int[pixels.Length]; Array.Fill(previous,-1);
        var position = 0; var repeated = pixels[0] == pixels[1];
        void Insert(int key, int at) { previous[at] = heads[key]; heads[key] = at; }
        while (position < pixels.Length - 2)
        {
            var nextRepeat = pixels[position+1] == pixels[position+2];
            if (repeated && nextRepeat)
            {
                var run = 1;
                while (position+run+2 < pixels.Length && pixels[position+run+2] == pixels[position]) run++;
                if (run > MaximumLength) { position += run-MaximumLength; run = MaximumLength; }
                while (run > 0)
                {
                    var key = Fingerprint(pixels[position],(uint)run--);
                    Insert(key,position++);
                }
                repeated = false;
            }
            else
            {
                var key = Fingerprint(pixels[position],pixels[position+1]); Insert(key,position++); repeated=nextRepeat;
            }
        }
        previous[position]=heads[Fingerprint(pixels[position],pixels[position+1])];
        var window=Math.Min((1<<20)-120,quality>75 ? (1<<20)-120 : width*(quality>50 ? 256 : quality>25 ? 64 : 16));
        var checks=8+quality*quality/128;
        int Length(int first,int second,int already,int limit)
        {
            if(pixels[first+already]!=pixels[second+already])return 0;
            var n=0;while(n<limit&&pixels[first+n]==pixels[second+n])n++;
            return n;
        }
        position=pixels.Length-2;
        while(position>0)
        {
            var limit=Math.Min(MaximumLength,pixels.Length-1-position);var enough=Math.Min(256,limit);
            var minimum=Math.Max(0,position-window);var candidate=previous[position];var remaining=checks;
            var length=0;var distance=0;
            if(!lowEffort)
            {
                if(position>=width)
                {
                    var found=Length(position-width,position,length,limit);
                    if(found>length){length=found;distance=width;}
                    remaining--;
                }
                var left=Length(position-1,position,length,limit);
                if(left>length){length=left;distance=1;}
                remaining--;
                if(length==MaximumLength)candidate=minimum-1;
            }
            while(candidate>=minimum && --remaining!=0)
            {
                if(pixels[candidate+length]==pixels[position+length])
                {
                    var found=Length(candidate,position,0,limit);
                    if(found>length)
                    {
                        length=found;distance=position-candidate;
                        if(length>=enough)break;
                    }
                }
                candidate=previous[candidate];
            }
            var extensionOrigin=position;
            while(true)
            {
                result[position]=(uint)(distance<<12|length);position--;
                if(distance==0||position==0||position<distance||pixels[position-distance]!=pixels[position])break;
                if(length==MaximumLength && distance!=1 && position+MaximumLength<extensionOrigin)break;
                if(length<MaximumLength){length++;extensionOrigin=position;}
            }
        }
        return result;
    }
}
