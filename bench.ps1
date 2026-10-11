param([switch]$Smoke, [switch]$DedicatedVm, [string]$Filter, [switch]$NoBuild, [switch]$HorizontalFlip,
    [switch]$Warm, [ValidateRange(0, 62)][int]$Cpu = 0, [switch]$WarmConfirm,
    [ValidateSet('Flip', 'Png', 'Jpeg', 'JpegDecode', 'Webp', 'WebpDecode', 'Vp8Decode', 'Nearest')][string]$WarmFamily = 'Flip', [string]$WarmCases)
$ErrorActionPreference = 'Stop'
if ($WarmConfirm -and (-not $Warm -or $Smoke)) { throw '-WarmConfirm requires -Warm with -DedicatedVm.' }
if (($PSBoundParameters.ContainsKey('WarmFamily') -or $PSBoundParameters.ContainsKey('WarmCases')) -and -not $Warm) {
    throw '-WarmFamily/-WarmCases require -Warm.'
}
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
    if ($Warm) {
        if ($HorizontalFlip -or $PSBoundParameters.ContainsKey('Filter')) { throw '-Warm supplies its own narrow selection.' }
        $warmJob = if ($Smoke) { '--warm-smoke' } elseif ($WarmConfirm) { '--warm-confirm' } else { '--warm-short' }
        $warmArguments = @($warmJob, '--warm-cpu', $Cpu, '--warm-family', $WarmFamily)
        if ($PSBoundParameters.ContainsKey('WarmCases')) { $warmArguments += @('--warm-cases', $WarmCases) }
        dotnet $benchmarkDll @warmArguments --artifacts "$runArtifacts" --buildTimeout 600 --stopOnFirstError
    } elseif ($HorizontalFlip) {
        if ($PSBoundParameters.ContainsKey('Filter')) { throw '-HorizontalFlip supplies its own narrow selection.' }
        $flipJob = if ($Smoke) { '--horizontal-flip-smoke' } else { '--horizontal-flip-short' }
        dotnet $benchmarkDll $flipJob --artifacts "$runArtifacts" --buildTimeout 600 --stopOnFirstError
    } elseif ($PSBoundParameters.ContainsKey('Filter')) {
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
