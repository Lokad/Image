"""Original exact/negative controls; no statistical timing numbers are published."""
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
arguments = parser.parse_args()
root = Path(__file__).resolve().parents[2]
worker = root/'external/warm-reference/ffmpeg-png-warm'
allowed = sorted(os.sched_getaffinity(0))
assert arguments.cpu in allowed
other = next(cpu for cpu in allowed if cpu != arguments.cpu)
width, height = 513, 259

def pixels(depth):
    values = [((x*12347+y*7919+channel*23459+x*y*101)&65535)
              for y in range(height) for x in range(width) for channel in range(4)]
    if depth == 8: values = [(value+128)//257 for value in values]
    values[3] = values[width*4-1] = 0
    return bytes(values) if depth == 8 else struct.pack('<'+'H'*len(values),*values)

def validate_png(data, raw, depth):
    assert data[:8] == bytes((137,80,78,71,13,10,26,10))
    position, compressed, seen_end = 8, bytearray(), False
    while position < len(data):
        length = int.from_bytes(data[position:position+4], 'big')
        kind = data[position+4:position+8]; payload = data[position+8:position+8+length]
        assert len(payload) == length and position+length+12 <= len(data)
        assert zlib.crc32(kind+payload) == int.from_bytes(data[position+8+length:position+12+length], 'big')
        if kind == b'IHDR': assert payload == struct.pack('>IIBBBBB',width,height,depth,6,0,0,0)
        elif kind == b'IDAT': compressed.extend(payload)
        elif kind == b'IEND': assert not payload and position+12 == len(data); seen_end = True
        position += length+12
    assert seen_end
    unfiltered = zlib.decompress(compressed)
    row_bytes = width*4*(depth//8)
    assert len(unfiltered) == (row_bytes+1)*height
    for y in range(height):
        offset = y*(row_bytes+1)
        assert unfiltered[offset] == 0
        row = unfiltered[offset+1:offset+1+row_bytes]
        if depth == 16: row = b''.join(row[i:i+2][::-1] for i in range(0,len(row),2))
        assert row == raw[y*row_bytes:(y+1)*row_bytes]
    return len(compressed)

def run(command, protocol):
    return subprocess.run(['taskset','-c',str(arguments.cpu),*command],input=protocol,
        text=True,capture_output=True,timeout=30)

records = []
with tempfile.TemporaryDirectory(prefix='lokad-image-png-controls-') as temporary:
    folder = Path(temporary)
    for depth in (8,16):
        raw = pixels(depth)
        raster, encoded, decoded = (folder/f'{depth}.{extension}' for extension in ('raw','png','decoded'))
        raster.write_bytes(raw)
        commands = [([str(worker),'png-encode',str(raster),str(width),str(height),str(depth),str(encoded)],'encode'),
                    ([str(worker),'png-decode',str(encoded),str(width),str(height),str(depth),str(decoded),str(raster)],'decode')]
        for command,operation in commands:
            completed = run(command,'sample 1\nsample 2\ncheck\nsample 1\nquit\n')
            assert completed.returncode == 0 and not completed.stderr, (operation,depth,completed.stderr)
            rows = [json.loads(line) for line in completed.stdout.splitlines()]
            ready = rows[0]
            assert ready['state'] == 'ready' and ready['operation'] == 'png-'+operation
            assert ready['cpu'] == arguments.cpu and ready['affinityCount'] == 1
            assert ready['codecThreads'] == 1 and ready['threadType'] == ready['activeThreadType'] == 0
            assert ready['independentLiveOutputs'] and ready['initialCompletedOperations'] == 3
            if operation == 'encode': assert ready['compressionLevel'] == 6 and ready['filter'] == 0
            samples = [row for row in rows if row['state'] == 'sample']
            assert [row['operations'] for row in samples] == [1,2,1]
            assert [row['completedOperations'] for row in samples] == [4,6,8]
            assert all(row['elapsedNanoseconds'] > 0 and row['cpu'] == arguments.cpu for row in samples)
            assert rows[3] == dict(state='checked',exact=True,sourceUnchanged=True)
            idat_bytes = validate_png(encoded.read_bytes(),raw,depth)
            if operation == 'decode': assert decoded.read_bytes() == raw
            assert raster.read_bytes() == raw
            controls = [(['taskset','-c',f'{arguments.cpu},{other}',*command],'quit\n','affinity'),
                        (['taskset','-c',str(arguments.cpu),*command],'sample 0\n','zero count'),
                        (['taskset','-c',str(arguments.cpu),*command],'sample 10000001\n','count bound'),
                        (['taskset','-c',str(arguments.cpu),*command],'sample 1 trailing\n','trailing protocol'),
                        (['taskset','-c',str(arguments.cpu),*command],'unknown\n','unknown protocol')]
            for native,protocol,name in controls:
                rejected = subprocess.run(native,input=protocol,text=True,capture_output=True,timeout=30)
                assert rejected.returncode == 1 and rejected.stderr, name
            records.append(dict(operation=operation,depth=depth,rasterSha256=hashlib.sha256(raw).hexdigest(),
                encodedSha256=hashlib.sha256(encoded.read_bytes()).hexdigest(),encodedBytes=encoded.stat().st_size,
                idatBytes=idat_bytes,exactPixels=width*height,independentLiveOutputs=True,
                countersExact=True,sourceUnchanged=True,negativeControls=len(controls)))
destination = root/'artifacts/warm-reference'
destination.mkdir(parents=True,exist_ok=True)
report = dict(schemaVersion=1,project='Lokad.Image',correctnessOnly=True,complete=True,cpu=arguments.cpu,
    distinctSourceRasters=2,scoredCases=0,cases=records)
(destination/'png-controls.json').write_text(json.dumps(report,indent=2)+'\n')
print(json.dumps(report))
