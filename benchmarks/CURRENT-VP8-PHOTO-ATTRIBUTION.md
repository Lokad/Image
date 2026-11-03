# Current VP8 photograph attribution

One two-second capture on the qualified RSP186 DLL uses the existing complete
513×259 opaque photograph/Q75/effort4 control. The 17,688-byte packet, frozen
quality, normalization, source/output ownership and quiet guards pass. No builds
or repeated suite/package qualification. CPU0 executes; CPU1 collects. Profiled
elapsed is discarded: samples and instruction skid are not component timings.

| Current sampled location | Samples, out of 1,961 |
| --- | ---: |
| Luma4 decisions, including inlined scoring | 357 |
| Quantization helpers | 242 |
| Reconstruction AddVector | 165 |
| Coefficient emission | 154 |
| Luma16 decisions | 108 |
| Token statistics | 85 |

Luma4's 6,685-byte body has 111 samples in IG91/95, covering residual-magnitude
preparation/validation and scalar table summation. The existing quantizer already
forms zigzag levels before writing them; scoring reloads them for flatness,
magnitudes, nonzero/last position and rates. These are concrete repeated-work
leads, not a claimed saving. Coefficient emission spans a 3,931-byte body; its
costs are distributed, with no isolated gain established.

Current code already shares source texture, batches forward predictions, reuses
uniform predictions and applies rate/SSD bounds. [Paired quantization](VP8-QUANTIZATION-PAIR-CONFIRMATION.md)
has no useful independent gain. [Packed quantization](VP8-PACKED-QUANTIZATION-REJECTED.md),
the earlier [residual-summary screen](VP8-RESIDUAL-SUMMARY-DEFERRED.md) and
[combined prices](WARM-VP8-COMBINED-PRICES-SCREEN.md) retain their specific adverse
or inconclusive evidence. Their old thresholds and family-closure wording do not
ban a materially different architectural design; they do not justify replaying
unchanged candidates.

The next bounded hypothesis shares quantization byproducts with candidate scoring,
preserving level caps, exact costs, ordered decisions/ties, trellis and portable
execution. Use existing complete photograph/alpha controls; verify gains before
acceptance. Existing costs remain unchanged. [Earlier attribution](https://github.com/lokad/image/blob/66340be3e4d3e14163ce9efeb58b8574207331b9/benchmarks/CURRENT-VP8-PHOTO-ATTRIBUTION.md)
and its raw archive remain history.

[Evidence](current-vp8-photo-attribution-evidence.json) pins current sources,
instructions, actual producers and quality/source gates. Raw
`vp8-current-decision-pipeline-attribution.tar.xz`: 1,081,224 B / 35 members,
SHA256 `a4dfe14c4aef46feb8b38a9613e264841df213dcb758e50b3160aa1d1c3876ca`,
mirrored/member-verified, with the pinned qualified-baseline dependency.
