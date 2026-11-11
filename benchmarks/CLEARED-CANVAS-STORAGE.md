# Cleared canvas storage

Public zeroed canvases now reuse the existing bounded pixel storage. On the frozen
[complete public text control](TEXT-COMPLETE-PUBLIC-PIPELINES.md), the eight-draw
batch saves **665.54 µs /24.46% raw /29.86% normalized** with separated raw and
normalized confidence. Native time moves +7.69%; those views remain distinct.
One-shot remains a confirmed pass, but its incremental timing intervals overlap.
No encoder, decoder, resizing, font parser, layout or raster arithmetic changes.

| Warm complete boundary,384×160RGBA16 | Lokad µs | ImageSharp µs | Ratio / upper | GC B, Lokad / reference | Status |
| --- | ---: | ---: | ---: | ---: | --- |
| One-shot font→fit→layout→draw→dispose | 1,313.88 | 6,739.69 | 0.195 / 0.218 | 283,275 / 9,256,953 | pass |
| Prepare + eight owned fresh-canvas draws + disposal | 2,055.00 | 7,634.49 | 0.269 / 0.302 | 284,623 / 9,494,633 | pass |
| Preparation creation only (diagnostic) | 1,167.36 | 6,679.23 | 0.175 / 0.197 | 283,282 / 9,227,176 | pass |
| One owned fresh-canvas draw after preparation (diagnostic) | 105.65 | 81.93 | 1.290 / 2.006 | 169 / 33,433 | uncertain |
| Fresh canvas create + explicit clear + disposal (diagnostic) | 7.18 | 6.11 | 1.176 / 1.198 | 113 / 1,049 | gap |
| Existing canvas clear + prepared draw (diagnostic) | 103.39 | 78.81 | 1.312 / 1.609 | 57 / 32,433 | gap |

Reference is **ImageSharp.Drawing2.1.7 / Fonts2.1.3 / ImageSharp3.1.12**, not FFmpeg
or a bare shaping engine. Scope remains DejaVu Sans Regular2.37, `AV To AV To`,
OpenType/kerning, wrap96/height30, maximum24/minimum8/step.25, selected16.75 after
30fitting trials, origin8/8, RGB20/40/60. Borrowed font/options and every public
fitting/layout call, lazy first use, canvas creation/explicit clear, independently
owned output and disposal remain in their declared complete boundaries.

The directly measured batch amortizes to **256.87/954.31 µs/draw** including
preparation and cleanup. Preparation creation is a separate diagnostic, with
prepared-owner cleanup outside its clocks; complete batches include cleanup.
Separate stages are never added to infer an end-to-end cost. Steady fresh-canvas
drawing is uncertain (0.850–2.006), and existing-canvas drawing remains a scoped gap.
No arbitrary repeat-count, general-font or overall text-slowdown claim is made.
Parser eager/lazy validation differs; reference font ownership drops references
for GC reclamation, while managed admission releases promptly. Managed preparation
owns raster workspace; reference retains positioned/flattened paths and allocates
raster storage per draw. These remain integration comparisons.

Fresh canvas GC falls491,657→113B; one-shot774,915→283,275B; eight-draw batch
4,217,697→284,623B. These totals are not peaks or retention. Managed active prepared
capacity remains39,904B and governed peak807,708B. Current approximate full-GC
prepared-owner deltas are34,256/22,504B; raw before/live/after values are retained.
Default idle capacity now includes released canvases: this repeated font/canvas
sequence can retain757,076+491,520=1,248,596B, separately from live owners.
That occupancy follows the source and observed no-array-allocation steady samples;
it is not a physical heap census. The existing global ceiling stays **two exact-size
arrays /4MiB**. Physical/native/JIT peaks remain unmeasured.

Cleared hits are private through full clearing, with direct cancellation checkpoints.
Cold misses retain CLR zero initialization without redundant page touching. A
size miss keeps one recent other size only when it fits alongside the canvas,
leaving a slot for the original return policy. Concurrent fills may prevent reuse;
they never alias live storage or enlarge the cap. Governed calls still enter the
original admitted allocation before any pool access: refusal is terminal, with no
pool fallback. Original ImageBuffer fields, constructor, disposal, internal
Allocate/overwrite/Clone/row/sample bodies and all old pool/font bodies are verbatim.
Thus existing codec/resize timing and ISA evidence remain valid; public Create
only prepares their fixtures outside clocks. Mixed font/image effects are included
in both complete text workloads. No general caching framework or new owner is added.

Focused tests cover dirty/zero publication at both depths, live sibling independence,
two-slot/cap/interleaved-size behavior, concurrent leases, cancellation/recovery,
aggregate admission, one terminal refusal and subsequent success. Original full-word
16-bit frames, glyphs/clusters/lines/fitting remain exact, reference positions/bounds
within1e-4pixel. Approved normalized visual alpha mean4.010/RMSE6.920, coverage1.542%,
edge0 retain gates8/16/2%/1pixel. No numerical gate changes.

Three-pair complete-pipeline screens precede independent12alternating-pair
confirmations,32operations/sample; canvas diagnostics use12pairs/128operations.
All actuals, before/after reference brackets, conservative paired/all-actual views,
CPU0/tieringoff/nonconcurrentGC and idle-v2 guards are retained. Existing-canvas raw
time moves +8.36% while reference moves +9.05%, with overlapping normalized intervals;
no demonstrated production regression is hidden. The first candidate's useful
confirmations remain evidence; the final revision removes its common ownership
flag/branch. No favorable predecessor repeat or new workload grid.

Only after affected costs reconciled: **6,275 full/live checks per OS**,
183internal/162public focused normal/scalar per OS, then **the SAME actual Release
package consumer/all three examples on Windows/Linux**, including dirty cleared
canvas/surviving-owner checks. Unchanged codec/resize/ISA proof is reused.
Published packages remain immutable. Current private0.2.1 package
`aabb3a2b416b39694b79c20f064b2f77687b4a47be7eca8af94d0dd6dd482c45` contains the timed DLL
`8e01913c3a55e91a897a0b70ac029cfb07702c869401535131255adc537a287c`. Publication is postponed.

[Evidence](cleared-canvas-storage-evidence.json) pins sources, all affected costs,
retention, invalidation and qualification. Mirrored/member-verified
`cleared-canvas-storage.tar.xz`:5,048,284B/274members,
SHA256 `2fd988788447e31dafed4345889e4da114339bf53c394b7ee13caace8d992bf7`. Dependencies retain the unchanged tools/references,
original font/license/fixtures and prior full-library evidence. All289required
paths remain; these scoped gains do not declare whole-library parity.
