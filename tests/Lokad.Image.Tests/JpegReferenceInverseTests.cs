using System.Security.Cryptography;
using Lokad.Image.Internal;
using Lokad.Image.Tooling;

namespace Lokad.Image.Tests;

public sealed class JpegReferenceInverseTests
{
    public static IEnumerable<object[]> Cases() => OriginalJpegInverseControls.Cases();

    [Fact]
    public void ExistingBaselineCoefficientsAndPlanarSamplesMatchPinnedNative()
        => BaselinePlanes();

    internal static byte[] BaselinePlanes()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "tests", "Fixtures", "jpeg-manifest.json")))
            directory = directory.Parent;
        Assert.NotNull(directory);
        var encoded = File.ReadAllBytes(Path.Combine(directory.FullName, "tests", "Fixtures", "jpeg-ffmpeg-YCbCr444-31x23.jpg"));
        Assert.Equal("ad1d9d11e42273349e85cfebc9b73c89231b595b883cd25d2b7832f62c353573", Hash(encoded));
        using var root = new TestMemoryBudget(4 * 1024 * 1024);
        using var observed = new MemoryStream();
        using (var context = new OperationContext(ImageLimits.Default with { MemoryBudget = root }, default))
        {
            var decoder = new JpegDecoder(encoded, context);
            decoder.ObserveFirstBaselineScan(observed);
        }
        Assert.Equal(0, root.Bytes); Assert.False(root.IsDisposed);
        // Complete public-libjpeg coefficient visitor record: natural-order
        // quantizers and all36 visible blocks, including frame sampling fields.
        Assert.Equal("8c5ceb73a9935b65b94ff7b017449b646bc3bcbcc68789f1afc21474c67d190e", Hash(observed.ToArray()));
        observed.Position = 0;
        using var reader = new BinaryReader(observed);
        Assert.Equal("LJCF0001"u8.ToArray(), reader.ReadBytes(8));
        Assert.Equal(31, reader.ReadInt32()); Assert.Equal(23, reader.ReadInt32()); Assert.Equal(3, reader.ReadInt32());
        var quantization = new ushort[64]; var coefficients = new int[64]; var block = new byte[64];
        using var planes = new MemoryStream();
        using (var context = new OperationContext(ImageLimits.Default with { MemoryBudget = root }, default))
        {
            for (var component = 0; component < 3; component++)
            {
                Assert.Equal(component + 1, reader.ReadInt32()); Assert.Equal(1, reader.ReadInt32()); Assert.Equal(2, reader.ReadInt32());
                Assert.Equal(4, reader.ReadInt32()); Assert.Equal(3, reader.ReadInt32());
                for (var k = 0; k < 64; k++) quantization[k] = reader.ReadUInt16();
                var plane = new byte[32 * 24];
                for (var y = 0; y < 3; y++)
                for (var x = 0; x < 4; x++)
                {
                    for (var k = 0; k < 64; k++) coefficients[k] = reader.ReadInt16();
                    JpegInverseTransform.Inverse(coefficients, quantization, block, context);
                    for (var row = 0; row < 8; row++) block.AsSpan(row * 8, 8).CopyTo(plane.AsSpan((y * 8 + row) * 32 + x * 8, 8));
                }
                for (var y = 0; y < 23; y++) planes.Write(plane.AsSpan(y * 32, 31));
            }
        }
        Assert.Equal(observed.Length, observed.Position);
        Assert.Equal(2139, planes.Length);
        // These are the complete actual FFmpeg planar bytes, with scalar and
        // normal native agreement on this image, independently checked first.
        Assert.Equal("37fc65b130eb90dd9d43e8ffa6831d910de969f692921ede3ea0ece80f286c2c", Hash(planes.ToArray()));
        Assert.Equal(0, root.Bytes); Assert.False(root.IsDisposed);
        return planes.ToArray();
    }

    private static string Hash(byte[] bytes) => Convert.ToHexStringLower(SHA256.HashData(bytes));

    [Theory] [MemberData(nameof(Cases))]
    public void OriginalSpectraMatchPinnedNormalSimdBytes(int id, string expectedHash)
    {
        var coefficients = new int[64]; var quantization = new ushort[64];
        OriginalJpegInverseControls.Fill(id, coefficients, quantization);
        var beforeCoefficients = coefficients.ToArray(); var beforeQuantization = quantization.ToArray();
        var guarded = Enumerable.Repeat((byte)165, 80).ToArray();
        using var root = new TestMemoryBudget(0);
        using var context = new OperationContext(ImageLimits.Default with { MemoryBudget = root, MaxWorkingBytes = 1 }, default);
        JpegInverseTransform.Inverse(coefficients, quantization, guarded.AsSpan(8, 64), context);
        Assert.Equal(expectedHash, Convert.ToHexStringLower(SHA256.HashData(guarded.AsSpan(8, 64))));
        Assert.Equal(beforeCoefficients, coefficients); Assert.Equal(beforeQuantization, quantization);
        Assert.All(guarded.Take(8).Concat(guarded.Skip(72)), value => Assert.Equal(165, value));
        Assert.Equal(0, root.Bytes); Assert.Equal(0, root.Attempts); Assert.False(root.IsDisposed);
    }

    [Theory] [InlineData(63, 64, 64)] [InlineData(64, 63, 64)] [InlineData(64, 64, 63)]
    public void IncompleteBlocksFailBeforeMutationOr(int coefficients, int quantizers, int pixels)
    {
        var output = Enumerable.Repeat((byte)165, pixels).ToArray();
        using var context = new OperationContext(ImageLimits.Default, default);
        Assert.Throws<ArgumentException>(() => JpegInverseTransform.Inverse(new int[coefficients], new ushort[quantizers], output, context));
        Assert.All(output, value => Assert.Equal(165, value));     }

    [Fact]
    public void IntegerExtremesKeepFrozenScalarPixelsWithoutHeapAdmission()
    {
        // Original 32 spectra, frozen against the preceding scalar matrix.
        // Exercise wrapping products/sums, saturated rows and word DC bias.
        var coefficients = new int[64]; var quantizers = new ushort[64];
        var pixels = new byte[32 * 64]; uint state = 0x91c72b35;
        using var root = new TestMemoryBudget(0);
        using var context = new OperationContext(ImageLimits.Default with { MemoryBudget = root, MaxWorkingBytes = 1 }, default);
        for (var block = 0; block < 32; block++)
        {
            for (var k = 0; k < 64; k++)
            {
                state ^= state << 13; state ^= state >> 17; state ^= state << 5;
                coefficients[k] = unchecked((int)state); quantizers[k] = (ushort)(1 + state % 65535);
            }
            if (block < 8)
            {
                coefficients[0] = new[] { int.MinValue, int.MaxValue, -32768, 32767, -1024, 0, 1024, 4095 }[block];
                quantizers[0] = 65535;
            }
            JpegInverseTransform.Inverse(coefficients, quantizers, pixels.AsSpan(block * 64, 64), context);
        }
        Assert.Equal("93fe53a2ff35c1d82ee9e99b5a8e54aa6a0eb47fb21e77926a4fc078c412df0b", Hash(pixels));
        Assert.Equal(0, root.Bytes); Assert.Equal(0, root.Attempts); Assert.False(root.IsDisposed);
    }

    [Fact]
    public void CancellationPrecedesTransformAndOutputMutation()
    {
        using var canceled = new CancellationTokenSource();
        using var root = new TestMemoryBudget(0);
        using var context = new OperationContext(ImageLimits.Default with { MemoryBudget = root }, canceled.Token);
        canceled.Cancel();
        var output = Enumerable.Repeat((byte)165, 64).ToArray();
        var exception = Assert.Throws<OperationCanceledException>(() =>
            JpegInverseTransform.Inverse(new int[64], new ushort[64], output, context));
        Assert.Equal(canceled.Token, exception.CancellationToken);
        Assert.All(output, value => Assert.Equal(165, value));
        Assert.Equal(0, root.Bytes); Assert.Equal(0, root.Attempts); Assert.False(root.IsDisposed);
    }
}
