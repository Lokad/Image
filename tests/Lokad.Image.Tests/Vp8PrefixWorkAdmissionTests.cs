using Lokad.Image.Internal;
using Lokad.Image.Tooling;

namespace Lokad.Image.Tests;

public sealed class Vp8PrefixWorkAdmissionTests
{
    private const int Blocks = 65;
    private static Vp8Planes Planes() => new(13 * 16, 5 * 16, 13 * 16,
        new byte[Blocks * 256], new byte[Blocks * 64], new byte[Blocks * 64]);
    private const long PrefixEligibility = Blocks * (16L * 6 * 192 + 4 * 512L + 24L * 512 + 64)
        + 64 * 24L * 256 + 1056 * 16 + Blocks * 24L;

    [Fact]
    public void PrefixEligibilityIsLocalAndDoesNotExtendTheActualWorkAllowance()
    {
        using var root = new TestMemoryBudget(65536);
        using var context = new OperationContext(new ImageLimits { MaxWork = PrefixEligibility, MemoryBudget = root }, default);
        using (var model = Vp8Encoder.CoefficientModel.TryCreate(Planes(), Blocks, 6, 4, context, fullFrame: true))
        {
            Assert.NotNull(model); Assert.Equal(64, model.CacheMacroblocks); Assert.False(model.Intra16);
            Assert.Equal(24064, root.Bytes); Assert.Equal(PrefixEligibility, context.RemainingWork);
            Assert.Equal(ImageLimitKind.Work, Assert.Throws<ImageLimitException>(() => context.Work(PrefixEligibility + 1)).Limit);
            Assert.Equal(PrefixEligibility, context.RemainingWork);
        }
        Assert.Equal(0, root.Bytes); Assert.False(root.IsDisposed);
    }

    [Fact]
    public void InsufficientLocalEligibilityOrUnrequestedPolicyMakesNoSharedReservation()
    {
        using var root = new TestMemoryBudget(65536);
        using var below = new OperationContext(new ImageLimits { MaxWork = PrefixEligibility - 1, MemoryBudget = root }, default);
        Assert.Null(Vp8Encoder.CoefficientModel.TryCreate(Planes(), Blocks, 6, 4, below, fullFrame: true));
        using var unrequested = new OperationContext(new ImageLimits { MaxWork = PrefixEligibility, MemoryBudget = root }, default);
        Assert.Null(Vp8Encoder.CoefficientModel.TryCreate(Planes(), Blocks, 6, 4, unrequested));
        Assert.Equal(0, root.Attempts); Assert.Equal(0, root.Bytes);
    }
}
