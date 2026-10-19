using System.Numerics;
using System.Globalization;
using System.Reflection;
using System.Security.Cryptography;
using System.Text.Json;
using Lokad.Image;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using LOptions = Lokad.Image.TextOptions;
using SOptions = SixLabors.Fonts.TextOptions;

const int width = 384, height = 160;
var root = args.Length > 0 ? Path.GetFullPath(args[0]) : Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../../../"));
var fontPath = args.Length > 1 ? Path.GetFullPath(args[1]) : Path.Combine(root, "external/text-reference/fonts");
var provenance=JsonDocument.Parse(File.ReadAllText(Path.Combine(root,"tools/TextReference/font-provenance.json")));
foreach(var entry in provenance.RootElement.GetProperty("files").EnumerateArray()) {
 var path=Path.Combine(fontPath,entry.GetProperty("file").GetString()!);
 if(new FileInfo(path).Length!=entry.GetProperty("bytes").GetInt64()
  || Convert.ToHexStringLower(SHA256.HashData(File.ReadAllBytes(path)))!=entry.GetProperty("sha256").GetString())
  throw new InvalidDataException("Reference font/provenance mismatch.");
}
var files = new[] { "DejaVuSans.ttf", "DejaVuSans-Bold.ttf", "DejaVuSans-Oblique.ttf" };
var collection = new FontCollection();
var families = files.Select(f => collection.Add(Path.Combine(fontPath, f))).ToArray();
var managedFonts = files.Select(f => TrueTypeFont.Load(File.ReadAllBytes(Path.Combine(fontPath, f)))).ToArray();
var cases = new[] {
 new Case("LatinKerning", "AV To"), new Case("MultilineEmptyCrLf", "AV\r\n\r\nTo"),
 new Case("NarrowLongWord", "AV AVAVAVAV", Wrap:30), new Case("LineSpacing", "AV\nTo", Spacing:1.4f),
 new Case("AlignCenter", "AV\nTo", Wrap:96, Alignment:TextAlignment.Center),
 new Case("AlignEnd", "AV\nTo", Wrap:96, Alignment:TextAlignment.End),
 new Case("SuppliedBold", "AV To", Face:1), new Case("SuppliedItalic", "AV To", Face:2),
 new Case("CombiningMarks", "A\u0301 g\u0301"), new Case("MixedBidi", "ABC \u05D0\u05D1\u05D2 123"),
 new Case("ArabicMarksDigits", "\u0627\u0644\u0633\u064E\u0651\u0644\u064E\u0627\u0645\u064F 123 \u0639\u0644\u064A\u0643\u0645"),
 new Case("ConstrainedFit", "AV To AV To", Size:24, Wrap:96) };
var observations = new List<object>();
try {
 foreach (var c in cases) {
  var style = c.Face == 1 ? SixLabors.Fonts.FontStyle.Bold : c.Face == 2 ? SixLabors.Fonts.FontStyle.Italic : SixLabors.Fonts.FontStyle.Regular;
  var font = families[c.Face].CreateFont(c.Size, style);
  var face=managedFonts[c.Face];
  var bodyHeight=(face.Ascender-(double)face.Descender)*c.Size/face.UnitsPerEm;
  var lineAdvance=(face.Ascender-(double)face.Descender+Math.Max(0,face.LineGap))*c.Size/face.UnitsPerEm*c.Spacing;
  // Explicitly match the chosen face-metrics line boxes. Do not turn different
  // default line-height/leading policies into production compatibility changes.
  var referenceSpacing=(float)(lineAdvance/c.Size);
  var referenceOriginY=(float)(-(lineAdvance-bodyHeight)/2);
  var options = new SOptions(font) { Dpi=72, Origin=new(0,referenceOriginY), HintingMode=HintingMode.None,
   KerningMode=KerningMode.Standard, TextDirection=TextDirection.Auto, WordBreaking=WordBreaking.BreakWord,
   WrappingLength=c.Wrap, LineSpacing=referenceSpacing, TextAlignment=c.Alignment,
   HorizontalAlignment=HorizontalAlignment.Left, VerticalAlignment=VerticalAlignment.Top };
  var recorder=new Recorder(); new TextRenderer(recorder).RenderText(c.Text, options);
  TextMeasurer.TryMeasureCharacterAdvances(c.Text, options, out var advances);
  using var layout=TextLayout.Create(managedFonts[c.Face], c.Text, new LOptions {
   FontSize=c.Size, WrapWidth=c.Wrap<0?double.PositiveInfinity:c.Wrap, LineSpacing=c.Spacing, Kerning=true });
  using var managed=ImageBuffer.Create(width,height,8);
  ImageDrawing.DrawText(managed,layout,8,8,RgbaColor.From8(20,40,60));
  using var reference=new Image<Rgba32>(width,height);
  var draw=new RichTextOptions(font) { Dpi=72, Origin=new(8,8+referenceOriginY), HintingMode=HintingMode.None,
   KerningMode=options.KerningMode, TextDirection=options.TextDirection, WordBreaking=options.WordBreaking,
   WrappingLength=c.Wrap,LineSpacing=referenceSpacing,TextAlignment=c.Alignment,
   HorizontalAlignment=HorizontalAlignment.Left,VerticalAlignment=VerticalAlignment.Top };
  reference.Mutate(x=>x.DrawText(draw,c.Text,Color.FromRgba(20,40,60,255)));
  long absolute=0,squares=0,managedCoverage=0,referenceCoverage=0; int union=0;
  var lm=new Ink();var lr=new Ink();
  for(int y=0;y<height;y++)for(int x=0;x<width;x++) {
   var a=managed.GetRow8(y)[x*4+3];var b=reference[x,y].A;
   if(a!=0)lm.Include(x,y);if(b!=0)lr.Include(x,y);if(a!=0||b!=0)union++;
   int d=a-b;absolute+=Math.Abs(d);squares+=d*d;managedCoverage+=a;referenceCoverage+=b;
  }
  var managedGlyphs=Enumerable.Range(0,layout.GlyphCount).Select(layout.GetGlyph).ToArray();
  var exactGlyphSequence=managedGlyphs.Select(g=>g.GlyphIndex).SequenceEqual(recorder.Glyphs.Select(g=>g.GlyphId));
  // Character measurement can expose a newline placeholder that the renderer
  // does not emit. Compare rendered clusters, retaining the complete raw records.
  var renderedAdvances=advances.ToArray().Where(a=>a.Codepoint.Value is not (10 or 13)).ToArray();
  if(!renderedAdvances.Select(a=>a.Codepoint.Value).SequenceEqual(recorder.Glyphs.Select(g=>g.CodePoint)))
   throw new InvalidDataException("Reference measurement/render glyph association differs.");
  var clusterStarts=StringInfo.ParseCombiningCharacters(c.Text);
  int Cluster(int offset){var i=Array.BinarySearch(clusterStarts,offset);return clusterStarts[i>=0?i:~i-1];}
  var exactSourceClusters=managedGlyphs.Select(g=>Cluster(g.SourceOffset)).SequenceEqual(renderedAdvances.Select(a=>a.StringIndex));
  double? positionError=exactGlyphSequence?0:null;
  if(exactGlyphSequence)for(var i=0;i<managedGlyphs.Length;i++) {
   var glyph=managedGlyphs[i];var metrics=face.GetGlyphMetrics(glyph.GlyphIndex);if(!metrics.HasOutline)continue;
   var scale=c.Size/(double)face.UnitsPerEm;var native=recorder.Glyphs[i].Bounds;
   positionError=Math.Max(positionError!.Value,Math.Max(Math.Abs(glyph.X+metrics.LeftSideBearing*scale-native.X),Math.Abs(glyph.BaselineY-metrics.YMax*scale-native.Y)));
  }
  var meanAlpha=absolute/(double)Math.Max(1,union);var rmse=Math.Sqrt(squares/(double)Math.Max(1,union));
  var coverageError=Math.Abs(managedCoverage-referenceCoverage)/(double)Math.Max(1,referenceCoverage);
  var boundsError=lm.EdgeDifference(lr);
  var missing=c.Alignment!=TextAlignment.Start?"alignment":c.Name=="ConstrainedFit"?"fit API":null;
  var pass=exactGlyphSequence&&exactSourceClusters&&positionError<=1.0/64&&layout.LineCount==TextMeasurer.CountLines(c.Text,options)
   &&meanAlpha<=8&&rmse<=16&&coverageError<=0.02&&boundsError<=1&&missing is null;
  observations.Add(new { c.Name,c.Text,c.Size,c.Wrap,c.Spacing,Alignment=c.Alignment.ToString(),Font=files[c.Face],
   Managed=new { layout.LineCount,layout.LayoutBounds,layout.InkBounds,layout.Bounds,
    Glyphs=managedGlyphs,RasterBounds=lm.Value },
   Reference=new { ExplicitLineSpacing=referenceSpacing,ExplicitOriginY=referenceOriginY,LineCount=TextMeasurer.CountLines(c.Text,options),Advance=Rect(TextMeasurer.MeasureAdvance(c.Text,options)),
    Bounds=Rect(TextMeasurer.MeasureBounds(c.Text,options)),Glyphs=recorder.Glyphs,
    Advances=advances.ToArray().Select(a=>new { CodePoint=a.Codepoint.Value,a.GraphemeIndex,a.StringIndex,Bounds=Rect(a.Bounds) }).ToArray(),RasterBounds=lr.Value },
   Raster=new { UnionPixels=union,MeanAbsoluteAlpha=meanAlpha,AlphaRmse=rmse,RelativeCoverageError=coverageError,MaximumRasterEdgeDifference=boundsError },
   Comparison=new {ExactGlyphSequence=exactGlyphSequence,ExactSourceClusters=exactSourceClusters,MaximumGlyphPositionError=positionError,Pass=pass},MissingPublicBoundary=missing });
 }
} finally { foreach(var font in managedFonts)font.Dispose(); }
Console.WriteLine(JsonSerializer.Serialize(new {
 ProductionSha256=Convert.ToHexStringLower(SHA256.HashData(File.ReadAllBytes(typeof(ImageBuffer).Assembly.Location))),
 References=new[] {typeof(TextRenderer).Assembly,typeof(RichTextOptions).Assembly,typeof(Image<Rgba32>).Assembly}.Select(a=>new {a.GetName().Name,Version=a.GetCustomAttribute<AssemblyInformationalVersionAttribute>()!.InformationalVersion}),
 NoTiming=true,Criteria=new {ExactGlyphsAndSourceClusters=true,MaximumGlyphPositionError=1.0/64,MaximumRasterEdgeDifference=1,MeanAbsoluteAlpha=8,AlphaRmse=16,RelativeCoverageError=0.02,
  PixelErrorDomain="Union of nonzero-alpha pixels; channel scale 0..255",LineBoxPolicy="Explicitly matched face-metrics line advance and leading origin; outer trailing leading is not compared"},
 FontProvenance=provenance.RootElement,Cases=observations }));
static object Rect(FontRectangle r)=>new {r.X,r.Y,r.Width,r.Height};
sealed record Case(string Name,string Text,int Face=0,float Size=18,float Wrap=-1,float Spacing=1,TextAlignment Alignment=TextAlignment.Start);
sealed class Ink {
 int left=int.MaxValue,top=int.MaxValue,right=-1,bottom=-1;
 internal void Include(int x,int y){left=Math.Min(left,x);top=Math.Min(top,y);right=Math.Max(right,x);bottom=Math.Max(bottom,y);}
 internal int EdgeDifference(Ink other)=>right<0||other.right<0?(right==other.right?0:int.MaxValue):new[]{Math.Abs(left-other.left),Math.Abs(top-other.top),Math.Abs(right-other.right),Math.Abs(bottom-other.bottom)}.Max();
 internal object Value=>right<0?new{X=0,Y=0,Width=0,Height=0}:new{X=left,Y=top,Width=right-left+1,Height=bottom-top+1};
}
sealed class Recorder:IGlyphRenderer {
 internal sealed record Glyph(int GlyphId,int CodePoint,FontRectangle Bounds);
 internal List<Glyph> Glyphs {get;}=[];
 public bool BeginGlyph(in FontRectangle bounds,in GlyphRendererParameters p){Glyphs.Add(new(p.GlyphId,p.CodePoint.Value,bounds));return false;}
 public void BeginText(in FontRectangle bounds){} public void EndText(){} public void BeginFigure(){} public void EndFigure(){} public void EndGlyph(){}
 public void MoveTo(Vector2 p){} public void LineTo(Vector2 p){} public void QuadraticBezierTo(Vector2 p,Vector2 q){} public void CubicBezierTo(Vector2 p,Vector2 q,Vector2 r){}
 public TextDecorations EnabledDecorations()=>TextDecorations.None;
 public void SetDecoration(TextDecorations d,Vector2 a,Vector2 b,float thickness){}
}
