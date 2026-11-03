using System.Runtime.InteropServices;
using Lokad.Image.Internal;
using Lokad.Image.Tooling;

namespace Lokad.Image.Tests;

public sealed class Vp8lTraceStorageTests
{
    [Theory]
    [InlineData(27306, 2, false)]
    [InlineData(27307, 7, true)]
    public void PhysicalCapacityBoundaryPreservesDenseLiteralPathAndIndependentOutput(int length, int width, bool borrowed)
    {
        using var root = new TestMemoryBudget(4_000_000);
        using var context = new OperationContext(new() { MemoryBudget = root }, default);
        var pixels = Enumerable.Range(0, length).Select(i => unchecked(0xff000000u + (uint)i * 2654435761u)).ToArray();
        var matches = new uint[length];
        using var seed = new Vp8lTokenBuffer(context, length);
        foreach (var pixel in pixels) seed.Add(Vp8lToken.Literal(pixel));
        var expected = seed.Tokens.ToArray();
        var output = new Vp8lTokenBuffer(context, length);
        output.BorrowScratchWords(context, length * 2).Fill(-1);
        var before = root.Bytes; long stateBytes = 0;
        root.Admitted = _ => stateBytes = root.Bytes - before;
        using var trace = Vp8lReferenceLz.Trace(pixels, matches, width, seed.Tokens, context, scratch: output);
        // At this boundary, separate path/window storage is over 200 KiB.
        // Borrowing must be selected by physical capacity, rather than admission.
        Assert.Equal(borrowed, stateBytes < 100_000);
        Assert.Same(output, trace); Assert.Equal(before, root.Bytes);
        Assert.True(trace.Tokens.SequenceEqual(expected)); Assert.True(seed.Tokens.SequenceEqual(expected));
        Assert.All(matches, x => Assert.Equal(0u, x));
        seed.Dispose(); Assert.Equal(length * 8L, root.Bytes);
        Assert.True(trace.Tokens.SequenceEqual(expected));
        trace.Dispose(); trace.Dispose(); Assert.Equal(0, root.Bytes); Assert.False(root.IsDisposed);
    }

    [Theory]
    [InlineData(0)] [InlineData(1)] [InlineData(2)]
    public void TraceRejectsAliasedIdleCapacityBeforeOverwritingAnyInput(int alias)
    {
        const int length = 27307;
        using var root = new TestMemoryBudget(4_000_000);
        using var context = new OperationContext(new() { MemoryBudget = root }, default);
        using var output = new Vp8lTokenBuffer(context, length);
        var words = output.BorrowScratchWords(context, length * 2);
        words.Fill(unchecked((int)0xff123456)); var original = words.ToArray();
        var pixels = new uint[length]; var matches = new uint[length];
        var seed = new[] { Vp8lToken.Literal(0) };
        void Run()
        {
            var input = output.BorrowScratchWords(context, length * 2);
            Vp8lReferenceLz.Trace(
                alias == 0 ? MemoryMarshal.Cast<int, uint>(input)[..length] : pixels,
                alias == 1 ? MemoryMarshal.Cast<int, uint>(input)[..length] : matches,
                7, alias == 2 ? MemoryMarshal.Cast<int, Vp8lToken>(input) : seed, context, scratch: output);
        }
        var error = Assert.Throws<ArgumentException>(Run);
        Assert.Equal("seed", error.ParamName);
        Assert.True(words.SequenceEqual(original)); Assert.Equal(0, root.Bytes);
        Assert.False(root.IsDisposed);
        using var next = new Vp8lTokenBuffer(context, length);
        Assert.Equal(length * 8L, root.Bytes);
    }

    [Theory]
    [InlineData(0)] [InlineData(1)] [InlineData(3079)] [InlineData(8197)]
    public void TailCommitPreservesOrderThroughOverlappingMovesAndIndependentLifetime(int first)
    {
        const int length = 8197;
        using var root = new TestMemoryBudget(1_000_000);
        using var context = new OperationContext(new() { MemoryBudget = root }, default);
        using var foreign = new OperationContext(new() { MemoryBudget = root }, default);
        using var output = new Vp8lTokenBuffer(context, length);
        var storage = MemoryMarshal.Cast<int, Vp8lToken>(output.BorrowScratchWords(context, length * 2));
        for (var i = 0; i < length; i++) storage[i] = Vp8lToken.Literal((uint)i);
        Assert.Throws<ArgumentException>(() => output.CommitTail(foreign, first));
        Assert.Throws<ArgumentOutOfRangeException>(() => output.CommitTail(context, length + 1));
        output.CommitTail(context, first);
        Assert.Equal(length - first, output.Count);
        for (var i = 0; i < output.Count; i++) Assert.Equal(Vp8lToken.Literal((uint)(first + i)), output.Tokens[i]);
        Assert.Equal(length * 8L, root.Bytes);
        if (output.Count != 0) Assert.Throws<ArgumentException>(() => output.CommitTail(context, 0));
        output.Dispose(); Assert.Equal(0, root.Bytes); Assert.False(root.IsDisposed);
    }

    [Fact]
    public void CanceledTailCommitDoesNotPublishPartialTokensAndReleasesCapacity()
    {
        using var root = new TestMemoryBudget(1_000_000);
        using var cancellation = new CancellationTokenSource();
        using var context = new OperationContext(new() { MemoryBudget = root }, cancellation.Token);
        using var output = new Vp8lTokenBuffer(context, 8197);
        output.BorrowScratchWords(context, 16394).Fill(-1);
        cancellation.Cancel();
        Assert.Throws<OperationCanceledException>(() => output.CommitTail(context, 1));
        Assert.Equal(0, output.Count); Assert.Equal(8197 * 8L, root.Bytes);
        output.Dispose(); Assert.Equal(0, root.Bytes);
        using var next = new OperationContext(new() { MemoryBudget = root }, default);
        using var tokens = new Vp8lTokenBuffer(next, 8197);
        tokens.Add(Vp8lToken.Literal(0xff123456)); Assert.Equal(1, tokens.Count);
    }
}
