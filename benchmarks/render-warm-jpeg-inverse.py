"""Render the original one-primitive inverse-DCT round."""
import json
from pathlib import Path
import statistics

root=Path(__file__).resolve().parent
data=json.loads((root/'warm-jpeg-inverse-evidence.json').read_text())
baseline=data['observations']['baseline'][0]; last=data['observations']['simd'][-1]
lines=['# Warm single-CPU JPEG inverse-DCT round','',
    'This round changes one production primitive: inverse DCT. Safe AVX-512',
    'eight-pixel or AVX four-pixel lanes preserve the frozen owned binary64 basis,',
    'scalar v/u accumulation order, separate multiplications, scalar final',
    'rounding/clamping and 4096 logical work units. The scalar fallback remains.',
    'The only added storage is a 512-byte static transpose of the existing basis.',
    'There is no per-block/image allocation or cache.','',
    'It reuses the single [warm JPEG decode pair](WARM-JPEG-DECODE.md). The',
    '[original numeric evidence](warm-jpeg-inverse-evidence.json) retains both',
    'baseline and candidate raw samples/statistics, native brackets, identities,',
    'output pixels, allocations and quietness. Regenerate this report with',
    '`python benchmarks/render-warm-jpeg-inverse.py`.','',
    '| Phase | Repeat | Managed mean, µs | Native before / after, µs | Worst bracket ratio | Conservative upper |',
    '| --- | --- | ---: | ---: | ---: | ---: |']
for phase,rows in data['observations'].items():
    for i,row in enumerate(rows,1):
        native=[statistics.mean(v['NanosecondsPerOperation'] for v in row[key])/1000 for key in ('nativeBefore','nativeAfter')]
        upper=row['conservativeRatioUpper']; formatted=f'{upper:.2f}x' if upper is not None else 'unbounded'
        lines.append(f"| {phase} | {i} | {row['managedStatistics']['Mean']/1000:.2f} | {native[0]:.2f} / {native[1]:.2f} | {row['worstBracketRatio']:.2f}x | {formatted} |")
host=last['host']
lines+=['',f"Measured {data['measuredDate']}, {host['ProcessorName']}, {host['OsVersion']},",
    f"SDK {host['DotNetCliVersion']}, runtime {host['RuntimeVersion']}. CPU 0 only,",
    'one codec/conversion thread, normal native SIMD and optimized managed JIT',
    'with tiering disabled and non-concurrent workstation GC. Two short repeats',
    'guide priority and retain uncertainty; uniform parity is not established.','',
    'Both intervals decode the same resident 324,002-byte baseline 8-bit 4:4:4',
    'JPEG to independent RGBA8, including full-range BT.601 conversion, allocation',
    'and release. Native state reset is timed; contexts/wrappers are reused but',
    'all pixels are decoded/converted each call. Padded native output needs no',
    'tight-row copy. Public managed Decode and disposal are timed. Startup, I/O,',
    'IPC, validation, hashing, generation and packing writes are excluded.','',
    'No encoder-quality enumeration or additional source raster is added. Frozen',
    'prepared owned pixels remain exact, and the input/managed/native output',
    'hashes are unchanged across both rounds. Native pixels match the public',
    'FFmpeg CLI exactly. RGB agreement stays within maximum 4/RMSE 1.5, opaque',
    'alpha is exact, and both subjects retain independent live output storage.','',
    f"Managed BDN allocation is {baseline['managedMemory']['BytesAllocatedPerOperation']:,} before and",
    f"{last['managedMemory']['BytesAllocatedPerOperation']:,} bytes per operation after. Work/memory admission,",
    'cancellation, input integrity, owned pixels and all 160 frozen encoder',
    'bitstreams remain unchanged. Nineteen original controls freeze scalar output',
    'for half ties, clipping, dense/extreme coefficients and ushort quantizers,',
    'and check guarded output, immutable inputs and work/cancellation boundaries.','',
    'Windows/Linux full Release/live suites pass 1,841 tests (261 internal,',
    '686 public, 894 FFmpeg comparisons). Each of normal, AVX-512 disabled, AVX',
    'disabled and all intrinsics disabled modes passes 41 internal and 70 public',
    'JPEG controls on Linux. The same actual dependency-free Release package and',
    'all three usage examples pass both OS:',
    f"SHA256 `{data['fullValidation']['packageSha256']}`.",'',
    'Source-byte and pre-run quietness checks pass. Failed post-run rolling-load',
    'checks and later passing cooldowns remain recorded without weakened limits.',
    'The baseline workload-only profile attributed 54.05% to inverse DCT; it is',
    'kept separate from these parity measurements. Remaining decoder primitives',
    'need fresh attribution, and other families retain their separate open rows.','',
    'Reproduce sequentially on the quiet dedicated Linux VM following the',
    '[worker instructions](../tools/WarmReference/README.md#jpeg-decode),',
    'reusing the frozen preparation, then running smoke and two independent warm',
    'JpegDecode repeats with a passing cooldown between them.','']
(root/'WARM-JPEG-INVERSE.md').write_text('\n'.join(lines),encoding='utf-8',newline='\n')
