"""Original protocol/ownership controls using the existing prepared JPEG only."""
import argparse
import hashlib
import json
import os
from pathlib import Path
import subprocess
import tempfile

parser = argparse.ArgumentParser()
parser.add_argument('--cpu', type=int, default=0)
parser.add_argument('--sampling', choices=['444','420','422','gray','progressive-gray','progressive-420','progressive-444'], default='444')
arguments = parser.parse_args()
root = Path(__file__).resolve().parents[2]
worker = root/'external/warm-reference/ffmpeg-jpeg-decode-warm'
fixed=arguments.sampling!='444'
progressive=arguments.sampling.startswith('progressive-')
sampling=arguments.sampling.removeprefix('progressive-')
subsampled=sampling in ('420','422')
profile=f'jpeg-progressive-{sampling}.json' if progressive else f'jpeg-decode{sampling}.json' if fixed else 'jpeg-match.json'
prepared = json.loads((root/'artifacts/warm-reference'/profile).read_text())
if progressive:prepared=prepared['input']
input_file = root/prepared['nativeFile' if fixed else 'managedFile']
encoded = input_file.read_bytes()
assert hashlib.sha256(encoded).hexdigest() == (prepared['encodedSha256'] if fixed else prepared['match']['closestOwned']['encodedSha256'])
operation='jpeg-gray-decode' if sampling=='gray' else f'jpeg{sampling}-decode' if fixed and sampling!='444' else 'jpeg-decode'
allowed = sorted(os.sched_getaffinity(0))
assert arguments.cpu in allowed
other = next(cpu for cpu in allowed if cpu != arguments.cpu)

def run(command, protocol='quit\n', cpu=None):
    return subprocess.run(['taskset', '-c', cpu or str(arguments.cpu), *command],
        input=protocol, text=True, capture_output=True, timeout=30)

with tempfile.TemporaryDirectory(prefix='lokad-image-jpeg-decode-controls-') as temporary:
    folder = Path(temporary)
    output = folder/'output.raw'
    command = [str(worker), operation, str(input_file), '513', '259', '8', str(output)]
    process = run(command, 'sample 1\nsample 2\ncheck\nsample 1\nquit\n')
    assert process.returncode == 0 and not process.stderr, process.stderr
    rows = [json.loads(line) for line in process.stdout.splitlines()]
    ready = rows[0]
    assert ready['schemaVersion'] == 1 and ready['state'] == 'ready'
    assert ready['operation'] == operation and (ready['width'], ready['height'], ready['depth']) == (513,259,8)
    assert ready['sampling']==sampling and ready['chromaExpansionTimed']==subsampled
    assert ready['chromaVerticalFactor']==(2 if sampling=='420' else 1)
    assert ready['grayscale']==(sampling=='gray')
    assert ready['cpu'] == arguments.cpu and ready['affinityCount'] == 1
    assert ready['codecThreads'] == ready['conversionThreads'] == 1
    assert ready['threadType'] == ready['activeThreadType'] == 0
    assert ready['independentLiveOutputs'] and ready['pixelsDeterministic'] and ready['initialCompletedOperations'] == 3
    assert ready['colorConversionTimed'] and ready['stateResetTimed'] and ready['decoder'] == 'mjpeg'
    samples = [row for row in rows if row['state'] == 'sample']
    assert [row['operations'] for row in samples] == [1,2,1]
    assert [row['completedOperations'] for row in samples] == [4,6,8]
    assert all(row['elapsedNanoseconds'] > 0 and row['cpu'] == arguments.cpu
        and row['outputStride'] >= 513*4 and row['outputBytes'] == 513*259*4 for row in samples)
    assert rows[3] == dict(state='checked',exact=True,sourceUnchanged=True)
    raw = output.read_bytes()
    assert len(raw) == 513*259*4 and all(value == 255 for value in raw[3::4])
    assert input_file.read_bytes() == encoded
    negatives = []
    def reject(name, args, protocol='quit\n', cpu=None):
        result = run(args, protocol, cpu)
        assert result.returncode == 1 and result.stderr, (name,result.returncode,result.stderr)
        negatives.append(name)
    reject('multiple CPUs', command, cpu=f'{arguments.cpu},{other}')
    for protocol,name in [('sample 0\n','zero count'), ('sample 10000001\n','count bound'),
        ('sample 1 trailing\n','trailing protocol'), ('unknown\n','unknown protocol')]:
        reject(name, command, protocol)
    reject('extra argument', command+['extra'])
    for index,value,name in [(1,'png-decode','operation'), (3,'0','zero width'), (3,'32769','width bound'),
        (3,'512','wrong width'), (4,'258','wrong height'), (5,'16','depth'), (2,str(folder/'missing'),'missing input')]:
        changed = command.copy(); changed[index] = value; reject(name, changed)
    damaged = folder/'truncated.jpg'; damaged.write_bytes(encoded[:-1])
    changed = command.copy(); changed[2] = str(damaged); reject('truncated JPEG', changed)
    changed=command.copy();changed[1]='jpeg420-decode' if sampling=='444' else 'jpeg-decode'
    reject('wrong declared sampling',changed)
    report = dict(schemaVersion=1, project='Lokad.Image', correctnessOnly=True, complete=True,
        cpu=arguments.cpu, distinctSourceRasters=1, extraSourceRasters=0, qualityEnumeration=False,
        inputSha256=hashlib.sha256(encoded).hexdigest(), inputBytes=len(encoded), outputBytes=len(raw),
        outputSha256=hashlib.sha256(raw).hexdigest(), alphaExact=True, independentLiveOutputs=True,
        countersExact=True, sourceUnchanged=True, nativeReady=ready, negativeControls=negatives)
destination = root/'artifacts/warm-reference'
(destination/(f'jpeg-decode{arguments.sampling}-controls.json' if fixed else 'jpeg-decode-controls.json')).write_text(json.dumps(report,indent=2)+'\n')
print(json.dumps(report))
