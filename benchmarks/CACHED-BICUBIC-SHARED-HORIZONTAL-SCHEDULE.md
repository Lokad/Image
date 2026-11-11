# Shared horizontal bicubic preparation and execution

The existing complete narrow Down8 Resize improves
**474.98→452.28 µs** against the contemporary immutable predecessor:
**22.70 µs / 4.78% raw / 5.11% reference-normalized**.
Filtered/all-actual raw and conservative normalized confidence intervals separate.
Native time moves +0.35%; that clock movement remains explicit.
Current 452.28/zimg 454.42 µs, upper 1.005, is
**uncertain**, not a parity claim. Wide8 378.51/375.02 µs,
upper 1.020, is uncertain.

All 24 existing shared factory/executor controls refresh on the same immutable
module: **22 pass / 0 gaps / 2 uncertain**.
The eight equivalent wide/narrow one-shot/prepared boundaries remain separately
labeled. Passing cases stop; uncertainty alone does not establish a tuning need.

| Complete operation / explicitly labeled zimg | Lokad / native µs | Ratio / upper | GC B/op | Retained plan B | Status |
| --- | ---: | ---: | ---: | ---: | --- |
| Wide8 | 378.51 / 375.02 | 1.009 / 1.020 | 198,450 | 0 | uncertain |
| Wide16 | 386.38 / 443.04 | 0.872 / 0.885 | 198,446 | 0 | pass |
| Down64x32_8 | 374.11 / 399.28 | 0.937 / 0.946 | 197,683 | 0 | pass |
| Down64x32_16 | 392.41 / 415.54 | 0.944 / 0.960 | 197,677 | 0 | pass |
| Down32x16_8 | 311.98 / 378.45 | 0.824 / 0.840 | 197,299 | 0 | pass |
| Down32x16_16 | 325.12 / 431.05 | 0.754 / 0.791 | 197,295 | 0 | pass |
| Down16x8_8 | 293.59 / 453.82 | 0.647 / 0.652 | 201,205 | 0 | pass |
| Down16x8_16 | 301.94 / 508.48 | 0.594 / 0.622 | 201,202 | 0 | pass |
| Down8 | 452.28 / 454.42 | 0.995 / 1.005 | 199,982 | 0 | uncertain |
| Down16 | 449.79 / 616.20 | 0.730 / 0.737 | 199,957 | 0 | pass |
| Horizontal8 | 408.18 / 577.12 | 0.707 / 0.731 | 199,980 | 0 | pass |
| Horizontal16 | 508.51 / 577.37 | 0.881 / 0.886 | 199,929 | 0 | pass |
| Vertical8 | 457.53 / 588.42 | 0.778 / 0.783 | 367,676 | 0 | pass |
| Vertical16 | 441.31 / 590.82 | 0.747 / 0.753 | 367,597 | 0 | pass |
| Cross8 | 768.30 / 840.72 | 0.914 / 0.918 | 199,947 | 0 | pass |
| Cross16 | 821.70 / 889.98 | 0.923 / 0.936 | 199,891 | 0 | pass |
| wide/Repeated8 | 258.97 / 347.40 | 0.745 / 0.752 | 137 | 198,432 | pass |
| wide/Repeated16 | 331.70 / 415.65 | 0.798 / 0.803 | 137 | 198,432 | pass |
| wide/OneShot8 | 349.70 / 989.97 | 0.353 / 0.358 | 198,455 | 0 | pass |
| wide/OneShot16 | 397.92 / 602.90 | 0.660 / 0.694 | 198,453 | 0 | pass |
| narrow/Repeated8 | 436.53 / 455.85 | 0.958 / 0.968 | 138 | 199,840 | pass |
| narrow/Repeated16 | 396.33 / 572.47 | 0.692 / 0.726 | 137 | 199,840 | pass |
| narrow/OneShot8 | 494.53 / 1057.83 | 0.467 / 0.490 | 199,991 | 0 | pass |
| narrow/OneShot16 | 452.32 / 713.95 | 0.634 / 0.644 | 199,983 | 0 | pass |

Ordinary Resize includes geometry/working-storage preparation, source reads,
exact computation/certification/repair, independently owned output and disposal.
Its zimg 3.0.6 reference retains graph/scratch as originally declared. The eight
one-shot/prepared controls compare equivalent public operation boundaries, with
necessary preparation included or separately retained on both sides. Existing
frozen sources/settings, full-depth output checks, native float tolerances,
ownership and CPU0 idle-v2 gates remain unchanged. No new images/settings grid.

One shared preparation body packs four eligible coefficient records into their
unused tails. One closed generic numeric source emits the fixed narrow 8-tap/+2
and wide 16-tap/+4 schedules. Narrow groups read 14 source pixels instead of 20;
wide groups retain the accepted 28 instead of 40. Each lane keeps its original
ordered Add(Multiply) sequence. Four eight/sixteen-double tails hold 32/64
interleaved coefficients; live weights, offsets, counts, normalizers, old pair
markers and original exact repair stay unchanged. The new group marker uses
only dead slot 14/30. Original pair/adjacent/indexed/odd fallbacks stay verbatim.
The shared BicubicMath primitive and all bounded/bilinear kernels are unchanged.

Canonical narrow preparation has 62 groups using 15,872 B of existing padding.
Those counts and source byte volumes are design facts, not phase times or CPU
forecasts. Preparation runs inside ordinary Resize or owned plan creation.
No new array, owner, reservation, pool, owned capacity or retained state is added.
Wide/narrow plans retain 198,432/199,840 B; total GC and ownership remain distinct.
Down8 GC 199,982 B/op includes amortized instrumentation;
one/few-byte differences from earlier jobs are not an array-saving claim.

A passes exact pixels and has a useful narrow incremental confirmation, but its
wide screen moves adversely and generic span lowering keeps extra scalar
bookkeeping. B uses literal bounded views and loop comparisons in type-selected
branches. Actual emitted narrow/wide kernels shrink 538/616→497/579 B and stack
reservations 48→16 B, without vector spills or FMA. Code size is not timing proof.
Type selection disappears from the numeric loops; no per-pixel policy dispatch.
All A screens/confirmation and original code remain retained. No production
family is banned from an inconclusive implementation.

The original predecessor confirmation 435.54/native 418.81 µs is also retained.
Its raw clock differs materially from contemporary measurements; it is never
silently substituted or added to stage costs. A's initial normalized confidence
overlaps that earlier predecessor, motivating one contemporary predecessor
confirmation before B. No successful candidate measurement is repeated for a
favorable bound. Other clock movements and adverse observations remain in the
comparison evidence; there is no uniform raw-gain claim across the 24 controls.

The SAME actual package consumer checks all four frozen wide/narrow byte/word
plans, cancellation, prompt charge release and independently surviving outputs:

| Consumer platform / geometry / depth | Retained plan B | Coarse governed peak B | Independent result B |
| --- | ---: | ---: | ---: |
| windows wide/8 | 0 | 32,768 | 32,768 |
| windows narrow/8 | 0 | 132,096 | 132,096 |
| windows wide/16 | 0 | 65,536 | 65,536 |
| windows narrow/16 | 0 | 264,192 | 264,192 |
| linux wide/8 | 198,432 | 231,200 | 32,768 |
| linux narrow/8 | 199,840 | 331,936 | 132,096 |
| linux wide/16 | 198,432 | 263,968 | 65,536 |
| linux narrow/16 | 199,840 | 464,032 | 264,192 |

Borrowed input is excluded. Windows can use portable fallback preparation;
Linux uses the cached SIMD schedules. These are coarse admission observations,
not physical/native/JIT peak measurements. Operation scratch retention after
disposal is zero; retained plans/results stay charged through their owners.

Nineteen new narrow cases share the earlier nineteen wide geometry/ordered-bit/
poisoned-padding/guard/cancellation/no-allocation tests and exact public-plan
low-alpha/survivor coverage. Windows/Linux each pass **6,401 full/live checks**,
492 internal/789 public affected checks in normal/scalar/AVX2-off/AVX512-off modes,
including all three named storage suites. Unchanged codec/text ISA evidence is
reused. The SAME actual Release-package consumer and all three usage examples
pass both OSes. Two core/embedded versions per OS, zero compilation failures,
no public/harness/native/fixture builds, one full/package milestone and one pack.

Package `5ba30ae2f57cad85b3880a6dcda207e36d79c13866f386beffe6d5b68a30236d` contains timed DLL `550e5b4ee68f0013e2d19de38a31bb6fdae7a5da3dda81125cb9f0ef881d9209`.
Published releases remain immutable; private 0.2.1 packaging is not publication.
[Evidence](cached-bicubic-shared-horizontal-schedule-evidence.json). Raw `cached-bicubic-shared-horizontal-schedule.tar.xz`:
2,469,368 B / 358 members, SHA256 `e9f8a4afeb1c72ca8d63fb902afed33f674f516233313facb25b32244a4870ce`,
mirrored/member-verified with the prior current-owner dependency.
[Earlier horizontal quartets](CACHED-BICUBIC-HORIZONTAL-QUARTETS.md) retain their
incremental evidence and raw sources; this report owns all current affected costs.
All 289 material paths remain required. Publication is postponed.
