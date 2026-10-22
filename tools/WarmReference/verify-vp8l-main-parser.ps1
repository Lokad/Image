param(
    [Parameter(Mandatory)][string]$SourcePath,
    [Parameter(Mandatory)][string]$NativePacketPath,
    [string]$AssemblyPath
)
$ErrorActionPreference='Stop'
function Hash([byte[]]$Bytes) { [Convert]::ToHexStringLower([Security.Cryptography.SHA256]::HashData($Bytes)) }
$source=[IO.File]::ReadAllBytes((Resolve-Path -LiteralPath $SourcePath).Path)
$sourceHash='70f0fb18c341e2740b875a5a34e21ca63f0821b3386352fe2971020c5a6aac80'
if($source.Length -ne 531468 -or (Hash $source) -ne $sourceHash){throw 'Frozen source differs.'}
if($AssemblyPath) {
    $assemblyFile=(Resolve-Path -LiteralPath $AssemblyPath).Path
    $directory=[IO.Path]::GetDirectoryName($assemblyFile)
    foreach($name in 'Probe.dll','AuxiliaryProbe.dll'){[void][Reflection.Assembly]::LoadFrom((Join-Path $directory $name))}
    [void][Reflection.Assembly]::LoadFrom($assemblyFile)
} else {
    $names='Vp8lPredictorParity','Vp8lCrossColorParity','Vp8lMatchParity','Vp8lLzParity','Vp8lPrefixParity','Vp8lBitParity','Vp8lAuxiliaryParity','Vp8lMainParity'
    Add-Type -Path @($names | ForEach-Object { Join-Path $PSScriptRoot "${_}.cs" })
}
$first=[Vp8lMainParity]::Run($source)
$next=[Vp8lMainParity]::Run($source)
if($first -cne $next -or (Hash $source) -ne $sourceHash){throw 'Repeated result or borrowed source changed.'}
$packet=[IO.File]::ReadAllBytes((Resolve-Path -LiteralPath $NativePacketPath).Path)
if($packet.Length -ne 183812 -or (Hash $packet) -ne '08ddac115f1770d778e6dbc76373440224b32e6f93b616605e234c63206eebde'){throw 'Frozen reference differs.'}
$result=$first | ConvertFrom-Json
if($result.samples -ne 132867 -or
   $result.indexSha256 -ne '343de9bb699497ccfdb3ad33f243e182416c7a2d8423d909ac847affd12efc68' -or
   $result.codedResidualSha256 -ne 'b72edfa146d02b4adbbe48a667ad28bf10b6073b2800c7679ccfc546c9969610' -or
   $result.caches.standard.BestBits -ne 10 -or $result.caches.repeats.BestBits -ne 10 -or
   $result.parser.cacheBits -ne 10 -or $result.parser.winner -ne 'trace' -or
   $result.parser.tokens -ne 110306 -or $result.parser.literals -ne 52617 -or
   $result.parser.copies -ne 6039 -or $result.parser.caches -ne 51650 -or
   $result.parser.semanticTokenSha256 -ne '060676025dd0b04254e06cc29988e5efbad7917b2a7e612407546e0851a1b574') {throw 'Source-derived main candidates or final semantic tokens differ.'}
$result | Add-Member -NotePropertyName verification -NotePropertyValue ([ordered]@{
    allMatchCandidatesExact=$true;allSemanticTokensExact=$true;sourceUnchanged=$true
    repeatedResultExact=$true;goldenPacketReadAfterModel=$true
    productionBuilds=0;nativeEncodes=0;timingRuns=0
    scope='ONE existing513x259 RGBA8 method4/quality75/exact=1 control. Source-only parse. Spatial groups, bit serialization and complete encoder remain open.'
})
$result | ConvertTo-Json -Depth 6
