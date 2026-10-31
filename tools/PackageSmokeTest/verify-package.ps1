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
        $package = Join-Path $repoRoot "artifacts/nugets/Lokad.Image.$projectVersion.nupkg"
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
        foreach ($document in @('README.md', 'docs/spec.md', 'LICENSE.txt', 'icon.png', 'docs/resource-admission.md', 'docs/codec-parity.md', 'docs/lanczos3-fma-certificate.md', 'docs/jpeg-inverse-rounding.md', 'docs/usage.md', 'docs/migration.md', 'docs/text-compatibility.md', 'docs/source-metadata.md')) {
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
    $streamingConsumerPath = [Security.SecurityElement]::Escape((Join-Path $repoRoot 'tools/PackageSmokeTest/GovernedStreamingConsumer.cs'))
    $shapingFixturePath = [Security.SecurityElement]::Escape((Join-Path $repoRoot 'tools/Shared/AuthoredOpenType.cs'))
    $shapingConsumerPath = [Security.SecurityElement]::Escape((Join-Path $repoRoot 'tools/PackageSmokeTest/TextShapingConsumer.cs'))
    $jpegFixturePath = [Security.SecurityElement]::Escape((Join-Path $repoRoot 'tools/Shared/AuthoredJpegCompatibility.cs'))
    $progressiveJpegFixturePath = [Security.SecurityElement]::Escape((Join-Path $repoRoot 'tools/Shared/AuthoredJpeg.cs'))
    $directRgbFixturePath = [Security.SecurityElement]::Escape((Join-Path $repoRoot 'tools/Shared/OriginalDirectRgbControls.cs'))
    $pixelFixturePath = [Security.SecurityElement]::Escape((Join-Path $repoRoot 'tools/Shared/OriginalPixels.cs'))
    $vp8lFixturePath = [Security.SecurityElement]::Escape((Join-Path $repoRoot 'tools/Shared/OriginalVp8lControls.cs'))
    $authoredWebpPath = [Security.SecurityElement]::Escape((Join-Path $repoRoot 'tools/Shared/AuthoredWebp.cs'))
    $authoredPngPath = [Security.SecurityElement]::Escape((Join-Path $repoRoot 'tools/FormatCases/AuthoredPng.cs'))
    $vp8FixturePath = [Security.SecurityElement]::Escape((Join-Path $repoRoot 'tools/Shared/OriginalVp8Controls.cs'))
    $pngGradientPath = [Security.SecurityElement]::Escape((Join-Path $repoRoot 'tools/Shared/OriginalPngGradient.cs'))
    $pngOraclePath = [Security.SecurityElement]::Escape((Join-Path $repoRoot 'tools/Shared/IndependentRgbaPng.cs'))
    $pngConsumerPath = [Security.SecurityElement]::Escape((Join-Path $repoRoot 'tools/PackageSmokeTest/PngCompressionConsumer.cs'))
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
    <Compile Include="$streamingConsumerPath" Link="GovernedStreamingConsumer.cs" />
    <Compile Include="$shapingFixturePath" Link="AuthoredOpenType.cs" />
    <Compile Include="$shapingConsumerPath" Link="TextShapingConsumer.cs" />
    <Compile Include="$jpegFixturePath" Link="AuthoredJpegCompatibility.cs" />
    <Compile Include="$progressiveJpegFixturePath" Link="AuthoredJpeg.cs" />
    <Compile Include="$directRgbFixturePath" Link="OriginalDirectRgbControls.cs" />
    <Compile Include="$pixelFixturePath" Link="OriginalPixels.cs" />
    <Compile Include="$vp8lFixturePath" Link="OriginalVp8lControls.cs" />
    <Compile Include="$authoredWebpPath" Link="AuthoredWebp.cs" />
    <Compile Include="$authoredPngPath" Link="AuthoredPng.cs" />
    <Compile Include="$vp8FixturePath" Link="OriginalVp8Controls.cs" />
    <Compile Include="$pngGradientPath" Link="OriginalPngGradient.cs" />
    <Compile Include="$pngOraclePath" Link="IndependentRgbaPng.cs" />
    <Compile Include="$pngConsumerPath" Link="PngCompressionConsumer.cs" />
  </ItemGroup>
</Project>
"@ | Set-Content -LiteralPath (Join-Path $consumer 'Consumer.csproj') -Encoding utf8
    @'
using System.IO.Compression;
using Lokad.Image;
foreach (var control in Lokad.Image.Tooling.OriginalVp8Controls.Cases())
{
    var name = (string)control[0]; var depth = (int)control[1]; var effort = (int)control[2]; var quality = (int)control[3];
    var length = (int)control[4]; var hash = (string)control[5];
    using var root = new Lokad.Image.Tooling.TestMemoryBudget(128_000_000);
    using var image = Lokad.Image.Tooling.OriginalVp8lControls.Buffer(name, depth, root);
    var before = Lokad.Image.Tooling.OriginalPixels.Bytes(image); var retained = root.Bytes;
    var options = new WebpEncodeOptions { Lossless = false, Effort = effort, Quality = quality,
        Limits = new() { MemoryBudget = root } };
    using var packet = new MemoryStream(); ImageCodec.EncodeWebp(image, packet, options);
    using var asynchronous = new MemoryStream(); await ImageCodec.EncodeWebpAsync(image, asynchronous, options);
    var bytes = packet.ToArray();
    if (bytes.Length != length || Convert.ToHexStringLower(System.Security.Cryptography.SHA256.HashData(bytes)) != hash
        || !bytes.AsSpan().SequenceEqual(asynchronous.ToArray()))
        throw new Exception($"Package VP8 complete reference packet differs: {name}/{depth}/{effort}/{quality}.");
    if (root.Bytes != retained || !before.AsSpan().SequenceEqual(Lokad.Image.Tooling.OriginalPixels.Bytes(image)))
        throw new Exception("Package VP8 source/admission ownership differs.");
    image.Dispose(); if (root.Bytes != 0 || !packet.CanWrite || !asynchronous.CanWrite || root.IsDisposed)
        throw new Exception("Package VP8 retained-source/output ownership differs.");
}
foreach (var control in Lokad.Image.Tooling.OriginalVp8lControls.Cases())
{
    var name = (string)control[0]; var effort = (int)control[1]; var length = (int)control[2]; var hash = (string)control[3];
    using var root = new Lokad.Image.Tooling.TestMemoryBudget(128_000_000);
    using var image = Lokad.Image.Tooling.OriginalVp8lControls.Buffer(name, 8, root);
    var before = Lokad.Image.Tooling.OriginalPixels.Bytes(image); var retained = root.Bytes;
    using var packet = new MemoryStream();
    ImageCodec.EncodeWebp(image, packet, new() { Effort = effort, Limits = new() { MemoryBudget = root } });
    if (packet.Length != length || Convert.ToHexStringLower(System.Security.Cryptography.SHA256.HashData(packet.ToArray())) != hash)
        throw new Exception($"Package VP8L reference packet differs: {name}/{effort}.");
    if (root.Bytes != retained || !before.AsSpan().SequenceEqual(Lokad.Image.Tooling.OriginalPixels.Bytes(image)))
        throw new Exception("Package VP8L source/admission ownership differs.");
    image.Dispose(); if (root.Bytes != 0 || !packet.CanWrite || root.IsDisposed)
        throw new Exception("Package VP8L retained-source/output ownership differs.");
}
foreach (var pngCompression in new[] { CompressionLevel.Optimal, CompressionLevel.NoCompression, CompressionLevel.Fastest, CompressionLevel.SmallestSize })
foreach (var pngDepth in new[] { 8, 16 })
foreach (var pngColor in new[] { PngColorType.Grayscale, PngColorType.Rgb, PngColorType.GrayscaleAlpha, PngColorType.Rgba })
{
    if (pngCompression != CompressionLevel.Optimal && pngColor != PngColorType.Rgba) continue;
    using var pngImage = Lokad.Image.Tooling.OriginalPixels.Buffer(513, 259, pngDepth,
        grayscale: pngColor is PngColorType.Grayscale or PngColorType.GrayscaleAlpha,
        opaque: pngColor is PngColorType.Grayscale or PngColorType.Rgb);
    if (pngColor is PngColorType.GrayscaleAlpha or PngColorType.Rgba)
    {
        if (pngDepth == 8) pngImage.GetRow8(0)[3] = pngImage.GetRow8(0)[513 * 4 - 1] = 0;
        else pngImage.GetRow16(0)[3] = pngImage.GetRow16(0)[513 * 4 - 1] = 0;
    }
    var expectedPngHash = pngCompression != CompressionLevel.Optimal ? (pngDepth, pngCompression) switch
    {
        (8, CompressionLevel.NoCompression) => "c8f17d7b3b3c1aff850539b1be0ee318956a58ec6ce555da332838ed0cc1cd3c",
        (16, CompressionLevel.NoCompression) => "9b31604447d61d5ab7a30aad3cfde4cd73fef6d85ff506e74baeba807e54574d",
        (8, CompressionLevel.Fastest) => "a024058695b0f61d4a57440b08975132958f8fdd151f1c0c58d56dce1c720b14",
        (16, CompressionLevel.Fastest) => "adbcc5115ee9acce294ab70f65b6f24862b7451d90f870a082269a8008c08e25",
        (8, CompressionLevel.SmallestSize) => "4d8f15d9fdb8f415544b831c8f0845c1532adc8696f2707264bced25fc424c3c",
        (16, CompressionLevel.SmallestSize) => "40135537a01f3ddcda1fef33d138dd00c08c5744d5639d377b1728874da95d6d",
        _ => throw new Exception("Unknown PNG policy control.")
    } : (pngColor, pngDepth) switch
    {
        (PngColorType.Grayscale, 8) => "c5b4a248c1ca0f76115d06e7795698f50ac7e623c366fcee4a659cb08da2872a",
        (PngColorType.Rgb, 8) => "e1b0f39154bb07b69c6fab59512dbd55f1fb855c0a9805a44892d510efb73228",
        (PngColorType.GrayscaleAlpha, 8) => "eefb49b94adbd654b38b4e33edc8aac89fdfd9273a8124a1fbf7884ee8789fb5",
        (PngColorType.Rgba, 8) => "ad9ed127da583d68bdb749bb983b73966e8e814527216449c4557fdd89668920",
        (PngColorType.Grayscale, 16) => "97b2e6d66070a7f42d3f487e837bd5c3ef2a1beb0362d5ec02110794007c3f70",
        (PngColorType.Rgb, 16) => "4b8a2c3c99342ab51f2919c35f5dfb50506cb6c5812e84ede61c53f0db7388bf",
        (PngColorType.GrayscaleAlpha, 16) => "addd3f236e49cef7fc937ab70022420681cf4d9f61d895b680605934429aca76",
        (PngColorType.Rgba, 16) => "4f8f4d9950068c51b006ea87e2200523a5064e00842373eeb9c3a7f38bc12a0b",
        _ => throw new Exception("Unknown PNG reference control.")
    };
    using var pngRoot = new Lokad.Image.Tooling.TestMemoryBudget(4 * 1024 * 1024);
    using var pngPacket = new MemoryStream();
    await ImageCodec.EncodePngAsync(pngImage, pngPacket, new PngEncodeOptions
    { ColorType = pngColor, Compression = pngCompression, Limits = ImageLimits.Default with { MemoryBudget = pngRoot } });
    if (Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(pngPacket.ToArray())).ToLowerInvariant()
        != expectedPngHash || pngRoot.Bytes != 0 || pngRoot.IsDisposed)
        throw new Exception("Packaged managed PNG bytes or shared ownership differ.");
}
foreach (var referenceDepth in new[] { 8, 16 })
foreach (var referenceSampling in new[] { JpegSampling.YCbCr444, JpegSampling.YCbCr422, JpegSampling.YCbCr420, JpegSampling.Grayscale })
{
    using var referenceImage = Lokad.Image.Tooling.OriginalPixels.Buffer(513, 259, referenceDepth);
    if (referenceDepth == 8) { referenceImage.GetRow8(0)[3] = 0; referenceImage.GetRow8(0)[513 * 4 - 1] = 0; }
    else { referenceImage.GetRow16(0)[3] = 0; referenceImage.GetRow16(0)[513 * 4 - 1] = 0; }
    using var referenceRoot = new Lokad.Image.Tooling.TestMemoryBudget(
        (referenceSampling == JpegSampling.YCbCr444 ? 4 : 2) * 1024 * 1024);
    using var referencePacket = new MemoryStream();
    await ImageCodec.EncodeJpegAsync(referenceImage, referencePacket, new JpegEncodeOptions
    {
        Sampling = referenceSampling,
        Quality = referenceSampling == JpegSampling.Grayscale ? 69 : 85,
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
        (JpegSampling.Grayscale, 8) => "444e39e6126dd3077d22c0b4f60e38d8a22611fa5721b226c5168a63b7e94423",
        (JpegSampling.Grayscale, 16) => "2a99dee8551c14e8acceb8df6adec6a2a263a9f037ec00cabe2791c96b178bfc",
        _ => throw new Exception("Unknown reference control.")
    };
    if (Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(referencePacket.ToArray())).ToLowerInvariant()
        != expectedReferenceHash || referenceRoot.Bytes != 0 || referenceRoot.IsDisposed)
        throw new Exception("Packaged reference JPEG bytes or shared ownership differ.");
    if (referenceSampling == JpegSampling.YCbCr444)
    {
        // Keep the former2MiB shared root as a terminal-refusal check. A new,
        // independent call declares a tight local workspace before admission,
        // selecting recomputation without a retry inside the failed operation.
        using var constrainedRoot = new Lokad.Image.Tooling.TestMemoryBudget(2 * 1024 * 1024);
        using var refusedPacket = new MemoryStream();
        var constrained = new JpegEncodeOptions
        { Sampling = referenceSampling, Quality = 85, BackgroundRgb = 0x204060,
            Limits = ImageLimits.Default with { MemoryBudget = constrainedRoot } };
        try
        {
            await ImageCodec.EncodeJpegAsync(referenceImage, refusedPacket, constrained);
            throw new Exception("Packaged JPEG444 ignored insufficient shared admission.");
        }
        catch (ImageLimitException e) when (e.Limit == ImageLimitKind.WorkingBytes && e.Scope == ImageLimitScope.SharedBudget) { }
        if (constrainedRoot.Bytes != 0 || constrainedRoot.Refusals != 1 || refusedPacket.Length != 0 || constrainedRoot.IsDisposed)
            throw new Exception("Packaged JPEG444 retried refusal or retained storage.");
        using var recomputedPacket = new MemoryStream();
        await ImageCodec.EncodeJpegAsync(referenceImage, recomputedPacket, constrained with
        { Limits = constrained.Limits with { MaxWorkingBytes = 2 * 1024 * 1024 } });
        if (!recomputedPacket.ToArray().AsSpan().SequenceEqual(referencePacket.ToArray())
            || constrainedRoot.Bytes != 0 || constrainedRoot.Refusals != 1 || constrainedRoot.IsDisposed)
            throw new Exception("Packaged JPEG444 declared-tight recovery differs.");
    }
    if (referenceSampling == JpegSampling.YCbCr420 && referenceDepth == 8)
    {
        using var restartPacket = new MemoryStream();
        await ImageCodec.EncodeJpegAsync(referenceImage, restartPacket, new JpegEncodeOptions
        {
            Sampling = JpegSampling.YCbCr420,
            RestartInterval = 33, BackgroundRgb = 0x204060,
            Limits = ImageLimits.Default with { MemoryBudget = referenceRoot }
        });
        if (Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(restartPacket.ToArray())).ToLowerInvariant()
            != "1af33b85d17d4dd24a19f98e66242eba42c7c77b7347dfa564a83d64b19feea4" || referenceRoot.Bytes != 0)
            throw new Exception("Packaged reference row-restart bytes or ownership differ.");
    }
    if (referenceSampling == JpegSampling.Grayscale && referenceDepth == 8)
    {
        var gray = new byte[513 * 259];
        for (var y = 0; y < 259; y++)
        for (var x = 0; x < 513; x++)
        {
            var pixel = referenceImage.GetRow8(y).Slice(x * 4, 4); var alpha = pixel[3];
            var r = (pixel[0] * alpha + 32 * (255 - alpha) + 127) / 255;
            var g = (pixel[1] * alpha + 64 * (255 - alpha) + 127) / 255;
            var b = (pixel[2] * alpha + 96 * (255 - alpha) + 127) / 255;
            gray[y * 513 + x] = (byte)Math.Floor((0.299 * r + 0.587 * g) + 0.114 * b + 0.5);
        }
        using var callerGrayPacket = new MemoryStream();
        await ImageCodec.EncodeJpegAsync(gray, 513, 259, JpegSampleFormat.Grayscale, callerGrayPacket,
            options: new JpegEncodeOptions { Quality = 69,
                Limits = ImageLimits.Default with { MemoryBudget = referenceRoot } });
        if (!callerGrayPacket.ToArray().AsSpan().SequenceEqual(referencePacket.ToArray()) || referenceRoot.Bytes != 0)
            throw new Exception("Packaged reference caller gray sampling override or ownership differs.");
        using var grayRestartPacket = new MemoryStream();
        await ImageCodec.EncodeJpegAsync(gray, 513, 259, JpegSampleFormat.Grayscale, grayRestartPacket,
            options: new JpegEncodeOptions { Quality = 69,
                RestartInterval = 3, Limits = ImageLimits.Default with { MemoryBudget = referenceRoot } });
        if (Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(grayRestartPacket.ToArray())).ToLowerInvariant()
            != "25fb908c13a1e15645bfe07948e7f1d70366d6bf26e99eb3a2a88f112c78ea02" || referenceRoot.Bytes != 0)
            throw new Exception("Packaged reference grayscale restart bytes or ownership differ.");
    }
}
await Lokad.Image.Tooling.CooperativeMemoryConsumer.Verify();
Lokad.Image.Tooling.TextShapingConsumer.Verify();
Lokad.Image.Tooling.PngCompressionConsumer.Verify();
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
foreach (var (cachedDepth, cachedHash) in new[]
{
    (8, "656bb2248362c88dff8fe510dcc21055155976a3dd2b33aa20c48d2b036f59a8"),
    (16, "2676207be4d1a7c9109061fc3c8eb5a0d6b8d10a97bbbc611fbebda3330ffd9f")
})
{
    using var cachedRoot = new Lokad.Image.Tooling.TestMemoryBudget(128_000_000);
    var cachedLimits = new ImageLimits { MemoryBudget = cachedRoot };
    using var cachedSource = ImageBuffer.Create(513, 259, cachedDepth, cachedLimits);
    var cachedRecipe = Lokad.Image.Tooling.OriginalPixels.Generate(513, 259, cachedDepth);
    for (var y = 0; y < 259; y++)
    {
        if (cachedDepth == 8) cachedRecipe.AsSpan(y * 513 * 4, 513 * 4).CopyTo(cachedSource.GetRow8(y));
        else for (var i = 0; i < 513 * 4; i++)
            cachedSource.GetRow16(y)[i] = System.Buffers.Binary.BinaryPrimitives.ReadUInt16LittleEndian(
                cachedRecipe.AsSpan((y * 513 * 4 + i) * 2));
    }
    if (cachedDepth == 8) cachedSource.GetRow8(0)[3] = cachedSource.GetRow8(0)[513 * 4 - 1] = 0;
    else cachedSource.GetRow16(0)[3] = cachedSource.GetRow16(0)[513 * 4 - 1] = 0;
    var cachedBefore = Lokad.Image.Tooling.OriginalPixels.Bytes(cachedSource);
    var cachedEnlargementHeight = cachedDepth == 16 ? 519 : 259;
    var cachedEnlargementHash = cachedDepth == 16
        ? "c17909e129d3a6775ebd3c863c5fce3e8c3fedfd0f9cb5aefd6eda4a68e993c0"
        : "ef9e67095fcbc4a0d9a4fb2591577e9141a188e6b6323a44073cfd2ea41b7724";
    using (var cachedEnlarged = ImageTransforms.Resize(cachedSource, 1027, cachedEnlargementHeight, ResizeFilter.Bicubic, cachedLimits))
    {
        if (Convert.ToHexStringLower(System.Security.Cryptography.SHA256.HashData(
            Lokad.Image.Tooling.OriginalPixels.Bytes(cachedEnlarged))) != cachedEnlargementHash
            || cachedRoot.Bytes != (513L * 259 + 1027L * cachedEnlargementHeight) * 4 * (cachedDepth / 8))
            throw new Exception("Package cached enlargement pixels or retained charge differ.");
    }
    if (cachedRoot.Bytes != 513L * 259 * 4 * (cachedDepth / 8)) throw new Exception("Package cached enlargement charge was not released.");
    using var cachedFirst = ImageTransforms.Resize(cachedSource, 256, 129, ResizeFilter.Bicubic, cachedLimits);
    using var cachedSecond = ImageTransforms.Resize(cachedSource, 256, 129, ResizeFilter.Bicubic, cachedLimits);
    if (Convert.ToHexStringLower(System.Security.Cryptography.SHA256.HashData(
        Lokad.Image.Tooling.OriginalPixels.Bytes(cachedSecond))) != cachedHash
        || !cachedBefore.AsSpan().SequenceEqual(Lokad.Image.Tooling.OriginalPixels.Bytes(cachedSource)))
        throw new Exception("Package cached bicubic pixels or borrowed input differ.");
    var cachedLive = cachedRoot.Bytes;
    cachedRoot.RefuseAtGrowth = cachedRoot.GrowthAttempts + 1;
    try
    {
        using var refused = ImageTransforms.Resize(cachedSource, 256, 129, ResizeFilter.Bicubic, cachedLimits);
        throw new Exception("Package cached bicubic admission refusal was ignored.");
    }
    catch (ImageLimitException) { if (cachedRoot.Bytes != cachedLive) throw new Exception("Package cached bicubic refusal leaked storage."); }
    finally { cachedRoot.RefuseAtGrowth = 0; }
    using (var recovered = ImageTransforms.Resize(cachedSource, 256, 129, ResizeFilter.Bicubic, cachedLimits))
        if (!Lokad.Image.Tooling.OriginalPixels.Bytes(recovered).AsSpan().SequenceEqual(Lokad.Image.Tooling.OriginalPixels.Bytes(cachedSecond)))
            throw new Exception("Package cached bicubic did not recover after admission refusal.");
    if (cachedDepth == 8) cachedFirst.GetRow8(0)[0] ^= 255;
    else cachedFirst.GetRow16(0)[0] ^= 65535;
    cachedSource.Dispose(); cachedFirst.Dispose();
    if (cachedRoot.Bytes != 256L * 129 * 4 * (cachedDepth / 8)
        || Convert.ToHexStringLower(System.Security.Cryptography.SHA256.HashData(
            Lokad.Image.Tooling.OriginalPixels.Bytes(cachedSecond))) != cachedHash)
        throw new Exception("Package cached bicubic survivor ownership differs.");
    cachedSecond.Dispose();
    if (cachedRoot.Bytes != 0 || cachedRoot.IsDisposed) throw new Exception("Package cached bicubic root cleanup differs.");
}
Console.WriteLine("Packaged cached bicubic passed (8/16-bit pixels, admission and ownership).");
using (var crossRoot = new Lokad.Image.Tooling.TestMemoryBudget((513L * 259 + 2L * 256 * 519) * 8 + 4096))
{
    const long crossOutputBytes = 256L * 519 * 8;
    var crossLimits = new ImageLimits { MemoryBudget = crossRoot,
        MaxWorkingBytes = (513L * 259 + 256L * 519) * 8 + 4096 };
    using var crossSource = ImageBuffer.Create(513, 259, 16, crossLimits);
    var crossRecipe = Lokad.Image.Tooling.OriginalPixels.Generate(513, 259, 16);
    for (var y = 0; y < 259; y++)
    for (var i = 0; i < 513 * 4; i++)
        crossSource.GetRow16(y)[i] = System.Buffers.Binary.BinaryPrimitives.ReadUInt16LittleEndian(
            crossRecipe.AsSpan((y * 513 * 4 + i) * 2));
    crossSource.GetRow16(0)[3] = crossSource.GetRow16(0)[513 * 4 - 1] = 0;
    var crossInput = Lokad.Image.Tooling.OriginalPixels.Bytes(crossSource);
    const string crossHash = "ee02e57ff550047bf5296fb5af51c9396005653a8d37c3e0032abcdbb6db0db1";
    using var crossFirst = ImageTransforms.Resize(crossSource, 256, 519, ResizeFilter.Bicubic, crossLimits);
    using var crossSecond = ImageTransforms.Resize(crossSource, 256, 519, ResizeFilter.Bicubic, crossLimits);
    var crossLive = crossRoot.Bytes;
    if (crossLive != 513L * 259 * 8 + 2 * crossOutputBytes
        || Convert.ToHexStringLower(System.Security.Cryptography.SHA256.HashData(
            Lokad.Image.Tooling.OriginalPixels.Bytes(crossSecond))) != crossHash
        || !crossInput.AsSpan().SequenceEqual(Lokad.Image.Tooling.OriginalPixels.Bytes(crossSource)))
        throw new Exception("Packaged tight word bicubic pixels or retained admission differ.");
    crossRoot.RefuseAtGrowth = crossRoot.GrowthAttempts + 1;
    try
    {
        using var refused = ImageTransforms.Resize(crossSource, 256, 519, ResizeFilter.Bicubic, crossLimits);
        throw new Exception("Packaged tight word bicubic ignored shared admission refusal.");
    }
    catch (ImageLimitException e) when (e.Scope == ImageLimitScope.SharedBudget && e.Limit == ImageLimitKind.WorkingBytes)
    {
        if (crossRoot.Bytes != crossLive || crossRoot.Refusals != 1)
            throw new Exception("Packaged tight word bicubic refusal cleanup differs.");
    }
    finally { crossRoot.RefuseAtGrowth = 0; }
    crossFirst.GetRow16(0)[0] ^= 65535; crossFirst.Dispose();
    using (var recovered = ImageTransforms.Resize(crossSource, 256, 519, ResizeFilter.Bicubic, crossLimits))
        if (Convert.ToHexStringLower(System.Security.Cryptography.SHA256.HashData(
            Lokad.Image.Tooling.OriginalPixels.Bytes(recovered))) != crossHash)
            throw new Exception("Packaged tight word bicubic failed after released capacity.");
    crossSource.Dispose();
    if (crossRoot.Bytes != crossOutputBytes
        || Convert.ToHexStringLower(System.Security.Cryptography.SHA256.HashData(
            Lokad.Image.Tooling.OriginalPixels.Bytes(crossSecond))) != crossHash)
        throw new Exception("Packaged tight word bicubic survivor depends on disposed owners.");
    crossSecond.Dispose();
    if (crossRoot.Bytes != 0 || crossRoot.IsDisposed)
        throw new Exception("Packaged tight word bicubic root cleanup differs.");
}
Console.WriteLine("Packaged tight word bicubic passed (exact pixels, shared admission and surviving owners).");
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
foreach (var (name, width, height, expectedHash) in new[]
{
    ("vp8-ffmpeg-31x23-effort6-quality75-opaque", 31, 23, "0183ca82890160db504bf6e1265a9dd6471cdf0bf98617e7dd5b7c3365f83432"),
    ("vp8-authored-filter-normal-level40", 33, 33, "af9c71cdea27cf4a1d8e398d2a61bb70678c4a1a8c028cef86d80b4744f0aea1")
})
{
    var packet = File.ReadAllBytes(Path.Combine(Path.GetDirectoryName(args[5])!, name + ".webp"));
    using var root = new Lokad.Image.Tooling.TestMemoryBudget(2 * 1024 * 1024);
    var options = new DecodeOptions { Limits = ImageLimits.Default with { MemoryBudget = root } };
    using (var retained = ImageCodec.Decode(packet, options))
    {
        using (var input = new MemoryStream(packet, writable: false))
        {
            using var asynchronous = await ImageCodec.DecodeAsync(input, options);
            if (Convert.ToHexStringLower(System.Security.Cryptography.SHA256.HashData(Lokad.Image.Tooling.OriginalPixels.Bytes(asynchronous))) != expectedHash
                || !input.CanRead || root.Bytes != 2L * width * height * 4)
                throw new Exception("Packaged VP8 exact async samples or retained owners differ.");
        }
        Array.Clear(packet);
        if (Convert.ToHexStringLower(System.Security.Cryptography.SHA256.HashData(Lokad.Image.Tooling.OriginalPixels.Bytes(retained))) != expectedHash
            || root.Bytes != (long)width * height * 4 || root.IsDisposed)
            throw new Exception("Packaged VP8 exact samples or independent input lifetime differ.");
    }
    if (root.Bytes != 0 || root.IsDisposed) throw new Exception("Packaged VP8 retained admission was not released.");
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
    var horizontal = 4 * (int)Math.Round(Math.Sqrt(2) * Math.Cos((2 * (sx & 7) + 1) * Math.PI / 16) * 16384, MidpointRounding.AwayFromZero);
    var vertical = sparseTerms == 2 ? 32 * (int)Math.Round(Math.Sqrt(2) * Math.Cos((2 * (sy & 7) + 1) * Math.PI / 16) * 16384, MidpointRounding.AwayFromZero) : 0;
    int SparseSample(int sample)
    {
        var row = (sample * 8 * 16383 + horizontal + 1024) >> 11;
        return Math.Clamp(((row + 32) * 16383 + vertical) >> 20, 0, 255);
    }
    for (var sc = 0; sc < 3; sc++)
        if (sparseSurvivor.GetRow8(sy)[sx * 4 + sc] != ((SparseSample(sparseValues[sc]) * SparseSample(sparseValues[3]) * 257) >> 16))
            throw new Exception("Packaged sparse JPEG analytical sample differs.");
    if (sparseSurvivor.GetRow8(sy)[sx * 4 + 3] != 255) throw new Exception("Packaged sparse JPEG alpha differs.");
}
sparseSurvivor.Dispose();
if (sparseRoot.Bytes != 0) throw new Exception("Packaged sparse JPEG retained charge leaked.");
}
Console.WriteLine("Packaged exact completed-row JPEG pixels and retained ownership passed.");
{
    var indexedFrame = new Lokad.Image.Tooling.AuthoredPng.Frame("package-indexed", 3, 4, 19, 17, true);
    var indexedPacket = Lokad.Image.Tooling.AuthoredPng.Build(indexedFrame);
    var originalPacket = indexedPacket.ToArray();
    var expectedInfo = ImageCodec.Identify(indexedPacket);
    using var indexedRoot = new Lokad.Image.Tooling.TestMemoryBudget(8192);
    var indexedLimits = new ImageLimits { MemoryBudget = indexedRoot };
    if (ImageCodec.ValidatePixels(indexedPacket, indexedLimits) != expectedInfo || indexedRoot.Bytes != 0
        || indexedRoot.IsDisposed || indexedRoot.Peak > 8192)
        throw new Exception("Packaged indexed PNG validation storage or descriptor differs.");
    var invalidRows = Lokad.Image.Tooling.AuthoredPng.FilteredRows(indexedFrame);
    invalidRows[0] = 5;
    var invalidPacket = Lokad.Image.Tooling.AuthoredPng.Wrap(indexedFrame,
        Lokad.Image.Tooling.AuthoredPng.StoredZlib(invalidRows));
    var rejected = false;
    try { ImageCodec.ValidatePixels(invalidPacket, indexedLimits); }
    catch (ImageDataException) { rejected = true; }
    if (!rejected || indexedRoot.Bytes != 0 || indexedRoot.IsDisposed
        || ImageCodec.ValidatePixels(indexedPacket, indexedLimits) != expectedInfo || indexedRoot.Bytes != 0
        || !indexedPacket.AsSpan().SequenceEqual(originalPacket))
        throw new Exception("Packaged indexed PNG validation cleanup or input ownership differs.");
    Console.WriteLine($"Packaged indexed PNG validation fits bounded shared storage: {indexedRoot.Peak} bytes.");
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
    var limits = new ImageLimits { MaxWorkingBytes = 8448 };
    using var headers = new MemoryStream(packet, writable: false);
    if (ImageCodec.InspectHeaders(headers, limits) != expected || headers.Position != headers.Length || !headers.CanRead)
        throw new Exception("Package fixed-workspace header inspection differs.");
    headers.Position = 0;
    using var root = new Lokad.Image.Tooling.TestMemoryBudget(8448);
    if (ImageCodec.InspectHeadersAsync(headers, limits with { MemoryBudget = root }).GetAwaiter().GetResult() != expected || root.Bytes != 0)
        throw new Exception("Package async/shared header inspection differs.");
    headers.Position = 0;
    var probeOptions = new ImageProbeOptions { Limits = limits with { MemoryBudget = root } };
    var probe = ImageCodec.Probe(headers, probeOptions);
    if ((probe.Width, probe.Height) != (expected.Width, expected.Height)
        || probe.Orientation is { } knownOrientation && knownOrientation != expected.Orientation
        || probe.BytesRead > 1024 || probe.BytesTraversed != headers.Position || root.Bytes != 0 || !headers.CanRead)
        throw new Exception("Package selective probe acquisition/ownership differs.");
    headers.Position = 0;
    var allFacts = ImageCodec.ProbeAsync(headers, probeOptions with { Facts = ImageProbeFacts.All }).GetAwaiter().GetResult();
    if (allFacts.EstablishedFacts != ImageProbeFacts.All || allFacts.SourceColor != expected.SourceColor
        || allFacts.SourceAlpha != expected.SourceAlpha || allFacts.Orientation != expected.Orientation || root.Bytes != 0)
        throw new Exception("Package async selective probe metadata/admission differs.");
    using var validationRoot = new Lokad.Image.Tooling.TestMemoryBudget(4*1024*1024);
    var validationLimits = new ImageLimits { MaxWorkingBytes = 4*1024*1024, MemoryBudget = validationRoot };
    if (ImageCodec.ValidatePixels(packet, validationLimits) != expected || validationRoot.Bytes != 0)
        throw new Exception("Package span pixel validation metadata/admission differs.");
    headers.Position = 0;
    if (ImageCodec.ValidatePixels(headers, validationLimits) != expected || validationRoot.Bytes != 0 || headers.Position != headers.Length)
        throw new Exception("Package stream pixel validation differs.");
    headers.Position = 0;
    if ((await ImageCodec.ValidatePixelsAsync(headers, validationLimits)) != expected || validationRoot.Bytes != 0 || !headers.CanRead)
        throw new Exception("Package async pixel validation ownership/admission differs.");
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
    var limits = new ImageLimits { MaxWorkingBytes = 2 * 1024 * 1024 };
    using var root = new Lokad.Image.Tooling.TestMemoryBudget(limits.MaxWorkingBytes);
    limits = limits with { MemoryBudget = root };
    using var webp = new MemoryStream();
    await ImageCodec.EncodeWebpAsync(image, webp, new() { Limits = limits, Lossless = true });
    using var webpExpected = new MemoryStream(); ImageCodec.EncodeWebp(image, webpExpected, new() { Lossless = true });
    var gray = new byte[image.Width * image.Height];
    for (var y = 0; y < image.Height; y++) for (var x = 0; x < image.Width; x++) gray[y * image.Width + x] = image.GetRow8(y)[x * 4];
    using var samples = new MemoryStream();
    await ImageCodec.EncodeJpegAsync(gray.AsMemory(), image.Width, image.Height, JpegSampleFormat.Grayscale, samples,
        options: new() { Limits = limits, Quality = 69 });
    using var samplesExpected = new MemoryStream();
    ImageCodec.EncodeJpeg(gray, image.Width, image.Height, JpegSampleFormat.Grayscale, samplesExpected, options: new() { Quality = 69 });
    if (!webp.ToArray().AsSpan().SequenceEqual(webpExpected.ToArray()) || !samples.ToArray().AsSpan().SequenceEqual(samplesExpected.ToArray())
        || root.Bytes != 0 || !webp.CanWrite || !samples.CanWrite)
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
    var limits = new ImageLimits { MaxWorkingBytes = 2 * 1024 * 1024 };
    using var root = new Lokad.Image.Tooling.TestMemoryBudget(limits.MaxWorkingBytes);
    limits = limits with { MemoryBudget = root };
    using var budgetInfoInput = new MemoryStream(packet, writable: false);
    if (await ImageCodec.IdentifyAsync(budgetInfoInput, limits) != ImageCodec.Identify(packet))
        throw new Exception("Package async budget Identify differs.");
    using var budgetInput = new MemoryStream(packet, writable: false);
    using (var result = await ImageCodec.DecodeAsync(budgetInput, new() { Limits = limits }))
    {
        if (root.Bytes != result.Width * result.Height * 4L)
            throw new Exception("Package async retained admission differs.");
        for (var y = 0; y < result.Height; y++)
            if (!result.GetRow8(y).SequenceEqual(expected.GetRow8(y))) throw new Exception("Package async budget pixels differ.");
    }
    if (root.Bytes != 0 || !pixelsInput.CanRead || !budgetInput.CanRead || !budgetInfoInput.CanRead)
        throw new Exception("Package async lifetime/refund differs.");
}
var sharedLimits = new ImageLimits { MaxWorkingBytes = 2 * 1024 * 1024 };
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
using var shared = new Lokad.Image.Tooling.TestMemoryBudget(sharedLimits.MaxWorkingBytes);
sharedLimits = sharedLimits with { MemoryBudget = shared };
if (!shared.TryReserve(256, out var callerStorage)) throw new Exception("Caller reservation refused.");
using (callerStorage)
{
    using (var retained = ImageCodec.Decode(jpeg.ToArray(), new DecodeOptions { Limits = sharedLimits }))
    {
        if (shared.Bytes != 256 + 8 * 8 * 4) throw new Exception("Retained image admission differs.");
        for (var y = 0; y < 8; y++)
            if (!retained.GetRow8(y).SequenceEqual(opaque.GetRow8(y))) throw new Exception("Admitted pixels differ.");
    }
    if (shared.Bytes != 256) throw new Exception("Image disposal did not release its own pixels.");
}
if (shared.Bytes != 0 || shared.IsDisposed) throw new Exception("Caller root ownership differs.");
var callerGray = new byte[64];
ImageCodec.DecodeJpeg(jpeg.ToArray(), callerGray, JpegSampleFormat.Grayscale,
    options: new JpegDecodeOptions { Limits = sharedLimits });
if (callerGray.AsSpan().IndexOfAnyExcept((byte)123) >= 0 || shared.Bytes != 0)
    throw new Exception("Caller-owned grayscale admission differs.");
using var callerEncoded = new MemoryStream();
ImageCodec.EncodeJpeg(callerSamples, 8, 8, JpegSampleFormat.Rgb, callerEncoded, 27,
    new JpegEncodeOptions { Quality = 100, Sampling = JpegSampling.Grayscale, RestartInterval = 1 });
if (!callerEncoded.ToArray().AsSpan().SequenceEqual(jpeg.ToArray()) || !callerEncoded.CanWrite)
    throw new Exception("Caller RGB encoding differs.");
using var callerEncodedGray = new MemoryStream();
ImageCodec.EncodeJpeg(callerGray, 8, 8, JpegSampleFormat.Grayscale, callerEncodedGray,
    options: new JpegEncodeOptions { Quality = 100, RestartInterval = 1, Limits = sharedLimits });
if (!callerEncodedGray.ToArray().AsSpan().SequenceEqual(jpeg.ToArray()) || shared.Bytes != 0)
    throw new Exception("Caller grayscale encoding/admission differs.");
using var emptyRoot = new Lokad.Image.Tooling.TestMemoryBudget(0);
try
{
    using var refused = ImageBuffer.Create(8, 8, 8, new ImageLimits { MemoryBudget = emptyRoot });
    throw new Exception("Package accepted refused image storage.");
}
catch (ImageLimitException failure)
{
    if (failure.Limit != ImageLimitKind.WorkingBytes || failure.Scope != ImageLimitScope.SharedBudget
        || emptyRoot.Bytes != 0 || emptyRoot.Refusals != 1 || emptyRoot.IsDisposed)
        throw new Exception("Shared refusal or cleanup differs.");
}
if (typeof(ImageLimits).GetProperty("MaxWork") is not null
    || typeof(ImageBuffer).Assembly.GetType("Lokad.Image.ImageBudget") is not null
    || typeof(ImageBuffer).Assembly.GetType("Lokad.Image.BudgetedImage") is not null
    || Enum.GetNames<ImageLimitKind>().Contains("Work"))
    throw new Exception("Removed fuel/ownership APIs remain in the package.");
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
var unusableExif = metadataBytes.ToArray(); unusableExif[12 + 18 + 63*12] = 9;
try { ImageCodec.Identify(unusableExif); throw new Exception("Package default EXIF policy is not strict."); }
catch (ImageDataException) { }
using (var optionalRoot = new Lokad.Image.Tooling.TestMemoryBudget(1024*1024))
{
    var optionalLimits = new ImageLimits { MemoryBudget = optionalRoot };
    var optionalPolicy = ImageMetadataPolicy.IgnoreInvalid;
    if (ImageCodec.Identify(unusableExif, optionalLimits, default, optionalPolicy).Orientation != ImageOrientation.Normal)
        throw new Exception("Package permissive span identification differs.");
    if (ImageCodec.ValidatePixels(unusableExif, optionalLimits, metadataPolicy: optionalPolicy).Orientation != ImageOrientation.Normal
        || optionalRoot.Bytes != 0)
        throw new Exception("Package permissive pixel validation differs.");
    using var optionalInput = new MemoryStream(unusableExif);
    if ((await ImageCodec.IdentifyAsync(optionalInput, optionalLimits, default, optionalPolicy)).Orientation != ImageOrientation.Normal)
        throw new Exception("Package permissive async identification differs.");
    optionalInput.Position = 0;
    if ((await ImageCodec.InspectHeadersAsync(optionalInput, optionalLimits, default, optionalPolicy)).Orientation != ImageOrientation.Normal)
        throw new Exception("Package permissive header inspection differs.");
    optionalInput.Position = 0;
    if ((await ImageCodec.ProbeAsync(optionalInput, new() { Limits = optionalLimits, MetadataPolicy = optionalPolicy, Facts = ImageProbeFacts.All })).Orientation != ImageOrientation.Normal)
        throw new Exception("Package permissive probing differs.");
    using (var optionalPixels = ImageCodec.Decode(unusableExif, new() { Limits = optionalLimits, MetadataPolicy = optionalPolicy }))
        if (!optionalPixels.GetRow8(0).SequenceEqual(opaque.GetRow8(0)) || optionalPixels.Orientation != ImageOrientation.Normal)
            throw new Exception("Package permissive pixels differ.");
    var optionalSamples = new byte[64];
    if (ImageCodec.DecodeJpeg(unusableExif, optionalSamples, JpegSampleFormat.Grayscale,
        options: new() { Limits = optionalLimits, MetadataPolicy = optionalPolicy }).Orientation != ImageOrientation.Normal
        || optionalSamples.Any(value => value != 123) || optionalRoot.Bytes != 0 || !optionalInput.CanRead)
        throw new Exception("Package permissive caller samples/admission/ownership differ.");
}
using var webp = new MemoryStream();
ImageCodec.EncodeWebp(flipped, webp, new WebpEncodeOptions { Effort = 6 });
using var webpPixels = ImageCodec.Decode(webp.ToArray());
if (webpPixels.Width != 2 || webpPixels.Height != 1 || webpPixels.BitDepth != 8 || !webp.CanWrite)
    throw new Exception("Package consumer WebP contract differs.");
for (var sample = 0; sample < 8; sample++)
    if (webpPixels.GetRow8(0)[sample] != (flipped.GetRow16(0)[sample] + 128) / 257)
        throw new Exception("Package consumer WebP normalized pixels differ.");
using var webpPeakRoot = new Lokad.Image.Tooling.TestMemoryBudget(ImageLimits.Default.MaxWorkingBytes);
using var webpPeakPacket = new MemoryStream();
ImageCodec.EncodeWebp(flipped, webpPeakPacket, new() { Effort = 6, Limits = new() { MemoryBudget = webpPeakRoot } });
if (webpPeakRoot.Bytes != 0) throw new Exception("Package VP8L peak probe leaked admission.");
var webpLimits = new ImageLimits { MaxWorkingBytes = (long)flipped.Stride * flipped.Height + webpPeakRoot.Peak, MaxOutputBytes = (int)webp.Length };
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
// Derive the local boundary for this actual encoded profile through the public
// cooperative-admission adapter; authored decode-profile thresholds are separate.
using var webpDecodeRoot = new Lokad.Image.Tooling.TestMemoryBudget(ImageLimits.Default.MaxWorkingBytes);
using (var probe = ImageCodec.Decode(webp.ToArray(), new() { Limits = new() { MemoryBudget = webpDecodeRoot } })) { }
if (webpDecodeRoot.Bytes != 0) throw new Exception("Package VP8L decode probe leaked ownership.");
var webpDecodeLimits = new ImageLimits { MaxWorkingBytes = webp.Length + webpDecodeRoot.Peak };
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
// Frozen original JPEG decoder controls, read by this package-only consumer.
foreach (var (sampling, packetHash, pixelHash) in new[]
{
    ("444", "ad1d9d11e42273349e85cfebc9b73c89231b595b883cd25d2b7832f62c353573", "fbf313b3c7bc442832b67d3b08bf228da503aefc142d4ac2ed8d2bfc720b76e4"),
    ("422", "a9fabad2b1f038030698fc31490cdfc55ea9cd362f42759cb90dc239864b4667", "cec70c8458142ab8d00666d40771cb7d08fe129e022aa55eca70ffccd1ba9f5d"),
    ("420", "9e4b1365e3dd349e01dddd50cdf7ed801d9ca554bb1a16c54c561aa9159bef1c", "009c2dd00e6ae9653438d65ae999a1eff56080340b6d8852617fe7281c5024c3")
})
{
    var jpegExactData = File.ReadAllBytes(args[5].Replace("YCbCr444", "YCbCr" + sampling));
    var jpegExactNative = File.ReadAllBytes(args[6].Replace("YCbCr444", "YCbCr" + sampling));
    if (Convert.ToHexStringLower(System.Security.Cryptography.SHA256.HashData(jpegExactData)) != packetHash
        || Convert.ToHexStringLower(System.Security.Cryptography.SHA256.HashData(jpegExactNative)) != pixelHash)
        throw new Exception("Package JPEG decoder control provenance differs.");
    using var jpegExactRoot = new Lokad.Image.Tooling.TestMemoryBudget(4 * 1024 * 1024);
    var jpegExactOptions = new DecodeOptions { Limits = ImageLimits.Default with { MemoryBudget = jpegExactRoot } };
    using (var jpegExactImage = ImageCodec.Decode(jpegExactData, jpegExactOptions))
    {
        if (!Lokad.Image.Tooling.OriginalPixels.Bytes(jpegExactImage).AsSpan().SequenceEqual(jpegExactNative))
            throw new Exception("Package array JPEG pixels differ from the frozen reference.");
        using (var jpegExactInput = new MemoryStream(jpegExactData, writable: false))
        {
            using var jpegExactSync = ImageCodec.Decode(jpegExactInput, jpegExactOptions);
            if (!jpegExactInput.CanRead || !Lokad.Image.Tooling.OriginalPixels.Bytes(jpegExactSync).AsSpan().SequenceEqual(jpegExactNative))
                throw new Exception("Package synchronous JPEG pixels or stream ownership differ.");
        }
        using (var jpegExactInput = new MemoryStream(jpegExactData, writable: false))
        {
            using var jpegExactAsync = await ImageCodec.DecodeAsync(jpegExactInput, jpegExactOptions);
            if (!jpegExactInput.CanRead || !Lokad.Image.Tooling.OriginalPixels.Bytes(jpegExactAsync).AsSpan().SequenceEqual(jpegExactNative))
                throw new Exception("Package asynchronous JPEG pixels or stream ownership differ.");
        }
        if (jpegExactRoot.Bytes != jpegExactNative.Length || jpegExactRoot.IsDisposed)
            throw new Exception("Package retained JPEG ownership differs.");
    }
    var jpegExactRgb = Enumerable.Repeat((byte)165, 98 * 23 + 7).ToArray();
    ImageCodec.DecodeJpeg(jpegExactData, jpegExactRgb, JpegSampleFormat.Rgb, 98,
        new JpegDecodeOptions { Limits = jpegExactOptions.Limits });
    for (var y = 0; y < 23; y++)
    {
        for (var x = 0; x < 31; x++)
            if (!jpegExactRgb.AsSpan(y * 98 + x * 3, 3).SequenceEqual(jpegExactNative.AsSpan((y * 31 + x) * 4, 3)))
                throw new Exception("Package caller RGB pixels differ.");
        if (jpegExactRgb.AsSpan(y * 98 + 93, 5).ContainsAnyExcept((byte)165))
            throw new Exception("Package caller RGB padding changed.");
    }
    if (jpegExactRgb.AsSpan(98 * 23).ContainsAnyExcept((byte)165)) throw new Exception("Package caller RGB tail changed.");
    var jpegExactGray = new byte[713];
    ImageCodec.DecodeJpeg(jpegExactData, jpegExactGray, JpegSampleFormat.Grayscale,
        options: new JpegDecodeOptions { Limits = jpegExactOptions.Limits });
    if (Convert.ToHexStringLower(System.Security.Cryptography.SHA256.HashData(jpegExactGray)) != "7c208d4a6353d69de6f9da697fe23efe603fef3fb2499568c47bd08f52752a98")
        throw new Exception("Package caller grayscale pixels differ.");
    using var jpegExactOrientedPacket = new MemoryStream();
    jpegExactOrientedPacket.Write(jpegExactData.AsSpan(0, 2));
    jpegExactOrientedPacket.Write(new byte[] {255,225,(byte)(app1Length >> 8),(byte)app1Length});
    jpegExactOrientedPacket.Write("Exif\0\0"u8); jpegExactOrientedPacket.Write(tiff); jpegExactOrientedPacket.Write(jpegExactData.AsSpan(2));
    var jpegExactOriented = new byte[713 * 3];
    var jpegExactInfo = ImageCodec.DecodeJpeg(jpegExactOrientedPacket.ToArray(), jpegExactOriented, JpegSampleFormat.Rgb,
        options: new JpegDecodeOptions { ApplyOrientation = true, Limits = jpegExactOptions.Limits });
    if ((jpegExactInfo.Width, jpegExactInfo.Height, jpegExactInfo.Orientation) != (23,31,ImageOrientation.Normal))
        throw new Exception("Package caller RGB orientation differs.");
    for (var y = 0; y < 23; y++)
    for (var x = 0; x < 31; x++)
        if (!jpegExactOriented.AsSpan((x * 23 + 22 - y) * 3, 3).SequenceEqual(jpegExactNative.AsSpan((y * 31 + x) * 4, 3)))
            throw new Exception("Package oriented caller RGB pixels differ.");
    if (jpegExactRoot.Bytes != 0 || jpegExactRoot.IsDisposed) throw new Exception("Package JPEG admission cleanup differs.");
}
// Existing original varying CMYK/YCCK controls exercise the actual package.
foreach (var isYcck in new[] { false, true })
{
    var fourData = Lokad.Image.Tooling.AuthoredJpegCompatibility.Create(65, 31, new byte[] {17,191,250,63},
        isYcck ? 2 : 1, isYcck ? 2 : 1, isYcck, isYcck ? 2 : 0, restart: isYcck ? 1 : 0, vary: true);
    var fourHash = isYcck ? "adabc4449f31ac8f77fb941686c4cbbc1f178ba919d373220bdf1991a07b71cc" : "8deae32e317740b58b94ddcbbb0270c6437616391d9d559f25aef7984413fd44";
    var grayHash = isYcck ? "04282e0fb862d9360eb9ed1138b3bd343b675818ee2bba2f292254b5e90bca15" : "34e9c10cdde85e632e61821274846ed642d1f5c575295f42eb212e5aa4c5aaad";
    using var fourRoot = new Lokad.Image.Tooling.TestMemoryBudget(4 * 1024 * 1024);
    var fourLimits = ImageLimits.Default with { MemoryBudget = fourRoot };
    using (var fourImage = ImageCodec.Decode(fourData, new DecodeOptions { Limits = fourLimits }))
    {
        if (Convert.ToHexStringLower(System.Security.Cryptography.SHA256.HashData(Lokad.Image.Tooling.OriginalPixels.Bytes(fourImage))) != fourHash || fourRoot.Bytes != 65*31*4)
            throw new Exception("Package four-component native samples or retained charges differ.");
        using (var fourInput = new MemoryStream(fourData, writable:false))
        {
            using var fourSync = ImageCodec.Decode(fourInput, new DecodeOptions { Limits = fourLimits });
            if (!fourInput.CanRead || Convert.ToHexStringLower(System.Security.Cryptography.SHA256.HashData(Lokad.Image.Tooling.OriginalPixels.Bytes(fourSync))) != fourHash)
                throw new Exception("Package four-component synchronous delivery differs.");
        }
        using (var fourInput = new MemoryStream(fourData, writable:false))
        {
            using var fourAsync = await ImageCodec.DecodeAsync(fourInput, new DecodeOptions { Limits = fourLimits });
            if (!fourInput.CanRead || Convert.ToHexStringLower(System.Security.Cryptography.SHA256.HashData(Lokad.Image.Tooling.OriginalPixels.Bytes(fourAsync))) != fourHash)
                throw new Exception("Package four-component asynchronous delivery differs.");
        }
    }
    foreach (var oriented in new[] { false, true })
    {
        using var fourPacket = new MemoryStream();fourPacket.Write(fourData.AsSpan(0,2));
        fourPacket.Write(new byte[] {255,225,(byte)(app1Length>>8),(byte)app1Length});
        fourPacket.Write("Exif\0\0"u8);fourPacket.Write(tiff);fourPacket.Write(fourData.AsSpan(2));
        var width = oriented ? 31 : 65; var height = oriented ? 65 : 31;
        var rgbStride = width*3+5;var grayStride=width+5;
        var rgbSamples=Enumerable.Repeat((byte)165,rgbStride*height+7).ToArray();
        var graySamples=Enumerable.Repeat((byte)165,grayStride*height+7).ToArray();
        var fourOptions=new JpegDecodeOptions { ApplyOrientation=oriented, Limits=fourLimits };
        ImageCodec.DecodeJpeg(fourPacket.ToArray(),rgbSamples,JpegSampleFormat.Rgb,rgbStride,fourOptions);
        ImageCodec.DecodeJpeg(fourPacket.ToArray(),graySamples,JpegSampleFormat.Grayscale,grayStride,fourOptions);
        var rgbaSamples=new byte[65*31*4];var packedGray=new byte[65*31];
        for(var y=0;y<31;y++)for(var x=0;x<65;x++)
        {
            var dx=oriented?30-y:x;var dy=oriented?x:y;
            rgbSamples.AsSpan(dy*rgbStride+dx*3,3).CopyTo(rgbaSamples.AsSpan((y*65+x)*4,3));
            rgbaSamples[(y*65+x)*4+3]=255;packedGray[y*65+x]=graySamples[dy*grayStride+dx];
        }
        if(Convert.ToHexStringLower(System.Security.Cryptography.SHA256.HashData(rgbaSamples))!=fourHash
            ||Convert.ToHexStringLower(System.Security.Cryptography.SHA256.HashData(packedGray))!=grayHash)
            throw new Exception("Package four-component caller RGB/gray/orientation differs.");
        for(var y=0;y<height;y++)if(rgbSamples.AsSpan(y*rgbStride+width*3,5).ContainsAnyExcept((byte)165)
            ||graySamples.AsSpan(y*grayStride+width,5).ContainsAnyExcept((byte)165))throw new Exception("Package four-component row padding changed.");
        if(rgbSamples.AsSpan(rgbStride*height).ContainsAnyExcept((byte)165)||graySamples.AsSpan(grayStride*height).ContainsAnyExcept((byte)165))
            throw new Exception("Package four-component caller tail changed.");
    }
    if(fourRoot.Bytes!=0||fourRoot.IsDisposed)throw new Exception("Package four-component admission cleanup differs.");
}
// Existing original quarter-width controls exercise direct RGB/Y and ownership.
foreach (var vertical in new[] { 1, 2 })
{
    var quarterData = Lokad.Image.Tooling.AuthoredJpegCompatibility.Create(67, 19, new byte[] {60,105,145}, 4, vertical, restart:1, vary:true);
    var quarterHash = vertical == 1 ? "da994d5e09fdd9775c14b64341bd5684ee2a5f7675f46750f57f8d027b9b3c3f" : "5cce3dca30a43f41ddfe87cc42ec1b42ee8eaec3132dc2e0bc702e4ee7b49fbd";
    var grayHash = "373d0d5705ccba1e7ce7640e541c3fd1f37457dcaf5bbb28ec863e3485d1d167";
    using var quarterRoot = new Lokad.Image.Tooling.TestMemoryBudget(4 * 1024 * 1024);
    var quarterLimits = ImageLimits.Default with { MemoryBudget = quarterRoot };
    using (var quarterImage = ImageCodec.Decode(quarterData, new DecodeOptions { Limits = quarterLimits }))
    {
        if (Convert.ToHexStringLower(System.Security.Cryptography.SHA256.HashData(Lokad.Image.Tooling.OriginalPixels.Bytes(quarterImage))) != quarterHash || quarterRoot.Bytes != 67*19*4)
            throw new Exception("Package quarter-width JPEG native samples or retained charges differ.");
        using (var quarterInput = new MemoryStream(quarterData, writable:false))
        {
            using var quarterSync = ImageCodec.Decode(quarterInput, new DecodeOptions { Limits = quarterLimits });
            if (!quarterInput.CanRead || Convert.ToHexStringLower(System.Security.Cryptography.SHA256.HashData(Lokad.Image.Tooling.OriginalPixels.Bytes(quarterSync))) != quarterHash)
                throw new Exception("Package quarter-width JPEG synchronous delivery differs.");
        }
        using (var quarterInput = new MemoryStream(quarterData, writable:false))
        {
            using var quarterAsync = await ImageCodec.DecodeAsync(quarterInput, new DecodeOptions { Limits = quarterLimits });
            if (!quarterInput.CanRead || Convert.ToHexStringLower(System.Security.Cryptography.SHA256.HashData(Lokad.Image.Tooling.OriginalPixels.Bytes(quarterAsync))) != quarterHash)
                throw new Exception("Package quarter-width JPEG asynchronous delivery differs.");
        }
    }
    foreach (var oriented in new[] { false, true })
    {
        using var quarterPacket = new MemoryStream();quarterPacket.Write(quarterData.AsSpan(0,2));
        quarterPacket.Write(new byte[] {255,225,(byte)(app1Length>>8),(byte)app1Length});
        quarterPacket.Write("Exif\0\0"u8);quarterPacket.Write(tiff);quarterPacket.Write(quarterData.AsSpan(2));
        var width = oriented ? 19 : 67; var height = oriented ? 67 : 19;
        var rgbStride = width*3+5;var grayStride=width+5;
        var rgbSamples=Enumerable.Repeat((byte)165,rgbStride*height+7).ToArray();
        var graySamples=Enumerable.Repeat((byte)165,grayStride*height+7).ToArray();
        var quarterOptions=new JpegDecodeOptions { ApplyOrientation=oriented, Limits=quarterLimits };
        ImageCodec.DecodeJpeg(quarterPacket.ToArray(),rgbSamples,JpegSampleFormat.Rgb,rgbStride,quarterOptions);
        ImageCodec.DecodeJpeg(quarterPacket.ToArray(),graySamples,JpegSampleFormat.Grayscale,grayStride,quarterOptions);
        var rgbaSamples=new byte[67*19*4];var packedGray=new byte[67*19];
        for(var y=0;y<19;y++)for(var x=0;x<67;x++)
        {
            var dx=oriented?18-y:x;var dy=oriented?x:y;
            rgbSamples.AsSpan(dy*rgbStride+dx*3,3).CopyTo(rgbaSamples.AsSpan((y*67+x)*4,3));
            rgbaSamples[(y*67+x)*4+3]=255;packedGray[y*67+x]=graySamples[dy*grayStride+dx];
        }
        if(Convert.ToHexStringLower(System.Security.Cryptography.SHA256.HashData(rgbaSamples))!=quarterHash
            ||Convert.ToHexStringLower(System.Security.Cryptography.SHA256.HashData(packedGray))!=grayHash)
            throw new Exception("Package quarter-width JPEG caller RGB/gray/orientation differs.");
        for(var y=0;y<height;y++)if(rgbSamples.AsSpan(y*rgbStride+width*3,5).ContainsAnyExcept((byte)165)
            ||graySamples.AsSpan(y*grayStride+width,5).ContainsAnyExcept((byte)165))throw new Exception("Package quarter-width JPEG row padding changed.");
        if(rgbSamples.AsSpan(rgbStride*height).ContainsAnyExcept((byte)165)||graySamples.AsSpan(grayStride*height).ContainsAnyExcept((byte)165))
            throw new Exception("Package quarter-width JPEG caller tail changed.");
    }
    if(quarterRoot.Bytes!=0||quarterRoot.IsDisposed)throw new Exception("Package quarter-width JPEG admission cleanup differs.");
}
using (var filterEdge = ImageCodec.Decode(Lokad.Image.Tooling.AuthoredJpegCompatibility.Create(513,259,
    new byte[] {128,255,128},4,2,restart:1,blockSample:(c,x,y)=>c==1?(x<16?255:139):128)))
{
    if(Convert.ToHexStringLower(System.Security.Cryptography.SHA256.HashData(Lokad.Image.Tooling.OriginalPixels.Bytes(filterEdge)))
        !="8770ce38bb893d8a545fe21e0f68885be77c8ab6b6e356ec1d4ed9c6ac0414d1")
        throw new Exception("Packaged quarter-width near-integral filter precision differs.");
}
foreach (var directSampling in new[] { 422, 420 })
{
    var direct = Lokad.Image.Tooling.OriginalDirectRgbControls.Load(directSampling, Path.GetDirectoryName(args[5]));
    using var directRoot = new Lokad.Image.Tooling.TestMemoryBudget(4*1024*1024);
    var directLimits = ImageLimits.Default with { MemoryBudget=directRoot };
    using(var directImage=ImageCodec.Decode(direct.Encoded,new DecodeOptions { Limits=directLimits }))
    {
        if(Lokad.Image.Tooling.OriginalDirectRgbControls.Hash(Lokad.Image.Tooling.OriginalPixels.Bytes(directImage))!=direct.RgbaSha256 || directRoot.Bytes!=513*259*4)
            throw new Exception("Packaged direct-RGB pixels or retained storage differ.");
        using(var directInput=new MemoryStream(direct.Encoded,writable:false))
        {
            using var directStream=ImageCodec.Decode(directInput,new DecodeOptions { Limits=directLimits });
            if(!directInput.CanRead||Lokad.Image.Tooling.OriginalDirectRgbControls.Hash(Lokad.Image.Tooling.OriginalPixels.Bytes(directStream))!=direct.RgbaSha256)
                throw new Exception("Packaged direct-RGB synchronous delivery differs.");
        }
        using(var directInput=new MemoryStream(direct.Encoded,writable:false))
        {
            using var directStream=await ImageCodec.DecodeAsync(directInput,new DecodeOptions { Limits=directLimits });
            if(!directInput.CanRead||Lokad.Image.Tooling.OriginalDirectRgbControls.Hash(Lokad.Image.Tooling.OriginalPixels.Bytes(directStream))!=direct.RgbaSha256)
                throw new Exception("Packaged direct-RGB asynchronous delivery differs.");
        }
    }
    foreach(var directOrient in new[] { false,true })
    {
        using var directPacket=new MemoryStream();directPacket.Write(direct.Encoded.AsSpan(0,2));
        directPacket.Write(new byte[] {255,225,(byte)(app1Length>>8),(byte)app1Length});
        directPacket.Write("Exif\0\0"u8);directPacket.Write(tiff);directPacket.Write(direct.Encoded.AsSpan(2));
        var directWidth=directOrient?259:513;var directHeight=directOrient?513:259;
        foreach(var directFormat in new[] {JpegSampleFormat.Rgb,JpegSampleFormat.Grayscale})
        {
            var directChannels=(int)directFormat;var directStride=directWidth*directChannels+7;
            var directSamples=Enumerable.Repeat((byte)165,directStride*directHeight+11).ToArray();
            ImageCodec.DecodeJpeg(directPacket.ToArray(),directSamples,directFormat,directStride,
                new JpegDecodeOptions { ApplyOrientation=directOrient,Limits=directLimits });
            var directPacked=new byte[513*259*directChannels];
            for(var y=0;y<259;y++)for(var x=0;x<513;x++)
            {
                var dx=directOrient?258-y:x;var dy=directOrient?x:y;
                directSamples.AsSpan(dy*directStride+dx*directChannels,directChannels).CopyTo(directPacked.AsSpan((y*513+x)*directChannels,directChannels));
            }
            if(Lokad.Image.Tooling.OriginalDirectRgbControls.Hash(directPacked)!=(directFormat==JpegSampleFormat.Rgb?direct.RgbSha256:direct.GraySha256))
                throw new Exception("Packaged direct-RGB caller RGB/gray/orientation differs.");
            for(var y=0;y<directHeight;y++)if(directSamples.AsSpan(y*directStride+directWidth*directChannels,7).ContainsAnyExcept((byte)165))
                throw new Exception("Packaged direct-RGB row padding changed.");
            if(directSamples.AsSpan(directStride*directHeight).ContainsAnyExcept((byte)165))throw new Exception("Packaged direct-RGB tail changed.");
        }
    }
    if(directRoot.Bytes!=0||directRoot.IsDisposed)throw new Exception("Packaged direct-RGB admission cleanup differs.");
}
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
    $jpegExactFixture = Join-Path $repoRoot 'tests/Fixtures/jpeg-ffmpeg-YCbCr444-31x23.jpg'
    $jpegExactExpected = Join-Path $repoRoot 'tests/Fixtures/jpeg-ffmpeg-YCbCr444-31x23.rgba'
    dotnet build (Join-Path $consumer 'Consumer.csproj') -c Release --no-restore --tl:off --nologo -v minimal -m:1 -nr:false -p:UseSharedCompilation=false
    if ($LASTEXITCODE -ne 0) { throw 'Package-only consumer build failed.' }
    dotnet (Join-Path $consumer 'bin/Release/net10.0/Consumer.dll') $vp8Fixture $vp8Expected $pngFixture $pngExpected $librarySha256 $jpegExactFixture $jpegExactExpected
    if ($LASTEXITCODE -ne 0) { throw 'Package-only consumer failed.' }
    & (Join-Path $PSScriptRoot 'verify-usage.ps1') -ConsumerDirectory $consumer -GuidePath (Join-Path $repoRoot 'docs/usage.md') -LibrarySha256 $librarySha256
} finally { Pop-Location }
