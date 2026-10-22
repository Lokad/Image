param([Parameter(Mandatory)][string]$ModelAssembly,[Parameter(Mandatory)][string]$NativeEvidencePath)
$ErrorActionPreference='Stop'
[void][Reflection.Assembly]::LoadFrom((Resolve-Path $ModelAssembly).Path)
function Hash([byte[]]$b){[Convert]::ToHexStringLower([Security.Cryptography.SHA256]::HashData($b))}
$native=Get-Content -Raw $NativeEvidencePath|ConvertFrom-Json
if($native.cases.Count -ne 7){throw 'Expected the single input at methods0..6.'}
$source=[byte[]]::new(4103*2*4)
for($i=0;$i -lt $source.Length;$i+=4){$source[$i]=48;$source[$i+1]=211;$source[$i+2]=255}
$source[$source.Length-4]=77
$sourceHash=Hash $source
if($sourceHash -ne '6429d23b373c1aaf3967399292008764e55453ea970f822366616d425795eecf'){throw 'Frozen source differs.'}
$rows=@()
for($method=0;$method -le 6;$method++){
    $case=$native.cases[$method]
    $targetHash=if($method -eq 0){'ffaf55b38823d030ac8ee2988ddcf34da5a195d6a0f3043506262d68915c8c64'}else{'8f241022ea6585480f36d10a8d4c2cfefcc3e311ae6120b14867e34260aeb9bc'}
    if($case.method -ne $method -or $case.quality -ne 75 -or -not $case.exactRgb -or $case.sourceSha256 -ne $sourceHash -or $case.packetSha256 -ne $targetHash){throw 'Native settings/identity differ.'}
    $first=[Vp8lPalettePacketParity]::Encode($source,4103,2,$method)
    $second=[Vp8lPalettePacketParity]::Encode($source,4103,2,$method)
    $expected=[Convert]::FromHexString($case.packetHex)
    if((Hash $expected) -ne $targetHash -or
       -not [Linq.Enumerable]::SequenceEqual[byte]($first.Packet,$expected) -or
       -not [Linq.Enumerable]::SequenceEqual[byte]($second.Packet,$expected) -or
       [Object]::ReferenceEquals($first.Packet,$second.Packet)){
        throw "Method $method differs: $($first.Packet.Length)B/$(Hash $first.Packet); $($first.Details|ConvertTo-Json -Depth 8 -Compress)"
    }
    $second.Packet[20]=$second.Packet[20] -bxor 1
    if([Linq.Enumerable]::SequenceEqual[byte]($second.Packet,$expected) -or
       -not [Linq.Enumerable]::SequenceEqual[byte]($first.Packet,$expected) -or (Hash $source) -ne $sourceHash){throw 'Mutation or source/output preservation differs.'}
    $rows+=[ordered]@{method=$method;packetBytes=$first.Packet.Length;packetSha256=$targetHash;rawExact=$true;independentOutputs=$true;sourceUnchanged=$true;negativeMutationRejected=$true;details=$first.Details}
}
[ordered]@{sourceOnly=$true;existingPublicCase='WebpLosslessAdmissionTests.TwoPassEncoderAdmitsSourceAndScratchAndChecksSizeBeforeWriting';sourceSha256=$sourceHash;cases=$rows;productionChanged=$false;nativeEncodes=0;timingRuns=0;scope='Existing two-color4103x2 input only at methods0..6/quality75/exact1; no general effort, production admission or public-depth qualification.'}|ConvertTo-Json -Depth 11
