using SixLabors.Fonts;
using SOptions = SixLabors.Fonts.TextOptions;

// Correctness-only reference observations through public Fonts APIs. The chosen
// face-metrics line boxes match the explicit policy used by the twelve controls.
internal static class FittingReference
{
    internal sealed record Selection(double FontSize, int Candidates, double Width, double Height);
    internal static Selection? Find(FontFamily family, FontStyle style, Lokad.Image.TrueTypeFont face, string text,
        double maximum, double minimum, double step, double width, double height, double spacing)
    {
        var range = maximum - minimum;
        var intervals = range == 0 ? 0 : step >= range ? 1 : (int)Math.Ceiling(range / step);
        for (var index = 0; index <= intervals; index++)
        {
            var size = index == intervals ? minimum : Math.Max(minimum, maximum - index * step);
            var body = (face.Ascender - (double)face.Descender) * size / face.UnitsPerEm;
            var advance = (face.Ascender - (double)face.Descender + Math.Max(0, face.LineGap)) * size / face.UnitsPerEm * spacing;
            var options = new SOptions(family.CreateFont((float)size, style)) { Dpi = 72, HintingMode = HintingMode.None,
                Origin = new(0, (float)(-(advance - body) / 2)), KerningMode = KerningMode.Standard,
                TextDirection = SixLabors.Fonts.TextDirection.Auto, WordBreaking = WordBreaking.BreakWord,
                WrappingLength = (float)width, LineSpacing = (float)(advance / size),
                HorizontalAlignment = HorizontalAlignment.Left, VerticalAlignment = VerticalAlignment.Top };
            var recorder = new BoundsRecorder(); new TextRenderer(recorder).RenderText(text, options);
            var lines = TextMeasurer.CountLines(text, options);
            var logical = TextMeasurer.MeasureAdvance(text, options);
            var x = Math.Min(0, recorder.Left); var y = Math.Min(0, recorder.Top);
            var right = Math.Max(logical.Width, recorder.Right);
            var bottom = Math.Max(lines == 0 ? 0 : body + (lines - 1) * advance, recorder.Bottom);
            if (right - x <= width && bottom - y <= height) return new(size, index + 1, right - x, bottom - y);
            if (size == minimum) break;
        }
        return null;
    }
    private sealed class BoundsRecorder : IGlyphRenderer
    {
        internal double Left, Top, Right, Bottom;
        public bool BeginGlyph(in FontRectangle bounds, in GlyphRendererParameters parameters)
        {
            // Fonts reports advance-like synthetic rectangles for spaces. They
            // are represented by the logical box, rather than treated as ink.
            if (!System.Text.Rune.IsWhiteSpace(new(parameters.CodePoint.Value)))
            {
                Left = Math.Min(Left, bounds.Left); Top = Math.Min(Top, bounds.Top);
                Right = Math.Max(Right, bounds.Right); Bottom = Math.Max(Bottom, bounds.Bottom);
            }
            return false;
        }
        public void BeginText(in FontRectangle bounds) { } public void EndText() { }
        public void BeginFigure() { } public void EndFigure() { } public void EndGlyph() { }
        public void MoveTo(System.Numerics.Vector2 point) { } public void LineTo(System.Numerics.Vector2 point) { }
        public void QuadraticBezierTo(System.Numerics.Vector2 second, System.Numerics.Vector2 third) { }
        public void CubicBezierTo(System.Numerics.Vector2 second, System.Numerics.Vector2 third, System.Numerics.Vector2 fourth) { }
        public TextDecorations EnabledDecorations() => TextDecorations.None;
        public void SetDecoration(TextDecorations decoration, System.Numerics.Vector2 start, System.Numerics.Vector2 end, float thickness) { }
    }
}
