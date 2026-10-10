"""Render original full-operation screens for shared inverse-DCT products."""
import json
from pathlib import Path
import statistics

root=Path(__file__).resolve().parents[1]/'benchmarks'
data=json.loads((root/'warm-jpeg-inverse-rows-evidence.json').read_text())
baseline=data['observations']['baseline'][0]; last=data['observations']['sharedRows'][-1]
lines=['# Warm single-CPU JPEG shared inverse-DCT products','',
    'This round changes one production primitive: the AVX-512 inverse DCT.',
    'Each exact dequantized horizontal product is shared across eight independent',
    'output-row accumulators. Every pixel retains its original v/u term order,',
    'separate multiplications, frozen owned binary64 basis and scalar final',
    'rounding/clamping. AVX and scalar fallbacks, 4096 logical work units and',
    'storage admission are unchanged; no allocation or cache is added.','',
    'It reuses the single [warm JPEG decode pair](WARM-JPEG-DECODE.md). The',
    '[numeric evidence](warm-jpeg-inverse-rows-evidence.json) retains the accepted',
    'sampling baseline and both candidate repeats, raw samples/statistics, native',
    'brackets, allocations, identities, output hashes and quietness.',
    'Regenerate with `python benchmarks/render-warm-jpeg-inverse-rows.py`.','',
    '| Phase | Repeat | Managed mean, µs | Native before / after, µs | Worst bracket ratio | Conservative upper |',
    '| --- | --- | ---: | ---: | ---: | ---: |']
for phase,rows in data['observations'].items():
    for i,row in enumerate(rows,1):
        native=[statistics.mean(v['NanosecondsPerOperation'] for v in row[key])/1000 for key in ('nativeBefore','nativeAfter')]
        upper=row['conservativeRatioUpper']; formatted=f'{upper:.2f}x' if upper is not None else 'unbounded'
        lines.append(f"| {phase} | {i} | {row['managedStatistics']['Mean']/1000:.2f} | {native[0]:.2f} / {native[1]:.2f} | {row['worstBracketRatio']:.2f}x | {formatted} |")
host=last['host']; validation=data['fullValidation']
lines+=['',f"Measured {data['measuredDate']}, {host['ProcessorName']}, {host['OsVersion']},",
    f"SDK {host['DotNetCliVersion']}, runtime {host['RuntimeVersion']}. CPU 0 only,",
    'normal native SIMD, one codec/conversion thread, optimized managed JIT with',
    'tiering disabled and non-concurrent workstation GC. Short repeats guide',
    'priority; uncertainty and the differing baseline screens remain visible.',
    'Uniform parity and stable speed certification are not established.','',
    f"The candidate means have a {data['repeatability']['managedMeanSpreadFraction']:.1%} spread relative to the smaller mean.",'',
    'Both subjects decode the identical resident 324,002-byte baseline 8-bit',
    '4:4:4 JPEG to independent RGBA8, including full-range BT.601 conversion,',
    'allocation and release. Native state reset is timed; contexts/wrappers are',
    'reused while each call decodes/converts every pixel. Padded native rows need',
    'no tight-row copy. Public managed Decode/disposal are timed. Startup, IPC,',
    'I/O, generation, hashing, validation and packing writes are excluded.','',
    'No raster or quality enumeration is added. Input and both output hashes',
    'remain unchanged; owned pixels equal the frozen preparation, native pixels',
    'equal the pinned CLI. Maximum 4/RMSE 1.5 decoder bounds, exact opaque alpha',
    'and independent live output storage are retained.','',
    f"Managed BDN allocation: {baseline['managedMemory']['BytesAllocatedPerOperation']:,} before and",
    f"{last['managedMemory']['BytesAllocatedPerOperation']:,} bytes/op after. Sampling/diagnostic variation",
    'is retained; the production primitive introduces no heap allocation or',
    'additional admission cost.','',
    'All 19 original inverse-transform controls pass, including frozen scalar',
    'half ties, clipping, dense/extreme coefficients and ushort quantizers,',
    'immutable inputs, guarded output, work/cancellation and complete lengths.',
    'Exact owned pixels match the accepted Release package across all 30 existing',
    'original JPEG fixtures; all 160 frozen encoder bitstreams remain unchanged.',
    'Windows/Linux full Release/live suites pass 1,855 tests (275 internal,',
    '686 public, 894 FFmpeg). Four SIMD/scalar Linux modes each pass 55 internal',
    'and 70 public JPEG controls. The same actual dependency-free Release package',
    'and all three usage examples pass both OS:',
    f"SHA256 `{validation['packageSha256']}`.",'',
    'Source-byte and pre-run quietness checks pass. Failed post-run load samples',
    'and later passing cooldowns remain without weakened thresholds. The separate',
    'accepted-sampling profile attributed 45.30% to inverse DCT leaf samples;',
    'profile timing does not enter parity evidence.','',
    'Reproduce sequentially on the dedicated quiet Linux VM using the',
    '[worker instructions](../tools/WarmReference/README.md#jpeg-decode), frozen',
    'preparation, output smoke and two independent warm JpegDecode repeats,',
    'with a passing cooldown between them.','']
(root/'WARM-JPEG-INVERSE-ROWS.md').write_text('\n'.join(lines),encoding='utf-8',newline='\n')
