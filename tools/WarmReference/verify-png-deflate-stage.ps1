param(
    [Parameter(Mandatory)][string]$SourcePath,
    [Parameter(Mandatory)][string]$NativeTokensPath,
    [Parameter(Mandatory)][string]$NativeAuditPath
)
$ErrorActionPreference = 'Stop'
function Read([string]$Path) { return ,([IO.File]::ReadAllBytes((Resolve-Path -LiteralPath $Path).Path)) }
function Hash([byte[]]$Bytes) { [Convert]::ToHexStringLower([Security.Cryptography.SHA256]::HashData($Bytes)) }
function Equal([byte[]]$Left, [byte[]]$Right) { [Linq.Enumerable]::SequenceEqual[byte]($Left, $Right) }
$source = Read $SourcePath
$expected = Read $NativeTokensPath
$audit = Get-Content -Raw -LiteralPath $NativeAuditPath | ConvertFrom-Json
$sourceHash = '70f0fb18c341e2740b875a5a34e21ca63f0821b3386352fe2971020c5a6aac80'
$tokenHash = '4c44f8be47e1b4248dde4f232937a7467ff716e83c7d52f382b12b39ef364673'
if ($source.Length -ne 531468 -or (Hash $source) -ne $sourceHash -or
    $expected.Length -ne 669066 -or (Hash $expected) -ne $tokenHash -or
    $audit.tokenSha256 -ne $tokenHash -or $audit.blocks.Count -ne 7 -or
    -not $audit.independentExpansionExact) { throw 'Frozen source or independent token observation differs.' }
Add-Type -Path (Join-Path $PSScriptRoot 'PngDeflateParity.cs')
# Only source pixels enter the encoder. Oracle tokens are comparison inputs below.
$first = [PngDeflateParity]::EncodeTokens($source)
$next = [PngDeflateParity]::EncodeTokens($source)
if (-not (Equal $first $expected) -or -not (Equal $next $expected) -or
    [Object]::ReferenceEquals($first, $next) -or (Hash $source) -ne $sourceHash) {
    throw 'Source-only tokens, source preservation or independent ownership differs.'
}
$sha = [Security.Cryptography.SHA256]::Create()
try {
    $cursor = 0
    foreach ($block in $audit.blocks) {
        if ($block.firstToken -ne $cursor) { throw 'Observed block token coverage is discontinuous.' }
        $blockHash = [Convert]::ToHexStringLower($sha.ComputeHash($first, $cursor * 6, $block.tokens * 6))
        if ($blockHash -ne $block.tokenSha256) { throw "Block token sequence differs at $cursor." }
        $cursor += $block.tokens
    }
    if ($cursor -ne 111511) { throw 'Incomplete block token coverage.' }
} finally { $sha.Dispose() }
$next[0] = $next[0] -bxor 1
if ((Equal $next $expected) -or -not (Equal $first $expected)) {
    throw 'Changed token was accepted or surviving output was changed.'
}
[ordered]@{
    sourceOnly=$true; sourceSha256=(Hash $source); sourceUnchanged=$true
    tokenCount=$first.Length / 6; tokenBytes=$first.Length; tokenSha256=(Hash $first)
    exactObservedBlocks=7; completeTokenSequenceExact=$true; negativeMutationRejected=$true
    independentLiveOutputs=$true; survivingOutputUnchanged=$true; oracleEncoderInputs=$false
    productionBuilds=0; nativeEncodes=0; timingRuns=0
} | ConvertTo-Json -Depth 4
