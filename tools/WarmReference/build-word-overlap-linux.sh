#!/usr/bin/env bash
set -euo pipefail
root=$(cd "$(dirname "${BASH_SOURCE[0]}")/../.." && pwd)
prefix="$root/external/linux-x64/native"
output=${1:-"$root/external/warm-reference/word-overlap"}
library=${2:-"$root/src/Lokad.Image/bin/Release/net10.0/Lokad.Image.dll"}
[[ $(uname -s) == Linux && -f "$library" ]] || { echo 'Use Linux with an existing immutable Release DLL.' >&2; exit 1; }
export PKG_CONFIG_PATH="$prefix/lib/pkgconfig${PKG_CONFIG_PATH:+:$PKG_CONFIG_PATH}"
mkdir -p "$output"
read -r -a cflags <<< "$(pkg-config --cflags libavutil)"
read -r -a libraries <<< "$(pkg-config --static --libs libavutil)"
gcc -std=c11 -O3 -Wall -Wextra -Werror "${cflags[@]}" "$root/tools/WarmReference/ffmpeg-copy-region-word-overlap-warm.c" \
    -o "$output/ffmpeg-copy-region-word-overlap-warm" "${libraries[@]}"
project="$root/tools/WarmReference/WordOverlapWorker/WordOverlapWorker.csproj"
dotnet restore "$project" --ignore-failed-sources --tl:off -p:NuGetAudit=false
dotnet build "$project" -c Release --no-restore --no-dependencies --tl:off -m:1 -nr:false \
    -p:UseSharedCompilation=false "-p:LokadImageAssembly=$library" -o "$output/managed"
