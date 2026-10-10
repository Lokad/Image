"""Render original quiet-VM observations for safe component-row sampling."""
import json
from pathlib import Path
import statistics

root = Path(__file__).resolve().parent
data = json.loads((root/'warm-jpeg-component-row-evidence.json').read_text())
baseline = data['observations']['baseline'][0]; last = data['observations']['componentRows'][-1]
quality = last['quality']; managed = quality['managedQuality']; native = quality['nativeQuality']
lines = ['# Warm single-CPU JPEG component-row construction', '',
    'One production primitive constructs eight full-resolution component samples',
    'from a complete RGBA8 row with safe AVX2/AVX-512 intrinsics. Exact alpha',
    'flattening, separate binary64 color operations, rounding and encoded bytes',
    'remain unchanged. The original generic production paths remain available.',
    'The retained screens remain above 2x; uniform parity is not established.', '',
    'The [original numeric evidence](warm-jpeg-component-row-evidence.json) retains',
    'actual samples, statistics, native brackets, quality, exact output hashes,',
    'source/tool/DLL identities, allocation, correctness controls and quietness.',
    'Regenerate with `python benchmarks/render-warm-jpeg-component-row.py`.', '',
    '| Phase | Repeat | Managed mean, µs | Native before / after, µs | Worst bracket ratio | Conservative upper |',
    '| --- | ---: | ---: | ---: | ---: | ---: |']
for phase, rows in data['observations'].items():
    for i, row in enumerate(rows,1):
        brackets = [statistics.mean(v['NanosecondsPerOperation'] for v in row[key])/1000 for key in ('nativeBefore','nativeAfter')]
        upper = row['conservativeRatioUpper']; formatted = f'{upper:.2f}x' if upper is not None else 'unbounded'
        lines.append(f"| {phase} | {i} | {row['managedStatistics']['Mean']/1000:.2f} | {brackets[0]:.2f} / {brackets[1]:.2f} | {row['worstBracketRatio']:.2f}x | {formatted} |")
host = last['host']
lines += ['',f"Measured {data['measuredDate']}, {host['ProcessorName']}, {host['OsVersion']},",
    f"SDK {host['DotNetCliVersion']}, runtime {host['RuntimeVersion']}. CPU 0 only,",
    'one codec/conversion thread, normal SIMD, optimized JIT with tiering disabled',
    'and non-concurrent workstation GC. Short independent screens guide priority',
    'and retain uncertainty.',
    f"Managed mean spread is {data['repeatability']['managedMeanSpreadFraction']*100:.3f}% relative to the smaller candidate mean.",'',
    'The same original 513x259 RGBA8 raster is flattened once on RGB 0x204060.',
    'Both intervals include full-range BT.601 conversion, 4:4:4 encode, independent',
    'output acquisition and release. Native contexts/scratch/wrappers are reused;',
    'every pixel is reprocessed. Managed MemoryStream creation, growth and',
    'disposal are timed. Startup, IPC, I/O, generation, hashing, checks and',
    'quality enumeration are excluded.', '',
    f"Frozen owned quality {managed['Setting']} / native qscale {native['Setting']} retain the",
    f"{quality['psnrGapDb']:.6f} dB source-error gap within the fixed 0.25 dB band.",
    f"Encoded sizes remain {managed['EncodedBytes']:,} / {native['EncodedBytes']:,} bytes.",
    'Exact hashes, decoder/alpha checks and the original seven owned settings',
    'remain unchanged. Managed fixed/native optimal Huffman policies are explicit.',
    'There is one original warm image and one encode case; no new preparation.', '',
    'Eight complete RGBA pixels occupy one safe 32-byte load. Integer lanes keep',
    'the original nearest encoded-space alpha blend. For its bounded numerator',
    '`0 <= n <= 65152`, division by 255 is exactly `(n + 1 + (n >> 8)) >> 8`.',
    'Each channel widens to binary64 before the existing color matrix; every',
    'multiply/add/subtract remains separate, followed by the existing exact byte',
    'rounding primitive. There is no FMA or reassociation. Only full-resolution',
    '8-bit complete rows use this dispatch. Padded right edges, 16-bit input,',
    'subsampled encoding and CPUs without the required intrinsics use the original',
    'path. Bottom-edge replication and component/block order remain unchanged.',
    'No buffer, retained row/pixel cache, table, dependency, working-byte admission',
    'or logical work is added.', '',
    f"Managed allocation was {baseline['managedMemory']['BytesAllocatedPerOperation']:,} bytes/op and is",
    f"{last['managedMemory']['BytesAllocatedPerOperation']:,} bytes/op in the last screen.",
    f"Observed changes are {data['allocationChange']['observedBytes']} bytes/op.",
    f"Automatic calibration uses {data['allocationChange']['actualManagedOperationsPerSample']['baseline']} baseline versus {data['allocationChange']['actualManagedOperationsPerSample']['componentRows']} candidate operations per actual sample.",
    'Direct controls through the actual baseline and measured DLLs retain equal',
    'allocation totals and identical sorted per-operation allocation values over',
    '16 complete encodes, with the same frozen output. The inclusive BDN diagnostic',
    'variation remains visible; no added library heap storage is observed.', '',
    'All 1,889 tests pass on Windows and Linux: 309 internal, 686 public and',
    '894 live FFmpeg comparisons. Four VM SIMD/scalar modes each pass 89 internal',
    'and 70 public JPEG controls. All 160 frozen encoder bitstreams pass across',
    'both source depths and all sampling modes. Thirteen new cases check every',
    'opaque RGB byte color for each component (50,331,648 reference samples),',
    '1,572,864 alpha/background reference samples, unaligned input/output guards,',
    'unchanged source rows, every transparency lane and invalid row/component',
    'rejection. BDN output smoke passes with the same frozen matched-quality bytes.', '',
    'A separate original 17x9 correctness input checks 28 complete operations',
    'through the baseline and candidate binaries on both platforms. Transparency',
    'in every lane and at block/right/bottom edges, work/memory/output limits',
    'and cancellation retain the exact error type/message/parameter, output prefix',
    'hash, unchanged source and writable caller stream. These controls make no',
    'timing claim and add no benchmark image.', '',
    'The same actual dependency-free Release package and all three usage examples',
    'pass both platforms. Package SHA256:',f"`{data['validation']['packageSha256']}`.", '',
    'Package and measured benchmark DLL hashes differ by build identity. Their',
    'method-body fingerprints and all 30 existing JPEG fixture outputs match',
    'through the actual binaries on the VM. These controls make no timing claim.', '',
    'Failed rolling-load samples and subsequent passing cooldowns remain recorded',
    'without relaxing thresholds. The earlier profile identifies unchanged scalar',
    'color/row work; it is not a post-quantization attribution. Profile warmup and',
    'calibration work is excluded from the parity rows.', '',
    'Reproduce sequentially on the quiet dedicated Linux VM following the',
    '[JPEG worker instructions](../tools/WarmReference/README.md#jpeg), using',
    'the frozen preparation, output smoke and two independent warm Jpeg repeats.', '']
(root/'WARM-JPEG-COMPONENT-ROW.md').write_text('\n'.join(lines),encoding='utf-8',newline='\n')
