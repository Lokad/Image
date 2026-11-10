# Batched two-row JPEG columns

Complete sparse-CMYK decode saves **26.08 us / 5.38% raw /
4.28% reference-normalized**, 484.49->458.40 us.
Filtered/all-actual raw and normalized confidence intervals separate. Native
movement is -1.16%. Exact pixels and allocations remain unchanged.

The standalone sparse upper 1.075 and RGB444 upper 1.012
remain uncertain. RGB's raw point moves with its native clock; no causal companion
gain, regression or equivalence is asserted. All 25 affected decode costs refresh,
including progressive/live/stream/caller boundaries. Four unchanged DC-only or
syntax-validation costs are reused. Current 29 reads: **22 passes / 2 gaps / 5 uncertain**.
Horizontal CMYK and caller CMYK remain gaps; passing paths stop.

## Preparation, execution and storage

One column publisher processes the existing two-low-row case with four signed
word pair-multiply batches and one 64-byte output store. Format-derived weights
retain the original cosine kernel and explicit reflected odd signs. Word bias
precedes widening; modular sums, shift 20, clipping and row order stay exact.
Original row transforms, dequantization, global DC-only shortcut, AVX2/scalar and
full-row column arithmetic remain. No packet, metadata or numerical policy changes.

A first inline version screens 505.09/native479.38 us for sparse, with an adverse
RGB point 1620.75/native1572.54 us versus contemporary 1587.83/native1582.45 us.
These short intervals are broad, proving no gain or regression. Deferring unused
widened-row preparation screens 501.16/native480.54 us but enlarges the inverse
frame 232->344 B and adds a 64-byte spill/reload. The final physical separation
keeps the exact publisher in a 210 B leaf with no stack adjustment; caller code is
1,665 B versus original 1,772 B, with 280 B versus 232 B frame adjustment. Complete
measurement includes the per-selected-block call and its ABI costs. This is a
scoped lifetime boundary, without a general inlining policy. Every adverse sample
and each of the three immutable implementations remains in the raw evidence.

No per-image owned capacity, pool, root, session, owner or public API is added.
Total allocation remains 20,944 B/op for sparse and 42,936 for RGB; independent
RGBA output retains 531,468 B until disposal and operation scratch retention is 0.
New fixed format weights occupy 256 B; observed helper JIT readonly data occupies
384 B including aligned weight copies, bias and permutation. Those differ from
per-image GC and admitted capacity. Physical process/native/JIT-stack peaks are
unmeasured. Existing output-pool retention and governed bypass remain separate.

One actual publisher test checks 6,144 row pairs/393,216 samples against a scalar
cosine oracle, including signed-word extremes/bias wrap, reflection, byte clipping,
source/output guards and zero allocation. Existing complete inverse basis, wide
quantizer, natural/scan-order, cancellation, refusal, recovery and surviving-owner
checks remain. An earlier lane model supplies no actual-SIMD or timing proof.
No benchmark image/settings grid is added.

## Current complete controls

| Complete frozen control | Lokad us | FFmpeg us | Ratio / upper | Allocated B/op | Status |
| --- | ---: | ---: | ---: | ---: | --- |
| DirectRgb444 | 1,457.77 | 1,451.29 | 1.004 / 1.012 | 42,936 | uncertain |
| Decode8 | 1,759.71 | 2,025.78 | 0.869 / 0.885 | 4,320 | pass |
| NativeDecode8 | 1,552.13 | 1,718.82 | 0.903 / 0.923 | 42,936 | pass |
| NativeDecode420 | 909.77 | 1,089.49 | 0.835 / 0.841 | 258,446 | pass |
| NativeDecode422 | 1,202.50 | 1,385.08 | 0.868 / 0.913 | 330,260 | pass |
| OwnedDecodeGray | 655.57 | 719.80 | 0.911 / 0.959 | 140,449 | pass |
| ProgressiveGray | 1,739.70 | 1,828.98 | 0.951 / 0.967 | 151,432 | pass |
| Progressive420 | 1,644.13 | 2,046.17 | 0.804 / 0.879 | 243,664 | pass |
| Progressive444 | 3,186.55 | 3,357.17 | 0.949 / 0.964 | 169,664 | pass |
| Progressive422 | 2,176.60 | 2,432.83 | 0.895 / 0.903 | 315,496 | pass |
| NativeDecode420Restart | 1,050.36 | 1,095.80 | 0.959 / 0.974 | 262,818 | pass |
| StreamDecode420Restart | 955.16 | 989.97 | 0.965 / 0.976 | 262,920 | pass |
| NativeDecodeCancelable8 | 1,575.86 | 1,722.60 | 0.915 / 0.938 | 42,936 | pass |
| ProgressiveCancelable444 | 3,653.00 | 3,648.86 | 1.001 / 1.017 | 169,664 | uncertain |
| DirectRgb422 | 1,106.19 | 1,097.98 | 1.007 / 1.027 | 330,260 | uncertain |
| DirectRgb420 | 806.12 | 745.12 | 1.082 / 1.100 | 258,452 | uncertain |
| CmykAdobe (reused) | 324.37 | 379.40 | 0.855 / 0.861 | 4,536 | pass |
| CmykAdobeAc | 438.61 | 392.08 | 1.119 / 1.160 | 20,944 | gap |
| CmykAdobeGeneral | 1,489.91 | 3,174.15 | 0.469 / 0.492 | 20,944 | pass |
| CmykAdobeSparse2D | 458.40 | 437.54 | 1.048 / 1.075 | 20,944 | uncertain |
| Ycck420 | 426.77 | 709.08 | 0.602 / 0.606 | 895,563 | pass |
| CmykCaller (reused) | 365.86 | 353.37 | 1.035 / 1.047 | 1,085,975 | gap |
| Ycck420CallerRgb | 475.72 | 604.80 | 0.787 / 0.826 | 762,719 | pass |
| Decode411 | 382.53 | 451.44 | 0.847 / 0.865 | 219,551 | pass |
| Decode410 | 344.03 | 506.86 | 0.679 / 0.686 | 189,090 | pass |
| CallerGray | 654.13 | 695.49 | 0.941 / 0.955 | 273,439 | pass |
| CallerRgb | 980.30 | 1,156.27 | 0.848 / 0.860 | 657,359 | pass |
| ValidateJpegBaseline420 (reused) | 483.06 | 674.27 | 0.716 / 0.720 | 10,016 | pass |
| ValidateJpegProgressive420 (reused) | 288.75 | 297.28 | 0.971 / 0.984 | 4,088 | pass |

Historical points suggested horizontal/dense CMYK and grayscale regressions.
One contemporary original countercheck per control resolves the CMYK clock
contradictions without separated adverse intervals. Grayscale initially has
664.59/native647.32 us versus original 652.96/native691.49 us, with overlapping raw
but adversely separated normalized intervals. Exactly one targeted unchanged-build
recheck resolves that clock regime: 654.13/native695.49 us, upper .955 pass. Its
raw/normalized intervals overlap the original. Earlier actuals remain retained;
there is no further unchanged sampling for favorable bounds. Other refreshed
points establish current status, without causal companion claims.

## Qualification and reproducibility

Windows/Linux each pass **6,182 full/live checks**, 459 internal/375 public JPEG
checks per normal/scalar/disabled-AVX2/AVX512 mode, and the **same actual Release
package/all three usage examples**. Unchanged resize 350/729 ISA evidence is reused.
Three successful main/embedded versions per OS serve this coherent round, with
zero production compile failures or harness/native/selector/fixture builds. Broad
and actual-package qualification run once per OS. Existing frozen input, pixel
expectation and reference projections are reused and byte-restored after each job.

All elapsed costs are complete public operations: parsing/preparation, entropy,
inverse/color normalization, independent output acquisition and disposal. FFmpeg
retains its documented codec/conversion contexts. Frozen packets/settings/native
pixel bounds, ownership, single CPU, sequential Release and idle-v2 guards remain.
Necessary managed work stays timed. Syntax validation and global DC-only controls
do not enter the changed method; their source-valid costs remain identified above.

[Evidence](jpeg-batched-two-row-columns-evidence.json) pins comparisons, every
stage/countercheck, current cases, storage and qualification. The
[preceding attribution](JPEG-SPARSE-CURRENT-ATTRIBUTION.md) retains sampled leads;
the [token descriptors](JPEG-PREPARED-TOKEN-DESCRIPTORS.md) remain in production.
Mirrored/member-verified raw `jpeg-batched-two-row-columns.tar.xz`:
13,569,020 B /622 members, SHA256
`8ed600096f4167228c797702e7420236ab95475d3e1e1fd3daaa97174a438cc2`. Private 0.2.1 package:
444,206 B, SHA256
`47616c839442aa21a16200b03eea0fe094c1d8a48ed4e56fdcfb0202eced8819`. Published releases stay immutable;
publication is postponed. All 283 material paths remain required; this round does
not establish whole-library parity.
