using System.Buffers.Binary;
using System.Security.Cryptography;
using Lokad.Image.Internal;
using Lokad.Image.Tooling;

namespace Lokad.Image.Tests;

public sealed class JpegGrayReferenceTests
{
    [Fact]
    public void ProductionIntegerStagesMatchFrozenNativeValues()
    {
        var source = OriginalPixels.Generate(513, 259, 8); source[3] = 0; source[513 * 4 - 1] = 0;
        var gray = new byte[513 * 259];
        for (var i = 0; i < gray.Length; i++)
        {
            var alpha = source[i * 4 + 3];
            var r = (source[i * 4] * alpha + 32 * (255 - alpha) + 127) / 255;
            var g = (source[i * 4 + 1] * alpha + 64 * (255 - alpha) + 127) / 255;
            var b = (source[i * 4 + 2] * alpha + 96 * (255 - alpha) + 127) / 255;
            gray[i] = (byte)Math.Floor((0.299 * r + 0.587 * g) + 0.114 * b + 0.5);
        }
        Assert.Equal("6b74f1d2c127f3bcccba2b377f4474b9fc7464890853d05bf058bbb38337df8d", Hash(gray));
        var dct = new byte[274560]; var quantized = new byte[274560]; var at = 0;
        var quantization = JpegGrayReferenceTransform.Quantization(69);
        Span<byte> samples = stackalloc byte[64];
        Span<short> transformed = stackalloc short[64], coefficients = stackalloc short[64];
        for (var top = 0; top < 259; top += 8)
        for (var left = 0; left < 513; left += 8)
        {
            for (var y = 0; y < 8; y++)
            for (var x = 0; x < 8; x++)
                samples[y * 8 + x] = gray[Math.Min(top + y, 258) * 513 + Math.Min(left + x, 512)];
            JpegGrayReferenceTransform.Forward(samples, transformed);
            JpegGrayReferenceTransform.Quantize(transformed, quantization, coefficients);
            for (var k = 0; k < 64; k++, at += 2)
            {
                BinaryPrimitives.WriteInt16LittleEndian(dct.AsSpan(at), transformed[k]);
                BinaryPrimitives.WriteInt16LittleEndian(quantized.AsSpan(at), coefficients[k]);
            }
        }
        Assert.Equal("5e861e273a85c021d4c2a62b95f28e824cde355113e0715f237ed0603335a2db", Hash(dct));
        Assert.Equal("fa5ae5e71aa6bdcfba65001c838deb80b3826d29f4421d2b352edac85623e57a", Hash(quantized));
    }

    [Fact]
    public void GrayscaleTableOwnerCoversReplacementPeaksAndReleasesScratch()
    {
        var histogram = Enumerable.Repeat(1, 256).ToArray();
        using var root = new TestMemoryBudget(1024 * 1024);
        using var context = new OperationContext(ImageLimits.Default with { MemoryBudget = root }, default);
        context.Reserve(16384);
        var before = GC.GetAllocatedBytesForCurrentThread();
        var table = JpegGrayReferenceTables.Huffman(histogram, context);
        var allocated = GC.GetAllocatedBytesForCurrentThread() - before;
        Assert.Equal(16384, root.Bytes); Assert.Equal(16384 + 65536 + 257 * 2048, root.Peak);
        Assert.True(allocated <= root.Peak, $"Allocated {allocated}, admitted peak {root.Peak}.");
        // Native case13 of the qualified34-case grayscale histogram batch.
        Assert.Equal("fe76a73fd404d34dcc701f254ced15adca29ba9412f4c782a1a4a120dc18d93a",
            Hash(table.Counts.Concat(table.Symbols).ToArray()));
        Assert.All(histogram, count => Assert.Equal(1, count));
        for (var i = 0; i < 256; i++) Assert.NotEqual((1 << table.Lengths[i]) - 1, table.Codes[i]);
        context.Release(16384); Assert.Equal(0, root.Bytes); Assert.False(root.IsDisposed);
    }

    private static string Hash(byte[] bytes) => Convert.ToHexStringLower(SHA256.HashData(bytes));
}
