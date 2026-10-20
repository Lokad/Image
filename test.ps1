param([switch]$Differential, [string]$Configuration = 'Release', [switch]$NoRestore, [switch]$SerialBuild, [switch]$NoBuild)
$ErrorActionPreference = 'Stop'
Push-Location $PSScriptRoot
try {
    if (-not $NoBuild) {
        $buildArguments = @('build', 'Lokad.Image.slnx', '-c', $Configuration, '--tl:off', '--nologo', '-v', 'minimal')
        if ($NoRestore) { $buildArguments += '--no-restore' }
        if ($SerialBuild) { $buildArguments += @('-m:1', '-nr:false', '-p:UseSharedCompilation=false') }
        dotnet @buildArguments
        if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
    }
    $projects = @('tests/Lokad.Image.Tests', 'tests/Lokad.Image.PublicApi.Tests')
    if ($Differential) { $projects += 'tests/Lokad.Image.DifferentialTests' }
    foreach ($project in $projects) {
        dotnet test $project -c $Configuration --no-build --tl:off --nologo -v minimal
        if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
    }
} finally { Pop-Location }
