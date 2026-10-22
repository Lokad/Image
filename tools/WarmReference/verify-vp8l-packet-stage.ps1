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
    $directory=[IO.Path]::GetDirectoryName($assemblyFile)
    foreach($name in 'Probe.dll','AuxiliaryProbe.dll','MainParityProbe.dll'){[void][Reflection.Assembly]::LoadFrom((Join-Path $directory $name))}
    [void][Reflection.Assembly]::LoadFrom($assemblyFile)
} else {
    $names='Vp8lPredictorParity','Vp8lCrossColorParity','Vp8lMatchParity','Vp8lLzParity','Vp8lPrefixParity','Vp8lBitParity','Vp8lAuxiliaryParity','Vp8lMainParity','Vp8lSpatialParity','Vp8lFrameParity','Vp8lPacketParity','Vp8lInitialChoice','Vp8lInputParity','Vp8lMainParsing'
    Add-Type -Path @($names | ForEach-Object { Join-Path $PSScriptRoot "${_}.cs" })
}
# Only source pixels enter either complete model invocation.
$first=[Vp8lPacketParity]::Build($source)
$next=[Vp8lPacketParity]::Build($source)
$packet=[IO.File]::ReadAllBytes((Resolve-Path -LiteralPath $NativePacketPath).Path)
$packetHash='08ddac115f1770d778e6dbc76373440224b32e6f93b616605e234c63206eebde'
$mapHash='ec2f7b311fbae0f5019064da191e2d61076bbf736f67a7dc70ae9ee8c7f28dd8'
if($packet.Length -ne 183812 -or (Hash $packet) -ne $packetHash){throw 'Frozen reference differs.'}
foreach($result in @($first,$next)) {
    if(-not(Equal $result.Packet $packet) -or (Hash $result.GroupMap) -ne $mapHash -or
       $result.Diagnostics.groups -ne 9 -or $result.Diagnostics.semanticTokens -ne 110306 -or
       $result.Framing.huffmanTrees -ne 45 -or $result.Framing.groupMapBits -ne 4466 -or
       $result.Framing.treeLengthsSha256 -ne 'f5af936066af9e065dc81ec04fe3fd072db5db09f224a315efe3d28dfc656c7d' -or
       $result.Framing.firstDataBit -ne 36698 -or $result.Framing.endBit -ne 1470323) {throw 'Complete source-derived packet or stage differs.'}
}
if((Hash $source) -ne $sourceHash -or [Object]::ReferenceEquals($first.Packet,$next.Packet) -or
   [Object]::ReferenceEquals($first.GroupMap,$next.GroupMap)){throw 'Source or independent output ownership differs.'}
$next.Packet[20]=$next.Packet[20] -bxor 1;$next.GroupMap[0]=$next.GroupMap[0] -bxor 1
if((Equal $next.Packet $packet) -or (Hash $next.GroupMap) -eq $mapHash -or
   -not(Equal $first.Packet $packet) -or (Hash $first.GroupMap) -ne $mapHash){throw 'Mutation accepted or surviving output changed.'}
[ordered]@{
    sourceOnly=$true;sourceSha256=$sourceHash;packetBytes=183812;packetSha256=(Hash $first.Packet)
    rawExact=$true;metadataExemptions=0;sourceUnchanged=$true;independentOutputs=$true
    negativeMutationsRejected=$true;goldenPacketReadAfterModel=$true
    diagnostics=$first.Diagnostics;framing=$first.Framing;clustering=$first.Clustering
    productionChanged=$false;productionBuilds=0;nativeBuilds=0;nativeEncodes=0;timingRuns=0
    scope='ONE frozen513x259 RGBA8 libwebp1.6.0 method4/quality75/exact=1 control. Complete source-only packet; other controls and admitted production integration remain open.'
} | ConvertTo-Json -Depth 6
