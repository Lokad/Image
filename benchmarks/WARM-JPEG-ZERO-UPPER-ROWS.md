# Omit exact zero JPEG frequency rows

Four-component costs are superseded by
[caller coefficient reuse](WARM-JPEG-CALLER-COEFFICIENT-SLOTS.md). The zero-row
primitive and unchanged three-component420 confirmation remain applicable.

The existing sparse-CMYK complete decode uses **25.5% less elapsed time**, with
exact pixels. Its **1.658× / upper1.669** FFmpeg cost remains a gap. No owned
storage, cache or table is added.

| Complete 513×259 decode | Lokad µs | FFmpeg µs | Ratio / upper | Managed B/op | Encoded input B |
| --- | ---: | ---: | ---: | ---: | ---: |
| Prior sparse-CMYK, independent | 982.26 | 450.45 | 2.181 / 2.197 | 1,085,515 | 22,708 |
| Zero-upper-row sparse-CMYK, independent | 731.43 | 441.28 | 1.658 / 1.669 | 1,085,520 | 22,708 |
| Current baseline420, owned RGBA8 | 1,124.01 | 1,008.25 | 1.115 / 1.120 | 757,328 | 52,093 |
| DC-only caller CMYK, unchanged-path evidence | 600.31 | 332.30 | 1.807 / 1.817 | 1,634,913 | 10,895 |

Baseline420 is an affected current-cost companion; elapsed time is 0.958% higher
than its prior independent result, without a fresh paired causal claim. DC-only
caller-CMYK bypasses the changed matrix, so its valid prior confirmation is reused.
It is now the largest confirmed material gap. Original inputs/settings and complete
operation boundaries remain frozen; native allocations are unmeasured. Measured
sparse-CMYK allocation rises by 5 B/op; existing stack capacity and all ownership,
admission, input, cancellation and output-lifetime contracts stay unchanged.

Six exactly zero upper frequency rows transform to zero under the original row
shortcut. A guarded row count shares the existing row calculation and omits those
rows and their column terms. Both lower rows retain **all sixteen frequencies**,
original word wrap/saturation and the column's wrapped-word bias. The remaining
unit/first-frequency products and reflected outputs preserve exact fixed-matrix
pixels. Other blocks retain the existing matrix; scalar, whole-DC and
dequantization paths stay unchanged. Eligibility uses post-product words, so
nonzero source coefficients that wrap to zero are handled correctly.

An independent full scalar matrix proves 4,096 mixed moderate/wide-product blocks,
262,144 exact pixels. One actual production test checks its frozen hash, all lower
frequencies, wrapped upper products, unaligned guarded views, borrowed inputs and
zero-budget execution. Existing 640 signed-boundary basis, 2,048 dense full-range,
native spectra, DC, cancellation and fallback coverage remain. This is separate
from the closed floating axis/prefix and certified-Q16 quadrant trials.

The pre-candidate one-second JWP1 profile collected 896 samples: 517 on inverse
callchains and 51 fill leaves whose mapped production caller was the inverse.
This justified removing zero-row evaluation/fill dispatch. The same trace was
decoded without a second profile or build. Fractions are diagnostic; they are
neither current post-change attribution nor elapsed-time predictions.

A fresh three-actual pair clears the strict 5% raw/normalized gate. One candidate
twelve reuses the valid original twelve: **25.536% filtered / 25.433% unfiltered
elapsed** and **23.989% normalized-point** reduction. Raw 99.9% CIs and conservative
normalized envelopes separately separate. Baseline420 receives one twelve-actual
confirmation. Exact normal/scalar/native pixels, source/owners, native settings
and idle-v2 guards pass; no completed measurement is repeated or gate relaxed.

All **5,171 checks/OS** pass (2,088 internal, 2,135 public, 948 live), plus
32 internal/98 public focused checks normal/scalar per OS. The same actual
Release package, governed streaming consumer and three examples pass on both
platforms. One core build/OS; zero harness/native/selector builds.

Older affected nonconstant AVX2 JPEG costs need applicable current evidence
before CPU-pass claims. The two affected controls above are refreshed; unchanged
DC-only/metadata-only/validation/encoder/other-codec/editing/text evidence is reused
where applicable. Stop this primitive and row-count variants. The full
**268-path ≤1×** objective remains open.

[Evidence](warm-jpeg-zero-upper-rows-evidence.json),
[prior packed-word matrix](WARM-JPEG-PACKED-WORD-INVERSE.md).
Mirrored/member-verified jpeg-zero-upper-rows-qualified.tar.xz, 154 members,
SHA256 86dd65feceb16b484a483f20c103838f35aff7de3fcaff99a400cb3bcb471bb1; dependencies include the retained baseline profile.
