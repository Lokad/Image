# Warm RGBA16 JPEG420 adaptive Huffman coding

Adaptive coding independently confirms **1.888× FFmpeg**, conservative upper
**1.931×**, averaging 3,481.14 µs with **297,772 managed bytes allocated per
operation**. Compared with the restored fixed-table screen, the mean decreases
**7.8%** and allocations **43.5%**. Both quietness guards pass; all 12 actual
results are retained. Stop tuning this fixed pair. Uniform warm parity and the
remaining JPEG420 RGBA8 upper-bound gap stay open.

| Stage | Lokad µs | Ratio / upper | Managed allocated B/op |
| --- | ---: | ---: | ---: |
| Restored fixed-table screen | 3,775.72 | 2.046 / 3.240 | 527,306 |
| Adaptive tables, separate code/amplitude screen | 3,733.52 | 2.033 / 2.759 | 297,772 |
| Combined-symbol screen | 3,425.62 | 1.869 / 2.034 | 297,772 |
| Corrected admission screen | 3,474.09 | 1.889 / 2.036 | 297,772 |
| Final independent confirmation | 3,481.14 | 1.888 / 1.931 | 297,772 |

The same original 513×259 RGBA16 raster, quality 69, native qscale 5 and
background RGB 0x204060 remain fixed. Complete blending/normalization, sampling,
transform, table construction, encoding and independent owned output release
are timed. Quantized coefficients, decoded samples, alpha and the 0.105942 dB
quality gap remain exact. Managed packets intentionally change from 147,730 to
74,056 bytes; native packets remain 52,117 bytes. This is the declared
source-quality comparison, not compression efficiency parity.

Original per-image canonical tables are limited to 16 bits and reserve the
all-ones code. Sampling and transforms run once; packed entropy events are
cached and emitted with the new tables. Small images, cancellable operations,
caller-controlled streams and tight work/memory budgets retain the fixed path.
The cache's full 861,696-byte capacity and 64 KiB table/scratch allowance are
admitted for this case. Private per-thread scratch is leased and recomputed,
with at most 4 MiB retained; it never caches an output packet. Combined symbols
retain both logical work charges. Exact output quotas use the same adaptive
packet with per-code writes; lowering a quota does not select a larger packet.

The initial integration exposed output-quota mode selection and historical
fixed-table assumptions in full public validation. Those failures remain
recorded. Corrected checks retain the original 160-case fixed quality report
unchanged and verify current quantization and exact decoded samples. Final
validation passes **2,767 tests per OS** (993 internal, 878 public, 896 live
differential), **39 focused entropy/admission checks** with intrinsics enabled
and disabled, and all three examples through the **same actual Release package**
on Windows/Linux. Eighteen new checks cover all sampling modes/depths, restart
edges, canonical tables, constrained fallback, exact quotas and held outputs.

Only encoder routing changes among existing methods; the other 600 bodies and
all 601 existing implementation flags match. Fresh builds and actual generated
children match all 629 candidate package methods and flags. The original fixed
sampling/transform/entropy path is retained. Sequential Release runs use one
pinned CPU, one native codec/conversion thread, normal SIMD, tiering off and
workstation nonconcurrent GC. Contexts and scratch may be reused; pixels and
encoded output are recomputed. Startup, I/O, IPC and checks are excluded. Native
allocations are unmeasured.

The [numeric evidence](warm-jpeg-adaptive-evidence.json) retains all intermediate
screens, confirmations, source/package identities and final validation. All four
actual child files and raw/source/package evidence were frozen before subsequent
preparation or mutation. The [benchmark summary](../docs/benchmark.md) selects
the final confirmed result. Revalidation of other shared JPEG encoder cases is
still required; earlier rolling results do not establish final-source parity.
