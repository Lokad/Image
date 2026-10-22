param(
    [Parameter(Mandatory)][string]$SourcePath,
    [Parameter(Mandatory)][string]$NativePacketPath,
    [string]$AssemblyPath
)
$ErrorActionPreference='Stop'
function Hash([byte[]]$Bytes) { [Convert]::ToHexStringLower([Security.Cryptography.SHA256]::HashData($Bytes)) }
function Equal([byte[]]$Left,[byte[]]$Right) { [Linq.Enumerable]::SequenceEqual[byte]($Left,$Right) }
$source=[IO.File]::ReadAllBytes((Resolve-Path -LiteralPath $SourcePath).Path)
$sourceHash='70f0fb18c341e2740b875a5a34e21ca63f0821b3386352fe2971020c5a6aac80'
if($source.Length -ne 531468 -or (Hash $source) -ne $sourceHash){throw 'Frozen source differs.'}
if($AssemblyPath) {
    $assemblyFile=(Resolve-Path -LiteralPath $AssemblyPath).Path
    [void][Reflection.Assembly]::LoadFrom((Join-Path ([IO.Path]::GetDirectoryName($assemblyFile)) 'Probe.dll'))
    [void][Reflection.Assembly]::LoadFrom($assemblyFile)
} else {
    $names='Vp8lPredictorParity','Vp8lCrossColorParity','Vp8lMatchParity','Vp8lLzParity','Vp8lPrefixParity','Vp8lBitParity','Vp8lAuxiliaryParity'
    Add-Type -Path @($names | ForEach-Object { Join-Path $PSScriptRoot "${_}.cs" })
}
# The complete model finishes before the comparison packet is read.
$first=[Vp8lAuxiliaryParity]::Build($source)
$next=[Vp8lAuxiliaryParity]::Build($source)
$packet=[IO.File]::ReadAllBytes((Resolve-Path -LiteralPath $NativePacketPath).Path)
if($packet.Length -ne 183812 -or (Hash $packet) -ne '08ddac115f1770d778e6dbc76373440224b32e6f93b616605e234c63206eebde'){throw 'Frozen reference packet differs.'}
$expected=[byte[]]::new(751)
[Array]::Copy($packet,20,$expected,0,751)
$expected[750]=$expected[750] -band 63 # Exactly6006 payload bits; zero padding is not compared.
foreach($result in @($first,$next)) {
    if($result.BitCount -ne 6006 -or -not(Equal $result.Bytes $expected) -or
       $result.Predictor.BitCount -ne 2492 -or $result.Predictor.TokenCount -ne 337 -or $result.Predictor.TokenStart -ne 267 -or
       $result.Color.BitCount -ne 3459 -or $result.Color.TokenCount -ne 713 -or $result.Color.TokenStart -ne 228 -or
       (Hash $result.Modes) -ne 'bdad28ced56613fe6dc21387f5f10cac85e43ec37c785d77ac80694426c763b7' -or
       (Hash $result.Residual) -ne '9a29c48cef0fd5f0de20121fa07b1807c41366ef3e3c9d7cf9937dc4b8558f0a' -or
       (Hash $result.ColorMap) -ne '870a05bb67a28441242246a7235844eafae6236571ee264c25f5a84d535420eb' -or
       (Hash $result.CodedResidual) -ne 'b72edfa146d02b4adbbe48a667ad28bf10b6073b2800c7679ccfc546c9969610') {throw 'Source-derived prefix or residual stage differs.'}
}
if((Hash $source) -ne $sourceHash){throw 'Source changed.'}
$properties='Bytes','Modes','Residual','ColorMap','CodedResidual'
$survivingHashes=@{}
foreach($name in $properties){$survivingHashes[$name]=Hash $first.$name}
foreach($name in $properties) {
    if([Object]::ReferenceEquals($first.$name,$next.$name)){throw "Aliased output: $name"}
    $next.$name[0]=$next.$name[0] -bxor 1
    if(Equal $first.$name $next.$name){throw "Mutation accepted: $name"}
}
foreach($name in $properties){if((Hash $first.$name) -ne $survivingHashes[$name]){throw "Surviving output changed: $name"}}
[ordered]@{
    sourceOnly=$true;sourceSha256=$sourceHash;prefixBits=6006;allPrefixBitsExact=$true
    prefixSha256=(Hash $first.Bytes);crossColorMapEntries=2145;codedResidualBytes=531468
    predictor=[ordered]@{bits=2492;tokens=337;firstDataBit=316;selection=$first.Predictor.Selection;indexSha256=$first.Predictor.MatchIndexSha256}
    color=[ordered]@{bits=3459;tokens=713;firstDataBit=2775;selection=$first.Color.Selection;indexSha256=$first.Color.MatchIndexSha256}
    independentOutputs=$true;sourceUnchanged=$true;negativeMutationsRejected=$true
    oracleEncoderInputs=$false;productionChanged=$false;productionBuilds=0;nativeEncodes=0;timingRuns=0
    scope='ONE fixed513x259 method4/exact=1 VP8L control. Complete transform prefix; main image and complete packet remain open. No production admission or performance claim.'
} | ConvertTo-Json -Depth 6
