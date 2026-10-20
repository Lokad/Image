# Exact SIMD VP8 forward DCT

Safe four-lane binary64 DCT qualifies the seven existing complete VP8 encodes
on current production. Their point means are **5.4–16.9% below earlier reported
checkpoints**; those earlier producers remain historical, rather than a new
controlled comparison isolating the DCT change. The text/diagram control now independently clears warm parity at
**0.831× / upper0.834×**. The opaque original controls remain gaps. All coefficients,
packets, decoded pixels, source-quality metrics, alpha and work charges are exact.
Compression sizes and their required photograph/diagram gates remain unchanged.

| Complete effort4 encode | Managed / reference µs | Ratio / conservative upper | Managed allocated B/op | Managed / reference bytes | Verification |
| --- | ---: | ---: | ---: | ---: | --- |
| Original opaque RGBA8 | 19,177.49 / 15,528.15 | 1.235 / 1.354 | 1,482,108 | 151,606 / 72,380 | Short gap |
| Original opaque RGBA16 | 19,531.60 / 15,602.44 | 1.252 / 1.284 | 1,481,944 | 151,606 / 72,380 | Short gap |
| Original alpha RGBA8 | 19,209.65 / 53,462.22 | 0.359 / 0.362 | 2,109,306 | 286,532 / 98,590 | Independent parity |
| Original alpha RGBA16 | 20,042.86 / 53,642.99 | 0.374 / 0.376 | 2,108,980 | 286,532 / 98,590 | Independent parity |
| Original alpha RGBA8, work budget32M | 21,546.32 / 53,834.21 | 0.400 / 0.403 | 2,616,214 | 471,436 / 98,590 | Independent parity |
| Photograph RGBA8 | 6,555.59 / 9,649.33 | 0.679 / 0.684 | 558,587 | 21,432 / 17,688 | Independent parity |
| Text/diagram RGBA8 | 4,856.65 / 5,841.64 | 0.831 / 0.834 | 489,948 | 8,288 / 7,100 | Independent parity |

Each vector lane follows the original scalar multiplication/addition order.
Products and additions remain separate; no FMA or reassociation crosses a
quantization midpoint. Division and midpoint-away rounding are unchanged. Other
hardware keeps the original scalar calculation. Bounded stack spans add no
per-operation heap scratch. A deterministic mathematical oracle checks 8,192
residual/format-quantizer combinations, signed extrema, impulses and ties, plus
output boundaries; existing transform-range/cache tests remain applicable.

Only these seven timed encoder paths change. Frozen inputs, managed/native
qualities100/75 for original controls and78/75 or75/75 for photograph/diagram,
effort4, native packets/workers and all quality metrics are reused. Fresh checks
compare every packet and source/alpha/decoder metric before/after scoring. Equal
quality numbers are not assumed equivalent. Required content sizes remain
**≤17,688/7,100 bytes** alongside the existing ≤0.25dB source-quality gate. Those
two size gates fail. Lossless encoding, all decoding, inspection, other codecs,
editing and text retain their previously identified evidence and limitations.

Dedicated quiet EPYC9V45 VM, Ubuntu24.04.4, .NET10.0.8/SDK10.0.204,
FFmpeg n8.1, one pinned CPU and one native codec/conversion thread. Normal SIMD,
tiering off, nonconcurrent workstation GC. Seven three-actual screens and five
independent twelve-actual confirmations retain all81 actuals and both idle-v2
guards per job. No long repeat follows either clear opaque gap. The upper is the
larger selected/all-actual confidence bound against the faster native bracket.
Complete operations include normalization/repacking, public admission, encoding,
fresh output growth, independent ownership and disposal. Input preparation,
validation, I/O, IPC and startup are excluded on both sides. Native allocation
totals are unmeasured.

All **4,535 tests/OS** (1,683 internal,1,920 public,932 liveFFmpeg), **169 affected
hardware-off checks/OS** (21 internal,148 public), and the **same actual private
0.2.0 package** consumer/all three examples pass on Windows/Linux. One immutable
core and one internal-test build per OS; no harness/native/selector rebuild,
quality recalibration or repeated production build after freezing. Existing
cooperative admission, cancellation, caller inputs and retained owners are preserved.
Published0.1.0 is unchanged; replacement readiness and full parity remain open.

Linux producer `733a4bc6ac96a4c5347ef4501fcad96fb4bbe17594191a8121d36bc0d85d83b8`;
Windows `9b8c6d063c6fbef96a82f989ed59cefd2a683fc074f4292ffab9bff7d209540d`;
actual package `e98a775242b4722b55b2bb5c432babc265004719ee109c1e62f8083590c12c84`.
[Numeric evidence](warm-vp8-forward-dct-evidence.json) includes every sample,
allocation, guard, complete boundary, packet identity and actual child hash.
Raw `c40-measurements.tar.gz` is retained on both hosts, SHA256
`5df04ae0fc80dae2b124c94b245d235ebf3f093ced3ab09bc73d5cf6cf1b6f1f`.
Qualification archives are also retained on both hosts:
`c40-qualified-windows.tar.gz`
`b07f796b6dbe5188c8b386d3839e3a95e72275b81d702c6c7967cfda8dec8040`;
`c40-qualified-linux.tar.gz`
`da918c9aead7c32eb044c5be400f48f503c6deccd70681a62522510cd34400ca`.
