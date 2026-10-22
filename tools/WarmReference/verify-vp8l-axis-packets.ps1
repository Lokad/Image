param([Parameter(Mandatory)][string]$ModelAssembly,[Parameter(Mandatory)][string]$NativePacketDirectory)
$ErrorActionPreference='Stop'
[void][Reflection.Assembly]::LoadFrom((Resolve-Path $ModelAssembly).Path)
function Hash([byte[]]$b){[Convert]::ToHexStringLower([Security.Cryptography.SHA256]::HashData($b))}
$source=[byte[]]::new(16383*4)
for($i=0;$i -lt 16383;$i++){$source[4*$i]=$i -band 255;$source[4*$i+1]=($i -shr 8) -band 255;$source[4*$i+2]=($i*13+($i -shr 2)) -band 255;$source[4*$i+3]=255}
$sourceHash=Hash $source
if($sourceHash -ne '64162f37f54ef4b5d5bcdfedaad077a2d12bbcd02f27901bf42d1182aa829c67'){throw 'Axis source differs.'}
$rows=@()
foreach($shape in @(@(16383,1),@(1,16383))){
    $width=$shape[0];$height=$shape[1]
    $first=[Vp8lSpatialOnlyParity]::Encode($source,$width,$height)
    $second=[Vp8lSpatialOnlyParity]::Encode($source,$width,$height)
    $expected=[IO.File]::ReadAllBytes((Resolve-Path (Join-Path $NativePacketDirectory "e42-axis-${width}x${height}-native.webp")).Path)
    $expectedHash=if($width -eq 1){'f96d04d0bf7da594ca6818123589c4d2611ce0badc4685c10cdfc75a23f916bb'}else{'40964b3fd9868a571bb8c3a37994371b90a00b971a63c79df8521d4bec2a4d03'}
    if($expected.Length -ne 74 -or (Hash $expected) -ne $expectedHash){throw 'Frozen axis reference differs.'}
    $same=[Linq.Enumerable]::SequenceEqual[byte]($first.Packet,$expected)
    if(-not $same -or -not [Linq.Enumerable]::SequenceEqual[byte]($second.Packet,$expected)){
        throw "Axis ${width}x${height} differs: $($first.Packet.Length)B/$(Hash $first.Packet); $($first.Details|ConvertTo-Json -Depth 9 -Compress)"
    }
    if([Object]::ReferenceEquals($first.Packet,$second.Packet)){throw 'Axis output is shared.'}
    $second.Packet[20]=$second.Packet[20] -bxor 1
    if([Linq.Enumerable]::SequenceEqual[byte]($second.Packet,$expected) -or -not [Linq.Enumerable]::SequenceEqual[byte]($first.Packet,$expected) -or (Hash $source) -ne $sourceHash){throw 'Axis source/output mutation differs.'}
    $rows+=[ordered]@{width=$width;height=$height;sourceSha256=$sourceHash;rawExact=$true;packetBytes=$first.Packet.Length;packetSha256=(Hash $first.Packet);independentOutputs=$true;sourceUnchanged=$true;negativeMutationRejected=$true;details=$first.Details;productionChanged=$false;nativeEncodes=0;timingRuns=0;scope='Reference-compatible16383 companion of the existing16384 functional input; no native packet claim at16384.'}
}
$rows|ConvertTo-Json -Depth 11
