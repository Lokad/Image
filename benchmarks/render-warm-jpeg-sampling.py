"""Render the original full-resolution JPEG sampling round."""
import json
from pathlib import Path
import statistics

root=Path(__file__).resolve().parent
data=json.loads((root/'warm-jpeg-sampling-evidence.json').read_text())
baseline=data['observations']['baseline'][0]; last=data['observations']['direct'][-1]
lines=['# Warm single-CPU JPEG full-resolution sampling round','',
    'This round changes one production primitive: component sampling. When both',
    'component factors equal the frame maximum, centered coordinates map exactly',
    'to integer x/y, with zero interpolation fractions. Read the existing admitted',
    'sample directly. Subsampled components retain the original interpolation.',
    'Planes, inverse DCT, color conversion, rounding, output allocation/ownership',
    'and per-row work/cancellation checks remain unchanged. No storage is added.','',
    'It reuses the single [warm JPEG decode pair](WARM-JPEG-DECODE.md), following',
    'the [Huffman lookup round](WARM-JPEG-PREFIX.md). The',
    '[numeric evidence](warm-jpeg-sampling-evidence.json) retains both phases, original raw samples,',
    'statistics, native brackets, allocations, identities and quietness.',
    'Regenerate with `python benchmarks/render-warm-jpeg-sampling.py`.','',
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
    'tiering disabled and non-concurrent workstation GC. These short independent',
    'repeats guide priority and retain uncertainty; uniform parity is not met.','',
    f"Managed means differ by {data['repeatability']['firstToSecondManagedDecreaseFraction']:.1%} between screens, while",
    'native brackets remain close and each screen has smaller internal variation.',
    'Both managed means improve on both baseline means, but these screens do not',
    'certify stable speed. Keep this variation visible when ranking further work.','',
    'Both subjects decode the identical resident 324,002-byte baseline 8-bit',
    '4:4:4 JPEG to independent RGBA8, including full-range BT.601 conversion,',
    'allocation and release. Native per-call state reset is timed; wrappers and',
    'contexts are reused, while each call decodes/converts every pixel. Padded',
    'native rows require no tight-row copy. Public managed Decode/disposal are',
    'timed. Startup, IPC, I/O, generation, hashing and validation are excluded.','',
    'No new raster or quality enumeration is added. Input and both output hashes',
    'remain unchanged, native pixels equal the pinned CLI, decoder agreement',
    'retains maximum 4/RMSE 1.5 and exact opaque alpha. Live outputs are independent.','',
    f"Managed BDN allocation: {baseline['managedMemory']['BytesAllocatedPerOperation']:,} before and",
    f"{last['managedMemory']['BytesAllocatedPerOperation']:,} bytes/op after, retaining sampling/diagnostic",
    'variation. The production change introduces no allocation or admission cost.','',
    'Exact owned decoded pixels match the accepted Release package on all 30',
    'existing original JPEG fixtures. An independent coordinate control covers',
    'all 262,140 coordinate/factor pairs over the complete JPEG dimension range',
    '(0 through 65,534; factors 1 through 4) with exact zero fractional weights.',
    'Existing analytical RGB/progressive, sampling, padding, work, cancellation',
    'and admission controls pass. Windows/Linux full Release/live suites pass',
    '1,855 tests (275 internal, 686 public, 894 FFmpeg). Four SIMD/scalar Linux',
    'modes each pass 55 internal and 70 public JPEG controls. All 160 frozen',
    'encoded JPEG bitstreams remain unchanged. The same actual dependency-free',
    'Release package and all three usage examples pass both OS:',
    f"SHA256 `{validation['packageSha256']}`.",'',
    'Source-byte and pre-run quietness checks pass; failed post-run load samples',
    'and later passing cooldowns remain without weakened thresholds. The separate',
    'accepted-lookup workload profile attributed 33.88% to Render leaf samples',
    'and 30.39% to inverse DCT; its timing is excluded from parity evidence.','',
    'Reproduce sequentially on the dedicated quiet Linux VM using the',
    '[worker instructions](../tools/WarmReference/README.md#jpeg-decode), frozen',
    'preparation, output smoke and two independent warm JpegDecode repeats,',
    'with a passing cooldown between them.','']
(root/'WARM-JPEG-SAMPLING.md').write_text('\n'.join(lines),encoding='utf-8',newline='\n')
