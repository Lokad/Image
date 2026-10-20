#!/usr/bin/env bash
set -euo pipefail
repo_root=$(cd "$(dirname "${BASH_SOURCE[0]}")/../.." && pwd)
prefix="$repo_root/external/linux-x64/native"
output="$repo_root/external/warm-reference"
export PKG_CONFIG_PATH="$prefix/lib/pkgconfig${PKG_CONFIG_PATH:+:$PKG_CONFIG_PATH}"
read -r -a cflags <<< "$(pkg-config --cflags libavcodec libavutil libswscale libavfilter)"
read -r -a libraries <<< "$(pkg-config --static --libs libavcodec libavutil libswscale libavfilter)"
gcc -std=c11 -O3 -Wall -Wextra -Werror -ffunction-sections -fdata-sections -Wl,--gc-sections -s \
    "${cflags[@]}" "$repo_root/tools/WarmReference/ffmpeg-png-retained2-warm.c" \
    -o "$output/ffmpeg-png-retained2-warm" "${libraries[@]}"
python3 - "$repo_root" "$output" <<'PY'
import hashlib,json,pathlib,subprocess,sys
r,o=map(pathlib.Path,sys.argv[1:]);sha=lambda p:hashlib.sha256(p.read_bytes()).hexdigest();prefix=r/'external/linux-x64/native'
v=dict(schemaVersion=1,purpose='Original complete two-retained-output public FFmpeg PNG adapter',
    workerSha256=sha(o/'ffmpeg-png-retained2-warm'),
    sourceSha256=sha(r/'tools/WarmReference/ffmpeg-png-retained2-warm.c'),
    includedSourceSha256=sha(r/'tools/WarmReference/ffmpeg-png-warm.c'),
    buildScriptSha256=sha(r/'tools/WarmReference/build-png-retained2-linux.sh'),
    compiler=subprocess.check_output(['gcc','--version'],text=True).splitlines()[0],
    flags=['-std=c11','-O3','-Wall','-Wextra','-Werror','-ffunction-sections','-fdata-sections','-Wl,--gc-sections','-s'],
    staticArchives={p.name:sha(p) for p in sorted((prefix/'lib').glob('*.a'))},
    libraries=subprocess.check_output(['pkg-config','--static','--libs','libavcodec','libavutil','libswscale','libavfilter'],text=True).strip().replace(str(prefix.resolve()),'{native-prefix}').replace(str(r),'{repository}'))
(o/'png-retained2-identity.json').write_text(json.dumps(v,indent=2)+'\n');print(json.dumps(v))
PY
