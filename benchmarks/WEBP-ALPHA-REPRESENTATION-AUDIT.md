# ALPH representation and storage lifetimes

Source/lifetime audit at `27159ae008d8cfc1919515b9a9b35f644c526b08`.
No production change, diagnostic build, new timing or performance claim.
The existing transparent 513×259 control remains the scope.

| Storage | Actual users and lifetime |
| --- | --- |
| 4 bytes/sample working frame | One mutable frame across sequential alpha-filter trials. Prediction mutates it; matching, cache selection and parsing still read it. Prepared coding owns tokens/maps/codes, not pixels. After all filters it transfers to the VP8 coefficient owner when physical capacity fits. |
| Match index and head storage | Chain links and reverse match results share one 4-byte/sample index. That index is reused across eligible filters. Dense heads borrow the empty 8-byte/sample token seed until matching finishes. |
| Standard/repeat/trace tokens | Standard and repeat candidates overlap through cache selection and cost comparison. The rejected repeat capacity already backs tracing; its path and forward windows end before output tokens occupy that storage. Live selected candidates remain independent. |
| Cache history | Borrows an unused token tail when it fits. Live tokens and source samples must remain disjoint. Every history user ends before cache application or further token writes. |
| Histograms and coding | Sparse leaves and recycled dense clusters already avoid full dense tile populations. Counts end after canonical-code construction; maps/codes/tokens survive counting and emission. Main tokens can transfer only after their final emission user. |
| Encoded ALPH payload | Old/new payloads overlap until replacement emission succeeds. Selected bytes remain independent through subsequent filter trials and actual async output users. |

A whole-pipeline two-channel representation would need green **and alpha**:
mode0/origin prediction introduces alpha residuals even though normalized input
contains alpha only in green. Palette, subtract-green and all-family trials can
change that representation further. Hashing and cache identities must remain
equivalent to the original packed samples; narrowing only matching already has
[inconclusive results](WEBP-COMPACT-MATCHING-INCONCLUSIVE.md).

For 132,867 samples, 4→2-byte working storage saves **265,734 bytes** in that one
array. It would no longer cover the existing **448,800-byte** VP8 coefficient
buffer. Reallocating coefficients would add **183,066 bytes net payload allocation**
across these two phases, before extra owners/conversion. Keeping the larger frame
preserves the transfer but removes this storage saving. These are source-derived
capacity calculations, not measured whole-operation allocations or physical peaks.
Token/index/histogram capacity does not shrink merely because pixels narrow.

The inspected libwebp checkout `4fa21912338357f89e4fd51cf2368325b59e9bd9`
also uses 32-bit working samples and 8-byte literal/copy records. Its encoder
retains the hash chain/reference lists and recycles variable-size token blocks.
That suggests studying token capacity rather than assuming narrower pixels are
its advantage. Only strategy was inspected; no source or table was copied.

**Decision:** do not implement the whole-pipeline narrowing hypothesis from this
audit. It has no demonstrated CPU advantage and conflicts with a useful existing
handoff. This does not close compact representations or block-based tokens as a
family; a revised end state must account for all overlapping users and the lost
transfer. Avoid replaying the local matching conversion.

Relevant implementation boundaries are `Vp8AlphaEncoding`,
`Vp8lPreparedEncoding`, `Vp8lReferenceMainParsing`, `Vp8lTokenBuffer`,
`Vp8lReferenceCache.History`, `Vp8lPopulationWorkspace` and `Vp8FramePlan`.
[Transfer qualification](WEBP-ALPHA-COEFFICIENT-STORAGE.md),
[parsing lifetimes](WEBP-PARSING-LIFETIMES-AUDIT.md) and
[cache-tail qualification](WEBP-TOKEN-TAIL-HISTORY.md) retain measured evidence
and reproducible raw archives. Current packet, performance and same-package
qualification remain unchanged.
