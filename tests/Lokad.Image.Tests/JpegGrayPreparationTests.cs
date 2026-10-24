using System.Security.Cryptography;
using Lokad.Image.Internal;
using Lokad.Image.Tooling;

namespace Lokad.Image.Tests;

public sealed class JpegGrayPreparationTests
{
    [Fact]
    public void OriginalControlsAndEdgeLayoutsKeepFrozenGrayBytes()
    {
        using var hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
        var block = new byte[64]; var blocks = 0;
        foreach (var shape in new[] { (513, 259), (1, 1), (7, 9), (8, 8), (9, 7), (17, 3) })
        foreach (var depth in new[] { 8, 16 })
        foreach (var opaque in new[] { false, true })
        {
            var width = shape.Item1; var height = shape.Item2;
            using var image = OriginalPixels.Buffer(width, height, depth, opaque: opaque);
            if (!opaque)
            {
                if (depth == 8) { image.GetRow8(0)[3] = 0; image.GetRow8(0)[width * 4 - 1] = 0; }
                else { image.GetRow16(0)[3] = 0; image.GetRow16(0)[width * 4 - 1] = 0; }
            }
            var before = OriginalPixels.Bytes(image);
            for (var top = 0; top < height; top += 8)
            for (var left = 0; left < width; left += 8)
            {
                JpegGrayPreparation.CopyBlock(image, default, width, height, left, top, opaque ? null : 0x204060u, block);
                hash.AppendData(block); blocks++;
            }
            Assert.Equal(before, OriginalPixels.Bytes(image));
        }
        foreach (var shape in new[] { (513, 259), (1, 1), (7, 9), (9, 7) })
        {
            var width = shape.Item1; var height = shape.Item2; var rgb = new byte[width * height * 3];
            for (var i = 0; i < rgb.Length; i++) rgb[i] = (byte)(i * 73 + i / 3 * 19);
            var before = rgb.ToArray(); using var context = new OperationContext(ImageLimits.Default, default);
            var source = new JpegSampleInput(rgb, width, height, JpegSampleFormat.Rgb, 0, context);
            for (var top = 0; top < height; top += 8)
            for (var left = 0; left < width; left += 8)
            {
                JpegGrayPreparation.CopyBlock(null, source, width, height, left, top, null, block);
                hash.AppendData(block); blocks++;
            }
            Assert.Equal(before, rgb);
        }
        Assert.Equal(10766, blocks);
        Assert.Equal("a8cf28a328a23308f23d4ba8018977c94fa6a524af57c17e364b09e342a227fd",
            Convert.ToHexStringLower(hash.GetHashAndReset()));
    }

    [Fact]
    public void CallerRgbRowsClampEdgesWithoutReadingPadding()
    {
        foreach (var shape in new[] { (1, 1), (7, 9), (9, 7) })
        {
            var width = shape.Item1; var height = shape.Item2; var stride = width * 3 + 7;
            var bytes = Enumerable.Repeat((byte)0xA7, stride * height).ToArray();
            for (var y = 0; y < height; y++) for (var x = 0; x < width; x++)
            {
                var at = y * stride + x * 3;
                bytes[at] = (byte)(x * 73 + y * 19); bytes[at + 1] = (byte)(255 - bytes[at]); bytes[at + 2] = (byte)(x + y * 37);
            }
            var before = bytes.ToArray(); var block = new byte[64];
            using var context = new OperationContext(ImageLimits.Default, default);
            var source = new JpegSampleInput(bytes, width, height, JpegSampleFormat.Rgb, stride, context);
            for (var top = 0; top < height; top += 8) for (var left = 0; left < width; left += 8)
            {
                JpegGrayPreparation.CopyBlock(null, source, width, height, left, top, null, block);
                for (var y = 0; y < 8; y++) for (var x = 0; x < 8; x++)
                {
                    var at = Math.Min(top + y, height - 1) * stride + Math.Min(left + x, width - 1) * 3;
                    var luma = (0.299 * bytes[at] + 0.587 * bytes[at + 1]) + 0.114 * bytes[at + 2];
                    Assert.Equal((byte)Math.Floor(luma + 0.5), block[y * 8 + x]);
                }
            }
            Assert.Equal(before, bytes);
        }
    }

    [Fact]
    public void BackgroundAndOutputValidationKeepBorrowedImageIntact()
    {
        foreach (var depth in new[] { 8, 16 })
        {
            using var image = OriginalPixels.Buffer(8, 8, depth);
            var before = OriginalPixels.Bytes(image); var block = new byte[64];
            var error = Assert.Throws<ArgumentException>(() => JpegGrayPreparation.CopyBlock(image, default, 8, 8, 0, 0, null, block));
            Assert.Equal("image", error.ParamName); Assert.Equal(before, OriginalPixels.Bytes(image));
            foreach (var length in new[] { 0, 63, 65 })
            {
                var invalid = Enumerable.Repeat((byte)77, length).ToArray();
                error = Assert.Throws<ArgumentException>(() => JpegGrayPreparation.CopyBlock(image, default, 8, 8, 0, 0, 0x204060, invalid));
                Assert.Equal("output", error.ParamName); Assert.All(invalid, value => Assert.Equal((byte)77, value));
            }
        }
    }

    [Fact]
    public void WarmPreparationUsesNoManagedAllocation()
    {
        using var image8 = OriginalPixels.Buffer(8, 8, 8); using var image16 = OriginalPixels.Buffer(8, 8, 16);
        var rgb = Enumerable.Range(0, 192).Select(p => (byte)(p * 73)).ToArray(); var block = new byte[64];
        using var context = new OperationContext(ImageLimits.Default, default);
        var source = new JpegSampleInput(rgb, 8, 8, JpegSampleFormat.Rgb, 0, context);
        for (var i = 0; i < 256; i++)
        {
            JpegGrayPreparation.CopyBlock(image8, default, 8, 8, 0, 0, 0x204060, block);
            JpegGrayPreparation.CopyBlock(image16, default, 8, 8, 0, 0, 0x204060, block);
            JpegGrayPreparation.CopyBlock(null, source, 8, 8, 0, 0, null, block);
        }
        var before = GC.GetAllocatedBytesForCurrentThread();
        for (var i = 0; i < 10000; i++)
        {
            JpegGrayPreparation.CopyBlock(image8, default, 8, 8, 0, 0, 0x204060, block);
            JpegGrayPreparation.CopyBlock(image16, default, 8, 8, 0, 0, 0x204060, block);
            JpegGrayPreparation.CopyBlock(null, source, 8, 8, 0, 0, null, block);
        }
        Assert.Equal(0, GC.GetAllocatedBytesForCurrentThread() - before);
    }

    [Fact]
    public void ExistingOperationAdmissionCoversPreparationWithoutNewCharges()
    {
        using var root = new TestMemoryBudget(10000); var limits = ImageLimits.Default with { MemoryBudget = root };
        using var image = OriginalPixels.Buffer(8, 8, 8);
        using var context = new OperationContext(limits, default); context.Reserve(8192);
        var bytes = root.Bytes; var attempts = root.Attempts; var output = new byte[64];
        for (var i = 0; i < 256; i++) JpegGrayPreparation.CopyBlock(image, default, 8, 8, 0, 0, 0x204060, output);
        Assert.Equal(bytes, root.Bytes); Assert.Equal(attempts, root.Attempts); Assert.Equal(0, context.ConsumedWork);
        context.Release(8192); Assert.Equal(0, root.Bytes); Assert.False(root.IsDisposed);
    }
}
