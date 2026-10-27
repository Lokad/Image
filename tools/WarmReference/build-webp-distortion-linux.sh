#!/usr/bin/env bash
set -euo pipefail
repo_root=$(cd "$(dirname "${BASH_SOURCE[0]}")/../.." && pwd)
prefix="$repo_root/external/linux-x64/native"
output="$repo_root/external/warm-reference"
[[ $(uname -s) == Linux ]] || { echo 'The warm WebP reference requires Linux.' >&2; exit 1; }
[[ -f "$prefix/include/libavcodec/avcodec.h" ]] || { echo 'Provision the pinned FFmpeg tools first.' >&2; exit 1; }
export PKG_CONFIG_PATH="$prefix/lib/pkgconfig${PKG_CONFIG_PATH:+:$PKG_CONFIG_PATH}"
mkdir -p "$output"
# Derive this one fixed case from our original adapter. Keep prepared effort4
# source/binary/identity unchanged; fail if its three effort expressions drift.
python3 - "$repo_root" "$output" <<'PY'
import pathlib, sys
root, output = map(pathlib.Path, sys.argv[1:])
s = (root/'tools/WarmReference/ffmpeg-webp-opaque-warm.c').read_text()
for old, new in [('compression_level = 4', 'compression_level = 2'),
                 ('compression_level != 4', 'compression_level != 2'),
                 (r'\"effort\":4', r'\"effort\":2')]:
    assert s.count(old) == 1, ('Owned adapter effort expression changed', old)
    s = s.replace(old, new)
(output/'ffmpeg-webp-distortion-warm.c').write_text(s)
PY
read -r -a cflags <<< "$(pkg-config --cflags libavcodec libavutil)"
read -r -a libraries <<< "$(pkg-config --static --libs libavcodec libavutil)"
gcc -std=c11 -O3 -Wall -Wextra -Werror "${cflags[@]}" "$output/ffmpeg-webp-distortion-warm.c" \
    -o "$output/ffmpeg-webp-distortion-warm" "${libraries[@]}"
python3 - "$repo_root" "$output" "$prefix" <<'PY'
import hashlib, json, pathlib, subprocess, sys
root, output, prefix = map(pathlib.Path, sys.argv[1:])
digest = lambda path: hashlib.sha256(path.read_bytes()).hexdigest()
record = dict(schemaVersion=1, project='Lokad.Image', purpose='Original development-only fixed effort2 opaque WebP FFmpeg worker',
    sourceSha256=digest(root/'tools/WarmReference/ffmpeg-webp-opaque-warm.c'),
    generatedSourceSha256=digest(output/'ffmpeg-webp-distortion-warm.c'),
    onlyEffortChanged=True, effort=2,
    buildScriptSha256=digest(root/'tools/WarmReference/build-webp-distortion-linux.sh'),
    workerSha256=digest(output/'ffmpeg-webp-distortion-warm'),
    compiler=subprocess.check_output(['gcc','--version'],text=True).splitlines()[0],
    flags=['-std=c11','-O3','-Wall','-Wextra','-Werror'],
    staticArchives={p.name:digest(p) for p in sorted((prefix/'lib').glob('*.a'))},
    libraries=subprocess.check_output(['pkg-config','--static','--libs','libavcodec','libavutil'],text=True)
        .strip().replace(str(prefix.resolve()),'{native-prefix}').replace(str(root),'{repository}'))
(output/'webp-distortion-identity.json').write_text(json.dumps(record,indent=2)+'\n')
print(json.dumps(dict(built=True,effort=2,workerSha256=record['workerSha256'])))
PY
