param(
    [Parameter(Mandatory)][string]$SourcePlanesPath,
    [Parameter(Mandatory)][string]$NativePacketPath,
    [string]$OutputPrefix
)
$ErrorActionPreference = 'Stop'
function Hash([byte[]]$Bytes) { [Convert]::ToHexStringLower([Security.Cryptography.SHA256]::HashData($Bytes)) }
$planes = [IO.File]::ReadAllBytes((Resolve-Path -LiteralPath $SourcePlanesPath).Path)
$packet = [IO.File]::ReadAllBytes((Resolve-Path -LiteralPath $NativePacketPath).Path)
$sourceHash = 'ce4011effc2b9cecf5ecb432ec1a9b4ed18afdc65abed499eb962beca7a2624e'
$packetHash = 'bd0060bf5bbf48a9f55120c5e71b22ac2763751bb370e9c5c2d93ebba38ed622'
if ($planes.Length -ne 199687 -or (Hash $planes) -ne $sourceHash -or
    $packet.Length -ne 58012 -or (Hash $packet) -ne $packetHash) {
    throw 'Frozen420 planes or row-restart33 native packet differs.'
}
Add-Type -Path @((Join-Path $PSScriptRoot 'JpegColorParity.cs'),
    (Join-Path $PSScriptRoot 'JpegSubsampleParity.cs'), (Join-Path $PSScriptRoot 'JpegForwardParity.cs'),
    (Join-Path $PSScriptRoot 'JpegScanParity.cs'), (Join-Path $PSScriptRoot 'JpegReferenceTables.cs'))
$native = [JpegScanParity]::EncodeRowRestartStage($planes, $packet, $true)
$own = [JpegScanParity]::EncodeRowRestartStage($planes, $packet, $false)
$next = [JpegScanParity]::EncodeRowRestartStage($planes, $packet, $false)
if (-not [JpegColorParity]::Equal($native.Packet, $packet) -or
    -not [JpegColorParity]::Equal($own.Packet, $next.Packet) -or
    [Object]::ReferenceEquals($own.Packet, $next.Packet) -or
    (Hash $planes) -ne $sourceHash -or (Hash $packet) -ne $packetHash) {
    throw 'Supplied-table row-restart stage, input preservation or surviving output differs.'
}
if ($OutputPrefix) {
    [IO.File]::WriteAllBytes("$OutputPrefix-native-attribution.jpg", $native.Packet)
    [IO.File]::WriteAllBytes("$OutputPrefix-own-attribution.jpg", $own.Packet)
}
[ordered]@{
    project = 'Lokad.Image'; round = 'E9'; stageOnly = $true
    quantizersAndHuffmanTablesSupplied = $true; sourcePlanesSha256 = $sourceHash
    rawExact = $true; nativeAttributionBytes = $native.Packet.Length
    nativeAttributionSha256 = Hash $native.Packet
    ownAttributionBytes = $own.Packet.Length; ownAttributionSha256 = Hash $own.Packet
    rowRestartInterval = 33; restartMarkers = 16
    sourceAndSurvivingOutputsPreserved = $true
    productionChanged = $false; productionBuilds = 0; nativeEncodes = 0; timingRuns = 0
} | ConvertTo-Json -Depth 4
