# Rejected VP8 scoped-work trial

Caching coefficient-block work charges is flat on the retained single-CPU warm
VP8 decode screen: 13894.27 µs versus
13921.12 µs for the accepted baseline
(0.19% apparent gain). The candidate ratio is 2.2916x
FFmpeg, conservative upper 3.2819x.
The trial is discarded without a longer repeat. Production and candidate-only
tests are restored to the accepted fixed-token checkpoint.

The [numeric evidence](warm-vp8-local-work-rejected-evidence.json) retains samples,
uncertainty, identities, limits, quietness and immutable source/raw archive hashes.
The actual timed DLL is retained and matches the freshly built candidate's
production methods/flags. This trial uses the same frozen original 513x259 raster,
native quality-75 VP8/ALPH input and unchanged native worker. No new image, quality
search, CPU profile or JIT diagnostic occurs.

The candidate caches noncancellable entropy charges in a stack-local budget for
one coefficient block, publishes successful charges in finally on every exit,
and retains per-bit parent checks for cancellable tokens. Reader layout and
partition arrays remain unchanged. Four new scalar controls pass before editing
on both OSes. All 62 focused VP8 controls pass with normal and disabled intrinsics
on both OSes, comparing arithmetic state, work and failure order. Complete public
decode has the same minimum work budget, 23,122,008 units, through old/new libraries
with cancellable and noncancellable tokens. Below that boundary both reject; at
and above it both produce the frozen exact pixels. These are correctness checks
on one input, not additional benchmark images or timing evidence.

The original pixel hashes, exact alpha, RGB max1/RMSE0.132879, input integrity,
independent ownership and native CLI agreement remain. Both subjects perform
complete resident-input decode and output acquisition/release with the declared
conversion/reset/allocation boundary, CPU0, one codec/conversion thread, normal
SIMD, disabled tiering and non-concurrent workstation GC. Statistical prequiet
passes and postquiet fails under unchanged thresholds. No full-suite/package
acceptance or formal passing quiet confirmation is claimed for this rejected trial.
