# Fused lossless WebP inverse prediction

Native-input VP8L decoding is **6.6–7.2% faster**; compressed-alpha VP8 is
**3.6–3.9% faster**. Live lossless improves from1.626x to1.516x FFmpeg. These
are qualified three-actual improvements; all seven measured cases remain gaps.

The inverse predictor computes all four channels and residual addition together
with safe SSE2 intrinsics. Nested floor averages, selection ties, signed
half-gradient truncation, clipping and modulo-byte addition stay exact. Predictor
mode metadata is reused within its tile; pixels retain original dependency order,
borders and right-edge behavior. Original scalar helpers and fallback remain
byte-identical. No field, table, heap, mandatory admission or work charge is added.

| Existing decode | Lokad / FFmpeg µs | Ratio / upper | Managed B/op | Input B | Managed time improvement |
| --- | ---: | ---: | ---: | ---: | ---: |
| Native VP8L / live token | 4,573.29 / 3,016.50 | 1.516 / 1.579 | 1,216,180 | 183,812 | 7.03% |
| Native VP8L | 4,576.04 / 3,022.99 | 1.514 / 1.615 | 1,216,180 | 183,812 | 6.56% |
| Native VP8L / fresh Stream | 4,351.41 / 3,030.27 | 1.436 / 1.480 | 1,924,876 | 183,812 | 7.24% |
| VP8 + compressed alpha | 9,272.91 / 5,970.23 | 1.553 / 1.648 | 1,966,616 | 98,590 | 3.88% |
| VP8 + alpha / fresh Stream | 9,391.90 / 6,005.14 | 1.564 / 1.714 | 2,327,699 | 98,590 | 3.58% |
| Literal/run VP8L | 2,269.12 / 2,157.09 | 1.052 / 1.074 | 1,074,326 | 533,100 | 0.16% |
| Sixteen-color VP8L | 785.58 / 544.96 | 1.442 / 1.495 | 1,351,972 | 11,810 | 0.23% |

Literal/run and indexed controls are essentially unchanged. Allocations remain
unchanged for every case; B/op includes harness costs. Native allocations are
unmeasured. Frozen packets, the same two513x259 RGBA8/16 rasters and every sample,
including hidden RGB, remain. No fixture, shape, quality or reference grid is added.

Complete acquisition, parsing, entropy, transforms/reconstruction/conversion,
resource admission, independent output and disposal remain timed. Streams include
fresh acquisition, reads, copies and EOF. Startup, IPC, disk I/O, preparation and
validation remain outside timing. All7shorts retain all3actuals at200ms, native
50ms brackets, frozen hashes, independent owners and both `exclusive-vm-idle-v2`
guards. VP8L normal/scalar RGBA stays exact; compressed VP8 retains exact alpha,
RGBmax4/RMSE1.5 and its unchanged optional scalar-CLI policy. Every mean remains
above1, so no long confirmations or predictor micro-variants were run.

**3,722 Release tests per OS** pass:1,388internal+1,436public+898live FFmpeg,
plus65internal+243public selected scalar controls. Two new tests compare253,952
mode/channel inputs with the original scalar oracle, including all byte-pair
addition, averaging and half-gradient boundaries. The same actual Release
package's consumer and allthree examples pass on Windows/Linux. One measured
producer build per OS; frozen harness, selectors, workers and fixtures reused.
All affected means were read before full production qualification.

Only7rows change. Other200comparisons/all60passing proofs remain applicable;
totals stay60passes/143gaps/4confidence,127independent+80qualified shorts. The
rejected channel-read experiment is absent from production. The0.1.0release tag
and packages remain. [Numeric evidence](warm-vp8l-simd-prediction-evidence.json)
pins sources, actuals, conservative bounds, allocations, guards and actual package.
The full207-case warm <=1x objective remains open.
