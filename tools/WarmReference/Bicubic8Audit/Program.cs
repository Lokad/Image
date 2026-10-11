using System.Security.Cryptography;
using System.Text.Json;
using Lokad.Image;
using Lokad.Image.Tooling;
var folder = Path.GetFullPath(args[0]); Directory.CreateDirectory(folder);
var strategy = args.Length > 1 ? args[1] : "zscale";
if (strategy is not ("zscale" or "swscale" or "swscale-zscale")) throw new ArgumentException("Choose zscale, swscale or swscale-zscale.");
var input = OriginalPixels.Generate(513, 259, 8); input[3] = input[513 * 4 - 1] = 0;
var rational = ExactResizeReference.Resize(input, 513, 259, 8, 256, 129, ResizeFilter.Bicubic);
using var source = OriginalPixels.Buffer(513, 259, 8); source.GetRow8(0)[3] = source.GetRow8(0)[513 * 4 - 1] = 0;
using var first = ImageTransforms.Resize(source, 256, 129, ResizeFilter.Bicubic);
using var second = ImageTransforms.Resize(source, 256, 129, ResizeFilter.Bicubic);
var managed = OriginalPixels.Bytes(first);
if (!managed.AsSpan().SequenceEqual(OriginalPixels.Bytes(second))) throw new Exception("Managed output is not deterministic.");
first.GetRow8(0)[0] ^= 255;
if (!managed.AsSpan().SequenceEqual(OriginalPixels.Bytes(second)) || !input.AsSpan().SequenceEqual(OriginalPixels.Bytes(source))) throw new Exception("Source/output ownership differs.");
var cli = ResizeOracle.PremultipliedBilinearArguments(513, 259, 256, 129).ToList();
cli[cli.IndexOf("-vf") + 1] = cli[cli.IndexOf("-vf") + 1].Replace("filter=bilinear", "filter=bicubic:param_a=0:param_b=0.5");
if (strategy.StartsWith("swscale"))
    cli[cli.IndexOf("-vf") + 1] = "format=gbrapf32le,premultiply=inplace=1,scale=w=256:h=129:flags=bicubic+accurate_rnd+bitexact:param0=0:param1=0.5:gamma=0:sws_dither=none,format=gbrapf32le,unpremultiply=inplace=1,format=rgba";
if (strategy == "swscale-zscale")
    cli[cli.IndexOf("-vf") + 1] = cli[cli.IndexOf("-vf") + 1].Replace("unpremultiply=inplace=1,format=rgba", "unpremultiply=inplace=1,zscale=filter=point:rangein=full:range=full:matrixin=gbr:matrix=gbr:dither=none,format=gbrap,format=rgba");
var nativeScalar = OracleProcess.Run(OracleProcess.Ffmpeg, cli, input);
cli.RemoveRange(cli.IndexOf("-cpuflags"), 2);
var native = OracleProcess.Run(OracleProcess.Ffmpeg, cli, input);
if (!native.AsSpan().SequenceEqual(nativeScalar)) throw new Exception("Native normal/scalar pixels differ.");
var nativeManaged = Difference(native, managed);
var edgeAlphaMismatches = 0; var interiorAlphaMismatches = 0;
for (var y = 0; y < 129; y++)
for (var x = 0; x < 256; x++)
    if (native[(y * 256 + x) * 4 + 3] != managed[(y * 256 + x) * 4 + 3])
    {
        if (x == 0 || x == 255 || y == 0 || y == 128) edgeAlphaMismatches++;
        else interiorAlphaMismatches++;
    }
var record = new { strategy, project = "Lokad.Image", correctnessOnly = true, noTimingClaim = true,
    inputSha256 = Hash(input), rationalSha256 = Hash(rational), managedSha256 = Hash(managed), nativeSha256 = Hash(native),
    inputBytes = input.Length, outputBytes = managed.Length, sourceUnchanged = true, independentOwnedOutputs = true,
    nativeNormalScalarExact = true, managedRational = Difference(managed, rational), nativeRational = Difference(native, rational),
    nativeManaged, edgeAlphaMismatches, interiorAlphaMismatches, performancePairMatched = nativeManaged.AlphaMaximum == 0 && nativeManaged.RgbMaximum <= 1 && nativeManaged.RgbRmse <= 0.25 && nativeManaged.ZeroAlphaNonzeroRgbPixels == 0,
    filter = cli[cli.IndexOf("-vf") + 1] };
File.WriteAllBytes(Path.Combine(folder, "input.rgba"), input);
File.WriteAllBytes(Path.Combine(folder, "rational.rgba"), rational);
File.WriteAllBytes(Path.Combine(folder, "managed.rgba"), managed);
File.WriteAllBytes(Path.Combine(folder, "native.rgba"), native);
var json = JsonSerializer.Serialize(record, new JsonSerializerOptions { WriteIndented = true });
File.WriteAllText(Path.Combine(folder, "audit.json"), json + "\n"); Console.WriteLine(json);
static string Hash(byte[] pixels) => Convert.ToHexStringLower(SHA256.HashData(pixels));
static DifferenceResult Difference(byte[] actual, byte[] expected)
{
    if (actual.Length != expected.Length) throw new Exception("Output length differs.");
    var rgbMax = 0; var alphaMax = 0; long rgbSquared = 0, alphaSquared = 0; var rgbDifferent = 0; var alphaDifferent = 0; var zeroAlphaBadRgb = 0;
    var examples = new List<object>();
    for (var i = 0; i < actual.Length; i++)
    {
        var difference = Math.Abs(actual[i] - expected[i]);
        if (i % 4 == 3)
        {
            alphaMax = Math.Max(alphaMax, difference); alphaSquared += difference * difference;
            if (difference != 0) { alphaDifferent++; if (examples.Count < 8) examples.Add(new { x = i / 4 % 256, y = i / 4 / 256, expected = expected[i], actual = actual[i] }); }
            if (actual[i] == 0 && (actual[i - 3] | actual[i - 2] | actual[i - 1]) != 0) zeroAlphaBadRgb++;
        }
        else { rgbMax = Math.Max(rgbMax, difference); rgbSquared += difference * difference; if (difference != 0) rgbDifferent++; }
    }
    return new(rgbMax, Math.Sqrt((double)rgbSquared / (actual.Length / 4 * 3)), rgbDifferent,
        alphaMax, Math.Sqrt((double)alphaSquared / (actual.Length / 4)), alphaDifferent, zeroAlphaBadRgb, examples);
}
record DifferenceResult(int RgbMaximum, double RgbRmse, int RgbDifferentSamples, int AlphaMaximum,
    double AlphaRmse, int AlphaDifferentPixels, int ZeroAlphaNonzeroRgbPixels, List<object> AlphaExamples);
