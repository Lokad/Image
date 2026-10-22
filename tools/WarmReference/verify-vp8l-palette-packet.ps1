param(
    [Parameter(Mandatory)][string]$NativePacketPath,
    [Parameter(Mandatory)][string]$ModelAssembly
)
$ErrorActionPreference='Stop'
[void][Reflection.Assembly]::LoadFrom((Resolve-Path $ModelAssembly).Path)
function Hash([byte[]]$bytes){[Convert]::ToHexStringLower([Security.Cryptography.SHA256]::HashData($bytes))}
$source=[byte[]]::new(4103*2*4)
for($i=0;$i -lt $source.Length;$i+=4){$source[$i]=48;$source[$i+1]=211;$source[$i+2]=255}
$source[$source.Length-4]=77
$sourceHash=Hash $source
if($sourceHash -ne '6429d23b373c1aaf3967399292008764e55453ea970f822366616d425795eecf'){throw 'Source identity differs.'}
$expected=[IO.File]::ReadAllBytes((Resolve-Path $NativePacketPath).Path)
$expectedHash=Hash $expected
if($expected.Length -ne 46 -or $expectedHash -ne '8f241022ea6585480f36d10a8d4c2cfefcc3e311ae6120b14867e34260aeb9bc'){throw 'Reference identity differs.'}
$palette=[Vp8lPaletteParity]::Build($source,4103,2)
$first=[Vp8lPalettePacketParity]::Encode($source,4103,2)
$second=[Vp8lPalettePacketParity]::Encode($source,4103,2)
if(-not [Linq.Enumerable]::SequenceEqual[byte]($first.Packet,$expected) -or
   -not [Linq.Enumerable]::SequenceEqual[byte]($second.Packet,$expected) -or
   [Object]::ReferenceEquals($first.Packet,$second.Packet)){throw 'Complete packet/independent output differs.'}
$packed=[byte[]]::new($palette.Packed.Length*4)
[Buffer]::BlockCopy($palette.Packed,0,$packed,0,$packed.Length)
if(-not [Linq.Enumerable]::SequenceEqual[uint32]($palette.Colors,[uint32[]]@(3199999,5100543)) -or
   -not [Linq.Enumerable]::SequenceEqual[uint32]($palette.Deltas,[uint32[]]@(3199999,1900544)) -or
   (Hash $packed) -ne 'b575d8901a7aa4aabfb48c8505224f1fa438c8a2d990eb74fd3b1c5c1b1f13a2'){throw 'Palette/delta/packed source stage differs.'}
$second.Packet[20]=$second.Packet[20] -bxor 1
if([Linq.Enumerable]::SequenceEqual[byte]($second.Packet,$expected) -or
   -not [Linq.Enumerable]::SequenceEqual[byte]($first.Packet,$expected) -or (Hash $source) -ne $sourceHash){throw 'Source/output ownership or negative mutation check differs.'}
[ordered]@{
    existingPublicCase='WebpLosslessAdmissionTests.TwoPassEncoderAdmitsSourceAndScratchAndChecksSizeBeforeWriting'
    width=4103;height=2;sourceSha256=$sourceHash;paletteEntries=2;packedSamples=$palette.Packed.Length
    packingBits=$palette.PackingBits;codedWidth=$palette.CodedWidth;packedSha256=(Hash $packed)
    rawExact=$true;packetBytes=46;packetSha256=$expectedHash;independentOutputs=$true
    sourceUnchanged=$true;negativeMutationRejected=$true;details=$first.Details
    productionChanged=$false;nativeBuilds=0;nativeEncodes=0;timingRuns=0
    scope='Source-only complete packet for the existing two-color input. Other palette orders/sizes, production admission and complete public-operation qualification remain open.'
}|ConvertTo-Json -Depth 8
