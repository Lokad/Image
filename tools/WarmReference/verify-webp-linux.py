"""Original warm VP8 protocol controls on the existing raster; no timing claims."""
import argparse
import hashlib
import json
import os
from pathlib import Path
import subprocess
import tempfile

parser = argparse.ArgumentParser()
parser.add_argument('--cpu', type=int, default=0)
arguments = parser.parse_args()
root = Path(__file__).resolve().parents[2]
worker = root/'external/warm-reference/ffmpeg-webp-warm'
allowed = sorted(os.sched_getaffinity(0))
assert arguments.cpu in allowed
other = next(cpu for cpu in allowed if cpu != arguments.cpu)
width, height = 513, 259
values = [(((x*12347+y*7919+channel*23459+x*y*101)&65535)+128)//257
          for y in range(height) for x in range(width) for channel in range(4)]
values[3] = values[width*4-1] = 0
raw = bytes(values)

def header(encoded):
    # RFC 9649 RIFF and still extended VP8/alpha framing; no entropy decode.
    assert encoded[:4] == b'RIFF' and encoded[8:12] == b'WEBP'
    assert int.from_bytes(encoded[4:8],'little')+8 == len(encoded)
    position, chunks = 12, []
    while position < len(encoded):
        assert len(encoded)-position >= 8
        tag = encoded[position:position+4]
        size = int.from_bytes(encoded[position+4:position+8],'little')
        assert position+8+size+(size&1) <= len(encoded)
        payload = encoded[position+8:position+8+size]
        if tag == b'VP8X':
            assert position == 12 and size == 10 and payload[0]&16 and not payload[0]&2
            assert int.from_bytes(payload[4:7],'little')+1 == width
            assert int.from_bytes(payload[7:10],'little')+1 == height
        chunks.append(tag); position += 8+size+(size&1)
    assert chunks == [b'VP8X',b'ALPH',b'VP8 ']

with tempfile.TemporaryDirectory(prefix='lokad-image-webp-controls-') as temporary:
    folder = Path(temporary)
    raster, encoded = folder/'input.raw', folder/'output.webp'
    raster.write_bytes(raw)
    command = [str(worker),'webp-encode',str(raster),str(width),str(height),'8',str(encoded),'75']
    result = subprocess.run(['taskset','-c',str(arguments.cpu),*command],
        input='sample 1\nsample 2\ncheck\nsample 1\nquit\n',text=True,capture_output=True,timeout=30)
    assert result.returncode == 0 and not result.stderr, result.stderr
    rows = [json.loads(line) for line in result.stdout.splitlines()]
    ready = rows[0]
    assert ready['state'] == 'ready' and ready['operation'] == 'webp-encode'
    assert ready['width'] == width and ready['height'] == height and ready['depth'] == 8
    assert ready['cpu'] == arguments.cpu and ready['affinityCount'] == 1
    assert ready['codecThreads'] == ready['conversionThreads'] == 1
    assert ready['threadType'] == ready['activeThreadType'] == 0
    assert ready['quality'] == 75 and ready['effort'] == 4 and not ready['lossless'] and ready['inputFormat'] == 'BGRA'
    assert ready['independentLiveOutputs'] and ready['bitstreamDeterministic']
    assert ready['repackingTimed'] and ready['initialCompletedOperations'] == 3
    samples = [row for row in rows if row['state'] == 'sample']
    assert [row['operations'] for row in samples] == [1,2,1]
    assert [row['completedOperations'] for row in samples] == [4,6,8]
    assert all(row['elapsedNanoseconds'] > 0 and row['cpu'] == arguments.cpu
               and row['outputStride'] == 0 and row['outputBytes'] == encoded.stat().st_size for row in samples)
    assert rows[3] == dict(state='checked',exact=True,sourceUnchanged=True)
    data = encoded.read_bytes(); header(data)
    assert raster.read_bytes() == raw
    # Native decode correctness is independently checked without claiming parity.
    decoded = subprocess.run(['ffmpeg','-v','error','-threads','1','-i',str(encoded),'-frames:v','1',
        '-pix_fmt','rgba','-f','rawvideo','pipe:1'],capture_output=True,timeout=30,check=True).stdout
    assert len(decoded) == width*height*4 and decoded[3::4] == raw[3::4]
    controls = [(['taskset','-c',f'{arguments.cpu},{other}',*command],'quit\n','affinity')]
    for protocol,name in [('sample 0\n','zero count'),('sample 10000001\n','count bound'),
                          ('sample 1 trailing\n','trailing tokens'),('sample -1\n','negative count'),
                          ('unknown\n','unknown command'),('check trailing\n','trailing check'),
                          ('quit trailing\n','trailing quit')]:
        controls.append((['taskset','-c',str(arguments.cpu),*command],protocol,name))
    for index,value,name in [(7,'0','zero quality'),(7,'101','quality bound'),(5,'16','depth'),(3,'0','width')]:
        changed = command.copy(); changed[index] = value
        controls.append((['taskset','-c',str(arguments.cpu),*changed],'quit\n',name))
    short,extra = folder/'short.raw',folder/'extra.raw'
    short.write_bytes(raw[:-1]); extra.write_bytes(raw+b'\0')
    for path,name in [(short,'short raster'),(extra,'excess raster')]:
        changed = command.copy(); changed[2] = str(path)
        controls.append((['taskset','-c',str(arguments.cpu),*changed],'quit\n',name))
    for native,protocol,name in controls:
        rejected = subprocess.run(native,input=protocol,text=True,capture_output=True,timeout=30)
        assert rejected.returncode == 1 and rejected.stderr, name
    report = dict(schemaVersion=1,project='Lokad.Image',correctnessOnly=True,complete=True,cpu=arguments.cpu,
        distinctSourceRasters=1,scoredCases=0,rasterSha256=hashlib.sha256(raw).hexdigest(),
        encodedSha256=hashlib.sha256(data).hexdigest(),encodedBytes=len(data),vp8WithAlpha=True,alphaExact=True,
        independentLiveOutputs=True,countersExact=True,sourceUnchanged=True,negativeControls=len(controls),
        workerSha256=hashlib.sha256(worker.read_bytes()).hexdigest())
destination = root/'artifacts/warm-reference'
destination.mkdir(parents=True,exist_ok=True)
(destination/'webp-controls.json').write_text(json.dumps(report,indent=2)+'\n')
print(json.dumps(report))
