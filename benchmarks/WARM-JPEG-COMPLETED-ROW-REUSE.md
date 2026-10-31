# Exact completed-row JPEG reuse

Horizontal CMYK decoding improves **10.48% independently** (all actuals 10.26%,
native-normalized point 8.00%). Raw confidence separates in both views, and the
normalized candidate upper **1.396** is below original lower **1.417**.
Current cost is **535.551 µs**, **1.366× / upper 1.396**, **1,085,755 B/op**;
the −2 B/op allocation difference is noise. This remains a CPU gap.

| Existing complete warm one-CPU decode | Lokad µs | Ratio / upper | Allocated B/op |
| --- | ---: | ---: | ---: |
| CmykAdobeAc | 535.551 | 1.366 / 1.396 | 1,085,755 |
| CmykAdobeSparse2D | 638.035 | 1.442 / 1.481 | 1,085,758 |
| NativeDecode420 | 1,111.270 | 1.097 / 1.136 | 757,362 |
| Progressive420 | 2,138.627 | 1.096 / 1.108 | 775,318 |
| Progressive422 | 2,728.898 | 1.215 / 1.258 | 847,116 |
| Progressive444 | 4,557.046 | 1.351 / 1.392 | 984,212 |
| ProgressiveCancelable444 | 4,788.190 | 1.421 / 1.451 | 984,212 |

An eleven-line insertion reuses completed output bytes when the already-computed
second integer row is exactly zero. All eight column results then share the same
existing bias, modulo-Int32 product, shift, clipping and packing. The two-low-row
guard, row count, dequantization, whole-DC shortcut, dense/nonzero-row paths and
portable fallbacks are unchanged. There is no new buffer, owner or stack storage.
Cancellation and shared-admission boundaries are unchanged.

The first original short is 11.48% slower than the retained cost, triggering the
predeclared one fresh independent original twelve: **598.277 µs**. The candidate
short improves 9.92% raw / 7.65% normalized against that fresh baseline. One
independent candidate twelve passes the default 5% gate without a waiver.
The superseded noisy 18% short comparison remains archived and is not a claim.
No additional pair, variant, profile or benchmark input is introduced.

All seven affected displayed costs are reconciled once on the immutable build.
Companion values are current costs, not causal gain/regression comparisons.
Progressive420 is now a confirmed gap (lower 1.080), replacing old confidence-open
evidence. Frozen packets, settings, complete normalization, fresh owned output and
release are timed on both sides. Managed/native normal/scalar pixels, input and
independent output ownership and quiet-v2 guards pass. Native allocation is unmeasured.

Eight named no-call proofs preserve relevant prior costs: progressive grayscale,
dense CMYK, DC-only caller CMYK/YCCK420/411/410 and both JPEG coding validations.
Retained progressive coefficient records cover visible blocks; omitted progressive
MCU padding is DC-only and takes the earlier unchanged return. They are not claimed
as counts of all decoded MCU blocks. Other codecs, encoders, editing, text and
metadata inspection bypass the changed inverse primitive.

**5,291 checks per OS** pass: 2,158 internal + 2,185 public + 948 live.
Focused 349 internal and 346 public controls pass normal/scalar on both OSes and
Linux without AVX512. Two new frozen-hash controls cover 512 repeated-row and
512 nonzero-second-row spectra, signed/overflowing products, guarded unaligned
spans, source/quantizer immutability and zero warm arithmetic allocation. The
independent integer oracle also reproduces the existing 640-basis fixture hash.

One core/internal build per OS; zero public/differential/harness/native/selector
builds. The same actual Windows Release package/DLL passes both OSes and all three
examples, including exact Q14 one-/two-AC CMYK and retained-output admission.
Package SHA256 fbdc18545b8af35eb74a00c070ed876e7d376a38852c0c6defba47d1b965d100
(387,690 bytes). Published releases are unchanged. Temporary metadata/oracles
are restored byte-exactly; accepted production remains. The primitive is closed.

[Numeric evidence](warm-jpeg-completed-row-reuse-evidence.json) retains all actuals,
brackets, confidence, source/producer/package identities, scope and restoration.
Raw jpeg-completed-row-reuse-qualified.tar.xz SHA256
703866a741933611ed31767bbd094b37ad9761e72b30ea588604397ececce490
(2,944,900 bytes, 233 members) is mirrored and every member verified. Dependencies:
[horizontal CMYK](CURRENT-HORIZONTAL-CMYK-JPEG-COST.md) and
[refinement-window](WARM-JPEG-REFINEMENT-WINDOW.md) retain original inputs,
reference/harness identities and coefficient records. The full 268-path objective
remains open; this is not a release-wide immutable-package performance refresh.
