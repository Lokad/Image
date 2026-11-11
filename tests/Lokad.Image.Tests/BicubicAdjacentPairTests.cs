using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
using Lokad.Image.Internal;
using Lokad.Image.Tooling;

namespace Lokad.Image.Tests;

public sealed class BicubicAdjacentPairTests
{
    [Theory]
    [InlineData(255, false, false)] [InlineData(255, false, true)]
    [InlineData(255, true, false)] [InlineData(255, true, true)]
    [InlineData(65535, false, false)] [InlineData(65535, false, true)]
    [InlineData(65535, true, false)] [InlineData(65535, true, true)]
    public void CommonAndAdjacentWindowsKeepEveryOrderedDouble(int maximum, bool transparent, bool shifted)
    {
        if (!Avx512F.IsSupported) return;
        var random = new Random(819); var shift = shifted ? 1 : 0;
        var input = new Vector256<double>[6 + shift];
        var first = new[] { 97d, .1, -.125, .3, -.25, 83d };
        var second = new[] { 79d, -.01, .25, -.03, .5, 71d };
        for (var trial = 0; trial < 32; trial++)
        {
            for (var p = 0; p < input.Length; p++)
            {
                var a = transparent ? random.Next(3) : random.Next(maximum + 1);
                input[p] = Vector256.Create((double)a * random.Next(maximum + 1),
                    (double)a * random.Next(maximum + 1), (double)a * random.Next(maximum + 1), a);
            }
            var saved = input.ToArray(); var savedFirst = first.ToArray(); var savedSecond = second.ToArray();
            var pair = BicubicMath.FilterAdjacentHorizontalPair(input.AsSpan(1, 4 + shift),
                first.AsSpan(1, 4), second.AsSpan(1, 4), shifted);
            for (var c = 0; c < 8; c++)
            {
                double expected = 0;
                for (var t = 0; t < 4; t++)
                    expected += Product(input[1 + t + (c < 4 ? 0 : shift)].GetElement(c & 3),
                        (c < 4 ? first : second)[t + 1]);
                Assert.Equal(BitConverter.DoubleToInt64Bits(expected), BitConverter.DoubleToInt64Bits(pair.GetElement(c)));
            }
            Assert.Equal(saved, input); Assert.Equal(savedFirst, first); Assert.Equal(savedSecond, second);
        }
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static double Product(double a, double b) => a * b;

    [Theory]
    [InlineData(513, 1027, 255, 254)] [InlineData(31, 62, 0, 27)]
    [InlineData(9, 17, 2, 2)] [InlineData(7, 7, 0, 0)]
    public void EligibilityPreservesAllLiveOffsetsAndOddTail(int sourceWidth, int width, int common, int adjacent)
    {
        var counts = new int[width]; var storage = Enumerable.Repeat(-99, width * 5 + 2).ToArray();
        var offsets = storage.AsSpan(1, width * 5);
        for (var x = 0; x < width; x++)
        {
            var center = (x + .5) * sourceWidth / width - .5;
            var first = (int)Math.Ceiling(center - 2);
            counts[x] = (int)Math.Floor(center + 2) - first + 1;
            for (var t = 0; t < counts[x]; t++) offsets[x * 5 + t] = Math.Clamp(first + t, 0, sourceWidth - 1);
        }
        var original = offsets.ToArray();
        var paired = BicubicMath.PrepareAdjacentFourTapPairs(offsets, counts, sourceWidth, default);
        Assert.Equal(common + adjacent > 0, paired);
        var observedCommon = 0; var observedAdjacent = 0;
        for (var x = 0; x < width; x++)
        {
            Assert.Equal(original.AsSpan(x * 5, counts[x]).ToArray(), offsets.Slice(x * 5, counts[x]).ToArray());
            if (counts[x] == 5 || offsets[x * 5 + 4] == 0) continue;
            Assert.Equal(0, x & 1); Assert.True(x + 1 < width);
            Assert.Equal(4, counts[x]); Assert.Equal(4, counts[x + 1]);
            if (offsets[x * 5 + 4] == -1) observedCommon++;
            else { Assert.Equal(-2, offsets[x * 5 + 4]); observedAdjacent++; }
        }
        Assert.Equal(common, observedCommon); Assert.Equal(adjacent, observedAdjacent);
        Assert.Equal(-99, storage[0]); Assert.Equal(-99, storage[^1]);
    }

    [Theory]
    [InlineData(0)] [InlineData(1)] [InlineData(2)] [InlineData(3)] [InlineData(4)]
    public void InvalidOrFullWindowsKeepTheirOriginalOffsets(int kind)
    {
        var counts = kind == 1 ? new[] { 4, 5 } : kind == 2 ? new[] { 5, 5 } : new[] { 4, 4 };
        var offsets = new int[10]; var first = kind == 3 ? -1 : 2;
        for (var t = 0; t < 5; t++) { offsets[t] = first + t; offsets[5 + t] = first + 1 + t; }
        if (kind == 0) offsets[1] = offsets[0];
        var original = offsets.ToArray();
        Assert.False(BicubicMath.PrepareAdjacentFourTapPairs(offsets, counts, kind == 4 ? 6 : 20, default));
        for (var x = 0; x < 2; x++)
        {
            Assert.Equal(original.AsSpan(x * 5, counts[x]).ToArray(), offsets.AsSpan(x * 5, counts[x]).ToArray());
            if (counts[x] < 5) Assert.Equal(0, offsets[x * 5 + 4]);
        }
    }

    [Fact]
    public void PreCanceledPreparationDoesNotTouchTheBorrowedMetadata()
    {
        var offsets = Enumerable.Repeat(-99, 10).ToArray(); var token = new CancellationToken(true);
        var error = Assert.Throws<OperationCanceledException>(() =>
            BicubicMath.PrepareAdjacentFourTapPairs(offsets, new[] { 4, 4 }, 8, token));
        Assert.Equal(token, error.CancellationToken); Assert.All(offsets, value => Assert.Equal(-99, value));
    }

    [Theory]
    [InlineData(8, false)] [InlineData(8, true)] [InlineData(16, false)] [InlineData(16, true)]
    public void IdentityWidthKeepsExactRationalPixelsAndRetainedAdmission(int depth, bool tiny)
    {
        using var source = Lokad.Image.Tooling.OriginalPixels.Buffer(13, 7, depth);
        if (tiny)
            for (var y = 0; y < source.Height; y++) for (var x = 0; x < source.Width; x++)
                if (depth == 8) source.GetRow8(y)[x * 4 + 3] = (byte)(x < 7 ? 1 : 0);
                else source.GetRow16(y)[x * 4 + 3] = (ushort)(x < 7 ? 1 : 0);
        var input = OriginalPixels.Bytes(source); var bytes = 13 * 101 * (depth / 2);
        var expected = ExactResizeReference.Resize(input, 13, 7, depth, 13, 101, ResizeFilter.Bicubic);
        using var root = new TestMemoryBudget(bytes + 4096);
        var limits = new ImageLimits { MaxWorkingBytes = input.Length + bytes + 4096, MemoryBudget = root };
        using (var actual = ImageTransforms.Resize(source, 13, 101, ResizeFilter.Bicubic, limits))
        {
            Assert.Equal(expected, OriginalPixels.Bytes(actual)); Assert.Equal(bytes, root.Bytes);
            Assert.InRange(root.Peak, bytes, bytes + 4096);
        }
        Assert.Equal(0, root.Bytes); Assert.False(root.IsDisposed); Assert.Equal(input, OriginalPixels.Bytes(source));
    }
}
