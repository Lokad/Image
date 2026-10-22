param(
    [Parameter(Mandatory)][string]$HistogramsPath,
    [Parameter(Mandatory)][string]$ExpectedTablesPath
)
$ErrorActionPreference = 'Stop'
if ((Get-Item -LiteralPath $HistogramsPath).Length -ne 32768 -or
    (Get-Item -LiteralPath $ExpectedTablesPath).Length -gt 8768) { throw 'Bounded 32-case table batch required.' }
Add-Type -Path @((Join-Path $PSScriptRoot 'JpegColorParity.cs'),
    (Join-Path $PSScriptRoot 'JpegSubsampleParity.cs'),
    (Join-Path $PSScriptRoot 'JpegForwardParity.cs'),
    (Join-Path $PSScriptRoot 'JpegScanParity.cs'), (Join-Path $PSScriptRoot 'JpegReferenceTables.cs'))
function Hash([byte[]]$Bytes) { [Convert]::ToHexStringLower([Security.Cryptography.SHA256]::HashData($Bytes)) }
$histograms = [IO.File]::ReadAllBytes((Resolve-Path -LiteralPath $HistogramsPath).Path)
$histogramHash = Hash $histograms
$tables = [IO.File]::ReadAllBytes((Resolve-Path -LiteralPath $ExpectedTablesPath).Path)
$difference = [JpegReferenceTables]::FirstDifference($histograms, $tables)
if ($difference -ge 0) { throw "Exact Huffman comparison failed at histogram $difference." }
$tables[0] = $tables[0] -bxor 1
if ([JpegReferenceTables]::FirstDifference($histograms, $tables) -ne 0) { throw 'Changed table was not rejected.' }
$tables[0] = $tables[0] -bxor 1
if ((Hash $histograms) -ne $histogramHash) { throw 'Borrowed histograms changed.' }
[ordered]@{ cases=32; exact=$true; maximumLength=16; changedTableRejected=$true;
    histogramSha256=$histogramHash; tableSha256=(Hash $tables); inputUnchanged=$true } | ConvertTo-Json
