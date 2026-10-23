param([Parameter(Mandatory)][string]$ModelAssembly,[Parameter(Mandatory)][string]$NativeCandidateDirectory)
$ErrorActionPreference='Stop'
[void][Reflection.Assembly]::LoadFrom((Resolve-Path $ModelAssembly).Path)
Add-Type -Path (Join-Path $PSScriptRoot 'PalettePackingInput.cs')
function Hash([byte[]]$bytes){[Convert]::ToHexStringLower([Security.Cryptography.SHA256]::HashData($bytes))}
$source=[PalettePackingInput]::Create(23,13,18)
$sourceHash=Hash $source
if($sourceHash -ne 'd09cae7c789257e4d740adf2f43a18517e503c9c84748f2b8e662ce857436325'){throw 'Original input differs.'}
$first=[Vp8lPalettePacketParity]::CompareCandidates($source,23,13)
$second=[Vp8lPalettePacketParity]::CompareCandidates($source,23,13)
$rows=@();$hashes='a4bb56684758d4ff4fa58da15729a126098a74b228c54307204c95d7e4942736','2fbb3167c8da95e86dbec67aa0b5c853f4ed651e48bcdc8ebb70ce34cb75ac82'
foreach($i in 0,1){
    $result=if($i -eq 0){$first.Plain}else{$first.Spatial}
    $repeat=if($i -eq 0){$second.Plain}else{$second.Spatial}
    $expected=[IO.File]::ReadAllBytes((Resolve-Path (Join-Path $NativeCandidateDirectory "e50-palette-candidate-${i}.webp")).Path)
    if((Hash $expected) -ne $hashes[$i] -or
       -not [Linq.Enumerable]::SequenceEqual[byte]($result.Packet,$expected) -or
       -not [Linq.Enumerable]::SequenceEqual[byte]($repeat.Packet,$expected) -or
       [Object]::ReferenceEquals($result.Packet,$repeat.Packet)){throw 'Native candidate or ownership differs.'}
    $repeat.Packet[20]=$repeat.Packet[20] -bxor 1
    if([Linq.Enumerable]::SequenceEqual[byte]($repeat.Packet,$expected) -or
       -not [Linq.Enumerable]::SequenceEqual[byte]($result.Packet,$expected) -or (Hash $source) -ne $sourceHash){throw 'Mutation/source preservation differs.'}
    $rows+=[ordered]@{candidate=$i;packetBytes=$result.Packet.Length;packetSha256=$hashes[$i];rawExact=$true;independentOutputs=$true;sourceUnchanged=$true;negativeMutationRejected=$true;details=$result.Details}
}
$selected=[Vp8lPalettePacketParity]::Encode($source,23,13,5)
if((Hash $selected.Packet) -ne $hashes[0] -or (Hash $first.Selected.Packet) -ne $hashes[0] -or [Object]::ReferenceEquals($selected.Packet,$first.Selected.Packet)){throw 'Complete writer selection differs.'}
[ordered]@{sourceOnly=$true;sourceSha256=$sourceHash;cases=$rows;selectedPacketBytes=$selected.Packet.Length;selectedPacketSha256=$hashes[0];selectedRawExact=$true;productionChanged=$false;timingRuns=0;scope='Existing18-color23x13 method5/quality75/exact1 control: both forced transform-candidate packets and final selection. Public reference selects plain; no claim of a spatial-winning control or general effort-family qualification.'}|ConvertTo-Json -Depth 12
