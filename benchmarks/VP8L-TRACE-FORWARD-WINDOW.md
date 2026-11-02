# Bounded VP8L trace working state

Forward cost and pending-match storage now use at most 8,192 entries. Only the
backtracking path remains proportional to the image. Complete-encode allocations
fall by **2.49 MB** for the photograph/diagram and **4.99 MB** for transparent
lossy encoding with lossless alpha. Exact packets and managed pixels are unchanged.

| Complete encode | Allocated B, before → after¹ | Admitted peak B, before → after | Current Lokad / reference µs | Ratio / upper |
| --- | ---: | ---: | ---: | ---: |
| Photograph / libwebp exact=1 | 19,328,008 → 16,834,504 | 12,217,052 → 12,217,052 | 103,705.27 / 86,982.67 | 1.192 / 1.231 |
| Text/diagram / libwebp exact=1 | 10,481,072 → 7,987,568 | 6,174,402 → 5,055,600 | 10,392.20 / 10,057.04 | 1.033 / 1.075 |
| Transparent lossy / FFmpeg | 19,224,056 → 14,237,048 | 6,579,383 → 4,085,883 | 68,339.72 / 54,643.25 | 1.251 / 1.301 |

¹ Direct complete-operation allocation samples isolate the storage change.
Independent BenchmarkDotNet costs allocate 16,835,524 / 7,988,537 / 14,238,680 B/op,
including harness accounting. Peaks are shared admission charges, excluding
borrowed inputs/caller stream capacity, not physical heap peaks. No trace workspace
is retained between encodes. Another phase determines the unchanged photograph peak.

Three independent current measurements retain twelve actuals and twelve native
samples before/after, using the existing quiet single-CPU boundaries, workers,
inputs and gates. The valid [preceding measurements](VP8L-SHARED-PREDICTION.md)
are reused for before/after interpretation. **No incremental CPU gain is claimed.**
The diagram is 3.59% / 360 µs slower in both raw confidence views; alpha is 9.55%
faster in raw elapsed. Reference drift of −2.48% / −6.59% respectively leaves
normalized comparisons inconclusive. Photograph confidence also overlaps.
All adverse results remain; there was no favorable resampling. Photograph/alpha
CPU gaps and diagram parity uncertainty remain open. The accepted benefit is
less allocation and lower diagram/alpha live capacity.

At forward iteration `a`, tracing touches `[a−1, a+8188]`. Copy lengths are at
most 4095; lookahead extension runs only when `reach < a+length−1`, so its next
position is at most `a+4094`, followed by another bounded copy. Expiring `a−2`
before the next literal is safe: its future alias `a+8190` has not been touched.
The full `ushort` path retains future-resolution writes and survives the sweep.
Forward arrays shrink from `20N` to `20·min(N,8192)` bytes; path storage stays `2N`.
Prices, absolute source positions, ties, search order and candidate ownership stay
unchanged. There is no pool, extra owner or cross-operation retention.

Five original periodic/literal controls freeze actual pre-change token, cache,
cost and seed hashes at ring boundaries and repeated wraps. They reconstruct
every pixel, preserve borrowed input and keep outputs valid after seed/index
disposal. Focused shared refusal/cancellation tests verify terminal cleanup and
successful subsequent tracing. Existing native token/candidate controls remain.
Four complete packets, including original 127×65 bars, retain exactly
160,032 / 7,234 / 98,590 / 660 bytes. The small bars screen is slower and unchanged
in allocation; that diagnostic result is retained too.

Windows/Linux each pass 5,411 full/live checks and 234 affected checks in normal
and scalar modes. The same actual Release package and three usage examples pass
both platforms. Published packages remain immutable; publication is postponed.

[Evidence](vp8l-trace-forward-window-evidence.json) retains all costs, source and
producer pins, original oracle capture, lifetime proof, guards and package checks.
Raw vp8l-trace-forward-window.tar.xz: 13,238,792 B / 407 members, SHA256
43fc1fba702cfad183f961d149de19a1db207ea172b528000aa395284b478211,
mirrored/member-verified, including original/selected sources, immutable runtimes
and old/new package artifacts. No measurements or qualification jobs were repeated.
Other affected encoder costs remain historical; unchanged paths reuse valid proof.
