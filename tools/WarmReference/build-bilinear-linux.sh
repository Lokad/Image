#!/usr/bin/env bash
set -euo pipefail
repo_root=$(cd "$(dirname "${BASH_SOURCE[0]}")/../.." && pwd)
prefix="$repo_root/external/linux-x64/native"
output="$repo_root/external/warm-reference"
one_shot=0
stem=ffmpeg-bilinear-warm
identity=bilinear-identity.json
if [[ $# == 1 && $1 == --one-shot ]]; then
    one_shot=1
    stem=ffmpeg-bilinear-one-shot-warm
    identity=bilinear-one-shot-identity.json
elif [[ $# != 0 ]]; then
    echo 'Usage: build-bilinear-linux.sh [--one-shot]' >&2; exit 1
fi
[[ $(uname -s) == Linux ]] || { echo 'The warm native reference requires Linux.' >&2; exit 1; }
[[ -f "$prefix/include/libavfilter/avfilter.h" ]] || { echo 'Provision the pinned Linux FFmpeg tools first.' >&2; exit 1; }
export PKG_CONFIG_PATH="$prefix/lib/pkgconfig${PKG_CONFIG_PATH:+:$PKG_CONFIG_PATH}"
mkdir -p "$output"
read -r -a cflags <<< "$(pkg-config --cflags libavfilter libavutil)"
read -r -a libraries <<< "$(pkg-config --static --libs libavfilter libavutil)"
gcc -std=c11 -O3 -Wall -Wextra -Werror -DLOKAD_IMAGE_BILINEAR_ONE_SHOT="$one_shot" "${cflags[@]}" "$repo_root/tools/WarmReference/ffmpeg-bilinear-warm.c" \
    -o "$output/$stem" "${libraries[@]}"
python3 - "$repo_root" "$output" "$prefix" "$stem" "$identity" "$one_shot" <<'PY'
import hashlib, json, pathlib, re, subprocess, sys
root, output, prefix = map(pathlib.Path, sys.argv[1:4])
stem, identity, one_shot = sys.argv[4], sys.argv[5], int(sys.argv[6])
digest = lambda path: hashlib.sha256(path.read_bytes()).hexdigest()
record = dict(schemaVersion=1, project='Lokad.Image', purpose='Original development-only warm FFmpeg worker',
    sourceSha256=digest(root/'tools/WarmReference/ffmpeg-bilinear-warm.c'),
    buildScriptSha256=digest(root/'tools/WarmReference/build-bilinear-linux.sh'),
    workerSha256=digest(output/stem),
    compiler=subprocess.check_output(['gcc','--version'], text=True).splitlines()[0],
    flags=['-std=c11','-O3','-Wall','-Wextra','-Werror',f'-DLOKAD_IMAGE_BILINEAR_ONE_SHOT={one_shot}'],
    oneShotGraph=bool(one_shot),
    staticArchives={p.name: digest(p) for p in sorted((prefix/'lib').glob('*.a'))},
    libraries=subprocess.check_output(['pkg-config','--static','--libs','libavfilter','libavutil'], text=True)
        .strip())
record['libraries']=re.sub(r'-L\S+', '-L{native-prefix}/lib',record['libraries'])
(output/identity).write_text(json.dumps(record, indent=2)+'\n')
print(json.dumps(dict(built=True, workerSha256=record['workerSha256'])))
PY
