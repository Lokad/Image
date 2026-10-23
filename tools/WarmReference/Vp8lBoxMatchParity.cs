// Independent source-only short-distance match index. Window coordinates use
// the RFC 9649 distance geometry; no upstream implementation table is imported.
using System;
using System.Linq;

public static class Vp8lBoxMatchParity
{
    public static uint[] Build(uint[] pixels,int width)
        => Build(pixels,width,Vp8lMatchParity.Build(pixels,width,75));
    internal static uint[] Build(uint[] pixels,int width,uint[] ordinary)
    {
        if(width<1||pixels.Length%width!=0||ordinary.Length!=pixels.Length)throw new ArgumentException("Expected complete rows and a matching ordinary index.");
        var result=new uint[pixels.Length];if(pixels.Length<2)return result;
        var slots=new int[32];
        for(var y=0;y<=6;y++)for(var x=-6;x<=6;x++)
        {
            var distance=y*width+x;if(distance<=0)continue;
            var code=Vp8lLzParity.Plane(distance,width)-1;
            if(code>=0&&code<slots.Length)slots[code]=distance;
        }
        var offsets=slots.Where(d=>d!=0).ToArray();
        var entering=offsets.Where(d=>!offsets.Contains(d-1)).ToArray();
        var previousLength=0;var previousDistance=0;
        for(var at=1;at<pixels.Length;at++)
        {
            var length=(int)(ordinary[at]&4095);var distance=(int)(ordinary[at]>>12);
            if(length<4095||!offsets.Contains(distance))
            {
                var carry=previousLength>1&&previousLength<4095;
                length=carry?previousLength-1:0;distance=carry?previousDistance:0;
                foreach(var candidate in carry?entering:offsets)
                {
                    if(candidate>at)continue;
                    var n=0;var limit=Math.Min(4095,pixels.Length-at);
                    while(n<limit&&pixels[at+n]==pixels[at+n-candidate])n++;
                    if(n<=length)continue;length=n;distance=candidate;
                    if(n==4095)break;
                }
            }
            if(length>4){result[at]=(uint)(distance<<12|length);previousLength=length;previousDistance=distance;}
            else{previousLength=0;previousDistance=0;}
        }
        return result;
    }
}
