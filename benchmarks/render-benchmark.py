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
    if name in {'warm-jpeg-buffered-eob-evidence.json', 'warm-vp8-cancellable-block-evidence.json',
                'warm-current-resident-png-evidence.json', 'warm-deflate-materialized-cursor-evidence.json',
                'warm-png-async-identify-evidence.json'}:
        status += '; metadata timing open'
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
    return f'| [{label}](../benchmarks/{report}) | â€” | â€” | â€” | â€” | {status} |'


def table(rows):
    return '\n'.join(['| Operation / fixed profile | Lokad Âµs | Reference Âµs | Ratio / upper | Lokad allocated B/op | Status |',
                      '| --- | ---: | ---: | ---: | ---: | --- |', *rows])


def render():
    decodes = [
        row('PNG RGBA8, two retained outputs / caller-owned memory root', 'warm-deflate-materialized-cursor-evidence.json', ['cases', 'Png8Retained2', -1], 'WARM-DEFLATE-MATERIALIZED-CURSOR.md', verification='Gap (short; qualified)'),
        row('PNG DecodeAsync, fresh nonseekable Stream', 'warm-deflate-materialized-cursor-evidence.json', ['cases', 'Png8AsyncNonSeekable', -1], 'WARM-DEFLATE-MATERIALIZED-CURSOR.md', verification='Gap (short; qualified)'),
        row('PNG IdentifyAsync, fresh nonseekable StreamÂ¶', 'warm-png-async-identify-evidence.json', ['cases', 'Png8AsyncNonSeekable', -1], 'WARM-PNG-ASYNC-IDENTIFY.md', verification='Gap (short; qualified)'),
        row('PNG Identify, fresh StreamÂ¶', 'warm-png-stream-identify-evidence.json', ['observations', -1], 'WARM-PNG-STREAM-IDENTIFY.md', True),
        row('WebP Identify, resident span (parser)', 'warm-webp-identify-evidence.json', ['observations', -1], 'WARM-WEBP-IDENTIFY.md', True),
        row('JPEG Identify, resident spanâ€–', 'warm-input-reconciliation-evidence.json', ['cases', 'IdentifyJpeg8', -1], 'WARM-INPUT-RECONCILIATION.md', True),
        row('PNG Identify, resident spanÂ¶', 'warm-png-identify-evidence.json', ['observations', -1], 'WARM-PNG-IDENTIFY.md', True),
        row('PNG palette4/full tRNS Adam7', 'warm-png-palette4-adam7-evidence.json', ['observations', -1], 'WARM-PNG-PALETTE4-ADAM7.md', True),
        row('PNG8 Stream, orient/crop/nearest fit', 'warm-input-reconciliation-evidence.json', ['cases', 'PngCropFit8', -1], 'WARM-INPUT-RECONCILIATION.md', verification='Gap (short; qualified)'),
        row('PNG16 Stream, orient/crop/nearest fit', 'warm-input-reconciliation-evidence.json', ['cases', 'PngCropFit16', -1], 'WARM-INPUT-RECONCILIATION.md', verification='Gap (short; qualified)'),
        row('PNG RGBA8, fresh seekable Stream', 'warm-input-reconciliation-evidence.json', ['cases', 'Png8', -1], 'WARM-INPUT-RECONCILIATION.md', verification='Gap (short; qualified)'),
        row('PNG RGBA16, fresh seekable Stream', 'warm-input-reconciliation-evidence.json', ['cases', 'Png16', -1], 'WARM-INPUT-RECONCILIATION.md', True),
        row('JPEG420 restart, fresh seekable Stream', 'warm-jpeg-buffered-eob-evidence.json', ['cases', 'StreamDecode420Restart', -1], 'WARM-JPEG-BUFFERED-EOB.md', verification='Gap (short; qualified)'),
        row('WebP VP8 + alpha, fresh seekable Stream', 'warm-vp8-cancellable-block-evidence.json', ['cases', 'StreamNative8', -1], 'WARM-VP8-CANCELLABLE-BLOCK.md', verification='Gap (short; qualified)'),
        row('WebP VP8L, fresh seekable Stream', 'warm-input-reconciliation-evidence.json', ['cases', 'StreamNativeLossless8', -1], 'WARM-INPUT-RECONCILIATION.md', verification='Gap (short; qualified)'),
        row('PNG RGBA8, None', 'warm-current-resident-png-evidence.json', ['cases', 'Decode8', -1], 'WARM-CURRENT-RESIDENT-PNG.md', verification='Gap (short; qualified)'),
        row('PNG RGBA8, resident / ImageBudget quota', 'warm-shared-budget-evidence.json', ['cases', 'PngBudgetDecode8', -1], 'WARM-SHARED-BUDGET.md', verification='Gap (short; qualified)'),
        row('PNG RGBA8, None / live cancelable token', 'warm-png-cancel-blocks-evidence.json', ['cases', 'DecodeCancelable8', -1], 'WARM-PNG-CANCEL-BLOCKS.md', verification='Gap (short; qualified)'),
        row('PNG RGBA8, None / exact working limit', 'warm-png-streaming-rows-evidence.json', ['cases', 'DecodeTight8', -1], 'WARM-PNG-STREAMING-ROWS.md', verification='Gap (short; qualified)'),
        row('PNG RGBA8, None / exact limit + live cancelable token', 'warm-png-streaming-rows-evidence.json', ['cases', 'DecodeTightCancelable8', -1], 'WARM-PNG-STREAMING-ROWS.md', verification='Gap (short; qualified)'),
        row('PNG RGBA8, stored Deflate / None', 'warm-current-png-decode-evidence.json', ['cases', 'DecodeNoCompression8', -1], 'WARM-CURRENT-PNG-DECODE.md', True),
        row('PNG RGBA16, stored Deflate / None', 'warm-current-png-decode-evidence.json', ['cases', 'DecodeNoCompression16', -1], 'WARM-CURRENT-PNG-DECODE.md', True),
        row('PNG RGBA16, None', 'warm-current-resident-png-evidence.json', ['cases', 'Decode16', -1], 'WARM-CURRENT-RESIDENT-PNG.md', verification='Gap (short; qualified)'),
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
        row('PNG gray1 â†’ RGBA8, Adam7', 'warm-current-png-adam7-evidence.json', ['cases', 'DecodeGray1Adam7', -1], 'WARM-CURRENT-PNG-ADAM7.md', True),
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
        row('PNG palette4 + tRNS â†’ RGBA8, None', 'warm-current-png-decode-evidence.json', ['cases', 'DecodePalette4', -1], 'WARM-CURRENT-PNG-DECODE.md', True),
        row('PNG palette2 + tRNS â†’ RGBA8, None', 'warm-current-png-decode-evidence.json', ['cases', 'DecodePalette2', -1], 'WARM-CURRENT-PNG-DECODE.md', True),
        row('PNG palette1 + tRNS â†’ RGBA8, None', 'warm-current-png-decode-evidence.json', ['cases', 'DecodePalette1', -1], 'WARM-CURRENT-PNG-DECODE.md', True),
        row('PNG opaque gray1 â†’ RGBA8, None', 'warm-current-png-decode-evidence.json', ['cases', 'DecodeGray1', -1], 'WARM-CURRENT-PNG-DECODE.md', True),
        row('PNG gray1 + tRNS key1 â†’ RGBA8, exact timed FFmpeg color-key', 'warm-png-gray1-transparency-evidence.json', ['observations', -1], 'WARM-PNG-GRAY1-TRANSPARENCY.md', True),
        row('PNG gray2 + tRNS key0 â†’ RGBA8, None', 'warm-png-packed-transparency-evidence.json', ['observations', -1], 'WARM-PNG-PACKED-TRANSPARENCY.md', True),
        pending('PNG gray1/key1 and gray2/key1, unfiltered native', 'WARM-PNG-PACKED-TRANSPARENCY.md', 'Unscored: native alpha differs'),
        row('PNG opaque gray2 â†’ RGBA8, None', 'warm-current-png-decode-evidence.json', ['cases', 'DecodeGray2', -1], 'WARM-CURRENT-PNG-DECODE.md', True),
        row('PNG opaque gray4 â†’ RGBA8, None', 'warm-current-png-decode-evidence.json', ['cases', 'DecodeGray4', -1], 'WARM-CURRENT-PNG-DECODE.md', True),
        row('JPEG baseline gray', 'warm-jpeg-buffered-eob-evidence.json', ['cases', 'GrayResident', -1], 'WARM-JPEG-BUFFERED-EOB.md', verification='Gap (short; qualified)'),
        row('JPEG gray â†’ caller GRAY8', 'warm-jpeg-buffered-eob-evidence.json', ['cases', 'GraySamples', -1], 'WARM-JPEG-BUFFERED-EOB.md', verification='Gap (short; qualified)'),
        row('JPEG untagged â†’ caller CMYK8', 'warm-jpeg-buffered-eob-evidence.json', ['cases', 'CmykCaller', -1], 'WARM-JPEG-BUFFERED-EOB.md', verification='Gap (short; qualified)'),
        row('JPEG 4:2:0 â†’ caller RGB24', 'warm-jpeg-buffered-eob-evidence.json', ['cases', 'Rgb420Samples', -1], 'WARM-JPEG-BUFFERED-EOB.md', True),
        row('JPEG Adobe CMYK, four full-resolution planes', 'warm-jpeg-buffered-eob-evidence.json', ['cases', 'Adobe', -1], 'WARM-JPEG-BUFFERED-EOB.md', verification='Gap (short; qualified)'),
        row('JPEG Adobe CMYK, one AC control', 'warm-jpeg-buffered-eob-evidence.json', ['cases', 'OneAc', -1], 'WARM-JPEG-BUFFERED-EOB.md', verification='Gap (short; qualified)'),
        row('JPEG Adobe CMYK, general transform control', 'warm-jpeg-buffered-eob-evidence.json', ['cases', 'General', -1], 'WARM-JPEG-BUFFERED-EOB.md', verification='Gap (short; qualified)'),
        row('JPEG Adobe CMYK, two-dimensional sparse control', 'warm-jpeg-buffered-eob-evidence.json', ['cases', 'Sparse2D', -1], 'WARM-JPEG-BUFFERED-EOB.md', verification='Gap (short; qualified)'),
        row('JPEG YCCK420, full-resolution black', 'warm-jpeg-buffered-eob-evidence.json', ['cases', 'Ycck420', -1], 'WARM-JPEG-BUFFERED-EOB.md', verification='Gap (short; qualified)'),
        row('JPEG YCCK420 â†’ caller RGB24', 'warm-jpeg-buffered-eob-evidence.json', ['cases', 'Ycck420CallerRgb', -1], 'WARM-JPEG-BUFFERED-EOB.md', verification='Gap (short; qualified)'),
        row('JPEG H4V1 / 4:1:1 â†’ owned RGBA8', 'warm-jpeg-buffered-eob-evidence.json', ['cases', 'Decode411', -1], 'WARM-JPEG-BUFFERED-EOB.md', verification='Gap (short; qualified)'),
        row('JPEG H4V2 / 4:1:0 â†’ owned RGBA8', 'warm-jpeg-buffered-eob-evidence.json', ['cases', 'Decode410', -1], 'WARM-JPEG-BUFFERED-EOB.md', verification='Gap (short; qualified)'),
        row('JPEG baseline 4:2:0', 'warm-jpeg-buffered-eob-evidence.json', ['cases', 'NativeDecode420', -1], 'WARM-JPEG-BUFFERED-EOB.md', verification='Gap (short; qualified)'),
        row('JPEG baseline 4:2:0, row restart markers', 'warm-jpeg-buffered-eob-evidence.json', ['cases', 'NativeDecode420Restart', -1], 'WARM-JPEG-BUFFERED-EOB.md', verification='Gap (short; qualified)'),
        row('JPEG baseline 4:2:2', 'warm-jpeg-buffered-eob-evidence.json', ['cases', 'NativeDecode422', -1], 'WARM-JPEG-BUFFERED-EOB.md', verification='Gap (short; qualified)'),
        row('JPEG baseline 4:4:4, owned input', 'warm-jpeg-buffered-eob-evidence.json', ['cases', 'Decode8', -1], 'WARM-JPEG-BUFFERED-EOB.md', verification='Gap (short; qualified)'),
        row('JPEG baseline 4:4:4, native input', 'warm-jpeg-buffered-eob-evidence.json', ['cases', 'NativeDecode8', -1], 'WARM-JPEG-BUFFERED-EOB.md', verification='Gap (short; qualified)'),
        row('JPEG baseline direct RGB, full resolution', 'warm-jpeg-buffered-eob-evidence.json', ['cases', 'DirectRgb444', -1], 'WARM-JPEG-BUFFERED-EOB.md', verification='Gap (short; qualified)'),
        row('JPEG direct RGB 4:2:2, timed centered correction', 'warm-jpeg-buffered-eob-evidence.json', ['cases', 'DirectRgb422', -1], 'WARM-JPEG-BUFFERED-EOB.md', verification='Gap (short; qualified)'),
        row('JPEG direct RGB 4:2:0, timed component-plane interpretation', 'warm-jpeg-buffered-eob-evidence.json', ['cases', 'DirectRgb420', -1], 'WARM-JPEG-BUFFERED-EOB.md', verification='Gap (short; qualified)'),
        row('JPEG baseline 4:4:4, native input / live cancelable token', 'warm-input-reconciliation-evidence.json', ['cases', 'NativeDecodeCancelable8', -1], 'WARM-INPUT-RECONCILIATION.md', verification='Gap (short; qualified)'),
        row('JPEG progressive gray', 'warm-input-reconciliation-evidence.json', ['cases', 'ProgressiveGray', -1], 'WARM-INPUT-RECONCILIATION.md', verification='Gap (short; qualified)'),
        row('JPEG progressive 4:2:0', 'warm-input-reconciliation-evidence.json', ['cases', 'Progressive420', -1], 'WARM-INPUT-RECONCILIATION.md', verification='Gap (short; qualified)'),
        row('JPEG progressive 4:4:4 / live cancelable token', 'warm-jpeg-411-simd-evidence.json', ['cases', 'ProgressiveCancelable444', -1], 'WARM-JPEG-411-SIMD.md', verification='Gap (short; qualified)'),
        row('JPEG progressive 4:2:2', 'warm-input-reconciliation-evidence.json', ['cases', 'Progressive422', -1], 'WARM-INPUT-RECONCILIATION.md', verification='Gap (short; qualified)'),
        row('JPEG progressive 4:4:4', 'warm-input-reconciliation-evidence.json', ['cases', 'Progressive444', -1], 'WARM-INPUT-RECONCILIATION.md', verification='Gap (short; qualified)'),
        row('WebP VP8 + alpha', 'warm-vp8-cancellable-block-evidence.json', ['cases', 'Native8', -1], 'WARM-VP8-CANCELLABLE-BLOCK.md', verification='Gap (short; qualified)'),
        row('WebP VP8 opaque, simple luma filter', 'warm-vp8-cancellable-block-evidence.json', ['cases', 'SimpleOpaque8', -1], 'WARM-VP8-CANCELLABLE-BLOCK.md', verification='Gap (short; qualified)'),
        row('WebP VP8 opaque, live cancelable token', 'warm-vp8-cancellable-block-evidence.json', ['cases', 'OpaqueCancelable8', -1], 'WARM-VP8-CANCELLABLE-BLOCK.md', verification='Gap (short; qualified)'),
        row('WebP VP8, opaque', 'warm-vp8-cancellable-block-evidence.json', ['cases', 'Opaque8', -1], 'WARM-VP8-CANCELLABLE-BLOCK.md', verification='Gap (short; qualified)'),
        row('WebP VP8L, sixteen-color indexed input', 'warm-vp8l-simd-prediction-evidence.json', ['cases', 'IndexedNative16Colors8', -1], 'WARM-VP8L-SIMD-PREDICTION.md', verification='Gap (short; qualified)'),
        row('WebP VP8L, literal/run input', 'warm-vp8l-simd-prediction-evidence.json', ['cases', 'Lossless8', -1], 'WARM-VP8L-SIMD-PREDICTION.md', verification='Gap (short; qualified)'),
        row('WebP VP8L, native input', 'warm-vp8l-simd-prediction-evidence.json', ['cases', 'NativeLossless8', -1], 'WARM-VP8L-SIMD-PREDICTION.md', verification='Gap (short; qualified)'),
        row('WebP VP8L, native input / live cancelable token', 'warm-vp8l-simd-prediction-evidence.json', ['cases', 'NativeLosslessCancelable8', -1], 'WARM-VP8L-SIMD-PREDICTION.md', verification='Gap (short; qualified)'),
    ]
    encodes = [
        row('WebP EncodeWebpAsync, exact backend* / fresh nonseekable Stream', 'vp8l-color-cost-evidence.json', ['cases', 'WebpLossless8AsyncNonSeekable', -1], 'WARM-VP8L-COLOR-COST.md', verification='Cost screen (CPU parity deferred)'),
        row('JPEG EncodeJpegAsync, caller RGB420 / fresh nonseekable Stream', 'warm-jpeg-async-samples-evidence.json', ['cases', 'JpegRgb420AsyncSamples', -1], 'WARM-JPEG-ASYNC-SAMPLES.md', verification='Gap (short; qualified)'),
        row('JPEG EncodeJpegAsync, RGBA8 / fresh nonseekable Stream / matched quality', 'warm-jpeg-async-encode-evidence.json', ['cases', 'Jpeg8AsyncNonSeekable', -1], 'WARM-JPEG-ASYNC-ENCODE.md', verification='Gap (short; qualified)'),
        row('PNG EncodePngAsync, fresh nonseekable Stream / level6', 'warm-png-async-encode-evidence.json', ['cases', 'Png8AsyncNonSeekable', -1], 'WARM-PNG-ASYNC-ENCODE.md', True),
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
        row('JPEG from caller gray samples, standalone SIMD libjpeg-turbo', 'warm-jpeg-compact-color-evidence.json', ['cases', 'OriginalGray', -1], 'WARM-JPEG-COMPACT-COLOR.md', verification='Gap (short; qualified)'),
        row('JPEG 4:2:0 from caller RGB / optional CompactColor', 'warm-jpeg-compact-color-evidence.json', ['cases', 'CompactColorRgb420', -1], 'WARM-JPEG-COMPACT-COLOR.md', verification='Gap (short; qualified)'),
        row('JPEG 4:2:0 from caller RGB samples', 'warm-jpeg-caller-mcu-evidence.json', ['cases', 'Encode420_8', -1], 'WARM-JPEG-CALLER-MCU.md', verification='Gap (short; qualified)'),
        row('WebP VP8, opaqueâ€¡', 'vp8-noise-spectrum-evidence.json', ['observations', 0], 'WARM-VP8-NOISE-SPECTRUM.md', verification='Cost screen (CPU parity deferred)'),
        row('WebP VP8, opaque from RGBA16â€¡', 'vp8-noise-spectrum-evidence.json', ['observations', 1], 'WARM-VP8-NOISE-SPECTRUM.md', verification='Cost screen (CPU parity deferred)'),
        row('WebP VP8 + alphaâ€¡', 'vp8-noise-spectrum-evidence.json', ['observations', 2], 'WARM-VP8-NOISE-SPECTRUM.md', verification='Cost screen (CPU parity deferred)'),
        row('WebP VP8 + alpha from RGBA16â€¡', 'vp8-noise-spectrum-evidence.json', ['observations', 3], 'WARM-VP8-NOISE-SPECTRUM.md', verification='Cost screen (CPU parity deferred)'),
        row('WebP VP8 + alphaâ€¡, caller work budget32M', 'vp8-histogram384-evidence.json', ['observations', 0], 'WARM-VP8-HISTOGRAM384.md', verification='Cost screen (CPU parity deferred)'),
        row('WebP VP8L, exact backend*', 'vp8l-color-cost-evidence.json', ['cases', 'Encode8', -1], 'WARM-VP8L-COLOR-COST.md', verification='Cost screen (CPU parity deferred)'),
        row('WebP VP8L, opaque RGBA8 / exact backend*', 'vp8l-color-cost-evidence.json', ['cases', 'EncodeOpaque8', -1], 'WARM-VP8L-COLOR-COST.md', verification='Cost screen (CPU parity deferred)'),
        row('WebP VP8L from RGBA16, exact backend*', 'vp8l-color-cost-evidence.json', ['cases', 'Encode16', -1], 'WARM-VP8L-COLOR-COST.md', verification='Cost screen (CPU parity deferred)'),
    ]
    edits = [
        row('Horizontal flip RGBA8, two live outputs / per image', 'warm-horizontal-flip512-evidence.json', ['cases', 'Rgba8', -1], 'WARM-HORIZONTAL-FLIP512.md', verification='Gap (short; qualified)'),
        row('Nearest resize RGBA8', 'warm-coverage-quiet-evidence.json', ['cases', 'nearest8', 'observations', -1], 'WARM-COVERAGE-QUIET.md', True),
        row('Nearest upscale RGBA8', 'warm-nearestup8-evidence.json', ['observations', -1], 'WARM-NEAREST-UP8.md', True),
        row('Nearest upscale RGBA16', 'warm-nearestup16-evidence.json', ['observations', -1], 'WARM-NEAREST-UP16.md', True),
        row('Nearest resize RGBA16', 'warm-prepared-confirm-evidence.json', ['observations', -1], 'WARM-PREPARED-CONFIRM.md', True),
        row('Bilinear resize RGBA8', 'warm-bilinear-byte-row-cache-evidence.json', ['cases', 'Down8', -1], 'WARM-BILINEAR-BYTE-ROW-CACHE.md', verification='Gap (short; qualified)'),
        row('Bilinear resize RGBA16â€ ', 'warm-bilinear-word-cache-evidence.json', ['observations', -1], 'WARM-BILINEAR-WORD-CACHE.md', verification='Gap (short; qualified)'),
        row('Bilinear upscale RGBA8â€ ', 'warm-bilinear-byte-factorization-evidence.json', ['cases', 'Up8', -1], 'WARM-BILINEAR-BYTE-FACTORIZATION.md', verification='Gap (short; qualified)'),
        row('Bilinear upscale RGBA16â€ ', 'warm-current-bilinear-evidence.json', ['cases', 'Up16', -1], 'WARM-CURRENT-BILINEAR.md', True),
        row('RGBA8 bicubic, single-pixel reduction', 'warm-bicubic-single-pixel-evidence.json', ['observations', -1], 'WARM-BICUBIC-SINGLE-PIXEL.md', True),
        row('Bicubic resize RGBA8Â§', 'warm-bicubic-byte-emitter-evidence.json', ['cases', 'Down8', -1], 'WARM-BICUBIC-BYTE-EMITTER.md', verification='Gap (short; qualified)'),
        row('Bicubic resize RGBA16Â§', 'warm-bicubic-byte-emitter-evidence.json', ['cases', 'Down16', -1], 'WARM-BICUBIC-BYTE-EMITTER.md', verification='Gap (short; qualified)'),
        row('Bicubic upscale RGBA8Â§', 'warm-bicubic-byte-emitter-evidence.json', ['cases', 'Up8', -1], 'WARM-BICUBIC-BYTE-EMITTER.md', verification='Gap (short; qualified)'),
        row('Bicubic upscale RGBA16Â§', 'warm-bicubic-byte-emitter-evidence.json', ['cases', 'Up16', -1], 'WARM-BICUBIC-BYTE-EMITTER.md', verification='Gap (short; qualified)'),
        row('Bicubic mixed-axis RGBA8Â§', 'warm-bicubic-byte-paired-evidence.json', ['cases', 'Mixed8', -1], 'WARM-BICUBIC-BYTE-PAIRED.md', verification='Gap (short; qualified)'),
        row('Bicubic mixed-axis RGBA16Â§', 'warm-bicubic-paired-columns-evidence.json', ['cases', 'Mixed16', -1], 'WARM-BICUBIC-PAIRED-COLUMNS.md', verification='Gap (short; qualified)'),
        row('Bicubic width reduction / height enlargement RGBA8Â§', 'warm-bicubic-byte-emitter-evidence.json', ['cases', 'Cross8', -1], 'WARM-BICUBIC-BYTE-EMITTER.md', verification='Gap (short; qualified)'),
        row('Bicubic width reduction / height enlargement RGBA16Â§', 'warm-bicubic-byte-emitter-evidence.json', ['cases', 'Cross16', -1], 'WARM-BICUBIC-BYTE-EMITTER.md', verification='Gap (short; qualified)'),
        row('Bicubic RGBA8, 128Ã—64 / wider supportÂ§', 'warm-current-bicubic-evidence.json', ['cases', 'Wide8', 'observations', -1], 'WARM-CURRENT-BICUBIC.md', True),
        row('Bicubic RGBA16, 128Ã—64 / wider supportÂ§', 'warm-current-bicubic-evidence.json', ['cases', 'Wide16', 'observations', -1], 'WARM-CURRENT-BICUBIC.md', True),
        row('Bicubic width reduction, unchanged height RGBA8Â§', 'warm-bicubic-byte-emitter-evidence.json', ['cases', 'Horizontal8', -1], 'WARM-BICUBIC-BYTE-EMITTER.md', verification='Gap (short; qualified)'),
        row('Bicubic width reduction, unchanged height RGBA16Â§', 'warm-bicubic-byte-emitter-evidence.json', ['cases', 'Horizontal16', -1], 'WARM-BICUBIC-BYTE-EMITTER.md', verification='Gap (short; qualified)'),
        row('Bicubic width enlargement, unchanged height RGBA8Â§', 'warm-bicubic-byte-emitter-evidence.json', ['cases', 'HorizontalUp8', -1], 'WARM-BICUBIC-BYTE-EMITTER.md', verification='Gap (short; qualified)'),
        row('Bicubic width enlargement, unchanged height RGBA16Â§', 'warm-bicubic-enlargement-strips-evidence.json', ['cases', 'HorizontalUp16', -1], 'WARM-BICUBIC-ENLARGEMENT-STRIPS.md', verification='Gap (short; qualified)'),
        row('Bicubic RGBA8, 64Ã—32 / support beyond 32Â§', 'warm-current-bicubic-evidence.json', ['cases', 'Down64x32_8', 'observations', -1], 'WARM-CURRENT-BICUBIC.md', verification='Gap (short; qualified)'),
        row('Bicubic RGBA16, 64Ã—32 / support beyond 32Â§', 'warm-current-bicubic-evidence.json', ['cases', 'Down64x32_16', 'observations', -1], 'WARM-CURRENT-BICUBIC.md', True),
        row('Bicubic RGBA8, 32Ã—16 / support beyond 64Â§', 'warm-current-bicubic-evidence.json', ['cases', 'Down32x16_8', 'observations', -1], 'WARM-CURRENT-BICUBIC.md', True),
        row('Bicubic RGBA16, 32Ã—16 / support beyond 64Â§', 'warm-current-bicubic-evidence.json', ['cases', 'Down32x16_16', 'observations', -1], 'WARM-CURRENT-BICUBIC.md', True),
        row('Bicubic RGBA8, 16Ã—8 / adaptive support beyond 128Â§', 'warm-current-bicubic-evidence.json', ['cases', 'Down16x8_8', 'observations', -1], 'WARM-CURRENT-BICUBIC.md', True),
        row('Bicubic RGBA16, 16Ã—8 / adaptive support beyond 128Â§', 'warm-current-bicubic-evidence.json', ['cases', 'Down16x8_16', 'observations', -1], 'WARM-CURRENT-BICUBIC.md', True),
        row('Bicubic RGBA8, 256Ã—129 / input+output+4 KiB limitÂ§', 'warm-bicubic-byte-strips-evidence.json', ['observations', -1], 'WARM-BICUBIC-BYTE-STRIPS.md', verification='Gap (short; qualified)'),
        row('Bicubic RGBA16, 256Ã—129 / input+output+4 KiB limitÂ§', 'warm-current-bicubic-evidence.json', ['cases', 'TightDown16', 'observations', -1], 'WARM-CURRENT-BICUBIC.md', True),
        row('Bicubic RGBA8, 128Ã—64 / input+output+4 KiB limitÂ§', 'warm-bicubic-four-columns-evidence.json', ['cases', 'TightWide8', -1], 'WARM-BICUBIC-FOUR-COLUMNS.md', verification='Gap (short; qualified)'),
        row('Bicubic RGBA16, 128Ã—64 / input+output+4 KiB limitÂ§', 'warm-current-bicubic-evidence.json', ['cases', 'TightWide16', 'observations', -1], 'WARM-CURRENT-BICUBIC.md', True),
        row('Bicubic RGBA8, 64Ã—32 / input+output+4 KiB limitÂ§', 'warm-bicubic-weight-broadcast-evidence.json', ['cases', 'Tight64x32_8', -1], 'WARM-BICUBIC-WEIGHT-BROADCAST.md', verification='Gap (short; qualified)'),
        row('Bicubic RGBA16, 64Ã—32 / input+output+4 KiB limitÂ§', 'warm-bicubic-word-broadcast-evidence.json', ['cases', 'Tight64x32_16', -1], 'WARM-BICUBIC-WORD-BROADCAST.md', verification='Gap (short; qualified)'),
        row('Bicubic RGBA16, 32Ã—16 / input+output+4 KiB limitÂ§', 'warm-current-bicubic-evidence.json', ['cases', 'Tight32x16_16', 'observations', -1], 'WARM-CURRENT-BICUBIC.md', True),
        row('Bicubic RGBA8, 32Ã—16 / input+output+4 KiB limitÂ§', 'warm-bicubic-packed-reduction-evidence.json', ['cases', 'Tight32x16_8', -1], 'WARM-BICUBIC-PACKED-REDUCTION.md', verification='Gap (short; qualified)'),
        row('Bicubic RGBA16, 16Ã—8 / input+output+4 KiB limitÂ§', 'warm-current-bicubic-evidence.json', ['cases', 'Tight16x8_16', 'observations', -1], 'WARM-CURRENT-BICUBIC.md', True),
        row('Bicubic RGBA8, 16Ã—8 / input+output+4 KiB limitÂ§', 'warm-current-bicubic-evidence.json', ['cases', 'Tight16x8_8', 'observations', -1], 'WARM-CURRENT-BICUBIC.md', True),
        row('Bicubic RGBA16 enlargement / input+output+4 KiB limitÂ§', 'warm-bicubic-enlargement-strips-evidence.json', ['cases', 'TightUp16', -1], 'WARM-BICUBIC-ENLARGEMENT-STRIPS.md', verification='Gap (short; qualified)'),
        row('Bicubic RGBA8 enlargement / input+output+4 KiB limitÂ§', 'warm-bicubic-enlargement-strips-evidence.json', ['cases', 'TightUp8', -1], 'WARM-BICUBIC-ENLARGEMENT-STRIPS.md', verification='Gap (short; qualified)'),
        row('Bicubic RGBA16, width up / height down / input+output+4 KiB limitÂ§', 'warm-bicubic-paired-columns-evidence.json', ['cases', 'TightMixed16', -1], 'WARM-BICUBIC-PAIRED-COLUMNS.md', verification='Gap (short; qualified)'),
        row('Bicubic RGBA8, width up / height down / input+output+4 KiB limitÂ§', 'warm-bicubic-byte-paired-evidence.json', ['cases', 'TightMixed8', -1], 'WARM-BICUBIC-BYTE-PAIRED.md', verification='Gap (short; qualified)'),
        row('Bicubic RGBA16, width down / height up / input+output+4 KiB limitÂ§', 'warm-current-bicubic-evidence.json', ['cases', 'TightCross16', 'observations', -1], 'WARM-CURRENT-BICUBIC.md', True),
        row('Bicubic RGBA8, width down / height up / input+output+4 KiB limitÂ§', 'warm-current-bicubic-evidence.json', ['cases', 'TightCross8', 'observations', -1], 'WARM-CURRENT-BICUBIC.md', True),
        row('Bicubic RGBA16, 8Ã—4 / support beyond 256 / input+output+4 KiB limitÂ§', 'warm-current-bicubic-evidence.json', ['cases', 'Tight8x4_16', 'observations', -1], 'WARM-CURRENT-BICUBIC.md', True),
        row('Bicubic RGBA8, 8Ã—4 / support beyond 256 / input+output+4 KiB limitÂ§', 'warm-current-bicubic-evidence.json', ['cases', 'Tight8x4_8', 'observations', -1], 'WARM-CURRENT-BICUBIC.md', True),
        row('Bicubic RGBA16, 4Ã—2 / packed support beyond 304 / input+output+4 KiB limitÂ§', 'warm-current-bicubic-evidence.json', ['cases', 'Tight4x2_16', 'observations', -1], 'WARM-CURRENT-BICUBIC.md', True),
        row('Bicubic RGBA8, 4Ã—2 / packed support beyond 304 / input+output+4 KiB limitÂ§', 'warm-current-bicubic-evidence.json', ['cases', 'Tight4x2_8', 'observations', -1], 'WARM-CURRENT-BICUBIC.md', True),
        row('Bicubic RGBA16, 2Ã—2 / generated support beyond 544 / input+output+4 KiB limitÂ§', 'warm-current-bicubic-evidence.json', ['cases', 'Tight2x2_16', 'observations', -1], 'WARM-CURRENT-BICUBIC.md', True),
        row('Bicubic RGBA16, 2Ã—9 / bounded row blocks / input+output+4 KiB limitÂ§', 'warm-current-bicubic-evidence.json', ['cases', 'Tight2x9_16', 'observations', -1], 'WARM-CURRENT-BICUBIC.md', True),
        row('Bicubic RGBA16, 4Ã—9 / packed row blocks / input+output+4 KiB limitÂ§', 'warm-current-bicubic-evidence.json', ['cases', 'Tight4x9_16', 'observations', -1], 'WARM-CURRENT-BICUBIC.md', True),
        row('Bicubic RGBA16, 8Ã—17 / full-coefficient row blocks / input+output+4 KiB limitÂ§', 'warm-bicubictight8x17word-generated-evidence.json', ['observations', -1], 'WARM-BICUBIC-TIGHT-8X17-RGBA16.md', True),
        row('Bicubic RGBA8, 8Ã—17 / full-coefficient row blocks / input+output+4 KiB limitÂ§', 'warm-bicubictight8x17byte-generated-evidence.json', ['observations', -1], 'WARM-BICUBIC-TIGHT-8X17-RGBA8.md', True),
        row('Bicubic RGBA8, 4Ã—9 / packed row blocks / input+output+4 KiB limitÂ§', 'warm-current-bicubic-evidence.json', ['cases', 'Tight4x9_8', 'observations', -1], 'WARM-CURRENT-BICUBIC.md', True),
        row('Bicubic RGBA8, 2Ã—9 / bounded row blocks / input+output+4 KiB limitÂ§', 'warm-current-bicubic-evidence.json', ['cases', 'Tight2x9_8', 'observations', -1], 'WARM-CURRENT-BICUBIC.md', True),
        row('Bicubic RGBA8, 2Ã—2 / generated support beyond 544 / input+output+4 KiB limitÂ§', 'warm-current-bicubic-evidence.json', ['cases', 'Tight2x2_8', 'observations', -1], 'WARM-CURRENT-BICUBIC.md', True),
        row('Bicubic height reduction, unchanged width RGBA8Â§', 'warm-bicubic-byte-emitter-evidence.json', ['cases', 'Vertical8', -1], 'WARM-BICUBIC-BYTE-EMITTER.md', verification='Gap (short; qualified)'),
        row('Bicubic height reduction, unchanged width RGBA8 / input+output+4 KiB limitÂ§', 'warm-bicubic-byte-paired-evidence.json', ['cases', 'TightVertical8', -1], 'WARM-BICUBIC-BYTE-PAIRED.md', verification='Gap (short; qualified)'),
        row('Bicubic height reduction, unchanged width RGBA16Â§', 'warm-bicubic-byte-emitter-evidence.json', ['cases', 'Vertical16', -1], 'WARM-BICUBIC-BYTE-EMITTER.md', verification='Gap (short; qualified)'),
        row('Bicubic height reduction, unchanged width RGBA16 / input+output+4 KiB limitÂ§', 'warm-bicubic-paired-columns-evidence.json', ['cases', 'TightVertical16', -1], 'WARM-BICUBIC-PAIRED-COLUMNS.md', verification='Gap (short; qualified)'),
        row('Bicubic width reduction, unchanged height RGBA16 / input+output+4 KiB limitÂ§', 'warm-current-bicubic-evidence.json', ['cases', 'TightHorizontal16', 'observations', -1], 'WARM-CURRENT-BICUBIC.md', True),
        row('Bicubic width reduction, unchanged height RGBA8 / input+output+4 KiB limitÂ§', 'warm-bicubictighthorizontal8-cross-evidence.json', ['observations', -1], 'WARM-BICUBIC-TIGHT-HORIZONTAL8.md', True),
        row('Bicubic height enlargement, unchanged width RGBA8Â§', 'warm-bicubic-byte-emitter-evidence.json', ['cases', 'VerticalUp8', -1], 'WARM-BICUBIC-BYTE-EMITTER.md', True),
        row('Bicubic height enlargement, unchanged width RGBA8 / input+output+4 KiB limitÂ§', 'warm-bicubic-enlargement-strips-evidence.json', ['cases', 'TightVerticalUp8', -1], 'WARM-BICUBIC-ENLARGEMENT-STRIPS.md', verification='Gap (short; qualified)'),
        row('Bicubic height enlargement, unchanged width RGBA16Â§', 'warm-bicubic-byte-emitter-evidence.json', ['cases', 'VerticalUp16', -1], 'WARM-BICUBIC-BYTE-EMITTER.md', verification='Gap (short; qualified)'),
        row('Bicubic height enlargement, unchanged width RGBA16 / input+output+4 KiB limitÂ§', 'warm-bicubic-enlargement-strips-evidence.json', ['cases', 'TightVerticalUp16', -1], 'WARM-BICUBIC-ENLARGEMENT-STRIPS.md', verification='Gap (short; qualified)'),
        row('Bicubic width enlargement, unchanged height RGBA16 / input+output+4 KiB limitÂ§', 'warm-bicubic-enlargement-strips-evidence.json', ['cases', 'TightHorizontalUp16', -1], 'WARM-BICUBIC-ENLARGEMENT-STRIPS.md', verification='Gap (short; qualified)'),
        row('Bicubic width enlargement, unchanged height RGBA8 / input+output+4 KiB limitÂ§', 'warm-bicubic-enlargement-strips-evidence.json', ['cases', 'TightHorizontalUp8', -1], 'WARM-BICUBIC-ENLARGEMENT-STRIPS.md', verification='Gap (short; qualified)'),
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
        row('WebP VP8L photograph RGBA8', 'vp8l-color-cost-evidence.json', ['cases', 'PhotographLossless8', -1], 'WARM-VP8L-COLOR-COST.md', verification='Cost screen (CPU parity deferred)'),
        row('WebP VP8L text/diagram RGBA8', 'vp8l-color-cost-evidence.json', ['cases', 'DiagramLossless8', -1], 'WARM-VP8L-COLOR-COST.md', verification='Cost screen (CPU parity deferred)'),
        row('WebP VP8 photograph RGBA8 / matched quality', 'vp8-noise-spectrum-evidence.json', ['observations', 5], 'WARM-VP8-NOISE-SPECTRUM.md', verification='Cost screen (CPU parity deferred)'),
        row('WebP VP8 text/diagram RGBA8 / matched quality', 'vp8-noise-spectrum-evidence.json', ['observations', 6], 'WARM-VP8-NOISE-SPECTRUM.md', verification='Cost screen (CPU parity deferred)'),
    ])
    edits.extend([
        row('Lanczos3 reduction RGBA8', 'warm-lanczos3-certified-fma-evidence.json', ['cases', 'Reduce8', -1], 'WARM-LANCZOS3-CERTIFIED-FMA.md', verification='Gap (short; qualified)'),
        row('Lanczos3 reduction RGBA16', 'warm-lanczos3-certified-fma-evidence.json', ['cases', 'Reduce16', -1], 'WARM-LANCZOS3-CERTIFIED-FMA.md', verification='Gap (short; qualified)'),
        row('Lanczos3 enlargement RGBA8', 'warm-lanczos3-certified-fma-evidence.json', ['cases', 'Enlarge8', -1], 'WARM-LANCZOS3-CERTIFIED-FMA.md', verification='Gap (short; qualified)'),
        row('Lanczos3 enlargement RGBA16', 'warm-lanczos3-certified-fma-evidence.json', ['cases', 'Enlarge16', -1], 'WARM-LANCZOS3-CERTIFIED-FMA.md', verification='Gap (short; qualified)'),
    ])
    all_rows = decodes + encodes + edits
    assert sum('; metadata timing open' in value for value in all_rows) == 31
    producer_rows = [value.replace('; metadata timing open', '') for value in all_rows]
    parity_count = sum(' | Parity confirmed |' in value for value in producer_rows)
    confidence_count = sum(' | Confidence open |' in value for value in producer_rows)
    gap_count = sum(' | Gap' in value for value in producer_rows)
    reconfirm_count = sum(' | Prior production; reconfirm |' in value for value in producer_rows)
    cost_count = sum(' | Cost screen (CPU parity deferred) |' in value for value in producer_rows)
    assert parity_count + confidence_count + gap_count + reconfirm_count + cost_count == 239
    return f'''# Lokad.Image benchmark state

Updated 2026-10-21. Priority: **complete-packet compression at the frozen quality
gates**. Seven declared lossy WebP controls pass; lossless WebP is next. Verified incremental
gains may cost more CPU. Warm single-CPU parity (independently confirmed upperâ‰¤1x)
remains a deferred objective; speed-only work and repeated confirmations wait
while compression gates remain open. The 239 numeric rows retain tested-producer evidence: **{parity_count} passes,
{gap_count} gaps, {confidence_count} confidence-open case and {cost_count} short cost screens**. The finite
[material checklist](../benchmarks/WARM-COVERAGE.md) has 256 named cases, 17 unmeasured.
The completed â‰¤2x phase belongs to 0.1.0; compression currently takes precedence over CPU parity. Results apply
to the named fixed inputs/settings/hardware, without extrapolating to other profiles.

These tables retain the tested producers identified by each report, including
explicitly labeled compression-first cost screens.
The [typed source-metadata extension](source-metadata.md) is functionally qualified
by4,571 tests/OS,259 affected scalar checks/OS and the same actual private package
consumer/all three examples on Windows/Linux. **31 marked decode/inspection rows
predate its changed header preflight and await timing confirmation.** The complete
metadata-returning PNG reference also needs equivalent typed fields before timing.
There are23 unaffected current rows (two caller JPEG encodes, four Lanczos3, four
async outputs, one bicubic, seven VP8 encodes and five synchronous lossless encodes),
and216 historical/awaiting-confirmation rows. Pixel kernels, source-quality/resource
proofs and unaffected encoder/edit timings retain their identified scope. This does
not prove expanded-scope parity or replacement readiness. Published0.1.0 is immutable.
The checkpoint descriptions below retain their linked producer scopes; the metadata
qualification above supersedes any earlier description of these31 rows as current.

[Buffered JPEG EOB decoding](../benchmarks/WARM-JPEG-BUFFERED-EOB.md) saves 10.0%
on sparse-2D CMYK and 12.2% on one-AC CMYK, preserving exact pixels and allocation
behavior. All21 JPEG rows were qualified at that checkpoint. Caller RGB420
had an independent pass at **0.926x/upper0.931x**; the other20 were gaps.
The unchanged progressive/live-token decoders, metadata, encoders and other
codecs retain their identified scopes. A late affected-list correction included
the already required fresh-stream JPEG case, with no new input or settings.
Full suites reused the verified frozen build through `test.ps1 -NoBuild`;
actual-package consumers and all three examples pass on both OSes.
Largest current gaps are now Lanczos3 reduction, **1.588x** at 16 bits and **1.548x**
at 8 bits. An [exact unsigned-product trial](../benchmarks/WARM-LANCZOS3-PRODUCT-TRIAL.md)
regressed 4.9% and was rejected. A current 944-sample profile puts 50.4% in RGBA16
horizontal filtering. The [vertical-first trial](../benchmarks/WARM-LANCZOS3-VERTICAL-FIRST-TRIAL.md)
cut allocation 71.1% but had no elapsed gain and was also rejected. Accepted
production is restored. [Four-column bicubic strips](../benchmarks/WARM-BICUBIC-FOUR-COLUMNS.md)
then save **16.2%** on TightWide8: **1.433x/upper1.644x**, 32,913 B/op, with exact
pixels and the same 4 KiB reservation. Full suites and the same actual package
qualify both OSes. Only that existing row changes. The
[original VP8 block reader](../benchmarks/WARM-VP8-COEFFICIENT-BLOCK.md) saved 11.2%
at its checkpoint. A [live-token block helper](../benchmarks/WARM-VP8-CANCELLABLE-BLOCK.md)
now saves **11.6%**, reaching **1.395x/upper1.437x**, with 760,572 B/op unchanged,
while preserving every per-decision cancellation poll and exact failure state.
All five existing VP8 rows refresh; four control point means are 0.5â€“1.8% slower,
with all quality/owner/quiet gates retained and no small-movement retuning. A
[prepared-double Lanczos trial](../benchmarks/WARM-LANCZOS3-PREPARED-FMA-TRIAL.md)
is correct but 1.4% slower, at 154,894 B/op, and is rejected. Accepted production
and all selected rows remain unchanged. The already named
[caller-owned-root baseline](../benchmarks/WARM-COOPERATIVE-MEMORY.md) led to a
[materialized Deflate cursor](../benchmarks/WARM-DEFLATE-MATERIALIZED-CURSOR.md):
two retained decodes improve **15.8%**, to **1.265x/upper1.304x**, 2,574,671 B/op.
The admitted peak stays 2,306,560 bytes including resident input. Async PNG decode
improves **23.2%**, to **1.113x/upper1.176x**, at unchanged 12,784 B/op. Exact
pixels/work/failure state, independent survivors, refusal/cancellation cleanup and
both guards pass. All21 async managed controls revalidate; unchanged native
controls are reused. Two current rows refresh; previously historical PNG decode
rows remain historical. Metadata, encoding and other codec/editing paths retain
their scopes. The 924-sample profile describes the previous producer only.
The [FP32 byte Lanczos trial](../benchmarks/WARM-LANCZOS3-FP32-BYTE-TRIAL.md)
is rejected (50.7% slower); exact accepted production and tests are restored.
All60 focused normal/scalar/AVX512-off controls pass perOS. New sampled profiles
retain their respective producers. No selected row or full/package proof repeats.
The [current resident PNG checks](../benchmarks/WARM-CURRENT-RESIDENT-PNG.md)
resolve those two old measurements without builds or repeated qualification;
RGBA8 is1.134x/upper1.330x and RGBA16 is1.039x/upper1.064x.
Both remain gaps; no longer repeat. The
[paired-integer Lanczos trial](../benchmarks/WARM-LANCZOS3-PAIRED-BYTE-TRIAL.md)
parks its small3.5% point-mean gain; once-prepared factor packing is neutral.
Both variants retain exact pixels, original controls and admitted storage;
accepted production is restored with no full/package repeat. The
[current sparse JPEG diagnostic](../benchmarks/WARM-JPEG-SPARSE-COST.md) places
20.4% of963samples in the integer inverse transform; entropy/scan work is also
material. No timing claim. The stronger
[exact-quadrant trial](../benchmarks/WARM-JPEG-SPARSE-QUADRANT-TRIAL.md)
reduces frequency products64â†’16 but only saves a0.6% point mean; it is parked
and accepted production is restored without full/package repeats. The
[current direct-RGB diagnostic](../benchmarks/WARM-JPEG-DIRECT-RGB-COST.md)
places 47.6% of934 samples in baseline AC reading and1.3% in SIMD RGB packing.
The frozen Sparse2D packet contains no stuffed bytes. No timing claim or new
workload matrix. JPEG tuning is deferred while lossless WebP size takes priority.
[Certified FMA Lanczos3](../benchmarks/WARM-LANCZOS3-CERTIFIED-FMA.md) saves
13.6â€“23.8%, with fewer allocations and exact pixels; its four gaps are now
1.061â€“1.588x. Two full-FMA horizontal trials and the earlier prepared-row trial
were rejected. Earlier profiles describe their original producers only.
Optimize demonstrated gaps; stop passing cases. No pixel precision is relaxed.
The [sparse JPEG FP32-FMA trial](../benchmarks/WARM-JPEG-SPARSE-FMA-TRIAL.md)
regressed7.2%. Subsequent [storage trials](../benchmarks/WARM-JPEG-STORAGE-TRIALS.md)
cut allocation 48.9â€“86.9%, but failed to improve elapsed time. Those candidates were
removed; their ownership checks passed. A [constant-DC trial](../benchmarks/WARM-JPEG-DC-TRIAL.md)
saved only 1.2% and was parked. Accepted integer/EOB rounds preserve exact pixels;
full parity remains open.

Historical async PNG and lossless WebP output producers independently passed CPU parity. WebP uses approved
libwebp exact=1 (FFmpeg encoder backend), retaining exact hidden RGB and compressed
sizes **145,648/183,812 B** at that historical producer. Current alpha packets are
**60,904/183,812 B**; their refreshed CPU costs are below. Prior timing passes
do not establish current CPU parity.
All six declared lossless WebP packet gates pass: photograph159,104/ref160,032 B,
diagram7,160/ref7,234 B, opaque41,088/ref121,896 B and alpha8/16/async60,904/ref183,812 B.
These fixed controls stop; broader content/profile parity is unproven. Exact pixels and
unchanged libwebp exact=1 reference/settings remain mandatory. The
[caller-JPEG controls](../benchmarks/WARM-JPEG-CALLER-MCU.md) also leave a joint
size/quality gap: RGB42074,040/52,093 B at source-PSNR difference0.106110 dB
(gate0.25 dB); grayscale58,823/58,910 B. RGB420 compression improvements must
retain matched reconstruction quality and avoid material grayscale regression.
These content/sampling controls do not establish general compression equivalence.
The [spatial coding-group round](../benchmarks/WARM-VP8L-SPATIAL-ENTROPY.md)
reduces the fixed photograph from184,518 to174,748 B; diagram remains7,636 B.
The subsequent [coding-cost eligibility round](../benchmarks/WARM-VP8L-CODE-COST.md)
reduces the existing opaque control from170,156 to123,602 B against121,896 B.
Those six historical C49 encodes independently passed CPU parity. The
[adaptive-map round](../benchmarks/WARM-VP8L-ADAPTIVE-GROUPS.md) then saved2,330 B.
The [residual-color round](../benchmarks/WARM-VP8L-RESIDUAL-COLOR.md) saved2,610 B.
The [uncached-model round](../benchmarks/WARM-VP8L-NO-CACHE.md) saved another5,800 B
on the photograph, which remains **164,008/ref160,032 B**.
The [backward-reference round](../benchmarks/WARM-VP8L-REFERENCES.md) closes the fixed
diagram gate: **7,636→7,160/ref7,234 B**, saving476 B with exact pixels.
The [code-cost predictor round](../benchmarks/WARM-VP8L-CODECOST-PREDICTION.md)
saved2,598 B: photograph **161,410/ref160,032 B** at that producer.
The [seeded-reference round](../benchmarks/WARM-VP8L-SEEDED-REFERENCES.md)
closes opaque **123,602→41,088/ref121,896 B** and reduces already-passing
alpha8/16/async **145,648→60,904/ref183,812 B**, with exact RGBA. These controls
stop; the following photograph refinements close its remaining declared gap.
The [bounded second refinement](../benchmarks/WARM-VP8L-SECOND-REFINEMENT.md)
saved492 B: photograph **160,918/ref160,032 B** at that producer.
The [color refit](../benchmarks/WARM-VP8L-COLOR-REFIT.md) saved306 B, reaching
**160,612/ref160,032 B** at that producer. The current
[color-code-cost round](../benchmarks/WARM-VP8L-COLOR-COST.md) saves **1,508 B**:
photograph **159,104/ref160,032 B**, **928 B below** reference. All **six declared
VP8L complete-packet gates pass and stop**, with exact normalized/hidden RGBA.
Actual current channel/coefficient-map codes price one fixed coordinate pass;
the shared color-candidate backend preserves admitted overlap, borrowed owners and
terminal cleanup. **4,856tests/OS**,121internal+117public scalar and the SAME actual
package qualify the change. A status-only SPEC correction reuses these valid binaries/proofs.
One photograph cost refreshes: **7.330x/upper8.250x**, **67,414,112 B/op**, accepting
extra CPU/allocation for compression. Five C110 costs/producer identities remain
selected after skipped-analysis/identical-packet reconciliation; no timing repeats,
independent CPU confirmation or new workload. CPU parity remains deferred. Default
JPEG and two PNG packet gates stay open. The earlier
[structure audit](../benchmarks/WARM-VP8L-COMPRESSION-STRUCTURE.md) is diagnostic evidence.
The [seven-control lossy size ledger](../benchmarks/WARM-VP8-SIZE-COVERAGE.md)
now records **seven passing complete-packet gates**. Fixed primary
photograph17,366/ref17,688 B and diagram6,722/ref7,100 B retain their matched
managed77/75 versus native75/effort4 settings; source-PSNR gaps0.140944/0.117409 dB
pass unchanged0.25dB and exact-alpha gates. Passing complete cases stay stopped.

All **seven declared lossy WebP complete-packet gates pass**. The
[token-histogram round](../benchmarks/WARM-VP8-HISTOGRAM384.md) reduces the final
work32M packet **99,448→98,490/ref98,590 B**, 100 B below its fixed reference.
Every signed coefficient, native/public pixel, source-quality record and compressed
ALPH byte is exactly C87. One token-class counter replaces repeated branch updates;
384 cached blocks support final entropy fitting while 128 still controls coefficient
decisions. The 63,232 B owner is shared-admitted before allocation; actual 31,802,951
units fit 32M, terminal failure/ownership contracts preserved. Both OSes pass 4,759
tests, 281 internal + 14 public + 4 differential scalar and the same actual package/all examples.
One guarded short cost replaces this row; six historical C83 costs retain producer
identities without new current-code confirmations. The other passing packets are
opaque8/16 **70,298/ref72,380 B**, alpha8/16 **96,016/ref98,590 B**, photograph
**17,366/ref17,688 B** and diagram **6,722/ref7,100 B**. Frozen inputs/settings and
quality gates remain. These controls establish no broader compression or CPU-parity
claim. Lossless WebP is next; default JPEG/PNG complete-packet gaps and the full
finite single-CPU warm-performance objective remain open.

The earlier [adaptive-predictor screen](../benchmarks/WARM-VP8L-ADAPTIVE-PREDICTOR-AUDIT.md)
was rejected on its original basis. Its recipe is now reconsidered and qualified
with current coding groups above; the historical2,652B is not a current-code gain.
The [opaque coding-cost audit](../benchmarks/WARM-VP8L-OPAQUE-COST-AUDIT.md)
identified the skipped plan subsequently selected and qualified above. Its original
untimed evidence remains historical; the six table rows use the accepted producer.

The two JPEG async-output cases remain short gaps; all timings/allocations follow.

The 17 unmeasured representatives cover header inspection, drawing and text.
[PNG fixed-workspace controls](../benchmarks/WARM-PNG-HEADERS-CONTROLS.md)
agree on all 21 original cases, including deliberately opaque CRC/Deflate. Timing
awaits the explicitly labeled bounded FFmpeg AVIO/descriptor adapter decision;
complete-packet parsing would add whole-input buffering outside this boundary.
Full warm parity and release readiness remain open.
The [default-instance font extension](text-default-instance-qualification.md) passes
4,596 correctness tests/OS,105 affected scalar checks, twelve public font controls
and the same actual package/examples on both platforms. It changes no codec/edit/
inspection benchmark path. One authored default-variation layout representative
is added because region/row validation is a distinct required path; it awaits
reference-boundary qualification and timing. Existing timing evidence is reused.


JPEG caller-sample packets are **58,823/58,910 B** grayscale (managed/SIMD libjpeg-turbo),
**74,040/52,093 B** Original RGB420, and **48,701/52,093 B** opt-in CompactColor
RGB420 (managed/FFmpeg). Source-PSNR gaps are0.000033/0.106110/0.017274dB inside
0.25dB. Equal quality numbers are not the matching rule; the
[optional-policy screen](../benchmarks/WARM-JPEG-COMPACT-COLOR.md) includes complete
time and allocations, while retaining the Original control. WebP photograph/diagram
lossless sizes are169,808/160,032 B
and 7,636/7,234 B; [lossy sizes](../benchmarks/WARM-VP8-DIRECTION-COVERAGE.md)
are 17,366/17,688 B and 6,722/7,100 B at the documented matched source quality (managed77/75, native75). Compression-
efficiency parity is not established. Current operation timings/allocations follow.

Dedicated AMD EPYC9V45 VM; Ubuntu24.04.4; .NET10.0.8/SDK10.0.204;
FFmpeg n8.1 (`9047fa1b084f`). One pinned CPU, one codec/conversion/filter thread,
normal SIMD, tiering off, nonconcurrent workstation GC. Inputs are the two original
513Ã—259 RGBA8/16 rasters, the fixed photograph/text diagram pair and documented projections; exact case dimensions and
policies are in linked reports. Only the two required content controls were added; no settings grid.

Times include complete public operations, admission, conversion, fresh independent
output ownership and disposal. Native contexts may be reused; managed private
storage may be reused after release. No decoded output or descriptor is cached.
Fresh Stream rows include source-wrapper/input acquisition, reads/copies/EOF and
release. Startup, I/O, IPC, preparation/hashing and validation are excluded.

Ratio = Lokad.Image / faster native pre/post mean; lower is faster. Upper retains
the larger selected/all-actual confidence bound against the faster native bracket's
lower bound. Parity confirmed requires an independent quiet repeat with upperâ‰¤1;
a meanâ‰¤1 with upper>1 stays confidence-open. Gap rows exceed the new target.
Independent jobs retain all 12 actuals; explicitly labeled short screens retain
all three. Both retain 5s `exclusive-vm-idle-v2` guards. Earlier records
retain their original protocol and failures. **B/op** is managed allocation from
BenchmarkDotNet/raw GC bytes per operation, rounded up. Native allocations are
**unmeasured**, not zero. Timings were collected across rounds; unchanged-path
reconciliation now covers every numeric row.

**Decode and metadata**

{table(decodes)}

**Encode**

{table(encodes)}

**Editing**

{table(edits)}

* VP8L encode uses **libwebp exact=1, FFmpeg's encoder backend**, through its
public API because FFmpeg does not expose hidden-RGB preservation. All decoded
RGBA samples remain exact. Single-component grayscale JPEG uses the approved
benchmark-only **SIMD libjpeg-turbo3.1.4.1** reference; FFmpeg emits three components.
Production stays safe managed .NET 10 without dependencies or native bindings.

â€¡ Procedural VP8 benchmark inputs freeze managed/native quality 100/75 and effort 4, retaining
the 0.25 dB source-quality gate and exact alpha. Matched-quality content controls use
their linked frozen settings. [Current packet sizes](../benchmarks/WARM-VP8-DIRECTION-COVERAGE.md)
and per-codec reports accompany time/allocation evidence; size parity remains open.

Â¶ PNG Identify references use the public FFmpeg PNG parser/CRC plus original
complete structural/EXIF validation and owned descriptors, without inflation;
the Stream reference also times fresh AVIO/scratch/input/EOF and release.
â€– JPEG Identify uses FFmpeg MJPEG discard-all, original complete structural/EXIF
validation and a timed17-byte canonical SOF2 metadata projection, fresh packet/
descriptor ownership and reset, with zero pixel-buffer requests. WebP Identify
uses the public FFmpeg WebP parser plus original complete RIFF/EXIF validation,
including mandatory parser buffering/EOF flush and descriptor ownership. Its
0.0040x/upper0.0045x is specific to this metadata boundary and parser copy cost.
All six managed metadata fields remain exact.

â€  RGBA16 bilinear and explicit RGBA8 upscale allow native-only alpha max1/RMSEâ‰¤0.25;
Up8 times zero-alpha RGB clearing. Managed pixels remain exact; Down8 alpha is exact.
Â§ Bicubic uses **zimg, FFmpeg's zscale backend**, with all required clamped halo,
packing, premultiplied float filtering, unpremultiplication/quantization, zero-alpha
RGB clearing and independent owners timed. Native RGB/alpha max1 and both RMSEâ‰¤0.25;
only RGBA16 513Ã—259â†’513Ã—519 permits native RGBâ‰¤2 where reference alphaâ‰¤16,â‰¤1
elsewhere, retaining alphaâ‰¤1/both RMSEâ‰¤0.25. Managed frozen pixels and independently
certified rational-half cases remain exact. Linked reports specify every halo
and scope control; no managed tolerance is introduced.

PNG byte RGB/gray/gray-alpha Optimal uses managed9/native6; RGBA Fastest uses2/1,
SmallestSize9/9, with exact samples and the historical5% IDAT timing-equivalence
gate. [Complete-packet reconciliation](../benchmarks/PNG-COMPLETE-PACKET-AUDIT.md)
finds15 retained passes and two open level6RGBA8 boundaries, both218,775/210,973 B.
A timing pass or IDAT tolerance is not a packet-size pass. Packed-gray
transparency uses the approved exact timed native color-key correction. Raw
incompatible baselines/refusals remain unscored; approved equivalent replacements
are confirmed. GIF/animation, startup, concurrency and arbitrary profiles are
outside this still-image fixed warm scope.

Maintain the selected evidence in [the renderer](../benchmarks/render-benchmark.py),
regenerate with `python benchmarks/render-benchmark.py`, then run `--check`.
Preserve raw evidence, failures, controls and package/source qualification.
Defer CPU-only optimization while compression gates are open; retain passing
performance evidence. Add a later case only when evidence identifies a distinct
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
