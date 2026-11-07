# Successful-block publication in the live VP8 reader

CPT479, 2026-11-07. Accepted architectural simplification; publication postponed.
[Current evidence](vp8-completed-block-publication-evidence.json) owns the existing
`OpaqueCancelable8` cost. All283 named required paths remain in scope and incomplete.

## Failure lifetime and exactness

The cancellable coefficient kernel published private arithmetic state in a
`finally` block. Its only production caller is `Vp8Decoder.Coefficients`, inside
`ReadPlanes`; there is no catch, retry or cursor resumption. `WebpCodec` immediately
consumes a fresh private decoder for pixels or validation. A failed operation
abandons that decoder and unwinds through its operation-owned storage scopes.
No cursor or partial planes escape.

Publication now occurs once, on successful block completion. The entire coefficient
loop and original moving-value arithmetic are unchanged: model/token/category/sign
order, every entry/decision cancellation poll, coefficients, canonical successful
cursor and following reads remain exact. Only abandoned private exception state
loses its resumability invariant. Public cancellation/exception tokens, storage
cleanup, stream delivery, source/independent survivors and actual async lifetimes
are unchanged. There is no new owner, capacity, cache/session or fallback.

The mechanical source projection reconstructs the entire qualified original file.
Original tests are restored byte for byte after the separate representation study.
Actual1024-block scalar/coefficient/cursor/continuation, phase/max-symbol and
token/category/sign controls pass; standalone decision cancellation ordinals still
check the real helper before every read and canonical publication. They do not
promise that an abandoned complete decoder may be resumed after failure.

## Complete quiet-VM operation

Release, one pinned CPU, frozen513×259 opaque VP8 input72380B and exact531468B
RGBA output. Complete parsing, preparation, reconstruction/filtering/conversion,
independent output ownership and disposal remain timed. Native codec/context state
remains retained as declared. Confirmations have twelve managed actuals and twelve
samples per native bracket, original pixel/source/ownership gates and idle-v2 guards.

| Existing live opaque decode | Managed µs | FFmpeg µs | Ratio / upper | Managed GC B/op |
| --- | ---: | ---: | ---: | ---: |
| Contemporary original, independent confirmation | 5,154.87 | 4,789.34 | 1.076 / 1.081 | 228,864 |
| Accepted, independent confirmation | 5,072.31 | 4,798.30 | 1.057 / 1.065 | 228,864 |

Saving **82.55µs/1.60% raw**,1.79% native-normalized; native moves+0.187%.
Filtered and all-actual raw/normalized intervals separate. The remaining274µs
point gap is material; this is not parity or a general decoder speedup claim.
No unchanged favorable resampling or fixed continuation threshold.

## Representation study retained, not selected

VFW478 independently derives UInt64 high56 alignment with32-bit refills.146880
bounded algebra witnesses and original focused functional checks pass. Its short
original/candidate costs5315.66/5278.40µs show only37.26µs/.70% point improvement
with wide overlapping intervals and unchanged228864B/op. That screen establishes
neither gain/loss nor equivalence, and has no independent confirmation.
All source/modules/raw are retained; arithmetic and private test adapters restore
to the qualified original before CPT479. The older
[UInt32/fixed24 experiment](VP8-FIXED-ALIGNMENT-WINDOW-REJECTED.md) used a different
validation boundary/refill and former5% policy; neither result bans the family.

## Storage, generated code and qualification

GC allocation is fresh above. Numeric owner/retention evidence reuses unchanged
[output ownership](VP8-OVERWRITE-OUTPUT.md): governed coarse peak747020B, retained
result531468B until disposal; default sequential idle pixel capacity531468B in the
existing capped pool, bypassed for shared budgets. No new capacity or retained
state. Current functional/package checks requalify refusal/cancellation/cleanup,
borrowed roots, survivors and actual async users. Physical/native peak is unmeasured.

The required Linux actual-package consumer also captures optimized code, with no
extra job/build/timing. Pixel kernel shrinks3789→3195B; stack frame stays232B.
Static stack-operand instructions fall381→124, including cold paths. These are
descriptive counts, not dynamic traffic or predicted savings; they do not prove
complete register residency. Original JIT is source-valid retained LDA463 evidence,
current JIT uses the actual Windows-built package on Linux10.0.8, tiering off.
Different pinned modules are disclosed. Validation-mode JIT/functionality also
pass; no separate live-validation CPU claim is made.

Both stages use one immutable core/embedded build per OS: two total each, because
production changed between stages. Public/harness/selector/native/fixture/diagnostic
builds remain zero. Accepted stage passes **5837 full/live checks per OS**
(2572internal/2317public/948differential),40internal/239public per normal/scalar
mode, and the SAME actual private Release package/all three examples on Windows
and Linux. Other ISA and unchanged-path proof is reused. No displayed numeric
table row changes:125 numbers reuse valid paths, and the current live cost appears
in the summary prose. This is not a125-row actual-package release refresh.

## Reproduction

Raw `vp8-completed-block-publication-qualified.tar.xz`, SHA256 `8e5d2d91fd10e69d94adf93b9a9e717caacc53fe811cfe1751ffd38974c5b006`, 2228108 B/139 members,
is mirrored and every member verified. It retains both stages, original/current
sources/modules, cached input providers, samples/guards, call/ownership projection,
algebra witnesses, JIT and full Windows/Linux/package logs. Its `tmp/vfw478-*`,
`tmp/cpt479-*` and `tmp/vpa477-*` scripts reproduce the bounded experiments.
The preceding [VP8L milestone](VP8L-FINAL-PUBLICATION.md) owns nine unchanged reader
costs, including its explicit palette regression; its global qualification is
superseded here. Publication remains postponed.
