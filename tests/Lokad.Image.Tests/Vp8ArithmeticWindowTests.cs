using Lokad.Image.Internal;
using Lokad.Image.Tooling;

namespace Lokad.Image.Tests;

public sealed partial class Vp8CoefficientBlockTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void MaximumRenormalizationKeepsEachCursorAndUnreadTail(bool live)
    {
        using var cancellation = new CancellationTokenSource();
        using var root = new TestMemoryBudget(0);
        var token = live ? cancellation.Token : default;
        using var context = new OperationContext(new() { MemoryBudget = root }, token);
        for (var phase = 0; phase < 8; phase++)
        {
            var data = new byte[273];
            new byte[] { 0xD3, 0x5A, 0xC7, 0x19, 0x88 }.CopyTo(data, 268);
            var saved = data.ToArray();
            var actual = Reader(data, 0, phase, 128, 0); var expected = actual;
            Assert.True(live ? actual.CanReadCancellableCoefficientBlock(data, context)
                : actual.CanReadCoefficientBlock(data, context));
            var window = actual.BeginWindow(data);
            ReadOnlySpan<byte> input = data;
            // Probability zero and a zero code consume seven bits every time.
            // The nonzero unread tail must survive maximum normalization/refill.
            for (var i = 0; i < 304; i++)
            {
                var bit = i % 2 == 0
                    ? Vp8BooleanReader.CancellableBlockReader.Read(in input, 0, token,
                        ref window.Position, ref window.Bits, ref window.Range, ref window.Value)
                    : Vp8BooleanReader.CancellableBlockReader.TokenRead(in input, 0, token,
                        ref window.Position, ref window.Bits, ref window.Range, ref window.Value);
                Assert.Equal(expected.Read(data, 0, context), bit);
                var checkpoint = actual; window.Publish(ref checkpoint);
                Assert.Equal(State(expected), State(checkpoint));
            }
            window.Publish(ref actual);
            for (var i = 0; i < 31; i++)
            {
                Assert.Equal(expected.Read(data, 128, context), actual.Read(data, 128, context));
                Assert.Equal(State(expected), State(actual));
            }
            Assert.Equal(saved, data);
        }
        Assert.Equal(0, root.GrowthAttempts); Assert.Equal(0, root.Bytes); Assert.False(root.IsDisposed);
        Assert.True(cancellation.Token.CanBeCanceled);
    }
}
