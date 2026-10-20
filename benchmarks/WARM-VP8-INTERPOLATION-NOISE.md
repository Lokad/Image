# Interpolation-aware WebP source quantization

Propagating quantization noise through centered chroma interpolation reduces the
same opaque packets **83,838 → 74,962 B** (10.6%), alpha packets **108,880 → 99,816 B**
(8.3%) and constrained packet **120,146 → 111,094 B** (7.5%). The five complete-packet
gates remain open. Primary photograph/diagram packets and quality remain unchanged.

| Existing control | Previous → current / reference B | Source-PSNR gap dB | Managed / reference µs | Managed allocated B/op |
| --- | ---: | ---: | ---: | ---: |
| EncodeOpaque8 | 83,838 → 74,962 / 72,380 | 0.016224 | 14,873.97 / 15,522.15 | 1,002,488 |
| EncodeOpaque16 | 83,838 → 74,962 / 72,380 | 0.016224 | 15,478.99 / 15,573.08 | 1,002,488 |
| Encode8 | 108,880 → 99,816 / 98,590 | 0.038933 | 21,829.63 / 53,523.02 | 1,532,032 |
| Encode16 | 108,880 → 99,816 / 98,590 | 0.038933 | 22,376.11 / 53,581.44 | 1,532,032 |
| EncodeBudget8 | 120,146 → 111,094 / 98,590 | 0.038933 | 14,856.36 / 54,164.87 | 1,806,692 |
| PhotographLossy8 | 17,366 → 17,366 / 17,688 | 0.140944 | 34,396.38 / 9,714.89 | 590,152 |
| DiagramLossy8 | 6,722 → 6,722 / 7,100 | 0.117409 | 13,942.60 / 5,832.30 | 570,856 |

Inputs, native references/settings and all source-quality, decoder and exact-alpha
gates stay fixed. Procedural settings are managed100/native75, effort4; primary
photo77/diagram75 remain separate passing controls. Numeric quality equality is
not a matching criterion. The constrained encode consumes31,916,945 units within
32M. Unrestricted ALPH18,579/ref24,588 B remains stopped; constrained ALPH28,333 B
still exceeds the reference component size.

Independent equal-variance coefficient noise propagates through the existing
centered bilinear chroma expansion and RGB matrix. Interior quarter phases give
100/256 chroma noise variance; coincident edge taps are combined before squaring.
The unchanged source-only MSE/(Quality+1) allowance and base quantizer mapping then
select four source-derived segments. Local equal neighboring steps and independent
noise are heuristic assumptions; empirical quality gates remain decisive.

No new buffer/frame is introduced. Existing map ownership, borrowed-root admission,
stack allowance and actual work are preserved, with32 additional logical units per
sample and the matching eligibility envelope. Refusal/exhaustion/cancellation stay
terminal. Eight independent impulse-energy tests cover phases, edges and degenerate
axes; existing shared admission, replacement peaks, cleanup and output tests pass.

Both OSes pass4,674 tests plus202internal+8public+2diff scalar checks. Normal/scalar
packets and managed pixels agree acrossOS. The same actual private0.2.0 package
and all three examples pass bothOS. One immutable core/internal build perOS; public,
differential, reference, harness, native and selector binaries are reused.

Seven guarded three-actual cost screens cover the seven existing affected encodes.
No independent timing repeat or CPU-parity claim; native allocations are unmeasured.
Unchanged decoder and other codec/edit/text/inspection evidence retains its scope.
The [structured evidence](vp8-interpolation-noise-evidence.json) retains immutable
producer/package proofs, the source-only model, full receipts and raw samples.
