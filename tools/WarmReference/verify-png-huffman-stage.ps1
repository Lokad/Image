param(
    [Parameter(Mandatory)][string]$SourcePath,
    [Parameter(Mandatory)][string]$NativeAuditPath
)
$ErrorActionPreference = 'Stop'
function Hash([byte[]]$Bytes) { [Convert]::ToHexStringLower([Security.Cryptography.SHA256]::HashData($Bytes)) }
function Equal([byte[]]$Left, [byte[]]$Right) { [Linq.Enumerable]::SequenceEqual[byte]($Left, $Right) }
function Structures($Blocks) {
    $bytes = [IO.MemoryStream]::new()
    try {
        foreach ($block in $Blocks) {
            foreach ($values in @($block.Literals, $block.Distances, $block.Lengths)) {
                $bytes.WriteByte($values.Length -band 255); $bytes.WriteByte($values.Length -shr 8)
                $bytes.Write($values)
            }
            $bytes.WriteByte($block.Runs.Length -band 255); $bytes.WriteByte($block.Runs.Length -shr 8)
            foreach ($run in $block.Runs) {
                $bytes.WriteByte($run.Symbol); $bytes.WriteByte($run.Extra); $bytes.WriteByte($run.ExtraBits)
            }
        }
        return ,$bytes.ToArray()
    } finally { $bytes.Dispose() }
}
$source = [IO.File]::ReadAllBytes((Resolve-Path -LiteralPath $SourcePath).Path)
$sourceHash = '70f0fb18c341e2740b875a5a34e21ca63f0821b3386352fe2971020c5a6aac80'
$audit = Get-Content -Raw -LiteralPath $NativeAuditPath | ConvertFrom-Json
if ($source.Length -ne 531468 -or (Hash $source) -ne $sourceHash -or
    $audit.packetSha256 -ne 'ad9ed127da583d68bdb749bb983b73966e8e814527216449c4557fdd89668920' -or
    $audit.tokenSha256 -ne '4c44f8be47e1b4248dde4f232937a7467ff716e83c7d52f382b12b39ef364673' -or
    $audit.blocks.Count -ne 7 -or -not $audit.independentExpansionExact) {
    throw 'Frozen source or independent reference observation differs.'
}
Add-Type -Path @((Join-Path $PSScriptRoot 'PngDeflateParity.cs'), (Join-Path $PSScriptRoot 'PngHuffmanParity.cs'))
# No oracle tree, frequencies, runs, tokens or packet enter Generate.
$blocks = [PngHuffmanParity]::Generate($source)
$next = [PngHuffmanParity]::Generate($source)
if ($blocks.Length -ne 7 -or (Hash $source) -ne $sourceHash) { throw 'Block count or source differs.' }
$runs = 0
for ($i = 0; $i -lt $blocks.Length; $i++) {
    $actual = $blocks[$i]; $expected = $audit.blocks[$i]
    if (-not (Equal $actual.Literals ([byte[]]$expected.literalLengths)) -or
        -not (Equal $actual.Distances ([byte[]]$expected.distanceLengths)) -or
        -not (Equal $actual.Lengths ([byte[]]$expected.codeLengths)) -or
        $actual.Runs.Length -ne $expected.headerRuns.Count) { throw "Coding structures differ at block $i." }
    for ($j = 0; $j -lt $actual.Runs.Length; $j++) {
        $a = $actual.Runs[$j]; $e = $expected.headerRuns[$j]
        if ($a.Symbol -ne $e[0] -or $a.Extra -ne $e[1] -or $a.ExtraBits -ne $e[2]) {
            throw "Code-length run differs at block $i, run $j."
        }
    }
    $runs += $actual.Runs.Length
}
$firstBytes = Structures $blocks; $nextBytes = Structures $next
if (-not (Equal $firstBytes $nextBytes) -or [Object]::ReferenceEquals($blocks[0].Literals, $next[0].Literals)) {
    throw 'Repeated structures or independent outputs differ.'
}
$next[0].Literals[0] = $next[0].Literals[0] -bxor 1
if ((Equal (Structures $next) $firstBytes) -or -not (Equal (Structures $blocks) $firstBytes)) {
    throw 'Changed coding length was accepted or surviving output changed.'
}
[ordered]@{
    sourceOnly=$true; sourceSha256=(Hash $source); sourceUnchanged=$true
    blocks=7; exactTrees=21; exactCodeLengthRuns=$runs; structuresBytes=$firstBytes.Length
    structuresSha256=(Hash $firstBytes); negativeMutationRejected=$true
    independentLiveOutputs=$true; survivingOutputUnchanged=$true; oracleEncoderInputs=$false
    productionBuilds=0; nativeEncodes=0; timingRuns=0
} | ConvertTo-Json
