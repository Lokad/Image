using System.Buffers.Binary;
using System.Security.Cryptography;
using System.Text.Json;
using Lokad.Image;
using Lokad.Image.Tooling;

var folder = Path.GetFullPath(args[0]); Directory.CreateDirectory(folder);
var input = OriginalPixels.Generate(513, 259, 16);
input.AsSpan(6, 2).Clear(); input.AsSpan((513 * 4 - 1) * 2, 2).Clear();
var expected = ExactResizeReference.Resize(input, 513, 259, 16, 256, 129, ResizeFilter.Bilinear);
using var image = OriginalPixels.Buffer(513, 259, 16);
image.GetRow16(0)[3] = image.GetRow16(0)[513 * 4 - 1] = 0;
using var resized = ImageTransforms.Resize(image, 256, 129, ResizeFilter.Bilinear);
var managed = OriginalPixels.Bytes(resized);
var cli = ResizeOracle.PremultipliedBilinearArguments(513, 259, 256, 129, 16).ToList();
var native = OracleProcess.Run(OracleProcess.Ffmpeg, cli, input);
var normalArgs = cli.ToList(); normalArgs.RemoveRange(normalArgs.IndexOf("-cpuflags"), 2);
var normal = OracleProcess.Run(OracleProcess.Ffmpeg, normalArgs, input);
// Inspect the native floating-point alpha stage before 16-bit quantization.
var floatArgs = normalArgs.ToList();
floatArgs[floatArgs.IndexOf("-vf") + 1] = "format=gbrap16le,zscale=w=256:h=129:filter=bilinear:rangein=full:range=full:matrixin=gbr:matrix=gbr:dither=none,format=gbrapf32le";
floatArgs[floatArgs.IndexOf("-pix_fmt") + 1] = "gbrapf32le";
var floats = OracleProcess.Run(OracleProcess.Ffmpeg, floatArgs, input);
int Read(byte[] data, int i) => BinaryPrimitives.ReadUInt16LittleEndian(data.AsSpan(i * 2, 2));
object Compare(byte[] actual)
{
    long colorSquared = 0, alphaSquared = 0; int colorMaximum = 0, alphaMaximum = 0, colorMismatches = 0, alphaMismatches = 0;
    for (var p = 0; p < 256 * 129; p++)
    for (var c = 0; c < 4; c++)
    {
        var delta = Math.Abs(Read(actual, p * 4 + c) - Read(expected, p * 4 + c));
        if (c == 3) { alphaSquared += (long)delta * delta; alphaMaximum = Math.Max(alphaMaximum, delta); if (delta != 0) alphaMismatches++; }
        else { colorSquared += (long)delta * delta; colorMaximum = Math.Max(colorMaximum, delta); if (delta != 0) colorMismatches++; }
    }
    return new { colorMaximum, colorRmse = Math.Sqrt((double)colorSquared / (256 * 129 * 3)), colorMismatches,
        alphaMaximum, alphaRmse = Math.Sqrt((double)alphaSquared / (256 * 129)), alphaMismatches };
}
var examples = new List<object>(); var smallestHalfDistance = double.MaxValue; var largestHalfDistance = 0.0;
for (var p = 0; p < 256 * 129; p++)
{
    if (Read(normal, p * 4 + 3) == Read(expected, p * 4 + 3)) continue;
    var x = p % 256; var y = p / 256;
    long cx = (2L * x + 1) * 513 - 256, cy = (2L * y + 1) * 259 - 129;
    var fx = (int)(cx / 512); var fy = (int)(cy / 258); long numerator = 0, denominator = 0;
    for (var sy = fy - 3; sy <= fy + 4; sy++)
    for (var sx = fx - 3; sx <= fx + 4; sx++)
    {
        var wx = Math.Max(0, 1026 - Math.Abs(512L * sx - cx));
        var wy = Math.Max(0, 518 - Math.Abs(258L * sy - cy));
        var weight = wx * wy; var ix = Math.Clamp(sx, 0, 512); var iy = Math.Clamp(sy, 0, 258);
        numerator += weight * Read(input, (iy * 513 + ix) * 4 + 3); denominator += weight;
    }
    var rational = (double)numerator / denominator;
    var halfDistance = Math.Abs(rational - Math.Floor(rational) - 0.5);
    smallestHalfDistance = Math.Min(smallestHalfDistance, halfDistance); largestHalfDistance = Math.Max(largestHalfDistance, halfDistance);
    var floating = BitConverter.Int32BitsToSingle(BinaryPrimitives.ReadInt32LittleEndian(floats.AsSpan((3 * 256 * 129 + p) * 4, 4))) * 65535.0;
    if (examples.Count < 8) examples.Add(new { x, y, numerator, denominator, rational, halfDistance, nativeFloatAlpha = floating,
        expected = Read(expected, p * 4 + 3), managed = Read(managed, p * 4 + 3), native = Read(normal, p * 4 + 3) });
}
string Hash(byte[] bytes) => Convert.ToHexStringLower(SHA256.HashData(bytes));
foreach (var (name, data) in new[] { ("input.rgba64le", input), ("rational.rgba64le", expected), ("managed.rgba64le", managed), ("native.rgba64le", normal), ("native.gbrapf32le", floats) })
    File.WriteAllBytes(Path.Combine(folder, name), data);
var record = new { schemaVersion = 1, project = "Lokad.Image", correctnessOnly = true, noTimingClaim = true,
    inputSha256 = Hash(input), rationalSha256 = Hash(expected), managedSha256 = Hash(managed), nativeSha256 = Hash(normal),
    managedRationalPixelsExact = expected.AsSpan().SequenceEqual(managed), normalAndDisabledFfmpegFlagsExact = normal.AsSpan().SequenceEqual(native),
    sourceUnchanged = input.AsSpan().SequenceEqual(OriginalPixels.Bytes(image)), managed = Compare(managed), native = Compare(normal),
    smallestHalfDistance, largestHalfDistance, examples };
var json = JsonSerializer.Serialize(record, new JsonSerializerOptions { WriteIndented = true });
File.WriteAllText(Path.Combine(folder, "audit.json"), json + "\n"); Console.WriteLine(json);
