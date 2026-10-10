"""Render retained MCU-coordinate rejection observations."""
import json,statistics
from pathlib import Path
root=Path(__file__).resolve().parent
data=json.loads((root/'warm-jpeg-mcu-rejection-evidence.json').read_text(encoding='utf-8'))
rows=data['observations']
lines=['# Warm JPEG rejected MCU-coordinate hoist','',
    'Calculating MCU row and column once provided no useful measured gain.',
    'Native-input changes are small and overlap sample variability; the affected',
    'owned-input screen is slightly slower. The production change was discarded.',
    'The retained wrapper implementation remains roughly 2.8x FFmpeg.','',
    'The [numeric evidence](warm-jpeg-mcu-rejection-evidence.json) retains actual',
    'samples, uncertainty, native brackets, allocations, input/output hashes, source',
    'identities and controls. Regenerate with',
    '`python benchmarks/render-warm-jpeg-mcu-rejection.py`.','',
    '| Input / stage | Repeat | Managed mean, µs | Native before / after, µs | Worst bracket ratio | Conservative upper |',
    '| --- | ---: | ---: | ---: | ---: | ---: |']
for key,label in [('baseline','Native / accepted wrapper'),('mcuCoordinates','Native / rejected hoist'),('ownedBaseline','Owned / accepted wrapper'),('ownedAffected','Owned / rejected screen')]:
    for index,row in enumerate(rows[key],1):
        native=[statistics.mean(value['NanosecondsPerOperation'] for value in row[k])/1000 for k in ('nativeBefore','nativeAfter')]
        upper=row['conservativeRatioUpper']; formatted=f'{upper:.2f}x' if upper is not None else 'unbounded'
        lines.append(f"| {label} | {index} | {row['managedStatistics']['Mean']/1000:.2f} | {native[0]:.2f} / {native[1]:.2f} | {row['worstBracketRatio']:.2f}x | {formatted} |")
lines+=['','The change calculated quotient/remainder after the existing MCU work admission',
    'and restart handling, then reused those coordinates for every component block.',
    'Correctness-only original generated-code controls confirm that division moved',
    'out of the inner block loop. All thirty original fixture outputs matched.',
    'Less repeated arithmetic alone did not establish a useful speedup.','',
    'All three screens use the same two frozen JPEGs from one existing original',
    '513x259 RGBA8 raster. No new image, preparation or quality enumeration is used.',
    'Runs are sequential on CPU 0 of the dedicated AMD EPYC 9V45 VM, with .NET',
    '10.0.8 / SDK 10.0.204, one codec/conversion thread, normal SIMD, disabled tiering',
    'and non-concurrent workstation GC. Resident identical compressed bytes feed',
    'complete public decode, conversion, independent owned allocation and disposal.',
    'Native reset is timed; startup, IPC, I/O, generation, hashing and validation',
    'are excluded. Frozen output hashes match the accepted wrapper version.','',
    'All 2,000 tests pass on Windows and Linux: 420 internal, 686 public and',
    '894 live FFmpeg comparisons. Baseline/progressive sampling, restart, logical',
    'work, memory/output admission and cancellation remain checked. Corpus unchanged.',
    f"Inclusive allocation changes: {data['allocationChange']['nativeObservedBytes']} bytes/op native,",
    f"{data['allocationChange']['ownedObservedBytes']} bytes/op owned.",'',
    'Failed post-run quietness checks and later passing cooldowns are retained.',
    'Short screens neither certify small speedups nor establish regressions. The',
    'production implementation and verified Release package from the accepted',
    'wrapper checkpoint remain unchanged. Uniform warm parity stays open.','']
(root/'WARM-JPEG-MCU-REJECTION.md').write_text('\n'.join(lines),encoding='utf-8',newline='\n')
