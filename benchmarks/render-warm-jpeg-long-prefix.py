"""Render original screens for the adaptive nine/ten-bit Huffman lookup."""
import json
from pathlib import Path
import statistics

root = Path(__file__).resolve().parent
data = json.loads((root/'warm-jpeg-long-prefix-evidence.json').read_text())
observations = data['observations']
lines = ['# Warm JPEG adaptive long-prefix lookup', '',
    'One production primitive adds an input-derived lookup for nine/ten-bit',
    'Huffman codes after the existing eight-bit lookup. Other lengths retain',
    'the scalar reader. Tables without those codes keep their original lookup',
    'payload and construction work. The selected native-input gap improves',
    'but remains above 2x; this does not establish uniform parity.', '',
    'The [original numeric evidence](warm-jpeg-long-prefix-evidence.json) retains',
    'managed actual samples, statistics and allocations, native brackets, source',
    'and binary identities, exact input/output hashes, syntax, controls and',
    'quietness. Regenerate with `python benchmarks/render-warm-jpeg-long-prefix.py`.', '',
    '| Input / stage | Repeat | Managed mean, µs | Native before / after, µs | Worst bracket ratio | Conservative upper |',
    '| --- | ---: | ---: | ---: | ---: | ---: |']
for key, label in [('baseline', 'Native / baseline'), ('additionalPrefix', 'Native / additional lookup'), ('ownedAffected', 'Owned / affected screen')]:
    for index, row in enumerate(observations[key], 1):
        native = [statistics.mean(v['NanosecondsPerOperation'] for v in row[key])/1000 for key in ('nativeBefore', 'nativeAfter')]
        upper = row['conservativeRatioUpper']; formatted = f'{upper:.2f}x' if upper is not None else 'unbounded'
        lines.append(f"| {label} | {index} | {row['managedStatistics']['Mean']/1000:.2f} | {native[0]:.2f} / {native[1]:.2f} | {row['worstBracketRatio']:.2f}x | {formatted} |")
last = observations['additionalPrefix'][-1]; host = last['host']
lines += ['', f"Measured {data['measuredDate']}, {host['ProcessorName']}, {host['OsVersion']},",
    f"SDK {host['DotNetCliVersion']}, runtime {host['RuntimeVersion']}. CPU 0 only,",
    'one codec/conversion thread, normal SIMD, optimized JIT with tiering disabled',
    'and non-concurrent workstation GC. Native-input managed mean spread is',
    f"{data['repeatability']['managedMeanSpreadFraction']*100:.1f}%; the earlier baseline spread remains recorded.",
    'The owned input has one affected screen, not a new confirmation. These',
    'short screens guide priority and retain uncertainty.', '',
    'Both frozen JPEGs come from one existing original 513x259 RGBA8 raster,',
    'flattened once on RGB 0x204060. No additional source raster, upstream corpus,',
    'preparation or quality enumeration is used. Each decoder pair receives',
    'identical resident bytes. Managed pixels remain exactly frozen; native pixels',
    'match the pinned public CLI with normal SIMD. RGB maximum 4/RMSE 1.5 and',
    'exact opaque alpha remain required.', '',
    'Both intervals include full decode/conversion, independent owned output',
    'allocation and release. Native context/conversion/input packet/frame wrappers',
    'are reused, with state reset and every pixel decoded/converted each call.',
    'Padded native rows need no tight-row copy. Managed public Decode and disposal',
    'are timed. Startup, IPC, I/O, generation, hashing, checks and file packing',
    'are excluded.', '',
    'The additional lookup is generated from each encoded canonical table under',
    'T.81 C.2. It admits 2,048 payload bytes and 1,024 construction-work units',
    'before allocation. The native input has two such AC tables, adding 4,096',
    'admitted bytes and 2,048 construction units. The fixed-code owned input has',
    'none. Observed x64 allocation increases by 4,176 bytes: the 4,096-byte',
    'payload, 48 bytes of array headers and 32 bytes of additional table fields.',
    'The owned input adds only 16 bytes for its two table fields. Every symbol',
    'retains its original code-bit work. Non-mutating lookahead',
    'commits only through the last byte required by a complete code; cached',
    'padding, markers, truncation and tight budgets keep scalar failure precedence.', '',
    'All 1,861 tests pass on Windows and Linux: 281 internal, 686 public and',
    '894 live FFmpeg comparisons. On the VM, 61 internal and 70 public JPEG',
    'controls pass separately with normal SIMD, AVX-512 disabled, AVX disabled',
    'and all intrinsics disabled. The 30 existing original JPEG fixtures retain',
    'exact owned pixels against the previous library. New controls cover nine/',
    'ten-bit code padding/positions/work, additional storage admission, partial',
    'budgets and marker failure precedence. BDN output smoke passes.', '',
    'The same actual dependency-free Release package passes verification and',
    'all three usage-guide examples on both platforms. Package SHA256:',
    f"`{data['validation']['packageSha256']}`.", '',
    'Package and benchmark builds have separate DLL identities. On the VM, their',
    'method-body fingerprints match and the same 30 existing fixtures reproduce',
    'identical pixels through both actual binaries. These are correctness checks',
    'without timing; both DLL hashes remain in the evidence.', '',
    f"Native-input managed allocation is {last['managedMemory']['BytesAllocatedPerOperation']:,} bytes per operation.",
    'This includes the additional lookups; allocation observations remain in the',
    'numeric evidence. Failed rolling-load samples and later passing cooldowns',
    'are retained without relaxing the quietness gate. The baseline workload',
    f"profile attributed {data['baselineAttribution']['inclusivePrimitivePercent']['JpegHuffman']:.2f}% to entropy/Huffman work;",
    'its timings are excluded from the parity rows.', '',
    'Reproduce sequentially on the quiet dedicated Linux VM following the',
    '[worker instructions](../tools/WarmReference/README.md#jpeg-decode).', '']
(root/'WARM-JPEG-LONG-PREFIX.md').write_text('\n'.join(lines), encoding='utf-8', newline='\n')
