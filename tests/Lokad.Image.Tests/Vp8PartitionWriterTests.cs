using System.Security.Cryptography;
using Lokad.Image.Internal;
using Lokad.Image.Tooling;

namespace Lokad.Image.Tests;

public sealed class Vp8PartitionWriterTests
{
    private static (int Bit, int Probability) Symbol(string name, int index) => name.StartsWith("mixed", StringComparison.Ordinal)
        ? ((index * 13 + (index >> 2)) & 1, (index * 73 + 19 * (index >> 3)) & 255)
        : name == "pending-ff-carry" ? index < 512 ? (1, 255) : (0, 0) : (0, 128);

    private static void Feed(Vp8PartitionWriter writer, string name, int count)
    {
        for (var p = 0; p < count; p++) { var symbol = Symbol(name, p); writer.Write(symbol.Bit, symbol.Probability); }
    }

    // Original symbol controls; native observations are frozen independently.
    [Theory]
    [InlineData("mixed-prefix-0", 0, 2, "96a296d224f285c67bee93c30f8a309157f0daa35dc5b87e410b78630a09cfc7")]
    [InlineData("mixed-prefix-1", 1, 3, "709e80c88487a2411e1ee4dfb9f22a861492d20c4765150c0c794abd70f8147c")]
    [InlineData("mixed-prefix-7", 7, 4, "69e10340beca1b78703080ac90724c8b5ae7e0853c02e8119cee880d6737aaaa")]
    [InlineData("mixed-prefix-8", 8, 4, "69e10340beca1b78703080ac90724c8b5ae7e0853c02e8119cee880d6737aaaa")]
    [InlineData("mixed-prefix-15", 15, 5, "dd6ee4acc4ecb827ebe24fa9cbd5eeb46eb1822f5a814976832dc39dd07364d2")]
    [InlineData("mixed-prefix-16", 16, 5, "dd6ee4acc4ecb827ebe24fa9cbd5eeb46eb1822f5a814976832dc39dd07364d2")]
    [InlineData("mixed-prefix-31", 31, 9, "939662c684a84d1693d2f6ad204553f70e96c28aac5846ef391a5d74508c05e4")]
    [InlineData("mixed-prefix-255", 255, 48, "bec2856158bd54599900048f85f6fd3e24a1b22e43d34952f2b2257fd94dfcef")]
    [InlineData("mixed-prefix-4096", 4096, 731, "1b0e79ec837012005e5e2168c4b5bafefd0f36dc7a4e93195980b5d21dc36cdb")]
    [InlineData("uniform-prefix-1", 1, 3, "709e80c88487a2411e1ee4dfb9f22a861492d20c4765150c0c794abd70f8147c")]
    [InlineData("uniform-prefix-2", 2, 3, "709e80c88487a2411e1ee4dfb9f22a861492d20c4765150c0c794abd70f8147c")]
    [InlineData("uniform-prefix-8", 8, 3, "709e80c88487a2411e1ee4dfb9f22a861492d20c4765150c0c794abd70f8147c")]
    [InlineData("uniform-prefix-9", 9, 3, "709e80c88487a2411e1ee4dfb9f22a861492d20c4765150c0c794abd70f8147c")]
    [InlineData("uniform-zero", 512, 66, "efbb03b7a7f6fd3c29391d4d0281e1830a85caadd831c3f04716faca4107a42e")]
    [InlineData("pending-ff-carry", 520, 457, "05fe09c31628d786b3d9b798fda832381dbdccdbb1f90b2fa7a9e75b799bfc52")]
    public void CanonicalBytesMatchNativeAndFitItsExactOutputLimit(string name, int symbols, int bytes, string expected)
    {
        using var root = new TestMemoryBudget(1_000_000); using var context = new OperationContext(new() { MemoryBudget = root }, default);
        using var writer = new Vp8PartitionWriter(context, bytes); Feed(writer, name, symbols); writer.Finish();
        Assert.Equal(bytes, writer.Bytes.Length); Assert.Equal(expected, Convert.ToHexStringLower(SHA256.HashData(writer.Bytes)));
        Assert.Equal(writer.CapacityBytes, root.Bytes); Assert.Equal(bytes, writer.Memory.Length);
        Assert.Throws<InvalidOperationException>(() => writer.Write(0)); writer.Dispose(); writer.Dispose();
        Assert.Equal(0, root.Bytes); Assert.False(root.IsDisposed); Assert.Throws<InvalidOperationException>(() => writer.Memory.Length);
    }

    [Fact]
    public void AggregateRefusalIsTerminalAndDisposalReleasesRetainedCapacity()
    {
        using var root = new TestMemoryBudget(1_000_000); using var caller = root.CreateScope();
        Assert.True(caller.TryReserve(999_000, out var held)); using var context = new OperationContext(new() { MemoryBudget = root }, default);
        using var writer = new Vp8PartitionWriter(context, 731);
        var error = Assert.Throws<ImageLimitException>(() => Feed(writer, "mixed", 4096));
        Assert.Equal((ImageLimitKind.WorkingBytes, ImageLimitScope.SharedBudget), (error.Limit, error.Scope)); Assert.Equal(1, root.Refusals);
        Assert.Equal(999_000 + writer.CapacityBytes, root.Bytes); writer.Dispose(); Assert.Equal(999_000, root.Bytes); held.Dispose();
        using var later = new OperationContext(new() { MemoryBudget = root }, default); using var next = new Vp8PartitionWriter(later, 731);
        Feed(next, "mixed", 4096); next.Finish(); next.Dispose(); Assert.Equal(0, root.Bytes); Assert.False(root.IsDisposed);
    }

    [Fact]
    public void CancellationAfterReplacementAdmissionRefundsTheUncommittedBuffer()
    {
        using var root = new TestMemoryBudget(1_000_000); using var cancel = new CancellationTokenSource();
        using var context = new OperationContext(new() { MemoryBudget = root }, cancel.Token); using var writer = new Vp8PartitionWriter(context, 731);
        root.Admitted = count => { if (count > 192) cancel.Cancel(); };
        Assert.Throws<OperationCanceledException>(() => Feed(writer, "mixed", 4096));
        Assert.Equal(192, writer.CapacityBytes); Assert.Equal(192, root.Bytes); Assert.Equal(448, root.Peak);
        writer.Dispose(); Assert.Equal(0, root.Bytes); root.Admitted = null;
        using var later = new OperationContext(new() { MemoryBudget = root }, default); using var next = new Vp8PartitionWriter(later, 731);
        Feed(next, "mixed", 4096); next.Finish(); next.Dispose(); Assert.Equal(0, root.Bytes);
    }

    [Fact]
    public void WorkFailureDuringReplacementCopyRefundsNewCapacityAndKeepsOldOwnership()
    {
        long work;
        using (var sizing = new OperationContext(new(), default))
        using (var writer = new Vp8PartitionWriter(sizing, 731))
        {
            for (var p = 0; writer.CapacityBytes <= 192; p++) { var symbol = Symbol("mixed", p); writer.Write(symbol.Bit, symbol.Probability); }
            work = sizing.ConsumedWork;
        }
        using var root = new TestMemoryBudget(1_000_000); using var context = new OperationContext(new() { MemoryBudget = root, MaxWork = work - 1 }, default);
        using var failed = new Vp8PartitionWriter(context, 731);
        Assert.Equal(ImageLimitKind.Work, Assert.Throws<ImageLimitException>(() => Feed(failed, "mixed", 4096)).Limit);
        Assert.Equal(192, failed.CapacityBytes); Assert.Equal(192, root.Bytes); Assert.Equal(448, root.Peak);
        failed.Dispose(); Assert.Equal(0, root.Bytes);
    }

    [Fact]
    public void LocalAndOutputLimitsRemainDistinctAndInvalidSymbolsDoNotChangeTheStream()
    {
        using var root = new TestMemoryBudget(1_000_000); using var limited = new OperationContext(new() { MemoryBudget = root, MaxWorkingBytes = 191 }, default);
        using var writer = new Vp8PartitionWriter(limited, 731);
        var error = Assert.Throws<ImageLimitException>(() => writer.Write(0, 19));
        Assert.Equal((ImageLimitKind.WorkingBytes, ImageLimitScope.Operation), (error.Limit, error.Scope)); Assert.Equal(0, root.Bytes); Assert.Equal(0, root.Refusals);
        using var context = new OperationContext(new() { MemoryBudget = root }, default); using var tiny = new Vp8PartitionWriter(context, 1);
        Assert.Equal(ImageLimitKind.OutputBytes, Assert.Throws<ImageLimitException>(() => tiny.Finish()).Limit); tiny.Dispose(); Assert.Equal(0, root.Bytes);
        using var next = new Vp8PartitionWriter(context, 2); Assert.Throws<ArgumentOutOfRangeException>(() => next.Write(2));
        Assert.Throws<ArgumentOutOfRangeException>(() => next.Write(0, 256)); next.Finish(); Assert.Equal(new byte[2], next.Bytes.ToArray());
    }
}
