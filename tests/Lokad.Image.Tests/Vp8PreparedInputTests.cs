using System.Runtime.InteropServices;
using System.Security.Cryptography;
using Lokad.Image.Internal;
using Lokad.Image.Tooling;

namespace Lokad.Image.Tests;

public sealed class Vp8PreparedInputTests
{
    // Original controls, observed through libwebp1.6.0's public undithered
    // conversion API before transparent cleanup. No encoded oracle is an input.
    public static IEnumerable<object[]> Controls()
    {
        yield return ["opaque", 8, "f1e516c70c0d315b2f785b90fdd6a6c6e26e30eca898b7f1616380ab18aae715"];
        yield return ["photograph", 8, "b5e23d147ee602a9abeee06279c7f462c087b691a04d649ec8d349df7d9910a9"];
        yield return ["diagram", 8, "38d9463e671d1f131f62d637d7972a83f1d4f3c6b0657e2983d498ed1020089c"];
        yield return ["asymmetric", 8, "b77569bd20a7ce1b4e6877cb628a9977f7d49b10e250ce8cda9d16e50a83bf28"];
        yield return ["opaque", 16, "f1e516c70c0d315b2f785b90fdd6a6c6e26e30eca898b7f1616380ab18aae715"];
        yield return ["asymmetric", 16, "b77569bd20a7ce1b4e6877cb628a9977f7d49b10e250ce8cda9d16e50a83bf28"];
        yield return ["singleton", 8, "f8b2cdc0bd43d7e47a46cc40550af0cba0b5d3b8aeecbddf19dea2b18bbb4c00"];
        yield return ["two-color", 8, "9d51a3272e0dc3ec843b0ab1439848a0d49aebcb6bdd2a2dfbd7ecb8e02a93a8"];
    }

    internal static ImageBuffer Source(string name, int depth, TestMemoryBudget root)
    {
        if (depth != 16) return OriginalVp8lControls.Buffer(name, depth, root);
        var pixels = OriginalPixels.Generate(513, 259, 16);
        var words = MemoryMarshal.Cast<byte, ushort>(pixels.AsSpan());
        if (name == "opaque") for (var i = 3; i < words.Length; i += 4) words[i] = 65535;
        else words[3] = words[513 * 4 - 1] = 0;
        var result = ImageBuffer.Create(513, 259, 16, new() { MemoryBudget = root });
        for (var y = 0; y < result.Height; y++) words.Slice(y * 513 * 4, 513 * 4).CopyTo(result.GetRow16(y));
        return result;
    }

    internal static string Hash(Vp8PreparedInput input)
    {
        using var hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
        for (var y = 0; y < input.Height; y++) hash.AppendData(input.Y.Slice(y * input.Stride, input.Width));
        for (var y = 0; y < (input.Height + 1) / 2; y++) hash.AppendData(input.U.Slice(y * input.Stride / 2, (input.Width + 1) / 2));
        for (var y = 0; y < (input.Height + 1) / 2; y++) hash.AppendData(input.V.Slice(y * input.Stride / 2, (input.Width + 1) / 2));
        hash.AppendData(input.Alpha); return Convert.ToHexStringLower(hash.GetHashAndReset());
    }

    internal static void Padding(ReadOnlySpan<byte> plane, int stride, int width, int height)
    {
        for (var y = 0; y < plane.Length / stride; y++)
        for (var x = 0; x < stride; x++)
            Assert.Equal(plane[Math.Min(y, height - 1) * stride + Math.Min(x, width - 1)], plane[y * stride + x]);
    }

    [Theory]
    [MemberData(nameof(Controls))]
    public void OrdinaryInputConversionMatchesNativeSamplesAndRetainsOnlyOwnedCapacity(string name, int depth, string expected)
    {
        using var root = new TestMemoryBudget(16_000_000); using var source = Source(name, depth, root);
        var original = OriginalPixels.Bytes(source); var sourceBytes = root.Bytes;
        using var context = new OperationContext(new() { MemoryBudget = root }, default);
        context.ReserveBorrowed(sourceBytes);
        using var input = Vp8PreparedInput.Create(source, context);
        Assert.Equal(expected, Hash(input)); Assert.Equal(original, OriginalPixels.Bytes(source));
        Assert.Equal(sourceBytes + input.CapacityBytes, root.Bytes);
        var scratch = root.Peak - sourceBytes - input.CapacityBytes;
        Assert.True(scratch is 1024 or 4096);
        if (depth != 8 || input.HasAlpha || source.Width < 16) Assert.Equal(1024, scratch);
        Padding(input.Y, input.Stride, input.Width, input.Height);
        Padding(input.U, input.Stride / 2, (input.Width + 1) / 2, (input.Height + 1) / 2);
        Padding(input.V, input.Stride / 2, (input.Width + 1) / 2, (input.Height + 1) / 2);
        var alpha = original.Where((_, i) => depth == 8 && i % 4 == 3).ToArray();
        if (depth == 8 && input.HasAlpha) Assert.Equal(alpha, input.Alpha.ToArray());
        source.Dispose(); context.ReleaseBorrowed(sourceBytes);
        Assert.Equal(input.CapacityBytes, root.Bytes); Assert.Equal(expected, Hash(input));
        input.Dispose(); input.Dispose(); Assert.Equal(0, root.Bytes);
        Assert.Throws<ObjectDisposedException>(() => input.Y.Length); Assert.False(root.IsDisposed);
    }

    [Fact]
    public void OpaqueVectorScratchRefusalIsTerminalWithoutSelectingSmallerScalarWorkspace()
    {
        if (!System.Runtime.Intrinsics.X86.Avx512F.IsSupported || !System.Runtime.Intrinsics.X86.Avx512BW.IsSupported
            || !System.Runtime.Intrinsics.X86.Avx512Vbmi.IsSupported) return;
        using var source = OriginalVp8lControls.Buffer("opaque", 8);
        long capacity;
        using (var sizing = new OperationContext(ImageLimits.Default, default))
        using (var input = Vp8PreparedInput.Create(source, sizing)) capacity = input.CapacityBytes;
        using var root = new TestMemoryBudget(capacity + 4096); using var caller = root.CreateScope();
        Assert.True(caller.TryReserve(3072, out var held)); // The scalar workspace would fit.
        using var context = new OperationContext(new() { MemoryBudget = root }, default);
        var error = Assert.Throws<ImageLimitException>(() => Vp8PreparedInput.Create(source, context));
        Assert.Equal((ImageLimitKind.WorkingBytes, ImageLimitScope.SharedBudget), (error.Limit, error.Scope));
        Assert.Equal(3072, root.Bytes); Assert.Equal(1, root.Refusals);
        held.Dispose();
        using var next = Vp8PreparedInput.Create(source, context); Assert.Equal(capacity, root.Bytes);
        Assert.Equal(capacity + 4096, root.Peak);
        next.Dispose(); Assert.Equal(0, root.Bytes); Assert.False(root.IsDisposed);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void AggregateRefusalAtOutputOrScratchIsTerminalAndRecoversWithoutDisposingCaller(bool scratch)
    {
        using var source = OriginalVp8lControls.Buffer("asymmetric", 8);
        long capacity;
        using (var sizing = new OperationContext(ImageLimits.Default, default))
        using (var input = Vp8PreparedInput.Create(source, sizing)) capacity = input.CapacityBytes;
        using var root = new TestMemoryBudget(capacity + 1024); using var caller = root.CreateScope();
        var callerBytes = scratch ? 1 : 1025;
        Assert.True(caller.TryReserve(callerBytes, out var held));
        using var context = new OperationContext(new() { MemoryBudget = root }, default);
        var error = Assert.Throws<ImageLimitException>(() => Vp8PreparedInput.Create(source, context));
        Assert.Equal((ImageLimitKind.WorkingBytes, ImageLimitScope.SharedBudget), (error.Limit, error.Scope));
        Assert.Equal(callerBytes, root.Bytes); Assert.Equal(1, root.Refusals); held.Dispose(); Assert.Equal(0, root.Bytes);
        using var next = Vp8PreparedInput.Create(source, context); Assert.Equal(capacity, root.Bytes);
        next.Dispose(); Assert.Equal(0, root.Bytes); Assert.False(root.IsDisposed);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    public void CancellationAfterAdmissionReleasesStorageAndPermitsAnotherOperation(int growth)
    {
        using var source = OriginalVp8lControls.Buffer("asymmetric", 8);
        using var root = new TestMemoryBudget(16_000_000); using var cancel = new CancellationTokenSource();
        root.Admitted = _ => { if (root.GrowthAttempts == growth) cancel.Cancel(); };
        using var context = new OperationContext(new() { MemoryBudget = root }, cancel.Token);
        Assert.Throws<OperationCanceledException>(() => Vp8PreparedInput.Create(source, context));
        Assert.Equal(0, root.Bytes); root.Admitted = null;
        using var later = new OperationContext(new() { MemoryBudget = root }, default);
        using var next = Vp8PreparedInput.Create(source, later); Assert.NotEqual(0, root.Bytes);
        next.Dispose(); Assert.Equal(0, root.Bytes);
    }
}
