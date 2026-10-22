param(
    [Parameter(Mandatory)][string]$SourceGrayPath,
    [Parameter(Mandatory)][string]$NativePacketPath,
    [string]$OutputPath
)
$ErrorActionPreference = 'Stop'
function Hash([byte[]]$Bytes) { [Convert]::ToHexStringLower([Security.Cryptography.SHA256]::HashData($Bytes)) }
$source = [IO.File]::ReadAllBytes((Resolve-Path -LiteralPath $SourceGrayPath).Path)
$packet = [IO.File]::ReadAllBytes((Resolve-Path -LiteralPath $NativePacketPath).Path)
$sourceHash = '6b74f1d2c127f3bcccba2b377f4474b9fc7464890853d05bf058bbb38337df8d'
$packetHash = '25fb908c13a1e15645bfe07948e7f1d70366d6bf26e99eb3a2a88f112c78ea02'
if ($source.Length -ne 132867 -or (Hash $source) -ne $sourceHash -or
    $packet.Length -ne 61152 -or (Hash $packet) -ne $packetHash) { throw 'Frozen source/restart packet differs.' }
Add-Type -Path @((Join-Path $PSScriptRoot 'JpegColorParity.cs'),
    (Join-Path $PSScriptRoot 'JpegSubsampleParity.cs'), (Join-Path $PSScriptRoot 'JpegForwardParity.cs'),
    (Join-Path $PSScriptRoot 'JpegScanParity.cs'), (Join-Path $PSScriptRoot 'JpegReferenceTables.cs'),
    (Join-Path $PSScriptRoot 'JpegGrayParity.cs'), (Join-Path $PSScriptRoot 'JpegGrayReferenceTables.cs'),
    (Join-Path $PSScriptRoot 'JpegGrayPacketParity.cs'))
$first = [JpegGrayPacketParity]::Encode($source, 3)
$next = [JpegGrayPacketParity]::Encode($source, 3)
$noRestart = [JpegGrayPacketParity]::Encode($source)
if (-not [JpegColorParity]::Equal($first.Packet, $packet) -or
    -not [JpegColorParity]::Equal($next.Packet, $packet) -or
    [Object]::ReferenceEquals($first.Packet, $next.Packet) -or
    (Hash $source) -ne $sourceHash -or
    (Hash $noRestart.Packet) -ne '444e39e6126dd3077d22c0b4f60e38d8a22611fa5721b226c5168a63b7e94423') {
    throw 'Complete source-only packet, old packet, borrowed source or independent output differs.'
}
$mutated = [byte[]]$packet.Clone(); $mutated[-3] = $mutated[-3] -bxor 1
if ([JpegColorParity]::Equal($first.Packet, $mutated)) { throw 'Changed entropy byte was accepted.' }
$restarts = 0
for ($i = 0; $i -lt $first.Packet.Length - 1; $i++) {
    if ($first.Packet[$i] -eq 255 -and $first.Packet[$i + 1] -ge 208 -and $first.Packet[$i + 1] -le 215) {
        if ($first.Packet[$i + 1] -ne 208 + ($restarts -band 7)) { throw 'Restart sequence differs.' }
        $restarts++
    }
}
if ($restarts -ne 714) { throw 'Restart count differs.' }
if ($OutputPath) { [IO.File]::WriteAllBytes($OutputPath, $first.Packet) }
[ordered]@{
    project='Lokad.Image'; round='E13'; prototypeOnly=$true; rawExact=$true;
    sourceGraySha256=$sourceHash; packetBytes=$first.Packet.Length; packetSha256=$packetHash;
    restartInterval=3; restartMarkers=$restarts; optimizedHuffman=$true; oldNoRestartPacketExact=$true;
    oraclePacketOrTableInputs=$false; sourceAndSurvivingOutputsPreserved=$true;
    changedEntropyByteRejected=$true; attributionExceptionUsed=$false;
    productionChanged=$false; productionBuilds=0; timingRuns=0
} | ConvertTo-Json -Depth 4
