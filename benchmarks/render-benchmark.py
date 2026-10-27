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
    vp8 = json.loads((ROOT / 'benchmarks/warm-vp8-borrowed-cursor-evidence.json').read_text(encoding='utf-8'))
    jpeg = json.loads((ROOT / 'benchmarks/warm-jpeg-horizontal-area-filter-evidence.json').read_text(encoding='utf-8'))
    color = json.loads((ROOT / 'benchmarks/warm-vp8l-scored-plane-histograms-evidence.json').read_text(encoding='utf-8'))
    reads = json.loads((ROOT / 'benchmarks/warm-image-read-evidence.json').read_text(encoding='utf-8'))
    probes = json.loads((ROOT / 'benchmarks/warm-probe-completed-reads-evidence.json').read_text(encoding='utf-8'))
    png_validation = json.loads((ROOT / 'benchmarks/png-validation-current-evidence.json').read_text(encoding='utf-8'))
    streamed = json.loads((ROOT / 'benchmarks/warm-png-streaming-validation-evidence.json').read_text(encoding='utf-8'))
    reconciled = json.loads((ROOT / 'benchmarks/warm-read-reconciliation-evidence.json').read_text(encoding='utf-8'))
    assert not reconciled['productionChanged'] and len(reconciled['cases']) == 6
    assert not reads['productionChanged'] and len(reads['cases']) == 12
    assert probes['productionChanged'] and len(probes['cases']) == 4
    assert not png_validation['productionChanged'] and png_validation['sourceAndQualifiedProducersRestored']
    assert streamed['productionChanged'] and len(streamed['cases']) == 4
    assert vp8['productionChanged'] and vp8['independentQuietConfirmationThisPair']
    assert jpeg['productionChanged'] and jpeg['independentQuietConfirmationThisPair']
    assert color['version'] == '0.2.1' and color['independentQuietConfirmationThisPair']
    assert current['version'] == '0.2.1' and current['independentQuietConfirmationThisPair']
    assert set(current['cases']) == {'EncodeFast8', 'EncodeSmallest8', 'EncodeSmallest16'}
    vp8_cases = {'case06': 'DiagramLossyDistortion8', 'case07': 'Alpha8',
                 'case08': 'PhotographLossy8', 'case09': 'DiagramLossy8',
                 'case10': 'DiagramLossyTrellis8', 'case11': 'Opaque16'}
    rows = []
    for case in evidence['cases']:
        observation = case['observation']
        color_case = case['id'] in {'case12', 'case13', 'case14', 'case15'}
        vp8_case = case['id'] in vp8_cases
        updated = vp8_case or color_case or case['id'] in {'case00', 'case02', 'case03', 'case17', 'case18', 'case19', 'case20'}
        producer = vp8 if vp8_case else jpeg if case['id'] in {'case02', 'case03'} else reconciled if case['id'] == 'case00' else color if color_case else stored if case['id'] == 'case20' else current
        if vp8_case:
            observation = vp8['cases'][vp8_cases[case['id']]][-1]
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
        samples = 12 if case['id'] in {'case02', 'case11', 'case15'} or updated and receipt['caseId'] in {'EncodeNoCompression16', 'EncodeFast8'} else 3
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
        producer = probes if case in probes['cases'] else streamed if case in streamed['cases'] else reads
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
    read_table = '\n'.join([header + ' Status |', separator + ' --- |', *read_rows])
    return f'''# Lokad.Image benchmarks

Updated 2026-10-28. **† Rows** use qualified 0.2.1 development-path evidence;
**‡ stored PNG** retains its unchanged 0.2.1 proof. Fourteen other controls retain
published 0.2.0 measurements on unchanged paths. VP8 Stream decode now has a
current-cost screen. The release snapshot remains immutable. Thirteen read
controls below use qualified 0.2.1 builds. These 44 displayed controls do not
complete the full CPU-parity objective
or establish general replacement readiness.

Measurements use the dedicated quiet Ubuntu 24.04 x64 VM
({evidence['machine']['cpuModel']}): one pinned CPU per operation, warm .NET 10,
sequential Release execution, no concurrency. This representative scope is not
the full performance objective.

Stored PNG16, Fastest PNG8, caller-RGB420 JPEG, opaque RGBA16 VP8 and VP8L photograph have 12 measured iterations each;
other codec/editing rows are short BenchmarkDotNet screens with three. Native brackets and
idle/process guards pass. SIMD scored-plane histograms reduce photograph elapsed
by 6.4% against a fresh independent baseline, with unchanged allocations and
exact packets. It remains a1.350× / upper1.371 gap; RGBA8/16/async are current screens.
Horizontal JPEG chroma filtering independently reduces caller-RGB420 elapsed by
16.1%, retaining373,416B/op and the approved exact packet boundary. Its1.283× /
upper1.294 gap remains; governed RGBA16 has a current1.224× screen.
Borrowed arithmetic cursors independently reduce opaque RGBA16 VP8 elapsed by
6.8%, retaining1,201,552B/op and exact packets; its1.410× / upper1.420 gap remains
open. The five other affected VP8 rows have current screens, without independent
speedup/parity claims.
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
[Borrowed VP8 cursors](../benchmarks/WARM-VP8-BORROWED-CURSOR.md) supply all six
current VP8 encoding rows;
[scored-plane histograms](../benchmarks/WARM-VP8L-SCORED-PLANE-HISTOGRAMS.md) supply the
four affected VP8L rows. Other paths retain applicable prior proof.
[JPEG area filtering](../benchmarks/WARM-JPEG-HORIZONTAL-AREA-FILTER.md) refreshes
caller RGB420 and governed RGBA16; other subsampled encoder costs need current
evidence before a pass claim. Grayscale/4:4:4 and decoding paths are unchanged.
The [read reconciliation](../benchmarks/WARM-READ-RECONCILIATION.md) refreshes VP8
Stream decode and four existing tight PNG companions without a speedup claim.

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
validation pass independently; progressive420 remains a confirmed gap. Other
validation rows are short screens, without independent parity claims. The
[streaming PNG validator](../benchmarks/WARM-PNG-VALIDATION.md) reduces PNG16 elapsed
by42.4% and warm span allocation from17,008 to240B/op. Its repeat passes with a
narrow upper0.99966; the preceding upper1.00108 is retained in the evidence.
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
