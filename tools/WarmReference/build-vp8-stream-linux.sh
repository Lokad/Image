#!/usr/bin/env bash
set -euo pipefail
repo_root=$(cd "$(dirname "${BASH_SOURCE[0]}")/../.." && pwd)
prefix="$repo_root/external/linux-x64/native"
output="$repo_root/external/warm-reference"
[[ $(uname -s) == Linux ]] || { echo 'The warm VP8 reference requires Linux.' >&2; exit 1; }
[[ -f "$prefix/include/libswscale/swscale.h" ]] || { echo 'Provision the pinned FFmpeg tools first.' >&2; exit 1; }
export PKG_CONFIG_PATH="$prefix/lib/pkgconfig${PKG_CONFIG_PATH:+:$PKG_CONFIG_PATH}"
mkdir -p "$output"
read -r -a cflags <<< "$(pkg-config --cflags libavformat libavcodec libavutil libswscale)"
read -r -a libraries <<< "$(pkg-config --static --libs libavformat libavcodec libavutil libswscale)"
gcc -std=c11 -O3 -Wall -Wextra -Werror "${cflags[@]}" "$repo_root/tools/WarmReference/ffmpeg-vp8-stream-warm.c" \
    -o "$output/ffmpeg-vp8-stream-warm" "${libraries[@]}"
python3 - "$repo_root" "$output" "$prefix" <<'PY'
import hashlib, json, pathlib, subprocess, sys
root, output, prefix = map(pathlib.Path, sys.argv[1:])
digest = lambda path: hashlib.sha256(path.read_bytes()).hexdigest()
record = dict(schemaVersion=1, project='Lokad.Image', purpose='Original development-only warm VP8 fresh Stream decode FFmpeg worker',
    sourceSha256=digest(root/'tools/WarmReference/ffmpeg-vp8-stream-warm.c'),
    buildScriptSha256=digest(root/'tools/WarmReference/build-vp8-stream-linux.sh'),
    workerSha256=digest(output/'ffmpeg-vp8-stream-warm'),
    compiler=subprocess.check_output(['gcc','--version'],text=True).splitlines()[0],
    flags=['-std=c11','-O3','-Wall','-Wextra','-Werror'],
    staticArchives={p.name:digest(p) for p in sorted((prefix/'lib').glob('*.a'))},
    libraries=subprocess.check_output(['pkg-config','--static','--libs','libavformat','libavcodec','libavutil','libswscale'],text=True)
        .strip().replace(str(prefix.resolve()),'{native-prefix}').replace(str(root),'{repository}'))
(output/'vp8-stream-identity.json').write_text(json.dumps(record,indent=2)+'\n')
print(json.dumps(dict(built=True,workerSha256=record['workerSha256'])))
PY
