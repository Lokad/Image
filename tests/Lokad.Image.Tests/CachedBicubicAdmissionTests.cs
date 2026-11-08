using System.Runtime.Intrinsics.X86;
using Lokad.Image.Internal;
using Lokad.Image.Tooling;

namespace Lokad.Image.Tests;

public sealed class CachedBicubicAdmissionTests
{
    [Theory]
    [InlineData(513,256)]
    [InlineData(517,257)]
    [InlineData(65,32)]
    [InlineData(67,33)]
    [InlineData(33,16)]
    [InlineData(513,255)]
    [InlineData(33,17)]
    [InlineData(257,513)]
    public void PreparedHorizontalWindowsPreserveEveryOrderedDouble(int sourceWidth,int width)
    {
        if(!Avx512F.IsSupported || !Avx512DQ.IsSupported || !Avx2.IsSupported || !Sse41.IsSupported)return;
        var scale=Math.Max(1,(double)sourceWidth/width);
        var weights=new double[width*16];var offsets=new int[width*16];var counts=new int[width];
        for(var x=0;x<width;x++)
        {
            var center=(x+0.5)*sourceWidth/width-0.5;var first=(int)Math.Ceiling(center-2*scale);
            counts[x]=(int)Math.Floor(center+2*scale)-first+1;
            for(var tap=0;tap<counts[x];tap++)
            {
                weights[x*16+tap]=ImageTransforms.Kernel((first+tap-center)/scale,ResizeFilter.Bicubic);
                offsets[x*16+tap]=Math.Clamp(first+tap,0,sourceWidth-1)*4;
            }
        }
        var original=offsets.ToArray();using var context=new OperationContext(new ImageLimits(),default);
        CachedBicubicResize.PrepareOverlappingHorizontalPairs(offsets,counts,width,sourceWidth,context);
        var prepared=offsets.ToArray();
        if(sourceWidth==513 && width==256)Assert.Equal(124,Enumerable.Range(0,width/2).Count(p=>offsets[p*32+15]!=0));
        foreach(var maximum in new[]{255,65535})
        {
            var random=new Random(611);var decoded=new double[sourceWidth*4];var result=new double[width*4];
            for(var trial=0;trial<4;trial++)
            {
                for(var p=0;p<sourceWidth;p++)
                {
                    var alpha=trial==0?maximum:random.Next(maximum+1);
                    for(var c=0;c<4;c++)decoded[p*4+c]=c==3?alpha:(double)random.Next(maximum+1)*alpha;
                }
                result.AsSpan().Fill(double.NaN);
                CachedBicubicResize.FilterHorizontal(decoded,result,weights,offsets,counts,width,16,false,false,context);
                for(var x=0;x<width;x++)for(var c=0;c<4;c++)
                {
                    double expected=0;
                    var count=x==width-1 && (width&1)!=0?counts[x]:Math.Max(counts[x&~1],counts[(x&~1)+1]);
                    for(var tap=0;tap<count;tap++)expected+=decoded[original[x*16+tap]+c]*weights[x*16+tap];
                    Assert.Equal(BitConverter.DoubleToInt64Bits(expected),BitConverter.DoubleToInt64Bits(result[x*4+c]));
                }
                Assert.Equal(prepared,offsets);
            }
        }
    }

    // Frozen pre-consolidation charges: small byte/word, odd mixed padding,
    // wide stack metadata and wide heap metadata. No output/source ownership
    // is transferred to the borrowed working context.
    [Theory]
    [InlineData(8, 33, 17, 16, 8, 12640)]
    [InlineData(16, 17, 9, 35, 19, 25732)]
    [InlineData(8, 17, 19, 35, 9, 26244)]
    [InlineData(8, 33, 17, 2, 2, 13368)]
    [InlineData(16, 257, 17, 3, 2, 84036)]
    public void CachedStorageKeepsFrozenAdmissionRefusalCancellationAndRecovery(
        int depth, int sourceWidth, int sourceHeight, int width, int height, int charge)
    {
        if (!Avx512F.IsSupported || !Avx512DQ.IsSupported || !Avx2.IsSupported || !Sse41.IsSupported) return;
        using var source = OriginalPixels.Buffer(sourceWidth, sourceHeight, depth);
        for (var y = 0; y < sourceHeight; y++) for (var x = 0; x < sourceWidth; x++)
            if (depth == 8) source.GetRow8(y)[x * 4 + 3] = 255;
            else source.GetRow16(y)[x * 4 + 3] = 65535;
        var input = OriginalPixels.Bytes(source);
        using var direct = ImageTransforms.Resize(source, width, height, ResizeFilter.Bicubic,
            new ImageLimits { MaxWorkingBytes = input.Length + (long)width * height * 4 * (depth / 8) + 4095 });
        var expected = OriginalPixels.Bytes(direct);
        using var result = ImageBuffer.Create(width, height, depth);
        var scaleX = Math.Max(1, (double)sourceWidth / width); var scaleY = Math.Max(1, (double)sourceHeight / height);
        var tapsX = (long)Math.Ceiling(4 * scaleX) + 1; var tapsY = (long)Math.Ceiling(4 * scaleY) + 1;
        using var root = new TestMemoryBudget(charge);
        var limits = new ImageLimits { MaxWorkingBytes = charge + 4096L, MemoryBudget = root };
        using (var context = new OperationContext(limits, default))
        {
            Assert.True(CachedBicubicResize.TryApply(source, result, scaleX, scaleY, tapsX, tapsY, context));
            Assert.Equal(charge + 4096L, context.RemainingWorkingBytes);
        }
        Assert.Equal(expected, OriginalPixels.Bytes(result)); Assert.Equal(input, OriginalPixels.Bytes(source));
        Assert.Equal(charge, root.Peak); Assert.Equal(0, root.Bytes); Assert.Equal(1, root.GrowthAttempts);

        using var refused = new TestMemoryBudget(charge - 1);
        using (var context = new OperationContext(limits with { MemoryBudget = refused }, default))
        {
            var error = Assert.Throws<ImageLimitException>(() => CachedBicubicResize.TryApply(source, result, scaleX, scaleY, tapsX, tapsY, context));
            Assert.Equal((ImageLimitKind.WorkingBytes, ImageLimitScope.SharedBudget), (error.Limit, error.Scope));
        }
        Assert.Equal(1, refused.Refusals); Assert.Equal(1, refused.GrowthAttempts); Assert.Equal(0, refused.Bytes);
        using var cancellation = new CancellationTokenSource(); root.Admitted = _ => cancellation.Cancel();
        for (var y = 0; y < height; y++)
            if (depth == 8) result.GetRow8(y).Fill(7); else result.GetRow16(y).Fill(7);
        var untouched = OriginalPixels.Bytes(result);
        using (var context = new OperationContext(limits, cancellation.Token))
            Assert.Throws<OperationCanceledException>(() => CachedBicubicResize.TryApply(source, result, scaleX, scaleY, tapsX, tapsY, context));
        Assert.Equal(untouched, OriginalPixels.Bytes(result)); Assert.Equal(0, root.Bytes); Assert.Equal(0, root.Refusals);
        root.Admitted = null;
        using (var context = new OperationContext(limits, default))
            Assert.True(CachedBicubicResize.TryApply(source, result, scaleX, scaleY, tapsX, tapsY, context));
        Assert.Equal(expected, OriginalPixels.Bytes(result)); Assert.Equal(input, OriginalPixels.Bytes(source));
        Assert.Equal(0, root.Bytes); Assert.False(root.IsDisposed); Assert.False(refused.IsDisposed);
    }
}
