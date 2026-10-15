# PNG16 Average reconstruction

Complete RGBA16 Average decoding independently confirms **1.344x FFmpeg /
conservative upper 1.357x**, at **8,552 managed B/op**. Full words, alpha and
hidden RGB are exact. Production is unchanged; no tuning or speedup is claimed.

| Quiet observation | Lokad.Image µs | FFmpeg µs | Ratio / upper | Managed B/op |
| --- | ---: | ---: | ---: | ---: |
| Three-actual screen | 5,732.29 | 4,397.25 | 1.304 / 2.433 | 8,552 |
| Independent twelve-actual confirmation | 5,894.17 | 4,384.37 | 1.344 / 1.357 | 8,552 |

The input reuses the original deterministic 513x259 RGBA16 raster. Original
authoring follows [PNG sections 9.2/9.3 Average](https://www.w3.org/TR/png-3/):
convert full words to big-endian bytes, then subtract floor((left+up)/2) modulo
256 with an eight-byte pixel stride, nine-bit sums and zero left/top boundaries.
Every row is Average, compression remains BCL Optimal level6, and byte authoring
retains its original behavior. No image, geometry, quality or effort search is
introduced. The total stays two original performance rasters.

Both subjects time complete identical-input decoding, all prediction/endian
conversion, independently owned output acquisition and release. Native codec/
packet/frame wrappers are reused; every operation decodes all pixels. There is
no decoded-output cache. Normal/scalar native CLI samples, alpha/hidden RGB,
source preservation, geometry/orientation and two live outputs pass before/after
timing. Startup, I/O, IPC, authoring, hashing and validation are excluded.

Sequential Release runs use one pinned CPU, one codec/conversion thread, normal
SIMD, disabled tiering and workstation nonconcurrent GC. Both versioned quiet
guards pass on both runs. All twelve confirmation actuals are retained;
BenchmarkDotNet selects eleven. The upper includes selected and all-actual
intervals against the faster native bracket's lower bound. Native allocations
are **unmeasured**; managed figures include harness overhead.

Both runs reuse the same verified immutable production DLL and harness. Only
two benchmark inputs change; the native worker is unchanged. Existing **3,383
Release tests per OS**, including **898 live comparisons**, and the same actual
package's consumer plus all three examples on Windows/Linux are reused from the
[Sub round](WARM-PNG-SUB16.md). No production rebuild, profile, full suite,
package proof or method audit is repeated.

[Numeric evidence](warm-pngaverage16-evidence.json) retains every actual,
allocation, guard, assembly/job identity, source inventory and qualification
reuse. Stop this confirmed pair; remaining material workloads and unmatched
policies stay open.
