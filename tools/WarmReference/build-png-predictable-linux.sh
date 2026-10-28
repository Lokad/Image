#!/usr/bin/env bash
set -euo pipefail
repo_root=$(cd "$(dirname "${BASH_SOURCE[0]}")/../.." && pwd)
prefix="$repo_root/external/linux-x64/native"
output="$repo_root/external/warm-reference"
[[ $(uname -s) == Linux ]] || { echo 'The warm PNG reference requires Linux.' >&2; exit 1; }
export PKG_CONFIG_PATH="$prefix/lib/pkgconfig${PKG_CONFIG_PATH:+:$PKG_CONFIG_PATH}"
mkdir -p "$output"

# Derive one original adapter variant without changing the frozen None worker
# or its source/build identity. The only changes select mixed prediction and
# report that selection; all normalization/packet ownership code is shared.
python3 - "$repo_root" "$output" <<'PY'
from pathlib import Path
import sys
root, output = map(Path, sys.argv[1:])
source = (root/'tools/WarmReference/ffmpeg-png-warm.c').read_text()
for old, new in [
    ('av_opt_set(value->priv_data, "pred", "none", 0), "Set PNG filter None"',
     'av_opt_set(value->priv_data, "pred", "mixed", 0), "Set PNG mixed prediction"'),
    (': -1, work.encode ? 0 : -1,', ': -1, work.encode ? 5 : -1,')]:
    assert source.count(old) == 1, 'Original PNG adapter changed; review the variant.'
    source = source.replace(old, new)
(output/'ffmpeg-png-predictable-generated.c').write_text(source)
PY
read -r -a cflags <<< "$(pkg-config --cflags libavcodec libavutil libswscale libavfilter)"
read -r -a libraries <<< "$(pkg-config --static --libs libavcodec libavutil libswscale libavfilter)"
gcc -std=c11 -O3 -Wall -Wextra -Werror "${cflags[@]}" "$output/ffmpeg-png-predictable-generated.c" \
    -o "$output/ffmpeg-png-predictable-warm" "${libraries[@]}"
python3 - "$repo_root" "$output" "$prefix" <<'PY'
import hashlib, json, pathlib, subprocess, sys
root, output, prefix = map(pathlib.Path, sys.argv[1:])
digest = lambda path: hashlib.sha256(path.read_bytes()).hexdigest()
record = dict(schemaVersion=1, project='Lokad.Image', purpose='Original warm mixed-prediction PNG adapter',
    sourceSha256=digest(root/'tools/WarmReference/ffmpeg-png-warm.c'),
    generatedSourceSha256=digest(output/'ffmpeg-png-predictable-generated.c'),
    buildScriptSha256=digest(root/'tools/WarmReference/build-png-predictable-linux.sh'),
    workerSha256=digest(output/'ffmpeg-png-predictable-warm'),
    compiler=subprocess.check_output(['gcc','--version'], text=True).splitlines()[0],
    flags=['-std=c11','-O3','-Wall','-Wextra','-Werror'],
    staticArchives={p.name:digest(p) for p in sorted((prefix/'lib').glob('*.a'))},
    libraries=subprocess.check_output(['pkg-config','--static','--libs','libavcodec','libavutil','libswscale','libavfilter'], text=True)
        .strip().replace(str(prefix.resolve()), '{native-prefix}').replace(str(root), '{repository}'))
(output/'png-predictable-identity.json').write_text(json.dumps(record,indent=2)+'\n')
print(json.dumps(dict(built=True, workerSha256=record['workerSha256'])))
PY
