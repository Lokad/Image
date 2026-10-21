# JPEG444 precision-grid decisions

C165 makes the final coefficient decisions on the intended source-derived
precision grid. It preserves the original fine-nearest error allowance instead
of granting extra modeled distortion when the grid becomes coarser. This is a
compression improvement; complete-packet parity remains open.

| Existing 513×259 procedural control | Previous B | Current / reference packet B | Source-PSNR gap |
| --- | ---: | ---: | ---: |
| Original4:4:4, RGBA8 | 138,074 | 135,630 / 133,041 | 0.218811 dB |
| Original4:4:4, RGBA16 | 138,002 | 135,576 / 133,043 | 0.220739 dB |

The savings are **2,444/2,426 B**. Both pass the unchanged0.25 dB quality gate,
exact alpha and decoder bounds (maximum4, RGB RMSE1.5). Source, sampling,
managed quality69/native qscale5, Original DQT and0x204060 flattening stay frozen.
Equal quality numbers do not establish equivalence. These procedural controls
do not establish photographic, diagram or general JPEG compression claims.
Optional CompactColor remains separate; passing controls stay stopped.

The existing quality-derived power-of-two factors select effective quantizers
before one local/DAG/component-pair pass. Its MCU error limit is
`max(coarseNearestError, fineNearestError + fineNearestError/32)`. The coarse
nearest point is a feasible floor. Selected levels are rescaled into the literal
Original DQT; no later precision pass overwrites them. The rounded BT.601 source,
direct DCT, source-only Q8 trainer and lambda remain unchanged. No reference
quality enters selection, and no quality, cap, weight or settings grid is fitted.

Two128-entry ushort arrays add512 stack bytes inside the same8KiB workspace.
The former precision pass's2,880 work units cover preparation/rescaling before
mutation; the total decision bound stays866,322/MCU and the default2B work limit
is unchanged. There is no second transform, full refinement, frame/input copy or
heap matrix. Shared admission, terminal refusal, cancellation, asynchronous
users and independent output lifetimes retain their contracts.

Both OS pass **5,006 Release tests** (2,087 internal,1,983 public,936 live FFmpeg),
plus176 internal and62 public scalar checks each. The actual8/16 packets match
the independent prototypes across RGBA, caller samples, fixed/adaptive storage,
baseline/progressive syntax, cancellation and async output. Every411,840 chosen
and original rounded-source-nearest coefficients per depth is independently
reconstructed. Scalar/normal/native pixels equal their equivalent baseline
packets. The same actual private0.2.0 package and all three usage examples pass
on Windows and Linux.

One core and one test build/project/OS remain frozen. The C161 portable benchmark,
differential/native/driver binaries and pinned log parser are reused. Private
cache/parser helper corrections require no rebuild or repeated timing. Rejected
companions C162 (failed quality) and C163 (larger packet) remain retained.

One quiet-VM three-actual complete forward444 screen records
**1,312,293.197 µs / reference3,781.709 µs**, ratio347.011/upper361.945,
**613,632 B/op**. Both versioned idle guards pass; native allocations are
unmeasured. Prior C159 was1,300,808.740 µs and613,208 B/op. The higher cost is
accepted for the byte savings. This is not a CPU-parity pass or an independent
confirmation; speed-only work remains deferred.

Five444 packet rows and their forward cost replace C159. Other invocations
containing changed Original444 encoding retain historical timing until measured.
Relevant420/422/grayscale/CompactColor, decoder, PNG and WebP paths retain valid
evidence. The finite256-case warm objective and replacement readiness stay open.
The [packet ledger](JPEG-COMPLETE-PACKET-AUDIT.md) has13 default passes/5 open and
one separate optional pass; its stale root counters are corrected from its
already-correct individual rows. The largest packet gap is now about1.95%.

[Numeric evidence](jpeg444-precision-grid-evidence.json) identifies the frozen
builds, actual package, original receipt and mirrored raw archives. The
[benchmark summary](../docs/benchmark.md) remains limited to three tables.
