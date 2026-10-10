"""Render original quiet-VM screens for exact JPEG color-row construction."""
import json
from pathlib import Path
import statistics

root = Path(__file__).resolve().parent
data = json.loads((root/'warm-jpeg-color-row-evidence.json').read_text())
observations = data['observations']
lines = ['# Warm JPEG full-resolution color rows', '',
    'One production primitive converts eight full-resolution YCbCr samples to',
    'RGBA8 with safe AVX2/AVX-512F/VL intrinsics. Each original binary64 color',
    'operation, rounding result and output pixel remains unchanged. The retained',
    'gaps remain above 2x; short screens do not establish uniform parity.', '',
    'The [original numeric evidence](warm-jpeg-color-row-evidence.json) retains',
    'actual samples, statistics, allocation, native brackets, source and binary',
    'identities, frozen input/output hashes, correctness controls and quietness.',
    'Regenerate with `python benchmarks/render-warm-jpeg-color-row.py`.', '',
    '| Input / stage | Repeat | Managed mean, µs | Native before / after, µs | Worst bracket ratio | Conservative upper |',
    '| --- | ---: | ---: | ---: | ---: | ---: |']
for key,label in [('baseline','Native / baseline'),('colorRows','Native / color rows'),('ownedBaseline','Owned / baseline screen'),('ownedAffected','Owned / affected screen')]:
    for index,row in enumerate(observations[key],1):
        native = [statistics.mean(v['NanosecondsPerOperation'] for v in row[k])/1000 for k in ('nativeBefore','nativeAfter')]
        upper = row['conservativeRatioUpper']; formatted = f'{upper:.2f}x' if upper is not None else 'unbounded'
        lines.append(f"| {label} | {index} | {row['managedStatistics']['Mean']/1000:.2f} | {native[0]:.2f} / {native[1]:.2f} | {row['worstBracketRatio']:.2f}x | {formatted} |")
last = observations['colorRows'][-1]; host = last['host']; change = data['allocationChange']
nativeControl, ownedControl = change['directStreamControls']['controls']
lines += ['',f"Measured {data['measuredDate']}, {host['ProcessorName']}, {host['OsVersion']},",
    f"SDK {host['DotNetCliVersion']}, runtime {host['RuntimeVersion']}. CPU 0 only,",
    'one codec/conversion thread, normal SIMD, optimized JIT with tiering disabled',
    'and non-concurrent workstation GC. Native-input managed mean spread is',
    f"{data['repeatability']['managedMeanSpreadFraction']*100:.3f}%. The owned affected input has one screen only.",
    'Uncertainty and prior observations remain in the numeric evidence.', '',
    'Both frozen JPEGs come from one existing original 513x259 RGBA8 raster,',
    'flattened once on RGB 0x204060. No additional raster, upstream corpus,',
    'preparation or quality enumeration is used. Each pair receives identical',
    'resident JPEG bytes and includes complete public decode/conversion, owned',
    'output allocation and disposal. Native context/conversion/input wrappers',
    'are reused; state reset and every decoded/converted pixel remain timed.',
    'Startup, IPC, I/O, generation, hashing, checks and file packing are excluded.', '',
    'Equal component factors already map each source sample exactly to the',
    'output coordinate. Complete eight-pixel rows widen each of three byte planes',
    'to bounded integer lanes. Chroma subtraction by 128 is exact; the original',
    'binary64 color matrix retains its separate multiply/add/subtract operations.',
    'The existing sample-rounding primitive writes three eight-byte rows in',
    '24 bytes of the existing inverse-block scratch. Safe widening and shifts',
    'interleave R/G/B and opaque alpha into one 32-byte output. No buffer, cache,',
    'table, allocation, working-byte admission or logical work is added.',
    'Original row tails, subsampled interpolation, grayscale, explicit RGB and',
    'non-SIMD production paths remain. Common 1x2 factors with equal sampling',
    'qualify. Geometry/orientation, row work/cancellation, entropy/source/error',
    'order and independent output ownership remain unchanged.', '',
    'All 1,901 tests pass on Windows and Linux: 321 internal, 686 public and',
    '894 live FFmpeg comparisons. Four VM SIMD/scalar modes each pass 101 internal',
    'and 70 public JPEG controls. Twelve new cases check all 16,777,216 Y/Cb/Cr',
    'byte triples: 50,331,648 original scalar-reference RGB samples and exact',
    'opaque alpha. Eight unaligned mixed phases check input/output/scratch guards',
    'and unchanged source samples. Ten invalid length cases reject before writing.',
    'The 30 original JPEG fixtures keep exact owned pixels and 160 frozen encoder',
    'bitstreams remain unchanged. Output smoke and normal-SIMD FFmpeg CLI pixel',
    'controls pass on both frozen warm inputs.', '',
    'The same actual dependency-free Release package passes verification and',
    'all three usage-guide examples on both platforms. Package SHA256:',
    f"`{data['validation']['packageSha256']}`.", '',
    'Package and benchmark builds have distinct DLL identities. On the VM,',
    'their method-body fingerprints and all 30 original fixture pixels match.',
    'These controls make no timing claim; both DLL hashes remain recorded.', '',
    f"Native-input managed allocation is {last['managedMemory']['BytesAllocatedPerOperation']:,} bytes/op.",
    f"Inclusive BDN changes against the prior native screens are {change['nativeObservedBytes']} bytes/op;",
    f"the owned affected screen changes by {change['ownedObservedBytes']} bytes/op. Diagnostics remain visible.",
    'Separate strict controls perform 16 complete public Stream decodes on each',
    'frozen input through the baseline and measured DLLs. Candidate allocation',
    'totals do not increase; owned output hashes remain identical. Fresh caller',
    'streams and output disposal are included; argument storage is reused equally.',
    f"Native-input totals: {nativeControl['rows'][0]['totalAllocatedBytes']:,} / {nativeControl['rows'][1]['totalAllocatedBytes']:,} bytes (baseline / candidate).",
    f"Owned-input totals: {ownedControl['rows'][0]['totalAllocatedBytes']:,} / {ownedControl['rows'][1]['totalAllocatedBytes']:,} bytes (baseline / candidate).",
    'These are',
    'allocation controls, not timings; parity uses resident span inputs.', '',
    'Failed quietness samples and later passing cooldowns remain recorded.',
    'The separate current baseline profile attributes 26.14% of leaf work to',
    'Render, 23.34% to ReadBlock, 14.83% to Huffman.Read and 12.53% to inverse DCT.',
    'The coefficient/entropy group spans several methods and accounts for',
    '54.47% inclusively. Method names are matched without parameter type names.',
    'Profile workload includes warmup/calibration and is excluded from parity',
    'timing. The profile does not attribute a separate percentage to color math.', '',
    'Reproduce sequentially on the quiet dedicated Linux VM following the',
    '[worker instructions](../tools/WarmReference/README.md#jpeg-decode).', '']
(root/'WARM-JPEG-COLOR-ROW.md').write_text('\n'.join(lines),encoding='utf-8',newline='\n')
