#nullable enable
using System;
using System.Linq;
using System.Collections.Generic;
// Independent source-only spatial clustering. Counts/math come from our frozen
// models. Algorithmic sampling has a fixed Lehmer seed; no experimental search.
internal static class Vp8lSpatialParity
{
    internal sealed record Result(int[] Map,int[][][] Counts,object Details);
    private sealed class Histogram
    {
        internal required int[][] Counts;
        internal required long[] Costs;
        internal required int[] Trivial;
        internal required bool[] Used;
        internal long Total=>Costs.Sum();
        internal int Bin;
    }
    private sealed class Pair
    {
        internal int A,B;
        internal required long[] Costs;
        internal long Total,Delta;
    }
    private sealed class Merger(Func<int[],long> population,int[] sizes)
    {
        private readonly int[][] scratch=sizes.Select(n=>new int[n]).ToArray();
        internal Histogram Initial(int[][] counts)
        {
            var trivial=new int[5];var used=new bool[5];
            for(var bank=0;bank<5;bank++)
            {
                var nonzero=Enumerable.Range(0,counts[bank].Length).Where(i=>counts[bank][i]!=0).ToArray();
                used[bank]=nonzero.Length!=0;trivial[bank]=nonzero.Length==1?nonzero[0]:-1;
            }
            return new(){Counts=counts,Costs=counts.Select(population).ToArray(),Trivial=trivial,Used=used};
        }
        private static long Limit(long baseline,long change)=>change>long.MaxValue-baseline?long.MaxValue:baseline+change;
        internal long[]? Evaluate(Histogram a,Histogram b,long maximum)
        {
            if(maximum<=0)return null;
            var costs=new long[5];long total=0;
            for(var bank=0;bank<5;bank++)
            {
                if(!a.Used[bank]||!b.Used[bank]||a.Trivial[bank]>=0&&a.Trivial[bank]==b.Trivial[bank])costs[bank]=a.Used[bank]?a.Costs[bank]:b.Costs[bank];
                else
                {
                    var sum=scratch[bank];for(var i=0;i<sum.Length;i++)sum[i]=a.Counts[bank][i]+b.Counts[bank][i];
                    costs[bank]=population(sum);
                }
                total+=costs[bank];if(total>=maximum)return null;
            }
            return costs;
        }
        internal Histogram Combine(Histogram a,Histogram b,long[] costs)
        {
            var counts=new int[5][];
            for(var bank=0;bank<5;bank++)
            {counts[bank]=new int[a.Counts[bank].Length];for(var i=0;i<counts[bank].Length;i++)counts[bank][i]=a.Counts[bank][i]+b.Counts[bank][i];}
            return new(){Counts=counts,Costs=costs,Trivial=Enumerable.Range(0,5).Select(i=>a.Trivial[i]==b.Trivial[i]?a.Trivial[i]:-1).ToArray(),Used=Enumerable.Range(0,5).Select(i=>a.Used[i]||b.Used[i]).ToArray(),Bin=a.Bin};
        }
        private static void Remove<T>(List<T> list,int index){list[index]=list[^1];list.RemoveAt(list.Count-1);}
        private static void Promote(List<Pair> pool,int index)
        {if(pool[index].Delta<pool[0].Delta)(pool[0],pool[index])=(pool[index],pool[0]);}
        private static void Rename(Pair pair,int oldIndex,int newIndex)
        {if(pair.A==oldIndex)pair.A=newIndex;if(pair.B==oldIndex)pair.B=newIndex;if(pair.A>pair.B)(pair.A,pair.B)=(pair.B,pair.A);}
        private bool Refresh(Pair pair,List<Histogram> histograms,long threshold)
        {
            var a=histograms[pair.A];var b=histograms[pair.B];var sum=a.Total+b.Total;
            var costs=Evaluate(a,b,Limit(sum,threshold));if(costs==null)return false;
            pair.Costs=costs;pair.Total=costs.Sum();pair.Delta=pair.Total-sum;return true;
        }
        private long Push(List<Pair> pool,List<Histogram> histograms,int a,int b,long threshold,int capacity)
        {
            if(pool.Count==capacity)return 0;
            if(a>b)(a,b)=(b,a);
            var pair=new Pair{A=a,B=b,Costs=[]};if(!Refresh(pair,histograms,threshold))return 0;
            pool.Add(pair);Promote(pool,pool.Count-1);return pair.Delta;
        }
        internal int MergeBins(List<Histogram> items)
        {
            var min=Enumerable.Range(0,3).Select(c=>items.Min(h=>h.Costs[c])).ToArray();
            var max=Enumerable.Range(0,3).Select(c=>items.Max(h=>h.Costs[c])).ToArray();
            foreach(var h in items)
            {
                h.Bin=0;
                for(var c=0;c<3;c++){var range=max[c]-min[c];h.Bin=h.Bin*4+(range==0?0:(int)((4-1e-6)*(h.Costs[c]-min[c])/range));}
            }
            var first=Enumerable.Repeat(-1,64).ToArray();var failures=new int[64];var merges=0;
            for(var at=0;at<items.Count;)
            {
                var candidate=items[at];var bin=candidate.Bin;
                if(first[bin]<0){first[bin]=at++;continue;}
                var original=items[first[bin]];var change=-((candidate.Total*2+50)/100);
                var costs=Evaluate(original,candidate,original.Total+candidate.Total+change);
                if(costs==null){at++;continue;}
                bool Nontrivial(Histogram h)=>Enumerable.Range(1,3).Any(c=>h.Trivial[c]<0);
                var combined=Combine(original,candidate,costs);
                if(!Nontrivial(combined)||Nontrivial(candidate)&&Nontrivial(original)||failures[bin]>=32)
                {items[first[bin]]=combined;Remove(items,at);merges++;}
                else{failures[bin]++;at++;}
            }
            return merges;
        }
        internal (bool Greedy,int Merges,uint Seed) Sample(List<Histogram> items)
        {
            const int threshold=43,capacity=9;
            if(items.Count<threshold)return (true,0,1);
            var pool=new List<Pair>();var outer=items.Count;var misses=0;var limit=outer/2;var merges=0;uint seed=1;
            for(var iteration=0;iteration<outer&&items.Count>=threshold&&++misses<limit;iteration++)
            {
                var best=pool.Count==0?0:pool[0].Delta;var range=(uint)((items.Count-1)*items.Count);var tries=items.Count/2;
                for(var trial=0;items.Count>=2&&trial<tries;trial++)
                {
                    seed=(uint)((ulong)seed*48271%2147483647);var value=seed%range;
                    var a=(int)(value/(items.Count-1));var b=(int)(value%(items.Count-1));if(b>=a)b++;
                    var delta=Push(pool,items,a,b,best,capacity);
                    if(delta<0){best=delta;if(pool.Count==capacity)break;}
                }
                if(pool.Count==0)continue;
                var chosen=pool[0];var first=chosen.A;var second=chosen.B;
                items[first]=Combine(items[first],items[second],chosen.Costs);Remove(items,second);merges++;
                for(var at=0;at<pool.Count;)
                {
                    var pair=pool[at];var a=pair.A==first||pair.A==second;var b=pair.B==first||pair.B==second;
                    if(a&&b){Remove(pool,at);continue;}
                    if(a||b){Rename(pair,second,first);if(!Refresh(pair,items,0)){Remove(pool,at);continue;}}
                    Rename(pair,items.Count,second);Promote(pool,at);at++;
                }
                misses=0;
            }
            return(items.Count<=threshold,merges,seed);
        }
        internal int Greedy(List<Histogram> items)
        {
            var capacity=items.Count*items.Count;var pool=new List<Pair>();var merges=0;
            for(var a=0;a<items.Count;a++)for(var b=a+1;b<items.Count;b++)Push(pool,items,a,b,0,capacity);
            while(pool.Count!=0)
            {
                var chosen=pool[0];var a=chosen.A;var b=chosen.B;
                items[a]=Combine(items[a],items[b],chosen.Costs);Remove(items,b);merges++;
                for(var at=0;at<pool.Count;)
                {
                    var p=pool[at];if(p.A==a||p.A==b||p.B==a||p.B==b){Remove(pool,at);continue;}
                    Rename(p,items.Count,b);Promote(pool,at);at++;
                }
                for(var other=0;other<items.Count;other++)if(other!=a)Push(pool,items,a,other,0,capacity);
            }
            return merges;
        }
        internal int[] Remap(Histogram[] original,List<Histogram> groups)
        {
            var map=new int[original.Length];
            for(var tile=0;tile<original.Length;tile++)
            {
                if(!original[tile].Used.Any(x=>x)){map[tile]=tile==0?0:map[tile-1];continue;}
                long best=long.MaxValue;
                for(var group=0;group<groups.Count;group++)
                {
                    var costs=Evaluate(groups[group],original[tile],Limit(groups[group].Total,best));
                    if(costs!=null){best=costs.Sum()-groups[group].Total;map[tile]=group;}
                }
            }
            return map;
        }
    }
    internal static Result Build(int[][][] counts,Func<int[],long> population)
    {
        var merger=new Merger(population,counts[0].Select(h=>h.Length).ToArray());var original=counts.Select(merger.Initial).ToArray();var items=original.Where(h=>h.Used.Any(x=>x)).ToList();
        var binMerges=items.Count>128?merger.MergeBins(items):0;var afterBins=items.Count;
        var sample=merger.Sample(items);var afterSample=items.Count;var greedyMerges=sample.Greedy?merger.Greedy(items):0;
        var map=merger.Remap(original,items);
        var result=Enumerable.Range(0,items.Count).Select(_=>counts[0].Select(h=>new int[h.Length]).ToArray()).ToArray();
        for(var tile=0;tile<map.Length;tile++)for(var bank=0;bank<5;bank++)for(var symbol=0;symbol<counts[tile][bank].Length;symbol++)result[map[tile]][bank][symbol]+=counts[tile][bank][symbol];
        return new(map,result,new{binMerges,afterBins,sampleMerges=sample.Merges,afterSample,greedyMerges,sampleSeed=sample.Seed});
    }
}
