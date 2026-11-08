namespace Lokad.Image.Internal;

internal sealed partial class JpegHuffman
{
    internal const int BaselinePairBytes = 4096 * sizeof(uint) + 24;
    internal uint[]? BaselinePairs { get; private set; }

    internal bool CanPrepareBaselinePairs
    {
        get
        {
            var first = 32; var second = 32;
            for (var length = 1; length <= 16; length++)
            for (var index = 0; index < counts[length]; index++)
            {
                var symbol = symbols[firstSymbol[length] + index]; var size = symbol & 15;
                if (symbol == 0) second = Math.Min(second, length);
                else if (size is >= 1 and <= 7)
                {
                    first = Math.Min(first, length + size);
                    second = Math.Min(second, length + size);
                }
            }
            return first + second <= 12;
        }
    }

    // Scan-owned T.81 F.2.2 pairs. Bits0..3 consumed,4..7 first zero run,
    // 8..12 second position offset,16..23/24..31 signed amplitudes.
    // Zero descriptor is a miss; zero second amplitude denotes EOB.
    internal void PrepareBaselinePairs(OperationContext context)
    {
        if (BaselinePairs is not null) throw new InvalidOperationException("JPEG pairs already have a scan owner.");
        context.Cancellation.ThrowIfCancellationRequested();
        if (!CanPrepareBaselinePairs) return;
        context.Reserve(BaselinePairBytes);
        try
        {
            var pairs = new uint[4096];
            for (var prefix = 0; prefix < pairs.Length; prefix++)
            {
                if ((prefix & 63) == 0) context.Cancellation.ThrowIfCancellationRequested();
                var cache = (ulong)(uint)prefix << 52;
                var first = LookupBaselinePrefix(cache, 12);
                var firstCount = first & 15; var firstSymbol = first >> 4; var firstSize = firstSymbol & 15;
                if (firstCount == 0 || firstSize is < 1 or > 7) continue;
                var firstWidth = firstCount + firstSize;
                if (firstWidth >= 12) continue;
                var secondCache = cache << firstWidth;
                var second = LookupBaselinePrefix(secondCache, 12 - firstWidth);
                var secondCount = second & 15; var secondSymbol = second >> 4; var secondSize = secondSymbol & 15;
                if (secondCount == 0 || secondSymbol != 0 && secondSize is < 1 or > 7) continue;
                var width = firstWidth + secondCount + secondSize;
                if (width > 12) continue;
                var firstValue = JpegEntropyReader.ReadHighAlignedSigned(cache << firstCount, firstSize);
                var secondValue = secondSymbol == 0 ? 0
                    : JpegEntropyReader.ReadHighAlignedSigned(secondCache << secondCount, secondSize);
                var run = firstSymbol >> 4; var next = run + 1 + (secondSymbol >> 4);
                pairs[prefix] = (uint)width | (uint)run << 4 | (uint)next << 8
                    | (uint)unchecked((byte)firstValue) << 16 | (uint)unchecked((byte)secondValue) << 24;
            }
            BaselinePairs = pairs;
        }
        catch { context.Release(BaselinePairBytes); throw; }
    }

    internal void ReleaseBaselinePairs(OperationContext context)
    {
        if (BaselinePairs is null) return;
        BaselinePairs = null;
        context.Release(BaselinePairBytes);
    }
}
