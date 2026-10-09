"""Original warm JPEG protocol controls on the existing raster; no timing claims."""
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
worker = root/'external/warm-reference/ffmpeg-jpeg-warm'
allowed = sorted(os.sched_getaffinity(0))
assert arguments.cpu in allowed
other = next(cpu for cpu in allowed if cpu != arguments.cpu)
width, height = 513, 259
values = [(((x*12347+y*7919+channel*23459+x*y*101)&65535)+128)//257
          for y in range(height) for x in range(width) for channel in range(4)]
values[3] = values[width*4-1] = 0
raw = bytes(values)

def header(encoded):
    # Public T.81 B.2 marker framing and baseline frame header, not entropy decode.
    assert encoded[:2] == b'\xff\xd8' and encoded[-2:] == b'\xff\xd9'
    position, frame = 2, None
    while position < len(encoded)-2:
        assert encoded[position] == 255
        marker = encoded[position+1]
        length = int.from_bytes(encoded[position+2:position+4], 'big')
        assert length >= 2 and position+length+2 <= len(encoded)
        payload = encoded[position+4:position+length+2]
        if marker == 192:
            assert frame is None and payload[:6] == struct.pack('>BHHB',8,height,width,3)
            assert len(payload) == 15
            factors = [payload[7+3*i] for i in range(3)]
            # T.81 permits equivalent 4:4:4 factors, including 2x2 for all planes.
            assert factors[0] == factors[1] == factors[2], factors
            assert 1 <= factors[0] >> 4 <= 4 and 1 <= factors[0] & 15 <= 4
            frame = payload
        if marker == 218:
            assert frame is not None
            return
        position += length+2
    raise AssertionError('Missing frame/scan header')

with tempfile.TemporaryDirectory(prefix='lokad-image-jpeg-controls-') as temporary:
    folder = Path(temporary)
    raster, encoded = folder/'input.raw', folder/'output.jpg'
    raster.write_bytes(raw)
    command = [str(worker),'jpeg-encode',str(raster),str(width),str(height),'8',str(encoded),'5']
    result = subprocess.run(['taskset','-c',str(arguments.cpu),*command],
        input='sample 1\nsample 2\ncheck\nsample 1\nquit\n',text=True,capture_output=True,timeout=30)
    assert result.returncode == 0 and not result.stderr, result.stderr
    rows = [json.loads(line) for line in result.stdout.splitlines()]
    ready = rows[0]
    assert ready['state'] == 'ready' and ready['operation'] == 'jpeg-encode'
    assert ready['width'] == width and ready['height'] == height and ready['depth'] == 8
    assert ready['cpu'] == arguments.cpu and ready['affinityCount'] == 1
    assert ready['codecThreads'] == ready['conversionThreads'] == 1
    assert ready['threadType'] == ready['activeThreadType'] == 0
    assert ready['qscale'] == 5 and ready['backgroundRgb'] == 0x204060 and ready['sampling'] == '444'
    assert ready['independentLiveOutputs'] and ready['bitstreamDeterministic']
    assert ready['colorConversionTimed'] and ready['flatteningTimed'] and ready['initialCompletedOperations'] == 3
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
    assert len(decoded) == width*height*4 and all(value == 255 for value in decoded[3::4])
    controls = [(['taskset','-c',f'{arguments.cpu},{other}',*command],'quit\n','affinity')]
    for protocol,name in [('sample 0\n','zero count'),('sample 10000001\n','count bound'),
                          ('sample 1 trailing\n','trailing tokens'),('sample -1\n','negative count'),
                          ('unknown\n','unknown command'),('check trailing\n','trailing check'),
                          ('quit trailing\n','trailing quit')]:
        controls.append((['taskset','-c',str(arguments.cpu),*command],protocol,name))
    for index,value,name in [(7,'0','zero quality'),(7,'32','quality bound'),(5,'16','depth'),(3,'0','width')]:
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
        encodedSha256=hashlib.sha256(data).hexdigest(),encodedBytes=len(data),baseline444=True,alphaExact=True,
        independentLiveOutputs=True,countersExact=True,sourceUnchanged=True,negativeControls=len(controls),
        workerSha256=hashlib.sha256(worker.read_bytes()).hexdigest())
destination = root/'artifacts/warm-reference'
destination.mkdir(parents=True,exist_ok=True)
(destination/'jpeg-controls.json').write_text(json.dumps(report,indent=2)+'\n')
print(json.dumps(report))
