"""Render the original narrow JPEG byte-output measurements."""
import json
from pathlib import Path
import statistics

root = Path(__file__).resolve().parent
data = json.loads((root/'warm-jpeg-byte-evidence.json').read_text(encoding='utf-8'))
first = data['observations']['baseline'][0]
last = data['observations']['byteOutput'][-1]
quality = first['quality']
managed,native = quality['managedQuality'],quality['nativeQuality']
lines = [
    '# Warm single-CPU JPEG byte-output round', '',
    'This round changes one primitive: LimitedWriteStream.WriteByte. A safe',
    'one-byte stack span replaces the inherited per-byte heap array, while',
    'retaining the same span write and admission checks. It measures the existing',
    '513x259 RGBA8-to-4:4:4 pair after the accepted',
    '[RGB extraction round](WARM-JPEG-RGB.md). JPEG remains above 2x.', '',
    'The [original numeric evidence](warm-jpeg-byte-evidence.json) retains actual',
    'managed samples/statistics, native elapsed/count brackets, source/tool/DLL',
    'identities, quality, output sizes, allocations and quietness. Regenerate this',
    'document with `python benchmarks/render-warm-jpeg-byte.py`.', '',
    '| Phase | Repeat | Managed mean, µs | Native before / after, µs | Worst bracket ratio | Conservative upper |',
    '| --- | --- | ---: | ---: | ---: | ---: |'
]
for phase,rows in data['observations'].items():
    for index,row in enumerate(rows,1):
        values = [statistics.mean(sample['NanosecondsPerOperation'] for sample in row[key])/1000 for key in ('nativeBefore','nativeAfter')]
        upper = row['conservativeRatioUpper']
        rendered = f'{upper:.2f}x' if upper is not None else 'unbounded'
        lines.append(f"| {phase} | {index} | {row['managedStatistics']['Mean']/1000:.2f} | {values[0]:.2f} / {values[1]:.2f} | {row['worstBracketRatio']:.2f}x | {rendered} |")
host = last['host']
lines += ['',
    f"Measured {data['measuredDate']}, {host['ProcessorName']}, {host['OsVersion']},",
    f"SDK {host['DotNetCliVersion']}, runtime {host['RuntimeVersion']}. Both subjects",
    'use CPU 0 only and one codec/conversion thread. Normal native SIMD is enabled;',
    'managed optimized JIT has tiering disabled, with workstation GC and background',
    'GC disabled. Two independent short repeats guide priority and retain raw',
    'uncertainty. This round does not certify a passing case or uniform parity.', '',
    'Both intervals include resident original RGBA8, exact single-round alpha',
    'flattening on RGB 0x204060, full-range BT.601 conversion, 4:4:4 encoding and',
    'complete independent output acquisition/release. Native contexts, scratch and',
    'wrappers are reused; every pixel is reprocessed. Fresh managed MemoryStream',
    'creation, growth and disposal are timed. Startup, IPC, I/O, hashing, validation',
    'and quality enumeration are excluded from both intervals.', '',
    f"One existing raster and frozen qscale {native['Setting']} / quality {managed['Setting']} pair are reused.",
    f"Its RGB source-error gap remains {quality['psnrGapDb']:.5f} dB, within the unchanged 0.25 dB band.",
    f"The original {quality['ownedEnumeratedSettings']}-setting preparation remains; no extra quality search or",
    'benchmark raster is added. Setup/cleanup freshly check selected bitstreams,',
    'geometry, decoder agreement, alpha, sampling, source integrity and ownership.',
    f"Output remains {managed['EncodedBytes']:,} managed / {native['EncodedBytes']:,} native bytes;",
    'managed fixed Huffman and native optimal Huffman effort remain explicit.', '',
    f"Managed BDN allocation changes from {first['managedMemory']['BytesAllocatedPerOperation']:,} to",
    f"{last['managedMemory']['BytesAllocatedPerOperation']:,} bytes per operation. Color matrix, sampling, DCT,",
    'entropy work and output bytes are unchanged. Every write retains cancellation',
    'and quota checks, span dispatch, successful-write accounting and caller stream',
    'ownership. No output batching, image cache or row/block buffer is added.', '',
    'Four new original controls verify mixed byte/span quota boundaries, cancellation',
    'precedence, failed caller writes and zero per-byte heap allocation with a span',
    'sink. Windows/Linux full Release/live suites pass 1,822 tests (242 internal,',
    '686 public, 894 FFmpeg comparisons). Linux additionally passes 26 internal and',
    '70 public JPEG/output controls in each of normal, AVX-512 disabled, AVX disabled',
    'and all intrinsics disabled modes. The same actual dependency-free Release',
    'package and all three usage examples pass both OS:',
    f"SHA256 `{data['validation']['packageSha256']}`.", '',
    'Source-byte and pre-run quietness checks pass. Original post-run rolling-load',
    'failures and later passing cooldown observations remain in the evidence; no',
    'threshold is weakened and hypervisor isolation is not inferred. Other JPEG',
    'paths, PNG, WebP and remaining editing operations still need their own evidence.', '',
    'Reproduce sequentially on the quiet dedicated Linux VM using the',
    '[native worker instructions](../tools/WarmReference/README.md):', '',
    '```powershell',
    './tools/WarmReference/capture-source.ps1',
    'bash tools/WarmReference/build-jpeg-linux.sh',
    'dotnet build Lokad.Image.slnx -c Release --no-restore --tl:off -m:1',
    '# Reuse and freshly validate the frozen preparation; do not enumerate quality.',
    './bench.ps1 -Smoke -Warm -WarmFamily Jpeg -Cpu 0 -NoBuild',
    './bench.ps1 -DedicatedVm -Warm -WarmFamily Jpeg -Cpu 0 -NoBuild',
    '# Repeat after a passing cooldown.',
    './bench.ps1 -DedicatedVm -Warm -WarmFamily Jpeg -Cpu 0 -NoBuild',
    '```', ''
]
(root/'WARM-JPEG-BYTE.md').write_text('\n'.join(lines),encoding='utf-8',newline='\n')
