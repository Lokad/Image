# Warm RGBA16 bilinear register construction

The fixed RGBA16 bilinear case independently confirms at
1.874x FFmpeg, conservative upper 1.908x,
with both quiet guards passing. Managed mean falls 16.3% from the previous
accepted screen. Stop tuning this point; full required warm coverage remains open.

| Run | Lokad.Image us | FFmpeg us | Ratio / upper | Managed allocated B/op | Actual/filtered | Quiet guards |
| --- | ---: | ---: | ---: | ---: | ---: | --- |
| Previous accepted screen | 936.46 | 419.61 | 2.23172 / 3.20258 | 290,111 | 3/3 | Pass |
| Register construction screen | 862.08 | 412.44 | 2.09021 / 2.89713 | 290,119 | 3/3 | Failed / unqualified |
| Inline store screen | 799.06 | 416.27 | 1.91956 / 2.79428 | 290,118 | 3/3 | Failed / unqualified |
| Independent quiet confirmation | 784.03 | 418.41 | 1.87384 / 1.90756 | 290,118 | 12/10 | Pass |

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

Final measured owned source is `74260a594d3dad7d8cbcbd98aede8d5158bc2c6033656bce404721d355779bbb`
at recorded revision `0ab44907219f7791f8b891a382a56851c094cc89`.
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
