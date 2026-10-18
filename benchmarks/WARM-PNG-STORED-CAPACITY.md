# Stored PNG output capacity

Updated 2026-10-18. Complete stored RGBA16 PNG encoding is **48.1% faster** and
independently confirms **0.861× FFmpeg / conservative upper0.870×**. Allocation
falls **72.9%**, from4,202,998 to1,140,504B/op. Stored RGBA8 improves21.0% with
71.3% less allocation, but remains a qualified short gap at1.295×/upper1.430×.

| Existing case / job | Lokad.Image µs | FFmpeg µs | Ratio / upper | Managed B/op | PNG bytes managed/native |
| --- | ---: | ---: | ---: | ---: | ---: |
| RGBA16 / three-actual screen | 349.20 | 407.91 | 0.856 / 0.963 | 1,140,504 | 1,063,615/1,066,552 |
| RGBA16 / independent twelve-actual confirmation | 349.92 | 406.43 | 0.861 / 0.870 | 1,140,504 | 1,063,615/1,066,552 |
| RGBA8 / three-actual screen | 245.37 | 189.47 | 1.295 / 1.430 | 604,319 | 531,971/533,444 |

Both original513×259 rasters, source quality and exact managed/native packets
remain unchanged. Full public EncodePng includes fresh MemoryStream creation,
capacity allocation/growth, encoding, owned output and disposal; no redundant
ToArray is timed. Native encoding includes its existing packing/endian/filter
boundaries, owned packet acquisition and release. Capacity is allocated inside
production, never pre-sized by the benchmark. Context/private storage reuse
retains the original ownership rules. Native allocations are unmeasured.

After original workspace admission and full remaining row-work/output preflight,
large stored RGBA8/16 encodes can reserve a plain MemoryStream's capacity once.
The size estimate is unrelated to logical output admission: original quota checks,
counters, row packing, BCL NoCompression, CRC/Adler, chunks and every write remain.
Capacity never shrinks. Custom streams and live cancellation tokens retain the
original path. Fixed storage's growth refusal preserves the original write
failure; read-only/disposed, nonzero-position, work/output limits and partial
prefixes retain their behavior. No dependency, unsafe code, native binding,
compressor replacement or extra codec workspace is introduced.

Only the two named stored encodes change. Other14named PNG encodes use different
compression/color paths; JPEG stream methods and PNG decoders are unchanged.
All205 other comparisons and59preceding passing proofs remain applicable. The
finite207-case scope now has **60parity passes,143gaps,4confidence-open**;145selected
independent confirmations and62qualified short screens. Stop stored RGBA16.
Stored RGBA8 receives no routine twelve-actual repeat while materially above1×.
No input/shape/depth/filter/quality grid was added.

One six-second pre-change user-mode profile identified copying/allocation as a
plausible large cost alongside the4.2MB allocation for a1.06MB packet. Its1801
samples exclude kernel work and partly unresolved native/R2R frames; fractions
are not wall-time shares. The existing complete-operation profile driver was
reused; no second profile or CRC microtuning. A single producer build perOS served
all three statistical jobs. Existing generic short/twelve-actual selectors,
harness, approved byte/dedicated-word workers and frozen fixtures were reused.

Jobs run sequentially on one pinned CPU with normal SIMD, tiering off and
workstation nonconcurrent GC. Allthree screen actuals and alltwelve confirmation
actuals are selected/retained, with both exclusive-vm-idle-v2 guards and native
brackets. Exact samples/alpha/hiddenRGB, normal/scalar CLI decoding, stored blocks,
unchanged5%IDAT-size gate, source preservation and live independent owners pass.

**3,701 Release tests perOS** pass (1,374internal+1,429public+898liveFFmpeg), plus
21scalar stream/capacity and44scalar public PNG checks perOS. Nine new cases cover
the exact working/work boundary, one-byte-below rejection, ownership, original
packets, stream failures and limit prefixes. The same actual private Release
package passes its isolated consumer and allthree usage examples on Windows/Linux.
Screened producer bytes and the measured frozen harness are preserved; the
published0.1.0 packages/tag remain intact.

[Numeric evidence](warm-png-stored-capacity-evidence.json) retains all statistics,
allocations/packet sizes, immutable identities, quiet guards, profile scope,
precise invalidation and cross-platform/same-package proofs with raw checksums.
