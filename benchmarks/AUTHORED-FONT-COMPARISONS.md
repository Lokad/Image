# Authored-font public comparisons

ATS1142 resolves the five original authored-font reference mismatches without a
Lokad.Image production change. AFT1143 measures their complete operations on the
unchanged actual-package DLL. The 289-path scope and frozen controls are unchanged.

## Reference corrections

These comparisons use **source-corrected Fonts 2.1.3 / Drawing 2.1.7 public
integration with ImageSharp 3.1.12**, not FFmpeg or a bare shaping engine. Stock
assemblies remain retained; this tooling does not establish general parser,
hinting or font-engine equivalence. Existing qualified DejaVu/default-variation
comparisons retain their original references and stop.

| Reference issue | Independent check and correction |
| --- | --- |
| cmap4 glyph-array delta omitted | [AFC1140](AUTHORED-FONT-CMAP4-CORRECTION.md) isolates the nonzero forward-array delta against a source-built control. |
| Public `TabWidth` ignored | Stock widths 1 and 4 both advance 8 px; copying the adjusted advance gives 4.8 and 19.2 px. |
| Kerning repeated for each same-face formatting run | `AV AV` measures 40.640003 px in one run, 35.52 px in three unchanged runs. Positioning each distinct face once restores 40.640003 px. |
| Compound point attachment and horizontal phantom placement | FreeType 2.13.2 exactly matches all four managed compound advances/ink boxes. Correct point attachment, inherited metrics and side-bearing normalization in the reference. |
| Same-pixel raster spans overfilled | A public path enclosing a 0.25×1 pixel rectangle produces alpha 65,535 rather than 16,384. Accumulating its actual span width restores exactly 16,384. |

The original Drawing checkout is revision
`9911bc204b7eca0cbdc388bf1107958367d32817`; Fonts remains
`eadc8dbc041d1b335ecf790ce400b5e069f075de`. Both use shared infrastructure
`353b9afe32a8000410312d17263407cd7bb82d19`. Corrected artifact hashes, exact patches,
build commands, failed build diagnostics and untouched stock/source controls are
in the retained raw evidence. Drawing's source-built control reproduces all six
stock-Drawing observations exactly. Its compile-only `nameof` spelling correction
preserves the attribute string; repository-query settings are recorded explicitly.
Upstream sources/binaries remain ignored external tooling, never production inputs.

Fixed tab increments still differ from the required next-stop policy. The public
adapter measures space width, adjusts only invisible tab runs and reconstructs
the full layout until their advances reach the next four-space stop. Every call
stays inside timing: **nine public layout calls, two rounds** for each frozen
control. Visible text/font sizes, wrapping and font bytes are unchanged. Only
verified empty tab placeholders are normalized out; their raw placement records,
subsequent positions, clusters, bounds and complete frames remain checked.
A separate unwrapped two-tab check reaches 19.2/38.4 px without managed positions.
An exploratory wrapped two-tab case drops a reference tab and remains unsupported
by this adapter; neither it nor a new settings grid is claimed as qualified.

## Correctness and ownership

All six preflight observations match Windows normal and Linux normal/scalar,
including exact managed frames and survivor/budget cleanup. The five controls
pass exact glyph/cluster/line and nominal-advance checks. Positioned pens and
resolved per-glyph ink differ by less than 0.000004 px, below the unchanged
1/64 px tolerance. Raw `hmtx` metadata is not a resolved composite bearing;
the former harness's direct comparison of those different quantities is corrected,
with additional per-glyph ink checks. FreeType separately confirms the geometry.
The [OpenType horizontal metrics](https://learn.microsoft.com/en-us/typography/opentype/spec/hmtx)
and [compound glyph specification](https://learn.microsoft.com/en-us/typography/opentype/spec/glyf)
define the independent placement checks.

Clipped RGBA8/16 visual means are **3.082/3.018**, RMSE **5.599/5.499**, coverage
errors **0.313%/0.304%**, edges **0 px**, in normalized 0..255 units. The unchanged
gates remain mean≤8, RMSE≤16, coverage≤2%, edges≤1 px. Full 16-bit samples are
preserved. The previous failing RMSE 16.193/16.121 is retained, not discarded or
relabeled. Managed frame hashes and every fixture byte remain unchanged.

## Complete operation boundaries

The supplied face is prepared outside both timers. Layout includes independently
owned positioned outlines/results and disposal. The two render controls also
include layout, coverage and composition into a borrowed 513×259 canvas at
(-0.25,1.5). Both canvases reset outside **every** operation. Drawing jobs use
one invocation/unroll and an iteration reset; layout jobs retain normal batching.
The first drawing screen is invalid: its inherited generated BDN unroll delegate
made 16 calls under a one-operation job. Both elapsed and GC totals expose that
mismatch. The corrected driver binds single-call delegates for drawing and checks
for a new reset before every draw. Initial samples remain retained as a tooling
defect, not performance evidence; layout screen batching is unaffected.
Necessary reference adaptation, all public calls, path/glyph/advance arrays,
optional public Fill and disposal stay inside. There is no fitting search in
these controls. Eager/lazy validation, font caches and retained reference storage
differ; these are repeated supplied-face operations, not one-shot font loading
or overall text-processing performance. Historical stage costs are never summed.

All five **pass and stop** after independent confirmation. Each row retains all
12 managed actuals and both independent 12-actual reference brackets, with every
versioned idle/process guard passing on CPU 0, one processor, optimized JIT and
nonconcurrent workstation GC. No favorable sampling retries or discarded actuals.

| Complete operation | Managed / reference µs | Conservative ratio upper | Managed / reference allocated B |
| --- | ---: | ---: | ---: |
| OriginalUnicode layout | 2.585 / 117.234 | 0.023246 | 5,592 / 367,584 |
| PackedBmp layout | 2.610 / 120.738 | 0.022768 | 5,088 / 385,016 |
| CompositeTransforms layout | 2.604 / 112.021 | 0.023809 | 6,152 / 366,960 |
| RGBA8 layout + drawing | 40.938 / 223.704 | 0.305806 | 49,448 / 417,912 |
| RGBA16 layout + drawing | 45.314 / 224.872 | 0.331074 | 49,448 / 417,912 |

Allocation columns use separate warmed complete public-operation/disposal
diagnostics, three batches of 128 with all observations retained. Raw BDN GC totals
are also retained: single-invocation drawing includes 736 B of common harness
scaffolding beyond these public-operation figures. Windows managed drawing has
one 5 B/op diagnostic variation; no allocation observation is filtered away.

Total GC allocations, governed peak capacity and surviving retained layout are
separate observations; physical/JIT/native peaks and reference retained capacity
are unmeasured. No production pool, cache or ownership change is introduced.
Layout-only governed peaks are **12,064/11,816/12,696 B**, with surviving layout
charges **10,380/10,308/10,388 B** after font disposal. Rendering peaks are
**578,240/1,109,708 B**, including standing font/canvas capacities
**532,972/1,064,440 B**; surviving layout remains 10,380 B. Every shared root returns
to zero after its owners dispose. Allocations are not a claim about physical peaks.

Reuse unchanged 6,749 full/live checks per OS and SAME actual Release-package
consumer/all three examples from [JSC1137](JPEG-BATCHED-ENTROPY.md). No production,
embedded/public test, VM or package builds are needed for these tooling changes.
Focused complete-output/ownership checks cover ten Windows sides and twenty
Linux normal/scalar sides. Publication remains postponed.

[Evidence](authored-font-comparisons-evidence.json) identifies immutable producers,
corrected and stock references, source patches, sample/GC records and qualification
reuse. Its mirrored, member-verified raw archive retains fixtures, full frames,
source controls, minimal reproductions, exact compiled tooling, all guards/actuals
and the invalid initial drawing screen. Restore the archive and its declared
dependencies; the archived `aft1143-deploy.py` prepares focused checks and
`aft1143-measure.py` runs the bounded screen/confirmation. `ats1142-build-drawing-reference.py`
and the exact external patches reproduce the explicitly labeled reference changes.
No upstream source/binary is tracked or used by the production build.

All 17 inherited names now have performance evidence: **15 pass, two copy gaps**.
The full 289-path objective remains open. Next: ordinary-text uncertainty, then
meaningful owned-loading costs and copy gaps, ranked by absolute savings/coverage.
Complete DejaVu and default-variation passes remain stopped. A final immutable-
package full refresh/qualification follows resolution of the remaining paths.
