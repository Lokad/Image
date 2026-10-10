"""Render the original one-primitive Huffman prefix-lookup round."""
import json
from pathlib import Path
import statistics

root=Path(__file__).resolve().parent
data=json.loads((root/'warm-jpeg-prefix-evidence.json').read_text())
baseline=data['observations']['baseline'][0]; last=data['observations']['prefix'][-1]
lines=['# Warm single-CPU JPEG Huffman lookup round','',
    'This round changes one production primitive: canonical Huffman decoding.',
    'Each encoded table generates its own eight-bit prefix lookup, following',
    'T.81 C.2. Each table admits 512 additional bytes and 256 construction work',
    'units before allocation. Valid short codes charge their original logical',
    'bit count together; the original scalar reader handles long or unavailable',
    'codes, markers, truncation and insufficient work budgets.','',
    'Cached complete codes inspect no additional byte. Otherwise at most one',
    'unstuffed byte is inspected without mutating reader state. Only a valid',
    'admitted short code commits that state. Near the work limit, the original',
    'per-bit walk preserves partial consumption and format/limit precedence.',
    'Cancellation remains checked per code, with at most eight logical bits',
    'admitted together. Tables are rebuilt on every public Decode; no pixels or',
    'outputs are cached.','',
    'It reuses the single [warm JPEG decode pair](WARM-JPEG-DECODE.md). The',
    '[original numeric evidence](warm-jpeg-prefix-evidence.json) retains both',
    'accepted inverse-DCT baseline repeats and lookup repeats, raw statistics,',
    'native brackets, allocations, identities and quietness. Regenerate with',
    '`python benchmarks/render-warm-jpeg-prefix.py`.','',
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
    'one codec/conversion thread, normal native SIMD, optimized managed JIT',
    'with tiering disabled and non-concurrent workstation GC. Two short repeats',
    'guide priority; uniform parity is not established.','',
    'Both subjects decode the same resident 324,002-byte baseline 8-bit 4:4:4',
    'JPEG to independent RGBA8, including full-range BT.601 conversion, allocation',
    'and release. Native state reset is timed; contexts/wrappers are reused but',
    'every call decodes and converts all pixels. Padded native output needs no',
    'tight-row copy. Public managed Decode and disposal are timed. Startup, I/O,',
    'IPC, validation, hashing, generation and packing writes are excluded.','',
    'No quality enumeration or additional raster is added. Input and both output',
    'hashes remain unchanged. Owned pixels equal the frozen preparation; native',
    'pixels equal the pinned FFmpeg CLI. RGB agreement retains maximum 4/RMSE 1.5',
    'bounds and exact opaque alpha, with independent live output storage.','',
    f"Managed BDN allocation is {baseline['managedMemory']['BytesAllocatedPerOperation']:,} before and",
    f"{last['managedMemory']['BytesAllocatedPerOperation']:,} bytes per operation after. Two fresh tables",
    'add 1,024 admitted payload bytes and 512 construction work units to this',
    'pair. Their array/object overhead accounts for 1,088 actual allocated bytes;',
    'raw BDN allocation counts also retain sampling/diagnostic variation.',
    'Exact memory/work boundaries include these costs, including replaced and',
    'unused Huffman definitions.','',
    'Windows/Linux full Release/live suites pass 1,855 tests (275 internal,',
    '686 public, 894 FFmpeg comparisons). Each of normal, AVX-512 disabled, AVX',
    'disabled and all intrinsics disabled modes passes 55 internal and 70 public',
    'JPEG controls on Linux. Original canonical/long-code, padding/restart,',
    'partial-work and malformed-code controls pass; three added controls cover',
    'mixed cached short codes, storage admission and marker/error precedence.',
    'Frozen scalar transform output and all 160 encoded JPEG bitstreams pass.',
    'The same actual dependency-free Release package and all three usage examples',
    f"pass both OS: SHA256 `{data['fullValidation']['packageSha256']}`.",'',
    'Source-byte and pre-run quietness checks pass. Failed post-run rolling-load',
    'checks and later passing cooldowns remain without weakened thresholds.',
    'The separate accepted-inverse workload profile attributed 57.05% inclusively',
    'to entropy reading and 30.55% to ReadBits leaf samples. Profile timing is',
    'excluded from parity evidence; remaining primitives need fresh attribution.','',
    'Reproduce sequentially on the quiet dedicated Linux VM using the',
    '[worker instructions](../tools/WarmReference/README.md#jpeg-decode), the',
    'frozen preparation, output smoke and two independent warm JpegDecode repeats',
    'with a passing cooldown between them.','']
(root/'WARM-JPEG-PREFIX.md').write_text('\n'.join(lines),encoding='utf-8',newline='\n')
