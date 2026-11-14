using System.Security.Cryptography;
using Lokad.Image.Internal;
using Lokad.Image.Tooling;

namespace Lokad.Image.Tests;

public sealed partial class Vp8CoefficientBlockTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void ModeWindowMatchesScalarDecisionsEveryCursorAndFollowingReads(bool live)
    {
        var random = new Random(63861004);
        using var cancellation = new CancellationTokenSource();
        using var root = new TestMemoryBudget(0);
        var token = live ? cancellation.Token : default;
        using var context = new OperationContext(new() { MemoryBudget = root }, token);
        ReadOnlySpan<int> probabilities = [0, 1, 127, 128, 254, 255, 145, 156, 163, 142, 114, 183];
        for (var trial = 0; trial < 256; trial++)
        {
            var data = new byte[288]; random.NextBytes(data); var saved = data.ToArray();
            var range = 128 + trial % 128;
            var actual = Reader(data, trial % 12, trial % 8, range, random.Next(range)); var expected = actual;
            Assert.True(actual.CanReadModeWindow(data));
            var window = actual.BeginWindow(data);
            ReadOnlySpan<byte> input = data;
            for (var decision = 0; decision < 121; decision++)
            {
                var p = decision % 2 == 0 ? probabilities[(trial + decision) % probabilities.Length] : random.Next(256);
                var bit = Vp8BooleanReader.CancellableBlockReader.Read(in input, p, token,
                    ref window.Position, ref window.Bits, ref window.Range, ref window.Value);
                Assert.Equal(expected.Read(data, p, context), bit);
                var checkpoint = actual; window.Publish(ref checkpoint); Assert.Equal(State(expected), State(checkpoint));
            }
            window.Publish(ref actual);
            for (var i = 0; i < 19; i++)
            {
                var p = random.Next(256);
                Assert.Equal(expected.Read(data, p, context), actual.Read(data, p, context));
                Assert.Equal(State(expected), State(actual));
            }
            Assert.Equal(saved, data);
        }
        Assert.Equal(0, root.GrowthAttempts); Assert.Equal(0, root.Bytes); Assert.False(root.IsDisposed);
        Assert.True(cancellation.Token.CanBeCanceled);
    }

    [Fact]
    public void ModeWindowCannotBorrowLookaheadFromFollowingPartition()
    {
        var data = new byte[1024];
        var header = new Vp8BooleanReader(data, 0, 273); var before = State(header);
        Assert.True(header.CanReadModeWindow(data));
        Assert.False(header.CanReadModeWindow(data.AsSpan(0, 272)));
        Assert.False(new Vp8BooleanReader(data, 0, 272).CanReadModeWindow(data));
        Assert.False(Reader(data, 0, 7, 255, 255).CanReadModeWindow(data));
        Assert.Equal(before, State(header));
    }

    [Fact]
    public void ModeWindowLargePacketKeepsExactNormalLivePixelsAndBorrowedRoot()
    {
        using var source = OriginalPixels.Buffer(513, 259, 8, opaque: true);
        using var encoded = new MemoryStream();
        ImageCodec.EncodeWebp(source, encoded, new() { Lossless = false, Quality = 75, Effort = 4 });
        var bytes = encoded.ToArray();
        Assert.Equal(72380, bytes.Length);
        Assert.Equal("39eb4095d1aaebc21fee2a92aa45180fb6adcd92bc5538589f914f53aa9ed88f", Hash(bytes));
        const string expected = "d554b9166a0669ce581ede6b9197156c4041efa41dde182d9ffd61968fb950ac";
        using var root = new TestMemoryBudget(16 * 1024 * 1024);
        using var cancellation = new CancellationTokenSource();
        var options = new DecodeOptions { Limits = new() { MemoryBudget = root } };
        using var plain = ImageCodec.Decode(bytes, options);
        using var live = ImageCodec.Decode(bytes, options, cancellation.Token);
        Assert.Equal(expected, Hash(OriginalPixels.Bytes(plain)));
        Assert.Equal(expected, Hash(OriginalPixels.Bytes(live)));
        Assert.Equal(2 * 513L * 259 * 4, root.Bytes);
        cancellation.Cancel();
        var error = Assert.Throws<OperationCanceledException>(() => ImageCodec.Decode(bytes, options, cancellation.Token));
        Assert.Equal(cancellation.Token, error.CancellationToken); Assert.Equal(2 * 513L * 259 * 4, root.Bytes);
        plain.GetRow8(0)[0] ^= 1; plain.Dispose(); Array.Clear(bytes);
        Assert.Equal(expected, Hash(OriginalPixels.Bytes(live))); Assert.Equal(513L * 259 * 4, root.Bytes);
        live.Dispose(); Assert.Equal(0, root.Bytes);
        using (var following = ImageCodec.Decode(encoded.ToArray(), options))
            Assert.Equal(expected, Hash(OriginalPixels.Bytes(following)));
        Assert.Equal(0, root.Bytes); Assert.False(root.IsDisposed); Assert.True(cancellation.Token.CanBeCanceled);

        static string Hash(byte[] data) => Convert.ToHexStringLower(SHA256.HashData(data));
    }
}
