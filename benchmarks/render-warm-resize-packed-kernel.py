import json
import sys
from pathlib import Path

root = Path(__file__).resolve().parent
evidence = json.loads((root / 'warm-resize-packed-kernel-evidence.json').read_text(encoding='utf-8'))
short, confirm = evidence['candidate']
assert evidence['project'] == 'Lokad.Image' and not evidence['uniformWarmParityComplete']
assert evidence['verification']['actualTimedProductionIlAndFlagsMatchPackage']
assert evidence['verification']['expectedAddedBytesPerOperation'] == 0
rows = '\n'.join(
    f"| {label} | {item['managedStatistics']['Mean'] / 1000:.2f} | "
    f"{item['worstBracketRatio']:.5f}x | {item['conservativeRatioUpper']:.5f}x |"
    for label, item in [('Prepared kernel baseline', evidence['baseline']), ('Short screen', short), ('Confirmation repeat', confirm)])
content = f'''# Warm resize: packed RGBA8 accumulation

Complete RGBA8 bilinear resize improves {evidence['screenedMeanGainPercent']:.2f}% in the
confirmation repeat on one original 513x259-to-256x129 pair. It reaches
{confirm['worstBracketRatio']:.2f}x FFmpeg; the uniform 2x target remains open.

| Run | Managed mean (microseconds) | Worst native bracket ratio | Conservative upper |
| --- | ---: | ---: | ---: |
{rows}

The compact short-support kernel loads each RGBA8 sample with one bounded
four-byte read. SSE4.1 widens bytes to integers and AVX converts them to four
binary64 lanes. Independent RGB lanes accumulate weighted premultiplied color;
a fourth lane containing one accumulates weighted alpha. Hardware and sample
depth selection occur outside the loops. Scalar and UInt16 paths remain.

Each lane retains the original multiplication, addition and tap order, without
FMA or reassociation. Alpha normalization, exact tiny-alpha arithmetic, rounding,
clamping, cancellation and admission remain unchanged. Output storage is
independent and source pixels remain unchanged. Three fixed batches of 16
complete Resize/Dispose calls retain exactly the baseline allocation totals.
The primitive adds no heap workspace or performance image/pair.

The managed boundary includes public Resize, geometry preparation, admission,
allocation, filtering and disposal. The original public-FFmpeg-API graph includes
packed/planar conversion, widened triangle filtering, encoded-space
premultiplication/unpremultiplication and independent output acquisition/release.
Native graph/source/wrappers/geometry contexts are reused. Native samples bracket
managed samples. Both use one CPU/filter thread, thread type zero and normal SIMD;
managed tiering is off with workstation/nonconcurrent GC. Startup, IPC, I/O,
generation, hashing and validation are excluded.

Managed pixels retain the exact rational-reference digest. Native alpha is exact;
RGB retains maximum error 1 / RMSE 0.25, with observed RMSE 0.003177. This output
has no zero-alpha pixels; broader native alpha-edge coverage remains open.

175 baseline controls per OS and 175 candidate controls per OS with intrinsics
enabled/disabled pass, including frozen binary64 digests, rational references,
tiny alpha, tight-memory fallback, work/cancellation and ownership controls.
All 2122 Release tests per OS pass. The same actual dependency-free Release
package passes all three examples on both OSes. Both statistical child DLLs and
fresh production IL/flags match it. Only PreparedResize.Apply changes among
538 production methods; all other bodies and implementation flags remain
equivalent with metadata references resolved. Fourteen native controls pass.

One actual-package JIT inspection accompanies allocation correctness controls
({evidence['currentJitInspection']['nativeCodeBytes']} code bytes). It confirms packed widening,
conversion, multiplication and addition without FMA. It makes no diagnostic
timing or instruction-share claim. Pre-run quiet guards pass and post-run
failures remain. Independent quiet confirmation and other resize depths,
directions, sizes and bicubic warm coverage are open. A later cooldown does not
certify the statistical intervals.

The [numeric evidence](warm-resize-packed-kernel-evidence.json) retains statistics,
boundaries, source and actual DLL identities. The immutable archive SHA-256 is
`{evidence['immutableEvidence']['rawArchiveSha256']}`.
Regenerate with `python benchmarks/render-warm-resize-packed-kernel.py`;
use `--check` to verify this report.
'''
target = root / 'WARM-RESIZE-PACKED-KERNEL.md'
if '--check' in sys.argv:
    assert target.read_text(encoding='utf-8') == content
else:
    target.write_text(content, encoding='utf-8', newline='\n')
