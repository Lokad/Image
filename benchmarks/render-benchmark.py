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


def row(label, name, path, report, confirmed=False, verification=None, current=False):
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
        assert quiet
        assert evidence.get('independentQuietConfirmationThisPair') or observation.get('certifiedThisPair')
    if verification:
        assert not confirmed and quiet
    memory = observation.get('managedMemory')
    if memory:
        allocated = memory['BytesAllocatedPerOperation']
    else:
        gc = observation['managed']['rawGc']
        match = re.fullmatch(r'// GC:\s+\d+\s+\d+\s+\d+\s+(\d+)\s+(\d+)', gc)
        assert match, gc
        allocated = math.ceil(int(match[1]) / int(match[2]))
    assert allocated >= 0
    status = 'Historical independent; remeasure' if confirmed else 'Historical short; remeasure'
    if current:
        if confirmed:
            status = 'Current parity confirmed' if upper <= 1 else ('Current gap (independent)' if ratio > 1 else 'Current confidence open')
        else:
            assert verification
            status = verification
    return f'| [{label}](../benchmarks/{report}) | {stats["Mean"] / 1000:,.2f} | {native / 1000:,.2f} | {ratio:.3f} / {upper:.3f} | {allocated:,} | {status} |'


def table(rows):
    return '\n'.join(['| Operation / fixed profile | Lokad µs | Reference µs | Ratio / upper | Lokad allocated B/op | Status |',
                      '| --- | ---: | ---: | ---: | ---: | --- |', *rows])


def render():
    anchors = [
        row('WebP VP8 + alpha, fresh seekable Stream', 'warm-vp8-vector-color-evidence.json', ['observations', -1], 'WARM-VP8-VECTOR-COLOR.md', confirmed=True, current=True),
        row('JPEG 4:4:4, forward-only / exact reference', 'warm-jpeg-paired-entropy-evidence.json', ['cases', 'jpeg444', -1], 'WARM-JPEG-PAIRED-ENTROPY.md', verification='Current gap (short; qualified)', current=True),
        row('JPEG 4:2:0 from caller RGB samples', 'warm-jpeg-paired-entropy-evidence.json', ['observations', -1], 'WARM-JPEG-PAIRED-ENTROPY.md', confirmed=True, current=True),
        row('JPEG grayscale, standalone SIMD libjpeg-turbo', 'warm-jpeg-paired-entropy-evidence.json', ['cases', 'gray', -1], 'WARM-JPEG-PAIRED-ENTROPY.md', verification='Current gap (short; qualified)', current=True),
        row('JPEG from caller GRAY8, standalone SIMD libjpeg-turbo', 'warm-jpeg-paired-entropy-evidence.json', ['cases', 'callerGray', -1], 'WARM-JPEG-PAIRED-ENTROPY.md', verification='Current gap (short; qualified)', current=True),
        row('VP8 RGBA8', 'warm-vp8-luma4-rate-bound-evidence.json', ['observations', -1], 'WARM-VP8-LUMA4-RATE-BOUND.md', confirmed=True, current=True),
        row('VP8L RGBA8 / libwebp exact=1', 'warm-vp8l-chain-capacity-evidence.json', ['observations', -1], 'WARM-VP8L-CHAIN-CAPACITY.md', confirmed=True, current=True),
        row('VP8L photograph / libwebp exact=1', 'warm-current-compression-controls-evidence.json', ['cases', 'PhotographLossless8', -1], 'WARM-CURRENT-COMPRESSION-CONTROLS.md', verification='Current gap (short; qualified)', current=True),
        row('VP8L text/diagram / libwebp exact=1', 'warm-current-compression-controls-evidence.json', ['cases', 'DiagramLossless8', -1], 'WARM-CURRENT-COMPRESSION-CONTROLS.md', verification='Current gap (short; qualified)', current=True),
        row('PNG RGBA8, SmallestSize9 / native9', 'warm-current-encoder-anchors-evidence.json', ['cases', 'png8', -1], 'WARM-CURRENT-ENCODER-ANCHORS.md', verification='Current gap (short; qualified)', current=True),
        row('Bilinear resize RGBA16', 'warm-current-editing-anchors-evidence.json', ['cases', 'bilinearDown16', -1], 'WARM-CURRENT-EDITING-ANCHORS.md', verification='Current gap (short; qualified)', current=True),
        row('Bicubic resize RGBA8', 'warm-bicubic-byte-emitter-evidence.json', ['cases', 'Down8', -1], 'WARM-BICUBIC-BYTE-EMITTER.md', verification='Gap (short; qualified)'),
        row('Bicubic upscale RGBA16', 'warm-bicubic-byte-emitter-evidence.json', ['cases', 'Up16', -1], 'WARM-BICUBIC-BYTE-EMITTER.md', verification='Gap (short; qualified)'),
        row('Lanczos3 reduction RGBA8', 'warm-lanczos3-certified-fma-evidence.json', ['cases', 'Reduce8', -1], 'WARM-LANCZOS3-CERTIFIED-FMA.md', verification='Gap (short; qualified)'),
        row('Lanczos3 enlargement RGBA16', 'warm-lanczos3-certified-fma-evidence.json', ['cases', 'Enlarge16', -1], 'WARM-LANCZOS3-CERTIFIED-FMA.md', verification='Gap (short; qualified)'),
    ]
    return """# Benchmark state

Updated2026-10-26. Development0.2.0 remains under qualification; warm single-CPU
parity and replacement readiness are open. Compression correctness precedes CPU
gains. Published0.1.0 remains unchanged.

The cleanup removes fuel, ceiling reservations and unpublished wrappers.
Pre-cleanup timings describe their frozen producers, **not the cleaned build**.
VP8 encoding/stream decoding, VP8L and caller-RGB420 JPEG have current independent
cost confirmations.
Paired JPEG entropy appends use 6.7% less elapsed than the preceding quiet short,
with unchanged allocations and exact packets. Other shared-writer JPEG encoding
costs become historical; JPEG444 and the two grayscale boundaries now have current
short screens, without an independent parity claim. VP8 rate pruning and matcher
storage remain qualified;
PNG SmallestSize retains unchanged current short evidence. Exact eight-pixel
VP8 color/chroma batching reduces complete decode elapsed by21.5% versus its
preceding short, confirming a1.301× gap with unchanged1,966,583B/op; RGBA16 bilinear reduction has
a current 1.339× short gap with exact managed words. Existing VP8L photograph and
diagram controls now have current 1.619×/2.282× screens and 29,610,520/45,404,827 B/op;
their packets remain byte exact. Older producer ratios are historical. Parity remains open.
The [complete historical inventory](benchmark-pre-cleanup.md) retains all257cases,
raw links and baseline exceptions; [coverage](../benchmarks/WARM-COVERAGE.md)
defines the remaining finite scope and evidence invalidation.

| Current qualification | Evidence | Status |
| --- | --- | --- |
| Packet/pixel/memory/cancellation regression | [Current qualification](../benchmarks/WARM-VP8-VECTOR-COLOR.md) | 4,922 checks/OS; 601 affected scalar WebP/OS and same actual package passed |
| CPU and allocation measurements after cleanup | [VP8 decode color](../benchmarks/WARM-VP8-VECTOR-COLOR.md), [JPEG append](../benchmarks/WARM-JPEG-PAIRED-ENTROPY.md), [VP8 rate bound](../benchmarks/WARM-VP8-LUMA4-RATE-BOUND.md), [matcher storage](../benchmarks/WARM-VP8L-CHAIN-CAPACITY.md) | Exact packets; other required paths and full parity open |
| Full warm target | Independent quiet upper≤1× for each required path, one CPU/operation | Open |

Current screens are labeled; other cost anchors remain **pre-cleanup**. B/op is managed allocation, distinct
from peak cooperative admission and native allocation. Ratios use the faster native
bracket; upper is the recorded conservative confidence bound. Historical shorts
are leads for measurement, not current demonstrated gaps.

""" + table(anchors) + """

Complete encoder packets remain the acceptance boundary; metadata differences
are limited to the approved JPEG attribution COM. These are the named controls,
not broad arbitrary-content/settings compression claims.

| Frozen packet scope | Lokad.Image bytes | Reference bytes | Verification |
| --- | ---: | ---: | --- |
| PNG,14depth/color/policy controls | Identical | Identical | Complete raw packets |
| JPEG RGBA8,444 /422 /420 | 133039 /71166 /52091 | 133041 /71168 /52093 | Exact except approved COM |
| JPEG gray8, Q69 | 58910 | 58910 | Complete raw packet; SIMD libjpeg-turbo |
| VP8L photograph / diagram | 160032 /7234 | 160032 /7234 | Exact=1; complete raw packets |
| VP8 photograph / diagram, Q75/method4 | 17688 /7100 | 17688 /7100 | Complete raw packets |

[Codec parity](codec-parity.md) records the full packet identities and producer
scope: JPEG10controls, PNG14, VP8L19selected+2forced, VP8 twenty513×259identities
plus the large retry control. Decoder exactness and editing oracle requirements
remain separate. A timing pass,5%IDAT tolerance or equal numeric qualities cannot
replace packet equality.

Reference operations include normalization, input delivery and independent output
ownership. FFmpeg is pinned; grayscale encoding uses the approved explicitly
labeled SIMD libjpeg-turbo3.1.4.1 reference; VP8L uses libwebp exact=1; bicubic uses
zimg, FFmpeg's zscale backend, with the approved full halo/alpha/packing boundary.
Native-only resize tolerances remain in the retained reports; managed pixels
stay exact. GIF/animation, startup, concurrency and arbitrary profiles are excluded.

Use the exclusive quiet VM, versioned idle/process guard and sequential Release
jobs. Screen narrowly; independently confirm promising cases and stop passing
ones. Reuse immutable builds and unchanged qualification. Historical experiments,
failed guards and rejected candidates stay retained; none silently becomes current
evidence. Regenerate with `python benchmarks/render-benchmark.py`, then `--check`.
"""


if __name__ == '__main__':
    text = render()
    output = ROOT / 'docs/benchmark.md'
    assert len(re.findall(r'^\| ---', text, re.M)) == 3
    if '--check' in sys.argv:
        assert output.read_text(encoding='utf-8') == text, 'Benchmark summary differs from retained evidence.'
        print('Benchmark summary matches retained evidence (three tables).')
    else:
        output.write_text(text, encoding='utf-8', newline='\n')
