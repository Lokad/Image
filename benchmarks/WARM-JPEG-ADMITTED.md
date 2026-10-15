# Adaptive JPEG entropy byte admission

JPEG420 with row restart markers now independently confirms **1.958x FFmpeg /
conservative upper 1.973x**, at **297,780 managed allocated B/op**. One entropy
byte primitive reduces the retained managed mean **8.0%**, closing a **2.128x**
gap while preserving the complete packet. Uniform current-source parity remains
open.

| Measurement | Lokad.Image us | FFmpeg us | Ratio / upper | Allocated B/op |
| --- | ---: | ---: | ---: | ---: |
| Original three-actual screen | 3,430.56 | 1,612.38 | 2.128 / 2.308 | 297,772 |
| Candidate three-actual screen | 3,158.08 | 1,608.83 | 1.963 / 2.218 | 297,780 |
| Independent twelve-actual confirmation | 3,157.50 | 1,612.58 | 1.958 / 1.973 | 297,780 |
| Nonrestart adaptive RGBA8 regression screen | 3,156.98 | 1,765.97 | 1.788 / 1.916 | 297,780 |
| Fixed forward-only RGBA16 regression screen | 3,440.43 | 1,836.61 | 1.873 / 2.651 | 263,820 |

The two regression screens check means before full qualification; their short
intervals are not new independent confirmations. Earlier certificates remain
linked in the summary. The writer's additional reference costs eight bytes per
operation on these paths.

The existing original 513x259 RGBA8 raster is encoded into a fresh MemoryStream,
with adaptive Huffman tables, quality69, background RGB0x204060 and restart
interval33 MCUs. FFmpeg uses optimal Huffman tables and qscale5. Both emit DRI33
and sixteen row restart markers, cycling RST0 through RST7. The original native
adapter sets two logical slices but checks the public default serial executors,
one codec/conversion thread and zero active threading. No upstream code or build
change is introduced. Packet parsing and corrupted-DRI/RST negative controls
run outside timing. Both decoders reproduce each encoder's existing nonrestart
pixels exactly.

The source-quality gap remains **0.106110 dB**, within **0.25 dB**. Registered
decoder agreement stays within maximum4/RMSE1.5, with exact opaque alpha.
Source preservation, deterministic packets and independently retained outputs
pass before and after timing. Managed output remains **74,086 bytes**, SHA-256
`68f646f5264a92cca439630e99330f458152e7d42efa10d5aebd4b50d28097d3`;
native output is **58,012 bytes**. Compression efficiency parity is not claimed.

Adaptive encoding already admits a conservative whole-packet bound. When it
fits the quota and the output is a noncancellable stock MemoryStream, entropy
bytes bypass repeated bounded-stream checks. Individual byte writes, stuffing,
restart framing, work charges, counters and partial failures are preserved.
Caller subclasses, cancellable tokens, tight output quotas and fixed coding
retain their original writes. Five new test cases compare 9,000 original/admitted
executions per mode and OS, including work exhaustion, exact quotas, read-only
and nonexpandable streams, cancellation and caller callbacks.

All versioned quiet guards pass. The confirmation retains all twelve actuals;
its upper is the larger filtered/all-actual interval against the faster native
bracket's lower bound. Sequential Release measurements use one pinned CPU,
normal SIMD, disabled tiering and workstation nonconcurrent GC. Full flattening,
conversion, transform, histogram/table construction, entropy coding and independent
output acquisition/release are timed. Preparation, validation, I/O, IPC and
startup are excluded; no encoded result is cached. Native allocations are
unmeasured; managed figures include harness overhead.

The frozen core/harness is reused throughout the bounded experiment. Focused
checks pass normally and with intrinsics disabled on both OSes. Accepted
production passes **3,298 Release tests per OS**, including **896 live FFmpeg
comparisons**, plus the consumer and all three usage examples through the same
actual dependency-free Release package on Windows and Linux. No whole-library
method audit is repeated.

The [numeric evidence](warm-jpeg-admitted-evidence.json) retains all samples,
allocations, guards, packet identities, actual jobs/builds and package proof.
Stop this confirmed pair; remaining material public/codec coverage and final
current-source qualification stay open.
