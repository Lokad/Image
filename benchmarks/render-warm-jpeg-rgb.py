"""Render original narrow JPEG measurements; standard-library Python only."""
import json
from pathlib import Path
import statistics

root = Path(__file__).resolve().parent
data = json.loads((root/'warm-jpeg-rgb-evidence.json').read_text(encoding='utf-8'))
first = data['observations']['baseline'][0]
last = data['observations']['rgb'][-1]
quality = first['quality']
managed,native = quality['managedQuality'],quality['nativeQuality']
host = first['host']
lines = [
    '# Warm single-CPU JPEG RGB extraction round', '',
    'This round changes one JPEG input primitive: RGBA extraction and flattening.',
    'It measures the existing 513x259 RGBA8-to-4:4:4 encode pair, after the accepted',
    '[SIMD DCT round](WARM-JPEG-DCT.md). JPEG remains above 2x and uniform warm',
    'parity is not established.', '',
    'The [original numeric evidence](warm-jpeg-rgb-evidence.json) retains managed',
    'statistics/actual samples, native elapsed/count brackets, source/tool/DLL',
    'identities, quality, sizes, allocations and quietness. Regenerate this document',
    'with `python benchmarks/render-warm-jpeg-rgb.py`.', '',
    '| Phase | Repeat | Managed mean, µs | Native before / after, µs | Worst bracket ratio | Conservative upper |',
    '| --- | --- | ---: | ---: | ---: | ---: |'
]
for phase,rows in data['observations'].items():
    for index,row in enumerate(rows,1):
        values = [statistics.mean(sample['NanosecondsPerOperation'] for sample in row[key])/1000 for key in ('nativeBefore','nativeAfter')]
        upper = row['conservativeRatioUpper']
        rendered = f'{upper:.2f}x' if upper is not None else 'unbounded'
        lines.append(f"| {phase} | {index} | {row['managedStatistics']['Mean']/1000:.2f} | {values[0]:.2f} / {values[1]:.2f} | {row['worstBracketRatio']:.2f}x | {rendered} |")
lines += ['',
    f"Measured {data['measuredDate']}, {host['ProcessorName']}, {host['OsVersion']},",
    f"SDK {host['DotNetCliVersion']}, runtime {host['RuntimeVersion']}. Both subjects",
    'use CPU 0 only and one codec/conversion thread, with normal native SIMD.',
    'Managed optimized JIT has tiering disabled; workstation GC and background',
    'GC disabled. Independent short repeats guide priority, with raw uncertainty;',
    'no passing case is certified by this round.', '',
    'Resident original RGBA8 becomes complete independent JPEG bytes. Both subjects',
    'time single-round alpha flattening on RGB 0x204060, full-range BT.601, 4:4:4',
    'encoding and result acquisition/release. Native contexts/scratch/wrappers are',
    'reused but every image is processed again. Managed MemoryStream creation,',
    'growth and disposal are included. Startup, IPC, I/O, validation and quality',
    'search are excluded from both scored intervals.', '',
    f"One existing raster and the frozen qscale {native['Setting']} / quality {managed['Setting']} pair are reused.",
    f"Its RGB source-error gap remains {quality['psnrGapDb']:.5f} dB (fixed band 0.25 dB).",
    f"The original {quality['ownedEnumeratedSettings']}-setting preparation is preserved; no quality search or new",
    'benchmark raster is added. Setup/cleanup freshly check selected bitstreams,',
    'decoder agreement, opaque alpha, geometry, sampling, unchanged input and',
    'independent live storage. Exact bitstreams and quality metrics are unchanged.', '',
    f"Managed output remains {managed['EncodedBytes']:,} bytes; native {native['EncodedBytes']:,} bytes.",
    'Managed fixed Huffman / native optimal Huffman effort is explicit.',
    f"BenchmarkDotNet reports {last['managedMemory']['BytesAllocatedPerOperation']:,} allocated bytes per operation,",
    '151 bytes above the prior round out of 11.4 MB. A separate direct allocation',
    'check runs 16 complete public encode/dispose calls per library: both have',
    'the same per-call allocation range, and the candidate total is lower by 248',
    'bytes. The raw counts and DLL/output identities are retained; there is no',
    'observed allocation growth in this check. Output byte writes retain their',
    'separate allocation cost. No image cache or row/block buffer is introduced.', '',
    f"Separate accepted-DCT CPU sampling attributes {data['attribution']['inclusivePrimitivePercent']['JpegEncoder.ReadRgb']:.2f}%",
    'of encode CPU to RGB reading/flattening, including row access. ReadRgb now',
    'acquires one bounded four-channel row span rather than making four Sample',
    'calls. At 8 bits, cancelling the common factor 255 in the specified blend',
    'gives the exact normalized integer numerator divided by 255 with +127 nearest',
    'rounding. The odd denominator has no half ties. The 16-bit once-only blend/',
    'depth reduction remains unchanged. Color matrix, sampling, DCT, entropy, work/',
    'memory/output admission and cancellation are unchanged.', '',
    'The independent rational-residual test covers 524,288 value/alpha/background',
    'combinations. All 160 frozen original managed JPEG bitstreams reproduce across',
    'five patterns, four sampling modes, two depths and four qualities. These are',
    'correctness checks, not extra timed profiles. Windows/Linux full Release/live',
    'suites pass 1,818 tests (238 internal, 686 public, 894 FFmpeg comparisons).',
    'Linux additionally passes 22 internal and 70 public JPEG controls in each of',
    'normal, AVX-512 disabled, AVX disabled and all intrinsics disabled modes.',
    'The same actual dependency-free Release package and all three usage examples',
    f"pass both OS; package SHA256 `{data['validation']['packageSha256']}`.", '',
    'Pre-run source-byte and quietness checks pass. Preserve original post-run',
    'rolling-load failures and final passing cooldown checks without weakening',
    'thresholds; these observations do not prove hypervisor isolation. Other JPEG',
    'depth/sampling warm paths, decoding, WebP and remaining edits still need evidence.', '',
    'Reproduce sequentially on the dedicated quiet Linux VM, using the',
    '[native worker instructions](../tools/WarmReference/README.md):', '',
    '```powershell',
    './tools/WarmReference/capture-source.ps1',
    'bash tools/WarmReference/build-jpeg-linux.sh',
    'dotnet build Lokad.Image.slnx -c Release --no-restore --tl:off -m:1',
    'dotnet benchmarks/Lokad.Image.Benchmarks/bin/Release/net10.0/Lokad.Image.Benchmarks.dll --prepare-warm-jpeg 0 5',
    './bench.ps1 -Smoke -Warm -WarmFamily Jpeg -Cpu 0 -NoBuild',
    './bench.ps1 -DedicatedVm -Warm -WarmFamily Jpeg -Cpu 0 -NoBuild',
    '# Repeat after a passing cooldown; revalidate the same frozen pair.',
    './bench.ps1 -DedicatedVm -Warm -WarmFamily Jpeg -Cpu 0 -NoBuild',
    '```', ''
]
(root/'WARM-JPEG-RGB.md').write_text('\n'.join(lines),encoding='utf-8',newline='\n')
