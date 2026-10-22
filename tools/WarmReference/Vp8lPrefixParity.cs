#nullable enable
using System;
using System.Linq;
using System.Collections.Generic;
// Independent canonical-tree construction: mathematical run segmentation and
// a deterministic priority queue, with depth bounded by a positive-weight floor.
internal static class Vp8lPrefixParity
{
    private sealed record Node(long Weight,int Symbol,Node? First=null,Node? Second=null);
    private static int[] Smooth(int[] original)
    {
        var counts=original.ToArray();var end=counts.Length;while(end>0&&counts[end-1]==0)end--;
        if(end==0)return counts;
        var protectedRun=new bool[end];
        for(var start=0;start<end;)
        {
            var stop=start+1;while(stop<end&&counts[stop]==counts[start])stop++;
            if(stop-start>=(counts[start]==0?5:7))Array.Fill(protectedRun,true,start,stop-start);
            start=stop;
        }
        var span=0;long sum=0;var center=counts[0];
        for(var at=0;at<=end;at++)
        {
            var boundary=at==end||protectedRun[at]||at>0&&protectedRun[at-1]||Math.Abs(counts[at]-center)>=4;
            if(boundary)
            {
                if(span>=4||span>=3&&sum==0)
                {
                    var replacement=sum==0?0:Math.Max(1,(int)((sum+span/2)/span));
                    Array.Fill(counts,replacement,at-span,span);
                }
                span=0;sum=0;
                center=at<end-3?(counts[at]+counts[at+1]+counts[at+2]+counts[at+3]+2)/4:at<end?counts[at]:0;
            }
            span++;
            if(at<end){sum+=counts[at];if(span>=4)center=(int)((sum+span/2)/span);}
        }
        return counts;
    }
    internal static byte[] Lengths(int[] source,int maximum=15)
    {
        var weights=Smooth(source);var lengths=new byte[source.Length];
        if(!weights.Any(x=>x!=0))return lengths;
        for(long floor=1;;floor*=2)
        {
            var queue=new PriorityQueue<Node,(long Weight,int Kind,int Rank)>();var serial=0;
            for(var symbol=0;symbol<weights.Length;symbol++)if(weights[symbol]!=0)
            {
                var node=new Node(Math.Max(weights[symbol],floor),symbol);
                queue.Enqueue(node,(node.Weight,0,-symbol));
            }
            if(queue.Count==1){lengths[queue.Dequeue().Symbol]=1;return lengths;}
            while(queue.Count>1)
            {
                var first=queue.Dequeue();var second=queue.Dequeue();var parent=new Node(first.Weight+second.Weight,-1,first,second);
                queue.Enqueue(parent,(parent.Weight,1,serial++));
            }
            Array.Clear(lengths);var deepest=0;
            void Walk(Node node,int depth)
            {
                if(node.Symbol>=0){deepest=Math.Max(deepest,depth);if(depth<=maximum)lengths[node.Symbol]=(byte)depth;}
                else {Walk(node.First!,depth+1);Walk(node.Second!,depth+1);}
            }
            Walk(queue.Dequeue(),0);if(deepest<=maximum)return lengths;
        }
    }
}
