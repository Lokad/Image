# Fresh VP8L Stream decoding

Complete fresh-stream VP8L decoding independently confirms **1.834x FFmpeg /
conservative upper 1.851x**, with **1,924,876 managed B/op**. Every RGBA sample,
including alpha and hidden RGB, is exact. Production is unchanged.

| Measurement | Lokad.Image µs | FFmpeg µs | Ratio / upper | Managed B/op |
| --- | ---: | ---: | ---: | ---: |
| Three-actual screen | 5,879.72 | 3,203.66 | 1.835 / 4.075 | 1,924,876 |
| Independent twelve-actual confirmation | 5,549.36 | 3,026.08 | 1.834 / 1.851 | 1,924,876 |

Each operation creates a fresh read-only seekable MemoryStream with a non-exposed
backing buffer, calls public `ImageCodec.Decode(Stream)` and disposes the input
and independently owned output. All reading, buffering, copies and disposal are
timed. The frozen native VP8L input is **183,812 bytes**, SHA-256
`08ddac115f1770d778e6dbc76373440224b32e6f93b616605e234c63206eebde`.
It reuses the original deterministic 513x259 RGBA8 raster and exact libwebp
encoder preparation; no new image, encoding or settings search is introduced.

The native operation creates a fresh public AVIO context and 8,192-byte scratch,
queries input length, reads into a fresh owned padded packet and checks EOF.
It resets the software decoder, decodes all pixels, allocates independent RGBA
output, converts ARGB to RGBA and releases input/scratch/output. All this work is
timed. Codec/conversion contexts and packet/frame wrappers are reused. Resident
source bytes are reused; there is no prepared stream packet or decoded output
cache. No demux stage is added to either complete-image decode boundary.

Before and after timing, validation checks exact normal/scalar native CLI pixels,
every managed sample, geometry/orientation, source preservation, complete input
consumption, caller lifetime and two simultaneously live independent outputs.
Sequential Release runs use one pinned CPU, one codec/conversion thread, normal
SIMD, disabled tiering and workstation nonconcurrent GC. Both quiet guards pass
on both runs. All twelve confirmation actuals remain selected and retained. The
upper takes the larger filtered/all-actual bound against the faster native
bracket's lower bound. Startup, file I/O, IPC, hashing and validation are excluded.
Native allocations are **unmeasured**; managed figures include harness overhead.

The same verified immutable production DLL and harness serve both runs. Only
two benchmark inputs and the original native adapter/build script change.
Existing **3,371 Release tests per OS**, including **898 live comparisons**, and
the same actual package's consumer plus all three examples are reused. No
production rebuild, full suite, package proof or method audit is repeated.
The initial benchmark dispatch refusal remains retained as preparation evidence.

[Numeric evidence](warm-vp8lstream-evidence.json) retains all samples, allocations,
guards, actual assemblies/jobs, source inventories and qualification reuse.
Stop this confirmed pair; remaining material public/codec coverage and unmatched
policies stay open.
