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


def row(label, name, path, report, confirmed=False, verification=None):
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
    if verification:
        assert not confirmed and quiet and ratio <= 2
        status = verification
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
        row('PNG8 Stream, orient/crop/nearest fit', 'warm-png-composition8-evidence.json', ['observations', -1], 'WARM-PNG-COMPOSITION8.md', True),
        row('PNG16 Stream, orient/crop/nearest fit', 'warm-png-fused-evidence.json', ['observations', -1], 'WARM-PNG-FUSED-INFLATION.md', True),
        row('PNG RGBA8, fresh seekable Stream', 'warm-stream-png8-evidence.json', ['observations', -1], 'WARM-STREAM-PNG8.md', True),
        row('PNG RGBA16, fresh seekable Stream', 'warm-stream-png16-evidence.json', ['observations', -1], 'WARM-STREAM-PNG16.md', True),
        row('JPEG420 restart, fresh seekable Stream', 'warm-jpegstream-evidence.json', ['observations', -1], 'WARM-JPEG-STREAM.md', True),
        row('WebP VP8 + alpha, fresh seekable Stream', 'warm-vp8stream-evidence.json', ['observations', -1], 'WARM-VP8-STREAM.md', True),
        row('WebP VP8L, fresh seekable Stream', 'warm-vp8lstream-evidence.json', ['observations', -1], 'WARM-VP8L-STREAM.md', True),
        row('PNG RGBA8, None', 'warm-pngtightcancel8-evidence.json', ['regressions', 1], 'WARM-PNG-TIGHT-CANCELABLE8.md', verification='Regression screen; prior confirmed'),
        row('PNG RGBA8, None / live cancelable token', 'warm-png-boundaries-current-evidence.json', ['cases', 'byteDefault', -1], 'WARM-PNG-DECODE-BOUNDARIES.md', True),
        row('PNG RGBA8, None / exact working limit', 'warm-pngcountall-evidence.json', ['observations', -1], 'WARM-PNG-COUNTING.md', True),
        row('PNG RGBA8, None / exact limit + live cancelable token', 'warm-pngtightcancel8-evidence.json', ['observations', -1], 'WARM-PNG-TIGHT-CANCELABLE8.md', True),
        row('PNG RGBA8, stored Deflate / None', 'warm-pngstore8decode-evidence.json', ['observations', -1], 'WARM-PNG-STORED8-DECODE.md', True),
        row('PNG RGBA16, stored Deflate / None', 'warm-pngstored16-decode-evidence.json', ['observations', -1], 'WARM-PNG-STORED16-DECODE.md', True),
        row('PNG RGBA16, None', 'warm-pngliteralrun-evidence.json', ['observations', -1], 'WARM-PNG-LITERAL-RUN.md', True),
        row('PNG RGBA16, None / exact working limit', 'warm-png-boundaries-current-evidence.json', ['cases', 'wordExact', -1], 'WARM-PNG-DECODE-BOUNDARIES.md', True),
        row('PNG RGBA16, None / live cancelable token', 'warm-pngcountall-evidence.json', ['wordConfirmations', 1], 'WARM-PNG-COUNTING.md', True),
        row('PNG RGBA16, None / exact limit + live cancelable token', 'warm-pngcountall-evidence.json', ['wordConfirmations', 0], 'WARM-PNG-COUNTING.md', True),
        row('PNG RGBA8, Paeth', 'warm-paeth4-evidence.json', ['observations', -1], 'WARM-PAETH-FOUR-CHANNEL.md', True),
        row('PNG RGBA8, Up', 'warm-pngup8-baseline-evidence.json', ['observations', -1], 'WARM-PNG-UP8-BASELINE.md', True),
        row('PNG RGBA16, Up', 'warm-pngup16-evidence.json', ['observations', -1], 'WARM-PNG-UP16.md', True),
        row('PNG RGBA8, Sub', 'warm-png-sub8-current-evidence.json', ['observations', -1], 'WARM-PNG-SUB8-CURRENT.md', True),
        row('PNG RGBA16, Sub', 'warm-pngsub16-evidence.json', ['observations', -1], 'WARM-PNG-SUB16.md', True),
        row('PNG RGBA8, Average', 'warm-pngaverage8-baseline-evidence.json', ['observations', -1], 'WARM-PNG-AVERAGE8-BASELINE.md', True),
        row('PNG RGBA16, Average', 'warm-pngaverage16-evidence.json', ['observations', -1], 'WARM-PNG-AVERAGE16.md', True),
        row('PNG RGBA16, Paeth', 'warm-pngsub16-evidence.json', ['regressions', 1], 'WARM-PNG-SUB16.md', verification='Regression screen; prior confirmed'),
        row('PNG RGB8, Paeth', 'warm-pngrgbpaeth8-evidence.json', ['observations', -1], 'WARM-PNG-RGB8-PAETH.md', True),
        row('PNG RGBA16, Adam7', 'warm-pngscatter-evidence.json', ['observations', -1], 'WARM-PNG-SCATTER.md', True),
        row('PNG gray1 → RGBA8, Adam7', 'warm-pnggray1adam7-evidence.json', ['observations', -1], 'WARM-PNG-GRAY1-ADAM7.md', True),
        row('PNG RGB16, None', 'warm-pngrgb16-current-evidence.json', ['observations', -1], 'WARM-PNG-RGB16-CURRENT.md', True),
        row('PNG gray8, None', 'warm-pnggray8-current-evidence.json', ['observations', -1], 'WARM-PNG-GRAY8-CURRENT.md', True),
        row('PNG gray8 + tRNS key127, None', 'warm-pnggray8trns-row-evidence.json', ['observations', -1], 'WARM-PNG-GRAY8-TRANSPARENCY.md', True),
        row('PNG gray16 + tRNS key32639, None', 'warm-pnggray16trns-evidence.json', ['observations', -1], 'WARM-PNG-GRAY16-TRANSPARENCY.md', True),
        row('PNG gray-alpha8, None', 'warm-pnggrayalpha8-row-evidence.json', ['observations', -1], 'WARM-PNG-GRAY-ALPHA8-ROW.md', True),
        row('PNG RGB8, None', 'warm-pngrgb8-row-evidence.json', ['observations', -1], 'WARM-PNG-RGB8-ROW.md', True),
        row('PNG RGB8 + tRNS key(48,139,231), None', 'warm-pngrgb8trns-row-evidence.json', ['observations', -1], 'WARM-PNG-RGB8-TRANSPARENCY.md', True),
        row('PNG RGB16 + tRNS key(12347,35806,59265), None', 'warm-pngrgb16trns-row-evidence.json', ['observations', -1], 'WARM-PNG-RGB16-TRANSPARENCY.md', True),
        row('PNG gray16, None', 'warm-pnggray16-row-evidence.json', ['observations', -1], 'WARM-PNG-GRAY16-ROW.md', True),
        row('PNG gray-alpha16, None', 'warm-pnggrayalpha16-row-evidence.json', ['observations', -1], 'WARM-PNG-GRAY-ALPHA16-ROW.md', True),
        row('PNG palette8 + tRNS, None', 'warm-pngpalette-simd-evidence.json', ['observations', -1], 'WARM-PNG-PALETTE-SIMD.md', True),
        row('PNG palette8 + two-entry tRNS, None', 'warm-pngpal8a2-evidence.json', ['observations', -1], 'WARM-PNG-PALETTE8-PARTIAL-ALPHA.md', True),
        row('PNG palette4 + tRNS → RGBA8, None', 'warm-pngdecode-reuse-evidence.json', ['cases', 'palette4', 'observations', -1], 'WARM-PNG-DECODE-REUSE.md', True),
        row('PNG palette2 + tRNS → RGBA8, None', 'warm-pngpalette2-row-evidence.json', ['observations', -1], 'WARM-PNG-PALETTE2-ROW.md', True),
        row('PNG palette1 + tRNS → RGBA8, None', 'warm-pngpalette1-row-evidence.json', ['observations', -1], 'WARM-PNG-PALETTE1-ROW.md', True),
        row('PNG opaque gray1 → RGBA8, None', 'warm-pnggray1-row-evidence.json', ['observations', -1], 'WARM-PNG-GRAY1-ROW.md', True),
        row('PNG gray1 + tRNS key1 → RGBA8, exact timed FFmpeg color-key', 'warm-png-gray1-transparency-evidence.json', ['observations', -1], 'WARM-PNG-GRAY1-TRANSPARENCY.md', True),
        row('PNG gray2 + tRNS key0 → RGBA8, None', 'warm-png-packed-transparency-evidence.json', ['observations', -1], 'WARM-PNG-PACKED-TRANSPARENCY.md', True),
        pending('PNG gray1/key1 and gray2/key1, unfiltered native', 'WARM-PNG-PACKED-TRANSPARENCY.md', 'Unscored: native alpha differs'),
        row('PNG opaque gray2 → RGBA8, None', 'warm-pnggray2-current-evidence.json', ['observations', -1], 'WARM-PNG-GRAY2-CURRENT.md', True),
        row('PNG opaque gray4 → RGBA8, None', 'warm-pnggray4-reuse-evidence.json', ['observations', -1], 'WARM-PNG-GRAY4-REUSE.md', True),
        row('JPEG baseline gray', 'warm-jpegac-fused-evidence.json', ['regressionControl', 'observations', -1], 'WARM-JPEG-AC-FUSED.md', True),
        row('JPEG baseline 4:2:0', 'warm-jpeg420-bulk-evidence.json', ['observations', -1], 'WARM-JPEG420-BULK.md', True),
        row('JPEG baseline 4:2:0, row restart markers', 'warm-jpeg-restartdecode-evidence.json', ['observations', -1], 'WARM-JPEG-RESTART-DECODE.md', True),
        row('JPEG baseline 4:2:2', 'warm-coverage-quiet-evidence.json', ['cases', 'jpeg422', 'observations', -1], 'WARM-COVERAGE-QUIET.md', True),
        row('JPEG baseline 4:4:4, owned input', 'warm-jpeg444owned-quiet-evidence.json', ['observations', -1], 'WARM-JPEG444-OWNED-QUIET.md', True),
        row('JPEG baseline 4:4:4, native input', 'warm-jpeg444canceldecode-evidence.json', ['regressions', 0], 'WARM-JPEG-CANCELABLE-DECODE.md', verification='Regression screen; prior confirmed'),
        row('JPEG baseline 4:4:4, native input / live cancelable token', 'warm-jpegprogressive444cancel-evidence.json', ['regressions', 1], 'WARM-JPEG-PROGRESSIVE-CANCELABLE444.md', verification='Regression screen; prior confirmed'),
        row('JPEG progressive gray', 'warm-jpegrefinement-evidence.json', ['observations', -1], 'WARM-JPEG-REFINEMENT.md', True),
        row('JPEG progressive 4:2:0', 'warm-jpegprogressive420-evidence.json', ['observations', -1], 'WARM-JPEG-PROGRESSIVE420.md', True),
        row('JPEG progressive 4:4:4 / live cancelable token', 'warm-jpegprogressive444cancel-evidence.json', ['observations', -1], 'WARM-JPEG-PROGRESSIVE-CANCELABLE444.md', True),
        row('JPEG progressive 4:2:2', 'warm-jpegprogressive422-evidence.json', ['observations', -1], 'WARM-JPEG-PROGRESSIVE422.md', True),
        row('JPEG progressive 4:4:4', 'warm-jpegprogressive444cancel-evidence.json', ['regressions', 0], 'WARM-JPEG-PROGRESSIVE-CANCELABLE444.md', verification='Regression screen; prior confirmed'),
        row('WebP VP8 + alpha', 'warm-vp8-filter8-evidence.json', ['alphaBearingControl', 'observations', -1], 'WARM-VP8-FILTER8.md', True),
        row('WebP VP8, opaque', 'warm-vp8-filter8-evidence.json', ['observations', -1], 'WARM-VP8-FILTER8.md', True),
        row('WebP VP8L, literal/run input', 'warm-vp8ldecode-quiet-evidence.json', ['observations', -1], 'WARM-VP8L-DECODE-QUIET.md', True),
        row('WebP VP8L, native input', 'warm-vp8l-canonical-evidence.json', ['observations', -1], 'WARM-VP8L-CANONICAL.md', True),
    ]
    encodes = [
        row('PNG RGBA8, level 6 / None', 'warm-pngfast16-recode-evidence.json', ['regressions', 1], 'WARM-PNG-FAST16-RECODE.md', verification='Regression screen; prior confirmed'),
        row('PNG RGBA8, NoCompression / None', 'warm-pngstore8copy-evidence.json', ['observations', -1], 'WARM-PNG-STORED8-ENCODE.md', True),
        row('PNG RGBA16, NoCompression / None', 'warm-pngstore16-encode-evidence.json', ['observations', -1], 'WARM-PNG-STORED16-ENCODE.md', True),
        row('PNG RGBA8, Fastest managed2/native1 / None', 'warm-pngfast8-evidence.json', ['observations', -1], 'WARM-PNG-FAST8.md', True),
        row('PNG RGBA16, Fastest managed2/native1 / None', 'warm-pngfast2-evidence.json', ['stockObservations', -1], 'WARM-PNG-FAST2.md', True),
        row('PNG RGBA16, Fastest managed2/native1 / live cancelable token', 'warm-pngfast2-evidence.json', ['observations', -1], 'WARM-PNG-FAST2.md', True),
        row('PNG RGBA16, Fastest managed2/native1 / forward-only output', 'warm-pngfastforward16-evidence.json', ['observations', -1], 'WARM-PNG-FAST-FORWARD16.md', True),
        row('PNG RGBA16, level 6 / None', 'warm-pngfast2-evidence.json', ['regressions', 0], 'WARM-PNG-FAST2.md', verification='Regression screen; prior confirmed'),
        row('PNG RGB8, Optimal managed9/native6 / None', 'warm-pngbyte-optimal-evidence.json', ['cases', 'rgb', 'confirm'], 'WARM-PNG-BYTE-OPTIMAL.md', True),
        row('PNG RGB16, level 6 / None', 'warm-pngrgb16encode-evidence.json', ['observations', -1], 'WARM-PNG-RGB16-ENCODE.md', True),
        row('PNG gray8, Optimal managed9/native6 / None', 'warm-pngbyte-optimal-evidence.json', ['cases', 'gray', 'confirm'], 'WARM-PNG-BYTE-OPTIMAL.md', True),
        row('PNG gray16, level 6 / None', 'warm-pnggray16encode-evidence.json', ['observations', -1], 'WARM-PNG-GRAY16-ENCODE.md', True),
        row('PNG gray-alpha8, Optimal managed9/native6 / None', 'warm-pngbyte-optimal-evidence.json', ['cases', 'alpha', 'confirm'], 'WARM-PNG-BYTE-OPTIMAL.md', True),
        row('PNG gray-alpha16, level 6 / None', 'warm-pnggrayalpha16encode-evidence.json', ['observations', -1], 'WARM-PNG-GRAY-ALPHA16-ENCODE.md', True),
        row('JPEG 4:4:4, adaptive Huffman / matched quality', 'warm-jpeg4448-adaptive-evidence.json', ['observations', -1], 'WARM-JPEG444-RGBA8-ADAPTIVE.md', True),
        row('JPEG 4:4:4, forward-only output / geometric AC Huffman', 'warm-jpeg444stream-evidence.json', ['observations', -1], 'WARM-JPEG444-FORWARD.md', True),
        row('JPEG 4:4:4, live cancelable token / geometric AC Huffman', 'warm-jpeg420cancel16-evidence.json', ['regressions', 0], 'WARM-JPEG-FIXED-OCCUPANCY.md', verification='Regression screen; prior confirmed'),
        row('JPEG 4:4:4 from RGBA16, adaptive Huffman / full normalization', 'warm-jpeg44416-adaptive-evidence.json', ['observations', -1], 'WARM-JPEG444-RGBA16-ADAPTIVE.md', True),
        row('JPEG 4:2:2, adaptive Huffman / matched quality', 'warm-jpeg4228-adaptive-evidence.json', ['observations', -1], 'WARM-JPEG422-RGBA8-ADAPTIVE.md', True),
        row('JPEG 4:2:2 from RGBA16, adaptive Huffman / full normalization', 'warm-jpeg42216-evidence.json', ['observations', -1], 'WARM-JPEG422-RGBA16.md', True),
        row('JPEG 4:2:0, adaptive Huffman / matched quality', 'warm-jpeg420pair-evidence.json', ['observations', -1], 'WARM-JPEG420-CHROMA-PAIR.md', True),
        row('JPEG 4:2:0, adaptive / row restart markers', 'warm-jpeg-admitted-evidence.json', ['observations', -1], 'WARM-JPEG-ADMITTED.md', True),
        row('JPEG 4:2:0, forward-only output / geometric AC Huffman', 'warm-jpeg-fixed-geometric-evidence.json', ['observations', -1], 'WARM-JPEG-FIXED-GEOMETRIC.md', True),
        row('JPEG 4:2:0 from RGBA16, forward-only output', 'warm-jpeg420cancel16-evidence.json', ['regressions', 1], 'WARM-JPEG-FIXED-OCCUPANCY.md', verification='Regression screen; prior confirmed'),
        row('JPEG 4:2:0 from RGBA16, live cancelable token', 'warm-jpeg420cancel16-evidence.json', ['observations', -1], 'WARM-JPEG-FIXED-OCCUPANCY.md', True),
        row('JPEG 4:2:0 from RGBA16, adaptive Huffman', 'warm-jpeg-adaptive-evidence.json', ['observations', -1], 'WARM-JPEG-ADAPTIVE-HUFFMAN.md', True),
        pending('JPEG grayscale, single component', 'WARM-COVERAGE.md', 'Unscored: native baseline unavailable'),
        row('WebP VP8, opaque‡', 'warm-vp8-encode-evidence.json', ['observations', -1], 'WARM-VP8-OPAQUE-ENCODE.md', True),
        row('WebP VP8, opaque from RGBA16‡', 'warm-vp8opaque16-evidence.json', ['observations', -1], 'WARM-VP8-OPAQUE-RGBA16.md', True),
        row('WebP VP8 + alpha‡', 'warm-vp8-encode-evidence.json', ['regressionControls', 0], 'WARM-VP8-OPAQUE-ENCODE.md', verification='Regression screen; prior confirmed'),
        row('WebP VP8 + alpha from RGBA16‡', 'warm-vp8-encode-evidence.json', ['regressionControls', 1], 'WARM-VP8-OPAQUE-ENCODE.md', verification='Regression screen; prior confirmed'),
        row('WebP VP8L, exact backend*', 'warm-vp8lencode-exact-evidence.json', ['observations', -1], 'WARM-VP8L-ENCODE-EXACT.md', True),
        row('WebP VP8L from RGBA16, exact backend*', 'warm-vp8lencode16-evidence.json', ['observations', -1], 'WARM-VP8L-RGBA16-ENCODE.md', True),
    ]
    edits = [
        row('Horizontal flip RGBA8, two live outputs / per image', 'warm-retained-pool2-evidence.json', ['observations', -1], 'WARM-RETAINED-FLIP.md', True),
        row('Nearest resize RGBA8', 'warm-coverage-quiet-evidence.json', ['cases', 'nearest8', 'observations', -1], 'WARM-COVERAGE-QUIET.md', True),
        row('Nearest upscale RGBA8', 'warm-nearestup8-evidence.json', ['observations', -1], 'WARM-NEAREST-UP8.md', True),
        row('Nearest upscale RGBA16', 'warm-nearestup16-evidence.json', ['observations', -1], 'WARM-NEAREST-UP16.md', True),
        row('Nearest resize RGBA16', 'warm-prepared-confirm-evidence.json', ['observations', -1], 'WARM-PREPARED-CONFIRM.md', True),
        row('Bilinear resize RGBA8', 'warm-bilinear8-quiet-evidence.json', ['observations', -1], 'WARM-BILINEAR8-QUIET.md', True),
        row('Bilinear resize RGBA16†', 'warm-bilinear16-register-evidence.json', ['observations', -1], 'WARM-BILINEAR16-REGISTER.md', True),
        row('Bilinear upscale RGBA8†', 'warm-bilinearup8-batch-evidence.json', ['observations', -1], 'WARM-BILINEAR-UP8-BATCH.md', True),
        row('Bilinear upscale RGBA16†', 'warm-bilinearup16-rows-evidence.json', ['observations', -1], 'WARM-BILINEAR-UP16-ROWS.md', True),
        row('Bicubic resize RGBA8§', 'warm-bicubic16-rows-evidence.json', ['byteRegressionControl', 'observations', -1], 'WARM-BICUBIC-RGBA16-ROWS.md', True),
        row('Bicubic resize RGBA16§', 'warm-bicubic16-rows-evidence.json', ['observations', -1], 'WARM-BICUBIC-RGBA16-ROWS.md', True),
        row('Bicubic upscale RGBA8§', 'warm-bicubicup8-rows-evidence.json', ['observations', -1], 'WARM-BICUBIC-UP8-ROWS.md', True),
        row('Bicubic upscale RGBA16§', 'warm-bicubicup16-rows-evidence.json', ['observations', -1], 'WARM-BICUBIC-UP16-ROWS.md', True),
        row('Bicubic mixed-axis RGBA8§', 'warm-bicubicmixed16-rows-evidence.json', ['byteRegressionControl', 'observations', -1], 'WARM-BICUBIC-MIXED16-ROWS.md', True),
        row('Bicubic mixed-axis RGBA16§', 'warm-bicubicmixed16-rows-evidence.json', ['observations', -1], 'WARM-BICUBIC-MIXED16-ROWS.md', True),
        row('Bicubic width reduction / height enlargement RGBA8§', 'warm-bicubiccross8-rows-evidence.json', ['observations', -1], 'WARM-BICUBIC-CROSS8-ROWS.md', True),
        row('Bicubic width reduction / height enlargement RGBA16§', 'warm-bicubiccross16-rows-evidence.json', ['observations', -1], 'WARM-BICUBIC-CROSS16-ROWS.md', True),
        row('Bicubic RGBA8, 128×64 / wider support§', 'warm-bicubicwide8-rows-evidence.json', ['observations', -1], 'WARM-BICUBIC-WIDE8-ROWS.md', True),
        row('Bicubic RGBA16, 128×64 / wider support§', 'warm-bicubicwide16-rows-evidence.json', ['observations', -1], 'WARM-BICUBIC-WIDE16-ROWS.md', True),
        row('Bicubic width reduction, unchanged height RGBA8§', 'warm-bicubichorizontal8-rows-evidence.json', ['observations', -1], 'WARM-BICUBIC-HORIZONTAL8-ROWS.md', True),
        row('Bicubic width reduction, unchanged height RGBA16§', 'warm-bicubichorizontal16-rows-evidence.json', ['observations', -1], 'WARM-BICUBIC-HORIZONTAL16-ROWS.md', True),
        row('Bicubic width enlargement, unchanged height RGBA8§', 'warm-bicubichorizontalup8-rows-evidence.json', ['observations', -1], 'WARM-BICUBIC-HORIZONTAL-UP8-ROWS.md', True),
        row('Bicubic width enlargement, unchanged height RGBA16§', 'warm-bicubichorizontalup16-evidence.json', ['observations', -1], 'WARM-BICUBIC-HORIZONTAL-UP16.md', True),
        row('Bicubic RGBA8, 64×32 / support beyond 32§', 'warm-bicubic64x32-8-rows-evidence.json', ['observations', -1], 'WARM-BICUBIC-64X32-RGBA8.md', True),
        row('Bicubic RGBA16, 64×32 / support beyond 32§', 'warm-bicubic64x32-16-rows-evidence.json', ['observations', -1], 'WARM-BICUBIC-64X32-RGBA16.md', True),
        row('Bicubic RGBA8, 32×16 / support beyond 64§', 'warm-bicubic32x16-8-rows-evidence.json', ['observations', -1], 'WARM-BICUBIC-32X16-RGBA8.md', True),
        row('Bicubic RGBA16, 32×16 / support beyond 64§', 'warm-bicubic32x16-16-rows-evidence.json', ['observations', -1], 'WARM-BICUBIC-32X16-RGBA16.md', True),
        row('Bicubic RGBA8, 16×8 / adaptive support beyond 128§', 'warm-bicubic16x8-8-rows-evidence.json', ['observations', -1], 'WARM-BICUBIC-16X8-ADAPTIVE.md', True),
        row('Bicubic RGBA16, 16×8 / adaptive support beyond 128§', 'warm-bicubic16x8-16-evidence.json', ['observations', -1], 'WARM-BICUBIC-16X8-RGBA16.md', True),
        row('Bicubic RGBA8, 256×129 / input+output+4 KiB limit§', 'warm-bicubictight16-strips-evidence.json', ['byteRegressionControl', 'observations', -1], 'WARM-BICUBIC-TIGHT16-STRIPS.md', True),
        row('Bicubic RGBA16, 256×129 / input+output+4 KiB limit§', 'warm-bicubictight16-strips-evidence.json', ['observations', -1], 'WARM-BICUBIC-TIGHT16-STRIPS.md', True),
        row('Bicubic RGBA8, 128×64 / input+output+4 KiB limit§', 'warm-bicubictightwide8-strips-evidence.json', ['observations', -1], 'WARM-BICUBIC-TIGHT-WIDE8-STRIPS.md', True),
        row('Bicubic RGBA16, 128×64 / input+output+4 KiB limit§', 'warm-bicubictightwide16-strips-evidence.json', ['observations', -1], 'WARM-BICUBIC-TIGHT-WIDE16-STRIPS.md', True),
        row('Bicubic RGBA8, 64×32 / input+output+4 KiB limit§', 'warm-bicubictight64x32-8-packed-evidence.json', ['observations', -1], 'WARM-BICUBIC-TIGHT-64X32-RGBA8.md', True),
        row('Bicubic RGBA16, 64×32 / input+output+4 KiB limit§', 'warm-bicubictight64x32-16-packed-evidence.json', ['observations', -1], 'WARM-BICUBIC-TIGHT-64X32-RGBA16.md', True),
        row('Bicubic RGBA16, 32×16 / input+output+4 KiB limit§', 'warm-bicubictight32x16-16-columns-evidence.json', ['observations', -1], 'WARM-BICUBIC-TIGHT-32X16-RGBA16.md', True),
        row('Bicubic RGBA8, 32×16 / input+output+4 KiB limit§', 'warm-bicubictight32x16-8-columns-evidence.json', ['observations', -1], 'WARM-BICUBIC-TIGHT-32X16-RGBA8.md', True),
        row('Bicubic RGBA16, 16×8 / input+output+4 KiB limit§', 'warm-bicubictight16x8-16-streaming-evidence.json', ['observations', -1], 'WARM-BICUBIC-TIGHT-16X8-RGBA16.md', True),
        row('Bicubic RGBA8, 16×8 / input+output+4 KiB limit§', 'warm-bicubictight16x8-8-streaming-evidence.json', ['observations', -1], 'WARM-BICUBIC-TIGHT-16X8-RGBA8.md', True),
        row('Bicubic RGBA16 enlargement / input+output+4 KiB limit§', 'warm-bicubictighthorizontalup8-cardinal-evidence.json', ['twoAxisWordConfirmation'], 'WARM-BICUBIC-TIGHT-HORIZONTAL-UP8.md', True),
        row('Bicubic RGBA8 enlargement / input+output+4 KiB limit§', 'warm-bicubictighthorizontalup8-cardinal-evidence.json', ['twoAxisByteConfirmation'], 'WARM-BICUBIC-TIGHT-HORIZONTAL-UP8.md', True),
        row('Bicubic RGBA16, width up / height down / input+output+4 KiB limit§', 'warm-bicubictightmixed16-vertical-first-evidence.json', ['observations', -1], 'WARM-BICUBIC-TIGHT-MIXED16.md', True),
        row('Bicubic RGBA8, width up / height down / input+output+4 KiB limit§', 'warm-bicubictightmixed8-vertical-first-evidence.json', ['observations', -1], 'WARM-BICUBIC-TIGHT-MIXED8.md', True),
        row('Bicubic RGBA16, width down / height up / input+output+4 KiB limit§', 'warm-bicubictightcross16-rows-evidence.json', ['observations', -1], 'WARM-BICUBIC-TIGHT-CROSS16.md', True),
        row('Bicubic RGBA8, width down / height up / input+output+4 KiB limit§', 'warm-bicubictightcross8-rows-evidence.json', ['observations', -1], 'WARM-BICUBIC-TIGHT-CROSS8.md', True),
        row('Bicubic RGBA16, 8×4 / support beyond 256 / input+output+4 KiB limit§', 'warm-bicubictight8x4word-stream-evidence.json', ['observations', -1], 'WARM-BICUBIC-TIGHT-8X4-RGBA16.md', True),
        row('Bicubic RGBA8, 8×4 / support beyond 256 / input+output+4 KiB limit§', 'warm-bicubictight8x4byte-stream-evidence.json', ['observations', -1], 'WARM-BICUBIC-TIGHT-8X4-RGBA8.md', True),
        row('Bicubic RGBA16, 4×2 / packed support beyond 304 / input+output+4 KiB limit§', 'warm-bicubictight4x2word-packed-evidence.json', ['observations', -1], 'WARM-BICUBIC-TIGHT-4X2-RGBA16.md', True),
        row('Bicubic RGBA8, 4×2 / packed support beyond 304 / input+output+4 KiB limit§', 'warm-bicubictight4x2byte-packed-evidence.json', ['observations', -1], 'WARM-BICUBIC-TIGHT-4X2-RGBA8.md', True),
        row('Bicubic RGBA16, 2×2 / generated support beyond 544 / input+output+4 KiB limit§', 'warm-bicubictight2x2word-generated-evidence.json', ['observations', -1], 'WARM-BICUBIC-TIGHT-2X2-RGBA16.md', True),
        row('Bicubic RGBA16, 2×9 / bounded row blocks / input+output+4 KiB limit§', 'warm-bicubictight2x9word-generated-evidence.json', ['observations', -1], 'WARM-BICUBIC-TIGHT-2X9-RGBA16.md', True),
        row('Bicubic RGBA16, 4×9 / packed row blocks / input+output+4 KiB limit§', 'warm-bicubictight4x9word-generated-evidence.json', ['observations', -1], 'WARM-BICUBIC-TIGHT-4X9-RGBA16.md', True),
        row('Bicubic RGBA16, 8×17 / full-coefficient row blocks / input+output+4 KiB limit§', 'warm-bicubictight8x17word-generated-evidence.json', ['observations', -1], 'WARM-BICUBIC-TIGHT-8X17-RGBA16.md', True),
        row('Bicubic RGBA8, 8×17 / full-coefficient row blocks / input+output+4 KiB limit§', 'warm-bicubictight8x17byte-generated-evidence.json', ['observations', -1], 'WARM-BICUBIC-TIGHT-8X17-RGBA8.md', True),
        row('Bicubic RGBA8, 4×9 / packed row blocks / input+output+4 KiB limit§', 'warm-bicubictight4x9byte-generated-evidence.json', ['observations', -1], 'WARM-BICUBIC-TIGHT-4X9-RGBA8.md', True),
        row('Bicubic RGBA8, 2×9 / bounded row blocks / input+output+4 KiB limit§', 'warm-bicubictight2x9byte-generated-evidence.json', ['observations', -1], 'WARM-BICUBIC-TIGHT-2X9-RGBA8.md', True),
        row('Bicubic RGBA8, 2×2 / generated support beyond 544 / input+output+4 KiB limit§', 'warm-bicubictight2x2byte-generated-evidence.json', ['observations', -1], 'WARM-BICUBIC-TIGHT-2X2-RGBA8.md', True),
        row('Bicubic height reduction, unchanged width RGBA8§', 'warm-bicubictightvertical8-cachepref-evidence.json', ['defaultMemoryConfirmation'], 'WARM-BICUBIC-TIGHT-VERTICAL8.md', True),
        row('Bicubic height reduction, unchanged width RGBA8 / input+output+4 KiB limit§', 'warm-bicubictightvertical8-cachepref-evidence.json', ['observations', -1], 'WARM-BICUBIC-TIGHT-VERTICAL8.md', True),
        row('Bicubic height reduction, unchanged width RGBA16§', 'warm-bicubictightvertical16-cachepref-evidence.json', ['defaultMemoryConfirmation'], 'WARM-BICUBIC-TIGHT-VERTICAL16.md', True),
        row('Bicubic height reduction, unchanged width RGBA16 / input+output+4 KiB limit§', 'warm-bicubictightvertical16-cachepref-evidence.json', ['observations', -1], 'WARM-BICUBIC-TIGHT-VERTICAL16.md', True),
        row('Bicubic width reduction, unchanged height RGBA16 / input+output+4 KiB limit§', 'warm-bicubictighthorizontal16-cross-evidence.json', ['observations', -1], 'WARM-BICUBIC-TIGHT-HORIZONTAL16.md', True),
        row('Bicubic width reduction, unchanged height RGBA8 / input+output+4 KiB limit§', 'warm-bicubictighthorizontal8-cross-evidence.json', ['observations', -1], 'WARM-BICUBIC-TIGHT-HORIZONTAL8.md', True),
        row('Bicubic height enlargement, unchanged width RGBA8§', 'warm-bicubicverticalup8-rows-evidence.json', ['observations', -1], 'WARM-BICUBIC-VERTICAL-UP8-ROWS.md', True),
        row('Bicubic height enlargement, unchanged width RGBA8 / input+output+4 KiB limit§', 'warm-bicubictightverticalup8-deferred-evidence.json', ['observations', -1], 'WARM-BICUBIC-TIGHT-VERTICAL-UP8.md', True),
        row('Bicubic height enlargement, unchanged width RGBA16§', 'warm-bicubicverticalup16-rows-evidence.json', ['observations', -1], 'WARM-BICUBIC-VERTICAL-UP16-ROWS.md', True),
        row('Bicubic height enlargement, unchanged width RGBA16 / input+output+4 KiB limit§', 'warm-bicubictightverticalup16-deferred-evidence.json', ['observations', -1], 'WARM-BICUBIC-TIGHT-VERTICAL-UP16.md', True),
        row('Bicubic width enlargement, unchanged height RGBA16 / input+output+4 KiB limit§', 'warm-bicubictighthorizontalup16-cardinal-evidence.json', ['observations', -1], 'WARM-BICUBIC-TIGHT-HORIZONTAL-UP16.md', True),
        row('Bicubic width enlargement, unchanged height RGBA8 / input+output+4 KiB limit§', 'warm-bicubictighthorizontalup8-cardinal-evidence.json', ['observations', -1], 'WARM-BICUBIC-TIGHT-HORIZONTAL-UP8.md', True),
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

Updated 2026-10-15. Latest retained warm measurements on the dedicated AMD EPYC
9V45 VM: Ubuntu 24.04.4, .NET 10.0.8 / SDK 10.0.204, FFmpeg n8.1
(`9047fa1b084f`). One pinned CPU, one codec/conversion/filter thread, normal SIMD,
tiering off, workstation nonconcurrent GC. Two original 513×259 RGBA8/16 rasters,
with documented projections for other pixel formats;
downsample/crop outputs are 256×129, upscale outputs are 1027×519,
and quarter turns/transposes are 259×513.

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

† RGBA16 bilinear and the explicit RGBA8 upscale allow native-only alpha maximum
error 1 / RMSE <=0.25, alongside the same RGB bound. Up8 also includes writable
output acquisition and zero-alpha RGB clearing inside native timing. Managed
pixels remain exact against the rational reference; Down8 native alpha stays
exact. The RGBA16 independent confirmation clears 2x with both guards passing.
Earlier failed screens/prechecks and rejected prototypes remain recorded in
the linked reports.

‡ VP8 encode fixes native quality 75 / managed quality 100 at effort 4. Source
PSNR gap is 0.00365 dB within the unchanged 0.25 dB band; alpha is exact.
Managed output is 471,436 bytes versus 98,590 bytes. Quality-matched timing is
confirmed; compression efficiency parity is not established.

§ Bicubic RGBA8/16 uses **zimg, FFmpeg's zscale backend**, with timed four-pixel
clamped edge padding (4.697% additional input area); wider-support RGBA8/16 uses
eight pixels (9.489%); the 64×32 byte/word cases use sixteen (19.364%);
the 2×2 and 2×9 byte/word cases use 512 pixels (1384.169%);
the 4×2 and 4×9 byte/word cases use 256 pixels (494.787%);
the 8×4 and 8×17 byte/word cases use 128 pixels (198.069%);
the 32×16 byte/word cases use thirty-two (40.269%); the 16×8 byte/word cases use
sixty-four (86.703%). All include complete source/output
packing, filtering and independent output allocation/release. The explicit
native-only RGB/alpha maximum-1 / RMSE-0.25 bounds preserve exact managed pixels.
RGBA8 256×129 downsample scalar pixels are exact; normal SIMD differs by one RGB and one
alpha sample. Enlargement and mixed-axis scalar results are bounded too. RGBA16 normal/scalar results both stay within those bounds. This
comparison is distinct from the unmatched ordinary FFmpeg CLI.
For RGBA16 height enlargement with unchanged width only, the approved native RGB
bound is two samples where reference alpha is at most sixteen, one elsewhere.
RGB/alpha RMSE ≤0.25 and alpha maximum one remain unchanged; managed pixels are
exact. Negative controls reject errors outside this scope. The original strict
refusal and rejected conversion diagnostic remain unscored.
Bicubic enlargement, mixed-axis resize, wider-support downsampling and single-axis reduction use premultiplied float output and timed straight-RGBA
unpremultiplication/quantization, preserving unclamped alpha for the color ratio
and clearing RGB when rounded alpha is zero. Its packing uses equivalent bounded
quantization; the slower general-purpose-math comparison remains superseded.
The horizontal/vertical RGBA8 cases preserve frozen original binary64 pixels exactly;
six/twenty-five independently certified exact rational alpha halves retain the rounding
already permitted by SPEC.md. Vertical RGBA8 enlargement likewise preserves its
one independently certified exact rational blue half. No managed pixel tolerance is added.

Allocations are managed bytes per timed operation, from BenchmarkDotNet memory
diagnostics or raw GC allocated bytes / operation count (rounded up, including
harness overhead). Decoder rows use resident encoded spans unless labeled fresh Stream. The fresh
seekable PNG rows include input acquisition and copies for both subjects; their
linked reports define the full boundaries, including timed native PNG16 endian
conversion and fresh owned native-endian output. Separate fresh
public Stream controls measure Paeth8 **2,275,800**, Paeth16 **4,713,416**, and
Adam7-16 **5,058,912 B/op**, identically on both OSes with normal/scalar code.
These are distinct boundaries. FFmpeg allocation totals are **not measured**;
they must not be interpreted as zero. The composed PNG8 Stream/orient/crop/fit
row includes every managed intermediate and the complete native filter graph;
its output is 127x64. Other retained-owner and cold allocation patterns
need separate coverage.

The latest accepted round confirms the existing RGBA8 PNG Sub workload at
**1.686x / upper 1.697x**, with **4,456 B/op** and exact pixels. The original
packet, qualified core, harness and native worker are unchanged; all twelve
confirmation actuals and both quiet guards pass. Existing **3,428 Release tests
per OS**, including **898 live comparisons**, and the same actual package's
consumer plus all three examples on Windows/Linux are reused. No production,
harness or native rebuild, full suite, package proof, method audit or profile
is repeated. An extra preparation execution after a private flag typo remains
retained unscored and excluded from comparative statistics. Prior Sub and packed
transparency results retain their own sources and evidence.

PNG RGB8/gray8/gray-alpha8 encoding uses managed level9/native6 with exact samples
and the unchanged 5% IDAT-size gate; earlier level6/level6 refusals remain unscored.
GIF and animation are deferred. Full required warm coverage remains **open**;
the [material coverage audit](../benchmarks/WARM-COVERAGE.md) tracks remaining
layouts and public boundaries. Stop confirmed pairs; rolling older-source results
do not prove uniform current-source parity. Linked reports preserve superseded
evidence, failed guards, rejected trials and exact package/source qualification.

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
