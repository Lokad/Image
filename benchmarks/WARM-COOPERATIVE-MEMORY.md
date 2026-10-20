# Caller-owned admission: two retained PNG outputs

This baseline is superseded by the
[materialized Deflate cursor](WARM-DEFLATE-MATERIALIZED-CURSOR.md). Its original
measurement and diagnostic remain evidence for the previous producer.

The already named `MemoryBudget/Png8Retained2` operation is now measured on the
unchanged qualified producer. It remains a parity gap; no longer repeat runs
above one. No new fixture or settings grid is added.

| Complete operation | Lokad µs | FFmpeg µs | Ratio / upper | Lokad allocated B/op |
| --- | ---: | ---: | ---: | ---: |
| Two PNG8 decodes, one caller-owned root, overlapping outputs | 4,280.80 | 2,850.88 | 1.502 / 1.567 | 2,574,662 |

Both sides decode the same resident 513×259 PNG twice. The first output remains
alive throughout the second decode; both independent 531,468-byte RGBA outputs
are then released. All samples, alpha and hidden RGB are exact. Native normal/
scalar controls also verify mutation/release of one output leaves the other valid.
Native allocation totals are unmeasured.

Managed timing includes all `ImageLimits.MemoryBudget` admission, uncached pixel/
scratch allocation, decoding, retained reservations and refunds. The finite
32 MiB root and its 218,775-byte resident-input charge are prepared outside timing;
the observed admitted peak is 2,306,560 bytes, distinct from cumulative allocations.
This is the cooperative root contract, separate from the earlier `ImageBudget`
quota benchmark. Native FFmpeg includes two complete decodes, independently
owned frames and releases; its private codec/padded packet/frame wrappers are
prepared outside timing. Native has no admission API: managed accounting is
explicit additional work, with no output or operation omitted.

Setup/cleanup verify exact retained charges, borrowed root/input, source stability,
surviving ownership, terminal refusal, cancellation cleanup and subsequent success.
The existing 36 cooperative-memory controls pass normally/scalar on each OS after
test-adapter cleanup is changed to remove disposed references. No production
governor, new library allocation tracker or production change is introduced.
The unchanged 4,478-test/OS and same actual private-package/three-example proof
are reused. The harness builds once successfully per OS, with one Windows
constructor-overload correction; the native adapter builds once. The existing
Decode8 selector is reused. One short retains all three actuals, both native
brackets and both official idle-v2 guards.

One subsequent **diagnostic-only** one-second profile of this same producer and
case retains 924 samples and both guards: `ZlibFraming.Walk` 49.6%, literal copying
22.0%, long-literal decoding 8.0%. These locate sampled execution, not measured
speedups. Admission is not the dominant sampled cost. Next: establish feasibility
for one fused materialized-block bit cursor, preserving exact work, malformed-input
failure ordering, output bounds and original counting/cancelable paths. Earlier
literal-only batching and local-work trials are not repeated unchanged.

Coverage stays finite:254 named cases, now238 numeric and16 unmeasured;
72 independent passes,165 mean gaps and one confidence-open. Functional memory
qualification does not imply performance parity. Text/Lanczos3 functional scopes
retain their proofs; full parity and release readiness remain open.
[Evidence](warm-cooperative-memory-evidence.json) retains statistics, allocations,
controls, producer/worker/source identities and mirrored raw archives.
