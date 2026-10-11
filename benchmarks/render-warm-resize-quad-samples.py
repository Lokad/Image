import json
import sys
from pathlib import Path

root = Path(__file__).resolve().parent
evidence = json.loads((root / 'warm-resize-quad-samples-evidence.json').read_text(encoding='utf-8'))
short, confirm = evidence['candidate']
assert evidence['project'] == 'Lokad.Image' and not evidence['uniformWarmParityComplete']
assert evidence['verification']['actualTimedProductionIlAndFlagsMatchPackage']
assert evidence['verification']['expectedAddedBytesPerOperation'] == 0
rows = '\n'.join(
    f"| {label} | {item['managedStatistics']['Mean'] / 1000:.2f} | "
    f"{item['worstBracketRatio']:.5f}x | {item['conservativeRatioUpper']:.5f}x |"
    for label, item in [('Paired packed-sample baseline', evidence['baseline']), ('Short screen', short), ('Confirmation repeat', confirm)])
content = f'''# Warm resize: grouped four-tap samples

Complete RGBA8 bilinear resize improves {evidence['screenedMeanGainPercent']:.2f}% in the
confirmation repeat on one original 513x259-to-256x129 pair. It reaches
{confirm['worstBracketRatio']:.2f}x FFmpeg; the uniform 2x target remains open.

| Run | Managed mean (microseconds) | Worst native bracket ratio | Conservative upper |
| --- | ---: | ---: | ---: |
{rows}

For exactly four consecutive horizontal sample offsets, the paired row kernel
uses one bounded 16-byte read per source row, replacing four separate four-byte
reads per row. Interleaving forms four independent RGBA sample pairs; four
unrolled taps retain the original accumulation order. Packed widening,
conversion and direct weighted-alpha broadcasts use the existing AVX512F/AVX2
path. Each pixel retains separate binary64 multiplication/addition, alpha
normalization, exact tiny-alpha handling, halves-away rounding and clamping.

Duplicate clamped offsets and other tap counts retain the general paired path.
Unequal vertical supports, odd tails, unsupported hardware and UInt16 retain
the existing filtering paths. Work charges, bounded cancellation, admission,
source and independent ownership remain. There is no new heap or stack
workspace. Three fixed batches of 16 complete Resize/Dispose calls retain
exactly the baseline allocation totals. No performance raster or pair was added.

The managed boundary includes public Resize, geometry preparation, admission,
allocation, all filtering and disposal. The public-FFmpeg-API graph includes
packed/planar conversion, widened triangle filtering, encoded-space alpha
handling and independent output acquisition/release. Native graph/source/
wrappers/geometry contexts are reused. Native samples before and after bracket
managed samples. Both subjects use one CPU/filter thread, thread type zero and
normal SIMD. Managed tiering is off with workstation/nonconcurrent GC. Startup,
IPC, I/O, generation, hashing and validation are excluded.

181 baseline controls per OS and 181 candidate controls per OS with intrinsics
enabled/disabled pass, covering frozen binary64 digests, rational references,
literal RGB halves, tiny alpha, tight memory, work/cancellation and ownership.
All 2128 Release tests per OS pass. The same actual dependency-free Release
package passes all three usage examples on both OSes. Both statistical child
DLLs and fresh production IL/flags match it. Only PairedResize.TryRows changes
among 542 existing production methods; one internal tap helper is added.
Other existing bodies and implementation flags remain equivalent with metadata
references resolved. Fourteen native controls pass. Managed rational-reference
pixels remain exact; native alpha is exact and RGB stays within maximum 1 /
RMSE 0.25. This output has no zero-alpha pixels; broader native alpha-edge
coverage remains open.

One actual-package JIT inspection accompanies allocation correctness controls
({evidence['currentJitInspection']['nativeCodeBytes']} code bytes for the paired row kernel). It confirms grouped loads,
interleaving and inlined taps with packed widening/conversion/multiply/add,
without FMA or per-tap wide coefficient stack materialization. This is a code
shape check, with no diagnostic timing or instruction-share claim. Pre-run
quiet guards pass; post-run failures remain preserved. Independent quiet
confirmation and other material warm cases remain open. A later cooldown does
not certify statistical intervals.

The [numeric evidence](warm-resize-quad-samples-evidence.json) retains statistics,
boundaries, source and actual DLL identities. The immutable archive SHA-256 is
`{evidence['immutableEvidence']['rawArchiveSha256']}`.
Regenerate with `python benchmarks/render-warm-resize-quad-samples.py`;
use `--check` to verify this report.
'''
target = root / 'WARM-RESIZE-QUAD-SAMPLES.md'
if '--check' in sys.argv:
    assert target.read_text(encoding='utf-8') == content
else:
    target.write_text(content, encoding='utf-8', newline='\n')
