# Direct JPEG coefficient-block emission

**All 18 declared JPEG encoding controls now pass and stop:** the current
row-restart comparison below and 17 unchanged [adaptive controls](JPEG-LOCAL-ENTROPY-REPLAY.md).
This covers their frozen asymmetric 513×259 inputs/settings and declared public
boundaries, not all image content or whole-library parity.

| Complete RGBA8 → 420, restart33 | Lokad / FFmpeg µs | Ratio / upper | GC B/op | Packet B, Lokad / FFmpeg | Status |
| --- | ---: | ---: | ---: | ---: | --- |
| Current immutable package DLL | 1,339.28 / 1,609.15 | 0.832 / 0.841 | 151,180 | 58,010 / 58,012 | Pass |

Two coherent steps remove repeated direct symbol/writer dispatch and per-group
field publication, then compute magnitude and signed amplitude from one sign mask.
Independent complete-operation confirmations measure **19.21% / 395.75 µs** for
the first step and **17.00% / 280.17 µs** for the second. These are successive
whole-operation comparisons; their percentages or historical timings are not
added into a cumulative claim. The final pair measures 1,648.48 → 1,368.31 µs,
ratio interval 0.824–0.837. Each confirmation follows a short screen and retains
twelve alternating pairs of 64 complete operations per subject.

One writer-owned block emitter uses the existing packing kernel and local
reservoir/component Huffman arrays. Histogram-only helpers lose unused writer/
table branches and the unused Signed wrapper is removed. Private values are
Int16 AC or differences of two Int16 DC coefficients, bounded to ±65,535.
The sign-mask identity is exact throughout that domain; the general full-Int32
Category method and all code/amplitude validation remain unchanged. Failed writer
state is private and abandoned at the established encoder boundary. No frame
replay cache, extra histogram pass, session, emitter framework or new capacity.

Current costs include flattening, normalization, fixed tables, complete encoding,
restart framing, fresh MemoryStream growth, independent output and disposal.
Original settings/inputs, exact packets with the strict 2 B attribution exception,
quality sanity, source/owners and CPU0 idle-v2 gates pass. Every actual and both
twelve-sample native brackets remain. Source generation, JIT, IPC and validation
stay outside clocks. Paired incremental effects and standalone current costs are
separate views. No timing is repeated for a favorable bound.

The worker requests optimal Huffman, but its two **serial** slice contexts force
the T.81 example fixed tables in [FFmpeg n8.1 initialization](https://github.com/FFmpeg/FFmpeg/blob/n8.1/libavcodec/mjpegenc.c#L599).
Native codec/conversion/frame/scratch setup and table expansion precede timing;
managed fixed-table creation remains inside every public operation. Native still
converts/encodes each source and acquires/releases an independent complete packet
while timed. This clarifies effective behavior behind generic receipt text;
settings, packets and boundaries are unchanged. Source insight and sampled code
locations are not setup timings or justification for a cache/session.

No new heap capacity or retention. Writer buffer/admission remain 512/640 B,
table owner 16 KiB and disposed writer retention zero. Existing coefficient-pool
and caller-root bypass policies remain. GC allocation, admitted capacity and
retained state are distinct; physical/native/JIT peaks are unmeasured. Small GC
counter rounding differences are not capacity changes.

Windows/Linux each pass **6,648 full/live checks**, plus **472 internal/379 public**
per normal/scalar mode. Eight new cases cover 4,096 independently packed blocks,
all 131,071 possible DC differences, width/sign/tail/predictor/restart boundaries,
unused gray slots, admission refusal/cancellation, stream failure prefixes and
cleanup. Existing owned/borrowed sync/async and tight-recompute contracts pass.
Shared packing, buffer lifetimes and 17 adaptive paths remain source-identical;
their passing costs retain original artifact identities. No adaptive retuning.

The SAME final actual Release package and three usage examples pass both OSes,
including frozen color/gray restart and tight-recompute packets, terminal refusal
and root release. Two immutable core/internal builds per OS cover A+B. A was
already qualified when its profile exposed the sign-branch lead; its source,
package, full checks and measurements remain archived. B receives affected full/
package qualification after performance reconciliation. One paired helper build
is reused; prepared harnesses, references and fixtures need no builds. Neither
production artifact is rebuilt after timing.

Unchanged decoder/PNG/WebP/resize/drawing/text/inspection evidence is retained.
Published packages remain immutable; publication and full 289-path parity stay
open. Next, audit the existing opaque VP8L confidence evidence before proposing
a revised measurement; uncertainty alone does not establish an optimization gap.

[Evidence](jpeg-direct-block-emission-evidence.json) pins producers, identities,
current costs, integer/source proofs, all actuals and qualification. Raw
`jpeg-bounded-block-emission-qualified.tar.xz`: 8,569,520 B / 256 members,
SHA256 `1f818a00b99bcabf97b14fe440165b18c1f485fa9c9d396f8a8ce29b617b3573`, mirrored and member-verified.
Dependencies retain the first step, attribution and original tools/fixtures.
