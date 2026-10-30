# Exact SIMD JPEG dequantization

Current costs are superseded by [constant blocks](WARM-JPEG-CONSTANT-BLOCK.md).
The accepted SIMD primitive and all original evidence remain retained.

The fixed inverse's 64 scalar widened products become safe AVX2 low-word
products. The existing sparse-CMYK complete decode uses **13.9% less elapsed
time**, with exact pixels. It remains **2.453× / upper2.462** FFmpeg.

| Complete 513×259 decode | Lokad µs | FFmpeg µs | Ratio / upper | Managed B/op | Encoded input B |
| --- | ---: | ---: | ---: | ---: | ---: |
| Prior sparse-CMYK, independent | 1,247.21 | 437.65 | 2.850 / 2.858 | 1,085,508 | 22,708 |
| SIMD sparse-CMYK, independent | 1,073.80 | 437.73 | 2.453 / 2.462 | 1,085,513 | 22,708 |
| Current baseline420, owned RGBA8 | 1,249.21 | 1,008.36 | 1.239 / 1.245 | 757,319 | 52,093 |
| Current DC-only caller CMYK | 1,002.66 | 330.66 | 3.032 / 3.051 | 1,634,905 | 10,895 |

Companions refresh affected existing paths, without causal gain claims against
their older costs. Native allocations are unmeasured. Complete operations include
fresh owned output acquisition/release or fresh caller destination allocation,
decoding, normalization/packing and scratch cleanup on one warm CPU. Original
packets, input delivery, sampling, settings, native workers and ownership checks
remain frozen. Native/managed normal/scalar pixels match exactly; CMYK black is
never alpha. ICC conversion and EXIF reorientation remain off.

The low 16 bits of an Int32×UInt16 product equal word multiplication of the
truncated operands throughout the domain. DC keeps its full-width product,
level shift and signed-word saturation in their original order. Existing matrix
rounding, signed wrap, scalar fallback, cancellation and admission are unchanged.
No new owned buffer, cache or storage charge is introduced; measured sparse-CMYK
allocation is 5 B/op above the prior independent result. Three actual-helper
tests cover every AC low word, every quantizer with wide coefficient boundaries,
unaligned spans, guards, borrowing and DC extremes; the independent fixed-kernel
spectra and full-range scalar-block tests remain.

A fresh three-actual pair clears the strict 5% raw/normalized screen. One new
twelve-actual confirmation reuses the valid original twelve: **13.904% filtered /
13.923% unfiltered elapsed** and **13.921% normalized-point** reduction. Raw 99.9%
CIs and conservative normalized envelopes separately separate. Each companion
has one twelve-actual confirmation. All actuals, brackets and idle-v2 guards are
retained. A missing cached selector runtimeconfig was recovered before scoring;
no completed measurement was repeated.

All **5,166 checks/OS** pass (2,083 internal, 2,135 public, 948 live), plus
27 internal/98 public focused checks normal/scalar per OS. The same actual
Release package, governed streaming consumer and three examples pass on
Windows/Linux. One core build/OS; zero harness/native/selector builds.

This common inverse invalidates older JPEG rendering costs, including caller,
stream, progressive and extended-color paths. Only the three controls above are
refreshed here. Metadata-only inspection and pixel-coding validation bypass it;
encoder/other-codec/resident-editing/text evidence remains applicable. Caller
CMYK is now the largest confirmed material gap. Stop this SIMD preparation
primitive; the full **268-path ≤1×** objective remains open.

[Evidence](warm-jpeg-dequantization-evidence.json),
[original fixed-policy oracle](CURRENT-CMYK-FIXED-INVERSE-COST.md).
Mirrored/member-verified jpeg-simd-dequantization-qualified.tar.xz, 168 members,
SHA256 601f710049b4c9a172fce8e6bca53db221f5caf92eed31d93545ed60013183f2; dependencies are pinned by hash.
