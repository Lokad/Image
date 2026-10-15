# Packed gray1 Adam7 decoding

Complete gray1 Adam7 decoding independently confirms **1.018x FFmpeg /
conservative upper 1.032x**, at **1,424 managed B/op**. Every projected RGB and
opaque-alpha sample remains exact. Production is unchanged; no speedup is claimed.

| Quiet observation | Lokad.Image µs | FFmpeg µs | Ratio / upper | Managed B/op |
| --- | ---: | ---: | ---: | ---: |
| Three-actual screen | 1,265.37 | 1,247.08 | 1.015 / 1.070 | 1,424 |
| Independent twelve-actual confirmation | 1,265.54 | 1,243.33 | 1.018 / 1.032 | 1,424 |

The existing 513x259 byte raster's black/white projection is reused: the original
red high bit selects0/255, all RGB channels equal it, and alpha becomes255.
Original authoring follows [PNG Adam7 and scanline packing](https://www.w3.org/TR/png-3/):
seven specified passes, MSB-first bits, zero pass padding, filter None and BCL
Optimal level6. Every pass sample and padding bit is validated. No image, geometry,
filter or effort enumeration is introduced; the total remains two original rasters.

Both subjects time complete identical-input decoding, all passes, bit expansion,
scatter and independent output acquisition/release. Native codec/input packet/frame
wrappers are reused; full decoding, MONOBLACK-to-RGBA conversion and second owned
output acquisition/release repeat every operation. No decoded output is cached.
Normal/scalar native CLI pixels, geometry/orientation, source preservation and two
live owners pass outside timing. Startup, I/O, IPC, authoring and validation are
excluded. Native allocations are **unmeasured**; managed figures include harness overhead.

Sequential Release runs use one pinned CPU, one codec/conversion thread, normal
SIMD, disabled tiering and workstation nonconcurrent GC. Both versioned quiet
guards pass on both runs. All twelve confirmation actuals are retained;
BenchmarkDotNet selects ten. The upper includes selected and all-actual intervals
against the faster native bracket's lower bound. Both runs use the same immutable
production DLL, harness and native worker.

The initial adapter refused interlaced grayscale before measurement. Only its
existing one-bit path's admission guard changes; other interlaced gray profiles
remain excluded from this adapter. Corrected preparation reuses the unchanged
managed harness and generated short child. The refusal remains unscored and retained.

Only two benchmark inputs and one native adapter guard change. Existing **3,373
Release tests per OS**, including **898 live comparisons**, and the same actual
package's consumer plus all three examples on Windows/Linux are reused from the
[streaming round](WARM-PNG-FAST2.md). No production rebuild, profile, full suite,
package proof or method audit is repeated.

[Numeric evidence](warm-pnggray1adam7-evidence.json) retains all actuals, allocations,
guards, source/assembly/job/native identities, preparation refusal and qualification
reuse. Stop this confirmed pair; other material workloads and unmatched references stay open.
