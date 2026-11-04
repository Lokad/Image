#!/usr/bin/env bash
set -euo pipefail
repo_root=$(cd "$(dirname "${BASH_SOURCE[0]}")/../.." && pwd)
prefix="$repo_root/external/linux-x64/native"
output="$repo_root/external/warm-reference"
[[ $(uname -s) == Linux ]] || { echo 'The PNG RGB16 reference requires Linux.' >&2; exit 1; }
[[ -f "$prefix/include/libavcodec/avcodec.h" ]] || { echo 'Provision the pinned Linux FFmpeg tools first.' >&2; exit 1; }
export PKG_CONFIG_PATH="$prefix/lib/pkgconfig${PKG_CONFIG_PATH:+:$PKG_CONFIG_PATH}"
mkdir -p "$output"

# Project only our adapter; preserve every existing worker and its identity.
# Move the identical RGB conversion preparation into the complete operation.
python3 - "$repo_root" "$output" <<'PY'
from pathlib import Path
import sys
root, output = map(Path, sys.argv[1:])
source = (root/'tools/WarmReference/ffmpeg-png-warm.c').read_text()
start = source.index('                work.prepared = frame(work.width, work.height, output_format);\n                work.conversion = sws_alloc_context();')
end = source.index('            }\n        }\n    } else {', start)
preparation = source[start:end].replace('output_format', 'AV_PIX_FMT_RGB48BE').replace('work.', 'work->')
changes = [
    ('if (work.depth != 8 || (strcmp(argv[1], "png-encode") && strcmp(argv[1], "png-fast16-encode")))',
     'if (work.depth != 16 || strcmp(argv[1], "png-rgb-encode"))'),
    ('One-shot PNG qualification is scoped to RGBA8 Optimal/Fastest.',
     'One-shot PNG qualification is scoped to RGB16 Optimal6.'),
    ('if (work->codec || work->packet) fail("One-shot state survived its operation.");',
     'if (work->codec || work->packet || work->prepared || work->conversion) fail("One-shot state survived its operation.");'),
    ('work->depth, 6, encode_level(work));', 'work->depth, 2, encode_level(work));'),
    ('    work->packet = av_packet_alloc(); if (!work->packet) fail("Allocate fresh PNG packet wrapper failed.");',
     '    work->packet = av_packet_alloc(); if (!work->packet) fail("Allocate fresh PNG packet wrapper failed.");\n' + preparation),
    ('        av_packet_free(&work->packet); avcodec_free_context(&work->codec);',
     '        av_packet_free(&work->packet); avcodec_free_context(&work->codec); av_frame_free(&work->prepared);\n'
     '        sws_free_context(&work->conversion);'),
    ('        if (work.encode_rgb || work.encode_gray || work.encode_gray_alpha) {',
     '        if (!LOKAD_IMAGE_PNG_ONE_SHOT && (work.encode_rgb || work.encode_gray || work.encode_gray_alpha)) {'),
    ('\\"borrowedResidentSource\\":%s,',
     '\\"borrowedResidentSource\\":%s,\\"wordWorkspacePreparationTimed\\":true,\\"wordWorkspaceRetainedBetweenOperations\\":false,'
     '\\"conversionPreparationTimed\\":true,\\"conversionRetainedBetweenOperations\\":false,'),
]
for before, after in changes:
    assert source.count(before) == 1, before
    source = source.replace(before, after)
(output/'ffmpeg-png-one-shot-rgb16-generated.c').write_text(source)
PY

read -r -a cflags <<< "$(pkg-config --cflags libavcodec libavutil libswscale libavfilter)"
read -r -a libraries <<< "$(pkg-config --static --libs libavcodec libavutil libswscale libavfilter)"
gcc -std=c11 -O3 -Wall -Wextra -Werror -DLOKAD_IMAGE_PNG_ONE_SHOT=1 "${cflags[@]}" \
    "$output/ffmpeg-png-one-shot-rgb16-generated.c" -o "$output/ffmpeg-png-one-shot-rgb16-warm" "${libraries[@]}"
python3 - "$repo_root" "$output" "$prefix" <<'PY'
import hashlib, json, pathlib, subprocess, sys
root, output, prefix = map(pathlib.Path, sys.argv[1:])
digest = lambda path: hashlib.sha256(path.read_bytes()).hexdigest()
record = dict(schemaVersion=1, project='Lokad.Image', purpose='Complete one-shot RGB16 PNG FFmpeg adapter',
    oneShotCodec=True, scope='Existing RGB16 Optimal6 control only', inputDepth=16, colorType=2, encoderLevel=6,
    wordWorkspacePreparationTimed=True, wordWorkspaceRetainedBetweenOperations=False,
    conversionPreparationTimed=True, conversionRetainedBetweenOperations=False,
    sourceSha256=digest(root/'tools/WarmReference/ffmpeg-png-warm.c'),
    buildScriptSha256=digest(root/'tools/WarmReference/build-png-one-shot-rgb16-linux.sh'),
    compiledSourceSha256=digest(output/'ffmpeg-png-one-shot-rgb16-generated.c'),
    workerSha256=digest(output/'ffmpeg-png-one-shot-rgb16-warm'),
    compiler=subprocess.check_output(['gcc','--version'], text=True).splitlines()[0],
    flags=['-std=c11','-O3','-Wall','-Wextra','-Werror','-DLOKAD_IMAGE_PNG_ONE_SHOT=1'],
    staticArchives={p.name: digest(p) for p in sorted((prefix/'lib').glob('*.a'))},
    libraries=subprocess.check_output(['pkg-config','--static','--libs','libavcodec','libavutil','libswscale','libavfilter'], text=True)
        .strip().replace(str(prefix.resolve()), '{native-prefix}').replace(str(root), '{repository}'))
(output/'png-one-shot-rgb16-identity.json').write_text(json.dumps(record,indent=2)+'\n')
print(json.dumps(dict(built=True, workerSha256=record['workerSha256'], oneShotCodec=True, depth=16, colorType=2, encoderLevel=6)))
PY
