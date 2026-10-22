param([switch]$NoBuild, [string]$PackagePath)
$ErrorActionPreference = 'Stop'
$repoRoot = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '../..'))
Push-Location $repoRoot
try {
    if ($PSBoundParameters.ContainsKey('PackagePath')) {
        if ([string]::IsNullOrWhiteSpace($PackagePath)) { throw 'Supply a nonempty existing package path.' }
        $package = [IO.Path]::GetFullPath($PackagePath, $repoRoot)
        if (-not (Test-Path -LiteralPath $package -PathType Leaf)) { throw "Package not found: $package" }
    } else {
        $packArguments = @('pack', 'src/Lokad.Image/Lokad.Image.csproj', '-c', 'Release', '--tl:off', '--nologo', '-v', 'minimal')
        if ($NoBuild) { $packArguments += @('--no-build', '--no-restore') }
        dotnet @packArguments
        if ($LASTEXITCODE -ne 0) { throw 'Release pack failed.' }
        [xml]$projectMetadata = Get-Content -LiteralPath (Join-Path $repoRoot 'src/Lokad.Image/Lokad.Image.csproj') -Raw
        $projectVersion = $projectMetadata.Project.PropertyGroup.Version
        $package = Join-Path $repoRoot "artifacts/nuget/Lokad.Image.$projectVersion.nupkg"
    }
    $archive = [IO.Compression.ZipFile]::OpenRead($package)
    try {
        $nuspec = $archive.Entries | Where-Object { $_.FullName.EndsWith('.nuspec') }
        $reader = [IO.StreamReader]::new($nuspec.Open())
        try { [xml]$metadata = $reader.ReadToEnd() } finally { $reader.Dispose() }
        $packageVersion = $metadata.SelectSingleNode("//*[local-name()='metadata']/*[local-name()='version']").InnerText
        if ($packageVersion -notmatch '^\d+\.\d+\.\d+(?:-[0-9A-Za-z.-]+)?$') { throw 'Invalid package version.' }
        if ($metadata.SelectNodes("//*[local-name()='dependency']").Count -ne 0) { throw 'Package contains NuGet dependencies.' }
        if ($metadata.SelectSingleNode("//*[local-name()='metadata']/*[local-name()='icon']").InnerText -ne 'icon.png') {
            throw 'Package icon metadata is missing or incorrect.'
        }
        foreach ($entry in $archive.Entries) {
            if ($entry.FullName -match '(?i)(^|/)(external|tests|tools|benchmarks|runtimes)/|PLAN\.md|\.exe$|\.so$|\.dylib$') {
                throw "Unexpected package content: $($entry.FullName)"
            }
        }
        if (-not ($archive.Entries.FullName -contains 'lib/net10.0/Lokad.Image.dll')) { throw 'Library DLL missing.' }
        foreach ($document in @('README.md', 'SPEC.md', 'LICENSE.txt', 'icon.png', 'docs/resource-admission.md', 'docs/lanczos3-fma-certificate.md', 'docs/jpeg-inverse-rounding.md', 'docs/jpeg-forward-quantization.md', 'docs/usage.md', 'docs/migration.md')) {
            $entry = $archive.GetEntry($document)
            if ($null -eq $entry) { throw "Package document missing: $document" }
            $bytes = [IO.MemoryStream]::new()
            $stream = $entry.Open()
            try { $stream.CopyTo($bytes) } finally { $stream.Dispose() }
            try {
                $expected = [IO.File]::ReadAllBytes((Join-Path $repoRoot $document))
                if ([Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($bytes.ToArray())) -ne
                    [Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($expected))) {
                    throw "Package document differs from current source: $document"
                }
            } finally { $bytes.Dispose() }
        }
        $libraryBytes = [IO.MemoryStream]::new()
        $libraryStream = $archive.GetEntry('lib/net10.0/Lokad.Image.dll').Open()
        try { $libraryStream.CopyTo($libraryBytes) } finally { $libraryStream.Dispose() }
        try { $librarySha256 = [Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($libraryBytes.ToArray())).ToLowerInvariant() }
        finally { $libraryBytes.Dispose() }
    } finally { $archive.Dispose() }
    $consumer = Join-Path $repoRoot ('artifacts/package-smoke/' + [Guid]::NewGuid().ToString('N'))
    New-Item -ItemType Directory -Path $consumer -Force | Out-Null
    # The supplied directory may contain several builds with the same ID and
    # version. Give NuGet a feed containing only the explicitly selected pack.
    $feedDirectory = Join-Path $consumer 'feed'
    New-Item -ItemType Directory -Path $feedDirectory -Force | Out-Null
    Copy-Item -LiteralPath $package -Destination (Join-Path $feedDirectory "Lokad.Image.$packageVersion.nupkg")
    $fontFixturePath = [Security.SecurityElement]::Escape((Join-Path $repoRoot 'tools/Shared/AuthoredTrueType.cs'))
    $memoryFixturePath = [Security.SecurityElement]::Escape((Join-Path $repoRoot 'tools/Shared/TestMemoryBudget.cs'))
    $memoryConsumerPath = [Security.SecurityElement]::Escape((Join-Path $repoRoot 'tools/PackageSmokeTest/CooperativeMemoryConsumer.cs'))
    $shapingFixturePath = [Security.SecurityElement]::Escape((Join-Path $repoRoot 'tools/Shared/AuthoredOpenType.cs'))
    $shapingConsumerPath = [Security.SecurityElement]::Escape((Join-Path $repoRoot 'tools/PackageSmokeTest/TextShapingConsumer.cs'))
    $jpegFixturePath = [Security.SecurityElement]::Escape((Join-Path $repoRoot 'tools/Shared/AuthoredJpegCompatibility.cs'))
    $pixelFixturePath = [Security.SecurityElement]::Escape((Join-Path $repoRoot 'tools/Shared/OriginalPixels.cs'))
    @"
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>Exe</OutputType><TargetFramework>net10.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings><Nullable>enable</Nullable>
    <ManagePackageVersionsCentrally>false</ManagePackageVersionsCentrally>
    <RestorePackagesWithLockFile>false</RestorePackagesWithLockFile>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="Lokad.Image" Version="$packageVersion" />
    <Compile Include="$fontFixturePath" Link="AuthoredTrueType.cs" />
    <Compile Include="$memoryFixturePath" Link="TestMemoryBudget.cs" />
    <Compile Include="$memoryConsumerPath" Link="CooperativeMemoryConsumer.cs" />
    <Compile Include="$shapingFixturePath" Link="AuthoredOpenType.cs" />
    <Compile Include="$shapingConsumerPath" Link="TextShapingConsumer.cs" />
    <Compile Include="$jpegFixturePath" Link="AuthoredJpegCompatibility.cs" />
    <Compile Include="$pixelFixturePath" Link="OriginalPixels.cs" />
  </ItemGroup>
</Project>
"@ | Set-Content -LiteralPath (Join-Path $consumer 'Consumer.csproj') -Encoding utf8
    @'
using Lokad.Image;
foreach (var referenceDepth in new[] { 8, 16 })
foreach (var referenceSampling in new[] { JpegSampling.YCbCr444, JpegSampling.YCbCr422, JpegSampling.YCbCr420 })
{
    using var referenceImage = Lokad.Image.Tooling.OriginalPixels.Buffer(513, 259, referenceDepth);
    if (referenceDepth == 8) { referenceImage.GetRow8(0)[3] = 0; referenceImage.GetRow8(0)[513 * 4 - 1] = 0; }
    else { referenceImage.GetRow16(0)[3] = 0; referenceImage.GetRow16(0)[513 * 4 - 1] = 0; }
    using var referenceRoot = new Lokad.Image.Tooling.TestMemoryBudget(2 * 1024 * 1024);
    using var referencePacket = new MemoryStream();
    await ImageCodec.EncodeJpegAsync(referenceImage, referencePacket, new JpegEncodeOptions
    {
        QuantizationPolicy = JpegQuantizationPolicy.Reference, Sampling = referenceSampling,
        BackgroundRgb = 0x204060, Limits = ImageLimits.Default with { MemoryBudget = referenceRoot }
    });
    var expectedReferenceHash = (referenceSampling, referenceDepth) switch
    {
        (JpegSampling.YCbCr444, 8) => "21389750d4936f4c45f8252020843576916cd5689913de2396ee4113df379ecd",
        (JpegSampling.YCbCr444, 16) => "f69ce841c602373b71afac9ce403d2d634fe84630cca21cd585c370169cc2a07",
        (JpegSampling.YCbCr422, 8) => "a1d3d66671a19606075b7604173922d7677236928381a14355bc6fb640ebd3a4",
        (JpegSampling.YCbCr422, 16) => "afe4c4f147555ca198d3c82858c481eb0901dab08cd3ccf7c287915fba00d030",
        (JpegSampling.YCbCr420, 8) => "83e9e80a87c1928c8e23e2929b15348d0e8e8ce5568247420f6e366abeb830e3",
        (JpegSampling.YCbCr420, 16) => "20461d0d4c85dd2d07e1068e2f9d7a856805c637f1d9576201b35456eeee71ed",
        _ => throw new Exception("Unknown reference control.")
    };
    if (Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(referencePacket.ToArray())).ToLowerInvariant()
        != expectedReferenceHash || referenceRoot.Bytes != 0 || referenceRoot.IsDisposed)
        throw new Exception("Packaged reference JPEG bytes or shared ownership differ.");
}
await Lokad.Image.Tooling.CooperativeMemoryConsumer.Verify();
Lokad.Image.Tooling.TextShapingConsumer.Verify();
if (Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(
    File.ReadAllBytes(typeof(ImageBuffer).Assembly.Location))).ToLowerInvariant() != args[4])
    throw new Exception("Package consumer loaded a different assembly.");
var fontInput = Lokad.Image.Tooling.AuthoredTrueType.Build(style: FontStyle.Bold | FontStyle.Italic);
using var font = TrueTypeFont.Load(fontInput); Array.Fill(fontInput, (byte)0xA7);
if (font.Style != (FontStyle.Bold | FontStyle.Italic) || font.GetGlyphIndex(new System.Text.Rune(0x1F642)) != 7
    || font.GetGlyphMetrics(new System.Text.Rune('A')).AdvanceWidth != 660)
    throw new Exception("Package supplied TrueType ownership/Unicode/metrics differ.");
using var layoutFace = TrueTypeFont.Load(Lokad.Image.Tooling.AuthoredTrueType.Build());
using var textLayout = TextLayout.Create(layoutFace, "AV AVg", new TextOptions { FontSize = 10, WrapWidth = 12 });
using var curveLayout = TextLayout.Create(layoutFace, "g", new TextOptions { FontSize = 10 });
if (textLayout.GlyphCount != 5 || textLayout.LineCount != 3 || Math.Abs(textLayout.LayoutBounds.Width - 11.2) > 1e-10
    || textLayout.LayoutBounds.Height != 34 || textLayout.GetGlyph(2).SourceOffset != 3
    || curveLayout.InkBounds != new TextBounds(0, 4, 6, 5))
    throw new Exception("Package supplied TrueType layout/wrapping/quadratic bounds differ.");
using var textCanvas = ImageBuffer.Create(10, 12, 16);
ImageDrawing.DrawText(textCanvas, curveLayout, 1, 0, new RgbaColor(12345, 23456, 34567));
if (textCanvas.GetRow16(6)[4 * 4] != 12345 || textCanvas.GetRow16(6)[4 * 4 + 3] != 65535
    || textCanvas.GetRow16(0)[4 * 4 + 3] != 0)
    throw new Exception("Package antialiased text pixels/precision differ.");
using var source = ImageBuffer.Create(2, 1, 16);
new ushort[] { 12345, 1, 65534, 43210, 54321, 65534, 1, 12345 }.CopyTo(source.GetRow16(0));
using var flipped = ImageTransforms.FlipHorizontal(source);
using var encoded = new MemoryStream();
ImageCodec.EncodePng(flipped, encoded);
using var decoded = ImageCodec.Decode(encoded.ToArray());
if (decoded.BitDepth != 16 || !decoded.GetRow16(0).SequenceEqual(flipped.GetRow16(0)))
    throw new Exception("Exact package consumer pixels differ.");
using (var packetInput = new MemoryStream(encoded.ToArray(), writable: false))
using (var streamed = ImageCodec.Decode(packetInput))
{
    if (!streamed.GetRow16(0).SequenceEqual(flipped.GetRow16(0)) || !packetInput.CanRead || packetInput.Position != packetInput.Length)
        throw new Exception("Package leased Stream decode pixels/ownership/EOF differ.");
}
// Exercise the README JPEG/WebP options through the isolated actual package.
using var exampleJpeg = new MemoryStream();
ImageCodec.EncodeJpeg(flipped, exampleJpeg, new JpegEncodeOptions { Quality = 85, BackgroundRgb = 0xFFFFFF });
using var flattenedExample = ImageBuffer.Create(2, 1, 8);
const long sampleMaximum = 65535;
const long sampleDenominator = sampleMaximum * sampleMaximum;
for (var x = 0; x < 2; x++)
{
    var alpha = (long)flipped.GetRow16(0)[x * 4 + 3];
    for (var channel = 0; channel < 3; channel++)
    {
        var color = (long)flipped.GetRow16(0)[x * 4 + channel];
        flattenedExample.GetRow8(0)[x * 4 + channel] = (byte)((color * alpha * 255
            + 255 * (sampleMaximum - alpha) * sampleMaximum + sampleDenominator / 2) / sampleDenominator);
    }
    flattenedExample.GetRow8(0)[x * 4 + 3] = 255;
}
using var exampleJpegReference = new MemoryStream();
ImageCodec.EncodeJpeg(flattenedExample, exampleJpegReference, new JpegEncodeOptions { Quality = 85 });
if (!exampleJpeg.ToArray().AsSpan().SequenceEqual(exampleJpegReference.ToArray()) || !exampleJpeg.CanWrite)
    throw new Exception("README JPEG background/depth rounding or stream ownership differs.");
using var exampleWebp = new MemoryStream();
ImageCodec.EncodeWebp(flipped, exampleWebp, new WebpEncodeOptions { Lossless = false, Quality = 85, Effort = 4 });
using var exampleWebpPixels = ImageCodec.Decode(exampleWebp.ToArray());
using var opaqueWebpSource = ImageBuffer.Create(33, 1, 8);
for (var x = 0; x < 33; x++)
    new byte[] { (byte)(x * 17), (byte)(x * 29), (byte)(x * 47), 255 }.CopyTo(opaqueWebpSource.GetRow8(0).Slice(x * 4, 4));
using var opaqueWebp = new MemoryStream(); ImageCodec.EncodeWebp(opaqueWebpSource, opaqueWebp);
using var opaqueWebpPixels = ImageCodec.Decode(opaqueWebp.ToArray());
if (!opaqueWebpPixels.GetRow8(0).SequenceEqual(opaqueWebpSource.GetRow8(0)))
    throw new Exception("Package opaque singleton-alpha WebP pixels differ.");
if ((exampleWebpPixels.Width, exampleWebpPixels.Height, exampleWebpPixels.BitDepth) != (2, 1, 8) || !exampleWebp.CanWrite)
    throw new Exception("README WebP geometry, precision or stream ownership differs.");
for (var x = 0; x < 2; x++)
    if (exampleWebpPixels.GetRow8(0)[x * 4 + 3] != (flipped.GetRow16(0)[x * 4 + 3] + 128) / 257)
        throw new Exception("README WebP alpha differs.");
// Verify the documented source + row + IDAT + BCL encoder admission through
// the actual restored package, including rejection before any output is written.
var pngLimits = new ImageLimits { MaxWorkingBytes = 16 + 16 + 65536 + 1024 * 1024 };
using var admittedPng = new MemoryStream();
ImageCodec.EncodePng(flipped, admittedPng, new PngEncodeOptions { Limits = pngLimits });
if (!admittedPng.ToArray().AsSpan().SequenceEqual(encoded.ToArray()))
    throw new Exception("Package consumer PNG admission changes its encoded output.");
using var rejectedPng = new MemoryStream();
try
{
    ImageCodec.EncodePng(flipped, rejectedPng, new PngEncodeOptions { Limits = pngLimits with { MaxWorkingBytes = pngLimits.MaxWorkingBytes - 1 } });
    throw new Exception("Package consumer PNG workspace limit was not enforced.");
}
catch (ImageLimitException failure)
{
    if (failure.Limit != ImageLimitKind.WorkingBytes)
        throw new Exception("Package consumer PNG limit category differs.");
}
if (rejectedPng.Length != 0 || !rejectedPng.CanWrite)
    throw new Exception("Package consumer PNG admission writes output or closes the caller's stream.");
try
{
    ImageCodec.Identify("GIF89a"u8);
    throw new Exception("Package consumer accepted an unsupported format.");
}
catch (UnsupportedImageException failure)
{
    if (failure.Feature != UnsupportedImageFeature.Format || failure.Format is not null)
        throw new Exception("Package consumer unsupported-format category differs.");
}
using var tinyAlpha = ImageBuffer.Create(2, 1, 16);
new ushort[] { 65535, 0, 0, 0, 0, 65535, 65535, 1 }.CopyTo(tinyAlpha.GetRow16(0));
using var halfAlpha = ImageBuffer.Create(5, 2, 16);
for (var y = 0; y < 2; y++)
for (var x = 0; x < 5; x++)
    new ushort[] { 65535, 65535, 65535, (ushort)(1 - y) }.CopyTo(halfAlpha.GetRow16(y).Slice(x * 4, 4));
foreach (var filter in new[] { ResizeFilter.Bilinear, ResizeFilter.Bicubic })
{
    using var enlarged = ImageTransforms.Resize(tinyAlpha, 4, 1, filter);
    if (!enlarged.GetRow16(0).Slice(4, 4).SequenceEqual(new ushort[4]))
        throw new Exception("Package consumer resize retains RGB at rounded zero alpha.");
    using var resizedHalf = ImageTransforms.Resize(halfAlpha, 3, 3, filter);
    for (var x = 0; x < 3; x++)
        if (!resizedHalf.GetRow16(1).Slice(x * 4, 4).SequenceEqual(new ushort[] { 65535, 65535, 65535, 1 }))
            throw new Exception("Package consumer resize loses visible color at an exact half-alpha tie.");
}
using (var packageWideRoot = new Lokad.Image.Tooling.TestMemoryBudget(513 * 259 * 4 + 2 * 128 * 64 * 4 + 4096))
{
    var packageWideLimits = new ImageLimits { MemoryBudget = packageWideRoot, MaxWorkingBytes = 513 * 259 * 4 + 128 * 64 * 4 + 4096 };
    using var packageWideSource = ImageBuffer.Create(513, 259, 8, packageWideLimits);
    var packageWideInput = Lokad.Image.Tooling.OriginalPixels.Generate(513, 259, 8);
    for (var y = 0; y < 259; y++) packageWideInput.AsSpan(y * 513 * 4, 513 * 4).CopyTo(packageWideSource.GetRow8(y));
    packageWideSource.GetRow8(0)[3] = packageWideSource.GetRow8(0)[513 * 4 - 1] = 0;
    using var packageWideFirst = ImageTransforms.Resize(packageWideSource, 128, 64, ResizeFilter.Bicubic, packageWideLimits);
    using var packageWideSecond = ImageTransforms.Resize(packageWideSource, 128, 64, ResizeFilter.Bicubic, packageWideLimits);
    var packageWideHash = Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(Lokad.Image.Tooling.OriginalPixels.Bytes(packageWideSecond))).ToLowerInvariant();
    if (packageWideHash != "5067190b92fe644a75b0001848c4a555feb638c72592239dbf0ea0d63c518e71")
        throw new Exception("Package bounded bicubic pixels differ.");
    var packageWidePixel = packageWideSecond.GetRow8(0)[0];
    packageWideFirst.GetRow8(0)[0] ^= 255;
    packageWideSource.Dispose();
    if (packageWideRoot.Bytes != 2 * 128 * 64 * 4 || packageWideSecond.GetRow8(0)[0] != packageWidePixel)
        throw new Exception("Package bounded bicubic survivor ownership differs.");
    packageWideFirst.Dispose(); packageWideSecond.Dispose();
    if (packageWideRoot.Bytes != 0 || packageWideRoot.IsDisposed)
        throw new Exception("Package bounded bicubic root cleanup differs.");
}
using (var packageVp8Source = Lokad.Image.Tooling.OriginalPixels.Buffer(64, 64, 8, opaque: true))
using (var packageVp8Encoded = new MemoryStream())
using (var packageVp8Root = new Lokad.Image.Tooling.TestMemoryBudget(2 * 1024 * 1024))
using (var packageVp8Cancellation = new CancellationTokenSource())
{
    ImageCodec.EncodeWebp(packageVp8Source, packageVp8Encoded, new WebpEncodeOptions { Lossless = false, Quality = 75, Effort = 4 });
    var packageVp8Packet = packageVp8Encoded.ToArray();
    if (packageVp8Packet.Length <= 1024) throw new Exception("Package VP8 block fixture is too short.");
    var packageVp8Options = new DecodeOptions { Limits = new ImageLimits { MemoryBudget = packageVp8Root } };
    using var packageVp8Fast = ImageCodec.Decode(packageVp8Packet, packageVp8Options);
    using var packageVp8Original = ImageCodec.Decode(packageVp8Packet, packageVp8Options, packageVp8Cancellation.Token);
    if (!Lokad.Image.Tooling.OriginalPixels.Bytes(packageVp8Fast).AsSpan().SequenceEqual(Lokad.Image.Tooling.OriginalPixels.Bytes(packageVp8Original))
        || packageVp8Root.Bytes != 2 * 64 * 64 * 4)
        throw new Exception("Package VP8 block pixels or retained charges differ.");
    var packageVp8Pixel = packageVp8Original.GetRow8(0)[0];
    packageVp8Fast.GetRow8(0)[0] ^= 255; packageVp8Fast.Dispose();
    if (packageVp8Original.GetRow8(0)[0] != packageVp8Pixel || packageVp8Root.Bytes != 64 * 64 * 4)
        throw new Exception("Package VP8 block survivors alias or lose their charge.");
    packageVp8Original.Dispose();
    if (packageVp8Root.Bytes != 0 || packageVp8Root.IsDisposed)
        throw new Exception("Package VP8 block cleanup or borrowed root differs.");
}
using (var packagePngSource = Lokad.Image.Tooling.OriginalPixels.Buffer(64, 64, 8))
using (var packagePngEncoded = new MemoryStream())
using (var packagePngRoot = new Lokad.Image.Tooling.TestMemoryBudget(2 * 1024 * 1024))
using (var packagePngCancellation = new CancellationTokenSource())
{
    // Reuse the existing original64x64 source; no new performance input.
    ImageCodec.EncodePng(packagePngSource, packagePngEncoded);
    var packagePngPacket = packagePngEncoded.ToArray();
    var packagePngOptions = new DecodeOptions { Limits = new ImageLimits { MemoryBudget = packagePngRoot } };
    using var packagePngFast = ImageCodec.Decode(packagePngPacket, packagePngOptions);
    using var packagePngOriginal = ImageCodec.Decode(packagePngPacket, packagePngOptions, packagePngCancellation.Token);
    var packagePngExpected = Lokad.Image.Tooling.OriginalPixels.Bytes(packagePngSource);
    if (!Lokad.Image.Tooling.OriginalPixels.Bytes(packagePngFast).AsSpan().SequenceEqual(packagePngExpected)
        || !Lokad.Image.Tooling.OriginalPixels.Bytes(packagePngOriginal).AsSpan().SequenceEqual(packagePngExpected)
        || packagePngRoot.Bytes != 2 * 64 * 64 * 4)
        throw new Exception("Package PNG cursor pixels or retained charges differ.");
    packagePngFast.GetRow8(0)[0] ^= 255; packagePngFast.Dispose();
    if (!Lokad.Image.Tooling.OriginalPixels.Bytes(packagePngOriginal).AsSpan().SequenceEqual(packagePngExpected)
        || packagePngRoot.Bytes != 64 * 64 * 4)
        throw new Exception("Package PNG cursor survivor aliases or loses its charge.");
    packagePngOriginal.Dispose();
    if (packagePngRoot.Bytes != 0 || packagePngRoot.IsDisposed)
        throw new Exception("Package PNG cursor cleanup or borrowed root differs.");
}
using var originalPng = ImageCodec.Decode(File.ReadAllBytes(args[2]));
using (var drawing = ImageBuffer.Create(3, 2, 16))
using (var overlay = ImageBuffer.Create(1, 1, 8))
{
    ImageDrawing.FillRectangle(overlay, 0, 0, 1, 1, RgbaColor.From8(255, 0, 0, 128));
    ImageDrawing.Composite(drawing, overlay, 1, 0);
    ImageDrawing.CopyRegion(drawing, 1, 0, 1, 1, drawing, 2, 1);
    if (!drawing.GetRow16(1).Slice(8, 4).SequenceEqual(new ushort[] { 65535, 0, 0, 32896 }))
        throw new Exception("Package drawing color/alpha/overlap/precision differs.");
}
var originalPngExpected = File.ReadAllBytes(args[3]);
if (originalPng.Width != 19 || originalPng.Height != 17 || originalPng.BitDepth != 16)
    throw new Exception("Package consumer authored PNG profile differs.");
for (var y = 0; y < 17; y++)
for (var sample = 0; sample < 19*4; sample++)
    if (originalPng.GetRow16(y)[sample] != System.Buffers.Binary.BinaryPrimitives.ReadUInt16LittleEndian(originalPngExpected.AsSpan((y*19*4+sample)*2)))
        throw new Exception("Package consumer authored PNG samples differ.");
using var opaque = ImageBuffer.Create(8, 8, 8);
// Original T.81 four-component stream: unit quantization, one-bit DC-zero/EOB
// tables and four zero blocks. Every coded channel is analytically 128.
using var cmykFixture = new MemoryStream(); cmykFixture.Write(new byte[] { 255, 216 });
void CmykSegment(byte marker, byte[] payload)
{
    cmykFixture.Write(new byte[] { 255, marker, (byte)((payload.Length + 2) >> 8), (byte)(payload.Length + 2) });
    cmykFixture.Write(payload);
}
CmykSegment(0xEE, new byte[] { 65, 100, 111, 98, 101, 0, 100, 0, 0, 0, 0, 0 });
var cmykQuantization = new byte[65]; cmykQuantization.AsSpan(1).Fill(1); CmykSegment(0xDB, cmykQuantization);
CmykSegment(0xC0, new byte[] { 8, 0, 1, 0, 2, 4, 1, 17, 0, 2, 17, 0, 3, 17, 0, 4, 17, 0 });
var cmykTables = new byte[36]; cmykTables[1] = cmykTables[19] = 1; cmykTables[18] = 16; CmykSegment(0xC4, cmykTables);
CmykSegment(0xDA, new byte[] { 4, 1, 0, 2, 0, 3, 0, 4, 0, 0, 63, 0 });
cmykFixture.Write(new byte[] { 0, 255, 217 });
var cmykBytes = cmykFixture.ToArray();
using var cmykPixels = ImageCodec.Decode(cmykBytes);
if (!cmykPixels.GetRow8(0).SequenceEqual(new byte[] { 64, 64, 64, 255, 64, 64, 64, 255 }))
    throw new Exception("Package consumer CMYK color/black/alpha differs.");
var cmykSamples = new byte[8]; ImageCodec.DecodeJpeg(cmykBytes, cmykSamples, JpegSampleFormat.Cmyk);
if (cmykSamples.AsSpan().IndexOfAnyExcept((byte)127) >= 0)
    throw new Exception("Package consumer CMYK inversion differs.");
byte[] sparseValues = [80, 170, 90, 200];
foreach (var sparseTerms in new[] { 1, 2 })
{
var sparsePacket = Lokad.Image.Tooling.AuthoredJpegCompatibility.Create(17, 9, sparseValues, adobe: 0, nonzeroAc: true, acTerms: sparseTerms);
using var sparseRoot = new Lokad.Image.Tooling.TestMemoryBudget(1024 * 1024);
var sparseOptions = new DecodeOptions { Limits = new() { MemoryBudget = sparseRoot } };
using var sparseFirst = ImageCodec.Decode(sparsePacket, sparseOptions);
using var sparseSurvivor = ImageCodec.Decode(sparsePacket, sparseOptions);
if (sparseRoot.Bytes != 2 * 17 * 9 * 4) throw new Exception("Packaged sparse JPEG retained charges differ.");
sparseFirst.GetRow8(0).Fill(173); sparseFirst.Dispose(); Array.Clear(sparsePacket);
if (sparseRoot.Bytes != 17 * 9 * 4 || sparseRoot.IsDisposed) throw new Exception("Packaged sparse JPEG ownership differs.");
for (var sy = 0; sy < 9; sy++)
for (var sx = 0; sx < 17; sx++)
{
    var delta = (int)Math.Round((Math.Cos((2 * (sx & 7) + 1) * Math.PI / 16)
        + (sparseTerms == 2 ? Math.Cos((2 * (sy & 7) + 1) * Math.PI / 16) : 0)) / Math.Sqrt(2), MidpointRounding.AwayFromZero);
    for (var sc = 0; sc < 3; sc++)
        if (sparseSurvivor.GetRow8(sy)[sx * 4 + sc] != ((sparseValues[sc] + delta) * (sparseValues[3] + delta) + 127) / 255)
            throw new Exception("Packaged sparse JPEG analytical sample differs.");
    if (sparseSurvivor.GetRow8(sy)[sx * 4 + 3] != 255) throw new Exception("Packaged sparse JPEG alpha differs.");
}
sparseSurvivor.Dispose();
if (sparseRoot.Bytes != 0) throw new Exception("Packaged sparse JPEG retained charge leaked.");
}
for (var y = 0; y < 8; y++)
for (var x = 0; x < 8; x++) new byte[] { 123, 123, 123, 255 }.CopyTo(opaque.GetRow8(y)[(x*4)..]);
using var jpeg = new MemoryStream();
ImageCodec.EncodeJpeg(opaque, jpeg, new JpegEncodeOptions { Sampling = JpegSampling.Grayscale, Quality = 100, RestartInterval = 1 });
using var jpegPixels = ImageCodec.Decode(jpeg.ToArray());
if (jpegPixels.Width != 8 || jpegPixels.Height != 8 || jpegPixels.BitDepth != 8 || !jpeg.CanWrite)
    throw new Exception("Package consumer JPEG contract differs.");
for (var y = 0; y < 8; y++)
    if (!jpegPixels.GetRow8(y).SequenceEqual(opaque.GetRow8(y))) throw new Exception("Package consumer JPEG pixels differ.");
VerifyAsyncInput(jpeg.ToArray()).GetAwaiter().GetResult();
VerifyAsyncOutput(opaque).GetAwaiter().GetResult();
foreach (var packet in new[] { encoded.ToArray(), jpeg.ToArray(), exampleWebp.ToArray() })
{
    var expected = ImageCodec.Identify(packet);
    var limits = new ImageLimits { MaxWorkingBytes = 8448, MaxWork = 1_000_000 };
    using var headers = new MemoryStream(packet, writable: false);
    if (ImageCodec.InspectHeaders(headers, limits) != expected || headers.Position != headers.Length || !headers.CanRead)
        throw new Exception("Package fixed-workspace header inspection differs.");
    headers.Position = 0;
    var budget = new ImageBudget(limits.MaxWorkingBytes, limits.MaxWork);
    if (budget.InspectHeadersAsync(headers, limits).GetAwaiter().GetResult() != expected || budget.RemainingWorkingBytes != 8448)
        throw new Exception("Package async/shared header inspection differs.");
}
static async Task VerifyAsyncOutput(ImageBuffer image)
{
    using var png = new MemoryStream(); await ImageCodec.EncodePngAsync(image, png);
    using var pngExpected = new MemoryStream(); ImageCodec.EncodePng(image, pngExpected);
    if (!png.ToArray().AsSpan().SequenceEqual(pngExpected.ToArray()) || !png.CanWrite)
        throw new Exception("Package async PNG packet/ownership differs.");
    using var jpegOutput = new MemoryStream(); var options = new JpegEncodeOptions { Quality = 69, BackgroundRgb = 0 };
    await ImageCodec.EncodeJpegAsync(image, jpegOutput, options);
    using var jpegExpected = new MemoryStream(); ImageCodec.EncodeJpeg(image, jpegExpected, options);
    if (!jpegOutput.ToArray().AsSpan().SequenceEqual(jpegExpected.ToArray())) throw new Exception("Package async JPEG differs.");
    var limits = new ImageLimits { MaxWorkingBytes = 2 * 1024 * 1024, MaxWork = 2_000_000 };
    var budget = new ImageBudget(limits.MaxWorkingBytes, 4 * limits.MaxWork);
    using var webp = new MemoryStream();
    await budget.EncodeWebpAsync(image, webp, new() { Limits = limits, Lossless = true });
    using var webpExpected = new MemoryStream(); ImageCodec.EncodeWebp(image, webpExpected, new() { Lossless = true });
    var gray = new byte[image.Width * image.Height];
    for (var y = 0; y < image.Height; y++) for (var x = 0; x < image.Width; x++) gray[y * image.Width + x] = image.GetRow8(y)[x * 4];
    using var samples = new MemoryStream();
    await budget.EncodeJpegAsync(gray.AsMemory(), image.Width, image.Height, JpegSampleFormat.Grayscale, samples,
        options: new() { Limits = limits, Quality = 69 });
    using var samplesExpected = new MemoryStream();
    ImageCodec.EncodeJpeg(gray, image.Width, image.Height, JpegSampleFormat.Grayscale, samplesExpected, options: new() { Quality = 69 });
    if (!webp.ToArray().AsSpan().SequenceEqual(webpExpected.ToArray()) || !samples.ToArray().AsSpan().SequenceEqual(samplesExpected.ToArray())
        || budget.RemainingWorkingBytes != budget.MaxWorkingBytes || budget.ConsumedWork <= 0 || !webp.CanWrite || !samples.CanWrite)
        throw new Exception("Package async output shared admission/packets/ownership differs.");
}
static async Task VerifyAsyncInput(byte[] packet)
{
    using var input = new MemoryStream(packet, writable: false);
    if (await ImageCodec.IdentifyAsync(input) != ImageCodec.Identify(packet) || !input.CanRead)
        throw new Exception("Package async Identify or input ownership differs.");
    using var pixelsInput = new MemoryStream(packet, writable: false);
    using var pixels = await ImageCodec.DecodeAsync(pixelsInput);
    using var expected = ImageCodec.Decode(packet);
    for (var y = 0; y < pixels.Height; y++)
        if (!pixels.GetRow8(y).SequenceEqual(expected.GetRow8(y))) throw new Exception("Package async pixels differ.");
    var limits = new ImageLimits { MaxWorkingBytes = 2 * 1024 * 1024, MaxWork = 2_000_000 };
    var budget = new ImageBudget(limits.MaxWorkingBytes, 2 * limits.MaxWork);
    using var budgetInfoInput = new MemoryStream(packet, writable: false);
    if (await budget.IdentifyAsync(budgetInfoInput, limits) != ImageCodec.Identify(packet))
        throw new Exception("Package async budget Identify differs.");
    using var budgetInput = new MemoryStream(packet, writable: false);
    using (var result = await budget.DecodeAsync(budgetInput, new() { Limits = limits }))
    {
        if (budget.RemainingWorkingBytes != limits.MaxWorkingBytes - result.Image.Width * result.Image.Height * 4L)
            throw new Exception("Package async retained admission differs.");
        for (var y = 0; y < result.Image.Height; y++)
            if (!result.Image.GetRow8(y).SequenceEqual(expected.GetRow8(y))) throw new Exception("Package async budget pixels differ.");
    }
    if (budget.RemainingWorkingBytes != limits.MaxWorkingBytes || !pixelsInput.CanRead || !budgetInput.CanRead || !budgetInfoInput.CanRead)
        throw new Exception("Package async lifetime/refund differs.");
}
var sharedLimits = new ImageLimits { MaxWorkingBytes = 2 * 1024 * 1024, MaxWork = 2_000_000 };
var callerSamples = Enumerable.Repeat((byte)0xA7, 8 * 27).ToArray();
var sampleInfo = ImageCodec.DecodeJpeg(jpeg.ToArray(), callerSamples, JpegSampleFormat.Rgb, 27);
if ((sampleInfo.Width, sampleInfo.Height) != (8, 8))
    throw new Exception("Package consumer JPEG sample dimensions differ.");
for (var y = 0; y < 8; y++)
{
    if (callerSamples.AsSpan(y * 27, 24).IndexOfAnyExcept((byte)123) >= 0
        || callerSamples.AsSpan(y * 27 + 24, 3).IndexOfAnyExcept((byte)0xA7) >= 0)
        throw new Exception("Package consumer JPEG samples or padding differ.");
}
var shared = new ImageBudget(sharedLimits.MaxWorkingBytes + 256, sharedLimits.MaxWork * 2);
using (shared.Reserve(256, 1))
{
    using (var budgeted = shared.Decode(jpeg.ToArray(), new DecodeOptions { Limits = sharedLimits }))
    {
        if (shared.RemainingWorkingBytes != sharedLimits.MaxWorkingBytes - 8 * 8 * 4)
            throw new Exception("Package consumer retained shared memory differs.");
        for (var y = 0; y < 8; y++)
            if (!budgeted.Image.GetRow8(y).SequenceEqual(opaque.GetRow8(y)))
                throw new Exception("Package consumer budgeted pixels differ.");
    }
    if (shared.RemainingWorkingBytes != sharedLimits.MaxWorkingBytes)
        throw new Exception("Package consumer budgeted disposal did not release pixels.");
}
if (shared.RemainingWorkingBytes != shared.MaxWorkingBytes || shared.ConsumedWork <= 1
    || shared.RemainingWork != shared.MaxWork - shared.ConsumedWork)
    throw new Exception("Package consumer shared admission did not settle.");
var callerGray = new byte[64];
shared.DecodeJpeg(jpeg.ToArray(), callerGray, JpegSampleFormat.Grayscale,
    options: new JpegDecodeOptions { Limits = sharedLimits });
if (callerGray.AsSpan().IndexOfAnyExcept((byte)123) >= 0 || shared.RemainingWorkingBytes != shared.MaxWorkingBytes)
    throw new Exception("Package consumer caller-owned grayscale admission differs.");
using var callerEncoded = new MemoryStream();
ImageCodec.EncodeJpeg(callerSamples, 8, 8, JpegSampleFormat.Rgb, callerEncoded, 27,
    new JpegEncodeOptions { Quality = 100, Sampling = JpegSampling.Grayscale, RestartInterval = 1, BaselineOnly = true });
if (!callerEncoded.ToArray().AsSpan().SequenceEqual(jpeg.ToArray()) || !callerEncoded.CanWrite)
    throw new Exception("Package consumer caller RGB encoding differs.");
using var callerEncodedGray = new MemoryStream();
shared.EncodeJpeg(callerGray, 8, 8, JpegSampleFormat.Grayscale, callerEncodedGray,
    options: new JpegEncodeOptions { Quality = 100, RestartInterval = 1, Limits = sharedLimits });
if (!callerEncodedGray.ToArray().AsSpan().SequenceEqual(jpeg.ToArray()) || shared.RemainingWorkingBytes != shared.MaxWorkingBytes)
    throw new Exception("Package consumer caller grayscale encoding/admission differs.");
try
{
    new ImageBudget(1, sharedLimits.MaxWork).Identify(jpeg.ToArray(), sharedLimits);
    throw new Exception("Package consumer accepted exhausted shared memory.");
}
catch (ImageLimitException failure)
{
    if (failure.Limit != ImageLimitKind.WorkingBytes || failure.Scope != ImageLimitScope.SharedBudget)
        throw new Exception("Package consumer shared limit category differs.");
}
// Independently authored TIFF IFD0: 63 unrelated inline LONG tags followed by
// a SHORT orientation. Validate error classification through the actual package.
var tiff = new byte[14 + 64*12]; tiff[0] = tiff[1] = (byte)'I';
System.Buffers.Binary.BinaryPrimitives.WriteUInt16LittleEndian(tiff.AsSpan(2), 42);
System.Buffers.Binary.BinaryPrimitives.WriteUInt32LittleEndian(tiff.AsSpan(4), 8);
System.Buffers.Binary.BinaryPrimitives.WriteUInt16LittleEndian(tiff.AsSpan(8), 64);
for (var i = 0; i < 64; i++)
{
    var entry = tiff.AsSpan(10 + i*12, 12);
    System.Buffers.Binary.BinaryPrimitives.WriteUInt16LittleEndian(entry, i == 63 ? (ushort)0x0112 : (ushort)(0xC000+i));
    System.Buffers.Binary.BinaryPrimitives.WriteUInt16LittleEndian(entry[2..], i == 63 ? (ushort)3 : (ushort)4);
    System.Buffers.Binary.BinaryPrimitives.WriteUInt32LittleEndian(entry[4..], 1);
    if (i == 63) System.Buffers.Binary.BinaryPrimitives.WriteUInt16LittleEndian(entry[8..], 6);
}
using var metadataJpeg = new MemoryStream();
metadataJpeg.Write(jpeg.ToArray().AsSpan(0, 2));
var app1Length = tiff.Length + 8;
metadataJpeg.Write(new byte[] { 255, 225, (byte)(app1Length >> 8), (byte)app1Length });
metadataJpeg.Write("Exif\0\0"u8); metadataJpeg.Write(tiff); metadataJpeg.Write(jpeg.ToArray().AsSpan(2));
var metadataBytes = metadataJpeg.ToArray();
var metadataLimits = new ImageLimits { MaxContainerItems = 64 };
if (ImageCodec.Identify(metadataBytes, metadataLimits).Orientation != ImageOrientation.Rotate90)
    throw new Exception("Package consumer bounded EXIF orientation differs.");
using var metadataPixels = ImageCodec.Decode(metadataBytes, new DecodeOptions { ApplyOrientation = true, Limits = metadataLimits });
if (metadataPixels.Orientation != ImageOrientation.Normal || !metadataPixels.GetRow8(0).SequenceEqual(opaque.GetRow8(0)))
    throw new Exception("Package consumer oriented JPEG pixels differ.");
try
{
    ImageCodec.Identify(metadataBytes, metadataLimits with { MaxContainerItems = 63 });
    throw new Exception("Package consumer EXIF entry limit was not enforced.");
}
catch (ImageLimitException) { }
using var rejectedMetadata = new MemoryStream(metadataBytes);
try
{
    using var unexpected = ImageCodec.Decode(rejectedMetadata, new DecodeOptions { Limits = metadataLimits with { MaxContainerItems = 63 } });
    throw new Exception("Package consumer stream EXIF entry limit was not enforced.");
}
catch (ImageLimitException) { }
if (!rejectedMetadata.CanRead) throw new Exception("Package consumer EXIF failure closed its input stream.");
using var webp = new MemoryStream();
ImageCodec.EncodeWebp(flipped, webp, new WebpEncodeOptions { Effort = 6 });
using var webpPixels = ImageCodec.Decode(webp.ToArray());
if (webpPixels.Width != 2 || webpPixels.Height != 1 || webpPixels.BitDepth != 8 || !webp.CanWrite)
    throw new Exception("Package consumer WebP contract differs.");
for (var sample = 0; sample < 8; sample++)
    if (webpPixels.GetRow8(0)[sample] != (flipped.GetRow16(0)[sample] + 128) / 257)
        throw new Exception("Package consumer WebP normalized pixels differ.");
var webpLimits = new ImageLimits { MaxWorkingBytes = 16 + 8192, MaxOutputBytes = (int)webp.Length };
using var admittedWebp = new MemoryStream();
ImageCodec.EncodeWebp(flipped, admittedWebp, new WebpEncodeOptions { Effort = 6, Limits = webpLimits });
if (!admittedWebp.ToArray().AsSpan().SequenceEqual(webp.ToArray()))
    throw new Exception("Package consumer VP8L encoder admission changes its output.");
foreach (var exhausted in new[] { webpLimits with { MaxWorkingBytes = webpLimits.MaxWorkingBytes - 1 }, webpLimits with { MaxOutputBytes = webpLimits.MaxOutputBytes - 1 } })
{
    using var rejected = new MemoryStream();
    try
    {
        ImageCodec.EncodeWebp(flipped, rejected, new WebpEncodeOptions { Effort = 6, Limits = exhausted });
        throw new Exception("Package consumer VP8L encoder admission was not enforced.");
    }
    catch (ImageLimitException) { }
    if (rejected.Length != 0 || !rejected.CanWrite) throw new Exception("Package consumer VP8L admission writes output or closes its stream.");
}
// The fixed full-alphabet encoder's five trees peak while building the distance
// tree: 512 parser + 72 references + 3376 existing trees + 72 lengths + 400 tree.
var webpDecodeLimits = new ImageLimits { MaxWorkingBytes = webp.Length + 8944 };
using var admittedWebpPixels = ImageCodec.Decode(webp.ToArray(), new DecodeOptions { Limits = webpDecodeLimits });
if (!admittedWebpPixels.GetRow8(0).SequenceEqual(webpPixels.GetRow8(0)))
    throw new Exception("Package consumer VP8L decode admission changes its pixels.");
try
{
    using var unexpected = ImageCodec.Decode(webp.ToArray(), new DecodeOptions { Limits = webpDecodeLimits with { MaxWorkingBytes = webpDecodeLimits.MaxWorkingBytes - 1 } });
    throw new Exception("Package consumer VP8L decode admission was not enforced.");
}
catch (ImageLimitException) { }
using var hidden = ImageBuffer.Create(1, 1, 8);
new byte[] { 12, 34, 56, 0 }.CopyTo(hidden.GetRow8(0));
using var hiddenOutput = new MemoryStream();
ImageCodec.EncodeWebp(hidden, hiddenOutput);
using var hiddenPixels = ImageCodec.Decode(hiddenOutput.ToArray());
if (!hiddenPixels.GetRow8(0).SequenceEqual(hidden.GetRow8(0)))
    throw new Exception("Package consumer hidden RGB differs.");
using var lossyOutput = new MemoryStream();
ImageCodec.EncodeWebp(flipped, lossyOutput, new WebpEncodeOptions { Lossless = false, Quality = 85, Effort = 6 });
using var lossyPixels = ImageCodec.Decode(lossyOutput.ToArray());
if (lossyPixels.Width != 2 || lossyPixels.Height != 1 || lossyPixels.BitDepth != 8 || !lossyOutput.CanWrite)
    throw new Exception("Package consumer VP8 encoder contract differs.");
for (var x = 0; x < 2; x++)
    if (lossyPixels.GetRow8(0)[x*4+3] != (flipped.GetRow16(0)[x*4+3]+128)/257)
        throw new Exception("Package consumer VP8 encoder alpha differs.");
// A single macroblock charges 8192 parser + (20+13+256) metadata +
// (384+128) padded YUV planes. This exceeds the small raw-alpha/render phase.
var vp8DecodeLimits = new ImageLimits { MaxWorkingBytes = lossyOutput.Length + 8993 };
using var admittedVp8 = ImageCodec.Decode(lossyOutput.ToArray(), new DecodeOptions { Limits = vp8DecodeLimits });
if (!admittedVp8.GetRow8(0).SequenceEqual(lossyPixels.GetRow8(0)))
    throw new Exception("Package consumer VP8 decode admission changes its pixels.");
try
{
    using var unexpected = ImageCodec.Decode(lossyOutput.ToArray(), new DecodeOptions { Limits = vp8DecodeLimits with { MaxWorkingBytes = vp8DecodeLimits.MaxWorkingBytes - 1 } });
    throw new Exception("Package consumer VP8 reconstruction admission was not enforced.");
}
catch (ImageLimitException) { }
using var vp8Input = File.OpenRead(args[0]);
using var vp8Pixels = ImageCodec.Decode(vp8Input);
var vp8Expected = File.ReadAllBytes(args[1]);
if (vp8Pixels.Width != 31 || vp8Pixels.Height != 23 || vp8Pixels.BitDepth != 8 || !vp8Input.CanRead)
    throw new Exception("Package consumer VP8 contract differs.");
double vp8Squares = 0; var vp8Maximum = 0;
for (var y = 0; y < 23; y++)
for (var sample = 0; sample < 31*4; sample++)
{
    var actual = vp8Pixels.GetRow8(y)[sample]; var expected = vp8Expected[y*31*4+sample];
    if (sample % 4 == 3) { if (actual != expected) throw new Exception("Package consumer VP8 alpha differs."); }
    else { var difference = Math.Abs(actual-expected); vp8Maximum = Math.Max(vp8Maximum,difference); vp8Squares += difference*difference; }
}
if (vp8Maximum > 4 || Math.Sqrt(vp8Squares/(31*23*3)) > 1.5)
    throw new Exception("Package consumer VP8 RGB differs.");
Console.WriteLine("Exact Release package consumer passed.");
'@ | Set-Content -LiteralPath (Join-Path $consumer 'Program.cs') -Encoding utf8
    $config = [xml]'<configuration><packageSources><clear /><add key="local" value="" /></packageSources></configuration>'
    $config.configuration.packageSources.add.value = $feedDirectory
    $config.Save((Join-Path $consumer 'NuGet.Config'))
    # An isolated package cache proves this exact pack is used even when its
    # prerelease version matches an older local build.
    dotnet restore (Join-Path $consumer 'Consumer.csproj') --configfile (Join-Path $consumer 'NuGet.Config') --packages (Join-Path $consumer 'packages') --tl:off --nologo -v minimal
    if ($LASTEXITCODE -ne 0) { throw 'Package-only consumer restore failed.' }
    $vp8Fixture = Join-Path $repoRoot 'tests/Fixtures/vp8-ffmpeg-31x23-effort6-quality75-alpha.webp'
    $vp8Expected = Join-Path $repoRoot 'tests/Fixtures/vp8-ffmpeg-31x23-effort6-quality75-alpha.rgba'
    $pngFixture = Join-Path $repoRoot 'tests/Fixtures/png-authored-c6-d16-adam7-all-filters.png'
    $pngExpected = Join-Path $repoRoot 'tests/Fixtures/png-authored-c6-d16-adam7-all-filters.rgba'
    dotnet build (Join-Path $consumer 'Consumer.csproj') -c Release --no-restore --tl:off --nologo -v minimal -m:1 -nr:false -p:UseSharedCompilation=false
    if ($LASTEXITCODE -ne 0) { throw 'Package-only consumer build failed.' }
    dotnet (Join-Path $consumer 'bin/Release/net10.0/Consumer.dll') $vp8Fixture $vp8Expected $pngFixture $pngExpected $librarySha256
    if ($LASTEXITCODE -ne 0) { throw 'Package-only consumer failed.' }
    & (Join-Path $PSScriptRoot 'verify-usage.ps1') -ConsumerDirectory $consumer -GuidePath (Join-Path $repoRoot 'docs/usage.md') -LibrarySha256 $librarySha256
} finally { Pop-Location }
