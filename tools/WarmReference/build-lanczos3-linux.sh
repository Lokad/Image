#!/usr/bin/env bash
set -euo pipefail
repo_root=$(cd "$(dirname "${BASH_SOURCE[0]}")/../.." && pwd)
prefix="$repo_root/external/linux-x64/native"
output="$repo_root/external/warm-reference"
[[ $(uname -s) == Linux ]] || { echo 'Linux required.' >&2; exit 1; }
export PKG_CONFIG_PATH="$prefix/lib/pkgconfig${PKG_CONFIG_PATH:+:$PKG_CONFIG_PATH}"
mkdir -p "$output"
read -r -a cflags <<< "$(pkg-config --cflags zimg)"
read -r -a libraries <<< "$(pkg-config --static --libs zimg)"
gcc -std=c11 -O3 -Wall -Wextra -Werror "${cflags[@]}" "$repo_root/tools/WarmReference/zimg-lanczos3-warm.c" \
    -o "$output/zimg-lanczos3-warm" "${libraries[@]}" -lstdc++ -lm
python3 - "$repo_root" "$output" "$prefix" <<'PY'
import hashlib,json,pathlib,subprocess,sys
root,output,prefix=map(pathlib.Path,sys.argv[1:]);digest=lambda p:hashlib.sha256(p.read_bytes()).hexdigest()
record=dict(schemaVersion=1,project='Lokad.Image',purpose='Original Lanczos3 adapter to the FFmpeg zscale backend',
    sourceSha256=digest(root/'tools/WarmReference/zimg-lanczos3-warm.c'),buildScriptSha256=digest(root/'tools/WarmReference/build-lanczos3-linux.sh'),
    workerSha256=digest(output/'zimg-lanczos3-warm'),compiler=subprocess.check_output(['gcc','--version'],text=True).splitlines()[0],
    flags=['-std=c11','-O3','-Wall','-Wextra','-Werror','-lstdc++','-lm'],zimgArchiveSha256=digest(prefix/'lib/libzimg.a'))
(output/'lanczos3-identity.json').write_text(json.dumps(record,indent=2)+'\n');print(json.dumps(record))
PY
