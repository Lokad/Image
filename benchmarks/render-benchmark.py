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
    if name in {'vp8-noise-spectrum-evidence.json', 'vp8-histogram384-evidence.json'}:
        status = 'Prior production; reconfirm'  # Public VP8 encoder replaced; byte proof is separate.
    if name == 'vp8l-color-cost-evidence.json':
        status = 'Prior production; reconfirm'  # Full encoder replaced by packet-exact pipeline.
    if (name == 'warm-jpeg-buffered-eob-evidence.json'
            or name == 'warm-jpeg-411-simd-evidence.json'
            or name == 'warm-input-reconciliation-evidence.json' and label.startswith('JPEG') and 'Identify' not in label):
        status = 'Prior production; reconfirm'  # Every JPEG inverse block now uses the reference integer policy.
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
    return f'| [{label}](../benchmarks/{report}) | — | — | — | — | {status} |'


def table(rows):
    return '\n'.join(['| Operation / fixed profile | Lokad µs | Reference µs | Ratio / upper | Lokad allocated B/op | Status |',
                      '| --- | ---: | ---: | ---: | ---: | --- |', *rows])


def render():
    decodes = [
        row('PNG RGBA8, two retained outputs / caller-owned memory root', 'warm-deflate-materialized-cursor-evidence.json', ['cases', 'Png8Retained2', -1], 'WARM-DEFLATE-MATERIALIZED-CURSOR.md', verification='Gap (short; qualified)'),
        row('PNG DecodeAsync, fresh nonseekable Stream', 'warm-deflate-materialized-cursor-evidence.json', ['cases', 'Png8AsyncNonSeekable', -1], 'WARM-DEFLATE-MATERIALIZED-CURSOR.md', verification='Gap (short; qualified)'),
        row('PNG IdentifyAsync, fresh nonseekable Stream¶', 'warm-png-async-identify-evidence.json', ['cases', 'Png8AsyncNonSeekable', -1], 'WARM-PNG-ASYNC-IDENTIFY.md', verification='Gap (short; qualified)'),
        row('PNG Identify, fresh Stream¶', 'warm-png-stream-identify-evidence.json', ['observations', -1], 'WARM-PNG-STREAM-IDENTIFY.md', True),
        row('WebP Identify, resident span (parser)', 'warm-webp-identify-evidence.json', ['observations', -1], 'WARM-WEBP-IDENTIFY.md', True),
        row('JPEG Identify, resident span‖', 'warm-input-reconciliation-evidence.json', ['cases', 'IdentifyJpeg8', -1], 'WARM-INPUT-RECONCILIATION.md', True),
        row('PNG Identify, resident span¶', 'warm-png-identify-evidence.json', ['observations', -1], 'WARM-PNG-IDENTIFY.md', True),
        row('PNG palette4/full tRNS Adam7', 'warm-png-palette4-adam7-evidence.json', ['observations', -1], 'WARM-PNG-PALETTE4-ADAM7.md', True),
        row('PNG8 Stream, orient/crop/nearest fit', 'warm-input-reconciliation-evidence.json', ['cases', 'PngCropFit8', -1], 'WARM-INPUT-RECONCILIATION.md', verification='Gap (short; qualified)'),
        row('PNG16 Stream, orient/crop/nearest fit', 'warm-input-reconciliation-evidence.json', ['cases', 'PngCropFit16', -1], 'WARM-INPUT-RECONCILIATION.md', verification='Gap (short; qualified)'),
        row('PNG RGBA8, fresh seekable Stream', 'warm-input-reconciliation-evidence.json', ['cases', 'Png8', -1], 'WARM-INPUT-RECONCILIATION.md', verification='Gap (short; qualified)'),
        row('PNG RGBA16, fresh seekable Stream', 'warm-input-reconciliation-evidence.json', ['cases', 'Png16', -1], 'WARM-INPUT-RECONCILIATION.md', True),
        row('JPEG420 restart, fresh seekable Stream', 'warm-jpeg-buffered-eob-evidence.json', ['cases', 'StreamDecode420Restart', -1], 'WARM-JPEG-BUFFERED-EOB.md', verification='Gap (short; qualified)'),
        row('WebP VP8 + alpha, fresh seekable Stream', 'warm-current-vp8-samples-evidence.json', ['observations', -1], 'WARM-CURRENT-VP8-SAMPLES.md', verification='Gap (short; qualified)'),
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
        row('JPEG baseline gray', 'warm-jpeg-buffered-eob-evidence.json', ['cases', 'GrayResident', -1], 'WARM-JPEG-BUFFERED-EOB.md', verification='Gap (short; qualified)'),
        row('JPEG gray → caller GRAY8', 'warm-jpeg-buffered-eob-evidence.json', ['cases', 'GraySamples', -1], 'WARM-JPEG-BUFFERED-EOB.md', verification='Gap (short; qualified)'),
        row('JPEG untagged → caller CMYK8', 'warm-jpeg-buffered-eob-evidence.json', ['cases', 'CmykCaller', -1], 'WARM-JPEG-BUFFERED-EOB.md', verification='Gap (short; qualified)'),
        row('JPEG 4:2:0 → caller RGB24', 'warm-jpeg-buffered-eob-evidence.json', ['cases', 'Rgb420Samples', -1], 'WARM-JPEG-BUFFERED-EOB.md', True),
        row('JPEG Adobe CMYK, four full-resolution planes', 'warm-jpeg-buffered-eob-evidence.json', ['cases', 'Adobe', -1], 'WARM-JPEG-BUFFERED-EOB.md', verification='Gap (short; qualified)'),
        row('JPEG Adobe CMYK, one AC control', 'warm-jpeg-buffered-eob-evidence.json', ['cases', 'OneAc', -1], 'WARM-JPEG-BUFFERED-EOB.md', verification='Gap (short; qualified)'),
        row('JPEG Adobe CMYK, general transform control', 'warm-jpeg-buffered-eob-evidence.json', ['cases', 'General', -1], 'WARM-JPEG-BUFFERED-EOB.md', verification='Gap (short; qualified)'),
        row('JPEG Adobe CMYK, two-dimensional sparse control', 'warm-jpeg-buffered-eob-evidence.json', ['cases', 'Sparse2D', -1], 'WARM-JPEG-BUFFERED-EOB.md', verification='Gap (short; qualified)'),
        row('JPEG YCCK420, full-resolution black', 'warm-jpeg-buffered-eob-evidence.json', ['cases', 'Ycck420', -1], 'WARM-JPEG-BUFFERED-EOB.md', verification='Gap (short; qualified)'),
        row('JPEG YCCK420 → caller RGB24', 'warm-jpeg-buffered-eob-evidence.json', ['cases', 'Ycck420CallerRgb', -1], 'WARM-JPEG-BUFFERED-EOB.md', verification='Gap (short; qualified)'),
        row('JPEG H4V1 / 4:1:1 → owned RGBA8', 'warm-jpeg-buffered-eob-evidence.json', ['cases', 'Decode411', -1], 'WARM-JPEG-BUFFERED-EOB.md', verification='Gap (short; qualified)'),
        row('JPEG H4V2 / 4:1:0 → owned RGBA8', 'warm-jpeg-buffered-eob-evidence.json', ['cases', 'Decode410', -1], 'WARM-JPEG-BUFFERED-EOB.md', verification='Gap (short; qualified)'),
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
        row('WebP VP8 + alpha', 'warm-vp8-cancellable-block-evidence.json', ['cases', 'Native8', -1], 'WARM-VP8-CANCELLABLE-BLOCK.md', verification='Prior production; reconfirm'),
        row('WebP VP8 opaque, simple luma filter', 'warm-vp8-cancellable-block-evidence.json', ['cases', 'SimpleOpaque8', -1], 'WARM-VP8-CANCELLABLE-BLOCK.md', verification='Prior production; reconfirm'),
        row('WebP VP8 opaque, live cancelable token', 'warm-vp8-cancellable-block-evidence.json', ['cases', 'OpaqueCancelable8', -1], 'WARM-VP8-CANCELLABLE-BLOCK.md', verification='Prior production; reconfirm'),
        row('WebP VP8, opaque', 'warm-vp8-cancellable-block-evidence.json', ['cases', 'Opaque8', -1], 'WARM-VP8-CANCELLABLE-BLOCK.md', verification='Prior production; reconfirm'),
        row('WebP VP8L, sixteen-color indexed input', 'warm-vp8l-simd-prediction-evidence.json', ['cases', 'IndexedNative16Colors8', -1], 'WARM-VP8L-SIMD-PREDICTION.md', verification='Gap (short; qualified)'),
        row('WebP VP8L, literal/run input', 'warm-vp8l-simd-prediction-evidence.json', ['cases', 'Lossless8', -1], 'WARM-VP8L-SIMD-PREDICTION.md', verification='Gap (short; qualified)'),
        row('WebP VP8L, native input', 'warm-vp8l-simd-prediction-evidence.json', ['cases', 'NativeLossless8', -1], 'WARM-VP8L-SIMD-PREDICTION.md', verification='Gap (short; qualified)'),
        row('WebP VP8L, native input / live cancelable token', 'warm-vp8l-simd-prediction-evidence.json', ['cases', 'NativeLosslessCancelable8', -1], 'WARM-VP8L-SIMD-PREDICTION.md', verification='Gap (short; qualified)'),
    ]
    encodes = [
        row('WebP EncodeWebpAsync, exact backend* / fresh nonseekable Stream', 'vp8l-color-cost-evidence.json', ['cases', 'WebpLossless8AsyncNonSeekable', -1], 'WARM-VP8L-COLOR-COST.md', verification='Cost screen (CPU parity deferred)'),
        row('JPEG EncodeJpegAsync, caller RGB420 / fresh nonseekable Stream', 'warm-jpeg-async-samples-evidence.json', ['cases', 'JpegRgb420AsyncSamples', -1], 'WARM-JPEG-ASYNC-SAMPLES.md', verification='Prior production; reconfirm'),
        row('JPEG EncodeJpegAsync, RGBA8 / fresh nonseekable Stream / matched quality', 'warm-jpeg-async-encode-evidence.json', ['cases', 'Jpeg8AsyncNonSeekable', -1], 'WARM-JPEG-ASYNC-ENCODE.md', verification='Prior production; reconfirm'),
        row('PNG EncodePngAsync, Optimal managed9/native6 / prior timing', 'warm-png-async-encode-evidence.json', ['cases', 'Png8AsyncNonSeekable', -1], 'PNG-OPTIMAL-RGBA8.md', verification='Prior production; reconfirm'),
        row('PNG RGBA8 Optimal6/None', 'warm-current-encoder-costs-evidence.json', ['cases', 'png', -1], 'WARM-CURRENT-ENCODER-COSTS.md', verification='Gap (short; qualified)'),
        row('PNG RGBA8, NoCompression / None', 'warm-png-stored-capacity-evidence.json', ['cases', 'EncodeNoCompression8', -1], 'WARM-PNG-STORED-CAPACITY.md', verification='Prior production; reconfirm'),
        row('PNG RGBA16, NoCompression / None', 'warm-png-stored-capacity-evidence.json', ['cases', 'EncodeNoCompression16', -1], 'WARM-PNG-STORED-CAPACITY.md', verification='Prior production; reconfirm'),
        row('PNG RGBA8, Fastest managed2/native1 / None', 'warm-png-chunk-crc-evidence.json', ['cases', 'EncodeFast8', -1], 'WARM-PNG-CHUNK-CRC.md', verification='Prior production; reconfirm'),
        row('PNG RGBA16, Fastest managed2/native1 / None', 'warm-png-chunk-crc-evidence.json', ['cases', 'EncodeFast16', -1], 'WARM-PNG-CHUNK-CRC.md', verification='Prior production; reconfirm'),
        row('PNG RGBA16, Fastest managed2/native1 / live cancelable token', 'warm-png-chunk-crc-evidence.json', ['cases', 'EncodeFastCancelable16', -1], 'WARM-PNG-CHUNK-CRC.md', verification='Prior production; reconfirm'),
        row('PNG RGBA16, Fastest managed2/native1 / forward-only output', 'warm-png-chunk-crc-evidence.json', ['cases', 'EncodeFastForward16', -1], 'WARM-PNG-CHUNK-CRC.md', verification='Prior production; reconfirm'),
        row('PNG RGBA16, level 6 / None', 'warm-png-chunk-crc-evidence.json', ['cases', 'Encode16', -1], 'WARM-PNG-CHUNK-CRC.md', verification='Prior production; reconfirm'),
        row('PNG RGBA8, SmallestSize9 / native9', 'warm-png-chunk-crc-evidence.json', ['cases', 'EncodeSmallest8', -1], 'WARM-PNG-CHUNK-CRC.md', verification='Prior production; reconfirm'),
        row('PNG RGBA16, SmallestSize managed9/native9 / None', 'warm-png-chunk-crc-evidence.json', ['cases', 'EncodeSmallest16', -1], 'WARM-PNG-CHUNK-CRC.md', verification='Prior production; reconfirm'),
        row('PNG RGB8, Optimal managed9/native6 / None', 'warm-png-byte-packing-evidence.json', ['cases', 'EncodeRgb8', -1], 'WARM-PNG-BYTE-PACKING.md', verification='Prior production; reconfirm'),
        row('PNG RGB16, level 6 / None', 'warm-png-chunk-crc-evidence.json', ['cases', 'EncodeRgb16', -1], 'WARM-PNG-CHUNK-CRC.md', verification='Prior production; reconfirm'),
        row('PNG gray8, Optimal managed9/native6 / None', 'warm-png-byte-packing-evidence.json', ['cases', 'EncodeGray8', -1], 'WARM-PNG-BYTE-PACKING.md', verification='Prior production; reconfirm'),
        row('PNG gray16, level 6 / None', 'warm-png-chunk-crc-evidence.json', ['cases', 'EncodeGray16', -1], 'WARM-PNG-CHUNK-CRC.md', verification='Prior production; reconfirm'),
        row('PNG gray-alpha8, Optimal managed9/native6 / None', 'warm-png-byte-packing-evidence.json', ['cases', 'EncodeGrayAlpha8', -1], 'WARM-PNG-BYTE-PACKING.md', verification='Prior production; reconfirm'),
        row('PNG gray-alpha16, level 6 / None', 'warm-png-chunk-crc-evidence.json', ['cases', 'EncodeGrayAlpha16', -1], 'WARM-PNG-CHUNK-CRC.md', verification='Prior production; reconfirm'),
        row('JPEG 4:4:4, adaptive Huffman / matched quality', 'warm-jpeg-vector-ac-evidence.json', ['cases', 'Encode8', -1], 'WARM-JPEG-VECTOR-AC.md', verification='Prior production; reconfirm'),
        row('JPEG 4:4:4, forward-only / coded source histograms', 'jpeg444-coded-source-histogram-evidence.json', ['cases', 'Encode444Stream8', -1], 'JPEG444-CODED-SOURCE-HISTOGRAMS.md', verification='Cost screen (CPU parity deferred)'),
        row('JPEG 4:4:4, live cancelable token / prior geometric Huffman', 'warm-jpeg-fixed-fields-evidence.json', ['cases', 'Encode444Cancelable8', -1], 'WARM-JPEG-FIXED-FIELDS.md', verification='Prior production; reconfirm'),
        row('JPEG 4:4:4 from RGBA16, adaptive Huffman / full normalization', 'warm-jpeg-vector-ac-evidence.json', ['cases', 'Encode16', -1], 'WARM-JPEG-VECTOR-AC.md', verification='Prior production; reconfirm'),
        row('JPEG 4:2:2, adaptive Huffman / matched quality', 'warm-jpeg-vector-ac-evidence.json', ['cases', 'Encode422_8', -1], 'WARM-JPEG-VECTOR-AC.md', verification='Prior production; reconfirm'),
        row('JPEG 4:2:2 from RGBA16, adaptive Huffman / full normalization', 'warm-jpeg-vector-ac-evidence.json', ['cases', 'Encode422_16', -1], 'WARM-JPEG-VECTOR-AC.md', verification='Prior production; reconfirm'),
        row('JPEG RGBA8 4:2:0', 'warm-jpeg-separable-quantization-evidence.json', ['observations', -1], 'WARM-JPEG-SEPARABLE-QUANTIZATION.md', confirmed=True),
        row('JPEG 4:2:0, shared budget / matched quality', 'warm-shared-budget-evidence.json', ['cases', 'JpegBudgetEncode4208', -1], 'WARM-SHARED-BUDGET.md', verification='Prior production; reconfirm'),
        row('JPEG 4:2:0, adaptive / row restart markers', 'warm-jpeg-vector-ac-evidence.json', ['cases', 'Encode420Restart8', -1], 'WARM-JPEG-VECTOR-AC.md', verification='Prior production; reconfirm'),
        row('JPEG 4:2:0, forward-only output / geometric AC Huffman', 'warm-jpeg-fixed-fields-evidence.json', ['cases', 'Encode420Stream8', -1], 'WARM-JPEG-FIXED-FIELDS.md', verification='Prior production; reconfirm'),
        row('JPEG 4:2:0 from RGBA16, forward-only output', 'warm-jpeg-fixed-fields-evidence.json', ['cases', 'Encode420Stream16', -1], 'WARM-JPEG-FIXED-FIELDS.md', verification='Prior production; reconfirm'),
        row('JPEG 4:2:0 from RGBA16, live cancelable token', 'warm-jpeg-fixed-fields-evidence.json', ['cases', 'Encode420Cancelable16', -1], 'WARM-JPEG-FIXED-FIELDS.md', verification='Prior production; reconfirm'),
        row('JPEG 4:2:0 from RGBA16, adaptive Huffman', 'warm-jpeg-separable-quantization-evidence.json', ['currentRgba16Cost', 'observations', -1], 'WARM-JPEG-SEPARABLE-QUANTIZATION.md', verification='Gap (short; qualified)'),
        row('JPEG grayscale, standalone SIMD libjpeg-turbo', 'warm-jpeg-vector-ac-evidence.json', ['cases', 'EncodeGray8', -1], 'WARM-JPEG-VECTOR-AC.md', True),
        row('JPEG from caller gray samples, standalone SIMD libjpeg-turbo', 'warm-jpeg-compact-color-evidence.json', ['cases', 'OriginalGray', -1], 'WARM-JPEG-COMPACT-COLOR.md', verification='Gap (short; qualified)'),
        row('JPEG 4:2:0 from caller RGB / optional CompactColor', 'warm-jpeg-compact-color-evidence.json', ['cases', 'CompactColorRgb420', -1], 'WARM-JPEG-COMPACT-COLOR.md', verification='Gap (short; qualified)'),
        row('JPEG 4:2:0 from caller RGB / AC trellis and padding pruning', 'jpeg420-ac-trellis-evidence.json', ['cases', 'Encode420_8', -1], 'JPEG420-AC-TRELLIS.md', verification='Cost screen (CPU parity deferred)'),
        row('WebP VP8, opaque‡', 'vp8-noise-spectrum-evidence.json', ['observations', 0], 'WARM-VP8-NOISE-SPECTRUM.md', verification='Cost screen (CPU parity deferred)'),
        row('WebP VP8, opaque from RGBA16‡', 'vp8-noise-spectrum-evidence.json', ['observations', 1], 'WARM-VP8-NOISE-SPECTRUM.md', verification='Cost screen (CPU parity deferred)'),
        row('VP8 RGBA8', 'warm-current-encoder-costs-evidence.json', ['cases', 'vp8', -1], 'WARM-CURRENT-ENCODER-COSTS.md', verification='Gap (short; qualified)'),
        row('WebP VP8 + alpha from RGBA16‡', 'vp8-noise-spectrum-evidence.json', ['observations', 3], 'WARM-VP8-NOISE-SPECTRUM.md', verification='Cost screen (CPU parity deferred)'),
        row('WebP VP8 + alpha‡, caller work budget32M', 'vp8-histogram384-evidence.json', ['observations', 0], 'WARM-VP8-HISTOGRAM384.md', verification='Cost screen (CPU parity deferred)'),
        row('VP8L RGBA8 / libwebp exact=1', 'warm-current-encoder-costs-evidence.json', ['cases', 'vp8l', -1], 'WARM-CURRENT-ENCODER-COSTS.md', verification='Gap (short; qualified)'),
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
        row('Bicubic RGBA8, 128×64 / input+output+4 KiB limit§', 'warm-bicubic-four-columns-evidence.json', ['cases', 'TightWide8', -1], 'WARM-BICUBIC-FOUR-COLUMNS.md', verification='Gap (short; qualified)'),
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
    labels = [value.split('](', 1)[0] for value in all_rows]
    assert len(labels) == len(set(labels)), 'Duplicate benchmark operation labels hide distinct evidence.'
    assert sum('; metadata timing open' in value for value in all_rows) == 30
    producer_rows = [value.replace('; metadata timing open', '') for value in all_rows]
    parity_count = sum(' | Parity confirmed |' in value for value in producer_rows)
    confidence_count = sum(' | Confidence open |' in value for value in producer_rows)
    gap_count = sum(' | Gap' in value for value in producer_rows)
    reconfirm_count = sum(' | Prior production; reconfirm |' in value for value in producer_rows)
    cost_count = sum(' | Cost screen (CPU parity deferred) |' in value for value in producer_rows)
    assert parity_count + confidence_count + gap_count + reconfirm_count + cost_count == 239
    return f'''# Lokad.Image benchmark state

Updated 2026-10-24. **Declared encoder and decoder byte controls are qualified**
against pinned FFmpeg builds/settings; warm single-CPU parity remains open.
Only explicitly approved encoder-attribution metadata may differ; see
[codec compatibility](codec-parity.md).

The default managed JPEG Reference encoder matches ten frozen packets across
grayscale/444/422/420,8/16-bit sources and declared color/grayscale restarts.
Color uses only the approved COM replacement; grayscale is raw-exact. Required
image/caller/sync/async/ImageBudget boundaries are qualified. Superseded
Original/CompactColor experiments and their public switches have been removed.

All four production [managed PNG policies](../benchmarks/PNG-MANAGED-POLICIES.md)
match14 frozen complete packets raw-exact: all eight Optimal color/depth controls
plus RGBA8/16 stored, fast and smallest. The17 existing delivery/profile rows
are exact, including checksums/framing. Both platforms pass4,807 Release checks,
69 affected PNG scalar checks and the SAME actual private0.2.0 package with all
three examples. Four current RGBA8 encoder anchors have guarded packet-exact cost
screens. JPEG has independently confirmed improvements from bounded MCU conversion,
integer SIMD transforms/quantization and separable chroma-row reuse. Its4.97× gap
remains open. The current change passes5,047 checks and the same actual private
package on each platform. See [JPEG evidence](../benchmarks/WARM-JPEG-SEPARABLE-QUANTIZATION.md).
An existing same-build RGBA16 JPEG420 screen shows a6.21× gap, with complete
packet equality and full16-bit normalization inside the operation.
Other replaced encoder observations remain historical.
All17 historical BCL PNG timing rows below are superseded, as are the earlier
JPEG encoder producers. No new timing or image/settings controls were added.

The production VP8 pipeline now matches20 pinned complete packet identities
across22 source/settings rows, including all efforts and default-method8/16-bit
normalization. Photograph/diagram packets are exactly17,688/7,100B at the pinned
Q75/method4 settings. The separate large-header control also passes. Shared
admission, exact normalized alpha, stream ownership and direct async delivery
are qualified; [VP8 scope](../benchmarks/VP8-INPUT-STAGE.md) records details.
All seven prior VP8 time/allocation rows are superseded. In particular, the old
32M-work timing describes a success on the prior algorithm; current encoding
must fail that resource boundary, so it is not a new performance pass. No CPU
measurement or retuning accompanies byte qualification.

The managed [VP8L encoder](../benchmarks/VP8L-COMPLETE-PACKET.md) now reproduces
all19 selected packet identities through25 existing settings checks, plus two
additional forced spatial/box candidates (21 distinct complete packets). Public sync/async/ImageBudget
boundaries retain the asymmetric packet at8/16bits. The photograph/diagram controls
match160,032/7,234B exactly. Source-only proofs are retained as earlier evidence;
public production no longer uses the superseded full-image encoder.
Prepared transforms/tokens/maps/canonical codes share one borrowed admission root.
One normalized working image releases before emission; counting/write reuse prepared
state without an encoded-frame buffer. Refusal/cancellation and output ownership
pass. Both platforms pass1,855 internal+2,070 public+940 live FFmpeg checks and27
scalar public/axis checks, with one immutable worker set. A test-only extension
against the unchanged core passes26 internal/27 public packet checks and27 scalar
packet checks, including the two16383 axes. The SAME private Release
package and all3usage examples pass on both. The native encoder refuses16384;
those axis cases remain functional extensions without an encoder-byte claim.
Six affected encoder time/allocation rows below describe the prior producer and
require new costs later. Frozen decoder inputs and unchanged code retain evidence.

Current JPEG decoding uses the pinned integer inverse and rounded-chroma color
policy. Thirty frozen grayscale/4:4:4/4:2:2/4:2:0 controls and the declared
public stream/caller/orientation boundaries have exact pixel gates. Named CMYK/YCCK
controls also require exact RGB/grayscale, including black-before-color precision.
Named4:1:1/4:1:0 controls retain fractional chroma through final RGB rounding.
Their historical timing workers include additional centered normalization; those
unchanged references remain historical and need equivalent current outputs before
new timing qualification. No baseline or performance pass is inferred from the
new byte checks on the same frozen513x259 packets.
Direct-RGB422/420 caller grayscale shares integer BT.601 precision after normalized
8-bit components; the existing RGBA/RGB reference settings and hashes remain.
Earlier JPEG
decode timings and allocations describe the replaced producer and are marked
**Prior production; reconfirm**. No new decoder timing/allocation run is claimed.

VP8 decoder RGB now uses the exact integer normalization on164 frozen controls.
One current fresh-Stream screen uses the same98,590-byte input, exact normalized
pixels and unchanged native baseline. Both quiet guards pass; it measures
10,523.95µs versus5,981.68µs,1,966,591B/op. This is a short1.759× gap observation,
with no independent confirmation or speedup claim. The other four VP8 decode
rows still describe the replaced renderer.

The initial PNG/JPEG/VP8L encoder adoption had no timing/allocation runs; four existing anchors now have current guarded screens. Unchanged paths
retain evidence on their recorded frozen inputs; new default-encoded fixtures do
not silently replace those inputs. CPU tuning/independent confirmation resumes
after exact codec work. The full warm single-CPU objective remains independent
conservative upper<=1 for every material required path. Published0.1.0 is immutable;
replacement readiness remains open.

The [finite checklist](../benchmarks/WARM-COVERAGE.md) remains 256 named cases:
239 numeric and 17 unmeasured. Tested-producer rows contain **{parity_count} passes,
{gap_count} gaps, {confidence_count} confidence-open case, {cost_count} short cost
screens and {reconfirm_count} superseded encoder rows**. The latter predate
JPEG decision/adaptive work or the managed PNG/VP8L encoder replacements; current packet evidence is
reconciled and new costs are deferred while compression gates remain open.
Their stored values are historical, not demonstrated current gaps. Another 30 marked
decode/inspection rows await confirmation after typed source-metadata changes;
the PNG metadata reference first needs equivalent typed fields. Unaffected evidence
is reused; the scope of every tested producer remains in its linked report.

Dedicated AMD EPYC 9V45; Ubuntu 24.04.4; .NET 10.0.8/SDK 10.0.204; FFmpeg n8.1.
One pinned CPU and codec/conversion/filter thread, normal SIMD, tiering off,
workstation nonconcurrent GC. Times cover complete public operations, admission,
normalization, fresh output ownership and release. Source preparation, startup,
IPC, I/O, hashing and validation are excluded. Fresh-stream rows include wrapper,
read/copy/EOF and release costs. Context/private scratch reuse is allowed;
completed decoded or encoded outputs are not cached.

Ratio uses the faster native pre/post mean. Upper is the larger selected/all-actual
bound against the faster native bracket's lower confidence bound. Independent
jobs retain 12 actuals; short screens retain 3, with both idle-v2 guards. **B/op** is
managed BenchmarkDotNet/raw-GC allocation rounded up. Native allocation is
**unmeasured**, not zero. Compression gains may cost more CPU; no cost-only screen
is a CPU-parity confirmation.

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

‡ Historical procedural VP8 timings use managed/native quality100/75 and
matched-source tolerances. The replacement encoder instead uses the pinned native
policy and complete-packet equality; those times/allocations do not qualify it.
The historical32M successful call is now a required terminal Work failure.


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

The historical BCL PNG timing rows used managed9/native6 for Optimal,2/1 for
Fastest and9/9 for SmallestSize, with a5% IDAT timing-equivalence gate.
[Their packet audit](../benchmarks/PNG-COMPLETE-PACKET-AUDIT.md) found15 passes
and two open level-6 RGBA8 boundaries. These are superseded producer observations:
the current managed encoder instead requires all14 declared complete packets
to match exactly. A timing pass or IDAT tolerance establishes no packet-size pass.
Packed-gray
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
