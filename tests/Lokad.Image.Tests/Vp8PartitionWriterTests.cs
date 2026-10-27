using System.Buffers.Binary;
using System.Security.Cryptography;
using Lokad.Image.Internal;
using Lokad.Image.Tooling;

namespace Lokad.Image.Tests;

public sealed class Vp8PartitionWriterTests
{
    [Fact]
    public void AllProbabilitiesAndOriginalSeededSymbolsRetainCompleteBytesAndOwnership()
    {
        using var root = new TestMemoryBudget(1_000_000);
        using var context = new OperationContext(new() { MemoryBudget = root }, default);
        using var hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
        Span<byte> length = stackalloc byte[4]; uint state = 0x31415926;
        for (var sequence = 0; sequence < 8192; sequence++)
        {
            using var writer = new Vp8PartitionWriter(context, 4096);
            var cursor = writer.Borrow();
            try { for (var p = 0; p < 128; p++)
            {
                state ^= state << 13; state ^= state >> 17; state ^= state << 5;
                var bit = (int)(state >> 31); var probability = (int)(state & 255);
                if (sequence < 512) { bit = sequence & 1; probability = sequence >> 1; }
                cursor.Write(bit != 0, (byte)probability);
            }
            } finally { cursor.Commit(); }
            writer.Finish(); BinaryPrimitives.WriteInt32LittleEndian(length, writer.Bytes.Length);
            hash.AppendData(length); hash.AppendData(writer.Bytes);
            Assert.Equal(writer.CapacityBytes, root.Bytes);
            writer.Dispose(); Assert.Equal(0, root.Bytes); Assert.False(root.IsDisposed);
        }
        // Frozen from the original bit-by-bit integer definition, independently
        // checked against the fifteen native controls below. No upstream corpus.
        Assert.Equal("67a28cdc34771e134794754305c2c1727e88b7746566c00bb4beafddcdc384b2",
            Convert.ToHexStringLower(hash.GetHashAndReset()));
    }

    private static (int Bit, int Probability) Symbol(string name, int index) => name.StartsWith("mixed", StringComparison.Ordinal)
        ? ((index * 13 + (index >> 2)) & 1, (index * 73 + 19 * (index >> 3)) & 255)
        : name == "pending-ff-carry" ? index < 512 ? (1, 255) : (0, 0) : (0, 128);

    private static void Feed(Vp8PartitionWriter writer, string name, int count)
    {
        var cursor = writer.Borrow();
        try { for (var p = 0; p < count; p++) { var symbol = Symbol(name, p); cursor.Write(symbol.Bit != 0, (byte)symbol.Probability); } }
        finally { cursor.Commit(); }
    }

    private static void Literal(Vp8PartitionWriter writer, int value, int count)
    {
        var cursor = writer.Borrow();
        try { cursor.Literal(value, count); }
        finally { cursor.Commit(); }
    }

    [Fact]
    public void CommittedFragmentsPreserveNativeCarryTerminationAndRetainedCapacity()
    {
        using var root = new TestMemoryBudget(1_000_000);
        using var context = new OperationContext(new() { MemoryBudget = root }, default);
        foreach (var (name, count, bytes, expected) in new[]
        {
            ("mixed", 4096, 731, "1b0e79ec837012005e5e2168c4b5bafefd0f36dc7a4e93195980b5d21dc36cdb"),
            ("pending-ff-carry", 520, 457, "05fe09c31628d786b3d9b798fda832381dbdccdbb1f90b2fa7a9e75b799bfc52")
        })
        foreach (var fragment in new[] { 1, 7, 16, 31, 64, 255 })
        {
            using var writer = new Vp8PartitionWriter(context, bytes);
            for (var start = 0; start < count; start += fragment)
            {
                var cursor = writer.Borrow();
                try
                {
                    for (var i = start; i < Math.Min(start + fragment, count); i++)
                    {
                        var symbol = Symbol(name, i);
                        cursor.Write(symbol.Bit != 0, (byte)symbol.Probability);
                    }
                }
                finally { cursor.Commit(); }
                Assert.Equal(writer.CapacityBytes, root.Bytes);
            }
            writer.Finish();
            Assert.Equal(expected, Convert.ToHexStringLower(SHA256.HashData(writer.Bytes)));
            Assert.Equal(bytes, writer.Memory.Length);
            writer.Dispose(); Assert.Equal(0, root.Bytes); Assert.False(root.IsDisposed);
        }
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
        Assert.Throws<InvalidOperationException>(() => { writer.Borrow(); }); writer.Dispose(); writer.Dispose();
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
    public void LocalAndOutputLimitsRemainDistinctAndInvalidLiteralsDoNotChangeTheStream()
    {
        using var root = new TestMemoryBudget(1_000_000); using var limited = new OperationContext(new() { MemoryBudget = root, MaxWorkingBytes = 191 }, default);
        using var writer = new Vp8PartitionWriter(limited, 731);
        var error = Assert.Throws<ImageLimitException>(() => Feed(writer, "mixed", 1));
        Assert.Equal((ImageLimitKind.WorkingBytes, ImageLimitScope.Operation), (error.Limit, error.Scope)); Assert.Equal(0, root.Bytes); Assert.Equal(0, root.Refusals);
        using var context = new OperationContext(new() { MemoryBudget = root }, default); using var tiny = new Vp8PartitionWriter(context, 1);
        Assert.Equal(ImageLimitKind.OutputBytes, Assert.Throws<ImageLimitException>(() => tiny.Finish()).Limit); tiny.Dispose(); Assert.Equal(0, root.Bytes);
        using var next = new Vp8PartitionWriter(context, 2); Assert.Throws<ArgumentOutOfRangeException>(() => Literal(next, 1, 0));
        Assert.Throws<ArgumentOutOfRangeException>(() => Literal(next, 0, 32)); next.Finish(); Assert.Equal(new byte[2], next.Bytes.ToArray());
    }

    [Fact]
    public void OutputLimitsAtFragmentAndGrowthBoundariesReleaseOnlyOwnedStorage()
    {
        using var root = new TestMemoryBudget(1_000_000);
        foreach (var (name, symbols, length) in new[] { ("mixed", 4096, 731), ("pending-ff-carry", 520, 457) })
        foreach (var limit in new[] { 0, 1, 2, 63, 64, 65, 127, 128, 129, 255, 256, 257, 456, 457, 730 })
        {
            if (limit >= length) continue;
            using var context = new OperationContext(new() { MemoryBudget = root }, default);
            using var writer = new Vp8PartitionWriter(context, limit);
            var error = Assert.Throws<ImageLimitException>(() => { Feed(writer, name, symbols); writer.Finish(); });
            Assert.Equal(ImageLimitKind.OutputBytes, error.Limit); Assert.Equal(writer.CapacityBytes, root.Bytes);
            writer.Dispose(); writer.Dispose(); Assert.Equal(0, root.Bytes); Assert.False(root.IsDisposed);
        }
        using var later = new OperationContext(new() { MemoryBudget = root }, default);
        using var next = new Vp8PartitionWriter(later, 731); Feed(next, "mixed", 4096); next.Finish();
        Assert.Equal("1b0e79ec837012005e5e2168c4b5bafefd0f36dc7a4e93195980b5d21dc36cdb",
            Convert.ToHexStringLower(SHA256.HashData(next.Bytes)));
        next.Dispose(); Assert.Equal(0, root.Bytes); Assert.Equal(0, root.Refusals);
    }
}
