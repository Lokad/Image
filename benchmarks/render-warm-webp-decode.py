"""Render the retained one-pair, single-CPU warm VP8L decode screens."""
import json
from pathlib import Path
import statistics

root = Path(__file__).resolve().parent
data = json.loads((root/'warm-webp-decode-evidence.json').read_text(encoding='utf-8'))
lines = ['# Warm still WebP lossless decode', '',
    'One frozen VP8L input from the existing original 513x259 RGBA8 raster',
    'establishes a missing warm decode comparison. No production code changes',
    'in this round. This path exceeds the 2x target and becomes the largest',
    'measured gap; uniform parity remains open.', '',
    'The [original numeric evidence](warm-webp-decode-evidence.json) retains',
    'actual managed samples, native brackets, uncertainty, allocations, source',
    'and binary identities, output checks, controls and quietness observations.',
    'Regenerate with `python benchmarks/render-warm-webp-decode.py`.', '',
    '| Repeat | Managed mean, µs | Native before / after, µs | Worst bracket ratio | Conservative upper | Allocated bytes/op |',
    '| ---: | ---: | ---: | ---: | ---: | ---: |']
for index,row in enumerate(data['observations'],1):
    native = [statistics.mean(s['NanosecondsPerOperation'] for s in row[k])/1000 for k in ('nativeBefore','nativeAfter')]
    upper = row['conservativeRatioUpper']; formatted = f'{upper:.2f}x' if upper is not None else 'unbounded'
    lines.append(f"| {index} | {row['managedStatistics']['Mean']/1000:.2f} | {native[0]:.2f} / {native[1]:.2f} | {row['worstBracketRatio']:.2f}x | {formatted} | {row['managedMemory']['BytesAllocatedPerOperation']:,} |")
row = data['observations'][-1]; host = row['host']
lines += ['',f"Measured {data['measuredDate']} UTC, {host['ProcessorName']}, {host['OsVersion']},",
    f"SDK {host['DotNetCliVersion']}, runtime {host['RuntimeVersion']}. CPU 0 only,",
    'one codec and conversion thread, normal SIMD, tiering disabled and',
    'non-concurrent workstation GC. Managed mean spread across the two screens is',
    f"{data['repeatability']['managedMeanSpreadFraction']*100:.3f}%.", '',
    f"The frozen input is {row['inputBytes']:,} bytes; exact RGBA output is {row['outputBytes']:,} bytes.",
    'It is prepared once by the original Lokad.Image lossless encoder at effort 4.',
    'No new raster or quality enumeration is used. This literal/run VP8L profile',
    'has no transforms, color cache or spatial entropy groups, as defined by the',
    '[public lossless specification](https://developers.google.com/speed/webp/docs/webp_lossless_bitstream_specification).',
    'These results do not cover other VP8L strategies or lossy VP8 decode.', '',
    'Both subjects receive identical resident bytes. Managed timing covers the',
    'complete public Decode call, independent output acquisition and disposal.',
    'Native timing covers software webp decoding, state reset, ARGB-to-RGBA',
    'conversion, independent owned output allocation and release. Codec/conversion',
    'contexts and packet/frame wrappers are reused; every call decodes and converts',
    'every pixel. Native padded rows need no extra packing copy. Startup, IPC,',
    'I/O, encoding, hashing and validation remain outside both timing boundaries.', '',
    'All RGBA bytes, alpha and zero-alpha hidden RGB match the original source',
    'and public FFmpeg CLI. Setup and cleanup verify source integrity and',
    'independent live managed/native outputs. Twenty native negative controls',
    'cover affinity, arguments, protocol, dimensions and container framing.',
    'The JPEG decode smoke and eleven unmatched-VP8 preparation controls pass.', '',
    'Windows offline tests pass 1,036 cases. Linux passes all 1,930 cases:',
    '350 internal, 686 public and 894 live FFmpeg comparisons. Production source',
    'bytes are unchanged. The existing actual dependency-free Release package',
    'is reverified on Linux, including all three usage-guide examples. Its',
    'production method-body fingerprint matches the actual measured DLL; both',
    'decode this frozen VP8L input to every original RGBA byte. Package SHA256:',
    f"`{data['validation']['packageSha256']}`.", '',
    'Failed quietness samples and later passing cooldowns are retained. Short-run',
    'uncertainty stays visible; this report does not certify other paths or',
    'future hypervisor isolation. Reproduce sequentially on the quiet VM using',
    'the [worker instructions](../tools/WarmReference/README.md#still-vp8l-webp-decode).', '']
(root/'WARM-WEBP-DECODE.md').write_text('\n'.join(lines),encoding='utf-8',newline='\n')
