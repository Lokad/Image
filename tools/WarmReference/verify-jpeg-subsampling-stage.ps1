param(
    [Parameter(Mandatory)][string]$SourcePath,
    [Parameter(Mandatory)][string]$ExpectedYuvPath,
    [Parameter(Mandatory)][string]$ReferencePacketPath,
    [Parameter(Mandatory)][ValidateSet(8,16)][int]$Depth,
    [Parameter(Mandatory)][ValidateSet(422,420)][int]$Sampling,
    [string]$OutputPath
)
$ErrorActionPreference = 'Stop'
$evidence = Get-Content -Raw (Join-Path $PSScriptRoot '../../benchmarks/jpeg-byte-parity-evidence.json') | ConvertFrom-Json
$sourceControl = $evidence.controls | Where-Object sourceDepth -EQ $Depth
$stageControl = $evidence.subsampling.controls | Where-Object { $_.depth -eq $Depth -and $_.sampling -eq $Sampling }
if (-not $sourceControl -or -not $stageControl) { throw 'Frozen subsampling identities required.' }
function Hash([byte[]]$Bytes) { [Convert]::ToHexStringLower([Security.Cryptography.SHA256]::HashData($Bytes)) }
$source = [IO.File]::ReadAllBytes((Resolve-Path -LiteralPath $SourcePath).Path)
$reference = [IO.File]::ReadAllBytes((Resolve-Path -LiteralPath $ReferencePacketPath).Path)
$planes = [IO.File]::ReadAllBytes((Resolve-Path -LiteralPath $ExpectedYuvPath).Path)
if ($source.Length -ne 513 * 259 * $Depth / 2 -or $planes.Length -ne $stageControl.stageBytes -or
    (Hash $source) -ne $sourceControl.sourceSha256 -or
    (Hash $reference) -ne $stageControl.nativePacketSha256 -or
    (Hash $planes) -ne $stageControl.stageSha256) { throw 'Frozen source, stage or packet identity differs.' }
Add-Type -Path @((Join-Path $PSScriptRoot 'JpegColorParity.cs'),
    (Join-Path $PSScriptRoot 'JpegSubsampleParity.cs'),
    (Join-Path $PSScriptRoot 'JpegForwardParity.cs'),
    (Join-Path $PSScriptRoot 'JpegScanParity.cs'),
    (Join-Path $PSScriptRoot 'JpegReferenceTables.cs'))
$actualPlanes = [JpegSubsampleParity]::Convert($source, $Depth, $Sampling)
if (-not [JpegColorParity]::Equal($actualPlanes, $planes)) { throw 'Exact chroma stage differs.' }
$native = [JpegScanParity]::EncodeGenerated($source, $Depth, $true, $Sampling)
if (-not [JpegColorParity]::Equal($native.Packet, $reference)) { throw 'Complete generated packet differs.' }
$owned = [JpegScanParity]::EncodeGenerated($source, $Depth, $false, $Sampling)
# Reconstruct only the specifically approved attribution segment for comparison.
# Every byte outside that single COM, including every other header, must survive.
$adjusted = [Collections.Generic.List[byte]]::new($reference.Length)
$adjusted.AddRange([byte[]]@(255,216,255,254,0,14))
$adjusted.AddRange([Text.Encoding]::ASCII.GetBytes("Lokad.Image`0"))
$adjusted.AddRange([byte[]]$reference[20..($reference.Length - 1)])
if (-not [JpegColorParity]::Equal($owned.Packet, $adjusted.ToArray()) -or
    [Object]::ReferenceEquals($owned.Packet, $native.Packet) -or
    -not [JpegColorParity]::Equal($native.Packet, $reference) -or
    (Hash $source) -ne $sourceControl.sourceSha256 -or
    (Hash $planes) -ne $stageControl.stageSha256 -or
    (Hash $reference) -ne $stageControl.nativePacketSha256) { throw 'Attribution exception or independent ownership differs.' }
if ($OutputPath) { [IO.File]::WriteAllBytes($OutputPath, $owned.Packet) }
[ordered]@{
    depth=$Depth; sampling=$Sampling; width=513; height=259;
    exactSamples=$planes.Length; exactPlanes=$true; stageSha256=(Hash $actualPlanes);
    sourceUnchanged=$true; sourceSha256=(Hash $source); survivingPacketUnchanged=$true;
    completeNativeAttributionPacketExact=$true; nativePacketBytes=$native.Packet.Length;
    nativePacketSha256=(Hash $native.Packet); ownAttributionPacketBytes=$owned.Packet.Length;
    ownAttributionPacketSha256=(Hash $owned.Packet); onlyApprovedAttributionDifference=$true;
    oraclePacketOrTableInputs=$false; independentCompleteEncoder=$true; phantomValues=$owned.PhantomValues
} | ConvertTo-Json -Depth 4
