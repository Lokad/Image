using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics.X86;

namespace Lokad.Image.Tests
{
    public sealed class CachedWidePreparedGeometryTests
    {
        [Theory]
        [InlineData(513,128,32)] [InlineData(512,128,32)] [InlineData(517,127,32)]
        [InlineData(513,64,64)] [InlineData(513,32,128)] [InlineData(513,16,256)]
        [InlineData(33,3,64)]
        public void ClampedGeometryPreservesEveryOrderedDouble(int sourceWidth,int width,int capacity)
            => Internal.CachedBicubicResize.VerifyPreparedWideWindows(sourceWidth,width,capacity,-1,-1);

        [Theory]
        [InlineData(16,16)] [InlineData(16,17)] [InlineData(32,32)] [InlineData(1,0)]
        public void FullUnequalAndZeroPaddingPreserveEveryOrderedDouble(int firstCount,int secondCount)
            => Internal.CachedBicubicResize.VerifyPreparedWideWindows(96,3,32,firstCount,secondCount);

        [Fact]
        public void FullCapacityClampedOffsetsRemainOnTheIndexedPath()
            => Internal.CachedBicubicResize.VerifyPreparedWideWindows(24,3,32,32,32);

        [Fact]
        public void CanceledWindowFilteringLeavesBorrowedStorageUntouched()
            => Internal.CachedBicubicResize.VerifyPreparedWideWindowCancellation();

        [Fact]
        public void OnlyDeadPaddingCarriesProofWithCountsInTheSameOwner()
            => Internal.CachedBicubicResize.VerifyPreparedWideMetadata();

        [Theory] [InlineData(-1,1)] [InlineData(7,0)] [InlineData(int.MaxValue,0)]
        public void InvalidSourceWindowsCannotAcquireAProof(int first,int second)
            => Internal.CachedBicubicResize.VerifyPreparedWideInvalid(first,second);

        [Fact]
        public void PreparationAndExecutionUseNoHeapOrSharedAdmission()
            => Internal.CachedBicubicResize.VerifyPreparedWideAllocation();

        [Fact]
        public void CanceledPreparationDoesNotWriteGeometry()
            => Internal.CachedBicubicResize.VerifyPreparedWidePreparationCancellation();

    }
}

namespace Lokad.Image.Internal
{
    internal static partial class CachedBicubicResize
    {
        internal static void VerifyPreparedWideWindows(int sourceWidth,int width,int capacity,int firstCount,int secondCount)
        {
            if(!Avx.IsSupported)return;
            var weights=new double[width*capacity];var offsets=new int[width*capacity];var counts=new int[width];
            var scale=Math.Max(1,(double)sourceWidth/width);
            for(var x=0;x<width;x++)
            {
                var center=(x+.5)*sourceWidth/width-.5;
                var first=(int)Math.Ceiling(center-2*scale);
                counts[x]=(int)Math.Floor(center+2*scale)-first+1;
                if(firstCount>=0)
                {
                    first=x==0?12:x==1?24:94;
                    counts[x]=x==0?firstCount:x==1?secondCount:3;
                }
                Assert.InRange(counts[x],0,capacity);
                for(var t=0;t<counts[x];t++)
                {
                    offsets[x*capacity+t]=Math.Clamp(first+t,0,sourceWidth-1);
                    weights[x*capacity+t]=firstCount<0?ImageTransforms.Kernel((first+t-center)/scale,ResizeFilter.Bicubic)
                        :t%2==0?(t+1)*.1:-(t+1)*.0625;
                }
            }
            var originalOffsets=offsets.ToArray();var originalWeights=weights.ToArray();var originalCounts=counts.ToArray();
            using var root=new Tooling.TestMemoryBudget(0);
            using var context=new OperationContext(new(){MemoryBudget=root},default);
            PrepareWideHorizontalPairs(offsets,counts,width,sourceWidth,capacity,context);
            var preparedOffsets=offsets.ToArray();
            for(var x=0;x<width;x++)
                Assert.Equal(originalOffsets.AsSpan(x*capacity,counts[x]).ToArray(),offsets.AsSpan(x*capacity,counts[x]).ToArray());
            foreach(var maximum in new[]{255,65535})foreach(var transparent in new[]{false,true})
            {
                var random=new Random(700);var decoded=new double[sourceWidth*4];
                for(var x=0;x<sourceWidth;x++)
                {
                    var alpha=transparent?random.Next(3):random.Next(maximum+1);
                    for(var c=0;c<4;c++)decoded[x*4+c]=c==3?alpha:(double)random.Next(maximum+1)*alpha;
                }
                var input=decoded.ToArray();var result=new double[width*4+16];Array.Fill(result,double.NaN);
                FilterWideHorizontal(decoded,result.AsSpan(8,width*4),weights,offsets,counts,width,capacity,context);
                for(var x=0;x<width;x++)for(var c=0;c<4;c++)
                {
                    var n=x==width-1 && (width&1)!=0?counts[x]:Math.Max(counts[x&~1],counts[(x&~1)+1]);
                    double expected=0;
                    for(var t=0;t<n;t++)expected+=WideOracleProduct(decoded[originalOffsets[x*capacity+t]*4+c],weights[x*capacity+t]);
                    Assert.Equal(BitConverter.DoubleToInt64Bits(expected),BitConverter.DoubleToInt64Bits(result[8+x*4+c]));
                }
                Assert.All(result.Take(8).Concat(result.Skip(width*4+8)),v=>Assert.True(double.IsNaN(v)));
                Assert.Equal(input,decoded);Assert.Equal(preparedOffsets,offsets);
                Assert.Equal(originalWeights,weights);Assert.Equal(originalCounts,counts);
                Assert.Equal(0,root.Bytes);Assert.False(root.IsDisposed);
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static double WideOracleProduct(double a,double b)=>a*b;

        internal static void VerifyPreparedWideWindowCancellation()
        {
            if(!Avx.IsSupported)return;
            using var cancellation=new CancellationTokenSource();
            using var root=new Tooling.TestMemoryBudget(0);
            using var context=new OperationContext(new(){MemoryBudget=root},cancellation.Token);
            cancellation.Cancel();
            var input=new double[128];var output=Enumerable.Repeat(double.NaN,12).ToArray();
            var error=Assert.Throws<OperationCanceledException>(()=>FilterWideHorizontal(input,output,new double[96],new int[96],new[]{16,16,3},3,32,context));
            Assert.Equal(cancellation.Token,error.CancellationToken);Assert.All(output,v=>Assert.True(double.IsNaN(v)));
            Assert.All(input,v=>Assert.Equal(0d,v));Assert.Equal(0,root.Bytes);Assert.False(root.IsDisposed);
        }

        internal static void VerifyPreparedWideMetadata()
        {
            const int width=9,capacity=8,sourceWidth=24;
            var geometry=new int[width*(capacity+1)];
            var offsets=geometry.AsSpan(0,width*capacity);var counts=geometry.AsSpan(width*capacity,width);
            new[]{3,3,8,8,4,3,2,2,3}.AsSpan().CopyTo(counts);
            for(var x=0;x<width;x++)for(var t=0;t<counts[x];t++)offsets[x*capacity+t]=x*2+t;
            offsets[7*capacity+1]=17; // A discontinuity in an otherwise eligible pair.
            var before=geometry.ToArray();
            using var root=new Tooling.TestMemoryBudget(0);
            using var context=new OperationContext(new(){MemoryBudget=root},default);
            PrepareWideHorizontalPairs(offsets,counts,width,sourceWidth,capacity,context);
            for(var i=0;i<geometry.Length;i++)Assert.Equal(i==capacity-1?-1:before[i],geometry[i]);
            var prepared=geometry.ToArray();
            PrepareWideHorizontalPairs(offsets,counts,width,sourceWidth,capacity,context);
            Assert.Equal(prepared,geometry);Assert.Equal(0,root.Attempts);Assert.Equal(0,root.Bytes);Assert.False(root.IsDisposed);
        }

        internal static void VerifyPreparedWideInvalid(int first,int second)
        {
            var offsets=new int[16];var counts=new[]{2,2};
            offsets[0]=first;offsets[1]=first==int.MaxValue?int.MaxValue:first+1;
            offsets[8]=second;offsets[9]=second+1;
            var before=offsets.ToArray();
            using var root=new Tooling.TestMemoryBudget(0);
            using var context=new OperationContext(new(){MemoryBudget=root},default);
            PrepareWideHorizontalPairs(offsets,counts,2,8,8,context);
            Assert.Equal(before,offsets);Assert.Equal(new[]{2,2},counts);
            Assert.Equal(0,root.Attempts);Assert.Equal(0,root.Bytes);Assert.False(root.IsDisposed);
        }

        internal static void VerifyPreparedWideAllocation()
        {
            const int width=4,capacity=8,sourceWidth=32;
            var offsets=new int[width*capacity];var weights=new double[offsets.Length];var counts=new[]{2,2,2,2};
            for(var x=0;x<width;x++)for(var t=0;t<2;t++){offsets[x*capacity+t]=x+t;weights[x*capacity+t]=.5;}
            var decoded=Enumerable.Repeat(123d,sourceWidth*4).ToArray();var result=new double[width*4];
            using var root=new Tooling.TestMemoryBudget(0);
            using var context=new OperationContext(new(){MemoryBudget=root},default);
            PrepareWideHorizontalPairs(offsets,counts,width,sourceWidth,capacity,context);
            if(Avx.IsSupported)FilterWideHorizontal(decoded,result,weights,offsets,counts,width,capacity,context);
            var before=GC.GetAllocatedBytesForCurrentThread();
            for(var i=0;i<128;i++)
            {
                PrepareWideHorizontalPairs(offsets,counts,width,sourceWidth,capacity,context);
                if(Avx.IsSupported)FilterWideHorizontal(decoded,result,weights,offsets,counts,width,capacity,context);
            }
            Assert.Equal(before,GC.GetAllocatedBytesForCurrentThread());
            if(Avx.IsSupported)Assert.All(result,value=>Assert.Equal(123d,value));
            Assert.Equal(0,root.Attempts);Assert.Equal(0,root.Bytes);Assert.False(root.IsDisposed);
        }

        internal static void VerifyPreparedWidePreparationCancellation()
        {
            var offsets=new int[24];var counts=new[]{2,2,2};
            for(var x=0;x<3;x++)for(var t=0;t<2;t++)offsets[x*8+t]=x+t;
            var before=offsets.ToArray();
            using var cancellation=new CancellationTokenSource();using var root=new Tooling.TestMemoryBudget(0);
            using var context=new OperationContext(new(){MemoryBudget=root},cancellation.Token);
            cancellation.Cancel();
            var error=Assert.Throws<OperationCanceledException>(()=>PrepareWideHorizontalPairs(offsets,counts,3,8,8,context));
            Assert.Equal(cancellation.Token,error.CancellationToken);Assert.Equal(before,offsets);
            Assert.Equal(new[]{2,2,2},counts);Assert.Equal(0,root.Bytes);Assert.False(root.IsDisposed);
        }
    }
}
