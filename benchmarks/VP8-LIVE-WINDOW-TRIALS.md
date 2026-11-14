# Live-token arithmetic-window trials

Neither implementation is accepted. The qualified production source, library,
embedded tests, public bindings and private package are restored exactly. Current
[decode costs](VP8-DECODE-CURRENT-CONFIDENCE.md) remain unchanged: eleven WebP reads
pass; the live-token control retains its1.28%gap. Publication remains postponed.

Both designs reuse arithmetic state across eligible24/25-block macroblocks while
preserving every block-entry/per-decision cancellation check, exact coefficient
math, successful cursors and short-partition fallback. The first passes a separate
data span; the revision reads that same borrowed input from the window. No new
owner, heap capacity, cache, pool or retained state. All samples/owners are exact;
measured allocations stay **230,880 B/op**. Capacity and retention are separate
from allocation; physical/native/JIT peaks are unmeasured.

| Complete managed live decode pairs | Qualified → candidate µs | Ratio interval |
| --- | ---: | ---: |
| Separate input span, screen | 4,857.83 → 4,805.17 | 0.954152–1.025454 |
| Separate input span, confirm | 4,837.34 → 4,801.59 | 0.990790–0.994432 |
| Window input field, screen | 4,848.33 → 4,826.86 | 0.952006–1.041136 |
| Window input field, confirm | 4,840.26 → 4,822.96 | 0.995797–0.997056 |

Independent confirmations separate **35.75µs/.739%** and **17.30µs/.357%** scoped
ALC gains. Screens use3×32operations; confirmations12×128, all actuals retained.
Each isolated module owns its unchanged default pixel store and borrows identical
resident encoded input; complete public Decode, live-token checks, parsing,
normalization, independent result and disposal remain timed. Setup and pixel/
source/owner validation stay outside both clocks. The prepared paired tool is reused.

| Actual public harness / FFmpeg | Managed / native µs | Conservative ratio interval | Status |
| --- | ---: | ---: | --- |
| Separate input span, Opaque8 | 4,730.56 / 4,789.32 | 0.981605–0.992589 | pass |
| Separate input span, OpaqueCancelable8 | 4,878.45 / 4,794.82 | 1.014032–1.020023 | gap |
| Window input field, Opaque8 | 4,762.91 / 4,783.34 | 0.988597–1.001032 | uncertain |
| Window input field, OpaqueCancelable8 | 4,830.93 / 4,776.37 | 1.008609–1.013971 | gap |
| Contemporary qualified baseline, live | 4,885.75 / 4,821.72 | 0.961536–1.033975 | uncertain |

These captures retain12alternating128-operation pairs and all12samples in each
native bracket. Ordinary and live token settings, exact pixels/alpha, source and
output ownership, native identities, CPU0/naturalGC and idle-v2 gates are unchanged.
Native retains its declared codec/conversion contexts. The qualified baseline-only
capture investigates the paired/standalone discrepancy; its variability does not
prove a standalone gain or regression. No candidate capture is repeated. Different
ALC/default-harness execution contexts remain scoped; no cause is asserted.

The complete live gaps persist and the revised ordinary control becomes uncertain.
The smaller state bridges, unchanged storage and scoped gains do not establish a
useful standalone stage that justifies added dispatch. This is a decision about
these implementations, not a permanent restriction on window reuse. Do not replay
them without materially new evidence; continue an independent material item.

Each revision passes **1,039internal/402public normal and scalar checks** on Linux,
including3,136mixed-plane scalar-oracle blocks, every completed pixel/validation
cursor, following reads, partition edges and canceled-decision prefix/borrowed-root
checks. Existing cancellation-ordinal and refusal/cleanup tests pass. Token math
and per-decision polling helpers reconstruct exactly. Two core/embedded builds,
zero Windows/tool/harness/native/fixture builds and zero broad/package repeats.
Both candidates and their tests are retained in raw evidence; prototype-only
production/tests are removed. Reuse the original **6,686full/live checks per OS,
scalar/ISA and SAME-package consumer/all three examples**, without restoration builds.

[Evidence](vp8-live-window-trials-evidence.json) pins designs, source/modules,
every capture and exact restoration. Raw `vp8-live-window-trials.tar.xz`, SHA256
`fb2fdc35e485e434fa2ecde57516ce5d8a77f220a029be0d957b31a668693ec7` (13,943,496B/411members), is mirrored and
member-verified. Dependencies retain current attribution, reference identities
and actual package. All289material paths remain required.
