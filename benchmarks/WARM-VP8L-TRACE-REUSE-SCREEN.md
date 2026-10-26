# Rejected VP8L trace-token reuse

One lifetime change recycles a losing repeated parse's token owner for tracing.
The two existing513×259RGBA8 controls retain complete normalization, encoding,
fresh output acquisition/release and oneCPU. All packets/settings remain frozen.

| Quiet candidate | Lokad µs | Reference µs | Ratio / upper | Allocated B/op |
| --- | ---: | ---: | ---: | ---: |
| vp8, short | 93,549.36 | 53,413.07 | 1.751 / 1.904 | 21,234,848 |
| vp8, confirm | 93,311.56 | 53,412.06 | 1.747 / 1.763 | 21,234,848 |
| vp8l, short | 194,906.48 | 121,565.98 | 1.603 / 1.925 | 46,594,704 |
| vp8l, confirm | 195,462.18 | 121,469.56 | 1.609 / 1.639 | 46,594,704 |

The allocation savings are real:2,123,552B/op(VP8,9.1%) and1,080,224B/op(VP8L,2.3%).
Complete98,590/183,812-byte packets remain exact. Both guards and all12actuals pass.
The independent means are0.5%/1.7% higher than retained prior confirmations;
unpaired observations do not establish a causal regression. No CPU benefit is
demonstrated. The candidate is rejected while CPU parity remains open; there is
no representation/inlining/validation variant or further confirmation.

Context/capacity/nonalias, cached/uncached tokens, uninterrupted capacity charge,
refusal/cancellation cleanup and independent recovery passed26focused checks perOS.
Selected production source and Windows/Linux binaries are restored byte-for-byte.
The reuse API and its dedicated tests are removed. A useful two-case theory against
the actual selected Trace checks scratch refusal/cancellation, borrowed input
immutability, later independent output reconstruction and explicit cleanup. It
passes normal/scalar bothOS;22restoredLZ/main/ownership checks pass perOS.

Selected4,922/full/601scalar checks and SAMEactualpackage qualification are reused;
no full suite/package repeat follows rejection. The accepted VP8decoder gain and
all current summary rows remain unchanged. [Evidence](warm-vp8l-trace-reuse-screen-evidence.json)
retains candidate/source/binaries, raw results, restoration and mirrored archive.
Next count long-copy endpoint updates in TraceState.Push on the same inputs before
a CPU batching candidate. Full253-path warm parity/readiness remain open.
