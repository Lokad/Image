# Vertical VP8 SIMD filtering

The five existing VP8 decode workloads are **5.5-9.5% faster**, with unchanged
allocations and pixels. Live opaque decoding improves9.1%, from1.838x to1.663x
FFmpeg; all five remain qualified short parity gaps.

Eight independent vertical edge neighborhoods are transposed into signed-word
SIMD lanes, filtered with the original arithmetic, then transposed back. All
loads/stores stay within bounded rows. The original horizontal SIMD/scalar
bodies, edge order, work charges and cancellation-before-writes remain. The
guard requires SSE2,8or16 segments and complete row neighborhoods; other inputs
retain scalar filtering. No heap, fields, table or mandatory admission is added.

| Existing selector | Lokad / FFmpeg µs | Ratio / upper | Managed B/op | Managed time improvement |
| --- | ---: | ---: | ---: | ---: |
| VP8 opaque / live token | 7,993.36 / 4,805.60 | 1.663 / 1.725 | 760,548 | +9.06% |
| VP8 opaque | 7,827.85 / 4,794.82 | 1.633 / 1.679 | 760,548 | +9.50% |
| VP8 opaque / simple filter | 8,065.87 / 4,900.49 | 1.646 / 1.708 | 760,548 | +5.53% |
| VP8 + compressed alpha | 9,896.30 / 5,966.36 | 1.659 / 1.690 | 1,966,616 | +8.17% |
| VP8 + alpha / fresh Stream | 10,072.43 / 6,002.17 | 1.678 / 1.756 | 2,327,699 | +8.31% |

Each quiet short retains all three200ms actuals, native50ms pre/post brackets and
both `exclusive-vm-idle-v2` guards. Managed allocation includes harness costs;
native allocation is unmeasured. Every mean exceeds1, so no routine twelve-actual
confirmation was run. The same two513x259 rasters, frozen packets, workers and
quality gates are reused. Complete acquisition, parsing, entropy/reconstruction,
filter/conversion, independent output ownership and release remain timed;
the Stream case includes fresh input acquisition, reads, copies and EOF.

All frozen managed/native hashes, exact alpha, native RGBmax4/RMSE1.5, normal CLI,
source preservation and independent-owner gates pass. The unchanged harness
requests optional scalar CLI checks for live, simple-filter and Stream cases;
ordinary opaque/alpha intentionally reportfalse. The ordinary wrapper initially
assumedtrue, after its measurement and both guards had passed. The initial
refusal is retained and every remaining check was validated on that same result.
The resolution reused the original measurement, all frozen inputs and every
comparison gate.

The retained profile's exact filter bytes match the predecessor; no new profile
was run. An independent512basis-bit audit proves the original transpose/inverse.
Four new minimal-row cases compare every byte and exact work against the existing
scalar reference, including last-row/no-padding and macroblock borders. Existing
complete macroblock and work/cancellation failure checks pass. Removing only the
new usings, guarded dispatch and helpers exactly restores the entire predecessor
producer; existing horizontal/scalar arithmetic remains byte for byte.

**3,718 Release tests perOS** pass:1,384internal+1,436public+898live FFmpeg, plus
30internal+251public scalar controls perOS. The same actual Release package's
consumer and all three examples pass on Windows/Linux. One producer build perOS,
zero harness/selector/native builds during five short jobs, no new workload or
image/settings grid, and all control means read before full qualification.

Only five existing VP8decode rows change. The other202 comparisons and all60
passing proofs retain applicable production paths. Three older independent gaps
are superseded by qualified short evidence, retaining127selected independent
confirmations+80qualified shorts. The release0.1.0 tag/packages remain intact.
[Numeric evidence](warm-vp8-vertical-filter-evidence.json) pins source/builds,
allocations, every actual, uncertainty, raw archives and the actual package.
The full207-case <=1x objective remains open at60passes/143gaps/4confidence.
