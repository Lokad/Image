import json
import sys
from pathlib import Path

root = Path(__file__).resolve().parent
evidence = json.loads((root / 'warm-resize-paired-weights-evidence.json').read_text(encoding='utf-8'))
short, confirm = evidence['candidate']
assert evidence['project'] == 'Lokad.Image' and not evidence['uniformWarmParityComplete']
assert evidence['verification']['actualTimedProductionIlAndFlagsMatchPackage']
assert evidence['verification']['expectedAddedBytesPerOperation'] == 0
rows = '\n'.join(
    f"| {label} | {item['managedStatistics']['Mean'] / 1000:.2f} | "
    f"{item['worstBracketRatio']:.5f}x | {item['conservativeRatioUpper']:.5f}x |"
    for label, item in [('Packed accumulation baseline', evidence['baseline']), ('Short screen', short), ('Confirmation repeat', confirm)])
content = f'''# Warm resize: paired packed-sample filtering

Complete RGBA8 bilinear resize improves {evidence['screenedMeanGainPercent']:.2f}% in the
confirmation repeat on one original 513x259-to-256x129 pair. It reaches
{confirm['worstBracketRatio']:.2f}x FFmpeg; the uniform 2x target remains open.

| Run | Managed mean (microseconds) | Worst native bracket ratio | Conservative upper |
| --- | ---: | ---: | ---: |
{rows}

A compact separate kernel filters two independent RGBA8 output rows with
eight binary64 lanes on AVX512F/AVX2 hardware. The rows share horizontal weights,
offsets and loop control. Each tap uses two bounded four-byte sample reads;
packed widening and conversion produce both RGBA samples together. Source alpha
is extracted from each bounded UInt32 read. Its accumulation
lane is set to one before widening. Weighted alpha is broadcast and inserted
directly into the wide coefficient register, avoiding alpha permutations and
per-tap coefficient stack materialization. Multiplication and addition remain
separate and each pixel retains
its original tap order. Inactive lanes add zero without changing output pixels.

Each row retains its original work charge and cancellation stays bounded.
Paired vertical weights/offsets use 192 bytes of fixed stack storage; the existing
caller also retains its 96-byte storage. There is no new heap workspace.
Unequal row supports, odd tails, unsupported hardware and UInt16 retain the
existing filtering path. Alpha normalization, exact tiny-alpha arithmetic,
halves-away rounding, clamping, source and independent ownership remain.
Three fixed batches of 16 complete Resize/Dispose calls retain exactly the
baseline allocation totals. No performance raster or pair was added.

The managed boundary includes public Resize, geometry preparation, admission,
allocation, all filtering and disposal. The original public-FFmpeg-API graph
includes packed/planar conversion, widened triangle filtering, encoded-space
alpha handling and independent output acquisition/release. Native graph/source/
wrappers/geometry contexts are reused. Native samples before and after bracket
managed samples. Both subjects use one CPU/filter thread, thread type zero and
normal SIMD. Managed tiering is off with workstation/nonconcurrent GC. Startup,
IPC, I/O, generation, hashing and validation are excluded.

181 baseline controls per OS and 181 candidate controls per OS with intrinsics
enabled/disabled pass, including frozen binary64 digests, rational references,
literal RGB half cases, tiny alpha, tight-memory fallback, work/cancellation
and ownership. All 2128 Release tests per OS pass. The same actual dependency-free
Release package passes all three usage examples on both OSes. Both statistical
child DLLs and fresh production IL/flags match it. Only the existing
PreparedResize.Apply body changes among 538 existing production methods; four
named internal paired-kernel methods are added. All other existing bodies and
implementation flags remain equivalent with metadata references resolved.
Fourteen native controls pass. Managed rational-reference pixels remain exact;
native alpha is exact and RGB stays within maximum 1 / RMSE 0.25. The measured
output has no zero-alpha pixels; broader native alpha-edge coverage remains open.

One actual-package JIT inspection accompanies allocation correctness controls
({evidence['currentJitInspection']['nativeCodeBytes']} code bytes for the paired row kernel). It confirms wide
widening/conversion/multiply/add and direct coefficient construction without FMA
or a per-tap wide stack temporary, with no diagnostic timing or
instruction-share claim. Pre-run quiet guards pass and post-run failures remain.
Independent quiet confirmation and other material warm cases remain open.
A later cooldown does not certify the statistical intervals.

The [numeric evidence](warm-resize-paired-weights-evidence.json) retains statistics,
boundaries, source and actual DLL identities. The immutable archive SHA-256 is
`{evidence['immutableEvidence']['rawArchiveSha256']}`.
Regenerate with `python benchmarks/render-warm-resize-paired-weights.py`;
use `--check` to verify this report.
'''
target = root / 'WARM-RESIZE-PAIRED-WEIGHTS.md'
if '--check' in sys.argv:
    assert target.read_text(encoding='utf-8') == content
else:
    target.write_text(content, encoding='utf-8', newline='\n')
