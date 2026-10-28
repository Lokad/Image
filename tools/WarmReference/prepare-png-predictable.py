"""Prepare one original predictable RGBA16 PNG control; never measure startup/IO."""
import array
import filecmp
import hashlib
import json
import os
from pathlib import Path
import shutil
import subprocess
import sys

root = Path(__file__).resolve().parents[2]
os.chdir(root)
if sys.byteorder != 'little' or not hasattr(os, 'sched_setaffinity'):
    raise RuntimeError('Use the dedicated little-endian Linux benchmark environment.')
cpu = int(os.environ.get('LOKAD_IMAGE_WARM_CPU', '0'))
os.sched_setaffinity(0, {cpu})
folder = Path('artifacts/warm-reference/png-predictable16')
folder.mkdir(parents=True, exist_ok=True)
raw = folder/'input.rgba64le'
digest = lambda p: hashlib.sha256(Path(p).read_bytes()).hexdigest()
input_hash = '14c4457890991939f2798e6897275eb6c0d5dc53d88e9ede82b3657aaf2bfc95'
if not raw.exists():
    with raw.open('xb') as output:
        for y in range(3000):
            row = array.array('H')
            for x in range(3000):
                original = 2999-x
                row.extend((16*original, 16*y, 8*(original+y), 65535))
            row.tofile(output)
assert raw.stat().st_size == 72_000_000 and digest(raw) == input_hash
ffmpeg = shutil.which('ffmpeg')
assert ffmpeg and Path(ffmpeg).resolve().is_relative_to((root/'external/linux-x64').resolve())
worker = Path('external/warm-reference/ffmpeg-png-predictable-warm')
assert worker.is_file(), 'Run build-png-predictable-linux.sh first.'
packets = []
commands = []
for mode, extra in [('normal', []), ('scalar', ['-cpuflags', '0'])]:
    png = folder/('native-'+mode+'.png')
    decoded = folder/('decoded-'+mode+'.rgba64le')
    encode = [ffmpeg, '-hide_banner', '-loglevel', 'error', '-y', *extra,
              '-threads', '1', '-filter_threads', '1', '-f', 'rawvideo', '-pixel_format', 'rgba64le',
              '-video_size', '3000x3000', '-i', str(raw), '-frames:v', '1', '-pix_fmt', 'rgba64be',
              '-c:v', 'png', '-compression_level', '6', '-pred', 'mixed', '-threads', '1', str(png)]
    subprocess.run(encode, check=True)
    commands.append([value.replace(ffmpeg, '{pinned-ffmpeg}') for value in encode])
    subprocess.run([ffmpeg, '-hide_banner', '-loglevel', 'error', '-y', *extra,
                    '-threads', '1', '-filter_threads', '1', '-i', str(png), '-frames:v', '1',
                    '-pix_fmt', 'rgba64le', '-f', 'rawvideo', '-threads', '1', str(decoded)], check=True)
    assert filecmp.cmp(raw, decoded, shallow=False) and png.stat().st_size < 2*1024*1024
    destination = folder/('adapter-'+mode+'.png')
    result = subprocess.run([str(worker), 'png-encode', str(raw), '3000', '3000', '16', str(destination)],
                            input=b'check\nquit\n', env={**os.environ, **({'LOKAD_IMAGE_WARM_NATIVE_SCALAR': '1'} if mode == 'scalar' else {})},
                            capture_output=True, check=True)
    rows = [json.loads(line) for line in result.stdout.splitlines()]
    ready = rows[0]
    assert ready['width'] == ready['height'] == 3000 and ready['depth'] == 16
    assert ready['compressionLevel'] == 6 and ready['filter'] == 5
    assert ready['codecThreads'] == ready['affinityCount'] == 1 and ready['independentLiveOutputs']
    assert ready['scalarMode'] == (mode == 'scalar') and rows[1]['exact'] and rows[1]['sourceUnchanged']
    assert digest(destination) == digest(png)
    packets.append(dict(mode=mode, bytes=png.stat().st_size, sha256=digest(png), everySampleExact=True))
assert packets[0]['sha256'] == packets[1]['sha256']
proof = dict(schemaVersion=1, project='Lokad.Image', caseId='EncodePredictable16', timing=False,
             width=3000, height=3000, depth=16, inputBytes=raw.stat().st_size, inputSha256=input_hash,
             recipe='Horizontal flip of R=16*x,G=16*y,B=8*(x+y),A=65535',
             reference='Pinned FFmpeg PNG, level6/pred=mixed, one CPU. Exact pixels and complete packet <2MiB.',
             ffmpegSha256=digest(ffmpeg), workerSha256=digest(worker), packets=packets, commands=commands)
(folder/'proof.json').write_text(json.dumps(proof, indent=2)+'\n')
print(json.dumps(dict(prepared=True, timing=False, inputSha256=input_hash, packets=packets)))
