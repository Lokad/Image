# Exact packed-word JPEG inverse

Affected sparse-CMYK/baseline420 costs are superseded by
[zero upper rows](WARM-JPEG-ZERO-UPPER-ROWS.md). The accepted packed-word
primitive and unchanged DC-only confirmation remain retained.

The existing sparse-CMYK complete decode uses **8.6% less elapsed time**, with
exact pixels. Its **2.181× / upper2.197** FFmpeg cost remains the largest
confirmed material gap. No per-operation storage is added.

| Complete 513×259 decode | Lokad µs | FFmpeg µs | Ratio / upper | Managed B/op | Encoded input B |
| --- | ---: | ---: | ---: | ---: | ---: |
| Prior sparse-CMYK, independent | 1,074.77 | 441.68 | 2.433 / 2.446 | 1,085,513 | 22,708 |
| Packed-word sparse-CMYK, independent | 982.26 | 450.45 | 2.181 / 2.197 | 1,085,515 | 22,708 |
| Current baseline420, owned RGBA8 | 1,113.34 | 1,009.11 | 1.103 / 1.107 | 757,328 | 52,093 |
| DC-only caller CMYK, unchanged-path evidence | 600.31 | 332.30 | 1.807 / 1.817 | 1,634,913 | 10,895 |

Baseline420 refreshes an affected existing control, without a fresh paired causal
gain claim. DC-only caller-CMYK returns before the changed matrix; its valid prior
twelve-iteration confirmation is reused. Source-body hashes prove the warm DC,
dequantization and generated-kernel bodies unchanged; new row weights are created
once and never accessed by that branch. Startup is excluded. No new control or
settings grid. Native allocations are unmeasured. Sparse-CMYK's measured
allocation rises by 2 B/op; no dynamic owned capacity or lifetime is introduced.

Safe AVX2 signed-word multiply-add pairs replace widened product pairs in the
two matrix passes. Modulo-Int32 associativity preserves all overflowing sums.
The independently generated Q14 weights fit signed words. Four inline SIMD
values (128 bytes of fixed type data) prepare row pairs without another heap
table. Column DC bias still wraps in signed words; the unit sums remain widened
before multiplication. Shifts, saturation, constant-block/dequantization paths,
scalar fallback, cancellation and admission stay unchanged. No upstream code
or tables are copied.

The full-domain algebra and 983,096 bounded comparisons precede implementation.
An independent full scalar matrix reproduces the original frozen 32-block hash,
then freezes **640 signed-boundary basis blocks and 2,048 dense full-width
coefficient/quantizer blocks**. Two production tests check those exact hashes,
lane order, guards, unaligned views, borrowed inputs and zero-budget execution.
Original native spectra, DC/full-range and cancellation tests remain.

A fresh three-actual pair clears the strict 5% raw/normalized screen. One new
twelve-actual confirmation reuses the valid original twelve: **8.607% filtered /
8.674% unfiltered elapsed** and **10.387% normalized-point** reduction. Raw 99.9%
CIs and conservative normalized envelopes separately separate. Baseline420 has
one twelve-actual current-cost confirmation. All original inputs/settings,
normal/scalar/native exact pixels, complete operations, source/owners and idle-v2
guards pass. No completed job is repeated, gate relaxed or variant attempted.

All **5,170 checks/OS** pass (2,087 internal, 2,135 public, 948 live), plus
31 internal/98 public focused checks normal/scalar per OS. The same actual
Release package, governed streaming consumer and three examples pass on both
platforms. One core build/OS; zero harness/native/selector builds.

Older nonconstant AVX2 JPEG rendering costs need current evidence before CPU-pass
claims; only the two affected controls above are refreshed here. Metadata-only
inspection and pixel-coding validation bypass the inverse. Other-codec,
encoder/resident-editing/text evidence remains applicable. Stop this primitive;
the full **268-path ≤1×** objective remains open.

[Evidence](warm-jpeg-packed-word-inverse-evidence.json),
[prior constant blocks](WARM-JPEG-CONSTANT-BLOCK.md).
Mirrored/member-verified jpeg-packed-word-inverse-qualified.tar.xz, 153 members,
SHA256 5f9cd770480f4a8ab0e1b5dfa8383232a693757d11f846f2a1191dc0e5a77ee2; dependencies are pinned by hash.
