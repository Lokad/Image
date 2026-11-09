# Paired JPEG tile packing

The scoped packing gain and raw evidence below remain valid. Current JPEG read
costs and whole-build qualification are owned by
[live prepared Huffman decoding](JPEG-LIVE-PREPARED-HUFFMAN.md); its change affects
23 baseline costs and reuses six unchanged progressive comparisons.

JPT711 shares bounded component views across two rows and keeps samples packed
through byte/word interleaves. Complete sparse-CMYK decode saves **30.48 µs /
5.46% raw / 5.50% reference-normalized**, 558.00→527.52 µs.
Filtered/all-actual raw and normalized confidence separate in the reversed
independent comparison; native movement is +0.036%. Exact pixels and allocations
are unchanged, including the frozen 22,708-byte input and 531,468-byte frame.
This is a scoped sparse-control gain; full JPEG parity remains open.

RGB generalization is inconclusive. A contemporary horizontal-AC CMYK original
records **478.97→503.62 µs**, a separated raw slowdown of 24.65 µs / 5.15%, while
native 394.25→426.76 µs moves +8.25%. Normalized bounds overlap despite a favorable point.
No causal regression, nonregression, equivalence or broader gain is established.
The completed candidate is reused for both comparisons, with no favorable repeat.

| Existing complete 513×259 decode | Lokad µs | FFmpeg µs | Ratio / upper | Allocated B/op | Status |
| --- | ---: | ---: | ---: | ---: | --- |
| Lokad-produced JPEG444 | 1,699.30 | 1,993.94 | 0.852 / 0.866 | 4,320 | pass |
| FFmpeg-produced JPEG444 | 1,688.53 | 1,812.93 | 0.931 / 0.948 | 42,936 | pass |
| FFmpeg-produced JPEG444, live token | 1,899.33 | 1,759.27 | 1.080 / 1.131 | 10,120 | gap |
| Direct RGB444 | 1,620.17 | 1,566.23 | 1.034 / 1.045 | 42,936 | gap |
| Adobe CMYK, DC | 315.68 | 401.55 | 0.786 / 0.795 | 4,536 | pass |
| Adobe CMYK, horizontal AC | 503.62 | 426.76 | 1.180 / 1.196 | 20,944 | gap |
| Adobe CMYK, dense | 1,421.69 | 3,288.48 | 0.432 / 0.445 | 20,944 | pass |
| Adobe CMYK, sparse two-dimensional | 527.52 | 474.91 | 1.111 / 1.119 | 20,944 | gap |

Eight equal-grid owned-result costs refresh on the actual-package DLL; 21 unchanged
read/caller/stream/validation paths reuse source-valid evidence. The 29-read scope
remains **18 pass / 7 gaps / 4 uncertain**. Passing cases stop. Companion historical point
differences are not causal claims. No new image, sampling or quality grid is added.

## Coherent transition

The motivating [capture](ARCHITECTURAL-CURRENT-ATTRIBUTION.md) locates 252
of 1,997 samples in generic MCU packing. Its complete-operation shares become
historical after this loop changes; samples never established phase times.

| Three-actual development stage | Managed µs | Native µs | Tile helper B |
| --- | ---: | ---: | ---: |
| Qualified original | 516.95 | 445.51 | — |
| Single-row initialized extension | 632.42 | 473.21 | 336 |
| Single-row unobserved upper lanes | 626.25 | 447.97 | 319 |
| Paired bounded vector rows | 534.77 | 477.27 | 373 |

The first two stages retain scalar-to-vector stack expansion and adverse points.
The final design loads initialized 16-byte row pairs directly; generated code
removes those vector stack round-trips and repeated row-span preparation. Parent
code is 1,242 B versus the original 1,231 B. Instruction/code-size changes alone
establish no gain. The final short's adverse raw/favorable normalized point amid
native movement is resolved by the independent comparison above, not exclusions.
All earlier implementations, samples and guards remain in raw evidence.

One shared tile borrower handles RGB and raw CMYK. Complete 64-byte IDCT tiles
include initialized padding; loads stay bounded and only visible rows are stored.
Original entry validation precedes writes, partial-width/portable paths remain,
and raw K stays data until complete EOI/late-Adobe interpretation. No early color
conversion/publication, input copy, encoder, numerical, cancellation or owner change.
The discarded implementations supply no production fallback or retained test hook.

Primary allocation remains 20,944 B/op; retained output is 531,468 B until disposal,
with no retained operation scratch. Existing arrays, coarse admissions, capacity
and 4 MiB-capped default pool are unchanged; governed calls bypass it. No physical,
native or stack peak is remeasured and no peak/allocation saving is claimed.

Windows/Linux each pass **6,096 full/live checks**, 448 internal / 375 public per
normal/scalar/AVX2-disabled/AVX512-disabled mode, and the SAME actual Release
package/all three examples. Three coherent main/embedded versions per OS serve
development; broad/package qualification occurs only on the final immutable stage.
Existing harnesses, workers, selectors, fixtures and useful MCU behavioral tests
are reused, with zero builds for those inputs. No extra correctness grid is added.

Complete public parsing/preparation, entropy, IDCT, normalization, independent
output and disposal remain timed on the quiet VM, sequential Release/one CPU.
FFmpeg retains its labeled contexts and has no equivalent live cancellation token.
Exact managed frames, original native bounds and strict encoder/metadata contracts
remain. The intended Windows DLL supplies timings; Linux-native code is qualified
separately. This is scoped development qualification, not a full release refresh.

[Evidence](jpeg-paired-tile-packing-evidence.json) pins current 29-read ownership,
all three stages, comparisons, source/JIT/storage proof and package qualification.
Mirrored/member-verified raw `jpeg-paired-tile-packing-qualified.tar.xz`:
14,840,272 B / 372 members, SHA256
`4dcb3fa1b539b1bc0bfa2866ca648eb8a8e1ee78cab623ba3ab37fcf2595ffd8`.
Private package 439,487 B, SHA256
`39b42c1def46a83e828e6775fe1ecb3a5583e9f62cffb95234594daf01ea36ed`.
Published packages stay immutable; publication is postponed and all 283 paths remain
required. [Narrow scan preparation](JPEG-NARROW-SCAN-PREPARATION.md) retains prior
progressive gains and valid unchanged-path costs. Next inspect the remaining
live-token baseline444 boundary, the largest refreshed absolute JPEG gap.
