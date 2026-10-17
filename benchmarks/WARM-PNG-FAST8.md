# RGBA8 Fastest PNG encoding

Historical <=2x checkpoint. Its PNG encode timings are superseded by the current
[chunk checksum round](WARM-PNG-CHUNK-CRC.md), which refreshes all16 affected
selectors and distinguishes independent parity from qualified short gaps.

Complete RGBA8 Fastest encoding independently confirms **0.707x FFmpeg /
conservative upper 0.712x**, at **527,669 managed B/op**. Full RGBA, alpha and
hidden RGB are exact. Production is unchanged; no speedup is claimed.

| Quiet observation | Lokad.Image µs | FFmpeg µs | Ratio / upper | Managed B/op |
| --- | ---: | ---: | ---: | ---: |
| Three-actual screen | 3,096.95 | 4,356.18 | 0.711 / 0.885 | 527,669 |
| Independent twelve-actual confirmation | 3,077.83 | 4,351.79 | 0.707 / 0.712 | 527,669 |

The original 513x259 RGBA8 raster is reused without projection. Explicit managed
BCL level2/native level1, filter None and the unchanged5% IDAT-size gate apply:
231,372 versus236,066 bytes, a1.988% difference. Normal/scalar native CLI pixels,
full alpha/hidden RGB, source preservation and two live owners pass outside timing.
No image, geometry, quality, filter or effort search is introduced; the total
remains two original performance rasters.

Both subjects time complete encoding and independent output acquisition/release.
Managed fresh MemoryStream creation/growth, row packing, compression, CRCs and
disposal are included; no redundant ToArray is timed. Native codec/source/packet
wrappers are reused, with independent encoded packet acquisition/encoding/release
every operation. No output is cached. Byte depth requires no endian conversion.
Startup, I/O, IPC, fixture preparation and validation are excluded. Native
allocations are **unmeasured**; managed figures include harness overhead.

Sequential Release runs use one pinned CPU, one codec/conversion thread, normal
SIMD, disabled tiering and workstation nonconcurrent GC. Both versioned quiet
guards pass on both runs. All twelve confirmation actuals are retained;
BenchmarkDotNet selects eleven. The upper includes selected and all-actual
intervals against the faster native bracket's lower bound. Both runs use the
same immutable production DLL, harness and native worker.

Only the benchmark selector and native adapter's depth admission/description
change. The existing native RGBA8 path is reused; its legacy fast16 executable
and protocol names remain. Existing **3,373 Release tests per OS**, including
**898 live comparisons**, and the same actual package's consumer plus all three
examples on Windows/Linux are reused from the [streaming round](WARM-PNG-FAST2.md).
No production rebuild, profile, full suite, package proof or method audit is repeated.

[Numeric evidence](warm-pngfast8-evidence.json) retains all actuals, allocations,
guards, source/assembly/job/native identities and qualification reuse. Stop this
confirmed pair; remaining material workloads and unmatched reference cases stay open.
