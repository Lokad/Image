# Forward-only RGBA16 Fastest output

Historical <=2x checkpoint. Its PNG encode timings are superseded by the current
[chunk checksum round](WARM-PNG-CHUNK-CRC.md), which refreshes all16 affected
selectors and distinguishes independent parity from qualified short gaps.

Complete forward-only RGBA16 Fastest encoding independently confirms **0.771x
FFmpeg / conservative upper 0.805x**, with **2,103,770 managed B/op**. The complete
packet is byte-for-byte identical to the frozen stock-output packet; full words,
alpha and hidden RGB remain exact. Production is unchanged.

| Quiet observation | Lokad.Image µs | FFmpeg µs | Ratio / upper | Managed B/op |
| --- | ---: | ---: | ---: | ---: |
| Three-actual screen | 12,781.60 | 17,672.13 | 0.723 / 0.775 | 2,103,770 |
| Independent twelve-actual confirmation | 13,628.70 | 17,677.29 | 0.771 / 0.805 | 2,103,770 |

Each managed operation creates an independent MemoryStream and a fresh
forward-only caller wrapper, encodes through its span callbacks, and disposes
both. Length/Position/Seek are unavailable; the underlying caller remains open
after encoding. Wrapper creation, callback dispatch, complete row packing,
compression, CRCs, output growth/acquisition and release are timed. The native
operation directly acquires independent encoded packet storage and needs no
seeking or equivalent wrapper; complete conversion/encoding/release are timed.
Codec/packet wrappers and endian workspace are reused, with conversion repeated
every operation. No output is cached. Startup, I/O, IPC, generation and validation
are excluded. Native allocations are **unmeasured**.

The original 513x259 RGBA16 raster, frozen stock packet and native worker are
reused. Managed BCL level2/native level1 are explicit; IDAT remains962,820 versus
965,318 bytes within the unchanged5% gate. Full samples, normal/scalar native CLI,
source preservation, caller lifetime and two independent outputs pass outside
timing. No image, setting, strategy or geometry enumeration is introduced.

Sequential Release runs use one pinned CPU, one codec/conversion thread, normal
SIMD, disabled tiering and workstation nonconcurrent GC. Both quiet guards pass
on both runs. All twelve confirmation actuals remain selected and retained. The
upper includes selected and all-actual intervals against the faster native
bracket's lower bound. Both runs use the same immutable production DLL/harness.

Only one benchmark input changes. Existing **3,373 Release tests per OS**,
including **898 live comparisons**, and the same actual package's consumer plus
all three examples are reused from the [streaming round](WARM-PNG-FAST2.md).
No production rebuild, full suite, package proof or method audit is repeated.
The old receipt phrase “Both level1” was a descriptive error: its explicit
managed/native numeric fields and validation already enforce2/1. This harness
corrects the text; no measurement or comparison gate changes retroactively.

[Numeric evidence](warm-pngfastforward16-evidence.json) retains all samples,
allocations, guards, actual source/assembly/job identities and qualification
reuse. Stop this confirmed pair; remaining material workloads and unmatched
reference cases stay open.
