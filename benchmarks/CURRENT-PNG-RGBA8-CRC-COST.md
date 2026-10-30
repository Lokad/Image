# Current RGBA8 PNG decode cost

The existing 513×259 resident RGBA8/None control confirms a **1.092× gap**,
upper **1.098**, lower 1.085, at **1,520.358 µs** and
**4,472 B/op**. Its older cost is historical after the shared CRC change.
This is one current observation, without a causal before/after gain claim.

Both subjects decode the same frozen packet into fresh independently owned RGBA
pixels, including normalization and release. Every RGBA, alpha and hidden RGB
sample, unchanged source, settings, native worker and original ownership boundary
passes. Native allocations are unmeasured.

All twelve actuals, native brackets, filtered/unfiltered confidence and both
idle-v2 guards pass and are retained. The immutable CRC build and cached generic
four-operation selector are reused: no builds, new fixtures/settings, profiles or
successful retiming. Existing both-OS correctness, actual Release package and
governed consumer qualification remain valid; production is unchanged.

The source review finds the accepted materialized block cursor and direct None
row copy already in use. Earlier literal/refill/classifier/token-loop trials
remain closed. No new substantial candidate follows from this observation.
The full 268-path warm parity objective remains open.

[Evidence](current-png-rgba8-crc-cost-evidence.json) pins producers and the mirrored,
member-verified raw archive with reproducibility dependencies.
