namespace Lokad.Image.Tests;

public sealed class FontInputAdmissionTests
{
    [Fact]
    public void ManagedArrayCeilingIsTypedBeforeAnyOwnedAllocation()
    {
        var limits = new ImageLimits { MaxInputBytes = int.MaxValue, MaxWorkingBytes = long.MaxValue};
        var failure = Assert.Throws<ImageLimitException>(() => TrueTypeFont.InputContext(Array.MaxLength + 1, limits, default));
        Assert.Equal(ImageLimitKind.BufferLength, failure.Limit);
        failure = Assert.Throws<ImageLimitException>(() => TrueTypeFont.InputContext(Array.MaxLength + 1,
            limits with { MaxInputBytes = Array.MaxLength }, default));
        Assert.Equal(ImageLimitKind.InputBytes, failure.Limit);
    }

    [Fact]
    public void MaximumLengthAdmissionUsesLongArithmeticWithoutAllocatingTheFont()
    {
        var limits = new ImageLimits { MaxInputBytes = int.MaxValue, MaxWorkingBytes = 2L * Array.MaxLength + 256};
        var context = TrueTypeFont.InputContext(Array.MaxLength, limits, default);
        Assert.Equal(0, context.RemainingWorkingBytes);         var failure = Assert.Throws<ImageLimitException>(() => TrueTypeFont.InputContext(Array.MaxLength,
            limits with { MaxWorkingBytes = limits.MaxWorkingBytes - 1 }, default));
        Assert.Equal(ImageLimitKind.WorkingBytes, failure.Limit);
    }
}
