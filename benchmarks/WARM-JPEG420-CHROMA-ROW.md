# JPEG 4:2:0 chroma row conversion

The independent quiet repeat is **3.605 ms / 1.978x FFmpeg**, but its conservative
upper bound is **2.022x**. This fixed case remains **open**. All twelve actuals
are retained; ten filtered results form the reported mean, and the upper takes
the larger filtered/all-actual bound. Both quiet guards pass.

The same original 513x259 RGBA8 raster, native qscale 5, managed quality 69 and
RGB 0x204060 background remain fixed. Exactly one managed quality is evaluated.
Encoded bytes, source-PSNR gap 0.106110 dB, opaque alpha, decoder agreement,
source integrity and independent outputs are unchanged. Complete public encode,
conversion, output acquisition and release boundaries match the
[previous luma round](WARM-JPEG420-LUMA-ROW.md).

| Retained observation | Lokad.Image ms | Ratio / conservative upper | Managed B/op |
| --- | ---: | ---: | ---: |
| Previous luma screen | 6.258 | 3.462 / 3.731 | 527,251 |
| Subsampled row SIMD screen | 3.828 | 2.099 / 2.232 | 527,335 |
| Inlined conversion screen | 3.630 | 1.989 / 2.120 | 527,335 |
| Independent twelve-actual repeat | 3.605 | 1.978 / 2.022 | 527,335 |

The retained repeat mean is 42.4% below the previous screen. Native allocations
are not measured; managed raw GC totals include harness overhead. This does not
establish uniform parity, compression efficiency parity or a simultaneous rerun
of all reported profiles.

One primitive converts dense 2:1 chroma rows from sixteen RGBA8 pixels in one
or two source rows. Safe SIMD preserves integer alpha flattening and separate
binary64 matrix operations. Even/odd samples accumulate in the original dy/dx
order; only the final average rounds to a byte. Right-edge padding, other depths
and other hardware retain scalar sampling. No workspace, pixel cache, work
charge or admission change is introduced.

Generated-code inspection found four helper calls returning 512-bit vectors
through stack slots. An inlining hint removes all four calls; a second inspection
confirms no separately compiled helper. A final exact-reciprocal box-scale
experiment did not demonstrate a useful gain and was reverted byte for byte.
Its raw screen remains recorded. The retained code was force-rebuilt on both
OSes and all 574 methods and flags match the actual package.

Both systems pass 2,449 Release tests: 699 internal, 854 public and 896 live
FFmpeg comparisons. All 34 scalar component/chroma-row controls pass. Twenty new
controls include an analytical trap for rounding taps before averaging, mixed
alpha/backgrounds, source/output guards, lane order and invalid row layouts.
All three usage examples pass through the same actual Release package on both
OSes. The [numeric evidence](warm-jpeg420-chroma-row-evidence.json) records every
scored run, source/child/package identity, JIT diagnostic, rejected experiment
and immutable raw archive. No timing guard failed in this round.

Keep the upper-bound gap open. Cover remaining material unknown paths before
more small tuning; do not widen tolerances, change settings or repeat the same
source merely to manufacture a passing bound.
