# Text edge-capacity trials

**Both implementations remain unpromoted.** They reduce ordinary-draw allocation
by 38.4%, but the first establishes no CPU gain and the retained-hint revision is
confirmed slower. Production, its qualified package and current benchmark costs
are unchanged. These implementation results are not a permanent family ban.

| Complete ordinary RGBA16 public draw / paired DLL diagnostic | Baseline / prototype µs | Ratio interval | Baseline / prototype GC B/op |
| --- | ---: | ---: | ---: |
| Separate capacity pass, three-pair screen | 47.134 / 47.917 | 0.947263–1.091045 | 83,240 / 51,288 |
| Separate pass, independent twelve-pair confirmation | 49.097 / 49.328 | 0.982785–1.027324, inconclusive | 83,240 / 51,288 |
| Layout-retained hint, three-pair screen | 48.756 / 49.135 | 0.931704–1.090091 | 83,240 / 51,288 |
| Retained hint, independent twelve-pair confirmation | 48.442 / 49.666 | 1.012888–1.037867, slower | 83,240 / 51,288 |

An untimed attribution first established **83,216 B per public draw: 56 B context
+83,160 B actual raster construction**. Actual drawing allocates zero. The existing
timed adapter adds 24 B, separate from public allocation. Three warmed observations
at each depth preserve every frozen sample. The observed curved shape has 448
edges/318 distinct events in 512-element arrays; source-derived superseded growth
payload is 32,256 B, separate from its final 34,304 B edge/event payload.

RCE1154 estimates initial edge capacity from segment curvature before the original
subdivision. It keeps exact arithmetic, edge order, ordinary growth on an
underestimate, cancellation and terminal admission. Nonfinite/saturated estimates
select the original allocation policy before reserving anything. The extra segment
pass removes allocation but supplies no confirmed elapsed gain.

TCH1155 derives the hint during the existing tight-bounds traversal and retains
one scalar with the owned glyph shape, removing the extra draw-time pass. It adds
8 physical bytes per unique shape within the existing 512 B descriptor allowance;
no new array, pool, public API or general cache. Its preparation belongs to layout
and fitting costs, not free setup. Despite that revision, independent complete
draws are **2.53% /1.224 µs slower**, with separated mean intervals. Stop this
implementation; do not repeat unchanged captures for favorable numbers.

Both prototypes allocate **51,264 B/public draw** at either depth; actual raster
construction allocates 51,208 B and drawing zero. Post-preparation admitted raster
storage falls **48,984→45,400 B**. These are coarse logical capacities, not physical
peaks; default caller-owned layout/canvas storage and reference retention remain
separate. No CPU gain follows merely from fewer allocated bytes.

Both OSes pass **441 public checks per normal/scalar mode for each prototype**,
including original font/layout/drawing/admission/cancellation/ownership controls.
The attribution preserves original full-byte/full-word frames and quiet guards.
Paired workers use the unchanged frozen DejaVu face, text, options, public draw,
per-operation reset, full-frame validation and original reference correctness
checks. All actuals remain. The first driver reuses 1,024-operation batches; the
revised implementation uses one predeclared 4,096-operation integration for the
small expected effect. One CPU0 operation runs at a time with natural
nonconcurrent GC. These are incremental DLL comparisons, not ImageSharp or FFmpeg
performance. No reference identity, quality gate or fixture changes.

Only ignored copies were built. Canonical modules and package never change;
reuse **6,760 full/live checks per OS and the SAME actual package/all three
examples**. No canonical-production/embedded/public/original-harness/reference/fixture/
package rebuild or broad qualification campaign follows these unpromoted trials.
Initial tooling/boundedness corrections and all failed preparation logs remain
archived alongside every screen and independent sample.

[Evidence](text-edge-capacity-trials-evidence.json) pins both variants, attribution,
current qualification and scope. Mirrored/member-verified
`text-edge-capacity-trials.tar.xz`: SHA256
`b450ac8b4c369620d9e667a19ff325df5261b9ca7dedaab6e3564963f84a6562`,
35,433,920 B /362 members. Separate allocation-attribution raw archive:
`aba551b4327e77a9e9509663e9a67c8041e91f294493ca1326e7bdae4be4e907`,
406,052 B /36 members. Executable `rce1154-*`, `tch1155-*` and `oaa1153*`
recipes, actual modules/source and pinned dependencies reproduce the studies.

The [original ordinary gaps](ORDINARY-TEXT-LONG-INTEGRATION.md) remain current.
Continue meaningful font-loading and copy/owned-result work by absolute savings
and coverage. Passing paths stay stopped unless affected. The 289-path objective,
final complete refresh and publication remain outstanding. This report supplies
no displayed current costs and is not a renderer dependency.
