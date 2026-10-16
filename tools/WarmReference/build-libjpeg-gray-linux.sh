#!/usr/bin/env bash
set -euo pipefail
repo_root=$(cd "$(dirname "${BASH_SOURCE[0]}")/../.." && pwd)
source_root="$repo_root/external/libjpeg-turbo"
build_root="$repo_root/external/jpeg-gray-bench-build"
output="$repo_root/external/warm-reference"
revision=9217719d3a58633923b096af4c1d50d304768a64
[[ $(uname -s) == Linux ]] || { echo 'The grayscale reference requires Linux.' >&2; exit 1; }
if [[ ! -d "$source_root" ]]; then
    git clone --depth 1 --branch 3.1.4.1 https://github.com/libjpeg-turbo/libjpeg-turbo.git "$source_root"
fi
[[ $(git -C "$source_root" rev-parse HEAD) == "$revision" ]]
[[ -z $(git -C "$source_root" status --porcelain) ]]
cmake -S "$source_root" -B "$build_root" -DCMAKE_BUILD_TYPE=Release \
    -DENABLE_SHARED=OFF -DENABLE_STATIC=ON -DWITH_SIMD=ON -DWITH_TURBOJPEG=OFF
cmake --build "$build_root" --target jpeg-static --parallel 1
mkdir -p "$output"
cc -std=c11 -O3 -ffp-contract=off -fno-math-errno -Wall -Wextra -Werror \
    -I"$source_root/src" -I"$build_root" \
    "$repo_root/tools/WarmReference/libjpeg-gray-warm-candidate.c" "$build_root/libjpeg.a" -lm \
    -o "$output/libjpeg-gray-warm-candidate"
python3 - "$repo_root" "$source_root" "$build_root" "$output" <<'PY'
import hashlib,json,pathlib,subprocess,sys
root,source,build,output=map(pathlib.Path,sys.argv[1:])
sha=lambda p:hashlib.sha256(p.read_bytes()).hexdigest()
cache=(build/'CMakeCache.txt').read_text();assert 'WITH_SIMD:BOOL=ON' in cache and 'WITH_SIMD_INT:INTERNAL=1' in cache
record=dict(schemaVersion=1,project='Lokad.Image',purpose='Original standalone SIMD libjpeg-turbo reference; single-component grayscale JPEG benchmark exception only',revision=subprocess.check_output(['git','-C',str(source),'rev-parse','HEAD'],text=True).strip(),simdEnabled=True,workerSha256=sha(output/'libjpeg-gray-warm-candidate'),sourceSha256=sha(root/'tools/WarmReference/libjpeg-gray-warm-candidate.c'),buildScriptSha256=sha(root/'tools/WarmReference/build-libjpeg-gray-linux.sh'),staticLibrarySha256=sha(build/'libjpeg.a'),compiler=subprocess.check_output(['cc','--version'],text=True).splitlines()[0],flags=['-std=c11','-O3','-ffp-contract=off','-fno-math-errno','-Wall','-Wextra','-Werror'])
(output/'libjpeg-gray-identity.json').write_text(json.dumps(record,indent=2)+'\n')
print(json.dumps(dict(built=True,workerSha256=record['workerSha256'],simdEnabled=True)))
PY
