"""Correctness-only controls for the borrowed-canvas rectangle-copy reference."""
import hashlib
import json
import os
from pathlib import Path
import subprocess
import tempfile

root = Path(__file__).resolve().parents[2]
worker = root / 'external/warm-reference/ffmpeg-copy-region-warm'
records = []
for depth, relative, pinned in [
    (8, 'artifacts/warm-reference/webp-decode/source.rgba', '70f0fb18c341e2740b875a5a34e21ca63f0821b3386352fe2971020c5a6aac80'),
    (16, 'artifacts/warm-reference/bilinear16-audit/input.rgba64le', '3f861b61360b442c84063e7a6e1cb4f35957dcaa16f898048beaa36bbf4272f9'),
]:
    source = root / relative
    pixels = source.read_bytes()
    assert hashlib.sha256(pixels).hexdigest() == pinned
    pixel = depth // 8 * 4
    stride = 513 * pixel
    expected = bytearray(pixels)
    for y in range(129):
        start = (y + 65) * stride + 127 * pixel
        destination = (y + 19) * stride + 17 * pixel
        expected[destination:destination + 256 * pixel] = pixels[start:start + 256 * pixel]
    with tempfile.TemporaryDirectory(prefix='lokad-image-copy-region-') as name:
        folder = Path(name)
        output = folder / 'output.rgba'
        arguments = [str(worker), 'copy-region', str(source), '513', '259', str(depth), str(output)]

        def run(values, protocol='quit\n', cpus='0'):
            return subprocess.run(['taskset', '-c', cpus, *values], input=protocol, capture_output=True, text=True, timeout=30)

        result = run(arguments, 'sample 1\ncheck\nquit\n')
        assert result.returncode == 0 and not result.stderr, result.stderr
        ready, sample, check = map(json.loads, result.stdout.splitlines())
        assert ready['operation'] == 'copy-region' and ready['depth'] == depth
        assert ready['affinityCount'] == ready['workerThreads'] == 1 and ready['cpu'] == 0
        assert ready['borrowedSource'] and ready['borrowedCanvas'] and ready['canvasMutationTimed']
        assert not ready['sourcePreparationTimed'] and not ready['canvasPreparationTimed'] and not ready['outputAllocationTimed']
        assert sample['operations'] == 1 and sample['completedOperations'] == ready['initialCompletedOperations'] + 1
        assert sample['outputStride'] == stride and check['exact'] and check['sourceUnchanged']
        assert output.read_bytes() == expected and source.read_bytes() == pixels
        controls = []

        def reject(label, values, protocol='quit\n', cpus='0'):
            result = run(values, protocol, cpus)
            assert result.returncode != 0 and result.stderr, label
            controls.append(label)

        reject('missing output', arguments[:-1])
        for label, index, value in [('operation', 1, 'crop'), ('zero width', 3, '0'),
                                    ('width text', 3, '513x'), ('wrong width', 3, '512'),
                                    ('dimension bound', 4, '32769'), ('depth', 5, '12')]:
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
        records.append(dict(depth=depth, sourceSha256=pinned, outputSha256=hashlib.sha256(expected).hexdigest(),
                            fullCanvasExact=True, alphaExact=True, hiddenRgbExact=True, sourceUnchanged=True,
                            countersExact=True, controls=controls))
record = dict(schemaVersion=1, project='Lokad.Image', correctnessOnly=True, noTimingClaim=True,
              workerSha256=hashlib.sha256(worker.read_bytes()).hexdigest(), cases=records)
(root / 'tmp/copy-region-reference-controls.json').write_text(json.dumps(record, indent=2) + '\n')
print(json.dumps(record))
