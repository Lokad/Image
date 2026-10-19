using System.Security.Cryptography;
using System.Text.Json;
using Lokad.Image;
using Lokad.Image.Tooling;

if (args.Length != 1) throw new ArgumentException("Supply the four-case output directory.");
var directory = Path.GetFullPath(args[0]); Directory.CreateDirectory(directory);
var cases = new List<object>();
foreach (var depth in new[] { 8, 16 })
foreach (var enlarge in new[] { false, true })
{
    var width = enlarge ? 1027 : 256; var height = enlarge ? 519 : 129;
    var caseId = $"{(enlarge ? "Enlarge" : "Reduce")}{depth}";
    using var source = OriginalPixels.Buffer(513, 259, depth);
    if (depth == 8) source.GetRow8(0)[3] = source.GetRow8(0)[513 * 4 - 1] = 0;
    else source.GetRow16(0)[3] = source.GetRow16(0)[513 * 4 - 1] = 0;
    var original = OriginalPixels.Bytes(source);
    var expected = LanczosReference.Resize(original, 513, 259, depth, width, height);
    using var first = ImageTransforms.Resize(source, width, height, ResizeFilter.Lanczos3);
    using var second = ImageTransforms.Resize(source, width, height, ResizeFilter.Lanczos3);
    if (!OriginalPixels.Bytes(first).AsSpan().SequenceEqual(expected)
        || !OriginalPixels.Bytes(second).AsSpan().SequenceEqual(expected)) throw new Exception($"{caseId}: independent exact oracle differs.");
    if (depth == 8) first.GetRow8(0)[0] ^= 1; else first.GetRow16(0)[0] ^= 1;
    if (!OriginalPixels.Bytes(second).AsSpan().SequenceEqual(expected)
        || !OriginalPixels.Bytes(source).AsSpan().SequenceEqual(original)) throw new Exception($"{caseId}: ownership differs.");
    File.WriteAllBytes(Path.Combine(directory, caseId + ".raw"), expected);
    var record = new { caseId, depth, outputWidth = width, outputHeight = height, inputSha256 = Hash(original), outputSha256 = Hash(expected),
        inputBytes = original.Length, outputBytes = expected.Length, independentDecimalBigIntegerExact = true, sourceUnchanged = true, independentLiveOutputs = true };
    cases.Add(record); Console.WriteLine(JsonSerializer.Serialize(record));
}
File.WriteAllText(Path.Combine(directory, "managed-controls.json"), JsonSerializer.Serialize(new {
    project = "Lokad.Image", correctnessOnly = true, noTimingClaim = true,
    productionSha256 = Convert.ToHexStringLower(SHA256.HashData(File.ReadAllBytes(typeof(ImageBuffer).Assembly.Location))), cases
}, new JsonSerializerOptions { WriteIndented = true }) + "\n");
static string Hash(byte[] bytes) => Convert.ToHexStringLower(SHA256.HashData(bytes));
