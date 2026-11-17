using System.Buffers.Binary;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
using System.Text;
using Lokad.Image.Internal;
using Lokad.Image.Tooling;

namespace Lokad.Image
{
    public sealed partial class TrueTypeFont
    {
        internal static bool TestGroups12(ReadOnlySpan<byte> bytes, int count, int glyphs) => TryValidateGroups12(bytes, count, glyphs);
        internal static void TestTranspose12(Vector256<int> a, Vector256<int> b, Vector256<int> c,
            out Vector256<int> first, out Vector256<int> last, out Vector256<int> glyph)
            => TransposeGroups12(a, b, c, out first, out last, out glyph);
    }
}

namespace Lokad.Image.Tests
{
    public class FontMappingGroupTests
    {
        [Fact]
        public void ActualTransposePreservesEveryRecordField()
        {
            if (!Avx2.IsSupported) return;
            TrueTypeFont.TestTranspose12(Vector256.Create(0,1,2,3,4,5,6,7), Vector256.Create(8,9,10,11,12,13,14,15),
                Vector256.Create(16,17,18,19,20,21,22,23), out var first, out var last, out var glyph);
            for (var i = 0; i < 8; i++)
            { Assert.Equal(3*i, first.GetElement(i)); Assert.Equal(3*i+1, last.GetElement(i)); Assert.Equal(3*i+2, glyph.GetElement(i)); }
        }

        [Fact]
        public void ActualEightRecordClassifierMatchesIndependentUInt64Oracle()
        {
            if (!Avx2.IsSupported) return;
            var random = new Random(1243); var records = new uint[24]; var encoded = new byte[97];
            for (var trial = 0; trial < 8192; trial++)
            {
                var glyphs = new[] {1,4,6253,65535}[trial % 4]; var previous = -1;
                for (var i = 0; i < 8; i++)
                {
                    var first = previous + 1 + random.Next(3); var last = first + random.Next(Math.Min(glyphs, 4));
                    records[i*3] = (uint)first; records[i*3+1] = (uint)last; records[i*3+2] = (uint)random.Next(glyphs-(last-first)); previous = last;
                }
                Check(records, glyphs, encoded);
                for (var lane = 0; lane < 8; lane++)
                {
                    (int Field,uint Value)[] changes = [(0,0x80000000),(0,uint.MaxValue),(1,0x80000000),(1,uint.MaxValue),
                        (1,0x110000),(2,0x80000000),(2,uint.MaxValue),(2,(uint)glyphs),(2,0x7fffffff),
                        (0,lane == 0 ? uint.MaxValue : records[lane*3-2])];
                    foreach (var (field,value) in changes)
                    {
                        var at = lane*3+field; var saved = records[at]; records[at] = value;
                        Check(records, glyphs, encoded); records[at] = saved;
                    }
                }
            }
            for (var i = 0; i < 8; i++) { records[i*3] = (uint)i; records[i*3+1] = (uint)i; records[i*3+2] = 0; }
            Check(records,1,encoded); records[21]=records[22]=0x10ffff; Check(records,1,encoded);
        }

        private static void Check(uint[] records, int glyphs, byte[] encoded)
        {
            for (var i = 0; i < records.Length; i++) BinaryPrimitives.WriteUInt32BigEndian(encoded.AsSpan(1+i*4),records[i]);
            long previous = -1; var expected = true;
            for (var i = 0; i < 8; i++)
            {
                ulong first = records[i*3], last = records[i*3+1], glyph = records[i*3+2];
                if (last < first || (long)first <= previous || last > 0x10ffff || glyph + last - first >= (ulong)glyphs) { expected=false; break; }
                previous=(long)last;
            }
            Assert.Equal(expected,TrueTypeFont.TestGroups12(encoded.AsSpan(1),8,glyphs));
        }

        [Theory]
        [InlineData(0)] [InlineData(1)] [InlineData(7)] [InlineData(8)] [InlineData(9)]
        [InlineData(15)] [InlineData(16)] [InlineData(17)] [InlineData(31)] [InlineData(32)] [InlineData(33)]
        public void RealConstructorPreservesBatchTailsOwnedMapsAndCancelablePath(int count)
        {
            var mapping = Mapping(count); var input = AuthoredTrueType.Build(tableOverrides:new Dictionary<string,byte[]>{{"cmap",mapping}});
            var original = input.ToArray(); using var root = new TestMemoryBudget(input.Length*2L+512);
            using var cancellation = new CancellationTokenSource(); var limits=new ImageLimits{MemoryBudget=root};
            using var first=TrueTypeFont.Load(input,limits); using var second=TrueTypeFont.Load(input,limits,cancellation.Token);
            Assert.Equal(first.StorageBytes+second.StorageBytes,root.Bytes); Assert.Equal(original,input);
            input.AsSpan().Clear();
            for (var i=0;i<count;i++)
            { Assert.Equal(2,first.GetGlyphIndex(new Rune(32+i*4))); Assert.Equal(2,second.GetGlyphIndex(new Rune(32+i*4))); }
            first.Dispose(); Assert.Equal(second.StorageBytes,root.Bytes); second.Dispose(); Assert.Equal(0,root.Bytes); Assert.False(root.IsDisposed);
        }

        [Theory]
        [InlineData(false)] [InlineData(true)]
        public void InvalidGroupsLimitsCancellationAndRefusalReleaseTheBorrowedRoot(bool cancelable)
        {
            var clean=AuthoredTrueType.Build(tableOverrides:new Dictionary<string,byte[]>{{"cmap",Mapping(17)}});
            using var cancellation=new CancellationTokenSource(); var token=cancelable?cancellation.Token:default;
            using var root=new TestMemoryBudget(clean.Length+256L); var limits=new ImageLimits{MemoryBudget=root};
            foreach (var lane in new[]{0,7,8,15,16})
            foreach (var field in new[]{0,1,2})
            foreach (var value in new uint[]{0x80000000,uint.MaxValue,0x110000})
            {
                var bad=clean.ToArray(); var cmap=AuthoredTrueType.Offset(bad,"cmap");
                AuthoredTrueType.W32(bad,cmap+28+lane*12+field*4,value); var saved=bad.ToArray();
                var failure=Assert.Throws<ImageDataException>(()=>TrueTypeFont.Load(bad,limits,token));
                Assert.Equal("Invalid Unicode mapping group.",failure.Message); Assert.Equal(saved,bad); Assert.Equal(0,root.Bytes);
            }
            using (var recovered=TrueTypeFont.Load(clean,limits,token))
            {
                Assert.Equal(recovered.StorageBytes,root.Bytes);
                var refused=Assert.Throws<ImageLimitException>(()=>TrueTypeFont.Load(clean,limits,token));
                Assert.Equal(ImageLimitScope.SharedBudget,refused.Scope); Assert.Equal(recovered.StorageBytes,root.Bytes);
            }
            Assert.Equal(0,root.Bytes); Assert.False(root.IsDisposed);
            var header=clean.ToArray(); var table=AuthoredTrueType.Offset(header,"cmap");
            AuthoredTrueType.W32(header,table+24,100001); AuthoredTrueType.W16(header,table+14,1);
            var cap=Assert.Throws<ImageLimitException>(()=>TrueTypeFont.Load(header,limits,token)); Assert.Equal(ImageLimitKind.ContainerItems,cap.Limit); Assert.Equal(0,root.Bytes);
            using var canceled=new CancellationTokenSource();root.Admitted=_=>canceled.Cancel();
            Assert.ThrowsAny<OperationCanceledException>(()=>TrueTypeFont.Load(clean,limits,canceled.Token));Assert.Equal(0,root.Bytes);
            root.Admitted=null;using(var recovered=TrueTypeFont.Load(clean,limits))Assert.Equal(8,recovered.GlyphCount);Assert.Equal(0,root.Bytes);
        }

        private static byte[] Mapping(int count)
        {
            var result=new byte[28+count*12]; AuthoredTrueType.W16(result,2,1); AuthoredTrueType.W16(result,4,3); AuthoredTrueType.W16(result,6,10);
            AuthoredTrueType.W32(result,8,12); AuthoredTrueType.W16(result,12,12);AuthoredTrueType.W32(result,16,(uint)(16+count*12));AuthoredTrueType.W32(result,24,(uint)count);
            for(var i=0;i<count;i++){AuthoredTrueType.W32(result,28+i*12,(uint)(32+i*4));AuthoredTrueType.W32(result,32+i*12,(uint)(32+i*4));AuthoredTrueType.W32(result,36+i*12,2);}
            return result;
        }
    }
}
