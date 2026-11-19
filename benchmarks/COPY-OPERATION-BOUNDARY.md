# Complete copy-operation reference boundary

**Measurement/tooling correction; no production change.** Dynamic clipping and
address preparation now run inside every native copy operation. Two byte-copy
gaps remain; separate-word confidence is open. No historical-clock difference is
a Lokad.Image speedup.

| Complete borrowed operation, original inputs | Managed / FFmpeg adapter µs | Conservative ratio interval / status | GC B/op |
| --- | ---: | --- | ---: |
| Separate RGBA8, 256×129 | 1.952957 / 1.905536 | 1.012121–1.037472 / gap | 0 |
| Clipped RGBA8, 254×129 effective | 1.982622 / 1.913508 | 1.021465–1.049530 / gap | 0 |
| Separate RGBA16, 256×129 | 3.827293 / 3.816783 | .991834–1.011718 / uncertain | 0 |

Two prior adapters had different boundaries. The fixed separate-copy worker
`f0bcf912` (source `34517087`) has no clipping: plane arguments are prevalidated,
and its sample loop calls `av_image_copy_plane` with prepared addresses. The
extended clipped worker `b16ae1cc` (source `e2bff073`) contains clipping, but GCC
versions/inlines the loop and hoists geometry/address preparation in its fast
paths. Not every version removes every conditional; the retained assembly shows
the distinction. Neither establishes the documented per-operation preparation.
Their results remain prepared-native diagnostics in the linked history.

The explicit correction marks the independently authored `apply` wrapper
`noinline,noclone`. Reconstructing the prior source proves its operation body is
unchanged. Actual assembly now calls `apply` for every operation; that function
loads geometry, clips, forms addresses, calls public `av_image_copy_plane`, and
observes output. Native pointer/capacity validity is still prevalidated; managed
public validation, limits and cancellation remain timed. This is a bounded
adapter comparison, not a general native public-API equivalence claim.

One corrected worker build produces `911f1946` from source `f2d1a375`, with the
same GCC13.3, `-std=c11 -O3 -Wall -Wextra -Werror`, build script `c324b12f` and
FFmpeg n8.1 `libavutil.a` `8c94c7bf`. Frozen workers are untouched. Original
513×259 inputs, geometry, quality/pixel gates and ownership remain. The actual
immutable package `f4c5b5db` supplies timed DLL `7b59eabe`; production, embedded
tests, harnesses, drivers, native archive, fixtures and package have **zero rebuilds**.

All three normal/scalar preflights pass. Every full canvas, source, alpha, hidden
RGB, untouched pixel, caller lifetime and native identity gate passes. CPU0,
optimized non-tiered JIT, natural nonconcurrent GC and idle-v2 guards remain.
Each comparison runs once with all twelve statistical units/group and three
native groups retained. Separate byte uses its original interleaved driver
`9337b7ae`, 65,536 operations/unit; clipped byte and word use original partitioned
driver `413b6e5e`, 65,536/262,144 operations/unit and sixteen subdivisions. Original
warmups and conservative statistics remain; no favorable filtering/replay.

Preparation failures are retained: an initial source-body proof compared the two
different adapter versions; missing mini-root markers selected the parent during
an untimed preflight; an incompatible byte driver rejects before actuals. Exact
matching source, root markers and the original byte driver correct them. All
three scored captures run once. Recipes, failures, source identities and assembly
make these repairs reproducible.

Added owned/admitted/retained capacity remains **0 B**. Caller storage and
unmeasured physical/native/JIT peaks stay separate. Unchanged production reuses
[pending-publication qualification](JPEG-PENDING-PUBLICATION.md): **6,844 full/live
checks each OS**, the SAME actual package and all three examples on both OSes.
No broad/package repetition follows a reference-only correction.

Prioritize the two demonstrated byte gaps by absolute savings. Word and async
probe uncertainty are not proof of optimization need. Unchanged passing self
copies, flips, drawing, fonts/text, codecs and resizing remain stopped with their
identified source-valid evidence. All **289 material paths** and the final
**165-comparison immutable-package refresh** remain required; publication is postponed.

[Evidence](copy-operation-boundary-evidence.json) pins every identity and receipt.
Mirrored/member-verified `copy-operation-boundary.tar.xz`: **492,168 B /90 members**,
SHA256 `74ac5f858e5ce39b7c081b2e493b822d9595d9f33512ccae4a1295984a115dd1`.
Archived `cbr1262-*` recipes, original drivers, validation, assembly and actuals
reproduce the round with its named dependencies. Superseded measurements remain
in [self-row history](SELF-ROW-STORAGE.md), [byte reconciliation](BYTE-COPY-REFERENCE-RECONCILIATION.md)
and [partitioned diagnostics](COPY-PARTITIONED-REFERENCE.md); rejected managed
experiments remain in their own reports.

## Placement-balanced RGBA16 confirmation

The current `d52e53f2` package measures **3.812207 µs** against **3.792899 µs**
for the same FFmpeg plane-copy operation. The conservative interval is
**0.995051–1.014625** and the adjacent-pair interval is **0.999432–1.010360**.
The word result remains uncertain at **0 B/op**.

The driver allocates twelve independent source/canvas backing pairs before
timing. Each statistical unit uses one pair, with equal counts in both roles
and alternating managed/native order. It retains 16 subdivisions and 262,144
calls per unit. All before, paired and after reference groups stay in the
original confidence calculation. Original whole-run warmups remain and each
backing assignment receives equal managed and native warmup before timing.

The frozen managed delegate and consumer invoke the complete public operation.
The reference uses the previously verified in-process adapter with the frozen
`apply` and FFmpeg plane-copy instruction bodies. The benchmark holds both sides’
backing arrays live and pinned outside the clocks. It resets each assignment
before its warmup and checks the exact full frame and unchanged source afterward.
Original normal and scalar checks pass on all 24 assignments. All quiet guards
and sample counts pass. The standalone comparison above records the earlier
independently allocated boundary.

Eleven of twelve matched units have higher managed means. The result leaves
parity unresolved after controlling the sampled backing roles.
[Observations, driver and archive](copy-vector-width-trials-evidence.json)
