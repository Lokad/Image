"""Render retained RGBA16 register/inlining evidence; never measure performance."""
import json,math,re,sys
from pathlib import Path
r=Path(__file__).resolve().parent;e=json.loads((r/'warm-bilinear16-register-evidence.json').read_text(encoding='utf-8'))
assert e['independentQuietConfirmationThisPair'] and e['thisFixedPairClearsConservativeTwo']
rows=[]
for o in [dict(e['previousAcceptedScreen'],label='Previous accepted screen'),*e['observations']]:
 s=o['managed']['statistics'];native=min(b['mean'] for b in o['nativeBrackets']);gc=re.fullmatch(r'// GC:\s+\d+\s+\d+\s+\d+\s+(\d+)\s+(\d+)',o['managed']['rawGc']);assert gc
 allocation=math.ceil(int(gc[1])/int(gc[2]));quiet=all(g['passed'] for g in o['quietness'].values())
 rows.append(f'| {o["label"]} | {s["Mean"]/1000:.2f} | {native/1000:.2f} | {o["worstBracketRatio"]:.5f} / {o["conservativeRatioUpper"]:.5f} | {allocation:,} | {len(o["managed"]["actual"])}/{s["N"]} | {"Pass" if quiet else "Failed / unqualified"} |')
last=e['observations'][-1];reduction=(1-last['managed']['statistics']['Mean']/e['previousAcceptedScreen']['managed']['statistics']['Mean'])*100
content=f'''# Warm RGBA16 bilinear register construction

The fixed RGBA16 bilinear case independently confirms at
{last['worstBracketRatio']:.3f}x FFmpeg, conservative upper {last['conservativeRatioUpper']:.3f}x,
with both quiet guards passing. Managed mean falls {reduction:.1f}% from the previous
accepted screen. Stop tuning this point; full required warm coverage remains open.

| Run | Lokad.Image us | FFmpeg us | Ratio / upper | Managed allocated B/op | Actual/filtered | Quiet guards |
| --- | ---: | ---: | ---: | ---: | ---: | --- |
{chr(10).join(rows)}

One existing original 513x259 RGBA16 raster produces an independently owned
256x129 bilinear result. Managed output remains exact against the rational
reference. Native-only alpha stays within the approved maximum 1 / RMSE <=0.25
in full 16-bit sample units; RGB uses the same bound and zero-alpha RGB is zero.
Normal/scalar CLI output, deterministic pixels, source integrity and independent
live owners pass. RGBA8 native alpha remains exact. No new performance raster,
geometry, quality or strategy sweep is used.

The verified six-second profile attributes 71.7% to paired-row taps and 20.7%
to RGBA16 stores. One primitive keeps alpha extraction and coefficient
application in SIMD lanes after widening each bounded input pixel once.
Explicit register construction replaces generic constructions that generated
stack round trips. A second narrow change inlines per-pixel stores. Original
tap order, separate multiply/add operations, binary64 rounding, tiny-alpha
fallback, admission, work charges and cancellation remain unchanged. No new
workspace, cached output, dependency, unsafe code or native production path.
The combined-store prototype regressed and was reverted. Its raw evidence and
the slow initial vector-construction variant remain retained.

Both final screens failed their post-run quiet guard and remain unqualified.
The separate twelve-actual confirmation passes both guards without a retry.
All twelve actuals remain retained, including two filtered outliers; upper is
the larger filtered/all-actual conservative bound. Managed iterations target
200ms; native pre/post brackets target 50ms. CPU0, one filter/conversion thread,
normal SIMD, tiering off and nonconcurrent workstation GC stay fixed.
`exclusive-vm-idle-v2` checks live overlap and five-second aggregate/CPU0 busy
<=5%, I/O wait <=1%, steal <=1%. No failed interval is recategorized.

Timing includes complete public Resize, geometry preparation, admission,
allocation/materialization and disposal. Native public FFmpeg filtering includes
packed/planar conversion, widened triangle filtering, encoded-space alpha
premultiplication/unpremultiplication, final RGBA64 conversion and independent
output acquisition/release. Graph/source/frame wrappers and geometry contexts
are reused; output pixels are not cached. Startup, I/O, IPC, hashing and
validation are excluded. Allocation is raw GC bytes / operations rounded up,
including harness; native allocations are unmeasured.

Final measured owned source is `{last['source']['sourceSha256']}`
at recorded revision `{last['source']['sourceRevision']}`.
Four actual child identities, original receipts, package and all source bytes
are archived before preparation or source mutation. Both OSes pass 2,398 Release
tests each (648 internal, 854 public API, 896 live comparisons), 147 scalar
resize/admission controls each and all three examples through the same actual
dependency-free Release package. All 571 accepted fresh/timed/package method
bodies and implementation flags match. RGBA8 validation-only smoke passes.

A subsequent audit found that the earlier rollback left a stale compiled
four-tap prototype: the prior fresh-DLL QA assertion failed and did not prove
the accepted package. Original timed child/package proofs remain valid.
That incorrect profile is rejected and archived; a forced rebuild is verified
before the accepted diagnostic profile. The earlier report's fresh-DLL claim
is corrected explicitly. Source overlays that preserve timestamps require
`-t:Rebuild` plus method-body/flag checks before profiling or measurement.

[Numeric evidence](warm-bilinear16-register-evidence.json) retains all actuals,
brackets, guards, input/output/source/child/package identities, rejected variants,
diagnostics, correction and immutable archive digests. See the
[current three-table summary](../docs/benchmark.md) and
[measurement recipe](../tools/WarmReference/README.md).
Regenerate with `python benchmarks/render-warm-bilinear16-register.py`;
`--check` verifies this report. Unmatched and uncovered scope stays open.
'''
p=r/'WARM-BILINEAR16-REGISTER.md'
if '--check' in sys.argv:
 assert p.read_text(encoding='utf-8')==content,'RGBA16 register report is stale.'
 print('RGBA16 register report is current.')
else:p.write_text(content,encoding='utf-8',newline='\n')
