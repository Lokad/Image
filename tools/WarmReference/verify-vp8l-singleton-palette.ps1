param([Parameter(Mandatory)][string]$NativePacketPath)
$ErrorActionPreference='Stop'
$names='Vp8lPredictorParity','Vp8lCrossColorParity','Vp8lMatchParity','Vp8lLzParity','Vp8lPrefixParity','Vp8lBitParity','Vp8lAuxiliaryParity','Vp8lSingletonPaletteParity'
Add-Type -Path @($names|ForEach-Object{Join-Path $PSScriptRoot "${_}.cs"})
$source=[byte[]]@(255,0,1,0)
$first=[Vp8lSingletonPaletteParity]::Encode($source)
$second=[Vp8lSingletonPaletteParity]::Encode($source)
$expected=[IO.File]::ReadAllBytes((Resolve-Path -LiteralPath $NativePacketPath).Path)
$hash=[Convert]::ToHexStringLower([Security.Cryptography.SHA256]::HashData($expected))
if($expected.Length -ne 34 -or $hash -ne '98dc19987970964d8c3a2bd9e57c9e4cd4e29dc1e99fe8658b087f53aabcb618' -or
   -not [Linq.Enumerable]::SequenceEqual[byte]($first,$expected) -or
   -not [Linq.Enumerable]::SequenceEqual[byte]($second,$expected) -or [Object]::ReferenceEquals($first,$second)){throw 'Singleton palette packet differs.'}
$second[20]=$second[20] -bxor 1
if([Linq.Enumerable]::SequenceEqual[byte]($second,$expected) -or
   -not [Linq.Enumerable]::SequenceEqual[byte]($first,$expected) -or
   -not [Linq.Enumerable]::SequenceEqual[byte]($source,[byte[]]@(255,0,1,0))){throw 'Source/output ownership or mutation check differs.'}
[ordered]@{existingPublicCase='WebpTests.SixteenBitInputRoundsOnceToEightBitSamples';packetBytes=34;packetSha256=$hash;rawExact=$true;independentOutputs=$true;sourceUnchanged=$true;negativeMutationRejected=$true;productionChanged=$false;nativeBuilds=0;nativeEncodes=0;timingRuns=0;scope='First one-color palette primitive only. Multi-color packing/sorting, integration, admission and public normalization remain open.'}|ConvertTo-Json -Depth 4
