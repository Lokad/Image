# Warm work admission

Explicitly inlining OperationContext.Work lowers complete PNG16 decode screens
to 2.22x / 2.22x, PNG8 to 2.11x and the affected native-input JPEG decode to
2.06x FFmpeg. PNG16 repeats are stable and all six actual samples fall below
the latest retained baseline samples. Every measured case remains above 2x;
uniform parity is open.

The production change adds one inlining hint and keeps the complete original
method body. Cancellation, exact atomic work admission, charge grouping, memory
and output limits, image algorithms, precision, hidden RGB, quality, compression
effort and independent ownership retain their existing behavior.

The [numeric evidence](warm-work-admission-evidence.json) retains original
samples, confidence bounds, allocations, source/input/output identities, job
receipts and raw archive hashes. Regenerate with
`python benchmarks/render-warm-work-admission.py`.

| Stage | Managed mean, µs | Native before / after, µs | Worst bracket ratio | Conservative upper |
| --- | ---: | ---: | ---: | ---: |
| Prior PNG Decode16 | 7592.82 | 3228.33 / 3223.98 | 2.36x | 2.57x |
| Prior PNG Decode8 | 3378.80 | 1553.06 / 1430.71 | 2.36x | 4.24x |
| Prior PNG Decode16 | 6827.11 | 2927.41 / 2929.16 | 2.33x | 2.84x |
| Inlined PNG Decode16 | 6482.70 | 2919.78 / 2935.67 | 2.22x | 2.44x |
| Inlined PNG Decode8 | 2994.19 | 1418.29 / 1422.28 | 2.11x | 2.32x |
| Inlined PNG Decode16 | 6469.95 | 2919.63 / 2916.54 | 2.22x | 2.39x |
| Prior native JPEG | 3657.04 | 1718.52 / 1719.48 | 2.13x | 2.25x |
| Prior native JPEG | 3694.14 | 1717.73 / 1726.43 | 2.15x | 2.54x |
| Affected native JPEG | 3539.85 | 1755.60 / 1722.52 | 2.06x | 2.74x |

Runs use the same existing 513x259 inputs: two original rasters at 8/16 bits
and the frozen native JPEG of the existing 8-bit raster. No new image or quality
enumeration. PNG output samples are exact, including zero-alpha hidden RGB.
The identical JPEG input retains its prior output identities and decoder error
bounds. PNG16 and JPEG allocation match the latest relevant baselines exactly;
PNG8 measures 755,294 versus 755,300 B/op. The hint introduces no allocation.

Complete public decode and independent output acquisition/release are timed.
Necessary native conversion and reset stay inside the boundary; native contexts
and scratch are reused. Startup, IPC, I/O, generation, hashes and validation are
excluded. Sequential Release runs use CPU 0, one codec/conversion thread, normal
SIMD, .NET 10.0.8 / SDK 10.0.204, disabled tiering and non-concurrent workstation
GC on the dedicated AMD EPYC 9V45 VM.

One narrow JIT inspection of the same PNG16 input confirms that the optimized
Validate listing has no separate Work calls, versus the earlier retained listing.
Code size rises from 7,441 to 9,380 bytes. Both quiet checks pass. This supplies
no new CPU-cost attribution or parity timing. Three canonical Decode calls still
receive the reader by reference; test that escape next, one primitive at a time.
The earlier framing CPU profile describes the code before Work inlining.

All 2,028 tests pass on Windows and Linux: 448 internal, 686 public and 894 live
FFmpeg comparisons. The same actual dependency-free Release package passes all
three usage examples on both platforms. All production method bodies and method
implementation flags match the statistical library; Work inlining is present in
both. Matching IL alone would miss this change. Package SHA256:
`3ad41ce5d341a39cde352ffd93e7a2555794c18c2794aad6866c60be9b1a40c2`.

Statistical pre-run quietness passes; post-run load checks fail and remain in
the raw evidence with all original samples. The stable repeated reduction
supports retaining the change, without certifying <=2x. PNG8 and native JPEG
have one affected screen each. Other consumers need rolling rechecks; no blanket
speedup is inferred. Unknown, unmatched and other >2x directions stay open.
