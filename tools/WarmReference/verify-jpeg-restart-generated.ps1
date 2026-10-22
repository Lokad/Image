param(
    [Parameter(Mandatory)][string]$SourcePath,
    [Parameter(Mandatory)][string]$NativePacketPath,
    [string]$OutputPrefix
)
$ErrorActionPreference='Stop'
function Hash([byte[]]$Bytes) { [Convert]::ToHexStringLower([Security.Cryptography.SHA256]::HashData($Bytes)) }
$source=[IO.File]::ReadAllBytes((Resolve-Path -LiteralPath $SourcePath).Path)
$packet=[IO.File]::ReadAllBytes((Resolve-Path -LiteralPath $NativePacketPath).Path)
$sourceHash='70f0fb18c341e2740b875a5a34e21ca63f0821b3386352fe2971020c5a6aac80'
$packetHash='bd0060bf5bbf48a9f55120c5e71b22ac2763751bb370e9c5c2d93ebba38ed622'
if ($source.Length -ne 531468 -or (Hash $source) -ne $sourceHash -or
    $packet.Length -ne 58012 -or (Hash $packet) -ne $packetHash) { throw 'Frozen source or native restart packet differs.' }
Add-Type -Path @((Join-Path $PSScriptRoot 'JpegColorParity.cs'),
    (Join-Path $PSScriptRoot 'JpegSubsampleParity.cs'), (Join-Path $PSScriptRoot 'JpegForwardParity.cs'),
    (Join-Path $PSScriptRoot 'JpegScanParity.cs'), (Join-Path $PSScriptRoot 'JpegReferenceTables.cs'),
    (Join-Path $PSScriptRoot 'JpegFixedReferenceTables.cs'), (Join-Path $PSScriptRoot 'JpegRestartParity.cs'))
$quantization=$null; $tables=$null
[JpegScanParity]::ReadTables($packet,[ref]$quantization,[ref]$tables)
$tableResults=@()
foreach ($family in @(0,1,2,3)) {
    $table=[JpegFixedReferenceTables]::Create($family)
    if (-not [JpegColorParity]::Equal($table.Counts,$tables[$family].Counts) -or
        -not [JpegColorParity]::Equal($table.Symbols,$tables[$family].Symbols)) { throw "Fixed format table $family differs." }
    $id=[byte](($family -shr 1)*16+($family -band 1))
    [byte[]]$payload=@($id)+$table.Counts+$table.Symbols
    $tableResults+=@{id=$id;symbols=$table.Symbols.Length;sha256=(Hash $payload)}
}
$native=[JpegRestartParity]::Encode($source,$true)
$own=[JpegRestartParity]::Encode($source,$false)
$next=[JpegRestartParity]::Encode($source,$false)
if (-not [JpegColorParity]::Equal($native.Packet,$packet) -or
    -not [JpegColorParity]::Equal($own.Packet,$next.Packet) -or
    [Object]::ReferenceEquals($own.Packet,$next.Packet) -or
    (Hash $own.Packet) -ne '1af33b85d17d4dd24a19f98e66242eba42c7c77b7347dfa564a83d64b19feea4' -or
    (Hash $source) -ne $sourceHash -or (Hash $packet) -ne $packetHash) {
    throw 'Source-only restart packet, source preservation or surviving output differs.'
}
if ($OutputPrefix) {
    [IO.File]::WriteAllBytes("$OutputPrefix-native-attribution.jpg",$native.Packet)
    [IO.File]::WriteAllBytes("$OutputPrefix-own-attribution.jpg",$own.Packet)
}
[ordered]@{
    project='Lokad.Image';round='E10';prototypeOnly=$true
    oraclePacketOrTableInputs=$false;sourceRgba8Sha256=$sourceHash;fixedTables=$tableResults
    rawExact=$true;nativeAttributionBytes=$native.Packet.Length;nativeAttributionSha256=(Hash $native.Packet)
    ownAttributionBytes=$own.Packet.Length;ownAttributionSha256=(Hash $own.Packet)
    rowRestartInterval=33;restartMarkers=16;sourceAndSurvivingOutputsPreserved=$true
    productionChanged=$false;productionBuilds=0;nativeEncodes=0;timingRuns=0
} | ConvertTo-Json -Depth 5
