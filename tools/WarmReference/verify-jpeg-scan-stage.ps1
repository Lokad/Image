param(
    [Parameter(Mandatory)][string]$SourcePath,
    [Parameter(Mandatory)][string]$ReferencePacketPath,
    [Parameter(Mandatory)][string]$ExpectedPhantomsPath,
    [Parameter(Mandatory)][ValidateSet(8,16)][int]$Depth,
    [string]$OutputPath
)
$ErrorActionPreference = 'Stop'
$control = (Get-Content -Raw (Join-Path $PSScriptRoot '../../benchmarks/jpeg-byte-parity-evidence.json') | ConvertFrom-Json).controls |
    Where-Object sourceDepth -EQ $Depth
if ((Get-Item -LiteralPath $SourcePath).Length -ne (513 * 259 * $Depth / 2) -or
    (Get-Item -LiteralPath $ReferencePacketPath).Length -ne $control.nativeCompletePacketBytes -or
    (Get-Item -LiteralPath $ExpectedPhantomsPath).Length -ne 24960) { throw 'Frozen stage inputs required.' }
Add-Type -Path @((Join-Path $PSScriptRoot 'JpegColorParity.cs'),
    (Join-Path $PSScriptRoot 'JpegForwardParity.cs'), (Join-Path $PSScriptRoot 'JpegScanParity.cs'),
    (Join-Path $PSScriptRoot 'JpegReferenceTables.cs'))
function Hash([byte[]]$Bytes) { [Convert]::ToHexStringLower([Security.Cryptography.SHA256]::HashData($Bytes)) }
$source = [IO.File]::ReadAllBytes((Resolve-Path -LiteralPath $SourcePath).Path)
$reference = [IO.File]::ReadAllBytes((Resolve-Path -LiteralPath $ReferencePacketPath).Path)
$phantoms = [IO.File]::ReadAllBytes((Resolve-Path -LiteralPath $ExpectedPhantomsPath).Path)
if ((Hash $source) -ne $control.sourceSha256 -or (Hash $reference) -ne $control.nativeCompletePacketSha256) {
    throw 'Frozen source or reference identity differs.'
}
$planes = [JpegColorParity]::Convert($source, $Depth)
$nativeAttribution = [JpegScanParity]::Encode($planes, $reference, $phantoms, $true)
if (-not [JpegColorParity]::Equal($nativeAttribution.Packet, $reference)) {
    $first = 0
    while ($first -lt [Math]::Min($nativeAttribution.Packet.Length, $reference.Length) -and
        $nativeAttribution.Packet[$first] -eq $reference[$first]) { $first++ }
    throw "Complete stage packet differs at byte $first; actual $($nativeAttribution.Packet.Length), reference $($reference.Length)."
}
$ownedAttribution = [JpegScanParity]::Encode($planes, $reference, $phantoms, $false)
if ([Object]::ReferenceEquals($nativeAttribution.Packet, $ownedAttribution.Packet) -or
    -not [JpegColorParity]::Equal($nativeAttribution.Packet, $reference) -or (Hash $source) -ne $control.sourceSha256) {
    throw 'Surviving packet or borrowed input changed.'
}
$generatedNative = [JpegScanParity]::EncodeGenerated($source, $Depth, $true)
if (-not [JpegColorParity]::Equal($generatedNative.Packet, $reference)) {
    throw 'Independent generated-table encoder differs from the complete native packet.'
}
$generatedOwned = [JpegScanParity]::EncodeGenerated($source, $Depth, $false)
if (-not [JpegColorParity]::Equal($generatedOwned.Packet, $ownedAttribution.Packet) -or
    (Hash $source) -ne $control.sourceSha256 -or (Hash $reference) -ne $control.nativeCompletePacketSha256 -or
    -not [JpegColorParity]::Equal($nativeAttribution.Packet, $reference)) {
    throw 'Independent generated-table encoder or surviving inputs/outputs differ.'
}
if ($OutputPath) { [IO.File]::WriteAllBytes($OutputPath, $generatedOwned.Packet) }
[ordered]@{ depth=$Depth; completeNativeAttributionPacketExact=$true;
    nativePacketBytes=$nativeAttribution.Packet.Length; nativePacketSha256=(Hash $nativeAttribution.Packet);
    managedAttributionBytes=$ownedAttribution.Packet.Length; managedAttributionSha256=(Hash $ownedAttribution.Packet);
    phantomValues=$ownedAttribution.PhantomValues; sourceUnchanged=$true; survivingPacketUnchanged=$true;
    oracleTablesSupplied=$false; suppliedTableStageExact=$true; independentCompleteEncoder=$true;
    generatedNativeAttributionSha256=(Hash $generatedNative.Packet);
    histograms=$generatedOwned.Histograms } | ConvertTo-Json -Depth 5
