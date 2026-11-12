using Lokad.Image.Internal;

namespace Lokad.Image.Tests;

public sealed class WebpInverseColorTests
{
    [Theory]
    [InlineData(3, 2)] [InlineData(8, 3)] [InlineData(17, 4)]
    [InlineData(33, 2)] [InlineData(513, 5)] [InlineData(513, 9)]
    public void TileCoefficientsPreserveSignedChannelsAndUpdatedRedAcrossBatchTails(int width, int bits)
    {
        const int height = 35;
        var random = new Random(849 + width + bits);
        var storage = new uint[width * height + 2];
        var expected = new uint[width * height];
        var tileWidth = (width + (1 << bits) - 1) >> bits;
        var tiles = new uint[tileWidth * ((height + (1 << bits) - 1) >> bits)];
        int[] signed = [-128, -127, -33, -1, 0, 1, 31, 32, 127];
        for (var i = 0; i < tiles.Length; i++)
            tiles[i] = (uint)((byte)signed[i % signed.Length] |
                (byte)signed[(i * 5 + 2) % signed.Length] << 8 |
                (byte)signed[(i * 7 + 4) % signed.Length] << 16);
        var originalTiles = tiles.ToArray();
        storage[0] = 0x13579bdf; storage[^1] = 0x2468ace0;
        for (var y = 0; y < height; y++)
        for (var x = 0; x < width; x++)
        {
            var i = y * width + x;
            var green = i % 256; var red = random.Next(256); var blue = random.Next(256);
            var alpha = i % 3 == 0 ? 0 : random.Next(256);
            if (i % 7 == 0) red = 128;
            storage[i + 1] = (uint)(alpha << 24 | red << 16 | green << 8 | blue);
            var coefficients = tiles[(y >> bits) * tileWidth + (x >> bits)];
            var signedGreen = green < 128 ? green : green - 256;
            red = Modulo(red + Delta((sbyte)coefficients, signedGreen));
            blue = Modulo(blue + Delta((sbyte)(coefficients >> 8), signedGreen) +
                Delta((sbyte)(coefficients >> 16), red < 128 ? red : red - 256));
            expected[i] = (uint)(alpha << 24 | red << 16 | green << 8 | blue);
        }
        using var context = new OperationContext(new() { MaxWorkingBytes = 1 }, default);
        WebpLosslessDecoder.ApplyColorTransform(storage.AsSpan(1, expected.Length), width, height, bits, tiles, context);
        Assert.True(expected.AsSpan().SequenceEqual(storage.AsSpan(1, expected.Length)));
        Assert.Equal(originalTiles, tiles); Assert.Equal(1, context.RemainingWorkingBytes);
        Assert.Equal(0x13579bdfu, storage[0]); Assert.Equal(0x2468ace0u, storage[^1]);
        var before = GC.GetAllocatedBytesForCurrentThread();
        WebpLosslessDecoder.ApplyColorTransform(storage.AsSpan(1, expected.Length), width, height, bits, tiles, context);
        Assert.Equal(0, GC.GetAllocatedBytesForCurrentThread() - before);
    }

    [Fact]
    public void CanceledColorTransformPreservesUnwrittenStorageAndBorrowedContext()
    {
        using var cancellation = new CancellationTokenSource();
        using var context = new OperationContext(new() { MaxWorkingBytes = 1 }, cancellation.Token);
        uint[] pixels = [0x0012ff80, 0xff34ff70]; var original = pixels.ToArray();
        cancellation.Cancel();
        var error = Assert.Throws<OperationCanceledException>(() =>
            WebpLosslessDecoder.ApplyColorTransform(pixels, 2, 1, 2, new uint[] { 0x00807fff }, context));
        Assert.Equal(cancellation.Token, error.CancellationToken);
        Assert.Equal(original, pixels); Assert.Equal(1, context.RemainingWorkingBytes);
    }

    private static int Delta(int coefficient, int sample) => (int)Math.Floor(coefficient * sample / 32.0);
    private static int Modulo(int sample) => (sample % 256 + 256) % 256;
}
