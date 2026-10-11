import json,sys
from pathlib import Path
r=Path(__file__).resolve().parent
v=json.loads((r/'warm-resize-prepared-evidence.json').read_text(encoding='utf-8'))
b,short,confirm=v['baseline'],*v['candidate']
assert v['project']=='Lokad.Image' and not v['uniformWarmParityComplete']
assert v['verification']['actualTimedProductionIlAndFlagsMatchPackage']
rows='\n'.join(f"| {label} | {item['managedStatistics']['Mean']/1000:.2f} | {item['worstBracketRatio']:.5f}x | {item['conservativeRatioUpper']:.5f}x |"
 for label,item in [('Retained geometry baseline',b),('Short screen',short),('Confirmation repeat',confirm)])
content=f'''# Warm resize: compact prepared filtering kernel

Complete RGBA8 bilinear resize improves {v['screenedMeanGainPercent']:.2f}% in the
confirmation repeat on one original 513x259-to-256x129 pair. It reaches
{confirm['worstBracketRatio']:.2f}x FFmpeg; the uniform 2x target remains open.

| Run | Managed mean (microseconds) | Worst native bracket ratio | Conservative upper |
| --- | ---: | ---: | ---: |
{rows}

The short-support path now uses a compact byte/UInt16-specialized filtering
kernel. It captures source/destination views once, prepares at most eight
vertical weights and clamped row offsets per output row, and reads the retained
horizontal geometry plan directly. The tap loop no longer performs row-access
calls, runtime sample-depth selection, coefficient switches or direct-path
selection. Wide vertical support and tight-memory geometry keep the prior path.

Every output pixel retains the same tap order, binary64 weight multiplication
and accumulation, clamping, alpha normalization and rounding. Tiny positive
alpha still uses the existing exact arithmetic. Work admission and cancellation
checkpoints remain, with independent output ownership and source integrity.
There is no explicit new SIMD, FMA, separable pass or output cache.

The retained horizontal geometry workspace remains admitted conservatively,
leaving 4096 bytes for exact alpha scratch. The kernel adds 96 bytes of fixed
stack storage and no heap workspace. Three fixed batches of 16 complete
Resize/Dispose calls retain exactly the baseline allocation totals. Geometry
preparation, allocation, all filtering and disposal remain inside managed timing;
geometry and output pixels are not retained across calls.

The original public-FFmpeg-API graph includes packed/planar conversion, widened
triangle filtering, encoded-space premultiplication/unpremultiplication and
independent output pixel acquisition/release. Graph/source/wrappers/geometry
contexts are reused. Native samples before and after bracket managed samples.
Both subjects use one CPU/filter thread, thread type zero and normal SIMD;
managed tiering is off with workstation/nonconcurrent GC. Startup, IPC, I/O,
generation, hashing and validation are excluded.

Managed pixels retain the exact rational-reference digest. Native alpha is exact;
RGB retains maximum error 1 / RMSE 0.25, with observed RMSE 0.003177. The selected
output has no zero-alpha pixels; broader native alpha-edge coverage remains open.
No performance image or pair was added.

175 baseline controls per OS and 175 candidate controls per OS with intrinsics
enabled/disabled pass, including frozen binary64 digests, rational references,
tiny alpha, tight-memory fallback, work/cancellation and ownership controls.
All 2122 Release tests per OS pass. The same actual dependency-free Release
package passes all three examples on both OSes; both statistical child DLLs and
fresh production IL/flags match it. Only Resize changes among 535 existing
production methods; three internal prepared-kernel methods are added. All other
existing bodies and flags remain equivalent. Fourteen native controls pass.

One actual-package JIT inspection is retained with the fixed allocation controls
({v['currentJitInspection']['nativeCodeBytes']} code bytes). It validates code shape,
without a diagnostic timing or instruction-share claim.
Pre-run quiet guards pass and post-run failures remain. Independent quiet
confirmation and other depths/directions/sizes/bicubic warm coverage are open;
a later cooldown does not certify the statistical intervals.

The [numeric evidence](warm-resize-prepared-evidence.json) records statistics,
boundaries, source and actual DLL identities. The final immutable archive SHA-256
is `{v['immutableEvidence']['rawArchiveSha256']}`. Regenerate this report with
`python benchmarks/render-warm-resize-prepared.py`; use `--check` to verify it.
'''
p=r/'WARM-RESIZE-PREPARED.md'
if '--check' in sys.argv:assert p.read_text(encoding='utf-8')==content
else:p.write_text(content,encoding='utf-8',newline='\n')
