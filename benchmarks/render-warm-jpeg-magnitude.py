import json,statistics
from pathlib import Path
root=Path(__file__).resolve().parent
data=json.loads((root/'warm-jpeg-magnitude-evidence.json').read_text(encoding='utf-8'))
rows=data['observations']; change=data['allocationChange']
lines=['# Warm JPEG fused certificate magnitude','',
    'The horizontal inverse-transform pass reuses each dequantized coefficient',
    'for four independent exact magnitude sums. This removes a separate serial',
    'pass and repeated dequantization while retaining the same rounding certificate.',
    'Uniform warm parity remains open.','',
    'The [numeric evidence](warm-jpeg-magnitude-evidence.json) retains actual',
    'samples, uncertainty, native brackets, allocations, source/input/output hashes,',
    'primitive/assembly controls and terminal receipts. Regenerate with',
    '`python benchmarks/render-warm-jpeg-magnitude.py`.','',
    '| Input / stage | Repeat | Managed mean, µs | Native before / after, µs | Worst bracket ratio | Conservative upper |',
    '| --- | ---: | ---: | ---: | ---: | ---: |']
for key,label in [('baseline','Native / separate magnitude'),('preliminary','Native / eight-sum screen'),('magnitude','Native / fused four sums'),('ownedBaseline','Owned / separate magnitude'),('ownedAffected','Owned / affected screen')]:
    for index,row in enumerate(rows[key],1):
        brackets=[statistics.mean(sample['NanosecondsPerOperation'] for sample in row[k])/1000 for k in ('nativeBefore','nativeAfter')]
        upper=row['conservativeRatioUpper']; upperText=f'{upper:.2f}x' if upper is not None else 'unbounded'
        lines.append(f"| {label} | {index} | {row['managedStatistics']['Mean']/1000:.2f} | {brackets[0]:.2f} / {brackets[1]:.2f} | {row['worstBracketRatio']:.2f}x | {upperText} |")
profile=data['currentCpuProfile']
lines+=['',
    'Each product and every partial magnitude sum is an integer below 2^53.',
    'Regrouping therefore gives the identical bound in the',
    '[rounding proof](../docs/jpeg-inverse-rounding.md). Extreme blocks discard',
    'their horizontal sums before writing and use the original direct evaluation.',
    'Uncertain rows, AVX/scalar paths, the frozen basis and all 4096 admitted work',
    'units remain unchanged. Generated code retains vector accumulators in',
    'registers and uses separate products and additions. No per-block buffer,',
    'image cache, new table, dependency or unsafe code is introduced.','',
    'The initial eight-sum version had one screen. Generated-code inspection',
    'found two scalar sums spilling to the stack. Four independent exact sums',
    'reduce register pressure; the horizontal loop keeps scalar and vector sums',
    'in registers. The small shape change has no separate speedup claim.','',
    'The two frozen JPEGs use one existing original 513x259 RGBA8 raster. No new',
    'image, preparation or quality enumeration is used. Complete public decode,',
    'conversion, independent owned allocation and disposal are timed on identical',
    'resident compressed bytes. Native contexts retain timed reset and all pixel',
    'work. Startup, IPC, I/O, hashing and validation are excluded. Runs are',
    'sequential on CPU 0 of the dedicated AMD EPYC 9V45 VM: one codec/conversion',
    'thread, normal SIMD, .NET 10.0.8 / SDK 10.0.204, tiering disabled and',
    'non-concurrent workstation GC.','',
    'All 2,008 tests pass on Windows and Linux: 428 internal, 686 public and',
    '894 live FFmpeg comparisons. The same eight original coefficient families',
    '(2,947 blocks) match the accepted assembly in verified AVX-512, AVX and scalar',
    'configurations. All twelve original transform hashes, half-boundary controls,',
    'full integer/quantizer extremes, input/destination guards, exact admission and',
    'cancellation checks remain. No test or original raster corpus growth.','',
    'The same actual dependency-free Release package passes verification and all',
    'three usage examples on both platforms. Its method bodies, every pixel of',
    'thirty original JPEG fixtures and the primitive families match the actual',
    'measured DLL. The packaged rounding proof matches source. Package SHA256:',
    f"`{data['validation']['packageSha256']}`.",'',
    f"Inclusive allocation changes: {change['nativeObservedBytes']} bytes/op native,",
    f"{change['ownedObservedBytes']} bytes/op owned. Managed mean spread:",
    f"{data['repeatability']['managedMeanSpreadFraction']*100:.3f}%.",'',
    'One separate CPU sampling run uses the same frozen native JPEG and current',
    'source. Workload samples include warmup and calibration; they are attribution',
    'evidence, not another parity result. Method matching excludes parameter types.',
    f"The coefficient/entropy method union is {profile['inclusivePrimitivePercent'].get('coefficientEntropyGroup',0):.2f}% inclusive;",
    f"inverse transform is {profile['inclusivePrimitivePercent'].get('JpegTransform.Inverse',0):.2f}% inclusive.",
    'The numeric evidence retains the exclusive leaves, primitive inclusives and',
    'trace hash. The built profile-project assembly matches measured method bodies',
    'and all thirty original JPEG fixture pixels; the child build log and source',
    'inventory accompany the trace. Other timing screens exclude the profiler.','',
    'Failed post-run quietness checks and later passing cooldowns are retained.',
    'Short screens guide ranking and do not certify uniform parity. The owned',
    'input has one affected screen; both native and managed uncertainty remain',
    'visible. Other measured, unmatched and unknown directions remain open.','']
(root/'WARM-JPEG-MAGNITUDE.md').write_text('\n'.join(lines),encoding='utf-8',newline='\n')
