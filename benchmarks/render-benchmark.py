"""Render the current immutable-package benchmark evidence."""

import argparse
import json
import math
from pathlib import Path
import re


ROOT = Path(__file__).resolve().parents[1]
EVIDENCE = ROOT / 'benchmarks/final-package-benchmarks.json'
DOCUMENT = ROOT / 'docs/benchmark.md'
GROUPS = (('codec', 'Codecs'), ('editing', 'Editing and text'), ('inspection', 'Inspection and pixel validation'))


def number(value):
    return f'{math.ceil(value):,}' if value is not None else '—'


def render(data):
    qualification = data['qualification']
    tests = ' and '.join(f"{name} ({r['normalTests']:,} normal and {r['scalarTests']:,} scalar)"
                         for name, r in qualification.items())
    text = data['textPreparation']
    preparation = text['preparationUs']
    amortized = text['amortizedBatchUsPerDraw']
    lines = [
        '# Lokad.Image benchmarks', '',
        f"All **{data['displayedRows']} comparisons** pass on Lokad.Image **{data['version']}**",
        f"from package `{data['packageSha256'][:12]}`. The [coverage inventory](../benchmarks/WARM-COVERAGE.md)",
        f"covers **{data['materialPaths']} required paths**, with scoped evidence for the remaining operations.", '',
        'Measurements ran sequentially on one pinned CPU of the dedicated quiet VM under .NET 10',
        'with optimized JIT and nonconcurrent workstation GC. Each operation includes its',
        'declared input delivery, normalization, output ownership and disposal. Pass requires a',
        'conservative ratio upper bound of 1 or less. The tables use Lokad/reference ratios.', '',
        '† The reference can retain prepared codec contexts or filter graphs according to the',
        'recorded boundary. One-shot rows include the named setup. zimg is the FFmpeg zscale',
        'backend and uses float intermediates. Lokad resizing preserves its exact managed',
        'pixel contract. Startup, fixture preparation and validation are outside the clocks.', '',
        'Allocation is GC bytes per complete operation. A dash denotes an unmeasured value.',
        'Governed peak capacity and retained state are recorded separately in the evidence.',
        'Packet sizes cover the declared [codec-parity controls](codec-parity.md).', '',
        'Text rows use ImageSharp.Drawing 2.1.7, Fonts 2.1.3 and ImageSharp 3.1.12. The',
        'five authored controls include the [reproducible reference corrections]'
        '(../benchmarks/AUTHORED-FONT-COMPARISONS.md). The complete integrations include all',
        '30 fitting trials. The repeated row measures preparation, eight draws and cleanup',
        'together. FreeType loading retains its scoped copy/open/retain/dispose boundary.',
        'Font validation eagerness and prepared-state retention differ between references.', '',
        f"Preparation alone took **{preparation[0]:,.2f} / {preparation[1]:,.2f} µs**. The measured",
        f"eight-draw batch amortizes to **{amortized[0]:,.2f} / {amortized[1]:,.2f} µs/draw**.",
        f"Managed preparation retained **{text['managedAdmittedPreparedBytes']:,} B** of admitted capacity.",
        'The approximate live-heap observations and the scoped preparation and steady-draw',
        'diagnostics are recorded in the current evidence.', '',
        f'The test suites passed on {tests}, including live FFmpeg differential tests.',
        'Both platforms also passed the consumer smoke test and all three usage examples',
        'with the same measured package. [Current benchmark evidence](../benchmarks/final-package-benchmarks.json)', '',
    ]
    for group, title in GROUPS:
        lines.extend((f'## {title}', ''))
        heading = '| Operation | Reference | Lokad µs | Ref. µs | Ratio / upper | GC B/op Lokad / ref. | Status |'
        divider = '| --- | --- | ---: | ---: | ---: | ---: | --- |'
        if group == 'codec':
            heading += ' Packet B Lokad / ref. |'
            divider += ' ---: |'
        lines.extend((heading,divider))
        for row in data['rows'].values():
            if row['family'] != group:
                continue
            title = re.sub(r' \((?:pass|scoped pass|parity confirmed)\)', '', row['label'])
            title = title.replace('|', r'\|')
            ratio = row['managedUs']/row['nativeUs']
            memory = f"{number(row['allocatedBytes'])} / {number(row.get('referenceAllocatedBytes'))}"
            values = [title,row['reference'],f"{row['managedUs']:,.3f}",f"{row['nativeUs']:,.3f}",
                      f"{ratio:.3f} / {row['upper']:.6f}",memory,row['status'].capitalize()]
            if group == 'codec':
                managed = row.get('managedOutputBytes',row.get('packetBytes'))
                native = row.get('nativeOutputBytes',row.get('packetBytes'))
                values.append(f'{number(managed)} / {number(native)}' if managed is not None else '—')
            lines.append('| ' + ' | '.join(values) + ' |')
        lines.append('')
    return '\n'.join(lines)


def verify(data):
    assert data['materialPaths'] == 289 and data['allScopeEvidencePassing']
    assert data['displayedRows'] == len(data['rows']) == 165
    assert list(data['rows']) == [f'case{i:03d}' for i in range(165)]
    assert set(data['qualification']) == {'Windows','Linux'}
    for result in data['qualification'].values():
        assert result['normalTests'] > 0 and result['scalarTests'] > 0 and result['liveDifferential']
        assert result['actualPackageConsumer'] and result['usageExamples'] == 3
        assert result['actualPackageSha256'] == data['packageSha256']
    for row in data['rows'].values():
        assert row['status'] == 'pass' and row['upper'] <= 1 and row['managedUs'] > 0 and row['nativeUs'] > 0
        assert row['evidence']['members'] and row['reference'] and row['family'] in dict(GROUPS)


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--check',action='store_true',help='Verify that the checked-in document matches the evidence')
    args = parser.parse_args()
    data = json.loads(EVIDENCE.read_text(encoding='utf-8'))
    verify(data)
    body = render(data)
    if args.check:
        if body != DOCUMENT.read_text(encoding='utf-8'):
            raise SystemExit('Benchmark summary differs from current immutable-package evidence.')
        print('Benchmark summary matches the verified immutable-package evidence (three tables).')
    else:
        DOCUMENT.write_text(body,encoding='utf-8',newline='\n')


if __name__ == '__main__':
    main()
