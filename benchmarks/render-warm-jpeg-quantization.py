"""Render original quiet-VM observations for safe coefficient quantization."""
import json
from pathlib import Path
import statistics

root = Path(__file__).resolve().parent
data = json.loads((root/'warm-jpeg-quantization-evidence.json').read_text())
baseline = data['observations']['baseline'][0]; last = data['observations']['quantization'][-1]
quality = last['quality']; managed = quality['managedQuality']; native = quality['nativeQuality']
lines = ['# Warm single-CPU JPEG coefficient quantization', '',
    'One production primitive divides and rounds eight coefficients together with',
    'safe AVX-512F/AVX2 intrinsics. Every binary64 quotient, away-from-zero half',
    'tie and encoded byte remains unchanged. Scalar fallback is retained.',
    'The retained screens remain above 2x; uniform parity is not established.', '',
    'The [original numeric evidence](warm-jpeg-quantization-evidence.json) retains',
    'actual samples, statistics, native brackets, quality, exact output hashes,',
    'source/tool/DLL identities, allocation, correctness controls and quietness.',
    'Regenerate with `python benchmarks/render-warm-jpeg-quantization.py`.', '',
    '| Phase | Repeat | Managed mean, µs | Native before / after, µs | Worst bracket ratio | Conservative upper |',
    '| --- | ---: | ---: | ---: | ---: | ---: |']
for phase, rows in data['observations'].items():
    for i, row in enumerate(rows, 1):
        brackets = [statistics.mean(v['NanosecondsPerOperation'] for v in row[key])/1000 for key in ('nativeBefore','nativeAfter')]
        upper = row['conservativeRatioUpper']; formatted = f'{upper:.2f}x' if upper is not None else 'unbounded'
        lines.append(f"| {phase} | {i} | {row['managedStatistics']['Mean']/1000:.2f} | {brackets[0]:.2f} / {brackets[1]:.2f} | {row['worstBracketRatio']:.2f}x | {formatted} |")
host = last['host']
lines += ['', f"Measured {data['measuredDate']}, {host['ProcessorName']}, {host['OsVersion']},",
    f"SDK {host['DotNetCliVersion']}, runtime {host['RuntimeVersion']}. CPU 0 only,",
    'one codec/conversion thread, normal SIMD, optimized JIT with tiering disabled',
    'and non-concurrent workstation GC. These short independent screens guide',
    'priority and retain uncertainty.',
    f"Managed mean spread is {data['repeatability']['managedMeanSpreadFraction']*100:.3f}% relative to the smaller candidate mean.", '',
    'The same original 513x259 RGBA8 raster is flattened once on RGB 0x204060.',
    'Both intervals include full-range BT.601 conversion, 4:4:4 encode, independent',
    'output acquisition and release. Native contexts/scratch/wrappers are reused;',
    'every pixel is reprocessed. Managed MemoryStream creation, growth and',
    'disposal are timed. Startup, IPC, I/O, generation, hashing, checks and',
    'quality enumeration are excluded.', '',
    f"Frozen owned quality {managed['Setting']} / native qscale {native['Setting']} retain the",
    f"{quality['psnrGapDb']:.6f} dB source-error gap within the fixed 0.25 dB band.",
    f"Encoded sizes remain {managed['EncodedBytes']:,} / {native['EncodedBytes']:,} bytes, with identical",
    'hashes and decoder/alpha checks. Managed fixed/native optimal Huffman',
    'policies remain explicit. Preparation retains seven original owned settings',
    'and one source; no setting, image or preparation is added.', '',
    'Quantization follows [T.81](https://www.w3.org/Graphics/JPEG/itu-t81.pdf)',
    'F.1.1.4 and A.3.4 with the original owned binary64 rounding policy.',
    'Each exact ushort quantizer is widened to binary64 before division. The',
    'quotient magnitude is floored and incremented only if its exact fractional',
    'part is at least one half; the original sign is then restored. Bounded',
    'encoder results convert to Int32 and copy directly to the existing block.',
    'Its 256-byte destination uses a safe stack span within the already admitted',
    '8,192-byte fixed workspace. No additional working storage is admitted.',
    'Transform, color/alpha, sample/entropy order, quality, work/admission and',
    'cancellation remain unchanged. There is no reciprocal approximation, new',
    'table, buffer, allocation, cache or dependency.', '',
    f"Managed allocation was {baseline['managedMemory']['BytesAllocatedPerOperation']:,} bytes/op and is",
    f"{last['managedMemory']['BytesAllocatedPerOperation']:,} bytes/op in the last candidate screen.",
    f"Observed changes are {data['allocationChange']['observedBytes']} bytes/op; all diagnostics remain recorded.", '',
    'The initial helper variant prevented JIT stack promotion of the existing',
    '64-int destination, adding exactly 280 heap bytes per operation: 256 payload',
    'bytes and the 24-byte x64 array header. Its JIT listings and direct allocation',
    'checks confirm the cause. Both initial timing screens and the failed strict',
    'allocation control remain recorded. Explicit safe stack storage restores',
    'identical direct allocation totals over 16 complete encodes through the',
    'baseline and final measured DLL. These controls make no timing claim.', '',
    'All 1,876 tests pass on Windows and Linux: 296 internal, 686 public and',
    '894 live FFmpeg comparisons. Four VM SIMD/scalar modes each pass 76 internal',
    'and 70 public JPEG controls. All 160 frozen encoder bitstreams pass across',
    'both source depths and all sampling modes. Eight new cases check all 255',
    'encoder quantizers at 76,788 boundary inputs (85,184 scalar-reference lanes',
    'including padding), signed zero/subnormals, eight mixed-lane rotations,',
    'unchanged inputs, output guards and incomplete-block rejection. Original',
    'output/memory/work limits, cancellation and caller ownership pass.',
    'BDN output smoke passes with the same frozen matched-quality bytes.', '',
    'The same actual dependency-free Release package and all three usage examples',
    'pass both platforms. Package SHA256:', f"`{data['validation']['packageSha256']}`.", '',
    'Package and measured benchmark DLL hashes differ by build identity. Their',
    'method-body fingerprints and all 30 existing JPEG fixture outputs match',
    'through the actual binaries on the VM. These controls make no timing claim.', '',
    'Failed rolling-load samples and later passing cooldowns are retained',
    'without relaxing thresholds. A separate baseline workload profile attributed',
    '55.36% to Encode leaf samples, 16.96% to forward DCT and 16.96% inclusively',
    'to entropy writing. The profile combines work inside Encode; it does not',
    'attribute a separate percentage to quantization. Its warmup/calibration',
    'samples are excluded from the parity rows.', '',
    'Reproduce sequentially on the quiet dedicated Linux VM following the',
    '[JPEG worker instructions](../tools/WarmReference/README.md#jpeg), using',
    'the frozen preparation, output smoke and two independent warm Jpeg repeats.', '']
(root/'WARM-JPEG-QUANTIZATION.md').write_text('\n'.join(lines), encoding='utf-8', newline='\n')
