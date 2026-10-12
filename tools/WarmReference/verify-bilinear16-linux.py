"""Correctness-only RGBA16 strict refusal and explicit bounded native alpha."""
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

    bounded = subprocess.run(command + ['--allow-alpha-rounding1'], input='check\nquit\n', capture_output=True, text=True, timeout=30)
    assert bounded.returncode == 0 and not bounded.stderr, bounded.stderr
    ready16, check16 = map(json.loads, bounded.stdout.splitlines())
    assert ready16['alphaTolerance1'] and ready16['depth'] == 16 and ready16['independentLiveOutputs']
    assert check16['matched'] and not check16['alphaExact'] and check16['alphaMaximum'] == 1
    assert check16['alphaMeanSquared'] == 62 / (256 * 129) and check16['sourceUnchanged']
    bounded_output_sha256 = digest(output)
    invalid = subprocess.run(command + ['--unknown'], input='quit\n', capture_output=True, text=True, timeout=30)
    assert invalid.returncode != 0 and not invalid.stdout and 'RGBA16-only' in invalid.stderr

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
                  boundedAlpha16Explicit=True, nativeAlphaMaximum=check16['alphaMaximum'],
                  nativeAlphaMeanSquared=check16['alphaMeanSquared'], boundedOutputSha256=bounded_output_sha256,
                  unknownToleranceRejected=True, strictAlphaWarmComparisonOpen=True)
    target = root / 'artifacts/warm-reference/bilinear16-bounded-native-controls.json'
    target.write_text(json.dumps(record, indent=2) + '\n')
    print(json.dumps(record))
