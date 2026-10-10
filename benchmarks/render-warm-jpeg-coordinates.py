"""Render original warm JPEG encoder block-geometry screens."""
import json
from pathlib import Path
import statistics

root=Path(__file__).resolve().parents[1]/'benchmarks'
data=json.loads((root/'warm-jpeg-coordinates-evidence.json').read_text())
baseline=data['observations']['baseline'][0]; last=data['observations']['coordinates'][-1]
quality=last['quality']; validation=data['validation']
managed=quality['managedQuality']; native=quality['nativeQuality']
lines=['# Warm single-CPU JPEG block-coordinate round','',
    'This round changes one production primitive: encoder block geometry.',
    'Derive each block origin and component sampling factors once outside the',
    'pixel/sample traversal. Sample order, edge clamping, flattening, color math,',
    'rounding, DCT and entropy coding remain unchanged, as do work/admission checks',
    'and failure behavior. No allocation, storage or cache is added.','',
    'It reuses the single matched [warm JPEG encode pair](WARM-JPEG-BYTE.md).',
    'The [numeric evidence](warm-jpeg-coordinates-evidence.json) retains both',
    'phases, actual managed samples/statistics, native elapsed/count brackets,',
    'source/tool/DLL identities, output bytes, quality, allocations and quietness.',
    'Regenerate with `python benchmarks/render-warm-jpeg-coordinates.py`.','',
    '| Phase | Repeat | Managed mean, µs | Native before / after, µs | Worst bracket ratio | Conservative upper |',
    '| --- | --- | ---: | ---: | ---: | ---: |']
for phase,rows in data['observations'].items():
    for i,row in enumerate(rows,1):
        brackets=[statistics.mean(v['NanosecondsPerOperation'] for v in row[key])/1000 for key in ('nativeBefore','nativeAfter')]
        upper=row['conservativeRatioUpper']; formatted=f'{upper:.2f}x' if upper is not None else 'unbounded'
        lines.append(f"| {phase} | {i} | {row['managedStatistics']['Mean']/1000:.2f} | {brackets[0]:.2f} / {brackets[1]:.2f} | {row['worstBracketRatio']:.2f}x | {formatted} |")
host=last['host']
lines+=['',f"Measured {data['measuredDate']}, {host['ProcessorName']}, {host['OsVersion']},",
    f"SDK {host['DotNetCliVersion']}, runtime {host['RuntimeVersion']}. CPU 0 only,",
    'normal native SIMD, one codec/conversion thread, optimized managed JIT with',
    'tiering disabled and non-concurrent workstation GC. Short independent',
    'screens guide priority and retain uncertainty; uniform parity is not met.',
    f"Candidate managed means have a {data['repeatability']['managedMeanSpreadFraction']:.1%} spread relative to the smaller mean.",'',
    'Both intervals include resident original RGBA8, exact once-rounded alpha',
    'flattening on RGB 0x204060, full-range BT.601 conversion, 4:4:4 encoding and',
    'complete independent output acquisition/release. Native contexts, scratch',
    'and wrappers are reused; every pixel is reprocessed. Fresh managed',
    'MemoryStream creation, growth and disposal are timed. Startup, IPC, I/O,',
    'generation, hashing, validation and quality enumeration are excluded.','',
    f"Frozen owned quality {managed['Setting']} / native qscale {native['Setting']} retain the",
    f"{quality['psnrGapDb']:.6f} dB source-error gap within the fixed 0.25 dB band.",
    f"Encoded sizes remain {managed['EncodedBytes']:,} / {native['EncodedBytes']:,} bytes. Managed fixed",
    'and native optimal Huffman policies remain explicit. The original preparation',
    'used seven owned settings and one source; this round adds neither settings',
    'nor images. Encoded hashes and all quality/decoder/alpha checks are unchanged.','',
    f"BDN managed allocation: {baseline['managedMemory']['BytesAllocatedPerOperation']:,} before and",
    f"{last['managedMemory']['BytesAllocatedPerOperation']:,} bytes/op after. Sampling/diagnostic variation",
    'is retained; this primitive adds no heap allocation or admission cost.','',
    'Windows/Linux full Release/live suites pass 1,855 tests (275 internal,',
    '686 public, 894 FFmpeg). Four SIMD/scalar Linux modes each pass 55 internal',
    'and 70 public JPEG controls. All 160 original frozen encoder bitstreams pass,',
    'including both input depths and all sampling modes. Exact memory/output',
    'boundaries, cancellation, caller stream ownership and immutable input pass.',
    'The same actual dependency-free Release package and all three usage examples',
    f"pass both OS: SHA256 `{validation['packageSha256']}`.",'',
    'Source-byte and pre-run quietness checks pass; failed post-run load samples',
    'and later passing cooldowns remain without weakened thresholds. The separate',
    'accepted encoder profile attributed 55.90% to Encode leaf samples and 13.99%',
    'to forward DCT; profile timing does not enter parity evidence.','',
    'Reproduce sequentially on the quiet dedicated Linux VM using the',
    '[JPEG worker instructions](../tools/WarmReference/README.md#jpeg), frozen',
    'preparation, output/quality smoke and two independent warm Jpeg repeats,',
    'with a passing cooldown between them.','']
(root/'WARM-JPEG-COORDINATES.md').write_text('\n'.join(lines),encoding='utf-8',newline='\n')
