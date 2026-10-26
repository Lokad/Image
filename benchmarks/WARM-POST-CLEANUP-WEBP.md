# Current WebP costs after cleanup

Two existing 513×259 RGBA8 anchors were screened on the unchanged qualified
cleanup library. Complete operations include normalization/repacking, encoding,
fresh independent output acquisition and release, on one CPU. Reference settings
remain Q75/method4; VP8L uses the approved libwebp exact=1 backend. No input,
effort or quality sweep was added. All packet bytes match their frozen references.

| Three-actual screen | Lokad µs | Reference µs | Ratio / upper | Allocated B/op | Packet bytes, both |
| --- | ---: | ---: | ---: | ---: | ---: |
| VP8 | 94,853.24 | 53,467.74 | 1.774 / 1.911 | 24,424,944 | 98,590 |
| VP8L | 223,136.69 | 121,152.46 | 1.842 / 2.624 | 48,232,248 | 183,812 |

Both versioned pre/post quiet guards pass. All actuals, filtered results, native
brackets and raw GC counts are retained. The broad VP8L upper bound is a short
screen's uncertainty, not a demonstrated >2× regression. Neither case passes the
≤1× target; no independent repeat or cleanup speedup is claimed.

The same immutable qualified core served both screens. Generated child hosts and
native workers were reused; harness preparation/boundary metadata was refreshed
once, before scoring. Two stale fuel phrases were removed from tooling. No
production or test input changed, so the cleanup's Windows/Linux, scalar and actual
package proofs remain valid without repeating them. Packet identity, exact alpha,
VP8L hidden RGB, source stability and independent output lifetimes are checked
before and after each run. Quality metrics are untimed sanity checks of identical
packets, not encoder selection or acceptance tolerances.

VP8L has more absolute excess elapsed time and allocation on these controls.
Next use bounded diagnostics to identify a substantial primitive; retain the other
named measurement uncertainties. This is no arbitrary-content performance claim.
[Evidence](warm-post-cleanup-webp-evidence.json) records producers, guards, samples,
raw archive identity and precise qualification reuse. Full warm parity remains open.
