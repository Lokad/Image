# Shared JPEG block support

JBS787 prepares one exact natural-order nonzero mask for each dequantized block,
then reuses it for DC-only, two-low-row and row-AC decisions. This removes repeated
zero searches without changing transform arithmetic, rounding, clipping or publication.
No heap storage, cache, retained capacity or public API is added.

| Complete decode, frozen predecessor → candidate | Before µs | Current µs | Saved µs | Raw / normalized reduction | Managed B/op |
| --- | ---: | ---: | ---: | ---: | ---: |
| Horizontal-AC Adobe CMYK | 454.79 | 400.50 | 54.29 | 11.94% / 11.10% | 20,944 |
| Dense RGB444 | 1563.69 | 1132.93 | 430.76 | 27.55% / 27.45% | 42,936 |

Both gains separate filtered/all-actual raw and normalized confidence intervals.
Three-actual screens precede independent twelve-actual predecessor/candidate
confirmations. These are scoped incremental gains; current standalone reference
status is reported separately below. No gain is inferred from GC counts.

## Current complete-operation evidence

All 27 existing pixel-decode controls refresh on the same immutable candidate;
two coding-only validations reuse unchanged evidence. No image/settings grid is
added. Quiet CPU 0 Release operations preserve source/reference identities, exact
managed pixels, case-specific native bounds, scalar CLI checks and independent
owned outputs. Each current confirmation has 12 actuals and 12 native samples per bracket.

| Existing control | Lokad.Image µs | Reference µs | Ratio / upper | Managed B/op | Status |
| --- | ---: | ---: | ---: | ---: | --- |
| DirectRgb444 | 1,132.93 | 1,454.06 | 0.779 / 0.794 | 42,936 | Pass |
| Decode8 | 1,417.11 | 1,898.27 | 0.747 / 0.749 | 4,320 | Pass |
| NativeDecode8 | 1,210.31 | 1,722.76 | 0.703 / 0.719 | 42,936 | Pass |
| NativeDecode420 | 672.11 | 1,008.04 | 0.667 / 0.673 | 258,454 | Pass |
| NativeDecode422 | 871.88 | 1,346.72 | 0.647 / 0.653 | 330,262 | Pass |
| OwnedDecodeGray | 574.77 | 682.76 | 0.842 / 0.849 | 140,454 | Pass |
| ProgressiveGray | 1,594.34 | 1,713.53 | 0.930 / 0.933 | 151,432 | Pass |
| Progressive420 | 1,415.48 | 1,952.93 | 0.725 / 0.744 | 243,676 | Pass |
| Progressive444 | 2,852.33 | 3,357.36 | 0.850 / 0.857 | 169,664 | Pass |
| Progressive422 | 1,787.37 | 2,246.71 | 0.796 / 0.800 | 315,496 | Pass |
| NativeDecode420Restart | 822.16 | 1,014.23 | 0.811 / 0.827 | 262,826 | Pass |
| StreamDecode420Restart | 807.53 | 992.24 | 0.814 / 0.822 | 262,927 | Pass |
| NativeDecodeCancelable8 | 1,236.76 | 1,718.91 | 0.719 / 0.728 | 42,936 | Pass |
| ProgressiveCancelable444 | 3,032.89 | 3,360.37 | 0.903 / 0.909 | 169,664 | Pass |
| DirectRgb422 | 878.77 | 1,093.80 | 0.803 / 0.813 | 330,262 | Pass |
| DirectRgb420 | 586.14 | 742.78 | 0.789 / 0.799 | 258,458 | Pass |
| CmykAdobe | 340.77 | 376.69 | 0.905 / 0.918 | 4,536 | Pass |
| CmykAdobeAc | 400.50 | 402.37 | 0.995 / 1.006 | 20,944 | Uncertain |
| CmykAdobeGeneral | 1,285.49 | 3,110.89 | 0.413 / 0.418 | 20,944 | Pass |
| CmykAdobeSparse2D | 471.28 | 432.85 | 1.089 / 1.104 | 20,944 | Gap |
| Ycck420 | 427.02 | 708.87 | 0.602 / 0.608 | 895,563 | Pass |
| CmykCaller | 337.83 | 330.43 | 1.022 / 1.028 | 1,085,977 | Gap |
| Ycck420CallerRgb | 432.76 | 605.56 | 0.715 / 0.724 | 762,721 | Pass |
| Decode411 | 388.83 | 451.06 | 0.862 / 0.875 | 219,552 | Pass |
| Decode410 | 338.61 | 507.84 | 0.667 / 0.679 | 189,090 | Pass |
| CallerGray | 564.34 | 648.76 | 0.870 / 0.877 | 273,443 | Pass |
| CallerRgb | 789.67 | 1,156.27 | 0.683 / 0.688 | 657,367 | Pass |
| ValidateJpegBaseline420 (unchanged coding-only) | 483.06 | 674.27 | 0.716 / 0.720 | 10,016 | Pass |
| ValidateJpegProgressive420 (unchanged coding-only) | 288.75 | 297.28 | 0.971 / 0.984 | 4,088 | Pass |

Across 29 reads: **26 pass / 2 gaps / 1 uncertain**. The separate equivalent fresh-context
horizontal-CMYK view is 434.28/FFmpeg 752.29µs, upper0.588 pass,
20,944B/op. Its original retained-reference view remains uncertain. Fresh mode
times decoder/frame/packing create/dispose with the same borrowed padded packet;
the retained worker keeps those contexts outside timing. Both include complete
decode, device-color conversion, normalization and output ownership/release.
The fresh view is not evidence of an additional production speedup.

Sparse CMYK is now a demonstrated gap. A targeted frozen-predecessor confirmation
also has a gap (468.44/439.02µs). Current 471.28µs is
0.61% adverse in filtered means; filtered intervals separate adversely, all-actual
intervals overlap. Preserve this cost, not a blanket no-regression claim. No
prior passing control becomes a demonstrated gap. Horizontal CMYK uncertainty
is not an instruction to retune or repeat unchanged sampling for a favorable bound.

## Numerical, storage and qualification scope

The mask is derived from actual signed words after dequantization: modular AC
products can become zero and DC products saturate. Raw coefficient support would
be incorrect. AVX512BW uses direct word masks; AVX2 uses BMI2 extraction or portable
bit compaction. The original scalar numerical/query path and encoder Ac routine
remain verbatim. Source reverse proof restores the entire inverse by reverting
only support queries and the mask argument; every arithmetic/store/checkpoint
statement stays unchanged. Tests cover 64 one-hot positions, signed extremes,
4,000 unaligned random blocks, unchanged inputs and no heap allocation; existing
IDCT/decoder tests cover word wrap, saturation, ties and cancellation.

A single 2 s profile of the qualified predecessor has 1,998 sampled locations:
InverseAvx2 516; CoreLib 275, of which 136/117 have inverse/Avx2-inverse callers.
JIT output shows repeated short-word zero-search calls. Locations include
inlining/skid and do not establish phase costs or removable time percentages.
The same raw trace corrected a DSO-name parser error; no new sampling was used.

Both Windows and Linux pass **6,246 full/live checks** (2,937 internal, 2,361 public,
948 differential). JPEG-focused checks pass 464 internal/341 public per normal/scalar
mode; Linux additionally checks disabled AVX512 and disabled AVX512+BMI2, and
Windows checks 464 internal with those two disabled. The exact filter is Jpeg;
older wider counts included ownership tests. Unchanged text/resize ISA evidence
is reused. The SAME actual Release package and all three usage examples pass both OSes.
Windows/timed DLL `84e5de5b5af920627f241dc321e7e73c8b03cfc1adfeebcf3ffc9bd1f19b7069`;
Linux DLL `32ae5c0e7ee7ed8b4bde62b5a6cb2ea44a49dd13b81e9d0cce57b3082ddec1c3`;
private 0.2.1 package `1bf157eae4f9ac07818a612af420d1dc38865e676ec7ad8beea2586f6abc5225`. Published packages remain immutable.

Measured total allocations stay separate from live capacity and retention. There
is no added owned image/scratch capacity or retained operation state. Existing
default pools are unchanged and not remeasured; physical/native/JIT-stack peaks
are unmeasured. No admission, cancellation, input/output ownership or async
lifetime boundary changes. Refusal remains terminal.

One production and embedded-test build per OS are reused throughout; existing
public/harness/native builds are rebound, not rebuilt. Three startup-only stale
expectation failures had zero timed actuals. The isolated root restores qualified
RGBA/CMYK/RGB hashes and projected samples from archived evidence, without changing
encoded packets, reference settings or original artifact files. Completed captures
are preserved and not repeated. All affected performance reconciliation preceded
broad/package qualification. Further mask/dequantization fusion is not part of
this accepted stage.

[Evidence](jpeg-shared-block-support-evidence.json) pins source/module/input
identities, all actuals, guards, adverse reconciliation and actual-package proof.
Mirrored/member-verified raw `jpeg-shared-block-support.tar.xz`: 9,846,520 B/493 members,
SHA256 `79706f7519aa0218907cec5e6b2c87dcafb8d6d8c22c164a7d7cb7ee08d66367`. Pinned dependencies retain unchanged fixtures/harnesses
and [context boundaries](JPEG-CMYK-CONTEXT-BOUNDARIES.md), sampled attribution
and [prior column preparation](JPEG-BATCHED-TWO-ROW-COLUMNS.md). Those pixel CPU
costs are superseded only on paths refreshed here. Other codecs/encoders, editing
and text evidence remains source-valid. Publication stays postponed.
