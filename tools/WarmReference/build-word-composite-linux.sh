#!/usr/bin/env bash
set -euo pipefail
root=$(cd "$(dirname "${BASH_SOURCE[0]}")/../.." && pwd)
output=${1:-"$root/external/warm-reference/word-composite"}
library=${2:-"$root/src/Lokad.Image/bin/Release/net10.0/Lokad.Image.dll"}
reference=${3:-"$root/tools/TextReference/bin/Release/net10.0/SixLabors.ImageSharp.dll"}
[[ $(uname -s) == Linux && -f "$library" && -f "$reference" ]] || {
    echo 'Use Linux with existing immutable Release and approved ImageSharp 3.1.12 DLLs.' >&2; exit 1;
}
library=$(realpath "$library")
reference=$(realpath "$reference")
output=$(realpath -m "$output")
project="$root/tools/WarmReference/WordCompositeWorker/WordCompositeWorker.csproj"
dotnet restore "$project" --ignore-failed-sources --tl:off -p:NuGetAudit=false
dotnet build "$project" -c Release --no-restore --no-dependencies --tl:off -m:1 -nr:false \
    -p:UseSharedCompilation=false "-p:LokadImageAssembly=$library" "-p:ImageSharpAssembly=$reference" -o "$output"
