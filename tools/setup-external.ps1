param([string]$FfmpegPath, [string]$FfprobePath)
$ErrorActionPreference = 'Stop'
$repoRoot = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$manifest = Get-Content -LiteralPath (Join-Path $PSScriptRoot 'external-sources.json') -Raw | ConvertFrom-Json
foreach ($source in $manifest.sources) {
    $destination = Join-Path $repoRoot $source.destination
    if (-not (Test-Path -LiteralPath $destination)) {
        git clone --quiet --depth 1 --single-branch --branch $source.tag $source.url $destination
        if ($LASTEXITCODE -ne 0) { throw 'Reference source clone failed.' }
    }
    $revision = git -C $destination rev-parse HEAD
    if ($LASTEXITCODE -ne 0 -or $revision -ne $source.commit) { throw 'Reference source revision differs from manifest.' }
}
if ($FfmpegPath -or $FfprobePath) {
    if (-not $IsWindows -or -not $FfmpegPath -or -not $FfprobePath) {
        throw 'Pinned binary registration currently supports Windows x64 and requires both executable paths.'
    }
    $identity = $manifest.executables.'windows-x64'
    $ffmpegHash = (Get-FileHash -LiteralPath $FfmpegPath -Algorithm SHA256).Hash.ToLowerInvariant()
    $ffprobeHash = (Get-FileHash -LiteralPath $FfprobePath -Algorithm SHA256).Hash.ToLowerInvariant()
    if ($ffmpegHash -ne $identity.ffmpegSha256 -or $ffprobeHash -ne $identity.ffprobeSha256) {
        throw 'Binary hashes differ from the pinned oracle; pass the actual executables, not PATH shims.'
    }
    $tools = Join-Path $repoRoot 'external/tools'
    New-Item -ItemType Directory -Path $tools -Force | Out-Null
    Copy-Item -LiteralPath $FfmpegPath -Destination (Join-Path $tools 'ffmpeg.exe')
    Copy-Item -LiteralPath $FfprobePath -Destination (Join-Path $tools 'ffprobe.exe')
    Write-Output 'Pinned oracle executables registered under external/tools.'
}
