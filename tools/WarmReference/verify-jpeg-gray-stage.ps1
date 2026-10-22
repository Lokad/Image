param(
    [Parameter(Mandatory)][string]$SourceGrayPath,
    [Parameter(Mandatory)][string]$NativeDctPath,
    [Parameter(Mandatory)][string]$NativeCoefficientPath,
    [Parameter(Mandatory)][string]$NativePacketPath,
    [Parameter(Mandatory)][string]$HistogramsPath,
    [Parameter(Mandatory)][string]$NativeTablesPath,
    [string]$OutputPath
)
$ErrorActionPreference = 'Stop'
$pin = (Get-Content -Raw (Join-Path $PSScriptRoot '../../benchmarks/jpeg-byte-parity-evidence.json') | ConvertFrom-Json).grayscaleReplication
function Read([string]$Path) { return ,([IO.File]::ReadAllBytes((Resolve-Path -LiteralPath $Path).Path)) }
function Hash([byte[]]$Bytes) { [Convert]::ToHexStringLower([Security.Cryptography.SHA256]::HashData($Bytes)) }
$source = Read $SourceGrayPath; $dct = Read $NativeDctPath; $coefficients = Read $NativeCoefficientPath
$packet = Read $NativePacketPath; $histograms = Read $HistogramsPath; $tables = Read $NativeTablesPath
if ($source.Length -ne 132867 -or $dct.Length -ne 274560 -or $coefficients.Length -ne 274728 -or
    $packet.Length -ne 58910 -or $histograms.Length -ne 34816 -or $tables.Length -gt 9316 -or
    (Hash $source) -ne $pin.sourceGraySha256 -or (Hash $dct) -ne $pin.nativeDctSha256 -or
    (Hash $coefficients) -ne $pin.nativeCoefficientDumpSha256 -or (Hash $packet) -ne $pin.nativePacketSha256 -or
    (Hash $histograms) -ne $pin.nativeHuffmanHistogramsSha256 -or (Hash $tables) -ne $pin.nativeHuffmanTablesSha256) {
    throw 'Frozen grayscale source, stages, packet or table batch differs.'
}
Add-Type -Path @((Join-Path $PSScriptRoot 'JpegColorParity.cs'),
    (Join-Path $PSScriptRoot 'JpegSubsampleParity.cs'), (Join-Path $PSScriptRoot 'JpegForwardParity.cs'),
    (Join-Path $PSScriptRoot 'JpegScanParity.cs'), (Join-Path $PSScriptRoot 'JpegReferenceTables.cs'),
    (Join-Path $PSScriptRoot 'JpegGrayParity.cs'), (Join-Path $PSScriptRoot 'JpegGrayReferenceTables.cs'),
    (Join-Path $PSScriptRoot 'JpegGrayPacketParity.cs'))
$actualDct = [JpegGrayParity]::TransformRaster($source, $false)
$actualCoefficients = [JpegGrayParity]::TransformRaster($source, $true)
if (-not [JpegColorParity]::Equal($actualDct, $dct) -or
    -not [JpegColorParity]::Equal($actualCoefficients, [byte[]]$coefficients[168..($coefficients.Length - 1)])) {
    throw 'Independent raw DCT or quantized coefficients differ.'
}
$cursor = 0
for ($row = 0; $row -lt 34; $row++) {
    $frequencies = [int[]]::new(256)
    for ($i = 0; $i -lt 256; $i++) { $frequencies[$i] = [BitConverter]::ToInt32($histograms, $row * 1024 + $i * 4) }
    $generated = [JpegGrayReferenceTables]::Huffman($frequencies)
    $count = [int]$tables[$cursor + 16] + ([int]$tables[$cursor + 17] -shl 8)
    if ($count -ne $generated.Symbols.Length -or
        -not [JpegColorParity]::Equal($generated.Counts, [byte[]]$tables[$cursor..($cursor + 15)]) -or
        -not [JpegColorParity]::Equal($generated.Symbols, [byte[]]$tables[($cursor + 18)..($cursor + 17 + $count)])) {
        throw "Native grayscale table differs at case $row."
    }
    $cursor += 18 + $count
}
if ($cursor -ne $tables.Length) { throw 'Trailing table data.' }
$first = [JpegGrayPacketParity]::Encode($source)
$next = [JpegGrayPacketParity]::Encode($source)
if (-not [JpegColorParity]::Equal($first.Packet, $packet) -or
    -not [JpegColorParity]::Equal($next.Packet, $packet) -or
    [Object]::ReferenceEquals($first.Packet, $next.Packet) -or
    (Hash $source) -ne $pin.sourceGraySha256 -or (Hash $histograms) -ne $pin.nativeHuffmanHistogramsSha256) {
    throw 'Complete source-only packet, input preservation or independent output ownership differs.'
}
foreach ($family in @(0, 1)) {
    for ($i = 0; $i -lt 256; $i++) {
        $index = if ($family -eq 0) { 0 } else { 2 }
        if ($first.Histograms[$index][$i] -ne [BitConverter]::ToInt32($histograms, $family * 1024 + $i * 4)) {
            throw 'Generated complete-operation histogram differs.'
        }
    }
}
if ($OutputPath) { [IO.File]::WriteAllBytes($OutputPath, $first.Packet) }
[ordered]@{
    sourceOnly=$true; sourceSha256=(Hash $source); sourceUnchanged=$true;
    exactRawDctValues=137280; rawDctSha256=(Hash $actualDct);
    exactQuantizedValues=137280; quantizedSha256=(Hash $actualCoefficients);
    exactHuffmanCases=34; exactCompletePacket=$true; packetBytes=$first.Packet.Length;
    packetSha256=(Hash $first.Packet); histogramsExact=$true; independentLiveOutputs=$true;
    survivingOutputUnchanged=$true; oraclePacketOrTableInputs=$false; attributionExceptionUsed=$false;
    productionBuilds=0; timingRuns=0
} | ConvertTo-Json -Depth 4
