# Rejected VP8 reader-state caching

Keeping arithmetic/cursor state in locals is flat on the same complete warm
VP8 decode screen: 13163.52 µs versus
13184.11 µs for the retained baseline,
an apparent 0.16% gain. Its ratio is 2.1729x
FFmpeg, conservative upper 3.2606x.
The candidate is discarded without a longer repeat; production Read is restored
to the exact accepted source. Three independent offset regression controls remain.

The [numeric evidence](warm-vp8-reader-state-rejected-evidence.json) retains samples,
uncertainty, source/output/tool identities and immutable archive hashes. One
optimized-JIT inspection of the saved actual baseline DLL shows repeated field
loads/stores during coefficient-token normalization. The diagnostic produces the
frozen exact pixels and matches the baseline package's production methods/flags.
Coefficients has880 code bytes and CoefficientToken5240. An initial quiet guard
rejects before launching a diagnostic child; the unique reattempt is the only
actual inspection, with passing pre/post quietness. This supplies code shape,
not CPU-cost attribution or parity timing.

The candidate caches range/value and bit cursor for the original bit-by-bit
normalization. It publishes exact state on success and before partition truncation
or span index failure, including the original range doubling before a failed byte
access. Work/cancellation and probability/interval validation keep their order.
Production input cannot alias the private reader state. Reader layout, token
decisions, field values, pixels and allocations are not intentionally changed.
All29 reader controls pass before editing on both OSes; all69 focused VP8 controls
pass with normal/disabled intrinsics. They compare result, exception type/message,
all five state fields and remaining work. New offsets1/3/11 cover absolute cursors,
shorter spans and repeated failures through exhaustion of the work budget.

This uses one existing original513x259 raster and frozen native quality75 VP8/ALPH
input, unchanged native worker and complete decode/output acquisition/release
boundary. Source integrity, exact prior managed pixels/alpha, RGBmax1/RMSE0.132879,
native CLI agreement and independent output owners remain. CPU0, one codec/
conversion thread, normal SIMD, disabled tiering and non-concurrent workstation
GC remain. No image or encoder search, CPU profile or automatic screen repetition
occurs. Statistical prequiet passes/postquiet fails unchanged thresholds; no
performance or package acceptance is claimed for this discarded candidate.

After restoration, all 2,100 Release tests pass on both OSes: 520 internal,
686 public API and 894 live FFmpeg comparisons. Fresh production methods and
implementation flags match the retained actual integer-chroma package, and its
30 existing original JPEG outputs remain exact. No new production package is
needed for the retained test and evidence changes.
