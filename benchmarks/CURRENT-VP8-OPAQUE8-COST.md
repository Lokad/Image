# Current opaque RGBA8 VP8 cost

The existing 513×259 opaque byte-input control independently confirms a
**1.268× gap / upper 1.308×**, with an exact **72,380-byte packet**. This fills
missing current evidence, without a production change or causal gain claim.

| Complete warm one-CPU operation | Lokad µs | FFmpeg µs | Ratio / upper | Managed B/op | Packet B, both |
|---|---:|---:|---:|---:|---:|
| Opaque RGBA8 → VP8 | 19,848.951 | 15,654.148 | 1.268 / 1.308 | 1,201,552 | 72,380 |

Both subjects retain original RGB, alpha 255, frozen Q75/effort4 and VP8/4:2:0.
Complete operations include all source processing/repacking, encoding, fresh
independent output acquisition and release. No alpha-plane encoding or word
down-conversion is required. Native allocation is unmeasured. Complete packet
equality establishes equivalence, without numeric-quality matching assumptions.

All twelve actuals, filtered/unfiltered confidence, native brackets, exact
packet/pixel/alpha/source/ownership checks and both idle-v2 guards pass. The
conservative lower bound is 1.226. Old 333,580-byte timing evidence predates the
packet-exact encoder and current QIV1 quantizer; it remains historical. Other
current VP8/VP8L comparisons and applicable qualification remain valid.

Zero production/harness/host/native/selector builds, new fixtures/settings,
profiles or repeated full/package checks. The existing generic driver reads the
original enum value from the environment. Two selector-preparation checks failed
before any benchmark; their logs are retained. The completed twelve was recovered
after a collector incorrectly required a word-normalization reference flag for
byte input. The existing byte protocol correctly reports no such reference;
frozen raw input, complete packets and every original gate pass. No timing rerun
or baseline/quality adjustment occurred.

Applicable 5,369 checks/OS, 948 live comparisons and the SAME actual Release package/
three examples are reused. The summary now has 105 comparisons in three tables;
all eight displayed VP8 encoder controls have independent costs. All 268 required
paths remain in scope; full warm parity and replacement readiness remain open.

[Evidence](current-vp8-opaque8-cost-evidence.json). Mirrored/member-verified
current-vp8-opaque8-cost.tar.xz, 1,975,472 B/67 members,
SHA256 2d6e650d5e38aca7d5a220706455e1ea8a3b7ffe59ae23d8c4225a7f2e747224.
Raw retains all samples, brackets, guards, recovery/failure logs, actual cached
binaries/driver/worker, original fixtures and pinned qualification dependencies.
