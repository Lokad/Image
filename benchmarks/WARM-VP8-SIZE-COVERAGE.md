# Lossy WebP complete-packet coverage

All **seven declared controls** pass complete-packet size and the unchanged
reconstruction-quality gates. This establishes the frozen content/settings below,
without a broader lossy WebP or CPU-parity claim.

| Existing control | Managed / reference packet B | Managed / native quality | Source-PSNR gap dB | Gate |
| --- | ---: | ---: | ---: | --- |
| WarmCompression/PhotographLossy8 | 17,366 / 17,688 | 77 / 75 | 0.140944 | Pass |
| WarmCompression/DiagramLossy8 | 6,722 / 7,100 | 75 / 75 | 0.117409 | Pass |
| WarmWebp/EncodeOpaque8 | 70,298 / 72,380 | 100 / 75 | 0.005052 | Pass |
| WarmWebp/EncodeOpaque16 | 70,298 / 72,380 | 100 / 75 | 0.005052 | Pass |
| WarmWebp/Encode8 | 96,016 / 98,590 | 100 / 75 | 0.021958 | Pass |
| WarmWebp/Encode16 | 96,016 / 98,590 | 100 / 75 | 0.021958 | Pass |
| WarmWebp/EncodeBudget8 | 98,490 / 98,590 | 100 / 75 | 0.025713 | Pass |

The [histogram384 milestone](WARM-VP8-HISTOGRAM384.md) closes the final gate,
saving958 B with exact prior signed coefficients, native/public RGBA, source
quality and compressed ALPH. The larger owner is admitted before allocation;
31,802,951 actual units fit32M. Both OSes pass4,759 tests, scalar checks and the
same actual private0.2.0 package/all three examples. One current short cost is
recorded; six historical C83 costs retain their producer identities without new
current-code confirmations. Full CPU parity and replacement readiness remain open.

Inputs, native75/effort4 references, packet hashes, exact alpha and native/public
decoder bounds remain frozen. Source-PSNR absolute gap must stay≤0.25dB. Numerical
quality equality is not matching. C57 selected photograph77 by the existing bounded
closest-quality procedure before assessing size; the previous78 point remains
recorded. Diagram75 stays valid. Procedural100 is a fixed tested setting, not the
library default or a general promise of equivalent quality across encoders.

Earlier milestones preserve their original evidence:
[compressed alpha](WARM-VP8-ALPHA-MATCHES.md),
[limited-work statistics](WARM-VP8-PREFIX-WORK.md),
[coefficient refinement](WARM-VP8-COEFFICIENT-REFINEMENT.md),
[prediction pruning](WARM-WEBP-PRUNED-PREFIX.md),
[DC/AC noise](WARM-VP8-NOISE-SPECTRUM.md),
[zero/one decisions](WARM-VP8-ZERO-ONE.md) and
[small-level decisions](WARM-VP8-SMALL-LEVELS.md).

[Structured ledger](vp8-size-coverage-evidence.json) identifies every selected
receipt and its scope. Lossless WebP, default JPEG and PNG complete-packet gaps
remain separate requirements; a component, timing pass or optional JPEG policy
does not close them.
