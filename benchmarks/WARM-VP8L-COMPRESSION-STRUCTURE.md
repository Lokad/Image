# Lossless WebP compression structure audit

The current accepted build reproduces both frozen size gaps. Independent RFC9649
prototypes demonstrate substantial improvements, but neither size gate is met.
These are untimed diagnostic packets, not production results or performance passes.

| Fixed opaque RGBA8 content, 513×259 | Current Lokad B | Best diagnostic B | Frozen libwebp reference / size ceiling B |
| --- | ---: | ---: | ---: |
| Photograph | 383,154 | 196,528 | 160,032 |
| Text/diagram | 15,120 | 7,636 | 7,234 |

The photograph result uses adaptive prefix codes, a global spatial predictor and
green decorrelation. That strategy harms the diagram. The best diagram uses green
decorrelation, a1,024-entry color cache, existing run/previous-row references and
compact code-length metadata. Naive general match search worsens its size;
one bit-cost parse decision leaves it unchanged. Those variants are stopped.

The original hash-verified reference packets are reused from the existing raw
archive. Their format structure identifies distinct work: the photograph uses
spatial prediction, color transforms and nine entropy groups; the diagram uses
green decorrelation and a color cache. Neither uses a palette. This is format
inspection and algorithmic insight, with no imported implementation or table.

All nine prototype packets plus the original reference per control preserve every
source sample through normal/scalar FFmpeg and current managed decoding on both
OSes. Managed decode/reencode reproduces each original C26 packet exactly, normally
and with intrinsics off. Source hashes, opaque projection, native method4/quality75
and exact=1 remain fixed. No new source, effort/quality grid or timed workload.

One original prepared probe and the immutable accepted cores are reused: zero
production/harness/selector/native/probe builds, scored jobs or full-suite/package
repeats. Existing4,484tests/OS and same actual private0.2.0 package evidence remain
applicable. Allocations and speed of these prototypes are unmeasured. Production,
selected timings, coverage counts and published0.1.0 are unchanged.

Next: bounded adaptive prefix coding, then one managed color-cache encoder path
using the demonstrated strategy. New storage must be admitted before allocation;
refusal stays terminal, work/cancellation and output preflight remain explicit.
Untimed size/exactness checks precede existing quiet-VM short/independent tests.
Further photograph work is required. Neither these two controls nor their best
diagnostic results establish general-image compression parity.

[Evidence](warm-vp8l-compression-structure-evidence.json) records all experiments,
producer identities and the mirrored3,710,270-byte raw archive
`c33-lossless-compression-diagnostic.tar.gz` (SHA256
`67ccdb846a34f1d8d862795b51ea010092e22c015158e226cf39144740800e01`).
