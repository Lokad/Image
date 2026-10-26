# Current WebP costs after cleanup

Historical checkpoint: the [bounded refinement round](WARM-VP8L-BOUNDED-REFINEMENT.md)
supersedes these affected CPU costs. Raw observations and qualification identities
below retain their original producer scope.

Two existing 513×259 RGBA8 anchors were screened on the final qualified cleanup
library. Complete operations include normalization/repacking, encoding, fresh
independent output acquisition and release, on one CPU. Reference settings remain
Q75/method4; VP8L uses the approved libwebp exact=1 backend. No input, effort or
quality sweep was added. Complete packets match their frozen references.

| Three-actual screen | Lokad µs | Reference µs | Ratio / upper | Allocated B/op | Packet bytes, both |
| --- | ---: | ---: | ---: | ---: | ---: |
| VP8 | 94,930.99 | 53,630.43 | 1.770 / 2.015 | 24,424,944 | 98,590 |
| VP8L | 226,254.12 | 121,530.92 | 1.862 / 2.230 | 48,232,248 | 183,812 |

Both pre/post versioned quiet guards pass. All actuals, filtered results, native
brackets and raw GC counts are retained. Broad short-screen confidence is not a
demonstrated regression. Neither case passes the ≤1× target; no independent repeat
or cleanup speedup is claimed. VP8L has the greater absolute excess elapsed time
and allocation on these controls.

The earlier two screens preceded the final fuel-only copied-count pass removal
in the shared cache scorer. Their raw results remain historical. Only the two
affected VP8L/compressed-VP8-alpha anchors were refreshed. The same immutable final
qualified core, unchanged harness, native workers and generated hosts served both
new screens; no build/sync/profile ran during scoring. Exact alpha, VP8L hidden
RGB, source stability and independent output lifetimes are checked before/after.
Quality metrics are untimed sanity checks of identical packets, not encoder
selection or acceptance tolerances.

The [final residue qualification](cleanup-final-residue-evidence.json) records
4,903 Windows/Linux checks each, 224 affected scalar checks each and the same actual
private Release package/all three examples on both systems. Unaffected scalar
proofs are reused. Packet/pixel/memory contracts are unchanged. No qualification
repeat was needed for the subsequent timing runs.

[Evidence](warm-post-cleanup-webp-evidence.json) records all four screens, producers,
guards, samples, both raw archives and precise reuse. Next use bounded VP8L
diagnostics before choosing a substantial primitive. Other named producer
uncertainties and full warm parity remain open; no general content claim is made.
