# Deferred VP8 residual summary

The existing effort2 diagram has a fresh qualified baseline of **3,993.94 µs,
1.583× reference time**, versus 3,831.38 µs / 1.522× for a shared SIMD residual
summary. That is only a 4.07% short-screen difference, with overlapping confidence
intervals. The prototype is deferred and removed; larger remaining gaps take priority.

Both three-actual jobs use the same 513×259 input, Q75/effort2 reference, cached
worker/selector and complete independently owned output boundary. All pixel,
source/ownership, 7,330-byte packet and idle-v2 gates pass. Both allocate 741,401
B/op. Their conservative uppers are 1.709× baseline and 1.620× candidate.

The candidate replaced repeated last-nonzero scans in coefficient emission and
statistics with one AVX2/scalar primitive. Normal/scalar Windows comparisons cover
2,097,152 signed one-hot cases, 280 range boundaries and 24,576 exact event/state
comparisons; normal Linux checks also pass. Instrumented profile times are discarded.

The immutable qualified source/DLL is restored on both systems. No candidate
confirmation, full-suite repeat, package regeneration or new workload was needed.
Previous qualification remains valid; no prototype helper survives in production.
The source, binaries, proofs and raw samples are retained in the mirrored,
member-verified archive identified by [the evidence](vp8-residual-summary-deferred-evidence.json).

Reuse the fresh 1.583× baseline, rather than treating the release's 1.685× short as
the current largest gap. Follow [the current queue](WARM-COVERAGE.md) for priority.
Do not renew this mask family without a materially different opportunity.
