"""Exact/negative controls only. These are not statistical performance measurements."""
import argparse
import hashlib
import json
import os
from pathlib import Path
import struct
import subprocess
import tempfile

parser = argparse.ArgumentParser()
parser.add_argument('--cpu', type=int, default=0)
arguments = parser.parse_args()
root = Path(__file__).resolve().parents[2]
worker = root/'external/warm-reference/ffmpeg-warm'
allowed = sorted(os.sched_getaffinity(0))
assert arguments.cpu in allowed
other = next((cpu for cpu in allowed if cpu != arguments.cpu), None)
assert other is not None, 'Two allowed CPUs are needed for the negative affinity control.'
width, height = 513, 259

def raw(depth):
    # Same original asymmetric-v1 source as HorizontalFlipInput.Create, including
    # zero-alpha hidden RGB at both first-row ends. Exactly two inputs total.
    values = [((x*12347+y*7919+channel*23459+x*y*101)&65535)
              for y in range(height) for x in range(width) for channel in range(4)]
    if depth == 8:
        values = [(value+128)//257 for value in values]
    values[3] = values[width*4-1] = 0
    return bytes(values) if depth == 8 else struct.pack('<'+'H'*len(values), *values)

records = []
with tempfile.TemporaryDirectory(prefix='lokad-image-warm-controls-') as temporary:
    directory = Path(temporary)
    for depth in (8, 16):
        pixels = raw(depth)
        pixel_bytes = depth//8*4
        expected = bytearray(len(pixels))
        for y in range(height):
            for x in range(width):
                source = (y*width+x)*pixel_bytes
                target = (y*width+width-x-1)*pixel_bytes
                expected[target:target+pixel_bytes] = pixels[source:source+pixel_bytes]
        source, output = directory/f'input-{depth}.raw', directory/f'output-{depth}.raw'
        source.write_bytes(pixels)
        command = [str(worker),'hflip',str(source),str(width),str(height),str(depth),str(output)]
        completed = subprocess.run(['taskset','-c',str(arguments.cpu),*command],
            input='sample 1\nsample 2\ncheck\nsample 1\nquit\n',text=True,capture_output=True,timeout=30)
        assert completed.returncode == 0 and not completed.stderr, completed.stderr
        rows = [json.loads(line) for line in completed.stdout.splitlines()]
        assert len(rows) == 5 and rows[0]['state'] == 'ready' and rows[0]['affinityCount'] == 1
        assert rows[0]['cpu'] == arguments.cpu and rows[0]['filterThreads'] == 1 and rows[0]['threadType'] == 0
        samples = [row for row in rows if row['state'] == 'sample']
        assert [row['operations'] for row in samples] == [1,2,1]
        assert [row['completedOperations'] for row in samples] == [2,4,6]
        assert all(row['elapsedNanoseconds'] > 0 and row['cpu'] == arguments.cpu for row in samples)
        assert rows[3] == dict(state='checked',exact=True,sourceUnchanged=True)
        assert output.read_bytes() == expected and source.read_bytes() == pixels
        controls = [(['taskset','-c',f'{arguments.cpu},{other}',*command], 'quit\n', 'affinity'),
                    (['taskset','-c',str(arguments.cpu),*command], 'sample 0\n', 'zero count'),
                    (['taskset','-c',str(arguments.cpu),*command], 'sample 10000001\n', 'count bound'),
                    (['taskset','-c',str(arguments.cpu),*command], 'sample 1 trailing\n', 'trailing protocol'),
                    (['taskset','-c',str(arguments.cpu),*command], 'unknown\n', 'unknown protocol')]
        for native_command, protocol, name in controls:
            rejected = subprocess.run(native_command,input=protocol,text=True,capture_output=True,timeout=30)
            assert rejected.returncode == 1 and rejected.stderr, name
        records.append(dict(depth=depth, inputSha256=hashlib.sha256(pixels).hexdigest(),
            outputSha256=hashlib.sha256(expected).hexdigest(), exactPixels=width*height,
            countersExact=True, sourceUnchanged=True, negativeControls=len(controls)))
report = dict(schemaVersion=1, project='Lokad.Image', correctnessOnly=True, complete=True,
    cpu=arguments.cpu, distinctInputs=2, cases=records)
destination = root/'artifacts/warm-reference'
destination.mkdir(parents=True, exist_ok=True)
(destination/'controls.json').write_text(json.dumps(report,indent=2)+'\n')
print(json.dumps(report))
