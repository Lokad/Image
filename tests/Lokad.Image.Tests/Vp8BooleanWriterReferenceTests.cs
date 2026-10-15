using System.Reflection;
using Lokad.Image.Internal;

namespace Lokad.Image.Tests;

public sealed class Vp8BooleanWriterReferenceTests
{
    [Theory]
    [InlineData(false)] [InlineData(true)]
    public void CompiledFormatPathsPreserveScalarTreesAndConstrainedFailures(bool tokens)
    {
        // Owned format trees from RFC 6386 sections 11 and 13; the reference
        // rediscovers each leaf recursively and emits one bit at a time.
        sbyte[] tree = tokens ? [-12, 2, -1, 4, -2, 6, 8, 12, -3, 10, -4, -5, 14, 16, -6, -7, 18, 20, -8, -9, -10, -11]
            : [-1, 2, -2, 4, -3, 6, 8, 12, -4, 10, -6, -7, -5, 14, -8, 16, -9, -10];
        var leaves = tokens ? 12 : 10; var paths = Vp8BooleanWriter.CompileTree(tree, leaves);
        foreach (var root in tokens ? new[] { 0, 2 } : new[] { 0 })
        foreach (var pattern in new[] { 0, 1, 2, 3 })
        foreach (var maximum in new[] { 1, 64, 1024 })
        foreach (var work in new[] { 1, 9, 40, 1000, 10000 })
        {
            var probabilities = Enumerable.Range(0, leaves - 1).Select(i => (byte)(pattern switch
                { 0 => 0, 1 => 255, 2 => 128, _ => (i * 173 + 91) & 255 })).ToArray();
            var limits = ImageLimits.Default with { MaxWork = work };
            var originalContext = new OperationContext(limits, default); var packedContext = new OperationContext(limits, default);
            using var original = new ScalarWriter(originalContext, maximum);
            using var packed = new Vp8BooleanWriter(packedContext, maximum);
            string? failure = null;
            for (var i = 0; i < 128; i++)
            {
                var token = i % leaves; if (root == 2 && token == 11) continue;
                failure = Attempt(() => original.Tree(tree, probabilities, token, root));
                Assert.Equal(failure, Attempt(() => packed.Codeword(root == 2 ? paths[token] >> 5 : paths[token], probabilities)));
                Compare(); if (failure is not null) break;
            }
            if (failure is null) { Assert.Equal(Attempt(original.Finish), Attempt(packed.Finish)); Compare(); }
            void Compare()
            {
                Assert.Equal(State(original), State(packed));
                Assert.Equal(originalContext.RemainingWork, packedContext.RemainingWork);
                Assert.Equal(originalContext.RemainingWorkingBytes, packedContext.RemainingWorkingBytes);
            }
        }
    }

    [Theory]
    [InlineData(0, false)] [InlineData(1, false)] [InlineData(2, false)]
    [InlineData(63, false)] [InlineData(64, false)] [InlineData(65, false)]
    [InlineData(1024, false)] [InlineData(1024, true)]
    public void PackedNormalizationPreservesEveryScalarIntervalAndFailure(int maximum, bool cancel)
    {
        foreach (var work in new[] { 1, 3, 127, 1024, 10000 })
        foreach (var memory in new[] { 32, 95, 96, 159, 160, 1000000 })
        {
            using var cancellation = new CancellationTokenSource();
            var limits = ImageLimits.Default with { MaxWork = work, MaxWorkingBytes = memory };
            var originalContext = new OperationContext(limits, cancel ? cancellation.Token : default);
            var packedContext = new OperationContext(limits, cancel ? cancellation.Token : default);
            using var original = new ScalarWriter(originalContext, maximum);
            using var packed = new Vp8BooleanWriter(packedContext, maximum);
            uint random = 173; string? failure = null;
            for (var i = 0; i < 1024; i++)
            {
                if (cancel && i == 17) cancellation.Cancel();
                random ^= random << 13; random ^= random >> 17; random ^= random << 5;
                var bit = (int)(random & 1); var probability = i & 255;
                failure = Attempt(() => original.Write(bit, probability));
                Assert.Equal(failure, Attempt(() => packed.Write(bit, probability)));
                Compare(); if (failure is not null) break;
            }
            if (failure is null)
            {
                Assert.Equal(Attempt(original.Finish), Attempt(packed.Finish)); Compare();
            }
            void Compare()
            {
                Assert.Equal(State(original), State(packed));
                Assert.Equal(originalContext.RemainingWork, packedContext.RemainingWork);
                Assert.Equal(originalContext.RemainingWorkingBytes, packedContext.RemainingWorkingBytes);
            }
        }
    }

    private static string? Attempt(Action action)
    {
        try { action(); return null; }
        catch (Exception error) { return error.GetType().FullName + ": " + error.Message; }
    }

    private static string State(object writer)
    {
        object? Read(string name) => writer.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(writer);
        return string.Join('|', Read("position"), Read("used"), Read("bottom"), Read("range"), Read("finished"),
            Convert.ToHexString((byte[])Read("buffer")!));
    }

    // Original owned bit-at-a-time writer, retained as the failure/state oracle.
    private sealed class ScalarWriter(OperationContext context, int maximum) : IDisposable
    {
        private byte[] buffer = [];
        private int position, used, bottom, range = 255;
        private bool finished;
    
        public ReadOnlySpan<byte> Bytes => finished ? buffer.AsSpan(0, position) : throw new InvalidOperationException();
    
        public void Write(int bit, int probability = 128)
        {
            if (finished) throw new InvalidOperationException("Arithmetic partition is finished.");
            if ((uint)bit > 1 || (uint)probability > 255) throw new ArgumentOutOfRangeException(nameof(bit));
            context.Work(1);
            var boundary = 1 + ((range - 1) * probability >> 8);
            if (bit == 0) range = boundary;
            else
            {
                bottom += boundary; range -= boundary;
                if (bottom >= 256) { bottom -= 256; Carry(); }
            }
            while (range < 128)
            {
                Append(bottom >> 7); bottom = bottom * 2 & 255; range *= 2;
            }
        }
    
        public void Literal(int value, int count)
        {
            if ((uint)count > 24 || value < 0 || (uint)value >= 1u << count) throw new ArgumentOutOfRangeException(nameof(value));
            for (var bit = count - 1; bit >= 0; bit--) Write(value >> bit & 1);
        }
    
        public void Tree(ReadOnlySpan<sbyte> tree, ReadOnlySpan<byte> probabilities, int token, int root = 0)
        {
            // Small bounded format trees: discover the leaf path without an implementation code table.
            var path = 0; var depth = 0;
            if (!Find(tree, root, token, 0, 0, ref path, ref depth)) throw new ArgumentOutOfRangeException(nameof(token));
            var node = root;
            for (var bit = depth - 1; bit >= 0; bit--)
            {
                var branch = path >> bit & 1; Write(branch, probabilities[node / 2]); node = tree[node + branch];
            }
        }
    
        private static bool Find(ReadOnlySpan<sbyte> tree, int node, int token, int path, int depth, ref int result, ref int count)
        {
            if (node < 0)
            {
                if (-node - 1 != token) return false;
                result = path; count = depth; return true;
            }
            return Find(tree, tree[node], token, path * 2, depth + 1, ref result, ref count)
                || Find(tree, tree[node + 1], token, path * 2 + 1, depth + 1, ref result, ref count);
        }
    
        public void Finish()
        {
            if (finished) throw new InvalidOperationException("Arithmetic partition is finished.");
            context.Cancellation.ThrowIfCancellationRequested();
            for (var bit = 7; bit >= 0; bit--) Append(bottom >> bit & 1);
            while (used != 0) Append(0);
            // Zero lookahead for decoders with a wider internal arithmetic window.
            for (var bit = 0; bit < 16; bit++) Append(0);
            finished = true;
        }
    
        private void Append(int bit)
        {
            if (used == 0)
            {
                if (position == maximum || position == Array.MaxLength) throw new ImageLimitException("VP8 arithmetic partition exceeds its output limit.");
                if (position == buffer.Length) Grow();
                buffer[position++] = 0;
            }
            buffer[position - 1] |= (byte)(bit << (7 - used)); used = (used + 1) & 7;
        }
    
        private void Carry()
        {
            var index = position - 1; var increment = used == 0 ? 1 : 1 << (8 - used);
            while (index >= 0)
            {
                context.Work(1);
                var sum = buffer[index] + increment; buffer[index] = unchecked((byte)sum);
                if (sum < 256) return;
                index--; increment = 1;
            }
            throw new InvalidOperationException("Arithmetic interval escaped the initial range.");
        }
    
        private void Grow()
        {
            var size = (int)Math.Min(Math.Min(maximum, Array.MaxLength), Math.Max(64L, buffer.Length * 2L));
            context.Reserve(size + 32L);
            var next = new byte[size]; context.Work(position); buffer.AsSpan(0, position).CopyTo(next);
            if (buffer.Length != 0) context.Release(buffer.Length + 32L);
            buffer = next;
        }
    
        public void Dispose()
        {
            if (buffer.Length != 0) context.Release(buffer.Length + 32L);
            buffer = []; position = 0; finished = true;
        }
    }
}
