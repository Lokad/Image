# PNG16 Up reconstruction

Complete RGBA16 Up decoding independently confirms **1.844x FFmpeg /
conservative upper 1.864x**, at **8,552 managed B/op**. Full words, alpha and
hidden RGB remain exact. Production is unchanged; no speedup is claimed.

| Quiet observation | Lokad.Image µs | FFmpeg µs | Ratio / upper | Managed B/op |
| --- | ---: | ---: | ---: | ---: |
| Three-actual screen | 3,647.93 | 1,887.84 | 1.932 / 2.175 | 8,552 |
| Independent twelve-actual confirmation | 3,473.99 | 1,883.52 | 1.844 / 1.864 | 8,552 |

Original authoring follows [PNG section 9.2, Up](https://www.w3.org/TR/png-3/):
reuse the original 513x259 RGBA16 raster, convert full words to big-endian bytes,
then subtract the previous row modulo256, with a zero top boundary. Every row
uses Up and compression remains BCL Optimal level6. The existing byte authoring
retains identical behavior. No new image, geometry, setting or strategy search
is introduced; the total remains two original performance rasters.

Both subjects time complete decoding of identical resident input, reconstruction,
endian conversion and independent output acquisition/release. Native codec,
packet and frame wrappers are reused; complete decoding repeats every operation.
No decoded output is cached. Normal/scalar native CLI pixels, full samples,
alpha/hidden RGB, geometry/orientation, source preservation and two simultaneous
owners pass outside timing. Startup, I/O, IPC, authoring and validation are excluded.

Sequential Release runs use one pinned CPU, one codec/conversion thread, normal
SIMD, disabled tiering and workstation nonconcurrent GC. Both versioned quiet
guards pass on both runs. All twelve confirmation actuals are retained;
BenchmarkDotNet selects ten. The upper includes selected and all-actual intervals
against the faster native bracket's lower bound. Native allocations are
**unmeasured**; managed figures include harness overhead.

Only two benchmark inputs change; the production DLL and native worker remain
unchanged. The same verified immutable production/harness serves both runs.
Existing **3,373 Release tests per OS**, including **898 live comparisons**, and
the same actual package's consumer plus all three examples on Windows/Linux are
reused from the [streaming round](WARM-PNG-FAST2.md). No production rebuild,
profile, full suite, package proof or method audit is repeated.

[Numeric evidence](warm-pngup16-evidence.json) retains every actual, allocation,
guard, source/assembly/job identity and qualification reuse. Stop this confirmed
pair; remaining material workloads and unmatched reference cases stay open.
