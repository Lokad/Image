# Current progressive JPEG costs

Three existing controls have independent quiet-VM costs on qualified production.
All remain CPU gaps. Managed frames equal the frozen native pixels exactly;
normal/scalar correctness and independent output ownership pass.

| Complete warm one-CPU decode | Lokad µs | Ratio / upper | Allocated B/op |
| --- | ---: | ---: | ---: |
| ProgressiveGray | 2,475.155 | 1.442 / 1.476 | 683,126 |
| ProgressiveCancelable444 | 4,833.602 | 1.438 / 1.447 | 984,212 |
| Progressive444 | 4,953.638 | 1.472 / 1.501 | 984,212 |

All use original 513×259 progressive packets and coefficient proofs. Complete
resident-input decoding, conversion, fresh independently owned RGBA8 output and
release are timed on both subjects. FFmpeg's software MJPEG decoder resets state
per operation; codec/conversion contexts and packet/frame wrappers are reused.
Grayscale includes GRAY8 expansion. The live token is never signaled; setup is
excluded and native has no equivalent option. Startup, I/O, IPC and validation
are excluded. Native allocation is unmeasured.

Each has one retained independent twelve and both native brackets; conservative
lower bounds exceed 1×. Temporary managed-hash metadata is refreshed to the
already frozen native frame, then original headers restored byte-exactly. Packets,
quantization, coefficients, reference settings and pixel tolerances stay fixed.
No causal gain or regression claim follows from comparing separate token cases.

The grayscale/live measurements reuse the unchanged A128 producer and 5,267-check
per-OS/package proof, with zero builds. Ordinary 4:4:4 uses the qualified LRP1
producer (JPEG paths unchanged), 5,285 checks per OS, live comparisons, the same
actual Release package on both OSes and three examples. It needs one small cached-
source selector build changing only the case; no library/harness/native rebuild.
An archive containing only the grayscale selector is refused before any actuals.
No new fixture, profile, quality sweep or successful timing repeat.

[Grayscale/live numeric evidence](current-progressive-jpeg-costs-evidence.json)
pins raw current-progressive-jpeg-costs.tar.xz, SHA256
3e47edc85c95f8fb34f8b7af94b7eb5a9e411f220076086cc79cc83aba062b4b
(76 members). [Ordinary 4:4:4 evidence](current-progressive-jpeg-444-cost-evidence.json)
pins raw current-progressive-jpeg-444-cost.tar.xz, SHA256
9547e5a7553ae6f186161501a5f702da0352f5f8f3fadbc17b885b63651648f3
(40 members). Both are mirrored and member-verified, retaining actual libraries,
selectors, workers, packets, frames, receipts and guards. The full 268-path
objective remains open. Reproduce with the [reference tooling](../tools/WarmReference/README.md).
