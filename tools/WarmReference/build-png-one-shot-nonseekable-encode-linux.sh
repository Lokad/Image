#!/usr/bin/env bash
set -euo pipefail
repo_root=$(cd "$(dirname "${BASH_SOURCE[0]}")/../.." && pwd)
prefix="$repo_root/external/linux-x64/native"
output="$repo_root/external/warm-reference"
[[ $(uname -s) == Linux ]] || { echo 'The PNG one-shot output reference requires Linux.' >&2; exit 1; }
[[ -f "$prefix/include/libavformat/avio.h" ]] || { echo 'Provision the pinned FFmpeg tools first.' >&2; exit 1; }
export PKG_CONFIG_PATH="$prefix/lib/pkgconfig${PKG_CONFIG_PATH:+:$PKG_CONFIG_PATH}"
mkdir -p "$output"

# Project our owned adapter without replacing its retained-context comparison.
# Delivery, growing output and independent ownership stay inside apply.
python3 - "$repo_root" "$output" <<'PY'
from pathlib import Path
import sys
root, output = map(Path, sys.argv[1:])
source = (root/'tools/WarmReference/ffmpeg-png-nonseekable-encode-warm.c').read_text()
start = source.index('    const AVCodec *codec=avcodec_find_encoder(AV_CODEC_ID_PNG);')
end = source.index('    work.source=av_frame_alloc();', start)
configuration = source[start:end].replace('work.', 'work->')
source = source[:start] + source[end:]
entry = 'static Output apply(Work *work)\n{\n'
assert source.count(entry) == 1
prepare = ('static void prepare_codec(Work *work)\n{\n'
    '    if (work->codec || work->packet) fail("One-shot codec state survived its operation.");\n'
    + configuration + '    work->packet=av_packet_alloc();\n'
    '    if (!work->packet) fail("Allocate fresh packet wrapper failed.");\n}\n')
source = source.replace(entry, prepare + entry + '    prepare_codec(work);\n')
changes = [
    ('    work.source=av_frame_alloc(); work.packet=av_packet_alloc(); if (!work.source || !work.packet) fail("Frame/packet wrapper allocation failed.");',
     '    work.source=av_frame_alloc(); if (!work.source) fail("Source frame allocation failed.");'),
    ('av_packet_unref(work->packet);', 'av_packet_free(&work->packet); avcodec_free_context(&work->codec);'),
    (r'\"freshNonSeekableOutput\":true,',
     r'\"codecPreparationTimed\":true,\"packetWrapperPreparationTimed\":true,\"codecRetainedBetweenOperations\":false,\"freshNonSeekableOutput\":true,'),
]
for before, after in changes:
    assert source.count(before) == 1, before
    source = source.replace(before, after)
(output/'ffmpeg-png-one-shot-nonseekable-encode-generated.c').write_text(source)
PY

read -r -a cflags <<< "$(pkg-config --cflags libavformat libavcodec libavutil)"
read -r -a libraries <<< "$(pkg-config --static --libs libavformat libavcodec libavutil)"
gcc -std=c11 -O3 -Wall -Wextra -Werror "${cflags[@]}" \
    "$output/ffmpeg-png-one-shot-nonseekable-encode-generated.c" \
    -o "$output/ffmpeg-png-one-shot-nonseekable-encode-warm" "${libraries[@]}"
python3 - "$repo_root" "$output" "$prefix" <<'PY'
import hashlib, json, pathlib, subprocess, sys
root, output, prefix = map(pathlib.Path, sys.argv[1:])
digest = lambda path: hashlib.sha256(path.read_bytes()).hexdigest()
record = dict(schemaVersion=1, project='Lokad.Image', purpose='Complete one-shot PNG encoding to independent nonseekable output',
    sourceSha256=digest(root/'tools/WarmReference/ffmpeg-png-nonseekable-encode-warm.c'),
    buildScriptSha256=digest(root/'tools/WarmReference/build-png-one-shot-nonseekable-encode-linux.sh'),
    compiledSourceSha256=digest(output/'ffmpeg-png-one-shot-nonseekable-encode-generated.c'),
    workerSha256=digest(output/'ffmpeg-png-one-shot-nonseekable-encode-warm'),
    oneShotCodec=True, codecPreparationTimed=True, packetWrapperPreparationTimed=True,
    codecRetainedBetweenOperations=False,
    compiler=subprocess.check_output(['gcc','--version'],text=True).splitlines()[0],
    flags=['-std=c11','-O3','-Wall','-Wextra','-Werror'],
    staticArchives={p.name:digest(p) for p in sorted((prefix/'lib').glob('*.a'))},
    libraries=subprocess.check_output(['pkg-config','--static','--libs','libavformat','libavcodec','libavutil'],text=True)
        .strip().replace(str(prefix.resolve()),'{native-prefix}').replace(str(root),'{repository}'))
(output/'png-one-shot-nonseekable-encode-identity.json').write_text(json.dumps(record,indent=2)+'\n')
print(json.dumps(dict(built=True,workerSha256=record['workerSha256'])))
PY
