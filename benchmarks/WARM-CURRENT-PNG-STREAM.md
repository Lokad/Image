# Current PNG Stream evidence

All three named PNG Stream uncertainties independently confirm on current
production paths. Their older timings preceded fused framing/inflation. The
later word-composition proof includes fusion and remains valid. Stop all four
fixed pairs; no production optimization is needed.

| Fixed Stream case | Lokad.Image µs | FFmpeg µs | Ratio / upper | Managed B/op | Actual / selected | Verification |
| --- | ---: | ---: | ---: | ---: | ---: | --- |
| Png8 | 1,982.59 | 1,388.00 | 1.428 / 1.441 | 748,088 | 12 / 11 | Current confirmation |
| PngCropFit8 | 2,129.50 | 1,473.35 | 1.445 / 1.454 | 1,445,000 | 12 / 11 | Current confirmation |
| Png16 | 3,499.10 | 3,080.71 | 1.136 / 1.146 | 3,067,227 | 12 / 12 | Current confirmation |
| PngCropFit16 | 3,572.35 | 2,936.91 | 1.216 / 1.231 | 4,460,393 | 12 / 11 | Reused unchanged path |

Every operation starts with a fresh read-only seekable MemoryStream whose
backing buffer is not exposed. Complete public Decode(Stream) reads, buffering,
input copies, full decode, independent output ownership and input/output
disposal are timed. The encoded PNG is resident; startup, file/network I/O,
IPC, generation, hashing and validation are excluded. No decoded-output cache.

FFmpeg's equivalent operation times fresh public AVIO acquisition, 8192-byte
scratch, length query, complete av_get_packet acquisition into a fresh owned
padded packet, EOF checking, full PNG decode, independent output acquisition
and release. Codec/frame/packet wrappers are warm; each payload is reacquired.
Plain PNG16 times complete BE-to-LE conversion into fresh owned native-endian
RGBA output. Byte decoding requires no conversion thread.

Composition additionally times AutoOrient(Normal), centered 256x129 crop and
nearest ResizeToFit(128,64,false), producing 127x64. Every managed intermediate
owner is acquired/materialized/disposed. Native crop/point graphs may keep
intermediate views; final output is independently owned. Word composition keeps
RGBA64BE through crop, then times its public graph's selected-word conversion/
packing into exact RGBA64LE. No artificial full-frame endian copy is added.

Frozen packet/original raster/final output hashes, exact complete RGBA samples
(including alpha/hidden RGB), precision/geometry, source preservation, EOF,
caller stream remaining open and simultaneously held independent outputs pass.
Only the existing two original 513x259 byte/word rasters and existing fixed
composition are used. No image, geometry, depth, filter or quality grid is added.

Three-actual screens precede independent twelve-actual confirmations on the
same immutable production/harness/native builds. All twelve actuals are kept;
conservative uppers include selected/all-actual intervals against the faster
native bracket's lower confidence bound. Both versioned quiet guards pass each
scored run. Sequential Release uses CPU0, one codec/conversion/filter thread,
normal SIMD, tiering disabled and workstation nonconcurrent GC. Managed
allocations include harness costs; native allocation totals are unmeasured.

The only code change is benchmark-only parsing of one-to-three distinct Stream
selectors. Prefix/suffix comparison verifies Setup and the complete measured
operation are unchanged. One harness build and two shared three-case selection
drivers replace separate driver builds for each pair; all cases run sequentially.
Every cached child, including WarmDry, receives verified assemblies. Production
and native build counts are zero; all three native worker/source/build hashes
match their prior references. Current production/package qualification is reused:
3,483 Release tests per OS, including 898 live comparisons, and the consumer/all
three examples through the same actual Release package on both OS. No repeated
full suite, package proof, profile or whole-library audit is introduced.

PngCropFit16's actual timed production files match the accepted fusion commit;
the retained source archive, not its earlier parent revision, establishes this.
Its timed DLL matches that round's full qualification. Its 960,565-byte encoded packet
is below the earlier one-MiB input-cache ceiling; later counting changes affect
nonmaterializing paths. Its selected Stream/composition/nearest paths remain
unchanged. [Numeric evidence](warm-current-png-stream-evidence.json) records the
exact witness/source/archive hash, all new samples, allocations, guards,
immutable assemblies and qualification reuse. This closes named B07 fixed-input
scope; the full required warm objective remains open. Next: named PNG encodes.
