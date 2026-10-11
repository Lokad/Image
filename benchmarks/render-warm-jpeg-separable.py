import json,statistics
from pathlib import Path
root=Path(__file__).resolve().parent
data=json.loads((root/'warm-jpeg-separable-evidence.json').read_text(encoding='utf-8'))
rows=data['observations']; change=data['allocationChange']
lines=['# Warm JPEG certified separable inverse transform','',
    'Horizontal and vertical AVX-512/VL passes reduce inverse-transform work.',
    'A conservative numerical certificate preserves every original output byte;',
    'uncertain rows and extreme blocks use the original direct calculation.',
    'The retained JPEG decode cases remain above the 2x target.','',
    'The [numeric evidence](warm-jpeg-separable-evidence.json) retains actual',
    'samples, uncertainty, native brackets, allocations, source/input/output hashes,',
    'primitive/assembly controls and terminal receipts. Regenerate with',
    '`python benchmarks/render-warm-jpeg-separable.py`.','',
    '| Input / stage | Repeat | Managed mean, µs | Native before / after, µs | Worst bracket ratio | Conservative upper |',
    '| --- | ---: | ---: | ---: | ---: | ---: |']
for key,label in [('baseline','Native / accepted wrapper'),('preliminary','Native / initial two-pass screen'),('separable','Native / fixed spans'),('ownedBaseline','Owned / accepted wrapper'),('ownedAffected','Owned / affected screen')]:
    for index,row in enumerate(rows[key],1):
        brackets=[statistics.mean(sample['NanosecondsPerOperation'] for sample in row[k])/1000 for k in ('nativeBefore','nativeAfter')]
        upper=row['conservativeRatioUpper']; upperText=f'{upper:.2f}x' if upper is not None else 'unbounded'
        lines.append(f"| {label} | {index} | {row['managedStatistics']['Mean']/1000:.2f} | {brackets[0]:.2f} / {brackets[1]:.2f} | {row['worstBracketRatio']:.2f}x | {upperText} |")
lines+=['',
    'The [rounding proof](../docs/jpeg-inverse-rounding.md) bounds reassociation',
    'against the frozen direct binary64 evaluation. It retains separate products,',
    'the original basis, half ties, clipping and all 4096 admitted logical work',
    'units. No per-block buffer, image cache, new table, dependency or unsafe code',
    'is introduced. AVX/scalar paths retain the original calculation.','',
    'The initial screen prompted generated-code inspection rather than a repeat:',
    'its horizontal pass stored eight intermediate vectors back to the stack on',
    'every iteration. Fixed-size spans and hoisted basis access keep them in',
    'registers. This is a refinement of the same primitive. Original generated-code',
    'and fixture controls are correctness evidence, not timing measurements.','',
    'Untimed counters in a private copy of the owned source find no uncertain',
    'rows on either frozen JPEG: 6,630 / 6,435 factored blocks, with matching',
    'decoded output hashes. These controls do not measure certificate cost.','',
    'The two frozen JPEGs use one existing original 513x259 RGBA8 raster. No new',
    'image, preparation or quality enumeration is used. Complete public decode,',
    'conversion, independent owned allocation and disposal are timed on identical',
    'resident compressed bytes. Native contexts retain timed reset and all pixel',
    'work. Startup, IPC, I/O, hashing and validation are excluded. Runs are',
    'sequential on CPU 0 of the dedicated AMD EPYC 9V45 VM: one codec/conversion',
    'thread, normal SIMD, .NET 10.0.8 / SDK 10.0.204, tiering disabled and',
    'non-concurrent workstation GC.','',
    'All 2,008 tests pass on Windows and Linux: 428 internal, 686 public and',
    '894 live FFmpeg comparisons. Eight new original coefficient families contain',
    '2,947 blocks; their bytes match the previous owned assembly in verified',
    'AVX-512, AVX and scalar configurations. All twelve original transform hashes,',
    'input/destination guards, exact admission and cancellation controls remain.',
    'The original raster fixture corpus and frozen decode output hashes are unchanged.','',
    'The same actual dependency-free Release package passes verification and all',
    'three usage examples on both platforms. It includes the rounding proof.',
    'Its method bodies, all thirty original JPEG fixture outputs and the primitive',
    'families match the actual measured DLL. Package SHA256:',
    f"`{data['validation']['packageSha256']}`.",'',
    f"Inclusive allocation changes: {change['nativeObservedBytes']} bytes/op native,",
    f"{change['ownedObservedBytes']} bytes/op owned. Managed mean spread:",
    f"{data['repeatability']['managedMeanSpreadFraction']*100:.3f}%.",'',
    'Failed post-run quietness checks and later passing cooldowns are retained.',
    'Preparatory attempts refused timing after failed pre-run quiet checks;',
    'their terminal receipts remain alongside the later actual screens.',
    'Short screens guide ranking and do not certify uniform parity. The owned',
    'input has one affected screen; both native and managed uncertainty remain',
    'visible. Other measured, unmatched and unknown directions remain open.','']
(root/'WARM-JPEG-SEPARABLE.md').write_text('\n'.join(lines),encoding='utf-8',newline='\n')
