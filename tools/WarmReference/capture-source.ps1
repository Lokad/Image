$ErrorActionPreference = 'Stop'
$repoRoot = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '../..'))
Push-Location $repoRoot
try {
    [string[]]$paths = @(git ls-files --cached --others --exclude-standard)
    if ($LASTEXITCODE -ne 0 -or $paths.Length -eq 0) { throw 'Cannot establish the owned source inventory.' }
    $paths = @($paths | Select-Object -Unique)
    [Array]::Sort($paths, [StringComparer]::Ordinal)
    $files = foreach ($path in $paths) {
        if ($path -match '(^|/)(PLAN\.md|external|tmp|artifacts|bin|obj|BenchmarkDotNet\.Artifacts)(/|$)|\.pem$') {
            throw "Ignored or private path reached source inventory: $path"
        }
        $item = Get-Item -LiteralPath $path
        if ($item.LinkType) { throw "Source inventory requires a regular file: $path" }
        [ordered]@{ path = $path; bytes = $item.Length; sha256 = (Get-FileHash -LiteralPath $path -Algorithm SHA256).Hash.ToLowerInvariant() }
    }
    $canonical = ($files | ForEach-Object { $_.path + [char]0 + $_.bytes.ToString([Globalization.CultureInfo]::InvariantCulture) + [char]0 + $_.sha256 + [char]10 }) -join ''
    $hash = [Convert]::ToHexStringLower([Security.Cryptography.SHA256]::HashData([Text.Encoding]::UTF8.GetBytes($canonical)))
    $head = & git rev-parse --verify HEAD 2>$null
    if ($LASTEXITCODE -ne 0) { $head = $null }
    $record = [ordered]@{ schemaVersion = 1; project = 'Lokad.Image'; sourceRevision = $head; sourceSha256 = $hash
        sourceHashScope = 'Ordinal relative-path order; path NUL decimal byte length NUL file SHA256 LF for every owned source/fixture file'
        committedRevisionAvailable = $null -ne $head; files = @($files) }
    $output = Join-Path $repoRoot 'tmp'
    New-Item -ItemType Directory -Path $output -Force | Out-Null
    [IO.File]::WriteAllText((Join-Path $output 'benchmark-source.json'), ($record | ConvertTo-Json -Depth 5) + [char]10, [Text.UTF8Encoding]::new($false))
    [IO.File]::WriteAllLines((Join-Path $output 'benchmark-source-files.txt'), $paths, [Text.UTF8Encoding]::new($false))
    "Owned source snapshot: $hash ($($paths.Length) files; committed revision available: $($null -ne $head))"
} finally { Pop-Location }
