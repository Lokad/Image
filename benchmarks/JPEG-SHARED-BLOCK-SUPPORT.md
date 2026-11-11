# Shared JPEG dequantization and block support

JDF799 publishes natural-order support directly from two SIMD dequantized product
vectors. Natural and scan order share one store/mask helper. DC replaces its bit
after the original full-width product, level shift and saturation; wrapped AC
words determine all other bits. This removes a separate 64-word reread/query on
AVX512. Portable conversion, inverse arithmetic, entropy grammar, Int32 coefficient
storage and encoder packets remain unchanged.

| Complete existing decode, predecessor → candidate | Before µs | Current µs | Saved µs | Raw / normalized reduction | Allocated B/op |
| --- | ---: | ---: | ---: | ---: | ---: |
| Sparse Adobe CMYK | 436.10 | 419.25 | 16.85 | 3.86% / 3.38% | 20,944 |
| Dense RGB444 | 1173.05 | 1127.70 | 45.35 | 3.87% / 3.65% | 42,936 |

Independent twelve-actual confirmations separate filtered/all-actual raw and
normalized confidence intervals on both controls. The mixed three-actual screens
remain: sparse 438.43→422.36 µs, dense 1144.42→1174.05 µs. Independent confirmation
resolves the early dense contradiction; no unchanged favorable repeat follows.
No causal gain is inferred for the other refreshed controls.

## Current complete-operation evidence

All 27 existing pixel-decode controls refresh on the same immutable candidate;
two coding-only validations reuse unchanged evidence. Exact managed/qualified
pixels, case-specific native bounds, normal/scalar CLI, source/independent ownership,
settings and quiet CPU 0 gates pass. All twelve actuals and native brackets remain.

| Existing complete control | Lokad µs | Reference µs | Ratio / upper | Allocated B/op | Status |
| --- | ---: | ---: | ---: | ---: | --- |
| DirectRgb444 | 1,127.70 | 1,451.87 | 0.777 / 0.784 | 42,936 | Pass |
| Decode8 | 1,425.25 | 1,899.64 | 0.750 / 0.757 | 4,320 | Pass |
| NativeDecode8 | 1,185.85 | 1,720.87 | 0.689 / 0.695 | 42,936 | Pass |
| NativeDecode420 | 668.74 | 1,013.98 | 0.660 / 0.670 | 258,454 | Pass |
| NativeDecode422 | 884.68 | 1,349.79 | 0.655 / 0.663 | 330,262 | Pass |
| OwnedDecodeGray | 574.20 | 681.95 | 0.842 / 0.847 | 140,454 | Pass |
| ProgressiveGray | 1,587.42 | 1,712.14 | 0.927 / 0.937 | 151,432 | Pass |
| Progressive420 | 1,443.79 | 1,950.04 | 0.740 / 0.745 | 243,676 | Pass |
| Progressive444 | 2,852.57 | 3,359.77 | 0.849 / 0.852 | 169,664 | Pass |
| Progressive422 | 1,788.29 | 2,241.06 | 0.798 / 0.804 | 315,496 | Pass |
| NativeDecode420Restart | 801.85 | 1,012.96 | 0.792 / 0.801 | 262,831 | Pass |
| StreamDecode420Restart | 802.02 | 991.79 | 0.809 / 0.820 | 262,927 | Pass |
| NativeDecodeCancelable8 | 1,227.18 | 1,721.54 | 0.713 / 0.727 | 42,936 | Pass |
| ProgressiveCancelable444 | 3,036.83 | 3,360.43 | 0.904 / 0.907 | 169,664 | Pass |
| DirectRgb422 | 868.21 | 1,095.79 | 0.792 / 0.805 | 330,262 | Pass |
| DirectRgb420 | 586.90 | 743.67 | 0.789 / 0.795 | 258,455 | Pass |
| CmykAdobe | 267.41 | 375.66 | 0.712 / 0.733 | 4,536 | Pass |
| CmykAdobeAc | 397.12 | 402.99 | 0.985 / 1.008 | 20,944 | Uncertain |
| CmykAdobeGeneral | 1,274.91 | 3,112.44 | 0.410 / 0.413 | 20,944 | Pass |
| CmykAdobeSparse2D | 419.25 | 433.52 | 0.967 / 0.978 | 20,944 | Pass |
| Ycck420 | 420.81 | 709.56 | 0.593 / 0.598 | 895,563 | Pass |
| CmykCaller | 318.93 | 330.45 | 0.965 / 0.972 | 1,085,981 | Pass |
| Ycck420CallerRgb | 429.43 | 605.39 | 0.709 / 0.715 | 762,717 | Pass |
| Decode411 | 382.03 | 448.23 | 0.852 / 0.863 | 219,551 | Pass |
| Decode410 | 337.85 | 507.32 | 0.666 / 0.671 | 189,090 | Pass |
| CallerGray | 565.15 | 648.12 | 0.872 / 0.875 | 273,443 | Pass |
| CallerRgb | 785.68 | 1,160.34 | 0.677 / 0.682 | 657,367 | Pass |
| ValidateJpegBaseline420 | 483.06 | 674.27 | 0.716 / 0.720 | 10,016 | Pass |
| ValidateJpegProgressive420 | 288.75 | 297.28 | 0.971 / 0.984 | 4,088 | Pass |

**28 pass / 0 gaps / 1 uncertain** across 29 reads. Sparse and caller CMYK now pass
and stop. Horizontal CMYK remains uncertain against the retained native context;
its equivalent fresh-context view refreshes to 398.40/FFmpeg 745.74 µs,
upper 0.537: pass, 20,944 B/op. No previous pass becomes a demonstrated gap.

Managed decode/admission, normalization, independently owned output and disposal
stay timed. The original FFmpeg worker retains decoder/frame/packing contexts;
the separate fresh view includes their creation/disposal with the same borrowed
padded packet. Both include full decode, color conversion and output ownership.
No one-shot/prepared boundary, input, quality setting or image grid changes.

The entire inverse source reconstructs after restoring only dequantization/support
producers and their call prelude. Numerical inverse arithmetic, bias, wrap/saturation,
packing, cancellation and decoder stores are verbatim. Existing exhaustive fullword/
quantizer/DC controls now also check the actual returned mask. Scan-order checks,
independent matrix hashes, guards, zero allocation/admission, cancellation and
recovery remain. Earlier rejected entropy/coefficient fusion changes a different
boundary; its adverse evidence remains retained, not replayed.

Existing input/row stack arrays total 256 B. Product vectors have 128 B of additional
logical lifetime; physical/JIT-stack/native peaks are unmeasured. Primary/generalization
allocation remains 20,944/42,936 B/op. Other few-byte aggregate-counter movements
are reported as measured, not savings. No new heap owner, cache, admitted capacity
or retained scratch; independent output lifetime and existing pools are unchanged.

Windows/Linux each pass **6,248 full/live checks** (2,939 internal, 2,361 public,
948 live). JPEG 464 internal/341 public pass per normal/scalar/AVX512off/AVX2off and
disabled-AVX512+BMI2 mode on both OSes. Unchanged resize/text ISA proof is reused.
One main/embedded build per OS serves the stage, with no harness/native/fixture
rebuild. Affected performance reconciliation precedes broad qualification. One
actual Release pack and the SAME package consumer/all three examples pass W/L.
The final identity check initially read older preparation recipes; actual qualified
prior receipts resolve it without timing or broad-check repeats.

Private 0.2.1 package `5a7efc04fa1c778f78b9dee00d743cb5839d904bc4301efda7db91239b20b9d3` (449,242 B),
Windows/timed DLL `b372f51ac5710cf70864799b1430e67c536c337d8bed29a7f8d13c4fb701ed52`,
Linux `a306871065bd8121c448ed47ef5fa5b087c2bee58e027122ea5a256b9cfe41b1`. Published releases remain immutable.

[Evidence](jpeg-shared-block-support-evidence.json) pins all observations and
reproduction. Mirrored/member-verified raw `jpeg-product-support-preparation.tar.xz`:
3,794,272 B / 339 members, SHA256 `f6f60f4c0b1c34220ab956ecddcef6a84c9049dad4d68694df732dc87556df8f`.
[Preceding shared-mask stage](https://github.com/lokad/image/blob/68dbc0f7d98c82510522ffd6c1d85a7159bbe432/benchmarks/JPEG-SHARED-BLOCK-SUPPORT.md)
and [motivating attribution](JPEG-SPARSE-CURRENT-ATTRIBUTION.md) retain history.
All 289 paths remain required; this is not a whole-release timing refresh.
Publication stays postponed.
