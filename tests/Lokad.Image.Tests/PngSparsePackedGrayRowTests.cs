using System.Buffers.Binary;
using Lokad.Image.Internal;

namespace Lokad.Image.Tests;

public sealed class PngSparsePackedGrayRowTests
{
    [Theory]
    [InlineData(1)] [InlineData(2)] [InlineData(4)]
    public void SparseGrayKeepsAllSamplesFullWordKeysHiddenRgbPaddingHolesAndGuards(int depth)
    {
        var maximum = (1 << depth) - 1;
        foreach (var width in new[] { 0, 1, 7, 8, 9, 513, 4097 })
        foreach (var step in new[] { 2, 4, 8 })
        foreach (var key in new[] { -1, 0, maximum, 0x100 })
        {
            var indices = Enumerable.Range(0, width).Select(x => (x * 7 + x / 13) & maximum).ToArray();
            var bytes = (width * depth + 7) / 8; var row = new byte[bytes + 2]; row[0] = row[^1] = 0xa5;
            for (var x = 0; x < width; x++) row[1 + x * depth / 8] |= (byte)(indices[x] << (8 - depth - x * depth % 8));
            var used = width * depth % 8; if (used != 0) row[bytes] |= (byte)((1 << (8 - used)) - 1);
            byte[] transparency = key < 0 ? [] : new byte[2];
            if (key >= 0) BinaryPrimitives.WriteUInt16BigEndian(transparency, (ushort)key);
            var original = row.ToArray(); var originalTransparency = transparency.ToArray();
            var length = width == 0 ? 0 : ((width - 1) * step + 1) * 4;
            var actual = Enumerable.Repeat((byte)0xa5, length + 14).ToArray(); var expected = actual.ToArray();
            for (var x = 0; x < width; x++)
            {
                var gray = (byte)(indices[x] * 255 / maximum); var at = 7 + x * step * 4;
                expected[at] = expected[at + 1] = expected[at + 2] = gray;
                expected[at + 3] = indices[x] == key ? (byte)0 : (byte)255;
            }
            PngCodec.CopySparsePackedGrayRow(row.AsSpan(1, bytes), actual.AsSpan(7, length), width, depth, step, transparency, default);
            Assert.Equal(expected, actual); Assert.Equal(original, row); Assert.Equal(originalTransparency, transparency);
        }
    }

    [Fact]
    public void CancellationPrecedesInputAccessAndLeavesSparsePixelsUntouched()
    {
        var output = Enumerable.Repeat((byte)0xa5, 4097 * 8).ToArray();
        var token = new CancellationToken(true);
        var error = Assert.Throws<OperationCanceledException>(() =>
            PngCodec.CopySparsePackedGrayRow([], output, 4097, 1, 2, [], token));
        Assert.Equal(token, error.CancellationToken); Assert.All(output, value => Assert.Equal((byte)0xa5, value));
    }
}
