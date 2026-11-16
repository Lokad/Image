# Complete JPEG and WebP header references

Current production costs are in [JPEG stuffed-pair acquisition](JPEG-STUFFED-PAIRS.md).
The measurements below retain the pre-change baseline and reference qualification.

The two previously unconfirmed original boundaries now have independent current
measurements. **JPEG is a demonstrated gap;WebP passes and stops.** These are
complete `InspectHeaders(Stream)` operations,not dimension-only probes or pixel
decoding. No production change or speedup is claimed.

| Original complete boundary | Lokad / reference µs | Ratio interval / status | Managed GC B/op |
| --- | ---: | --- | ---: |
| JPEG420 fresh stream,65,163 B | 5.566 / 2.816 | 1.951798–1.994260 / gap | 8,768 |
| Lossless RGBA WebP fresh seekable stream,533,100 B | .131984 / .296087 | .434391–.456690 / pass-stop | 552 |

The reference is explicitly **FFmpeg n8.1 AVIO/owned descriptor APIs plus an
original bounded structural/EXIF adapter**,not public packet-parser performance.
FFmpeg's MJPEG/WebP packet parsers can accumulate complete packets;complete-frame
mode requires an available packet. Those boundaries do not qualify fresh opaque
stream inspection with bounded workspace. This uses the existing PNG-reference
approach;no upstream implementation/tables/corpus are copied.

Both subjects acquire and release a fresh non-exposed seekable source and bounded
working state,scan complete framing/late metadata/EOF and create an independent
nine-field source descriptor. Inputs are borrowed resident packets,never copied
whole peroperation. Neither subject decodes compressed pixels or caches a result.
Output survives source/sibling release;native parameters also survive AVIO disposal.
All necessary source/workspace/descriptor acquisition and release remain timed.
The existing65,163-byte JPEG is exactly the JGD1093B complete-header guard input;
WebP is the original533,100-byte lossless fixture. No new image/settings grid.

Each independent confirmation retains12actuals,alternating order and three native
brackets;JPEG16,384/WebP131,072 complete operations per actual,two clocks per batch.
CPU0,optimized JIT,natural nonconcurrent workstation GC and idle-v2 guard apply.
All-actual two-sided99.9%Student-t/paired-log confidence includes managed upper
mean versus the fastest native lower bracket. No filtering or favorable repeat.
Original short screens remain:JPEG6.029/2.787µs,upper2.879609;WebP.130976/.287721µs,
upper.954957. These are not historical-clock gains.

Twenty original same-packet controls pass on both implementations:late EXIF and
Adobe color declarations,EOF/truncation/trailing bytes,marker/RIFF/padding/feature
errors and opaque entropy/image data that this API must not decode. Frozen controls
also pass Windows/Linux normal/scalar with the SAME actual timed/package DLL,
exact typed fields,independent surviving outputs,borrowed input,cancellation,
refusal and recovery. Reuse unchanged production's6,745full/live checks eachOS
and actual package/all three examples. One managed-tool and one native-tool build;
zero production,embedded,public,existing-harness,Windows,fixture or package builds.

GC allocation is8,768/552B per JPEG/WebP operation. The earlier8,776B scoped
JPEG driver uses a different stream wrapper;this is not a production allocation
reduction. Managed admission remains
512B initial/8,448B bulk maximum. Native uses8,192B AVIO scratch+8,192B adapter
staging+32B header,excluding contexts/descriptors;this is not a measured total
peak. Reader storage ends with the operation;small independent metadata outputs
retain their normal ownership. Borrowed input,allocation,admitted capacity,
retention and unmeasured physical/native/JIT peaks remain distinct.

A single two-second complete-JPEG profile retains1,991locations/832mapped:
marker acquisition290,async-method setup238,byte reads140,outer JPEG driver113.
The packet contains191raw`FF00`pairs. These counts locate a lead,not phase costs
or removable-time estimates;all instrumented clocks are discarded. Next test:
consume stuffed pairs within marker acquisition and avoid repeated builder/driver
handoffs,with one grammar and unchanged read/cancel/restart/failure/owner behavior.
This complete-entropy path differs from earlier small prefix-probe trials.

[Evidence](bounded-headers-current-reference-evidence.json) pins source/reference,
worker/packet identities,all actuals,controls,selected profile and qualification
reuse. Raw `tmp/bounded-headers-current-reference.tar.xz`, SHA-256
`fb208712fb2eb6e26c6e0def6c7b18815a6a243036c516b33360b7a07e4a132d`,
721,524B/101members,is mirrored/member-verified;current core/package are a pinned
dependency. Build only tools with `build-bounded-headers-linux.sh`;run
`verify-bounded-headers.py` before `measure-bounded-headers.py` on the quiet VM.
Their CLI options select existing packets and exact expected metadata. Publication
stays postponed;all289required paths and prior adverse evidence remain.
