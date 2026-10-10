"""Render retained rejection decisions and current CPU attribution."""
import json
from pathlib import Path
root=Path(__file__).resolve().parent
data=json.loads((root/'warm-jpeg-rejected-primitives-evidence.json').read_text(encoding='utf-8'))
lines=['# Warm JPEG rejected primitives','',
    'Three narrow candidates provided no useful speedup on the retained native JPEG.',
    'All production changes were discarded. Twenty-seven original boundary tests',
    'remain. The retained JPEG decode gap is still about 2.91x; uniform parity is open.','',
    'The [numeric evidence](warm-jpeg-rejected-primitives-evidence.json) retains',
    'actual managed samples, confidence intervals, native brackets, allocation',
    'diagnostics, frozen input/output hashes, source identities and quietness checks.',
    'Regenerate with `python benchmarks/render-warm-jpeg-rejected-primitives.py`.','',
    '| Candidate | Repeat | Managed mean, µs | Worst bracket ratio | Conservative upper | Allocation change, bytes/op |',
    '| --- | ---: | ---: | ---: | ---: | ---: |']
for item in data['rejectedCandidates']:
    for index,row in enumerate(item['observations']):
        upper=row['conservativeRatioUpper']; formatted=f'{upper:.2f}x' if upper is not None else 'unbounded'
        lines.append(f"| {item['name']} | {index+1} | {row['managedStatistics']['Mean']/1000:.2f} | {row['worstBracketRatio']:.2f}x | {formatted} | {item['allocationDelta'][index]:,} |")
lines+=['','The adaptive twelve-bit lookup replaced the ten-bit table for definitions',
    'containing eleven/twelve-bit codes. Its added admission/allocation had no useful',
    'benefit. The combined ten-bit lookup placed short codes in the existing long',
    'table and selected it directly; it was also slower. Skipping zero coefficients',
    'in the binary64 inverse transform was slower in one screen, so its repeat and',
    'affected-input screen were omitted. These are observations, not CPU attribution',
    'of the rejected code or certified regression bounds.','',
    'All five screens ran sequentially on CPU 0 of the dedicated AMD EPYC 9V45 VM,',
    'with .NET 10.0.8 / SDK 10.0.204, normal SIMD, disabled tiering and background',
    'GC, one codec/conversion thread and the same frozen 133,041-byte JPEG. It comes',
    'from one existing original 513x259 RGBA8 raster; no new image, preparation or',
    'quality enumeration was used. Each pair receives identical resident bytes.',
    'Full public decode, conversion, independent owned output allocation and disposal',
    'are timed; native reset remains included. Startup, IPC, I/O, generation, hashing',
    'and validation are excluded. All output identities match the retained baseline.','',
    'Before each sample, the five-second quietness check passed. Post-run rolling',
    'load checks failed; all failures and later passing cooldowns are retained.',
    'Short screens guide rejection and ranking; they do not certify uniform parity.','',
    'After reverting all candidates, one current CPU profile uses the same JPEG.',
    'Method matching excludes parameter types. The following inclusive proportions',
    'are sampled workload CPU, including warmup/calibration, and are not parity timing.','',
    '| Current primitive / group | Inclusive sampled CPU |',
    '| --- | ---: |']
for name,value in data['currentAttribution']['inclusivePrimitivePercent'].items():
    lines.append(f'| {name} | {value:.2f}% |')
lines+=['','All 1,994 tests pass on Windows and Linux: 414 internal, 686 public and',
    '894 live FFmpeg comparisons. The retained controls cover sixteen mixed-code',
    'cached phases, 288 partial short-code budgets with a long table, 184 partial',
    'eleven/twelve-bit budgets, stuffing/padding/restarts, marker/truncation versus',
    'work precedence, invalid-prefix failures, exact table admission and cancellation.',
    'Canonical codes are independently derived under',
    '[T.81 C.2](https://www.w3.org/Graphics/JPEG/itu-t81.pdf). Fixture inputs remain original.','',
    'The final production library is the previous accepted implementation. The same',
    'actual dependency-free Release package passes verification and all three usage',
    'examples on both platforms. SHA256:',f"`{data['validation']['packageSha256']}`.",'',
    'Reproduce sequentially with the [worker instructions](../tools/WarmReference/README.md#jpeg-decode).','']
(root/'WARM-JPEG-REJECTED-PRIMITIVES.md').write_text('\n'.join(lines),encoding='utf-8',newline='\n')
