#!/usr/bin/env bash
set -euo pipefail
repo_root=$(cd "$(dirname "${BASH_SOURCE[0]}")/../.." && pwd)
prefix="$repo_root/external/linux-x64/native"
output="$repo_root/external/warm-reference"
[[ $(uname -s) == Linux ]] || { echo 'Linux required.' >&2; exit 1; }
export PKG_CONFIG_PATH="$prefix/lib/pkgconfig${PKG_CONFIG_PATH:+:$PKG_CONFIG_PATH}"
mkdir -p "$output"
read -r -a cflags <<< "$(pkg-config --cflags libavcodec libavutil libswscale)"
read -r -a libraries <<< "$(pkg-config --static --libs libavcodec libavutil libswscale)"
gcc -std=c11 -O3 -Wall -Wextra -Werror "${cflags[@]}" "$repo_root/tools/WarmReference/ffmpeg-jpeg-rgb-samples-warm.c" \
    -o "$output/ffmpeg-jpeg-rgb-samples-warm" "${libraries[@]}"
python3 - "$repo_root" "$output" "$prefix" <<'PY'
import hashlib,json,pathlib,subprocess,sys
root,output,prefix=map(pathlib.Path,sys.argv[1:])
digest=lambda p:hashlib.sha256(p.read_bytes()).hexdigest()
record=dict(schemaVersion=1,project='Lokad.Image',purpose='Original caller-owned RGB420 JPEG decode FFmpeg reference',
    sourceSha256=digest(root/'tools/WarmReference/ffmpeg-jpeg-rgb-samples-warm.c'),
    buildScriptSha256=digest(root/'tools/WarmReference/build-jpeg-rgb-samples-linux.sh'),
    workerSha256=digest(output/'ffmpeg-jpeg-rgb-samples-warm'),
    compiler=subprocess.check_output(['gcc','--version'],text=True).splitlines()[0],
    flags=['-std=c11','-O3','-Wall','-Wextra','-Werror'],
    staticArchives={p.name:digest(p) for p in sorted((prefix/'lib').glob('*.a'))})
(output/'jpeg-rgb-samples-identity.json').write_text(json.dumps(record,indent=2)+'\n')
print(json.dumps(dict(built=True,workerSha256=record['workerSha256'])))
PY
