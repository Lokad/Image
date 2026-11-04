#!/usr/bin/env bash
set -euo pipefail
repo_root=$(cd "$(dirname "${BASH_SOURCE[0]}")/../.." && pwd)
prefix="$repo_root/external/linux-x64/native"
output="$repo_root/external/warm-reference"
depth=${1:?Specify the existing control depth: 8 or 16}
[[ "$depth" == 8 || "$depth" == 16 ]] || { echo 'Depth must be 8 or 16.' >&2; exit 1; }
[[ $(uname -s) == Linux ]] || { echo 'The PNG one-shot reference requires Linux.' >&2; exit 1; }
[[ -f "$prefix/include/libavcodec/avcodec.h" ]] || { echo 'Provision the pinned Linux FFmpeg tools first.' >&2; exit 1; }
export PKG_CONFIG_PATH="$prefix/lib/pkgconfig${PKG_CONFIG_PATH:+:$PKG_CONFIG_PATH}"
mkdir -p "$output"

# Derive only this level-9 adapter in ignored storage. The established byte/word
# Optimal/Fastest workers, builders and original C source stay frozen.
python3 - "$repo_root" "$output" "$depth" <<'PY'
from pathlib import Path
import sys
root, output = map(Path, sys.argv[1:3]); depth = int(sys.argv[3])
source = (root/'tools/WarmReference/ffmpeg-png-warm.c').read_text()
changes = [
    ('if (work.depth != 8 || (strcmp(argv[1], "png-encode") && strcmp(argv[1], "png-fast16-encode")))',
     f'if (work.depth != {depth} || strcmp(argv[1], "png-smallest{depth}-encode"))'),
    ('One-shot PNG qualification is scoped to RGBA8 Optimal/Fastest.',
     f'One-shot PNG qualification is scoped to RGBA{depth} SmallestSize9.'),
]
if depth == 16:
    changes += [
        ('if (work->codec || work->packet) fail("One-shot state survived its operation.");',
         'if (work->codec || work->packet || work->prepared) fail("One-shot state survived its operation.");'),
        ('    work->packet = av_packet_alloc(); if (!work->packet) fail("Allocate fresh PNG packet wrapper failed.");',
         '    work->packet = av_packet_alloc(); if (!work->packet) fail("Allocate fresh PNG packet wrapper failed.");\n'
         '    work->prepared = frame(work->width, work->height, AV_PIX_FMT_RGBA64BE);'),
        ('        av_packet_free(&work->packet); avcodec_free_context(&work->codec);',
         '        av_packet_free(&work->packet); avcodec_free_context(&work->codec); av_frame_free(&work->prepared);'),
        ('        if (work.depth == 16 && !work.encode_rgb && !work.encode_gray && !work.encode_gray_alpha) work.prepared = frame(work.width, work.height, AV_PIX_FMT_RGBA64BE);',
         '        // The one-shot word frame is allocated in apply and freed after codec users finish.'),
        ('\\"borrowedResidentSource\\":%s,',
         '\\"borrowedResidentSource\\":%s,\\"wordWorkspacePreparationTimed\\":true,\\"wordWorkspaceRetainedBetweenOperations\\":false,'),
    ]
for before, after in changes:
    assert source.count(before) == 1, before
    source = source.replace(before, after)
(output/f'ffmpeg-png-one-shot9-{depth}-generated.c').write_text(source)
PY

read -r -a cflags <<< "$(pkg-config --cflags libavcodec libavutil libswscale libavfilter)"
read -r -a libraries <<< "$(pkg-config --static --libs libavcodec libavutil libswscale libavfilter)"
gcc -std=c11 -O3 -Wall -Wextra -Werror -DLOKAD_IMAGE_PNG_ONE_SHOT=1 "${cflags[@]}" \
    "$output/ffmpeg-png-one-shot9-$depth-generated.c" -o "$output/ffmpeg-png-one-shot9-$depth-warm" "${libraries[@]}"
python3 - "$repo_root" "$output" "$prefix" "$depth" <<'PY'
import hashlib, json, pathlib, subprocess, sys
root, output, prefix = map(pathlib.Path, sys.argv[1:4]); depth = int(sys.argv[4])
digest = lambda path: hashlib.sha256(path.read_bytes()).hexdigest()
record = dict(schemaVersion=1, project='Lokad.Image', purpose=f'Complete one-shot RGBA{depth} PNG level9 FFmpeg adapter',
    oneShotCodec=True, scope=f'Existing RGBA{depth} SmallestSize9 control only', inputDepth=depth, encoderLevel=9,
    wordWorkspacePreparationTimed=depth == 16, wordWorkspaceRetainedBetweenOperations=False,
    sourceSha256=digest(root/'tools/WarmReference/ffmpeg-png-warm.c'),
    buildScriptSha256=digest(root/'tools/WarmReference/build-png-one-shot9-linux.sh'),
    compiledSourceSha256=digest(output/f'ffmpeg-png-one-shot9-{depth}-generated.c'),
    workerSha256=digest(output/f'ffmpeg-png-one-shot9-{depth}-warm'),
    compiler=subprocess.check_output(['gcc','--version'], text=True).splitlines()[0],
    flags=['-std=c11','-O3','-Wall','-Wextra','-Werror','-DLOKAD_IMAGE_PNG_ONE_SHOT=1'],
    staticArchives={p.name: digest(p) for p in sorted((prefix/'lib').glob('*.a'))},
    libraries=subprocess.check_output(['pkg-config','--static','--libs','libavcodec','libavutil','libswscale','libavfilter'], text=True)
        .strip().replace(str(prefix.resolve()), '{native-prefix}').replace(str(root), '{repository}'))
(output/f'png-one-shot9-{depth}-identity.json').write_text(json.dumps(record,indent=2)+'\n')
print(json.dumps(dict(built=True, workerSha256=record['workerSha256'], oneShotCodec=True, depth=depth, encoderLevel=9)))
PY
