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
        assert quiet
        assert evidence.get('independentQuietConfirmationThisPair') or observation.get('certifiedThisPair')
    if confirmed:
        status = 'Parity confirmed' if upper <= 1 else ('Confidence open' if ratio <= 1 else 'Gap (independent)')
    else:
        status = 'Quiet certification open' if quiet is None else ('Quiet repeat open' if quiet else 'Quiet guard failed / open')
    if verification:
        assert not confirmed and quiet
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
        row('PNG Identify, fresh Stream¶', 'warm-png-stream-identify-evidence.json', ['observations', -1], 'WARM-PNG-STREAM-IDENTIFY.md', True),
        row('WebP Identify, resident span (parser)', 'warm-webp-identify-evidence.json', ['observations', -1], 'WARM-WEBP-IDENTIFY.md', True),
        row('JPEG Identify, resident span‖', 'warm-jpeg-entropy-evidence.json', ['observations', -1], 'WARM-JPEG-ENTROPY.md', True),
        row('PNG Identify, resident span¶', 'warm-png-identify-evidence.json', ['observations', -1], 'WARM-PNG-IDENTIFY.md', True),
        row('PNG palette4/full tRNS Adam7', 'warm-png-palette4-adam7-evidence.json', ['observations', -1], 'WARM-PNG-PALETTE4-ADAM7.md', True),
        row('PNG8 Stream, orient/crop/nearest fit', 'warm-current-png-stream-evidence.json', ['cases', 'PngCropFit8', -1], 'WARM-CURRENT-PNG-STREAM.md', True),
        row('PNG16 Stream, orient/crop/nearest fit', 'warm-png-fused-evidence.json', ['observations', -1], 'WARM-PNG-FUSED-INFLATION.md', True),
        row('PNG RGBA8, fresh seekable Stream', 'warm-current-png-stream-evidence.json', ['cases', 'Png8', -1], 'WARM-CURRENT-PNG-STREAM.md', True),
        row('PNG RGBA16, fresh seekable Stream', 'warm-current-png-stream-evidence.json', ['cases', 'Png16', -1], 'WARM-CURRENT-PNG-STREAM.md', True),
        row('JPEG420 restart, fresh seekable Stream', 'warm-jpeg-ac-cache-evidence.json', ['observations', 8], 'WARM-JPEG-AC-CACHE.md', True),
        row('WebP VP8 + alpha, fresh seekable Stream', 'warm-vp8l-borrowed-green-evidence.json', ['cases', 'StreamNative8', -1], 'WARM-VP8L-BORROWED-GREEN.md', verification='Gap (short; qualified)'),
        row('WebP VP8L, fresh seekable Stream', 'warm-vp8l-borrowed-green-evidence.json', ['cases', 'StreamNativeLossless8', -1], 'WARM-VP8L-BORROWED-GREEN.md', verification='Gap (short; qualified)'),
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
        row('PNG RGBA8, Paeth', 'warm-paeth-six-channel-evidence.json', ['cases', 'DecodePaeth8', -1], 'WARM-PAETH-SIX-CHANNEL.md', verification='Gap (short; qualified)'),
        row('PNG RGBA8, Up', 'warm-png-up-simd-evidence.json', ['cases', 'DecodeUp8', -1], 'WARM-PNG-UP-SIMD.md', True),
        row('PNG RGBA16, Up', 'warm-png-up-simd-evidence.json', ['cases', 'DecodeUp16', -1], 'WARM-PNG-UP-SIMD.md', True),
        row('PNG RGBA8, Sub', 'warm-png-sub8-current-evidence.json', ['observations', -1], 'WARM-PNG-SUB8-CURRENT.md', True),
        row('PNG RGBA16, Sub', 'warm-pngsub16-evidence.json', ['observations', -1], 'WARM-PNG-SUB16.md', True),
        row('PNG RGBA8, Average', 'warm-current-png-decode-evidence.json', ['cases', 'DecodeAverage8', -1], 'WARM-CURRENT-PNG-DECODE.md', True),
        row('PNG RGBA16, Average', 'warm-pngaverage16-evidence.json', ['observations', -1], 'WARM-PNG-AVERAGE16.md', True),
        row('PNG RGBA16, Paeth', 'warm-current-png-decode-evidence.json', ['cases', 'DecodePaeth16', -1], 'WARM-CURRENT-PNG-DECODE.md', True),
        row('PNG RGB8, Paeth', 'warm-paeth-six-channel-evidence.json', ['cases', 'DecodeRgbPaeth8', -1], 'WARM-PAETH-SIX-CHANNEL.md', verification='Gap (short; qualified)'),
        row('PNG RGB16, Paeth', 'warm-paeth-six-channel-evidence.json', ['cases', 'DecodeRgbPaeth16', -1], 'WARM-PAETH-SIX-CHANNEL.md', True),
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
        row('JPEG progressive gray', 'warm-jpeg-refinement-fused-evidence.json', ['cases', 'ProgressiveGray', -1], 'WARM-JPEG-REFINEMENT-FUSED.md', True),
        row('JPEG progressive 4:2:0', 'warm-jpeg-refinement-fused-evidence.json', ['cases', 'Progressive420', -1], 'WARM-JPEG-REFINEMENT-FUSED.md', True),
        row('JPEG progressive 4:4:4 / live cancelable token', 'warm-jpeg-cancelable-refinement-evidence.json', ['cases', 'ProgressiveCancelable444', -1], 'WARM-JPEG-CANCELABLE-REFINEMENT.md', verification='Gap (short; qualified)'),
        row('JPEG progressive 4:2:2', 'warm-jpeg-refinement-fused-evidence.json', ['cases', 'Progressive422', -1], 'WARM-JPEG-REFINEMENT-FUSED.md', True),
        row('JPEG progressive 4:4:4', 'warm-jpeg-refinement-fused-evidence.json', ['cases', 'Progressive444', -1], 'WARM-JPEG-REFINEMENT-FUSED.md', True),
        row('WebP VP8 + alpha', 'warm-vp8l-borrowed-green-evidence.json', ['cases', 'Native8', -1], 'WARM-VP8L-BORROWED-GREEN.md', verification='Gap (short; qualified)'),
        row('WebP VP8 opaque, simple luma filter', 'warm-vp8-simple-filter-evidence.json', ['observations', -1], 'WARM-VP8-SIMPLE-FILTER.md', True),
        row('WebP VP8 opaque, live cancelable token', 'warm-vp8-cancelable-evidence.json', ['observations', -1], 'WARM-VP8-CANCELABLE.md', True),
        row('WebP VP8, opaque', 'warm-vp8-filter8-evidence.json', ['observations', -1], 'WARM-VP8-FILTER8.md', True),
        row('WebP VP8L, sixteen-color indexed input', 'warm-vp8l-borrowed-green-evidence.json', ['cases', 'IndexedNative16Colors8', -1], 'WARM-VP8L-BORROWED-GREEN.md', verification='Gap (short; qualified)'),
        row('WebP VP8L, literal/run input', 'warm-vp8l-borrowed-green-evidence.json', ['cases', 'Lossless8', -1], 'WARM-VP8L-BORROWED-GREEN.md', verification='Gap (short; qualified)'),
        row('WebP VP8L, native input', 'warm-vp8l-borrowed-green-evidence.json', ['cases', 'NativeLossless8', -1], 'WARM-VP8L-BORROWED-GREEN.md', verification='Gap (short; qualified)'),
        row('WebP VP8L, native input / live cancelable token', 'warm-vp8l-borrowed-green-evidence.json', ['cases', 'NativeLosslessCancelable8', -1], 'WARM-VP8L-BORROWED-GREEN.md', verification='Gap (short; qualified)'),
    ]
    encodes = [
        row('PNG RGBA8, level 6 / None', 'warm-png-chunk-crc-evidence.json', ['cases', 'Encode8', -1], 'WARM-PNG-CHUNK-CRC.md', True),
        row('PNG RGBA8, NoCompression / None', 'warm-png-chunk-crc-evidence.json', ['cases', 'EncodeNoCompression8', -1], 'WARM-PNG-CHUNK-CRC.md', verification='Gap (short; qualified)'),
        row('PNG RGBA16, NoCompression / None', 'warm-png-chunk-crc-evidence.json', ['cases', 'EncodeNoCompression16', -1], 'WARM-PNG-CHUNK-CRC.md', verification='Gap (short; qualified)'),
        row('PNG RGBA8, Fastest managed2/native1 / None', 'warm-png-chunk-crc-evidence.json', ['cases', 'EncodeFast8', -1], 'WARM-PNG-CHUNK-CRC.md', True),
        row('PNG RGBA16, Fastest managed2/native1 / None', 'warm-png-chunk-crc-evidence.json', ['cases', 'EncodeFast16', -1], 'WARM-PNG-CHUNK-CRC.md', True),
        row('PNG RGBA16, Fastest managed2/native1 / live cancelable token', 'warm-png-chunk-crc-evidence.json', ['cases', 'EncodeFastCancelable16', -1], 'WARM-PNG-CHUNK-CRC.md', True),
        row('PNG RGBA16, Fastest managed2/native1 / forward-only output', 'warm-png-chunk-crc-evidence.json', ['cases', 'EncodeFastForward16', -1], 'WARM-PNG-CHUNK-CRC.md', True),
        row('PNG RGBA16, level 6 / None', 'warm-png-chunk-crc-evidence.json', ['cases', 'Encode16', -1], 'WARM-PNG-CHUNK-CRC.md', True),
        row('PNG RGBA8, SmallestSize9 / native9', 'warm-png-chunk-crc-evidence.json', ['cases', 'EncodeSmallest8', -1], 'WARM-PNG-CHUNK-CRC.md', True),
        row('PNG RGBA16, SmallestSize managed9/native9 / None', 'warm-png-chunk-crc-evidence.json', ['cases', 'EncodeSmallest16', -1], 'WARM-PNG-CHUNK-CRC.md', True),
        row('PNG RGB8, Optimal managed9/native6 / None', 'warm-png-byte-packing-evidence.json', ['cases', 'EncodeRgb8', -1], 'WARM-PNG-BYTE-PACKING.md', True),
        row('PNG RGB16, level 6 / None', 'warm-png-chunk-crc-evidence.json', ['cases', 'EncodeRgb16', -1], 'WARM-PNG-CHUNK-CRC.md', True),
        row('PNG gray8, Optimal managed9/native6 / None', 'warm-png-byte-packing-evidence.json', ['cases', 'EncodeGray8', -1], 'WARM-PNG-BYTE-PACKING.md', True),
        row('PNG gray16, level 6 / None', 'warm-png-chunk-crc-evidence.json', ['cases', 'EncodeGray16', -1], 'WARM-PNG-CHUNK-CRC.md', verification='Gap (short; qualified)'),
        row('PNG gray-alpha8, Optimal managed9/native6 / None', 'warm-png-byte-packing-evidence.json', ['cases', 'EncodeGrayAlpha8', -1], 'WARM-PNG-BYTE-PACKING.md', True),
        row('PNG gray-alpha16, level 6 / None', 'warm-png-chunk-crc-evidence.json', ['cases', 'EncodeGrayAlpha16', -1], 'WARM-PNG-CHUNK-CRC.md', True),
        row('JPEG 4:4:4, adaptive Huffman / matched quality', 'warm-jpeg-adaptive-ac-evidence.json', ['cases', 'Encode8', -1], 'WARM-JPEG-ADAPTIVE-AC.md', verification='Gap (short; qualified)'),
        row('JPEG 4:4:4, forward-only output / geometric AC Huffman', 'warm-jpeg-fixed-ac-fusion-evidence.json', ['cases', 'Encode444Stream8', -1], 'WARM-JPEG-FIXED-AC-FUSION.md', verification='Gap (short; qualified)'),
        row('JPEG 4:4:4, live cancelable token / geometric AC Huffman', 'warm-jpeg-fixed-ac-fusion-evidence.json', ['cases', 'Encode444Cancelable8', -1], 'WARM-JPEG-FIXED-AC-FUSION.md', verification='Gap (short; qualified)'),
        row('JPEG 4:4:4 from RGBA16, adaptive Huffman / full normalization', 'warm-jpeg-adaptive-ac-evidence.json', ['cases', 'Encode16', -1], 'WARM-JPEG-ADAPTIVE-AC.md', verification='Gap (short; qualified)'),
        row('JPEG 4:2:2, adaptive Huffman / matched quality', 'warm-jpeg-adaptive-ac-evidence.json', ['cases', 'Encode422_8', -1], 'WARM-JPEG-ADAPTIVE-AC.md', verification='Gap (short; qualified)'),
        row('JPEG 4:2:2 from RGBA16, adaptive Huffman / full normalization', 'warm-jpeg-adaptive-ac-evidence.json', ['cases', 'Encode422_16', -1], 'WARM-JPEG-ADAPTIVE-AC.md', verification='Gap (short; qualified)'),
        row('JPEG 4:2:0, adaptive Huffman / matched quality', 'warm-jpeg-adaptive-ac-evidence.json', ['cases', 'Encode420_8', -1], 'WARM-JPEG-ADAPTIVE-AC.md', verification='Gap (short; qualified)'),
        row('JPEG 4:2:0, adaptive / row restart markers', 'warm-jpeg-adaptive-ac-evidence.json', ['cases', 'Encode420Restart8', -1], 'WARM-JPEG-ADAPTIVE-AC.md', verification='Gap (short; qualified)'),
        row('JPEG 4:2:0, forward-only output / geometric AC Huffman', 'warm-jpeg-fixed-ac-fusion-evidence.json', ['cases', 'Encode420Stream8', -1], 'WARM-JPEG-FIXED-AC-FUSION.md', verification='Gap (short; qualified)'),
        row('JPEG 4:2:0 from RGBA16, forward-only output', 'warm-jpeg-fixed-ac-fusion-evidence.json', ['cases', 'Encode420Stream16', -1], 'WARM-JPEG-FIXED-AC-FUSION.md', verification='Gap (short; qualified)'),
        row('JPEG 4:2:0 from RGBA16, live cancelable token', 'warm-jpeg-fixed-ac-fusion-evidence.json', ['cases', 'Encode420Cancelable16', -1], 'WARM-JPEG-FIXED-AC-FUSION.md', verification='Gap (short; qualified)'),
        row('JPEG 4:2:0 from RGBA16, adaptive Huffman', 'warm-jpeg-adaptive-ac-evidence.json', ['cases', 'Encode420_16', -1], 'WARM-JPEG-ADAPTIVE-AC.md', verification='Gap (short; qualified)'),
        row('JPEG grayscale, standalone SIMD libjpeg-turbo', 'warm-jpeg-adaptive-ac-evidence.json', ['cases', 'EncodeGray8', -1], 'WARM-JPEG-ADAPTIVE-AC.md', verification='Gap (short; qualified)'),
        row('WebP VP8, opaque‡', 'warm-vp8-source-views-evidence.json', ['cases', 'EncodeOpaque8', -1], 'WARM-VP8-SOURCE-VIEWS.md', verification='Gap (short; qualified)'),
        row('WebP VP8, opaque from RGBA16‡', 'warm-vp8-source-views-evidence.json', ['cases', 'EncodeOpaque16', -1], 'WARM-VP8-SOURCE-VIEWS.md', verification='Gap (short; qualified)'),
        row('WebP VP8 + alpha‡', 'warm-vp8-source-views-evidence.json', ['cases', 'Encode8', -1], 'WARM-VP8-SOURCE-VIEWS.md', True),
        row('WebP VP8 + alpha from RGBA16‡', 'warm-vp8-source-views-evidence.json', ['cases', 'Encode16', -1], 'WARM-VP8-SOURCE-VIEWS.md', True),
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
        row('Bilinear resize RGBA16†', 'warm-bilinear-word-cache-evidence.json', ['observations', -1], 'WARM-BILINEAR-WORD-CACHE.md', verification='Gap (short; qualified)'),
        row('Bilinear upscale RGBA8†', 'warm-bilinear-byte-factorization-evidence.json', ['cases', 'Up8', -1], 'WARM-BILINEAR-BYTE-FACTORIZATION.md', verification='Gap (short; qualified)'),
        row('Bilinear upscale RGBA16†', 'warm-current-bilinear-evidence.json', ['cases', 'Up16', -1], 'WARM-CURRENT-BILINEAR.md', True),
        row('RGBA8 bicubic, single-pixel reduction', 'warm-bicubic-single-pixel-evidence.json', ['observations', -1], 'WARM-BICUBIC-SINGLE-PIXEL.md', True),
        row('Bicubic resize RGBA8§', 'warm-current-bicubic-evidence.json', ['cases', 'Down8', 'observations', -1], 'WARM-CURRENT-BICUBIC.md', True),
        row('Bicubic resize RGBA16§', 'warm-current-bicubic-evidence.json', ['cases', 'Down16', 'observations', -1], 'WARM-CURRENT-BICUBIC.md', True),
        row('Bicubic upscale RGBA8§', 'warm-current-bicubic-evidence.json', ['cases', 'Up8', 'observations', -1], 'WARM-CURRENT-BICUBIC.md', True),
        row('Bicubic upscale RGBA16§', 'warm-current-bicubic-evidence.json', ['cases', 'Up16', 'observations', -1], 'WARM-CURRENT-BICUBIC.md', True),
        row('Bicubic mixed-axis RGBA8§', 'warm-bicubic-byte-paired-evidence.json', ['cases', 'Mixed8', -1], 'WARM-BICUBIC-BYTE-PAIRED.md', verification='Gap (short; qualified)'),
        row('Bicubic mixed-axis RGBA16§', 'warm-bicubic-paired-columns-evidence.json', ['cases', 'Mixed16', -1], 'WARM-BICUBIC-PAIRED-COLUMNS.md', verification='Gap (short; qualified)'),
        row('Bicubic width reduction / height enlargement RGBA8§', 'warm-current-bicubic-evidence.json', ['cases', 'Cross8', 'observations', -1], 'WARM-CURRENT-BICUBIC.md', True),
        row('Bicubic width reduction / height enlargement RGBA16§', 'warm-current-bicubic-evidence.json', ['cases', 'Cross16', 'observations', -1], 'WARM-CURRENT-BICUBIC.md', True),
        row('Bicubic RGBA8, 128×64 / wider support§', 'warm-current-bicubic-evidence.json', ['cases', 'Wide8', 'observations', -1], 'WARM-CURRENT-BICUBIC.md', True),
        row('Bicubic RGBA16, 128×64 / wider support§', 'warm-current-bicubic-evidence.json', ['cases', 'Wide16', 'observations', -1], 'WARM-CURRENT-BICUBIC.md', True),
        row('Bicubic width reduction, unchanged height RGBA8§', 'warm-current-bicubic-evidence.json', ['cases', 'Horizontal8', 'observations', -1], 'WARM-CURRENT-BICUBIC.md', True),
        row('Bicubic width reduction, unchanged height RGBA16§', 'warm-current-bicubic-evidence.json', ['cases', 'Horizontal16', 'observations', -1], 'WARM-CURRENT-BICUBIC.md', True),
        row('Bicubic width enlargement, unchanged height RGBA8§', 'warm-current-bicubic-evidence.json', ['cases', 'HorizontalUp8', 'observations', -1], 'WARM-CURRENT-BICUBIC.md', True),
        row('Bicubic width enlargement, unchanged height RGBA16§', 'warm-bicubic-enlargement-strips-evidence.json', ['cases', 'HorizontalUp16', -1], 'WARM-BICUBIC-ENLARGEMENT-STRIPS.md', verification='Gap (short; qualified)'),
        row('Bicubic RGBA8, 64×32 / support beyond 32§', 'warm-current-bicubic-evidence.json', ['cases', 'Down64x32_8', 'observations', -1], 'WARM-CURRENT-BICUBIC.md', True),
        row('Bicubic RGBA16, 64×32 / support beyond 32§', 'warm-current-bicubic-evidence.json', ['cases', 'Down64x32_16', 'observations', -1], 'WARM-CURRENT-BICUBIC.md', True),
        row('Bicubic RGBA8, 32×16 / support beyond 64§', 'warm-current-bicubic-evidence.json', ['cases', 'Down32x16_8', 'observations', -1], 'WARM-CURRENT-BICUBIC.md', True),
        row('Bicubic RGBA16, 32×16 / support beyond 64§', 'warm-current-bicubic-evidence.json', ['cases', 'Down32x16_16', 'observations', -1], 'WARM-CURRENT-BICUBIC.md', True),
        row('Bicubic RGBA8, 16×8 / adaptive support beyond 128§', 'warm-current-bicubic-evidence.json', ['cases', 'Down16x8_8', 'observations', -1], 'WARM-CURRENT-BICUBIC.md', True),
        row('Bicubic RGBA16, 16×8 / adaptive support beyond 128§', 'warm-current-bicubic-evidence.json', ['cases', 'Down16x8_16', 'observations', -1], 'WARM-CURRENT-BICUBIC.md', True),
        row('Bicubic RGBA8, 256×129 / input+output+4 KiB limit§', 'warm-bicubic-byte-strips-evidence.json', ['observations', -1], 'WARM-BICUBIC-BYTE-STRIPS.md', verification='Gap (short; qualified)'),
        row('Bicubic RGBA16, 256×129 / input+output+4 KiB limit§', 'warm-current-bicubic-evidence.json', ['cases', 'TightDown16', 'observations', -1], 'WARM-CURRENT-BICUBIC.md', True),
        row('Bicubic RGBA8, 128×64 / input+output+4 KiB limit§', 'warm-current-bicubic-evidence.json', ['cases', 'TightWide8', 'observations', -1], 'WARM-CURRENT-BICUBIC.md', True),
        row('Bicubic RGBA16, 128×64 / input+output+4 KiB limit§', 'warm-current-bicubic-evidence.json', ['cases', 'TightWide16', 'observations', -1], 'WARM-CURRENT-BICUBIC.md', True),
        row('Bicubic RGBA8, 64×32 / input+output+4 KiB limit§', 'warm-current-bicubic-evidence.json', ['cases', 'Tight64x32_8', 'observations', -1], 'WARM-CURRENT-BICUBIC.md', True),
        row('Bicubic RGBA16, 64×32 / input+output+4 KiB limit§', 'warm-current-bicubic-evidence.json', ['cases', 'Tight64x32_16', 'observations', -1], 'WARM-CURRENT-BICUBIC.md', True),
        row('Bicubic RGBA16, 32×16 / input+output+4 KiB limit§', 'warm-current-bicubic-evidence.json', ['cases', 'Tight32x16_16', 'observations', -1], 'WARM-CURRENT-BICUBIC.md', True),
        row('Bicubic RGBA8, 32×16 / input+output+4 KiB limit§', 'warm-current-bicubic-evidence.json', ['cases', 'Tight32x16_8', 'observations', -1], 'WARM-CURRENT-BICUBIC.md', True),
        row('Bicubic RGBA16, 16×8 / input+output+4 KiB limit§', 'warm-current-bicubic-evidence.json', ['cases', 'Tight16x8_16', 'observations', -1], 'WARM-CURRENT-BICUBIC.md', True),
        row('Bicubic RGBA8, 16×8 / input+output+4 KiB limit§', 'warm-current-bicubic-evidence.json', ['cases', 'Tight16x8_8', 'observations', -1], 'WARM-CURRENT-BICUBIC.md', True),
        row('Bicubic RGBA16 enlargement / input+output+4 KiB limit§', 'warm-bicubic-enlargement-strips-evidence.json', ['cases', 'TightUp16', -1], 'WARM-BICUBIC-ENLARGEMENT-STRIPS.md', verification='Gap (short; qualified)'),
        row('Bicubic RGBA8 enlargement / input+output+4 KiB limit§', 'warm-bicubic-enlargement-strips-evidence.json', ['cases', 'TightUp8', -1], 'WARM-BICUBIC-ENLARGEMENT-STRIPS.md', verification='Gap (short; qualified)'),
        row('Bicubic RGBA16, width up / height down / input+output+4 KiB limit§', 'warm-bicubic-paired-columns-evidence.json', ['cases', 'TightMixed16', -1], 'WARM-BICUBIC-PAIRED-COLUMNS.md', verification='Gap (short; qualified)'),
        row('Bicubic RGBA8, width up / height down / input+output+4 KiB limit§', 'warm-bicubic-byte-paired-evidence.json', ['cases', 'TightMixed8', -1], 'WARM-BICUBIC-BYTE-PAIRED.md', verification='Gap (short; qualified)'),
        row('Bicubic RGBA16, width down / height up / input+output+4 KiB limit§', 'warm-current-bicubic-evidence.json', ['cases', 'TightCross16', 'observations', -1], 'WARM-CURRENT-BICUBIC.md', True),
        row('Bicubic RGBA8, width down / height up / input+output+4 KiB limit§', 'warm-current-bicubic-evidence.json', ['cases', 'TightCross8', 'observations', -1], 'WARM-CURRENT-BICUBIC.md', True),
        row('Bicubic RGBA16, 8×4 / support beyond 256 / input+output+4 KiB limit§', 'warm-current-bicubic-evidence.json', ['cases', 'Tight8x4_16', 'observations', -1], 'WARM-CURRENT-BICUBIC.md', True),
        row('Bicubic RGBA8, 8×4 / support beyond 256 / input+output+4 KiB limit§', 'warm-current-bicubic-evidence.json', ['cases', 'Tight8x4_8', 'observations', -1], 'WARM-CURRENT-BICUBIC.md', True),
        row('Bicubic RGBA16, 4×2 / packed support beyond 304 / input+output+4 KiB limit§', 'warm-current-bicubic-evidence.json', ['cases', 'Tight4x2_16', 'observations', -1], 'WARM-CURRENT-BICUBIC.md', True),
        row('Bicubic RGBA8, 4×2 / packed support beyond 304 / input+output+4 KiB limit§', 'warm-current-bicubic-evidence.json', ['cases', 'Tight4x2_8', 'observations', -1], 'WARM-CURRENT-BICUBIC.md', True),
        row('Bicubic RGBA16, 2×2 / generated support beyond 544 / input+output+4 KiB limit§', 'warm-current-bicubic-evidence.json', ['cases', 'Tight2x2_16', 'observations', -1], 'WARM-CURRENT-BICUBIC.md', True),
        row('Bicubic RGBA16, 2×9 / bounded row blocks / input+output+4 KiB limit§', 'warm-current-bicubic-evidence.json', ['cases', 'Tight2x9_16', 'observations', -1], 'WARM-CURRENT-BICUBIC.md', True),
        row('Bicubic RGBA16, 4×9 / packed row blocks / input+output+4 KiB limit§', 'warm-current-bicubic-evidence.json', ['cases', 'Tight4x9_16', 'observations', -1], 'WARM-CURRENT-BICUBIC.md', True),
        row('Bicubic RGBA16, 8×17 / full-coefficient row blocks / input+output+4 KiB limit§', 'warm-bicubictight8x17word-generated-evidence.json', ['observations', -1], 'WARM-BICUBIC-TIGHT-8X17-RGBA16.md', True),
        row('Bicubic RGBA8, 8×17 / full-coefficient row blocks / input+output+4 KiB limit§', 'warm-bicubictight8x17byte-generated-evidence.json', ['observations', -1], 'WARM-BICUBIC-TIGHT-8X17-RGBA8.md', True),
        row('Bicubic RGBA8, 4×9 / packed row blocks / input+output+4 KiB limit§', 'warm-current-bicubic-evidence.json', ['cases', 'Tight4x9_8', 'observations', -1], 'WARM-CURRENT-BICUBIC.md', True),
        row('Bicubic RGBA8, 2×9 / bounded row blocks / input+output+4 KiB limit§', 'warm-current-bicubic-evidence.json', ['cases', 'Tight2x9_8', 'observations', -1], 'WARM-CURRENT-BICUBIC.md', True),
        row('Bicubic RGBA8, 2×2 / generated support beyond 544 / input+output+4 KiB limit§', 'warm-current-bicubic-evidence.json', ['cases', 'Tight2x2_8', 'observations', -1], 'WARM-CURRENT-BICUBIC.md', True),
        row('Bicubic height reduction, unchanged width RGBA8§', 'warm-bicubictightvertical8-cachepref-evidence.json', ['defaultMemoryConfirmation'], 'WARM-BICUBIC-TIGHT-VERTICAL8.md', True),
        row('Bicubic height reduction, unchanged width RGBA8 / input+output+4 KiB limit§', 'warm-bicubic-byte-paired-evidence.json', ['cases', 'TightVertical8', -1], 'WARM-BICUBIC-BYTE-PAIRED.md', verification='Gap (short; qualified)'),
        row('Bicubic height reduction, unchanged width RGBA16§', 'warm-bicubictightvertical16-cachepref-evidence.json', ['defaultMemoryConfirmation'], 'WARM-BICUBIC-TIGHT-VERTICAL16.md', True),
        row('Bicubic height reduction, unchanged width RGBA16 / input+output+4 KiB limit§', 'warm-bicubic-paired-columns-evidence.json', ['cases', 'TightVertical16', -1], 'WARM-BICUBIC-PAIRED-COLUMNS.md', verification='Gap (short; qualified)'),
        row('Bicubic width reduction, unchanged height RGBA16 / input+output+4 KiB limit§', 'warm-current-bicubic-evidence.json', ['cases', 'TightHorizontal16', 'observations', -1], 'WARM-CURRENT-BICUBIC.md', True),
        row('Bicubic width reduction, unchanged height RGBA8 / input+output+4 KiB limit§', 'warm-bicubictighthorizontal8-cross-evidence.json', ['observations', -1], 'WARM-BICUBIC-TIGHT-HORIZONTAL8.md', True),
        row('Bicubic height enlargement, unchanged width RGBA8§', 'warm-current-bicubic-evidence.json', ['cases', 'VerticalUp8', 'observations', -1], 'WARM-CURRENT-BICUBIC.md', True),
        row('Bicubic height enlargement, unchanged width RGBA8 / input+output+4 KiB limit§', 'warm-bicubic-enlargement-strips-evidence.json', ['cases', 'TightVerticalUp8', -1], 'WARM-BICUBIC-ENLARGEMENT-STRIPS.md', verification='Gap (short; qualified)'),
        row('Bicubic height enlargement, unchanged width RGBA16§', 'warm-current-bicubic-evidence.json', ['cases', 'VerticalUp16', 'observations', -1], 'WARM-CURRENT-BICUBIC.md', True),
        row('Bicubic height enlargement, unchanged width RGBA16 / input+output+4 KiB limit§', 'warm-bicubic-enlargement-strips-evidence.json', ['cases', 'TightVerticalUp16', -1], 'WARM-BICUBIC-ENLARGEMENT-STRIPS.md', verification='Gap (short; qualified)'),
        row('Bicubic width enlargement, unchanged height RGBA16 / input+output+4 KiB limit§', 'warm-bicubic-enlargement-strips-evidence.json', ['cases', 'TightHorizontalUp16', -1], 'WARM-BICUBIC-ENLARGEMENT-STRIPS.md', verification='Gap (short; qualified)'),
        row('Bicubic width enlargement, unchanged height RGBA8 / input+output+4 KiB limit§', 'warm-bicubic-enlargement-strips-evidence.json', ['cases', 'TightHorizontalUp8', -1], 'WARM-BICUBIC-ENLARGEMENT-STRIPS.md', verification='Gap (short; qualified)'),
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
    all_rows = decodes + encodes + edits
    parity_count = sum(' | Parity confirmed |' in value for value in all_rows)
    confidence_count = sum(' | Confidence open |' in value for value in all_rows)
    gap_count = sum(' | Gap' in value for value in all_rows)
    return f'''# Lokad.Image benchmark state

Updated 2026-10-18. The current goal is **warm single-CPU parity: independent
conservative upper ≤1x**. The 207 numeric rows contain **{parity_count} confirmed parity
cases, {gap_count} measured gaps and {confidence_count} confidence-open cases**. The earlier
≤2x phase was completed for 0.1.0; subsequent qualified improvements distinguish
short screens from independent confirmations. The finite [material checklist](../benchmarks/WARM-COVERAGE.md) and
fixed inputs remain; results do not extrapolate to other images, settings,
sizes or hardware. Stop cases already confirmed at ≤1x.

Dedicated AMD EPYC9V45 VM; Ubuntu24.04.4; .NET10.0.8/SDK10.0.204;
FFmpeg n8.1 (`9047fa1b084f`). One pinned CPU, one codec/conversion/filter thread,
normal SIMD, tiering off, nonconcurrent workstation GC. Inputs are the two original
513×259 RGBA8/16 rasters and documented projections; exact case dimensions and
policies are in linked reports. No image or settings grid was added.

Times include complete public operations, admission, conversion, fresh independent
output ownership and disposal. Native contexts may be reused; managed private
storage may be reused after release. No decoded output or descriptor is cached.
Fresh Stream rows include source-wrapper/input acquisition, reads/copies/EOF and
release. Startup, I/O, IPC, preparation/hashing and validation are excluded.

Ratio = Lokad.Image / faster native pre/post mean; lower is faster. Upper retains
the larger selected/all-actual confidence bound against the faster native bracket's
lower bound. Parity confirmed requires an independent quiet repeat with upper≤1;
a mean≤1 with upper>1 stays confidence-open. Gap rows exceed the new target.
Independent jobs retain all 12 actuals; explicitly labeled short screens retain
all three. Both retain 5s `exclusive-vm-idle-v2` guards. Earlier records
retain their original protocol and failures. **B/op** is managed allocation from
BenchmarkDotNet/raw GC bytes per operation, rounded up. Native allocations are
**unmeasured**, not zero. Timings were collected across rounds; precise unchanged-
path reconciliation makes the retained evidence applicable to current production.

Latest production round: [exact byte bilinear factorization](../benchmarks/WARM-BILINEAR-BYTE-FACTORIZATION.md)
reduces RGBA8 enlargement time 16.8%, preserving exact sums, pixels and allocation.
Its 1.456× mean remains a qualified short gap; no routine long confirmation was
run. All 206 other comparisons and 58 passing proofs are reused. Previous
[bicubic strips](../benchmarks/WARM-BICUBIC-ENLARGEMENT-STRIPS.md) retain seven
qualified short improvements. Passing cases are stopped.
The 207 rows select 163 independent confirmations and 44 qualified short screens.
Fresh **3,659 Release cases per OS** (1,349 internal +1,412 public +898 live FFmpeg),
plus 132 scalar bilinear/reference controls, pass. The consumer and all
three examples pass through
the **same actual Release package on Windows/Linux**. Production remains safe
managed dependency-free C#/.NET10. Unaffected scalar proofs are reused.

**Decode and metadata**

{table(decodes)}

**Encode**

{table(encodes)}

**Editing**

{table(edits)}

* VP8L encode uses **libwebp exact=1, FFmpeg's encoder backend**, through its public
API: the FFmpeg encoder does not expose hidden-RGB preservation. Decoded RGBA is
exact; managed/native packets are533,100/183,812 B. Compression-efficiency parity
is not established. Single-component grayscale JPEG uses approved benchmark-only
**SIMD libjpeg-turbo3.1.4.1**, since FFmpeg emits three components; its current qualified short is shown above,
154,893 B/op,58,823/58,910 B compressed; source-PSNR gap0.000033dB. Complete
normalization and output ownership are timed.

‡ VP8 encode freezes managed/native quality100/75, effort4, and passes the
unchanged0.25dB source-quality gate with exact alpha. Managed/native packets are
333,580/72,380 B for opaque inputs and471,436/98,590 B with alpha; compression-
efficiency parity is not established. JPEG lossy comparisons preserve their
frozen source-quality-matched settings and0.25dB band; sizes remain in linked reports.

¶ PNG Identify references use the public FFmpeg PNG parser/CRC plus original
complete structural/EXIF validation and owned descriptors, without inflation;
the Stream reference also times fresh AVIO/scratch/input/EOF and release.
‖ JPEG Identify uses FFmpeg MJPEG discard-all, original complete structural/EXIF
validation and a timed17-byte canonical SOF2 metadata projection, fresh packet/
descriptor ownership and reset, with zero pixel-buffer requests. WebP Identify
uses the public FFmpeg WebP parser plus original complete RIFF/EXIF validation,
including mandatory parser buffering/EOF flush and descriptor ownership. Its
0.0040x/upper0.0045x is specific to this metadata boundary and parser copy cost.
All six managed metadata fields remain exact.

† RGBA16 bilinear and explicit RGBA8 upscale allow native-only alpha max1/RMSE≤0.25;
Up8 times zero-alpha RGB clearing. Managed pixels remain exact; Down8 alpha is exact.
§ Bicubic uses **zimg, FFmpeg's zscale backend**, with all required clamped halo,
packing, premultiplied float filtering, unpremultiplication/quantization, zero-alpha
RGB clearing and independent owners timed. Native RGB/alpha max1 and both RMSE≤0.25;
only RGBA16 513×259→513×519 permits native RGB≤2 where reference alpha≤16,≤1
elsewhere, retaining alpha≤1/both RMSE≤0.25. Managed frozen pixels and independently
certified rational-half cases remain exact. Linked reports specify every halo
and scope control; no managed tolerance is introduced.

PNG byte RGB/gray/gray-alpha Optimal uses managed9/native6; RGBA Fastest uses2/1,
SmallestSize9/9, with exact samples and the unchanged5% IDAT-size gate. Packed-gray
transparency uses the approved exact timed native color-key correction. Raw
incompatible baselines/refusals remain unscored; approved equivalent replacements
are confirmed. GIF/animation, startup, concurrency and arbitrary profiles are
outside this still-image fixed warm scope.

Maintain the selected evidence in [the renderer](../benchmarks/render-benchmark.py),
regenerate with `python benchmarks/render-benchmark.py`, then run `--check`.
Preserve raw evidence, failures, controls and package/source qualification.
Stop independently confirmed ≤1x cases. Add a later case only when evidence identifies a distinct
required algorithm or public boundary omitted from the finite checklist.
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
