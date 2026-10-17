# Fixed JPEG entropy emission with nonzero occupancy

The [fixed AC fusion round](WARM-JPEG-FIXED-AC-FUSION.md) replaces the three fixed
encoder timings below, including the two previous short regression witnesses.
Earlier independent results remain historical; their hot traversal has changed.

Complete cancelable RGBA16 JPEG420 encoding improves **19.1%**, from **2.006×**
FFmpeg to independently confirmed **1.651× / upper 1.663×**, at **3,031.41 µs**
and unchanged **263,788 B/op**. All twelve actuals are retained, including one
BDN-filtered outlier; the upper includes filtered/all-actual uncertainty.
Every versioned quiet guard passes. Stop this fixed pair.

The existing original 513×259 word raster, quality69/native qscale5, background
RGB 0x204060 and packets remain frozen. A live cancelable token, never signaled
during measurement, selects fixed geometric Huffman coding on a stock MemoryStream.
Full word normalization, alpha flattening, conversion, transform, serialization
and owned output acquisition/release are timed on both subjects. Token/source
setup is excluded. Native has no equivalent cancellation setting. Its wrappers
and contexts are reused; no output is cached.

One six-second profile places about 60% in fixed-block and entropy emission.
The accepted primitive uses eight AVX2 comparisons and BMI2 x64 bit deposits to
map nonzero raster coefficients into their zigzag positions. It visits only
nonzero AC values while preserving DC, ZRL/EOB, amplitudes, bit order, work,
callbacks and exact partial failures. Eight row masks are derived from the
independent standard zigzag as shared format metadata. No per-image heap
workspace is added; the original scalar traversal remains on other processors.
Nine new controls cover every scan position, signed extremes, sparse/dense blocks,
pending bits, work/output quotas, callback exceptions and cancellation against
the original scalar traversal, including predictor and pending-writer state.

The managed **81,275-byte** packet remains byte-identical to the
[word forward-only anchor](WARM-JPEG-FORWARD-RGBA16.md), reusing its independent
coefficient/amplitude recoding proof. Native remains **52,117 bytes**. Registered
decoder pixels/source errors and opaque output alpha are unchanged; source-PSNR
gap **0.105942 dB** stays within **0.25 dB**, and decoder maximum3 / RMSE0.593394
within maximum4 / RMSE1.5. Source/owners and pre-cancel rejection without output
pass. No quality, tolerance or packet-policy change; size-efficiency parity is
not claimed.

The short candidate passes at 1.658× / upper1.717× before independent confirmation
on the same core/harness. Shared cancelable JPEG444 and forward-only word420
regression means pass at 1.496× and 1.666× before full qualification; these are
screens with prior confirmations retained. Only three workload types and the two
existing original rasters are used; no quality sweep or coefficient-visitor repeat.

All **3,343 Release tests per OS**, including **896 live FFmpeg comparisons**,
and **92 focused scalar checks per OS** pass. The same actual dependency-free
Release package passes its consumer and all three examples on Windows/Linux.
Sequential Release runs use one pinned CPU, one codec/conversion thread, normal
SIMD, tiering off and workstation nonconcurrent GC. Startup, I/O, IPC, preparation
and validation are excluded; native allocations are unmeasured.
[Numeric evidence](warm-jpeg420cancel16-evidence.json) retains all actuals,
allocations, guards, actual jobs/four identities, profile and qualification.
Remaining material public/current-source warm coverage stays open.
