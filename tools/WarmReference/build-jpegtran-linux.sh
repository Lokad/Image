#!/usr/bin/env bash
set -euo pipefail
repo_root=$(cd "$(dirname "${BASH_SOURCE[0]}")/../.." && pwd)
source_root="$repo_root/external/libjpeg-turbo"
build_root="$repo_root/external/jpegtran-build"
output="$repo_root/external/warm-reference"
revision=9217719d3a58633923b096af4c1d50d304768a64
[[ $(uname -s) == Linux ]] || { echo 'JPEG fixture preparation requires Linux.' >&2; exit 1; }
if [[ ! -d "$source_root" ]]; then
    git clone --depth 1 --branch 3.1.4.1 https://github.com/libjpeg-turbo/libjpeg-turbo.git "$source_root"
fi
[[ $(git -C "$source_root" rev-parse HEAD) == "$revision" ]] || { echo 'JPEG fixture tool checkout differs from the pinned revision.' >&2; exit 1; }
[[ -z $(git -C "$source_root" status --porcelain) ]] || { echo 'JPEG fixture tool checkout contains changes.' >&2; exit 1; }
# This tool rewrites coefficients outside measured intervals; SIMD is unnecessary.
cmake -S "$source_root" -B "$build_root" -DCMAKE_BUILD_TYPE=Release \
    -DENABLE_SHARED=OFF -DENABLE_STATIC=ON -DWITH_SIMD=OFF -DWITH_TURBOJPEG=OFF
cmake --build "$build_root" --target jpegtran-static --parallel 1
mkdir -p "$output"
cp "$build_root/jpegtran-static" "$output/jpegtran"
cc -std=c11 -O2 -Wall -Wextra -Werror -I"$source_root/src" -I"$build_root" \
    "$repo_root/tools/WarmReference/jpeg-coefficients.c" "$build_root/libjpeg.a" \
    -o "$output/jpeg-coefficients"
python3 - "$repo_root" "$source_root" "$build_root" "$output" <<'PY'
import hashlib,json,pathlib,subprocess,sys
root,source,build,output=map(pathlib.Path,sys.argv[1:])
digest=lambda p:hashlib.sha256(p.read_bytes()).hexdigest()
record=dict(schemaVersion=1,project='Lokad.Image',purpose='Ignored lossless progressive fixture producer; never a production or timing input',
    repository='https://github.com/libjpeg-turbo/libjpeg-turbo',tag='3.1.4.1',
    revision=subprocess.check_output(['git','-C',str(source),'rev-parse','HEAD'],text=True).strip(),
    buildScriptSha256=digest(root/'tools/WarmReference/build-jpegtran-linux.sh'),
    jpegtranSha256=digest(output/'jpegtran'),staticLibrarySha256=digest(build/'libjpeg.a'),
    coefficientVisitorSourceSha256=digest(root/'tools/WarmReference/jpeg-coefficients.c'),
    coefficientVisitorSha256=digest(output/'jpeg-coefficients'),
    compiler=subprocess.check_output(['cc','--version'],text=True).splitlines()[0],
    options=dict(buildType='Release',shared=False,static=True,simd=False,turboJpeg=False,buildJobs=1))
(output/'jpegtran-identity.json').write_text(json.dumps(record,indent=2)+'\n')
print(json.dumps(record))
PY
