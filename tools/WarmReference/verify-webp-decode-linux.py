"""Original correctness-only protocol/ownership controls on one frozen VP8L input."""
import argparse
import hashlib
import json
import os
from pathlib import Path
import subprocess
import tempfile

parser = argparse.ArgumentParser()
parser.add_argument('--cpu',type=int,default=0)
parser.add_argument('--native-input',action='store_true')
arguments = parser.parse_args()
root = Path(__file__).resolve().parents[2]
worker = root/'external/warm-reference/ffmpeg-webp-decode-warm'
family = 'webp-native-decode' if arguments.native_input else 'webp-decode'
prepared = json.loads((root/f'artifacts/warm-reference/{family}/preparation.json').read_text())
input_file = root/prepared['InputFile']; encoded = input_file.read_bytes()
expected = (root/prepared['RasterFile']).read_bytes()
assert hashlib.sha256(encoded).hexdigest() == prepared['InputSha256'] and len(encoded) == prepared['InputBytes']
assert hashlib.sha256(expected).hexdigest() == prepared['RasterSha256'] and len(expected) == 513*259*4
allowed = sorted(os.sched_getaffinity(0)); assert arguments.cpu in allowed
other = next(cpu for cpu in allowed if cpu != arguments.cpu)

def run(command,protocol='quit\n',cpu=None):
    return subprocess.run(['taskset','-c',cpu or str(arguments.cpu),*command],input=protocol,text=True,capture_output=True,timeout=30)

with tempfile.TemporaryDirectory(prefix='lokad-image-vp8l-decode-controls-') as temporary:
    folder = Path(temporary); output = folder/'output.raw'
    command = [str(worker),'webp-decode',str(input_file),'513','259','8',str(output)]
    process = run(command,'sample 1\nsample 2\ncheck\nsample 1\nquit\n')
    assert process.returncode == 0 and not process.stderr,process.stderr
    rows = [json.loads(line) for line in process.stdout.splitlines()]; ready = rows[0]
    assert ready['schemaVersion'] == 1 and ready['state'] == 'ready' and ready['operation'] == 'webp-decode'
    assert (ready['width'],ready['height'],ready['depth']) == (513,259,8)
    assert ready['cpu'] == arguments.cpu and ready['affinityCount'] == 1
    assert ready['codecThreads'] == ready['conversionThreads'] == 1 and ready['threadType'] == ready['activeThreadType'] == 0
    assert ready['independentLiveOutputs'] and ready['pixelsDeterministic'] and ready['initialCompletedOperations'] == 3
    assert ready['colorConversionTimed'] and ready['stateResetTimed'] and ready['decoder'] == 'webp' and ready['inputFormat'] == 'ARGB' and ready['lossless']
    samples = [row for row in rows if row['state'] == 'sample']
    assert [row['operations'] for row in samples] == [1,2,1] and [row['completedOperations'] for row in samples] == [4,6,8]
    assert all(row['elapsedNanoseconds'] > 0 and row['cpu'] == arguments.cpu and row['outputStride'] >= 513*4 and row['outputBytes'] == len(expected) for row in samples)
    assert rows[3] == dict(state='checked',exact=True,sourceUnchanged=True)
    raw = output.read_bytes(); assert raw == expected and input_file.read_bytes() == encoded
    ffmpeg = os.environ.get('LOKAD_IMAGE_FFMPEG','ffmpeg')
    cli = subprocess.run([ffmpeg,'-hide_banner','-loglevel','error','-threads:v','1','-thread_type:v','0','-filter_threads','1','-noautorotate',
        '-f','image2pipe','-i','pipe:0','-frames:v','1','-threads:v','1','-pix_fmt','rgba','-f','rawvideo','pipe:1'],input=encoded,capture_output=True,timeout=30,check=True)
    assert not cli.stderr and cli.stdout == raw
    negatives = []
    def reject(name,args,protocol='quit\n',cpu=None):
        result = run(args,protocol,cpu); assert result.returncode == 1 and result.stderr,(name,result.returncode,result.stderr)
        negatives.append(name)
    reject('multiple CPUs',command,cpu=f'{arguments.cpu},{other}')
    for protocol,name in [('sample 0\n','zero count'),('sample 10000001\n','count bound'),('sample 1 trailing\n','trailing protocol'),('unknown\n','unknown protocol')]:
        reject(name,command,protocol)
    reject('extra argument',command+['extra'])
    for index,value,name in [(1,'jpeg-decode','operation'),(3,'0','zero width'),(3,'32769','width bound'),(3,'512','wrong width'),(4,'258','wrong height'),(5,'16','depth'),(2,str(folder/'missing'),'missing input')]:
        changed = command.copy(); changed[index] = value; reject(name,changed)
    mutations = [('truncated RIFF',encoded[:-1])]
    for offset,value,name in [(0,0,'RIFF signature'),(12,0,'VP8L FourCC'),(20,0,'VP8L signature'),(24,encoded[24]|0x20,'VP8L version')]:
        damaged = bytearray(encoded); damaged[offset] = value; mutations.append((name,damaged))
    for offset,name in [(4,'RIFF size'),(16,'chunk size')]:
        damaged = bytearray(encoded); size = int.from_bytes(damaged[offset:offset+4],'little'); damaged[offset:offset+4] = (size+2).to_bytes(4,'little'); mutations.append((name,damaged))
    for index,(name,data) in enumerate(mutations):
        path = folder/f'damaged-{index}.webp'; path.write_bytes(data)
        changed = command.copy(); changed[2] = str(path); reject(name,changed)
    report = dict(schemaVersion=1,project='Lokad.Image',correctnessOnly=True,noTimingClaim=True,complete=True,cpu=arguments.cpu,
        distinctSourceRasters=1,extraSourceRasters=0,qualityEnumeration=False,
        inputSha256=prepared['InputSha256'],inputBytes=len(encoded),rasterSha256=prepared['RasterSha256'],outputBytes=len(raw),outputSha256=hashlib.sha256(raw).hexdigest(),
        allRgbaSamplesExact=True,alphaExact=True,hiddenRgbExact=True,nativeCliPixelsExact=True,independentLiveOutputs=True,countersExact=True,sourceUnchanged=True,nativeReady=ready,negativeControls=negatives)
(root/f'artifacts/warm-reference/{family}-controls.json').write_text(json.dumps(report,indent=2)+'\n')
print(json.dumps(report))
