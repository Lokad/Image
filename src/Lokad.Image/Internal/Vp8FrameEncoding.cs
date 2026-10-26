using System.Buffers.Binary;

namespace Lokad.Image.Internal;

// Two independently owned arithmetic partitions, without a joined encoded-frame
// allocation. Confined to the enclosing operation; await delivery before disposal.
// The public encoder retains these owners through actual output delivery.
internal sealed class Vp8FrameEncoding : IDisposable
{
    private readonly OperationContext context;
    private readonly int width, height;
    private Vp8PartitionWriter? header, tokens;
    private long metadata;
    private bool padded;
    internal int Length { get; private set; }
    internal int WebpLength => checked(20 + Length);
    internal long CapacityBytes => metadata + (header?.CapacityBytes ?? 0) + (tokens?.CapacityBytes ?? 0);
    private Vp8FrameEncoding(Vp8FramePlan frame, OperationContext context) { width = frame.Width; height = frame.Height; this.context = context; }

    internal static Vp8FrameEncoding Create(Vp8FramePlan frame, OperationContext context)
    {
        context.Cancellation.ThrowIfCancellationRequested(); var result = new Vp8FrameEncoding(frame, context);
        try
        {
            context.Reserve(512); result.metadata = 512; context.Cancellation.ThrowIfCancellationRequested();
            result.header = new(context, context.Limits.MaxOutputBytes); result.tokens = new(context, context.Limits.MaxOutputBytes);
            var scratch = frame.Columns * 13L + 1024; context.Reserve(scratch);
            try { context.Cancellation.ThrowIfCancellationRequested(); result.Encode(frame); }
            finally { context.Release(scratch); }
            result.header.Finish(); result.tokens.Finish();
            if (result.header!.Bytes.Length >= 524288) throw new ImageLimitException(ImageLimitKind.BufferLength, "VP8 first partition exceeds its 19-bit format limit.");
            var length = 10L + result.header!.Bytes.Length + result.tokens!.Bytes.Length;
            if (20 + length + (length & 1) > context.Limits.MaxOutputBytes)
                throw new ImageLimitException(ImageLimitKind.OutputBytes, "Encoded WebP exceeds its output limit.");
            // The pinned container counts the final zero inside the VP8 chunk,
            // rather than advertising an odd chunk followed by RIFF padding.
            result.padded = (length & 1) != 0; result.Length = checked((int)(length + (length & 1))); return result;
        }
        catch { result.Dispose(); throw; }
    }

    private static void Bit(Vp8PartitionWriter writer, bool value, int probability = 128) => writer.Write(value ? 1 : 0, probability);
    private static void Signed(Vp8PartitionWriter writer, int value, int width)
    {
        Bit(writer, value != 0); if (value != 0) writer.Literal((Math.Abs(value) << 1) | (value < 0 ? 1 : 0), width + 1);
    }
    private static int Geometry(int mode) => mode switch { 4 => 5, 5 => 6, 6 => 4, _ => mode };

    private static void SmallMode(Vp8PartitionWriter writer, int mode, int top, int left)
    {
        var p = Vp8Tables.BlockModeProbabilities.Slice((Geometry(top) * 10 + Geometry(left)) * 9, 9);
        Bit(writer, mode != 0, p[0]); if (mode == 0) return;
        Bit(writer, mode != 1, p[1]); if (mode == 1) return;
        Bit(writer, mode != 2, p[2]); if (mode == 2) return;
        Bit(writer, mode >= 6, p[3]);
        if (mode < 6) { Bit(writer, mode != 3, p[4]); if (mode != 3) Bit(writer, mode == 5, p[5]); }
        else { Bit(writer, mode != 6, p[6]); if (mode != 6) { Bit(writer, mode != 7, p[7]); if (mode != 7) Bit(writer, mode == 9, p[8]); } }
    }

    private static int Band(int position) => position switch
    { 0 => 0, 1 => 1, 2 => 2, 3 => 3, 5 => 4, 6 => 5, 15 => 7, _ => 6 };

    private static ReadOnlySpan<byte> Category(int category) => category switch
    {
        0 => [159], 1 => [165, 145], 2 => [173, 148, 140], 3 => [176, 155, 140, 135],
        4 => [180, 157, 141, 134, 130], _ => [254, 254, 243, 230, 196, 177, 153, 140, 133, 130, 129]
    };

    private static bool Coefficients(Vp8PartitionWriter writer, ReadOnlySpan<short> levels, int type, int first, int initialContext, ReadOnlySpan<byte> probabilities)
    {
        var last = -1; for (var n = first; n < 16; n++) if (levels[n] != 0) last = n;
        var at = ((type * 8 + Band(first)) * 3 + initialContext) * 11;
        Bit(writer, last >= 0, probabilities[at]); if (last < 0) return false;
        for (var n = first; n <= last; n++)
        {
            var level = Math.Abs((int)levels[n]); Bit(writer, level != 0, probabilities[at + 1]);
            if (level == 0) { at = ((type * 8 + Band(n + 1)) * 3) * 11; continue; }
            Bit(writer, level > 1, probabilities[at + 2]);
            if (level > 1)
            {
                Bit(writer, level > 4, probabilities[at + 3]);
                if (level <= 4) { Bit(writer, level != 2, probabilities[at + 4]); if (level != 2) Bit(writer, level == 4, probabilities[at + 5]); }
                else
                {
                    Bit(writer, level > 10, probabilities[at + 6]);
                    if (level <= 10) Bit(writer, level > 6, probabilities[at + 7]);
                    else { Bit(writer, level >= 35, probabilities[at + 8]); Bit(writer, level >= (level < 35 ? 19 : 67), probabilities[at + (level < 35 ? 9 : 10)]); }
                    var category = 0; var start = 5; var bits = 1;
                    while (category < 5 && level >= start + (1 << bits)) { start += 1 << bits; bits++; category++; }
                    var p = Category(category); var remainder = level - start;
                    for (var b = 0; b < p.Length; b++) Bit(writer, (remainder & (1 << (p.Length - 1 - b))) != 0, p[b]);
                }
            }
            Bit(writer, levels[n] < 0);
            if (n < 15) { at = ((type * 8 + Band(n + 1)) * 3 + Math.Min(level, 2)) * 11; Bit(writer, n < last, probabilities[at]); }
        }
        return true;
    }

    private void Encode(Vp8FramePlan frame)
    {
        var h = header!; var t = tokens!; var p = frame.SegmentProbabilities;
        h.Write(0); h.Write(0); Bit(h, frame.SegmentCount > 1);
        if (frame.SegmentCount > 1)
        {
            Bit(h, frame.UpdateMap); h.Write(1); h.Write(1); // Updated, absolute segment features.
            foreach (var q in frame.Quantizers) Signed(h, q, 7);
            foreach (var level in frame.FilterLevels) Signed(h, level, 6);
            if (frame.UpdateMap)
            {
                Span<byte> map = stackalloc byte[3] { p.Root, p.Left, p.Right };
                foreach (var probability in map) { Bit(h, probability != 255); if (probability != 255) h.Literal(probability, 8); }
            }
        }
        var maximumFilter = 0; foreach (var level in frame.FilterLevels) maximumFilter = Math.Max(maximumFilter, level);
        h.Write(0); h.Literal(maximumFilter, 6); h.Literal(0, 3); h.Write(0); // Strong filter, sharpness0, no mode/ref deltas.
        h.Literal(0, 2); h.Literal(frame.Quantizers[0], 7);
        var deltas = frame.Deltas; Signed(h, deltas.LumaDc, 4); Signed(h, deltas.WalshDc, 4); Signed(h, deltas.WalshAc, 4); Signed(h, deltas.ChromaDc, 4); Signed(h, deltas.ChromaAc, 4);
        h.Write(0); // No subsequent frame's entropy refresh.
        var probabilities = frame.Probabilities;
        for (var i = 0; i < 1056; i++) { var changed = probabilities[i] != Vp8Tables.CoefficientProbabilities[i]; Bit(h, changed, Vp8Tables.CoefficientUpdateProbabilities[i]); if (changed) h.Literal(probabilities[i], 8); }
        Bit(h, frame.UseSkipProbability);
        if (frame.UseSkipProbability) h.Literal(frame.SkipProbability, 8);
        var modeTop = new byte[frame.Columns * 4]; var coefficientTop = new byte[frame.Columns * 9];
        Span<byte> modeLeft = stackalloc byte[4]; Span<byte> contexts = stackalloc byte[18];
        for (var index = 0; index < frame.Count; index++)
        {
            context.Cancellation.ThrowIfCancellationRequested(); var x = index % frame.Columns;
            if (x == 0) { modeLeft.Clear(); contexts[9..].Clear(); }
            var block = frame.Block(index); var modes = frame.SmallModes(index);
            if (frame.UpdateMap) { Bit(h, block.Segment >= 2, p.Root); Bit(h, (block.Segment & 1) != 0, block.Segment < 2 ? p.Left : p.Right); }
            if (frame.UseSkipProbability) Bit(h, block.Skipped, frame.SkipProbability);
            Bit(h, block.Intra16, 145);
            if (block.Intra16)
            {
                var branch = block.LumaMode is 1 or 3; Bit(h, branch, 156); Bit(h, branch ? block.LumaMode == 1 : block.LumaMode == 2, branch ? 128 : 163);
                modeTop.AsSpan(x * 4, 4).Fill((byte)block.LumaMode); modeLeft.Fill((byte)block.LumaMode);
            }
            else
            {
                for (var by = 0; by < 4; by++) for (var bx = 0; bx < 4; bx++)
                {
                    var mode = modes[by * 4 + bx]; SmallMode(h, mode, modeTop[x * 4 + bx], modeLeft[by]); modeTop[x * 4 + bx] = modeLeft[by] = mode;
                }
            }
            var uv = block.ChromaMode; Bit(h, uv != 0, 142); if (uv != 0) { Bit(h, uv != 2, 114); if (uv != 2) Bit(h, uv == 1, 183); }
            coefficientTop.AsSpan(x * 9, 9).CopyTo(contexts); var top = contexts[..9]; var left = contexts.Slice(9, 9); var levels = frame.Levels(index);
            if (frame.UseSkipProbability && block.Skipped)
            {
                ResetAfterSkip(contexts, block.Intra16); top.CopyTo(coefficientTop.AsSpan(x * 9, 9)); continue;
            }
            if (block.Intra16) top[8] = left[8] = Coefficients(t, levels[..16], 1, 0, top[8] + left[8], probabilities) ? (byte)1 : (byte)0;
            for (var by = 0; by < 4; by++) for (var bx = 0; bx < 4; bx++)
                top[bx] = left[by] = Coefficients(t, levels.Slice(16 + (by * 4 + bx) * 16, 16), block.Intra16 ? 0 : 3, block.Intra16 ? 1 : 0, top[bx] + left[by], probabilities) ? (byte)1 : (byte)0;
            for (var channel = 0; channel < 2; channel++) for (var by = 0; by < 2; by++) for (var bx = 0; bx < 2; bx++)
            {
                var a = 4 + channel * 2 + bx; var b = 4 + channel * 2 + by;
                top[a] = left[b] = Coefficients(t, levels.Slice(272 + (channel * 4 + by * 2 + bx) * 16, 16), 2, 0, top[a] + left[b], probabilities) ? (byte)1 : (byte)0;
            }
            top.CopyTo(coefficientTop.AsSpan(x * 9, 9));
        }
    }

    internal static void ResetAfterSkip(Span<byte> contexts, bool intra16)
    {
        if (contexts.Length < 18) throw new ArgumentException("Incomplete VP8 coefficient contexts.");
        contexts[..8].Clear(); contexts.Slice(9, 8).Clear();
        if (intra16) contexts[8] = contexts[17] = 0;
    }

    private void FrameHeader(Span<byte> bytes)
    {
        var tag = (header!.Bytes.Length << 5) | 16; bytes[0] = (byte)tag; bytes[1] = (byte)(tag >> 8); bytes[2] = (byte)(tag >> 16);
        bytes[3] = 0x9d; bytes[4] = 1; bytes[5] = 0x2a;
        BinaryPrimitives.WriteUInt16LittleEndian(bytes[6..], (ushort)width); BinaryPrimitives.WriteUInt16LittleEndian(bytes[8..], (ushort)height);
    }

    internal void WriteFrame(Stream output)
    {
        if (metadata == 0) throw new ObjectDisposedException(nameof(Vp8FrameEncoding));
        Span<byte> bytes = stackalloc byte[10]; FrameHeader(bytes); context.Cancellation.ThrowIfCancellationRequested(); output.Write(bytes);
        context.Cancellation.ThrowIfCancellationRequested(); output.Write(header!.Bytes); context.Cancellation.ThrowIfCancellationRequested(); output.Write(tokens!.Bytes); context.Cancellation.ThrowIfCancellationRequested();
        if (padded) { context.Cancellation.ThrowIfCancellationRequested(); output.WriteByte(0); }
    }

    internal void WriteOpaqueWebp(Stream output)
    {
        if (metadata == 0) throw new ObjectDisposedException(nameof(Vp8FrameEncoding));
        Span<byte> bytes = stackalloc byte[20]; "RIFF"u8.CopyTo(bytes); BinaryPrimitives.WriteUInt32LittleEndian(bytes[4..], (uint)(WebpLength - 8));
        "WEBPVP8 "u8.CopyTo(bytes[8..]); BinaryPrimitives.WriteUInt32LittleEndian(bytes[16..], (uint)Length);
        context.Cancellation.ThrowIfCancellationRequested(); output.Write(bytes); WriteFrame(output);
    }

    internal async ValueTask WriteFrameAsync(Stream output)
    {
        if (metadata == 0) throw new ObjectDisposedException(nameof(Vp8FrameEncoding));
        context.Cancellation.ThrowIfCancellationRequested(); var bytes = new byte[10]; FrameHeader(bytes);
        await output.WriteAsync(bytes, context.Cancellation).ConfigureAwait(false);
        context.Cancellation.ThrowIfCancellationRequested(); await output.WriteAsync(header!.Memory, context.Cancellation).ConfigureAwait(false);
        context.Cancellation.ThrowIfCancellationRequested(); await output.WriteAsync(tokens!.Memory, context.Cancellation).ConfigureAwait(false); context.Cancellation.ThrowIfCancellationRequested();
        if (padded) { context.Cancellation.ThrowIfCancellationRequested(); await output.WriteAsync(new byte[1], context.Cancellation).ConfigureAwait(false); }
    }

    internal async ValueTask WriteOpaqueWebpAsync(Stream output)
    {
        if (metadata == 0) throw new ObjectDisposedException(nameof(Vp8FrameEncoding));
        context.Cancellation.ThrowIfCancellationRequested(); var bytes = new byte[20]; "RIFF"u8.CopyTo(bytes); BinaryPrimitives.WriteUInt32LittleEndian(bytes.AsSpan(4), (uint)(WebpLength - 8));
        "WEBPVP8 "u8.CopyTo(bytes.AsSpan(8)); BinaryPrimitives.WriteUInt32LittleEndian(bytes.AsSpan(16), (uint)Length);
        await output.WriteAsync(bytes, context.Cancellation).ConfigureAwait(false); await WriteFrameAsync(output).ConfigureAwait(false);
        context.Cancellation.ThrowIfCancellationRequested();
    }

    internal int WebpLengthWithAlpha(Vp8AlphaEncoding alpha)
    {
        if (metadata == 0) throw new ObjectDisposedException(nameof(Vp8FrameEncoding));
        if (alpha.Width != width || alpha.Height != height) throw new ArgumentException("Alpha dimensions do not match.", nameof(alpha));
        var length = 46L + alpha.Bytes.Length + (alpha.Bytes.Length & 1) + Length;
        if (length > context.Limits.MaxOutputBytes)
            throw new ImageLimitException(ImageLimitKind.OutputBytes, "Encoded WebP exceeds its output limit.");
        return checked((int)length);
    }

    private void AlphaHeader(Span<byte> bytes, Vp8AlphaEncoding alpha)
    {
        bytes.Clear(); "RIFF"u8.CopyTo(bytes); BinaryPrimitives.WriteUInt32LittleEndian(bytes[4..], (uint)(WebpLengthWithAlpha(alpha) - 8));
        "WEBPVP8X"u8.CopyTo(bytes[8..]); bytes[16] = 10; bytes[20] = 16;
        var w = width - 1; var h = height - 1;
        for (var i = 0; i < 3; i++) { bytes[24 + i] = (byte)(w >> (8 * i)); bytes[27 + i] = (byte)(h >> (8 * i)); }
        "ALPH"u8.CopyTo(bytes[30..]); BinaryPrimitives.WriteUInt32LittleEndian(bytes[34..], (uint)alpha.Bytes.Length);
    }

    internal void WriteWebp(Stream output, Vp8AlphaEncoding alpha)
    {
        Span<byte> bytes = stackalloc byte[38]; AlphaHeader(bytes, alpha); context.Cancellation.ThrowIfCancellationRequested(); output.Write(bytes);
        context.Cancellation.ThrowIfCancellationRequested(); output.Write(alpha.Bytes);
        if ((alpha.Bytes.Length & 1) != 0) { context.Cancellation.ThrowIfCancellationRequested(); output.WriteByte(0); }
        "VP8 "u8.CopyTo(bytes); BinaryPrimitives.WriteUInt32LittleEndian(bytes[4..], (uint)Length);
        context.Cancellation.ThrowIfCancellationRequested(); output.Write(bytes[..8]); WriteFrame(output);
    }

    internal async ValueTask WriteWebpAsync(Stream output, Vp8AlphaEncoding alpha)
    {
        context.Cancellation.ThrowIfCancellationRequested(); var bytes = new byte[38]; AlphaHeader(bytes, alpha); context.Cancellation.ThrowIfCancellationRequested();
        await output.WriteAsync(bytes, context.Cancellation).ConfigureAwait(false);
        context.Cancellation.ThrowIfCancellationRequested(); await output.WriteAsync(alpha.Memory, context.Cancellation).ConfigureAwait(false);
        if ((alpha.Memory.Length & 1) != 0) { context.Cancellation.ThrowIfCancellationRequested(); await output.WriteAsync(new byte[1], context.Cancellation).ConfigureAwait(false); }
        "VP8 "u8.CopyTo(bytes); BinaryPrimitives.WriteUInt32LittleEndian(bytes.AsSpan(4), (uint)Length);
        context.Cancellation.ThrowIfCancellationRequested(); await output.WriteAsync(bytes.AsMemory(0, 8), context.Cancellation).ConfigureAwait(false);
        await WriteFrameAsync(output).ConfigureAwait(false); context.Cancellation.ThrowIfCancellationRequested();
    }

    public void Dispose()
    {
        header?.Dispose(); header = null; tokens?.Dispose(); tokens = null;
        var bytes = Interlocked.Exchange(ref metadata, 0); if (bytes != 0) context.Release(bytes);
    }
}
