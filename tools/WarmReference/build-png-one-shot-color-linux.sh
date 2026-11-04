#!/usr/bin/env bash
set -euo pipefail
repo_root=$(cd "$(dirname "${BASH_SOURCE[0]}")/../.." && pwd)
prefix="$repo_root/external/linux-x64/native"
output="$repo_root/external/warm-reference"
[[ $(uname -s) == Linux ]] || { echo 'The PNG reference requires Linux.' >&2; exit 1; }
[[ -f "$prefix/include/libavcodec/avcodec.h" ]] || { echo 'Provision the pinned Linux FFmpeg tools first.' >&2; exit 1; }
export PKG_CONFIG_PATH="$prefix/lib/pkgconfig${PKG_CONFIG_PATH:+:$PKG_CONFIG_PATH}"
mkdir -p "$output"

# Project only our adapter. Share its existing color preparation verbatim across
# layouts; move frame/SWS/filter-graph ownership into each complete operation.
python3 - "$repo_root" "$output" <<'PY'
from pathlib import Path
import sys
root, output = map(Path, sys.argv[1:])
source = (root/'tools/WarmReference/ffmpeg-png-warm.c').read_text()
start = source.index('            enum AVPixelFormat output_format = work.encode_gray_alpha')
end = source.index('        }\n    } else {', start)
preparation = source[start:end].replace('work.', 'work->').replace('gray16_graph(&work)', 'gray16_graph(work)')
source = source[:start] + '            if (!LOKAD_IMAGE_PNG_ONE_SHOT) prepare_color(&work);\n' + source[end:]
changes = [
    ('static void release(Work *work)', 'static void prepare_color(Work *work)\n{\n' + preparation + '}\nstatic void release(Work *work)'),
    ('if (work.depth != 8 || (strcmp(argv[1], "png-encode") && strcmp(argv[1], "png-fast16-encode")))',
     'if ((work.depth != 8 && work.depth != 16) || (!work.encode_rgb && !work.encode_gray && !work.encode_gray_alpha))'),
    ('One-shot PNG qualification is scoped to RGBA8 Optimal/Fastest.', 'One-shot PNG color qualification requires RGB/gray/gray-alpha Optimal6.'),
    ('if (work->codec || work->packet) fail("One-shot state survived its operation.");',
     'if (work->codec || work->packet || work->prepared || work->conversion || work->gray_graph) fail("One-shot state survived its operation.");'),
    ('work->depth, 6, encode_level(work));', 'work->depth, work->encode_gray_alpha ? 4 : work->encode_gray ? 0 : 2, encode_level(work));'),
    ('    work->packet = av_packet_alloc(); if (!work->packet) fail("Allocate fresh PNG packet wrapper failed.");',
     '    work->packet = av_packet_alloc(); if (!work->packet) fail("Allocate fresh PNG packet wrapper failed.");\n    prepare_color(work);'),
    ('        av_packet_free(&work->packet); avcodec_free_context(&work->codec);',
     '        av_packet_free(&work->packet); avcodec_free_context(&work->codec); av_frame_free(&work->prepared);\n'
     '        sws_free_context(&work->conversion); avfilter_graph_free(&work->gray_graph);\n'
     '        work->gray_input = work->gray_output = NULL;'),
    ('        if (work->gray_graph) av_frame_unref(work->prepared);', ''),
    ('work.gray_graph ? "true" : "false", work.gray_graph ? "true" : "false"',
     'work.encode_gray && work.depth == 16 ? "true" : "false", work.encode_gray && work.depth == 16 ? "true" : "false"'),
    ('work.key_graph || work.gray_graph ? 1 : 0',
     'work.key_graph || (work.encode_gray && work.depth == 16) ? 1 : 0'),
    ('\\"borrowedResidentSource\\":%s,',
     '\\"borrowedResidentSource\\":%s,\\"colorWorkspacePreparationTimed\\":true,\\"colorWorkspaceRetainedBetweenOperations\\":false,'
     '\\"wordWorkspacePreparationTimed\\":true,\\"wordWorkspaceRetainedBetweenOperations\\":false,'),
]
for before, after in changes:
    assert source.count(before) == 1, before
    source = source.replace(before, after)
(output/'ffmpeg-png-one-shot-color-generated.c').write_text(source)
PY
read -r -a cflags <<< "$(pkg-config --cflags libavcodec libavutil libswscale libavfilter)"
read -r -a libraries <<< "$(pkg-config --static --libs libavcodec libavutil libswscale libavfilter)"
gcc -std=c11 -O3 -Wall -Wextra -Werror -DLOKAD_IMAGE_PNG_ONE_SHOT=1 "${cflags[@]}" \
    "$output/ffmpeg-png-one-shot-color-generated.c" -o "$output/ffmpeg-png-one-shot-color-warm" "${libraries[@]}"
python3 - "$repo_root" "$output" "$prefix" <<'PY'
import hashlib, json, pathlib, subprocess, sys
root, output, prefix = map(pathlib.Path, sys.argv[1:])
digest = lambda path: hashlib.sha256(path.read_bytes()).hexdigest()
record = dict(schemaVersion=1, project='Lokad.Image', purpose='Complete one-shot color PNG FFmpeg adapter',
    oneShotCodec=True, scope='Existing RGB8, gray8/16 and gray-alpha8 Optimal6 controls', encoderLevel=6,
    colorWorkspacePreparationTimed=True, colorWorkspaceRetainedBetweenOperations=False,
    wordWorkspacePreparationTimed=True, wordWorkspaceRetainedBetweenOperations=False,
    identicalConversionOrPackingPreserved=True,
    sourceSha256=digest(root/'tools/WarmReference/ffmpeg-png-warm.c'),
    buildScriptSha256=digest(root/'tools/WarmReference/build-png-one-shot-color-linux.sh'),
    compiledSourceSha256=digest(output/'ffmpeg-png-one-shot-color-generated.c'),
    workerSha256=digest(output/'ffmpeg-png-one-shot-color-warm'),
    compiler=subprocess.check_output(['gcc','--version'], text=True).splitlines()[0],
    flags=['-std=c11','-O3','-Wall','-Wextra','-Werror','-DLOKAD_IMAGE_PNG_ONE_SHOT=1'],
    staticArchives={p.name: digest(p) for p in sorted((prefix/'lib').glob('*.a'))},
    libraries=subprocess.check_output(['pkg-config','--static','--libs','libavcodec','libavutil','libswscale','libavfilter'], text=True)
        .strip().replace(str(prefix.resolve()), '{native-prefix}').replace(str(root), '{repository}'))
(output/'png-one-shot-color-identity.json').write_text(json.dumps(record,indent=2)+'\n')
print(json.dumps(dict(built=True, workerSha256=record['workerSha256'], oneShotCodec=True, encoderLevel=6)))
PY
