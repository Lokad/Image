using System.Numerics;
using System.Runtime.Intrinsics.X86;

namespace Lokad.Image.Internal;

internal static partial class CachedBicubicResize
{
    // Storage is owned by the enclosing operation or public plan, whose context
    // admits it before allocation. This value adds no separate owner or lease.
    internal readonly struct Workspace(double[] storage, int[] geometry, int capacity, int cacheWidth,
        bool wide, bool mixed, bool horizontalUp, bool identityHorizontal, long bytes)
    {
        internal readonly double[] Storage = storage;
        internal readonly int[] Geometry = geometry;
        internal readonly int Capacity = capacity, CacheWidth = cacheWidth;
        internal readonly bool Wide = wide, Mixed = mixed, HorizontalUp = horizontalUp,
            IdentityHorizontal = identityHorizontal;
        internal readonly long Bytes = bytes;
    }

    internal static bool TryPrepare(int sourceWidth, int sourceHeight, int bitDepth, int width, int height,
        double scaleX, long tapsX, long tapsY, OperationContext context, out Workspace workspace)
    {
        workspace = default;
        var support = Math.Max(tapsX, tapsY);
        if (bitDepth is not (8 or 16) || support > 2048) return false;
        var wide = support > 16;
        var capacity = !wide ? 16 : support > 128 ? (int)BitOperations.RoundUpToPowerOf2((uint)support)
            : support > 64 ? 128 : support > 32 ? 64 : 32;
        var heapMetadata = capacity > 128;
        var upsample = width > sourceWidth && height > sourceHeight;
        var mixed = width > sourceWidth && height < sourceHeight || width < sourceWidth && height > sourceHeight;
        var horizontalDown = width < sourceWidth && height == sourceHeight;
        var verticalDown = width == sourceWidth && height < sourceHeight;
        var horizontalUp = bitDepth == 8 && width > sourceWidth && height == sourceHeight;
        var verticalUp = width == sourceWidth && height > sourceHeight;
        if ((wide ? width >= sourceWidth || height >= sourceHeight
                : !upsample && !mixed && !horizontalDown && !verticalDown && !horizontalUp && !verticalUp
                    && (width >= sourceWidth || height >= sourceHeight))
            || width < 2 || tapsX > capacity || tapsY > capacity
            || !Avx512F.IsSupported || !Avx512DQ.IsSupported || !Avx2.IsSupported || !Sse41.IsSupported)
            return false;
        var identityHorizontal = verticalDown || verticalUp;
        var cacheWidth = (long)width + (mixed ? width & 1 : 0);
        var decodedElements = identityHorizontal ? 0 : sourceWidth * 4;
        var dataElements = cacheWidth * (capacity * 4) + (long)width * (capacity + 1) + decodedElements;
        var geometryElements = (long)width * (capacity + 1);
        var elements = dataElements + (heapMetadata ? capacity : 0);
        var indexElements = geometryElements + (heapMetadata ? 2 * capacity : 0);
        var bytes = elements * sizeof(double) + indexElements * sizeof(int) + (heapMetadata ? 0 : capacity * 8);
        if (elements > Array.MaxLength || indexElements > Array.MaxLength
            || bytes > context.RemainingWorkingBytes - ResizeAlpha.ScratchBytes)
            return false;
        context.Reserve(bytes);
        try
        {
            var storage = new double[(int)elements];
            var geometry = new int[(int)indexElements];
            var cacheElements = (int)(cacheWidth * (capacity * 4));
            var weights = storage.AsSpan(cacheElements, width * capacity);
            var normalizers = storage.AsSpan(cacheElements + width * capacity, width);
            var offsets = geometry.AsSpan(0, width * capacity);
            var counts = geometry.AsSpan(width * capacity, width);
            for (var x = 0; x < width; x++)
            {
                context.Cancellation.ThrowIfCancellationRequested();
                var center = (x + 0.5) * sourceWidth / width - 0.5;
                var first = (int)Math.Ceiling(center - 2 * scaleX);
                var count = (int)Math.Floor(center + 2 * scaleX) - first + 1;
                counts[x] = count;
                for (var t = 0; t < count; t++)
                {
                    var w = ImageTransforms.Kernel((first + t - center) / scaleX, ResizeFilter.Bicubic);
                    weights[x * capacity + t] = w; normalizers[x] += w;
                    offsets[x * capacity + t] = Math.Clamp(first + t, 0, sourceWidth - 1) * (wide ? 1 : 4);
                }
            }
            if (!wide && !identityHorizontal)
                PrepareOverlappingHorizontalPairs(offsets, counts, width, sourceWidth, context);
            if (wide) PrepareWideHorizontalPairs(offsets, counts, width, sourceWidth, capacity, context);
            if (wide && capacity == 32)
                PrepareWideHorizontalQuartets(weights, offsets, counts, width, sourceWidth, context);
            workspace = new(storage, geometry, capacity, (int)cacheWidth, wide, mixed, horizontalUp, identityHorizontal, bytes);
            return true;
        }
        catch { context.Release(bytes); throw; }
    }
}
