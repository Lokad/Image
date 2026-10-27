"""Render the current release snapshot from retained evidence; never measure."""
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
    distortion = json.loads((ROOT / 'benchmarks/vp8-residual-summary-deferred-evidence.json').read_text(encoding='utf-8'))
    trellis = json.loads((ROOT / 'benchmarks/vp8-trellis-state-reduction-rejected-evidence.json').read_text(encoding='utf-8'))
    color = json.loads((ROOT / 'benchmarks/warm-vp8l-color-trial-reuse-evidence.json').read_text(encoding='utf-8'))
    reads = json.loads((ROOT / 'benchmarks/warm-image-read-evidence.json').read_text(encoding='utf-8'))
    assert not reads['productionChanged'] and len(reads['cases']) == 12
    assert not distortion['productionChanged'] and distortion['restoration']['sourceAndImmutableProducerRestored']
    assert not trellis['productionChanged'] and trellis['restoration']['sourceAndImmutableProducerRestored']
    assert color['version'] == '0.2.1' and color['independentQuietConfirmationThisPair']
    assert current['version'] == '0.2.1' and current['independentQuietConfirmationThisPair']
    assert set(current['cases']) == {'EncodeFast8', 'EncodeSmallest8', 'EncodeSmallest16'}
    rows = []
    for case in evidence['cases']:
        observation = case['observation']
        color_case = case['id'] in {'case12', 'case13', 'case14', 'case15'}
        updated = color_case or case['id'] in {'case06', 'case10', 'case17', 'case18', 'case19', 'case20'}
        producer = color if color_case else stored if case['id'] == 'case20' else current
        if case['id'] == 'case06':
            observation = distortion['baseline']
        elif case['id'] == 'case10':
            observation = trellis['baseline']
        elif updated:
            observation = producer['cases'][observation['receipt']['caseId']][-1]
        receipt = observation['receipt']
        stats = observation['managed']['statistics']
        native = min(b['mean'] for b in observation['nativeBrackets'])
        ratio = observation['worstBracketRatio']
        upper = observation['conservativeRatioUpper']
        assert receipt['librarySha256'] == (producer['producers']['linuxSha256'] if updated else evidence['librarySha256'])
        assert all(g['passed'] for g in observation['quietness'].values())
        samples = 12 if case['id'] == 'case12' or updated and receipt['caseId'] in {'EncodeNoCompression16', 'EncodeFast8'} else 3
        assert (8 if samples == 12 else 2) <= stats['N'] <= samples
        assert len(receipt['nativeBefore']) == len(receipt['nativeAfter']) == samples
        assert math.isclose(stats['Mean'] / native, ratio, rel_tol=1e-12)
        allocated = observation['managedMemory']['BytesAllocatedPerOperation']
        label = case['label'] + (' §' if case['id'] == 'case00' else ' ‡' if case['id'] == 'case20' else ' †' if updated else '')
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
        observation = observations[-1]
        receipt = observation['receipt']
        assert receipt['librarySha256'] == reads['producers']['linuxSha256']
        assert receipt['descriptorExact'] and receipt['prefixAndCodingScopesVerified']
        assert all(q['passed'] for q in observation['quietness'].values())
        confirmed = reads['status'][case].startswith('confirmed-')
        samples = 12 if confirmed else 3
        assert len(receipt['nativeBefore']) == len(receipt['nativeAfter']) == samples
        assert len(observation['managed']['actual']) == samples
        native = min(b['mean'] for b in observation['nativeBrackets'])
        stats = observation['managed']['statistics']
        ratio, upper = observation['worstBracketRatio'], observation['conservativeRatioUpper']
        assert math.isclose(stats['Mean'] / native, ratio, rel_tol=1e-12)
        assert (upper <= 1) == (reads['status'][case] == 'confirmed-pass')
        status = 'Confirmed pass' if upper <= 1 else 'Confirmed gap' if confirmed else 'Screen gap'
        read_rows.append(f'| {label} | {stats["Mean"] / 1000:,.3f} | {native / 1000:,.3f} | '
                         f'{ratio:.3f} / {upper:.3f} | {observation["managedMemory"]["BytesAllocatedPerOperation"]:,} | {status} |')
    read_table = '\n'.join([header + ' Status |', separator + ' --- |', *read_rows])
    return f'''# Lokad.Image benchmarks

Updated 2026-10-27. **† Rows** use qualified 0.2.1 development-path evidence;
**‡ stored PNG** retains its unchanged 0.2.1 proof. Twenty other controls retain
published 0.2.0 measurements on unchanged paths. **§ Decode** retains historical
costs pending current confirmation after read-policy changes. The release snapshot
remains immutable. Twelve new read controls below use the qualified 0.2.1 build.
These 43 controls do not complete the full CPU-parity objective
or establish general replacement readiness.

Measurements use the dedicated quiet Ubuntu 24.04 x64 VM
({evidence['machine']['cpuModel']}): one pinned CPU per operation, warm .NET 10,
sequential Release execution, no concurrency. This representative scope is not
the full performance objective.

Stored PNG16, Fastest PNG8 and current VP8L RGBA8 have 12 measured iterations each;
other rows are short BenchmarkDotNet screens with three. Native brackets and
idle/process guards pass. VP8L RGBA8 measures 1.486× / upper1.507 after exact
color-trial reuse, with 8 additional allocated B/op and unchanged array capacity.
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
[Effort2](../benchmarks/VP8-RESIDUAL-SUMMARY-DEFERRED.md) and
[trellis](../benchmarks/VP8-TRELLIS-STATE-REDUCTION-REJECTED.md) retain current baseline
shorts; [color-trial reuse](../benchmarks/WARM-VP8L-COLOR-TRIAL-REUSE.md) supplies the
four affected VP8L rows. Other paths retain applicable prior proof.

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
cleanup. Inputs are 513×259 except indexed Adam7. Baseline420 validation passes
independently; progressive420 remains a confirmed gap. Other new rows are short
screens, without independent parity claims. See [read boundaries and raw evidence](../benchmarks/WARM-IMAGE-READ.md).

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
