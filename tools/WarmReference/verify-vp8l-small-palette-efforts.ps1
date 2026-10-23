param([Parameter(Mandatory)][string]$ModelAssembly,[Parameter(Mandatory)][string]$NativeDirectory)
$ErrorActionPreference='Stop'
[void][Reflection.Assembly]::LoadFrom((Resolve-Path $ModelAssembly).Path)
Add-Type -Path (Join-Path $PSScriptRoot 'PalettePackingInput.cs')
function Hash([byte[]]$b){[Convert]::ToHexStringLower([Security.Cryptography.SHA256]::HashData($b))}
$source=[byte[]]::new(4103*2*4)
for($i=0;$i -lt $source.Length;$i+=4){$source[$i]=48;$source[$i+1]=211;$source[$i+2]=255};$source[$source.Length-4]=77
$sourceHash=Hash $source
if($sourceHash -ne '6429d23b373c1aaf3967399292008764e55453ea970f822366616d425795eecf'){throw 'Two-color source differs.'}
$comparison=[Vp8lPalettePacketParity]::CompareCandidates($source,4103,2)
$rows=@();$packetHashes='8f241022ea6585480f36d10a8d4c2cfefcc3e311ae6120b14867e34260aeb9bc','c14334d5aba4d17c193a0a0026d4a2b95bb2f9f8d7dc557b8fa15c66ad155dca'
foreach($i in 0,1){
    $coded=[IO.File]::ReadAllBytes((Resolve-Path (Join-Path $NativeDirectory "e51-box-${i}.coded")).Path)
    $pixels=[uint[]]::new($coded.Length/4);[Buffer]::BlockCopy($coded,0,$pixels,0,$coded.Length)
    $index=[Vp8lBoxMatchParity]::Build($pixels,513);$secondIndex=[Vp8lBoxMatchParity]::Build($pixels,513)
    $actual=[byte[]]::new($index.Length*4);[Buffer]::BlockCopy($index,0,$actual,0,$actual.Length)
    $expected=[IO.File]::ReadAllBytes((Resolve-Path (Join-Path $NativeDirectory "e51-box-${i}.matches")).Path)
    if((Hash $expected) -ne '036d7cf07d987fa2b59bb7e162f3d0a18141f1cd6ba2747843ec9a729295b6b7' -or
       -not [Linq.Enumerable]::SequenceEqual[byte]($actual,$expected) -or [Object]::ReferenceEquals($index,$secondIndex)){throw 'Box primitive index or ownership differs.'}
    $secondIndex[0]=$secondIndex[0] -bxor 1
    if([Linq.Enumerable]::SequenceEqual[uint]($index,$secondIndex)){throw 'Index mutation not rejected.'}
    # Observed coded samples enter only this index primitive test. Complete
    # candidate encoding below takes original source RGBA and no oracle stages.
    $first=[Vp8lPalettePacketParity]::EncodeBoxCandidate($source,4103,2,[bool]$i)
    $second=[Vp8lPalettePacketParity]::EncodeBoxCandidate($source,4103,2,[bool]$i)
    $expected=[IO.File]::ReadAllBytes((Resolve-Path (Join-Path $NativeDirectory "e51-box-${i}.webp")).Path)
    $ordinary=[IO.File]::ReadAllBytes((Resolve-Path (Join-Path $NativeDirectory "e51-ordinary-${i}.webp")).Path)
    $selected=if($i -eq 0){$comparison.Plain}else{$comparison.Spatial}
    if((Hash $expected) -ne $packetHashes[$i] -or
       -not [Linq.Enumerable]::SequenceEqual[byte]($first.Packet,$expected) -or
       -not [Linq.Enumerable]::SequenceEqual[byte]($second.Packet,$expected) -or
       -not [Linq.Enumerable]::SequenceEqual[byte]($selected.Packet,$ordinary) -or $selected.Details.box -or
       [Object]::ReferenceEquals($first.Packet,$second.Packet)){throw 'Source-derived box packet or first-candidate tie selection differs.'}
    $second.Packet[20]=$second.Packet[20] -bxor 1
    if([Linq.Enumerable]::SequenceEqual[byte]($second.Packet,$expected) -or
       -not [Linq.Enumerable]::SequenceEqual[byte]($first.Packet,$expected) -or (Hash $source) -ne $sourceHash){throw 'Packet/source mutation differs.'}
    $rows+=[ordered]@{spatial=[bool]$i;indexSamples=$index.Length;nonzeroIndexEntries=($index|Where-Object {$_ -ne 0}).Count;allIndexEntriesExact=$true;indexSha256=(Hash $actual);packetBytes=$first.Packet.Length;packetSha256=$packetHashes[$i];rawExact=$true;ordinaryBoxTieKeepsOrdinary=$true;encoderOracleStageInputs=$false;independentOutputs=$true;sourceUnchanged=$true;negativeMutationRejected=$true;details=$first.Details}
}
$numericSource=[PalettePackingInput]::Create(23,13,18);$numericHash=Hash $numericSource
if($numericHash -ne 'd09cae7c789257e4d740adf2f43a18517e503c9c84748f2b8e662ce857436325'){throw 'Numeric palette source differs.'}
$first=[Vp8lPalettePacketParity]::Encode($numericSource,23,13,0)
$second=[Vp8lPalettePacketParity]::Encode($numericSource,23,13,0)
$expected=[IO.File]::ReadAllBytes((Resolve-Path (Join-Path $NativeDirectory 'e51-palette18-method0-native.webp')).Path)
if((Hash $expected) -ne '634ad1b52a2b14326ac2e3336e9e5cfb4848d2b94b24ca305b23df26211ed315' -or
   -not [Linq.Enumerable]::SequenceEqual[byte]($first.Packet,$expected) -or
   -not [Linq.Enumerable]::SequenceEqual[byte]($second.Packet,$expected) -or [Object]::ReferenceEquals($first.Packet,$second.Packet)){throw 'Numeric palette packet differs.'}
$second.Packet[20]=$second.Packet[20] -bxor 1
if([Linq.Enumerable]::SequenceEqual[byte]($second.Packet,$expected) -or
   -not [Linq.Enumerable]::SequenceEqual[byte]($first.Packet,$expected) -or (Hash $numericSource) -ne $numericHash){throw 'Numeric palette mutation/source differs.'}
[ordered]@{sourceOnly=$true;boxSourceSha256=$sourceHash;boxCases=$rows;numericSourceSha256=$numericHash;numericPacketBytes=$first.Packet.Length;numericPacketSha256=(Hash $first.Packet);numericRawExact=$true;independentOutputs=$true;sourceUnchanged=$true;negativeMutationRejected=$true;productionChanged=$false;timingRuns=0;scope='Existing two-color4103x2 and18-color23x13 sources, method5 short-distance box candidates and method0 numeric palette with implicit zero. No new fixture or settings grid; observed coded input is used only for an isolated index primitive test.'}|ConvertTo-Json -Depth 12
