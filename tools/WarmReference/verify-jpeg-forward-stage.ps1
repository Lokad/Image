param(
    [Parameter(Mandatory)][string]$InputYuvPath,
    [Parameter(Mandatory)][string]$ExpectedDctPath,
    [string]$ExpectedCoefficientsPath
)
$ErrorActionPreference = 'Stop'
if ((Get-Item -LiteralPath $InputYuvPath).Length -ne (513 * 259 * 3) -or
    (Get-Item -LiteralPath $ExpectedDctPath).Length -ne (65 * 33 * 3 * 128)) {
    throw 'Frozen 513x259 planar YUV444 and complete little-endian transform output required.'
}
Add-Type -TypeDefinition ([IO.File]::ReadAllText((Join-Path $PSScriptRoot 'JpegForwardParity.cs')))
function Hash([byte[]]$Bytes) { [Convert]::ToHexStringLower([Security.Cryptography.SHA256]::HashData($Bytes)) }
$inputPlanes = [IO.File]::ReadAllBytes((Resolve-Path -LiteralPath $InputYuvPath).Path)
$inputHash = Hash $inputPlanes
$expected = [IO.File]::ReadAllBytes((Resolve-Path -LiteralPath $ExpectedDctPath).Path)
$difference = [JpegForwardParity]::FirstDifference($inputPlanes, $expected)
if ($difference -ge 0) { throw "Exact forward-stage comparison failed at coefficient $difference." }
$expected[0] = $expected[0] -bxor 1
if ([JpegForwardParity]::FirstDifference($inputPlanes, $expected) -ne 0) { throw 'Changed forward coefficient was not rejected.' }
$expected[0] = $expected[0] -bxor 1
$quantizationExact = $null
$coefficientHash = $null
if ($ExpectedCoefficientsPath) {
    if ((Get-Item -LiteralPath $ExpectedCoefficientsPath).Length -ne 824144) {
        throw 'Frozen public coefficient dump required.'
    }
    $coefficients = [IO.File]::ReadAllBytes((Resolve-Path -LiteralPath $ExpectedCoefficientsPath).Path)
    $difference = [JpegForwardParity]::FirstQuantizationDifference($inputPlanes, $coefficients)
    if ($difference -ge 0) { throw "Exact quantization comparison failed at coefficient $difference." }
    $coefficients[168] = $coefficients[168] -bxor 1
    if ([JpegForwardParity]::FirstQuantizationDifference($inputPlanes, $coefficients) -ne 0) { throw 'Changed coded coefficient was not rejected.' }
    $coefficients[168] = $coefficients[168] -bxor 1
    $quantizationExact = $true
    $coefficientHash = Hash $coefficients
}
if ((Hash $inputPlanes) -ne $inputHash) { throw 'Borrowed input changed.' }
[ordered]@{ exact=$true; values=411840; blocks=6435; inputUnchanged=$true;
    changedCoefficientRejected=$true;
    inputSha256=$inputHash; expectedDctSha256=(Hash $expected);
    quantizationExact=$quantizationExact; coefficientDumpSha256=$coefficientHash } | ConvertTo-Json
