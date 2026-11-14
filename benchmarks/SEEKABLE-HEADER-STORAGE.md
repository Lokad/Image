# Bounded seekable header storage

Complete PNG header inspection confirms **0.306 / adapter 0.572 µs**, ratio
**0.535 / upper 0.548**: pass and stop.
Its contemporary original-build baseline was **0.680 / 0.596 µs** and 8,528 B/op.
The complete operation now allocates **592 B/op**. These sub-microsecond savings
do not establish whole-library parity.

The unchanged original 218,775-byte 513×259 RGBA8 PNG enters a fresh, non-exposed
seekable stream and returns a fresh independent nine-field typed descriptor.
Source creation/release, header/EXIF/framing scan, opaque-IDAT seeks, EOF and all
workspace/result ownership are timed. The explicitly labeled reference uses fresh
**FFmpeg AVIO plus a bounded header/EXIF adapter**, 8,192-byte scratch and owned
AVCodecParameters; it is not the public PNG parser. CRC and compressed pixels are
excluded equally. No input view, whole-packet copy, raster or cached descriptor.

Seekable inspection starts with 256-byte prefetch plus the existing 256-byte
reader/header allowance: **512 B admitted peak**, down from 8,448 B. JPEG bulk
scan requests 8,192-byte scratch at a drained refill, after pending users finish;
discard/release the consumed small array before reserve/allocation. Separate
header assembly preserves partial reads. Refusal remains terminal; no retry,
fallback, pool or session. Small JPEG EOI clears the outstanding bulk request.
Nonseekable/default probe capacities, grammar and public contracts are unchanged.

The final JPEG-only EOI refinement changes no PNG executed path or timed method.
Reuse its original independent confirmation, with all 12 actuals, adverse samples,
native brackets and idle-v2 guards. The final DLL reruns the same 21 frozen
structural/EXIF/CRC controls, normal/scalar/native. The timed and final producer
identities remain distinct in [evidence](seekable-header-storage-evidence.json).
This source-valid reuse is not a new whole-release package timing refresh.

The final immutable DLL also reconciles three bounded complete-operation
companions against the original qualified managed DLL. Fresh source, full public
call, independent descriptor and all source/workspace release are included.
These paired measurements are **incremental managed evidence, not native parity**.
Async PNG uses immediately completed resident reads; held-user tests cover real
suspension/cancellation. No new image/settings grid or timing retry.

| Managed-only companion | Original → final µs | Allocated B/op | Paired result |
| --- | ---: | ---: | --- |
| JpegBulkSync | 4.472 → 4.448 | 8,528 → 8,808 | inconclusive |
| WebpSeekableSync | 0.408 → 0.140 | 8,528 → 592 | lower elapsed |
| PngHeadersAsync | 0.966 → 0.359 | 8,528 → 592 | lower elapsed |

JPEG bulk elapsed is inconclusive, with paired change −0.024 µs and confidence
interval −0.242..+0.194 µs. Its extra **280 B/op** is the small initial array;
the old/new owners do not overlap and admitted peak remains **8,448 B**.
No JPEG speedup is claimed. WebP and async PNG paired intervals are below zero.
Post-operation retained storage is **zero**; total GC allocation, logical admitted
capacity and unmeasured physical/native/JIT peaks remain distinct.

Both platforms pass **6,640 full/live checks** (3,248 internal/2,444 public/948 live).
Focused normal/scalar checks pass 1 internal/173 public cases per mode/platform.
Sixteen new public cases cover short reads, small JPEG EOF, aggregate roots,
replacement peak, pre-read refusal, terminal bulk refusal and subsequent recovery,
caller failures and cancellation while actual async users still hold small/bulk
storage. Existing input/metadata/precision/framing limits remain.
The **same actual final Release package** and all three examples pass Windows/Linux;
its new consumer verifies small/bulk admission, refusal/recovery and async lifetime.
The earlier immutable package and published releases are preserved.

One coherent broad/package milestone follows affected-cost reconciliation.
Unchanged encoder, decoder, editing, resize and text evidence is reused; existing
arithmetic/ISA qualification remains valid. PLAN.md changes stay confined to the
authorized font/text section. Publication is postponed; the 289-path goal is open.
Raw archive identity and reproducibility are recorded in the linked evidence.


Precommit review also reconciles the four existing dimension probes on the final
DLL, using cached harness/driver and 12 original untimed controls: PNG 0.160/adapter
0.122 µs, WebP 0.156/0.123 µs, JPEG 0.324/0.193 µs and async JPEG 0.401/0.201 µs.
All remain small absolute gaps, at 744 B/op. Each keeps all 12 actuals and exact
original input/descriptor/prefix/ownership/reference gates. No historical-clock
speedup or whole-library pass is claimed, and no fixture/settings/build is added.

This corrects an ordering miss: shared probes were omitted from the first
header-cost reconciliation before broad/package checks. Those checks remain valid
for the same unchanged final artifact and were not repeated; all affected costs
are now reconciled before acceptance. Earlier raw flags are retained with this
explicit scope correction. The probe archive supplements the original raw bundle.
