# Adaptive JPEG nonzero AC planning

Updated 2026-10-17. Skipping zero coefficients with safe SIMD occupancy reduces
the eight existing adaptive JPEG encode times **9.9–14.8%**. Exact packets,
quality, work and allocations are unchanged. **Every row remains a qualified
three-actual parity gap**, with all actuals and both quiet guards retained.

| Existing adaptive encode | Managed µs | Reference µs | Ratio / upper | Managed B/op | JPEG B managed/native |
| --- | ---: | ---: | ---: | ---: | ---: |
| RGBA16 / 4:4:4 | 5,111.29 | 3,849.14 | 1.328 / 1.435 | 556,286 | 158,827/133,043 |
| RGBA16 / 4:2:0 | 2,748.91 | 1,832.39 | 1.500 / 1.610 | 297,780 | 74,056/52,117 |
| RGBA8 / 4:2:0 | 2,556.72 | 1,765.50 | 1.448 / 1.581 | 297,780 | 74,040/52,093 |
| RGBA16 / 4:2:2 | 3,353.32 | 2,425.65 | 1.382 / 1.500 | 294,116 | 90,253/71,210 |
| RGBA8 / 4:2:2 | 3,257.61 | 2,350.81 | 1.386 / 1.513 | 294,116 | 90,214/71,168 |
| RGBA8 / 4:4:4 | 5,075.94 | 3,775.22 | 1.345 / 1.503 | 559,429 | 158,812/133,041 |
| RGBA8 / gray, SIMD libjpeg-turbo | 1,764.82 | 1,410.26 | 1.251 / 1.358 | 154,893 | 58,823/58,910 |
| RGBA8 / 4:2:0 / restart33 | 2,552.02 | 1,610.91 | 1.584 / 1.769 | 297,780 | 74,086/58,012 |

The two original 513×259 RGBA8/16 rasters, background0x204060 and frozen settings
remain: managed quality69 except422 quality65, FFmpeg qscale5, restart33 where
selected. Single-component grayscale uses the approved benchmark-only
**SIMD-enabled standalone libjpeg-turbo3.1.4.1**, quality69/ISLOW/optimal Huffman
with exact owned quantizers. Its historical native preparation annotation remains
unchanged. Production stays safe managed C#/.NET10 without dependencies.

Complete operations include source normalization/alpha flattening, color sampling,
transforms, histogram/Huffman preparation, entropy coding, fresh MemoryStream
growth and independent packet acquisition/disposal. Native contexts/private
scratch may be reused; every packet is newly encoded and owned. Startup, I/O,
IPC, preparation and validation are excluded. Native allocations are unmeasured;
managed B/op includes harness amortization. Matched quality does not establish
compression-efficiency parity. Every frozen packet/hash/size, decoded sample,
exact alpha/source/owners and ≤0.25dB source-quality gate passes unchanged.

The current six-second profile has35.3% of5,906 samples in AppendAdaptiveAc.
This is attribution, not a timing claim. One new branch reuses the existing
fixed encoder's AVX2/BMI2 scan masks to visit only nonzero AC positions. Zero-run
events, amplitudes, histogram counts and range-error order stay exact. Other
hardware retains the original scalar loop. No field, table, helper, heap
workspace, work charge or admission change is added.

Removing only this branch restores the **entire** predecessor JpegEncoder file
byte for byte. Fixed coding, sampling, transforms and the writer are unchanged.
All **199 other comparisons and58 prior passing proofs are reused**. Exactly
eight adaptive rows replace their older independent results, including grayscale.

One immutable candidate/harness/native/fixture build serves all eight screens.
The first mean is read before controls in batches of at most three. Every mean
remains materially above parity, so no twelve-actual confirmation is run.
The existing dynamic selection driver is reused without rebuilding. No new
performance workload, raster or quality/settings grid is added.

All **3,650 Release tests per OS** pass (1,344 internal,1,408 public,898 live
FFmpeg), plus20 scalar adaptive checks per OS. Two new authored facts compare
every scan position, signed magnitude, sparse/dense layout and invalid range
against independent scalar events, histograms and partial failures. The same
actual private Release package passes its consumer and all three examples on
Windows/Linux; symbols match and Linux production bytes stay identical through
qualification. Unaffected scalar proofs and the published0.1.0 package/tag remain.

[Numeric evidence](warm-jpeg-adaptive-ac-evidence.json) retains statistics,
quality/sizes, receipts, identities and qualification. Raw archive
`parity-p25-qualified-evidence.tar.gz`, SHA256
`a63a0af7039d58471beb4d133adcfa76a6c44edaea71d63a584772477463dc5a`.
