using System.Reflection;
using Lokad.Image.Internal;
using Lokad.Image.Tooling;

namespace Lokad.Image.Tests;

public sealed class JpegBudgetEventCacheTests
{
    private const int Side = 129;
    private static readonly FieldInfo Pool = typeof(JpegEncoder).GetField("adaptiveEvents", BindingFlags.NonPublic | BindingFlags.Static)!;

    [Theory]
    [InlineData(0, false)] [InlineData(0, true)]
    [InlineData(1, false)] [InlineData(1, true)]
    [InlineData(2, false)] [InlineData(2, true)]
    public void CallerBudgetNeitherBorrowsNorPopulatesUnownedThreadCache(int input, bool matchingCapacity)
    {
        var previous = Pool.GetValue(null);
        var capacity = 17 * 17 * (input == 2 ? 1 : 3) * 64;
        var marker = Enumerable.Repeat(0xFDFDFDFDu, matchingCapacity ? capacity : 1).ToArray();
        Pool.SetValue(null, marker);
        try
        {
            using var root = new TestMemoryBudget(4 * 1024 * 1024);
            var options = Options(root);
            using var first = new MemoryStream(); using var second = new MemoryStream();
            Encode(input, first, options); var held = first.ToArray();
            Assert.Equal(0, root.Bytes); Assert.False(root.IsDisposed);
            Assert.True(root.Peak >= 8192L + capacity * sizeof(uint) + 65536);
            Assert.Same(marker, Pool.GetValue(null)); Assert.All(marker, value => Assert.Equal(0xFDFDFDFDu, value));
            Encode(input, second, options); first.Dispose();
            Assert.Equal(held, second.ToArray()); Assert.True(second.CanWrite);
            Assert.Equal(0, root.Bytes); Assert.False(root.IsDisposed);
            Assert.Same(marker, Pool.GetValue(null)); Assert.All(marker, value => Assert.Equal(0xFDFDFDFDu, value));
        }
        finally { Pool.SetValue(null, previous); }
    }

    [Theory]
    [InlineData(0)] [InlineData(1)] [InlineData(2)]
    public void FailedOutputDoesNotRetainAChargedArrayAfterRefundAndLaterSuccess(int input)
    {
        var previous = Pool.GetValue(null); var marker = new uint[] { 12345 }; Pool.SetValue(null, marker);
        try
        {
            using var root = new TestMemoryBudget(4 * 1024 * 1024); using var output = new MemoryStream();
            var options = Options(root);
            var error = Assert.Throws<ImageLimitException>(() => Encode(input, output,
                options with { Limits = options.Limits with { MaxOutputBytes = 100 } }));
            Assert.Equal(ImageLimitKind.OutputBytes, error.Limit); Assert.InRange(output.Length, 1, 100);
            Assert.Equal(0, root.Bytes); Assert.False(root.IsDisposed); Assert.Same(marker, Pool.GetValue(null)); Assert.Equal(12345u, marker[0]);
            using var next = new MemoryStream(); Encode(input, next, options);
            Assert.True(next.Length > 100); Assert.Equal(0, root.Bytes); Assert.Same(marker, Pool.GetValue(null));
        }
        finally { Pool.SetValue(null, previous); }
    }

    [Theory]
    [InlineData(0)] [InlineData(1)] [InlineData(2)]
    public void RefusedEventStorageStopsBeforeAllocationOrOutputWithoutTouchingPool(int input)
    {
        var previous = Pool.GetValue(null); var marker = new uint[] { 56789 }; Pool.SetValue(null, marker);
        try
        {
            using var root = new TestMemoryBudget(4 * 1024 * 1024) { RefuseAtGrowth = 2 };
            using var output = new MemoryStream(); var options = Options(root);
            var error = Assert.Throws<ImageLimitException>(() => Encode(input, output, options));
            Assert.Equal((ImageLimitKind.WorkingBytes, ImageLimitScope.SharedBudget), (error.Limit, error.Scope));
            Assert.Equal(1, root.Refusals); Assert.Equal(0, root.Bytes); Assert.Equal(0, output.Length);
            Assert.Same(marker, Pool.GetValue(null)); Assert.Equal(56789u, marker[0]); Assert.False(root.IsDisposed);
            root.RefuseAtGrowth = 0; Encode(input, output, options);
            Assert.True(output.Length > 0); Assert.Equal(0, root.Bytes); Assert.Same(marker, Pool.GetValue(null));
        }
        finally { Pool.SetValue(null, previous); }
    }

    private static JpegEncodeOptions Options(TestMemoryBudget root) => new()
    { QuantizationPolicy = JpegQuantizationPolicy.Original, Quality = 69, Sampling = JpegSampling.YCbCr444, BackgroundRgb = 0x204060, Limits = new() { MemoryBudget = root } };

    private static void Encode(int input, Stream output, JpegEncodeOptions options)
    {
        if (input == 0)
        {
            using var image = ImageBuffer.Create(Side, Side, 8);
            for (var y = 0; y < Side; y++) image.GetRow8(y).Fill(255);
            ImageCodec.EncodeJpeg(image, output, options);
        }
        else
        {
            var format = input == 1 ? JpegSampleFormat.Rgb : JpegSampleFormat.Grayscale;
            var samples = Enumerable.Repeat((byte)255, Side * Side * (int)format).ToArray();
            ImageCodec.EncodeJpeg(samples, Side, Side, format, output, options: options);
            Assert.All(samples, value => Assert.Equal((byte)255, value));
        }
    }
}
