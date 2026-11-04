# Shared Lanczos row driver

One driver now owns result allocation/disposal, row-ID initialization and vertical
window/slot scheduling. Exact and certified execution keep their typed kernels,
private in-place row views, Q30 weights, accumulation order, certificates and
uncertain-pixel repair. Dispatch occurs at row/tap boundaries, with no generic
per-pixel abstraction. The duplicate FMA driver is removed.

Independent prepared byte reduction improves **603.45 → 579.45 µs**, saving
**24.01 µs / 3.98% raw / 4.34% reference-normalized**. Filtered/all-actual raw
and conservative normalized confidence separate. Its native clock is **0.380%
slower**, disclosed. Word enlargement/reduction points rise 0.65%/0.90%; intervals
overlap, with no gain, equivalence or causal regression claim. No repeated timing
to select favorable points; unchanged-path baseline confirmations are reused.

| Existing 513×259 / zimg; reduction 256×129, enlargement 1027×519 | Lokad µs | Reference µs | Ratio / upper | Allocated B/op | Plan retained B |
| --- | ---: | ---: | ---: | ---: | ---: |
| RGBA8 reduction, one-shot | 774.18 | 1,707.18 | 0.453 / 0.457 | 138,647 | 0 |
| RGBA8 reduction, prepared | 579.45 | 490.59 | 1.181 / 1.201 | 139 | 139,800 |
| RGBA16 reduction, one-shot | 761.86 | 1,949.02 | 0.391 / 0.395 | 146,864 | 0 |
| RGBA16 reduction, prepared | 568.64 | 509.71 | 1.116 / 1.121 | 139 | 148,040 |
| RGBA8 enlargement, one-shot | 4,235.82 | 5,671.99 | 0.747 / 0.756 | 319,229 | 0 |
| RGBA8 enlargement, prepared | 3,991.24 | 4,392.76 | 0.909 / 0.915 | 148 | 325,156 |
| RGBA16 enlargement, one-shot | 4,528.68 | 5,069.81 | 0.893 / 0.897 | 4,592,777 | 0 |
| RGBA16 enlargement, prepared | 4,211.41 | 3,894.83 | 1.081 / 1.100 | 4,265,302 | 333,396 |

All eight costs are current on one immutable selected build. Five still pass and
stop; prepared byte reduction and both prepared word boundaries retain gaps.
The five passing-boundary refreshes establish current costs, without additional
incremental claims. Four refreshed native clocks are roughly 7–9% slower than older
observations, with raw managed costs also higher; clocks/raw deltas remain in the
evidence. No control/settings grid expands. One-shot includes geometry/workspace
preparation and independent output/disposal on both sides; prepared operations
exclude explicitly retained plans/scratch. Source processing and normalization
stay timed. Native float does not relax exact managed pixels/full 16-bit samples
or existing native RGB/alpha max≤1/RMSE≤0.25 gates.

No new scratch array, admission request, cache/session or owner model is added.
Plan retention/capacity is unchanged; output pooling is unchanged and excluded
for governed calls. Word enlargement's 4,265,302 B/op is mostly its required
4,264,104 B independent output. B/op, live admitted capacity and retained state
remain distinct; complete physical/admitted peaks and native graph/input-plane
retention/allocations are not newly measured. All cancellation checkpoints and
terminal-refusal/exception cleanup remain at their corresponding boundaries.

Both OSes pass **5,569 full/live checks** (2,346 internal, 2,275 public, 948 live),
plus **74 internal/15 public Lanczos checks per normal/scalar mode**. All eight
frozen boundaries pass normal/scalar checking on Linux. The SAME actual Release
package and all three examples pass Windows/Linux, including retained-plan root
admission, cancellation/refusal cleanup and surviving independent outputs.
One core/internal build per OS, no harness/native/selector/public rebuilds and
one package; no production change after measurement. Published packages remain
immutable. Other families reuse valid unchanged-path evidence.

[Evidence](lanczos-shared-row-driver-evidence.json) pins sources, reused baselines,
three shorts, eight independent current costs, clock movement and qualification.
Mirrored/member-verified raw `lanczos-shared-row-driver-qualified.tar.xz`: 18,204,652 B / 299 members,
SHA256 `d68135ae270a1aad2dbe2c7a47f1d66ff8138af996738ae0af4a9a0303c33bdf`. Sources, actual producers/package, guards,
actuals, frozen checks and reproduction remain. Dependencies pin the prior
qualified package and [paired output/reference evidence](LANCZOS-PAIRED-ROW-OUTPUT.md).
The full 282-path objective stays open; publication is postponed.
