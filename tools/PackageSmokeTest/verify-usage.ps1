param(
    [Parameter(Mandatory)][string]$ConsumerDirectory,
    [Parameter(Mandatory)][string]$GuidePath,
    [Parameter(Mandatory)][string]$LibrarySha256
)
$ErrorActionPreference = 'Stop'
$guide = [IO.File]::ReadAllText($GuidePath)
$snippets = [regex]::Matches($guide, '(?ms)^```csharp\r?\n(.*?)\r?\n```$')
if ($snippets.Count -ne 3) { throw 'Expected the three runnable usage-guide examples.' }
$cache = Join-Path $ConsumerDirectory 'packages'
$config = Join-Path $ConsumerDirectory 'NuGet.Config'

$inputSetup = @'
const int docWidth = 300, docHeight = 200;
using var docSeed = ImageBuffer.Create(docWidth, docHeight, 16);
for (var docY = 0; docY < docHeight; docY++)
for (var docX = 0; docX < docWidth; docX++)
for (var docChannel = 0; docChannel < 4; docChannel++)
    docSeed.GetRow16(docY)[docX * 4 + docChannel] = DocSample(docX, docY, docChannel);
using (var docFixture = File.Create("input.png")) ImageCodec.EncodePng(docSeed, docFixture);
var docInputBytes = File.ReadAllBytes("input.png");
var docInputHash = Convert.ToHexStringLower(SHA256.HashData(docInputBytes));
'@
$checks = @(
@'
if (!input.CanRead || !output.CanWrite || image.BitDepth != 16 || thumbnail.BitDepth != 16)
    throw new Exception("Thumbnail example ownership or precision differs.");
if (thumbnail.Width > 128 || thumbnail.Height > 96 || thumbnail.Width > crop.Width || thumbnail.Height > crop.Height)
    throw new Exception("Thumbnail example violates fit/no-upscale bounds.");
output.Dispose(); // The caller closes its output before reopening the file.
using var docDecoded = ImageCodec.Decode(File.ReadAllBytes("thumbnail.png"));
if ((docDecoded.Width, docDecoded.Height, docDecoded.BitDepth) != (thumbnail.Width, thumbnail.Height, 16))
    throw new Exception("Thumbnail output geometry or precision differs.");
for (var docY = 0; docY < image.Height; docY++)
for (var docX = 0; docX < image.Width; docX++)
for (var docChannel = 0; docChannel < 4; docChannel++)
    if (image.GetRow16(docY)[docX * 4 + docChannel] != DocSample(docX, docY, docChannel))
        throw new Exception("Thumbnail source changed.");
for (var docY = 0; docY < thumbnail.Height; docY++)
    if (!docDecoded.GetRow16(docY).SequenceEqual(thumbnail.GetRow16(docY)))
        throw new Exception("Thumbnail PNG changes edited samples.");
var docOriginal = image.GetRow16(0)[0];
crop.GetRow16(0)[0] ^= 1;
if (image.GetRow16(0)[0] != docOriginal) throw new Exception("Crop shares source storage.");
'@,
@'
if (!output.CanWrite || (rotated.Width, rotated.Height, enlarged.Width, enlarged.Height, enlarged.BitDepth) != (2, 1, 4, 2, 16))
    throw new Exception("16-bit example geometry, precision or stream ownership differs.");
output.Dispose();
using var docDecoded = ImageCodec.Decode(File.ReadAllBytes("samples16.png"));
ushort[] docExpected = [1000, 2000, 3000, 65535, 1000, 2000, 3000, 65535,
    12345, 23456, 34567, 32768, 12345, 23456, 34567, 32768];
if ((docDecoded.Width, docDecoded.Height, docDecoded.BitDepth) != (4, 2, 16))
    throw new Exception("16-bit PNG dimensions differ.");
for (var docY = 0; docY < 2; docY++)
    if (!enlarged.GetRow16(docY).SequenceEqual(docExpected) || !docDecoded.GetRow16(docY).SequenceEqual(docExpected))
        throw new Exception("Crop/flip/quarter-turn/nearest example differs from its direct coordinate result.");
if (!source.GetRow16(0).SequenceEqual(new ushort[] {1000, 2000, 3000, 65535, 4000, 5000, 6000, 65535})
    || !source.GetRow16(1).SequenceEqual(new ushort[] {12345, 23456, 34567, 32768, 65535, 0, 65535, 0}))
    throw new Exception("16-bit source was mutated.");
'@,
@'
if (!input.CanRead || !encoded.CanWrite || thumbnail.Width > 128 || thumbnail.Height > 96)
    throw new Exception("Bounded workflow geometry or ownership differs.");
using var docDecoded = ImageCodec.Decode(File.ReadAllBytes("bounded-thumbnail.png"));
if ((docDecoded.Width, docDecoded.Height, docDecoded.BitDepth) != (thumbnail.Width, thumbnail.Height, 16))
    throw new Exception("Bounded output geometry or precision differs.");
for (var docY = 0; docY < thumbnail.Height; docY++)
    if (!docDecoded.GetRow16(docY).SequenceEqual(thumbnail.GetRow16(docY)))
        throw new Exception("Bounded output changes pixels.");
cancellation.Cancel();
try
{
    using var docUnexpected = ImageCodec.Decode(docInputBytes, new DecodeOptions { Limits = limits }, token);
    throw new Exception("Documented token did not propagate cancellation.");
}
catch (OperationCanceledException) { }
'@
)
$inputCheck = @'
if (Convert.ToHexStringLower(SHA256.HashData(File.ReadAllBytes("input.png"))) != docInputHash)
    throw new Exception("An example changed its input file.");
'@
$sampleFunction = @'
static ushort DocSample(int x, int y, int channel) => channel == 3
    ? (ushort)((x + y) % 3 == 0 ? 0 : 65535 - (x * 257 + y * 11) % 65536)
    : (ushort)((x * 1103 + y * 1789 + channel * 997 + 13) & 65535);
'@
$identityCheck = @'
var docLibrary = typeof(ImageCodec).Assembly.Location;
if (Convert.ToHexStringLower(SHA256.HashData(File.ReadAllBytes(docLibrary))) != args[0])
    throw new Exception("Usage consumer loaded a different package assembly.");
Console.WriteLine("Usage example passed through the exact Release package.");
'@

for ($index = 0; $index -lt $snippets.Count; $index++) {
    $directory = Join-Path $ConsumerDirectory ('usage-example-' + ($index + 1))
    New-Item -ItemType Directory -Path $directory | Out-Null
    $project = Join-Path $directory 'Example.csproj'
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
'@ | Set-Content -LiteralPath $project -Encoding utf8
    $snippet = [regex]::Replace($snippets[$index].Groups[1].Value, '^using Lokad\.Image;\r?\n', '')
    $parts = @('using Lokad.Image;', 'using System.Security.Cryptography;')
    if ($index -ne 1) { $parts += $inputSetup }
    $parts += @($snippet, $checks[$index])
    if ($index -ne 1) { $parts += $inputCheck }
    $parts += $identityCheck
    if ($index -ne 1) { $parts += $sampleFunction }
    ($parts -join "`n") | Set-Content -LiteralPath (Join-Path $directory 'Program.cs') -Encoding utf8
    dotnet restore $project --configfile $config --packages $cache --tl:off --nologo --disable-parallel -p:NuGetAudit=false -v minimal
    if ($LASTEXITCODE -ne 0) { throw 'Usage example package-only restore failed.' }
    $assets = Get-Content -LiteralPath (Join-Path $directory 'obj/project.assets.json') -Raw | ConvertFrom-Json
    $libraries = @($assets.libraries.PSObject.Properties.Name)
    if ($libraries.Count -ne 1 -or $libraries[0] -ne 'Lokad.Image/0.1.0') { throw 'Usage example package graph is not dependency-free.' }
    dotnet build $project -c Release --no-restore --tl:off --nologo -v minimal -m:1 -nr:false -p:UseSharedCompilation=false
    if ($LASTEXITCODE -ne 0) { throw 'Usage example Release build failed.' }
    Push-Location $directory
    try {
        dotnet (Join-Path $directory 'bin/Release/net10.0/Example.dll') $LibrarySha256
        if ($LASTEXITCODE -ne 0) { throw 'Usage example package-only execution failed.' }
    } finally { Pop-Location }
}
Write-Host 'All three usage-guide examples passed through the exact dependency-free Release package.'
