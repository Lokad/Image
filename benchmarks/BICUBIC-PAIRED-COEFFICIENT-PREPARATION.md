# Paired bicubic coefficient preparation

Complete RGBA8 bicubic513x259->256x129 with4KiB working admission saves
**33.29us /6.43% raw /6.48% reference-normalized**,
518.20->484.90us. Filtered/all-actual confidence separates in both views;
native movement is0.056%. Upper1.167 remains a gap.

| Complete existing control | Lokad us | zimg us | Ratio /upper | Allocated B/op | Evidence |
| --- | ---: | ---: | ---: | ---: | --- |
| Byte ->256x129,4KiB | 484.90 | 417.35 | 1.162 /1.167 | 113 | current gap |
| Word ->256x129,4KiB | 560.37 | 572.84 | 0.978 /0.985 | 113 | unchanged-path pass |

One contemporary original and independent final confirmation each retain12actuals.
The first final confirmation has a separated23.49us/4.53% raw saving, but normalized
confidence overlaps while native brackets move418.93->450.41us. Exactly one
targeted immutable-candidate check resolves that movement at484.90/native417.35us.
The stable original518.20/native417.12us is reused. Every uncertain/adverse actual
remains; no further unchanged sampling. Word costs are earlier source-valid
evidence, supported by identical current instructions, not new word timing.

## Preparation, execution and storage

Eligible eight-tap/+2 byte columns store two taps per group as exact doubles
[a0,a1,b0,b1], once per strip in their existing coefficient capacity. Fixed five
source-pair/four-coefficient-group views let the horizontal loop load one group
and use immediate shuffles. Both lanes retain original Multiply/Add and tap order.
Normalizers, geometry, row scheduling, certificates, individual repair, tiny-alpha
unwind, generic/clipped/full/odd and word paths remain unchanged.

Before decoding,128B of the existing decoded-row storage stages the transpose.
After all horizontal users finish, uncertain scalar replay borrows64B there to
restore original coefficients. The existing replay reads original source, not
decoded storage; a future source-row miss rebuilds its full decode extent before
use. No array, heap capacity, root, owner, pool, session or public API is added.
Explicit3232/4080B payloads,4096B admission, independently retained132096B result,
113B/op and retained scratch0 remain. Existing output-pool retention is separate;
governed calls bypass it. Physical/native/JIT-stack peaks remain unmeasured.

Three actual-production tests cover signed-zero/near-half bit layout, untouched
full/generic/odd columns, old-kernel ordered SIMD equivalence at alpha0/1/255/65535,
NaN canaries, zero allocations, scalar half rounding, partial tiny-alpha refusal,
borrowed-root ownership, cancellation before writes and recovery. The existing
151x83->75x41 complete functional control generalizes clipped/odd execution.
An initial test-selector precedence error was corrected with test-only rebuilds;
the weaker source and logs remain. Encoder packets and native gates are unchanged.

The initial variable-permute layout screens512.27us; actual code reloads two
lane-index vectors per step. The immediate layout screens508.62us but retains
repeated byte-span induction/slicing. Final typed views screen493.97us before
independent confirmation. These are distinct architectural stages, with no gain
claims from their screens. Final byte body sizes5891->5934B and6015->5999B,
plus385B preparation/827B rare replay helpers. Word driver/locator/both strips/
OriginalWord instructions match the qualified baseline after process-address and
security-cookie normalization only. Shared math/geometry sources are unchanged.

## Qualification and boundaries

Windows/Linux each pass **6161 full/live checks**,335internal/729public resize per
normal/scalar/disabled-AVX2/AVX512 mode, and the SAME actual Release package/all
three usage examples. Final immutable builds are reused. Three coherent core and
four successful embedded-test versions per OS include the test-only correction;
full/package qualification runs once per OS. Zero harness/native/selector/fixture
builds and no new benchmark image/settings grid. Unchanged word passes stop.

Complete public Resize includes source conversion, geometry/workspace preparation,
filtering, independent output and disposal. The labeled FFmpeg zscale zimg backend
retains graph/input/scratch; halo, packing, filtering, fresh output/release and
approved native float bounds remain timed. This is not equivalent fresh-graph
work. Managed exact pixels remain distinct from native bounds. Quiet idle-v2,
one CPU, sequential Release and frozen source/input/output/ownership gates pass.

[Evidence](bicubic-paired-coefficient-preparation-evidence.json) owns current costs,
source undo, emitted-word reuse, staged results and qualification. Mirrored/member-
verified raw `bicubic-paired-coefficient-preparation.tar.xz`:
12238996B/321members, SHA256
`efa42f668066f45149db2ac81ba01b17554c1728d3bba40ae58d8a9263f2eab8`. Private0.2.1 package442945B, SHA256
`3fb43206fd32d92aac022412f8e422b34aeb6b3ffa6cdf5c28cc1fe7f7ad3800`. Published packages remain immutable;
publication is postponed. [Attribution](BOUNDED-BYTE-COEFFICIENT-PREPARATION-ATTRIBUTION.md)
and [shared publication](BICUBIC-SHARED-BYTE-PUBLICATION.md) retain earlier history.
All283required paths remain; bounded wide-byte~70us and narrow-byte~68us gaps
motivate larger storage/scheduling reuse before more kernel tuning.
