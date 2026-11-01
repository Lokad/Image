"""One correctness-only paired-overlap protocol control and bounded rejections."""
import hashlib
import json
import os
from pathlib import Path
import subprocess
import tempfile

root = Path(__file__).resolve().parents[2]
worker = root / 'external/warm-reference/ffmpeg-copy-region-overlap-warm'
source = root / 'artifacts/warm-reference/webp-decode/source.rgba'
pixels = source.read_bytes()
assert hashlib.sha256(pixels).hexdigest() == '70f0fb18c341e2740b875a5a34e21ca63f0821b3386352fe2971020c5a6aac80'

def move(original, sx, sy, dx, dy):
    result = bytearray(original)
    for row in range(129):
        a = ((sy + row) * 513 + sx) * 4
        b = ((dy + row) * 513 + dx) * 4
        result[b:b + 1024] = original[a:a + 1024]
    return bytes(result)

forward = move(pixels, 17, 19, 127, 65)
expected = move(forward, 127, 65, 17, 19)
assert move(expected, 17, 19, 127, 65) == forward
assert move(forward, 127, 65, 17, 19) == expected and forward != expected
with tempfile.TemporaryDirectory(prefix='lokad-image-copy-overlap-') as name:
    folder = Path(name)
    output = folder / 'output.rgba'
    arguments = [str(worker), 'copy-region-overlap', str(source), '513', '259', '8', str(output)]

    def run(values, protocol='quit\n', cpus='0'):
        return subprocess.run(['taskset', '-c', cpus, *values], input=protocol, capture_output=True, text=True, timeout=30)

    p = run(arguments, 'sample 3\ncheck\nquit\n')
    assert p.returncode == 0 and not p.stderr, p.stderr
    ready, sample, check = map(json.loads, p.stdout.splitlines())
    assert ready['operation'] == 'copy-region-overlap' and ready['sourceAliasesCanvas'] and ready['borrowedCanvas']
    assert ready['publicMovesPerPair'] == 2 and ready['nativeCallsPerPair'] == 6 and ready['bandHeight'] == 46
    assert ready['disjointNativeRanges'] and ready['restorationTimed'] and ready['temporaryPixelBytes'] == 0
    assert ready['affinityCount'] == ready['workerThreads'] == 1 and ready['cpu'] == 0
    assert not ready['sourcePreparationTimed'] and not ready['canvasPreparationTimed'] and not ready['outputAllocationTimed']
    assert sample['operations'] == 3 and sample['completedOperations'] == ready['initialCompletedOperations'] + 3
    assert sample['publicMoves'] == 2 * sample['completedOperations'] and sample['nativeBandCalls'] == 6 * sample['completedOperations']
    assert check['exact'] and check['snapshotSemanticsExact'] and check['sourceAliasesCanvas']
    assert output.read_bytes() == expected and source.read_bytes() == pixels
    controls = []

    def reject(label, values, protocol='quit\n', cpus='0'):
        p = run(values, protocol, cpus)
        assert p.returncode != 0 and p.stderr, label
        controls.append(label)

    reject('missing output', arguments[:-1])
    for label, index, value in [('operation', 1, 'copy-region'), ('zero width', 3, '0'),
                                ('width text', 3, '513x'), ('wrong width', 3, '512'),
                                ('dimension bound', 4, '32769'), ('depth', 5, '16')]:
        values = arguments.copy()
        values[index] = value
        reject(label, values)
    for label, data in [('truncated', pixels[:-1]), ('extra', pixels + b'\0')]:
        path = folder / (label + '.rgba')
        path.write_bytes(data)
        values = arguments.copy()
        values[2] = str(path)
        reject(label, values)
    for command in ('sample 0', 'sample 10000001', 'sample 1 extra', 'invalid'):
        reject(command, arguments, command + '\n')
    allowed = sorted(os.sched_getaffinity(0))
    assert len(allowed) >= 2
    reject('multiple CPUs', arguments, cpus=','.join(map(str, allowed[:2])))
    record = dict(schemaVersion=1, project='Lokad.Image', correctnessOnly=True, noTimingClaim=True,
                  sourceSha256=hashlib.sha256(pixels).hexdigest(), forwardSha256=hashlib.sha256(forward).hexdigest(),
                  outputSha256=hashlib.sha256(expected).hexdigest(), fullCanvasExact=True, snapshotSemanticsExact=True,
                  bothMovesCounted=True, nativeRangesDisjoint=True, temporaryPixelBytes=0,
                  controls=controls, workerSha256=hashlib.sha256(worker.read_bytes()).hexdigest())
    (root / 'tmp/copy-region-overlap-reference-controls.json').write_text(json.dumps(record, indent=2) + '\n')
    print(json.dumps(record))
