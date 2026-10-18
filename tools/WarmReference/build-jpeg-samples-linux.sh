#!/usr/bin/env bash
set -euo pipefail
repo_root=$(cd "$(dirname "${BASH_SOURCE[0]}")/../.." && pwd)
prefix="$repo_root/external/linux-x64/native"
gray_source="$repo_root/external/libjpeg-turbo"
gray_build="$repo_root/external/jpeg-gray-bench-build"
output="$repo_root/external/warm-reference"
[[ $(uname -s) == Linux ]]
[[ $(git -C "$gray_source" rev-parse HEAD) == 9217719d3a58633923b096af4c1d50d304768a64 ]]
grep -q 'WITH_SIMD:BOOL=ON' "$gray_build/CMakeCache.txt"
export PKG_CONFIG_PATH="$prefix/lib/pkgconfig${PKG_CONFIG_PATH:+:$PKG_CONFIG_PATH}"
read -r -a cflags <<< "$(pkg-config --cflags libavcodec libavutil libswscale)"
read -r -a libraries <<< "$(pkg-config --static --libs libavcodec libavutil libswscale)"
gcc -std=c11 -O3 -Wall -Wextra -Werror "${cflags[@]}" "$repo_root/tools/WarmReference/jpeg-samples-warm.c" -o "$output/jpeg-samples-rgb-warm" "${libraries[@]}"
cc -std=c11 -O3 -ffp-contract=off -fno-math-errno -Wall -Wextra -Werror -DGRAY_REFERENCE \
    -I"$gray_source/src" -I"$gray_build" "$repo_root/tools/WarmReference/jpeg-samples-warm.c" "$gray_build/libjpeg.a" -lm -o "$output/jpeg-samples-gray-warm"
python3 - "$repo_root" "$prefix" "$gray_build" "$output" <<'PY'
import hashlib,json,pathlib,subprocess,sys
root,prefix,gray,output=map(pathlib.Path,sys.argv[1:]);sha=lambda p:hashlib.sha256(p.read_bytes()).hexdigest()
for kind in ['gray','rgb']:
    record=dict(schemaVersion=1,project='Lokad.Image',sampleFormat=kind,
        sourceSha256=sha(root/'tools/WarmReference/jpeg-samples-warm.c'),
        buildScriptSha256=sha(root/'tools/WarmReference/build-jpeg-samples-linux.sh'),
        workerSha256=sha(output/f'jpeg-samples-{kind}-warm'),
        compiler=subprocess.check_output(['cc','--version'],text=True).splitlines()[0],
        simdEnabled=True,reference='SIMD-enabled libjpeg-turbo 3.1.4.1' if kind=='gray' else 'FFmpeg n8.1 MJPEG',
        staticArchives={p.name:sha(p) for p in ([gray/'libjpeg.a'] if kind=='gray' else sorted((prefix/'lib').glob('*.a')))})
    (output/f'jpeg-samples-{kind}-identity.json').write_text(json.dumps(record,indent=2)+'\n')
    print(json.dumps(record))
PY
