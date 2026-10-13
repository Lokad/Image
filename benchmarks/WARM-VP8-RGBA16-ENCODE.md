# Warm VP8 WebP encoding from RGBA16

Encoding the original RGBA16 raster to VP8 with alpha independently confirms
**0.809× FFmpeg**, conservative upper **0.819×**, averaging 43.362 ms with
**2,615,472 managed bytes allocated per operation**. All 12 actual results are
retained and both quietness guards pass. Production and package are unchanged.
Stop tuning this fixed case; uniform required warm parity remains open.

| Run | Lokad µs | Ratio | Conservative upper | Quiet guards |
| --- | ---: | ---: | ---: | --- |
| Three-actual screen | 43,257.11 | 0.803 | 0.887 | Pass |
| Independent twelve-actual confirmation | 43,362.07 | 0.809 | 0.819 | Pass |

Both subjects encode the existing original 513×259 RGBA16 raster. Every channel,
including alpha, is normalized with integer `(sample+128)/257` inside each timed
operation. The original native adapter then repacks RGBA64LE to BGRA8 and uses
FFmpeg's public libwebp encoder. Its normalized samples match the exact reference
before Ready and after measurement; a corrupted alpha reference fails explicitly.
That reference is a validation target and is never used as timed source pixels.
The prior native 8-bit output remains byte-identical; no original image is added.

Native quality 75 and managed quality 100, effort 4 on both subjects, match at
**0.003646 dB** within the unchanged 0.25 dB alpha-weighted RGB source-error band.
Only one owned setting is evaluated. Normalized output alpha is exact; geometry,
sampling, decoder agreement, source integrity, deterministic bytes and independent
held output owners pass before and after measurement. The original 15 invalid
metadata controls and three additional depth-16 controls reject before a worker.

Lokad.Image emits **471,436 bytes**, versus FFmpeg's **98,590 bytes**. Managed
fixed entropy and native adaptive coding differ; these results establish the
declared source-quality timing comparison, not compression efficiency parity.

Managed timing includes fresh MemoryStream growth, normalization, full encoding
and disposal. Native contexts, scratch frames and wrappers are reused while
normalization, repacking, encoding and owned packet acquisition/release repeat
each operation. Startup, I/O, IPC, preparation and checks are excluded. Native
allocations are unmeasured. Sequential Release runs use one pinned CPU, one codec
thread, normal SIMD, tiering off and workstation nonconcurrent GC. The screen
mean was parsed before the single independent confirmation.

Only the development harness and original native adapter change. Forced fresh
builds and both actual generated children match all 599 production method bodies
and flags of the verified actual Release package. Prior 2,715 Release/differential
tests per OS, 60 focused row checks in normal/scalar modes per OS and all three
same-package examples on Windows and Linux are reused for unchanged production.

The [numeric evidence](warm-vp8encode16-evidence.json) retains every actual sample,
allocations, controls and exact source/child/native identities. All four actual
child files, source, receipts and actual package were frozen before subsequent
preparation or mutation. The [benchmark summary](../docs/benchmark.md) reports
this fixed pair at its measured source; JPEG420 gaps and required material
coverage remain open.
