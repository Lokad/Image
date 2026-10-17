# Single-component grayscale JPEG encoding

Historical timing checkpoint: its adaptive encode rows are superseded by
[nonzero AC planning](WARM-JPEG-ADAPTIVE-AC.md). The original packet, quality,
approved reference and complete operation boundaries remain unchanged.

Lokad.Image independently confirms **1.388× SIMD-enabled standalone libjpeg-turbo**,
conservative upper **1.398×**, at **154,893 allocated B/op**. This is the approved
grayscale benchmark-tooling exception: FFmpeg's MJPEG encoder emits three
components. Production remains safe managed code, dependency-free and without
native bindings. No production optimization is needed for this pair.

| Quiet observation | Lokad.Image µs | libjpeg-turbo µs | Ratio / upper | Managed B/op | Compressed bytes, managed / reference |
| --- | ---: | ---: | ---: | ---: | ---: |
| Three-actual screen | 1,969.01 | 1,437.48 | 1.370 / 3.380 | 154,893 | 58,823 / 58,910 |
| Independent twelve-actual confirmation | 1,958.56 | 1,411.27 | 1.388 / 1.398 | 154,893 | 58,823 / 58,910 |

Both subjects encode the existing original 513×259 RGBA8 raster into one
component, using the same owned mathematical quality69 quantizers, adaptive/
optimal Huffman coding and no restart markers. Alpha is flattened once onto
RGB0x204060, then converted to full-range BT.601 grayscale. Every operation
includes normalization, transforms, histogram/table construction, entropy coding
and fresh output acquisition/release. Managed MemoryStream creation, growth and
disposal are included. Native codec/gray scratch are warm, but every source
sample is normalized and encoded again and every packet is newly allocated and
freed. Startup, I/O, IPC, fixture creation and validation are excluded.

The frozen source-quality PSNR gap is **0.000033 dB**, within the unchanged
0.25 dB gate. FFmpeg remains the decoder oracle: public/native decoder agreement
stays within max4/RMSE1.5, with exact opaque alpha. Normal/scalar native outputs
match exactly. The managed packet stays **58,823 bytes**, SHA-256
`2442100fb89c3000f4646e8ae9c5e25b45980dd2d9e1be3d38f38e69f9a32cea`;
the reference packet stays **58,910 bytes**, SHA-256
`444e39e6126dd3077d22c0b4f60e38d8a22611fa5721b226c5168a63b7e94423`.
Frame components and quantization tables match exactly; source preservation,
simultaneously held independent outputs and corrupted-reference rejection pass.

The prepared worker and fixtures are reused. libjpeg-turbo3.1.4.1 is pinned at
`9217719d3a58633923b096af4c1d50d304768a64`, built Release/static with SIMD enabled
and AVX2 ISLOW DCT support. The raw worker retains its historical preapproval
annotation; accepted reference policy is recorded separately. The preparation
record preserves its actual build driver and the public reproduction recipe.
Neither the worker nor production DLL is rebuilt for this experiment. One
changed benchmark DLL and necessary cached selection drivers suffice.

Both versioned quiet guards pass on both runs. Sequential Release uses CPU0,
one codec/conversion thread, disabled tiering and workstation nonconcurrent GC.
The short mean passes; its wide interval is not certification. Confirmation
retains all twelve actuals, selects eleven, and takes the larger selected/
all-actual upper against the faster native bracket's lower confidence bound.
Native allocation totals are unmeasured; managed figures include harness costs.

Production/package inputs match the existing
[JPEG AC planning qualification](WARM-JPEG-AC-PLANNING.md): 3,483 Release tests per
OS, including 898 live comparisons, and the same actual package's consumer plus
all three examples on Windows/Linux. Those valid proofs are reused. No new
original image, quality grid, profile, production rebuild or repeated package
proof is introduced. The [numeric evidence](warm-jpeg-grayscale-evidence.json)
retains all samples, allocations, sizes, guards, identities and qualification.
Stop this confirmed pair; the bounded remaining-coverage audit is recorded in
[material coverage](WARM-COVERAGE.md).
