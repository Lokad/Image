# Complete-byte VP8L channel decoding

The existing literal/run decode is **36.0% faster**, reducing its ratio from
1.654x to **1.054x** FFmpeg (conservative short upper1.094x). It remains an open
parity gap. The other six affected paths are essentially unchanged; their full
observations, including the1.04% slower alpha Stream, are retained.

After the original green symbol, three complete256-symbol eight-bit channel
trees can decode red, blue and alpha from one staged reservoir. This reuses the
existing tables and object storage, consumes exactly24 logical bit reads and
reads only bytes required by those codes. Short input or insufficient work
declines before reader mutation; live tokens retain the original separate reads.
Zero-bit leaves, green/RLE/cache/groups, transforms and output ownership remain.
No field, table, heap allocation, memory reservation or work charge is added.

| Existing selector | Lokad / FFmpeg µs | Ratio / upper | Managed B/op | Managed time improvement |
| --- | ---: | ---: | ---: | ---: |
| VP8L literal/run | 2,272.83 / 2,156.57 | 1.054 / 1.094 | 1,074,326 | +36.04% |
| VP8L native input | 4,897.39 / 3,019.50 | 1.622 / 1.713 | 1,216,180 | +0.51% |
| VP8L live token | 4,918.99 / 3,024.90 | 1.626 / 1.801 | 1,216,180 | +1.24% |
| VP8L fresh Stream | 4,691.23 / 3,030.05 | 1.548 / 1.670 | 1,924,876 | +2.01% |
| VP8L sixteen colors | 787.37 / 544.00 | 1.447 / 1.477 | 1,351,972 | +0.72% |
| VP8 + compressed alpha | 10,777.19 / 5,977.78 | 1.803 / 1.888 | 1,966,616 | +0.22% |
| VP8 + alpha, fresh Stream | 10,984.83 / 6,010.87 | 1.827 / 1.900 | 2,327,699 | -1.04% |

Each quiet short keeps all three200ms actuals, native50ms pre/post brackets and
both `exclusive-vm-idle-v2` guards. Allocations include harness costs; native
allocations are unmeasured. Every mean exceeds1, so no routine twelve-actual
confirmation was run. The original packets and two513x259 rasters are reused;
input sizes are533,100B literal/run,183,812B native lossless,11,810B indexed and
98,590B compressed-alpha VP8. Settings, workers and quality gates are unchanged.
Complete input acquisition, parsing, decode/conversion, fresh independent output
and release remain timed; Stream cases include reads, copies and EOF.

Frozen managed pixels, source preservation and independent owners pass. Lossless
rows preserve every RGBA sample, alpha and hiddenRGB, with exact normal/scalar
native CLI checks. VP8 alpha retains exact alpha and native RGBmax4/RMSE1.5.
Ordinary Native8 does not request the optional scalar CLI comparison; its false
flag is not a mismatch. The Stream case requests it and passes.

An independent174,720-transition audit checks bit reservoirs, short input and
budgets. Three new internal facts cover channel values/phases/following fields,
refusal/partial failure/cancellation/recovery, and unchanged admission/single-leaf
behavior. **3,714 Release tests perOS** pass:1,380internal+1,436public+898live
FFmpeg, plus63internal+243public scalar controls perOS. The same actual Release
package's consumer and all three examples pass on Windows and Linux.

Only seven existing decode rows are replaced. The other200 comparisons and all
60passing proofs retain applicable production paths. One producer build perOS,
zero harness/selector/native builds during timing, no new profile or workload,
and all control means read before full qualification. The release0.1.0 tag and
packages are preserved. [Numeric evidence](warm-vp8l-byte-channels-evidence.json)
pins builds, source inventories, allocations, every actual, uncertainty, raw
archives and the actual package. The full207-case <=1x objective remains open.
