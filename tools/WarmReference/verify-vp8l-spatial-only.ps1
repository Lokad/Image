param(
    [Parameter(Mandatory)][string]$NativePacketPath,
    [Parameter(Mandatory)][string]$ModelAssembly
)
$ErrorActionPreference='Stop'
[void][Reflection.Assembly]::LoadFrom((Resolve-Path $ModelAssembly).Path)
function Hash([byte[]]$bytes){[Convert]::ToHexStringLower([Security.Cryptography.SHA256]::HashData($bytes))}
$source=[byte[]]::new(19*7*4)
for($y=0;$y -lt 7;$y++){for($x=0;$x -lt 19;$x++){
    $i=4*($y*19+$x);$source[$i]=$x*13;$source[$i+1]=$y*31;$source[$i+2]=$x+$y
    $source[$i+3]=if($x%3 -eq 0){0}else{$x*11}
}}
$sourceHash=Hash $source
if($sourceHash -ne 'c4c881cd7125a4f91eb16d548728801b3cfacf1c1f8ede5933f514260eb8eb96'){throw 'Frozen source differs.'}
$first=[Vp8lSpatialOnlyParity]::Encode($source,19,7)
$second=[Vp8lSpatialOnlyParity]::Encode($source,19,7)
$expected=[IO.File]::ReadAllBytes((Resolve-Path $NativePacketPath).Path)
$expectedHash=Hash $expected
if($expected.Length -ne 82 -or $expectedHash -ne 'c432b85f60f9817358985ffad69ae307d33926f02de3fb80abe8645b5f526c58'){throw 'Frozen reference differs.'}
if(-not [Linq.Enumerable]::SequenceEqual[byte]($first.Packet,$expected) -or
   -not [Linq.Enumerable]::SequenceEqual[byte]($second.Packet,$expected)){
    throw "Packet differs: $($first.Packet.Length)B/$(Hash $first.Packet); residual $(Hash $first.Residual); modes $($first.Modes); $($first.Details|ConvertTo-Json -Depth 8 -Compress)"
}
foreach($name in 'Packet','Modes','ColorMap','Residual','Coded'){
    if([Object]::ReferenceEquals($first.$name,$second.$name)){throw "Shared output: $name"}
}
if($first.Modes.Length -ne 1 -or $first.Modes[0] -ne 2 -or
   -not [Linq.Enumerable]::SequenceEqual[byte]($first.ColorMap,[byte[]]@(0,0,0,255)) -or
   (Hash $first.Residual) -ne '7416018eeb4d196de281306b2952d9cae631dd8148e419555a311e44d6bcd106' -or
   (Hash $first.Coded) -ne '7416018eeb4d196de281306b2952d9cae631dd8148e419555a311e44d6bcd106'){throw 'Intermediate stage differs.'}
$second.Packet[20]=$second.Packet[20] -bxor 1;$second.Modes[0]=0;$second.ColorMap[0]=1;$second.Residual[0]=$second.Residual[0] -bxor 1;$second.Coded[0]=$second.Coded[0] -bxor 1
if([Linq.Enumerable]::SequenceEqual[byte]($second.Packet,$expected) -or
   -not [Linq.Enumerable]::SequenceEqual[byte]($first.Packet,$expected) -or
   $first.Modes[0] -ne 2 -or $first.ColorMap[0] -ne 0 -or
   (Hash $first.Residual) -ne '7416018eeb4d196de281306b2952d9cae631dd8148e419555a311e44d6bcd106' -or
   (Hash $first.Coded) -ne '7416018eeb4d196de281306b2952d9cae631dd8148e419555a311e44d6bcd106' -or
   (Hash $source) -ne $sourceHash){throw 'Mutation or source/output preservation differs.'}
[ordered]@{
    existingPublicCase='WebpTests.LosslessEncodePreservesAlphaAndHiddenRgbOnNonseekableOutput'
    width=19;height=7;sourceSha256=$sourceHash;packetBytes=82;packetSha256=$expectedHash
    rawExact=$true;metadataExemptions=0;intermediateMapsAndResidualsExact=$true;independentOutputs=$true
    sourceUnchanged=$true;negativeMutationsRejected=$true;goldenPacketReadAfterModel=$true;details=$first.Details
    productionChanged=$false;nativeBuilds=0;nativeEncodes=0;timingRuns=0
    scope='Existing19x7 spatial-only input with uniform predictor/color maps. Other map/sizing paths and admitted production remain open.'
}|ConvertTo-Json -Depth 10
