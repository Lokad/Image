"""Render original warm Huffman wrapper observations."""
import json,statistics
from pathlib import Path
root=Path(__file__).resolve().parent
data=json.loads((root/'warm-jpeg-huffman-hot-evidence.json').read_text(encoding='utf-8'))
rows=data['observations']; change=data['allocationChange']
lines=['# Warm JPEG short-code wrapper','',
    'One production primitive moves long/canonical fallback out of the common',
    'Huffman wrapper, allowing the caller to inline its short-code probe.',
    'Warm JPEG decode improves, while both retained gaps remain above 2x.','',
    'The [numeric evidence](warm-jpeg-huffman-hot-evidence.json) retains actual',
    'samples, uncertainty, native brackets, allocations, input/output hashes,',
    'source identities, controls and quietness observations. Regenerate with',
    '`python benchmarks/render-warm-jpeg-huffman-hot.py`.','',
    '| Input / stage | Repeat | Managed mean, µs | Native before / after, µs | Worst bracket ratio | Conservative upper |',
    '| --- | ---: | ---: | ---: | ---: | ---: |']
for key,label in [('baseline','Native / retained baseline'),('huffmanHot','Native / short wrapper'),('ownedBaseline','Owned / retained baseline'),('ownedAffected','Owned / affected screen')]:
    for index,row in enumerate(rows[key],1):
        native=[statistics.mean(value['NanosecondsPerOperation'] for value in row[k])/1000 for k in ('nativeBefore','nativeAfter')]
        upper=row['conservativeRatioUpper']; formatted=f'{upper:.2f}x' if upper is not None else 'unbounded'
        lines.append(f"| {label} | {index} | {row['managedStatistics']['Mean']/1000:.2f} | {native[0]:.2f} / {native[1]:.2f} | {row['worstBracketRatio']:.2f}x | {formatted} |")
host=rows['huffmanHot'][-1]['host']; rejected=data['rejectedCanonicalSymbol']['observations'][0]
lines+=['',f"Measured {data['measuredDate']}, {host['ProcessorName']}, {host['OsVersion']},",
    f"SDK {host['DotNetCliVersion']}, runtime {host['RuntimeVersion']}. CPU 0, one",
    'codec/conversion thread, normal SIMD, tiering disabled and non-concurrent',
    'workstation GC. Runs are sequential. Native-input managed mean spread:',
    f"{data['repeatability']['managedMeanSpreadFraction']*100:.3f}%. The owned input has one affected screen only.",'',
    'Two frozen JPEGs use one existing original 513x259 RGBA8 raster. No new image,',
    'preparation or quality enumeration is used. Each pair receives identical',
    'resident compressed bytes. Complete public decode, conversion, independent',
    'owned output allocation and disposal are timed. Reused native contexts retain',
    'timed reset and every decoded/converted pixel. Startup, IPC, I/O, generation,',
    'hashing and correctness checks are excluded.','',
    'The existing short-code probe now has a small wrapper marked for inlining.',
    'Its long-code lookup and canonical traversal move unchanged into a helper.',
    'Current baseline generated code showed a saved-register prologue on every',
    'Read call. The candidate baseline-block caller contains the short probe and',
    'fallback helper call directly, with no separate Read-wrapper call. Both actual',
    'package and measured DLL retain the inlining flag and matching method bodies.',
    'Code generation controls use original fixture inputs and make no timing claim.','',
    'Tables remain independently derived under',
    '[T.81 C.2](https://www.w3.org/Graphics/JPEG/itu-t81.pdf). Allocation, table',
    'construction/admission, logical work, cancellation, source/error positions,',
    'padding/restarts and scalar fallback are unchanged. No storage, dependency,',
    'unsafe code or decoded cache is added.','',
    'A preceding complete canonical-symbol candidate was discarded after one',
    f"{rejected['managedStatistics']['Mean']/1000:.2f} µs screen ({rejected['worstBracketRatio']:.2f}x). It had no useful gain;",
    'its repeat and affected-input screen were omitted. Original evidence remains.','',
    'All 2,000 tests pass on Windows and Linux: 420 internal, 686 public and',
    '894 live FFmpeg comparisons. Thirty-three canonical cases include sixteen',
    'mixed cached phases, 288 partial short-code budgets with long tables and all',
    '800 partial nine-to-sixteen-bit budget/phase combinations. Complete long symbols,',
    'marker/truncation/work precedence, invalid prefixes, source positions, exact',
    'table admission and cancellation remain checked. Output smoke and native CLI',
    'checks pass. Frozen outputs match the retained baseline; fixture corpus unchanged.','',
    'The same actual dependency-free Release package passes verification and all',
    'three usage examples on both platforms. Its method-body fingerprint and every',
    'pixel of thirty original JPEG fixtures match the actual measured DLL. SHA256:',
    f"`{data['validation']['packageSha256']}`.",'',
    f"Inclusive BDN allocation changes: {change['nativeObservedBytes']} bytes/op for native repeats,",
    f"{change['ownedObservedBytes']} bytes/op for the owned screen. Full diagnostics remain visible.",'',
    'Failed post-run quietness observations and later passing cooldowns are retained.',
    'The separate baseline CPU profile precedes this wrapper change. Its workload',
    'includes warmup/calibration; it is not parity timing or candidate attribution.',
    'Short screens guide ranking and retain both managed and native uncertainty.',
    'Uniform warm parity remains open.','',
    'Reproduce sequentially with the [worker instructions](../tools/WarmReference/README.md#jpeg-decode).','']
(root/'WARM-JPEG-HUFFMAN-HOT.md').write_text('\n'.join(lines),encoding='utf-8',newline='\n')
