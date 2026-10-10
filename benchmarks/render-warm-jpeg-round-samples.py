"""Render original quiet-VM screens for JPEG sample rounding/conversion."""
import json
from pathlib import Path
import statistics

root = Path(__file__).resolve().parent
data = json.loads((root/'warm-jpeg-round-samples-evidence.json').read_text())
observations = data['observations']
lines = ['# Warm JPEG sample rounding and conversion', '',
    'The final inverse-DCT conversion rounds, clamps and writes eight independent',
    'pixels with safe AVX-512F/VL intrinsics. Transform accumulation and exact',
    'owned pixels remain unchanged. The retained gaps are still above 2x;',
    'these short screens do not establish uniform parity.', '',
    'The [original numeric evidence](warm-jpeg-round-samples-evidence.json) retains',
    'actual samples, statistics, allocation, native brackets, source and binary',
    'identities, frozen input/output hashes, correctness controls and quietness.',
    'Regenerate with `python benchmarks/render-warm-jpeg-round-samples.py`.', '',
    '| Input / stage | Repeat | Managed mean, µs | Native before / after, µs | Worst bracket ratio | Conservative upper |',
    '| --- | ---: | ---: | ---: | ---: | ---: |']
for key, label in [('baseline', 'Native / baseline'), ('sampleConversion', 'Native / SIMD conversion'), ('ownedBaseline', 'Owned / baseline screen'), ('ownedAffected', 'Owned / affected screen')]:
    for index, row in enumerate(observations[key], 1):
        native = [statistics.mean(v['NanosecondsPerOperation'] for v in row[k])/1000 for k in ('nativeBefore', 'nativeAfter')]
        upper = row['conservativeRatioUpper']; formatted = f'{upper:.2f}x' if upper is not None else 'unbounded'
        lines.append(f"| {label} | {index} | {row['managedStatistics']['Mean']/1000:.2f} | {native[0]:.2f} / {native[1]:.2f} | {row['worstBracketRatio']:.2f}x | {formatted} |")
last = observations['sampleConversion'][-1]; host = last['host']; change = data['allocationChange']
lines += ['', f"Measured {data['measuredDate']}, {host['ProcessorName']}, {host['OsVersion']},",
    f"SDK {host['DotNetCliVersion']}, runtime {host['RuntimeVersion']}. CPU 0 only,",
    'one codec/conversion thread, normal SIMD, optimized JIT with tiering disabled',
    'and non-concurrent workstation GC. Native-input managed mean spread is',
    f"{data['repeatability']['managedMeanSpreadFraction']*100:.3f}%. The owned affected input has one screen only.",
    'Uncertainty and the earlier observations remain in the numeric evidence.',
    f"The initial conversion variant screened at {data['rejectedInitialConversionScreen']['worstBracketRatio']:.2f}x and did not improve the baseline.",
    'That variant used explicit vector division without the inlining request.',
    'Its source, samples and quietness remain recorded separately.', '',
    'Both frozen JPEGs come from one existing original 513x259 RGBA8 raster,',
    'flattened once on RGB 0x204060. No additional raster, upstream corpus,',
    'preparation or quality enumeration is used. Each pair receives identical',
    'resident JPEG bytes and includes complete public decode/conversion, owned',
    'output allocation and disposal. Native context/conversion/input wrappers',
    'are reused; state reset and every decoded/converted pixel remain timed.',
    'Startup, IPC, I/O, generation, hashing, checks and file packing are excluded.', '',
    'One production primitive changes. The eight row sums use separate',
    'multiplication by the exact 0.25 and addition of 128. This equals division',
    'by four; the helper is inlined. Finite samples are clipped to the',
    'byte range, floored and incremented only when their exact fractional part',
    'is at least one half. Adding 0.5 before flooring could incorrectly round',
    'values immediately below a tie. The resulting integers are safely narrowed',
    'and copied to eight output bytes. The original basis, v/u accumulation',
    'order, separate products, 4,096 work units and admission/cancellation stay',
    'unchanged. AVX/scalar fallbacks remain available; there is no FMA,',
    'reassociation, new table, buffer, heap allocation or cache.', '',
    'All 1,868 tests pass on Windows and Linux: 288 internal, 686 public and',
    '894 live FFmpeg comparisons. The VM also passes 68 internal and 70 public',
    'JPEG controls in each of four modes: normal SIMD, AVX-512 disabled, AVX',
    'disabled and all intrinsics disabled. Five new cases cover 1,048 original',
    'scalar-reference boundary values, all eight rotations of distinct lanes,',
    'guard bytes and rejection of incomplete rows before writing.',
    'The 30 existing original JPEG fixtures retain exact pixels against the',
    'previous library; 160 frozen encoder bitstreams also pass. BDN output',
    'smoke and frozen warm outputs pass, with exact normal-SIMD FFmpeg CLI pixels.', '',
    'The same actual dependency-free Release package passes verification and',
    'all three usage-guide examples on both platforms. Package SHA256:',
    f"`{data['validation']['packageSha256']}`.", '',
    'Package and benchmark builds have distinct DLL identities. On the VM,',
    'their method-body fingerprints and all 30 original fixture pixels match.',
    'These controls make no timing claim; both DLL hashes remain recorded.', '',
    f"Native-input managed allocation is {last['managedMemory']['BytesAllocatedPerOperation']:,} bytes/op.",
    f"Observed changes against the prior native screens are {change['nativeObservedBytes']} bytes/op;",
    f"the owned affected screen changes by {change['ownedObservedBytes']} bytes/op. No storage is added.",
    'Failed quietness samples and later passing cooldowns remain recorded.',
    'A separate baseline workload profile attributed 34.41% of leaf work to',
    'inverse DCT, the largest individual primitive, and 41.45% inclusively to',
    'entropy/Huffman across several methods. Profile warmup/calibration samples',
    'are excluded from parity timing.', '',
    'Reproduce sequentially on the quiet dedicated Linux VM following the',
    '[worker instructions](../tools/WarmReference/README.md#jpeg-decode).', '']
(root/'WARM-JPEG-ROUND-SAMPLES.md').write_text('\n'.join(lines), encoding='utf-8', newline='\n')
