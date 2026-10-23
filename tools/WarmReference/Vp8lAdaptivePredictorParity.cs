// Independent source-only hierarchical predictor selection. RFC 9649 defines
// sample prediction; per-level histories and mathematical entropy select tiles.
// This is not an admitted production encoder.
using System;
using System.Linq;

public sealed record Vp8lAdaptivePredictorResult(byte[] Modes,int SelectedBits,int StoredBits,
    long[] EntropyCosts,byte[][] CandidateModes,byte[] Residual);
public static class Vp8lAdaptivePredictorParity
{
    private const long Scale=1L<<23;
    public static Vp8lAdaptivePredictorResult Select(byte[] rgba,int width,int height,int minBits,int maxBits,bool subtractGreen=true)
    {
        if(width<1||height<1||rgba.Length!=checked(width*height*4))throw new ArgumentException("Expected RGBA8 dimensions.");
        if(minBits<2||minBits>maxBits||maxBits>9)throw new ArgumentOutOfRangeException(nameof(minBits));
        var pixels=new uint[width*height];
        for(var at=0;at<pixels.Length;at++)
        {
            var green=rgba[at*4+1];var adjustment=subtractGreen?green:0;
            pixels[at]=(uint)rgba[at*4+3]<<24|(uint)((rgba[at*4]-adjustment)&255)<<16
                |(uint)green<<8|(uint)((rgba[at*4+2]-adjustment)&255);
        }
        var levels=maxBits-minBits+1;
        var candidates=new byte[levels][];var histograms=new int[levels][];
        var histories=new int[levels][];var frequencies=new int[levels][];
        for(var level=0;level<levels;level++)
        {
            var bits=minBits+level;
            candidates[level]=new byte[((width+(1<<bits)-1)>>bits)*((height+(1<<bits)-1)>>bits)];
            histograms[level]=new int[14*1024];histories[level]=new int[1024];frequencies[level]=new int[14];
        }
        // Complete each square's children in quadrant order before its parent.
        // Only one reusable histogram per level is live; source samples are read-only.
        void Visit(int level,int x0,int y0)
        {
            if(x0>=width||y0>=height)return;
            var bits=minBits+level;var histogram=histograms[level];Array.Clear(histogram);
            if(level==0)
            {
                for(var y=y0;y<Math.Min(y0+(1<<bits),height);y++)
                for(var x=x0;x<Math.Min(x0+(1<<bits),width);x++)
                {
                    var at=y*width+x;var actual=pixels[at];
                    for(var mode=0;mode<14;mode++)
                    {
                        var prediction=y==0?x==0?0xff000000u:pixels[at-1]
                            :x==0?pixels[at-width]:Vp8lPredictorParity.Predict(mode,pixels[at-1],pixels[at-width],pixels[at-width-1],pixels[x==width-1?y*width:at-width+1]);
                        for(var plane=0;plane<4;plane++)
                        {
                            var shift=(3-plane)*8;
                            histogram[mode*1024+plane*256+(int)(((actual>>shift)-(prediction>>shift))&255)]++;
                        }
                    }
                }
            }
            else
            {
                var half=1<<(bits-1);
                for(var child=0;child<4;child++)
                {
                    var childX=x0+(child&1)*half;var childY=y0+(child>>1)*half;
                    if(childX>=width||childY>=height)continue;
                    Visit(level-1,childX,childY);
                    var lower=histograms[level-1];for(var i=0;i<histogram.Length;i++)histogram[i]+=lower[i];
                }
            }
            var columns=(width+(1<<bits)-1)>>bits;var tx=x0>>bits;var ty=y0>>bits;var tile=ty*columns+tx;
            var modes=candidates[level];var accumulated=histories[level];var chosen=0;var best=long.MaxValue;
            for(var mode=0;mode<14;mode++)
            {
                var cost=Vp8lPredictorParity.Cost(histogram.AsSpan(mode*1024,1024),accumulated);
                if(tx!=0&&modes[tile-1]==mode)cost-=15*Scale;
                if(ty!=0&&modes[tile-columns]==mode)cost-=15*Scale;
                if(cost<best){best=cost;chosen=mode;}
            }
            modes[tile]=(byte)chosen;frequencies[level][chosen]++;
            for(var i=0;i<1024;i++)accumulated[i]+=histogram[chosen*1024+i];
        }
        for(var y=0;y<height;y+=1<<maxBits)
        for(var x=0;x<width;x+=1<<maxBits)Visit(levels-1,x,y);
        long Entropy(int[] counts)=>Vp8lPredictorParity.SLog(counts.Sum())-counts.Sum(Vp8lPredictorParity.SLog);
        var costs=new long[levels];var winner=0;
        for(var level=0;level<levels;level++)
        {
            costs[level]=Entropy(frequencies[level]);
            for(var plane=0;plane<4;plane++)costs[level]+=Entropy(histories[level].AsSpan(plane*256,256).ToArray());
            if(costs[level]<costs[winner])winner=level;
        }
        var selected=minBits+winner;var original=candidates[winner];var stored=selected;
        var oldColumns=(width+(1<<selected)-1)>>selected;var oldRows=(height+(1<<selected)-1)>>selected;
        for(var bits=selected+1;bits<=9;bits++)
        {
            var stride=1<<(bits-selected);var same=true;
            for(var y=0;same&&y<oldRows;y++)for(var x=0;x<oldColumns;x++)
                if(original[y*oldColumns+x]!=original[(y/stride*stride)*oldColumns+x/stride*stride]){same=false;break;}
            if(!same)break;stored=bits;
        }
        var step=1<<(stored-selected);var newColumns=(width+(1<<stored)-1)>>stored;var newRows=(height+(1<<stored)-1)>>stored;
        var result=new byte[newColumns*newRows];
        for(var y=0;y<newRows;y++)for(var x=0;x<newColumns;x++)result[y*newColumns+x]=original[y*step*oldColumns+x*step];
        return new(result,selected,stored,costs,candidates,Vp8lPredictorParity.Residual(rgba,width,height,stored,result,subtractGreen));
    }
}
