"""Render retained single-CPU warm JPEG baseline-block observations."""
import json
from pathlib import Path
import statistics

root = Path(__file__).resolve().parent
data = json.loads((root/'warm-jpeg-baseline-block-evidence.json').read_text())
observations = data['observations']
lines = ['# Warm JPEG baseline coefficient blocks', '',
    'One production primitive separates admitted baseline JPEG coefficient blocks',
    'from progressive/refinement decoding. Decoded pixels and failure boundaries',
    'remain unchanged. Both retained gaps exceed 2x; these short screens do not',
    'establish uniform parity.', '',
    'The [original numeric evidence](warm-jpeg-baseline-block-evidence.json)',
    'retains samples, statistics, allocation diagnostics, native brackets, source',
    'and binary identities, frozen input/output hashes, controls and quietness.',
    'Regenerate with `python benchmarks/render-warm-jpeg-baseline-block.py`.', '',
    '| Input / stage | Repeat | Managed mean, µs | Native before / after, µs | Worst bracket ratio | Conservative upper |',
    '| --- | ---: | ---: | ---: | ---: | ---: |']
for key,label in [('baseline','Native / baseline'),('baselineBlocks','Native / baseline blocks'),('ownedBaseline','Owned / baseline screen'),('ownedAffected','Owned / affected screen')]:
    for index,row in enumerate(observations[key],1):
        native = [statistics.mean(v['NanosecondsPerOperation'] for v in row[k])/1000 for k in ('nativeBefore','nativeAfter')]
        upper = row['conservativeRatioUpper']; formatted = f'{upper:.2f}x' if upper is not None else 'unbounded'
        lines.append(f"| {label} | {index} | {row['managedStatistics']['Mean']/1000:.2f} | {native[0]:.2f} / {native[1]:.2f} | {row['worstBracketRatio']:.2f}x | {formatted} |")
last = observations['baselineBlocks'][-1]; host = last['host']; change = data['allocationChange']
nativeControl,ownedControl = change['directStreamControls']['controls']
lines += ['',f"Measured {data['measuredDate']}, {host['ProcessorName']}, {host['OsVersion']},",
    f"SDK {host['DotNetCliVersion']}, runtime {host['RuntimeVersion']}. CPU 0 only,",
    'one codec/conversion thread, normal SIMD, optimized JIT with tiering disabled',
    'and non-concurrent workstation GC. Native-input managed mean spread is',
    f"{data['repeatability']['managedMeanSpreadFraction']*100:.3f}%. The owned input has one affected screen only.", '',
    'Two frozen JPEGs come from one existing original 513x259 RGBA8 raster.',
    'No additional image, preparation or quality enumeration is used. Each pair',
    'receives identical resident JPEG bytes and includes complete public decode,',
    'conversion, independent owned output allocation and disposal. Native context,',
    'conversion and input wrappers are reused; reset and every decoded/converted',
    'pixel remain timed. Startup, IPC, I/O, generation, hashing, correctness checks',
    'and packing are excluded.', '',
    'Existing scan admission establishes SS=0, SE=63 and Ah=Al=0. The separate',
    'baseline loop uses a constant AC band and removes variable amplitude shifts',
    'and progressive/refinement state, following [T.81 F.2.2](https://www.w3.org/Graphics/JPEG/itu-t81.pdf).',
    'DC categories/predictor bounds, AC categories, zero runs, end-of-block,',
    'coefficient positions and symbol/amplitude/error order remain identical.',
    'The original progressive loop, Huffman/reader primitives, MCU/restart handling,',
    'work/cancellation, geometry/orientation, rendering and independent ownership',
    'remain. No storage, cache, table, allocation or dependency is added.', '',
    'All 1,915 tests pass on Windows and Linux: 335 internal, 686 public and',
    '894 live FFmpeg comparisons. Four VM SIMD/scalar modes each pass 115 internal',
    'and 70 public JPEG controls. Fourteen new cases cover 45 DC signed-endpoint',
    'subcases, 2,520 AC position/category/signed-endpoint subcases, dense mixed',
    'coefficients, zero runs, exact marker/source positions, untouched coefficients',
    'and guards. Seven malformed cases preserve failure messages, work and partial',
    'writes; two tight work budgets, cancellation and two invalid block lengths',
    'check admission before input/mutation. Thirty original fixtures retain exact',
    'pixels; all 160 frozen encoder bitstreams remain unchanged. Output smoke and',
    'FFmpeg CLI pixel controls pass.', '',
    'The same actual dependency-free Release package passes verification and all',
    'three usage-guide examples on both platforms. Package SHA256:',
    f"`{data['validation']['packageSha256']}`.",
    'Package and measured DLL identities differ; their method-body fingerprints',
    'and all 30 original fixture pixels match on the VM.', '',
    f"Inclusive BDN allocation changes are {change['nativeObservedBytes']} bytes/op for the native-input repeats",
    f"and {change['ownedObservedBytes']} bytes/op for the owned screen. Raw diagnostics remain visible.",
    'Separate strict controls perform 16 complete public Stream decodes per frozen',
    'input through baseline and measured DLLs. Candidate totals do not increase;',
    'owned pixel hashes match. Fresh caller streams and disposal are included.',
    f"Native-input totals: {nativeControl['rows'][0]['totalAllocatedBytes']:,} / {nativeControl['rows'][1]['totalAllocatedBytes']:,} bytes (baseline / candidate).",
    f"Owned-input totals: {ownedControl['rows'][0]['totalAllocatedBytes']:,} / {ownedControl['rows'][1]['totalAllocatedBytes']:,} bytes (baseline / candidate).",
    'These controls make no timing claim; warm parity uses resident span inputs.', '',
    'Failed quietness samples and later passing cooldowns are retained. The earlier',
    'profile predates the color-row and baseline-block changes. Its unchanged',
    'coefficient loop guided this candidate; its percentages are not candidate',
    'measurements. Profile warmup/calibration is excluded from parity timing.', '',
    'Reproduce sequentially on the quiet dedicated VM using the',
    '[worker instructions](../tools/WarmReference/README.md#jpeg-decode).', '']
(root/'WARM-JPEG-BASELINE-BLOCK.md').write_text('\n'.join(lines),encoding='utf-8',newline='\n')
