"""Render original single-pair coefficient-category screens."""
import json
from pathlib import Path
import statistics

root = Path(__file__).resolve().parent
data = json.loads((root/'warm-jpeg-category-evidence.json').read_text())
baseline = data['observations']['baseline'][0]
last = data['observations']['categories'][-1]
quality = last['quality']; managed = quality['managedQuality']; native = quality['nativeQuality']
lines = ['# Warm single-CPU JPEG magnitude-category round', '',
    'One production primitive replaces the coefficient magnitude-category shift',
    'loop with the BCL leading-zero primitive. Hardware lowering is used where',
    'available, with a safe software fallback. Math.Abs retains the original',
    'Int32 minimum overflow; zero remains category zero. Color/quantization math,',
    'sample/entropy order, work/admission/cancellation and failure behavior remain',
    'unchanged. No table, allocation, storage, cache or dependency is added.', '',
    'The [original numeric evidence](warm-jpeg-category-evidence.json) retains',
    'actual samples/statistics, native brackets, quality, exact encoded outputs,',
    'source/tool/DLL identities, allocations, validation and quietness. Regenerate',
    'with `python benchmarks/render-warm-jpeg-category.py`.', '',
    '| Phase | Repeat | Managed mean, µs | Native before / after, µs | Worst bracket ratio | Conservative upper |',
    '| --- | ---: | ---: | ---: | ---: | ---: |']
for phase, rows in data['observations'].items():
    for i, row in enumerate(rows, 1):
        brackets = [statistics.mean(v['NanosecondsPerOperation'] for v in row[key])/1000 for key in ('nativeBefore', 'nativeAfter')]
        upper = row['conservativeRatioUpper']; formatted = f'{upper:.2f}x' if upper is not None else 'unbounded'
        lines.append(f"| {phase} | {i} | {row['managedStatistics']['Mean']/1000:.2f} | {brackets[0]:.2f} / {brackets[1]:.2f} | {row['worstBracketRatio']:.2f}x | {formatted} |")
host = last['host']
lines += ['', f"Measured {data['measuredDate']}, {host['ProcessorName']}, {host['OsVersion']},",
    f"SDK {host['DotNetCliVersion']}, runtime {host['RuntimeVersion']}. CPU 0 only,",
    'one codec/conversion thread, normal SIMD, optimized JIT with tiering disabled',
    'and non-concurrent workstation GC. These short independent screens retain',
    'uncertainty and guide priority; uniform parity is not established.',
    f"Managed mean spread is {data['repeatability']['managedMeanSpreadFraction']:.1%} relative to the smaller candidate mean.", '',
    'The same original 513x259 RGBA8 raster is flattened once on RGB 0x204060.',
    'Both intervals include complete full-range BT.601 conversion, 4:4:4 encode,',
    'independent output acquisition and release. Native contexts/scratch/wrappers',
    'are reused, with every pixel reprocessed. Managed MemoryStream creation,',
    'growth and disposal are timed. Startup, IPC, I/O, generation, hashing,',
    'checks and quality enumeration are excluded.', '',
    f"Frozen owned quality {managed['Setting']} / native qscale {native['Setting']} retain the",
    f"{quality['psnrGapDb']:.6f} dB source-error gap within the fixed 0.25 dB band.",
    f"Encoded sizes remain {managed['EncodedBytes']:,} / {native['EncodedBytes']:,} bytes, with identical",
    'encoded hashes and decoder/alpha checks. Managed fixed/native optimal Huffman',
    'policies remain explicit. Preparation uses its original seven owned settings',
    'and one source; this round adds neither settings nor images.', '',
    f"Managed allocation is {baseline['managedMemory']['BytesAllocatedPerOperation']:,} before and",
    f"{last['managedMemory']['BytesAllocatedPerOperation']:,} bytes per operation after. Diagnostic variation",
    'is retained; this primitive adds no allocation or admission cost.', '',
    'All 1,863 tests pass on Windows/Linux: 283 internal, 686 public and 894 live',
    'FFmpeg comparisons. Four SIMD/scalar VM modes each pass 63 internal and 70',
    'public JPEG controls. All 160 frozen encoder bitstreams pass across both',
    'input depths and all sampling modes. Two additional controls cover all',
    '131,071 signed JPEG magnitude values, all 31 integer category edges and',
    'the original Int32 minimum overflow. Output/memory limits, cancellation,',
    'caller stream ownership and unchanged input pass. BDN output smoke passes.', '',
    'The same actual dependency-free Release package and all three usage examples',
    'pass both platforms. Package SHA256:', f"`{data['validation']['packageSha256']}`.", '',
    'The package and measured benchmark DLL have separate binary hashes. Their',
    'method-body fingerprints match, and the same 30 existing JPEG fixtures yield',
    'identical pixels through both actual binaries on the VM.', '',
    'Failed post-run rolling-load samples and later passing cooldowns remain',
    'without relaxed thresholds. A separate workload profile attributed 6.79%',
    'to the category loop, 50.23% to Encode leaf samples and 16.77% to forward DCT.',
    'Its trace and source identity are retained; profile timing is excluded from',
    'the parity rows.', '',
    'Reproduce sequentially on the quiet dedicated Linux VM following the',
    '[JPEG worker instructions](../tools/WarmReference/README.md#jpeg), using',
    'the frozen preparation, smoke and two independent warm Jpeg repeats.', '']
(root/'WARM-JPEG-CATEGORY.md').write_text('\n'.join(lines), encoding='utf-8', newline='\n')
