param([Parameter(Mandatory)][string]$NativePacketPath,[Parameter(Mandatory)][string]$ModelAssembly)
$ErrorActionPreference='Stop'
[void][Reflection.Assembly]::LoadFrom((Resolve-Path $ModelAssembly).Path)
$source=[byte[]]@(255,0,1,0)
$first=[Vp8lPalettePacketParity]::Encode($source,1,1).Packet
$second=[Vp8lPalettePacketParity]::Encode($source,1,1).Packet
$expected=[IO.File]::ReadAllBytes((Resolve-Path -LiteralPath $NativePacketPath).Path)
$hash=[Convert]::ToHexStringLower([Security.Cryptography.SHA256]::HashData($expected))
if($expected.Length -ne 34 -or $hash -ne '98dc19987970964d8c3a2bd9e57c9e4cd4e29dc1e99fe8658b087f53aabcb618' -or
   -not [Linq.Enumerable]::SequenceEqual[byte]($first,$expected) -or
   -not [Linq.Enumerable]::SequenceEqual[byte]($second,$expected) -or [Object]::ReferenceEquals($first,$second)){throw 'Singleton palette packet differs.'}
$second[20]=$second[20] -bxor 1
if([Linq.Enumerable]::SequenceEqual[byte]($second,$expected) -or
   -not [Linq.Enumerable]::SequenceEqual[byte]($first,$expected) -or
   -not [Linq.Enumerable]::SequenceEqual[byte]($source,[byte[]]@(255,0,1,0))){throw 'Source/output ownership or mutation check differs.'}
[ordered]@{existingPublicCase='WebpTests.SixteenBitInputRoundsOnceToEightBitSamples';packetBytes=34;packetSha256=$hash;rawExact=$true;independentOutputs=$true;sourceUnchanged=$true;negativeMutationRejected=$true;productionChanged=$false;nativeBuilds=0;nativeEncodes=0;timingRuns=0;scope='Existing1x1 palette packet through shared palette composition. Arbitrary palettes, production admission and public normalization remain open.'}|ConvertTo-Json -Depth 4
