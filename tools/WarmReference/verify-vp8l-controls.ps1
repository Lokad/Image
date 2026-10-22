param(
    [Parameter(Mandatory)][string]$SourcePath,
    [Parameter(Mandatory)][string]$FixtureDirectory,
    [Parameter(Mandatory)][string]$FirstReferenceArchive,
    [Parameter(Mandatory)][string]$RemainingReferenceArchive,
    [Parameter(Mandatory)][string]$AssemblyPath,
    [string]$LibraryPath='src/Lokad.Image/bin/Release/net10.0/Lokad.Image.dll'
)
$ErrorActionPreference='Stop'
function FileHash([string]$Path){[Convert]::ToHexStringLower([Security.Cryptography.SHA256]::HashData([IO.File]::ReadAllBytes((Resolve-Path -LiteralPath $Path).Path)))}
if((FileHash $FirstReferenceArchive) -ne '131e24330559655b4c71c1a9e086ee3155ba8f5d3f96f0ddddd34718d8f7f12e' -or
   (FileHash $RemainingReferenceArchive) -ne 'd62a0362758b5744f06b797febee6fdbaf09452461c4f47fd901d87acf348eef'){throw 'Frozen reference archives differ.'}
[void][Reflection.Assembly]::LoadFrom((Resolve-Path -LiteralPath $LibraryPath).Path)
[void][Reflection.Assembly]::LoadFrom((Resolve-Path -LiteralPath $AssemblyPath).Path)
$references=@(Get-ChildItem (Join-Path $PSHOME 'ref') -Filter '*.dll'|ForEach-Object FullName)
$references+=@((Resolve-Path $AssemblyPath).Path,(Resolve-Path $LibraryPath).Path)
Add-Type -Path (Join-Path $PSScriptRoot 'Vp8lControlProbe.cs') -ReferencedAssemblies $references
$rows=[Vp8lControlProbe]::Run((Resolve-Path -LiteralPath $SourcePath).Path,
    (Resolve-Path -LiteralPath $FixtureDirectory).Path,
    (Resolve-Path -LiteralPath $FirstReferenceArchive).Path,
    (Resolve-Path -LiteralPath $RemainingReferenceArchive).Path) | ConvertFrom-Json
$names='Encode8','EncodeOpaque8','PhotographLossless8','DiagramLossless8'
$sourceHashes='70f0fb18c341e2740b875a5a34e21ca63f0821b3386352fe2971020c5a6aac80','54e27bd73c9ee221bdd4872e6b7bf79676257346cee2eff8ddacccb89e0cdb0a','2b970f3ddee2a651ca381aab10b90db78a2ce08db8c7f70154bec92c1ec33090','ba885c79271ce0e2861cce19458d141b5462f2a169634e7c46a33fc0afb9d243'
$packetHashes='08ddac115f1770d778e6dbc76373440224b32e6f93b616605e234c63206eebde','465b70013a8a598ec5e95e2e4d8fff3042317fa21dc31f2350fe9edb46ce465f','8e8c2a10a1be8c27233d7c70ab5c704d95924759408ecb8c02a3da50927aabb8','399c218d2fd333724a3b9f77d8c53bd958f01483b314c496935b022d7a03e0bf'
if($rows.Count -ne 4){throw 'Frozen control count differs.'}
for($i=0;$i -lt 4;$i++){
    if($rows[$i].name -ne $names[$i] -or $rows[$i].sourceSha256 -ne $sourceHashes[$i] -or
       -not $rows[$i].rawExact -or $rows[$i].sha256 -ne $packetHashes[$i] -or
       -not $rows[$i].sourceUnchanged -or -not $rows[$i].independentOutputs -or -not $rows[$i].negativeMutationsRejected){throw 'Complete packet, source or ownership differs.'}
}
[ordered]@{
    sourceOnly=$true;uniquePackets=4;allRawExact=$true;metadataExemptions=0;cases=$rows
    productionChanged=$false;nativeBuilds=0;nativeEncodes=0;timingRuns=0
    scope='Four frozen513x259 RGBA8 cores at method4/quality75/exact=1. Source model only; RGBA16/async public boundaries, other algorithms and admitted production integration remain open.'
} | ConvertTo-Json -Depth 10
