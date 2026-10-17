using System.Reflection;
using Lokad.Image.Internal;

namespace Lokad.Image.Tests;

public sealed class Vp8SourceConversionTests
{
    private delegate Vp8Planes Conversion(ImageBuffer image, OperationContext context, out byte[]? alpha);
    private static readonly Conversion Actual = typeof(Vp8Encoder).GetMethod("Convert", BindingFlags.Static | BindingFlags.NonPublic)!.CreateDelegate<Conversion>();

    [Theory]
    [InlineData(1)] [InlineData(4)]
    public void FirstWorkChargeStillPrecedesDisposedStorageAccess(long budget)
    {
        using var image = ImageBuffer.Create(1, 1, 8); image.Dispose();
        var limits = ImageLimits.Default with { MaxWork = budget };
        var original = new OperationContext(limits, default); var current = new OperationContext(limits, default);
        var originalError = Record.Exception(() => Reference(image, original, out _));
        var currentError = Record.Exception(() => Actual(image, current, out _));
        Assert.Equal(originalError?.GetType(), currentError?.GetType());
        Assert.Equal(original.RemainingWork, current.RemainingWork);
        Assert.Equal(original.RemainingWorkingBytes, current.RemainingWorkingBytes);
    }

    [Theory]
    [InlineData(8, 1, 1)] [InlineData(16, 1, 1)]
    [InlineData(8, 1, 3)] [InlineData(16, 1, 3)]
    [InlineData(8, 3, 1)] [InlineData(16, 3, 1)]
    [InlineData(8, 17, 9)] [InlineData(16, 17, 9)]
    public void SourceViewsPreserveScalarPlanesAlphaAndAdmission(int depth, int width, int height)
    {
        foreach (var opacity in new[] { 0, 128, 255, -1 })
        {
            using var image = ImageBuffer.Create(width, height, depth);
            for (var y = 0; y < height; y++)
            for (var x = 0; x < width; x++)
            for (var c = 0; c < 4; c++)
            {
                var value = (x * 4093 + y * 937 + c * 19937 + 128) & 65535;
                if (c == 3) value = opacity >= 0 ? opacity * 257 : new[] { 0, 1, 128, 65406, 65407, 65535 }[(x + y) % 6];
                if (depth == 8) image.GetRow8(y)[x * 4 + c] = (byte)((value + 128) / 257);
                else image.GetRow16(y)[x * 4 + c] = (ushort)value;
            }
            var before = image.Pixels.ToArray();
            var expectedContext = new OperationContext(ImageLimits.Default, default);
            var expected = Reference(image, expectedContext, out var expectedAlpha);
            var actualContext = new OperationContext(ImageLimits.Default, default);
            var actual = Actual(image, actualContext, out var actualAlpha);
            Assert.Equal(expected.Y, actual.Y); Assert.Equal(expected.U, actual.U); Assert.Equal(expected.V, actual.V);
            Assert.Equal(expectedAlpha, actualAlpha);
            Assert.Equal(expectedContext.RemainingWork, actualContext.RemainingWork);
            Assert.Equal(expectedContext.RemainingWorkingBytes, actualContext.RemainingWorkingBytes);
            Assert.Equal(before, image.Pixels.ToArray());
            var work = ImageLimits.Default.MaxWork - expectedContext.RemainingWork;
            foreach (var budget in new[] { 1L, width * 4L, work - 1, work })
            {
                var limits = ImageLimits.Default with { MaxWork = budget };
                var original = new OperationContext(limits, default); var current = new OperationContext(limits, default);
                var originalError = Record.Exception(() => Reference(image, original, out _));
                var currentError = Record.Exception(() => Actual(image, current, out _));
                Assert.Equal(originalError?.GetType(), currentError?.GetType());
                Assert.Equal(original.RemainingWork, current.RemainingWork);
                Assert.Equal(original.RemainingWorkingBytes, current.RemainingWorkingBytes);
            }
        }
    }

    private static Vp8Planes Reference(ImageBuffer image, OperationContext context, out byte[]? alpha)
    {
        var stride = (image.Width + 15) / 16 * 16; var height = (image.Height + 15) / 16 * 16;
        var length = checked(stride * height); context.Reserve(length * 3L / 2 + 128);
        var result = new Vp8Planes(image.Width, image.Height, stride, new byte[length], new byte[length / 4], new byte[length / 4]);
        var transparent = false;
        for (var y = 0; y < image.Height; y++)
        {
            context.Work(image.Width * 4L);
            for (var x = 0; x < image.Width; x++) transparent |= OriginalSample(image, x, y, 3) != 255;
        }
        alpha = null;
        if (transparent)
        {
            context.Reserve((long)image.Width * image.Height + 32);
            alpha = new byte[image.Width * image.Height];
            for (var y = 0; y < image.Height; y++)
            {
                context.Work(image.Width * 4L);
                for (var x = 0; x < image.Width; x++) alpha[y * image.Width + x] = (byte)OriginalSample(image, x, y, 3);
            }
        }
        for (var y = 0; y < height; y++)
        {
            context.Work(stride * 32L);
            for (var x = 0; x < stride; x++)
            {
                var r = OriginalSample(image, x, y, 0); var g = OriginalSample(image, x, y, 1); var b = OriginalSample(image, x, y, 2);
                result.Y[y * stride + x] = OriginalRound(16 + 219.0 / 255 * (.299 * r + .587 * g + .114 * b));
            }
        }
        for (var y = 0; y < height / 2; y++)
        {
            context.Work(stride * 64L);
            for (var x = 0; x < stride / 2; x++)
            {
                double r = 0, g = 0, b = 0; int alphaSum = 0, redSum = 0, greenSum = 0, blueSum = 0;
                for (var dy = 0; dy < 2; dy++)
                    for (var dx = 0; dx < 2; dx++)
                    {
                        var sx = x * 2 + dx; var sy = y * 2 + dy;
                        var red = OriginalSample(image, sx, sy, 0); var green = OriginalSample(image, sx, sy, 1); var blue = OriginalSample(image, sx, sy, 2);
                        var a = OriginalSample(image, sx, sy, 3);
                        r += red / 4.0; g += green / 4.0; b += blue / 4.0;
                        alphaSum += a; redSum += a * red; greenSum += a * green; blueSum += a * blue;
                    }
                if (alphaSum != 0)
                {
                    r = (double)redSum / alphaSum; g = (double)greenSum / alphaSum; b = (double)blueSum / alphaSum;
                }
                result.U[y * stride / 2 + x] = OriginalRound(128 + 224.0 / 255 * (-.168735891647855 * r - .331264108352145 * g + .5 * b));
                result.V[y * stride / 2 + x] = OriginalRound(128 + 224.0 / 255 * (.5 * r - .418687589158345 * g - .081312410841655 * b));
            }
        }
        return result;
    }

    private static int OriginalSample(ImageBuffer image, int x, int y, int channel)
    {
        var value = image.Sample(Math.Min(x, image.Width - 1), Math.Min(y, image.Height - 1), channel);
        return image.BitDepth == 8 ? value : (value + 128) / 257;
    }

    private static byte OriginalRound(double value) => (byte)Math.Clamp((int)Math.Floor(value + .5), 0, 255);

}
