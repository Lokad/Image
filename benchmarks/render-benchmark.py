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
        row('WebP VP8 + alpha, fresh seekable Stream', 'warm-vp8-shared-small-rows-evidence.json', ['cases', 'vp8-stream', -1], 'WARM-VP8-SHARED-SMALL-ROWS.md', verification='Current gap (short; qualified)', current=True),
        row('JPEG 4:4:4, forward-only / exact reference', 'warm-jpeg-paired-entropy-evidence.json', ['cases', 'jpeg444', -1], 'WARM-JPEG-PAIRED-ENTROPY.md', verification='Current gap (short; qualified)', current=True),
        row('JPEG 4:2:0 from caller RGB samples', 'warm-jpeg-paired-entropy-evidence.json', ['observations', -1], 'WARM-JPEG-PAIRED-ENTROPY.md', confirmed=True, current=True),
        row('JPEG RGBA16 4:2:0 / caller IMemoryBudget', 'warm-current-jpeg-root-lanczos-evidence.json', ['cases', 'JpegShared16', -1], 'WARM-CURRENT-JPEG-ROOT-LANCZOS.md', verification='Current gap (short; qualified)', current=True),
        row('JPEG grayscale, standalone SIMD libjpeg-turbo', 'warm-jpeg-paired-entropy-evidence.json', ['cases', 'gray', -1], 'WARM-JPEG-PAIRED-ENTROPY.md', verification='Current gap (short; qualified)', current=True),
        row('JPEG from caller GRAY8, standalone SIMD libjpeg-turbo', 'warm-jpeg-paired-entropy-evidence.json', ['cases', 'callerGray', -1], 'WARM-JPEG-PAIRED-ENTROPY.md', verification='Current gap (short; qualified)', current=True),
        row('VP8 diagram, effort2 / distortion-only search', 'warm-vp8-signed-domain-guard-evidence.json', ['cases', 'DiagramLossyDistortion8', -1], 'WARM-VP8-SIGNED-DOMAIN-GUARD.md', verification='Current gap (short; qualified)', current=True),
        row('VP8 RGBA8', 'warm-vp8-signed-domain-guard-evidence.json', ['cases', 'vp8', -1], 'WARM-VP8-SIGNED-DOMAIN-GUARD.md', verification='Current gap (short; qualified)', current=True),
        row('VP8 opaque photograph', 'warm-vp8-signed-domain-guard-evidence.json', ['cases', 'PhotographLossy8', -1], 'WARM-VP8-SIGNED-DOMAIN-GUARD.md', confirmed=True, current=True),
        row('VP8 opaque text/diagram', 'warm-vp8-signed-domain-guard-evidence.json', ['cases', 'DiagramLossy8', -1], 'WARM-VP8-SIGNED-DOMAIN-GUARD.md', verification='Current gap (short; qualified)', current=True),
        row('VP8 diagram, effort6 / trellis', 'warm-vp8-signed-domain-guard-evidence.json', ['cases', 'DiagramLossyTrellis8', -1], 'WARM-VP8-SIGNED-DOMAIN-GUARD.md', verification='Current gap (short; qualified)', current=True),
        row('VP8 opaque RGBA16, including normalization', 'warm-vp8-signed-domain-guard-evidence.json', ['cases', 'EncodeOpaque16', -1], 'WARM-VP8-SIGNED-DOMAIN-GUARD.md', verification='Current gap (short; qualified)', current=True),
        row('VP8L RGBA8 / libwebp exact=1', 'warm-vp8l-indexed-histogram-replay-evidence.json', ['cases', 'vp8l', -1], 'WARM-VP8L-INDEXED-HISTOGRAM-REPLAY.md', verification='Current gap (short; qualified)', current=True),
        row('VP8L RGBA16, including normalization / libwebp exact=1', 'warm-vp8l-indexed-histogram-replay-evidence.json', ['cases', 'Encode16', -1], 'WARM-VP8L-INDEXED-HISTOGRAM-REPLAY.md', verification='Current gap (short; qualified)', current=True),
        row('VP8L async, fresh nonseekable output / libwebp exact=1', 'warm-vp8l-indexed-histogram-replay-evidence.json', ['cases', 'AsyncNonSeekable', -1], 'WARM-VP8L-INDEXED-HISTOGRAM-REPLAY.md', confirmed=True, current=True),
        row('VP8L photograph / libwebp exact=1', 'warm-vp8l-photograph-confirmation-evidence.json', ['observations', -1], 'WARM-VP8L-PHOTOGRAPH-CONFIRMATION.md', confirmed=True, current=True),
        row('VP8L text/diagram / libwebp exact=1', 'warm-vp8l-indexed-histogram-replay-evidence.json', ['cases', 'DiagramLossless8', -1], 'WARM-VP8L-INDEXED-HISTOGRAM-REPLAY.md', confirmed=True, current=True),
        row('PNG RGBA8, SmallestSize9 / native9', 'warm-current-encoder-anchors-evidence.json', ['cases', 'png8', -1], 'WARM-CURRENT-ENCODER-ANCHORS.md', verification='Current gap (short; qualified)', current=True),
        row('Bilinear resize RGBA16', 'warm-current-editing-anchors-evidence.json', ['cases', 'bilinearDown16', -1], 'WARM-CURRENT-EDITING-ANCHORS.md', verification='Current gap (short; qualified)', current=True),
        row('Bicubic resize RGBA8', 'warm-bicubic-byte-emitter-evidence.json', ['cases', 'Down8', -1], 'WARM-BICUBIC-BYTE-EMITTER.md', verification='Gap (short; qualified)'),
        row('Bicubic upscale RGBA16', 'warm-bicubic-byte-emitter-evidence.json', ['cases', 'Up16', -1], 'WARM-BICUBIC-BYTE-EMITTER.md', verification='Gap (short; qualified)'),
        row('Bicubic RGBA16, width down / height up / input+output+4 KiB', 'warm-current-bicubic-cross-streaming-evidence.json', ['cases', 'TightCross16', -1], 'WARM-CURRENT-BICUBIC-CROSS-STREAMING.md', verification='Current gap (short; qualified)', current=True),
        row('Bicubic RGBA8, 32×16 / input+output+4 KiB', 'warm-current-bicubic-cross-streaming-evidence.json', ['cases', 'Tight32x16_8', -1], 'WARM-CURRENT-BICUBIC-CROSS-STREAMING.md', verification='Current gap (short; qualified)', current=True),
        row('Bicubic RGBA8, 32×16 / default workspace', 'warm-current-bicubic-cross-streaming-evidence.json', ['cases', 'Down32x16_8', -1], 'WARM-CURRENT-BICUBIC-CROSS-STREAMING.md', confirmed=True, current=True),
        row('Lanczos3 reduction RGBA8', 'warm-current-jpeg-root-lanczos-evidence.json', ['cases', 'Reduce8', -1], 'WARM-CURRENT-JPEG-ROOT-LANCZOS.md', verification='Current gap (short; qualified)', current=True),
        row('Lanczos3 reduction RGBA16', 'warm-current-jpeg-root-lanczos-evidence.json', ['cases', 'Reduce16', -1], 'WARM-CURRENT-JPEG-ROOT-LANCZOS.md', verification='Current gap (short; qualified)', current=True),
        row('Lanczos3 enlargement RGBA8', 'warm-current-jpeg-root-lanczos-evidence.json', ['cases', 'Enlarge8', -1], 'WARM-CURRENT-JPEG-ROOT-LANCZOS.md', verification='Current gap (short; qualified)', current=True),
        row('Lanczos3 enlargement RGBA16', 'warm-current-jpeg-root-lanczos-evidence.json', ['cases', 'Enlarge16', -1], 'WARM-CURRENT-JPEG-ROOT-LANCZOS.md', verification='Current gap (short; qualified)', current=True),
    ]
    return """# Benchmark state

Updated 2026-10-27. Development0.2.0 remains under qualification; warm single-CPU
parity and replacement readiness are open. Compression correctness precedes CPU
gains. Published0.1.0 remains unchanged.

Pre-cleanup costs describe their frozen producers, **not the cleaned build**.
Current independent confirmations and short screens are labeled below; other
required paths remain open. Indexed VP8L tile reconstruction removes a full
histogram slab and copy while preserving exact packets. Async allocation falls
37.73% to 29,689,080 B/op; independent cost is 1.622× (upper 1.641×), exact 183,812 B.
Diagram independently measures 11.12 ms, 1.166× (upper 1.204×), 27,419,167 B/op and
exact 7,234 B: 15.68% less elapsed than its preceding independent mean. Separate
runs are not paired baseline gains. Exact signed VP8 coefficient validation now
confirms 15.483 ms for the photograph, 1.590× (upper 1.610×), 809,072 B/op and
exact 17,688 B: 3.40% below its preceding independent mean. Six affected VP8
anchors are reconciled. Lossless photograph now independently confirms 1.599×
(upper 1.629×), 20,410,208 B/op and exact 160,032 B, without a production change.
Async VP8L 1.622× remains the largest selected independent gap.
Five further named costs are refreshed without production changes: shared-root
JPEG RGBA16 1.462×; Lanczos3 reduction8/16 1.478×/1.438× and enlargement8/16
1.039×/1.310×. These remain short screens, with exact packets/managed frames and
unchanged approved native bounds. No independent parity claim follows.
The existing default-workspace bicubic RGBA8 32×16 reduction independently
passes at 0.927×, upper 0.934×, with exact pixels and 199,372 B/op. This passing
scope is precise; its constrained-workspace counterpart and RGBA16 cross-axis
control remain short gaps at 1.235× and 1.549×.
Required upper ≤1× stays open.

The [complete historical inventory](benchmark-pre-cleanup.md) retains all257cases,
raw links and baseline exceptions; [coverage](../benchmarks/WARM-COVERAGE.md)
defines the remaining finite scope and evidence invalidation.

| Current qualification | Evidence | Status |
| --- | --- | --- |
| Packet/pixel/memory/cancellation regression | [Current qualification](../benchmarks/WARM-VP8-SIGNED-DOMAIN-GUARD.md) | 4,959 checks/OS; 638 affected scalar WebP/OS and same actual package passed |
| Benchmark delivery/inspection validation | [Cleanup follow-up](../benchmarks/WARM-ASYNC-WEBP-CURRENT.md) | 9 Windows / 11 Linux groups; production qualification reused |
| Additional required search algorithm | [Distortion-only VP8 scope](../benchmarks/WARM-COVERAGE.md) | Existing diagram, Q75/effort2; current qualified short gap |
| CPU and allocation measurements after cleanup | [VP8 signed guard](../benchmarks/WARM-VP8-SIGNED-DOMAIN-GUARD.md), [JPEG append](../benchmarks/WARM-JPEG-PAIRED-ENTROPY.md), [indexed histogram replay](../benchmarks/WARM-VP8L-INDEXED-HISTOGRAM-REPLAY.md), [shared prediction rows](../benchmarks/WARM-VP8-SHARED-SMALL-ROWS.md) | Exact packets; async VP8L allocation falls 37.73%; actual capacity admission and retained ownership remain |
| Full warm target | Independent quiet upper≤1× for all 255 named required paths, one CPU/operation | Open |

This table selects current paths; unlisted inventory costs still need applicable evidence.
B/op is managed allocation, distinct
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
