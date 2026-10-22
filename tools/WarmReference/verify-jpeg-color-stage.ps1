param(
    [Parameter(Mandatory)][string]$SourcePath,
    [Parameter(Mandatory)][string]$ExpectedYuvPath,
    [Parameter(Mandatory)][ValidateSet(8,16)][int]$Depth
)
$ErrorActionPreference = 'Stop'
if ((Get-Item -LiteralPath $SourcePath).Length -ne (513 * 259 * $Depth / 2) -or
    (Get-Item -LiteralPath $ExpectedYuvPath).Length -ne (513 * 259 * 3)) {
    throw 'This stage check covers only the frozen 513x259 RGBA controls.'
}
Add-Type -TypeDefinition ([IO.File]::ReadAllText((Join-Path $PSScriptRoot 'JpegColorParity.cs')))
function Hash([byte[]]$Bytes) { [Convert]::ToHexStringLower([Security.Cryptography.SHA256]::HashData($Bytes)) }
$source = [IO.File]::ReadAllBytes((Resolve-Path -LiteralPath $SourcePath).Path)
$sourceHash = Hash $source
$expected = [IO.File]::ReadAllBytes((Resolve-Path -LiteralPath $ExpectedYuvPath).Path)
$actual = [JpegColorParity]::Convert($source, $Depth)
$actualHash = Hash $actual
$expectedHash = Hash $expected
if (-not [JpegColorParity]::Equal($actual, $expected) -or (Hash $source) -ne $sourceHash) {
    throw 'Exact color-stage comparison or borrowed-source preservation failed.'
}
[ordered]@{ depth=$Depth; width=513; height=259; samples=$actual.Length; exact=$true;
    sourceUnchanged=$true; sourceSha256=$sourceHash; yuvSha256=$actualHash } | ConvertTo-Json
