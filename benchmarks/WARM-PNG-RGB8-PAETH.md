# RGB8 Paeth decoding

Complete RGB8 Paeth decoding independently confirms **1.204x FFmpeg /
conservative upper 1.214x**, at **3,432 managed B/op**. Original RGB and opaque
alpha are exact. Production is unchanged; no speedup is claimed.

| Quiet observation | Lokad.Image µs | FFmpeg µs | Ratio / upper | Managed B/op |
| --- | ---: | ---: | ---: | ---: |
| Three-actual screen | 3,153.36 | 2,606.45 | 1.210 / 1.399 | 3,432 |
| Independent twelve-actual confirmation | 3,140.91 | 2,608.88 | 1.204 / 1.214 | 3,432 |

The original 513x259 RGBA8 raster's existing opaque RGB projection is reused:
all RGB bytes remain unchanged and alpha becomes255. Original authoring follows
[PNG section 9.4, Paeth](https://www.w3.org/TR/png-3/) with a three-byte stride,
specified tie order, modulo256 byte differences and zero top/left boundaries.
Every row uses Paeth, with BCL Optimal level6. No image, geometry, quality or
effort enumeration is introduced; the total remains two original performance rasters.

Both subjects time complete decoding of identical resident input, reconstruction,
channel expansion and independent output acquisition/release. Native codec/input
packet/frame wrappers are reused; complete decoding, RGB24-to-RGBA conversion and
second owned output acquisition/release repeat every operation. No decoded output
is cached. Normal/scalar native CLI pixels, geometry/orientation, source preservation
and two live owners pass outside timing. Startup, I/O, IPC, authoring and validation
are excluded. Native allocations are **unmeasured**; managed figures include harness overhead.

Sequential Release runs use one pinned CPU, one codec/conversion thread, normal
SIMD, disabled tiering and workstation nonconcurrent GC. Both versioned quiet
guards pass on both runs. All twelve confirmation actuals are retained;
BenchmarkDotNet selects ten. The upper includes selected and all-actual intervals
against the faster native bracket's lower bound. Both runs use the same immutable
production DLL and harness; the native worker is unchanged.

Only two benchmark inputs change. Existing **3,373 Release tests per OS**, including
**898 live comparisons**, and the same actual package's consumer plus all three
examples on Windows/Linux are reused from the [streaming round](WARM-PNG-FAST2.md).
No production rebuild, profile, full suite, package proof or method audit is repeated.

[Numeric evidence](warm-pngrgbpaeth8-evidence.json) retains every actual, allocation,
guard, source/assembly/job identity and qualification reuse. Stop this confirmed
pair; remaining material workloads and unmatched reference cases stay open.
