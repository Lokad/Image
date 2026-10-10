"""Render the original single-input native-prepared JPEG decode screens."""
import json
from pathlib import Path
import statistics

root = Path(__file__).resolve().parent
data = json.loads((root/'warm-jpeg-native-decode-evidence.json').read_text())
rows = data['observations']; last = rows[-1]
lines = ['# Warm decode of the native-prepared JPEG', '',
    'This screen decodes the existing FFmpeg-produced 513x259 JPEG from identical',
    'resident bytes to independent owned RGBA8. It adds one explicit benchmark',
    'input and changes no production code. Default `Decode8` still selects only',
    'the original owned JPEG; `NativeDecode8` selects only this native input.', '',
    'The [original numeric evidence](warm-jpeg-native-decode-evidence.json) retains',
    'managed samples, statistics and allocations, native elapsed/count brackets,',
    'source/tool/DLL identities, output hashes, agreement and quietness. Regenerate',
    'with `python benchmarks/render-warm-jpeg-native-decode.py`.', '',
    '| Repeat | Managed mean, µs | Native before / after, µs | Worst bracket ratio | Conservative upper |',
    '| --- | ---: | ---: | ---: | ---: |']
for i, row in enumerate(rows, 1):
    native = [statistics.mean(v['NanosecondsPerOperation'] for v in row[key])/1000 for key in ('nativeBefore', 'nativeAfter')]
    upper = row['conservativeRatioUpper']; formatted = f'{upper:.2f}x' if upper is not None else 'unbounded'
    lines.append(f"| {i} | {row['managedStatistics']['Mean']/1000:.2f} | {native[0]:.2f} / {native[1]:.2f} | {row['worstBracketRatio']:.2f}x | {formatted} |")
host = last['host']
lines += ['', f"Measured {data['measuredDate']}, {host['ProcessorName']}, {host['OsVersion']},",
    f"SDK {host['DotNetCliVersion']}, runtime {host['RuntimeVersion']}. CPU 0 only,",
    'one codec/conversion thread, normal SIMD, tiering disabled and non-concurrent',
    'workstation GC. These short repeats guide ranking; they do not establish',
    'uniform parity or coverage of other JPEG sampling modes.', '',
    'The frozen qscale-5 JPEG has 133,041 bytes, variable Huffman tables and common',
    '1x2 component factors, which remain 4:4:4 and exercise a padded block layout.',
    'It comes from the same original deterministic RGBA8 raster flattened once',
    'on RGB 0x204060. Preparation and encoder qualities are reused unchanged;',
    'there is no new raster or quality enumeration. Input SHA256:',
    f"`{last['identity']['inputSha256']}`.", '',
    'Both subjects decode and convert every pixel, acquire independent output',
    'and release it. Native context/conversion/input packet/frame wrappers are',
    'reused, with decoder state reset per operation. Padded native rows need no',
    'tight-row copy. Managed public Decode and disposal are timed. Startup, IPC,',
    'I/O, generation, hashing, checks and file packing are excluded.', '',
    f"RGB agreement is maximum {last['decoderMaximum']}, RMSE {last['decoderRmse']:.6f}, within",
    'the unchanged maximum 4/RMSE 1.5 bounds, with exact opaque alpha. Managed',
    'pixels reproduce the original frozen preparation. Native pixels exactly',
    'match the pinned public FFmpeg CLI with normal SIMD and the same conversion.',
    'Smoke and setup/cleanup verify repeated pixels, independent live owners,',
    'unchanged source, geometry and single-thread boundaries. The retained fourteen',
    'shared worker rejection controls use the original owned JPEG; they are not',
    'reported as fourteen additional tests of this new input.', '',
    f"A separate workload profile attributes {data['attribution']['inclusivePrimitivePercent']['JpegHuffman']:.2f}%",
    'to entropy/Huffman processing and identifies it as the next optimization',
    'target. Its source and trace hash are retained; profile samples are excluded',
    'from the parity rows.', '',
    f"Each output has {last['managedOutputBytes']:,} logical bytes.",
    f"Managed BDN allocation is {last['managedMemory']['BytesAllocatedPerOperation']:,} bytes per operation.",
    'Failed rolling-load checks and later passing cooldowns remain in the evidence.',
    'Reproduce sequentially on the quiet dedicated Linux VM following the',
    '[worker instructions](../tools/WarmReference/README.md#jpeg-decode).', '']
(root/'WARM-JPEG-NATIVE-DECODE.md').write_text('\n'.join(lines), encoding='utf-8', newline='\n')
