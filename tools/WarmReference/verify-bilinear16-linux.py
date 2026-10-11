"""Correctness-only RGBA16 precision, ownership and timing-refusal controls."""
import argparse
import hashlib
import json
import subprocess
import tempfile
from pathlib import Path

parser = argparse.ArgumentParser()
parser.add_argument('--cpu', type=int, default=0)
args = parser.parse_args()
root = Path(__file__).resolve().parents[2]
source = root / 'artifacts/warm-reference/bilinear16-audit/input.rgba64le'
digest = lambda p: hashlib.sha256(p.read_bytes()).hexdigest()
assert digest(source) == '3f861b61360b442c84063e7a6e1cb4f35957dcaa16f898048beaa36bbf4272f9'
worker = root / 'external/warm-reference/ffmpeg-bilinear-warm'

with tempfile.TemporaryDirectory(prefix='lokad-image-bilinear16-') as name:
    folder = Path(name)
    output = folder / 'result.raw'
    command = ['taskset', '-c', str(args.cpu), str(worker), 'bilinear-resize',
               str(source), '513', '259', '16', str(output)]
    failed = subprocess.run(command, input='sample 1\nquit\n', capture_output=True, text=True, timeout=30)
    assert failed.returncode != 0 and not failed.stdout
    assert 'alpha differs from the exact reference; refusing timing' in failed.stderr

    # This small control adds no performance raster. Hidden RGB and ownership
    # must remain correct when all 16-bit alpha samples are zero.
    raw = folder / 'zero-alpha.raw'
    data = bytearray(9 * 7 * 8)
    for i in range(9 * 7):
        for channel in range(3):
            data[i * 8 + channel * 2:i * 8 + channel * 2 + 2] = ((i * 12347 + channel * 23459) & 65535).to_bytes(2, 'little')
    raw.write_bytes(data)
    command = ['taskset', '-c', str(args.cpu), str(worker), 'bilinear-resize',
               str(raw), '9', '7', '16', str(output)]
    good = subprocess.run(command, input='check\nquit\n', capture_output=True, text=True, timeout=30)
    assert good.returncode == 0 and not good.stderr, good.stderr
    ready, check = map(json.loads, good.stdout.splitlines())
    assert ready['depth'] == 16 and ready['independentLiveOutputs']
    assert ready['filterThreads'] == 1 and ready['threadType'] == 0 and ready['cpu'] == args.cpu
    assert check['matched'] and check['alphaExact'] and check['sourceUnchanged']
    assert output.read_bytes() == bytes(4 * 3 * 8) and raw.read_bytes() == data
    record = dict(schemaVersion=1, project='Lokad.Image', correctnessOnly=True, noTimingClaim=True,
                  workerSha256=digest(worker), originalSourceSha256=digest(source),
                  strictAlphaMismatchRefusesReadyAndSample=True, mismatchExitCode=failed.returncode,
                  mismatchStderr=failed.stderr.strip(), rgba16ZeroAlphaRgbZero=True,
                  rgba16IndependentLiveOutputs=True, rgba16SourceUnchanged=True,
                  strictAlphaWarmComparisonOpen=True)
    target = root / 'artifacts/warm-reference/bilinear16-native-audit.json'
    target.write_text(json.dumps(record, indent=2) + '\n')
    print(json.dumps(record))
