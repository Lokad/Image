# Opaque VP8 encoding from RGBA16

Current production measurements are superseded by [VP8 source conversion](WARM-VP8-SOURCE-VIEWS.md). The retained results below are historical; the source-view change invalidates the four VP8 encode timings, with both alpha paths independently reconfirmed.

Complete word-source encoding independently confirms **1.908x FFmpeg /
conservative upper 1.922x**, at **2,535,736 managed allocated B/op**.
Production is unchanged; uniform current-source coverage remains open.

| Measurement | Lokad.Image ms | FFmpeg ms | Ratio / upper | Allocated B/op |
| --- | ---: | ---: | ---: | ---: |
| Three-actual screen | 29.713 | 15.562 | 1.909 / 1.989 | 2,535,736 |
| Independent twelve-actual confirmation | 29.778 | 15.604 | 1.908 / 1.922 | 2,535,736 |

The existing original 513x259 RGBA16 raster keeps every RGB word and sets alpha
to65535. Managed quality100/native75, both effort4, are fixed with no sweep.
Each operation includes full sample normalization `(sample+128)/257`, repacking,
conversion, complete encoding, independent output acquisition and release.
Every native normalized sample matches the integer reference. The normalized
raster and both encoders' packets, decoded hashes and complete quality samples
equal the [qualified opaque RGBA8 anchor](WARM-VP8-OPAQUE-ENCODE.md).

The unchanged whole-raster RGB source-PSNR gap is **0.025638 dB**, within
**0.25 dB**. Opaque alpha is exact; decoder maximum1/RMSE0.106900 remains inside
maximum4/RMSE1.5. Deterministic packets, source preservation and two independent
live outputs pass before/after timing. Managed output remains333,580 bytes,
native72,380 bytes. This is a source-quality timing comparison; fixed managed
entropy versus adaptive native entropy does not establish size efficiency parity.

The distinct original word adapter preserves existing byte/alpha workers. Native
codec/scratch/frame/packet wrappers may be reused, but normalization and encoding
repeat in full. Fresh managed MemoryStream growth/disposal is timed; no output
is cached. Sequential Release runs use one pinned CPU, one codec thread, normal
SIMD, disabled tiering and workstation nonconcurrent GC. Preparation, I/O, IPC,
validation and startup are excluded. Native allocations are unmeasured; managed
figures include harness/GC bookkeeping.

Both versioned quiet guards pass. All twelve confirmation actuals are retained
and selected; the conservative upper takes the larger filtered/all-actual
interval against the faster native bracket's lower bound. Screen and confirmation
reuse the same verified immutable production DLL/harness. Only three benchmark
files and an original native adapter/build script change. Existing3,308 Release
tests per OS,896 live FFmpeg comparisons and consumer plus three examples through
the same actual Release package are reused. No production rebuild, full suite,
package proof or whole-library method audit repeats.

The [numeric evidence](warm-vp8opaque16-evidence.json) retains every sample,
allocation, guard, actual job/assembly identity, packet and qualification link.
Stop this confirmed pair; remaining material public/codec/current-source
coverage stays open.
