# Certified subsampled JPEG color reconstruction

Six existing complete JPEG decodes improve **7.3–16.4%** with exactly unchanged
pixels. Their means remain above parity; these are qualified short results.

| Existing decode | Lokad / FFmpeg µs | Ratio / upper | Managed B/op | JPEG input B |
| --- | ---: | ---: | ---: | ---: |
| 4:2:0 restart / fresh Stream | 1,370.98 / 993.98 | 1.379 / 1.694 | 951,080 | 58,012 |
| 4:2:0 restart / resident | 1,366.40 / 1,014.67 | 1.347 / 1.490 | 761,720 | 58,012 |
| Baseline 4:2:0 | 1,204.44 / 1,009.50 | 1.193 / 1.338 | 757,345 | 52,093 |
| Baseline 4:2:2 | 1,573.59 / 1,344.77 | 1.170 / 1.322 | 829,145 | 71,168 |
| Progressive 4:2:0 | 2,425.66 / 1,949.91 | 1.244 / 1.404 | 775,336 | 52,364 |
| Progressive 4:2:2 | 3,075.44 / 2,246.89 | 1.369 / 1.839 | 847,162 | 71,251 |

The safe sixteen-pixel interior kernel forms exact integer sixteenth chroma numerators,
then applies the original color matrix in float. Against the original binary64
evaluation, coefficient error contributes <2^-16 each; at most four float operations
contribute≤ 2^-16 each, with intermediate magnitudes < 512. Including binary64 error,
2^-14 + 2^-15 + 2^-40 < 2^-13 bounds every channel. Clipping is nonexpansive. A block is
written only when every channel is farther than 2^-13 from a rounding half.
Otherwise two unchanged eight-pixel binary64 batches handle it before any output write.
Exact-half and independent binary64 oracle tests exercise the certificate and
fallback. Scalar edges/tails and other hardware keep their original bodies.
There is no new heap array, table, field, reservation or logical work charge;
source preservation, ownership, per-row work, cancellation and admission remain.

Both sides time complete decoding, conversion and independent output acquisition
and release, with the original 513×259 authored inputs. Fresh Stream includes a new
non-exposed input stream, actual acquisition/copying and EOF validation; native
includes matching AVIO/owned input acquisition. FFmpeg performs full-range BT.601,
ceil-sized chroma expansion and crop within each operation. All frozen packet and
managed pixel hashes pass, as do RGB maximum 4/RMSE 1.5 and exact opaque alpha gates.
Compressed output is inapplicable to decoding; frozen input sizes appear above.
No encoder or packet changes are included. Native allocation is unmeasured;
managed B/op includes varying harness amortization.

One CPU, normal SIMD, tiering off, nonconcurrent workstation GC. Six screens with
three 200ms actuals each, 50ms native pre/post samples and both 5s idle-v2 guards
retain all 18 actuals and outliers.
Upper is the larger selected/all-actual bound against the conservative native
lower bracket. All means remain > 1, so no long confirmation was warranted.

All **3,724 Release tests per OS** pass: 1,390 internal, 1,436 public and 898 live FFmpeg.
Focused checks per OS include 26 internal and 63 public tests, with 8,192 authored color blocks and the known
exact-half rejection; 26 focused hardware-off checks per OS also pass. Prior 49 scalar
JPEG controls per OS and unaffected qualification remain applicable. The **same actual
private Release package** passes its consumer and all three examples on both
Windows and Linux. Measured production bytes remain unchanged through qualification.

[Numeric evidence](warm-jpeg-color16-evidence.json) retains all actuals, guards,
source reconciliation, frozen references and qualification hashes. Exactly six
selected timings change; 201 comparisons and all 62 passing proofs retain their paths.
No new image/workload/settings grid. The 207-case objective remains open at 62 passes,
144 mean gaps and one confidence case. No color lane/margin/inlining grid follows.
