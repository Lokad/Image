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
    return '\n'.join(['| Operation / fixed profile | Lokad µs | Reference µs | Ratio / upper | Lokad allocated B/op | Status |',
                      '| --- | ---: | ---: | ---: | ---: | --- |', *rows])


def render():
    decodes = [
        row('PNG palette4/full tRNS Adam7', 'warm-png-palette4-adam7-evidence.json', ['observations', -1], 'WARM-PNG-PALETTE4-ADAM7.md', True),
        row('PNG8 Stream, orient/crop/nearest fit', 'warm-current-png-stream-evidence.json', ['cases', 'PngCropFit8', -1], 'WARM-CURRENT-PNG-STREAM.md', True),
        row('PNG16 Stream, orient/crop/nearest fit', 'warm-png-fused-evidence.json', ['observations', -1], 'WARM-PNG-FUSED-INFLATION.md', True),
        row('PNG RGBA8, fresh seekable Stream', 'warm-current-png-stream-evidence.json', ['cases', 'Png8', -1], 'WARM-CURRENT-PNG-STREAM.md', True),
        row('PNG RGBA16, fresh seekable Stream', 'warm-current-png-stream-evidence.json', ['cases', 'Png16', -1], 'WARM-CURRENT-PNG-STREAM.md', True),
        row('JPEG420 restart, fresh seekable Stream', 'warm-jpeg-ac-cache-evidence.json', ['observations', 8], 'WARM-JPEG-AC-CACHE.md', True),
        row('WebP VP8 + alpha, fresh seekable Stream', 'warm-vp8stream-evidence.json', ['observations', -1], 'WARM-VP8-STREAM.md', True),
        row('WebP VP8L, fresh seekable Stream', 'warm-vp8lstream-evidence.json', ['observations', -1], 'WARM-VP8L-STREAM.md', True),
        row('PNG RGBA8, None', 'warm-current-png-decode-evidence.json', ['cases', 'Decode8', -1], 'WARM-CURRENT-PNG-DECODE.md', True),
        row('PNG RGBA8, None / live cancelable token', 'warm-png-boundaries-current-evidence.json', ['cases', 'byteDefault', -1], 'WARM-PNG-DECODE-BOUNDARIES.md', True),
        row('PNG RGBA8, None / exact working limit', 'warm-pngcountall-evidence.json', ['observations', -1], 'WARM-PNG-COUNTING.md', True),
        row('PNG RGBA8, None / exact limit + live cancelable token', 'warm-pngtightcancel8-evidence.json', ['observations', -1], 'WARM-PNG-TIGHT-CANCELABLE8.md', True),
        row('PNG RGBA8, stored Deflate / None', 'warm-current-png-decode-evidence.json', ['cases', 'DecodeNoCompression8', -1], 'WARM-CURRENT-PNG-DECODE.md', True),
        row('PNG RGBA16, stored Deflate / None', 'warm-current-png-decode-evidence.json', ['cases', 'DecodeNoCompression16', -1], 'WARM-CURRENT-PNG-DECODE.md', True),
        row('PNG RGBA16, None', 'warm-current-png16-bicubic16-evidence.json', ['cases', 'png16', -1], 'WARM-CURRENT-PNG16-BICUBIC16.md', True),
        row('PNG RGBA16, None / exact working limit', 'warm-png-boundaries-current-evidence.json', ['cases', 'wordExact', -1], 'WARM-PNG-DECODE-BOUNDARIES.md', True),
        row('PNG RGBA16, None / live cancelable token', 'warm-pngcountall-evidence.json', ['wordConfirmations', 1], 'WARM-PNG-COUNTING.md', True),
        row('PNG RGBA16, None / exact limit + live cancelable token', 'warm-pngcountall-evidence.json', ['wordConfirmations', 0], 'WARM-PNG-COUNTING.md', True),
        row('PNG RGBA8, Paeth', 'warm-current-png-decode-evidence.json', ['cases', 'DecodePaeth8', -1], 'WARM-CURRENT-PNG-DECODE.md', True),
        row('PNG RGBA8, Up', 'warm-current-png-decode-evidence.json', ['cases', 'DecodeUp8', -1], 'WARM-CURRENT-PNG-DECODE.md', True),
        row('PNG RGBA16, Up', 'warm-pngup16-evidence.json', ['observations', -1], 'WARM-PNG-UP16.md', True),
        row('PNG RGBA8, Sub', 'warm-png-sub8-current-evidence.json', ['observations', -1], 'WARM-PNG-SUB8-CURRENT.md', True),
        row('PNG RGBA16, Sub', 'warm-pngsub16-evidence.json', ['observations', -1], 'WARM-PNG-SUB16.md', True),
        row('PNG RGBA8, Average', 'warm-current-png-decode-evidence.json', ['cases', 'DecodeAverage8', -1], 'WARM-CURRENT-PNG-DECODE.md', True),
        row('PNG RGBA16, Average', 'warm-pngaverage16-evidence.json', ['observations', -1], 'WARM-PNG-AVERAGE16.md', True),
        row('PNG RGBA16, Paeth', 'warm-current-png-decode-evidence.json', ['cases', 'DecodePaeth16', -1], 'WARM-CURRENT-PNG-DECODE.md', True),
        row('PNG RGB8, Paeth', 'warm-pngrgbpaeth8-evidence.json', ['observations', -1], 'WARM-PNG-RGB8-PAETH.md', True),
        row('PNG RGB16, Paeth', 'warm-png-rgb16-paeth-evidence.json', ['observations', -1], 'WARM-PNG-RGB16-PAETH.md', True),
        row('PNG gray8, Paeth', 'warm-png-gray8-paeth-evidence.json', ['observations', -1], 'WARM-PNG-GRAY8-PAETH.md', True),
        row('PNG gray-alpha8, Paeth', 'warm-png-gray-alpha8-paeth-evidence.json', ['observations', -1], 'WARM-PNG-GRAY-ALPHA8-PAETH.md', True),
        row('PNG RGBA16, Adam7', 'warm-current-png-adam7-evidence.json', ['cases', 'DecodeAdam7_16', -1], 'WARM-CURRENT-PNG-ADAM7.md', True),
        row('PNG gray1 → RGBA8, Adam7', 'warm-current-png-adam7-evidence.json', ['cases', 'DecodeGray1Adam7', -1], 'WARM-CURRENT-PNG-ADAM7.md', True),
        row('PNG palette8 + tRNS, Adam7', 'warm-png-palette8-adam7-evidence.json', ['observations', -1], 'WARM-PNG-PALETTE8-ADAM7.md', True),
        row('PNG RGB16, None', 'warm-current-png-decode-evidence.json', ['cases', 'DecodeRgb16', -1], 'WARM-CURRENT-PNG-DECODE.md', True),
        row('PNG gray8, None', 'warm-current-png-decode-evidence.json', ['cases', 'DecodeGray8', -1], 'WARM-CURRENT-PNG-DECODE.md', True),
        row('PNG gray8 + tRNS key127, None', 'warm-current-png-decode-evidence.json', ['cases', 'DecodeGray8Transparency', -1], 'WARM-CURRENT-PNG-DECODE.md', True),
        row('PNG gray16 + tRNS key32639, None', 'warm-current-png-decode-evidence.json', ['cases', 'DecodeGray16Transparency', -1], 'WARM-CURRENT-PNG-DECODE.md', True),
        row('PNG gray-alpha8, None', 'warm-current-png-decode-evidence.json', ['cases', 'DecodeGrayAlpha8', -1], 'WARM-CURRENT-PNG-DECODE.md', True),
        row('PNG RGB8, None', 'warm-current-png-decode-evidence.json', ['cases', 'DecodeRgb8', -1], 'WARM-CURRENT-PNG-DECODE.md', True),
        row('PNG RGB8 + tRNS key(48,139,231), None', 'warm-current-png-decode-evidence.json', ['cases', 'DecodeRgb8Transparency', -1], 'WARM-CURRENT-PNG-DECODE.md', True),
        row('PNG RGB16 + tRNS key(12347,35806,59265), None', 'warm-current-png-decode-evidence.json', ['cases', 'DecodeRgb16Transparency', -1], 'WARM-CURRENT-PNG-DECODE.md', True),
        row('PNG gray16, None', 'warm-current-png-decode-evidence.json', ['cases', 'DecodeGray16', -1], 'WARM-CURRENT-PNG-DECODE.md', True),
        row('PNG gray-alpha16, None', 'warm-current-png-decode-evidence.json', ['cases', 'DecodeGrayAlpha16', -1], 'WARM-CURRENT-PNG-DECODE.md', True),
        row('PNG byte indices / sixteen-entry palette + tRNS, None', 'warm-png-short-palette-evidence.json', ['observations', 2], 'WARM-PNG-SHORT-PALETTE.md', True),
        row('PNG palette8 + tRNS, None', 'warm-current-png-decode-evidence.json', ['cases', 'DecodePalette8', -1], 'WARM-CURRENT-PNG-DECODE.md', True),
        row('PNG palette8 + two-entry tRNS, None', 'warm-current-png-decode-evidence.json', ['cases', 'DecodePalette8PartialAlpha', -1], 'WARM-CURRENT-PNG-DECODE.md', True),
        row('PNG palette4 + tRNS → RGBA8, None', 'warm-current-png-decode-evidence.json', ['cases', 'DecodePalette4', -1], 'WARM-CURRENT-PNG-DECODE.md', True),
        row('PNG palette2 + tRNS → RGBA8, None', 'warm-current-png-decode-evidence.json', ['cases', 'DecodePalette2', -1], 'WARM-CURRENT-PNG-DECODE.md', True),
        row('PNG palette1 + tRNS → RGBA8, None', 'warm-current-png-decode-evidence.json', ['cases', 'DecodePalette1', -1], 'WARM-CURRENT-PNG-DECODE.md', True),
        row('PNG opaque gray1 → RGBA8, None', 'warm-current-png-decode-evidence.json', ['cases', 'DecodeGray1', -1], 'WARM-CURRENT-PNG-DECODE.md', True),
        row('PNG gray1 + tRNS key1 → RGBA8, exact timed FFmpeg color-key', 'warm-png-gray1-transparency-evidence.json', ['observations', -1], 'WARM-PNG-GRAY1-TRANSPARENCY.md', True),
        row('PNG gray2 + tRNS key0 → RGBA8, None', 'warm-png-packed-transparency-evidence.json', ['observations', -1], 'WARM-PNG-PACKED-TRANSPARENCY.md', True),
        pending('PNG gray1/key1 and gray2/key1, unfiltered native', 'WARM-PNG-PACKED-TRANSPARENCY.md', 'Unscored: native alpha differs'),
        row('PNG opaque gray2 → RGBA8, None', 'warm-current-png-decode-evidence.json', ['cases', 'DecodeGray2', -1], 'WARM-CURRENT-PNG-DECODE.md', True),
        row('PNG opaque gray4 → RGBA8, None', 'warm-current-png-decode-evidence.json', ['cases', 'DecodeGray4', -1], 'WARM-CURRENT-PNG-DECODE.md', True),
        row('JPEG baseline gray', 'warm-current-jpeg-decode-evidence.json', ['cases', 'OwnedDecodeGray', -1], 'WARM-CURRENT-JPEG-DECODE.md', True),
        row('JPEG baseline 4:2:0', 'warm-current-jpeg-decode-evidence.json', ['cases', 'NativeDecode420', -1], 'WARM-CURRENT-JPEG-DECODE.md', True),
        row('JPEG baseline 4:2:0, row restart markers', 'warm-current-jpeg-decode-evidence.json', ['cases', 'NativeDecode420Restart', -1], 'WARM-CURRENT-JPEG-DECODE.md', True),
        row('JPEG baseline 4:2:2', 'warm-current-jpeg-decode-evidence.json', ['cases', 'NativeDecode422', -1], 'WARM-CURRENT-JPEG-DECODE.md', True),
        row('JPEG baseline 4:4:4, owned input', 'warm-current-jpeg-decode-evidence.json', ['cases', 'Decode8', -1], 'WARM-CURRENT-JPEG-DECODE.md', True),
        row('JPEG baseline 4:4:4, native input', 'warm-current-jpeg-decode-evidence.json', ['cases', 'NativeDecode8', -1], 'WARM-CURRENT-JPEG-DECODE.md', True),
        row('JPEG baseline direct RGB, full resolution', 'warm-jpeg-ac-cache-evidence.json', ['observations', 2], 'WARM-JPEG-AC-CACHE.md', True),
        row('JPEG direct RGB 4:2:2, timed centered correction', 'warm-current-jpeg-decode-evidence.json', ['cases', 'DirectRgb422', -1], 'WARM-CURRENT-JPEG-DECODE.md', True),
        row('JPEG direct RGB 4:2:0, timed component-plane interpretation', 'warm-current-jpeg-decode-evidence.json', ['cases', 'DirectRgb420', -1], 'WARM-CURRENT-JPEG-DECODE.md', True),
        row('JPEG baseline 4:4:4, native input / live cancelable token', 'warm-jpeg-ac-run-evidence.json', ['cancelableConfirmations', -1], 'WARM-JPEG-BASELINE-AC-RUN.md', True),
        row('JPEG progressive gray', 'warm-current-jpeg-progressive-evidence.json', ['cases', 'ProgressiveGray', -1], 'WARM-CURRENT-JPEG-PROGRESSIVE.md', True),
        row('JPEG progressive 4:2:0', 'warm-current-jpeg-progressive-evidence.json', ['cases', 'Progressive420', -1], 'WARM-CURRENT-JPEG-PROGRESSIVE.md', True),
        row('JPEG progressive 4:4:4 / live cancelable token', 'warm-jpegprogressive444cancel-evidence.json', ['observations', -1], 'WARM-JPEG-PROGRESSIVE-CANCELABLE444.md', True),
        row('JPEG progressive 4:2:2', 'warm-current-jpeg-progressive-evidence.json', ['cases', 'Progressive422', -1], 'WARM-CURRENT-JPEG-PROGRESSIVE.md', True),
        row('JPEG progressive 4:4:4', 'warm-jpeg-ac-cache-evidence.json', ['observations', 7], 'WARM-JPEG-AC-CACHE.md', True),
        row('WebP VP8 + alpha', 'warm-vp8-filter8-evidence.json', ['alphaBearingControl', 'observations', -1], 'WARM-VP8-FILTER8.md', True),
        row('WebP VP8 opaque, simple luma filter', 'warm-vp8-simple-filter-evidence.json', ['observations', -1], 'WARM-VP8-SIMPLE-FILTER.md', True),
        row('WebP VP8 opaque, live cancelable token', 'warm-vp8-cancelable-evidence.json', ['observations', -1], 'WARM-VP8-CANCELABLE.md', True),
        row('WebP VP8, opaque', 'warm-vp8-filter8-evidence.json', ['observations', -1], 'WARM-VP8-FILTER8.md', True),
        row('WebP VP8L, sixteen-color indexed input', 'warm-vp8l-indexed16-evidence.json', ['observations', -1], 'WARM-VP8L-INDEXED16.md', True),
        row('WebP VP8L, literal/run input', 'warm-current-vp8l-literal-evidence.json', ['observations', -1], 'WARM-CURRENT-VP8L-LITERAL.md', True),
        row('WebP VP8L, native input', 'warm-vp8l-canonical-evidence.json', ['observations', -1], 'WARM-VP8L-CANONICAL.md', True),
        row('WebP VP8L, native input / live cancelable token', 'warm-vp8l-cancelable-evidence.json', ['observations', -1], 'WARM-VP8L-CANCELABLE.md', True),
    ]
    encodes = [
        row('PNG RGBA8, level 6 / None', 'warm-current-png-encode-evidence.json', ['cases', 'Encode8', -1], 'WARM-CURRENT-PNG-ENCODE.md', True),
        row('PNG RGBA8, NoCompression / None', 'warm-pngstore8copy-evidence.json', ['observations', -1], 'WARM-PNG-STORED8-ENCODE.md', True),
        row('PNG RGBA16, NoCompression / None', 'warm-pngstore16-encode-evidence.json', ['observations', -1], 'WARM-PNG-STORED16-ENCODE.md', True),
        row('PNG RGBA8, Fastest managed2/native1 / None', 'warm-pngfast8-evidence.json', ['observations', -1], 'WARM-PNG-FAST8.md', True),
        row('PNG RGBA16, Fastest managed2/native1 / None', 'warm-pngfast2-evidence.json', ['stockObservations', -1], 'WARM-PNG-FAST2.md', True),
        row('PNG RGBA16, Fastest managed2/native1 / live cancelable token', 'warm-pngfast2-evidence.json', ['observations', -1], 'WARM-PNG-FAST2.md', True),
        row('PNG RGBA16, Fastest managed2/native1 / forward-only output', 'warm-pngfastforward16-evidence.json', ['observations', -1], 'WARM-PNG-FAST-FORWARD16.md', True),
        row('PNG RGBA16, level 6 / None', 'warm-current-png-encode-evidence.json', ['cases', 'Encode16', -1], 'WARM-CURRENT-PNG-ENCODE.md', True),
        row('PNG RGBA8, SmallestSize9 / native9', 'warm-png-smallest8-evidence.json', ['observations', -1], 'WARM-PNG-SMALLEST8.md', True),
        row('PNG RGBA16, SmallestSize managed9/native9 / None', 'warm-png-smallest16-evidence.json', ['observations', -1], 'WARM-PNG-SMALLEST16.md', True),
        row('PNG RGB8, Optimal managed9/native6 / None', 'warm-pngbyte-optimal-evidence.json', ['cases', 'rgb', 'confirm'], 'WARM-PNG-BYTE-OPTIMAL.md', True),
        row('PNG RGB16, level 6 / None', 'warm-pngrgb16encode-evidence.json', ['observations', -1], 'WARM-PNG-RGB16-ENCODE.md', True),
        row('PNG gray8, Optimal managed9/native6 / None', 'warm-pngbyte-optimal-evidence.json', ['cases', 'gray', 'confirm'], 'WARM-PNG-BYTE-OPTIMAL.md', True),
        row('PNG gray16, level 6 / None', 'warm-pnggray16encode-evidence.json', ['observations', -1], 'WARM-PNG-GRAY16-ENCODE.md', True),
        row('PNG gray-alpha8, Optimal managed9/native6 / None', 'warm-pngbyte-optimal-evidence.json', ['cases', 'alpha', 'confirm'], 'WARM-PNG-BYTE-OPTIMAL.md', True),
        row('PNG gray-alpha16, level 6 / None', 'warm-pnggrayalpha16encode-evidence.json', ['observations', -1], 'WARM-PNG-GRAY-ALPHA16-ENCODE.md', True),
        row('JPEG 4:4:4, adaptive Huffman / matched quality', 'warm-current-jpeg-adaptive-evidence.json', ['cases', 'Encode8', -1], 'WARM-CURRENT-JPEG-ADAPTIVE.md', True),
        row('JPEG 4:4:4, forward-only output / geometric AC Huffman', 'warm-jpeg444stream-evidence.json', ['observations', -1], 'WARM-JPEG444-FORWARD.md', True),
        row('JPEG 4:4:4, live cancelable token / geometric AC Huffman', 'warm-jpeg420cancel16-evidence.json', ['regressions', 0], 'WARM-JPEG-FIXED-OCCUPANCY.md', verification='Regression screen; prior confirmed'),
        row('JPEG 4:4:4 from RGBA16, adaptive Huffman / full normalization', 'warm-current-jpeg-adaptive-evidence.json', ['cases', 'Encode16', -1], 'WARM-CURRENT-JPEG-ADAPTIVE.md', True),
        row('JPEG 4:2:2, adaptive Huffman / matched quality', 'warm-current-jpeg-adaptive-evidence.json', ['cases', 'Encode422_8', -1], 'WARM-CURRENT-JPEG-ADAPTIVE.md', True),
        row('JPEG 4:2:2 from RGBA16, adaptive Huffman / full normalization', 'warm-current-jpeg-adaptive-evidence.json', ['cases', 'Encode422_16', -1], 'WARM-CURRENT-JPEG-ADAPTIVE.md', True),
        row('JPEG 4:2:0, adaptive Huffman / matched quality', 'warm-jpeg-ac-planning-evidence.json', ['observations', 4], 'WARM-JPEG-AC-PLANNING.md', True),
        row('JPEG 4:2:0, adaptive / row restart markers', 'warm-jpeg-ac-planning-evidence.json', ['observations', 2], 'WARM-JPEG-AC-PLANNING.md', True),
        row('JPEG 4:2:0, forward-only output / geometric AC Huffman', 'warm-jpeg-fixed-geometric-evidence.json', ['observations', -1], 'WARM-JPEG-FIXED-GEOMETRIC.md', True),
        row('JPEG 4:2:0 from RGBA16, forward-only output', 'warm-jpeg420cancel16-evidence.json', ['regressions', 1], 'WARM-JPEG-FIXED-OCCUPANCY.md', verification='Regression screen; prior confirmed'),
        row('JPEG 4:2:0 from RGBA16, live cancelable token', 'warm-jpeg420cancel16-evidence.json', ['observations', -1], 'WARM-JPEG-FIXED-OCCUPANCY.md', True),
        row('JPEG 4:2:0 from RGBA16, adaptive Huffman', 'warm-jpeg-ac-planning-evidence.json', ['observations', 6], 'WARM-JPEG-AC-PLANNING.md', True),
        row('JPEG grayscale, standalone SIMD libjpeg-turbo', 'warm-jpeg-grayscale-evidence.json', ['observations', -1], 'WARM-JPEG-GRAYSCALE.md', True),
        row('WebP VP8, opaque‡', 'warm-vp8-encode-evidence.json', ['observations', -1], 'WARM-VP8-OPAQUE-ENCODE.md', True),
        row('WebP VP8, opaque from RGBA16‡', 'warm-vp8opaque16-evidence.json', ['observations', -1], 'WARM-VP8-OPAQUE-RGBA16.md', True),
        row('WebP VP8 + alpha‡', 'warm-current-vp8-encode-evidence.json', ['cases', 'Encode8', -1], 'WARM-CURRENT-VP8-ENCODE.md', True),
        row('WebP VP8 + alpha from RGBA16‡', 'warm-current-vp8-encode-evidence.json', ['cases', 'Encode16', -1], 'WARM-CURRENT-VP8-ENCODE.md', True),
        row('WebP VP8L, exact backend*', 'warm-vp8lencode-exact-evidence.json', ['observations', -1], 'WARM-VP8L-ENCODE-EXACT.md', True),
        row('WebP VP8L, opaque RGBA8 / exact backend*', 'warm-vp8l-opaque8-evidence.json', ['observations', -1], 'WARM-VP8L-OPAQUE8.md', True),
        row('WebP VP8L from RGBA16, exact backend*', 'warm-vp8lencode16-evidence.json', ['observations', -1], 'WARM-VP8L-RGBA16-ENCODE.md', True),
    ]
    edits = [
        row('Horizontal flip RGBA8, two live outputs / per image', 'warm-retained-pool2-evidence.json', ['observations', -1], 'WARM-RETAINED-FLIP.md', True),
        row('Nearest resize RGBA8', 'warm-coverage-quiet-evidence.json', ['cases', 'nearest8', 'observations', -1], 'WARM-COVERAGE-QUIET.md', True),
        row('Nearest upscale RGBA8', 'warm-nearestup8-evidence.json', ['observations', -1], 'WARM-NEAREST-UP8.md', True),
        row('Nearest upscale RGBA16', 'warm-nearestup16-evidence.json', ['observations', -1], 'WARM-NEAREST-UP16.md', True),
        row('Nearest resize RGBA16', 'warm-prepared-confirm-evidence.json', ['observations', -1], 'WARM-PREPARED-CONFIRM.md', True),
        row('Bilinear resize RGBA8', 'warm-bilinear8-quiet-evidence.json', ['observations', -1], 'WARM-BILINEAR8-QUIET.md', True),
        row('Bilinear resize RGBA16†', 'warm-current-bilinear-evidence.json', ['cases', 'Down16', -1], 'WARM-CURRENT-BILINEAR.md', True),
        row('Bilinear upscale RGBA8†', 'warm-current-bilinear-evidence.json', ['cases', 'Up8', -1], 'WARM-CURRENT-BILINEAR.md', True),
        row('Bilinear upscale RGBA16†', 'warm-current-bilinear-evidence.json', ['cases', 'Up16', -1], 'WARM-CURRENT-BILINEAR.md', True),
        row('RGBA8 bicubic, single-pixel reduction', 'warm-bicubic-single-pixel-evidence.json', ['observations', -1], 'WARM-BICUBIC-SINGLE-PIXEL.md', True),
        row('Bicubic resize RGBA8§', 'warm-current-bicubic-evidence.json', ['cases', 'Down8', 'observations', -1], 'WARM-CURRENT-BICUBIC.md', True),
        row('Bicubic resize RGBA16§', 'warm-current-bicubic-evidence.json', ['cases', 'Down16', 'observations', -1], 'WARM-CURRENT-BICUBIC.md', True),
        row('Bicubic upscale RGBA8§', 'warm-current-bicubic-evidence.json', ['cases', 'Up8', 'observations', -1], 'WARM-CURRENT-BICUBIC.md', True),
        row('Bicubic upscale RGBA16§', 'warm-current-bicubic-evidence.json', ['cases', 'Up16', 'observations', -1], 'WARM-CURRENT-BICUBIC.md', True),
        row('Bicubic mixed-axis RGBA8§', 'warm-current-bicubic-evidence.json', ['cases', 'Mixed8', 'observations', -1], 'WARM-CURRENT-BICUBIC.md', True),
        row('Bicubic mixed-axis RGBA16§', 'warm-current-bicubic-evidence.json', ['cases', 'Mixed16', 'observations', -1], 'WARM-CURRENT-BICUBIC.md', True),
        row('Bicubic width reduction / height enlargement RGBA8§', 'warm-current-bicubic-evidence.json', ['cases', 'Cross8', 'observations', -1], 'WARM-CURRENT-BICUBIC.md', True),
        row('Bicubic width reduction / height enlargement RGBA16§', 'warm-bicubiccross16-rows-evidence.json', ['observations', -1], 'WARM-BICUBIC-CROSS16-ROWS.md', True),
        row('Bicubic RGBA8, 128×64 / wider support§', 'warm-current-bicubic-evidence.json', ['cases', 'Wide8', 'observations', -1], 'WARM-CURRENT-BICUBIC.md', True),
        row('Bicubic RGBA16, 128×64 / wider support§', 'warm-current-bicubic-evidence.json', ['cases', 'Wide16', 'observations', -1], 'WARM-CURRENT-BICUBIC.md', True),
        row('Bicubic width reduction, unchanged height RGBA8§', 'warm-current-bicubic-evidence.json', ['cases', 'Horizontal8', 'observations', -1], 'WARM-CURRENT-BICUBIC.md', True),
        row('Bicubic width reduction, unchanged height RGBA16§', 'warm-current-bicubic-evidence.json', ['cases', 'Horizontal16', 'observations', -1], 'WARM-CURRENT-BICUBIC.md', True),
        row('Bicubic width enlargement, unchanged height RGBA8§', 'warm-current-bicubic-evidence.json', ['cases', 'HorizontalUp8', 'observations', -1], 'WARM-CURRENT-BICUBIC.md', True),
        row('Bicubic width enlargement, unchanged height RGBA16§', 'warm-current-png16-bicubic16-evidence.json', ['cases', 'horizontalUp16', -1], 'WARM-CURRENT-PNG16-BICUBIC16.md', True),
        row('Bicubic RGBA8, 64×32 / support beyond 32§', 'warm-current-bicubic-evidence.json', ['cases', 'Down64x32_8', 'observations', -1], 'WARM-CURRENT-BICUBIC.md', True),
        row('Bicubic RGBA16, 64×32 / support beyond 32§', 'warm-current-bicubic-evidence.json', ['cases', 'Down64x32_16', 'observations', -1], 'WARM-CURRENT-BICUBIC.md', True),
        row('Bicubic RGBA8, 32×16 / support beyond 64§', 'warm-current-bicubic-evidence.json', ['cases', 'Down32x16_8', 'observations', -1], 'WARM-CURRENT-BICUBIC.md', True),
        row('Bicubic RGBA16, 32×16 / support beyond 64§', 'warm-bicubic32x16-16-rows-evidence.json', ['observations', -1], 'WARM-BICUBIC-32X16-RGBA16.md', True),
        row('Bicubic RGBA8, 16×8 / adaptive support beyond 128§', 'warm-bicubic16x8-8-rows-evidence.json', ['observations', -1], 'WARM-BICUBIC-16X8-ADAPTIVE.md', True),
        row('Bicubic RGBA16, 16×8 / adaptive support beyond 128§', 'warm-bicubic16x8-16-evidence.json', ['observations', -1], 'WARM-BICUBIC-16X8-RGBA16.md', True),
        row('Bicubic RGBA8, 256×129 / input+output+4 KiB limit§', 'warm-current-bicubic-evidence.json', ['cases', 'TightDown8', 'observations', -1], 'WARM-CURRENT-BICUBIC.md', True),
        row('Bicubic RGBA16, 256×129 / input+output+4 KiB limit§', 'warm-current-bicubic-evidence.json', ['cases', 'TightDown16', 'observations', -1], 'WARM-CURRENT-BICUBIC.md', True),
        row('Bicubic RGBA8, 128×64 / input+output+4 KiB limit§', 'warm-current-bicubic-evidence.json', ['cases', 'TightWide8', 'observations', -1], 'WARM-CURRENT-BICUBIC.md', True),
        row('Bicubic RGBA16, 128×64 / input+output+4 KiB limit§', 'warm-current-bicubic-evidence.json', ['cases', 'TightWide16', 'observations', -1], 'WARM-CURRENT-BICUBIC.md', True),
        row('Bicubic RGBA8, 64×32 / input+output+4 KiB limit§', 'warm-current-bicubic-evidence.json', ['cases', 'Tight64x32_8', 'observations', -1], 'WARM-CURRENT-BICUBIC.md', True),
        row('Bicubic RGBA16, 64×32 / input+output+4 KiB limit§', 'warm-current-bicubic-evidence.json', ['cases', 'Tight64x32_16', 'observations', -1], 'WARM-CURRENT-BICUBIC.md', True),
        row('Bicubic RGBA16, 32×16 / input+output+4 KiB limit§', 'warm-bicubictight32x16-16-columns-evidence.json', ['observations', -1], 'WARM-BICUBIC-TIGHT-32X16-RGBA16.md', True),
        row('Bicubic RGBA8, 32×16 / input+output+4 KiB limit§', 'warm-current-bicubic-evidence.json', ['cases', 'Tight32x16_8', 'observations', -1], 'WARM-CURRENT-BICUBIC.md', True),
        row('Bicubic RGBA16, 16×8 / input+output+4 KiB limit§', 'warm-bicubictight16x8-16-streaming-evidence.json', ['observations', -1], 'WARM-BICUBIC-TIGHT-16X8-RGBA16.md', True),
        row('Bicubic RGBA8, 16×8 / input+output+4 KiB limit§', 'warm-bicubictight16x8-8-streaming-evidence.json', ['observations', -1], 'WARM-BICUBIC-TIGHT-16X8-RGBA8.md', True),
        row('Bicubic RGBA16 enlargement / input+output+4 KiB limit§', 'warm-bicubictighthorizontalup8-cardinal-evidence.json', ['twoAxisWordConfirmation'], 'WARM-BICUBIC-TIGHT-HORIZONTAL-UP8.md', True),
        row('Bicubic RGBA8 enlargement / input+output+4 KiB limit§', 'warm-bicubictighthorizontalup8-cardinal-evidence.json', ['twoAxisByteConfirmation'], 'WARM-BICUBIC-TIGHT-HORIZONTAL-UP8.md', True),
        row('Bicubic RGBA16, width up / height down / input+output+4 KiB limit§', 'warm-current-bicubic-evidence.json', ['cases', 'TightMixed16', 'observations', -1], 'WARM-CURRENT-BICUBIC.md', True),
        row('Bicubic RGBA8, width up / height down / input+output+4 KiB limit§', 'warm-current-bicubic-evidence.json', ['cases', 'TightMixed8', 'observations', -1], 'WARM-CURRENT-BICUBIC.md', True),
        row('Bicubic RGBA16, width down / height up / input+output+4 KiB limit§', 'warm-current-bicubic-evidence.json', ['cases', 'TightCross16', 'observations', -1], 'WARM-CURRENT-BICUBIC.md', True),
        row('Bicubic RGBA8, width down / height up / input+output+4 KiB limit§', 'warm-current-bicubic-evidence.json', ['cases', 'TightCross8', 'observations', -1], 'WARM-CURRENT-BICUBIC.md', True),
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
        row('Bicubic width reduction, unchanged height RGBA16 / input+output+4 KiB limit§', 'warm-current-bicubic-evidence.json', ['cases', 'TightHorizontal16', 'observations', -1], 'WARM-CURRENT-BICUBIC.md', True),
        row('Bicubic width reduction, unchanged height RGBA8 / input+output+4 KiB limit§', 'warm-bicubictighthorizontal8-cross-evidence.json', ['observations', -1], 'WARM-BICUBIC-TIGHT-HORIZONTAL8.md', True),
        row('Bicubic height enlargement, unchanged width RGBA8§', 'warm-bicubicverticalup8-rows-evidence.json', ['observations', -1], 'WARM-BICUBIC-VERTICAL-UP8-ROWS.md', True),
        row('Bicubic height enlargement, unchanged width RGBA8 / input+output+4 KiB limit§', 'warm-current-bicubic-evidence.json', ['cases', 'TightVerticalUp8', 'observations', -1], 'WARM-CURRENT-BICUBIC.md', True),
        row('Bicubic height enlargement, unchanged width RGBA16§', 'warm-bicubicverticalup16-rows-evidence.json', ['observations', -1], 'WARM-BICUBIC-VERTICAL-UP16-ROWS.md', True),
        row('Bicubic height enlargement, unchanged width RGBA16 / input+output+4 KiB limit§', 'warm-current-bicubic-evidence.json', ['cases', 'TightVerticalUp16', 'observations', -1], 'WARM-CURRENT-BICUBIC.md', True),
        row('Bicubic width enlargement, unchanged height RGBA16 / input+output+4 KiB limit§', 'warm-bicubictighthorizontalup16-cardinal-evidence.json', ['observations', -1], 'WARM-BICUBIC-TIGHT-HORIZONTAL-UP16.md', True),
        row('Bicubic width enlargement, unchanged height RGBA8 / input+output+4 KiB limit§', 'warm-bicubictighthorizontalup8-cardinal-evidence.json', ['observations', -1], 'WARM-BICUBIC-TIGHT-HORIZONTAL-UP8.md', True),
        row('Crop RGBA8', 'warm-current-exact-edits-evidence.json', ['cases', 'Crop8', 'observations', -1], 'WARM-CURRENT-EXACT-EDITS.md', True),
        row('Crop RGBA16', 'warm-current-exact-edits-evidence.json', ['cases', 'Crop16', 'observations', -1], 'WARM-CURRENT-EXACT-EDITS.md', True),
        row('Clone RGBA8', 'warm-current-exact-edits-evidence.json', ['cases', 'Clone8', 'observations', -1], 'WARM-CURRENT-EXACT-EDITS.md', True),
        row('Clone RGBA16', 'warm-current-exact-edits-evidence.json', ['cases', 'Clone16', 'observations', -1], 'WARM-CURRENT-EXACT-EDITS.md', True),
        row('Normal orientation RGBA8', 'warm-current-exact-edits-evidence.json', ['cases', 'NormalOrientation8', 'observations', -1], 'WARM-CURRENT-EXACT-EDITS.md', True),
        row('Normal orientation RGBA16', 'warm-current-exact-edits-evidence.json', ['cases', 'NormalOrientation16', 'observations', -1], 'WARM-CURRENT-EXACT-EDITS.md', True),
        row('Vertical flip RGBA8', 'warm-current-exact-edits-evidence.json', ['cases', 'VerticalFlip8', 'observations', -1], 'WARM-CURRENT-EXACT-EDITS.md', True),
        row('Vertical flip RGBA16', 'warm-current-exact-edits-evidence.json', ['cases', 'VerticalFlip16', 'observations', -1], 'WARM-CURRENT-EXACT-EDITS.md', True),
        row('Horizontal flip RGBA8', 'warm-current-exact-edits-evidence.json', ['cases', 'HorizontalFlip8', 'observations', -1], 'WARM-CURRENT-EXACT-EDITS.md', True),
        row('Horizontal flip RGBA16', 'warm-current-exact-edits-evidence.json', ['cases', 'HorizontalFlip16', 'observations', -1], 'WARM-CURRENT-EXACT-EDITS.md', True),
        row('Half turn RGBA8', 'warm-current-exact-edits-evidence.json', ['cases', 'HalfTurn8', 'observations', -1], 'WARM-CURRENT-EXACT-EDITS.md', True),
        row('Half turn RGBA16', 'warm-current-exact-edits-evidence.json', ['cases', 'HalfTurn16', 'observations', -1], 'WARM-CURRENT-EXACT-EDITS.md', True),
        row('Clockwise quarter turn RGBA8', 'warm-current-exact-edits-evidence.json', ['cases', 'Clockwise8', 'observations', -1], 'WARM-CURRENT-EXACT-EDITS.md', True),
        row('Clockwise quarter turn RGBA16', 'warm-current-exact-edits-evidence.json', ['cases', 'Clockwise16', 'observations', -1], 'WARM-CURRENT-EXACT-EDITS.md', True),
        row('Counterclockwise quarter turn RGBA8', 'warm-current-exact-edits-evidence.json', ['cases', 'Counterclockwise8', 'observations', -1], 'WARM-CURRENT-EXACT-EDITS.md', True),
        row('Counterclockwise quarter turn RGBA16', 'warm-current-exact-edits-evidence.json', ['cases', 'Counterclockwise16', 'observations', -1], 'WARM-CURRENT-EXACT-EDITS.md', True),
        row('Transpose RGBA8', 'warm-current-exact-edits-evidence.json', ['cases', 'Transpose8', 'observations', -1], 'WARM-CURRENT-EXACT-EDITS.md', True),
        row('Transpose RGBA16', 'warm-current-exact-edits-evidence.json', ['cases', 'Transpose16', 'observations', -1], 'WARM-CURRENT-EXACT-EDITS.md', True),
        row('Mirrored transpose RGBA16', 'warm-current-exact-edits-evidence.json', ['cases', 'Transverse16', 'observations', -1], 'WARM-CURRENT-EXACT-EDITS.md', True),
        row('Mirrored transpose RGBA8', 'warm-current-exact-edits-evidence.json', ['cases', 'Transverse8', 'observations', -1], 'WARM-CURRENT-EXACT-EDITS.md', True),
    ]
    return f'''# Lokad.Image benchmark state

Updated 2026-10-16. Latest retained warm measurements on the dedicated AMD EPYC
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

Ratio = Lokad.Image / the faster reference pre/post mean; lower is faster. Upper is
the retained conservative confidence bound, taking the larger filtered/all-actual
bound where available. **Confirmed** requires an independent quiet repeat and
upper ≤2. Failed guards remain failures. Full required warm coverage is **open**.
Current-path reconciliation closes all twenty named exact edits;
[bicubic reconciliation](../benchmarks/WARM-CURRENT-BICUBIC.md) covers
{read('warm-current-bicubic-evidence.json')['confirmedNamedB12Cases']}/61 named cases, including reused unchanged paths.
Recent confirmations use `exclusive-vm-idle-v2`: live job checks and
five-second aggregate/CPU0 idle samples; historical load is diagnostic only.
Earlier receipts retain their original load guard and its failures.

*VP8L encode uses **libwebp exact=1 (FFmpeg encoder backend)** through its public
API. The public FFmpeg encoder changes hidden RGB and exposes no exact option.
Both subjects preserve every decoded RGBA sample, but compression differs:
Lokad.Image emits 533,100 bytes versus 183,812 bytes. This is an exact-pixel
timing comparison; compression efficiency parity is not established.

Single-component grayscale JPEG encoding uses **SIMD-enabled standalone
libjpeg-turbo3.1.4.1** as an approved tooling-only reference exception; FFmpeg's
MJPEG encoder emits three components. It confirms **1.388x / upper1.398x** at
**154,893 B/op**, including normalization and fresh output ownership/release.
Compressed sizes are **58,823 / 58,910 bytes**, managed/reference; the source-
quality gap is **0.000033dB**, within the existing0.25dB gate. Prepared fixtures,
worker and qualified production DLL are reused. The production library remains
safe managed code with no dependency or native binding.

Remaining current-production work is a finite set of named changed-path
confirmations and distinct unmeasured public boundaries. Historical confirmations
remain valid at their recorded sources; reuse requires unchanged relevant paths.
Warm identification-only coverage is missing because the existing inspection
benchmark includes startup and I/O. See [material coverage](../benchmarks/WARM-COVERAGE.md).

All **four existing bilinear cases** now have independent confirmation on the
current qualified core. Their conservative uppers range from 1.382x to 1.892x;
the tables retain exact ratios and allocations. Exact managed rational pixels,
native RGB bounds and the approved scoped alpha policies pass. No production
optimization, rebuild or repeated qualification was needed. Stop these cases;
the finite remaining coverage checklist stays open.

Current adaptive JPEG reconciliation: **4 affected 444/422 byte/word pairs**
independently confirm on the same qualified core, worst upper **1.639x**.
Exact frozen packets and source-quality gates pass; full normalization and fresh
output ownership are timed. [Current report](../benchmarks/WARM-CURRENT-JPEG-ADAPTIVE.md)
also records compressed sizes. No production optimization or rebuild is needed.

Current ordinary JPEG decode reconciliation: **8/8 named affected cases**
independently confirm on the qualified core, worst upper **1.626x**.
Exact frozen pixels, existing native agreement bounds and complete output
ownership pass. [Current report](../benchmarks/WARM-CURRENT-JPEG-DECODE.md)
states the fixed inputs and remaining cases. No production rebuild or retuning.

Current progressive JPEG reconciliation confirms the three named gray/420/422
pairs, worst upper **1.960x**. Exact frozen pixels and unchanged native bounds
pass. [Current report](../benchmarks/WARM-CURRENT-JPEG-PROGRESSIVE.md) includes
allocations and unchanged-build/qualification reuse. Stop these cases and the
already confirmed444/live-token444 pairs; remaining material coverage is open.

Current PNG decode reconciliation: **25/25 named affected cases** independently
confirm for current production paths, worst upper **1.494x**. Exact RGBA/alpha/hidden
RGB and complete output ownership pass. [Current report](../benchmarks/WARM-CURRENT-PNG-DECODE.md)
includes allocations and remaining cases. Shared three-case drivers preserve
sequential short/independent confirmation jobs without production rebuilds.

The demonstrated stored RGBA16 gap is resolved: **2.219x → 1.090x**, upper
**1.096x**; allocation falls **1,072,108 → 8,552B/op**. Bounded private IDAT
storage reuse halves decode time. Exact pixels, both quiet guards, all twelve
actuals and fresh Windows/Linux differential/same-package qualification pass.
Unchanged smaller-input confirmations remain valid; other named coverage is open.

The named Adam7 follow-up is closed: **RGBA16 1.377x / upper1.381x** and
**packed gray1 1.314x / upper1.320x** independently confirm on unchanged production
inputs. [Current evidence](../benchmarks/WARM-CURRENT-PNG-ADAM7.md) records exact
pixels, allocations, all twelve actuals and both guards. Later palette4/8 proofs
are reused; no production rebuild or repeated qualification.

The named PNG Stream follow-up is closed: **Png8 1.428x / upper1.441x**, **PngCropFit8 1.445x / upper1.454x**, **Png16 1.136x / upper1.146x**. Full input copying,
decode/conversion/composition and ownership boundaries remain timed.
[Current report](../benchmarks/WARM-CURRENT-PNG-STREAM.md) includes allocations;
word-composition proof and production/package qualification are reused.

**Decode**

{table(decodes)}

The named RGBA PNG encode follow-up is closed: **Encode16 0.745x / upper0.763x**, **Encode8 0.314x / upper0.316x**. Exact samples,
full packet ownership and the unchanged5% IDAT-size gate pass.
[Current report](../benchmarks/WARM-CURRENT-PNG-ENCODE.md) includes compressed
sizes and allocations. Production/harness/native builds and qualification repeat
counts are zero; stop these cases and retain other valid confirmed settings.

The named alpha-bearing VP8 follow-up is closed: **Encode16 0.576x / upper0.581x**, **Encode8 0.569x / upper0.573x**. The unchanged fixed quality pair,
exact alpha, full normalization/ownership and decoder bounds pass.
[Current report](../benchmarks/WARM-CURRENT-VP8-ENCODE.md) includes allocations and
471,436/98,590-byte packets; no compression-efficiency parity is claimed.
Prepared quality records/packets and production qualification are reused.

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

The latest accepted production round isolates adaptive JPEG AC event planning
from sampling/transforms. Row-restart encoding improves from **2.001x** to
independently confirmed **1.799x / upper1.809x**, **10.4% less managed time**,
with unchanged **297,780 B/op** and exact whole packets. The nonrestart
regression mean is1.631x. One six-second profile, one changed core build and
unchanged harness/native workers suffice. Twenty-one focused normal/scalar
checks per OS, **3,483 Release tests per OS** including **898 live comparisons**,
and the consumer/all three examples through the same actual Release package
on Windows/Linux pass. No image, selector or settings grid is added.

Three existing cases subsequently confirm on that same qualified immutable
build: adaptive nonrestart JPEG420 at **1.634x / upper1.641x**, **297,780 B/op**;
PNG16 None decode at **1.045x / upper1.059x**, **8,552 B/op**; and RGBA16 bicubic
width enlargement at **1.473x / upper1.499x**, **2,129,311 B/op**. No production
or harness rebuild, profile, new fixture or repeated qualification is needed.
All twelve actuals and both quiet guards are retained for each. Earlier
near-2x results and the bicubic pre-guard refusal remain in linked evidence.
The existing adaptive JPEG420 RGBA16 source pair subsequently confirms at
**1.682x / upper1.697x**, **297,780 B/op**, including complete full-word alpha
flattening/normalization. It reuses the same qualified core/harness/native worker
and frozen packet, with no new selector, fixture, profile or repeated proofs.

The preceding production round widens the local baseline JPEG AC bit
cache, reducing a current-build **2.040x** RGB gap by **16.8%**, independently
confirming **1.695x / upper 1.709x** at **2,664,366 B/op**. Four checked ordinary
bytes supply several short coefficients per refill. Logical byte/bit state,
partial coefficients, work/error ordering and the original fallback remain
exact; no heap workspace is added. The same qualified build independently
confirms progressive 4:4:4 at **1.877x / upper 1.896x**, **2,682,364 B/op**, and fresh
JPEG Stream at **1.632x / upper 1.673x**, **1,813,266 B/op**.
The Stream worker now performs source-preservation validation outside timing.
Both regression means precede full **3,483-test-per-OS** qualification, including
**898 live comparisons**, and the same actual Release package's consumer plus
all three examples on Windows/Linux. Additional confirmations reuse those
proofs. No input variants or repeated production/harness builds are added.

The preceding RGB420/RGB422 rows retain their independent 1.638x/upper1.646x and
1.511x/upper1.520x results with 70.6% and 62.9% less managed time respectively.
Those measurements precede the current shared entropy primitive.

The [single-pixel bicubic comparison](../benchmarks/WARM-BICUBIC-SINGLE-PIXEL.md)
adds one unchanged-production RGBA8 reduction, independently confirmed at
**0.213x / upper 0.227x**, **130 B/op**, against the labeled zimg/FFmpeg zscale
backend. Managed/normal/scalar native samples are exact. Required rectangular
halo/packing/filtering/quantization/ownership work is timed. Existing full
qualification is reused; no production rebuild or profile is needed.

The [packed palette4 Adam7 comparison](../benchmarks/WARM-PNG-PALETTE4-ADAM7.md)
independently confirms **1.304x / upper 1.387x**, **2,592 B/op**, using the existing
original sixteen-entry palette projection. Managed/normal/scalar native pixels,
alpha and hidden RGB are exact. Complete packed decode, palette expansion,
interlaced placement and output ownership are timed. Production qualification
is reused; no production rebuild, profile or repeated package proof is needed.

The [RGBA8 SmallestSize comparison](../benchmarks/WARM-PNG-SMALLEST8.md)
independently confirms **0.835x / upper 0.841x**, **527,564 B/op**, with numeric
levels9/9, exact RGBA and the unchanged5% IDAT-size gate (209,314/210,283 bytes).
Production and package qualification are reused without a production rebuild,
profile or repeated proofs. The measured binaries remain immutable; a word-only
receipt-prose correction after measurement needs no repeat.

The [live-token opaque VP8 comparison](../benchmarks/WARM-VP8-CANCELABLE.md)
independently confirms **1.838x / upper 1.843x**, **760,548 B/op**, with exact
qualified managed pixels, opaque alpha and unchanged decoder bounds. Existing
frozen input, pre-cancellation/token identity, normal/scalar CLI and independent
owners are checked; token setup/disposal is outside timing. One harness build,
no production/native rebuild/profile or repeated qualification is required.

The [VP8 simple-filter comparison](../benchmarks/WARM-VP8-SIMPLE-FILTER.md)
independently confirms **1.746x / upper 1.765x**, **760,548 B/op**. One original
packet on the existing opacity projection independently verifies coded/effective
simple filtering at level8. Managed pixels/alpha, normal/scalar CLI anchors and
unchanged native agreement bounds pass. FFmpeg's decoder remains the baseline;
libwebp only authors the untimed fixture. Existing production qualification is
reused without a production/native decoder rebuild, profile or repeated proof.

The [indexed VP8L comparison](../benchmarks/WARM-VP8L-INDEXED16.md) independently
confirms **1.519x / upper 1.550x**, **1,351,961 B/op**, with exact RGBA/hidden RGB
through managed and normal/scalar native decoders. One 11,810-byte type3/sixteen-
entry packet reuses the existing PNG palette projection and exact=1 author.
FFmpeg's decoder remains the baseline; all inverse expansion/packing/ownership
is timed. Existing production/native builds and qualification are reused, with
one harness build and no profile or repeated proofs.

Recent unchanged-production coverage confirms RGB16 Paeth at1.904x /
upper1.928x,6,504 B/op, and gray-alpha8 Paeth at1.017x / upper1.068x,2,408 B/op.
Those rounds reuse the preceding immutable production qualification.

The palette8/tRNS Adam7 pair now independently confirms **1.167x / upper1.177x**,
with **4,960 B/op**, exact alpha/hidden RGB and both quiet guards passing.
Optional native process-CPU telemetry accompanies unchanged wall timing; CPU/wall
ratios0.99727–0.99915 show no substantial descheduling in the new observations.
The historical sixfold native slowdown remains unexplained and unscored.
One worker/harness build suffices; current production qualification is reused.

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
