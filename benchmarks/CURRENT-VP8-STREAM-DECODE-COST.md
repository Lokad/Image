# Current VP8 stream decode cost

The existing fresh seekable VP8+ALPH decode confirms a **1.215× gap**, upper
**1.228**, lower 1.210, at **7,279.108 µs** and
**1,966,583 B/op**. The earlier three-actual screen is superseded, without
pooling or a causal before/after speedup claim.

Both subjects consume the same original 98,590-byte, 513×259 VP8+ALPH packet.
Complete operations include fresh non-exposed seekable input acquisition/copies
and EOF, decoding, chroma/color/alpha normalization, fresh independent RGBA8
output ownership and release. All pixels and alpha match the frozen native CLI
exactly; borrowed input and independently surviving outputs remain intact.
Native allocations are unmeasured. Original reference settings are unchanged.

All twelve actuals, native brackets, filtered/unfiltered bounds and both idle-v2
guards pass and are retained. Production, harness, fixture and native worker are
reused. The historical confirmation driver was absent, so one small replacement
selector uses twelve actuals and sixteen unroll calls, matching its job. There
are no production/harness/native rebuilds, new fixture/settings or timing repeats.

Applicable both-OS correctness and actual Release-package/governed consumer proof
remain valid; production is unchanged. The full 268-path parity objective remains
open. [Evidence](current-vp8-stream-decode-cost-evidence.json) pins producers and
the mirrored, member-verified raw archive with reproduction dependencies.
