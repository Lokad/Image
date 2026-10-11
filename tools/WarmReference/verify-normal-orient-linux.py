"""Correctness-only protocol controls on one existing original RGBA8/RGBA16 raster."""
import argparse, hashlib, json, os, subprocess, tempfile
from pathlib import Path

parser = argparse.ArgumentParser()
parser.add_argument('--cpu', type=int, default=0)
parser.add_argument('--depth', type=int, choices=(8,16), default=8)
args = parser.parse_args()
root = Path(__file__).resolve().parents[2]
worker = root/'external/warm-reference/ffmpeg-normal-orient-warm'
source = root/('artifacts/warm-reference/bilinear16-audit/input.rgba64le' if args.depth == 16
              else 'artifacts/warm-reference/webp-decode/source.rgba')
pixels = source.read_bytes()
assert hashlib.sha256(pixels).hexdigest() == ('3f861b61360b442c84063e7a6e1cb4f35957dcaa16f898048beaa36bbf4272f9' if args.depth == 16
    else '70f0fb18c341e2740b875a5a34e21ca63f0821b3386352fe2971020c5a6aac80')
pixel_bytes = args.depth//8*4
expected = pixels

with tempfile.TemporaryDirectory(prefix='lokad-image-normal-orient-') as name:
    folder = Path(name)
    output = folder/'output.rgba'
    base = [str(worker), 'normal-orient', str(source), '513', '259', str(args.depth), str(output)]
    def run(arguments, protocol='check\nquit\n', cpus=None):
        return subprocess.run(['taskset', '-c', str(args.cpu) if cpus is None else cpus, *arguments],
            input=protocol, capture_output=True, text=True, timeout=30)
    good = run(base, 'sample 1\ncheck\nquit\n')
    assert good.returncode == 0 and not good.stderr, good.stderr
    ready, sample, check = [json.loads(line) for line in good.stdout.splitlines()]
    assert ready['operation'] == 'normal-orient' and ready['filterThreads'] == 1 and ready['threadType'] == 0
    assert ready['affinityCount'] == 1 and ready['cpu'] == args.cpu and ready['independentLiveOutputs']
    assert (ready['outputWidth'], ready['outputHeight']) == (513, 259)
    assert sample['operations'] == 1 and sample['completedOperations'] == ready['initialCompletedOperations']+1
    assert ready['depth'] == args.depth and ready['sharedNullView'] and ready['ownedCopyTimed']
    assert sample['outputStride'] >= 513*pixel_bytes and check['exact'] and check['sourceUnchanged']
    assert output.read_bytes() == expected and source.read_bytes() == pixels
    controls = []
    def reject(label, arguments, protocol='quit\n', cpus=None):
        result = run(arguments, protocol, cpus)
        assert result.returncode != 0 and result.stderr, label
        controls.append(label)
    reject('missing argument', base[:-1])
    for label, index, value in [('operation',1,'hflip'), ('dimension zero',3,'0'),
            ('dimension text',3,'513x'), ('dimension bound',3,'32769'),
            ('minimum output',3,'1'), ('wrong precision',5,'12')]:
        values = base.copy(); values[index] = value; reject(label,values)
    for label, data in [('truncated input',pixels[:-1]), ('extra input',pixels+b'\0')]:
        path = folder/(label+'.raw'); path.write_bytes(data)
        values = base.copy(); values[2] = str(path); reject(label,values)
    for command in ('sample 0', 'sample 10000001', 'sample 1 extra', 'invalid'):
        reject(command,base,command+'\n')
    allowed = sorted(os.sched_getaffinity(0))
    assert len(allowed) >= 2, 'Need a two-CPU affinity negative control.'
    reject('multiple CPUs',base,cpus=','.join(map(str,allowed[:2])))
    record = dict(schemaVersion=1, project='Lokad.Image', correctnessOnly=True, noTimingClaim=True,
        complete=True, cpu=args.cpu, depth=args.depth, distinctSourceRasters=1, extraSourceRasters=0,
        sourceSha256=hashlib.sha256(pixels).hexdigest(), outputSha256=hashlib.sha256(expected).hexdigest(),
        inputBytes=len(pixels), outputBytes=len(expected), nativeRgbaExact=True,
        alphaExact=True, hiddenRgbExact=True, independentLiveOutputs=True,
        nativeOperationCountersExact=True, sourceUnchanged=True, controls=controls,
        workerSha256=hashlib.sha256(worker.read_bytes()).hexdigest())
    destination = root/('artifacts/warm-reference/normal-orient16-controls.json' if args.depth == 16
                       else 'artifacts/warm-reference/normal-orient-controls.json')
    destination.write_text(json.dumps(record,indent=2)+'\n')
    print(json.dumps(record))
