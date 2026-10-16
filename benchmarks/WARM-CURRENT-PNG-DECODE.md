# Current PNG decode confirmations

21 of the 25 named PNG decode cases awaiting current-path evidence independently
confirm within the 2x warm single-CPU target for current production paths.
Older full-operation timings preceded shared overwrite-pool and fused framing/
inflation changes. Stop confirmed pairs; full required warm coverage remains open.

| Existing case | PNG color / depth / filter | Lokad.Image µs | FFmpeg µs | Ratio / upper | Managed B/op | Actual / selected |
| --- | --- | ---: | ---: | ---: | ---: | ---: |
| DecodeRgb16 | 2 / 16 / 0 | 2,297.09 | 2,227.02 | 1.031 / 1.034 | 6,504 | 12 / 12 |
| DecodeGray16 | 0 / 16 / 0 | 714.81 | 957.95 | 0.746 / 0.751 | 2,408 | 12 / 12 |
| DecodeGrayAlpha16 | 4 / 16 / 0 | 1,563.09 | 1,795.59 | 0.871 / 0.874 | 4,456 | 12 / 11 |
| DecodeUp8 | 6 / 8 / 2 | 2,209.03 | 1,518.35 | 1.455 / 1.458 | 4,456 | 12 / 12 |
| DecodePaeth8 | 6 / 8 / 4 | 3,230.00 | 2,843.71 | 1.136 / 1.140 | 4,456 | 12 / 12 |
| DecodeGray16Transparency | 0 / 16 / 0 | 2,970.49 | 2,055.01 | 1.445 / 1.450 | 2,440 | 12 / 12 |
| DecodeAverage8 | 6 / 8 / 3 | 2,380.32 | 1,850.31 | 1.286 / 1.298 | 4,456 | 12 / 10 |
| DecodePalette4 | 3 / 4 / 0 | 109.81 | 140.53 | 0.781 / 0.799 | 984 | 12 / 12 |
| DecodeGray8 | 0 / 8 / 0 | 357.47 | 363.27 | 0.984 / 0.993 | 1,384 | 12 / 11 |
| DecodeGray4 | 0 / 4 / 0 | 116.09 | 169.54 | 0.685 / 0.698 | 872 | 12 / 11 |
| Decode8 | 6 / 8 / 0 | 1,904.89 | 1,392.72 | 1.368 / 1.371 | 4,456 | 12 / 12 |
| DecodeNoCompression16 | 6 / 16 / 0 | 322.73 | 296.18 | 1.090 / 1.096 | 8,552 | 12 / 11 |
| DecodePalette8PartialAlpha | 3 / 8 / 0 | 540.03 | 363.49 | 1.486 / 1.494 | 2,208 | 12 / 12 |
| DecodeRgb8Transparency | 2 / 8 / 0 | 1,281.05 | 1,282.56 | 0.999 / 1.001 | 3,464 | 12 / 12 |
| DecodeRgb8 | 2 / 8 / 0 | 1,293.31 | 1,523.89 | 0.849 / 0.853 | 3,432 | 12 / 12 |
| DecodeGrayAlpha8 | 4 / 8 / 0 | 845.98 | 1,156.04 | 0.732 / 0.733 | 2,408 | 12 / 12 |
| DecodeRgb16Transparency | 2 / 16 / 0 | 2,316.17 | 3,182.24 | 0.728 / 0.732 | 6,536 | 12 / 12 |
| DecodeNoCompression8 | 6 / 8 / 0 | 150.38 | 136.49 | 1.102 / 1.114 | 4,456 | 12 / 11 |
| DecodePaeth16 | 6 / 16 / 4 | 5,401.14 | 5,511.74 | 0.980 / 0.990 | 8,552 | 12 / 12 |
| DecodeGray1 | 0 / 1 / 0 | 59.11 | 303.42 | 0.195 / 0.199 | 488 | 12 / 12 |
| DecodePalette8 | 3 / 8 / 0 | 349.92 | 364.06 | 0.961 / 0.970 | 2,456 | 12 / 12 |

Each operation decodes the identical resident original 513x259 PNG into an
independently owned RGBA buffer. Managed public Decode, admission, container/
checksum/framing validation, complete inflation/predictor reconstruction,
coded-color/depth expansion, output materialization and disposal are timed.
FFmpeg's public software PNG decoder includes required color/alpha expansion,
word endian conversion, independent output acquisition/release. Context/frame/
packet wrappers are warm; every call decodes all pixels. Native padded rows are
exposed without an extra packing copy. Managed released exact-size storage may
be reused while independent live owners remain separate. No decoded-output cache.
Startup, I/O, IPC, generation, hashing and validation are excluded.

Frozen packet, original/projected raster and native output hashes remain exact.
Every RGBA sample, precision, alpha, hidden RGB, normal/scalar CLI output, source
preservation and simultaneously held independent owners pass before/after timing.
Word conversion is timed where required. Only existing named packets/projections
and the two retained original byte/word rasters are used; no additional image or
shape/depth/filter/palette/quality grid is introduced.

Every useful passing three-actual screen precedes an independent twelve-actual
confirmation on the same immutable production, benchmark and native worker.
Every actual remains retained; conservative uppers include selected/all-actual
uncertainty against the faster native bracket's lower confidence bound. Both
versioned quiet guards pass every scored run. Sequential Release uses CPU0,
one codec/conversion thread, normal SIMD, tiering disabled and workstation
nonconcurrent GC. Managed allocations include harness costs; native allocation
totals are unmeasured.

The first RGB16/gray16/gray-alpha16 round uses the existing three-case PNG job.
One shared driver is prepared for the three-actual screens and one for the
twelve-actual confirmations; each case runs sequentially. Case IDs, generated
parameters and identical driver hashes are checked in the retained evidence.
Production, harness and native rebuild counts are zero; every cached child,
including WarmDry, receives verified assemblies.

The gray16 short run was retained after correcting a private postcheck of a
historical descriptive label, gray versus the clarified gray16be spelling.
The original refused controller record is retained. Packet/pixel hashes, actual
depth/color/filter/interlace, binaries, raw timings, ownership checks and both
quiet guards revalidate unchanged. No replacement timing run is used.

The [numeric evidence](warm-current-png-decode-evidence.json)
retains all samples, quiet guards, allocations, exact identities and qualification
reuse. Each result certifies its named fixed input/public boundary.

The stored RGBA16 current-code screen demonstrated 2.219x
and 1,072,108B/op. Its 1,063,366-byte concatenated IDAT exceeded
the private one-MiB input cache ceiling, allocating on every decode. Raising
that independent cache to the existing four-MiB pool default halves decode
time (659.32 → 322.73µs), independently confirmed
at 1.090x / upper1.096x and
8,552B/op. At most two released exact-size arrays fit within
the combined four-MiB idle ceiling; active lease bytes, checksum/framing/work,
failure order and independent live owners remain unchanged. Startup is excluded.

One production build serves both sampling lengths; the existing verified drivers,
benchmark harness, native worker and packets are reused without rebuilding them.
All twelve confirmation actuals are retained. Fresh qualification passes 3,483
Release tests per OS (1,199 internal, 1,386 public, 898 live differential), plus
the consumer and all three examples through the same actual Release package on
Windows/Linux. The package remains safe managed and dependency-free.

Only stored16 among the 51 existing PNG decode/composition rows crosses the old
ceiling: exact IDAT sizes or whole-packet upper bounds establish this without
a method audit or new workload. The earlier eleven PNG cases remain below
one MiB and use the same selected algorithms and exact lease behavior; reuse
their confirmations. Other codecs/editing/encoding are unaffected. Existing
uncertain cases remain open for their earlier named invalidators. The numeric
evidence retains the original failing screen and fresh qualification archive.

Remaining named cases, largest retained ratios first: DecodePalette2, DecodeGray2, DecodeGray8Transparency, DecodePalette1.
