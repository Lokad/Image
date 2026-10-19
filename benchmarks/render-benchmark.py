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
        row('JPEG Identify, resident span‖', 'warm-input-reconciliation-evidence.json', ['cases', 'IdentifyJpeg8', -1], 'WARM-INPUT-RECONCILIATION.md', True),
        row('PNG Identify, resident span¶', 'warm-png-identify-evidence.json', ['observations', -1], 'WARM-PNG-IDENTIFY.md', True),
        row('PNG palette4/full tRNS Adam7', 'warm-png-palette4-adam7-evidence.json', ['observations', -1], 'WARM-PNG-PALETTE4-ADAM7.md', True),
        row('PNG8 Stream, orient/crop/nearest fit', 'warm-input-reconciliation-evidence.json', ['cases', 'PngCropFit8', -1], 'WARM-INPUT-RECONCILIATION.md', verification='Gap (short; qualified)'),
        row('PNG16 Stream, orient/crop/nearest fit', 'warm-input-reconciliation-evidence.json', ['cases', 'PngCropFit16', -1], 'WARM-INPUT-RECONCILIATION.md', verification='Gap (short; qualified)'),
        row('PNG RGBA8, fresh seekable Stream', 'warm-input-reconciliation-evidence.json', ['cases', 'Png8', -1], 'WARM-INPUT-RECONCILIATION.md', verification='Gap (short; qualified)'),
        row('PNG RGBA16, fresh seekable Stream', 'warm-input-reconciliation-evidence.json', ['cases', 'Png16', -1], 'WARM-INPUT-RECONCILIATION.md', True),
        row('JPEG420 restart, fresh seekable Stream', 'warm-input-reconciliation-evidence.json', ['cases', 'StreamDecode420Restart', -1], 'WARM-INPUT-RECONCILIATION.md', verification='Gap (short; qualified)'),
        row('WebP VP8 + alpha, fresh seekable Stream', 'warm-input-reconciliation-evidence.json', ['cases', 'StreamNative8', -1], 'WARM-INPUT-RECONCILIATION.md', verification='Gap (short; qualified)'),
        row('WebP VP8L, fresh seekable Stream', 'warm-input-reconciliation-evidence.json', ['cases', 'StreamNativeLossless8', -1], 'WARM-INPUT-RECONCILIATION.md', verification='Gap (short; qualified)'),
        row('PNG RGBA8, None', 'warm-current-png-decode-evidence.json', ['cases', 'Decode8', -1], 'WARM-CURRENT-PNG-DECODE.md', True),
        row('PNG RGBA8, resident / shared budget', 'warm-shared-budget-evidence.json', ['cases', 'PngBudgetDecode8', -1], 'WARM-SHARED-BUDGET.md', verification='Gap (short; qualified)'),
        row('PNG RGBA8, None / live cancelable token', 'warm-png-cancel-blocks-evidence.json', ['cases', 'DecodeCancelable8', -1], 'WARM-PNG-CANCEL-BLOCKS.md', verification='Gap (short; qualified)'),
        row('PNG RGBA8, None / exact working limit', 'warm-png-streaming-rows-evidence.json', ['cases', 'DecodeTight8', -1], 'WARM-PNG-STREAMING-ROWS.md', verification='Gap (short; qualified)'),
        row('PNG RGBA8, None / exact limit + live cancelable token', 'warm-png-streaming-rows-evidence.json', ['cases', 'DecodeTightCancelable8', -1], 'WARM-PNG-STREAMING-ROWS.md', verification='Gap (short; qualified)'),
        row('PNG RGBA8, stored Deflate / None', 'warm-current-png-decode-evidence.json', ['cases', 'DecodeNoCompression8', -1], 'WARM-CURRENT-PNG-DECODE.md', True),
        row('PNG RGBA16, stored Deflate / None', 'warm-current-png-decode-evidence.json', ['cases', 'DecodeNoCompression16', -1], 'WARM-CURRENT-PNG-DECODE.md', True),
        row('PNG RGBA16, None', 'warm-current-png16-bicubic16-evidence.json', ['cases', 'png16', -1], 'WARM-CURRENT-PNG16-BICUBIC16.md', True),
        row('PNG RGBA16, None / exact working limit', 'warm-png-streaming-rows-evidence.json', ['cases', 'DecodeTight16', -1], 'WARM-PNG-STREAMING-ROWS.md', verification='Gap (short; qualified)'),
        row('PNG RGBA16, None / live cancelable token', 'warm-png-cancel-blocks-evidence.json', ['cases', 'DecodeCancelable16', -1], 'WARM-PNG-CANCEL-BLOCKS.md', verification='Gap (short; qualified)'),
        row('PNG RGBA16, None / exact limit + live cancelable token', 'warm-png-streaming-rows-evidence.json', ['cases', 'DecodeTightCancelable16', -1], 'WARM-PNG-STREAMING-ROWS.md', verification='Gap (short; qualified)'),
        row('PNG RGBA8, Paeth', 'warm-paeth-six-channel-evidence.json', ['cases', 'DecodePaeth8', -1], 'WARM-PAETH-SIX-CHANNEL.md', verification='Gap (short; qualified)'),
        row('PNG RGBA8, Up', 'warm-png-up-simd-evidence.json', ['cases', 'DecodeUp8', -1], 'WARM-PNG-UP-SIMD.md', True),
        row('PNG RGBA16, Up', 'warm-png-up-simd-evidence.json', ['cases', 'DecodeUp16', -1], 'WARM-PNG-UP-SIMD.md', True),
        row('PNG RGBA8, Sub', 'warm-png-sub-prefix-evidence.json', ['cases', 'DecodeSub8', -1], 'WARM-PNG-SUB-PREFIX.md', True),
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
        row('PNG RGB8 + tRNS key(48,139,231), None', 'warm-png-rgb-transparency-confirm-evidence.json', ['cases', 'DecodeRgb8Transparency', -1], 'WARM-PNG-RGB-TRANSPARENCY-CONFIRM.md', True),
        row('PNG RGB16 + tRNS key(12347,35806,59265), None', 'warm-current-png-decode-evidence.json', ['cases', 'DecodeRgb16Transparency', -1], 'WARM-CURRENT-PNG-DECODE.md', True),
        row('PNG gray16, None', 'warm-current-png-decode-evidence.json', ['cases', 'DecodeGray16', -1], 'WARM-CURRENT-PNG-DECODE.md', True),
        row('PNG gray-alpha16, None', 'warm-current-png-decode-evidence.json', ['cases', 'DecodeGrayAlpha16', -1], 'WARM-CURRENT-PNG-DECODE.md', True),
        row('PNG byte indices / sixteen-entry palette + tRNS, None', 'warm-png-short-palette-evidence.json', ['observations', 2], 'WARM-PNG-SHORT-PALETTE.md', True),
        row('PNG palette8 + tRNS, None', 'warm-current-png-decode-evidence.json', ['cases', 'DecodePalette8', -1], 'WARM-CURRENT-PNG-DECODE.md', True),
        row('PNG palette8 + two-entry tRNS, None', 'warm-png-partial-alpha-evidence.json', ['cases', 'DecodePalette8PartialAlpha', -1], 'WARM-PNG-PARTIAL-ALPHA.md', True),
        row('PNG palette4 + tRNS → RGBA8, None', 'warm-current-png-decode-evidence.json', ['cases', 'DecodePalette4', -1], 'WARM-CURRENT-PNG-DECODE.md', True),
        row('PNG palette2 + tRNS → RGBA8, None', 'warm-current-png-decode-evidence.json', ['cases', 'DecodePalette2', -1], 'WARM-CURRENT-PNG-DECODE.md', True),
        row('PNG palette1 + tRNS → RGBA8, None', 'warm-current-png-decode-evidence.json', ['cases', 'DecodePalette1', -1], 'WARM-CURRENT-PNG-DECODE.md', True),
        row('PNG opaque gray1 → RGBA8, None', 'warm-current-png-decode-evidence.json', ['cases', 'DecodeGray1', -1], 'WARM-CURRENT-PNG-DECODE.md', True),
        row('PNG gray1 + tRNS key1 → RGBA8, exact timed FFmpeg color-key', 'warm-png-gray1-transparency-evidence.json', ['observations', -1], 'WARM-PNG-GRAY1-TRANSPARENCY.md', True),
        row('PNG gray2 + tRNS key0 → RGBA8, None', 'warm-png-packed-transparency-evidence.json', ['observations', -1], 'WARM-PNG-PACKED-TRANSPARENCY.md', True),
        pending('PNG gray1/key1 and gray2/key1, unfiltered native', 'WARM-PNG-PACKED-TRANSPARENCY.md', 'Unscored: native alpha differs'),
        row('PNG opaque gray2 → RGBA8, None', 'warm-current-png-decode-evidence.json', ['cases', 'DecodeGray2', -1], 'WARM-CURRENT-PNG-DECODE.md', True),
        row('PNG opaque gray4 → RGBA8, None', 'warm-current-png-decode-evidence.json', ['cases', 'DecodeGray4', -1], 'WARM-CURRENT-PNG-DECODE.md', True),
        row('JPEG baseline gray', 'warm-jpeg-gray-current-evidence.json', ['cases', 'OwnedDecodeGray', -1], 'WARM-JPEG-GRAY-CURRENT.md', verification='Gap (short; qualified)'),
        row('JPEG gray → caller GRAY8', 'warm-jpeg-caller-rgb420-evidence.json', ['cases', 'OwnedDecodeGray', -1], 'WARM-JPEG-CALLER-RGB420.md', verification='Gap (short; qualified)'),
        row('JPEG 4:2:0 → caller RGB24', 'warm-jpeg-caller-rgb420-evidence.json', ['cases', 'NativeDecode420', -1], 'WARM-JPEG-CALLER-RGB420.md', True),
        row('JPEG Adobe CMYK, four full-resolution planes', 'warm-jpeg-ycck-simd-evidence.json', ['cases', 'CmykAdobe', -1], 'WARM-JPEG-YCCK-SIMD.md', verification='Gap (short; qualified)'),
        row('JPEG Adobe CMYK, one AC control', 'warm-jpeg-ycck-simd-evidence.json', ['cases', 'CmykAdobeAc', -1], 'WARM-JPEG-YCCK-SIMD.md', verification='Gap (short; qualified)'),
        row('JPEG Adobe CMYK, general transform control', 'warm-jpeg-ycck-simd-evidence.json', ['cases', 'CmykAdobeGeneral', -1], 'WARM-JPEG-YCCK-SIMD.md', verification='Gap (short; qualified)'),
        row('JPEG Adobe CMYK, two-dimensional sparse control', 'warm-jpeg-ycck-simd-evidence.json', ['cases', 'CmykAdobeSparse2D', -1], 'WARM-JPEG-YCCK-SIMD.md', verification='Gap (short; qualified)'),
        row('JPEG YCCK420, full-resolution black', 'warm-jpeg-ycck-simd-evidence.json', ['cases', 'Ycck420', -1], 'WARM-JPEG-YCCK-SIMD.md', verification='Gap (short; qualified)'),
        row('JPEG baseline 4:2:0', 'warm-input-reconciliation-evidence.json', ['cases', 'NativeDecode420', -1], 'WARM-INPUT-RECONCILIATION.md', verification='Gap (short; qualified)'),
        row('JPEG baseline 4:2:0, row restart markers', 'warm-input-reconciliation-evidence.json', ['cases', 'NativeDecode420Restart', -1], 'WARM-INPUT-RECONCILIATION.md', verification='Gap (short; qualified)'),
        row('JPEG baseline 4:2:2', 'warm-input-reconciliation-evidence.json', ['cases', 'NativeDecode422', -1], 'WARM-INPUT-RECONCILIATION.md', verification='Gap (short; qualified)'),
        row('JPEG baseline 4:4:4, owned input', 'warm-input-reconciliation-evidence.json', ['cases', 'Decode8', -1], 'WARM-INPUT-RECONCILIATION.md', verification='Gap (short; qualified)'),
        row('JPEG baseline 4:4:4, native input', 'warm-input-reconciliation-evidence.json', ['cases', 'NativeDecode8', -1], 'WARM-INPUT-RECONCILIATION.md', verification='Gap (short; qualified)'),
        row('JPEG baseline direct RGB, full resolution', 'warm-input-reconciliation-evidence.json', ['cases', 'DirectRgb444', -1], 'WARM-INPUT-RECONCILIATION.md', verification='Gap (short; qualified)'),
        row('JPEG direct RGB 4:2:2, timed centered correction', 'warm-input-reconciliation-evidence.json', ['cases', 'DirectRgb422', -1], 'WARM-INPUT-RECONCILIATION.md', verification='Gap (short; qualified)'),
        row('JPEG direct RGB 4:2:0, timed component-plane interpretation', 'warm-input-reconciliation-evidence.json', ['cases', 'DirectRgb420', -1], 'WARM-INPUT-RECONCILIATION.md', verification='Gap (short; qualified)'),
        row('JPEG baseline 4:4:4, native input / live cancelable token', 'warm-input-reconciliation-evidence.json', ['cases', 'NativeDecodeCancelable8', -1], 'WARM-INPUT-RECONCILIATION.md', verification='Gap (short; qualified)'),
        row('JPEG progressive gray', 'warm-input-reconciliation-evidence.json', ['cases', 'ProgressiveGray', -1], 'WARM-INPUT-RECONCILIATION.md', verification='Gap (short; qualified)'),
        row('JPEG progressive 4:2:0', 'warm-input-reconciliation-evidence.json', ['cases', 'Progressive420', -1], 'WARM-INPUT-RECONCILIATION.md', verification='Gap (short; qualified)'),
        row('JPEG progressive 4:4:4 / live cancelable token', 'warm-input-reconciliation-evidence.json', ['cases', 'ProgressiveCancelable444', -1], 'WARM-INPUT-RECONCILIATION.md', verification='Gap (short; qualified)'),
        row('JPEG progressive 4:2:2', 'warm-input-reconciliation-evidence.json', ['cases', 'Progressive422', -1], 'WARM-INPUT-RECONCILIATION.md', verification='Gap (short; qualified)'),
        row('JPEG progressive 4:4:4', 'warm-input-reconciliation-evidence.json', ['cases', 'Progressive444', -1], 'WARM-INPUT-RECONCILIATION.md', verification='Gap (short; qualified)'),
        row('WebP VP8 + alpha', 'warm-vp8l-simd-prediction-evidence.json', ['cases', 'Native8', -1], 'WARM-VP8L-SIMD-PREDICTION.md', verification='Gap (short; qualified)'),
        row('WebP VP8 opaque, simple luma filter', 'warm-vp8-block-prediction-evidence.json', ['cases', 'SimpleOpaque8', -1], 'WARM-VP8-BLOCK-PREDICTION.md', verification='Gap (short; qualified)'),
        row('WebP VP8 opaque, live cancelable token', 'warm-vp8-block-prediction-evidence.json', ['cases', 'OpaqueCancelable8', -1], 'WARM-VP8-BLOCK-PREDICTION.md', verification='Gap (short; qualified)'),
        row('WebP VP8, opaque', 'warm-vp8-block-prediction-evidence.json', ['cases', 'Opaque8', -1], 'WARM-VP8-BLOCK-PREDICTION.md', verification='Gap (short; qualified)'),
        row('WebP VP8L, sixteen-color indexed input', 'warm-vp8l-simd-prediction-evidence.json', ['cases', 'IndexedNative16Colors8', -1], 'WARM-VP8L-SIMD-PREDICTION.md', verification='Gap (short; qualified)'),
        row('WebP VP8L, literal/run input', 'warm-vp8l-simd-prediction-evidence.json', ['cases', 'Lossless8', -1], 'WARM-VP8L-SIMD-PREDICTION.md', verification='Gap (short; qualified)'),
        row('WebP VP8L, native input', 'warm-vp8l-simd-prediction-evidence.json', ['cases', 'NativeLossless8', -1], 'WARM-VP8L-SIMD-PREDICTION.md', verification='Gap (short; qualified)'),
        row('WebP VP8L, native input / live cancelable token', 'warm-vp8l-simd-prediction-evidence.json', ['cases', 'NativeLosslessCancelable8', -1], 'WARM-VP8L-SIMD-PREDICTION.md', verification='Gap (short; qualified)'),
    ]
    encodes = [
        row('PNG RGBA8, level 6 / None', 'warm-png-chunk-crc-evidence.json', ['cases', 'Encode8', -1], 'WARM-PNG-CHUNK-CRC.md', True),
        row('PNG RGBA8, NoCompression / None', 'warm-png-stored-capacity-evidence.json', ['cases', 'EncodeNoCompression8', -1], 'WARM-PNG-STORED-CAPACITY.md', verification='Gap (short; qualified)'),
        row('PNG RGBA16, NoCompression / None', 'warm-png-stored-capacity-evidence.json', ['cases', 'EncodeNoCompression16', -1], 'WARM-PNG-STORED-CAPACITY.md', True),
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
        row('JPEG 4:4:4, adaptive Huffman / matched quality', 'warm-jpeg-vector-ac-evidence.json', ['cases', 'Encode8', -1], 'WARM-JPEG-VECTOR-AC.md', verification='Gap (short; qualified)'),
        row('JPEG 4:4:4, forward-only output / geometric AC Huffman', 'warm-jpeg-fixed-fields-evidence.json', ['cases', 'Encode444Stream8', -1], 'WARM-JPEG-FIXED-FIELDS.md', verification='Gap (short; qualified)'),
        row('JPEG 4:4:4, live cancelable token / geometric AC Huffman', 'warm-jpeg-fixed-fields-evidence.json', ['cases', 'Encode444Cancelable8', -1], 'WARM-JPEG-FIXED-FIELDS.md', verification='Gap (short; qualified)'),
        row('JPEG 4:4:4 from RGBA16, adaptive Huffman / full normalization', 'warm-jpeg-vector-ac-evidence.json', ['cases', 'Encode16', -1], 'WARM-JPEG-VECTOR-AC.md', verification='Gap (short; qualified)'),
        row('JPEG 4:2:2, adaptive Huffman / matched quality', 'warm-jpeg-vector-ac-evidence.json', ['cases', 'Encode422_8', -1], 'WARM-JPEG-VECTOR-AC.md', verification='Gap (short; qualified)'),
        row('JPEG 4:2:2 from RGBA16, adaptive Huffman / full normalization', 'warm-jpeg-vector-ac-evidence.json', ['cases', 'Encode422_16', -1], 'WARM-JPEG-VECTOR-AC.md', verification='Gap (short; qualified)'),
        row('JPEG 4:2:0, adaptive Huffman / matched quality', 'warm-jpeg-vector-ac-evidence.json', ['cases', 'Encode420_8', -1], 'WARM-JPEG-VECTOR-AC.md', verification='Gap (short; qualified)'),
        row('JPEG 4:2:0, shared budget / matched quality', 'warm-shared-budget-evidence.json', ['cases', 'JpegBudgetEncode4208', -1], 'WARM-SHARED-BUDGET.md', verification='Gap (short; qualified)'),
        row('JPEG 4:2:0, adaptive / row restart markers', 'warm-jpeg-vector-ac-evidence.json', ['cases', 'Encode420Restart8', -1], 'WARM-JPEG-VECTOR-AC.md', verification='Gap (short; qualified)'),
        row('JPEG 4:2:0, forward-only output / geometric AC Huffman', 'warm-jpeg-fixed-fields-evidence.json', ['cases', 'Encode420Stream8', -1], 'WARM-JPEG-FIXED-FIELDS.md', verification='Gap (short; qualified)'),
        row('JPEG 4:2:0 from RGBA16, forward-only output', 'warm-jpeg-fixed-fields-evidence.json', ['cases', 'Encode420Stream16', -1], 'WARM-JPEG-FIXED-FIELDS.md', verification='Gap (short; qualified)'),
        row('JPEG 4:2:0 from RGBA16, live cancelable token', 'warm-jpeg-fixed-fields-evidence.json', ['cases', 'Encode420Cancelable16', -1], 'WARM-JPEG-FIXED-FIELDS.md', verification='Gap (short; qualified)'),
        row('JPEG 4:2:0 from RGBA16, adaptive Huffman', 'warm-jpeg-vector-ac-evidence.json', ['cases', 'Encode420_16', -1], 'WARM-JPEG-VECTOR-AC.md', verification='Gap (short; qualified)'),
        row('JPEG grayscale, standalone SIMD libjpeg-turbo', 'warm-jpeg-vector-ac-evidence.json', ['cases', 'EncodeGray8', -1], 'WARM-JPEG-VECTOR-AC.md', True),
        row('JPEG from caller gray samples, standalone SIMD libjpeg-turbo', 'warm-jpeg-caller-mcu-evidence.json', ['cases', 'EncodeGray8', -1], 'WARM-JPEG-CALLER-MCU.md', verification='Gap (short; qualified)'),
        row('JPEG 4:2:0 from caller RGB samples', 'warm-jpeg-caller-mcu-evidence.json', ['cases', 'Encode420_8', -1], 'WARM-JPEG-CALLER-MCU.md', verification='Gap (short; qualified)'),
        row('WebP VP8, opaque‡', 'warm-vp8-adaptive-probabilities-evidence.json', ['cases', 'EncodeOpaque8', -1], 'WARM-VP8-ADAPTIVE-PROBABILITIES.md', verification='Gap (short; qualified)'),
        row('WebP VP8, opaque from RGBA16‡', 'warm-vp8-adaptive-probabilities-evidence.json', ['cases', 'EncodeOpaque16', -1], 'WARM-VP8-ADAPTIVE-PROBABILITIES.md', verification='Gap (short; qualified)'),
        row('WebP VP8 + alpha‡', 'warm-vp8-adaptive-probabilities-evidence.json', ['cases', 'Encode8', -1], 'WARM-VP8-ADAPTIVE-PROBABILITIES.md', True),
        row('WebP VP8 + alpha from RGBA16‡', 'warm-vp8-adaptive-probabilities-evidence.json', ['cases', 'Encode16', -1], 'WARM-VP8-ADAPTIVE-PROBABILITIES.md', True),
        row('WebP VP8 + alpha‡, caller work budget32M', 'warm-vp8-adaptive-probabilities-evidence.json', ['cases', 'EncodeBudget8', -1], 'WARM-VP8-ADAPTIVE-PROBABILITIES.md', True),
        row('WebP VP8L, exact backend*', 'warm-vp8l-previous-row-evidence.json', ['cases', 'Encode8', -1], 'WARM-VP8L-PREVIOUS-ROW.md', True),
        row('WebP VP8L, opaque RGBA8 / exact backend*', 'warm-vp8l-previous-row-evidence.json', ['cases', 'EncodeOpaque8', -1], 'WARM-VP8L-PREVIOUS-ROW.md', True),
        row('WebP VP8L from RGBA16, exact backend*', 'warm-vp8l-previous-row-evidence.json', ['cases', 'Encode16', -1], 'WARM-VP8L-PREVIOUS-ROW.md', True),
    ]
    edits = [
        row('Horizontal flip RGBA8, two live outputs / per image', 'warm-horizontal-flip512-evidence.json', ['cases', 'Rgba8', -1], 'WARM-HORIZONTAL-FLIP512.md', verification='Gap (short; qualified)'),
        row('Nearest resize RGBA8', 'warm-coverage-quiet-evidence.json', ['cases', 'nearest8', 'observations', -1], 'WARM-COVERAGE-QUIET.md', True),
        row('Nearest upscale RGBA8', 'warm-nearestup8-evidence.json', ['observations', -1], 'WARM-NEAREST-UP8.md', True),
        row('Nearest upscale RGBA16', 'warm-nearestup16-evidence.json', ['observations', -1], 'WARM-NEAREST-UP16.md', True),
        row('Nearest resize RGBA16', 'warm-prepared-confirm-evidence.json', ['observations', -1], 'WARM-PREPARED-CONFIRM.md', True),
        row('Bilinear resize RGBA8', 'warm-bilinear-byte-row-cache-evidence.json', ['cases', 'Down8', -1], 'WARM-BILINEAR-BYTE-ROW-CACHE.md', verification='Gap (short; qualified)'),
        row('Bilinear resize RGBA16†', 'warm-bilinear-word-cache-evidence.json', ['observations', -1], 'WARM-BILINEAR-WORD-CACHE.md', verification='Gap (short; qualified)'),
        row('Bilinear upscale RGBA8†', 'warm-bilinear-byte-factorization-evidence.json', ['cases', 'Up8', -1], 'WARM-BILINEAR-BYTE-FACTORIZATION.md', verification='Gap (short; qualified)'),
        row('Bilinear upscale RGBA16†', 'warm-current-bilinear-evidence.json', ['cases', 'Up16', -1], 'WARM-CURRENT-BILINEAR.md', True),
        row('RGBA8 bicubic, single-pixel reduction', 'warm-bicubic-single-pixel-evidence.json', ['observations', -1], 'WARM-BICUBIC-SINGLE-PIXEL.md', True),
        row('Bicubic resize RGBA8§', 'warm-bicubic-byte-emitter-evidence.json', ['cases', 'Down8', -1], 'WARM-BICUBIC-BYTE-EMITTER.md', verification='Gap (short; qualified)'),
        row('Bicubic resize RGBA16§', 'warm-bicubic-byte-emitter-evidence.json', ['cases', 'Down16', -1], 'WARM-BICUBIC-BYTE-EMITTER.md', verification='Gap (short; qualified)'),
        row('Bicubic upscale RGBA8§', 'warm-bicubic-byte-emitter-evidence.json', ['cases', 'Up8', -1], 'WARM-BICUBIC-BYTE-EMITTER.md', verification='Gap (short; qualified)'),
        row('Bicubic upscale RGBA16§', 'warm-bicubic-byte-emitter-evidence.json', ['cases', 'Up16', -1], 'WARM-BICUBIC-BYTE-EMITTER.md', verification='Gap (short; qualified)'),
        row('Bicubic mixed-axis RGBA8§', 'warm-bicubic-byte-paired-evidence.json', ['cases', 'Mixed8', -1], 'WARM-BICUBIC-BYTE-PAIRED.md', verification='Gap (short; qualified)'),
        row('Bicubic mixed-axis RGBA16§', 'warm-bicubic-paired-columns-evidence.json', ['cases', 'Mixed16', -1], 'WARM-BICUBIC-PAIRED-COLUMNS.md', verification='Gap (short; qualified)'),
        row('Bicubic width reduction / height enlargement RGBA8§', 'warm-bicubic-byte-emitter-evidence.json', ['cases', 'Cross8', -1], 'WARM-BICUBIC-BYTE-EMITTER.md', verification='Gap (short; qualified)'),
        row('Bicubic width reduction / height enlargement RGBA16§', 'warm-bicubic-byte-emitter-evidence.json', ['cases', 'Cross16', -1], 'WARM-BICUBIC-BYTE-EMITTER.md', verification='Gap (short; qualified)'),
        row('Bicubic RGBA8, 128×64 / wider support§', 'warm-current-bicubic-evidence.json', ['cases', 'Wide8', 'observations', -1], 'WARM-CURRENT-BICUBIC.md', True),
        row('Bicubic RGBA16, 128×64 / wider support§', 'warm-current-bicubic-evidence.json', ['cases', 'Wide16', 'observations', -1], 'WARM-CURRENT-BICUBIC.md', True),
        row('Bicubic width reduction, unchanged height RGBA8§', 'warm-bicubic-byte-emitter-evidence.json', ['cases', 'Horizontal8', -1], 'WARM-BICUBIC-BYTE-EMITTER.md', verification='Gap (short; qualified)'),
        row('Bicubic width reduction, unchanged height RGBA16§', 'warm-bicubic-byte-emitter-evidence.json', ['cases', 'Horizontal16', -1], 'WARM-BICUBIC-BYTE-EMITTER.md', verification='Gap (short; qualified)'),
        row('Bicubic width enlargement, unchanged height RGBA8§', 'warm-bicubic-byte-emitter-evidence.json', ['cases', 'HorizontalUp8', -1], 'WARM-BICUBIC-BYTE-EMITTER.md', verification='Gap (short; qualified)'),
        row('Bicubic width enlargement, unchanged height RGBA16§', 'warm-bicubic-enlargement-strips-evidence.json', ['cases', 'HorizontalUp16', -1], 'WARM-BICUBIC-ENLARGEMENT-STRIPS.md', verification='Gap (short; qualified)'),
        row('Bicubic RGBA8, 64×32 / support beyond 32§', 'warm-current-bicubic-evidence.json', ['cases', 'Down64x32_8', 'observations', -1], 'WARM-CURRENT-BICUBIC.md', verification='Gap (short; qualified)'),
        row('Bicubic RGBA16, 64×32 / support beyond 32§', 'warm-current-bicubic-evidence.json', ['cases', 'Down64x32_16', 'observations', -1], 'WARM-CURRENT-BICUBIC.md', True),
        row('Bicubic RGBA8, 32×16 / support beyond 64§', 'warm-current-bicubic-evidence.json', ['cases', 'Down32x16_8', 'observations', -1], 'WARM-CURRENT-BICUBIC.md', True),
        row('Bicubic RGBA16, 32×16 / support beyond 64§', 'warm-current-bicubic-evidence.json', ['cases', 'Down32x16_16', 'observations', -1], 'WARM-CURRENT-BICUBIC.md', True),
        row('Bicubic RGBA8, 16×8 / adaptive support beyond 128§', 'warm-current-bicubic-evidence.json', ['cases', 'Down16x8_8', 'observations', -1], 'WARM-CURRENT-BICUBIC.md', True),
        row('Bicubic RGBA16, 16×8 / adaptive support beyond 128§', 'warm-current-bicubic-evidence.json', ['cases', 'Down16x8_16', 'observations', -1], 'WARM-CURRENT-BICUBIC.md', True),
        row('Bicubic RGBA8, 256×129 / input+output+4 KiB limit§', 'warm-bicubic-byte-strips-evidence.json', ['observations', -1], 'WARM-BICUBIC-BYTE-STRIPS.md', verification='Gap (short; qualified)'),
        row('Bicubic RGBA16, 256×129 / input+output+4 KiB limit§', 'warm-current-bicubic-evidence.json', ['cases', 'TightDown16', 'observations', -1], 'WARM-CURRENT-BICUBIC.md', True),
        row('Bicubic RGBA8, 128×64 / input+output+4 KiB limit§', 'warm-current-bicubic-evidence.json', ['cases', 'TightWide8', 'observations', -1], 'WARM-CURRENT-BICUBIC.md', True),
        row('Bicubic RGBA16, 128×64 / input+output+4 KiB limit§', 'warm-current-bicubic-evidence.json', ['cases', 'TightWide16', 'observations', -1], 'WARM-CURRENT-BICUBIC.md', True),
        row('Bicubic RGBA8, 64×32 / input+output+4 KiB limit§', 'warm-bicubic-weight-broadcast-evidence.json', ['cases', 'Tight64x32_8', -1], 'WARM-BICUBIC-WEIGHT-BROADCAST.md', verification='Gap (short; qualified)'),
        row('Bicubic RGBA16, 64×32 / input+output+4 KiB limit§', 'warm-bicubic-word-broadcast-evidence.json', ['cases', 'Tight64x32_16', -1], 'WARM-BICUBIC-WORD-BROADCAST.md', verification='Gap (short; qualified)'),
        row('Bicubic RGBA16, 32×16 / input+output+4 KiB limit§', 'warm-current-bicubic-evidence.json', ['cases', 'Tight32x16_16', 'observations', -1], 'WARM-CURRENT-BICUBIC.md', True),
        row('Bicubic RGBA8, 32×16 / input+output+4 KiB limit§', 'warm-bicubic-packed-reduction-evidence.json', ['cases', 'Tight32x16_8', -1], 'WARM-BICUBIC-PACKED-REDUCTION.md', verification='Gap (short; qualified)'),
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
        row('Bicubic height reduction, unchanged width RGBA8§', 'warm-bicubic-byte-emitter-evidence.json', ['cases', 'Vertical8', -1], 'WARM-BICUBIC-BYTE-EMITTER.md', verification='Gap (short; qualified)'),
        row('Bicubic height reduction, unchanged width RGBA8 / input+output+4 KiB limit§', 'warm-bicubic-byte-paired-evidence.json', ['cases', 'TightVertical8', -1], 'WARM-BICUBIC-BYTE-PAIRED.md', verification='Gap (short; qualified)'),
        row('Bicubic height reduction, unchanged width RGBA16§', 'warm-bicubic-byte-emitter-evidence.json', ['cases', 'Vertical16', -1], 'WARM-BICUBIC-BYTE-EMITTER.md', verification='Gap (short; qualified)'),
        row('Bicubic height reduction, unchanged width RGBA16 / input+output+4 KiB limit§', 'warm-bicubic-paired-columns-evidence.json', ['cases', 'TightVertical16', -1], 'WARM-BICUBIC-PAIRED-COLUMNS.md', verification='Gap (short; qualified)'),
        row('Bicubic width reduction, unchanged height RGBA16 / input+output+4 KiB limit§', 'warm-current-bicubic-evidence.json', ['cases', 'TightHorizontal16', 'observations', -1], 'WARM-CURRENT-BICUBIC.md', True),
        row('Bicubic width reduction, unchanged height RGBA8 / input+output+4 KiB limit§', 'warm-bicubictighthorizontal8-cross-evidence.json', ['observations', -1], 'WARM-BICUBIC-TIGHT-HORIZONTAL8.md', True),
        row('Bicubic height enlargement, unchanged width RGBA8§', 'warm-bicubic-byte-emitter-evidence.json', ['cases', 'VerticalUp8', -1], 'WARM-BICUBIC-BYTE-EMITTER.md', True),
        row('Bicubic height enlargement, unchanged width RGBA8 / input+output+4 KiB limit§', 'warm-bicubic-enlargement-strips-evidence.json', ['cases', 'TightVerticalUp8', -1], 'WARM-BICUBIC-ENLARGEMENT-STRIPS.md', verification='Gap (short; qualified)'),
        row('Bicubic height enlargement, unchanged width RGBA16§', 'warm-bicubic-byte-emitter-evidence.json', ['cases', 'VerticalUp16', -1], 'WARM-BICUBIC-BYTE-EMITTER.md', verification='Gap (short; qualified)'),
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
        row('Horizontal flip RGBA8', 'warm-horizontal-flip512-evidence.json', ['cases', 'Depth8', -1], 'WARM-HORIZONTAL-FLIP512.md', True),
        row('Horizontal flip RGBA16', 'warm-current-exact-edits-evidence.json', ['cases', 'HorizontalFlip16', 'observations', -1], 'WARM-CURRENT-EXACT-EDITS.md', True),
        row('Half turn RGBA8', 'warm-edit-confidence-evidence.json', ['cases', 'HalfTurn8', 'observations', -1], 'WARM-EDIT-CONFIDENCE.md', True),
        row('Half turn RGBA16', 'warm-current-exact-edits-evidence.json', ['cases', 'HalfTurn16', 'observations', -1], 'WARM-CURRENT-EXACT-EDITS.md', True),
        row('Clockwise quarter turn RGBA8', 'warm-current-exact-edits-evidence.json', ['cases', 'Clockwise8', 'observations', -1], 'WARM-CURRENT-EXACT-EDITS.md', True),
        row('Clockwise quarter turn RGBA16', 'warm-edit-confidence-evidence.json', ['cases', 'Clockwise16', 'observations', -1], 'WARM-EDIT-CONFIDENCE.md', True),
        row('Counterclockwise quarter turn RGBA8', 'warm-current-exact-edits-evidence.json', ['cases', 'Counterclockwise8', 'observations', -1], 'WARM-CURRENT-EXACT-EDITS.md', True),
        row('Counterclockwise quarter turn RGBA16', 'warm-current-exact-edits-evidence.json', ['cases', 'Counterclockwise16', 'observations', -1], 'WARM-CURRENT-EXACT-EDITS.md', True),
        row('Transpose RGBA8', 'warm-edit-confidence-evidence.json', ['cases', 'Transpose8', 'observations', -1], 'WARM-EDIT-CONFIDENCE.md', True),
        row('Transpose RGBA16', 'warm-current-exact-edits-evidence.json', ['cases', 'Transpose16', 'observations', -1], 'WARM-CURRENT-EXACT-EDITS.md', True),
        row('Mirrored transpose RGBA16', 'warm-current-exact-edits-evidence.json', ['cases', 'Transverse16', 'observations', -1], 'WARM-CURRENT-EXACT-EDITS.md', True),
        row('Mirrored transpose RGBA8', 'warm-current-exact-edits-evidence.json', ['cases', 'Transverse8', 'observations', -1], 'WARM-CURRENT-EXACT-EDITS.md', True),
    ]
    encodes.extend([
        row('WebP VP8L photograph RGBA8', 'warm-vp8l-previous-row-evidence.json', ['cases', 'PhotographLossless8', -1], 'WARM-VP8L-PREVIOUS-ROW.md', True),
        row('WebP VP8L text/diagram RGBA8', 'warm-vp8l-previous-row-evidence.json', ['cases', 'DiagramLossless8', -1], 'WARM-VP8L-PREVIOUS-ROW.md', True),
        row('WebP VP8 photograph RGBA8 / matched quality', 'warm-vp8-adaptive-probabilities-evidence.json', ['cases', 'PhotographLossy8', -1], 'WARM-VP8-ADAPTIVE-PROBABILITIES.md', True),
        row('WebP VP8 text/diagram RGBA8 / matched quality', 'warm-vp8-adaptive-probabilities-evidence.json', ['cases', 'DiagramLossy8', -1], 'WARM-VP8-ADAPTIVE-PROBABILITIES.md', True),
    ])
    all_rows = decodes + encodes + edits
    parity_count = sum(' | Parity confirmed |' in value for value in all_rows)
    confidence_count = sum(' | Confidence open |' in value for value in all_rows)
    gap_count = sum(' | Gap' in value for value in all_rows)
    reconfirm_count = sum(' | Prior production; reconfirm |' in value for value in all_rows)
    assert parity_count + confidence_count + gap_count + reconfirm_count == 223
    return f'''# Lokad.Image benchmark state

Updated 2026-10-19. The current goal is **warm single-CPU parity: independent
conservative upper ≤1x**. The 223 retained numeric rows contain **{parity_count} current
parity confirmations, {gap_count} measured gaps and {confidence_count} confidence-open cases**;
all numeric rows have current evidence after the
[input reconciliation](../benchmarks/WARM-INPUT-RECONCILIATION.md). Twenty-two prior
rows were refreshed on the same qualified immutable build: JPEG Identify and
RGBA16 PNG Stream independently pass; twenty other cases remain short gaps.
The preceding baseline grayscale proof is reused. The earlier
≤2x phase was completed for 0.1.0; subsequent qualified improvements distinguish
short screens from independent confirmations. The finite [material checklist](../benchmarks/WARM-COVERAGE.md) and
fixed inputs remain; results do not extrapolate to other images, settings,
sizes or hardware. Stop cases already confirmed at ≤1x.

[Current grayscale JPEG decoding](../benchmarks/WARM-JPEG-GRAY-CURRENT.md)
resolves one stale row: **1.426×**, **672,186B/op**, qualified short gap. Frozen
pixels/reference/boundaries and the same qualified build are reused; no rebuild,
long repeat or repeated package proof. The later input reconciliation above refreshes all remaining prior rows.

[Shared-budget codec boundaries](../benchmarks/WARM-SHARED-BUDGET.md) now have
qualified shorts: PNG RGBA8 decode **1.439× /4,561B/op**, JPEG420 encode
**1.145× /297,781B/op**. Public admission/actual-work settlement and independent
owners are timed; PNG retained-pixel leases and disposal are included. Frozen
JPEG bytes stay **74,040/52,093**, with unchanged quality/alpha gates. Both remain
gaps; no long repeat or production/package rebuild. A long-lived finite caller
budget and its resident-input/source reservation are prepared outside timing.
Direct sample decoding, JPEG compatibility, async I/O, inspection, drawing and
text now account for **twenty-three pending representatives**, with no grid.
Existing numeric evidence is reused where paths remain unchanged.

[Caller RGB420 JPEG decoding](../benchmarks/WARM-JPEG-CALLER-RGB420.md) now
independently passes **0.966× /upper0.971×**, **624,508B/op**, with **71.3% less
time**. Direct SIMD RGB packing preserves exact pixels, limits and ownership
without an RGBA frame. Its gray rejection control remains a short gap at
**1.326× /273,422B/op**. The same complete native operations and gates remain.
All4,176tests/OS,73affected hardware-disabled checks/OS and the same actual private
package consumer/all three examples pass on both OS. Exactly these two numeric
rows change; prior passing paths retain evidence.23new boundaries remain.

[Identical horizontal JPEG rows](../benchmarks/WARM-JPEG-HORIZONTAL-ROW.md)
reduce the one-AC CMYK mean **10.0%**,746.37→671.44µs, at **1.726x FFmpeg**.
DC/general remain **1.395x/1.126x**, about **1,086KB/op** after fourth-buffer reuse.
One new named2D sparse control rejects the shortcut and measures **1.819x**;
it closes a concrete retained-algorithm omission, with the same original raster
and settings. Scope **246finite cases**,24unmeasured. Exact pixels, original
work/admission/owners and native gates pass. All4,197tests/OS,61affected hardware-off
checks/OS and same actual private package consumer/all examples pass on both OS.
Three rows refreshed, one added; all70passing paths retain evidence and stop.

[A bounded exact sparse-prefix trial](../benchmarks/WARM-JPEG-SPARSE-PREFIX-TRIAL.md)
was rejected as immaterial. Accepted production, current rows and qualification
remain unchanged; no long repeat or package rerun.

[Exact SIMD YCCK420 rows](../benchmarks/WARM-JPEG-YCCK-SIMD.md) cut elapsed time
**76.1%**,4,761.24→1,135.97µs: **1.604x /upper1.636x**, **1,470,246B/op**.
Exact managed pixels and the approved native-only transition bound remain.
Four shared-dispatch CMYK controls refresh at1.391x/1.732x/1.113x/1.812x;
all five remain short gaps. All4,202tests/OS,66affected hardware-off checks/OS
and the same actual private package consumer/all three examples pass on both OS.
Exactly five numeric rows change; all218other rows and70passing proofs are reused.
Scope246cases,23unmeasured; no new image/grid. CMYK2D sparse is the largest gap.

`IdentifyAsync`/`DecodeAsync` now use asynchronous stream reads with bounded private
input storage, complete validation and independent outputs; two named PNG8 front-end
representatives remain unmeasured. Shared-budget reservation lifetime across awaits
is correctness-tested. Synchronous Decode now avoids the final full-input copy,
admitting actual retained capacity through decoding. All seven affected existing Stream
workloads now have current evidence in the input reconciliation above.
Resident decoding, Identify and async-input methods retain their existing evidence.

Async PNG/JPEG/WebP output, including direct JPEG samples, uses admitted private
staging and real asynchronous writes, retaining JPEG adaptive coding when admitted.
Four named staging/admission/delivery representatives remain unmeasured; no
codec, shape or settings grid was added. Existing numeric statuses are unchanged.

`InspectHeaders`/`InspectHeadersAsync` provide fixed-workspace dimensions/orientation
inspection, including shared budgets. Three named PNG/JPEG/WebP header boundaries
remain unmeasured; their distinct seek/scan grammars exclude PNG CRCs and compressed
pixel validation. Resolve equivalent native header scope before timing. Existing
full `Identify` checks and numeric evidence are unchanged.

Drawing is correctness-qualified at 8/16 bits, including mixed-depth source-over,
clipping and snapshot semantics for self-overlap without a full-frame copy. Six
named fill/copy/composite cases remain unmeasured. Resolve equivalent native canvas
delivery, state restoration and ownership before timing; no numeric status changed.

Supplied TrueType loading, outlines, layout/measurement and 8/16-bit antialiased
rendering are correctness-qualified. Three layout and two complete rendering
operations remain unmeasured. Prepare the same immutable face outside timing on
both sides; include needed geometry, positions, bounds and independent outputs.
Rendering includes layout and complete canvas updates; no cached masks. Resolve
equivalent font-backend quality/scope first. Analytical coverage and separately
recorded default native raster differences supply no timing/allocation result.

The [direct-sample JPEG quality check](../benchmarks/warm-jpeg-sample-quality-evidence.json)
reproduces the frozen packets exactly: grayscale **58,823 / 58,910 bytes**
(managed / SIMD libjpeg-turbo), RGB 4:2:0 **74,040 / 52,093 bytes** (managed / FFmpeg).
Source PSNR gaps are **0.000033 / 0.106110 dB**, within the existing 0.25 dB gate;
equal quality numbers are not the matching rule. The [two raw-input screens](../benchmarks/WARM-JPEG-SAMPLE-ENCODE.md)
initially measured1.145×gray and2.004×RGB420. [Fused MCU preparation](../benchmarks/WARM-JPEG-CALLER-MCU.md)
now cuts RGB time **33.7%**, to **2,097.34µs /1.328× /297,894B/op**; its packet
and quality remain exact. Gray control is **1.162× /155,007B/op**.
Both remain qualified short parity gaps; each includes necessary normalization and independent
owned output. No long repeat follows these clear gaps; the accepted MCU primitive passes4,169tests/OS,
109affected intrinsics-disabled checks/OS and the same actual private package
consumer/all3examples on both OS. Other evidence is reused.

[Bounded VP8 coefficient adaptation](../benchmarks/WARM-VP8-ADAPTIVE-PROBABILITIES.md)
cuts original opaque bytes **54.5%**, allocations **41.6%**, and complete encode
time **7.9–9.8%**; those cases remain **1.320–1.323×** short gaps. Alpha-bearing
bytes fall **39.2%**, with time **8.5–8.8%** lower. Photograph/diagram packets are
**21,432/17,688 B** and **8,288/7,100 B** (managed/reference), at unchanged frozen
quality and exact old decoded pixels. Their times increase **1.6%/1.7%**; photo
allocation rises3.2%, diagram falls0.2%. Diagram now has an independent
**1.000208× / upper1.009341×** result, so its old parity proof is superseded.
The other three default passing encodes and a new work-budget fallback independently
pass. That fallback uses the existing alpha image,32M work units and the original
471,436-byte fixed-model packet. No new image, quality tuning or settings grid.

[Previous-row VP8L references](../benchmarks/WARM-VP8L-PREVIOUS-ROW.md) retain
all five passing time proofs; current photograph/diagram packets are **383,154/
160,032 B** and **15,120/7,234 B**. Diagram size fell51.3% and allocation48.4%;
the search increased times40–55% versus earlier rounds, still below the reference.
Only six default VP8 timings and their packet metadata changed; the one distinct
caller-budget boundary adds a numeric row. The VP8 round passes **4,167 tests/OS**,
**183 affected intrinsics-disabled checks/OS**, and the same actual private package
consumer/all three examples on both OS. Unaffected evidence is reused. Compression
efficiency has improved; the full warm parity objective remains open.

Dedicated AMD EPYC9V45 VM; Ubuntu24.04.4; .NET10.0.8/SDK10.0.204;
FFmpeg n8.1 (`9047fa1b084f`). One pinned CPU, one codec/conversion/filter thread,
normal SIMD, tiering off, nonconcurrent workstation GC. Inputs are the two original
513×259 RGBA8/16 rasters, the fixed photograph/text diagram pair and documented projections; exact case dimensions and
policies are in linked reports. Only the two required content controls were added; no settings grid.

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
**unmeasured**, not zero. Timings were collected across rounds; unchanged-path
reconciliation now covers every numeric row.

Latest [wider RGBA8 horizontal reversal](../benchmarks/WARM-HORIZONTAL-FLIP512.md)
improves the single-output flip **22.0%**, independently confirming **0.950× /
upper 0.965×**. Two live outputs improve **27.9%** but remain a qualified **1.053×**
gap. Both retain **128 B/image**. At that performance round, other 205 comparisons
and all 63 prior passing proofs remained valid. Its **3,739 Release tests per OS**, 67 affected public
checks/OS with AVX-512 disabled and the **same actual package consumer/all three
examples on Windows/Linux** pass. Production remains safe managed dependency-free
.NET 10; original fallbacks, work/cancellation, admission and ownership remain.

Latest measurements: [three single-owner edits](../benchmarks/WARM-EDIT-CONFIDENCE.md)
independently close transpose RGBA8 at**0.995x upper**. Half turn RGBA8 remains a
small mean gap; clockwise RGBA16 still straddles. [RGB8 PNG transparency](../benchmarks/WARM-PNG-RGB-TRANSPARENCY-CONFIRM.md)
retains its1.012x mean gap/1.014x upper. Current totals are **{parity_count} passes /
{gap_count} gaps / {confidence_count} confidence open / {reconfirm_count} prior-production rows**.
Every actual/outlier and both quiet guards remain; unchanged qualification is reused.
The24 named new boundaries remain unmeasured; stop current passing paths.

**Decode and metadata**

{table(decodes)}

**Encode**

{table(encodes)}

**Editing**

{table(edits)}

* VP8L encode uses **libwebp exact=1, FFmpeg's encoder backend**, through its public
API: the FFmpeg encoder does not expose hidden-RGB preservation. Decoded RGBA is
exact; alpha-bearing packets are533,100/183,812 B and opaque400,262/121,896 B
(managed/native). Compression-efficiency parity
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
