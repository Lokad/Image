# Warm PNG sparse RGBA reconstruction

Complete Adam7 RGBA16 decode improves 20.0% over the
quiet baseline, independently confirming at 1.648x FFmpeg,
conservative upper 1.700x. Both unchanged quiet guards
pass. This fixed pair clears2x; broader required warm coverage remains open.

| Run | Managed mean (microseconds) | Worst native bracket ratio | Conservative upper | Quiet guards |
| --- | ---: | ---: | ---: | --- |
| Quiet baseline | 8206.74 | 2.00934x | 2.07570x | Pass |
| Sparse-copy screen | 6293.36 | 1.55520x | 1.62182x | Pass |
| Independent confirmation | 6567.82 | 1.64833x | 1.69955x | Pass |

The [baseline diagnosis](WARM-PNG-ADAM7-BASELINE.md) attributes about18% of sampled
CPU to generic sparse-pixel conversion and repeated destination access. One
primitive now copies packed four/eight-byte RGBA pixels directly into Adam7
positions with safe MemoryMarshal span reads/writes. RGBA16 swaps adjacent sample
bytes on little-endian hosts while retaining channel order. Sparse holes retain
their prior pass samples. Dense RGBA rows and other PNG color profiles retain
their existing paths. No upstream implementation, table or source is copied.

Only the existing ReadPass body changes (633 to767 IL bytes), plus the new
CopySparseRgbaRow method. The other561 existing production bodies are equivalent
after resolving metadata references; all existing implementation flags are exact.
There is no workspace, allocation or admission change. Existing row charges and
4096-pixel cancellation checkpoints are preserved. Six new internal controls
check sparse holes, guard bytes, channel order, hidden samples, source preservation,
widths around cancellation boundaries and cancellation before the first write.

The same 937,465-byte seven-pass/filter-None/level6 PNG uses the existing 513x259
RGBA16 raster. All sixteen-bit samples, alpha, hidden RGB, source bytes and held
independent outputs remain exact. Normal/scalar owned and FFmpeg CLI outputs
match on both OSes. Public Stream allocations remain5,058,912 bytes per operation.
Exact work minimum17,728,138 and working-memory minimum3,208,161 are unchanged;
one below each fails and the minimum produces exact pixels. Pre-cancellation
remains exact. No raster, geometry, filter, quality or effort sweep is added.

Windows and Linux each pass615 internal,848 public and896 live FFmpeg tests
(2,359 total), plus scalar six-primitive and31-public-PNG controls. All three
usage examples pass on both OSes through the same actual dependency-free Release
package. Fresh, packaged and both generated timing libraries have identical
complete production methods and flags across all563 methods.

The resident-input boundaries, normal SIMD, CPU0, one codec/conversion thread,
thread type zero, tiering off and nonconcurrent workstation GC are unchanged.
One short screen precedes one independent confirmation with12 actual managed
iterations and12 native samples in each bracket. BenchmarkDotNet selects all12;
every actual measurement is also independently included without overhead
subtraction. The conservative upper takes the larger bound: filtered
1.69955x, all-actual
1.69955x. Each view uses its managed
confidence critical value for the native bracket lower bounds. The baseline's
earlier failed post-load guard remains failed.

The [numeric evidence](warm-pngscatter-evidence.json) retains raw actual and
filtered measurements, native brackets, guards, input/source/actual-child/package
identities, semantic method comparison, exact admission controls and immutable
archives. Regenerate with `python benchmarks/render-warm-pngscatter.py`;
`--check` verifies this report. See the [reference recipe](../tools/WarmReference/README.md).
Stop tuning this passing point; cover one material Paeth-filtered RGBA16 case on
the existing raster and rerank.
