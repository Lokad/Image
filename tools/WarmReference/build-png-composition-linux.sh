#!/usr/bin/env bash
set -euo pipefail
repo_root=$(cd "$(dirname "${BASH_SOURCE[0]}")/../.." && pwd)
prefix="$repo_root/external/linux-x64/native"
output="$repo_root/external/warm-reference"
[[ $(uname -s) == Linux ]] || { echo 'The warm PNG composition reference requires Linux.' >&2; exit 1; }
[[ -f "$prefix/include/libavformat/avio.h" ]] || { echo 'Provision the pinned FFmpeg tools first.' >&2; exit 1; }
export PKG_CONFIG_PATH="$prefix/lib/pkgconfig${PKG_CONFIG_PATH:+:$PKG_CONFIG_PATH}"
mkdir -p "$output"
read -r -a cflags <<< "$(pkg-config --cflags libavformat libavcodec libavfilter libavutil)"
read -r -a libraries <<< "$(pkg-config --static --libs libavformat libavcodec libavfilter libavutil)"
gcc -std=c11 -O3 -Wall -Wextra -Werror "${cflags[@]}" "$repo_root/tools/WarmReference/ffmpeg-png-composition-warm.c" \
    -o "$output/ffmpeg-png-composition-warm" "${libraries[@]}"
python3 - "$repo_root" "$output" "$prefix" <<'PY'
import hashlib, json, pathlib, subprocess, sys
root, output, prefix = map(pathlib.Path, sys.argv[1:])
digest = lambda path: hashlib.sha256(path.read_bytes()).hexdigest()
record = dict(schemaVersion=1, project='Lokad.Image', purpose='Original development-only warm PNG composition FFmpeg worker',
    sourceSha256=digest(root/'tools/WarmReference/ffmpeg-png-composition-warm.c'),
    buildScriptSha256=digest(root/'tools/WarmReference/build-png-composition-linux.sh'),
    workerSha256=digest(output/'ffmpeg-png-composition-warm'),
    compiler=subprocess.check_output(['gcc','--version'],text=True).splitlines()[0],
    flags=['-std=c11','-O3','-Wall','-Wextra','-Werror'],
    staticArchives={p.name:digest(p) for p in sorted((prefix/'lib').glob('*.a'))},
    libraries=subprocess.check_output(['pkg-config','--static','--libs','libavformat','libavcodec','libavfilter','libavutil'],text=True)
        .strip().replace(str(prefix.resolve()),'{native-prefix}').replace(str(root),'{repository}'))
(output/'png-composition-identity.json').write_text(json.dumps(record,indent=2)+'\n')
print(json.dumps(dict(built=True,workerSha256=record['workerSha256'])))
PY
