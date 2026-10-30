# Separable strips for JPEG 4:1:1

The existing complete owned RGBA8 control improves **56.2%** with exact
pixels. Its independent **1.233x / upper 1.237** remains a gap.

| 513x259 DC-only H4V1 baseline JPEG | Lokad us | FFmpeg us | Ratio / upper | Managed B/op | Encoded input B |
| --- | ---: | ---: | ---: | ---: | ---: |
| Original, independent | 1,257.52 | 449.06 | 2.800 / 2.823 | 751,280 | 3,904 |
| Separable strips, independent | 550.79 | 446.84 | 1.233 / 1.237 | 751,295 | 3,904 |

Positions are prepared once per 64-column strip, then the actual existing
horizontal filter and SIMD/scalar color-row kernel reconstruct every row.
Identical top/bottom rows with zero vertical weight exactly preserve the former
Q7-to-Q9 conversion. Original Q14 positions, 1/500 leading-tap threshold, clamped
edges, Q13 color, alpha and every rounding boundary remain unchanged. Caller
samples still use their real pixel routine; passing 4:1:0/YCCK kernels are unchanged.

Existing admission reserves **1,536 bytes before stack allocation**: 1,280 bytes
of descriptors and 256 bytes of Q7 rows, alongside live planes and owned output.
It refunds after actual users on every exit. Refusal is terminal; cancellation
checks each strip and row. No new heap/frame copy, persistent cache, budget or
owner. Full-operation allocation changes by +15 B/op; no allocation win claimed.

Fresh original/candidate three-actual screens precede one candidate twelve,
reusing the valid original twelve: **56.2004% filtered / 56.1783% all
elapsed** improvement. Raw 99.9% intervals and conservative normalized
envelopes both show separation. Frozen packet, independent/native/scalar pixels,
source, ownership, native settings and idle-v2 guards pass. No variants, profiles,
passing-case retiming, harness/native/selector builds or successful job repeats.
One core build per OS is reused throughout.

Both OS pass **5,188 checks each** (2,092 internal, 2,148 public, 948 live),
plus 338 internal/345 public focused checks normal/scalar. Three added public
cases extend baseline/progressive odd-edge, caller/native equality, retained
ownership, refusal/cancellation/cleanup/recovery coverage to 4:1:1. The unchanged
row-kernel arithmetic/guard/portable proof is reused. The same actual Release
package, governed async consumer and three examples pass on both OSes; existing
array/sync/async quarter controls exercise the new path.

Claims cover this original DC-only control and settings. Other affected owned
4:1:1 boundaries need applicable costs before CPU-pass claims. Passing 4:1:0 and
YCCK evidence is reused without retiming. The full **268-path <=1x** objective
remains open.

[Evidence](warm-jpeg-quarter-single-strips-evidence.json),
[original control](WARM-JPEG-QUARTER-STRIPS.md).
Mirrored/member-verified jpeg-quarter-single-strips-qualified.tar.xz, 152 members,
SHA256 e644094f2f51ea3b33fcff4837e1d9a82868ec7b895ea96220b7106e4dc6785e; prior source/build/reference dependencies are pinned.
