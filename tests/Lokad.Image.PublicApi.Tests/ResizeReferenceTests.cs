using System.Buffers.Binary;
using System.Security.Cryptography;
using Lokad.Image.Tooling;

namespace Lokad.Image.PublicApi.Tests;

public sealed class ResizeReferenceTests
{
    [Theory]
    [InlineData(false)] [InlineData(true)]
    public void UInt16PairedStoresKeepVisibleAndZeroAlphaRowsIndependent(bool reverse)
    {
        using var source = ImageBuffer.Create(2, 2, 16);
        ushort[] hidden = [65535, 17, 54321, 0, 21, 65535, 9, 0];
        ushort[] visible = [100, 200, 300, 65535, 65535, 12345, 54321, 65535];
        hidden.CopyTo(source.GetRow16(reverse ? 1 : 0));
        visible.CopyTo(source.GetRow16(reverse ? 0 : 1));
        var input = OriginalPixels.Bytes(source);
        using var prepared = ImageTransforms.Resize(source, 3, 2, ResizeFilter.Bilinear);
        using var direct = ImageTransforms.Resize(source, 3, 2, ResizeFilter.Bilinear,
            new ImageLimits { MaxWorkingBytes = input.Length + 3 * 2 * 8 + 4096 });
        Assert.Equal(OriginalPixels.Bytes(direct), OriginalPixels.Bytes(prepared));
        Assert.Equal(new ushort[12], prepared.GetRow16(reverse ? 1 : 0).ToArray());
        Assert.Equal(visible.AsSpan(0, 4).ToArray(), prepared.GetRow16(reverse ? 0 : 1).Slice(0, 4).ToArray());
        Assert.Equal(visible.AsSpan(4, 4).ToArray(), prepared.GetRow16(reverse ? 0 : 1).Slice(8, 4).ToArray());
        Assert.Equal(input, OriginalPixels.Bytes(source));
    }

    [Theory]
    [InlineData(ResizeFilter.Bilinear, 3)] [InlineData(ResizeFilter.Bilinear, 4)]
    [InlineData(ResizeFilter.Bicubic, 3)] [InlineData(ResizeFilter.Bicubic, 4)]
    public void UInt16PairedRowsPreserveTinyAlphaHalvesAndOddTails(ResizeFilter filter, int height)
    {
        using var source = ImageBuffer.Create(2, 2, 16);
        ushort[] row = [65535, 23, 42, 0, 65535, 54321, 12345, 1];
        row.CopyTo(source.GetRow16(0)); row.CopyTo(source.GetRow16(1));
        var input = OriginalPixels.Bytes(source);
        using var prepared = ImageTransforms.Resize(source, 3, height, filter);
        using var direct = ImageTransforms.Resize(source, 3, height, filter,
            new ImageLimits { MaxWorkingBytes = input.Length + 3L * height * 8 + 4096 });
        Assert.Equal(OriginalPixels.Bytes(direct), OriginalPixels.Bytes(prepared));
        for (var y = 0; y < height; y++)
            Assert.Equal(new ushort[] { 65535, 54321, 12345, 1 }, prepared.GetRow16(y).Slice(4, 4).ToArray());
        Assert.Equal(input, OriginalPixels.Bytes(source));
        Assert.Throws<OperationCanceledException>(() => ImageTransforms.Resize(source, 3, height, filter,
            cancellationToken: new CancellationToken(true)));
    }

    [Theory]
    [InlineData(8, 1, 5, 2053, 3)] [InlineData(16, 1, 5, 2053, 3)]
    [InlineData(8, 513, 3, 2051, 2)] [InlineData(16, 513, 3, 2051, 2)]
    [InlineData(8, 2053, 2, 513, 1)] [InlineData(16, 2053, 2, 513, 1)]
    [InlineData(8, 4093, 1, 1025, 5)] [InlineData(16, 4093, 1, 1025, 5)]
    public void NearestPreservesExactCellsAcrossCopyCheckpointsWithNoScratch(int depth,
        int sourceWidth, int sourceHeight, int width, int height)
    {
        using var source = OriginalPixels.Buffer(sourceWidth, sourceHeight, depth);
        var input = OriginalPixels.Bytes(source);
        var expected = ExactResizeReference.Resize(input, sourceWidth, sourceHeight, depth, width, height, ResizeFilter.Nearest);
        var work = (long)width * height * 4;
        var limits = new ImageLimits { MaxWorkingBytes = input.Length + (long)expected.Length, MaxWork = work };
        using var first = ImageTransforms.Resize(source, width, height, ResizeFilter.Nearest, limits);
        using var second = ImageTransforms.Resize(source, width, height, ResizeFilter.Nearest, limits);
        Assert.Equal(expected, OriginalPixels.Bytes(first));
        Assert.Equal(expected, OriginalPixels.Bytes(second));
        if (depth == 8) first.GetRow8(0)[0] ^= 255;
        else first.GetRow16(0)[0] ^= 65535;
        Assert.Equal(expected, OriginalPixels.Bytes(second));
        Assert.Equal(input, OriginalPixels.Bytes(source));
        Assert.Throws<ImageLimitException>(() => ImageTransforms.Resize(source, width, height, ResizeFilter.Nearest,
            limits with { MaxWorkingBytes = limits.MaxWorkingBytes - 1 }));
        Assert.Throws<ImageLimitException>(() => ImageTransforms.Resize(source, width, height, ResizeFilter.Nearest,
            limits with { MaxWork = work - 1 }));
    }

    [Theory]
    [InlineData(ResizeFilter.Bilinear, 255, 255, 1, 255, 128)]
    [InlineData(ResizeFilter.Bilinear, 255, 254, 0, 254, 127)]
    [InlineData(ResizeFilter.Bilinear, 254, 255, 1, 255, 128)]
    [InlineData(ResizeFilter.Bicubic, 255, 255, 1, 255, 128)]
    [InlineData(ResizeFilter.Bicubic, 255, 254, 0, 254, 127)]
    [InlineData(ResizeFilter.Bicubic, 254, 255, 1, 255, 128)]
    public void ByteFilteringRoundsAtAndOnEitherSideOfColorHalves(ResizeFilter filter,
        byte leftAlpha, byte rightAlpha, byte red, byte green, byte blue)
    {
        // Original two-pixel input: the center has equal geometric weights.
        // Alpha imbalance moves all three RGB values below or above their halves.
        using var source = ImageBuffer.Create(2, 1, 8);
        var input = new byte[] { 0, 254, 0, leftAlpha, 1, 255, 255, rightAlpha };
        input.CopyTo(source.GetRow8(0));
        using var result = ImageTransforms.Resize(source, 3, 1, filter);
        Assert.Equal(new byte[] { red, green, blue, 255 }, result.GetRow8(0).Slice(4, 4).ToArray());
        Assert.Equal(input, source.GetRow8(0).ToArray());
    }

    [Theory]
    [InlineData(8, ResizeFilter.Bilinear)]
    [InlineData(8, ResizeFilter.Bicubic)]
    [InlineData(16, ResizeFilter.Bilinear)]
    [InlineData(16, ResizeFilter.Bicubic)]
    public void PreparedHorizontalGeometryRetainsPixelsWithTightMemoryLimits(int depth, ResizeFilter filter)
    {
        using var source = OriginalPixels.Buffer(67, 13, depth);
        var sourcePixels = OriginalPixels.Bytes(source);
        foreach (var target in new[] { (40, 7), (71, 17) })
        {
            using var prepared = ImageTransforms.Resize(source, target.Item1, target.Item2, filter);
            var peak = sourcePixels.Length + (long)target.Item1 * target.Item2 * 4 * (depth / 8) + 4096;
            using var direct = ImageTransforms.Resize(source, target.Item1, target.Item2, filter,
                limits: new ImageLimits { MaxWorkingBytes = peak });
            Assert.Equal(OriginalPixels.Bytes(direct), OriginalPixels.Bytes(prepared));
            Assert.Equal(sourcePixels, OriginalPixels.Bytes(source));
        }
    }

    [Theory]
    [InlineData(8, ResizeFilter.Bilinear, "4920566bb257cb6dda29eaa0b48876ed0d9da0435186f9c8f817a1742b6540bf")]
    [InlineData(8, ResizeFilter.Bicubic, "da6936513ba65f959adcc10cafc83410f6f7253b834535b822831d3a4b698f92")]
    [InlineData(16, ResizeFilter.Bilinear, "10a39d150207778a378bb653ae928135089a54f33221d0d074a333c5c346f659")]
    [InlineData(16, ResizeFilter.Bicubic, "ae77b88f4eecec987e663fb7aa4401d2b923ad6f9351ad4a51e7545982916ccd")]
    public void FilteredOriginalRastersRetainBinary64Rounding(int depth, ResizeFilter filter, string expected)
    {
        // Original asymmetric-v1 inputs and pre-SIMD scalar outputs. Concatenate
        // these five existing target shapes, transparent then opaque, in order.
        // This tightens the independent rational-reference test's one-level bound
        // to exact retained pixels across edges, shrinking and mixed-axis scaling.
        using var hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
        foreach (var target in new[] { (5, 3), (19, 15), (4, 15), (1, 1), (9, 7) })
        foreach (var opaque in new[] { false, true })
        {
            using var source = OriginalPixels.Buffer(9, 7, depth, opaque: opaque);
            using var resized = ImageTransforms.Resize(source, target.Item1, target.Item2, filter);
            hash.AppendData(OriginalPixels.Bytes(resized));
        }
        Assert.Equal(expected, Convert.ToHexStringLower(hash.GetHashAndReset()));
    }

    public static IEnumerable<object[]> Profiles()
    {
        foreach (var depth in new[] { 8, 16 })
        foreach (var filter in Enum.GetValues<ResizeFilter>())
        foreach (var target in new[] { (5, 3), (19, 15), (4, 15), (1, 1), (9, 7) })
        foreach (var opaque in new[] { false, true }) yield return [depth, filter, target.Item1, target.Item2, opaque];
    }

    [Theory]
    [MemberData(nameof(Profiles))]
    public void OriginalRastersMatchExactRationalAccumulation(int depth, ResizeFilter filter, int width, int height, bool opaque)
    {
        var pixels = OriginalPixels.Generate(9, 7, depth, opaque: opaque);
        var expected = ExactResizeReference.Resize(pixels, 9, 7, depth, width, height, filter);
        using var source = OriginalPixels.Buffer(9, 7, depth, opaque: opaque);
        using var resized = ImageTransforms.Resize(source, width, height, filter);
        Assert.Equal((width, height, depth), (resized.Width, resized.Height, resized.BitDepth));
        var actual = OriginalPixels.Bytes(resized); var maximum = 0;
        for (var offset = 0; offset < actual.Length; offset += depth / 8)
        {
            var a = depth == 8 ? actual[offset] : BinaryPrimitives.ReadUInt16LittleEndian(actual.AsSpan(offset, 2));
            var b = depth == 8 ? expected[offset] : BinaryPrimitives.ReadUInt16LittleEndian(expected.AsSpan(offset, 2));
            maximum = Math.Max(maximum, Math.Abs(a - b));
        }
        // Binary64 accumulation can fall on either side of an exact half. This
        // one-sample analytical bound is independent of native scaler behavior.
        Assert.True(maximum <= (filter == ResizeFilter.Nearest || (width, height) == (9, 7) ? 0 : 1),
            $"{depth}/{filter}/{width}x{height}/opaque={opaque}: maximum rational-reference error {maximum}.");
        Assert.Equal(pixels, OriginalPixels.Bytes(source));
    }

    [Theory]
    [InlineData(8, ResizeFilter.Bilinear)]
    [InlineData(8, ResizeFilter.Bicubic)]
    [InlineData(16, ResizeFilter.Bilinear)]
    [InlineData(16, ResizeFilter.Bicubic)]
    public void RoundedZeroAlphaHasZeroRgbForFractionalEdgesAndTinyPositiveAlpha(int depth, ResizeFilter filter)
    {
        using var edge = ImageBuffer.Create(9, 7, depth);
        var maximum = depth == 8 ? 255 : 65535;
        for (var y = 0; y < 7; y++)
        for (var x = 0; x < 9; x++)
        {
            var channels = new[] { maximum, maximum / 3, maximum / 7, x >= 4 ? maximum : 0 };
            for (var channel = 0; channel < 4; channel++)
                if (depth == 8) edge.GetRow8(y)[x * 4 + channel] = (byte)channels[channel];
                else edge.GetRow16(y)[x * 4 + channel] = (ushort)channels[channel];
        }
        using var reduced = ImageTransforms.Resize(edge, 5, 3, filter);
        AssertZeroAlphaRgb(reduced);
        using var tiny = ImageBuffer.Create(2, 1, depth);
        if (depth == 8) new byte[] { 255, 0, 0, 0, 0, 255, 255, 1 }.CopyTo(tiny.GetRow8(0));
        else new ushort[] { 65535, 0, 0, 0, 0, 65535, 65535, 1 }.CopyTo(tiny.GetRow16(0));
        using var enlarged = ImageTransforms.Resize(tiny, 4, 1, filter);
        AssertZeroAlphaRgb(enlarged);
        Assert.Equal(new byte[4 * (depth / 8)], OriginalPixels.Bytes(enlarged).AsSpan(4 * (depth / 8), 4 * (depth / 8)).ToArray());
    }

    private static void AssertZeroAlphaRgb(ImageBuffer image)
    {
        var zeroAlphaPixels = 0;
        for (var y = 0; y < image.Height; y++)
        for (var x = 0; x < image.Width; x++)
        {
            var alpha = image.BitDepth == 8 ? image.GetRow8(y)[x * 4 + 3] : image.GetRow16(y)[x * 4 + 3];
            if (alpha != 0) continue;
            zeroAlphaPixels++;
            for (var c = 0; c < 3; c++) Assert.Equal(0, image.BitDepth == 8 ? image.GetRow8(y)[x * 4 + c] : image.GetRow16(y)[x * 4 + c]);
        }
        Assert.True(zeroAlphaPixels > 0);
    }

    public static IEnumerable<object[]> LowAlphaProfiles()
    {
        foreach (var depth in new[] { 8, 16 })
        foreach (var filter in new[] { ResizeFilter.Bilinear, ResizeFilter.Bicubic })
        foreach (var size in new[] { (5, 2, 3, 3), (9, 7, 5, 3), (3, 2, 5, 3) })
        foreach (var checker in new[] { false, true }) yield return [depth, filter, size.Item1, size.Item2, size.Item3, size.Item4, checker];
    }

    [Theory]
    [MemberData(nameof(LowAlphaProfiles))]
    public void HalfAlphaRoundingRetainsVisibleRgbExactly(int depth, ResizeFilter filter,
        int sourceWidth, int sourceHeight, int width, int height, bool checker)
    {
        using var source = ImageBuffer.Create(sourceWidth, sourceHeight, depth);
        var maximum = depth == 8 ? 255 : 65535;
        for (var y = 0; y < sourceHeight; y++)
        for (var x = 0; x < sourceWidth; x++)
        {
            var alpha = (checker ? (x + y) % 2 == 0 : y < sourceHeight / 2) ? 1 : 0;
            if (depth == 8) new byte[] { (byte)(alpha > 0 ? 255 : 127), (byte)(alpha > 0 ? 85 : 255),
                (byte)(alpha > 0 ? 36 : 7), (byte)alpha }.CopyTo(source.GetRow8(y).Slice(x * 4, 4));
            else new ushort[] { (ushort)(alpha > 0 ? 65535 : 32767), (ushort)(alpha > 0 ? 21845 : 65535),
                (ushort)(alpha > 0 ? 9362 : 7), (ushort)alpha }.CopyTo(source.GetRow16(y).Slice(x * 4, 4));
        }
        var expected = ExactResizeReference.Resize(OriginalPixels.Bytes(source), sourceWidth, sourceHeight, depth, width, height, filter);
        using var result = ImageTransforms.Resize(source, width, height, filter);
        Assert.Equal(expected, OriginalPixels.Bytes(result));
        if (sourceHeight == 2 && !checker)
        {
            // Every center-row sample lies halfway between alpha 1 and 0.
            // Literal expectations ensure this does not merely mirror a helper.
            for (var x = 0; x < width; x++)
            {
                if (depth == 8) Assert.Equal(new byte[] { 255, 85, 36, 1 }, result.GetRow8(1).Slice(x * 4, 4).ToArray());
                else Assert.Equal(new ushort[] { 65535, 21845, 9362, 1 }, result.GetRow16(1).Slice(x * 4, 4).ToArray());
            }
        }
        Assert.Equal(maximum, depth == 8 ? source.GetRow8(0)[0] : source.GetRow16(0)[0]);
    }

    [Theory]
    [InlineData(8, ResizeFilter.Bilinear)]
    [InlineData(8, ResizeFilter.Bicubic)]
    [InlineData(16, ResizeFilter.Bilinear)]
    [InlineData(16, ResizeFilter.Bicubic)]
    public void HalfAlphaResolutionRespectsExactMemoryAdmissionAndWorkLimits(int depth, ResizeFilter filter)
    {
        using var source = ImageBuffer.Create(5, 2, depth);
        for (var y = 0; y < 2; y++)
        for (var x = 0; x < 5; x++)
            if (depth == 8) new byte[] { 255, 255, 255, (byte)(1 - y) }.CopyTo(source.GetRow8(y).Slice(x * 4, 4));
            else new ushort[] { 65535, 65535, 65535, (ushort)(1 - y) }.CopyTo(source.GetRow16(y).Slice(x * 4, 4));
        var peak = (5 * 2 + 3 * 3) * 4 * (depth / 8) + 4096;
        using var exact = ImageTransforms.Resize(source, 3, 3, filter, limits: new ImageLimits { MaxWorkingBytes = peak });
        using var prepared = ImageTransforms.Resize(source, 3, 3, filter);
        Assert.Equal(OriginalPixels.Bytes(exact), OriginalPixels.Bytes(prepared));
        Assert.Throws<ImageLimitException>(() => ImageTransforms.Resize(source, 3, 3, filter, limits: new ImageLimits { MaxWorkingBytes = peak - 1 }));
        // The ordinary float path's entire work fits; resolving half alpha must
        // consume additional work rather than escaping the operation budget.
        var ordinaryWork = filter == ResizeFilter.Bilinear ? 540 : 1440;
        Assert.Throws<ImageLimitException>(() => ImageTransforms.Resize(source, 3, 3, filter, limits: new ImageLimits { MaxWork = ordinaryWork }));
        Assert.Throws<OperationCanceledException>(() => ImageTransforms.Resize(source, 3, 3, filter,
            cancellationToken: new CancellationToken(true)));
        Assert.Equal(depth == 8 ? 255 : 65535, depth == 8 ? source.GetRow8(0)[0] : source.GetRow16(0)[0]);
    }
}
