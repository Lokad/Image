# Fresh Stream PNG8 decoding

Fresh seekable Stream decoding independently confirms **1.907x FFmpeg / conservative
upper 1.916x**, with **756,605 allocated B/op**. Exact pixels and both quiet guards
pass. Production is unchanged; this fixed public boundary clears the target.
Uniform current-source coverage remains open.

| Measurement | Lokad.Image us | FFmpeg us | Ratio / upper | Allocated B/op |
| --- | ---: | ---: | ---: | ---: |
| Three-actual screen | 2,664.43 | 1,394.54 | 1.911 / 2.121 | 756,605 |
| Independent twelve-actual confirmation | 2,655.37 | 1,392.55 | 1.907 / 1.916 | 756,605 |

Each managed operation creates a fresh read-only seekable `MemoryStream` whose
backing array is not publicly exposed, calls public `ImageCodec.Decode(Stream)`,
then disposes the independently owned result and stream. All reads, buffering,
copies, complete PNG decoding and release are timed. The source is the existing
original 513x259 RGBA8 raster encoded once outside timing; its frozen PNG SHA-256
is `4c0c823a1b84d7619f784ace6c2749ed10c0d54b7dc353beed3ad4888e0af57f`.
No additional original image or profile sweep is introduced.

FFmpeg uses a fresh public AVIO context with 8,192-byte scratch, an in-memory
read/seek callback, a length query and `av_get_packet` to acquire a fresh owned
padded packet on every operation. Acquisition, copies, EOF read, complete PNG
decoding and input/output release are timed. Codec/frame/packet wrappers are
reused; the input payload is reacquired each time, never a prepared input view.
The independent-still key flag prevents inter-frame PNG delta semantics.
The native result exposes owned aligned rows; no packing copy is required.

Validation outside timing compares every RGBA sample, including hidden RGB,
checks source preservation, and holds two results while mutating one to detect
aliasing. Managed validation also checks non-exposed input buffers, complete input
consumption and caller-stream lifetime. Native initialization and final checks
verify complete pixels, source preservation and independent live frame storage.
Initial adapter EOF/key-flag preparation failures are retained and unscored.

Sequential Release runs use one pinned CPU, one codec thread, normal SIMD,
tiering disabled and workstation nonconcurrent GC. The versioned
`exclusive-vm-idle-v2` guard passes before and after each run. All twelve
confirmation actuals remain retained and selected; the bound takes the larger
filtered/all-actual interval against the faster native bracket's lower bound.
Startup, file/network I/O, IPC, fixture creation, full-input comparisons and
hashing are excluded. Native allocations are unmeasured; managed figures include
harness overhead.

The same verified immutable production DLL and harness serve both runs. Owned
source inventories differ from the accepted build only in six benchmark inputs.
The existing **3,289 Release tests per OS**, including **896 live FFmpeg cases**,
and the consumer plus three examples through the same actual Release package
remain applicable. No repeated production build, full suite, package proof or
whole-library method audit is required for this benchmark-only addition.

The [numeric evidence](warm-stream-png8-evidence.json) retains raw samples,
allocations, guards, generated jobs, source/assembly identities and qualification
reuse. Stop this confirmed pair; remaining material public/codec/current-source
coverage stays open.
