# Fresh Stream PNG16 decoding

Fresh seekable PNG16 Stream decoding independently confirms **1.819x FFmpeg /
conservative upper 1.830x**, with **3,075,772 allocated B/op**. Every 16-bit RGBA
sample is exact, including hidden RGB and alpha. Production is unchanged;
uniform current-source coverage remains open.

| Measurement | Lokad.Image us | FFmpeg us | Ratio / upper | Allocated B/op |
| --- | ---: | ---: | ---: | ---: |
| Three-actual screen | 5,647.02 | 3,077.31 | 1.835 / 1.919 | 3,075,772 |
| Independent twelve-actual confirmation | 5,626.97 | 3,093.68 | 1.819 / 1.830 | 3,075,772 |

Each operation creates a fresh read-only seekable MemoryStream whose backing
array is not publicly exposed, calls public `ImageCodec.Decode(Stream)`, then
releases the independently owned image and stream. All input reads, buffering,
copies, full decoding and disposal are timed. The existing original 513x259
RGBA16 raster and frozen **960,565-byte** PNG are reused; no new original image
or profile search is introduced. Its encoded SHA-256 is
`3e4b64d778c395392451d5331d2a11b541d262e2f39826e1322d22d6f03bb4a8`.

The native operation uses a fresh public AVIO context and 8,192-byte scratch,
queries length and acquires a fresh owned padded packet with an explicit EOF
read. It decodes PNG into RGBA64BE, acquires a fresh owned RGBA64LE frame and
converts every sample to little endian before releasing input and output storage.
All acquisition, copies, conversion and release are timed. Codec/frame/packet
wrappers are reused; the payload is reacquired every operation, never a prepared
input view. Existing eight-bit native sources and binaries remain unchanged.

Before and after timing, validation checks every sample, depth, geometry,
orientation, source preservation, stream EOF/caller lifetime and two-live-output
independence. Mutating one result must leave its sibling unchanged. Sequential
Release measurements use one pinned CPU, one codec/conversion thread, normal
SIMD, disabled tiering and workstation nonconcurrent GC. Both versioned quiet
guards pass for both runs. All twelve confirmation actuals remain selected and
retained; the bound takes the larger filtered/all-actual interval against the
faster native bracket's lower bound. Startup, file/network I/O, IPC, fixture
creation, hashing and validation are excluded. Native allocations are unmeasured;
managed figures include harness overhead.

Both runs reuse the same immutable qualified production DLL and harness. Source
inventories show only four benchmark inputs and earlier rejection documentation
differ from the accepted build. Existing **3,289 Release tests per OS**, including
**896 live FFmpeg comparisons**, and consumer plus three examples through the
same actual Release package are reused. No production rebuild, full suite,
package proof or whole-library method audit is repeated.

The [numeric evidence](warm-stream-png16-evidence.json) retains raw samples,
allocations, generated jobs, guards, source/assembly identities and qualification
reuse. Stop this confirmed pair; other material paths and final current-source
coverage remain open.
