using System.Security.Cryptography;
using Lokad.Image.Internal;
using Lokad.Image.Tooling;
using static Lokad.Image.Tests.Vp8lReferencePredictorTests;

namespace Lokad.Image.Tests;

public sealed class Vp8lPreparedEncodingTests
{
    internal static (byte[] Pixels, int Width, int Height) Input(string name) => OriginalVp8lControls.Input(name);

    [Theory]
    [MemberData(nameof(OriginalVp8lControls.Cases), MemberType = typeof(OriginalVp8lControls))]
    public void GenericRoutingMatchesAllExistingSelectedPacketControls(string name, int effort, int bytes, string expected)
    {
        var source = Input(name); var original = source.Pixels.ToArray();
        using var root = new TestMemoryBudget(128_000_000);
        using var context = new OperationContext(new() { MemoryBudget = root }, default);
        var working = Working(source.Pixels, false, context);
        using var prepared = Vp8lPreparedEncoding.Prepare(working, source.Width, effort, context);
        Array.Clear(working); context.Release(working.Length * 4L); var retained = root.Bytes;
        using var first = new MemoryStream(); prepared.Write(first); var packet = first.ToArray();
        Assert.Equal(bytes, packet.Length); Assert.Equal(expected, Convert.ToHexStringLower(SHA256.HashData(packet)));
        Assert.Equal(retained, root.Bytes); Assert.Equal(original, source.Pixels);
        prepared.Dispose(); Assert.Equal(0, root.Bytes); Assert.True(first.CanWrite); Assert.False(root.IsDisposed);
        Assert.Equal(expected, Convert.ToHexStringLower(SHA256.HashData(packet)));
    }

    [Fact]
    public void SharedRefusalDuringCandidateComparisonDoesNotReturnAnEarlierCandidateAndRecoversAfterCallerRelease()
    {
        var source = Input("two-color"); long peak;
        using (var calibrationRoot = new TestMemoryBudget(10_000_000))
        using (var calibration = new OperationContext(new() { MemoryBudget = calibrationRoot }, default))
        {
            var pixels = Working(source.Pixels, false, calibration);
            using var prepared = Vp8lPreparedEncoding.Prepare(pixels, source.Width, 5, calibration); peak = calibrationRoot.Peak;
            prepared.Dispose(); calibration.Release(pixels.Length * 4L); Assert.Equal(0, calibrationRoot.Bytes);
        }
        using var root = new TestMemoryBudget(peak); using var caller = root.CreateScope();
        Assert.True(caller.TryReserve(1, out var held));
        using var context = new OperationContext(new() { MemoryBudget = root }, default);
        var working = Working(source.Pixels, false, context);
        var error = Assert.Throws<ImageLimitException>(() => Vp8lPreparedEncoding.Prepare(working, source.Width, 5, context));
        Assert.Equal((ImageLimitKind.WorkingBytes, ImageLimitScope.SharedBudget), (error.Limit, error.Scope));
        Assert.Equal(1, root.Refusals); Assert.Equal(1 + working.Length * 4L, root.Bytes);
        context.Release(working.Length * 4L); held.Dispose(); Assert.Equal(0, root.Bytes);
        var nextPixels = Working(source.Pixels, false, context);
        using var next = Vp8lPreparedEncoding.Prepare(nextPixels, source.Width, 5, context);
        using var output = new MemoryStream(); next.Write(output); Assert.Equal(46, output.Length);
        next.Dispose(); context.Release(nextPixels.Length * 4L); Assert.Equal(0, root.Bytes); Assert.False(root.IsDisposed);
    }
}
