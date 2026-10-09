param([switch]$Smoke, [switch]$DedicatedVm, [string]$Filter, [switch]$NoBuild)
$ErrorActionPreference = 'Stop'
if (-not $Smoke -and -not $DedicatedVm) {
    throw 'Performance measurement requires the quiet dedicated VM. Use -Smoke to verify the harness.'
}
Push-Location $PSScriptRoot
$previousRunId = $env:LOKAD_IMAGE_BENCHMARK_RUN_ID
try {
    $env:LOKAD_IMAGE_BENCHMARK_RUN_ID = [Guid]::NewGuid().ToString('N')
    Write-Host "Benchmark validation run: $env:LOKAD_IMAGE_BENCHMARK_RUN_ID"
    if (-not $NoBuild) {
        dotnet build Lokad.Image.slnx -c Release --no-restore --tl:off --nologo -v minimal -m:1 -nr:false -p:UseSharedCompilation=false
        if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
    }
    $job = if ($Smoke) { 'Dry' } else { 'Short' }
    # Quoting preserves wildcard filters on Unix, where native unquoted arguments
    # otherwise expand to matching filenames before BenchmarkDotNet sees them.
    $runArtifacts = Join-Path $PSScriptRoot "artifacts/benchmark-runs/$env:LOKAD_IMAGE_BENCHMARK_RUN_ID"
    $benchmarkDll = Join-Path $PSScriptRoot 'benchmarks/Lokad.Image.Benchmarks/bin/Release/net10.0/Lokad.Image.Benchmarks.dll'
    if ($PSBoundParameters.ContainsKey('Filter')) {
        if ([string]::IsNullOrWhiteSpace($Filter)) { throw 'Supply a nonempty explicit benchmark filter.' }
        dotnet $benchmarkDll --filter "$Filter" --job $job --artifacts "$runArtifacts"
    } else {
        $representativeJob = if ($Smoke) { '--representative-smoke' } else { '--representative-short' }
        dotnet $benchmarkDll $representativeJob --artifacts "$runArtifacts" --buildTimeout 600 --stopOnFirstError
    }
    if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
} finally {
    $env:LOKAD_IMAGE_BENCHMARK_RUN_ID = $previousRunId
    Pop-Location
}
