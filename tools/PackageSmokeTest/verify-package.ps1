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
        $package = Join-Path $repoRoot 'artifacts/nuget/Lokad.Image.0.1.0.nupkg'
    }
    $archive = [IO.Compression.ZipFile]::OpenRead($package)
    try {
        $nuspec = $archive.Entries | Where-Object { $_.FullName.EndsWith('.nuspec') }
        $reader = [IO.StreamReader]::new($nuspec.Open())
        try { [xml]$metadata = $reader.ReadToEnd() } finally { $reader.Dispose() }
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
        foreach ($document in @('README.md', 'SPEC.md', 'LICENSE.txt', 'icon.png', 'docs/resource-admission.md', 'docs/jpeg-inverse-rounding.md', 'docs/jpeg-forward-quantization.md', 'docs/usage.md')) {
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
    Copy-Item -LiteralPath $package -Destination (Join-Path $feedDirectory 'Lokad.Image.0.1.0.nupkg')
    @'
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>Exe</OutputType><TargetFramework>net10.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings><Nullable>enable</Nullable>
    <ManagePackageVersionsCentrally>false</ManagePackageVersionsCentrally>
    <RestorePackagesWithLockFile>false</RestorePackagesWithLockFile>
  </PropertyGroup>
  <ItemGroup><PackageReference Include="Lokad.Image" Version="0.1.0" /></ItemGroup>
</Project>
'@ | Set-Content -LiteralPath (Join-Path $consumer 'Consumer.csproj') -Encoding utf8
    @'
using Lokad.Image;
if (Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(
    File.ReadAllBytes(typeof(ImageBuffer).Assembly.Location))).ToLowerInvariant() != args[4])
    throw new Exception("Package consumer loaded a different assembly.");
using var source = ImageBuffer.Create(2, 1, 16);
new ushort[] { 12345, 1, 65534, 43210, 54321, 65534, 1, 12345 }.CopyTo(source.GetRow16(0));
using var flipped = ImageTransforms.FlipHorizontal(source);
using var encoded = new MemoryStream();
ImageCodec.EncodePng(flipped, encoded);
using var decoded = ImageCodec.Decode(encoded.ToArray());
if (decoded.BitDepth != 16 || !decoded.GetRow16(0).SequenceEqual(flipped.GetRow16(0)))
    throw new Exception("Exact package consumer pixels differ.");
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
using var originalPng = ImageCodec.Decode(File.ReadAllBytes(args[2]));
var originalPngExpected = File.ReadAllBytes(args[3]);
if (originalPng.Width != 19 || originalPng.Height != 17 || originalPng.BitDepth != 16)
    throw new Exception("Package consumer authored PNG profile differs.");
for (var y = 0; y < 17; y++)
for (var sample = 0; sample < 19*4; sample++)
    if (originalPng.GetRow16(y)[sample] != System.Buffers.Binary.BinaryPrimitives.ReadUInt16LittleEndian(originalPngExpected.AsSpan((y*19*4+sample)*2)))
        throw new Exception("Package consumer authored PNG samples differ.");
using var opaque = ImageBuffer.Create(8, 8, 8);
for (var y = 0; y < 8; y++)
for (var x = 0; x < 8; x++) new byte[] { 123, 123, 123, 255 }.CopyTo(opaque.GetRow8(y)[(x*4)..]);
using var jpeg = new MemoryStream();
ImageCodec.EncodeJpeg(opaque, jpeg, new JpegEncodeOptions { Sampling = JpegSampling.Grayscale, Quality = 100, RestartInterval = 1 });
using var jpegPixels = ImageCodec.Decode(jpeg.ToArray());
if (jpegPixels.Width != 8 || jpegPixels.Height != 8 || jpegPixels.BitDepth != 8 || !jpeg.CanWrite)
    throw new Exception("Package consumer JPEG contract differs.");
for (var y = 0; y < 8; y++)
    if (!jpegPixels.GetRow8(y).SequenceEqual(opaque.GetRow8(y))) throw new Exception("Package consumer JPEG pixels differ.");
var sharedLimits = new ImageLimits { MaxWorkingBytes = 2 * 1024 * 1024, MaxWork = 2_000_000 };
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
