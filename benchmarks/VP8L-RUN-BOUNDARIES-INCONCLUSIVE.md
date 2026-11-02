# Lossless WebP run-boundary trial

The current ordinary RGBA8 encoder's profile attributes 359/783 exclusive samples
to paired histogram costs. A materially different implementation from the earlier
window-statistics trial computes exact adjacent-sum run boundaries in SIMD blocks,
then retains the same run-order, integer entropy/refinement, tie and packet logic.
One 64-bit boundary mask replaces scalar per-bin branching; no heap storage,
cache, admission or ownership change. Portable scalar execution remains.

| Existing complete RGBA8 encode | Lokad µs | libwebp exact=1 µs | Ratio / upper | Allocated B/op |
| --- | ---: | ---: | ---: | ---: |
| Current independent baseline, twelve actuals | 137,365.44 | 122,207.95 | 1.124 / 1.139 | 26,131,848 |
| Candidate short, three actuals | 160,790.21 | 131,027.63 | 1.227 / 1.535 | 26,131,848 |

The screen is unpromising: raw point cost rises 17.1%, with a 7.2% native clock
shift and wide confidence. This proves neither a gain nor a causal slowdown.
Restore the implementation without independent confirmation or favorable
resampling. The helper, overlapping loads and bit scanning do not justify
maintained complexity on this evidence. A different future design requires
concrete reasoning; the histogram-cost family is not permanently closed.

All original source/settings, complete-operation, packet/pixel, alpha/hidden RGB,
normal/scalar native, output ownership and quiet-guard gates pass. The complete
packet is exactly 183,812 bytes. Windows/Linux each pass 32 focused checks in
normal/scalar modes for candidate and restoration. They include 10,080 frozen
paired costs, range extremes, cancellation, zero allocation and 4,650 new boundary
comparisons. The new behavioral test remains against the actual restored code;
the trial implementation is removed.

Qualified production DLLs and the actual 0.2.1 package remain unchanged.
Existing full/live/package proof is reused; no broad repeat or package rebuild.
[Evidence](vp8l-run-boundaries-inconclusive-evidence.json) retains the hypothesis,
adverse screen, source/producers and restoration. Raw
vp8l-run-boundaries-inconclusive.tar.xz: 6,285,424 B / 150 members, SHA256
fe578122eaa081337b1b4b08b6c6ffd4cfa13caabd46a167ddb9c5a06c15f9bf,
mirrored/member-verified. Baseline qualification depends on the retained compact
Deflate archive. Current elapsed-cost tables remain unchanged.
