"""Render current benchmarks from retained evidence; never measure."""
import json
import math
import re
import sys
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]


def render():
    evidence = json.loads((ROOT / 'benchmarks/release-0.2.0-benchmark-evidence.json').read_text(encoding='utf-8'))
    assert evidence['version'] == '0.2.0' and len(evidence['cases']) == 31
    assert evidence['allDisplayedCasesRefreshed'] and not evidence['productionChanged']
    stored = json.loads((ROOT / 'benchmarks/warm-png-bulk-stored-evidence.json').read_text(encoding='utf-8'))
    current = json.loads((ROOT / 'benchmarks/warm-png-distance-symbol-evidence.json').read_text(encoding='utf-8'))
    vp8 = json.loads((ROOT / 'benchmarks/warm-vp8-simd-input-evidence.json').read_text(encoding='utf-8'))
    trellis = json.loads((ROOT / 'benchmarks/current-vp8-trellis-encoding-evidence.json').read_text(encoding='utf-8'))
    photograph = json.loads((ROOT / 'benchmarks/current-vp8-photograph-encoding-evidence.json').read_text(encoding='utf-8'))
    alpha = json.loads((ROOT / 'benchmarks/current-vp8-alpha8-encoding-evidence.json').read_text(encoding='utf-8'))
    jpeg = json.loads((ROOT / 'benchmarks/warm-jpeg-horizontal-area-filter-evidence.json').read_text(encoding='utf-8'))
    vp8l = json.loads((ROOT / 'benchmarks/warm-vp8l-prepared-prediction-cost-evidence.json').read_text(encoding='utf-8'))
    reads = json.loads((ROOT / 'benchmarks/warm-image-read-evidence.json').read_text(encoding='utf-8'))
    probes = json.loads((ROOT / 'benchmarks/warm-probe-completed-reads-evidence.json').read_text(encoding='utf-8'))
    png_validation = json.loads((ROOT / 'benchmarks/png-validation-current-evidence.json').read_text(encoding='utf-8'))
    png8_validation = json.loads((ROOT / 'benchmarks/current-png8-validation-evidence.json').read_text(encoding='utf-8'))
    streamed = json.loads((ROOT / 'benchmarks/warm-png-streaming-validation-evidence.json').read_text(encoding='utf-8'))
    reconciled = json.loads((ROOT / 'benchmarks/warm-read-reconciliation-evidence.json').read_text(encoding='utf-8'))
    resident = json.loads((ROOT / 'benchmarks/current-png-resident-identify-evidence.json').read_text(encoding='utf-8'))
    resident_other = json.loads((ROOT / 'benchmarks/current-jpeg-webp-resident-identify-evidence.json').read_text(encoding='utf-8'))
    stream_identify = json.loads((ROOT / 'benchmarks/current-png-stream-identify-evidence.json').read_text(encoding='utf-8'))
    decoders = json.loads((ROOT / 'benchmarks/current-default-decoders-evidence.json').read_text(encoding='utf-8'))
    resident_decode = json.loads((ROOT / 'benchmarks/current-vp8l-resident-decode-evidence.json').read_text(encoding='utf-8'))
    jpeg_decode = json.loads((ROOT / 'benchmarks/warm-jpeg-paired-columns-evidence.json').read_text(encoding='utf-8'))
    assert jpeg_decode['productionChanged'] and jpeg_decode['independentQuietConfirmationThisPair']
    assert not decoders['productionChanged'] and len(decoders['cases']) == 3
    assert not resident_decode['productionChanged'] and resident_decode['independentQuietConfirmation']
    assert not reconciled['productionChanged'] and len(reconciled['cases']) == 6
    assert not resident['productionChanged'] and resident['status']['PngResident'] == 'screen-gap'
    assert not resident_other['productionChanged'] and resident_other['independentQuietConfirmation']
    assert not stream_identify['productionChanged'] and len(stream_identify['cases']) == 2
    assert not reads['productionChanged'] and len(reads['cases']) == 12
    assert probes['productionChanged'] and len(probes['cases']) == 4
    assert not png_validation['productionChanged'] and png_validation['sourceAndQualifiedProducersRestored']
    assert not png8_validation['productionChanged'] and png8_validation['independentQuietConfirmation']
    assert streamed['productionChanged'] and len(streamed['cases']) == 4
    assert vp8['productionChanged'] and vp8['independentQuietConfirmationThisPair']
    assert not trellis['productionChanged'] and trellis['independentQuietConfirmation']
    assert not photograph['productionChanged'] and photograph['independentQuietConfirmation']
    assert not alpha['productionChanged'] and alpha['independentQuietConfirmation']
    assert jpeg['productionChanged'] and jpeg['independentQuietConfirmationThisPair']
    assert vp8l['version'] == '0.2.1' and vp8l['independentQuietConfirmationThisPair']
    assert current['version'] == '0.2.1' and current['independentQuietConfirmationThisPair']
    assert set(current['cases']) == {'EncodeFast8', 'EncodeSmallest8', 'EncodeSmallest16'}
    vp8_cases = {'case06': 'DiagramLossyDistortion8', 'case07': 'Alpha8',
                 'case08': 'PhotographLossy8', 'case09': 'DiagramLossy8',
                 'case10': 'DiagramLossyTrellis8', 'case11': 'Opaque16'}
    rows = []
    for case in evidence['cases']:
        observation = case['observation']
        vp8l_case = case['id'] in {'case12', 'case13', 'case14', 'case15'}
        vp8_case = case['id'] in vp8_cases
        updated = vp8_case or vp8l_case or case['id'] in {'case00', 'case02', 'case03', 'case17', 'case18', 'case19', 'case20'}
        producer = trellis if case['id'] == 'case10' else photograph if case['id'] == 'case08' else alpha if case['id'] == 'case07' else vp8 if vp8_case else jpeg if case['id'] in {'case02', 'case03'} else reconciled if case['id'] == 'case00' else vp8l if vp8l_case else stored if case['id'] == 'case20' else current
        if vp8_case:
            observation = producer['cases'][vp8_cases[case['id']]][-1]
        elif case['id'] == 'case00':
            observation = producer['cases']['Vp8Stream'][-1]
        elif case['id'] in {'case02', 'case03'}:
            observation = jpeg['cases']['Rgb420' if case['id'] == 'case02' else 'Rgba16'][-1]
        elif updated:
            observation = producer['cases'][observation['receipt']['caseId']][-1]
        receipt = observation['receipt']
        stats = observation['managed']['statistics']
        native = min(b['mean'] for b in observation['nativeBrackets'])
        ratio = observation['worstBracketRatio']
        upper = observation['conservativeRatioUpper']
        assert receipt['librarySha256'] == (producer['producers']['linuxSha256'] if updated else evidence['librarySha256'])
        assert all(g['passed'] for g in observation['quietness'].values())
        samples = 12 if case['id'] in {'case02', 'case06', 'case07', 'case08', 'case10', 'case12'} or updated and receipt['caseId'] in {'EncodeNoCompression16', 'EncodeFast8'} else 3
        assert (8 if samples == 12 else 2) <= stats['N'] <= samples
        assert len(receipt['nativeBefore']) == len(receipt['nativeAfter']) == samples
        assert math.isclose(stats['Mean'] / native, ratio, rel_tol=1e-12)
        allocated = observation['managedMemory']['BytesAllocatedPerOperation']
        label = case['label'] + (' ‡' if case['id'] == 'case20' else ' †' if updated else '')
        if case['kind'] == 'editing':
            label += f' → {receipt["outputWidth"]}×{receipt["outputHeight"]}'
        cells = [label, f'{stats["Mean"] / 1000:,.2f}', f'{native / 1000:,.2f}',
                 f'{ratio:.3f} / {upper:.3f}', f'{allocated:,}']
        if case['kind'] == 'codec':
            packet = f'{receipt["managedOutputBytes"]:,} / {receipt["nativeOutputBytes"]:,}' if case['operation'] == 'encode' else '—'
            cells.append(packet)
        rows.append((case['kind'], '| ' + ' | '.join(cells) + ' |'))
    for case, label in [('Jpeg420', 'JPEG baseline420, resident decode'),
                        ('WebpResident', 'VP8L RGBA8, resident decode'),
                        ('WebpStream', 'VP8L RGBA8, fresh seekable Stream decode')]:
        producer = jpeg_decode if case == 'Jpeg420' else resident_decode if case == 'WebpResident' else decoders
        observation = producer['cases'][case][-1]
        receipt, stats = observation['receipt'], observation['managed']['statistics']
        samples = 12 if case in {'Jpeg420', 'WebpResident'} else 3
        assert producer['status'][case] == ('confirmed-gap' if samples == 12 else 'screen-gap')
        assert len(observation['managed']['actual']) == samples
        assert receipt['librarySha256'] == producer['producers']['linuxSha256']
        assert receipt['managedOutputSha256'] == receipt['nativeOutputSha256']
        assert all(q['passed'] for q in observation['quietness'].values())
        assert len(receipt['nativeBefore']) == len(receipt['nativeAfter']) == samples
        native = min(b['mean'] for b in observation['nativeBrackets'])
        assert math.isclose(stats['Mean'] / native, observation['worstBracketRatio'], rel_tol=1e-12)
        rows.append(('codec', f'| {label} † | {stats["Mean"] / 1000:,.2f} | {native / 1000:,.2f} | '
                     f'{observation["worstBracketRatio"]:.3f} / {observation["conservativeRatioUpper"]:.3f} | '
                     f'{observation["managedMemory"]["BytesAllocatedPerOperation"]:,} | — |'))
    header = '| Operation / fixed control | Lokad µs | Reference µs | Ratio / upper | Lokad allocated B/op |'
    separator = '| --- | ---: | ---: | ---: | ---: |'
    codecs = '\n'.join([header + ' Packet B, Lokad / reference |', separator + ' ---: |',
                         *(row for kind, row in rows if kind == 'codec')])
    editing = '\n'.join([header, separator, *(row for kind, row in rows if kind == 'editing')])
    read_labels = ['Probe PNG dimensions, seekable', 'Probe JPEG dimensions, seekable',
                   'Probe WebP dimensions, seekable', 'Probe JPEG dimensions, async nonseekable',
                   'Validate PNG8, span', 'Validate PNG16, span', 'Validate indexed Adam7 19×17, span',
                   'Validate JPEG baseline420, span', 'Validate JPEG progressive420, span',
                   'Validate VP8L+alpha, span', 'Validate VP8+alpha, span',
                   'Validate PNG16, async nonseekable']
    read_rows = []
    for label, (case, observations) in zip(read_labels, reads['cases'].items(), strict=True):
        producer = png8_validation if case == 'ValidatePng8' else probes if case in probes['cases'] else streamed if case in streamed['cases'] else reads
        observations = producer['cases'][case]
        observation = observations[-1]
        receipt = observation['receipt']
        assert receipt['librarySha256'] == producer['producers']['linuxSha256']
        assert receipt['descriptorExact'] and receipt['prefixAndCodingScopesVerified']
        assert all(q['passed'] for q in observation['quietness'].values())
        confirmed = producer['status'][case].startswith('confirmed-')
        samples = 12 if confirmed else 3
        assert len(receipt['nativeBefore']) == len(receipt['nativeAfter']) == samples
        assert len(observation['managed']['actual']) == samples
        native = min(b['mean'] for b in observation['nativeBrackets'])
        stats = observation['managed']['statistics']
        ratio, upper = observation['worstBracketRatio'], observation['conservativeRatioUpper']
        assert math.isclose(stats['Mean'] / native, ratio, rel_tol=1e-12)
        assert (upper <= 1) == (producer['status'][case] == 'confirmed-pass')
        status = 'Confirmed pass' if upper <= 1 else 'Confirmed gap' if confirmed else 'Screen gap'
        read_rows.append(f'| {label} | {stats["Mean"] / 1000:,.3f} | {native / 1000:,.3f} | '
                         f'{ratio:.3f} / {upper:.3f} | {observation["managedMemory"]["BytesAllocatedPerOperation"]:,} | {status} |')
    headers = reconciled['cases']['PngHeaders'][-1]
    assert headers['receipt']['descriptorExact'] and headers['receipt']['typedSourceMetadataExact']
    assert headers['receipt']['librarySha256'] == reconciled['producers']['linuxSha256']
    assert len(headers['managed']['actual']) == 3 and all(q['passed'] for q in headers['quietness'].values())
    read_rows.append(f'| Inspect PNG8 complete headers, seekable, adapter reference | '
                     f'{headers["managed"]["statistics"]["Mean"] / 1000:,.3f} | '
                     f'{min(b["mean"] for b in headers["nativeBrackets"]) / 1000:,.3f} | '
                     f'{headers["worstBracketRatio"]:.3f} / {headers["conservativeRatioUpper"]:.3f} | '
                     f'{headers["managedMemory"]["BytesAllocatedPerOperation"]:,} | Screen gap |')
    identified = resident['cases']['PngResident'][-1]
    assert identified['receipt']['descriptorExact'] and identified['receipt']['typedSourceMetadataExact']
    assert identified['receipt']['descriptorFields'] == 9 and identified['receipt']['nativeReady']['outputBytes'] == 36
    assert identified['receipt']['librarySha256'] == resident['producers']['linuxSha256']
    assert len(identified['managed']['actual']) == 3 and all(q['passed'] for q in identified['quietness'].values())
    read_rows.append(f'| Identify PNG8 complete container, resident, adapter reference | '
                     f'{identified["managed"]["statistics"]["Mean"] / 1000:,.3f} | '
                     f'{min(b["mean"] for b in identified["nativeBrackets"]) / 1000:,.3f} | '
                     f'{identified["worstBracketRatio"]:.3f} / {identified["conservativeRatioUpper"]:.3f} | '
                     f'{identified["managedMemory"]["BytesAllocatedPerOperation"]:,} | Screen gap |')
    for case, label in [('Jpeg8', 'Identify JPEG420, resident, canonical adapter'),
                        ('Webp8', 'Identify VP8L+alpha, resident, parser adapter')]:
        identified = resident_other['cases'][case][-1]
        assert resident_other['status'][case] == 'confirmed-pass' and identified['conservativeRatioUpper'] <= 1
        assert identified['receipt']['typedSourceMetadataExact'] and identified['receipt']['descriptorFields'] == 9
        assert identified['receipt']['librarySha256'] == resident_other['producers']['linuxSha256']
        assert len(identified['managed']['actual']) == 12 and all(q['passed'] for q in identified['quietness'].values())
        read_rows.append(f'| {label} | {identified["managed"]["statistics"]["Mean"] / 1000:,.3f} | '
                         f'{min(b["mean"] for b in identified["nativeBrackets"]) / 1000:,.3f} | '
                         f'{identified["worstBracketRatio"]:.3f} / {identified["conservativeRatioUpper"]:.3f} | '
                         f'{identified["managedMemory"]["BytesAllocatedPerOperation"]:,} | Confirmed pass |')
    for case, label, samples in [('PngSeekable', 'Identify PNG8, fresh seekable Stream, adapter reference', 3),
                                  ('PngAsync', 'Identify PNG8, async nonseekable, adapter reference', 12)]:
        identified = stream_identify['cases'][case][-1]
        assert identified['receipt']['typedSourceMetadataExact'] and identified['receipt']['descriptorFields'] == 9
        assert identified['receipt']['librarySha256'] == stream_identify['producers']['linuxSha256']
        assert len(identified['managed']['actual']) == samples and all(q['passed'] for q in identified['quietness'].values())
        status = 'Confirmed gap' if samples == 12 else 'Screen gap'
        read_rows.append(f'| {label} | {identified["managed"]["statistics"]["Mean"] / 1000:,.3f} | '
                         f'{min(b["mean"] for b in identified["nativeBrackets"]) / 1000:,.3f} | '
                         f'{identified["worstBracketRatio"]:.3f} / {identified["conservativeRatioUpper"]:.3f} | '
                         f'{identified["managedMemory"]["BytesAllocatedPerOperation"]:,} | {status} |')
    read_table = '\n'.join([header + ' Status |', separator + ' --- |', *read_rows])
    return f'''# Lokad.Image benchmarks

Updated 2026-10-28. **† Rows** use qualified 0.2.1 development-path evidence;
**‡ stored PNG** retains its unchanged 0.2.1 proof. Fourteen other controls retain
published 0.2.0 measurements on unchanged paths. VP8 Stream decode now has a
current-cost screen. The release snapshot remains immutable. Eighteen read
controls below use qualified 0.2.1 builds. Default JPEG420 decode has independent
current evidence; VP8L resident decode is independently confirmed and Stream retains a current screen. These 52 displayed controls do not
complete the full CPU-parity objective
or establish general replacement readiness.

Measurements use the dedicated quiet Ubuntu 24.04 x64 VM
({evidence['machine']['cpuModel']}): one pinned CPU per operation, warm .NET 10,
sequential Release execution, no concurrency. This representative scope is not
the full performance objective.

Stored PNG16, Fastest PNG8, caller-RGB420 JPEG, JPEG420/VP8L resident decode, VP8 efforts2/6, VP8 photograph/alpha and VP8L RGBA8 encode have 12 measured iterations each;
other codec/editing rows are short BenchmarkDotNet screens with three. Native brackets and
idle/process guards pass. Shared-history predictor pricing reduces VP8L RGBA8
elapsed by 7.4% against a fresh independent baseline, preserving exact packets.
It remains a1.332× / upper1.360 gap. Allocation rises by8,224B/op for one reusable
8KiB analysis buffer; RGBA16/async/photograph have current screens.
Horizontal JPEG chroma filtering independently reduces caller-RGB420 elapsed by
16.1%, retaining373,416B/op and the approved exact packet boundary. Its1.283× /
upper1.294 gap remains; governed RGBA16 has a current1.224× screen.
SIMD input preparation reduces VP8 effort2 elapsed by13.2% using all actuals in
the final independent comparison (14.3% filtered);1.345× / upper1.423 remains open,
with741,375B/op and exact packets. An earlier independent pair gives5.4% with
overlapping intervals; both pairs and the material native drift remain recorded.
VP8 effort6 independently confirms a1.539× / upper1.551 gap,754,344B/op and exact
6,818B packet. VP8 photograph confirms 1.467× / upper 1.501, 809,072 B/op and an exact
17,688-byte packet. VP8 alpha confirms 1.296× / upper 1.306, 21,353,208 B/op and
an exact 98,590-byte packet. These follow-ups reuse the immutable build and package
proof. Two other VP8 companions retain wide screens, without gain/regression claims.
BothOS pass5055checks and the same actual package/three examples; no owned storage is added.
Ratio uses the faster native bracket; upper is the conservative confidence bound
including unfiltered managed samples. Short screens are not new independent
parity confirmations. B/op measures managed allocation, not native
allocation, peak live memory or cooperative-admission charges.

Operations include normalization, input delivery and independently owned output;
async encoding includes staging/delivery/release. Fixture preparation is excluded.
Source images are 513×259; resize rows state output dimensions. The
[release evidence](../benchmarks/release-0.2.0-benchmark-evidence.json) and
[stored publication](../benchmarks/WARM-PNG-BULK-STORED.md) and
[distance mapping](../benchmarks/WARM-PNG-DISTANCE-SYMBOL.md) record exact dimensions,
settings, boundaries, binaries, samples, package checks and retained raw archives.
[SIMD VP8 input preparation](../benchmarks/WARM-VP8-SIMD-INPUT.md) supplies all six
VP8 encoding rows, retaining earlier observations and uncertainty; the
[trellis follow-up](../benchmarks/current-vp8-trellis-encoding-evidence.json)
and [photograph follow-up](../benchmarks/current-vp8-photograph-encoding-evidence.json)
and [alpha follow-up](../benchmarks/current-vp8-alpha8-encoding-evidence.json)
resolve their wide screens without rebuilding or repeating qualification.
[shared-history predictor pricing](../benchmarks/WARM-VP8L-PREPARED-PREDICTION-COST.md) supplies the
four affected VP8L rows. Other paths retain applicable prior proof.
[JPEG area filtering](../benchmarks/WARM-JPEG-HORIZONTAL-AREA-FILTER.md) refreshes
caller RGB420 and governed RGBA16; other subsampled encoder costs need current
evidence before a pass claim. Grayscale/4:4:4 encoding paths are unchanged.
The [read reconciliation](../benchmarks/WARM-READ-RECONCILIATION.md) refreshes VP8
Stream decode and four existing tight PNG companions without a speedup claim.
The [default decoder reconciliation](../benchmarks/CURRENT-DEFAULT-DECODERS.md)
retains the VP8L Stream screen at1.304×,1,216,190B/op. A
[resident confirmation](../benchmarks/current-vp8l-resident-decode-evidence.json)
resolves its wide screen at1.333× / upper1.404,1,216,162B/op,with the same
immutable build and exact pixels;no gain/regression or general decode claim follows.
[Paired JPEG inverse columns](../benchmarks/WARM-JPEG-PAIRED-COLUMNS.md) reduce
JPEG420 decode from 1.331 ms to 1.298 ms: 2.5% less filtered elapsed and 2.4% using
all actuals, with separated intervals and unchanged 757,319 B/op. Exact pixels
remain; the 1.285× / upper 1.304 gap stays open. Both platforms pass 5,056 full
checks, affected scalar tests and the same actual package/three examples. Other
JPEG inverse-render costs need current evidence before pass claims; validation,
encoding and unrelated paths retain applicable proof. Packet/settings and owned
output boundaries stay fixed. Remaining wide VP8 companions need resolution
before tuning; PNG Fastest retains its applicable independent gap.


''' + codecs + '''

Packets pass existing complete-byte gates; color JPEG differs only by the approved
two-byte-smaller attribution COM. Lossless pixels, lossy reconstruction and alpha
gates remain enforced. [Codec parity](codec-parity.md) gives the broader frozen
control scope. These measurements make no arbitrary-content compression claim.

''' + editing + '''

Resize references use FFmpeg or its explicitly labeled zimg/zscale backend with
the approved halo, alpha and packing boundary. Managed pixels stay exact; native
rounding tolerances remain the frozen per-case bounds in the evidence. Tight
bicubic rows allow only input + output + 4 KiB local workspace.

References are pinned FFmpeg n8.1 and its libwebp 1.6.0 backend. Grayscale JPEG
uses the approved standalone **SIMD libjpeg-turbo 3.1.4.1** exception; lossless
WebP uses **libwebp exact=1** to preserve hidden RGB. Defaults/quality numbers
across encoders are not assumed equivalent. Startup, concurrency, GIF/animation
and unlisted content/settings are excluded.

''' + read_table + '''

Probes compare requested dimensions using FFmpeg AVIO/owned descriptors plus an
original bounded header adapter; they stop before pixel payloads. Validation
compares complete coding and source metadata against FFmpeg natural-frame decode
plus an original metadata adapter, without RGBA normalization or caller raster
copies. Async uses resident immediate callbacks and includes acquisition/EOF and
cleanup. Inputs are 513×259 except indexed Adam7. Baseline420, PNG16 span and async
validation pass independently; PNG8 and progressive420 remain confirmed gaps. Other
validation rows are short screens, without independent parity claims. The
[streaming PNG validator](../benchmarks/WARM-PNG-VALIDATION.md) reduces PNG16 elapsed
by42.4% and warm span allocation from17,008 to240B/op. Its repeat passes with a
narrow upper0.99966; the preceding upper1.00108 is retained in the evidence.
The [PNG8 follow-up](../benchmarks/current-png8-validation-evidence.json) confirms
1.349× / upper 1.354 and 240 B/op on the same immutable build and frozen packet,
without a gain claim. Its profile identifies streaming Deflate/history handling
as the next substantial lead.
JPEG probing has twelve-actual gap
confirmations; PNG/WebP probing remains screened. The
[workspace reduction](../benchmarks/WARM-PROBE-WORKSPACE.md) lowers all four probes
from 8,680 to 744 B/op; later-fact/header scans retain their larger workspace.
[Completed acquisition](../benchmarks/WARM-PROBE-COMPLETED-READS.md) independently
reduces JPEG probing elapsed by 46.5% sync / 50.6% async, with unchanged allocation.
See [read boundaries and raw evidence](../benchmarks/WARM-IMAGE-READ.md).
Complete PNG header inspection uses the explicitly labeled bounded adapter,
checking structure/Strict EXIF and all nine source fields through EOF while
skipping IDAT. CRC and pixels are opaque on both sides. Its 21 frozen RGBA8
controls pass; the timing remains a screen gap, distinct from prefix probing.
[Resident PNG Identify](../benchmarks/CURRENT-PNG-RESIDENT-IDENTIFY.md) checks the
complete 218,775B container, including CRC and all nine metadata fields, without
pixel decoding. Its 21 existing controls pass normal/scalar; the timing is a
current screen gap.
[Resident JPEG/WebP Identify](../benchmarks/CURRENT-JPEG-WEBP-RESIDENT-IDENTIFY.md)
passes independently with all nine metadata facts,96B/op each. JPEG retains the
explicit canonical-header adapter; WebP includes public-parser packet buffering
and EOF flush. These frozen packet results are not general codec speed claims.
[PNG stream Identify](../benchmarks/CURRENT-PNG-STREAM-IDENTIFY.md) retains complete
acquisition/EOF/ownership and all nine facts. Seekable is a screen gap; async has
one independent gap confirmation resolving its wide short interval. Existing
same-packet/delivery controls pass normal/scalar. Other default decoder boundaries
remain pending; these results do not imply a general read-API pass.

Read allocations use the default null shared budget and warm permitted private
pools. [Governed pixel-validation storage](../benchmarks/PIXEL-VALIDATION-STORAGE.md)
separately compares allocations/admitted peaks with decode-and-dispose on five
declared controls; these figures measure different ownership/accounting settings.

[Measurement instructions](../benchmarks/README.md) and the active
[finite coverage objective](../benchmarks/WARM-COVERAGE.md) retain reproducibility
and outstanding work. Rebuild this summary with
`python benchmarks/render-benchmark.py`; verify it with `--check`.
'''


if __name__ == '__main__':
    text = render()
    assert len(re.findall(r'^\| ---', text, re.M)) == 3
    output = ROOT / 'docs/benchmark.md'
    if '--check' in sys.argv:
        assert output.read_text(encoding='utf-8') == text, 'Benchmark summary differs from retained evidence.'
        print('Benchmark summary matches current and retained release evidence (three tables).')
    else:
        output.write_text(text, encoding='utf-8', newline='\n')
