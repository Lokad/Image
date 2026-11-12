#!/usr/bin/env bash
set -euo pipefail
repo_root=$(cd "$(dirname "${BASH_SOURCE[0]}")/../.." && pwd)
prefix="$repo_root/external/linux-x64/native"
output="$repo_root/external/warm-reference"
[[ $(uname -s) == Linux ]] || { echo 'The PNG reference requires Linux.' >&2; exit 1; }
[[ -f "$prefix/include/libavcodec/avcodec.h" ]] || { echo 'Provision the pinned Linux tools first.' >&2; exit 1; }
export PKG_CONFIG_PATH="$prefix/lib/pkgconfig${PKG_CONFIG_PATH:+:$PKG_CONFIG_PATH}"
mkdir -p "$output"

# Project our authored adapter into ignored storage. Original retained-context
# workers and existing one-shot workers keep their sources and identities.
python3 - "$repo_root" "$output" <<'PY'
from pathlib import Path
import sys
root, output = map(Path, sys.argv[1:])
original = (root/'tools/WarmReference/ffmpeg-png-warm.c').read_text()
for depth in (8, 16):
    source = original
    def replace(before, after):
        global source
        assert source.count(before) == 1, before
        source = source.replace(before, after)
    replace('if (work.depth != 8 || (strcmp(argv[1], "png-encode") && strcmp(argv[1], "png-fast16-encode")))',
            f'if (work.depth != {depth} || strcmp(argv[1], "png-store-encode"))')
    replace('One-shot PNG qualification is scoped to RGBA8 Optimal/Fastest.',
            f'One-shot PNG qualification is scoped to stored RGBA{depth}.')
    replace('if (work.encode_stored && work.depth != 8)',
            f'if (work.encode_stored && work.depth != {depth})')
    replace('Stored PNG encode reference requires RGBA8 input.',
            f'Stored PNG encode reference requires RGBA{depth} input.')
    if depth == 16:
        replace('if (work->codec || work->packet) fail("One-shot state survived its operation.");',
                'if (work->codec || work->packet || work->prepared) fail("One-shot state survived its operation.");')
        replace('    work->packet = av_packet_alloc(); if (!work->packet) fail("Allocate fresh PNG packet wrapper failed.");',
                '    work->packet = av_packet_alloc(); if (!work->packet) fail("Allocate fresh PNG packet wrapper failed.");\n'
                '    work->prepared = frame(work->width, work->height, AV_PIX_FMT_RGBA64BE);')
        replace('        av_packet_free(&work->packet); avcodec_free_context(&work->codec);',
                '        av_packet_free(&work->packet); avcodec_free_context(&work->codec); av_frame_free(&work->prepared);')
        replace('        if (work.depth == 16 && !work.encode_rgb && !work.encode_gray && !work.encode_gray_alpha) work.prepared = frame(work.width, work.height, AV_PIX_FMT_RGBA64BE);',
                '        // The word conversion frame belongs to each complete operation.')
        replace('\\"borrowedResidentSource\\":%s,',
                '\\"borrowedResidentSource\\":%s,\\"wordWorkspacePreparationTimed\\":true,\\"wordWorkspaceRetainedBetweenOperations\\":false,')
        source = source.replace('"png-store-encode"', '"png-store16-encode"')
    (output/f'ffmpeg-png-one-shot-store{depth}-generated.c').write_text(source)
PY

read -r -a cflags <<< "$(pkg-config --cflags libavcodec libavutil libswscale libavfilter)"
read -r -a libraries <<< "$(pkg-config --static --libs libavcodec libavutil libswscale libavfilter)"
for depth in 8 16; do
    gcc -std=c11 -O3 -Wall -Wextra -Werror -DLOKAD_IMAGE_PNG_ONE_SHOT=1 "${cflags[@]}" \
        "$output/ffmpeg-png-one-shot-store$depth-generated.c" \
        -o "$output/ffmpeg-png-one-shot-store$depth-warm" "${libraries[@]}"
done
python3 - "$repo_root" "$output" "$prefix" <<'PY'
import hashlib, json, pathlib, subprocess, sys
root, output, prefix = map(pathlib.Path, sys.argv[1:])
digest = lambda path: hashlib.sha256(path.read_bytes()).hexdigest()
for depth in (8, 16):
    record = dict(schemaVersion=1, project='Lokad.Image', purpose='Complete one-shot stored PNG FFmpeg adapter',
        oneShotCodec=True, scope=f'Existing RGBA{depth} NoCompression control only', encoderLevel=0, inputDepth=depth,
        wordWorkspacePreparationTimed=depth == 16, wordWorkspaceRetainedBetweenOperations=False,
        sourceSha256=digest(root/'tools/WarmReference/ffmpeg-png-warm.c'),
        buildScriptSha256=digest(root/'tools/WarmReference/build-png-one-shot-stored-linux.sh'),
        compiledSourceSha256=digest(output/f'ffmpeg-png-one-shot-store{depth}-generated.c'),
        workerSha256=digest(output/f'ffmpeg-png-one-shot-store{depth}-warm'),
        compiler=subprocess.check_output(['gcc','--version'], text=True).splitlines()[0],
        flags=['-std=c11','-O3','-Wall','-Wextra','-Werror','-DLOKAD_IMAGE_PNG_ONE_SHOT=1'],
        staticArchives={p.name: digest(p) for p in sorted((prefix/'lib').glob('*.a'))},
        libraries=subprocess.check_output(['pkg-config','--static','--libs','libavcodec','libavutil','libswscale','libavfilter'], text=True)
            .strip().replace(str(prefix.resolve()), '{native-prefix}').replace(str(root), '{repository}'))
    (output/f'png-one-shot-store{depth}-identity.json').write_text(json.dumps(record,indent=2)+'\n')
    print(json.dumps(dict(depth=depth, built=True, workerSha256=record['workerSha256'])))
PY
