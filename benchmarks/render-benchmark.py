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
    status = 'Confirmed' if confirmed else ('Above 2x / open' if quiet and ratio > 2 else ('Quiet certification open' if quiet is None else ('Quiet repeat open' if quiet else 'Quiet guard failed / open')))
    if not confirmed and quiet and evidence.get('independentQuietRepeatCompleted') and upper > 2:
        status = 'Upper above 2x / open'
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
        row('PNG RGBA8, stored Deflate / None', 'warm-pngstore8decode-evidence.json', ['observations', -1], 'WARM-PNG-STORED8-DECODE.md', True),
        row('PNG RGBA16, None', 'warm-pngliteralrun-evidence.json', ['observations', -1], 'WARM-PNG-LITERAL-RUN.md', True),
        row('PNG RGBA8, Paeth', 'warm-paeth4-evidence.json', ['observations', -1], 'WARM-PAETH-FOUR-CHANNEL.md', True),
        row('PNG RGBA8, Up', 'warm-pngup8-baseline-evidence.json', ['observations', -1], 'WARM-PNG-UP8-BASELINE.md', True),
        row('PNG RGBA8, Sub', 'warm-pngsub8-baseline-evidence.json', ['observations', -1], 'WARM-PNG-SUB8-BASELINE.md', True),
        row('PNG RGBA8, Average', 'warm-pngaverage8-baseline-evidence.json', ['observations', -1], 'WARM-PNG-AVERAGE8-BASELINE.md', True),
        row('PNG RGBA16, Paeth', 'warm-paethvector-evidence.json', ['observations', -1], 'WARM-PAETH-VECTOR.md', True),
        row('PNG RGBA16, Adam7', 'warm-pngscatter-evidence.json', ['observations', -1], 'WARM-PNG-SCATTER.md', True),
        row('PNG RGB16, None', 'warm-pngrgb16-current-evidence.json', ['observations', -1], 'WARM-PNG-RGB16-CURRENT.md', True),
        row('PNG gray8, None', 'warm-pnggray8-current-evidence.json', ['observations', -1], 'WARM-PNG-GRAY8-CURRENT.md', True),
        row('PNG gray8 + tRNS key127, None', 'warm-pnggray8trns-row-evidence.json', ['observations', -1], 'WARM-PNG-GRAY8-TRANSPARENCY.md', True),
        row('PNG gray16 + tRNS key32639, None', 'warm-pnggray16trns-evidence.json', ['observations', -1], 'WARM-PNG-GRAY16-TRANSPARENCY.md', True),
        row('PNG gray-alpha8, None', 'warm-pnggrayalpha8-row-evidence.json', ['observations', -1], 'WARM-PNG-GRAY-ALPHA8-ROW.md', True),
        row('PNG RGB8, None', 'warm-pngrgb8-row-evidence.json', ['observations', -1], 'WARM-PNG-RGB8-ROW.md', True),
        row('PNG RGB8 + tRNS key(48,139,231), None', 'warm-pngrgb8trns-row-evidence.json', ['observations', -1], 'WARM-PNG-RGB8-TRANSPARENCY.md', True),
        row('PNG gray16, None', 'warm-pnggray16-row-evidence.json', ['observations', -1], 'WARM-PNG-GRAY16-ROW.md', True),
        row('PNG gray-alpha16, None', 'warm-pnggrayalpha16-row-evidence.json', ['observations', -1], 'WARM-PNG-GRAY-ALPHA16-ROW.md', True),
        row('PNG palette8 + tRNS, None', 'warm-pngpalette-simd-evidence.json', ['observations', -1], 'WARM-PNG-PALETTE-SIMD.md', True),
        row('PNG palette8 + two-entry tRNS, None', 'warm-pngpal8a2-evidence.json', ['observations', -1], 'WARM-PNG-PALETTE8-PARTIAL-ALPHA.md', True),
        row('PNG palette4 + tRNS → RGBA8, None', 'warm-pngdecode-reuse-evidence.json', ['cases', 'palette4', 'observations', -1], 'WARM-PNG-DECODE-REUSE.md', True),
        row('PNG palette2 + tRNS → RGBA8, None', 'warm-pngpalette2-row-evidence.json', ['observations', -1], 'WARM-PNG-PALETTE2-ROW.md', True),
        row('PNG palette1 + tRNS → RGBA8, None', 'warm-pngpalette1-row-evidence.json', ['observations', -1], 'WARM-PNG-PALETTE1-ROW.md', True),
        row('PNG opaque gray1 → RGBA8, None', 'warm-pnggray1-row-evidence.json', ['observations', -1], 'WARM-PNG-GRAY1-ROW.md', True),
        row('PNG opaque gray2 → RGBA8, None', 'warm-pnggray2-current-evidence.json', ['observations', -1], 'WARM-PNG-GRAY2-CURRENT.md', True),
        row('PNG opaque gray4 → RGBA8, None', 'warm-pnggray4-reuse-evidence.json', ['observations', -1], 'WARM-PNG-GRAY4-REUSE.md', True),
        row('JPEG baseline gray', 'warm-jpegac-fused-evidence.json', ['regressionControl', 'observations', -1], 'WARM-JPEG-AC-FUSED.md', True),
        row('JPEG baseline 4:2:0', 'warm-jpeg420-bulk-evidence.json', ['observations', -1], 'WARM-JPEG420-BULK.md', True),
        row('JPEG baseline 4:2:2', 'warm-coverage-quiet-evidence.json', ['cases', 'jpeg422', 'observations', -1], 'WARM-COVERAGE-QUIET.md', True),
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
        row('PNG RGBA8, NoCompression / None', 'warm-pngstore8copy-evidence.json', ['observations', -1], 'WARM-PNG-STORED8-ENCODE.md', True),
        row('PNG RGBA16, level 6 / None', 'warm-png16-encode-crc-evidence.json', ['observations', -1], 'WARM-PNG16-ENCODE-CRC.md', True),
        pending('PNG RGB8, level 6 / None', 'WARM-PNG-RGB8-ENCODE-BOUNDARY.md', 'Compression-size gate failed; no timing'),
        row('PNG RGB16, level 6 / None', 'warm-pngrgb16encode-evidence.json', ['observations', -1], 'WARM-PNG-RGB16-ENCODE.md', True),
        pending('PNG gray8, level 6 / None', 'WARM-PNG-GRAY8-ENCODE-BOUNDARY.md', 'Compression-size gate failed; no timing'),
        row('PNG gray16, level 6 / None', 'warm-pnggray16encode-evidence.json', ['observations', -1], 'WARM-PNG-GRAY16-ENCODE.md', True),
        pending('PNG gray-alpha8, level 6 / None', 'WARM-PNG-GRAY-ALPHA8-ENCODE-BOUNDARY.md', 'Compression-size gate failed; no timing'),
        row('PNG gray-alpha16, level 6 / None', 'warm-pnggrayalpha16encode-evidence.json', ['observations', -1], 'WARM-PNG-GRAY-ALPHA16-ENCODE.md', True),
        row('JPEG 4:4:4, matched quality', 'warm-jpegencode-quiet-evidence.json', ['observations', -1], 'WARM-JPEG-ENCODE-QUIET.md', True),
        row('JPEG 4:4:4 from RGBA16, full normalization', 'warm-jpeg16-component-row-evidence.json', ['observations', -1], 'WARM-JPEG16-COMPONENT-ROW.md', True),
        row('JPEG 4:2:2, matched quality', 'warm-jpeg422encode-evidence.json', ['observations', -1], 'WARM-JPEG422-ENCODE.md', True),
        row('JPEG 4:2:0, matched quality', 'warm-jpeg420-chroma-row-evidence.json', ['observations', -1], 'WARM-JPEG420-CHROMA-ROW.md'),
        row('JPEG 4:2:0 from RGBA16, full normalization', 'warm-jpeg42016-entropy-rejected-evidence.json', ['observations', 0], 'WARM-JPEG42016-ENTROPY-REJECTED.md'),
        row('WebP VP8 + alpha‡', 'warm-vp8-alphaweighted-evidence.json', ['observations', -1], 'WARM-VP8-ALPHAWEIGHTED.md', True),
        row('WebP VP8 + alpha from RGBA16‡', 'warm-vp8encode16-evidence.json', ['observations', -1], 'WARM-VP8-RGBA16-ENCODE.md', True),
        row('WebP VP8L, exact backend*', 'warm-vp8lencode-exact-evidence.json', ['observations', -1], 'WARM-VP8L-ENCODE-EXACT.md', True),
        row('WebP VP8L from RGBA16, exact backend*', 'warm-vp8lencode16-evidence.json', ['observations', -1], 'WARM-VP8L-RGBA16-ENCODE.md', True),
    ]
    edits = [
        row('Nearest resize RGBA8', 'warm-coverage-quiet-evidence.json', ['cases', 'nearest8', 'observations', -1], 'WARM-COVERAGE-QUIET.md', True),
        row('Nearest resize RGBA16', 'warm-prepared-confirm-evidence.json', ['observations', -1], 'WARM-PREPARED-CONFIRM.md', True),
        row('Bilinear resize RGBA8', 'warm-bilinear8-quiet-evidence.json', ['observations', -1], 'WARM-BILINEAR8-QUIET.md', True),
        row('Bilinear resize RGBA16†', 'warm-bilinear16-register-evidence.json', ['observations', -1], 'WARM-BILINEAR16-REGISTER.md', True),
        pending('Bicubic resize RGBA8', 'bicubic8-boundary-evidence.json', 'Unmatched boundary/alpha; no timing'),
        row('Crop RGBA8', 'warm-cropclone8-flip16-evidence.json', ['cases', 'crop8', 'observations', -1], 'WARM-CROPCLONE8-FLIP16.md', True),
        row('Crop RGBA16', 'warm-crop-copy-evidence.json', ['observations', -1], 'WARM-CROP-COPY.md', True),
        row('Clone RGBA8', 'warm-cropclone8-flip16-evidence.json', ['cases', 'clone8', 'observations', -1], 'WARM-CROPCLONE8-FLIP16.md', True),
        row('Clone RGBA16', 'warm-clone-reuse-evidence.json', ['observations', -1], 'WARM-CLONE-REUSE.md', True),
        row('Normal orientation RGBA8', 'warm-copies8-quiet-evidence.json', ['cases', 'normal8', 'observations', -1], 'WARM-COPIES8-QUIET.md', True),
        row('Normal orientation RGBA16', 'warm-flip-orientation-quiet-evidence.json', ['cases', 'normal16', 'observations', -1], 'WARM-FLIP-ORIENTATION-QUIET.md', True),
        row('Vertical flip RGBA8', 'warm-flipturn8-fresh-evidence.json', ['cases', 'vertical', 'observations', -1], 'WARM-FLIPTURN8-FRESH.md', True),
        row('Vertical flip RGBA16', 'warm-directions16-quiet-evidence.json', ['cases', 'vflip16', 'observations', -1], 'WARM-DIRECTIONS16-QUIET.md', True),
        row('Horizontal flip RGBA8', 'warm-flip-orientation-quiet-evidence.json', ['cases', 'flip8', 'observations', -1], 'WARM-FLIP-ORIENTATION-QUIET.md', True),
        row('Horizontal flip RGBA16', 'warm-cropclone8-flip16-evidence.json', ['cases', 'flip16', 'observations', -1], 'WARM-CROPCLONE8-FLIP16.md', True),
        row('Half turn RGBA8', 'warm-flipturn8-fresh-evidence.json', ['cases', 'half', 'observations', -1], 'WARM-FLIPTURN8-FRESH.md', True),
        row('Half turn RGBA16', 'warm-halfturn-pixels-evidence.json', ['observations', -1], 'WARM-HALFTURN-PIXELS.md', True),
        row('Clockwise quarter turn RGBA8', 'warm-directions8-quiet-evidence.json', ['cases', 'clockwise', 'observations', -1], 'WARM-DIRECTIONS8-QUIET.md', True),
        row('Clockwise quarter turn RGBA16', 'warm-clockwise-tiles-evidence.json', ['observations', -1], 'WARM-CLOCKWISE-TILES.md', True),
        row('Counterclockwise quarter turn RGBA8', 'warm-directions8-quiet-evidence.json', ['cases', 'counterclockwise', 'observations', -1], 'WARM-DIRECTIONS8-QUIET.md', True),
        row('Counterclockwise quarter turn RGBA16', 'warm-directions16-quiet-evidence.json', ['cases', 'ccw16', 'observations', -1], 'WARM-DIRECTIONS16-QUIET.md', True),
        row('Transpose RGBA8', 'warm-transpose8-parity-evidence.json', ['observations', -1], 'WARM-TRANSPOSE8-PARITY.md', True),
        row('Transpose RGBA16', 'warm-coverage-quiet-evidence.json', ['cases', 'transpose16', 'observations', -1], 'WARM-COVERAGE-QUIET.md', True),
        row('Mirrored transpose RGBA16', 'warm-directions16-quiet-evidence.json', ['cases', 'mirror16', 'observations', -1], 'WARM-DIRECTIONS16-QUIET.md', True),
        row('Mirrored transpose RGBA8', 'warm-directions8-quiet-evidence.json', ['cases', 'transverse', 'observations', -1], 'WARM-DIRECTIONS8-QUIET.md', True),
    ]
    return f'''# Lokad.Image benchmark state

Updated 2026-10-13. Latest retained warm measurements on the dedicated AMD EPYC
9V45 VM: Ubuntu 24.04.4, .NET 10.0.8 / SDK 10.0.204, FFmpeg n8.1
(`9047fa1b084f`). One pinned CPU, one codec/conversion/filter thread, normal SIMD,
tiering off, workstation nonconcurrent GC. Two original 513×259 RGBA8/16 rasters,
with documented projections for other pixel formats;
resize/crop outputs are 256×129, quarter turns/transposes 259×513.

Times cover complete public operations and independent output acquisition/release,
including admission, conversion and disposal. Native wrappers/contexts are reused;
managed editing and PNG decode may reuse released pixel storage. No output is cached. Startup,
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

† RGBA16 bilinear allows native-only alpha maximum error 1 / RMSE <=0.25 in
full 16-bit sample units, alongside the same RGB bound. Managed pixels still
match the exact rational reference; RGBA8 native alpha stays exact. The latest
RGBA16 independent confirmation clears 2x with both quiet guards passing.
Earlier failed screens/prechecks and rejected prototypes remain recorded in
the linked reports.

‡ VP8 encode fixes native quality 75 / managed quality 100 at effort 4. Source
PSNR gap is 0.00365 dB within the unchanged 0.25 dB band; alpha is exact.
Managed output is 471,436 bytes versus 98,590 bytes. Quality-matched timing is
confirmed; compression efficiency parity is not established.

Allocations are managed bytes per timed operation, from BenchmarkDotNet memory
diagnostics or raw GC allocated bytes / operation count (rounded up, including
harness overhead). Decoder timing uses resident encoded spans. Separate fresh
public Stream controls measure Paeth8 **2,275,800**, Paeth16 **4,713,416**, and
Adam7-16 **5,058,912 B/op**, identically on both OSes with normal/scalar code.
These are distinct boundaries. FFmpeg allocation totals are **not measured**;
they must not be interpreted as zero. Retained owners and cold allocation patterns
need separate coverage.

The [material coverage audit](../benchmarks/WARM-COVERAGE.md) lists distinct
unmeasured primitives and public boundaries without expanding a profile matrix.
Other depths/directions/profiles remain open. Unmatched cases receive no parity
timing. GIF and
animation are deferred. No averages establish the uniform ≤2× target.
Historical horizontal-flip launches and their bounds remain in the earlier report.

The latest [PNG Average RGBA8 check](../benchmarks/WARM-PNG-AVERAGE8-BASELINE.md)
independently confirms 1.704x / upper 1.743x with 12,976 B/op using unchanged
production. All five filters have a confirmed RGBA8 case. The
[RGB8 encode check](../benchmarks/WARM-PNG-RGB8-ENCODE-BOUNDARY.md) is exact in
pixels/scanlines but unscored: IDAT sizes differ by 12.31%, exceeding the existing
5% gate. Row batching recovers only 120 bytes; the compression comparison remains
open. The [RGB16 encode check](../benchmarks/WARM-PNG-RGB16-ENCODE.md) independently
confirms 0.865x / upper 0.871x with 2,102,765 B/op and unchanged production.
The [gray8 encode check](../benchmarks/WARM-PNG-GRAY8-ENCODE-BOUNDARY.md) also has
exact pixels/scanlines but remains unscored: IDAT sizes differ by 16.65%, and
batching saves only 34 bytes. Its compression comparison remains open.
The [gray16 encode check](../benchmarks/WARM-PNG-GRAY16-ENCODE.md) independently
confirms 1.150x / upper 1.156x with 1,051,455 B/op. Its native baseline uses exact
red-plane extraction and includes conversion/intermediate ownership within each
operation. Production is unchanged.
The [gray-alpha8 encode check](../benchmarks/WARM-PNG-GRAY-ALPHA8-ENCODE-BOUNDARY.md)
preserves every sample with explicit timed lossless storage packing before the
native PNG encoder, but remains unscored: IDAT sizes differ by 5.17%; batching
saves only 61 bytes. Native colorspace conversion's alpha rounding is rejected.
The [gray-alpha16 encode check](../benchmarks/WARM-PNG-GRAY-ALPHA16-ENCODE.md)
independently confirms 0.903x / upper 0.915x with 1,052,368 B/op. Both complete
words and hidden RGB are exact; native storage packing/endian conversion occurs
inside every operation before FFmpeg PNG encoding. Production is unchanged.
Other material layouts and encoding projections remain open. The
[NoCompression RGBA8 check](../benchmarks/WARM-PNG-STORED8-ENCODE.md) drops from
11.526x to an independently confirmed 1.883x / upper 1.933x, with 2,102,472 B/op.
One bulk row-copy primitive preserves encoded bytes, admission and cancellation;
full tests and the same actual package pass on both OSes. The
[stored-Deflate RGBA8 decode check](../benchmarks/WARM-PNG-STORED8-DECODE.md)
independently confirms 1.159x / upper 1.168x with 12,976 B/op, unchanged production
and exact frozen input. The
[RGB16 check](../benchmarks/WARM-PNG-RGB16-CURRENT.md) is confirmed at 1.819x / upper
1.823x with 15,024 B/op. JPEG420's earlier upper-bound gap remains open.
Stop tuning confirmed pairs. Other profiles, ownership boundaries and the full
required scope remain open; earlier failures and rejected experiments stay recorded.

The [gray16 transparency check](../benchmarks/WARM-PNG-GRAY16-TRANSPARENCY.md)
independently confirms 1.722x / upper 1.755x with 10,960 B/op and unchanged
production. Full 16-bit samples, alpha and hidden RGB are exact; the prior gray8
fixture is unchanged. RGB16 transparency and other material coverage remain open.

The [RGB8 transparency round](../benchmarks/WARM-PNG-RGB8-TRANSPARENCY.md)
reduces the screened mean 59.9%, from 3.524x to an independently confirmed
1.417x / upper 1.439x with unchanged 11,984 B/op. One exact RGB-key row kernel
preserves hidden RGB, work and cancellation. Full tests and the same actual
package pass on both OSes. RGB16 transparency and other material coverage remain
open.

The [JPEG420 RGBA16 entropy checks](../benchmarks/WARM-JPEG42016-ENTROPY-REJECTED.md)
retain the validated encoder at 2.046x / upper 3.240x with 527,306 B/op. Inlining,
cached byte dispatch and symbol batching provide insufficient headroom or no
useful gain; all three are discarded. Exact original production is restored
and JPEG420 remains open. The next investigation targets Huffman coding rather
than further stream dispatch changes.

The [adaptive JPEG Huffman prototype](../benchmarks/JPEG-HUFFMAN-PROTOTYPE.md)
cuts one existing packet by 49.9% while preserving every coefficient and pixel
in independent C#, managed and normal/scalar FFmpeg checks. Production is
unchanged; complete encode speed and allocations remain unmeasured, and this
correctness result does not establish parity.

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
