# Current VP8 emitter profile

One bounded capture reuses the qualified core, prepared worker/host and original
513×259 RGBA8 VP8 Q75/method4 control. Complete normalization, encoding and fresh
independent output acquisition/release run on one CPU. Instrumented times are
discarded; no production/benchmark/native rebuild or settings grid is added.

| Exclusive attribution | Samples | Share of workload samples |
| --- | ---: | ---: |
| `Vp8lReferenceMatcher.Build` | 85 | 11.2% |
| `Vp8PartitionWriter.Write` | 45 | 5.9% |
| `Vp8MacroblockDecision.Luma4` | 43 | 5.7% |
| `Vp8Prediction.SmallBlock` | 39 | 5.1% |
| `Vp8lReferenceCache.FillAllFromHistory` | 30 | 4.0% |

There are 759 workload-scoped samples, 750 managed. These diagnostic shares are
not exact component times or predicted gains. Both idle-v2 guards pass; the full
98,590-byte packet remains exact. The [qualified 1.735× cost](WARM-VP8-LUMA4-RATE-BOUND.md)
remains reusable because JPEG writer changes do not affect this path.

Alpha matching remains the largest individual lead. Recent prefix/storage and
source-energy variants stay rejected. The distinct boolean emitter normalizes
by publishing one shifted bit at a time. A bounded independent arithmetic proof
covers all 255 positive byte ranges ×256 bottoms ×8 pending-bit counts: 522,240
states match direct group publication, including 30,720 cross-byte groups.

This is preparation only. Carry propagation, canonical termination, output-limit
and failure ordering, memory admission and actual native symbol/whole-packet
controls must still qualify any production change. No prototype, speedup or
complete encoder equivalence is inferred from this proof. Try at most one
substantial group-emission candidate and one existing complete-operation screen;
independently confirm only a promising result. Remove no-gain code.

[Evidence](warm-current-vp8-emitter-profile-evidence.json) retains the bounded raw
trace, source/producer identities, validation, arithmetic proof and mirrored
archive. Current regression/scalar/actual-package qualification is reused unchanged.
Full 253-path warm parity and replacement readiness remain open.
