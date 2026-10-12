"""Render the current warm comparison from retained evidence; never run a benchmark."""
import json
import math
import re
import statistics
import sys
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]


def read(name):
    return json.loads((ROOT / 'benchmarks' / name).read_text(encoding='utf-8'))


def select(value, path):
    for key in path:
        value = value[key]
    return value


def row(label, name, path, report, confirmed=False):
    evidence = read(name)
    observation = select(evidence, path)
    if 'quietnessBefore' not in observation and 'quietness' not in observation and len(path) >= 2 and path[-2] == 'cases':
        parent = select(evidence, path[:-2])
        if 'quietnessBefore' in parent:
            observation = {**observation, 'quietnessBefore': parent['quietnessBefore'], 'quietnessAfter': parent['quietnessAfter']}
    stats = observation.get('managedStatistics') or observation['managed']['statistics']
    if 'nativeBrackets' in observation:
        native = min(b['mean'] for b in observation['nativeBrackets'])
    else:
        native = min(statistics.mean(s.get('NanosecondsPerOperation', s.get('ElapsedNanoseconds', 0) / s.get('Operations', 1))
                                     for s in observation[side]) for side in ('nativeBefore', 'nativeAfter'))
    assert native > 0 and stats['Mean'] > 0
    ratio, upper = observation['worstBracketRatio'], observation['conservativeRatioUpper']
    assert abs(stats['Mean'] / native - ratio) < 1e-9
    if 'quietness' in observation:
        quiet = all(g['passed'] for g in observation['quietness'].values())
    elif 'quietnessBefore' in observation:
        quiet = observation['quietnessBefore']['passed'] and observation['quietnessAfter']['passed']
    else:
        quiet = None  # Older grouped evidence has no guards attached to each observation.
    if confirmed:
        assert quiet and upper <= 2
        assert evidence.get('independentQuietConfirmationThisPair') or observation.get('certifiedThisPair')
    status = 'Confirmed' if confirmed else ('Quiet certification open' if quiet is None else ('Quiet repeat open' if quiet else 'Quiet guard failed / open'))
    memory = observation.get('managedMemory')
    if memory:
        allocated = memory['BytesAllocatedPerOperation']
    else:
        gc = observation['managed']['rawGc']
        match = re.fullmatch(r'// GC:\s+\d+\s+\d+\s+\d+\s+(\d+)\s+(\d+)', gc)
        assert match, gc
        allocated = math.ceil(int(match[1]) / int(match[2]))
    assert allocated >= 0
    return f'| [{label}](../benchmarks/{report}) | {stats["Mean"] / 1000:,.2f} | {native / 1000:,.2f} | {ratio:.3f} / {upper:.3f} | {allocated:,} | {status} |'


def pending(label, report, status):
    return f'| [{label}](../benchmarks/{report}) | — | — | — | — | {status} |'


def table(rows):
    return '\n'.join(['| Operation / fixed profile | Lokad µs | FFmpeg µs | Ratio / upper | Lokad allocated B/op | Status |',
                      '| --- | ---: | ---: | ---: | ---: | --- |', *rows])


def render():
    decodes = [
        row('PNG RGBA8, None', 'warm-png8decode-quiet-evidence.json', ['observations', -1], 'WARM-PNG8-DECODE-QUIET.md', True),
        row('PNG RGBA16, None', 'warm-pngliteralrun-evidence.json', ['observations', -1], 'WARM-PNG-LITERAL-RUN.md', True),
        row('PNG RGBA8, Paeth', 'warm-paeth4-evidence.json', ['observations', -1], 'WARM-PAETH-FOUR-CHANNEL.md', True),
        row('PNG RGBA16, Paeth', 'warm-paethvector-evidence.json', ['observations', -1], 'WARM-PAETH-VECTOR.md', True),
        row('PNG RGBA16, Adam7', 'warm-pngscatter-evidence.json', ['observations', -1], 'WARM-PNG-SCATTER.md', True),
        row('JPEG baseline gray', 'warm-jpegac-fused-evidence.json', ['regressionControl', 'observations', -1], 'WARM-JPEG-AC-FUSED.md', True),
        row('JPEG baseline 4:2:0', 'warm-jpeg420-bulk-evidence.json', ['observations', -1], 'WARM-JPEG420-BULK.md', True),
        row('JPEG baseline 4:2:2', 'warm-jpeg422-rows-evidence.json', ['observations', -1], 'WARM-JPEG422-ROWS.md'),
        row('JPEG baseline 4:4:4, owned input', 'warm-jpeg444owned-quiet-evidence.json', ['observations', -1], 'WARM-JPEG444-OWNED-QUIET.md', True),
        row('JPEG baseline 4:4:4, native input', 'warm-jpegac-fused-evidence.json', ['observations', -1], 'WARM-JPEG-AC-FUSED.md', True),
        row('JPEG progressive gray', 'warm-jpegrefinement-evidence.json', ['observations', -1], 'WARM-JPEG-REFINEMENT.md', True),
        row('JPEG progressive 4:2:0', 'warm-jpegprogressive420-evidence.json', ['observations', -1], 'WARM-JPEG-PROGRESSIVE420.md', True),
        row('JPEG progressive 4:2:2', 'warm-jpegprogressive422-evidence.json', ['observations', -1], 'WARM-JPEG-PROGRESSIVE422.md', True),
        row('JPEG progressive 4:4:4', 'warm-jpegmask-evidence.json', ['observations', -1], 'WARM-JPEG-MASK.md', True),
        row('WebP VP8 + alpha', 'warm-vp8decode-quiet-evidence.json', ['observations', -1], 'WARM-VP8-DECODE-QUIET.md', True),
        row('WebP VP8L, literal/run input', 'warm-vp8ldecode-quiet-evidence.json', ['observations', -1], 'WARM-VP8L-DECODE-QUIET.md', True),
        row('WebP VP8L, native input', 'warm-vp8l-canonical-evidence.json', ['observations', -1], 'WARM-VP8L-CANONICAL.md', True),
    ]
    encodes = [
        row('PNG RGBA8, level 6 / None', 'warm-png8encode-evidence.json', ['observations', -1], 'WARM-PNG8-ENCODE.md', True),
        row('PNG RGBA16, level 6 / None', 'warm-png16-encode-crc-evidence.json', ['observations', -1], 'WARM-PNG16-ENCODE-CRC.md', True),
        row('JPEG 4:4:4, matched quality', 'warm-jpegencode-quiet-evidence.json', ['observations', -1], 'WARM-JPEG-ENCODE-QUIET.md', True),
        pending('WebP VP8 + alpha', 'WARM-VP8-CHROMA-REJECTED.md', 'Unmatched quality; no timing'),
        row('WebP VP8L, exact backend*', 'warm-vp8lencode-exact-evidence.json', ['observations', -1], 'WARM-VP8L-ENCODE-EXACT.md', True),
    ]
    edits = [
        row('Nearest resize RGBA8', 'warm-nearest-baseline-evidence.json', ['observation'], 'WARM-NEAREST-BASELINE.md'),
        row('Nearest resize RGBA16', 'warm-prepared-confirm-evidence.json', ['observations', -1], 'WARM-PREPARED-CONFIRM.md', True),
        row('Bilinear resize RGBA8', 'warm-bilinear8-quiet-evidence.json', ['observations', -1], 'WARM-BILINEAR8-QUIET.md', True),
        pending('Bilinear resize RGBA16', 'bilinear16-precision-evidence.json', 'Unmatched alpha; no timing'),
        pending('Bicubic resize RGBA8', 'bicubic8-boundary-evidence.json', 'Unmatched boundary/alpha; no timing'),
        row('Crop RGBA16', 'warm-crop-copy-evidence.json', ['observations', -1], 'WARM-CROP-COPY.md', True),
        row('Clone RGBA16', 'warm-clone-reuse-evidence.json', ['observations', -1], 'WARM-CLONE-REUSE.md', True),
        row('Normal orientation RGBA16', 'warm-normal-rows-evidence.json', ['observations', -1], 'WARM-NORMAL-ROWS.md'),
        row('Vertical flip RGBA16', 'warm-vflip-rows-evidence.json', ['observations', -1], 'WARM-VFLIP-ROWS.md'),
        row('Horizontal flip RGBA8', 'results/warm-horizontal-flip-2026-10-09.json', ['observations', 'confirmation', 1, 'cases', 0], 'WARM-HORIZONTAL-FLIP.md'),
        row('Horizontal flip RGBA16', 'results/warm-horizontal-flip-2026-10-09.json', ['observations', 'confirmation', 0, 'cases', 1], 'WARM-HORIZONTAL-FLIP.md'),
        row('Half turn RGBA16', 'warm-halfturn-pixels-evidence.json', ['observations', -1], 'WARM-HALFTURN-PIXELS.md', True),
        row('Clockwise quarter turn RGBA16', 'warm-clockwise-tiles-evidence.json', ['observations', -1], 'WARM-CLOCKWISE-TILES.md', True),
        row('Counterclockwise quarter turn RGBA16', 'warm-counterclockwise-tiles-evidence.json', ['observations', -1], 'WARM-COUNTERCLOCKWISE-TILES.md'),
        row('Transpose RGBA8', 'warm-transpose8-parity-evidence.json', ['observations', -1], 'WARM-TRANSPOSE8-PARITY.md', True),
        row('Transpose RGBA16', 'warm-transpose-tiles-evidence.json', ['observations', -1], 'WARM-TRANSPOSE-TILES.md'),
        row('Mirrored transpose RGBA16', 'warm-transverse-tiles-evidence.json', ['observations', -1], 'WARM-TRANSVERSE-TILES.md'),
    ]
    return f'''# Lokad.Image benchmark state

Updated 2026-10-12. Latest retained warm measurements on the dedicated AMD EPYC
9V45 VM: Ubuntu 24.04.4, .NET 10.0.8 / SDK 10.0.204, FFmpeg n8.1
(`9047fa1b084f`). One pinned CPU, one codec/conversion/filter thread, normal SIMD,
tiering off, workstation nonconcurrent GC. Two original 513×259 RGBA8/16 rasters;
resize/crop outputs are 256×129, quarter turns/transposes 259×513.

Times cover complete public operations and independent output acquisition/release,
including admission, conversion and disposal. Native wrappers/contexts are reused;
managed editing may reuse released pixel storage. No output is cached. Startup,
I/O, IPC, fixture preparation and validation are excluded. Each linked report
declares the exact workload, input and measured source; these are rolling results
at those sources, not a simultaneous rerun of the current HEAD.

Ratio = Lokad.Image / the faster FFmpeg pre/post mean; lower is faster. Upper is
the retained conservative confidence bound, taking the larger filtered/all-actual
bound where available. **Confirmed** requires an independent quiet repeat and
upper ≤2. Failed guards remain failures. Full required warm coverage is **open**.
Recent confirmations use `exclusive-vm-idle-v2`: live job checks and
five-second aggregate/CPU0 idle samples; historical load is diagnostic only.
Earlier receipts retain their original load guard and its failures.

*VP8L encode uses **libwebp exact=1 (FFmpeg encoder backend)** through its public
API. The public FFmpeg encoder changes hidden RGB and exposes no exact option.
Both subjects preserve every decoded RGBA sample, but compression differs:
Lokad.Image emits 533,100 bytes versus 183,812 bytes. This is an exact-pixel
timing comparison; compression efficiency parity is not established.

**Decode**

{table(decodes)}

**Encode**

{table(encodes)}

**Editing**

{table(edits)}

Allocations are managed bytes per timed operation, from BenchmarkDotNet memory
diagnostics or raw GC allocated bytes / operation count (rounded up, including
harness overhead). Decoder timing uses resident encoded spans. Separate fresh
public Stream controls measure Paeth8 **2,275,800**, Paeth16 **4,713,416**, and
Adam7-16 **5,058,912 B/op**, identically on both OSes with normal/scalar code.
These are distinct boundaries. FFmpeg allocation totals are **not measured**;
they must not be interpreted as zero. Retained owners and cold allocation patterns
need separate coverage.

Other depths/directions/profiles and horizontal-flip quiet
certification remain open. Unmatched cases receive no parity timing. GIF and
animation are deferred. No averages establish the uniform ≤2× target.
Horizontal flips retain the worst upper bound of their two historical launches.

Maintain this summary whenever a performance round is accepted: update the
corresponding evidence selection in [the renderer](../benchmarks/render-benchmark.py),
regenerate with `python benchmarks/render-benchmark.py`, and run it with `--check`.
Preserve superseded raw evidence and failed runs in their linked reports. Add a
new row only for material coverage; prioritize the largest remaining gap one
primitive at a time. See the [measurement recipe](../tools/WarmReference/README.md).
'''


if __name__ == '__main__':
    text = render()
    output = ROOT / 'docs/benchmark.md'
    assert text.count('| Operation / fixed profile |') == 3
    if '--check' in sys.argv:
        assert output.read_text(encoding='utf-8') == text, 'Benchmark summary differs from retained evidence.'
        print('Benchmark summary matches retained evidence (three tables).')
    else:
        output.write_text(text, encoding='utf-8', newline='\n')
