"""Render the original narrow warm JPEG decode screens."""
import json
from pathlib import Path
import statistics

root=Path(__file__).resolve().parent
data=json.loads((root/'warm-jpeg-decode-evidence.json').read_text())
rows=data['observations']; last=rows[-1]
lines=['# Warm single-CPU JPEG decode screen','',
    'The existing prepared 513x259 baseline 8-bit 4:4:4 JPEG is decoded from',
    'identical resident bytes to independent owned RGBA8 on both subjects.',
    'This round adds one benchmark direction and changes no production primitive.',
    'The selected decoder is above 2x and becomes the largest measured gap.','',
    'The [original numeric evidence](warm-jpeg-decode-evidence.json) retains managed',
    'samples/statistics, native elapsed/count brackets, source/tool/DLL identities,',
    'output hashes, decoder agreement, allocations and quietness. Regenerate with',
    '`python benchmarks/render-warm-jpeg-decode.py`.','',
    '| Repeat | Managed mean, µs | Native before / after, µs | Worst bracket ratio | Conservative upper |',
    '| --- | ---: | ---: | ---: | ---: |']
for i,row in enumerate(rows,1):
    native=[statistics.mean(v['NanosecondsPerOperation'] for v in row[key])/1000 for key in ('nativeBefore','nativeAfter')]
    upper=row['conservativeRatioUpper']; formatted=f'{upper:.2f}x' if upper is not None else 'unbounded'
    lines.append(f"| {i} | {row['managedStatistics']['Mean']/1000:.2f} | {native[0]:.2f} / {native[1]:.2f} | {row['worstBracketRatio']:.2f}x | {formatted} |")
host=last['host']
lines+=['',f"Measured {data['measuredDate']}, {host['ProcessorName']}, {host['OsVersion']},",
    f"SDK {host['DotNetCliVersion']}, runtime {host['RuntimeVersion']}. CPU 0 only,",
    'one codec/conversion thread, normal native SIMD, optimized managed JIT with',
    'tiering disabled and non-concurrent workstation GC. Short repeats guide',
    'priority and retain uncertainty; they do not establish uniform parity.','',
    'Both intervals include full decode, full-range BT.601 to RGBA8, owned output',
    'allocation and release. Native decoder/conversion/input packet/frame wrappers',
    'are reused; decoder state resets and all pixels are decoded/converted every',
    'operation. Padded native RGBA rows need no tight-row copy. Managed public',
    'Decode and disposal are timed. Startup, IPC, I/O, generation, hashing,',
    'validation and tight-row file writes are excluded from both intervals.','',
    'The already prepared 324,002-byte owned quality-69 JPEG is reused; input SHA256',
    f"`{last['identity']['inputSha256']}`. No encoder quality search or additional",
    'source raster is added. Since encoded input is identical, the decoder pair',
    'needs decoder agreement rather than an encoder source-quality match.',
    f"RGB agreement is maximum {last['decoderMaximum']}, RMSE {last['decoderRmse']:.6f}, within",
    'the unchanged maximum 4/RMSE 1.5 bounds. Opaque alpha is exact. Native pixels',
    'match the pinned public FFmpeg CLI exactly with the same conversion flags.',
    'Setup/cleanup check repeated pixels, independent live owners and unchanged',
    'encoded source. Fourteen native protocol/argument/affinity rejection controls',
    'and BenchmarkDotNet output smoke pass.','',
    f"Each output has {last['managedOutputBytes']:,} logical bytes; native padded rows are recorded.",
    f"Managed BDN allocation is {last['managedMemory']['BytesAllocatedPerOperation']:,} bytes per operation.",
    'Owned source-byte and pre-run quietness checks pass. Post-run rolling-load',
    'failures and later passing cooldowns remain recorded without weakened limits.',
    'JPEG encode, PNG, WebP and other editing paths retain their separate evidence.','',
    f"A separate workload-only CPU profile attributes {data['attribution']['inclusivePrimitivePercent']['JpegTransform.Inverse']:.2f}%",
    'to inverse DCT, the first production primitive selected for optimization.',
    'Entropy reading and raster rendering remain separate substantial costs.',
    'The original trace hash and weighted attribution are retained in the evidence;',
    'profile samples are excluded from the parity timing rows.','',
    'Reproduce sequentially on the quiet dedicated Linux VM following the',
    '[worker instructions](../tools/WarmReference/README.md#jpeg-decode).','']
(root/'WARM-JPEG-DECODE.md').write_text('\n'.join(lines),encoding='utf-8',newline='\n')
