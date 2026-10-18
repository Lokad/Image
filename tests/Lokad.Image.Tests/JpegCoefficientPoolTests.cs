using System.Collections.Concurrent;
using Lokad.Image.Internal;

namespace Lokad.Image.Tests;

public sealed class JpegCoefficientPoolTests
{
    [Fact]
    public void ThreeActivePlanesNeverShareAndEveryReusedSampleIsZero()
    {
        var pool=new JpegCoefficientPool(4096);
        var planes=Enumerable.Range(0,3).Select(_=>pool.Rent(128)).ToArray();
        for(var i=0;i<3;i++)
        {
            Array.Fill(planes[i],i+1);
            for(var j=0;j<i;j++)Assert.NotSame(planes[j],planes[i]);
        }
        foreach(var plane in planes)pool.Return(plane);
        var next=Enumerable.Range(0,3).Select(_=>pool.Rent(128)).ToArray();
        foreach(var plane in next)
        {
            Assert.Contains(planes,x=>ReferenceEquals(x,plane));Assert.All(plane,x=>Assert.Equal(0,x));
        }
        for(var i=0;i<3;i++)for(var j=0;j<i;j++)Assert.NotSame(next[i],next[j]);
        var fourth=pool.Rent(128);
        Assert.DoesNotContain(next,x=>ReferenceEquals(x,fourth));
    }

    [Fact]
    public void ExactLengthsCombinedCeilingShapeMissAndDuplicateReturnStayBounded()
    {
        var pool=new JpegCoefficientPool(32);
        var first=pool.Rent(4);var second=pool.Rent(4);var third=pool.Rent(4);
        pool.Return(first);pool.Return(second);pool.Return(third);pool.Return(first);
        Assert.Same(first,pool.Rent(4));Assert.Same(second,pool.Rent(4));Assert.NotSame(third,pool.Rent(4));
        pool.Return(first);var changed=pool.Rent(3);Assert.Equal(3,changed.Length);
        Assert.NotSame(first,pool.Rent(4));
        var large=pool.Rent(9);pool.Return(large);Assert.NotSame(large,pool.Rent(9));
        var disabled=new JpegCoefficientPool(0);var original=disabled.Rent(4);disabled.Return(original);
        Assert.NotSame(original,disabled.Rent(4));
    }

    [Fact]
    public void ConcurrentPrivateLeasesKeepActiveValuesAndZeroOnReuse()
    {
        var pool=new JpegCoefficientPool(4096);
        var active=new ConcurrentDictionary<int[],byte>(ReferenceEqualityComparer.Instance);
        Parallel.For(0,16,worker=>
        {
            for(var iteration=0;iteration<40;iteration++)
            {
                var storage=pool.Rent(97);Assert.True(active.TryAdd(storage,0));
                Assert.All(storage,value=>Assert.Equal(0,value));Array.Fill(storage,worker+1);
                Thread.Yield();Assert.All(storage,value=>Assert.Equal(worker+1,value));
                Assert.True(active.TryRemove(storage,out _));pool.Return(storage);
            }
        });
        Assert.Empty(active);
    }

}
