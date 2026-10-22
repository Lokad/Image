using System.Text.Json;
using System.Globalization;
using Lokad.Image;
using Lokad.Image.Tooling;

if (args.Length < 2) throw new ArgumentException("Usage: ImageProbe identify INPUT | convert INPUT OUTPUT | flip INPUT OUTPUT | resize INPUT OUTPUT WIDTH HEIGHT [FILTER] | decode-raw INPUT OUTPUT | decode-sequence DIRECTORY OUTPUT EXTENSION COUNT | decode-oriented-sequence DIRECTORY OUTPUT EXTENSION COUNT | encode-sequence RAW DIRECTORY WIDTH HEIGHT DEPTH FORMAT COUNT QUALITY EFFORT [BACKGROUND_RGB_HEX] | encode-quality-sequence RAW DIRECTORY WIDTH HEIGHT DEPTH FORMAT COUNT QUALITY_CSV EFFORT [BACKGROUND_RGB_HEX] | edit-sequence RAW OUTPUT WIDTH HEIGHT DEPTH COUNT OPERATION | resize-sequence RAW OUTPUT WIDTH HEIGHT DEPTH COUNT TARGET_WIDTH TARGET_HEIGHT FILTER | encode-jpeg RAW OUTPUT WIDTH HEIGHT DEPTH QUALITY SAMPLING [BACKGROUND_RGB_HEX] | encode-webp RAW OUTPUT WIDTH HEIGHT DEPTH QUALITY EFFORT | resize-jpeg INPUT OUTPUT WIDTH HEIGHT FILTER QUALITY SAMPLING | webp INPUT OUTPUT QUALITY EFFORT");
if (args[0] == "png-crop-nearest-sequence")
{
    if (args.Length != 14)
        throw new ArgumentException("Usage: ImageProbe png-crop-nearest-sequence INPUT_DIRECTORY OUTPUT_DIRECTORY WIDTH HEIGHT DEPTH COUNT CROP_X CROP_Y CROP_WIDTH CROP_HEIGHT TARGET_WIDTH TARGET_HEIGHT Nearest");
    var width = Integer(args[3]); var height = Integer(args[4]); var depth = Integer(args[5]); var count = Integer(args[6]);
    var x = Integer(args[7]); var y = Integer(args[8]); var cropWidth = Integer(args[9]); var cropHeight = Integer(args[10]);
    var targetWidth = Integer(args[11]); var targetHeight = Integer(args[12]);
    // The last argument makes the selected filter explicit in evidence and rejects accidental alternatives.
    if (args[13] != "Nearest" || width < 1 || height < 1 || depth is not (8 or 16) || count is < 1 or > 100000
        || x < 0 || y < 0 || cropWidth < 1 || cropHeight < 1 || x > width - cropWidth || y > height - cropHeight
        || targetWidth < 1 || targetHeight < 1)
        throw new ArgumentException("A PNG crop/nearest sequence requires a contained crop, positive geometry, 8/16-bit samples, 1..100000 frames and the Nearest filter.");
    _ = checked((long)targetWidth * targetHeight * 4 * (depth / 8) * count);
    var sourceDirectory = Path.GetFullPath(args[1]); var targetDirectory = Path.GetFullPath(args[2]);
    if (string.Equals(sourceDirectory.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar),
        targetDirectory.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar),
        OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal))
        throw new ArgumentException("PNG input and output directories must differ.");
    if (Directory.GetFileSystemEntries(sourceDirectory).Length != count)
        throw new InvalidDataException("PNG input count differs from its declared frame count.");
    for (var frame = 0; frame < count; frame++)
        if (!File.Exists(Path.Combine(sourceDirectory, "frame-" + frame.ToString("D4", CultureInfo.InvariantCulture) + ".png")))
            throw new InvalidDataException("PNG sequence contains a missing or unexpected file.");
    Directory.CreateDirectory(targetDirectory);
    for (var frame = 0; frame < count; frame++)
    {
        var name = "frame-" + frame.ToString("D4", CultureInfo.InvariantCulture) + ".png";
        var encoded = File.ReadAllBytes(Path.Combine(sourceDirectory, name));
        var info = ImageCodec.Identify(encoded);
        if ((info.Format, info.Width, info.Height, info.BitDepth) != (ImageFormat.Png, width, height, depth))
            throw new InvalidDataException("PNG sequence geometry or precision differs from its declared profile.");
        using var decoded = ImageCodec.Decode(encoded);
        using var cropped = ImageTransforms.Crop(decoded, x, y, cropWidth, cropHeight);
        using var resized = ImageTransforms.Resize(cropped, targetWidth, targetHeight, ResizeFilter.Nearest);
        using var frameOutput = File.Create(Path.Combine(targetDirectory, name));
        ImageCodec.EncodePng(resized, frameOutput);
    }
    return;
}
if (args[0] == "resize-sequence" && args.Length == 10)
{
    var width = Integer(args[3]); var height = Integer(args[4]); var depth = Integer(args[5]); var count = Integer(args[6]);
    var targetWidth = Integer(args[7]); var targetHeight = Integer(args[8]); var filter = EnumValue<ResizeFilter>(args[9]);
    if (width < 1 || height < 1 || targetWidth < 1 || targetHeight < 1 || depth is not (8 or 16) || count is < 1 or > 100000)
        throw new ArgumentException("A resize sequence requires positive source/target geometry, 8/16-bit samples and 1..100000 frames.");
    _ = checked((long)targetWidth * targetHeight * 4 * (depth / 8) * count);
    using var sequenceInput = File.OpenRead(args[1]);
    if (sequenceInput.Length != checked((long)width * height * 4 * (depth / 8) * count))
        throw new InvalidDataException("Raw sequence length differs from its declared frame count and geometry.");
    using var sequenceOutput = File.Create(args[2]);
    for (var frame = 0; frame < count; frame++)
    {
        using var raw = RawPixels.Read(sequenceInput, width, height, depth);
        using var resized = ImageTransforms.Resize(raw, targetWidth, targetHeight, filter);
        RawPixels.Write(resized, sequenceOutput);
    }
    if (sequenceInput.ReadByte() != -1) throw new InvalidDataException("Raw sequence contains trailing samples.");
    return;
}
if (args[0] == "edit-sequence" && args.Length == 8)
{
    var width = Integer(args[3]); var height = Integer(args[4]); var depth = Integer(args[5]); var count = Integer(args[6]);
    var operation = EnumValue<GeometryOperation>(args[7]); GeometryWorkload.ValidateGeometry(width, height, operation);
    if (depth is not (8 or 16) || count is < 1 or > 100000)
        throw new ArgumentException("An edit sequence requires 8/16-bit samples and 1..100000 frames.");
    using var sequenceInput = File.OpenRead(args[1]);
    if (sequenceInput.Length != checked((long)width * height * 4 * (depth / 8) * count))
        throw new InvalidDataException("Raw sequence length differs from its declared frame count and geometry.");
    using var sequenceOutput = File.Create(args[2]);
    for (var frame = 0; frame < count; frame++)
    {
        using var raw = RawPixels.Read(sequenceInput, width, height, depth);
        using var edited = GeometryWorkload.Apply(raw, operation);
        RawPixels.Write(edited, sequenceOutput);
    }
    if (sequenceInput.ReadByte() != -1) throw new InvalidDataException("Raw sequence contains trailing samples.");
    return;
}
if (args[0] is "encode-sequence" or "encode-quality-sequence" && args.Length is 10 or 11)
{
    var width = Integer(args[3]); var height = Integer(args[4]); var depth = Integer(args[5]); var count = Integer(args[7]);
    var format = args[6]; var effort = Integer(args[9]);
    var perFrameQuality = args[0] == "encode-quality-sequence";
    var qualities = perFrameQuality ? args[8].Split(',').Select(Integer).ToArray() : new[] { Integer(args[8]) };
    var jpeg = format is "jpeg444" or "jpeg422" or "jpeg420";
    var background = args.Length == 11 ? (uint?)HexRgb(args[10]) : null;
    if (width < 1 || height < 1 || depth is not (8 or 16) || count is < 1 or > 100000
        || format is not ("png" or "jpeg444" or "jpeg422" or "jpeg420" or "webp-lossless" or "webp-lossy")
        || (background.HasValue && !jpeg)
        || (perFrameQuality && (qualities.Length != count || format is not ("jpeg444" or "jpeg422" or "jpeg420" or "webp-lossy")))
        || (format != "png" && qualities.Any(quality => quality is < 1 or > 100))
        || (format.StartsWith("webp", StringComparison.Ordinal) && effort is < 0 or > 6))
        throw new ArgumentException("An encode sequence requires positive geometry, 8/16-bit samples, PNG/JPEG444/JPEG422/JPEG420/WebP and 1..100000 frames; quality 1..100 and WebP effort 0..6. Background RGB is only valid for JPEG. Encode-quality-sequence requires exactly one comma-separated quality per JPEG/lossy WebP frame.");
    var jpegOptions = qualities.Select(quality => new JpegEncodeOptions { QuantizationPolicy = JpegQuantizationPolicy.Original, Quality = quality, BackgroundRgb = background,
        Sampling = format == "jpeg444" ? JpegSampling.YCbCr444 : format == "jpeg422" ? JpegSampling.YCbCr422 : JpegSampling.YCbCr420 }).ToArray();
    using var sequenceInput = File.OpenRead(args[1]);
    if (sequenceInput.Length != checked((long)width * height * 4 * (depth / 8) * count))
        throw new InvalidDataException("Raw sequence length differs from its declared frame count and geometry.");
    Directory.CreateDirectory(args[2]);
    var extension = format == "png" ? ".png" : jpeg ? ".jpg" : ".webp";
    for (var frame = 0; frame < count; frame++)
    {
        using var raw = RawPixels.Read(sequenceInput, width, height, depth);
        var qualityIndex = perFrameQuality ? frame : 0;
        var name = "frame-" + frame.ToString("D4", CultureInfo.InvariantCulture) + extension;
        using var frameOutput = File.Create(Path.Combine(args[2], name));
        switch (format)
        {
            case "png": ImageCodec.EncodePng(raw, frameOutput); break;
            case "jpeg444" or "jpeg422" or "jpeg420": ImageCodec.EncodeJpeg(raw, frameOutput, jpegOptions[qualityIndex]); break;
            default: ImageCodec.EncodeWebp(raw, frameOutput, new WebpEncodeOptions { Lossless = format == "webp-lossless", Quality = qualities[qualityIndex], Effort = effort }); break;
        }
    }
    if (sequenceInput.ReadByte() != -1) throw new InvalidDataException("Raw sequence contains trailing samples.");
    return;
}
if (args[0] is "decode-sequence" or "decode-oriented-sequence" && args.Length == 5)
{
    var count = Integer(args[4]);
    if (count is < 1 or > 100000 || args[3] is not (".png" or ".jpg" or ".webp"))
        throw new ArgumentException("A decode sequence requires PNG/JPEG/WebP and 1..100000 frames.");
    using var sequenceOutput = File.Create(args[2]);
    for (var frame = 0; frame < count; frame++)
    {
        var name = "frame-" + frame.ToString("D4", CultureInfo.InvariantCulture) + args[3];
        using var frameInput = File.OpenRead(Path.Combine(args[1], name));
        using var decoded = ImageCodec.Decode(frameInput, new DecodeOptions { ApplyOrientation = args[0] == "decode-oriented-sequence" });
        RawPixels.Write(decoded, sequenceOutput);
    }
    return;
}
if (args[0] == "encode-webp" && args.Length == 8)
{
    using var raw = RawPixels.Read(args[1], Integer(args[3]), Integer(args[4]), Integer(args[5]));
    using var webpOutput = File.Create(args[2]);
    ImageCodec.EncodeWebp(raw, webpOutput, new WebpEncodeOptions { Lossless = false, Quality = Integer(args[6]), Effort = Integer(args[7]) });
    return;
}
if (args[0] == "encode-jpeg" && args.Length is 8 or 9)
{
    var background = args.Length == 9 ? (uint?)HexRgb(args[8]) : null;
    var options = new JpegEncodeOptions { QuantizationPolicy = JpegQuantizationPolicy.Original, Quality = Integer(args[6]), Sampling = EnumValue<JpegSampling>(args[7]), BackgroundRgb = background };
    using var raw = RawPixels.Read(args[1], Integer(args[3]), Integer(args[4]), Integer(args[5]));
    using var jpegOutput = File.Create(args[2]);
    ImageCodec.EncodeJpeg(raw, jpegOutput, options);
    return;
}
using var input = File.OpenRead(args[1]);
if (args[0] == "identify")
{
    Console.WriteLine(JsonSerializer.Serialize(ImageCodec.Identify(input)));
    return;
}
using var image = ImageCodec.Decode(input, new DecodeOptions { ApplyOrientation = args[0] != "decode-raw" });
if (args[0] == "decode-raw" && args.Length == 3)
{
    RawPixels.Write(image, args[2]);
    return;
}
if (args[0] == "resize-jpeg" && args.Length == 8)
{
    using var resized = ImageTransforms.Resize(image, Integer(args[3]), Integer(args[4]), EnumValue<ResizeFilter>(args[5]));
    using var jpegOutput = File.Create(args[2]);
    ImageCodec.EncodeJpeg(resized, jpegOutput, new JpegEncodeOptions { QuantizationPolicy = JpegQuantizationPolicy.Original, Quality = Integer(args[6]), Sampling = EnumValue<JpegSampling>(args[7]) });
    return;
}
if (args[0] == "webp" && args.Length == 5)
{
    using var webpOutput = File.Create(args[2]);
    ImageCodec.EncodeWebp(image, webpOutput, new WebpEncodeOptions { Lossless = false, Quality = int.Parse(args[3]), Effort = int.Parse(args[4]) });
    return;
}
using var result = args[0] switch
{
    "convert" when args.Length == 3 => image.Clone(),
    "flip" when args.Length == 3 => ImageTransforms.FlipHorizontal(image),
    "resize" when args.Length is 5 or 6 => ImageTransforms.Resize(image, Integer(args[3]), Integer(args[4]), args.Length == 6 ? EnumValue<ResizeFilter>(args[5]) : ResizeFilter.Bicubic),
    _ => throw new ArgumentException("Unsupported command or argument count.")
};
using var output = File.Create(args[2]);
switch (Path.GetExtension(args[2]).ToLowerInvariant())
{
    case ".png": ImageCodec.EncodePng(result, output); break;
    case ".jpg" or ".jpeg": ImageCodec.EncodeJpeg(result, output); break;
    case ".webp": ImageCodec.EncodeWebp(result, output); break;
    default: throw new ArgumentException("Output must be PNG, JPEG or WebP.");
}

static int Integer(string value) => int.Parse(value, CultureInfo.InvariantCulture);
static uint HexRgb(string value) => value.Length == 6 && uint.TryParse(value, NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture, out var color)
    ? color : throw new ArgumentException("Background RGB requires six hexadecimal digits.");
static T EnumValue<T>(string value) where T : struct, Enum
    => Enum.TryParse<T>(value, out var result) && Enum.IsDefined(result) ? result : throw new ArgumentException($"Unsupported {typeof(T).Name}: {value}.");
