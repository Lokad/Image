# Parallel fixed JPEG fields

Five existing complete fixed-Huffman JPEG encodes improve **14.0–17.0%**, with
unchanged packets, allocations and quality. Their means remain above parity;
these are qualified three-actual results.

| Existing encode | Lokad / FFmpeg µs | Ratio / upper | Managed B/op | Managed / native JPEG B |
| --- | ---: | ---: | ---: | ---: |
| 4:2:0 / forward-only / RGBA8 | 2,310.38 / 1,769.17 | 1.306 / 1.488 | 263,819 | 81,255 / 52,093 |
| 4:4:4 / forward-only | 4,379.70 / 3,778.08 | 1.159 / 1.353 | 526,029 | 170,978 / 133,041 |
| 4:4:4 / live token | 4,465.15 / 3,775.40 | 1.183 / 1.364 | 525,997 | 170,978 / 133,041 |
| 4:2:0 / forward-only / RGBA16 | 2,393.98 / 1,839.60 | 1.301 / 1.415 | 263,819 | 81,275 / 52,117 |
| 4:2:0 / live token / RGBA16 | 2,444.30 / 1,841.37 | 1.327 / 1.443 | 263,787 | 81,275 / 52,117 |

AVX512F/AVX512CD prepares 64 exact integer category/amplitude words in 256 bytes
of fixed stack scratch. The math is already used by the adaptive event planner;
fixed output now uses it before the original occupied-coefficient traversal.
Each invalid value is still reported at its original scan position after prior
zero-run effects, including the original minimum-integer absolute-value overflow.
Codeword/amplitude work and cancellation checks, byte callbacks, stuffing,
quotas, pending/cache state and finally publication retain their boundaries.
Other hardware and short spans keep scalar field preparation. The existing
8 KiB encoder admission covers the fixed payload; no heap array, persistent
field, reservation or logical work charge is added. General/admitted writer
methods, sampling, transforms, quantization and adaptive emission remain unchanged.

The original 513×259 RGBA8/16 rasters, matched fixed quality/background settings
and complete public operations remain. Forward-only cases acquire a fresh wrapper
and owned output; live-token cases include every original cancellation poll.
The prepared FFmpeg software MJPEG reference includes its approved normalization,
conversion and independent packet ownership/release, with one codec/conversion
thread. Frozen packet and per-encoder decoded-pixel hashes remain exact, with the
existing ≤0.25 dB PSNR band and exact alpha. Compressed sizes appear above.
Native allocation is unmeasured; managed B/op includes amortized harness overhead.
No new raster, performance case or quality/settings grid was introduced.

One CPU, normal SIMD, tiering off and nonconcurrent workstation GC. Five short
jobs retain all 15 actuals at 200ms, 50ms native pre/post samples and both 5s
idle-v2 guards. Upper is the larger selected/all-actual bound against the
conservative native lower bracket. All means exceed 1; no long confirmation
was warranted. One immutable producer/harness/selector/reference build served
the screens; all affected means were read before full qualification.

All **3,727 Release tests per OS** pass: 1,392 internal, 1,437 public and 898 live
FFmpeg comparisons. Focused checks per OS comprise 33 internal and 63 public
tests. The 33 internal checks also pass with AVX-512 disabled, retaining the
AVX2/BMI2 fixed traversal and testing scalar field preparation. The new independent
scalar comparison covers all 2,046 valid signed AC magnitudes; existing pending
bits, widths, quotas, work, invalid-value, callback and cancellation matrices
remain. Unaffected scalar evidence is reused. The **same actual private Release
package** passes its consumer and all three examples on Windows and Linux.
Measured production bytes remain unchanged through qualification.

[Numeric evidence](warm-jpeg-fixed-fields-evidence.json) retains all actuals,
guards, exact source reconciliation, references and package hashes. Exactly five
qualified shorts replace their predecessors; 202 comparisons and all 62 passing
proofs retain their paths. Full 207-case parity remains open: 62 passes, 144 mean
gaps and one confidence case, with 124 independent selections and 83 shorts.
No category, amplitude, width, table or inlining variants follow this gain.
