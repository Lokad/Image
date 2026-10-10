"""Render retained single-CPU warm JPEG canonical-prefix observations."""
import json
from pathlib import Path
import statistics

root = Path(__file__).resolve().parent
data = json.loads((root/'warm-jpeg-canonical-prefix-evidence.json').read_text())
observations = data['observations']
lines = ['# Warm JPEG canonical fallback prefix', '',
    'One production primitive consumes eight Huffman prefix bits together when',
    'no short code is assigned, then continues canonical traversal. Pixels and failure boundaries',
    'remain unchanged. Both retained gaps exceed 2x; these short screens do not',
    'establish uniform parity.', '',
    'The [original numeric evidence](warm-jpeg-canonical-prefix-evidence.json)',
    'retains samples, statistics, allocation diagnostics, native brackets, source',
    'and binary identities, frozen input/output hashes, controls and quietness.',
    'Regenerate with `python benchmarks/render-warm-jpeg-canonical-prefix.py`.', '',
    '| Input / stage | Repeat | Managed mean, µs | Native before / after, µs | Worst bracket ratio | Conservative upper |',
    '| --- | ---: | ---: | ---: | ---: | ---: |']
for key,label in [('baseline','Native / baseline'),('canonicalPrefix','Native / canonical prefix'),('ownedBaseline','Owned / baseline screen'),('ownedAffected','Owned / affected screen')]:
    for index,row in enumerate(observations[key],1):
        native = [statistics.mean(v['NanosecondsPerOperation'] for v in row[k])/1000 for k in ('nativeBefore','nativeAfter')]
        upper = row['conservativeRatioUpper']; formatted = f'{upper:.2f}x' if upper is not None else 'unbounded'
        lines.append(f"| {label} | {index} | {row['managedStatistics']['Mean']/1000:.2f} | {native[0]:.2f} / {native[1]:.2f} | {row['worstBracketRatio']:.2f}x | {formatted} |")
last = observations['canonicalPrefix'][-1]; host = last['host']; change = data['allocationChange']
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
    'After existing short/long lookup misses, a complete eight-bit prefix is checked',
    'against the existing short-code table. A zero entry proves no assigned code',
    'of length eight or less. Admit its original eight logical work units, commit',
    'only its required bytes and continue canonical traversal at length nine.',
    'The canonical codes follow [T.81 C.2](https://www.w3.org/Graphics/JPEG/itu-t81.pdf).',
    'Assigned short codes, markers, truncation and tight budgets retain the original',
    'one-bit fallback without speculative state mutation. Padding, restart markers,',
    'symbols/amplitudes, error/source/work positions, cancellation, decoded pixels',
    'and independent ownership remain unchanged. No storage, table, cache,',
    'allocation or dependency is added.', '',
    'All 1,930 tests pass on Windows and Linux: 350 internal, 686 public and',
    '894 live FFmpeg comparisons. Four VM SIMD/scalar modes each pass 130 internal',
    'and 70 public JPEG controls. Fifteen new cases cover 48 cached phases of',
    '11 to 16-bit canonical codes (144 exact symbols), padding and restart positions.',
    'All 128 partial long-code work budgets and 64 short-code budget/phase pairs',
    'retain exact byte positions. Six marker/truncation inputs and 16 cached',
    'marker/budget phases retain failure messages, source position and work.',
    'Thirty original fixtures retain exact pixels; all 160 frozen encoder bitstreams',
    'remain unchanged. Output smoke and FFmpeg CLI pixel controls pass.', '',
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
    'Failed quietness samples and later passing cooldowns are retained. A separate',
    'current baseline profile ranks Huffman.Read at 23.04% leaf, inverse DCT at',
    '20.37% and ReadBits at 13.01%. The coefficient/entropy group spans several',
    'methods (52.73% inclusive). Method matching excludes parameter types.',
    'These are baseline CPU samples, not candidate percentages or parity timings;',
    'profile workload includes warmup/calibration.', '',
    'Reproduce sequentially on the quiet dedicated VM using the',
    '[worker instructions](../tools/WarmReference/README.md#jpeg-decode).', '']
(root/'WARM-JPEG-CANONICAL-PREFIX.md').write_text('\n'.join(lines),encoding='utf-8',newline='\n')
