using Lokad.Image.Internal;
namespace Lokad.Image.Tests;
public sealed class SelfRowStorageTests
{
    [Theory]
    [InlineData(-1)] [InlineData(1)]
    public void SharedBackingKeepsSnapshotAndEveryEndByteExact(int direction)
    {
        foreach (var length in Enumerable.Range(256, 770).Concat(new[] { 2055, 2056, 2057, 4096, 32768 }))
        {
            var stride = length + 73; var storage = new byte[stride * 5 + 29];
            for (var i = 0; i < storage.Length; i++) storage[i] = (byte)(i * 53 + (i >> 4));
            var before = storage.ToArray(); var expected = before.ToArray();
            var sy = direction > 0 ? 0 : 1; var dy = direction > 0 ? 1 : 0;
            var sx = direction > 0 ? 7 : 11; var dx = direction > 0 ? 11 : 7;
            for (var row = 0; row < 3; row++)
                Array.Copy(before, (sy + row) * stride + sx, expected, (dy + row) * stride + dx, length);
            var first = direction > 0 ? 2 : 0; var step = direction > 0 ? -stride : stride;
            IndependentRowCopy.CopySelfRows(storage, (sy + first) * stride + sx,
                (dy + first) * stride + dx, length, 3, step, default);
            Assert.Equal(expected, storage);
            before.AsSpan().CopyTo(storage); using var canceled = new CancellationTokenSource(); canceled.Cancel();
            Assert.ThrowsAny<OperationCanceledException>(() => IndependentRowCopy.CopySelfRows(storage,
                (sy + first) * stride + sx, (dy + first) * stride + dx, length, 3, step, canceled.Token));
            Assert.Equal(before, storage);
        }
    }
}
