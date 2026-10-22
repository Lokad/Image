param(
    [Parameter(Mandatory)][string]$SourcePath,
    [Parameter(Mandatory)][string]$NativePacketPath,
    [string]$OutputPath
)
$ErrorActionPreference = 'Stop'
function Hash([byte[]]$Bytes) { [Convert]::ToHexStringLower([Security.Cryptography.SHA256]::HashData($Bytes)) }
function Equal([byte[]]$Left, [byte[]]$Right) { [Linq.Enumerable]::SequenceEqual[byte]($Left, $Right) }
$source = [IO.File]::ReadAllBytes((Resolve-Path -LiteralPath $SourcePath).Path)
$native = [IO.File]::ReadAllBytes((Resolve-Path -LiteralPath $NativePacketPath).Path)
$sourceHash = '70f0fb18c341e2740b875a5a34e21ca63f0821b3386352fe2971020c5a6aac80'
$packetHash = 'ad9ed127da583d68bdb749bb983b73966e8e814527216449c4557fdd89668920'
if ($source.Length -ne 531468 -or (Hash $source) -ne $sourceHash -or
    $native.Length -ne 210973 -or (Hash $native) -ne $packetHash) {
    throw 'Frozen source or native level6 packet differs.'
}
Add-Type -Path @((Join-Path $PSScriptRoot 'PngDeflateParity.cs'),
    (Join-Path $PSScriptRoot 'PngHuffmanParity.cs'), (Join-Path $PSScriptRoot 'PngPacketParity.cs'))
# Source pixels are the ONLY encoder input, including token/tree/framing generation.
$first = [PngPacketParity]::Encode($source)
$next = [PngPacketParity]::Encode($source)
if (-not (Equal $first.Packet $native) -or -not (Equal $next.Packet $native) -or
    [Object]::ReferenceEquals($first.Packet, $next.Packet) -or (Hash $source) -ne $sourceHash -or
    -not [Linq.Enumerable]::SequenceEqual[int]($first.BlockEndBits, [int[]]@(242485,490966,739587,988053,1235177,1482570,1682209))) {
    throw 'Complete source-only packet, block bits, source or ownership differs.'
}
$next.Packet[0] = $next.Packet[0] -bxor 1
if ((Equal $next.Packet $native) -or -not (Equal $first.Packet $native)) {
    throw 'Changed packet was accepted or surviving output changed.'
}
if ($OutputPath) { [IO.File]::WriteAllBytes([IO.Path]::GetFullPath($OutputPath), $first.Packet) }
[ordered]@{
    sourceOnly=$true; sourceSha256=(Hash $source); sourceUnchanged=$true
    packetBytes=$first.Packet.Length; packetSha256=(Hash $first.Packet); completePacketRawExact=$true
    exactBlockEndBits=$first.BlockEndBits; attributionExceptionUsed=$false
    negativeMutationRejected=$true; independentLiveOutputs=$true; survivingOutputUnchanged=$true
    oracleEncoderInputs=$false; productionBuilds=0; nativeEncodes=0; timingRuns=0
} | ConvertTo-Json
