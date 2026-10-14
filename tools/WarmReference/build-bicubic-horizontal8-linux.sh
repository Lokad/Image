#!/usr/bin/env bash
set -euo pipefail
repo_root=$(cd "$(dirname "${BASH_SOURCE[0]}")/../.." && pwd)
prefix="$repo_root/external/linux-x64/native"
output="$repo_root/external/warm-reference"
[[ $(uname -s) == Linux ]] || { echo 'The warm reference requires Linux.' >&2; exit 1; }
[[ -f "$prefix/include/zimg.h" ]] || { echo 'Provision the pinned Linux tools first.' >&2; exit 1; }
export PKG_CONFIG_PATH="$prefix/lib/pkgconfig${PKG_CONFIG_PATH:+:$PKG_CONFIG_PATH}"
mkdir -p "$output"
read -r -a cflags <<< "$(pkg-config --cflags zimg)"
read -r -a libraries <<< "$(pkg-config --static --libs zimg)"
gcc -std=c11 -O3 -Wall -Wextra -Werror -DLOKAD_IMAGE_HORIZONTAL_ONLY=1 -DLOKAD_IMAGE_SAMPLE_DEPTH=8 "${cflags[@]}" "$repo_root/tools/WarmReference/zimg-bicubic-mixed8-warm.c" \
    -o "$output/zimg-bicubic-horizontal8-warm" "${libraries[@]}" -lstdc++ -lm
python3 - "$repo_root" "$output" "$prefix" <<'PY'
import hashlib,json,pathlib,subprocess,sys
root,output,prefix=map(pathlib.Path,sys.argv[1:])
digest=lambda p:hashlib.sha256(p.read_bytes()).hexdigest()
record=dict(schemaVersion=1,project='Lokad.Image',purpose='Original development-only warm FFmpeg scaler backend worker',
    sourceSha256=digest(root/'tools/WarmReference/zimg-bicubic-mixed8-warm.c'),
    buildScriptSha256=digest(root/'tools/WarmReference/build-bicubic-horizontal8-linux.sh'),
    workerSha256=digest(output/'zimg-bicubic-horizontal8-warm'),
    compiler=subprocess.check_output(['gcc','--version'],text=True).splitlines()[0],
    flags=['-std=c11','-O3','-Wall','-Wextra','-Werror','-DLOKAD_IMAGE_HORIZONTAL_ONLY=1','-DLOKAD_IMAGE_SAMPLE_DEPTH=8'],
    staticArchives={'libzimg.a':digest(prefix/'lib/libzimg.a')},
    zimgVersion=subprocess.check_output(['pkg-config','--modversion','zimg'],text=True).strip(),
    libraries=subprocess.check_output(['pkg-config','--static','--libs','zimg'],text=True).strip().replace(str(prefix),'{native-prefix}'))
(output/'bicubic-horizontal8-identity.json').write_text(json.dumps(record,indent=2)+'\n')
print(json.dumps(dict(built=True,workerSha256=record['workerSha256'])))
PY
