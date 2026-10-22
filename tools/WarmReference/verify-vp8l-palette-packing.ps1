param([Parameter(Mandatory)][string]$ModelAssembly,[Parameter(Mandatory)][string]$NativeEvidencePath)
$ErrorActionPreference='Stop'
[void][Reflection.Assembly]::LoadFrom((Resolve-Path $ModelAssembly).Path)
Add-Type -Path (Join-Path $PSScriptRoot 'PalettePackingInput.cs')
function Hash([byte[]]$b){[Convert]::ToHexStringLower([Security.Cryptography.SHA256]::HashData($b))}
$native=Get-Content -Raw $NativeEvidencePath|ConvertFrom-Json
$names='Palette4ColorPartialPacking','Palette16ColorPartialPacking','Palette18ColorUnpackedZero'
$sources='4d1445a674a8bb595abb9087529668f16b5e6354681099d351be847543536e11','185909f068ddee0655967de50590853d679ac05bfa13acb9bbd6ed29c05e07da','d09cae7c789257e4d740adf2f43a18517e503c9c84748f2b8e662ce857436325'
$packets='5b7387a2bc7bbf253f6488dc6d10b26a395eecb5835819e718c8f043ed268ab7','b24691e94f5e5369d84f8de9074c436ca470f6ca9a3a9a7389c1256972cb1d33','a4bb56684758d4ff4fa58da15729a126098a74b228c54307204c95d7e4942736'
if($native.method -ne 4 -or $native.quality -ne 75 -or -not $native.exactRgb -or $native.cases.Count -ne 3){throw 'Frozen native configuration differs.'}
$rows=@()
for($at=0;$at -lt 3;$at++){
    $case=$native.cases[$at]
    if($case.name -ne $names[$at] -or $case.sourceSha256 -ne $sources[$at] -or $case.packetSha256 -ne $packets[$at]){throw 'Frozen palette case identity differs.'}
    $source=[PalettePackingInput]::Create($case.width,$case.height,$case.colors)
    if((Hash $source) -ne $sources[$at]){throw 'Original cross-language fixture differs.'}
    $palette=[Vp8lPaletteParity]::Build($source,$case.width,$case.height)
    $first=[Vp8lPalettePacketParity]::Encode($source,$case.width,$case.height)
    $second=[Vp8lPalettePacketParity]::Encode($source,$case.width,$case.height)
    $expected=[Convert]::FromHexString($case.packetHex)
    $packed=[byte[]]::new($palette.Packed.Length*4);[Buffer]::BlockCopy($palette.Packed,0,$packed,0,$packed.Length)
    if((Hash $expected) -ne $packets[$at] -or (Hash $packed) -ne $case.packedSha256 -or
       -not [Linq.Enumerable]::SequenceEqual[uint32]($palette.Colors,[uint32[]]$case.paletteArgb) -or
       -not [Linq.Enumerable]::SequenceEqual[byte]($first.Packet,$expected) -or
       -not [Linq.Enumerable]::SequenceEqual[byte]($second.Packet,$expected) -or
       [Object]::ReferenceEquals($first.Packet,$second.Packet)){throw 'Palette ordering, packing, packet or independent output differs.'}
    $second.Packet[20]=$second.Packet[20] -bxor 1
    if([Linq.Enumerable]::SequenceEqual[byte]($second.Packet,$expected) -or
       -not [Linq.Enumerable]::SequenceEqual[byte]($first.Packet,$expected) -or (Hash $source) -ne $sources[$at]){throw 'Mutation/source preservation differs.'}
    $rows+=[ordered]@{name=$case.name;width=$case.width;height=$case.height;sourceColors=$case.colors;encodedPaletteEntries=$case.encodedPaletteEntries;sourceSha256=$sources[$at];packetBytes=$first.Packet.Length;packetSha256=$packets[$at];rawExact=$true;paletteOrderExact=$true;packedPixelsExact=$true;independentOutputs=$true;sourceUnchanged=$true;negativeMutationRejected=$true;details=$first.Details}
}
[ordered]@{sourceOnly=$true;cases=$rows;productionChanged=$false;nativeEncodes=0;timingRuns=0;scope='Three named4/16/18-color method4 controls only; other algorithms/settings, admitted production and public boundaries remain open.'}|ConvertTo-Json -Depth 11
