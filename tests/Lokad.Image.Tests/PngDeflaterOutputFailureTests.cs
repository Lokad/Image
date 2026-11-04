using System.Security.Cryptography;
using Lokad.Image.Internal;
using Lokad.Image.Tooling;

namespace Lokad.Image.Tests;

public sealed class PngDeflaterOutputFailureTests
{
    [Theory]
    [InlineData(1, 0)] [InlineData(1, 1)] [InlineData(1, 2)]
    [InlineData(6, 0)] [InlineData(6, 1)] [InlineData(6, 2)]
    [InlineData(9, 0)] [InlineData(9, 1)] [InlineData(9, 2)]
    public void OutputFailuresPreserveTheAcceptedPrefixAndReleaseActualUsers(int level, int mode)
    {
        var input = OriginalDeflateMatch.Create(false); var original = input.ToArray();
        var expectedHash = level == 1 ? "69e71b5130919c68443efb049041b9046d46271e15f4861b0538637e38a25527"
            : level == 6 ? "4f018a3e97ec49f78ddb73365caace2d6f8012beb026ffcb6033f38f11148ab7"
            : "d264b218480edf0d84609f1fc2c1297d7eff25730239cf1fdf916f1921086fc8";
        using var root = new TestMemoryBudget(PngDeflater.WorkspaceBytes);
        using var reference = new ObservedOutput(root);
        using (var context = new OperationContext(new() { MemoryBudget = root }, default))
        using (var encoder = new PngDeflater(reference, context, level))
        { encoder.Write(input); encoder.Complete(); }
        var complete = reference.Accepted.ToArray();
        Assert.Equal(expectedHash, Convert.ToHexStringLower(SHA256.HashData(complete)));
        Assert.True(reference.Chunks.Count > 3); Assert.Equal(0, root.Bytes);

        foreach (var target in new[] { 1, 2, reference.Chunks.Count - 1 })
        {
            using var cancel = new CancellationTokenSource();
            using var output = new ObservedOutput(root, reference.Chunks, target, mode, cancel);
            using (var context = new OperationContext(new() { MemoryBudget = root }, cancel.Token))
            using (var encoder = new PngDeflater(output, context, level))
            {
                var error = Record.Exception(() => { encoder.Write(input); encoder.Complete(); });
                if (mode == 2) Assert.Equal(cancel.Token, Assert.IsType<OperationCanceledException>(error).CancellationToken);
                else Assert.Same(output.Failure, error);
                Assert.Equal(target, output.Writes); encoder.Dispose(); encoder.Dispose();
                Assert.Equal(target, output.Writes); Assert.Equal(0, root.Bytes);
            }
            var acceptedLength = reference.Chunks.Take(target - 1).Sum(c => c.Length)
                + (mode == 0 ? 0 : mode == 1 ? Math.Min(17, reference.Chunks[target - 1].Length) : reference.Chunks[target - 1].Length);
            Assert.Equal(complete.AsSpan(0, acceptedLength).ToArray(), output.Accepted.ToArray());
            Assert.False(root.IsDisposed); Assert.True(output.CanWrite); Assert.Equal(original, input);
        }

        using var nextContext = new OperationContext(new() { MemoryBudget = root }, default);
        using var nextOutput = new MemoryStream();
        using (var next = new PngDeflater(nextOutput, nextContext, level)) { next.Write(input); next.Complete(); }
        Assert.Equal(complete, nextOutput.ToArray()); Assert.Equal(0, root.Bytes); Assert.False(root.IsDisposed);
    }

    private sealed class ObservedOutput(TestMemoryBudget root, List<byte[]>? reference = null,
        int target = 0, int mode = 0, CancellationTokenSource? cancellation = null) : Stream
    {
        internal MemoryStream Accepted { get; } = new();
        internal List<byte[]> Chunks { get; } = [];
        internal int Writes { get; private set; }
        internal IOException Failure { get; } = new("Authored output failure at an actual write boundary.");
        public override void Write(ReadOnlySpan<byte> data)
        {
            Assert.Equal(PngDeflater.WorkspaceBytes, root.Bytes); Assert.False(root.IsDisposed);
            if (reference is not null) Assert.True(data.SequenceEqual(reference[Writes]));
            Writes++; Chunks.Add(data.ToArray());
            if (Writes == target && mode == 0) throw Failure;
            if (Writes == target && mode == 1)
            { Accepted.Write(data[..Math.Min(17, data.Length)]); throw Failure; }
            Accepted.Write(data);
            if (Writes == target && mode == 2) cancellation!.Cancel();
        }
        public override void Write(byte[] data, int offset, int count) => Write(data.AsSpan(offset, count));
        public override bool CanRead => false;
        public override bool CanSeek => false;
        public override bool CanWrite => true;
        public override long Length => throw new NotSupportedException();
        public override long Position { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }
        public override void Flush() => throw new NotSupportedException();
        public override int Read(byte[] data, int offset, int count) => throw new NotSupportedException();
        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
        public override void SetLength(long value) => throw new NotSupportedException();
        protected override void Dispose(bool disposing) { if (disposing) Accepted.Dispose(); base.Dispose(disposing); }
    }
}
