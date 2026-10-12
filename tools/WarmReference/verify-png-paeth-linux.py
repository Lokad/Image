"""Original correctness-only controls for one existing-raster Paeth RGBA8/16 input."""
import argparse
import hashlib
import json
import os
from pathlib import Path
import struct
import subprocess
import tempfile
import zlib

parser = argparse.ArgumentParser()
parser.add_argument('--cpu', type=int, default=0)
parser.add_argument('--depth', type=int, choices=(8,16), default=16)
args = parser.parse_args()
root = Path(__file__).resolve().parents[2]
depth = args.depth
bytes_per_pixel = depth//8*4
folder = root/f'artifacts/warm-reference/png-paeth-{depth}'
prepared = json.loads(folder.with_suffix('.json').read_text())
raw = (folder/'expected.raw').read_bytes()
data = (folder/'input.png').read_bytes()
digest = lambda value: hashlib.sha256(value).hexdigest()
assert digest(raw) == prepared['rasterSha256'] and digest(data) == prepared['inputSha256']
values = [(x*12347+y*7919+c*23459+x*y*101)&65535
          for y in range(259) for x in range(513) for c in range(4)]
values[3] = values[513*4-1] = 0
expected_raw = struct.pack('<'+'H'*len(values), *values) if depth == 16 else bytes((v+128)//257 for v in values)
assert raw == expected_raw
assert data[:8] == bytes((137, 80, 78, 71, 13, 10, 26, 10))
position = 8
compressed = bytearray()
types = []
while position < len(data):
    length = int.from_bytes(data[position:position+4], 'big')
    kind = data[position+4:position+8]
    payload = data[position+8:position+8+length]
    assert len(payload) == length and position+length+12 <= len(data)
    assert zlib.crc32(kind+payload) == int.from_bytes(data[position+8+length:position+12+length], 'big')
    types.append(kind)
    if kind == b'IHDR': assert payload == struct.pack('>IIBBBBB', 513, 259, depth, 6, 0, 0, 0)
    if kind == b'IDAT': compressed.extend(payload)
    if kind == b'IEND': assert not payload and position+12 == len(data)
    position += length+12
assert types == [b'IHDR', b'IDAT', b'IEND']
filtered = zlib.decompress(compressed)
position = 0
previous = bytes(513*bytes_per_pixel)
for y in range(259):
    assert filtered[position] == 4
    position += 1
    row = bytearray(513*bytes_per_pixel)
    for i in range(len(row)):
        left = row[i-bytes_per_pixel] if i >= bytes_per_pixel else 0
        up = previous[i]
        upper_left = previous[i-bytes_per_pixel] if i >= bytes_per_pixel else 0
        p = left+up-upper_left
        distances = (abs(p-left), abs(p-up), abs(p-upper_left))
        # Minimum-distance tie order is left, then up, then upper-left.
        prediction = (left, up, upper_left)[distances.index(min(distances))]
        row[i] = (filtered[position+i]+prediction)&255
        assert row[i] == raw[y*len(row)+((i^1) if depth == 16 else i)]
    position += len(row)
    previous = row
assert position == len(filtered)
allowed = sorted(os.sched_getaffinity(0))
assert args.cpu in allowed
other = next(cpu for cpu in allowed if cpu != args.cpu)
worker = root/'external/warm-reference/ffmpeg-png-warm'
with tempfile.TemporaryDirectory(prefix='lokad-image-paeth-controls-') as temporary:
    output = Path(temporary)/'decoded.raw'
    command = [str(worker), 'png-decode', str(folder/'input.png'), '513', '259', str(depth), str(output), str(folder/'expected.raw')]
    p = subprocess.run(['taskset','-c',str(args.cpu),*command], input='sample 1\nsample 2\ncheck\nsample 1\nquit\n', text=True, capture_output=True, timeout=30)
    assert p.returncode == 0 and not p.stderr, p.stderr
    rows = [json.loads(line) for line in p.stdout.splitlines()]
    ready = rows[0]
    assert ready['operation'] == 'png-decode' and ready['depth'] == depth
    assert ready['cpu'] == args.cpu and ready['affinityCount'] == ready['codecThreads'] == 1
    assert ready['threadType'] == ready['activeThreadType'] == 0 and ready['independentLiveOutputs']
    samples = [v for v in rows if v['state'] == 'sample']
    assert [v['operations'] for v in samples] == [1,2,1]
    assert [v['completedOperations'] for v in samples] == [4,6,8]
    assert rows[3] == dict(state='checked', exact=True, sourceUnchanged=True)
    assert output.read_bytes() == raw and (folder/'input.png').read_bytes() == data
    controls = [(f'{args.cpu},{other}',command,'quit\n'),(str(args.cpu),command,'sample 0\n'),
                (str(args.cpu),command,'sample 10000001\n'),(str(args.cpu),command,'sample 1 trailing\n'),
                (str(args.cpu),command,'unknown\n'),
                (str(args.cpu),[command[0],'png-adam7-decode',*command[2:]],'quit\n'),
                (str(args.cpu),[*command[:3],'512',*command[4:]],'quit\n'),
                (str(args.cpu),[*command[:5],str(8 if depth == 16 else 16),*command[6:]],'quit\n')]
    for affinity, native, protocol in controls:
        rejected = subprocess.run(['taskset','-c',affinity,*native], input=protocol, text=True, capture_output=True, timeout=30)
        assert rejected.returncode == 1 and rejected.stderr
report = dict(schemaVersion=1, project='Lokad.Image', correctnessOnly=True, complete=True,
              distinctSourceRasters=1, scoredCases=0, depth=depth, cpu=args.cpu, inputSha256=digest(data),
              rasterSha256=digest(raw), filteredBytes=len(filtered), exactSamples=513*259*4,
              allPaethRowsExact=True, alphaAndHiddenRgbExact=True, independentLiveOutputs=True,
              sourceUnchanged=True, countersExact=True, negativeControls=len(controls))
(root/f'artifacts/warm-reference/png-paeth-{depth}-controls.json').write_text(json.dumps(report, indent=2)+'\n')
print(json.dumps(report))
