namespace Lokad.Image.Internal;

// Unicode 17 Arabic joining, applied to logical same-level runs before GSUB.
// https://www.unicode.org/versions/Unicode17.0.0/core-spec/chapter-9/#G7480
internal static class ArabicJoining
{
    internal const uint Isolated = 1, Initial = 2, Medial = 4, Final = 8;
    internal const uint FormMask = Isolated | Initial | Medial | Final;
    private static bool Forward(JoiningType type) => type is JoiningType.L or JoiningType.D or JoiningType.C;
    private static bool Backward(JoiningType type) => type is JoiningType.R or JoiningType.D or JoiningType.C;

    internal static void Apply(ShapingBuffer glyphs, int start = 0, int length = -1)
    {
        var context = glyphs.Context;
        if (start < 0 || start > glyphs.Count || length < -1 || length > glyphs.Count - start)
            throw new ArgumentOutOfRangeException(nameof(length));
        var end = length < 0 ? glyphs.Count : start + length; context.Cancellation.ThrowIfCancellationRequested();
        var previous = -1; var previousType = JoiningType.U; byte previousLevel = 0;
        for (var i = start; i < end; i++)
        {
            context.Cancellation.ThrowIfCancellationRequested(); ref var glyph = ref glyphs[i]; glyph.Features &= ~FormMask;
            if (glyph.Glyph < 0 || glyph.Kind is 2 or 3)
            { previous = -1; previousType = JoiningType.U; continue; }
            var type = UnicodeTextProperties.Joining(glyph.Scalar);
            if (previous >= 0 && previousLevel != glyph.Level) { previous = -1; previousType = JoiningType.U; }
            if (type == JoiningType.T) continue;
            if (type is JoiningType.R or JoiningType.L or JoiningType.D) glyph.Features |= Isolated;
            if (previous >= 0 && Forward(previousType) && Backward(type))
            {
                // A forward connection upgrades the preceding final/isolated
                // glyph; a backward connection upgrades the current isolated.
                if (previousType is JoiningType.R or JoiningType.L or JoiningType.D)
                {
                    ref var left = ref glyphs[previous];
                    left.Features = (left.Features & ~FormMask) | ((left.Features & Final) != 0 ? Medial : Initial);
                }
                if (type is JoiningType.R or JoiningType.L or JoiningType.D)
                    glyph.Features = (glyph.Features & ~FormMask) | Final;
            }
            previous = i; previousType = type; previousLevel = glyph.Level;
        }
    }
}
