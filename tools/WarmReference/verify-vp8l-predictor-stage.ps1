param(
    [Parameter(Mandatory)][string]$SourcePath,
    [Parameter(Mandatory)][string]$NativeModesPath,
    [Parameter(Mandatory)][string]$NativeResidualGzipPath,
    [string]$AssemblyPath
)
$ErrorActionPreference='Stop'
function Hash([byte[]]$Bytes) { [Convert]::ToHexStringLower([Security.Cryptography.SHA256]::HashData($Bytes)) }
function Equal([byte[]]$Left,[byte[]]$Right) { [Linq.Enumerable]::SequenceEqual[byte]($Left,$Right) }
$source=[IO.File]::ReadAllBytes((Resolve-Path -LiteralPath $SourcePath).Path)
$sourceHash='70f0fb18c341e2740b875a5a34e21ca63f0821b3386352fe2971020c5a6aac80'
if($source.Length -ne 531468 -or (Hash $source) -ne $sourceHash){throw 'Frozen513x259 RGBA8 source differs.'}
if($AssemblyPath) { [void][Reflection.Assembly]::LoadFrom((Resolve-Path -LiteralPath $AssemblyPath).Path) }
else { Add-Type -Path (Join-Path $PSScriptRoot 'Vp8lPredictorParity.cs') }
# Only source samples and the declared8x8 stage layout enter the model.
$first=[Vp8lPredictorParity]::EncodeModes($source,513,259,3)
$residual=[Vp8lPredictorParity]::Residual($source,513,259,3,$first)
$next=[Vp8lPredictorParity]::EncodeModes($source,513,259,3)
$nextResidual=[Vp8lPredictorParity]::Residual($source,513,259,3,$next)
$argb=[IO.File]::ReadAllBytes((Resolve-Path -LiteralPath $NativeModesPath).Path)
if($argb.Length -ne 8580 -or (Hash $argb) -ne '6835fd3eccbb60127357c8a6b671eb7068677bfea9f34b22d5b4526fde6c3294'){throw 'Frozen native predictor map differs.'}
$expected=[byte[]]::new(2145)
for($i=0;$i -lt $expected.Length;$i++){$expected[$i]=$argb[$i*4+1]}
$residualInput=[IO.File]::OpenRead((Resolve-Path -LiteralPath $NativeResidualGzipPath).Path)
try {
    $gzip=[IO.Compression.GZipStream]::new($residualInput,[IO.Compression.CompressionMode]::Decompress,$true)
    try {
        $expectedResidual=[byte[]]::new(531468)
        $gzip.ReadExactly($expectedResidual)
        if($gzip.ReadByte() -ne -1){throw 'Native residual length exceeds the frozen control.'}
    } finally {$gzip.Dispose()}
} finally {$residualInput.Dispose()}
if((Hash $expectedResidual) -ne '9a29c48cef0fd5f0de20121fa07b1807c41366ef3e3c9d7cf9937dc4b8558f0a' -or
   -not(Equal $first $expected) -or -not(Equal $next $expected) -or
   -not(Equal $residual $expectedResidual) -or -not(Equal $nextResidual $expectedResidual) -or
   [Object]::ReferenceEquals($first,$next) -or [Object]::ReferenceEquals($residual,$nextResidual) -or
   (Hash $source) -ne $sourceHash){throw 'Source-derived modes/residuals, source or independent outputs differ.'}
$next[0]=$next[0] -bxor 1;$nextResidual[0]=$nextResidual[0] -bxor 1
if((Equal $next $expected) -or (Equal $nextResidual $expectedResidual) -or
   -not(Equal $first $expected) -or -not(Equal $residual $expectedResidual)){throw 'Changed samples accepted or surviving outputs changed.'}
[ordered]@{
    sourceOnly=$true;sourceSha256=(Hash $source);width=513;height=259;tileBits=3
    modeSamples=2145;modeSha256=(Hash $first);allModesExact=$true
    residualBytes=$residual.Length;residualSha256=(Hash $residual);allResidualBytesExact=$true
    independentOutputs=$true;sourceUnchanged=$true;negativeMutationsRejected=$true
    oracleEncoderInputs=$false;productionBuilds=0;nativeEncodes=0;timingRuns=0
    scope='One fixed method4 VP8L stage; no complete encoder or automatic tile-size qualification.'
} | ConvertTo-Json -Depth 3
